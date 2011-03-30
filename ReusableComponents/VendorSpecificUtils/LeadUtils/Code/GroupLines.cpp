#include "stdafx.h"
#include "GroupLines.h"
#include "MiscLeadUtils.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <UCLIDExceptionDlg.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
static const long gnUNSPECIFIED								= -1;
static const long gnDEFAULT_MIN_LINE_COUNT					= 2;
static const long gnDEFAULT_MAX_LINE_COUNT					= gnUNSPECIFIED;
static const long gnDEFAULT_MIN_COLUMN_COUNT				= 2;
static const long gnDEFAULT_MAX_COLUMN_COUNT				= gnUNSPECIFIED;
static const long gnDEFAULT_MIN_WIDTH_SCORE					= 50;
static const long gnDEFAULT_EXACT_WIDTH_SCORE				= 95;
static const long gnDEFAULT_MIN_SPACING_SCORE				= 60; 
static const long gnDEFAULT_EXACT_SPACING_SCORE				= 90;
static const long gnDEFAULT_MIN_SPACING						= gnUNSPECIFIED;
static const long gnDEFAULT_MAX_SPACING						= 200;
static const long gnDEFAULT_MAX_COLUMN_SPACING				= 500;
static const long gnDEFAULT_MIN_COLUMN_WIDTH				= gnUNSPECIFIED;
static const long gnDEFAULT_MAX_COLUMN_WIDTH				= gnUNSPECIFIED;
static const long gnDEFAULT_MIN_OVERALL_WIDTH				= gnUNSPECIFIED;
static const long gnDEFAULT_MAX_OVERALL_WIDTH				= gnUNSPECIFIED;
static const long gnDEFAULT_COMBINE_GROUP_PERCENTAGE		= 90;
static const long gnDEFAULT_COLUMN_ALIGNMENT_REQUIREMENT	= 35;
static const long gnDEFAULT_COLUMN_LINE_DIFF_ALLOWANCE		= 0; 
static const bool gbDEFAULT_IS_HORIZONTAL					= true;
static const long gnLINE_INTERSECT_ALLOWANCE				= 20;
static const long gnMAX_GROUP_CHILDREN						= 2;
static const long gnMAX_GROUPINGS_PER_PAGE					= 500;

//-------------------------------------------------------------------------------------------------
// Global helper functions
//-------------------------------------------------------------------------------------------------
bool isLineBelow(LineRect rectLine1, LineRect rectLine2)
{
	ASSERT_ARGUMENT("ELI19513", rectLine1.IsHorizontal() == rectLine2.IsHorizontal());

	return (rectLine1.LinePosition() < rectLine2.LinePosition());
}

