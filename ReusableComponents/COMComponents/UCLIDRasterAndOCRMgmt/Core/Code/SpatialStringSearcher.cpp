// SpatialStringSearcher.cpp : Implementation of CSpatialStringSearcher
#include "stdafx.h"
#include "UCLIDRasterAndOCRMgmt.h"
#include "SpatialStringSearcher.h"
#include "SpatialPageInfo.h"
#include "CPPLetter.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <ComponentLicenseIDs.h>
#include <ComUtils.h>
#include <Random.h>

#include <map>

using namespace std;

//-------------------------------------------------------------------------------------------------
// LocalEntity
//-------------------------------------------------------------------------------------------------
CSpatialStringSearcher::LocalEntity::LocalEntity(long lLeft, long lTop, long lRight, long lBottom)
: m_lLeft(lLeft),
m_lTop(lTop),
m_lRight(lRight),
m_lBottom(lBottom),
m_bExcluded(false)
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
	else if(rEnt.m_lRight <= m_lRight &&
		rEnt.m_lLeft >= m_lLeft &&
		rEnt.m_lTop >= m_lTop &&
		rEnt.m_lBottom <= m_lBottom)
	{
		return kContains;
	}
	else if(m_lRight <= rEnt.m_lRight &&
		m_lLeft >= rEnt.m_lLeft &&
		m_lTop >= rEnt.m_lTop &&
		m_lBottom <= rEnt.m_lBottom)
	{
		return kContained;
	}
	else
	{
		CPoint ptMiddle = CPoint((rEnt.m_lLeft + rEnt.m_lRight) / 2, (rEnt.m_lTop + rEnt.m_lBottom) / 2);

		if (ptMiddle.x >= m_lLeft && ptMiddle.x <= m_lRight &&
			ptMiddle.y >= m_lTop && ptMiddle.y <= m_lBottom)
		{
			return kOverlapping;
		}
	}
	
	return kTouching;
}
//-------------------------------------------------------------------------------------------------
// LocalLetter
//-------------------------------------------------------------------------------------------------
CSpatialStringSearcher::LocalLetter::LocalLetter() : LocalEntity(),
	m_uiLetter(0), 
	m_uiWord(0), 
	m_uiLine(0), 
	m_uiParagraph(0), 
	m_uiZone(0),
	m_lEndFlags(0)
{
}
//-------------------------------------------------------------------------------------------------
bool CSpatialStringSearcher::LocalLetter::operator<(const LocalLetter& l2)
{
	return m_uiLetter < l2.m_uiLetter;
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
CSpatialStringSearcher::LocalSubstring::LocalSubstring(unsigned int uiStartLetter, 
													   unsigned int uiEndLetter) 
	: m_uiStartLetter(uiStartLetter), m_uiEndLetter(uiEndLetter)
{
}
//-------------------------------------------------------------------------------------------------
void CSpatialStringSearcher::LocalSubstring::expandLeft(long lWords, 
	const vector<LocalLetter>& vecLetters, const vector<LocalWord>& vecWords)
{
	long lStartWord = (long)(vecLetters[m_uiStartLetter].m_uiWord) - lWords;
	if (lStartWord < 0)
	{
		lStartWord = 0;
	}

	m_uiStartLetter = vecWords[lStartWord].m_uiStart;
}
//-------------------------------------------------------------------------------------------------
void CSpatialStringSearcher::LocalSubstring::expandRight(long lWords,
	const vector<LocalLetter>& vecLetters, const vector<LocalWord>& vecWords)
{
	unsigned int uiEndWord = vecLetters[m_uiEndLetter].m_uiWord + lWords;
	unsigned int uiMaxWord = vecWords.size() - 1;
	if (uiEndWord > uiMaxWord)
	{
		uiEndWord = uiMaxWord;
	}

	m_uiEndLetter = vecWords[uiEndWord].m_uiEnd;
}
//-------------------------------------------------------------------------------------------------
bool CSpatialStringSearcher::LocalSubstring::isConnectedTo(const LocalSubstring& other) const
{
	return other.m_uiEndLetter + 1 >= m_uiStartLetter && other.m_uiStartLetter <= m_uiEndLetter + 1;
}
//-------------------------------------------------------------------------------------------------
void CSpatialStringSearcher::LocalSubstring::combineWith(const LocalSubstring& other)
{
	m_uiStartLetter = min(m_uiStartLetter, other.m_uiStartLetter);
	m_uiEndLetter = max(m_uiEndLetter, other.m_uiEndLetter);
}

//-------------------------------------------------------------------------------------------------
// CSpatialStringSearcher
//-------------------------------------------------------------------------------------------------
CSpatialStringSearcher::CSpatialStringSearcher()
: m_ipMemoryManager(__nullptr)
{
	// Set the default settings
	m_bIncludeDataOnBoundary = true; 
	m_bUseOriginalImageCoordinates = false;
	m_bUseMidpointsOnly = false;
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
void CSpatialStringSearcher::FinalRelease()
{
	try
	{
		clear();

		// If memory usage has been reported, report that this instance is no longer using any
		// memory.
		RELEASE_MEMORY_MANAGER(m_ipMemoryManager, "ELI36092");
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI31137");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialStringSearcher::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ISpatialStringSearcher,
		&IID_ILicensedComponent,
		&IID_IManageableMemory
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
STDMETHODIMP CSpatialStringSearcher::InitSpatialStringSearcher(ISpatialString* pSpatialString,
													VARIANT_BOOL vbUseOriginalImageCoordinates)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Ensure the new spatial string is not NULL
		UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipSpatialString(pSpatialString);
		ASSERT_ARGUMENT("ELI25663", ipSpatialString != __nullptr);

		validateLicense();

		// Clear out the information from any previously set string
		clear();

		m_bUseOriginalImageCoordinates = asCppBool(vbUseOriginalImageCoordinates);

		// assign our string to the new string
		m_ipSpatialString = ipSpatialString;
		m_strSourceDocName = asString(m_ipSpatialString->SourceDocName);

		// confirm that the string is spatial
		if (m_ipSpatialString->GetMode() == kSpatialMode)
		{
			// Get the spatial page info map
			m_ipSpatialPageInfoMap = m_ipSpatialString->SpatialPageInfos;
			createLocalLetters();
			createLocalWords();
			createLocalLines();
		}

		// Shrink the vectors to fit the current data
		m_vecLetters.shrink_to_fit();
		m_vecWords.shrink_to_fit();
		m_vecLines.shrink_to_fit();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07789");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialStringSearcher::GetDataInRegion(ILongRectangle *ipRect, 
													 VARIANT_BOOL bRotateRectanglePerOCR, 
													 ISpatialString **ipReturnString)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI25606", ipReturnString != __nullptr);

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
		ASSERT_RESOURCE_ALLOCATION("ELI19896", ipNewStr != __nullptr);

		// Return the string
		*ipReturnString = (ISpatialString*) ipNewStr.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10636");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialStringSearcher::GetDataOutOfRegion(ILongRectangle *ipRect, ISpatialString **ipReturnString)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI25890", ipRect != __nullptr);
		ASSERT_ARGUMENT("ELI25891", ipReturnString != __nullptr);

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
			if (!m_vecLetters[ui].letter.m_bIsSpatial)
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
		ASSERT_RESOURCE_ALLOCATION("ELI25892", ipNewStr != __nullptr);

		*ipReturnString = (ISpatialString*) ipNewStr.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07790");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialStringSearcher::ExtendDataInRegion(ILongRectangle *pRect, 
	long lNumWordsToExtend, VARIANT_BOOL vbExtendHeight, ISpatialString** ppFound)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI29540", ppFound != __nullptr);

		validateLicense();

		ILongRectanglePtr ipRect(pRect);
		ASSERT_RESOURCE_ALLOCATION("ELI29541", ipRect != __nullptr);

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
			createStringFromLetterIndexes(vecLetters, asCppBool(vbExtendHeight));
		ASSERT_RESOURCE_ALLOCATION("ELI29542", ipFound != __nullptr);

		// Return the string
		*ppFound = (ISpatialString*) ipFound.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29539");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialStringSearcher::GetLeftWord(ILongRectangle *pRect, ISpatialString **ppReturnString)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI31665", ppReturnString != __nullptr);
		ILongRectanglePtr ipRect(pRect);
		ASSERT_ARGUMENT("ELI31666", ipRect != __nullptr);

		validateLicense();

		// Default to NULL return
		*ppReturnString = __nullptr;

		// If less than 5 spatial letters in the searcher, there is no
		// enough context to look for words, just return
		if (m_vecLetters.size() < 5)
		{
			return S_OK;
		}

		// Get a vector of all the letters that fall in the region
		vector<int> vecLetters;		
		getUnsortedLettersInRegion(ipRect, vecLetters);

		// Convert the vector of letters to a vector of substrings
		vector<LocalSubstring> vecSubstrings;
		getLettersAsSubstrings(vecLetters, vecSubstrings);

		if (vecSubstrings.size() < 1)
		{
			return S_OK;
		}

		// Get the first substring and expand it to the left
		LocalSubstring& sub = vecSubstrings[0];
		unsigned int uiStart = sub.m_uiStartLetter;
		sub.expandLeft(1, m_vecLetters, m_vecWords);

		// If expansion is in same word, just return
		if (uiStart <= sub.m_uiStartLetter)
		{
			return S_OK;
		}

		unsigned int uiWord = m_vecLetters[sub.m_uiStartLetter].m_uiWord;

		// If word is outside bounds of word vector, just return
		if (uiWord < 0 || uiWord >= m_vecWords.size())
		{
			return S_OK;
		}

		// Get the starting word for the expanded substring
		LocalWord& word = m_vecWords[uiWord];

		// Clear the letter vector and add the indexes for each letter in the left-most word
		vecLetters.clear();
		for (unsigned int i=word.m_uiStart; i <= word.m_uiEnd; i++)
		{
			vecLetters.push_back(i);
		}

		// Create a return string from the left most word
		UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipFound =
			createStringFromLetterIndexes(vecLetters, false);
		ASSERT_RESOURCE_ALLOCATION("ELI31667", ipFound != __nullptr);

		*ppReturnString = (ISpatialString*)ipFound.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31668");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialStringSearcher::GetRightWord(ILongRectangle *pRect, ISpatialString **ppReturnString)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI31672", ppReturnString != __nullptr);
		ILongRectanglePtr ipRect(pRect);
		ASSERT_ARGUMENT("ELI31673", ipRect != __nullptr);

		validateLicense();

		// Default to NULL return
		*ppReturnString = __nullptr;

		// If less than 5 spatial letters in the searcher, there is no
		// enough context to look for words, just return
		if (m_vecLetters.size() < 5)
		{
			return S_OK;
		}

		// Get a vector of all the letters that fall in the region
		vector<int> vecLetters;		
		getUnsortedLettersInRegion(ipRect, vecLetters);

		// Convert the vector of letters to a vector of substrings
		vector<LocalSubstring> vecSubstrings;
		getLettersAsSubstrings(vecLetters, vecSubstrings);

		if (vecSubstrings.size() < 1)
		{
			return S_OK;
		}

		// Get the first substring and expand it to the right
		LocalSubstring& sub = vecSubstrings[vecSubstrings.size()-1];
		unsigned int uiEnd = sub.m_uiEndLetter;
		sub.expandRight(1, m_vecLetters, m_vecWords);
		unsigned int uiNewEnd = sub.m_uiEndLetter;
		if (uiNewEnd >= m_vecLetters.size())
		{
			uiNewEnd = m_vecLetters.size() - 1;
		}

		// If expansion is in same word, just return
		if (uiEnd >= uiNewEnd)
		{
			return S_OK;
		}

		unsigned int uiWord = m_vecLetters[uiNewEnd].m_uiWord;

		// If word is outside bounds of word vector, just return
		if (uiWord < 0 || uiWord >= m_vecWords.size())
		{
			return S_OK;
		}

		// Get the ending word for the expanded substring
		LocalWord& word = m_vecWords[uiWord];


		// Clear the letter vector and add the indexes for each letter in the right-most word
		vecLetters.clear();
		for (unsigned int i=word.m_uiStart; i <= word.m_uiEnd && i < m_vecLetters.size(); i++)
		{
			vecLetters.push_back(i);
		}

		// Create a return string from the left most word
		UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipFound =
			createStringFromLetterIndexes(vecLetters, false);
		ASSERT_RESOURCE_ALLOCATION("ELI31674", ipFound != __nullptr);

		*ppReturnString = (ISpatialString*)ipFound.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI31675");
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

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07791");
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
STDMETHODIMP CSpatialStringSearcher::SetUseMidpointsOnly(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		m_bUseMidpointsOnly = asCppBool(newVal);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36398");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialStringSearcher::ExcludeDataInRegion(ILongRectangle *pRect)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ILongRectanglePtr ipRect(pRect);
		ASSERT_ARGUMENT("ELI36740", ipRect != __nullptr);

		validateLicense();

		// Exclude all characters in the region.
		vector<int> vecLetterIndices;		
		getUnsortedLettersInRegion(ipRect, vecLetterIndices);

		size_t nCount = vecLetterIndices.size();
		for (size_t i = 0; i < nCount; i++)
		{
			m_vecLetters[vecLetterIndices[i]].m_bExcluded = true;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36741");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialStringSearcher::GetCharacterIndexesInRegion(ILongRectangle *pRect, IVariantVector **ppReturnVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI44815", ppReturnVal != __nullptr);
		ILongRectanglePtr ipRect(pRect);
		ASSERT_ARGUMENT("ELI44816", ipRect != __nullptr);

		validateLicense();

		// Default to NULL return
		*ppReturnVal = __nullptr;

		//// If less than 5 spatial letters in the searcher, there is no
		//// enough context to look for words, just return
		//if (m_vecLetters.size() < 5)
		//{
		//	return S_OK;
		//}

		// Get a vector of all the letters that fall in the region
		vector<int> vecLetters;		
		getUnsortedLettersInRegion(ipRect, vecLetters);

		if (vecLetters.size() > 0)
		{
			UCLID_COMUTILSLib::IVariantVectorPtr ipVV(CLSID_VariantVector);
			ASSERT_RESOURCE_ALLOCATION("ELI44817", ipVV != __nullptr);

			for (auto it = vecLetters.begin(); it != vecLetters.end(); ++it)
			{
				ipVV->PushBack(*it);
			}

			*ppReturnVal = ipVV.Detach();
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI44818");
}

//-------------------------------------------------------------------------------------------------
// IManageableMemory
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialStringSearcher::raw_ReportMemoryUsage(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		if (m_ipMemoryManager == __nullptr)
		{
			m_ipMemoryManager.CreateInstance(MEMORY_MANAGER_CLASS);
		}
		
		int nSize = sizeof(*this);
		// Report the size of all letters, words, and lines.
		nSize += m_vecLetters.size() * sizeof(LocalLetter);
		nSize += m_vecWords.size() * sizeof(LocalWord);
		nSize += m_vecLines.size() * sizeof(LocalLine);

		// [FlexIDSCore:5373]
		// For nested COM classes that are of a known or calculatable size such as SpatialPageInfo,
		// don't make nested calls into IMemoryManager's for each of those instances as this will
		// cause excessive COM calls hurting performance and using more memory that necessary.
		// Instead, just report their likely sizes here.
		if (m_ipSpatialPageInfoMap != __nullptr)
		{
			nSize += m_ipSpatialPageInfoMap->Size * (sizeof(long) + sizeof(CSpatialPageInfo));
		}

		m_ipMemoryManager->ReportUnmanagedMemoryUsage(nSize);

		// Report the size of the spatial string.
		if (m_ipSpatialString != __nullptr)
		{
			IManageableMemoryPtr ipManageableMemory = m_ipSpatialString;
			ASSERT_RESOURCE_ALLOCATION("ELI36030", ipManageableMemory != __nullptr);

			ipManageableMemory->ReportMemoryUsage();
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI36032");
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
	m_ipSpatialPageInfoMap = __nullptr;
	m_ipSpatialString = __nullptr;
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
	vector<CPPLetter> vecImageCppLetters;
	vector<CPPLetter> vecOCRCppLetters;
	long numLetters = 0;
	{
		// Copy the letters in original image coordinates to an array.
		CPPLetter* pLetters = NULL;
		m_ipSpatialString->GetOriginalImageLetterArray(&numLetters, (void**)&pLetters);
		ASSERT_RESOURCE_ALLOCATION("ELI36396", pLetters != __nullptr);

		vecImageCppLetters.resize(numLetters);
		memcpy(&(vecImageCppLetters[0]), pLetters, numLetters * sizeof(CPPLetter));

		// Copy the letters in OCR coordinates to an array.
		m_ipSpatialString->GetOCRImageLetterArray(&numLetters, (void**)&pLetters);
		ASSERT_RESOURCE_ALLOCATION("ELI36397", pLetters != __nullptr);

		vecOCRCppLetters.resize(numLetters);
		memcpy(&(vecOCRCppLetters[0]), pLetters, numLetters * sizeof(CPPLetter));
	}

	for (long i = 0; i < numLetters; i++)
	{
		const CPPLetter& imageLetter = vecImageCppLetters[i];
		const CPPLetter& OCRLetter = vecOCRCppLetters[i];

		m_vecLetters.push_back(LocalLetter(i, iCurrZone, iCurrParagraph, iCurrLine, iCurrWord));
		LocalLetter& rLocalLetter = m_vecLetters[i];
		rLocalLetter.letter = OCRLetter;
		
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
		if (OCRLetter.m_bIsEndOfParagraph)
		{
			iCurrParagraph++;
			rLocalLetter.m_lEndFlags |= kEOP;
		}
		// if this is the last letter in a Zone
		if (OCRLetter.m_bIsEndOfZone)
		{
			iCurrZone++;
			rLocalLetter.m_lEndFlags |= kEOZ;
		}

		//ONLY SPATIAL LETTERS MAY PROCEED BEYOND THIS POINT
		if (!OCRLetter.m_bIsSpatial)
		{
			continue;
		}

		// Set the letter's bounds according to whether the caller wishes to search according to OCR
		// or original image coordinates.
		if (m_bUseOriginalImageCoordinates)
		{
			// Set the bounds
			rLocalLetter.m_lLeft = imageLetter.m_ulLeft;
			rLocalLetter.m_lRight = imageLetter.m_ulRight;
			rLocalLetter.m_lTop = imageLetter.m_ulTop;
			rLocalLetter.m_lBottom = imageLetter.m_ulBottom;
		}
		else
		{
			rLocalLetter.m_lLeft = OCRLetter.m_ulLeft;
			rLocalLetter.m_lRight = OCRLetter.m_ulRight;
			rLocalLetter.m_lTop = OCRLetter.m_ulTop;
			rLocalLetter.m_lBottom = OCRLetter.m_ulBottom;
		}

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
		if (!rLocalLetter.letter.m_bIsSpatial)
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
		if (!rLocalLetter.letter.m_bIsSpatial)
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
bool CSpatialStringSearcher::intersectionMeetsCriteria(EIntersection eIntersection)
{
	if (m_bUseMidpointsOnly)
	{
		// kOverlapping indicates that the midpoint of the entity is included in the specified
		// bounds.
		if (eIntersection >= kOverlapping)
		{
			return true;
		}
	}
	else if ((m_bIncludeDataOnBoundary && eIntersection > kNotIntersecting) ||
			 (!m_bIncludeDataOnBoundary && eIntersection == kContains) )
	{
		return true;
	}

	return false;
}
//-------------------------------------------------------------------------------------------------
void CSpatialStringSearcher::getUnsortedLettersInRegion(ILongRectanglePtr ipRect, 
														vector<int> &rvecLetters)
{
	try
	{
		ASSERT_ARGUMENT("ELI25615", ipRect != __nullptr);
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
				if (rLetter.m_bExcluded || !rLetter.letter.m_bIsSpatial)
				{
					continue;
				}
				// Test the rLetter against the region
				EIntersection eIntersection = region.intersect(rLetter);

				if (intersectionMeetsCriteria(eIntersection))
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

				if (intersectionMeetsCriteria(eIntersection))
				{
					for (unsigned int uiLetter = rWord.m_uiStart; uiLetter < rWord.m_uiEnd; uiLetter++)
					{
						LocalLetter& rLetter = m_vecLetters[uiLetter];
						if (rLetter.m_bExcluded || !rLetter.letter.m_bIsSpatial)
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

				if (intersectionMeetsCriteria(eIntersection))
				{
					for (unsigned int uiLetter = rLine.m_uiStart;
						uiLetter < rLine.m_uiEnd; uiLetter++)
					{
						LocalLetter& rLetter = m_vecLetters[uiLetter];
						if (rLetter.m_bExcluded || !rLetter.letter.m_bIsSpatial)
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
	unsigned int uiStartSubstring = m_vecLetters[vecLetters[0]].m_uiLetter;
	unsigned int uiEndSubstring = uiStartSubstring;
	for	(unsigned int i = 1; i < vecLetters.size(); i++)
	{
		// Has a new substring been encountered?
		unsigned int uiCurrentLetter = vecLetters[i];
		if (uiCurrentLetter == uiEndSubstring + 1)
		{
			// This is the next letter in the same substring, remember it and keep looking
			uiEndSubstring = uiCurrentLetter;
		}
		else
		{
			// This is the end of a substring
			LocalSubstring substring(uiStartSubstring, uiEndSubstring);
			vecSubstrings.push_back(substring);

			// Start a new substring
			uiStartSubstring = uiCurrentLetter;
			uiEndSubstring = uiCurrentLetter;
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
	Random random;
	for (unsigned int i = 0; i < vecSubstrings.size(); i++)
	{
		LocalSubstring& substring = vecSubstrings[i];

		unsigned long ulLeft = random.uniform(1, lNumWordsToExpand + 1);
		unsigned long ulRight = random.uniform(1, lNumWordsToExpand + 1);
		substring.expandLeft(ulLeft, m_vecLetters, m_vecWords);
		substring.expandRight(ulRight, m_vecLetters, m_vecWords);
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

	// Iterate through each substring
	for (unsigned int i = 0; i < vecSubstrings.size(); i++)
	{
		const LocalSubstring& substring = vecSubstrings[i];

		// Iterate through each letter of the substring
		for (unsigned int j = substring.m_uiStartLetter; j <= substring.m_uiEndLetter; j++)
		{
			const LocalLetter& letter = m_vecLetters[j];

			// Add the letter if it is spatial
			if (letter.letter.m_bIsSpatial)
			{
				vecLetters.push_back(j);
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
	ASSERT_ARGUMENT("ELI16739", ipRect != __nullptr);

	if (m_ipSpatialString->HasSpatialInfo())
	{
		// Get first page number of the SpatialString
		// and associated SpatialPageInfo
		long nPageNum = m_ipSpatialString->GetFirstPageNumber();
		UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr ipInfo = m_ipSpatialString->GetPageInfo(nPageNum);
		ASSERT_RESOURCE_ALLOCATION("ELI16740", ipInfo != __nullptr);

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
	const vector<int> &rveciLetters, bool bAdjustHeight)
{
	// Create a new spatial string that will contain all the data in the region
	UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipNewStr(CLSID_SpatialString);
	ASSERT_RESOURCE_ALLOCATION("ELI08026", ipNewStr != __nullptr);

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
		addLocalLetter(vecNewLetters, rCurrLocalLetter, bAdjustHeight);

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
			const LocalLetter& localLetter = m_vecLetters[uiLetter];
			if (localLetter.letter.m_bIsSpatial)
			{
				break;
			}
			// If the letter isn't spatial add it
			addLocalLetter(vecNewLetters, localLetter, bAdjustHeight);
			cLastLetter = (char) localLetter.letter.m_usGuess1;

			// update the word index for non-spatial letters [P16 #2849]
			currWord = localLetter.m_uiWord;
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
void CSpatialStringSearcher::addLocalLetter(vector<CPPLetter>& vecLetters, 
											const LocalLetter& letter, bool bAdjustHeight)
{
	if (bAdjustHeight)
	{
		CPPLetter cppLetter = letter.letter;

		const LocalLine& line = m_vecLines[letter.m_uiLine];
		if (cppLetter.m_ulTop > (unsigned long) line.m_lTop)
		{
			cppLetter.m_ulTop = line.m_lTop;
		}
		if (cppLetter.m_ulBottom < (unsigned long) line.m_lBottom)
		{
			cppLetter.m_ulBottom = line.m_lBottom;
		}

		vecLetters.push_back(cppLetter);
	}
	else
	{
		vecLetters.push_back(letter.letter);
	}
}