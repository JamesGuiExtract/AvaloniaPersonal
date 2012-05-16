//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ShortcutSettingsDlg.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS: Wayne Lenius
//
//==================================================================================================

#include "stdafx.h"
#include "resource.h"
#include "ShortcutSettings.h"

#include <IcoMapOptions.h>
#include <TemporaryResourceOverride.h>
#include <UCLIDException.h>

#include <algorithm>
#include <vector>
using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

extern HINSTANCE gModuleResource;

/////////////////////////////////////////////////////////////////////////////
// CShortcutSettings dialog

IMPLEMENT_DYNCREATE(CShortcutSettings, CPropertyPage)

CShortcutSettings::CShortcutSettings()
: CPropertyPage(CShortcutSettings::IDD), 
  m_bInitialized(false), 
  m_bApplied(false),
  m_pToolTips(new CToolTipCtrl)
{
	EnableAutomation();
	//{{AFX_DATA_INIT(CShortcutSettings)
	m_zCurve1 = _T("");
	m_zCurve2 = _T("");
	m_zCurve3 = _T("");
	m_zCurve4 = _T("");
	m_zCurve5 = _T("");
	m_zCurve6 = _T("");
	m_zCurve7 = _T("");
	m_zCurve8 = _T("");
	m_zCustom = _T("");
	m_zDeleteSketch = _T("");
	m_zFinishPart = _T("");
	m_zFinishSketch = _T("");
	m_zForward = _T("");
	m_zGreater = _T("");
	m_zLeft = _T("");
	m_zLess = _T("");
	m_zLine = _T("");
	m_zReverse = _T("");
	m_zRight = _T("");
	m_zLineAngle = _T("");
	//}}AFX_DATA_INIT
}

CShortcutSettings::~CShortcutSettings()
{
	// Clean up tooltips object
	if (m_pToolTips)
	{
		delete m_pToolTips;
		m_pToolTips = NULL;
	}
}

void CShortcutSettings::OnFinalRelease()
{
	// When the last reference for an automation object is released
	// OnFinalRelease is called.  The base class will automatically
	// deletes the object.  Add additional cleanup required for your
	// object before calling the base class.

	CPropertyPage::OnFinalRelease();
}

