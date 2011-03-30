// StringTokenizerSplitterPP.cpp : Implementation of CStringTokenizerSplitterPP
#include "stdafx.h"
#include "AFSplitters.h"
#include "StringTokenizerSplitterPP.h"

#include <UCLIDException.h>
#include <Prompt2Dlg.h>
#include <cpputil.h>
#include <comutils.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const int giATTRIBUTE_NAME_COLUMN = 0;
const int giATTRIBUTE_VALUE_COLUMN = 1;
const int giATTRIBUTE_NAME_COLUMN_WIDTH = 150;

const int NUM_OF_CHARS = 4096;

//-------------------------------------------------------------------------------------------------
// CStringTokenizerSplitterPP
//-------------------------------------------------------------------------------------------------
CStringTokenizerSplitterPP::CStringTokenizerSplitterPP()
{
	try
	{
		// Check licensing
		validateLicense();

		m_dwTitleID = IDS_TITLEStringTokenizerSplitterPP;
		m_dwHelpFileID = IDS_HELPFILEStringTokenizerSplitterPP;
		m_dwDocStringID = IDS_DOCSTRINGStringTokenizerSplitterPP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI07707")
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
bool CStringTokenizerSplitterPP::storeDelimiterField(
	UCLID_AFSPLITTERSLib::IStringTokenizerSplitterPtr ipSplitterObj)

{
	try
	{
		// get the delimiter character, and ensure that the delimiting character
		// is a printable character, and that the editbox only contains exactly one 
		// character as the delimiter
		CComBSTR bstrDelimiter;
		m_editDelimiter.GetWindowText(bstrDelimiter.m_str);
		string strDelimiter = asString(bstrDelimiter.m_str);

		if (strDelimiter.length() != 1 || 
			!containsNonWhitespaceChars(strDelimiter))
		{
			UCLIDException ue("ELI05791", "Invalid delimiter length!\r\n"
				"\r\n"
				"Please specify exactly one character as a delimiter. "\
				"The character specified as the delimiter should be a printable "
				"character - i.e. one of the characters on the keyboard.");
			ue.addDebugInfo("strDelimiter", strDelimiter);
			throw ue;
		}
		short sDelimiter = strDelimiter[0];
		ipSplitterObj->Delimiter = sDelimiter;
		return true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05790")

	// if we reached here, it's because the delimiter could not be stored
	// properly
	m_editDelimiter.SetSel(0, -1);
	m_editDelimiter.SetFocus();
	return false;
}
//-------------------------------------------------------------------------------------------------
bool CStringTokenizerSplitterPP::storeFieldNameExpression(
	UCLID_AFSPLITTERSLib::IStringTokenizerSplitterPtr ipSplitterObj)
{
	try
	{
		// get the field name expression, and ensure that it is
		// at least one character long
		CComBSTR bstrNameExpression;
		m_editNameExpression.GetWindowText(bstrNameExpression.m_str);
		ipSplitterObj->FieldNameExpression = bstrNameExpression.m_str;

		return true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05793")

	// if we reached here, it's because the field name expression 
	// could not be stored properly
	m_editNameExpression.SetSel(0, -1);
	m_editNameExpression.SetFocus();
	return false;
}
//-------------------------------------------------------------------------------------------------
bool CStringTokenizerSplitterPP::storeAttributeNameValues(
	UCLID_AFSPLITTERSLib::IStringTokenizerSplitterPtr ipSplitterObj)
{
	try
	{
		IIUnknownVectorPtr ipVecAttributeNameAndValue;
		int nTotalItems;
		
		// get the items for the sub-attribute names/values, and
		// ensure that there exists at least one entry.
		nTotalItems = m_listSubAttributeNameValues.GetItemCount();
		if (nTotalItems == 0)
		{
			MessageBox("Please specify one or sub-attributes to build using tokens.");
			return false;
		}
		else
		{
			ipVecAttributeNameAndValue.CreateInstance(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI05377", ipVecAttributeNameAndValue != __nullptr);
			for (int n = 0; n < nTotalItems; n++)
			{
				// get item texts from list
				string strEnt1 = getItemText(n, giATTRIBUTE_NAME_COLUMN);
				string strEnt2 = getItemText(n, giATTRIBUTE_VALUE_COLUMN);
				IStringPairPtr ipStringPair(CLSID_StringPair);
				ipStringPair->StringKey = strEnt1.c_str();
				ipStringPair->StringValue = strEnt2.c_str();
				ipVecAttributeNameAndValue->PushBack(ipStringPair);
			}
			
			// update the splitted object
			ipSplitterObj->AttributeNameAndValueExprVector = ipVecAttributeNameAndValue;
			return true;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05795")

	// if we reached here, it's because the attribute names+values
	// could not be stored properly
	return false;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerSplitterPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	ATLTRACE(_T("CStringTokenizerSplitterPP::Apply\n"));

	try
	{
		for (UINT i = 0; i < m_nObjects; i++)
		{
			// update the underlying objects
			UCLID_AFSPLITTERSLib::IStringTokenizerSplitterPtr ipSplitterObj = m_ppUnk[i];

			if (!storeDelimiterField(ipSplitterObj))
				return S_FALSE;

			// get and store the split type
			EStringTokenizerSplitType eSplitType = m_btnRadio1.GetCheck() == 1 ? 
				kEachTokenAsSubAttribute : kEachTokenAsSpecified;
			ipSplitterObj->SplitType = (UCLID_AFSPLITTERSLib::EStringTokenizerSplitType) eSplitType;

			// depending upon the split type, get either the field name
			// expression or the name-value vector, and store it in the Splitter object
			switch (eSplitType)
			{
			case kEachTokenAsSubAttribute:
				if (!storeFieldNameExpression(ipSplitterObj))
				{
					return S_FALSE;
				}
				break;

			case kEachTokenAsSpecified:
				if (!storeAttributeNameValues(ipSplitterObj))
				{
					return S_FALSE;
				}
				break;
			
			default:
				THROW_LOGIC_ERROR_EXCEPTION("ELI05373")
			}
		}
		m_bDirty = FALSE;
		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05789")

	// if we reached here, it's because something went wrong
	// (such as one of the put methods throwing an exception because the
	// put data was invalid)
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CStringTokenizerSplitterPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		if (pbValue == NULL)
		{
			return E_POINTER;
		}

		// Check license
		validateLicense();

		// If no exception, then pbValue is true
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Windows Message Handlers
//-------------------------------------------------------------------------------------------------
LRESULT CStringTokenizerSplitterPP::OnClickedDelimiterHelp(WORD wNotifyCode,
	WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// show tooltip info
		CString zText("Please specify exactly one character as the delimiter.\n"
					  "\n"
			          "A delimiter is a character that separates text into multiple parts.\n"
					  "Examples of delimiters are the comma (,), semi-colon (;), hypen (-), etc.");
		m_infoTip.Show(zText);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05566");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CStringTokenizerSplitterPP::OnClickedNameExpressionHelp(WORD wNotifyCode, 
	WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// show tooltip info
		CString zText("The field name expression will be used to name the various sub-attribute fields.\n"
					  "If you use this option, each token (part) will become a sub-attribute.\n"
					  "\n"
					  "Use %d to indicate the token number.  For instance, the expression Field_%d\n"
					  "will create sub-attributes with the names Field_1, Field_2, Field_3, etc.\n"
					  "\n"
					  "If you do not use %d in the field name expression, all created sub-attributes\n"
					  "will have the same name.");
		m_infoTip.Show(zText);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05567");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CStringTokenizerSplitterPP::OnClickedSubAttributesHelp(WORD wNotifyCode, 
	WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// show tooltip info
		CString zText("In the token expression field, use %1 to indicate the first token,\n"
					  "%2 to indicate the second token, etc.\n"
					  "\n"
					  "Token numbers start with 1 (one).  For instance, if a string contains\n"
					  "3 tokens, the valid token numbers are 1, 2, and 3.\n"
					  "If you indicate a token number that is larger than the number\n"
					  "of tokens found, it will be replaced with an empty string.\n"
					  "\n"
					  "You may not specify any \"variables\" in the Sub-attribute name field.\n"
					  "The values you specify in the Sub-attribute name column will directly\n"
					  "be used as the Sub-attribute name without any string or variable\n"
					  "substitutions.");
		m_infoTip.Show(zText);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05565");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CStringTokenizerSplitterPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// create tooltip object and set no delay.
		m_infoTip.Create(CWnd::FromHandle(m_hWnd));
		m_infoTip.SetShowDelay(0);

		// "connect" the control classes to the actual controls
		m_btnRadio1 = GetDlgItem(IDC_RADIO1);
		m_btnRadio2 = GetDlgItem(IDC_RADIO2);
		m_editDelimiter = GetDlgItem(IDC_EDIT_DELIMITER);
		m_txtExpressionLabel = GetDlgItem(IDC_STATIC_NAME_EXPRESSION_LABEL);
		m_editNameExpression = GetDlgItem(IDC_EDIT_NAME_EXPRESSION);
		m_listSubAttributeNameValues = GetDlgItem(IDC_LIST_SUB_ATTRIBUTES);
		m_btnAdd = GetDlgItem(IDC_BUTTON_ADD);
		m_btnRemove = GetDlgItem(IDC_BUTTON_REMOVE);
		m_btnModify = GetDlgItem(IDC_BUTTON_MODIFY);
		m_btnUp = GetDlgItem(IDC_BTN_UP);
		m_btnDown = GetDlgItem(IDC_BTN_DOWN);

		// load icons for up and down buttons
		m_btnUp.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_UP)));
		m_btnDown.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_DOWN)));

		// update both buttons
		// TODO: is this call necessary here?
		updateUpAndDownButtons();

		// get access to the underlying object who's property's are being
		// exposed by this property page
		UCLID_AFSPLITTERSLib::IStringTokenizerSplitterPtr ipObj = m_ppUnk[0];
		if (ipObj == __nullptr)
		{
			throw UCLIDException("ELI05358", "No object associated with this property page!");
		}

		// update the delimiter text
		char cDelimiter = (char)ipObj->Delimiter;
		char pszTemp[] = ".";
		pszTemp[0] = cDelimiter;
		m_editDelimiter.SetWindowText(pszTemp);
		
		// setup the listbox columns
		CRect listRect;
		m_listSubAttributeNameValues.GetClientRect(&listRect);
		m_listSubAttributeNameValues.SetExtendedListViewStyle(LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT);
		m_listSubAttributeNameValues.InsertColumn(giATTRIBUTE_NAME_COLUMN, 
			"Sub-Attribute Name", LVCFMT_LEFT, giATTRIBUTE_NAME_COLUMN_WIDTH,
			giATTRIBUTE_NAME_COLUMN);
		m_listSubAttributeNameValues.InsertColumn(giATTRIBUTE_VALUE_COLUMN, 
			"Token Expression", LVCFMT_LEFT, listRect.right - 
			giATTRIBUTE_NAME_COLUMN_WIDTH, giATTRIBUTE_VALUE_COLUMN);

		// update the state of the radio button based upon split type
		// also update the state of the controls associated with the
		// split type
		UCLID_AFSPLITTERSLib::EStringTokenizerSplitType eSplitType;
		eSplitType = ipObj->SplitType;
		switch(eSplitType)
		{
		case UCLID_AFSPLITTERSLib::kEachTokenAsSubAttribute:
			{				
				// Set correct checked/enabled/disabled state for controls
				m_btnRadio1.SetCheck(TRUE);
				BOOL bTemp;
				OnClickedRadio1(NULL, NULL, NULL, bTemp);
				
				// update the name expression field
				_bstr_t _bstrNameExpression = ipObj->FieldNameExpression;
				m_editNameExpression.SetWindowText(_bstrNameExpression);

				break;
			}
		
		case UCLID_AFSPLITTERSLib::kEachTokenAsSpecified:
			{
				// Set correct checked/enabled/disabled state for controls
				m_btnRadio2.SetCheck(TRUE);
				BOOL bTemp;
				OnClickedRadio2(NULL, NULL, NULL, bTemp);

				// update the list box
				IIUnknownVectorPtr ipVec= ipObj->AttributeNameAndValueExprVector;
				if (ipVec == __nullptr)
				{
					throw UCLIDException("ELI05359", "Unable to retrieve AttributeName-and-ValueExpression vector!");
				}
				int nVecSize = ipVec->Size();
				for (int i = 0; i < nVecSize; i++)
				{
					IStringPairPtr ipStringPair = ipVec->At(i);
					string strKey = ipStringPair->StringKey;
					string strValue = ipStringPair->StringValue;
					m_listSubAttributeNameValues.InsertItem(i, strKey.c_str());
					m_listSubAttributeNameValues.SetItemText(i, 
						giATTRIBUTE_VALUE_COLUMN, strValue.c_str());
				}
				break;
			}
		
		default:
			THROW_LOGIC_ERROR_EXCEPTION("ELI05357")
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05337")

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CStringTokenizerSplitterPP::OnUpdateEditDelimiter(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// TODO: try to prevent the user from typing more than one character
		// in the delimiter field
		// CComBSTR bstrText;
		// m_editDelimiter.GetWindowText(bstrText.m_str);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05402")
		
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CStringTokenizerSplitterPP::OnClickedRadio1(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_txtExpressionLabel.EnableWindow(TRUE);
		m_editNameExpression.EnableWindow(TRUE);
		m_listSubAttributeNameValues.EnableWindow(FALSE);
		m_btnAdd.EnableWindow(FALSE);
		m_btnRemove.EnableWindow(FALSE);
		m_btnModify.EnableWindow(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05338")
		
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CStringTokenizerSplitterPP::OnClickedRadio2(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_txtExpressionLabel.EnableWindow(FALSE);
		m_editNameExpression.EnableWindow(FALSE);
		m_listSubAttributeNameValues.EnableWindow(TRUE);
		m_btnAdd.EnableWindow(TRUE);
		
		// if there is a current selection, then enable the remove/modify
		// buttons
		int nSelections = m_listSubAttributeNameValues.GetSelectedCount();
		
		// enable and disable Remove and Modify buttons properly
		m_btnRemove.EnableWindow(nSelections >= 1 ? TRUE: FALSE);
		m_btnModify.EnableWindow(nSelections == 1 ? TRUE: FALSE);
		
		// update up and down buttons
		updateUpAndDownButtons();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05339")

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CStringTokenizerSplitterPP::OnClickedButtonAdd(WORD wNotifyCode, WORD wID, 
													   HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// add entry to the list view
		int nTotalItems = m_listSubAttributeNameValues.GetItemCount();
		int nNewIndex = 0;
		if (nTotalItems>0)
		{
			nNewIndex = nTotalItems;
		}

		// show prompt dialog for entering translation infos
		CString zEnt1, zEnt2;
		int nExistingItemIndex = -1;
		bool bSuccess = promptForReplacements(zEnt1, zEnt2, nExistingItemIndex);
		if (bSuccess)
		{
			if (nExistingItemIndex >= 0)
			{
				m_listSubAttributeNameValues.SetItemText(nExistingItemIndex, giATTRIBUTE_NAME_COLUMN, zEnt1);
				m_listSubAttributeNameValues.SetItemText(nExistingItemIndex, giATTRIBUTE_VALUE_COLUMN, zEnt2);
			}
			else
			{
				m_listSubAttributeNameValues.InsertItem(nNewIndex, zEnt1);
				m_listSubAttributeNameValues.SetItemText(nNewIndex, giATTRIBUTE_VALUE_COLUMN, zEnt2);
			}

			updateUpAndDownButtons();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05361")

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CStringTokenizerSplitterPP::OnClickedButtonModify(WORD wNotifyCode, WORD wID, 
														  HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// get current selected item texts
		int nSelectedItemIndex = m_listSubAttributeNameValues.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
		if (nSelectedItemIndex >= 0)
		{
			// get the selected item text to pass into the prompt dialog
			CString zEnt1 = getItemText(nSelectedItemIndex, giATTRIBUTE_NAME_COLUMN).c_str();
			CString zEnt2 = getItemText(nSelectedItemIndex, giATTRIBUTE_VALUE_COLUMN).c_str();

			int nExistingItemIndex = nSelectedItemIndex;
			bool bSuccess = promptForReplacements(zEnt1, zEnt2, nExistingItemIndex);
			// show prompt dialog for entering translation infos
			if (bSuccess)
			{
				// if the entry already exists, then replace the old one with the new one
				if (nExistingItemIndex >= 0 && 
					nExistingItemIndex != nSelectedItemIndex)
				{
					nSelectedItemIndex = nExistingItemIndex;
				}

				m_listSubAttributeNameValues.SetItemText(nSelectedItemIndex, giATTRIBUTE_NAME_COLUMN, zEnt1);
				m_listSubAttributeNameValues.SetItemText(nSelectedItemIndex, giATTRIBUTE_VALUE_COLUMN, zEnt2);				
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05362")

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CStringTokenizerSplitterPP::OnClickedButtonRemove(WORD wNotifyCode, WORD wID, 
														  HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// get index of first selected item
		
		int nItem = m_listSubAttributeNameValues.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
		
		// at least one item is selected
		if (nItem != -1)
		{
			int nRes = MessageBox("Delete selected item(s)?", "Confirm Delete", MB_YESNO);
			if (nRes == IDYES)
			{
				int nFirstItem = nItem;
				
				while(nItem != -1)
				{
					// remove from the UI listbox
					m_listSubAttributeNameValues.DeleteItem(nItem);
								
					nItem = m_listSubAttributeNameValues.GetNextItem(nItem - 1, ((nItem == 0) ? LVNI_ALL : LVNI_BELOW) | LVNI_SELECTED);
				}
				
				// if there's more item(s) below last deleted item, then set 
				// selection on the next item
				int nTotalNumOfItems = m_listSubAttributeNameValues.GetItemCount();
				if (nFirstItem < nTotalNumOfItems)
				{
					m_listSubAttributeNameValues.SetItemState(nFirstItem, LVIS_SELECTED, LVIS_SELECTED);
				}
				else if (nTotalNumOfItems > 0)
				{
					// select the last item
					m_listSubAttributeNameValues.SetItemState(nTotalNumOfItems - 1, LVIS_SELECTED, LVIS_SELECTED);
				}
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05363")

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CStringTokenizerSplitterPP::OnKeydownListSubAttributes(int idCtrl, 
								   LPNMHDR pnmh, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// only delete or del key can delete the selected item
		int nDelete = ::GetKeyState(VK_DELETE);
		if (nDelete < 0)
		{
			return OnClickedButtonRemove(pnmh->code, pnmh->idFrom, pnmh->hwndFrom, bHandled);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05368");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CStringTokenizerSplitterPP::OnDblclkListSubAttributes(int idCtrl, 
															  LPNMHDR pnmh, 
															  BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return OnClickedButtonModify(pnmh->code, pnmh->idFrom, pnmh->hwndFrom, bHandled);
}
//-------------------------------------------------------------------------------------------------
LRESULT CStringTokenizerSplitterPP::OnItemchangedListSubAttributes(int idCtrl, 
																   LPNMHDR pnmh, 
																   BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		int nSelections = m_listSubAttributeNameValues.GetSelectedCount();
		
		// enable and disable Remove and Modify buttons properly
		m_btnRemove.EnableWindow(nSelections >= 1 ? TRUE: FALSE);
		m_btnModify.EnableWindow(nSelections == 1 ? TRUE: FALSE);

		// update up and down buttons
		updateUpAndDownButtons();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05365");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CStringTokenizerSplitterPP::OnClickedBtnDown(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// get current selected item texts
		int nSelectedItemIndex = m_listSubAttributeNameValues.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
		if (nSelectedItemIndex >= 0)
		{
			// get the index of the item right below currently selected item
			int nBelowIndex = nSelectedItemIndex + 1;

			// get selected item text from list
			string strEnt1 = getItemText(nSelectedItemIndex, giATTRIBUTE_NAME_COLUMN);
			string strEnt2 = getItemText(nSelectedItemIndex, giATTRIBUTE_VALUE_COLUMN);
			// then remove the selected item
			m_listSubAttributeNameValues.DeleteItem(nSelectedItemIndex);

			// now insert the item right before the item that was above
			m_listSubAttributeNameValues.InsertItem(nBelowIndex, strEnt1.c_str());
			m_listSubAttributeNameValues.SetItemText(nBelowIndex, 
				giATTRIBUTE_VALUE_COLUMN, strEnt2.c_str());

			// keep this item selected
			m_listSubAttributeNameValues.SetItemState(nBelowIndex, LVIS_SELECTED, LVIS_SELECTED);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05371");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CStringTokenizerSplitterPP::OnClickedBtnUp(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// get current selected item texts
		int nSelectedItemIndex = m_listSubAttributeNameValues.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
		if (nSelectedItemIndex >= 0)
		{
			// get the index of the item right above currently selected item
			int nAboveIndex = nSelectedItemIndex - 1;
			if (nAboveIndex < 0)
			{
				return 0;
			}

			// get selected item text from list
			string strEnt1 = getItemText(nSelectedItemIndex, giATTRIBUTE_NAME_COLUMN);
			string strEnt2 = getItemText(nSelectedItemIndex, giATTRIBUTE_VALUE_COLUMN);
			// then remove the selected item
			m_listSubAttributeNameValues.DeleteItem(nSelectedItemIndex);

			// now insert the item right before the item that was above
			m_listSubAttributeNameValues.InsertItem(nAboveIndex, strEnt1.c_str());
			m_listSubAttributeNameValues.SetItemText(nAboveIndex, 
				giATTRIBUTE_VALUE_COLUMN, strEnt2.c_str());
			
			// keep this item selected
			m_listSubAttributeNameValues.SetItemState(nAboveIndex, LVIS_SELECTED, LVIS_SELECTED);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05372");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private / helper methods
//-------------------------------------------------------------------------------------------------
bool CStringTokenizerSplitterPP::promptForReplacements(CString& zEnt1, CString& zEnt2, 
													   int& nItemIndex)
{
	bool bFocusOnInput1 = true;

	while (true)
	{
		Prompt2Dlg promptDlg("Specify Sub-Attribute Name & Token Expression",
					"Sub-Attribute name : ", zEnt1,
					"Token expression : ", zEnt2, bFocusOnInput1);

		int nRes = promptDlg.DoModal();
		if (nRes == IDOK)
		{
			zEnt1 = promptDlg.m_zInput1;
			zEnt2 = promptDlg.m_zInput2;

			// check whether the specified sub-attribute-name and
			// sub-attribute-value expression are valid.  
			// If not, bring up the dialog box again for correction
			UCLID_AFSPLITTERSLib::IStringTokenizerSplitterPtr ipSplitterObj = m_ppUnk[0];
			_bstr_t _bstrSubAttributeName = (LPCTSTR) zEnt1;
			_bstr_t _bstrSubAttributeValue = (LPCTSTR) zEnt2;

			if (ipSplitterObj->IsValidSubAttributeName(_bstrSubAttributeName) 
				== VARIANT_FALSE)
			{
				bFocusOnInput1 = true;
				continue;
			}
			else if (ipSplitterObj->IsValidSubAttributeValueExpression(
				_bstrSubAttributeValue) == VARIANT_FALSE)
			{
				bFocusOnInput1 = false;
				continue;
			}

			// check whether or not the entered the zEnt1 already exists in the list
			int nExistingItemIndex = existsStringToBeReplaced((LPCTSTR) zEnt1);
			if (nExistingItemIndex>=0 && nExistingItemIndex != nItemIndex) // entry already exists in the list
			{
				CString zMsg("");
				zMsg.Format("<%s> already exists in the list. Do you wish to overwrite the existing entry?", zEnt1);
				nRes = MessageBox(zMsg, "Overwrite Existing?", MB_YESNO);
				if (nRes == IDYES)
				{
					nItemIndex = nExistingItemIndex;
					return true;
				}
				
				// user clicked NO, let's keep the prompt dlg
				bFocusOnInput1 = true;
				continue;
			}
			
			// no duplicates found
			return true;
		}
		else
		{
			// user clicked Cancel
			break;
		}
	}

	return false;
}
//-------------------------------------------------------------------------------------------------
int CStringTokenizerSplitterPP::existsStringToBeReplaced(const string& strAttributeName)
{
	int nExistsIndex = -1;

	// get total items count
	int nTotalNum = m_listSubAttributeNameValues.GetItemCount();
	map<string, int> mapStringPairs;
	for (int n=0; n<nTotalNum; n++)
	{
		string strAttribute(getItemText(n, giATTRIBUTE_NAME_COLUMN));
		mapStringPairs[strAttribute] = n;
	}

	map<string, int>::iterator it = mapStringPairs.find(strAttributeName);
	if (it != mapStringPairs.end())
	{
		nExistsIndex = it->second;
	}

	return nExistsIndex;
}
//-------------------------------------------------------------------------------------------------
string CStringTokenizerSplitterPP::getItemText(int nIndex, int nColumnNum)
{
	string strItemText("");
	
	// get current selected item texts
	if (nIndex >= 0)
	{
		char pszValue[NUM_OF_CHARS];
		m_listSubAttributeNameValues.GetItemText(nIndex, nColumnNum, pszValue, NUM_OF_CHARS);
		strItemText = pszValue;
	}

	return strItemText;
}
//-------------------------------------------------------------------------------------------------
void CStringTokenizerSplitterPP::updateUpAndDownButtons()
{
	// enable/disable up and down arrow key buttons appropriately
	// disable both
	m_btnUp.EnableWindow(FALSE);
	m_btnDown.EnableWindow(FALSE);

	int nSelections = m_listSubAttributeNameValues.GetSelectedCount();
	
	if (nSelections == 1)
	{
		// get current selected item texts
		int nSelectedItemIndex = m_listSubAttributeNameValues.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
		int nTotalNum = m_listSubAttributeNameValues.GetItemCount();
		
		if (nTotalNum > 1)
		{
			if (nSelectedItemIndex == 0)
			{
				// First item selected
				// enable down button only
				m_btnUp.EnableWindow(FALSE);
				m_btnDown.EnableWindow(TRUE);
			}
			else if (nSelectedItemIndex > 0 && nSelectedItemIndex < nTotalNum-1)
			{
				// Some item other that first and last item selected
				// enable both buttons
				m_btnUp.EnableWindow(TRUE);
				m_btnDown.EnableWindow(TRUE);
			}
			else if (nSelectedItemIndex == nTotalNum-1)
			{
				// Last item selected
				// enable up button only
				m_btnUp.EnableWindow(TRUE);
				m_btnDown.EnableWindow(FALSE);
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CStringTokenizerSplitterPP::validateLicense()
{
	// Property Page requires Editor license
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI07676", "StringTokenizerSplitter PP" );
}
//-------------------------------------------------------------------------------------------------
