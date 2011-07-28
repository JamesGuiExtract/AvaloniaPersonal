#include "stdafx.h"
#include "SpatialAttributeMergeUtils.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
const CRect grectNULL							= CRect(0, 0, 0, 0);
const _bstr_t gbstrNEEDS_NAME					= "___NEEDS_NAME___";

//--------------------------------------------------------------------------------------------------
// CSpatialAttributeMergeUtils
//--------------------------------------------------------------------------------------------------
CSpatialAttributeMergeUtils::CSpatialAttributeMergeUtils()
: m_ipDocText(__nullptr)
, m_dOverlapPercent(75)
, m_bUseMutualOverlap(false)
, m_eNameMergeMode(kSpecifyField)
, m_eTypeMergeMode(kSpecifyField)
, m_strSpecifiedName("")
, m_strSpecifiedType("")
, m_strSpecifiedValue("000-00-0000")
, m_bPreserveAsSubAttributes(false)
, m_bCreateMergedRegion(false)
, m_bTreatNameListAsRegex(false)
, m_eValueMergeMode(kSpecifyField)
, m_bTreatValueListAsRegex(true)
, m_bTypeFromName(false)
, m_bPreserveType(false)
, m_bTreatTypeListAsRegex(false)
, m_ipNameMergePriority(__nullptr)
, m_ipValueMergePriority(__nullptr)
, m_ipTypeMergePriority(__nullptr)
, m_ipParser(__nullptr)
{
	try
	{
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI31768");
}
//--------------------------------------------------------------------------------------------------
CSpatialAttributeMergeUtils::~CSpatialAttributeMergeUtils()
{
	try
	{
		m_ipDocText = __nullptr;
		m_ipNameMergePriority = __nullptr;
		m_ipQualifiedMerges = __nullptr;
		m_mapChildToParentAttributes.clear();
		m_mapAttributeInfo.clear();
		m_mapSpatialInfos.clear();
		m_ipValueMergePriority = __nullptr;
		m_ipTypeMergePriority = __nullptr;
		m_ipParser = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI31769");
}
//-------------------------------------------------------------------------------------------------
HRESULT CSpatialAttributeMergeUtils::FinalConstruct()
{
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
void CSpatialAttributeMergeUtils::FinalRelease()
{
	try
	{
		m_ipDocText = __nullptr;
		m_ipNameMergePriority = __nullptr;
		m_ipQualifiedMerges = __nullptr;
		m_mapChildToParentAttributes.clear();
		m_mapAttributeInfo.clear();
		m_mapSpatialInfos.clear();
		m_ipValueMergePriority = __nullptr;
		m_ipTypeMergePriority = __nullptr;
		m_ipParser = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI32133");
}

//--------------------------------------------------------------------------------------------------
// ISpatialAttributeMergeUtils
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::get_OverlapPercent(double *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI32088", pVal != __nullptr);

		validateLicense();
		
		*pVal = m_dOverlapPercent;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32089")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::put_OverlapPercent(double newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI32090", newVal >= 0 && newVal <= 100.0);

		validateLicense();

		m_dOverlapPercent = newVal;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32091")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::get_UseMutualOverlap(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI32085", pVal != __nullptr);

		validateLicense();
		
		*pVal = asVariantBool(m_bUseMutualOverlap);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32086")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::put_UseMutualOverlap(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bUseMutualOverlap = asCppBool(newVal);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32087")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::get_NameMergeMode(EFieldMergeMode *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI32092", pVal != __nullptr);

		validateLicense();
		
		*pVal = m_eNameMergeMode;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32093")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::put_NameMergeMode(EFieldMergeMode newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI32094", newVal == kSpecifyField || newVal == kPreserveField);

		validateLicense();

		m_eNameMergeMode = newVal;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32095")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::get_TypeMergeMode(EFieldMergeMode *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI32096", pVal != __nullptr);

		validateLicense();
		
		*pVal = m_eTypeMergeMode;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32097")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::put_TypeMergeMode(EFieldMergeMode newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI32098", newVal == kSpecifyField || newVal == kCombineField
			|| newVal == kSelectField);

		validateLicense();

		m_eTypeMergeMode = newVal;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32099")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::get_SpecifiedName(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI32100", pVal != __nullptr);

		validateLicense();
		
		*pVal = get_bstr_t(m_strSpecifiedName).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32101")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::put_SpecifiedName(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_strSpecifiedName = asString(newVal);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32102")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::get_SpecifiedType(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI32103", pVal != __nullptr);

		validateLicense();
		
		*pVal = get_bstr_t(m_strSpecifiedType).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32104")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::put_SpecifiedType(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_strSpecifiedType = asString(newVal);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32105")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::get_SpecifiedValue(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI32106", pVal != __nullptr);

		validateLicense();
		
		*pVal = get_bstr_t(m_strSpecifiedValue).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32107")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::put_SpecifiedValue(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI32108", !asString(newVal).empty());

		validateLicense();

		m_strSpecifiedValue = asString(newVal);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32109")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::get_NameMergePriority(IVariantVector **ppVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI32110", ppVal != __nullptr);

		validateLicense();
		
		IVariantVectorPtr ipShallowCopy = m_ipNameMergePriority;
		*ppVal = ipShallowCopy.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32111")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::put_NameMergePriority(IVariantVector *pNewVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI32112", pNewVal != __nullptr);

		validateLicense();

		m_ipNameMergePriority = pNewVal;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32113")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::get_PreserveAsSubAttributes(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI32114", pVal != __nullptr);

		validateLicense();
		
		*pVal = asVariantBool(m_bPreserveAsSubAttributes);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32115")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::put_PreserveAsSubAttributes(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bPreserveAsSubAttributes = asCppBool(newVal);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32116")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::get_CreateMergedRegion(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI32117", pVal != __nullptr);

		validateLicense();
		
		*pVal = asVariantBool(m_bCreateMergedRegion);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32118")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::put_CreateMergedRegion(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bCreateMergedRegion = asCppBool(newVal);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32119")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::get_TreatNameListAsRegex(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI33103", pVal != __nullptr);

		validateLicense();
		
		*pVal = asVariantBool(m_bTreatNameListAsRegex);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33104")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::put_TreatNameListAsRegex(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bTreatNameListAsRegex = asCppBool(newVal);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33105")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::get_ValueMergeMode(EFieldMergeMode *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI33106", pVal != __nullptr);

		validateLicense();
		
		*pVal = m_eValueMergeMode;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33107")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::put_ValueMergeMode(EFieldMergeMode newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI33108", newVal != kCombineField);

		validateLicense();

		m_eValueMergeMode = newVal;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33109")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::get_ValueMergePriority(IVariantVector **ppVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI33083", ppVal != __nullptr);

		validateLicense();
		
		IVariantVectorPtr ipShallowCopy = m_ipValueMergePriority;
		*ppVal = ipShallowCopy.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33084")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::put_ValueMergePriority(IVariantVector *pNewVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI33085", pNewVal != __nullptr);

		validateLicense();

		m_ipValueMergePriority = pNewVal;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33086")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::get_TreatValueListAsRegex(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI33087", pVal != __nullptr);

		validateLicense();
		
		*pVal = asVariantBool(m_bTreatValueListAsRegex);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33088")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::put_TreatValueListAsRegex(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bTreatValueListAsRegex = asCppBool(newVal);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33089")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::get_TypeFromName(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI33090", pVal != __nullptr);

		validateLicense();
		
		*pVal = asVariantBool(m_bTypeFromName);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33091")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::put_TypeFromName(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bTypeFromName = asCppBool(newVal);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33092")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::get_PreserveType(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI33093", pVal != __nullptr);

		validateLicense();
		
		*pVal = asVariantBool(m_bPreserveType);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33094")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::put_PreserveType(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bPreserveType = asCppBool(newVal);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33095")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::get_TypeMergePriority(IVariantVector **ppVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI33096", ppVal != __nullptr);

		validateLicense();
		
		IVariantVectorPtr ipShallowCopy = m_ipTypeMergePriority;
		*ppVal = ipShallowCopy.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33097")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::put_TypeMergePriority(IVariantVector *pNewVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI33098", pNewVal != __nullptr);

		validateLicense();

		m_ipTypeMergePriority = pNewVal;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33099")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::get_TreatTypeListAsRegex(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI33100", pVal != __nullptr);

		validateLicense();
		
		*pVal = asVariantBool(m_bTreatTypeListAsRegex);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33101")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::put_TreatTypeListAsRegex(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bTreatTypeListAsRegex = asCppBool(newVal);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33102")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::FindQualifiedMerges(IIUnknownVector* pAttributes,
	ISpatialString *pDocText)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		m_ipDocText = pDocText;
		ASSERT_ARGUMENT("ELI32067", m_ipDocText != __nullptr);

		IIUnknownVectorPtr ipAttributes(pAttributes);
		ASSERT_ARGUMENT("ELI32068", ipAttributes != __nullptr);

		initialize(ipAttributes);

		findQualifiedMerges(ipAttributes, ipAttributes);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32069")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::CompareAttributeSets(IIUnknownVector* pvecAttributeSet1,
	IIUnknownVector* pvecAttributeSet2, ISpatialString *pDocText, VARIANT_BOOL* pvbMatching)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		m_ipDocText = pDocText;
		ASSERT_ARGUMENT("ELI32070", m_ipDocText != __nullptr);

		IIUnknownVectorPtr ipAttributeSet1(pvecAttributeSet1);
		ASSERT_ARGUMENT("ELI32071", ipAttributeSet1 != __nullptr);

		IIUnknownVectorPtr ipAttributeSet2(pvecAttributeSet2);
		ASSERT_ARGUMENT("ELI32072", ipAttributeSet2 != __nullptr);

		ASSERT_ARGUMENT("ELI32073", pvbMatching != __nullptr);

		*pvbMatching = VARIANT_TRUE;

		// Initialize for a comparison
		initialize(ipAttributeSet1, ipAttributeSet2);

		// Compare for qualified merges.
		findQualifiedMerges(ipAttributeSet1, ipAttributeSet2);

		// The attribute sets match if every spatial attribute in both sets have an associated
		// merge result.
		bool bMatching = areAllAttributesMerged(ipAttributeSet1) &&
						 areAllAttributesMerged(ipAttributeSet2);

		*pvbMatching = asVariantBool(bMatching);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31774")
}
//---------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::ApplyMerges(IIUnknownVector* pvecAttributeSet)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		IIUnknownVectorPtr ipAttributeSet(pvecAttributeSet);
		ASSERT_ARGUMENT("ELI32074", ipAttributeSet != __nullptr);

		// Remove results that were unable to be named using the name priority list.
		removeInvalidResults();

		// Apply the merged results to the rule's output
		applyResults(ipAttributeSet);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32075")
}

