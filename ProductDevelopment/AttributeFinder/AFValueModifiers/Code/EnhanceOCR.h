// EnhanceOCR.h : Declaration of the EnhanceOCR

#pragma once
#include "resource.h"       // main symbols
#include "AFValueModifiers.h"
#include "..\..\AFCore\Code\AFCategories.h"
#include "..\..\..\..\ReusableComponents\COMComponents\UCLIDRasterAndOCRMgmt\Core\Code\SpatialStringLoader.h"

#include <FindLines.h>
#include <LeadToolsBitmap.h>
#include <IdentifiableObject.h>
#include <CPPLetter.h>
#include <ComUtils.h>
#include <CachedObjectFromFile.h>
#include <StringLoader.h>
#include <CommentedTextFileReader.h>
#include <MiscLeadUtils.h>

#include <string>
#include <memory>
using namespace std;

//--------------------------------------------------------------------------------------------------
// CEnhanceOCR
//--------------------------------------------------------------------------------------------------
class ATL_NO_VTABLE CEnhanceOCR :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CEnhanceOCR, &CLSID_EnhanceOCR>,
	public IPersistStream,
	public ISupportErrorInfo,
	public IDispatchImpl<IEnhanceOCR, &IID_IEnhanceOCR, &LIBID_UCLID_AFVALUEMODIFIERSLib>,
	public IDispatchImpl<IAttributeModifyingRule, &IID_IAttributeModifyingRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<IOutputHandler, &IID_IAttributeModifyingRule, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<IDocumentPreprocessor, &IID_IDocumentPreprocessor, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IMustBeConfiguredObject, &IID_IMustBeConfiguredObject, &LIBID_UCLID_COMUTILSLib>,
	public ISpecifyPropertyPagesImpl<CEnhanceOCR>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IIdentifiableObject, &IID_IIdentifiableObject, &LIBID_UCLID_COMUTILSLib>,
	private CIdentifiableObject
{
	public:
	CEnhanceOCR();
	~CEnhanceOCR();

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();
	void FinalRelease();

	DECLARE_REGISTRY_RESOURCEID(IDR_ENHANCEOCR)

	BEGIN_COM_MAP(CEnhanceOCR)
		COM_INTERFACE_ENTRY(IEnhanceOCR)
		COM_INTERFACE_ENTRY2(IDispatch, IEnhanceOCR)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
		COM_INTERFACE_ENTRY(IPersistStream)
		COM_INTERFACE_ENTRY(IAttributeModifyingRule)
		COM_INTERFACE_ENTRY(IOutputHandler)
		COM_INTERFACE_ENTRY(IDocumentPreprocessor)
		COM_INTERFACE_ENTRY(ICategorizedComponent)
		COM_INTERFACE_ENTRY(IMustBeConfiguredObject)
		COM_INTERFACE_ENTRY_IMPL(ISpecifyPropertyPages)
		COM_INTERFACE_ENTRY(ICopyableObject)
		COM_INTERFACE_ENTRY(ILicensedComponent)
		COM_INTERFACE_ENTRY(IIdentifiableObject)
	END_COM_MAP()

	BEGIN_PROP_MAP(CEnhanceOCR)
		PROP_PAGE(CLSID_EnhanceOCRPP)
	END_PROP_MAP()

	BEGIN_CATEGORY_MAP(CEnhanceOCR)
		IMPLEMENTED_CATEGORY(CATID_AFAPI_VALUE_MODIFIERS)
		IMPLEMENTED_CATEGORY(CATID_AFAPI_OUTPUT_HANDLERS)
		IMPLEMENTED_CATEGORY(CATID_AFAPI_DOCUMENT_PREPROCESSORS)
	END_CATEGORY_MAP()

// IEnhanceOCR
	STDMETHOD(get_ConfidenceCriteria)(long *pVal);
	STDMETHOD(put_ConfidenceCriteria)(long newVal);
	STDMETHOD(get_FilterPackage)(EFilterPackage *pVal);
	STDMETHOD(put_FilterPackage)(EFilterPackage newVal);
	STDMETHOD(get_CustomFilterPackage)(BSTR *pVal);
	STDMETHOD(put_CustomFilterPackage)(BSTR newVal);
	STDMETHOD(get_PreferredFormatRegexFile)(BSTR *pVal);
	STDMETHOD(put_PreferredFormatRegexFile)(BSTR newVal);
	STDMETHOD(get_CharsToIgnore)(BSTR *pVal);
	STDMETHOD(put_CharsToIgnore)(BSTR newVal);
	STDMETHOD(get_OutputFilteredImages)(VARIANT_BOOL *pVal);
	STDMETHOD(put_OutputFilteredImages)(VARIANT_BOOL newVal);
	STDMETHOD(EnhanceDocument)(
		IAFDocument* pDocument, ITagUtility* pTagUtility, IProgressStatus *pProgressStatus);

// IAttributeModifyingRule
	STDMETHOD(raw_ModifyValue)(IAttribute *pAttribute, IAFDocument *pOriginInput,
		IProgressStatus* pProgressStatus);

// IOutputHandler
	STDMETHOD(raw_ProcessOutput)(IIUnknownVector * pAttributes, IAFDocument * pDoc,
		IProgressStatus* pProgressStatus);

// IDocumentPreprocessor
	STDMETHOD(raw_Process)(/*[in]*/ IAFDocument* pDocument,/*[in]*/ IProgressStatus *pProgressStatus);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR *pstrComponentDescription);

