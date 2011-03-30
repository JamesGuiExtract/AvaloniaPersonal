// EntityKeywords.cpp : Implementation of CEntityKeywords
#include "stdafx.h"
#include "AFUtils.h"
#include "EntityKeywords.h"
#include "..\\..\\AFCore\\Code\\Common.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <misc.h>
#include <comutils.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CEntityKeywords
//-------------------------------------------------------------------------------------------------
CEntityKeywords::CEntityKeywords()
:m_bReadKeywordsFromFile(false),
m_ipPersonTitleList(NULL),m_ipPersonSuffixList(NULL),m_ipPersonDesignatorList(NULL),
m_ipPersonAliasList(NULL),m_ipEntityTrimTrailingPhraseList(NULL),m_ipPersonTrimIdentifierList(NULL),
m_ipCompanySuffixList(NULL),m_ipCompanyDesignatorList(NULL),m_ipCompanyAliasList(NULL),
m_ipCompanyAssignorList(NULL),m_ipStreetNameList(NULL),m_ipStreetAbbreviationList(NULL),
m_ipBuildingNameList(NULL),m_ipBuildingAbbreviationList(NULL),m_ipNumberWordList(NULL),
m_ipMonthWordList(NULL),m_ipDirectionIndicatorList(NULL),m_ipAddressIndicatorList(NULL),
m_ipPersonAliasAKAList(NULL),m_ipPersonAliasFKAList(NULL),m_ipPersonAliasNKAList(NULL),
m_ipCompanyAliasDBAList(NULL),m_ipCompanyAliasSBMList(NULL),m_ipCompanyAliasSIIList(NULL),
m_ipCompanyAliasBMWList(NULL),m_ipRelatedCompanyList(NULL),m_ipRelatedCompanyDivisionList(NULL),
m_ipRelatedCompanySubdivisionList(NULL),m_ipRelatedCompanySubsidiaryList(NULL),
m_ipRelatedCompanyBranchList(NULL)
{
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityKeywords::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IEntityKeywords,
		&IID_ILicensedComponent
	};

	for (int i = 0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
		{
			return S_OK;
		}
	}

	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityKeywords::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		if (pbValue == NULL)
		{
			return E_POINTER;
		}

		// Check license
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

