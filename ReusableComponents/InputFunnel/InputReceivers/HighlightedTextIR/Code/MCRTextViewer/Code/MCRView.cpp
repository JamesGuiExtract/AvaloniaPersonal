//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	MCRView.cpp
//
// PURPOSE:	This is an implementation file for CMCRView() class.
//			Where the CMCRView() class has been derived from CRichEditView() class.
//			The code written in this file makes it possible to implement the various
//			application methods in the user interface.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================

#include "stdafx.h"
#include "resource.h"       // main symbols
#include "Winuser.h"		// for IDC_HAND
#include "MCRView.h"

#include "MCRTextViewerCtl.h"
#include "MCRTextViewer.h"
#include "MCRDocument.h"
#include "MCRFrame.h"

#include <UCLIDException.h>
#include <richedit.h>
#include <vector>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// CMCRView
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(CMCRView, CRichEditView)

//-------------------------------------------------------------------------------------------------
CMCRView::CMCRView()
:m_pMCRTextViewerCtrl(NULL)
{
	bHandCursor = false;
}
//-------------------------------------------------------------------------------------------------
CMCRView::~CMCRView()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20404");
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CMCRView, CRichEditView)
	//{{AFX_MSG_MAP(CMCRView)
	ON_WM_MOUSEMOVE()
	ON_WM_LBUTTONDOWN()
	ON_WM_SETCURSOR()
	ON_WM_RBUTTONDOWN()
	ON_COMMAND(ID_SEND_SELECTED, OnSendSelected)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CMCRView diagnostics
//-------------------------------------------------------------------------------------------------
#ifdef _DEBUG
void CMCRView::AssertValid() const
{
	CView::AssertValid();
}
//-------------------------------------------------------------------------------------------------
void CMCRView::Dump(CDumpContext& dc) const
{
	CView::Dump(dc);
}
#endif //_DEBUG

