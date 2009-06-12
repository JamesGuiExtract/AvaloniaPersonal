// RepositorySettingsPP.cpp : implementation file

#include "stdafx.h"
#include "LaserficheCustomComponents.h"
#include "RepositorySettingsPP.h"
#include "IDShieldLF.h"
#include "TagInfoDlg.h"
#include "SelectDirectoryDlg.h"
#include "WaitDlg.h"
#include "LFItemCollection.h"

#include <UCLIDException.h>
#include <comutils.h>
#include <LFMiscUtils.h>

//-------------------------------------------------------------------------------------------------
// CRepositorySettingsPP
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CRepositorySettingsPP, CPropertyPage)
//-------------------------------------------------------------------------------------------------
CRepositorySettingsPP::CRepositorySettingsPP(CIDShieldLF *pIDShieldLF) 
	: CPropertyPage(CRepositorySettingsPP::IDD)
	, CIDShieldLFHelper(pIDShieldLF)
	, m_zSettingsDirectory(_T(""))
{
	try
	{
		// Display "Logging in..." while the admin console is initialized.
		ASSERT_RESOURCE_ALLOCATION("ELI21869", m_pIDShieldLF->m_apDlgWait.get() != NULL);
		m_pIDShieldLF->m_apDlgWait->showMessage("Logging in...");
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20783");
}
//-------------------------------------------------------------------------------------------------
CRepositorySettingsPP::~CRepositorySettingsPP()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20712");
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CRepositorySettingsPP, CPropertyPage)
	ON_BN_CLICKED(IDC_BTN_CREATE_TAGS, &CRepositorySettingsPP::OnBnClickedBtnCreateTags)
	ON_BN_CLICKED(IDC_BTN_ABOUT_TAGS, &CRepositorySettingsPP::OnBnClickedBtnAboutTags)
	ON_BN_CLICKED(IDC_BTN_REFRESH, &CRepositorySettingsPP::OnBnClickedBtnRefresh)
	ON_BN_CLICKED(IDC_BTN_CHANGE_REPOSITORY, &CRepositorySettingsPP::OnBnClickedBtnChangeRepository)
	ON_BN_CLICKED(IDC_BTN_UNTAG_PENDING, &CRepositorySettingsPP::OnBnClickedBtnUntagPending)
	ON_BN_CLICKED(IDC_BTN_MARK_VERIFIED, &CRepositorySettingsPP::OnBnClickedBtnMarkVerified)
	ON_BN_CLICKED(IDC_BTN_MARK_FAILED_AS_PENDING, &CRepositorySettingsPP::OnBnClickedBtnMarkFailedAsPending)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// Overrides
//-------------------------------------------------------------------------------------------------
void CRepositorySettingsPP::DoDataExchange(CDataExchange* pDX)
{
	CPropertyPage::DoDataExchange(pDX);
	DDX_Text(pDX, IDC_EDIT_SETTINGS_DIRECTORY, m_zSettingsDirectory);
}
//-------------------------------------------------------------------------------------------------
BOOL CRepositorySettingsPP::OnInitDialog()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		try
		{
			CPropertyPage::OnInitDialog();

			updateStatus();

			// Initialization complete; hide the "Logging in..." wait message.
			ASSERT_RESOURCE_ALLOCATION("ELI21870", m_pIDShieldLF->m_apDlgWait.get() != NULL);
			m_pIDShieldLF->m_apDlgWait->hide();
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20784")
	}
	catch (UCLIDException &ue)
	{
		ue.display();

		// If a problem was encountered initializing the repository settings tab, don't display
		// the dialog. End it.
		EndDialog(IDCANCEL);
	}

	return TRUE;
}

