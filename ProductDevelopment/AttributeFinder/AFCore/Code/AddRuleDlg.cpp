//============================================================================
//
// COPYRIGHT (c) 2003 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	AddRuleDlg.cpp
//
// PURPOSE:	Implementation of CAddRuleDlg class
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//============================================================================

#include "stdafx.h"
#include "afcore.h"
#include "AddRuleDlg.h"
#include "AFCategories.h"
#include "RequiredInterfaces.h"

#include <TemporaryResourceOverride.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <COMUtils.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// Column widths
const int giENABLE_LIST_COLUMN = 0;
const int giDESC_LIST_COLUMN = 1;

const bstr_t gbstrATTRIBUTE_MODIFYING_RULE_DISPLAY_NAME = "Attribute Modifying Rule";
const bstr_t gbstrRULE_SPECIFIC_DOCUMENT_PREPROCESSOR_DISPLAY_NAME = "Rule-Specific Document Preprocessor";
const bstr_t gbstrRULE_SPECIFIC_OUTPUT_HANDLER_DISPLAY_NAME = "Rule-Specific Output Handler";

//-------------------------------------------------------------------------------------------------
// CAddRuleDlg dialog
//-------------------------------------------------------------------------------------------------
CAddRuleDlg::CAddRuleDlg(IClipboardObjectManagerPtr ipCBMgr,
						 UCLID_AFCORELib::IAttributeRulePtr ipRule, 
						 CWnd* pParent /*=__nullptr*/)
	: CDialog(CAddRuleDlg::IDD, pParent),
	m_eContextMenuCtrl(kNoControl),
	m_ipDocPreprocessor(__nullptr),
	m_bInitialized(false), 
	m_ipRule(ipRule),
	m_ipClipboardMgr(ipCBMgr),
	m_ipOutputHandler(__nullptr),
	m_wMgr(this, "")
{
	// create MiscUtils object
	m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
	ASSERT_RESOURCE_ALLOCATION("ELI16009", m_ipMiscUtils != __nullptr);

	//{{AFX_DATA_INIT(CAddRuleDlg)
	m_bApplyMod = FALSE;
	m_zDescription = _T("");
	m_zPrompt = _T("Select Attribute Finding rule");
	m_zPPDescription = _T("");
	m_zOHDescription = _T("");
	m_bIgnoreModErrors = FALSE;
	m_bIgnoreDocPPErrors = FALSE;
	m_bIgnoreOHErrors = FALSE;
	//}}AFX_DATA_INIT
}
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CAddRuleDlg)
	DDX_Control(pDX, IDC_STATIC_CONFIGURE, m_lblConfigure);
	DDX_Control(pDX, IDC_LIST_RULES, m_listRules);
	DDX_Control(pDX, IDC_COMBO_RULE, m_comboRule);
	DDX_Control(pDX, IDC_BTN_RULEUP, m_btnRuleUp);
	DDX_Control(pDX, IDC_BTN_RULEDOWN, m_btnRuleDown);
	DDX_Control(pDX, IDC_BTN_DELRULE, m_btnDelRule);
	DDX_Control(pDX, IDC_BTN_CONRULE2, m_btnConRule2);
	DDX_Control(pDX, IDC_BTN_CONRULE, m_btnConRule);
	DDX_Control(pDX, IDC_BTN_ADDRULE, m_btnAddRule);
	DDX_Control(pDX, IDC_BTN_SELECTPP, m_btnSelectPreprocessor);
	DDX_Control(pDX, IDC_BTN_SELECT_OH, m_btnSelectOutputHandler);
	DDX_Check(pDX, IDC_CHECK_MODIFY, m_bApplyMod);
	DDX_Check(pDX, IDC_CHECK_AFRULE_DOC_PP, m_bDocPP);
	DDX_Check(pDX, IDC_CHECK_AFRULE_OH, m_bOH);
	DDX_Text(pDX, IDC_EDIT_DESC, m_zDescription);
	DDX_Text(pDX, IDC_STATIC_DESC, m_zPrompt);
	DDX_Text(pDX, IDC_EDIT_PREPROCESSOR, m_zPPDescription);
	DDX_Text(pDX, IDC_EDIT_OUTPUTHANDLER, m_zOHDescription);
	DDX_Check(pDX, IDC_CHECK_IGNORE_PP_ERRORS, m_bIgnoreDocPPErrors);
	DDX_Check(pDX, IDC_CHECK_IGNORE_MODIFIER_ERRORS, m_bIgnoreModErrors);
	DDX_Check(pDX, IDC_CHECK_IGNORE_OH_ERRORS, m_bIgnoreOHErrors);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CAddRuleDlg, CDialog)
	//{{AFX_MSG_MAP(CAddRuleDlg)
	ON_BN_CLICKED(IDC_BTN_CONRULE2, OnBtnConfigureRule2)
	ON_BN_CLICKED(IDC_BTN_ADDRULE, OnBtnAddRule)
	ON_BN_CLICKED(IDC_BTN_DELRULE, OnBtnDeleteRule)
	ON_BN_CLICKED(IDC_BTN_CONRULE, OnBtnConfigureRule)
	ON_BN_CLICKED(IDC_BTN_RULEUP, OnBtnRuleUp)
	ON_BN_CLICKED(IDC_BTN_RULEDOWN, OnBtnRuleDown)
	ON_BN_CLICKED(IDC_BTN_SELECTPP, OnBtnSelectPreprocessor)
	ON_BN_CLICKED(IDC_BTN_SELECT_OH, OnBtnSelectOutputHandler)
	ON_BN_CLICKED(IDC_CHECK_MODIFY, OnCheckModify)
	ON_BN_CLICKED(IDC_CHECK_AFRULE_DOC_PP, &CAddRuleDlg::OnBnClickedCheckAfruleDocPp)
	ON_BN_CLICKED(IDC_CHECK_AFRULE_OH, &CAddRuleDlg::OnBnClickedCheckAFRuleOH)
	ON_CBN_SELCHANGE(IDC_COMBO_RULE, OnSelchangeComboRule)
	ON_NOTIFY(NM_CLICK, IDC_LIST_RULES, OnClickListRules)
	ON_NOTIFY(NM_DBLCLK, IDC_LIST_RULES, OnDblclkListRules)
	ON_WM_MOUSEACTIVATE()
	ON_NOTIFY(NM_RCLICK, IDC_LIST_RULES, OnRclickListRules)
	ON_NOTIFY(NM_RCLICK, IDC_EDIT_PREPROCESSOR, OnRclickEditPreprocessor)
	ON_NOTIFY(NM_RCLICK, IDC_EDIT_OUTPUTHANDLER, OnRclickEditOutputHandler)
	ON_COMMAND(ID_EDIT_CUT, OnEditCut)
	ON_COMMAND(ID_EDIT_COPY, OnEditCopy)
	ON_COMMAND(ID_EDIT_PASTE, OnEditPaste)
	ON_COMMAND(ID_EDIT_DELETE, OnEditDelete)
	ON_WM_SETCURSOR()
	ON_NOTIFY(LVN_ITEMCHANGED, IDC_LIST_RULES, OnItemchangedListRules)
	ON_EN_CHANGE(IDC_EDIT_DESC, OnChangeEditDesc)
	ON_WM_SIZE()
	ON_WM_GETMINMAXINFO()
	//}}AFX_MSG_MAP
	ON_STN_DBLCLK(IDC_EDIT_PREPROCESSOR, &CAddRuleDlg::OnDoubleClickDocumentPreprocessor)
	ON_STN_DBLCLK(IDC_EDIT_OUTPUTHANDLER, &CAddRuleDlg::OnDoubleClickOutputHandler)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// Public methods
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::SetPromptText(string strPrompt) 
{
	// Set the prompt text
	m_zPrompt = strPrompt.c_str();
}