//-------------------------------------------------------------------------------------------------
// CMCRView message handlers
//-------------------------------------------------------------------------------------------------
BOOL CMCRView::PreCreateWindow(CREATESTRUCT& cs) 
{
	if(!CRichEditView::PreCreateWindow(cs))
	{
		return FALSE;
	}

	cs.lpszClass = RICHEDIT_CLASSA;

	return TRUE;
}
//-------------------------------------------------------------------------------------------------
void CMCRView::OnMouseMove(UINT nFlags, CPoint point) 
{
	// Last entity ID compelling a hand cursor
	static long lLastEntityID = 0;

	// if there are no highlights, just return
	if (m_pMCRTextViewerCtrl->getCurrentTokenPositions().size() == 0)
		return;

	// Check for text in the control
	CString	zMCRText;
	GetRichEditCtrl().GetWindowText( zMCRText );

	// Trim leading and trailing whitespace
	zMCRText.TrimLeft(" \t\n\r");
	zMCRText.TrimRight(" \t\n\r");

	// Stop processing if empty string
	if (!zMCRText.IsEmpty())
	{
		// Get current control text size
		int iTextSize = m_pMCRTextViewerCtrl->getTextSize() * 2;

		int iTextTol = (int)((double)m_pMCRTextViewerCtrl->getTextSize() * (0.25));

		CPoint	chStPt;
		CPoint	chEndPt; 

		long	lStPos;
		long	lEndPos;

		// If in view-mode and Selection is Enabled 
		if ((m_pMCRTextViewerCtrl->getViewMode()) && 
			(m_pMCRTextViewerCtrl->getTextSelectionMode()))
		{
			///////////////////////////////////////////////
			// Find upper and lower bounding entities based 
			// on mouse position and character position
			///////////////////////////////////////////////

			// Initialize upper and lower bounds
			long	lMaxEntity = m_pMCRTextViewerCtrl->getCurrentTokenPositions().size() - 1;
			long	lLowerBound = 0;
			long	lUpperBound = lMaxEntity;
			CPoint	ptStart;

			// Initialize and validate staring point for bounds check
			long	lTestEntity = lLastEntityID;		// Initial is previous ID
			if (lTestEntity > lMaxEntity)
			{
				lTestEntity = lMaxEntity / 2;
			}

			// Compute appropriate step sizes for bounds checks
			long	lStepUp = (lMaxEntity - lLastEntityID) / 2;
			long	lStepDown = lLastEntityID / 2;
			if (lStepUp > lMaxEntity)
			{
				lStepUp = lMaxEntity / 2;
			}
			if (lStepDown > lMaxEntity)
			{
				lStepDown = lMaxEntity / 2;
			}

			// Find upper bound and approximate lower bound
			while (true)
			{
				// Get starting character position of this item
				lStPos = m_pMCRTextViewerCtrl->
					getCurrentTokenPositions().at( lTestEntity ).m_lStartPos;

				// Get the top left corner point of this character
				ptStart = GetRichEditCtrl().GetCharPos( lStPos );
				
				// Compare top left point of this character 
				// with mouse position
				if (ptStart.y > point.y)
				{
					// Character is lower than mouse
					if (lTestEntity < lUpperBound)
					{
						// Store new upper bound
						lUpperBound = lTestEntity;
					}

					// Decrement the Entity ID by the appropriate step
					lTestEntity -= lStepDown;
					lStepUp = lStepDown;
					if (lTestEntity < 0)
					{
						lTestEntity = 0;
					}
				}
				else
				{
					// Character is above or even with mouse
					if (lTestEntity > lLowerBound)
					{
						// Store new lower bound
						lLowerBound = lTestEntity;
					}

					// Increment the Entity ID by the appropriate step
					lTestEntity += lStepUp;
					if (lTestEntity > lMaxEntity)
					{
						lTestEntity = lMaxEntity;
					}
				}

				// Shrink the step size by half
				if (lStepUp > 1)
				{
					lStepUp /= 2;
					lStepDown = lStepUp;
				}
				else
				{
					// Can't shrink step size, time to stop searching
					break;
				}
			}

			// Determine character position of approximate lower bound
			lStPos = m_pMCRTextViewerCtrl->
				getCurrentTokenPositions().at( lLowerBound ).m_lStartPos;

			// Determine line containing approximate lower bound
			long lLowLine = GetRichEditCtrl().LineFromChar( lStPos );

			// Is there an earlier line AND an earlier entity?
			if ((lLowLine > 0) && (lLowerBound > 0))
			{
				// Step backwards through earlier entities to find
				// the last one on the previous line
				long lFinalLine = lLowLine - 1;
				int i;
				for (i = lLowerBound - 1; i >= 0; i--)
				{
					// Get first character of this entity
					lStPos = m_pMCRTextViewerCtrl->
						getCurrentTokenPositions().at( lLowerBound ).m_lStartPos;

					// Get line containing this character
					lLowLine = GetRichEditCtrl().LineFromChar( lStPos );

					// Compare lines
					if (lLowLine == lFinalLine)
					{
						// Stop here
						break;
					}
				}

				// A better lower bound is the i'th entity
				lLowerBound = i;
				if (lLowerBound < 0)
				{
					lLowerBound  = 0;
				}
			}

			//////////////////////////////////////////////////////
			// Loop through bounding entities and determine cursor
			//////////////////////////////////////////////////////
			for (long lMCRCount = lLowerBound; lMCRCount <= lUpperBound; lMCRCount++)
			{
				// Get starting character position of this item
				lStPos = m_pMCRTextViewerCtrl->getCurrentTokenPositions().at( lMCRCount ).m_lStartPos;

				// Get the top left corner point of this character
				chStPt = GetRichEditCtrl().GetCharPos( lStPos );
				
				// Get ending character position of this item
				lEndPos = m_pMCRTextViewerCtrl->getCurrentTokenPositions().at( lMCRCount ).m_lEndPos;
				
				// Incremented one position to get the position of the end of 
				// the character position
				// Or we get the beginning of the next position
				chEndPt = GetRichEditCtrl().GetCharPos( lEndPos + 1 );

				// Check to see if entire MCR Text item is on one line
				if (chStPt.y == chEndPt.y)
				{
					// Check to see if we are on the row
					if ((point.y > (chStPt.y)) && 
						(point.y < (chEndPt.y + iTextSize - iTextTol)))
					{
						// Check to see if we are over this item
						if (point.x > chStPt.x && point.x < chEndPt.x)
						{
							// Set the flag
							bHandCursor = true;

							// Change cursor to hand
							HCURSOR hc = ::LoadCursor( AfxGetApp()->m_hInstance, 
								MAKEINTRESOURCE( IDC_POINT_CURSOR ));
							::SetCursor( hc );

							// Set last entity ID
							lLastEntityID = lMCRCount;

							// Just return so as to not reset the flag
							return;
						}
						// On the row but not on the item
						else
						{
							// Clear the flag
							bHandCursor = false;
						}
					}
					// Not on the row
					else
					{
						// Clear the flag
						bHandCursor = false;
					}
				}
				// MCR Text item is in multiple rows
				else
				{
					// Check to see if we are on one of the rows
					if ((point.y > chStPt.y) && 
						(point.y < chEndPt.y + iTextSize - iTextTol))
					{
						// Check to see if we are over this item
						if (((point.y > chStPt.y) && 
							(point.y < chStPt.y + iTextSize - iTextTol) && 
							(point.x > chStPt.x))	// On the first line
							|| 
							(((point.y > chEndPt.y+2) && 
							(point.y < chEndPt.y + iTextSize - iTextTol)) && 
							point.x < chEndPt.x))	// Is on the last line
						{
							// Set the flag
							bHandCursor = true;

							// Change cursor to hand
							HCURSOR hc = ::LoadCursor( AfxGetApp()->m_hInstance, 
								MAKEINTRESOURCE( IDC_POINT_CURSOR ));
							::SetCursor( hc );

							// Set last entity ID
							lLastEntityID = lMCRCount;

							// Just return so as to not reset the flag
							return;
						}
						else
						{
							// Clear the flag
							bHandCursor = false;
						}

						// Check to see if we are on one of the 
						// more than two rows
						if ((chEndPt.y - chStPt.y) > iTextSize)
						{	
							// If the text is in more than 2 lines highlight the middle rows
							if ((point.y > chStPt.y + iTextSize) && 
								(point.y < chEndPt.y))
							{
								// Set the flag
								bHandCursor = true;

								// Change cursor to hand
								HCURSOR hc = ::LoadCursor( AfxGetApp()->m_hInstance, 
									MAKEINTRESOURCE( IDC_POINT_CURSOR ));
								::SetCursor( hc );

								// Set last entity ID
								lLastEntityID = lMCRCount;

								// Just return so as to not reset the flag
								return;
							}
							else
							{
								// Clear the flag
								bHandCursor = false;
							}
						}
					}
				}
			}
		}
	}

	if (!bHandCursor)
	{
		CRichEditView::OnMouseMove(nFlags, point);
	}
}
//-------------------------------------------------------------------------------------------------
void CMCRView::OnLButtonDown(UINT nFlags, CPoint point) 
{
	// Save current modify status
	BOOL bTempModify = GetRichEditCtrl().GetModify();

	CPoint	chStPt;
	CPoint	chEndPt; 

	long	lStPos;
	long	lEndPos;

	// Get current text size
	int iTextSize = m_pMCRTextViewerCtrl->getTextSize() * 2;
	int iTextTol = (int)((double)m_pMCRTextViewerCtrl->getTextSize() * (0.25));

	BOOL bItIsMCRText = FALSE;

	int iCurLine = 0;
	int	iMcrLine = 0;

	// Disable the selection hiding
	GetRichEditCtrl().HideSelection( FALSE, FALSE ); 

	// Get current (selected) line
	iCurLine = GetRichEditCtrl().LineFromChar( -1 );

	unsigned long lMCRCount;
	bool bFireTextSelected = false;

	// If in view-mode and Selection is Enabled 
	if ((m_pMCRTextViewerCtrl->getViewMode()) && 
		(m_pMCRTextViewerCtrl->getTextSelectionMode()))
	{
		// Loop through MCR'd text objects
		for (lMCRCount = 0; 
			lMCRCount < m_pMCRTextViewerCtrl->getCurrentTokenPositions().size();
			lMCRCount++)
		{
			// Get starting character position of this item
			lStPos = m_pMCRTextViewerCtrl->getCurrentTokenPositions().at( lMCRCount ).m_lStartPos;

			// Get the top left corner point of this character
			chStPt = GetRichEditCtrl().GetCharPos( lStPos );
			
			// Get ending character position of this item
			lEndPos = m_pMCRTextViewerCtrl->getCurrentTokenPositions().at(lMCRCount).m_lEndPos;

			// Incremented one position to get the position of the end of 
			// the character position
			// Or we get the beginning of the next position
			chEndPt = GetRichEditCtrl().GetCharPos( lEndPos + 1 );

			if (chStPt.y == chEndPt.y && 
				((point.y > (chStPt.y)) && 
				(point.y < (chEndPt.y + iTextSize - iTextTol))))
			{
				// Entire MCR Text is on one line
				if (point.x > chStPt.x && point.x < chEndPt.x)
				{
					// Cursor is within the MCR text
					// User clicked on this MCRtext, so fire the event
					bFireTextSelected = true;
					break;
				}
			}
			else
			{
				// MCR Text in multiple rows
				if (point.y > chStPt.y && point.y < chEndPt.y + iTextSize - iTextTol)
				{
					if ((point.y > chStPt.y && point.y < chStPt.y + iTextSize - iTextTol && point.x > chStPt.x)	// On the first line
						|| ((point.y > chEndPt.y && point.y < chEndPt.y + iTextSize - iTextTol) && point.x < chEndPt.x))	// Is on the last line
					{
						// Cursor is within the MCR text
						// User clicked on this MCRtext, so fire the event
						bFireTextSelected = true;
						break;
					}
					if ((chEndPt.y - chStPt.y) > iTextSize)
					{	
						// If the text is in more than 2 lines highlight the middle rows
						if (point.y > chStPt.y + iTextSize && point.y < chEndPt.y)
						{
							// Cursor is within the MCR text
							// User clicked on this MCRtext, so fire the event
							bFireTextSelected = true;
							break;
						}
					}
				}
			}
		}					// end of for loop
	}						// end of if loop

	//	Replace modify state
	GetRichEditCtrl().SetModify( bTempModify );

	// Release the earlier selection for that disable the selection hiding

	// Base class behavior
	CRichEditView::OnLButtonDown(nFlags, point);

	//fire up the text selected event after OnLButtonDown is finished
	if (bFireTextSelected && lMCRCount >= 0)
	{
		m_pMCRTextViewerCtrl->FireTextSelected( lMCRCount );
	}
}
//-------------------------------------------------------------------------------------------------
void CMCRView::OnInitialUpdate() 
{
	CRichEditView::OnInitialUpdate();
	
	// Set to read-only
	GetRichEditCtrl().SetOptions( ECOOP_SET, ECO_READONLY );
}
//-------------------------------------------------------------------------------------------------
BOOL CMCRView::OnSetCursor(CWnd* pWnd, UINT nHitTest, UINT message) 
{
	// Check to see if the hand/point cursor has been loaded
	if (bHandCursor)
	{
		// Just return true
		return TRUE;
	}

	return CRichEditView::OnSetCursor(pWnd, nHitTest, message);
}
//-------------------------------------------------------------------------------------------------
void CMCRView::OnRButtonDown(UINT nFlags, CPoint point) 
{
	// Only display context menu if text selection is enabled
	if (m_pMCRTextViewerCtrl->getTextSelectionMode())
	{
		// Load the context menu
		CMenu menu;
		menu.LoadMenu( IDR_MNU_HTIR_CONTEXT );
		CMenu *pContextMenu = menu.GetSubMenu( 0 );
		
		// Retrieve selected text
		CString	zText;
		zText = GetRichEditCtrl().GetSelText();
		
		UINT nEnable = (MF_BYCOMMAND | MF_ENABLED);
		UINT nDisable = (MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
		
		// If no selection, disable the Send Text item
		if (zText.IsEmpty())
		{
			pContextMenu->EnableMenuItem( ID_SEND_SELECTED, nDisable );
		}
		else
		{
			pContextMenu->EnableMenuItem( ID_SEND_SELECTED, nEnable );
		}
		
		// Map the point to the correct position
		ClientToScreen( &point );
		
		// Display and manage the context menu
		pContextMenu->TrackPopupMenu( TPM_LEFTALIGN | TPM_LEFTBUTTON | TPM_RIGHTBUTTON, 
			point.x, point.y, this );
	}
	
	CRichEditView::OnRButtonDown(nFlags, point);
}
//-------------------------------------------------------------------------------------------------
void CMCRView::OnSendSelected() 
{
	// Retrieve selected text
	CString	zText;
	zText = GetRichEditCtrl().GetSelText();

	// Remove leading and trailing carriage returns
	zText.TrimLeft( "\r\n" );
	zText.TrimRight( "\r\n" );

	// Fire the event
	if (!zText.IsEmpty())
	{
		m_pMCRTextViewerCtrl->FireSelectedText( (LPCTSTR)zText );
	}
}
//-------------------------------------------------------------------------------------------------
