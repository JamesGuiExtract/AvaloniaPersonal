
#include "stdafx.h"
#include "afcore.h"
#include "TesterDlgSettingsPage.h"
#include "TesterConfigMgr.h"

#include <UCLIDException.h>
#include <TemporaryResourceOverride.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

class TesterConfigMgr;

extern const int giCONTROL_SPACING;

//-------------------------------------------------------------------------------------------------
// RuleTesterDlg
//-------------------------------------------------------------------------------------------------
TesterDlgSettingsPage::TesterDlgSettingsPage()
: CPropertyPage(TesterDlgSettingsPage::IDD), m_pTesterConfigMgr(NULL)
{
	//{{AFX_DATA_INIT(TesterDlgSettingsPage)
	m_nScope = -1;
	//}}AFX_DATA_INIT
}
//-------------------------------------------------------------------------------------------------
TesterDlgSettingsPage::~TesterDlgSettingsPage()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16306");
}
//-------------------------------------------------------------------------------------------------
void TesterDlgSettingsPage::DoDataExchange(CDataExchange* pDX)
{
	CPropertyPage::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(TesterDlgSettingsPage)
	DDX_Radio(pDX, IDC_RADIO_ALL, m_nScope);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
void TesterDlgSettingsPage::setTesterConfigMgr(TesterConfigMgr *pTesterConfigMgr)
{
	m_pTesterConfigMgr = pTesterConfigMgr;
}
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TesterDlgSettingsPage, CPropertyPage)

BEGIN_MESSAGE_MAP(TesterDlgSettingsPage, CPropertyPage)
	//{{AFX_MSG_MAP(TesterDlgSettingsPage)
	ON_WM_SIZE()
	ON_BN_CLICKED(IDC_RADIO_ALL, OnRadioAll)
	ON_BN_CLICKED(IDC_RADIO_CURRENT, OnRadioCurrent)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// TesterDlgSettingsPage message handlers
//-------------------------------------------------------------------------------------------------
void TesterDlgSettingsPage::OnSize(UINT nType, int cx, int cy) 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	// call the base class method
	CPropertyPage::OnSize(nType, cx, cy);

	// do not continue if the configuration mgr has not been set
	if (!m_pTesterConfigMgr)
	{
		UCLIDException("ELI05249", "Internal coding error!").display();
		return;
	}

	// only do resizing if the controls have been initialized
	if (GetDlgItem(IDC_STATIC_SCOPE) != __nullptr)
	{
		// get the client coords of the dialog
		CRect rectDlg;
		GetClientRect(&rectDlg);
		
		// resize the static group box 
		CRect rectGroupBoxLabel;
		GetDlgItem(IDC_STATIC_SCOPE)->GetWindowRect(&rectGroupBoxLabel);
		ScreenToClient(&rectGroupBoxLabel);
		rectGroupBoxLabel.left = giCONTROL_SPACING;
		rectGroupBoxLabel.top = giCONTROL_SPACING;
		rectGroupBoxLabel.right = rectDlg.right - giCONTROL_SPACING;
		rectGroupBoxLabel.bottom = rectDlg.bottom - giCONTROL_SPACING;
		GetDlgItem(IDC_STATIC_SCOPE)->MoveWindow(&rectGroupBoxLabel);

		// resize the two radio buttons
		CRect rectRadio;
		GetDlgItem(IDC_RADIO_ALL)->GetWindowRect(&rectRadio);
		long nRadioButtonHeight = rectRadio.Height();
		rectRadio.left = rectGroupBoxLabel.left + giCONTROL_SPACING;
		rectRadio.top = rectGroupBoxLabel.top + 2 * giCONTROL_SPACING;
		rectRadio.right = rectGroupBoxLabel.right - giCONTROL_SPACING;
		rectRadio.bottom = rectRadio.top + nRadioButtonHeight;
		GetDlgItem(IDC_RADIO_ALL)->MoveWindow(&rectRadio);
		rectRadio.top = rectRadio.bottom + giCONTROL_SPACING;
		rectRadio.bottom = rectRadio.top + nRadioButtonHeight;
		GetDlgItem(IDC_RADIO_CURRENT)->MoveWindow(&rectRadio);
	}
}
//-------------------------------------------------------------------------------------------------
void TesterDlgSettingsPage::OnRadioAll() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	// store Test scope
	UpdateData(TRUE);
	m_pTesterConfigMgr->setAllAttributesTestScope(m_nScope == 0);
}
//-------------------------------------------------------------------------------------------------
void TesterDlgSettingsPage::OnRadioCurrent() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	// store Test scope
	UpdateData(TRUE);
	m_pTesterConfigMgr->setAllAttributesTestScope(m_nScope == 0);
}
//-------------------------------------------------------------------------------------------------
BOOL TesterDlgSettingsPage::OnInitDialog() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		CPropertyPage::OnInitDialog();
	
		// Retrieve persistent Test Scope
		bool bAllAttributes = m_pTesterConfigMgr->getAllAttributesTestScope();
		m_nScope = bAllAttributes ? 0 : 1;
		UpdateData(FALSE);

		// when this window is displayed, restore the correct state
		// for the scope radio buttons.
		// NOTE: this window can be brought up before or after the
		// current attribute name has been set.
		setCurrentAttributeName(m_strCurrentAttributeName);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05250")

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}

//-------------------------------------------------------------------------------------------------
// Private / helper methods
//-------------------------------------------------------------------------------------------------
void TesterDlgSettingsPage::setCurrentAttributeName(const string& strAttributeName)
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	// update the internal copy of the current attribute name
	m_strCurrentAttributeName = strAttributeName;
	
	// if this dialog has not yet been initialized, then just
	// return
	if (!IsWindow(m_hWnd))
		return;

	// update the UI depending upon whether the current
	// attribute name is set or not.
	const string strCURRENT_ATTRIBUTE_LABEL = "Current attribute";
	if (m_strCurrentAttributeName == "")
	{
		// set the default text for the current-attribute radio button,
		// disable the button, and select the all-attributes radio button
		GetDlgItem(IDC_RADIO_CURRENT)->SetWindowText(strCURRENT_ATTRIBUTE_LABEL.c_str());
		GetDlgItem(IDC_RADIO_CURRENT)->EnableWindow(FALSE);
		m_nScope = 0;
		UpdateData(FALSE);
	}
	else
	{
		// set the correct label for the radio button, and enable the radio button
		string strLabel = strCURRENT_ATTRIBUTE_LABEL;
		strLabel += " ('";
		strLabel += m_strCurrentAttributeName;
		strLabel += "')";
		GetDlgItem(IDC_RADIO_CURRENT)->SetWindowText(strLabel.c_str());
		GetDlgItem(IDC_RADIO_CURRENT)->EnableWindow(TRUE);
	}
}
//-------------------------------------------------------------------------------------------------
const string& TesterDlgSettingsPage::getCurrentAttributeName() const
{
	return m_strCurrentAttributeName;
}
//-------------------------------------------------------------------------------------------------
bool TesterDlgSettingsPage::isAllAttributesScopeSet() const
{
	return m_nScope == 0;
}
//-------------------------------------------------------------------------------------------------
