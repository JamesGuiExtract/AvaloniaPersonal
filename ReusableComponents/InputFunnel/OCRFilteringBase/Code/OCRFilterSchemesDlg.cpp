// OCRFilterSchemesDlg.cpp : implementation file
//

#include "stdafx.h"
#include "OCRFilterSchemesDlg.h"
#include "OCRFilterSettingsDlg.h"

#include <cpputil.h>
#include <UCLIDException.h>
#include <TemporaryResourceOverride.h>
#include <PromptDlg.h>

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

extern AFX_EXTENSION_MODULE OCRFilteringBaseModule;

/////////////////////////////////////////////////////////////////////////////
// OCRFilterSchemesDlg dialog

OCRFilterSchemesDlg::OCRFilterSchemesDlg(CWnd* pParent)
	: CDialog(OCRFilterSchemesDlg::IDD, pParent),
	  m_bInitialized(false)
{
	//{{AFX_DATA_INIT(OCRFilterSchemesDlg)
	m_zValidFilterStrings = _T("");
	//}}AFX_DATA_INIT
	
	// read in all FODs and FSDs in the bin directory (i.e.
	// the same directory as current dll module)
	reloadFODs();
	reloadFSDs();

	getCurrentScheme();
}

OCRFilterSchemesDlg::~OCRFilterSchemesDlg()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16471");
}

void OCRFilterSchemesDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(OCRFilterSchemesDlg)
	DDX_Control(pDX, IDC_BTN_OpenSettingsDlg, m_btnOpen);
	DDX_Control(pDX, IDC_EDIT_EnabledStrings, m_editValidFilterStrings);
	DDX_Control(pDX, IDC_CMB_SchemeName, m_cmbSchemes);
	DDX_Text(pDX, IDC_EDIT_EnabledStrings, m_zValidFilterStrings);
	//}}AFX_DATA_MAP
}


BEGIN_MESSAGE_MAP(OCRFilterSchemesDlg, CDialog)
	//{{AFX_MSG_MAP(OCRFilterSchemesDlg)
	ON_CBN_SELCHANGE(IDC_CMB_SchemeName, OnSelchangeCMBSchemeName)
	ON_WM_SIZE()
	ON_WM_GETMINMAXINFO()
	ON_BN_CLICKED(IDC_BTN_OpenSettingsDlg, OnBTNOpen)
	ON_WM_DESTROY()
	ON_WM_CLOSE()
	ON_CBN_CLOSEUP(IDC_CMB_SchemeName, OnCloseupCMBSchemeName)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// OCRFilterSchemesDlg message handlers
