// SplitRegionIntoContentAreas.h : Declaration of the CSplitRegionIntoContentAreas

#pragma once
#include "resource.h"       // main symbols
#include "AFValueModifiers.h"
#include "..\..\AFCore\Code\AFCategories.h"

#include <FindLines.h>
#include <LeadToolsBitmap.h>

#include <string>
#include <vector>
#include <list>
#include <memory>
using namespace std;

// The CSplitRegionIntoContentAreas rule will use many implementations of virtual PixelProcessor 
// class to systematically implement various types of operations by traversing each pixel in an image
// region.  The macro DECLARE_PIXEL_PROCESSOR is declared here to simplify the declaration
// of a new PixelProcessor implementation
#define DECLARE_PIXEL_PROCESSOR(x) \
	class x : public PixelProcessor\
	{ \
	public : \
		x(CSplitRegionIntoContentAreas *pparent, CRect rect) : PixelProcessor(pparent, rect) {} \
		virtual int x::processPixel(int x, int y); \
	};

//--------------------------------------------------------------------------------------------------
// CSplitRegionIntoContentAreas
//--------------------------------------------------------------------------------------------------
class ATL_NO_VTABLE CSplitRegionIntoContentAreas :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CSplitRegionIntoContentAreas, &CLSID_SplitRegionIntoContentAreas>,
	public IPersistStream,
	public ISupportErrorInfo,
	public IDispatchImpl<ISplitRegionIntoContentAreas, &IID_ISplitRegionIntoContentAreas, &LIBID_UCLID_AFVALUEMODIFIERSLib>,
	public IDispatchImpl<IAttributeModifyingRule, &IID_IAttributeModifyingRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<IAttributeSplitter, &IID_IAttributeSplitter, &LIBID_UCLID_AFSPLITTERSLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public ISpecifyPropertyPagesImpl<CSplitRegionIntoContentAreas>
{
public:
	CSplitRegionIntoContentAreas();
	~CSplitRegionIntoContentAreas();

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();
	void FinalRelease();

	DECLARE_REGISTRY_RESOURCEID(IDR_SPLITREGIONINTOCONTENTAREAS)

	BEGIN_COM_MAP(CSplitRegionIntoContentAreas)
		COM_INTERFACE_ENTRY(ISplitRegionIntoContentAreas)
		COM_INTERFACE_ENTRY2(IDispatch, ISplitRegionIntoContentAreas)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
		COM_INTERFACE_ENTRY(IPersistStream)
		COM_INTERFACE_ENTRY(IAttributeModifyingRule)
		COM_INTERFACE_ENTRY(IAttributeSplitter)
		COM_INTERFACE_ENTRY(ICategorizedComponent)
		COM_INTERFACE_ENTRY(ICopyableObject)
		COM_INTERFACE_ENTRY(ILicensedComponent)
		COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
		COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
	END_COM_MAP()

	BEGIN_PROP_MAP(CSplitRegionIntoContentAreas)
		PROP_PAGE(CLSID_SplitRegionIntoContentAreasPP)
	END_PROP_MAP()

	BEGIN_CATEGORY_MAP(CSplitRegionIntoContentAreas)
		IMPLEMENTED_CATEGORY(CATID_AFAPI_VALUE_MODIFIERS)
		IMPLEMENTED_CATEGORY(CATID_AFAPI_ATTRIBUTE_SPLITTERS)
	END_CATEGORY_MAP()

// ISplitRegionIntoContentAreas
	STDMETHOD(get_DefaultAttributeText)(BSTR *pVal);
	STDMETHOD(put_DefaultAttributeText)(BSTR newVal);
	STDMETHOD(get_AttributeName)(BSTR *pVal);
	STDMETHOD(put_AttributeName)(BSTR newVal);
	STDMETHOD(get_MinimumWidth)(double *pVal);
	STDMETHOD(put_MinimumWidth)(double newVal);
	STDMETHOD(get_MinimumHeight)(double *pVal);
	STDMETHOD(put_MinimumHeight)(double newVal);
	STDMETHOD(get_IncludeGoodOCR)(VARIANT_BOOL *pVal);
	STDMETHOD(put_IncludeGoodOCR)(VARIANT_BOOL newVal);
	STDMETHOD(get_IncludePoorOCR)(VARIANT_BOOL *pVal);
	STDMETHOD(put_IncludePoorOCR)(VARIANT_BOOL newVal);
	STDMETHOD(get_GoodOCRType)(BSTR *pVal);
	STDMETHOD(put_GoodOCRType)(BSTR newVal);
	STDMETHOD(get_PoorOCRType)(BSTR *pVal);
	STDMETHOD(put_PoorOCRType)(BSTR newVal);
	STDMETHOD(get_OCRThreshold)(long *pVal);
	STDMETHOD(put_OCRThreshold)(long newVal);
	STDMETHOD(get_UseLines)(VARIANT_BOOL *pVal);
	STDMETHOD(put_UseLines)(VARIANT_BOOL newVal);
	STDMETHOD(get_ReOCRWithHandwriting)(VARIANT_BOOL *pVal);
	STDMETHOD(put_ReOCRWithHandwriting)(VARIANT_BOOL newVal);
	STDMETHOD(get_IncludeOCRAsTrueSpatialString)(VARIANT_BOOL *pVal);
	STDMETHOD(put_IncludeOCRAsTrueSpatialString)(VARIANT_BOOL newVal);
	STDMETHOD(get_RequiredHorizontalSeparation)(long *pVal);
	STDMETHOD(put_RequiredHorizontalSeparation)(long newVal);

// IAttributeModifyingRule
	STDMETHOD(raw_ModifyValue)(IAttribute *pAttribute, IAFDocument *pOriginInput,
		IProgressStatus* pProgressStatus);

// IAttributeSplitter
	STDMETHOD(raw_SplitAttribute)(IAttribute *pAttribute, IAFDocument *pAFDoc, 
		IProgressStatus *pProgressStatus);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR *pstrComponentDescription);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL *pbValue);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown* *pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown *pObject);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL *pbValue);

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

