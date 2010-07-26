//-------------------------------------------------------------------------------------------------
//
// COPYRIGHT (c) 2003 - 2008 EXTRACT SYSTEMS LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	RuleSetEditor_MessageHandlers.cpp
//
// PURPOSE:	Implementation of CRuleSetEditor Message Handlers
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//-------------------------------------------------------------------------------------------------

#include "stdafx.h"
#include "afcore.h"
#include "RuleSetEditor.h"
#include "AddRuleDlg.h"
#include "AFCategories.h"
#include "RuleTesterDlg.h"
#include "RequiredInterfaces.h"
#include "AFAboutDlg.h"
#include "RuleSetPropertiesDlg.h"

#include "..\..\..\InputFunnel\IFCore\Code\IFCategories.h"

#include <TemporaryResourceOverride.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <Win32Util.h>
#include <MRUList.h>
#include <ComUtils.h>
#include <PromptDlg.h>
#include <FileDialogEx.h>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const int giAUTO_SAVE_TIMERID = 1001;
const int giAUTO_SAVE_FREQUENCY = 60 * 1000;

const bstr_t gbstrINPUT_VALIDATOR_DISPLAY_NAME = "Input Validator";
const bstr_t gbstrDOCUMENT_PREPROCESSOR_DISPLAY_NAME = "Document Preprocessor";
const bstr_t gbstrATTRIBUTE_SPLITTER_DISPLAY_NAME = "Attribute Splitter";
const bstr_t gbstrGLOBAL_OUTPUT_HANDLER_DISPLAY_NAME = "Output Handler";

static const int g_nSTATUS_BAR_HEIGHT = 18;

