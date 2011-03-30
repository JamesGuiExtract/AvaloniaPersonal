// EntityKeywords.h : Declaration of the CEntityKeywords

#pragma once

#include "resource.h"       // main symbols

#include <KeywordListReader.h>
#include <memory>

#include <string>
#include <vector>

/////////////////////////////////////////////////////////////////////////////
// CEntityKeywords
class ATL_NO_VTABLE CEntityKeywords : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CEntityKeywords, &CLSID_EntityKeywords>,
	public ISupportErrorInfo,
	public IDispatchImpl<IEntityKeywords, &IID_IEntityKeywords, &LIBID_UCLID_AFUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CEntityKeywords();

DECLARE_REGISTRY_RESOURCEID(IDR_ENTITYKEYWORDS)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CEntityKeywords)
	COM_INTERFACE_ENTRY(IEntityKeywords)
//DEL 	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IEntityKeywords)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IEntityKeywords
public:
	STDMETHOD(get_PersonTrimIdentifiers)(/*[out, retval]*/ IVariantVector * *pVal);
	STDMETHOD(get_EntityTrimTrailingPhrases)(/*[out, retval]*/ IVariantVector * *pVal);
	STDMETHOD(get_PersonAlias)(/*[in]*/ EPersonAliasType eType, 
		/*[out, retval]*/ IVariantVector * *pVal);
	STDMETHOD(get_CompanyAssignors)(/*[out, retval]*/ IVariantVector * *pVal);
	STDMETHOD(get_CompanyDesignators)(/*[out, retval]*/ IVariantVector * *pVal);
	STDMETHOD(get_CompanySuffixes)(/*[out, retval]*/ IVariantVector * *pVal);
	STDMETHOD(get_PersonDesignators)(/*[out, retval]*/ IVariantVector * *pVal);
	STDMETHOD(get_PersonSuffixes)(/*[out, retval]*/ IVariantVector * *pVal);
	STDMETHOD(get_PersonTitles)(/*[out, retval]*/ IVariantVector * *pVal);
	STDMETHOD(get_StreetNames)(/*[out, retval]*/ IVariantVector * *pVal);
	STDMETHOD(get_StreetAbbreviations)(/*[out, retval]*/ IVariantVector * *pVal);
	STDMETHOD(get_BuildingNames)(/*[out, retval]*/ IVariantVector * *pVal);
	STDMETHOD(get_BuildingAbbreviations)(/*[out, retval]*/ IVariantVector * *pVal);
	STDMETHOD(get_DirectionIndicators)(/*[out, retval]*/ IVariantVector * *pVal);
	STDMETHOD(get_CompanyAlias)(/*[in]*/ ECompanyAliasType eType, 
		/*[out, retval]*/ IVariantVector * *pVal);
	STDMETHOD(get_NumberWords)(/*[out, retval]*/ IVariantVector * *pVal);
	STDMETHOD(get_MonthWords)(/*[out, retval]*/ IVariantVector * *pVal);
	STDMETHOD(get_AddressIndicators)(/*[out, retval]*/ IVariantVector * *pVal);
	STDMETHOD(GetPersonAliasLabel)(/*[in]*/ EPersonAliasType eType, /*[out, retval]*/ BSTR *pVal);
	STDMETHOD(GetCompanyAliasLabel)(/*[in]*/ ECompanyAliasType eType, /*[out, retval]*/ BSTR *pVal);
	STDMETHOD(GetRelatedCompanyLabel)(/*[in]*/ ERelatedCompanyType eType, /*[out, retval]*/ BSTR *pVal);
	STDMETHOD(get_RelatedCompany)(/*[in]*/ ERelatedCompanyType eType, 
		/*[out, retval]*/ IVariantVector * *pVal);
	STDMETHOD(get_KeywordCollection)(/*[in]*/ BSTR strKeyword, 
		/*[out, retval]*/ IVariantVector * *pVal);
	STDMETHOD(get_KeywordPattern)(/*[in]*/ BSTR strKeyword, 
		/*[out, retval]*/ BSTR* pstrPattern);