void CShortcutSettings::DoDataExchange(CDataExchange* pDX)
{
	CPropertyPage::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CShortcutSettings)
	DDX_Control(pDX, IDC_EDIT_LINE_ANGLE, m_ctrlLineAngle);
	DDX_Control(pDX, IDC_EDIT_RIGHT, m_ctrlRight);
	DDX_Control(pDX, IDC_EDIT_REVERSE, m_ctrlReverse);
	DDX_Control(pDX, IDC_EDIT_LINE, m_ctrlLine);
	DDX_Control(pDX, IDC_EDIT_LESS, m_ctrlLess);
	DDX_Control(pDX, IDC_EDIT_LEFT, m_ctrlLeft);
	DDX_Control(pDX, IDC_EDIT_GREATER, m_ctrlGreater);
	DDX_Control(pDX, IDC_EDIT_FORWARD, m_ctrlForward);
	DDX_Control(pDX, IDC_EDIT_FINISHSKETCH, m_ctrlFinishSketch);
	DDX_Control(pDX, IDC_EDIT_FINISHPART, m_ctrlFinishPart);
	DDX_Control(pDX, IDC_EDIT_DELETESKETCH, m_ctrlDeleteSketch);
	DDX_Control(pDX, IDC_EDIT_CUSTOM, m_ctrlCustom);
	DDX_Control(pDX, IDC_EDIT_CURVE8, m_ctrlCurve8);
	DDX_Control(pDX, IDC_EDIT_CURVE7, m_ctrlCurve7);
	DDX_Control(pDX, IDC_EDIT_CURVE6, m_ctrlCurve6);
	DDX_Control(pDX, IDC_EDIT_CURVE5, m_ctrlCurve5);
	DDX_Control(pDX, IDC_EDIT_CURVE4, m_ctrlCurve4);
	DDX_Control(pDX, IDC_EDIT_CURVE3, m_ctrlCurve3);
	DDX_Control(pDX, IDC_EDIT_CURVE2, m_ctrlCurve2);
	DDX_Control(pDX, IDC_EDIT_CURVE1, m_ctrlCurve1);
	DDX_Text(pDX, IDC_EDIT_CURVE1, m_zCurve1);
	DDX_Text(pDX, IDC_EDIT_CURVE2, m_zCurve2);
	DDX_Text(pDX, IDC_EDIT_CURVE3, m_zCurve3);
	DDX_Text(pDX, IDC_EDIT_CURVE4, m_zCurve4);
	DDX_Text(pDX, IDC_EDIT_CURVE5, m_zCurve5);
	DDX_Text(pDX, IDC_EDIT_CURVE6, m_zCurve6);
	DDX_Text(pDX, IDC_EDIT_CURVE7, m_zCurve7);
	DDX_Text(pDX, IDC_EDIT_CURVE8, m_zCurve8);
	DDX_Text(pDX, IDC_EDIT_CUSTOM, m_zCustom);
	DDX_Text(pDX, IDC_EDIT_DELETESKETCH, m_zDeleteSketch);
	DDX_Text(pDX, IDC_EDIT_FINISHPART, m_zFinishPart);
	DDX_Text(pDX, IDC_EDIT_FINISHSKETCH, m_zFinishSketch);
	DDX_Text(pDX, IDC_EDIT_FORWARD, m_zForward);
	DDX_Text(pDX, IDC_EDIT_GREATER, m_zGreater);
	DDX_Text(pDX, IDC_EDIT_LEFT, m_zLeft);
	DDX_Text(pDX, IDC_EDIT_LESS, m_zLess);
	DDX_Text(pDX, IDC_EDIT_LINE, m_zLine);
	DDX_Text(pDX, IDC_EDIT_REVERSE, m_zReverse);
	DDX_Text(pDX, IDC_EDIT_RIGHT, m_zRight);
	DDX_Text(pDX, IDC_EDIT_LINE_ANGLE, m_zLineAngle);
	//}}AFX_DATA_MAP
}


BEGIN_MESSAGE_MAP(CShortcutSettings, CPropertyPage)
	//{{AFX_MSG_MAP(CShortcutSettings)
	ON_BN_CLICKED(IDC_BUTTON_RESTORE, OnButtonRestore)
	//}}AFX_MSG_MAP
	ON_CONTROL_RANGE(EN_CHANGE, IDC_EDIT_CURVE1, IDC_EDIT_DELETESKETCH, OnChangeCommand)
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CShortcutSettings message handlers

BOOL CShortcutSettings::PreTranslateMessage(MSG* pMsg) 
{
	// Needed for tooltip appearance
	if (m_bInitialized && m_pToolTips)
	{
		m_pToolTips->RelayEvent(pMsg); 
	}
	
	return CPropertyPage::PreTranslateMessage(pMsg);
}

BOOL CShortcutSettings::OnSetActive() 
{
	try
	{
		// if Apply button is clicked, disable Cancel button
		//if (m_bApplied)
		//{
		//	CancelToClose();
		//}

		IcoMapOptions::sGetInstance().setActiveOptionPageNum(m_iShortcutSettingsPageIndex);
	}
	catch (UCLIDException &uclidException)
	{
		uclidException.display();
		return FALSE;
	}
	catch (...)
	{
		UCLIDException uclidException("ELI01821", "Unknown exception was caught");
		uclidException.display();
		return FALSE;
	}

	return CPropertyPage::OnSetActive();
}

