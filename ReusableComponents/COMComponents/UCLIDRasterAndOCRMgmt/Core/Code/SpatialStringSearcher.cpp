// SpatialStringSearcher.cpp : Implementation of CSpatialStringSearcher
#include "stdafx.h"
#include "UCLIDRasterAndOCRMgmt.h"
#include "SpatialStringSearcher.h"
#include "CPPLetter.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <ComponentLicenseIDs.h>
#include <ComUtils.h>

#include <map>

using namespace std;

//-------------------------------------------------------------------------------------------------
// LocalEntity
//-------------------------------------------------------------------------------------------------
CSpatialStringSearcher::LocalEntity::LocalEntity(long lLeft, long lTop, long lRight, long lBottom)
: m_lLeft(lLeft),
m_lTop(lTop),
m_lRight(lRight),
m_lBottom(lBottom)
{
}
//-------------------------------------------------------------------------------------------------
CSpatialStringSearcher::EIntersection CSpatialStringSearcher::LocalEntity::intersect(const LocalEntity& rEnt)
{
	if ((m_lLeft >= rEnt.m_lRight) || 
		(m_lTop >= rEnt.m_lBottom) ||
		(m_lRight <= rEnt.m_lLeft) ||
		(m_lBottom <= rEnt.m_lTop))
	{
		return kNotIntersecting;
	}
	if(	rEnt.m_lRight <= m_lRight &&
		rEnt.m_lLeft >= m_lLeft &&
		rEnt.m_lTop >= m_lTop &&
		rEnt.m_lBottom <= m_lBottom)
	{
		return kContains;
	}
	if(	m_lRight <= rEnt.m_lRight &&
		m_lLeft >= rEnt.m_lLeft &&
		m_lTop >= rEnt.m_lTop &&
		m_lBottom <= rEnt.m_lBottom)
	{
		return kContained;
	}
	
	return kIntersecting;
}
//-------------------------------------------------------------------------------------------------
// LocalLetter
//-------------------------------------------------------------------------------------------------
CSpatialStringSearcher::LocalLetter::LocalLetter() : LocalEntity(),

	m_uiLetter(0), m_uiWord(0), m_uiLine(0), m_uiParagraph(0), m_uiZone(0),
	m_bIsSpatial(false),
	m_lEndFlags(0)
{
}
//-------------------------------------------------------------------------------------------------
bool CSpatialStringSearcher::LocalLetter::operator<(const LocalLetter& l2)
{
	if (m_uiLetter < l2.m_uiLetter)
	{
		return true;
	}
	else
	{
		return false;
	}
}

//-------------------------------------------------------------------------------------------------
// LocalWord
//-------------------------------------------------------------------------------------------------
CSpatialStringSearcher::LocalWord::LocalWord() : LocalEntity(),
	m_uiStart(0), m_uiEnd(0)
{
}
	
//-------------------------------------------------------------------------------------------------
// LocalLine
//-------------------------------------------------------------------------------------------------
CSpatialStringSearcher::LocalLine::LocalLine() :  LocalEntity(),
	m_uiStart(0), m_uiEnd(0)
{
}

//-------------------------------------------------------------------------------------------------
// LocalSubstring
//-------------------------------------------------------------------------------------------------
CSpatialStringSearcher::LocalSubstring::LocalSubstring(unsigned int uiStartWord, 
													   unsigned int uiEndWord) 
	: m_uiStartWord(uiStartWord), m_uiEndWord(uiEndWord)
{
}
//-------------------------------------------------------------------------------------------------
void CSpatialStringSearcher::LocalSubstring::expandLeft(long lWords)
{
	long lStartWord = (long)(m_uiStartWord) - lWords;
	if (lStartWord < 0)
	{
		lStartWord = 0;
	}

	m_uiStartWord = lStartWord;
}
//-------------------------------------------------------------------------------------------------
void CSpatialStringSearcher::LocalSubstring::expandRight(long lWords, unsigned int uiMaxWord)
{
	unsigned int uiEndWord = m_uiEndWord + lWords;
	if (uiEndWord > uiMaxWord)
	{
		uiEndWord = uiMaxWord;
	}

	m_uiEndWord = uiEndWord;
}
//-------------------------------------------------------------------------------------------------
bool CSpatialStringSearcher::LocalSubstring::isConnectedTo(const LocalSubstring& other) const
{
	return other.m_uiEndWord + 1 >= m_uiStartWord && other.m_uiStartWord <= m_uiEndWord + 1;
}
//-------------------------------------------------------------------------------------------------
void CSpatialStringSearcher::LocalSubstring::combineWith(const LocalSubstring& other)
{
	m_uiStartWord = min(m_uiStartWord, other.m_uiStartWord);
	m_uiEndWord = max(m_uiEndWord, other.m_uiEndWord);
}

