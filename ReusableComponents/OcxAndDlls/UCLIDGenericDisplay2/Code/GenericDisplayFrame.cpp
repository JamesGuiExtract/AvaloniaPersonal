//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	GenericDisplayFrame.cpp
//
// PURPOSE:	This is an implementation file for CGenericDisplayFrame() and CGenericDisplayToolBar() classes.
//			Where the CGenericDisplayFrame() and  CGenericDisplayFrame classes have been derived
//			from CFrameWnd()  class.
//			The code written in this file makes it possible to implement the various
//			methods in the Frame .
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================
// GenericDisplayFrame.cpp : implementation file
//

#include "stdafx.h"
#include "GenericDisplay.h"
#include "GenericDisplayFrame.h"
#include "UCLIDException.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//==========================================================================================
static UINT BASED_CODE indicators[] =
{
	ID_SEPARATOR,           // status line indicator
	ID_SEPARATOR,
	ID_SEPARATOR,
	ID_SEPARATOR,
};


/////////////////////////////////////////////////////////////////////////////
// CGenericDisplayFrame

IMPLEMENT_DYNCREATE(CGenericDisplayFrame, CFrameWnd)
//==========================================================================================
CGenericDisplayFrame::CGenericDisplayFrame()
{
}
//==========================================================================================
CGenericDisplayFrame::~CGenericDisplayFrame()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16451");
}
//==========================================================================================

BEGIN_MESSAGE_MAP(CGenericDisplayFrame, CFrameWnd)
	//{{AFX_MSG_MAP(CGenericDisplayFrame)
	ON_WM_CREATE()
	ON_WM_SIZE()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
//==========================================================================================
/////////////////////////////////////////////////////////////////////////////
// CGenericDisplayFrame message handlers

int CGenericDisplayFrame::OnCreate(LPCREATESTRUCT lpCreateStruct) 
{
	if (CFrameWnd::OnCreate(lpCreateStruct) == -1)
		return -1;

	//	create status bar
	if (!CreateStatusBar())
		return -1;
		
	return 0;
}
//==========================================================================================
BOOL CGenericDisplayFrame::CreateStatusBar()
{
	//	create status bar
	if (!m_wndStatusBar.Create(this) ||
		!m_wndStatusBar.SetIndicators(indicators,
		  sizeof(indicators)/sizeof(UINT)))
	{
		TRACE0("Failed to create status bar\n");
		return FALSE;      // fail to create
	}

	//	set pane style
	m_wndStatusBar.SetPaneInfo(1, ID_SEPARATOR, SBPS_NORMAL, 80);
	m_wndStatusBar.SetPaneInfo(2, ID_SEPARATOR, SBPS_NORMAL, 80);
	m_wndStatusBar.SetPaneInfo(3, ID_SEPARATOR, SBPS_NORMAL, 80);

	return TRUE;
}
//==========================================================================================
void CGenericDisplayFrame::SetOVRStatus(UINT nKey)
{
	//	check for the key
	if(nKey == VK_INSERT)
		//	check for the current status
		if(m_wndStatusBar.GetPaneStyle(3) == SBPS_NORMAL)
			//	set the pane to normal state
			m_wndStatusBar.SetPaneStyle(3, SBPS_DISABLED);
		else
			//	set the pane to disable state
			m_wndStatusBar.SetPaneStyle(3, SBPS_NORMAL);
}
//==========================================================================================
void CGenericDisplayFrame::statusText (int iPane, CString zStText)
{
	CRect rect;
	//	set the status bar text
	switch (iPane)
	{
	case 0:
		m_wndStatusBar.SetWindowText( LPCTSTR(zStText) );
		break;
	case 1:
	case 2:
	case 3:
	case 4:
		m_wndStatusBar.SetPaneText(iPane, LPCTSTR(zStText));
		m_wndStatusBar.GetItemRect(iPane, rect); 
		m_wndStatusBar.InvalidateRect(rect);
		break;
	}
}
//==========================================================================================
void CGenericDisplayFrame::OnSize(UINT nType, int cx, int cy) 
{
	CFrameWnd::OnSize(nType, cx, cy);
	
	// Compute appropriate pane sizes
	int iSmall = 0;
	int iLarge = 0;
	if( cx > 320 )
	{
		// Allow four panes to be visible
		iSmall = min( 80, cx / 4 );
		iLarge = cx - 4 * iSmall;
	}
	else
	{
		// Allow just the status text pane to be visible
		iLarge = cx;
	}

	// Adjust pane sizes
	//m_wndStatusBar.SetPaneInfo( 1, ID_SEPARATOR, SBPS_NORMAL, iSmall );
	m_wndStatusBar.SetPaneInfo( 1, ID_SEPARATOR, SBPS_NORMAL, iSmall + 50);
	m_wndStatusBar.SetPaneInfo( 2, ID_SEPARATOR, SBPS_NORMAL, iSmall );
	m_wndStatusBar.SetPaneInfo( 3, ID_SEPARATOR, SBPS_NORMAL, iSmall );
	m_wndStatusBar.SetPaneInfo( 0, ID_SEPARATOR, SBPS_NOBORDERS | SBPS_STRETCH, 
		iLarge );
}
//==========================================================================================
