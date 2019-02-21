// ImageLineUtility.cpp : Implementation of CImageLineUtility

#include "stdafx.h"
#include "ImageLineUtility.h"

#include <l_bitmap.h>		// LeadTools Imaging library
#include <MiscLeadUtils.h>
#include <LeadToolsBitmapFreeer.h>

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>
#include <LeadToolsLicenseRestrictor.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 1;

// Distance defaults in terms of percentage of page
const long gnUNSPECIFIED = -1;
const long gnDEFAULT_COLUMN_WIDTH_MIN = 10;
const long gnDEFAULT_COLUMN_WIDTH_MAX = gnUNSPECIFIED;
const long gnDEFAULT_OVERALL_WIDTH_MIN = gnUNSPECIFIED;
const long gnDEFAULT_OVERALL_WIDTH_MAX = gnUNSPECIFIED;
const long gnDEFAULT_LINE_SPACING_MIN = 1;
const long gnDEFAULT_LINE_SPACING_MAX = 5;
const long gnDEFAULT_COLUMN_SPACING_MAX = 20;
// Distance defaults in terms of thousandths of an inch
const long gnDEFAULT_BRIDGE_GAP = 350;
const long gnMINIMUM_DIMENSION_LOWER_LIMIT = 0;
const long gnMINIMUM_DIMENSION_UPPER_LIMIT = 99;
const long gnMAXIMUM_DIMENSION_LOWER_LIMIT = 0;
const long gnMAXIMUM_DIMENSION_UPPER_LIMIT = 100;

// Minimum hundredths of a degree of rotation requested for rotation to be applied.
const L_INT gnMIN_ROTATION = 10;