private:
	///////////////
	// Data Members
	///////////////

	// Examples: Mr, Mrs, Dr
	IVariantVectorPtr	m_ipPersonTitleList;

	// Examples: Jr, Sr, III
	IVariantVectorPtr	m_ipPersonSuffixList;

	// Examples: Incorporated, Company, LLC
	IVariantVectorPtr	m_ipCompanySuffixList;
	
	// Examples: Bank, Mortgage, FSB
	IVariantVectorPtr	m_ipCompanyDesignatorList;
	
	// Examples: as assigned to
	IVariantVectorPtr	m_ipCompanyAssignorList;
	
	// Examples: n/k/a, whose address, FKA
	IVariantVectorPtr	m_ipPersonAliasList;

	// Examples: d/b/a, DBA
	IVariantVectorPtr	m_ipCompanyAliasList;

	// Examples: a single person, husband and wife, joint tenancy
	IVariantVectorPtr	m_ipPersonDesignatorList;

	// Examples: whose address is
	IVariantVectorPtr	m_ipEntityTrimTrailingPhraseList;

	// Examples: wife, spouse, husband
	IVariantVectorPtr	m_ipPersonTrimIdentifierList;

	// Examples: Avenue, Road, Street
	IVariantVectorPtr	m_ipStreetNameList;

	// Examples: Ave, Rd. St
	IVariantVectorPtr	m_ipStreetAbbreviationList;

	// Examples: Apartment, Suite
	IVariantVectorPtr	m_ipBuildingNameList;

	// Examples: Apt, Ste
	IVariantVectorPtr	m_ipBuildingAbbreviationList;

	// Examples: Attention, Attn, Return To
	IVariantVectorPtr	m_ipDirectionIndicatorList;

	// Examples: Five, Forty, Two, Hundred, Thousand
	IVariantVectorPtr	m_ipNumberWordList;

	// Examples: January, February, Mar, Apr
	IVariantVectorPtr	m_ipMonthWordList;

	// Examples: Whose Address Is, Rwesiding At
	IVariantVectorPtr	m_ipAddressIndicatorList;

	// Examples: A Division Of, A Subsidiary Of
	IVariantVectorPtr	m_ipRelatedCompanyList;

	// Sub-collections for Person Alias expressions
	IVariantVectorPtr	m_ipPersonAliasAKAList;
	IVariantVectorPtr	m_ipPersonAliasNKAList;
	IVariantVectorPtr	m_ipPersonAliasFKAList;

	// Sub-collections for Company Alias expressions
	IVariantVectorPtr	m_ipCompanyAliasDBAList;
	IVariantVectorPtr	m_ipCompanyAliasSBMList;
	IVariantVectorPtr	m_ipCompanyAliasSIIList;
	IVariantVectorPtr	m_ipCompanyAliasBMWList;

	// Sub-collections for Related Company expressions
	IVariantVectorPtr	m_ipRelatedCompanyDivisionList;
	IVariantVectorPtr	m_ipRelatedCompanySubdivisionList;
	IVariantVectorPtr	m_ipRelatedCompanySubsidiaryList;
	IVariantVectorPtr	m_ipRelatedCompanyBranchList;

	// DAT file reader
	std::unique_ptr<KeywordListReader> m_apKlr;

	// Vector of Keywords as read from DAT file
	std::vector<std::string>	m_vecKeywords;

	// Flag indicating if DAT file has already been read
	bool	m_bReadKeywordsFromFile;

	//////////
	// Methods
	//////////

	// Retrieve collection of strings associated with strKeyword and 
	// populate ipList
	void	buildVariantVector(IVariantVectorPtr ipList, std::string strKeyword);

	// Create and populate specified sublist of Company Alias items
	void	makeCompanyAliasList(ECompanyAliasType eType);

	// Create and populate specified sublist of Person Alias items
	void	makePersonAliasList(EPersonAliasType eType);

	// Create and populate specified sublist of Related Company items
	void	makeRelatedCompanyList(ERelatedCompanyType eType);

	// Reads Keywords DAT (or ETF) file
	void	readKeywordsFile();

	// Check license state for this component
	void	validateLicense();
};