//---------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//---------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialAttributeMergeUtils::InterfaceSupportsErrorInfo(REFIID riid)
{
	try
	{
		static const IID* arr[] = 
		{
			&IID_ISpatialAttributeMergeUtils,
			&IID_ILicensedComponent
		};

		for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
		{
			if (InlineIsEqualGUID(*arr[i],riid))
				return S_OK;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31772")
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CSpatialAttributeMergeUtils::initialize(IIUnknownVectorPtr ipAttributeSet1,
										     IIUnknownVectorPtr ipAttributeSet2/* = __nullptr*/)
{
	m_ipQualifiedMerges.CreateInstance(CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION("ELI32076", m_ipQualifiedMerges != __nullptr);

	// Ensure m_mapChildToParentAttributes is empty before starting processing
	m_mapChildToParentAttributes.clear();

	// Ensure the attribute info cache is cleared.
	m_mapAttributeInfo.clear();

	// Ensure the standardized page info cache is cleared.
	m_mapSpatialInfos.clear();

	// Load ipAttributeSet1 into the attribute data cache.
	long nSet1Size = ipAttributeSet1->Size();
	for (long i = 0; i < nSet1Size; i++)
	{
		// Retrieve the pair of attributes to test.
		IAttributePtr ipAttribute = ipAttributeSet1->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI32077", ipAttribute != __nullptr);

		// [FlexIDSCore:3328] Don't attempt to process non-spatial attributes 
		ISpatialStringPtr ipValue = ipAttribute->Value;
		if (ipValue != __nullptr && asCppBool(ipValue->HasSpatialInfo()))
		{
			loadAttributeInfo(ipAttribute);
		}
	}

	if (ipAttributeSet2 != __nullptr)
	{
		// Load ipAttributeSet2 into the attribute data cache.
		long nSet2Size = ipAttributeSet2->Size();
		for (long i = 0; i < nSet2Size; i++)
		{
			// Retrieve the pair of attributes to test.
			IAttributePtr ipAttribute = ipAttributeSet2->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI32078", ipAttribute != __nullptr);

			// [FlexIDSCore:3328] Don't attempt to process non-spatial attributes 
			ISpatialStringPtr ipValue = ipAttribute->Value;
			if (ipValue != __nullptr && asCppBool(ipValue->HasSpatialInfo()))
			{
				loadAttributeInfo(ipAttribute);
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CSpatialAttributeMergeUtils::findQualifiedMerges(IIUnknownVectorPtr ipAttributeSet1,
													  IIUnknownVectorPtr ipAttributeSet2)
{
	// Create a temporary working vector in which merged attributes will be placed.
	IIUnknownVectorPtr ipTargetAttributes(CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION("ELI32079", ipTargetAttributes != __nullptr);

	ipTargetAttributes->Append(ipAttributeSet2);
	
	long nSet1Size = ipAttributeSet1->Size();
	long nTargetSetSize = ipTargetAttributes->Size();

	// Cycle through the attribute sets every possible pair for merging.
	// Get the count of target attributes with each iteration since new attributes may be added
	// as processing progresses.
	for (long i = 0; i < nSet1Size; i++)
	{
		bool bAttribute1Merged = false;

		// Retrieve the pair of attributes to test.
		IAttributePtr ipAttribute1 = ipAttributeSet1->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI23008", ipAttribute1 != __nullptr);

		// [FlexIDSCore:3328] Don't attempt to process non-spatial attributes 
		if (m_mapAttributeInfo.find(ipAttribute1) == m_mapAttributeInfo.end())
		{
			continue;
		}

		for (long j = 0; j < nTargetSetSize; j++)
		{
			IAttributePtr ipAttribute2 = ipTargetAttributes->At(j);
			ASSERT_RESOURCE_ALLOCATION("ELI23009", ipAttribute2 != __nullptr);

			// Don't compare an attribute to itself.
			if (ipAttribute1 == ipAttribute2)
			{
				continue;
			}

			// [FlexIDSCore:3328] Don't attempt to process non-spatial attributes 
			if (m_mapAttributeInfo.find(ipAttribute2) == m_mapAttributeInfo.end())
			{
				continue;
			}

			// Retrieves the set of pages in which the attributes overlap by the 
			// specified amount.
			set<long> setPagesWithOverlap = getPagesWithOverlap(ipAttribute1, ipAttribute2);

			for each (long nPage in setPagesWithOverlap)
			{
				IAttributePtr ipAttributeToRemove = __nullptr;

				// For each page that there is sufficient overlap, merge the attributes.
				IAttributePtr ipMergedAttribute = mergeAttributes(ipAttribute1, ipAttribute2, 
					nPage, ipAttributeToRemove);

				if (ipMergedAttribute != __nullptr)
				{
					bAttribute1Merged = true;

					// Add the merge result to the list of merged results and the list of 
					// attributes eligible for merging.
					m_ipQualifiedMerges->PushBackIfNotContained(ipMergedAttribute);
					ipTargetAttributes->PushBackIfNotContained(ipMergedAttribute);
					nTargetSetSize = ipTargetAttributes->Size();

					// It is possible both these attributes were already merged in which case
					// one of them will have been removed from the lists. 
					if (ipAttributeToRemove != __nullptr)
					{
						// Remove this attribute from ipMergeResults
						removeAttribute(ipAttributeToRemove, m_ipQualifiedMerges, nPage);
						
						// Remove this attribute from ipTargetAttributes
						long nTargetIndex = removeAttribute(ipAttributeToRemove, ipTargetAttributes, nPage);
						nTargetSetSize--;

						// Adjust the j index appropriately.
						j -= (nTargetIndex <= j) ? 1 : 0;
					}
				}
			}
		}

		// Restart the iteration in case an previous attributes qualify to be merged
		// with this result.
		if (bAttribute1Merged)
		{
			i = -1;
		}
	}
}
//-------------------------------------------------------------------------------------------------
set<long> CSpatialAttributeMergeUtils::getPagesWithOverlap(IAttributePtr ipAttribute1, 
														   IAttributePtr ipAttribute2)
{
	ASSERT_ARGUMENT("ELI22889", ipAttribute1 != __nullptr);
	ASSERT_ARGUMENT("ELI22890", ipAttribute2 != __nullptr);

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
double CSpatialAttributeMergeUtils::calculateOverlap(IAttributePtr ipAttribute1,
													 IAttributePtr ipAttribute2,
													 long nPage)
{
	ASSERT_ARGUMENT("ELI22982", ipAttribute1 != __nullptr);
	ASSERT_ARGUMENT("ELI22983", ipAttribute2 != __nullptr);

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

	if (m_bUseMutualOverlap)
	{
		return 100.0 * (double) nTotalIntersection / (double) max(nTotalArea1, nTotalArea2);
	}
	else
	{
		return 100.0 * (double) nTotalIntersection / (double) min(nTotalArea1, nTotalArea2);
	}
}
//--------------------------------------------------------------------------------------------------
void CSpatialAttributeMergeUtils::loadAttributeInfo(IAttributePtr ipAttribute)
{
	ASSERT_ARGUMENT("ELI23055", ipAttribute != __nullptr);

	m_mapAttributeInfo[ipAttribute].setPages.clear();
	m_mapAttributeInfo[ipAttribute].mapRasterZones.clear();

	// Obtain the spatial string value of both attributes.
	ISpatialStringPtr ipValue = ipAttribute->Value;
	ASSERT_RESOURCE_ALLOCATION("ELI23056", ipValue != __nullptr);

	// Retrieve the first and last page number of each.
	long nFirstPage = ipValue->GetFirstPageNumber();
	long nLastPage = ipValue->GetLastPageNumber();

	for (long nPage = nFirstPage; nPage <= nLastPage; nPage++)
	{
		// Retrieve the specific page we need.
		ISpatialStringPtr ipPage = ipValue->GetSpecifiedPages(nPage, nPage);
		ASSERT_RESOURCE_ALLOCATION("ELI22957", ipPage != __nullptr);

		if (!asCppBool(ipPage->HasSpatialInfo()))
		{
			// If this page has no spatial information, it has no raster zones.
			continue;
		}

		// Use raster zones to retrieve the spatial information.
		IIUnknownVectorPtr ipRasterZones =
			ipPage->GetTranslatedImageRasterZones(m_ipDocText->SpatialPageInfos);
		ASSERT_RESOURCE_ALLOCATION("ELI22904", ipRasterZones != __nullptr);

		// Cycle through each raster zone and obtain a CRect representing each.
		long nCount = ipRasterZones->Size();
		
		ILongRectanglePtr ipPageBounds = __nullptr;
		if (nCount > 0)
		{
			// Get the page bounds (for use by GetRectangularBounds).
			// NOTE: All zones will be on the same page, so we only need to get the bounds once.
			ipPageBounds = ipPage->GetOCRImageBounds();
			ASSERT_RESOURCE_ALLOCATION("ELI30312", ipPageBounds != __nullptr);
		}

		for (long i = 0; i < nCount; i++)
		{
			IRasterZonePtr ipRasterZone = ipRasterZones->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI22881", ipRasterZone != __nullptr);

			// Obtain a rect describing the location of this raster zone.
			ILongRectanglePtr ipRect = ipRasterZone->GetRectangularBounds(ipPageBounds);
			ASSERT_RESOURCE_ALLOCATION("ELI22882", ipRect != __nullptr);

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
set<long> CSpatialAttributeMergeUtils::getAttributePages(IAttributePtr ipAttribute1,
														 IAttributePtr ipAttribute2)
{
	ASSERT_ARGUMENT("ELI22952", ipAttribute1 != __nullptr);
	ASSERT_ARGUMENT("ELI22953", ipAttribute2 != __nullptr);

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
IAttributePtr CSpatialAttributeMergeUtils::mergeAttributes(IAttributePtr ipAttribute1, 
														   IAttributePtr ipAttribute2, long nPage,
														   IAttributePtr &ripAttributeToRemove)
{
	ASSERT_ARGUMENT("ELI22887", ipAttribute1 != __nullptr);
	ASSERT_ARGUMENT("ELI22888", ipAttribute2 != __nullptr);

	ripAttributeToRemove = __nullptr;

	// Provides an attribute to merge into and determine which two attributes need to be merged 
	// (if either provided attribute has already been merged, the attribute pointer(s) will be 
	// changed to reflect the merge result it is already a part of).
	IAttributePtr ipMergedResult = getMergeTarget(ipAttribute1, ipAttribute2, nPage);

	if (ipMergedResult == __nullptr)
	{
		// These two attributes have already been merged into the same result.
		return __nullptr;
	}

	// Merge ipAttribute1 and ipAttribute2 into ipMergedResult
	mergeAttributePair(ipMergedResult, ipAttribute1, ipAttribute2, nPage);

	if (ipAttribute1 != ipMergedResult)
	{
		// If ipAttribute1 isn't the merge result, re-associate it and any of its associated 
		// attributes with ipMergedResult.
		if (associateAttributeWithResult(ipAttribute1, ipMergedResult, nPage))
		{
			// ipAttribute1 is a merge result itself and is no longer needed
			ripAttributeToRemove = ipAttribute1;
		}
	}

	if (ipAttribute2 != ipMergedResult)
	{
		if (ripAttributeToRemove != __nullptr)
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
			// ipAttribute2 is a merge result itself and is no longer needed
			ripAttributeToRemove = ipAttribute2;
		}
	}

	return ipMergedResult;
}
//--------------------------------------------------------------------------------------------------
IAttributePtr CSpatialAttributeMergeUtils::getMergeTarget(IAttributePtr &ripAttribute1, 
														  IAttributePtr &ripAttribute2, long nPage)
{
	// The attribute to merge into
	IAttributePtr ipMergedAttribute = __nullptr;

	// See if ripAttribute1 has already been merged into another result.
	AttributeMap::iterator iterAttribute1Parent
		= m_mapChildToParentAttributes.find(AttributePage(ripAttribute1, nPage));

	if (iterAttribute1Parent != m_mapChildToParentAttributes.end())
	{
		// ripAttribute1 has been merged into another result-- see if that result is 
		// on this page.
		IAttributePtr ipParentAttribute = iterAttribute1Parent->second;
		ASSERT_RESOURCE_ALLOCATION("ELI22993", ipParentAttribute != __nullptr);

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
		ASSERT_RESOURCE_ALLOCATION("ELI22994", ipParentAttribute != __nullptr);

		if (ipParentAttribute == ipMergedAttribute)
		{
			// If the result in on this page and matches the existing result of ipAttribute1,
			// these two attributes have already been merged; return __nullptr to indicate
			// nothing needs to be done.
			return __nullptr;
		}

		// If the result is on this page, merge into that existing result
		ipMergedAttribute = ipParentAttribute;
		ripAttribute2 = ipParentAttribute;
	}

	// If neither attribute is associated with an existing search result, 
	if (ipMergedAttribute == __nullptr)
	{
		ipMergedAttribute.CreateInstance(CLSID_Attribute);
		ASSERT_RESOURCE_ALLOCATION("ELI22891", ipMergedAttribute != __nullptr);
	}
	
	return ipMergedAttribute;
}
//--------------------------------------------------------------------------------------------------
void CSpatialAttributeMergeUtils::mergeAttributePair(IAttributePtr ipMergedAttribute, 
													 IAttributePtr ipAttribute1, 
													 IAttributePtr ipAttribute2, long nPage)
{
	ASSERT_ARGUMENT("ELI22976", ipMergedAttribute != __nullptr);
	ASSERT_ARGUMENT("ELI22977", ipAttribute1 != __nullptr);
	ASSERT_ARGUMENT("ELI22978", ipAttribute2 != __nullptr);

	IAttributePtr ipNamePreservedAttribute = __nullptr;
	bool bAttributeNamesMatch = false;

	if (m_eNameMergeMode == kSpecifyField)
	{
		// Use the m_strSpecifiedName for the attribute name
		ipMergedAttribute->Name = _bstr_t(m_strSpecifiedName.c_str());
	}
	else if (m_eNameMergeMode == kPreserveField)
	{
		// Set the attribute name by using one of the existing names & m_ipNameMergePriority.
		ipNamePreservedAttribute = getAttributeToPreserve(ipAttribute1, ipAttribute2,
			ipAttribute1->Name, ipAttribute2->Name, m_ipNameMergePriority, m_bTreatNameListAsRegex,
			&bAttributeNamesMatch);

		if (ipNamePreservedAttribute == __nullptr)
		{
			ipMergedAttribute->Name = gbstrNEEDS_NAME;
		}
		else
		{
			ipMergedAttribute->Name = ipNamePreservedAttribute->Name;
		}
	}
	else
	{
		// An invalid EMergeMode is being used.
		THROW_LOGIC_ERROR_EXCEPTION("ELI22928");
	}

	if (m_eValueMergeMode == kSpecifyField)
	{
		ipMergedAttribute->Value = createMergedValue(m_strSpecifiedValue, ipAttribute1,
			ipAttribute2, nPage);
	}
	else if (m_eValueMergeMode == kPreserveField)
	{
		// Set the attribute value text by using one of the existing values & m_ipValueMergePriority.
		IAttributePtr ipValuePreservedAttribute = getAttributeToPreserve(ipAttribute1, ipAttribute2,
			ipAttribute1->Value->String, ipAttribute2->Value->String, m_ipValueMergePriority,
			m_bTreatValueListAsRegex);

		// If we couldn't determine which value to preserve, pick one. (With value preservation
		// list, its probably not safe to assume the preservation list will cover all possible
		// values.)
		if (ipValuePreservedAttribute == __nullptr)
		{
			ipValuePreservedAttribute = ipAttribute1;
			
			UCLIDException ue("ELI33119", "Application trace: unable to find match for either "
				"attribute in the value preservation list.");
			ue.log();
		}

		ipMergedAttribute->Value = createMergedValue(
			asString(ipValuePreservedAttribute->Value->String),
			ipAttribute1, ipAttribute2, nPage);
	}
	else if (m_eValueMergeMode == kSelectField)
	{
		if (ipNamePreservedAttribute == __nullptr)
		{
			// If a name to preserve has not yet been found, assign gbstrNEEDS_NAME since the
			// merged result needs some text value for now. If no valid name is found, the attribute
			// will never be returned, so there is no risk of gbstrNEEDS_NAME ending up as the
			// attribute value.
			ipMergedAttribute->Value = createMergedValue(asString(gbstrNEEDS_NAME),
				ipAttribute1, ipAttribute2, nPage);
		}
		else
		{
			ipMergedAttribute->Value = createMergedValue(
				asString(ipNamePreservedAttribute->Value->String),
				ipAttribute1, ipAttribute2, nPage);
		}
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
	else if (m_eTypeMergeMode == kSelectField)
	{
		// First try to use the type of the attribute that supplied the name is specified.
		if (m_bTypeFromName)
		{
			if (ipNamePreservedAttribute != __nullptr)
			{
				ipMergedAttribute->Type = ipNamePreservedAttribute->Type;
			}
		}

		// The TypeMergePriority list can be used as a backup even if TypeFromName was specified if
		// both attributes matched.
		if (m_bPreserveType && (!m_bTypeFromName || bAttributeNamesMatch))
		{
			IAttributePtr ipTypePreservedAttribute = getAttributeToPreserve(ipAttribute1, ipAttribute2,
				ipAttribute1->Type, ipAttribute2->Type, m_ipTypeMergePriority, m_bTreatTypeListAsRegex);

			if (ipTypePreservedAttribute != __nullptr)
			{
				ipMergedAttribute->Type = ipTypePreservedAttribute->Type;
			}
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
ISpatialStringPtr CSpatialAttributeMergeUtils::createMergedValue(string strText,
																 IAttributePtr ipAttribute1, 
																 IAttributePtr ipAttribute2,
																 long nPage)
{
	ASSERT_ARGUMENT("ELI22963", ipAttribute1 != __nullptr);
	ASSERT_ARGUMENT("ELI22964", ipAttribute2 != __nullptr);

	// If an entry has not been created for the specified page in m_mapSpatialInfos, create one now.
	if (m_mapSpatialInfos.find(nPage) == m_mapSpatialInfos.end())
	{
		// We want to modify the existing page's PageInfo for the attribute, but we don't want
		// to affect the existing page, so obtain a copy.
		ICopyableObjectPtr ipCloneThis = m_ipDocText->GetPageInfo(nPage);
		ASSERT_RESOURCE_ALLOCATION("ELI25268", ipCloneThis != __nullptr);

		ISpatialPageInfoPtr ipPageInfoClone = ipCloneThis->Clone();
		ASSERT_RESOURCE_ALLOCATION("ELI25269", ipPageInfoClone != __nullptr);
		
		// Remove any skew and orientation since the retrieved bounding rectangles will be in terms
		// of literal page coordinates.
		ipPageInfoClone->Deskew = 0;
		ipPageInfoClone->Orientation = kRotNone;

		m_mapSpatialInfos[nPage] = ipPageInfoClone;
	}

	// Create a new spatial page info map from the map of standard page infos
	ILongToObjectMapPtr ipPageInfos(CLSID_LongToObjectMap);
	ASSERT_RESOURCE_ALLOCATION("ELI26682", ipPageInfos != __nullptr);
	for (map<long, ISpatialPageInfoPtr>::iterator it = m_mapSpatialInfos.begin();
		it != m_mapSpatialInfos.end(); it++)
	{
		ipPageInfos->Set(it->first, it->second);
	}

	ISpatialStringPtr ipMergedValue(CLSID_SpatialString);
	ASSERT_RESOURCE_ALLOCATION("ELI22892", ipMergedValue != __nullptr);

	IIUnknownVectorPtr ipMergedRasterZones(CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION("ELI30844", ipMergedRasterZones != __nullptr);

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
		ASSERT_RESOURCE_ALLOCATION("ELI30972", ipPageBounds != __nullptr);

		// Create a vector of all raster zones in the two attributes.
		vector<CRect> vecZoneRects(m_mapAttributeInfo[ipAttribute1].mapRasterZones[nPage].begin(),
								   m_mapAttributeInfo[ipAttribute1].mapRasterZones[nPage].end());
		vecZoneRects.insert(vecZoneRects.end(),
							m_mapAttributeInfo[ipAttribute2].mapRasterZones[nPage].begin(),
							m_mapAttributeInfo[ipAttribute2].mapRasterZones[nPage].end());

		// Merge any overlapping raster zones.
		for (vector<CRect>::iterator i = vecZoneRects.begin(); i != vecZoneRects.end(); i++)
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

	// If there is only one resulting raster zone and the text to assign does not have any newline
	// chars, create a psuedo-spatial string with strText as the text spread evenly across the
	// entire area of the attribute.
	if (ipMergedRasterZones->Size() == 1 &&
		strText.find('\r') == string::npos && strText.find('\n') == string::npos)
	{
		IRasterZonePtr ipRasterZone = ipMergedRasterZones->At(0);
		ASSERT_RESOURCE_ALLOCATION("ELI30973", ipRasterZone != __nullptr);
	
		ipMergedValue->CreatePseudoSpatialString(ipRasterZone, strText.c_str(),
			m_ipDocText->SourceDocName, m_ipDocText->SpatialPageInfos);
	}
	// Otherwise, create a hybrid result with strText as the text value.
	else
	{
		ipMergedValue->CreateHybridString(ipMergedRasterZones, strText.c_str(),
			m_ipDocText->SourceDocName, m_ipDocText->SpatialPageInfos);
	}

	return ipMergedValue;
}
//--------------------------------------------------------------------------------------------------
IAttributePtr CSpatialAttributeMergeUtils::getAttributeToPreserve(IAttributePtr &ipAttributeA, 
																  IAttributePtr &ipAttributeB,
																  _bstr_t bstrValueA, _bstr_t bstrValueB, 
																  IVariantVectorPtr ipValuePriorityList,
																  bool bTreatAsRegEx,
																  bool *pbBothMatch/* = __nullptr*/)
{
	if (pbBothMatch != __nullptr)
	{
		*pbBothMatch = false;
	}
	IAttributePtr ipResult = __nullptr;

	ASSERT_ARGUMENT("ELI22896", ipValuePriorityList != __nullptr);

	string strValueA = asString(bstrValueA);
	string strValueB = asString(bstrValueB);

	// Iterate through the priority list looking for the first matching value. The loop will
	// make case-insensitive comparisons, but will attempt to use the list to resolve differences
	// in casing.
	long nCount = ipValuePriorityList->Size;
	for (long i = 0; i < nCount; i++)
	{
		bool bCaseInsensitive = false;
		string strPattern = asString(ipValuePriorityList->GetItem(i).bstrVal);
		
		if (textIsMatch(strValueA, strPattern, bTreatAsRegEx, bCaseInsensitive))
		{
			// ValueA is a match
			if (!bCaseInsensitive)
			{
				if (pbBothMatch != __nullptr)
				{
					*pbBothMatch = 
						textIsMatch(strValueA, strValueB, bTreatAsRegEx, bCaseInsensitive)
						&& !bCaseInsensitive;
				}

				// Return immediately with a case-sensitive match. Otherwise, check valueB
				// in case it is a case-sensitive match.
				return ipAttributeA;
			}

			ipResult = ipAttributeA;
		}

		if (textIsMatch(strValueB, strPattern, bTreatAsRegEx, bCaseInsensitive))
		{
			if (pbBothMatch != __nullptr)
			{
				*pbBothMatch = (ipResult != __nullptr);
			}

			// ValueB is a match
			ipResult = ipAttributeB;
		}

		if (ipResult != __nullptr)
		{
			// We have found a match.
			return ipResult;
		}
	}

	if (_strcmpi(strValueA.c_str(), strValueB.c_str()) == 0)
	{
		// If value A and B are equal, use this value. (Save this check for last to allow the list
		// to resolve case sensisitivity differences if possible).
		if (pbBothMatch != __nullptr)
		{
			*pbBothMatch = true;
		}
		return ipAttributeA;
	}

	// Finally, if at least one of the two attributes have a value for the specified field but the
	// other does not, return that since something is better than nothing.
	if (!strValueA.empty() && strValueB.empty())
	{
		return ipAttributeA;
	}
	else if (strValueA.empty() && !strValueB.empty())
	{
		return ipAttributeB;
	}

	return __nullptr;
}
//--------------------------------------------------------------------------------------------------
bool CSpatialAttributeMergeUtils::associateAttributeWithResult(IAttributePtr ipAttribute, 
															   IAttributePtr ipMergedResult,
															   long nPage)
{
	ASSERT_ARGUMENT("ELI23013", ipAttribute != __nullptr);
	ASSERT_ARGUMENT("ELI23014", ipMergedResult != __nullptr);

	bool bRemoveAttribute = false;

	// Get the existing sub-attribute list for the merge result.
	IIUnknownVectorPtr ipResultSubAttributes = ipMergedResult->SubAttributes;
	ASSERT_RESOURCE_ALLOCATION("ELI23005", ipResultSubAttributes != __nullptr);

	if (m_mapChildToParentAttributes[AttributePage(ipAttribute, nPage)] == ipAttribute)
	{
		// ipAttribute is a merge result itself; this merge result is no longer needed.
		bRemoveAttribute = true;

		if (m_bPreserveAsSubAttributes)
		{
			// Since we are preserving original attributes as sub-attributes, all sub-attributes
			// associated with ipAttribute need to be transfered to the merge result.
			IIUnknownVectorPtr ipSourceSubAttributes = ipAttribute->SubAttributes;
			ASSERT_RESOURCE_ALLOCATION("ELI22997", ipSourceSubAttributes != __nullptr);

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
void CSpatialAttributeMergeUtils::removeInvalidResults()
{
	if (m_ipQualifiedMerges == __nullptr)
	{
		throw UCLIDException("ELI32080",
			"SpatialAttributeCompare cannot validate results that haven't been generated.");
	}

	// Loop through all merge results looking for ones that have been flagged as nameless.
	long nCount = m_ipQualifiedMerges->Size();
	for (long i = 0; i < nCount; i++)
	{
		IAttributePtr ipMergeResult = m_ipQualifiedMerges->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI23255", ipMergeResult != __nullptr);

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
				ASSERT_RESOURCE_ALLOCATION("ELI23047", ipAttribute != __nullptr);

				// Check for attributes associated with the bad merge but that are not the bad
				// merge itself.
				if (iter->second == ipMergeResult && ipAttribute != ipMergeResult)
				{
					ue.addDebugInfo("Attribute Name", asString(ipAttribute->Name));

					// Invalidate this attribute's link in m_mapChildToParentAttributes 
					m_mapChildToParentAttributes[iter->first] = __nullptr;
				}
			}

			// Log the exception
			ue.log();

			// Remove the attribute from ipMergeResults
			removeAttribute(ipMergeResult, m_ipQualifiedMerges);

			// Update the current index and count accordingly
			i--;
			nCount--;
		}
	}
}
//--------------------------------------------------------------------------------------------------
void CSpatialAttributeMergeUtils::applyResults(IIUnknownVectorPtr ipAttributes)
{
	if (m_ipQualifiedMerges == __nullptr)
	{
		throw UCLIDException("ELI32081",
			"SpatialAttributeCompare cannot apply results that haven't been generated.");
	}

	ASSERT_ARGUMENT("ELI23040", m_ipQualifiedMerges != __nullptr);
	ASSERT_ARGUMENT("ELI23041", ipAttributes != __nullptr);

	// Add the merge results to the rule's results
	ipAttributes->Append(m_ipQualifiedMerges);

	// Now loop through all attributes involved in a merge, and remove any
	// that have been replaced by merge results on all the attribute's pages.
	for (AttributeMap::iterator iter = m_mapChildToParentAttributes.begin();
		 iter != m_mapChildToParentAttributes.end();
		 iter++)
	{
		// Get the attribute to examine.
		IAttributePtr ipAttribute = iter->first.first;
		ASSERT_RESOURCE_ALLOCATION("ELI23046", ipAttribute != __nullptr);

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
				iterAttributePage->second == __nullptr ||
				ipAttribute == iterAttributePage->second)
			{
				// We either found an attribute that is already a merge result or that was not
				// merged on one of its pages.
				bRemoveFromResults = false;
			}
		}

		// If necessary, remove this attribute from the resulting attribute list.
		if (bRemoveFromResults)
		{
			ipAttributes->RemoveValue(ipAttribute);
		}
	}
}
//--------------------------------------------------------------------------------------------------
long CSpatialAttributeMergeUtils::removeAttribute(IAttributePtr ipAttribute, 
									   IIUnknownVectorPtr ipAttributeList, long nPage/* = -1*/)
{
	ASSERT_ARGUMENT("ELI23051", ipAttribute != __nullptr);
	ASSERT_ARGUMENT("ELI23052", ipAttributeList != __nullptr);

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
	for each (long nPage in setPages);
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
IRasterZonePtr CSpatialAttributeMergeUtils::createRasterZone(CRect rect, long nPage)
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
bool CSpatialAttributeMergeUtils::areAllAttributesMerged(IIUnknownVectorPtr ipAttributes)
{
	long nCount = ipAttributes->Size();
		
	for (long i = 0; i < nCount; i++)
	{
		IAttributePtr ipAttribute = ipAttributes->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI32082", ipAttribute != __nullptr);

		// Ignore attributes without spatial info
		ISpatialStringPtr ipValue = ipAttribute->Value;
		if (ipValue == __nullptr || !asCppBool(ipValue->HasSpatialInfo()))
		{
			continue;
		}

		set<long> setPages = m_mapAttributeInfo[ipAttribute].setPages;
		for (set<long>::iterator iterPage = setPages.begin(); iterPage != setPages.end(); iterPage++)
		{
			if (m_mapChildToParentAttributes.find(AttributePage(ipAttribute, *iterPage)) ==
				m_mapChildToParentAttributes.end())
			{
				return false;
			}
		}
	}

	return true;
}
//--------------------------------------------------------------------------------------------------
bool CSpatialAttributeMergeUtils::textIsMatch(const string& strText, const string& strPattern,
	bool bTreatAsRegEx, bool &rbCaseInsensitive)
{
	rbCaseInsensitive = false;

	if (bTreatAsRegEx)
	{
		if (m_ipParser == __nullptr)
		{
			IMiscUtilsPtr ipMiscUtils(CLSID_MiscUtils);
			ASSERT_RESOURCE_ALLOCATION("ELI33110", ipMiscUtils != __nullptr );

			m_ipParser = ipMiscUtils->GetNewRegExpParserInstance("SpatialAttributeMergeUtils");
			ASSERT_RESOURCE_ALLOCATION("ELI33111", m_ipParser != __nullptr);
		}
		
		m_ipParser->Pattern = strPattern.c_str();
		return asCppBool(m_ipParser->StringMatchesPattern(strText.c_str()));
	}
	else
	{
		if (strText == strPattern)
		{
			return true;
		}
		else if (_strcmpi(strText.c_str(), strPattern.c_str()) == 0)
		{
			rbCaseInsensitive = true;
			return true;
		}
	}

	return false;
}
//--------------------------------------------------------------------------------------------------
void CSpatialAttributeMergeUtils::validateLicense()
{
	VALIDATE_LICENSE(gnFLEXINDEX_IDSHIELD_CORE_OBJECTS, "ELI31773", "SpatialAttributeMergeUtils");
}