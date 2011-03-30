#include "stdafx.h"
#include "FindLines.h"
#include "ExtractZoneAsImage.h"
#include "MiscLeadUtils.h"
#include "LeadToolsBitmapFreeer.h"

#include <l_bitmap.h>		// LeadTools Imaging library
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <UCLIDExceptionDlg.h>

#include <cmath>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// Default values for line-finding parameters
static const long gnDEFAULT_GAP_LENGTH		= 7;
static const long gnDEFAULT_WIDTH_MAX		= 30;
static const long gnDEFAULT_LENGTH_MIN		= 500;
static const long gnDEFAULT_WALL			= 35;
static const long gnDEFAULT_WALL_PERCENT	= 99;
static const long gnDEFAULT_VARIANCE		= 20;

static const long gnDEFAULT_GAP_BRIDGE		= 100;

static const long gnDEFAULT_EXT_SCANLINES	= 5;
static const long gnDEFAULT_EXT_GAP			= 20;
static const long gnDEFAULT_EXT_CONSECUTIVE	= 20;
static const long gnDEFAULT_EXT_TELESCOPING	= 35;

static const long gnINTERSECTION_ALLOWANCE_MAX = 20;

// The number of pixels that must be scanned length-wise before a 
// shift in the scan area is allowed.  For example, 15 allows line
// extension to follow a line with a slope of 15x to 1y.
static const long gnEXT_SHIFT_AFTER_DIST = 15;

// Determines how many pixels initializeTrackingPos will potentially scan to initialize line positions.
static const int gnINITIALIZATION_DIST = 30;

// correctFatLines will re-attempt to find lines that are at least gnFAT_LINE_CUTTOFF pixels thick 
// and whose width is in the top gnFAT_LINE_MAX_CHECK_PERCENT percent of the lines found.  If a 
// narrower line is found, its length must be at least gnFAT_LINE_LENGTH_OVERLAP percent of the
// length of the original line to use it as a replacement for the original line.
static const int gnFAT_LINE_CUTTOFF = 25;
static const int gnFAT_LINE_MAX_CHECK_PERCENT = 25;
static const int gnFAT_LINE_LENGTH_OVERLAP = 85;

//-------------------------------------------------------------------------------------------------
// Statics
//-------------------------------------------------------------------------------------------------
CMutex LeadToolsLineFinder::ms_mutex;