//-------------------------------------------------------------------------------------------------
// CSpatialStringSearcher
//-------------------------------------------------------------------------------------------------
CSpatialStringSearcher::CSpatialStringSearcher()
{
	// Set the default settings
	m_bIncludeDataOnBoundary = true; 
	m_eBoundaryResolution = kCharacter; 
	
	// Initialize all the values 
	clear();
}
//-------------------------------------------------------------------------------------------------
CSpatialStringSearcher::~CSpatialStringSearcher()
{
	try
	{
		clear();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16540");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialStringSearcher::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ISpatialStringSearcher,
		&IID_ILicensedComponent
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ISpatialStringSearcher
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialStringSearcher::InitSpatialStringSearcher(ISpatialString* pSpatialString)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Ensure the new spatial string is not NULL
		UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipSpatialString(pSpatialString);
		ASSERT_ARGUMENT("ELI25663", ipSpatialString != NULL);

		validateLicense();

		// Clear out the information from any previously set string
		clear();

		// assign our string to the new string
		m_ipSpatialString = ipSpatialString;
		m_strSourceDocName = asString(ipSpatialString->SourceDocName);

		// confirm that the string is spatial
		if (m_ipSpatialString->GetMode() == kSpatialMode)
		{
			// Get the spatial page info map
			m_ipSpatialPageInfoMap = ipSpatialString->SpatialPageInfos;
			createLocalLetters();
			createLocalWords();
			createLocalLines();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07789");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialStringSearcher::GetDataInRegion(ILongRectangle *ipRect, 
													 VARIANT_BOOL bRotateRectanglePerOCR, 
													 ISpatialString **ipReturnString)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI25606", ipReturnString != NULL);

		validateLicense();

		// Rotate the rectangle based on orientation of recognized text
		if (bRotateRectanglePerOCR == VARIANT_TRUE)
		{
			rotateRectangle( ipRect );
		}

		// Get a vector of all the letters that fall in the region
		vector<int> inLetters;		
		getUnsortedLettersInRegion(ipRect, inLetters);

		// sort the letters from top to bottom left to right 
		// This means that we are basically ignoring zone information
		sort(inLetters.begin(), inLetters.end());

		// Build the SpatialString
		UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipNewStr =
			createStringFromLetterIndexes(inLetters);
		ASSERT_RESOURCE_ALLOCATION("ELI19896", ipNewStr != NULL);

		// Return the string
		*ipReturnString = (ISpatialString*) ipNewStr.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10636");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialStringSearcher::GetDataOutOfRegion(ILongRectangle *ipRect, ISpatialString **ipReturnString)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI25890", ipRect != NULL);
		ASSERT_ARGUMENT("ELI25891", ipReturnString != NULL);

		validateLicense();

		// Get a vector of all the letters that fall in the region and should therefore be excluded
		// from the spatial string we return
		vector<int> inLetters;		
		getUnsortedLettersInRegion(ipRect, inLetters);
		// sort the letters so that the letter that came first in the spatial string
		// comes first in the vector and so on
		sort(inLetters.begin(), inLetters.end());
	
		// The Vector of spatial letters indexes that lie outside the region
		vector<int> outLetters;

		// Add to out Letters every spatial letter that is not inside the region
		// (i.e. not contained in inLetters)
		// this keeps track of where we are in inLetters
		unsigned int iExclude = 0;
		for (unsigned int ui = 0; ui < m_vecLetters.size(); ui++)
		{
			// we only add spatial letters
			if (!m_vecLetters[ui].m_bIsSpatial)
			{
				continue;
			}

			// If i is in the region we will exclude it
			if (!inLetters.empty() && (iExclude < inLetters.size()) && (inLetters[iExclude] == ui)) 
			{
				iExclude++;
				continue;
			}
			outLetters.push_back(ui);
		}

		// Build the SpatialString
		UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipNewStr =
			createStringFromLetterIndexes(outLetters);
		ASSERT_RESOURCE_ALLOCATION("ELI25892", ipNewStr != NULL);

		*ipReturnString = (ISpatialString*) ipNewStr.Detach();

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07790");
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialStringSearcher::ExtendDataInRegion(ILongRectangle *pRect, 
	long lNumWordsToExtend, VARIANT_BOOL vbExtendHeight, ISpatialString** ppFound)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI29540", ppFound != NULL);

		validateLicense();

		ILongRectanglePtr ipRect(pRect);
		ASSERT_RESOURCE_ALLOCATION("ELI29541", ipRect != NULL);

		// Get a vector of all the letters that fall in the region
		vector<int> vecLetters;		
		getUnsortedLettersInRegion(ipRect, vecLetters);

		// Convert the vector of letters to a vector of substrings
		vector<LocalSubstring> vecSubstrings;
		getLettersAsSubstrings(vecLetters, vecSubstrings);

		// Expand each substring if necessary
		if (lNumWordsToExtend > 0)
		{
			expandSubstrings(vecSubstrings, lNumWordsToExtend);
		}

		// Combine substrings that overlap
		combineSubstrings(vecSubstrings);

		// Convert the substrings to a vector of letters
		getSubstringsAsLetters(vecSubstrings, vecLetters);

		// sort the letters from top to bottom left to right 
		// This means that we are basically ignoring zone information
		sort(vecLetters.begin(), vecLetters.end());

		// Build the SpatialString
		UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipFound =	
			createStringFromLetterIndexes(vecLetters);
		ASSERT_RESOURCE_ALLOCATION("ELI29542", ipFound != NULL);

		// Return the string
		*ppFound = (ISpatialString*) ipFound.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29539");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialStringSearcher::SetIncludeDataOnBoundary(VARIANT_BOOL bInclude)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		if (bInclude == VARIANT_TRUE)
		{
			m_bIncludeDataOnBoundary = true;
		}
		else
		{
			m_bIncludeDataOnBoundary = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07791");
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialStringSearcher::SetBoundaryResolution(ESpatialEntity eResolution)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();
		m_eBoundaryResolution = eResolution;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07792");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialStringSearcher::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	try
	{
		validateLicense();

		// If validateLicense doesn't throw any exception, then pbValue is true
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CSpatialStringSearcher::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnEXTRACT_CORE_OBJECTS;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI07981", "Spatial String Searcher" );
}
//-------------------------------------------------------------------------------------------------
void CSpatialStringSearcher::clear()
{
	// Empty all the local data stuctures
	m_vecLetters.clear();
	m_vecWords.clear();
	m_vecLines.clear();

	// Release our pointer to the spatial string and its associated data items
	m_ipSpatialPageInfoMap = NULL;
	m_ipSpatialString = NULL;
	m_strSourceDocName = "";

	// Set the document boundaries to 0
	m_lStringLeft = -1;
	m_lStringRight = -1;
	m_lStringTop = -1;
	m_lStringBottom = -1; 
}
//-------------------------------------------------------------------------------------------------
void CSpatialStringSearcher::createLocalLetters()
{
	// Keep track of the index of the current word, line... as
	// local letters need that information
	unsigned int iCurrWord = 0;
	unsigned int iCurrLine = 0;
	unsigned int iCurrParagraph = 0;
	unsigned int iCurrZone = 0;

	// populate the list of local letters and add them to the spatial data structure
	long numLetters;
	CPPLetter* pLetters = NULL;
	m_ipSpatialString->GetOCRImageLetterArray(&numLetters, (void**)&pLetters);
	ASSERT_RESOURCE_ALLOCATION("ELI25990", pLetters != NULL);
	for (long i = 0; i < numLetters; i++)
	{
		const CPPLetter& letter = pLetters[i];

		m_vecLetters.push_back(LocalLetter());
		LocalLetter& rLocalLetter = m_vecLetters[i];
		rLocalLetter.letter = letter;
		
		// Set the letters properties
		rLocalLetter.m_uiLetter = i;
		rLocalLetter.m_uiWord = iCurrWord;
		rLocalLetter.m_uiLine = iCurrLine;
		rLocalLetter.m_uiParagraph = iCurrParagraph;
		rLocalLetter.m_uiZone = iCurrZone;

		// if this is the last letter in a word
		if (m_ipSpatialString->GetIsEndOfWord(i) == VARIANT_TRUE)
		{
			iCurrWord++;
			rLocalLetter.m_lEndFlags |= kEOW;
		}	
		// if this is the last letter in a Line
		if (m_ipSpatialString->GetIsEndOfLine(i) == VARIANT_TRUE)
		{
			iCurrLine++;
			rLocalLetter.m_lEndFlags |= kEOL;
		}
		// if this is the last letter in a Paragraph
		if (letter.m_bIsEndOfParagraph)
		{
			iCurrParagraph++;
			rLocalLetter.m_lEndFlags |= kEOP;
		}
		// if this is the last letter in a Zone
		if (letter.m_bIsEndOfZone)
		{
			iCurrZone++;
			rLocalLetter.m_lEndFlags |= kEOZ;
		}

		//ONLY SPATIAL LETTERS MAY PROCEED BEYOND THIS POINT
		if (!letter.m_bIsSpatial)
		{
			rLocalLetter.m_bIsSpatial = false;
			continue;
		}
		// Duh
		rLocalLetter.m_bIsSpatial = true;

		// Set the bounds
		rLocalLetter.m_lLeft = letter.m_usLeft;
		rLocalLetter.m_lRight = letter.m_usRight;
		rLocalLetter.m_lTop = letter.m_usTop;
		rLocalLetter.m_lBottom = letter.m_usBottom;

		// Create the document boundaries
		if (rLocalLetter.m_lLeft < m_lStringLeft || m_lStringLeft < 0)
		{
			m_lStringLeft = rLocalLetter.m_lLeft;
		}
		if (rLocalLetter.m_lRight > m_lStringRight || m_lStringRight < 0)
		{
			m_lStringRight = rLocalLetter.m_lRight;
		}
		if (rLocalLetter.m_lTop < m_lStringTop || m_lStringTop < 0)
		{
			m_lStringTop = rLocalLetter.m_lTop;
		}
		if (rLocalLetter.m_lBottom > m_lStringBottom || m_lStringBottom < 0)
		{
			m_lStringBottom = rLocalLetter.m_lBottom;
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CSpatialStringSearcher::createLocalWords()
{
	// Maintain a current word that will be added when an
	// IsEndOfWord flag is reached
	LocalWord currWord;

	// These are set to -1 so we know they have not been initialized
	currWord.m_lLeft = currWord.m_lRight = currWord.m_lTop = currWord.m_lBottom = -1;
	currWord.m_uiStart = 0;
	currWord.m_uiEnd = 0;

	// When we add a word to a spatial data structure don't want 
	// the word padded on either end with whitespace 
	// when this flag is true the start of the current word(above) will
	// be set when the next spatial character is reached
	bool setWordStart = true;

	for (unsigned int ui = 0; ui < m_vecLetters.size(); ui++)
	{
		LocalLetter& rLocalLetter = m_vecLetters[ui];

		// Ignore non-spatial characters
		if (!rLocalLetter.m_bIsSpatial)
		{
			continue;
		}

		//ONLY SPATIAL LETTERS MAY PROCEED BEYOND THIS POINT
		if (setWordStart)
		{
			currWord.m_uiStart = ui;
			setWordStart = false;
		}

		// update the bounds of the current word
		if (rLocalLetter.m_lLeft < currWord.m_lLeft || currWord.m_lLeft < 0)
		{
			currWord.m_lLeft = rLocalLetter.m_lLeft;
		}
		if (rLocalLetter.m_lRight > currWord.m_lRight || currWord.m_lRight < 0)
		{
			currWord.m_lRight = rLocalLetter.m_lRight;
		}
		if (rLocalLetter.m_lTop < currWord.m_lTop || currWord.m_lTop < 0)
		{
			currWord.m_lTop = rLocalLetter.m_lTop;
		}
		if (rLocalLetter.m_lBottom > currWord.m_lBottom || currWord.m_lBottom < 0)
		{
			currWord.m_lBottom = rLocalLetter.m_lBottom;
		}

		// if this is the last letter in a word
		if (rLocalLetter.m_lEndFlags & kEOW)
		{
			// set the end of the current word
			currWord.m_uiEnd = ui + 1;
			//add the word to the vector
			m_vecWords.push_back(currWord);
			// Reset our currWord variable to the appropriate defaults
			currWord.m_lLeft = currWord.m_lRight = currWord.m_lTop = currWord.m_lBottom = -1;				
			// set its starting letter and ending letter
			// both of these values should be overwritten
			currWord.m_uiStart = ui + 1;
			currWord.m_uiEnd = ui + 1;

			// This means that the starting letter of this
			// word should be set as the next spatial character
			setWordStart = true;
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CSpatialStringSearcher::createLocalLines()
{
	// Maintain a current word that will be added when an
	// end of line
	LocalLine currLine;
	
	// These are set to -1 so we know they have not been initialized
	currLine.m_lLeft = currLine.m_lRight = currLine.m_lTop = currLine.m_lBottom = -1;
	currLine.m_uiStart = 0;
	currLine.m_uiEnd = 0;

	// When we add a line to a spatial data structure don't want 
	// the line padded on either end with whitespace 
	// when this flag is true the start of the current line(above) will
	// be set when the next spatial character is reached
	bool setLineStart = true;

	for (unsigned int ui = 0; ui < m_vecLetters.size(); ui++)
	{
		LocalLetter& rLocalLetter = m_vecLetters[ui];

		// Ignore non-spatial characters
		if (!rLocalLetter.m_bIsSpatial)
		{
			continue;
		}

		//ONLY SPATIAL LETTERS MAY PROCEED BEYOND THIS POINT
		if (setLineStart)
		{
			currLine.m_uiStart = ui;
			setLineStart = false;
		}

		// update the bounds of the current word
		if (rLocalLetter.m_lLeft < currLine.m_lLeft || currLine.m_lLeft < 0)
		{
			currLine.m_lLeft = rLocalLetter.m_lLeft;
		}
		if (rLocalLetter.m_lRight > currLine.m_lRight || currLine.m_lRight < 0)
		{
			currLine.m_lRight = rLocalLetter.m_lRight;
		}
		if (rLocalLetter.m_lTop < currLine.m_lTop || currLine.m_lTop < 0)
		{
			currLine.m_lTop = rLocalLetter.m_lTop;
		}
		if (rLocalLetter.m_lBottom > currLine.m_lBottom || currLine.m_lBottom < 0)
		{
			currLine.m_lBottom = rLocalLetter.m_lBottom;
		}

		// if this is the last letter in a line
		if (rLocalLetter.m_lEndFlags & kEOL)
		{
			// set the end of the current word
			currLine.m_uiEnd = ui + 1;
			//add the word to the vector
			m_vecLines.push_back(currLine);
			// Reset our currLine variable to the appropriate defaults
			currLine.m_lLeft = currLine.m_lRight = currLine.m_lTop = currLine.m_lBottom = -1;
			// set its starting letter and ending letter
			// both of these values should be overwritten
			currLine.m_uiStart = ui + 1;
			currLine.m_uiEnd = ui + 1;

			// This means that the starting letter of this
			// word should be set as the next spatial character
			setLineStart = true;
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CSpatialStringSearcher::getUnsortedLettersInRegion(ILongRectanglePtr ipRect, 
														vector<int> &rvecLetters)
{
	try
	{
		ASSERT_ARGUMENT("ELI25615", ipRect != NULL);
		long lLeft, lTop, lRight, lBottom;
		ipRect->GetBounds(&lLeft, &lTop, &lRight, &lBottom);
		LocalEntity region(lLeft, lTop, lRight, lBottom);

		// Set left bound for region
		if (region.m_lLeft < 0)
		{
			region.m_lLeft = m_lStringLeft;
		}

		// Set right bound for region
		if (region.m_lRight < 0)
		{
			region.m_lRight = m_lStringRight;
		}

		// Set top bound for region
		if (region.m_lTop < 0)
		{
			region.m_lTop = m_lStringTop;
		}

		// Set bottom bound for region
		if (region.m_lBottom < 0)
		{
			region.m_lBottom = m_lStringBottom; 
		}

		if (m_eBoundaryResolution == kCharacter)
		{
			for (unsigned long ui = 0; ui < m_vecLetters.size(); ui++)
			{
				LocalLetter& rLetter = m_vecLetters[ui];
				if (!rLetter.m_bIsSpatial)
				{
					continue;
				}
				// Test the rLetter against the region
				EIntersection eIntersection = region.intersect(rLetter);

				if( (m_bIncludeDataOnBoundary && eIntersection > kNotIntersecting) ||
					(!m_bIncludeDataOnBoundary && eIntersection == kContains) )
				{
					rvecLetters.push_back(ui);
				}
			}
		}
		else if (m_eBoundaryResolution == kWord)
		{
			for (unsigned int ui = 0; ui < m_vecWords.size(); ui++)
			{	
				LocalWord& rWord = m_vecWords[ui];

				// test the word against the region
				EIntersection eIntersection =  region.intersect(rWord);

				if ((m_bIncludeDataOnBoundary && eIntersection > kNotIntersecting) ||
					(!m_bIncludeDataOnBoundary && eIntersection == kContains) )
				{
					for (unsigned int uiLetter = rWord.m_uiStart; uiLetter < rWord.m_uiEnd; uiLetter++)
					{
						LocalLetter& rLetter = m_vecLetters[uiLetter];
						if (!rLetter.m_bIsSpatial)
						{
							continue;
						}
						rvecLetters.push_back(uiLetter);
					}
				}
			}
		}
		else if (m_eBoundaryResolution == kLine)
		{
			for (unsigned int ui = 0; ui < m_vecLines.size(); ui++)
			{
				LocalLine& rLine = m_vecLines[ui];

				// test the word against the region
				EIntersection eIntersection =  region.intersect(rLine);

				if ((m_bIncludeDataOnBoundary && eIntersection > kNotIntersecting) ||
					(!m_bIncludeDataOnBoundary && eIntersection == kContains) )
				{
					for (unsigned int uiLetter = rLine.m_uiStart;
						uiLetter < rLine.m_uiEnd; uiLetter++)
					{
						LocalLetter& rLetter = m_vecLetters[uiLetter];
						if (!rLetter.m_bIsSpatial)
						{
							continue;
						}
						rvecLetters.push_back(uiLetter);
					}
				}
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25616");
}
//-------------------------------------------------------------------------------------------------
void CSpatialStringSearcher::getLettersAsSubstrings(const vector<int>& vecLetters, 
													vector<LocalSubstring>& vecSubstrings)
{
	// Clear the vector of substrings to begin
	vecSubstrings.clear();

	// If there are no letters, there are no substrings
	if (vecLetters.size() <= 0)
	{
		return;
	}

	// Iterate through each letter looking for substrings
	unsigned int uiStartSubstring = m_vecLetters[vecLetters[0]].m_uiWord;
	unsigned int uiEndSubstring = uiStartSubstring;
	for	(unsigned int i = 1; i < vecLetters.size(); i++)
	{
		// Has a new word been encountered?
		unsigned int uiCurrentWord = m_vecLetters[vecLetters[i]].m_uiWord;
		if (uiEndSubstring != uiCurrentWord)
		{
			if (uiEndSubstring + 1 == uiCurrentWord)
			{
				// The new word is the next word, remember this word and keep looking
				uiEndSubstring = uiCurrentWord;
			}
			else
			{
				// The new word was not the next word, this is the end of a substring
				LocalSubstring substring(uiStartSubstring, uiEndSubstring);
				vecSubstrings.push_back(substring);

				// Start a new substring
				uiStartSubstring = uiCurrentWord;
				uiEndSubstring = uiCurrentWord;
			}
		}
	}

	// Add the last substring
	LocalSubstring substring(uiStartSubstring, uiEndSubstring);
	vecSubstrings.push_back(substring);
}
//-------------------------------------------------------------------------------------------------
void CSpatialStringSearcher::expandSubstrings(vector<LocalSubstring>& vecSubstrings, 
											  long lNumWordsToExpand)
{
	unsigned int uiMaxWord = m_vecWords.size() - 1;
	for (unsigned int i = 0; i < vecSubstrings.size(); i++)
	{
		LocalSubstring& substring = vecSubstrings[i];
		substring.expandLeft(lNumWordsToExpand);
		substring.expandRight(lNumWordsToExpand, uiMaxWord);
	}
}
//-------------------------------------------------------------------------------------------------
void CSpatialStringSearcher::combineSubstrings(vector<LocalSubstring>& vecSubstrings)
{
	// Need at least two substrings to combine
	if (vecSubstrings.size() < 2)
	{
		return;
	}

	vector<LocalSubstring>::iterator iterPrevious = vecSubstrings.begin();
	vector<LocalSubstring>::iterator iterCurrent = iterPrevious + 1;
	while (iterCurrent < vecSubstrings.end())
	{
		LocalSubstring& current = *iterCurrent;
		if (iterPrevious->isConnectedTo(current))
		{
			iterPrevious->combineWith(current);
			iterCurrent = vecSubstrings.erase(iterCurrent);
			continue;
		}

		iterPrevious = iterCurrent;
		iterCurrent++;
	}
}
//-------------------------------------------------------------------------------------------------
void CSpatialStringSearcher::getSubstringsAsLetters(const vector<LocalSubstring>& vecSubstrings, 
													vector<int>& vecLetters)
{
	vecLetters.clear();

	if (vecSubstrings.size() <= 0)
	{
		return;
	}

	// Iterate through each substrings
	for (unsigned int i = 0; i < vecSubstrings.size(); i++)
	{
		const LocalSubstring& substring = vecSubstrings[i];

		// Iterate through each word of the substring
		for (unsigned int j = substring.m_uiStartWord; j <= substring.m_uiEndWord; j++)
		{
			const LocalWord& word = m_vecWords[j];

			// Iterate through each letter of the word
			for (unsigned int k = word.m_uiStart; k < word.m_uiEnd; k++)
			{
				const LocalLetter& letter = m_vecLetters[k];

				// Add the letter if it is spatial
				if (letter.m_bIsSpatial)
				{
					vecLetters.push_back(k);
				}
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CSpatialStringSearcher::insertNonSpatialCharacter(char c, long flags, vector<CPPLetter>& vecLetters)
{
	CPPLetter letter;
	letter.m_usGuess1 = letter.m_usGuess2 = letter.m_usGuess3 = c;

	if (flags & kEOP)
	{
		letter.m_bIsEndOfParagraph = false;
	}
	if (flags & kEOZ)
	{
		letter.m_bIsEndOfZone = false;
	}
	letter.m_bIsSpatial = false;

	vecLetters.push_back(letter);
}
//-------------------------------------------------------------------------------------------------
void CSpatialStringSearcher::rotateRectangle(ILongRectanglePtr ipRect)
{
	// Validate smart pointer
	ASSERT_ARGUMENT("ELI16739", ipRect != NULL);

	if (m_ipSpatialString->HasSpatialInfo())
	{
		// Get first page number of the SpatialString
		// and associated SpatialPageInfo
		long nPageNum = m_ipSpatialString->GetFirstPageNumber();
		UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr ipInfo = m_ipSpatialString->GetPageInfo(nPageNum);
		ASSERT_RESOURCE_ALLOCATION("ELI16740", ipInfo != NULL);

		// Get the page info
		UCLID_RASTERANDOCRMGMTLib::EOrientation eOrientation;
		long nWidth(-1), nHeight(-1);
		double dDeskew(0.0);
		ipInfo->GetPageInfo(&nWidth, &nHeight, &eOrientation, &dDeskew);

		// Rotate the rectangle based on orientation of SpatialString data member
		switch (eOrientation)
		{
		case kRotNone:
			// Do nothing to the rectangle
			break;

		case kRotLeft:
			// Rotate the rectangle 90 degrees counterclockwise
			ipRect->Rotate( nWidth, nHeight, -90 );
			break;

		case kRotDown:
			// Rotate the rectangle 180 degrees
			ipRect->Rotate( nWidth, nHeight, 180 );
			break;

		case kRotRight:
			// Rotate the rectangle 90 degrees clockwise
			ipRect->Rotate( nWidth, nHeight, 90 );
			break;

		default:
			{
				// Unsupported orientation
				UCLIDException ue("ELI16741", "Unsupported orientation!");
				ue.addDebugInfo("Orientation", eOrientation);
				throw ue;
			}
			break;
		}
	}
}
//-------------------------------------------------------------------------------------------------
UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr CSpatialStringSearcher::createStringFromLetterIndexes(
	const vector<int> &rveciLetters)
{
	// Create a new spatial string that will contain all the data in the region
	UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipNewStr(CLSID_SpatialString);
	ASSERT_RESOURCE_ALLOCATION("ELI08026", ipNewStr != NULL);

	// Create a vector of letters that we will use to fill our new spatial string
	vector<CPPLetter> vecNewLetters;

	// Keep track of the number of ILetters in ipNewLetters
	int numNewLetters = 0;
	
	// These are used to track when we cross line, word... boundaries
	// so we can appropriately set end of word flags and so on
	long currWord = 0;
	long currLine = 0;
	long currParagraph = 0;
	long currZone = 0;

	// initialize the current boundary variables to be equal to the first letters information
	if ((rveciLetters.size() > 0) && (m_vecLetters.size() > (unsigned int)rveciLetters[0]))
	{
		LocalLetter& rFirstLetter = m_vecLetters[rveciLetters[0]];

		currWord = rFirstLetter.m_uiWord;
		currLine = rFirstLetter.m_uiLine;
		currParagraph = rFirstLetter.m_uiParagraph;
		currZone = rFirstLetter.m_uiZone;
	}

	// The index in ipNewLetters of ipLastSpatialLetter
	int iLastSpatialLetter = -1;
	char cLastLetter = '\0';

	unsigned int ui = 0;

	for (; ui < rveciLetters.size(); ui++)
	{
		LocalLetter& rCurrLocalLetter = m_vecLetters[rveciLetters[ui]];

		// Get the upcoming letter
		char cNewLetter = (char)rCurrLocalLetter.letter.m_usGuess1;

		// check for new word breaks
		if (currWord != rCurrLocalLetter.m_uiWord)
		{
			// Add a space if one won't already be added
			if (!isWhitespaceChar(cNewLetter) && !isWhitespaceChar(cLastLetter))
			{
				insertNonSpatialCharacter(' ', 0, vecNewLetters);
			}
			currWord = rCurrLocalLetter.m_uiWord;
		}
		// check for new line breaks
		if (currLine != rCurrLocalLetter.m_uiLine)
		{
			// If we have created a line break where there was not one
			// before we have to insert newLine characters
			if (!(cNewLetter == '\n') && !(cLastLetter == '\n'))
			{
				insertNonSpatialCharacter('\r', 0, vecNewLetters);
				insertNonSpatialCharacter('\n', 0, vecNewLetters);
			}
			currLine = rCurrLocalLetter.m_uiLine;
		}
		// Check for new paragraph breaks
		if (currParagraph != rCurrLocalLetter.m_uiParagraph)
		{
			// Paragraphs have an additional newLine after them
			// in addition to one for the line break
			if ((iLastSpatialLetter != -1) && 
				(!vecNewLetters[iLastSpatialLetter].m_bIsEndOfParagraph))
			{
				// Set the last letter to be an end of paragraph
				vecNewLetters[iLastSpatialLetter].m_bIsEndOfParagraph = true;

				// insert a new line
				insertNonSpatialCharacter('\r', 0, vecNewLetters);
				insertNonSpatialCharacter('\n', 0, vecNewLetters);
			}
			currParagraph = rCurrLocalLetter.m_uiParagraph;
		}
		// Check for new zone breaks
		if (currZone != rCurrLocalLetter.m_uiZone)
		{
			if ((iLastSpatialLetter != -1) && 
				(!vecNewLetters[iLastSpatialLetter].m_bIsEndOfZone))
			{
				// Set the last letter to be an end of zone
				vecNewLetters[iLastSpatialLetter].m_bIsEndOfZone = true;
			}
			currZone = rCurrLocalLetter.m_uiZone;
		}

		// Add the current letter to our vector (our string)
		vecNewLetters.push_back(rCurrLocalLetter.letter);

		// A new spatial letter has been added and now it is the 
		// last spatialLetter
		iLastSpatialLetter = vecNewLetters.size() - 1;

		cLastLetter = (char) vecNewLetters[iLastSpatialLetter].m_usGuess1;

		// Non-spatial characters like Whitespace characters 
		// are not in the spatial data structure so they are not
		// in our rveciLetters vector.  Since we want to retain them 
		// we add all non-spatial characters trailing any spatial character
		// in our region
		for (unsigned int uiLetter = rveciLetters[ui] + 1; uiLetter < m_vecLetters.size(); uiLetter++)
		{
			// Stop once a spatial character is reached
			if (m_vecLetters[uiLetter].m_bIsSpatial)
			{
				break;
			}
			// If the letter isn't spatial add it
			vecNewLetters.push_back(m_vecLetters[uiLetter].letter);
			cLastLetter = (char) m_vecLetters[uiLetter].letter.m_usGuess1;

			// update the word index for non-spatial letters [P16 #2849]
			currWord = m_vecLetters[uiLetter].m_uiWord;
		}
	}

	// Add our letters to the string
	if (vecNewLetters.size() > 0)
	{
		vecNewLetters[iLastSpatialLetter].m_bIsEndOfParagraph = true;
		ipNewStr->CreateFromLetterArray(vecNewLetters.size(), &(vecNewLetters[0]),
			m_strSourceDocName.c_str(), m_ipSpatialPageInfoMap);
	}
	else
	{
		ipNewStr->SourceDocName = m_strSourceDocName.c_str();
		ipNewStr->ReplaceAndDowngradeToNonSpatial("");
	}
	
	return ipNewStr;
}
//-------------------------------------------------------------------------------------------------
