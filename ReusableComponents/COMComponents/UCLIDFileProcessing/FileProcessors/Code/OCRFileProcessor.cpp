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
const unsigned long gnCurrentVersion = 4;

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
  m_ipMiscUtils(__nullptr)
{
	try
	{
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI11043")
}
//--------------------------------------------------------------------------------------------------
COCRFileProcessor::~COCRFileProcessor()
{
	try
	{
		m_ipMiscUtils = __nullptr;
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
		&IID_IIdentifiableObject
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
	IFileProcessingDB *pDB)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		// reset the cancel event
		m_CancelEvent.reset();

		_bstr_t bstrAllowRestartable = pDB->GetDBInfoSetting("AllowRestartableProcessing", VARIANT_FALSE);
		m_bAllowCancelBeforeComplete = asString(bstrAllowRestartable) == "1";
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

		string strInputFileName = asString(ipFileRecord->Name);

		// Default to successful completion
		*pResult = kProcessingSuccessful;

		// verify the file existence
		::validateFileOrFolderExistence(strInputFileName);

		// append .uss to the image file name
		string strOutputFileName = strInputFileName + ".uss";

		ISpatialStringPtr ipSS = __nullptr;
		
		EFileType eFileType = getFileType(strInputFileName);
		if (eFileType == kTXTFile || eFileType == kXMLFile || eFileType == kCSVFile)
		{
			// If a text file, load as "indexed" text.
			ipSS.CreateInstance(CLSID_SpatialString);
			ASSERT_RESOURCE_ALLOCATION("ELI31683", ipSS != __nullptr);

			ipSS->LoadFrom(strInputFileName.c_str(), VARIANT_FALSE);
		}
		else if (pDB == __nullptr || !m_bParallelize || ipFileRecord->Pages < 3 )
		{
			// get the image to OCR
			string strImageToOcr = m_bUseCleanedImageIfAvailable ?
				getCleanImageNameIfExists(strInputFileName) : strInputFileName;

			// based on the type
			switch (m_eOCRPageRangeType)
			{
			case UCLID_FILEPROCESSORSLib::kOCRAll:
				{
					ipSS = getOCREngine()->RecognizeTextInImage(strImageToOcr.c_str(), 
						1, -1, UCLID_RASTERANDOCRMGMTLib::kNoFilter, "", 
						UCLID_RASTERANDOCRMGMTLib::kRegistry, VARIANT_TRUE, pProgressStatus);
				}
				break;
			case UCLID_FILEPROCESSORSLib::kOCRSpecifiedPages:
				{
					ipSS = getOCREngine()->RecognizeTextInImage2(strImageToOcr.c_str(), 
						get_bstr_t(m_strSpecificPages), VARIANT_TRUE, pProgressStatus);
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

			// Create the work items
			long nWorkGroup = createWorkItems(nActionID, ipFileRecord, ipDB);
			
			IProgressStatusPtr ipProgressStatus(pProgressStatus);
			EWorkItemStatus eGroupStatus;
			EFileProcessingResult processingResult = 
				waitForWorkToComplete(ipProgressStatus, ipDB, nWorkGroup, strInputFileName);

			if (processingResult == kProcessingCancelled )
			{
				*pResult = processingResult;
				return S_OK;
			}

			// Stitch the results
			ipSS = stitchWorkItems(strInputFileName ,nWorkGroup, ipDB);

			if (ipProgressStatus != NULL)
			{
				ipProgressStatus->CompleteCurrentItemGroup();
			}

			if (ipSS == NULL)
			{
				*pResult = kProcessingCancelled;
				return S_OK;
			}
		}
		if (ipSS->Size == 0)
		{
			UCLIDException ue("ELI34137", "Application trace: OCR output is blank");
			ue.addDebugInfo("File", strInputFileName);
			ue.log();
		}

		// OutputFileName = InputFileName.uss even if cleaned image was used
		ipSS->SaveTo(strOutputFileName.c_str(), VARIANT_TRUE, VARIANT_TRUE);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11047")

	return S_OK;
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

		*pstrComponentDescription = _bstr_t("Core: OCR document").Detach();
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

			loadGUID(pStream);
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

		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		// Save the GUID for the IIdentifiableObject interface.
		saveGUID(pStream);

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

		string strInput = asString(ipFAMTM->ExpandTags(ipWorkItem->Input, ipWorkItem->FileName));

		string strInputFileName = getPathAndFileNameWithoutExtension(strInput);
		string strExt = getExtensionFromFullPath(strInput);
		
		if (strExt.empty() )
		{
			UCLIDException ue ("ELI36855", "Invalid input for WorkItem.");
			ue.addDebugInfo("Input", strInput);
			throw ue;
		}

		long nPageNumber = asLong(getExtensionFromFullPath(strInput).substr(1,string::npos));

		// get the image to OCR
		string strImageToOcr = m_bUseCleanedImageIfAvailable ?
			getCleanImageNameIfExists(strInputFileName) : strInputFileName;

		ISpatialStringPtr ipSS = getOCREngine()->RecognizeTextInImage(strImageToOcr.c_str(), 
			nPageNumber, nPageNumber, UCLID_RASTERANDOCRMGMTLib::kNoFilter, "", 
			UCLID_RASTERANDOCRMGMTLib::kRegistry, VARIANT_TRUE, pProgressStatus);
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
long COCRFileProcessor::createWorkItems(long nActionID, IFileRecordPtr ipFileRecord, IFileProcessingDBPtr ipDB)
{
	try
	{
		string strFileName = asString(ipFileRecord->Name);

		// Get the total number of pages in the image
		long nTotalPages = getNumberOfPagesInImage(strFileName);

		IIUnknownVectorPtr ipWorkItemsToAdd(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI36910", ipWorkItemsToAdd != __nullptr);
		
		// Get info needed for the work item group
		string strUPI = UPI::getCurrentProcessUPI().getUPI();
		IFileProcessingTaskPtr ipTask(this);
		_bstr_t bstrStringized = getMiscUtils()->GetObjectAsStringizedByteStream(ipTask);
		
		// Check if work item group already exists
		long nWorkItemGroupID = ipDB->FindWorkItemGroup(ipFileRecord->FileID, nActionID, bstrStringized, nTotalPages);

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
				nWorkItemGroupID = ipDB->CreateWorkItemGroup(ipFileRecord->FileID, nActionID, bstrStringized, nTotalPages); 
				for (long i = 1; i <= nTotalPages; i++)
				{
					// Add the page to the work items to add list
					ipWorkItemsToAdd->PushBack(createWorkItem("<SourceDocName>", i));

					// Limit the number of work items that are added in one call
					if (i % gnMAX_WORK_ITEMS_TO_ADD_PER_CALL == 0)
					{
						ipDB->AddWorkItems(nWorkItemGroupID, ipWorkItemsToAdd);
						ipWorkItemsToAdd->Clear();
					}
				}
			}
			break;
		case UCLID_FILEPROCESSORSLib::kOCRSpecifiedPages:
			{
				// Get the pages to be processed
				vector<int> vecPages = getPageNumbers(nTotalPages, m_strSpecificPages, false);

				// Get the total number of pages to be processed
				int numberOfPages = vecPages.size();

				// Create the work item group
				nWorkItemGroupID = ipDB->CreateWorkItemGroup(ipFileRecord->FileID, nActionID, bstrStringized, numberOfPages); 

				// Add the work items to the group
				for (int i = 0; i < numberOfPages; i++)
				{
					// Add the page to the work items to add list
					ipWorkItemsToAdd->PushBack(createWorkItem("<SourceDocName>", vecPages[i]));	

					// Limit the number of work items that are added in one call
					if ((i + 1) % gnMAX_WORK_ITEMS_TO_ADD_PER_CALL == 0)
					{
						ipDB->AddWorkItems(nWorkItemGroupID, ipWorkItemsToAdd);
						ipWorkItemsToAdd->Clear();
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
ISpatialStringPtr COCRFileProcessor::stitchWorkItems(const string &strInputFile, long nWorkItemGroupID, IFileProcessingDBPtr ipDB)
{
	try
	{
		IIUnknownVectorPtr ipWorkItems;

		IIUnknownVectorPtr ipPagesToStitch(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI36858", ipPagesToStitch != __nullptr);

		ISpatialStringPtr ipSS(CLSID_SpatialString);
		ASSERT_RESOURCE_ALLOCATION("ELI36860", ipSS != __nullptr);

		// Get lists of the workitems in small groups
		long nStart = 0;
		bool bDone = false;
		while (!bDone)
		{
			ipWorkItems = ipDB->GetWorkItemsForGroup(nWorkItemGroupID, nStart, gnMAX_WORK_ITEMS_TO_GET_PER_CALL);
			ASSERT_RESOURCE_ALLOCATION("ELI36857", ipWorkItems != __nullptr);
			
			nStart += gnMAX_WORK_ITEMS_TO_GET_PER_CALL;

			long nPageNumber;
			long nWorkItems = ipWorkItems->Size();
			for  (long i = 0; i != nWorkItems; i++ )
			{
				IWorkItemRecordPtr ipWorkItem = ipWorkItems->At(i);
				ASSERT_RESOURCE_ALLOCATION("ELI36859", ipWorkItem != __nullptr);

				// Get the output from the WorkItem record
				ISpatialStringPtr ipOutput = ipWorkItem->BinaryOutput;
				ASSERT_RESOURCE_ALLOCATION("ELI37084", ipOutput != __nullptr);

				// Get the page number
				string strExt = getExtensionFromFullPath(asString(ipWorkItem->Input));
				nPageNumber = asLong(strExt.substr(1));

				ipOutput->SourceDocName = strInputFile.c_str();

				ipPagesToStitch->PushBack(ipOutput);
			}
			bDone = nWorkItems < gnMAX_WORK_ITEMS_TO_GET_PER_CALL;
		}
		ipSS->CreateFromSpatialStrings(ipPagesToStitch);

		return ipSS;
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
IWorkItemRecordPtr COCRFileProcessor::createWorkItem(string strFileName, int iPageNumber)
{
	IWorkItemRecordPtr ipWorkItem(CLSID_WorkItemRecord);
	ASSERT_RESOURCE_ALLOCATION("ELI36911", ipWorkItem != __nullptr);

	ipWorkItem->Status = kWorkUnitPending;
	strFileName += "." + asString(iPageNumber);
	ipWorkItem->Input = strFileName.c_str();

	return ipWorkItem;
}
//-------------------------------------------------------------------------------------------------
EFileProcessingResult COCRFileProcessor::waitForWorkToComplete(IProgressStatusPtr ipProgressStatus,
	IFileProcessingDBPtr ipDB, long nWorkGroup, const string &strInputFileName)
{
	// Semaphore is assumed to be aquired for the thread before the call
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
	while (eAllWorkItemStatus != kWorkUnitComplete)
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
			if (nTotalProcessedOrFailed != lLastProcessed)
			{
				string strText = "OCR'd " + asString(nTotalProcessedOrFailed) + " of " + asString(wigsStatusRecord.lTotal) + " page(s)";
				ipProgressStatus->CompleteProgressItems(strText.c_str(), nTotalProcessedOrFailed - lLastProcessed);
				lLastProcessed = nTotalProcessedOrFailed;
			}
		}
		if (eAllWorkItemStatus == kWorkUnitFailed)
		{
			UCLIDException ue("ELI36861", "Work group processing failed.");
			ue.addDebugInfo("FileName", strInputFileName);
			throw ue;
		}
	}
	return kProcessingSuccessful;
}
//-------------------------------------------------------------------------------------------------
