// BoxFinderPP.cpp : Implementation of CBoxFinderPP

#include "stdafx.h"
#include "BoxFinderPP.h"
#include "..\..\AFUtils\Code\Helper.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>
#include <Misc.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const int gnUNSPECIFIED = -1;
const int MAX_CLUE_CHARS = 4096;
const string gstrSPECIFY_PAGES = "Please specify pages to include!";
const string gstrSPECIFY_BOX_SIZE = "Please expected box dimensions!";
const string gstrINVALID_MIN_DIMENSION = "Invalid value.\r\n\r\nMinimum expected box size values "
	"must be a percentage value in the range 0 - 99 or be left blank for unspecified.";
const string gstrINVALID_MAX_DIMENSION = "Invalid value.\r\n\r\nMaximum expected box size values " 
	"must be a percentage value in the range 0 - 100 or be left blank for unspecified.";
const long gnMINIMUM_DIMENSION_LOWER_LIMIT = 0;
const long gnMINIMUM_DIMENSION_UPPER_LIMIT = 99;
const long gnMAXIMUM_DIMENSION_LOWER_LIMIT = 0;
const long gnMAXIMUM_DIMENSION_UPPER_LIMIT = 100;


//-------------------------------------------------------------------------------------------------
// CBoxFinderPP
//-------------------------------------------------------------------------------------------------
CBoxFinderPP::CBoxFinderPP()
{
	try
	{
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20335");
}
//-------------------------------------------------------------------------------------------------
CBoxFinderPP::~CBoxFinderPP()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI19677");
}
//-------------------------------------------------------------------------------------------------
HRESULT CBoxFinderPP::FinalConstruct()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
void CBoxFinderPP::FinalRelease()
{
}