// IMustBeConfiguredObject
	STDMETHOD(raw_IsConfigured)(VARIANT_BOOL *pbValue);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL *pbValue);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown* *pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown *pObject);

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IIdentifiableObject
	STDMETHOD(get_InstanceGUID)(GUID *pVal);

/////////////////
// Classes
/////////////////

	// Represents a portion of OCR text. Each instance can be assumed to consist of text all on the
	// same line.
	class OCRResult
	{
	public:
		OCRResult();
		OCRResult(ISpatialStringPtr ipSpatialString, bool bIsOriginal, string strCharsToIgnore = "");

		// Tests whether the result contains any spatial text.
		bool IsEmpty() const;

		// Calculates the horizontal area the result occupies (widths of all spatial characters
		// combined) as a measure of how much content was able to be OCR'd.
		// dFactor can be used to compensate the span based on a factor of difference that certain
		// filters typically change OCR char width by. The width of each character
		// is capped at nMaxCharWidth to compensate for cases where OCR returns what are several
		// letters as one separate-- no minimum cap to de-emphacize cases where a poorly OCR'd
		// returns a bunch of very tiny characters. 
		unsigned long Span(double dFactor, unsigned long nMaxCharWidth) const;
		
		// Returns > 0 if the other results is below this result or this result is empty.
		// Returns 0 if the other result spatially overlaps with this result.
		// Returns < 1 if the other result is below or vertically in line with this result or empty.
		long CompareVerticalArea(const OCRResult &other) const;
		
		// Returns the index of any letter nXPos intersects with.
		long AdvanceToPos(long nXPos) const;

		// Returns the index of the next letter after nXPos.
		long AdvanceToNextPos(long nXPos) const;

		// Returns the confidence of the letter at the specified index.
		const CPPLetter* GetLetter(long nIndex) const;

		// Returns the X-axis mid-point of the letter at the specified index.
		long GetPos(long nIndex) const;

		// Returns the confidence of the letter at the specified index.
		long GetConfidence(long nIndex) const;

		// Returns a pointer to the original OCRResult of the letter at the specified index or
		// __nullptr if this is an original OCR result.
		OCRResult* GetSourceResult(long nIndex) const;

		// Indicates if all characters in the result meet the configured confidence criteria.
		bool AllCharsMeetConfidenceCriteria(long nConfidence);

		// Adds a new letter to this result. pSourceResult should specify the result the letter
		// originally came from.
		void AddLetter(CPPLetter newLetter, OCRResult *pSourceResult);

		// Removes extra whitespace characters from the result that may have resulted from merging
		// results.
		void TrimExtraWhiteSpace();

		// Returns the string value of the result.
		string ToString() const;

		// Returns the SpatialString value of the result.
		ISpatialStringPtr ToSpatialString(string strSourceDocName, long nPage,
			ILongToObjectMapPtr ipPageInfoMap) const;

		//////////////
		// Variables
		//////////////

		// The result's letters.
		vector<CPPLetter> m_vecLetters;

		// Maps each letter to the result it is originally from.
		vector<OCRResult*> m_vecSourceResults;

		// The overall confidence of the result.
		long m_nOverallConfidence;
		
		// The ratio of word chars to all spatial chars in the result. (a lesser ratio is often
		// indicative of poor OCR quality.
		double m_dWordCharRatio;

		// The average width of word characters in the result.
		long m_nAverageCharWidth;

		// The number of spatial chars in the result.
		long m_nSpatialCharCount;

		// Indicates if this results represents the original document text (as opposed to the text
		// from OCR'ing a filtered image).
		bool m_bIsOriginal;

private:
		// Populates the result letters. vecRemainingLetters consists of letters not on the same
		// line as this result that should be added to subsequent results.
		bool populateLetters(CPPLetter *pLetters, long nNumLetters,
			vector<CPPLetter> &vecRemainingLetters, string strCharsToIgnore);

		// Updates this results member variables based upon the letters it currently contains.
		void updateStats();

		// Gets the index of the character at the specified nXPos, or -1 if no character is at that
		// position.
		long GetIndexOfPos(long nXPos) const;

		// A positive number indicates the position in this result in which the specified character
		// should be added.
		// bIsLastChar indicates if this character is known to fall sequentially after all other
		// characters currently in the result; setting this to true will reduce the chances that
		// GetInsertionPosition will incorrectly decide the character belongs on a new line.
		// -1 indicates the letter is on a separate line and belongs in a separate result.
		// -2 indicates the letter appears to overlap with a letter already in the result and likely
		// indicates incorrectly OCR'd text.
		long GetInsertionPosition(const CPPLetter *pLetter, bool bProbablySameLine,
			bool bIsLastChar) const;
	};

	// Represents all OCR text in a given image area. May contain more than one OCRResult instance.
	struct ZoneData
	{
		// The area of the current page represented by this instance.
		ILongRectanglePtr m_ipRect;

		// The image area to be processed for this zone. If different from m_ipRect, only the
		// resulting text within m_ipRect will be used.
		ILongRectanglePtr m_ipProcessingRect;

		// The area of the current page to process and OCR (may include surrounding text for OCR
		// context)
		ILongRectanglePtr m_ipImageProcessingRect;

		// The original OCR text from this zone.
		ISpatialStringPtr m_ipOriginalText;

		// The current OCR text associated with this zone (will reflect enhance operations).
		vector<OCRResult*> m_vecResults;

		// The the overall text value from this zone.
		string m_strResult;

		// The original OCR confidence of this zone.
		long m_nOriginalConfidence;

		// The current the overall OCR confidence of this zone (m_vecResults)
		long m_nConfidence;

		// The original overall horizontal span of this zone (m_vecResults)
		unsigned long m_nOriginalSpan;

		// The current overall horizontal span of this zone (m_vecResults)
		unsigned long m_nSpan;

		// Represents a result that contains a preferred format that should be returned as the
		// result of this area unless m_nConfidence is much higher than the confidence of
		// m_pPreferredResult.
		OCRResult* m_pPreferredResult;

		// Indicates whether an OCR attempt against a filter image has been able to produce a result
		// with significantly higher confidence than the exiting text.
		bool m_bMeetsConfidenceCriteria;

		ZoneData()
			: m_ipRect(__nullptr)
			, m_ipOriginalText(__nullptr)
			, m_nConfidence(0)
			, m_nOriginalConfidence(0)
			, m_nSpan(0)
			, m_nOriginalSpan(0)
			, m_pPreferredResult(__nullptr)
			, m_bMeetsConfidenceCriteria(false)
		{}
	};

	/////////////////
	// Variables
	/////////////////

	// The OCR confidence percentage below which OCR will attempt to be enhanced.
	long m_nConfidenceCriteria;

	// The the set of image filters that will be used against the document.
	UCLID_AFVALUEMODIFIERSLib::EFilterPackage m_eFilterPackage;

	// A custom defined filter package to use.
	string m_strCustomFilterPackage;

	// A regex file that specifies formats that are preferred and should be used over other formats.
	// The final result will contain this pattern unless the confidence of the possibility(ies)
	// containing this pattern are very substantially outweighed by a possibility that does not
	// contain this pattern.
	string m_strPreferredFormatRegexFile;

	// Any chars in this string will be prevented from being created by the EnhanceOCR process.
	string m_strCharsToIgnore;

	// Indicates whether copies of the filtered images should be output (for debug purposes).
	bool m_bOutputFilteredImages;

	// The set of filters to use to for processing.
	string* m_pFilters;

	// The number of filters being used for processing.
	long m_nFilterCount;

	// A custom defined sequence of filters.
	vector<string> m_vecCustomFilterPackage;

	// Custom defined filter matrices.
	map<string, vector<L_INT>> m_mapCustomFilters;

	// Cached readers used to load custom filters from disk.
	map<string, CachedObjectFromFile<std::string, StringLoader>> m_cachedFileLoaders;

	// Used by m_cachedFileLoaders.
	map<string, vector<string>> m_cachedFileLines;

	// This object may used to read m_strPreferredFormatRegexFile.
	CachedObjectFromFile<std::string, StringLoader> m_cachedRegexLoader;

	// Used to load the original document text.
	CachedObjectFromFile<ISpatialStringPtr, SpatialStringLoader> m_cachedDocText;

	// Progress status
	IProgressStatusPtr m_ipProgressStatus;

	// Used to expand m_strPreferredFormatRegexFile.
	ITagUtilityPtr m_ipTagUtility;

	// The name of the document currently being processed.
	string m_strSourceDocName;

	// The current document being processed
	IAFDocumentPtr m_ipCurrentDoc;

	// Indicates whether the all the area of every page should be processed regardless of the input
	// AFDocument.
	bool m_bProcessFullDoc;

	// The SpatialPageInfo for the document.
	ILongToObjectMapPtr m_ipPageInfoMap;

	// The current document page being processed
	long m_nCurrentPage;

	// The bitmap for the current document page being processed.
	unique_ptr<LeadToolsBitmap> m_apPageBitmap;

	// The temporary file for the current filtered image.
	unique_ptr<TemporaryFileName> m_apFilteredBitmapFileName;

	// The OCR'd text of the current document page.
	ISpatialStringPtr m_ipCurrentPageText;

	// The SpatialPageInfo of the current document page.
	ISpatialPageInfoPtr m_ipCurrentPageInfo;

	// A rectangle encapsulating the area of the current page to process.
	ILongRectanglePtr m_ipPageRect;

	// A raster zone encapsulating the area of the current page to process.
	IRasterZonePtr m_ipPageRasterZone;

	// The average character width for the current page.
	long m_nAvgPageCharWidth;

	// The spatial string searcher used to locate original document text in specified areas.
	ISpatialStringSearcherPtr m_ipSpatialStringSearcher;

	// Rectangles describing areas containing high-confidence text on the current page.
	vector<ILongRectanglePtr> m_vecHighConfRects;

	// A holding area for the OCRResults generated during processing since the m_vecSourceResults
	// may refer back to previously generated results.
	vector<unique_ptr<OCRResult>> m_vecResults;

	// The split region into content areas object used to locate image zones to enhance.
	UCLID_AFVALUEMODIFIERSLib::ISplitRegionIntoContentAreasPtr m_ipSRICA;

	// The Regex parser used to evaluate the regex from m_strPreferredFormatRegexFile.
	IRegularExprParserPtr m_ipPreferredFormatParser;

	// The OCR engine used to recognize text from filtered images.
	IOCREnginePtr m_ipOCREngine;

	// Used get the regex parser.
	IMiscUtilsPtr m_ipMiscUtils;

	BrushCollection m_brushes;
	PenCollection m_pens;

	bool m_bDirty;

	/////////////////
	// Methods
	/////////////////

	// Enhances the OCR text on the specified document.
	void enhanceOCR(IAFDocumentPtr ipAFDoc);

	// Returns the enhanced OCR text on the specified document page.
	ISpatialStringPtr enhanceOCR(IAFDocumentPtr ipAFDoc, long nPage);

	// Enhances OCR text in the specified attributes' spatial area.
	void enhanceOCR(IIUnknownVector *pAttributes, IAFDocument *pDoc);

	// Enhances the OCR text in the specified zones.
	void enhanceOCR(vector<ZoneData> &zones);

	// Initializes the filters per the object settings.
	void initializeFilters();

	// Initializes any specified custom filters.
	void initializeCustomFilters();

	void initializeCustomFilter(string strFilterName, string strFilterFilename);

	// Initializes variables for the specified page.
	ISpatialStringSearcherPtr setCurrentPage(IAFDocumentPtr ipDoc, long nPage);

	// Gets a rectangle representing the area of the current page to process.
	ILongRectanglePtr getPageRect();

	// Gets a raster zone representing the area of the current page to process.
	IRasterZonePtr getPageRasterZone();

	// Removes all words from the specified ipPageText that do not contain any characters below
	// m_nConfidenceCriteria and returns the zones where this high confidence text was found.
	vector<ILongRectanglePtr> removeHighConfidenceText(ISpatialStringPtr ipPageText);
	
	// Prepares the specified image areas for enhancement.
	void prepareImagePage(vector<ILongRectanglePtr> &vecRectsToEnhance);

	// Locates zones to enhance that include the specified low confidence text while excluding any
	// specified areas to ignore.
	vector<ILongRectanglePtr> prepareImagePage(ISpatialStringPtr ipLowConfidenceText, 
											   vector<ILongRectanglePtr> vecZonesToIgnore);

	// Removes any black borders from the bitmap.
	void removeBlackBorders(pBITMAPHANDLE phBitmap);

	// Erases the specified rects from the specified bitmap
	void eraseImageZones(LeadToolsBitmap &ltBitmap, vector<ILongRectanglePtr> vecZonesToErase);

	// Applies the specified filter (or filter combination) to the specified rectangles on the
	// current image page to produce and image page to be re-OCRed.
	void generateFilteredImage(string strFilter, set<ILongRectanglePtr> *psetRectsToFilter);

	// Applies the specified filter(s) to the image. May be either a single filter or 2 filters
	// separated by "+". Each filter can have a single parameter delimited by a dash.
	// Example: "gaussian-1+medium-15".
	void applyFilters(pBITMAPHANDLE phBitmap, string strFilters, ILongRectanglePtr ipRect);

	// Applies the specified filter of the specified dimensions where the dimension is the number of
	// pixels wide and the filter is assumed to be square. The resulting pixel is black if the sum
	// of the weighted pixel values vs the available color levels is greater than dThreshold.
	void applyFilter(pBITMAPHANDLE phBitmap, L_INT nDim, const L_INT pFilter[], double dThreshold);

	// Locates the image regions to enhance in the current page image using the provided low quality
	// text as hints for areas that need enhancement.
	vector<ILongRectanglePtr> getRectsToEnhance(ISpatialStringPtr ipLowQualityText);

	// Inflates the specified by nWidth on the left and right sides and nHeight on the top and
	// bottom. (nWidth of 1 makes the rect 2 pixels wider as per CRect::InflateRect)
	ILongRectanglePtr inflateRect(ILongRectanglePtr ipRect, long nWidth, long nHeight);

	// Re-loads the image into m_apPageBitmap, but with only the specified rects. (the rest of the
	// image will be blank).
	void loadImagePageWithSpecifiedRectsOnly(vector<ILongRectanglePtr> &vecRectsToEnhance);

	// Create ZoneData instance for each specified rect in the current image page. ipSearcher is
	// used to retrieve the original text for each zone.
	// sizeExpandProcessingRect can be used to expand the area of pixels processed to include extra
	// the area around the zone which can improve OCR results. (though only the text within the
	// original rects will be used)
	vector<ZoneData> createZonesFromRects(vector<ILongRectanglePtr> vecRects,
		ISpatialStringSearcherPtr ipSearcher, CSize sizeExpandProcessingRect = CSize());

	// OCRs the current filtered image and returns a spatial string searcher that can be used to
	// retrieve the text found in a particular image area.
	ISpatialStringSearcherPtr OCRFilteredImage();

	// OCR's the specified rect on the current page.
	ISpatialStringPtr OCRRegion(ILongRectanglePtr ipRect);

	// Applies the results of an OCR attempt to the specified zone. Returns true the zone now meets
	// m_nConfidenceCriteria for all letters; false otherwise.
	bool applyOCRAttempt(ZoneData& zoneData, ZoneData& OCRAttemptData);

	// Populates the specified OCRResult vector using the specified input text. Each separate line
	// in ipText will become a separate OCRResult instance.
	void populateResults(vector<OCRResult*>& vecResults, ISpatialStringPtr ipText,
						 bool bIsOriginal);

	// Updates stats for the specified image zones where the specified nConfBias is added to the
	// zone's character confidence and the zone's total span is multiplied by dSpanBias.
	void updateZoneData(ZoneData& zoneData, bool bIsOriginal, double dSpanBias);

	// Sets or updates the zone's preferred result based upon whether the current result matches the
	// preferred format.
	void updateZonePreferredResult(ZoneData& zoneData);

	// Combines firstResult with secondResult on a character by character basis to produce a result
	// containing the highest confidence result at each spatial position.
	// Non-word chars from secondResult that do not already exist in firstResult will not be added to
	// the output unless bAllowNewNonWordChars is true.
	OCRResult* mergeResults(const OCRResult& exisitingResult, const OCRResult& newResult,
							bool bAllowNewNonWordChars);

	// Applies the specified ZoneData to the original text by replacing or inserted the enhanced
	// text from the zone.
	void applyZoneResults(ISpatialStringPtr ipOriginalText, const ZoneData& zoneData);

	// Gets the OCR engine to use recognize text from the filtered images.
	IOCREnginePtr getOCREngine();

	// Gets a spatial string searcher for the current page's original text.
	ISpatialStringSearcherPtr getSpatialStringSearcher();

	// Gets a split region into content areas instance that is used to locate image zones that need
	// enhancement.
	UCLID_AFVALUEMODIFIERSLib::ISplitRegionIntoContentAreasPtr getSRICA();

	// Gets a regex parser to determine if a value is in a preferred format.
	IRegularExprParserPtr getPreferredFormatRegex();

	// Expands path tags and functions using either the m_ipTagUtility provided in a task execution
	// context or AFTagManager in a rule object execution context.
	string expandPathTagsAndFunctions(string strFileName);
	
	// Gets a CommentedTextFileReader for the specified filename. All decryption/auto-encryption
	// and caching is handled automatically.
	CommentedTextFileReader getFileReader(const string& strFilename);

	// Resets all variables to their default values.
	void reset();

	void validateLicense();

	// Checks to see if Enhance OCR is licensed. If not, Enhance OCR rule objects will be allowed to
	// "run", but will not enhance the OCR; it will return the original text from the uss file.
	// An application trace to this effect will be logged once per process.
	bool isEnhanceOCRLicensed();
};

OBJECT_ENTRY_AUTO(__uuidof(EnhanceOCR), CEnhanceOCR)