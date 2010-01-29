#pragma once

#include "LeadUtils.h"
#include "LineRect.h"

#include <string>
#include <vector>
#include <algorithm>
#include <list>
#include <set>
using namespace std;

///////////////////////////
// LeadToolsLineGroup
///////////////////////////
class LEADUTILS_API LeadToolsLineGroup
{
public:
	// Settings that govern how lines are grouped together
	struct Settings
	{
		// Minimum number of lines per column
		long m_nLineCountMin;

		// Maximum number of lines per column (accepts -1 for unspecified)
		long m_nLineCountMax;

		// Minimum number of columns per area (accepts -1 for unspecified)
		long m_nColumnCountMin;

		// Maximum number of columns per area (accepts -1 for unspecified)
		long m_nColumnCountMax;

		// The percentage of overlap (overlap / combined width) that must exist
		// to be considered for grouping (0 - 100)
		long m_nAlignmentScoreMin;

		// The percentage of overlap (overlap / combined width) that must exist
		// to overcome a spacing score of < m_nSpacingScoreExact (0 - 100)
		long m_nAlignmentScoreExact;

		// The similarity in line spacing (new spacing / existing spacing) that 
		// must exist to be considered for grouping (0 - 100)
		long m_nSpacingScoreMin;

		// The similarity in line spacing (new spacing / existing spacing) that 
		// must exist to overcome an alignment score of < m_nAlignmentScoreExact. (0 - 100)
		long m_nSpacingScoreExact;

		// The minimum required spacing between lines in pixels (accepts -1 for unspecified)
		long m_nSpacingMin;

		// The maximum allowed spacing between lines in pixels (accepts -1 for unspecified)
		long m_nSpacingMax;

		// The maximum allowed spacing between columns in pixels (accepts -1 for unspecified)
		// Based on the area between the columns, not the overall column width
		long m_nColumnSpacingMax;

		// The minimun required width of a column in pixels (accepts -1 for unspecified)
		long m_nColumnWidthMin;

		// The maximum allowed width of a column in pixels (accepts -1 for unspecified)
		long m_nColumnWidthMax;

		// The overall required width of an area in pixels (accepts -1 for unspecified)
		long m_nOverallWidthMin;

		// The overall allowed width of an area in pixels (accepts -1 for unspecified)
		long m_nOverallWidthMax;

		// The percentage of one area that must be included in another area for it to be 
		// considered a subset of the larger area (0 - 100)
		long m_nCombineGroupPercentage;

		// The percentage of row height by which a column can be vertically offset
		// with another and be considered for grouping (0 - 100)
		long m_nColumnAlignmentRequirement;

		// The allowable difference in the number of rows between columns considered for grouping
		long m_nColumnLineDiffAllowance;

		bool m_bHorizontal;

	} m_Settings;

	LeadToolsLineGroup();
	LeadToolsLineGroup(LineRect rectLine, const Settings &settings);
	LeadToolsLineGroup(const LeadToolsLineGroup &group);
	~LeadToolsLineGroup();

	LeadToolsLineGroup& operator =(LeadToolsLineGroup &group);

	// PROMISE: Group provided line rects together into image areas
	// ARGS:	rvecLineRects- A vector of LineRects representing lines in an image
	//						   NOTE: The rects will be reordered in the vector following this call
	//			rGroupRects-   A return vector of LineRects representing image areas that meet the 
	//						   requirements specified in m_Settings
	//			rectBounds-	   Specifies the bounds in which all return values must be included.
	//						   Any region found overlapping these bounds will be clipped so that
	//						   the included portion is returned.
	//			pvecSubLineRects- Can be NULL.  If specified, it is used to return
	//				a vector of LineRect vectors.  Each LineRect vector corresponds to the LineRect
	//				area that was returned at the same index.
	// RETURNS: true if processing completed successfully.  false indicates the algorithm 
	//			short-circuited after encountering too many potential line grouping permutations.
	//			In this case the results may be still useful, but also may not be as accurate
	//			or complete as they otherwise would.
	bool groupLines(vector<LineRect> &rvecLineRects, 
					vector<LineRect> &rGroupRects,
					CRect rectBounds,
					vector< vector<LineRect> > *pvecSubLineRects = NULL);

