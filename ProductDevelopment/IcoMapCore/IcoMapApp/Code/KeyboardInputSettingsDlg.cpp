//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	KeyboardInputSettingsDlg.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS: Duan Wang
//
//==================================================================================================

#include "stdafx.h"
#include "resource.h"
#include "KeyboardInputSettingsDlg.h"

#include <IcoMapOptions.h>
#include <TemporaryResourceOverride.h>
#include <UCLIDException.h>

#include <algorithm>

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

extern HINSTANCE gModuleResource;

/////////////////////////////////////////////////////////////////////////////
// KeyboardInputSettingsDlg property page

IMPLEMENT_DYNCREATE(KeyboardInputSettingsDlg, CPropertyPage)

KeyboardInputSettingsDlg::KeyboardInputSettingsDlg() 
: CPropertyPage(KeyboardInputSettingsDlg::IDD), 
  m_bInitialized(false), 
  m_bApplied(false),
  m_pToolTips(new CToolTipCtrl)
{
	EnableAutomation();
	//{{AFX_DATA_INIT(KeyboardInputSettingsDlg)
	m_editEast = _T("");
	m_editNorth = _T("");
	m_editNE = _T("");
	m_editNW = _T("");
	m_editSouth = _T("");
	m_editSE = _T("");
	m_editSW = _T("");
	m_editWest = _T("");
	//}}AFX_DATA_INIT
}

KeyboardInputSettingsDlg::~KeyboardInputSettingsDlg()
{
	if (m_pToolTips)
	{
		delete m_pToolTips;
	}
}


void KeyboardInputSettingsDlg::OnFinalRelease()
{
	// When the last reference for an automation object is released
	// OnFinalRelease is called.  The base class will automatically
	// deletes the object.  Add additional cleanup required for your
	// object before calling the base class.

	CPropertyPage::OnFinalRelease();
}

void KeyboardInputSettingsDlg::DoDataExchange(CDataExchange* pDX)
{
	CPropertyPage::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(KeyboardInputSettingsDlg)
	DDX_Control(pDX, IDC_EDIT_W, m_ctrlWest);
	DDX_Control(pDX, IDC_EDIT_SW, m_ctrlSW);
	DDX_Control(pDX, IDC_EDIT_SE, m_ctrlSE);
	DDX_Control(pDX, IDC_EDIT_S, m_ctrlSouth);
	DDX_Control(pDX, IDC_EDIT_NW, m_ctrlNW);
	DDX_Control(pDX, IDC_EDIT_NE, m_ctrlNE);
	DDX_Control(pDX, IDC_EDIT_N, m_ctrlNorth);
	DDX_Control(pDX, IDC_EDIT_E, m_ctrlEast);
	DDX_Text(pDX, IDC_EDIT_E, m_editEast);
	DDV_MaxChars(pDX, m_editEast, 1);
	DDX_Text(pDX, IDC_EDIT_N, m_editNorth);
	DDV_MaxChars(pDX, m_editNorth, 1);
	DDX_Text(pDX, IDC_EDIT_NE, m_editNE);
	DDV_MaxChars(pDX, m_editNE, 1);
	DDX_Text(pDX, IDC_EDIT_NW, m_editNW);
	DDV_MaxChars(pDX, m_editNW, 1);
	DDX_Text(pDX, IDC_EDIT_S, m_editSouth);
	DDV_MaxChars(pDX, m_editSouth, 1);
	DDX_Text(pDX, IDC_EDIT_SE, m_editSE);
	DDV_MaxChars(pDX, m_editSE, 1);
	DDX_Text(pDX, IDC_EDIT_SW, m_editSW);
	DDV_MaxChars(pDX, m_editSW, 1);
	DDX_Text(pDX, IDC_EDIT_W, m_editWest);
	DDV_MaxChars(pDX, m_editWest, 1);
	//}}AFX_DATA_MAP
}


BEGIN_MESSAGE_MAP(KeyboardInputSettingsDlg, CPropertyPage)
	//{{AFX_MSG_MAP(KeyboardInputSettingsDlg)
	//}}AFX_MSG_MAP
	ON_CONTROL_RANGE(EN_UPDATE, IDC_EDIT_NW, IDC_EDIT_SE, OnUpdateDirection)
END_MESSAGE_MAP()

BEGIN_DISPATCH_MAP(KeyboardInputSettingsDlg, CPropertyPage)
	//{{AFX_DISPATCH_MAP(KeyboardInputSettingsDlg)
		// NOTE - the ClassWizard will add and remove mapping macros here.
	//}}AFX_DISPATCH_MAP
END_DISPATCH_MAP()

// Note: we add support for IID_IKeyboardInputSettingsDlg to support typesafe binding
//  from VBA.  This IID must match the GUID that is attached to the 
//  dispinterface in the .ODL file.

// {98D869D9-7549-11D5-817A-0050DAD4FF55}
static const IID IID_IKeyboardInputSettingsDlg =
{ 0x98d869d9, 0x7549, 0x11d5, { 0x81, 0x7a, 0x0, 0x50, 0xda, 0xd4, 0xff, 0x55 } };

