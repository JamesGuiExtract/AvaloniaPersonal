// MergeAttributeTrees.cpp : Implementation of CMergeAttributeTrees

#include "stdafx.h"
#include "MergeAttributeTrees.h"

#include <ComponentLicenseIDs.h>
#include <ComUtils.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <StringTokenizer.h>
#include <UCLIDException.h>

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

//--------------------------------------------------------------------------------------------------
// CMergeAttributeTrees
//--------------------------------------------------------------------------------------------------
CMergeAttributeTrees::CMergeAttributeTrees()
:
m_strAttributesToBeMerged(""),
m_eMergeInto(kFirstAttribute),
m_bDirty(false),
m_bDiscardNonMatch(false),
m_bCompareTypeInfo(true),
m_bCompareSubAttributes(false)
{
	try
	{
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26337");
}
//--------------------------------------------------------------------------------------------------
CMergeAttributeTrees::~CMergeAttributeTrees()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI26338");
}
//-------------------------------------------------------------------------------------------------
HRESULT CMergeAttributeTrees::FinalConstruct()
{
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
void CMergeAttributeTrees::FinalRelease()
{
}

//--------------------------------------------------------------------------------------------------
// IMergeAttributeTrees
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributeTrees::get_AttributesToBeMerged(BSTR* pbstrAttributesToBeMerged)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI26339", pbstrAttributesToBeMerged != NULL);

		validateLicense();
		
		*pbstrAttributesToBeMerged = get_bstr_t(m_strAttributesToBeMerged).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26340")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributeTrees::put_AttributesToBeMerged(BSTR bstrAttributesToBeMerged)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// Get the string and validate it
		string strAttributesToBeMerged = asString(bstrAttributesToBeMerged);
		validateAttributeString(strAttributesToBeMerged);

		// Store the string
		m_strAttributesToBeMerged = strAttributesToBeMerged;

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26341")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributeTrees::get_MergeAttributeTreesInto(EMergeAttributeTreesInto* peMergeInto)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI26342", peMergeInto != NULL);

		validateLicense();
		
		*peMergeInto = m_eMergeInto;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26343")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributeTrees::put_MergeAttributeTreesInto(EMergeAttributeTreesInto eMergeInto)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_eMergeInto = eMergeInto;

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26344")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributeTrees::get_SubAttributesToCompare(BSTR* pbstrSubAttributesToCompare)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI26345", pbstrSubAttributesToCompare != NULL);

		validateLicense();

		// Get the sub attribute compares as a single multi-line string
		*pbstrSubAttributesToCompare = get_bstr_t(getSubAttributeComparesAsString()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26346")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributeTrees::put_SubAttributesToCompare(BSTR bstrSubAttributesToCompare)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// Set the sub attribute compares from the multi-line string
		setSubAttributeComparesFromString(asString(bstrSubAttributesToCompare));

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26347")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributeTrees::get_DiscardNonMatchingComparisons(VARIANT_BOOL* pvbDiscard)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI26348", pvbDiscard != NULL);

		validateLicense();
		
		*pvbDiscard = asVariantBool(m_bDiscardNonMatch);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26349")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributeTrees::put_DiscardNonMatchingComparisons(VARIANT_BOOL vbDiscard)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bDiscardNonMatch = asCppBool(vbDiscard);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26350")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributeTrees::get_CaseSensitive(VARIANT_BOOL* pvbCaseSensitive)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI26351", pvbCaseSensitive != NULL);

		validateLicense();
		
		*pvbCaseSensitive = asVariantBool(m_bCaseSensitive);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26352")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributeTrees::put_CaseSensitive(VARIANT_BOOL vbCaseSensitive)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bCaseSensitive = asCppBool(vbCaseSensitive);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26353")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributeTrees::get_CompareTypeInformation(
	VARIANT_BOOL* pvbCompareTypeInformation)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI26354", pvbCompareTypeInformation != NULL);

		validateLicense();
		
		*pvbCompareTypeInformation = asVariantBool(m_bCompareTypeInfo);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26355")
}
//--------------------------------------------------------------------------------------------------
// TODO: FUTURE - Fully implement this setting
//STDMETHODIMP CMergeAttributeTrees::put_CompareTypeInformation(VARIANT_BOOL vbCompareTypeInformation)
//{
//	AFX_MANAGE_STATE(AfxGetStaticModuleState());
//
//	try
//	{
//		validateLicense();
//
//		m_bCompareTypeInfo = asCppBool(vbCompareTypeInformation);
//
//		m_bDirty = true;
//
//		return S_OK;
//	}
//	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26356")
//}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributeTrees::get_CompareSubAttributes(VARIANT_BOOL* pvbCompareSubAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI26357", pvbCompareSubAttributes != NULL);

		validateLicense();
		
		*pvbCompareSubAttributes = asVariantBool(m_bCompareSubAttributes);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26358")
}
//--------------------------------------------------------------------------------------------------
// TODO: FUTURE - Fully implement this setting
//STDMETHODIMP CMergeAttributeTrees::put_CompareTypeInformation(VARIANT_BOOL vbCompareSubAttributes)
//{
//	AFX_MANAGE_STATE(AfxGetStaticModuleState());
//
//	try
//	{
//		validateLicense();
//
//		m_bCompareSubAttributes = asCppBool(vbCompareSubAttributes);
//
//		m_bDirty = true;
//
//		return S_OK;
//	}
//	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26359")
//}