//--------------------------------------------------------------------------------------------------
void OCRFilterSchemesDlg::OnBTNOpen() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(OCRFilteringBaseModule.hResource);
	
	try
	{
		showFilterSettingsDlg(true);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03551")	
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSchemesDlg::OnClose() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(OCRFilteringBaseModule.hResource);
	
	try
	{
		// save the current scheme name
		m_CfgOCRFiltering.setLastUsedOCRFilteringScheme(m_strCurrentScheme);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03763")	

	// Just hide the Dialog
	CDialog::ShowWindow(SW_HIDE);
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSchemesDlg::OnDestroy() 
{
	CDialog::OnDestroy();
	
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(OCRFilteringBaseModule.hResource);
	
	try
	{
		// save the current scheme name
		m_CfgOCRFiltering.setLastUsedOCRFilteringScheme(m_strCurrentScheme);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03764")		
}
//--------------------------------------------------------------------------------------------------
BOOL OCRFilterSchemesDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(OCRFilteringBaseModule.hResource);

	try
	{
		CDialog::OnInitDialog();

		// set icon for this dlg
		HICON hIcon = LoadIcon(OCRFilteringBaseModule.hResource, MAKEINTRESOURCE(IDI_ICON_SchemeDlg));
		SetIcon(hIcon, FALSE);

		// load icon for the button
		m_btnOpen.SetIcon(::LoadIcon(OCRFilteringBaseModule.hResource, MAKEINTRESOURCE(IDI_ICON_MENU)));

		// read last used scheme from persistence store
		getCurrentScheme();

		// update the edit box with current filter scheme
		refresh();

		UpdateData(FALSE);

		m_bInitialized = true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03541")

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSchemesDlg::OnSize(UINT nType, int cx, int cy) 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(OCRFilteringBaseModule.hResource);
	
	try
	{
		CDialog::OnSize(nType, cx, cy);
		
		if (m_bInitialized)
		{
			// get this dialog rect
			CRect rectDlg;
			GetWindowRect(&rectDlg);
			ScreenToClient(&rectDlg);
			
			// only enlarge or shrink the edit box according to 
			// the current dialog bottom and width 
			CRect rectEdit;
			m_editValidFilterStrings.GetWindowRect(&rectEdit);
			ScreenToClient(&rectEdit);
			m_editValidFilterStrings.MoveWindow(rectEdit.left, rectEdit.top, 
						rectDlg.Width()-8, rectDlg.bottom-rectEdit.top-4, TRUE);
			
			Invalidate();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03556")
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSchemesDlg::OnGetMinMaxInfo(MINMAXINFO* lpMMI)
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(OCRFilteringBaseModule.hResource);

	try
	{
		// Minimum width 
		lpMMI->ptMinTrackSize.x = 242;

		// Minimum height
		lpMMI->ptMinTrackSize.y = 48;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03557")
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSchemesDlg::OnSelchangeCMBSchemeName() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(OCRFilteringBaseModule.hResource);
	
	try
	{
		// get current selected text index
		int nIndex = m_cmbSchemes.GetCurSel();	
		if (nIndex != LB_ERR)
		{
			// reset current scheme
			CString cstrText("");
			m_cmbSchemes.GetLBText(nIndex, cstrText);
			if (_stricmp(m_strCurrentScheme.c_str(), cstrText) != 0)
			{
				m_strCurrentScheme = (LPCTSTR)cstrText;
				
				// refresh the edit box
				refresh();
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03547")
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSchemesDlg::OnCloseupCMBSchemeName() 
{
	OnSelchangeCMBSchemeName();	
}
//--------------------------------------------------------------------------------------------------

//==============================================================================================
// Public functions
//==============================================================================================
void OCRFilterSchemesDlg::createModeless()
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(OCRFilteringBaseModule.hResource);

	try
	{
		Create(OCRFilterSchemesDlg::IDD, NULL);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03542")
}	
//--------------------------------------------------------------------------------------------------
const string& OCRFilterSchemesDlg::getCurrentScheme()
{
	if (m_strCurrentScheme.empty())
	{
		m_strCurrentScheme = m_CfgOCRFiltering.getLastUsedOCRFilteringScheme();
	}

	return m_strCurrentScheme;
}
//--------------------------------------------------------------------------------------------------
string OCRFilterSchemesDlg::getValidChars(const string& strInputType)
{
	string strValidChars("");
	
	// get current scheme
	FilterSchemeDefinitions::iterator itFSD = m_FSDs.find(m_strCurrentScheme);
	if (itFSD != m_FSDs.end())
	{
		FilterSchemeDefinition &rFSD = itFSD->second;
		if (rFSD.isFilteringEnabled())
		{
			// fist get all FOD names from the FSDs
			vector<string> vecEnabledFODs = rFSD.getFODNames();
			for (unsigned int n = 0; n < vecEnabledFODs.size(); n++)
			{
				string strFODName(vecEnabledFODs[n]);
				// each input type has its own set of chars with no duplication
				FilterOptionsDefinitions::iterator itFOD = m_FODs.find(strFODName);
				if (itFOD != m_FODs.end())
				{
					FilterOptionsDefinition &rFOD = itFOD->second;
					vector<string> vecInputTypes = rFOD.getInputTypes();
					vector<string>::iterator itInputTypes = find(vecInputTypes.begin(), vecInputTypes.end(), strInputType);
					if (itInputTypes != vecInputTypes.end())
					{
						// We shall count the case sensitivies
						bool bExactCase, bAllUpper, bAllLower;
						rFSD.getCaseSensitivities(strFODName, bExactCase, bAllUpper, bAllLower);
						
						// we need a set of chars from all enabled choices within the input type
						vector<string> vecEnabledChoiceIDs = rFSD.getEnabledInputChoiceIDs(strFODName);
						
						// get all valid chars for this input first
						// Note: this set of chars may or may not be included in the final return
						// since it's all up to the case-sensitivities
						string strBasicValidChars = rFOD.getCharsForInputChoices(vecEnabledChoiceIDs);
						
						// get always enabled chars
						strValidChars += strBasicValidChars + rFOD.getCharsAlwaysEnabled();
							
						string strExactCaseChars("");
						string strUpperCaseChars("");
						string strLowerCaseChars("");
						if (bExactCase)
						{
							strExactCaseChars = strValidChars;
						}
						if (bAllUpper)
						{
							strUpperCaseChars = strValidChars;
							// make all chars to upper case first
							::makeUpperCase(strUpperCaseChars);
						}
						if (bAllLower)
						{
							strLowerCaseChars = strValidChars;
							::makeLowerCase(strLowerCaseChars);
						}
						
						strValidChars = strExactCaseChars + strUpperCaseChars + strLowerCaseChars;
					}
				}
			}
		}
	}
	
	strValidChars = FilterOptionsDefinition::sCreateUniqueCharsSet(strValidChars);
	return strValidChars;
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSchemesDlg::refresh()
{
	if (::IsWindow(m_hWnd))
	{
		refreshSchemesList();
		m_zValidFilterStrings = printValidFilterStringDescriptions().c_str();
		
		UpdateData(FALSE);
	}
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSchemesDlg::setCurrentScheme(const string& strCurrentScheme)
{
	m_strCurrentScheme = strCurrentScheme;

	// store into the persistence
	m_CfgOCRFiltering.setLastUsedOCRFilteringScheme(m_strCurrentScheme);

	refresh();
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSchemesDlg::showFilterSettingsDlg(bool bEditFSD)
{
	if (bEditFSD)
	{
		// if there's no fod, do not show the filtering settings dlg
		if (m_FODs.size() == 0)
		{
			AfxMessageBox("No FOD file is detected in the Bin directory.");
			return;
		}

		OCRFilterSettingsDlg settingsDlg(&m_FODs, &m_FSDs, m_strCurrentScheme, this);
		
		int nRes = settingsDlg.DoModal();
		// Now update the map of FSDs no matter there's a change or not
		reloadFSDs();

		if (nRes != IDOK)
		{
			// set FSD back to the last saved FSD if the user 
			// chooses not to save before close the settings dlg
			m_strCurrentScheme = settingsDlg.m_strPrevFSDName;
		}
		else
		{
			m_strCurrentScheme = settingsDlg.m_strCurrentFSDName;
		}
		
		// refresh the display of all valid strings in the edit box
		refresh();

	}
	else
	{
		FilterOptionsDefinitions tempFODs(m_FODs);
		OCRFilterSettingsDlg settingsDlg(&m_FODs, this);
		int nRes = settingsDlg.DoModal();
		if (nRes == IDCANCEL)
		{
			// restore back to the original FODs
			m_FODs = tempFODs;
		}
	}
}


//==============================================================================================
// Helper functions
//==============================================================================================
void OCRFilterSchemesDlg::createDefaultScheme()
{
	// create FSDs
	FilterSchemeDefinition tempFSD(&m_FODs);
	tempFSD.createDefaultScheme();

	string strFSDName(DEFAULT_SCHEME);
	m_FSDs.insert(make_pair(strFSDName, tempFSD));
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSchemesDlg::createFSD(const string& strFSDFileName)
{
	// get the FSD Name (i.e. the name of the file without extension)
	string strFSDName(::getFileNameWithoutExtension(strFSDFileName));
		
	// create FSD
	FilterSchemeDefinition FSD(&m_FODs);

	// force a read of the FSD file
	FSD.readFromFile(strFSDFileName);
	
	m_FSDs.insert(make_pair(strFSDName, FSD));
}
//--------------------------------------------------------------------------------------------------
string OCRFilterSchemesDlg::getCurrentModuleFullPath()
{
	try
	{
		return getModuleDirectory(OCRFilteringBaseModule.hModule);
	}
	catch (...)
	{
		return string("");
	}
}
//--------------------------------------------------------------------------------------------------
string OCRFilterSchemesDlg::printValidFilterStringDescriptions()
{
	string strValidFilterString("");
	
	// based on current filtering scheme find all enabled choices
	FilterSchemeDefinitions::iterator itFSD = m_FSDs.find(m_strCurrentScheme);
	if (itFSD != m_FSDs.end())
	{
		FilterSchemeDefinition& rFSD = itFSD->second;
		if (rFSD.isFilteringEnabled())
		{
			vector<string> vecInputTypes;
			FilterOptionsDefinitions::iterator itFODs = m_FODs.begin();
			for (; itFODs != m_FODs.end(); itFODs++)
			{
				vector<string> vecTempInputTypes = itFODs->second.getInputTypes();
				for (unsigned int n = 0; n < vecTempInputTypes.size(); n++)
				{
					// get the input type once
					string strInputType(vecTempInputTypes[n]);
					vector<string>::iterator itVec = find(vecInputTypes.begin(), vecInputTypes.end(), strInputType);
					if (itVec == vecInputTypes.end())
					{
						vecInputTypes.push_back(strInputType);
					}
				}
			}

			for (unsigned int n = 0; n < vecInputTypes.size(); n++)
			{
				string strInputType(vecInputTypes[n]);
				strValidFilterString += "[" + strInputType + "]\r\n";
				strValidFilterString += getValidChars(strInputType);
				strValidFilterString += "\r\n \r\n";
			}
		}
	}
	
	return strValidFilterString;
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSchemesDlg::refreshSchemesList()
{
	m_cmbSchemes.ResetContent();

	// iterate through all FSDs, get the FSDs' name
	FilterSchemeDefinitions::iterator itFSD = m_FSDs.begin();
	for (; itFSD != m_FSDs.end(); itFSD++)
	{
		CString cstrToAdd(itFSD->first.c_str());
		// no same entry in the combo box list
		if (m_cmbSchemes.FindString(-1, cstrToAdd) < 0)
		{
			m_cmbSchemes.AddString(cstrToAdd);
		}
	}

	// if current scheme string is empty or it doesn't exist in the cmb list, then
	// set it to the first item found in cmb list
	if (  ( m_strCurrentScheme.empty() 
		  || m_cmbSchemes.FindStringExact(-1, m_strCurrentScheme.c_str()) == LB_ERR )
		&& m_cmbSchemes.GetCount() > 0)
	{
		// set to first item
		int nIndex = m_cmbSchemes.SetCurSel(0);
		CString cstrText("");
		m_cmbSchemes.GetLBText(nIndex, cstrText);
		m_strCurrentScheme = (LPCTSTR)cstrText;
	}
	
	// show current scheme in the combo box window
	m_cmbSchemes.SelectString(-1, m_strCurrentScheme.c_str());
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSchemesDlg::reloadFODs()
{
	// reset contents
	m_FODs.clear();

	// All FSD and FOD files are in the same directory as this dll module
	string strFilePath(getCurrentModuleFullPath() + "\\");
	string strFODFiles = strFilePath + "*.FOD";

	// Now find all FOD files
	CFileFind finder;
	BOOL bSuccess = finder.FindFile(strFODFiles.c_str());
	while (bSuccess)
	{
		// look for the next file
		bSuccess = finder.FindNextFile();
		string strFODFileName(finder.GetFileName());
		// get the Input type Name (i.e. the name of the file without extension)
		string strFODName(::getFileNameWithoutExtension(strFODFileName));
		// get current file name in full path
		strFODFileName = strFilePath + strFODFileName;

		// create FODs
		m_FODs.insert(make_pair(strFODName, FilterOptionsDefinition(strFODFileName)));
	}
}
//--------------------------------------------------------------------------------------------------
void OCRFilterSchemesDlg::reloadFSDs()
{
	// reset contents
	m_FSDs.clear();

	string strFilePath(getCurrentModuleFullPath() + "\\");
	// only if there's non empty FODs
	if (!m_FODs.empty())
	{
		// create a default scheme
		createDefaultScheme();

		// Now find all FSD files
		string strFSDFiles = strFilePath + "*.FSD";
		CFileFind finder;
		BOOL bSuccess = finder.FindFile(strFSDFiles.c_str());
		while (bSuccess)
		{
			// look for the next file
			bSuccess = finder.FindNextFile();
			// Now the file name is the one without full path
			string strFSDFileName(finder.GetFileName());
			
			// create FSD
			createFSD(strFilePath + strFSDFileName);
		}
	}
}
//--------------------------------------------------------------------------------------------------
