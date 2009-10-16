// ScheduleGrid.cpp : implementation file
//

#include "stdafx.h"
#include "RWUtils.h"
#include "ScheduleGrid.h"
#include "SuspendWindowUpdates.h"

#include <UCLIDException.h>
#include <cpputil.h>

#include <vector>

using namespace std;

// Color constants
const COLORREF gcolorActive = RGB(0, 220, 120);
const COLORREF gcolorInactive = RGB(255, 255, 255);

//-------------------------------------------------------------------------------------------------
// ScheduleGrid
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(ScheduleGrid, CGXGridWnd)

//-------------------------------------------------------------------------------------------------
ScheduleGrid::ScheduleGrid()
: m_bInitialized(false), m_pointMouseDown(0,0)
{
	m_vecScheduledHours.resize(giNUMBER_OF_HOURS_IN_WEEK);
}
//-------------------------------------------------------------------------------------------------
ScheduleGrid::~ScheduleGrid()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI28130");
}
//-------------------------------------------------------------------------------------------------

BEGIN_MESSAGE_MAP(ScheduleGrid, CGXGridWnd)
	ON_WM_LBUTTONDOWN()
	ON_WM_LBUTTONUP()
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// ScheduleGrid message handlers
//-------------------------------------------------------------------------------------------------
void ScheduleGrid::OnLButtonDown(UINT nFlags, CPoint point)
{	
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Save the point that the left mouse button was clicked
		m_pointMouseDown = point;
		
		// Do normal processing
		CGXGridWnd::OnLButtonDown(nFlags, point);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI28075");
}
//-------------------------------------------------------------------------------------------------
void ScheduleGrid::OnLButtonUp(UINT nFlags, CPoint point)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Finish the selection
		CGXGridWnd::OnLButtonUp(nFlags, point);

		// Check if the down mouse click was in a header
		ROWCOL rcRow;
		ROWCOL rcCol;
		
		int nHitTestResult = HitTest(m_pointMouseDown, &rcRow, &rcCol);
		if (nHitTestResult == GX_HEADERHIT || nHitTestResult == GX_NOHIT)
		{
			// Do not need to do any further processing
			return;
		}

		// Reset the saved point to 0,0
		m_pointMouseDown.SetPoint(0,0);

		// Change the color of the selected area and clear the selection ranges
		CGXGridParam *pGridParam = GetParam();
		ASSERT_RESOURCE_ALLOCATION("ELI28152", pGridParam != NULL);

		CGXRangeList *pRangeList =  pGridParam->GetRangeList();
		ASSERT_RESOURCE_ALLOCATION("ELI28147", pRangeList != NULL);

		// Invert the state for the selected area
		int nHour = 0;
		for ( int nCol = 2; nCol <= 8; nCol++)
		{
			for (int nRow = 1; nRow < 25; nRow++)
			{
				// If the current row, col is in the list invert it's state
				if ( pRangeList->IsCellInList(nRow, nCol))
				{
					m_vecScheduledHours[nHour] = !m_vecScheduledHours[nHour];
				}
				nHour++;
			}
		}

		// Need check for case of click in a single cell since this is not in the range list
		ROWCOL r,c;
		GetCurrentCell(&r,&c);
		// Check that it is not a header and not in the range list
		if (r > 0 && c > 1 && !pRangeList->IsCellInList(r,c))
		{
			// Calculate the hour
			nHour = r-1 + (c-2) * 24;
			m_vecScheduledHours[nHour] = !m_vecScheduledHours[nHour];
		}

		// Reset the current cell
		ResetCurrentCell(FALSE);
		
		// Clear all selections
		SelectRange(CGXRange(1, 2, 24, 8), FALSE);

		// Update the colors based on the new states
		colorGrid();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI28076");
}

