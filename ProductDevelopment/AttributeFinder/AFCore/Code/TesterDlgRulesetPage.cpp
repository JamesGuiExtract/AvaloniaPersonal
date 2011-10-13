// TesterDlgRulesetPage.cpp : implementation file
//

#include "stdafx.h"
#include "afcore.h"
#include "TesterDlgRulesetPage.h"
#include "RulesetPropertiesDlg.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include "Common.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

extern const int giCONTROL_SPACING;

//-------------------------------------------------------------------------------------------------
// TesterDlgRulesetPage property page
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNCREATE(TesterDlgRulesetPage, CPropertyPage)

TesterDlgRulesetPage::TesterDlgRulesetPage() : CPropertyPage(TesterDlgRulesetPage::IDD)
{
	//{{AFX_DATA_INIT(TesterDlgRulesetPage)
		// NOTE: the ClassWizard will add member initialization here
	//}}AFX_DATA_INIT
}

TesterDlgRulesetPage::~TesterDlgRulesetPage()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16305");
}

void TesterDlgRulesetPage::DoDataExchange(CDataExchange* pDX)
{
	CPropertyPage::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(TesterDlgRulesetPage)
	DDX_Control(pDX, IDC_RULESET, m_editRulesFileName);
	//}}AFX_DATA_MAP
}


BEGIN_MESSAGE_MAP(TesterDlgRulesetPage, CPropertyPage)
	//{{AFX_MSG_MAP(TesterDlgRulesetPage)
	ON_BN_CLICKED(IDC_BTN_BROWSE_RSD, OnBtnBrowseRsd)
	ON_WM_SIZE()
	ON_BN_CLICKED(IDC_BTN_RULESET_PROPERTIES, OnBtnRulesetProperties)
	ON_EN_CHANGE(IDC_RULESET, &TesterDlgRulesetPage::OnChangeRuleset)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// TesterDlgRulesetPage message handlers
//-------------------------------------------------------------------------------------------------
string TesterDlgRulesetPage::getRulesFileName()
{
	CString zFileName;
	m_editRulesFileName.GetWindowText(zFileName );
	return string(zFileName);
}
//-------------------------------------------------------------------------------------------------
void TesterDlgRulesetPage::setRulesFileName(string strFileName)
{
	m_editRulesFileName.SetWindowText(strFileName.c_str());
}
//-------------------------------------------------------------------------------------------------
void TesterDlgRulesetPage::OnBtnBrowseRsd() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		string strFileName = getRulesFileName();

		// ask user to select file to load
		CFileDialog fileDlg(TRUE, ".rsd;.etf", strFileName.c_str(), 
			OFN_ENABLESIZING | OFN_EXPLORER | OFN_FILEMUSTEXIST | OFN_HIDEREADONLY | OFN_PATHMUSTEXIST,
			gstrRSD_FILE_OPEN_FILTER.c_str(), this);
	
		if (fileDlg.DoModal() != IDOK)
		{
			return;
		}
		strFileName = (LPCTSTR) fileDlg.GetPathName();

		// verify extension is RSD
		string strExt = getExtensionFromFullPath( strFileName, true );
		if (( strExt != ".rsd" ) && ( strExt != ".etf" ))
		{
			throw UCLIDException("ELI08821", "File is not an RSD file.");
		}

		setRulesFileName( strFileName );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI34011");
}
//-------------------------------------------------------------------------------------------------
void TesterDlgRulesetPage::OnSize(UINT nType, int cx, int cy) 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		CPropertyPage::OnSize(nType, cx, cy);

		if ( GetDlgItem(IDC_RULESET) != __nullptr )
		{
			CRect rectDlg;
			GetClientRect(&rectDlg);
		
			// resize the Ruleset static label
			CRect rectRulesetLabel;
			GetDlgItem(IDC_STATIC_RULESET)->GetWindowRect(&rectRulesetLabel);
			ScreenToClient(&rectRulesetLabel);
			long nRulesetLabelWidth = rectRulesetLabel.Width();
			long nRulesetLabelHeight = rectRulesetLabel.Height();
			rectRulesetLabel.left = giCONTROL_SPACING;
			rectRulesetLabel.top = giCONTROL_SPACING;
			rectRulesetLabel.right = rectRulesetLabel.left + nRulesetLabelWidth;
			rectRulesetLabel.bottom = rectRulesetLabel.top + nRulesetLabelHeight;
			GetDlgItem(IDC_STATIC_RULESET)->MoveWindow(&rectRulesetLabel);

			// Resize the browse button position
			CRect rectBrowseButton;
			GetDlgItem(IDC_BTN_BROWSE_RSD)->GetWindowRect(&rectBrowseButton);
			ScreenToClient(&rectBrowseButton);
			long nBrowseButtonWidth = rectBrowseButton.Width();
			long nBrowseButtonHeight = rectBrowseButton.Height();
			rectBrowseButton.right = rectDlg.right - giCONTROL_SPACING;
			rectBrowseButton.left = rectBrowseButton.right - nBrowseButtonWidth;
			rectBrowseButton.top = rectRulesetLabel.top;
			rectBrowseButton.bottom = rectBrowseButton.top + nBrowseButtonHeight;
			GetDlgItem(IDC_BTN_BROWSE_RSD)->MoveWindow(&rectBrowseButton);
		
			// Set Ruleset edit size and position relative to the other controls
			CRect rectRuleset;
			rectRuleset.left = rectRulesetLabel.right + giCONTROL_SPACING;
			rectRuleset.right = rectBrowseButton.left - giCONTROL_SPACING;
			rectRuleset.top = rectRulesetLabel.top;
			rectRuleset.bottom = rectRulesetLabel.bottom;
			GetDlgItem(IDC_RULESET)->MoveWindow(&rectRuleset);

			// Set Ruleset Properties button position relative to the other controls
			CWnd *pWndPropertiesBtn = GetDlgItem(IDC_BTN_RULESET_PROPERTIES);
			CRect rectPropertiesBtn;
			pWndPropertiesBtn->GetWindowRect(&rectPropertiesBtn);
			ScreenToClient(&rectPropertiesBtn);
			rectPropertiesBtn.MoveToXY(rectRuleset.left, rectRuleset.bottom + giCONTROL_SPACING);
			pWndPropertiesBtn->MoveWindow(&rectPropertiesBtn);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI34012");
}
//-------------------------------------------------------------------------------------------------
void TesterDlgRulesetPage::OnBtnRulesetProperties()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		string strRuleFilename = getRulesFileName();
		validateFileOrFolderExistence(strRuleFilename);

		UCLID_AFCORELib::IRuleSetPtr ipRuleSet(CLSID_RuleSet);
		ASSERT_RESOURCE_ALLOCATION("ELI34014", ipRuleSet != __nullptr);

		ipRuleSet->LoadFrom(strRuleFilename.c_str(), VARIANT_FALSE);

		// Show properties dialog in read-only mode.
		CRuleSetPropertiesDlg dialog(ipRuleSet, true);
		
		dialog.DoModal();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI34013");
}
//-------------------------------------------------------------------------------------------------
void TesterDlgRulesetPage::OnChangeRuleset()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Enable the ruleset properties button iff there is text in the ruleset path edit box.
		BOOL bEnablePropertiesBtn = asMFCBool(!getRulesFileName().empty());
		GetDlgItem(IDC_BTN_RULESET_PROPERTIES)->EnableWindow(bEnablePropertiesBtn);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI34022");
}
