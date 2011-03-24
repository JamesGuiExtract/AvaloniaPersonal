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

#include <math.h>
#include <set>

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
const CRect grectNULL = CRect(0, 0, 0, 0);
const double gfCharWidthToAvgCharRatio = 1.1;

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr CSpatialString::getThisAsCOMPtr()
{
    UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipThis(this);
    ASSERT_RESOURCE_ALLOCATION("ELI16977", ipThis != NULL);

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
        ASSERT_ARGUMENT("ELI25797", ipStringToInsert != NULL);

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
                    ASSERT_RESOURCE_ALLOCATION("ELI15308", ipZone != NULL);

                    // Add the raster zone if not already present
                    if (!isRasterZoneInVector(ipZone, vecCombinedZones))
                    {
                        vecCombinedZones.push_back(ipZone);
                    }
                }
            }

            if (eSourceMode != kNonSpatialMode)
            {
                IIUnknownVectorPtr ipZones = ipStringToInsert->GetOCRImageRasterZones();
                ASSERT_RESOURCE_ALLOCATION("ELI15309", ipZones != NULL);

                long lSize = ipZones->Size();
                for (long i = 0; i < lSize; i++)
                {
                    // Get each zone present
                    UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipZone = ipZones->At(i);
                    ASSERT_RESOURCE_ALLOCATION("ELI15310", ipZone != NULL);

                    // Add the raster zone if not already present
                    if (!isRasterZoneInVector(ipZone, vecCombinedZones))
                    {
                        vecCombinedZones.push_back(ipZone);
                    }
                }
            }

            // call updateHybrid to reset this object as a hybrid string
            updateHybrid(vecCombinedZones, strTemp);

            // If the source string had spatial info, update the page info map
            if (eSourceMode != kNonSpatialMode)
            {
                updateAndValidateCompatibleSpatialPageInfo(ipStringToInsert->SpatialPageInfos);
            }
        }
        else if (m_eMode == kSpatialMode || eSourceMode == kSpatialMode)
        {
            // At least one string is spatial, the other is either spatial or non-spatial;
            // so the result will be spatial.

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
                                UCLIDException ue("ELI25798",
                                    "Cannot insert string: Page inconsistency");
                                ue.addDebugInfo("First page of insert", lFirstPage);
                                ue.addDebugInfo("Page To Insert After", letter.m_usPageNumber);
                                throw ue;
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

            // If the old string was spatial, page info must be properly updated 
            if (eSourceMode != kNonSpatialMode)
            {
                updateAndValidateCompatibleSpatialPageInfo(ipStringToInsert->SpatialPageInfos);
            }
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
        ASSERT_ARGUMENT("ELI25803", ipRasterZones != NULL);

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

        for (long i=0; i < lSize; i++)
        {
            UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipZone = ipRasterZones->At(i);
            ASSERT_RESOURCE_ALLOCATION("ELI25805", ipZone != NULL);

            m_vecRasterZones.push_back(ipZone);
        }

        updateAndValidateCompatibleSpatialPageInfo(ipPageInfoMap);

        // Set mode to hybrid
        m_eMode = kHybridMode;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25806");
}
//-------------------------------------------------------------------------------------------------
void CSpatialString::updateAndValidateCompatibleSpatialPageInfo(ILongToObjectMapPtr ipPageInfoMap)
{
    try
    {
        if (ipPageInfoMap == NULL)
        {
            // Nothing to add, just return
            return;
        }

        // If the current page info map is NULL just replace it
        if (m_ipPageInfoMap == NULL)
        {
            m_ipPageInfoMap = ipPageInfoMap;
        }
        // Else - merge the info maps (validating compatible pages)
        else
        {
            // the target already has a spatial page info map, compare source's
            // keys one at a time and add any that the target is missing. [P13 #4728]
            IVariantVectorPtr ipKeys( ipPageInfoMap->GetKeys() );
            ASSERT_RESOURCE_ALLOCATION("ELI25807", ipKeys != NULL);

            // iterate through each of source's keys
            long lSize = ipKeys->Size;
            for(long i=0; i<lSize; i++)
            {
                // get the ith key
                long lKey = ipKeys->GetItem(i).lVal;

                UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr ipSourceInfo =
                    ipPageInfoMap->GetValue(lKey);
                ASSERT_RESOURCE_ALLOCATION("ELI25808", ipSourceInfo != NULL);

                // if the target doesn't have this key, add it
                if(m_ipPageInfoMap->Contains(lKey) == VARIANT_FALSE)
                {
                    m_ipPageInfoMap->Set(lKey, ipSourceInfo);
                }
                // else the target has the key so need to validate that the infos are compatible
                else
                {
                    UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr ipCurrentInfo =
                        m_ipPageInfoMap->GetValue(lKey);
                    ASSERT_RESOURCE_ALLOCATION("ELI25809", ipCurrentInfo != NULL);

                    // Check that the spatial page infos are compatible
                    if (ipCurrentInfo->Equal(ipSourceInfo) == VARIANT_FALSE)
                    {
                        long lSourceWidth, lCurrentWidth, lSourceHeight, lCurrentHeight;
                        UCLID_RASTERANDOCRMGMTLib::EOrientation eSourceOrient, eCurrentOrient;
                        double dSourceDeskew, dCurrentDeskew;

                        UCLIDException ue("ELI25810",
                            "Cannot merge with incompatible spatial page info!");
                        ue.addDebugInfo("Page Number", lKey);

                        // Try to get the page info values and add it as debug info
                        try
                        {
                            ipSourceInfo->GetPageInfo(&lSourceWidth, &lSourceHeight,
                                &eSourceOrient, &dSourceDeskew);
                            ipCurrentInfo->GetPageInfo(&lCurrentWidth, &lCurrentHeight,
                                &eCurrentOrient, &dCurrentDeskew);

                            ue.addDebugInfo("Source Width", lSourceWidth);
                            ue.addDebugInfo("Source Height", lSourceHeight);
                            ue.addDebugInfo("Source Orientation", eSourceOrient);
                            ue.addDebugInfo("Source Deskew", dSourceDeskew);
                            ue.addDebugInfo("Current Width", lCurrentWidth);
                            ue.addDebugInfo("Current Height", lCurrentHeight);
                            ue.addDebugInfo("Current Orientation", eCurrentOrient);
                            ue.addDebugInfo("Current Deskew", dCurrentDeskew);
                        }
                        catch(...)
                        {
                        }

                        throw ue;
                    }
                }
            }
        }
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
                // This operation requires that this object is a spatial string
                UCLIDException ue( "ELI14762", "Non spatial strings cannot be multi-page spatial strings!");
                throw ue;
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
                    ASSERT_RESOURCE_ALLOCATION("ELI14763", ipRZone != NULL);
                    
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
        ASSERT_RESOURCE_ALLOCATION("ELI06458", ipSpatialString != NULL);

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
            if (ipMatches == NULL || ipMatches->Size() <= 0)
            {
                // no matches found
                return;
            }

            // a final string after replacement
            UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipFinal(CLSID_SpatialString);
            ASSERT_RESOURCE_ALLOCATION("ELI06792", ipFinal != NULL);

            // Get the number of matches
            long nSize = ipMatches->Size();

            // Make some tracking variables
            long nNonMatchStart = 0, nNonMatchEnd = 0;
            for (long n = 0; n < nSize; n++)
            {
                // Get the info of the match
                ITokenPtr ipMatch = ipMatches->At(n);
                ASSERT_RESOURCE_ALLOCATION("ELI06820", ipMatch != NULL);

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
                        ASSERT_RESOURCE_ALLOCATION("ELI06824", ipTempStr != NULL);

                        // append it to the final string
                        ipFinal->Append(ipTempStr);
                    }
                }

                // get the actual replacement string
                string strActualReplacement = asString(ipMatch->Value);

                // now create a spatial string that has the replacement string
                UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipReplacement(CLSID_SpatialString);
                ASSERT_RESOURCE_ALLOCATION("ELI06793", ipReplacement != NULL);

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
                            letter.m_usGuess1 = letter.m_usGuess2 = letter.m_usGuess3 =
                                (unsigned short) strActualReplacement[i];

                            // whitespace chars may not have spatial information
                            if (isWhitespaceChar(strActualReplacement[i]))
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
                        ASSERT_RESOURCE_ALLOCATION("ELI15097", ipTempStr != NULL);

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
                    ASSERT_RESOURCE_ALLOCATION("ELI06825", ipTempStr != NULL);

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
            ASSERT_RESOURCE_ALLOCATION("ELI25278", ipResult != NULL);
            if (lOccurrenceIndex >= 0)
            {
                // Return the specified match
                ITokenPtr ipMatch = ipMatches->At(lOccurrenceIndex);
                ASSERT_RESOURCE_ALLOCATION("ELI25279", ipMatch != NULL);
                ipResult->PushBack(ipMatch);
            }
        }
    }
    else
    {
        // Default to an empty result
        ipResult.CreateInstance(CLSID_IUnknownVector);
        ASSERT_RESOURCE_ALLOCATION("ELI25282", ipResult != NULL);

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
                ASSERT_RESOURCE_ALLOCATION("ELI25284", ipMatch != NULL);
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
                ASSERT_RESOURCE_ALLOCATION("ELI25283", ipMatch != NULL);
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
    if (m_ipPageInfoMap == NULL)
    {
        m_ipPageInfoMap.CreateInstance(CLSID_LongToObjectMap);
        ASSERT_RESOURCE_ALLOCATION("ELI15648", m_ipPageInfoMap != NULL);
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
void CSpatialString::loadTextWithPositionalData(const string& strFileName)
{
	// Load the file as text.
	string text = getTextFileContentsAsString(strFileName);
	size_t length = text.length();

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
	CPPLetter *plastSpatialLetter = NULL;

	// Loop through each charater of the file.
	for (size_t i = 0; i < length; i++)
	{
		char c = text[i];
		CPPLetter& letter = vecLetters[i];

		// Specify the character.
		letter.m_usGuess1 = c;
		letter.m_usGuess2 = c;
		letter.m_usGuess3 = c;

		// If the character is whitespace, don't specify spatial info, but check if this makes the
		// last non-spatial character the end of a zone or the end of a paragraph.
		if (isWhitespaceChar(c))
		{
			if (plastSpatialLetter != NULL)
			{
				// If there is more than one consecutive non-spatial char, ensure the last spatial
				// char is treated as end-of-zone.
				if (!vecLetters[i - 1].m_bIsSpatial)
				{
					c = '\t';
				}

				switch (c)
				{
					case '\r':
					case '\n': 
						plastSpatialLetter->m_bIsEndOfZone = true;
						plastSpatialLetter->m_bIsEndOfParagraph = true;
						plastSpatialLetter = NULL;
						break;

					case '\t': 
						if (!plastSpatialLetter->m_bIsEndOfZone)
						{
							plastSpatialLetter->m_bIsEndOfZone = true;
						}
				}
			}
		}
		// This is a spatial character, assign an "index" for the character in the text file.
		else
		{
			letter.m_bIsSpatial = true;
			letter.m_usPageNumber = 1;
			letter.m_ulLeft = i;
			letter.m_ulRight = i;
			letter.m_ulTop = 0;
			letter.m_ulBottom = 1;
			plastSpatialLetter = &letter;
		}
	}

	// The page info should be as many pixels wide as there are characters in the file and 1 pixel
	// high.
	UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr ipPageInfo(CLSID_SpatialPageInfo);
	ASSERT_RESOURCE_ALLOCATION("ELI31685", ipPageInfo != NULL);
	ipPageInfo->Width = length;
	ipPageInfo->Height = 1;
	ipPageInfo->Deskew = 0.0;
	ipPageInfo->Orientation = UCLID_RASTERANDOCRMGMTLib::kRotNone;

	// Create a spatial page info map
	ILongToObjectMapPtr ipPageInfoMap(CLSID_LongToObjectMap);
	ASSERT_RESOURCE_ALLOCATION("ELI31686", ipPageInfoMap != NULL);
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
        UCLIDException ue("ELI06806", "Cannot operate on spatial strings from different sources!");
        ue.addDebugInfo("this.SourceDocName", m_strSourceDocName);
        ue.addDebugInfo("other.SourceDocName", strSourceDocName);
        throw ue;
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
        if( m_ipPageInfoMap != NULL && bResetPageInfoMap)
        {
            // Clear the reference, do not remove the SpatialPageInfo objects
            m_ipPageInfoMap = NULL;
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
        ASSERT_ARGUMENT("ELI10464", (letters != NULL));
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
    ASSERT_ARGUMENT("ELI15421", ipNewZone != NULL);

    for (vector<UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr>::const_iterator it = vecZones.begin();
        it != vecZones.end(); it++)
    {
        // Retrieve the Ith zone
        UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipThisZone = (*it);
        ASSERT_RESOURCE_ALLOCATION("ELI15423", ipThisZone != NULL);

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
IIUnknownVectorPtr CSpatialString::getOCRImageRasterZonesGroupedByConfidence(
    IVariantVectorPtr ipVecOCRConfidenceBoundaries, IVariantVectorPtr &ipZoneOCRConfidenceTiers)
{
    try
    {
        ASSERT_ARGUMENT("ELI25396", ipVecOCRConfidenceBoundaries != NULL);

        if (m_eMode != kSpatialMode)
        {
            UCLIDException ue("ELI25385", 
                "Cannot perform this operation on a hybrid or non-spatial string!");
            ue.addDebugInfo(string("String"), m_strString);
            throw ue;
        }

        IIUnknownVectorPtr ipZones(CLSID_IUnknownVector);
        ASSERT_RESOURCE_ALLOCATION("ELI25364", ipZones != NULL);

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

        // This loop iterates the entire string character by character to build the raster zones.
        size_t j = 0;
        while (j < m_vecLetters.size())
        {
            // Declare properties to apply to the next raster zone.
            short sCurrentPage = -1;
            unsigned char ucLowerConfidenceBounds = 0;
            unsigned char ucUpperConfidenceBounds = 100;
            long nConfidenceTier = nConfidenceTierCount;
            CRect rectCurrentZone = grectNULL;
            bool bEndOfLine = false;

            // This loop iterates through all characters belonging to a single raster zone.
            while (j < m_vecLetters.size())
            {
                // Ignore non-spatial characters.
                if (!m_vecLetters[j].m_bIsSpatial)
                {
                    j++;
                    continue;
                }

                // If sCurrentPage is -1, we are starting a new zone.  Initialize the zones properties.
                if (sCurrentPage == -1)
                {
                    // Determine the page the zone will be on.
                    sCurrentPage = m_vecLetters[j].m_usPageNumber;

                    // Determine the OCR confidence tier this zone belongs to as well as what the upper
                    // and lower boundaries of the tier are by iterating through each tier in ascending
                    // order until the correct one is found (if none is found, it will be in the top 
                    // tier with a upper OCR bound of 100).
                    unsigned char ucLastBoundary = 0;
                    for (long k = 0; k < nConfidenceTierCount; k++)
                    {
                        // Retrieve the next boundary.
                        unsigned char ucBoundary = vecBoundaries[k];

                        // If the next boundary is greater or equal to the current char confidence, we
                        // have found the appropriate tier.
                        if (m_vecLetters[j].m_ucCharConfidence <= ucBoundary)
                        {
                            ucLowerConfidenceBounds = ucLastBoundary;
                            ucUpperConfidenceBounds = ucBoundary;
                            nConfidenceTier = k;
                            break;
                        }
                        // Otherwise update the ucLowerConfidenceBounds in case there are no more tiers
                        // to search (which would mean the character belongs in the top tier).
                        else
                        {
                            ucLowerConfidenceBounds = ucBoundary;
                        }

                        ucLastBoundary = ucBoundary;
                    }
                }
                // If we have changed pages, this is the end of the raster zone as well as the end of
                // the current line.
                else if (sCurrentPage != m_vecLetters[j].m_usPageNumber)
                {
                    bEndOfLine = true;
                    break;
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

                // If this character is the last of the current line, break off the raster zone (but
                // move on to the next char).
                if (getIsEndOfLine(j))
                {
                    bEndOfLine = true;
                    j++;
                    break;
                }

                j++;
            }

            // As long as a raster zone was initialized, apply the properties to ipZoneOCRConfidenceTiers,
            // vecCurrentLineZones and vecAllZones.
            if (sCurrentPage != -1)
            {
                // Record the confidence tier of the zone.
                ipZoneOCRConfidenceTiers->PushBack(nConfidenceTier);

                // Make sure the left side of the zone extends all the way back to the zone that
                // preceeded it on the same line to prevent gaps between the zones.
                if (rectCurrentLineZone != grectNULL)
                {
                    rectCurrentZone.left = rectCurrentLineZone.right;
                }

                // Keep track of all the raster zones on the current line so their top and bottom
                // boundaries can be unified before creating the IRasterZone instances.
                vecCurrentLineZones.push_back(rectCurrentZone);

                // Check to see if the current line's raster zones need to be finalized.
                // [LegacyRCAndUtils:5412] getEndOfLine doesn't always end up getting set to true.
                // If this is the last character, it is the EOL.
                if (bEndOfLine || j == m_vecLetters.size())
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

                // Reset rectCurrentZone so it can be used by the next zone. 
                rectCurrentZone = grectNULL;
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
            ASSERT_RESOURCE_ALLOCATION("ELI25386", ipZone != NULL);

            // Build the raster zone
            ipZone->CreateFromData(lStartX, lStartY, lEndX, lEndY, lHeight, zone.first);

            ipZones->PushBack(ipZone);
        }

        return ipZones;
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25816");
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CSpatialString::getOriginalImageRasterZonesGroupedByConfidence(
    IVariantVectorPtr ipVecOCRConfidenceBoundaries, IVariantVectorPtr &ipZoneOCRConfidenceTiers)
{
    try
    {
        // Get the untranslated zones (OCR image zones)
        IIUnknownVectorPtr ipZones = getOCRImageRasterZonesGroupedByConfidence(
            ipVecOCRConfidenceBoundaries, ipZoneOCRConfidenceTiers);
        ASSERT_RESOURCE_ALLOCATION("ELI25817", ipZones != NULL);

        // Create a new return vector
        IIUnknownVectorPtr ipNewZones(CLSID_IUnknownVector);
        ASSERT_RESOURCE_ALLOCATION("ELI25818", ipNewZones != NULL);

        // Iterate through each of the found zones and translate them to original image coordinates
        long lSize = ipZones->Size();
        for (long i=0; i < lSize; i++)
        {
            UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipZone = ipZones->At(i);
            ASSERT_RESOURCE_ALLOCATION("ELI25819", ipZone != NULL);

            UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipNewZone = translateToOriginalImageZone(ipZone);
            ASSERT_RESOURCE_ALLOCATION("ELI25820", ipNewZone != NULL);

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
        ASSERT_ARGUMENT("ELI25397", ipZone != NULL);

        // Get the data from the raster zone
        long lStartX, lStartY, lEndX, lEndY, lHeight, lPageNum;
        ipZone->GetData(&lStartX, &lStartY, &lEndX, &lEndY, &lHeight, &lPageNum);

        // Return the a new raster zone containing the translated data
        return translateToOriginalImageZone(lStartX, lStartY, lEndX, lEndY, lHeight, lPageNum);
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25822");
}
//-------------------------------------------------------------------------------------------------
UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr CSpatialString::translateToOriginalImageZone(
    long lStartX, long lStartY, long lEndX, long lEndY, long lHeight, long nPage)
{
    try
    {
        return translateToNewPageInfo(lStartX, lStartY, lEndX, lEndY, lHeight, nPage);
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25823");
}
//-------------------------------------------------------------------------------------------------
UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr CSpatialString::translateToNewPageInfo(
    UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipZone, ILongToObjectMapPtr ipNewPageInfoMap/*= NULL*/)
{
    try
    {
        ASSERT_ARGUMENT("ELI28025", m_eMode != kNonSpatialMode);
        ASSERT_ARGUMENT("ELI28026", ipZone != NULL);

        // Get the data from the raster zone
        long lStartX, lStartY, lEndX, lEndY, lHeight, lPageNum;
        ipZone->GetData(&lStartX, &lStartY, &lEndX, &lEndY, &lHeight, &lPageNum);
        
        UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr ipNewPageInfo = NULL;
        if (ipNewPageInfoMap != NULL)
        {
            ipNewPageInfo = ipNewPageInfoMap->GetValue(lPageNum);
            ASSERT_RESOURCE_ALLOCATION("ELI28027", ipNewPageInfo != NULL);
        }

        // Return the a new raster zone containing the translated data
        return translateToNewPageInfo(lStartX, lStartY, lEndX, lEndY, lHeight, lPageNum,
            ipNewPageInfo);
    }
    CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28178");
}
//-------------------------------------------------------------------------------------------------
UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr CSpatialString::translateToNewPageInfo(
    long lStartX, long lStartY, long lEndX, long lEndY, long lHeight, int nPage,
    UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr ipNewPageInfo)
{
    try
    {
        ASSERT_ARGUMENT("ELI25704", m_eMode != kNonSpatialMode);

        // Now build the raster zone for this page
        UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipNewZone(CLSID_RasterZone);
        ASSERT_RESOURCE_ALLOCATION("ELI25390", ipNewZone != NULL);

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
            m_ipPageInfoMap->GetValue(nPage);
        ASSERT_RESOURCE_ALLOCATION("ELI28028", ipOrigPageInfo != NULL);

        // Get the page information
        long lOriginalHeight, lOriginalWidth;
        UCLID_RASTERANDOCRMGMTLib::EOrientation eOrient;
        double deskew;
        ipOrigPageInfo->GetPageInfo(&lOriginalWidth, &lOriginalHeight, &eOrient, &deskew);

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
        double dNewTheta = (ipNewPageInfo == NULL) ? 0 : ipNewPageInfo->GetTheta();

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
        if (ipNewPageInfo != NULL)
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

            // Set the collection of zones
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
        m_ipPageInfoMap = NULL;

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
                ASSERT_RESOURCE_ALLOCATION("ELI19471", ipRZone != NULL);

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
                ASSERT_RESOURCE_ALLOCATION("ELI15093", ipRZone != NULL);

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
        ASSERT_RESOURCE_ALLOCATION("ELI25829", ipWords != NULL);

        long nStartPos = 0;
        long nNumLetters = m_strString.size();
        for (long i = 0; i < nNumLetters; i++)
        {
            if (getIsEndOfWord(i)) 
            {
                // get the word beginning at nStartPos and ending with current letter
                UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipItem = getSubString(nStartPos, i);
                ASSERT_RESOURCE_ALLOCATION("ELI15371", ipItem != NULL);

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
            ASSERT_RESOURCE_ALLOCATION("ELI16914", ipItem != NULL);

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
IIUnknownVectorPtr CSpatialString::getLinesUnknownVector()
{
    try
    {
        // Create an IUnknownVector to be returned
        IIUnknownVectorPtr ipLines(CLSID_IUnknownVector);
        ASSERT_RESOURCE_ALLOCATION("ELI26015", ipLines != NULL);

        // Get the start and end position for each line
        vector<pair<long, long>> vecLines;
        getLines(vecLines);

        // Get a substring for each line and add it to an IUnknownVector
        for (vector<pair<long, long>>::iterator it = vecLines.begin(); it != vecLines.end(); it++)
        {
            UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipLine = getSubString(it->first, it->second);
            ASSERT_RESOURCE_ALLOCATION("ELI26016", ipLine != NULL);

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
        ASSERT_RESOURCE_ALLOCATION("ELI25834", ipParagraphs != NULL);

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
                ASSERT_RESOURCE_ALLOCATION("ELI16916", ipItem != NULL);

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
            ASSERT_RESOURCE_ALLOCATION("ELI16917", ipItem != NULL);

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
        ASSERT_RESOURCE_ALLOCATION("ELI25836", ipWords != NULL);
        long lSize = ipWords->Size();
        for (long iWord = 0; iWord < lSize; iWord++)
        {
            UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipWord = ipWords->At(iWord);
            ASSERT_RESOURCE_ALLOCATION("ELI25837", ipWord != NULL);
            
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
                ASSERT_RESOURCE_ALLOCATION("ELI25840", ipCopyable != NULL);

                // Clone the raster zone
                UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipZone = ipCopyable->Clone();
                ASSERT_RESOURCE_ALLOCATION("ELI25841", ipZone != NULL);

                // Add the copy to the vector of zones
                vecZones.push_back(ipZone);
            }
        }
        else if (m_eMode == kSpatialMode)
        {
            // Handle kSpatialMode objects. Create a raster zone for each line.
            vector<pair<long, long>> vecLines;
            getLines(vecLines);

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
                ASSERT_RESOURCE_ALLOCATION("ELI26006", ipRect != NULL);
                ipRect->SetBounds(rect.left, rect.top, rect.right, rect.bottom);

                // Create a new raster zone from the rectangle
                UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipNewZone(CLSID_RasterZone);
                ASSERT_RESOURCE_ALLOCATION("ELI26007", ipNewZone != NULL);
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
        ASSERT_RESOURCE_ALLOCATION("ELI25845", ipZones != NULL);

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
        ASSERT_RESOURCE_ALLOCATION("ELI25848", ipZones != NULL);

        // Get the OCR image raster zones
        vector<UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr> vecZones = getOCRImageRasterZones();
        for (vector<UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr>::iterator it = vecZones.begin();
            it != vecZones.end(); it++)
        {
            UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipZone(*it);
            ASSERT_RESOURCE_ALLOCATION("ELI25849", ipZone != NULL);
            UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipNewZone =
                translateToOriginalImageZone(ipZone);
            ASSERT_RESOURCE_ALLOCATION("ELI25850", ipNewZone != NULL);

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
        ASSERT_RESOURCE_ALLOCATION("ELI28024", ipZones != NULL);

        // Get the OCR image raster zones
        vector<UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr> vecZones = getOCRImageRasterZones();
        for (vector<UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr>::iterator it = vecZones.begin();
            it != vecZones.end(); it++)
        {
            UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipZone(*it);
            ASSERT_RESOURCE_ALLOCATION("ELI28179", ipZone != NULL);

            UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipNewZone =
                translateToNewPageInfo(ipZone, ipNewPageInfoMap);
            ASSERT_RESOURCE_ALLOCATION("ELI28180", ipNewZone != NULL);

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
        if (m_ipPageInfoMap == NULL)
        {
            throw UCLIDException("ELI30321", "Page info missing, failed to get page bounds!");
        }

        UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr ipPageInfo = m_ipPageInfoMap->GetValue(nPage);
        ASSERT_RESOURCE_ALLOCATION("ELI30322", ipPageInfo != NULL);

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
        ASSERT_RESOURCE_ALLOCATION("ELI30323", ipRect != NULL);
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
        ASSERT_ARGUMENT("ELI07695", ipList != NULL);

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
        ASSERT_ARGUMENT("ELI22329", ipRegExprParser != NULL );
        ASSERT_ARGUMENT("ELI07698", ipList != NULL);

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
                get_bstr_t( strSearch.c_str() ), VARIANT_TRUE, VARIANT_FALSE );
            ASSERT_RESOURCE_ALLOCATION("ELI06827", ipMatches != NULL);

            // If match is found
            if (ipMatches->Size() > 0)
            {
                // Retrieve the match
                IObjectPairPtr ipObjectPair = ipMatches->At(0);
                ASSERT_RESOURCE_ALLOCATION("ELI06828", ipObjectPair != NULL);
                ITokenPtr ipToken = ipObjectPair->Object1;
                ASSERT_RESOURCE_ALLOCATION("ELI06829", ipToken != NULL);
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
        ASSERT_ARGUMENT("ELI25996", ipSource != NULL);
        
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
            ASSERT_RESOURCE_ALLOCATION("ELI25775", ipZones != NULL);

            long lSize = ipZones->Size();
            for (long i=0; i < lSize; i++)
            {
                UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipZone = ipZones->At(i);
                ASSERT_RESOURCE_ALLOCATION("ELI25776", ipZone != NULL);

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
            ICopyableObjectPtr ipCopyObj = ipSource->SpatialPageInfos;
            ASSERT_RESOURCE_ALLOCATION("ELI25777", ipCopyObj != NULL);

            m_ipPageInfoMap = ipCopyObj->Clone();
            ASSERT_RESOURCE_ALLOCATION("ELI25778", m_ipPageInfoMap != NULL);
        }

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
    ASSERT_RESOURCE_ALLOCATION("ELI29753", ipCopyObj != NULL);

    ILongToObjectMapPtr ipPageInfoMap = ipCopyObj->Clone();
    ASSERT_RESOURCE_ALLOCATION("ELI29754", ipPageInfoMap != NULL);

    // Remove the any skew or rotation from m_ipPageInfoMap. (image coordinates)
    IVariantVectorPtr ipKeys = m_ipPageInfoMap->GetKeys();
    ASSERT_RESOURCE_ALLOCATION("ELI29755", ipKeys != NULL);

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
            ASSERT_RESOURCE_ALLOCATION("ELI30134", ipZone != NULL);

            setPageNumbers.insert(ipZone->PageNumber);
        }

        // For each page, go to the original image and build the spatial page info with
        // no rotation and 0 deskew
        for(set<int>::iterator it = setPageNumbers.begin(); it != setPageNumbers.end(); it++)
        {
            // Create a new spatial page info
            UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr ipInfo(CLSID_SpatialPageInfo);
            ASSERT_RESOURCE_ALLOCATION("ELI30135", ipInfo != NULL);

            // Get the image dimensions and set the page info
            int nWidth(0), nHeight(0);
            getImagePixelHeightAndWidth(m_strSourceDocName, nHeight, nWidth, *it);
            ipInfo->SetPageInfo(nWidth, nHeight,
                (UCLID_RASTERANDOCRMGMTLib::EOrientation) kRotNone, 0.0);

            // Update both the new info map and the original info map
            // (Need to update both maps for this case since the page info is not
            // contained in the original map either)
            ipPageInfoMap->Set(*it, ipInfo);
            m_ipPageInfoMap->Set(*it, ipInfo);
        }
    }
    else
    {
        // 0 the deskew and set the rotation to 0 for all page infos
        for (long i = 0; i < nCount; i++)
        {
            UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr ipPageInfo =
                m_ipPageInfoMap->GetValue(ipKeys->GetItem(i));
            ASSERT_RESOURCE_ALLOCATION("ELI29756", ipPageInfo != NULL);

            ipPageInfo->Deskew = 0;
            ipPageInfo->Orientation = (UCLID_RASTERANDOCRMGMTLib::EOrientation)0;
        }
    }

    // Translate the zones to account for the difference between OCR and image coordinate systems
    // (since the page infos currently represent the image coordinate system, the SpatialString
    // currently represents the opposite skew/rotation that it originally did.
    IIUnknownVectorPtr ipZones = getTranslatedImageRasterZones(ipPageInfoMap);
    ASSERT_RESOURCE_ALLOCATION("ELI29757", ipZones != NULL);
    
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
    ASSERT_RESOURCE_ALLOCATION("ELI29870", ipCopyObj != NULL);

    UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipStringToMergeCopy =
        (UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr)ipCopyObj->Clone();
    ASSERT_RESOURCE_ALLOCATION("ELI29871", ipStringToMergeCopy != NULL);

    // Translating raster zones only works on a hybrid string.
    ipStringToMergeCopy->DowngradeToHybridMode();

    // A unified spatial page infos needs to be created. Start with ipStringToMerge's
    // spatial page infos, and replace any shared pages with this string's page info.
    ipCopyObj = ipStringToMergeCopy->SpatialPageInfos;
    ASSERT_RESOURCE_ALLOCATION("ELI29872", ipCopyObj != NULL);

    ILongToObjectMapPtr ipUnifiedPageInfoMap = (ILongToObjectMapPtr)ipCopyObj->Clone();
    ASSERT_RESOURCE_ALLOCATION("ELI29873", ipUnifiedPageInfoMap != NULL);

    IVariantVectorPtr ipExistingSpatialPages = m_ipPageInfoMap->GetKeys();
    ASSERT_RESOURCE_ALLOCATION("ELI29874", ipExistingSpatialPages != NULL);

    long nPageCount = ipExistingSpatialPages->Size;
    for (long i = 0; i < nPageCount; i++)
    {
        long nPage = (long)ipExistingSpatialPages->Item[i];

        UCLID_RASTERANDOCRMGMTLib::ISpatialPageInfoPtr ipPageInfo = m_ipPageInfoMap->GetValue(nPage);
        ASSERT_RESOURCE_ALLOCATION("ELI29875", ipPageInfo != NULL);

        ipUnifiedPageInfoMap->Set(nPage, ipPageInfo);
    }

    // In order for ipStringToMerge's rasterZones to show up in the correct spot if the spatial page
    // infos differed at all, we need to convert ipStringToMerge's raster zones into the
    // ipUnifiedPageInfoMap coordinate system.
    IIUnknownVectorPtr ipTranslatedRasterZones =
        ipStringToMergeCopy->GetTranslatedImageRasterZones(ipUnifiedPageInfoMap);
    ASSERT_RESOURCE_ALLOCATION("ELI29876", ipTranslatedRasterZones != NULL);

    // Recreate ipStringToMergeCopy using the translated raster zones and
    // unifiedSpatialPageInfos. The two spatial strings are now able to be
    // merged.
    ipStringToMergeCopy->CreateHybridString(ipTranslatedRasterZones, ipStringToMergeCopy->String,
        m_strSourceDocName.c_str(), ipUnifiedPageInfoMap);

    // Append the spatially compatible ipStringToMergeCopy
    append(ipStringToMergeCopy);
}
//-------------------------------------------------------------------------------------------------