//-------------------------------------------------------------------------------------------------
// LeadToolsLineFinder
//-------------------------------------------------------------------------------------------------
LeadToolsLineFinder::LeadToolsLineFinder() :
	m_bHorizontal(true),
	m_pvecLines(NULL),
	m_pBitmap(NULL),
	m_nBridgeGapSmallerThan(gnDEFAULT_GAP_BRIDGE),
	m_bExtendLineFragments(true),
	m_nExtensionScanLines(gnDEFAULT_EXT_SCANLINES),
	m_nExtensionGap(gnDEFAULT_EXT_GAP),
	m_nExtensionConsecutiveMinimum(gnDEFAULT_EXT_CONSECUTIVE),
	m_nExtensionTelescoping(gnDEFAULT_EXT_TELESCOPING),
	m_bException(false)
{
	// Initialize line-finding settings with default values
	m_lr = GetLeadToolsSizedStruct<LINEREMOVE>(0);
	m_lr.uFlags = LINE_USE_GAP | LINE_USE_DPI | 
		LINE_REMOVE_ENTIRE | LINE_USE_VARIANCE | LINE_CALLBACK_REGION;

	m_lr.iGapLength					= gnDEFAULT_GAP_LENGTH; 
	m_lr.iMaxLineWidth				= gnDEFAULT_WIDTH_MAX; 
	m_lr.iMinLineLength				= gnDEFAULT_LENGTH_MIN;
	m_lr.iWall						= gnDEFAULT_WALL; 
	m_lr.iMaxWallPercent			= gnDEFAULT_WALL_PERCENT;
	m_lr.iVariance					= gnDEFAULT_VARIANCE;
}
//-------------------------------------------------------------------------------------------------
LeadToolsLineFinder::LeadToolsLineFinder(const LeadToolsLineFinder &source) :
	m_pvecLines(NULL),
	m_pBitmap(NULL),
	m_lr(source.m_lr),
	m_nBridgeGapSmallerThan(source.m_nBridgeGapSmallerThan),
	m_bExtendLineFragments(source.m_bExtendLineFragments),
	m_nExtensionScanLines(source.m_nExtensionScanLines),
	m_nExtensionGap(source.m_nExtensionGap),
	m_nExtensionConsecutiveMinimum(source.m_nExtensionConsecutiveMinimum),
	m_nExtensionTelescoping(source.m_nExtensionTelescoping),
	m_bException(false)
{
	// Don't copy line vector, bitmap or exception.  These are temporary pointers to resources
	// that can't be guaranteed after the findLines call completes. Only the settings should be 
	// copied.
}
//-------------------------------------------------------------------------------------------------
LeadToolsLineFinder::~LeadToolsLineFinder()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI19042");
}
//-------------------------------------------------------------------------------------------------
LeadToolsLineFinder& LeadToolsLineFinder::operator =(LeadToolsLineFinder &source)
{
	// Don't copy line vector, bitmap, exception or line extension que.  These are temporary 
	// resources that can't be guaranteed after the findLines call completes and that
	// shouldn't be re-used in a copy.  Only the settings should be copied.
	m_pvecLines = NULL;
	m_pBitmap = NULL;
	m_queScanLines.clear();
	m_bException = false;

	m_lr							= source.m_lr;

	m_nBridgeGapSmallerThan			= source.m_nBridgeGapSmallerThan;

	m_bExtendLineFragments			= source.m_bExtendLineFragments;
	m_nExtensionScanLines			= source.m_nExtensionScanLines;
	m_nExtensionGap					= source.m_nExtensionGap;
	m_nExtensionConsecutiveMinimum	= source.m_nExtensionConsecutiveMinimum;
	m_nExtensionTelescoping			= source.m_nExtensionTelescoping;

	return *this;
}
//-------------------------------------------------------------------------------------------------
void LeadToolsLineFinder::findLines(pBITMAPHANDLE pBitmap, L_UINT uFlags, vector<LineRect>& rvecLines)
{
	L_INT32 nRet = FAILURE;

	try
	{	
		ASSERT_ARGUMENT("ELI18846", pBitmap != __nullptr);

		m_bHorizontal = (uFlags == LINEREMOVE_HORIZONTAL);

		// Verify that Document support is licensed.  Needed for L_LineRemoveBitmap
		unlockDocumentSupport();

		// Before processing a line, clear any exception from a previous call
		m_bException = false;

		// Set horizontal/vertical setting
		m_lr.uRemoveFlags = uFlags;

		// Assign the currently targeted bitmap and line vector
		m_pBitmap = pBitmap;
		m_pvecLines = &rvecLines;

		// Call L_LineRemoveBitmap to search for lines.  lineRemoveCB will be called for
		// each line that is discovered.  Pass a pointer to this class to the function
		// to allow lineRemoveCB to add this line to the current collection.
		int nRet = L_LineRemoveBitmap(m_pBitmap, &m_lr, lineRemoveCB, (LPVOID *)this, 0);

		// If the exception flag is set, throw the exception.
		if (m_bException)
		{
			throw m_ue;
		}

		// If the return value from L_LineRemoveBitmap otherwise indicates failure, throw an exception
		throwExceptionIfNotSuccess(nRet, "ELI19045", "Failure when searching for image lines!");

		// [P16:2884] A higher LINEREMOVE::iWall settings means some lines will have a larger width
		// than appropriate.  Attempt to correct such lines.
		correctFatLines(rvecLines);

		// Merge lines only after correctFatLines so that fat lines do not trigger lines to merge
		// inappropriately
		mergeLines(rvecLines);

		// If line fragment extension is enabled, execute
		if (m_bExtendLineFragments)
		{
			extendLines();
		}

		// Clear the pointers to the vector and bitmap now that processing is complete.
		m_pvecLines = NULL;
		m_pBitmap = NULL;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI19496");
}
//-------------------------------------------------------------------------------------------------
void LeadToolsLineFinder::addLine(CRect rect)
{
	// Ensure the vector pointer is initialized
	if (m_pvecLines == NULL)
	{
		UCLIDException ue("ELI18994", "Internal error: addLine called without destination vector!");
		throw ue;
	}

	LineRect rectLine(rect, m_bHorizontal);
	m_pvecLines->push_back(rectLine);
}
//-------------------------------------------------------------------------------------------------
bool LeadToolsLineFinder::isWider(const LineRect &rectLine1, const LineRect &rectLine2)
{
	return (rectLine1.LineWidth() > rectLine2.LineWidth());
}
//-------------------------------------------------------------------------------------------------
void LeadToolsLineFinder::correctFatLines(vector<LineRect>& rvecLines)
{
	// Sort lines from widest to thinnest.
	sort(rvecLines.begin(), rvecLines.end(), isWider);
	
	// Do not attempt to correct more than gnFAT_LINE_MAX_CHECK_PERCENT of the lines.
	// If this document has large number of fat lines, it is likely only a small percentage of them 
	// are correctable problems so it will be inefficient and not worthwhile to process them all.
	size_t nCount = (m_pvecLines->size() * gnFAT_LINE_MAX_CHECK_PERCENT / 100);

	for (size_t i = 0; i < nCount; i++)
	{
		// If the width of this line is below gnFAT_LINE_CUTTOFF. All remaining lines will be as well.
		if (m_pvecLines->at(i).LineWidth() < gnFAT_LINE_CUTTOFF)
		{
			break;
		}

		// Create a new bitmap from the image region that was reported as a line.
		BITMAPHANDLE hBitmapRegion;
		LeadToolsBitmapFreeer bitmapFreeer(hBitmapRegion, true);
		L_CopyBitmapRect(&hBitmapRegion, m_pBitmap, sizeof(BITMAPHANDLE), 
			m_pvecLines->at(i).left, m_pvecLines->at(i).top, 
			m_pvecLines->at(i).Width(), m_pvecLines->at(i).Height());

		// Set m_pvecLines to point to a new collection of lines where L_LineRemoveBitmap is to 
		// insert lines found within region.  lineRemoveCB uses m_pvecLines at the collection
		// to add found lines. This container needs to be separate from the general line collection.
		vector<LineRect> vecContainedLines;
		m_pvecLines = &vecContainedLines;

		// Search for lines within this image area using an iWall setting half the size of the
		// original search.
		LINEREMOVE lr(m_lr);
		lr.iWall = m_lr.iWall / 2;
		L_LineRemoveBitmap(&hBitmapRegion, &lr, lineRemoveCB, (LPVOID *)this, 0);

		// Reset m_pvecLines to point to the original line collection
		m_pvecLines = &rvecLines;

		// Cycle through all lines contained in the region
		for each (LineRect vecContainedLine in vecContainedLines)
		{
			// If a contained line overlaps sufficiently with the original
			if (100 * vecContainedLine.LineLength() / m_pvecLines->at(i).LineLength() > gnFAT_LINE_LENGTH_OVERLAP)
			{
				// Convert the coordinates to be relative to the page rather than the region
				vecContainedLine.MoveToXY(m_pvecLines->at(i).left + vecContainedLine.left, 
										  m_pvecLines->at(i).top + vecContainedLine.top);

				// Replace the former (fat) line with the new (thinner) line.
				m_pvecLines->at(i) = vecContainedLine;
				break;
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------
void LeadToolsLineFinder::mergeLines(vector<LineRect> &rvecLines)
{
	vector<LineRect> vecMergedLines;

	// For each line in the set rvecLines
	while (rvecLines.empty() == false)
	{
		// Remove the last item in the vector
		LineRect rectLine = rvecLines.back();
		rvecLines.pop_back();

		// If it is not able to be merged, add the line to the set vecMergedLines.
		// If it was merged, the line will be added to vecMergedLines when we encounter
		// the last line with which it was merged.
		if (attemptMerge(rectLine) == false)
		{
			vecMergedLines.push_back(rectLine);
		}
	}

	// Return the merged set of lines.
	rvecLines = vecMergedLines;
}
//-------------------------------------------------------------------------------------------------
bool LeadToolsLineFinder::attemptMerge(LineRect &rrectLine)
{
	if (m_pvecLines == NULL)
	{
		UCLIDException ue("ELI19511", "Internal error: attemptMerge called without target vector!");
		throw ue;
	}

	// Create a vector to keep track of all existing lines for which the incoming line is merged with
	vector<int> vecMergedIndexes;

	// Loop through the exising vector of lines.  Continue looping after 
	// merges as its possible the line will qualify for merging with more than one line.
	for (size_t i = 0; i < m_pvecLines->size(); i++)
	{
		// If the line exactly matches an existing line, consider it merged
		if ((*m_pvecLines)[i] == rrectLine)
		{
			// Keep track of which lines were merged.
			vecMergedIndexes.push_back(i);
			continue;
		}

		// When testing for intersection, use half of the average of the line widths
		// as a offset allowance.
		int nIntersectionAllowance = (m_pvecLines->at(i).LineWidth() + rrectLine.LineWidth()) / 4;
		nIntersectionAllowance = min(nIntersectionAllowance, gnINTERSECTION_ALLOWANCE_MAX);

		// First test for 2 lines stacked closely on top of one another.
		LineRect rectTest(m_pvecLines->at(i));
		rectTest.InflateLine(0, nIntersectionAllowance);
		LineRect rectIntersection(m_bHorizontal);
		if (IntersectRect(&rectIntersection, &rectTest, &rrectLine))
		{
			// If an intersection is found, test to see if more than 50% of the 
			// shorter line is included in the intersection.  If so, merge.
			int nShortLineLength = min(rectTest.LineLength(), rrectLine.LineLength());

			if ((double) rectIntersection.LineLength() / (double) nShortLineLength > 0.50)
			{
				// These lines qualify for unification. Modify the existing line.
				// Use the vertical positioning of the narrower of the two.  In most cases, lines
				// that overlap in this manner will be due to text or handwriting on a line
				// that is recognized as a line itself.  This "line" is almost always
				// wider;  ignoring the verical positioning of such a line tends to yield the best
				// results.
				if (m_pvecLines->at(i).LineLength() < rrectLine.LineLength())
				{
					m_pvecLines->at(i).m_nLineTopOrLeftEdge = rrectLine.m_nLineTopOrLeftEdge;
					m_pvecLines->at(i).m_nLineBottomOrRightEdge = rrectLine.m_nLineBottomOrRightEdge;
				}

				// Combine the two for the overall line length
				m_pvecLines->at(i).m_nLineTopOrLeftEnd = 
					min(rectTest.m_nLineTopOrLeftEnd, rrectLine.m_nLineTopOrLeftEnd);

				m_pvecLines->at(i).m_nLineBottomOrRightEnd = 
					max(rectTest.m_nLineBottomOrRightEnd, rrectLine.m_nLineBottomOrRightEnd);

				// Update the incoming line to reflect the unified value
				rrectLine = m_pvecLines->at(i);

				// Keep track of which lines were merged.
				vecMergedIndexes.push_back(i);
			}
		}
		// Test to see if an existing line is positioned in-line with the current line.
		else if (abs(rrectLine.m_nLineTopOrLeftEdge - m_pvecLines->at(i).m_nLineTopOrLeftEdge) 
					<= nIntersectionAllowance ||
				abs(rrectLine.m_nLineBottomOrRightEdge - m_pvecLines->at(i).m_nLineBottomOrRightEdge) 
					<= nIntersectionAllowance)
		{
			// Create inflated test rect to test for intersection that accounts for 
			// nBridgeGapSmallerThan.
			LineRect rectTest2(m_pvecLines->at(i));
			rectTest2.InflateLine(m_nBridgeGapSmallerThan, 0);

			// Test for intersection 
			if (rectTest2.IntersectRect(&rectTest2, &rrectLine))
			{
				// These regions qualify for unification.  Update the existing rect with a union
				// of what was found.
				m_pvecLines->at(i).UnionRect(&m_pvecLines->at(i), &rrectLine);
				
				// Update the incoming line to reflect the unified value
				rrectLine = m_pvecLines->at(i);

				// Keep track of which lines were merged.
				vecMergedIndexes.push_back(i);
			}
		}
	}

	// If the line was merged with multiple existing lines, keep the most recent result as it
	// should be the most complete.  Erase all previous matches from back to front so the indexes
	// remain valid. Start at size - 2 because size - 1 is the index we would want to keep.
	int nIndexToDelete = -1;
	for (int i = vecMergedIndexes.size() - 2; i >= 0; i--)
	{
		nIndexToDelete = vecMergedIndexes[i];
		m_pvecLines->erase(m_pvecLines->begin() + nIndexToDelete);
	}

	// Merge successful if vecMergedIndexes has at least one entry
	return (vecMergedIndexes.size() > 0);
}
//-------------------------------------------------------------------------------------------------
void LeadToolsLineFinder::extendLines()
{
	// Processing time appears to be adversely affected at times if multiple threads are allowed
	// to run extendLines at the same time.  Limiting this call to one thread at a time does not
	// affect performance too much on a small number of cores since only 10-15% of total findLines 
	// processing time is spent in extendLines.  However, on a machine with a large number of cores
	// this function may become a bottleneck.
	CSingleLock lg( &ms_mutex, TRUE );

	vector<LineRect> vecExtendedLines;

	for (vector<LineRect>::iterator prectLine = m_pvecLines->begin();
		prectLine != m_pvecLines->end();
		prectLine ++)
	{
		// Attempt to extend each line in both directions
		bool bModified = extendLine(*prectLine, -1);
		bModified |= extendLine(*prectLine, 1);

		// If the line was extended, add it to a separate vector
		if (bModified)
		{
			vecExtendedLines.push_back(*prectLine);
		}
	}

	// Add extended lines into the primary vector after testing for meg
	for (vector<LineRect>::iterator prectLine = vecExtendedLines.begin();
		prectLine != vecExtendedLines.end();
		prectLine ++)
	{
		addLine(*prectLine);
	}
}
//-------------------------------------------------------------------------------------------------
bool LeadToolsLineFinder::extendLine(LineRect &rrectLine, int nDirection)
{
	ASSERT_ARGUMENT("ELI18997", nDirection == 1 || nDirection == -1);

	// Initialize tracking pos depending upon the specified direction to extend the line
	int nLenPos;
	if (nDirection == 1)
	{
		nLenPos = rrectLine.m_nLineBottomOrRightEnd;
	}
	else
	{
		nLenPos = rrectLine.m_nLineTopOrLeftEnd;
	}

	// Find the line position within the rect at the specified end
	int nTrackPos = initializeTrackingPos(rrectLine, nDirection, gnINITIALIZATION_DIST);

	// Initialize the top edge of the scan area accordingly
	int nTrackEdge = nTrackPos - (m_nExtensionScanLines / 2);

	// Variable to keep track of the distance we should skip ahead on each iteration
	int nTrackDist = 0;

	// The current gap (white pixel) accumulation
	int nCurrentGap = 0;

	// The current number of consecutive black pixels that have been found
	int nConsecutive = 0;

	// true if an error was encountered reading a pixel (offpage)
	bool bErrorReadingPixel = false;

	// Return value
	bool bResult = false;

	// Initialize the vector of scan lines to zero for each scan line
	m_queScanLines.clear();
	m_queScanLines.assign(m_nExtensionScanLines, 0);

	// Each loop scans progressively further out along the line
	while (!bErrorReadingPixel)
	{
		bool bFoundBlack = false;

		// Scan the pixel for each scan line at this position
		for (int i = 0; i < m_nExtensionScanLines; i++)
		{
			// Check pixel color
			bool bBlackPixel = checkPixel(nLenPos, nTrackEdge + i, bErrorReadingPixel);

			// If there was a problem reading the pixel, break out of the loop
			if (bErrorReadingPixel)
			{
				break;
			}

			if (bBlackPixel)
			{
				// Update the consecutive pixel count for this line
				m_queScanLines[i] = m_queScanLines[i] + 1;

				bFoundBlack = true;
			}
			else
			{
				// Reset the consecutive pixel count for this line
				m_queScanLines[i] = 0;
			}
		}

		if (bErrorReadingPixel)
		{
			// If there was a problem reading the pixel, break out of the loop
			break;
		}
		else if (bFoundBlack)
		{
			// Update the overall consecutive black pixel count as long one of the scan
			// lines had a black pixel
			nConsecutive++;
		}
		else
		{
			// If none of the scan lines had a black pixel, increment the gap count
			nCurrentGap++;
			nConsecutive = 0;

			// If the gap has exceeded the allowable size, break.
			if (nCurrentGap > m_nExtensionGap)
			{
				break;
			}
		}

		if (nConsecutive >= m_nExtensionConsecutiveMinimum)
		{
			// Enough consecutive black pixels have been found to qualify the line for extension
			nCurrentGap = 0;

			// Extend the rect in the appropriate direction
			if (nDirection == -1)
			{
				rrectLine.m_nLineTopOrLeftEnd = nLenPos;
			}
			else
			{
				rrectLine.m_nLineBottomOrRightEnd = nLenPos;
			}

			// Widen the line as necessary
			if (nTrackEdge + (m_nExtensionScanLines / 2) < rrectLine.m_nLineTopOrLeftEdge)
			{
				rrectLine.m_nLineTopOrLeftEdge = nTrackEdge + (m_nExtensionScanLines / 2);
			}
			else if (nTrackEdge + (m_nExtensionScanLines / 2) > rrectLine.m_nLineBottomOrRightEdge)
			{
				rrectLine.m_nLineBottomOrRightEdge = nTrackEdge + (m_nExtensionScanLines / 2);
			}

			bResult = true;
		}

		// Update the tracking postion (nTrackDist == 0 indicates the first call to updateTrackingPos
		// for this line & direction
		updateTrackingPos(nTrackEdge, bFoundBlack, nTrackDist == 0);

		// Set nTrackDist according to whether we found a black pixel
		if (bFoundBlack)
		{
			nTrackDist = 1;
		}
		else
		{
			nTrackDist ++;
		}

		// Increment the scan position according to nTrackDist and the telescoping parameter
		nLenPos += max(nTrackDist * m_nExtensionTelescoping / 100, 1) * nDirection;

		// Do not allow the line to be extended over a gap larger than the m_nBridgeGapSmallerThan value
		if ((nDirection == -1 && (rrectLine.m_nLineTopOrLeftEnd - nLenPos - nConsecutive) > m_nBridgeGapSmallerThan) ||
			(nDirection == 1 && (nLenPos - rrectLine.m_nLineBottomOrRightEnd - nConsecutive) > m_nBridgeGapSmallerThan))
		{
			break;
		}
	}

	return bResult;
}
//-------------------------------------------------------------------------------------------------
void LeadToolsLineFinder::updateTrackingPos(int &rnTrackEdge, bool bFoundBlackPixel, bool bFirst)
{
	int nMaxStreak = 0;
	int nNewCenter = 0;

	if (bFirst)
	{
		// The first call for this line & direction;  initialize tracking info
		m_TrackingInfo.nCenter = rnTrackEdge + (m_nExtensionScanLines / 2);
		m_TrackingInfo.nTargetShift = 0;
		m_TrackingInfo.nAccumulatedShift = 0;
	}

	if (bFoundBlackPixel && m_queScanLines[m_TrackingInfo.nCenter - rnTrackEdge] == 0)
	{
		// We found a black pixel, but not a the line that previously was considered
		// to be the line center.  Assign a new center based on the scan line with 
		// the longest running streak of black pixels
		for (size_t i = 0; i < m_queScanLines.size(); i++)
		{
			if (m_queScanLines[i] > nMaxStreak)
			{
				nMaxStreak = m_queScanLines[i];
				nNewCenter = rnTrackEdge + i;
			}
		}

		if (nMaxStreak > 0)
		{
			m_TrackingInfo.nCenter = nNewCenter;

			// Based on the new center, where should the scan area be shifted to
			// in the long run (specified in pixel offset from current scan area)
			m_TrackingInfo.nTargetShift = 
				m_TrackingInfo.nCenter - rnTrackEdge - (m_queScanLines.size() / 2);
		}
	}

	if (m_TrackingInfo.nTargetShift != 0)
	{
		// A shift of the scan window is called for; monitor progress toward that shift
		bool bShift = false;

		// Obtain the direction of the desired shift (-1 for up, 1 for down)
		int nSignOfShift = (m_TrackingInfo.nTargetShift / abs(m_TrackingInfo.nTargetShift));

		// Add to accumulation toward the next shift
		m_TrackingInfo.nAccumulatedShift += nSignOfShift;

		if (m_TrackingInfo.nAccumulatedShift == 0)
		{
			// If accumulation total is zero, it qualifies for a shift in either direction
			bShift = true;
		}
		else
		{
			// If shift accumulation is not zero, it qualifies for a shift if the
			// the sign of the shift is the same sign as the accumulation
			int nSignAccumulation = 
				(m_TrackingInfo.nAccumulatedShift / abs(m_TrackingInfo.nAccumulatedShift));
			
			if (nSignAccumulation == nSignOfShift)
			{
				bShift = true;
			}
		}
		
		if (bShift)
		{
			// A shift is qualified.  Shift the scan window, and update all parameters
			// to reflect the shift.
			m_TrackingInfo.nAccumulatedShift -= (gnEXT_SHIFT_AFTER_DIST * nSignOfShift);
			rnTrackEdge += nSignOfShift;
			m_TrackingInfo.nCenter += nSignOfShift;
			m_TrackingInfo.nTargetShift -= nSignOfShift;

			// Shift the scan lines to account for the shifted scan window
			if (nSignOfShift == 1)
			{
				m_queScanLines.pop_front();
				m_queScanLines.push_back(0);
			}
			else
			{
				m_queScanLines.pop_back();
				m_queScanLines.push_front(0);
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------
int LeadToolsLineFinder::initializeTrackingPos(LineRect rectLine, int nDirection, int nLen)
{
	ASSERT_ARGUMENT("ELI18995", nDirection == 1 || nDirection == -1);
	ASSERT_ARGUMENT("ELI18996", nLen >= 0);

	// If specified length to search is zero, return the center of the provided rect
	if (nLen == 0)
	{
		return rectLine.LinePosition();
	}

	// If specified length is longer than the line rect provided, shorten the length
	// appropriately
	if (nLen > rectLine.LineLength())
	{
		nLen = rectLine.LineLength();
	}

	// Create a scan line for each pixel of width in the line
	set<int> setScanLines;
	for (int i = rectLine.m_nLineTopOrLeftEdge; i <= rectLine.m_nLineBottomOrRightEdge; i++)
	{
		setScanLines.insert(i);
	}

	// Initialize the scan position based on the direction specified
    int nPos;
	if (nDirection == 1)
	{
		nPos = rectLine.m_nLineBottomOrRightEnd;
	}
	else
	{
		nPos = rectLine.m_nLineTopOrLeftEnd;
	}

	// bInitialized is true once a black pixel has been found
	bool bInitialized = false;
	
	// bErrorReadingPixel if we failed to read a pixel (offpage)
	bool bErrorReadingPixel = false;

	// Scan from the end of the line, eliminating scan lines as white pixels are
	// found in each
	for (int i = nPos; nLen > abs(nPos - i); i += nDirection)
	{
		vector< set<int>::iterator > vecItersToDelete;

		// Remove every scan line from the vector which does not contain a black
		// pixel at this position
		for (set<int>::iterator iter = setScanLines.begin(); iter != setScanLines.end(); iter++)
		{
			if (!checkPixel(i, *iter, bErrorReadingPixel) && !bErrorReadingPixel)
			{
				vecItersToDelete.push_back(iter);
			}
		}

		if (bErrorReadingPixel || setScanLines.size() == vecItersToDelete.size())
		{
			// Either there was an error reading a pixel, or there were no black pixels
			// found at this postion
			if (!bErrorReadingPixel && !bInitialized)
			{
				// If we have not yet found a black pixel for this end of the line,
				// retry 1 pixel further in from the edge of the line;
				if (nDirection == 1)
				{
					rectLine.m_nLineBottomOrRightEnd--;
					return initializeTrackingPos(rectLine, nDirection, nLen - 1);
				}
				else
				{
					rectLine.m_nLineTopOrLeftEnd++;
					return initializeTrackingPos(rectLine, nDirection, nLen - 1);
				}
			}
			else
			{
				// If we are initiallized (or an error was encounterd), return an
				// average of the remaining scan lines.
				int nTotal = 0;
				for each(int i in setScanLines)
				{
					nTotal += i;
				}
				return (nTotal / setScanLines.size());
			}
		}
		else
		{
			// Black pixels were found.  Remove disqualified scan lines and continue scanning
			if (vecItersToDelete.size() > 0)
			{
				bInitialized = true;
			}

			for each(set<int>::iterator iter in vecItersToDelete)
			{
				setScanLines.erase(iter);
			}
		}
	}
	// Scan is complete

	if (setScanLines.size() == 0)
	{
		// We should not get to this point with zero scan lines
		THROW_LOGIC_ERROR_EXCEPTION("ELI18998");
	}

	// Return an average of the remaining scan lines.
	int nTotal = 0;
	for each(int i in setScanLines)
	{
		nTotal += i;
	}
	return (nTotal / setScanLines.size());
}
//-------------------------------------------------------------------------------------------------
bool LeadToolsLineFinder::checkPixel(int nLenPos, int nWidthPos, bool &rbErrorReadingPixel)
{
	COLORREF bgdColor;
	
	if (m_bHorizontal)
	{
		bgdColor = L_GetPixelColor(m_pBitmap, nWidthPos, nLenPos);
	}
	else
	{
		bgdColor = L_GetPixelColor(m_pBitmap, nLenPos, nWidthPos);
	}

	// 0x80000000 value indicates an error (see L_GetPixelColor documentation)
	if (bgdColor == 0x80000000)
	{
		rbErrorReadingPixel = true;
		return false;
	}

	// Return true if the average red, green and blue value is less than 128 (50% saturation)
	return (GetRValue(bgdColor) + GetGValue(bgdColor) + GetBValue(bgdColor) < 128 * 3);
}

//-------------------------------------------------------------------------------------------------
// Global callback
//-------------------------------------------------------------------------------------------------
L_INT EXT_FUNCTION lineRemoveCB(HRGN hRgn, L_INT32 nStartRow, L_INT32 nStartCol, L_INT32 nLength, 
								L_VOID *pUserData)
 {
	// pUserData is a LeadToolsLineFinder instance to use
	// Assign outside of try scope so it can be used in the catch handler
	LeadToolsLineFinder *pLineFinder = (LeadToolsLineFinder *) pUserData;

	try
	{
		try
		{
			ASSERT_ARGUMENT("ELI18372", hRgn != __nullptr);
			ASSERT_ARGUMENT("ELI18373", pLineFinder != __nullptr);

			// Obtain the CRect region of the line that was found
			CRgn rgnLine;
			rgnLine.Attach(hRgn);
			CRect rectLine;
			rgnLine.GetRgnBox(&rectLine);

			DeleteObject(hRgn);

			// Add the found line to pLineFinder's collection
			pLineFinder->addLine(rectLine);
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI19043");
	}
	catch (UCLIDException &ue)
	{
		// Pass back exception information to pLineFinder.
		ue.addDebugInfo("ELI19046", "Failure when processing image line!");
		pLineFinder->m_ue = ue;
		pLineFinder->m_bException = true;

		return FAILURE;
	}

	// We don't need or want the line actually removed. Indicate success, but without line removal
	return SUCCESS_NOREMOVE; 
}
//-------------------------------------------------------------------------------------------------