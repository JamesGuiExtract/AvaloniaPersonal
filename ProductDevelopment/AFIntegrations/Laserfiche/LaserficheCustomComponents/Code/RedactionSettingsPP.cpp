// RedactionSettingsPP.cpp : Implmentation for CRedactionSettingsPP
#include "stdafx.h"
#include "LaserficheCustomComponents.h"
#include "RedactionSettingsPP.h"
#include "IDShieldLF.h"

#include <UCLIDException.h>
#include <comutils.h>
#include <Common.h>

//--------------------------------------------------------------------------------------------------
// CRedactionSettingsPP
//--------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CRedactionSettingsPP, CPropertyPage)
//--------------------------------------------------------------------------------------------------
CRedactionSettingsPP::CRedactionSettingsPP(CIDShieldLF *pIDShieldLF)
	: CPropertyPage(CRedactionSettingsPP::IDD)
	, CIDShieldLFHelper(pIDShieldLF)
	, m_zMasterRSD(_T(""))
	, m_bRedactHCData(FALSE)
	, m_bRedactMCData(FALSE)
	, m_bRedactLCData(FALSE)
	, m_bAutoTag(FALSE)
	, m_bOnDemand(FALSE)
	, m_bEnsureTextRedactions(FALSE)
{
	try
	{
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20790");
}
//--------------------------------------------------------------------------------------------------
CRedactionSettingsPP::~CRedactionSettingsPP()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20791");
}
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CRedactionSettingsPP, CPropertyPage)
	ON_EN_CHANGE(IDC_EDIT_RULES, &CRedactionSettingsPP::OnChange)
	ON_BN_CLICKED(IDC_CHK_HCDATA, &CRedactionSettingsPP::OnChange)
	ON_BN_CLICKED(IDC_CHK_MCDATA, &CRedactionSettingsPP::OnChange)
	ON_BN_CLICKED(IDC_CHK_LCDATA, &CRedactionSettingsPP::OnChange)
	ON_BN_CLICKED(IDC_RADIO_VERIFY_ALL, &CRedactionSettingsPP::OnChange)
	ON_BN_CLICKED(IDC_RADIO_VERIFY_SENSITIVE, &CRedactionSettingsPP::OnChange)
	ON_BN_CLICKED(IDC_CHK_AUTOSTART_VERIFICATION, &CRedactionSettingsPP::OnChange)
	ON_BN_CLICKED(IDC_BTN_RULES_BROWSE, &CRedactionSettingsPP::OnBnClickedBtnBrowse)
	ON_BN_CLICKED(IDC_CHK_ENABLE_VERIFICATION, &CRedactionSettingsPP::OnBnClickedChkEnableVerification)
END_MESSAGE_MAP()

