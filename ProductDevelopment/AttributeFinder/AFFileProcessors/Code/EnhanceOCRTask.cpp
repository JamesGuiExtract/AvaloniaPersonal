// EnhanceOCRTask.cpp : Implementation of CEnhanceOCRTask

#include "stdafx.h"
#include "AFFileProcessors.h"
#include "EnhanceOCRTask.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <ComUtils.h>

// add license management function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCURRENT_VERSION				= 1;

//-------------------------------------------------------------------------------------------------
// CEnhanceOCRTask
//-------------------------------------------------------------------------------------------------
CEnhanceOCRTask::CEnhanceOCRTask()
{
	try
	{
		m_ipEnhanceOCR.CreateInstance(CLSID_EnhanceOCR);
		ASSERT_RESOURCE_ALLOCATION("ELI36628", m_ipEnhanceOCR != __nullptr);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI36587");
}
//-------------------------------------------------------------------------------------------------
CEnhanceOCRTask::~CEnhanceOCRTask()
{
	try
	{
		m_ipEnhanceOCR = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI36588");
}
//-------------------------------------------------------------------------------------------------
HRESULT CEnhanceOCRTask::FinalConstruct()
{
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
void CEnhanceOCRTask::FinalRelease()
{
}

//--------------------------------------------------------------------------------------------------
// IEnhanceOCRTask
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCRTask::get_ConfidenceCriteria(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI36589", pVal != __nullptr);

		validateLicense();

		*pVal = m_ipEnhanceOCR->ConfidenceCriteria;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36590")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCRTask::put_ConfidenceCriteria(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_ipEnhanceOCR->ConfidenceCriteria = newVal;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36591")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCRTask::get_FilterPackage(EFilterPackage *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI36592", pVal != __nullptr);

		validateLicense();

		*pVal = m_ipEnhanceOCR->FilterPackage;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36593")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCRTask::put_FilterPackage(EFilterPackage newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_ipEnhanceOCR->FilterPackage = newVal;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36594")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCRTask::get_CustomFilterPackage(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI36595", pVal != __nullptr);

		validateLicense();

		*pVal = m_ipEnhanceOCR->CustomFilterPackage.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36596")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCRTask::put_CustomFilterPackage(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_ipEnhanceOCR->CustomFilterPackage = newVal;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36597")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCRTask::get_PreferredFormatRegexFile(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI36598", pVal != __nullptr);

		validateLicense();

		*pVal = m_ipEnhanceOCR->PreferredFormatRegexFile.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36599")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCRTask::put_PreferredFormatRegexFile(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_ipEnhanceOCR->PreferredFormatRegexFile = newVal;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36600")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCRTask::get_CharsToIgnore(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI36601", pVal != __nullptr);

		validateLicense();

		*pVal = m_ipEnhanceOCR->CharsToIgnore.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36602")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCRTask::put_CharsToIgnore(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_ipEnhanceOCR->CharsToIgnore = newVal;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36603")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCRTask::get_OutputFilteredImages(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI36604", pVal != __nullptr);

		validateLicense();

		*pVal = m_ipEnhanceOCR->OutputFilteredImages;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36605")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCRTask::put_OutputFilteredImages(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_ipEnhanceOCR->OutputFilteredImages = asCppBool(newVal);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36606")
}

//--------------------------------------------------------------------------------------------------
// IFileProcessingTask
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCRTask::raw_Init(long nActionID, IFAMTagManager* pFAMTM,
	IFileProcessingDB *pDB)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		// nothing to do
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17792");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCRTask::raw_ProcessFile(IFileRecord* pFileRecord, long nActionID,
	IFAMTagManager *pTagManager, IFileProcessingDB *pDB, IProgressStatus *pProgressStatus,
	VARIANT_BOOL bCancelRequested, EFileProcessingResult *pResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		IFileRecordPtr ipFileRecord(pFileRecord);
		ASSERT_ARGUMENT("ELI36670", ipFileRecord != __nullptr);

		ITagUtilityPtr ipTagUtility(pTagManager);
		ASSERT_ARGUMENT("ELI36720", ipTagUtility != __nullptr);

		string strSourceDocName = asString(ipFileRecord->GetName());
		string strUssFilename = strSourceDocName + ".uss";

		IAFDocumentPtr ipAFDoc(CLSID_AFDocument);
		ipAFDoc->Text->SourceDocName = strSourceDocName.c_str();
		
		if (isValidFile(strUssFilename))
		{
			ipAFDoc->Text->LoadFrom(strUssFilename.c_str(), VARIANT_FALSE);
		}
		
		m_ipEnhanceOCR->EnhanceDocument(ipAFDoc, ipTagUtility, pProgressStatus);

		ipAFDoc->Text->SaveTo(strUssFilename.c_str(), VARIANT_TRUE, VARIANT_TRUE);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36665")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCRTask::raw_Cancel()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// nothing to do
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36658");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCRTask::raw_Close()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		// Ensure all accumulated USB clicks are decremented.
		UCLID_AFCORELib::IRuleSetPtr ipRuleSet(CLSID_RuleSet);
		ASSERT_RESOURCE_ALLOCATION("ELI36659", ipRuleSet != __nullptr);

		ipRuleSet->Cleanup();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36660");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCRTask::raw_Standby(VARIANT_BOOL* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{		
		ASSERT_ARGUMENT("ELI36661", pVal != __nullptr);

		*pVal = VARIANT_TRUE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36662");
}

//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCRTask::get_MinStackSize(unsigned long *pnMinStackSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI36663", pnMinStackSize != __nullptr);

		validateLicense();

		*pnMinStackSize = 0;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36664");
}

