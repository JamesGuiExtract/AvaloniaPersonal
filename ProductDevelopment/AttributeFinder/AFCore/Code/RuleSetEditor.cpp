//-------------------------------------------------------------------------------------------------
//
// COPYRIGHT (c) 2003 - 2008 EXTRACT SYSTEMS LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	RuleSetEditor.cpp
//
// PURPOSE:	Implementation of CRuleSetEditor class
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//-------------------------------------------------------------------------------------------------

#include "stdafx.h"
#include "afcore.h"
#include "RuleSetEditor.h"
#include "ImportRuleSetDlg.h"
#include "Common.h"
#include "RuleTesterDlg.h"

#include <RegistryPersistenceMgr.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <Win32Util.h>
#include <MRUList.h>
#include <ComUtils.h>
#include <PromptDlg.h>
#include <FileDialogEx.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const int giENABLE_LIST_COLUMN = 0;

const string gstrRECOVERY_PROMPT =
	"It appears that you were unable to save your work from \n"
	"the previous session because of an application crash\n"
	"or other catastrophic failure.  Would you like to attempt\n"
	"recovering the Rule Set Definition from your previous session?\n";

// Moved these to header file as data members to allow access in two CPP files
//const int giPRIORITY_LIST_COLUMN = 1;
//const int giDESC_LIST_COLUMN = 2;

//-------------------------------------------------------------------------------------------------
// CRuleSetEditor dialog
//-------------------------------------------------------------------------------------------------
CRuleSetEditor::CRuleSetEditor(const string& strFileName /*=""*/,
							   const string& strBinFolder /*=""*/, 
							   CWnd* pParent /*=NULL*/)
