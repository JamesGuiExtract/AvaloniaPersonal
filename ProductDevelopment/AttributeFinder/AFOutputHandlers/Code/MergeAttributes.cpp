// MergeAttributes.cpp : Implementation of CMergeAttributes

#include "stdafx.h"
#include "MergeAttributes.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
// Version 2: Added CreateMergedRegion. Default is false, however when version 1 files are loaded it
// will default to true to maintain behavior for version 1 rule objects.
const unsigned long gnCurrentVersion			= 2;
const unsigned long gnDEFAULT_OVERLAP_PERCENT	= 75;
const CRect grectNULL							= CRect(0, 0, 0, 0);
const _bstr_t gbstrNEEDS_NAME					= "___NEEDS_NAME___";

//--------------------------------------------------------------------------------------------------
// CMergeAttributes
//--------------------------------------------------------------------------------------------------
CMergeAttributes::CMergeAttributes()
: m_bDirty(false)
, m_ipDocText(NULL)
{
	try
	{
		reset();

		m_ipAFUtility.CreateInstance(CLSID_AFUtility);
		ASSERT_RESOURCE_ALLOCATION("ELI22876", m_ipAFUtility != NULL);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI22730");
}
//--------------------------------------------------------------------------------------------------
CMergeAttributes::~CMergeAttributes()
{
	try
	{
		m_ipAFUtility = NULL;
		m_ipNameMergePriority = NULL;
		m_mapChildToParentAttributes.clear();
		m_mapAttributeInfo.clear();
		m_mapSpatialInfos.clear();
		m_ipDocText = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI22731");
}
//-------------------------------------------------------------------------------------------------
HRESULT CMergeAttributes::FinalConstruct()
{
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
void CMergeAttributes::FinalRelease()
{
}

//--------------------------------------------------------------------------------------------------
// IMergeAttributes
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::get_AttributeQuery(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22809", pVal != NULL);

		validateLicense();
		
		*pVal = get_bstr_t(m_strAttributeQuery).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22810")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::put_AttributeQuery(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22941", !asString(newVal).empty());

		validateLicense();

		m_strAttributeQuery = asString(newVal);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22811")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::get_OverlapPercent(double *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22814", pVal != NULL);

		validateLicense();
		
		*pVal = m_dOverlapPercent;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22815")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::put_OverlapPercent(double newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22915", newVal >= 0 && newVal <= 100);

		validateLicense();

		m_dOverlapPercent = newVal;

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22816")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::get_NameMergeMode(EFieldMergeMode *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22817", pVal != NULL);

		validateLicense();
		
		*pVal = m_eNameMergeMode;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22818")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::put_NameMergeMode(EFieldMergeMode newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22931", newVal == kSpecifyField || newVal == kPreserveField);

		validateLicense();

		m_eNameMergeMode = newVal;

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22819")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::get_TypeMergeMode(EFieldMergeMode *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22820", pVal != NULL);

		validateLicense();
		
		*pVal = m_eTypeMergeMode;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22821")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::put_TypeMergeMode(EFieldMergeMode newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22930", newVal == kSpecifyField || newVal == kCombineField);

		validateLicense();

		m_eTypeMergeMode = newVal;

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22822")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::get_SpecifiedName(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22823", pVal != NULL);

		validateLicense();
		
		*pVal = get_bstr_t(m_strSpecifiedName).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22824")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::put_SpecifiedName(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_strSpecifiedName = asString(newVal);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22825")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::get_SpecifiedType(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22826", pVal != NULL);

		validateLicense();
		
		*pVal = get_bstr_t(m_strSpecifiedType).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22827")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::put_SpecifiedType(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_strSpecifiedType = asString(newVal);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22828")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::get_SpecifiedValue(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22905", pVal != NULL);

		validateLicense();
		
		*pVal = get_bstr_t(m_strSpecifiedValue).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22906")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::put_SpecifiedValue(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22914", !asString(newVal).empty());

		validateLicense();

		m_strSpecifiedValue = asString(newVal);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22907")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::get_NameMergePriority(IVariantVector **ppVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22829", ppVal != NULL);

		validateLicense();
		
		IVariantVectorPtr ipShallowCopy = m_ipNameMergePriority;
		*ppVal = ipShallowCopy.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22830")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::put_NameMergePriority(IVariantVector *pNewVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22842", pNewVal != NULL);

		validateLicense();

		m_ipNameMergePriority = pNewVal;

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22831")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::get_PreserveAsSubAttributes(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI22911", pVal != NULL);

		validateLicense();
		
		*pVal = asVariantBool(m_bPreserveAsSubAttributes);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22912")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::put_PreserveAsSubAttributes(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bPreserveAsSubAttributes = asCppBool(newVal);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22913")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::get_CreateMergedRegion(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI30839", pVal != NULL);

		validateLicense();
		
		*pVal = asVariantBool(m_bCreateMergedRegion);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI30840")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::put_CreateMergedRegion(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bCreateMergedRegion = asCppBool(newVal);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI30841")
}