//-------------------------------------------------------------------------------------------------
// CImageLineUtility
//-------------------------------------------------------------------------------------------------
CImageLineUtility::CImageLineUtility() :
	m_nColumnWidthMin(gnDEFAULT_COLUMN_WIDTH_MIN),
	m_nColumnWidthMax(gnDEFAULT_COLUMN_WIDTH_MAX),
	m_nOverallWidthMin(gnDEFAULT_OVERALL_WIDTH_MIN),
	m_nOverallWidthMax(gnDEFAULT_OVERALL_WIDTH_MAX),
	m_nLineSpacingMin(gnDEFAULT_LINE_SPACING_MIN),
	m_nLineSpacingMax(gnDEFAULT_LINE_SPACING_MAX),
	m_nColumnSpacingMax(gnDEFAULT_COLUMN_SPACING_MAX),
	m_nBridgeGapSmallerThan(gnDEFAULT_BRIDGE_GAP)
{
	try
	{
		// If PDF support is licensed initialize support
		// NOTE: no exception is thrown or logged if PDF support is not licensed.
		initPDFSupport();
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI20619");
}
//--------------------------------------------------------------------------------------------------
CImageLineUtility::~CImageLineUtility()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI18796");
}
//--------------------------------------------------------------------------------------------------
HRESULT CImageLineUtility::FinalConstruct()
{
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
void CImageLineUtility::FinalRelease()
{
}

//--------------------------------------------------------------------------------------------------
// IImageLineUtility
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::FindLines(BSTR bstrImageFileName, long nPageNum, double dRotation,
										  IIUnknownVector **ppHorzLineRects, IIUnknownVector **ppVertLineRects)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		string strImageFileName = asString(bstrImageFileName);
		ASSERT_ARGUMENT("ELI19017", !strImageFileName.empty());
		ASSERT_ARGUMENT("ELI19019", nPageNum >= 0);

		validateLicense();

		// Create LineRect vectors to receive the lines
		vector<LineRect> vecHorzLineRects;
		vector<LineRect> vecVertLineRects;

		// Process the page
		findLines(strImageFileName, nPageNum, dRotation,
				  (ppHorzLineRects != __nullptr) ? &vecHorzLineRects : NULL,
				  (ppVertLineRects != __nullptr) ? &vecVertLineRects : NULL);

		// If horizontal lines were requested, copy the resulting vector to ppHorzLineRects
		if (ppHorzLineRects != __nullptr)
		{
			IIUnknownVectorPtr ipHorzLineRects = lineRectVecToILongRectangleVec(vecHorzLineRects);
			*ppHorzLineRects = ipHorzLineRects.Detach();
		}

		// If vertical lines were requested, copy the resulting vector to ppVertLineRects
		if (ppVertLineRects != __nullptr)
		{
			IIUnknownVectorPtr ipVertLineRects = lineRectVecToILongRectangleVec(vecVertLineRects);
			*ppVertLineRects = ipVertLineRects.Detach();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18408")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::FindLineRegions(BSTR bstrImageFileName, long nPageNum, 
											double dRotation, VARIANT_BOOL bHorizontal, 
											IIUnknownVector **ppSubLineRects, 
											IIUnknownVector **ppGroupRects)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		string strImageFileName = asString(bstrImageFileName);
		ASSERT_ARGUMENT("ELI19021", !strImageFileName.empty());
		ASSERT_ARGUMENT("ELI19022", nPageNum >= 0);
		ASSERT_ARGUMENT("ELI18708", ppGroupRects != __nullptr);

		validateLicense();

		vector<LineRect> vecLineRects;
		CRect rectPageBounds;

		if (asCppBool(bHorizontal))
		{
			findLines(strImageFileName, nPageNum, dRotation, &vecLineRects, NULL, &rectPageBounds);
		}
		else
		{
			findLines(strImageFileName, nPageNum, dRotation, NULL, &vecLineRects, &rectPageBounds);
		}

		// Tell LineGrouper whether it should group horizontally or vertically
		m_LineGrouper.m_Settings.m_bHorizontal = asCppBool(bHorizontal);

		// Line grouping is requested
		vector<LineRect> vecGroupRects;

		if (ppSubLineRects != __nullptr)
		{
			// Lines are requested with the groups
			vector< vector<LineRect> > vecSubLineRects;

			// Group the lines
			if (m_LineGrouper.groupLines(vecLineRects, vecGroupRects, 
										 rectPageBounds, &vecSubLineRects) == false)
			{
				UCLIDException ue("ELI21526", "Application trace: Line image region search aborted! "
											  "Too many potential line groupings found; results may "
											  "not be accurate.");
				ue.addDebugInfo("Filename", strImageFileName);
				ue.addDebugInfo("Page", nPageNum);
				ue.log();
			}

			// Create a vector to store the lines
			IIUnknownVectorPtr ipSubLineRects(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI18712", ipSubLineRects != __nullptr);

			for each (vector<LineRect> vecLineGroup in vecSubLineRects)
			{
				// Create and populate an IIUnknownVector of ILongRectangles with the 
				// line rects that comprise each line area
				ipSubLineRects->PushBack(lineRectVecToILongRectangleVec(vecLineGroup));
			}

			// Now create and populate an IIUnknownVector of ILongRectangles that contains
			// all the lines in the document
			ipSubLineRects->PushBack(lineRectVecToILongRectangleVec(vecLineRects));

			// Return the collections of lines
			*ppSubLineRects = ipSubLineRects.Detach();
		}
		else
		{
			// No sub-lines are requested. Group the lines.
			if (m_LineGrouper.groupLines(vecLineRects, vecGroupRects, rectPageBounds) == false)
			{
				UCLIDException ue("ELI21527", "Application trace: Line image region search aborted. "
											  "Too many potential line groupings found; results may "
											  "not be accurate.");
				ue.addDebugInfo("Filename", strImageFileName);
				ue.addDebugInfo("Page", nPageNum);
				ue.log();
			}
		}

		// Populate ipRects with ILongRectangles representing the image areas 
		IIUnknownVectorPtr ipGroupRects = lineRectVecToILongRectangleVec(vecGroupRects);

		*ppGroupRects = ipGroupRects.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19846")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::FindBoxContainingRect(ILongRectangle *pRect, 
													  IIUnknownVector *pHorzLineRects, 
													  IIUnknownVector *pVertLineRects, 
													  long nRequiredMatchingBoundaries,
												      VARIANT_BOOL *pbIncompleteResult,
													  ILongRectangle **ppBoxRect)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Validate arguments
		ILongRectanglePtr ipRect(pRect);
		ASSERT_ARGUMENT("ELI19822", ipRect != __nullptr);
		IIUnknownVectorPtr ipHorzLineRects(pHorzLineRects);
		ASSERT_ARGUMENT("ELI19823", ipHorzLineRects != __nullptr);
		IIUnknownVectorPtr ipVertLineRects(pVertLineRects);
		ASSERT_ARGUMENT("ELI19824", ipVertLineRects != __nullptr);
		ASSERT_ARGUMENT("ELI19825", ppBoxRect != __nullptr);
		ASSERT_ARGUMENT("ELI21529", pbIncompleteResult != __nullptr);

		validateLicense();

		// Copy pHorzLineRects to a vector<LineRect> 
		vector<LineRect> vecHorzLineRects;
		longRectangleVecToLineRectVec(ipHorzLineRects, true, vecHorzLineRects);

		// Copy pVertLineRects to a vector<LineRect>
		vector<LineRect> vecVertLineRects;
		longRectangleVecToLineRectVec(ipVertLineRects, false, vecVertLineRects);

		// Create a LineRect to represent the targeted image area
		LineRect rectBox(CRect(ipRect->Left, ipRect->Top, ipRect->Right, ipRect->Bottom), true);

		// Find the box
		bool bIncompleteResult;
		LineRect rectResult(true);
		if (m_LineGrouper.findBoxContainingRect(rectBox, vecHorzLineRects, vecVertLineRects, 
			nRequiredMatchingBoundaries, rectResult, bIncompleteResult))
		{
			// A qualifying box was found... copy the result to the ppBoxRect return value
			ILongRectanglePtr ipReturnVal(CLSID_LongRectangle);
			ASSERT_RESOURCE_ALLOCATION("ELI19828", ipReturnVal != __nullptr);

			ipReturnVal->SetBounds(rectResult.left, rectResult.top, rectResult.right, rectResult.bottom);

			*ppBoxRect = ipReturnVal.Detach();
		}
		else
		{
			// Could not find a qualifying box.  Return NULL.
			*ppBoxRect = NULL;
		}

		*pbIncompleteResult = asVariantBool(bIncompleteResult);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19826")

}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::get_LineLengthMin(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18825", pVal != __nullptr);

		// Check license
		validateLicense();

		*pVal = m_LineFinder.m_lr.iMinLineLength;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18826");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::put_LineLengthMin(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18961", newVal >= 0);

		// Check license
		validateLicense();

		m_LineFinder.m_lr.iMinLineLength = newVal;
		
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19498");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::get_LineThicknessMax(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18836", pVal != __nullptr);

		// Check license
		validateLicense();
		
		*pVal = m_LineFinder.m_lr.iMaxLineWidth;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18838");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::put_LineThicknessMax(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18962", newVal >= 0);

		// Check license
		validateLicense();

		m_LineFinder.m_lr.iMaxLineWidth = newVal;
		
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18839");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::get_LineGapMax(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18847", pVal != __nullptr);

		// Check license
		validateLicense();
		
		*pVal = m_LineFinder.m_lr.iGapLength;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18848");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::put_LineGapMax(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18963", newVal >= 0);

		// Check license
		validateLicense();

		m_LineFinder.m_lr.iGapLength = newVal;
		
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18849");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::get_LineVarianceMax(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18850", pVal != __nullptr);

		// Check license
		validateLicense();
		
		*pVal = m_LineFinder.m_lr.iVariance;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18851");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::put_LineVarianceMax(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18966", newVal >= 0);

		// Check license
		validateLicense();

		m_LineFinder.m_lr.iVariance = newVal;
		
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18852");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::get_LineWall(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18853", pVal != __nullptr);

		// Check license
		validateLicense();
		
		*pVal = m_LineFinder.m_lr.iWall;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18854");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::put_LineWall(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18964", newVal >= 0);

		// Check license
		validateLicense();

		m_LineFinder.m_lr.iWall = newVal;
		
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18855");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::get_LineWallPercentMax(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18856", pVal != __nullptr);

		// Check license
		validateLicense();
		
		*pVal = m_LineFinder.m_lr.iMaxWallPercent;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18857");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::put_LineWallPercentMax(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18965", newVal >= 0 && newVal <= 100);

		// Check license
		validateLicense();

		m_LineFinder.m_lr.iMaxWallPercent = newVal;
		
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18858");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::get_LineBridgeGap(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18859", pVal != __nullptr);

		// Check license
		validateLicense();
		
		*pVal = m_nBridgeGapSmallerThan;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18860");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::put_LineBridgeGap(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18967", newVal >= 0);

		// Check license
		validateLicense();

		m_nBridgeGapSmallerThan = newVal;
		
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18861");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::get_ExtendLineFragments(VARIANT_BOOL *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18862", pVal != __nullptr);

		// Check license
		validateLicense();
		
		*pVal = asVariantBool(m_LineFinder.m_bExtendLineFragments);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18863");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::put_ExtendLineFragments(VARIANT_BOOL newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check license
		validateLicense();

		m_LineFinder.m_bExtendLineFragments = asCppBool(newVal);
		
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18864");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::get_ExtensionScanWidth(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18865", pVal != __nullptr);

		// Check license
		validateLicense();
		
		*pVal = m_LineFinder.m_nExtensionScanLines;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18866");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::put_ExtensionScanWidth(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18968", newVal >= 0);

		// Check license
		validateLicense();

		// Ensure that new setting is always odd numbered
		if (newVal % 2 != 1)
		{
			newVal ++;
		}

		m_LineFinder.m_nExtensionScanLines = newVal;
		
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18867");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::get_ExtensionTelescoping(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18868", pVal != __nullptr);

		// Check license
		validateLicense();
		
		*pVal = m_LineFinder.m_nExtensionTelescoping;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18869");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::put_ExtensionTelescoping(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18969", 
			newVal >= gnMAXIMUM_DIMENSION_LOWER_LIMIT && newVal <= gnMAXIMUM_DIMENSION_UPPER_LIMIT);

		// Check license
		validateLicense();

		m_LineFinder.m_nExtensionTelescoping = newVal;
		
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18870");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::get_ExtensionGapAllowance(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18871", pVal != __nullptr);

		// Check license
		validateLicense();
		
		*pVal = m_LineFinder.m_nExtensionGap;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18872");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::put_ExtensionGapAllowance(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18970", newVal >= 0);

		// Check license
		validateLicense();

		m_LineFinder.m_nExtensionGap = newVal;
		
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18873");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::get_ExtensionConsecutiveMin(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18874", pVal != __nullptr);

		// Check license
		validateLicense();
		
		*pVal = m_LineFinder.m_nExtensionConsecutiveMinimum;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18875");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::put_ExtensionConsecutiveMin(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18971", newVal >= 0);

		// Check license
		validateLicense();

		m_LineFinder.m_nExtensionConsecutiveMinimum = newVal;
		
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18876");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::get_RowCountMin(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18877", pVal != __nullptr);

		// Check license
		validateLicense();
		
		*pVal = m_LineGrouper.m_Settings.m_nLineCountMin;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18878");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::put_RowCountMin(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18972", newVal > 1);

		// Check license
		validateLicense();

		m_LineGrouper.m_Settings.m_nLineCountMin = newVal;
		
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18879");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::get_RowCountMax(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18880", pVal != __nullptr);

		// Check license
		validateLicense();
		
		*pVal = m_LineGrouper.m_Settings.m_nLineCountMax;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18881");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::put_RowCountMax(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18973", newVal == gnUNSPECIFIED || newVal > 1);

		// Check license
		validateLicense();

		m_LineGrouper.m_Settings.m_nLineCountMax = newVal;
		
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18882");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::get_ColumnCountMin(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18883", pVal != __nullptr);

		// Check license
		validateLicense();
		
		*pVal = m_LineGrouper.m_Settings.m_nColumnCountMin;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18884");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::put_ColumnCountMin(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18974", newVal == gnUNSPECIFIED || newVal > 0);

		// Check license
		validateLicense();

		m_LineGrouper.m_Settings.m_nColumnCountMin = newVal;
		
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18885");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::get_ColumnCountMax(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18886", pVal != __nullptr);

		// Check license
		validateLicense();
		
		*pVal = m_LineGrouper.m_Settings.m_nColumnCountMax;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18887");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::put_ColumnCountMax(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18975", newVal == gnUNSPECIFIED || newVal > 0);

		// Check license
		validateLicense();

		m_LineGrouper.m_Settings.m_nColumnCountMax = newVal;
		
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18888");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::get_ColumnWidthMin(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18988", pVal != __nullptr);

		// Check license
		validateLicense();

		*pVal = m_nColumnWidthMin;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18989");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::put_ColumnWidthMin(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// verify that the new value is either gnUNSPECIFIED or a valid percentage
		ASSERT_ARGUMENT("ELI18990", newVal == gnUNSPECIFIED || 
			(newVal >= gnMINIMUM_DIMENSION_LOWER_LIMIT && newVal <= gnMINIMUM_DIMENSION_UPPER_LIMIT));

		// Check license
		validateLicense();

		m_nColumnWidthMin = newVal;
		
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18991");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::get_ColumnWidthMax(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18827", pVal != __nullptr);

		// Check license
		validateLicense();

		*pVal = m_nColumnWidthMax;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18828");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::put_ColumnWidthMax(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// verify that the new value is either gnUNSPECIFIED or a valid percentage
		ASSERT_ARGUMENT("ELI18976", newVal == gnUNSPECIFIED || 
			(newVal >= gnMAXIMUM_DIMENSION_LOWER_LIMIT && newVal <= gnMAXIMUM_DIMENSION_UPPER_LIMIT));

		// Check license
		validateLicense();

		m_nColumnWidthMax = newVal;
		
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18829");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::get_OverallWidthMin(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18889", pVal != __nullptr);

		// Check license
		validateLicense();
		
		*pVal = m_nOverallWidthMin;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18890");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::put_OverallWidthMin(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// verify that the new value is either gnUNSPECIFIED or a valid percentage
		ASSERT_ARGUMENT("ELI18977", newVal == gnUNSPECIFIED || 
			(newVal >= gnMINIMUM_DIMENSION_LOWER_LIMIT && newVal <= gnMINIMUM_DIMENSION_UPPER_LIMIT));

		// Check license
		validateLicense();

		m_nOverallWidthMin = newVal;
		
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18891");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::get_OverallWidthMax(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18892", pVal != __nullptr);

		// Check license
		validateLicense();
		
		*pVal = m_nOverallWidthMax;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18893");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::put_OverallWidthMax(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// verify that the new value is either gnUNSPECIFIED or a valid percentage
		ASSERT_ARGUMENT("ELI18978", newVal == gnUNSPECIFIED || 
			(newVal >= gnMAXIMUM_DIMENSION_LOWER_LIMIT && newVal <= gnMAXIMUM_DIMENSION_UPPER_LIMIT));

		// Check license
		validateLicense();

		m_nOverallWidthMax = newVal;
		
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18894");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::get_LineSpacingMin(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18895", pVal != __nullptr);

		// Check license
		validateLicense();
		
		*pVal = m_nLineSpacingMin;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18896");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::put_LineSpacingMin(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// verify that the new value is either gnUNSPECIFIED or a valid percentage
		ASSERT_ARGUMENT("ELI18979", newVal == gnUNSPECIFIED || 
			(newVal >= gnMINIMUM_DIMENSION_LOWER_LIMIT && newVal <= gnMINIMUM_DIMENSION_UPPER_LIMIT));

		// Check license
		validateLicense();

		m_nLineSpacingMin = newVal;
		
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18897");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::get_LineSpacingMax(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18898", pVal != __nullptr);

		// Check license
		validateLicense();
		
		*pVal = m_nLineSpacingMax;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18899");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::put_LineSpacingMax(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// verify that the new value is either gnUNSPECIFIED or a valid percentage
		ASSERT_ARGUMENT("ELI18980", newVal == gnUNSPECIFIED || 
			(newVal >= gnMAXIMUM_DIMENSION_LOWER_LIMIT && newVal <= gnMAXIMUM_DIMENSION_UPPER_LIMIT));

		// Check license
		validateLicense();

		m_nLineSpacingMax = newVal;
		
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18900");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::get_ColumnSpacingMax(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18901", pVal != __nullptr);

		// Check license
		validateLicense();
		
		*pVal = m_nColumnSpacingMax;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18902");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::put_ColumnSpacingMax(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// verify that the new value is either gnUNSPECIFIED or a valid percentage
		ASSERT_ARGUMENT("ELI18981", newVal == gnUNSPECIFIED || 
			(newVal >= gnMAXIMUM_DIMENSION_LOWER_LIMIT && newVal <= gnMAXIMUM_DIMENSION_UPPER_LIMIT));

		// Check license
		validateLicense();

		m_nColumnSpacingMax = newVal;
		
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18903");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::get_AlignmentScoreMin(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18904", pVal != __nullptr);

		// Check license
		validateLicense();
		
		*pVal = m_LineGrouper.m_Settings.m_nAlignmentScoreMin;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18905");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::put_AlignmentScoreMin(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18982", 
			newVal >= gnMINIMUM_DIMENSION_LOWER_LIMIT && newVal <= gnMINIMUM_DIMENSION_UPPER_LIMIT);

		// Check license
		validateLicense();

		m_LineGrouper.m_Settings.m_nAlignmentScoreMin = newVal;
		
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18906");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::get_AlignmentScoreExact(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18907", pVal != __nullptr);

		// Check license
		validateLicense();
		
		*pVal = m_LineGrouper.m_Settings.m_nAlignmentScoreExact;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18908");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::put_AlignmentScoreExact(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18983", 
			newVal >= gnMAXIMUM_DIMENSION_LOWER_LIMIT && newVal <= gnMAXIMUM_DIMENSION_UPPER_LIMIT);

		// Check license
		validateLicense();

		m_LineGrouper.m_Settings.m_nAlignmentScoreExact = newVal;
		
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18909");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::get_SpacingScoreMin(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18910", pVal != __nullptr);

		// Check license
		validateLicense();
		
		*pVal = m_LineGrouper.m_Settings.m_nSpacingScoreMin;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18911");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::put_SpacingScoreMin(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18984", 
			newVal >= gnMINIMUM_DIMENSION_LOWER_LIMIT && newVal <= gnMINIMUM_DIMENSION_UPPER_LIMIT);

		// Check license
		validateLicense();

		m_LineGrouper.m_Settings.m_nSpacingScoreMin = newVal;
		
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18912");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::get_SpacingScoreExact(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18913", pVal != __nullptr);

		// Check license
		validateLicense();
		
		*pVal = m_LineGrouper.m_Settings.m_nSpacingScoreExact;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18914");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::put_SpacingScoreExact(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18985", 
			newVal >= gnMAXIMUM_DIMENSION_LOWER_LIMIT && newVal <= gnMAXIMUM_DIMENSION_UPPER_LIMIT);

		// Check license
		validateLicense();

		m_LineGrouper.m_Settings.m_nSpacingScoreExact = newVal;
		
		m_bDirty = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18915");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::raw_CopyFrom(IUnknown *pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license
		validateLicense();

		UCLID_IMAGEUTILSLib::IImageLineUtilityPtr ipCopyThis = pObject;
		ASSERT_ARGUMENT("ELI18797", ipCopyThis != __nullptr);

		// copy member variables
		m_nColumnWidthMin		= ipCopyThis->ColumnWidthMin;
		m_nColumnWidthMax		= ipCopyThis->ColumnWidthMax;
		m_nOverallWidthMin		= ipCopyThis->OverallWidthMin;
		m_nOverallWidthMax		= ipCopyThis->OverallWidthMax;
		m_nLineSpacingMin		= ipCopyThis->LineSpacingMin;
		m_nLineSpacingMax		= ipCopyThis->LineSpacingMax;
		m_nColumnSpacingMax		= ipCopyThis->ColumnSpacingMax;
		m_nBridgeGapSmallerThan	= ipCopyThis->LineBridgeGap;

		// Re-initialize LineFinder
		m_LineFinder = LeadToolsLineFinder();

		// Copy LineFinder settings
		m_LineFinder.m_lr.iMinLineLength			= ipCopyThis->LineLengthMin;
		m_LineFinder.m_lr.iMaxLineWidth				= ipCopyThis->LineThicknessMax;
		m_LineFinder.m_lr.iGapLength				= ipCopyThis->LineGapMax;
		m_LineFinder.m_lr.iVariance					= ipCopyThis->LineVarianceMax;
		m_LineFinder.m_lr.iWall						= ipCopyThis->LineWall;
		m_LineFinder.m_lr.iMaxWallPercent			= ipCopyThis->LineWallPercentMax;
		m_LineFinder.m_bExtendLineFragments			= asCppBool(ipCopyThis->ExtendLineFragments);
		m_LineFinder.m_nExtensionScanLines			= ipCopyThis->ExtensionScanWidth;
		m_LineFinder.m_nExtensionTelescoping		= ipCopyThis->ExtensionTelescoping;
		m_LineFinder.m_nExtensionGap				= ipCopyThis->ExtensionGapAllowance;
		m_LineFinder.m_nExtensionConsecutiveMinimum	= ipCopyThis->ExtensionConsecutiveMin;

		// Re-initialize LineGrouper
		m_LineGrouper = LeadToolsLineGroup();

		// Copy LineGrouper settings
		m_LineGrouper.m_Settings.m_nLineCountMin		= ipCopyThis->RowCountMin;
		m_LineGrouper.m_Settings.m_nLineCountMax		= ipCopyThis->RowCountMax;
		m_LineGrouper.m_Settings.m_nColumnCountMin		= ipCopyThis->ColumnCountMin;
		m_LineGrouper.m_Settings.m_nColumnCountMax		= ipCopyThis->ColumnCountMax;
		m_LineGrouper.m_Settings.m_nAlignmentScoreMin	= ipCopyThis->AlignmentScoreMin;
		m_LineGrouper.m_Settings.m_nAlignmentScoreExact	= ipCopyThis->AlignmentScoreExact;
		m_LineGrouper.m_Settings.m_nSpacingScoreMin		= ipCopyThis->SpacingScoreMin;
		m_LineGrouper.m_Settings.m_nSpacingScoreExact	= ipCopyThis->SpacingScoreExact;
		m_LineGrouper.m_Settings.m_nSpacingMin			= ipCopyThis->LineSpacingMin;
		m_LineGrouper.m_Settings.m_nSpacingMax			= ipCopyThis->LineSpacingMax;
		m_LineGrouper.m_Settings.m_nColumnSpacingMax	= ipCopyThis->ColumnSpacingMax;
		m_LineGrouper.m_Settings.m_nColumnWidthMin		= ipCopyThis->ColumnWidthMin;
		m_LineGrouper.m_Settings.m_nColumnWidthMax		= ipCopyThis->ColumnWidthMax;
		m_LineGrouper.m_Settings.m_nOverallWidthMin		= ipCopyThis->OverallWidthMin;
		m_LineGrouper.m_Settings.m_nOverallWidthMax		= ipCopyThis->OverallWidthMax;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18798");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::raw_Clone(IUnknown **pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate license
		validateLicense();

		ASSERT_ARGUMENT("ELI18799", pObject != __nullptr);

		// Create another instance of this object
		ICopyableObjectPtr ipObjCopy(CLSID_ImageLineUtility);
		ASSERT_RESOURCE_ALLOCATION("ELI18800", ipObjCopy != __nullptr);

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);
	
		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18801");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPersistStream
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::GetClassID(CLSID *pClassID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI18802", pClassID != __nullptr);

		*pClassID = CLSID_ImageLineUtility;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18803");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::IsDirty(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		return m_bDirty ? S_OK : S_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18804");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::Load(IStream *pStream)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		ASSERT_ARGUMENT("ELI18805", pStream != __nullptr);

		// reset member variables
		m_nColumnWidthMin		= gnDEFAULT_COLUMN_WIDTH_MIN;
		m_nColumnWidthMax		= gnDEFAULT_COLUMN_WIDTH_MAX;
		m_nOverallWidthMin		= gnDEFAULT_OVERALL_WIDTH_MIN;
		m_nOverallWidthMax		= gnDEFAULT_OVERALL_WIDTH_MAX;
		m_nLineSpacingMin		= gnDEFAULT_LINE_SPACING_MIN;
		m_nLineSpacingMax		= gnDEFAULT_LINE_SPACING_MAX;
		m_nColumnSpacingMax		= gnDEFAULT_COLUMN_SPACING_MAX;
		m_nBridgeGapSmallerThan	= gnDEFAULT_BRIDGE_GAP;

		// Re-initialize LineFinder
		m_LineFinder = LeadToolsLineFinder();

		// Re-initialize LineGrouper
		m_LineGrouper = LeadToolsLineGroup();		
		
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
			UCLIDException ue("ELI18806", "Unable to load newer rule finding condition!");
			ue.addDebugInfo("Current Version", gnCurrentVersion);
			ue.addDebugInfo("Version to Load", nDataVersion);
			throw ue;
		}

		// Write member variables
		dataReader >> m_nColumnWidthMin;
		dataReader >> m_nColumnWidthMax;
		dataReader >> m_nOverallWidthMin;
		dataReader >> m_nOverallWidthMax;
		dataReader >> m_nLineSpacingMin;
		dataReader >> m_nLineSpacingMax;
		dataReader >> m_nColumnSpacingMax;
		dataReader >> m_nBridgeGapSmallerThan;

		// Read line finding variables
		long lTemp;
		dataReader >> lTemp;
		m_LineFinder.m_lr.iMinLineLength = lTemp;
		dataReader >> lTemp;
		m_LineFinder.m_lr.iMaxLineWidth = lTemp;
		dataReader >> lTemp;
		m_LineFinder.m_lr.iGapLength = lTemp;
		dataReader >> lTemp;
		m_LineFinder.m_lr.iVariance = lTemp;
		dataReader >> lTemp;
		m_LineFinder.m_lr.iWall = lTemp;
		dataReader >> lTemp;
		m_LineFinder.m_lr.iMaxWallPercent = lTemp;
		dataReader >> m_LineFinder.m_bExtendLineFragments;
		dataReader >> m_LineFinder.m_nExtensionScanLines;
		dataReader >> m_LineFinder.m_nExtensionTelescoping;
		dataReader >> m_LineFinder.m_nExtensionGap;
		dataReader >> m_LineFinder.m_nExtensionConsecutiveMinimum;

		// Read line grouping variables
		dataReader >> m_LineGrouper.m_Settings.m_nLineCountMin;
		dataReader >> m_LineGrouper.m_Settings.m_nLineCountMax;
		dataReader >> m_LineGrouper.m_Settings.m_nColumnCountMin;
		dataReader >> m_LineGrouper.m_Settings.m_nColumnCountMax;
		dataReader >> m_LineGrouper.m_Settings.m_nAlignmentScoreMin;
		dataReader >> m_LineGrouper.m_Settings.m_nAlignmentScoreExact;
		dataReader >> m_LineGrouper.m_Settings.m_nSpacingScoreMin;
		dataReader >> m_LineGrouper.m_Settings.m_nSpacingScoreExact;
		dataReader >> m_LineGrouper.m_Settings.m_nSpacingMin;
		dataReader >> m_LineGrouper.m_Settings.m_nSpacingMax;
		dataReader >> m_LineGrouper.m_Settings.m_nColumnSpacingMax;
		dataReader >> m_LineGrouper.m_Settings.m_nColumnWidthMax;
		dataReader >> m_LineGrouper.m_Settings.m_nOverallWidthMin;
		dataReader >> m_LineGrouper.m_Settings.m_nOverallWidthMax;
		
		// The following are not currently configurable, but load them anyway for future convenience
		dataReader >> m_LineGrouper.m_Settings.m_nCombineGroupPercentage;
		dataReader >> m_LineGrouper.m_Settings.m_nColumnAlignmentRequirement;
		dataReader >> m_LineGrouper.m_Settings.m_nColumnLineDiffAllowance;

		// Clear the dirty flag as we've loaded a fresh object
		m_bDirty = false;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18807");
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::Save(IStream *pStream, BOOL fClearDirty)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		ASSERT_ARGUMENT("ELI18808", pStream != __nullptr);

		// Create a bytestream and stream this object's data into it
		ByteStream data;
		ByteStreamManipulator dataWriter(ByteStreamManipulator::kWrite, data);

		dataWriter << gnCurrentVersion;

		// Write member variables
		dataWriter << m_nColumnWidthMin;
		dataWriter << m_nColumnWidthMax;
		dataWriter << m_nOverallWidthMin;
		dataWriter << m_nOverallWidthMax;
		dataWriter << m_nLineSpacingMin;
		dataWriter << m_nLineSpacingMax;
		dataWriter << m_nColumnSpacingMax;
		dataWriter << m_nBridgeGapSmallerThan;

		// Write line finding variables
		dataWriter << (long) m_LineFinder.m_lr.iMinLineLength;
		dataWriter << (long) m_LineFinder.m_lr.iMaxLineWidth;
		dataWriter << (long) m_LineFinder.m_lr.iGapLength;
		dataWriter << (long) m_LineFinder.m_lr.iVariance;
		dataWriter << (long) m_LineFinder.m_lr.iWall;
		dataWriter << (long) m_LineFinder.m_lr.iMaxWallPercent;
		dataWriter << m_LineFinder.m_bExtendLineFragments;
		dataWriter << m_LineFinder.m_nExtensionScanLines;
		dataWriter << m_LineFinder.m_nExtensionTelescoping;
		dataWriter << m_LineFinder.m_nExtensionGap;
		dataWriter << m_LineFinder.m_nExtensionConsecutiveMinimum;

		// Write line grouping variables
		dataWriter << m_LineGrouper.m_Settings.m_nLineCountMin;
		dataWriter << m_LineGrouper.m_Settings.m_nLineCountMax;
		dataWriter << m_LineGrouper.m_Settings.m_nColumnCountMin;
		dataWriter << m_LineGrouper.m_Settings.m_nColumnCountMax;
		dataWriter << m_LineGrouper.m_Settings.m_nAlignmentScoreMin;
		dataWriter << m_LineGrouper.m_Settings.m_nAlignmentScoreExact;
		dataWriter << m_LineGrouper.m_Settings.m_nSpacingScoreMin;
		dataWriter << m_LineGrouper.m_Settings.m_nSpacingScoreExact;
		dataWriter << m_LineGrouper.m_Settings.m_nSpacingMin;
		dataWriter << m_LineGrouper.m_Settings.m_nSpacingMax;
		dataWriter << m_LineGrouper.m_Settings.m_nColumnSpacingMax;
		dataWriter << m_LineGrouper.m_Settings.m_nColumnWidthMax;
		dataWriter << m_LineGrouper.m_Settings.m_nOverallWidthMin;
		dataWriter << m_LineGrouper.m_Settings.m_nOverallWidthMax;
		// The following are not currently configurable, but save them anyway for future convenience
		dataWriter << m_LineGrouper.m_Settings.m_nCombineGroupPercentage;
		dataWriter << m_LineGrouper.m_Settings.m_nColumnAlignmentRequirement;
		dataWriter << m_LineGrouper.m_Settings.m_nColumnLineDiffAllowance;
		
		dataWriter.flushToByteStream();

		// Write the bytestream data into the IStream object
		long nDataLength = data.getLength();
		pStream->Write(&nDataLength, sizeof(nDataLength), NULL);
		pStream->Write(data.getData(), nDataLength, NULL);
		
		// Clear the flag as specified
		if (fClearDirty)
		{
			m_bDirty = false;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18809");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::GetSizeMax(ULARGE_INTEGER *pcbSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	return E_NOTIMPL;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::raw_IsLicensed(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI18814", pbValue != __nullptr);

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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18815");

	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CImageLineUtility::InterfaceSupportsErrorInfo(REFIID riid)
{
	try
	{
		static const IID* arr[] = 
		{
			&IID_IImageLineUtility,
			&IID_IPersistStream,
			&IID_ICopyableObject,
			&IID_ILicensedComponent
		};

		for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
		{
			if (InlineIsEqualGUID(*arr[i],riid))
				return S_OK;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18813")

	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void CImageLineUtility::findLines(string strImageFileName, long nPageNum, double dRotation,
								  vector<LineRect> *pvecHorzLineRects, 
								  vector<LineRect> *pvecVertLineRects,
								  CRect *prectPageBounds/* = NULL*/)
{
	ASSERT_ARGUMENT("ELI19843", !strImageFileName.empty());
	ASSERT_ARGUMENT("ELI19844", nPageNum > 0);

	if (pvecHorzLineRects == NULL && pvecVertLineRects == NULL)
	{
		// Caller didn't request either horizontal or vertical lines. Nothing to do.
		return;
	}

	// Initialize BITMAPHANDLE, FILEINFO and LOADFILEOPTION for L_LoadBitmap call
	BITMAPHANDLE hBitmap;
	LeadToolsBitmapFreeer bitmapFreeer(hBitmap, true);
	FILEINFO fileInfo = GetLeadToolsSizedStruct<FILEINFO>(0);
	LOADFILEOPTION lfo = GetLeadToolsSizedStruct<LOADFILEOPTION>(ELO_IGNOREVIEWPERSPECTIVE);
	lfo.PageNumber = nPageNum;

	// Load the image in its original bits-per-pixel color depth so that current default-dithering
	// method doesn't affect how the image is loaded.
	// https://extract.atlassian.net/browse/ISSUE-13210
	loadImagePage(strImageFileName, hBitmap, fileInfo, lfo);

	{
		LeadToolsLicenseRestrictor leadToolsLicenseGuard;

		// Convert to bitonal since LeadTool's line finding only works on 1 bit images.
		// No dithering gives better results than the default (ordered?) dither method
		// https://extract.atlassian.net/browse/ISSUE-13210
		const long nDEFAULT_NUMBER_OF_COLORS = 0;
		L_INT nRet = L_ColorResBitmap(&hBitmap, &hBitmap, sizeof(BITMAPHANDLE), 1, CRF_NODITHERING | CRF_FIXEDPALETTE,
			NULL, NULL, nDEFAULT_NUMBER_OF_COLORS, NULL, NULL);
		throwExceptionIfNotSuccess(nRet, "ELI38459",
			"Internal error: Unable to convert image to bi-tonal!", strImageFileName);

		// ViewPerspective appears to be valid in situations where ELO_IGNOREVIEWPERSPECTIVE
		// does not work.  (For instance BMPs can be loaded with a valid BOTTOM_LEFT view perspective,
		// but ELO_IGNOREVIEWPERSPECTIVE will not ignore that view perspective).  For that reason, 
		// compensate for view perspective.  Its important to use fileInfo's ViewPerspective here
		// rather than hBitmap's.  
		if (fileInfo.ViewPerspective != TOP_LEFT)
		{
			nRet = L_ChangeBitmapViewPerspective(&hBitmap, &hBitmap, sizeof(BITMAPHANDLE), TOP_LEFT);
			throwExceptionIfNotSuccess(nRet, "ELI20623",
				"Internal error: ChangeBitmapViewPerspective operation failed!", strImageFileName);
		}
	}

	// L_RotateBitmap takes the rotation degrees in hundredths of a degree.  Convert
	// to hundredths of a degree
	L_INT nRotation = (L_INT)(dRotation * 100.0);

	// If the specified rotation is >= gnMIN_ROTATION, rotate the image as requested 
	if (abs(nRotation) >= gnMIN_ROTATION)
	{
		LeadToolsLicenseRestrictor leadToolsLicenseGuard;

		// Determine if the image is to be rotated such that it is to be closer to perpendicular
		// to the original orientation than parallel.
		bool perpendicular = (round(dRotation / 90) % 2 != 0);
		CSize sizeOrig(hBitmap.Width, hBitmap.Height);

		// Rotate the bitmap.
		// Allow resizing of the image if it is to be rotated perpendicular to the original
		// orientation-- otherwise unless the image is square, text is likely to extend out of the
		// image bounds in one direction while there will be empty whitespace at either end in the
		// other direction.
		L_INT nRet = L_RotateBitmap(&hBitmap, nRotation, perpendicular ? ROTATE_RESIZE : 0,
			RGB(255, 255, 255));
		throwExceptionIfNotSuccess(nRet, "ELI20465",
			"Internal error: Unable to apply rotation to image!", strImageFileName);

		// If the image was rotated and allowed to be resized, it needs to be trimmed back to the
		// original page dimensions so that OCR coordinates remain valid. (Otherwise they will
		// be offset by the amount of space that was added to the left & top edges of the image.
		if (perpendicular)
		{
			int	nXTrimAmount = (hBitmap.Width - sizeOrig.cy) / 2;
			int	nYTrimAmount = (hBitmap.Height - sizeOrig.cx) / 2;

			L_INT nRet = L_TrimBitmap(&hBitmap, nXTrimAmount, nYTrimAmount,
				hBitmap.Width - (2 * nXTrimAmount), hBitmap.Height - (2 * nYTrimAmount));

			throwExceptionIfNotSuccess(nRet, "ELI30319",
				"Internal error: Unable to trim rotated image!", strImageFileName);
		}
	}

	// Populate the page bounds rect (if provided).  
	if (prectPageBounds != __nullptr)
	{
		prectPageBounds->left = 0;
		prectPageBounds->top = 0;
		// Coordinates are zero based, so subtract 1 for the bottom/right bounds.
		prectPageBounds->right = fileInfo.Width - 1;
		prectPageBounds->bottom = fileInfo.Height - 1;
	}

	if (pvecHorzLineRects != __nullptr)
	{
		// Convert units according to image dimensions and resolution
		adjustSettingsForImage(fileInfo, true);

		// Search for horizontal lines
		m_LineFinder.findLines(&hBitmap, LINEREMOVE_HORIZONTAL, *pvecHorzLineRects);
	}

	if (pvecVertLineRects != __nullptr)
	{
		// Convert units according to image dimensions and resolution
		adjustSettingsForImage(fileInfo, false);

		// Search for vertical lines
		m_LineFinder.findLines(&hBitmap, LINEREMOVE_VERTICAL, *pvecVertLineRects);
	}

	// (bitmapFreeer will call L_FreeBitmap)
}
//-------------------------------------------------------------------------------------------------
void CImageLineUtility::adjustSettingsForImage(const FILEINFO &fileinfo, bool bHorizontal)
{
	// Ensure we have necessary information to make the unit conversions
	if (fileinfo.Width <= 0 || fileinfo.Height <= 0)
	{
		UCLIDException ue("ELI18916", "Internal error: Missing image data!");
		throw ue;
	}

	// Calculate the distance in pixels of one percent of the page
	int nXFactor, nYFactor;
	if (bHorizontal)
	{
		nXFactor = fileinfo.Width / 100;
		nYFactor = fileinfo.Height / 100;
	}
	else
	{
		nXFactor = fileinfo.Height / 100;
		nYFactor = fileinfo.Width / 100;
	}

	// Convert column width minimum to pixels
	if (m_nColumnWidthMin == gnUNSPECIFIED)
	{
		m_LineGrouper.m_Settings.m_nColumnWidthMin = gnUNSPECIFIED;
	}
	else
	{
		m_LineGrouper.m_Settings.m_nColumnWidthMin = m_nColumnWidthMin * nXFactor;
	}

	// Convert column width maximum to pixels
	if (m_nColumnWidthMax == gnUNSPECIFIED)
	{
		m_LineGrouper.m_Settings.m_nColumnWidthMax = gnUNSPECIFIED;
	}
	else
	{
		m_LineGrouper.m_Settings.m_nColumnWidthMax = m_nColumnWidthMax * nXFactor;
	}

	// Convert overall width minimum to pixels
	if (m_nOverallWidthMin == gnUNSPECIFIED)
	{
		m_LineGrouper.m_Settings.m_nOverallWidthMin = gnUNSPECIFIED;
	}
	else
	{
		m_LineGrouper.m_Settings.m_nOverallWidthMin = m_nOverallWidthMin * nXFactor;
	}

	// Convert overall width maximum to pixels
	if (m_nOverallWidthMax == gnUNSPECIFIED)
	{
		m_LineGrouper.m_Settings.m_nOverallWidthMax = gnUNSPECIFIED;
	}
	else
	{
		m_LineGrouper.m_Settings.m_nOverallWidthMax = m_nOverallWidthMax * nXFactor;
	}

	// Convert line spacing minimum to pixels
	if (m_nLineSpacingMin == gnUNSPECIFIED)
	{
		m_LineGrouper.m_Settings.m_nSpacingMin = gnUNSPECIFIED;
	}
	else
	{
		m_LineGrouper.m_Settings.m_nSpacingMin = m_nLineSpacingMin * nYFactor;
	}

	// Convert line spacing maximum to pixels
	if (m_nLineSpacingMax == gnUNSPECIFIED)
	{
		m_LineGrouper.m_Settings.m_nSpacingMax = gnUNSPECIFIED;
	}
	else
	{
		m_LineGrouper.m_Settings.m_nSpacingMax = m_nLineSpacingMax * nYFactor;
	}

	// Convert column spacing maximum to pixels
	if (m_nColumnSpacingMax == gnUNSPECIFIED)
	{
		m_LineGrouper.m_Settings.m_nColumnSpacingMax = gnUNSPECIFIED;
	}
	else
	{
		m_LineGrouper.m_Settings.m_nColumnSpacingMax = m_nColumnSpacingMax * nXFactor;
	}

	// Use default m_LineFinder.m_nBridgeGapSmallerThan if we don't have necessary info
	// to convert to pixels
	if (fileinfo.XResolution > 0)
	{
		// Convert BridgeGapSmallerThan from thousandths of an inch to pixels
		m_LineFinder.m_nBridgeGapSmallerThan = m_nBridgeGapSmallerThan * fileinfo.XResolution / 1000;
	}
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CImageLineUtility::lineRectVecToILongRectangleVec(const vector<LineRect> &vecRects)
{
	// Create the IUnknownVector to store the result
	IIUnknownVectorPtr ipResult(CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION("ELI19028", ipResult != __nullptr);

	for each (LineRect rect in vecRects)
	{
		ILongRectanglePtr ipRect(CLSID_LongRectangle);
		ASSERT_RESOURCE_ALLOCATION("ELI18703", ipRect != __nullptr);

		// Set the bounds of  the rectangle
		ipRect->SetBounds(rect.left, rect.top, rect.right, rect.bottom);

		ipResult->PushBack(ipRect);
	}

	return ipResult;
}
//-------------------------------------------------------------------------------------------------
void CImageLineUtility::longRectangleVecToLineRectVec(IIUnknownVectorPtr ipRects, bool bHorizontal,
													  vector<LineRect> &rvecRects)
{
	ASSERT_ARGUMENT("ELI19842", ipRects != __nullptr);

	for (int i = 0; i < ipRects->Size(); i++)
	{
		ILongRectanglePtr ipRect = ipRects->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI19841", ipRect != __nullptr);

		LineRect rect(CRect(ipRect->Left, ipRect->Top, ipRect->Right, ipRect->Bottom), bHorizontal);

		rvecRects.push_back(rect);
	}
}
//-------------------------------------------------------------------------------------------------
void CImageLineUtility::validateLicense()
{
	VALIDATE_LICENSE(gnEXTRACT_CORE_OBJECTS, "ELI18810", "Image Line Utils");
}
//-------------------------------------------------------------------------------------------------