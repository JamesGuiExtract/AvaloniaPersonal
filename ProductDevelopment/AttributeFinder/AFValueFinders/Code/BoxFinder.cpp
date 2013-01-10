// BoxFinder.cpp : Implementation of CBoxFinder

#include "stdafx.h"
#include "BoxFinder.h"
#include "Common.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>
#include <Misc.h>
#include <AFTagManager.h>
#include <CPPLetter.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion		= 4;
// Version 2: 
//		Added m_bFirstBoxOnly
// Version 3:
//		Box Min/Max Width/Height are now floating point coordinates.
// Version 4: Added CIdentifiableRuleObject

const int gnMIN_ZONE_HEIGHT					= 20;
const int gnMIN_ZONE_WIDTH					= 20;
const long gnUNSPECIFIED					= -1;
const double gdUNSPECIFIED					= numeric_limits<double>::signaling_NaN();
const int gnDATA_SEARCH_RECT_SIZE			= 25;
const string gstrDEFAULT_ATTRIBUTE_TEXT		= "000-00-0000";
const long gnLINEUTIL_COLUMN_COUNT_MIN		= 1;
const long gnLINEUTIL_COLUMN_WIDTH_MIN		= 1;
const long gnLINEUTIL_OVERALL_WIDTH_MIN		= 1;
const long gnLINEUTIL_LINE_LENGTH_MIN		= 200;
const long gnLINEUTIL_LINE_SPACING_MIN		= 1;
const long gnLINEUTIL_LINE_SPACING_MAX		= 100;
const long gnLINEUTIL_HORZ_LINE_BRIDGE_GAP	= 150;
const long gnLINEUTIL_VERT_LINE_BRIDGE_GAP	= 50;
const long gnLINEUTIL_EXTENSION_CONSECUTIVE	= 9;
const long gnLINEUTIL_EXTENSION_GAP_ALLOWANCE = 13;
const long gnLINEUTIL_EXTENSION_SCAN_WIDTH	= 3;
const double gdMINIMUM_DIMENSION_LOWER_LIMIT	= 0.0;
const double gdMINIMUM_DIMENSION_UPPER_LIMIT	= 100.0;
const double gdMAXIMUM_DIMENSION_LOWER_LIMIT	= 0.0;
const double gdMAXIMUM_DIMENSION_UPPER_LIMIT	= 100.0;

//-------------------------------------------------------------------------------------------------
// CBoxFinder
//-------------------------------------------------------------------------------------------------
CBoxFinder::CBoxFinder() :
	m_bDirty(false)
{
	try
	{
		resetDataMembers();

		m_ipClues.CreateInstance(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI19742", m_ipClues != __nullptr);

		m_ipMisc.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI22432", m_ipMisc != __nullptr);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29473");
}
//-------------------------------------------------------------------------------------------------
CBoxFinder::~CBoxFinder()
{
	try
	{
		m_ipClues = __nullptr;
		m_ipImageLineUtility = __nullptr;
		m_ipSpatialStringSearcher = __nullptr;
		m_ipMisc = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI19219");
}
//-------------------------------------------------------------------------------------------------
HRESULT CBoxFinder::FinalConstruct()
{
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
void CBoxFinder::FinalRelease()
{
}

//--------------------------------------------------------------------------------------------------
// IBoxFinder
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::get_Clues(IUnknown **ppVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI19694", ppVal != __nullptr);

		validateLicense();

		IVariantVectorPtr ipShallowCopy = m_ipClues;
		*ppVal = ipShallowCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19695")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::put_Clues(IUnknown *pNewVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI19909", pNewVal != __nullptr);

		validateLicense();

		m_ipClues = pNewVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19696")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::get_CluesAreRegularExpressions(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI19697", pVal != __nullptr);

		validateLicense();
		
		*pVal = asVariantBool(m_bCluesAreRegularExpressions);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19698")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::put_CluesAreRegularExpressions(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bCluesAreRegularExpressions = asCppBool(newVal);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19699")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::get_CluesAreCaseSensitive(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI19700", pVal != __nullptr);

		validateLicense();
		
		*pVal = asVariantBool(m_bCluesAreCaseSensitive);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19701")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::put_CluesAreCaseSensitive(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bCluesAreCaseSensitive = asCppBool(newVal);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19702")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::get_ClueLocation(EClueLocation *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI19703", pVal != __nullptr);

		validateLicense();

		*pVal = m_eClueLocation;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19704")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::put_ClueLocation(EClueLocation newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_eClueLocation = newVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19705")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::get_PageSelectionMode(EPageSelectionMode *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI19706", pVal != __nullptr);

		validateLicense();

		*pVal = m_ePageSelectionMode;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19707")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::put_PageSelectionMode(EPageSelectionMode newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_ePageSelectionMode = newVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19708")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::get_NumFirstPages(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI19709", pVal != __nullptr);

		validateLicense();

		*pVal = m_nNumFirstPages;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19710")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::put_NumFirstPages(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_nNumFirstPages = newVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19711")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::get_NumLastPages(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI19712", pVal != __nullptr);

		validateLicense();

		*pVal = m_nNumLastPages;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19713")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::put_NumLastPages(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_nNumLastPages = newVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19714")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::get_SpecifiedPages(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI19715", pVal != __nullptr);

		validateLicense();

		*pVal = get_bstr_t(m_strSpecifiedPages).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19716")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::put_SpecifiedPages(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_strSpecifiedPages = asString(newVal);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19717")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::get_BoxWidthMin(double *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI19718", pVal != __nullptr);

		validateLicense();

		*pVal = m_dBoxWidthMin;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19719")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::put_BoxWidthMin(double newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// verify that the new value is either gdUNSPECIFIED or a valid percentage
		ASSERT_ARGUMENT("ELI19894", !isDoubleValueSpecified(newVal) || 
			(newVal >= gdMINIMUM_DIMENSION_LOWER_LIMIT && newVal <= gdMINIMUM_DIMENSION_UPPER_LIMIT));

		validateLicense();

		m_dBoxWidthMin = newVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19720")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::get_BoxWidthMax(double *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI19721", pVal != __nullptr);

		validateLicense();

		*pVal = m_dBoxWidthMax;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19722")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::put_BoxWidthMax(double newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// verify that the new value is either gdUNSPECIFIED or a valid percentage
		ASSERT_ARGUMENT("ELI19891", !isDoubleValueSpecified(newVal) || 
			(newVal >= gdMAXIMUM_DIMENSION_LOWER_LIMIT && newVal <= gdMAXIMUM_DIMENSION_UPPER_LIMIT));

		validateLicense();

		m_dBoxWidthMax = newVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19723")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::get_BoxHeightMin(double *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI19724", pVal != __nullptr);

		validateLicense();

		*pVal = m_dBoxHeightMin;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19725")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::put_BoxHeightMin(double newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// verify that the new value is either gdUNSPECIFIED or a valid percentage
		ASSERT_ARGUMENT("ELI19892", !isDoubleValueSpecified(newVal) || 
			(newVal >= gdMINIMUM_DIMENSION_LOWER_LIMIT && newVal <= gdMINIMUM_DIMENSION_UPPER_LIMIT));

		validateLicense();

		m_dBoxHeightMin = newVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19726")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::get_BoxHeightMax(double *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI19727", pVal != __nullptr);

		validateLicense();

		*pVal = m_dBoxHeightMax;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19728")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::put_BoxHeightMax(double newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// verify that the new value is either gdUNSPECIFIED or a valid percentage
		ASSERT_ARGUMENT("ELI19893", !isDoubleValueSpecified(newVal) || 
			(newVal >= gdMAXIMUM_DIMENSION_LOWER_LIMIT && newVal <= gdMAXIMUM_DIMENSION_UPPER_LIMIT));

		validateLicense();

		m_dBoxHeightMax = newVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19729")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::get_FindType(EFindType *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI19730", pVal != __nullptr);

		validateLicense();

		*pVal = m_eFindType;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19731")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::put_FindType(EFindType newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_eFindType = newVal;

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19732")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::get_AttributeText(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI19733", pVal != __nullptr);

		validateLicense();

		*pVal = get_bstr_t(m_strAttributeText).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19734")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::put_AttributeText(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_strAttributeText = asString(newVal);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19735")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::get_ExcludeClueArea(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI19736", pVal != __nullptr);

		validateLicense();

		*pVal = asVariantBool(m_bExcludeClueArea);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19737")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::put_ExcludeClueArea(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bExcludeClueArea = asCppBool(newVal);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19738")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::get_IncludeClueText(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI19739", pVal != __nullptr);

		validateLicense();

		*pVal = asVariantBool(m_bIncludeClueText);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19740")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::put_IncludeClueText(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bIncludeClueText = asCppBool(newVal);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19741")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::get_IncludeLines(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI19234", pVal != __nullptr);

		validateLicense();

		*pVal = asVariantBool(m_bIncludeLines);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19235")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::put_IncludeLines(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bIncludeLines = asCppBool(newVal);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19236")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::get_FirstBoxOnly(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI20209", pVal != __nullptr);

		validateLicense();

		*pVal = asVariantBool(m_bFirstBoxOnly);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20210")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::put_FirstBoxOnly(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		m_bFirstBoxOnly = asCppBool(newVal);

		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI20211")

	return S_OK;
}


