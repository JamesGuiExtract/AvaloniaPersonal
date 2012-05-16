//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	HistoryEdit.cpp
//
// PURPOSE:	A CEdit subclass that allows you to display a text history of events.
//
// NOTES:	
//
// AUTHORS: Duan Wang
//
//==================================================================================================

#include "stdafx.h"
#include "resource.h"
#include "HistoryEdit.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// HistoryEdit

HistoryEdit::HistoryEdit()
{
	m_bSelectable = FALSE;
}

HistoryEdit::~HistoryEdit()
{
}

BEGIN_MESSAGE_MAP(HistoryEdit, CEdit)
	//{{AFX_MSG_MAP(HistoryEdit)
	ON_WM_SETFOCUS()
	ON_WM_CONTEXTMENU()
	ON_COMMAND(ID_COPY, OnCopy)
	ON_COMMAND(ID_CLEAR_ALL, OnClearAll)
	ON_COMMAND(ID_SELECT_ALL, OnSelectAll)
	ON_COMMAND(ID_DELETE_SELECTED, OnDeleteSelected)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// HistoryEdit operations

void HistoryEdit::AppendString(CString str)
//
//  Purpose:
//    Appends a text string to the history buffer.
//
//  Returns:
//    None.
//
{
	CString   strBuffer;    // current contents of edit control
	
	// Append string
	GetWindowText (strBuffer);
	if (!strBuffer.IsEmpty())
		strBuffer += "\r\n";
	strBuffer += str;
	SetWindowText (strBuffer);
	
	// Scroll the edit control
	LineScroll (GetLineCount(), 0);
}

/////////////////////////////////////////////////////////////////////////////
// HistoryEdit message handlers

void HistoryEdit::OnSetFocus(CWnd* pOldWnd) 
{
	// Don't allow user to select text
	if (m_bSelectable)
	{
		CEdit::OnSetFocus (pOldWnd);
	}
	else
	{
		pOldWnd->SetFocus();
	}
}

void HistoryEdit::OnContextMenu(CWnd* pWnd, CPoint point) 
{
	SetFocus();

	// Load the context menu
	CMenu menu;
	menu.LoadMenu(IDR_MNU_HISTORY_CONTEXT);
	CMenu *pContextMenu = menu.GetSubMenu(0);

	// Determine selection state
	int nStartPos = -1;
	int nEndPos = -1;
	GetSel(nStartPos, nEndPos);

	UINT nEnable = (MF_BYCOMMAND | MF_ENABLED);
	UINT nDisable = (MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);

	// if there's no selection, disable the copy and deleted selected items
	if (nStartPos == nEndPos)
	{
		pContextMenu->EnableMenuItem(ID_COPY, nDisable);
		pContextMenu->EnableMenuItem(ID_DELETE_SELECTED, nDisable);
	}
	else
	{
		pContextMenu->EnableMenuItem(ID_COPY, nEnable);
		pContextMenu->EnableMenuItem(ID_DELETE_SELECTED, nEnable);
	}

	// Retrieve current history buffer
	CString   strBuffer;
	GetWindowText( strBuffer );

	// If there are no entries, disable the Delete All and Select All items
	if (strBuffer.IsEmpty())
	{
		pContextMenu->EnableMenuItem(ID_CLEAR_ALL, nDisable);
		pContextMenu->EnableMenuItem(ID_SELECT_ALL, nDisable);
	}
	else
	{
		pContextMenu->EnableMenuItem(ID_CLEAR_ALL, nEnable);
		pContextMenu->EnableMenuItem(ID_SELECT_ALL, nEnable);
	}

	// Display and manage the context menu
	pContextMenu->TrackPopupMenu(TPM_LEFTALIGN | TPM_LEFTBUTTON | TPM_RIGHTBUTTON, 
									point.x, point.y, this);
}

void HistoryEdit::OnCopy() 
{
	Copy();	
}

void HistoryEdit::OnClearAll() 
{
	SetSel(0, -1);	
	// Delete contents
	ReplaceSel("");
}

void HistoryEdit::OnSelectAll() 
{
	// select all contents
	SetSel(0, -1);	
}

void HistoryEdit::OnDeleteSelected() 
{
	// Delete selected text
	ReplaceSel("");
}
