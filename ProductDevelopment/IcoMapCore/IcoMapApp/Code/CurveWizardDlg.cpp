//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	CurveWizardDlg.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	John Hurd
//
//==================================================================================================

#include "stdafx.h"
#include "CurveWizardDlg.h"

#include "CurveTool.h"
#include "CurveToolManager.h"
#include "CurveWizardResetDlg.h"
#include "CurveWizardSaveAsDlg.h"

#include <TemporaryResourceOverride.h>
#include <UCLIDException.h>


#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// Global variable
extern HINSTANCE gModuleResource;

static string g_strCurveParam1("");
static string g_strCurveParam2("");
static string g_strCurveParam3("");

//-------------------------------------------------------------------------------------------------
CurveWizardDlg::CurveWizardDlg(CWnd* pParent /*=NULL*/) : 
	CDialog(CurveWizardDlg::IDD, pParent),
	m_pDjinni(new CurveDjinni),
	m_pToolTips(new CToolTipCtrl)
{
	//{{AFX_DATA_INIT(CurveWizardDlg)
		// NOTE: the ClassWizard will add member initialization here
	//}}AFX_DATA_INIT

	// Load the icon from the correct DLL resource
	TemporaryResourceOverride temporaryResourceOverride( gModuleResource );
	m_hIcon = AfxGetApp()->LoadIcon( IDI_ICON_GENIE );
}
//-------------------------------------------------------------------------------------------------
CurveWizardDlg::~CurveWizardDlg()
{
	if (m_pDjinni)
	{
		delete m_pDjinni;
	}
	if (m_pToolTips)
	{
		delete m_pToolTips;
	}

}
//-------------------------------------------------------------------------------------------------
void CurveWizardDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CurveWizardDlg)
	DDX_Control(pDX, IDC_BTN_SaveAs, m_btnSaveAs);
	DDX_Control(pDX, IDC_CBN_Parameter3, m_cbnParameter3);
	DDX_Control(pDX, IDC_CBN_Parameter2, m_cbnParameter2);
	DDX_Control(pDX, IDC_CBN_Parameter1, m_cbnParameter1);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CurveWizardDlg, CDialog)
	//{{AFX_MSG_MAP(CurveWizardDlg)
	ON_BN_CLICKED(IDC_BTN_Reset, OnBTNReset)
	ON_BN_CLICKED(IDC_BTN_SaveAs, OnBTNSaveAs)
	ON_CBN_SELENDOK(IDC_CBN_Parameter1, OnSelendokCBNParameter1)
	ON_CBN_SELENDOK(IDC_CBN_Parameter2, OnSelendokCBNParameter2)
	ON_CBN_SELENDOK(IDC_CBN_Parameter3, OnSelendokCBNParameter3)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
