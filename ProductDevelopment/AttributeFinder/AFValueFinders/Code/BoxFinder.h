// BoxFinder.h : Declaration of the CBoxFinder

#pragma once
#include "resource.h"       // main symbols
#include "AFValueFinders.h"
#include "..\..\AFCore\Code\AFCategories.h"

#include <CachedListLoader.h>

#include <string>
#include <vector>
using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CBoxFinder
/////////////////////////////////////////////////////////////////////////////
class ATL_NO_VTABLE CBoxFinder :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CBoxFinder, &CLSID_BoxFinder>,
	public IPersistStream,
	public ISupportErrorInfo,
	public IDispatchImpl<IBoxFinder, &IID_IBoxFinder, &LIBID_UCLID_AFVALUEFINDERSLib>,
	public IDispatchImpl<IAttributeFindingRule, &IID_IAttributeFindingRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public ISpecifyPropertyPagesImpl<CBoxFinder>
{
public:
	CBoxFinder();
	~CBoxFinder();

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();
	void FinalRelease();

	DECLARE_REGISTRY_RESOURCEID(IDR_BOXFINDER)

	BEGIN_COM_MAP(CBoxFinder)
		COM_INTERFACE_ENTRY(IBoxFinder)
		COM_INTERFACE_ENTRY2(IDispatch, IBoxFinder)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
		COM_INTERFACE_ENTRY(IAttributeFindingRule)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
		COM_INTERFACE_ENTRY(IPersistStream)
		COM_INTERFACE_ENTRY(ILicensedComponent)
		COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
		COM_INTERFACE_ENTRY(ICategorizedComponent)
		COM_INTERFACE_ENTRY(ICopyableObject)
		COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
	END_COM_MAP()

	BEGIN_PROP_MAP(CBoxFinder)
		PROP_PAGE(CLSID_BoxFinderPP)
	END_PROP_MAP()

	BEGIN_CATEGORY_MAP(CBoxFinder)
		IMPLEMENTED_CATEGORY(CATID_AFAPI_VALUE_FINDERS)
	END_CATEGORY_MAP()

// IBoxFinder
	STDMETHOD(get_Clues)(IUnknown **ppVal);
	STDMETHOD(put_Clues)(IUnknown *pNewVal);
	STDMETHOD(get_CluesAreRegularExpressions)(VARIANT_BOOL *pVal);
	STDMETHOD(put_CluesAreRegularExpressions)(VARIANT_BOOL newVal);
	STDMETHOD(get_CluesAreCaseSensitive)(VARIANT_BOOL *pVal);
	STDMETHOD(put_CluesAreCaseSensitive)(VARIANT_BOOL newVal);
	STDMETHOD(get_ClueLocation)(EClueLocation *pVal);
	STDMETHOD(put_ClueLocation)(EClueLocation newVal);
	STDMETHOD(get_PageSelectionMode)(EPageSelectionMode *pVal);
	STDMETHOD(put_PageSelectionMode)(EPageSelectionMode newVal);
	STDMETHOD(get_NumFirstPages)(long *pVal);
	STDMETHOD(put_NumFirstPages)(long newVal);
	STDMETHOD(get_NumLastPages)(long *pVal);
	STDMETHOD(put_NumLastPages)(long newVal);
	STDMETHOD(get_SpecifiedPages)(BSTR *pVal);
	STDMETHOD(put_SpecifiedPages)(BSTR newVal);
	STDMETHOD(get_BoxWidthMin)(long *pVal);
	STDMETHOD(put_BoxWidthMin)(long newVal);
	STDMETHOD(get_BoxWidthMax)(long *pVal);
	STDMETHOD(put_BoxWidthMax)(long newVal);
	STDMETHOD(get_BoxHeightMin)(long *pVal);
	STDMETHOD(put_BoxHeightMin)(long newVal);
	STDMETHOD(get_BoxHeightMax)(long *pVal);
	STDMETHOD(put_BoxHeightMax)(long newVal);
	STDMETHOD(get_FindType)(EFindType *pVal);
	STDMETHOD(put_FindType)(EFindType newVal);
	STDMETHOD(get_AttributeText)(BSTR *pVal);
	STDMETHOD(put_AttributeText)(BSTR newVal);
	STDMETHOD(get_ExcludeClueArea)(VARIANT_BOOL *pVal);
	STDMETHOD(put_ExcludeClueArea)(VARIANT_BOOL newVal);
	STDMETHOD(get_IncludeClueText)(VARIANT_BOOL *pVal);
	STDMETHOD(put_IncludeClueText)(VARIANT_BOOL newVal);
	STDMETHOD(get_IncludeLines)(VARIANT_BOOL *pVal);
	STDMETHOD(put_IncludeLines)(VARIANT_BOOL newVal);
	STDMETHOD(get_FirstBoxOnly)(VARIANT_BOOL *pVal);
	STDMETHOD(put_FirstBoxOnly)(VARIANT_BOOL newVal);

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

	IMiscUtilsPtr m_ipMisc;

	// Clue list (vector of BSTRs)
	IVariantVectorPtr m_ipClues;

	// How the clue list should be interpreted
	bool m_bCluesAreRegularExpressions;
	bool m_bCluesAreCaseSensitive;
	bool m_bFirstBoxOnly;

	// Clue location relative to target box
	EClueLocation m_eClueLocation;

	// The page selection mode to determine which pages are processed
	EPageSelectionMode m_ePageSelectionMode;

	// Depending on m_ePageSelectionMode, which pages to process
	long m_nNumFirstPages;
	long m_nNumLastPages;
	string m_strSpecifiedPages;

	// Required dimensions of box. Any of these can be -1 to represent an unspecified dimension
	long m_nBoxWidthMin;
	long m_nBoxWidthMax;
	long m_nBoxHeightMin;
	long m_nBoxHeightMax;

	// Whether to return a spatial area or the text in the box
	EFindType m_eFindType;

	// The text to assign to any resulting spatial attribute
	string m_strAttributeText;

	// What to do with the clue
	bool m_bExcludeClueArea;
	bool m_bIncludeClueText;

	// Specifies whether lines found on the page should be included in the output
	bool m_bIncludeLines;

	// The ImageLineUtility object is used to find and group lines as well store
	// settings governing the line and region detection
	IImageLineUtilityPtr m_ipImageLineUtility;

	// The spatial string searcher is used to locate clues within boxes
	ISpatialStringSearcherPtr m_ipSpatialStringSearcher;

	// Cached list loader object to read clues from files
	CCachedListLoader m_cachedListLoader;

	/////////////////
	// Methods
	/////////////////

	// PROMISE: Retrives or creates and validates m_ipImageLineUtility as well as sets default values
	//			that work well for finding boxes on pages.
	IImageLineUtilityPtr getImageLineUtility();

	// PROMISE: Retrives or creates and validates m_ipSpatialStringSearcher
	ISpatialStringSearcherPtr getSpatialStringSearcher();

	// PROMISE: Creates an IAttributePtr containing a hybrid string from the provided variables
	// ARGS:	ipRects- An IIUnknownVectorPr of ILongRectangles that specify the image areas
	//			strText- The text to assign to the attribute
	//			strSourceDocName- The document name to link to the attribute raster zones
	//          ipPageInfoMap - The spatial page info map associated with the source document
	//			nPageNum- The page number to link to the attribute raster zone
	IAttributePtr createAttributeFromRects(IIUnknownVectorPtr ipRects, const string &strText, 
		const string &strSourceDocName, ILongToObjectMapPtr ipPageInfoMap, int nPageNum);

	// PROMISE: Creates an IAttributePtr with an artificially created spatial string 
	//			occupying specified location.
	// ARGS:	ipDocText- Spatial string belonging to the document for which the attribute
	//				will be created.
	//			nPageNum- The page number on which to create the attribute
	//			ipRect- Rectangle describing the bounds which the spatial string should
	//				completely fill.
	IAttributePtr createRegionResult(ISpatialStringPtr ipDocText, int nPageNum,
										ILongRectanglePtr ipRect);

	// PROMISE: Creates an IAttributePtr representing the text found in the found box
	// ARGS:	ipAFDoc- The document
	//			ipBox- A rectangle representing the box that was found
	//			ipClue- The found instance of a clue that was used to find the box
	IAttributePtr createTextResult(IAFDocumentPtr ipAFDoc, ILongRectanglePtr ipBox, 
		ISpatialStringPtr ipClue, IRegularExprParserPtr ipParser);
	
	// PROMISE: Finds any clues on the specified page of the document text provided
	//			using the provided clue list
	// ARGS:	ipClues- The clues to search for
	//			ipDocText- The document's text
	//			nPageNum- The page of the document to search
	//			rbPageContainsText- Set to true by getCluesOnPage if the specified
	//				page contained text, false if it did not.
	IIUnknownVectorPtr getCluesOnPage(IVariantVectorPtr ipClues, ISpatialStringPtr ipDocText, 
		int nPageNum, bool &rbPageContainsText, IRegularExprParserPtr ipParser);

	// PROMISE: Searches for a box using the provided instance of a clue that was found
	// ARGS:	ripClueRect- Rectangle representing the clue bounds relative to the native
	//				document (rather than relative to the page text's page info)
	//			ipHorzLineRects- The horizontal lines from the document page
	//			ipVertLineRects- The vertical lines from the document page
	//			rbIncompleteResults- If false, the algorithm short-circuited after 
	//								 encountering too many potential boxes.
	ILongRectanglePtr findBoxContainingClue(ILongRectanglePtr ipNativeClueRect,
											IIUnknownVectorPtr ipHorzLineRects,
											IIUnknownVectorPtr ipVertLineRects,
											bool &rbIncompleteResult);

	// PROMISE: Given a retangle indicating the location of a box containing a clue, returns a
	//			rectangle that specifyies where to search for the box that contains the desired 
	//			data.
	ILongRectanglePtr createDataSearchRect(ILongRectanglePtr ipClueRect);

	// PROMISE: Returns true if the indicated box meets the required dimensions and
	//			if we haven't already found the same box. ripExistingBoxes is vector of
	//			boxes that have already qualified to prevent duplicate boxes from 
	//			qualifying.
	bool qualifyBox(ILongRectanglePtr ipRect, ISpatialPageInfoPtr ipPageInfo,
					IIUnknownVectorPtr &ripExistingBoxes);

	// PROMISE: Returns a rectangle that excludes the vertical extent of the clue used to 
	//			find the box.  If the entire rectangle is excluded, NULL is returned.
	//			Relevant only if m_eClueLocation is kSameBox, kBoxToLeft or kBoxToRight.
	ILongRectanglePtr excludeVerticalSpatialAreaOfClue(ILongRectanglePtr ipFoundBox, 
		ILongRectanglePtr ipClueBounds);

	// PROMISE: Returns a clues list with any instances of files from the clue list ("file://") 
	// with the clues from that file in place of the file specifier.
	IVariantVectorPtr getExpandedClueList(IAFDocumentPtr ipAFDoc);

	// PROMISE: Returns a vector of page numbers to search from the specified document text
	vector<int> getPagesToSearch(ISpatialStringPtr ipDocText);

	// PROMISE: Reset all data members to default values
	void resetDataMembers();

	void validateLicense();
};

OBJECT_ENTRY_AUTO(__uuidof(BoxFinder), CBoxFinder)