void CShortcutSettings::createToolTips()
{
	m_pToolTips->AddTool(GetDlgItem(IDC_EDIT_CURVE1), "Command to initiate curve #1");
	m_pToolTips->AddTool(GetDlgItem(IDC_EDIT_CURVE2), "Command to initiate curve #2");
	m_pToolTips->AddTool(GetDlgItem(IDC_EDIT_CURVE3), "Command to initiate curve #3");
	m_pToolTips->AddTool(GetDlgItem(IDC_EDIT_CURVE4), "Command to initiate curve #4");
	m_pToolTips->AddTool(GetDlgItem(IDC_EDIT_CURVE5), "Command to initiate curve #5");
	m_pToolTips->AddTool(GetDlgItem(IDC_EDIT_CURVE6), "Command to initiate curve #6");
	m_pToolTips->AddTool(GetDlgItem(IDC_EDIT_CURVE7), "Command to initiate curve #7");
	m_pToolTips->AddTool(GetDlgItem(IDC_EDIT_CURVE8), "Command to initiate curve #8");
	m_pToolTips->AddTool(GetDlgItem(IDC_EDIT_LINE), "Command to initiate a direction line");
	m_pToolTips->AddTool(GetDlgItem(IDC_EDIT_LINE_ANGLE), "Command to initiate an internal or deflection angle line");
	m_pToolTips->AddTool(GetDlgItem(IDC_EDIT_CUSTOM), "Command to initiate a custom curve");
	m_pToolTips->AddTool(GetDlgItem(IDC_EDIT_RIGHT), "Command to indicate curve is concave right");
	m_pToolTips->AddTool(GetDlgItem(IDC_EDIT_LEFT), "Command to indicate curve is concave left");
	m_pToolTips->AddTool(GetDlgItem(IDC_EDIT_GREATER), "Command to indicate curve delta angle is greater than 180");
	m_pToolTips->AddTool(GetDlgItem(IDC_EDIT_LESS), "Command to indicate curve delta angle is less than 180");
	m_pToolTips->AddTool(GetDlgItem(IDC_EDIT_FORWARD), "Command to indicate forward direction");
	m_pToolTips->AddTool(GetDlgItem(IDC_EDIT_REVERSE), "Command to indicate reverse direction");
	m_pToolTips->AddTool(GetDlgItem(IDC_EDIT_FINISHSKETCH), "Command to finish the sketch");
	m_pToolTips->AddTool(GetDlgItem(IDC_EDIT_FINISHPART), "Command to finish the part");
	m_pToolTips->AddTool(GetDlgItem(IDC_EDIT_DELETESKETCH), "Command to delete the sketch");
	m_pToolTips->AddTool(GetDlgItem(IDC_BUTTON_RESTORE), "Restore default shortcuts");
}

BOOL CShortcutSettings::OnInitDialog() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);
	
	try
	{
		CPropertyPage::OnInitDialog();
		
		// Setup for and create tooltips
		EnableToolTips( true );
		m_pToolTips->Create( this, TTS_ALWAYSTIP );
		createToolTips();

		// Retrieve stored shortcuts
		if (!m_bInitialized)
		{
			IcoMapOptions& icoMapOpt = IcoMapOptions::sGetInstance();
			m_zCurve1 = (icoMapOpt.getShortcut( kShortcutCurve1 )).c_str();
			m_zCurve2 = (icoMapOpt.getShortcut( kShortcutCurve2 )).c_str();
			m_zCurve3 = (icoMapOpt.getShortcut( kShortcutCurve3 )).c_str();
			m_zCurve4 = (icoMapOpt.getShortcut( kShortcutCurve4 )).c_str();
			m_zCurve5 = (icoMapOpt.getShortcut( kShortcutCurve5 )).c_str();
			m_zCurve6 = (icoMapOpt.getShortcut( kShortcutCurve6 )).c_str();
			m_zCurve7 = (icoMapOpt.getShortcut( kShortcutCurve7 )).c_str();
			m_zCurve8 = (icoMapOpt.getShortcut( kShortcutCurve8 )).c_str();

			m_zLine = (icoMapOpt.getShortcut( kShortcutLine )).c_str();
			m_zLineAngle = (icoMapOpt.getShortcut( kShortcutLineAngle )).c_str();
			m_zCustom = (icoMapOpt.getShortcut( kShortcutGenie )).c_str();
			m_zRight = (icoMapOpt.getShortcut( kShortcutRight )).c_str();
			m_zLeft = (icoMapOpt.getShortcut( kShortcutLeft )).c_str();
			m_zGreater = (icoMapOpt.getShortcut( kShortcutGreater )).c_str();
			m_zLess = (icoMapOpt.getShortcut( kShortcutLess )).c_str();
			m_zForward = (icoMapOpt.getShortcut( kShortcutForward )).c_str();
			m_zReverse = (icoMapOpt.getShortcut( kShortcutReverse )).c_str();

			m_zFinishSketch = (icoMapOpt.getShortcut( kShortcutFinishSketch )).c_str();
			m_zFinishPart = (icoMapOpt.getShortcut( kShortcutFinishPart )).c_str();
			m_zDeleteSketch = (icoMapOpt.getShortcut( kShortcutDeleteSketch )).c_str();

			UpdateData( FALSE );

			m_bInitialized = true;
		}
	}
	catch (UCLIDException &uclidException)
	{
		uclidException.display();
		return FALSE;
	}
	catch (...)
	{
		UCLIDException uclidException("ELI01822", "Unknown exception was caught");
		uclidException.display();
		return FALSE;
	}

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}