static const UINT indicators[] =
{
	//ID_SEPARATOR,           // status line indicator
	ID_INDICATOR_COUNTERS,
	ID_INDICATOR_SERIAL_NUMBERS,
	ID_INDICATOR_INTERNAL_USE_ONLY
};
//-------------------------------------------------------------------------------------------------
// CRuleSetEditor message handlers
//-------------------------------------------------------------------------------------------------
BOOL CRuleSetEditor::PreTranslateMessage(MSG* pMsg) 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// translate accelerators
		static HACCEL hAccel = LoadAccelerators(_Module.m_hInst, 
			MAKEINTRESOURCE(IDR_ACCELERATOR_EDITORDLG));
		if (TranslateAccelerator(m_hWnd, hAccel, pMsg))
		{
			// since the message has been handled, no further dispatch is needed
			return TRUE;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05587")

	return CDialog::PreTranslateMessage(pMsg);
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnFileNew() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// if checkModification() returns false, it means the user
		// want to cancel current task, i.e. creating a new file
		if (!checkModification())
		{
			return;
		}

		// create a new ruleset object
		UCLID_AFCORELib::IRuleSetPtr ipRuleSet(CLSID_RuleSet);
		ASSERT_RESOURCE_ALLOCATION("ELI04735", ipRuleSet != NULL);

		// use the new/empty ruleset object as the object associated with
		// this UI.
		m_ipRuleSet = ipRuleSet;
		refreshUIFromRuleSet();

		// A new file doesn't have a name yet
		m_strCurrentFileName = "";
		
		// update window caption
		updateWindowCaption();

		// Delete recovery file - user has started a new file
		m_FRM.deleteRecoveryFile();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04416")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnFileOpen() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// call openFile with "" argument to indicate that we want the file-open
		// dialog box to be displayed.
		openFile("");
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04417")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnFileSave() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		// Validate that the rule set is unencrypted and licensed to be saved
		validateRuleSetCanBeSaved();

		if (m_strCurrentFileName.empty())
		{
			// if current rule set hasn't been saved yet.
			OnFileSaveas();
		}
		else 
		{
			bool bFileReadOnly = isFileReadOnly( m_strCurrentFileName );
			try
			{
				if (bFileReadOnly)
				{
					UCLIDException ue( "ELI09214", "File is read only." );
					ue.addDebugInfo("FileName", m_strCurrentFileName );
					throw ue;
				}
			}
			CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09215");
			if (bFileReadOnly)
			{
				OnFileSaveas();
			}
			else
			{
				m_ipRuleSet->SaveTo(get_bstr_t(m_strCurrentFileName.c_str()), 
					VARIANT_TRUE);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04418")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnFileSaveas() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// Validate that the rule set is unencrypted and licensed to be saved
		validateRuleSetCanBeSaved();

		// ask user to select file to save to
		CFileDialogEx fileDlg(FALSE, ".rsd", NULL, OFN_ENABLESIZING | OFN_EXPLORER | 
			OFN_HIDEREADONLY | OFN_PATHMUSTEXIST | OFN_OVERWRITEPROMPT,
			"Ruleset definition files (*.rsd)|*.rsd|All Files (*.*)|*.*||", this);

		string strDir = ::getDirectoryFromFullPath(m_strLastFileOpened) + "\\";
		fileDlg.m_ofn.lpstrInitialDir = strDir.c_str();
			
		if (fileDlg.DoModal() != IDOK)
		{
			return;
		}

		// Save the ruleset object to the specified file
		_bstr_t bstrFileName = get_bstr_t(fileDlg.GetPathName());
		m_ipRuleSet->SaveTo(bstrFileName, VARIANT_TRUE);

		// update the caption of the window to contain the filename
		m_strLastFileOpened = bstrFileName;
		m_strCurrentFileName = m_strLastFileOpened;

		// save to MRU list
		addFileToMRUList(m_strLastFileOpened);

		updateWindowCaption();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04419")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnFileExit() 
{
	OnClose();
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnHelpAbout() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// Display the About box with version information
		CAFAboutDlg dlgAbout( kFlexIndexHelpAbout, "", m_apRuleTesterDlg->getAFEngine() );
		dlgAbout.DoModal();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04441")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnHelpHelp() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
#ifdef _DEBUG
		MessageBox("Please launch Help file manually from the network location.", "Help File", MB_OK);
		return;
#endif

		// Get path to Help file by navigating from the parent folder
		string strHelpPath = m_strBinFolder + "\\..\\FlexIndex\\Help\\FlexIndexSDK.chm";
		simplifyPathName(strHelpPath);

		// Check for file existence
		if (!isFileOrFolderValid(strHelpPath))
		{
			UCLIDException ue("ELI07357", "Can't find Help file.");
			ue.addDebugInfo("Bin Folder", m_strBinFolder);
			ue.addDebugInfo("Help File Path", strHelpPath);
			throw ue;
		}

		// Open the Help file
		runEXE("hh.exe", strHelpPath);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04421")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnToolsCheck() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		// create instance of the category manager
		ICategoryManagerPtr ipCatMgr(CLSID_CategoryManager);
		if (ipCatMgr == NULL)
		{
			throw UCLIDException("ELI04327", "Unable to create instance of CategoryManager.");
		}

		// create a vector of all categories we care about.
		vector<string> vecCategories;
		vecCategories.push_back(AFAPI_VALUE_FINDERS_CATEGORYNAME);
		vecCategories.push_back(AFAPI_VALUE_MODIFIERS_CATEGORYNAME);
		vecCategories.push_back(INPUTFUNNEL_IR_CATEGORYNAME);
		vecCategories.push_back(INPUTFUNNEL_IV_CATEGORYNAME);
		vecCategories.push_back(AFAPI_OUTPUT_HANDLERS_CATEGORYNAME);
		vecCategories.push_back(AFAPI_ATTRIBUTE_SPLITTERS_CATEGORYNAME);
		vecCategories.push_back(AFAPI_DOCUMENT_PREPROCESSORS_CATEGORYNAME);
		vecCategories.push_back(AFAPI_DATA_SCORERS_CATEGORYNAME);
		vecCategories.push_back(AFAPI_CONDITIONS_CATEGORYNAME);
		vecCategories.push_back(AFAPI_ATTRIBUTE_SELECTORS_CATEGORYNAME);

		// Create a vector for component counts
		vector<int> vecCounts;

		// for each category we care about, find new components that may
		// be registered in that category
		vector<string>::const_iterator iter;
		for (iter = vecCategories.begin(); iter != vecCategories.end(); iter++)
		{
			// delete the cache file for the current category
			_bstr_t _bstrCategory = get_bstr_t(iter->c_str());
			ipCatMgr->DeleteCache(_bstrCategory);

			// recreate the cache file for the category
			IStrToStrMapPtr ipMap = ipCatMgr->GetDescriptionToProgIDMap1(_bstrCategory);
			ASSERT_RESOURCE_ALLOCATION("ELI12063", ipMap != NULL);

			// Store count of components found
			vecCounts.push_back( ipMap->Size );
		}

		// Create message for user with component count details (#1507)
		string strText = "Count\tComponent Category";
		int iCategoryCount = vecCategories.size();
		int i;
		for (i = 0; i < iCategoryCount; i++)
		{
			strText += "\n";
			strText += asString( vecCounts[i] );
			strText += "\t";
			strText += vecCategories[i];
		}
		MessageBox( strText.c_str(), "Components Found", MB_OK );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04326")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnToolsTest() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		if (!asCppBool(::IsWindow(m_apRuleTesterDlg->m_hWnd)))
		{
			throw UCLIDException ("ELI18588", "Rule Tester has not been initialized.");
		}

		m_apRuleTesterDlg->ShowWindow(SW_SHOW);
		
		// the dialog may already been shown on screen before this method was called
		// (because we are dealing with a modeless dialog), but may not be in focus.
		// so, set focus to the modeless dialog.
		m_apRuleTesterDlg->SetFocus();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04422")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnToolsHarness() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// Path to TestHarness.exe found in updateToolsMenu()
		// Make sure path was found
		if (!m_strTestHarnessPath.empty())
		{
			// run TestHarness.exe
			runEXE(m_strTestHarnessPath);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07047")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnDropFiles(HDROP hDropInfo)
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{
		// setup hDropInfo to automatically be released when we go out of scope
		DragDropFinisher finisher(hDropInfo);

		unsigned int iNumFiles = DragQueryFile(hDropInfo, 0xFFFFFFFF, NULL, NULL);
		for (unsigned int ui = 0; ui < iNumFiles; ui++)
		{
			// get the full path to the dragged filename
			char pszFile[MAX_PATH + 1];
			DragQueryFile(hDropInfo, ui, pszFile, MAX_PATH);

			// process dropped file
			processDroppedFile(pszFile);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19307")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnBtnAddAttribute() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		// Use basic Prompt dialog 
		PromptDlg dlg("Add Attribute", "Define attribute name");
		
		// Check result from modal dialog
		if (promptForAttributeName(dlg))
		{
			CString zInput = dlg.m_zInput;
			// ensure that an attribute with the specified name is not already
			// in the map
			if (m_ipAttributeNameToInfoMap->Contains(get_bstr_t(zInput)))
			{
				// the user is trying to add an attribute that already exists.
				// select that attribute for them in the UI to indicate to the
				// user that the attribute has already been added.
				m_comboAttr.SetCurSel(m_comboAttr.FindStringExact(-1, zInput));
				
				// throw exception indicating that attribute already exists
				UCLIDException ue("ELI04497", "Specified attribute has already been added to list.");
				ue.addDebugInfo("Attribute name", (LPCTSTR)zInput);
				throw ue;
			}
			
			// add the attribute entry to the map of 
			// attribute name-to-attributeinfo objects
			UCLID_AFCORELib::IAttributeFindInfoPtr ipAttrFindInfo(CLSID_AttributeFindInfo);
			ASSERT_RESOURCE_ALLOCATION("ELI04502", ipAttrFindInfo != NULL);
			m_ipAttributeNameToInfoMap->Set(get_bstr_t((LPCTSTR)zInput),
				ipAttrFindInfo);
			
			// Add the attribute to the combo box
			int iIndex = m_comboAttr.AddString(zInput);
			
			// Default to selection of the new attribute
			if (iIndex >= 0)
			{
				m_comboAttr.SetCurSel(iIndex);
				
				// let the Rule Tester dialog know what the current attribute is
				m_apRuleTesterDlg->setCurrentAttributeName((LPCTSTR)zInput);
				
				// Also store Info pointer
				m_ipInfo = ipAttrFindInfo;
				
				// Update the display for this new Attribute
				refreshUIFromAttribute();
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04423")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnBtnDeleteAttribute() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		// Retrieve current selection index
		int iIndex = m_comboAttr.GetCurSel();
		if (iIndex >= 0)
		{
			// Retrieve current selection text
			CString	zAttribute;
			m_comboAttr.GetLBText( iIndex, zAttribute );

			// Request confirmation
			CString	zPrompt;
			int		iResult;
			zPrompt.Format( "Are you sure that attribute '%s' should be deleted?", 
				zAttribute );
			iResult = MessageBox( LPCTSTR(zPrompt), "Confirm Delete", 
				MB_YESNO | MB_ICONQUESTION );

			// Act on response
			if (iResult == IDYES)
			{
				// delete the entry from the map
				m_ipAttributeNameToInfoMap->RemoveItem(get_bstr_t((LPCTSTR) zAttribute));

				// delete the selected entry from the UI
				m_comboAttr.DeleteString( iIndex );

				// Select nearest attribute
				int iCount = m_comboAttr.GetCount();
				if (iIndex >= iCount)
				{
					iIndex = iCount - 1;
				}

				if (iIndex >= 0)
				{
					m_comboAttr.SetCurSel( iIndex );

					// get the currently selected attribute name
					CString	zText;
					m_comboAttr.GetLBText( iIndex, zText );
					_bstr_t	bstrText;
					bstrText = get_bstr_t(LPCTSTR(zText));

					// get access to the attribute find info associated with
					// the currently selected attribute.
					m_ipInfo = m_ipAttributeNameToInfoMap->GetValue( bstrText );

					// let the Rule Tester dialog know what the current attribute is
					m_apRuleTesterDlg->setCurrentAttributeName((LPCTSTR) zText);
				}
				else
				{
					// if there are no attributes, there is no attribute find info
					m_ipInfo = NULL;

					// let the Rule Tester dialog know what the current attribute is
					m_apRuleTesterDlg->setCurrentAttributeName("");
				}

				// Refresh the combo box display
				m_comboAttr.Invalidate();

				// refresh all UI elements so that they correspond to the
				// currently selected attribute
				refreshUIFromAttribute();
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04424")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnBtnRenameAttribute() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		// Retrieve current selection index
		int iIndex = m_comboAttr.GetCurSel();
		if (iIndex >= 0)
		{
			// Retrieve current selection text
			CString	zAttribute;
			m_comboAttr.GetLBText( iIndex, zAttribute );

			// Use basic Prompt dialog 
			PromptDlg dlg("Rename Attribute", "Define new attribute name", zAttribute);
			
			// Check result from modal dialog
			if (promptForAttributeName(dlg))
			{
				CString zInput = dlg.m_zInput;
				// rename the map entry
				m_ipAttributeNameToInfoMap->RenameKey(get_bstr_t((LPCTSTR) zAttribute),
					get_bstr_t((LPCTSTR)zInput));
				
				// Delete the old string from the UI and add the new string
				m_comboAttr.DeleteString(iIndex);
				iIndex = m_comboAttr.AddString(zInput);
				
				// Default to selection of the new attribute
				if (iIndex >= 0)
				{
					m_comboAttr.SetCurSel( iIndex );
					
					// let the Rule Tester dialog know what the current attribute is
					m_apRuleTesterDlg->setCurrentAttributeName((LPCTSTR)zInput);
					
					// Update button states
					setButtonStates();
					
				}	// end if new index >= 0
			}		// end if Prompt results available
		}			// end if current selection defined
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04425")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnSelchangeComboAttributes() 
{
	UpdateData( TRUE );

	// Get Attribute index and name
	int iIndex = m_comboAttr.GetCurSel();
	CString	zAttribute;
	m_comboAttr.GetLBText( iIndex, zAttribute );
	_bstr_t	bstrAttribute = LPCTSTR(zAttribute);

	// let the Rule Tester dialog know that the current attribute has changed
	m_apRuleTesterDlg->setCurrentAttributeName((LPCTSTR) zAttribute);

	// Get AttributeFindInfo object for this attribute
	m_ipInfo = m_ipAttributeNameToInfoMap->GetValue((const char *) bstrAttribute );

	// Refresh display based on this Attribute
	refreshUIFromAttribute();
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnRclickComboAttributes(NMHDR* pNMHDR, LRESULT* pResult) 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// Check for current Attribute selection
		int iIndex = -1;
		iIndex = m_comboAttr.GetCurSel();

		// Load the context menu
		CMenu menu;
		menu.LoadMenu( IDR_MNU_CONTEXT );
		CMenu *pContextMenu = menu.GetSubMenu( 0 );

		// Add Duplicate and a separator to the menu
		pContextMenu->InsertMenu( 0, MF_BYPOSITION | MF_SEPARATOR, 0, "" );
		pContextMenu->InsertMenu( 0, MF_BYPOSITION, ID_EDIT_DUPLICATE, "Duplicate" );

		// Set the control
		m_eContextMenuCtrl = kAttributes;

		//////////////////////////
		// Enable or disable items
		//////////////////////////
		UINT nEnable = (MF_BYCOMMAND | MF_ENABLED);
		UINT nDisable = (MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
		bool bEnable = iIndex > -1;

		// No Attribute item selected
		pContextMenu->EnableMenuItem(ID_EDIT_DUPLICATE, bEnable ? nEnable : nDisable);
		pContextMenu->EnableMenuItem(ID_EDIT_CUT, bEnable ? nEnable : nDisable);
		pContextMenu->EnableMenuItem(ID_EDIT_COPY, bEnable ? nEnable : nDisable);
		pContextMenu->EnableMenuItem(ID_EDIT_DELETE, bEnable ? nEnable : nDisable);

		bEnable = clipboardObjectIsAttribute();
		pContextMenu->EnableMenuItem(ID_EDIT_PASTE, bEnable ? nEnable : nDisable);

		// Map the point to the correct position
		CPoint	point;
		GetCursorPos(&point);
		
		// Display and manage the context menu
		pContextMenu->TrackPopupMenu( TPM_LEFTALIGN | TPM_LEFTBUTTON | TPM_RIGHTBUTTON, 
			point.x, point.y, this );

		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05465")
}
//-------------------------------------------------------------------------------------------------
BOOL CRuleSetEditor::OnInitDialog() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		CDialog::OnInitDialog();
		
		// initiate the timer event to do the auto-saving
		SetTimer(giAUTO_SAVE_TIMERID, giAUTO_SAVE_FREQUENCY, NULL);

		// Set the icon for this dialog.  The framework does this automatically
		//  when the application's main window is not a dialog
		SetIcon( m_hIcon, TRUE );			// Set big icon
		SetIcon( m_hIcon, FALSE );			// Set small icon

		// Prepare the list control for Attribute Rules
		prepareList();

		// Set Up and Down bitmaps to buttons
		m_btnRuleUp.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_UP)));
		m_btnRuleDown.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_DOWN)));

		// create status bar
		m_statusBar.Create(this);
		m_statusBar.GetStatusBarCtrl().SetMinHeight(g_nSTATUS_BAR_HEIGHT);
		m_statusBar.SetIndicators(indicators, sizeof(indicators)/sizeof(UINT));
		
		// And position the control bars
		RepositionBars( AFX_IDW_CONTROLBAR_FIRST, AFX_IDW_CONTROLBAR_LAST, 0 );

		// update the window caption to be the default window
		// caption when no file is loaded
		updateWindowCaption();

		// Update the Tools menu to include a Test Harness item
		updateToolsMenu();

		// Update the display for initial state
		refreshUIFromRuleSet();

		// center the static prompt which shows the "ruleset is encrypted" message
		CWnd *pWnd = GetDlgItem( IDC_STATIC_PROMPT );
		if (pWnd != NULL)
		{
			pWnd->CenterWindow();
		}

		// create rule set tester
		m_apRuleTesterDlg->Create(RuleTesterDlg::IDD, m_pParentWnd);

		// create the MRU list
		refreshFileMRU();
		setStatusBarText();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04426")

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//--------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnBtnAddRule() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		// Create an empty IAttributeRule object
		UCLID_AFCORELib::IAttributeRulePtr	ipRule( CLSID_AttributeRule );

		// Get name of selected attribute
		int iIndex = m_comboAttr.GetCurSel();
		if (iIndex == -1)
		{
			return;
		}
		CString	zAttribute;
		m_comboAttr.GetLBText( iIndex, zAttribute );

		// Create the Add Rule dialog
		CAddRuleDlg	dlg(m_ipClipboardMgr, ipRule );

		// Set the prompt text
		CString	zPrompt;
		zPrompt.Format( "Select rule to find value for attribute '%s'", zAttribute );
		string strPrompt = (LPCTSTR)zPrompt;
		dlg.SetPromptText(strPrompt);

		// Display the modal dialog
		int iReturn = dlg.DoModal();

		if (iReturn == IDOK)
		{
			// Retrieve the rule description
			CString	zDescription = dlg.m_zDescription;
			int nCurrentSelectedIndex = -1;

			// Add the description to the list
			if (!zDescription.IsEmpty())
			{
				// Check for current rule selection
				POSITION pos = m_listRules.GetFirstSelectedItemPosition();
				if (pos != NULL)
				{
					// Get index of first selection
					nCurrentSelectedIndex = m_listRules.GetNextSelectedItem( pos );
				}

				// Check for item count if no selection
				if (nCurrentSelectedIndex == -1)
				{
					nCurrentSelectedIndex = m_listRules.GetItemCount();
				}

				//////////////////
				// Insert new item before selection or at end of list
				//////////////////
				// Add the item without text in Enabled column
				m_listRules.InsertItem( nCurrentSelectedIndex, "" );

				// Add description text
				m_listRules.SetItemText( nCurrentSelectedIndex, m_iDESC_LIST_COLUMN, 
					LPCTSTR(zDescription) );

				// Default enabled state to true
				m_listRules.SetCheck( nCurrentSelectedIndex, TRUE );

				///////////////////////////////
				// Insert this Rule into vector
				///////////////////////////////
				// Retrieve existing vector
				IIUnknownVectorPtr	ipRules = m_ipInfo->GetAttributeRules();
				if (ipRules == NULL)
				{
					// Create and throw exception
					throw UCLIDException("ELI04590", 
						"Unable to retrieve Attribute Rules.");
				}

				// Insert the new Rule
				ipRules->Insert( nCurrentSelectedIndex, ipRule );

				// Store the updated vector
				m_ipInfo->PutAttributeRules( ipRules );
			}

			// Refresh the display
			UpdateData( TRUE );

			// clear the selection of the previous selected item if any
			clearListSelection();
			
			// Select the newly added rule or retain current selection
			m_listRules.SetItemState( nCurrentSelectedIndex, LVIS_SELECTED, LVIS_SELECTED );
			m_listRules.SetFocus();
		}

		// Update button states
		setButtonStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04427")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnBtnDeleteRule() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		// Check for current rule selection
		int iIndex = -1;
		POSITION pos = m_listRules.GetFirstSelectedItemPosition();
		if (pos != NULL)
		{
			// Get index of first selection
			iIndex = m_listRules.GetNextSelectedItem( pos );
		}

		// Just return if no items are selected
		if (iIndex == -1)
		{
			return;
		}

		// Check for multiple selection of Rules
		int iIndex2 = m_listRules.GetNextSelectedItem( pos );

		// Handle single-selection case
		int		iResult;
		CString	zPrompt;
		if (iIndex2 == -1)
		{
			// Retrieve current Rule description
			CString	zDescription = m_listRules.GetItemText( iIndex, 
				m_iDESC_LIST_COLUMN );

			// Create prompt for confirmation
			zPrompt.Format( "Are you sure that rule '%s' should be deleted?", 
				zDescription );
		}
		// Handle multiple-selection case
		else
		{
			// Create prompt for confirmation
			zPrompt.Format( "Are you sure that the selected rules should be deleted?" );
		}

		// Present MessageBox
		iResult = MessageBox( LPCTSTR(zPrompt), "Confirm Delete", 
			MB_YESNO | MB_ICONQUESTION );

		// Act on response
		if (iResult == IDYES)
		{
			// Mark selected items for deletion
			markSelectedRules();

			// Delete the marked rules
			deleteMarkedRules();

			// Select the next (or the last) rule
			int iCount = m_listRules.GetItemCount();
			if (iCount <= iIndex)
			{
				iIndex = iCount - 1;
			}
		}

		// Retain selection and focus
		m_listRules.SetItemState( iIndex, LVIS_SELECTED, LVIS_SELECTED );
		m_listRules.SetFocus();

		// Refresh the display
		UpdateData( TRUE );

		// Update button states
		setButtonStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04428")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnBtnConfigureRule() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		// Get name of selected attribute
		int iIndex = m_comboAttr.GetCurSel();
		if (iIndex == -1)
		{
			return;
		}
		CString	zAttribute;
		m_comboAttr.GetLBText( iIndex, zAttribute );

		// Retrieve current selection index
		iIndex = -1;
		POSITION pos = m_listRules.GetFirstSelectedItemPosition();
		if (pos != NULL)
		{
			// Get index of first selection
			iIndex = m_listRules.GetNextSelectedItem( pos );
		}

		if (iIndex > -1)
		{
			// Retrieve collected rules
			IIUnknownVectorPtr ipRules = m_ipInfo->GetAttributeRules();
			ASSERT_RESOURCE_ALLOCATION( "ELI15499", ipRules != NULL );

			// Retrieve current rule
			UCLID_AFCORELib::IAttributeRulePtr	ipRule = ipRules->At( iIndex );
			ASSERT_RESOURCE_ALLOCATION( "ELI15500", ipRule != NULL );

			// Make copy for configure purposes
			ICopyableObjectPtr ipCopyableObject = ipRule;
			if (ipCopyableObject == NULL)
			{
				throw UCLIDException( "ELI04715", 
					"Attribute Rule does not support copying." );
			}

			UCLID_AFCORELib::IAttributeRulePtr	ipNewRule = 
				ipCopyableObject->Clone();
			if (ipNewRule == NULL)
			{
				// Create and throw exception
				throw UCLIDException("ELI04592", 
					"Unable to retrieve Attribute Rule.");
			}

			// Use Add Rule dialog 
			CAddRuleDlg	dlg(m_ipClipboardMgr, ipNewRule );

			// Set the prompt text
			CString	zPrompt;
			zPrompt.Format( "Select rule to find value for attribute '%s'", zAttribute );
			string strPrompt = (LPCTSTR)zPrompt;
			dlg.SetPromptText(strPrompt);

			// Check result from modal dialog
			if (dlg.DoModal() == IDOK)
			{
				// Retrieve the rule description
				CString	zDescription = dlg.m_zDescription;

				// Add the description to the list
				if (!zDescription.IsEmpty())
				{
					// Update the description text
					m_listRules.SetItemText( iIndex, m_iDESC_LIST_COLUMN, 
						LPCTSTR(zDescription) );

					//////////////////////////
					// Update the Rules vector
					//////////////////////////
					// Retrieve existing vector
					IIUnknownVectorPtr	ipRules = m_ipInfo->GetAttributeRules();
					if (ipRules == NULL)
					{
						// Create and throw exception
						throw UCLIDException("ELI04593", 
							"Unable to retrieve Attribute Rules.");
					}

					// Store the updated Rule
					ipRules->Set( iIndex, ipNewRule );
				}

				// Refresh the display
				UpdateData( TRUE );

				// Update button states
				setButtonStates();
			}			// end if results accepted

			// Retain selection and focus
			m_listRules.SetItemState( iIndex, LVIS_SELECTED, LVIS_SELECTED );
			m_listRules.SetFocus();

			// Update button states
			setButtonStates();
		}				// end if current selection defined
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04429")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnBtnRuleUp() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		// Check for current rule selection
		int iIndex = -1;
		POSITION pos = m_listRules.GetFirstSelectedItemPosition();
		if (pos != NULL)
		{
			// Get index of first selection
			iIndex = m_listRules.GetNextSelectedItem( pos );
		}

		// Selection cannot be at top of list
		if (iIndex > 0)
		{
			// Retrieve current Rule flag
			BOOL	bEnabled = m_listRules.GetCheck( iIndex );

			// Retrieve current Rule description
			CString	zDescription = m_listRules.GetItemText( iIndex, 
				m_iDESC_LIST_COLUMN );

			// Delete the item
			m_listRules.DeleteItem( iIndex );

			// Replace the item one row higher
			m_listRules.InsertItem( iIndex - 1, "" );

			// Restore the flag
			m_listRules.SetCheck( iIndex - 1, bEnabled );

			// Restore the description
			m_listRules.SetItemText( iIndex - 1, m_iDESC_LIST_COLUMN, 
				LPCTSTR(zDescription) );

			//////////////////////////
			// Update the Rules vector
			//////////////////////////
			// Retrieve existing vector
			IIUnknownVectorPtr	ipRules = m_ipInfo->GetAttributeRules();
			if (ipRules == NULL)
			{
				// Create and throw exception
				throw UCLIDException("ELI04594", 
					"Unable to retrieve Attribute Rules.");
			}

			// Swap the rules
			ipRules->Swap( iIndex, iIndex - 1 );

			// Retain selection and focus
			m_listRules.SetItemState( iIndex - 1, LVIS_SELECTED, LVIS_SELECTED );
			m_listRules.SetFocus();

			// Update button states
			setButtonStates();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04430")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnBtnRuleDown() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		// Check for current rule selection
		int iIndex = -1;
		POSITION pos = m_listRules.GetFirstSelectedItemPosition();
		if (pos != NULL)
		{
			// Get index of first selection
			iIndex = m_listRules.GetNextSelectedItem( pos );
		}

		// Selection cannot be at bottom of list
		int iCount = m_listRules.GetItemCount();
		if (iIndex < iCount - 1)
		{
			// Retrieve current Rule flag
			BOOL	bEnabled = m_listRules.GetCheck( iIndex );

			// Retrieve current Rule description
			CString	zDescription = m_listRules.GetItemText( iIndex, 
				m_iDESC_LIST_COLUMN );

			// Delete the item
			m_listRules.DeleteItem( iIndex );

			// Replace the item one row lower
			m_listRules.InsertItem( iIndex + 1, "" );

			// Restore the flag
			m_listRules.SetCheck( iIndex + 1, bEnabled );

			// Restore the description
			m_listRules.SetItemText( iIndex + 1, m_iDESC_LIST_COLUMN, 
				LPCTSTR(zDescription) );

			//////////////////////////
			// Update the Rules vector
			//////////////////////////
			// Retrieve existing vector
			IIUnknownVectorPtr	ipRules = m_ipInfo->GetAttributeRules();
			if (ipRules == NULL)
			{
				// Create and throw exception
				throw UCLIDException("ELI04595", 
					"Unable to retrieve Attribute Rules.");
			}

			// Swap the rules
			ipRules->Swap( iIndex, iIndex + 1 );

			// Retain selection and focus
			m_listRules.SetItemState( iIndex + 1, LVIS_SELECTED, LVIS_SELECTED );
			m_listRules.SetFocus();

			// Update button states
			setButtonStates();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04431")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnClickListRules(NMHDR* pNMHDR, LRESULT* pResult) 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		////////////////////////////
		// Check for check box click
		////////////////////////////

		// Retrieve position of click
		CPoint	p;
		GetCursorPos( &p );
		m_listRules.ScreenToClient( &p );


		UINT	uiFlags;
		int		iIndex = m_listRules.HitTest( p, &uiFlags );

		// Was click on the checkbox?
		// Note: if the item is clicked on anywhere but the check box,
		// the flag contains LVHT_ONITEMSTATEICON, LVHT_ONITEMICON and 
		// LVHT_ONITEMLABEL. If the item is clicked on only the check box
		// part, the flag will only contain LVHT_ONITEMSTATEICON 
		if (iIndex >= 0
			&& (uiFlags & LVHT_ONITEMSTATEICON)
			&& !(uiFlags & LVHT_ONITEMICON)
			&& !(uiFlags & LVHT_ONITEMLABEL))
		{
			// Retrieve existing vector of Rules
			IIUnknownVectorPtr	ipRules = m_ipInfo->GetAttributeRules();

			// Retrieve this rule from vector
			UCLID_AFCORELib::IAttributeRulePtr	ipRule = ipRules->At(iIndex);

			// Get the old state of the checkbox
			BOOL bChecked = m_listRules.GetCheck(iIndex);
			// Enable the rule if the old state of the check box is unchecked
			ipRule->IsEnabled = asVariantBool(!bChecked);
		}
		
		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04432")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnDblclkListRules(NMHDR* pNMHDR, LRESULT* pResult) 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		// This will act as Select + Configure
		OnBtnConfigureRule();

		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04433")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnRclickListRules(NMHDR* pNMHDR, LRESULT* pResult) 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// Check for current Attribute selection
		int iAttributeIndex = -1;
		iAttributeIndex = m_comboAttr.GetCurSel();

		// Just return if no Attribute is selected
		if (iAttributeIndex == -1)
		{
			return;
		}

		// always wait till the item(s) r-clicked is(are) selected
		// then bring up the context menu
		if (pNMHDR)
		{
			int iIndex = -1;
			POSITION pos = m_listRules.GetFirstSelectedItemPosition();
			if (pos != NULL)
			{
				// Get index of first selection
				iIndex = m_listRules.GetNextSelectedItem( pos );
			}

			// Load the context menu
			CMenu menu;
			menu.LoadMenu( IDR_MNU_CONTEXT );
			CMenu *pContextMenu = menu.GetSubMenu( 0 );
			
			// Set the control
			m_eContextMenuCtrl = kRulesList;
			
			//////////////////////////
			// Enable or disable items
			//////////////////////////
			UINT nEnable = (MF_BYCOMMAND | MF_ENABLED);
			UINT nDisable = (MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
			bool bEnable = iIndex > -1;
			
			// No Rule item selected
			pContextMenu->EnableMenuItem(ID_EDIT_CUT, bEnable ? nEnable : nDisable);
			pContextMenu->EnableMenuItem(ID_EDIT_COPY, bEnable ? nEnable : nDisable);
			pContextMenu->EnableMenuItem(ID_EDIT_DELETE, bEnable ? nEnable : nDisable);
			
			bEnable = 
				m_ipClipboardMgr->ObjectIsIUnknownVectorOfType(IID_IAttributeRule) == VARIANT_TRUE;
			pContextMenu->EnableMenuItem(ID_EDIT_PASTE, bEnable ? nEnable : nDisable);
			
			// Map the point to the correct position
			CPoint	point;
			GetCursorPos(&point);
			
			// Display and manage the context menu
			pContextMenu->TrackPopupMenu( TPM_LEFTALIGN | TPM_LEFTBUTTON | TPM_RIGHTBUTTON, 
				point.x, point.y, this );
		}

		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05403")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnCheckStop() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		UpdateData( TRUE );

		// Apply the setting to the current Info object
		if (m_ipInfo != NULL)
		{
			m_ipInfo->PutStopSearchingWhenValueFound( asVariantBool(m_bStopWhenValueFound) );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04596")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnRclickValidatorText(NMHDR* pNMHDR, LRESULT* pResult)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		// Load the context menu
		CMenu menu;
		menu.LoadMenu( IDR_MNU_CONTEXT );
		CMenu *pContextMenu = menu.GetSubMenu( 0 );

		// Set the control
		m_eContextMenuCtrl = kValidator;

		//////////////////////////
		// Enable or disable items
		//////////////////////////
		UINT nEnable = (MF_BYCOMMAND | MF_ENABLED);
		UINT nDisable = (MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
		bool bEnable = m_ipInfo != NULL && !m_zIVDescription.IsEmpty();

			// No Attribute item selected
		pContextMenu->EnableMenuItem(ID_EDIT_CUT, bEnable ? nEnable : nDisable);
		pContextMenu->EnableMenuItem(ID_EDIT_COPY, bEnable ? nEnable : nDisable);
		pContextMenu->EnableMenuItem(ID_EDIT_DELETE, bEnable ? nEnable : nDisable);

		bEnable = 
			m_ipClipboardMgr->ObjectIsTypeWithDescription(IID_IInputValidator) == VARIANT_TRUE;
		pContextMenu->EnableMenuItem(ID_EDIT_PASTE, bEnable ? nEnable : nDisable);

		// Map the point to the correct position
		CPoint	point;
		GetCursorPos( &point );
		
		// Display and manage the context menu
		pContextMenu->TrackPopupMenu( TPM_LEFTALIGN | TPM_LEFTBUTTON | TPM_RIGHTBUTTON, 
			point.x, point.y, this );

		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05472")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnBtnSelectIV() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		if (m_ipInfo)
		{
			// get the position of the input validator button
			RECT rect;
			m_btnSelectIV.GetWindowRect(&rect);

			// prompt user to select and configure input validator
			VARIANT_BOOL vbDirty = m_ipMiscUtils->HandlePlugInObjectCommandButtonClick(
				m_ipInfo->InputValidator, gbstrINPUT_VALIDATOR_DISPLAY_NAME, get_bstr_t(INPUTFUNNEL_IV_CATEGORYNAME),
				VARIANT_TRUE, gRequiredInterfaces.ulCount, gRequiredInterfaces.pIIDs, rect.right, rect.top);

			// check if input validator was modified
			if (vbDirty == VARIANT_TRUE)
			{
				// update controls based on new or modified input validator
				updateCheckBoxAndEditControlBasedOnObject(m_ipInfo->InputValidator,
					m_bInputValidator, IDC_CHECK_INPUT_VALIDATOR, m_zIVDescription);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04440")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnBtnSelectPreprocessor() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// get the position of the document preprocessor button
		RECT rect;
		m_btnSelectPreprocessor.GetWindowRect(&rect);

		// prompt user to select and configure document preprocessor
		VARIANT_BOOL vbDirty = m_ipMiscUtils->HandlePlugInObjectCommandButtonClick(
			m_ipRuleSet->GlobalDocPreprocessor,	gbstrDOCUMENT_PREPROCESSOR_DISPLAY_NAME, 
			get_bstr_t(AFAPI_DOCUMENT_PREPROCESSORS_CATEGORYNAME), VARIANT_TRUE, 
			gRequiredInterfaces.ulCount, gRequiredInterfaces.pIIDs, rect.right, rect.top);

		// check if document preprocessor was modified
		if (vbDirty == VARIANT_TRUE)
		{
			// update controls to reflect the new or changed document preprocessor
			updateCheckBoxAndEditControlBasedOnObject(m_ipRuleSet->GlobalDocPreprocessor,
				m_bDocumentPP, IDC_CHECK_DOCUMENT_PP, m_zPPDescription);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06020")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnRclickSplitterText(NMHDR* pNMHDR, LRESULT* pResult)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		// Load the context menu
		CMenu menu;
		menu.LoadMenu( IDR_MNU_CONTEXT );
		CMenu *pContextMenu = menu.GetSubMenu( 0 );

		// Set the control
		m_eContextMenuCtrl = kSplitter;

		//////////////////////////
		// Enable or disable items
		//////////////////////////
		bool bEnable = m_ipInfo != NULL && !m_zAttributeSplitterDescription.IsEmpty();
		UINT nEnableValue = bEnable ? (MF_BYCOMMAND | MF_ENABLED) : (MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);

		pContextMenu->EnableMenuItem(ID_EDIT_CUT, nEnableValue);
		pContextMenu->EnableMenuItem(ID_EDIT_COPY, nEnableValue);
		pContextMenu->EnableMenuItem(ID_EDIT_DELETE, nEnableValue);

		// only enable Paste menu item if the object from clipboard is of type AttributeSplitter
		bEnable = 
			m_ipClipboardMgr->ObjectIsTypeWithDescription(IID_IAttributeSplitter) == VARIANT_TRUE;
		nEnableValue = bEnable ? (MF_BYCOMMAND | MF_ENABLED) : (MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
		pContextMenu->EnableMenuItem( ID_EDIT_PASTE, nEnableValue);

		// Map the point to the correct position
		CPoint	point;
		GetCursorPos( &point );
		
		// Display and manage the context menu
		pContextMenu->TrackPopupMenu( TPM_LEFTALIGN | TPM_LEFTBUTTON | TPM_RIGHTBUTTON, 
			point.x, point.y, this );

		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05473")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnRclickPreprocessorText(NMHDR* pNMHDR, LRESULT* pResult)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		// Load the context menu
		CMenu menu;
		menu.LoadMenu(IDR_MNU_CONTEXT);
		CMenu *pContextMenu = menu.GetSubMenu(0);

		// Set the control
		m_eContextMenuCtrl = kPreprocessor;

		//////////////////////////
		// Enable or disable items
		//////////////////////////
		UINT nEnable = (MF_BYCOMMAND | MF_ENABLED);
		UINT nDisable = (MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
		bool bEnable = !m_zPPDescription.IsEmpty();

		// Preprocessor is not defined
		pContextMenu->EnableMenuItem(ID_EDIT_CUT, bEnable ? nEnable : nDisable);
		pContextMenu->EnableMenuItem(ID_EDIT_COPY, bEnable ? nEnable : nDisable);
		pContextMenu->EnableMenuItem(ID_EDIT_DELETE, bEnable ? nEnable : nDisable);

		// Check Clipboard object type to enable/disable Paste menu item
		bEnable = 
			m_ipClipboardMgr->ObjectIsTypeWithDescription(IID_IDocumentPreprocessor) == VARIANT_TRUE;
		pContextMenu->EnableMenuItem(ID_EDIT_PASTE, bEnable ? nEnable : nDisable);

		// Map the point to the correct position
		CPoint	point;
		GetCursorPos( &point );
		// Display and manage the context menu
		pContextMenu->TrackPopupMenu(TPM_LEFTALIGN|TPM_LEFTBUTTON|TPM_RIGHTBUTTON, 
			point.x, point.y, this);

		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06063")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnRclickHandlerText(NMHDR* pNMHDR, LRESULT* pResult)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		// Load the context menu
		CMenu menu;
		menu.LoadMenu( IDR_MNU_CONTEXT );
		CMenu *pContextMenu = menu.GetSubMenu( 0 );

		// Set the control
		m_eContextMenuCtrl = kOutputHandler;

		//////////////////////////
		// Enable or disable items
		//////////////////////////
		UINT nEnable = (MF_BYCOMMAND | MF_ENABLED);
		UINT nDisable = (MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
		bool bEnable = !m_zOutputHandlerDescription.IsEmpty();

		// Output Handler is not defined
		pContextMenu->EnableMenuItem( ID_EDIT_CUT, bEnable ? nEnable : nDisable );
		pContextMenu->EnableMenuItem( ID_EDIT_COPY, bEnable ? nEnable : nDisable );
		pContextMenu->EnableMenuItem( ID_EDIT_DELETE, bEnable ? nEnable : nDisable );

		// Check Clipboard object type to enable/disable Paste menu item
		bEnable = 
			m_ipClipboardMgr->ObjectIsTypeWithDescription(IID_IOutputHandler) == VARIANT_TRUE;
		pContextMenu->EnableMenuItem( ID_EDIT_PASTE, bEnable ? nEnable : nDisable );

		// Map the point to the correct position
		CPoint	point;
		GetCursorPos( &point );

		// Display and manage the context menu
		pContextMenu->TrackPopupMenu( TPM_LEFTALIGN | TPM_LEFTBUTTON | TPM_RIGHTBUTTON, 
			point.x, point.y, this );

		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07731")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnBtnSelectAttributeSplitter() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		if (m_ipInfo)
		{	
			// get the position of the attribute handler button
			RECT rect;
			m_btnSelectAttributeSplitter.GetWindowRect(&rect);

			// prompt user to select and configure attribute splitter
			VARIANT_BOOL vbDirty = m_ipMiscUtils->HandlePlugInObjectCommandButtonClick(m_ipInfo->AttributeSplitter,
				gbstrATTRIBUTE_SPLITTER_DISPLAY_NAME, get_bstr_t(AFAPI_ATTRIBUTE_SPLITTERS_CATEGORYNAME),
				VARIANT_TRUE, gRequiredInterfaces.ulCount, gRequiredInterfaces.pIIDs, rect.right, rect.top);

			// check if attribute splitter has been changed
			if (vbDirty == VARIANT_TRUE)
			{
				// update controls to reflect new or modified attribute splitter
				updateCheckBoxAndEditControlBasedOnObject(m_ipInfo->AttributeSplitter,
					m_bAttSplitter, IDC_CHECK_ATT_SPLITTER, m_zAttributeSplitterDescription);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05295")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnBtnSelectOutputHandler() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		// get the position of the output handler button
		RECT rect;
		m_btnSelectOH.GetWindowRect(&rect);

		// prompt user to select and configure output handler
		VARIANT_BOOL vbDirty = m_ipMiscUtils->HandlePlugInObjectCommandButtonClick(
			m_ipRuleSet->GlobalOutputHandler, gbstrGLOBAL_OUTPUT_HANDLER_DISPLAY_NAME, 
			get_bstr_t(AFAPI_OUTPUT_HANDLERS_CATEGORYNAME), VARIANT_TRUE, 0, NULL, rect.right, rect.top);

		// check if output handler has been modified
		if (vbDirty == VARIANT_TRUE)
		{
			// update controls to display modified output handler
			updateCheckBoxAndEditControlBasedOnObject(m_ipRuleSet->GlobalOutputHandler,
				m_bOutputHandler, IDC_CHECK_OUTPUT_HANDLER, m_zOutputHandlerDescription);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07730")
}
//--------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnDoubleClickDocumentPreprocessor()
{
	try
	{
		// update GlobalOutputHandler based on double-click event
		VARIANT_BOOL vbDirty = m_ipMiscUtils->HandlePlugInObjectDoubleClick(m_ipRuleSet->GlobalDocPreprocessor,
			gbstrDOCUMENT_PREPROCESSOR_DISPLAY_NAME, get_bstr_t(AFAPI_DOCUMENT_PREPROCESSORS_CATEGORYNAME), 
			VARIANT_TRUE, gRequiredInterfaces.ulCount, gRequiredInterfaces.pIIDs);

		if (vbDirty == VARIANT_TRUE)
		{
			// update display of the output handler check box and edit control
			updateCheckBoxAndEditControlBasedOnObject(m_ipRuleSet->GlobalDocPreprocessor,
				m_bDocumentPP, IDC_CHECK_DOCUMENT_PP, m_zPPDescription);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16038")
}
//--------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnDoubleClickInputValidator()
{
	try
	{
		// do nothing if attribute combo box is empty
		if (m_comboAttr.GetCount() != 0)
		{
			// update InputValidator based on double-click event
			VARIANT_BOOL vbDirty = m_ipMiscUtils->HandlePlugInObjectDoubleClick(
				m_ipInfo->InputValidator, gbstrINPUT_VALIDATOR_DISPLAY_NAME, 
				get_bstr_t(INPUTFUNNEL_IV_CATEGORYNAME), VARIANT_TRUE, 
				gRequiredInterfaces.ulCount, gRequiredInterfaces.pIIDs);

			// check if InputValidator has been modified
			if (vbDirty == VARIANT_TRUE)
			{
				// update display of the output handler check box and edit control
				updateCheckBoxAndEditControlBasedOnObject(m_ipInfo->InputValidator,
					m_bInputValidator, IDC_CHECK_INPUT_VALIDATOR, m_zIVDescription);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16039")
}
//--------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnDoubleClickAttributeSplitter()
{
	try
	{
		// do nothing if attribute combo box is empty
		if (m_comboAttr.GetCount() != 0)
		{
			// update GlobalOutputHandler based on double-click event
			VARIANT_BOOL vbDirty = m_ipMiscUtils->HandlePlugInObjectDoubleClick(m_ipInfo->AttributeSplitter,
				gbstrATTRIBUTE_SPLITTER_DISPLAY_NAME, get_bstr_t(AFAPI_ATTRIBUTE_SPLITTERS_CATEGORYNAME),
				VARIANT_TRUE, gRequiredInterfaces.ulCount, gRequiredInterfaces.pIIDs);

			// check if GlobalOutputHandler was changed
			if (vbDirty == VARIANT_TRUE)
			{
				// update display of the output handler check box and edit control
				updateCheckBoxAndEditControlBasedOnObject(m_ipInfo->AttributeSplitter,
					m_bAttSplitter, IDC_CHECK_ATT_SPLITTER, m_zAttributeSplitterDescription);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16040")
}
//--------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnDoubleClickOutputHandler()
{
	try
	{
		// modify GlobalOutputHandler based on user input
		VARIANT_BOOL vbDirty = m_ipMiscUtils->HandlePlugInObjectDoubleClick(m_ipRuleSet->GlobalOutputHandler,
			gbstrGLOBAL_OUTPUT_HANDLER_DISPLAY_NAME, get_bstr_t(AFAPI_OUTPUT_HANDLERS_CATEGORYNAME),
			VARIANT_TRUE, gRequiredInterfaces.ulCount, gRequiredInterfaces.pIIDs);

		if (vbDirty == VARIANT_TRUE)
		{
			// update display of the output handler check box and edit control
			updateCheckBoxAndEditControlBasedOnObject(m_ipRuleSet->GlobalOutputHandler,
				m_bOutputHandler, IDC_CHECK_OUTPUT_HANDLER, m_zOutputHandlerDescription);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16041")
}
//-------------------------------------------------------------------------------------------------
int CRuleSetEditor::OnMouseActivate(CWnd* pDesktopWnd, UINT nHitTest, UINT message)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		// Interested in Right click messages
		if ((nHitTest == HTCLIENT) && (message == WM_RBUTTONDOWN))
		{
			LRESULT	result = 0;

			// Get positions of controls of interest
			CRect	rectCombo;
			CRect	rectRules;
			m_comboAttr.GetWindowRect( &rectCombo );
			m_listRules.GetWindowRect( &rectRules );

			// Get mouse position
			CPoint	point;
			GetCursorPos( &point );

			// Check to see if right-click in Attributes combo box
			if ((point.x >= rectCombo.left) && (point.x <= rectCombo.right) &&
				(point.y >= rectCombo.top) && (point.y <= rectCombo.bottom))
			{
				// Create and manage a context menu for Attributes combo box
				OnRclickComboAttributes( NULL, &result );
			}
			// Check to see if right-click in Rules list
			else if ((point.x >= rectRules.left) && (point.x <= rectRules.right) &&
				(point.y >= rectRules.top) && (point.y <= rectRules.bottom))
			{
				// Create and manage a context menu for Rules list
				OnRclickListRules( NULL, &result );
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05474")

	return CWnd::OnMouseActivate(pDesktopWnd, nHitTest, message);
}
//-------------------------------------------------------------------------------------------------
BOOL CRuleSetEditor::OnSetCursor(CWnd* pWnd, UINT nHitTest, UINT message)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		// Interested in Right click messages
		if ((nHitTest == HTCLIENT) && (message == WM_RBUTTONDOWN))
		{
			LRESULT	result = 0;

			// Get positions of controls of interest
			CRect	rectIV;
			CRect	rectSplitter;
			CRect	rectPreprocessor;
			CRect	rectOH;
			GetDlgItem( IDC_EDIT_IV )->GetWindowRect( &rectIV );
			GetDlgItem( IDC_EDIT_ATTRIBUTE_SPLITTER )->GetWindowRect( &rectSplitter );
			GetDlgItem( IDC_EDIT_PREPROCESSOR )->GetWindowRect( &rectPreprocessor );
			GetDlgItem( IDC_EDIT_OH )->GetWindowRect( &rectOH );

			// Get mouse position
			CPoint	point;
			GetCursorPos( &point );

			// Check to see if right-click in Input Validator text
			if ((point.x >= rectIV.left) && (point.x <= rectIV.right) &&
				(point.y >= rectIV.top) && (point.y <= rectIV.bottom))
			{
				// Create and manage a context menu for Input Validator
				OnRclickValidatorText( NULL, &result );
			}
			// Check to see if right-click in Attribute Splitter text
			else if ((point.x >= rectSplitter.left) && (point.x <= rectSplitter.right) &&
				(point.y >= rectSplitter.top) && (point.y <= rectSplitter.bottom))
			{
				// Create and manage a context menu for Attribute Splitter
				OnRclickSplitterText( NULL, &result );
			}
			// Check to see if right-click in Document Preprocessor text
			else if ((point.x >= rectPreprocessor.left) && (point.x <= rectPreprocessor.right) &&
				(point.y >= rectPreprocessor.top) && (point.y <= rectPreprocessor.bottom))
			{
				// Create and manage a context menu for Document Preprocessor
				OnRclickPreprocessorText( NULL, &result );
			}
			// Check to see if right-click in Output Handler text
			else if ((point.x >= rectOH.left) && (point.x <= rectOH.right) &&
				(point.y >= rectOH.top) && (point.y <= rectOH.bottom))
			{
				// Create and manage a context menu for Output Handler
				OnRclickHandlerText( NULL, &result );
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05475")

	return CWnd::OnSetCursor(pWnd, nHitTest, message);
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnClose() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		// if checkModification() returns false, it means the user
		// want to cancel current task, i.e. closing the rule set editor
		if (!checkModification()) 
		{
			return;
		}

		// Clear the ClipboardManager
		m_ipClipboardMgr->Clear();
		
		// we are exiting the application at the user's request, regardless
		// of whether they are saving the current ruleset or not.  So,
		// it is OK to delete the recovery file
		m_FRM.deleteRecoveryFile();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04808")
	
	CDialog::OnClose();

	// this function is usually called by clicking the X on the
	// top-right corner of the dialog to close the dialog.
	// Since we leave OnCancel() implementation empty, this is
	// the place to call CDialog::OnCancel() to close the window.
	CDialog::OnCancel();
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnCancel()
{
	// purpose of having this function here is to prevent
	// user from closing the dialog by pressing Escape key
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnOK()
{
	// purpose of having this function here is to prevent
	// user from closing the dialog by pressing Enter key
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnTimer(UINT nIDEvent) 
{
	try
	{
		// if we received a timer event to perform an auto-save, do the auto-save
		// as long as the currently loaded ruleset is not encrypted and is licensed to save
		if (nIDEvent == giAUTO_SAVE_TIMERID && m_ipRuleSet->CanSave == VARIANT_TRUE)
		{
			// NOTE: we are passing VARIANT_TRUE as the second argument
			// here because we don't want the internal dirty flag to be
			// effected by this SaveTo() call.
			m_ipRuleSet->SaveTo(get_bstr_t(m_FRM.getRecoveryFileName().c_str()),
				VARIANT_FALSE);
		}

		CDialog::OnTimer(nIDEvent);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05506")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnItemchangedListRules(NMHDR* pNMHDR, LRESULT* pResult) 
{
	try
	{
		setButtonStates();
		
		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06846")
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnSize(UINT nType, int cx, int cy) 
{
	CDialog::OnSize(nType, cx, cy);
	
	try
	{
		static bool bInitialized = false;
		if (!bInitialized)
		{
			bInitialized = true;
			return;
		}
		
		// take care of the state of the rule tester dialog if it's visible
		if (m_apRuleTesterDlg.get() && m_apRuleTesterDlg->IsWindowVisible())
		{
			WINDOWPLACEMENT placement1, placement2;
			GetWindowPlacement(&placement1);
			if (placement1.showCmd == SW_SHOWMINIMIZED)
			{
				// get the current state of the tester dlg
				m_apRuleTesterDlg->GetWindowPlacement(&placement2);
				// minimize tester dialog
				placement2.showCmd = SW_MINIMIZE;
				
				m_apRuleTesterDlg->SetWindowPlacement(&placement2);
			}
			else if (placement1.showCmd == SW_SHOWNORMAL)
			{
				// get the current state of the tester dlg
				m_apRuleTesterDlg->GetWindowPlacement(&placement2);
				// restore tester dialog
				placement2.showCmd = SW_RESTORE;
				m_apRuleTesterDlg->SetWindowPlacement(&placement2);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08055")
}
//--------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnSelectMRUMenu(UINT nID)
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{	
		if (nID >= ID_MRU_FILE1 && nID <= ID_MRU_FILE5)
		{
			// Get the current selected file index of MRU list
			int nCurrentSelectedFileIndex = nID - ID_MRU_FILE1;

			// Get file name string
			string strFileToOpen(ma_pMRUList->at(nCurrentSelectedFileIndex));

			openFile(strFileToOpen);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09654")
}
//--------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnFileProperties() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{	
		CRuleSetPropertiesDlg dialog(m_ipRuleSet);
		
		if (dialog.DoModal() == IDOK)
		{
			// update the status bar as the user may have changed settings
			setStatusBarText();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11332")
	
}
//-------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnBnClickedCheckDocumentPp()
{
	UpdateData( TRUE );

	// Retrieve the Preprocessor
	IObjectWithDescriptionPtr ipGDPP = m_ipRuleSet->GlobalDocPreprocessor;
	ASSERT_RESOURCE_ALLOCATION("ELI15510", ipGDPP != NULL);

	// Check what the checkbox is set to and then
	// Enable or Disable the PreProcessor accordingly
	if( m_bDocumentPP == TRUE )
	{
		ipGDPP->PutEnabled( VARIANT_TRUE );
	}
	else
	{
		ipGDPP->PutEnabled( VARIANT_FALSE );
	}
}
//--------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnBnClickedCheckOutputHandler()
{
	UpdateData( TRUE );

	// Retrieve the Output Handler
	IObjectWithDescriptionPtr ipOH = m_ipRuleSet->GlobalOutputHandler;
	ASSERT_RESOURCE_ALLOCATION("ELI15511", ipOH != NULL);

	// Check what the checkbox is set to and then
	// Enable or Disable the Output Handler accordingly
	if( m_bOutputHandler == TRUE )
	{
		ipOH->PutEnabled( VARIANT_TRUE );
	}
	else
	{
		ipOH->PutEnabled( VARIANT_FALSE );
	}
}
//--------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnBnClickedCheckInputValidator()
{
	UpdateData( TRUE );

	// Retrieve the Input Validator
	IObjectWithDescriptionPtr ipIV = m_ipInfo->InputValidator;
	ASSERT_RESOURCE_ALLOCATION("ELI15512", ipIV != NULL);

	// Check what the checkbox is set to and then
	// Enable or Disable the Input Validator accordingly
	if( m_bInputValidator == TRUE )
	{
		ipIV->PutEnabled( VARIANT_TRUE );
	}
	else
	{
		ipIV->PutEnabled( VARIANT_FALSE );
	}
}
//--------------------------------------------------------------------------------------------------
void CRuleSetEditor::OnBnClickedCheckAttSplitter()
{
	UpdateData( TRUE );

	// Retrieve the Splitter
	IObjectWithDescriptionPtr ipSplitter = m_ipInfo->AttributeSplitter;
	ASSERT_RESOURCE_ALLOCATION("ELI15513", ipSplitter != NULL);

	// Check what the checkbox is set to and then
	// Enable or Disable the Attribute Splitter accordingly
	if( m_bAttSplitter == TRUE )
	{
		ipSplitter->PutEnabled( VARIANT_TRUE );
	}
	else
	{
		ipSplitter->PutEnabled( VARIANT_FALSE );
	}
}
//--------------------------------------------------------------------------------------------------