//-------------------------------------------------------------------------------------------------
// IEntityKeywords
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityKeywords::get_PersonTitles(IVariantVector **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// If the list object has not yet been created, create it and populate it.
		if (m_ipPersonTitleList == __nullptr)
		{
			// Object creation
			m_ipPersonTitleList.CreateInstance( CLSID_VariantVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI05840", m_ipPersonTitleList != __nullptr );

			// Populate IVariantVector
			buildVariantVector( m_ipPersonTitleList, "PersonTitles" );
		}

		// Provide vector to caller
		IVariantVectorPtr ipShallowCopy = m_ipPersonTitleList;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05841")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityKeywords::get_PersonSuffixes(IVariantVector **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// If the list object has not yet been created, create it and populate it.
		if (m_ipPersonSuffixList == __nullptr)
		{
			// Object creation
			m_ipPersonSuffixList.CreateInstance( CLSID_VariantVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI05946", m_ipPersonSuffixList != __nullptr );

			// Populate IVariantVector
			buildVariantVector( m_ipPersonSuffixList, "PersonSuffixes" );
		}

		// Provide vector to caller
		IVariantVectorPtr ipShallowCopy = m_ipPersonSuffixList;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05947")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityKeywords::get_PersonDesignators(IVariantVector **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// If the list object has not yet been created, create it and populate it.
		if (m_ipPersonDesignatorList == __nullptr)
		{
			// Object creation
			m_ipPersonDesignatorList.CreateInstance( CLSID_VariantVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI06360", m_ipPersonDesignatorList != __nullptr );

			// Populate IVariantVector
			buildVariantVector( m_ipPersonDesignatorList, "PersonDesignators" );
		}

		// Provide vector to caller
		IVariantVectorPtr ipShallowCopy = m_ipPersonDesignatorList;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05948")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityKeywords::get_CompanySuffixes(IVariantVector **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// If the list object has not yet been created, create it and populate it.
		if (m_ipCompanySuffixList == __nullptr)
		{
			// Object creation
			m_ipCompanySuffixList.CreateInstance( CLSID_VariantVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI05949", m_ipCompanySuffixList != __nullptr );

			// Populate IVariantVector
			buildVariantVector( m_ipCompanySuffixList, "CompanySuffixes" );
		}

		// Provide vector to caller
		IVariantVectorPtr ipShallowCopy = m_ipCompanySuffixList;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05950")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityKeywords::get_CompanyDesignators(IVariantVector **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// If the list object has not yet been created, create it and populate it.
		if (m_ipCompanyDesignatorList == __nullptr)
		{
			// Object creation
			m_ipCompanyDesignatorList.CreateInstance( CLSID_VariantVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI05951", m_ipCompanyDesignatorList != __nullptr );

			// Populate IVariantVector
			buildVariantVector( m_ipCompanyDesignatorList, "CompanyDesignators" );
		}

		// Provide vector to caller
		IVariantVectorPtr ipShallowCopy = m_ipCompanyDesignatorList;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05952")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityKeywords::get_CompanyAssignors(IVariantVector **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// If the list object has not yet been created, create it and populate it.
		if (m_ipCompanyAssignorList == __nullptr)
		{
			// Object creation
			m_ipCompanyAssignorList.CreateInstance( CLSID_VariantVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI06345", m_ipCompanyAssignorList != __nullptr );

			// Populate IVariantVector
			buildVariantVector( m_ipCompanyAssignorList, "CompanyAssignors" );
		}

		// Provide vector to caller
		IVariantVectorPtr ipShallowCopy = m_ipCompanyAssignorList;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06339")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityKeywords::get_PersonAlias(EPersonAliasType eType, IVariantVector **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// Declare IVariantVector smart pointer
		IVariantVectorPtr ipShallowCopy(NULL);

		// Alias collection is dependent on requested Type
		switch (eType)
		{
		case kPersonAliasAKA:
			// Ensure that list exists
			makePersonAliasList( kPersonAliasAKA );

			// Provide vector to caller
			ipShallowCopy = m_ipPersonAliasAKAList;
			*pVal = ipShallowCopy.Detach();
			break;

		case kPersonAliasFKA:
			// Ensure that list exists
			makePersonAliasList( kPersonAliasFKA );

			// Provide vector to caller
			ipShallowCopy = m_ipPersonAliasFKAList;
			*pVal = ipShallowCopy.Detach();
			break;

		case kPersonAliasNKA:
			// Ensure that list exists
			makePersonAliasList( kPersonAliasNKA );

			// Provide vector to caller
			ipShallowCopy = m_ipPersonAliasNKAList;
			*pVal = ipShallowCopy.Detach();
			break;

		case kPersonAliasAll:
			// Ensure that complete list exists
			makePersonAliasList( kPersonAliasAll );

			// Provide vector to caller
			ipShallowCopy = m_ipPersonAliasList;
			*pVal = ipShallowCopy.Detach();
			break;

		default:
			// Unknown Alias type
			UCLIDException ue( "ELI09611", "Unable to get unknown Person Alias type!" );
			ue.addDebugInfo( "Person Alias Type", (long)eType );
			throw ue;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06349")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityKeywords::get_EntityTrimTrailingPhrases(IVariantVector **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// If the list object has not yet been created, create it and populate it.
		if (m_ipEntityTrimTrailingPhraseList == __nullptr)
		{
			// Object creation
			m_ipEntityTrimTrailingPhraseList.CreateInstance( CLSID_VariantVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI06378", m_ipEntityTrimTrailingPhraseList != __nullptr );

			// Populate IVariantVector
			buildVariantVector( m_ipEntityTrimTrailingPhraseList, "EntityTrimTrailingPhrases" );
		}

		// Provide vector to caller
		IVariantVectorPtr ipShallowCopy = m_ipEntityTrimTrailingPhraseList;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06377")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityKeywords::get_PersonTrimIdentifiers(IVariantVector **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// If the list object has not yet been created, create it and populate it.
		if (m_ipPersonTrimIdentifierList == __nullptr)
		{
			// Object creation
			m_ipPersonTrimIdentifierList.CreateInstance( CLSID_VariantVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI06394", m_ipPersonTrimIdentifierList != __nullptr );

			// Populate IVariantVector
			buildVariantVector( m_ipPersonTrimIdentifierList, "PersonTrimIdentifiers" );
		}

		// Provide vector to caller
		IVariantVectorPtr ipShallowCopy = m_ipPersonTrimIdentifierList;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06395")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityKeywords::get_StreetNames(IVariantVector **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// If the list object has not yet been created, create it and populate it.
		if (m_ipStreetNameList == __nullptr)
		{
			// Object creation
			m_ipStreetNameList.CreateInstance( CLSID_VariantVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI07189", m_ipStreetNameList != __nullptr );

			// Populate IVariantVector
			buildVariantVector( m_ipStreetNameList, "StreetNames" );
		}

		// Provide vector to caller
		IVariantVectorPtr ipShallowCopy = m_ipStreetNameList;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07190")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityKeywords::get_StreetAbbreviations(IVariantVector **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// If the list object has not yet been created, create it and populate it.
		if (m_ipStreetAbbreviationList == __nullptr)
		{
			// Object creation
			m_ipStreetAbbreviationList.CreateInstance( CLSID_VariantVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI07199", m_ipStreetAbbreviationList != __nullptr );

			// Populate IVariantVector
			buildVariantVector( m_ipStreetAbbreviationList, "StreetAbbreviations" );
		}

		// Provide vector to caller
		IVariantVectorPtr ipShallowCopy = m_ipStreetAbbreviationList;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07200")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityKeywords::get_BuildingNames(IVariantVector **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// If the list object has not yet been created, create it and populate it.
		if (m_ipBuildingNameList == __nullptr)
		{
			// Object creation
			m_ipBuildingNameList.CreateInstance( CLSID_VariantVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI07205", m_ipBuildingNameList != __nullptr );

			// Populate IVariantVector
			buildVariantVector( m_ipBuildingNameList, "BuildingNames" );
		}

		// Provide vector to caller
		IVariantVectorPtr ipShallowCopy = m_ipBuildingNameList;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07206")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityKeywords::get_BuildingAbbreviations(IVariantVector **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// If the list object has not yet been created, create it and populate it.
		if (m_ipBuildingAbbreviationList == __nullptr)
		{
			// Object creation
			m_ipBuildingAbbreviationList.CreateInstance( CLSID_VariantVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI07207", m_ipBuildingAbbreviationList != __nullptr );

			// Populate IVariantVector
			buildVariantVector( m_ipBuildingAbbreviationList, "BuildingAbbreviations" );
		}

		// Provide vector to caller
		IVariantVectorPtr ipShallowCopy = m_ipBuildingAbbreviationList;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07208")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityKeywords::get_DirectionIndicators(IVariantVector **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// If the list object has not yet been created, create it and populate it.
		if (m_ipDirectionIndicatorList == __nullptr)
		{
			// Object creation
			m_ipDirectionIndicatorList.CreateInstance( CLSID_VariantVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI07212", m_ipDirectionIndicatorList != __nullptr );

			// Populate IVariantVector
			buildVariantVector( m_ipDirectionIndicatorList, "DirectionIndicators" );
		}

		// Provide vector to caller
		IVariantVectorPtr ipShallowCopy = m_ipDirectionIndicatorList;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07213")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityKeywords::get_CompanyAlias(ECompanyAliasType eType, IVariantVector **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// Create IVariantVector smart pointer
		IVariantVectorPtr ipShallowCopy(NULL);

		// Alias collection is dependent on requested Type
		switch (eType)
		{
		case kCompanyAliasAKA:
			// Ensure that list exists
			makeCompanyAliasList( kCompanyAliasAKA );

			// Provide vector to caller
			// NOTE: Provides Person Alias sublist
			ipShallowCopy = m_ipPersonAliasAKAList;
			*pVal = ipShallowCopy.Detach();
			break;

		case kCompanyAliasFKA:
			// Ensure that list exists
			makeCompanyAliasList( kCompanyAliasFKA );

			// Provide vector to caller
			// NOTE: Provides Person Alias sublist
			ipShallowCopy = m_ipPersonAliasFKAList;
			*pVal = ipShallowCopy.Detach();
			break;

		case kCompanyAliasNKA:
			// Ensure that list exists
			makeCompanyAliasList( kCompanyAliasNKA );

			// Provide vector to caller
			// NOTE: Provides Person Alias sublist
			ipShallowCopy = m_ipPersonAliasNKAList;
			*pVal = ipShallowCopy.Detach();
			break;

		case kCompanyAliasDBA:
			// Ensure that list exists
			makeCompanyAliasList( kCompanyAliasDBA );

			// Provide vector to caller
			ipShallowCopy = m_ipCompanyAliasDBAList;
			*pVal = ipShallowCopy.Detach();
			break;

		case kCompanyAliasSBM:
			// Ensure that list exists
			makeCompanyAliasList( kCompanyAliasSBM );

			// Provide vector to caller
			ipShallowCopy = m_ipCompanyAliasSBMList;
			*pVal = ipShallowCopy.Detach();
			break;

		case kCompanyAliasSII:
			// Ensure that list exists
			makeCompanyAliasList( kCompanyAliasSII );

			// Provide vector to caller
			ipShallowCopy = m_ipCompanyAliasSIIList;
			*pVal = ipShallowCopy.Detach();
			break;

		case kCompanyAliasBMW:
			// Ensure that list exists
			makeCompanyAliasList( kCompanyAliasBMW );

			// Provide vector to caller
			ipShallowCopy = m_ipCompanyAliasBMWList;
			*pVal = ipShallowCopy.Detach();
			break;

		case kCompanyAliasAll:
			// Ensure that complete list exists
			makeCompanyAliasList( kCompanyAliasAll );

			// Provide vector to caller
			ipShallowCopy = m_ipCompanyAliasList;
			*pVal = ipShallowCopy.Detach();
			break;

		default:
			// Unknown Alias type
			UCLIDException ue( "ELI09632", "Unable to get unknown Company Alias type!" );
			ue.addDebugInfo( "Company Alias Type", (long)eType );
			throw ue;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08685")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityKeywords::get_NumberWords(IVariantVector **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// If the list object has not yet been created, create it and populate it.
		if (m_ipNumberWordList == __nullptr)
		{
			// Object creation
			m_ipNumberWordList.CreateInstance( CLSID_VariantVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI08747", m_ipNumberWordList != __nullptr );

			// Populate IVariantVector
			buildVariantVector( m_ipNumberWordList, "NumberWords" );
		}

		// Provide vector to caller
		IVariantVectorPtr ipShallowCopy = m_ipNumberWordList;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08748")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityKeywords::get_MonthWords(IVariantVector **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// If the list object has not yet been created, create it and populate it.
		if (m_ipMonthWordList == __nullptr)
		{
			// Object creation
			m_ipMonthWordList.CreateInstance( CLSID_VariantVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI08752", m_ipMonthWordList != __nullptr );

			// Populate IVariantVector
			buildVariantVector( m_ipMonthWordList, "MonthWords" );
		}

		// Provide vector to caller
		IVariantVectorPtr ipShallowCopy = m_ipMonthWordList;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08753")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityKeywords::get_AddressIndicators(IVariantVector **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// If the list object has not yet been created, create it and populate it.
		if (m_ipAddressIndicatorList == __nullptr)
		{
			// Object creation
			m_ipAddressIndicatorList.CreateInstance( CLSID_VariantVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI09460", m_ipAddressIndicatorList != __nullptr );

			// Populate IVariantVector
			buildVariantVector( m_ipAddressIndicatorList, "AddressIndicators" );
		}

		// Provide vector to caller
		IVariantVectorPtr ipShallowCopy = m_ipAddressIndicatorList;
		*pVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09461")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityKeywords::GetPersonAliasLabel(EPersonAliasType eType, BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// Label depends on Alias type
		switch (eType)
		{
		case kPersonAliasAll:
			// No special label for Type
			*pVal = _bstr_t( "" ).copy();
			break;

		case kPersonAliasAKA:
			*pVal = _bstr_t( "AKA" ).copy();
			break;

		case kPersonAliasFKA:
			*pVal = _bstr_t( "FKA" ).copy();
			break;

		case kPersonAliasNKA:
			*pVal = _bstr_t( "NKA" ).copy();
			break;

		default:
			// No special label for Type
			*pVal = _bstr_t( "" ).copy();
			break;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09568")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityKeywords::GetCompanyAliasLabel(ECompanyAliasType eType, BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// Label depends on Alias type
		switch (eType)
		{
		case kCompanyAliasAll:
			// No special label for Type
			*pVal = _bstr_t( "" ).copy();
			break;

		case kCompanyAliasDBA:
			*pVal = _bstr_t( "DBA" ).copy();
			break;

		case kCompanyAliasSBM:
			*pVal = _bstr_t( "SBM" ).copy();
			break;

		case kCompanyAliasSII:
			*pVal = _bstr_t( "SII" ).copy();
			break;

		case kCompanyAliasBMW:
			*pVal = _bstr_t( "BMW" ).copy();
			break;

		default:
			// No special label for Type
			*pVal = _bstr_t( "" ).copy();
			break;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09600")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityKeywords::GetRelatedCompanyLabel(ERelatedCompanyType eType, BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// Label depends on type
		switch (eType)
		{
		case kRelatedCompanyAll:
			// No special label for Type
			*pVal = _bstr_t( "" ).copy();
			break;

		case kRelatedCompanyDivision:
			*pVal = _bstr_t( "Division" ).copy();
			break;

		case kRelatedCompanySubdivision:
			*pVal = _bstr_t( "Subdivision" ).copy();
			break;

		case kRelatedCompanySubsidiary:
			*pVal = _bstr_t( "Subsidiary" ).copy();
			break;

		case kRelatedCompanyBranch:
			*pVal = _bstr_t( "Branch" ).copy();
			break;

		default:
			// No special label for Type
			*pVal = _bstr_t( "" ).copy();
			break;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09646")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityKeywords::get_RelatedCompany(ERelatedCompanyType eType, IVariantVector **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// Declare an IVariantVector smart pointer
		IVariantVectorPtr ipShallowCopy(NULL);

		// Collection is dependent on requested Type
		switch (eType)
		{
		case kRelatedCompanyDivision:
			// Ensure that list exists
			makeRelatedCompanyList( kRelatedCompanyDivision );

			// Provide vector to caller
			ipShallowCopy = m_ipRelatedCompanyDivisionList;
			*pVal = ipShallowCopy.Detach();
			break;

		case kRelatedCompanySubdivision:
			// Ensure that list exists
			makeRelatedCompanyList( kRelatedCompanySubdivision );

			// Provide vector to caller
			ipShallowCopy = m_ipRelatedCompanySubdivisionList;
			*pVal = ipShallowCopy.Detach();
			break;

		case kRelatedCompanySubsidiary:
			// Ensure that list exists
			makeRelatedCompanyList( kRelatedCompanySubsidiary );

			// Provide vector to caller
			ipShallowCopy = m_ipRelatedCompanySubsidiaryList;
			*pVal = ipShallowCopy.Detach();
			break;

		case kRelatedCompanyBranch:
			// Ensure that list exists
			makeRelatedCompanyList( kRelatedCompanyBranch );

			// Provide vector to caller
			ipShallowCopy = m_ipRelatedCompanyBranchList;
			*pVal = ipShallowCopy.Detach();
			break;

		case kRelatedCompanyAll:
			// Ensure that complete list exists
			makeRelatedCompanyList( kRelatedCompanyAll );

			// Provide vector to caller
			ipShallowCopy = m_ipRelatedCompanyList;
			*pVal = ipShallowCopy.Detach();
			break;

		default:
			// Unknown Related Company type
			UCLIDException ue( "ELI09648", "Unable to get unknown Related Company type!" );
			ue.addDebugInfo( "Related Company Type", (long)eType );
			throw ue;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09647")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityKeywords::get_KeywordCollection(BSTR strKeyword, IVariantVector **pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// Create VariantVector for collection
		IVariantVectorPtr ipCollection( CLSID_VariantVector );
		ASSERT_RESOURCE_ALLOCATION( "ELI10103", ipCollection != __nullptr );

		// Collection is dependent on requested Type
		string strLocalKey = asString( strKeyword );
		if (strLocalKey.length() == 0)
		{
			// Throw exception
			UCLIDException ue( "ELI10104", "Unable to build keyword collection for blank keyword!" );
			throw ue;
		}

		// Build and return the collection
		buildVariantVector( ipCollection, strLocalKey );
		*pVal = ipCollection.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10100")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CEntityKeywords::get_KeywordPattern(BSTR strKeyword, BSTR* pstrPattern)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check licensing
		validateLicense();

		// Create VariantVector for collection
		IVariantVectorPtr ipCollection( CLSID_VariantVector );
		ASSERT_RESOURCE_ALLOCATION( "ELI10106", ipCollection != __nullptr );

		// Collection is dependent on requested Type
		string strLocalKey = asString( strKeyword );
		if (strLocalKey.length() == 0)
		{
			// Throw exception
			UCLIDException ue( "ELI10107", "Unable to build keyword pattern for blank keyword!" );
			throw ue;
		}

		// Build the collection (only 1 item expected)
		buildVariantVector( ipCollection, strLocalKey );

		// Retrieve the first item in the collection
		_bstr_t bstrPattern;
		if (ipCollection->Size > 0)
		{
			bstrPattern = ipCollection->GetItem( 0 );
		}

		// Provide pattern to caller
		*pstrPattern = bstrPattern.copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10105")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CEntityKeywords::buildVariantVector(IVariantVectorPtr ipList, string strKeyword)
{
	// Check for previous reading of DAT file
	if (!m_bReadKeywordsFromFile)
	{
		readKeywordsFile();
	}

	// Reset the VariantVector
	ipList->Clear();

	// Retrieve the collection of strings associated with the keyword
	vector<string>	vecStrings;
	if (!m_apKlr->GetStringsForKeyword( strKeyword, vecStrings ))
	{
		// Throw exception
		UCLIDException ue( "ELI10052", "Unable to build collection!" );
		ue.addDebugInfo( "Keyword", strKeyword );
		throw ue;
	}

	// Add each item in vector to IVariantVector
	long lSize = vecStrings.size();
	for (int i = 0; i < lSize; i++)
	{
		ipList->PushBack( vecStrings[i].c_str() );
	}
}
//-------------------------------------------------------------------------------------------------
void CEntityKeywords::makeCompanyAliasList(ECompanyAliasType eType)
{
	// Alias collection is dependent on requested Type
	switch (eType)
	{
	case kCompanyAliasAKA:
		// Create the appropriate Person Alias sublist
		makePersonAliasList( kPersonAliasAKA );
		break;

	case kCompanyAliasFKA:
		// Create the appropriate Person Alias sublist
		makePersonAliasList( kPersonAliasFKA );
		break;

	case kCompanyAliasNKA:
		// Create the appropriate Person Alias sublist
		makePersonAliasList( kPersonAliasNKA );
		break;

	case kCompanyAliasDBA:
		// If the list object has not yet been created, create it and populate it.
		if (m_ipCompanyAliasDBAList == __nullptr)
		{
			// Object creation
			m_ipCompanyAliasDBAList.CreateInstance( CLSID_VariantVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI09622", m_ipCompanyAliasDBAList != __nullptr );

			// Populate IVariantVector
			buildVariantVector( m_ipCompanyAliasDBAList, "CompanyAliasDBA" );
		}
		break;

	case kCompanyAliasSBM:
		// If the list object has not yet been created, create it and populate it.
		if (m_ipCompanyAliasSBMList == __nullptr)
		{
			// Object creation
			m_ipCompanyAliasSBMList.CreateInstance( CLSID_VariantVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI09623", m_ipCompanyAliasSBMList != __nullptr );

			// Populate IVariantVector
			buildVariantVector( m_ipCompanyAliasSBMList, "CompanyAliasSBM" );
		}
		break;

	case kCompanyAliasSII:
		// If the list object has not yet been created, create it and populate it.
		if (m_ipCompanyAliasSIIList == __nullptr)
		{
			// Object creation
			m_ipCompanyAliasSIIList.CreateInstance( CLSID_VariantVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI09624", m_ipCompanyAliasSIIList != __nullptr );

			// Populate IVariantVector
			buildVariantVector( m_ipCompanyAliasSIIList, "CompanyAliasSII" );
		}
		break;

	case kCompanyAliasBMW:
		// If the list object has not yet been created, create it and populate it.
		if (m_ipCompanyAliasBMWList == __nullptr)
		{
			// Object creation
			m_ipCompanyAliasBMWList.CreateInstance( CLSID_VariantVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI09625", m_ipCompanyAliasBMWList != __nullptr );

			// Populate IVariantVector
			buildVariantVector( m_ipCompanyAliasBMWList, "CompanyAliasBMW" );
		}
		break;

	case kCompanyAliasAll:
		// If the list object has not yet been created, create it and populate it.
		if (m_ipCompanyAliasList == __nullptr)
		{
			// Object creation
			m_ipCompanyAliasList.CreateInstance( CLSID_VariantVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI09626", m_ipCompanyAliasList != __nullptr );

			// Populate IVariantVector
			buildVariantVector( m_ipCompanyAliasList, "CompanyAlias" );
		}
		break;

	default:
		// Unknown Alias type
		UCLIDException ue( "ELI09627", "Unable to make list for unknown Company Alias type!" );
		ue.addDebugInfo( "Company Alias Type", (long)eType );
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void CEntityKeywords::makePersonAliasList(EPersonAliasType eType)
{
	// Alias collection is dependent on requested Type
	switch (eType)
	{
	case kPersonAliasAKA:
		// If the list object has not yet been created, create it and populate it.
		if (m_ipPersonAliasAKAList == __nullptr)
		{
			// Object creation
			m_ipPersonAliasAKAList.CreateInstance( CLSID_VariantVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI09604", m_ipPersonAliasAKAList != __nullptr );

			// Populate IVariantVector
			buildVariantVector( m_ipPersonAliasAKAList, "PersonAliasAKA" );
		}
		break;

	case kPersonAliasFKA:
		// If the list object has not yet been created, create it and populate it.
		if (m_ipPersonAliasFKAList == __nullptr)
		{
			// Object creation
			m_ipPersonAliasFKAList.CreateInstance( CLSID_VariantVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI09605", m_ipPersonAliasFKAList != __nullptr );

			// Populate IVariantVector
			buildVariantVector( m_ipPersonAliasFKAList, "PersonAliasFKA" );
		}
		break;

	case kPersonAliasNKA:
		// If the list object has not yet been created, create it and populate it.
		if (m_ipPersonAliasNKAList == __nullptr)
		{
			// Object creation
			m_ipPersonAliasNKAList.CreateInstance( CLSID_VariantVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI09606", m_ipPersonAliasNKAList != __nullptr );

			// Populate IVariantVector
			buildVariantVector( m_ipPersonAliasNKAList, "PersonAliasNKA" );
		}
		break;

	case kPersonAliasAll:
		// If the list object has not yet been created, create it and populate it.
		if (m_ipPersonAliasList == __nullptr)
		{
			// Object creation
			m_ipPersonAliasList.CreateInstance( CLSID_VariantVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI09607", m_ipPersonAliasList != __nullptr );

			// Populate IVariantVector
			buildVariantVector( m_ipPersonAliasList, "PersonAlias" );
		}
		break;

	default:
		// Unknown Alias type
		UCLIDException ue( "ELI09610", "Unable to make list for unknown Person Alias type!" );
		ue.addDebugInfo( "Person Alias Type", (long)eType );
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void CEntityKeywords::makeRelatedCompanyList(ERelatedCompanyType eType)
{
	// Alias collection is dependent on requested Type
	switch (eType)
	{
	case kRelatedCompanyDivision:
		// If the list object has not yet been created, create it and populate it.
		if (m_ipRelatedCompanyDivisionList == __nullptr)
		{
			// Object creation
			m_ipRelatedCompanyDivisionList.CreateInstance( CLSID_VariantVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI09651", m_ipRelatedCompanyDivisionList != __nullptr );

			// Populate IVariantVector
			buildVariantVector( m_ipRelatedCompanyDivisionList, "RelatedCompanyDivision" );
		}
		break;

	case kRelatedCompanySubdivision:
		// If the list object has not yet been created, create it and populate it.
		if (m_ipRelatedCompanySubdivisionList == __nullptr)
		{
			// Object creation
			m_ipRelatedCompanySubdivisionList.CreateInstance( CLSID_VariantVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI09652", m_ipRelatedCompanySubdivisionList != __nullptr );

			// Populate IVariantVector
			buildVariantVector( m_ipRelatedCompanySubdivisionList, "RelatedCompanySubdivision" );
		}
		break;

	case kRelatedCompanySubsidiary:
		// If the list object has not yet been created, create it and populate it.
		if (m_ipRelatedCompanySubsidiaryList == __nullptr)
		{
			// Object creation
			m_ipRelatedCompanySubsidiaryList.CreateInstance( CLSID_VariantVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI09653", m_ipRelatedCompanySubsidiaryList != __nullptr );

			// Populate IVariantVector
			buildVariantVector( m_ipRelatedCompanySubsidiaryList, "RelatedCompanySubsidiary" );
		}
		break;

	case kRelatedCompanyBranch:
		// If the list object has not yet been created, create it and populate it.
		if (m_ipRelatedCompanyBranchList == __nullptr)
		{
			// Object creation
			m_ipRelatedCompanyBranchList.CreateInstance( CLSID_VariantVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI09697", m_ipRelatedCompanyBranchList != __nullptr );

			// Populate IVariantVector
			buildVariantVector( m_ipRelatedCompanyBranchList, "RelatedCompanyBranch" );
		}
		break;

	case kRelatedCompanyAll:
		// If the list object has not yet been created, create it and populate it.
		if (m_ipRelatedCompanyList == __nullptr)
		{
			// Object creation
			m_ipRelatedCompanyList.CreateInstance( CLSID_VariantVector );
			ASSERT_RESOURCE_ALLOCATION( "ELI09649", m_ipRelatedCompanyList != __nullptr );

			// Populate IVariantVector
			buildVariantVector( m_ipRelatedCompanyList, "RelatedCompany" );
		}
		break;

	default:
		// Unknown Related Company type
		UCLIDException ue( "ELI09650", "Unable to make list for unknown Related Company type!" );
		ue.addDebugInfo( "Related Company Type", (long)eType );
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void CEntityKeywords::readKeywordsFile()
{
	// Clear the vector
	m_vecKeywords.clear();

	// Determine path to DAT file
	UCLID_AFUTILSLib::IAFUtilityPtr ipAFUtility( CLSID_AFUtility );
	ASSERT_RESOURCE_ALLOCATION( "ELI10038", ipAFUtility != __nullptr );
	string strComponentDataDir = ipAFUtility->GetComponentDataFolder();
	string strFileName =  strComponentDataDir + "\\AFUtility\\" + "Keywords.dat.etf";

	// Check for current encryption
	autoEncryptFile( strFileName, gstrAF_AUTO_ENCRYPT_KEY_PATH.c_str() );

	// Create List Reader
	vector<string> vecLines = convertFileToLines( strFileName );
	m_apKlr = unique_ptr<KeywordListReader>( new KeywordListReader(vecLines) );

	// Read DAT file and retain list of defined Keywords
	m_apKlr->ReadKeywords( m_vecKeywords );

	// Set flag
	m_bReadKeywordsFromFile = true;
}
//-------------------------------------------------------------------------------------------------
void CEntityKeywords::validateLicense()
{
	static const unsigned long ENTITY_KEYWORDS_COMPONENT_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE( ENTITY_KEYWORDS_COMPONENT_ID, "ELI05931", "Entity Keywords" );
}
//-------------------------------------------------------------------------------------------------