//--------------------------------------------------------------------------------------------------
// IOutputHandler
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributeTrees::raw_ProcessOutput(IIUnknownVector* pAttributes, IAFDocument *pAFDoc,
												 IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		IIUnknownVectorPtr ipAttributes(pAttributes);
		ASSERT_ARGUMENT("ELI26360", ipAttributes != NULL);

		IAFDocumentPtr ipAFDocument(pAFDoc);
		ASSERT_ARGUMENT("ELI26361", ipAFDocument != NULL);

		// TODO: Implement the output handler logic

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26362")
}

//--------------------------------------------------------------------------------------------------
// IPersistStream
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributeTrees::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI26363", pClassID != NULL);

		*pClassID = CLSID_MergeAttributeTrees;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26364");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributeTrees::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		return m_bDirty ? S_OK : S_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26365");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributeTrees::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		ASSERT_ARGUMENT("ELI26366", pStream != NULL);

		// Reset values
		m_strAttributesToBeMerged = "";
		m_vecSubattributesToCompare.clear();
		m_eMergeInto = kFirstAttribute;

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
			UCLIDException ue("ELI26367", 
				"Unable to load newer merge attributes rule!");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		// Read data members from the stream
		dataReader >> m_strAttributesToBeMerged;
		long lTemp;

		// Read the EMergeAttributeTreesInto value
		dataReader >> lTemp;
		m_eMergeInto = (EMergeAttributeTreesInto) lTemp;

		// Get the sub attribute compares string
		string strTemp;
		dataReader >> strTemp;
		setSubAttributeComparesFromString(strTemp);

		// Read the remaining flags
		dataReader >> m_bDiscardNonMatch;
		dataReader >> m_bCaseSensitive;
		dataReader >> m_bCompareTypeInfo;
		dataReader >> m_bCompareSubAttributes;

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26368");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributeTrees::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		ASSERT_ARGUMENT("ELI26369", pStream != NULL);

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		// Write the current version
		dataWriter << gnCurrentVersion;

		// Write the data members to the stream
		dataWriter << m_strAttributesToBeMerged;
		dataWriter << (long) m_eMergeInto;
		dataWriter << getSubAttributeComparesAsString();
		dataWriter << m_bDiscardNonMatch;
		dataWriter << m_bCaseSensitive;
		dataWriter << m_bCompareTypeInfo;
		dataWriter << m_bCompareSubAttributes;

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

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26370");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributeTrees::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}

//--------------------------------------------------------------------------------------------------
// ICategorizedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributeTrees::raw_GetComponentDescription(BSTR *pbstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI26371", pbstrComponentDescription != NULL)

		*pbstrComponentDescription = _bstr_t("Merge attribute trees").Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26372")
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributeTrees::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI26373", pbValue != NULL);

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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26374");
}

