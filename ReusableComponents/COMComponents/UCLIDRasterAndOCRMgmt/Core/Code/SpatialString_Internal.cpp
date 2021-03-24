// SpatialStringInternal.cpp : Implementation of CSpatialString private methods
#include "stdafx.h"
#include "SpatialString.h"
#include "UCLIDRasterAndOCRMgmt.h"

#include <UCLIDException.h>
#include <COMUtils.h>
#include <cpputil.h>
#include <ComponentLicenseIDs.h>
#include <LicenseMgmt.h>
#include <MiscLeadUtils.h>
#include <CompressionEngine.h>
#include <TemporaryFileName.h>

#include <math.h>
#include <set>

#include <Zipper.h>
#include <Unzipper.h>
#include <rapidjson/document.h>
#include <rapidjson/filereadstream.h>
#include <rapidjson/istreamwrapper.h>
#include <rapidjson/pointer.h>


//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
const CRect grectNULL = CRect(0, 0, 0, 0);
const double gfCharWidthToAvgCharRatio = 1.1;
const string gstrSPATIAL_STRING_FILE_SIGNATURE = "UCLID Spatial String (USS) File";
const _bstr_t gbstrSPATIAL_STRING_STREAM_NAME("SpatialString");

// CPPLetters
const CPPLetter gletterSLASH_R('\r','\r','\r',-1,-1,-1,-1,-1,false,false,false,0,100,0);
const CPPLetter gletterSLASH_N('\n','\n','\n',-1,-1,-1,-1,-1,false,false,false,0,100,0);
const CPPLetter gletterSPACE(' ',' ',' ',-1,-1,-1,-1,-1,false,false,false,0,100,0);
const CPPLetter gletterTAB('\t','\t','\t',-1,-1,-1,-1,-1,false,false,false,0,100,0);

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr CSpatialString::getThisAsCOMPtr()
{
    UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipThis(this);
    ASSERT_RESOURCE_ALLOCATION("ELI16977", ipThis != __nullptr);

    return ipThis;
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::append(UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipStringToAppend)
{
    try
    {
        // Insert the string at the end of the current string
        insert(m_strString.length(), ipStringToAppend);
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25796");
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::insert(long nPos,
                            UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipStringToInsert)
{
    try
    {
        // verify valid argument sent in
        ASSERT_ARGUMENT("ELI25797", ipStringToInsert != __nullptr);

        // Verify that nPos is a valid index
        // If nPos < 0 or nPos is out of bounds on the string, throw an exception
        if ( (nPos < 0) || 
            (nPos > (long)m_strString.length()) )
        {
            // If nPos isn't valid, throw an exception
            UCLIDException ue("ELI14951", "Index out of bounds!");
            ue.addDebugInfo("Index", nPos);
            ue.addDebugInfo("String length", m_strString.length());
            throw ue;
        }

        // Make sure that there is a source doc name and that the new spatial string's
        // source doc name matches this.sourcedocname
        validateAndMergeSourceDocName(asString(ipStringToInsert->SourceDocName));

        // Get the spatial mode of the string to insert
        UCLID_RASTERANDOCRMGMTLib::ESpatialStringMode eSourceMode = ipStringToInsert->GetMode();
        if (m_eMode == kHybridMode || eSourceMode == kHybridMode)
        {
            // At least one of the strings is hybrid, so the result is a hybrid string.

            // If the source string had spatial info, update the page info map
			bool requiresTranslation = false;
            if (eSourceMode != kNonSpatialMode)
            {
                requiresTranslation = updateAndValidateCompatibleSpatialPageInfo(ipStringToInsert->SpatialPageInfos);
            }

            // get the text associated with the hybrid string
            string strTemp = m_strString;
            string strTextToInsert = asString(ipStringToInsert->String);
            strTemp.insert(nPos, strTextToInsert);

            // get the raster zones of the two strings and append them onto the same vector
            vector<UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr> vecCombinedZones;
            if (m_eMode != kNonSpatialMode)
            {
                vector<UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr> vecZones =
                    getOCRImageRasterZones();
                vector<UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr>::iterator it = vecZones.begin();
                for(; it != vecZones.end(); it++)
                {
                    // Get each zone present
                    UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipZone = (*it);
                    ASSERT_RESOURCE_ALLOCATION("ELI15308", ipZone != __nullptr);

                    // Add the raster zone if not already present
                    if (!isRasterZoneInVector(ipZone, vecCombinedZones))
                    {
                        vecCombinedZones.push_back(ipZone);
                    }
                }
            }

            if (eSourceMode != kNonSpatialMode)
            {
				IIUnknownVectorPtr ipZones;
				if (requiresTranslation)
				{
					ipZones = ipStringToInsert->GetTranslatedImageRasterZones(m_ipPageInfoMap);
				}
				else
				{
					ipZones = ipStringToInsert->GetOCRImageRasterZones();
				}
                ASSERT_RESOURCE_ALLOCATION("ELI15309", ipZones != __nullptr);

                long lSize = ipZones->Size();
                for (long i = 0; i < lSize; i++)
                {
                    // Get each zone present
                    UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipZone = ipZones->At(i);
                    ASSERT_RESOURCE_ALLOCATION("ELI15310", ipZone != __nullptr);

                    // Add the raster zone if not already present
                    if (!isRasterZoneInVector(ipZone, vecCombinedZones))
                    {
                        vecCombinedZones.push_back(ipZone);
                    }
                }
            }

            // call updateHybrid to reset this object as a hybrid string
            updateHybrid(vecCombinedZones, strTemp);
        }
        else if (m_eMode == kSpatialMode || eSourceMode == kSpatialMode)
        {
            // At least one string is spatial, the other is either spatial or non-spatial;
            // so the result will be spatial.

            // If the old string was spatial, page info must be properly updated 
            if (eSourceMode != kNonSpatialMode)
            {
                bool requiresTranslation = updateAndValidateCompatibleSpatialPageInfo(ipStringToInsert->SpatialPageInfos);
				if (requiresTranslation)
				{
					ICopyableObjectPtr ipCopyThis(ipStringToInsert);
					ipStringToInsert = ipCopyThis->Clone();
					ipStringToInsert->TranslateToNewPageInfo(m_ipPageInfoMap);
				}
            }

            vector<CPPLetter> vecFinalLetters;
                        
            // If this string is non-spatial, make a non-spatial letter array for it in order
            // to keep the size of the string and the letter array identical.
            if (m_eMode == kNonSpatialMode)
            {
                getNonSpatialLetters(m_strString, vecFinalLetters);
            }
            else
            {
                // If this object is spatial, copy the current letters into a temporary vector
                vecFinalLetters = m_vecLetters;
            }
            
            // Need a couple variables to use for insertion
            vector<CPPLetter> vecLetters;
            CPPLetter* letters = NULL;
            long nNumLetters = 0;

            // Get the vector of letters for the string to insert
            // If the spatial string to insert is spatial, get the letter array
            if( eSourceMode == kSpatialMode)
            {
                // get the letters associated with the other string
                ipStringToInsert->GetOCRImageLetterArray(&nNumLetters, (void**)&letters);

                if (nNumLetters > 0)
                {	
                    vecLetters.resize(nNumLetters);
					// Since the m_vecLetters was just resized to nNumLetters the copy size 
					// is the same
					long lCopySize = sizeof(CPPLetter) * nNumLetters;

					memcpy_s(&(vecLetters[0]), lCopySize, letters, lCopySize);
                }

                // Verify that the letters being inserted fit between the letters already
                // in the string if the existing string is spatial
                if (m_eMode == kSpatialMode)
                {
                    // Get the insertion first page number
                    long lFirstPage = ipStringToInsert->GetFirstPageNumber();

                    // Check if the first page to insert is after or the same
                    // as the first page of the spatial letters
                    for (long i = nPos-1; i >= 0; i--)
                    {
                        CPPLetter& letter = vecFinalLetters[i];
                        if (letter.m_bIsSpatial)
                        {
                            if (lFirstPage < letter.m_usPageNumber)
                            {
                                // [FlexIDSCore:3942]
								// Rather than throwing an exception here, instead convert the
								// spatial string to a hybrid string in which page order doesn't
								// matter.
								downgradeToHybrid();
								insert(nPos, ipStringToInsert);
								return;
                            }

                            break;
                        }
                    }

                    // Get the insertion last page number
                    long lLastPage = ipStringToInsert->GetLastPageNumber();

                    // Check if the last page to insert is before or the same
                    // as the last page of the spatial letters
                    for (size_t i = nPos + vecLetters.size(); i < vecFinalLetters.size(); i++)
                    {
                        CPPLetter& letter = vecFinalLetters[i];
                        if (letter.m_bIsSpatial)
                        {
                            if (lLastPage > letter.m_usPageNumber)
                            {
                                UCLIDException ue("ELI25799",
                                    "Cannot insert string: Page inconsistency");
                                ue.addDebugInfo("Last page of insert", lLastPage);
                                ue.addDebugInfo("Page To Insert Before", letter.m_usPageNumber);
                                throw ue;
                            }

                            break;
                        }
                    }
                }
            }
            // If the spatial string to insert is not spatial, create a letter array from the string.
            else
            {
                string strTemp = asString(ipStringToInsert->String);
                // Note: this call will build the string
                getNonSpatialLetters( strTemp, vecLetters);
            }

            // insert the letters of the other string in the right spot in
            // the temporary letters vector
            vecFinalLetters.insert(vecFinalLetters.begin() + nPos, vecLetters.begin(), vecLetters.end());

            // Update the number of the letters
            nNumLetters = vecFinalLetters.size();

            // call updateLetters to recompute this object
            updateLetters(&(vecFinalLetters[0]), nNumLetters);
        }
        else
        {
            // Both the strings are non-spatial, so the result is non-spatial.
            
            // compute the new string
            string strTemp = m_strString;
            string strTextToInsert = asString(ipStringToInsert->String);
            strTemp.insert(nPos, strTextToInsert);

            // call updateString to reset this object as a string
            updateString(strTemp);
        }
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25800");
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::appendString(const string& strText)
{
    try
    {
        // Insert the string at the end of the current string
        insertString(m_strString.length(), strText);
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25801");
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::insertString(long nPos, const string& strText)
{
    try
    {
        // Verify that nPos is a valid index
        // If nPos < 0 or nPos is out of bounds on the string, throw an exception
        if ( (nPos < 0) || 
            (nPos > (long)m_strString.length()) )
        {
            // If nPos isn't valid, throw an exception
            UCLIDException ue("ELI26593", "Index out of bounds!");
            ue.addDebugInfo("Index", nPos);
            ue.addDebugInfo("String length", m_strString.length());
            throw ue;
        }

        if (m_eMode != kSpatialMode)
        {
            // Since the string is either hybrid or non-spatial, just update the text
            m_strString.insert(nPos, strText);
        }
		// This string is kSpatialMode and this is an append operation
		// Don't recompute the whole string just to append a non-spatial string
		else if (nPos > 0 && nPos == m_strString.length())
		{
			if (!strText.empty())
			{
				// Update the end-of-paragraph flag of the last letter if needed
				CPPLetter& lastOldLetter = m_vecLetters[nPos - 1];
				if ((strText[0] == '\r' || strText[0] == '\n')
					&& !isWhitespaceChar(lastOldLetter.m_usGuess1))
				{
					lastOldLetter.m_bIsEndOfParagraph = true;
				}
				m_strString += strText;

				vector<CPPLetter> newLetters;
				getNonSpatialLetters(strText, newLetters);
				m_vecLetters.insert(m_vecLetters.end(), newLetters.begin(), newLetters.end());
			}
		}
        else
        {
            // Get the current letter vector
            vector<CPPLetter> vecFinalLetters = m_vecLetters;

            // Get the letters from the string
            vector<CPPLetter> vecLetters;
            getNonSpatialLetters(strText, vecLetters);

            // Insert the letters into the string
            vecFinalLetters.insert(vecFinalLetters.begin() + nPos, vecLetters.begin(),
                vecLetters.end());

            // Update the number of the letters
            long nNumLetters = vecFinalLetters.size();

            // call updateLetters to recompute this object
            updateLetters(&(vecFinalLetters[0]), nNumLetters);
        }
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25802");
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::addRasterZones(IIUnknownVectorPtr ipRasterZones,
                                    ILongToObjectMapPtr ipPageInfoMap)
{
    try
    {
        ASSERT_ARGUMENT("ELI25803", ipRasterZones != __nullptr);

        // Get the size and assert that there is at least 1 raster zone to append
        long lSize = ipRasterZones->Size();
        if (lSize < 1)
        {
            UCLIDException ue("ELI25804", "Cannot add empty raster zone vector to string!");
            throw ue;
        }

        // If string is spatial then first downgrade it to hybrid
        if (m_eMode == kSpatialMode)
        {
            downgradeToHybrid();
        }

        bool requiresTranslation = updateAndValidateCompatibleSpatialPageInfo(ipPageInfoMap);

        for (long i=0; i < lSize; i++)
        {
            UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipZone = ipRasterZones->At(i);
            ASSERT_RESOURCE_ALLOCATION("ELI25805", ipZone != __nullptr);

			if (requiresTranslation)
			{
				ipZone = translateToNewPageInfo(ipZone, ipPageInfoMap,  getPageInfoMap());
			}

            m_vecRasterZones.push_back(ipZone);
        }

        // Set mode to hybrid
        m_eMode = kHybridMode;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25806");
}
//-------------------------------------------------------------------------------------------------
bool CSpatialString::updateAndValidateCompatibleSpatialPageInfo(ILongToObjectMapPtr ipPageInfoMap)
{
    try
    {
		bool requiresTranslation = false;

        if (ipPageInfoMap == __nullptr || m_ipPageInfoMap == ipPageInfoMap)
        {
            // Nothing to add, just return
        }
        // If the current page info map is NULL just replace it
        else if (m_ipPageInfoMap == __nullptr)
        {
            m_ipPageInfoMap = ipPageInfoMap;

			// After being assigned to a SpatialString, the page info map must not be modifed, otherwise
			// it may affect other SpatialStrings that share these page infos.
			m_ipPageInfoMap->SetReadonly();
        }
        // Else - merge the info maps (validating compatible pages)
        else
        {
            // the target already has a spatial page info map, compare source's
            // keys one at a time and add any that the target is missing. [P13 #4728]
            IVariantVectorPtr ipKeys( ipPageInfoMap->GetKeys() );
            ASSERT_RESOURCE_ALLOCATION("ELI25807", ipKeys != __nullptr);

			// m_ipPageInfoMap will be read-only. Need to create a new copy to make it readable.
			// Shallow copy because the PageInfo instances themselves are immutable and don't need
			// to be cloned.
			IShallowCopyablePtr ipCopyThis(m_ipPageInfoMap);
			ASSERT_RESOURCE_ALLOCATION("ELI36300", ipCopyThis != __nullptr);
			ILongToObjectMapPtr ipMergedPageInfos = ipCopyThis->ShallowCopy();
			ASSERT_RESOURCE_ALLOCATION("ELI36301", ipMergedPageInfos != __nullptr);

            // iterate through each of source's keys
            long lSize = ipKeys->Size;
            for(long i=0; i<lSize; i++)
            {
                // get the ith key
                long lKey = ipKeys->GetItem(i).lVal;

                UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr ipSourceInfo =
                    ipPageInfoMap->GetValue(lKey);
                ASSERT_RESOURCE_ALLOCATION("ELI25808", ipSourceInfo != __nullptr);

                // if the target doesn't have this key, add it
                if(ipMergedPageInfos->Contains(lKey) == VARIANT_FALSE)
                {
                    ipMergedPageInfos->Set(lKey, ipSourceInfo);
                }
                // else the target has the key so need to validate that the infos are compatible
                else
                {
                    UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr ipCurrentInfo =
                        ipMergedPageInfos->GetValue(lKey);
                    ASSERT_RESOURCE_ALLOCATION("ELI25809", ipCurrentInfo != __nullptr);

                    // Check that the spatial page infos are compatible
                    if (ipCurrentInfo->Equal(ipSourceInfo, VARIANT_TRUE) == VARIANT_FALSE)
                    {
                        long lSourceWidth, lCurrentWidth, lSourceHeight, lCurrentHeight;
                        UCLID_RASTERANDOCRMGMTLib::EOrientation eSourceOrient, eCurrentOrient;
                        double dSourceDeskew, dCurrentDeskew;
						ipSourceInfo->GetPageInfo(&lSourceWidth, &lSourceHeight,
							&eSourceOrient, &dSourceDeskew);
						ipCurrentInfo->GetPageInfo(&lCurrentWidth, &lCurrentHeight,
							&eCurrentOrient, &dCurrentDeskew);

						if (lSourceHeight == lCurrentHeight && lSourceWidth == lCurrentWidth)
						{
							requiresTranslation = true;
						}
						else
						{
							UCLIDException ue("ELI25810",
								"Cannot merge with incompatible spatial page info!");
							ue.addDebugInfo("Page Number", lKey);

							// Try to get the page info values and add it as debug info
							try
							{
								ue.addDebugInfo("Source Width", lSourceWidth);
								ue.addDebugInfo("Source Height", lSourceHeight);
								ue.addDebugInfo("Source Orientation", eSourceOrient);
								ue.addDebugInfo("Source Deskew", dSourceDeskew);
								ue.addDebugInfo("Current Width", lCurrentWidth);
								ue.addDebugInfo("Current Height", lCurrentHeight);
								ue.addDebugInfo("Current Orientation", eCurrentOrient);
								ue.addDebugInfo("Current Deskew", dCurrentDeskew);
							}
							catch (...)
							{
							}

							throw ue;
						}
                    }
                }
            }

			m_ipPageInfoMap = ipMergedPageInfos;

			// After being assigned to a SpatialString, the page info map must not be modifed, otherwise
			// it may affect other SpatialStrings that share these page infos.
			m_ipPageInfoMap->SetReadonly();
        }

		return requiresTranslation;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25811");
}
//-------------------------------------------------------------------------------------------------
bool CSpatialString::isMultiPage()
{
    try
    {
        // Default to string is single page
        bool bReturn = false;
        switch( m_eMode )
        {
            case kNonSpatialMode:
            {
				// Don't force caller to have to separately check if a string is spatial; if it's
				// not spatial it's not multi-page.
				return false;
            }
            break;
            case kHybridMode:
            {
                long nCurrPage = -1;
                vector<UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr>::iterator it;
                for(it = m_vecRasterZones.begin(); it != m_vecRasterZones.end(); it++)
                {
                    // Get the RasterZone from the IUnknownVector of raster zones
                    UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipRZone = (*it);
                    ASSERT_RESOURCE_ALLOCATION("ELI14763", ipRZone != __nullptr);
                    
                    if( nCurrPage < 0 )
                    {
                        // Get a page for the comparison on the first iteration.
                        nCurrPage = ipRZone->PageNumber;
                    }
                    else
                    {
                        // It doesn't matter what page this is, because if there are any 2 
                        // zones that don't have identical page numbers, then it must be a 
                        // multi-page image.
                        if( nCurrPage != ipRZone->PageNumber )
                        {
                            bReturn = true;
                            break;
                        }
                    }
                }
            }
            break;
            case kSpatialMode:
            {
                // iterate through each of the letters and check to see if 
                // there exists two spatial letter objects on different pages
                long nCurrPage = -1;
                long nNumLetters = m_vecLetters.size();
                for (int i = 0; i < nNumLetters; i++)
                {
                    // get the letter object at this position
                    CPPLetter& letter = m_vecLetters[i];

                    if (letter.m_bIsSpatial)
                    {
                        // if we found the first spatial character, then
                        // remember its page number
                        if (nCurrPage < 0)
                        {
                            nCurrPage = letter.m_usPageNumber;
                        }
                        else
                        {
                            // this is a subsequent spatial character
                            // we have come across.  If it is on the same
                            // page as all previous spatial chars, then 
                            // just keep searching.  If it is on a different
                            // page, then immediately return true.
                            if (letter.m_usPageNumber != nCurrPage)
                            {
                                bReturn = true;
                                break;
                            }
                        }
                    }
                }
            }
            break;

            default:
                THROW_LOGIC_ERROR_EXCEPTION("ELI25812");
        }//end switch

        return bReturn;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25813");
}
//-------------------------------------------------------------------------------------------------
UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr CSpatialString::getSubString(long nStart, long nEnd)
{
    try
    {
        // verify start and end index
        verifyValidIndex(nStart);

        // end index must either be -1 or a valid index
        if (nEnd == -1)
        {
            // end index value of -1 means "until end of string"
            nEnd = m_strString.length() - 1;
        }
        else
        {
            verifyValidIndex(nEnd);
        }

        // Verify that the end is past the start
        if( nStart > nEnd )
        {
        UCLIDException ue("ELI14968", "Start cannot be past the end of the substring!");
            ue.addDebugInfo("Start:", asString( nStart ) );
            ue.addDebugInfo("End:", asString( nEnd ) );
            throw ue;
        }

        // create new spatial string
        UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipSpatialString(CLSID_SpatialString);
        ASSERT_RESOURCE_ALLOCATION("ELI06458", ipSpatialString != __nullptr);

        //Get the substring of the object
        string strSubStr = m_strString.substr(nStart, nEnd - nStart + 1);

        // The operation depends on what mode this spatial string is in
        if( m_eMode == kSpatialMode )
        {
            // If the string is of type kSpatialMode, then the returned value will also be 
            // of kSpatialMode type. 
            long nLength = (nEnd+1) - nStart;

            // Take the chunk of the Letters vector that corresponds to the substring.
            if(!m_vecLetters.empty())
            {
                ipSpatialString->CreateFromLetterArray(nLength, &(m_vecLetters[nStart]),
                    m_strSourceDocName.c_str(), m_ipPageInfoMap);
            }
            else
            {
                THROW_LOGIC_ERROR_EXCEPTION("ELI15264");
            }
        }
        else if( m_eMode== kHybridMode )
        {
            // Add the Raster Zone(s) from this object to the new spatial string
            ipSpatialString->CreateHybridString(getOCRImageRasterZonesUnknownVector(),
                strSubStr.c_str(), m_strSourceDocName.c_str(), m_ipPageInfoMap);
        }
        // The mode is kNonSpatialMode, so just set the string
        else
        {
            ipSpatialString->CreateNonSpatialString(strSubStr.c_str(), m_strSourceDocName.c_str());
        }

		// Set the OCR engine version
		ipSpatialString->OCREngineVersion = get_bstr_t(m_strOCREngineVersion);

		// Set the OCR parameters
		if (m_ipOCRParameters != __nullptr)
		{
			UCLID_RASTERANDOCRMGMTLib::IHasOCRParametersPtr ipHasOCRParameters(ipSpatialString);
			ASSERT_RESOURCE_ALLOCATION("ELI46183", ipHasOCRParameters != __nullptr);
			ipHasOCRParameters->OCRParameters = getOCRParameters();
		}

        return ipSpatialString;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25814");
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::performReplace(const string& stdstrToFind, const string& stdstrReplacement,
    VARIANT_BOOL vbCaseSensitive, long lOccurrence, IRegularExprParserPtr ipRegExpr)
{
    try
    {
        try
        {
            IIUnknownVectorPtr ipMatches = getReplacements(stdstrToFind, stdstrReplacement, 
                asCppBool(vbCaseSensitive), lOccurrence, ipRegExpr);
            if (ipMatches == __nullptr || ipMatches->Size() <= 0)
            {
                // no matches found
                return;
            }

            // a final string after replacement
            UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipFinal(CLSID_SpatialString);
            ASSERT_RESOURCE_ALLOCATION("ELI06792", ipFinal != __nullptr);

            // Get the number of matches
            long nSize = ipMatches->Size();

            // Make some tracking variables
            long nNonMatchStart = 0, nNonMatchEnd = 0;
            for (long n = 0; n < nSize; n++)
            {
                // Get the info of the match
                ITokenPtr ipMatch = ipMatches->At(n);
                ASSERT_RESOURCE_ALLOCATION("ELI06820", ipMatch != __nullptr);

                // Get the start and end position of the match
                unsigned long ulStartPos, ulEndPos;
                ipMatch->GetStartAndEndPosition((long*) &ulStartPos, (long*) &ulEndPos);

                if (ulStartPos > 0)
                {
                    // Need a flag if the values don't make sense. 
                    nNonMatchEnd = ulStartPos - 1;

                    // Prevent invalid call to replace
                    if( nNonMatchStart <= nNonMatchEnd)
                    {
                        // get the sub string after the match and before the next match
                        UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipTempStr = 
                            getSubString(nNonMatchStart, nNonMatchEnd);
                        ASSERT_RESOURCE_ALLOCATION("ELI06824", ipTempStr != __nullptr);

                        // append it to the final string
                        ipFinal->Append(ipTempStr);
                    }
                }

                // get the actual replacement string
                string strActualReplacement = asString(ipMatch->Value);

                // now create a spatial string that has the replacement string
                UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipReplacement(CLSID_SpatialString);
                ASSERT_RESOURCE_ALLOCATION("ELI06793", ipReplacement != __nullptr);

                // Make sure the replacement string is actually replacing the found value with something
                if( strActualReplacement.length() > 0 )
                {
                    if ( m_eMode == kSpatialMode )
                    {
                        vector<CPPLetter> vecLetters;

                        unsigned long ulReplacementLength = strActualReplacement.length();
                        unsigned long ulActualFindLength = ulEndPos - ulStartPos + 1;

                        // create the replacement string with spatial information from the section of the
                        // original string being replaced, with the spatial information spread out roughly 
                        // across the entire replacement string
                        for (unsigned long i = 0; i < ulReplacementLength; i++)
                        {
                            // calculate the offset in the to-be-replaced string the spatial information of 
                            // which should be associated with position i to evenly copy over the spatial information
                            // into the replacement string.
                            unsigned long ulOffset = 0;
                            if (ulReplacementLength > 1)
                            {
                                // calculate offset
                                ulOffset = ulActualFindLength * i / (ulReplacementLength - 1);

                                // the maximum allowed offset is ulActualFindLength - 1
                                ulOffset = min(ulOffset, ulActualFindLength - 1);
                            }

                            // copy over the spatial information associated with the character at the calculated
                            // offset and override the actual char
                            CPPLetter letter = m_vecLetters[ulStartPos + ulOffset];
							unsigned short usChar = strActualReplacement[i];
							letter.m_usGuess1 = letter.m_usGuess2 = letter.m_usGuess3 =
								usChar;

                            // Set newline chars to be non-spatial to prevent spatial string searcher issues
                            if (usChar == '\r' || usChar == '\n')
                            {
                                letter.m_bIsSpatial = false;
                            }

                            // push the letter into the vector of letters which will later be used to build
                            // a spatial replacement string
                            vecLetters.push_back(letter);
                        }

                        // Create the replacement from the letter array
                        ipReplacement->CreateFromLetterArray(vecLetters.size(), &(vecLetters[0]),
                            m_strSourceDocName.c_str(), m_ipPageInfoMap);
                    }
                    else if( m_eMode == kHybridMode )
                    {
                        // Get the substring for the match
                        UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipTempStr =
                            getSubString(ulStartPos, ulEndPos);
                        ASSERT_RESOURCE_ALLOCATION("ELI15097", ipTempStr != __nullptr);

                        // Build the replacement spatial string from the substring's raster zones
                        // and use the replacement string for the text.
                        ipReplacement->CreateHybridString(ipTempStr->GetOCRImageRasterZones(),
                            strActualReplacement.c_str(), m_strSourceDocName.c_str(), m_ipPageInfoMap);
                    }
                    else if (m_eMode == kNonSpatialMode)
                    {
                        ipReplacement->CreateNonSpatialString(strActualReplacement.c_str(),
                            m_strSourceDocName.c_str());
                    }
                    else
                    {
                        UCLIDException ue("ELI15096", "Invalid mode for spatial string!");
                        ue.addDebugInfo("Mode:", asString( m_eMode ) );
                        throw ue;
                    }
                }
                // For a spatial string that has an empty replacement string, the end replacement 
                // spatial string will be kNonSpatial
                else 
                {
                    ipReplacement->CreateNonSpatialString(strActualReplacement.c_str(),
                        m_strSourceDocName.c_str());
                }

                // append the replacement to the final string
                ipFinal->Append(ipReplacement);

                // get the sub string after the last match if any
                if (n == nSize - 1 && ulEndPos < m_strString.size() - 1)
                {
                    UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipTempStr =
                        getSubString(ulEndPos+1, -1);
                    ASSERT_RESOURCE_ALLOCATION("ELI06825", ipTempStr != __nullptr);

                    // append it to the final string
                    ipFinal->Append(ipTempStr);
                }

                // move on to the next non match starting position
                nNonMatchStart = ulEndPos + 1;
            }

            // If the final string is kNonSpatialMode, and this object is hybrid or spatial, 
            // turn the final string into a kHybridMode object with the raster zones and 
            // spatial page info of this object
            if (m_eMode != kNonSpatialMode && stdstrReplacement != ""
                && ipFinal->HasSpatialInfo() == VARIANT_FALSE)
            {
                // Get the string that has already been calculated
                string strTemp = asString( ipFinal->String );

                // Build the final string from the raster zones of this object and 
                // give it this object's spatial page info
                ipFinal->CreateHybridString(getOCRImageRasterZonesUnknownVector(),
                    strTemp.c_str(), m_strSourceDocName.c_str(), m_ipPageInfoMap);
            }

            // update this spatial string from the constructed one
            copyFromSpatialString(ipFinal);
        }
        CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION( "ELI15668" );
    }
    catch(UCLIDException &ue)
    {
        // These debug data members should be encrypted because they may contain regular
        // expressions
        ue.addDebugInfo("FindString", stdstrToFind, true);
        ue.addDebugInfo("ReplaceString", stdstrReplacement, true);
        throw ue;
    }
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CSpatialString::getReplacements(const string& strFind, 
    const string& strReplacement, bool bCaseSensitive, long lOccurrence, 
    IRegularExprParserPtr ipRegExpr)
{
    IIUnknownVectorPtr ipResult = __nullptr;
    if (ipRegExpr != __nullptr)
    {
        // Set the regular expression options
        ipRegExpr->IgnoreCase = asVariantBool(!bCaseSensitive);
        ipRegExpr->Pattern = strFind.c_str();

        // Find the matches of strToFind in the string and the text it will be replaced with
        IIUnknownVectorPtr ipMatches = ipRegExpr->FindReplacements(m_strString.c_str(), 
            strReplacement.c_str(), asVariantBool(lOccurrence == 1));

        // If we are matching the first or all occurrences, ipMatches is the result
        if (lOccurrence == 0 || lOccurrence == 1)
        {
            ipResult = ipMatches;
        }
        else
        {
            // Get the number of matches
            long lSize = ipMatches->Size();

            // Calculate the index of the requested index
            long lOccurrenceIndex = -1;
            if (lOccurrence < 0)
            {
                // Negative occurrences are calculated from the end of the matches
                lOccurrenceIndex = lSize + lOccurrence;
            }
            else if (lOccurrence <= lSize)
            {
                // Positive occurrences are calculated from the front of the matches
                lOccurrenceIndex = lOccurrence - 1;
            }

            // Return no matches if the limit has been exceeded
            ipResult.CreateInstance(CLSID_IUnknownVector);
            ASSERT_RESOURCE_ALLOCATION("ELI25278", ipResult != __nullptr);
            if (lOccurrenceIndex >= 0)
            {
                // Return the specified match
                ITokenPtr ipMatch = ipMatches->At(lOccurrenceIndex);
                ASSERT_RESOURCE_ALLOCATION("ELI25279", ipMatch != __nullptr);
                ipResult->PushBack(ipMatch);
            }
        }
    }
    else
    {
        // Default to an empty result
        ipResult.CreateInstance(CLSID_IUnknownVector);
        ASSERT_RESOURCE_ALLOCATION("ELI25282", ipResult != __nullptr);

        // Get the string to search and the string to search for in the appropriate casing
        string strString = m_strString;
        string strTarget = strFind;
        if (!bCaseSensitive)
        {
            makeLowerCase(strString);
            makeLowerCase(strTarget);
        }

        // Check if all matches should be returned
        if (lOccurrence == 0)
        {
            // Iterate through each match
            unsigned long ulStart = strString.find(strTarget, 0);
            while (ulStart != string::npos)
            {
                unsigned long ulEnd = ulStart + strTarget.size();

                // Create the specified match
                ITokenPtr ipMatch(CLSID_Token);
                ASSERT_RESOURCE_ALLOCATION("ELI25284", ipMatch != __nullptr);
                ipMatch->StartPosition = ulStart;
                ipMatch->EndPosition = ulEnd - 1;
                ipMatch->Value = strReplacement.c_str();

                // Add it the list of found matches
                ipResult->PushBack(ipMatch);

                // Find the next match
                ulStart = strString.find(strTarget, ulEnd);
            }
        }
        else
        {
            // A specific match is being requested

            // Iterate through the matches
            long i = 0;
            unsigned long ulStart = 0;
            unsigned long ulEnd = 0;
            long lUpper = abs(lOccurrence);
            while (i < lUpper)
            {
                ulStart = lOccurrence > 0 ? 
                    strString.find(strTarget, ulEnd) : strString.rfind(strTarget, ulStart-1);
                if (ulStart == string::npos)
                {
                    break;
                }

                i++;
                ulEnd = ulStart + strTarget.size();
            }

            // Check if the specified occurrence was found
            if (i >= lUpper)
            {
                // Create the specified match
                ITokenPtr ipMatch(CLSID_Token);
                ASSERT_RESOURCE_ALLOCATION("ELI25283", ipMatch != __nullptr);
                ipMatch->StartPosition = ulStart;
                ipMatch->EndPosition = ulEnd - 1;
                ipMatch->Value = strReplacement.c_str();

                // Add it to the result
                ipResult->PushBack(ipMatch);
            }
        }
    }

    return ipResult;
}
//-------------------------------------------------------------------------------------------------
ILongToObjectMapPtr CSpatialString::getPageInfoMap()
{
    // if the page info map object does not exist, create an empty one
    if (m_ipPageInfoMap == __nullptr)
    {
        m_ipPageInfoMap.CreateInstance(CLSID_LongToObjectMap);
        ASSERT_RESOURCE_ALLOCATION("ELI15648", m_ipPageInfoMap != __nullptr);
    }

    // return the page info map
    return m_ipPageInfoMap;
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::validateLicense()
{
    static const unsigned long THIS_COMPONENT_ID = gnEXTRACT_CORE_OBJECTS;

    VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI05844", "Spatial String" );
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::verifyValidIndex(long nIndex)
{
    // throw an exception if nIndex is not a valid index for m_strString
    if (nIndex < 0 ||( (unsigned long) nIndex >= m_strString.length()))
    {
        UCLIDException ue("ELI06463", "Invalid index!");
        ue.addDebugInfo("nIndex", nIndex);
        ue.addDebugInfo("Vector size", m_strString.length());
        throw ue;
    }
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::verifySpatialness()
{
    // if this string is not spatial, then throw an exception 
    if (m_eMode != kSpatialMode)
    {
        UCLIDException ue("ELI06814", "Cannot perform this operation on a non-spatial string!");
        ue.addDebugInfo(string("String"), m_strString);
        throw ue;
    }

    if ( m_vecLetters.size() != m_strString.length() )
    {
        UCLIDException ue("ELI12921", "String size does not match letter vector size!");
        ue.addDebugInfo("String length:", asString(m_strString.length()) );
        ue.addDebugInfo("Vector size:", asString(m_vecLetters.size()) );
        throw ue;
    }

    if( m_vecLetters.empty() )
    {
        UCLIDException ue("ELI15416", "Letters vector cannot be empty for a kSpatialMode string!");
        ue.addDebugInfo("Vector size:", asString(m_vecLetters.size()) );
        throw ue;

    }
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::loadFromTXTFile(const string& strFileName)
{
    // get the text file contents as a string
    string strTemp = getTextFileContentsAsString(strFileName);

    // reset all member variables
    reset(true, true);

    m_strString = strTemp;
    m_strSourceDocName = strFileName;
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::saveToTXTFile(const string& strFileName)
{
    // save the output to a text file
    ofstream ofs(strFileName.c_str());
	if (!ofs.is_open())
	{
		UCLIDException ue("ELI34222", "Output file could not be opened.");
		ue.addDebugInfo("Filename", strFileName);
		ue.addWin32ErrorInfo();
		throw ue;
	}

    // replace any \r\n with \r since ofstream inserts
    // \r in front of any \n existing in the output string.
    string strTemp;
    strTemp.assign(m_strString);
    ::replaceVariable(strTemp, "\r\n", "\n");

    ofs << strTemp;

    // Close the file and wait for it to be readable
    ofs.close();
    waitForFileToBeReadable(strFileName);
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::loadTextWithPositionalData(const string& strFileName, EFileType eFileType)
{
	bool hasExplicitPositionalData = false;
	string text = "";
	size_t inputLength;
	size_t length;

	if (eFileType == kRichTextFile)
	{
		// Extract visible text with positional data
		hasExplicitPositionalData = true;
		IRichTextExtractorPtr ipExtractor("Extract.Utilities.Parsers.RichTextExtractorClass");
		ASSERT_RESOURCE_ALLOCATION("ELI48359", ipExtractor != __nullptr);

		text = ipExtractor->GetIndexedTextFromFile(get_bstr_t(strFileName.c_str()), VARIANT_FALSE);
		inputLength = text.length();
		length = inputLength / 10;
		ASSERT_RUNTIME_CONDITION("ELI46643", length * 10 == inputLength, "Invalid file length for indexed text!")
	}
	else
	{
		// Load the file as text.
		text = getTextFileContentsAsString(strFileName);
		length = inputLength = text.length();
	}
	
	// Check for an empty string
	if (text.empty())
	{
		reset(true, true);

		// Copy the source doc name
		m_strSourceDocName = strFileName;
		return;
	}

	// Initialize a letter array that will be used to create the spatial string.
	vector<CPPLetter> vecLetters(length);
	CPPLetter *pLastNonWhitespaceLetter = NULL;

	int nCharSize = hasExplicitPositionalData ? 10 : 1;
	size_t nMaxPos = 0;

	// Loop through each character of the file.
	for (size_t i = 0; i < length; i++)
	{
		size_t nInputIdx = i * nCharSize;
		unsigned char c = text[nInputIdx];
		size_t nPos = i;
		size_t nLen = 1;

		if (hasExplicitPositionalData)
		{
			string strPos = text.substr(nInputIdx + 1, 8);
			string strLen = text.substr(nInputIdx + 9, 1);
			try
			{
				nPos = std::stoul(strPos, __nullptr, 16);
				nLen = std::stoul(strLen, __nullptr, 16);
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI46644")
		}
		nMaxPos = max(nPos + nLen, nMaxPos);

		CPPLetter& letter = vecLetters[i];

		// Specify the character.
		letter.m_usGuess1 = c;
		letter.m_usGuess2 = c;
		letter.m_usGuess3 = c;

		// Assign spatial info using index or explicit position
		letter.m_bIsSpatial = true;
		letter.m_usPageNumber = 1;
		letter.m_ulLeft = nPos;
		letter.m_ulRight = nPos + nLen;
		letter.m_ulTop = 0;
		letter.m_ulBottom = 1;

		if (c == '\r' || c == '\n')
		{
			letter.m_bIsSpatial = false;
		}

		// If the character is whitespace, check if this makes the
		// last non-whitespace character the end of a zone or the end of a paragraph.
		if (isWhitespaceChar(c))
		{
			if (pLastNonWhitespaceLetter != NULL)
			{
				if (c == '\r' || c == '\n')
				{
					pLastNonWhitespaceLetter->m_bIsEndOfZone = true;
					pLastNonWhitespaceLetter->m_bIsEndOfParagraph = true;
					pLastNonWhitespaceLetter = NULL;
				}
				// If there is more than one consecutive whitespace char, ensure the last spatial
				// char is treated as end-of-zone.
				else if (isWhitespaceChar(vecLetters[i - 1].m_usGuess1))
				{
					pLastNonWhitespaceLetter->m_bIsEndOfZone = true;
				}

			}
		}
		else
		{
			pLastNonWhitespaceLetter = &letter;
		}

		// Make sure the previous letter is marked as end of zone if there has been a gap
		// https://extract.atlassian.net/browse/ISSUE-16678
		if (i > 0)
		{
			CPPLetter& prevLetter = vecLetters[i - 1];
			if (prevLetter.m_ulRight < letter.m_ulLeft)
			{
				prevLetter.m_bIsEndOfZone = true;
			}
		}
	}

	// The page info should be as many pixels wide as the max position and 1 pixel
	// high.
	UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr ipPageInfo(CLSID_SpatialPageInfo);
	ASSERT_RESOURCE_ALLOCATION("ELI31685", ipPageInfo != __nullptr);
	ipPageInfo->Initialize(nMaxPos, 1, UCLID_RASTERANDOCRMGMTLib::kRotNone, 0.0);

	// Create a spatial page info map
	ILongToObjectMapPtr ipPageInfoMap(CLSID_LongToObjectMap);
	ASSERT_RESOURCE_ALLOCATION("ELI31686", ipPageInfoMap != __nullptr);
	ipPageInfoMap->Set(1, ipPageInfo);

	getThisAsCOMPtr()->CreateFromLetterArray(length, &(vecLetters[0]), strFileName.c_str(),
		ipPageInfoMap);
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::validateAndMergeSourceDocName(const string& strSourceDocName)
{
    // if a source document name is associated with the other string,
    // and if that is different from the source document name associated
    // with this string, then throw an exception
    if (!strSourceDocName.empty() && !m_strSourceDocName.empty() && 
        strSourceDocName != m_strSourceDocName)
    {
		// [DataEntry:1232]
		// For the time being, making this a logged application trace to avoid exceptions being
		// thrown in the DE framework when rules were run against the document when it had a
		// different source doc name.
		UCLIDException ue("ELI06806",
			"Application trace: Cannot operate on spatial strings from different sources!");
        ue.addDebugInfo("this.SourceDocName", m_strSourceDocName);
        ue.addDebugInfo("other.SourceDocName", strSourceDocName);
		ue.log();
		m_strSourceDocName = strSourceDocName;
		return;
    }

    // if this object has no source document name associated with it, but the
    // other one does, then copy the other object's source document name to this
    if (m_strSourceDocName.empty() && !strSourceDocName.empty())
    {
        m_strSourceDocName = strSourceDocName;
    }
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::performConsistencyCheck()
{
    // perform consistency check to ensure that the size of the
    // string and letters vector are equal (if the letters vector exists)
    if (m_eMode == kSpatialMode)
    {
        if (m_vecLetters.size() != m_strString.length())
        {
            UCLIDException ue("ELI06874", "Letters and string don't match in size!");
            ue.addDebugInfo("Letters length", m_vecLetters.size());
            ue.addDebugInfo("String length", m_strString.length());
            throw ue;
        }
    }
    // Verify that the Hybrid mode has a valid raster zone vector
    else if( m_eMode == kHybridMode )
    {
        if (m_vecRasterZones.size() < 1)
        {
            UCLIDException ue("ELI14964", "Hybrid mode requires at least one raster zone!");
            ue.addDebugInfo("Mode:", asString( m_eMode ));
            ue.addDebugInfo("NumZones:", asString( m_vecRasterZones.size() ) );
            throw ue;
        }

        // If a kHybridMode string is empty, it should discard all spatial information and
        // become a kNonSpatialMode spatial string.
        if ( m_strString == "" )
        {
            downgradeToNonSpatial();
        }
    }
    else if( m_eMode == kNonSpatialMode )
    {
        // if the string is not spatial, then the letters vector must be empty
        if (m_vecLetters.size() != 0)
        {
            UCLIDException ue("ELI06875", "Letters must not exist for non-spatial string.");
            ue.addDebugInfo("Mode:", asString( m_eMode ));
            ue.addDebugInfo("NumLetters:", asString( m_vecLetters.size() ) );
            throw ue;
        }
        
        // Non spatial objects should not have raster zones
        if( m_vecRasterZones.size() > 0 )
        {
            UCLIDException ue("ELI15280", "Raster zones must not exist for a non-spatial string.");
            ue.addDebugInfo("Mode:", asString( m_eMode ));
            ue.addDebugInfo("Num Raster zones:", m_vecRasterZones.size());
            throw ue;
        }
    }
    else
    {
        // Should never get here
        UCLIDException ue("ELI15051", "Invalid spatial string mode!");
        ue.addDebugInfo("Mode:", asString( m_eMode ));
        throw ue;
    }
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::getNonSpatialLetters(const string& strText, std::vector<CPPLetter>& vecLetters)
{
    // create the letter objects and populate the vector
    long nSize = strText.length();
    for (int i = 0; i < nSize; i++)
    {
        CPPLetter letter;
        letter.m_usGuess1 = letter.m_usGuess2 = letter.m_usGuess3 = strText[i];

        letter.m_bIsSpatial = false;
        vecLetters.push_back(letter);
    }
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::reset(bool bResetSourceDocName, bool bResetPageInfoMap)
{
    try
    {
		// clear all its contents
        m_strString = "";
        m_vecLetters.clear();
        m_strOCREngineVersion = "";

        m_vecRasterZones.clear();

        // Clear the spatial page info map
        if( m_ipPageInfoMap != __nullptr && bResetPageInfoMap)
        {
            // Clear the reference, do not remove the SpatialPageInfo objects
            m_ipPageInfoMap = __nullptr;
        }

        // Set the mode to non-spatial
        m_eMode = kNonSpatialMode;

        // reset the source document name if appropriate
        if (bResetSourceDocName)
        {
            m_strSourceDocName = "";
        }

        // set the dirty flag to true since a modification was made
        m_bDirty = true;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25991");
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::processLetters(CPPLetter* letters, long nNumLetters)
{
    // Reset all the member variables except for the source doc name and the page info
    reset(false, false);

	m_strString.reserve(nNumLetters);

    // iterate through each letter to determine
    // the state of the String and IsSpatial attributes
    for (long i = 0; i < nNumLetters; i++)
    {
        CPPLetter& letter = letters[i];

        // Update the string by using the Guess1 attribute
        // Check to make sure char is not NULL before adding
        if( letter.m_usGuess1 != NULL )
        {
            m_strString += (char) letter.m_usGuess1;
        }

        // if we still haven't come across a spatial character
        // then check to see if the current character is spatial
        if (letter.m_bIsSpatial)
        {
            m_eMode = kSpatialMode;
        }
    }

    // If this is a spatial string we need to maintain
    // the LetterVector
    if(m_eMode == kSpatialMode)
    {
        m_vecLetters.resize(nNumLetters);
        
        if(nNumLetters > 0)
        {
			// Since the m_vecLetters was just resized to nNumLetters the copy size 
			// is the same
			long lCopySize = sizeof(CPPLetter) * nNumLetters;
            memcpy_s(&m_vecLetters[0], lCopySize , letters, lCopySize);
        }
    }

    // Verify that this object is in a good state.
    performConsistencyCheck();

    // set the dirty flag to true since a modification was made
    m_bDirty = true;
}
//-------------------------------------------------------------------------------------------------
bool CSpatialString::getStartAndEndPageNumber(long& nStartPage, long& nEndPage, long nTotalPageNum)
{
    // validate the start/end page number
    if ( (nStartPage == -1 && nEndPage == -1)
        || (nStartPage > 0 && nEndPage > 0 && nStartPage > nEndPage)
        || nEndPage == 0 || nEndPage < -1
        || nStartPage == 0 || nStartPage < -1 )
    {
        string strMsg = "Invalid start/end page number specified.\r\n"
            "startPage = -1 OR startPage > 0"
            "endPage = -1 OR endPage > 0"
            "If startPage == -1, then the endPage field represents the \"last X pages\"."
            "If endPage == -1, then the search should go from the specified start page to the last page in the document."
            "If startPage == endPage == -1, then that's an error condition.";
        UCLIDException ue("ELI07420", strMsg);
        ue.addDebugInfo("Start Page", nStartPage);
        ue.addDebugInfo("End Page", nEndPage);
        throw ue;
    }

    // Default to return all pages
    bool bAllPages = true;

    // Return pages from start page to end of document
    if (nStartPage >= 1 && nEndPage == -1)
    {
        // Set end page to total pages
        nEndPage = nTotalPageNum;

        // If nStartPage == 1 then return entire document
        bAllPages = (nStartPage == 1);
    }
    // last X number of pages need to be returned
    else if (nStartPage == -1 && nEndPage > 0)
    {
        // if specified number of pages exceeds the 
        // total number of pages, return entire document
        if (nEndPage > nTotalPageNum)
        {
            nStartPage = 1;
            nEndPage = nTotalPageNum;
            bAllPages = true;
        }
        else
        {
            nStartPage = nTotalPageNum - nEndPage + 1;
            nEndPage = nTotalPageNum;
            bAllPages = false;
        }
    }
    else if (nStartPage > 0 && nEndPage > 0)
    {
        if (nEndPage > nTotalPageNum)
        {
            nEndPage = nTotalPageNum;
        }

        bAllPages = false;
    }

    return bAllPages;
}
//-------------------------------------------------------------------------------------------------
bool CSpatialString::overlapX(const RECT& rect1, const RECT& rect2, long nMinOverlap)
{
    // Check the left and right boundaries
    if (rect1.left > rect2.right || 
        rect1.right < rect2.left )
    {
        return false;
    }

    long lLeft = min(rect1.left, rect2.left);
    long lRight = max(rect1.right, rect2.right);
    long lOver = lRight - lLeft;
    long lLength1 = rect1.right - rect1.left;
    long lLength2 = rect2.right - rect2.left;
    long lTotal = lLength1 + lLength2;
    long lOverlap = lTotal - lOver;
    long lPercentOverlap = 0;

    if (lLength1 < lLength2)
    {
        lPercentOverlap = (lOverlap*100) / lLength1;
    }
    else
    {
        lPercentOverlap = (lOverlap*100) / lLength2;
    }

    return (lPercentOverlap > nMinOverlap);
}
//-------------------------------------------------------------------------------------------------
bool CSpatialString::getIsEndOfWord(long nIndex)
{
    // return false if the index is invalid
    if (nIndex < 0 || (unsigned long) nIndex >= m_strString.size())
    {
        return false;
    }

    // If nIndex is whitespace it is not the end of a word
    char cLetter = m_strString[nIndex];
    if (cLetter == ' ' || cLetter == '\t' || 
        cLetter == '\r' || cLetter == '\n' || 
        cLetter == '\v' || cLetter == '\f')
    {
        return false;
    }

    // If nIndex is the last character in the string is is the end of a word
    if (nIndex == m_strString.size() - 1)
    {
        return true;
    }
    
    // If the next character is whitepace nIndex is the end of a word
    char cNextLetter = m_strString[nIndex + 1];

    if (cNextLetter == ' ' || cNextLetter == '\t' || 
        cNextLetter == '\r' || cNextLetter == '\n' || 
        cNextLetter == '\v' || cNextLetter == '\f')
    {
        return true;
    }
    
    return false;
}
//-------------------------------------------------------------------------------------------------
bool CSpatialString::getIsEndOfLine(long nIndex)
{
    // return false if the index is invalid
    if (nIndex < 0 || (unsigned long) nIndex >= m_strString.size())
    {
        return false;
    }

    // If nIndex is whitespace it is not the end of a line
    char cLetter = m_strString[nIndex];

    if (cLetter == '\r' || cLetter == '\n')
    {
        return false;
    }

    // get the current page of this letter, if it is available (ie. mode is spatial)
    unsigned short usPageNum = 0;
    if(m_eMode == kSpatialMode && m_vecLetters[nIndex].m_bIsSpatial)
    {
        usPageNum = m_vecLetters[nIndex].m_usPageNumber;
    }

    unsigned long nNextPos = nIndex + 1;

    while (true)
    {
        // if we have reached the end of the string
        // then nIndex was the last non-whitespace character so
        // it was the end of a line
        if (nNextPos >= m_strString.size())
        {
            return true;		
        }

        // if the next letter is on a different page, then this is the end of a line. [P16 #2477]
        if(usPageNum != 0)
        {
            // need to search for the next spatial letter and check its page number.
            // [LegacyRCAndUtils #4976]
            for (unsigned long i = nNextPos; i < m_vecLetters.size(); i++)
            {
                const CPPLetter& nextLetter = m_vecLetters[i];
                
                if(nextLetter.m_bIsSpatial)
                {
                    // letter is spatial, check the page number:
                    // if page number is different then return true(is end of line)
                    // else the string is on the same page break from the loop 
                    // (it may still be end of line)
                    if(nextLetter.m_usPageNumber != usPageNum)
                    {
                        return true;
                    }
                    else
                    {
                        break;
                    }
                }
                // next letter is non-spatial, keep searching
            }
        }

        // I am defining the end of a line as \r*\n
        char cNextLetter = m_strString[nNextPos];

        if (cNextLetter == '\r')
        {
            nNextPos++;
        }
        else if (cNextLetter == '\n' )
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
//-------------------------------------------------------------------------------------------------
bool CSpatialString::getIsEndOfZone(long nIndex, bool bTreatGapsAsZoneBoundaries)
{
	// Invalid index
	if (nIndex < 0 || (unsigned long)nIndex >= m_strString.size())
	{
		return false;
	}

	// Last letter
	else if (nIndex == m_strString.size() - 1)
	{
		return true;
	}

	// Marked as end of zone
	CPPLetter& thisLetter = m_vecLetters[nIndex];
	if (thisLetter.m_bIsEndOfZone)
	{
		return true;
	}

	// Check for gap (right index assumed to be exclusive)
	return bTreatGapsAsZoneBoundaries && thisLetter.m_ulRight < m_vecLetters[nIndex + 1].m_ulLeft;
}
//-------------------------------------------------------------------------------------------------
bool CSpatialString::getIsEndOfLine(size_t index, CRect rectCurrentLineZone)
{
	// Check to see if this is the end of the line based on the char value or page number first.
	// This will not take into account the spatial location of of the next character.
    if (getIsEndOfLine(index))
    {
        return true;
    }
	
	// [DataEntry:1119]
	// Otherwise, if the next character of a line is a spatial character that is either completely
	// above or below rectCurrentZone or whose right side is less that the right side of
	// rectCurrentLineZone, this should be considered the end of the line.
	while (true)
	{
		// If the next char is past the end of the string, the original index value was the last
		// spatial char in the string, thus the end of the line.
		index++;
		if (index >= m_vecLetters.size())
		{
			return true;
		}

		// https://extract.atlassian.net/browse/ISSUE-12998
		// Keep searching for the next spatial char in case the next spatial char represents a new
		// line compared to rectCurrentLineZone.
		if (!m_vecLetters[index].m_bIsSpatial)
		{
			continue;
		}

		// Get the spatial area of the next character.
		CRect rectNextLetter(m_vecLetters[index].m_ulLeft, m_vecLetters[index].m_ulTop,
			m_vecLetters[index].m_ulRight, m_vecLetters[index].m_ulBottom);
							
		if (rectNextLetter.top > rectCurrentLineZone.bottom || 
			rectNextLetter.bottom < rectCurrentLineZone.top || 
			rectNextLetter.right < rectCurrentLineZone.right)
		{
			return true;
		}
		else
		{
			break;
		}
	}

	return false;
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::updateString(const std::string& strText)
{
    // Reset everything except m_strSourceDocName
    reset(false, true);

    // update string
    m_strString = strText;

    // make sure that this object is in a good state
    performConsistencyCheck();

    // Set the dirty flag to true since a modification was made
    m_bDirty = true;
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::updateLetters(CPPLetter* letters, long nNumLetters)
{
    // verify num letters is not negative
    ASSERT_ARGUMENT("ELI12922", nNumLetters >= 0);

    // verify that pointer is specified if letters exist
    if (nNumLetters > 0)
    {
        ASSERT_ARGUMENT("ELI10464", (letters != __nullptr));
    }

    // recompute this object's attributes from the letters
    processLetters(letters, nNumLetters);
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::updateHybrid(const vector<UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr>& vecZones,
                                  const string& strText)
{
    // Reset everything except m_strSourceDocName and m_ipPageInfoMap
    // [P16:2911] Don't reset m_ipPageInfoMap because that would result in a disallowed 
    // situation-- a hybrid that didn't have a page info map.
    reset(false, false);

    // Put the raster zones into this object's vector
    m_vecRasterZones = vecZones;

    // Update the string variable
    m_strString = strText;

    // Update the spatial mode of this object based upon the number of raster zones
    if(m_vecRasterZones.size() > 0)
    {
        m_eMode = kHybridMode;
    }
    else
    {
        m_eMode = kNonSpatialMode;
    }

    // Verify that this object is in a valid state
    performConsistencyCheck();

    // Set the dirty flag to true since a modification was made
    m_bDirty = true;
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::checkForFontInfo( VARIANT_BOOL* pbType, long nNumCharsOfType, 
                                        long nNumSpatialChars, long nMinPercentage)
{
    long nPercent = 0;

    if( nNumSpatialChars > 0 )
    {
        nPercent = (long) ceil((double)(100*nNumCharsOfType) / (double) nNumSpatialChars);
    }

    *pbType = asVariantBool(nPercent >= nMinPercentage);
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::reviewSpatialStringAndDowngradeIfNeeded()
{
    // Verify that this object is in a valid state
    //performConsistencyCheck();

    // Get the number of letters in the vector
    long nLetters = m_vecLetters.size();

    // If there is at least one letter in the letters vector
    if( nLetters > 0 )
    {
        // Set the mode to non-spatial to test the spatial-ness of the letters vector
        m_eMode = kNonSpatialMode;

        // Check each letter to see if any have spatial information
        for( int i = 0; i < nLetters; i++)
        {
            CPPLetter& letter = m_vecLetters[i];
            if (letter.m_bIsSpatial)
            {
                // If there is at least one letter that is spatial, this object is spatial
                m_eMode = kSpatialMode;
                break;
            }
        }//end for

        // If no letters in the vector are spatial, downgrade this object to kNonSpatialMode
        if( m_eMode == kNonSpatialMode )
        {
            downgradeToNonSpatial();
        }
    }
    // If there is a RasterZone vector with items in it then downgrade this objects to hybrid mode
    // if it is not already in hybrid mode.
    else if(m_vecRasterZones.size() > 0 )
    {
        // do not attempt to downgrade if it already in Hybrid mode [p13 #4693]
        if (m_eMode != kHybridMode)
        {
            downgradeToHybrid();
        }
    }
    else
    {
        // Not hybrid, not spatial, so downgrade this object to non-spatial
        downgradeToNonSpatial();
    }
}
//-------------------------------------------------------------------------------------------------
bool CSpatialString::isRasterZoneInVector(UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipNewZone, 
                                          const vector<UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr>& vecZones)
{
    // Check parameters
    ASSERT_ARGUMENT("ELI15421", ipNewZone != __nullptr);

    for (vector<UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr>::const_iterator it = vecZones.begin();
        it != vecZones.end(); it++)
    {
        // Retrieve the Ith zone
        UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipThisZone = (*it);
        ASSERT_RESOURCE_ALLOCATION("ELI15423", ipThisZone != __nullptr);

        // Check data members
        if (ipThisZone->Equals( ipNewZone ) == VARIANT_TRUE)
        {
            return true;
        }
    }

    // No match was found
    return false;
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::fitPointsWithinBounds(double &x1, double &y1, double &x2, double &y2, 
                           double dCenterLeft, double dCenterTop)
{
    // handle the special case of a vertical line
    if(x1 == x2)
    {
        // ensure the point is within the upper limits of the page boundaries
        if(x1 > dCenterLeft)
        {
            x1 = x2 = dCenterLeft;
        }
        else if(x1 < -dCenterLeft)
        {
            x1 = x2 = -dCenterLeft;
        }

        if(y1 > dCenterTop)
        {
            y1 = y2 = dCenterTop;
        }
        else if(y1 < -dCenterTop)
        {
            y1 = y2 = -dCenterTop;
        }

        // ensure the line is at least 3 pixels long [P16 #2570]
        if(abs(y2-y1) < 3)
        {
            // shift the point closest to the center
            if( abs(y1) < abs(y2) )
            {
                // shift the first point away from the second point
                y1 = (y2 < 0 ? y2 + 3 : y2 - 3);
            }
            else
            {
                // shift the second point away from the first point
                y2 = (y1 < 0 ? y1 + 3 : y1 - 3);
            }
        }
    }
    else
    {
        // the line is not vertical, so its slope is defined

        // calculate the line connecting the two points
        double dSlope = (y1 - y2) / (x1 - x2);
        double dYIntercept = -dSlope * x1 + y1;

        // truncate the line segment if it is 
        // outside the bounds of original image
        shiftPointInsideBoundsAlongLine(x1, y1, dSlope, dYIntercept, dCenterLeft, dCenterTop);
        shiftPointInsideBoundsAlongLine(x2, y2, dSlope, dYIntercept, dCenterLeft, dCenterTop);

        // ensure the line is at least 3 pixels long [P16 #2570]
        if(sqrt( pow(x1-x2,2) + pow(y1-y2,2) ) < 3)
        {
            // shift the x coordinate closest to the center
            if( abs(x1) < abs(x2) )
            {
                // shift the first x coordinate away from the second x coordinate
                x1 = (x2 < 0 ? x2 + 3 : x2 - 3);
            }
            else
            {
                // shift the second x coordinate away from the first x coordinate
                x2 = (x1 < 0 ? x1 + 3 : x1 - 3);
            }
        }
    }
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::shiftPointInsideBoundsAlongLine(double &x, double &y, 
                                                     double dSlope, double dYIntercept, 
                                                     double dCenterLeft, double dCenterTop)
{
    // ensure point is within the left and right bounds of the image
    if(x < -dCenterLeft)
    {
        // find the point where the line intersects the left side of the image
        x = -dCenterLeft;
        y = dSlope * x + dYIntercept;
    }	
    else if(x > dCenterLeft)
    {
        // find the point where the line intersects the right side of the image
        x = dCenterLeft;
        y = dSlope * x + dYIntercept;
    }
        
    // ensure the point is within the top and bottom bounds of the image
    if(y < -dCenterTop)
    {
        // find the point where the line intersects the top side of the image
        y = -dCenterTop;
        x = (y - dYIntercept) / dSlope;
    }
    else if(y > dCenterTop)
    {
        // find the point where the line intersects the bottom side of the image
        y = dCenterTop;
        x = (y - dYIntercept) / dSlope;
    }
}
//-------------------------------------------------------------------------------------------------
long CSpatialString::findFirstInstanceOfStringCS(const string& strSearchString, long nStartPos, 
                                                 bool bCaseSensitive)
{
    // Make local copies of the strings
    string strMaster = m_strString;
    string strSearchMaster = strSearchString;

    // Convert both strings to lower case if this is a case-insensitive search
    if (!bCaseSensitive)
    {
        makeLowerCase( strMaster );
        makeLowerCase( strSearchMaster );
    }

    // Return position
    return strMaster.find( strSearchMaster.c_str(), nStartPos );
}
//-------------------------------------------------------------------------------------------------
void getConfidenceTier(const CPPLetter& letter, const vector<unsigned char>& vecBoundaries,
	long &nConfidenceTier, unsigned char &ucLowerConfidenceBounds,
	unsigned char &ucUpperConfidenceBounds, bool bAllowHigherTier)
{
	long nOriginalTier = nConfidenceTier;
	long nConfidenceTierCount = vecBoundaries.size();

	// Determine the OCR confidence tier this zone belongs to as well as what the upper
    // and lower boundaries of the tier are by iterating through each tier in ascending
    // order until the correct one is found (if none is found, it will be in the top 
    // tier with a upper OCR bound of 100).
    unsigned char ucLastBoundary = 0;
    for (long k = 0; k < nConfidenceTierCount; k++)
    {
		if (!bAllowHigherTier && k == nConfidenceTier)
		{
			return;
		}

        // Retrieve the next boundary.
        unsigned char ucBoundary = vecBoundaries[k];

        // If the next boundary is greater or equal to the current char confidence, we
        // have found the appropriate tier.
        if (letter.m_ucCharConfidence <= ucBoundary)
        {
            ucLowerConfidenceBounds = ucLastBoundary;
            ucUpperConfidenceBounds = ucBoundary;
            nConfidenceTier = k;
            return;
        }

        ucLastBoundary = ucBoundary;
    }

	// Otherwise update the ucLowerConfidenceBounds in case there are no more tiers
    // to search (which would mean the character belongs in the top tier).
	ucLowerConfidenceBounds = ucLastBoundary;
	ucUpperConfidenceBounds = 100;
	nConfidenceTier = nConfidenceTierCount;
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CSpatialString::getOCRImageRasterZonesGroupedByConfidence(bool bByWord,
    IVariantVectorPtr ipVecOCRConfidenceBoundaries, IVariantVectorPtr &ipZoneOCRConfidenceTiers,
	IVariantVectorPtr &ipIndices)
{
    try
    {
        ASSERT_ARGUMENT("ELI25396", ipVecOCRConfidenceBoundaries != __nullptr);

        if (m_eMode != kSpatialMode)
        {
            UCLIDException ue("ELI25385", 
                "Cannot perform this operation on a hybrid or non-spatial string!");
            ue.addDebugInfo(string("String"), m_strString);
            throw ue;
        }

        IIUnknownVectorPtr ipZones(CLSID_IUnknownVector);
        ASSERT_RESOURCE_ALLOCATION("ELI25364", ipZones != __nullptr);

        // Verify the boundaries and place them into a vector for more efficient access.
        vector<unsigned char> vecBoundaries;
        unsigned char ucLastBoundary = 0;
        long nConfidenceTierCount = ipVecOCRConfidenceBoundaries->Size;
        for (int i = 0; i < nConfidenceTierCount; i++)
        {
            unsigned char ucBoundary = (unsigned char)ipVecOCRConfidenceBoundaries->GetItem(i).cVal;

            // Ensure the boundaries are specified in ascending order.
            if (ucBoundary <= ucLastBoundary)
            {
                THROW_LOGIC_ERROR_EXCEPTION("ELI25365");
            }

            vecBoundaries.push_back(ucBoundary);

            ucLastBoundary = ucBoundary;
        }

        // Create vectors to track the areas for the full set of raster zones as well as the raster
        // zones for the current line (that are divided by OCR confidence)
        vector<pair<int, CRect> > vecAllZones;
        vector<CRect> vecCurrentLineZones;
        CRect rectCurrentLineZone = grectNULL;
		bool bStartingNewWord = true;
		long nLastWordConfidence = -1;
		// Keeps track of the start of each zones in order to populate ipIndices
		long nZoneStart = -1;
		long nLastZoneStart = 0;
		// When bByWord is true, keep track of the data for the next zone to be added (but that is
		// not done being compiled.
		CRect rectPendingWordZone = grectNULL;
		bool bPendingEndOfZone = false;
		long nPendingWordConfidence = -1;
		long nPendingZoneStart = 0;
		short sPendingPage = -1;

        // This loop iterates the entire string character by character to build the raster zones.
        size_t j = 0;
        while (j < m_vecLetters.size() || rectPendingWordZone != grectNULL)
        {
            // Declare properties to apply to the next raster zone.
            short sCurrentPage = -1;
            unsigned char ucLowerConfidenceBounds = 0;
            unsigned char ucUpperConfidenceBounds = 100;
            long nConfidenceTier = nConfidenceTierCount;
            CRect rectCurrentZone = grectNULL;
            bool bEndOfZone = false;
			// If bByWord, this zones aggregates consecutive words of matching confidence tiers.
			CRect rectCombinedWordZone = grectNULL;

			// If there is a existing word zone that was not included in the last returned raster
			// because of a character in a different confidence tier, initialize a new zone for the
			// pending word.
			if (rectPendingWordZone != grectNULL)
			{
				rectCombinedWordZone = rectPendingWordZone;
				rectCurrentZone = rectPendingWordZone;
				rectPendingWordZone = grectNULL;
				nLastWordConfidence = nPendingWordConfidence;
				nLastZoneStart = nPendingZoneStart;

				bEndOfZone = bPendingEndOfZone;
				if (bEndOfZone)
				{
					sCurrentPage = sPendingPage;
				}
				bPendingEndOfZone = false;
			}

            // This loop iterates through all characters belonging to a single raster zone.
            while (!bEndOfZone && j < m_vecLetters.size())
            {
                // Ignore non-spatial characters.
                if (!m_vecLetters[j].m_bIsSpatial)
                {
					// [DataEntry:1119]
					// Even though this char is non-spatial, still check to see if the subsequent
					// char(s) indicate that this is the end of a line.
					if (getIsEndOfLine(j))
					{
						j++;
						bEndOfZone = true;
						break;
					}

                    j++;
                    continue;
                }

				// When bByWord, new zones may start only on the first character of a word.
				if (bByWord && bStartingNewWord)
				{
					nZoneStart = j;
				}

                // If sCurrentPage is -1, we are starting a new zone.  Initialize the zones properties.
                if (sCurrentPage == -1)
                {
                    // Determine the page the zone will be on.
                    sCurrentPage = m_vecLetters[j].m_usPageNumber;

					nZoneStart = j;
					getConfidenceTier(m_vecLetters[j], vecBoundaries, nConfidenceTier,
						ucLowerConfidenceBounds, ucUpperConfidenceBounds, true);
					bStartingNewWord = false;
                }
                // If we have changed pages, this is the end of the raster zone
                else if (sCurrentPage != m_vecLetters[j].m_usPageNumber)
                {
                    bEndOfZone = true;
                    break;
                }
				// If bByWord, check each letter's confidence to see if it lowers the confidence
				// tier of the word as a whole.
				else if (bByWord)
				{
					// Will only allow confidence tier to be lowered, not raised, unless this is the
					// first char of a new word
					getConfidenceTier(m_vecLetters[j], vecBoundaries,
						nConfidenceTier, ucLowerConfidenceBounds, ucUpperConfidenceBounds, bStartingNewWord);
					bStartingNewWord = false;
				}
                // If the current letter does not fall within the OCR confidence bounds of the current
                // raster zone, break out of the current raster zone loop.
                else if (m_vecLetters[j].m_ucCharConfidence <= ucLowerConfidenceBounds ||
                    m_vecLetters[j].m_ucCharConfidence > ucUpperConfidenceBounds)
                {
                    break;
                }

                // Get the spatial area of the current character.
                CRect rectLetter(m_vecLetters[j].m_ulLeft, m_vecLetters[j].m_ulTop, 
                    m_vecLetters[j].m_ulRight, m_vecLetters[j].m_ulBottom);

                // Combine the character's area with the area of the current raster zone as a whole.
                rectCurrentZone.UnionRect(rectCurrentZone, rectLetter);

				// Got to the end of the word when grouping zones by word. Determine whether to
				// break off a zone at this point.
				if (bByWord && getIsEndOfWord(j))
				{
					bStartingNewWord = true;

					// If this was the first word of a new zone, initialize.
					if (rectCombinedWordZone == grectNULL)
					{
						nLastZoneStart = nZoneStart;
						nLastWordConfidence = nConfidenceTier;
						rectCombinedWordZone = rectCurrentZone;
					}
					// If the confidence tier was the same, extend the current word zone.
					else if (nConfidenceTier == nLastWordConfidence)
					{
						rectCombinedWordZone.UnionRect(rectCombinedWordZone, rectCurrentZone);
					}
					// If the confidence for this word differed from the first word.
					else
					{
						bPendingEndOfZone = true;
						rectPendingWordZone = rectCurrentZone;
						nPendingWordConfidence = nConfidenceTier;
						nPendingZoneStart = nZoneStart;
						sPendingPage = sCurrentPage;
						
						rectCurrentZone = grectNULL;
						j++;
						break;
					}

					rectCurrentZone = grectNULL;
				}

                // If this character is the last of the current line, break off the raster zone (but
                // move on to the next char).
                if (getIsEndOfLine(j, rectCurrentZone))
                {
                    bEndOfZone = true;
                    j++;
                    break;
                }

                j++;
            }

			// When bByWord, use the current aggregated word zone
			if (bByWord)
			{
				rectCurrentZone = rectCombinedWordZone;
				nConfidenceTier = nLastWordConfidence;
			}

            // As long as a raster zone was initialized, apply the properties to ipZoneOCRConfidenceTiers,
            // vecCurrentLineZones and vecAllZones.
            if (sCurrentPage != -1)
            {
                // Record the confidence tier of the zone.
                ipZoneOCRConfidenceTiers->PushBack(nConfidenceTier);
				ipIndices->PushBack(nLastZoneStart);

                // Make sure the left side of the zone extends all the way back to the zone that
                // preceeded it on the same line to prevent gaps between the zones.
                if (!bByWord && rectCurrentLineZone != grectNULL)
                {
                    rectCurrentZone.left = rectCurrentLineZone.right;
                }

                // Keep track of all the raster zones on the current line so their top and bottom
                // boundaries can be unified before creating the IRasterZone instances.
                vecCurrentLineZones.push_back(rectCurrentZone);

                // Check to see if the current line's raster zones need to be finalized.
                // [LegacyRCAndUtils:5412] getEndOfLine doesn't always end up getting set to true
				// for the last character in the SpatialString. If this is the last character, it
				// is the end of the zone.
                if (bEndOfZone || j == m_vecLetters.size())
                {	
                    // If more than one raster zone shares the same line, ensure the top and bottom
                    // borders are all the same.
                    if (rectCurrentLineZone != grectNULL)
                    {
                        int top = min(rectCurrentZone.top, rectCurrentLineZone.top);
                        int bottom = max(rectCurrentZone.bottom, rectCurrentLineZone.bottom);

                        for (vector<CRect>::iterator iter = vecCurrentLineZones.begin();
                            iter != vecCurrentLineZones.end(); iter++)
                        {
                            iter->top = top;
                            iter->bottom = bottom;
                        }

                        rectCurrentLineZone = grectNULL;
                    }

                    // Add the raster zones's page and image area as pairs to vecAllZones.
                    for each (CRect rectZone in vecCurrentLineZones)
                    {
                        vecAllZones.push_back(pair<int, CRect>((int)sCurrentPage, rectZone));
                    }

                    vecCurrentLineZones.clear();
                }
                else
                {
                    // Update the overall bounds for the current line.
                    rectCurrentLineZone.UnionRect(rectCurrentLineZone, rectCurrentZone);
                }
            }
        }

        // Cycle through all raster zone areas that have been collected and create the
        // IUnknownVector of IRasterZones.
        for each (pair<int, CRect> zone in vecAllZones)
        {
            long lHeight = zone.second.Height();

            // Ensure height is at least 5 and an odd number
            if (lHeight < 5)
            {
                lHeight = 5;
            }
            else if ((lHeight % 2) == 0)
            {
                lHeight++;
            }

            // Turn the bounding box into start and end point values
            long lStartX = zone.second.left;
            long lStartY = (zone.second.bottom + zone.second.top) / 2;
            long lEndX = zone.second.right;
            long lEndY = lStartY;

            UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipZone(CLSID_RasterZone);
            ASSERT_RESOURCE_ALLOCATION("ELI25386", ipZone != __nullptr);

            // Build the raster zone
            ipZone->CreateFromData(lStartX, lStartY, lEndX, lEndY, lHeight, zone.first);

            ipZones->PushBack(ipZone);
        }

        return ipZones;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25816");
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CSpatialString::getOriginalImageRasterZonesGroupedByConfidence(bool bByWord,
	IVariantVectorPtr ipVecOCRConfidenceBoundaries, IVariantVectorPtr &ipZoneOCRConfidenceTiers,
	IVariantVectorPtr &ipIndices)
{
    try
    {
        // Get the untranslated zones (OCR image zones)
        IIUnknownVectorPtr ipZones = getOCRImageRasterZonesGroupedByConfidence(bByWord,
            ipVecOCRConfidenceBoundaries, ipZoneOCRConfidenceTiers, ipIndices);
        ASSERT_RESOURCE_ALLOCATION("ELI25817", ipZones != __nullptr);

        // Create a new return vector
        IIUnknownVectorPtr ipNewZones(CLSID_IUnknownVector);
        ASSERT_RESOURCE_ALLOCATION("ELI25818", ipNewZones != __nullptr);

        // Iterate through each of the found zones and translate them to original image coordinates
        long lSize = ipZones->Size();
        for (long i=0; i < lSize; i++)
        {
            UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipZone = ipZones->At(i);
            ASSERT_RESOURCE_ALLOCATION("ELI25819", ipZone != __nullptr);

            UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipNewZone = translateToOriginalImageZone(ipZone);
            ASSERT_RESOURCE_ALLOCATION("ELI25820", ipNewZone != __nullptr);

            ipNewZones->PushBack(ipNewZone);
        }

        return ipNewZones;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25821");
}
//-------------------------------------------------------------------------------------------------
UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr CSpatialString::translateToOriginalImageZone(
    UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipZone)
{
    try
    {
        ASSERT_ARGUMENT("ELI25387", m_eMode != kNonSpatialMode);
        ASSERT_ARGUMENT("ELI25397", ipZone != __nullptr);

        // Get the data from the raster zone
        long lStartX, lStartY, lEndX, lEndY, lHeight, lPageNum;
        ipZone->GetData(&lStartX, &lStartY, &lEndX, &lEndY, &lHeight, &lPageNum);

        // Return the a new raster zone containing the translated data
        return translateToOriginalImageZone(lStartX, lStartY, lEndX, lEndY, lHeight, lPageNum);
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25822");
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::translateToNewPageInfo(CPPLetter *pLetters, long nNumLetters,
	ILongToObjectMapPtr ipNewPageInfoMap)
{
    try
    {
        ASSERT_ARGUMENT("ELI36399", m_eMode != kNonSpatialMode);

		long nCurrentPage = -1;
		double theta = 0;
		CPoint pointOrigCenter;
		CPoint pointDestCenter;
		UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr ipOrigPageInfo = __nullptr;
		UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr ipNewPageInfo = __nullptr;

		for (long i = 0; i < nNumLetters; i++)
		{
			CPPLetter& letter = pLetters[i];

			if (!letter.m_bIsSpatial)
			{
				continue;
			}

			// Obtain the original page info associated with this spatial string.
			if (nCurrentPage != (long)letter.m_usPageNumber)
			{
				nCurrentPage = (long)letter.m_usPageNumber;
				ipOrigPageInfo = m_ipPageInfoMap->GetValue(nCurrentPage);
				ASSERT_RESOURCE_ALLOCATION("ELI36400", ipOrigPageInfo != __nullptr);

				ipNewPageInfo = (ipNewPageInfoMap == __nullptr) 
					? __nullptr
					: ipNewPageInfoMap->GetValue(nCurrentPage);

				// Get the page information
				UCLID_RASTERANDOCRMGMTLib::EOrientation eOrient;
				double deskew;
				long nWidth, nHeight;
				ipOrigPageInfo->GetPageInfo(&nWidth, &nHeight, &eOrient, &deskew);
				bool bInvertOrigCoordinates = (eOrient == kRotLeft || eOrient == kRotRight);
				// The bounding box's coordinates are relative to the rotated image.
				// The original image coordinates need to be inverted to obtain the center point of an
				// image that has been rotated to the left or right.
				pointOrigCenter = getImageCenterPoint(nWidth, nHeight, bInvertOrigCoordinates);

				bool bInvertFinalCoordinates = false;
				if (ipNewPageInfo != __nullptr)
				{
					ipNewPageInfo->GetPageInfo(&nWidth, &nHeight, &eOrient, &deskew);
					bInvertFinalCoordinates = (eOrient == kRotLeft || eOrient == kRotRight);
				}

				pointDestCenter = getImageCenterPoint(nWidth, nHeight, bInvertFinalCoordinates);

				// The angle associated with the current coordinate system.
				double dOrigTheta = ipOrigPageInfo->GetTheta();

				// The angle associated with the coordinate system we are converting to.
				double dNewTheta = (ipNewPageInfo == __nullptr) ? 0 : ipNewPageInfo->GetTheta();

				// The angle difference between the new and old coordinate systems.
				theta = dNewTheta - dOrigTheta;
			}

			// turn the bounding box into a start and end point
			// using the rotated image's top-left coordinate system
			double p1X = (double)letter.m_ulLeft;
			double p1Y = (double)letter.m_ulTop;
			double p2X = (double)letter.m_ulRight;
			double p2Y = (double)letter.m_ulBottom;

			// Translate the center of the page to the origin
			// (ie. convert bounding box's coordinates to Cartesian 
			// coordinate system with center of image at the origin).
			p1X -= pointOrigCenter.x;
			p1Y = pointOrigCenter.y - p1Y;
			p2X -= pointOrigCenter.x;
			p2Y = pointOrigCenter.y - p2Y;

			// rotate the start and end point counter-clockwise about the origin
			double sintheta = sin(theta);
			double costheta = cos(theta);
			double np1X = p1X*costheta - p1Y*sintheta;
			double np1Y = p1X*sintheta + p1Y*costheta;
			double np2X = p2X*costheta - p2Y*sintheta;
			double np2Y = p2X*sintheta + p2Y*costheta;

			// ensure that the points fit within the bounds of the original image
			fitPointsWithinBounds(np1X, np1Y, np2X, np2Y, pointDestCenter.x, pointDestCenter.y);

			// convert coordinates from Cartesian coordinate system to
			// original image's top-left coordinate system
			np1X += pointDestCenter.x;
			np1Y = pointDestCenter.y - np1Y;
			np2X += pointDestCenter.x;
			np2Y = pointDestCenter.y - np2Y;

			// Swap points if necessary so that start is less than end
			// or else highlights may look wrong (too small)
			letter.m_ulLeft = (long)min(np1X, np2X);
			letter.m_ulTop = (long)min(np1Y, np2Y);
			letter.m_ulRight = (long)max(np1X, np2X);
			letter.m_ulBottom = (long)max(np1Y, np2Y);
		}
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI36401");
}
//-------------------------------------------------------------------------------------------------
UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr CSpatialString::translateToOriginalImageZone(
    long lStartX, long lStartY, long lEndX, long lEndY, long lHeight, long nPage)
{
    try
    {
        return translateToNewPageInfo(lStartX, lStartY, lEndX, lEndY, lHeight, nPage, getPageInfoMap(), __nullptr);
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25823");
}
//-------------------------------------------------------------------------------------------------
UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr CSpatialString::translateToNewPageInfo(
    UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipZone,
	ILongToObjectMapPtr ipOldPageInfoMap,
	ILongToObjectMapPtr ipNewPageInfoMap)
{
    try
    {
        ASSERT_ARGUMENT("ELI28026", ipZone != __nullptr);

        // Get the data from the raster zone
        long lStartX, lStartY, lEndX, lEndY, lHeight, lPageNum;
        ipZone->GetData(&lStartX, &lStartY, &lEndX, &lEndY, &lHeight, &lPageNum);
        
		// [FlexIDSCore:5308]
		// If ipNewPageInfoMap is specified, but doesn't contain an entry for lPageNum, treat
		// ipNewPageInfoMap as if it were null (use the image coordinates).
        UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr ipNewPageInfo = __nullptr;
        if (ipNewPageInfoMap != __nullptr && asCppBool(ipNewPageInfoMap->Contains(lPageNum)))
        {
            ipNewPageInfo = ipNewPageInfoMap->GetValue(lPageNum);
            ASSERT_RESOURCE_ALLOCATION("ELI28027", ipNewPageInfo != __nullptr);
        }

        // Return the a new raster zone containing the translated data
        return translateToNewPageInfo(lStartX, lStartY, lEndX, lEndY, lHeight, lPageNum,
			ipOldPageInfoMap,
            ipNewPageInfo);
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28178");
}
//-------------------------------------------------------------------------------------------------
UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr CSpatialString::translateToNewPageInfo(
    long lStartX, long lStartY, long lEndX, long lEndY, long lHeight, int nPage,
	ILongToObjectMapPtr ipOldPageInfoMap,
    UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr ipNewPageInfo)
{
    try
    {
        ASSERT_ARGUMENT("ELI25704", ipOldPageInfoMap != __nullptr);
        ASSERT_ARGUMENT("ELI46450", ipOldPageInfoMap->Contains(nPage));

        // Now build the raster zone for this page
        UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipNewZone(CLSID_RasterZone);
        ASSERT_RESOURCE_ALLOCATION("ELI25390", ipNewZone != __nullptr);

        // Ensure some height conditions are met (don't ask me)
        if (lHeight < 5)
        {
            lHeight = 5;
        }
        else if ((lHeight % 2) == 0)
        {
            lHeight++;
        }

        // turn the bounding box into a start and end point
        // using the rotated image's top-left coordinate system
        double p1X = (double)lStartX;
        double p1Y = (double)lStartY;
        double p2X = (double)lEndX;
        double p2Y = (double)lEndY;

        // Obtain the original page info associated with this spatial string.
        UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr ipOrigPageInfo =
            ipOldPageInfoMap->GetValue(nPage);
        ASSERT_RESOURCE_ALLOCATION("ELI28028", ipOrigPageInfo != __nullptr);

        // Get the page information
        long lOriginalHeight, lOriginalWidth;
        UCLID_RASTERANDOCRMGMTLib::EOrientation eOrient;
        double deskew;
        ipOrigPageInfo->GetPageInfo(&lOriginalWidth, &lOriginalHeight, &eOrient, &deskew);

		// Short-circuit if original info is not rotated and new info is null (translating to image coordinates)
		// I added this to prevent issues with 1-char-wide zones being interpreted as vertical lines but this is a decent optimisation in any case
		if (eOrient == 0 && deskew == 0 && ipNewPageInfo == __nullptr)
		{
			ipNewZone->CreateFromData(lStartX, lStartY, lEndX, lEndY, lHeight, nPage);
			return ipNewZone;
		}

        // Ensure same height conditions as above are met for page dimensions (else x coordinates will become -inf)
        if (lOriginalHeight < 5)
        {
            lOriginalHeight = 5;
        }
        else if ((lOriginalHeight % 2) == 0)
        {
            lOriginalHeight++;
        }

        // Define the center of the image relative to the 
        // top-left coordinate system of the rotated image.
        // NOTE: ipOrigPageInfo's height and width are relative to the original image.
        // The bounding box's coordinates are relative to the rotated image.
        // The original image coordinates need to be inverted to obtain the center point of an image
        // that has been rotated to the left or right.
        bool invertOrigCoordinates = (eOrient == kRotLeft || eOrient == kRotRight);
        CPoint pointCenter = getImageCenterPoint(lOriginalWidth, lOriginalHeight,
            invertOrigCoordinates);

        // Translate the center of the page to the origin
        // (ie. convert bounding box's coordinates to Cartesian 
        // coordinate system with center of image at the origin).
        p1X -= pointCenter.x;
        p1Y = pointCenter.y - p1Y;
        p2X -= pointCenter.x;
        p2Y = pointCenter.y - p2Y;

        // The angle associated with the current coordinate system.
        double dOrigTheta = ipOrigPageInfo->GetTheta();

        // The angle associated with the coordinate system we are converting to.
        double dNewTheta = (ipNewPageInfo == __nullptr) ? 0 : ipNewPageInfo->GetTheta();

        // The angle difference between the new and old coordinate systems.
        double theta = dNewTheta - dOrigTheta;

        // rotate the start and end point counter-clockwise about the origin
        double sintheta = sin(theta);
        double costheta = cos(theta);
        double np1X = p1X*costheta - p1Y*sintheta;
        double np1Y = p1X*sintheta + p1Y*costheta;
        double np2X = p2X*costheta - p2Y*sintheta;
        double np2Y = p2X*sintheta + p2Y*costheta;

        // In order to move the coordinate system origin back to the top-left of the image we will
        // need to invert the x and y coordinates of the center if the new image coordinate system is
        // rotated to the left or right.
        bool invertFinalCoordinates = false;
        if (ipNewPageInfo != __nullptr)
        {
            invertFinalCoordinates =
                    (ipNewPageInfo->Orientation == kRotLeft || 
                     ipNewPageInfo->Orientation == kRotRight);
        }

        // translate the center of the page back
        // (ie. convert to original image's top-left coordinate system)
        // NOTE: If the original image was skewed, it is possible that
        // one of more of these two points exists in the deskewed image
        // but are outside the boundaries of the original skewed image.
        // In this case, we will need to make sure the points are contained
        // within the original image.
        pointCenter =
                getImageCenterPoint(lOriginalWidth, lOriginalHeight, invertFinalCoordinates);

        // ensure that the points fit within the bounds of the original image
        fitPointsWithinBounds(np1X, np1Y, np2X, np2Y, pointCenter.x, pointCenter.y);

        // convert coordinates from Cartesian coordinate system to
        // original image's top-left coordinate system
        np1X += pointCenter.x;
        np1Y = pointCenter.y - np1Y;
        np2X += pointCenter.x;
        np2Y = pointCenter.y - np2Y;
        
        // create the zone
        ipNewZone->CreateFromData((long) np1X, (long) np1Y, (long) np2X, (long) np2Y,
            lHeight, nPage);

        return ipNewZone;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28029");
}
//-------------------------------------------------------------------------------------------------
CPoint CSpatialString::getImageCenterPoint(int nImageWidth, int nImageHeight, bool invertCoordinates)
{
    CPoint pointCenter(nImageWidth / 2, nImageHeight / 2);

    if (invertCoordinates)
    {
        pointCenter = CPoint(pointCenter.y, pointCenter.x);
    }

    return pointCenter;
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::downgradeToHybrid()
{
    try
    {
        // Cannot downgrade from non-spatial mode
        if(m_eMode == kSpatialMode)
        {
            // Compute the vector of raster zones
            vector<UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr> vecZones = getOCRImageRasterZones();

            // set the collection of zones
            m_vecRasterZones = vecZones;

            // Clear the Letters vector
            m_vecLetters.clear();

            // Set the new mode
            m_eMode = kHybridMode;

            // Since things are changed, set the dirty flag
            m_bDirty = true;
        }
        else if (m_eMode == kNonSpatialMode)
        {
            UCLIDException ue("ELI15076", "Invalid downgrade attempt!");
            ue.addDebugInfo("From:", asString( m_eMode) );
            ue.addDebugInfo("To:", "Hybrid" );
            throw ue;
        }
        // nothing to do if the string is already hybrid
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25824");
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::downgradeToNonSpatial()
{
    try
    {
        // No matter what mode this object is in, clear the spatial 
        // information and set it to non-spatial mode
        m_vecLetters.clear();

        m_vecRasterZones.clear();

        m_eMode = kNonSpatialMode;

        // Clear the Spatial Page Info pointer
        m_ipPageInfoMap = __nullptr;

        m_bDirty = true;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25825");
}
//-------------------------------------------------------------------------------------------------
long CSpatialString::getFirstPageNumber()
{
    try
    {
        // this operation requires that this object is spatial
        if(m_eMode == kNonSpatialMode)
        {
            UCLIDException ue("ELI15090","GetFirstPageNumber() requires spatial or hybrid mode!");
            throw ue;
        }

        long nFirstPage = 0;
        if( m_eMode == kHybridMode )
        {
            // Use the bNotInitialized flag and a starting value of 0 to make sure that 
            // there is at least 1 useful page number in the raster zone.
            bool bNotInitialized = true;
            size_t nSize = m_vecRasterZones.size();
            for(size_t i = 0; i < nSize; i++)
            {
                UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipRZone = m_vecRasterZones[i];
                ASSERT_RESOURCE_ALLOCATION("ELI19471", ipRZone != __nullptr);

                // If the raster zone's page is before the current first page, replace the current 
                // page with the raster zone's page value.
                long nPage = ipRZone->PageNumber;
                if( (nFirstPage > nPage) || bNotInitialized )
                {
                    nFirstPage = nPage;
                    bNotInitialized = false;
                }
            }

            // If there is not at least 1 raster zone to get a page number from, 
            // this object is in a bad state and an exception should be thrown.
            if( bNotInitialized )
            {
                UCLIDException ue("ELI15463", "Unable to find first page number in hybrid string!");
                ue.addDebugInfo("nRasterZones", m_vecRasterZones.size() );
                throw ue;
            }
        }
        else if( m_eMode == kSpatialMode )
        {
            CPPLetter letter;
            long nIndex = getNextOCRImageSpatialLetter(0, letter);

            // We know this is a spatial string so nIndex should be valid
            // because there must be at least one spatial character
            if (nIndex < 0 || (unsigned long) nIndex >= m_vecLetters.size())
            {
                THROW_LOGIC_ERROR_EXCEPTION("ELI10468");
            }

            nFirstPage = letter.m_usPageNumber;
        }
        else
        {
            UCLIDException ue("ELI15092", "Invalid mode for Get First Page Number!");
            ue.addDebugInfo("Mode:", asString( m_eMode ));
            throw ue;
        }

        return nFirstPage;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25826");
}
//-------------------------------------------------------------------------------------------------
long CSpatialString::getLastPageNumber()
{
    try
    {
        // this operation requires that this object is spatial
        if(m_eMode == kNonSpatialMode)
        {
            UCLIDException ue("ELI15095","getLastPageNumber() requires spatial or hybrid mode!");
            throw ue;
        }

        long nLastPage = -1;
        if ( m_eMode == kHybridMode )
        {
            size_t nSize = m_vecRasterZones.size();
            for(size_t i = 0; i < nSize; i++)
            {
                UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipRZone = m_vecRasterZones[i];
                ASSERT_RESOURCE_ALLOCATION("ELI15093", ipRZone != __nullptr);

                long nPageNum = ipRZone->PageNumber;

                // If the raster zone's page is after the current last page, replace the current 
                // page with the raster zone's page value.
                if( nLastPage < nPageNum)
                {
                    nLastPage = nPageNum;
                }
            }
        }
        else if ( m_eMode == kSpatialMode )
        {
            for (long i = m_vecLetters.size() - 1; i >= 0; i--)
            {
                if (m_vecLetters[i].m_bIsSpatial)
                {
                    nLastPage = m_vecLetters[i].m_usPageNumber;
                    break;
                }
            }
        }
        else
        {
            UCLIDException ue("ELI09755", "Invalid mode for Get Last Page Number!");
            ue.addDebugInfo("Mode:", asString( m_eMode ));
            throw ue;
        }

        return nLastPage;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25827");
}
//-------------------------------------------------------------------------------------------------
long CSpatialString::getNextOCRImageSpatialLetter(long nStart, CPPLetter& rLetter)
{
    try
    {
        // Make sure this string is spatial
        verifySpatialness();

        verifyValidIndex(nStart);

        long nIndex = -1;
        long nSize = m_vecLetters.size();
        for (long i = nStart; i < nSize; i++)
        {
            CPPLetter& letter = m_vecLetters[i];
            if (letter.m_bIsSpatial)
            {
                nIndex = i;

                // Copy the letter
                rLetter = letter;
                break;
            }
        }

        return nIndex;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25828");
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CSpatialString::getWords()
{
    try
    {
        // Create vector for resulting ISpatialStrings
        IIUnknownVectorPtr ipWords(CLSID_IUnknownVector);
        ASSERT_RESOURCE_ALLOCATION("ELI25829", ipWords != __nullptr);

        long nStartPos = 0;
        long nNumLetters = m_strString.size();
        for (long i = 0; i < nNumLetters; i++)
        {
            if (getIsEndOfWord(i)) 
            {
                // get the word beginning at nStartPos and ending with current letter
                UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipItem = getSubString(nStartPos, i);
                ASSERT_RESOURCE_ALLOCATION("ELI15371", ipItem != __nullptr);

                // Add this word to the return vector
                ipWords->PushBack(ipItem);

                // Move the Start position of next word to after end of current word
                nStartPos = i + 1;

                // skip any whitespace
                for( long j = i+1; j < nNumLetters; j++)
                {
                    char c = m_strString[j];

                    if( isWhitespaceChar(c) )
                    {
                        nStartPos++;
                    }
                    else
                    {
                        i = j-1;
                        break;
                    }
                }
            }
        }

        // Check for case last letter not marked as end of word
        if (nStartPos < nNumLetters) 
        {
            // get the word beginning at nStartPos and ending with last letter
            UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipItem =
                getSubString(nStartPos, nNumLetters - 1);
            ASSERT_RESOURCE_ALLOCATION("ELI16914", ipItem != __nullptr);

            ipWords->PushBack(ipItem);
        }

        return ipWords;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25830");
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::getLines(vector<pair<long, long>>& rvecLines)
{
    try
    {
        // Clear the vector first
        rvecLines.clear();

        long nStartPos = 0;
        long nNumLetters = m_strString.size();
        for (long i = 0; i < nNumLetters; i++)
        {
            if (getIsEndOfLine(i)) 
            {
                // get the line beginning ending with current letter
                rvecLines.push_back(pair<long, long>(nStartPos, i));

                // Move the Start position of next line to after end of this line
                nStartPos = i + 1;
            }
            else
            {
                char cLetter = m_strString[i];

                if (cLetter == '\r' || cLetter == '\n')
                {
                    // skip this letter
                    nStartPos = i + 1;
                }
            }
        }

        // Check for case last letter not marked as end of line
        if (nStartPos < nNumLetters) 
        {
            // get the line beginning at nStartPos and ending with last letter
            rvecLines.push_back(pair<long, long>(nStartPos, nNumLetters - 1));
        }
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25832");
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::getLinesAndZones(vector<pair<long, long>>& rvecLines)
{
    try
    {
        // Clear the vector first
        rvecLines.clear();

		bool bIsTextBased = std::all_of(m_vecLetters.begin(), m_vecLetters.end(), [] (const CPPLetter &letter)
			{
				return !letter.m_bIsSpatial
					|| letter.m_ulTop == 0
					&& letter.m_ulBottom <= 5 // Starts as 1 but allow for inflation (not sure if this happens...)
					&& (letter.m_ulRight - letter.m_ulLeft) == 1;
			});

        long nStartPos = 0;
        long nNumLetters = m_strString.size();
        for (long i = 0; i < nNumLetters; i++)
        {
            if (getIsEndOfLine(i) || (bIsTextBased && getIsEndOfZone(i, true))) 
            {
                // get the line beginning ending with current letter
                rvecLines.push_back(pair<long, long>(nStartPos, i));

                // Move the Start position of next line to after end of this line
                nStartPos = i + 1;
            }
            else
            {
                char cLetter = m_strString[i];

                if (cLetter == '\r' || cLetter == '\n')
                {
                    // skip this letter
                    nStartPos = i + 1;
                }
            }
        }

        // Check for case last letter not marked as end of line
        if (nStartPos < nNumLetters) 
        {
            // get the line beginning at nStartPos and ending with last letter
            rvecLines.push_back(pair<long, long>(nStartPos, nNumLetters - 1));
        }
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI48354");
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CSpatialString::getLinesUnknownVector()
{
    try
    {
        // Create an IUnknownVector to be returned
        IIUnknownVectorPtr ipLines(CLSID_IUnknownVector);
        ASSERT_RESOURCE_ALLOCATION("ELI26015", ipLines != __nullptr);

        // Get the start and end position for each line
        vector<pair<long, long>> vecLines;
        getLines(vecLines);

        // Get a substring for each line and add it to an IUnknownVector
        for (vector<pair<long, long>>::iterator it = vecLines.begin(); it != vecLines.end(); it++)
        {
            UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipLine = getSubString(it->first, it->second);
            ASSERT_RESOURCE_ALLOCATION("ELI26016", ipLine != __nullptr);

            ipLines->PushBack(ipLine);
        }

        return ipLines;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26017");
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CSpatialString::getParagraphs()
{
    try
    {
        // Make sure string is spatial and has data
        if( m_eMode != kSpatialMode )
        {
            UCLIDException ue( "ELI25833", 
                "getParagraphs() requires a spatial string in spatial mode!");
            ue.addDebugInfo("Mode:", asString (m_eMode) );
            throw ue;
        }
        
        // Create vector for resulting ISpatialStrings
        IIUnknownVectorPtr ipParagraphs(CLSID_IUnknownVector);
        ASSERT_RESOURCE_ALLOCATION("ELI25834", ipParagraphs != __nullptr);

        long nStartPos = 0;
        long nNumLetters = m_vecLetters.size();
        for (long i = 0; i < nNumLetters; i++)
        {
            // Get CPPLetter
            CPPLetter& letter = m_vecLetters[i];
            
            if (letter.m_bIsEndOfParagraph) 
            {
                // get the paragraph beginning ending with current letter
                UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipItem = getSubString(nStartPos, i);
                ASSERT_RESOURCE_ALLOCATION("ELI16916", ipItem != __nullptr);

                ipParagraphs->PushBack(ipItem);

                // Move the Start position of next paragraphs to after end of this paragraph
                nStartPos = i + 1;
            }
        }

        // Check for case last letter not marked as end of paragraph
        if (nStartPos < nNumLetters) 
        {
            // get the paragraph beginning at nStartPos and ending with last letter
            UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipItem =
                getSubString(nStartPos, nNumLetters - 1);
            ASSERT_RESOURCE_ALLOCATION("ELI16917", ipItem != __nullptr);

            ipParagraphs->PushBack(ipItem);
        }

        return ipParagraphs;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25835");
}
//-------------------------------------------------------------------------------------------------
long CSpatialString::getAverageCharHeight()
{
    try
    {
        // Make sure this string is spatial
        verifySpatialness();

        // Keep track of the width of all the characters put together
        // and the number of character in order to calculate the mean width
        long totalCharHeight = 0;
        long numChars = 0;

        // Default the return value to 0
        long lReturnVal = 0;

        // Get the average width of all the spatial characters in the string(don't worry about 
        // space between chars)
        for (unsigned int uiLetter = 0; uiLetter < m_vecLetters.size(); uiLetter++)
        {	
            CPPLetter& letter = m_vecLetters[uiLetter];

            if(!letter.m_bIsSpatial)
            {
                continue;
            }

            totalCharHeight += letter.m_ulBottom - letter.m_ulTop;
            numChars++;
        }

        // Check for characters found
        if (numChars > 0)
        {
            // Calculate the average height of the chars.
            lReturnVal = (long)(totalCharHeight / numChars);
        }

        return lReturnVal;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28031");
}
//-------------------------------------------------------------------------------------------------
long CSpatialString::getAverageCharWidth()
{
    try
    {
        // Make sure this string is spatial
        verifySpatialness();

        // Keep track of the width of all the characters put together
        // and the number of character in order to calculate the mean width
        long totalCharWidth = 0;
        long numChars = 0;

        // Default the return value to 0
        long lReturnVal = 0;

        // Get all of the Words
        IIUnknownVectorPtr ipWords = getWords();
        ASSERT_RESOURCE_ALLOCATION("ELI25836", ipWords != __nullptr);
        long lSize = ipWords->Size();
        for (long iWord = 0; iWord < lSize; iWord++)
        {
            UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipWord = ipWords->At(iWord);
            ASSERT_RESOURCE_ALLOCATION("ELI25837", ipWord != __nullptr);
            
            if(ipWord->GetMode() != kSpatialMode)
            {
                continue;
            }

            CPPLetter* letters = NULL;
            long nNumLetters = -1;
            ipWord->GetOCRImageLetterArray(&nNumLetters, (void**)&letters);

            // we need at least to characters to determine the width of the first character
            if (nNumLetters < 2)
            {
                continue;
            }

            // loop through the entire word
            long iLetter = 0;
            for (iLetter = 0; iLetter < nNumLetters - 1; iLetter++)
            {
                const CPPLetter& letter1 = letters[iLetter];
                const CPPLetter& letter2 = letters[iLetter + 1];
                
                if ((!letter1.m_bIsSpatial) || (!letter2.m_bIsSpatial) )
                {
                    continue;
                }

                totalCharWidth += letter2.m_ulLeft - letter1.m_ulLeft;
                numChars++;
            }
        }
        
        if (numChars > 0)
        {
            lReturnVal = (long)((float)totalCharWidth / (float)numChars);
        }
        // If no spatial characters where found then just look at each spatial letter
        else
        {
            totalCharWidth = 0;
            numChars = 0;

            // Get the average width of all the spatial characters in the string(don't worry about 
            // space between chars)
            for (unsigned int uiLetter = 0; uiLetter < m_vecLetters.size(); uiLetter++)
            {	
                CPPLetter& letter = m_vecLetters[uiLetter];

                if(!letter.m_bIsSpatial)
                {
                    continue;
                }

                long w = letter.m_ulRight - letter.m_ulLeft;

                totalCharWidth += w;
                numChars++;
            }

            // Check for characters found
            if (numChars > 0)
            {
                // Multiply the average char width by a presetConstant
                lReturnVal = (long) ((totalCharWidth / numChars) * gfCharWidthToAvgCharRatio);
            }
            else
            {
                // This should never happen and I believe it 
                // should probably throw an exception
                lReturnVal = 0;
            }
        }

        return lReturnVal;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25838");
}
//-------------------------------------------------------------------------------------------------
vector<UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr> CSpatialString::getOCRImageRasterZones()
{
    try
    {
        // Ensure the string is spatial
        if (m_eMode == kNonSpatialMode)
        {
            UCLIDException ue("ELI25839", "Cannot get raster zones from a non-spatial string!");
            throw ue;
        }

        vector<UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr> vecZones;
        if (m_eMode == kHybridMode)
        {
            // Get a copy of each of the zones
            for (vector<UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr>::iterator it = m_vecRasterZones.begin();
                it != m_vecRasterZones.end(); it++)
            {
                // Get the raster zone as a copyable object
                ICopyableObjectPtr ipCopyable(*it);
                ASSERT_RESOURCE_ALLOCATION("ELI25840", ipCopyable != __nullptr);

                // Clone the raster zone
                UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipZone = ipCopyable->Clone();
                ASSERT_RESOURCE_ALLOCATION("ELI25841", ipZone != __nullptr);

                // Add the copy to the vector of zones
                vecZones.push_back(ipZone);
            }
        }
        else if (m_eMode == kSpatialMode)
        {
            // Handle kSpatialMode objects. Create a raster zone for each line.
            vector<pair<long, long>> vecLines;
            getLinesAndZones(vecLines);

            long lLettersSize = m_vecLetters.size();
            for (vector<pair<long, long>>::iterator it = vecLines.begin();
                it != vecLines.end(); it++)
            {
                // Get the first spatial letter for this line (if no spatial letters just continue)
                CPPLetter letter;
                long lIndex = getNextOCRImageSpatialLetter(it->first, letter);
                if (lIndex == -1 || lIndex > it->second)
                {
                    continue;
                }

                // Form a rectangle from the letter
                CRect rect(letter.m_ulLeft, letter.m_ulTop, letter.m_ulRight, letter.m_ulBottom);

                // Iterate the rest of the letters in the line updating the rectangle bounds
                long lBounds = min(it->second + 1, lLettersSize);
                for (long i = lIndex+1; i < lBounds; i++)
                {
                    const CPPLetter& tempLetter = m_vecLetters[i];

                    // Only look at spatial letters
                    if (!tempLetter.m_bIsSpatial)
                    {
                        continue;
                    }

                    // Update the rectangle based on this spatial letter
                    rect.left = min(rect.left, (LONG)tempLetter.m_ulLeft);
                    rect.top = min(rect.top, (LONG)tempLetter.m_ulTop);
                    rect.right = max(rect.right, (LONG)tempLetter.m_ulRight);
                    rect.bottom = max(rect.bottom, (LONG)tempLetter.m_ulBottom);
                }

                // Create a long rectangle from the CRect
                ILongRectanglePtr ipRect(CLSID_LongRectangle);
                ASSERT_RESOURCE_ALLOCATION("ELI26006", ipRect != __nullptr);
                ipRect->SetBounds(rect.left, rect.top, rect.right, rect.bottom);

                // Create a new raster zone from the rectangle
                UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipNewZone(CLSID_RasterZone);
                ASSERT_RESOURCE_ALLOCATION("ELI26007", ipNewZone != __nullptr);
                ipNewZone->CreateFromLongRectangle(ipRect, letter.m_usPageNumber);

                // Add the new raster zone to the vector
                vecZones.push_back(ipNewZone);
            }
        }

        return vecZones;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25843");
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CSpatialString::getOCRImageRasterZonesUnknownVector()
{
    try
    {
        // Ensure the string is spatial
        if (m_eMode == kNonSpatialMode)
        {
            UCLIDException ue("ELI25844", "Cannot get raster zones from a non-spatial string!");
            throw ue;
        }

        IIUnknownVectorPtr ipZones(CLSID_IUnknownVector);
        ASSERT_RESOURCE_ALLOCATION("ELI25845", ipZones != __nullptr);

        // Get the vector of zones
        vector<UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr> vecZones = getOCRImageRasterZones();

        // Iterate the collection of zones and add them to the IUknownVector
        vector<UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr>::iterator it;
        for (it = vecZones.begin(); it != vecZones.end(); it++)
        {
            ipZones->PushBack((*it));
        }

        return ipZones;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25846");
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CSpatialString::getOriginalImageRasterZones()
{
    try
    {
        // Ensure the string is spatial
        if (m_eMode == kNonSpatialMode)
        {
            UCLIDException ue("ELI25847", "Cannot get raster zones from a non-spatial string!");
            throw ue;
        }

        IIUnknownVectorPtr ipZones(CLSID_IUnknownVector);
        ASSERT_RESOURCE_ALLOCATION("ELI25848", ipZones != __nullptr);

        // Get the OCR image raster zones
        vector<UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr> vecZones = getOCRImageRasterZones();
        for (vector<UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr>::iterator it = vecZones.begin();
            it != vecZones.end(); it++)
        {
            UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipZone(*it);
            ASSERT_RESOURCE_ALLOCATION("ELI25849", ipZone != __nullptr);
            UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipNewZone =
                translateToOriginalImageZone(ipZone);
            ASSERT_RESOURCE_ALLOCATION("ELI25850", ipNewZone != __nullptr);

            ipZones->PushBack(ipNewZone);
        }

        return ipZones;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25851");
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CSpatialString::getTranslatedImageRasterZones(
                                                            ILongToObjectMapPtr ipNewPageInfoMap)
{
    try
    {
        // Ensure the string is spatial
        if (m_eMode == kNonSpatialMode)
        {
            UCLIDException ue("ELI28032", "Cannot get raster zones from a non-spatial string!");
            throw ue;
        }

        IIUnknownVectorPtr ipZones(CLSID_IUnknownVector);
        ASSERT_RESOURCE_ALLOCATION("ELI28024", ipZones != __nullptr);

        // Get the OCR image raster zones
        vector<UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr> vecZones = getOCRImageRasterZones();
        for (vector<UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr>::iterator it = vecZones.begin();
            it != vecZones.end(); it++)
        {
            UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipZone(*it);
            ASSERT_RESOURCE_ALLOCATION("ELI28179", ipZone != __nullptr);

            UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipNewZone =
                translateToNewPageInfo(ipZone, m_ipPageInfoMap, ipNewPageInfoMap);
            ASSERT_RESOURCE_ALLOCATION("ELI28180", ipNewZone != __nullptr);

            ipZones->PushBack(ipNewZone);
        }

        return ipZones;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28181");
}
//-------------------------------------------------------------------------------------------------
ILongRectanglePtr CSpatialString::getPageBounds(long nPage, bool bUseOCRImageCoordinates)
{
    try
    {
        if (m_ipPageInfoMap == __nullptr)
        {
            throw UCLIDException("ELI30321", "Page info missing, failed to get page bounds!");
        }

        UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr ipPageInfo = m_ipPageInfoMap->GetValue(nPage);
        ASSERT_RESOURCE_ALLOCATION("ELI30322", ipPageInfo != __nullptr);

        long nWidth(-1), nHeight(-1);
        UCLID_RASTERANDOCRMGMTLib::EOrientation ePageOrientation;
        double dDeskew;
        ipPageInfo->GetPageInfo(&nWidth, &nHeight, &ePageOrientation, &dDeskew);

        if (bUseOCRImageCoordinates)
        {
            // Determine which way to orient the search based on the page text orientation.
            bool bTextIsHorizontal = ePageOrientation == kRotNone ||								 
                                     ePageOrientation == kRotDown ||
                                     ePageOrientation == kRotFlipped ||
                                     ePageOrientation == kRotFlippedDown;

            // If the page is rotated to the right or left, in terms of OCR coordinates, the page
            // dimensions need to be swapped.
            if (!bTextIsHorizontal)
            {
                swap(nWidth, nHeight);
            }
        }

        ILongRectanglePtr ipRect(CLSID_LongRectangle);
        ASSERT_RESOURCE_ALLOCATION("ELI30323", ipRect != __nullptr);
        ipRect->SetBounds(0, 0, nWidth, nHeight);

        return ipRect;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30324");
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::remove(long nStart, long nEnd)
{
    try
    {
        // validate start index
        verifyValidIndex(nStart);

        // end index must either be -1 or a valid index
        if (nEnd == -1)
        {
            // end index value of -1 means "until end of string"
            nEnd = m_strString.length() - 1;
        }
        
        // verify that nEnd is not less than nStart
        if( nEnd < nStart )
        {
            UCLIDException ue("ELI14889", "Boundary violation!");
            ue.addDebugInfo("nEnd:", asString(nEnd));
            ue.addDebugInfo("nStart:", asString(nStart));
            throw ue;
        }

        // verify nEnd is a valid index
        verifyValidIndex(nEnd);

        // remove the letters from the vector if this string is spatial
        if (m_eMode == kSpatialMode)
        {
            // Verify that the remove operation won't remove the last spatial letters from the
            // spatial string. If the remove will render this string kNonspatialMode, then this object
            // must preserve its raster zones and convert to kHybridMode

            // Get the first spatial letter in the string. If the letter is before the start of the remove
            // operation OR after the end of the remove operation, then the string will remain spatial. 
            CPPLetter letter;
            long nPos = getNextOCRImageSpatialLetter(0, letter);

            if( nPos >= nStart && nPos <= nEnd )
            {
                // However, if the first spatial letter is within the remove area, check to see if there
                // is a spatial letter after the remove area. If there is NOT, then we need to get the raster
                // zones from this object and change it to a hybrid string.
                nPos = getNextOCRImageSpatialLetter(nEnd, letter);

                // If there is not at least 1 spatial letter present after the removal spot
                if( nPos == -1 )
                {
                    downgradeToHybrid();
                }
                else
                {
                    // Treat this string the same as below
                    m_vecLetters.erase(m_vecLetters.begin() + nStart, m_vecLetters.begin() + nEnd + 1);
                }
            }
            else
            {
                // If there will be at least 1 spatial letter left, then remove the substring from
                // the letters vector.
                // nEnd + 1 because the second parameter needs to be one past the last element to
                // be removed
                m_vecLetters.erase(m_vecLetters.begin() + nStart, m_vecLetters.begin() + nEnd + 1);
            }

            // Remove the letters from the string. This must be done AFTER the calls to GetNextSpatialLetter()
            // to prevent an exception from mismatched string / vector length.
            m_strString.erase(nStart, nEnd - nStart + 1);

            // because we modified the spatial string, we need to check
            // if the string is still spatial
            reviewSpatialStringAndDowngradeIfNeeded();
        }
        else
        {
            // For kHybridMode and for kNonSpatialMode, remove the specified
            // characters from the string.
            m_strString.erase(nStart, nEnd - nStart + 1);
        }

        // set the dirty flag to true since a modification was made
        m_bDirty = true;

        // perform consistency check to ensure that the size of the
        // string and letters vector are equal (if the letters vector exists)
        performConsistencyCheck();
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25852");
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::findFirstItemInVector(IVariantVectorPtr ipList, bool bCaseSensitive,
                                           bool bPrioritizedVector, long lStartSearchPos,
                                           long &rlStart, long &rlEnd)
{
    try
    {
        // Ensure the vector is not null
        ASSERT_ARGUMENT("ELI07695", ipList != __nullptr);

        // Ensure the start position is valid
        if (lStartSearchPos < 0)
        {
            lStartSearchPos = 0;
        }

        // make a copy of m_strString to operate on
        string strLocalString(m_strString);
        if (!bCaseSensitive)
        {
            // if case insensitive, make the string to upper case
            ::makeUpperCase(strLocalString);
        }

        // Placeholders to indicate best match
        long lStartLocation = strLocalString.length();
        long lMatchLength = 0;

        // total number of items in the list
        long lCount = ipList->Size;
        for (long i = 0; i < lCount; i++)
        {
            // Retrieve each item
            string strItem = asString(_bstr_t(ipList->GetItem(i)));
            if (!bCaseSensitive)
            {
                // if case insensitive, make the string to upper case
                ::makeUpperCase(strItem);
            }

            // get each item string length
            int nItemLen = strItem.size();

            // Check for string presence in text
            if (nItemLen > 0)
            {
                // Check the string
                int nFoundPos = strLocalString.find(strItem, lStartSearchPos);
                if (nFoundPos != string::npos)
                {
                    // Check previous find
                    if (nFoundPos < lStartLocation)
                    {
                        // Earlier position, save it
                        lStartLocation = nFoundPos;

                        // Save item length
                        lMatchLength = nItemLen;
                    }
                    // Same position, now check for longer string than previous
                    else if (nFoundPos == lStartLocation)
                    {
                        if (lMatchLength < nItemLen)
                        {
                            lMatchLength = nItemLen;
                        }
                    }

                    // check if this is a prioritized vector
                    if (bPrioritizedVector)
                    {
                        // no need to search subsequent items in vector,
                        // this match takes priority.
                        break;
                    }

                }			// end if item was found in text
            }				// end if item has non-zero length
        }					// end for each item in collection

        // Search is complete, now return results
        if (lMatchLength > 0)
        {
            // Position of first character in match
            rlStart = lStartLocation;

            // Position of last character in match
            rlEnd = lStartLocation + lMatchLength - 1;
        }
        else
        {
            // Return defaults
            rlStart = -1;
            rlEnd = -1;
        }
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25853");
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::findFirstItemInRegExpVector(IVariantVectorPtr ipList, bool bCaseSensitive,
                                                bool bPrioritizedVector, long lStartSearchPos,
                                                IRegularExprParserPtr ipRegExprParser,
                                                long &rlStart, long &rlEnd)
{
    try
    {
        // Make sure the parser passed and vector passed are not NULL
        ASSERT_ARGUMENT("ELI22329", ipRegExprParser != __nullptr );
        ASSERT_ARGUMENT("ELI07698", ipList != __nullptr);

        // Ensure the start position is valid
        if (lStartSearchPos < 0)
        {
            lStartSearchPos = 0;
        }

        // Search after end of string
        if ((unsigned long) lStartSearchPos >= m_strString.length())
        {
            rlStart = -1;
            rlEnd = -1;
            return;
        }

        // Create local string to search AT starting position
        string	strSearch = m_strString.substr( 
            lStartSearchPos, m_strString.length() - lStartSearchPos );

        // to store the ealiest match position
        long lMatchStartPos = -1;
        long lMatchEndPos = -1;

        // total number of items in list
        long lCount = ipList->Size;

        // set case sensitivity
        ipRegExprParser->IgnoreCase = asVariantBool(!bCaseSensitive);

        // Examine each pattern in list
        for (long i = 0; i < lCount; i++)
        {
            // Set each item as the pattern
            ipRegExprParser->Pattern = _bstr_t(ipList->GetItem(i));

            // Search for the item
            IIUnknownVectorPtr ipMatches = ipRegExprParser->Find( 
                get_bstr_t( strSearch.c_str() ), VARIANT_TRUE, VARIANT_FALSE, VARIANT_FALSE );
            ASSERT_RESOURCE_ALLOCATION("ELI06827", ipMatches != __nullptr);

            // If match is found
            if (ipMatches->Size() > 0)
            {
                // Retrieve the match
                IObjectPairPtr ipObjectPair = ipMatches->At(0);
                ASSERT_RESOURCE_ALLOCATION("ELI06828", ipObjectPair != __nullptr);
                ITokenPtr ipToken = ipObjectPair->Object1;
                ASSERT_RESOURCE_ALLOCATION("ELI06829", ipToken != __nullptr);
                long lStartPos, lEndPos;
                ipToken->GetStartAndEndPosition(&lStartPos, &lEndPos);

                // get the earliest start position, which is greater than
                // or equal to the specified start search position
                if (lStartPos <= lMatchStartPos || lMatchStartPos < 0)
                {
                    // if the match start position is same as the earliest,
                    // and match end position is less than or equal to the 
                    // stored end position, then continue searching
                    if (lStartPos == lMatchStartPos && lEndPos <= lMatchEndPos)
                    {
                        continue;
                    }

                    // Earlier position, save it
                    lMatchStartPos = lStartPos;
                    // Store the longest match end position
                    lMatchEndPos = lEndPos;
                }

                // check if this is a prioritized vector
                if (bPrioritizedVector)
                {
                    // no need to search subsequent regular expressions in vector,
                    // this regular expression takes priority.
                    break;
                }
            }
        }

        rlStart = (lMatchStartPos > -1) ? lMatchStartPos + lStartSearchPos : -1;
        rlEnd = (lMatchStartPos > -1) ? lMatchEndPos + lStartSearchPos : -1;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25854");
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::copyFromSpatialString(UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipSource)
{
    try
    {
        ASSERT_ARGUMENT("ELI25996", ipSource != __nullptr);
        
        // Reset all member variables
        reset(true, true);

        // Get the mode of the source objec
        UCLID_RASTERANDOCRMGMTLib::ESpatialStringMode eSourceMode = ipSource->GetMode();

        // depending upon whether the source object is spatial, copy the letters
        // or the string
        if (eSourceMode == kSpatialMode)
        {
            // Need to set the spatial mode right away to enable future operations
            m_eMode = kSpatialMode;

            // create a copy of the letters associated with this object
            CPPLetter* letters = NULL;
            long nNumLetters = 0;
            ipSource->GetOCRImageLetterArray(&nNumLetters, (void**)&letters);
            
			m_vecLetters.resize(nNumLetters);
            
            if(nNumLetters > 0)
            {
				// Since the m_vecLetters was just resized to nNumLetters the copy size 
				// is the same
				long lCopySize = sizeof(CPPLetter) * nNumLetters;

				memcpy_s(&(m_vecLetters[0]), lCopySize, letters, lCopySize);
            }
        }
        // If the object is hybrid, get the raster zone(s)
        else if( eSourceMode == kHybridMode )
        {
            m_eMode = kHybridMode;

            IIUnknownVectorPtr ipZones = ipSource->GetOCRImageRasterZones();
            ASSERT_RESOURCE_ALLOCATION("ELI25775", ipZones != __nullptr);

            long lSize = ipZones->Size();
            for (long i=0; i < lSize; i++)
            {
                UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipZone = ipZones->At(i);
                ASSERT_RESOURCE_ALLOCATION("ELI25776", ipZone != __nullptr);

                m_vecRasterZones.push_back(ipZone);
            }
        }
        else
        {
            // Non Spatial string, set the mode.
            m_eMode = kNonSpatialMode;
        }

        // copy the string from the source object
        m_strString = asString(ipSource->String);
        
        // copy the source document name
        validateAndMergeSourceDocName(asString(ipSource->SourceDocName));

        // Copy the OCR Engine version
        m_strOCREngineVersion = asString(ipSource->OCREngineVersion);

        // copy the spatial page info map
        if (eSourceMode != kNonSpatialMode)
        {
			// The SpatialPageInfo class can be treated as immutable. No need to clone; just make a
			// shallow copy.
            m_ipPageInfoMap = ipSource->SpatialPageInfos;
        }

		// Copy OCR parameters
		UCLID_RASTERANDOCRMGMTLib::IHasOCRParametersPtr ipHasOCRParams(ipSource);
		ASSERT_RESOURCE_ALLOCATION("ELI45902", ipHasOCRParams != __nullptr);
		ICopyableObjectPtr ipTheOCRParams = ipHasOCRParams->OCRParameters;
		ASSERT_RESOURCE_ALLOCATION("ELI45903", ipTheOCRParams != __nullptr);
		m_ipOCRParameters = ipTheOCRParams->Clone();

        // Downgrade the spatial mode if needed
        reviewSpatialStringAndDowngradeIfNeeded();

        performConsistencyCheck();
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25995");
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::autoConvertLegacyHybridString()
{
    // Make a copy of the original page infos (OCR coordinates)
    ICopyableObjectPtr ipCopyObj = m_ipPageInfoMap;
    ASSERT_RESOURCE_ALLOCATION("ELI29753", ipCopyObj != __nullptr);

    ILongToObjectMapPtr ipPageInfoMap = ipCopyObj->Clone();
    ASSERT_RESOURCE_ALLOCATION("ELI29754", ipPageInfoMap != __nullptr);

    // Remove the any skew or rotation from m_ipPageInfoMap. (image coordinates)
    IVariantVectorPtr ipKeys = m_ipPageInfoMap->GetKeys();
    ASSERT_RESOURCE_ALLOCATION("ELI29755", ipKeys != __nullptr);

    // Check for empty PageInfoMap (Hybrid strings created in 5.0 have an empty info map)
    // [FlexIDSCore #4281]
    long nCount = ipKeys->Size;
    if (nCount == 0)
    {
        // Look for the original image to build a spatial page info map from
        if (!isValidFile(m_strSourceDocName))
        {
            UCLIDException ue("ELI30133",
                "Spatial page info map was empty and original source image was not found.");
            ue.addDebugInfo("Original File Name", m_strSourceDocName);
            throw ue;
        }

        // Get the page numbers from all raster zones
        set<int> setPageNumbers;
        for (size_t i = 0; i < m_vecRasterZones.size(); i++)
        {
            UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipZone = m_vecRasterZones[i];
            ASSERT_RESOURCE_ALLOCATION("ELI30134", ipZone != __nullptr);

            setPageNumbers.insert(ipZone->PageNumber);
        }

        // For each page, go to the original image and build the spatial page info with
        // no rotation and 0 deskew
        for(set<int>::iterator it = setPageNumbers.begin(); it != setPageNumbers.end(); it++)
        {
            // Create a new spatial page info
            UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr ipInfo(CLSID_SpatialPageInfo);
            ASSERT_RESOURCE_ALLOCATION("ELI30135", ipInfo != __nullptr);

            // Get the image dimensions and set the page info
            int nWidth(0), nHeight(0);
            getImagePixelHeightAndWidth(m_strSourceDocName, nHeight, nWidth, *it);
            ipInfo->Initialize(nWidth, nHeight,
                (UCLID_RASTERANDOCRMGMTLib::EOrientation) kRotNone, 0.0);

            // Update both the new info map and the original info map
            // (Need to update both maps for this case since the page info is not
            // contained in the original map either)
            ipPageInfoMap->Set(*it, ipInfo);
			getThisAsCOMPtr()->SetPageInfo(*it, ipInfo);
        }
    }
    else
    {
        // 0 the deskew and set the rotation to 0 for all page infos
        for (long i = 0; i < nCount; i++)
        {
            UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr ipPageInfo =
                m_ipPageInfoMap->GetValue(ipKeys->GetItem(i));
            ASSERT_RESOURCE_ALLOCATION("ELI29756", ipPageInfo != __nullptr);

			UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr ipNewPageInfo(CLSID_SpatialPageInfo);
			ASSERT_RESOURCE_ALLOCATION("ELI36292", ipNewPageInfo != __nullptr);

			// Assign a new spatial page info without orientation or deskew.
			ipNewPageInfo->Initialize(ipPageInfo->Width, ipPageInfo->Height,
				(UCLID_RASTERANDOCRMGMTLib::EOrientation)0, 0);
			getThisAsCOMPtr()->SetPageInfo(ipKeys->GetItem(i), ipNewPageInfo);
        }
    }

    // Translate the zones to account for the difference between OCR and image coordinate systems
    // (since the page infos currently represent the image coordinate system, the SpatialString
    // currently represents the opposite skew/rotation that it originally did.
    IIUnknownVectorPtr ipZones = getTranslatedImageRasterZones(ipPageInfoMap);
    ASSERT_RESOURCE_ALLOCATION("ELI29757", ipZones != __nullptr);
    
    // Re-create the string with the original page infos, but with the translated zones which will
    // cancel out the skew/rotation of the OCR coordinate system (ie, will translate it into the
    // image coordinate system).
    string strString = m_strString;
    string strSourceDocName = m_strSourceDocName;
    getThisAsCOMPtr()->CreateHybridString(ipZones, _bstr_t(strString.c_str()),
        _bstr_t(strSourceDocName.c_str()), ipPageInfoMap);
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::mergeAsHybridString(UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipStringToMerge)
{
    if (ipStringToMerge->GetMode() == kNonSpatialMode || m_eMode == kNonSpatialMode)
    {
        UCLIDException ue("ELI29866", "Cannot merge a non-spatial string as a hybrid string!");
        throw ue;
    }

    // Clone ipStringToMerge so that the source is not modified in any way. 
    ICopyableObjectPtr ipCopyObj = ipStringToMerge;
    ASSERT_RESOURCE_ALLOCATION("ELI29870", ipCopyObj != __nullptr);

    UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipStringToMergeCopy =
        (UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr)ipCopyObj->Clone();
    ASSERT_RESOURCE_ALLOCATION("ELI29871", ipStringToMergeCopy != __nullptr);

    // Translating raster zones only works on a hybrid string.
    ipStringToMergeCopy->DowngradeToHybridMode();

    // A unified spatial page infos needs to be created. Start with ipStringToMerge's
    // spatial page infos, and replace any shared pages with this string's page info.
    ipCopyObj = ipStringToMergeCopy->SpatialPageInfos;
    ASSERT_RESOURCE_ALLOCATION("ELI29872", ipCopyObj != __nullptr);

    ILongToObjectMapPtr ipUnifiedPageInfoMap = (ILongToObjectMapPtr)ipCopyObj->Clone();
    ASSERT_RESOURCE_ALLOCATION("ELI29873", ipUnifiedPageInfoMap != __nullptr);

    IVariantVectorPtr ipExistingSpatialPages = m_ipPageInfoMap->GetKeys();
    ASSERT_RESOURCE_ALLOCATION("ELI29874", ipExistingSpatialPages != __nullptr);

    long nPageCount = ipExistingSpatialPages->Size;
    for (long i = 0; i < nPageCount; i++)
    {
        long nPage = (long)ipExistingSpatialPages->Item[i];

        UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr ipPageInfo = m_ipPageInfoMap->GetValue(nPage);
        ASSERT_RESOURCE_ALLOCATION("ELI29875", ipPageInfo != __nullptr);

        ipUnifiedPageInfoMap->Set(nPage, ipPageInfo);
    }

    // In order for ipStringToMerge's rasterZones to show up in the correct spot if the spatial page
    // infos differed at all, we need to convert ipStringToMerge's raster zones into the
    // ipUnifiedPageInfoMap coordinate system.
    IIUnknownVectorPtr ipTranslatedRasterZones =
        ipStringToMergeCopy->GetTranslatedImageRasterZones(ipUnifiedPageInfoMap);
    ASSERT_RESOURCE_ALLOCATION("ELI29876", ipTranslatedRasterZones != __nullptr);

    // Recreate ipStringToMergeCopy using the translated raster zones and
    // unifiedSpatialPageInfos. The two spatial strings are now able to be
    // merged.
    ipStringToMergeCopy->CreateHybridString(ipTranslatedRasterZones, ipStringToMergeCopy->String,
        m_strSourceDocName.c_str(), ipUnifiedPageInfoMap);

    // Append the spatially compatible ipStringToMergeCopy
    append(ipStringToMergeCopy);
}
//-------------------------------------------------------------------------------------------------
long CSpatialString::getFirstCharPositionOfPage(long nPageNum)
{
	long nFirstCharPos = 0;

	// if the string is not spatial there is no way to determine the page number so return 0 
	// for the first position
	if (m_eMode != kSpatialMode)
	{
		return nFirstCharPos;
	}

	long nFirstPageNumber = getFirstPageNumber();
	long nLastPageNumber = getLastPageNumber();

	// if the first page number in the string is less than or the page number requested
	// return 0
	if (nPageNum <= nFirstPageNumber) 
	{
		return nFirstCharPos;
	}

	// if the page is higher than the last page return the last character position
	if ( nPageNum > nLastPageNumber) 
	{
		for (long i = m_vecLetters.size() - 1; i >= 0; i--)
		{
			if (m_vecLetters[i].m_bIsSpatial)
			{
				nFirstCharPos = i;
				break;
			}
		}
		return  nFirstCharPos;
	}

	// Search for the page using a binary search of the letters
	long nHigh = m_vecLetters.size() -1;
	long nLow = 0;
	long nDiff = nHigh - nLow;
	long nMiddle;
	long nHighPage = nLastPageNumber;
	long nLowPage = nFirstPageNumber;
	while (nDiff > 1)
	{
		nMiddle = nHigh - nDiff/2;
		CPPLetter *pCurrLetter = &m_vecLetters[nMiddle];
		long nMiddlePageNumber;
		if (pCurrLetter->m_bIsSpatial)
		{
			nMiddlePageNumber = pCurrLetter->m_usPageNumber;
		}
		else
		{
			// get the page number for the middle from the next spatial character
			long i = nMiddle;
			do
			{
				i++;
				pCurrLetter = &m_vecLetters[i];
			}
			while (i < nHigh && !pCurrLetter->m_bIsSpatial);

			nMiddlePageNumber = (i == nHigh) ? nHighPage : pCurrLetter->m_usPageNumber;
		}

		// Reset the High and Low based on the where the middle falls
		if (nMiddlePageNumber >= nPageNum)
		{
			nHighPage = nMiddlePageNumber;
			nHigh = nMiddle;
		}
		else
		{
			nLowPage = nMiddlePageNumber;
			nLow = nMiddle;
		}

		// Calculate the difference between the high and low for the next trip through the loop
		nDiff = nHigh - nLow;
	}

	// After the loop the high will be the first position on the page
	return nHigh;
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::setSurroundingWhitespace(UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipString,
											  long& rnPos)
{
	long nPage = ipString->GetFirstPageNumber();
	if (nPage != ipString->GetLastPageNumber())
	{
		UCLIDException ue("ELI36428",
			"Cannot adjust surrounding whitespace for a multi-page string");
		throw ue;
	}

	long nCount = m_vecLetters.size();
	if (rnPos > nCount)
	{
		UCLIDException ue("ELI36713", "Invalid SpatialString insertion position.");
		throw ue;
	}

	UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr ipPageInfo = ipString->GetPageInfo(nPage);
	ASSERT_RESOURCE_ALLOCATION("ELI36732", ipPageInfo != __nullptr);

	ILongRectanglePtr ipBounds = ipString->GetOCRImageBounds();
	long nLeft;
	long nTop;
	long nRight;
	long nBottom;
	ipBounds->GetBounds(&nLeft, &nTop, &nRight, &nBottom);

	// Whitespace that already exists before/after the insertion position.
	set<long> setExistingWS;
	set<long> setLettersToRemove;
	bool bExistingLF = false;
	long nAvgWidth = ipString->GetAverageCharWidth();
	// How much space should be allowed between the inserted chars and any existing, neighboring
	// spatial chars before a space character should be inserted between them.
	long nNewSpaceAllowance = nAvgWidth / 2 + 1;
	// How much space should be allowed between the inserted chars and any existing, neighboring
	// spatial chars before existing space characters are removed.
	// https://extract.atlassian.net/browse/ISSUE-12088
	// Allow for less of a gap before removing existing spaces.
	long nDelSpaceAllowance = nNewSpaceAllowance / 2;

	// Start at the insertion position and work backwards looking for whitespace following the
	// last preceeding spatial character. 
	for (int nPrevPos = rnPos - 1; nPrevPos >= 0; nPrevPos--)
	{
		CPPLetter &prevLetter = m_vecLetters[nPrevPos];

		if (prevLetter.m_bIsSpatial)
		{
			// Check for a new page
			if (prevLetter.m_usPageNumber != nPage)
			{
				break;
			}
			// Check for a new line using the spatial info of the string to be inserted
			int nVerticalMidPoint = (prevLetter.m_ulTop + prevLetter.m_ulBottom) / 2;
			int nHorizontalMidPoint = (prevLetter.m_ulLeft + prevLetter.m_ulRight) / 2;
			if (nVerticalMidPoint < nTop || 
				nVerticalMidPoint > nBottom || 
				nHorizontalMidPoint > nLeft)
			{
				if (!bExistingLF)
				{
					// If ipString should be on a separate line from the preceding chars but there
					// is no existing preceding whitespace, insert a newline before ipString.
					ipString->InsertString(0, "\r\n");
				}
			}
			// Check for a new word based on the spatial info of the string to be inserted
			else if ((long)prevLetter.m_ulRight + nNewSpaceAllowance < nLeft)
			{
				if (setExistingWS.empty())
				{
					// If ipString should be a separate word from the preceeding text but there is
					// no existing preceeding whitespace, insert a space before ipString.
					ipString->InsertString(0, " ");
				}
			}
			// Spatially ipString appears to be part of the preceeding word; remove any intervening
			// whitespace chars.
			else if ((long)prevLetter.m_ulRight + nDelSpaceAllowance >= nLeft)
			{
				setLettersToRemove.insert(setExistingWS.begin(), setExistingWS.end());
			}

			break;
		}
		else 
		{
			setExistingWS.insert(nPrevPos);

			if (prevLetter.m_usGuess1 == '\r' || prevLetter.m_usGuess1 == '\n')
			{
				bExistingLF = true;
			}
		}
	}

	// If any preceding whitespace was removed, the index at which ipString should be inserted needs
	// to be adjusted.
	int nPosAdjustment = setLettersToRemove.size();
	
	// Reset to check trailing whitespace.
	setExistingWS.clear();
	bExistingLF = false;

	// If string to be inserted is on a page following the end of this string, append double-newline
	if (rnPos == nCount && m_eMode != kNonSpatialMode && nPage > getLastPageNumber())
	{
		appendString("\r\n\r\n");
		nPosAdjustment -= 4;
		m_bDirty = true;
	}
	else
	{
		// Start at the insertion position and work forwards looking for whitespace preceeding the
		// last next spatial character. 
		for (int nNextPos = rnPos; nNextPos < nCount; nNextPos++)
		{
			CPPLetter &nextLetter = m_vecLetters[nNextPos];

			if (nextLetter.m_bIsSpatial)
			{
				// Check for a new page
				if (nextLetter.m_usPageNumber != nPage)
				{
					break;
				}
				// Check for a new line using the spatial info of the string to be inserted
				int nVerticalMidPoint = (nextLetter.m_ulTop + nextLetter.m_ulBottom) / 2;
				int nHorizontalMidPoint = (nextLetter.m_ulLeft + nextLetter.m_ulRight) / 2;
				if (nVerticalMidPoint < nTop || 
					nVerticalMidPoint > nBottom || 
					nHorizontalMidPoint < nRight)
				{
					if (!bExistingLF)
					{
						// If ipString should be on a separate line from the following chars but there
						// is no existing trailing whitespace, insert a newline after ipString.
						ipString->AppendString("\r\n");
					}
				}
				// Check for a new word based on the spatial info of the string to be inserted
				else if ((long)nextLetter.m_ulLeft - nNewSpaceAllowance > nRight)
				{
					if (setExistingWS.empty())
					{
						// If ipString should be a separate word from the following chars but there is
						// no existing trailing whitespace, insert a space after ipString.
						ipString->AppendString(" ");
					}
				}
				// Spatially ipString appears to be part of the following word; remove any intervening
				// whitespace chars.
				else if ((long)nextLetter.m_ulLeft - nDelSpaceAllowance <= nRight)
				{
					setLettersToRemove.insert(setExistingWS.begin(), setExistingWS.end());
				}

				break;
			}
			else
			{
				setExistingWS.insert(nNextPos);

				if (nextLetter.m_usGuess1 == '\r' || nextLetter.m_usGuess1 == '\n')
				{
					bExistingLF = true;
				}
			}
		}
	}

	// Remove any whitepsace chars identified for removal.
	if (!setLettersToRemove.empty())
	{
		// Copy all the the chars to be removed to a new CPPLetter vector.
		vector<CPPLetter> vecNewLetters;
		long nNewLetterCount = nCount - setLettersToRemove.size();
		vecNewLetters.reserve(nNewLetterCount);
		for (long i = 0; i < nCount; i++)
		{
			if (setLettersToRemove.find(i) == setLettersToRemove.end())
			{
				vecNewLetters.push_back(m_vecLetters[i]);
			}
		}

		if (nNewLetterCount == 0)
		{
			reset(false, false);
		}
		else
		{
			CPPLetter *pNewLetters = &vecNewLetters[0];
			updateLetters(pNewLetters, nNewLetterCount);
		}

		rnPos -= nPosAdjustment;
		m_bDirty = true;
	}
}
//-------------------------------------------------------------------------------------------------
ILongToObjectMapPtr CSpatialString::getUnrotatedPageInfoMap()
{
	// If page info map is null there is nothing to do
	if (m_ipPageInfoMap == __nullptr)
	{
		return __nullptr;
	}

	ILongToObjectMapPtr ipResult;
	ipResult.CreateInstance(CLSID_LongToObjectMap);
	ASSERT_RESOURCE_ALLOCATION("ELI38485", ipResult != __nullptr);

	// Zero the deskew and set the rotation to none all page infos
    IVariantVectorPtr ipKeys = m_ipPageInfoMap->GetKeys();
    ASSERT_RESOURCE_ALLOCATION("ELI38486", ipKeys != __nullptr);

    long nCount = ipKeys->Size;
	for (long i = 0; i < nCount; i++)
	{
		long nPage = ipKeys->GetItem(i);

		UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr ipPageInfo = m_ipPageInfoMap->GetValue(nPage);
		ASSERT_RESOURCE_ALLOCATION("ELI38487", ipPageInfo != __nullptr);

		UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr ipNewPageInfo(CLSID_SpatialPageInfo);
		ASSERT_RESOURCE_ALLOCATION("ELI38488", ipNewPageInfo != __nullptr);

		// Make a new spatial page info without orientation or deskew.
		long nWidth, nHeight;
		ipPageInfo->GetWidthAndHeight(&nWidth, &nHeight);
		ipNewPageInfo->Initialize(nWidth, nHeight, (UCLID_RASTERANDOCRMGMTLib::EOrientation)0, 0.0);

		ipResult->Set(nPage, ipNewPageInfo);
	}

	return ipResult;
}
//-------------------------------------------------------------------------------------------------
UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr CSpatialString::makeBlankPage(int nPage, string textForPage)
{
	// Create the spatial string for the page
	UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipPage(CLSID_SpatialString);
	ASSERT_RESOURCE_ALLOCATION("ELI39512", ipPage != __nullptr);

	// Create non-spatial string if there is no text supplied
	if (textForPage.empty())
	{
		ipPage->CreateNonSpatialString("", m_strSourceDocName.c_str());
	}
	// Else it can be pseudo-spatial
	else
	{
		// Get the size of the page
		int nHeight, nWidth;
		getImagePixelHeightAndWidth(m_strSourceDocName, nHeight, nWidth, nPage);

		// Set a long rectangle with the boundaries
		ILongRectanglePtr ipRect( CLSID_LongRectangle );
		ASSERT_RESOURCE_ALLOCATION("ELI39513", ipRect != __nullptr);

		ipRect->SetBounds(0, 0, nWidth, nHeight);

		// create a spatial page info with an orientation of kRotNone and a skew of 0.0
		UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr ipPageInfo(CLSID_SpatialPageInfo);
		ASSERT_RESOURCE_ALLOCATION("ELI39515", ipPageInfo != __nullptr);
		ipPageInfo->Initialize(nWidth, nHeight, UCLID_RASTERANDOCRMGMTLib::kRotNone, 0.0);
		
		// Create the page infos map for the page
		ILongToObjectMapPtr ipPageInfos = __nullptr;
		ipPageInfos.CreateInstance(CLSID_LongToObjectMap);
		ASSERT_RESOURCE_ALLOCATION("ELI39516", ipPageInfos != __nullptr);
		ipPageInfos->Set(nPage, ipPageInfo);

		UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipZone(CLSID_RasterZone);
		ASSERT_RESOURCE_ALLOCATION("ELI39514", ipZone != __nullptr);
		ipZone->CreateFromLongRectangle(ipRect, nPage);
		ipPage->CreatePseudoSpatialString(ipZone, textForPage.c_str(),
			m_strSourceDocName.c_str(), ipPageInfos);
	}

	// Set the OCR parameters
	if (m_ipOCRParameters != __nullptr)
	{
		UCLID_RASTERANDOCRMGMTLib::IHasOCRParametersPtr ipHasOCRParameters(ipPage);
		ASSERT_RESOURCE_ALLOCATION("ELI46182", ipHasOCRParameters != __nullptr);
		ipHasOCRParameters->OCRParameters = getOCRParameters();
	}

	return ipPage;
}
//-------------------------------------------------------------------------------------------------
typedef map<long, UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr> PageMap;
unique_ptr<PageMap> CSpatialString::loadPagesFromArchive(const string& strFileName, bool bReadInfoOnly,
	string* strOriginalSourceDocName /*= __nullptr*/, 
	long nPage /*= -1*/,
	bool bLoadIntoThis /*= false*/)
{
	bool returnAllPages = nPage < 0;
	ASSERT_ARGUMENT("ELI46768", !(returnAllPages && bLoadIntoThis));
	ASSERT_ARGUMENT("ELI46769", !(bReadInfoOnly && bLoadIntoThis));

	unique_ptr<PageMap> pageMap;
	if (bLoadIntoThis)
	{
		// Clear this instance in case the requested page doesn't exist
		reset(true, true);
	}
	else
	{
		pageMap = std::make_unique<PageMap>();
	}

	CUnzipper uz(strFileName.c_str());
	unique_ptr<TemporaryFileName> tmpFile;
	string tmpPath;
	if (!bReadInfoOnly)
	{
		tmpFile = std::make_unique<TemporaryFileName>(true);
		tmpPath = tmpFile->getName();
	}
	if (uz.GotoFirstFile(NULL))
	{
		do
		{
			UZ_FileInfo info;
			uz.GetFileInfo(info);
			if (info.bFolder)
			{
				continue;
			}

			string fileName = info.szFileName;

			// Get page number and file type from the name
			size_t p = fileName.find('.');
			if (p <= 0)
			{
				continue;
			}
			string baseName = fileName.substr(0, p);
			long pageNumber;
			if (!isAllNumericChars(baseName, pageNumber))
			{
				continue;
			}
			p = fileName.rfind('.');
			if (p > fileName.length() - 2)
			{
				continue;
			}
			string ext = fileName.substr(p + 1);
			makeLowerCase(ext);
			if (ext != "uss" && ext != "json")
			{
				continue;
			}

			// Load info from, e.g., 0000.json
			// This file needs to come before the page(s) that are to be loaded (in the Central Directory Header) or it may never be read.
			// 7zip and windows built-in explorer zip utility sort files alphabetically by filename so it is easy to add this file
			// to set or change the source doc name
			if (pageNumber == 0
				&& ext == "json"
				&& strOriginalSourceDocName != __nullptr)
			{
				if (tmpPath.empty())
				{
					tmpFile = std::make_unique<TemporaryFileName>(true);
					tmpPath = tmpFile->getName();
				}
                Util::retry(50, "unzip info file",
					[&]() -> bool {
						return uz.UnzipFileTo(tmpPath.c_str());
					},
					[&](int tries) -> void {
						UCLIDException ue("ELI49789", "Application trace: Failed to unzip info file. Retrying...");
						ue.addDebugInfo("Attempt", tries);
						ue.addDebugInfo("File Name", strFileName);
						ue.log();

						Sleep(max(1000, 100 * tries));
					},
					"ELI49790"
				);

				ifstream ifs(tmpPath);
				rapidjson::IStreamWrapper isw(ifs);
				rapidjson::Document document;
				document.ParseStream(isw);
				if (!document.HasParseError())
				{
					const auto& sdnIt = document.FindMember("SourceDocName");
					if (sdnIt != document.MemberEnd())
					{
						*strOriginalSourceDocName = sdnIt->value.GetString();
					}
				}
			}

			if (pageNumber > 0
				&& (bLoadIntoThis || pageMap->find(pageNumber) == pageMap->end())
				&& (returnAllPages || pageNumber == nPage))
			{
				if (bReadInfoOnly)
				{
					pageMap->insert(make_pair(pageNumber, __nullptr));
				}
				else
				{
					Util::retry(50, "unzip page",
                        [&]() -> bool {
							return uz.UnzipFileTo(tmpPath.c_str());
                        },
                        [&](int tries) -> void {
                            UCLIDException ue("ELI49794", "Application trace: Failed to unzip page. Retrying...");
                            ue.addDebugInfo("Attempt", tries);
                            ue.addDebugInfo("File Name", strFileName);
                            ue.addDebugInfo("Page Number", pageNumber);
							ue.log();

							Sleep(max(1000, 100 * tries));
                        },
						"ELI49795" 
					);

					if (bLoadIntoThis)
					{
						if (ext == "uss")
						{
							loadFromStorageObject(tmpPath, getThisAsCOMPtr());
						}
						else
						{
							loadFromGoogleJson(tmpPath, pageNumber, getThisAsCOMPtr());
						}
						return __nullptr;
					}
					else
					{
						UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr page(CLSID_SpatialString);
						ASSERT_RESOURCE_ALLOCATION("ELI46786", page != __nullptr);

						if (ext == "uss")
						{
							loadFromStorageObject(tmpPath, page);
						}
						else
						{
							loadFromGoogleJson(tmpPath, pageNumber, page);
						}
						pageMap->insert(make_pair(pageNumber, page));
					}
				}
				if (!returnAllPages)
				{
					break;
				}
			}
		} while (uz.GotoNextFile(NULL));
	}

	return pageMap;
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::loadFromStorageObject(const string& strInputFile, UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipLoadInto)
{
	try
	{
		// Load this object from the file
		IPersistStreamPtr ipPersistStream = ipLoadInto;
		ASSERT_RESOURCE_ALLOCATION("ELI46772", ipPersistStream != __nullptr);

		if (CompressionEngine::isGZipFile(strInputFile))
		{
			// The .uss file may be compressed.
			// create a temporary file with the uncompressed output
			TemporaryFileName tmpFile(true);
			CompressionEngine::decompressFile(strInputFile, tmpFile.getName());

			try
			{
				readObjectFromFile(ipPersistStream, get_bstr_t(tmpFile.getName().c_str()),
					gbstrSPATIAL_STRING_STREAM_NAME, false, gstrSPATIAL_STRING_FILE_SIGNATURE);
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI49791")
		}
		else
		{
			try
			{
				readObjectFromFile(ipPersistStream, get_bstr_t(strInputFile.c_str()),
					gbstrSPATIAL_STRING_STREAM_NAME, false, gstrSPATIAL_STRING_FILE_SIGNATURE);
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI49792")
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI49793")
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::saveToStorageObject(const string& strFullFileName, UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipSpatialString, bool bCompress, bool bClearDirty)
{
	unique_ptr<TemporaryFileName> tmpFile;
	string strOutputFileName = strFullFileName;
	bool useTempFile = bCompress;
	if (useTempFile)
	{
		tmpFile = std::make_unique<TemporaryFileName>(true);
		strOutputFileName = tmpFile->getName();
	}
	writeObjectToFile(ipSpatialString, _bstr_t(strOutputFileName.c_str()), 
		gbstrSPATIAL_STRING_STREAM_NAME, bClearDirty, 
		gstrSPATIAL_STRING_FILE_SIGNATURE, useTempFile);

	// Wait until the file is readable
	waitForFileToBeReadable(strOutputFileName);

	// if requested, compress the above created temporary file and 
	// save as the specified filename
	if (bCompress)
	{
		// Compress the file (compress file includes a call to waitForFileToBeReadable
		// no need to include one here)
		CompressionEngine::compressFile(strOutputFileName, strFullFileName);
	}
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::loadFromArchive(const string& strFileName)
{
	string originalSourceDocName;

	// Get the existing page numbers
	auto pageMap = loadPagesFromArchive(strFileName, true, &originalSourceDocName);

	long count = pageMap->size();
	if (count == 0)
	{
		getThisAsCOMPtr()->CreateNonSpatialString("", "");
	}
	else if (count == 1)
	{
		// If there's only one page, then load it directly into this instance 
		loadPagesFromArchive(strFileName, false, __nullptr, pageMap->begin()->first, true);
	}
	else
	{
		pageMap = loadPagesFromArchive(strFileName, false);

		// Put the pages in order
		IIUnknownVectorPtr pages(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI46773", pages != __nullptr);
		for (auto& p : *pageMap)
		{
			// Skip empty (or non-spatial) pages
			// https://extract.atlassian.net/browse/ISSUE-16436
			if (p.second->HasSpatialInfo())
			{
				pages->PushBack(p.second);
			}
		}

		// Combine the pages
		getThisAsCOMPtr()->CreateFromSpatialStrings(pages, VARIANT_FALSE);
	}

	if (!originalSourceDocName.empty())
	{
		m_strSourceDocName = originalSourceDocName;
	}
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::savePagesToArchive(const string& strOutputFile, IIUnknownVectorPtr ipPages, bool bCompress, bool bAppend)
{
	long previousPage = 0;
	long count = ipPages->Size();

	// Validate the input
	for (long i = 0; i < count; i++)
	{
		UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr page = ipPages->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI46783", page != __nullptr)

		// loadFromArchive requires spatial mode strings, since it uses CreateFromSpatialStrings,
		// so assert that the pages to save are spatial so that the archive will be loadable.
		ASSERT_RUNTIME_CONDITION("ELI46771", page->GetMode() == kSpatialMode,
			"Mode must be kSpatialMode to save");

		long firstPage = page->GetFirstPageNumber();
		long lastPage = page->GetLastPageNumber();
		ASSERT_RUNTIME_CONDITION("ELI46774", firstPage == lastPage && firstPage > previousPage,
			"Expecting input collection to contain single-page strings in strictly ascending page order");
	}

	// Write to the archive
	if (isValidFile(strOutputFile))
	{
		waitForFileAccess(strOutputFile, giMODE_WRITE_ONLY);
	}
	CZipper z(strOutputFile.c_str(), NULL, bAppend, bCompress);

	TemporaryFileName tmpPageFile(true);
	string tmpPagePath = tmpPageFile.getName();
	_bstr_t bstrtTmpPagePath = get_bstr_t(tmpPagePath);
	for (long i = 0; i < count; i++)
	{
		UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr page = ipPages->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI46784", page != __nullptr)

		long pageNumber = page->GetFirstPageNumber();

		// Create a new page info collection with only the necessary page in it
		ILongToObjectMapPtr pageInfos = page->GetSpatialPageInfos();
		ASSERT_RESOURCE_ALLOCATION("ELI46782", pageInfos != __nullptr)

		if (pageInfos->Size > 1)
		{
			UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr pageInfo = pageInfos->GetValue(pageNumber);
			ASSERT_RESOURCE_ALLOCATION("ELI46781", pageInfo != __nullptr)

			pageInfos.CreateInstance(CLSID_LongToObjectMap);
			ASSERT_RESOURCE_ALLOCATION("ELI50085", pageInfos != __nullptr)

			pageInfos->Set(pageNumber, pageInfo);
			page->PutSpatialPageInfos(pageInfos);
		}

		writeObjectToFile(page, bstrtTmpPagePath, gbstrSPATIAL_STRING_STREAM_NAME, false, gstrSPATIAL_STRING_FILE_SIGNATURE, true);
		waitForFileToBeReadable(tmpPagePath);

		// Build internal zip file path
		char pszTemp[16];
		sprintf_s(pszTemp, sizeof(pszTemp), "%04u", pageNumber);
		string baseName(pszTemp);
		string internalPath = baseName + "." + asString(page->OCREngineVersion) + ".uss";

		Util::retry(50, "save page to uss file",
			[&]() -> bool {
                return z.AddFileToPathInZip(bstrtTmpPagePath, internalPath.c_str());
			},
			[&](int tries) -> void {
				UCLIDException ue("ELI51628", "Application trace: Unable to save page to uss file. Retrying...");
				ue.addDebugInfo("Attempt", tries);
				ue.addDebugInfo("Output File", strOutputFile);
				ue.addDebugInfo("Page Number", pageNumber);
				ue.log();
				Sleep(max(1000, 100 * tries));

				// Always reopen with bAppend = true when retrying or else the recently added pages will be lost
				// https://extract.atlassian.net/browse/ISSUE-17435
				z.OpenZip(strOutputFile.c_str(), NULL, true);
			},
			"ELI49657"
		);
	}
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::appendToArchive(const string& strOutputFile, bool bCompress, bool bCheckForExistingPages)
{
	IIUnknownVectorPtr pages = getThisAsCOMPtr()->GetPages(VARIANT_FALSE, "");
	ASSERT_RESOURCE_ALLOCATION("ELI46764", pages != __nullptr);

	// Retrieve the page numbers that already exist in the zip
	auto preexistingPageMap = loadPagesFromArchive(strOutputFile, true);
	long preexistingCount = preexistingPageMap->size();
    long pagesToWriteCount = pages->Size();

	if (bCheckForExistingPages)
	{
		if (preexistingCount == 0)
		{
			savePagesToArchive(strOutputFile, pages, bCompress);
		}
		else
		{
			// Ensure that there are no pages in common in the substring and the existing archive
			for (long i = 0, count = pages->Size(); i < count; i++)
			{
				UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr page = pages->At(i);
				long firstPage = page->GetFirstPageNumber();
				ASSERT_RUNTIME_CONDITION("ELI46775", preexistingPageMap->find(firstPage) == preexistingPageMap->end(), "Page already exists in the archive");
			}

			savePagesToArchive(strOutputFile, pages, bCompress, true);
		}
	}
	else
	{
		// Append or create without regard to contents
		savePagesToArchive(strOutputFile, pages, bCompress, true);
	}

    // Verify that all the pages were saved successfully
    // https://extract.atlassian.net/browse/ISSUE-17435
    auto pageMap = loadPagesFromArchive(strOutputFile, true);
    if (pageMap->size() != preexistingCount + pagesToWriteCount)
    {
		UCLIDException uex("ELI51560", "Incorrect number of pages written!");
		uex.addDebugInfo("Expected page count", preexistingCount + pagesToWriteCount);
		uex.addDebugInfo("Actual page count", pageMap->size());
		throw uex;
    }
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::loadFromGoogleJson(const string& strInputFile, long nPageNumber, UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipLoadInto)
{
	FILE* fp;
	errno_t error = fopen_s(&fp, strInputFile.c_str(), "rb");
	if (error)
	{
		UCLIDException uex("ELI46794", "Error opening temp file");
		uex.addDebugInfo("Error Number", error);
		throw uex;
	}
	char readBuffer[65536];
	rapidjson::FileReadStream is(fp, readBuffer, sizeof(readBuffer));
	rapidjson::Document document;
	document.ParseStream(is);
	fclose(fp);

	if (document.HasParseError())
	{
		UCLIDException uex("ELI46795", "Error parsing JSON");
		uex.addDebugInfo("Page Number", nPageNumber);
		throw uex;
	}

	// Check for fullTextAnnotation object in the full json that is returned from GCV
	const rapidjson::Value* textAnnotation = rapidjson::GetValueByPointer(document, "/responses/0/fullTextAnnotation");
	if (textAnnotation == __nullptr)
	{
		// Else, assume the whole document is the fullTextAnnotation object (not sure which makes more sense to store)
		textAnnotation = rapidjson::GetValueByPointer(document, "");
	}

	const rapidjson::Value* page = rapidjson::GetValueByPointer(*textAnnotation, "/pages/0");
	if (page == __nullptr)
	{
		ipLoadInto->Clear();
		return;
	}

	// Width and height may be in pixel or points.
	// They are in points when there are normalizedVertices instead of vertices
	// Points will be translated to pixels later in this method
	auto& widthIt = page->FindMember("width");
	auto& heightIt = page->FindMember("height");
	ASSERT_RUNTIME_CONDITION("ELI46790", widthIt != page->MemberEnd() && heightIt != page->MemberEnd(), "Width and height information is missing");
	long width = widthIt->value.GetInt();
	long height = heightIt->value.GetInt();
	bool hasNormalizedVertices = false;
	bool hasVertices = false;
	double widthConvertedFromPoints = (double)width * 300 / 72;
	double heightConvertedFromPoints = (double)height * 300 / 72;

	vector<CPPLetter> letters;
	vector<double> thetas;
	const auto& textIt = textAnnotation->FindMember("text");
	if (textIt != textAnnotation->MemberEnd())
	{
		rapidjson::SizeType length = textIt->value.GetStringLength();
		letters.reserve(length);
		thetas.reserve(length);
	}

	const auto& blocksIt = page->FindMember("blocks");
	if (blocksIt != page->MemberEnd())
	{
		long blockIdx = 0;
		for (auto& block : blocksIt->value.GetArray())
		{
			const auto& paragraphsIt = block.FindMember("paragraphs");
			if (paragraphsIt != block.MemberEnd())
			{
				long paragraphIdx = 0;
				for (auto& paragraph : paragraphsIt->value.GetArray())
				{
					const auto wordsIt = paragraph.FindMember("words");
					if (wordsIt != paragraph.MemberEnd())
					{
						try
						{
							// Add symbols from these words to letters and also collect rotation angles and set vertice/normalizedVertice flags, if not already set
							const auto& words = wordsIt->value.GetArray();
							addWordsToLetterArray(words, (unsigned short)nPageNumber, widthConvertedFromPoints, heightConvertedFromPoints, letters, hasVertices, hasNormalizedVertices, thetas);
						}
						catch (UCLIDException& uex)
						{
							uex.addDebugInfo("Paragraph Index", paragraphIdx);
							uex.addDebugInfo("Block Index", blockIdx);
							throw;
						}
					}

					// Add empty line after each paragraph (this won't be quite consistent with SSOCR2,
					// since the last paragraph of the document will end with an empty line, but I guess it's close enough)
					letters.insert(letters.end(), { gletterSLASH_R, gletterSLASH_N });
					paragraphIdx++;
				}
			}
			blockIdx++;
		}
	}

	if (hasNormalizedVertices)
	{
		height = (long)round(heightConvertedFromPoints);
		width = (long)round(widthConvertedFromPoints);
	}

	// Create spatial string in image coordinates
	UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr imagePageInfo(CLSID_SpatialPageInfo);
	imagePageInfo->Initialize(width, height, UCLID_RASTERANDOCRMGMTLib::kRotNone, 0);
	ILongToObjectMapPtr imagePageInfos(CLSID_LongToObjectMap);
	imagePageInfos->Set(nPageNumber, imagePageInfo);
	ipLoadInto->CreateFromLetterArray(letters.size(), &letters[0], "Unknown", imagePageInfos);


	// Calculate the predominate text angle, create OCR-coordinate page info and translate the string
	UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr newPageInfo(CLSID_SpatialPageInfo);
	ILongToObjectMapPtr newPageInfos(CLSID_LongToObjectMap);
	newPageInfos->Set(nPageNumber, newPageInfo);

	sort(thetas.begin(), thetas.end());
	double medTheta = thetas[thetas.size() / 2]; // range (-PI, PI)
	long rotation = (long)round(medTheta * 2 / MathVars::PI) * 90; // (-180, -90, 0, 90, 180)
	double deskew = medTheta * 180 / MathVars::PI; // range (-180, 180)
	deskew -= rotation;

	UCLID_RASTERANDOCRMGMTLib::EOrientation orientation = UCLID_RASTERANDOCRMGMTLib::kRotNone;
	switch ((rotation + 360) % 360)
	{
		case 0:
		{
			break;
		}
		case 180:
		{
			orientation = UCLID_RASTERANDOCRMGMTLib::kRotDown;
			break;
		}
		case 90:
		{
			orientation = UCLID_RASTERANDOCRMGMTLib::kRotLeft;
			break;
		}
		case 270:
		{
			orientation = UCLID_RASTERANDOCRMGMTLib::kRotRight;
			break;
		}
		default:
		{
			THROW_LOGIC_ERROR_EXCEPTION("ELI46788");
		}
	}
	newPageInfo->Initialize(width, height, orientation, deskew);
	ipLoadInto->TranslateToNewPageInfo(newPageInfos);
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::addWordsToLetterArray(const rapidjson::Value::ConstArray& words,
		unsigned short pageNumber, double widthConvertedFromPoints, double heightConvertedFromPoints,
		vector<CPPLetter>& letters, bool& hasVertices, bool& hasNormalizedVertices, vector<double>& thetas)
{
	CPPLetter letter(-1, -1, -1, -1, -1, -1, -1, pageNumber, false, false, true, 0, 100, 0);
	for (long wordIdx = 0, numWords = words.Size(); wordIdx < numWords; ++wordIdx)
	{
		try
		{
			try
			{
				const auto& word = words[wordIdx];
				AnnotationSpatialProperties wordProperties;
				bool wordPropertiesAreSet = getAnnotationSpatialInfo(word, hasVertices, hasNormalizedVertices, wordProperties);
				if (wordPropertiesAreSet)
				{
					thetas.push_back(wordProperties.theta);
				}

				// SYMBOLS
				const auto& symbolsIt = word.FindMember("symbols");
				if (symbolsIt != word.MemberEnd())
				{
					const auto& symbols = symbolsIt->value.GetArray();
					for (long symbolIdx = 0, numSymbols = symbols.Size(); symbolIdx < numSymbols; ++symbolIdx)
					{
						const auto& symbol = symbols[symbolIdx];
						const auto& textIt = symbol.FindMember("text");
						if (textIt != symbol.MemberEnd())
						{
							string text = textIt->value.GetString();
							unsigned short c = text.length() == 0 ? (unsigned short)'^' : getWindows1252FromUTF8(text);
							letter.m_usGuess1 = letter.m_usGuess2 = letter.m_usGuess3 = c;

							const auto& confidenceIt = symbol.FindMember("confidence");
							if (confidenceIt != symbol.MemberEnd())
							{
								letter.m_ucCharConfidence = (unsigned char)(confidenceIt->value.GetDouble() * 100);
							}

							// Set spatial info for character directly or by dividing up the word bounds
							AnnotationSpatialProperties symbolProperties;
							bool symbolPropertiesAreSet = getAnnotationSpatialInfo(symbol, hasVertices, hasNormalizedVertices, symbolProperties);
							if (!symbolPropertiesAreSet)
							{
								if (wordPropertiesAreSet)
								{
									getSymbolSpatialInfoFromWord(wordProperties, numSymbols, symbolIdx, symbolProperties);
									symbolPropertiesAreSet = true;
								}
							}
							if (symbolPropertiesAreSet)
							{
								setLetterBounds(letter, symbolProperties, hasNormalizedVertices ? widthConvertedFromPoints : 1, hasNormalizedVertices ? heightConvertedFromPoints : 1);
							}

							// Get break properties
							string breakType = "UNKNOWN";
							const auto& propertyIt = symbol.FindMember("property");
							if (propertyIt != symbol.MemberEnd())
							{
								const auto& symbolProperties = propertyIt->value;
								const auto& detectedBreakIt = symbolProperties.FindMember("detectedBreak");
								if (detectedBreakIt != symbolProperties.MemberEnd())
								{
									bool prefixBreak = false;
									const auto& detectedBreak = detectedBreakIt->value;
									const auto& breakTypeIt = detectedBreak.FindMember("type");
									if (breakTypeIt != detectedBreak.MemberEnd())
									{
										breakType = breakTypeIt->value.GetString();
									}
									const auto& isPrefixIt = detectedBreak.FindMember("isPrefix");
									if (isPrefixIt != detectedBreak.MemberEnd())
									{
										prefixBreak = isPrefixIt->value.GetBool();
									}

									// The spec has this isPrefix property but I haven't seen it in any output so rather than try to handle this just ignore prefix breaks
									if (prefixBreak)
									{
										breakType = "UNKNOWN";
									}
								}
							}

							if ((symbolIdx == numSymbols - 1 && wordIdx == numWords - 1))
							{
								letter.m_bIsEndOfParagraph = letter.m_bIsEndOfZone = true;
							}
							else if (breakType == "EOL_SURE_SPACE")
							{
								letter.m_bIsEndOfZone = true;
							}

							letters.push_back(letter);

							if (breakType == "SPACE")
							{
								letters.push_back(gletterSPACE);
							}
							else if (breakType == "SURE_SPACE")
							{
								letters.push_back(gletterTAB);
							}
							else if (breakType == "EOL_SURE_SPACE" || breakType == "LINE_BREAK")
							{
								letters.insert(letters.end(), { gletterSLASH_R, gletterSLASH_N });
							}
						}
					}
				}
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI46793")
		}
		catch (UCLIDException& uex)
		{
			uex.addDebugInfo("Word Index", wordIdx);
			throw uex;
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::getSymbolSpatialInfoFromWord(const AnnotationSpatialProperties& wordProperties, long numSymbolsInWord, long symbolIdx,
	AnnotationSpatialProperties& symbolProperties)
{
	double symbolWidth = wordProperties.width / numSymbolsInWord;
	symbolProperties.height = wordProperties.height;
	symbolProperties.width = symbolWidth;
	symbolProperties.theta = wordProperties.theta;
	symbolProperties.orientation = wordProperties.orientation;

	double startOfSymbol = symbolIdx * symbolWidth;
	double endOfSymbol = startOfSymbol + symbolWidth;
	double cosTheta = cos(wordProperties.theta);
	double sinTheta = sin(wordProperties.theta);
	double startX = startOfSymbol * cosTheta;
	double endX = endOfSymbol * cosTheta;
	double startY = startOfSymbol * sinTheta;
	double endY = endOfSymbol * sinTheta;
	symbolProperties.x1 = wordProperties.x1 + startX;
	symbolProperties.y1 = wordProperties.y1 + startY;
	symbolProperties.x2 = wordProperties.x1 + endX;
	symbolProperties.y2 = wordProperties.y1 + endY;
	symbolProperties.x3 = wordProperties.x4 + endX;
	symbolProperties.y3 = wordProperties.y4 + endY;
	symbolProperties.x4 = wordProperties.x4 + startX;
	symbolProperties.y4 = wordProperties.y4 + startY;
}
//----------------------------------------------------------------------------------------------
void CSpatialString::setLetterBounds(CPPLetter& letter, const AnnotationSpatialProperties& properties,
	const double& denormalizationFactorX, const double& denormalizationFactorY)
{
	double top, right, bottom, left;
	// TranslateToNewPageInfo() treats top as the y of top-left corner, bottom as the y of the bottom-right corner, etc.
	// so set these properties accordingly or else the spatial info of skewed letters can get squashed
	// https://extract.atlassian.net/browse/ISSUE-16415
	switch (properties.orientation)
	{
		case 0:
		{
			top = properties.y1;
			right = properties.x3;
			bottom = properties.y3;
			left = properties.x1;
			break;
		}
		case 180:
		{
			top = properties.y3;
			right = properties.x1;
			bottom = properties.y1;
			left = properties.x3;
			break;
		}
		case 90:
		{
			top = properties.y4;
			right = properties.x2;
			bottom = properties.y2;
			left = properties.x4;
			break;
		}
		case 270:
		{
			top = properties.y2;
			right = properties.x4;
			bottom = properties.y4;
			left = properties.x2;
			break;
		}
		default:
		{
			THROW_LOGIC_ERROR_EXCEPTION("ELI46787");
		}
	}
	letter.m_ulTop = (unsigned long)(top * denormalizationFactorY);
	letter.m_ulRight = (unsigned long)(right * denormalizationFactorX);
	letter.m_ulBottom = (unsigned long)(bottom * denormalizationFactorY);
	letter.m_ulLeft = (unsigned long)(left * denormalizationFactorX);
	letter.m_bIsSpatial = true;
}
//----------------------------------------------------------------------------------------------
bool CSpatialString::getAnnotationSpatialInfo(const rapidjson::Value& el, bool& hasVertices,
	bool& hasNormalizedVertices, CSpatialString::AnnotationSpatialProperties& properties)
{
	rapidjson::Value::ConstMemberIterator vertIt;
	const auto& bbIt = el.FindMember("boundingBox");
	if (bbIt != el.MemberEnd())
	{
		if (hasVertices)
		{
			vertIt = bbIt->value.FindMember("vertices");
		}
		else if (hasNormalizedVertices)
		{
			vertIt = bbIt->value.FindMember("normalizedVertices");
		}
		else
		{
			vertIt = bbIt->value.FindMember("vertices");
			if (vertIt != bbIt->value.MemberEnd())
			{
				hasVertices = true;
			}
			else
			{
				vertIt = bbIt->value.FindMember("normalizedVertices");
				if (vertIt != bbIt->value.MemberEnd())
				{
					hasNormalizedVertices = true;
				}
			}
		}

		if (vertIt != bbIt->value.MemberEnd())
		{
			rapidjson::Value::ConstArray& v = vertIt->value.GetArray();
			if (vertIt != bbIt->value.MemberEnd() && v.Size() > 3)
			{
				double *points[4][2] = {
					{ &properties.x1, &properties.y1 },
					{ &properties.x2, &properties.y2 },
					{ &properties.x3, &properties.y3 },
					{ &properties.x4, &properties.y4 } };

				for (long i = 0; i < 4; i++)
				{
					const auto& v = vertIt->value[i];
					const auto& x = v.FindMember("x");
					// If coordinate is omitted it's value is 0
					// https://cloud.google.com/vision/docs/reference/rpc/google.cloud.vision.v1p2beta1#zero-coordinate-values_2
					if (x == v.MemberEnd())
					{
						*points[i][0] = 0;
					}
					else
					{
						*points[i][0] = x->value.GetDouble();
					}
					const auto& y = vertIt->value[i].FindMember("y");
					if (y == v.MemberEnd())
					{
						*points[i][1] = 0;
					}
					else
					{
						*points[i][1] = y->value.GetDouble();
					}
				}
				double dx2 = (properties.x2 - properties.x1);
				double dy2 = (properties.y2 - properties.y1);
				double dx4 = (properties.x4 - properties.x1);
				double dy4 = (properties.y4 - properties.y1);
				properties.theta = atan2(dy2, dx2); // range (-PI, PI)
				properties.orientation = (long)round(properties.theta * 2 / MathVars::PI + 4) % 4 * 90; // (0, 90, 180, 270)
				properties.width = sqrt(dx2 * dx2 + dy2 * dy2);
				properties.height = sqrt(dx4 * dx4 + dy4 * dy4);

				return true;
			}
		}
	}
	return false;
}