//-------------------------------------------------------------------------------------------------
// IAccessRequired interface implementation
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCRTask::raw_RequiresAdminAccess(VARIANT_BOOL* pbResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI36656", pbResult != __nullptr);

		*pbResult = VARIANT_FALSE;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36657");
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCRTask::raw_GetComponentDescription(BSTR *pbstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI36607", pbstrComponentDescription != __nullptr)

		*pbstrComponentDescription = _bstr_t("Core: Enhance OCR").Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36608")
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCRTask::raw_IsConfigured(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Check parameter
		ASSERT_ARGUMENT("ELI36609", pbValue != __nullptr);

		IMustBeConfiguredObjectPtr ipMustBeConfiguredObject(m_ipEnhanceOCR);
		ASSERT_RESOURCE_ALLOCATION("ELI36650", ipMustBeConfiguredObject != __nullptr);

		*pbValue = ipMustBeConfiguredObject->IsConfigured();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36610");
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCRTask::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license
		validateLicense();

		UCLID_AFFILEPROCESSORSLib::IEnhanceOCRTaskPtr ipCopyThis = pObject;
		ASSERT_ARGUMENT("ELI36611", ipCopyThis != __nullptr);

		m_ipEnhanceOCR->ConfidenceCriteria			= ipCopyThis->ConfidenceCriteria;
		m_ipEnhanceOCR->FilterPackage				= ipCopyThis->FilterPackage;
		m_ipEnhanceOCR->CustomFilterPackage			= ipCopyThis->CustomFilterPackage;
		m_ipEnhanceOCR->PreferredFormatRegexFile	= ipCopyThis->PreferredFormatRegexFile;
		m_ipEnhanceOCR->CharsToIgnore				= ipCopyThis->CharsToIgnore;
		m_ipEnhanceOCR->OutputFilteredImages		= ipCopyThis->OutputFilteredImages;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36612");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCRTask::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license
		validateLicense();

		ASSERT_ARGUMENT("ELI36613", pObject != __nullptr);

		// Create another instance of this object
		ICopyableObjectPtr ipObjCopy(CLSID_EnhanceOCRTask);
		ASSERT_RESOURCE_ALLOCATION("ELI36614", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);
	
		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36615");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCRTask::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI36616", pClassID != __nullptr);

		*pClassID = CLSID_EnhanceOCRTask;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36617");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCRTask::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		IPersistStreamPtr ipPersistStream(m_ipEnhanceOCR);
		ASSERT_RESOURCE_ALLOCATION("ELI36651", ipPersistStream != __nullptr);

		return ipPersistStream->IsDirty();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36618");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCRTask::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		ASSERT_ARGUMENT("ELI36619", pStream != __nullptr);
		
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
			UCLIDException ue("ELI36620", "Unable to load newer enhance OCR task instance!");
			ue.addDebugInfo("Current Version", gnCURRENT_VERSION);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		// Read the EnhanceOCR object from the stream
		IPersistStreamPtr ipOutputToXML;
		readObjectFromStream(ipOutputToXML, pStream, "ELI36653");
		m_ipEnhanceOCR = ipOutputToXML;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36621");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCRTask::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		ASSERT_ARGUMENT("ELI36622", pStream != __nullptr);

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		// Write the current version
		dataWriter << gnCURRENT_VERSION;

		dataWriter.flushToByteStream();		

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		// Write the Enhance OCR object to the stream
		IPersistStreamPtr ipOuputXmlStream = m_ipEnhanceOCR;
		ASSERT_RESOURCE_ALLOCATION("ELI36654", ipOuputXmlStream != __nullptr);
		writeObjectToStream(ipOuputXmlStream, pStream, "ELI36655", fClearDirty);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36623");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCRTask::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCRTask::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI36624", pbValue != __nullptr);

		try
		{
			// check the license
			validateLicense();

			// If no exception, then pbValue is true
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36625");

	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CEnhanceOCRTask::InterfaceSupportsErrorInfo(REFIID riid)
{
	try
	{
		static const IID* arr[] = 
		{
			&IID_IEnhanceOCRTask,
			&IID_IFileProcessingTask,
			&IID_IPersistStream,
			&IID_ICategorizedComponent,
			&IID_ICopyableObject,
			&IID_ILicensedComponent
		};

		for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
		{
			if (InlineIsEqualGUID(*arr[i],riid))
				return S_OK;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36626")

	return S_FALSE;
}
//--------------------------------------------------------------------------------------------------
// Private Methods
//--------------------------------------------------------------------------------------------------
void CEnhanceOCRTask::validateLicense()
{
	VALIDATE_LICENSE(gnFLEXINDEX_IDSHIELD_SERVER_CORE, "ELI36627", "Enhance OCR Task");
}