BEGIN_INTERFACE_MAP(KeyboardInputSettingsDlg, CPropertyPage)
	INTERFACE_PART(KeyboardInputSettingsDlg, IID_IKeyboardInputSettingsDlg, Dispatch)
END_INTERFACE_MAP()

/////////////////////////////////////////////////////////////////////////////
// KeyboardInputSettingsDlg message handlers

BOOL KeyboardInputSettingsDlg::PreTranslateMessage(MSG* pMsg) 
{
	//for tool tip to appear 
	if (m_bInitialized && m_pToolTips)
	{
		m_pToolTips->RelayEvent(pMsg); 
	}
	
	return CPropertyPage::PreTranslateMessage(pMsg);
}

BOOL KeyboardInputSettingsDlg::OnSetActive() 
{
	try
	{
		//if (m_bApplied)
		//{
		//	CancelToClose();
		//}

		IcoMapOptions::sGetInstance().setActiveOptionPageNum(m_iKeyboardInputSettingsPageIndex);
	}
	catch (UCLIDException &uclidException)
	{
		uclidException.display();
		return FALSE;
	}
	catch (...)
	{
		UCLIDException uclidException("ELI01156", "Unknown exception was caught");
		uclidException.display();
		return FALSE;
	}

	return CPropertyPage::OnSetActive();
}

void KeyboardInputSettingsDlg::createToolTips()
{
	m_pToolTips->AddTool(GetDlgItem(IDC_EDIT_NW), "Direction starts from North due West");
	m_pToolTips->AddTool(GetDlgItem(IDC_EDIT_N), "True North");
	m_pToolTips->AddTool(GetDlgItem(IDC_EDIT_NE), "Direction starts from North due East");
	m_pToolTips->AddTool(GetDlgItem(IDC_EDIT_E), "True East");
	m_pToolTips->AddTool(GetDlgItem(IDC_EDIT_SE), "Direction starts from South due East");
	m_pToolTips->AddTool(GetDlgItem(IDC_EDIT_S), "True South");
	m_pToolTips->AddTool(GetDlgItem(IDC_EDIT_SW), "Direction starts from South due West");
	m_pToolTips->AddTool(GetDlgItem(IDC_EDIT_W), "True West");
}

BOOL KeyboardInputSettingsDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);
	
	try
	{
		CPropertyPage::OnInitDialog();
		
		EnableToolTips(true);
		m_pToolTips->Create(this, TTS_ALWAYSTIP);
		
		createToolTips();
		
		if (!m_bInitialized)
		{
			IcoMapOptions& icoMapOpt = IcoMapOptions::sGetInstance();
			m_editNorth = icoMapOpt.getKeyboardInputCode(kN);
			m_editEast = icoMapOpt.getKeyboardInputCode(kE);
			m_editSouth = icoMapOpt.getKeyboardInputCode(kS);
			m_editWest = icoMapOpt.getKeyboardInputCode(kW);
			m_editNE = icoMapOpt.getKeyboardInputCode(kNE);
			m_editSE = icoMapOpt.getKeyboardInputCode(kSE);
			m_editSW = icoMapOpt.getKeyboardInputCode(kSW);
			m_editNW = icoMapOpt.getKeyboardInputCode(kNW);
						
			UpdateData(FALSE);
			
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
		UCLIDException uclidException("ELI01157", "Unknown exception was caught");
		uclidException.display();
		return FALSE;
	}

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}

void KeyboardInputSettingsDlg::saveSettings()
{
	if (m_bInitialized)
	{
		setDirectionShortcut(kN, m_editNorth);
		setDirectionShortcut(kE, m_editEast);
		setDirectionShortcut(kS, m_editSouth);
		setDirectionShortcut(kW, m_editWest);
		setDirectionShortcut(kNE, m_editNE);
		setDirectionShortcut(kSE, m_editSE);
		setDirectionShortcut(kSW, m_editSW);
		setDirectionShortcut(kNW, m_editNW);
	}
}

void KeyboardInputSettingsDlg::OnUpdateDirection(UINT nID)
{
	try
	{
		UpdateData(TRUE);
		switch (nID)
		{
		case IDC_EDIT_N:
			checkAlphaNumeric(m_editNorth, nID);
			break;
		case IDC_EDIT_NE:
			checkAlphaNumeric(m_editNE, nID);
			break;
		case IDC_EDIT_E:
			checkAlphaNumeric(m_editEast, nID);
			break;
		case IDC_EDIT_SE:
			checkAlphaNumeric(m_editSE, nID);
			break;
		case IDC_EDIT_S:
			checkAlphaNumeric(m_editSouth, nID);
			break;
		case IDC_EDIT_SW:
			checkAlphaNumeric(m_editSW, nID);
			break;
		case IDC_EDIT_W:
			checkAlphaNumeric(m_editWest, nID);
			break;
		case IDC_EDIT_NW:
			checkAlphaNumeric(m_editNW, nID);
			break;
		default:
			THROW_LOGIC_ERROR_EXCEPTION("ELI02181")
			break;
		}
		UpdateData(FALSE);

		// Enable the Apply button
		SetModified( TRUE );
	}
	catch (UCLIDException &uclidException)
	{
		uclidException.display();
	}
	catch (...)
	{
		UCLIDException uclidException("ELI02180", "Unknown exception was caught");
		uclidException.display();
	}
	
}

