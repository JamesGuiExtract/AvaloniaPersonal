// SpatialStringSearcher.h : Declaration of the CSpatialStringSearcher

#pragma once

#include "resource.h"       // main symbols
#include "CPPLetter.h"

#include <vector>
#include <list>
#include <string>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CSpatialStringSearcher
class ATL_NO_VTABLE CSpatialStringSearcher : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CSpatialStringSearcher, &CLSID_SpatialStringSearcher>,
	public ISupportErrorInfo,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ISpatialStringSearcher, &IID_ISpatialStringSearcher, &LIBID_UCLID_RASTERANDOCRMGMTLib>,
	public IDispatchImpl<IManageableMemory, &IID_IManageableMemory, &LIBID_UCLID_COMUTILSLib>
{
public:
	CSpatialStringSearcher();
	~CSpatialStringSearcher();

DECLARE_REGISTRY_RESOURCEID(IDR_SPATIALSTRINGSEARCHER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

	void FinalRelease();

BEGIN_COM_MAP(CSpatialStringSearcher)
	COM_INTERFACE_ENTRY(ISpatialStringSearcher)
	COM_INTERFACE_ENTRY2(IDispatch,ISpatialStringSearcher)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(IManageableMemory)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ISpatialStringSearcher
	STDMETHOD(SetBoundaryResolution)(ESpatialEntity eResolution);
	STDMETHOD(SetIncludeDataOnBoundary)(VARIANT_BOOL bInclude);
	STDMETHOD(GetDataOutOfRegion)(ILongRectangle* ipRect, ISpatialString** ipReturnString);
	STDMETHOD(GetDataInRegion)(ILongRectangle *ipRect, VARIANT_BOOL bRotateRectanglePerOCR, 
		ISpatialString** ipReturnString);
	STDMETHOD(InitSpatialStringSearcher)(ISpatialString* pSpatialString,
		VARIANT_BOOL vbUseOriginalImageCoordinates);
	STDMETHOD(ExtendDataInRegion)(ILongRectangle *pRect, long lNumWordsToExtend, 
		VARIANT_BOOL vbExtendHeight, ISpatialString** ppFound);
	STDMETHOD(GetLeftWord)(ILongRectangle* ipRect, ISpatialString** ipReturnString);
	STDMETHOD(GetRightWord)(ILongRectangle* ipRect, ISpatialString** ipReturnString);
	STDMETHOD(SetUseMidpointsOnly)(VARIANT_BOOL newVal);
	STDMETHOD(ExcludeDataInRegion)(ILongRectangle *pRect);
	STDMETHOD(GetCharacterIndexesInRegion)(ILongRectangle *pRect, IVariantVector** ipReturnVal);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IManageableMemory
	STDMETHOD(raw_ReportMemoryUsage)(void);

	////////////
	// Enumerations
	////////////
	// Intersection Testing
	enum EIntersection
	{
		kNotIntersecting,
		kTouching,
		kOverlapping,
		kContained,
		kContains	
	};
	enum EEndOfFlags
	{
		kEOW = 0x01,
		kEOL = 0x02,
		kEOP = 0x04,
		kEOZ = 0x08
	};

private:

	////////////
	// Classes
	////////////
	class LocalEntity
	{
	public:
		LocalEntity(long lLeft = -1, long lTop = -1, long lRight = -1, long lBottom = -1);

		////////////
		// Methods
		////////////
		EIntersection intersect(const LocalEntity& rEnt);

		////////////
		// Variables
		////////////
		long m_lLeft, m_lRight, m_lTop, m_lBottom;

		// Indicates whether this entity should be excluded from search results.
		bool m_bExcluded;
	};
	// used internally by SpatialStringSearcher to represent the string 
	// LocalLetter basically just wraps ILetter
	class LocalLetter : public LocalEntity 
	{
	public:
		LocalLetter();

		LocalLetter(unsigned int uiLetter, unsigned int uiZone, unsigned int uiParagraph,
			unsigned int uiLine, unsigned int uiWord)
			: m_uiLetter(uiLetter),
			  m_uiZone(uiZone),
			  m_uiParagraph(uiParagraph),
			  m_uiLine(uiLine),
			  m_uiWord(uiWord)
		{};

		////////////
		// Methods
		////////////
		// This was for the std::sort method but is no longer needed
		bool operator< (const LocalLetter& l2);

		////////////
		// Variables
		////////////
		CPPLetter letter;
		// The index of the Letter in the spatial string the letter came from
		unsigned int m_uiLetter;
		// The index of the zone that this letter belonged to in the string it came from
		unsigned int m_uiZone;
		// The index of the zone that this letter belonged to in the string it came from
		unsigned int m_uiParagraph;
		// The index of the zone that this letter belonged to in the string it came from
		unsigned int m_uiLine;
		// The index of the word that this letter belonged to in the string it came from
		unsigned int m_uiWord;
		// tells whether this letter is the end of a Line(Zone, word... )
		long m_lEndFlags;
	};

	// represents a word with a start and end letter index 
	// from m_vecLetters
	class LocalWord : public LocalEntity 
	{
	public:
		LocalWord();
		////////////
		// Variables
		////////////
		// the starting and ending letter indexes of the word
		unsigned int m_uiStart, m_uiEnd;
	};

	// represents a Line with a start and end letter index 
	// from m_vecLetters
	class LocalLine : public LocalEntity 
	{
	public:
		LocalLine();
		////////////
		// Variables
		////////////
		// the starting and ending letter indexes of the line
		unsigned int m_uiStart, m_uiEnd;
	};

	// Represents one or more letters. May span lines.
	class LocalSubstring
	{
	public:
		LocalSubstring(unsigned int uiStartLetter, unsigned int uiEndLetter);

		// Expands the substring to the left by the specified number of words
		void expandLeft(long lWords, const vector<LocalLetter>& vecLetters, 
			const vector<LocalWord>& vecWords);

		// Expands the substring to the left by the specified number of words
		void expandRight(long lWords, const vector<LocalLetter>& vecLetters, 
			const vector<LocalWord>& vecWords);

		// true if other is adjacent or overlapping with the substring; false otherwise
		bool isConnectedTo(const LocalSubstring& other) const;

		// Expands the substring the minimum amount possible to incorporate other
		void combineWith(const LocalSubstring& other);

		////////////
		// Variables
		////////////
		// The starting and ending indexes of the substring in m_vecLetters
		unsigned int m_uiStartLetter, m_uiEndLetter;
	};

	////////////
	// Methods
	////////////
	// Throws an exception if this component is not licensed to run
	void validateLicense();

	// Clears everything in the structure except the settings
	void clear();
	
	// These two methods populate m_vecLetters and m_vecWords
	// from the data in m_ipSpatialString
	// create LocalLetters MUST be called before create local words
	// as it uses the data in m_vecLetters
	void createLocalLetters();
	void createLocalWords();
	void createLocalLines();

	// Indicates whether the specified intersection level meets the configured criteria.
	bool intersectionMeetsCriteria(EIntersection eIntersection);

	// This method returns a list of all the letters(indexes) in the specified region
	// it is used by GetDataInRegion and GetDataOutOfRegion
	void getUnsortedLettersInRegion(ILongRectanglePtr ipRect, vector<int> &pvecLetters);

	// This method rotates ipRect based on the orientation of m_ipSpatialString
	void rotateRectangle(ILongRectanglePtr ipRect);

	// Gets a vector of substrings that corresponds to the vector of letter indices
	void getLettersAsSubstrings(const vector<int>& vecLetters, 
		vector<LocalSubstring>& vecSubstrings);

	// Expands each substring by the specified number of words
	void expandSubstrings(vector<LocalSubstring>& vecSubstrings, long lNumWordsToExpand);

	// Combines any substrings that are adjacent or overlap
	void combineSubstrings(vector<LocalSubstring>& vecSubstrings);

	// Gets a vector of letter index that corresponds to the vector of substrings
	void getSubstringsAsLetters(const vector<LocalSubstring>& vecSubstrings, 
		vector<int>& vecLetters);

	// Inserts non spatial character at the end of letters
	void insertNonSpatialCharacter(char c, long flags, vector<CPPLetter>& vecLetters);

	UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr createStringFromLetterIndexes(
		const vector<int> &rveciLetters, bool bAdjustHeight=false);

	void addLocalLetter(vector<CPPLetter>& vecLetters, const LocalLetter& letter, 
		bool bAdjustHeight);

	////////////
	// Variables
	////////////
	// The spatial string that can be searched spatially (and its associated values)
	UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr m_ipSpatialString;
	string m_strSourceDocName;
	ILongToObjectMapPtr m_ipSpatialPageInfoMap;

	long m_lStringLeft;
	long m_lStringRight;
	long m_lStringTop;
	long m_lStringBottom;

	// if this is true features that lie on a defined boundary will count as 
	// being inside the boundary
	bool m_bIncludeDataOnBoundary;

	// If true, all searches will assume the specified coordinates are in image coordinates; if
	// false all searches will be made assuming the specified coordinates are in OCR coordinates.
	bool m_bUseOriginalImageCoordinates;

	// If true, only the midpoint of the characters/words/lines will be tested for
	// inclusion/exclusion rather than using the entire area.
	bool m_bUseMidpointsOnly;

	// determines the type of feature that will be checked 
	// for boundary intersection
	ESpatialEntity m_eBoundaryResolution;

	// Contains all the Letters
	vector<LocalLetter> m_vecLetters;
	// Contains all tehe words
	vector<LocalWord> m_vecWords;
	// Contains all the lines
	vector<LocalLine> m_vecLines;

	// Allows reporting of memory usage to the garabage collector when being referenced by managed
	// code.
	IMemoryManagerPtr m_ipMemoryManager;
};