//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CBoxFinderPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_AFVALUEFINDERSLib::IBoxFinderPtr ipBoxFinder = m_ppUnk[0];
		ASSERT_RESOURCE_ALLOCATION("ELI19678", ipBoxFinder);

		// Map controls to member variables
		m_listClues							= GetDlgItem(IDC_LIST_CLUES);
		m_btnCluesAreRegularExpressions		= GetDlgItem(IDC_CHK_AS_REGEXPR);
		m_btnCluesAreCaseSensitive			= GetDlgItem(IDC_CHK_CASE_SENSITIVE);
		m_btnFirstBoxOnly					= GetDlgItem(IDC_CHK_FIRST_BOX_ONLY);
		m_btnAdd							= GetDlgItem(IDC_BTN_ADD_CLUE);
		m_btnModify							= GetDlgItem(IDC_BTN_MODIFY_CLUE);
		m_btnRemove							= GetDlgItem(IDC_BTN_REMOVE_CLUE);
		m_btnUp.SubclassDlgItem(IDC_BTN_CLUE_UP, CWnd::FromHandle(m_hWnd));
		m_btnDown.SubclassDlgItem(IDC_BTN_CLUE_DOWN, CWnd::FromHandle(m_hWnd));
		m_pictClueDiagram					= GetDlgItem(IDC_PICT_CLUE_DIAGRAM);

		m_radioAllPages						= GetDlgItem(IDC_RADIO_ALL_PAGES);
		m_radioFirstPages					= GetDlgItem(IDC_RADIO_FIRST_PAGES);
		m_radioLastPages					= GetDlgItem(IDC_RADIO_LAST_PAGES);
		m_radioSpecifiedPages				= GetDlgItem(IDC_RADIO_SPECIFIED_PAGES);
		m_editFirstPageNums					= GetDlgItem(IDC_EDIT_FIRST_PAGE_NUMS);
		m_editLastPageNums					= GetDlgItem(IDC_EDIT_LAST_PAGE_NUMS);
		m_editSpecifiedPageNums				= GetDlgItem(IDC_EDIT_SPECIFIED_PAGE_NUMS);
		
		m_editBoxWidthMin					= GetDlgItem(IDC_EDIT_BOX_WIDTH_MIN);
		m_editBoxWidthMax					= GetDlgItem(IDC_EDIT_BOX_WIDTH_MAX);
		m_editBoxHeightMin					= GetDlgItem(IDC_EDIT_BOX_HEIGHT_MIN);
		m_editBoxHeightMax					= GetDlgItem(IDC_EDIT_BOX_HEIGHT_MAX);

		m_radioFindSpatialArea				= GetDlgItem(IDC_RADIO_RETURN_BOX_AREA);
		m_radioFindText						= GetDlgItem(IDC_RADIO_RETURN_TEXT);
		m_editAttributeText					= GetDlgItem(IDC_EDIT_ATTRIBUTE_TEXT);
		m_chkExcludeClueArea				= GetDlgItem(IDC_CHECK_EXCLUDE_CLUE_AREA);
		m_chkIncludeClueText				= GetDlgItem(IDC_CHECK_INCLUDE_CLUE_TEXT);
		m_chkIncludeLines					= GetDlgItem(IDC_CHECK_INCLUDE_LINES);

		// Create and initialize the info tip control
		m_infoTip.Create(CWnd::FromHandle(m_hWnd));
		m_infoTip.SetShowDelay(0);

		// Assign icons to the up and down buttons
		m_btnUp.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_UP)));
		m_btnDown.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_DOWN)));

		// update clue buttons' state
		updateClueButtons();

		// Load the clue list to the page
		initializeClueList(ipBoxFinder->Clues);

		// Set the clue checkboxes appropriately
		m_btnCluesAreRegularExpressions.SetCheck(
			asCppBool(ipBoxFinder->CluesAreRegularExpressions) ? BST_CHECKED : BST_UNCHECKED);
		m_btnCluesAreCaseSensitive.SetCheck(
			asCppBool(ipBoxFinder->CluesAreCaseSensitive) ? BST_CHECKED : BST_UNCHECKED);
		m_btnFirstBoxOnly.SetCheck(
			asCppBool(ipBoxFinder->FirstBoxOnly) ? BST_CHECKED : BST_UNCHECKED);

		// Initialize the location of the clue diagram and display the current clue location setting
		m_pictClueDiagram.GetWindowRect(&m_rectClueBitmap);
		ScreenToClient(&m_rectClueBitmap);
		displayClueLocation((EClueLocation) ipBoxFinder->ClueLocation);

		// Initialize the page specification edit boxes
		if (ipBoxFinder->NumFirstPages != 0)
		{
			m_editFirstPageNums.SetWindowText(asString(ipBoxFinder->NumFirstPages).c_str());
		}
		if (ipBoxFinder->NumLastPages != 0)
		{
			m_editLastPageNums.SetWindowText(asString(ipBoxFinder->NumLastPages).c_str());
		}
		m_editSpecifiedPageNums.SetWindowText(asString(ipBoxFinder->SpecifiedPages).c_str());

		// check and enable controls as necessary according to the page selection mode
		switch (ipBoxFinder->PageSelectionMode)
		{
			case kAllPages:			onSelectPages(IDC_RADIO_ALL_PAGES); break;
			case kFirstPages:		onSelectPages(IDC_RADIO_FIRST_PAGES); break;
			case kLastPages:		onSelectPages(IDC_RADIO_LAST_PAGES); break;
			case kSpecifiedPages:	onSelectPages(IDC_RADIO_SPECIFIED_PAGES); break;
			default:				onSelectPages(IDC_RADIO_ALL_PAGES); break;
		}

		// Initialize the page dimension controls
		if (ipBoxFinder->BoxWidthMin != gnUNSPECIFIED)
		{
			m_editBoxWidthMin.SetWindowText(asString(ipBoxFinder->BoxWidthMin).c_str());
		}

		if (ipBoxFinder->BoxWidthMax != gnUNSPECIFIED)
		{
			m_editBoxWidthMax.SetWindowText(asString(ipBoxFinder->BoxWidthMax).c_str());
		}

		if (ipBoxFinder->BoxHeightMin != gnUNSPECIFIED)
		{
			m_editBoxHeightMin.SetWindowText(asString(ipBoxFinder->BoxHeightMin).c_str());
		}
	
		if (ipBoxFinder->BoxHeightMax != gnUNSPECIFIED)
		{
			m_editBoxHeightMax.SetWindowText(asString(ipBoxFinder->BoxHeightMax).c_str());
		}

		// Initialize the return value specification controls
		m_chkExcludeClueArea.SetCheck(
			asCppBool(ipBoxFinder->ExcludeClueArea) ? BST_CHECKED : BST_UNCHECKED);
		m_chkIncludeClueText.SetCheck(
			asCppBool(ipBoxFinder->IncludeClueText) ? BST_CHECKED : BST_UNCHECKED);
		m_editAttributeText.SetWindowText(asString(ipBoxFinder->AttributeText).c_str());
		
		if (asCppBool(ipBoxFinder->FindType == kImageRegion))
		{
			onSelectFindType(IDC_RADIO_RETURN_BOX_AREA);
		}
		else
		{
			onSelectFindType(IDC_RADIO_RETURN_TEXT);
		}

		m_chkIncludeLines.SetCheck(asCppBool(ipBoxFinder->IncludeLines) ? BST_CHECKED : BST_UNCHECKED);

		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19679");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CBoxFinderPP::OnLButtonUp(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	CPoint ptMousePos((CPoint)lParam);

	try
	{
		if (m_rectClueBitmap.PtInRect(ptMousePos) == TRUE)
		{
			// If the mouse was clicked within the clue location diagram, retrieve the new setting
			// and update the display accordingly.
			UCLID_AFVALUEFINDERSLib::IBoxFinderPtr ipBoxFinder = m_ppUnk[0];
			ASSERT_RESOURCE_ALLOCATION("ELI19752", ipBoxFinder);

			EClueLocation eNewClueLocation = getSelectedClueLocation(ptMousePos);
			displayClueLocation(eNewClueLocation);
			ipBoxFinder->ClueLocation = (UCLID_AFVALUEFINDERSLib::EClueLocation) eNewClueLocation;	 
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19763");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CBoxFinderPP::OnBnClickedFindType(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Handle selection of one of the return type radio buttons
		onSelectFindType(wID);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19758");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CBoxFinderPP::OnBnClickedSelectedPages(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Handle selection of one of the page selection radio buttons
		onSelectPages(wID);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19759");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CBoxFinderPP::OnBnClickedBtnAddClue(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Prompt user to enter a new clue
		CString zClue;
		bool bSuccess = promptForValue(zClue, m_listClues, "", -1);

		if (bSuccess)
		{
			int nTotal = m_listClues.GetItemCount();
				
			// new item index
			int nIndex = m_listClues.InsertItem(nTotal, zClue);
			for (int n = 0; n <= nTotal; n++)
			{
				// deselect any other items
				int nState = (n == nIndex) ? LVIS_SELECTED : 0;

				// only select current newly added item
				m_listClues.SetItemState(n, nState, LVIS_SELECTED);
			}

			// adjust the column width in case there is a vertical scrollbar
			CRect rect;
			m_listClues.GetClientRect(&rect);			
			m_listClues.SetColumnWidth(0, rect.Width());

			// Call updateClueButtons to update the related buttons
			updateClueButtons();

			SetDirty(TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19760");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CBoxFinderPP::OnBnClickedBtnModifyClue(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// get currently selected item
		int nSelectedItemIndex = m_listClues.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
		if (nSelectedItemIndex==LB_ERR)
		{
			return 0;
		}

		char pszValue[MAX_CLUE_CHARS];
		// get selected text
		m_listClues.GetItemText(nSelectedItemIndex, 0, pszValue, MAX_CLUE_CHARS);

		// Prompt user to modify the current value
		CString zEnt(pszValue);
		bool bSuccess = promptForValue(zEnt, m_listClues, "", -1);
		if (bSuccess)
		{
			// If the user OK'd the box, save the new value
			m_listClues.DeleteItem(nSelectedItemIndex);
			
			int nIndex = m_listClues.InsertItem(nSelectedItemIndex, zEnt);

			m_listClues.SetItemState(nIndex, LVIS_SELECTED, LVIS_SELECTED);

			// Call updateClueButtons to update the related buttons
			updateClueButtons();

			// Set Dirty flag
			SetDirty(TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19761");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CBoxFinderPP::OnBnClickedBtnRemoveClue(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// get first selected item
		int nItem = m_listClues.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
		
		if (nItem != -1)
		{
			int nRes = MessageBox("Delete selected item(s)?", "Confirm", MB_YESNO);
			if (nRes == IDYES)
			{
				// remove selected items
				int nFirstItem = nItem;
				
				// delete any selected item since this list ctrl allows multiple selection
				while(nItem != -1)
				{
					// remove from the UI listbox
					m_listClues.DeleteItem(nItem);
					// get next item selected item
					nItem = m_listClues.GetNextItem(nItem - 1, ((nItem == 0) ? LVNI_ALL : LVNI_BELOW) | LVNI_SELECTED);
				}
				
				// if there's more item(s) below last deleted item, then set 
				// selection on the next item
				int nTotalNumOfItems = m_listClues.GetItemCount();
				if (nFirstItem < nTotalNumOfItems)
				{
					m_listClues.SetItemState(nFirstItem, LVIS_SELECTED, LVIS_SELECTED);
				}
				else if (nTotalNumOfItems > 0)
				{
					// select the last item
					m_listClues.SetItemState(nTotalNumOfItems - 1, LVIS_SELECTED, LVIS_SELECTED);
				}
			}

			// adjust the column width in case there is a vertical scrollbar now
			CRect rect;
			m_listClues.GetClientRect(&rect);
			m_listClues.SetColumnWidth(0, rect.Width());

			// Call updateClueButtons to update the related buttons
			updateClueButtons();

			SetDirty(TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19762");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CBoxFinderPP::OnBnClickedBtnUp(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// get current selected item index
		int nSelectedItemIndex = m_listClues.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
		if (nSelectedItemIndex >= 0)
		{
			// get the index of the item right above currently selected item
			int nAboveIndex = nSelectedItemIndex - 1;
			if (nAboveIndex < 0)
			{
				return 0;
			}

			// get selected item text from list
			char pszValue[MAX_CLUE_CHARS];
			m_listClues.GetItemText(nSelectedItemIndex, 0, pszValue, MAX_CLUE_CHARS);
			CString zEnt(pszValue);
			// then remove the selected item
			m_listClues.DeleteItem(nSelectedItemIndex);

			// now insert the item right before the item that was above
			int nActualIndex = m_listClues.InsertItem(nAboveIndex, zEnt);
			
			// keep this item selected
			m_listClues.SetItemState(nActualIndex, LVIS_SELECTED, LVIS_SELECTED);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20212");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CBoxFinderPP::OnBnClickedBtnDown(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// get current selected item index
		int nSelectedItemIndex = m_listClues.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
		if (nSelectedItemIndex >= 0)
		{
			// get the index of the item right below currently selected item
			int nBelowIndex = nSelectedItemIndex + 1;
			if (nBelowIndex == m_listClues.GetItemCount())
			{
				return 0;
			}

			char pszValue[MAX_CLUE_CHARS];
			// get selected item text from list
			m_listClues.GetItemText(nSelectedItemIndex, 0, pszValue, MAX_CLUE_CHARS);
			CString zEnt(pszValue);

			// then remove the selected item
			m_listClues.DeleteItem(nSelectedItemIndex);

			// now insert the item right before the item that was above
			int nActualIndex = m_listClues.InsertItem(nBelowIndex, zEnt);

			// keep this item selected
			m_listClues.SetItemState(nActualIndex, LVIS_SELECTED, LVIS_SELECTED);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20213");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CBoxFinderPP::OnItemChangedListClues(int idCtrl, LPNMHDR pNMHDR, BOOL& bHandle)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		updateClueButtons();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20214");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CBoxFinderPP::OnDblclkListClues(int idCtrl, LPNMHDR pNMHDR, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		if (m_listClues.GetSelectedCount() == 1)
		{
			OnBnClickedBtnModifyClue(0, 0, 0, bHandled);
		}
		else if (m_listClues.GetSelectedCount() == 0)
		{
			OnBnClickedBtnAddClue(0, 0, 0, bHandled);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20330");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CBoxFinderPP::OnClickedClueDynamicListInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// show tooltip info
		CString zText("- Dynamically loading a string list from a file is supported.\n"
					  "- To specify a dynamic file, an entry must begin with \"file://\".\n"
					  "- A file may be specified in combination with static entries or\n"
					  "  additional dynamic lists.\n"
					  "- Path tags such as <RSDFileDir> and <ComponentDataDir> may be used.\n"
					  "- For example, if an entry in the list is file://<RSDFileDir>\\list.txt,\n"
					  "  the entry will be replaced dynamically at runtime with the contents\n"
					  "  of the file.\n");
		m_infoTip.Show(zText);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20215");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinderPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ATLTRACE(_T("CBoxFinderPP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			// Obtain interface pointer to the IImageRegionWithLines class
			UCLID_AFVALUEFINDERSLib::IBoxFinderPtr ipBoxFinder = m_ppUnk[i];
			ASSERT_RESOURCE_ALLOCATION("ELI19680", ipBoxFinder != __nullptr);

			// Store clue settings
			ipBoxFinder->Clues = retrieveAndValidateClues();

			ipBoxFinder->CluesAreRegularExpressions =
				asVariantBool(m_btnCluesAreRegularExpressions.GetCheck() == BST_CHECKED);
			ipBoxFinder->CluesAreCaseSensitive =
				asVariantBool(m_btnCluesAreCaseSensitive.GetCheck() == BST_CHECKED);
			ipBoxFinder->FirstBoxOnly = 
				asVariantBool(m_btnFirstBoxOnly.GetCheck() == BST_CHECKED);

			// Store page selection settings
			if (m_radioAllPages.GetCheck() == BST_CHECKED)
			{
				ipBoxFinder->PageSelectionMode = UCLID_AFVALUEFINDERSLib::kAllPages;
			}
			else if (m_radioFirstPages.GetCheck() == BST_CHECKED)
			{
				ipBoxFinder->PageSelectionMode = UCLID_AFVALUEFINDERSLib::kFirstPages;
			}
			else if (m_radioLastPages.GetCheck() == BST_CHECKED)
			{
				ipBoxFinder->PageSelectionMode = UCLID_AFVALUEFINDERSLib::kLastPages;
			}
			else if (m_radioSpecifiedPages.GetCheck() == BST_CHECKED)
			{
				ipBoxFinder->PageSelectionMode = UCLID_AFVALUEFINDERSLib::kSpecifiedPages;
			}

			// If FirstPages or LastPages is selected, ensure a page value has been provided
			ipBoxFinder->NumFirstPages = verifyControlValueAsLong(m_editFirstPageNums, 0, 
				m_radioFirstPages.GetCheck() == BST_CHECKED ? gstrSPECIFY_PAGES : "");

			ipBoxFinder->NumLastPages = verifyControlValueAsLong(m_editLastPageNums, 0,
				m_radioLastPages.GetCheck() == BST_CHECKED ? gstrSPECIFY_PAGES : "");

			// Validate the specified pages value
			CComBSTR bstrPages;
			m_editSpecifiedPageNums.GetWindowText(&bstrPages);
			try
			{
				validatePageNumbers(asString(bstrPages));
				ipBoxFinder->SpecifiedPages = bstrPages.m_str;
			}
			catch (UCLIDException &ue)
			{
				// If kSpecifiedPages is being used and we've failed validation, throw an execption
				if (ipBoxFinder->PageSelectionMode == UCLID_AFVALUEFINDERSLib::kSpecifiedPages)
				{
					m_editSpecifiedPageNums.SetFocus();
					throw ue;
				}
				
				// If kSpecifiedPages is not being used, don't worry about a bad validation in this case;
				// Just don't save the new value
			}

			// Store box dimension settings
			ipBoxFinder->BoxWidthMin = verifyControlValueAsLong(m_editBoxWidthMin, 
				gnMINIMUM_DIMENSION_LOWER_LIMIT, gnMINIMUM_DIMENSION_UPPER_LIMIT, 
				gstrINVALID_MIN_DIMENSION, gnUNSPECIFIED);
			ipBoxFinder->BoxWidthMax = verifyControlValueAsLong(m_editBoxWidthMax, 
				gnMAXIMUM_DIMENSION_LOWER_LIMIT, gnMAXIMUM_DIMENSION_UPPER_LIMIT, 
				gstrINVALID_MAX_DIMENSION, gnUNSPECIFIED);

			if (ipBoxFinder->BoxWidthMax != gnUNSPECIFIED &&
				ipBoxFinder->BoxWidthMin > ipBoxFinder->BoxWidthMax)
			{
				m_editBoxWidthMax.SetFocus();

				UCLIDException ue("ELI19803", "Box width maximum value is less than the minimum value!");
				throw ue;
			}

			ipBoxFinder->BoxHeightMin = verifyControlValueAsLong(m_editBoxHeightMin,
				gnMINIMUM_DIMENSION_LOWER_LIMIT, gnMINIMUM_DIMENSION_UPPER_LIMIT, 
				gstrINVALID_MIN_DIMENSION, gnUNSPECIFIED);
			ipBoxFinder->BoxHeightMax = verifyControlValueAsLong(m_editBoxHeightMax,
				gnMAXIMUM_DIMENSION_LOWER_LIMIT, gnMAXIMUM_DIMENSION_UPPER_LIMIT, 
				gstrINVALID_MAX_DIMENSION, gnUNSPECIFIED);

			if (ipBoxFinder->BoxHeightMax != gnUNSPECIFIED &&
				ipBoxFinder->BoxHeightMin > ipBoxFinder->BoxHeightMax)
			{
				m_editBoxHeightMax.SetFocus();

				UCLIDException ue("ELI19804", "Box height maximum value is less than the minimum value!");
				throw ue;
			}

			// Store the clue location
			ipBoxFinder->ExcludeClueArea =
				asVariantBool(m_chkExcludeClueArea.GetCheck() == BST_CHECKED);
			ipBoxFinder->IncludeClueText =
				asVariantBool(m_chkIncludeClueText.GetCheck() == BST_CHECKED);

			// Store return value format settings
			CComBSTR bstrAttributeText;
			m_editAttributeText.GetWindowText(&bstrAttributeText);
			ipBoxFinder->AttributeText = bstrAttributeText.m_str;
			
			if (m_radioFindSpatialArea.GetCheck() == BST_CHECKED)
			{
				if (bstrAttributeText.Length() == 0)
				{
					m_editAttributeText.SetFocus();

					UCLIDException ue("ELI19884", "Text to assign to any found boxes must be specified!");
					throw ue;
				}

				ipBoxFinder->FindType = (UCLID_AFVALUEFINDERSLib::EFindType) kImageRegion;
			}
			else
			{
				ipBoxFinder->FindType = (UCLID_AFVALUEFINDERSLib::EFindType) kText;
			}

			ipBoxFinder->IncludeLines =
				asVariantBool(m_chkIncludeLines.GetCheck() == BST_CHECKED);
		}

		SetDirty(FALSE);

		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19681");

	// If we reached here, it's because of an exception
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CBoxFinderPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI19682", pbValue != __nullptr);

		try
		{
			// check the license
			validateLicense();

			// If no exception, then pbValue is true
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19683");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void CBoxFinderPP::initializeClueList(IVariantVectorPtr ipClues)
{
	ASSERT_RESOURCE_ALLOCATION("ELI19746", ipClues != __nullptr);

	m_listClues.SetExtendedListViewStyle(LVS_EX_GRIDLINES|LVS_EX_FULLROWSELECT);
	CRect rect;			
	m_listClues.GetClientRect(&rect);
	// make the first column as wide as the whole list box
	m_listClues.InsertColumn(0, "Dummy Header", LVCFMT_LEFT, rect.Width(), 0);

	long nSize = ipClues->Size;
	if (nSize > 0)
	{
		for (int n = 0; n < nSize; n++)
		{
			string strClue = asString(_bstr_t(ipClues->GetItem(n)));

			m_listClues.InsertItem(n, strClue.c_str());
		}
		// select the first item in the list
		m_listClues.SetItemState(0, LVIS_SELECTED, LVIS_SELECTED);
	}

	m_listClues.GetClientRect(&rect);
	// adjust the column width in case there is a vertical scrollbar now
	m_listClues.SetColumnWidth(0, rect.Width());
}
//-------------------------------------------------------------------------------------------------
IVariantVectorPtr CBoxFinderPP::retrieveAndValidateClues()
{
	int nTotalNumOfClues = m_listClues.GetItemCount();
	if (nTotalNumOfClues == 0)
	{
		m_listClues.SetFocus();

		UCLIDException ue("ELI19750", "One or more clues must be specified!");
		throw ue;
	}

	// get all list items
	IVariantVectorPtr ipClues(CLSID_VariantVector);
	ASSERT_RESOURCE_ALLOCATION("ELI19751", ipClues!=NULL);

	for (long n=0; n<nTotalNumOfClues; n++)
	{
		// always get the first column item
		char pszValue[MAX_CLUE_CHARS];
		m_listClues.GetItemText(n, 0, pszValue, MAX_CLUE_CHARS); 
		ipClues->PushBack(pszValue);
	}
	
	return ipClues;
}
//-------------------------------------------------------------------------------------------------
EClueLocation CBoxFinderPP::getSelectedClueLocation(CPoint &ptMousePos)
{
	// Convert mouse coordinates into a row and column value
	MapWindowPoints(m_pictClueDiagram.m_hWnd, &ptMousePos, 1);

	int nRow = ptMousePos.y / (m_rectClueBitmap.Height() / 3);
	int nCol = ptMousePos.x / (m_rectClueBitmap.Width() / 3);

	switch ((nRow * 3) + nCol)
	{
		case 0:		return kBoxToTopLeft;
		case 1:		return kBoxToTop;
		case 2:		return kBoxToTopRight;
		case 3:		return kBoxToLeft;
		case 4:		return kSameBox;
		case 5:		return kBoxToRight;
		case 6:		return kBoxToBottomLeft;
		case 7:		return kBoxToBottom;
		case 8:		return kBoxToBottomRight;
		default:	return kSameBox;
	}
}
//-------------------------------------------------------------------------------------------------
void CBoxFinderPP::displayClueLocation(EClueLocation eClueLocation)
{
	int nResourceId = 0;

	if (m_radioFindSpatialArea.GetCheck() == BST_CHECKED)
	{
		// Update the enabled status of the exclude clue area checkbox
		if (eClueLocation == kBoxToLeft ||
			eClueLocation == kSameBox ||
			eClueLocation == kBoxToRight)
		{
			m_chkExcludeClueArea.EnableWindow(TRUE);
		}
		else
		{
			m_chkExcludeClueArea.EnableWindow(FALSE);
		}
	}

	// Retrieve the bitmap ID assosiated with the new clue location setting
	switch (eClueLocation)
	{
		case kBoxToTopLeft:		nResourceId = IDB_BITMAP_CLUETOPLEFT;  break;
		case kBoxToTop:			nResourceId = IDB_BITMAP_CLUETOP;  break;
		case kBoxToTopRight:	nResourceId = IDB_BITMAP_CLUETOPRIGHT;  break;
		case kBoxToLeft:		nResourceId = IDB_BITMAP_CLUELEFT;  break;
		case kSameBox:			nResourceId = IDB_BITMAP_CLUESAME;  break;
		case kBoxToRight:		nResourceId = IDB_BITMAP_CLUERIGHT;  break;
		case kBoxToBottomLeft:	nResourceId = IDB_BITMAP_CLUEBOTTOMLEFT;  break;
		case kBoxToBottom:		nResourceId = IDB_BITMAP_CLUEBOTTOM;  break;
		case kBoxToBottomRight:	nResourceId = IDB_BITMAP_CLUEBOTTOMRIGHT;  break;
	}

	// Load the new bitmap
	if (nResourceId != 0)
	{
		m_pictClueDiagram.SetBitmap(::LoadBitmap(_Module.m_hInstResource, MAKEINTRESOURCE(nResourceId)));
	}
}
//-------------------------------------------------------------------------------------------------
void CBoxFinderPP::onSelectPages(WORD wRadioId)
{
	// Start by unchecking and disabling all page controls
	m_radioAllPages.SetCheck(BST_UNCHECKED);
	m_radioFirstPages.SetCheck(BST_UNCHECKED);
	m_editFirstPageNums.EnableWindow(FALSE);
	m_radioLastPages.SetCheck(BST_UNCHECKED);
	m_editLastPageNums.EnableWindow(FALSE);
	m_radioSpecifiedPages.SetCheck(BST_UNCHECKED);
	m_editSpecifiedPageNums.EnableWindow(FALSE);

	switch(wRadioId)
	{
		case IDC_RADIO_ALL_PAGES: 
		{
			// check all pages radio
			m_radioAllPages.SetCheck(BST_CHECKED);
			break;
		}

		case IDC_RADIO_FIRST_PAGES: 
		{
			// check first pages radio, and enable first pages edit box
			m_radioFirstPages.SetCheck(BST_CHECKED);
			m_editFirstPageNums.EnableWindow(TRUE);
			break;
		}

		case IDC_RADIO_LAST_PAGES: 
		{
			// check last pages radio, and enable last pages edit box
			m_radioLastPages.SetCheck(BST_CHECKED);
			m_editLastPageNums.EnableWindow(TRUE);
			break;
		}

		case IDC_RADIO_SPECIFIED_PAGES: 
		{
			// check specified pages radio, and enable specified pages edit box
			m_radioSpecifiedPages.SetCheck(BST_CHECKED);
			m_editSpecifiedPageNums.EnableWindow(TRUE);
			break;
		}

		default:
		{
			THROW_LOGIC_ERROR_EXCEPTION("ELI19747");
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CBoxFinderPP::onSelectFindType(WORD wRadioId)
{
	if (wRadioId == IDC_RADIO_RETURN_BOX_AREA)
	{
		// Update controls to reflect a spatial area return type
		m_radioFindSpatialArea.SetCheck(BST_CHECKED);
		m_radioFindText.SetCheck(BST_UNCHECKED);

		m_editAttributeText.EnableWindow(TRUE);
		m_chkIncludeClueText.EnableWindow(FALSE);

		UCLID_AFVALUEFINDERSLib::IBoxFinderPtr ipBoxFinder = m_ppUnk[0];
		ASSERT_RESOURCE_ALLOCATION("ELI19789", ipBoxFinder);

		EClueLocation eClueLocation = (EClueLocation) ipBoxFinder->ClueLocation;
		if (eClueLocation == kBoxToLeft ||
			eClueLocation == kSameBox ||
			eClueLocation == kBoxToRight)
		{
			m_chkExcludeClueArea.EnableWindow(TRUE);
		}
		else
		{
			m_chkExcludeClueArea.EnableWindow(FALSE);
		}
	}
	else if (wRadioId == IDC_RADIO_RETURN_TEXT)
	{
		// Update controls to reflect a text return type
		m_radioFindSpatialArea.SetCheck(BST_UNCHECKED);
		m_radioFindText.SetCheck(BST_CHECKED);

		m_editAttributeText.EnableWindow(FALSE);
		m_chkExcludeClueArea.EnableWindow(FALSE);
		m_chkIncludeClueText.EnableWindow(TRUE);
	}
	else
	{
		THROW_LOGIC_ERROR_EXCEPTION("ELI19748");
	}
}
//-------------------------------------------------------------------------------------------------
void CBoxFinderPP::updateClueButtons()
{
	// enable/disable up and down arrow key buttons appropriately
	m_btnUp.EnableWindow(FALSE);
	m_btnDown.EnableWindow(FALSE);

	// get current selected item index
	int nSelectedItemIndex = m_listClues.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
	int nSelCount = m_listClues.GetSelectedCount();
	int nCount = m_listClues.GetItemCount();
	
	if (nSelCount == 0)
	{
		// Modify and remove should be disabled if there are no clues selected
		m_btnModify.EnableWindow(FALSE);
		m_btnRemove.EnableWindow(FALSE);
	}
	else
	{
		// Modify should be enabled if there is exactly one clue selected
		m_btnModify.EnableWindow(asMFCBool(nSelCount == 1));
		// Remove should be enabled if there is at least one clue selected
		m_btnRemove.EnableWindow(asMFCBool(nSelCount >= 1));

		if ((nCount > 1) && (nSelCount == 1))
		{
			if (nSelectedItemIndex == 0)
			{
				// First item selected
				// enable down button only
				m_btnUp.EnableWindow(FALSE);
				m_btnDown.EnableWindow(TRUE);
			}
			else if (nSelectedItemIndex > 0 && nSelectedItemIndex < (nCount - 1))
			{
				// Some item other that first and last item selected
				// enable both buttons
				m_btnUp.EnableWindow(TRUE);
				m_btnDown.EnableWindow(TRUE);
			}
			else if (nSelectedItemIndex == (nCount - 1))
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
void CBoxFinderPP::validateLicense()
{
	VALIDATE_LICENSE(gnRULESET_EDITOR_UI_OBJECT, "ELI19684", "BoxFinder PP");
}
//-------------------------------------------------------------------------------------------------
