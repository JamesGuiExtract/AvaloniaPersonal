// ScansoftOCR2.h : Declaration of the CScansoftOCR2

#pragma once

#include "resource.h"       // main symbols

#include <KernelAPI.h>	
#include <Win32Event.h>
#include <CPPLetter.h>

#include <string>
#include <vector>
#include <memory>
#include <list>
#include <afxmt.h>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CScansoftOCR2
class ATL_NO_VTABLE CScansoftOCR2 : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CScansoftOCR2, &CLSID_ScansoftOCR2>,
	public ISupportErrorInfo,
	public IDispatchImpl<IScansoftOCR2, &IID_IScansoftOCR2, &LIBID_UCLID_SSOCR2Lib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IPrivateLicensedComponent, &IID_IPrivateLicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CScansoftOCR2();
	~CScansoftOCR2();

	friend class ScansoftOCRCfg;

DECLARE_REGISTRY_RESOURCEID(IDR_SCANSOFTOCR2)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CScansoftOCR2)
	COM_INTERFACE_ENTRY(IScansoftOCR2)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IScansoftOCR2)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(IPrivateLicensedComponent)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IScansoftOCR2
	STDMETHOD(RecognizeText)(/*[in]*/ BSTR bstrImageFileName, /*[in]*/ IVariantVector* pPageNumbers, 
		/*[in]*/ ILongRectangle* pZone, /*[in]*/ long lRotationInDegrees, /*[in]*/ EFilterCharacters eFilter,	
		/*[in]*/ BSTR bstrCustomFilterCharacters, /*[in]*/ EOcrTradeOff eTradeOff, 
		/*[in]*/ VARIANT_BOOL vbDetectHandwriting, /*[in]*/ VARIANT_BOOL vbReturnUnrecognized, 
		/*[in]*/ VARIANT_BOOL vbReturnSpatialInfo, /*[in]*/ VARIANT_BOOL vbUpdateProgressStatus, 
		/*[in]*/ EPageDecompositionMethod eDecompMethod, /*[out, retval]*/ BSTR* pStream);
	STDMETHOD(GetPID)(long* pPID);
	STDMETHOD(SupportsTrainingFiles)(VARIANT_BOOL *pbValue);
	STDMETHOD(LoadTrainingFile)(BSTR strTrainingFileName);
	STDMETHOD(WillPerformThirdRecognitionPass)(VARIANT_BOOL *vbWillPerformThirdRecognitionPass);
	STDMETHOD(GetProgress)(long* plProcessID, long* plPercentComplete, 
		long* plPageIndex, long* plPageNumber);
	STDMETHOD(GetPrimaryDecompositionMethod)(EPageDecompositionMethod *ePrimaryDecompositionMethod);

