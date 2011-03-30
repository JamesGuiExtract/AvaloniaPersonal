// InputManager.cpp : Implementation of CInputManager

#include "stdafx.h"
#include "IFCore.h"
#include "InputManager.h"
#include "ConfigMgrIF.h"
#include "IFCoreCP.h"
#include "IFCategories.h"

#include <LicenseMgmt.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <CommentedTextFileReader.h>
#include <ValueRestorer.h>
#include <COMUtils.h>
#include <ComponentLicenseIDs.h>

#include <algorithm>
#include <io.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

using namespace std;
extern CComModule _Module;

//static const string g_strAbbyyOCREngineProgId("AbbyyOCR.AbbyyOCREngine.1");
static const string g_strScansoftOCREngineProgId("SSOCR.ScansoftOCR.1");

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// InputRequestInfo
//-------------------------------------------------------------------------------------------------
InputRequestInfo::InputRequestInfo()
{
	clear();
}
//-------------------------------------------------------------------------------------------------
InputRequestInfo::~InputRequestInfo()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		clear();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16466");
}
//-------------------------------------------------------------------------------------------------
void InputRequestInfo::clear()
{
	m_strPrompt = "";
}
//-------------------------------------------------------------------------------------------------
void InputRequestInfo::set(UCLID_INPUTFUNNELLib::IInputValidatorPtr ipInputValidator, const string& strPrompt)
{
	m_ipInputValidator = ipInputValidator;
	m_strPrompt = strPrompt;
}