void CShortcutSettings::saveSettings()
{
	if (m_bInitialized)
	{
		IcoMapOptions& icoMapOpt = IcoMapOptions::sGetInstance();
		icoMapOpt.setShortcutCode( kShortcutCurve1, string(m_zCurve1) );
		icoMapOpt.setShortcutCode( kShortcutCurve2, string(m_zCurve2) );
		icoMapOpt.setShortcutCode( kShortcutCurve3, string(m_zCurve3) );
		icoMapOpt.setShortcutCode( kShortcutCurve4, string(m_zCurve4) );
		icoMapOpt.setShortcutCode( kShortcutCurve5, string(m_zCurve5) );
		icoMapOpt.setShortcutCode( kShortcutCurve6, string(m_zCurve6) );
		icoMapOpt.setShortcutCode( kShortcutCurve7, string(m_zCurve7) );
		icoMapOpt.setShortcutCode( kShortcutCurve8, string(m_zCurve8) );
		
		icoMapOpt.setShortcutCode( kShortcutLine, string(m_zLine) );
		icoMapOpt.setShortcutCode( kShortcutLineAngle, string(m_zLineAngle) );
		icoMapOpt.setShortcutCode( kShortcutGenie, string(m_zCustom) );
		icoMapOpt.setShortcutCode( kShortcutLeft, string(m_zLeft) );
		icoMapOpt.setShortcutCode( kShortcutRight, string(m_zRight) );
		icoMapOpt.setShortcutCode( kShortcutGreater, string(m_zGreater) );
		icoMapOpt.setShortcutCode( kShortcutLess, string(m_zLess) );
		icoMapOpt.setShortcutCode( kShortcutForward, string(m_zForward) );
		icoMapOpt.setShortcutCode( kShortcutReverse, string(m_zReverse) );
		
		icoMapOpt.setShortcutCode( kShortcutFinishSketch, string(m_zFinishSketch) );
		icoMapOpt.setShortcutCode( kShortcutFinishPart, string(m_zFinishPart) );
		icoMapOpt.setShortcutCode( kShortcutDeleteSketch, string(m_zDeleteSketch) );
	}
}

void CShortcutSettings::OnChangeCommand(UINT nID)
{
	try
	{
		UpdateData( TRUE );

		// Enable the Apply button
		SetModified( TRUE );
	}
	catch (UCLIDException &uclidException)
	{
		uclidException.display();
	}
	catch (...)
	{
		UCLIDException uclidException("ELI02182", "Unknown exception was caught");
		uclidException.display();
	}
}