// IPrivateLicensedComponent
	STDMETHOD(raw_InitPrivateLicense)(/*[in]*/ BSTR strPrivateLicenseKey);
	STDMETHOD(raw_IsPrivateLicensed)(/*[out, retval]*/ VARIANT_BOOL *pbIsLicensed);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:

	//////////////
	// Variable types
	//////////////

	// Releases memory allocated by RecAPI calls. Create this object after the RecAPI call has 
	// allocated space for the object. MemoryType is the data type of the object to release when 
	// RecMemoryReleaser goes out of scope.
	// NOTE: To release memory in m_listZoneLayouts after calls to kRecGetOCRZoneLayout,
	// use CScansoftOCR2 as the MemoryType and pass CScansoftOCR2's this pointer to the constructor.
	// NOTE: Pass a Win32Event to signal that event when the RecMemoryReleaser goes out of scope.
	template<typename MemoryType>
	class RecMemoryReleaser
	{
	public:
		RecMemoryReleaser(MemoryType* pMemoryType);
		~RecMemoryReleaser();

	private:
		MemoryType* m_pMemoryType;
	};

	// Stores data associated with the layout of a zone
	struct ZoneLayoutType
	{
		LPRECT prectSubZones;
		int iNumSubZones;
	};

	enum EDisplayCharsType
	{
		kDisplayCharsTypeNone,
		kDisplayCharsTypeOnChange,
		kDisplayCharsTypeAlways
	};

	//////////////
	// Variables
	//////////////
	bool m_bPrivateLicenseInitialized;

	// true if three ocr passes should be run
	// set from the registry (not passed in)
	bool m_bRunThirdRecPass;

	// after locating zones, whether the zone-ordering algorithm should run (true)
	// or whether to use the unsorted results of the zone location (false).
	// set from the registry.
	bool m_bOrderZones;

	// the accuracy/speed tradeoff setting for recognition.
	// set from the registry.
	RMTRADEOFF m_eTradeoff;
	
	// true if failed pages should be skipped; false if failed pages should fail the document.
	bool m_bSkipPageOnFailure;

	// The maximum percentage of pages that can fail without failing the document.
	unsigned long m_uiMaxOcrPageFailurePercentage;

	// The maximum number of pages that can fail without failing the document.
	unsigned long m_uiMaxOcrPageFailureNumber;

	// The number of decomposition methods to try. Must be 2 or 3.
	unsigned long m_uiDecompositionMethods;

	// The page numbers of failed pages.
	vector<int> m_vecFailedPages;

	// The sequence of decomposition methods to try.
	IMG_DECOMP m_decompositionMethods[3];

	// type of recognized characters to return
    EFilterCharacters m_eFilter;

	// whether the filter contains a numeral or alphabetic character.
	// used when recognizing handwriting to determine what printed text to remove (if any).
	bool m_bFilterContainsAlpha;
	bool m_bFilterContainsNumeral;
	
	bool m_bHardKill;

	// After OCR is finished this will contain the resultant spatial string
	ISpatialStringPtr m_ipSpatialString;

	// handles for the image file and image page
	HIMGFILE m_hImageFile;
	HPAGE m_hPage;

	EDisplayCharsType m_eDisplayFilterCharsType;	

	// For retrieving configuration settings
	auto_ptr<ScansoftOCRCfg> m_apCfg;

	static string ms_strLastDisplayedFilterChars;

	static CScansoftOCR2* ms_pInstance;

	// progress status update variables
	static long ms_lProcessID;
	static long ms_lPercentComplete;
	static long ms_lPageIndex;       
	static long ms_lPageNumber;      

	// mutex to protect progress status update variables
	static CMutex ms_mutexProgressStatus;

	// whether to record progress status updates
	static bool ms_bUpdateProgressStatus;

	// retains the currently OCRing page number and page index
	static long ms_lCurrentPageNumber;
	static unsigned int ms_uiCurrentPageIndex;

	// The version of the RecAPI engine
	string m_strVersion;

	// these are for controlling the timeout thread
	// when this (auto reset)event is signaled
	// the timeout thread will stop running
	Win32Event m_eventKillTimeoutThread;
	// Sigaling this auto reset event lets the timeout thread
	// know that some progress has been made an it should reset 
	// its timeout
	Win32Event m_eventProgressMade;

	// used by the timeout thread
	// this is the amount of time that RecApi will be allowed to run without progress being
	// reported before the timeout thread kills the process
	long m_nTimeoutLength;

	// lists for zones and corresponding zone layouts, used for zone ordering
	list<ZONE> m_listZones;
	list<ZoneLayoutType> m_listZoneLayouts;

	/////////////
	// Methods
	/////////////
	void validateLicense();


	//---------------------------------------------------------------------------------------------
	// PURPOSE: Sets the decomposition method to use based on the specified index.
	void setDecompositionMethodIndex(unsigned int index);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Sets the sequence of decomposition methods to use based on the specified initial
	//          decomposition method.
	// PARAMS:  eDecompMethod - The initial decomposition method in the decomposition sequence
	void setDecompositionSequence(EPageDecompositionMethod eDecompMethod);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Sets the speed-accuracy tradeoff
	// PARAMS: eTradeOff -
	//           (a) kAccurate - Most accurate, but slowest
	//           (b) kBalanced - Compromise between accuracy and speed
	//           (c) kFast - Fastest, but least accurate
	//           (d) kRegistry - Uses the value specified in the registry
	void CScansoftOCR2::setTradeOff(EOcrTradeOff eTradeOff);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: The text in the specified area of the specified page of the specified image will 
	//          be recognized by the OCR engine and returned in either rstrText or pvecLetters.
	//          ipPageInfos will contain the spatial information about the specified page.
	// PARAMS: (1) strImageFileName is a valid image file name to rotate and recognize.
	//         (2) nPageNum is a 1-based image page number on which to recognize text.
	//         (3) pZone is a rectangular area of the unrotated and undeskewed image in which to 
	//             recognize text, or NULL if the whole page should be recognized.
	//		   (4) nRotationInDegrees is one of {90, 180, 270, 360} if an explicit rotation should
	//             be applied, or 0 if the rotation should automatically detected.
	//         (5) bDetectHandwriting is true if handwritten text should be recognized, false if
	//             machine printed text should be recognized.
	//         (6) bReturnUnrecognized is true if unrecognized characters should be returned in
	//             the final output, flase if unrecognized characters should be dropped.
	//         (7) rstrText will contain the recognized text if pvecLetters == NULL, will not be
	//             modified is pvecLetters != NULL.
	//         (8) pvecLetters will contain the recognized text if pvecLetters != NULL, will not be
	//             modified if pvecLetters == NULL.
	//         (9) ipPageInfo will contain the spatial page info for the specified page.
	void rotateAndRecognizeTextInImagePage(const string& strImageFileName, long nPageNum, 
		LPRECT pZone, long nRotationInDegrees, bool bDetectHandwriting, bool bReturnUnrecognized,
		string& rstrText, vector<CPPLetter>* pvecLetters, ILongToObjectMapPtr ipPageInfos);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Detects and removes lines from the image in memory within the area defined by pArea.
	//          Creates filter-less, printed-text recognition zones from the regions in between 
	//          the detected horizontal lines such that the whole of pArea is encompassed.
	// REQUIRE: pArea must be a valid region of image prior to rotation and deskew.
	void createZonesFromLineRemoval(const RECT* pArea);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Recognizes printed text on the image page and removes high-confidence machine 
	//          printed letters that might interfere with handwriting recognition. Creates 
	//          handwriting recognition zones contained in pArea based on the spatial information
	//          of the detected glyphs (characters).
	// REQUIRE: All desired pre-recognition steps (eg. inserting user zones) must be performed
	//          prior to calling this method. It is expected that the RecAPI engine will contain
	//          filter-less, printed-text recognition zones that are contained in pArea prior
	//          to calling this method.
	// PARAMS:  (1) pArea is the area in which to recognize printed text.
	//          (2) imgInfo contains information about the current image page as obtained from a
	//              call to kRecGetImgInfo. This information is used to determine a reasonable
	//              minimum zone height and to properly remove letters on different image types.
	void prepareHandwritingZones(const RECT* pArea, const IMG_INFO& imgInfo);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Clears the zones from the RecAPI engine and inserts all the zones in m_listZones
	//          into the RecAPI engine as user zones.
	void insertZones();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Clears pLetter from the image contained in memory.
	// PARAMS:  (1) pLetter is the letter to be removed, as obtained from calling kRecGetLetters
	//              after recognizing text.
	//          (2) bIsBW is true if the image in memory is black-and-white, or false if it is not
	//              black-and-white. This is needed because black-and-white images are stored
	//              differently in memory than other image types.
	void removeLetter(const LETTER* pLetter, bool bIsBW);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Ensures the recently located zones of the current page are in top-down order.
	// REQUIRE: m_hPage != NULL
	// PROMISE: Checks the OCR zones of the current page are in top-down order. If they are
	//          not in order, inserts them in order as user zones. If no zones have been OCRed,
	//          this function does nothing. Top-down order means:
	//          (1) Vertically higher zones come before vertically lower zones.
	//          (2) When zones are aligned vertically, the leftmost zone comes first.
	void orderZones();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To fill pvecLetters with the most recently recognized text on nPageNum
	//          with lVerticalDPI vertical dots per inch. Unrecognized characters can be optionally
	//          returned as well.
	// REQUIRE: All parameters must be non-NULL. RecAPI must have just finished a 
	//          successful call to kRecRecognize on nPageNum of a document. lVerticalDPI
	//          must be the vertical dots per inch of the recently processed page.
	// PROMISE: pvecLetters will contain the CPPLetter objects for each recognized letter.
	//          Unrecognized characters will be discarded. \r\n will be inserted at the end of
	//          lines and \r\n\r\n will be inserted at the end of paragraphs. Each letter object's
	//			page number will be set to nPageNum. lVerticalDPI will be used to determine each 
	//          letter object's font size. bReturnUnrecognized will return all detected characters
	//          if true, or only recognized characters if false.
	void addRecognizedLettersToVector(vector<CPPLetter>* pvecLetters, long nPageNum, 
		long lVerticalDPI, bool bReturnUnrecognized);
	//---------------------------------------------------------------------------------------------
	// PROMISE: To return true if either the string or letter have some contents.
	//			If ipLetters is NULL, then strText's contents are checked.  Otherwise,
	//			ipLetters's contents are checked.
	bool hasContent(const string& strText, const vector<CPPLetter>* pvecLetters);
	//---------------------------------------------------------------------------------------------
	// PROMISE: To append ipLettersToAppend to ripLetters (if ripLetters != NULL) or
	//			to append strTextToAppend to rstrText (if ripLetters == NULL)
	void appendContent(string& rstrText, vector<CPPLetter>* pvecLetters,
		const string& strTextToAppend, 
		const vector<CPPLetter>* pvecLettersToAppend);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Sets the end of zone and end of paragraph information of the last character in 
	//          pvecPageLetters to that of pletterScanSoft. 
	void setLastPageLetterBoundary(vector<CPPLetter>* pvecPageLetters, LETTER* pletterScanSoft);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Stores the most recently recognized text into rstrText. If bReturnUnrecognized is
	//          true all found text will be returned, otherwise only recognized text will be returned.
	// REQUIRE: rstrText != NULL. kRecRecognize must just have been successfully called.
	// PROMISE: rstrText will contain the most recently recognized text from OCR engine.
	void getRecognizedText(string& rstrText, bool bReturnUnrecognized);
	//---------------------------------------------------------------------------------------------
	// PROMISE: To append ulCount instances of \r\n as non-spatial characters to pvecLetters.  
	//			Each of the appended characters will have -1 as their PageNumber field.
	// NOTE:    If a space character preceeds these characters, it will be removed. [P13 #4750]
	void appendSlashRSlashN(vector<CPPLetter>* pvecLetters, unsigned long ulCount);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To log information to a debug file
	// REQUIRE: ipLetters points to a valid IIUnknownVector of ILetter objects
	// PROMISE: To write information about strText and ipLetters to a file called
	//			SSOCR.log in the directory where this DLL exists.
	void logDebugInfo(const string& strText, vector<CPPLetter>* pvecLetters);
	//---------------------------------------------------------------------------------------------
	// internal function called by logDebugInfo()
	string getDebugRepresentation(char cChar);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: to recieve progress updates from RecAPI
	static RECERR __stdcall ProgressMon(LPPROGRESSMONITOR mon, void* pContext);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Updates imgRotate if nRotationInDegrees != 0 and updates ipPageInfo with final
	//			orientation value
	// REQUIRE: ipPageInfo != NULL
	//			nRotationInDegrees = 
	//				  0 - retain automatic orientation from rimgRotate
	//				 90 - update rimgRotate to ROT_RIGHT
	//				180 - update rimgRotate to ROT_DOWN
	//				270 - update rimgRotate to ROT_LEFT
	//				360 - update rimgRotate to ROT_NONE
	void updateOrientation(int nRotationInDegrees, IMG_ROTATE &rimgRotate, 
		ISpatialPageInfoPtr ipPageInfo);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Returns whether RecAPI is able to apply the given deskew to the image with the 
	//			specified image info.
	// PARAMS:  dDeskewDegrees is the angle of intended deskew to apply in degrees.
	//          imgInfo is the image info as obtained from kRecGetImgInfo on the skewed image.
	// PROMISE: Returns true if calling kRecDeskewImg will apply dDeskewDegrees to the image. 
	//          Returns false if the deskew request will be ignored.
	// NOTE:    kRecDeskewImg ignores certain small values of deskew based on the RecAPI 
	//          tradeoff setting (Kernel.OcrMgr.TradeOff) and the color information of the image.
	//          When no deskew has been applied, it is important to store zero for the deskew in 
	//          the spatial page info object. 
	bool isDeskewable(double dDeskewDegrees, IMG_INFO imgInfo);
	//--------------------------------------------------------------------------------------------
	// PURPOSE: Sets RecAPI's default character filter and RecAPI's filter plus character set, if
	//          the parameters differ from the currently set options.
	// PARAMS:  (1) bstrCharSet is the set of filter plus characters to recognize. This value is
	//              only used if the kCustomFilter option is enabled in eFilter.
	//          (2) eFilter is the set of filter options to apply.
	void setCharacterSetFilter(const _bstr_t& bstrCharSet, EFilterCharacters eFilter);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Sets m_ipSpatialString to the text recognized on the specified image pages using
	//          the specified options.
	// PARAMS: (1) strImageFileName is a valid image file name on which to recognize text.
	//         (2) vecPageNumbers contains 1-based page numbers on which to recognize text.
	//         (3) pZone is a rectangular area of the unrotated and undeskewed image in which to 
	//             recognize text, or NULL if the whole page should be recognized.
	//		   (4) nRotationInDegrees is one of {90, 180, 270, 360} if an explicit rotation should
	//             be applied, or 0 if the rotation should automatically detected.
	//         (5) bDetectHandwriting is true if handwritten text should be recognized, false if
	//             machine printed text should be recognized.
	//         (6) bReturnUnrecognized is true if unrecognized characters should be returned in
	//             the final output, flase if unrecognized characters should be dropped.
	//         (7) bReturnSpatialInfo is true if m_ipSpatialString should be spatial, false if 
	//             m_ipSpatialString should be non-spatial
	void recognizeText(string strFileName, const vector<long> &vecPageNumbers, RECT* pZone, 
		long nRotationInDegrees, bool bDetectHandwriting, bool bReturnUnrecognized, 
		bool bReturnSpatialInfo);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To recognize text on the specified pages using the specified parameters and store
	//          the result in:
	//          A) pvecLetters, if pvecLetters != NULL
	//          B) rstrText, if pvecLetters == NULL
	// PARAMS:  (1) vecPageNumbers - a vector of page numbers from which to recognize text
	//          (2) See rotateAndRecognizeTextInImagePage for other parameter descriptions
	void recognizeTextOnPages(const string& strFileName, const vector<long> vecPageNumbers, 
		LPRECT pZone, long nRotationInDegrees, bool bDetectHandwriting, bool bReturnUnrecognized, 
		string& rstrText, vector<CPPLetter>* pvecLetters, ILongToObjectMapPtr ipPageInfos);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Creates a spatial string (m_ipSpatialString) from the specified parameters.
	// PARAMS:  (1) strFileName - the source document name
	//          (2) pvecLetters - the vector of spatial letters. if the resultant spatial string
	//              should be:
	//              (A) spatial, pvecLetters != NULL
	//              (B) non-spatial, pvecLetters == NULL
	//          (3) strText - letters from which to create a non-spatial string. used only if
	//              pvecLetters is NULL.
	//          (4) ipPageInfos - spatial page info map. used only if pvecLetters != NULL.
	void storeResultAsSpatialString(const string& strFileName, vector<CPPLetter>* pvecLetters, 
		const string& strText, const ILongToObjectMapPtr& ipPageInfos);
	//--------------------------------------------------------------------------------------------
	// These methods are basically called directly by their COM counterparts.
	bool supportsTrainingFiles();
	void loadTrainingFile(string strTrainingFileName);
	//---------------------------------------------------------------------------------------------
	// Used by to set up RecAPI initially
	void init();
	void initEngineAndLicense();
	string getThisDLLFolder();
	//---------------------------------------------------------------------------------------------
	// this thread runs in the background ensuring that no operation takes to long it does this 
	// by monitoring progress updates in RecApi if too much time goes by between progress updates
	// this thread will Terminate the process
	static UINT timeoutThreadProc(void* pParam);
	// this is the member function implementation of the timeout thread
	void timeoutLoop();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Frees any memory allocated by calls to kRecGetOCRZoneLayout
	// PROMISE: Calls kRecFree for each non-NULL zone layout in m_listZoneLayouts
	void freeZoneLayoutMemory();
};

// helper method
//-------------------------------------------------------------------------------------------------
bool zoneIsLessThan(ZONE zoneLeft, ZONE zoneRight);
//-------------------------------------------------------------------------------------------------
// PURPOSE: Returns true if usLetterCode is a basic latin character (ie. alphabetic, numeric, or 
//          punctuation) or if it is the degree symbol. Returns false if usLetterCode is not in
//          basic latin character set (eg. accented characters, fractions, copyright symbol).
// P16 #2356
bool isBasicLatinCharacter(unsigned short usLetterCode);
//-------------------------------------------------------------------------------------------------
