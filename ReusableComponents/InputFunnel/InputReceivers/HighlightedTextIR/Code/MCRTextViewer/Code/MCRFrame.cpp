//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	MCRFrame.cpp
//
// PURPOSE:	This is an implementation file for CMCRFrame() class.
//			CMCRFrame() class has been derived from CFrameWnd() class.
//			The code written in this file makes it possible to implement the 
//			various methods in the Frame.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================
// MCRFrame.cpp : implementation file
//

#include "stdafx.h"
#include "resource.h"
#include "MCRFrame.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif


/////////////////////////////////////////////////////////////////////////////
// CMCRFrame

IMPLEMENT_DYNCREATE(CMCRFrame, CFrameWnd)

CMCRFrame::CMCRFrame()
{
}


BEGIN_MESSAGE_MAP(CMCRFrame, CFrameWnd)
	//{{AFX_MSG_MAP(CMCRFrame)
	ON_WM_CREATE()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CMCRFrame message handlers

int CMCRFrame::OnCreate(LPCREATESTRUCT lpCreateStruct) 
{
	if (CFrameWnd::OnCreate(lpCreateStruct) == -1)
		return -1;

	return 0;
}