//-------------------------------------------------------------------------------------------------
// IAttributeFindingRule
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::raw_ParseText(IAFDocument* pAFDoc, IProgressStatus *pProgressStatus, 
									   IIUnknownVector **ppAttributes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{		
		validateLicense();

		IAFDocumentPtr ipAFDoc(pAFDoc);
		ASSERT_ARGUMENT("ELI19220", ipAFDoc != __nullptr);
		ASSERT_ARGUMENT("ELI19222", ppAttributes != __nullptr);

		ISpatialStringPtr ipDocText(ipAFDoc->Text);
		ASSERT_RESOURCE_ALLOCATION("ELI20224", ipDocText != __nullptr);

		// The search should stop when this flag gets set to true
		bool bEndSearch = false;

		// Expand clue list to replace any file names with the clues from the specified file
		IVariantVectorPtr ipExpandedClues = getExpandedClueList(ipAFDoc);

		// Create an attribute vector to store the results
		IIUnknownVectorPtr ipAttributes(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI19223", ipAttributes != __nullptr );

		// Get a regular expression parser
		IRegularExprParserPtr ipParser = m_ipMisc->GetNewRegExpParserInstance("BoxFinder");
		ASSERT_RESOURCE_ALLOCATION("ELI22433", ipParser != __nullptr);

		// Populate a vector of ints that indicates which pages
		// to process
		const vector<int> &vecPages = getPagesToSearch(ipDocText);

		// Process one page at a time
		for each (int nPageNum in vecPages)
		{
			// Keep track of whether there was any short-circuiting of the box finding algorithm
			// while searching this page.
			bool bPageResultsIncomplete = false;

			// Search for any clues that exist on this page
			bool bPageContainsText;
			IIUnknownVectorPtr ipFoundClues = getCluesOnPage(ipExpandedClues, ipDocText, 
															 nPageNum, bPageContainsText,
															 ipParser);

			// [P16:3005] If the page contains no text, we won't be able to obtain the PageInfo
			// to search for lines.  Ignore this page.
			if (bPageContainsText == false)
			{
				continue;
			}

			// Create an IUnknownVector to keep track of boxes that are found
			IIUnknownVectorPtr ipFoundBoxes(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI19858", ipFoundBoxes != __nullptr);

			IIUnknownVectorPtr ipHorzLineRects = __nullptr;
			IIUnknownVectorPtr ipVertLineRects = __nullptr;

			// Retrieve the page info to check the page's orientation
			ISpatialPageInfoPtr ipPageInfo = ipDocText->GetPageInfo(nPageNum);
			ASSERT_RESOURCE_ALLOCATION("ELI19992", ipPageInfo != __nullptr);

			// Determine which way to orient the search based on the page text orientation.
			double dRotation = ipPageInfo->Deskew;

			switch (ipPageInfo->Orientation)
			{
				case kRotNone: dRotation += 0; break;
				case kRotLeft: dRotation += 90; break;
				case kRotDown: dRotation += 180; break;
				case kRotRight: dRotation += 270; break;
			}

			// If any clues were found or lines are requested in output, search for lines.
			if (ipFoundClues->Size() > 0 || m_bIncludeLines)
			{
				// I'm having the best luck at this point by using a smaller gap bridge setting
				// when searching for smaller lines.  Therefore, search for the lines in two
				// steps: First find horizontal lines with the normal BridgeGap setting,
				// then override the bridge gap setting when searching for vertical lines.
				// This is less efficient, so the issue may need to be revisited.
				getImageLineUtility()->LineBridgeGap = gnLINEUTIL_HORZ_LINE_BRIDGE_GAP;

				// Look for horizontal lines first
				getImageLineUtility()->FindLines(
					ipDocText->SourceDocName, nPageNum, -dRotation, &ipHorzLineRects, NULL);

				// Reset line bridge setting for vertical lines
				getImageLineUtility()->LineBridgeGap = gnLINEUTIL_VERT_LINE_BRIDGE_GAP;

				// Look for vertical lines now
				getImageLineUtility()->FindLines(
					ipDocText->SourceDocName, nPageNum, -dRotation, NULL, &ipVertLineRects);
			}

			// If lines are requested, create an attribute from the rects of the found lines.
			if (m_bIncludeLines)
			{
				IIUnknownVectorPtr ipLineRects(CLSID_IUnknownVector);
				ASSERT_RESOURCE_ALLOCATION("ELI19851", ipLineRects != __nullptr);

				// Include both horizontal and vertial lines in the same attribute.
				ipLineRects->Append(ipHorzLineRects);
				ipLineRects->Append(ipVertLineRects);

				if (ipLineRects->Size() > 0)
				{
					// Create an attribute representing the lines on the page
					string strAttributeName = "Page " + asString(nPageNum) + " Lines";
					IAttributePtr ipLines = createAttributeFromRects(ipLineRects, strAttributeName, 
						asString(ipDocText->SourceDocName), ipDocText->SpatialPageInfos, nPageNum);

					ipAttributes->PushBack(ipLines);
				}
			}

			// For each clue that was found...
			int nNumClues = ipFoundClues->Size();
			for (int i = 0; i < nNumClues && bEndSearch == false; i++)
			{
				ISpatialStringPtr ipFoundClue = ipFoundClues->At(i);
				ASSERT_RESOURCE_ALLOCATION("ELI20230", ipFoundClue != __nullptr);

				// [P16:3004] If the found clue doesn't have any spatial info, it is not valid
				// to search for a box that contains it.  Ignore this clue.
				if (ipFoundClue->HasSpatialInfo() == VARIANT_FALSE)
				{
					continue;
				}

				ILongRectanglePtr ipClueRect = ipFoundClue->GetOCRImageBounds();

				bool bIncompleteBoxResult = false;

				ILongRectanglePtr ipDataBox = findBoxContainingClue(ipClueRect,
					ipHorzLineRects, ipVertLineRects, bIncompleteBoxResult);

				// If there was a problem searching for this box, set the PageResultsIncomplete flag.
				if (bIncompleteBoxResult)
				{
					bPageResultsIncomplete = true;
				}

				// If we found a box that meets the required dimensions...
				if (ipDataBox != __nullptr && 
					qualifyBox(ipDataBox, ipDocText->GetPageInfo(nPageNum), ipFoundBoxes))
				{
					IAttributePtr ipResult = __nullptr;

					if (m_eFindType == kImageRegion)
					{
						// Return a spatial string occupying the area of the box that was located
						if (m_bExcludeClueArea)
						{
							// Exclude the vertical extent of the clue from area the spatial string
							// will occupy
							ipDataBox = excludeVerticalSpatialAreaOfClue(ipDataBox, ipClueRect);
							if (ipDataBox == __nullptr)
							{
								// If the exclusion process eliminated the found region entirely
								// (if the clue is bigger than the found box itself) then don't return
								// the attribute
								continue;
							}
						}

						// Create the resulting attribute
						ipResult = createRegionResult(ipDocText, nPageNum, ipDataBox);
					}
					else if (m_eFindType == kText)
					{
						// Create the resulting attribute
						ipResult = createTextResult(ipAFDoc, ipDataBox, ipFoundClue, ipParser);
					}
					else
					{
						THROW_LOGIC_ERROR_EXCEPTION("ELI19784");
					}

					// Add the resulting attribute to the the attribute list
					ASSERT_RESOURCE_ALLOCATION("ELI20228", ipResult != __nullptr);					
					ipAttributes->PushBack(ipResult);

					// Stop searching if so configured
					if (m_bFirstBoxOnly)
					{
						bEndSearch = true;
					}
				}
			}

			if (bPageResultsIncomplete)
			{
				UCLIDException ue("ELI21528", "Application trace: Box search aborted! Too many potential boxes " 
					"found; results may not be accurate.");
				ue.addDebugInfo("Filename", asString(ipDocText->SourceDocName));
				ue.addDebugInfo("Page", nPageNum);
				ue.log();
			}

			if (bEndSearch)
			{
				break;
			}
		}

		// return the vector
		*ppAttributes = ipAttributes.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19224");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IMustBeConfiguredObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::raw_IsConfigured(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license
		validateLicense();

		// Check parameter
		ASSERT_ARGUMENT("ELI19225", pbValue != __nullptr);

		// Ensure clues and attribute text is specified (if necessary).  All other settings will be
		// validated as they are set.
		if (m_ipClues->Size == 0)
		{
			*pbValue = VARIANT_FALSE;
		}
		else if	(m_eFindType == (UCLID_AFVALUEFINDERSLib::EFindType) kImageRegion && 
			     m_strAttributeText.empty())
		{
			*pbValue = VARIANT_FALSE;
		}
		else
		{
			*pbValue = VARIANT_TRUE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19226");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::raw_GetComponentDescription(BSTR *pbstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19227", pbstrComponentDescription != __nullptr)

		*pbstrComponentDescription = _bstr_t("Box finder").Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19228")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license
		validateLicense();

		UCLID_AFVALUEFINDERSLib::IBoxFinderPtr ipCopyThis = pObject;
		ASSERT_ARGUMENT("ELI19229", ipCopyThis != __nullptr);

		// Clone the clue list member
		ICopyableObjectPtr ipCopyableClues = ipCopyThis->Clues;
		ASSERT_RESOURCE_ALLOCATION("ELI19693", ipCopyableClues != __nullptr);
		m_ipClues = ipCopyableClues->Clone();
		ASSERT_RESOURCE_ALLOCATION("ELI19910", m_ipClues != __nullptr);

		// Copy data members
		m_bCluesAreRegularExpressions	= asCppBool(ipCopyThis->CluesAreRegularExpressions);
		m_bCluesAreCaseSensitive		= asCppBool(ipCopyThis->CluesAreCaseSensitive);
		m_bFirstBoxOnly					= asCppBool(ipCopyThis->FirstBoxOnly);
		m_eClueLocation					= (EClueLocation) ipCopyThis->ClueLocation;
		m_ePageSelectionMode			= (EPageSelectionMode) ipCopyThis->PageSelectionMode;
		m_nNumFirstPages				= ipCopyThis->NumFirstPages;
		m_nNumLastPages					= ipCopyThis->NumLastPages;
		m_strSpecifiedPages				= asString(ipCopyThis->SpecifiedPages);
		m_dBoxWidthMin					= ipCopyThis->BoxWidthMin;
		m_dBoxWidthMax					= ipCopyThis->BoxWidthMax;
		m_dBoxHeightMin					= ipCopyThis->BoxHeightMin;
		m_dBoxHeightMax					= ipCopyThis->BoxHeightMax;
		m_eFindType						= (EFindType) ipCopyThis->FindType;
		m_strAttributeText				= asString(ipCopyThis->AttributeText);
		m_bExcludeClueArea				= asCppBool(ipCopyThis->ExcludeClueArea);
		m_bIncludeClueText				= asCppBool(ipCopyThis->IncludeClueText);
		m_bIncludeLines					= asCppBool(ipCopyThis->IncludeLines);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19237");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license
		validateLicense();

		ASSERT_ARGUMENT("ELI19238", pObject != __nullptr);

		// Create another instance of this object
		ICopyableObjectPtr ipObjCopy(CLSID_BoxFinder);
		ASSERT_RESOURCE_ALLOCATION("ELI19239", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);
	
		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19240");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19241", pClassID != __nullptr);

		*pClassID = CLSID_BoxFinder;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19242");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		return m_bDirty ? S_OK : S_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19243");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		ASSERT_ARGUMENT("ELI20165", pStream != __nullptr);

		// Reset data members
		resetDataMembers();
		
		// Read the bytestream data from the IStream object
		long nDataLength = 0;
		pStream->Read(&nDataLength, sizeof(nDataLength), NULL);
		ByteStream data(nDataLength);
		pStream->Read(data.getData(), nDataLength, NULL);
		ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);

		// Read the individual data items from the bytestream
		unsigned long nDataVersion = 0;
		dataReader >> nDataVersion;

		// Check for newer version
		if (nDataVersion > gnCurrentVersion)
		{
			// Throw exception
			UCLIDException ue("ELI19244", "Unable to load newer box finding rule!");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		// Read data members from the stream
		dataReader >> m_bCluesAreRegularExpressions;
		dataReader >> m_bCluesAreCaseSensitive;

		if (nDataVersion >= 2)
		{
			dataReader >> m_bFirstBoxOnly;
		}

		long lTemp = (long) kSameBox;
		dataReader >> lTemp;
		m_eClueLocation = (EClueLocation) lTemp;

		lTemp = (long) kAllPages;
		dataReader >> lTemp;
		m_ePageSelectionMode = (EPageSelectionMode) lTemp;

		dataReader >> m_nNumFirstPages;
		dataReader >> m_nNumLastPages;
		dataReader >> m_strSpecifiedPages;
		if (nDataVersion < 3)
		{
			long temp;
			dataReader >> temp;
			m_dBoxWidthMin = (temp == gnUNSPECIFIED) ? gdUNSPECIFIED : temp;
			dataReader >> temp;
			m_dBoxWidthMax = (temp == gnUNSPECIFIED) ? gdUNSPECIFIED : temp;
			dataReader >> temp;
			m_dBoxHeightMin = (temp == gnUNSPECIFIED) ? gdUNSPECIFIED : temp;
			dataReader >> temp;
			m_dBoxHeightMax = (temp == gnUNSPECIFIED) ? gdUNSPECIFIED : temp;
		}
		else
		{
			dataReader >> m_dBoxWidthMin;
			dataReader >> m_dBoxWidthMax;
			dataReader >> m_dBoxHeightMin;
			dataReader >> m_dBoxHeightMax;
		}
		
		lTemp = (long) kImageRegion;
		dataReader >> lTemp;
		m_eFindType = (EFindType) lTemp;

		dataReader >> m_strAttributeText;
		dataReader >> m_bExcludeClueArea;
		dataReader >> m_bIncludeClueText;
		dataReader >> m_bIncludeLines;

		// Read the clues list object from the stream
		IPersistStreamPtr ipClues;
		readObjectFromStream(ipClues, pStream, "ELI19743");
		m_ipClues = ipClues;

		if (nDataVersion >= 4)
		{
			// Load the GUID for the IIdentifiableRuleObject interface.
			loadGUID(pStream);
		}
		
		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19246");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		ASSERT_ARGUMENT("ELI19247", pStream != __nullptr);

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		// Write the current version
		dataWriter << gnCurrentVersion;

		// Write the data members to the stream
		dataWriter << m_bCluesAreRegularExpressions;
		dataWriter << m_bCluesAreCaseSensitive;
		dataWriter << m_bFirstBoxOnly;
		dataWriter << (long) m_eClueLocation;
		dataWriter << (long) m_ePageSelectionMode;
		dataWriter << m_nNumFirstPages;
		dataWriter << m_nNumLastPages;
		dataWriter << m_strSpecifiedPages;
		dataWriter << m_dBoxWidthMin;
		dataWriter << m_dBoxWidthMax;
		dataWriter << m_dBoxHeightMin;
		dataWriter << m_dBoxHeightMax;
		dataWriter << (long) m_eFindType;
		dataWriter << m_strAttributeText;
		dataWriter << m_bExcludeClueArea;
		dataWriter << m_bIncludeClueText;
		dataWriter << m_bIncludeLines;

		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);

		// Write the clues list to the stream
		IPersistStreamPtr ipClueStream(m_ipClues);
		ASSERT_RESOURCE_ALLOCATION("ELI19744", ipClueStream != __nullptr);
		writeObjectToStream(ipClueStream, pStream, "ELI19745", fClearDirty);

		// Save the GUID for the IIdentifiableRuleObject interface.
		saveGUID(pStream);

		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19250");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19251", pbValue != __nullptr);

		try
		{
			// check the license
			validateLicense();

			// If no exception, then pbValue is true
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19252");

	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::InterfaceSupportsErrorInfo(REFIID riid)
{
	try
	{
		static const IID* arr[] = 
		{
			&IID_IImageRegionWithLines,
			&IID_IAttributeFindingRule,
			&IID_IPersistStream,
			&IID_ICategorizedComponent,
			&IID_ISpecifyPropertyPages,
			&IID_ICopyableObject,
			&IID_IMustBeConfiguredObject,
			&IID_ILicensedComponent,
			&IID_IIdentifiableRuleObject
		};

		for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
		{
			if (InlineIsEqualGUID(*arr[i],riid))
				return S_OK;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19253")

	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IIdentifiableRuleObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinder::get_InstanceGUID(GUID *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		*pVal = getGUID();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI33563")
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
IImageLineUtilityPtr CBoxFinder::getImageLineUtility()
{
	// Create image Utils object if not already created
	if (m_ipImageLineUtility == __nullptr)
	{
		m_ipImageLineUtility.CreateInstance(CLSID_ImageLineUtility);
		ASSERT_RESOURCE_ALLOCATION("ELI19254", m_ipImageLineUtility != __nullptr);
		
		// Initialize the line utility with parameters geared toward finding boxes
		m_ipImageLineUtility->ColumnCountMin			= gnLINEUTIL_COLUMN_COUNT_MIN;
		m_ipImageLineUtility->ColumnWidthMin			= gnLINEUTIL_COLUMN_WIDTH_MIN;
		m_ipImageLineUtility->OverallWidthMin			= gnLINEUTIL_OVERALL_WIDTH_MIN;
		m_ipImageLineUtility->LineLengthMin				= gnLINEUTIL_LINE_LENGTH_MIN;
		m_ipImageLineUtility->LineSpacingMin			= gnLINEUTIL_LINE_SPACING_MIN;
		m_ipImageLineUtility->LineSpacingMax			= gnLINEUTIL_LINE_SPACING_MAX;
		m_ipImageLineUtility->LineBridgeGap				= gnLINEUTIL_HORZ_LINE_BRIDGE_GAP;
		m_ipImageLineUtility->ExtensionConsecutiveMin	= gnLINEUTIL_EXTENSION_CONSECUTIVE;
		m_ipImageLineUtility->ExtensionGapAllowance		= gnLINEUTIL_EXTENSION_GAP_ALLOWANCE;
		m_ipImageLineUtility->ExtensionScanWidth		= gnLINEUTIL_EXTENSION_SCAN_WIDTH;
	}

	return m_ipImageLineUtility;
}
//-------------------------------------------------------------------------------------------------
ISpatialStringSearcherPtr CBoxFinder::getSpatialStringSearcher()
{
	if (m_ipSpatialStringSearcher == __nullptr)
	{
		m_ipSpatialStringSearcher.CreateInstance(CLSID_SpatialStringSearcher);
		ASSERT_RESOURCE_ALLOCATION("ELI19643", m_ipSpatialStringSearcher != __nullptr);

		// Set the string searcher to ignore data that intersects with 
		// the boundary of the search area
		m_ipSpatialStringSearcher->SetIncludeDataOnBoundary(VARIANT_FALSE);
	}

	return m_ipSpatialStringSearcher;
}
//-------------------------------------------------------------------------------------------------
IAttributePtr CBoxFinder::createAttributeFromRects(IIUnknownVectorPtr ipRects, 
		const string &strText, const string &strSourceDocName, ILongToObjectMapPtr ipPageInfoMap,
		int nPageNum)
{
	ASSERT_ARGUMENT("ELI19255", ipRects != __nullptr);
	ASSERT_ARGUMENT("ELI19256", !strText.empty());

	// Create an IIUnknownVector to store the raster zones that will be used for the attribute
	IIUnknownVectorPtr ipAttributeZones(CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION("ELI19257", ipAttributeZones != __nullptr);

	// For each ipRect that is to be used
	int nNumRects = ipRects->Size();
	for (long i = 0; i < nNumRects; i++)
	{	
		// Obtain a copy of the rect for the raster zone
		ILongRectanglePtr ipRect = ipRects->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI19259", ipRect != __nullptr);

		ILongRectanglePtr ipRectClone = ipRect->Clone();
		ASSERT_RESOURCE_ALLOCATION("ELI19260", ipRectClone != __nullptr);

		// Ensure the each rect has a height of at least gnMIN_ZONE_HEIGHT to be sure it is noticeable
		int nHeight = ipRectClone->Bottom - ipRectClone->Top;
		if (nHeight < gnMIN_ZONE_HEIGHT)
		{
			int nPadding = gnMIN_ZONE_HEIGHT - nHeight;
			ipRectClone->Expand(0, nPadding);
			ipRectClone->Offset(0, - min(ipRectClone->Top , nPadding / 2));
		}

		// Ensure the each rect has a width of at least gnMIN_ZONE_WIDTH to be sure it is noticeable
		int nWidth = ipRectClone->Right - ipRectClone->Left;
		if (nWidth < gnMIN_ZONE_WIDTH)
		{
			int nPadding = gnMIN_ZONE_WIDTH - nWidth;
			ipRectClone->Expand(nPadding, 0);
			ipRectClone->Offset(- min(ipRectClone->Left , nPadding / 2), 0);
		}

		// Create the raster zone
		IRasterZonePtr	ipZone(CLSID_RasterZone);
		ASSERT_RESOURCE_ALLOCATION("ELI19261", ipZone != __nullptr);

		ipZone->CreateFromLongRectangle(ipRectClone, nPageNum);

		// Store the raster zone
		ipAttributeZones->PushBack(ipZone);
	}

	// Build a SpatialString from the raster zone vector
	ISpatialStringPtr ipValue(CLSID_SpatialString);
	ASSERT_RESOURCE_ALLOCATION("ELI19262", ipValue != __nullptr);

	ipValue->CreateHybridString(ipAttributeZones, strText.c_str(), 
		strSourceDocName.c_str(), ipPageInfoMap);

	// Assign the spatial string to the attribute
	IAttributePtr ipAttribute(CLSID_Attribute);
	ASSERT_RESOURCE_ALLOCATION("ELI19263", ipAttribute != __nullptr );
	ipAttribute->Value = ipValue;

	return ipAttribute;
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CBoxFinder::getCluesOnPage(IVariantVectorPtr ipClues, ISpatialStringPtr ipDocText, 
											  int nPageNum, bool &rbPageContainsText,
											  IRegularExprParserPtr ipParser)
{
	ASSERT_ARGUMENT("ELI20221", ipClues);
	ASSERT_ARGUMENT("ELI19833", ipDocText != __nullptr);
	ASSERT_ARGUMENT("ELI19834", nPageNum > 0);

	// Allocate vector to receive the clues
	IIUnknownVectorPtr ipFoundClues(CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION("ELI19853", ipFoundClues != __nullptr);

	// Retrieve the spatial string data for the specified page
	ISpatialStringPtr ipPage = ipDocText->GetSpecifiedPages(nPageNum, nPageNum);
	ASSERT_RESOURCE_ALLOCATION("ELI19830", ipPage != __nullptr);

	if (ipPage->String.length() == 0)
	{
		// Report that no text was on the page
		rbPageContainsText = false;
	}
	else // If there was text on the page
	{
		rbPageContainsText = true;
		long nStart = -1;
		long nEnd = -1;

		// Loop collecting one clue per pass
		do
		{
			if (m_bCluesAreRegularExpressions)
			{
				// Search for clue as regular expression
				ipPage->FindFirstItemInRegExpVector(ipClues, 
					asVariantBool(m_bCluesAreCaseSensitive), asVariantBool(m_bFirstBoxOnly), 
					nStart + 1, ipParser, &nStart, &nEnd);
			}
			else
			{
				// Search for clue as string
				ipPage->FindFirstItemInVector(ipClues,
					asVariantBool(m_bCluesAreCaseSensitive), asVariantBool(m_bFirstBoxOnly),
					nStart + 1, &nStart, &nEnd);
			}

			if (nStart != -1)
			{
				// If we found a clue, extract it from the page info and add it to the ipClues vector.
				ISpatialStringPtr ipClue = ipPage->GetSubString(nStart, nEnd);
				ASSERT_RESOURCE_ALLOCATION("ELI19831", ipClue != __nullptr);

				ipFoundClues->PushBack(ipClue);
			}
		}
		while (nStart != -1);
	}

	if (ipFoundClues->Size() > 0 && m_eFindType == kText)
	{
		// If we found clues on this page, and we are going to return text (meaning we will need
		// to search for text in a box), point the string searcher to the current page
		getSpatialStringSearcher()->InitSpatialStringSearcher(ipPage);
	}

	return ipFoundClues;
}
//-------------------------------------------------------------------------------------------------
ILongRectanglePtr CBoxFinder::findBoxContainingClue(ILongRectanglePtr ipNativeClueRect,
													IIUnknownVectorPtr ipHorzLineRects,
													IIUnknownVectorPtr ipVertLineRects,
													bool &rbIncompleteResult)
{
	ASSERT_ARGUMENT("ELI20234", ipNativeClueRect != __nullptr);
	ASSERT_ARGUMENT("ELI20235", ipHorzLineRects != __nullptr);
	ASSERT_ARGUMENT("ELI20236", ipVertLineRects != __nullptr);

	// Initialize return value to NULL
	ILongRectanglePtr ipDataBox = __nullptr;

	VARIANT_BOOL bIncompleteResults;

	// Search for a box that contains the clue's bounds
	ILongRectanglePtr ipClueBox = getImageLineUtility()->FindBoxContainingRect(
		ipNativeClueRect, ipHorzLineRects, ipVertLineRects, 0, &bIncompleteResults);

	// If there was a problem searching for this box, set the rbIncompleteResult parameter to true.
	rbIncompleteResult = asCppBool(bIncompleteResults);

	// If we found a box containing the clue
	if (ipClueBox != __nullptr)
	{
		if (m_eClueLocation == kSameBox)
		{
			// Data is to be contained in the same box
			ipDataBox = ipClueBox;
		}
		else
		{
			// Create a rectangle describing where to look for the data box
			// based on the location of the clue box
			ILongRectanglePtr ipDataSearchRect = createDataSearchRect(ipClueBox);

			// Attempt to find the box containing the desired data
			ipDataBox = getImageLineUtility()->FindBoxContainingRect(
				ipDataSearchRect, ipHorzLineRects, ipVertLineRects, 1, &bIncompleteResults);

			// If there was a problem searching for this box, set the rbIncompleteResult parameter to true.
			if (asCppBool(bIncompleteResults))
			{
				rbIncompleteResult = true;
			}
		}
	}

	return ipDataBox;
}
//-------------------------------------------------------------------------------------------------
ILongRectanglePtr CBoxFinder::createDataSearchRect(ILongRectanglePtr ipClueRect)
{
	ASSERT_ARGUMENT("ELI19838", ipClueRect != __nullptr);

	// Create an ILongRectangle
	ILongRectanglePtr ipDataSearchRect(CLSID_LongRectangle);
	ASSERT_RESOURCE_ALLOCATION("ELI19839", ipDataSearchRect != __nullptr);

	// Set bounds to match clue rects bounds where appropriate.  On the other bounds, offset them
	// from the ClueRect by gnDATA_SEARCH_RECT_SIZE pixels.
	switch (m_eClueLocation)
	{
		case kBoxToTopLeft:
			{
				ipDataSearchRect->SetBounds(ipClueRect->Right, ipClueRect->Bottom,
											ipClueRect->Right + gnDATA_SEARCH_RECT_SIZE,
											ipClueRect->Bottom + gnDATA_SEARCH_RECT_SIZE);
			}
			break;

		case kBoxToTop:
			{
				ipDataSearchRect->SetBounds(ipClueRect->Left, ipClueRect->Bottom, ipClueRect->Right, 
											ipClueRect->Bottom + gnDATA_SEARCH_RECT_SIZE);
			}
			break;

		case kBoxToTopRight:
			{
				ipDataSearchRect->SetBounds(ipClueRect->Left - gnDATA_SEARCH_RECT_SIZE, 
											ipClueRect->Bottom, ipClueRect->Left, 
											ipClueRect->Bottom + gnDATA_SEARCH_RECT_SIZE);
			}
			break;

		case kBoxToRight:
			{
				ipDataSearchRect->SetBounds(ipClueRect->Left - gnDATA_SEARCH_RECT_SIZE, 
											ipClueRect->Top, ipClueRect->Left, ipClueRect->Bottom);
			}
			break;

		case kBoxToBottomRight:
			{
				ipDataSearchRect->SetBounds(ipClueRect->Left - gnDATA_SEARCH_RECT_SIZE, 
											ipClueRect->Top - gnDATA_SEARCH_RECT_SIZE,
											ipClueRect->Left, ipClueRect->Top);
			}
			break;

		case kBoxToBottom:
			{
				ipDataSearchRect->SetBounds(ipClueRect->Left, 
											ipClueRect->Top - gnDATA_SEARCH_RECT_SIZE,
											ipClueRect->Right, ipClueRect->Top);
			}
			break;

		case kBoxToBottomLeft:
			{
				ipDataSearchRect->SetBounds(ipClueRect->Right, 
											ipClueRect->Top - gnDATA_SEARCH_RECT_SIZE,
											ipClueRect->Right + gnDATA_SEARCH_RECT_SIZE, ipClueRect->Top);
			}
			break;

		case kBoxToLeft:
			{
				ipDataSearchRect->SetBounds(ipClueRect->Right, ipClueRect->Top,
											ipClueRect->Right + gnDATA_SEARCH_RECT_SIZE, 
											ipClueRect->Bottom);
			}
			break;

		default:
			{
				THROW_LOGIC_ERROR_EXCEPTION("ELI19840");
			}
	}

	return ipDataSearchRect;
}
//-------------------------------------------------------------------------------------------------
bool CBoxFinder::qualifyBox(ILongRectanglePtr ipRect, ISpatialPageInfoPtr ipPageInfo,
							  IIUnknownVectorPtr &ripExistingBoxes)
{
	ASSERT_ARGUMENT("ELI19801", ipRect != __nullptr);
	ASSERT_ARGUMENT("ELI19802", ipPageInfo != __nullptr);
	ASSERT_ARGUMENT("ELI19859", ripExistingBoxes != __nullptr);

	long nPageWidth(-1), nPageHeight(-1);
	EOrientation eOrientation;
	double dDeskew;
	ipPageInfo->GetPageInfo(&nPageWidth, &nPageHeight, &eOrientation, &dDeskew);

	long nRectLeft(-1), nRectTop(-1), nRectRight(-1), nRectBottom(-1);
	ipRect->GetBounds(&nRectLeft, &nRectTop, &nRectRight, &nRectBottom);

	bool bTextIsHorizontal = eOrientation == kRotNone ||											 
							 eOrientation == kRotDown ||
							 eOrientation == kRotFlipped ||
							 eOrientation == kRotFlippedDown;

	// Test the width of the box against specs if necessary
	if (isDoubleValueSpecified(m_dBoxWidthMin) || isDoubleValueSpecified(m_dBoxWidthMax))
	{
		double dPageWidth = (bTextIsHorizontal ? nPageWidth : nPageHeight);
		double dBoxWidth = nRectRight - nRectLeft;
		double dWidthPercent = 100.0 * dBoxWidth / dPageWidth;

		if (isDoubleValueSpecified(m_dBoxWidthMin) && dWidthPercent < m_dBoxWidthMin)
		{
			return false;
		}
		else if (isDoubleValueSpecified(m_dBoxWidthMax) && dWidthPercent > m_dBoxWidthMax)
		{
			return false;
		}
	}
	
	// Test the height of the box against specs if necessary
	if (isDoubleValueSpecified(m_dBoxHeightMin) || isDoubleValueSpecified(m_dBoxHeightMax))
	{
		double dPageHeight = (bTextIsHorizontal ? nPageHeight : nPageWidth);
		double dBoxHeight = nRectBottom - nRectTop;
		double dHeightPercent = 100.0 * dBoxHeight / dPageHeight;

		if (isDoubleValueSpecified(m_dBoxHeightMin) && dHeightPercent < m_dBoxHeightMin)
		{
			return false;
		}
		else if (isDoubleValueSpecified(m_dBoxHeightMax) && dHeightPercent > m_dBoxHeightMax)
		{
			return false;
		}
	}

	// Test to see if we have already found this box
	int nNumBoxes = ripExistingBoxes->Size();
	for (int i = 0; i < nNumBoxes; i++)
	{
		ILongRectanglePtr ipExistingBox = ripExistingBoxes->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI19860", ipExistingBox);

		long nBoxLeft(-1), nBoxTop(-1), nBoxRight(-1), nBoxBottom(-1);
		ipExistingBox->GetBounds(&nBoxLeft, &nBoxTop, &nBoxRight, &nBoxBottom);
		
		if (nBoxLeft == nRectLeft &&
			nBoxTop == nRectTop &&
			nBoxRight == nRectRight &&
			nBoxBottom == nRectBottom)
		{
			// We've already found this box
			return false;
		}
	}

	// Keep track of the fact that we found this box
	ripExistingBoxes->PushBack(ipRect);

	return true;
}
//-------------------------------------------------------------------------------------------------
ILongRectanglePtr CBoxFinder::excludeVerticalSpatialAreaOfClue(ILongRectanglePtr ipFoundBox, 
															   ILongRectanglePtr ipClueBounds)
{
	ASSERT_ARGUMENT("ELI19779", ipFoundBox != __nullptr);
	ASSERT_ARGUMENT("ELI19783", ipClueBounds != __nullptr);

	// Create a copy of the passed in rect to return as the result;
	ILongRectanglePtr ipResult = ipFoundBox->Clone();
	ASSERT_RESOURCE_ALLOCATION("ELI19991", ipResult != __nullptr);

	if (m_eClueLocation == kSameBox ||
		m_eClueLocation == kBoxToLeft || 
		m_eClueLocation == kBoxToRight)
	{
		// Exclusion of clue's vertical extent only appropriate when the clue
		// is in the same row as the target box

		CRect rectBox(ipFoundBox->Left, ipFoundBox->Top, ipFoundBox->Right, ipFoundBox->Bottom);
		CRect rectClue(ipClueBounds->Left, ipClueBounds->Top, ipClueBounds->Right, ipClueBounds->Bottom);

		// If the clue is to the top side of the data
		if (rectClue.CenterPoint().y <= rectBox.CenterPoint().y)
		{
			if (rectClue.bottom >= rectBox.bottom)
			{
				// The clue area excludes the entire result.
				return NULL;
			}
			else
			{
				ipResult->Top = rectClue.bottom;
			}
		}
		// If the clue is to the bottom side of the data
		else
		{
			if (rectClue.top <= rectBox.top)
			{
				// The clue area excludes the entire result.
				return NULL;
			}
			else
			{
				ipResult->Bottom = rectClue.top;
			}
		}
	}

	return ipResult;
}
//-------------------------------------------------------------------------------------------------
IAttributePtr CBoxFinder::createRegionResult(ISpatialStringPtr ipDocText, int nPageNum,
												ILongRectanglePtr ipRect)
{
	ASSERT_ARGUMENT("ELI19773", ipDocText != __nullptr);
	ASSERT_ARGUMENT("ELI19774", ipRect != __nullptr);
	ASSERT_ARGUMENT("ELI19776", nPageNum >= 1);

	// Create the raster zone
	IRasterZonePtr	ipZone(CLSID_RasterZone);
	ASSERT_RESOURCE_ALLOCATION("ELI19771", ipZone != __nullptr);

	ipZone->CreateFromLongRectangle(ipRect, nPageNum);

	// Create the spatial string
	ISpatialStringPtr ipSpatialString(CLSID_SpatialString);
	ASSERT_RESOURCE_ALLOCATION("ELI19770", ipSpatialString);

	// Build a spatial string (in spatial mode) that occupies the full extent of ipZone with
	// the letters of m_strAttributeText spread evenly across it.
	ipSpatialString->CreatePseudoSpatialString(ipZone, m_strAttributeText.c_str(),
		ipDocText->SourceDocName, ipDocText->SpatialPageInfos);

	// Create the attribute
	IAttributePtr ipAttribute(CLSID_Attribute);
	ASSERT_RESOURCE_ALLOCATION("ELI19767", ipAttribute != __nullptr );

	ipAttribute->Value = ipSpatialString;

	return ipAttribute;
}
//-------------------------------------------------------------------------------------------------
IAttributePtr CBoxFinder::createTextResult(IAFDocumentPtr ipAFDoc, ILongRectanglePtr ipBox, 
										   ISpatialStringPtr ipClue, IRegularExprParserPtr ipParser)
{
	ASSERT_ARGUMENT("ELI20237", ipAFDoc != __nullptr);
	ASSERT_ARGUMENT("ELI20226", ipBox != __nullptr);
	ASSERT_ARGUMENT("ELI20227", ipClue != __nullptr);

	// Return the spatial string text found within the box
	ISpatialStringPtr ipText =  getSpatialStringSearcher()->GetDataInRegion(ipBox, VARIANT_FALSE);
	ASSERT_RESOURCE_ALLOCATION("ELI19785", ipText != __nullptr);

	// Expand clue list to replace any file names with the clues from the specified file
	IVariantVectorPtr ipExpandedClues = getExpandedClueList(ipAFDoc);

	if (m_bIncludeClueText == true && m_eClueLocation != kSameBox)
	{
		// If the user has requested for the clue text to be included, but the
		// clue is in a different box, add the clue to the found text
		if (ipText->Size == 0)
		{
			ipText = ipClue;
		}
		else
		{
			if (m_eClueLocation == kBoxToTopLeft ||
				m_eClueLocation == kBoxToTop ||
				m_eClueLocation == kBoxToTopRight ||
				m_eClueLocation == kBoxToLeft)
			{
				ipText->Insert(0, ipClue);
			}
			else
			{
				ipText->Append(ipClue);
			}
		}
	}
	else if (m_bIncludeClueText == false && m_eClueLocation == kSameBox)
	{
		// If the user has requested not to include the clue in the result, but the clue is in the
		// same box as the desired data, the should be removed from the found text.
		long nStart = -1;
		long nEnd = -1;

		// Get the bounds of each spatial character in the clue.
		ILongRectanglePtr ipClueBounds = ipClue->GetOCRImageBounds();
		ASSERT_RESOURCE_ALLOCATION("ELI34235", ipClueBounds != __nullptr);

		CRect clueBounds;
		ipClueBounds->GetBounds(&(clueBounds.left), &(clueBounds.top),
			&(clueBounds.right), &(clueBounds.bottom));

		vector<CRect> vecClueLetterRects;
		long nClueLength = ipClue->Size;
		for (long i = 0; i < nClueLength; i++)
		{
			CRect rectLetter = getLetterRect(ipClue, i);
			if (!asCppBool(rectLetter.IsRectNull()))
			{
				vecClueLetterRects.push_back(rectLetter);
			}
		}

		// Loop through each character in the found text to find any that are spatially identical to
		// characters in the clue, which should be removed.
		long nTextLength = ipText->Size;
		for (long i = 0; i < nTextLength; i++)
		{
			CRect rectLetter = getLetterRect(ipText, i);
			if (!asCppBool(rectLetter.IsRectNull()))
			{
				for each (CRect rectClueLetter in vecClueLetterRects)
				{
					if (rectLetter == rectClueLetter)
					{
						// This character is a part of the clue. Remove it.
						ipText->Remove(i, i);
						if (!asCppBool(ipText->HasSpatialInfo()))
						{
							// If there is nothing left in the box text except non-spatial text,
							// consider all of text part of the clue and clear it out.
							ipText->ReplaceAndDowngradeToNonSpatial("");
							nTextLength = 0;
						}
						else
						{
							i--;
							nTextLength--;
						}

						break;
					}
				}
			}
		}
	}

	// Remove excess whitespace
	ipText->Trim(" \r\n", " \r\n" );

	// Assign the resulting spatial string to the resulting attribute.
	IAttributePtr ipResult(CLSID_Attribute);
	ASSERT_RESOURCE_ALLOCATION("ELI19786", ipResult != __nullptr);

	ipResult->Value = ipText;

	return ipResult;
}
//-------------------------------------------------------------------------------------------------
IVariantVectorPtr CBoxFinder::getExpandedClueList(IAFDocumentPtr ipAFDoc)
{
	ASSERT_ARGUMENT("ELI20220", ipAFDoc != __nullptr);

	// Get a list of values that includes values from any specified files.
	IVariantVectorPtr ipExpandedList = m_cachedListLoader.expandList(m_ipClues, ipAFDoc);
	ASSERT_RESOURCE_ALLOCATION("ELI20217", ipExpandedList != __nullptr);

	return ipExpandedList;
}
//-------------------------------------------------------------------------------------------------
vector<int> CBoxFinder::getPagesToSearch(ISpatialStringPtr ipDocText)
{
	ASSERT_ARGUMENT("ELI20225", ipDocText != __nullptr);

	vector<int> vecPages;

	// [FlexIDSCore:3069] Return immediately if the document does not have spatial information
	if (ipDocText->GetMode() == kNonSpatialMode)
	{
		return vecPages;
	}

	long nLastPageNumber = ipDocText->GetLastPageNumber();

	if (m_ePageSelectionMode == kAllPages)
	{
		// Include all pages
		for(int i = 1; i <= nLastPageNumber; i++)
		{
			vecPages.push_back(i);
		}
	}
	else if (m_ePageSelectionMode == kFirstPages)
	{
		// Include the first [m_nNumFirstPages] pages
		for(int i = 1; i <= m_nNumFirstPages && i <= nLastPageNumber; i++)
		{
			vecPages.push_back(i);
		}
	}
	else if (m_ePageSelectionMode == kLastPages)
	{
		// Include the first [m_nNumLastPages] pages
		int nFirst = nLastPageNumber - m_nNumLastPages + 1;
		if (nFirst < 1)
		{
			nFirst = 1;
		}

		for(int i = nFirst; i <= nLastPageNumber; i++)
		{
			vecPages.push_back(i);
		}
	}
	else if (m_ePageSelectionMode == kSpecifiedPages)
	{
		// Include [m_strSpecifiedPages] pages
		vecPages = getPageNumbers(nLastPageNumber, m_strSpecifiedPages);
	}

	return vecPages;
}
//-------------------------------------------------------------------------------------------------
CRect CBoxFinder::getLetterRect(ISpatialStringPtr ipString, int nIndex)
{
	ILetterPtr ipLetter;
	if (ipString->GetNextOCRImageSpatialLetter(nIndex, &ipLetter) != nIndex)
	{
		// If the character at the specified index is not spatial, return an empty rect.
		return CRect(); 
	}
	ASSERT_RESOURCE_ALLOCATION("ELI34236", ipLetter != __nullptr);

	CPPLetter cppLetter;
	ILongRectanglePtr ipCharBounds = ipLetter->GetCppLetter(&cppLetter);

	return CRect(cppLetter.m_ulLeft, cppLetter.m_ulTop, cppLetter.m_ulRight,
		cppLetter.m_ulBottom);
}
//-------------------------------------------------------------------------------------------------
void CBoxFinder::resetDataMembers()
{
	m_ipClues						= NULL;
	m_ipImageLineUtility			= NULL;
	m_bIncludeLines					= false;
	m_bCluesAreRegularExpressions	= false;
	m_bCluesAreCaseSensitive		= false;
	m_bFirstBoxOnly					= false;
	m_eClueLocation					= kSameBox;
	m_ePageSelectionMode			= kAllPages;
	m_nNumFirstPages				= 0;
	m_nNumLastPages					= 0;
	m_strSpecifiedPages				= "";
	m_dBoxWidthMin					= gdUNSPECIFIED;
	m_dBoxWidthMax					= gdUNSPECIFIED;
	m_dBoxHeightMin					= gdUNSPECIFIED;
	m_dBoxHeightMax					= gdUNSPECIFIED;
	m_eFindType						= kImageRegion;
	m_strAttributeText				= gstrDEFAULT_ATTRIBUTE_TEXT;
	m_bExcludeClueArea				= false;
	m_bIncludeClueText				= false;
	m_bIncludeLines					= false;
}
//-------------------------------------------------------------------------------------------------
void CBoxFinder::validateLicense()
{
	VALIDATE_LICENSE(gnFLEXINDEX_IDSHIELD_CORE_OBJECTS, "ELI19264", "Box Finding Rule");
}
//-------------------------------------------------------------------------------------------------