//-------------------------------------------------------------------------------------------------
// CRepositorySettingsPP message handlers
//-------------------------------------------------------------------------------------------------
void CRepositorySettingsPP::OnBnClickedBtnAboutTags()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Display information on the tags ID Shield needs.
		CTagInfoDlg dlgTagInfo(this);
		dlgTagInfo.DoModal();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20779");
}
//-------------------------------------------------------------------------------------------------
void CRepositorySettingsPP::OnBnClickedBtnCreateTags()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		CWaitCursor waitCursor;

		m_pIDShieldLF->createTags(m_vecMissingTags);

		updateStatus();

		if (!m_vecMissingTags.empty())
		{
			throw UCLIDException("ELI20785", "Unknown error creating ID Shield tags!");
		}

		MessageBox("The required tags were successfully created.", 
			"ID Shield Administration Console", MB_OK);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20720");
}
//-------------------------------------------------------------------------------------------------
void CRepositorySettingsPP::OnBnClickedBtnRefresh()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		CWaitCursor waitCursor;

		updateStatus();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20786");
}
//-------------------------------------------------------------------------------------------------
void CRepositorySettingsPP::OnBnClickedBtnChangeRepository()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	try
	{
		CWaitCursor waitCursor;

		ILFFolderPtr ipSettingsFolder = NULL;
		string strSettingsDirectory;

		try
		{
			ipSettingsFolder = m_pIDShieldLF->getSettingsFolder(true);
			
			if (ipSettingsFolder)
			{
				strSettingsDirectory = ipSettingsFolder->FindFullPath();
			}
		}
		catch (...)
		{
			// Do not pass on exceptions finding the settings directory on this tab.
			// Try to find it, but if it is not found it should not be considered a problem
			// at this point.
		}

		CSelectDirectoryDlg dlgSelectDirectory;
		if (dlgSelectDirectory.prompt(strSettingsDirectory) == IDOK)
		{
			// The user has entered a new Laserfiche folder to function as the ID Shield folder.
			// We need a new waitcursor after the SelectDirectory dialog is closed.
			CWaitCursor waitCursor;

			ILFDatabasePtr ipDatabase = m_pIDShieldLF->m_ipDatabase;
			ASSERT_RESOURCE_ALLOCATION("ELI20823", ipDatabase != NULL);

			ILFFolderPtr ipNewFolder = NULL;
			
			try
			{
				try
				{
					// Locate the specified folder
					ipNewFolder = ipDatabase->GetEntryByPath(get_bstr_t(strSettingsDirectory));
				}
				CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20824");
			}
			catch (UCLIDException &ue)
			{
				UCLIDException uexOuter("ELI20817", "Invalid Laserfiche Directory!", ue);
				uexOuter.addDebugInfo("Path", strSettingsDirectory);
				uexOuter.display();
			}
	
			if (ipNewFolder != NULL)
			{
				try
				{
					// Verify necessary access rights to the new folder.
					verifyHasRight(ipNewFolder, ACCESS_CREATE_FILES,
						"Missing \"Create Documents\" access rights for ID Shield settings folder!");
					verifyHasRight(ipNewFolder, ACCESS_WRITE,
						"Missing \"Modify Contents\" access rights for ID Shield settings folder!");
					verifyHasRight(ipNewFolder, ACCESS_READ,
						"Missing \"Read\" access rights for ID Shield settings folder!");

					string strNewPath = asString(ipNewFolder->FindFullPath()) + 
						"\\" + gstrSETTINGS_FILE;

					// Attempt to locate any IDShieldLF ini file that already exists in this folder
					ILFDocumentPtr ipExistingIni = NULL;
					try
					{
						ipExistingIni = ipDatabase->GetEntryByPath(get_bstr_t(strNewPath));
					}
					catch (...) 
					{
						// It is not a problem if an existing ini file could not be found.
					}

					if (ipExistingIni == NULL && ipSettingsFolder != NULL)
					{
						// If we failed to find an existing ini at the new path, attempt to copy
						// from the old directory as a basis for the new ini file.
						string strOldPath = asString(ipSettingsFolder->FindFullPath()) 
							+ "\\" + gstrSETTINGS_FILE;

						try
						{
							ILFDocumentPtr ipOldIni = 
								ipDatabase->GetEntryByPath(get_bstr_t(strOldPath));

							ILFDocumentPtr ipCopy(CLSID_LFDocument);
							ASSERT_RESOURCE_ALLOCATION("ELI20825", ipCopy != NULL);

							ipCopy->CreateCopyOf(ipOldIni, get_bstr_t(gstrSETTINGS_FILE), 
								ipNewFolder, VARIANT_FALSE);
						}
						catch (...) 
						{
							// If we failed to move the ini from the old directory (for instance, if
							// it didn't exist in the old directory), this isn't a critical error
							// since a new ini file will be created anyway. Ignore any exceptions here.
						}
					}

					// Move gstrTAG_SETTINGS to the new folder. 
					if (ipSettingsFolder != NULL)
					{
						removeTag(ipSettingsFolder, gstrTAG_SETTINGS);
					}
					
					addTag(ipNewFolder, gstrTAG_SETTINGS);

					m_pIDShieldLF->m_ipSettingsFolder = ipNewFolder;

					updateStatus();
				}
				CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20816")
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20792");
}
//-------------------------------------------------------------------------------------------------
void CRepositorySettingsPP::OnBnClickedBtnUntagPending()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	
	// Declare outside try block in order to dispose of properly in case of an exception
	ILFDocumentPtr ipDocument = NULL;
	
	try
	{
		try
		{
			vector<string> vecPrompts(2);
			vecPrompts[0] = "Are you sure you wish to remove the tag \"" + 
							gstrTAG_PENDING_PROCESSING + "\" from all documents?";
			vecPrompts[1] = "This operation cannot be undone once started!\r\n\r\nAre you sure "
							"you wish to remove the tag \"" + 
							gstrTAG_PENDING_PROCESSING + "\" from all documents?";

			for (size_t i = 0; i < vecPrompts.size(); i++)
			{
				if (MessageBox(vecPrompts[i].c_str(), gstrPRODUCT_NAME.c_str(), 
					MB_ICONWARNING | MB_YESNO) == IDNO)
				{
					return;
				}
			}

			CWaitDlg dlgWait("Removing \"Needs Processing\" tag from all documents...", m_pParentWnd);

			int nFailedCount = performDocumentOperations(m_pIDShieldLF->m_ipDatabase,
				m_pIDShieldLF->m_RepositorySettings.strDocumentSearchType,
				vector<string>(),
				vector<string>(1, gstrTAG_PENDING_PROCESSING),
				vector<string>(1, gstrTAG_PENDING_PROCESSING),
				vector<string>());

			dlgWait.close();

			// Prompt concerning any failures.
			if (nFailedCount > 0)
			{
				CString csMessage;
				csMessage.Format("Failed to remove \"Needs Processing\" tag from %i document(s)!\r\n\r\n"
					"See log for details.", nFailedCount); 
				MessageBox(csMessage, "ID Shield Administration Console", MB_ICONWARNING);
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20983");
	}
	catch (UCLIDException &ue)
	{
		safeDispose(ipDocument);

		ue.display();
	}
}
//-------------------------------------------------------------------------------------------------
void CRepositorySettingsPP::OnBnClickedBtnMarkVerified()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	// Declare outside try block in order to dispose of properly in case of an exception
	ILFDocumentPtr ipDocument = NULL;
	
	try
	{
		try
		{
			vector<string> vecPrompts(2);
			vecPrompts[0] = "Are you sure you wish to add the tag \"" +
							gstrTAG_VERIFIED + "\" to all documents tagged \"" + 
							gstrTAG_PROCESSED + "\"?\r\n\r\nThis will result in the tag \"" +
							gstrTAG_PENDING_VERIFICATION + "\" being removed (if present).";
			vecPrompts[1] = "This operation cannot be undone once started!\r\n\r\n"
							"Are you sure you wish to add the tag \"" +
							gstrTAG_VERIFIED + "\" to all documents tagged \"" + 
							gstrTAG_PROCESSED + "\"?";

			for (size_t i = 0; i < vecPrompts.size(); i++)
			{
				if (MessageBox(vecPrompts[i].c_str(), gstrPRODUCT_NAME.c_str(), 
					MB_ICONWARNING | MB_YESNO) == IDNO)
				{
					return;
				}
			}

			CWaitDlg dlgWait("Marking \"Processed\" documents as \"Verified\"...", m_pParentWnd);

			int nFailedCount = performDocumentOperations(m_pIDShieldLF->m_ipDatabase,
				m_pIDShieldLF->m_RepositorySettings.strDocumentSearchType,
				vector<string>(1, gstrTAG_VERIFIED),
				vector<string>(1, gstrTAG_PENDING_VERIFICATION),
				vector<string>(1, gstrTAG_PROCESSED),
				vector<string>(1, gstrTAG_VERIFIED),
				gcolorCLUE_HIGHLIGHT);

			dlgWait.close();

			// Prompt concerning any failures.
			if (nFailedCount > 0)
			{
				CString csMessage;
				csMessage.Format("Failed to tag %i \"Processed\" document(s) as \"Verified\"!\r\n\r\n"
					"See log for details.", nFailedCount); 
				MessageBox(csMessage, "ID Shield Administration Console", MB_ICONWARNING);
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20985");
	}
	catch (UCLIDException &ue)
	{
		safeDispose(ipDocument);

		ue.display();
	}
}
//-------------------------------------------------------------------------------------------------
void CRepositorySettingsPP::OnBnClickedBtnMarkFailedAsPending()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	// Declare outside try block in order to dispose of properly in case of an exception
	ILFDocumentPtr ipDocument = NULL;
	
	try
	{
		try
		{
			vector<string> vecPrompts(2);
			vecPrompts[0] = "Are you sure you wish to change all documents tagged \"" + 
							gstrTAG_FAILED_PROCESSING + "\" to be tagged \"" +
							gstrTAG_PENDING_PROCESSING + "\" instead?";
			vecPrompts[1] = "This operation cannot be undone once started!\r\n\r\nAre you sure "
							"you want all documents tagged \"" + 
							gstrTAG_FAILED_PROCESSING + "\" to be tagged \"" + 
							gstrTAG_PENDING_PROCESSING + "\" instead?";

			for (size_t i = 0; i < vecPrompts.size(); i++)
			{
				if (MessageBox(vecPrompts[i].c_str(), gstrPRODUCT_NAME.c_str(), 
					MB_ICONWARNING | MB_YESNO) == IDNO)
				{
					return;
				}
			}

			CWaitDlg dlgWait("Marking \"Failed Processing\" documents as \"Needs Processing\"...", 
				m_pParentWnd);

			int nFailedCount = performDocumentOperations(m_pIDShieldLF->m_ipDatabase,
				m_pIDShieldLF->m_RepositorySettings.strDocumentSearchType,
				vector<string>(1, gstrTAG_PENDING_PROCESSING),
				vector<string>(1, gstrTAG_FAILED_PROCESSING),
				vector<string>(1, gstrTAG_FAILED_PROCESSING),
				vector<string>(1, gstrTAG_PENDING_PROCESSING));

			dlgWait.close();

			// Prompt concerning any failures.
			if (nFailedCount > 0)
			{
				CString csMessage;
				csMessage.Format("Failed to tag %i \"Failed Processing\" document(s) as "
					"\"Needs Processing\"!\r\n\r\nSee log for details.", nFailedCount); 
				MessageBox(csMessage, "ID Shield Administration Console", MB_ICONWARNING);
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI21676");
	}
	catch (UCLIDException &ue)
	{
		safeDispose(ipDocument);

		ue.display();
	}
}


//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CRepositorySettingsPP::updateStatus()
{
	bool bShowRedactionTab = false;
	bool bEnableChangeRepository = true;

	m_vecMissingTags = m_pIDShieldLF->findMissingTags();

	// If the repository is missing any tags, enable the "Create Tags" button
	if (m_vecMissingTags.size() == 0)
	{
		GetDlgItem(IDC_BTN_CREATE_TAGS)->EnableWindow(FALSE);
	}

	// Attempt to locate the settings folder.
	ILFFolderPtr ipSettingsFolder = NULL;
	try
	{
		ipSettingsFolder = m_pIDShieldLF->getSettingsFolder(true);
		ASSERT_RESOURCE_ALLOCATION("ELI21026", ipSettingsFolder != NULL);

		m_zSettingsDirectory = asString(ipSettingsFolder->FindFullPath()).c_str();

		try
		{
			verifyHasRight(m_pIDShieldLF->m_ipSettingsFolder, ACCESS_CREATE_FILES,
				"Missing \"Create Documents\" access rights for ID Shield settings folder!");
			verifyHasRight(m_pIDShieldLF->m_ipSettingsFolder, ACCESS_WRITE,
				"Missing \"Modify Contents\" access rights for ID Shield settings folder!");
			verifyHasRight(m_pIDShieldLF->m_ipSettingsFolder, ACCESS_READ,
				"Missing \"Read\" access rights for ID Shield settings folder!");

			// If the settings folder is found and we have the necessary rights, show the
			// redaction settings tab.
			bShowRedactionTab = true;
		}
		catch (UCLIDException &ue)
		{
			// We don't have sufficient rights to the folder.  Display the exception, then 
			// indicate the problem in the settings directory field.
			ue.display();
			m_zSettingsDirectory += " (insufficient permissions)";
		}
	}
	catch (UCLIDException &ue)
	{
		if (ue.getAllELIs().find("ELI20767") != string::npos)
		{
			// If multiple folders tagged as settings folder, indicate the problem in the 
			// settings directory field.
			m_zSettingsDirectory = "Multiple folders are tagged \"";
			m_zSettingsDirectory += gstrTAG_SETTINGS.c_str();
			m_zSettingsDirectory += "\"!";
			bEnableChangeRepository = false;
		}
		else
		{
			// For all other problems locating the settings directory, indicate
			// that it cannot be found.
			m_zSettingsDirectory = "Could not find a folder tagged \"";
			m_zSettingsDirectory += gstrTAG_SETTINGS.c_str();
			m_zSettingsDirectory += "\"!";
		}
	}

	UpdateData(FALSE);

	m_pIDShieldLF->showRedactionTab(bShowRedactionTab);
	GetDlgItem(IDC_BTN_CHANGE_REPOSITORY)->EnableWindow(asMFCBool(bEnableChangeRepository));

	// [FlexIDSIntegrations:11, 53]
	// If the repository is properly configured, warn a user of the administrative console
	// not to make changes or perform operations without stopping other users/services
	// first.
	static bool bPromptedAboutExistingSessions = false;
	if (!bPromptedAboutExistingSessions)
	{
		bPromptedAboutExistingSessions = true;

		if (bShowRedactionTab)
		{
			// Hide the "Logging in" message before displaying the warning prompt.
			ASSERT_RESOURCE_ALLOCATION("ELI21871", m_pIDShieldLF->m_apDlgWait.get() != NULL);
			m_pIDShieldLF->m_apDlgWait->hide();

			// Only prompt if the repository is configured when the admin console is first launched.
			int nResponse = MessageBox(
				"The ID Shield configuration cannot be changed while ID Shield is in use.\r\n\r\n"
				"Please ensure that no users or administrators are performing any ID Shield\r\n"
				"operations and that no instances of the ID Shield background redaction\r\n"
				"service are running.\r\n\r\n"
				"Click OK to continue to view / modify the ID Shield configuration.\r\n"
				"Click Cancel to exit the ID Shield Administration console now.",
				gstrPRODUCT_NAME.c_str(), MB_ICONWARNING | MB_OKCANCEL);

			if (nResponse == IDCANCEL)
			{
				EndDialog(IDCANCEL);
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------