//-------------------------------------------------------------------------------------------------
// CInputManager
//-------------------------------------------------------------------------------------------------
CInputManager::CInputManager()
: m_bInputEnabled(false),
  m_bWindowShown(false),
  m_nCurrentIVID(1),		// ID starts with 1
  m_lParentWndHandle(0),
  m_ipIRDescriptionToProgIDMap(NULL),
  m_ipIVDescriptionToProgIDMap(NULL),
  m_ipOCREngine(NULL),
  m_ipCurrentInputContext(NULL),
  m_hModuleSSOCR(NULL),
  m_hModuleCOMUtils(NULL)
{
	try
	{
		// create an instance of the OCR filter manager
		m_ipOCRFilterMgr.CreateInstance(CLSID_OCRFilterMgr);
		ASSERT_RESOURCE_ALLOCATION("ELI03472", m_ipOCRFilterMgr != __nullptr);

		// get current ocr engine prog id
		m_strDefaultOCREngineProgID = m_ConfigMgr.getOCREngineProgID();
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI03470");
}
//-------------------------------------------------------------------------------------------------
CInputManager::~CInputManager()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		Destroy();

		// destroy the window manager object
		m_apWindowIRManager.reset(__nullptr);

		// force the OCR engine object to go out of scope here
		m_ipOCREngine = __nullptr;

		// Free SSOCR library if it's still loaded
		if (m_hModuleSSOCR)
		{
			::FreeLibrary(m_hModuleSSOCR);
		}

		if (m_hModuleCOMUtils)
		{
			::FreeLibrary(m_hModuleCOMUtils);
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI03471");
}

//-------------------------------------------------------------------------------------------------
// IInputManager
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputManager::get_WindowsShown(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bWindowShown ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02477")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputManager::get_InputIsEnabled(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_bInputEnabled ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02480")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputManager::get_ParentWndHandle(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		*pVal = m_lParentWndHandle;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03029")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputManager::put_ParentWndHandle(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_lParentWndHandle = newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03030")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputManager::Destroy()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Disconnect all input receivers and input validators
		m_mapIDToInputReceiver.clear();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02483")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputManager::EnableInput1(BSTR strInputValidatorName, BSTR strPrompt, 
										 IInputContext* pInputContext)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();
		
		// if the input validator description to progid map has not yet been
		// initialized, initialize it.
		if (m_ipIVDescriptionToProgIDMap == __nullptr)
		{
			// create an instance of the category manager
			ICategoryManagerPtr ipCatMgr(CLSID_CategoryManager);
			if (ipCatMgr == __nullptr)
			{
				throw UCLIDException("ELI04278", "Unable to create instance of Category Manager!");
			}

			// get the description to progid map for input validators from the
			// category manager
			m_ipIVDescriptionToProgIDMap = ipCatMgr->GetDescriptionToProgIDMap1(
				_bstr_t(INPUTFUNNEL_IV_CATEGORYNAME.c_str()));

			if (m_ipIVDescriptionToProgIDMap == __nullptr)
			{
				UCLIDException ue("ELI04279", "Unable to retrieve components registered in specified category!");
				ue.addDebugInfo("Category", INPUTFUNNEL_IV_CATEGORYNAME);
				string stdstrIV = asString( strInputValidatorName );
				ue.addDebugInfo("InputValidator", stdstrIV);
				throw ue;
			}

			// load UCLIDCOMUtils.dll once to prevent it's being unloaded 
			// before InputManager's destructor is called
			loadCOMUtilsDll();
		}

		_bstr_t _bstrInputValidatorName = strInputValidatorName;
		if (m_ipIVDescriptionToProgIDMap->Contains(_bstrInputValidatorName)
			== VARIANT_FALSE)
		{
			// no input validator registered with the specified description, throw exception
			UCLIDException uclidException("ELI02268", "No Input Validator registered with specified description.");
			uclidException.addDebugInfo("strInputValidatorName", string(CString(strInputValidatorName)));
			throw uclidException;
		}

		string stdstrIVProgID = asString(m_ipIVDescriptionToProgIDMap->GetValue(_bstrInputValidatorName));
		UCLID_INPUTFUNNELLib::IInputValidatorPtr ipInputValidator(stdstrIVProgID.c_str());

		if (ipInputValidator)
		{
			// call input receiver(s) to enable input
			getThisAsCOMPtr()->EnableInput2(ipInputValidator, get_bstr_t(strPrompt), 
				(UCLID_INPUTFUNNELLib::IInputContext*) pInputContext);
		}
		else
		{
			// the strInputReceiverName is invalid
			UCLIDException uclidException("ELI02269", "Unable to create InputValidator object!");
			uclidException.addDebugInfo("strInputValidatorName", string(CString(strInputValidatorName)));
			throw uclidException;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02486")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputManager::EnableInput2(IInputValidator* pInputValidator, BSTR strPrompt,
										 IInputContext* pInputContext)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		m_bInputEnabled = true;
		
		// compare the pInputContext with current input context
		// if not same, reset the input context
		if (pInputContext != __nullptr)
		{
			UCLID_INPUTFUNNELLib::IInputContextPtr ipInputContext(pInputContext);
			if (m_ipCurrentInputContext != ipInputContext)
			{
				m_ipCurrentInputContext = ipInputContext;
				m_ipCurrentInputContext->Activate(getThisAsCOMPtr());
			}
		}

		if (pInputValidator)
		{
			// wrap input validator in smart pointer
			UCLID_INPUTFUNNELLib::IInputValidatorPtr ipInputValidator(pInputValidator);

			_bstr_t bstrInputType = ipInputValidator->GetInputType();

			// store current input info
			string stdstrPrompt = asString( strPrompt );

			m_currentInputRequestInfo.set(ipInputValidator, stdstrPrompt);
			
			// call input receiver(s) to enable input
			map<unsigned long, UCLID_INPUTFUNNELLib::IInputReceiverPtr >::iterator iter;
			for (iter = m_mapIDToInputReceiver.begin(); iter != m_mapIDToInputReceiver.end(); iter++)
			{
				// Retrieve the Input Receiver
				UCLID_INPUTFUNNELLib::IInputReceiverPtr pIR = iter->second;
				ASSERT_RESOURCE_ALLOCATION("ELI15629", pIR != __nullptr);

				// Enable input
				pIR->EnableInput(bstrInputType, get_bstr_t(strPrompt));
			}

			string stdstrInputType = asString(bstrInputType);
			
			// Enable all the external window input receivers
			getWindowIRManager()->NotifyInputEnabled(stdstrInputType, stdstrPrompt);
		}
		else
		{
			// the strInputReceiverName is invalid
			UCLIDException uclidException("ELI02276", "NULL Input Validator interface pointer.");
			throw uclidException;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02489")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputManager::DisableInput()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		m_bInputEnabled = false;
		
		// Disable all COM input receivers
		map<unsigned long, UCLID_INPUTFUNNELLib::IInputReceiverPtr >::iterator iter;
		for (iter = m_mapIDToInputReceiver.begin(); iter != m_mapIDToInputReceiver.end(); iter++)
		{
			// Retrieve the Input Receiver
			UCLID_INPUTFUNNELLib::IInputReceiverPtr pIR = iter->second;
			ASSERT_RESOURCE_ALLOCATION("ELI15630", pIR != __nullptr);

			// Disable input
			pIR->DisableInput();
		}

		// Disable all the external window input receivers
		getWindowIRManager()->NotifyInputDisabled();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02492")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputManager::CreateNewInputReceiver(BSTR strInputReceiverName, long *pnIRHandle)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		// if the input receiver description to progid map has not yet been
		// initialized, initialize it.
		if (m_ipIRDescriptionToProgIDMap == __nullptr)
		{
			// create an instance of the category manager
			ICategoryManagerPtr ipCatMgr(CLSID_CategoryManager);
			if (ipCatMgr == __nullptr)
			{
				throw UCLIDException("ELI04280", "Unable to create instance of Category Manager!");
			}

			// get the description to progid map for input receivers from the
			// category manager
			m_ipIRDescriptionToProgIDMap = ipCatMgr->GetDescriptionToProgIDMap1(
				_bstr_t(INPUTFUNNEL_IR_CATEGORYNAME.c_str()));

			if (m_ipIRDescriptionToProgIDMap == __nullptr)
			{
				UCLIDException ue("ELI04281", "Unable to retrieve components registered in specified category!");
				ue.addDebugInfo("Category", INPUTFUNNEL_IR_CATEGORYNAME);
				string stdstrIR = asString( strInputReceiverName );
				ue.addDebugInfo("InputReceiver", stdstrIR);
				throw ue;
			}

			// load UCLIDCOMUtils.dll once to prevent it's being unloaded 
			// before InputManager's destructor is called
			loadCOMUtilsDll();
		}

		_bstr_t _bstrInputReceiverName = get_bstr_t( strInputReceiverName );
		if (m_ipIRDescriptionToProgIDMap->Contains(_bstrInputReceiverName)
			== VARIANT_FALSE)
		{
			// no input receiver registered with the specified description, throw exception
			UCLIDException uclidException("ELI02261", "No Input Receiver registered yet.");
			uclidException.addDebugInfo("strInputReceiverName", asString(_bstrInputReceiverName));
			throw uclidException;
		}

		string stdstrIRProgID = asString(m_ipIRDescriptionToProgIDMap->GetValue(get_bstr_t(strInputReceiverName))); 

		// found the registered input receiver component's prog ID
		// Create it
		UCLID_INPUTFUNNELLib::IInputReceiverPtr ipInputReceiver(stdstrIRProgID.c_str());
		ASSERT_RESOURCE_ALLOCATION("ELI16958", ipInputReceiver != __nullptr);

		// connect the input receiver to the input manager
		*pnIRHandle = getThisAsCOMPtr()->ConnectInputReceiver(ipInputReceiver);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02501")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputManager::ConnectInputReceiver(IInputReceiver* pInputReceiver, 
												 long *pnIRHandle)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		// dummy id
		unsigned long nID = 0;

		// wrap input receiver in smart pointer
		UCLID_INPUTFUNNELLib::IInputReceiverPtr ipInputReceiver(pInputReceiver);

		// Make sure pInputReceiver doesn't exist in the map yet
		if (!findInputReceiverID(nID, ipInputReceiver))
		{
			VARIANT_BOOL bHasWindow = ipInputReceiver->HasWindow;
			if (bHasWindow == VARIANT_TRUE)
			{
				// set parent window handle
				ipInputReceiver->ParentWndHandle = m_lParentWndHandle;
			}
			UCLID_INPUTFUNNELLib::IIREventHandlerPtr ipEventHandler(this);
			ASSERT_RESOURCE_ALLOCATION("ELI16984", ipEventHandler != __nullptr);

			// set event handler for the input receiver
			ipInputReceiver->SetEventHandler(ipEventHandler);
			
			if (bHasWindow == VARIANT_TRUE)
			{
				// always show window by default
				ipInputReceiver->ShowWindow(VARIANT_TRUE);
			}
			
			// add the input receiver to the map
			m_mapIDToInputReceiver[m_nCurrentIVID] = ipInputReceiver;
			
			*pnIRHandle = m_nCurrentIVID;
			// increment current id count
			m_nCurrentIVID++;
			
			// if the input receiver uses OCR technology, pass it a pointer to the IOCRFilter
			// object
			VARIANT_BOOL bUsesOCR = ipInputReceiver->UsesOCR;
			if (bUsesOCR == VARIANT_TRUE)
			{
				// if the OCR engine object has not yet been initialized, initialize it
				// NOTE: creating the OCR engine in the constructor does not make sense for 
				// two reasons:
				// 1. If the code below was cut and pasted into the constuctor, some products
				//	  that use the InputFunnel, like IcoMap, cause an assertion failure because
				//	  of the Abbyy engine "evaluation copy" messagebox that comes up.
				// 2. If a customer wants to use their own OCR engine, they will be able to create
				//	  an instance of that OCR engine, and associate it with the InputFunnel
				//    before the code below executes.  If an OCR engine has already been
				//	  associated with the InputFunnel when this code runs, the default OCR
				//	  engine is then not initialized.
				if (m_ipOCREngine == __nullptr)
				{
					if (FAILED(m_ipOCREngine.CreateInstance(m_strDefaultOCREngineProgID.c_str())))
					{
						UCLIDException ue("ELI03585", "Unable to create instance of OCR engine object!");
						ue.addDebugInfo("EngineProgID", m_strDefaultOCREngineProgID);
						throw ue;
					}
					else if (m_strDefaultOCREngineProgID == g_strScansoftOCREngineProgId)
					{
						// initialize the private license for the OCR engine
						IPrivateLicensedComponentPtr ipScansoftOCREngine(m_ipOCREngine);
						// Load SSOCR.dll to make sure it will not be unloaded
						// before the destructor is called
						m_hModuleSSOCR = ::LoadLibrary("SSOCR.dll");
						if (m_hModuleSSOCR == NULL)
						{
							throw UCLIDException("ELI07607", "Failed to load SSOCR.dll.");
						}

						_bstr_t _bstrPrivateLicenseCode = LICENSE_MGMT_PASSWORD.c_str();
						ipScansoftOCREngine->InitPrivateLicense(_bstrPrivateLicenseCode);
					}
				}

				// specify the oCREngine object to the input receiver
				ipInputReceiver->SetOCREngine(m_ipOCREngine);

				UCLID_INPUTFUNNELLib::IOCRFilterPtr ipOCRFilter = m_ipOCRFilterMgr;
				ASSERT_RESOURCE_ALLOCATION("ELI16985", ipOCRFilter != __nullptr);

				// specify the OCRFilter object to the input receiver
				ipInputReceiver->SetOCRFilter(ipOCRFilter);
			}

			// enable/disable input
			if (m_bInputEnabled)
			{
				_bstr_t bstrInputType = m_currentInputRequestInfo.m_ipInputValidator->GetInputType();

				ipInputReceiver->EnableInput(bstrInputType,
					get_bstr_t(m_currentInputRequestInfo.m_strPrompt));
			}
			else
			{
				ipInputReceiver->DisableInput();
			}

			// notify input context about the newly connected input receiver
			if (m_ipCurrentInputContext != __nullptr)
			{
				m_ipCurrentInputContext->NotifyNewIRConnected(ipInputReceiver);
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02504")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputManager::DisconnectInputReceiver(long nIRHandle)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// Find the input receiver in the map
		UCLID_INPUTFUNNELLib::IInputReceiverPtr ipInputReceiver;
		bool bFound = findInputReceiver(nIRHandle, ipInputReceiver);
		if (bFound)
		{
			if (ipInputReceiver)
			{
				// remove the input receiver from the map
				m_mapIDToInputReceiver.erase(nIRHandle);
			}
			else
			{
				UCLIDException uclidException("ELI02240", "Null interface pointer is detected!");
				uclidException.addDebugInfo("Input Receiver ID", nIRHandle);
				throw uclidException;
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02507")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputManager::GetInputReceiver(long nIRHandle, IInputReceiver **pReceiver)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		validateLicense();
		
		UCLID_INPUTFUNNELLib::IInputReceiverPtr ipInputReceiver;
		// find the input receiver
		if (!findInputReceiver(nIRHandle, ipInputReceiver))
		{
			UCLIDException uclidException("ELI02251", "Failed to get specified InputReceiver.");
			uclidException.addDebugInfo("nIRHandle", nIRHandle);
			throw uclidException;
		}
		
		*pReceiver = (IInputReceiver*) ipInputReceiver.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02510")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputManager::ShowWindows(VARIANT_BOOL bShow)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		
		m_bWindowShown = asCppBool(bShow);
		
		// show input receivers' window if have window
		VARIANT_BOOL bHasWindow = VARIANT_FALSE;
		map<unsigned long, UCLID_INPUTFUNNELLib::IInputReceiverPtr >::iterator iter;
		for (iter = m_mapIDToInputReceiver.begin(); iter != m_mapIDToInputReceiver.end(); iter++)
		{
			// Retrieve the Input Receiver
			UCLID_INPUTFUNNELLib::IInputReceiverPtr pIR = iter->second;
			ASSERT_RESOURCE_ALLOCATION("ELI15631", pIR != __nullptr);

			// Check if window exists
			bHasWindow = pIR->HasWindow;
			if (bHasWindow == VARIANT_TRUE)
			{
				// Show or hide the window
				pIR->ShowWindow(bShow);
			}
		}	
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02513")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputManager::GetOCRFilterMgr(IOCRFilterMgr **pOCRFilterMgr)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// increment reference count and return pointer to caller
		UCLID_INPUTFUNNELLib::IOCRFilterMgrPtr ipOCRFilterMgr = m_ipOCRFilterMgr;
		*pOCRFilterMgr = (IOCRFilterMgr*) ipOCRFilterMgr.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03473")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputManager::SetOCREngine(IOCREngine *pEngine)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		validateLicense();

		if (pEngine == NULL)
		{
			UCLIDException ue("ELI03584", "Invalid OCR Engine object passed to InputManager.SetOCREngine()!");
			ue.addDebugInfo("pEngine", "NULL");
			throw ue;
		}

		// update the internal reference to the OCR engine object
		m_ipOCREngine = pEngine;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03583")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputManager::ProcessTextInput(BSTR strInput)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// create a text input object with the given text and simulate an input-received event
		UCLID_INPUTFUNNELLib::ITextInputPtr ipTextInput(CLSID_TextInput);
		ASSERT_RESOURCE_ALLOCATION("ELI16959", ipTextInput != __nullptr);
		ipTextInput->InitTextInput(NULL, get_bstr_t(strInput));

		// get event handler and notify input received
		UCLID_INPUTFUNNELLib::IIREventHandlerPtr ipEventHandler = getThisAsCOMPtr();
		ASSERT_RESOURCE_ALLOCATION("ELI16961",ipEventHandler != __nullptr);
		ipEventHandler->NotifyInputReceived(ipTextInput);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04108")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputManager::SetInputContext(IInputContext *pInputContext)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		UCLID_INPUTFUNNELLib::IInputContextPtr ipNewIC(pInputContext);
		if (pInputContext == NULL || m_ipCurrentInputContext == ipNewIC)
		{
			return S_OK;
		}

		m_ipCurrentInputContext = ipNewIC;
		if (m_ipCurrentInputContext)
		{
			m_ipCurrentInputContext->Activate(getThisAsCOMPtr());
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04109")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputManager::GetInputReceivers(IIUnknownVector **pInputReceivers)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// return all available input receivers from m_mapIDToInputReceiver
		IIUnknownVectorPtr ipInputReceivers(CLSID_IUnknownVector);

		map<unsigned long, UCLID_INPUTFUNNELLib::IInputReceiverPtr >::iterator it = m_mapIDToInputReceiver.begin();
		for (; it != m_mapIDToInputReceiver.end(); it++)
		{
			ipInputReceivers->PushBack(it->second);
		}

		*pInputReceivers = ipInputReceivers.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04113")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputManager::GetHWND(long *phWnd)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	*phWnd = (long) getWindowIRManager()->m_hWnd;

	return S_OK;
}
//-------------------------------------------------------------------------------------------------

// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputManager::raw_IsLicensed(VARIANT_BOOL * pbValue)
{	
	try
	{
		validateLicense();
		// if validateLicense doesn't throw any exception, then pbValue is true
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IIRInputHandler
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputManager::NotifyInputReceived(ITextInput * pTextInput)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	// if there's already an input text received and is being processed,
	// lock the process and not take any other input received events till 
	// current process finishes.
	static bool bLocked = false;

	try
	{
		validateLicense();

		UCLID_INPUTFUNNELLib::ITextInputPtr ipTextInput(pTextInput);
		ASSERT_RESOURCE_ALLOCATION("ELI16986", ipTextInput != __nullptr);

		// get input entity
		UCLID_INPUTFUNNELLib::IInputEntityPtr ipEntity = ipTextInput->GetInputEntity();
		ASSERT_RESOURCE_ALLOCATION("ELI16987", ipEntity != __nullptr);

		VARIANT_BOOL bRet = VARIANT_FALSE;
		if (bLocked)
		{
			::Beep(500, 50);

			if (ipEntity)
			{
				bRet = ipEntity->CanBeDeleted();
				if (asCppBool(bRet))
				{
					ipEntity->Delete();
				}
			}

			return S_OK;
		}

		// lock the current process and make sure that the value of bLocked gets
		// restored back to false upon exit from this method
		ValueRestorer<bool> restorer(bLocked, false);
		bLocked = true;

		// Validate the input
		bRet = m_currentInputRequestInfo.m_ipInputValidator->ValidateInput(ipTextInput);

		// if the input text is invalid
		if (bRet == VARIANT_FALSE)
		{
			// bring up the input correction
			UCLID_INPUTFUNNELLib::IInputCorrectionUIPtr ipInputCorrection(CLSID_InputCorrectionUI);
			ASSERT_RESOURCE_ALLOCATION("ELI16960", ipInputCorrection != __nullptr);
			
			// set the parent window attribute of the input correction UI
			ipInputCorrection->ParentWndHandle = m_lParentWndHandle;

			VARIANT_BOOL bSuccess = ipInputCorrection->PromptForCorrection( 
				m_currentInputRequestInfo.m_ipInputValidator, ipTextInput);
			
			// if user cancels
			if (bSuccess == VARIANT_FALSE)
			{
				// if there is an input entity object, delete it
				if (ipEntity != __nullptr)
				{
					bRet = ipEntity->CanBeDeleted();
					if (asCppBool(bRet))
					{
						ipEntity->Delete();
					}
				}

				// return without firing OnInputReceived()
				return S_OK;
			}
		}

		// we have a valid input
		if (ipEntity)
		{
			VARIANT_BOOL bMark = ipEntity->CanBeMarkedAsUsed();

			// if the input entity can be marked as used, then mark it
			if (bMark == VARIANT_TRUE)
			{
				// only mark it as used if it's not marked yet
				bMark = ipEntity->IsMarkedAsUsed();
				if (bMark == VARIANT_FALSE)
				{
					ipEntity->MarkAsUsed(VARIANT_TRUE);
				}
			}
		}
		
		// fire the event to the application
		Fire_NotifyInputReceived(pTextInput);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03149")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputManager::NotifyAboutToDestroy(IInputReceiver * pInputReceiver)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	HRESULT ret = S_OK;
	try
	{
		validateLicense();

		// call DisconnectInputReceiver
		unsigned long nID;
		if (findInputReceiverID(nID, pInputReceiver))
		{
			ret = DisconnectInputReceiver(nID);
			if (FAILED(ret))
			{
				return ret;
			}
		}

		return ret;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI02519")
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
UCLID_INPUTFUNNELLib::IInputManagerPtr CInputManager::getThisAsCOMPtr()
{
	UCLID_INPUTFUNNELLib::IInputManagerPtr ipThis(this);
	ASSERT_RESOURCE_ALLOCATION("ELI16969", ipThis != __nullptr);

	return ipThis;
}
//-------------------------------------------------------------------------------------------------
WindowIRManager* CInputManager::getWindowIRManager()
{
	// if the external-window IR manager has not yet been created,
	// create it now.
	if (m_apWindowIRManager.get() == NULL)
	{
		m_apWindowIRManager = unique_ptr<WindowIRManager>(new WindowIRManager(this));
	}

	return m_apWindowIRManager.get();
}
//-------------------------------------------------------------------------------------------------
bool CInputManager::findInputReceiver(unsigned long nID, 
									  UCLID_INPUTFUNNELLib::IInputReceiverPtr &ipInputReceiver)
{
	if (m_mapIDToInputReceiver.size() > 0)
	{
		map<unsigned long, UCLID_INPUTFUNNELLib::IInputReceiverPtr >::iterator iter = 
			m_mapIDToInputReceiver.find(nID);

		// found the input receiver
		if (iter != m_mapIDToInputReceiver.end())
		{
			ipInputReceiver = iter->second;
			
			return true;
		}	
	}
	
	return false;
}
//--------------------------------------------------------------------------------------------------
bool CInputManager::findInputReceiverID(unsigned long &nID, 
									const UCLID_INPUTFUNNELLib::IInputReceiverPtr &ipInputReceiver)
{
	if (m_mapIDToInputReceiver.size() > 0)
	{
		map<unsigned long, UCLID_INPUTFUNNELLib::IInputReceiverPtr >::iterator iter;
		for (iter = m_mapIDToInputReceiver.begin(); iter != m_mapIDToInputReceiver.end(); iter++)
		{
			if (iter->second == ipInputReceiver)
			{
				nID = iter->first;
				
				return true;
			}
		}
	}

	return false;
}
//-------------------------------------------------------------------------------------------------
void CInputManager::loadCOMUtilsDll()
{
	if (m_hModuleCOMUtils == NULL)
	{
		m_hModuleCOMUtils = ::LoadLibrary("UCLIDCOMUtils.dll");
		if (m_hModuleCOMUtils == NULL)
		{
			throw UCLIDException("ELI15705", "Failed to load UCLIDCOMUtils.dll!");
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CInputManager::validateLicense()
{
	static const unsigned long INPUT_MANAGER_COMPONENT_ID = gnINPUTFUNNEL_CORE_OBJECTS;
	
	VALIDATE_LICENSE( INPUT_MANAGER_COMPONENT_ID, "ELI02603", "InputManager" );
}
//-------------------------------------------------------------------------------------------------
