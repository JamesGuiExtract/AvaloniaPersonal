// TagCondition.cpp : Implementation of CTagCondition

#include "stdafx.h"
#include "TagCondition.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//-------------------------------------------------------------------------------------------------
// CTagCondition
//-------------------------------------------------------------------------------------------------
CTagCondition::CTagCondition() :
m_bAnyTags(false),
m_bConsiderMet(true),
m_ipVecTags(NULL)
{
	try
	{
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI27517");
}
//-------------------------------------------------------------------------------------------------
CTagCondition::~CTagCondition()
{
	try
	{
		m_ipVecTags = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI27518");
}
//-------------------------------------------------------------------------------------------------
HRESULT CTagCondition::FinalConstruct()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
void CTagCondition::FinalRelease()
{
	try
	{
		m_ipVecTags = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI27519");
}

//-------------------------------------------------------------------------------------------------
// ITagCondition
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTagCondition::get_ConsiderMet(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI27520", pVal != NULL);

		*pVal = asVariantBool(m_bConsiderMet);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27521");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTagCondition::put_ConsiderMet(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bConsiderMet = asCppBool(newVal);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27522");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTagCondition::get_AnyTags(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI27523", pVal != NULL);

		*pVal = asVariantBool(m_bAnyTags);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27524");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTagCondition::put_AnyTags(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bAnyTags = asCppBool(newVal);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27525");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTagCondition::get_Tags(IVariantVector **ppVecTags)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI27526", ppVecTags != NULL);

		IVariantVectorPtr ipShallowCopy = m_ipVecTags;

		*ppVecTags = ipShallowCopy.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27527");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTagCondition::put_Tags(IVariantVector *pVecTags)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		IVariantVectorPtr ipVecTags(pVecTags);

		m_ipVecTags = ipVecTags;

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27528");
}

//-------------------------------------------------------------------------------------------------
// IFAMCondition
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTagCondition::raw_FileMatchesFAMCondition(BSTR bstrFile, IFileProcessingDB *pFPDB, 
									BSTR bstrAction, IFAMTagManager *pFAMTM, VARIANT_BOOL* pRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		string strSourceFileName = asString(bstrFile);
		ASSERT_ARGUMENT("ELI27529", !strSourceFileName.empty());
		IFileProcessingDBPtr ipFPDB(pFPDB);
		ASSERT_ARGUMENT("ELI27530", ipFPDB != NULL);
		ASSERT_ARGUMENT("ELI27531", pFAMTM != NULL);
		ASSERT_ARGUMENT("ELI27532", pRetVal != NULL);

		validateLicense();

		// Ensure the tags vector is not NULL
		if (m_ipVecTags == NULL)
		{
			UCLIDException ue("ELI27533", "No tags set for condition to evaluate!");
			throw ue;
		}

		// Get the file ID
		long nFileID = ipFPDB->GetFileID(bstrFile);

		IVariantVectorPtr ipTagsOnFile = ipFPDB->GetTagsOnFile(nFileID);
		ASSERT_RESOURCE_ALLOCATION("ELI27534", ipTagsOnFile != NULL);

		// Default the found value to true
		bool bFound = true;

		// Iterate through all of the tags
		long lSize = m_ipVecTags->Size;
		for (long i=0; i < lSize; i++)
		{
			// Check if the file contains the tag
			if (ipTagsOnFile->Contains(m_ipVecTags->Item[i]) == VARIANT_TRUE)
			{
				// If any tags, then just break from loop
				if (m_bAnyTags)
				{
					break;
				}
			}
			// If tag is not found and all tags are required set found
			// to false and break from the loop
			else if (!m_bAnyTags)
			{
				bFound = false;
				break;
			}
		}

		// If consider not met, reverse the found value
		if (!m_bConsiderMet)
		{
			bFound = !bFound;
		}

		// Set the return value
		*pRetVal = asVariantBool(bFound);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27535");
}

//--------------------------------------------------------------------------------------------------
// ISupportErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CTagCondition::InterfaceSupportsErrorInfo(REFIID riid)
{
	try
	{
		static const IID* arr[] = 
		{
			&IID_ITagCondition,
			&IID_IFAMCondition,
			&IID_IPersistStream,
			&IID_ICopyableObject,
			&IID_ICategorizedComponent,
			&IID_IMustBeConfiguredObject,
			&IID_ILicensedComponent,
			&IID_ISpecifyPropertyPages
		};

		for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
		{
			if (InlineIsEqualGUID(*arr[i],riid))
				return S_OK;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27536")

	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTagCondition::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI27537", pbValue != NULL);

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

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27538");
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTagCondition::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI27539", pClassID != NULL);

		*pClassID = CLSID_TagCondition;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27540");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTagCondition::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		return m_bDirty ? S_OK : S_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27541");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTagCondition::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		IStreamPtr ipStream(pStream);
		ASSERT_ARGUMENT("ELI27542", ipStream != NULL);

		// Check license state
		validateLicense();

		// Reset existing members
		m_bConsiderMet = true;
		m_bAnyTags = false;
		m_ipVecTags = NULL;
		
		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		HANDLE_HRESULT(ipStream->Read(&nDataLength, sizeof(nDataLength), NULL), "ELI27543",
			"Unable to read object size from stream.", ipStream, __uuidof(IStream));
		ByteStream data(nDataLength);
		HANDLE_HRESULT(ipStream->Read(data.getData(), nDataLength, NULL), "ELI27544",
			"Unable to read object from stream.", ipStream, __uuidof(IStream));
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);

		// Read the individual data items from the bytestream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		// Check for newer version
		if (nDataVersion > gnCurrentVersion)
		{
			// Throw exception
			UCLIDException ue("ELI27545", "Unable to load newer tag condition!");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		// Read data members
		dataReader >> m_bConsiderMet;
		dataReader >> m_bAnyTags;

		// Read the file processing task from the stream
		IPersistStreamPtr ipTags;
		readObjectFromStream(ipTags, ipStream, "ELI27546");
		m_ipVecTags = ipTags;
		
		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27547");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTagCondition::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		IStreamPtr ipStream(pStream);
		ASSERT_ARGUMENT("ELI27548", ipStream != NULL);

		// Check license state
		validateLicense();

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		// Write the current version
		dataWriter << gnCurrentVersion;
		
		// Write data members;
		dataWriter << m_bConsiderMet;
		dataWriter << m_bAnyTags;
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		HANDLE_HRESULT(ipStream->Write(&nDataLength, sizeof(nDataLength), NULL), "ELI27549",
			"Unable to write object size to stream.", ipStream, __uuidof(IStream));
		HANDLE_HRESULT(ipStream->Write(data.getData(), nDataLength, NULL), "ELI27550",
			"Unable to write object to stream.", ipStream, __uuidof(IStream));

		// Write the vector of tags to the stream
		IPersistStreamPtr ipTags = m_ipVecTags;
		ASSERT_RESOURCE_ALLOCATION("ELI27551", ipTags != NULL);
		writeObjectToStream(ipTags, ipStream, "ELI27552", fClearDirty);

		// Clear the flag as specified
		if (fClearDirty == TRUE)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27553");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTagCondition::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTagCondition::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI27554", pObject != NULL);
			
		// Validate license
		validateLicense();

		EXTRACT_FAMCONDITIONSLib::ITagConditionPtr ipCopyThis = pObject;
		ASSERT_ARGUMENT("ELI27555", ipCopyThis != NULL);

		m_bConsiderMet = asCppBool(ipCopyThis->ConsiderMet);
		m_bAnyTags = asCppBool(ipCopyThis->AnyTags);

		// Get the tags as a copyable object
		ICopyableObjectPtr ipCopier(ipCopyThis->Tags);
		if (ipCopier == NULL)
		{
			// Tags is NULL, so set to NULL
			m_ipVecTags = NULL;
		}
		else
		{
			// Clone the tags
			m_ipVecTags = ipCopier->Clone();
			ASSERT_RESOURCE_ALLOCATION("ELI27556", m_ipVecTags != NULL);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27557");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTagCondition::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI27558", pObject != NULL);

		// Validate license
		validateLicense();

		// Create another instance of this object
		ICopyableObjectPtr ipObjCopy(CLSID_TagCondition);
		ASSERT_RESOURCE_ALLOCATION("ELI27559", ipObjCopy != NULL);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);
	
		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27560");
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTagCondition::raw_IsConfigured(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		ASSERT_ARGUMENT("ELI27561", pbValue != NULL);

		// Check license
		validateLicense();

		// Configured if:
		// 1. The tag vector is not NULL
		// 2. The tag vector is not empty
		bool bConfigured = m_ipVecTags != NULL && m_ipVecTags->Size > 0;

		*pbValue = asVariantBool(bConfigured);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27562");
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTagCondition::raw_GetComponentDescription(BSTR *pbstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI27563", pbstrComponentDescription != NULL)

		*pbstrComponentDescription = _bstr_t("Tag condition").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27564")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void CTagCondition::validateLicense()
{
	VALIDATE_LICENSE(gnFLEXINDEX_IDSHIELD_CORE_OBJECTS, "ELI27565", "Tag FAM Condition");
}
//-------------------------------------------------------------------------------------------------