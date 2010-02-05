
#include "stdafx.h"
#include "SetActionStatusFileProcessor.h"
#include "FileProcessorsUtils.h"

#include <UCLIDException.h>
#include <COMUtils.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
static const long gnCURRENT_VERSION = 1;

//--------------------------------------------------------------------------------------------------
// CSetActionStatusFileProcessor
//--------------------------------------------------------------------------------------------------
CSetActionStatusFileProcessor::CSetActionStatusFileProcessor()
:m_bDirty(false), m_eActionStatus(kActionPending), m_strActionName("")
{
	try
	{
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI15104");
}
//--------------------------------------------------------------------------------------------------
CSetActionStatusFileProcessor::~CSetActionStatusFileProcessor()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI15105")
}
//--------------------------------------------------------------------------------------------------
HRESULT CSetActionStatusFileProcessor::FinalConstruct()
{
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
void CSetActionStatusFileProcessor::FinalRelease()
{
}

//--------------------------------------------------------------------------------------------------
// ISupportsErrorInfo interface
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSetActionStatusFileProcessor::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ISetActionStatusFileProcessor,
		&IID_ILicensedComponent,
		&IID_ICategorizedComponent,
		&IID_IMustBeConfiguredObject,
		&IID_ICopyableObject,
		&IID_IPersistStream,
		&IID_IFileProcessingTask
	};

	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//--------------------------------------------------------------------------------------------------
