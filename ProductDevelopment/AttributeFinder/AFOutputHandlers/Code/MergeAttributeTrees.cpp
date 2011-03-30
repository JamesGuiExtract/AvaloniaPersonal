// MergeAttributeTrees.cpp : Implementation of CMergeAttributeTrees

#include "stdafx.h"
#include "MergeAttributeTrees.h"

#include <ComponentLicenseIDs.h>
#include <ComUtils.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <StringTokenizer.h>
#include <UCLIDException.h>

#include <utility>
#include <vector>

//--------------------------------------------------------------------------------------------------
// Local structs
//--------------------------------------------------------------------------------------------------
struct AttributeData
{
	AttributeData(const string& strValue = "", const string& strType = "") :
	value(strValue), type(strType)
	{
	}

	string value;
	string type;
};

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 2;
const bool gbDEFAULT_CASE_SENSITIVE = false;
const bool gbDEFAULT_DISCARD_NON_MATCH = false;
const bool gbDEFAULT_REMOVE_EMPTY = true;
const bool gbDEFAULT_COMPARE_TYPE_INFO = true;
const bool gbDEFAULT_COMPARE_SUB_ATTRIBUTES = false;

//--------------------------------------------------------------------------------------------------
// CMergeAttributeTrees
//--------------------------------------------------------------------------------------------------
CMergeAttributeTrees::CMergeAttributeTrees()
:
m_ipAFUtils(NULL),
m_strAttributesToBeMerged(""),
m_eMergeInto(kFirstAttribute),
m_bDirty(false),
m_bCaseSensitive(gbDEFAULT_CASE_SENSITIVE),
m_bDiscardNonMatch(gbDEFAULT_DISCARD_NON_MATCH),
m_bRemoveEmptyHierarchy(gbDEFAULT_REMOVE_EMPTY),
m_bCompareTypeInfo(gbDEFAULT_COMPARE_TYPE_INFO),
m_bCompareSubAttributes(gbDEFAULT_COMPARE_SUB_ATTRIBUTES)
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
	try
	{
		// Ensure the AFUtils object is released before the object is destructed
		m_ipAFUtils = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI26396");
}

//--------------------------------------------------------------------------------------------------
// IMergeAttributeTrees
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributeTrees::get_AttributesToBeMerged(BSTR* pbstrAttributesToBeMerged)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI26339", pbstrAttributesToBeMerged != __nullptr);

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
		ASSERT_ARGUMENT("ELI26342", peMergeInto != __nullptr);

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
		ASSERT_ARGUMENT("ELI26345", pbstrSubAttributesToCompare != __nullptr);

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
		ASSERT_ARGUMENT("ELI26348", pvbDiscard != __nullptr);

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
		ASSERT_ARGUMENT("ELI26351", pvbCaseSensitive != __nullptr);

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
		ASSERT_ARGUMENT("ELI26354", pvbCompareTypeInformation != __nullptr);

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
		ASSERT_ARGUMENT("ELI26357", pvbCompareSubAttributes != __nullptr);

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
STDMETHODIMP CMergeAttributeTrees::get_RemoveEmptyHierarchy(VARIANT_BOOL* pvbRemoveEmptyHierarchy)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI26455", pvbRemoveEmptyHierarchy != __nullptr);

		validateLicense();

		*pvbRemoveEmptyHierarchy = asVariantBool(m_bRemoveEmptyHierarchy);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26456");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributeTrees::put_RemoveEmptyHierarchy(VARIANT_BOOL vbRemoveEmptyHierarchy)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bRemoveEmptyHierarchy = asCppBool(vbRemoveEmptyHierarchy);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26457");
}

