//===========================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	TransferDlg.cpp
//
// PURPOSE:	To provide user interface allowing Transfer of Feature Attributes.
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//===========================================================================

#include "stdafx.h"
#include "TransferDlg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CTransferDlg dialog
/////////////////////////////////////////////////////////////////////////////

CTransferDlg::CTransferDlg(bool bOriginalDefined, bool bCanReplaceOriginal, 
						   bool bCanReplaceCurrent, CWnd* pParent /*=NULL*/)
	: CDialog(CTransferDlg::IDD, pParent),
	m_bOriginalDefined(bOriginalDefined),
	m_bCanReplaceOriginal(bCanReplaceOriginal),
	m_bCanReplaceCurrent(bCanReplaceCurrent),
	m_bTransferToOriginal(TRUE)
{
	//{{AFX_DATA_INIT(CTransferDlg)
		// NOTE: the ClassWizard will add member initialization here
	//}}AFX_DATA_INIT
}


void CTransferDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CTransferDlg)
		// NOTE: the ClassWizard will add DDX and DDV calls here
	//}}AFX_DATA_MAP
}


BEGIN_MESSAGE_MAP(CTransferDlg, CDialog)
	//{{AFX_MSG_MAP(CTransferDlg)
	ON_BN_CLICKED(IDC_RADIO_CURRENTASORIG, OnRadioCurrentasorig)
	ON_BN_CLICKED(IDC_RADIO_ORIGASCURRENT, OnRadioOrigascurrent)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CTransferDlg message handlers
/////////////////////////////////////////////////////////////////////////////
BOOL CTransferDlg::OnInitDialog() 
{
	CDialog::OnInitDialog();
	
	// Set appropriate radio button based on input setting
	CButton*	pButton = NULL;
	if (m_bTransferToOriginal) 
	{
		// Default to transfer to Original Data
		pButton = (CButton *)GetDlgItem( IDC_RADIO_CURRENTASORIG );
		if (pButton != NULL)
		{
			pButton->SetCheck( TRUE );
		}
	}
	else
	{
		pButton = (CButton *)GetDlgItem( IDC_RADIO_ORIGASCURRENT );
		if (pButton != NULL)
		{
			pButton->SetCheck( TRUE );
		}
	}

	// Check to see if transfer from Original to Current should be disabled
	if (!m_bOriginalDefined || !m_bCanReplaceCurrent)
	{
		pButton = (CButton *)GetDlgItem( IDC_RADIO_ORIGASCURRENT );
		if (pButton != NULL)
		{
			pButton->EnableWindow( FALSE );
		}
	}

	// Check to see if transfer from Current to Original should be disabled
	if (!m_bCanReplaceOriginal)
	{
		pButton = (CButton *)GetDlgItem( IDC_RADIO_CURRENTASORIG );
		if (pButton != NULL)
		{
			pButton->EnableWindow( FALSE );
		}
	}
	
	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}

/////////////////////////////////////////////////////////////////////////////
void CTransferDlg::OnOK() 
{
	// Provide a confirmation dialog
	CString	zText;
	if (m_bTransferToOriginal)
	{
		if (m_bOriginalDefined)
		{
			zText.Format( "Replace all Original Attributes with the Current Attributes?" );
		}
		else
		{
			zText.Format( "Copy all Current Attributes to Original Attributes?" );
		}
	}
	else
	{
		zText.Format( "Replace all Current Attributes with the Original Attributes?" );
	}

	int iResult = MessageBox( zText.operator LPCTSTR(), "Confirm Transfer", 
		MB_ICONQUESTION | MB_DEFBUTTON2 | MB_YESNO );

	// Check return from Message Box
	if (iResult == IDNO)
	{
		// Just stay active in the dialog
		return;
	}
	
	CDialog::OnOK();
}

/////////////////////////////////////////////////////////////////////////////
void CTransferDlg::OnRadioCurrentasorig() 
{
	// Set member variable
	m_bTransferToOriginal = true;
}

/////////////////////////////////////////////////////////////////////////////
void CTransferDlg::OnRadioOrigascurrent() 
{
	// Set member variable
	m_bTransferToOriginal = false;
}

/////////////////////////////////////////////////////////////////////////////
bool CTransferDlg::isTransferToOriginal()
{
	return m_bTransferToOriginal;
}