//--------------------------------------------------------------------------------------------------
// IOutputHandler
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::raw_ProcessOutput(IIUnknownVector* pAttributes, IAFDocument *pAFDoc,
												 IProgressStatus *pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		IIUnknownVectorPtr ipAttributes(pAttributes);
		ASSERT_ARGUMENT("ELI22877", ipAttributes != NULL);

		IAFDocumentPtr ipAFDocument(pAFDoc);
		ASSERT_ARGUMENT("ELI25276", ipAFDocument != NULL);

		m_ipDocText = ipAFDocument->Text;
		ASSERT_RESOURCE_ALLOCATION("ELI25277", m_ipDocText != NULL);

		// Ensure m_mapChildToParentAttributes is empty before starting processing
		m_mapChildToParentAttributes.clear();

		// Ensure the attribute info cache is cleared.
		m_mapAttributeInfo.clear();

		// Ensure the standardized page info cache is cleared.
		m_mapSpatialInfos.clear();

		// Obtain the set of attributes eligible for merging
		IIUnknownVectorPtr ipTargetAttributes = 
			m_ipAFUtility->QueryAttributes(ipAttributes, m_strAttributeQuery.c_str(), VARIANT_FALSE);
		ASSERT_RESOURCE_ALLOCATION("ELI22883", ipTargetAttributes != NULL);

		IIUnknownVectorPtr ipMergeResults(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI23042", ipMergeResults);

		// Load original candidates into the cache here.
		long nTargetSize = ipTargetAttributes->Size();
		for (long i = 0; i < nTargetSize; i++)
		{
			// Retrieve the pair of attributes to test.
			IAttributePtr ipAttribute = ipTargetAttributes->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI30971", ipAttribute != NULL);

			// [FlexIDSCore:3328] Don't attempt to process non-spatial attributes 
			ISpatialStringPtr ipValue = ipAttribute->Value;
			if (ipValue != NULL && asCppBool(ipValue->HasSpatialInfo()))
			{
				loadAttributeInfo(ipAttribute);
			}
		}

		// Cycle through the target attributes every possible pair for merging.
		// Get the count of target attributes with each iteration since new attributes may be added
		// as processing progresses.
		for (long i = 0; i < nTargetSize - 1; i++)
		{
			// Retrieve the pair of attributes to test.
			IAttributePtr ipAttribute1 = ipTargetAttributes->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI23008", ipAttribute1 != NULL);

			// [FlexIDSCore:3328] Don't attempt to process non-spatial attributes 
			if (m_mapAttributeInfo.find(ipAttribute1) == m_mapAttributeInfo.end())
			{
				continue;
			}

			for (long j = i + 1; j < nTargetSize; j++)
			{
				IAttributePtr ipAttribute2 = ipTargetAttributes->At(j);
				ASSERT_RESOURCE_ALLOCATION("ELI23009", ipAttribute2 != NULL);

				// [FlexIDSCore:3328] Don't attempt to process non-spatial attributes 
				ISpatialStringPtr ipValue2 = ipAttribute2->Value;
				if (ipValue2 == NULL || !asCppBool(ipValue2->HasSpatialInfo()))
				{
					continue;
				}

				// Retrieves the set of pages in which the attributes overlap by the 
				// specified amount.
				set<long> setPagesWithOverlap = getPagesWithOverlap(ipAttribute1, ipAttribute2);

				for each (long nPage in setPagesWithOverlap)
				{
					IAttributePtr ipAttributeToRemove = NULL;

					// For each page that there is sufficient overlap, merge the attributes.
					IAttributePtr ipMergedAttribute = mergeAttributes(ipAttribute1, ipAttribute2, 
						nPage, ipAttributeToRemove);

					if (ipMergedAttribute != NULL)
					{
						// Add the merge result to the list of merged results and the list of 
						// attributes eligible for merging.
						ipMergeResults->PushBackIfNotContained(ipMergedAttribute);
						ipTargetAttributes->PushBackIfNotContained(ipMergedAttribute);
						nTargetSize = ipTargetAttributes->Size();

						// Restart the j iteration in case the merged result would now merge with
						// a value between i and j.
						j = i;
					}
					
					// It is possible both these attributes were already merged in which case
					// one of them will have been removed from the lists. 
					if (ipAttributeToRemove != NULL)
					{
						// Remove this attribute from ipMergeResults
						removeAttribute(ipAttributeToRemove, ipMergeResults, nPage);
						
						// Remove this attribute from ipTargetAttributes
						long nTargetIndex = removeAttribute(ipAttributeToRemove, ipTargetAttributes, nPage);
						nTargetSize--;
						
						// Adjust the current indexes appropriately.
						i -= (nTargetIndex <= i) ? 1 : 0;
						j -= (nTargetIndex <= j) ? 1 : 0;
					}
				}
			}
		}

		// Remove results that were unable to be named using the name priority list.
		removeInvalidResults(ipMergeResults);

		// Apply the merged results to the rule's output
		applyResults(ipMergeResults, ipAttributes);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22732")
}

//--------------------------------------------------------------------------------------------------
// IPersistStream
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI22733", pClassID != NULL);

		*pClassID = CLSID_MergeAttributes;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22734");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		return m_bDirty ? S_OK : S_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22736");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		ASSERT_ARGUMENT("ELI22737", pStream != NULL);

		// Reset data members
		reset();
		
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
			UCLIDException ue("ELI22738", 
				"Unable to load newer merge attributes rule!");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		// Read data members from the stream
		dataReader >> m_strAttributeQuery;
		dataReader >> m_dOverlapPercent;
		long nTemp;
		dataReader >> nTemp;
		m_eNameMergeMode = (EFieldMergeMode) nTemp;
		dataReader >> nTemp;
		m_eTypeMergeMode = (EFieldMergeMode) nTemp;
		dataReader >> m_strSpecifiedName;
		dataReader >> m_strSpecifiedType;
		dataReader >> m_strSpecifiedValue;
		dataReader >> m_bPreserveAsSubAttributes;
		if (nDataVersion == 1)
		{
			// For old version of the rule, set m_bCreateMergedRegion = true so that the rule's
			// behavior is not changed.
			m_bCreateMergedRegion = true;
		}
		else
		{
			dataReader >> m_bCreateMergedRegion;
		}

		// Clone the NameMergePriority member
		IPersistStreamPtr ipNameMergePriority;
		readObjectFromStream(ipNameMergePriority, pStream, "ELI22844");
		m_ipNameMergePriority = ipNameMergePriority;

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22739");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		ASSERT_ARGUMENT("ELI22740", pStream != NULL);

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		// Write the current version
		dataWriter << gnCurrentVersion;

		// Write the data members to the stream
		dataWriter << m_strAttributeQuery;
		dataWriter << m_dOverlapPercent;
		dataWriter << (long) m_eNameMergeMode;
		dataWriter << (long) m_eTypeMergeMode;
		dataWriter << m_strSpecifiedName;
		dataWriter << m_strSpecifiedType;
		dataWriter << m_strSpecifiedValue;
		dataWriter << m_bPreserveAsSubAttributes;

		dataWriter << m_bCreateMergedRegion;

		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		// Write the NameMergePriority list to the stream
		IPersistStreamPtr ipNameMergePriority(m_ipNameMergePriority);
		ASSERT_RESOURCE_ALLOCATION("ELI22846", ipNameMergePriority != NULL);
		writeObjectToStream(ipNameMergePriority, pStream, "ELI22847", fClearDirty);

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22741");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}

