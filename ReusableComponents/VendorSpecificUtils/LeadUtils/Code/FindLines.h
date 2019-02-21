#pragma once

#include "LeadUtils.h"
#include "LineRect.h"

#include <l_bitmap.h>		// LeadTools Imaging library
#include <afxmt.h>

#include <UCLIDException.h>

#include <string>
#include <vector>
#include <deque>
#include <set>
#include <algorithm>
using namespace std;

///////////////////////////
// LeadToolsLineFinder
///////////////////////////
class LEADUTILS_API LeadToolsLineFinder
{
public:
	LeadToolsLineFinder();
	LeadToolsLineFinder(const LeadToolsLineFinder &source);
	~LeadToolsLineFinder();

	LeadToolsLineFinder& operator =(LeadToolsLineFinder &source);

	// PROMISE:	Return (as a vector of LineRects) the lines within the specified bitmap.
	// ARGS:	pBitmap - handle to a bitmap opened via LeadTool's L_LoadBitmap function
	//			uFlags - Specify LINEREMOVE_HORIZONTAL or LINEREMOVE_VERTICAL (VERTICAL
	//					 not currently supported)
	//			vecLines - A vector into which the result line rects are to be inserted.
	//					   findLines does not clear the vector prior to adding the rects
	void findLines(pBITMAPHANDLE pBitmap, L_UINT uFlags, vector<LineRect>& vecLines);

	// Public parameters to tune line detection
	// The LINEREMOVE struct that governs LeadTool's underlying call to find lines
	LINEREMOVE m_lr;

	// (in pixels) If two lines are positioned in-line and less than this distance from each other,
	// they will be combined into one line.  This setting is also used to cap the distance
	// that line fragment extension will scan
	long m_nBridgeGapSmallerThan;

	// If true, post-processing is enabled that will attempt to seek out lines that extend
	// beyond the rects returned via the underlying LeadTool's call
	bool m_bExtendLineFragments;

	// For ExtendLinesFragments: The width (in pixels) of the area to scan for line extensions
	// This is intended to be a odd number (1,3,5...) so that the scan area extends equa-distance
	// on either side the center of the line that is being tracked
	long m_nExtensionScanLines;

	// For ExtendLinesFragments: The number of white pixels that can be found before the scan
	// is aborted.  This count is "paused" every time a black pixel is found and reset every 
	// time the ConsecutiveMinimum is reached
	long m_nExtensionGap;

	// For ExtendLinesFragments: The number of consecutive black pixels that must eventually be
	// found to qualify a line for extension.
	long m_nExtensionConsecutiveMinimum;

	// For ExtendLinesFragments: When white pixels are found, the scan will begin jumping ahead
	// at greater distances goverened by this setting.  Can be 0 - 100.  If zero, the scan will
	// continue to look one pixel at a time even when white pixels are found.  If 100, for each
	// consecutive white pixel found, the scan will skip ahead an additional pixel.  For example,
	// a scan may search x pos 1000, 1001, 1003, 1006, 1010, 1015.  Once a black pixel is found, 
	// it will immediately revert to scanning ahead one pixel at a time.
	long m_nExtensionTelescoping;

private:
	/////////////////
	// Variables
	/////////////////

	static CCriticalSection ms_criticalSection;

	bool m_bHorizontal;

	// Handle to the bitmap currently being processed
	pBITMAPHANDLE m_pBitmap;

	// Pointer to the vector passed in to the currently running findLines call
	vector<LineRect> *m_pvecLines;

	// Used to track a line for line fragment extension
	deque<int> m_queScanLines;

	// Used to keep track of exceptions that fire in lineRemoveCB
	bool m_bException;
	UCLIDException m_ue;

	// Parameters used to govern line tracking for line fragment extension
	struct TrackingInfo
	{
		// The center of the line (width-wise-- ie, vertically on a horizontal line)
		int nCenter;

		// Given the current center position, the amount by which the current scan
		// area should eventually be shifed (vertically on a horizontal line).  The
		// scan area is not allowed to be shifted instantly in order to prevent tracking
		// black pixels that veere off the original line at sharp angles (ie, handwriting
		// or text)
		int nTargetShift;

		// An accumulation which can be used toward an eventual shift of the scan area
		int nAccumulatedShift;

	} m_TrackingInfo;

	/////////////////
	// Methods
	/////////////////

	// Adds the specified line rect to the vector passed into findLines.  Existing
	// lines will be scanned and the provided lines will be merged with an
	// existing line if appropriate.
	void addLine(CRect rect);

	// Attempts to merge the specified line with an existing line in the vector. 
	// Tests for merges are done in 2 ways:
	// 1) For 2 lines stacked closely on top of one another.
	// 2) For 2 lines that are in-line with each other with a small gap inbetween
	bool attemptMerge(LineRect &rrectLine);

	// Given the specified line set (rvecLines), uses attemptMerge to merge all
	// lines qualified for merging.
	void mergeLines(vector<LineRect> &rvecLines);

	// Returns true if rectLine1 is wider than rectLine2.  Used to order the line
	// set according to width in correctFatLines.
	static bool isWider(const LineRect &rectLine1, const LineRect &rectLine2);

	// A higher LINEREMOVE::iWall setting is now being used (35) which allows fat lines
	// or lines with jagged edges to be found more reliably.  However, this setting has
	// a negative side effect of returning areas which encompass not only the line, but
	// text on the line.  correctFatLines will re-process returned areas that are wide
	// compared to the other found lines with a lower iWall value to see if a more
	// accurate line can be found.
	void correctFatLines(vector<LineRect>& rvecLines);

	// Execute line fragment extension code
	void extendLines();

	// Attempt line fragment extension on the specified line in the specified 
	// direction (-1 for left or up and 1 for right or down)
	bool extendLine(LineRect &rrectLine, int nDirection);

	// updateTrackingPos updates the top edge of the scan area, keeps track of where the
	// center of the line is (width-wise) and shifts the scan area as appropriate
	void updateTrackingPos(int &rnTop, bool bFoundBlackPixel, bool bFirst);

	// Before attempting to track a line, determine the center of the line 
	// (width-wise) on the specified edger (-1 for left, 1 for right)
	int initializeTrackingPos(LineRect rectLine, int nDirection, int nLen);

	// Returns true for black, false for white.  If the image is grayscale, true will
	// be returned for < 50% color saturation.  If an error was encountered reading the
	// pixel (ie, the pixel is off-page), rbErrorReadingPixel will be set to true
	// NOTE: The caller of this function should protect with LeadToolsLicenseRestrictor
	bool checkPixel(int nX, int nY, bool &rbErrorReadingPixel);

	// Declare the callback function used for L_LineRemoveBitmap a friend so that it can access addLine
	friend L_INT EXT_FUNCTION lineRemoveCB(HRGN hRgn, L_INT32 nStartRow, L_INT32 nStartCol, 
		L_INT32 nLength, L_VOID *pUserData);
};