//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	CurveWizardSaveAsDlg.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	John Hurd
//
//==================================================================================================

#include "stdafx.h"
#include "CurveWizardSaveAsDlg.h"

#include "CurveTool.h"
#include "CurveToolManager.h"

#include <ECurveToolID.h>

extern HINSTANCE gModuleResource;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

CurveWizardSaveAsDlg::CurveWizardSaveAsDlg(CurveParameters curveParameters,CWnd* pParent /*=NULL*/)
	: CDialog(CurveWizardSaveAsDlg::IDD, pParent),
	m_curveParameters(curveParameters)
{
	//{{AFX_DATA_INIT(CurveWizardSaveAsDlg)
		// NOTE: the ClassWizard will add member initialization here
	//}}AFX_DATA_INIT
}


void CurveWizardSaveAsDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CurveWizardSaveAsDlg)
	DDX_Control(pDX, IDC_RADIO_Curve8, m_btnCurve8);
	DDX_Control(pDX, IDC_RADIO_Curve7, m_btnCurve7);
	DDX_Control(pDX, IDC_RADIO_Curve6, m_btnCurve6);
	DDX_Control(pDX, IDC_RADIO_Curve5, m_btnCurve5);
	DDX_Control(pDX, IDC_RADIO_Curve4, m_btnCurve4);
	DDX_Control(pDX, IDC_RADIO_Curve3, m_btnCurve3);
	DDX_Control(pDX, IDC_RADIO_Curve2, m_btnCurve2);
	DDX_Control(pDX, IDC_RADIO_Curve1, m_btnCurve1);
	//}}AFX_DATA_MAP
}


BEGIN_MESSAGE_MAP(CurveWizardSaveAsDlg, CDialog)
	//{{AFX_MSG_MAP(CurveWizardSaveAsDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

void CurveWizardSaveAsDlg::configureButtons(void)
{
	m_btnCurve1.SetBitmap(::LoadBitmap(gModuleResource, MAKEINTRESOURCE(IDB_BMP_Curve1))); 
	m_btnCurve2.SetBitmap(::LoadBitmap(gModuleResource, MAKEINTRESOURCE(IDB_BMP_Curve2))); 
	m_btnCurve3.SetBitmap(::LoadBitmap(gModuleResource, MAKEINTRESOURCE(IDB_BMP_Curve3))); 
	m_btnCurve4.SetBitmap(::LoadBitmap(gModuleResource, MAKEINTRESOURCE(IDB_BMP_Curve4))); 
	m_btnCurve5.SetBitmap(::LoadBitmap(gModuleResource, MAKEINTRESOURCE(IDB_BMP_Curve5))); 
	m_btnCurve6.SetBitmap(::LoadBitmap(gModuleResource, MAKEINTRESOURCE(IDB_BMP_Curve6))); 
	m_btnCurve7.SetBitmap(::LoadBitmap(gModuleResource, MAKEINTRESOURCE(IDB_BMP_Curve7))); 
	m_btnCurve8.SetBitmap(::LoadBitmap(gModuleResource, MAKEINTRESOURCE(IDB_BMP_Curve8))); 


	CheckRadioButton(IDC_RADIO_Curve1, IDC_RADIO_Curve8, IDC_RADIO_Curve1); 
}

ECurveToolID CurveWizardSaveAsDlg::getSelectedCurveToolID(void)
{
	ECurveToolID selectedToolID = kCurve1;

	switch (GetCheckedRadioButton(IDC_RADIO_Curve1,IDC_RADIO_Curve8))
	{
	case IDC_RADIO_Curve1:
		selectedToolID = kCurve1;
		break;
	case IDC_RADIO_Curve2:
		selectedToolID = kCurve2;
		break;
	case IDC_RADIO_Curve3:
		selectedToolID = kCurve3;
		break;
	case IDC_RADIO_Curve4:
		selectedToolID = kCurve4;
		break;
	case IDC_RADIO_Curve5:
		selectedToolID = kCurve5;
		break;
	case IDC_RADIO_Curve6:
		selectedToolID = kCurve6;
		break;
	case IDC_RADIO_Curve7:
		selectedToolID = kCurve7;
		break;
	case IDC_RADIO_Curve8:
		selectedToolID = kCurve8;
		break;
	default:
		ASSERT(false);		// Someone forgot to maintain this code.
	}

	return selectedToolID;
}

void CurveWizardSaveAsDlg::saveSelectedCurveTool(void)
{
	CurveTool *pTool = CurveToolManager::sGetInstance().getCurveTool(getSelectedCurveToolID());
	pTool->saveState();

	// add the updated curve tool id to curve tool manager
	CurveToolManager::sGetInstance().addUpdatedCurveToolIDS(pTool->getCurveToolID());
}

void CurveWizardSaveAsDlg::updateSelectedCurveTool(void)
{
	CurveTool* pTool = CurveToolManager::sGetInstance().getCurveTool(getSelectedCurveToolID());
	pTool->reset();

	int iCurveParameterID = 1;
	for (CurveParameters::const_iterator it = m_curveParameters.begin(); it != m_curveParameters.end(); it++)
	{
		pTool->setCurveParameter(iCurveParameterID,*it);
		++iCurveParameterID;
	}
	pTool->updateStateOfCurveToggles();
}

/////////////////////////////////////////////////////////////////////////////
// CurveWizardSaveAsDlg message handlers

void CurveWizardSaveAsDlg::OnCancel() 
{
	CDialog::OnCancel();
}

BOOL CurveWizardSaveAsDlg::OnInitDialog() 
{
	CDialog::OnInitDialog();
	
	configureButtons();	

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}

void CurveWizardSaveAsDlg::OnOK() 
{
	updateSelectedCurveTool();
	saveSelectedCurveTool();

	CDialog::OnOK();
}