//--------------------------------------------------------------------------------------------------
// Overrides
//--------------------------------------------------------------------------------------------------
void CRedactionSettingsPP::DoDataExchange(CDataExchange* pDX)
{
	CPropertyPage::DoDataExchange(pDX);
	DDX_Text(pDX, IDC_EDIT_RULES, m_zMasterRSD);
	DDX_Check(pDX, IDC_CHK_HCDATA, m_bRedactHCData);
	DDX_Check(pDX, IDC_CHK_MCDATA, m_bRedactMCData);
	DDX_Check(pDX, IDC_CHK_LCDATA, m_bRedactLCData);
	DDX_Check(pDX, IDC_CHK_ENABLE_VERIFICATION, m_bAutoTag);
	DDX_Check(pDX, IDC_CHK_AUTOSTART_VERIFICATION, m_bOnDemand);
	DDX_Check(pDX, IDC_CHK_ENSURE_TEXT_REDACTIONS, m_bEnsureTextRedactions);
}
//--------------------------------------------------------------------------------------------------
BOOL CRedactionSettingsPP::OnInitDialog()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		CPropertyPage::OnInitDialog();

		// Read existing settings from the IDShieldLF instance
		m_zMasterRSD = m_pIDShieldLF->m_RepositorySettings.strMasterRSD.c_str();
		m_bRedactHCData = asMFCBool(m_pIDShieldLF->m_RepositorySettings.bRedactHCData);
		m_bRedactMCData = asMFCBool(m_pIDShieldLF->m_RepositorySettings.bRedactMCData);
		m_bRedactLCData = asMFCBool(m_pIDShieldLF->m_RepositorySettings.bRedactLCData);
		m_bAutoTag = asMFCBool(m_pIDShieldLF->m_RepositorySettings.bAutoTagForVerify);
		((CButton *)GetDlgItem(IDC_RADIO_VERIFY_ALL))->SetCheck(
			asBSTChecked(m_pIDShieldLF->m_RepositorySettings.bTagAllForVerify));
		((CButton *)GetDlgItem(IDC_RADIO_VERIFY_SENSITIVE))->SetCheck(
			asBSTChecked(!m_pIDShieldLF->m_RepositorySettings.bTagAllForVerify));
		m_bOnDemand = asMFCBool(m_pIDShieldLF->m_RepositorySettings.bOnDemandVerify);
		m_bEnsureTextRedactions =
			asMFCBool(m_pIDShieldLF->m_RepositorySettings.bEnsureTextRedactions);

		// Update the controls on the tab
		updateVerifyControls();
		UpdateData(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20835");

	return TRUE;
}
//--------------------------------------------------------------------------------------------------
BOOL CRedactionSettingsPP::OnApply()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		UpdateData(TRUE);

		// Validate the specified rules file
		try
		{
			try
			{
				validateFileOrFolderExistence(m_zMasterRSD.GetString());
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI21065");
		}
		catch (UCLIDException &ue)
		{
			UCLIDException uexOuter("ELI21066", "The specified redaction rules file cannot be found!",
				ue);
			throw uexOuter;
		}

		// Store settings to IDShieldLF instance
		m_pIDShieldLF->m_RepositorySettings.strMasterRSD = m_zMasterRSD.GetString();
		m_pIDShieldLF->m_RepositorySettings.bRedactHCData = asCppBool(m_bRedactHCData);
		m_pIDShieldLF->m_RepositorySettings.bRedactMCData = asCppBool(m_bRedactMCData);
		m_pIDShieldLF->m_RepositorySettings.bRedactLCData = asCppBool(m_bRedactLCData);
		m_pIDShieldLF->m_RepositorySettings.bAutoTagForVerify = asCppBool(m_bAutoTag);
		m_pIDShieldLF->m_RepositorySettings.bTagAllForVerify = asCppBool(
			((CButton *)GetDlgItem(IDC_RADIO_VERIFY_ALL))->GetCheck());
		m_pIDShieldLF->m_RepositorySettings.bOnDemandVerify = asCppBool(m_bOnDemand);
		m_pIDShieldLF->m_RepositorySettings.bEnsureTextRedactions
			= asCppBool(m_bEnsureTextRedactions);

		m_pIDShieldLF->saveSettings();
			
		SetModified(FALSE);

		return CPropertyPage::OnApply();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20836");

	return FALSE;
}

//--------------------------------------------------------------------------------------------------
// Message handlers
//--------------------------------------------------------------------------------------------------
void CRedactionSettingsPP::OnChange()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// If any control has been changed, set modified flag.
		SetModified(TRUE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20837");
}
//--------------------------------------------------------------------------------------------------
void CRedactionSettingsPP::OnBnClickedChkEnableVerification()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		UpdateData(TRUE);

		// Update verification controls to reflect new verification state.
		updateVerifyControls();

		OnChange();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20839");
}
//--------------------------------------------------------------------------------------------------
void CRedactionSettingsPP::OnBnClickedBtnBrowse()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		CFileDialog fileDlg(TRUE, NULL, m_zMasterRSD.GetString(), 
			OFN_HIDEREADONLY | OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR,
			gstrRSD_FILE_OPEN_FILTER.c_str(), NULL);
		
		if (fileDlg.DoModal() == IDOK)
		{
			m_zMasterRSD = fileDlg.GetPathName();

			UpdateData(FALSE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20838");
}

//--------------------------------------------------------------------------------------------------
// Private
//--------------------------------------------------------------------------------------------------
void CRedactionSettingsPP::updateVerifyControls()
{
	GetDlgItem(IDC_RADIO_VERIFY_ALL)->EnableWindow(asCppBool(m_bAutoTag));
	GetDlgItem(IDC_RADIO_VERIFY_SENSITIVE)->EnableWindow(asCppBool(m_bAutoTag));
	GetDlgItem(IDC_CHK_AUTOSTART_VERIFICATION)->EnableWindow(asCppBool(m_bAutoTag));
}
//--------------------------------------------------------------------------------------------------