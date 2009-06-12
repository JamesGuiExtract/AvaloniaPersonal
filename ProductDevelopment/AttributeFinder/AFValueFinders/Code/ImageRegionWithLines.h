// ImageRegionWithLines.h : Declaration of the CImageRegionWithLines

#pragma once
#include "resource.h"       // main symbols
#include "AFValueFinders.h"
#include "..\..\AFCore\Code\AFCategories.h"

#include <string>
#include <vector>
using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CImageRegionWithLines
/////////////////////////////////////////////////////////////////////////////
class ATL_NO_VTABLE CImageRegionWithLines :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CImageRegionWithLines, &CLSID_ImageRegionWithLines>,
	public IPersistStream,
	public ISupportErrorInfo,
	public IDispatchImpl<IImageRegionWithLines, &IID_IImageRegionWithLines, &LIBID_UCLID_AFVALUEFINDERSLib>,
	public IDispatchImpl<IAttributeFindingRule, &IID_IAttributeFindingRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public ISpecifyPropertyPagesImpl<CImageRegionWithLines>
{
public:
	CImageRegionWithLines();
	~CImageRegionWithLines();

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();
	void FinalRelease();

	DECLARE_REGISTRY_RESOURCEID(IDR_IMAGEREGIONWITHLINES)

	BEGIN_COM_MAP(CImageRegionWithLines)
		COM_INTERFACE_ENTRY(IImageRegionWithLines)
		COM_INTERFACE_ENTRY2(IDispatch, IImageRegionWithLines)
		COM_INTERFACE_ENTRY(IAttributeFindingRule)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
		COM_INTERFACE_ENTRY(IPersistStream)
		COM_INTERFACE_ENTRY(ILicensedComponent)
		COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
		COM_INTERFACE_ENTRY(ICategorizedComponent)
		COM_INTERFACE_ENTRY(ICopyableObject)
		COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
	END_COM_MAP()

	BEGIN_PROP_MAP(CImageRegionWithLines)
		PROP_PAGE(CLSID_ImageRegionWithLinesPP)
	END_PROP_MAP()

	BEGIN_CATEGORY_MAP(CFindingRuleCondition)
		IMPLEMENTED_CATEGORY(CATID_AFAPI_VALUE_FINDERS)
	END_CATEGORY_MAP()

// IImageRegionWithLines
	STDMETHOD(get_LineUtil)(IUnknown **ppVal);
	STDMETHOD(put_LineUtil)(IUnknown *pNewVal);
	STDMETHOD(get_PageSelectionMode)(EPageSelectionMode *pVal);
	STDMETHOD(put_PageSelectionMode)(EPageSelectionMode newVal);
	STDMETHOD(get_NumFirstPages)(long *pVal);
	STDMETHOD(put_NumFirstPages)(long newVal);
	STDMETHOD(get_NumLastPages)(long *pVal);
	STDMETHOD(put_NumLastPages)(long newVal);
	STDMETHOD(get_SpecifiedPages)(BSTR *pVal);
	STDMETHOD(put_SpecifiedPages)(BSTR newVal);
	STDMETHOD(get_AttributeText)(BSTR *pVal);
	STDMETHOD(put_AttributeText)(BSTR newVal);
	STDMETHOD(get_IncludeLines)(VARIANT_BOOL *pVal);
	STDMETHOD(put_IncludeLines)(VARIANT_BOOL newVal);

// IAttributeFindingRule
	STDMETHOD(raw_ParseText)(IAFDocument* pAFDoc, IProgressStatus *pProgressStatus,
		IIUnknownVector **ppAttributes);

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

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
	STDMETHOD(raw_Clone)(IUnknown* *pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown *pObject);

private:
	/////////////////
	// Variables
	/////////////////

	bool m_bDirty;

	// The ImageLineUtility object is used to find and group lines as well store
	// settings governing the line and region detection
	IImageLineUtilityPtr m_ipImageLineUtility;

	// The page selection mode to determine which pages are processed
	EPageSelectionMode m_ePageSelectionMode;

	// Depending on m_ePageSelectionMode, which pages to process
	long m_nNumFirstPages;
	long m_nNumLastPages;
	string m_strSpecifiedPages;

	// The text to assign to any resulting attributes
	string m_strAttributeText;
	
	// Specifies whether lines that form the image areas will be included in the rule output
	bool m_bIncludeLines;

	/////////////////
	// Methods
	/////////////////

	// Retrives or creates and validates m_ipImageLineUtility
	IImageLineUtilityPtr getImageLineUtility();

	// PROMISE: Creates a hybrid string attribute from the provided variables
	// ARGS:	ipRects- An IIUnknownVectorPr of ILongRectangles that specify the image areas
	//			strText- The text to assign to the attribute
	//			strSourceDocName- The document name to link to the attribute raster zones
	//          ipPageInfoMap - The spatial page info map associated with the source document
	//			nPageNum- The page number to link to the attribute raster zone
	IAttributePtr createAttributeFromRects(IIUnknownVectorPtr ipRects, const string &strText, 
		const string &strSourceDocName, ILongToObjectMapPtr ipPageInfoMap, int nPageNum);

	// PROMISE: Creates a spatial string attribute from the provided variables
	// ARGS:	ipRect- An ILongRectangle that specifies the spatial area
	//			ipDocText- Spatial string belonging to the document for which the attribute
	//				will be created.
	//			nPageNum- The page number on which the spatial string should be created.
	IAttributePtr createSpatialAttribute(ILongRectanglePtr ipRect, 
										 ISpatialStringPtr ipDocText,
										 int nPageNum);

	void validateLicense();
};

OBJECT_ENTRY_AUTO(__uuidof(ImageRegionWithLines), CImageRegionWithLines)