// SelectPageRegion.h : Declaration of the CSelectPageRegion

#pragma once

#include "resource.h"       // main symbols
#include "..\..\AFCore\Code\AFCategories.h"

#include <string>
#include <vector>

/////////////////////////////////////////////////////////////////////////////
// CSelectPageRegion
class ATL_NO_VTABLE CSelectPageRegion : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CSelectPageRegion, &CLSID_SelectPageRegion>,
	public ISupportErrorInfo,
	public IDispatchImpl<ISelectPageRegion, &IID_ISelectPageRegion, &LIBID_UCLID_AFPREPROCESSORSLib>,
	public IDispatchImpl<IAttributeFindingRule, &IID_IAttributeFindingRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<IDocumentPreprocessor, &IID_IDocumentPreprocessor, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IPersistStream,
	public ISpecifyPropertyPagesImpl<CSelectPageRegion>
{
public:
	CSelectPageRegion();
	~CSelectPageRegion();

DECLARE_REGISTRY_RESOURCEID(IDR_SELECTPAGEREGION)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CSelectPageRegion)
	COM_INTERFACE_ENTRY(ISelectPageRegion)
	COM_INTERFACE_ENTRY2(IDispatch, ISelectPageRegion)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IAttributeFindingRule)
	COM_INTERFACE_ENTRY(IDocumentPreprocessor)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
	COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
END_COM_MAP()

BEGIN_PROP_MAP(CSelectPageRegion)
	PROP_PAGE(CLSID_SelectPageRegionPP)
END_PROP_MAP()

BEGIN_CATEGORY_MAP(CSelectPageRegion)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_DOCUMENT_PREPROCESSORS)
	IMPLEMENTED_CATEGORY(CATID_AFAPI_VALUE_FINDERS)
END_CATEGORY_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ISelectPageRegion
	STDMETHOD(GetVerticalRestriction)(/*[in, out]*/ long* pnStartPercentage, /*[in, out]*/ long* pnEndPercentage);
	STDMETHOD(SetVerticalRestriction)(/*[in]*/ long nStartPercentage, /*[in]*/ long nEndPercentage);
	STDMETHOD(GetHorizontalRestriction)(/*[in, out]*/ long* pnStartPercentage, /*[in, out]*/ long* pnEndPercentage);
	STDMETHOD(SetHorizontalRestriction)(/*[in]*/ long nStartPercentage, /*[in]*/ long nEndPercentage);
	STDMETHOD(SelectPages)(/*[in]*/ VARIANT_BOOL bSpecificPages, /*[in]*/ BSTR strSpecificPages);
	STDMETHOD(GetPageSelections)(/*[in, out]*/ VARIANT_BOOL *pbSpecificPages, /*[in, out]*/ BSTR *pstrSpecificPages);
	STDMETHOD(get_IncludeRegionDefined)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_IncludeRegionDefined)(/*[in]*/ VARIANT_BOOL newVal);

	STDMETHOD(get_PageSelectionType)(/*[out, retval]*/ EPageSelectionType *pVal);
	STDMETHOD(put_PageSelectionType)(/*[in]*/ EPageSelectionType newVal);

	STDMETHOD(get_Pattern)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_Pattern)(/*[in]*/ BSTR newVal);

	STDMETHOD(get_IsRegExp)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_IsRegExp)(/*[in]*/ VARIANT_BOOL newVal);

	STDMETHOD(get_IsCaseSensitive)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_IsCaseSensitive)(/*[in]*/ VARIANT_BOOL newVal);

	STDMETHOD(get_RegExpPageSelectionType)(/*[out, retval]*/ ERegExpPageSelectionType *pVal);
	STDMETHOD(put_RegExpPageSelectionType)(/*[in]*/ ERegExpPageSelectionType newVal);
	STDMETHOD(get_OCRSelectedRegion)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_OCRSelectedRegion)(/*[in]*/ VARIANT_BOOL newVal);
	STDMETHOD(get_SelectedRegionRotation)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_SelectedRegionRotation)(/*[in]*/ long newVal);

// IAttributeFindingRule
	STDMETHOD(raw_ParseText)(IAFDocument* pAFDoc, IProgressStatus *pProgressStatus,
		IIUnknownVector **pAttributes);

// IDocumentPreprocessor
	STDMETHOD(raw_Process)(/*[in]*/ IAFDocument* pDocument,/*[in]*/  IProgressStatus *pProgressStatus);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pstrComponentDescription);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL * pbValue);

// ICopyableObject
	STDMETHOD(raw_Clone)(/*[out, retval]*/ IUnknown* *pObject);
	STDMETHOD(raw_CopyFrom)(/*[in]*/ IUnknown *pObject);

private:

	////////////
	// Variables
	////////////
	bool m_bDirty;

	// Include/exclude region defined
	bool m_bIncludeRegion;

	// if select specific pages, what are they
	std::string m_strSpecificPages;

	// horizontal restriction
	long m_nHorizontalStartPercentage;
	long m_nHorizontalEndPercentage;

	// vertical restriction
	long m_nVerticalStartPercentage;
	long m_nVerticalEndPercentage;

	// spatial string searcher
	ISpatialStringSearcherPtr m_ipSpatialStringSearcher;

	// Utility Functions
	IAFUtilityPtr m_ipAFUtility;

	// For regular expression searching
	// The string to match which may or may not be a regular expression
	std::string m_strPattern;
	// true if m_strSearch is to be treated as a regular expression
	bool m_bIsRegExp;
	// true if m_strSearch is to be treated as case sensitive
	bool m_bIsCaseSensitive;
	EPageSelectionType m_ePageSelectionType;

	ERegExpPageSelectionType m_eRegExpPageSelectionType;

	// OCR the selected region
	bool m_bOCRSelectedRegion;

	// Rotation in degrees for OCR of selected region
	long m_nRegionRotation;

	////////////
	// Methods
	////////////

	// based on specified pages, get actual page numbers to be extracted
	// For instance, if specified pages are 2, 3, 5, total last page
	// is 9, and it is set to include region defined, then the return vector
	// shall contain 2,3 and 5. If it is set to exclude region defined, then 
	// the return vector shall contain 1,4,6,7,8,9
	std::vector<int> getActualPageNumbers(int nLastPageNumber, ISpatialStringPtr ipInputText, IAFDocumentPtr ipAFDoc);

	// return actual page content based on the restriction defined
	ISpatialStringPtr getIndividualPageContent(ISpatialStringPtr ipOriginPage);

	IOCREnginePtr getOCREngine();

	// based on the specific page and selection, get the actual string out
	ISpatialStringPtr getRegionContent(ISpatialStringPtr ipPageText, bool bPageSpecified);

	// check the horizontal and vertical restrictions to see if at least one
	// of them is defined.
	bool isRestrictionDefined();

	void validateLicense();

	// make sure start and end percentages are well within 0 ~ 100
	void validateStartEndPercentage(long nStartPercentage, long nEndPercentage);
};
