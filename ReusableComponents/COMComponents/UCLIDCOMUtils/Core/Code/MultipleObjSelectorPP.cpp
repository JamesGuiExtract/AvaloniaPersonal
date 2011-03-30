
// MultipleObjSelectorPP.cpp : Implementation of CMultipleObjSelectorPP
#include "stdafx.h"
#include "UCLIDCOMUtils.h"
#include "MultipleObjSelectorPP.h"
#include "ObjSelectDlg.h"
#include "Common.h"
#include "MiscUtils.h"

#include <TemporaryResourceOverride.h>
#include <COMUtils.h>
#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <cpputil.h>

//-------------------------------------------------------------------------------------------------
// CMultipleObjSelectorPP
//-------------------------------------------------------------------------------------------------
CMultipleObjSelectorPP::CMultipleObjSelectorPP() 
{
	try
	{
		// Check licensing
		validateLicense();

		m_dwTitleID = IDS_TITLEMultipleObjSelectorPP;
		m_dwHelpFileID = IDS_HELPFILEMultipleObjSelectorPP;
		m_dwDocStringID = IDS_DOCSTRINGMultipleObjSelectorPP;

		// create an instance of the clipboard object manager
		m_ipClipboardMgr.CreateInstance(CLSID_ClipboardObjectManager);
		ASSERT_RESOURCE_ALLOCATION("ELI09636", m_ipClipboardMgr != __nullptr);

		// instantiate MiscUtils object
		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI16029", m_ipMiscUtils != __nullptr);
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI08172")
}

//-------------------------------------------------------------------------------------------------
// Message handlers
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMultipleObjSelectorPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("CMultipleObjSelectorPP::Apply\n"));
		for (UINT i = 0; i < m_nObjects; i++)
		{
			// get the multiple-object holder
			UCLID_COMUTILSLib::IMultipleObjectHolderPtr ipMOH = m_ppUnk[i];
			ASSERT_RESOURCE_ALLOCATION("ELI08120", ipMOH != __nullptr);

			// Apply the enabled / disabled settings to the collected objects
			long lCount = m_ipObjects->Size();
			for (int j = 0; j < lCount; j++)
			{
				// Retrieve state from this checkbox
				BOOL bEnabled = m_listObjects.GetCheckState( j );

				// Retrieve this ObjectWithDescription
				UCLID_COMUTILSLib::IObjectWithDescriptionPtr ipObj = m_ipObjects->At( j );
				ASSERT_RESOURCE_ALLOCATION("ELI13616", ipObj != __nullptr);

				// Apply state to this ObjectWithDescription
				ipObj->Enabled = asVariantBool(bEnabled);
			}

			// update the objects
			ipMOH->ObjectsVector = m_ipObjects;
		}
		
		SetDirty(FALSE);
		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08133")

	// if we reached here, it's because of an exception that was caught
	return S_FALSE;
}
//-------------------------------------------------------------------------------------------------
LRESULT CMultipleObjSelectorPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// get access to the underlying multiple-object-holder
		UCLID_COMUTILSLib::IMultipleObjectHolderPtr ipMOH = m_ppUnk[0];
		ASSERT_RESOURCE_ALLOCATION("ELI08107", ipMOH != __nullptr);

		// get a reference to the objects from the multiple-object-holder
		m_ipObjects = ipMOH->ObjectsVector;
		ASSERT_RESOURCE_ALLOCATION("ELI08122", m_ipObjects != __nullptr);

		// get access the category name and object type
		m_strCategoryName = ipMOH->GetObjectCategoryName();
		m_strObjectType = ipMOH->GetObjectType();
		m_iid = ipMOH->GetRequiredIID();

		// initialize controls
		m_btnUp.SubclassDlgItem(IDC_BUTTON_UP, CWnd::FromHandle(m_hWnd));
		m_btnDown.SubclassDlgItem(IDC_BUTTON_DOWN, CWnd::FromHandle(m_hWnd));
		m_listObjects = GetDlgItem(IDC_LIST_OBJECTS);
		m_btnConfig = GetDlgItem(IDC_BUTTON_CONFIG);
		m_btnDelete = GetDlgItem(IDC_BUTTON_DELETE);
		m_staticPrompt = GetDlgItem(IDC_STATIC_PROMPT);

		// set the icons on the up/down buttons
		m_btnUp.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_UP)));
		m_btnDown.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_DOWN)));
		
		// setup the list object style
		m_listObjects.SetExtendedListViewStyle( 
			LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT | LVS_EX_CHECKBOXES );

		// Prepare column widths
		CRect rect;
		m_listObjects.GetClientRect(&rect);
		long	lEWidth = 70;
		long	lDWidth = rect.Width() - lEWidth;

		// Add 2 column headings to list
		m_listObjects.InsertColumn( 0, "Enabled", LVCFMT_CENTER, lEWidth, 0 );
		m_listObjects.InsertColumn( 1, "Description", LVCFMT_LEFT, lDWidth, 1 );

		// populate the list control from the objects vector
		long nNumItems = m_ipObjects->Size();
		for (int i = 0; i < nNumItems; i++)
		{
			// get the description of the item at the current position
			UCLID_COMUTILSLib::IObjectWithDescriptionPtr ipObj = m_ipObjects->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI08119", ipObj != __nullptr);
			CString zDescription = (LPCTSTR) ipObj->Description;

			// update the description
			m_listObjects.InsertItem(i, "");
			m_listObjects.SetItemText( i, 1, zDescription );

			// Retrieve enabled state and set check
			bool bEnabled = asCppBool(ipObj->Enabled);
			m_listObjects.SetCheckState( i, bEnabled );
		}

		// setup the static prompt to be specific to the object type
		string strPrompt = "Select ";
		strPrompt += m_strObjectType;
		strPrompt += "s";
		m_staticPrompt.SetWindowText(strPrompt.c_str());

		// Update the buttons' status
		updateButtonsStatus();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08108");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CMultipleObjSelectorPP::OnClickedButtonDown(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// get current selected item index and total item count
		int nSelectedItemIndex = m_listObjects.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
		int nTotalItems = m_listObjects.GetItemCount();

		// only perform the shift operation if appropriate entry is selected
		if (nSelectedItemIndex >= 0 && nSelectedItemIndex < nTotalItems - 1)
		{
			// get the selected item in local memory
			UCLID_COMUTILSLib::IObjectWithDescriptionPtr ipObj = 
				m_ipObjects->At(nSelectedItemIndex);
			ASSERT_RESOURCE_ALLOCATION("ELI08136", ipObj != __nullptr);

			// Retrieve current enabled/disabled state
			BOOL bEnabled = m_listObjects.GetCheckState( nSelectedItemIndex );

			// remove the selected item from list and collection
			m_listObjects.DeleteItem(nSelectedItemIndex);
			m_ipObjects->Remove(nSelectedItemIndex);

			// get the index of the item right below currently selected item
			int nBelowIndex = nSelectedItemIndex + 1;

			// now insert the item right before the item that was above
			CString zText = (LPCTSTR) ipObj->Description;
			int nActualIndex = m_listObjects.InsertItem( nBelowIndex, "" );
			m_listObjects.SetItemText( nActualIndex, 1, zText );
			m_listObjects.SetCheckState( nActualIndex, bEnabled );
			m_ipObjects->Insert(nBelowIndex, ipObj);

			// keep this item selected
			m_listObjects.SetItemState(nActualIndex, LVIS_SELECTED, LVIS_SELECTED);

			SetDirty(TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08110");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CMultipleObjSelectorPP::OnClickedButtonUp(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// get current selected item index and total item count
		int nSelectedItemIndex = m_listObjects.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);

		// only perform the shift operation if appropriate entry is selected
		if (nSelectedItemIndex > 0)
		{
			// get the selected item in local memory
			UCLID_COMUTILSLib::IObjectWithDescriptionPtr ipObj = 
				m_ipObjects->At(nSelectedItemIndex);
			ASSERT_RESOURCE_ALLOCATION("ELI19348", ipObj != __nullptr);

			// Retrieve current enabled/disabled state
			BOOL bEnabled = m_listObjects.GetCheckState( nSelectedItemIndex );

			// remove the selected item
			m_listObjects.DeleteItem(nSelectedItemIndex);
			m_ipObjects->Remove(nSelectedItemIndex);

			// get the index of the item right above currently selected item
			int nAboveIndex = nSelectedItemIndex - 1;

			// now insert the item right after the item that was above
			CString zText = (LPCTSTR) ipObj->Description;
			int nActualIndex = m_listObjects.InsertItem( nAboveIndex, "" );
			m_listObjects.SetItemText( nActualIndex, 1, zText );
			m_listObjects.SetCheckState( nActualIndex, bEnabled );
			m_ipObjects->Insert(nAboveIndex, ipObj);

			// keep this item selected
			m_listObjects.SetItemState(nActualIndex, LVIS_SELECTED, LVIS_SELECTED);

			SetDirty(TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08111");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CMultipleObjSelectorPP::OnClickedButtonInsert(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// create a new object-with-description object
		UCLID_COMUTILSLib::IObjectWithDescriptionPtr ipNewObj(CLSID_ObjectWithDescription);
		ASSERT_RESOURCE_ALLOCATION("ELI08117", ipNewObj != __nullptr);

		// create the dialog with the right parameters
		string strPrompt1 = m_strObjectType + " Description";
		string strPrompt2 = "Select " + m_strObjectType;
		CObjSelectDlg dlg(m_strObjectType, strPrompt1,
			strPrompt2, m_strCategoryName, ipNewObj, false, 0, NULL);
		
		if (dlg.DoModal() == IDOK)
		{
			CString zText = (LPCTSTR) ipNewObj->Description;

			// get the index of the first selected item (if there's an item selected)
			long nNumItems = m_listObjects.GetItemCount();
			int nSelectedItemIndex = m_listObjects.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
			if (nSelectedItemIndex == -1)
			{
				nSelectedItemIndex = nNumItems;
			}

			// Insert new item before selection or at end of list
			int nIndex = m_listObjects.InsertItem(nSelectedItemIndex, "");
			m_listObjects.SetItemText( nSelectedItemIndex, 1, zText.operator LPCTSTR() );
			m_ipObjects->Insert(nSelectedItemIndex, ipNewObj);

			// Set check
			bool bEnabled = asCppBool(ipNewObj->Enabled);
			m_listObjects.SetCheckState( nIndex, bEnabled );

			// only leave the most recent addition selected
			for (int n = 0; n <= nNumItems; n++)
			{
				int nState = (n == nIndex) ? LVIS_SELECTED : 0;
				
				m_listObjects.SetItemState(n, nState, LVIS_SELECTED);
			}

			SetDirty(TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08113");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CMultipleObjSelectorPP::OnClickedButtonDelete(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// only continue if there's at least one item selected
		int nSelectedItem = m_listObjects.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
		if (nSelectedItem == -1)
		{
			return S_OK;
		}

		// confirm if the user really wants to delete selected items
		if (MessageBox("Delete selected item(s)?", "Confirm Delete", MB_YESNO) == IDNO)
		{
			return S_OK;
		}

		// remove selected items
		long nFirstSelectedItem = nSelectedItem;
		while (nSelectedItem != -1)
		{
			// remove from the UI listbox
			m_listObjects.DeleteItem(nSelectedItem);

			// remove from internal vector
			m_ipObjects->Remove(nSelectedItem);

			// find next selected item (if any)
			nSelectedItem = m_listObjects.GetNextItem(nSelectedItem - 1, 
				((nSelectedItem == 0) ? LVNI_ALL : LVNI_BELOW) | LVNI_SELECTED);
		}
				
		// select the item in the list that was closest below the last 
		// item that was deleted
		int nTotalRemainingItems = m_listObjects.GetItemCount();
		if (nFirstSelectedItem < nTotalRemainingItems)
		{
			m_listObjects.SetItemState(nFirstSelectedItem, LVIS_SELECTED, LVIS_SELECTED);
		}
		else if (nTotalRemainingItems > 0)
		{
			// select the last item
			m_listObjects.SetItemState(nTotalRemainingItems - 1, LVIS_SELECTED, LVIS_SELECTED);
		}
		
		SetDirty(TRUE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08114");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CMultipleObjSelectorPP::OnClickedButtonConfig(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// get the index of the first selected item
		// if no item is selected, then just return (even though the
		// configure button is not clickable when no items are selected)
		// NOTE: the UI disables the configure button when multiple items are selected
		int nSelectedItemIndex = m_listObjects.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
		if (nSelectedItemIndex == -1)
		{
			return S_OK;
		}

		// get the object-with-description at the current index
		UCLID_COMUTILSLib::IObjectWithDescriptionPtr ipObj = m_ipObjects->At(nSelectedItemIndex);
		ASSERT_RESOURCE_ALLOCATION("ELI08125", ipObj != __nullptr);

		// get the position of the command button
		RECT rect;
		m_btnConfig.GetWindowRect(&rect);

		// create the context menu and allow the user to modify ipObj
		VARIANT_BOOL vbDirty = m_ipMiscUtils->HandlePlugInObjectCommandButtonClick(
			ipObj, get_bstr_t(m_strObjectType), get_bstr_t(m_strCategoryName), 
			VARIANT_FALSE, 0, NULL, rect.right, rect.top);
		
		if (vbDirty == VARIANT_TRUE)
		{
			// update the UI description
			CString zText = (LPCTSTR) ipObj->Description;
			m_listObjects.SetItemText(nSelectedItemIndex, 1, zText);

			// update the internal vector
			m_ipObjects->Set(nSelectedItemIndex, ipObj);

			// leave the recently configured item selected
			m_listObjects.SetItemState(nSelectedItemIndex, LVIS_SELECTED, LVIS_SELECTED);

			SetDirty(TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08115");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CMultipleObjSelectorPP::OnDblclkListObjects(int idCtrl, LPNMHDR pnmh, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// get the index of the selected item
		int nSelectedItemIndex = m_listObjects.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);

		// make sure an item is selected
		if (nSelectedItemIndex == -1)
		{
			return 0;
		}

		// get the currently selected object
		UCLID_COMUTILSLib::IObjectWithDescriptionPtr ipObject = m_ipObjects->At(nSelectedItemIndex);
		ASSERT_RESOURCE_ALLOCATION("ELI16032", ipObject != __nullptr);

		// modify ipObject based on user input
		VARIANT_BOOL vbDirty = m_ipMiscUtils->HandlePlugInObjectDoubleClick(ipObject, 
			get_bstr_t(m_strObjectType), get_bstr_t(m_strCategoryName), VARIANT_TRUE, 0, NULL); 

		if (vbDirty == VARIANT_TRUE)
		{
			// update the UI description
			CString zText = (LPCTSTR) ipObject->Description;
			m_listObjects.SetItemText(nSelectedItemIndex, 1, zText);

			// information has been modified
			SetDirty(TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08116");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CMultipleObjSelectorPP::OnItemChangedListObjects(int idCtrl, LPNMHDR pnmh, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Update the buttons' status
		updateButtonsStatus();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08129");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CMultipleObjSelectorPP::OnRClickListObjects(int idCtrl, LPNMHDR pnmh, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check for current Attribute selection
		int iIndex = -1;
		iIndex = m_listObjects.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);

		m_nRightClickIndex = iIndex;

		// Load the context menu
		CMenu menu;
		menu.LoadMenu( IDR_MENU_CONTEXT );
		CMenu *pContextMenu = menu.GetSubMenu( 0 );

		//////////////////////////
		// Enable or disable items
		//////////////////////////
		UINT nEnable = (MF_BYCOMMAND | MF_ENABLED);
		UINT nDisable = (MF_BYCOMMAND | MF_DISABLED | MF_GRAYED);
		bool bEnable = iIndex > -1;

		// No Attribute item selected
		pContextMenu->EnableMenuItem(ID_EDIT_CUT, bEnable ? nEnable : nDisable);
		pContextMenu->EnableMenuItem(ID_EDIT_COPY, bEnable ? nEnable : nDisable);
		pContextMenu->EnableMenuItem(ID_EDIT_DELETE, bEnable ? nEnable : nDisable);

		// Enable paste if clipboard contains either a Vector of objects
		// of type m_iid or a single object of type m_iid 
		bEnable = asCppBool(m_ipClipboardMgr->IUnknownVectorIsOWDOfType(m_iid))
			|| asCppBool(m_ipClipboardMgr->ObjectIsTypeWithDescription(m_iid));
		pContextMenu->EnableMenuItem(ID_EDIT_PASTE, bEnable ? nEnable : nDisable);

		// Map the point to the correct position
		CPoint	point;
		GetCursorPos(&point);
		
		// Display and manage the context menu
		pContextMenu->TrackPopupMenu( TPM_LEFTALIGN | TPM_LEFTBUTTON | TPM_RIGHTBUTTON, 
			point.x, point.y, CWnd::FromHandle(m_hWnd) );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09634");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CMultipleObjSelectorPP::OnEditCut(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// First copy the item to the Clipboard
		BOOL bTmp;
		OnEditCopy(0, 0, 0, bTmp);

		// Delete the item
		OnEditDelete(0, 0, 0, bTmp);

		// Update button states
	//	setButtonStates();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09635")
	
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CMultipleObjSelectorPP::OnEditCopy(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride( _Module.m_hInstResource );

	try
	{
		// Find index of first selected object
		int iIndex = -1;
		iIndex = m_listObjects.GetNextItem( -1, LVNI_ALL | LVNI_SELECTED );
		if (iIndex == -1)
		{
			// Throw exception
			throw UCLIDException( "ELI11118", "Unable to determine selected object!" );
		}

		// Create a vector for selected objects
		UCLID_COMUTILSLib::IIUnknownVectorPtr	ipCopiedObjects( CLSID_IUnknownVector );
		ASSERT_RESOURCE_ALLOCATION( "ELI11119", ipCopiedObjects != __nullptr );

		// Add each selected object to vector
		while (iIndex != -1)
		{
			// Retrieve the selected object
			IUnknownPtr	ipObject = m_ipObjects->At(iIndex);
			ASSERT_RESOURCE_ALLOCATION("ELI09638", ipObject != __nullptr );

			// Add the object to the vector
			ipCopiedObjects->PushBack( ipObject );

			// Get the next selection
			iIndex = m_listObjects.GetNextItem( iIndex, LVNI_ALL | LVNI_SELECTED );
		}

		// ClipboardManager will handle the Copy
		m_ipClipboardMgr->CopyObjectToClipboard( ipCopiedObjects );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09637")
	
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CMultipleObjSelectorPP::OnEditPaste(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	try
	{
		// Test ClipboardManager object
		IUnknownPtr	ipObject( NULL );
		bool	bSingleObject = false;
		if (asCppBool(m_ipClipboardMgr->IUnknownVectorIsOWDOfType(m_iid)))
		{
			// Object is a vector of ObjectWithDescription items of type m_iid
			ipObject = m_ipClipboardMgr->GetObjectInClipboard();
			ASSERT_RESOURCE_ALLOCATION( "ELI11120", ipObject != __nullptr );
		}
		else if (asCppBool(m_ipClipboardMgr->ObjectIsTypeWithDescription( m_iid )))
		{
			// Retrieve object from ClipboardManager
			ipObject = m_ipClipboardMgr->GetObjectInClipboard();
			ASSERT_RESOURCE_ALLOCATION("ELI09639", ipObject != __nullptr );
			bSingleObject = true;
		}
		else
		{
			// Throw exception, object is not of expected type
			throw UCLIDException( "ELI09640", 
				"Clipboard object is not of the correct object type" );
		}

		// Get index of first selected item (if there is an item selected)
		long nNumItems = m_listObjects.GetItemCount();
		int nSelectedItemIndex = m_listObjects.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
		if (nSelectedItemIndex == -1)
		{
			nSelectedItemIndex = nNumItems > 0 ? nNumItems - 1 : 0;
		}

		// Clear current selection
//		clearListSelection();

		// Handle single-object case
		if (bSingleObject)
		{
			// Retrieve object and description
			UCLID_COMUTILSLib::IObjectWithDescriptionPtr ipNewObj = ipObject;
			ASSERT_RESOURCE_ALLOCATION( "ELI11121", ipNewObj != __nullptr );
			CString zText = (LPCTSTR) ipNewObj->Description;

			// Insert new item before selection or at end of list
			int nIndex = m_listObjects.InsertItem(nSelectedItemIndex, "");
			m_listObjects.SetItemText( nSelectedItemIndex, 1, zText.operator LPCTSTR() );
			m_ipObjects->Insert(nSelectedItemIndex, ipNewObj);

			// Set check
			bool bEnabled = asCppBool(ipNewObj->Enabled);
			m_listObjects.SetCheckState( nSelectedItemIndex, bEnabled );

			// Select the new object
			m_listObjects.SetItemState( nSelectedItemIndex, LVIS_SELECTED, LVIS_SELECTED );
		}
		// Handle vector of one-or-more objects case
		else
		{
			// Get count of Objects in Clipboard vector
			UCLID_COMUTILSLib::IIUnknownVectorPtr	ipPastedObjects = ipObject;
			ASSERT_RESOURCE_ALLOCATION( "ELI11122", ipPastedObjects != __nullptr );
			int iCount = ipPastedObjects->Size();

			// Add each Object to the list and the vector
			for (int i = 0; i < iCount; i++)
			{
				// Retrieve object and description
				UCLID_COMUTILSLib::IObjectWithDescriptionPtr ipNewObj = 
					ipPastedObjects->At( i );
				ASSERT_RESOURCE_ALLOCATION( "ELI11123", ipNewObj != __nullptr );
				CString zText = (LPCTSTR) ipNewObj->Description;

				// Insert this item before selection or at end of list
				int nIndex = m_listObjects.InsertItem( nSelectedItemIndex + i, "" );
				m_listObjects.SetItemText( nSelectedItemIndex + i, 1, zText.operator LPCTSTR() );
				m_ipObjects->Insert( nSelectedItemIndex + i, ipNewObj );

				// Set check
				bool bEnabled = asCppBool(ipNewObj->Enabled);
				m_listObjects.SetCheckState( nSelectedItemIndex + i, bEnabled );

				// Select the new item
				m_listObjects.SetItemState( nSelectedItemIndex + i, LVIS_SELECTED, 
					LVIS_SELECTED );
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19371")
	
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CMultipleObjSelectorPP::OnEditDelete(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	// Call the existing handler
	return OnClickedButtonDelete(wNotifyCode, wID, hWndCtl, bHandled);
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CMultipleObjSelectorPP::updateButtonsStatus()
{
	// all buttons except the insert button are disabled by default
	m_btnUp.EnableWindow(FALSE);
	m_btnDown.EnableWindow(FALSE);
	m_btnConfig.EnableWindow(FALSE);
	m_btnDelete.EnableWindow(FALSE);

	// get current selected item index
	int nSelectedItemIndex = m_listObjects.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
	int nSelCount = m_listObjects.GetSelectedCount();
	int nLastItemIndex = m_listObjects.GetItemCount() - 1;

	if (nSelCount > 0)
	{
		// enable the config and delete buttons if appropriate
		m_btnConfig.EnableWindow( asMFCBool(nSelCount == 1) );
		m_btnDelete.EnableWindow( asMFCBool(nSelCount >= 1) );

		if (nSelCount == 1)
		{
			// enable the up button if appropriate
			if (nSelectedItemIndex > 0)
			{
				m_btnUp.EnableWindow(TRUE);
			}

			// enable the down button if appropriate
			if (nSelectedItemIndex < nLastItemIndex)
			{
				m_btnDown.EnableWindow(TRUE);
			}
		}
	}	
}
//-------------------------------------------------------------------------------------------------
void CMultipleObjSelectorPP::validateLicense()
{
	// Property Page requires Flex Index / ID Shield core license (P13 #4285)
	// Property Page requires Core Rule Writing license - WEL 5/23/08
	VALIDATE_LICENSE( gnRULE_WRITING_CORE_OBJECTS, "ELI08173", "MultipleObjSelector PP" );
}
//-------------------------------------------------------------------------------------------------
