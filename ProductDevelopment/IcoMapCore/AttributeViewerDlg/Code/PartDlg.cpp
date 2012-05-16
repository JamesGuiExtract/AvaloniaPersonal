//===========================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	PartDlg.cpp
//
// PURPOSE:	To provide user interface allowing View/Edit of a new or an 
//				existing Starting Point (Part).
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//===========================================================================

#include "stdafx.h"
#include "PartDlg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CPartDlg dialog
/////////////////////////////////////////////////////////////////////////////

CPartDlg::CPartDlg(LPCTSTR pszX, LPCTSTR pszY, bool bReadOnly, bool bNew, 
				   CWnd* pParent /*=NULL*/)
	: CDialog(CPartDlg::IDD, pParent),
	m_bReadOnly(bReadOnly),
	m_bNew(bNew)
{
	//{{AFX_DATA_INIT(CPartDlg)
	m_zXString = pszX;
	m_zYString = pszY;
	//}}AFX_DATA_INIT
}


void CPartDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CPartDlg)
	DDX_Text(pDX, IDC_EDIT_X, m_zXString);
	DDX_Text(pDX, IDC_EDIT_Y, m_zYString);
	//}}AFX_DATA_MAP
}


BEGIN_MESSAGE_MAP(CPartDlg, CDialog)
	//{{AFX_MSG_MAP(CPartDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CPartDlg message handlers
/////////////////////////////////////////////////////////////////////////////
BOOL CPartDlg::OnInitDialog() 
{
	CDialog::OnInitDialog();
	
	// Check to see if edit boxes should be read-only
	if (m_bReadOnly)
	{
		// Set dialog caption - ignores m_bNew member
		SetWindowText( "View Start Point" );

		// Set X to read-only
		CEdit*	pEdit = NULL;
		pEdit = (CEdit *)GetDlgItem( IDC_EDIT_X );
		if (pEdit != NULL)
		{
			pEdit->SetReadOnly( TRUE );
		}

		// Set Y to read-only
		pEdit = (CEdit *)GetDlgItem( IDC_EDIT_Y );
		if (pEdit != NULL)
		{
			pEdit->SetReadOnly( TRUE );
		}

		// Also set focus to the OK button
		CButton*	pButton = NULL;
		pButton = (CButton *)GetDlgItem( IDOK );
		if (pButton != NULL)
		{
			pButton->SetFocus();

			return FALSE;
		}
	}
	else
	{
		// Check New flag
		if (m_bNew)
		{
			// Set dialog caption
			SetWindowText( "New Start Point" );
		}
		else
		{
			// Set dialog caption
			SetWindowText( "Edit Start Point" );
		}
	}
	
	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}

/////////////////////////////////////////////////////////////////////////////
void CPartDlg::OnOK() 
{
	// Just return if dialog was read-only
	if (!m_bReadOnly)
	{
		// Update data members
		UpdateData( TRUE );

		//////////////////////////////////
		// TODO: Do appropriate validation
		//////////////////////////////////

		// Check to see that edit boxes are not empty
		if (m_zXString.IsEmpty())
		{
			// Display error message
			MessageBox( "Start point X must be defined.", "Error" );

			// Also set focus to the edit box
			CEdit*	pEdit = NULL;
			pEdit = (CEdit *)GetDlgItem( IDC_EDIT_X );
			if (pEdit != NULL)
			{
				pEdit->SetFocus();
			}

			// Back to the dialog
			return;
		}

		if (m_zYString.IsEmpty())
		{
			// Display error message
			MessageBox( "Start point Y must be defined.", "Error" );

			// Also set focus to the edit box
			CEdit*	pEdit = NULL;
			pEdit = (CEdit *)GetDlgItem( IDC_EDIT_Y );
			if (pEdit != NULL)
			{
				pEdit->SetFocus();
			}

			// Back to the dialog
			return;
		}
	}
	
	CDialog::OnOK();
}

/////////////////////////////////////////////////////////////////////////////
LPCTSTR CPartDlg::getX()
{
	return m_zXString.operator LPCTSTR();
}

/////////////////////////////////////////////////////////////////////////////
LPCTSTR CPartDlg::getY()
{
	return m_zYString.operator LPCTSTR();
}
