//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	CurveWizardResetDlg.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	John Hurd
//
//==================================================================================================

#include "stdafx.h"
#include "CurveWizardResetDlg.h"

#include "CurveToolManager.h"

extern HINSTANCE gModuleResource;


#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif


CurveWizardResetDlg::CurveWizardResetDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CurveWizardResetDlg::IDD, pParent)
{
	//{{AFX_DATA_INIT(CurveWizardResetDlg)
	//}}AFX_DATA_INIT
}


void CurveWizardResetDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CurveWizardResetDlg)
	DDX_Control(pDX, IDC_CHK_Curve7, m_btnCurve7);
	DDX_Control(pDX, IDC_CHK_Curve8, m_btnCurve8);
	DDX_Control(pDX, IDC_CHK_Curve6, m_btnCurve6);
	DDX_Control(pDX, IDC_CHK_Curve4, m_btnCurve4);
	DDX_Control(pDX, IDC_CHK_Curve5, m_btnCurve5);
	DDX_Control(pDX, IDC_CHK_Curve3, m_btnCurve3);
	DDX_Control(pDX, IDC_CHK_Curve2, m_btnCurve2);
	DDX_Control(pDX, IDC_CHK_Curve1, m_btnCurve1);
	//}}AFX_DATA_MAP
}


BEGIN_MESSAGE_MAP(CurveWizardResetDlg, CDialog)
	//{{AFX_MSG_MAP(CurveWizardResetDlg)
	ON_BN_CLICKED(IDC_BTN_SelectAll, OnBTNSelectAll)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

void CurveWizardResetDlg::configureButtons(void)
{
	m_btnCurve1.SetBitmap(::LoadBitmap(gModuleResource, MAKEINTRESOURCE(IDB_BMP_Curve1))); 
	m_btnCurve2.SetBitmap(::LoadBitmap(gModuleResource, MAKEINTRESOURCE(IDB_BMP_Curve2))); 
	m_btnCurve3.SetBitmap(::LoadBitmap(gModuleResource, MAKEINTRESOURCE(IDB_BMP_Curve3))); 
	m_btnCurve4.SetBitmap(::LoadBitmap(gModuleResource, MAKEINTRESOURCE(IDB_BMP_Curve4))); 
	m_btnCurve5.SetBitmap(::LoadBitmap(gModuleResource, MAKEINTRESOURCE(IDB_BMP_Curve5))); 
	m_btnCurve6.SetBitmap(::LoadBitmap(gModuleResource, MAKEINTRESOURCE(IDB_BMP_Curve6))); 
	m_btnCurve7.SetBitmap(::LoadBitmap(gModuleResource, MAKEINTRESOURCE(IDB_BMP_Curve7))); 
	m_btnCurve8.SetBitmap(::LoadBitmap(gModuleResource, MAKEINTRESOURCE(IDB_BMP_Curve8))); 
}

/////////////////////////////////////////////////////////////////////////////
// CurveWizardResetDlg message handlers

void CurveWizardResetDlg::OnCancel() 
{
	CDialog::OnCancel();
}

void CurveWizardResetDlg::OnOK() 
{
	CurveToolManager& toolManager = CurveToolManager::sGetInstance();
	if (m_btnCurve1.GetCheck())
	{
		toolManager.restoreDefaultState(kCurve1);
		toolManager.addUpdatedCurveToolIDS(kCurve1);
	}
	if (m_btnCurve2.GetCheck())
	{
		toolManager.restoreDefaultState(kCurve2);
		toolManager.addUpdatedCurveToolIDS(kCurve2);
	}
	if (m_btnCurve3.GetCheck())
	{
		toolManager.restoreDefaultState(kCurve3);
		toolManager.addUpdatedCurveToolIDS(kCurve3);
	}
	if (m_btnCurve4.GetCheck())
	{
		toolManager.restoreDefaultState(kCurve4);
		toolManager.addUpdatedCurveToolIDS(kCurve4);
	}
	if (m_btnCurve5.GetCheck())
	{
		toolManager.restoreDefaultState(kCurve5);
		toolManager.addUpdatedCurveToolIDS(kCurve5);
	}
	if (m_btnCurve6.GetCheck())
	{
		toolManager.restoreDefaultState(kCurve6);
		toolManager.addUpdatedCurveToolIDS(kCurve6);
	}
	if (m_btnCurve7.GetCheck())
	{
		toolManager.restoreDefaultState(kCurve7);
		toolManager.addUpdatedCurveToolIDS(kCurve7);
	}
	if (m_btnCurve8.GetCheck())
	{
		toolManager.restoreDefaultState(kCurve8);
		toolManager.addUpdatedCurveToolIDS(kCurve8);
	}

	CDialog::OnOK();
}

BOOL CurveWizardResetDlg::OnInitDialog() 
{
	CDialog::OnInitDialog();
	
	configureButtons();	

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}


void CurveWizardResetDlg::OnBTNSelectAll() 
{
	m_btnCurve1.SetCheck(1);
	m_btnCurve2.SetCheck(1);
	m_btnCurve3.SetCheck(1);
	m_btnCurve4.SetCheck(1);
	m_btnCurve5.SetCheck(1);
	m_btnCurve6.SetCheck(1);
	m_btnCurve7.SetCheck(1);
	m_btnCurve8.SetCheck(1);
}
