// SpatialString.h : Declaration of the CSpatialString

#pragma once

#include "resource.h"       // main symbols
#include "UCLIDRasterAndOCRMgmt.h"
#include "CPPLetter.h"

#include <Win32CriticalSection.h>

#include <string>
#include <vector>
#include <map>
#include <utility>
#include <rapidjson/document.h>
#include <cpputil.h>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CSpatialString
class ATL_NO_VTABLE CSpatialString : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CSpatialString, &CLSID_SpatialString>,
	public ISupportErrorInfo,
	public IPersistStream,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IComparableObject, &IID_IComparableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ISpatialString, &IID_ISpatialString, &LIBID_UCLID_RASTERANDOCRMGMTLib>,
	public IDispatchImpl<IManageableMemory, &IID_IManageableMemory, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IHasOCRParameters, &IID_IHasOCRParameters, &LIBID_UCLID_RASTERANDOCRMGMTLib>
{
public:
	CSpatialString();
	~CSpatialString();

DECLARE_REGISTRY_RESOURCEID(IDR_SPATIALSTRING)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CSpatialString)
	COM_INTERFACE_ENTRY(ISpatialString)
	COM_INTERFACE_ENTRY2(IDispatch,ISpatialString)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(IComparableObject)
	COM_INTERFACE_ENTRY(IManageableMemory)
	COM_INTERFACE_ENTRY(IHasOCRParameters)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ISpatialString
	STDMETHOD(get_String)( BSTR *pVal);
	STDMETHOD(FindFirstItemInVector)( IVariantVector* pList, VARIANT_BOOL bCaseSensitive,
					  VARIANT_BOOL bPrioritizedVector, long lStartSearchPos, 
					  long* plStart, long* plEnd);
	STDMETHOD(FindFirstItemInRegExpVector)( IVariantVector* pList, VARIANT_BOOL bCaseSensitive,
						VARIANT_BOOL bPrioritizedVector, long lStartSearchPos,
					   	IRegularExprParser *pRegExprParser, long* plStart, 
					    	long* plEnd);
	STDMETHOD(GetChar)( long nIndex,  long *pChar);
	STDMETHOD(get_Size)( long *pVal);
	STDMETHOD(GetOCRImageLetter)( long nIndex,  ILetter **pLetter);
	STDMETHOD(GetOriginalImageBounds)( ILongRectangle **pBounds);
	STDMETHOD(GetWords)( IIUnknownVector **pvecWords);
	STDMETHOD(GetLines)( IIUnknownVector **pvecLines);
	STDMETHOD(GetParagraphs)( IIUnknownVector **pvecParagraphs);
	STDMETHOD(Insert)( long nPos,  ISpatialString *pString);
	STDMETHOD(Append)( ISpatialString *pString);
	STDMETHOD(GetSubString)( long nStart,  long nEnd,  ISpatialString **pSubString);
	STDMETHOD(Remove)( long nStart,  long nEnd);
	STDMETHOD(Replace)(BSTR strToFind, BSTR strReplacement, VARIANT_BOOL vbCaseSensitive, 
		long lOccurrence, IRegularExprParser *pRegExpr);
	STDMETHOD(ConsolidateChars)( BSTR strChars, VARIANT_BOOL bCaseSensitive);
	STDMETHOD(Trim)( BSTR strTrimLeadingChars, BSTR strTrimTrailingChars);
	STDMETHOD(FindFirstInstanceOfChar)( long nChar,  long nStartPos, 
		 long *pMatchPos);
	STDMETHOD(Tokenize)( BSTR strDelimiter,  IIUnknownVector **pvecItems);
	STDMETHOD(ToUpperCase)();
	STDMETHOD(ToLowerCase)();
	STDMETHOD(ToTitleCase)();
	STDMETHOD(Clear)();
	STDMETHOD(Offset)( long nX,  long nY);
	STDMETHOD(LoadFrom)(BSTR strFullFileName, VARIANT_BOOL bSetDirtyFlagToTrue,  BSTR *pstrOriginalSourceDocName);
	STDMETHOD(SaveTo)(BSTR strFullFileName, VARIANT_BOOL bCompress,	VARIANT_BOOL bClearDirty);
	STDMETHOD(AppendString)(BSTR strTextToAppend);
	STDMETHOD(get_SourceDocName)( BSTR *pVal);
	STDMETHOD(put_SourceDocName)( BSTR newVal);
	STDMETHOD(SetChar)( long nIndex,  long nChar);
	STDMETHOD(ConsolidateString)( BSTR strConsolidateString, VARIANT_BOOL bCaseSensitive);
	STDMETHOD(UpdatePageNumber)( long nPageNumber);
	STDMETHOD(GetSpecifiedPages)( long nStartPageNum,  long nEndPageNum,  ISpatialString** ppResultString);
	STDMETHOD(GetPages)(VARIANT_BOOL vbIncludeBlankPages, BSTR strTextForBlankPages, IIUnknownVector **pvecPages);
	STDMETHOD(GetAverageLineHeight)( long *lpHeight);
	STDMETHOD(GetAverageCharWidth)( long* lpWidth);
	STDMETHOD(GetAverageCharHeight)( long *lpHeight);
	STDMETHOD(GetRelativePages)( long nStartPageNum,  long nEndPageNum,  ISpatialString** ppResultString);
	STDMETHOD(GetSplitLines)( long nMaxSpace,  IIUnknownVector** ppResultVector);
	STDMETHOD(CreateFromLines)( IIUnknownVector* pLines);
	STDMETHOD(GetJustifiedBlocks)( long nMinLines,  IIUnknownVector** ppResultVector);
	STDMETHOD(GetBlocks)( long nMinLines,  IIUnknownVector** ppResultVector);
	STDMETHOD(GetNextOCRImageSpatialLetter)( long nStartPos,  ILetter** pLetter,  long* pIndex);
	STDMETHOD(GetNextNonSpatialLetter)( long nStartPos,  ILetter** pLetter,  long* pIndex);
	STDMETHOD(GetIsEndOfWord)( long nIndex,  VARIANT_BOOL* pbIsEnd);
	STDMETHOD(GetIsEndOfLine)( long nIndex,  VARIANT_BOOL* pbIsEnd);
	STDMETHOD(LoadFromMultipleFiles)( IVariantVector *pvecFiles,  BSTR strSourceDocName);
	STDMETHOD(GetPageInfo)( long nPageNum,  ISpatialPageInfo** ppPageInfo);
	STDMETHOD(SetPageInfo)( long nPageNum,  ISpatialPageInfo* pPageInfo);
	STDMETHOD(get_SpatialPageInfos)( ILongToObjectMap **pVal);
	STDMETHOD(put_SpatialPageInfos)(ILongToObjectMap *pVal);
	STDMETHOD(GetOriginalImageRasterZones)( IIUnknownVector** ppRasterZones);
	STDMETHOD(IsMultiPage)( VARIANT_BOOL *pbRet);
	STDMETHOD(GetFirstPageNumber)( long *pRet);
	STDMETHOD(GetLastPageNumber)( long *pRet);
	STDMETHOD(FindFirstInstanceOfString)( BSTR strSearchString,  long nStartPos, 
		 long *pMatchPos);
	STDMETHOD(GetOCRImageLetterArray)( long* pnNumLetters,  void** ppLetters); 
	STDMETHOD(CreateFromLetterArray)( long nNumLetters,  void* pLetters, BSTR bstrSourceDocName,
		 ILongToObjectMap* pPageInfoMap); 
	STDMETHOD(CreateFromILetters)(IIUnknownVector* pLetters, BSTR bstrSourceDocName, ILongToObjectMap* pPageInfoMap);
	STDMETHOD(SelectWithFontSize)( VARIANT_BOOL bInclude,  long nMinFontSize,  long nMaxFontSize,  ISpatialString** ppResultString);
	STDMETHOD(GetCharConfidence)( long* pnMinConfidence, 
		 long* pnMaxConfidence,  long* pnAvgConfidence);
	STDMETHOD(GetFontSizeDistribution)( ILongToLongMap** ppMap);
	STDMETHOD(GetFontInfo)(  long nMinPercentage, 
			 VARIANT_BOOL* pbItalic,  VARIANT_BOOL* pbBold,
			 VARIANT_BOOL* pbSansSerif,  VARIANT_BOOL* pbSerif, 
			 VARIANT_BOOL* pbProportional,  VARIANT_BOOL* pbUnderline, 
			 VARIANT_BOOL* pbSuperScript,  VARIANT_BOOL* pbSubScript);
	STDMETHOD(IsSpatiallyLessThan)( ISpatialString* pSS,  VARIANT_BOOL* pbRetVal);
	STDMETHOD(GetMode)( ESpatialStringMode *pVal);
	STDMETHOD(DowngradeToNonSpatialMode)();
	STDMETHOD(DowngradeToHybridMode)();
	STDMETHOD(AddRasterZones)(IIUnknownVector *pVal, ILongToObjectMap* pPageInfoMap);
	STDMETHOD(HasSpatialInfo)( VARIANT_BOOL *pbValue);
	STDMETHOD(CreateHybridString)(IIUnknownVector *pVecRasterZones, BSTR bstrText, 
		BSTR bstrSourceDocName, ILongToObjectMap *pPageInfoMap);
	STDMETHOD(IsEmpty)( VARIANT_BOOL *pvbIsEmpty);
	STDMETHOD(ContainsStringInVector)( IVariantVector* pVecBSTRs, 
		 VARIANT_BOOL vbCaseSensitive,  VARIANT_BOOL vbAreRegExps, 
		 IRegularExprParser *pRegExprParser,
		 VARIANT_BOOL *pvbContainsString);
	STDMETHOD(ReplaceAndDowngradeToHybrid)( BSTR bstrReplacement);
	STDMETHOD(FindFirstInstanceOfStringCIS)( BSTR strSearchString,  long nStartPos, 
		 long *pMatchPos);
	STDMETHOD(CreatePseudoSpatialString)(IRasterZone *pZone, BSTR bstrText, BSTR bstrSourceDocName, 
		ILongToObjectMap *pPageInfoMap);
	STDMETHOD(GetWordLengthDist)(long* plTotalWords, ILongToLongMap** ppWordLengthMap);
	STDMETHOD(GetOriginalImageRasterZonesGroupedByConfidence)(IVariantVector* pVecOCRConfidenceBoundaries,
		VARIANT_BOOL vbByWord, IVariantVector** ppZoneOCRConfidenceTiers, 
		IVariantVector** ppZoneIndices, IIUnknownVector** ppRasterZones);
	STDMETHOD(CreateNonSpatialString)(BSTR bstrText, BSTR bstrSourceDocName);
	STDMETHOD(ReplaceAndDowngradeToNonSpatial)(BSTR bstrText);
	STDMETHOD(GetOCRImageRasterZones)(IIUnknownVector** ppRasterZones);
	STDMETHOD(GetOCRImageRasterZonesGroupedByConfidence)(IVariantVector* pVecOCRConfidenceBoundaries,
		VARIANT_BOOL vbByWord, IVariantVector** ppZoneOCRConfidenceTiers, 
		IVariantVector** ppZoneIndices, IIUnknownVector** ppRasterZones);
	STDMETHOD(GetOCRImageBounds)(ILongRectangle** ppBounds);
	STDMETHOD(InsertString)(long nPos, BSTR bstrText);
	STDMETHOD(GetTranslatedImageRasterZones)(ILongToObjectMap* pPageInfoMap,
		IIUnknownVector** ppRasterZones);
	STDMETHOD(GetTranslatedImageBounds)(ILongToObjectMap* pPageInfoMap, ILongRectangle** ppBounds);
	STDMETHOD(get_OCREngineVersion)(BSTR* pbstrOCREngine);
	STDMETHOD(put_OCREngineVersion)(BSTR bstrOCREngine);
	STDMETHOD(MergeAsHybridString)(ISpatialString* pStringToMerge);
	STDMETHOD(GetOriginalImagePageBounds)(long nPageNum, ILongRectangle** ppBounds);
	STDMETHOD(GetOCRImagePageBounds)(long nPageNum, ILongRectangle** ppBounds);
	STDMETHOD(ContainsCharacterOutsideFontRange)(long nFontMin, long nFontMax, VARIANT_BOOL* pbResult);
	STDMETHOD(GetFirstCharPositionOfPage)(long nPageNum, long *pFirstCharPos);
	STDMETHOD(GetOriginalImageLetterArray)(long* pnNumLetters, void** ppLetters);
	STDMETHOD(RemoveText)(ISpatialString* pTextToRemove, long nPage, ILongRectangle *pRect,
		long* nPos);
	STDMETHOD(InsertBySpatialPosition)(ISpatialString *pString,
		VARIANT_BOOL vbAllowOverlappingInsertion, VARIANT_BOOL* pbStringWasInserted);
	STDMETHOD(SetSurroundingWhitespace)(ISpatialString *pString, long nPos, long* pnNewPos);
	STDMETHOD(TranslateToNewPageInfo)(ILongToObjectMap* pPageInfoMap);
	STDMETHOD(ValidatePageDimensions)();
	STDMETHOD(CreateFromSpatialStrings)(IIUnknownVector *pStrings, VARIANT_BOOL vbInsertPageBreaks);
	STDMETHOD(GetUnrotatedPageInfoMap)( ILongToObjectMap **pVal);
	STDMETHOD(GetPseudoSpatialFromHybrid)(ISpatialString** ppResultString);
	STDMETHOD(AppendToFile)(BSTR bstrOutputFile, VARIANT_BOOL vbCheckForExistingPages);
	STDMETHOD(LoadPageFromFile)(BSTR bstrInputFile, long nPage);
	STDMETHOD(LoadPagesFromFile)(BSTR bstrInputFile, IIUnknownVector** pVal);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown **pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown *pObject);

