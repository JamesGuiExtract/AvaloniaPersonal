
#include "stdafx.h"
#include "SSOCR.h"
#include "ScansoftOCR.h"
#include "PSUpdateThreadMgr.h"

#include <COMUtils.h>
#include <cpputil.h>
#include <IdleProcessKiller.h>
#include <LicenseMgmt.h>
#include <RegConstants.h>
#include <RegistryPersistenceMgr.h>
#include <UCLIDException.h>

#include <memory>
#include <string>
using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// Globals and constants
//-------------------------------------------------------------------------------------------------
// global variables / static variables / externs
extern CComModule _Module;

string CScansoftOCR::ms_strCharSet;
string CScansoftOCR::ms_strTrainingFileName;

// the class number for windows dialogs
const int giWINDOWS_DIALOG_CLASSNUM = 32770;

// the maximum of number of characters to expect from SSOCR2 error dialogs
const int giMAX_ERROR_WINDOW_TEXT = 1024;

// title of SSOCR2 application error dialog
const char* gpszSSOCR2_APP_ERROR_TITLE = "SSOCR2.exe - Application Error";

// title of debugger error dialog
const char* gpszSSOCR2_DEBUGGER_ERROR_TITLE = "Visual Studio Just-In-Time Debugger";

// text to search for within a debugger error message to ensure it is SSOCR2-related
const char* gpszSSOCR2_DEBUGGER_ERROR_TEXT = "SSOCR2.exe";

// title of Vista error dialog
const char* gpszSSOCR2_VISTA_ERROR_TITLE = "Microsoft Windows";

// text to search for within a Vista error message to ensure it is SSOCR2-related
const char* gpszSSOCR2_VISTA_ERROR_TEXT = "SSOCR2 Module";

// number of milliseconds to wait before closing the SSOCR2 error dialog
const unsigned long gulSSOCR2_ERROR_CLOSE_WAIT = 2000;

