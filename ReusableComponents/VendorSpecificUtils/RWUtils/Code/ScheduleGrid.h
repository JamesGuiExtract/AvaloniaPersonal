#pragma once

#include "..\..\..\APIs\RogueWave\Inc\Grid\gxall.h"
#include "RWUtils.h"

#include <vector>

using namespace std;

// Constants
const int giNUMBER_OF_HOURS_IN_WEEK = 168;

//-------------------------------------------------------------------------------------------------
// ScheduleGrid
//-------------------------------------------------------------------------------------------------
class RW_UTILS_API ScheduleGrid : public CGXGridWnd
{
	DECLARE_DYNAMIC(ScheduleGrid)

public:
	ScheduleGrid();
	virtual ~ScheduleGrid();

	// Sets of the the grid with 8 columns and 24 rows
	// the first column will be labeled Hours the rest for
	// each day of the week.
	void PrepareGrid();

	// Returns const reference to the m_vecScheduledHours member
	const vector<bool> &GetScheduledHours();

	// Copies the vecScheduledHours argument to the m_vecScheduledHours member 
	void SetScheduledHours(const vector<bool> &vecScheduledHours);

protected:
	DECLARE_MESSAGE_MAP()

	afx_msg void OnLButtonDown(UINT nFlags, CPoint point);
	afx_msg void OnLButtonUp(UINT nFlags, CPoint point);

private:

	// Variables

	// Vector that contains a bool value for every hour in a week (168)
	vector<bool> m_vecScheduledHours;

	// Flag to indicate the grid has been initialized
	bool m_bInitialized;
	
	// Contains the last point the left mouse button was clicked
	// This is used in the mouse up method
	CPoint m_pointMouseDown;

	// Methods
	
	// Sets the color for the grid cells based on the value in the m_vecScheduledHours
	// vector.
	void colorGrid();
};


