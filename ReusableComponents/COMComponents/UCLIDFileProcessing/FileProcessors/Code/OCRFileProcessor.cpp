// OCRFileProcessor.cpp : Implementation of COCRFileProcessor
#include "stdafx.h"
#include "FileProcessors.h"
#include "OCRFileProcessor.h"
#include "FileProcessorsUtils.h"

#include <UCLIDException.h>
#include <TemporaryFileName.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <TemporaryResourceOverride.h>
#include <misc.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>
#include <Win32Semaphore.h>
#include <MiscLeadUtils.h>
#include <UPI.h>
#include <RegistryPersistenceMgr.h>
#include <RegConstants.h>
#include <StringTokenizer.h>

#include <io.h>
#include <cmath>
#include <strstream>
#include <fstream>
#include <string>

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

// global variables / static variables / externs
extern CComModule _Module;
//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 5;

// string for the MessageBox that will be displayed if opening an Object from a pre 6.0
// release of the software
const string gstrVERSION5_OCR_WARNING = "As of release 6.0 all tasks following the OCR document task "
										"will be\npassed the original source document name, not "
										"<SourceDocName>.uss.\nIf you have $FileNoExtOf(<SourceDocName>) "
										"in one of your tasks to\nstrip the .uss from a file after "
										"OCRing, you must remove this from\nyour task.";

// string for the title of the MessageBox that will be displayed if opening an Object from
// a pre 6.0 release of the software
const string gstrVERSION5_OCR_WARNING_LABEL = "Important Change To OCR Document Task!";

// Constant used to limit the number of work items that will be added for a work group at a time.
const int gnMAX_WORK_ITEMS_TO_ADD_PER_CALL = 25;

// Constant used to limit the number of work items that are retrieved in a group when stitching the 
// output file
const int gnMAX_WORK_ITEMS_TO_GET_PER_CALL = 100;

// Default constants for the OCR
const string& gstrDEFAULT_SKIP_PAGE_ON_FAILURE = "0";
const string& gstrDEFAULT_MAX_OCR_PAGE_FAILURE_PERCENTAGE = "25";
const string& gstrDEFAULT_MAX_OCR_PAGE_FAILURE_NUMBER = "10";

const long gnPAGES_PER_WORK_ITEM = 5;

const string& gstrCOMPONENT_DESCRIPTION = "Core: OCR document";