//-------------------------------------------------------------------------------------------------
void CurveWizardDlg::clearUsedParameter(EUsedParameterID eParameterID)
{
	if (eParameterID != kUnused)
	{
		for (PCBCollection::iterator it = m_vecPCB.begin(); it != m_vecPCB.end(); it++)
		{
			if ((*it).eUsed == eParameterID)
			{
				(*it).eUsed = kUnused;
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CurveWizardDlg::createParameterControlBlock(void)
{
	ParameterControlBlock pcb;

	pcb.eCurveParameterID = kArcDelta;
	pcb.strParameterDescription = CurveTool::sDescribeCurveParameter(pcb.eCurveParameterID);
	pcb.eUsed = kUnused;
	m_vecPCB.push_back(pcb);

	pcb.eCurveParameterID = kArcDegreeOfCurveChordDef;
	pcb.strParameterDescription = CurveTool::sDescribeCurveParameter(pcb.eCurveParameterID);
	pcb.eUsed = kUnused;
	m_vecPCB.push_back(pcb);

	pcb.eCurveParameterID = kArcDegreeOfCurveArcDef;
	pcb.strParameterDescription = CurveTool::sDescribeCurveParameter(pcb.eCurveParameterID);
	pcb.eUsed = kUnused;
	m_vecPCB.push_back(pcb);

	pcb.eCurveParameterID = kArcTangentInBearing;
	pcb.strParameterDescription = CurveTool::sDescribeCurveParameter(pcb.eCurveParameterID);
	pcb.eUsed = kUnused;
	m_vecPCB.push_back(pcb);

	pcb.eCurveParameterID = kArcTangentOutBearing;
	pcb.strParameterDescription = CurveTool::sDescribeCurveParameter(pcb.eCurveParameterID);
	pcb.eUsed = kUnused;
	m_vecPCB.push_back(pcb);

	pcb.eCurveParameterID = kArcChordBearing;
	pcb.strParameterDescription = CurveTool::sDescribeCurveParameter(pcb.eCurveParameterID);
	pcb.eUsed = kUnused;
	m_vecPCB.push_back(pcb);

	pcb.eCurveParameterID = kArcRadialInBearing;
	pcb.strParameterDescription = CurveTool::sDescribeCurveParameter(pcb.eCurveParameterID);
	pcb.eUsed = kUnused;
	m_vecPCB.push_back(pcb);

	pcb.eCurveParameterID = kArcRadialOutBearing;
	pcb.strParameterDescription = CurveTool::sDescribeCurveParameter(pcb.eCurveParameterID);
	pcb.eUsed = kUnused;
	m_vecPCB.push_back(pcb);

	pcb.eCurveParameterID = kArcRadius;
	pcb.strParameterDescription = CurveTool::sDescribeCurveParameter(pcb.eCurveParameterID);
	pcb.eUsed = kUnused;
	m_vecPCB.push_back(pcb);

	pcb.eCurveParameterID = kArcLength;
	pcb.strParameterDescription = CurveTool::sDescribeCurveParameter(pcb.eCurveParameterID);
	pcb.eUsed = kUnused;
	m_vecPCB.push_back(pcb);

	pcb.eCurveParameterID = kArcChordLength;
	pcb.strParameterDescription = CurveTool::sDescribeCurveParameter(pcb.eCurveParameterID);
	pcb.eUsed = kUnused;
	m_vecPCB.push_back(pcb);
}
//-------------------------------------------------------------------------------------------------
void CurveWizardDlg::createToolTips()
{
	m_pToolTips->AddTool(GetDlgItem(IDC_CBN_Parameter1), "Specify the first parameter");
	m_pToolTips->AddTool(GetDlgItem(IDC_CBN_Parameter2), "Specify the second parameter");
	m_pToolTips->AddTool(GetDlgItem(IDC_CBN_Parameter3), "Specify the third parameter");
	m_pToolTips->AddTool(GetDlgItem(IDOK), "Start drawing a curve");
	m_pToolTips->AddTool(GetDlgItem(IDC_BTN_SaveAs), "Save the current setting");
	m_pToolTips->AddTool(GetDlgItem(IDC_BTN_Reset), "Reset the settings");
	m_pToolTips->AddTool(GetDlgItem(IDCANCEL), "Close the dialog");
}
//-------------------------------------------------------------------------------------------------
ECurveParameterType CurveWizardDlg::getParameterID(CComboBox& rComboBox) 
{
	return getParameterControlBlock(getIndexPCB(rComboBox)).eCurveParameterID;
}
//-------------------------------------------------------------------------------------------------
CurveWizardDlg::ParameterControlBlock& CurveWizardDlg::getParameterControlBlock(int iIndexPCBCollection)
{
	try
	{
		 return m_vecPCB[iIndexPCBCollection];
	}
	catch(...)
	{
		UCLIDException uclidException("ELI01132","Internal error");
		uclidException.addDebugInfo("iIndexPCBCollection", iIndexPCBCollection);
		throw uclidException;
	}
}
//-------------------------------------------------------------------------------------------------
int CurveWizardDlg::getIndexPCB(const CComboBox& rComboBox)
{
	return rComboBox.GetItemData(rComboBox.GetCurSel());
}
//-------------------------------------------------------------------------------------------------
CurveParameters CurveWizardDlg::getSelectedCurveParameters(void) 
{
	CurveParameters curveParameters;
	curveParameters.push_back(getParameterID(m_cbnParameter1));
	curveParameters.push_back(getParameterID(m_cbnParameter2));
	curveParameters.push_back(getParameterID(m_cbnParameter3));

	return curveParameters;
}
//-------------------------------------------------------------------------------------------------
BOOL CurveWizardDlg::PreTranslateMessage(MSG* pMsg) 
{
	if (m_bInitialized && m_pToolTips)
	{
		m_pToolTips->RelayEvent(pMsg); 
	}
	
	// Check for shortcut keys for drop-downs (P10 #2797)
	if (pMsg->message == WM_KEYDOWN)
	{
		// Check for '1' for first drop-down
		if (pMsg->wParam == '1')
		{
			m_cbnParameter1.SetFocus();
			m_cbnParameter1.ShowDropDown();
		}
		// Check for '2' for second drop-down
		else if (pMsg->wParam == '2')
		{
			m_cbnParameter2.SetFocus();
			m_cbnParameter2.ShowDropDown();
		}
		// Check for '3' for third drop-down
		else if (pMsg->wParam == '3')
		{
			m_cbnParameter3.SetFocus();
			m_cbnParameter3.ShowDropDown();
		}
	}
	
	return CDialog::PreTranslateMessage(pMsg);
}
//-------------------------------------------------------------------------------------------------
void CurveWizardDlg::resetDropDownList(const CurveMatrix& rMatrix, CComboBox& rComboBox)
{
	rComboBox.ResetContent();

	int iIndexListBox = 0;
	int iIndexPCBCollection = 0;
	ParameterControlBlock pcb;
	for (PCBCollection::const_iterator it = m_vecPCB.begin(); it != m_vecPCB.end(); it++)
	{
		pcb = *it;
		if (pcb.eUsed == kUnused &&
			m_pDjinni->doesParameterExistInCurveMatrix(pcb.eCurveParameterID,rMatrix))
		{
			rComboBox.AddString(pcb.strParameterDescription.c_str());
			rComboBox.SetItemData(iIndexListBox,iIndexPCBCollection);
			++iIndexListBox;
		}
		++iIndexPCBCollection;
	}
}
//-------------------------------------------------------------------------------------------------
void CurveWizardDlg::resetDropDownList1(void)
{
	clearUsedParameter(kParameter1);
	clearUsedParameter(kParameter2);	
	clearUsedParameter(kParameter3);
	resetDropDownList(m_pDjinni->getCurveMatrix(),m_cbnParameter1);
}
//-------------------------------------------------------------------------------------------------
void CurveWizardDlg::resetDropDownList2(void)
{
	clearUsedParameter(kParameter2);	
	clearUsedParameter(kParameter3);
	CurveMatrix filteredMatrix = m_pDjinni->filterCurveMatrix(getParameterID(m_cbnParameter1),m_pDjinni->getCurveMatrix());
	resetDropDownList(filteredMatrix,m_cbnParameter2);
}
//-------------------------------------------------------------------------------------------------
void CurveWizardDlg::resetDropDownList3(void)
{
	clearUsedParameter(kParameter3);
	CurveMatrix filteredMatrix = m_pDjinni->filterCurveMatrix(getParameterID(m_cbnParameter1),m_pDjinni->getCurveMatrix());
	filteredMatrix = m_pDjinni->filterCurveMatrix(getParameterID(m_cbnParameter2),filteredMatrix);
	resetDropDownList(filteredMatrix,m_cbnParameter3);
}
//-------------------------------------------------------------------------------------------------
void CurveWizardDlg::updateCurrentCurveTool(void)
{
	CurveToolManager::sGetInstance().updateCurrentCurveTool(getSelectedCurveParameters());
}
//-------------------------------------------------------------------------------------------------
/////////////////////////////////////////////////////////////////////////////
// CurveWizardDlg message handlers


void CurveWizardDlg::OnBTNReset() 
{
	CurveWizardResetDlg	dlgReset;
	dlgReset.DoModal();
}
//-------------------------------------------------------------------------------------------------
void CurveWizardDlg::OnBTNSaveAs() 
{
	CurveWizardSaveAsDlg dlgSaveAs(getSelectedCurveParameters());
	dlgSaveAs.DoModal();
}
//-------------------------------------------------------------------------------------------------
void CurveWizardDlg::OnCancel() 
{
	m_vecPCB.clear();
	CDialog::OnCancel();
}
//-------------------------------------------------------------------------------------------------
BOOL CurveWizardDlg::OnInitDialog() 
{
	CDialog::OnInitDialog();

	EnableToolTips(true);
	m_pToolTips->Create(this, TTS_ALWAYSTIP);
	m_bInitialized = true;
		
	createToolTips();

	// Setup icons
	SetIcon(m_hIcon, TRUE);
	SetIcon(m_hIcon, FALSE);
	
	// disable the Draw button
	GetDlgItem(IDOK)->EnableWindow(FALSE);

	m_btnSaveAs.EnableWindow(FALSE);
	createParameterControlBlock();

	resetDropDownList1();

	// set three combo boxes to the last three wishes
	if (!g_strCurveParam1.empty() && !g_strCurveParam2.empty() && !g_strCurveParam3.empty())
	{
		resetDropDownList1();
		if (m_cbnParameter1.SelectString(-1, g_strCurveParam1.c_str()) != CB_ERR)
		{
			clearUsedParameter(kParameter1);
			getParameterControlBlock(getIndexPCB(m_cbnParameter1)).eUsed = kParameter1;
			
			resetDropDownList2();
			if (m_cbnParameter2.SelectString(-1, g_strCurveParam2.c_str()) != CB_ERR)
			{
				clearUsedParameter(kParameter2);
				getParameterControlBlock(getIndexPCB(m_cbnParameter2)).eUsed = kParameter2;
				
				resetDropDownList3();
				m_cbnParameter3.SelectString(-1, g_strCurveParam3.c_str());
				clearUsedParameter(kParameter3);
				getParameterControlBlock(getIndexPCB(m_cbnParameter3)).eUsed = kParameter3;
				GetDlgItem(IDOK)->EnableWindow(TRUE);
				m_btnSaveAs.EnableWindow(TRUE);
			}
		}
	}

	m_nSelectedItemCbn1 = m_cbnParameter1.GetCurSel();
	m_nSelectedItemCbn2 = m_cbnParameter2.GetCurSel();
	m_nSelectedItemCbn3 = m_cbnParameter3.GetCurSel();
	
	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//-------------------------------------------------------------------------------------------------
void CurveWizardDlg::OnOK() 
{
	updateCurrentCurveTool();

	// remember three parameters enter
	CurveParameters curveParams = getSelectedCurveParameters();
	g_strCurveParam1 = CurveTool::sDescribeCurveParameter(curveParams[0]);
	g_strCurveParam2 = CurveTool::sDescribeCurveParameter(curveParams[1]);
	g_strCurveParam3 = CurveTool::sDescribeCurveParameter(curveParams[2]);

	CDialog::OnOK();
}
//-------------------------------------------------------------------------------------------------
void CurveWizardDlg::OnSelendokCBNParameter1() 
{
	try
	{	
		if (m_nSelectedItemCbn1 != m_cbnParameter1.GetCurSel())
		{
			m_nSelectedItemCbn1 = m_cbnParameter1.GetCurSel();

			clearUsedParameter(kParameter1);
			getParameterControlBlock(getIndexPCB(m_cbnParameter1)).eUsed = kParameter1;

			// Reset the content in the second ComboBox
			resetDropDownList2();
			// Clear the content in the third ComboBox
			m_cbnParameter3.ResetContent();
			
			m_nSelectedItemCbn2 = m_cbnParameter2.GetCurSel();
			m_nSelectedItemCbn3 = m_cbnParameter3.GetCurSel();

			GetDlgItem(IDOK)->EnableWindow(FALSE);
			m_btnSaveAs.EnableWindow(FALSE);
		}
	}
	catch(UCLIDException& uclidException)
	{
		uclidException.display();
	}
}
//-------------------------------------------------------------------------------------------------
void CurveWizardDlg::OnSelendokCBNParameter2() 
{
	try
	{
		if (m_nSelectedItemCbn2 != m_cbnParameter2.GetCurSel())
		{
			m_nSelectedItemCbn2 = m_cbnParameter2.GetCurSel();
			
			clearUsedParameter(kParameter2);
			getParameterControlBlock(getIndexPCB(m_cbnParameter2)).eUsed = kParameter2;
			
			resetDropDownList3();

			m_nSelectedItemCbn3 = m_cbnParameter3.GetCurSel();
			
			GetDlgItem(IDOK)->EnableWindow(FALSE);
			m_btnSaveAs.EnableWindow(FALSE);
		}
	}
	catch(UCLIDException& uclidException)
	{
		uclidException.display();
	}
}
//-------------------------------------------------------------------------------------------------
void CurveWizardDlg::OnSelendokCBNParameter3() 
{
	try
	{
		if (m_nSelectedItemCbn3 != m_cbnParameter3.GetCurSel())
		{
			m_nSelectedItemCbn3 = m_cbnParameter3.GetCurSel();
			
			clearUsedParameter(kParameter3);
			getParameterControlBlock(getIndexPCB(m_cbnParameter3)).eUsed = kParameter3;
			
			GetDlgItem(IDOK)->EnableWindow(TRUE);
			m_btnSaveAs.EnableWindow(TRUE);
		}
	}
	catch(UCLIDException& uclidException)
	{
		uclidException.display();
	}
}
//-------------------------------------------------------------------------------------------------