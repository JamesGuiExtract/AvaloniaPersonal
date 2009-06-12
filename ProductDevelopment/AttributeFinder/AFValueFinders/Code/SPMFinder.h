// SPMFinder.h : Declaration of the CSPMFinder

#pragma once

#include "resource.h"       // main symbols
#include "PatternFileInterpreter.h"

#include "..\..\AFCore\Code\AFCategories.h"

#include <string>
#include <map>

/////////////////////////////////////////////////////////////////////////////
// CSPMFinder
class ATL_NO_VTABLE CSPMFinder : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CSPMFinder, &CLSID_SPMFinder>,
	public ISupportErrorInfo,
	public IDispatchImpl<ISPMFinder, &IID_ISPMFinder, &LIBID_UCLID_AFVALUEFINDERSLib>,
	public IDispatchImpl<IAttributeFindingRule, &IID_IAttributeFindingRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IPersistStream,
	public ISpecifyPropertyPagesImpl<CSPMFinder>
{
public:
	CSPMFinder();
	~CSPMFinder();

DECLARE_REGISTRY_RESOURCEID(IDR_SPMFINDER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CSPMFinder)
	COM_INTERFACE_ENTRY(ISPMFinder)
	COM_INTERFACE_ENTRY2(IDispatch,ISPMFinder)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IAttributeFindingRule)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CSPMFinder)
	PROP_PAGE(CLSID_SPMFinderPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CSPMFinder)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_VALUE_FINDERS)
END_CATEGORY_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IAttributeFindingRule
	STDMETHOD(raw_ParseText)(IAFDocument* pAFDoc, IProgressStatus *pProgressStatus,
		IIUnknownVector **pAttributes);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ICopyableObject
	STDMETHOD(raw_Clone)(/*[out, retval]*/ IUnknown* *pObject);
	STDMETHOD(raw_CopyFrom)(/*[in]*/ IUnknown *pObject);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL * pbValue);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// ISPMFinder
public:
	STDMETHOD(get_MinFirstToConsiderAsMatch)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_MinFirstToConsiderAsMatch)(/*[in]*/ long newVal);
	STDMETHOD(get_Preprocessors)(/*[out, retval]*/ IVariantVector **pVal);
	STDMETHOD(put_Preprocessors)(/*[in]*/ IVariantVector *newVal);
	STDMETHOD(get_GreedySearch)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_GreedySearch)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_TreatMultipleWSAsOne)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_TreatMultipleWSAsOne)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_CaseSensitive)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_CaseSensitive)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_StoreRuleWorked)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_StoreRuleWorked)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_RuleWorkedName)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_RuleWorkedName)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_RulesText)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_RulesText)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_RulesFileName)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_RulesFileName)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_IsPatternsFromFile)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_IsPatternsFromFile)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_DataScorer)(/*[out, retval]*/ IObjectWithDescription **ppObj);
	STDMETHOD(put_DataScorer)(/*[in]*/ IObjectWithDescription *pObj);
	STDMETHOD(get_MinScoreToConsiderAsMatch)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_MinScoreToConsiderAsMatch)(/*[in]*/ long newVal);
	STDMETHOD(get_ReturnMatchType)(/*[out, retval]*/ ESPMReturnMatchType *peVal);
	STDMETHOD(put_ReturnMatchType)(/*[in]*/ ESPMReturnMatchType eNewVal);
	STDMETHOD(get_IgnoreInvalidTags)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_IgnoreInvalidTags)(/*[in]*/ VARIANT_BOOL newVal);

private:	
	////////////
	// Variables
	////////////
	// flag to keep track of whether this object has been modified
	// since the last save-to-stream operation
	bool m_bDirty;

	ESPMReturnMatchType m_eReturnMatchType;
	long m_nMinScoreToConsiderAsMatch;

	// Only used if m_eReturnMatchType is kReturnFirstOrBest
	long m_nMinFirstToConsiderAsMatch;

	bool m_bIsPatternsFromFile;
	bool m_bCaseSensitive;
	bool m_bMultipleWSAsOne;
	bool m_bGreedySearch;
	bool m_bStoreRuleWorked;
	std::string m_strRulesFileName;
	std::string m_strRulesText;
	std::string m_strRuleWorkedName;
	IStringPatternMatcherPtr m_ipSPM;
	IAFUtilityPtr	m_ipAFUtility;
	IObjectWithDescriptionPtr m_ipDataScorer;

	bool m_bIgnoreInvalidTags;

	// map of pattern file name to PatternFileInterpreter
	std::map<std::string, PatternFileInterpreter> m_mapFileNameToInterpreter;

	IVariantVectorPtr m_ipPreprocessors;

	///////////
	// Methods
	///////////
	//----------------------------------------------------------------------------------------------
	UCLID_AFVALUEFINDERSLib::ISPMFinderPtr getThisAsCOMPtr();
	//----------------------------------------------------------------------------------------------
	// Whether or not the input file name string contains
	// valid <DocType> tag.
	// Return true if the file contains no <DocType> tag at all, or contains
	// one or more such tags. 
	bool containsValidDocTypeTag(const std::string& strFileName);

	IAFUtilityPtr getAFUtility();

	// Replace all <DocType> with current document type name,
	// auto encrypt file into .etf file if required, put prefix
	// in front of file name if any, and return the file name in full
	std::string getInputFileName(const std::string& strInputFile, IAFDocumentPtr ipAFDoc);

	// load pattern file
	// strPatternFileName - a fully specified file name
	void loadPatternFile(const std::string& strPatternFileName);

	void validateLicense();

	// whether or not the user has the RDT license in order to
	// define their own patterns (or pattern files)
	void validateRDTLicense();
};