//-------------------------------------------------------------------------------------------------
// Public Members
//-------------------------------------------------------------------------------------------------
void ScheduleGrid::PrepareGrid()
{
	try
	{
		CGXGridWnd::Initialize();
		
		CGXGridParam *pGridParam = GetParam();
		ASSERT_RESOURCE_ALLOCATION("ELI28148", pGridParam != NULL);

		// Do not show the current cell with a frame
		pGridParam->SetHideCurrentCell(GX_HIDE_ALWAYS);

		// Set up vector of column headings
		vector<string> vecCol;
		vecCol.push_back("Hours");
		vecCol.push_back("Sun");
		vecCol.push_back("Mon");
		vecCol.push_back("Tue");
		vecCol.push_back("Wed");
		vecCol.push_back("Thu");
		vecCol.push_back("Fri");
		vecCol.push_back("Sat");

		// Set up vector of row headings
		vector<string> vecRow;
		vecRow.push_back("12 AM");
		for (int i = 1; i < 12; i++ )
		{
			string strHour = asString(i) + " AM";
			vecRow.push_back(strHour);
		}
		vecRow.push_back("12 PM");
		for (int i = 1; i < 12; i++ )
		{
			string strHour = asString(i) + " PM";
			vecRow.push_back(strHour);
		}

		HideCols( 0, 0, TRUE, NULL, GX_UPDATENOW, gxDo );
		SetRowCount(24);
		SetColCount(8);

		SetColWidth(1, 8, 50);
		for( int i = 1; i <= vecCol.size(); i++)
		{
			SetStyleRange(CGXRange( 0, i ),
				ColHeaderStyle()
				.SetValue( (vecCol[i-1]).c_str() )
				.SetVerticalAlignment( DT_VCENTER )
				.SetEnabled(FALSE)
				);
		}

		// Set row Header labels
		for (int j = 1; j <= vecRow.size(); j++)
		{
			SetStyleRange(CGXRange( j, 1 ),
				RowHeaderStyle()
				.SetValue( (vecRow[j-1]).c_str() )
				.SetEnabled(FALSE)
				);
		}
		SetFrozenCols(2,1);
		CGXStyle cgxStyle;
		
		// Set Styles attributes that are the same for all 
		cgxStyle.SetEnabled(TRUE);
		cgxStyle.SetControl(GX_IDS_CTRL_STATIC);
		cgxStyle.SetInterior(gcolorInactive);
		SetStyleRange(CGXRange(1, 2, 24, 8), cgxStyle);

		pGridParam->EnableTrackColWidth(GX_TRACK_NOTHEADER);
		pGridParam->EnableTrackRowHeight(GX_TRACK_NOTHEADER);
		pGridParam->EnableMoveCols(FALSE);
		pGridParam->EnableMoveRows(FALSE);
		pGridParam->EnableSelection(GX_SELCELL | GX_SELMULTIPLE | GX_SELSHIFT | GX_SELKEYBOARD);
		pGridParam->GetProperties()->SetMarkColHeader(FALSE);
		pGridParam->GetProperties()->SetMarkRowHeader(FALSE);
		pGridParam->GetProperties()->SetColor(GX_COLOR_GRIDLINES, RGB(0, 0, 0));

		m_bInitialized = true;
		colorGrid();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28133");
}
//-------------------------------------------------------------------------------------------------
const vector<bool> &ScheduleGrid::GetScheduledHours()
{
	return m_vecScheduledHours;
}
//-------------------------------------------------------------------------------------------------
void ScheduleGrid::SetScheduledHours(const vector<bool> &vecScheduledHours)
{
	m_vecScheduledHours = vecScheduledHours;

	// Only update the grid if it has been initialized
	if (m_bInitialized)
	{
		colorGrid();
	}
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void ScheduleGrid::colorGrid()
{
	try
	{
		// Don't update grid until all colors have been set
		SuspendWindowUpdates suspendWindow(*this);

		// Style to apply
		CGXStyle cgxStyle;

		// Set Styles attributes that are the same for all 
		cgxStyle.SetEnabled(TRUE);
		cgxStyle.SetControl(GX_IDS_CTRL_STATIC);

		// Need to color the grid
		int nHour = 0;

		// Step through columns
		for ( int nCol = 2; nCol <= 8; nCol++)
		{
			// Step through rows
			for (int nRow = 1; nRow < 25; nRow++)
			{
				// Set color for current hour's state
				if (m_vecScheduledHours[nHour])
				{
					cgxStyle.SetInterior(gcolorActive);
					SetStyleRange(CGXRange(nRow, nCol),cgxStyle, gxCopy);
				}			
				else
				{
					cgxStyle.SetInterior(gcolorInactive);
					SetStyleRange(CGXRange(nRow, nCol),cgxStyle, gxCopy);
				}
				// Keep track of the overall hour of the week
				nHour++;
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28132");
}
//-------------------------------------------------------------------------------------------------