//--------------------------------------------------------------------------------------------------
// ICategorizedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::raw_GetComponentDescription(BSTR *pbstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI22742", pbstrComponentDescription != NULL)

		*pbstrComponentDescription = _bstr_t("Merge attributes").Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22743")
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI22744", pbValue != NULL);

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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22745");
}

//--------------------------------------------------------------------------------------------------
// ICopyableObject
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license
		validateLicense();

		UCLID_AFOUTPUTHANDLERSLib::IMergeAttributesPtr ipCopyThis = pObject;
		ASSERT_ARGUMENT("ELI22746", ipCopyThis != NULL);

		// Copy members
		m_strAttributeQuery			= asString(ipCopyThis->AttributeQuery);
		m_dOverlapPercent			= ipCopyThis->OverlapPercent;
		m_eNameMergeMode			= (EFieldMergeMode) ipCopyThis->NameMergeMode;
		m_eTypeMergeMode			= (EFieldMergeMode) ipCopyThis->TypeMergeMode;
		m_strSpecifiedName			= asString(ipCopyThis->SpecifiedName);
		m_strSpecifiedType			= asString(ipCopyThis->SpecifiedType);
		m_strSpecifiedValue			= asString(ipCopyThis->SpecifiedValue);
		m_bPreserveAsSubAttributes	= asCppBool(ipCopyThis->PreserveAsSubAttributes);
		m_bCreateMergedRegion		= asCppBool(ipCopyThis->CreateMergedRegion);

		// Clone the NameMergePriority member
		ICopyableObjectPtr ipCopyableNameMergePriority = ipCopyThis->NameMergePriority;
		ASSERT_RESOURCE_ALLOCATION("ELI22836", ipCopyableNameMergePriority != NULL);
		m_ipNameMergePriority = ipCopyableNameMergePriority->Clone();
		ASSERT_RESOURCE_ALLOCATION("ELI22837", m_ipNameMergePriority != NULL);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22747");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license
		validateLicense();

		ASSERT_ARGUMENT("ELI22748", pObject != NULL);

		// Create another instance of this object
		ICopyableObjectPtr ipObjCopy(CLSID_MergeAttributes);
		ASSERT_RESOURCE_ALLOCATION("ELI22749", ipObjCopy != NULL);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);
	
		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22750");
}

//--------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::raw_IsConfigured(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Check parameter
		ASSERT_ARGUMENT("ELI22751", pbValue != NULL);

		*pbValue = VARIANT_TRUE;

		if (m_strAttributeQuery.empty())
		{
			// An attribute query must be specified.
			*pbValue =  VARIANT_FALSE;
		}
		else if (m_strSpecifiedValue.empty())
		{
			// The value for merge results must be specified.
			*pbValue =  VARIANT_FALSE;
		}
		else if (m_dOverlapPercent < 0 || m_dOverlapPercent > 100)
		{
			// The overlap percent must be between 0 and 100
			*pbValue =  VARIANT_FALSE;
		}
		else if (m_eNameMergeMode != kSpecifyField && m_eNameMergeMode != kPreserveField)
		{
			// The name can be specified only via direct specification, or via preservation.
			*pbValue =  VARIANT_FALSE;
		}
		else if (m_eTypeMergeMode != kSpecifyField && m_eTypeMergeMode != kCombineField)
		{
			// The type can be specified only via direct specification, or combination.
			*pbValue =  VARIANT_FALSE;
		}
		else if (m_eNameMergeMode == kSpecifyField && m_strSpecifiedName.empty())
		{
			// If using name specification, the name specification field must be filled.
			*pbValue =  VARIANT_FALSE;
		}
		else if (m_eNameMergeMode == kPreserveField && m_ipNameMergePriority->Size == 0)
		{
			// If using name preservation, at least one entry must be in the name preservation list.
			*pbValue =  VARIANT_FALSE;
		}
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22752");
}