// ISetActionStatusFileProcessor
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSetActionStatusFileProcessor::get_ActionName(BSTR *pbstrRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		*pbstrRetVal = _bstr_t(m_strActionName.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15133");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSetActionStatusFileProcessor::put_ActionName(BSTR bstrNewVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try 
	{
		// Check license
		validateLicense();

		// verify validity of action name
		string strNewVal = asString(bstrNewVal);
		if (strNewVal.empty())
		{
			UCLIDException ue("ELI15135", "Action name cannot be empty!");
			throw ue;
		}

		// update action name
		m_strActionName = strNewVal;

		// set dirty flag to true
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15134");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSetActionStatusFileProcessor::get_ActionStatus(long *pRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		*pRetVal = (long) m_eActionStatus;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15136");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSetActionStatusFileProcessor::put_ActionStatus(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try 
	{
		// Check license
		validateLicense();

		// verify validity of action status
		if (newVal != kActionUnattempted && newVal != kActionPending &&
			newVal != kActionCompleted && newVal != kActionFailed && newVal != kActionSkipped)
		{
			UCLIDException ue("ELI15137", "Specified action status is not valid!");
			ue.addDebugInfo("newVal", newVal);
			throw ue;
		}

		// update action status
		m_eActionStatus = (EActionStatus) newVal;

		// set dirty flag to true
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15138");

	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// IFileProcessingTask
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSetActionStatusFileProcessor::raw_Init(long nActionID, IFAMTagManager* pFAMTM,
	IFileProcessingDB *pDB)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		// nothing to do
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15118");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSetActionStatusFileProcessor::raw_ProcessFile(BSTR bstrFileFullName, long nFileID,
	long nActionID, IFAMTagManager *pTagManager, IFileProcessingDB *pDB,
	IProgressStatus *pProgressStatus, VARIANT_BOOL bCancelRequested, EFileProcessingResult *pResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI17924", bstrFileFullName != NULL);
		ASSERT_ARGUMENT("ELI17925", asString(bstrFileFullName).empty() == false);
		ASSERT_ARGUMENT("ELI17927", pResult != NULL);
		
		// wrap the database pointer in a smart pointer object
		IFileProcessingDBPtr ipDB(pDB);
		ASSERT_RESOURCE_ALLOCATION("ELI15146", ipDB != NULL);

		IFAMTagManagerPtr ipTagManager(pTagManager);
		ASSERT_RESOURCE_ALLOCATION("ELI29126", ipTagManager != NULL);

		// Default to successful completion
		*pResult = kProcessingSuccessful;

		// Expand action name
		string strActionName = CFileProcessorsUtils::ExpandTagsAndTFE(ipTagManager, 
			m_strActionName, asString(bstrFileFullName));

		// Auto create if necessary
		ipDB->AutoCreateAction(strActionName.c_str());

		EActionStatus ePrevStatus = kActionUnattempted;

		// If nFileID == -1 then the file does not exist so add it to the database
		if (nFileID == -1)
		{
			// Call AddFile() to set the new status.  This will force the status to 
			// the new status unless the status is processing
			VARIANT_BOOL bAlreadyExists = VARIANT_FALSE;
			ipDB->AddFile(bstrFileFullName, strActionName.c_str(), kPriorityDefault,
				VARIANT_TRUE, VARIANT_FALSE, m_eActionStatus, &bAlreadyExists, &ePrevStatus);
		}
		else
		{
			// Ensure the file is not in processing
			if (ipDB->GetFileStatus(nFileID, strActionName.c_str(), VARIANT_FALSE)
				!= kActionProcessing)
			{
				ipDB->SetStatusForFile(nFileID, strActionName.c_str(), m_eActionStatus,
					&ePrevStatus);
			}
			else
			{
				UCLIDException uex("ELI24025", "Cannot change status from Processing");
				uex.addDebugInfo("Action Name", m_strActionName);
				uex.addDebugInfo("Expanded Action Name", strActionName);
				uex.addDebugInfo("Status Value", m_eActionStatus);

				// Try to add the status as a string
				try
				{
					uex.addDebugInfo("Status Requested",
						asString(ipDB->AsStatusString(m_eActionStatus)));
				}
				CATCH_AND_LOG_ALL_EXCEPTIONS("ELI24026");

				// Throw the exception
				throw uex;
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15116")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSetActionStatusFileProcessor::raw_Cancel()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// nothing to do
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15119");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSetActionStatusFileProcessor::raw_Close()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// nothing to do
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17784");

	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSetActionStatusFileProcessor::raw_IsLicensed(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// verify valid argument
		if (pbValue == NULL)
			return E_POINTER;

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

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSetActionStatusFileProcessor::raw_GetComponentDescription(BSTR *pstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19615", pstrComponentDescription != NULL);

		*pstrComponentDescription = _bstr_t("Core: Set file-action status in database").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15106")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSetActionStatusFileProcessor::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		UCLID_FILEPROCESSORSLib::ISetActionStatusFileProcessorPtr ipCopyFrom(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI15107", ipCopyFrom != NULL);
		
		// copy the action name
		m_strActionName = ipCopyFrom->ActionName;

		// copy the action status
		m_eActionStatus = (EActionStatus) ipCopyFrom->ActionStatus;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15108");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSetActionStatusFileProcessor::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		// create a new instance of this object and get the ICopyableObject interface 
		ICopyableObjectPtr ipObjCopy(CLSID_SetActionStatusFileProcessor);
		ASSERT_RESOURCE_ALLOCATION("ELI15109", ipObjCopy != NULL);

		// wrap this object in an IUnknown
		IUnknownPtr ipThis = this;
		ASSERT_RESOURCE_ALLOCATION("ELI15110", ipThis != NULL);

		// ask the new object to copy itself from this object
		ipObjCopy->CopyFrom(ipThis);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15112");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSetActionStatusFileProcessor::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	*pClassID = CLSID_SetActionStatusFileProcessor;
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSetActionStatusFileProcessor::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return m_bDirty ? S_OK : S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSetActionStatusFileProcessor::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// reset member variables
		m_strActionName = "";
		m_eActionStatus = kActionUnattempted;

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
		if (nDataVersion > gnCURRENT_VERSION)
		{
			// Throw exception
			UCLIDException ue("ELI15145", "Unable to load newer SetActionStatusFileProcessor component!");
			ue.addDebugInfo("Current Version", gnCURRENT_VERSION);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		// read the action name
		dataReader >> m_strActionName;

		// read the action status
		long nTemp = kActionUnattempted;
		dataReader >> nTemp;
		m_eActionStatus = (EActionStatus) nTemp;

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15113");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSetActionStatusFileProcessor::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		dataWriter << gnCURRENT_VERSION;

		dataWriter << m_strActionName;
		dataWriter << (long) m_eActionStatus;

		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15144");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSetActionStatusFileProcessor::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSetActionStatusFileProcessor::raw_IsConfigured(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// this object is considered configured as long as a action name is associated with it
		*pbValue = m_strActionName.empty() ? VARIANT_FALSE : VARIANT_TRUE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15114");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CSetActionStatusFileProcessor::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI15125", "SetActionStatus File Processor");
}
//-------------------------------------------------------------------------------------------------