:CDialog(CRuleSetEditor::IDD, pParent), 
 m_pParentWnd(pParent),
 m_apRuleTesterDlg(NULL), 
 m_strLastFileOpened(""),
 m_eContextMenuCtrl(kNoControl),
 m_strCurrentFileName(""),
 m_FRM(_Module.m_hInst, ".tmp"),
 m_iDESC_LIST_COLUMN(1),
 m_strBinFolder(strBinFolder),
 m_pMRUFilesMenu(NULL)
{
	try
	{
		//{{AFX_DATA_INIT(CRuleSetEditor)
		// Initialize the checkboxes
		m_bStopWhenValueFound = FALSE;
		m_bDocumentPP = FALSE;
		m_bInputValidator = FALSE;
		m_bAttSplitter = FALSE;
		m_bOutputHandler = FALSE;

		// Initialize the descriptions
		m_zPPDescription = _T("");
		m_zIVDescription = _T("");
		m_zAttributeSplitterDescription = _T("");
		m_zOutputHandlerDescription = _T("");
		//}}AFX_DATA_INIT

		// Note that LoadIcon does not require a subsequent DestroyIcon in Win32
		m_hIcon = AfxGetApp()->LoadIcon( IDI_ICON_DOC );

		// create an instance of the RuleSet object that can be used
		// for editing purposes for the lifetime of this dialog
		m_ipRuleSet.CreateInstance(CLSID_RuleSet);
		ASSERT_RESOURCE_ALLOCATION("ELI04495", m_ipRuleSet != NULL);

		// create an instance of the rule tester dialog
		m_apRuleTesterDlg = auto_ptr<RuleTesterDlg>(new RuleTesterDlg(&m_FRM, m_ipRuleSet, this));
		ASSERT_RESOURCE_ALLOCATION("ELI04815", m_apRuleTesterDlg.get() != NULL);

		// create an instance of the clipboard object manager
		m_ipClipboardMgr.CreateInstance(CLSID_ClipboardObjectManager);
		ASSERT_RESOURCE_ALLOCATION("ELI05554", m_ipClipboardMgr != NULL);
			
		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI07842", m_ipMiscUtils != NULL);

		ma_pUserCfgMgr = auto_ptr<IConfigurationSettingsPersistenceMgr>(
			new RegistryPersistenceMgr(HKEY_CURRENT_USER, gstrAF_AFCORE_KEY_PATH));
		
		ma_pMRUList = auto_ptr<MRUList>(new MRUList(ma_pUserCfgMgr.get(), "\\RuleSetEditor\\MRUList", "File_%d", 5));

		// display the wait cursor as it may take some time to
		// open the .RSD file if we do end up attempting such
		// an operation below.
		CWaitCursor waitCursor;
		
		// if a filename has been specified, try to open it
		// open the specified filename only if there is no
		// ruleset file to recover

		// if a ruleset recovery file exists, then ask the user if they
		// want to recover the ruleset
		string strRecoveryFileName;
		if (m_FRM.recoveryFileExists(strRecoveryFileName))
		{
			int iResult = MessageBox(gstrRECOVERY_PROMPT.c_str(), "Recovery", 
				MB_YESNO + MB_ICONQUESTION);

			if (iResult == IDYES)
			{
				// load the ruleset from the ruleset recovery file
				// NOTE: We are setting the bSetDirtyFlagToTrue flag to VARIANT_TRUE
				// so that the user is prompted for saving when they try
				// to close the RuleSet Editor window
				m_ipRuleSet->LoadFrom(get_bstr_t(
					strRecoveryFileName.c_str()), VARIANT_TRUE);
			}

			// at this point, regardless of whether the user decided to
			// recover the file or not, the recovery file should be deleted.
			// the recovery file can be deleted
			m_FRM.deleteRecoveryFile(strRecoveryFileName);
		}
		else if (strFileName != "")
		{
			// load the ruleset from the specified file
			m_ipRuleSet->LoadFrom(get_bstr_t(strFileName.c_str()), 
				VARIANT_FALSE);

			// update the current filename
			m_strCurrentFileName = strFileName;
		}
	
		// Create the associated IPersistStream interface
		m_ipRuleSetStream = m_ipRuleSet;

		// trim off trailing slash
		m_strBinFolder = ::trim(m_strBinFolder, "", "\\");
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI05074")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CRuleSetEditor)
	DDX_Control(pDX, IDC_BTN_SELECTPP, m_btnSelectPreprocessor);
	DDX_Control(pDX, IDC_BTN_SELECTIV, m_btnSelectIV);
	DDX_Control(pDX, IDC_BTN_SELECTOH, m_btnSelectOH);
	DDX_Control(pDX, IDC_BTN_ADDRULE, m_btnAddRule);
	DDX_Control(pDX, IDC_BTN_RULEUP, m_btnRuleUp);
	DDX_Control(pDX, IDC_BTN_RULEDOWN, m_btnRuleDown);
	DDX_Control(pDX, IDC_BTN_CONRULE, m_btnConRule);
	DDX_Control(pDX, IDC_BTN_DELRULE, m_btnDelRule);
	DDX_Control(pDX, IDC_LIST_RULES, m_listRules);
	DDX_Control(pDX, IDC_BTN_DELATTR, m_btnDelAttr);
	DDX_Control(pDX, IDC_BTN_RENATTR, m_btnRenAttr);
	DDX_Control(pDX, IDC_COMBO_ATTRIBUTES, m_comboAttr);
	DDX_Check(pDX, IDC_CHECK_STOP, m_bStopWhenValueFound);
	DDX_Check(pDX, IDC_CHECK_DOCUMENT_PP, m_bDocumentPP);
	DDX_Check(pDX, IDC_CHECK_INPUT_VALIDATOR, m_bInputValidator);
	DDX_Check(pDX, IDC_CHECK_ATT_SPLITTER, m_bAttSplitter);
	DDX_Check(pDX, IDC_CHECK_OUTPUT_HANDLER, m_bOutputHandler);
	DDX_Text(pDX, IDC_EDIT_PREPROCESSOR, m_zPPDescription);
	DDX_Text(pDX, IDC_EDIT_IV, m_zIVDescription);
	DDX_Text(pDX, IDC_EDIT_ATTRIBUTE_SPLITTER, m_zAttributeSplitterDescription);
	DDX_Text(pDX, IDC_EDIT_OH, m_zOutputHandlerDescription);
	//}}AFX_DATA_MAP
	DDX_Control(pDX, IDC_BTN_SELECT_ATTRIBUTE_SPLITTER, m_btnSelectAttributeSplitter);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CRuleSetEditor, CDialog)
	//{{AFX_MSG_MAP(CRuleSetEditor)
	ON_COMMAND(ID_FILE_NEW, OnFileNew)
	ON_COMMAND(ID_FILE_OPEN, OnFileOpen)
	ON_COMMAND(ID_FILE_SAVE, OnFileSave)
	ON_COMMAND(ID_FILE_SAVEAS, OnFileSaveas)
	ON_COMMAND(ID_FILE_EXIT, OnFileExit)
	ON_COMMAND(ID_HELP_ABOUT, OnHelpAbout)
	ON_COMMAND(ID_HELP_HELP, OnHelpHelp)
	ON_COMMAND(ID_TOOLS_CHECK, OnToolsCheck)
	ON_COMMAND(ID_TOOLS_TEST, OnToolsTest)
	ON_COMMAND(ID_TOOLS_HARNESS, OnToolsHarness)
	ON_BN_CLICKED(IDC_BTN_ADDATTR, OnBtnAddAttribute)
	ON_BN_CLICKED(IDC_BTN_DELATTR, OnBtnDeleteAttribute)
	ON_BN_CLICKED(IDC_BTN_RENATTR, OnBtnRenameAttribute)
	ON_BN_CLICKED(IDC_BTN_ADDRULE, OnBtnAddRule)
	ON_BN_CLICKED(IDC_BTN_DELRULE, OnBtnDeleteRule)
	ON_BN_CLICKED(IDC_BTN_CONRULE, OnBtnConfigureRule)
	ON_BN_CLICKED(IDC_BTN_RULEUP, OnBtnRuleUp)
	ON_BN_CLICKED(IDC_BTN_RULEDOWN, OnBtnRuleDown)
	ON_BN_CLICKED(IDC_BTN_SELECTIV, OnBtnSelectIV)
	ON_NOTIFY(NM_DBLCLK, IDC_LIST_RULES, OnDblclkListRules)
	ON_NOTIFY(NM_CLICK, IDC_LIST_RULES, OnClickListRules)
	ON_CBN_SELCHANGE(IDC_COMBO_ATTRIBUTES, OnSelchangeComboAttributes)
	ON_BN_CLICKED(IDC_CHECK_STOP, OnCheckStop)
	ON_WM_CLOSE()
	ON_COMMAND(ID_FILE_IMPORT, OnFileImport)
	ON_COMMAND(ID_FILE_EXPORT, OnFileExport)
	ON_WM_DROPFILES()
	ON_BN_CLICKED(IDC_BTN_SELECT_ATTRIBUTE_SPLITTER, OnBtnSelectAttributeSplitter)
	ON_NOTIFY(NM_RCLICK, IDC_LIST_RULES, OnRclickListRules)
	ON_COMMAND(ID_EDIT_CUT, OnEditCut)
	ON_COMMAND(ID_EDIT_COPY, OnEditCopy)
	ON_COMMAND(ID_EDIT_PASTE, OnEditPaste)
	ON_COMMAND(ID_EDIT_DELETE, OnEditDelete)
	ON_NOTIFY(NM_RCLICK, IDC_COMBO_ATTRIBUTES, OnRclickComboAttributes)
	ON_WM_MOUSEACTIVATE()
	ON_WM_SETCURSOR()
	ON_NOTIFY(NM_RCLICK, IDC_EDIT_IV, OnRclickValidatorText)
	ON_NOTIFY(NM_RCLICK, IDC_EDIT_ATTRIBUTE_SPLITTER, OnRclickSplitterText)
	ON_NOTIFY(NM_RCLICK, IDC_EDIT_PREPROCESSOR, OnRclickPreprocessorText)
	ON_WM_TIMER()
	ON_COMMAND(ID_EDIT_DUPLICATE, OnEditDuplicate)
	ON_BN_CLICKED(IDC_BTN_SELECTPP, OnBtnSelectPreprocessor)
	ON_NOTIFY(LVN_ITEMCHANGED, IDC_LIST_RULES, OnItemchangedListRules)
	ON_BN_CLICKED(IDC_BTN_SELECTOH, OnBtnSelectOutputHandler)
	ON_NOTIFY(NM_RCLICK, IDC_EDIT_OH, OnRclickHandlerText)
	ON_WM_SIZE()
	ON_COMMAND(ID_FILE_PROPERTIES, OnFileCounters)
	//}}AFX_MSG_MAP
	ON_COMMAND_RANGE(ID_MRU_FILE1, ID_MRU_FILE5, OnSelectMRUMenu)
	ON_BN_CLICKED(IDC_CHECK_DOCUMENT_PP, &CRuleSetEditor::OnBnClickedCheckDocumentPp)
	ON_BN_CLICKED(IDC_CHECK_INPUT_VALIDATOR, &CRuleSetEditor::OnBnClickedCheckInputValidator)
	ON_BN_CLICKED(IDC_CHECK_ATT_SPLITTER, &CRuleSetEditor::OnBnClickedCheckAttSplitter)
	ON_BN_CLICKED(IDC_CHECK_OUTPUT_HANDLER, &CRuleSetEditor::OnBnClickedCheckOutputHandler)
	ON_STN_DBLCLK(IDC_EDIT_OH, &CRuleSetEditor::OnDoubleClickOutputHandler)
	ON_STN_DBLCLK(IDC_EDIT_ATTRIBUTE_SPLITTER, &CRuleSetEditor::OnDoubleClickAttributeSplitter)
	ON_STN_DBLCLK(IDC_EDIT_IV, &CRuleSetEditor::OnDoubleClickInputValidator)
	ON_STN_DBLCLK(IDC_EDIT_PREPROCESSOR, &CRuleSetEditor::OnDoubleClickDocumentPreprocessor)
END_MESSAGE_MAP()

void CRuleSetEditor::clearListSelection()
{
	POSITION pos = m_listRules.GetFirstSelectedItemPosition();
	if (pos == NULL)
	{
		// no item selected, return
		return;
	}
	
	while (pos)
	{
		int nItemSelected = m_listRules.GetNextSelectedItem(pos);
		m_listRules.SetItemState(nItemSelected, 0, LVIS_SELECTED);
	}
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::processDroppedFile(char *pszFile) 
{
	// if the user dropped a uss or txt file, open the file in the
	// rule tester dialog
	string strExt = getExtensionFromFullPath(pszFile, true);
	if (strExt == ".rsd" || strExt == ".etf")
	{
		// if the CTRL key is pressed during a drag-and-drop, then
		// the user wants to import attributes from the file, otherwise
		// it is assumed that the user wants to just open the .RSD file
		// directly
		if (::isVirtKeyCurrentlyPressed(VK_CONTROL))
		{
			importFromFile(pszFile);
		}
		else
		{
			// open the .RSD file as if the user clicked on File-Open and
			// selected the .RSD file
			openFile(pszFile);
		}
	}
	else
	{
		// if the user dropped a file that's neither a RSD file 
		// nor an ETF file, then assume that the tester dialog is
		// supposed to handle the drop event

		// simulate opening the rule tester dialog from the menu
		OnToolsTest();

		// delegate the drop call to the tester dialog
		m_apRuleTesterDlg->processDroppedFile(pszFile);
	}
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::importFromFile(const string& strFileName) 
{
	// Create a vector of reserved attribute names
	vector<string> vecAttributes;
	int iCount = m_comboAttr.GetCount();
	CString	zAttribute;
	for (int j = 0; j < iCount; j++)
	{
		// Retrieve this attribute
		m_comboAttr.GetLBText( j, zAttribute );

		// Add this attribute to the vector
		vecAttributes.push_back( string( zAttribute.operator LPCTSTR() ) );
	}

	// Create a temporary ruleset object
	UCLID_AFCORELib::IRuleSetPtr ipImportRuleSet( CLSID_RuleSet );
	ASSERT_RESOURCE_ALLOCATION( "ELI05060", ipImportRuleSet != NULL );

	// verify extension is RSD
	string strExt = getExtensionFromFullPath( strFileName );
	makeUpperCase( strExt );
	if ( strExt != ".RSD" )
	{
		throw UCLIDException("ELI07656", "File is not an RSD file.");
	}

	// Load the ruleset object from the specified file 
	_bstr_t bstrFileName = get_bstr_t( strFileName.c_str() );
	ipImportRuleSet->LoadFrom( bstrFileName, VARIANT_FALSE );

	// Create and display the Import dialog
	CImportRuleSetDlg	dlgImport( ipImportRuleSet, vecAttributes, true );
	int iReturn = dlgImport.DoModal();
	if (iReturn == IDOK)
	{
		// Retrieve map of attribute names and attribute infos
		// to be imported
		IStrToObjectMapPtr ipImportMap;
		ipImportMap = ipImportRuleSet->AttributeNameToInfoMap;
		ASSERT_RESOURCE_ALLOCATION( "ELI05017", ipImportMap != NULL );

		// Add the selected rules to this rule set
		IVariantVectorPtr ipKeys = ipImportMap->GetKeys();
		long lNumKeys = ipKeys->Size;
		for (int i = 0; i < lNumKeys; i++)
		{
			// Retrieve the attribute name
			_bstr_t bstrName = ipKeys->GetItem( i );

			// Check for an attribute with the specified name already in the map
			if (m_ipAttributeNameToInfoMap->Contains( bstrName ))
			{
				// Throw exception indicating that attribute already exists
				UCLIDException ue( "ELI05018", "Specified attribute is already in the list!" );
				ue.addDebugInfo( "Attribute name", bstrName.operator const char *() );
				throw ue;
			}

			// Retrieve the associated AttributeFindInfo object
			UCLID_AFCORELib::IAttributeFindInfoPtr ipInfo = ipImportMap->GetValue( bstrName );
			ASSERT_RESOURCE_ALLOCATION( "ELI05019", ipInfo != NULL );

			// Add the Name and Info pair to the map
			m_ipAttributeNameToInfoMap->Set( bstrName, ipInfo );

			// Add the Attribute Name to the combo box
			int iIndex = m_comboAttr.AddString( bstrName.operator const char *() );
		}

		// Refresh the UI
		refreshUIFromRuleSet();
	}
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::openFile(string strFileName) 
{
	try
	{
		// if checkModification() returns false, it means the user
		// want to cancel current task, i.e. opening another file
		if (!checkModification()) return;
		
		if (strFileName == "")
		{
			// ask user to select file to load
			CFileDialogEx fileDlg(TRUE, ".rsd;.etf", m_strLastFileOpened.c_str(), 
				OFN_ENABLESIZING | OFN_EXPLORER | OFN_FILEMUSTEXIST | OFN_HIDEREADONLY | OFN_PATHMUSTEXIST,
				gstrRSD_FILE_OPEN_FILTER.c_str(), this);
			
			if (fileDlg.DoModal() != IDOK)
			{
				return;
			}
			
			strFileName = (LPCTSTR) fileDlg.GetPathName();
		}

		CWaitCursor wait;
		
		// make sure the file exists
		::validateFileOrFolderExistence(strFileName);

		// verify extension is RSD
		string strExt = getExtensionFromFullPath( strFileName, true );
		if (( strExt != ".rsd" ) && ( strExt != ".etf" ))
		{
			throw UCLIDException("ELI07658", "File is not an RSD file.");
		}
		
		// load the ruleset object from the specified file
		_bstr_t bstrFileName = get_bstr_t(strFileName.c_str());
		m_ipRuleSet->LoadFrom(bstrFileName, VARIANT_FALSE);
		
		// add the file to MRU list
		addFileToMRUList(strFileName);
		
		refreshUIFromRuleSet();
		
		// update the caption of the window to contain the filename
		m_strLastFileOpened = bstrFileName;
		m_strCurrentFileName = m_strLastFileOpened;
		updateWindowCaption();
		setStatusBarText();
	}
	catch (...)
	{
		removeFileFromMRUList(strFileName);
		throw;
	}
}
//-------------------------------------------------------------------------------------------------
bool CRuleSetEditor::checkModification()
{
	// Check modified state, prompt the user to save changes, etc.
	if (m_ipRuleSet)
	{
		IPersistStreamPtr ipPersistStream(m_ipRuleSet);
		if (ipPersistStream)
		{
			// if the rule set is modified, prompt for saving
			if (ipPersistStream->IsDirty() == S_OK)
			{
				int nRes = MessageBox("Current Rule Set has been modified. Do you wish to save the changes?", 
					"Save Changes?", MB_YESNOCANCEL);
				if (nRes == IDCANCEL)
				{
					// user wants to cancel the action, 
					// do not continue with any further action
					return false;
				}
				
				if (nRes == IDYES)
				{
					// save the changes
					OnFileSave();

					// Check for Cancel from Save dialog
					if (ipPersistStream->IsDirty() == S_OK)
					{
						// do not continue with any further action
						return false;
					}
				}
			}
		}
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::enableEditFeatures(bool bEnable) 
{
	// populate IDs of UI controls that must only be enabled in edit mode
	static vector<long> vecRSDControlIDs;
	if (vecRSDControlIDs.empty())
	{
		vecRSDControlIDs.push_back(IDC_EDIT_PREPROCESSOR);
		vecRSDControlIDs.push_back(IDC_BTN_SELECTPP);
		vecRSDControlIDs.push_back(IDC_STATIC_PP);
		vecRSDControlIDs.push_back(IDC_STATIC_ATTR);
		vecRSDControlIDs.push_back(IDC_BTN_ADDATTR);
		vecRSDControlIDs.push_back(IDC_BTN_DELATTR);
		vecRSDControlIDs.push_back(IDC_BTN_RENATTR);
		vecRSDControlIDs.push_back(IDC_COMBO_ATTRIBUTES);
		vecRSDControlIDs.push_back(IDC_STATIC_RULES);
		vecRSDControlIDs.push_back(IDC_LIST_RULES);
		vecRSDControlIDs.push_back(IDC_BTN_ADDRULE);
		vecRSDControlIDs.push_back(IDC_BTN_DELRULE);
		vecRSDControlIDs.push_back(IDC_BTN_CONRULE);
		vecRSDControlIDs.push_back(IDC_BTN_RULEUP);
		vecRSDControlIDs.push_back(IDC_BTN_RULEDOWN);
		vecRSDControlIDs.push_back(IDC_CHECK_STOP);
		vecRSDControlIDs.push_back(IDC_STATIC_IV);
		vecRSDControlIDs.push_back(IDC_EDIT_IV);
		vecRSDControlIDs.push_back(IDC_BTN_SELECTIV);
		vecRSDControlIDs.push_back(IDC_STATIC_SPLIT);
		vecRSDControlIDs.push_back(IDC_EDIT_ATTRIBUTE_SPLITTER);
		vecRSDControlIDs.push_back(IDC_BTN_SELECT_ATTRIBUTE_SPLITTER);
		vecRSDControlIDs.push_back(IDC_EDIT_OH);
		vecRSDControlIDs.push_back(IDC_BTN_SELECTOH);
		vecRSDControlIDs.push_back(IDC_STATIC_OH);
		vecRSDControlIDs.push_back(IDC_CHECK_DOCUMENT_PP);
		vecRSDControlIDs.push_back(IDC_CHECK_INPUT_VALIDATOR);
		vecRSDControlIDs.push_back(IDC_CHECK_ATT_SPLITTER);
		vecRSDControlIDs.push_back(IDC_CHECK_OUTPUT_HANDLER);
	}
	
	// Show/Hide the controls that should only be shown in edit-mode
	vector<long>::const_iterator iter;
	for (iter = vecRSDControlIDs.begin(); iter != vecRSDControlIDs.end(); iter++)
	{
		long nID = *iter;
		CWnd*	pWnd = GetDlgItem( nID );
		if (pWnd != NULL)
		{
			pWnd->ShowWindow( bEnable ? SW_SHOW : SW_HIDE );
		}
	}

	/////////////////////////////////
	// Hide/show the encrypted prompt
	/////////////////////////////////
	CWnd *pWnd = GetDlgItem( IDC_STATIC_PROMPT );
	if (pWnd != NULL)
	{
		pWnd->ShowWindow( bEnable ? SW_HIDE : SW_SHOW );
	}

	// populate IDs of menu items that must only be enabled in edit mode
	static vector<long> vecRSDMenuIDs;
	if (vecRSDMenuIDs.empty())
	{
		vecRSDMenuIDs.push_back(ID_FILE_IMPORT);
		vecRSDMenuIDs.push_back(ID_FILE_EXPORT);
		vecRSDMenuIDs.push_back(ID_FILE_SAVE);
		vecRSDMenuIDs.push_back(ID_FILE_SAVEAS);
		vecRSDMenuIDs.push_back(ID_FILE_PROPERTIES);
	}

	CMenu* pMainMenu = GetMenu();
	// Enable/disable the menu items that should only be enabled in edit-mode
	for (iter = vecRSDMenuIDs.begin(); iter != vecRSDMenuIDs.end(); iter++)
	{
		long nID = *iter;

		pMainMenu->EnableMenuItem( nID, 
			bEnable ? MF_BYCOMMAND | MF_ENABLED : MF_BYCOMMAND | MF_GRAYED );
	}
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::prepareList() 
{
	// Enable full row selection plus grid lines and checkboxes
	m_listRules.SetExtendedStyle( 
		LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT | LVS_EX_CHECKBOXES );

	//////////////////
	// Prepare headers
	//////////////////
	// Get dimensions of control
	CRect	rect;
	m_listRules.GetClientRect( &rect );
	
	// Retrieve fixed column widths
	long	lEWidth = 55;

	// Compute width for Description column
	CRect	rectList;
	m_listRules.GetClientRect( rectList );
	long	lDWidth = rectList.Width() - lEWidth;
	
	// Add 2 column headings to Rules list
	// It would be nice to center the left column's checkbox, 
	// however, in the help it specifies that the first column
	// must be left aligned. Using LVCFMT_CENTER has no effect.
	m_listRules.InsertColumn( giENABLE_LIST_COLUMN, "Enabled", 
		LVCFMT_LEFT, lEWidth, giENABLE_LIST_COLUMN );
	m_listRules.InsertColumn( m_iDESC_LIST_COLUMN, "Description", 
		LVCFMT_LEFT, lDWidth, m_iDESC_LIST_COLUMN );
}
//-------------------------------------------------------------------------------------------------
bool CRuleSetEditor::promptForAttributeName(PromptDlg& promptDlg)
{
	while (true)
	{
		// Check result from modal dialog
		if (promptDlg.DoModal() == IDOK)
		{
			string strInput = (LPCTSTR)promptDlg.m_zInput;

			if (strInput.empty())
			{
				MessageBox("Please provide non-empty Attribute name.", "Attribute Name", MB_OK);
				continue;
			}

			// ensure that the name is valid by putting on a test Attribute
			try
			{
				try
				{
					
					UCLID_AFCORELib::IAttributePtr ipTmp(CLSID_Attribute);
					ASSERT_RESOURCE_ALLOCATION("ELI09510", ipTmp != NULL);
					ipTmp->PutName( get_bstr_t(strInput) );
				}
				CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI09511");
			}
			catch(...)
			{
				continue;
			}

			return true;
		}

		break;
	}

	return false;
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::updateToolsMenu()
{
	///////////////////////////////
	// Find Test Harness executable
	///////////////////////////////

	// Get module directory
	m_strTestHarnessPath = m_strBinFolder;

	// Append EXE name
	if (!m_strTestHarnessPath.empty())
	{
		m_strTestHarnessPath += "\\TestHarness.exe";

		// Search for the EXE file
		if (::isFileOrFolderValid(m_strTestHarnessPath))
		{
			// Load the Tools submenu
			CMenu menu;
			menu.LoadMenu( IDR_MNU_EDITOR );
			CMenu *pMenu = menu.GetSubMenu( 1 );
			
			// Add an item and a separator for Execute Test Harness
			pMenu->InsertMenu( ID_TOOLS_CHECK, MF_BYCOMMAND | MF_STRING, 
				ID_TOOLS_HARNESS, "E&xecute Test Harness" );
			pMenu->InsertMenu( ID_TOOLS_CHECK, MF_BYCOMMAND | MF_SEPARATOR, 0, "" );
			
			// Associate the modified menu with the window
			SetMenu( &menu );
			menu.Detach();
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::updateWindowCaption()
{
	const string strWINDOW_TITLE = "RuleSet Editor";
	
	// compute the window caption depending upon the name of the
	// currently loaded file, if any
	string strResult;
	if (!m_strCurrentFileName.empty())
	{
		// if a file is currently loaded, then only display the filename and
		// not the full path.
		strResult = getFileNameFromFullPath(m_strCurrentFileName);
		strResult += " - ";
		strResult += strWINDOW_TITLE;
	}
	else
	{
		strResult = strWINDOW_TITLE;
	}

	// update the window caption
	SetWindowText(strResult.c_str());
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::refreshUIFromAttribute()
{
	/////////////////////////////////
	// Update list of Attribute Rules
	/////////////////////////////////
	// Clear the list
	m_listRules.DeleteAllItems();

	// Retrieve vector of Attribute Rules
	if (m_ipInfo)
	{
		IIUnknownVectorPtr	ipRules = m_ipInfo->GetAttributeRules();
		if (ipRules == NULL)
		{
			// Create and throw exception
			throw UCLIDException("ELI04584", "Unable to retrieve Attribute Rules!");
		}

		// Add each item from vector to list
		CString	zText;
		int iCount = ipRules->Size();
		for (int i = 0; i < iCount; i++)
		{
			// Retrieve this rule
			UCLID_AFCORELib::IAttributeRulePtr	ipRule = ipRules->At( i );
			if (ipRule != NULL)
			{
				// Retrieve the Rule description
				_bstr_t	bstrDescription = ipRule->GetDescription();

				// Retrieve the Enabled flag
				VARIANT_BOOL	vbEnabled = ipRule->GetIsEnabled();

				// Add the item to the list
				m_listRules.InsertItem( i, "" );
				m_listRules.SetItemText( i, m_iDESC_LIST_COLUMN, 
					bstrDescription.operator const char *() );

				m_listRules.SetCheck(i, vbEnabled == VARIANT_TRUE);
			}
		}
	}

	// Set stop searching flag
	if (m_ipInfo)
	{
		VARIANT_BOOL	vbStop = m_ipInfo->GetStopSearchingWhenValueFound();
		m_bStopWhenValueFound = asMFCBool(vbStop);

		// Retrieve Input Validator
		IObjectWithDescriptionPtr ipIV = m_ipInfo->InputValidator;
		ASSERT_RESOURCE_ALLOCATION("ELI15505", ipIV != NULL);

		// Update Input Validator description
		_bstr_t	bstrIVDesc = ipIV->Description;
		m_zIVDescription = bstrIVDesc.operator const char *();

		// Set the IV checkbox
		// Make sure that there is an input validator present
		if( m_zIVDescription.IsEmpty() )
		{
			// if there isnt an IV, disable the checkbox
			GetDlgItem( IDC_CHECK_INPUT_VALIDATOR )->EnableWindow( FALSE );
			m_bInputValidator = FALSE;
		}
		else
		{	// Enable the checkbox
			GetDlgItem( IDC_CHECK_INPUT_VALIDATOR )->EnableWindow( TRUE );

			// then set the checkbox value accordingly
			if( ipIV->GetEnabled() == VARIANT_TRUE )
			{
				m_bInputValidator = TRUE;
			}
			else
			{
				m_bInputValidator = FALSE;
			}
		}

		// Retrieve Attribute Splitter
		IObjectWithDescriptionPtr ipSplitter = m_ipInfo->AttributeSplitter;
		ASSERT_RESOURCE_ALLOCATION("ELI15506", ipSplitter != NULL);

		// Update AttributeSplitter description
		_bstr_t	bstrDesc = ipSplitter->Description;
		m_zAttributeSplitterDescription = (char *) bstrDesc;

		// Update the AS checkbox
		// Make sure there is an attribute splitter present
		if( m_zAttributeSplitterDescription.IsEmpty() )
		{
			// if there is no AS, disable the checkbox
			GetDlgItem( IDC_CHECK_ATT_SPLITTER )->EnableWindow( FALSE );
			m_bAttSplitter = FALSE;
		}
		else
		{
			// if an AS exists, enable the checkbox
			GetDlgItem( IDC_CHECK_ATT_SPLITTER )->EnableWindow( TRUE );

			// then set the value accordingly
			if( ipSplitter->GetEnabled() == VARIANT_TRUE )
			{
				m_bAttSplitter = TRUE;
			}
			else
			{
				m_bAttSplitter = FALSE;
			}
		}
	}
	else
	{
		m_bStopWhenValueFound = FALSE;
		m_zIVDescription = "";
		m_zAttributeSplitterDescription = "";

		// Disable the checkboxes and clear the checkmark.
		GetDlgItem( IDC_CHECK_INPUT_VALIDATOR )->EnableWindow( FALSE );
		m_bInputValidator = FALSE;
		GetDlgItem( IDC_CHECK_ATT_SPLITTER )->EnableWindow( FALSE );
		m_bAttSplitter = FALSE;
	}

	UpdateData(FALSE);

	// Update button states
	setButtonStates();
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::refreshUIFromRuleSet()
{
	// Enable or disable UI elements based on encryption state of Rule Set
	if (m_ipRuleSet->GetIsEncrypted() == VARIANT_TRUE)
	{
		enableEditFeatures(false);
		return;
	}
		
	enableEditFeatures(true);

	// Retrieve Preprocessor
	IObjectWithDescriptionPtr ipGDPP = m_ipRuleSet->GlobalDocPreprocessor;
	ASSERT_RESOURCE_ALLOCATION("ELI15507", ipGDPP != NULL);

	// Update Document Preprocessor description
	_bstr_t	bstrPreDesc = ipGDPP->Description;
	m_zPPDescription = (char *) bstrPreDesc;

	// Update Doc Preprocessor checkbox 
	// check if there is a Doc PP present
	if( m_zPPDescription.IsEmpty() )
	{
		//if not, disable the checkbox
		GetDlgItem( IDC_CHECK_DOCUMENT_PP )->EnableWindow( FALSE );
		m_bDocumentPP = FALSE;
	}
	else
	{
		// if there is a DocumentPP present,
		// enable the checkbox and set the value accordingly
		GetDlgItem( IDC_CHECK_DOCUMENT_PP )->EnableWindow( TRUE );
		if( ipGDPP->GetEnabled() == VARIANT_TRUE )
		{
			m_bDocumentPP = TRUE;
		}
		else
		{
			m_bDocumentPP = FALSE;
		}
	}

	// Retrieve Output Handler
	IObjectWithDescriptionPtr ipOH = m_ipRuleSet->GlobalOutputHandler;
	ASSERT_RESOURCE_ALLOCATION("ELI15508", ipOH != NULL);

	// Update Output Handler description
	_bstr_t	bstrOHDesc = ipOH->Description;
	m_zOutputHandlerDescription = (char *) bstrOHDesc;

	// Update Output Handler checkbox
	// check is an Output Handler exists
	if( m_zOutputHandlerDescription.IsEmpty() )
	{
		// if there is not an OH, disable the checkbox
		GetDlgItem( IDC_CHECK_OUTPUT_HANDLER )->EnableWindow( FALSE );
		m_bOutputHandler = FALSE;
	}
	else
	{
		// if there is an Output Handler present,
		// enable the checkbox and set the value accordingly
		GetDlgItem( IDC_CHECK_OUTPUT_HANDLER )->EnableWindow( TRUE );
		if( ipOH->GetEnabled() == VARIANT_TRUE )
		{
			m_bOutputHandler = TRUE;
		}
		else
		{
			m_bOutputHandler = FALSE;
		}
	}

	// because we keep accessing the attribute name to attribute-find-info map
	// many times during the scope of this window, keep cache a pointer to this map
	m_ipAttributeNameToInfoMap = m_ipRuleSet->AttributeNameToInfoMap;
	ASSERT_RESOURCE_ALLOCATION("ELI04543", m_ipAttributeNameToInfoMap != NULL);

	// clear all the UI controls or bring them back to their default state
	m_comboAttr.ResetContent();

	// refresh the UI from the ruleset object
	// first refresh the attributes list
	IVariantVectorPtr ipKeys = m_ipAttributeNameToInfoMap->GetKeys();
	ASSERT_RESOURCE_ALLOCATION("ELI15509", ipKeys != NULL);
	long nNumkeys = ipKeys->Size;
	for (int i = 0; i < nNumkeys; i++)
	{
		_bstr_t _bstrKeyName = ipKeys->GetItem(i);
		m_comboAttr.AddString(_bstrKeyName);
	}

	// if the combo box of attributes has 1 or more entry, select the first
	// entry
	if (nNumkeys > 0)
	{
		m_comboAttr.SetCurSel(0);

		CString	zAttribute;
		m_comboAttr.GetLBText( 0, zAttribute );
		_bstr_t	bstrAttribute( zAttribute.operator LPCTSTR() );

		// let the Rule Tester dialog know what the current attribute is
		m_apRuleTesterDlg->setCurrentAttributeName((LPCTSTR) zAttribute);

		// Get AttributeFindInfo object for this attribute
		m_ipInfo = m_ipAttributeNameToInfoMap->GetValue( bstrAttribute );
	}
	else
	{
		// let the Rule Tester dialog know what the current attribute is
		m_apRuleTesterDlg->setCurrentAttributeName("");

		m_ipInfo = NULL;
	}

	// Update Status Bar Text
	setStatusBarText();

	// Update other controls based on this Attribute
	refreshUIFromAttribute();
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::setButtonStates()
{
	// Check count in Attributes combo box
	CButton*	pButton;
	int	iCount = m_comboAttr.GetCount();
	if (iCount == 0)
	{
		////////////////////////////
		// Disable almost everything
		////////////////////////////
		// Disable Delete and Rename buttons
		m_btnDelAttr.EnableWindow( FALSE );
		m_btnRenAttr.EnableWindow( FALSE );

		// Disable Rule buttons and list
		m_btnAddRule.EnableWindow( FALSE );
		m_btnDelRule.EnableWindow( FALSE );
		m_btnConRule.EnableWindow( FALSE );
		m_btnRuleUp.EnableWindow( FALSE );
		m_btnRuleDown.EnableWindow( FALSE );
		m_listRules.EnableWindow( FALSE );

		// Disable check box
		pButton = (CButton *)GetDlgItem( IDC_CHECK_STOP );
		if (pButton != NULL)
		{
			pButton->EnableWindow( FALSE );
		}

		// Disable Select IV button
		m_btnSelectIV.EnableWindow( FALSE );
		
		// Disable the button associated with the attribute splitter
		m_btnSelectAttributeSplitter.EnableWindow( FALSE );

		// Everything except an Add button is disabled now, just return
		return;
	}
	else
	{
		// Enable Delete and Rename buttons
		m_btnDelAttr.EnableWindow( TRUE );
		m_btnRenAttr.EnableWindow( TRUE );

		// Enable Add button
		m_btnAddRule.EnableWindow( TRUE );

		// Enable list
		m_listRules.EnableWindow( TRUE );

		// Enable Select IV button
		m_btnSelectIV.EnableWindow( TRUE );

		// Enable the button associated with the attribute splitter
		m_btnSelectAttributeSplitter.EnableWindow( TRUE );
	}

	// Check count in Rules list
	POSITION pos = NULL;
	int iIndex = -1;
	iCount = m_listRules.GetItemCount();
	if (iCount == 0)
	{
		// Disable most buttons
		m_btnDelRule.EnableWindow( FALSE );
		m_btnConRule.EnableWindow( FALSE );
		m_btnRuleUp.EnableWindow( FALSE );
		m_btnRuleDown.EnableWindow( FALSE );
	}
	else
	{
		// Must be more than one rule for the checkbox
		pButton = (CButton *)GetDlgItem(IDC_CHECK_STOP);
		ASSERT_RESOURCE_ALLOCATION("ELI07395", pButton != NULL);
		pButton->EnableWindow( asMFCBool(iCount > 1) );

		// Next have to see if an item is selected
		pos = m_listRules.GetFirstSelectedItemPosition();
		if (pos != NULL)
		{
			// Get index of first selection
			iIndex = m_listRules.GetNextSelectedItem( pos );
		}

		if (iIndex > -1)
		{
			// Enable the Delete button
			m_btnDelRule.EnableWindow( TRUE );

			// Check for multiple selection
			int iIndex2 = m_listRules.GetNextSelectedItem( pos );
			if (iIndex2 == -1)
			{
				// Only one Rule selected, enable Configure
				m_btnConRule.EnableWindow( TRUE );

				// Must be more than one rule for these buttons
				if (iCount > 1)
				{
					// Cannot move last item down
					m_btnRuleDown.EnableWindow( asMFCBool(iIndex != iCount - 1) );
					// Cannot move first item up
					m_btnRuleUp.EnableWindow( asMFCBool(iIndex != 0) );
				}
				else
				{
					// Cannot change order if only one rule
					m_btnRuleUp.EnableWindow( FALSE );
					m_btnRuleDown.EnableWindow( FALSE );
				}
			}
			else
			{
				// More than one Rule is selected, disable other buttons
				m_btnConRule.EnableWindow( FALSE );
				m_btnRuleUp.EnableWindow( FALSE );
				m_btnRuleDown.EnableWindow( FALSE );
			}
		}
		else
		{
			// No selection --> disable most buttons
			m_btnDelRule.EnableWindow( FALSE );
			m_btnConRule.EnableWindow( FALSE );
			m_btnRuleUp.EnableWindow( FALSE );
			m_btnRuleDown.EnableWindow( FALSE );
		}
	}
}	
//-------------------------------------------------------------------------------------------------
bool CRuleSetEditor::clipboardObjectIsAttribute()
{
	bool bResult = false;

	// get the object from the clipboard
	IUnknownPtr ipObj = m_ipClipboardMgr->GetObjectInClipboard();

	// Object must be defined
	if (ipObj != NULL)
	{
		// Check to see if object is IStrToObjectMap
		UCLID_COMUTILSLib::IStrToObjectMapPtr ipMap = ipObj;
		if (ipMap != NULL)
		{
			// Get map size
			long lMapSize = ipMap->GetSize();

			// Loop through map items
			IUnknownPtr	ipItem;
			bool		bIsInfo = true;
			for (int i = 0; i < lMapSize; i++)
			{
				CComBSTR bstrKey;
				// Retrieve this item
				ipMap->GetKeyValue( i, &bstrKey, &ipItem );
				if (ipItem != NULL)
				{
					// Check to see if this item is an IAttributeFindInfo object
					UCLID_AFCORELib::IAttributeFindInfoPtr ipInfo = ipItem;
					if (ipInfo == NULL)
					{
						bIsInfo = false;
						break;
					}
				}				// end if map item != NULL
			}					// end for each map item

			// Check search result
			if (bIsInfo)
			{
				bResult = true;
			}
		}						// end if map != NULL
	}							// end if data member != NULL

	return bResult;
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::deleteMarkedRules() 
{
	// Get list count
	int iCount = m_listRules.GetItemCount();
	if (iCount == 0)
	{
		return;
	}

	// Retrieve existing vector
	IIUnknownVectorPtr	ipRules = m_ipInfo->GetAttributeRules();
	if (ipRules == NULL)
	{
		// Throw exception, unable to retrieve Attribute Rules
		throw UCLIDException( "ELI05571", "Unable to retrieve Attribute Rules!" );
	}

	// Step backwards through list
	for (int i = iCount - 1; i >= 0; i--)
	{
		// Retrieve ItemData and look for "mark"
		DWORD	dwData = m_listRules.GetItemData( i );
		if (dwData == 1)
		{
			// Remove this item from list
			m_listRules.DeleteItem( i );

			// Remove this item from the vector of Rules
			ipRules->Remove( i );
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::markSelectedRules() 
{
	POSITION pos = m_listRules.GetFirstSelectedItemPosition();
	if (pos != NULL)
	{
		// Get index of first selection
		int iIndex = m_listRules.GetNextSelectedItem( pos );

		// Loop through selected items
		while (iIndex != -1)
		{
			// Set ItemData = 1 as a "mark"
			m_listRules.SetItemData( iIndex, (DWORD) 1 );

			// Get index of next selected item
			iIndex = m_listRules.GetNextSelectedItem( pos );
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::refreshFileMRU()
{
	// if this is a initialization call, i.e. the dialog is
	// just created, then we need to read MRU from Registry
	ma_pMRUList->readFromPersistentStore();
	int nListSize = ma_pMRUList->getCurrentListSize();
	if (nListSize == 0)
	{
		return;
	}

	if (m_pMRUFilesMenu == NULL)
	{
		CMenu* pMainMenu = GetMenu();
		CMenu* pFileMenu = pMainMenu->GetSubMenu(0);
		m_pMRUFilesMenu = pFileMenu->GetSubMenu(9);
		if (m_pMRUFilesMenu == NULL)
		{
			throw UCLIDException("ELI09645", "Unable to get MRU File menu.");
		}
		// remove the "No File" item from the menu
		m_pMRUFilesMenu->RemoveMenu(ID_FILE_MRU, MF_BYCOMMAND);
	}
	
	// get total number of items currently on the menu
	int nTotalItems = m_pMRUFilesMenu->GetMenuItemCount();
	int n = 0;
	for (n = 0; n < nListSize; n++)
	{
		// if the file item already exists on the menu, just modify the file name
		if (nTotalItems > 0 && n < nTotalItems)
		{
			m_pMRUFilesMenu->ModifyMenu(n, MF_BYPOSITION, ID_MRU_FILE1 + n, ma_pMRUList->at(n).c_str());
			continue;
		}

		m_pMRUFilesMenu->InsertMenu(-1, MF_BYCOMMAND, ID_MRU_FILE1 + n, ma_pMRUList->at(n).c_str());
	}

	// if total number of items on the menu exceeds the 
	// number of entries from the Registry, remove the unnecessary one(s)
	if (nTotalItems > nListSize)
	{
		for (n = nListSize; n < nTotalItems; n++)
		{
			m_pMRUFilesMenu->RemoveMenu(ID_MRU_FILE1+n, MF_BYCOMMAND);
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::addFileToMRUList(const string& strFileToBeAdded)
{
	// we wish to have updated items all the time
	ma_pMRUList->readFromPersistentStore();
	ma_pMRUList->addItem(strFileToBeAdded);
	ma_pMRUList->writeToPersistentStore();
	
	refreshFileMRU();
}
//--------------------------------------------------------------------------------------------------
void CRuleSetEditor::removeFileFromMRUList(const string& strFileToBeRemoved)
{
	// remove the bad file from MRU List
	ma_pMRUList->readFromPersistentStore();
	ma_pMRUList->removeItem(strFileToBeRemoved);
	ma_pMRUList->writeToPersistentStore();

	refreshFileMRU();
}
//--------------------------------------------------------------------------------------------------
void CRuleSetEditor::setStatusBarText()
{
	string strSelectedCounters = "";
	bool bValidLicensing = true;

	if ( m_ipRuleSet->UseIndexingCounter == VARIANT_TRUE )
	{
		strSelectedCounters = "Indexing";

		// Requires special licensing - FLEX Index Rule Writing license
		if (!LicenseManagement::sGetInstance().isLicensed( gnFLEXINDEX_RULE_WRITING_OBJECTS ))
		{
			bValidLicensing = false;
		}
	}

	if ( m_ipRuleSet->UsePaginationCounter == VARIANT_TRUE )
	{
		if ( strSelectedCounters != "" )
		{
			strSelectedCounters = strSelectedCounters + ", ";
		}

		strSelectedCounters = strSelectedCounters + "Pagination";

		// Requires special licensing - full RDT license
		if (!LicenseManagement::sGetInstance().isLicensed( gnRULE_DEVELOPMENT_TOOLKIT_OBJECTS ))
		{
			bValidLicensing = false;
		}
	}

	if ( m_ipRuleSet->UsePagesRedactionCounter == VARIANT_TRUE )
	{
		if ( strSelectedCounters != "" )
		{
			strSelectedCounters = strSelectedCounters + ", ";
		}

		strSelectedCounters = strSelectedCounters + "Redaction (pages)";

		// Requires special licensing - ID Shield Rule Writing license
		if (!LicenseManagement::sGetInstance().isLicensed( gnIDSHIELD_RULE_WRITING_OBJECTS ))
		{
			bValidLicensing = false;
		}
	}

	if ( m_ipRuleSet->UseDocsRedactionCounter == VARIANT_TRUE )
	{
		if ( strSelectedCounters != "" )
		{
			strSelectedCounters = strSelectedCounters + ", ";
		}

		strSelectedCounters = strSelectedCounters + "Redaction (docs)";

		// Requires special licensing - full RDT license
		if (!LicenseManagement::sGetInstance().isLicensed( gnRULE_DEVELOPMENT_TOOLKIT_OBJECTS ))
		{
			bValidLicensing = false;
		}
	}

	// Make sure that counter requirements and licensing are valid
	if (!bValidLicensing)
	{
		// Create and retain a new ruleset object
		UCLID_AFCORELib::IRuleSetPtr ipRuleSet(CLSID_RuleSet);
		ASSERT_RESOURCE_ALLOCATION("ELI21506", ipRuleSet != NULL);
		m_ipRuleSet = ipRuleSet;

		// Reset the UI
		refreshUIFromRuleSet();

		// Create and throw exception - invalid combination
		UCLIDException ue( "ELI21502", "Invalid USB counter and licensing combination!" );
		ue.addDebugInfo( "Selected Counters", strSelectedCounters );
		ue.addDebugInfo( "FLEX Index Rule Writing", LicenseManagement::sGetInstance().isLicensed( 
			gnFLEXINDEX_RULE_WRITING_OBJECTS ) ? "1" : "0" );
		ue.addDebugInfo( "ID Shield Rule Writing", LicenseManagement::sGetInstance().isLicensed( 
			gnIDSHIELD_RULE_WRITING_OBJECTS ) ? "1" : "0" );
		throw ue;
	}

	// Set status bar text to NONE if appropriate
	if ( strSelectedCounters == "" )
	{
		strSelectedCounters = "NONE";
	}

	int iWidth;
	CRect clientRect;
	GetClientRect(clientRect);

	// Set width of counters status to 1/2 width of Client area
	iWidth = clientRect.Width() / 2;
	string strStatusBarText = "Counters: " + strSelectedCounters;
	m_statusBar.SetPaneText(m_statusBar.CommandToIndex(ID_INDICATOR_COUNTERS),
		strStatusBarText.c_str());
	m_statusBar.SetPaneInfo(m_statusBar.CommandToIndex(ID_INDICATOR_COUNTERS), 
		ID_INDICATOR_COUNTERS, SBPS_NORMAL, iWidth );
	
	// Set the width of the serial numbers to 1/6 width of Client area
	iWidth = iWidth / 3;
	string strSerials = m_ipRuleSet->KeySerialList;
	strStatusBarText = "SNs: " +  strSerials;
	m_statusBar.SetPaneText(m_statusBar.CommandToIndex(ID_INDICATOR_SERIAL_NUMBERS),
		strStatusBarText.c_str());
	m_statusBar.SetPaneInfo(m_statusBar.CommandToIndex(ID_INDICATOR_SERIAL_NUMBERS), 
		ID_INDICATOR_SERIAL_NUMBERS, SBPS_NORMAL, iWidth );
	
	// Set the width of the ForInternalUseOnly to 1/3 width of Client area
	iWidth = iWidth * 2;
	// Update the internal-use-only flag in the status bar
	strStatusBarText = "InternalUseOnly = ";
	strStatusBarText += (m_ipRuleSet->ForInternalUseOnly == VARIANT_TRUE) ?
		"Yes" : "No";
	
	m_statusBar.SetPaneText(m_statusBar.CommandToIndex(ID_INDICATOR_INTERNAL_USE_ONLY),
		strStatusBarText.c_str());
	m_statusBar.SetPaneInfo(m_statusBar.CommandToIndex(ID_INDICATOR_INTERNAL_USE_ONLY), 
		ID_INDICATOR_INTERNAL_USE_ONLY, SBPS_NORMAL, iWidth );
}
//--------------------------------------------------------------------------------------------------
void CRuleSetEditor::updateCheckBoxAndEditControlBasedOnObject(IObjectWithDescription* pObject, 
						BOOL &bCheckBoxState, UINT uiCheckBoxID, CString &zEditControlText)
{
	// use smart pointer
	IObjectWithDescriptionPtr ipObject = pObject;
	ASSERT_RESOURCE_ALLOCATION("ELI16033", ipObject);

	// update edit control text to match description
	zEditControlText = static_cast<const char*>(ipObject->Description);
	
	// get the checkbox using its ID
	CButton* pCheckBox = (CButton *) GetDlgItem(uiCheckBoxID);

	// throw an error if the check box doesn't exist
	if( !pCheckBox )
	{
		UCLIDException ue("ELI16046", "Unable to access check box resource.");
		throw ue;
	}

	// check if Object has no description (ie. no Object is selected)
	if( zEditControlText.IsEmpty() )
	{
		// disable checkbox
		pCheckBox->EnableWindow(FALSE);	

		// uncheck checkbox
		bCheckBoxState = FALSE;
	}
	else // object has description (ie. object is non-empty)
	{
		// enable checkbox
		pCheckBox->EnableWindow(TRUE);

		// set check box to true if Object is enabled, false otherwise
		bCheckBoxState = asMFCBool(ipObject->Enabled);
	}

	// update object based on check box state
	// NOTE: this also notifies the ipObject's persistence 
	//       object interface that ipObject has been updated.
	ipObject->Enabled = asVariantBool(bCheckBoxState);

	// refresh screen
	UpdateData(FALSE);
}
//--------------------------------------------------------------------------------------------------