//---------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//---------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributes::InterfaceSupportsErrorInfo(REFIID riid)
{
	try
	{
		static const IID* arr[] = 
		{
			&IID_IMergeAttributes,
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22753")
}

//--------------------------------------------------------------------------------------------------
// Private Methods
//--------------------------------------------------------------------------------------------------
set<long> CMergeAttributes::getPagesWithOverlap(IAttributePtr ipAttribute1, 
												IAttributePtr ipAttribute2)
{
	ASSERT_ARGUMENT("ELI22889", ipAttribute1 != NULL);
	ASSERT_ARGUMENT("ELI22890", ipAttribute2 != NULL);

	set<long> setPagesWithOverlap;

	// Retrieve the set of pages that are common to these attributes.
	set<long> setPages = getAttributePages(ipAttribute1, ipAttribute2);

	for each (long nPage in setPages)
	{
		// On each common page, see if there is enough mutual ovelap to merge the attributes.
		if (calculateOverlap(ipAttribute1, ipAttribute2, nPage) >= m_dOverlapPercent)
		{
			setPagesWithOverlap.insert(nPage);
		}
	}

	return setPagesWithOverlap;
}
//--------------------------------------------------------------------------------------------------
double CMergeAttributes::calculateOverlap(IAttributePtr ipAttribute1, IAttributePtr ipAttribute2,
										  long nPage)
{
	ASSERT_ARGUMENT("ELI22982", ipAttribute1 != NULL);
	ASSERT_ARGUMENT("ELI22983", ipAttribute2 != NULL);

	long nTotalIntersection = 0;
	long nTotalArea1 = 0;
	long nTotalArea2 = 0;

	// Compare every raster zone from each attribute to every raster zone from the other attribute.
	// Compile the total area of intersection as well as the total area of each attribute.
	for each (CRect rect1 in m_mapAttributeInfo[ipAttribute1].mapRasterZones[nPage])
	{
		for each (CRect rect2 in m_mapAttributeInfo[ipAttribute2].mapRasterZones[nPage])
		{
			// If these rects intersect, add the rect's area to the total area of intersection.
			CRect rectIntersection;
			if (rectIntersection.IntersectRect(rect1, rect2))
			{
				nTotalIntersection += rectIntersection.Width() * rectIntersection.Height();
			}

			// The first time through vec2Rects, calculate the total area of attribute 2
			if (nTotalArea1 == 0)
			{
				nTotalArea2 += rect2.Width() * rect2.Height();
			}
		}

		// Calculate the total area of attribute 1
		nTotalArea1 += rect1.Width() * rect1.Height();
	}

	// Ensure both attributes appear valid (ensure division by zero doesn't occur)
	if (nTotalArea1 == 0 || nTotalArea2 == 0)
	{
		return 0;
	}

	// Return the largest percentage of mutual overlap. [FlexIDSCore #3509]
	return 100.0 * (double) nTotalIntersection / (double) min(nTotalArea1, nTotalArea2);
}
//--------------------------------------------------------------------------------------------------
void CMergeAttributes::loadAttributeInfo(IAttributePtr ipAttribute)
{
	ASSERT_ARGUMENT("ELI23055", ipAttribute != NULL);

	m_mapAttributeInfo[ipAttribute].setPages.clear();
	m_mapAttributeInfo[ipAttribute].mapRasterZones.clear();

	// Obtain the spatial string value of both attributes.
	ISpatialStringPtr ipValue = ipAttribute->Value;
	ASSERT_RESOURCE_ALLOCATION("ELI23056", ipValue != NULL);

	// Retrieve the first and last page number of each.
	long nFirstPage = ipValue->GetFirstPageNumber();
	long nLastPage = ipValue->GetLastPageNumber();

	for (long nPage = nFirstPage; nPage <= nLastPage; nPage++)
	{
		// Retrieve the specific page we need.
		ISpatialStringPtr ipPage = ipValue->GetSpecifiedPages(nPage, nPage);
		ASSERT_RESOURCE_ALLOCATION("ELI22957", ipPage != NULL);

		if (!asCppBool(ipPage->HasSpatialInfo()))
		{
			// If this page has no spatial information, it has no raster zones.
			continue;
		}

		// Use raster zones to retrieve the spatial information.
		IIUnknownVectorPtr ipRasterZones =
			ipPage->GetTranslatedImageRasterZones(m_ipDocText->SpatialPageInfos);
		ASSERT_RESOURCE_ALLOCATION("ELI22904", ipRasterZones != NULL);

		// Cycle through each raster zone and obtain a CRect representing each.
		long nCount = ipRasterZones->Size();
		
		ILongRectanglePtr ipPageBounds = NULL;
		if (nCount > 0)
		{
			// Get the page bounds (for use by GetRectangularBounds).
			// NOTE: All zones will be on the same page, so we only need to get the bounds once.
			ipPageBounds = ipPage->GetOCRImageBounds();
			ASSERT_RESOURCE_ALLOCATION("ELI30312", ipPageBounds != NULL);
		}

		for (long i = 0; i < nCount; i++)
		{
			IRasterZonePtr ipRasterZone = ipRasterZones->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI22881", ipRasterZone != NULL);

			// Obtain a rect describing the location of this raster zone.
			ILongRectanglePtr ipRect = ipRasterZone->GetRectangularBounds(ipPageBounds);
			ASSERT_RESOURCE_ALLOCATION("ELI22882", ipRect != NULL);

			// Copy the ILongRectangle to a CRect to make rect comparisons easier.
			CRect rectBounds;
			ipRect->GetBounds(&(rectBounds.left), &(rectBounds.top),
				&(rectBounds.right), &(rectBounds.bottom));

			m_mapAttributeInfo[ipAttribute].mapRasterZones[nPage].push_back(rectBounds);
		}

		if (nCount > 0)
		{
			// Add this page to attribute's page set only after it is confirmed it contains raster zones
			m_mapAttributeInfo[ipAttribute].setPages.insert(nPage);
		}
	}
}
//--------------------------------------------------------------------------------------------------
set<long> CMergeAttributes::getAttributePages(IAttributePtr ipAttribute1, IAttributePtr ipAttribute2)
{
	ASSERT_ARGUMENT("ELI22952", ipAttribute1 != NULL);
	ASSERT_ARGUMENT("ELI22953", ipAttribute2 != NULL);

	set<long> setPages;

	if (m_mapAttributeInfo[ipAttribute1].setPages.empty() ||
		m_mapAttributeInfo[ipAttribute2].setPages.empty())
	{
		// If either attribute has no pages, return an empty set.
		return setPages;
	}

	// Get the first & last page of each attribute
	long nFirst1 = *(m_mapAttributeInfo[ipAttribute1].setPages.begin());
	long nFirst2 = *(m_mapAttributeInfo[ipAttribute2].setPages.begin());
	long nLast1 = *(--m_mapAttributeInfo[ipAttribute1].setPages.end());
	long nLast2 = *(--m_mapAttributeInfo[ipAttribute2].setPages.end());

	// Return every page starting with the greater of the "first" page values
	// and ending with the lesser of the "last" page values.
	for (int i = max(nFirst1, nFirst2); i <= min(nLast1, nLast2); i++)
	{
		setPages.insert(i);
	}

	return setPages;
}
//--------------------------------------------------------------------------------------------------
IAttributePtr CMergeAttributes::mergeAttributes(IAttributePtr ipAttribute1, 
												IAttributePtr ipAttribute2, long nPage,
												IAttributePtr &ripAttributeToRemove)
{
	ASSERT_ARGUMENT("ELI22887", ipAttribute1 != NULL);
	ASSERT_ARGUMENT("ELI22888", ipAttribute2 != NULL);

	ripAttributeToRemove = NULL;

	// Provides an attribute to merge into and determine which two attributes need to be merged 
	// (if either provided attribute has already been merged, the attribute pointer(s) will be 
	// changed to reflect the merge result it is already a part of).
	IAttributePtr ipMergedResult = getMergeTarget(ipAttribute1, ipAttribute2, nPage);

	if (ipMergedResult == NULL)
	{
		// These two attributes have already been merged into the same result.
		return NULL;
	}

	// Merge ipAttribute1 and ipAttribute2 into ipMergedResult
	mergeAttributePair(ipMergedResult, ipAttribute1, ipAttribute2, nPage);

	if (ipAttribute1 != ipMergedResult)
	{
		// If ipAttribute1 isn't the merge result, re-associate it and any of its associated 
		// attributes with ipMergedResult.
		if (associateAttributeWithResult(ipAttribute1, ipMergedResult, nPage))
		{
			// ipAttribute1 is no longer needed
			ripAttributeToRemove = ipAttribute1;
		}
	}

	if (ipAttribute2 != ipMergedResult)
	{
		if (ripAttributeToRemove != NULL)
		{
			// This would mean ipAttribute1 was an existing merge result (but not ipMergedResult).
			// If ipAttribute2 is not a merge result ipAttribute1 should have been ipMergedResult.
			// If ipAttribute2 is a merge result it it should be ipMergedResult, but is not.
			THROW_LOGIC_ERROR_EXCEPTION("ELI23017");
		}

		// If ipAttribute2 isn't the merge result, re-associate it and any of its associated 
		// attributees with ipMergedResult.
		if (associateAttributeWithResult(ipAttribute2, ipMergedResult, nPage))
		{
			// ipAttribute2 is no longer needed
			ripAttributeToRemove = ipAttribute2;
		}
	}

	return ipMergedResult;
}
//--------------------------------------------------------------------------------------------------
IAttributePtr CMergeAttributes::getMergeTarget(IAttributePtr &ripAttribute1, 
											   IAttributePtr &ripAttribute2, long nPage)
{
	// The attribute to merge into
	IAttributePtr ipMergedAttribute = NULL;

	// See if ripAttribute1 has already been merged into another result.
	AttributeMap::iterator iterAttribute1Parent
		= m_mapChildToParentAttributes.find(AttributePage(ripAttribute1, nPage));

	if (iterAttribute1Parent != m_mapChildToParentAttributes.end())
	{
		// ripAttribute1 has been merged into another result-- see if that result is 
		// on this page.
		IAttributePtr ipParentAttribute = iterAttribute1Parent->second;
		ASSERT_RESOURCE_ALLOCATION("ELI22993", ipParentAttribute != NULL);

		// If the result is on this page, merge into that existing result
		ipMergedAttribute = ipParentAttribute;
		ripAttribute1 = ipParentAttribute;
	}

	// See if ripAttribute2 has already been merged into another result.
	AttributeMap::iterator iterAttribute2Parent
		= m_mapChildToParentAttributes.find(AttributePage(ripAttribute2, nPage));

	if (iterAttribute2Parent != m_mapChildToParentAttributes.end())
	{
		// ripAttribute2 has been merged into another result-- see if that result is 
		// on this page.
		IAttributePtr ipParentAttribute = iterAttribute2Parent->second;
		ASSERT_RESOURCE_ALLOCATION("ELI22994", ipParentAttribute != NULL);

		if (ipParentAttribute == ipMergedAttribute)
		{
			// If the result in on this page and matches the existing result of ipAttribute1,
			// these two attributes have already been merged; return NULL to indicate
			// nothing needs to be done.
			return NULL;
		}

		// If the result is on this page, merge into that existing result
		ipMergedAttribute = ipParentAttribute;
		ripAttribute2 = ipParentAttribute;
	}

	// If neither attribute is associated with an existing search result, 
	if (ipMergedAttribute == NULL)
	{
		ipMergedAttribute.CreateInstance(CLSID_Attribute);
		ASSERT_RESOURCE_ALLOCATION("ELI22891", ipMergedAttribute != NULL);
	}
	
	return ipMergedAttribute;
}
//--------------------------------------------------------------------------------------------------
void CMergeAttributes::mergeAttributePair(IAttributePtr ipMergedAttribute, 
										  IAttributePtr ipAttribute1, 
										  IAttributePtr ipAttribute2, long nPage)
{
	ASSERT_ARGUMENT("ELI22976", ipMergedAttribute != NULL);
	ASSERT_ARGUMENT("ELI22977", ipAttribute1 != NULL);
	ASSERT_ARGUMENT("ELI22978", ipAttribute2 != NULL);

	// Assign value for the merged attribute
	ipMergedAttribute->Value = createMergedValue(ipAttribute1, ipAttribute2, nPage);

	if (m_eNameMergeMode == kSpecifyField)
	{
		// Use the m_strSpecifiedName for the attribute name
		ipMergedAttribute->Name = _bstr_t(m_strSpecifiedName.c_str());
	}
	else if (m_eNameMergeMode == kPreserveField)
	{
		// Set the attribute name by using one of the existing names & m_ipNameMergePriority.
		_bstr_t bstrName;
		if (!getValueToPreserve(ipAttribute1->Name, ipAttribute2->Name, m_ipNameMergePriority, bstrName))
		{
			ipMergedAttribute->Name = gbstrNEEDS_NAME;
		}
		else
		{
			ipMergedAttribute->Name = bstrName;
		}
	}
	else
	{
		// An invalid EMergeMode is being used.
		THROW_LOGIC_ERROR_EXCEPTION("ELI22928");
	}

	if (m_eTypeMergeMode == kSpecifyField)
	{
		// Use the m_strSpecifiedType for the attribute type
		ipMergedAttribute->Type = _bstr_t(m_strSpecifiedType.c_str());
	}
	else if (m_eTypeMergeMode == kCombineField)
	{
		// Combine existing types.
		if (ipAttribute1->Type.length() > 0 && ipAttribute2->Type.length() > 0)
		{
			// Both attributes have existing types.  Use AddType to merge the type lists.
			ipMergedAttribute->AddType(ipAttribute1->Type);
			ipMergedAttribute->AddType(ipAttribute2->Type);
		}
		else if (ipAttribute1->Type.length() > 0)
		{
			// ipAttribute1 is the only attribute with an existing type; use it's type.
			ipMergedAttribute->Type = ipAttribute1->Type;
		}
		else if (ipAttribute2->Type.length() > 0)
		{
			// ipAttribute2 is the only attribute with an existing type; use it's type.
			ipMergedAttribute->Type = ipAttribute2->Type;
		}
	}
	else
	{
		// An invalid EMergeMode is being used.
		THROW_LOGIC_ERROR_EXCEPTION("ELI22929");
	}

	// Load the updated spatial info into the cache
	loadAttributeInfo(ipMergedAttribute);
}
//--------------------------------------------------------------------------------------------------
ISpatialStringPtr CMergeAttributes::createMergedValue(IAttributePtr ipAttribute1, 
													  IAttributePtr ipAttribute2, 
													  long nPage)
{
	ASSERT_ARGUMENT("ELI22963", ipAttribute1 != NULL);
	ASSERT_ARGUMENT("ELI22964", ipAttribute2 != NULL);

	// If an entry has not been created for the specified page in m_mapSpatialInfos, create one now.
	if (m_mapSpatialInfos.find(nPage) == m_mapSpatialInfos.end())
	{
		// We want to modify the existing page's PageInfo for the attribute, but we don't want
		// to affect the existing page, so obtain a copy.
		ICopyableObjectPtr ipCloneThis = m_ipDocText->GetPageInfo(nPage);
		ASSERT_RESOURCE_ALLOCATION("ELI25268", ipCloneThis != NULL);

		ISpatialPageInfoPtr ipPageInfoClone = ipCloneThis->Clone();
		ASSERT_RESOURCE_ALLOCATION("ELI25269", ipPageInfoClone != NULL);
		
		// Remove any skew and orientation since the retrieved bounding rectangles will be in terms
		// of literal page coordinates.
		ipPageInfoClone->Deskew = 0;
		ipPageInfoClone->Orientation = kRotNone;

		m_mapSpatialInfos[nPage] = ipPageInfoClone;
	}

	// Create a new spatial page info map from the map of standard page infos
	ILongToObjectMapPtr ipPageInfos(CLSID_LongToObjectMap);
	ASSERT_RESOURCE_ALLOCATION("ELI26682", ipPageInfos != NULL);
	for (map<long, ISpatialPageInfoPtr>::iterator it = m_mapSpatialInfos.begin();
		it != m_mapSpatialInfos.end(); it++)
	{
		ipPageInfos->Set(it->first, it->second);
	}

	ISpatialStringPtr ipMergedValue(CLSID_SpatialString);
	ASSERT_RESOURCE_ALLOCATION("ELI22892", ipMergedValue != NULL);

	IIUnknownVectorPtr ipMergedRasterZones(CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION("ELI30844", ipMergedRasterZones != NULL);

	if (m_bCreateMergedRegion)
	{
		// This will store the total unified area of all raster zones.
		CRect rectMergedRegion;
		
		// Obtain the total area of ipAttribute1 on this page.
		for each (CRect rect1 in m_mapAttributeInfo[ipAttribute1].mapRasterZones[nPage])
		{
			rectMergedRegion.UnionRect(rectMergedRegion, rect1);
		}
	
		// Unify that with the total area of ipAttribute2 on this page.
		for each (CRect rect2 in m_mapAttributeInfo[ipAttribute2].mapRasterZones[nPage])
		{
			rectMergedRegion.UnionRect(rectMergedRegion, rect2);
		}

		ipMergedRasterZones->PushBack(createRasterZone(rectMergedRegion, nPage));
	}
	// Merge the raster zones individually.
	else
	{
		ILongRectanglePtr ipPageBounds = m_ipDocText->GetOCRImagePageBounds(nPage);
		ASSERT_RESOURCE_ALLOCATION("ELI30972", ipPageBounds != NULL);

		// Create a vector of all raster zones in the two attributes.
		vector<CRect> vecZoneRects(m_mapAttributeInfo[ipAttribute1].mapRasterZones[nPage].begin(),
								   m_mapAttributeInfo[ipAttribute1].mapRasterZones[nPage].end());
		vecZoneRects.insert(vecZoneRects.end(),
							m_mapAttributeInfo[ipAttribute2].mapRasterZones[nPage].begin(),
							m_mapAttributeInfo[ipAttribute2].mapRasterZones[nPage].end());

		// Merge any overlapping raster zones.
		for (vector<CRect>::iterator i = vecZoneRects.begin(); i < vecZoneRects.end() - 1; i++)
		{
			for (vector<CRect>::iterator j = i + 1; j != vecZoneRects.end(); j++)
			{
				CRect rectIntersection;
				if (rectIntersection.IntersectRect(*i, *j))
				{
					i->UnionRect(*i, *j);

					// If i & j were merged into i, raster zone j is no longer needed.
					vecZoneRects.erase(j);
					j = i;
				}
			}

			ipMergedRasterZones->PushBack(createRasterZone(*i, nPage));
		}
	}

	// If there is only one resulting raster zone, create a psuedo-spatial string with
	// m_strSpecifiedValue as the text spread evenly across the entire area of the attribute.
	if (ipMergedRasterZones->Size() == 1)
	{
		IRasterZonePtr ipRasterZone = ipMergedRasterZones->At(0);
		ASSERT_RESOURCE_ALLOCATION("ELI30973", ipRasterZone != NULL);
	
		ipMergedValue->CreatePseudoSpatialString(ipRasterZone, m_strSpecifiedValue.c_str(),
			m_ipDocText->SourceDocName, m_ipDocText->SpatialPageInfos);
	}
	// Otherwise, create a hybrid result with m_strSpecifiedValue as the text value.
	else
	{
		ipMergedValue->CreateHybridString(ipMergedRasterZones, m_strSpecifiedValue.c_str(),
			m_ipDocText->SourceDocName, m_ipDocText->SpatialPageInfos);
	}

	return ipMergedValue;
}
//--------------------------------------------------------------------------------------------------
bool CMergeAttributes::getValueToPreserve(_bstr_t bstrValueA, _bstr_t bstrValueB, 
										IVariantVectorPtr ipValuePriorityList, _bstr_t &rbstrResult)
{
	ASSERT_ARGUMENT("ELI22896", ipValuePriorityList != NULL);

	rbstrResult = "";

	// Create CStrings representing both values for easy case-sensitive or non-case-sensitive
	// comparisons.
	CString zValueA(asString(bstrValueA).c_str());
	CString zValueB(asString(bstrValueB).c_str());

	// Iterate through the priority list looking for the first matching value. The loop will
	// make case-insensitive comparisons, but will attempt to use the list to resolve differences
	// in casing.
	long nCount = ipValuePriorityList->Size;
	for (long i = 0; i < nCount; i++)
	{
		CString zValue(asString(ipValuePriorityList->GetItem(i).bstrVal).c_str());
		
		if (zValue.CompareNoCase(zValueA) == 0)
		{
			// ValueA is a match
			rbstrResult = bstrValueA;

			if (zValue == zValueA)
			{
				// Return immediately with a case-sensitive match. Otherwise, check valueB
				// in case it is a case-sensitive match.
				return true;
			}
		}

		if (zValue.CompareNoCase(zValueB) == 0)
		{
			// ValueB is a match
			rbstrResult = bstrValueB;
		}

		if (rbstrResult.length() != 0)
		{
			// We have found a match.
			return true;
		}
	}

	if (zValueA.CompareNoCase(zValueB) == 0)
	{
		// If value A and B are equal, use this value. (Save this check for last to allow the list
		// to resolve case sensisitivity differences if possible).
		rbstrResult = bstrValueA;
		return true;
	}

	return false;
}
//--------------------------------------------------------------------------------------------------
bool CMergeAttributes::associateAttributeWithResult(IAttributePtr ipAttribute, 
													IAttributePtr ipMergedResult,
													long nPage)
{
	ASSERT_ARGUMENT("ELI23013", ipAttribute != NULL);
	ASSERT_ARGUMENT("ELI23014", ipMergedResult != NULL);

	bool bRemoveAttribute = false;

	// Get the existing sub-attribute list for the merge result.
	IIUnknownVectorPtr ipResultSubAttributes = ipMergedResult->SubAttributes;
	ASSERT_RESOURCE_ALLOCATION("ELI23005", ipResultSubAttributes != NULL);

	if (m_mapChildToParentAttributes[AttributePage(ipAttribute, nPage)] == ipAttribute)
	{
		// ipAttribute is a merge result itself; this merge result is no longer needed.
		bRemoveAttribute = true;

		if (m_bPreserveAsSubAttributes)
		{
			// Since we are preserving original attributes as sub-attributes, all sub-attributes
			// associated with ipAttribute need to be transfered to the merge result.
			IIUnknownVectorPtr ipSourceSubAttributes = ipAttribute->SubAttributes;
			ASSERT_RESOURCE_ALLOCATION("ELI22997", ipSourceSubAttributes != NULL);

			ipResultSubAttributes->Append(ipSourceSubAttributes);
		}
	
		// All attributes that contributed to ipAttribute need to be re-associated with the
		// merge result in m_mapChildToParentAttributes
		for (AttributeMap::iterator iter = m_mapChildToParentAttributes.begin();
			 iter != m_mapChildToParentAttributes.end();
			 iter++)
		{
			if (iter->second == ipAttribute)
			{
				m_mapChildToParentAttributes[iter->first] = ipMergedResult;
			}
		}
	}
	else if (m_bPreserveAsSubAttributes)
	{
		// ipAttribute was not an existing merge result.  Add it to the merge result as a 
		// sub-attribute if specified.
		ipResultSubAttributes->PushBackIfNotContained(ipAttribute);
	}

	// Associate the source attribute with the merge result.
	m_mapChildToParentAttributes[AttributePage(ipAttribute, nPage)] = ipMergedResult;
	// Associate the merge result with itself.
	m_mapChildToParentAttributes[AttributePage(ipMergedResult, nPage)] = ipMergedResult;

	return bRemoveAttribute;
}
//--------------------------------------------------------------------------------------------------
void CMergeAttributes::removeInvalidResults(IIUnknownVectorPtr ipMergeResults)
{
	ASSERT_ARGUMENT("ELI23054", ipMergeResults != NULL);
	
	// Loop through all merge results looking for ones that have been flagged as nameless.
	long nCount = ipMergeResults->Size();
	for (long i = 0; i < nCount; i++)
	{
		IAttributePtr ipMergeResult = ipMergeResults->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI23255", ipMergeResult != NULL);

		// Check to see if the attribute has been flagged as not having been assigned a name.
		if (ipMergeResult->Name == gbstrNEEDS_NAME)
		{
			UCLIDException ue("ELI22897", "Application trace: Merge attributes rule failed to "
				"merge attributes because no attribute's name matched a value in the name "
				"preservation priority list.");

			// For each attribute that contributed to this merged result, add its name as
			// debug info and invalidate its link in m_mapChildToParentAttributes to ensure
			// it remains in the rule's result.
			for (AttributeMap::iterator iter = m_mapChildToParentAttributes.begin();
				 iter != m_mapChildToParentAttributes.end();
				 iter++)
			{
				IAttributePtr ipAttribute = iter->first.first;
				ASSERT_RESOURCE_ALLOCATION("ELI23047", ipAttribute != NULL);

				// Check for attributes associated with the bad merge but that are not the bad
				// merge itself.
				if (iter->second == ipMergeResult && ipAttribute != ipMergeResult)
				{
					ue.addDebugInfo("Attribute Name", asString(ipAttribute->Name));

					// Invalidate this attribute's link in m_mapChildToParentAttributes 
					m_mapChildToParentAttributes[iter->first] = NULL;
				}
			}

			// Log the exception
			ue.log();

			// Remove the attribute from ipMergeResults
			removeAttribute(ipMergeResult, ipMergeResults);

			// Update the current index and count accordingly
			i--;
			nCount--;
		}
	}
}
//--------------------------------------------------------------------------------------------------
void CMergeAttributes::applyResults(IIUnknownVectorPtr ipMergeResults, 
									IIUnknownVectorPtr ipAttributes)
{
	ASSERT_ARGUMENT("ELI23040", ipMergeResults != NULL);
	ASSERT_ARGUMENT("ELI23041", ipAttributes != NULL);

	// Add the merge results to the rule's results
	ipAttributes->Append(ipMergeResults);

	// Now loop through all attributes involved in a merge, and remove any
	// that have been replaced by merge results on all the attribute's pages.
	AttributeMap::iterator iter = m_mapChildToParentAttributes.begin();
	while (!m_mapChildToParentAttributes.empty())
	{
		// Get the attribute to examine.
		IAttributePtr ipAttribute = iter->first.first;
		ASSERT_RESOURCE_ALLOCATION("ELI23046", ipAttribute != NULL);

		// We'll remove this attribute from the rule's results unless it is a merge
		// result itself or there is at least one page on which the attribute was
		// not merged.
		bool bRemoveFromResults = true;

		// Check all of the attribute's pages.
		for each (long nPage in m_mapAttributeInfo[ipAttribute].setPages)
		{
			AttributeMap::iterator iterAttributePage = 
				m_mapChildToParentAttributes.find(AttributePage(ipAttribute, nPage));

			if (iterAttributePage == m_mapChildToParentAttributes.end() ||
				iterAttributePage->second == NULL ||
				ipAttribute == iterAttributePage->second)
			{
				// We either found an attribute that is already a merge result or that was not
				// merged on one of its pages.
				bRemoveFromResults = false;
			}

			// Remove this entry from the m_mapChildToParentAttributes.
			if (iterAttributePage != m_mapChildToParentAttributes.end())
			{
				m_mapChildToParentAttributes.erase(iterAttributePage);
			}
		}

		// If necessary, remove this attribute from the resulting attribute list.
		if (bRemoveFromResults)
		{
			ipAttributes->RemoveValue(ipAttribute);
		}

		// Move to the next entry in m_mapChildToParentAttributes
		iter = m_mapChildToParentAttributes.begin();
	}
}
//--------------------------------------------------------------------------------------------------
long CMergeAttributes::removeAttribute(IAttributePtr ipAttribute, 
									   IIUnknownVectorPtr ipAttributeList, long nPage/* = -1*/)
{
	ASSERT_ARGUMENT("ELI23051", ipAttribute != NULL);
	ASSERT_ARGUMENT("ELI23052", ipAttributeList != NULL);

	set<long> setPages;

	if (nPage == -1)
	{
		setPages = m_mapAttributeInfo[ipAttribute].setPages;
	}
	else
	{
		setPages.insert(nPage);
	}

	// Remove the map entries
	for each (nPage in setPages);
	{
		m_mapChildToParentAttributes.erase(AttributePage(ipAttribute, nPage));
		m_mapAttributeInfo[ipAttribute].setPages.erase(nPage);
		m_mapAttributeInfo[ipAttribute].mapRasterZones[nPage].clear();
	}

	// Before removing the attribute from ipAttributeList, obtain its index in the list.
	long nIndex;
	ipAttributeList->FindByReference(ipAttribute, 0, &nIndex);

	if (nIndex < 0)
	{
		THROW_LOGIC_ERROR_EXCEPTION("ELI23053");
	}

	// Remove the attribute from the list
	ipAttributeList->Remove(nIndex);

	return nIndex;
}
//--------------------------------------------------------------------------------------------------
IRasterZonePtr CMergeAttributes::createRasterZone(CRect rect, long nPage)
{
	
	ILongRectanglePtr ipRect(CLSID_LongRectangle);
	ASSERT_RESOURCE_ALLOCATION("ELI22894", ipRect);
	ipRect->SetBounds(rect.left, rect.top, rect.right, rect.bottom);

	// Create a raster zone based on this rectangle.
	IRasterZonePtr ipRasterZone(CLSID_RasterZone);
	ASSERT_RESOURCE_ALLOCATION("ELI22895", ipRasterZone);

	ipRasterZone->CreateFromLongRectangle(ipRect, nPage);
	return ipRasterZone;
}
//--------------------------------------------------------------------------------------------------
void CMergeAttributes::reset()
{
	// Reset all the rules settings.
	m_strAttributeQuery.clear();
	m_dOverlapPercent = gnDEFAULT_OVERLAP_PERCENT;
	m_eNameMergeMode = kSpecifyField;
	m_eTypeMergeMode = kSpecifyField;
	m_strSpecifiedName.clear();
	m_strSpecifiedType.clear();
	m_strSpecifiedValue = "000-00-0000";
	m_bPreserveAsSubAttributes = true;
	m_ipDocText = NULL;
	m_mapSpatialInfos.clear();
	m_bCreateMergedRegion = false;

	m_ipNameMergePriority.CreateInstance(CLSID_VariantVector);
	ASSERT_RESOURCE_ALLOCATION("ELI22840", m_ipNameMergePriority != NULL);
}
//--------------------------------------------------------------------------------------------------
void CMergeAttributes::validateLicense()
{
	VALIDATE_LICENSE(gnFLEXINDEX_IDSHIELD_CORE_OBJECTS, "ELI22754", 
		"Merge attributes output handler");
}
//--------------------------------------------------------------------------------------------------