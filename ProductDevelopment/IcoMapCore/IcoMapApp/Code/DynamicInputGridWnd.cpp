#include "stdafx.h"
#include "resource.h"
#include "DynamicInputGridWnd.h"

#include "IIcoMapUI.h"

#include <UCLIDException.h>
#include <CurveCalculationEngineImpl.h>
#include <AbstractMeasurement.hpp>
#include <Angle.hpp>
#include <Bearing.hpp>
#include <DistanceCore.h>
#include <ValueRestorer.h>
#include <cpputil.h>
#include <TPPoint.h>
#include <IcoMapOptions.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

using namespace std;

const int ROW_HEADER_WIDTH = 62;
const int TYPE_COL_WIDTH = 30;
// how many type value pair will be placed in each row?
const int NUM_TYPE_VALUE_PAIR = 3;

const string STORE_FIELD_NAME = "IcoMapAttr";

//-------------------------------------------------------------------------------------------------
// DynamicInputGridWnd
//-------------------------------------------------------------------------------------------------
DynamicInputGridWnd::DynamicInputGridWnd()
: m_bInitGrid(false),
  m_ipFeature(NULL),
  m_bCanChangeSelection(true),
  m_nCurrentSelectedRowIndex(-1),
  m_ipCurrentPartStartPoint(NULL),
  m_nCurrentSegmentIndexInPart(-1),
  m_bCurrentSegmentModified(false),
  m_nCurrentPartIndex(0),
  m_eCurrentSegmentType(kInvalidSegmentType),
  m_eCurrentInputType(kNone),
  m_eCurrentEditingParamType(kInvalidParameterType),
  m_ipCurrentPartSegments(CLSID_IUnknownVector),
  m_ipLatestSegmentInProgress(NULL),
  m_bLatestSegmentInfoComplete(false),
  m_ipAttributeManager(NULL),
  m_ipDisplayAdapter(NULL),
  m_pIcoMapUI(NULL),
  m_pInputProcessor(NULL),
  m_nTotalSegmentsDrawnByIcoMap(0),
  m_bTrackingSegments(false),
  m_bBlockSketchNotification(false),
  m_bFocusOnGrid(false),
  m_bIsCurrentCellModified(false)
{
	try
	{
		ASSERT_RESOURCE_ALLOCATION("ELI12122", m_ipCurrentPartSegments != NULL);

		ma_pCCE = auto_ptr<ICurveCalculationEngine>(new CurveCalculationEngineImpl);
		ASSERT_RESOURCE_ALLOCATION("ELI12068", ma_pCCE.get() != NULL);

		m_ModifyingCell.resetCell();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12067")
}
//-------------------------------------------------------------------------------------------------
DynamicInputGridWnd::~DynamicInputGridWnd()
{
	m_ipAttributeManager = NULL;
	m_ipDisplayAdapter = NULL;
	m_pIcoMapUI = NULL;
	m_pInputProcessor = NULL;
}

//-------------------------------------------------------------------------------------------------
// Overridables
//-------------------------------------------------------------------------------------------------
BOOL DynamicInputGridWnd::GetStyleRowCol(ROWCOL nRow, ROWCOL nCol, CGXStyle& style, 
										 GXModifyType mt, int nType)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Load stored style information of the cell
		BOOL bRet = CGXGridCore::GetStyleRowCol(nRow, nCol, style, mt, nType);

		if (!m_bTrackingSegments)
		{
			return bRet;
		}
		
		// Provide text and background color for individual cell
		if (nType == 0)
		{
			// Check for selected row
			if (m_nCurrentSelectedRowIndex == nRow)
			{
				unsigned long nCellRow, nCellCol;
				if (GetCurrentCell(&nCellRow, &nCellCol))
				{
					// if there's a current cell, do not 
					// change the original setting
					if (m_bFocusOnGrid && nCellRow == nRow && nCellCol == nCol)
					{
						if (nCol%2 == 0 && nCol != 0 && nRow != 0)
						{
							// no change of the color
							style.SetInterior(RGB(255, 255, 0));
							return bRet;
						}
					}
				}

				// Make sure that this cell is not being edited
				// and that this grid is active
				// Use system colors for background and text
				style.SetInterior(RGB(150, 150, 230));
				style.SetTextColor(::GetSysColor(COLOR_HIGHLIGHTTEXT));
				bRet = TRUE; 
			}
		}

		return bRet;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11983")

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
void DynamicInputGridWnd::DrawInvertCell(CDC* /*pDC*/, ROWCOL nRow, ROWCOL nCol, CRect rectItem)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	// If DrawInvertCell has been called 
	// from OnDrawTopLeftBottomRight
	// m_nNestedDraw is greater 0. There 
	// is no invalidation of the rectangle
	// necessary because the cell has 
	// already been drawn.
	if (m_nNestedDraw == 0)
	{ 
		// m_nNestedDraw equal to 0 means 
		// that PrepareChangeSelection,
		// PrepareClearSelection or 
		// UpdateSelectRange did call 
		// this method.
		CGXRange range;
		if (GetCoveredCellsRowCol(nRow, nCol, range)) 
		{
			rectItem = CalcRectFromRowCol(range.top, range.left, range.bottom, range.right); 
		}

		InvalidateRect(&rectItem); 
	} 
} 
//-------------------------------------------------------------------------------------------------
BOOL DynamicInputGridWnd::OnStartEditing(ROWCOL nRow, ROWCOL nCol)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		BOOL bRet = CGXGridCore::OnStartEditing(nRow, nCol);

		if (nRow > 0 && nCol > 0)
		{
			m_ModifyingCell.m_nRow = nRow;
			m_ModifyingCell.m_nCol = nCol;
		}

		return bRet;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11984")

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
BOOL DynamicInputGridWnd::OnEndEditing(ROWCOL nRow, ROWCOL nCol)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		BOOL bRet = CGXGridCore::OnEndEditing(nRow, nCol);

		if (nRow > 0 && nCol > 0)
		{
			if (m_ModifyingCell.m_nRow == nRow && m_ModifyingCell.m_nCol == nCol)
			{
				CGXControl* pControl = GetCurrentCellControl();
				if (pControl != NULL)
				{
					m_bIsCurrentCellModified = pControl->GetModify() == TRUE;
				}
			}
		}

		return bRet;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11985");

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
BOOL DynamicInputGridWnd::OnTrackColWidthMove(ROWCOL nCol, int nWidth)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		BOOL bRet = TRUE;
		// do not allow user to hide a col
		if (nWidth < 10)
		{
			bRet = FALSE;
		}
		
		return bRet;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11987")

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
BOOL DynamicInputGridWnd::OnSelDragRowsStart(ROWCOL nFirstRow, ROWCOL nLastRow)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// if last row is about to be moved, check if the latest
		// segment in progress is completed or not
		unsigned int nTotalNumOfRows = GetRowCount();
		if (nTotalNumOfRows > 0
			&& nFirstRow == nLastRow 
			&& nTotalNumOfRows == nFirstRow
			&& !m_bLatestSegmentInfoComplete)
		{
			// can't be drag-dropped
			return FALSE;
		}

		return CGXGridCore::OnSelDragRowsStart(nFirstRow, nLastRow);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11986")

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
BOOL DynamicInputGridWnd::StoreMoveRows(ROWCOL nFromRow, ROWCOL nToRow, ROWCOL nDestRow, BOOL bProcessed)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// check if there's only one row move at a time
		if (nFromRow != nToRow || nFromRow == nDestRow)
		{
			return FALSE;
		}

		unsigned int nLastRow = GetRowCount();
		// if there's at most one row in total
		if (nLastRow <= 1)
		{
			return FALSE;
		}

		long nOldIndex = nFromRow-1;
		long nNewIndex = nDestRow-1;

		// there are restrictions for rows that can 
		// only be moved to a certain destination row...

		// 1) Any row can't be moved to go below last row
		//    if last row is representing a segment in progress
		if (nLastRow == nDestRow && !m_bLatestSegmentInfoComplete)
		{
			return FALSE;
		}

		// 2) Any row that is representing a segment that requires
		//    tangent-in direction can't be moved to the top-most row
		if (nDestRow == 1)
		{
			unsigned long nSize = m_ipCurrentPartSegments->Size();
			// display is out-sinc with all segments for the part
			if (nFromRow > nSize)
			{
				UCLIDException ue("ELI12476", "Invalid row index");
				ue.addDebugInfo("Row", nFromRow);
				ue.addDebugInfo("TotalNumOfSegments", nSize);
				throw ue;
			}

			IESSegmentPtr ipSegmentToBeMoved = m_ipCurrentPartSegments->At(nOldIndex);
			if (ipSegmentToBeMoved == NULL 
				|| ipSegmentToBeMoved->requireTangentInDirection() == VARIANT_TRUE)
			{
				return FALSE;
			}
		}

		// 3) If the moving row is the first row, and its following row
		//    is representing a segment that requires tangent-in 
		//    direction, then the first row can't be moved to any where else
		if (nFromRow == 1)
		{
			long nSize = m_ipCurrentPartSegments->Size();
			// there's at least 2 segments
			if (nSize >= 2)
			{
				// the segment right below the first segment
				IESSegmentPtr ipSegmentAfterToBeMoved = m_ipCurrentPartSegments->At(nOldIndex+1);
				if (ipSegmentAfterToBeMoved != NULL 
					&& ipSegmentAfterToBeMoved->requireTangentInDirection() == VARIANT_TRUE)
				{
					return FALSE;
				}
			}
		}

		// otherwise, update the sequence as well as the drawing
		// get the segment to be moved
		IESSegmentPtr ipSegmentToBeMoved = m_ipCurrentPartSegments->At(nOldIndex);
		m_ipCurrentPartSegments->Remove(nOldIndex);
		m_ipCurrentPartSegments->Insert(nNewIndex, ipSegmentToBeMoved);
		
		// starting from which row do we need to update the drawing?
		long nStartSegmentIndex = nFromRow < nDestRow ? nFromRow - 1 : nDestRow - 1;
		if (m_ipDisplayAdapter)
		{
			// temporary block sketch notification
			ValueRestorer<bool> VR(m_bBlockSketchNotification);
			m_bBlockSketchNotification = true;

			m_ipDisplayAdapter->UpdateSegments(m_nCurrentPartIndex, 
						nStartSegmentIndex, m_ipCurrentPartSegments);

			// update current selected segment info
			setRowsSelection(nDestRow, 1, true, false);
		}

		return CGXGridCore::StoreMoveRows(nFromRow, nToRow, nDestRow, bProcessed);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11989")

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
BOOL DynamicInputGridWnd::OnLButtonClickedRowCol(ROWCOL nRow, ROWCOL nCol, UINT flags, CPoint point)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// If the cell is empty or noneditable, do not put focus on that line
		if (GetValueRowCol(nRow, nCol).IsEmpty() || nCol % 2 != 0 || nCol == 0)
		{
			// If the last row is highlighted when clicking empty or noneditable cell, 
			// and at the same time, the previous focus is on prompt line, we need 
			// to put focus on the last cell in the grid otherwise no cell or prompt line
			// will get focus [P10: 3101]
			if (m_nCurrentSelectedRowIndex == this->GetRowCount() && m_CurrentCell.isNull())
			{
				unsigned long nCellRow, nCellCol;
				nCellRow = GetRowCount();
				nCellCol = GetColCount();

				// Get the data of the last two editable cell on the last row
				CString zData1 = GetValueRowCol(nCellRow, nCellCol);
				CString zData2 = GetValueRowCol(nCellRow, nCellCol - 2);

				// Get the last editable cell that contains data and set
				// focus to it
				if (!zData1.IsEmpty())
				{
					SetCurrentCell(nCellRow, nCellCol);
				}
				else if (!zData2.IsEmpty())
				{
					SetCurrentCell(nCellRow, nCellCol - 2);
				}
				else
				{
					SetCurrentCell(nCellRow, nCellCol - 4);
				}
			}

			m_bFocusOnGrid = true;
			return FALSE;
		}

		BOOL bRet = CGXGridCore::OnLButtonClickedRowCol(nRow, nCol, flags, point);

		// Redraw the segment when user click on another cell
		//if (m_bTrackingSegments && isCurrentSegmentCalculatable())
		//{
		//	// Redraw the segment
		//	redrawCurrentSegment();
		//}

		// show no context menu if it's the row or col header
		if (nRow == 0 || nCol == 0)
		{
			return bRet;
		}

		// what if the row clicked is different than the currently
		// selected row?
		// Or grid wasn't in focus
		if (nRow != m_nCurrentSelectedRowIndex 
			|| !m_bFocusOnGrid )
		{
			setRowsSelection(nRow, 1);
			
			unsigned long nCurRow = 0, nCurCol = 0;
			// record the current cell coords
			m_CurrentCell.m_nRow = nRow;
			m_CurrentCell.m_nCol = nCol;
			
			// get index of parameter in the vec
			unsigned int nParamIndex = nCol/2 - 1;
			// if current selected cell is beyond the number of parameters the record has
			if (nParamIndex >= m_vecCurrentSegmentParams.size() || nParamIndex < 0)
			{
				m_bFocusOnGrid = true;

				m_eCurrentInputType = kNone;
				if (m_pInputProcessor)
				{
					// update the input type
					m_pInputProcessor->notifyInputTypeChanged(true);
				}
				return bRet;
			}
			
			// retrieve the parameter type of this cell
			IParameterTypeValuePairPtr ipParam = m_vecCurrentSegmentParams[nParamIndex];
			m_eCurrentEditingParamType = ipParam->eParamType;
			
			// update input type
			if (nCol%2 == 0)
			{
				m_eCurrentInputType = translateParameterTypeToInputType(m_eCurrentEditingParamType);
			}
			else
			{
				if (m_eCurrentInputType != kToggleCurve || m_eCurrentInputType != kToggleAngle)
				{
					m_eCurrentInputType = kNone;
				}
			}
			
			// input type changed here
			if (m_pInputProcessor)
			{
				// update the input type
				m_pInputProcessor->notifyInputTypeChanged(true);
			}
		}

		m_bFocusOnGrid = true;

		return bRet;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11993")

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
BOOL DynamicInputGridWnd::OnRButtonClickedRowCol(ROWCOL nRow, ROWCOL nCol, UINT flags, CPoint point)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		BOOL bRet = CGXGridCore::OnRButtonClickedRowCol(nRow, nCol, flags, point);

		// show no context menu if it's the row or col header
		if (nRow == 0 || nCol == 0)
		{
			return bRet;
		}

		// what if the row clicked is different than the currently
		// selected row?
		if (nRow != m_nCurrentSelectedRowIndex)
		{
			setRowsSelection(nRow, 1);
						
			// Set the current cell to the last editable cell that is not empty(P10 # 3119)
			ROWCOL nNewCol;
			for ( nNewCol = NUM_TYPE_VALUE_PAIR * 2; nNewCol >= 2; nNewCol -= 2)
			{
				if ( !GetValueRowCol(nRow, nNewCol).IsEmpty() )
				{
					SetCurrentCell(nRow, nNewCol);
					
					// Update Column parameter
					nCol = nNewCol;
					break;
				}
			}
		}

		// Before showing the context menu, update segment 
		// if it is supposed to be updated
		if (m_bCurrentSegmentModified)
		{
			try
			{
				try
				{
					// if the segment already drawn in the map,
					// redraw the segment
					if (m_nCurrentSegmentIndexInPart >= 0)
					{
						redrawCurrentSegment();
					}
				}
				catch (UCLIDException &ue)
				{
					ue.addHistoryRecord("ELI12607", "One or more of the segment parameter "
						"values are invalid. Original value(s) will be restored.");
					throw ue;
				}
			}
			catch (...)
			{
				// set selection back
				setRowsSelection(m_nCurrentSelectedRowIndex, 1, true, false);
				// refresh the display
				refreshCurrentRecordDisplay();
				throw;
			}
		}

		// show context menu
		CMenu menu;
		menu.LoadMenu(IDR_MNU_DIG_CONTEXT);
		CMenu *pContextMenu = menu.GetSubMenu(0);

		UINT nEnable = (MF_BYCOMMAND | MF_ENABLED);
		UINT nDisable = (MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);

		// vector of menu items
		static bool bPopulated = false;
		static vector<int> vecMenuItems;
		if (!bPopulated)
		{
			vecMenuItems.push_back(ID_DIG_INSERT_DUPLICATE);
			vecMenuItems.push_back(ID_DIG_REMOVE);
			vecMenuItems.push_back(ID_DIG_UP);
			vecMenuItems.push_back(ID_DIG_DOWN);
			bPopulated = true;
		}

		// vector of enable/disable
		vector<UINT> vecEnable;

		bool bIsFirstSegment = false;
		bool bIsLastSegment = m_nCurrentSelectedRowIndex == GetRowCount();
		// if the segment is the latest segment in progress and its
		// info hasn't been completed yet
		bool bIsLatestSegmentInProgress = 
			!m_bLatestSegmentInfoComplete && m_nCurrentSelectedRowIndex == GetRowCount();

		// if current selected segment is the first segment followed by a
		// segment that requires tangent-in direction
		bool bFirstAndFollowedByDependent = false;
		// if current segment is followed by a segment in progress
		bool bFollowedBySegmentInProgress = 
			m_nCurrentSelectedRowIndex == GetRowCount()-1 
			&& !m_bLatestSegmentInfoComplete 
			&& m_ipLatestSegmentInProgress != NULL;

		long nTotalNumOfSegments = m_ipCurrentPartSegments->Size();
		if (m_nCurrentSelectedRowIndex == 1)
		{
			bIsFirstSegment = true;
			if (nTotalNumOfSegments >= 2)
			{
				IESSegmentPtr ipSecondSegment = m_ipCurrentPartSegments->At(1);
				if (ipSecondSegment->requireTangentInDirection() == VARIANT_TRUE)
				{
					bFirstAndFollowedByDependent = true;
				}
			}
			// if there's only one segment drawn and there's a segment 
			// in progress, which is a segment that requires tangent-in
			else if (nTotalNumOfSegments == 1 && bFollowedBySegmentInProgress)
			{
				IESSegmentPtr ipSecondSegment(m_ipLatestSegmentInProgress);
				if (ipSecondSegment->requireTangentInDirection() == VARIANT_TRUE)
				{
					bFirstAndFollowedByDependent = true;
				}
			}
		}

		// if current selected segment is second segment and it requires tangent
		bool bSecondAndRequireTI = false;
		if (m_nCurrentSelectedRowIndex == 2 && nTotalNumOfSegments >= 2)
		{
			IESSegmentPtr ipCurrentSegment = m_ipCurrentPartSegments->At(1);
			if (ipCurrentSegment->requireTangentInDirection() == VARIANT_TRUE)
			{
				bSecondAndRequireTI = true;
			}
		}

		// 1) Insert Duplicate
		vecEnable.push_back(bIsLatestSegmentInProgress ? nDisable : nEnable);

		// 2) Remove
		// if the remove item is first segment and its following segment
		// requires tangent-in direction, then this first segment can't
		// be removed
		vecEnable.push_back(bFirstAndFollowedByDependent ? nDisable : nEnable);

		// 3) Move up
		// if current selected segment is second segment and it requires tangent,
		// or current segment is the segment in progress, or it is the first
		// segment, can't move up
		vecEnable.push_back(bSecondAndRequireTI 
							|| bIsLatestSegmentInProgress 
							|| bIsFirstSegment
							? nDisable : nEnable);

		// 4) Move down
		// if current selected segment is the first segment followed by a
		// segment that requires tangent-in direction, or it is last segment,
		// can't move down
		vecEnable.push_back(bIsLastSegment
							|| bFirstAndFollowedByDependent
							|| bFollowedBySegmentInProgress
							? nDisable : nEnable);

		for (unsigned int n=0; n<vecMenuItems.size(); n++)
		{
			pContextMenu->EnableMenuItem(vecMenuItems[n], vecEnable[n]);
		}

		CRect rectDIG;
		GetWindowRect(&rectDIG);
		// Display and manage the context menu
		pContextMenu->TrackPopupMenu(TPM_LEFTALIGN | TPM_RIGHTBUTTON, 
										rectDIG.left + point.x, rectDIG.top + point.y, 
										this, rectDIG);

		return bRet;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11994")

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
BOOL DynamicInputGridWnd::CanChangeSelection(CGXRange* pRange, BOOL bIsDragging, BOOL bKey)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		BOOL bRet = m_bCanChangeSelection ? TRUE : FALSE;

		if (bRet)
		{		
			bRet = CGXGridCore::CanChangeSelection(pRange, bIsDragging, bKey);
		}

		return bRet;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12146")

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
BOOL DynamicInputGridWnd::SetCurrentCell(ROWCOL nRow, ROWCOL nCol, UINT flags)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	// Adjust the column so that it will be on the next editable cell if not empty
	if ( nCol == 0 )
	{
		nCol = 2;
	}
	else if ( nCol % 2 != 0 )
	{
		nCol++;
	}

	if (!m_bTrackingSegments)
	{
		return CGXGridCore::SetCurrentCell(nRow, nCol, flags);
	}

	BOOL bRet = FALSE;

	try
	{
		unsigned int nTotalNumOfRows = GetRowCount();
		if (nRow > nTotalNumOfRows)
		{
			return bRet;
		}

		unsigned long nOldRow = 0, nOldCol = 0;
		bool bCanGetCurrentCell = false;
		if (GetCurrentCell(&nOldRow, &nOldCol))
		{
			bCanGetCurrentCell = true;
			// we're leaving the cell
			if (nOldRow != nRow || nOldCol != nCol)
			{
				// if the cell is the value cell
				if (nOldCol%2 == 0 && nOldCol > 0 && nOldRow > 0)
				{
					// if the current cell text is what we want
					CGXControl* pControl = GetCurrentCellControl();
					if (pControl)
					{
						if (m_ModifyingCell.m_nRow == nOldRow
							&& m_ModifyingCell.m_nCol == nOldCol)
						{
							m_bIsCurrentCellModified = pControl->GetModify() == TRUE;
						}

						validObjects();
						
						CString cstrActiveCellText("");
						unsigned int nValueIndex = nOldCol / 2 - 1;
						if (nValueIndex < m_vecCurrentSegmentParams.size())
						{
							// get current cell parameter type
							IParameterTypeValuePairPtr ipParam = m_vecCurrentSegmentParams[nValueIndex];
							ECurveParameterType eParamType = ipParam->eParamType;

							// get the user input
							pControl->GetCurrentText(cstrActiveCellText);
							// what's the existing text
							CString cstrOldText = GetValueRowCol(nOldRow, nOldCol);
							if (m_bIsCurrentCellModified)
							{
								m_bIsCurrentCellModified = false;
								m_ModifyingCell.resetCell();
								// send the text for validation
								SetValueRange(CGXRange(nOldRow, nOldCol), cstrOldText);
								m_pIcoMapUI->setInput((LPCTSTR)cstrActiveCellText);
							}
						}
					}
				}
			}
		}
		
		bRet = CGXGridCore::SetCurrentCell(nRow, nCol, flags);

		m_bFocusOnGrid = true;
		
		// the current cell text is valid, let's move on
		
		// ***********************************************
		// if selection is going to change to another row
		// redraw current row if it's modified and it is
		// not the latest segment in progress
		try
		{
			try
			{
				if (bCanGetCurrentCell && nOldRow > 0 && nOldRow != nRow)
				{
					if (m_bCurrentSegmentModified 
						&& m_nCurrentSegmentIndexInPart >= 0)
					{
						redrawCurrentSegment();
					}
				}
			}
			catch (UCLIDException &ue)
			{
				ue.addHistoryRecord("ELI12608", "One or more of the segment parameter "
					"values are invalid. Original value(s) will be restored.");
				throw ue;
			}
		}
		catch (...)
		{
			// set selection back
			setRowsSelection(m_nCurrentSelectedRowIndex, 1, true, false);
			// refresh the display
			refreshCurrentRecordDisplay();

			m_bCanChangeSelection = false;
			bRet = FALSE;
			throw;
		}

		if (nRow == 0)
		{
			return bRet;
		}

		// ***********************************************
		// if current new cell is contains editable data and can be set, 
		// then select the entire row
		if (m_nCurrentSelectedRowIndex != nRow && nCol != 0
			&& !GetValueRowCol(nRow, nCol).IsEmpty() && nCol % 2 == 0)
		{
			setRowsSelection(nRow, 1);
		}

		// ***********************************************
		// update the current selected cell input type, etc.

		// only even numbered cells are editable if it's not empty (except the first col)
		if (nCol%2 == 0 && nCol != 0)
		{
			// get index of parameter in the vec
			unsigned int nParamIndex = nCol/2 - 1;
			if (nParamIndex >= m_vecCurrentSegmentParams.size() || nParamIndex < 0)
			{
				if (m_eCurrentInputType != kToggleCurve && m_eCurrentInputType != kToggleAngle 
					// Set input type to None only if the cell contains data, Other wise
					// do not set input type to None because this cell will not be selected [P10: 3101]
					&& !GetValueRowCol(nRow, nCol).IsEmpty())
				{
					// Set the input type to none 
					m_eCurrentInputType = kNone;

					if (m_pInputProcessor)
					{
						// update the input type
						m_pInputProcessor->notifyInputTypeChanged(true);
					}
				}
				return bRet;
			}

			// retrieve the parameter type of this cell
			IParameterTypeValuePairPtr ipParam = m_vecCurrentSegmentParams[nParamIndex];
			m_eCurrentEditingParamType = ipParam->eParamType;

			// update input type
			m_eCurrentInputType = translateParameterTypeToInputType(m_eCurrentEditingParamType);
		}
		// The If block does not need the else part any more because noneditable
		// or empty cell will never get selected, so there is no need to set
		// input type to None

		// input type changed here
		if (m_pInputProcessor)
		{
			// update the input type
			m_pInputProcessor->notifyInputTypeChanged(true);
		}
		
		refreshRowDraw(nRow);

		m_bCanChangeSelection = true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11995")

	return bRet;
}
//-------------------------------------------------------------------------------------------------
BOOL DynamicInputGridWnd::CanSelectCurrentCell(BOOL bSelect, 
											   ROWCOL dwSelectRow, 
											   ROWCOL dwSelectCol, 
											   ROWCOL dwOldRow, 
											   ROWCOL dwOldCol)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	BOOL bRet = FALSE;

	try
	{
		// User can not select column zero or odd number column [P10: 3101]
		if (dwSelectCol % 2 != 0 || dwSelectCol == 0)
		{
			return bRet;
		}

		// User can not select cell without any data
		CString zData = GetValueRowCol(dwSelectRow, dwSelectCol);
		if (zData.IsEmpty())
		{
			return bRet;
		}

		bRet = CGXGridCore::CanSelectCurrentCell(bSelect, dwSelectRow, dwSelectCol, dwOldRow, dwOldCol);

		if (!m_bTrackingSegments)
		{
			return bRet;
		}

		if (dwSelectRow == dwOldRow && dwSelectCol == dwOldCol)
		{
			// set current cell
			m_CurrentCell.m_nRow = dwSelectRow;
			m_CurrentCell.m_nCol = dwSelectCol;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11992")

	return bRet;
}
//-------------------------------------------------------------------------------------------------
BOOL DynamicInputGridWnd::OnGridKeyDown(UINT nChar, UINT nRepCnt, UINT nFlags)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	BOOL bRet = FALSE;

	try
	{
		if (m_bTrackingSegments)
		{
			// if key is Del
			if (nChar == VK_DELETE)
			{
				return TRUE;
			}

			// if Tab key or Right key is pressed, the next editable cell will be 
			// selected and if there is no editable cell, the leftmost editable 
			// cell on the next row will be selected [P10: 3093]
			if ( (nChar == VK_TAB && ::GetKeyState(VK_SHIFT) >= 0) ||
				nChar == VK_RIGHT)
			{
				// Get the current row and col
				unsigned long nCellRow, nCellCol;
				if (GetCurrentCell(&nCellRow, &nCellCol))
				{
					// If the selected cell is an even number cell (editable cell)
					if (nCellCol % 2 == 0)
					{
						if (nCellCol < this->GetColCount())
						{
							// Get the data in the next even number cell to see
							// if it is empty
							CString strResult = this->GetValueRowCol(nCellRow, nCellCol + 2);
							if (strResult.IsEmpty())
							{
								// set to the first data cell on the next row
								this->SetCurrentCell(nCellRow + 1, 2);
							}
							else
							{
								// Go to next editable cell if available
								this->SetCurrentCell(nCellRow, nCellCol + 2);
							}
						}
						else
						{
							// Go to the first editable cell (column 2) on the next row
							this->SetCurrentCell(nCellRow + 1, 2);
						}

						return true;
					}
					// If the selected cell is an odd number cell (noneditable cell)
					else
					{
						// Go to the next editable cell in the same row
						nChar = VK_RIGHT;
					}
				}
			}

			// if Shift+Tab key or Left key is pressed, the previous editable cell will be 
			// selected and if there is no editable cell, the rightmost editable 
			// cell on the previous row will be selected
			if ( (nChar == VK_TAB && ::GetKeyState(VK_SHIFT) < 0) ||
				nChar == VK_LEFT)
			{
				// Get the current row and col
				unsigned long nCellRow, nCellCol;
				if (GetCurrentCell(&nCellRow, &nCellCol))
				{
					// If the selected cell is an even number cell (editable cell)
					if (nCellCol % 2 == 0)
					{
						if (nCellCol > 2)
						{
							// Go to previous editable cell if available
							this->SetCurrentCell(nCellRow, nCellCol - 2);
						}
						else if (nCellRow > 1)
						{
							// Get the last cell on the previous row to see if it is empty
							CString strResult = this->GetValueRowCol(nCellRow - 1, this->GetColCount());
							if (strResult.IsEmpty())
							{
								// Move the the last data cell on the previous row
								this->SetCurrentCell(nCellRow - 1, this->GetColCount() - 2);
							}
							else
							{
								// Go to the last editable cell (column 6) on the previous row
								this->SetCurrentCell(nCellRow - 1, this->GetColCount());
							}		
						}
						return true;
					}
					// If the selected cell is an odd number cell (noneditable cell)
					else
					{
						// Go to the previous editable cell in the same row
						nChar = VK_LEFT;
					}
				}
			}


			// if F2 is pressed
			if (nChar == VK_F2)
			{
				// if only the current cell has text and is 
				// not the non-editable cell
				unsigned long nCellRow, nCellCol;
				if (GetCurrentCell(&nCellRow, &nCellCol))
				{
					if (nCellCol%2 == 1)
					{
						return FALSE;
					}
					else if (nCellRow != 0 && nCellCol != 0)
					{
						CString cstrOldText = GetValueRowCol(nCellRow, nCellCol);
						if (cstrOldText.IsEmpty())
						{
							return FALSE;
						}
					}
				}
			}
		}

		bRet = CGXGridCore::OnGridKeyDown(nChar, nRepCnt, nFlags);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13435")

	return bRet;
}

//-------------------------------------------------------------------------------------------------
// protected methods
//-------------------------------------------------------------------------------------------------
BOOL DynamicInputGridWnd::PreTranslateMessage(MSG* pMsg)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	BOOL bRet = FALSE;

	try
	{
		if (!m_bTrackingSegments)
		{
			return bRet;
		}

		UINT nMsg = pMsg->message;
		if (nMsg == WM_MOUSEWHEEL)
		{
			short zDelta = (short) HIWORD (pMsg->wParam);
			int direction = GX_DOWN;
			if (zDelta > 0)
			{
				direction = GX_UP;
			}

			// if it's mouse wheel, scroll the grid up/down by one row
			DoScroll(direction, 1);
		}

		if (nMsg == WM_COMMAND)
		{
			UINT nID = (UINT)LOWORD(pMsg->wParam);
			bRet = onSelectContextMenu(nID);
		}

		if (!bRet)
		{
			bRet = CGXGridWnd::PreTranslateMessage(pMsg);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13436")

	return bRet;
}

//-------------------------------------------------------------------------------------------------
// Public methods
//-------------------------------------------------------------------------------------------------
void DynamicInputGridWnd::addCurrentSegment()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	// whehter or not the total number of segments gets incremented
	bool bTotalNumSegmentsIncremented = false;
	try
	{
		validObjects();
		
		// first make sure this segment's parameters are valid and sufficient
		if (!isCurrentSegmentCalculatable())
		{
			throw UCLIDException("ELI12242", "Insufficient or invalid parameter info to draw specified segment.");
		}
		
		ASSERT_RESOURCE_ALLOCATION("ELI12231", m_ipLatestSegmentInProgress != NULL);

		// get current segment parameters
		IIUnknownVectorPtr ipParams = getSegmentInfo();
		m_ipLatestSegmentInProgress->setParameters(ipParams);
		
		// increment total number of segments in the current sketch
		if (m_bTrackingSegments)
		{
			m_nTotalSegmentsDrawnByIcoMap++;
			// the total number of segments is incremented above
			bTotalNumSegmentsIncremented = true;
		}

		// add new segment to the end of the part
		if (m_eCurrentSegmentType == UCLID_FEATUREMGMTLib::kLine)
		{
			// add line segment
			ILineSegmentPtr ipLineSegment(m_ipLatestSegmentInProgress);
			m_ipDisplayAdapter->AddLineSegment(ipLineSegment);
		}
		else if (m_eCurrentSegmentType == UCLID_FEATUREMGMTLib::kArc)
		{
			// add arc segment
			IArcSegmentPtr ipArcSegment(m_ipLatestSegmentInProgress);
			m_ipDisplayAdapter->AddCurveSegment(ipArcSegment);
		}

		// Sometimes after adding the segment, this object will be reset to 
		// null. It is not supposed to happend. If it does, there must
		// be a defect somewhere.
		if (m_ipLatestSegmentInProgress == NULL)
		{
			throw UCLIDException("ELI12477", "Internal error! Please check with your software developer.");
		}

		// update the toggle state if the new segment is curve 
		// or internal/deflection angle line
		// get the toggle info
		if (m_eCurrentSegmentType == UCLID_FEATUREMGMTLib::kArc
			|| m_ipLatestSegmentInProgress->requireTangentInDirection() == VARIANT_TRUE)
		{
			string strToggleDirection(""), strToggleDeltaAngle("");
			int nSize = m_vecCurrentSegmentParams.size();
			for (int n=nSize-1; n>=0; n--)
			{
				IParameterTypeValuePairPtr ipParam = m_vecCurrentSegmentParams[n];
				if (ipParam->eParamType == kArcConcaveLeft)
				{
					strToggleDirection = ipParam->strValue;
					
					// if this is a line, no toggle delta angle
					if (m_eCurrentSegmentType == UCLID_FEATUREMGMTLib::kLine)
					{
						break;
					}
					
					// if toggle delta angle is already there
					if (!strToggleDeltaAngle.empty())
					{
						break;
					}
					
					continue;
				}
				
				if (ipParam->eParamType == kArcDeltaGreaterThan180Degrees)
				{
					strToggleDeltaAngle = ipParam->strValue;
					// if toggle direction is already there
					if (!strToggleDirection.empty())
					{
						break;
					}
					
					continue;
				}
			}
			
			// enable toggling if either is non empty
			// and never enable the confirm button
			m_pIcoMapUI->enableToggle(!strToggleDirection.empty(),
									  !strToggleDeltaAngle.empty());
			
			if (!strToggleDirection.empty())
			{
				m_pIcoMapUI->setToggleDirectionState(strToggleDirection == "1");
			}
			
			if (!strToggleDeltaAngle.empty())
			{
				m_pIcoMapUI->setToggleDeltaAngleState(strToggleDeltaAngle == "1");
			}
		}
		
		// update these if we are tracking segments
		if (m_bTrackingSegments)
		{
			// add the new segment to the vec
			m_ipCurrentPartSegments->PushBack(m_ipLatestSegmentInProgress);
			
			m_nCurrentSegmentIndexInPart = m_ipCurrentPartSegments->Size() - 1;
			
			// update the state
			m_pInputProcessor->updateState(kHasSegmentState);				
		}

		// this latest segment in progress has complete set of parameters
		m_bLatestSegmentInfoComplete = true;

		m_bCurrentSegmentModified = false;
	}
	catch (...)
	{
		// if the segment is the latest segment, remove it from the grid
		discardSegmentInProgress();

		// whether or not to decrement the total number of segments added
		if (bTotalNumSegmentsIncremented)
		{
			m_nTotalSegmentsDrawnByIcoMap--;
		}

		if (m_nCurrentSelectedRowIndex > 1)
		{
			int nNewIndex = m_nCurrentSelectedRowIndex-1;
			setRowsSelection(nNewIndex, 1);
			// set input type to none even the segment might
			// require toggle info
			m_eCurrentInputType = kNone;

			if (m_nCurrentSelectedRowIndex >= 1)
			{
				m_pInputProcessor->updateState(kHasSegmentState);
			}
			else
			{
				m_pInputProcessor->updateState(kHasPointState);
			}
		}
		throw;
	}
}
//-------------------------------------------------------------------------------------------------
void DynamicInputGridWnd::deactivateDIG()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	if (!m_bTrackingSegments)
	{
		return;
	}

	m_bFocusOnGrid = false;

	// first make sure the current cell text
	// has been properly processed if modified
	unsigned int nTotalRows = GetRowCount();
	if (m_CurrentCell.m_nRow <= nTotalRows 
		&& !m_CurrentCell.isNull() 
		&& m_CurrentCell.m_nCol % 2 == 0)
	{
		unsigned int nValueIndex = m_CurrentCell.m_nCol/2-1;
		if (nValueIndex < m_vecCurrentSegmentParams.size())
		{
			IParameterTypeValuePairPtr ipParam = m_vecCurrentSegmentParams[nValueIndex];
			ECurveParameterType eParamType = ipParam->eParamType;
			// stored direction is always in quadrant bearing format
			string strOldText = ipParam->strValue;
			strOldText = translateBDToCurrentFormat(eParamType, strOldText);

			// get whatever the user enters
			CString cstrInputText = GetValueRowCol(m_CurrentCell.m_nRow, m_CurrentCell.m_nCol);
			if (m_bIsCurrentCellModified)
			{
				m_bIsCurrentCellModified = false;
				m_ModifyingCell.resetCell();
				SetValueRange(CGXRange(m_CurrentCell.m_nRow, m_CurrentCell.m_nCol), strOldText.c_str());
				// send the input text for validation
				// Note: DO NOT use strNewText in case the reverse mode is set
				m_pIcoMapUI->setInput((LPCTSTR)cstrInputText);
			}
		}
	}

	// make sure the current selected row
	// is properly drawn if modified
	if (m_bCurrentSegmentModified)
	{
		try
		{
			try
			{
				// if the segment already drawn in the map,
				// redraw the segment
				if (m_nCurrentSegmentIndexInPart >= 0)
				{
					redrawCurrentSegment();
				}
			}
			catch (UCLIDException &ue)
			{
				ue.addHistoryRecord("ELI12609", "One or more of the segment parameter "
					"values are invalid. Original value(s) will be restored.");
				throw ue;
			}
		}
		catch (...)
		{
			// set selection back
			m_bCanChangeSelection = false;
			setRowsSelection(m_nCurrentSelectedRowIndex, 1, true, false);
			// refresh display
			refreshCurrentRecordDisplay();

			// set focus back to DIG
			SetFocus();
			throw;
		}
	}

	// DIG will not have any active cell to take in-place editing
	long nLastRow = GetRowCount();
	if (nLastRow > 0)
	{
		// select last row and update the row info, but 
		// do not flash the segment in the drawing
		setRowsSelection(nLastRow, 1, true, false);

		// scroll to the last row
		CRect rectDIG = GetGridRect();
		int nBottomVisibleRow = CalcBottomRowFromRect(rectDIG);
		if (nBottomVisibleRow < nLastRow)
		{
			ScrollCellInView(nLastRow, 0);
			refreshRowDraw(nLastRow);
		}
		
		//updateCurrentSegmentVariableSet(nLastRow - 1, true);
		
		m_nCurrentSelectedRowIndex = nLastRow;
	}

	// grid is not expecting any input since it 
	// doesn't have the focus
	m_eCurrentInputType = kNone;
	//m_CurrentCell.resetCell();

	if (m_pInputProcessor)
	{
		// update the input type
		m_pInputProcessor->notifyInputTypeChanged(false);
	}

	// Reset the current cell as grid is deactivated
	m_CurrentCell.resetCell();
}
//-------------------------------------------------------------------------------------------------
void DynamicInputGridWnd::deleteSketch()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	validObjects();

	m_ipDisplayAdapter->DeleteCurrentSketch();

	// reset all
	reset();
}
//-------------------------------------------------------------------------------------------------
void DynamicInputGridWnd::discardSegmentInProgress()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	validObjects();

	// update grid display 
	if (!m_bLatestSegmentInfoComplete)
	{
		// there's a segment in progress, remove the last row
		long nLastRow = GetRowCount();
		if (nLastRow > 0)
		{
			RemoveRows(nLastRow, nLastRow);
		}

		// reset current segment related variables
		updateCurrentSegmentVariableSet(-1);
		m_ipLatestSegmentInProgress = NULL;
		m_bLatestSegmentInfoComplete = true;
	}	
}
//-------------------------------------------------------------------------------------------------
void DynamicInputGridWnd::doResize() 
{
	if (!m_bInitGrid)
	{
		return;
	}

	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	long nWidth = getCurrentTrueGridWidth();

	// get total number of columns (excluding the row header)
	long nColCount = NUM_TYPE_VALUE_PAIR * 2;
	// new col width
	long nNewValueColWidth = (nWidth - TYPE_COL_WIDTH * NUM_TYPE_VALUE_PAIR)/NUM_TYPE_VALUE_PAIR;
	for (long n=1; n<=nColCount; n++)
	{
		long nColWidth = n%2 == 1 ? TYPE_COL_WIDTH : nNewValueColWidth;
		SetColWidth(n, n, nColWidth, NULL, GX_UPDATENOW, gxDo);
	}
}
//-------------------------------------------------------------------------------------------------
void DynamicInputGridWnd::finishCurrentPart()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	validObjects();

	if (m_bTrackingSegments)
	{
		if (m_ipCurrentPartStartPoint == NULL)
		{
			throw UCLIDException("ELI12125", "Please set start point for the part "
				"before calling finishCurentPart().");
		}
		
		// add this part to the feature
		if (m_ipFeature == NULL)
		{
			m_ipFeature.CreateInstance(CLSID_Feature);
			ASSERT_RESOURCE_ALLOCATION("ELI12123", m_ipFeature != NULL);
			
			// set feature type
			EFeatureType eFeatureType = (EFeatureType)m_ipDisplayAdapter->GetFeatureType();
			m_ipFeature->setFeatureType(eFeatureType);
		}
		
		// if current segment is supposed to be updated
		if (m_bCurrentSegmentModified)
		{
			try
			{
				try
				{
					// if the segment already drawn in the map,
					// redraw the segment
					if (m_nCurrentSegmentIndexInPart >= 0)
					{
						redrawCurrentSegment();
					}
				}
				catch (UCLIDException &ue)
				{
					ue.addHistoryRecord("ELI12610", "One or more of the segment parameter "
						"values are invalid. Original value(s) will be restored.");
					throw ue;
				}
			}
			catch (...)
			{
				// set selection back
				setRowsSelection(m_nCurrentSelectedRowIndex, 1, true, false);
				// refresh display
				refreshCurrentRecordDisplay();

				throw;
			}
		}

		// create a new part object
		IPartPtr ipThisPart(CLSID_Part);
		ASSERT_RESOURCE_ALLOCATION("ELI12124", ipThisPart != NULL);
		
		ipThisPart->setStartingPoint(m_ipCurrentPartStartPoint);
		long nSize = m_ipCurrentPartSegments->Size();
		for (long n=0; n<nSize; n++)
		{
			UCLID_FEATUREMGMTLib::IESSegmentPtr ipSegment = m_ipCurrentPartSegments->At(n);
			ipThisPart->addSegment(ipSegment);
		}
		
		// add the part to the feature
		m_ipFeature->addPart(ipThisPart);
		
		// reset this part
		m_ipCurrentPartStartPoint = NULL;
		m_ipCurrentPartSegments->Clear();
		
		updateCurrentSegmentVariableSet(-1);
		// start a new part
		m_nCurrentPartIndex++;
		
		// clear the contents of the grid
		long nRowCount = GetRowCount();
		if (nRowCount > 0)
		{
			// Remove all rows
			RemoveRows(1, nRowCount);
		}
	}

	m_ipLatestSegmentInProgress = NULL;

	// update the drawing map
	m_ipDisplayAdapter->FinishCurrentPart();
}
//-------------------------------------------------------------------------------------------------
string DynamicInputGridWnd::finishCurrentSketch()
{	
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	validObjects();

	// if we keep tracking of segments, we need to first 
	// store this last part before sketch is finished
	if (m_bTrackingSegments)
	{
		// make sure current editing part has been added to the feature already
		if (m_ipCurrentPartStartPoint != NULL 
			&& m_ipCurrentPartSegments->Size() > 0)
		{
			finishCurrentPart();
		}
	}

	// keep a copy of the feature created before the sketch
	// is finished to avoid having null feature
	IUCLDFeaturePtr ipCopyFeature(m_ipFeature);

	// update the drawing map
	_bstr_t bsFeatureID = m_ipDisplayAdapter->FinishCurrentSketch();

	// if we keep track of segments, we're able to store original
	// feature as entered
	if (ipCopyFeature != NULL)
	{
		// store the feature as string in the database
		m_ipAttributeManager->SetFeatureAttribute(bsFeatureID, 
							_bstr_t(STORE_FIELD_NAME.c_str()), ipCopyFeature);
	}

	// reset all
	reset();

	return (string)bsFeatureID;
}
//-------------------------------------------------------------------------------------------------
EInputType DynamicInputGridWnd::getCurrentInputType() const
{
	return m_eCurrentInputType;
}
//-------------------------------------------------------------------------------------------------
ESegmentType DynamicInputGridWnd::getCurrentSegmentType() const
{
	return m_eCurrentSegmentType;
}
//-------------------------------------------------------------------------------------------------
string DynamicInputGridWnd::getErrorSegmentReport()
{
	string strErrorSegment("");

	long nSize = m_ipCurrentPartSegments->Size();
	// only if we're tracking the segments
	if (!m_bTrackingSegments || (nSize == 0 && m_nTotalSegmentsDrawnByIcoMap == 0))
	{
		return "";
	}

	// the error segment starts from the end point of the
	// last segment to the start point of the first segment
	IPartPtr ipThisPart(CLSID_Part);
	ASSERT_RESOURCE_ALLOCATION("ELI12512", ipThisPart != NULL);
	
	// create a part object
	ipThisPart->setStartingPoint(m_ipCurrentPartStartPoint);
	UCLID_FEATUREMGMTLib::IESSegmentPtr ipPrevSegment(NULL);
	for (long n=0; n<nSize; n++)
	{
		UCLID_FEATUREMGMTLib::IESSegmentPtr ipSegment = m_ipCurrentPartSegments->At(n);
		if (n > 0 && ipSegment->requireTangentInDirection() == VARIANT_TRUE)
		{
			_bstr_t _bstrTangentInDirection = ipPrevSegment->getTangentOutDirection();
			ipSegment->setTangentInDirection(_bstrTangentInDirection);
		}

		ipThisPart->addSegment(ipSegment);
		ipPrevSegment = ipSegment;
	}

	if (nSize < m_nTotalSegmentsDrawnByIcoMap && m_ipLatestSegmentInProgress != NULL)
	{
		if (ipPrevSegment != NULL && m_ipLatestSegmentInProgress->requireTangentInDirection() == VARIANT_TRUE)
		{
			_bstr_t _bstrTangentInDirection = ipPrevSegment->getTangentOutDirection();
			m_ipLatestSegmentInProgress->setTangentInDirection(_bstrTangentInDirection);
		}

		// add the last segment to the part
		ipThisPart->addSegment(m_ipLatestSegmentInProgress);
	}

	// get end point from the part
	ICartographicPointPtr ipEndPoint = ipThisPart->getEndingPoint();

	if (ipEndPoint == NULL)
	{
		// no error
		return "";
	}

	// if start and end point coincide at the same spot, 
	// there's no closing error
	if (m_ipCurrentPartStartPoint->IsEqual(ipEndPoint) == VARIANT_TRUE)
	{
		strErrorSegment = "This part is perfectly closed."; 
		return strErrorSegment;
	}

	// get the error segment, which starts from the end point
	// of the part and ends at the start point of the part
	double dX, dY;
	ipEndPoint->GetPointInXY(&dX, &dY);
	TPPoint tpPointStart(dX, dY);
	m_ipCurrentPartStartPoint->GetPointInXY(&dX, &dY);
	TPPoint tpPointEnd(dX, dY);

	// always work in normal mode here
	ReverseModeValueRestorer rmvr;
	AbstractMeasurement::workInReverseMode(false);

	// the error bearing
	Bearing bearing;
	bearing.evaluate(tpPointStart, tpPointEnd);
	string strErrorDirection = 
		m_dirHelper.polarAngleInRadiansToDirectionInString(bearing.getRadians());
	strErrorSegment = "Error Direction: " + strErrorDirection;

	// the error distance (in current unit)
	strErrorSegment += "    Error Distance: ";
	double dErrorDistance = tpPointStart.distanceTo(tpPointEnd);
	strErrorSegment += getProperDistanceFormat(dErrorDistance);

	return strErrorSegment;
}
//-------------------------------------------------------------------------------------------------
void DynamicInputGridWnd::initDIG()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	Initialize();
	
	GetParam()->EnableUndo(FALSE);

	// Set full-row selection
	GetParam()->EnableSelection(GX_SELROW);
	GetParam()->SetSpecialMode(GX_MODELBOX_SS);

	// Disable row resizing
	GetParam()->EnableTrackRowHeight(FALSE);

	SetAutoScroll(TRUE);

	// enable grid tooltip
	EnableGridToolTips();

	// Tooltips
	GetParam()->GetStylesMap()->AddUserAttribute(GX_IDS_UA_TOOLTIPTEXT, 
         CGXStyle().SetWrapText(TRUE).SetAutoSize(TRUE));


	// hard code number of columns in the grid
	long nColCount = NUM_TYPE_VALUE_PAIR * 2;
	SetColCount(nColCount);

	// set row header as read-only
	SetStyleRange(CGXRange().SetCols(0),
				  CGXStyle()
				    .SetFont(CGXFont().SetBold(TRUE))
				  );

	SetColWidth(0, 0, ROW_HEADER_WIDTH, NULL, GX_UPDATENOW, gxDo);

	// Set first col Header text
	SetStyleRange(CGXRange(0, 0),
				  CGXStyle()
					.SetValue("Segment#")
					.SetHorizontalAlignment(DT_CENTER)
					.SetFont(CGXFont().SetBold(TRUE))
				  );

	// set row header style
	SetStyleRange(CGXRange().SetCols(0),
				  CGXStyle()
					.SetVerticalAlignment(DT_VCENTER)
					.SetHorizontalAlignment(DT_CENTER)
					.SetFont(CGXFont().SetBold(TRUE))
				  );


	// merge cells from (0, 1) to (0 ,6)
	SetCoveredCellsRowCol(0, 1, 0, NUM_TYPE_VALUE_PAIR*2);

	// set second col header text
	SetStyleRange(CGXRange(0, 1, 0, NUM_TYPE_VALUE_PAIR*2),
				  CGXStyle()
					.SetValue("Parameters")
					.SetVerticalAlignment(DT_VCENTER)
					.SetHorizontalAlignment(DT_CENTER)
					.SetFont(CGXFont().SetBold(TRUE))
				  );

	// current true grid width excluding some extra
	long nGridWidth = getCurrentTrueGridWidth();

	// value col width
	long nValueColWidth = (long)((nGridWidth 
		- TYPE_COL_WIDTH * NUM_TYPE_VALUE_PAIR) 
		/ NUM_TYPE_VALUE_PAIR);

	for (int n=1; n<=nColCount; n++)
	{
		bool bOddNumber = n%2 == 1;
		long nColWidth = bOddNumber ? TYPE_COL_WIDTH : nValueColWidth;
		SetColWidth(n, n, nColWidth, NULL, GX_UPDATENOW, gxDo);

		WORD nControl = bOddNumber ? GX_IDS_CTRL_STATIC : GX_IDS_CTRL_EDIT;
		BOOL bBold = bOddNumber ? TRUE : FALSE;
		// Editable column is single-line edit box
		SetStyleRange(CGXRange().SetCols(n),
						CGXStyle()
							.SetControl(nControl)
							.SetAllowEnter(FALSE)
							.SetAutoSize(FALSE)
							.SetWrapText(FALSE)
							.SetHorizontalAlignment(DT_CENTER)
							.SetVerticalAlignment(DT_VCENTER)
							.SetFont(CGXFont().SetBold(bBold))
					  );
		if (bOddNumber)
		{
			// only set odd column text color to redish
			SetStyleRange(CGXRange().SetCols(n),
							CGXStyle()
							.SetTextColor(RGB(230, 120, 0))
						  );
		}
	}

	// enable dragging move rows by user
	GetParam()->EnableMoveRows(TRUE);