	// PROMISE: Find boxes that can be formed by grouping the provided line rects.
	// ARGS:	rvecHorzLineRects- A vector of horizontal LineRects to use.
	//			rvecVertLineRects- A vector of vertical LineRects to use.
	//			rlistFoundBoxes- Returns the boxes that can be formed with the given lines.
	// NOTE:	rvecHorzLineRects will be sorted from top down and rvecVertLineRects
	//			will be sorted from left to right following this call.
	// WARNING: Do not call this function with an indeterminant number of lines.  
	//			Processing time will increase exponentially with the number of intersections
	//			found.  Performance will be noticeably affected if there 10 or more intersection
	//			of a single horizontal line (or nearly that many for multiple horizontal lines).
	// RETURNS: true if processing completed successfully.  false indicates the algorithm 
	//			short-circuited after encountering too many potential boxes. In this case the
	//			results may be still useful, but also may not be as accurate or complete as they
	//			otherwise would.
	bool findBoxes(vector<LineRect> &rvecHorzLineRects,
				   vector<LineRect> &rvecVertLineRects,
				   list<LeadToolsLineGroup> &rlistFoundBoxes);

	// PROMISE: Find a box that contains the provided LineRect which can be formed with the provided 
	//			lines.
	// ARGS:	vecHorzLineRects- A vector of horizontal LineRects to use.
	//			vecVertLineRects- A vector of vertical LineRects to use.
	//			nRequiredMatchingBoundaries- The number of boundaries of the provided LineRect
	//				that must line up with the boundaries of a resulting box
	//			rrectResult- If a qualifying box is found, rrectResult will be set accordingly.
	//			rbIncompleteResult- true if the function did not successfully complete processing.
	//				In this case the results may be still useful, but also may not be as accurate or
	//				complete as they otherwise would.
	// RETURNS: true if a qualifying box was found, false otherwise
	bool findBoxContainingRect(const LineRect &rect, vector<LineRect> &rvecHorzLineRects,
							   vector<LineRect> &rvecVertLineRects, int nRequiredMatchingBoundaries, 
							   LineRect &rrectResult, bool &rbIncompleteResult);
	
private:
	/////////////////
	// Variables
	/////////////////
	
	// Rect representing the most recently added line
	LineRect m_LineRect;

	// Rect representing the entire group area
	LineRect m_Rect;

	// Distance in pixels of most recently added line to the previous line
	long m_nSpacing;

	// Distance in pixels between the most recently added column group and the
	// previous column group
	long m_nColumnSpacing;

	// Line count (current column only-- use getTotalLineCount for all lines in group)
	long m_nLineCount;

	// Column count
	long m_nColumnCount;

	// Minimum number of require lines needed to satisfy less-than-exact scores
	long m_nLineMinimum;
	
	// Parameter used only within box finding functions to keep track of whether the group is a box
	// where all four sides have been identified.
	bool m_bIsFourSidedBox;

	// If either end of this line is the result of an intersection with a perpedicular line,
	// these variables will keep track of which line it intersected with 
	int m_nLineTopOrLeftEndId;
	int m_nLineBottomOrRightEndId;

	// A derivative group of lines (all within same column)
	auto_ptr<LeadToolsLineGroup> m_apSubLineGroup;

	// A derivative column grouping
	auto_ptr<LeadToolsLineGroup> m_apSubColumnGroup;

	/////////////////
	// Methods
	/////////////////

	// *** Functions that act on collections of groups *** //

	// Groups provided line rects into columns.  If bBoxBoundsOnly is true, group lines will
	// only look for boxes that can be formed with the provided lines, not line regions with 2 or
	// more lines. A return value of false indicates the algorithm short-circuited after
	// encountering too many potential line grouping permutations.
	bool groupLinesIntoColumns(list<LeadToolsLineGroup> &rlistGroups, bool bBoxBoundsOnly = false);

	// Groups provided groups into areas 
	void groupColumnsIntoAreas(list<LeadToolsLineGroup> &rlistGroups);

	// Removes unqualified areas as well as areas that are subsets of larger
	// areas in the provided list of groups
	void qualifyAreas(list<LeadToolsLineGroup> &rListGroups, bool bPreferLargerRegion = true);

	// Test the two provided group iterators to see if they overlap sufficiently, and if so, 
	// remove one of the groups from rListGroups. If bPreferLargerRegion is true, pareArea
	// will discard groups that are entirely or substantially contained in larger groups.  
	// If bPreferLargerRegion is false, it will discard groups that entirely or substantially
	// contain a smaller group within its bounds.  Groups that contain more member lines
	// will be prefered over groups with fewer member lines.
	// NOTE: pareArea assumes riterGroup_i is a primary and riterGroup_j a secondary iterator
	// in a 2 dimensional loop.  It will increment the iterator as appropriate to move the
	// loop forward.
	void pareArea(list<LeadToolsLineGroup> &rListGroups,
				  list<LeadToolsLineGroup>::iterator &riterGroup_i,
				  list<LeadToolsLineGroup>::iterator &riterGroup_j,
				  bool bPreferLargerRegion = true);