void KeyboardInputSettingsDlg::checkAlphaNumeric(CString &cstrEditCtrl, UINT ctrlID)
{
	if (!cstrEditCtrl.IsEmpty() && !IcoMapOptions::sGetInstance().isSpecialAlphaNumeric(cstrEditCtrl))
	{
		CString msg = cstrEditCtrl + " -- is not a valid key code for assignment.";
		AfxMessageBox(msg);
		cstrEditCtrl = "";
		GetDlgItem(ctrlID)->SetFocus();
	}

}

bool KeyboardInputSettingsDlg::validateKeyCodes()
{
	vector<CString> vecKeyCodes;
	// empty the vector first
	vecKeyCodes.clear();

	if (findDuplicateKey(m_editNorth, vecKeyCodes) || m_editNorth.IsEmpty())
	{
		m_ctrlNorth.SetFocus();
		m_ctrlNorth.SetSel(0, -1);
		return false;
	}
	if (findDuplicateKey(m_editEast, vecKeyCodes) || m_editEast.IsEmpty())
	{
		m_ctrlEast.SetFocus();
		m_ctrlEast.SetSel(0, -1);
		return false;
	}
	if (findDuplicateKey(m_editSouth, vecKeyCodes) || m_editSouth.IsEmpty())
	{
		m_ctrlSouth.SetFocus();
		m_ctrlSouth.SetSel(0, -1);
		return false;
	}
	if (findDuplicateKey(m_editWest, vecKeyCodes) || m_editWest.IsEmpty())
	{
		m_ctrlWest.SetFocus();
		m_ctrlWest.SetSel(0, -1);
		return false;
	}
	if (findDuplicateKey(m_editNE, vecKeyCodes) || m_editNE.IsEmpty())
	{
		m_ctrlNE.SetFocus();
		m_ctrlNE.SetSel(0, -1);
		return false;
	}
	if (findDuplicateKey(m_editSE, vecKeyCodes) || m_editSE.IsEmpty())
	{
		m_ctrlSE.SetFocus();
		m_ctrlSE.SetSel(0, -1);
		return false;
	}
	if (findDuplicateKey(m_editSW, vecKeyCodes) || m_editSW.IsEmpty())
	{
		m_ctrlSW.SetFocus();
		m_ctrlSW.SetSel(0, -1);
		return false;
	}
	if (findDuplicateKey(m_editNW, vecKeyCodes) || m_editNW.IsEmpty())
	{
		m_ctrlNW.SetFocus();
		m_ctrlNW.SetSel(0, -1);
		return false;
	}

	return true;
}

bool KeyboardInputSettingsDlg::findDuplicateKey(CString zKeyCode, vector<CString> &vecExistingKeyCodes)
{
	if (!zKeyCode.IsEmpty())
	{
		// make the string into upper case
		zKeyCode.MakeUpper();
		vector<CString>::iterator vecIter = find (vecExistingKeyCodes.begin(), vecExistingKeyCodes.end(), zKeyCode);
		// same key exists
		if (vecIter != vecExistingKeyCodes.end())
		{
			return true;
		}
		else
		{
			vecExistingKeyCodes.push_back(zKeyCode);
		}
	}

	// no duplicate keys found so far
	return false;
}

void KeyboardInputSettingsDlg::setDirectionShortcut(EDirectionType eDirectionType, CString zShortcut)
{
	IcoMapOptions& icoMapOpt = IcoMapOptions::sGetInstance();
	if (!zShortcut.IsEmpty())
	{
		icoMapOpt.setKeyboardInputCode(eDirectionType, zShortcut.GetAt(0));
	}
	else
	{
		icoMapOpt.setKeyboardInputCode(eDirectionType, 0);
	}
}

BOOL KeyboardInputSettingsDlg::OnApply() 
{	
	saveSettings();
	m_bApplied = true;

	return CPropertyPage::OnApply();
}

BOOL KeyboardInputSettingsDlg::OnKillActive() 
{
	// This member function is called by the framework when the page 
	// is no longer the active page.
	// Perform special data validation tasks.
	if (!validateKeyCodes())
	{
		::MessageBox( NULL, 
			"Duplicate keys or NULL key code(s) found. Please enter valid key code(s).",
			"Validation Error", MB_ICONEXCLAMATION | MB_OK );

		return FALSE;
	}

	// If the data was not updated successfully due to a dialog data 
	// validation (DDV) error, the page retains focus.
	// After this member function returns successfully, the framework 
	// will call the page’s OnOK function.

	return CPropertyPage::OnKillActive();
}