//--------------------------------------------------------------------------------------------------
// ICopyableObject
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributeTrees::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license
		validateLicense();

		UCLID_AFOUTPUTHANDLERSLib::IMergeAttributeTreesPtr ipSource = pObject;
		ASSERT_ARGUMENT("ELI26375", ipSource != NULL);

		// Copy members
		m_strAttributesToBeMerged = asString(ipSource->AttributesToBeMerged);
		m_eMergeInto = (EMergeAttributeTreesInto) ipSource->MergeAttributeTreesInto;
		setSubAttributeComparesFromString(asString(ipSource->SubAttributesToCompare));
		m_bDiscardNonMatch = asCppBool(ipSource->DiscardNonMatchingComparisons);
		m_bCaseSensitive = asCppBool(ipSource->CaseSensitive);
		m_bCompareTypeInfo = asCppBool(ipSource->CompareTypeInformation);
		m_bCompareSubAttributes = asCppBool(ipSource->CompareSubAttributes);

		// Since this object has changed, set the dirty flag
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26376");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributeTrees::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license
		validateLicense();

		ASSERT_ARGUMENT("ELI26377", pObject != NULL);

		// Create another instance of this object
		ICopyableObjectPtr ipObjCopy(CLSID_MergeAttributeTrees);
		ASSERT_RESOURCE_ALLOCATION("ELI26378", ipObjCopy != NULL);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);
	
		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26379");
}

//--------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributeTrees::raw_IsConfigured(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Check parameter
		ASSERT_ARGUMENT("ELI26380", pbValue != NULL);

		// Object is configured if there is an attribute query defined and
		// at least 1 sub attribute to compare
		bool bConfigured = !m_strAttributesToBeMerged.empty()
			&& m_vecSubattributesToCompare.size() > 0;

		*pbValue = asVariantBool(bConfigured);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26381");
}

//---------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//---------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributeTrees::InterfaceSupportsErrorInfo(REFIID riid)
{
	try
	{
		static const IID* arr[] = 
		{
			&IID_IMergeAttributeTrees,
			&IID_IOutputHandler,
			&IID_IPersistStream,
			&IID_ICategorizedComponent,
			&IID_ISpecifyPropertyPages,
			&IID_ICopyableObject,
			&IID_IMustBeConfiguredObject,
			&IID_ILicensedComponent
		};

		for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
		{
			if (InlineIsEqualGUID(*arr[i],riid))
				return S_OK;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26382")
}

//--------------------------------------------------------------------------------------------------
// Private Methods
//--------------------------------------------------------------------------------------------------
void CMergeAttributeTrees::validateLicense()
{
	VALIDATE_LICENSE(gnFLEXINDEX_IDSHIELD_CORE_OBJECTS, "ELI26383", 
		"Merge attribute trees output handler");
}
//--------------------------------------------------------------------------------------------------
void CMergeAttributeTrees::setSubAttributeComparesFromString(const string& strSubAttributes)
{
	try
	{
		// Validate the string
		validateAttributeString(strSubAttributes);

		// First clear the current vector of lines
		m_vecSubattributesToCompare.clear();

		// Now tokenize the string by new lines to fill the vector of comparisons
		StringTokenizer::sGetTokens(strSubAttributes, "\r\n", m_vecSubattributesToCompare);	
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26384");
}
//--------------------------------------------------------------------------------------------------
string CMergeAttributeTrees::getSubAttributeComparesAsString()
{
	try
	{
		// Build a string from the sub attributes
		size_t nSize = m_vecSubattributesToCompare.size();
		string strSubAttributes = nSize > 0 ? m_vecSubattributesToCompare[0] : "";
		for (size_t i=1; i < nSize; i++)
		{
			strSubAttributes += "\r\n";
			strSubAttributes += m_vecSubattributesToCompare[i];
		}

		return strSubAttributes;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26385");
}
//--------------------------------------------------------------------------------------------------
void CMergeAttributeTrees::validateAttributeString(const string& strSubAttributes)
{
	try
	{
		// Look for | character and throw exception if one is found
		if (strSubAttributes.find_first_of("|") != string::npos)
		{
			// Copy the string and convert any \r\n's to \\r\\n
			string strTemp = strSubAttributes;
			convertCppStringToNormalString(strTemp);

			UCLIDException uex("ELI26386",
				"| character is not valid for this objects attribute queries!");
			uex.addDebugInfo("Attribute Query", strTemp);
			throw uex;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26387");
}
//--------------------------------------------------------------------------------------------------