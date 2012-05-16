//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	SelectFeaturesDlg.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS: Duan Wang
//
//==================================================================================================

#include "stdafx.h"
#include "icomapapp.h"
#include "SelectFeaturesDlg.h"

#include <UCLIDException.h>
#include <TemporaryResourceOverride.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

extern HINSTANCE gModuleResource;

/////////////////////////////////////////////////////////////////////////////
// SelectFeaturesDlg dialog


SelectFeaturesDlg::SelectFeaturesDlg(CWnd* pParent /*=NULL*/)
	: CDialog(SelectFeaturesDlg::IDD, pParent), m_cstrSourceDocName("")
{
	//{{AFX_DATA_INIT(SelectFeaturesDlg)
	m_cstrSourceDocName = _T("");
	//}}AFX_DATA_INIT
}


void SelectFeaturesDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(SelectFeaturesDlg)
	DDX_Control(pDX, IDC_EDIT_SOURCE_DOC, m_editSourceDoc);
	DDX_Text(pDX, IDC_EDIT_SOURCE_DOC, m_cstrSourceDocName);
	//}}AFX_DATA_MAP
}


BEGIN_MESSAGE_MAP(SelectFeaturesDlg, CDialog)
	//{{AFX_MSG_MAP(SelectFeaturesDlg)
	ON_BN_CLICKED(IDC_BTN_BROWSE, OnBtnBrowse)
	ON_WM_CANCELMODE()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// SelectFeaturesDlg message handlers

void SelectFeaturesDlg::OnBtnBrowse() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride( gModuleResource );

	try
	{
		CFileDialog fileDialog(TRUE, NULL, NULL, 
			OFN_OVERWRITEPROMPT | OFN_NOCHANGEDIR,
			"All text and image files|*.txt;*.bmp*;*.rle*;*.dib*;*.rst*;*.gp4*;*.mil*;*.cal*;*.cg4*;*.flc*;*.fli*;*.gif*;*.jpg*;*.pcx*;*.pct*;*.png*;*.tga*;*.tif*|Text files (*.txt)|*.txt|All image files|*.bmp*;*.rle*;*.dib*;*.rst*;*.gp4*;*.mil*;*.cal*;*.cg4*;*.flc*;*.fli*;*.gif*;*.jpg*;*.pcx*;*.pct*;*.png*;*.tga*;*.tif*|All files (*.*)|*.*||", 
			NULL);

		if (fileDialog.DoModal() == IDOK)
		{
			m_cstrSourceDocName = fileDialog.GetPathName();
			UpdateData(FALSE);
		}
	}
	CATCH_UCLID_EXCEPTION("ELI01947")
	CATCH_UNEXPECTED_EXCEPTION("ELI01282");
}

void SelectFeaturesDlg::OnOK() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride( gModuleResource );

	try
	{
		UpdateData(TRUE);
		
		CDialog::OnOK();
	}
	CATCH_UCLID_EXCEPTION("ELI01948")
	CATCH_UNEXPECTED_EXCEPTION("ELI01283");
}

void SelectFeaturesDlg::OnCancel() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride( gModuleResource );

	try
	{
		m_cstrSourceDocName = "";
		
		CDialog::OnCancel();
	}
	CATCH_UCLID_EXCEPTION("ELI01949")
	CATCH_UNEXPECTED_EXCEPTION("ELI01284");
}

string SelectFeaturesDlg::GetSourceDocName() const
{
	std::string strTemp( (LPCTSTR) m_cstrSourceDocName );
	return strTemp;
}

BOOL SelectFeaturesDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride( gModuleResource );

	CDialog::OnInitDialog();
	m_editSourceDoc.SetFocus();

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