void CShortcutSettings::OnButtonRestore() 
{
	try
	{
		IcoMapOptions& icoMapOpt = IcoMapOptions::sGetInstance();

		///////////////
		// Set defaults
		///////////////
		m_zCurve1 = (icoMapOpt.getDefaultShortcut( kShortcutCurve1 )).c_str();
		m_zCurve2 = (icoMapOpt.getDefaultShortcut( kShortcutCurve2 )).c_str();
		m_zCurve3 = (icoMapOpt.getDefaultShortcut( kShortcutCurve3 )).c_str();
		m_zCurve4 = (icoMapOpt.getDefaultShortcut( kShortcutCurve4 )).c_str();
		m_zCurve5 = (icoMapOpt.getDefaultShortcut( kShortcutCurve5 )).c_str();
		m_zCurve6 = (icoMapOpt.getDefaultShortcut( kShortcutCurve6 )).c_str();
		m_zCurve7 = (icoMapOpt.getDefaultShortcut( kShortcutCurve7 )).c_str();
		m_zCurve8 = (icoMapOpt.getDefaultShortcut( kShortcutCurve8 )).c_str();
		m_zLine = (icoMapOpt.getDefaultShortcut( kShortcutLine )).c_str();
		m_zLineAngle = (icoMapOpt.getDefaultShortcut( kShortcutLineAngle )).c_str();
		m_zCustom = (icoMapOpt.getDefaultShortcut( kShortcutGenie )).c_str();
		m_zLeft = (icoMapOpt.getDefaultShortcut( kShortcutLeft )).c_str();
		m_zRight = (icoMapOpt.getDefaultShortcut( kShortcutRight )).c_str();
		m_zGreater = (icoMapOpt.getDefaultShortcut( kShortcutGreater )).c_str();
		m_zLess = (icoMapOpt.getDefaultShortcut( kShortcutLess )).c_str();
		m_zForward = (icoMapOpt.getDefaultShortcut( kShortcutForward )).c_str();
		m_zReverse = (icoMapOpt.getDefaultShortcut( kShortcutReverse )).c_str();
		m_zFinishSketch = (icoMapOpt.getDefaultShortcut( kShortcutFinishSketch )).c_str();
		m_zFinishPart = (icoMapOpt.getDefaultShortcut( kShortcutFinishPart )).c_str();
		m_zDeleteSketch = (icoMapOpt.getDefaultShortcut( kShortcutDeleteSketch )).c_str();

		SetModified( TRUE );

		// Update display
		UpdateData( FALSE );
	}
	catch (UCLIDException &uclidException)
	{
		uclidException.display();
	}
	catch (...)
	{
		UCLIDException uclidException("ELI01858", "Unknown exception was caught");
		uclidException.display();
	}
}

