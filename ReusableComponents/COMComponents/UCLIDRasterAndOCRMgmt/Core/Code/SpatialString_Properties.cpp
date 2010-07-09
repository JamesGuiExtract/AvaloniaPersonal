// SpatialStringProperties.cpp : Implementation of ISpatialString properties
#include "stdafx.h"
#include "SpatialString.h"
#include "UCLIDRasterAndOCRMgmt.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <COMUtils.h>

#include <cmath>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const double gfLineHeightToLargeCharRatio = 1.65;
const double gfLineHeightToSmallCharRatio = 2.15;
 
//-------------------------------------------------------------------------------------------------
// ISpatialString - Properties
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::get_String(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI20672", pVal != NULL);

		// Check license
		validateLicense();

		// For all modes the member variable string should be set to the strings value
		// there should never be a time when it is out of synch, just return that string
		*pVal = _bstr_t(m_strString.c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05842")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::get_Size(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI20670", pVal != NULL);

		// Check license
		validateLicense();

		// return the size of the string, which is available
		// regardless of whether the string is spatial or not
		*pVal = m_strString.length();
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06439")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::get_SourceDocName(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI20669", pVal != NULL);

		// validate license
		validateLicense();

		// return the current value of the source-doc-name attribute
		*pVal = _bstr_t(m_strSourceDocName.c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06804");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::put_SourceDocName(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		// store the new value of the source-doc-name attribute
		m_strSourceDocName = asString(newVal);

		// set the dirty flag to true since a modification was made
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06805");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::get_SpatialPageInfos(ILongToObjectMap** pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI20668", pVal != NULL);

		// Check license
		validateLicense();

		// Make sure this string is spatial
		if( m_eMode == kNonSpatialMode )
		{
			UCLIDException ue( "ELI14753", 
				"GetSpatialPageInfos() requires a string with spatial info!");
			throw ue;
		}

		// Return a reference to the internal page map
		ILongToObjectMapPtr ipShallowCopy = m_ipPageInfoMap;
		ASSERT_RESOURCE_ALLOCATION("ELI26005", ipShallowCopy != NULL);

		*pVal = ipShallowCopy.Detach();
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09134")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetOCRImageLetter(long nIndex, ILetter **pLetter)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI20667", pLetter != NULL);

		// Check license
		validateLicense();

		// this operation requires that this string is of kSpatialMode type
		if( m_eMode != kSpatialMode )
		{
			UCLIDException ue("ELI14735", "GetLetter() must be used on a spatial-mode string!");
			throw ue;
		}

		// ensure that nIndex is valid
		verifyValidIndex(nIndex);

		// Validate the vector before accessing it.
		performConsistencyCheck();

		// create letter object at the specified index
		UCLID_RASTERANDOCRMGMTLib::ILetterPtr ipLetter(CLSID_Letter);
		ASSERT_RESOURCE_ALLOCATION("ELI25855", ipLetter != NULL);

		ipLetter->CreateFromCppLetter((void*) &(m_vecLetters[nIndex]));
		
		// return the reference to the specific letter object
		*pLetter = (ILetter*) ipLetter.Detach();
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06438")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetChar(long nIndex, long *pChar)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI20666", pChar != NULL);

		// Check license
		validateLicense();

		// ensure that nIndex is valid
		verifyValidIndex(nIndex);

		// get the letter object at the specified index
		*pChar = m_strString[nIndex];
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19461")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::SetChar(long nIndex, long nChar)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		// Verify that the char to set is a valid ascii code
		if( nChar > 255 )
		{
			UCLIDException ue("ELI15311", "Invalid character specified for SetChar()!");
			ue.addDebugInfo("Ascii code:", asString( nChar ) );
			throw ue;			
		}

		// ensure that nIndex is valid
		verifyValidIndex(nIndex);

		// ensure that nChar is not a null char
		ASSERT_ARGUMENT("ELI14888", nChar != NULL);

		// Gurantee the attempt to access the vector will not fail
		performConsistencyCheck();

		// store the new value of the character at the specified position
		m_strString[nIndex] = (char) nChar;

		// update the letter object if the string is spatial
		if (m_eMode == kSpatialMode)
		{
			m_vecLetters[nIndex].m_usGuess1 = (unsigned short) nChar;
		}

		// set the dirty flag to true since a modification was made
		m_bDirty = true;
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06865");
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetOCRImageLetterArray(long* pnNumLetters, void** ppLetters)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI20664", pnNumLetters != NULL);
		ASSERT_ARGUMENT("ELI20665", ppLetters != NULL);

		// Check license
		validateLicense();

		// Make sure this string is spatial
		verifySpatialness();

		*pnNumLetters = m_vecLetters.size();
		*ppLetters = &(m_vecLetters[0]);
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10463")
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetOriginalImageBounds(ILongRectangle **pBounds)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI20663", pBounds != NULL);

		// Check license
		validateLicense();

		// this operation requires that this string is a string with spatial info associated with it
		if( m_eMode == kNonSpatialMode)
		{
			UCLIDException ue("ELI14737", "Unable to get bounds on a non-spatial string!");
			throw ue;
		}

		// This operation also requires that IsMultiPage == VARIANT_FALSE
		if(isMultiPage())
		{
			UCLIDException ue("ELI14733", "Spatial String must not be multi-page for GetBounds!");
			throw ue;
		}

		// Declare the long rectangle to hold the return value
		ILongRectanglePtr ipLongRectangle = NULL;

		// Get the Raster zones for this spatial string
		IIUnknownVectorPtr ipZones = getOriginalImageRasterZones();
		ASSERT_RESOURCE_ALLOCATION("ELI25856", ipZones != NULL);
		if (ipZones->Size() > 0)
		{
			// Get the first raster zone
			UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipZone = ipZones->At(0);
			ASSERT_RESOURCE_ALLOCATION("ELI25659", ipZone != NULL);

			// Get the spatial page info for this page.
			// Although it is possible to pass NULL into GetBoundsFromMultipleRasterZones
			// we should never have a spatial string that has spatial info that does not
			// have a spatial page info object so ASSERT that condition.
			UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr ipPageInfo =
				m_ipPageInfoMap->GetValue(ipZone->PageNumber);
			ASSERT_RESOURCE_ALLOCATION("ELI25660", ipPageInfo != NULL);

			// Get a bounding rectangle for the raster zones (restrict the bounding rectangle
			// by the page dimensions)
			ipLongRectangle = ipZone->GetBoundsFromMultipleRasterZones(ipZones, ipPageInfo);
			ASSERT_RESOURCE_ALLOCATION("ELI25661", ipLongRectangle != NULL);
		}
		else
		{
			// There are no raster zones, just return an empty rectangle
			ipLongRectangle.CreateInstance(CLSID_LongRectangle);
			ASSERT_RESOURCE_ALLOCATION("ELI25662", ipLongRectangle != NULL);
		}

		// return the long rectangle
		*pBounds = ipLongRectangle.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06437")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetWords(IIUnknownVector **pvecWords)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI20662", pvecWords != NULL);

		// Check license
		validateLicense();

		// Create vector for resulting ISpatialStrings
		IIUnknownVectorPtr ipWords = getWords();
		ASSERT_RESOURCE_ALLOCATION("ELI07144", ipWords != NULL);

		// Return the vector of word strings
		*pvecWords = ipWords.Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06436")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetLines(IIUnknownVector **pvecLines)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI20661", pvecLines != NULL);

		// Check license
		validateLicense();

		// Create vector for resulting ISpatialStrings
		IIUnknownVectorPtr ipLines = getLinesUnknownVector();
		ASSERT_RESOURCE_ALLOCATION("ELI07146", ipLines != NULL);

		// Return the vector of line  strings
		*pvecLines = ipLines.Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06435")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetParagraphs(IIUnknownVector **pvecParagraphs)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// TODO: This method should work for any type of string
		// including non spatial strings, using double newlines or double \r\n as 
		// paragraph separators.

		ASSERT_ARGUMENT("ELI20660", pvecParagraphs != NULL);

		// Check license
		validateLicense();

		// Make sure string is spatial and has data
		if( m_eMode != kSpatialMode )
		{
			UCLIDException ue( "ELI14741", 
				"GetParagraphs() requires a spatial string in spatial mode!");
			ue.addDebugInfo("Mode:", asString (m_eMode) );
			throw ue;
		}
		
		// Create vector for resulting ISpatialStrings
		IIUnknownVectorPtr ipParagraphs = getParagraphs();
		ASSERT_RESOURCE_ALLOCATION("ELI07148", ipParagraphs != NULL);

		// Return the vector of paragraph strings
		*pvecParagraphs = ipParagraphs.Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06434")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetSpecifiedPages(long nStartPageNum, long nEndPageNum, 
											   ISpatialString** ppResultString)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI20659", ppResultString != NULL);

		validateLicense();

		if (m_eMode == kNonSpatialMode)
		{
			UCLIDException ue("ELI25857", "GetSpecifiedPages is not valid for a non-spatial string!");
			throw ue;
		}

		performConsistencyCheck();

		// Get the last page number
		long nLastPageNumber = getLastPageNumber();

		// Get and validate the actual start and end page numbers
		bool bWholeDocument = getStartAndEndPageNumber(nStartPageNum, nEndPageNum, nLastPageNumber);

		// The spatial string that will contain the specified pages
		UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipReturn(CLSID_SpatialString);
		ASSERT_RESOURCE_ALLOCATION("ELI07418", ipReturn != NULL);

		// If this is a hybrid mode, copy the raster zones that are on the specified
		// pages. Also return spatial page info for the specified pages.
		if(m_eMode == kHybridMode)
		{
			IIUnknownVectorPtr ipSpecifiedZones(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI15315", ipSpecifiedZones != NULL);

			// Keep track of whether any raster zones were found on any of the specified pages.
			long qualifyingRasterZoneCount = 0;
			size_t nSourceRasterZoneCount = m_vecRasterZones.size();

			// Check each raster zone. If the page is one of the specified pages,
			// add it to the vector of raster zones.
			for(size_t i = 0; i < nSourceRasterZoneCount; i++)
			{
				UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipRaster = m_vecRasterZones[i];
				ASSERT_RESOURCE_ALLOCATION("ELI15313", ipRaster != NULL);

				long nPage = ipRaster->PageNumber;
				// If the page number is greater than or equal to the start number
				// AND the page number is less than or equal to the end number OR the end number is -1
				if ( (nPage >= nStartPageNum) && (nPage <= nEndPageNum) ) 
				{
					qualifyingRasterZoneCount++;

					// Since this page is in the specified range, append it to
					// the vector of raster zones
					ipSpecifiedZones->PushBack(ipRaster);
				}
			}

			// If qualifying raster zones were found, use them to build a raster zone.
			if (qualifyingRasterZoneCount > 0)
			{
				// If the entire spatial string is contained on the specified pages,
				// return a copy of the original (all text included) otherwise part
				// of the spatial string is not on the specified pages, cannot determine
				// which text is which so return blank text.
				ipReturn->CreateHybridString(ipSpecifiedZones,
					qualifyingRasterZoneCount == nSourceRasterZoneCount ? m_strString.c_str() : "",
					m_strSourceDocName.c_str(), m_ipPageInfoMap);
			}
		}
		else if (m_eMode == kSpatialMode)
		{
			// Note that the string is guaranteed to be spatial from this point on
			long nNumLetters = m_vecLetters.size();

			// if the specified page range is actually the entire document
			if (bWholeDocument)
			{
				// The return the entire document
				// set references to the letters
				if(!m_vecLetters.empty())
				{
					ipReturn->CreateFromLetterArray(nNumLetters, &(m_vecLetters[0]),
						m_strSourceDocName.c_str(), m_ipPageInfoMap);
				}
				else
				{
					// This condition should never happen. You cannot have a spatial string
					// with an empty letter vector
					UCLIDException ue("ELI25858", "Spatial string had empty letter vector!");
					ue.addDebugInfo("Source Document", m_strSourceDocName);
					ue.addDebugInfo("Number of pages", nLastPageNumber);
				}

				*ppResultString = (ISpatialString*) ipReturn.Detach();
				return S_OK;
			}
			// if specified start page number <= total number
			else if (nStartPageNum <= nLastPageNumber)
			{
				// create a vector to hold letter on those pages
				vector<CPPLetter> vecLetters;

				// any non-spatial characters preceeding the first spatial character should be
				// set to have the same page as the first spatial character
				// to do this we must here calculate the page number of the first spatial character
				CPPLetter tempLetter;
				long nFirstSpatialLetter = getNextOCRImageSpatialLetter(0, tempLetter);

				// this will hold the page number of the most recently processed 
				// spatial character for use in assigning the page number of 
				// non-spatial characters
				// Note: We know that the string is spatial here so we can 
				// assume that GetNextSpatialLetter must have returned a valid 
				// index
				long nLastSpatialPageNum = tempLetter.m_usPageNumber;

				for (long n = 0; n < nNumLetters; n++)
				{
					CPPLetter& letter = m_vecLetters[n];

					long nCurrentPageNumber =
						letter.m_bIsSpatial ? letter.m_usPageNumber : nLastSpatialPageNum;

					// break out of the loop if current page is beyond end page
					if (nCurrentPageNumber > nEndPageNum)
					{
						break;
					}
					// only get letters from specified starting to ending page
					else if (nCurrentPageNumber >= nStartPageNum)
					{
						vecLetters.push_back(letter);
					}

					// Update the last spatial page number
					nLastSpatialPageNum = nCurrentPageNumber;
				}

				// Set the return string from the vector of letters
				if (vecLetters.size() > 0)
				{
					ipReturn->CreateFromLetterArray(vecLetters.size(), &(vecLetters[0]),
						m_strSourceDocName.c_str(), m_ipPageInfoMap);
				}
			}
		}
		
		// Set the return value and return
		*ppResultString = (ISpatialString*)ipReturn.Detach();
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07417");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetRelativePages(long nStartPageNum, long nEndPageNum, 
											  ISpatialString **ppResultString)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI20658", ppResultString != NULL);

		validateLicense();

		if (m_eMode == kNonSpatialMode)
		{
			UCLIDException ue("ELI25859", "GetRelativePages is not valid for a non-spatial string!");
			throw ue;
		}

		UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipReturn(CLSID_SpatialString);
		ASSERT_RESOURCE_ALLOCATION("ELI08569", ipReturn != NULL);

		if( m_eMode == kHybridMode )
		{
			// Create an object that will store the relative raster zones as specified by the arguments
			IIUnknownVectorPtr ipRelativeRasterZones(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI15301", ipRelativeRasterZones != NULL);

			for(vector<UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr>::iterator it = m_vecRasterZones.begin();
				it != m_vecRasterZones.end(); it++)
			{
				// Get each raster zone
				UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipRZ(*it);
				ASSERT_RESOURCE_ALLOCATION("ELI15302", ipRZ != NULL);

				// Get the raster zones page number
				long nPageNumber = ipRZ->PageNumber;

				// Check if each raster zone is within the specified range. If it is, 
				// add it to the relevant raster zone vector list
				if( (nPageNumber >= nStartPageNum) && (nPageNumber <= nEndPageNum) )
				{
					// If it is , add it to the relative vector
					ipRelativeRasterZones->PushBack(ipRZ);
				}
			}

			// Create the new object from this object's relevant spatial info
			ipReturn->CreateHybridString(ipRelativeRasterZones, m_strString.c_str(),
				m_strSourceDocName.c_str(), m_ipPageInfoMap);
		}
		else if( m_eMode == kSpatialMode )
		{
			// how many letters are there in the string
			long nNumLetters = m_vecLetters.size();
			if (nNumLetters <= 0)
			{
				// This is an error condition, a truly spatial string should never have
				// an empty letter vector
				UCLIDException ue("ELI25860", "Spatial string had empty letter vector!");
				ue.addDebugInfo("Source Document", m_strSourceDocName);
				throw ue;
			}

			// get the last page number to save some time
			// Only spatial letters have valid page numbers
			long nLastPageNum = getLastPageNumber();

			// keep track of current page as we proceed
			long nCurrentPageNum = -1;
			long nTotalPageNum=0;

			// following For block calculates the actual total number of pages for this string
			for (long i = 0; i < nNumLetters; i++)
			{
				CPPLetter& letter = m_vecLetters[i];

				// what's the original page number for this letter
				long nOriginPageNumber = letter.m_usPageNumber;
				if (nCurrentPageNum != nOriginPageNumber)
				{
					nCurrentPageNum = nOriginPageNumber;
					nTotalPageNum++;
				}

				if (nOriginPageNumber == nLastPageNum)
				{
					break;
				}
			}

			// if the specified page range is actually the entire document
			if (getStartAndEndPageNumber(nStartPageNum, nEndPageNum, nTotalPageNum))
			{
				// Then return the entire document
				ipReturn->CreateFromLetterArray(nNumLetters, &(m_vecLetters[0]),
					m_strSourceDocName.c_str(), m_ipPageInfoMap);
			}
			else
			{
				// this will hold the page number of the most recently processed 
				// spatial character for use in assigning the page number of 
				// non-spatial characters
				long nLastOrigSpatialPageNum = 1;

				// any non-spatial characters preceeding the first spatial character should be
				// set to have the same page as the first spatial character
				// to do this we must here calculate the page number of the first spatial character
				CPPLetter letter;
				long nFirstSpatialLetter = getNextOCRImageSpatialLetter(0, letter);
				nLastOrigSpatialPageNum = letter.m_usPageNumber;

				nCurrentPageNum = -1;
				// the physical page number, not necessarily equals to the oringial page number
				long nCurrentPhysicalPageNumber = 0;
				// create a vector to hold letter on those pages
				vector<CPPLetter> vecLetters;
				for (long i = 0; i < nNumLetters; i++)
				{
					CPPLetter& tempLetter = m_vecLetters[i];

					// what's the original page number for this letter
					long nOriginPageNumber =
						tempLetter.m_bIsSpatial ? tempLetter.m_usPageNumber : nLastOrigSpatialPageNum;

					if (nCurrentPageNum != nOriginPageNumber)
					{
						// increment the physical page number
						nCurrentPhysicalPageNumber++;
						nCurrentPageNum = nOriginPageNumber;
					}

					// break out of the loop if start page is beyond end page
					if (nCurrentPhysicalPageNumber > nEndPageNum)
					{
						break;
					}

					// only get letters from specified starting to ending page
					if (nCurrentPhysicalPageNumber >= nStartPageNum 
						&& nCurrentPhysicalPageNumber <= nEndPageNum)
					{
						vecLetters.push_back(tempLetter);
					}

					nLastOrigSpatialPageNum = nOriginPageNumber;
				}

				if (vecLetters.size() > 0)
				{
					ipReturn->CreateFromLetterArray(vecLetters.size(), &(vecLetters[0]),
						m_strSourceDocName.c_str(), m_ipPageInfoMap);
				}
			}
		}		

		// Set the return value and return
		*ppResultString = (ISpatialString*)ipReturn.Detach();
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08568");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetPages(IIUnknownVector **pvecPages)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI20657", pvecPages != NULL);

		validateLicense();

		// Ensure the string is spatial
		if (m_eMode == kNonSpatialMode)
		{
			UCLIDException ue("ELI25861", "GetPages is not valid for a non-spatial string!");
			throw ue;
		}

		// Create vector for resulting ISpatialStrings
		IIUnknownVectorPtr ipPages(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI07482", ipPages != NULL);

		if (m_eMode == kHybridMode)
		{
			map<long, vector<UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr>> mapZonesToPages;

			// Map each of the raster zones to their specific pages
			for (vector<UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr>::iterator it = m_vecRasterZones.begin();
				it != m_vecRasterZones.end(); it++)
			{
				// Get the raster zone
				UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipZone(*it);
				ASSERT_RESOURCE_ALLOCATION("ELI25862", ipZone != NULL);

				// Add this raster zone to the map of pages to zones
				mapZonesToPages[ipZone->PageNumber].push_back(ipZone);
			}

			// Build a spatial string for each page from its collection of raster zones
			for (map<long, vector<UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr>>::iterator it = mapZonesToPages.begin();
				it != mapZonesToPages.end(); it++)
			{
				// Build the collection of raster zones for this page
				IIUnknownVectorPtr ipPageZones(CLSID_IUnknownVector);
				ASSERT_RESOURCE_ALLOCATION("ELI25863", ipPageZones != NULL);
				for (vector<UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr>::iterator vecIt = it->second.begin();
					vecIt != it->second.end(); vecIt++)
				{
					ipPageZones->PushBack(*vecIt);
				}

				// Build a spatial string for the zones
				UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipString(CLSID_SpatialString);
				ASSERT_RESOURCE_ALLOCATION("ELI25864", ipString != NULL);
				ipString->CreateHybridString(ipPageZones, m_strString.c_str(),
					m_strSourceDocName.c_str(), m_ipPageInfoMap);

				// Add the spatial string to the return vector
				ipPages->PushBack(ipString);
			}
		}
		else if (m_eMode == kSpatialMode)
		{
			// Get the first spatial letter (since string is spatial we are guaranteed that
			// there is at least 1 spatial letter)
			CPPLetter letter;
			long nFirstSpatial = getNextOCRImageSpatialLetter(0, letter);

			// Get the page number from the letter
			long nCurrPage = letter.m_usPageNumber;

			// Set the start position to beginning of the string
			long nStartPos = 0;

			// Loop over the letters of the spatial string and create substrings
			// when each new page is encountered
			long nNumLetters = m_vecLetters.size();
			for (long i = 0; i < nNumLetters; i++)
			{
				// Get Letter
				CPPLetter& tempLetter = m_vecLetters[i];

				if(!tempLetter.m_bIsSpatial)
				{
					continue;
				}

				if (tempLetter.m_usPageNumber != nCurrPage) 
				{
					// get the page beginning ending with current letter
					UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipItem =
						getSubString(nStartPos, i-1);
					ASSERT_RESOURCE_ALLOCATION("ELI25865", ipItem != NULL);

					ipPages->PushBack( ipItem );

					// Move the Start position of next zone to after end of this zone
					nStartPos = i;

					// Update the current page number
					nCurrPage = tempLetter.m_usPageNumber;
				}
			}

			// Check for case last letter not marked as end of page
			if ( nStartPos < nNumLetters ) 
			{
				// get the page beginning at nStartPos and ending with last letter
				UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipItem =
					getSubString(nStartPos, nNumLetters - 1);
				ASSERT_RESOURCE_ALLOCATION("ELI25866", ipItem != NULL);
				ipPages->PushBack( ipItem );
			}
		}

		// Return the vector of zone strings
		*pvecPages = ipPages.Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07481");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetAverageLineHeight(long *lpHeight)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	// Reasons this method is inaccurate
	// - We should probably be using the median line height of each paragraph
	// instead of the the mean
	// - Measuring from the bottom of one line to the bottom of the next is incorrect
	// if there are characters that descend below the line (such as g) on one line
	// but not the others
	// - When string spans multiple paragraphs it might be better to use a weighted
	// average of all its paragraphs
	try
	{
		ASSERT_ARGUMENT("ELI20416", lpHeight != NULL);

		// Check license
		validateLicense();

		// Make sure this string is spatial
		verifySpatialness();

		// Iterate over every paragraph in this string
		// For each paragraph compute the median line height
		// If the median line height 

		// Get all of the paragraphs
		IIUnknownVectorPtr ipParagraphs = getParagraphs();
		ASSERT_RESOURCE_ALLOCATION("ELI20417", ipParagraphs != NULL);

		long lParagraphCount = ipParagraphs->Size();

		// If this string has only one paragraph we will calculate its line height based on 
		// its lines
		// Note: it is inefficient to use recursion here because of the call to getParagraphs
		// on 1 paragraph strings, but once there is a speed improvement on getParagraphs
		// or a getNumParagraphs method the penalty disappears
		if (lParagraphCount == 1)
		{
			// First try to calculate the line height based on the distance between
			// the lines in the paragraph

			// Get all the lines in this string
			UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipParagraph = ipParagraphs->At(0);
			ASSERT_RESOURCE_ALLOCATION("ELI20418", ipParagraph != NULL);
			IIUnknownVectorPtr ipLines = ipParagraph->GetLines();
			ASSERT_RESOURCE_ALLOCATION("ELI20419", ipLines != NULL);
			
			unsigned int totalParagraphLineHeight = 0;
			unsigned int numUsedLines = 0;

			// to get the distance between two lines we have to have two lines
			// so the loop stops before ipLines->Size() - 1 instead of ipLines->Size()
			int iLine = 0;
			long lLinesSize = ipLines->Size();
			for (iLine = 0; iLine < lLinesSize - 1; iLine++)
			{
				// Get the bounding boxes of the two lines
				UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipLine1 = ipLines->At(iLine);
				ASSERT_RESOURCE_ALLOCATION("ELI20420", ipLine1 != NULL);
				UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipLine2 = ipLines->At(iLine + 1);
				ASSERT_RESOURCE_ALLOCATION("ELI20421", ipLine2 != NULL);

				// both must be spatial
				if (ipLine1->GetMode() != kSpatialMode || ipLine2->GetMode() != kSpatialMode)
				{
					continue;
				}

				ILongRectanglePtr ipBounds1 = ipLine1->GetOCRImageBounds();
				ASSERT_RESOURCE_ALLOCATION("ELI20422", ipBounds1 != NULL);
				ILongRectanglePtr ipBounds2 = ipLine2->GetOCRImageBounds();
				ASSERT_RESOURCE_ALLOCATION("ELI20423", ipBounds2 != NULL);

				// get the tops
				long lTop1 = ipBounds1->Top;
				long lTop2 = ipBounds2->Top;

				// initialize the height and gap to 0
				long lHeight(0), lGap(0);

				// the following code has been changed as per [p16 #2524]
				// compute height by computing the height of the lowest
				// line and the gap between the top of the lowest line and
				// the bottom of the highest line
				// line 1 is higher than line 2
				if (lTop1 < lTop2)
				{
					// compute height of the second line from its bounds
					lHeight = ipBounds2->Bottom - lTop2;

					// compute the gap between line 1 and line 2
					lGap = lTop2 - ipBounds1->Bottom;
				}
				// line 2 is higher than line 1
				else
				{
					// compute height of the first line from its bounds
					lHeight = ipBounds1->Bottom - lTop1;

					// compute the gap between line 1 and line 2
					lGap = lTop1 - ipBounds2->Bottom;
				}

				// the height is the height of the line + the gap (if it is greater than 0)
				totalParagraphLineHeight += (lHeight + (lGap > 0 ? lGap : 0));
				numUsedLines++;
			}
			
			if (numUsedLines > 0)
			{
				// return the height based on the average distance between lines
				*lpHeight = totalParagraphLineHeight / numUsedLines;
			}
			else
			{
				// this is the height of a 'normal' lowercase character such as 'e'
				int totSmallCharHeight = 0;
				int numSmallChars = 0;
				// This is the height of an uppercase letter or 'large' lowercase letter
				// such as 'K' or 'g' or 'b'
				int totLargeCharHeight = 0;
				int numLargeChars = 0;
				// This is the height of the tallest character in the string
				int maxCharHeight = 0;

				for (unsigned int uiLetter = 0; uiLetter < m_vecLetters.size(); uiLetter++)
				{
					CPPLetter& letter = m_vecLetters[uiLetter];
					char c = (char) letter.m_usGuess1;
					if ((c >= 'A' && c <= 'Z') || // if c is a 'tall' character
						c == 'l' ||
						c == 'f' ||
						c == 'd' ||
						c == 'b' ||
						c == 'g' ||
						c == 'y')
					{
						totLargeCharHeight += letter.m_usBottom - letter.m_usTop;
						numLargeChars++;
					}
					else if ( (c >= 'a' && c <= 'z') ) // if c is a 'small' character
					{
						totSmallCharHeight += letter.m_usBottom - letter.m_usTop;
						numSmallChars++;
					}
					else // if c is any other character character
					{
						int h = letter.m_usBottom - letter.m_usTop;
						if (h > maxCharHeight)
							maxCharHeight = h;
					}
				}

				// If there are any 'large' letters use their 
				// average height to calculate the line height
				if (numLargeChars > 0)
					*lpHeight = (long)((float)(totLargeCharHeight / numLargeChars) 
						* gfLineHeightToLargeCharRatio);
				else if (numSmallChars > 0) // use small chars if there are any
					*lpHeight = (long)((float)(totSmallCharHeight / numSmallChars) 
						* gfLineHeightToSmallCharRatio);
				else // use the largest character found
					*lpHeight = (long) (maxCharHeight * gfLineHeightToLargeCharRatio);
			}
		}
		// If it spans multiple paragraphs we will use the average height of all its paragraphs
		else 
		{
			int totalHeight = 0;
			int numLines = 0;
			long iParagraph = 0;
			for (iParagraph = 0; iParagraph < lParagraphCount; iParagraph++)
			{
				UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipParagraph = ipParagraphs->At(iParagraph);
				ASSERT_RESOURCE_ALLOCATION("ELI25867", ipParagraph != NULL);
				if(ipParagraph->GetMode() != kSpatialMode)
				{
					continue;
				}

				totalHeight += ipParagraph->GetAverageLineHeight();
				numLines++;
			}
			*lpHeight = totalHeight / numLines;
		}
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07793");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetAverageCharWidth(long *lpWidth)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI20656", lpWidth != NULL);

		// Check license
		validateLicense();

		*lpWidth = getAverageCharWidth();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07805");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetAverageCharHeight(long *lpHeight)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI28047", lpHeight != NULL);

		// Check license
		validateLicense();

		*lpHeight = getAverageCharHeight();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28048");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetSplitLines(long nMaxSpace, IIUnknownVector** ppResultVector)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI20654", ppResultVector != NULL);

		// Check license
		validateLicense();

		// Make sure this string is spatial
		verifySpatialness();

		IIUnknownVectorPtr ipNewLines(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI08818", ipNewLines != NULL);

		// TODO: Change code to use getLines(vector<pair<long, long>>) to reduce COM overhead
		// Check each line for splits
		IIUnknownVectorPtr ipLines = getLinesUnknownVector();
		ASSERT_RESOURCE_ALLOCATION("ELI25868", ipLines != NULL);

		long lLineCount = ipLines->Size();
		for (long i = 0; i < lLineCount; i++)
		{
			UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipLine = ipLines->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI08579", ipLine != NULL);

			// the line must be spatial in order to split it
			if (ipLine->GetMode() != kSpatialMode)
			{
				continue;
			}

			long nMaxPixelSpace = ipLine->GetAverageCharWidth()*nMaxSpace;
			long lNewLineStartPos = 0;

			// This represents the portion of the line that has not 
			// already been used
			// Initially it is the entire line
			UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipUnusedLine = ipLine;

			CPPLetter* letters = NULL;
			long nNumLetters = -1;
			ipLine->GetOCRImageLetterArray(&nNumLetters, (void**)&letters);
			
			for (long j = 0; j < nNumLetters - 1; j++)
			{
				long lLetter1 = ipLine->GetNextOCRImageSpatialLetter(j, NULL);

				if (lLetter1 < 0)
				{
					break;
				}

				long end1 = lLetter1;
				long lLetter2 = -1;
				// Make sure that the search does not exceed string length
				if (end1 < nNumLetters - 1)
				{
					lLetter2 = ipLine->GetNextOCRImageSpatialLetter(end1 + 1, NULL);
				}

				if (lLetter2 < 0)
				{
					break;
				}

				long start2 = lLetter2;

				// now ensure that only space characters exist between these two
				// if there are any non-whitespace, non-spatial characters in between
				// we do not want to split
				bool bAllWhitespace = true;
				
				for (long k = end1 + 1; k < start2; k++)
				{
					long c = ipLine->GetChar(k);

					if (!(c == '\r' || c == '\n' || c == '\t' || c == ' '))
					{
						bAllWhitespace = false;
						break;
					}
				}

				if (!bAllWhitespace)
				{
					j = start2;
					// because i will increment when we
					// continue
					j--;
					continue;
				}
				
				CPPLetter& letter1 = letters[end1];
				CPPLetter& letter2 = letters[start2];
			
				long lRight1 = letter1.m_usRight;
				long lLeft2 = letter2.m_usLeft;

				if ((lLeft2 - lRight1) > nMaxPixelSpace)
				{
					UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipLineSeg1 =
						ipLine->GetSubString(lNewLineStartPos, end1);
					ASSERT_RESOURCE_ALLOCATION("ELI25869", ipLineSeg1 != NULL);

					// Add the new line segment	
					ipNewLines->PushBack(ipLineSeg1);

					// the next line segment will start where this one ended
					lNewLineStartPos = start2;

					// this can be optimized out later
					// create a line segment or the rest of the line
					UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipLineSeg2 =
						ipLine->GetSubString(start2, nNumLetters-1);
					ipUnusedLine = ipLineSeg2;

					// resume our search for line breaks at the start of the new line
					j = lNewLineStartPos;
				}
			}

			// Add any unused line
			// Note that if the line is never split this is the entire line
			if (ipUnusedLine->GetMode() == kSpatialMode)
			{
				ipNewLines->PushBack(ipUnusedLine);
			}
		}
		*ppResultVector = ipNewLines.Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08817");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetJustifiedBlocks(long nMinLines, IIUnknownVector** ppResultVector)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI20653", ppResultVector != NULL);

		// Check license
		validateLicense();

		// Make sure this string is spatial
		verifySpatialness();

		// This Vector will hold the newly created blocks
		// which will be returned to the user
		IIUnknownVectorPtr ipNewBlocks(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI08870", ipNewBlocks != NULL);
		
		// The will hold the unused lines as we process
		// all the lines for addition into blocks
		vector<UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr> vecUnusedLines;
		vector<UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr> vecTmpLines;

		IIUnknownVectorPtr ipLines = getLinesUnknownVector();
		vecUnusedLines.clear();
		long lLineCount = ipLines->Size();
		for (long i = 0; i < lLineCount; i++)
		{
			UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipTmpString = ipLines->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI15414", ipTmpString != NULL);

			if (ipTmpString->GetMode() != kSpatialMode)
			{
				continue;
			}
			vecUnusedLines.push_back(ipLines->At(i));
		}
		vecTmpLines.clear();
		// While there are lines that have not been assigned a block
		while (vecUnusedLines.size() > 0)
		{
			// Get the first unused line and add it to the new block
			// It will also be used to test which additional lines should 
			// added to this block
			UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipTestLine = vecUnusedLines[0];

			// create a new block
			UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipNewBlock(CLSID_SpatialString);
			ASSERT_RESOURCE_ALLOCATION("ELI25870", ipNewBlock != NULL);

			ipNewBlock->Append(ipTestLine);
			ipNewBlock->AppendString(get_bstr_t("\r\n"));

			// This stores the number of lines in the current block
			// one (ipTestLine) has already been added
			long nNumLinesInBlock = 1;

			ILongRectanglePtr ipTestRect = ipTestLine->GetOCRImageBounds();
			ASSERT_RESOURCE_ALLOCATION("ELI25871", ipTestRect != NULL);

			long lTestLeft, lTestTop, lTestRight, lTestBottom;
			ipTestRect->GetBounds(&lTestLeft, &lTestTop, &lTestRight, &lTestBottom);

			// lTestCenter must be the center of any center justified block
			// that contains ipTestLine
			long lTestCenter = (lTestRight + lTestLeft) / 2;

			long lTestLineHeight = ipTestLine->GetAverageLineHeight();

			// The maximum error that will be allowed in determining a 
			// justified block
			long lMaxJustError = ipTestLine->GetAverageCharWidth()*4;

			// represents the best known justification of the current block
			enum EJustification
			{
				kUnknown,
				kLeft,
				kCenter
			} eJust;

			eJust = kUnknown;
			for (size_t j = 1; j < vecUnusedLines.size(); j++)
			{
				UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipLine = vecUnusedLines[j];
				ASSERT_RESOURCE_ALLOCATION("ELI25872", ipLine != NULL);

				ILongRectanglePtr ipRect = ipLine->GetOCRImageBounds();
				long lLeft, lTop, lBottom, lRight;
				ipRect->GetBounds(&lLeft, &lTop, &lRight, &lBottom);
				long lCenter = (lRight + lLeft) / 2;

				bool bPassLeft = (abs(lLeft - lTestLeft) < lMaxJustError);
				bool bPassCenter = (abs(lCenter - lTestCenter) < lMaxJustError);
				bool bPassHeight = (lBottom >= lTestTop && lTop < (lTestBottom + lTestLineHeight*1));

				bool bSuccess = false;
				if (eJust == kUnknown)
				{
					if (bPassLeft && bPassCenter)
					{
						bSuccess = true;
					}
					else if (bPassLeft)
					{
						bSuccess = true;
						eJust = kLeft;
					}
					else if (bPassCenter)
					{
						bSuccess = true;
						eJust = kCenter;
					}
				}
				else if (eJust == kLeft && bPassLeft)
				{
					bSuccess = true;
				}
				else if (eJust == kCenter && bPassCenter)
				{
					bSuccess = true;
				}

				if (bSuccess && bPassHeight)
				{
					nNumLinesInBlock++;
					ipNewBlock->Append(ipLine);
					// add a newline between lines
					ipNewBlock->AppendString(get_bstr_t("\r\n"));
					lTestTop = lTop;
					lTestBottom = lBottom;
				}
				else
				{
					vecTmpLines.push_back(ipLine);
				}	
			}

			if (nNumLinesInBlock >= nMinLines)
			{
				ipNewBlocks->PushBack(ipNewBlock);
			}

			vecUnusedLines.clear();
			for (unsigned int j = 0; j < vecTmpLines.size(); j++)
			{
				vecUnusedLines.push_back(vecTmpLines[j]);
			}
			vecTmpLines.clear();
		}
		*ppResultVector = ipNewBlocks.Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08850");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetBlocks(long nMinLines, IIUnknownVector** ppResultVector)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI20652", ppResultVector != NULL);

		// Check license
		validateLicense();

		// Make sure this string is spatial
		verifySpatialness();

		// This will hold all of the blocks we create
		// And will eventually be returned to the caller
		IIUnknownVectorPtr ipNewBlocks(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI08852", ipNewBlocks != NULL);
		
		// These will be used to hold lines temporarily during block creation
		vector<UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr> vecUnusedLines;
		vector<UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr> vecTmpLines;
	
		// get the lines from this string
		IIUnknownVectorPtr ipLines = getLinesUnknownVector();
		ASSERT_RESOURCE_ALLOCATION("ELI15087", ipLines != NULL);
		
		// Put the lines from this spatial string into the 
		// unused lines Vector
		vecUnusedLines.clear();
		
		long lLineCount = ipLines->Size();
		for (long i = 0; i < lLineCount; i++)
		{
			UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipLine = ipLines->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI08932", ipLine != NULL);
			vecUnusedLines.push_back(ipLine);
		}
		vecTmpLines.clear();
		
		// While there are lines that have not been assigned a block
		while (vecUnusedLines.size() > 0)
		{
			// create a new block
			UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipNewBlock(CLSID_SpatialString);
			ASSERT_RESOURCE_ALLOCATION("ELI25873", ipNewBlock != NULL);

			long nNumLinesInBlock = 0;

			// Get the first unused line and add it to the new block
			// It will also be used to test which additional lines should 
			// added to this block
			UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipTestLine = vecUnusedLines[0];
			ASSERT_RESOURCE_ALLOCATION("ELI25874", ipTestLine != NULL);
			
			ipNewBlock->Append(ipTestLine);
			ipNewBlock->AppendString(get_bstr_t("\r\n"));

			ILongRectanglePtr ipTestRect = ipTestLine->GetOCRImageBounds();
			ASSERT_RESOURCE_ALLOCATION("ELI25875", ipTestRect != NULL);
			RECT rectTest;
			ipTestRect->GetBounds(&(rectTest.left), &(rectTest.top),
				&(rectTest.right), &(rectTest.bottom));

			long lTestTop = rectTest.top;
			long lTestBottom = rectTest.bottom;

			long lTestLineHeight = ipTestLine->GetAverageLineHeight();
			for (size_t j = 1; j < vecUnusedLines.size(); j++)
			{
				UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipLine = vecUnusedLines[j];
				ILongRectanglePtr ipRect = ipLine->GetOCRImageBounds();
				RECT rect;
				ipRect->GetBounds(&(rect.left), &(rect.top), &(rect.right), &(rect.bottom));
				long lTop = rect.top;
				long lBottom = rect.bottom;

				// Ensure that the height is not too far apart 
				if (lBottom >= lTestTop && overlapX(rectTest, rect, 50))
				{
					// Add this line to the current block
					ipNewBlock->Append(ipLine);
					ipNewBlock->AppendString(get_bstr_t("\r\n"));
					// the block is now one line longer
					nNumLinesInBlock++;

					// expand the boundaries of the new block according to the new line
					if (rect.left < rectTest.left)
					{
						rectTest.left = rect.left;
					}
					if (rect.right > rectTest.right)
					{
						rectTest.right = rect.right;
					}
					lTestTop = lTop;
					lTestBottom = lBottom;

					lTestLineHeight = ipLine->GetAverageLineHeight();
				}
				else
				{
					vecTmpLines.push_back(ipLine);
				}
			}

			// add the block to our vector of blocks if it has enough lines
			if (nNumLinesInBlock >= nMinLines)
			{
				ipNewBlocks->PushBack(ipNewBlock);
			}

			// Put all the lines we still haven't used 
			// back into the used lines vector
			vecUnusedLines.clear();
			for (unsigned int j = 0; j < vecTmpLines.size(); j++)
			{
				vecUnusedLines.push_back(vecTmpLines[j]);
			}
			vecTmpLines.clear();
		}
		*ppResultVector = ipNewBlocks.Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08851");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetNextOCRImageSpatialLetter(long nStartPos, ILetter** pLetter, long* pIndex)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI20651", pIndex != NULL);

		// Check license
		validateLicense();

		CPPLetter letter;
		long nIndex = getNextOCRImageSpatialLetter(nStartPos, letter);

		// If found the next spatial letter and pLetter != NULL then return the letter
		if (nIndex != -1 && pLetter != NULL)
		{
			UCLID_RASTERANDOCRMGMTLib::ILetterPtr ipLetter(CLSID_Letter);
			ASSERT_RESOURCE_ALLOCATION("ELI25876", ipLetter != NULL);

			ipLetter->CreateFromCppLetter((void*)&letter);
			*pLetter = (ILetter*) ipLetter.Detach();
		}

		// Set the index
		*pIndex = nIndex;
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08966");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetNextNonSpatialLetter(long nStartPos, ILetter** pLetter, long* pIndex)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI20649", pIndex != NULL);

		// Check license
		validateLicense();

		verifyValidIndex(nStartPos);

		long lIndex = -1;
		if (m_eMode != kSpatialMode)
		{
			// All characters in a non-spatial or hybrid string are non-spatial
			// just return the next character
			char c = m_strString[nStartPos];
			lIndex = nStartPos;

			if (pLetter)
			{
				CPPLetter letter(c, c, c, 0, 0, 0, 0, -1, false, false, false, 0, 100, 0);
				UCLID_RASTERANDOCRMGMTLib::ILetterPtr ipLetter(CLSID_Letter);
				ASSERT_RESOURCE_ALLOCATION("ELI25877", ipLetter != NULL);
				ipLetter->CreateFromCppLetter(&letter);
				*pLetter = (ILetter*) ipLetter.Detach();
			}
		}
		else
		{
			long lSize = m_vecLetters.size();
			for (long i = nStartPos; i < lSize; i++)
			{
				CPPLetter& letter = m_vecLetters[i];
				if (!letter.m_bIsSpatial)
				{
					lIndex = i;
					if (pLetter)
					{
						UCLID_RASTERANDOCRMGMTLib::ILetterPtr ipLetter(CLSID_Letter);
						ASSERT_RESOURCE_ALLOCATION("ELI25878", ipLetter != NULL);
						ipLetter->CreateFromCppLetter((void*) &letter);
						*pLetter = (ILetter*)ipLetter.Detach();
					}
				}
			}
		}
		*pIndex = lIndex;
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08969");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetIsEndOfWord(long nIndex, VARIANT_BOOL* pbIsEnd)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI20647", pbIsEnd != NULL);

		// Check license
		validateLicense();

		verifyValidIndex(nIndex);
		
		*pbIsEnd = asVariantBool( getIsEndOfWord(nIndex) );
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08984");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetIsEndOfLine(long nIndex, VARIANT_BOOL* pbIsEnd)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI20646", pbIsEnd != NULL);

		// Check license
		validateLicense();

		verifyValidIndex(nIndex);
		
		*pbIsEnd = asVariantBool( getIsEndOfLine(nIndex) );
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI08985");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetPageInfo(long nPageNum, ISpatialPageInfo** ppPageInfo)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		ASSERT_ARGUMENT("ELI20645", ppPageInfo != NULL);

		// Check license
		validateLicense();

		// Verify that the page is valid
		if (nPageNum < 1 )
		{
			UCLIDException ue("ELI15080", "Invalid page number!");
			ue.addDebugInfo("Page:", asString( nPageNum ) );
			throw ue;
		}

		// Make sure this string is not non-spatial
		if( m_eMode == kNonSpatialMode )
		{
			UCLIDException ue( "ELI14751", "GetPageInfo() requires a spatial string with spatial information!");
			throw ue;
		}

		// Return the specified page info
		UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr ipPageInfo =
			m_ipPageInfoMap->GetValue(nPageNum);
		ASSERT_RESOURCE_ALLOCATION("ELI09125", ipPageInfo != NULL);
		*ppPageInfo = (ISpatialPageInfo*)ipPageInfo.Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11227");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::SetPageInfo(long nPageNum, ISpatialPageInfo* pPageInfo)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		// Check license
		validateLicense();

		// Verify that the page info is not null
		ASSERT_ARGUMENT("ELI15318", pPageInfo != NULL );

		// Verify that the page is valid
		if (nPageNum < 1 )
		{
			UCLIDException ue("ELI15079", "Invalid page number!");
			ue.addDebugInfo("Page:", asString( nPageNum ) );
			throw ue;
		}

		// Make sure this string is not non-spatial
		if( m_eMode == kNonSpatialMode )
		{
			UCLIDException ue( "ELI14752", "SetPageInfo() requires a spatial string with spatial information!");
			throw ue;
		}
		 
		getPageInfoMap()->Set(nPageNum, (IUnknown*)pPageInfo);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09124");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetOCRImageRasterZones(IIUnknownVector** ppRasterZones)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	try
	{
		ASSERT_ARGUMENT("ELI25879", ppRasterZones != NULL);

		// Check license
		validateLicense();

		IIUnknownVectorPtr ipZones = getOCRImageRasterZonesUnknownVector();
		ASSERT_RESOURCE_ALLOCATION("ELI25880", ipZones != NULL);

		*ppRasterZones = ipZones.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25881");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetOriginalImageRasterZones(IIUnknownVector** ppRasterZones)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	try
	{
		ASSERT_ARGUMENT("ELI20644", ppRasterZones != NULL);

		// Check license
		validateLicense();

		// This operation requires that this object has spatial information
		if( m_eMode == kNonSpatialMode )
		{
			UCLIDException ue( "ELI14764", "GetRasterZones() requires a spatial string!");
			throw ue;
		}

		// This vector will hold the raster zones and be returned to the caller
		IIUnknownVectorPtr ipZones = getOriginalImageRasterZones();
		ASSERT_RESOURCE_ALLOCATION("ELI09137", ipZones != NULL);

		// Detach the created vector of RasterZones and return.
		*ppRasterZones = ipZones.Detach();
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09123");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetTranslatedImageRasterZones(ILongToObjectMap* pPageInfoMap,
														   IIUnknownVector** ppRasterZones)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	try
	{
		ASSERT_ARGUMENT("ELI28033", ppRasterZones != NULL);
		
		ILongToObjectMapPtr ipPageInfoMap(pPageInfoMap);
		ASSERT_ARGUMENT("ELI28034", ipPageInfoMap != NULL);

		// Check license
		validateLicense();

		// This operation requires that this object has spatial information
		if( m_eMode == kNonSpatialMode )
		{
			UCLIDException ue( "ELI28035", "GetRasterZones() requires a spatial string!");
			throw ue;
		}

		// This vector will hold the raster zones and be returned to the caller
		IIUnknownVectorPtr ipZones = getTranslatedImageRasterZones(ipPageInfoMap);
		ASSERT_RESOURCE_ALLOCATION("ELI28036", ipZones != NULL);

		// Detach the created vector of RasterZones and return.
		*ppRasterZones = ipZones.Detach();
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28037");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetFirstPageNumber(long* pRet)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		ASSERT_ARGUMENT("ELI20643", pRet != NULL);

		// Check license
		validateLicense();

		*pRet = getFirstPageNumber();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19467");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetLastPageNumber(long* pRet)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	try
	{
		ASSERT_ARGUMENT("ELI20642", pRet != NULL);

		// Check license
		validateLicense();

		*pRet = getLastPageNumber();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09179");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetCharConfidence(long* pnMinConfidence, long* pnMaxConfidence, 
											long* pnAvgConfidence)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();	
		
		// Set the intitial values
		long nMinConfidence = 100;
		long nMaxConfidence = 0;
		long nAvgConfidence = 0;

		// For a kSpatialMode object
		if (m_eMode == kSpatialMode)
		{
			long nTotalConfidence = 0;
			long nTotalLetters = 0;

			// For each letter, calculate the total confidence of it
			for (unsigned int i = 0; i < m_vecLetters.size(); i++)
			{
				const CPPLetter& letter = m_vecLetters[i];
				if (letter.m_bIsSpatial && !isWhitespaceChar(letter.m_usGuess1))
				{
					// Add the confidence for the letter and increment the number of letters
					nTotalConfidence += letter.m_ucCharConfidence;
					nTotalLetters++;

					if (letter.m_ucCharConfidence < nMinConfidence)
					{
						nMinConfidence = letter.m_ucCharConfidence;
					}

					if (letter.m_ucCharConfidence > nMaxConfidence)
					{
						nMaxConfidence = letter.m_ucCharConfidence;
					}
				}
			}

			// If there was at least one letter, get the average confidence
			if (nTotalLetters > 0)
			{
				nAvgConfidence = nTotalConfidence / nTotalLetters;
			}
			else
			{
				// Empty strings are zero confidence [FIDSC #4009]
				nMinConfidence = 0;
			}
		}
		else
		{
			// For non-spatial or hybrid strings, the confidence is 100,
			// unless the string is empty [FIDSC #4009]
			long lConfidence = m_strString.empty() ? 0 : 100;

			nMinConfidence = lConfidence;
			nMaxConfidence = lConfidence;
			nAvgConfidence = lConfidence;
		}

		// If there is some min confidence, return it.
		if (pnMinConfidence)
		{
			*pnMinConfidence = nMinConfidence;
		}

		// If there is some max confidence, return it.
		if (pnMaxConfidence)
		{
			*pnMaxConfidence = nMaxConfidence;
		}

		// If there is some avg confidence, return it.
		if (pnAvgConfidence)
		{
			*pnAvgConfidence = nAvgConfidence;
		}
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10625")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetFontSizeDistribution(ILongToLongMap** ppMap)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI20638", ppMap != NULL);

		// Validate license first
		validateLicense();

		map<long, long> mapFontSizeToCount;

		// Non spatial mode and hybrid mode don't have a letters vector to use
		// so they are treated as a special case.
		if( (m_eMode == kNonSpatialMode) || (m_eMode == kHybridMode) )
		{
			mapFontSizeToCount[0] = m_strString.size();
		}
		else
		{
			// Mode must be spatial to utilize this method
			for (unsigned int i = 0; i < m_vecLetters.size(); i++)
			{
				CPPLetter& letter = m_vecLetters[i];
				mapFontSizeToCount[letter.m_ucFontSize]++;
			}
		}

		// Make a map
		ILongToLongMapPtr ipMap(CLSID_LongToLongMap);
		ASSERT_RESOURCE_ALLOCATION("ELI10659", ipMap != NULL);

		map<long, long>::iterator it;

		// For each letter, set the map for it
		for(it = mapFontSizeToCount.begin(); it != mapFontSizeToCount.end(); it++)
		{
			ipMap->Set(it->first, it->second);
		}

		*ppMap = ipMap.Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10658");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetFontInfo(long nMinPercentage, VARIANT_BOOL* pbItalic, 
										 VARIANT_BOOL* pbBold, VARIANT_BOOL* pbSansSerif, 
										 VARIANT_BOOL* pbSerif, VARIANT_BOOL* pbProportional, 
										 VARIANT_BOOL* pbUnderline, VARIANT_BOOL* pbSuperScript, 
										 VARIANT_BOOL* pbSubScript)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		// Initialize some values for each type of font
		long nNumItalic = 0, nNumBold = 0, nNumSansSerif = 0, nNumSerif = 0, 
			nNumProportional = 0, nNumUnderline = 0, nNumSuperScript = 0, nNumSubScript = 0;
		long nNumSpatialChars = 0;

		for (unsigned int i = 0; i < m_vecLetters.size(); i++)
		{
			// Get the letter
			CPPLetter& letter = m_vecLetters[i];
			if(!letter.m_bIsSpatial)
			{
				continue;
			}

			nNumSpatialChars++;

			// Check the flags for each font possibility
			if(letter.isItalic())
			{
				nNumItalic++;
			}

			if(letter.isBold())
			{
				nNumBold++;
			}

			if(letter.isSansSerif())
			{
				nNumSansSerif++;
			}

			if(letter.isSerif())
			{
				nNumSerif++;
			}

			if(letter.isProportional())
			{
				nNumProportional++;
			}

			if(letter.isUnderline())
			{
				nNumUnderline++;
			}

			if(letter.isSuperScript())
			{
				nNumSuperScript++;
			}

			if(letter.isSubScript())
			{
				nNumSubScript++;
			}
		}

		// Use the above values and use the helper method to check for font info
		if(pbItalic != NULL)
		{
			checkForFontInfo( pbItalic, nNumItalic, nNumSpatialChars, nMinPercentage);
		}

		if(pbBold != NULL)
		{
			checkForFontInfo( pbBold, nNumBold, nNumSpatialChars, nMinPercentage);
		}

		if(pbSansSerif)
		{
			checkForFontInfo( pbSansSerif, nNumSansSerif, nNumSpatialChars, nMinPercentage);
		}

		if(pbSerif != NULL)
		{
			checkForFontInfo( pbSerif, nNumSerif, nNumSpatialChars, nMinPercentage);
		}

		if(pbProportional != NULL)
		{
			checkForFontInfo( pbProportional, nNumProportional, nNumSpatialChars, nMinPercentage);
		}

		if(pbUnderline != NULL)
		{
			checkForFontInfo( pbUnderline, nNumUnderline, nNumSpatialChars, nMinPercentage);
		}

		if(pbSuperScript != NULL)
		{
			checkForFontInfo( pbSuperScript, nNumSuperScript, nNumSpatialChars, nMinPercentage);
		}

		if(pbSubScript != NULL)
		{
			checkForFontInfo( pbSubScript, nNumSubScript, nNumSpatialChars, nMinPercentage);
		}
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10686");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetMode(/*[out, retval]*/ ESpatialStringMode *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI20629", pVal != NULL);

		// Validate license first
		validateLicense();

		*pVal = m_eMode;
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14774");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::HasSpatialInfo(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{		
		ASSERT_ARGUMENT("ELI20628", pbValue != NULL);

		// Check license
		validateLicense();

		*pbValue = asVariantBool(m_eMode != kNonSpatialMode);
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14812")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::IsMultiPage(VARIANT_BOOL* pbRet)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	try
	{
		ASSERT_ARGUMENT("ELI20627", pbRet != NULL);

		// Check license
		validateLicense();

		*pbRet = asVariantBool(isMultiPage());
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09178");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::IsEmpty(VARIANT_BOOL *pvbIsEmpty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI20626", pvbIsEmpty != NULL);

		validateLicense();

		*pvbIsEmpty = asVariantBool( m_strString.empty() );
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16806");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetWordLengthDist(long* plTotalWords, ILongToLongMap** ppWordLengthMap)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI20621", ppWordLengthMap != NULL);

		validateLicense();

		// declare and map to hold the histogram of word counts
		map<long, long> mapLngthToCount;

		long lTotalWords = 0;

		// loop through the entire string
		size_t sztLength = m_strString.length();
		long lCurrentWordLength = 0;
		for (size_t i = 0; i < sztLength; i++)
		{
			// get the current letter 
			char cLetter = m_strString[i];

			// if its whitespace and the word length is not 0,
			// increment the word count for the current word size
			// set current word length back to 0 and increment total
			// words found count
			if (isWhitespaceChar(cLetter))
			{
				if (lCurrentWordLength != 0)
				{
					mapLngthToCount[lCurrentWordLength]++;
					lCurrentWordLength = 0;
					lTotalWords++;
				}
			}
			else
			{
				lCurrentWordLength++;
			}
		}

		// make sure we add the last word
		if (lCurrentWordLength != 0)
		{
			mapLngthToCount[lCurrentWordLength]++;
			lTotalWords++;
		}

		ILongToLongMapPtr ipMapLngthToCount(CLSID_LongToLongMap);
		ASSERT_RESOURCE_ALLOCATION("ELI20622", ipMapLngthToCount != NULL);

		for (map<long, long>::iterator it = mapLngthToCount.begin();
			it != mapLngthToCount.end(); it++)
		{
			ipMapLngthToCount->Set(it->first, it->second);
		}

		if (plTotalWords != NULL)
		{
			*plTotalWords = lTotalWords;
		}

		*ppWordLengthMap = ipMapLngthToCount.Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20620");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetOCRImageRasterZonesGroupedByConfidence(
					IVariantVector* pVecOCRConfidenceBoundaries, 
					IVariantVector** ppZoneOCRConfidenceTiers,
					IIUnknownVector** ppRasterZones)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		IVariantVectorPtr ipVecOCRConfidenceBoundaries(pVecOCRConfidenceBoundaries);
		ASSERT_ARGUMENT("ELI25369", ipVecOCRConfidenceBoundaries != NULL);
		ASSERT_ARGUMENT("ELI25370", ppZoneOCRConfidenceTiers != NULL);
		ASSERT_ARGUMENT("ELI25371", ppRasterZones != NULL);

		// Check license
		validateLicense();

		IVariantVectorPtr ipZoneOCRConfidenceTiers(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI25372", ipZoneOCRConfidenceTiers != NULL);

		// Retrieve a list like GetRasterZones, except that raster zones are split between letters
		// on opposite sides of a specified OCR confidence boundary.
		IIUnknownVectorPtr ipZones = getOCRImageRasterZonesGroupedByConfidence(
			ipVecOCRConfidenceBoundaries, ipZoneOCRConfidenceTiers);
		ASSERT_RESOURCE_ALLOCATION("ELI25367", ipZones != NULL);

		// Return the raster zones as well as a vector specifying the OCR confidence tier of each
		// zone.
		*ppZoneOCRConfidenceTiers = ipZoneOCRConfidenceTiers.Detach();
		*ppRasterZones = ipZones.Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25368");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetOriginalImageRasterZonesGroupedByConfidence(
					IVariantVector* pVecOCRConfidenceBoundaries, 
					IVariantVector** ppZoneOCRConfidenceTiers,
					IIUnknownVector** ppRasterZones)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		IVariantVectorPtr ipVecOCRConfidenceBoundaries(pVecOCRConfidenceBoundaries);
		ASSERT_ARGUMENT("ELI25705", ipVecOCRConfidenceBoundaries != NULL);
		ASSERT_ARGUMENT("ELI25706", ppZoneOCRConfidenceTiers != NULL);
		ASSERT_ARGUMENT("ELI25707", ppRasterZones != NULL);

		// Check license
		validateLicense();

		IVariantVectorPtr ipZoneOCRConfidenceTiers(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI25708", ipZoneOCRConfidenceTiers != NULL);

		// Retrieve a list like GetRasterZones, except that raster zones are split between letters
		// on opposite sides of a specified OCR confidence boundary.
		IIUnknownVectorPtr ipZones = getOriginalImageRasterZonesGroupedByConfidence(
			ipVecOCRConfidenceBoundaries, ipZoneOCRConfidenceTiers);
		ASSERT_RESOURCE_ALLOCATION("ELI25709", ipZones != NULL);

		// Return the raster zones as well as a vector specifying the OCR confidence tier of each
		// zone.
		*ppZoneOCRConfidenceTiers = ipZoneOCRConfidenceTiers.Detach();
		*ppRasterZones = ipZones.Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25710");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetOCRImageBounds(ILongRectangle** ppBounds)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI25711", ppBounds != NULL);

		// Check license
		validateLicense();

		// this operation requires that this string is a string with spatial info associated with it
		if( m_eMode == kNonSpatialMode)
		{
			UCLIDException ue("ELI25712", "Unable to get bounds on a non-spatial string!");
			throw ue;
		}

		// This operation also requires that IsMultiPage == VARIANT_FALSE
		if(isMultiPage())
		{
			UCLIDException ue("ELI25713", "Spatial String must not be multi-page for GetBounds!");
			throw ue;
		}

		// Declare the long rectangle to hold the return value
		ILongRectanglePtr ipLongRectangle = NULL;

		// Get the Raster zones for this spatial string
		IIUnknownVectorPtr ipZones = getOCRImageRasterZonesUnknownVector();
		ASSERT_RESOURCE_ALLOCATION("ELI25882", ipZones != NULL);
		if (ipZones->Size() > 0)
		{
			// Get the first raster zone
			UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipZone = ipZones->At(0);
			ASSERT_RESOURCE_ALLOCATION("ELI25714", ipZone != NULL);

			// Get a bounding rectangle for the raster zones (do not restrict the rectangle)
			ipLongRectangle = ipZone->GetBoundsFromMultipleRasterZones(ipZones, NULL);
			ASSERT_RESOURCE_ALLOCATION("ELI25716", ipLongRectangle != NULL);
		}
		else
		{
			// There are no raster zones, just return an empty rectangle
			ipLongRectangle.CreateInstance(CLSID_LongRectangle);
			ASSERT_RESOURCE_ALLOCATION("ELI25717", ipLongRectangle != NULL);
		}

		// return the long rectangle
		*ppBounds = ipLongRectangle.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25718")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetTranslatedImageBounds(ILongToObjectMap* pPageInfoMap,
													  ILongRectangle** ppBounds)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI28038", ppBounds != NULL);

		ILongToObjectMapPtr ipPageInfoMap(pPageInfoMap);
		ASSERT_ARGUMENT("ELI28039", ipPageInfoMap != NULL);

		// Check license
		validateLicense();

		// this operation requires that this string is a string with spatial info associated with it
		if( m_eMode == kNonSpatialMode)
		{
			UCLIDException ue("ELI28040", "Unable to get bounds on a non-spatial string!");
			throw ue;
		}

		// This operation also requires that IsMultiPage == VARIANT_FALSE
		if(isMultiPage())
		{
			UCLIDException ue("ELI28041", "Spatial String must not be multi-page for GetBounds!");
			throw ue;
		}

		// Declare the long rectangle to hold the return value
		ILongRectanglePtr ipLongRectangle = NULL;

		// Get the Raster zones for this spatial string
		IIUnknownVectorPtr ipZones = getTranslatedImageRasterZones(ipPageInfoMap);
		ASSERT_RESOURCE_ALLOCATION("ELI28042", ipZones != NULL);
		if (ipZones->Size() > 0)
		{
			// Get the first raster zone
			UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipZone = ipZones->At(0);
			ASSERT_RESOURCE_ALLOCATION("ELI28043", ipZone != NULL);

			// Get a bounding rectangle for the raster zones (do not restrict the rectangle)
			ipLongRectangle = ipZone->GetBoundsFromMultipleRasterZones(ipZones, NULL);
			ASSERT_RESOURCE_ALLOCATION("ELI28044", ipLongRectangle != NULL);
		}
		else
		{
			// There are no raster zones, just return an empty rectangle
			ipLongRectangle.CreateInstance(CLSID_LongRectangle);
			ASSERT_RESOURCE_ALLOCATION("ELI28045", ipLongRectangle != NULL);
		}

		// return the long rectangle
		*ppBounds = ipLongRectangle.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28046")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::get_OCREngineVersion(BSTR *pbstrOCREngine)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		ASSERT_ARGUMENT("ELI28308", pbstrOCREngine != NULL);

		*pbstrOCREngine = _bstr_t(m_strOCREngineVersion.c_str()).Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28309");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::put_OCREngineVersion(BSTR bstrOCREngine)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_strOCREngineVersion = asString(bstrOCREngine);
		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI28310");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::MergeAsHybridString(ISpatialString* pStringToMerge)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipStringToMerge(pStringToMerge);
		ASSERT_ARGUMENT("ELI29868", ipStringToMerge != NULL);

		mergeAsHybridString(ipStringToMerge);

		m_bDirty = true;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI29869");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetOriginalImagePageBounds(long nPageNum, ILongRectangle** ppBounds)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI30331", ppBounds != NULL);

		ILongRectanglePtr ipBounds = getPageBounds(nPageNum, false);
		
		*ppBounds = ipBounds.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI30325");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::GetOCRImagePageBounds(long nPageNum, ILongRectangle** ppBounds)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI30332", ppBounds != NULL);

		ILongRectanglePtr ipBounds = getPageBounds(nPageNum, true);

		*ppBounds = ipBounds.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI30326");
}
//-------------------------------------------------------------------------------------------------