//--------------------------------------------------------------------------------------------------
// IOutputHandler
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributeTrees::raw_ProcessOutput(IIUnknownVector* pAttributes, IAFDocument *pAFDoc,
												 IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check licensing
		validateLicense();

		IIUnknownVectorPtr ipAttributes(pAttributes);
		ASSERT_ARGUMENT("ELI26360", ipAttributes != __nullptr);

		IAFDocumentPtr ipAFDocument(pAFDoc);
		ASSERT_ARGUMENT("ELI26361", ipAFDocument != __nullptr);

		// Get an AFUtility object to query attributes
		IAFUtilityPtr ipAFUtil = getAFUtility();

		// Get a vector of attributes matching the specified query (do not remove the attributes
		// from the original vector of attributes)
		IIUnknownVectorPtr ipMatches = ipAFUtil->QueryAttributes(ipAttributes,
			m_strAttributesToBeMerged.c_str(), VARIANT_FALSE);
		ASSERT_RESOURCE_ALLOCATION("ELI26407", ipMatches != __nullptr);

		// Loop through all matches, comparing sub attributes and add each attribute
		// which matches other attributes to a collection of matches that will then be
		// processed
		vector<pair<IAttributePtr, vector<IAttributePtr>>> vecMatches;
		long lSize = ipMatches->Size();
		for (long i=0; i < lSize; i++)
		{
			// Get the first attribute
			IAttributePtr ipAttribute1 = ipMatches->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI26408", ipAttribute1 != __nullptr);

			// Get the sub attributes for the comparison
			IIUnknownVectorPtr ipSubAttributes1 = ipAttribute1->SubAttributes;
			ASSERT_RESOURCE_ALLOCATION("ELI26409", ipSubAttributes1 != __nullptr);

			// Vector to hold the list of attributes that need to be merged
			// with the first attribute
			vector<IAttributePtr> vecSubMatches;
			for (long j=i+1; j < lSize; j++)
			{
				// Get the next attribute
				IAttributePtr ipAttribute2 = ipMatches->At(j);
				ASSERT_RESOURCE_ALLOCATION("ELI26410", ipAttribute2 != __nullptr);

				// Get the sub attributes for comparison
				IIUnknownVectorPtr ipSubAttributes2 = ipAttribute2->SubAttributes;
				ASSERT_RESOURCE_ALLOCATION("ELI26411", ipSubAttributes2 != __nullptr);

				// Check if there is a match between these attributes
				if (compareSubAttributes(ipSubAttributes1, ipSubAttributes2))
				{
					// Add the match to the sub match vector, remove this value
					// from the matches collection and update index values
					vecSubMatches.push_back(ipAttribute2);
					ipMatches->Remove(j);
					j--;
					lSize--;
				}
			}

			// If there was at least one match, add the match to the match vector
			if (vecSubMatches.size() > 0)
			{
				vecMatches.push_back(pair<IAttributePtr, vector<IAttributePtr>>(ipAttribute1,
					vecSubMatches));
			}
		}

		// For all the matches, merge the sub attributes into a single attribute (the
		// single attribute will be either the first attribute or the one with the most children)
		vector<IAttributePtr> vecMergedAttributes;
		long lMatchSize = vecMatches.size();
		for(long i=0; i < lMatchSize; i++)
		{
			// Build a vector of all attributes that matched
			vector<IAttributePtr> vecNewMatch;
			vecNewMatch.push_back(vecMatches[i].first);
			vecNewMatch.insert(vecNewMatch.begin()+1, vecMatches[i].second.begin(),
				vecMatches[i].second.end());

			// Find which attribute to keep (either first or one with the most children)
			IAttributePtr ipKeep = __nullptr;
			if (m_eMergeInto == kFirstAttribute)
			{
				ipKeep = vecNewMatch[0];
				vecNewMatch.erase(vecNewMatch.begin());
			}
			else
			{
				// Start with the first item
				ipKeep = vecNewMatch[0];
				IIUnknownVectorPtr ipSubs = ipKeep->SubAttributes;
				ASSERT_RESOURCE_ALLOCATION("ELI26412", ipSubs != __nullptr);
				long lCurrentMax = ipSubs->Size();

				// Get the count of sub attributes and find the largest one
				vector<IAttributePtr>::iterator biggest = vecNewMatch.begin();
				for(vector<IAttributePtr>::iterator it = biggest+1; it != vecNewMatch.end(); it++)
				{
					ipSubs = (*it)->SubAttributes;
					ASSERT_RESOURCE_ALLOCATION("ELI26413", ipSubs != __nullptr);

					long lCount = ipSubs->Size();
					if (lCount > lCurrentMax)
					{
						lCurrentMax = lCount;
						ipKeep = (*it);
						biggest = it;
					}
				}

				// Remove the largest item from the match vector
				vecNewMatch.erase(biggest);
			}
			ASSERT_RESOURCE_ALLOCATION("ELI26414", ipKeep != __nullptr);

			// Now combine the sub attributes from all other attributes into the keeper
			IIUnknownVectorPtr ipKeepSubs = ipKeep->SubAttributes;
			ASSERT_RESOURCE_ALLOCATION("ELI26415", ipKeepSubs != __nullptr);
			vector<IAttributePtr> vecMoveRemove;
			for(vector<IAttributePtr>::iterator it = vecNewMatch.begin();
				it != vecNewMatch.end(); it++)
			{
				IIUnknownVectorPtr ipSubs = (*it)->SubAttributes;

				// And remove the non-matching sub attributes that need to be either removed
				// or moved to the very end
				getAttributesToMoveOrRemove(ipKeepSubs, ipSubs, vecMoveRemove);

				// Add the sub attributes to the keep attribute
				ipKeepSubs->Append(ipSubs);

				// Now clear the subattributes from this value
				ipSubs->Clear();
			}

			// If keeping the non-matching comparison attributes, append them
			// to the end of the list
			if (!m_bDiscardNonMatch && vecMoveRemove.size() > 0)
			{
				for (vector<IAttributePtr>::iterator it = vecMoveRemove.begin();
					it != vecMoveRemove.end(); it++)
				{
					ipKeepSubs->PushBack((*it));
				}
			}
			vecMoveRemove.clear();

			// Add the merged attributes to the merged attributes collection
			vecMergedAttributes.insert(vecMergedAttributes.begin(), vecNewMatch.begin(),
				vecNewMatch.end());
		}

		// Remove the merged attributes from the main attribute collection
		removeMergedAttributes(ipAttributes, vecMergedAttributes);

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
		ASSERT_ARGUMENT("ELI26363", pClassID != __nullptr);

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
// Version 2 - Added the remove empty attribute hierarchy setting
STDMETHODIMP CMergeAttributeTrees::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		ASSERT_ARGUMENT("ELI26366", pStream != __nullptr);

		// Reset values
		m_strAttributesToBeMerged = "";
		m_vecSubattributesToCompare.clear();
		m_eMergeInto = kFirstAttribute;
		m_bCaseSensitive = gbDEFAULT_CASE_SENSITIVE;
		m_bDiscardNonMatch = gbDEFAULT_DISCARD_NON_MATCH;
		m_bRemoveEmptyHierarchy = gbDEFAULT_REMOVE_EMPTY;
		m_bCompareTypeInfo = gbDEFAULT_COMPARE_TYPE_INFO;
		m_bCompareSubAttributes = gbDEFAULT_COMPARE_SUB_ATTRIBUTES;

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

		// Read the remove empty hierarchy flag
		if (nDataVersion >= 2)
		{
			dataReader >> m_bRemoveEmptyHierarchy;
		}

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

		ASSERT_ARGUMENT("ELI26369", pStream != __nullptr);

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
		dataWriter << m_bRemoveEmptyHierarchy;

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
		ASSERT_ARGUMENT("ELI26371", pbstrComponentDescription != __nullptr)

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
		ASSERT_ARGUMENT("ELI26373", pbValue != __nullptr);

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
		ASSERT_ARGUMENT("ELI26375", ipSource != __nullptr);

		// Copy members
		m_strAttributesToBeMerged = asString(ipSource->AttributesToBeMerged);
		m_eMergeInto = (EMergeAttributeTreesInto) ipSource->MergeAttributeTreesInto;
		setSubAttributeComparesFromString(asString(ipSource->SubAttributesToCompare));
		m_bDiscardNonMatch = asCppBool(ipSource->DiscardNonMatchingComparisons);
		m_bCaseSensitive = asCppBool(ipSource->CaseSensitive);
		m_bCompareTypeInfo = asCppBool(ipSource->CompareTypeInformation);
		m_bCompareSubAttributes = asCppBool(ipSource->CompareSubAttributes);
		m_bRemoveEmptyHierarchy = asCppBool(ipSource->RemoveEmptyHierarchy);

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

		ASSERT_ARGUMENT("ELI26377", pObject != __nullptr);

		// Create another instance of this object
		ICopyableObjectPtr ipObjCopy(CLSID_MergeAttributeTrees);
		ASSERT_RESOURCE_ALLOCATION("ELI26378", ipObjCopy != __nullptr);

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
		ASSERT_ARGUMENT("ELI26380", pbValue != __nullptr);

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
bool CMergeAttributeTrees::compareSubAttributes(IIUnknownVectorPtr ipSubAttributes1,
												IIUnknownVectorPtr ipSubAttributes2)
{
	try
	{
		// Get the attribute finder utility pointer
		IAFUtilityPtr ipAFUtil = getAFUtility();

		// Get the collection of sub attributes grouped by name
		IStrToObjectMapPtr ipNames1 = ipAFUtil->GetNameToAttributesMap(ipSubAttributes1);
		ASSERT_RESOURCE_ALLOCATION("ELI26400", ipNames1 != __nullptr);
		IStrToObjectMapPtr ipNames2 = ipAFUtil->GetNameToAttributesMap(ipSubAttributes2);
		ASSERT_RESOURCE_ALLOCATION("ELI26401", ipNames2 != __nullptr);

		// Loop through each of the comparison strings to find matches
		bool bMatch = true;
		for (vector<string>::iterator it = m_vecSubattributesToCompare.begin();
			it != m_vecSubattributesToCompare.end(); it++)
		{
			// Try to get the collection for this value
			_bstr_t bstrName((*it).c_str());
			IIUnknownVectorPtr ipValues1 = ipNames1->TryGetValue(bstrName);
			IIUnknownVectorPtr ipValues2 = ipNames2->TryGetValue(bstrName);

			// If either collection is NULL these attributes do not match
			// set match to false and break
			if (ipValues1 == __nullptr || ipValues2 == __nullptr)
			{
				bMatch = false;
				break;
			}

			// Get the sizes
			long lSize1 = ipValues1->Size();
			long lSize2 = ipValues2->Size();

			// Create and initialize vector to cache the values of the second collection
			vector<pair<bool, AttributeData>> vecCachedValues;
			vecCachedValues.resize(lSize2);
			for (long i=0; i < lSize2; i++)
			{
				AttributeData attrTemp;
				vecCachedValues[i] = pair<bool, AttributeData>(false, attrTemp);
			}

			// For each value in the first collection, search for a match in the second collection
			for (long i=0; i < lSize1; i++)
			{
				// Get the attribute
				IAttributePtr ipAttr1 = ipValues1->At(i);
				ASSERT_RESOURCE_ALLOCATION("ELI26402", ipAttr1 != __nullptr);

				// Get the value
				ISpatialStringPtr ipString1 = ipAttr1->Value;
				ASSERT_RESOURCE_ALLOCATION("ELI26403", ipString1 != __nullptr);

				// Get the strings and check case sensitivity
				AttributeData attr1(asString(ipString1->String), asString(ipAttr1->Type));
				if (!m_bCaseSensitive)
				{
					makeUpperCase(attr1.value);
					makeUpperCase(attr1.type);
				}

				// Now check if the first attribute matches any attribute in the second collection
				bool bInnerMatch = false;
				for (long j=0; j < lSize2; j++)
				{
					// Get the value from the cached collection
					AttributeData& attr2 = vecCachedValues[j].second;

					// Check if the cached value for this attribute exists
					if (!vecCachedValues[j].first)
					{
						// Build the value (get the attribute)
						IAttributePtr ipAttr2 = ipValues2->At(j);
						ASSERT_RESOURCE_ALLOCATION("ELI26404", ipAttr2 != __nullptr);

						// Get the value
						ISpatialStringPtr ipString2 = ipAttr2->Value;
						ASSERT_RESOURCE_ALLOCATION("ELI26405", ipString2 != __nullptr);

						// Get the strings and check case sensitivity
						attr2.value = asString(ipString2->String);
						attr2.type = asString(ipAttr2->Type);
						if (!m_bCaseSensitive)
						{
							makeUpperCase(attr2.value);
							makeUpperCase(attr2.type);
						}

						// Set the cached value flag to true
						vecCachedValues[j].first = true;
					}

					// Check the values
					if (attr1.value == attr2.value
						&& (!m_bCompareTypeInfo || attr1.type == attr2.type))
					{
						// Found a match, no need to keep searching
						bInnerMatch = true;
						break;
					}
				}

				if (!bInnerMatch)
				{
					bMatch = false;
					break;
				}
			}
		}

		return bMatch;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26406");
}
//--------------------------------------------------------------------------------------------------
IAFUtilityPtr CMergeAttributeTrees::getAFUtility()
{
	// If the AFUtils object has not been created yet, create one
	if (m_ipAFUtils == __nullptr)
	{
		m_ipAFUtils.CreateInstance(CLSID_AFUtility);
		ASSERT_RESOURCE_ALLOCATION("ELI26397", m_ipAFUtils != __nullptr);
	}

	// Return the AFUtils object
	return m_ipAFUtils;
}
//--------------------------------------------------------------------------------------------------
void CMergeAttributeTrees::getAttributesToMoveOrRemove(IIUnknownVectorPtr ipToKeep,
												   IIUnknownVectorPtr ipToCheck,
												   vector<IAttributePtr> &rvecAttributesToChange)
{
	try
	{
		IAFUtilityPtr ipAFUtil = getAFUtility();

		IStrToObjectMapPtr ipKeepMap = ipAFUtil->GetNameToAttributesMap(ipToKeep);
		ASSERT_RESOURCE_ALLOCATION("ELI26416", ipKeepMap != __nullptr);
		IStrToObjectMapPtr ipCheckMap = ipAFUtil->GetNameToAttributesMap(ipToCheck);
		ASSERT_RESOURCE_ALLOCATION("ELI26417", ipCheckMap != __nullptr);

		// Perform the comparison to find the matching attributes and the non-matching
		// (since this function is called after attributes have already been compared and
		// while the merge is being performed, we know that there is a match in the list
		for(vector<string>::iterator it = m_vecSubattributesToCompare.begin();
			it != m_vecSubattributesToCompare.end(); it++)
		{
			// Get the collection of values that have this name
			_bstr_t bstrName(it->c_str());
			IIUnknownVectorPtr ipKeepList = ipKeepMap->TryGetValue(bstrName);
			ASSERT_RESOURCE_ALLOCATION("ELI26418", ipKeepList != __nullptr);
			IIUnknownVectorPtr ipCheckList = ipCheckMap->TryGetValue(bstrName);
			ASSERT_RESOURCE_ALLOCATION("ELI26419", ipCheckList != __nullptr);

			// Get the size of the collections
			long lKeepSize = ipKeepList->Size();
			long lCheckSize = ipCheckList->Size();
			for (long i=0; i < lKeepSize; i++)
			{
				// Get the first keep item from the list
				IAttributePtr ipKeep = ipKeepList->At(i);
				ASSERT_RESOURCE_ALLOCATION("ELI26420", ipKeep != __nullptr);

				// Get the first keep spatial string
				ISpatialStringPtr ipKeepValue = ipKeep->Value;
				ASSERT_RESOURCE_ALLOCATION("ELI26421", ipKeepValue != __nullptr);

				// Get the strings and check case sensitivity
				string strKeepType = asString(ipKeep->Type);
				string strKeepValue = asString(ipKeepValue->String);
				if (!m_bCaseSensitive)
				{
					makeUpperCase(strKeepType);
					makeUpperCase(strKeepValue);
				}

				// Now find the value in the to check collection
				bool bMatched = false;
				for (long j=0; j < lCheckSize; j++)
				{
					// Get the to check attribute
					IAttributePtr ipCheck = ipCheckList->At(j);
					ASSERT_RESOURCE_ALLOCATION("ELI26422", ipCheck != __nullptr);

					// Get the value
					ISpatialStringPtr ipCheckValue = ipCheck->Value;
					ASSERT_RESOURCE_ALLOCATION("ELI26423", ipCheckValue != __nullptr);

					// Get the strings and check case sensitivity
					string strCheckType = asString(ipCheck->Type);
					string strCheckValue = asString(ipCheckValue->String);
					if (!m_bCaseSensitive)
					{
						makeUpperCase(strCheckType);
						makeUpperCase(strCheckValue);
					}

					// If this is the matching value, remove it from the ToCheck collection
					if (strCheckValue == strKeepValue
						&& (!m_bCompareTypeInfo || strCheckType == strKeepType))
					{
						bMatched = true;
						ipCheckList->Remove(j);
						j--;
						lCheckSize--;

						// Remove the attribute from the to check collection since it matches
						ipToCheck->RemoveValue(ipCheck);
					}
					else
					{
						// Non-matching comparison - add it to the to change collection
						rvecAttributesToChange.push_back(ipCheck);
					}
				}

				// If this attribute did not match any, add it to the to change collection
				if (!bMatched)
				{
					rvecAttributesToChange.push_back(ipKeep);
				}
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26424");
}
//--------------------------------------------------------------------------------------------------
void CMergeAttributeTrees::removeMergedAttributes(IIUnknownVectorPtr ipAttributes,
												  const vector<IAttributePtr>& vecMergedAttributes)
{
	try
	{
		ASSERT_ARGUMENT("ELI26458", ipAttributes != __nullptr);

		// Get an AFUtility pointer
		IAFUtilityPtr ipAFUtils = getAFUtility();

		// Iterate the vector of attributes to be removed (the sub-attributes that were merged)
		// and remove them from the attribute collection
		for (vector<IAttributePtr>::const_iterator it = vecMergedAttributes.begin();
			it != vecMergedAttributes.end(); it++)
		{
			// Create a null parent attribute
			IAttributePtr ipParent = __nullptr;

			// If removing empty hierarchy, check for a parent attribute
			if (m_bRemoveEmptyHierarchy)
			{
				ipParent = ipAFUtils->GetAttributeParent(ipAttributes, (*it));
			}

			ipAFUtils->RemoveAttribute(ipAttributes, (*it));

			// If the parent has been set then remove the hierarchy
			if (ipParent != __nullptr)
			{
				removeEmptyAttributesFromHierarchy(ipAttributes, ipParent);
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26428");
}
//--------------------------------------------------------------------------------------------------
void CMergeAttributeTrees::removeEmptyAttributesFromHierarchy(IIUnknownVectorPtr ipAttributes,
															 IAttributePtr ipParent)
{
	try
	{
		ASSERT_ARGUMENT("ELI26451", ipAttributes != __nullptr);
		ASSERT_ARGUMENT("ELI26452", ipParent != __nullptr);

		// Get the sub attributes
		IIUnknownVectorPtr ipSubs = ipParent->SubAttributes;
		ASSERT_RESOURCE_ALLOCATION("ELI26453", ipSubs != __nullptr);

		// If the sub attributes are empty then remove the parent
		if (ipSubs->Size() == 0)
		{
			// Get the grand parent attribute
			IAFUtilityPtr ipAFUtils = getAFUtility();
			IAttributePtr ipGrandParent = ipAFUtils->GetAttributeParent(ipAttributes, ipParent);

			// Remove the parent attribute
			ipAFUtils->RemoveAttribute(ipAttributes, ipParent);

			// If the grand parent is not null then check if this is now empty
			if (ipGrandParent != __nullptr)
			{
				removeEmptyAttributesFromHierarchy(ipAttributes, ipGrandParent);
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26454");
}
//--------------------------------------------------------------------------------------------------