	// Appends the results from a box search in one orientation (horizontal/vertical) to the
	// results of a box search in the other orientation. In the process, any box that was found
	// in both orientations is identified as a four-sided box that will be prefered over any box
	// where all 4 sides haven't been found when paring overlapping boxes from the final result.
	void appendBoxResults(list<LeadToolsLineGroup>& rlistExistingBoxes,
						  list<LeadToolsLineGroup> listBoxesToAppend);

	// Calculates the percentage of each specified line group that is contained in the overlap of
	// the two line groups. Both values will be zero if the line groups do not overlap.
	void calculateIntersection(const LeadToolsLineGroup& group1, const LeadToolsLineGroup& group2,
							   int& rnPercentInclusion_1, int &rnPercentInclusion_2);

	// Given the provided LineRect, locate nearby lines from vrecHorzLineRects and 
	// rvecVertLineRects.  Following the call, vrecHorzLineRects and rvecVertLineRects
	// will contain only lines that were found to be nearby the provided LineRect
	void findLinesAroundRect(const LineRect &rect, vector<LineRect> &vrecHorzLineRects,
							 vector<LineRect> &rvecVertLineRects);

	// A helper function to findLinesAroundRect.
	// Given the provided LineRect, find the first perpendicular line to either end
	// of the line that crosses paths with the provided line rect. rsetClosestLines
	// will contain the indexes of these lines (relative to the position of the 
	// lines in vecLineRects.  If pvecClosestIntersections is provided, it will be
	// populated with CRects that represent the closest points of the found lines
	// to the search rect.
	void findClosestLines(const LineRect &rectStart, const vector<LineRect> &vecLineRects,
						  set<int> &rsetClosestLines, 
						  vector<CRect> *pvecClosestIntersections = NULL);

	// Split the LeadToolsLineGroups rlistPrimaryLines in rlistPrimaryLines into 
	// all the possible permutations of lines segements that could result by
	// splitting the lines at the points of intersection with rvecSplitterLines.
	// For each member of rlistPrimaryLines that is split, either m_nLineTopOrLeftEndId or
	// m_nLineBottomOrRightEndId will be set according to the index of the line from 
	// rvecSplitterLines which triggered the split.
	void splitLines(list<LeadToolsLineGroup> &rlistPrimaryLines, vector<LineRect> &rvecSplitterLines);

	// *** Functions that act as an individual group *** //

	// If the specified line rect qualifies it is added to the existing group
	// and the function returns true.  Otherwise, false if returned and the group
	// is unchanged. psetExistingPairs represents the different line pairings
	// that have already been combined in a group.  If psetExistingPairs is non-NULL,
	// and a line pairing is found to already exist in one group, assimilateLine will
	// not allow a new group to start with those two lines-- it is assumed that the
	// new group would be a subset of the existing group.
	bool assimilateLine(LeadToolsLineGroup *pGroupCandidate, bool bRequireExact = false,
						set< pair<long, long> > *psetExistingPairs = NULL);

	// If the specified group (representing a column) qualifies it is combined with 
	// the existing group and the function returns true.  Otherwise, false if 
	// returned and the group is unchanged
	bool assimilateColumn(LeadToolsLineGroup *pGroupCandidate);

	// For lines: returns the lowest similarity percentage from comparing the specified spacing
	// with each existing row in the group.
	// For columns: returns the lowest similarity percentage from comparing the specified column
	// group's average spacing to the average spacing of each existing column in the group
	int scoreLineSpacing(int nSpacing);

	// For lines: returns the lowest alignment percentage (overlap / combined width) that 
	// results from comparing the specified line ends with each line in the group
	int scoreAlignment(int nTopOrLeftEnd, int nBottomOrRightEnd);

	// Helper function for scoreAlignment
	// Compares the line end IDs of the sub groups and updates the aligment score and line end
	// IDs as appropriate
	void compareLineEndIDs(int &rnAlignmentScore);

	// Tests group for qualification based on m_Settings.  If bMinimally is true, it tests
	// only if the minimum qualifications are met.  If false, it tests both minimum and
	// maximum qualifications
	bool qualified(bool bMinimally = false);

	// Returns an vector of LineRects representing the lines the comprise this group
	void getLineRects(vector<LineRect> &rvecLineRects) const;

	// Returns the average spacing for this column (does not include any sub-columns)
	int getAverageSpacing();

	// Returns the total number of included lines
	int getTotalLineCount();
};