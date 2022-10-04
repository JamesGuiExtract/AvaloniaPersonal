
#include "stdafx.h"
#include "SSOCR.h"
#include "ScansoftOCR.h"
#include "PSUpdateThreadMgr.h"
#include "OcrMethods.h"

#include <COMUtils.h>
#include <cpputil.h>
#include <IdleProcessKiller.h>
#include <LicenseMgmt.h>
#include <RegConstants.h>
#include <RegistryPersistenceMgr.h>
#include <UCLIDException.h>
#include <MutexUtils.h>

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

// Global mutex to ensure only one instance of 
const string gstr_NUANCE_LICENSE_RESET_MUTEX_NAME = "Global\\64EAE541-363A-481B-B51D-B1DDAB52AEB8";

// Substitute for broken function scope static in MSVC 2010
namespace
{
	static CCriticalSection gGetMaxRecognitionsPerOCREngineInstanceMutex;
}

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
		&IID_IOCREngine,
		&IID_IImageFormatConverter,
		&IID_IPrivateLicensedComponent,
		&IID_ILicensedComponent
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
	IOCRParameters* pOCRParameters,
	ISpatialString** pstrText)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		Win32CriticalSectionLockGuard lg(m_cs);
		
		// validate the license
		validateLicense();

		// Reset m_bKilledOcrDoNotRetry before checking/initializing the OCREngine since
		// WhackOCREngine can otherwise be called after the OCR engine is initialized but before
		// m_bKilledOcrDoNotRetry is reset.
		{
			Win32CriticalSectionLockGuard lg2(m_csKillingOCR);

			// Reset the killed OCR flag to false
			m_bKilledOcrDoNotRetry = false;
		}

		checkOCREngine();

		// Get the image file name and check for .uss extension [FlexIDSCore #3242]
		string stdstrImageFileName = asString(strImageFileName);
		if (getExtensionFromFullPath(stdstrImageFileName, true) == ".uss")
		{
			UCLIDException uex("ELI24861", "Cannot OCR a '.uss' file!");
			uex.addDebugInfo("File To OCR", stdstrImageFileName);
			throw uex;
		}

		// ensure the return value is not NULL
		ASSERT_ARGUMENT("ELI18184", pstrText != __nullptr);

		// recognize the text - with auto-rotation
		ISpatialStringPtr ipRecognizedText = recognizeText(strImageFileName, 
			createPageNumberVector(strImageFileName, lStartPage, lEndPage), NULL, 0, eFilter, 
			bstrCustomFilterCharacters, eTradeOff, VARIANT_FALSE, VARIANT_FALSE, 
			bReturnSpatialInfo, pProgressStatus, pOCRParameters);
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
													 IOCRParameters* pOCRParameters,
													 ISpatialString* *pstrText)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		Win32CriticalSectionLockGuard lg(m_cs);
		// validate the license
		validateLicense();

		// Reset m_bKilledOcrDoNotRetry before checking/initializing the OCREngine since
		// WhackOCREngine can otherwise be called after the OCR engine is initialized but before
		// m_bKilledOcrDoNotRetry is reset.
		{
			Win32CriticalSectionLockGuard lg2(m_csKillingOCR);

			// Reset the killed OCR flag to false
			m_bKilledOcrDoNotRetry = false;
		}

		checkOCREngine();

		// Get the image file name and check for .uss extension [FlexIDSCore #3242]
		string stdstrImageFileName = asString(strImageFileName);
		if (getExtensionFromFullPath(stdstrImageFileName, true) == ".uss")
		{
			UCLIDException uex("ELI24865", "Cannot OCR a '.uss' file!");
			uex.addDebugInfo("File To OCR", stdstrImageFileName);
			throw uex;
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
			pProgressStatus, pOCRParameters);
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
	IProgressStatus* pProgressStatus, IOCRParameters* pOCRParameters, ISpatialString* *pstrText)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		Win32CriticalSectionLockGuard lg(m_cs);

		// validate the retval
		ASSERT_ARGUMENT("ELI18186", pstrText != __nullptr);
		
		// validate the license
		validateLicense();

		// Reset m_bKilledOcrDoNotRetry before checking/initializing the OCREngine since
		// WhackOCREngine can otherwise be called after the OCR engine is initialized but before
		// m_bKilledOcrDoNotRetry is reset.
		{
			Win32CriticalSectionLockGuard lg2(m_csKillingOCR);

			// Reset the killed OCR flag to false
			m_bKilledOcrDoNotRetry = false;
		}

		checkOCREngine();

		// Get the image file name and check for .uss extension [FlexIDSCore #3242]
		string stdstrImageFileName = asString(strImageFileName);
		if (getExtensionFromFullPath(stdstrImageFileName, true) == ".uss")
		{
			UCLIDException uex("ELI24866", "Cannot OCR a '.uss' file!");
			uex.addDebugInfo("File To OCR", stdstrImageFileName);
			throw uex;
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
				bDetectHandwriting, bReturnUnrecognized, bReturnSpatialInfo, pProgressStatus, pOCRParameters);
		}
		else
		{
			// only one decomposition method is necessary
			ipRecognizedText = recognizePrintedTextInImageZone(strImageFileName, lStartPage, 
				lEndPage, pZone, nRotationInDegrees, eFilter, bstrCustomFilterCharacters, 
				bReturnUnrecognized, bReturnSpatialInfo, pProgressStatus, pOCRParameters);
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
STDMETHODIMP CScansoftOCR::raw_CreateOutputImage(BSTR bstrImageFileName, BSTR bstrFormat, BSTR bstrOutputFileName,
	IOCRParameters* pOCRParameters)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());
	
	try
	{

		IScansoftOCR2Ptr ipOcrEngine = getOCREngine();
		ASSERT_RESOURCE_ALLOCATION("ELI46464", ipOcrEngine != __nullptr);

		// Set output format so that all settings are available for SetOCRParameters
		ipOcrEngine->SetOutputFormat(bstrFormat);

		// Set the parameters (either from registry or parameters object)
		// Re-apply the settings in case they have changed since the engine was created
		ipOcrEngine->SetOCRParameters(pOCRParameters, VARIANT_TRUE);

		ipOcrEngine->CreateOutputImage(bstrImageFileName, bstrFormat, bstrOutputFileName);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI46465");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CScansoftOCR::raw_GetPDFImage(BSTR bstrFileName, int nPage, VARIANT* pImageData)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		try
		{
			try
			{
				// increment # of images processed
				InterlockedIncrement(&m_ulNumImagesProcessed);
				
				// https://extract.atlassian.net/browse/ISSUE-16861
				// checkOCREngine is purposely not called upfront as this was added to retrieve images
				// for the AppBackend API under a heavy load with as little risk as possible to triggering
				// nuance license errors. Whether or not it would risk inducing those errors, checkOCREngine
				// is additional overhead and involves searches for Windows not applicable running as a web
				// service.
				// See catch handler below.

				IScansoftOCR2Ptr ipOcrEngine = getOCREngine();
				ASSERT_RESOURCE_ALLOCATION("ELI49592", ipOcrEngine != __nullptr);

				_variant_t vtImageData = ipOcrEngine->GetPDFImage(bstrFileName, nPage);

				VariantCopy(pImageData, &vtImageData);

				return S_OK;
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI49615");
		}
		catch (UCLIDException &ue)
		{
			try
			{
				// In the event of an error, force a recycle of the OCR engine that should trigger
				// a reset of the nuance license manager if needed.
				if (m_pid > 0)
				{
					ue.addDebugInfo("ProcessKill", "Attempting OCR process recycle");
					killOCREngine();
				}
			}
			catch (...) { }

			throw ue;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI49593");
}

//-------------------------------------------------------------------------------------------------
// IImageFormatConverter
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CScansoftOCR::raw_ConvertImage(
		BSTR inputFileName,
		BSTR outputFileName,
		ImageFormatConverterFileType outputType,
		VARIANT_BOOL preserveColor,
		BSTR pagesToRemove,
		ImageFormatConverterNuanceFormat explicitFormat,
		long compressionLevel)
{
	try
	{
		// increment # of images processed
		InterlockedIncrement(&m_ulNumImagesProcessed);

		retryOnNLSFailure("convert image",
			[&]() -> void
			{
				getImageFormatConverter()->ConvertImage(
					inputFileName,
					outputFileName,
					outputType,
					preserveColor,
					pagesToRemove,
					explicitFormat,
					compressionLevel);
			},
			"ELI53650",
			"ELI53651");

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI53537");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CScansoftOCR::raw_ConvertImagePage(
		BSTR inputFileName,
		BSTR outputFileName,
		ImageFormatConverterFileType outputType,
		VARIANT_BOOL preserveColor,
		long page,
		ImageFormatConverterNuanceFormat explicitFormat,
		long compressionLevel)
{
	try
	{
		// increment # of images processed
		InterlockedIncrement(&m_ulNumImagesProcessed);

		retryOnNLSFailure("convert image page",
			[&]() -> void
			{
				getImageFormatConverter()->ConvertImagePage(
					inputFileName,
					outputFileName,
					outputType,
					preserveColor,
					page,
					explicitFormat,
					compressionLevel);
			},
			"ELI53652",
			"ELI53653");

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI53538");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CScansoftOCR::raw_CreateSearchablePdf(
	BSTR inputFileName,
	BSTR outputFileName,
	VARIANT_BOOL deleteOriginal,
	VARIANT_BOOL outputPdfA,
	BSTR userPassword,
	BSTR ownerPassword,
	VARIANT_BOOL passwordsAreEncrypted,
	long permissions)
{
	try
	{
		// increment # of images processed
		InterlockedIncrement(&m_ulNumImagesProcessed);

		retryOnNLSFailure("create searchable PDF",
			[&]() -> void
			{
				IImageFormatConverterPtr converter = getImageFormatConverter();

				// Set the parameters (defaults and from registry)
				// Re-apply the settings in case they have changed since the engine was created
				IScansoftOCR2Ptr ssocr2 = converter;
				ssocr2->SetOCRParameters(__nullptr, VARIANT_TRUE);

				converter->CreateSearchablePdf(
					inputFileName,
					outputFileName,
					deleteOriginal,
					outputPdfA,
					userPassword,
					ownerPassword,
					passwordsAreEncrypted,
					permissions);
			},
			"ELI53654",
			"ELI53655");

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI53538");
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
void CScansoftOCR::resetNuanceLicensing()
{
	try
	{
		// Since this will stop and restart the Nuance licensing service, ensure that only one
		// instance of this method is running at once on this machine.
		unique_ptr<CMutex> pMutex;
		pMutex.reset(getGlobalNamedMutex(gstr_NUANCE_LICENSE_RESET_MUTEX_NAME));
		ASSERT_RESOURCE_ALLOCATION("ELI35319", pMutex.get() != __nullptr);
		CSingleLock lock(pMutex.get(), TRUE);

		// Before attempting to reset, make sure other instance of resetNuanceLicensing didn't
		// already correct the situation. Otherwise, this would shutdown the licensing service for
		// the first thread after it had already repaired it.
		m_ipOCREngine.CreateInstance(CLSID_ScansoftOCR2);
		if (m_ipOCREngine != __nullptr)
		{
			return;
		}

		UCLIDException("ELI35316", "Application trace: Failed to initialize OCR engine; "
			"attempting reset of Nuance licensing.").log();

		string strCommonComponents = getModuleDirectory("BaseUtils.dll");
		string strParams = "/C \"" + strCommonComponents + "\\RepairNuanceLicensing.bat\" nopause";

		SHELLEXECUTEINFO shellExecInfo = {0};
		shellExecInfo.cbSize = sizeof(SHELLEXECUTEINFO);
		shellExecInfo.fMask = SEE_MASK_NOCLOSEPROCESS;
		shellExecInfo.hwnd = __nullptr;
		shellExecInfo.lpVerb = "open";
		shellExecInfo.lpFile = "cmd.exe";		
		shellExecInfo.lpParameters = strParams.c_str();	
		shellExecInfo.lpDirectory = strCommonComponents.c_str();
		shellExecInfo.nShow = SW_HIDE;
		shellExecInfo.hInstApp = __nullptr;	

		if (!asCppBool(ShellExecuteEx(&shellExecInfo)))
		{
			UCLIDException ue("ELI35320", "Nuance license repair utility failed.");
			ue.addDebugInfo("Return code", (int)shellExecInfo.hInstApp);
			throw ue;
		}

		if (WaitForSingleObject(shellExecInfo.hProcess, 20000) != 0)
		{
			UCLIDException ue("ELI35321", "Nuance license repair utility timeout.");
			throw ue;
		}

		// Make sure the licenses have had a chance to initialize before continuing.
		Util::retry(5, "initialize OCR engine after Nuance licensing reset",
			[&]() -> bool
			{
				m_ipOCREngine.CreateInstance(CLSID_ScansoftOCR2);
				return m_ipOCREngine != __nullptr;
			},
			[&](int tries) -> void { Sleep(tries * 1000); },
			"ELI35317");

		UCLIDException("ELI35318", "Application trace: Nuance licensing reset successful.").log();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI35322");
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

		if (m_ipOCREngine == __nullptr)
		{
			// Will re-attempt instantiating m_ipOCREngine as well.
			resetNuanceLicensing();
		}

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
	CSingleLock guard(&gGetMaxRecognitionsPerOCREngineInstanceMutex, TRUE);

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

	// Cleanup temporary data files
	recursiveRemoveDirectory(getTemporaryDataFolder(m_pid));

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
		// [FlexIDSCore:5043]
		// Log the exception that was caught unless the OCR engine was whacked (in which case the
		// error isn't important to the caller and it's likely the error itself was caused by the
		// whack call).
		if (!m_bKilledOcrDoNotRetry)
		{
			uex.log();
		}

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
	IProgressStatus* pProgressStatus, IOCRParameters* pOCRParameters)
{
	// increment # of images processed
	InterlockedIncrement(&m_ulNumImagesProcessed);

	// create auto-pointer for progress status update thread manager
	unique_ptr<PSUpdateThreadManager> apPSUpdateThread;

	// initialize stream to hold OCR results
	_bstr_t _bstrStream;

	// prepare decomposition methods for OCR loop
	EPageDecompositionMethod eDecompositionMethod[3];

	IScansoftOCR2Ptr ipOcrEngine = getOCREngine();
	ASSERT_RESOURCE_ALLOCATION("ELI49963", ipOcrEngine != __nullptr);

	// Set the parameters (either from registry or parameters object)
	// Re-apply the settings in case they have changed since the engine was created
	ipOcrEngine->SetOCRParameters(pOCRParameters, VARIANT_TRUE);

	// get the primary decomposition method from the OCR engine
	eDecompositionMethod[0] = ipOcrEngine->GetPrimaryDecompositionMethod();

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

	// iterate through different decomposition methods until one works. Also retry licensing failures
	// TODO: It appears that SSOCR2 already tries all the same decomp methods per-page in the case of
	// failure so trying them here is mostly a waste of CPU time.
	// This loop should be changed to only retry in the case of a license service failure.
	int decompMethodIdx = 0;
	int licenseErrors = 0;
	int tooManyLicenseErrors = 5;
	while (decompMethodIdx < iNumDecompositionMethods && licenseErrors < tooManyLicenseErrors)
	{
		try
		{
			// Get the OCR engine
			ipOcrEngine = getOCREngine();
			ASSERT_RESOURCE_ALLOCATION("ELI25216", ipOcrEngine != __nullptr);

			// Set the parameters if needed (either from registry or parameters object)
			ipOcrEngine->SetOCRParameters(pOCRParameters, VARIANT_FALSE);

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
				asVariantBool(pProgressStatus != __nullptr), eDecompositionMethod[decompMethodIdx]);

			// check if the progress status update thread still exists
			if (!killer.killedProcess() && apPSUpdateThread.get() != __nullptr)
			{
				// mark the progress status as complete
				apPSUpdateThread->notifyOCRComplete();
			}

			// OCR was successful, don't try again
			break;
		}
		catch (...)
		{
			UCLIDException ue = uex::fromCurrent("ELI11095");

			// NOTE: Check for manual killing of OCR engine first so that no exception
			// is logged indicating that the decomposition method failed.
			// Scope for critical section
			{
				Win32CriticalSectionLockGuard lg(m_csKillingOCR);

				if (m_bKilledOcrDoNotRetry)
				{
					// If OCR was killed, just return an empty spatial string
					ISpatialStringPtr ipSS(CLSID_SpatialString);
					return ipSS;
				}
			}

			bool licenseServiceFailed = isExceptionFromNLSFailure(ue);

			// Create an exception message specifying the method used, if applicable
			string strTopLevelText;
			if (licenseServiceFailed)
			{
				strTopLevelText = "Application trace: Unable to OCR document because of a licensing problem";
			}
			else if (eDecompositionMethod[decompMethodIdx] == kAutoDecomposition)
			{
				strTopLevelText = "Application trace: Unable to OCR document with automatic decomposition attempt.";
			}
			else if (eDecompositionMethod[decompMethodIdx] == kLegacyDecomposition)
			{
				strTopLevelText = "Application trace: Unable to OCR document with legacy decomposition attempt.";
			}
			else
			{
				strTopLevelText = "Application trace: Unable to OCR document with standard decomposition attempt.";
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

			// Setup the next loop
			if (licenseServiceFailed)
			{
				licenseErrors++;
			}
			else
			{
				decompMethodIdx++;
			}
		}
	}

	// Throw the aggregate exception if all decomposition methods failed.
	if (decompMethodIdx == iNumDecompositionMethods)
	{
		apAggregateException.reset(new UCLIDException("ELI25300",
			"Failure to OCR image. All decomposition methods failed.", *apAggregateException));

		apAggregateException->addDebugInfo("Failed Image", asString(strImageFileName));
		throw *apAggregateException;
	}
	// Throw the aggregate exception if all retries failed
	else if (licenseErrors == tooManyLicenseErrors)
	{
		apAggregateException.reset(new UCLIDException("ELI53535",
			"Failed to OCR image after retries", *apAggregateException));

		apAggregateException->addDebugInfo("Failed Image", asString(strImageFileName));
		throw *apAggregateException;
	}
	// ...or log the aggregate exception if at least one attempt failed before another succeeded.
	else if (decompMethodIdx > 0)
	{
		apAggregateException.reset(new UCLIDException("ELI29014",
			"Application trace: At least one OCR decomposition method failed.",
			*apAggregateException));

		apAggregateException->addDebugInfo("Image", asString(strImageFileName));
		apAggregateException->log();
	}

	// get the spatial string from the stream
	IPersistStreamPtr ipObj;
	readObjectFromBSTR(ipObj, _bstrStream);
	ASSERT_RESOURCE_ALLOCATION("ELI18352", ipObj != __nullptr);

	// Attach the passed in parameters to the spatial string
	// https://extract.atlassian.net/browse/ISSUE-15562
	if (pOCRParameters != __nullptr)
	{
		IHasOCRParametersPtr ipHasOCRParameters(ipObj);
		ASSERT_RESOURCE_ALLOCATION("ELI46179", ipHasOCRParameters != __nullptr);
		ipHasOCRParameters->OCRParameters = pOCRParameters;
	}

	// return the spatial string
	return ipObj;
}
//-------------------------------------------------------------------------------------------------
ISpatialStringPtr CScansoftOCR::recognizePrintedTextInImageZone(BSTR strImageFileName, 
	long lStartPage, long lEndPage, ILongRectangle* pZone, long nRotationInDegrees, 
	EFilterCharacters eFilter, BSTR bstrCustomFilterCharacters, VARIANT_BOOL bReturnUnrecognized, 
	VARIANT_BOOL bReturnSpatialInfo, IProgressStatus* pProgressStatus, IOCRParameters* pOCRParameters)
{
	// increment # of images processed
	InterlockedIncrement(&m_ulNumImagesProcessed);

	// create a variant vector array using the specified page number
	IVariantVectorPtr ipPageNumbers = createPageNumberVector(strImageFileName, lStartPage, lEndPage);
	ASSERT_RESOURCE_ALLOCATION("ELI18062", ipPageNumbers != __nullptr);

	// Get the OCR engine
	IScansoftOCR2Ptr ipOcrEngine = getOCREngine();
	ASSERT_RESOURCE_ALLOCATION("ELI25217", ipOcrEngine != __nullptr);

	ipOcrEngine->SetOCRParameters(pOCRParameters, VARIANT_TRUE);

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

	// Attach the passed in parameters to the spatial string
	// https://extract.atlassian.net/browse/ISSUE-15562
	if (pOCRParameters != __nullptr)
	{
		IHasOCRParametersPtr ipHasOCRParameters(ipStream);
		ASSERT_RESOURCE_ALLOCATION("ELI46180", ipHasOCRParameters != __nullptr);
		ipHasOCRParameters->OCRParameters = pOCRParameters;
	}

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
IImageFormatConverterPtr CScansoftOCR::getImageFormatConverter()
{
	IImageFormatConverterPtr ipImageFormatConverter(getOCREngine());
	ASSERT_RESOURCE_ALLOCATION("ELI53533", ipImageFormatConverter != __nullptr);

	return ipImageFormatConverter;
}
//-------------------------------------------------------------------------------------------------
void CScansoftOCR::retryOnNLSFailure(const string& description, function<void()> func, const string& eliCodeForRetry, const string& eliCodeForFailure)
{
	Util::conditionalRetry(5, func,
		[&](int tries) -> bool
		{
			UCLIDException inner = uex::fromCurrent("ELI53656");
			if (isExceptionFromNLSFailure(inner))
			{
				UCLIDException outer(eliCodeForRetry, "Application trace: Failed to " + description + ". Retrying...", inner);
				outer.addDebugInfo("Attempt", tries);
				outer.log();

				// If there was an NLS failure then killing the OCR engine will trigger a reset when it is recreated
				killOCREngine();

				return true;
			}

			return false;
		},
		[&]() -> void
		{
			UCLIDException ue = uex::fromCurrent("ELI53657");
			throw UCLIDException(eliCodeForFailure, "Failed to " + description, ue);
		});
}
//-------------------------------------------------------------------------------------------------