// IComparableObject
	STDMETHOD(raw_IsEqualTo)(IUnknown * pObj, VARIANT_BOOL * pbValue);

// IManageableMemory
	STDMETHOD(raw_ReportMemoryUsage)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)( VARIANT_BOOL * pbValue);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// IHasOCRParameters
	STDMETHOD(get_OCRParameters)(IOCRParameters** ppMap);
	STDMETHOD(put_OCRParameters)(IOCRParameters* pMap);

private:
	////////////
	// Variables
	////////////
	// the contents of this spatial string as a string
	string m_strString;

	// source document's name
	string m_strSourceDocName;

	// The OCR engine version
	string m_strOCREngineVersion;

	// A vector of letters containing spatial information about each letter in the string
	// This only necessarily exists when m_bIsSpatial == true
	// letter[i].m_lGuess1 should always correspond directly with m_strString.at(i)
	// (though one is a long and the other is a char)
	vector<CPPLetter> m_vecLetters;

	// A vector of letters containing spatial information about each letter in the string in
	// original image coordinates rather than in OCR coordinates (as m_vecLetters is)
	vector<CPPLetter> m_vecOriginalImageLetters;

	// the page info map and a method to access it that guarantees that the map
	// object is never null (i.e. the method will initialize m_ipPageInfoMap to an empty map
	// if m_ipPageInfoMap is NULL)
	ILongToObjectMapPtr m_ipPageInfoMap;
	ILongToObjectMapPtr getPageInfoMap();

	// boolean variable to keep track of the "dirty" state of this 
	// object since the last save or load operation
	bool m_bDirty;

	// Enumeration for one of three types.
	// kNonSpatialMode - Default value, no spatial information present.
	// kSpatialMode - m_strString is set and the vector of letters has at least 1 spatial letter
	// kHybridMode - m_strString is set and the spatial string has at least 1 RasterZone assigned to it
	ESpatialStringMode m_eMode;
	
	// Vector of raster zones (only contains zones when the string is hybrid)
	vector<UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr> m_vecRasterZones;

	// Allows reporting of memory usage to the garabage collector when being referenced by managed
	// code.
	IMemoryManagerPtr m_ipMemoryManager;

	// For thread protection 
	CCriticalSection m_criticalSection;
	
	IOCRParametersPtr m_ipOCRParameters;
		
	///////////
	// Methods
	///////////
	//----------------------------------------------------------------------------------------------
	UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr getThisAsCOMPtr();
	//----------------------------------------------------------------------------------------------
	// PROMISE:	To throw an exception if this object is not licensed to run
	void validateLicense();
	//----------------------------------------------------------------------------------------------
	// Inserts the specified spatial string at the specified position
	void insert(long nPos, UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipStringToInsert);
	//----------------------------------------------------------------------------------------------
	// Appends the specified spatial string to the end of this string
	void append(UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipStringToAppend);
	//----------------------------------------------------------------------------------------------
	// Inserts the specified text into this spatial string at the specified position
	void insertString(long nPos, const string& strText);
	//----------------------------------------------------------------------------------------------
	// Appends the specified text to the end of this string
	void appendString(const string& strText);
	//----------------------------------------------------------------------------------------------
	// Adds the specified vector of raster zones to this string (the resulting string
	// will now be hybrid mode, no matter what mode it started as)
	void addRasterZones(IIUnknownVectorPtr ipRasterZones, ILongToObjectMapPtr ipPageInfoMap);
	//----------------------------------------------------------------------------------------------
	// Returns true if this string is multi-paged and false otherwise
	bool isMultiPage();
	//----------------------------------------------------------------------------------------------
	// Gets a substring at the specified starting and ending location
	UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr getSubString(long nStart, long nEnd);
	//----------------------------------------------------------------------------------------------
	// Updates the spatial page info map with the new entries and validates that the
	// new entries are compatible (i.e. either the entry did not exist in the map
	// already or if it exists the values are equal)
	// PROMISE: To throw an exception if the width or height of a page are different
	//			in the new map than in the old.
	// RETURNS: Whether the spatial data needs to be translated from the current info to the new info:
	//			I.e., true if there is at least one page with different orientation or deskew value
	//			and false if all common pages have the same orientation and deskew values.
	bool updateAndValidateCompatibleSpatialPageInfo(ILongToObjectMapPtr ipPageInfoMap);
	//----------------------------------------------------------------------------------------------
	// PROMISE:	To return a vector of non-spatial letter objects where each
	//			of the letter objects in sequence represent the sequence of
	//			characters in strText.
	void getNonSpatialLetters(const string& strText, vector<CPPLetter>& vecLetters);
	//----------------------------------------------------------------------------------------------
	// PROMISE: To throw an exception if m_bIsSpatial == true and the length of
	//			m_ipLetters is not the same as the length of m_strString
	void performConsistencyCheck();
	//----------------------------------------------------------------------------------------------
	// PROMISE: To throw an exception if nIndex is not a valid index for the
	//			m_ipLetters vector
	void verifyValidIndex(long nIndex);
	//----------------------------------------------------------------------------------------------
	// PROMISE: To throw an exception if GetMode() != kSpatialMode
	void verifySpatialness();
	//----------------------------------------------------------------------------------------------
	// PROMISE: To throw an exception if strSourceDocName is not empty
	//			and name is different from m_strSourceDocName,
	//			and m_strSourceDocName is non-empty.
	//			If m_strSourceDocName is empty, and strSourceDocName
	//			is non-empty then strSourceDocName will be stored as this	
	//			object's source document name
	void validateAndMergeSourceDocName(const string& strSourceDocName);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	Reset all member variables.  The m_strSourceDocName attribute 
	//			will be reset only if bResetSourceDocName == true. The m_ipPageInfoMap will
	//			only be reset if bResetPageInfo == true and m_ipPageInfoMap != __nullptr
	void reset(bool bResetSourceDocName, bool bResetPageInfo);
	//----------------------------------------------------------------------------------------------
	// PROMISE: To compute m_strString from ipLetters, and to set m_ipLetters to 
	//			ipLetters if there is at least one spatial Letter therein.  
	//			The value for m_eMode will be also computed from the Letter objects
	void processLetters(CPPLetter* letters, long nNumLetters);
	//----------------------------------------------------------------------------------------------
	// methods to check/load/save this object from text or spatial files
	void loadFromTXTFile(const string& strFileName);
	void saveToTXTFile(const string& strFileName);
	//----------------------------------------------------------------------------------------------
	// Loads specified text file as "indexed" text by loading it as a spatial string where the X
	// coordinate of each letter is the position of the character in the text file.
	void loadTextWithPositionalData(const string& strFileName, EFileType eFileType);
	//----------------------------------------------------------------------------------------------
	// according to pass in start/end page number and total page number,
	// find out the actual start/end page number 
	// Return true if entire document shall be returned
	// Return false if a certain range of pages need to be returned
	bool getStartAndEndPageNumber(long& nStartPage, long& nEndPage, long nTotalPageNum);
	//----------------------------------------------------------------------------------------------
	// returns true if rect1 and 2 are overlapping 
	// horizontally by at least some percentage nMinOverlap
	bool overlapX(const RECT& rect1, const RECT& rect2, long nMinOverlap);
	//----------------------------------------------------------------------------------------------
	// Returns true if the letter at nIndex is the last letter of a word
	bool getIsEndOfWord(long nIndex);
	//----------------------------------------------------------------------------------------------
	// Returns true if the letter at nIndex is the last letter of a line. Considers only the char
	// value and page, not the spatial location of the next char in determining if nIndex is at the
	// end of a line.
	bool getIsEndOfLine(long nIndex);
	//----------------------------------------------------------------------------------------------
	// Returns true if the letter at nIndex is the last letter of a zone. Considers only the char
	// properties unless bTreatGapsAsZoneBoundaries is true.
	// If bTreatGapsAsZoneBoundaries is true then true if right of this letter is less than left of
	// the next letter
	bool getIsEndOfZone(long nIndex, bool bTreatGapsAsZoneBoundaries);
	//----------------------------------------------------------------------------------------------
	// Returns true if the letter at nIndex is the last letter of a line. Considers the spatial info
	// for the line up to index (specified by rectCurrentLineZone) to determine if a new line should
	// be started based upon the location of the next character compared to rectCurrentLineZone.
	bool getIsEndOfLine(size_t index, CRect rectCurrentLineZone);
	//----------------------------------------------------------------------------------------------
	// UpdateString was an implemented member from the IDL file, but moved here. The 
	// string is updated, but the m_strSourceDocName remains the same.
	void updateString(const string& string);
	//----------------------------------------------------------------------------------------------
	void updateLetters(CPPLetter* letters, long nNumLetters);
	//----------------------------------------------------------------------------------------------
	// UpdateHybrid puts a new string and RasterZone into the spatial string, but retains the 
	// value of m_strSourceDocName
	void updateHybrid(const vector<UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr>& vecZones,
		const string& strText);
	//----------------------------------------------------------------------------------------------
	// PROMISE: Downgrades the spatial string to non-spatial mode
	void downgradeToNonSpatial();
	//----------------------------------------------------------------------------------------------
	// REQUIRE: m_eMode != kNonSpatialMode
	// PROMISE: To downgrade the spatial string to hybrid mode
	void downgradeToHybrid();
	//----------------------------------------------------------------------------------------------
	// PROMISE: Will check if there is a percentage greater than the supplied min percentage of characters
	//			that have that type of font.
	void checkForFontInfo( VARIANT_BOOL* pbType, long nNumCharsOfType, long nNumSpatialChars, long nMinPercentage);
	//----------------------------------------------------------------------------------------------
	// PROMISE: If GetMode() == kSpatialMode check the letters vector for at least 1 spatial letter.
	//			If none are found, downgrade to kNonSpatialMode and clear the letter vector.
	//			If GetMode() == kHybridMode check the raster zone vector to verify that it has at least
	//			one raster zone. If none are found, then downgrade the spatial string to kNonSpatialMode.
	//			m_eMode will properly reflect the mode of the SpatialString
	void reviewSpatialStringAndDowngradeIfNeeded();
	//----------------------------------------------------------------------------------------------
	// REQUIRE: Each object within ipZones is an IRasterZone object
	// PROMISE: Examines data members of each Raster Zone object in ipZones to see if any zone 
	//			already contained in ipZones matches ipNewZone.  Returns true if a match is found, 
	//			otherwsie false.
	bool isRasterZoneInVector(UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipNewZone, 
		const vector<UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr>& vecZones);
	//---------------------------------------------------------------------------------------------
	// Method used internally by the Replace method
	void performReplace(const string& stdstrToFind, const string& stdstrReplacement,
		VARIANT_BOOL vbCaseSensitive, long lOccurrence, IRegularExprParserPtr ipRegExpr);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Gets replacement matches for the specified string
	IIUnknownVectorPtr getReplacements(const string& strFind, const string& strReplacement, 
		bool bCaseSensitive, long lOccurrence, IRegularExprParserPtr ipRegExpr);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: If one or more of two specified Cartesian coordinates is not contained inside an 
	//			image whose center is located at (dCenterLeft, dCenterTop) in top-left coordinates,
	//			shift the coordinates along the line that they form to be just within the image.
	// PARAMS:  (x1, y1) and (x2,y2) are points in a Cartesian coordinate system relative to an 
	//			image where the center of the image is the origin.
	//          (dCenterLeft, dCenterTop) are the left-top coordinates of center of the image.
	// PROMISE: If (x1, y1) and (x2, y2) were contained in the image, they will be unchanged. If
	//			one or more of them was outside the bounds of the image, they will be adjusted
	//			to fit along the edge of the image, preserving the angle between the two points.
	// NOTE:    In the case where the resultant line is less than three pixels, the line will be 
	//          extended to at least three pixels. [P16 #2570] This may mean that the angle between
	//          the points will not be preserved.
	static void fitPointsWithinBounds(double &x1, double &y1, double &x2, double &y2, 
		double dCenterLeft, double dCenterTop);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: If the specified Cartesian coordinate is not contained inside an image whose center
	//			is located at (lCenterLeft, lCenterTop) in top-left coordinates, shift the
	//			coordinate along the line specified by the given slope and y-intercept to fit just 
	//			within the image.
	// PARAMS:  (x, y) is a point in a Cartesian coordinate system relative to an image such that
	//			the center of the image is the origin.
	//			dSlope and dYIntercept are the slope and y-intercept of a line for the coordinate
	//			to be shifted along.
	//          (dCenterLeft, dCenterTop) are the left-top coordinates of center of the image.
	// PROMISE: If (x, y) is contained inside the image, it will be unchanged. If it is outside it 
	//			will be adjusted to the closest point where the line intersects the edge of image.
	static void shiftPointInsideBoundsAlongLine(double &x, double &y, double dSlope, double dYIntercept, 
		double dCenterLeft, double dCenterTop);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: Searches m_strString for strSearchString and returns the position of the first 
	//			character if found or -1 if not found.  The case-sensitive version of this function 
	//			is called by FindFirstInstanceOfString().  The case-insensitive version of this 
	//			function is called by FindFirstInstanceOfStringCIS().
	// PARAMS:  strSearchString - the string to be found
	//			nStartPos - the first character of m_strString to be searched
	//			bCaseSensitive - do a case-sensitive search if true or case-insensitive search 
	//				if false
	long findFirstInstanceOfStringCS(const string& strSearchString, long nStartPos, 
		bool bCaseSensitive);
	//----------------------------------------------------------------------------------------------
	// REQUIRE: GetMode == kSpatialMode
	// PURPOSE: Builds a raster zone using the specified area on the specified page.  The zones will
	// be positioned according to the skew and rotation of the page.
	UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr translateToOriginalImageZone(long lStartX,
		long lStartY, long lEndX, long lEndY, long lHeight, long nPage);
	UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr translateToOriginalImageZone(
		UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipZone);
	//----------------------------------------------------------------------------------------------
	// REQUIRE: GetMode == kSpatialMode
	// PURPOSE: Translates the coordinates of pLetters so that their original image coordinates
	//			would remain the same with ipNewPageInfo rather than the current spatial page info.
	void translateToNewPageInfo(CPPLetter *pLetters, long nNumLetters,
		ILongToObjectMapPtr ipNewPageInfoMap = __nullptr);
	//----------------------------------------------------------------------------------------------
	// REQUIRE: GetMode != kNonSpatialMode, ipPageInfoMap contains an entry the page the spatial
	//			string is on.
	// PURPOSE: Adjusts the coordinates of the raster zone so they are relative to the specified
	//			spatial page info. (rather than that of the page or OCR coordinates or that of the
	//			image itself) If ipNewPageInfoMap is NULL, the coordinates returned will be relative
	//			to the original image coordinates.
	static UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr translateToNewPageInfo(
		UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipZone,
		ILongToObjectMapPtr ipOldPageInfoMap,
		ILongToObjectMapPtr ipNewPageInfoMap);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: Adjusts the specified coordinates so they are relative to the specified spatial page
	//			info. If ipNewPageInfoMap is NULL, the coordinates returned will be relative to the
	//			original image coordinates.
	static UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr translateToNewPageInfo(long lStartX, long lStartY,
		long lEndX, long lEndY, long lHeight, int nPage,
		ILongToObjectMapPtr ipOldPageInfoMap,
		UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr ipNewPageInfo);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: Given an image of the specified width and height, returns the center point (with
	//			x and y coordinates inverted if specified.
	static CPoint getImageCenterPoint(int nImageWidth, int nImageHeight, bool invertCoordinates);
	//----------------------------------------------------------------------------------------------
	// REQUIRE: HasSpatialInfo() == true
	// PURPOSE: To return a vector of raster zones in the OCR coordinate system
	vector<UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr> getOCRImageRasterZones();
	//----------------------------------------------------------------------------------------------
	// REQUIRE: HasSpatialInfo() == true
	// PURPOSE: To return an IUknownVector of raster zones in the OCR coordinate system
	IIUnknownVectorPtr getOCRImageRasterZonesUnknownVector();
	//----------------------------------------------------------------------------------------------
	// REQUIRE: HasSpatialInfo() == true
	// PURPOSE: To return an IUknownVector of raster zones in the Original image coordinate system
	IIUnknownVectorPtr getOriginalImageRasterZones();
	//----------------------------------------------------------------------------------------------
	// REQUIRE: HasSpatialInfo() == true,  ipNewPageInfoMap contains an entry for the page the
	//			specified raster zone is on.
	// PURPOSE: To return an IUknownVector of raster zones in the coordinate system of the specified
	//			page infos.
	IIUnknownVectorPtr getTranslatedImageRasterZones(ILongToObjectMapPtr ipNewPageInfoMap);
	//----------------------------------------------------------------------------------------------
	// PURPORE: To get the bounds of the specified page number.
	// PARAMS:  If bUseOCRImageCoordinates is true and the specified page is rotated either 90 or 270
	//			degrees, the width and height will be swapped so that the width is the width relative
	//			to the text not relative to the physicial image.
	ILongRectanglePtr getPageBounds(long nPage, bool bUseOCRImageCoordinates);
	//----------------------------------------------------------------------------------------------
	// REQUIRE: GetMode == kSpatialMode
	// PURPOSE: Returns a vector of IRasterZones that represent the spatial string where the
	//			raster zones will be divided between letters on opposite sides of a specified
	//			OCR confidence boundary.
	// PARAMS:  ipVecOCRConfidenceBoundaries- Specifies the OCR Confidence boundaries used to
	//			divide raster zones. Must be a number in the range 1 - 100 and must be specified
	//			in ascending order.  A character with an OCR confidence matching a boundary value
	//			will be considered to be on the same side of the boundary with as those with 
	//			lower confidence levels. (ie. A boundary of 90 will group characters with OCR
	//			confidence values 0 - 90 and 91 - 100.
	//			vbByWord- If VARIANT_TRUE, each word will be returned as a zone having the
	//			confidence of its least confident letter. For example, if a 5 letter word has
	//			letter confidence values of 50, 60, 70, 80, and 90 it will be treated as if all
	//			characters have a confidence of 50.
	//			ipZoneOCRConfidenceTiers- Returns a vector that specifies which level of OCR
	//			confidence each raster zone is associated with as the index of the next higher
	//			boundary in pVecOCRConfidenceBoundaries. For example, if a boundaries of 70 and
	//			90 are specified, and a string contains 5 chars with confidences of 0, 69, 70,
	//			71 and 90, three zones will be returned with first 2 chars in the first zone,
	//			the third and fourth char in the second zone, and the last char in the third
	//			zone.  ppZoneOCRConfidenceLevels will have 3 entries: 0, 1 and 2.
	//			ppZoneIndices- Returns the character indicies of the start of each new zone
	//			returned beginning with the index of the first spatial character in the string.
	IIUnknownVectorPtr getOCRImageRasterZonesGroupedByConfidence(bool bByWord,
		IVariantVectorPtr ipVecOCRConfidenceBoundaries,
		IVariantVectorPtr &ipZoneOCRConfidenceTiers, IVariantVectorPtr &ipIndices);
	//----------------------------------------------------------------------------------------------
	// REQUIRE: GetMode == kSpatialMode
	// PURPOSE: Returns a vector of IRasterZones that represent the spatial string where the
	//			raster zones will be divided between letters on opposite sides of a specified
	//			OCR confidence boundary. The coordinates will be in the Original image coordinate
	//			system.
	// PARAMS:  ipVecOCRConfidenceBoundaries- Specifies the OCR Confidence boundaries used to
	//			divide raster zones. Must be a number in the range 1 - 100 and must be specified
	//			in ascending order.  A character with an OCR confidence matching a boundary value
	//			will be considered to be on the same side of the boundary with as those with 
	//			lower confidence levels. (ie. A boundary of 90 will group characters with OCR
	//			confidence values 0 - 90 and 91 - 100.
	//			vbByWord- If VARIANT_TRUE, each word will be returned as a zone having the
	//			confidence of its least confident letter. For example, if a 5 letter word has
	//			letter confidence values of 50, 60, 70, 80, and 90 it will be treated as if all
	//			characters have a confidence of 50.
	//			ipZoneOCRConfidenceTiers- Returns a vector that specifies which level of OCR
	//			confidence each raster zone is associated with as the index of the next higher
	//			boundary in pVecOCRConfidenceBoundaries. For example, if a boundaries of 70 and
	//			90 are specified, and a string contains 5 chars with confidences of 0, 69, 70,
	//			71 and 90, three zones will be returned with first 2 chars in the first zone,
	//			the third and fourth char in the second zone, and the last char in the third
	//			zone.  ppZoneOCRConfidenceLevels will have 3 entries: 0, 1 and 2.
	//			ppZoneIndices- Returns the character indicies of the start of each new zone
	//			returned beginning with the index of the first spatial character in the string.
	IIUnknownVectorPtr getOriginalImageRasterZonesGroupedByConfidence(bool bByWord,
		IVariantVectorPtr ipVecOCRConfidenceBoundaries,
		IVariantVectorPtr &ipZoneOCRConfidenceTiers, IVariantVectorPtr &ipIndices);
	//----------------------------------------------------------------------------------------------
	// REQUIRE: m_eMode != kNonSpatialMode
	// PURPOSE: To return the first page number for the spatial string
	long getFirstPageNumber();
	//----------------------------------------------------------------------------------------------
	// REQUIRE: m_eMode != kNonSpatialMode
	// PURPOSE: To return the last page number for the spatial string
	long getLastPageNumber();
	//----------------------------------------------------------------------------------------------
	// REQUIRE: m_eMode == kSpatialMode
	// PURPOSE: To return the next spatial letter from the spatial string (after the specified
	//			start position.  If no letter is found then will return -1.  If a letter is
	//			found then rLetter will contain the found letter.
	long getNextOCRImageSpatialLetter(long nStart, CPPLetter& rLetter);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To return an IUnknownVector of spatial strings containing the current spatial
	//			string divided by words.
	IIUnknownVectorPtr getWords();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To return a vector of pairs containing the begin and end characters for the string
	//			divided at line boundaries
	void getLines(vector<pair<long, long>>& rvecLines);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To return a vector of pairs containing the begin and end characters for the string
	//			divided at line and, if this is a text-based spatial string, zone boundaries
	void getLinesAndZones(vector<pair<long, long>>& rvecLines);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To return an IUnknownVector of spatial strings containing the current spatial
	//			string divided by lines.
	IIUnknownVectorPtr getLinesUnknownVector();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To return an IUnknownVector of spatial strings containing the current spatial
	//			string divided by paragraphs.
	IIUnknownVectorPtr getParagraphs();
	//----------------------------------------------------------------------------------------------
	// REQUIRE: m_eMode == kSpatialMode
	// PURPOSE: To compute the average character width for the spatial string
	long getAverageCharWidth();
	//----------------------------------------------------------------------------------------------
	// REQUIRE: m_eMode == kSpatialMode
	// PURPOSE: To compute the average character height for the spatial string
	// NOTE:	This value will be smaller than the result of GetAverageLineHeight which
	//			measures average the distance between lines.
	long getAverageCharHeight();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To remove the characters in the specified range (will downgrade the string
	//			if necessary after removing the characters)
	void remove(long nStart, long nEnd);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To find the start and end position of the first string in the vector of
	//			strings (sets start and end to -1 if none of the strings are found). If
	//			bPrioritizedVector is true then will stop after the first item in the vector
	//			is found rather than continuing to search in case there is an item with an
	//			earlier start and end position in the string.
	void findFirstItemInVector(IVariantVectorPtr ipList, bool bCaseSensitive,
		bool bPrioritizedVector, long lStartSearchPos, long& rlStart, long& rlEnd);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To find the start and end position of the first string in the vector of
	//			regular expressions (sets start and end to -1 if none of the strings are found). If
	//			bPrioritizedVector is true then will stop after the first item in the vector
	//			is found rather than continuing to search in case there is an item with an
	//			earlier start and end position in the string.
	void findFirstItemInRegExpVector(IVariantVectorPtr ipList, bool bCaseSensitive,
		bool bPrioritizedVector, long lStartSearchPos, IRegularExprParserPtr ipRegExprParser,
		long& rlStart, long& rlEnd);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To copy the specified spatial string into this spatial string
	void copyFromSpatialString(UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipSource);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: Converts a pre-version 8.0 hybrid string (<= SpatialString version 11) which uses
	//			image coordinates into the OCR coordinate system.
	void autoConvertLegacyHybridString();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: Appends the provided SpatialString producing a hybrid string result. Similar
	//			to append except that the method will compensate for differing page infos which
	//			would otherwise cause an exception in append.
	void mergeAsHybridString(UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipStringToMerge);
	//----------------------------------------------------------------------------------------------
	// Used by the GetFirstCharPositionOfPage to get the first position on a page
	// this returns the first character on a page with the assumption that the
	// non spatial characters after the last spatial character on the previous page and the first 
	// spatial character on the page specified are on the specified page. If the page is not found,
	// the last position on what would be the previous page will be returned.
	long getFirstCharPositionOfPage(long nPageNumber);
	//----------------------------------------------------------------------------------------------
	// Removes whitespace in this string and/or appends whitespace around pString in order to allow
	// pString to be added at position nPos and have appropriate whitespace surrounding it.
	// Returns the position at which pString is prepared to be added following the adjustments in
	//whitespace.
	void setSurroundingWhitespace(UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipString, long& rnPos);
	//----------------------------------------------------------------------------------------------
	// Gets the spatial page info map of the spatial string without any deskew or rotation applied.
	ILongToObjectMapPtr getUnrotatedPageInfoMap();
	//----------------------------------------------------------------------------------------------
	// Creates a blank page by going to the source document and getting the dimensions of the page
	// and then creates a pseudo-spatial string with the given text
	UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr makeBlankPage(int nPage, string textForPage);
	//----------------------------------------------------------------------------------------------
	IOCRParametersPtr getOCRParameters();
	//----------------------------------------------------------------------------------------------
	// Loads spatial string from (optionally gzipped) COM storage object
	void loadFromStorageObject(const string& strInputFile, UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipLoadInto);
	//----------------------------------------------------------------------------------------------
	// Loads spatial string from GCV json format
	// nPageNumber is assigned to the result (page number is not stored in the GCV json)
	void loadFromGoogleJson(const string& strInputFile, long nPageNumber, UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipLoadInto);
	//----------------------------------------------------------------------------------------------
	static void addWordsToLetterArray(const rapidjson::Value::ConstArray& words,
		unsigned short pageNumber, double widthConvertedFromPoints, double heightConvertedFromPoints,
		vector<CPPLetter>& letters, bool& hasVertices, bool& hasNormalizedVertices, vector<double>& thetas);
	//----------------------------------------------------------------------------------------------
	struct AnnotationSpatialProperties
	{
		double y1;
		double x1;
		double y2;
		double x2;
		double y3;
		double x3;
		double y4;
		double x4;
		double width;
		double height;
		double theta;
		long orientation;
	};
	static bool getAnnotationSpatialInfo(const rapidjson::Value& el, bool& hasVertices, bool& hasNormalizedVertices, AnnotationSpatialProperties& properties);
	//----------------------------------------------------------------------------------------------
	static void getSymbolSpatialInfoFromWord(const AnnotationSpatialProperties& wordProperties, long numSymbolsInWord, long symbolIdx, AnnotationSpatialProperties& symbolProperties);
	//----------------------------------------------------------------------------------------------
	static void setLetterBounds(CPPLetter& letter, const AnnotationSpatialProperties& properties, const double& denormalizationFactorX, const double& denormalizationFactorY);
	//----------------------------------------------------------------------------------------------
	// Loads individual pages from numbered files in a zip archive and puts them together
	void loadFromArchive(const string& strInputFile);
	//----------------------------------------------------------------------------------------------
	// Loads individual pages from numbered files in a zip archive
	// If bReadInfoOnly is true then the returned map will have a key for each page but null values
	// If bReadInfoOnly is false then the returned map will have corresponding page spatial strings for each key
	// If nPage > 0 then only the specified page number will be returned (or an empty map if that page doesn't exist in the archive)
	// If bLoadIntoThis is true then the specified page will be loaded into this instance and the return value will be null
	unique_ptr<map<long, UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr>> loadPagesFromArchive(
		const string& strInputFile,
		bool bReadInfoOnly,
		string* strOriginalSourceDocName=__nullptr,
		string* strOCREngineVersion=__nullptr,
		long nPage=-1,
		bool bLoadIntoThis=false);
	//----------------------------------------------------------------------------------------------
	// Save individual pages into numbered files in a zip archive
	// If bCompress is true then the default compression level (6) will be used to deflate the pages
	// If bAppend is true then the pages will be added to an existing zip file, else an existing file will be overwritten
	static void savePagesToArchive(const string& strOutputFile, IIUnknownVectorPtr ipPages, bool bCompress = true, bool bAppend = false);
	//----------------------------------------------------------------------------------------------
	// Split into pages and save as numbered files in a zip archive
	// The pages represented by this instance must not already exist in the archive
	// If bCompress is true then the default compression level (6) will be used to deflate the pages
	void appendToArchive(const string & strOutputFile, bool bCompress, bool bCheckForExistingPages);
	//----------------------------------------------------------------------------------------------
	static void saveToStorageObject(const string & strFullFileName, UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipSpatialString, bool bCompress, bool bClearDirty);
	//----------------------------------------------------------------------------------------------
};