//	GetParam()->EnableUndo(TRUE);

	// reset internal variables
	reset();

	m_bInitGrid = true;
}
//-------------------------------------------------------------------------------------------------
bool DynamicInputGridWnd::needTracking()
{
	return m_bTrackingSegments;
}
//-------------------------------------------------------------------------------------------------
bool DynamicInputGridWnd::notifySketchModified(long nNumOfSegmentsOfSketch)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	if (m_bBlockSketchNotification)
	{
		// if sketch notification needs to be blocked, no
		// further process required
		return m_bBlockSketchNotification;
	}

	if (!m_bTrackingSegments)
	{
		// when it's not tracking segments, always in-sync with actual segments
		m_nTotalSegmentsDrawnByIcoMap = nNumOfSegmentsOfSketch;
	}

	if (m_bTrackingSegments)
	{
		bool bHasPoint = nNumOfSegmentsOfSketch > 0 ? true : false;
		// there might not be any start point if segment number is 0
		if (nNumOfSegmentsOfSketch == 0 )
		{
			double dX, dY;
			bHasPoint = m_ipDisplayAdapter->GetLastPoint(&dX, &dY) == VARIANT_TRUE;
		}

		// if number of segments added is different from actual number 
		// of segments in the drawing, or if there's no segment and no
		// start point in the drawing
		if (m_nTotalSegmentsDrawnByIcoMap != nNumOfSegmentsOfSketch
			|| (nNumOfSegmentsOfSketch == 0 && !bHasPoint))
		{
			// stop tracking
			m_bTrackingSegments = false;

			// clear the grid 
			reset();

			// when it's not tracking segments, always in-sync with actual segments
			m_nTotalSegmentsDrawnByIcoMap = nNumOfSegmentsOfSketch;
		}
	}

	// if we are currently tracking each segments in the grid, there's no
	// need to further process this notification outside grid.
	return m_bTrackingSegments;
}
//-------------------------------------------------------------------------------------------------
bool DynamicInputGridWnd::processInput(const string& strInputText)
{
	// whether or not the input has been processed by this grid
	// ture - is processed by this grid, false otherwise

	validObjects();

	// what's current input type
	switch (m_eCurrentInputType)
	{
	case kAngle:
	case kBearing:
	case kDistance:
	case kToggleCurve:
	case kToggleAngle:
		{
			// store current parameter
			setSegmentParameter(m_eCurrentSegmentType, 
						m_eCurrentEditingParamType, strInputText);

			// redraw the segment
			//redrawCurrentSegment();
		}
		break;
	default:
		return false;
		break;
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
void DynamicInputGridWnd::redrawCurrentSegment()
{
	validObjects();
	
	// first make sure this segment's parameters are valid and sufficient
	if (!isCurrentSegmentCalculatable())
	{
		throw UCLIDException("ELI12084", "Insufficient or invalid parameter info to draw specified segment.");
	}
	
	UCLID_FEATUREMGMTLib::IESSegmentPtr ipCurrentSegment(NULL);
	
	if (!m_bTrackingSegments)
	{
		ipCurrentSegment = m_ipLatestSegmentInProgress;
	}
	else
	{
		// first make sure current segment has all 
		// necessary parameters for drawing
		if (m_nCurrentSegmentIndexInPart < m_ipCurrentPartSegments->Size())
		{
			ipCurrentSegment = m_ipCurrentPartSegments->At(m_nCurrentSegmentIndexInPart);
		}
		else
		{
			UCLIDException ue("ELI12086", "Invalid segment index");
			ue.addDebugInfo("Index", m_nCurrentSegmentIndexInPart);
			throw ue;
		}
	}
	
	ASSERT_RESOURCE_ALLOCATION("ELI12081", ipCurrentSegment != NULL);
	
	// get current segment parameters
	IIUnknownVectorPtr ipParams = getSegmentInfo();
	ipCurrentSegment->setParameters(ipParams);
	
	{
		
		// temporary block sketch notification
		ValueRestorer<bool> VR(m_bBlockSketchNotification);
		m_bBlockSketchNotification = true;

		// if it's the last segment of the sketch
		if (!m_bTrackingSegments
			|| (m_bTrackingSegments 
			&& m_nCurrentSegmentIndexInPart == m_ipCurrentPartSegments->Size()-1))
		{
			m_ipDisplayAdapter->EraseLastSegment();
			
			if (m_eCurrentSegmentType == UCLID_FEATUREMGMTLib::kLine)
			{
				ILineSegmentPtr ipLine(ipCurrentSegment);
				m_ipDisplayAdapter->AddLineSegment(ipLine);
			}
			else if (m_eCurrentSegmentType == UCLID_FEATUREMGMTLib::kArc)
			{
				IArcSegmentPtr ipCurve(ipCurrentSegment);
				m_ipDisplayAdapter->AddCurveSegment(ipCurve);
			}
			
			m_bCurrentSegmentModified = false;
			
			return;
		}

		// update segments
		m_ipDisplayAdapter->UpdateSegments(m_nCurrentPartIndex,
			m_nCurrentSegmentIndexInPart, m_ipCurrentPartSegments);
	}
	
	m_bCurrentSegmentModified = false;
}
//-------------------------------------------------------------------------------------------------
void DynamicInputGridWnd::reset()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	m_nCurrentSelectedRowIndex = -1;
	m_ipCurrentPartStartPoint = NULL;
	m_ipFeature = NULL;
	m_nCurrentPartIndex = 0;
	m_ipCurrentPartSegments->Clear();
	m_nTotalSegmentsDrawnByIcoMap = 0;

	updateCurrentSegmentVariableSet(-1);
	m_ipLatestSegmentInProgress = NULL;
	if (m_pIcoMapUI != NULL)
	{
		m_pIcoMapUI->enableToggle(false, false);
	}

	// clear the grid content
	long nRowCount = GetRowCount();
	if (nRowCount > 0)
	{
		// Remove all rows
		RemoveRows(1, nRowCount);
	}

	m_CurrentCell.resetCell();

	m_ModifyingCell.resetCell();
	m_bIsCurrentCellModified = false;

	m_bFocusOnGrid = false;

	if (m_pInputProcessor)
	{
		// update the input type
		m_pInputProcessor->notifyInputTypeChanged(false);
	}
}
//-------------------------------------------------------------------------------------------------
void DynamicInputGridWnd::setAttributeManager(IAttributeManager* pAttributeManager)
{
	m_ipAttributeManager = pAttributeManager;
	ASSERT_RESOURCE_ALLOCATION("ELI12128", m_ipAttributeManager != NULL);
}
//-------------------------------------------------------------------------------------------------
void DynamicInputGridWnd::setDisplayAdapter(IDisplayAdapter* pDisplayAdapter)
{
	m_ipDisplayAdapter = pDisplayAdapter;
	ASSERT_RESOURCE_ALLOCATION("ELI11937", m_ipDisplayAdapter != NULL);
}
//-------------------------------------------------------------------------------------------------
void DynamicInputGridWnd::setIcoMapUI(IIcoMapUI* pIcoMapUI)
{
	m_pIcoMapUI = pIcoMapUI;
	ASSERT_RESOURCE_ALLOCATION("ELI12121", m_pIcoMapUI != NULL);
}
//-------------------------------------------------------------------------------------------------
void DynamicInputGridWnd::setInputProcessor(InputProcessor* pInputProcessor)
{
	m_pInputProcessor = pInputProcessor;
	ASSERT_RESOURCE_ALLOCATION("ELI12248", m_pInputProcessor != NULL);
}
//-------------------------------------------------------------------------------------------------
void DynamicInputGridWnd::setSegmentParameter(ESegmentType eSegmentType,
									   ECurveParameterType eParamType,
									   const string& strValue,
									   bool bAddNewRecord)
{
	if (eSegmentType == kInvalidSegmentType)
	{
		throw UCLIDException("ELI12611", "Invalid segment type");
	}

	if (eParamType == kInvalidParameterType)
	{
		throw UCLIDException("ELI12612", "Invalid Parameter type");
	}

	validObjects();

	if (bAddNewRecord)
	{
		updateCurrentSegmentVariableSet(-1);
		m_ipLatestSegmentInProgress = NULL;
		m_bLatestSegmentInfoComplete = false;
		// disable toggle first
		m_pIcoMapUI->enableToggle(false, false);
	}

	m_eCurrentSegmentType = eSegmentType;
	// internally store the parameter pair
	internalStoreParameter(eParamType, strValue);

	if (!m_bTrackingSegments)
	{
		// no display necessary
		return;
	}

	// never display any toggle info
	if (eParamType == kArcConcaveLeft 
		|| eParamType == kArcDeltaGreaterThan180Degrees)
	{
		return;
	}

	// update the segment record displayed in the grid
	int nNumOfParams = m_vecCurrentSegmentParams.size();
	// m_nCurrentSelectedRowIndex is 1-based
	// nCurrentRecordIndex is 0-based
	int nCurrentRecordIndex = m_nCurrentSelectedRowIndex - 1;
	if (nCurrentRecordIndex < 0 || bAddNewRecord)
	{
		// this is a new segment that is going to be appended to the end 
		// of the grid list
		nCurrentRecordIndex = m_ipCurrentPartSegments->Size();
	}
	for (int n=0; n<nNumOfParams; n++)
	{
		IParameterTypeValuePairPtr ipParam = m_vecCurrentSegmentParams[n];
		ECurveParameterType eThisParamType = ipParam->eParamType;
		if (eThisParamType == eParamType)
		{
			// reset cell value
			displayParameterTypeValuePair(nCurrentRecordIndex, n, ipParam);
			break;
		}
	}
}
//-------------------------------------------------------------------------------------------------
void DynamicInputGridWnd::setStartPointForPart(ICartographicPointPtr ipStartPoint)
{
	validObjects();

	m_ipCurrentPartStartPoint = ipStartPoint;
	ASSERT_ARGUMENT("ELI12069", m_ipCurrentPartStartPoint != NULL);

	// if DIG is currently not tracking/displaying segments info
	// turn it on if total number of segments added is 0 and this
	// is the first part of the sketch
	if (!m_bTrackingSegments 
		&& m_nCurrentPartIndex == 0
		&& m_nTotalSegmentsDrawnByIcoMap == 0)
	{
		m_bTrackingSegments = true;
	}

	// set start point in display adapter will actually draw 
	// out the start point in the map
	double dX = 0.0, dY = 0.0;
	ipStartPoint->GetPointInXY(&dX, &dY);
	m_ipDisplayAdapter->SetStartPointForNextPart(dX, dY);
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void DynamicInputGridWnd::displayParameterTypeValuePair(int nRecordIndex,
													int nParameterIndex, 
													IParameterTypeValuePairPtr ipTypeValuePair)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	if (!m_bTrackingSegments)
	{
		return;
	}

	// parameter type will be stored in the odd numberd col
	// parameter value will be stored in the even numbered col

	// nRecordIndex is 0-based
	long nRow = nRecordIndex + 1;

	// current total number of rows
	long nNumOfRows = GetRowCount();

	if (nRow > nNumOfRows+1 || nRow <= 0)
	{
		UCLIDException ue("ELI12472", "Invalid row index");
		ue.addDebugInfo("TotalNumOfRows", nNumOfRows);
		ue.addDebugInfo("Row", nRow);
		throw ue;
	}

	if (nRow > nNumOfRows)
	{
		// add the row to the end
		InsertRows(nRow, 1);
		// make all cells read-only first
		SetStyleRange(CGXRange().SetCells(nRow, 1, nRow, NUM_TYPE_VALUE_PAIR * 2),
					  CGXStyle().SetReadOnly(TRUE));

		// scroll to the last row if not visible
		ScrollCellInView(nRow, 0);

		// select this new row
		setRowsSelection(nRow, 1, false);

		m_nCurrentSelectedRowIndex = nRow;
	}

	// nParameterIndex is 0-based
	int nTypeCol = nParameterIndex * 2 + 1;
	int nValueCol = (nParameterIndex + 1) * 2;

	// get the parameter type and value
	ECurveParameterType eParamType = ipTypeValuePair->eParamType;
	string strType = getCurveTypeName(eParamType);
	string strTypeDesc = getCurveTypeName(eParamType, false);
	string strValue = _bstr_t(ipTypeValuePair->strValue);
	// convert to current format for bearing and distance
	strValue = translateBDToCurrentFormat(eParamType, strValue);

	// unlock the readonly if the cell is readonly
	GetParam()->SetLockReadOnly(FALSE);
	
	// update the display for type and value info
	SetStyleRange(CGXRange(nRow, nTypeCol),
				  CGXStyle()
				  .SetReadOnly(FALSE)
				  .SetValue(strType.c_str())
				  .SetUserAttribute(GX_IDS_UA_TOOLTIPTEXT, strTypeDesc.c_str()));
	SetStyleRange(CGXRange(nRow, nValueCol),
				  CGXStyle()
				  .SetReadOnly(FALSE)
				  .SetValue(strValue.c_str()));

	GetParam()->SetLockReadOnly(TRUE);
}
//-------------------------------------------------------------------------------------------------
long DynamicInputGridWnd::getCurrentTrueGridWidth()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	// Determine overall width and width of Value column
	CRect rectGrid;
	GetClientRect(&rectGrid);

	// get the width of the grid excluding the width of the row header and space
	long nWidth = rectGrid.Width() - ROW_HEADER_WIDTH - 1;

	return nWidth;
}
//-------------------------------------------------------------------------------------------------
string DynamicInputGridWnd::getCurveTypeName(ECurveParameterType eCurveParamType, 
											 bool bAbbrievated)
{
	string strCurveTypeName("");

	switch (eCurveParamType)
	{
	case kLineDeflectionAngle:
		strCurveTypeName = bAbbrievated ? "LDA" : "Line Deflection Angle";
		break;
	case kLineInternalAngle:
		strCurveTypeName = bAbbrievated ? "LIA" : "Line Internal Angle";
		break;
	case kLineBearing:
		strCurveTypeName = bAbbrievated ? "LDR" : "Line Direction";
		break;
	case kLineDistance:
		strCurveTypeName = bAbbrievated ? "LDS" : "Line Distance";
		break;
	case kArcDelta:
		strCurveTypeName = bAbbrievated ? "ADT" : "Arc Delta Angle";
		break;
	case kArcStartAngle:
		strCurveTypeName = bAbbrievated ? "ASA" : "Arc Start Angle";
		break;
	case kArcEndAngle:
		strCurveTypeName = bAbbrievated ? "AEA" : "Arc End Angle";
		break;
	case kArcDegreeOfCurveChordDef:
		strCurveTypeName = bAbbrievated ? "ADC" : "Arc Degree Of Curve";
		break;
	case kArcDegreeOfCurveArcDef:
		strCurveTypeName = bAbbrievated ? "ADA" : "Arc Degrees Of Curve (Arc Definition)";
		break;
	case kArcTangentInBearing:
		strCurveTypeName = bAbbrievated ? "ATI" : "Tangent-in Direction";
		break;
	case kArcTangentOutBearing:
		strCurveTypeName = bAbbrievated ? "ATO" : "Tangent-out Direction";
		break;
	case kArcChordBearing:
		strCurveTypeName = bAbbrievated ? "ACD" : "Arc Chord Direction";
		break;
	case kArcRadialInBearing:
		strCurveTypeName = bAbbrievated ? "ARI" : "Arc Radial-in Direction";
		break;
	case kArcRadialOutBearing:
		strCurveTypeName = bAbbrievated ? "ARO" : "Arc Radial-out Direction";
		break;
	case kArcRadius:
		strCurveTypeName = bAbbrievated ? "ARS" : "Arc Radius";
		break;
	case kArcLength:
		strCurveTypeName = bAbbrievated ? "ALG" : "Arc Length";
		break;
	case kArcChordLength:
		strCurveTypeName = bAbbrievated ? "ACL" : "Arc Chord Length";
		break;
	case kArcExternalDistance:
		strCurveTypeName = bAbbrievated ? "AED" : "Arc External Distance";
		break;
	case kArcMiddleOrdinate:
		strCurveTypeName = bAbbrievated ? "AMO" : "Arc Middle Ordinate";
		break;
	case kArcTangentDistance:
		strCurveTypeName = bAbbrievated ? "ATD" : "Arc Tangent Distance";
		break;
	}

	return strCurveTypeName;
}
//-------------------------------------------------------------------------------------------------
IESSegmentPtr DynamicInputGridWnd::getDuplicateSegment(IESSegmentPtr ipToBeCopiedSegment)
{
	IESSegmentPtr ipDuplicate(NULL);
	ESegmentType eSegmentType = ipToBeCopiedSegment->getSegmentType();
	if (eSegmentType == UCLID_FEATUREMGMTLib::kLine)
	{
		// create a new line segment
		ILineSegmentPtr ipLineSegment(CLSID_LineSegment);
		ipDuplicate = ipLineSegment;
	}
	else if (eSegmentType == UCLID_FEATUREMGMTLib::kArc)
	{
		// create a new arc segment
		IArcSegmentPtr ipArcSegment(CLSID_ArcSegment);
		ipDuplicate = ipArcSegment;
	}
	ASSERT_RESOURCE_ALLOCATION("ELI12479", ipDuplicate != NULL);

	// creat a new vector
	IIUnknownVectorPtr ipNewParams(CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION("ELI12481", ipNewParams != NULL);	

	IIUnknownVectorPtr ipParams = ipToBeCopiedSegment->getParameters();
	long nSize = ipParams->Size();
	for (long n=0; n<nSize; n++)
	{
		IParameterTypeValuePairPtr ipNewParam(CLSID_ParameterTypeValuePair);
		ASSERT_RESOURCE_ALLOCATION("ELI12480", ipNewParam != NULL);
		IParameterTypeValuePairPtr ipToBeCopiedParam = ipParams->At(n);
		ipNewParam->eParamType = ipToBeCopiedParam->eParamType;
		ipNewParam->strValue = ipToBeCopiedParam->strValue;

		ipNewParams->PushBack(ipNewParam);
	}

	ipDuplicate->setParameters(ipNewParams);
	
	return ipDuplicate;
}
//-------------------------------------------------------------------------------------------------
string DynamicInputGridWnd::getProperDistanceFormat(double dDistanceInCurrentUnit)
{
	// get the current distance unit
	string strUnit(m_distanceCore.getStringFromUnit( 
								m_distanceCore.getCurrentDistanceUnit()));
	// based on the decimal place set in the icomap options
	char pszFormat[30];
	int	iOptionValue = IcoMapOptions::sGetInstance().getPrecisionDigits();
	sprintf_s(pszFormat, sizeof(pszFormat), "%%.%df %s", iOptionValue, strUnit.c_str());
	char pszConvertedDistance[100];
	sprintf_s(pszConvertedDistance, sizeof(pszConvertedDistance), pszFormat, dDistanceInCurrentUnit);

	return (LPCTSTR)pszConvertedDistance;
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr DynamicInputGridWnd::getSegmentInfo()
{
	IIUnknownVectorPtr ipNewParams(CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION("ELI12082", ipNewParams != NULL);

	int nSize = m_vecCurrentSegmentParams.size();
	for (int n=0; n<nSize; n++)
	{
		IParameterTypeValuePairPtr ipParam(CLSID_ParameterTypeValuePair);
		ASSERT_RESOURCE_ALLOCATION("ELI12618", ipParam != NULL);

		IParameterTypeValuePairPtr ipOldParam = m_vecCurrentSegmentParams[n];
		ipParam->eParamType = ipOldParam->eParamType;
		ipParam->strValue = ipOldParam->strValue;

		ipNewParams->PushBack(ipParam);
	}

	return ipNewParams;
}
//-------------------------------------------------------------------------------------------------
void DynamicInputGridWnd::internalStoreParameter(ECurveParameterType eParamType,
												 const string& strValue)
{
	IParameterTypeValuePairPtr ipParam(NULL);

	string strNewValue(strValue);
	// convert all direction value into quadrant bearing format
	EInputType eInputType = translateParameterTypeToInputType(eParamType);
	if (eInputType == kBearing)
	{
		// The input string is always representing the normal mode, we
		// need to get the acutal value which counts the reverse fact.
		m_dirHelper.evaluateDirection(strValue);
		// what's the actual value of the direction as polar angle
		double dAsPolarAngleRadians = m_dirHelper.getPolarAngleRadians();
		Angle::setPrevInReverseMode( Angle::isInReverseMode() );

		Bearing bearing;
		bearing.evaluateRadians(dAsPolarAngleRadians);

		// always store direction as quadrant bearing format
		strNewValue = bearing.asString();
	}

	// if the curve parameter type doesn't exist, create a new one
	int nNumOfParams = m_vecCurrentSegmentParams.size();
	for (int n=0; n<nNumOfParams; n++)
	{
		ECurveParameterType eThisParamType = m_vecCurrentSegmentParams[n]->eParamType;
		if (eThisParamType == eParamType)
		{
			ipParam = m_vecCurrentSegmentParams[n];
			// reset value if different
			string strOldValue = ipParam->strValue;
			if (strOldValue == strNewValue)
			{
				// no change of the value
				return;
			}

			ipParam->strValue = _bstr_t(strNewValue.c_str());
			break;
		}
	}

	// if this is a new parameter to the segment
	if (ipParam == NULL)
	{
		// create a new parameter type value pair object
		ipParam.CreateInstance(CLSID_ParameterTypeValuePair);
		ASSERT_RESOURCE_ALLOCATION("ELI12070", ipParam != NULL);

		ipParam->eParamType = eParamType;
		ipParam->strValue = _bstr_t(strNewValue.c_str());

		// store it in the vec
		m_vecCurrentSegmentParams.push_back(ipParam);

		// this is the latest segment
		if (m_ipLatestSegmentInProgress == NULL)
		{
			if (m_eCurrentSegmentType == UCLID_FEATUREMGMTLib::kLine)
			{
				// create a new line segment
				ILineSegmentPtr ipLineSegment(CLSID_LineSegment);
				m_ipLatestSegmentInProgress = ipLineSegment;
			}
			else if (m_eCurrentSegmentType == UCLID_FEATUREMGMTLib::kArc)
			{
				// create a new arc segment
				IArcSegmentPtr ipArcSegment(CLSID_ArcSegment);
				m_ipLatestSegmentInProgress = ipArcSegment;
			}
			else
			{
				throw UCLIDException("ELI12080", "Invalid segment type");
			}
			ASSERT_RESOURCE_ALLOCATION("ELI12145", m_ipLatestSegmentInProgress != NULL);
		}
	}

	// if this is the latest segment in progress
	if (m_nCurrentSegmentIndexInPart < 0 && m_ipLatestSegmentInProgress != NULL)
	{
		// store info in the segment in progress
		IIUnknownVectorPtr ipParams = getSegmentInfo();
		m_ipLatestSegmentInProgress->setParameters(ipParams);
	}

	// set CCE for validating curve only
	if (m_eCurrentSegmentType == kArc)
	{
		setCCEParameter(eParamType, strNewValue);
	}

	m_bCurrentSegmentModified = true;
}
//-------------------------------------------------------------------------------------------------
bool DynamicInputGridWnd::isCurrentSegmentCalculatable()
{
	bool bRet = false;

	switch (m_eCurrentSegmentType)
	{
	case UCLID_FEATUREMGMTLib::kLine:
		{
			// whether or not the line is using internal/deflection angle
			bool bFromInternalDeflectionAngle = false;
			// if it's from internal/deflection angle, then is there
			// some info about if the line is to the left or right of
			// the previous line
			bool bHasDirectionInfo = false;
			// whether the line has bearing info
			bool bHasBearing = false;
			// if the line has distance info
			bool bHasDistance = false;

			// go through all parameters
			int nSize = m_vecCurrentSegmentParams.size();
			for (int n=0; n<nSize; n++)
			{
				IParameterTypeValuePairPtr ipParam = m_vecCurrentSegmentParams[n];
				ECurveParameterType eCurveType = ipParam->eParamType;
				switch (eCurveType)
				{
				case kLineDeflectionAngle:
				case kLineInternalAngle:
					bFromInternalDeflectionAngle = true;
					break;
				case kLineBearing:
					bHasBearing = true;
					break;
				case kLineDistance:
					bHasDistance = true;
					break;
				case kArcConcaveLeft:
					bHasDirectionInfo = true;
					break;
				}
			}

			if (bHasBearing && bHasDistance)
			{
				bRet = true;
			}
			else if (bFromInternalDeflectionAngle && bHasDirectionInfo && bHasDistance)
			{
				// this line can't be the first segment in current part
				if (m_nCurrentSegmentIndexInPart == 0)
				{
					throw UCLIDException("ELI12075", "Line segment that is formed by "
					"internal/deflection angle can't be placed as first segment of current part.");
				}

				bRet = true;
			}
		}
		break;
	case UCLID_FEATUREMGMTLib::kArc:
		{
			// set a dummy start point
			ma_pCCE->setCurvePointParameter(kArcStartingPoint, 0.0, 0.0);
			// try to get mid, end and tangent-out
			string strTemp = ma_pCCE->getCurveParameter(kArcMidPoint);
			strTemp = ma_pCCE->getCurveParameter(kArcEndingPoint);
			strTemp = ma_pCCE->getCurveParameter(kArcTangentOutBearing);
			// if no exception throw, it's all good
			bRet = true;
		}
		break;
	default:
		throw UCLIDException("ELI12087", "Invalid segment type.");
		break;
	}

	return bRet;
}
//-------------------------------------------------------------------------------------------------
BOOL DynamicInputGridWnd::onSelectContextMenu(UINT nID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	if (nID >= ID_DIG_INSERT_DUPLICATE && nID <= ID_DIG_DOWN)
	{
		validObjects();

		int nTotalRows = GetRowCount();
		switch (nID)
		{
		case ID_DIG_INSERT_DUPLICATE:
			{
				// copy current segment to the new one
				long nCurrentIndex = m_nCurrentSelectedRowIndex-1;
				IESSegmentPtr ipCurrentSegment = m_ipCurrentPartSegments->At(nCurrentIndex);
				ASSERT_RESOURCE_ALLOCATION("ELI12482", ipCurrentSegment != NULL);
				IESSegmentPtr ipDuplicate = getDuplicateSegment(ipCurrentSegment);

				// insert the new one in front of the selected segment
				m_ipCurrentPartSegments->Insert(nCurrentIndex, ipDuplicate);

				if (m_bTrackingSegments)
				{
					m_nTotalSegmentsDrawnByIcoMap++;
				}
				
				m_ipDisplayAdapter->UpdateSegments(m_nCurrentPartIndex, 
					nCurrentIndex, m_ipCurrentPartSegments);
				
				// insert a blank row above the currently selected row
				InsertRows(m_nCurrentSelectedRowIndex, 1);
				
				unsigned int nToBeCopiedRow = m_nCurrentSelectedRowIndex+1;
				for (int nCol=1; nCol<=NUM_TYPE_VALUE_PAIR*2; nCol++)
				{
					if (!GetValueRowCol(nToBeCopiedRow, nCol).IsEmpty())
					{
						// copy the bigger row to the blank row
						CopyCells(CGXRange().SetCells(nToBeCopiedRow, nCol),
									m_nCurrentSelectedRowIndex, nCol);
					}
				}

				// select the original row
				setRowsSelection(m_nCurrentSelectedRowIndex+1, 1, true, false);
			}
			break;
		case ID_DIG_REMOVE:
			{
				int nReturn = MessageBox("Delete current segment?", "Confirm Delete", 
					MB_YESNO | MB_ICONQUESTION | MB_DEFBUTTON2 );
				
				// Only delete the sketch if user agrees
				if (nReturn == IDNO)
				{
					return TRUE;
				}

				// remove it from the drawing
				long nCurrentIndex = m_nCurrentSelectedRowIndex-1;
				// whether or not the segment is last segment in progress
				bool bIsLastSegmentInProgress = false;
				if (m_nCurrentSelectedRowIndex == nTotalRows
					&& !m_bLatestSegmentInfoComplete)
				{
					bIsLastSegmentInProgress = true;
				}

				if (!bIsLastSegmentInProgress)
				{
					// only remove the segment from the vec if this 
					// segment is not latest segment in progress
					m_ipCurrentPartSegments->Remove(nCurrentIndex);
				}
				
				// only decrement the count if we are tracking the 
				// segments and the segment is not last segment in progress
				if (m_bTrackingSegments	&& !bIsLastSegmentInProgress)
				{
					m_nTotalSegmentsDrawnByIcoMap--;
				}
				
				// if this is the last row
				if (m_nCurrentSelectedRowIndex == nTotalRows)
				{
					// if this the segment in progress
					if (bIsLastSegmentInProgress)
					{
						discardSegmentInProgress();
						if (m_pInputProcessor)
						{
							// if number of rows is more than 0
							if (GetRowCount() > 0)
							{
								m_pInputProcessor->notifyInputTypeChanged(false);
								m_pInputProcessor->updateState(kHasSegmentState);
							}
						}
					}
					else
					{
						m_ipDisplayAdapter->EraseLastSegment();
					}
				}
				// or this is the second to last row, and last row 
				// is a segment in progress
				else if (m_nCurrentSelectedRowIndex == nTotalRows-1 
					&& !m_bLatestSegmentInfoComplete)
				{
					m_ipDisplayAdapter->EraseLastSegment();
				}
				else
				{
					m_ipDisplayAdapter->UpdateSegments(m_nCurrentPartIndex, 
						nCurrentIndex, m_ipCurrentPartSegments);
				}
				
				// update the grid display
				RemoveRows(m_nCurrentSelectedRowIndex, m_nCurrentSelectedRowIndex);
				long nTotalRowsAfterDeletion = (long) GetRowCount();
				if (nTotalRowsAfterDeletion > 0)
				{
					// which row to select next?
					long nSelectRow = 
						nTotalRowsAfterDeletion < m_nCurrentSelectedRowIndex ? 
						nTotalRowsAfterDeletion : m_nCurrentSelectedRowIndex;
					setRowsSelection(nSelectRow, 1, true, false);
				}
				else
				{
					m_nCurrentSelectedRowIndex = -1;
					m_ipCurrentPartSegments->Clear();
					m_nTotalSegmentsDrawnByIcoMap = 0;
					m_CurrentCell.resetCell();
					
					updateCurrentSegmentVariableSet(-1);
					m_ipLatestSegmentInProgress = NULL;
					if (m_pIcoMapUI != NULL)
					{
						m_pIcoMapUI->enableToggle(false, false);
					}
					
					// no more rows in the grid, update state
					if (m_pInputProcessor)
					{
						m_pInputProcessor->updateState(kHasPointState);
					}
				}
			}
			break;
		case ID_DIG_UP:
			{
				swapRows(m_nCurrentSelectedRowIndex, m_nCurrentSelectedRowIndex - 1, false);
			}
			break;
		case ID_DIG_DOWN:
			{
				swapRows(m_nCurrentSelectedRowIndex, m_nCurrentSelectedRowIndex + 1, false);
			}
			break;
		}

		return TRUE;
	}

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
void DynamicInputGridWnd::refreshCurrentRecordDisplay()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	// Sometimes internally stored segment data record 
	// might be out of sync with the display. For instance,
	// if a curve was originally drawn correctly, then the user modified
	// one of its parameter that makes the curve to be invalid, IcoMap
	// will throw exception and the internal record for this curve segment
	// will not be updated to reflect the invalid value. The display 
	// needs to be refreshed to display the correct value.

	if (!m_bTrackingSegments)
	{
		return;
	}

	// if the segment exists in the drawing
	if (m_nCurrentSegmentIndexInPart >= 0)
	{
		int nNumOfParams = m_vecCurrentSegmentParams.size();
		for (int n=0; n<nNumOfParams; n++)
		{
			IParameterTypeValuePairPtr ipParam = m_vecCurrentSegmentParams[n];
			ECurveParameterType eParamType = ipParam->eParamType;

			if (eParamType == kArcConcaveLeft
				|| eParamType== kArcDeltaGreaterThan180Degrees
				|| (m_eCurrentSegmentType == UCLID_FEATUREMGMTLib::kLine
					&& eParamType == kArcTangentInBearing))
			{
				// no need to display any toggle info
				continue;
			}

			// what's the segment parameter type name
			string strType = getCurveTypeName(eParamType);
			// full description for tooltip
			string strTypeDesc = getCurveTypeName(eParamType, false);

			// parameter value
			string strValue = _bstr_t(ipParam->strValue);
			// convert to current format for bearing and distance
			strValue = translateBDToCurrentFormat(eParamType, strValue);
			
			// unlock the readonly if the cell is readonly
			GetParam()->SetLockReadOnly(FALSE);
			
			unsigned long nTypeCol = n * 2 + 1;
			unsigned long nValueCol = (n + 1) * 2;
			
			// update the display for type and value info
			SetStyleRange(CGXRange(m_nCurrentSelectedRowIndex, nTypeCol),
				CGXStyle()
				.SetValue(strType.c_str())
				.SetUserAttribute(GX_IDS_UA_TOOLTIPTEXT, strTypeDesc.c_str()));
			SetStyleRange(CGXRange(m_nCurrentSelectedRowIndex, nValueCol),
				CGXStyle()
				.SetValue(strValue.c_str()));
	
			GetParam()->SetLockReadOnly(TRUE);
		}
	}
}
//-------------------------------------------------------------------------------------------------
void DynamicInputGridWnd::refreshRowDraw(unsigned long nRow)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	// redraw the row to remove the inverted looking
	CRect rectRow = CalcRectFromRowCol(nRow, 0, nRow, NUM_TYPE_VALUE_PAIR*2); 
	InvalidateRect(&rectRow);
}
//-------------------------------------------------------------------------------------------------
void DynamicInputGridWnd::setCCEParameter(ECurveParameterType eParamType,
										  const string& strValue)
{
	EInputType eInputType = translateParameterTypeToInputType(eParamType);
	// depending upon what the type of the parameter is, parse the
	// value accordingly
	switch (eInputType)
	{
	// distance parameter types
	case kDistance:
		{
			// use the distance class to parse the value
			m_distanceCore.evaluate(strValue);
			if (m_distanceCore.isValid())
			{
				// distance value must be presented in current unit
				double dDist = m_distanceCore.getDistanceInCurrentUnit();
				ma_pCCE->setCurveDistanceParameter(eParamType, dDist);
			}
			else
			{
				UCLIDException ue("ELI12071", "Distance parameter value specified in invalid format!");
				ue.addDebugInfo("Input distance", strValue);
				throw ue;
			}
			break;
		}
		
	// bearing parameter types
	case kBearing:
		{
			// store the original mode, then set it back once 
			// this method is out of scope
			ReverseModeValueRestorer rmvr;
			// always work in normal mode here
			AbstractMeasurement::workInReverseMode(false);
			
			// the value is already in quadrant bearing format
			Bearing bearing(strValue.c_str());
			if (bearing.isValid())
			{
				double dRadians = bearing.getRadians();
				ma_pCCE->setCurveAngleOrBearingParameter(eParamType, dRadians);
			}
			else
			{
				UCLIDException ue("ELI12073", "Direction parameter value specified in invalid format!");
				ue.addDebugInfo("Direction", strValue);
				throw ue;
			}
			break;
		}
		
	// angle parameter types
	case kAngle:
		{
			Angle angle;
			angle.evaluate(strValue.c_str());
			if (angle.isValid())
			{
				double dRadians = angle.getRadians();
				ma_pCCE->setCurveAngleOrBearingParameter(eParamType, dRadians);
			}
			else
			{
				UCLIDException ue("ELI12074", "Angle parameter value specified in invalid format!");
				ue.addDebugInfo("Angle", strValue);
				throw ue;
			}
			break;
		}
			
	// boolean parameter types
	case kToggleCurve:
		{
			if (strValue == "0")
			{
				ma_pCCE->setCurveBooleanParameter(eParamType, false);
			}
			else if (strValue == "1")
			{
				ma_pCCE->setCurveBooleanParameter(eParamType, true);
			}
			else
			{
				UCLIDException ue("ELI19481", "Boolean parameter value specified in invalid format!");
				ue.addDebugInfo("Input parameter", strValue );
				throw ue;
			}
			break;
		}
	}
}
//-------------------------------------------------------------------------------------------------
void DynamicInputGridWnd::setRowsSelection(long nStartingRow, 
										   long nNumberOfRows,
										   bool bUpdateSegmentInfo,
										   bool bFlashSelection)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	long nTotalNumberOfRows = GetRowCount();

	if (nStartingRow > nTotalNumberOfRows)
	{
		return;
	}

	// Clear existing selection
	SetSelection(0);

	if (nNumberOfRows <= 0)
	{
		UCLIDException ue("ELI12246", "Please specify more than one row to select.");
		ue.addDebugInfo("Number Of Rows", nNumberOfRows);
		throw ue;
	}

	long nTop = nStartingRow;
	long nBottom = nStartingRow + nNumberOfRows - 1;
	long nLeft = 0;
	long nRight = NUM_TYPE_VALUE_PAIR * 2;

	// Set selection on this row
	POSITION area = GetParam()->GetRangeList()->AddTail(new CGXRange);
	SetSelection(area, nTop, nLeft, nBottom, nRight);

	if (nStartingRow != m_nCurrentSelectedRowIndex)
	{
		refreshRowDraw(m_nCurrentSelectedRowIndex);
	}

	// update the current record index
	if (bUpdateSegmentInfo 
		&& nNumberOfRows == 1)
	{
		updateRowRecordInfo(nStartingRow);

		// flash the selected segment
		if (bFlashSelection && nStartingRow >= 0 && m_nCurrentSegmentIndexInPart >= 0)
		{
			m_ipDisplayAdapter->FlashSegment(m_nCurrentPartIndex, m_nCurrentSegmentIndexInPart);
		}

		m_nCurrentSelectedRowIndex = nStartingRow;
	}
}
//-------------------------------------------------------------------------------------------------
void DynamicInputGridWnd::setSegmentInfo(IIUnknownVectorPtr ipParams)
{
	m_vecCurrentSegmentParams.clear();
	long nSize = ipParams->Size();
	for (long n=0; n<nSize; n++)
	{
		// create a new param type value pair to store info
		IParameterTypeValuePairPtr ipNewParam(CLSID_ParameterTypeValuePair);
		ASSERT_RESOURCE_ALLOCATION("ELI12083", ipNewParam != NULL);

		IParameterTypeValuePairPtr ipOldParam = ipParams->At(n);
		ipNewParam->eParamType = ipOldParam->eParamType;
		ipNewParam->strValue = ipOldParam->strValue;

		// store in the vec
		m_vecCurrentSegmentParams.push_back(ipNewParam);
	}
}
//-------------------------------------------------------------------------------------------------
void DynamicInputGridWnd::swapRows(unsigned int nRow1, unsigned int nRow2, bool bSelectRow1)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	// make sure these rows are valid
	unsigned int nTotalRows = GetRowCount();
	if (nRow1 > nTotalRows || nRow2 > nTotalRows)
	{
		UCLIDException ue("ELI12478", "Invalid row index");
		ue.addDebugInfo("nRow1", nRow1);
		ue.addDebugInfo("nRow2", nRow2);
		throw ue;
	}

	// determine which row is the later one
	unsigned int nBiggerRow = nRow1 > nRow2 ? nRow1 : nRow2;
	unsigned int nSmallerRow = nRow1 > nRow2 ? nRow2 : nRow1;

	// swap the segments and update the drawing
	m_ipCurrentPartSegments->Swap(nRow1-1, nRow2-1);
	if (m_ipDisplayAdapter)
	{
		// temporary block sketch notification
		ValueRestorer<bool> VR(m_bBlockSketchNotification);
		m_bBlockSketchNotification = true;
		
		m_ipDisplayAdapter->UpdateSegments(m_nCurrentPartIndex, 
			nSmallerRow - 1, m_ipCurrentPartSegments);
	}

	// insert a blank row above the smaller row
	InsertRows(nSmallerRow, 1);

	unsigned int nToBeCopiedRow = nBiggerRow+1;
	for (int nCol=1; nCol<=NUM_TYPE_VALUE_PAIR*2; nCol++)
	{
		if (!GetValueRowCol(nToBeCopiedRow, nCol).IsEmpty())
		{
			// copy the bigger row to the blank row
			CopyCells(CGXRange().SetCells(nToBeCopiedRow, nCol), nSmallerRow, nCol);
		}
	}

	// remove the bigger row
	RemoveRows(nBiggerRow+1, nBiggerRow+1);

	// select the row the selection has gone
	setRowsSelection(bSelectRow1 ? nRow1 : nRow2, 1, true, false);
}
//-------------------------------------------------------------------------------------------------
EInputType DynamicInputGridWnd::translateParameterTypeToInputType(ECurveParameterType eParamType)
{
	EInputType eInputType = kNone;

	switch (eParamType)
	{
	// distance parameter types
	case kLineDistance:
	case kArcRadius:
	case kArcLength:
	case kArcChordLength:
	case kArcExternalDistance:
	case kArcMiddleOrdinate:
	case kArcTangentDistance:
		eInputType = kDistance;
		break;
		
	// bearing parameter types
	case kLineBearing:
	case kArcTangentInBearing:
	case kArcTangentOutBearing:
	case kArcChordBearing:
	case kArcRadialInBearing:
	case kArcRadialOutBearing:
		eInputType = kBearing;
		break;
		
	// angle parameter types
	case kLineDeflectionAngle:
	case kLineInternalAngle:
	case kArcDegreeOfCurveChordDef:
	case kArcDegreeOfCurveArcDef:
	case kArcDelta:
	case kArcStartAngle:
	case kArcEndAngle:
		eInputType = kAngle;
		break;
			
	// boolean parameter types
	case kArcConcaveLeft:
	case kArcDeltaGreaterThan180Degrees:
		eInputType = kToggleCurve;
		break;
	}

	return eInputType;
}
//-------------------------------------------------------------------------------------------------
string DynamicInputGridWnd::translateBDToCurrentFormat(ECurveParameterType eParamType,
													   const string& strInText)
{
	string strOutText(strInText);

	EInputType eInputType = translateParameterTypeToInputType(eParamType);
	switch (eInputType)
	{
	case kBearing:
		{
			// always work in normal mode
			ReverseModeValueRestorer rmvr;
			AbstractMeasurement::workInReverseMode(false);

			Bearing bearing(strInText.c_str());
			if (!bearing.isValid())
			{
				UCLIDException ue("ELI12229", "Invalid direction input");
				ue.addDebugInfo("Input", strInText);
				throw ue;
			}

			// get the output in whatever is the current format, i.e. quadrant
			// bearing, polar angle or azimuth
			strOutText = m_dirHelper.polarAngleInRadiansToDirectionInString(
														bearing.getRadians());
		}
		break;
	case kDistance:
		{
			m_distanceCore.evaluate(strInText);
			if (!m_distanceCore.isValid())
			{
				UCLIDException ue("ELI12620", "Invalid distance input");
				ue.addDebugInfo("Input", strInText);
				throw ue;
			}

			// distance in current unit
			double dDistance = m_distanceCore.getDistanceInCurrentUnit();

			// get distance string in current unit form
			strOutText = getProperDistanceFormat(dDistance);
		}
		break;
	}

	return strOutText;
}
//-------------------------------------------------------------------------------------------------
void DynamicInputGridWnd::updateCurrentSegmentVariableSet(int nNewSegmentIndex, 
														  bool bUpdateIfSelectionSame)
{
	m_eCurrentInputType = kNone;
	m_eCurrentEditingParamType = kInvalidParameterType;
	// reset curve calculation engine
	ma_pCCE->reset();

	if (nNewSegmentIndex < 0)
	{
		m_vecCurrentSegmentParams.clear();
		m_nCurrentSegmentIndexInPart = -1;
		m_eCurrentSegmentType = kInvalidSegmentType;

		return;
	}

	// actual number of segments that are drawn in the map
	long nAcutalNumOfSegments = m_ipCurrentPartSegments->Size();

	// if current selected segment record is incomplete, i.e.
	// this is the lastest segment in progress
	if (m_ipLatestSegmentInProgress != NULL
		&& nAcutalNumOfSegments == nNewSegmentIndex)
	{
		// get all of its params
		IIUnknownVectorPtr ipNewParams = m_ipLatestSegmentInProgress->getParameters();
		// repopulate the content of m_vecCurrentSegmentParams
		setSegmentInfo(ipNewParams);
		
		m_nCurrentSegmentIndexInPart = -1;
		
		// update current segment type
		m_eCurrentSegmentType = m_ipLatestSegmentInProgress->getSegmentType();
	}
	// only update those variables if nNewSegmentIndex 
	// is not same as m_nCurrentSegmentIndexInPart,
	// and nNewSegmentIndex shall not exceed total number
	// of segments for this part
	else if (nNewSegmentIndex >= 0 
			&& (bUpdateIfSelectionSame || m_nCurrentSelectedRowIndex - 1 != nNewSegmentIndex)
			&& nNewSegmentIndex < nAcutalNumOfSegments)
	{
		m_nCurrentSegmentIndexInPart = nNewSegmentIndex;
		// get parameters from currently selected segment
		IESSegmentPtr ipSelectedSegment = m_ipCurrentPartSegments->At(nNewSegmentIndex);
		IIUnknownVectorPtr ipNewParams = ipSelectedSegment->getParameters();
		// repopulate the content of m_vecCurrentSegmentParams
		setSegmentInfo(ipNewParams);

		// update current segment type
		m_eCurrentSegmentType = ipSelectedSegment->getSegmentType();		
	}
	else
	{
		return;
	}

	int nNumOfParams = m_vecCurrentSegmentParams.size();
	for (int n=0; n<nNumOfParams; n++)
	{
		// check to see if this segment requires toggling
		IParameterTypeValuePairPtr ipParam = m_vecCurrentSegmentParams[n];
		ECurveParameterType eParamType = ipParam->eParamType;
		string strValue = _bstr_t(ipParam->strValue);
		if (eParamType == kArcConcaveLeft
			|| eParamType== kArcDeltaGreaterThan180Degrees)
		{
			// set it to toggle curve, even though it might be
			// toggling angle
			m_eCurrentInputType = kToggleCurve;

			if (m_eCurrentSegmentType == UCLID_FEATUREMGMTLib::kLine)
			{
				m_eCurrentInputType = kToggleAngle;
			}
		}

		// update the cce with the set of parameters
		if (m_eCurrentSegmentType == kArc)
		{
			setCCEParameter(eParamType, strValue);
		}
	}
}
//-------------------------------------------------------------------------------------------------
void DynamicInputGridWnd::updateRowRecordInfo(unsigned int nRowIndex)
{
	// update the set of variables for the current segment
	updateCurrentSegmentVariableSet(nRowIndex - 1, true);
	
	// disable the toggle by default in case the segment 
	// doesn't need any toggling
	m_pIcoMapUI->enableToggle(false, false);
	
	// if newly selected segment need toggle info
	if (m_nCurrentSelectedRowIndex >= 0 && 
		(m_eCurrentInputType == kToggleCurve || m_eCurrentInputType == kToggleAngle))
	{
		// get the toggle info
		string strToggleDirection(""), strToggleDeltaAngle("");
		int nSize = m_vecCurrentSegmentParams.size();
		for (int n=nSize-1; n>=0; n--)
		{
			IParameterTypeValuePairPtr ipParam = m_vecCurrentSegmentParams[n];
			if (ipParam->eParamType == kArcConcaveLeft)
			{
				strToggleDirection = ipParam->strValue;
				// if this is a line, no toggle delta angle
				if (m_eCurrentSegmentType == UCLID_FEATUREMGMTLib::kLine)
				{
					break;
				}
				
				// if toggle delta angle is already there
				if (!strToggleDeltaAngle.empty())
				{
					break;
				}
				
				continue;
			}
			
			if (ipParam->eParamType == kArcDeltaGreaterThan180Degrees)
			{
				strToggleDeltaAngle = ipParam->strValue;
				// if toggle direction is already there
				if (!strToggleDirection.empty())
				{
					break;
				}
				continue;
			}
		}
		
		// enable toggling if either is non empty
		// and never enable the confirm button
		m_pIcoMapUI->enableToggle(!strToggleDirection.empty(),
			!strToggleDeltaAngle.empty());
		
		if (!strToggleDirection.empty())
		{
			m_pIcoMapUI->setToggleDirectionState(strToggleDirection == "1");
		}
		
		if (!strToggleDeltaAngle.empty())
		{
			m_pIcoMapUI->setToggleDeltaAngleState(strToggleDeltaAngle == "1");
		}
	}
}
//-------------------------------------------------------------------------------------------------
void DynamicInputGridWnd::validObjects()
{
	if (m_ipAttributeManager == NULL
		|| m_ipDisplayAdapter == NULL
		|| m_pIcoMapUI == NULL)
	{
		throw UCLIDException("ELI12152", "Essential objects can't be NULL.");
	}
}
//-------------------------------------------------------------------------------------------------