bool CShortcutSettings::validateShortcuts()
{
	vector<CString> vecShortcuts;
	vecShortcuts.clear();

	if (findDuplicateShortcut(m_zCurve1, vecShortcuts) || m_zCurve1.IsEmpty())
	{
		m_ctrlCurve1.SetFocus();
		m_ctrlCurve1.SetSel(0, -1);
		return false;
	}
	if (findDuplicateShortcut(m_zCurve2, vecShortcuts) || m_zCurve2.IsEmpty())
	{
		m_ctrlCurve2.SetFocus();
		m_ctrlCurve2.SetSel(0, -1);
		return false;
	}
	if (findDuplicateShortcut(m_zCurve3, vecShortcuts) || m_zCurve3.IsEmpty())
	{
		m_ctrlCurve3.SetFocus();
		m_ctrlCurve3.SetSel(0, -1);
		return false;
	}
	if (findDuplicateShortcut(m_zCurve4, vecShortcuts) || m_zCurve4.IsEmpty())
	{
		m_ctrlCurve4.SetFocus();
		m_ctrlCurve4.SetSel(0, -1);
		return false;
	}
	if (findDuplicateShortcut(m_zCurve5, vecShortcuts) || m_zCurve5.IsEmpty())
	{
		m_ctrlCurve5.SetFocus();
		m_ctrlCurve5.SetSel(0, -1);
		return false;
	}
	if (findDuplicateShortcut(m_zCurve6, vecShortcuts) || m_zCurve6.IsEmpty())
	{
		m_ctrlCurve6.SetFocus();
		m_ctrlCurve6.SetSel(0, -1);
		return false;
	}
	if (findDuplicateShortcut(m_zCurve7, vecShortcuts) || m_zCurve7.IsEmpty())
	{
		m_ctrlCurve7.SetFocus();
		m_ctrlCurve7.SetSel(0, -1);
		return false;
	}
	if (findDuplicateShortcut(m_zCurve8, vecShortcuts) || m_zCurve8.IsEmpty())
	{
		m_ctrlCurve8.SetFocus();
		m_ctrlCurve8.SetSel(0, -1);
		return false;
	}
	if (findDuplicateShortcut(m_zLine, vecShortcuts) || m_zLine.IsEmpty())
	{
		m_ctrlLine.SetFocus();
		m_ctrlLine.SetSel(0, -1);
		return false;
	}
	if (findDuplicateShortcut(m_zLineAngle, vecShortcuts) || m_zLineAngle.IsEmpty())
	{
		m_ctrlLineAngle.SetFocus();
		m_ctrlLineAngle.SetSel(0, -1);
		return false;
	}
	if (findDuplicateShortcut(m_zCustom, vecShortcuts) || m_zCustom.IsEmpty())
	{
		m_ctrlCustom.SetFocus();
		m_ctrlCustom.SetSel(0, -1);
		return false;
	}
	if (findDuplicateShortcut(m_zRight, vecShortcuts) || m_zRight.IsEmpty())
	{
		m_ctrlRight.SetFocus();
		m_ctrlRight.SetSel(0, -1);
		return false;
	}
	if (findDuplicateShortcut(m_zLeft, vecShortcuts) || m_zLeft.IsEmpty())
	{
		m_ctrlLeft.SetFocus();
		m_ctrlLeft.SetSel(0, -1);
		return false;
	}
	if (findDuplicateShortcut(m_zLess, vecShortcuts) || m_zLess.IsEmpty())
	{
		m_ctrlLess.SetFocus();
		m_ctrlLess.SetSel(0, -1);
		return false;
	}
	if (findDuplicateShortcut(m_zGreater, vecShortcuts) || m_zGreater.IsEmpty())
	{
		m_ctrlGreater.SetFocus();
		m_ctrlGreater.SetSel(0, -1);
		return false;
	}
	if (findDuplicateShortcut(m_zForward, vecShortcuts) || m_zForward.IsEmpty())
	{
		m_ctrlForward.SetFocus();
		m_ctrlForward.SetSel(0, -1);
		return false;
	}
	if (findDuplicateShortcut(m_zReverse, vecShortcuts) || m_zReverse.IsEmpty())
	{
		m_ctrlReverse.SetFocus();
		m_ctrlReverse.SetSel(0, -1);
		return false;
	}
	if (findDuplicateShortcut(m_zFinishSketch, vecShortcuts) || m_zFinishSketch.IsEmpty())
	{
		m_ctrlFinishSketch.SetFocus();
		m_ctrlFinishSketch.SetSel(0, -1);
		return false;
	}
	if (findDuplicateShortcut(m_zFinishPart, vecShortcuts) || m_zFinishPart.IsEmpty())
	{
		m_ctrlFinishPart.SetFocus();
		m_ctrlFinishPart.SetSel(0, -1);
		return false;
	}
	if (findDuplicateShortcut(m_zDeleteSketch, vecShortcuts) || m_zDeleteSketch.IsEmpty())
	{
		m_ctrlDeleteSketch.SetFocus();
		m_ctrlDeleteSketch.SetSel(0, -1);
		return false;
	}
	 
	return true;
}

bool CShortcutSettings::findDuplicateShortcut(CString zShortcut, vector<CString> &vecExistingShortcuts)
{
	if (!zShortcut.IsEmpty())
	{
		zShortcut.MakeUpper();
		// Search for this string already being present
		vector<CString>::iterator vecIter = find( vecExistingShortcuts.begin(), vecExistingShortcuts.end(), zShortcut );
		
		// Was the string found
		if (vecIter != vecExistingShortcuts.end())
		{
			// found a duplicate
			return true;
		}
		else
		{
			// Not found, add this to the collection
			vecExistingShortcuts.push_back( zShortcut );
		}
	}
	
	// no duplicates are found
	return false;
}

BOOL CShortcutSettings::OnApply() 
{	
	saveSettings();

	m_bApplied = true;
	
	return CPropertyPage::OnApply();
}

BOOL CShortcutSettings::OnKillActive() 
{
	if (!validateShortcuts())
	{
		::MessageBox( NULL, 
			"Duplicate keys or Null key(s) found. Please enter proper key code(s).",
			"Validation Error", MB_ICONEXCLAMATION | MB_OK );

		return FALSE;
	}
	
	return CPropertyPage::OnKillActive();
}