//-------------------------------------------------------------------------------------------------
// LeadToolsLineGroup
//-------------------------------------------------------------------------------------------------
LeadToolsLineGroup::LeadToolsLineGroup() :
	m_nLineCount(0),
	m_nColumnCount(0),
	m_nSpacing(gnDEFAULT_MAX_SPACING / 2),
	m_nColumnSpacing(gnDEFAULT_MAX_COLUMN_SPACING / 2),
	m_nLineMinimum(0),
	m_bIsFourSidedBox(false),
	m_apSubLineGroup(__nullptr),
	m_apSubColumnGroup(__nullptr),
	m_LineRect(true),
	m_Rect(true),
	m_nLineTopOrLeftEndId(gnUNSPECIFIED),
	m_nLineBottomOrRightEndId(gnUNSPECIFIED)
{
	// Initialize default settings
	m_Settings.m_nLineCountMin					= gnDEFAULT_MIN_LINE_COUNT;
	m_Settings.m_nLineCountMax					= gnDEFAULT_MAX_LINE_COUNT;
	m_Settings.m_nColumnCountMin				= gnDEFAULT_MIN_COLUMN_COUNT;
	m_Settings.m_nColumnCountMax				= gnDEFAULT_MAX_COLUMN_COUNT;
	m_Settings.m_nAlignmentScoreMin				= gnDEFAULT_MIN_WIDTH_SCORE;
	m_Settings.m_nAlignmentScoreExact			= gnDEFAULT_EXACT_WIDTH_SCORE;
	m_Settings.m_nSpacingScoreMin				= gnDEFAULT_MIN_SPACING_SCORE;
	m_Settings.m_nSpacingScoreExact				= gnDEFAULT_EXACT_SPACING_SCORE;
	m_Settings.m_nSpacingMin					= gnDEFAULT_MIN_SPACING;
	m_Settings.m_nSpacingMax					= gnDEFAULT_MAX_SPACING;
	m_Settings.m_nColumnSpacingMax				= gnDEFAULT_MAX_COLUMN_SPACING;
	m_Settings.m_nColumnWidthMin				= gnDEFAULT_MIN_COLUMN_WIDTH;
	m_Settings.m_nColumnWidthMax				= gnDEFAULT_MAX_COLUMN_WIDTH;
	m_Settings.m_nOverallWidthMin				= gnDEFAULT_MIN_OVERALL_WIDTH;
	m_Settings.m_nOverallWidthMax				= gnDEFAULT_MAX_OVERALL_WIDTH;
	m_Settings.m_nCombineGroupPercentage		= gnDEFAULT_COMBINE_GROUP_PERCENTAGE;
	m_Settings.m_nColumnAlignmentRequirement	= gnDEFAULT_COLUMN_ALIGNMENT_REQUIREMENT;
	m_Settings.m_nColumnLineDiffAllowance		= gnDEFAULT_COLUMN_LINE_DIFF_ALLOWANCE;
	m_Settings.m_bHorizontal					= gbDEFAULT_IS_HORIZONTAL;
}
//-------------------------------------------------------------------------------------------------
LeadToolsLineGroup::LeadToolsLineGroup(LineRect rectLine, const Settings &settings) :
	m_LineRect(rectLine),
	m_Rect(rectLine),
	m_nLineCount(1),
	m_nColumnCount(1),
	m_nSpacing(gnDEFAULT_MAX_SPACING / 2),
	m_nColumnSpacing(gnDEFAULT_MAX_COLUMN_SPACING / 2),
	m_nLineMinimum(0),
	m_bIsFourSidedBox(false),
	m_nLineTopOrLeftEndId(gnUNSPECIFIED),
	m_nLineBottomOrRightEndId(gnUNSPECIFIED),
	m_apSubLineGroup(__nullptr),
	m_apSubColumnGroup(__nullptr),
	m_Settings(settings)
{
}
//-------------------------------------------------------------------------------------------------
LeadToolsLineGroup::LeadToolsLineGroup(const LeadToolsLineGroup &group) :
	m_LineRect(group.m_LineRect),
	m_Rect(group.m_Rect),
	m_nLineCount(group.m_nLineCount),
	m_nColumnCount(group.m_nColumnCount),
	m_nSpacing(group.m_nSpacing),
	m_nColumnSpacing(group.m_nColumnSpacing),
	m_nLineMinimum(group.m_nLineMinimum),
	m_bIsFourSidedBox(false),
	m_nLineTopOrLeftEndId(group.m_nLineTopOrLeftEndId),
	m_nLineBottomOrRightEndId(group.m_nLineBottomOrRightEndId),
	m_apSubLineGroup(__nullptr),
	m_apSubColumnGroup(__nullptr),
	m_Settings(group.m_Settings)
{
	// Settings copied from source group

	// Create a copy of the sub-line group if it exists
	if (group.m_apSubLineGroup.get() != __nullptr)
	{
		m_apSubLineGroup = 
			unique_ptr<LeadToolsLineGroup>(new LeadToolsLineGroup(*group.m_apSubLineGroup));
	}

	// Create a copy of the sub-column group if it exists
	if (group.m_apSubColumnGroup.get() != __nullptr)
	{
		m_apSubColumnGroup = 
			unique_ptr<LeadToolsLineGroup>(new LeadToolsLineGroup(*group.m_apSubColumnGroup));
	}
}
//-------------------------------------------------------------------------------------------------
LeadToolsLineGroup& LeadToolsLineGroup::operator =(LeadToolsLineGroup &group)
{
	m_LineRect					= group.m_LineRect;
	m_Rect						= group.m_Rect;
	m_nLineCount				= group.m_nLineCount;
	m_nColumnCount				= group.m_nColumnCount;
	m_nSpacing					= group.m_nSpacing;
	m_nColumnSpacing			= group.m_nColumnSpacing;
	m_nLineMinimum				= group.m_nLineMinimum;
	m_bIsFourSidedBox			= group.m_bIsFourSidedBox;
	m_nLineTopOrLeftEndId		= group.m_nLineTopOrLeftEndId;
	m_nLineBottomOrRightEndId	= group.m_nLineBottomOrRightEndId;

	m_Settings			= group.m_Settings;

	if (group.m_apSubLineGroup.get() != __nullptr)
	{
		// Create a copy of the sub-line group if it exists
		m_apSubLineGroup = 
			unique_ptr<LeadToolsLineGroup>(new LeadToolsLineGroup(*group.m_apSubLineGroup));
	}
	else
	{
		// Otherwise clear m_apSubLineGroup
		m_apSubLineGroup.reset();
	}

	if (group.m_apSubColumnGroup.get() != __nullptr)
	{
		// Create a copy of the sub-column group if it exists
		m_apSubColumnGroup = 
			unique_ptr<LeadToolsLineGroup>(new LeadToolsLineGroup(*group.m_apSubColumnGroup));
	}
	else
	{
		// Otherwise clear m_apSubColumnGroup
		m_apSubColumnGroup.reset();
	}

	return *this;
}
//-------------------------------------------------------------------------------------------------
LeadToolsLineGroup::~LeadToolsLineGroup()
{
	try
	{
		m_apSubLineGroup.reset();
		m_apSubColumnGroup.reset();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI18684");
}
//-------------------------------------------------------------------------------------------------
bool LeadToolsLineGroup::groupLines(vector<LineRect> &rvecLineRects,
									vector<LineRect> &rvecGroupRects,
									CRect rectBounds,
									vector< vector<LineRect> > *pvecSubLineRects/* = NULL*/)
{
	// Sorting from top-down makes groupLinesIntoColumns more efficient.  
	// Since groupLinesIntoColumns only searches for matching lines lower on the page 
	// than the line being evaluated, by placing the lines in vertical order we can avoid 
	// unnecessary tests against lines above the one being evaluated.
	sort(rvecLineRects.begin(), rvecLineRects.end(), isLineBelow);

	rvecGroupRects.clear();

	list<LeadToolsLineGroup> listGroups;

	// Populate a list of groups-- one group per line
	for each (const LineRect &rectLine in rvecLineRects)
	{
		LeadToolsLineGroup group(rectLine, m_Settings);
		listGroups.push_back(group);
	}

	// Group lines vertically into columns
	// Returns true if algorithm completed correctly. A return value of false indicates the
	// the algorithm short-circuited after encountering too many potential line grouping
	// permutations.
	bool bResult = groupLinesIntoColumns(listGroups);

	// Group columns horizontally into image areas
	groupColumnsIntoAreas(listGroups);

	// Ensure areas meet qualifications and remove duplicate areas
	qualifyAreas(listGroups);

	for each (const LeadToolsLineGroup &group in listGroups)
	{
		// Inflate the resulting areas vertically to capture one row above
		// and below the bounding lines and insert them into the result vector
		LineRect rect = group.m_Rect;
		rect.InflateLine(0, group.m_nSpacing);

		// [FlexIDSCore:3068] Don't allow a return value to extend outside of the bounds provided.
		if (rect.IntersectRect(rect, rectBounds) == false)
		{
			// If the return value doesn't intersect at all with the provided bounds, don't
			// include this region in the return value.
			continue;
		}

		rvecGroupRects.push_back(rect);

		if (pvecSubLineRects != __nullptr)
		{
			// If requested, also return the lines that comprise each group
			vector<LineRect> vecLineRects;
			group.getLineRects(vecLineRects);
			pvecSubLineRects->push_back(vecLineRects);
		}
	}

	return bResult;
}
//-------------------------------------------------------------------------------------------------
bool LeadToolsLineGroup::findBoxes(vector<LineRect> &rvecHorzLineRects,
								   vector<LineRect> &rvecVertLineRects,
								   list<LeadToolsLineGroup> &rlistFoundBoxes)
{
	// Sort the incoming lines from top down and left to right for efficiency.
	sort(rvecHorzLineRects.begin(), rvecHorzLineRects.end(), isLineBelow);
	sort(rvecVertLineRects.begin(), rvecVertLineRects.end(), isLineBelow);

	// Populate a list of groups-- one group per line
	list<LeadToolsLineGroup> listBoxes;
	for (size_t i = 0; i < rvecHorzLineRects.size(); i++)
	{
		LeadToolsLineGroup group(rvecHorzLineRects[i], m_Settings);
		listBoxes.push_back(group);
	}

	// Use the vertical lines to dice up the groups in to all possible permutations
	splitLines(listBoxes, rvecVertLineRects);

	// Call groupLinesIntoColumns with bBoxBoundsOnly == true to search for boxes that
	// can be formed. Returns true if algorithm completed correctly. A return value of 
	// false indicates the the algorithm short-circuited after encountering too many 
	// potential boxes boxes.
	bool bResult = groupLinesIntoColumns(listBoxes, true);

	// Ensure areas meet qualifications and remove duplicate areas
	qualifyAreas(listBoxes, false);
	
	// Add the qualified return values to the result list
	appendBoxResults(rlistFoundBoxes, listBoxes);

	return bResult;
}
//-------------------------------------------------------------------------------------------------
bool LeadToolsLineGroup::findBoxContainingRect(const LineRect &rect, vector<LineRect> &rvecHorzLineRects,
											   vector<LineRect> &rvecVertLineRects,
											   int nRequiredMatchingBoundaries, LineRect &rrectResult,
											   bool &rbIncompleteResult)
{
	// Search for lines immediately surrounding the provided linerect
	findLinesAroundRect(rect, rvecHorzLineRects, rvecVertLineRects);

	// Find all boxes that can be formed with these lines.  Search both horizontally and vertically
	// for boxes that can be formed (boxes are primarily formed via opposing box edges oriented
	// in the direction of the lines in the first parameter to findBoxes).  The results from the
	// first search will be combined with the results from the second.
	list<LeadToolsLineGroup> listFoundBoxes;
	if (findBoxes(rvecHorzLineRects, rvecVertLineRects, listFoundBoxes) &&
		findBoxes(rvecVertLineRects, rvecHorzLineRects, listFoundBoxes))
	{
		// findBoxes returned true in both directions.
		rbIncompleteResult = false;
	}
	else
	{
		// If findBoxes returns false, it indicates it was unable to properly finish processing
		// and that the results may not include all potential boxes.
		rbIncompleteResult = true;
	}

	// Merge any duplicated boxes from the two calls
	qualifyAreas(listFoundBoxes, false);

	// Keep track of whether we have found a qualifying box and whether all four sides of the
	// qualifying box have been found.
	bool bFoundBox = false;
	bool bFoundFourSidedBox = false;

	// Cycle through all boxes that were found.
	for each (const LeadToolsLineGroup &groupBox in listFoundBoxes)
	{
		// Test to see that the center point of the provided LineRect is contained in the box.
		if (groupBox.m_Rect.PtInRect(rect.CenterPoint()))
		{
			if (nRequiredMatchingBoundaries > 0)
			{
				// If caller has requested a certain number of boundaries of the box to match 
				// those of provided rect, count the number of matching boundaries
				int nMatchingBoundaries = 0;

				if (rect.left == groupBox.m_Rect.left)
				{
					nMatchingBoundaries++;
				}
				if (rect.top == groupBox.m_Rect.top)
				{
					nMatchingBoundaries++;
				}
				if (rect.right == groupBox.m_Rect.right)
				{
					nMatchingBoundaries++;
				}
				if (rect.bottom == groupBox.m_Rect.bottom)
				{
					nMatchingBoundaries++;
				}

				// Ignore this box if there are not enough matching boundaries.
				if (nMatchingBoundaries < nRequiredMatchingBoundaries)
				{
					continue;
				}
			}

			if (bFoundBox)
			{
				// If we already have a qualifying candidate, determine which candidate is more
				// desireable.
				if (bFoundFourSidedBox && !groupBox.m_bIsFourSidedBox)
				{
					// If already have a box where four sides were found, prefer it over any box
					// with just 2 sides found.
					continue;
				}
				else if ((groupBox.m_Rect.Width() * groupBox.m_Rect.Height()) > 
						 (rrectResult.Width() * rrectResult.Height()))
				{
					// Existing qualifying result is smaller and thus prefered.
					// Ignore the latest result.
					continue;
				}
			}

			// Set the return value to true and set the dimensions of rrectResult.
			bFoundBox = true;
			bFoundFourSidedBox = groupBox.m_bIsFourSidedBox;
			rrectResult = LineRect(groupBox.m_Rect, rrectResult.IsHorizontal());
		}
	}

	return bFoundBox;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
bool LeadToolsLineGroup::assimilateLine(LeadToolsLineGroup *pGroupCandidate, 
										bool bRequireExact/* = false*/,
										set< pair<long, long> > *psetExistingPairs/* = NULL*/)
{
	ASSERT_ARGUMENT("ELI18724", pGroupCandidate != __nullptr);

	// Only attempt to assimilate individual lines
	if (pGroupCandidate->m_nLineCount > 1)
	{
		return false;
	}

	// If psetExistingPairs was provided and we are looking to combine the first two lines of a
	// group, check to see if the ID of the line in the existing group and the ID the line we
	// are testing have already been combined in another group. If so, do not not combine
	// these two lines into a new group-- assume the new group would have been a subset of
	// the existing group.
	pair<long, long> pairIDs(pGroupCandidate->m_LineRect.GetID(), m_LineRect.GetID());
	if (psetExistingPairs != __nullptr && m_nLineCount == 1 &&
		psetExistingPairs->find(pairIDs) != psetExistingPairs->end())
	{
			return false;
	}

	// Do not try to group lines longer than the max column width
	if (m_Settings.m_nColumnWidthMax != gnUNSPECIFIED && 
		pGroupCandidate->m_LineRect.LineLength() > m_Settings.m_nColumnWidthMax)
	{
		return false;
	}

	// Ensure there is no overlap of the line rects
	LineRect rectTestIntersection(m_Settings.m_bHorizontal);
	if (rectTestIntersection.IntersectRect(m_LineRect, pGroupCandidate->m_LineRect) == TRUE)
	{
		return false;
	}

	// Ensure we never attempt to assimilate a line above or in-line with the current line
	if (pGroupCandidate->m_LineRect.LinePosition() <= m_LineRect.LinePosition())
	{
		return false;
	}

	// At this point, create composite group so we can execute more sophisticated checks
	unique_ptr<LeadToolsLineGroup> apNewSubGroup(new LeadToolsLineGroup(*this));
	*this = LeadToolsLineGroup(*pGroupCandidate);
	m_apSubLineGroup = move(apNewSubGroup);

	// Assign new line count
	m_nLineCount = m_apSubLineGroup->m_nLineCount + 1;

	// Assign new line spacing
	m_nSpacing = m_LineRect.LinePosition() - m_apSubLineGroup->m_LineRect.LinePosition();

	// Don't try to group lines that don't fall within spacing specifications 
	if (m_nSpacing <= 0 ||
		(m_Settings.m_nSpacingMax != gnUNSPECIFIED && m_nSpacing > m_Settings.m_nSpacingMax))
	{
		return false;
	}

	int nSpacingScore = scoreLineSpacing(m_nSpacing);

	// Test that the spacing score at least minimially qualifies the line
	if (nSpacingScore < m_Settings.m_nSpacingScoreMin)
	{
		return false;
	}

	int nAlignmentScore = 
		scoreAlignment(m_LineRect.m_nLineTopOrLeftEnd, m_LineRect.m_nLineBottomOrRightEnd);

	// Test that the alignment score at least minimially qualifies the line
	if (nAlignmentScore < m_Settings.m_nAlignmentScoreMin)
	{
		return false;
	}

	// If neither the spacing nor the alignment score are exact, this line doesn't qualify
	if (nAlignmentScore < m_Settings.m_nAlignmentScoreExact && 
		nSpacingScore < m_Settings.m_nSpacingScoreExact)
	{
		return false;
	}

	// If either the spacing or alignment score is less than exact, require at least 3 lines
	// in the final group so that spacing can be properly evaluated
	if (nAlignmentScore < m_Settings.m_nAlignmentScoreExact || 
		nSpacingScore < m_Settings.m_nSpacingScoreExact)
	{
		if (bRequireExact)
		{
			// If bRequireExact is set to true and either one of the two scores was not exact,
			// disqualify the grouping immediately.
			return false;
		}
		else
		{
			// Otherwise require the group to have a minimum of 3 lines to ensure the line spacing
			// can properly evaluated.
			m_nLineMinimum = 3;
		}
	}

	// Create a new rect as a union of the new line and existing group rect
	m_Rect.UnionRect(m_LineRect, m_apSubLineGroup->m_Rect);
	
	// Ensure resulting region is bounded by the mid-points of the lines, not the line edges
	// (whose thickness may vary)
	if (m_nLineCount == 2)
	{
		m_Rect.m_nLineTopOrLeftEdge = m_apSubLineGroup->m_LineRect.LinePosition();
	}
	m_Rect.m_nLineBottomOrRightEdge = m_LineRect.LinePosition();

	// If psetExistingPairs was provided, keep track of the lines that have been paired.
	if (psetExistingPairs != __nullptr)
	{
		psetExistingPairs->insert(pairIDs);
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
bool LeadToolsLineGroup::assimilateColumn(LeadToolsLineGroup *pGroupCandidate)
{
	ASSERT_ARGUMENT("ELI19497", pGroupCandidate != __nullptr);

	// Require that a "column" consist of at least 2 lines
	if (m_nLineCount < 2)
	{
		return false;
	}

	// If the line counts differ by more than m_nColumnLineDiffAllowance, disqualify
	if (abs(m_nLineCount - pGroupCandidate->m_nLineCount) > m_Settings.m_nColumnLineDiffAllowance)
	{
		return false;
	}

	// Don't bother assimilating the column if it doesn't meet the column width requirements
	if (m_Settings.m_nColumnWidthMin != gnUNSPECIFIED && 
		(m_Rect.LineLength() < m_Settings.m_nColumnWidthMin || 
		pGroupCandidate->m_Rect.LineLength() < m_Settings.m_nColumnWidthMin))
	{
		return false;
	}

	// Ensure there is no overlap of the groups
	LineRect rectTestIntersection(m_Settings.m_bHorizontal);
	if (rectTestIntersection.IntersectRect(m_Rect, pGroupCandidate->m_Rect) == TRUE)
	{
		return false;
	}

	// Obtain alignment scores for both edges of the areas
	int nEdge1Alignment = 
		100 * abs(pGroupCandidate->m_Rect.m_nLineTopOrLeftEdge - m_Rect.m_nLineTopOrLeftEdge) / m_nSpacing;
	int nEdge2Alignment = 
		100 * abs(pGroupCandidate->m_Rect.m_nLineBottomOrRightEdge - m_Rect.m_nLineBottomOrRightEdge) / m_nSpacing;

	// If neither of the edges qualify, return false.
	if (nEdge1Alignment > m_Settings.m_nColumnAlignmentRequirement &&
		nEdge2Alignment > m_Settings.m_nColumnAlignmentRequirement)
	{
		return false;
	}

	// Get the average line spacing for scoring later on
	int nAverageSpacing = pGroupCandidate->getAverageSpacing();

	// At this point, create composite group so we can execute more sophisticated checks
	unique_ptr<LeadToolsLineGroup> apNewSubGroup(new LeadToolsLineGroup(*this));
	*this = LeadToolsLineGroup(*pGroupCandidate);
	m_apSubColumnGroup = move(apNewSubGroup);
	m_nColumnCount = m_apSubColumnGroup->m_nColumnCount + 1;

	// Obtain the space between columns
	if (m_Rect.m_nLineTopOrLeftEnd < m_apSubColumnGroup->m_Rect.m_nLineTopOrLeftEnd)
	{
		m_nColumnSpacing = m_apSubColumnGroup->m_Rect.m_nLineTopOrLeftEnd - m_Rect.m_nLineBottomOrRightEnd;
	}
	else
	{
		m_nColumnSpacing = m_Rect.m_nLineTopOrLeftEnd - m_apSubColumnGroup->m_Rect.m_nLineBottomOrRightEnd;
	}

	// Ensure the spacing is less than the maximum allowable
	if (m_Settings.m_nColumnSpacingMax != gnUNSPECIFIED && 
		m_nColumnSpacing > m_Settings.m_nColumnSpacingMax)
	{
		return false;
	}

	// Check that the spacing score meets the exact match qualification
	if (scoreLineSpacing(nAverageSpacing) < m_Settings.m_nSpacingScoreExact)
	{
		return false;
	}

	// Create a new rect as a union of the new line and existing group rect
	m_Rect.UnionRect(m_Rect, m_apSubColumnGroup->m_Rect);

	// Combine line minimum requirements from each column
	m_nLineMinimum += m_apSubColumnGroup->m_nLineMinimum;

	return true;
}
//-------------------------------------------------------------------------------------------------
int LeadToolsLineGroup::scoreLineSpacing(int nSpacing)
{
	ASSERT_ARGUMENT("ELI18575", nSpacing >= 0);

	if (m_nColumnCount == 1 && m_nLineCount == 2)
	{
		// If there are only 2 lines, there is nothing to compare to.  Return 100
		return 100;
	}
	else if (m_nColumnCount == 1 && m_nLineCount > 2)
	{
		// Obtain the minimum spacing score by comparing to all existing space values
		int nSpacingScore = 100 *
			min(nSpacing, m_apSubLineGroup->m_nSpacing) / 
			max(nSpacing, m_apSubLineGroup->m_nSpacing);

		int nSubSpacingScore = m_apSubLineGroup->scoreLineSpacing(nSpacing);

		// [FlexIDSCore:2744] min is a macro that evaluates each parameter twice. Using a recursive
		// function call (scoreLineSpacing) directly can cause a dramatic performance hit as
		// a group expands in size. Evaluate scoreLineSpacing into nSubSpacingScore before
		// calling min.
		nSpacingScore = min(nSpacingScore, nSubSpacingScore);

		return nSpacingScore;
	}
	else if (m_nColumnCount > 1)
	{
		int nAverageSpacing = m_apSubColumnGroup->getAverageSpacing();

		// Obtain the minimum spacing score by comparing to all the average spacing
		// of each existing column
		int nSpacingScore = 100 * min(nSpacing, nAverageSpacing) / max(nSpacing, nAverageSpacing);

		if (m_nColumnCount > 2)
		{
			int nSubSpacingScore = m_apSubColumnGroup->scoreLineSpacing(nSpacing);

			// [FlexIDSCore:2744] min is a macro that evaluates each parameter twice. Using a recursive
			// function call (scoreLineSpacing) directly can cause a dramatic performance hit as
			// a group expands in size. Evaluate scoreLineSpacing into nSubSpacingScore before
			// calling min.
			nSpacingScore = min(nSpacingScore, nSubSpacingScore);
		}

		return nSpacingScore;
	}
	else
	{
		THROW_LOGIC_ERROR_EXCEPTION("ELI18578");
	}
}
//-------------------------------------------------------------------------------------------------
int LeadToolsLineGroup::scoreAlignment(int nTopOrLeftEnd, int nBottomOrRightEnd)
{
	ASSERT_ARGUMENT("ELI18579", nTopOrLeftEnd >= 0);
	ASSERT_ARGUMENT("ELI18592", nBottomOrRightEnd >= nTopOrLeftEnd);

	// Obtain combined width of both lines
	int nTotalLength =	max(m_LineRect.m_nLineBottomOrRightEnd, nBottomOrRightEnd) -
						min(m_LineRect.m_nLineTopOrLeftEnd, nTopOrLeftEnd);

	// Do not allow nTotalLength to be zero since we divide by this value later on.
	if (nTotalLength == 0)
	{
		return 0;
	}

	// Obtain width of the vertical intersection of the lines
	int nIntersectionNegative = max(m_LineRect.m_nLineTopOrLeftEnd, nTopOrLeftEnd);
	int nIntersectionPositive = min(m_LineRect.m_nLineBottomOrRightEnd, nBottomOrRightEnd);
	int nIntersectionLength = nIntersectionPositive - nIntersectionNegative;

	// Return the lowest alignment score by comparing to each of the existing lines
	int nAlignmentScore = 100 * nIntersectionLength / nTotalLength;

	// Keep track of the line end ids that would need to be assigned to a qualifying group.
	int nNewTopOrLeftEndId = gnUNSPECIFIED;
	int nNewBottomOrRightEndId = gnUNSPECIFIED;

	if (m_apSubLineGroup.get() != __nullptr)
	{
		int nSubAlignmentScore = m_apSubLineGroup->scoreAlignment(nTopOrLeftEnd, nBottomOrRightEnd);

		// [FlexIDSCore:2744] min is a macro that evaluates each parameter twice. Using a recursive
		// function call (scoreAlignment) directly can cause a dramatic performance hit as
		// a group expands in size. Evaluate scoreAlignment into nSubAlignmentScore before
		// calling min.
		nAlignmentScore = min(nAlignmentScore, nSubAlignmentScore);

		// Take line end IDs into account and update the alignment score as appropriate
		compareLineEndIDs(nAlignmentScore);
	}

	return nAlignmentScore;
}
//-------------------------------------------------------------------------------------------------
void LeadToolsLineGroup::compareLineEndIDs(int &rnAlignmentScore)
{
	// Don't bother evaluating if the minimum aligment score wasn't met
	if (rnAlignmentScore >= m_Settings.m_nAlignmentScoreMin)
	{
		if (m_nLineTopOrLeftEndId != gnUNSPECIFIED &&
			m_nLineTopOrLeftEndId == m_apSubLineGroup->m_nLineTopOrLeftEndId)
		{
			// If the two lines in question share the same m_nLineTopOrLeftEndId,
			// score an exact alignent.
			rnAlignmentScore = 100;
		}
		else if (m_nLineTopOrLeftEndId != gnUNSPECIFIED &&
			     m_apSubLineGroup->m_nLineTopOrLeftEndId != gnUNSPECIFIED)
		{
			// If the left ends of these lines were formed via intersections with different
			// lines, do not consider these lines for grouping.
			rnAlignmentScore = 0;
			return;
		}
		else
		{
			// Line end ids do not match, m_nLineTopOrLeftEndId should not be specified
			m_nLineTopOrLeftEndId = gnUNSPECIFIED;
		}

		if (m_nLineBottomOrRightEndId != gnUNSPECIFIED &&
			m_nLineBottomOrRightEndId == m_apSubLineGroup->m_nLineBottomOrRightEndId)
		{
			// If the two lines in question share the same m_nLineBottomOrRightEndId,
			// score an exact alignent.
			rnAlignmentScore = 100;
		}
		else if (m_nLineBottomOrRightEndId != gnUNSPECIFIED &&
			     m_apSubLineGroup->m_nLineBottomOrRightEndId != gnUNSPECIFIED)
		{
			// If the right ends of these lines were formed via intersections with different
			// lines, do not consider these lines for grouping.
			rnAlignmentScore = 0;
			return;
		}
		else
		{
			// Line end ids do not match, m_nLineTopOrLeftEndId should not be specified
			m_nLineTopOrLeftEndId = gnUNSPECIFIED;
		}
	}
}
//-------------------------------------------------------------------------------------------------
bool LeadToolsLineGroup::qualified(bool bMinimally/* = false*/)
{
	// Check minimum line count, column count, column width and overall width.
	if (getTotalLineCount() < m_nLineMinimum ||
		(m_Settings.m_nLineCountMin != gnUNSPECIFIED && m_nLineCount < m_Settings.m_nLineCountMin) ||
		(m_Settings.m_nColumnCountMin != gnUNSPECIFIED && m_nColumnCount < m_Settings.m_nColumnCountMin) ||
		(m_Settings.m_nColumnWidthMin != gnUNSPECIFIED && m_Rect.LineLength() < m_Settings.m_nColumnWidthMin) ||
		(m_Settings.m_nOverallWidthMin != gnUNSPECIFIED && m_Rect.LineLength() < m_Settings.m_nOverallWidthMin))
	{
		return false;
	}

	if (!bMinimally)
	{
		// If checking all qualifications, also test maximum line count, column count and overall width
		if ((m_Settings.m_nSpacingMin != gnUNSPECIFIED && m_nSpacing < m_Settings.m_nSpacingMin) ||
			(m_Settings.m_nLineCountMax != gnUNSPECIFIED && m_nLineCount > m_Settings.m_nLineCountMax) ||
			(m_Settings.m_nColumnCountMax != gnUNSPECIFIED && m_nColumnCount > m_Settings.m_nColumnCountMax) ||
			(m_Settings.m_nOverallWidthMax != gnUNSPECIFIED && m_Rect.LineLength() > m_Settings.m_nOverallWidthMax))
		{
			return false;
		}
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
void LeadToolsLineGroup::getLineRects(vector<LineRect> &rvecLineRects) const
{
	if (m_apSubColumnGroup.get() != __nullptr)
	{
		// Add line rects from any sub-columns
		m_apSubColumnGroup->getLineRects(rvecLineRects);
	}
	
	if (m_apSubLineGroup.get() != __nullptr)
	{
		// Add line rects from sub groups within this column
		m_apSubLineGroup->getLineRects(rvecLineRects);
	}

	// Add the line rect for this group
	rvecLineRects.push_back(m_LineRect);
}
//-------------------------------------------------------------------------------------------------
int LeadToolsLineGroup::getAverageSpacing()
{
	int nRes = 0;

	// Calculate the average spacing
	if (m_nLineCount >= 2)
	{
		nRes = m_nSpacing; 
		nRes += m_apSubLineGroup->getAverageSpacing() * (m_nLineCount - 2);
		nRes /= (m_nLineCount - 1);
	}

	return nRes;
}
//-------------------------------------------------------------------------------------------------
int LeadToolsLineGroup::getTotalLineCount()
{
	int nTotal = m_nLineCount;

	// Combine line count for this column with sub columns
	if (m_apSubColumnGroup.get() != __nullptr)
	{
		nTotal += m_apSubColumnGroup->getTotalLineCount();
	}

	return nTotal;
}
//-------------------------------------------------------------------------------------------------
bool LeadToolsLineGroup::groupLinesIntoColumns(list<LeadToolsLineGroup> &rlistGroups, bool bBoxBoundsOnly/* = false*/)
{
	// Keep track of the line pairs that have been combined into a group so that we don't needlessly
	// create sub-groups.
	set< pair<long, long> > setExistingPairs;

	// Assuming groups ordered from top to bottom for efficiency
	for (list<LeadToolsLineGroup>::iterator iterGroup_i = rlistGroups.begin();
		iterGroup_i != rlistGroups.end();
		iterGroup_i++)
	{
		// If we are looking only for box bounds, don't try to group more than 2 lines together
		if (bBoxBoundsOnly && iterGroup_i->m_nLineCount > 1)
		{
			continue;
		}

		// Keep track of the vertical position of the last assimilated line
		int nLastAssimilatedLinePos = -1;

		// For each group, cycle through all lines below this one
		list<LeadToolsLineGroup>::iterator iterGroup_j = iterGroup_i;
	
		// Don't try to assimilate with self
		iterGroup_j ++;

		// Keep track of the number of children created from this group.
		int nChildrenCreated = 0;

		while (iterGroup_j != rlistGroups.end())
		{
			// Create a clone of the current group from the outside iteration
			LeadToolsLineGroup newGroup = *iterGroup_i;

			// If we are looking only for box bounds, do not attempt to assimilate lines
			// any further down the page than the first one that was assimilated with this group.
			if (bBoxBoundsOnly && nLastAssimilatedLinePos != -1 &&
				nLastAssimilatedLinePos < iterGroup_j->m_LineRect.LinePosition())
			{
				break;
			}

			// Attempt to assimilate the group from the inside iteration into it
			// We have an iterator here, not a true pointer; thus the need to reference a dereference
			if (newGroup.assimilateLine(&*iterGroup_j, bBoxBoundsOnly, 
										bBoxBoundsOnly ? NULL : &setExistingPairs))
			{
				nChildrenCreated++;

				// If so, insert the new group immediately after the group in the primary
				// iteration. (so it will be encountered later in the primary iteration)
				list<LeadToolsLineGroup>::iterator iterGroup_iPlus = iterGroup_i;
				iterGroup_iPlus++;
				rlistGroups.insert(iterGroup_iPlus,newGroup);

				// Keep track of the vertical position of the last assimilated line
				nLastAssimilatedLinePos = newGroup.m_LineRect.LinePosition();

				// [FlexIDSCore:3066] To help slow the potential of this algrorithm to 
				// grow groupings exponetially, only allow at most gnMAX_GROUP_CHILDREN 
				// new groups to be formed by matching this group with new lines.
				if (nChildrenCreated >= gnMAX_GROUP_CHILDREN)
				{
					break;
				}

				// [FlexIDSCore:3066] Cuttoff the line grouping algorithm after 
				// gnMAX_GROUPINGS_PER_PAGE groupings have been formed.  This prevents
				// processing from hanging or running out of memory.
				if (rlistGroups.size() >= gnMAX_GROUPINGS_PER_PAGE)
				{
					return false;
				}
			}

			// If assimilation fails, let the newGroup clone fall out of scope
			iterGroup_j++;
		}
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
void LeadToolsLineGroup::groupColumnsIntoAreas(list<LeadToolsLineGroup> &rlistGroups)
{
	for (list<LeadToolsLineGroup>::iterator iterGroup_i = rlistGroups.begin();
		 iterGroup_i != rlistGroups.end();
		 iterGroup_i++)
	{
		// Don't try to compile any groups with less than 2 lines as a column
		if (iterGroup_i->m_nLineCount < 2)
		{
			continue;
		}

		// For each group, cycle through all groups below this group
		list<LeadToolsLineGroup>::iterator iterGroup_j = iterGroup_i;
		
		// Don't try to assimilate with self
		iterGroup_j ++;

		while (iterGroup_j != rlistGroups.end())
		{
			// Create a clone of the current group from the outside iteration
			LeadToolsLineGroup newGroup = *iterGroup_i;

			// Attempt to assimilate the group from the inside iteration into it
			// We have an iterator here, not a true pointer; thus the need to reference a dereference
			if (newGroup.assimilateColumn(&*iterGroup_j))
			{
				// If so, insert the new group immediately after the group in the primary
				// iteration (so it will be encountered later in the primary iteration)
				list<LeadToolsLineGroup>::iterator iterGroup_iPlus = iterGroup_i;
				iterGroup_iPlus++;
				rlistGroups.insert(iterGroup_iPlus,newGroup);
			}

			iterGroup_j ++;
		}
	}
}
//-------------------------------------------------------------------------------------------------
void LeadToolsLineGroup::qualifyAreas(list<LeadToolsLineGroup> &rListGroups,
									  bool bPreferLargerRegion/* = true*/)
{
	// Check group for mimimum qualification.  Do not throw out groups for maximum qualification
	// offenses at this point-- we want to "soak up" all derivative groups before testing
	// the most complete versions for maximum qualification violations
	list<LeadToolsLineGroup>::iterator iterGroup_i = rListGroups.begin();
	while (iterGroup_i != rListGroups.end())
	{
		if (!iterGroup_i->qualified(true))
		{
			// Group doesn't meet minimum standards.  Discard.
			list<LeadToolsLineGroup>::iterator iterToDelete = iterGroup_i;
			iterGroup_i = rListGroups.erase(iterToDelete);
		}
		else
		{
			iterGroup_i++;
		}
	}

	// Look for duplicate groups or groups that are entirely or substantially contained in
	// other groups and discard as appropriate.
	iterGroup_i = rListGroups.begin();
	while (iterGroup_i != rListGroups.end())
	{
		// Cycle through every group in the inside loop to look for derivative groups
		list<LeadToolsLineGroup>::iterator iterGroup_j = rListGroups.begin();

		while (iterGroup_i != rListGroups.end() && 
			   iterGroup_j != rListGroups.end())
		{
			// Test the two groups to see if one should be thrown out in favor of the other
			// The iterators are updated as appropriate
			pareArea(rListGroups, iterGroup_i, iterGroup_j, bPreferLargerRegion);
		}
	}

	// Finally traverse the list one more time, this time making sure groups are both
	// minimally and maximally qualified. 
	iterGroup_i = rListGroups.begin();
	while (iterGroup_i != rListGroups.end())
	{
		if (!iterGroup_i->qualified())
		{
			list<LeadToolsLineGroup>::iterator iterToDelete = iterGroup_i;
			iterGroup_i = rListGroups.erase(iterToDelete);
		}
		else
		{
			iterGroup_i++;
		}
	}
}
//-------------------------------------------------------------------------------------------------
void LeadToolsLineGroup::pareArea(list<LeadToolsLineGroup> &rListGroups,
								  list<LeadToolsLineGroup>::iterator &rIterGroup_i,
								  list<LeadToolsLineGroup>::iterator &rIterGroup_j,
								  bool bPreferLargerRegion/* = true*/)
{
	bool bDelete_i = false;
	bool bDelete_j = false;

	// Don't test against self
	if (rIterGroup_i != rIterGroup_j)
	{
		// What percent of group i and j are contained in the overlapping area?
		int nPercentInclusion_i, nPercentInclusion_j;
		calculateIntersection(*rIterGroup_i, *rIterGroup_j, nPercentInclusion_i, nPercentInclusion_j);

		if (nPercentInclusion_i > m_Settings.m_nCombineGroupPercentage ||
			nPercentInclusion_j > m_Settings.m_nCombineGroupPercentage)
		{
			// [FlexIDS:3865]
			// A box where all 4 sides have been found should always be prefered over a box where
			// only 2 sides have been found.
			if (rIterGroup_i->m_bIsFourSidedBox && !rIterGroup_j->m_bIsFourSidedBox)
			{
				bDelete_j = true;
			}
			else if (rIterGroup_j->m_bIsFourSidedBox && !rIterGroup_i->m_bIsFourSidedBox)
			{
				bDelete_i = true;
			}
			else if (nPercentInclusion_i > m_Settings.m_nCombineGroupPercentage &&
				nPercentInclusion_j > m_Settings.m_nCombineGroupPercentage)
			{
				// If both areas are sufficiently contained in the overlap area, prefer
				// the area with more lines.
				if (rIterGroup_i->m_nLineCount > rIterGroup_j->m_nLineCount)
				{
					bDelete_j = true;
				}
				else if (rIterGroup_j->m_nLineCount > rIterGroup_i->m_nLineCount)
				{
					bDelete_i = true;
				}
			}

			if (bDelete_i == false && bDelete_j == false)
			{
				// If either of the areas are sufficiently contained in the overlap area,
				// use bPreferLargerRegion to determine which group to toss out.
				if (bPreferLargerRegion)
				{
					bDelete_i = (nPercentInclusion_i > nPercentInclusion_j);
					bDelete_j = !bDelete_i;
				}
				else
				{
					bDelete_i = (nPercentInclusion_i < nPercentInclusion_j);
					bDelete_j = !bDelete_i;
				}
			}
		}
	}

	// Delete rIterGroup_i or rIterGroup_j as necessary.
	if (bDelete_i)
	{
		list<LeadToolsLineGroup>::iterator iterToDelete = rIterGroup_i;
		rIterGroup_i = rListGroups.erase(iterToDelete);

		// Given that the outside loop's iterator has changed, reset the secondary iterator
		rIterGroup_j = rListGroups.begin();
	}
	else if (bDelete_j)
	{
		list<LeadToolsLineGroup>::iterator iterToDelete = rIterGroup_j;
		rIterGroup_j = rListGroups.erase(iterToDelete);

		if (rIterGroup_j == rListGroups.end())
		{
			// If the secondary iterator is at the end of the list, increment the primary iterator
			// and reset the secondary iterator.
			rIterGroup_i++;
			rIterGroup_j = rListGroups.begin();
		}
	}
	else
	{
		// Nothing was deleted.  Increment the secondary iterator.
		rIterGroup_j++;

		if (rIterGroup_j == rListGroups.end())
		{
			// If the secondary iterator is at the end of the list, increment the primary iterator
			// and reset the secondary iterator.
			rIterGroup_i++;
			rIterGroup_j = rListGroups.begin();
		}
	}
}
//-------------------------------------------------------------------------------------------------
void LeadToolsLineGroup::appendBoxResults(list<LeadToolsLineGroup>& rlistExistingBoxes,
										  list<LeadToolsLineGroup> listBoxesToAppend)
{
	// Compare every box from rlistExistingBoxes to the ones in listBoxesToAppend to find boxes that
	// were found in both orientations (ie, all 4 sides have been identified).
	list<LeadToolsLineGroup>::iterator iterGroup_i = rlistExistingBoxes.begin();
	while (iterGroup_i != rlistExistingBoxes.end())
	{
		list<LeadToolsLineGroup>::iterator iterGroup_j = listBoxesToAppend.begin();

		while (iterGroup_i != rlistExistingBoxes.end() && 
			   iterGroup_j != listBoxesToAppend.end())
		{
			// Test to see if the boxes are similar enough to consider them the same box.
			int nPercentInclusion_i, nPercentInclusion_j;
			calculateIntersection(*iterGroup_i, *iterGroup_j, nPercentInclusion_i, nPercentInclusion_j);

			if (nPercentInclusion_i > m_Settings.m_nCombineGroupPercentage &&
				nPercentInclusion_j > m_Settings.m_nCombineGroupPercentage)
			{
				// Delete the smaller of the two boxes (though they will likely be the same size)
				// and mark the larger box as 4 sided.
				if (nPercentInclusion_i < nPercentInclusion_j)
				{
					list<LeadToolsLineGroup>::iterator iterToDelete = iterGroup_i;
					iterGroup_i = rlistExistingBoxes.erase(iterToDelete);

					iterGroup_j->m_bIsFourSidedBox = true;

					// Given that rlistExistingBoxes has changed, reset listBoxesToAppend's iterator
					iterGroup_j = listBoxesToAppend.begin();
				}
				else
				{
					list<LeadToolsLineGroup>::iterator iterToDelete = iterGroup_j;
					iterGroup_j = listBoxesToAppend.erase(iterToDelete);

					iterGroup_i->m_bIsFourSidedBox = true;
				}

				continue;
			}

			iterGroup_j++;
		}

		if (iterGroup_i == rlistExistingBoxes.end())
		{
			break;
		}
	
		iterGroup_i++;
	}

	// Combine the remaining boxes into rlistExistingBoxes
	rlistExistingBoxes.splice(rlistExistingBoxes.end(), listBoxesToAppend);
}
//-------------------------------------------------------------------------------------------------
void LeadToolsLineGroup::calculateIntersection(const LeadToolsLineGroup& group1,
			const LeadToolsLineGroup& group2, int& rnPercentInclusion_1, int &rnPercentInclusion_2)
{
	// Initialize inclusion to zero
	rnPercentInclusion_1 = 0;
	rnPercentInclusion_2 = 0;
	
	// Test to see if they overlap at all.
	LineRect rectTest = group1.m_Rect;
	if (rectTest.IntersectRect(group1.m_Rect, group2.m_Rect))
	{
		// What percent of group 1 is contained in the overlapping area?
		rnPercentInclusion_1 = 100 * (rectTest.Width() * rectTest.Height()) /
			(group1.m_Rect.Width() * group1.m_Rect.Height());

		// What percent of group 2 is contained in the overlapping area?
		rnPercentInclusion_2 = 100 * (rectTest.Width() * rectTest.Height()) /
			(group2.m_Rect.Width() * group2.m_Rect.Height());
	}
}
//-------------------------------------------------------------------------------------------------
void LeadToolsLineGroup::findLinesAroundRect(const LineRect &rect, vector<LineRect> &rvecHorzLineRects,
											 vector<LineRect> &rvecVertLineRects)
{
	// Sort the incoming lines from top down and left to right for efficiency.
	sort(rvecHorzLineRects.begin(), rvecHorzLineRects.end(), isLineBelow);
	sort(rvecVertLineRects.begin(), rvecVertLineRects.end(), isLineBelow);

	// Create a LineRect representing the specified rect oriented both vertically and horizontally.
	LineRect rectHorizontal(true);
	LineRect rectVertical(false);

	if (rect.IsHorizontal())
	{
		rectHorizontal = LineRect(rect);
		rectVertical = LineRect((CRect) rect, false);
	}
	else
	{
		rectHorizontal = LineRect((CRect) rect, true);
		rectVertical = LineRect(rect);
	}

	// Create variables to receive the indexes of the closest lines as well as rects representing
	// the points at which the lines would have intersected.
	set<int> setClosestHorzIndexes;
	set<int> setClosestVertIndexes;
	vector<CRect> vecClosestIntersections;

	// Find the 2 closest vertical lines (if they exist)
	findClosestLines(rectHorizontal, rvecVertLineRects, setClosestVertIndexes, &vecClosestIntersections);

	// Find the 2 closest horizontal lines (if they exist)
	findClosestLines(rectVertical, rvecHorzLineRects, setClosestHorzIndexes, &vecClosestIntersections);

	// For each of the 4 possible intersections, search for the 4 closest lines.
	// (one in each direction: up, down, left, right)
	for (size_t i = 0; i < vecClosestIntersections.size(); i++)
	{
		// Search for the 2 closest horizontal lines
		LineRect rectHorzIntersection(vecClosestIntersections[i], true);
		findClosestLines(rectHorzIntersection, rvecVertLineRects, setClosestVertIndexes);

		// Search for the 2 closest vertical lines
		LineRect rectVertIntersection(vecClosestIntersections[i], false);
		findClosestLines(rectVertIntersection, rvecHorzLineRects, setClosestHorzIndexes);
	}

	// Remove any lines from rvecHorzLineRects whose indexes are not found in setClosestHorzIndexes
	for (int i = rvecHorzLineRects.size() - 1; i >= 0; i--)
	{
		if (setClosestHorzIndexes.find(i) == setClosestHorzIndexes.end())
		{
			rvecHorzLineRects.erase(rvecHorzLineRects.begin() + i);
		}
	}

	// Remove any lines from rvecVertLineRects whose indexes are not found in setClosestVertIndexes
	for (int i = rvecVertLineRects.size() - 1; i >= 0; i--)
	{
		if (setClosestVertIndexes.find(i) == setClosestVertIndexes.end())
		{
			rvecVertLineRects.erase(rvecVertLineRects.begin() + i);
		}
	}
}
//-------------------------------------------------------------------------------------------------
void LeadToolsLineGroup::findClosestLines(const LineRect &rectStart, 
										  const vector<LineRect> &vecLineRects, 
										  set<int> &rsetClosestLines, 
										  vector<CRect> *pvecClosestIntersections/* = NULL*/)
{
	// Variables to keep track of the indexes of lines that are found.
	int nTopOrLeftIndex = -1;
	int nBottomOrRightIndex = -1;

	for (size_t i = 0; i < vecLineRects.size(); i++)
	{
		// Test to see if the lengthwise plane of rectStart intersects with the rect at this index 
		// at any point.
		if (vecLineRects[i].m_nLineBottomOrRightEnd > rectStart.LinePosition() &&
			vecLineRects[i].m_nLineTopOrLeftEnd < rectStart.LinePosition())
		{
			if (vecLineRects[i].LinePosition() > rectStart.LineMiddle())
			{
				// The plane is intersected to the bottom or right of rectStart.
				nBottomOrRightIndex = i;
				break;
			}
			else if (vecLineRects[i].LinePosition() < rectStart.LineMiddle())
			{
				// The plane is intersected to the top or left of rectStart. Continue to reset
				// this index until lines are encountered that intersect or are to the bottom or 
				// right of rectStart
				nTopOrLeftIndex = i;
			}
		}
	}

	if (nTopOrLeftIndex != -1)
	{
		// The line was intersected to the top or left
		rsetClosestLines.insert(nTopOrLeftIndex);

		if (pvecClosestIntersections)
		{
			// If requested, create a line to represent the point of intersection
			LineRect rectIntersection(vecLineRects[nTopOrLeftIndex]);
			rectIntersection.m_nLineTopOrLeftEnd = rectStart.m_nLineTopOrLeftEdge;
			rectIntersection.m_nLineBottomOrRightEnd = rectStart.m_nLineBottomOrRightEdge;
			pvecClosestIntersections->push_back((CRect) rectIntersection);
		}
	}

	if (nBottomOrRightIndex != -1)
	{
		// The line was intersected to the bottom or right
		rsetClosestLines.insert(nBottomOrRightIndex);

		if (pvecClosestIntersections)
		{
			// If requested, create a line to represent the point of intersection
			LineRect rectIntersection(vecLineRects[nBottomOrRightIndex]);
			rectIntersection.m_nLineTopOrLeftEnd = rectStart.m_nLineTopOrLeftEdge;
			rectIntersection.m_nLineBottomOrRightEnd = rectStart.m_nLineBottomOrRightEdge;
			pvecClosestIntersections->push_back((CRect) rectIntersection);
		}
	}
}
//-------------------------------------------------------------------------------------------------
void LeadToolsLineGroup::splitLines(list<LeadToolsLineGroup> &rlistPrimaryLines,
									vector<LineRect> &rvecSplitterLines)
{
	// Cycle through each of the primary lines
	for (list<LeadToolsLineGroup>::iterator iterGroup_i = rlistPrimaryLines.begin();
			iterGroup_i != rlistPrimaryLines.end();
			iterGroup_i++)
	{
		list<LeadToolsLineGroup>::iterator iterGroup_iPlus = iterGroup_i;
		iterGroup_iPlus++;

		// Test each perpendicular line for intersection.
		for (size_t i = 0; i < rvecSplitterLines.size(); i++)
		{
			// Determine the point at which the line should be split
			int nLinePosition = iterGroup_i->m_LineRect.LinePosition();
			int nSplitPosition = rvecSplitterLines[i].LinePosition();

			if (iterGroup_i->m_LineRect.m_nLineTopOrLeftEnd - gnLINE_INTERSECT_ALLOWANCE < nSplitPosition &&
				iterGroup_i->m_LineRect.m_nLineBottomOrRightEnd + gnLINE_INTERSECT_ALLOWANCE > nSplitPosition &&
				rvecSplitterLines[i].m_nLineTopOrLeftEnd - gnLINE_INTERSECT_ALLOWANCE < nLinePosition &&
				rvecSplitterLines[i].m_nLineBottomOrRightEnd + gnLINE_INTERSECT_ALLOWANCE > nLinePosition)
			{
				if (iterGroup_i->m_LineRect.m_nLineTopOrLeftEnd < nSplitPosition &&
					iterGroup_i->m_LineRect.m_nLineBottomOrRightEnd > nSplitPosition)
				{
					// Splitter line intersects primary line; 
					// Create new left-hand line
					LineRect newLine((*iterGroup_i).m_LineRect);
					newLine.m_nLineBottomOrRightEnd = nSplitPosition;

					LeadToolsLineGroup newGroup(newLine, m_Settings);
					newGroup.m_nLineBottomOrRightEndId = (int) i;
					newGroup.m_nLineTopOrLeftEndId = iterGroup_i->m_nLineTopOrLeftEndId;
					rlistPrimaryLines.insert(iterGroup_i,newGroup);

					// Create new right-hand line
					LineRect newLine2((*iterGroup_i).m_LineRect);
					newLine2.m_nLineTopOrLeftEnd = nSplitPosition;

					LeadToolsLineGroup newGroup2(newLine2, m_Settings);
					newGroup2.m_nLineTopOrLeftEndId = (int) i;
					newGroup2.m_nLineBottomOrRightEndId = iterGroup_i->m_nLineBottomOrRightEndId;
					rlistPrimaryLines.insert(iterGroup_iPlus,newGroup2);
				}
				else if (iterGroup_i->m_LineRect.m_nLineBottomOrRightEnd <= nSplitPosition)
				{
					// Primary line is to left of the splitter line
					// extend the line to the splitter line.
					if (iterGroup_i->m_nLineBottomOrRightEndId == gnUNSPECIFIED)
					{
						iterGroup_i->m_LineRect.m_nLineBottomOrRightEnd = nSplitPosition;
						iterGroup_i->m_Rect.m_nLineBottomOrRightEnd = nSplitPosition;
						iterGroup_i->m_nLineBottomOrRightEndId = (int) i;
					}
				}
				else if (iterGroup_i->m_LineRect.m_nLineTopOrLeftEnd >= nSplitPosition)
				{
					// Primary line is to the right of the splitter line
					// extend the line to the splitter line.
					if (iterGroup_i->m_nLineTopOrLeftEndId == gnUNSPECIFIED)
					{
						iterGroup_i->m_LineRect.m_nLineTopOrLeftEnd = nSplitPosition;
						iterGroup_i->m_Rect.m_nLineTopOrLeftEnd = nSplitPosition;
						iterGroup_i->m_nLineTopOrLeftEndId = (int) i;
					}
				}
				else
				{
					THROW_LOGIC_ERROR_EXCEPTION("ELI19903");
				}
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------