//-------------------------------------------------------------------------------------------------
// CScansoftOCR
//-------------------------------------------------------------------------------------------------
CScansoftOCR::CScansoftOCR()
: m_pid(-1),
  m_bPrivateLicenseInitialized(false),
  m_ulNumImagesProcessed(0),
  m_bLookForErrorDialog(false),
  m_bKilledOcrDoNotRetry(false),
  m_hwndErrorDialog(NULL)
{
	try
	{
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI03369")
}
//-------------------------------------------------------------------------------------------------
CScansoftOCR::~CScansoftOCR()
{
	try
	{
		killOCREngine();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI03374")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CScansoftOCR::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IOCREngine
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CScansoftOCR::raw_RecognizeTextInImage(BSTR strImageFileName, long lStartPage,
	long lEndPage, EFilterCharacters eFilter, BSTR bstrCustomFilterCharacters, 
	EOcrTradeOff eTradeOff, VARIANT_BOOL bReturnSpatialInfo, IProgressStatus* pProgressStatus, 
	ISpatialString** pstrText)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		Win32CriticalSectionLockGuard lg(m_cs);
		
		// validate the license
		validateLicense();
		checkOCREngine();

		// Get the image file name and check for .uss extension [FlexIDSCore #3242]
		string stdstrImageFileName = asString(strImageFileName);
		if (getExtensionFromFullPath(stdstrImageFileName, true) == ".uss")
		{
			UCLIDException uex("ELI24861", "Cannot OCR a '.uss' file!");
			uex.addDebugInfo("File To OCR", stdstrImageFileName);
			throw uex;
		}

		// scope for critical section
		{
			Win32CriticalSectionLockGuard lg2(m_csKillingOCR);

			// Reset the killed OCR flag to false
			m_bKilledOcrDoNotRetry = false;
		}

		// ensure the return value is not NULL
		ASSERT_ARGUMENT("ELI18184", pstrText != __nullptr);

		// recognize the text - with auto-rotation
		ISpatialStringPtr ipRecognizedText = recognizeText(strImageFileName, 
			createPageNumberVector(strImageFileName, lStartPage, lEndPage), NULL, 0, eFilter, 
			bstrCustomFilterCharacters, eTradeOff, VARIANT_FALSE, VARIANT_FALSE, 
			bReturnSpatialInfo, pProgressStatus);
		ASSERT_RESOURCE_ALLOCATION("ELI18355", ipRecognizedText != __nullptr);

		// return the recognized text
		*pstrText = ipRecognizedText.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05671")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CScansoftOCR::raw_RecognizeTextInImage2(BSTR strImageFileName, 
													 BSTR strPageNumbers,
													 VARIANT_BOOL bReturnSpatialInfo,
													 IProgressStatus* pProgressStatus,
													 ISpatialString* *pstrText)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		Win32CriticalSectionLockGuard lg(m_cs);
		// validate the license
		validateLicense();
		checkOCREngine();

		// Get the image file name and check for .uss extension [FlexIDSCore #3242]
		string stdstrImageFileName = asString(strImageFileName);
		if (getExtensionFromFullPath(stdstrImageFileName, true) == ".uss")
		{
			UCLIDException uex("ELI24865", "Cannot OCR a '.uss' file!");
			uex.addDebugInfo("File To OCR", stdstrImageFileName);
			throw uex;
		}

		// scope for critical section
		{
			Win32CriticalSectionLockGuard lg2(m_csKillingOCR);

			// Reset the killed OCR flag to false
			m_bKilledOcrDoNotRetry = false;
		}

		// ensure the return value is not NULL
		ASSERT_ARGUMENT("ELI18185", pstrText != __nullptr);

		// get a vector of specified page numbers
		IVariantVectorPtr ipvecPageNumbers 
			= getImageUtils()->GetImagePageNumbers(strImageFileName, strPageNumbers);
		ASSERT_RESOURCE_ALLOCATION("ELI10272", ipvecPageNumbers != __nullptr);

		// the vector of page numbers must contain at least one page number
		if (get_bstr_t(strPageNumbers).length() == 0)
		{
			throw UCLIDException("ELI10273", "You must specify at least one page to be recognized.");
		}

		// recognize the text - with auto-rotation
		ISpatialStringPtr ipRecognizedText = recognizeText(strImageFileName, ipvecPageNumbers, NULL, 
			0, kNoFilter, NULL, kRegistry, VARIANT_FALSE, VARIANT_FALSE, bReturnSpatialInfo, 
			pProgressStatus);
		ASSERT_RESOURCE_ALLOCATION("ELI18356", ipRecognizedText != __nullptr);

		*pstrText = ipRecognizedText.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10271")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CScansoftOCR::raw_SupportsTrainingFiles(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		Win32CriticalSectionLockGuard lg(m_cs);
		// validate the license
		validateLicense();
		checkOCREngine();

		// return true, as this implementation supports training files
		*pbValue = getOCREngine()->SupportsTrainingFiles();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03478")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CScansoftOCR::raw_LoadTrainingFile(BSTR strTrainingFileName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		Win32CriticalSectionLockGuard lg(m_cs);
		// validate the license
		validateLicense();
		checkOCREngine();

		getOCREngine()->LoadTrainingFile(strTrainingFileName);
		ms_strTrainingFileName = asString(strTrainingFileName);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03479")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CScansoftOCR::raw_RecognizeTextInImageZone(BSTR strImageFileName, long lStartPage, 
	long lEndPage, ILongRectangle* pZone, long nRotationInDegrees, EFilterCharacters eFilter, 
	BSTR bstrCustomFilterCharacters, VARIANT_BOOL bDetectHandwriting, 
	VARIANT_BOOL bReturnUnrecognized, VARIANT_BOOL bReturnSpatialInfo, 
	IProgressStatus* pProgressStatus, ISpatialString* *pstrText)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		Win32CriticalSectionLockGuard lg(m_cs);

		// validate the retval
		ASSERT_ARGUMENT("ELI18186", pstrText != __nullptr);
		
		// validate the license
		validateLicense();
		checkOCREngine();

		// Get the image file name and check for .uss extension [FlexIDSCore #3242]
		string stdstrImageFileName = asString(strImageFileName);
		if (getExtensionFromFullPath(stdstrImageFileName, true) == ".uss")
		{
			UCLIDException uex("ELI24866", "Cannot OCR a '.uss' file!");
			uex.addDebugInfo("File To OCR", stdstrImageFileName);
			throw uex;
		}

		// scope for critical section
		{
			Win32CriticalSectionLockGuard lg2(m_csKillingOCR);

			// Reset the killed OCR flag to false
			m_bKilledOcrDoNotRetry = false;
		}

		// Recognize the text on the desired page numbers
		ISpatialStringPtr ipRecognizedText;
		
		// check if handwriting is being detected
		// NOTE: handwriting recognition uses decomposition algorithms to detect
		// the location of individual characters, while recognition of printed text
		// does not.
		if(bDetectHandwriting == VARIANT_TRUE)
		{
			// recognize the text, reOCRing if one decomposition method fails
			ipRecognizedText = recognizeText(strImageFileName, 
				createPageNumberVector(strImageFileName, lStartPage, lEndPage), pZone, 
				nRotationInDegrees, eFilter, bstrCustomFilterCharacters, kRegistry, 
				bDetectHandwriting, bReturnUnrecognized, bReturnSpatialInfo, pProgressStatus);
		}
		else
		{
			// only one decomposition method is necessary
			ipRecognizedText = recognizePrintedTextInImageZone(strImageFileName, lStartPage, 
				lEndPage, pZone, nRotationInDegrees, eFilter, bstrCustomFilterCharacters, 
				bReturnUnrecognized, bReturnSpatialInfo, pProgressStatus);
		}
		ASSERT_RESOURCE_ALLOCATION("ELI18354", ipRecognizedText != __nullptr);

		// Return the final spatial string
		*pstrText = ipRecognizedText.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12774");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CScansoftOCR::raw_WhackOCREngine()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Lock the mutex over the section
		Win32CriticalSectionLockGuard lg(m_csKillingOCR);

		// Kill the OCR engine
		killOCREngine();

		// Set the do not retry flag to true
		m_bKilledOcrDoNotRetry = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22046");
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CScansoftOCR::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		Win32CriticalSectionLockGuard lg(m_cs);
		
		// ensure the return value pointer is valid 
		ASSERT_ARGUMENT("ELI18357", pbValue != __nullptr);

		try
		{
			// validate license
			validateLicense();
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18358");
		
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPrivateLicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CScansoftOCR::raw_InitPrivateLicense(BSTR strPrivateLicenseKey)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		Win32CriticalSectionLockGuard lg(m_cs);

		checkOCREngine();
		// ensure that the Engine has been created
		getOCREngine();

		string strKey = asString(strPrivateLicenseKey);
		initOCREngineLicense(strKey);

		// NOTE: the following statements will only be called if the initialization
		// succeeds (does not throw an exception)
		// the information is saved so that any future creations of the engine can be 
		// automatically privately licensed

		// the private license key is valid, set the bit
		m_bPrivateLicenseInitialized = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11025")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CScansoftOCR::raw_IsPrivateLicensed(VARIANT_BOOL *pbIsLicensed)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		Win32CriticalSectionLockGuard lg(m_cs);
		// NOTE: this method shall not check regular license
		*pbIsLicensed = asVariantBool(m_bPrivateLicenseInitialized);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11026")
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CScansoftOCR::validateLicense()
{
	// SSOCR MUST be privately licensed
	if (m_bPrivateLicenseInitialized)
	{
		return;
	}

	// Prepare and throw an exception if component is not licensed
	UCLIDException ue("ELI03638", "ScansoftOCR component is not privately licensed!");
	ue.addDebugInfo("Component Name", "Scansoft OCR Engine");
	throw ue;
}
//-------------------------------------------------------------------------------------------------
void CScansoftOCR::initOCREngineLicense(string strKey)
{
	IPrivateLicensedComponentPtr ipPLComponent = m_ipOCREngine;
	ASSERT_RESOURCE_ALLOCATION("ELI11049", ipPLComponent != __nullptr);
	ipPLComponent->InitPrivateLicense(strKey.c_str());
}
//-------------------------------------------------------------------------------------------------
IScansoftOCR2Ptr CScansoftOCR::getOCREngine()
{
	// if the maximum number of image recognitions per OCR engine instance
	// has already taken place, then recreate another instance of the OCR engine
	// and delete the current instance
	if (m_ulNumImagesProcessed == getMaxRecognitionsPerOCREngineInstance())
	{
		killOCREngine();
		InterlockedExchange(&m_ulNumImagesProcessed, 0);
	}

	if (m_ipOCREngine == __nullptr)
	{
		m_ipOCREngine.CreateInstance(CLSID_ScansoftOCR2);
		ASSERT_RESOURCE_ALLOCATION("ELI11088", m_ipOCREngine != __nullptr);

		m_pid = m_ipOCREngine->GetPID();

		// if the SSOCR2 has been successfully licensed by a call to InitPrivateLicense,
		// initialize the newly created SSOCR2 again, using the proper license key.
		// NOTE: if InitPrivateLicense was not previously called with the proper license key, 
		// m_bPrivateLicenseInitialized will be false and the newly created SSOCR2 will not be licensed.
		if (m_bPrivateLicenseInitialized)
		{
			initOCREngineLicense(LICENSE_MGMT_PASSWORD);

			// only load a training file if SSOCR2 has been licensed and a training
			// file has been specified by a previous call to LoadTrainingFile
			if(!ms_strTrainingFileName.empty())
			{
				m_ipOCREngine->LoadTrainingFile(ms_strTrainingFileName.c_str());
			}
		}
	}
	return m_ipOCREngine;
}
//-------------------------------------------------------------------------------------------------
unsigned long CScansoftOCR::getMaxRecognitionsPerOCREngineInstance()
{
	// only one thread may enter this function at any given time
	static CMutex ls_lock;
	CSingleLock guard(&ls_lock, TRUE);

	static unsigned long ls_ulMaxImageRecognitions = 0;
	static bool ls_bInitialized = false;
	
	if (!ls_bInitialized)
	{
		// constants used for registry access and default values
		const string strREG_PATH_FOR_THIS_CLASS = gstrCOM_COMPONENTS_REG_PATH + "\\UCLIDRasterAndOCRMgmt\\OCREngines\\SSOCR2";
		const string strREG_MAX_IMAGES_PER_INSTANCE_KEY_NAME = "MaxFilesPerInstance";
		const unsigned long ulDEFAULT_VALUE = 1000;
		bool bUseDefault = true;

		// create an instance of RegistryPersistenceMgr to query the registry
		unique_ptr<IConfigurationSettingsPersistenceMgr> apSettings = 
			unique_ptr<IConfigurationSettingsPersistenceMgr>(new RegistryPersistenceMgr(HKEY_LOCAL_MACHINE,
			strREG_PATH_FOR_THIS_CLASS));
		ASSERT_RESOURCE_ALLOCATION("ELI13194", apSettings.get() != __nullptr);

		if (apSettings->keyExists("", strREG_MAX_IMAGES_PER_INSTANCE_KEY_NAME))
		{
			// get the key value and convert to unsigned long.
			// if the key value cannot be accessed, or is invalid, 
			// an exception will automatically be thrown 
			string strKeyValue = apSettings->getKeyValue("", strREG_MAX_IMAGES_PER_INSTANCE_KEY_NAME,
				asString(ulDEFAULT_VALUE));
			ls_ulMaxImageRecognitions = asUnsignedLong(strKeyValue);
		}
		else
		{
			// write the default value to the registry
			apSettings->createKey("", strREG_MAX_IMAGES_PER_INSTANCE_KEY_NAME, asString(ulDEFAULT_VALUE));
			ls_ulMaxImageRecognitions = ulDEFAULT_VALUE;
		}

		ls_bInitialized = true;
	}

	return ls_ulMaxImageRecognitions;
}
//-------------------------------------------------------------------------------------------------
void CScansoftOCR::killOCREngine()
{
	// attempt to close SSOCR2 gracefully first. [P13 #4590]
	m_ipOCREngine = __nullptr;

	// hard kill OCR engine
	HANDLE hProcess = OpenProcess(PROCESS_ALL_ACCESS, FALSE, m_pid);
	if ( hProcess != __nullptr )
	{
		// terminate the process
		TerminateProcess(hProcess, 0);
		CloseHandle(hProcess);
	}
	m_pid = -1;
}
//-------------------------------------------------------------------------------------------------
void CScansoftOCR::checkOCREngine()
{
	try
	{
		try
		{
			// ping the OCR engine
			getOCREngine()->GetPID();

			// if there are any error messages from "dead" OCR engine instances, get rid of them

			// When the SSOCR2 process is terminated, **sometimes** an error message box
			// comes up indicating that a certain memory location could not be read.
			// check to see if one of those message boxes are up.  If so, close that
			// message box and log an exception

			// giWINDOWS_DIALOG_CLASSNUM is the WNDCLASS number of a dialog
			// and that is what we are looking for
			// NOTE: We are attempting to find the error window multiple times because
			// dismissing the error window **sometimes** brings up more error windows.
			m_bLookForErrorDialog = true;
			while(m_bLookForErrorDialog)
			{
				// don't look again unless a dialog window was found and dismissed
				// m_bLookForErrorDialog will be set internally if a dialog is dismissed.
				m_bLookForErrorDialog = false;

				// look for SSOCR2 application error dialogs
				m_hwndErrorDialog = FindWindowEx(NULL, NULL, MAKEINTATOM(giWINDOWS_DIALOG_CLASSNUM), 
					gpszSSOCR2_APP_ERROR_TITLE);
				while(m_hwndErrorDialog != __nullptr)
				{
					// auto-dismiss this window
					dismissErrorDialog(m_hwndErrorDialog);

					// search for the next SSOCR2 application error window
					m_hwndErrorDialog = FindWindowEx(NULL, m_hwndErrorDialog, 
						MAKEINTATOM(giWINDOWS_DIALOG_CLASSNUM), gpszSSOCR2_APP_ERROR_TITLE);
				}

				// look for debugger error dialogs
				m_hwndErrorDialog = FindWindowEx(NULL, NULL, MAKEINTATOM(giWINDOWS_DIALOG_CLASSNUM), 
					gpszSSOCR2_DEBUGGER_ERROR_TITLE);
				while(m_hwndErrorDialog != __nullptr)
				{
					// if this window contains SSOCR2-related text, auto-dismiss it
					EnumChildWindows(m_hwndErrorDialog, enumForSSOCR2Text, (LPARAM) this);
					
					// search for the next debugger error dialog
					m_hwndErrorDialog = FindWindowEx(NULL, m_hwndErrorDialog, 
						MAKEINTATOM(giWINDOWS_DIALOG_CLASSNUM), gpszSSOCR2_DEBUGGER_ERROR_TITLE);
				}

				// Look for Vista error dialogs
				m_hwndErrorDialog = FindWindowEx(NULL, NULL, MAKEINTATOM(giWINDOWS_DIALOG_CLASSNUM), 
					gpszSSOCR2_VISTA_ERROR_TITLE);
				while(m_hwndErrorDialog != __nullptr)
				{
					// if this window contains SSOCR2-related text, auto-dismiss it
					EnumChildWindows(m_hwndErrorDialog, enumForSSOCR2Text, (LPARAM) this);
					
					// search for the next debugger error dialog
					m_hwndErrorDialog = FindWindowEx(NULL, m_hwndErrorDialog, 
						MAKEINTATOM(giWINDOWS_DIALOG_CLASSNUM), gpszSSOCR2_VISTA_ERROR_TITLE);
				}
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI23454");
	}
	catch(UCLIDException& uex)
	{
		// Log the exception that was caught
		uex.log();

		if (m_pid > 0)
		{
			killOCREngine();
		}
	}
}
//-------------------------------------------------------------------------------------------------
IImageUtilsPtr CScansoftOCR::getImageUtils()
{
	if (m_ipImageUtils == __nullptr)
	{
		m_ipImageUtils.CreateInstance(CLSID_ImageUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI10274", m_ipImageUtils != __nullptr);
	}

	return m_ipImageUtils;
}
//-------------------------------------------------------------------------------------------------
ISpatialStringPtr CScansoftOCR::recognizeText(BSTR strImageFileName, IVariantVectorPtr ipPageNumbers, 
	ILongRectangle* pZone, long nRotationInDegrees, EFilterCharacters eFilter, 
	 BSTR bstrCustomFilterCharacters, EOcrTradeOff eTradeOff, VARIANT_BOOL vbDetectHandwriting, 
	VARIANT_BOOL vbReturnUnrecognized, VARIANT_BOOL bReturnSpatialInfo, 
	IProgressStatus* pProgressStatus)
{
	// increment # of images processed
	InterlockedIncrement(&m_ulNumImagesProcessed);

	// create auto-pointer for progress status update thread manager
	unique_ptr<PSUpdateThreadManager> apPSUpdateThread;

	// initialize stream to hold OCR results
	_bstr_t _bstrStream;

	// prepare decomposition methods for OCR loop
	EPageDecompositionMethod eDecompositionMethod[3];

	// get the primary decomposition method from the OCR engine
	eDecompositionMethod[0] = getOCREngine()->GetPrimaryDecompositionMethod();

	// set the secondary decomposition method to legacy, 
	// unless legacy was the primary decomposition method
	eDecompositionMethod[1] = (eDecompositionMethod[0] == kLegacyDecomposition ? 
			kStandardDecomposition : kLegacyDecomposition);

	// this value will only be used if the primary decomposition method is auto 
	// (see NOTE in next comment)
	eDecompositionMethod[2] = kStandardDecomposition;

	// set the total number of decomposition methods to try
	// NOTE: Auto decomposition simply uses a special algorithm to determine whether to use legacy
	//       or standard decomposition. If the primary decomposition method was not auto, we only
	//       need to do two decomposition methods (legacy and standard), since auto decomposition 
	//       would only try legacy or standard again. If auto decomposition was the primary method, 
	//       we need to try all three, since we do not know what method auto decomposition chose.
	//
	//       Thus the three possible decomposition method sequences are:
	//         1) Auto, Legacy, Standard
	//         2) Legacy, Standard
	//         3) Standard, Legacy
	int iNumDecompositionMethods = (eDecompositionMethod[0] == kAutoDecomposition ? 3 : 2);
	
	// Maintain an exception that aggregates together all exceptions generated by failed
	// decomposition attempts.
	unique_ptr<UCLIDException> apAggregateException;

	// iterate through different decomposition methods until one works
	bool bTryAgain = true;
	for(int i=0; bTryAgain && i < iNumDecompositionMethods; i++)
	{
		// don't OCR again (unless this decomposition method doesn't work)
		bTryAgain = false;

		try
		{
			try
			{
				// Get the OCR engine
				IScansoftOCR2Ptr ipOcrEngine = getOCREngine();
				ASSERT_RESOURCE_ALLOCATION("ELI25216", ipOcrEngine != __nullptr);

				// check if the progress status object should be updated
				if (pProgressStatus)
				{
					// create thread to handle progress status updates
					apPSUpdateThread.reset(new PSUpdateThreadManager(pProgressStatus, ipOcrEngine, 
						ipPageNumbers->Size));
					ASSERT_RESOURCE_ALLOCATION("ELI16207", apPSUpdateThread.get() != __nullptr);
				}

				// Kill the process if it idles
				IdleProcessKiller killer(m_pid);
				
				// OCR the image and get the results
				_bstrStream = ipOcrEngine->RecognizeText(strImageFileName, ipPageNumbers, pZone, 
					nRotationInDegrees, eFilter, bstrCustomFilterCharacters, eTradeOff, 
					vbDetectHandwriting, vbReturnUnrecognized, bReturnSpatialInfo, 
					asVariantBool(pProgressStatus != __nullptr), eDecompositionMethod[i]);

				// check if the progress status update thread still exists
				if (apPSUpdateThread.get() != __nullptr)
				{
					// mark the progress status as complete
					apPSUpdateThread->notifyOCRComplete();
				}
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI11095");
		}
		catch(UCLIDException ue)
		{
			// NOTE: Check for manual killing of OCR engine first so that no exception
			// is logged indicating that the decomposition method failed.
			// Scope for critical section
			{
				Win32CriticalSectionLockGuard lg(m_csKillingOCR);

				if(m_bKilledOcrDoNotRetry)
				{
					// If OCR was killed, just return an empty spatial string
					ISpatialStringPtr ipSS(CLSID_SpatialString);
					return ipSS;
				}
			}

			string strTopLevelText;

			// Create an exception message specifying the method used
			if (eDecompositionMethod[i] == kAutoDecomposition)
			{
				strTopLevelText = 
					"Application trace: Unable to OCR document with automatic decomposition attempt.";
			}
			else if (eDecompositionMethod[i] == kLegacyDecomposition)
			{
				strTopLevelText =
					"Application trace: Unable to OCR document with legacy decomposition attempt.";
			}
			else
			{
				strTopLevelText = 
					"Application trace: Unable to OCR document with standard decomposition attempt.";
			}

			// Create or append to the aggregate exception.
			if (apAggregateException.get() == NULL)
			{
				apAggregateException.reset(new UCLIDException("ELI25299", strTopLevelText));
			}
			else
			{
				apAggregateException.reset(new UCLIDException("ELI25304", strTopLevelText, 
					*apAggregateException));
			}

			// Add as debug info all history entries from ue
			apAggregateException->addDebugInfo("Exception History", ue);

			// kill the OCR engine before retrying
			killOCREngine();

			// try again with a different decomposition method if OCR was not intentionally killed
			bTryAgain = true;
		}
	}

	// Throw the aggregate exception if all decompsition methods failed.
	if (bTryAgain)
	{
		apAggregateException.reset(new UCLIDException("ELI25300", 
			"Failure to OCR image. All decomposition methods failed.", *apAggregateException));

		apAggregateException->addDebugInfo("Failed Image", asString(strImageFileName));
		throw *apAggregateException;
	}
	// ...or log the aggregate exception if at least one attempt failed before another succeeded.
	else if (apAggregateException.get() != __nullptr)
	{
		apAggregateException.reset(new UCLIDException("ELI29014", 
			"Application trace: At least one OCR decomposition methods failed.",
			*apAggregateException));

		apAggregateException->addDebugInfo("Image", asString(strImageFileName));
		apAggregateException->log();
	}


	// get the spatial string from the stream
	IPersistStreamPtr ipObj;
	readObjectFromBSTR(ipObj, _bstrStream);
	ASSERT_RESOURCE_ALLOCATION("ELI18352", ipObj != __nullptr);

	// return the spatial string
	return ipObj;
}
//-------------------------------------------------------------------------------------------------
ISpatialStringPtr CScansoftOCR::recognizePrintedTextInImageZone(BSTR strImageFileName, 
	long lStartPage, long lEndPage, ILongRectangle* pZone, long nRotationInDegrees, 
	EFilterCharacters eFilter, BSTR bstrCustomFilterCharacters, VARIANT_BOOL bReturnUnrecognized, 
	VARIANT_BOOL bReturnSpatialInfo, IProgressStatus* pProgressStatus)
{
	// increment # of images processed
	InterlockedIncrement(&m_ulNumImagesProcessed);

	// create a variant vector array using the specified page number
	IVariantVectorPtr ipPageNumbers = createPageNumberVector(strImageFileName, lStartPage, lEndPage);
	ASSERT_RESOURCE_ALLOCATION("ELI18062", ipPageNumbers != __nullptr);

	// Get the OCR engine
	IScansoftOCR2Ptr ipOcrEngine = getOCREngine();
	ASSERT_RESOURCE_ALLOCATION("ELI25217", ipOcrEngine != __nullptr);

	// create thread to handle progress status updates
	// if progress status updates were requested
	unique_ptr<PSUpdateThreadManager> apPSUpdateThread;
	if (pProgressStatus)
	{
		apPSUpdateThread.reset(new PSUpdateThreadManager(pProgressStatus, ipOcrEngine, 
			ipPageNumbers->Size));
		ASSERT_RESOURCE_ALLOCATION("ELI17315", apPSUpdateThread.get() != __nullptr);
	}

	// Kill the process if it idles
	IdleProcessKiller killer(m_pid);
	
	// OCR the image and get the results
	_bstr_t _bstrStream = ipOcrEngine->RecognizeText(strImageFileName, ipPageNumbers, pZone, 
		nRotationInDegrees, eFilter, bstrCustomFilterCharacters, kRegistry, VARIANT_FALSE, 
		bReturnUnrecognized, bReturnSpatialInfo, asVariantBool(pProgressStatus != __nullptr), 
		kAutoDecomposition);

	// mark the progress status as complete if the 
	// progress status update thread still exists
	if (apPSUpdateThread.get() != __nullptr)
	{
		apPSUpdateThread->notifyOCRComplete();
	}

	// return the spatial string
	IPersistStreamPtr ipStream;
	readObjectFromBSTR(ipStream, _bstrStream);
	ASSERT_RESOURCE_ALLOCATION("ELI18353", ipStream != __nullptr);

	// return the stream as a spatial string
	return (ISpatialStringPtr) ipStream;
}
//-------------------------------------------------------------------------------------------------
IVariantVectorPtr CScansoftOCR::createPageNumberVector(BSTR bstrImageFileName, long lStartPage, 
													   long lEndPage)
{
	// create the page number array
	IVariantVectorPtr ipPageNumbers;
	if (lEndPage < 1)
	{
		// Construct a string for the range of page numbers to search (e.g. "1-")
		string range = asString(lStartPage) + "-";

		// Get the page numbers
		ipPageNumbers = getImageUtils()->GetImagePageNumbers(bstrImageFileName, _bstr_t(range.c_str()));
	}
	else
	{
		ipPageNumbers.CreateInstance(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI17316", ipPageNumbers != __nullptr);

		for (long i = lStartPage; i <= lEndPage; i++)
		{
			ipPageNumbers->PushBack( _variant_t(i) );
		}
	}

	return ipPageNumbers;
}
//-------------------------------------------------------------------------------------------------
void CScansoftOCR::dismissErrorDialog(HWND hWnd)
{
	// Sleep for some time so that the message box is visible for some time
	Sleep(gulSSOCR2_ERROR_CLOSE_WAIT);

	// Close the message box
	::SendMessage(hWnd, WM_CLOSE, 0, 0);

	// log an exception indicating that the SSOCR2 error message was auto-dismissed
	UCLIDException ue("ELI13198",
		"Application trace: SSOCR2.exe Application Error message box auto dismissed.");
	ue.log();

	// dismissing the error dialog sometimes causes more error dialogs to pop up.
	// set the 'look more' flag to keep looking for newly created popup errors.
	m_bLookForErrorDialog = true;
}
//-------------------------------------------------------------------------------------------------
BOOL CALLBACK CScansoftOCR::enumForSSOCR2Text(HWND hWnd, LPARAM lParam)
{
	// get the text of this window
	char buf[giMAX_ERROR_WINDOW_TEXT] = {0};
	GetWindowText(hWnd, buf, giMAX_ERROR_WINDOW_TEXT);

	// check #1 if the text of this window contains SSOCR2 in the text
	if (strstr(buf, gpszSSOCR2_DEBUGGER_ERROR_TEXT) != __nullptr)
	{
		// auto-dismiss this error popup window
		CScansoftOCR* pSSOCR = (CScansoftOCR*) lParam;
		pSSOCR->dismissErrorDialog(pSSOCR->m_hwndErrorDialog);

		// stop enumeration, we have found the window
		return FALSE;
	}

	// check #2 if the text of this window contains SSOCR2 in the text
	if (strstr(buf, gpszSSOCR2_VISTA_ERROR_TEXT) != __nullptr)
	{
		// auto-dismiss this error popup window
		CScansoftOCR* pSSOCR = (CScansoftOCR*) lParam;
		pSSOCR->dismissErrorDialog(pSSOCR->m_hwndErrorDialog);

		// stop enumeration, we have found the window
		return FALSE;
	}

	// continue enumerating
	return TRUE;
}
//-------------------------------------------------------------------------------------------------
