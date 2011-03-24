// SpatialString.cpp : Implementation of CSpatialString
#include "stdafx.h"
#include "SpatialString.h"
#include "UCLIDRasterAndOCRMgmt.h"
#include "RasterZone.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <MiscLeadUtils.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const double gfORDER_MIN_OVERLAP_PERCENT = 20;

//-------------------------------------------------------------------------------------------------
// ISpatialString
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::IsSpatiallyLessThan(ISpatialString* pSS, VARIANT_BOOL *pbRetVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		// Make sure this object is in kSpatialMode
		if( m_eMode == kNonSpatialMode )
		{
			UCLIDException ue( "ELI14761", "Unable to spatially compare a non-spatial string!");
			throw ue;
		}

		// From here on the suffix S0 on variable will mean it applies to this string
		// and the suffix S1 on a string will mean it applies to the input string
		UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipS1(pSS);
		ASSERT_RESOURCE_ALLOCATION("ELI11250", ipS1 != NULL);

		// Make sure that the other spatial string has spatial info
		if( ipS1->HasSpatialInfo() != VARIANT_TRUE )
		{
			UCLIDException ue( "ELI14827", "Unable to spatially compare a non-spatial string!");
			throw ue;
		}

		// check for page number discrepancies
		long nFirstPageS0 = getFirstPageNumber(); // S0 is "this" string so just use local method
		long nFirstPageS1 = ipS1->GetFirstPageNumber();
		if (nFirstPageS0 < nFirstPageS1)
		{
			*pbRetVal = VARIANT_TRUE;
			return S_OK;
		}
		else if (nFirstPageS1 < nFirstPageS0)
		{
			*pbRetVal = VARIANT_FALSE;
			return S_OK;
		}

		if (nFirstPageS0 != nFirstPageS1)
		{
			THROW_LOGIC_ERROR_EXCEPTION("ELI11256");
		}

		UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipS0(this);
		ASSERT_RESOURCE_ALLOCATION("ELI11251", ipS0 != NULL);

		// now we know that both strings start on the same page
		// so we will get their bounding boxes on that page
		UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipFirstPageS0 =
			ipS0->GetSpecifiedPages(nFirstPageS0, nFirstPageS0);
		ASSERT_RESOURCE_ALLOCATION("ELI25970", ipFirstPageS0 != NULL);
		ILongRectanglePtr ipRectS0 = ipFirstPageS0->GetOriginalImageBounds();
		ASSERT_RESOURCE_ALLOCATION("ELI25971", ipRectS0 != NULL);
		
		UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipFirstPageS1 =
			ipS1->GetSpecifiedPages(nFirstPageS1, nFirstPageS1);
		ASSERT_RESOURCE_ALLOCATION("ELI25972", ipFirstPageS1 != NULL);
		ILongRectanglePtr ipRectS1 = ipFirstPageS1->GetOriginalImageBounds();
		ASSERT_RESOURCE_ALLOCATION("ELI25973", ipRectS1 != NULL);

		// Get the values from the rectangles
		long nLeftS0, nLeftS1, nTopS0, nTopS1, nRightTemp, nBottomS0, nBottomS1;
		ipRectS0->GetBounds(&nLeftS0, &nTopS0, &nRightTemp, &nBottomS0);
		ipRectS1->GetBounds(&nLeftS1, &nTopS1, &nRightTemp, &nBottomS1);

		// if the boxes do not over lap, the one "on-top" (higher on the page) 
		// will be considered less
		if (nBottomS0 < nTopS1)
		{
			*pbRetVal = VARIANT_TRUE;
			return S_OK;
		}
		else if (nBottomS1 < nTopS0)
		{
			*pbRetVal = VARIANT_FALSE;
			return S_OK;
		}

		// if the overlap is less than X% of smaller zone then
		// we will say the zone that starts higher on the page is less
		long nHeightS0 = nBottomS0 - nTopS0;
		long nHeightS1 = nBottomS1 - nTopS1;

		long nMinHeight = nHeightS0 < nHeightS1 ? nHeightS0 : nHeightS1;

		long nDiff0 = nBottomS0 - nTopS1;
		long nDiff1 = nBottomS1 - nTopS0;

		// Note that this overlap can be greater than the nMinHeight 
		// in the case where one zone is totally contained (in y) 
		// within the other.  We will cap it at nMinHeight.
		long nOverlap = nDiff0 < nDiff1 ? nDiff0 : nDiff1;
		nOverlap = nMinHeight < nOverlap ? nMinHeight : nOverlap;

		double dPercentOverlap = 100.0 * (double)nOverlap / (double)nMinHeight;

		if (dPercentOverlap >= gfORDER_MIN_OVERLAP_PERCENT)
		{
			if (nLeftS0 < nLeftS1)
			{
				*pbRetVal = VARIANT_TRUE;
				return S_OK;
			}
			else if (nLeftS1 < nLeftS0)
			{
				*pbRetVal = VARIANT_FALSE;
				return S_OK;
			}
		}

		if (nTopS0 < nTopS1)
		{
			*pbRetVal = VARIANT_TRUE;
			return S_OK;
		}
		else if (nTopS1 < nTopS0)
		{
			*pbRetVal = VARIANT_FALSE;
			return S_OK;
		}
		
		*pbRetVal = VARIANT_FALSE;

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11249");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::Offset(long nX, long nY)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		// Make sure this string is spatial
		verifySpatialness();

		// if this string is spatial, then apply the offset
		// to all the underlying letter objects
		if( m_eMode == kSpatialMode )
		{
			long nNumLetters = m_vecLetters.size();
			for (long i = 0; i < nNumLetters; i++)
			{
				CPPLetter& letter = m_vecLetters[i];

				if (letter.m_bIsSpatial)
				{
					letter.m_ulTop += nY;
					letter.m_ulLeft += nX;
					letter.m_ulRight += nX;
					letter.m_ulBottom += nY;
				}
			}//end for
		}
		else if( m_eMode == kHybridMode)
		{
			long nRasterZones = m_vecRasterZones.size();

			for(long i = 0; i < nRasterZones; i++)
			{
				// Make a RasterZone and get a RZ to work with
				UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipZone = m_vecRasterZones[i];
				ASSERT_RESOURCE_ALLOCATION("ELI19465", ipZone != NULL);
				
				long lStartX, lStartY, lEndX, lEndY, lHeight, lPageNumber;
				ipZone->GetData(&lStartX, &lStartY, &lEndX, &lEndY, &lHeight, &lPageNumber);

				// Shift the raster zone(s) by the specified offset
				ipZone->CreateFromData(lStartX + nX, lStartY + nY,
					lEndX + nX, lEndY + nY, lHeight, lPageNumber);
			}//end for
		}
		else
		{
			UCLIDException ue("ELI15054", "Unable to offset zones of a non-spatial string!");
			ue.addDebugInfo("Mode:", asString( m_eMode ));
			throw ue;
		}

		// set the dirty flag to true since a modification was made
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06662")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::UpdatePageNumber(long nPageNumber)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate license
		validateLicense();

		if( nPageNumber < 1 )
		{
			UCLIDException ue("ELI15057", "Invalid page number for update!");
			ue.addDebugInfo("Page:", asString( nPageNumber ) );
			throw ue;			
		}

		// this method is only appropriate for spatial strings
		switch( m_eMode )
		{
			case kNonSpatialMode:
			{
				// Not possible to update page number on a non-spatial string
				UCLIDException ue( "ELI14760", "Unable to update page number on a non-spatial string!");
				throw ue;
			}
			break;
			case kSpatialMode:
			{
				// Set the page number for all the Letters in the vector
				long nNumLetters = m_vecLetters.size();
				for (long i = 0; i < nNumLetters; i++)
				{
					m_vecLetters[i].m_usPageNumber = (unsigned short) nPageNumber;
				}
			}
			break;
			case kHybridMode:
			{
				// Set the page number for all the Raster Zones in the vector
				long nNumRasterZones = m_vecRasterZones.size();
				for(long i = 0; i < nNumRasterZones; i++ )
				{
					// Get each raster zone from the vector of Raster zones
					UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipZone = m_vecRasterZones[i];
					ASSERT_RESOURCE_ALLOCATION( "ELI14797", ipZone);
						
					// Update the page number for this raster zone
					ipZone->PageNumber = nPageNumber;
				}
			}
			break;
			default:
			{
				UCLIDException ue("ELI15058", "Invalid mode for spatial string!");
				ue.addDebugInfo("Mode:", asString( m_eMode ) );
				throw ue;
			}
			break;
		}

		// Update the spatial page info.
		// If there is one page, update it's page number to the new value.
		if( m_ipPageInfoMap->GetSize() == 1 )
		{
			// Since there is only one key, it's the only page in the page info map
			// Get the page info for the single page
			long nCurrentPageNumber = 0;
			UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr ipPageInfo;
			m_ipPageInfoMap->GetKeyValue(0, &nCurrentPageNumber, (IUnknown**)&ipPageInfo);
			ASSERT_RESOURCE_ALLOCATION("ELI15257", ipPageInfo != NULL);

			// If the keys do not match, set the current page number to the new page number
			if( nCurrentPageNumber != nPageNumber )
			{
				m_ipPageInfoMap->RenameKey( nCurrentPageNumber, nPageNumber );
			}
		}

		// set the dirty flag to true since a modification was made
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06911");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::SelectWithFontSize(VARIANT_BOOL bInclude, long nMinFontSize, 
												long nMaxFontSize, ISpatialString** ppResultString)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Make sure this string is spatial
		if( m_eMode != kSpatialMode )
		{
			UCLIDException ue( "ELI14755", "SelectWithFontSize() requires a spatial string in spatial mode!");
			throw ue;
		}

		if(nMinFontSize < -1 || nMaxFontSize < -1)
		{
			UCLIDException ue("ELI10661", "Invalid Font Size specification.");
			ue.addDebugInfo("MinFontSize", nMinFontSize);
			ue.addDebugInfo("MaxFontSize", nMaxFontSize);
			throw ue;
		}

		// This flag is for determining when whitespace and other non-spatial characters should 
		// be added(or excluded) to the string.  A non-spatial character will only be added if the
		// previous spatial character met the size criteria
		bool bAdd = false;

		vector<CPPLetter> vecNewLetters;
		for (unsigned int i = 0; i < m_vecLetters.size(); i++)
		{
			CPPLetter letter = m_vecLetters[i];
			bool bInRange = false;
			if ((nMinFontSize == -1 || letter.m_ucFontSize >= nMinFontSize) &&
				(nMaxFontSize == -1 || letter.m_ucFontSize <= nMaxFontSize))
			{
				bInRange = true;
			}

			// if we are excluding we want chars outside the fontsize range
			if(bInclude == VARIANT_FALSE)
			{
				bInRange = !bInRange;
			}
		
			if(letter.m_bIsSpatial && bInRange)
			{
				// if bAdd is false at this point we need 
				// to add all preceeding non-spatial characters
				// because they were not added and we 
				// want to preserve all non spatial chars
				// that "touch" a character in the range
				if(!bAdd)
				{
					vector<CPPLetter>::iterator itInsert = vecNewLetters.end();
					int j;
					for (j = i - 1; j >= 0; j--)
					{
						CPPLetter& testLetter = m_vecLetters[j];
						// insert non-spatials
						if(!testLetter.m_bIsSpatial)
						{
							vecNewLetters.insert(itInsert, testLetter);
						}
						else
						{
							// once a spatial char is reached move on
							break;
						}
					}
				}
				vecNewLetters.push_back(letter);
				
				bAdd = true;
			}
			else if(!letter.m_bIsSpatial && bAdd)
			{
				vecNewLetters.push_back(letter);
			}
			else
			{
				bAdd = false;
			}
		}

		UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipNewString(CLSID_SpatialString);
		ASSERT_RESOURCE_ALLOCATION("ELI10615", ipNewString != NULL);

		if (vecNewLetters.size() > 0)
		{
			ipNewString->CreateFromLetterArray(vecNewLetters.size(), &vecNewLetters[0],
				m_strSourceDocName.c_str(), m_ipPageInfoMap);
		}
		else
		{
			ipNewString->SourceDocName = m_strSourceDocName.c_str();
		}

		*ppResultString = (ISpatialString*)ipNewString.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10614")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::DowngradeToNonSpatialMode()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{	
		// Validate license first
		validateLicense();

		downgradeToNonSpatial();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14984");
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSpatialString::DowngradeToHybridMode()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license first
		validateLicense();

		// Down grade the string to hybrid mode
		downgradeToHybrid();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14985");
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
