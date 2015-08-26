
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
// static const long gnCURRENT_VERSION = 1;     -- original version
static const long gnCURRENT_VERSION = 2;        // updated 8/12/2015 - added document name
static const long gnVERSION_2 = 2;

const std::string strSOURCE_DOC_NAME_TAG = "<SourceDocName>";

//--------------------------------------------------------------------------------------------------
// CSetActionStatusFileProcessor
//--------------------------------------------------------------------------------------------------
CSetActionStatusFileProcessor::CSetActionStatusFileProcessor():
m_bDirty(false), 
m_eActionStatus(kActionPending), 
m_strActionName(strSOURCE_DOC_NAME_TAG),
m_documentName(""),
m_reportErrorWhenFileNotQueued(true)
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
        &IID_IFileProcessingTask,
        &IID_IAccessRequired
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
STDMETHODIMP CSetActionStatusFileProcessor::get_DocumentName(BSTR *pbstrRetVal)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    try
    {
        // Check license
        validateLicense();

        ASSERT_ARGUMENT("ELI38457", pbstrRetVal != __nullptr);

        *pbstrRetVal = _bstr_t(m_documentName.c_str()).Detach();
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38449");

    return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSetActionStatusFileProcessor::put_DocumentName(BSTR bstrNewVal)
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
            UCLIDException ue("ELI38450", "Document name cannot be empty!");
            throw ue;
        }

        // update action name
        m_documentName = strNewVal;

        // set dirty flag to true
        m_bDirty = true;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38451");

    return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSetActionStatusFileProcessor::get_ReportErrorWhenFileNotQueued(VARIANT_BOOL* pbVal)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    try
    {
        // Check license
        validateLicense();

        ASSERT_ARGUMENT("ELI38456", pbVal != __nullptr);

        *pbVal = m_reportErrorWhenFileNotQueued;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38454");     

    return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSetActionStatusFileProcessor::put_ReportErrorWhenFileNotQueued(VARIANT_BOOL bVal)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    try
    {
        // Check license
        validateLicense();

        m_reportErrorWhenFileNotQueued = asCppBool(bVal);

        // set dirty flag to true
        m_bDirty = true;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI38455");    

    return S_OK;
}
//-------------------------------------------------------------------------------------------------


//--------------------------------------------------------------------------------------------------
// IFileProcessingTask
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSetActionStatusFileProcessor::raw_Init(long nActionID, IFAMTagManager* pFAMTM,
    IFileProcessingDB *pDB, IFileRequestHandler* pFileRequestHandler)
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
STDMETHODIMP CSetActionStatusFileProcessor::raw_ProcessFile(IFileRecord* pFileRecord,
    long nActionID, IFAMTagManager *pTagManager, IFileProcessingDB *pDB,
    IProgressStatus *pProgressStatus, VARIANT_BOOL bCancelRequested, EFileProcessingResult *pResult)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState())

    try
    {
        // Check license
        validateLicense();

        ASSERT_ARGUMENT("ELI17927", pResult != __nullptr);
        
        // wrap the database pointer in a smart pointer object
        IFileProcessingDBPtr ipDB(pDB);
        ASSERT_RESOURCE_ALLOCATION("ELI15146", ipDB != __nullptr);

        IFAMTagManagerPtr ipTagManager(pTagManager);
        ASSERT_RESOURCE_ALLOCATION("ELI29126", ipTagManager != __nullptr);

        IFileRecordPtr ipFileRecord(pFileRecord);
        ASSERT_ARGUMENT("ELI31342", ipFileRecord != __nullptr);

        // Default to successful completion
        *pResult = kProcessingSuccessful;

        // Expand file name
        string fileName = CFileProcessorsUtils::ExpandTagsAndTFE(ipTagManager,
                                                                 m_documentName,
                                                                 asString(ipFileRecord->Name));

        // This is not a complete list of invalid chars, because the filename 
        // probably contains ':' and '\' chars. Primarily want to exclude ? and *.
        std::string invalidFilenameChars = "/?\"<>|*";
        ASSERT_ARGUMENT("ELI38452", !Contains(fileName, invalidFilenameChars, MatchSingleChar));

        // Expand action name - using previously expanded file name
        string strActionName = CFileProcessorsUtils::ExpandTagsAndTFE(ipTagManager, 
            m_strActionName, fileName);

        // Auto create if necessary
        ipDB->AutoCreateAction(strActionName.c_str());

        EActionStatus ePrevStatus = kActionUnattempted;

        bstr_t comFileName = get_bstr_t(fileName);

        if (m_reportErrorWhenFileNotQueued)
        {
            // Called for side-effect only - throw if the file hasn't been previously
            // added to the DB.
            ipDB->GetFileID(comFileName);
        }

        try
        {
            try
            {
                // This will throw if the tag-expanded-filename points to a path
                // that doesn't exist, or if the file doesn't exist. Using the 
                // double-catch-pattern to identify the actual target filename
                // in the error report - see the addDebugInfo() below.
                VARIANT_BOOL bAlreadyExists = VARIANT_FALSE;
                ipDB->AddFile(comFileName, strActionName.c_str(), kPriorityDefault,
                    VARIANT_TRUE, VARIANT_FALSE, m_eActionStatus, VARIANT_FALSE, &bAlreadyExists,
                    &ePrevStatus);
            }
            CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI38453");
        }
        catch (UCLIDException ue)
        {
            ue.addDebugInfo("Target Filename", fileName);
            throw ue;
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
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSetActionStatusFileProcessor::raw_Standby(VARIANT_BOOL* pVal)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState());
    
    try
    {       
        ASSERT_ARGUMENT("ELI33912", pVal != __nullptr);

        *pVal = VARIANT_TRUE;

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33913");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSetActionStatusFileProcessor::get_MinStackSize(unsigned long *pnMinStackSize)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState());
    
    try
    {
        ASSERT_ARGUMENT("ELI35019", pnMinStackSize != __nullptr);

        validateLicense();

        *pnMinStackSize = 0;

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI35020");
}

//-------------------------------------------------------------------------------------------------
// IAccessRequired interface implementation
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSetActionStatusFileProcessor::raw_RequiresAdminAccess(VARIANT_BOOL* pbResult)
{
    AFX_MANAGE_STATE(AfxGetStaticModuleState());

    try
    {
        ASSERT_ARGUMENT("ELI31191", pbResult != __nullptr);

        *pbResult = VARIANT_FALSE;

        return S_OK;
    }
    CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31192");
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
        ASSERT_ARGUMENT("ELI19615", pstrComponentDescription != __nullptr);

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
        ASSERT_RESOURCE_ALLOCATION("ELI15107", ipCopyFrom != __nullptr);
        
        // copy the action name
        m_strActionName = ipCopyFrom->ActionName;

        // copy the action status
        m_eActionStatus = (EActionStatus) ipCopyFrom->ActionStatus;

        m_documentName = ipCopyFrom->DocumentName;
        m_reportErrorWhenFileNotQueued = asCppBool(ipCopyFrom->ReportErrorWhenFileNotQueued);
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
        ASSERT_RESOURCE_ALLOCATION("ELI15109", ipObjCopy != __nullptr);

        // wrap this object in an IUnknown
        IUnknownPtr ipThis = this;
        ASSERT_RESOURCE_ALLOCATION("ELI15110", ipThis != __nullptr);

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

		ResetMemberVariables();

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

        if ( nDataVersion >= gnVERSION_2 )
        {
            dataReader >> m_documentName;
            dataReader >> m_reportErrorWhenFileNotQueued;
        }

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

        dataWriter << m_documentName;
        dataWriter << m_reportErrorWhenFileNotQueued;

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
void CSetActionStatusFileProcessor::ResetMemberVariables()
{
	m_bDirty = false;
	m_eActionStatus = kActionPending; 
	m_strActionName = "";
	m_documentName = strSOURCE_DOC_NAME_TAG;
	m_reportErrorWhenFileNotQueued = true;
}