private:

	/////////////////
	// Enums
	/////////////////

	enum EOrientation
	{
		kHorizontal,
		kVertical,
		kBoth
	};

	enum EBoundaryState
	{
		kNotFound,
		kFound,
		kLocked
	};

	/////////////////
	// Classes
	/////////////////

	// The PixelProcessor class is used to perform an operation on an image area by acting on each
	// pixel contained in the image area.  It is virtual and cannot be used directly.  Use the 
	// DECLARE_PIXEL_PROCESSOR macro to declare a new derivative and then define the processPixel
	// function.
	class PixelProcessor abstract
	{
	public:

		// Creates a PixelProcessor based on the specified portion of the currently loaded bitmap.
		// pparent- A pointer to the CSplitRegionIntoContentAreas that created this processor
		// rect- The portion of pparent's currently loaded image that should be processed.
		PixelProcessor(CSplitRegionIntoContentAreas *pparent, CRect rect);
		virtual ~PixelProcessor();

		// Performs the image processing.
		// NOTE: An exception is *not* thrown if rect does not intersect with any part of the
		// loaded image. In this case, the function returns immediately and no pixels are
		// processed.
		void process();

		// To be implemented by all PixelProcessor child classes to define the processing to
		// occur for each pixel in the image area.  A return value of < 0 indicates processing
		// should stop, 0 indicates processing should continue and a value > 0 indicates that this 
		// number of pixels that can be skipped before processing any further pixels.
		virtual int processPixel(int x, int y) abstract;

		// The CSplitRegionIntoContentAreas that created this processor
		CSplitRegionIntoContentAreas &m_parent;

		// The portion of m_parent's currently loaded image that should be processed.
		CRect m_rect;

		// These are used to store results of processing (if needed)
		int m_nXPixelValue;
		int m_nYPixelValue;
		int m_nPixelCount;
	};

	// Used to find any content areas in the image via pixel content rather than OCR'd text.
	// (no value returned).
	DECLARE_PIXEL_PROCESSOR(PixelContentSearcher);

	// Used to count the number of (black) pixels. (m_nPixelCount).
	DECLARE_PIXEL_PROCESSOR(PixelCounter);

	// Used to calculate the average position of all pixels in the image area.
	// The average horizontal position = m_nXPixelValue / m_nPixelCount
	// The average vertical position = m_nYPixelValue / m_nPixelCount
	DECLARE_PIXEL_PROCESSOR(PixelAverager);

	// Used to find an image area boundary to edge of pixel content. The specified
	// area should be a line (zero width or height) region representing the area's edge.
	// CSplitRegionIntoContentAreas::m_sizeEdgeSearchDirection should be used to specify
	// the direction in which the search should be performed (eg: cx == 1 means to the right)
	// m_nPixelCount == 1 if a pixel content was found at the specified edge (requires two
	// consecutive pixels in the specified expansion direction to qualify)
	DECLARE_PIXEL_PROCESSOR(PixelEdgeFinder);

	// Used to erase all pixels in the specified area.
	// (no value returned).
	DECLARE_PIXEL_PROCESSOR(PixelEraser);

	// A class to represent content area candidates.
	class ContentAreaInfo : public CRect
	{
	public:
		// Creates an area based on a spatial string and the specified page infos. If the specified
		// spatial string doesn't contain spatial information, the area's rect will be set to
		// NULL (0, 0, 0, 0).
		ContentAreaInfo(const ISpatialStringPtr &ipString,
			const ILongToObjectMapPtr &pSpatialPageInfos);

		// Creates an area based on the specified bounds of the currently loaded document page.
		ContentAreaInfo(const CRect &rect);

		// Returns the total area in pixels;
		int getArea() { return (Width() * Height()); }

		// Specifies the original area bounds prior to any modification.
		CRect m_rectOriginal;

		// The avarage OCR confidence of the area's text (100 = absolute confidence, 0 = no
		// confidence)(Will be zero if created via an image area).
		long m_nOCRConfidence;

		// Specifies whether the top edge of content has been found or whether it is locked
		// at a position deemed to be a likely dividing point between lines.
		EBoundaryState m_eTopBoundaryState;

		// Specifies whether the bottom edge of content has been found or whether it is locked
		// at a position deemed to be a likely dividing point between lines.
		EBoundaryState m_eBottomBoundaryState;
	};

	/////////////////
	// Variables
	/////////////////

	bool m_bDirty;

	// The text to assign to any resulting sub-attribute that does not contain OCR data
	string m_strDefaultAttributeText;

	// The name to assign to any resulting sub-attribute.
	string m_strAttributeName;

	// The minimum width a returned area must be (in average char width)
	double m_dMinimumWidth;

	// The minimum height a returned area must be (in average char height)
	double m_dMinimumHeight;

	// Whether to include areas based on well-OCR'd text
	bool m_bIncludeGoodOCR;

	// Whether to include areas based on poor OCR'd text
	bool m_bIncludePoorOCR;

	// The attribute type to be assigned to areas based on well-OCR'd text
	string m_strGoodOCRType;

	// The attribute type to be assigned to areas based on poorly OCR'd text.
	string m_strPoorOCRType;

	// If using OCR confidence to exclude results, what is the cuttoff value for OCR
	// quality? (0 = worst OCR confidence, 100 = best)
	long m_nOCRThreshold;

	// If true, lines will be found ignored when creating content areas and will removed
	// if attempting to re-OCR for handwriting.
	bool m_bUseLines;
	
	// If true, content areas that are based off text that OCR'd poorly (or not at all)
	// will be re-OCR'd using handwriting recognition.  If the results from this OCR
	// attempt are improved, the original area value will be replaced.
	bool m_bReOCRWithHandwriting;

	// If true, create a grandchild attribute with the original spatial string as the result
	// (may not occupy the entire region returned by the primary result)
	bool m_bIncludeOCRAsTrueSpatialString;

	// The horizontal separation (measured in average character width for the page)
	// required to delineate separate content areas on the same line.
	long m_nRequiredHorizontalSeparation;

	// The current document being processed
	IAFDocumentPtr m_ipCurrentDoc;

	// The current document page being processed
	long m_nCurrentPage;

	// The OCR'd text of the current document page.
	ISpatialStringPtr m_ipCurrentPageText;

	// Average size of a character for the current attribute.
	CSize m_sizeAvgChar;

	// The bitmap for the current document page being processed.
	auto_ptr<LeadToolsBitmap> m_apPageBitmap;

	// Specifies the bounds of the currently loaded page.
	CRect m_rectCurrentPage;
	
	// The spatial string searcher is used to locate clues within boxes
	ISpatialStringSearcherPtr m_ipSpatialStringSearcher;

	// OCR Engine to attempt handwriting recognition on attribute results.
	IOCREnginePtr m_ipOCREngine;

	// A collection of lines found on the current document page
	vector<LineRect> m_vecHorizontalLines;
	vector<LineRect> m_vecVerticalLines;

	// A collection of the current content area candidates
	vector<ContentAreaInfo> m_vecContentAreas;

	// A collection of image areas that can be ignored in subsquent processing, either because the
	// area is already a candidate, or it is determined that this area does not contain any content
	// areas.
	vector<CRect> m_vecExcludedAreas;

	// The direction of any ongoing search for a content area's edge (cx == 1 means to the right)
	CSize m_sizeEdgeSearchDirection;

	/////////////////
	// Methods
	/////////////////

	// Primary processing function.  Called directly by ModifyValue and SplitValue.
	void addContentAreaAttributes(IAFDocumentPtr ipDoc, IAttributePtr ipAttribute);
	
	// Given the specified vector of SpatialStrings, the vector will be expanded to ensure
	// each line is reasonably well connected... ie, a large horizontal space in the middle
	// will result in the line being divided into two.
	void splitLineFragments(IIUnknownVectorPtr ipLines);

	// Uses raw image pixel data to find content areas within the specified ipRect.
	void processRegionPixels(const CRect& rect);

	// Given the current set of content area candidates already expanded horizontally,
	// attempts to expand each vertically to the edge of pixel content.  If area borders
	// overlap, it will merge or adjust the boundary between the areas as appropriate.
	void expandAndMergeAreas();

	// Finalizes the area boundary borders and removes any duplicate or invalid candidate areas.
	void finalizeContentAreas(const CRect& rectRegion);

	// Merge any areas whose shared area is similar or that share similar y coordinates (in other 
	// words, that appear to represent different fragments of the same line).
	void mergeAreas();

	// Given the specified starting point, shift the specified rrect so that it is centered
	// as much as possible on the pixels it contains.  rrect will never be shifted such that any
	// portion of rrect is outside of rectClip, or so that ptStart is no longer contained in rrect.
	// Since the function is recursive, nRecursions is used to control the number of possible 
	// recursions.
	// A CRect is returned indicating an image region that no longer needs to be processed
	// since the pixels it contains would yield the same ending position of rrect.
	// It is required that either rrect or pstart are not NULL (zero'd out).
	CRect centerAreaRegionOnBlack(CPoint ptStart, CRect &rrect, const CRect &rectClip, 
								  int nMaxRecursions = 3);

	// Attempts to expand the specified rect horizontally to the maximum extent of content (pixels).
	// NOTE: As part of this process, it will attempt to re-center the region vertically on the
	// pixels contained in the area.
	bool expandHorizontally(CRect &rrect, const CRect &rectClip);

	// Attempts to expand the specified rrect in the direction specified by sizeExpansionDirection.
	// The expansion will proceed pending appropriate pixel percentage content of 
	// rectExpansionRegion (which will be shifted in the direction rrect is expanded).
	// nExpansionCutoff specifies the maximum gap in pixels between content to be considered
	// part of the same area.  
	void expandArea(CRect &rrect, CRect rectExpansionRegion, const CSize &sizeExpansionDirection, 
		int nExpansionCuttoff, const CRect &rectClip);
	
	// Attempts to move each border of the specified rrect inward until the first pixel content
	// is detected.  orientation can be kHorizontal to shrink only horizontally (the left
	// and right borders), kVertical to shrink only vertically, or kBoth.
	void shrinkToFit(CRect &rrect, EOrientation orientation = kBoth);

	// Attempts to merge the specified area with any other overlapping area either up or
	// down from the given area (as specified by bUp). rvecAreasToAdd will be filled with 
	// new areas that should be added to the existing list of candidate areas after the 
	// call completes.  If bRecurse = true, if a boundary needs to be adjusted, all other 
	// qualifying areas will be merged against the new boundary.  bRecuse = false is currently
	// used to ensure againsst infinite recursion. Returns true if the area was merged, false
	// otherwise. 
	// NOTE: If false was returned but an overlapping area is found that did not qualify to be 
	// merged, the borders of both areas may have been adjusted to reflect the best guess at
	// the boundary between two lines based on OCR data.
	bool attemptMerge(ContentAreaInfo &area, bool bUp, vector<ContentAreaInfo> &rvecAreasToAdd,
		bool bRecurse);

	// Adjusts candidate areas in two ways:
	// 1) For areas that are flush against a found line (the edge of the line matches the edge
	// of the image area, the boundary of the area is changed to match the mid-point of the
	// line width-wise.  This is because the thickness of the line likely blocked out pixel
	// content which otherwise would have been included in the image area.
	// 2) Areas which span a line with sufficient area on either side will be dissected into two 
	// separate content areas.
	ContentAreaInfo makeFlushWithLines(ContentAreaInfo area);

	// Returns true if rrect overlaps some part of the current page area.  If it does not, false
	// is returned or an exception is thrown according to the bThrowException value specified.
	// If rrect does overlap the current page in some way, its borders are adjusted so that
	// it lies completely within the current page boundaries.
	bool ensureRectInPage(CRect &rrect, bool bThrowException = true);

	// Returns true if the specified area is big enough to be returned as a result.
	// Checks only the height of the area if bCheckHeightOnly is true
	bool isBigEnough(const ContentAreaInfo &area, bool bCheckHeightOnly = false);

	// Returns true if the percentages of black pixels in the specified rect meets the requirement
	// for a content area.
	bool hasEnoughPixels(const CRect &rect);

	// Returns true if the specified content area meets configured specifications to be included
	// in output.
	bool areaMeetsSpecifications(const ContentAreaInfo &area);

	// Returns true if the given point on an image has been excluded from further processing.
	// If true, rpoint will be changed to indicate the last pixel in the excluded area.
	bool isExcluded(CPoint &rpoint);
	
	// Returns true if the specified point is within the bounds of the current page.
	// Checks the point relative to image bitmap boundaries:
	// x range: 0 - (width-1)
	// y range: 0 - (height-1)													
	bool isPointOnPage(const CPoint& point);

	// Returns true if rect1 lies above rect2. (used to sort the results of the rule).
	static bool isAreaAbove(const CRect &rect1, const CRect &rect2);

	// Creates a new attribute based on the provided rect.  If the rect contains
	// any OCR data, a spatial string is returned consisting of the OCR result plus a spatial
	// space character spanning the full bounds rect (so the attribute as a whole matches)
	// RRect's bounds.  If no text is found in the rect, a spatial string is created using the
	// default attribute text such that the entire area of rect is filled.
	IAttributePtr createResult(IAFDocumentPtr ipDoc, long nPage, ContentAreaInfo area,
		ILongToObjectMapPtr ipSpatialInfos);

	// Loads a 1-bit bitmap image for the specified document page and uses it to initialize
	// m_rectCurrentPage, m_vecHorizontalLines and m_vecVerticalLines.  The found lines are also
	// erased from the loaded bitmap.  Returns true if successful, or false if the bitmap could
	// not be loaded.
	bool loadPageBitmap(IAFDocumentPtr ipDoc, long nPage);

	// Retrieves or creates/validates m_ipSpatialStringSearcher and initializes
	// it for the specified page
	ISpatialStringSearcherPtr getSpatialStringSearcher(IAFDocumentPtr ipDoc, long nPage);

	// Loads private OCREngine instance
	IOCREnginePtr getOCREngine();

	// Initializes m_ipCurrentDoc, m_nCurrentPage and m_ipCurrentPageText for the given page.
	ISpatialStringPtr setCurrentPage(IAFDocumentPtr ipDoc, long nPage);

	// Resets data members;
	void reset();

	// If m_bReOCRWithHandwriting is set and handwriting is not licensed, logs an exception
	// and sets m_bReOCRWithHandwriting to false;
	void validateHandwritingLicense();

	void validateLicense();
};

OBJECT_ENTRY_AUTO(__uuidof(SplitRegionIntoContentAreas), CSplitRegionIntoContentAreas)