//-------------------------------------------------------------------------------------------------
// CAddRuleDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CAddRuleDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		CDialog::OnInitDialog();
		
		// Save original client width/height
		CRect rectDlg;
		GetClientRect(rectDlg);
		m_nDefaultW = rectDlg.Width();
		m_nDefaultH = rectDlg.Height();
	
		// Save minimum window width/height
		GetWindowRect(rectDlg);
		m_nMinWidth = rectDlg.Width() * 4 / 5;
		m_nMinHeight = rectDlg.Height() * 4 / 5;

		// Set Up and Down bitmaps to buttons
		m_btnRuleUp.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_UP)));
		m_btnRuleDown.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_DOWN)));

		// Setup list control
		prepareList();

		// Populate the combo box
		populateCombo();

		// Set flag
		m_bInitialized = true;
		
		// Set the Document Preprocessor
		setPreprocessor();

		// Set the AttributeFindingRule
		setAFRule();

		// Set the description
		setDescription();

		// Set the AttributeModifyingRules and the checkbox
		setAMRules();

		// Set the Output Handler
		setOutputHandler();

		// Initialize the buttons' enabled/disabled states
		setButtonStates();

		// [FlexIDSCore:4777]
		GetDlgItem(IDC_COMBO_RULE)->SetFocus();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04404")

	return FALSE;  // return TRUE unless you set the focus to a control
	               // EXCEPTION: OCX Property Pages should return FALSE
}
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::OnBtnConfigureRule2() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		// Make sure an Attribute Finding Rule is selected
		ICopyableObjectPtr ipCopy = m_ipAFRule;
		if (ipCopy != __nullptr)
		{
			// Clone the rule
			UCLID_AFCORELib::IAttributeFindingRulePtr ipRule = ipCopy->Clone();
			ASSERT_RESOURCE_ALLOCATION("ELI28512", ipRule != __nullptr);

			// Create the ObjectPropertiesUI object
			IObjectPropertiesUIPtr	ipProperties( CLSID_ObjectPropertiesUI );

			// Show the UI
			if (ipProperties != __nullptr)
			{
				// Get combo box text
				int iIndex = m_comboRule.GetCurSel();
				CString	zText;
				m_comboRule.GetLBText( iIndex, zText );

				// Create prompt string
				string strTitle = string(LPCTSTR(zText))  + " settings";
				_bstr_t	bstrTitle( strTitle.c_str() );

				// Display the Property Page and check if the settings were applied
				if (asCppBool(ipProperties->DisplayProperties1(ipRule, bstrTitle)))
				{
					// Set the configured rule object to be the finding rule
					m_ipAFRule = ipRule;
				}

				// Check configured state of Attribute Finding Rule
				showReminder();
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04405")
}
//--------------------------------------------------------------------------------------------------
void CAddRuleDlg::clearListSelection()
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
void CAddRuleDlg::OnBtnAddRule() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		// Create the IObjectWithDescription 
		// without description and without object pointer
		IObjectWithDescriptionPtr	ipObject( CLSID_ObjectWithDescription );
		ASSERT_RESOURCE_ALLOCATION("ELI10303", ipObject != __nullptr);

		// Allow the user to select and configure the object
		VARIANT_BOOL vbResult = m_ipMiscUtils->AllowUserToSelectAndConfigureObject(ipObject, 
			"Attribute Modifier", get_bstr_t(AFAPI_VALUE_MODIFIERS_CATEGORYNAME), VARIANT_FALSE, 
			gRequiredInterfaces.ulCount, gRequiredInterfaces.pIIDs);

		// Check result
		if (vbResult == VARIANT_TRUE)
		{
			// Retrieve the description
			_bstr_t	bstrText = ipObject->GetDescription();
			CString	zText( (const char *)bstrText );

			// Get index of previously selected AMRule
			int iIndex = -1;
			int iNewIndex = -1;
			POSITION pos = m_listRules.GetFirstSelectedItemPosition();
			if (pos != __nullptr)
			{
				// Get index of first selection
				iIndex = m_listRules.GetNextSelectedItem( pos );
			}

			// Get list count
			int iCount = m_listRules.GetItemCount();

			if (iIndex > -1)
			{
				// [FlexIDSCore:4918]
				// Add new rule after existing rule rather than before it.
				iIndex++;

				// Add a blank object to the list with no text.
				iNewIndex = m_listRules.InsertItem( iIndex, "" );
			}
			else
			{
				// Add a blank object to the bottom of the list with no text.
				iNewIndex = m_listRules.InsertItem( iCount, "");
			}
			// Put the text into the box.
			m_listRules.SetItemText( iNewIndex, giDESC_LIST_COLUMN, 
					LPCTSTR(zText) );

			// Set the checkbox based on the object's Enabled state.
			VARIANT_BOOL vbEnabled = ipObject->Enabled;
			if( vbEnabled == VARIANT_TRUE)
			{
				m_listRules.SetCheck(iNewIndex, TRUE);
			}
			else
			{
				m_listRules.SetCheck(iNewIndex, FALSE);
			}
			
			// Insert the Modifying Rule object-with-description into the vector
			m_ipAMRulesVector->Insert( iNewIndex, ipObject );

			// clear the previous selection if any
			clearListSelection();

			// Retain selection and focus
			m_listRules.SetItemState( iNewIndex, LVIS_SELECTED, LVIS_SELECTED );
			m_listRules.SetFocus();

			// Update the display
			UpdateData( FALSE );

			// Update button states
			setButtonStates();
		}		// end if description provided
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04406")
}
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::OnBtnDeleteRule() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// Check for current rule selection
		int iIndex = -1;
		POSITION pos = m_listRules.GetFirstSelectedItemPosition();
		if (pos != __nullptr)
		{
			// Get index of first selection
			iIndex = m_listRules.GetNextSelectedItem( pos );
		}

		// Check for multiple selection of Rules
		int iIndex2 = m_listRules.GetNextSelectedItem( pos );

		// Handle single-selection case
		int		iResult;
		CString	zPrompt;
		if (iIndex2 == -1)
		{
			// Retrieve current Rule description
			CString	zDescription = m_listRules.GetItemText( iIndex, 1 );

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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04407")
}
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::OnBtnConfigureRule() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		// Check for current rule selection
		int iIndex = -1;
		POSITION pos = m_listRules.GetFirstSelectedItemPosition();
		if (pos != __nullptr)
		{
			// Get index of first selection
			iIndex = m_listRules.GetNextSelectedItem( pos );
		}

		if (iIndex > -1)
		{
			// Retrieve current rule
			UCLID_COMUTILSLib::IObjectWithDescriptionPtr ipRule = 
				m_ipAMRulesVector->At(iIndex);
			ASSERT_RESOURCE_ALLOCATION("ELI16073", ipRule != __nullptr);

			// get the position of the button
			RECT rect;
			m_btnConRule.GetWindowRect(&rect);

			// show context menu and allow user to modify ipRule
			VARIANT_BOOL vbDirty = m_ipMiscUtils->HandlePlugInObjectCommandButtonClick(
				ipRule,	"Attribute Modifier", get_bstr_t(AFAPI_VALUE_MODIFIERS_CATEGORYNAME), 
				VARIANT_FALSE, 0, __nullptr, rect.right, rect.top);

			// check if ipRule was modified
			if (vbDirty == VARIANT_TRUE)
			{
				// update the list box based on the new rule
				replaceRuleInListAt(iIndex, ipRule);

				// ensure the modified rule is selected
				m_listRules.SetItemState(iIndex, LVIS_SELECTED, LVIS_SELECTED);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04408")
}
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::OnBtnRuleUp() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		// Check for current rule selection
		int iIndex = -1;
		POSITION pos = m_listRules.GetFirstSelectedItemPosition();
		if (pos != __nullptr)
		{
			// Get index of first selection
			iIndex = m_listRules.GetNextSelectedItem( pos );
		}

		// Selection cannot be at top of list
		if (iIndex > 0)
		{
			// Retrieve current Rule description
			CString	zDescription = m_listRules.GetItemText( iIndex, giDESC_LIST_COLUMN );

			// Retrieve current Rule information
			DWORD	dwInfo = m_listRules.GetItemData( iIndex );
			
			// Get current check state
			BOOL bChecked = m_listRules.GetCheck(iIndex);
			
			// Delete the item
			m_listRules.DeleteItem( iIndex );

			// Replace the item one row higher
			m_listRules.InsertItem( iIndex - 1, "" );

			// Restore the description
			m_listRules.SetItemText( iIndex -1 , giDESC_LIST_COLUMN, 
					LPCTSTR(zDescription) );

			// Restore the check state
			m_listRules.SetCheck(iIndex -1, bChecked);

			//////////////////////////
			// Update the Rules vector
			//////////////////////////
			// Swap the rules
			m_ipAMRulesVector->Swap( iIndex, iIndex - 1 );

			// Retain selection and focus
			m_listRules.SetItemState( iIndex - 1, LVIS_SELECTED, LVIS_SELECTED );
			m_listRules.SetFocus();

			// Update button states
			setButtonStates();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04409")
}
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::OnBtnRuleDown() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		// Check for current rule selection
		int iIndex = -1;
		POSITION pos = m_listRules.GetFirstSelectedItemPosition();
		if (pos != __nullptr)
		{
			// Get index of first selection
			iIndex = m_listRules.GetNextSelectedItem( pos );
		}

		// Selection cannot be at bottom of list
		int iCount = m_listRules.GetItemCount();
		if (iIndex < iCount - 1)
		{
			// Retrieve current Rule description
			CString	zDescription = m_listRules.GetItemText( iIndex, giDESC_LIST_COLUMN );

			// Retrieve current Rule information
			DWORD	dwInfo = m_listRules.GetItemData( iIndex );

			// Get current check state
			BOOL bChecked = m_listRules.GetCheck(iIndex);

			// Delete the item
			m_listRules.DeleteItem( iIndex );

			// Replace the item one row lower
			m_listRules.InsertItem( iIndex + 1, "" );

			// Restore the description
			m_listRules.SetItemText( iIndex +1 , giDESC_LIST_COLUMN, 
					LPCTSTR(zDescription) );

			// Restore the check state
			m_listRules.SetCheck(iIndex + 1, bChecked);

			//////////////////////////
			// Update the Rules vector
			//////////////////////////
			// Swap the rules
			m_ipAMRulesVector->Swap( iIndex, iIndex + 1 );

			// Retain selection and focus
			m_listRules.SetItemState( iIndex + 1, LVIS_SELECTED, LVIS_SELECTED );
			m_listRules.SetFocus();

			// Update button states
			setButtonStates();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04410")
}
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::OnBtnSelectPreprocessor() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// make sure the doc preprocessor is non-null
		if (m_ipDocPreprocessor == __nullptr)
		{
			setPreprocessor();
		}

		// get position of document preprocessor commands button
		RECT rect;
		m_btnSelectPreprocessor.GetWindowRect(&rect);

		// allow the user to select and configure the document preprocessor
		VARIANT_BOOL vbDirty = m_ipMiscUtils->HandlePlugInObjectCommandButtonClick(m_ipDocPreprocessor,
			"Rule-Specific Document Preprocessor", get_bstr_t(AFAPI_DOCUMENT_PREPROCESSORS_CATEGORYNAME),
			VARIANT_TRUE, gRequiredInterfaces.ulCount, gRequiredInterfaces.pIIDs, rect.right, rect.top);

		if (vbDirty == VARIANT_TRUE)
		{
			// update the preprocessor controls to reflect changes
			updatePreprocessorCheckBoxAndEditControl();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06085")
}
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::OnBtnSelectOutputHandler() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// make sure the output handler is non-null
		if (m_ipOutputHandler == __nullptr)
		{
			setOutputHandler();
		}

		// get position of output handler commands button
		RECT rect;
		m_btnSelectOutputHandler.GetWindowRect(&rect);

		// allow the user to select and configure the document preprocessor
		VARIANT_BOOL vbDirty = m_ipMiscUtils->HandlePlugInObjectCommandButtonClick(m_ipOutputHandler,
			"Rule-Specific Output Handler", get_bstr_t(AFAPI_OUTPUT_HANDLERS_CATEGORYNAME),
			VARIANT_TRUE, gRequiredInterfaces.ulCount, gRequiredInterfaces.pIIDs, rect.right, rect.top);

		if (vbDirty == VARIANT_TRUE)
		{
			// update the output handler controls to reflect changes
			updateOutputHandlerCheckBoxAndEditControl();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI40285")
}
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::OnClickListRules(NMHDR* pNMHDR, LRESULT* pResult) 
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
			// Get a pointer to the rule we checked / unchecked
			IObjectWithDescriptionPtr	ipObject;
			ipObject = m_ipAMRulesVector->At(iIndex);
			ASSERT_RESOURCE_ALLOCATION( "ELI13729", ipObject != __nullptr );

			// Get the old state of the checkbox
			BOOL bChecked = m_listRules.GetCheck(iIndex);

			// Enable the rule if the old state of the check box is unchecked
			ipObject->Enabled = !bChecked ? VARIANT_TRUE : VARIANT_FALSE;
		}
		
		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13728")
}
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::OnDblclkListRules(NMHDR* pNMHDR, LRESULT* pResult) 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		// check for current rule selection
		POSITION pos = m_listRules.GetFirstSelectedItemPosition();
		if (pos != __nullptr)
		{
			// get index of first selection
			int iIndex = m_listRules.GetNextSelectedItem( pos );
		
			if (iIndex > -1)
			{
				// retrieve current rule
				IObjectWithDescriptionPtr ipRule = m_ipAMRulesVector->At(iIndex);
				ASSERT_RESOURCE_ALLOCATION("ELI16037", ipRule != __nullptr);

				// handle the double click
				VARIANT_BOOL vbDirty = m_ipMiscUtils->HandlePlugInObjectDoubleClick(ipRule,
					gbstrATTRIBUTE_MODIFYING_RULE_DISPLAY_NAME,	get_bstr_t(AFAPI_VALUE_MODIFIERS_CATEGORYNAME), 
					VARIANT_TRUE, gRequiredInterfaces.ulCount, gRequiredInterfaces.pIIDs);

				if (vbDirty == VARIANT_TRUE)
				{
					// update the modified rule
					replaceRuleInListAt(iIndex, ipRule);
				}
			}
		}
	
		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04412")
}
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::OnSelchangeComboRule() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		UpdateData( TRUE );

		// Get index of selection
		int iIndex = m_comboRule.GetCurSel();

		// Retrieve name of selected Value Finding Rule
		CString	zAFRuleName;
		m_comboRule.GetLBText( iIndex, zAFRuleName );

		// Store the AF Rule
		m_ipAFRule = getObjectFromName( LPCTSTR(zAFRuleName) );

		// Check configured state of Attribute Finding Rule
		showReminder();

		// Update enabled/disabled button states
		setButtonStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04413")
}
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::OnItemchangedListRules(NMHDR* pNMHDR, LRESULT* pResult) 
{
	try
	{			
		// update button states
		setButtonStates();

		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06847")
}
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::OnCheckModify() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		UpdateData(TRUE);

		// Update enabled/disabled button states
		setButtonStates();

		// set focus on the list box if this check is checked
		if (m_bApplyMod)
		{
			m_listRules.SetFocus();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04414")
}
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::OnOK() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{	
		// Retrieve latest text
		UpdateData( TRUE );

		// First check to see that if the Attribute Finding Rule needs to be 
		// configured, it has been configured
		IMustBeConfiguredObjectPtr	ipConfiguredObj = m_ipAFRule;
		if (ipConfiguredObj != __nullptr)
		{
			// Has object been configured yet?
			if (ipConfiguredObj->IsConfigured() == VARIANT_FALSE)
			{
				MessageBox("Object has not been configured completely.  Please specify all required properties.", "Configuration");
				// Not configured, go directly to Configuration dialog
				OnBtnConfigureRule2();

				// Check dialog result by repeating the test
				if (ipConfiguredObj->IsConfigured() == VARIANT_FALSE)
				{
					// User cancelled from configure, just skip rest of OnOK
					return;
				}
			}
			// Else AF Rule IS configured and
			// remaining code should execute
		}
		// Else AF Rule does NOT require configuration and
		// remaining code should execute

		/////////////////////////
		// Check description text
		/////////////////////////
		if (m_zDescription.IsEmpty())
		{
			// No description available, get index of selection
			int iIndex = m_comboRule.GetCurSel();
			if (iIndex > -1)
			{
				// Default description to component description
				m_comboRule.GetLBText( iIndex, m_zDescription );

				UpdateData( FALSE );
			}
		}

		////////////////////
		// Save the settings
		////////////////////
		if (m_ipRule != __nullptr)
		{
			// Store the rule description
			_bstr_t	bstrDesc = LPCTSTR(m_zDescription);
			m_ipRule->PutDescription( bstrDesc );

			// Store the Attribute Finding Rule
			m_ipRule->PutAttributeFindingRule( m_ipAFRule );

			// Store the collection of Attribute Modifying Rule Infos
			m_ipRule->PutAttributeModifyingRuleInfos( m_ipAMRulesVector );

			// if there's no modifying rule, set to false
			bool bApply = m_bApplyMod && m_ipAMRulesVector->Size() > 0;
			m_ipRule->ApplyModifyingRules = bApply ? VARIANT_TRUE :VARIANT_FALSE;

			// Store the Document Preprocessor
			m_ipRule->RuleSpecificDocPreprocessor = m_ipDocPreprocessor;

			// Store the Output Handler
			m_ipRule->RuleSpecificOutputHandler = m_ipOutputHandler;
			
			m_ipRule->IgnorePreprocessorErrors = asVariantBool(m_bIgnoreDocPPErrors);
			m_ipRule->IgnoreModifierErrors = asVariantBool(m_bIgnoreModErrors);
			m_ipRule->IgnoreOutputHandlerErrors = asVariantBool(m_bIgnoreOHErrors);
		}

		CDialog::OnOK();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04415")
}
//-------------------------------------------------------------------------------------------------
int CAddRuleDlg::OnMouseActivate(CWnd* pDesktopWnd, UINT nHitTest, UINT message)
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
			CRect	rectRules;
			m_listRules.GetWindowRect( &rectRules );

			// Get mouse position
			CPoint	point;
			GetCursorPos( &point );

			// Check to see if right-click in Rules list
			if ((point.x >= rectRules.left) && (point.x <= rectRules.right) &&
				(point.y >= rectRules.top) && (point.y <= rectRules.bottom))
			{
				// Create and manage a context menu for Rules list
				OnRclickListRules( NULL, &result );
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05516")

	return CWnd::OnMouseActivate(pDesktopWnd, nHitTest, message);
}
//-------------------------------------------------------------------------------------------------
BOOL CAddRuleDlg::OnSetCursor(CWnd* pWnd, UINT nHitTest, UINT message)
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
			CRect	rectPre, rectOH;
			GetDlgItem( IDC_EDIT_PREPROCESSOR )->GetWindowRect( &rectPre );
			GetDlgItem( IDC_EDIT_OUTPUTHANDLER )->GetWindowRect( &rectOH );

			// Get mouse position
			CPoint	point;
			GetCursorPos( &point );

			// Check to see if right-click in Preprocessor edit box
			if ((point.x >= rectPre.left) && (point.x <= rectPre.right) &&
				(point.y >= rectPre.top) && (point.y <= rectPre.bottom))
			{
				// Create and manage a context menu for Preprocessor
				OnRclickEditPreprocessor( NULL, &result );
			}
			// Check to see if right-click in Output Handler edit box
			else if ((point.x >= rectOH.left) && (point.x <= rectOH.right) &&
				(point.y >= rectOH.top) && (point.y <= rectOH.bottom))
			{
				// Create and manage a context menu for Output Handler
				OnRclickEditOutputHandler( NULL, &result );
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06128")

	return CWnd::OnSetCursor(pWnd, nHitTest, message);
}
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::OnRclickListRules(NMHDR* pNMHDR, LRESULT* pResult) 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// Check for current rule selection
		if (pNMHDR)
		{
			int iIndex = -1;
			POSITION pos = m_listRules.GetFirstSelectedItemPosition();
			if (pos != __nullptr)
			{
				// Get index of first selection
				iIndex = m_listRules.GetNextSelectedItem( pos );
			}
			
			// Set the flag
			m_eContextMenuCtrl = kRulesList;
			
			// Load the context menu
			CMenu menu;
			menu.LoadMenu( IDR_MNU_CONTEXT );
			CMenu *pContextMenu = menu.GetSubMenu( 0 );
			
			//////////////////////////
			// Enable or disable items
			//////////////////////////
			UINT nEnable = (MF_BYCOMMAND | MF_ENABLED);
			UINT nDisable = (MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
			
			// enable/disable context menu items properly
			pContextMenu->EnableMenuItem(ID_EDIT_CUT, iIndex == -1 ? nDisable : nEnable);
			pContextMenu->EnableMenuItem(ID_EDIT_COPY, iIndex == -1 ? nDisable : nEnable );
			pContextMenu->EnableMenuItem(ID_EDIT_DELETE, iIndex == -1 ? nDisable : nEnable );
			
			// Check Clipboard object type
			// changed as per P16 #2551 JDS - Only allow pasting of AttributeModifyingRules
			// check not only if is it a vector of ObjectsWithDescription, but also
			// each of those objects are AttributeModifyingRules
			// Only interested in enabling or disabling the paste menu and don't want an exception displayed
			// so added the try catch block to remove the display of the exception when displaying the
			// context menu
			// https://extract.atlassian.net/browse/ISSUE-13155
			UINT nEnablePaste = nDisable;
			try
			{	
				if (m_ipClipboardMgr->IUnknownVectorIsOWDOfType(IID_IAttributeModifyingRule))
				{
					nEnablePaste = nEnable;
				}
				else if (m_ipClipboardMgr->ObjectIsTypeWithDescription( 
					IID_IAttributeModifyingRule ))
				{
					// Object is a single ObjectWithDescription item
					// We expect the embedded object to be an AM Rule
					nEnablePaste = nEnable;
				}
			}
			catch(...)
			{
				// Eat the exception
			}

			pContextMenu->EnableMenuItem( ID_EDIT_PASTE, nEnablePaste );
			
			// Map the point to the correct position
			CPoint	point;
			GetCursorPos( &point );
			
			// Display and manage the context menu
			pContextMenu->TrackPopupMenu( TPM_LEFTALIGN | TPM_LEFTBUTTON | TPM_RIGHTBUTTON, 
				point.x, point.y, this );
		}

		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05517")
}
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::OnRclickEditPreprocessor(NMHDR* pNMHDR, LRESULT* pResult) 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// Load the context menu
		CMenu menu;
		menu.LoadMenu( IDR_MNU_CONTEXT );
		CMenu *pContextMenu = menu.GetSubMenu( 0 );

		// Clear the flag
		m_eContextMenuCtrl = kPreprocessor;

		//////////////////////////
		// Enable or disable items
		//////////////////////////
		UINT nEnable = (MF_BYCOMMAND | MF_ENABLED);
		UINT nDisable = (MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);

		if (m_ipDocPreprocessor == __nullptr)
		{
			// No Rule defined, therefore no Preprocessor
			pContextMenu->EnableMenuItem(ID_EDIT_CUT, nDisable);
			pContextMenu->EnableMenuItem(ID_EDIT_COPY, nDisable);
			pContextMenu->EnableMenuItem(ID_EDIT_DELETE, nDisable);
		}
		else
		{
			// Rule defined, now retrieve the Preprocessor description
			CString	zDesc;
			zDesc = (char *)m_ipDocPreprocessor->Description;
			// Preprocessor is not defined
			pContextMenu->EnableMenuItem(ID_EDIT_CUT, zDesc.IsEmpty() ? nDisable : nEnable);
			pContextMenu->EnableMenuItem(ID_EDIT_COPY, zDesc.IsEmpty() ? nDisable : nEnable);
			pContextMenu->EnableMenuItem(ID_EDIT_DELETE, zDesc.IsEmpty() ? nDisable : nEnable);
			
			// Check Clipboard object type
			if (m_ipClipboardMgr->ObjectIsTypeWithDescription( 
				IID_IDocumentPreprocessor ))
			{
				// Object is an ObjectWithDescription item
				// where the object is a Document Preprocessor
				pContextMenu->EnableMenuItem( ID_EDIT_PASTE, nEnable );
			}
			else
			{
				// Object is not an ObjectWithDescription item
				// where the object is a Document Preprocessor
				pContextMenu->EnableMenuItem( ID_EDIT_PASTE, nDisable );
			}
		}

		// Map the point to the correct position
		CPoint	point;
		GetCursorPos(&point);
		
		// Display and manage the context menu
		pContextMenu->TrackPopupMenu( TPM_LEFTALIGN | TPM_LEFTBUTTON | TPM_RIGHTBUTTON, 
			point.x, point.y, this );

		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06086")
}
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::OnRclickEditOutputHandler(NMHDR* pNMHDR, LRESULT* pResult) 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// Load the context menu
		CMenu menu;
		menu.LoadMenu( IDR_MNU_CONTEXT );
		CMenu *pContextMenu = menu.GetSubMenu( 0 );

		// Clear the flag
		m_eContextMenuCtrl = kOutputHandler;

		//////////////////////////
		// Enable or disable items
		//////////////////////////
		UINT nEnable = (MF_BYCOMMAND | MF_ENABLED);
		UINT nDisable = (MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);

		if (m_ipOutputHandler == __nullptr)
		{
			// No Rule defined, therefore no OutputHandler
			pContextMenu->EnableMenuItem(ID_EDIT_CUT, nDisable);
			pContextMenu->EnableMenuItem(ID_EDIT_COPY, nDisable);
			pContextMenu->EnableMenuItem(ID_EDIT_DELETE, nDisable);
		}
		else
		{
			// Rule defined, now retrieve the OutputHandler description
			CString	zDesc;
			zDesc = (char *)m_ipOutputHandler->Description;
			// OutputHandler is not defined
			pContextMenu->EnableMenuItem(ID_EDIT_CUT, zDesc.IsEmpty() ? nDisable : nEnable);
			pContextMenu->EnableMenuItem(ID_EDIT_COPY, zDesc.IsEmpty() ? nDisable : nEnable);
			pContextMenu->EnableMenuItem(ID_EDIT_DELETE, zDesc.IsEmpty() ? nDisable : nEnable);
			
			// Check Clipboard object type
			if (m_ipClipboardMgr->ObjectIsTypeWithDescription( 
				IID_IOutputHandler ))
			{
				// Object is an ObjectWithDescription item
				// where the object is a Document OutputHandler
				pContextMenu->EnableMenuItem( ID_EDIT_PASTE, nEnable );
			}
			else
			{
				// Object is not an ObjectWithDescription item
				// where the object is a Document OutputHandler
				pContextMenu->EnableMenuItem( ID_EDIT_PASTE, nDisable );
			}
		}

		// Map the point to the correct position
		CPoint	point;
		GetCursorPos(&point);
		
		// Display and manage the context menu
		pContextMenu->TrackPopupMenu( TPM_LEFTALIGN | TPM_LEFTBUTTON | TPM_RIGHTBUTTON, 
			point.x, point.y, this );

		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI39301")
}
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::OnDoubleClickDocumentPreprocessor()
{
	try
	{
		// make sure the doc preprocessor is non-null
		if (m_ipDocPreprocessor == __nullptr)
		{
			setPreprocessor();
		}

		// update m_upDocPreprocessor based on user input
		VARIANT_BOOL vbDirty = m_ipMiscUtils->HandlePlugInObjectDoubleClick(
			m_ipDocPreprocessor, gbstrRULE_SPECIFIC_DOCUMENT_PREPROCESSOR_DISPLAY_NAME,
			get_bstr_t(AFAPI_DOCUMENT_PREPROCESSORS_CATEGORYNAME), 
			VARIANT_TRUE, gRequiredInterfaces.ulCount, gRequiredInterfaces.pIIDs);

		// check if m_upDocPreprocessor was modified
		if (vbDirty == VARIANT_TRUE)
		{
			// refresh the preprocessor check box and edit control to reflect changes
			updatePreprocessorCheckBoxAndEditControl();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16042")
}
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::OnDoubleClickOutputHandler()
{
	try
	{
		// make sure the doc OutputHandler is non-null
		if (m_ipOutputHandler == __nullptr)
		{
			setOutputHandler();
		}

		// update m_upOutputHandler based on user input
		VARIANT_BOOL vbDirty = m_ipMiscUtils->HandlePlugInObjectDoubleClick(
			m_ipOutputHandler, gbstrRULE_SPECIFIC_OUTPUT_HANDLER_DISPLAY_NAME,
			get_bstr_t(AFAPI_OUTPUT_HANDLERS_CATEGORYNAME), 
			VARIANT_TRUE, gRequiredInterfaces.ulCount, gRequiredInterfaces.pIIDs);

		// check if m_upOutputHandler was modified
		if (vbDirty == VARIANT_TRUE)
		{
			// refresh the OutputHandler check box and edit control to reflect changes
			updateOutputHandlerCheckBoxAndEditControl();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI39302")
}
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::OnChangeEditDesc() 
{
	UpdateData(TRUE);
}
//--------------------------------------------------------------------------------------------------
void CAddRuleDlg::OnGetMinMaxInfo(MINMAXINFO* lpMMI)
{
	lpMMI->ptMinTrackSize.x = m_nMinWidth;
	lpMMI->ptMinTrackSize.y = m_nMinHeight;
	CDialog::OnGetMinMaxInfo(lpMMI);
}
//--------------------------------------------------------------------------------------------------
void CAddRuleDlg::OnSize(UINT nType, int cx, int cy) 
{
	try
	{

		CDialog::OnSize(nType, cx, cy);

		if (m_bInitialized)
		{
			// Move/resize controls anchored all
			m_wMgr.moveAnchoredAll(*GetDlgItem(IDC_LIST_RULES), m_nDefaultW, m_nDefaultH, FALSE);

			// Move/resize controls anchored top, left right
			m_wMgr.moveAnchoredTopLeftRight(*GetDlgItem(IDC_EDIT_PREPROCESSOR), m_nDefaultW, m_nDefaultH, FALSE);
			m_wMgr.moveAnchoredTopLeftRight(*GetDlgItem(IDC_EDIT_DESC), m_nDefaultW, m_nDefaultH, FALSE);
			m_wMgr.moveAnchoredTopLeftRight(*GetDlgItem(IDC_COMBO_RULE), m_nDefaultW, m_nDefaultH, FALSE);

			// Move controls anchored top and right
			m_wMgr.moveAnchoredTopRight(*GetDlgItem(IDC_BTN_SELECTPP), m_nDefaultW, m_nDefaultH, FALSE);
			m_wMgr.moveAnchoredTopRight(*GetDlgItem(IDC_BTN_CONRULE2), m_nDefaultW, m_nDefaultH, FALSE);
			m_wMgr.moveAnchoredTopRight(*GetDlgItem(IDC_BTN_ADDRULE), m_nDefaultW, m_nDefaultH, FALSE);
			m_wMgr.moveAnchoredTopRight(*GetDlgItem(IDC_BTN_CONRULE), m_nDefaultW, m_nDefaultH, FALSE);
			m_wMgr.moveAnchoredTopRight(*GetDlgItem(IDC_BTN_DELRULE), m_nDefaultW, m_nDefaultH, FALSE);
			m_wMgr.moveAnchoredTopRight(*GetDlgItem(IDC_BTN_RULEDOWN), m_nDefaultW, m_nDefaultH, FALSE);
			m_wMgr.moveAnchoredTopRight(*GetDlgItem(IDC_BTN_RULEUP), m_nDefaultW, m_nDefaultH, FALSE);

			// Move controls anchored bottom and left
			m_wMgr.moveAnchoredBottomLeft(*GetDlgItem(IDC_CHECK_IGNORE_MODIFIER_ERRORS), m_nDefaultW, m_nDefaultH, FALSE);
			m_wMgr.moveAnchoredBottomLeft(*GetDlgItem(IDC_STATIC_USE_OH), m_nDefaultW, m_nDefaultH, FALSE);
			m_wMgr.moveAnchoredBottomLeft(*GetDlgItem(IDC_CHECK_AFRULE_OH), m_nDefaultW, m_nDefaultH, FALSE);
			m_wMgr.moveAnchoredBottomLeft(*GetDlgItem(IDC_CHECK_IGNORE_OH_ERRORS), m_nDefaultW, m_nDefaultH, FALSE);

			// Move controls anchored bottom, left and right
			m_wMgr.moveAnchoredBottomLeftRight(*GetDlgItem(IDC_EDIT_OUTPUTHANDLER), m_nDefaultW, m_nDefaultH, FALSE);
			m_wMgr.moveAnchoredBottomLeftRight(*GetDlgItem(IDC_STATIC_CONFIGURE), m_nDefaultW, m_nDefaultH, FALSE);

			// Move controls anchored bottom and right
			m_wMgr.moveAnchoredBottomRight(*GetDlgItem(IDC_BTN_SELECT_OH), m_nDefaultW, m_nDefaultH, FALSE);
			m_wMgr.moveAnchoredBottomRight(*GetDlgItem(IDOK), m_nDefaultW, m_nDefaultH, FALSE);
			m_wMgr.moveAnchoredBottomRight(*GetDlgItem(IDCANCEL), m_nDefaultW, m_nDefaultH, FALSE);

			// Update default values
			CRect rectDlg;
			GetClientRect(rectDlg);
			m_nDefaultW = rectDlg.Width();
			m_nDefaultH = rectDlg.Height();

			// Refresh window
			Invalidate();
			UpdateWindow();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05346");
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::deleteMarkedRules() 
{
	// Get list count
	int iCount = m_listRules.GetItemCount();
	if (iCount == 0)
	{
		return;
	}

	// Check vector of AM Rules
	if (m_ipAMRulesVector == __nullptr)
	{
		// Throw exception, AM Rules not defined
		throw UCLIDException( "ELI05521", "Attribute Modifying Rules are not defined!" );
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
			m_ipAMRulesVector->Remove( i );
		}
	}
}
//-------------------------------------------------------------------------------------------------
UCLID_AFCORELib::IAttributeFindingRulePtr CAddRuleDlg::getObjectFromName(string strName)
{
	// Check for the Prog ID in the map
	_bstr_t	bstrName( strName.c_str() );
	if (m_ipAFRulesMap->Contains( bstrName ) == VARIANT_TRUE)
	{
		// Retrieve the Prog ID string
		_bstr_t	bstrProgID = m_ipAFRulesMap->GetValue( bstrName );
		
		// Create the object
		ICategorizedComponentPtr ipComponent(asString(bstrProgID).c_str());

		return ipComponent;
	}
	
	// Not found in map, just return __nullptr
	return __nullptr;
}
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::markSelectedRules() 
{
	POSITION pos = m_listRules.GetFirstSelectedItemPosition();
	if (pos != __nullptr)
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
void CAddRuleDlg::populateCombo() 
{
	// Check Category Manager for Attribute Finding Rules
	UCLID_COMUTILSLib::ICategoryManagerPtr ipCategoryMgr( 
		__uuidof(CategoryManager) );

	if (ipCategoryMgr != __nullptr)
	{
		_bstr_t	bstrCategory( AFAPI_VALUE_FINDERS_CATEGORYNAME.c_str() );
		m_ipAFRulesMap = ipCategoryMgr->GetDescriptionToProgIDMap2( bstrCategory, 
			gRequiredInterfaces.ulCount, gRequiredInterfaces.pIIDs );

		// Add object names to the combo box
		CString	zName;
		long lSize = m_ipAFRulesMap->GetSize();
		UCLID_COMUTILSLib::IVariantVectorPtr ipKeys = m_ipAFRulesMap->GetKeys();
		for (int i = 0; i < lSize; i++)
		{
			// Add name to combo box
			zName = (char *)_bstr_t( ipKeys->GetItem( i ) );

			// The category manager will check licensing when building the cache; don't 
			// recheck here.  If a licensing issue was introduced since the last time
			// the cache was built, an exception will be presented upon object use.

			m_comboRule.AddString( zName );
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::prepareList() 
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
	long	lEWidth = 50;

	// Compute width for Description column
	CRect	rectList;
	m_listRules.GetClientRect( rectList );
	long	lDWidth = rectList.Width() - lEWidth;
	
	// Add 2 column headings to Rules list
	m_listRules.InsertColumn( giENABLE_LIST_COLUMN, "Enabled", 
		LVCFMT_LEFT, lEWidth, giENABLE_LIST_COLUMN );
	
	m_listRules.InsertColumn( giDESC_LIST_COLUMN, "Description", 
		LVCFMT_LEFT, lDWidth, giDESC_LIST_COLUMN );

	m_listRules.SetColumnWidth(giDESC_LIST_COLUMN, LVSCW_AUTOSIZE_USEHEADER);

}
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::setButtonStates()
{
	// Check modify flag
	CButton* pButton = (CButton *)GetDlgItem(IDC_CHECK_MODIFY);
	ASSERT_RESOURCE_ALLOCATION("ELI32945", pButton != __nullptr);
	bool bModifiersEnabled = (pButton->GetCheck() == BST_CHECKED);

	if (!bModifiersEnabled)
	{
		// Disable the buttons
		m_btnAddRule.EnableWindow( FALSE );
		m_btnDelRule.EnableWindow( FALSE );
		m_btnConRule.EnableWindow( FALSE );
		m_btnRuleUp.EnableWindow( FALSE );
		m_btnRuleDown.EnableWindow( FALSE );

		// Disable the list
		m_listRules.EnableWindow( FALSE );
	}
	else
	{
		// Enable the Add button and the list
		m_btnAddRule.EnableWindow( TRUE );
		m_listRules.EnableWindow( TRUE );

		int	iCount = m_listRules.GetItemCount();
		if (iCount == 0)
		{
			// Disable other Rule buttons
			m_btnDelRule.EnableWindow( FALSE );
			m_btnConRule.EnableWindow( FALSE );
			m_btnRuleUp.EnableWindow( FALSE );
			m_btnRuleDown.EnableWindow( FALSE );
		}
		else
		{
			// Next have to see if an item is selected
			int iIndex = -1;
			POSITION pos = m_listRules.GetFirstSelectedItemPosition();
			if (pos != __nullptr)
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
						// Check boundary conditions
						if (iIndex == iCount - 1)
						{
							// Cannot move last item down
							m_btnRuleDown.EnableWindow( FALSE );
						}
						else
						{
							m_btnRuleDown.EnableWindow( TRUE );
						}

						if (iIndex == 0)
						{
							// Cannot move first item up
							m_btnRuleUp.EnableWindow( FALSE );
						}
						else
						{
							m_btnRuleUp.EnableWindow( TRUE );
						}
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

	m_listRules.SetColumnWidth(giDESC_LIST_COLUMN, LVSCW_AUTOSIZE_USEHEADER);

	// if current selected value finding rule is a configurable object
	// enable the Configure button
	BOOL bEnable = FALSE;	
	ISpecifyPropertyPagesPtr ipPP(m_ipAFRule);
	if (ipPP != __nullptr) 
	{
		bEnable = TRUE;
	}
	else
	{
		IConfigurableObjectPtr ipConfigurableObject(m_ipAFRule);
		if (ipConfigurableObject != __nullptr)
		{
			bEnable = TRUE;
		}
	}
	m_btnConRule2.EnableWindow(bEnable);

	// Always enable the Select Document Preprocessor button
	m_btnSelectPreprocessor.EnableWindow( TRUE );

	CButton* pDocPPButton = (CButton *)GetDlgItem(IDC_CHECK_AFRULE_DOC_PP);
	ASSERT_RESOURCE_ALLOCATION("ELI32946", pDocPPButton != __nullptr);
	bool bDocPPE = (pDocPPButton->GetCheck() == BST_CHECKED);

	CButton* pIgnoreErrBtn = (CButton *)GetDlgItem(IDC_CHECK_IGNORE_PP_ERRORS);
	ASSERT_RESOURCE_ALLOCATION("ELI32947", pIgnoreErrBtn != __nullptr);
	pIgnoreErrBtn->EnableWindow(asMFCBool(bDocPPE));

	pIgnoreErrBtn = (CButton *)GetDlgItem(IDC_CHECK_IGNORE_MODIFIER_ERRORS);
	ASSERT_RESOURCE_ALLOCATION("ELI32948", pIgnoreErrBtn != __nullptr);
	pIgnoreErrBtn->EnableWindow(asMFCBool(bModifiersEnabled));

	// Always enable the Select Output Handler button
	m_btnSelectOutputHandler.EnableWindow( TRUE );

	CButton* pOHButton = (CButton *)GetDlgItem(IDC_CHECK_AFRULE_OH);
	ASSERT_RESOURCE_ALLOCATION("ELI39283", pOHButton != __nullptr);
	bool bOHE = (pOHButton->GetCheck() == BST_CHECKED);

	pIgnoreErrBtn = (CButton *)GetDlgItem(IDC_CHECK_IGNORE_OH_ERRORS);
	ASSERT_RESOURCE_ALLOCATION("ELI39284", pIgnoreErrBtn != __nullptr);
	pIgnoreErrBtn->EnableWindow(asMFCBool(bOHE));
}
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::setDescription() 
{
	// Retrieve description from AttributeRule
	if (m_ipRule != __nullptr)
	{
		_bstr_t	bstrDesc = m_ipRule->GetDescription();

		// Apply the string
		m_zDescription = (char *)bstrDesc;

		UpdateData( FALSE );
	}
}
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::setPreprocessor() 
{
	// Retrieve Document Preprocessor from Rule
	if (m_ipRule != __nullptr)
	{
		// Retrieve Object With Description
		m_ipDocPreprocessor = m_ipRule->RuleSpecificDocPreprocessor;
		ASSERT_RESOURCE_ALLOCATION( "ELI13947", m_ipDocPreprocessor != __nullptr)

		m_bIgnoreDocPPErrors = asMFCBool(m_ipRule->IgnorePreprocessorErrors);

		// update controls
		updatePreprocessorCheckBoxAndEditControl();
	}
}
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::setAFRule() 
{
	// Retrieve the Attribute Finding Rule
	if (m_ipRule != __nullptr)
	{
		m_ipAFRule = m_ipRule->GetAttributeFindingRule();
	}

	// Set the combo box selection
	if (m_ipAFRule != __nullptr)
	{
		// Get Component Description of the rule
		ICategorizedComponentPtr	ipComponent( m_ipAFRule );
		if (ipComponent != __nullptr)
		{
			_bstr_t	bstrCompDesc = ipComponent->GetComponentDescription();
			string	strActualName( bstrCompDesc );

			// Set the combo box selection
			m_comboRule.SelectString( -1, strActualName.c_str() );
		}
	}
	else
	{
		// Make sure that combo box has items
		if (m_comboRule.GetCount() > 0)
		{
			// Select first combo box item
			m_comboRule.SetCurSel( 0 );

			// Get the object from the name
			CString	zName;
			m_comboRule.GetLBText( 0, zName );

			m_ipAFRule = getObjectFromName( LPCTSTR(zName) );
			if (m_ipAFRule == __nullptr)
			{
				// Create and throw an exception
				UCLIDException	ue( "ELI04360", "Cannot find Attribute Finding Rule!");
				ue.addDebugInfo( "Rule name", LPCTSTR(zName) );
				throw ue;
			}
		}
	}

	// Check configured state of Attribute Finding Rule
	showReminder();
}
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::setAMRules() 
{
	// First make sure that the Attribute Rule is defined
	if (m_ipRule == __nullptr)
	{
		return;
	}

	// Retrieve the collection of Attribute Modifying Rule Infos
	// each = (AM Rule + Desc)
	m_ipAMRulesVector = m_ipRule->GetAttributeModifyingRuleInfos();

	int iCount = 0;
	int iIndex = -1;
	if (m_ipAMRulesVector != __nullptr)
	{
		// Get count
		iCount = m_ipAMRulesVector->Size();

		// Add rule descriptions to list
		for (int i = 0; i < iCount; i++)
		{
			// Retrieve this AMRule Info pointer
			IObjectWithDescriptionPtr ipObj = 
				m_ipAMRulesVector->At( i );

			if (ipObj != __nullptr)
			{
				// Get the description
				_bstr_t	bstrText = ipObj->GetDescription();

				// put the description in a usable format
				string strText = bstrText;
				CString zText = strText.c_str();

				// Add the item to the list
				m_listRules.InsertItem(i, "");
				m_listRules.SetItemText( i, giDESC_LIST_COLUMN, 
					LPCTSTR(zText) );

				// Set the checkbox accordingly
				VARIANT_BOOL vbEnabled = ipObj->GetEnabled();
				if(vbEnabled == VARIANT_TRUE)
				{
					m_listRules.SetCheck(i, TRUE);
				}
				else
				{
					m_listRules.SetCheck(i, FALSE);
				}
			}
		}
	}

	// Set the checkboxes
	m_bApplyMod = asMFCBool(m_ipRule->ApplyModifyingRules);
	m_bIgnoreModErrors = asMFCBool(m_ipRule->IgnoreModifierErrors);

	// Refresh the display
	UpdateData( FALSE );
}
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::setOutputHandler() 
{
	// Retrieve Output Handler from Rule
	if (m_ipRule != __nullptr)
	{
		// Retrieve Object With Description
		m_ipOutputHandler = m_ipRule->RuleSpecificOutputHandler;
		ASSERT_RESOURCE_ALLOCATION( "ELI39285", m_ipOutputHandler != __nullptr)

		m_bIgnoreOHErrors = asMFCBool(m_ipRule->IgnoreOutputHandlerErrors);

		// update controls
		updateOutputHandlerCheckBoxAndEditControl();
	}
}
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::showReminder() 
{
	bool bShow = false;

	// Check configured state of Attribute Finding Rule
	IMustBeConfiguredObjectPtr	ipConfiguredObj = m_ipAFRule;
	if (ipConfiguredObj != __nullptr)
	{
		// Has object been configured yet?
		if (ipConfiguredObj->IsConfigured() == VARIANT_FALSE)
		{
			// Else AF Rule IS NOT configured and
			// label should be shown
			bShow = true;
		}
		// Else AF Rule IS configured and
		// label should be hidden
	}
	// Else AF Rule does NOT require configuration and
	// label should be hidden

	// Show or hide the static text
	if (bShow)
	{
		m_lblConfigure.ShowWindow( SW_SHOW );
	}
	else
	{
		m_lblConfigure.ShowWindow( SW_HIDE );
	}
}
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::OnBnClickedCheckAfruleDocPp()
{
	UpdateData( TRUE );
	
	// Check what the checkbox is set to and then
	// Enable or Disable the Preprocessor accordingly
	if( m_bDocPP == TRUE )
	{
		m_ipDocPreprocessor->PutEnabled( VARIANT_TRUE );
	}
	else
	{
		m_ipDocPreprocessor->PutEnabled( VARIANT_FALSE );
	}

	setButtonStates();
}
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::OnBnClickedCheckAFRuleOH()
{
	UpdateData( TRUE );
	
	// Check what the checkbox is set to and then
	// Enable or Disable the Output Handler accordingly
	if( m_bOH == TRUE )
	{
		m_ipOutputHandler->PutEnabled( VARIANT_TRUE );
	}
	else
	{
		m_ipOutputHandler->PutEnabled( VARIANT_FALSE );
	}

	setButtonStates();
}
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::replaceRuleInListAt(int iIndex, IObjectWithDescriptionPtr ipNewRule)
{	
	// Delete old item
	m_listRules.DeleteItem( iIndex );

	// Insert the text into the list
	m_listRules.InsertItem( iIndex, "");

	// Get the data for the description
	_bstr_t bstrDesc = ipNewRule->Description;
	string	strBstrTemp = asString(bstrDesc);
	CString zDescription = strBstrTemp.c_str();

	// Restore the description
	m_listRules.SetItemText( iIndex , giDESC_LIST_COLUMN, 
		LPCTSTR(zDescription) );
	
	// Manage the check box
	m_listRules.SetCheck(iIndex, asMFCBool(ipNewRule->Enabled == VARIANT_TRUE) );
}
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::updatePreprocessorCheckBoxAndEditControl()
{
	// Use smart pointer
	IObjectWithDescriptionPtr ipPPWithDesc = m_ipDocPreprocessor;

	// Retrieve the Preprocessor description
	_bstr_t _bstrText = ipPPWithDesc->Description;
	
	// Update the displayed Preprocessor description
	m_zPPDescription = (const char *)_bstrText;

	// get check box
	CButton* pCheckBox = (CButton*) GetDlgItem( IDC_CHECK_AFRULE_DOC_PP );

	// validate check box
	if( !pCheckBox )
	{
		UCLIDException ue("ELI16047", "Unable to access check box resource.");
		throw ue;
	}

	if( m_zPPDescription.IsEmpty() )
	{
		//if the user selected 'None' disable and clear the checkbox
		pCheckBox->EnableWindow( FALSE );
		m_bDocPP = FALSE;
	}
	else
	{	
		// Enable the checkbox
		pCheckBox->EnableWindow( TRUE );

		// Set the checkbox based on the previous enabled value
		ipPPWithDesc->Enabled = m_ipDocPreprocessor->Enabled;
		m_bDocPP = asMFCBool(m_ipDocPreprocessor->Enabled == VARIANT_TRUE);

		// Enable the ignore errors check-box
		pCheckBox = (CButton*) GetDlgItem( IDC_CHECK_IGNORE_PP_ERRORS );
		pCheckBox->EnableWindow( TRUE );
	}
	UpdateData( FALSE );
}
//-------------------------------------------------------------------------------------------------
void CAddRuleDlg::updateOutputHandlerCheckBoxAndEditControl()
{
	// Use smart pointer
	IObjectWithDescriptionPtr ipOHWithDesc = m_ipOutputHandler;

	// Retrieve the Output Handler description
	_bstr_t _bstrText = ipOHWithDesc->Description;
	
	// Update the displayed Preprocessor description
	m_zOHDescription = (const char *)_bstrText;

	// get check box
	CButton* pCheckBox = (CButton*) GetDlgItem( IDC_CHECK_AFRULE_OH );

	// validate check box
	if( !pCheckBox )
	{
		UCLIDException ue("ELI39286", "Unable to access check box resource.");
		throw ue;
	}

	if( m_zOHDescription.IsEmpty() )
	{
		//if the user selected 'None' disable and clear the checkbox
		pCheckBox->EnableWindow( FALSE );
		m_bOH = FALSE;
	}
	else
	{	
		// Enable the checkbox
		pCheckBox->EnableWindow( TRUE );

		// Set the checkbox based on the previous enabled value
		ipOHWithDesc->Enabled = m_ipOutputHandler->Enabled;
		m_bOH = asMFCBool(m_ipOutputHandler->Enabled == VARIANT_TRUE);

		// Enable the ignore errors check-box
		pCheckBox = (CButton*) GetDlgItem( IDC_CHECK_IGNORE_OH_ERRORS );
		pCheckBox->EnableWindow( TRUE );
	}
	UpdateData( FALSE );
}
//-------------------------------------------------------------------------------------------------