//--------------------------------------------------------------------------------------------------
// COCRFileProcessor
//--------------------------------------------------------------------------------------------------
COCRFileProcessor::COCRFileProcessor()
: m_bDirty(false),
  m_eOCRPageRangeType(UCLID_FILEPROCESSORSLib::kOCRAll),
  m_bUseCleanedImageIfAvailable(true),
  m_strSpecificPages(""),
  m_bParallelize(false),
  m_bAllowCancelBeforeComplete(false),
  m_CancelEvent(),
  m_ipMiscUtils(__nullptr),
  m_bSkipPageOnFailure(gstrDEFAULT_SKIP_PAGE_ON_FAILURE == "1"),
  m_uiMaxOcrPageFailureNumber(asUnsignedLong(gstrDEFAULT_MAX_OCR_PAGE_FAILURE_NUMBER)),
  m_uiMaxOcrPageFailurePercentage(asUnsignedLong(gstrDEFAULT_MAX_OCR_PAGE_FAILURE_PERCENTAGE)),
  m_ipOCRParameters(__nullptr),
  m_bLoadOCRParametersFromRuleset(false),
  m_strOCRParametersRulesetName("")
{
	try
	{
		m_strComputerName = getComputerName();

		// Create the ruleset class using the prog id to avoid circular dependancy
		m_ipLoadOCRParameters.m_obj.CreateInstance("UCLIDAFCore.Ruleset");
		ASSERT_RESOURCE_ALLOCATION( "ELI45960", m_ipLoadOCRParameters.m_obj != __nullptr );
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI11043")
}
//--------------------------------------------------------------------------------------------------
COCRFileProcessor::~COCRFileProcessor()
{
	try
	{
		m_ipMiscUtils = __nullptr;
		m_ipLoadOCRParameters.Clear();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI11044")
}

//--------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ILicensedComponent,
		&IID_ICategorizedComponent,
		&IID_IFileProcessingTask,
		&IID_ICopyableObject,
		&IID_IPersistStream,
		&IID_IMustBeConfiguredObject,
		&IID_IOCRFileProcessor,
		&IID_IAccessRequired,
		&IID_IParallelizableTask,
		&IID_IIdentifiableObject,
		&IID_IHasOCRParameters
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	if (pbValue == NULL)
		return E_POINTER;

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

	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// IFileProcessingTask
//--------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::raw_Init(long nActionID, IFAMTagManager* pFAMTM,
	IFileProcessingDB *pDB, IFileRequestHandler* pFileRequestHandler)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		// reset the cancel event
		m_CancelEvent.reset();

		// https://extract.atlassian.net/browse/ISSUE-13214
		// If a FAM DB is not available, parallelization will not occur; the
		// AllowRestartableProcessing setting is therefore irrelevant, so avoid checking it in order
		// to allow this task to be used without a database.
		IFileProcessingDBPtr ipDB(pDB);
		if (ipDB != __nullptr)
		{
			_bstr_t bstrAllowRestartable = ipDB->GetDBInfoSetting("AllowRestartableProcessing", VARIANT_FALSE);
			m_bAllowCancelBeforeComplete = asString(bstrAllowRestartable) == "1";
		}
		
		getOCRSettings();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17786");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::raw_ProcessFile(IFileRecord* pFileRecord, long nActionID,
	IFAMTagManager *pTagManager, IFileProcessingDB *pDB, IProgressStatus *pProgressStatus,
	VARIANT_BOOL bCancelRequested, EFileProcessingResult *pResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI17923", pResult != __nullptr);
		IFileRecordPtr ipFileRecord(pFileRecord);
		ASSERT_ARGUMENT("ELI31586", ipFileRecord != __nullptr);

		// If cancel has been signaled before processing file and it is allowed exit with canceled status
		if (m_bAllowCancelBeforeComplete && bCancelRequested == VARIANT_TRUE)
		{
			*pResult = kProcessingCancelled;
			return S_OK;
		}
		string strInputFileName = asString(ipFileRecord->Name);

		// Default to successful completion
		*pResult = kProcessingSuccessful;

		// verify the file existence
		::validateFileOrFolderExistence(strInputFileName);

		// append .uss to the image file name
		string strOutputFileName = strInputFileName + ".uss";

		ISpatialStringPtr ipSS = __nullptr;
		bool bHasNoSpatialPages = false;
		
		EFileType eFileType = getFileType(strInputFileName);
		if (eFileType == kTXTFile || eFileType == kXMLFile || eFileType == kCSVFile)
		{
			// If a text file, load as "indexed" text.
			ipSS.CreateInstance(CLSID_SpatialString);
			ASSERT_RESOURCE_ALLOCATION("ELI31683", ipSS != __nullptr);

			ipSS->LoadFrom(strInputFileName.c_str(), VARIANT_FALSE);
		}
		else if (pDB == __nullptr || !m_bParallelize || ipFileRecord->Pages <= gnPAGES_PER_WORK_ITEM)
		{
			// get the image to OCR
			string strImageToOcr = m_bUseCleanedImageIfAvailable ?
				getCleanImageNameIfExists(strInputFileName) : strInputFileName;

			// get the OCR parameters from file if specified
			if (m_bLoadOCRParametersFromRuleset)
			{
				IFAMTagManagerPtr ipTagMgr(pTagManager);
				ASSERT_RESOURCE_ALLOCATION("ELI45876", ipTagMgr != __nullptr);

				loadOCRParameters(ipTagMgr, strInputFileName);
			}

			// based on the type
			switch (m_eOCRPageRangeType)
			{
			case UCLID_FILEPROCESSORSLib::kOCRAll:
				{
					ipSS = getOCREngine()->RecognizeTextInImage(strImageToOcr.c_str(), 
						1, -1, UCLID_RASTERANDOCRMGMTLib::kNoFilter, "", 
						UCLID_RASTERANDOCRMGMTLib::kRegistry, VARIANT_TRUE, pProgressStatus, getOCRParameters());
				}
				break;
			case UCLID_FILEPROCESSORSLib::kOCRSpecifiedPages:
				{
					ipSS = getOCREngine()->RecognizeTextInImage2(strImageToOcr.c_str(), 
						get_bstr_t(m_strSpecificPages), VARIANT_TRUE, pProgressStatus, getOCRParameters());
				}
				break;
			default:
				THROW_LOGIC_ERROR_EXCEPTION("ELI28161");
				break;
			}
			ASSERT_RESOURCE_ALLOCATION("ELI28150", ipSS != __nullptr);

			// Ensure source doc name is original image file if a clean image was used
			if (m_bUseCleanedImageIfAvailable)
			{
				ipSS->SourceDocName = strInputFileName.c_str();
			}
		}
		else
		{
			IFileProcessingDBPtr ipDB(pDB);
			ASSERT_RESOURCE_ALLOCATION("ELI36925", ipDB != __nullptr);

			IProgressStatusPtr ipProgressStatus(pProgressStatus);
			
			// Build the progress description string
			string strProgressDescription;
			if (ipProgressStatus != __nullptr)
			{
				string strTmp = asString(ipProgressStatus->Text);
				if (!strTmp.empty())
				{
					strProgressDescription = strTmp + " from " + m_strComputerName;
				}
			}
			else
			{
				strProgressDescription = gstrCOMPONENT_DESCRIPTION + " from " + m_strComputerName;
			}
			
			// Create the work items
			long nWorkGroup = createWorkItems(nActionID, ipFileRecord, ipDB, strProgressDescription);
			
			EFileProcessingResult processingResult = 
				waitForWorkToComplete(ipProgressStatus, ipDB, nWorkGroup, strInputFileName);

			if (processingResult == kProcessingCancelled )
			{
				*pResult = processingResult;
				return S_OK;
			}

			// Stitch the results
			bHasNoSpatialPages = !stitchWorkItems(strInputFileName, strOutputFileName, nWorkGroup, ipDB);

			if (ipProgressStatus != NULL)
			{
				ipProgressStatus->CompleteCurrentItemGroup();
			}
		}
		if (ipSS != __nullptr && ipSS->Size == 0 || ipSS == __nullptr && bHasNoSpatialPages)
		{
			UCLIDException ue("ELI34137", "Application trace: OCR output is blank");
			ue.addDebugInfo("File", strInputFileName);
			ue.log();
		}

		// OutputFileName = InputFileName.uss even if cleaned image was used
		// (NOTE: stitchWorkItems saves the string itself)
		if (ipSS != __nullptr)
		{
			ipSS->SaveTo(strOutputFileName.c_str(), VARIANT_TRUE, VARIANT_TRUE);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11047")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::raw_Cancel()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// if allowed to cancel then signal the cancel event
		if (m_bAllowCancelBeforeComplete)
		{
			m_CancelEvent.signal();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17785");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::raw_Close()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		// Free the OCR engine from memory
		m_ipOCREngine = __nullptr;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11139");
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::raw_Standby(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{		
		ASSERT_ARGUMENT("ELI33910", pVal != __nullptr);

		*pVal = VARIANT_TRUE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33911");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::get_MinStackSize(unsigned long *pnMinStackSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI35017", pnMinStackSize != __nullptr);

		validateLicense();

		*pnMinStackSize = 0;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI35018");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::get_DisplaysUI(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI44983", pVal != __nullptr);

		validateLicense();
		
		*pVal = VARIANT_FALSE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI44984");
}

//-------------------------------------------------------------------------------------------------
// IAccessRequired interface implementation
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::raw_RequiresAdminAccess(VARIANT_BOOL* pbResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI31189", pbResult != __nullptr);

		*pbResult = VARIANT_FALSE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31190");
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::raw_GetComponentDescription(BSTR * pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19614", pstrComponentDescription != __nullptr);

		*pstrComponentDescription = _bstr_t(gstrCOMPONENT_DESCRIPTION.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11031")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_FILEPROCESSORSLib::IOCRFileProcessorPtr ipCopyThis(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI11032", ipCopyThis != __nullptr);
		
		// copy data members
		m_eOCRPageRangeType = (UCLID_FILEPROCESSORSLib::EOCRFPPageRangeType)ipCopyThis->OCRPageRangeType;
		m_strSpecificPages = ipCopyThis->SpecificPages;
		m_bUseCleanedImageIfAvailable = asCppBool(ipCopyThis->UseCleanedImage);

		IParallelizableTaskPtr ipCopyParallizableTask(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI36852", ipCopyParallizableTask != __nullptr);

		m_bParallelize = asCppBool(ipCopyParallizableTask->Parallelize);

		m_bLoadOCRParametersFromRuleset = asCppBool(ipCopyThis->LoadOCRParametersFromRuleset);
		m_strOCRParametersRulesetName = ipCopyThis->OCRParametersRulesetName;

		// Copy OCR parameters
		IHasOCRParametersPtr ipOCRParams(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI45869", ipOCRParams != __nullptr);
		ICopyableObjectPtr ipMap = ipOCRParams->OCRParameters;
		ASSERT_RESOURCE_ALLOCATION("ELI45870", ipMap != __nullptr);
		m_ipOCRParameters = ipMap->Clone();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI12810");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance(CLSID_OCRFileProcessor);
		ASSERT_RESOURCE_ALLOCATION("ELI11033", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11034");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_OCRFileProcessor;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
// Version 3: added ability to use cleaned image if available
STDMETHODIMP COCRFileProcessor::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// reset member variables
		m_eOCRPageRangeType = UCLID_FILEPROCESSORSLib::kOCRAll;
		m_strSpecificPages = "";
		m_bUseCleanedImageIfAvailable = false;
		m_bParallelize = false;

		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read(&nDataLength, sizeof(nDataLength), NULL);
		ByteStream data(nDataLength);
		pStream->Read(data.getData(), nDataLength, NULL);
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);

		// Read the individual data items from the bytestream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		// Check for newer version
		if (nDataVersion > gnCurrentVersion)
		{
			// Throw exception
			UCLIDException ue("ELI11035", "Unable to load newer OCRFileProcessor component.");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}
		
		// check to see if the object we are processing was created with a 
		// pre 6.0 release of the software. if it is, we set bNewDirtyFlag
		// to true
		bool bNewDirtyFlag = false;
		if (nDataVersion == 1)
		{
			// display a message box with our warning message and title and the icon set
			// to warning
			MessageBox(NULL, gstrVERSION5_OCR_WARNING.c_str(), gstrVERSION5_OCR_WARNING_LABEL.c_str(),
				MB_ICONWARNING | MB_OK);
			
			// change the bNewDirtyFlag to true because we are processing an old version of
			// the object
			bNewDirtyFlag = true;
		}

		// load data members from stream
		long tmp;
		dataReader >> tmp;
		m_eOCRPageRangeType = (UCLID_FILEPROCESSORSLib::EOCRFPPageRangeType)tmp;
		dataReader >> m_strSpecificPages;
		
		// if version 3 and higher then read m_bUseCleanedImageIfAvailable
		if (nDataVersion >= 3)
		{
			dataReader >> m_bUseCleanedImageIfAvailable;
		}

		if (nDataVersion >=4 )
		{
			dataReader >> m_bParallelize;
		}

		// Read the OCR parameter settings
		if (nDataVersion >= 5)
		{
			dataReader >> m_bLoadOCRParametersFromRuleset;
			dataReader >> m_strOCRParametersRulesetName;
		}

		if (nDataVersion >=4 )
		{
			loadGUID(pStream);
		}

		// Read the OCR parameters
		if (nDataVersion >= 5 && !m_bLoadOCRParametersFromRuleset)
		{
			IPersistStreamPtr ipObj;

			::readObjectFromStream(ipObj, pStream, "ELI45859");
			ASSERT_RESOURCE_ALLOCATION("ELI45860", ipObj != __nullptr);
			m_ipOCRParameters = ipObj;
		}

		// set the dirty flag
		m_bDirty = bNewDirtyFlag;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11036");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		// write data members to stream
		dataWriter << gnCurrentVersion;
		dataWriter << (long)m_eOCRPageRangeType;
		dataWriter << m_strSpecificPages;
		dataWriter << m_bUseCleanedImageIfAvailable;
		dataWriter << m_bParallelize;
		dataWriter << m_bLoadOCRParametersFromRuleset;
		dataWriter << m_strOCRParametersRulesetName;

		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		// Save the GUID for the IIdentifiableObject interface.
		saveGUID(pStream);

		if (!m_bLoadOCRParametersFromRuleset)
		{
			IPersistStreamPtr ipPIObj = getOCRParameters();
			ASSERT_RESOURCE_ALLOCATION("ELI45884", ipPIObj != __nullptr);
			writeObjectToStream(ipPIObj, pStream, "ELI45885", fClearDirty);
		}

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10250");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::raw_IsConfigured(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		bool bConfigured = true;
		if (m_eOCRPageRangeType == UCLID_FILEPROCESSORSLib::kOCRSpecifiedPages && m_strSpecificPages.empty())
		{
			bConfigured = false;
		}

		*pbValue = asVariantBool(bConfigured);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11037");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IOCRFileProcessor
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::get_OCRPageRangeType(EOCRFPPageRangeType *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		*pVal = (EOCRFPPageRangeType)m_eOCRPageRangeType;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11038");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::put_OCRPageRangeType(EOCRFPPageRangeType newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		if (m_eOCRPageRangeType != (UCLID_FILEPROCESSORSLib::EOCRFPPageRangeType)newVal)
		{
			m_eOCRPageRangeType = (UCLID_FILEPROCESSORSLib::EOCRFPPageRangeType)newVal;
			m_bDirty = true;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11039");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::get_SpecificPages(BSTR *strSpecificPages)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		*strSpecificPages = _bstr_t(m_strSpecificPages.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11040");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::put_SpecificPages(BSTR strSpecificPages)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		// validate the string first
		string strDefinedPageNums = asString(strSpecificPages);
		::validatePageNumbers(strDefinedPageNums);

		if (strDefinedPageNums != m_strSpecificPages)
		{
			m_strSpecificPages = strDefinedPageNums;
			m_bDirty = true;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11041");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::get_UseCleanedImage(VARIANT_BOOL *pbUseCleaned)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		*pbUseCleaned = asVariantBool(m_bUseCleanedImageIfAvailable);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17458");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::put_UseCleanedImage(VARIANT_BOOL bUseCleaned)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		if (m_bUseCleanedImageIfAvailable != asCppBool(bUseCleaned))
		{
			m_bUseCleanedImageIfAvailable = asCppBool(bUseCleaned);
			m_bDirty = true;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17459");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::get_LoadOCRParametersFromRuleset(VARIANT_BOOL *pbLoadOCRParametersFromRuleset)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		*pbLoadOCRParametersFromRuleset = asVariantBool(m_bLoadOCRParametersFromRuleset);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI45933");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::put_LoadOCRParametersFromRuleset(VARIANT_BOOL bLoadOCRParametersFromRuleset)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		if (m_bLoadOCRParametersFromRuleset != asCppBool(bLoadOCRParametersFromRuleset))
		{
			m_bLoadOCRParametersFromRuleset = asCppBool(bLoadOCRParametersFromRuleset);
			m_bDirty = true;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI45934");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::get_OCRParametersRulesetName(BSTR *strOCRParametersRulesetName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		*strOCRParametersRulesetName = get_bstr_t(m_strOCRParametersRulesetName).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI45935");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::put_OCRParametersRulesetName(BSTR strOCRParametersRulesetName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		string strNewValue = asString(strOCRParametersRulesetName);

		if (strNewValue != m_strOCRParametersRulesetName)
		{
			m_strOCRParametersRulesetName = strNewValue;
			m_bDirty = true;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI45936");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IParallelizableTask Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::raw_ProcessWorkItem(IWorkItemRecord *pWorkItem, long nActionID,
		IFAMTagManager* pFAMTM, IFileProcessingDB* pDB, IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();
			
		IWorkItemRecordPtr ipWorkItem(pWorkItem);
		ASSERT_RESOURCE_ALLOCATION("ELI36854", ipWorkItem != __nullptr);

		IFileProcessingDBPtr ipDB(pDB);
		ASSERT_RESOURCE_ALLOCATION("ELI37074", ipDB != __nullptr);

		IFAMTagManagerPtr ipFAMTM(pFAMTM);
		ASSERT_RESOURCE_ALLOCATION("ELI37145", ipFAMTM != __nullptr);

		string strOriginalInput = ipWorkItem->Input;
		string strInput;
		vector<long> vecPages;
		parseOCRInputText(strOriginalInput, strInput, vecPages);

		// Changing this to ExpandTagsAndFunctions because it seems like the right thing to do
		// (I don't have a specific case where this was a problem before...)
		string strInputFileName = asString(ipFAMTM->ExpandTagsAndFunctions(get_bstr_t(strInput), ipWorkItem->FileName));

		// get the image to OCR
		string strImageToOcr = m_bUseCleanedImageIfAvailable ?
			getCleanImageNameIfExists(strInputFileName) : strInputFileName;

		// get the OCR parameters from file if specified
		if (m_bLoadOCRParametersFromRuleset)
		{
			loadOCRParameters(ipFAMTM, strInputFileName);
		}

		ISpatialStringPtr ipSS;
		
		// Initialize the progress for the workitem
		pProgressStatus->InitProgressStatus(pWorkItem->RunningTaskDescription,0, 1, VARIANT_TRUE);		
		
		if (m_eOCRPageRangeType == UCLID_FILEPROCESSORSLib::kOCRAll)
		{
			// make sure there are 2 pages in vecPages
			if (vecPages.size() != 2)
			{
				UCLIDException ueAll("ELI37795", "No Page specified.");
				throw ueAll;
			}

			// Add pages to the work item progress description
			string strTaskDescription = pWorkItem->RunningTaskDescription;
			if (vecPages[0] != vecPages[1])
			{
				strTaskDescription = strTaskDescription + " Pages " + asString(vecPages[0]) + 
					"-" + asString(vecPages[1]);
			}
			else
			{
				strTaskDescription = strTaskDescription + " Page" + asString(vecPages[0]);
			}
		
			pProgressStatus->StartNextItemGroup(strTaskDescription.c_str(),1);

			// process the 2 pages in the vecpages vector as start and end
			ipSS = getOCREngine()->RecognizeTextInImage(strImageToOcr.c_str(), 
				vecPages[0], vecPages[1], UCLID_RASTERANDOCRMGMTLib::kNoFilter, "", 
				UCLID_RASTERANDOCRMGMTLib::kRegistry, VARIANT_TRUE, pProgressStatus->SubProgressStatus,
				getOCRParameters());
		}
		else if (m_eOCRPageRangeType == UCLID_FILEPROCESSORSLib::kOCRSpecifiedPages)
		{
			// Will need to join each of the strings that are returned so need an IUnknownVector
			IIUnknownVectorPtr ipPages(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI37796", ipPages != __nullptr);
			
			// Initialize the progress for the workitem
			pProgressStatus->InitProgressStatus(pWorkItem->RunningTaskDescription,0, 
				vecPages.size(), VARIANT_TRUE);	
			
			// Add pages to the work item progress description
			string strPages = (vecPages.size() > 1) ? " Pages ": " Page ";
			for (size_t i = 0; i < vecPages.size(); i++)
			{
				strPages = strPages + ((i == 0) ? " " : ", ") + asString(vecPages[i]);
			}

			string strTaskDescription = asString(pWorkItem->RunningTaskDescription) + strPages;
			for (size_t i = 0;i < vecPages.size(); i++)
			{
				pProgressStatus->StartNextItemGroup(strTaskDescription.c_str(),1);
				ISpatialStringPtr ipTempSS = getOCREngine()->RecognizeTextInImage(strImageToOcr.c_str(), 
					vecPages[i], vecPages[i], UCLID_RASTERANDOCRMGMTLib::kNoFilter, "", 
					UCLID_RASTERANDOCRMGMTLib::kRegistry, VARIANT_TRUE, pProgressStatus->SubProgressStatus,
					getOCRParameters());
				pProgressStatus->CompleteProgressItems(strTaskDescription.c_str(), 1 );
				ipPages->PushBack(ipTempSS);
			}

			// Create the joint string
			ipSS.CreateInstance(CLSID_SpatialString);
			ipSS->CreateFromSpatialStrings(ipPages);
			
		}
		ASSERT_RESOURCE_ALLOCATION("ELI36856", ipSS != __nullptr);

		// Ensure source doc name is original image file if a clean image was used
		if (m_bUseCleanedImageIfAvailable)
		{
			ipSS->SourceDocName = strInputFileName.c_str();
		}
		
		// Save output to the database
		IPersistStreamPtr ipPersistObj = ipSS;
		ipDB->SaveWorkItemBinaryOutput(ipWorkItem->WorkItemID, ipPersistObj);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36853");	
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::get_Parallelize(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		*pVal = asVariantBool(m_bParallelize);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36850");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::put_Parallelize(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		if (m_bParallelize != asCppBool(newVal))
		{
			m_bParallelize = asCppBool(newVal);
			m_bDirty = true;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36849");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::get_InstanceGUID(GUID * pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		*pVal = getGUID();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37154");
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void COCRFileProcessor::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnOCR_ON_SERVER_FEATURE;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI11042", "OCR File Processor");
}
//-------------------------------------------------------------------------------------------------
IOCREnginePtr COCRFileProcessor::getOCREngine()
{
	if (m_ipOCREngine == __nullptr)
	{
		m_ipOCREngine.CreateInstance(CLSID_ScansoftOCR);
		ASSERT_RESOURCE_ALLOCATION("ELI11048", m_ipOCREngine != __nullptr);

		_bstr_t _bstrPrivateLicenseCode = get_bstr_t(LICENSE_MGMT_PASSWORD.c_str());
		IPrivateLicensedComponentPtr ipScansoftEngine(m_ipOCREngine);
		ipScansoftEngine->InitPrivateLicense( _bstrPrivateLicenseCode );
	}
	return m_ipOCREngine;
}
//-------------------------------------------------------------------------------------------------
long COCRFileProcessor::createWorkItems(long nActionID, IFileRecordPtr ipFileRecord, IFileProcessingDBPtr ipDB,
	const string &strProgressDescription)
{
	try
	{
		string strFileName = asString(ipFileRecord->Name);

		// vecPages will be set to have all the page numbers to be processed
		// if not doing all of the pages
		vector<int> vecPages;
		long nPagesToProcess = determinePagesToProcess(strFileName, vecPages);

		IIUnknownVectorPtr ipWorkItemsToAdd(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI36910", ipWorkItemsToAdd != __nullptr);
		
		// Get info needed for the work item group
		string strUPI = UPI::getCurrentProcessUPI().getUPI();
		IFileProcessingTaskPtr ipTask(this);
		_bstr_t bstrStringized = getMiscUtils()->GetObjectAsStringizedByteStream(ipTask);
		
		long nPagesForLastWorkItem = nPagesToProcess % gnPAGES_PER_WORK_ITEM;
		long nNumberOfWorkItems = nPagesToProcess / gnPAGES_PER_WORK_ITEM;
		nNumberOfWorkItems += (nPagesForLastWorkItem >0) ? 1:0;

		// Check if work item group already exists
		long nWorkItemGroupID = 0;
		
		// Only look for an existing work item group if this can be canceled
		if (m_bAllowCancelBeforeComplete)
		{
			nWorkItemGroupID = ipDB->FindWorkItemGroup(ipFileRecord->FileID, nActionID, bstrStringized, 
				nNumberOfWorkItems, strProgressDescription.c_str());
		}

		// The record already exists so return the group id
		if (nWorkItemGroupID != 0)
		{
			return nWorkItemGroupID;
		}

		switch (m_eOCRPageRangeType)
		{
		case UCLID_FILEPROCESSORSLib::kOCRAll:
			{
				// Create the work item group
				nWorkItemGroupID = ipDB->CreateWorkItemGroup(ipFileRecord->FileID, nActionID, 
					bstrStringized, nNumberOfWorkItems, strProgressDescription.c_str()); 
				for (long i = 0; i < nNumberOfWorkItems; i++)
				{
					string strPages;

					long nStartPage = i * gnPAGES_PER_WORK_ITEM + 1;
					long nEndPage = nStartPage -1;
					if (i < (nNumberOfWorkItems -1) || nPagesForLastWorkItem == 0)
					{
						nEndPage += gnPAGES_PER_WORK_ITEM;
					}
					else
					{ 
						nEndPage += nPagesForLastWorkItem;
					}
					strPages = asString(nStartPage) + "-" + asString(nEndPage);

					// Add the page to the work items to add list
					ipWorkItemsToAdd->PushBack(createWorkItem("<SourceDocName>", strPages));

					// Limit the number of work items that are added in one call
					if (((i + 1) % gnMAX_WORK_ITEMS_TO_ADD_PER_CALL) == 0)
					{
						ipDB->AddWorkItems(nWorkItemGroupID, ipWorkItemsToAdd);
						ipWorkItemsToAdd->Clear();
					}
				}
			}
			break;
		case UCLID_FILEPROCESSORSLib::kOCRSpecifiedPages:
			{
				// Create the work item group
				nWorkItemGroupID = ipDB->CreateWorkItemGroup(ipFileRecord->FileID, nActionID, 
					bstrStringized, nNumberOfWorkItems, strProgressDescription.c_str()); 

				string strPages = "";

				int i = 0;

				// Add the work items to the group
				for (;i < nPagesToProcess; i++ )
				{
					if (i % gnPAGES_PER_WORK_ITEM == 0)
					{
						strPages = asString(vecPages[i]);
					}
					else
					{
						strPages += "|" + asString(vecPages[i]);
					}
					
					int nextI = i + 1;
					if ((nextI % gnPAGES_PER_WORK_ITEM) == 0 || nextI >= nPagesToProcess)
					{
						// Add the page to the work items to add list
						ipWorkItemsToAdd->PushBack(createWorkItem("<SourceDocName>", strPages));	

						// Limit the number of work items that are added in one call
						if (((i + 1) % gnMAX_WORK_ITEMS_TO_ADD_PER_CALL) == 0)
						{
							ipDB->AddWorkItems(nWorkItemGroupID, ipWorkItemsToAdd);
							ipWorkItemsToAdd->Clear();
						}
					}
				}
			}
			break;
		default:
			THROW_LOGIC_ERROR_EXCEPTION("ELI36917");
			break;
		}

		// Add any remaining work items
		if (ipWorkItemsToAdd->Size() > 0)
		{
			ipDB->AddWorkItems(nWorkItemGroupID, ipWorkItemsToAdd);
		}
		return nWorkItemGroupID;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI36912");
}
//-------------------------------------------------------------------------------------------------
bool COCRFileProcessor::stitchWorkItems(const string &strInputFile, const string & strOutputFile, long nWorkItemGroupID, IFileProcessingDBPtr ipDB)
{
	try
	{
		IIUnknownVectorPtr ipWorkItems;

		// Setup the aggregate exception to use if pages fail
		bool bFailedWorkItem = false;
		UCLIDException aggregateException("ELI37552", "Application trace: At least one page of document failed to OCR.");
		string strPages = "";

		// Get lists of the workitems in small groups
		long nStart = 0;
		bool bDone = false;
		bool hasAtLeastOnePage = false;
		while (!bDone)
		{
			ipWorkItems = ipDB->GetWorkItemsForGroup(nWorkItemGroupID, nStart, gnMAX_WORK_ITEMS_TO_GET_PER_CALL);
			ASSERT_RESOURCE_ALLOCATION("ELI36857", ipWorkItems != __nullptr);
			
			nStart += gnMAX_WORK_ITEMS_TO_GET_PER_CALL;

			size_t nWorkItems = ipWorkItems->Size();
			for  (size_t i = 0; i != nWorkItems; i++ )
			{
				IWorkItemRecordPtr ipWorkItem = ipWorkItems->At(i);
				ASSERT_RESOURCE_ALLOCATION("ELI36859", ipWorkItem != __nullptr);

				UCLID_FILEPROCESSINGLib::EWorkItemStatus eStatus = ipWorkItem->Status;

				// if the status is failed add its info to the aggregate exception and continue with
				// next work item
				if (eStatus == kWorkUnitFailed)
				{
					UCLIDException workItemException;
					workItemException.createFromString("ELI37553", 
						asString(ipWorkItem->GetStringizedException()), false, false);
					aggregateException.addDebugInfo("Exception History", workItemException);
					aggregateException.addDebugInfo("WorkItemInput", asString( ipWorkItem->Input));
					bFailedWorkItem = true;
					continue;
				}

				// Get the output from the WorkItem record
				ISpatialStringPtr ipOutput;
				ipOutput = ipWorkItem->BinaryOutput;
				ASSERT_RESOURCE_ALLOCATION("ELI37084", ipOutput != __nullptr);

				// Prevent blank pages from getting passed to CreateFromSpatialStrings
				// https://extract.atlassian.net/browse/ISSUE-12448
				if (ipOutput->GetMode() == kSpatialMode)
				{
					ipOutput->SourceDocName = strInputFile.c_str();
					IHasOCRParametersPtr ipHasOCRParameters(ipOutput);
					ASSERT_RESOURCE_ALLOCATION("ELI46181", ipHasOCRParameters != __nullptr);
					ipHasOCRParameters->OCRParameters = getOCRParameters();

					if (hasAtLeastOnePage)
					{
						ipOutput->AppendToFile(strOutputFile.c_str());
					}
					else
					{
						hasAtLeastOnePage = true;
						ipOutput->SaveTo(strOutputFile.c_str(), VARIANT_TRUE, VARIANT_FALSE);
					}
				}
			}
			bDone = nWorkItems < gnMAX_WORK_ITEMS_TO_GET_PER_CALL;
		}

		// Log the aggregate exception if any pages failed
		if (bFailedWorkItem)
		{
			aggregateException.addDebugInfo("Pages that failed", strPages);
			aggregateException.log();
		}

		// If no page was recognized, write out an empty spatial string so that there will be a USS file whenever the OCR task succeeds
		if (!hasAtLeastOnePage)
		{
			ISpatialStringPtr ipOutput(CLSID_SpatialString);
			ASSERT_RESOURCE_ALLOCATION("ELI36860", ipOutput != __nullptr);

			IHasOCRParametersPtr ipHasOCRParameters(ipOutput);
			ASSERT_RESOURCE_ALLOCATION("ELI46763", ipHasOCRParameters != __nullptr);
			ipHasOCRParameters->OCRParameters = getOCRParameters();

			ipOutput->SaveTo(strOutputFile.c_str(), VARIANT_TRUE, VARIANT_FALSE);
		}

		return hasAtLeastOnePage;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI37133");
}
//-------------------------------------------------------------------------------------------------
IMiscUtilsPtr COCRFileProcessor::getMiscUtils()
{
	if (m_ipMiscUtils == __nullptr)
	{
		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI37035", m_ipMiscUtils != __nullptr);
	}
	return m_ipMiscUtils;
}
//-------------------------------------------------------------------------------------------------
IWorkItemRecordPtr COCRFileProcessor::createWorkItem(string strFileName, string strPages)
{
	IWorkItemRecordPtr ipWorkItem(CLSID_WorkItemRecord);
	ASSERT_RESOURCE_ALLOCATION("ELI36911", ipWorkItem != __nullptr);

	ipWorkItem->Status = kWorkUnitPending;
	strFileName += "|" + strPages;
	ipWorkItem->Input = strFileName.c_str();

	return ipWorkItem;
}
//-------------------------------------------------------------------------------------------------
EFileProcessingResult COCRFileProcessor::waitForWorkToComplete(IProgressStatusPtr ipProgressStatus,
	IFileProcessingDBPtr ipDB, long nWorkGroup, const string &strInputFileName)
{
	// Semaphore is assumed to be acquired for the thread before the call
	// to ProcessFile
	UPI upi = UPI::getCurrentProcessUPI();

	Win32Semaphore semaphore(upi.getProcessSemaphoreName());

	WorkItemGroupStatus wigsStatusRecord;
	
	long nTotalWorkItems = 0 ;
	EWorkItemStatus eAllWorkItemStatus = ipDB->GetWorkItemGroupStatus(nWorkGroup, &wigsStatusRecord);

	if (ipProgressStatus != NULL)
	{
		nTotalWorkItems = wigsStatusRecord.lTotal + 1/* for stitching*/;

		ipProgressStatus->InitProgressStatus("Initializing OCR....",0, nTotalWorkItems, VARIANT_FALSE);
	}

	long lLastProcessed = 0;

	// Wait for them to finish
	while (eAllWorkItemStatus != kWorkUnitComplete && eAllWorkItemStatus != kWorkUnitFailed)
	{
		// this is a polling method and is not the most efficient

		// Release the semaphore so other work can be done while waiting
		{
			Win32SemaphoreUnLockGuard unlockGuard(semaphore);

			if (m_CancelEvent.wait(200) != WAIT_TIMEOUT)
			{
				// May need to save state here
				return kProcessingCancelled;
			}
		}
		eAllWorkItemStatus = ipDB->GetWorkItemGroupStatus(nWorkGroup, &wigsStatusRecord);

		long nTotalProcessedOrFailed = wigsStatusRecord.lCompletedCount + wigsStatusRecord.lFailedCount;
		if (ipProgressStatus != NULL)
		{	
			if (eAllWorkItemStatus == kWorkUnitPending)
			{
				// Still waiting to start OCRing so turn/keep timer off
				ipProgressStatus->StopProgressTimer();
			}
			else
			{
				// No longer waiting for processor time so make sure timer is running
				ipProgressStatus->StartProgressTimer();
			}

			if (nTotalProcessedOrFailed != lLastProcessed)
			{
				string strText = "OCR'd " + asString(nTotalProcessedOrFailed) + " of " + asString(wigsStatusRecord.lTotal) + " page(s)";
				ipProgressStatus->CompleteProgressItems(strText.c_str(), nTotalProcessedOrFailed - lLastProcessed);
				lLastProcessed = nTotalProcessedOrFailed;
			}
		}

		if (eAllWorkItemStatus == kWorkUnitFailed)
		{
			unsigned long totalCount = wigsStatusRecord.lCompletedCount + wigsStatusRecord.lFailedCount;
			if (!m_bSkipPageOnFailure ||  
				(m_bSkipPageOnFailure && (wigsStatusRecord.lFailedCount > (long)m_uiMaxOcrPageFailureNumber ||
				wigsStatusRecord.lFailedCount *100.0 / totalCount > (double)m_uiMaxOcrPageFailurePercentage)))
			{

				// Make sure progress timer is running
				if (ipProgressStatus != NULL)
				{	
					ipProgressStatus->StartProgressTimer();
				}

				// need to aggregate the exceptions from the database

				UCLIDException ue("ELI36861", "Failed to OCR Document.");
				ue.addDebugInfo("FileName", strInputFileName);
				ue.addDebugInfo("Total Pages", totalCount);
				ue.addDebugInfo("Failed Pages", wigsStatusRecord.lFailedCount);

				throw aggregateWorkItemExceptions(ipDB, nWorkGroup, ue);
			}
		}
	}

	// Make sure progress timer is running
	if (ipProgressStatus != NULL)
	{	
		ipProgressStatus->StartProgressTimer();
	}

	return kProcessingSuccessful;
}
//-------------------------------------------------------------------------------------------------
UCLIDException & COCRFileProcessor::aggregateWorkItemExceptions(IFileProcessingDBPtr ipDB, 
	long nWorkItemGroupID, UCLIDException &exception)
{
	IIUnknownVectorPtr ipWorkItems = ipDB->GetFailedWorkItemsForGroup(nWorkItemGroupID);
	ASSERT_RESOURCE_ALLOCATION("ELI37550", ipWorkItems != __nullptr);

	long nNumberOfItems = ipWorkItems->Size();
	for (long i = 0; i < nNumberOfItems; i++)
	{
		IWorkItemRecordPtr ipWorkItem = ipWorkItems->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI37560", ipWorkItem != __nullptr);

		// Get the exception
		UCLIDException ue;
		ue.createFromString("ELI37551", asString(ipWorkItem->GetStringizedException()), false, false);
		exception.addDebugInfo("Exception History", ue);
	}
	return exception;
}
//-------------------------------------------------------------------------------------------------
void COCRFileProcessor::getOCRSettings()
{
	// Setup registry manager to OCREngine\SSOCR key
	RegistryPersistenceMgr regMgr(HKEY_CURRENT_USER, gstrREG_ROOT_KEY + "\\OCREngine\\SSOCR");

	string strValue;

	// Get value of SkipPageOnFailure key
	strValue = regMgr.getKeyValue("", "SkipPageOnFailure", gstrDEFAULT_SKIP_PAGE_ON_FAILURE);
	m_bSkipPageOnFailure = strValue == "1";

	// Get value of MaxOcrPageFailureNumber key
	strValue = regMgr.getKeyValue("", "MaxOcrPageFailureNumber", gstrDEFAULT_MAX_OCR_PAGE_FAILURE_NUMBER);
	m_uiMaxOcrPageFailureNumber = asUnsignedLong(strValue);

	// Get value of MaxOcrPageFailurePercentage key
	strValue = regMgr.getKeyValue("", "MaxOcrPageFailurePercentage", gstrDEFAULT_MAX_OCR_PAGE_FAILURE_PERCENTAGE);
	m_uiMaxOcrPageFailurePercentage = asUnsignedLong(strValue);
}
//-------------------------------------------------------------------------------------------------
long COCRFileProcessor::determinePagesToProcess(const string& strFileName, vector<int>& vecPages)
{
	// Get the total number of pages in the image
	long nPagesToProcess = getNumberOfPagesInImage(strFileName);

	// If only processing specified pages, fill the vecPages vector with the pages
	if (m_eOCRPageRangeType == UCLID_FILEPROCESSORSLib::kOCRSpecifiedPages)
	{
		vecPages = getPageNumbers(nPagesToProcess, m_strSpecificPages, false);
		nPagesToProcess = vecPages.size();
	}
	return nPagesToProcess;

}
//-------------------------------------------------------------------------------------------------
void COCRFileProcessor::parseOCRInputText(const string& strInputText, string& strFileName, vector<long>& vecPages)
{
	vector<string> vecTokens;
	StringTokenizer::sGetTokens(strInputText, "|", vecTokens);

	if (vecTokens.size() < 1)
	{
		UCLIDException ue("ELI37793", "WorkItem input not correct format.");
		ue.addDebugInfo("Expected format 1", "<FileName with tags>| <page range e.g. 1-4>");
		ue.addDebugInfo("Expected format 2", "<FileName with tags>| <pages e.g. 1|3|4>");
		ue.addDebugInfo("Input", strInputText);
		throw ue;
	}

	// if there is only one token - assume old format
	if (vecTokens.size() == 1)
	{
		long nPos = vecTokens[0].find_last_of('.');
		strFileName = vecTokens[0].substr(0, nPos - 1);
		long nPage = asLong(vecTokens[0].substr(nPos + 1,string::npos));
		vecPages.push_back(nPage);
		vecPages.push_back(nPage);
	}
	else if (vecTokens.size() == 2)
	{
		strFileName = vecTokens[0];
		vector<string> vecStrPages;
		StringTokenizer::sGetTokens(vecTokens[1],"-", vecStrPages);

		// should be at least 1 tokens
		if (vecStrPages.size() < 1)
		{
			UCLIDException ue("ELI37794", "WorkItem input not correct format.");
			ue.addDebugInfo("Expected format 1", "<FileName with tags>| <page range e.g. 1-4>");
			ue.addDebugInfo("Expected format 2", "<FileName with tags>| <pages e.g. 1|3|4>");
			ue.addDebugInfo("Input", strInputText);
			throw ue;
		}
		vecPages.push_back(asLong(vecStrPages[0]));

		if (vecStrPages.size() > 1)
		{
			vecPages.push_back(asLong(vecStrPages[1]));
		}
	}
	else
	{
		strFileName = vecTokens[0];
		for (size_t  i = 1; i < vecTokens.size(); i++)
		{
			vecPages.push_back(asLong(vecTokens[i]));
		}
	}
}
//-------------------------------------------------------------------------------------------------
// IHasOCRParameters
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::get_OCRParameters(IOCRParameters** ppOCRParameters)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		*ppOCRParameters = getOCRParameters().Detach();
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI45866");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRFileProcessor::put_OCRParameters(IOCRParameters* pOCRParameters)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_ipOCRParameters = pOCRParameters;
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI45868");
}
//-------------------------------------------------------------------------------------------------
IOCRParametersPtr COCRFileProcessor::getOCRParameters()
{
	if (m_ipOCRParameters == __nullptr)
	{
		m_ipOCRParameters.CreateInstance(CLSID_VariantVector);

		ASSERT_RESOURCE_ALLOCATION("ELI45871", m_ipOCRParameters != __nullptr);
	}

	return m_ipOCRParameters;
}
//-------------------------------------------------------------------------------------------------
void COCRFileProcessor::loadOCRParameters(IFAMTagManagerPtr ipTagMgr, string strSourceDocName)
{
	if (!m_strOCRParametersRulesetName.empty())
	{
		string strRulesetPath =
			ipTagMgr->ExpandTagsAndFunctions(get_bstr_t(m_strOCRParametersRulesetName), get_bstr_t(strSourceDocName));
		
		ASSERT_RUNTIME_CONDITION("ELI45867", isValidFile(strRulesetPath), "Missing OCR parameters file");

		m_ipLoadOCRParameters.loadObjectFromFile(strRulesetPath);

		IHasOCRParametersPtr ipHasOCRParameters(m_ipLoadOCRParameters.m_obj);
		m_ipOCRParameters = ipHasOCRParameters->OCRParameters;
	}
}
//-------------------------------------------------------------------------------------------------