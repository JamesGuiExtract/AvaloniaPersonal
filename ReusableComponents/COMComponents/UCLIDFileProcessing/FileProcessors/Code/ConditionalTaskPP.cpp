// ConditionalTaskPP.cpp : Implementation of CConditionalTaskPP

#include "stdafx.h"
#include "ConditionalTaskPP.h"
#include "..\..\..\UCLIDFileProcessing\Code\FPCategories.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <COMUtils.h>
#include <cpputil.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const bstr_t	gbstrFAM_CONDITION_DISPLAY_NAME = "Condition";

// Width of Run column.  Task column fills the rest of the list
const int		gnRUN_COLUMN_WIDTH = 55;

// Position of each column in list
const int		gnRUN_COLUMN = 0;
const int		gnTASK_COLUMN = 1;

//-------------------------------------------------------------------------------------------------
// CConditionalTaskPP
//-------------------------------------------------------------------------------------------------
CConditionalTaskPP::CConditionalTaskPP() :
m_iLastClickedResourceID(-1)
{
	try
	{
		// Check licensing
		validateLicense();

		m_dwTitleID = IDS_TITLECONDITIONALTASKPP;
		m_dwHelpFileID = IDS_HELPFILECONDITIONALTASKPP;
		m_dwDocStringID = IDS_DOCSTRINGCONDITIONALTASKPP;

		// create an instance of the clipboard object manager
		m_ipClipboardMgr.CreateInstance(CLSID_ClipboardObjectManager);
		ASSERT_RESOURCE_ALLOCATION("ELI23615", m_ipClipboardMgr != NULL);
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI16196")
}
//--------------------------------------------------------------------------------------------------
CConditionalTaskPP::~CConditionalTaskPP()
{
	try
	{
		m_ipClipboardMgr = NULL;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16611")
}
//-------------------------------------------------------------------------------------------------
HRESULT CConditionalTaskPP::FinalConstruct()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
void CConditionalTaskPP::FinalRelease()
{
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalTaskPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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

//--------------------------------------------------------------------------------------------------
// IPropertyPage
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CConditionalTaskPP::Apply()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("CConditionalTaskPP::Apply\n"));

		// Update the settings in each of the objects associated with this UI
		for (UINT i = 0; i < m_nObjects; i++)
		{
			// Retrieve the Conditional Task object
			UCLID_FILEPROCESSORSLib::IConditionalTaskPtr ipFP(m_ppUnk[i]);
			ASSERT_RESOURCE_ALLOCATION("ELI16199", ipFP != NULL);

			// Validate object settings as appropriate
			validateSettings();
		}

		SetDirty(FALSE);
		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16198")

	// An exception was caught
	return S_FALSE;
}

//--------------------------------------------------------------------------------------------------
// Message handlers
//--------------------------------------------------------------------------------------------------
LRESULT CConditionalTaskPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Get the underlying objet
		m_ipConditionalTaskFP = m_ppUnk[0];
		ASSERT_RESOURCE_ALLOCATION("ELI19459", m_ipConditionalTaskFP != NULL);

		// Prepare controls
		prepareControls();

		// Retrieve the FAM Condition
		IObjectWithDescriptionPtr ipFAMCondition = m_ipConditionalTaskFP->FAMCondition;
		ASSERT_RESOURCE_ALLOCATION("ELI16268", ipFAMCondition != NULL);

		// Display the FAM Condition
		CString zText = (LPCSTR) ipFAMCondition->Description;
		m_editConditionDescription.SetWindowText( zText );

		// Refresh the task lists - without selection
		refreshTasks( m_listTrueTasks, getTrueTasks() );
		refreshTasks( m_listFalseTasks, getFalseTasks() );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16200")

	return TRUE;
}
//-------------------------------------------------------------------------------------------------
LRESULT CConditionalTaskPP::OnBtnSelectCondition(WORD wNotifyCode, WORD wID, HWND hWndCtl, 
												 BOOL& bHandled) 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{	
		// Get the position of the "Commands >" button
		RECT rect;
		m_btnSelectCondition.GetWindowRect(&rect);

		// Prompt user to select and configure FAM Condition
		VARIANT_BOOL vbDirty = getMiscUtils()->HandlePlugInObjectCommandButtonClick(
			m_ipConditionalTaskFP->FAMCondition, gbstrFAM_CONDITION_DISPLAY_NAME, 
			get_bstr_t(FP_FAM_CONDITIONS_CATEGORYNAME), VARIANT_TRUE, 0, NULL, rect.right, rect.top);

		// Check if FAM Condition has been modified
		if (vbDirty == VARIANT_TRUE)
		{
			// Retrieve FAM Condition
			IObjectWithDescriptionPtr ipOWD = m_ipConditionalTaskFP->FAMCondition;
			if (ipOWD == NULL)
			{
				// Clear description if no FAM Condition object
				m_editConditionDescription.SetWindowText( "" );
			}
			else
			{
				// Update control text to match description
				m_editConditionDescription.SetWindowText( 
					static_cast<const char*>(ipOWD->Description) );
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16267")

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CConditionalTaskPP::OnLButtonDblClk(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Handling the Left button dbl click on the property page was implemented instead
		// of having the different methods for the controls, to fix the issue with double click 
		// copying the label contents to the clipboard FlexIDSCore #4227

		// Get the window ID that the mouse is in
		POINT pointMouse;
		pointMouse.x = GET_X_LPARAM(lParam); 
		pointMouse.y = GET_Y_LPARAM(lParam); 
		int iID = ChildWindowFromPointEx(pointMouse,CWP_SKIPTRANSPARENT).GetDlgCtrlID();

		// If the mouse was double clicked in condition control - configure
		if (iID == IDC_EDIT_CONDITION)
		{
			// Prompt user to configure FAM Condition
			VARIANT_BOOL vbDirty = getMiscUtils()->HandlePlugInObjectDoubleClick(
				m_ipConditionalTaskFP->FAMCondition, gbstrFAM_CONDITION_DISPLAY_NAME, 
				FP_FAM_CONDITIONS_CATEGORYNAME.c_str(), VARIANT_TRUE, 0, NULL);

			// Check if FAM Condition has been modified
			if (vbDirty == VARIANT_TRUE)
			{
				// Retrieve FAM Condition
				IObjectWithDescriptionPtr ipOWD = m_ipConditionalTaskFP->FAMCondition;
				if (ipOWD == NULL)
				{
					// Clear description if no FAM Condition object
					m_editConditionDescription.SetWindowText( "" );
				}
				else
				{
					// Update control text to match description
					m_editConditionDescription.SetWindowText( 
						static_cast<const char*>(ipOWD->Description) );
				}
			}

			bHandled = TRUE;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19111");

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CConditionalTaskPP::OnClickedBtnAdd(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Create a new ObjectWithDescription
		IObjectWithDescriptionPtr ipObject(CLSID_ObjectWithDescription);
		ASSERT_RESOURCE_ALLOCATION("ELI16272", ipObject != NULL);

		// Allow the user to select and configure
		VARIANT_BOOL vbDirty = getMiscUtils()->AllowUserToSelectAndConfigureObject( ipObject, 
			"Task",	get_bstr_t(FP_FILE_PROC_CATEGORYNAME), VARIANT_FALSE, 0, NULL );

		// Check for OK
		if (asCppBool(vbDirty))
		{
			// Validate ID and get appropriate list and tasks collection
			ATLControls::CListViewCtrl rList;
			IIUnknownVectorPtr ipCollection = NULL;
			getListAndTasks( wID, IDC_BTN_ADD, IDC_BTN_ADD2, rList, ipCollection );

			// Retrieve the insert position
			int iIndex = getInsertPosition( rList );

			// Insert the object-with-description into the vector and refresh the list
			ipCollection->Insert( iIndex, ipObject );
			refreshTasks( rList, ipCollection, iIndex );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16273")

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CConditionalTaskPP::OnClickedBtnRemove(WORD wNotifyCode, WORD wID, HWND hWndCtl, 
											   BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate ID and get appropriate list and tasks collection
		ATLControls::CListViewCtrl rList;
		IIUnknownVectorPtr ipCollection = NULL;
		getListAndTasks( wID, IDC_BTN_REMOVE, IDC_BTN_REMOVE2, rList, ipCollection );

		// Remove the selected items from the list
		removeSelectedItems(rList, ipCollection);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16284")

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CConditionalTaskPP::OnClickedBtnModify(WORD wNotifyCode, WORD wID, HWND hWndCtl, 
											   BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate ID and get appropriate list and tasks collection
		ATLControls::CListViewCtrl rList;
		IIUnknownVectorPtr ipCollection = NULL;
		getListAndTasks( wID, IDC_BTN_MODIFY, IDC_BTN_MODIFY2, rList, ipCollection );

		// Get index of current selection and retrieve the associated task
		int nSelectedItemIndex = rList.GetNextItem( -1, LVNI_ALL | LVNI_SELECTED );
		ASSERT_ARGUMENT( "ELI16289", nSelectedItemIndex >= 0 );
		IObjectWithDescriptionPtr ipObject = ipCollection->At( nSelectedItemIndex );
		ASSERT_RESOURCE_ALLOCATION("ELI16290", ipObject != NULL);

		// Get the position and dimensions of the command button
		RECT rectCommandButton;
		getDlgItemWindowRect( wID, rectCommandButton );

		// Allow the user to modify some or all of the task
		VARIANT_BOOL vbOK = getMiscUtils()->HandlePlugInObjectCommandButtonClick( ipObject, 
			"Task",	get_bstr_t(FP_FILE_PROC_CATEGORYNAME), VARIANT_FALSE, 0, NULL, 
			rectCommandButton.right, rectCommandButton.top);

		// Check for OK
		if (asCppBool(vbOK))
		{
			// Update the task collection
			ipCollection->Remove( nSelectedItemIndex );
			ipCollection->Insert( nSelectedItemIndex, ipObject );

			// Refresh the list
			refreshTasks( rList, ipCollection, nSelectedItemIndex );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16288")

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CConditionalTaskPP::OnClickedBtnUp(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate ID and get appropriate list and tasks collection
		ATLControls::CListViewCtrl rList;
		IIUnknownVectorPtr ipCollection = NULL;
		getListAndTasks( wID, IDC_BTN_UP, IDC_BTN_UP2, rList, ipCollection );

		// Get index of current selection
		int nSelectedItemIndex = rList.GetNextItem( -1, LVNI_ALL | LVNI_SELECTED );
		if (nSelectedItemIndex > 0)
		{
			// Exchange position of tasks within collection
			ipCollection->Swap( nSelectedItemIndex, nSelectedItemIndex - 1 );

			// Refresh the display, keeping the moved task selected
			refreshTasks( rList, ipCollection, nSelectedItemIndex - 1 );
		}
		// else first task is selected and cannot move up
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16280");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CConditionalTaskPP::OnClickedBtnDown(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate ID and get appropriate list and tasks collection
		ATLControls::CListViewCtrl rList;
		IIUnknownVectorPtr ipCollection = NULL;
		getListAndTasks( wID, IDC_BTN_DOWN, IDC_BTN_DOWN2, rList, ipCollection );

		// Get item count
		int iCount = rList.GetItemCount();

		// Get index of current selection
		int nSelectedItemIndex = rList.GetNextItem( -1, LVNI_ALL | LVNI_SELECTED );
		if (nSelectedItemIndex < iCount - 1)
		{
			// Exchange position of tasks within collection
			ipCollection->Swap( nSelectedItemIndex, nSelectedItemIndex + 1 );

			// Refresh the display, keeping the moved task selected
			refreshTasks( rList, ipCollection, nSelectedItemIndex + 1 );
		}
		// else last task is selected and cannot move down
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16281");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CConditionalTaskPP::OnItemChangedList(int idCtrl, LPNMHDR pnmh, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Retrieve notification structure
		LPNMLISTVIEW pLV = (LPNMLISTVIEW)pnmh;

		// Validate ID and get appropriate list and tasks collection
		ATLControls::CListViewCtrl rList;
		IIUnknownVectorPtr ipCollection = NULL;
		getListAndTasks( idCtrl, IDC_LIST_TRUE, IDC_LIST_FALSE, rList, ipCollection );

		// Get item number
		int iItem = pLV->iItem;

		// Get checkbox state from the list
		bool bListEnabled = asCppBool( rList.GetCheckState( iItem ) );

		// Get currently saved enabled/disabled setting from the collection
		bool bEnabled = asCppBool( getMiscUtils()->GetEnabledState( ipCollection, iItem ) );

		// Update collection if needed
		if (bListEnabled != bEnabled)
		{
			getMiscUtils()->SetEnabledState( ipCollection, iItem, 
				asVariantBool( bListEnabled ) );
		}
		// else no update because the checkbox has not changed state

		// Update button states
		setButtonStates(rList);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16279");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CConditionalTaskPP::OnDblclkList(int idCtrl, LPNMHDR pnmh, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Retrieve notification structure
		LPNMLISTVIEW pLV = (LPNMLISTVIEW)pnmh;

		// Validate ID and get appropriate list and tasks collection
		ATLControls::CListViewCtrl rList;
		IIUnknownVectorPtr ipCollection = NULL;
		getListAndTasks( idCtrl, IDC_LIST_TRUE, IDC_LIST_FALSE, rList, ipCollection );

		// Get item number
		int iItem = pLV->iItem;
		if (iItem == LB_ERR)
		{
			// If no item is selected, nothing to do
			return 0;
		}

		// Retrieve the task
		IObjectWithDescriptionPtr ipTask = ipCollection->At( iItem );
		ASSERT_RESOURCE_ALLOCATION("ELI16287", ipTask != NULL);

		// Allow the user to modify the task
		VARIANT_BOOL vbOK = getMiscUtils()->HandlePlugInObjectDoubleClick( ipTask, 
			"Task",	get_bstr_t(FP_FILE_PROC_CATEGORYNAME), VARIANT_FALSE, 0, NULL );

		// Check for OK
		if (asCppBool(vbOK))
		{
			// Update the task collection
			ipCollection->Remove( iItem );
			ipCollection->Insert( iItem, ipTask );

			// Refresh the list
			refreshTasks( rList, ipCollection, iItem );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16286");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CConditionalTaskPP::OnRClickList(int idCtrl, LPNMHDR pnmh, BOOL &bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate ID
		if ((idCtrl != IDC_LIST_TRUE) && (idCtrl != IDC_LIST_FALSE))
		{
			UCLIDException ue( "ELI23616", "Invalid resource ID!" );
			ue.addDebugInfo( "Requested ID", idCtrl );
			ue.addDebugInfo( "True List ID", IDC_LIST_TRUE );
			ue.addDebugInfo( "False List ID", IDC_LIST_FALSE );
			throw ue;
		}

		// Set the last clicked resource ID
		m_iLastClickedResourceID = idCtrl;

		// Get appropriate list and tasks collection
		ATLControls::CListViewCtrl rList = (idCtrl == IDC_LIST_TRUE)
			? m_listTrueTasks : m_listFalseTasks;

		// Get index of first selection (will return -1 if nothing selected)
		int iIndex = rList.GetNextItem( -1, LVNI_ALL | LVNI_SELECTED );
	
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

		// Enable menu items based on selection
		pContextMenu->EnableMenuItem(IDC_EDIT_CUT, bEnable ? nEnable : nDisable);
		pContextMenu->EnableMenuItem(IDC_EDIT_COPY, bEnable ? nEnable : nDisable);
		pContextMenu->EnableMenuItem(IDC_EDIT_DELETE, bEnable ? nEnable : nDisable);

		if (m_ipClipboardMgr->IUnknownVectorIsOWDOfType( 
			IID_IFileProcessingTask)
			|| m_ipClipboardMgr->ObjectIsTypeWithDescription(IID_IFileProcessingTask))
		{
			bEnable = true;
		}
		else
		{
			bEnable = false;
		}
		pContextMenu->EnableMenuItem(IDC_EDIT_PASTE, bEnable ? nEnable : nDisable);

		// Map the point to the correct position
		CPoint	point;
		GetCursorPos(&point);
		
		// Display and manage the context menu
		pContextMenu->TrackPopupMenu( TPM_LEFTALIGN | TPM_LEFTBUTTON | TPM_RIGHTBUTTON, 
			point.x, point.y, CWnd::FromHandle(m_hWnd) );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI23617");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CConditionalTaskPP::OnEditCut(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL &bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// First copy the item to the Clipboard
		BOOL bTmp;
		OnEditCopy(0, 0, 0, bTmp);

		// Delete the item
		OnEditDelete(0, 0, 0, bTmp);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI23618");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CConditionalTaskPP::OnEditCopy(WORD wNotifyCode, WORD wID, HWND hWndCtl,
											BOOL &bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate ID and get appropriate list and tasks collection
		ATLControls::CListViewCtrl rList;
		IIUnknownVectorPtr ipCollection = NULL;
		getListAndTasks( m_iLastClickedResourceID, IDC_LIST_TRUE, IDC_LIST_FALSE,
			rList, ipCollection );

		// Find index of first selected object
		int iIndex = -1;
		iIndex = rList.GetNextItem( -1, LVNI_ALL | LVNI_SELECTED );
		if (iIndex == -1)
		{
			// Throw exception
			throw UCLIDException( "ELI23619", "Unable to determine selected object!" );
		}

		// Create a vector for selected objects
		UCLID_COMUTILSLib::IIUnknownVectorPtr	ipCopiedObjects( CLSID_IUnknownVector );
		ASSERT_RESOURCE_ALLOCATION( "ELI23620", ipCopiedObjects != NULL );

		// Add each selected object to vector
		while (iIndex != -1)
		{
			// Retrieve the selected object
			IUnknownPtr	ipObject = ipCollection->At(iIndex);
			ASSERT_RESOURCE_ALLOCATION("ELI23621", ipObject != NULL );

			// Add the object to the vector
			ipCopiedObjects->PushBack( ipObject );

			// Get the next selection
			iIndex = rList.GetNextItem( iIndex, LVNI_ALL | LVNI_SELECTED );
		}

		// ClipboardManager will handle the Copy
		m_ipClipboardMgr->CopyObjectToClipboard( ipCopiedObjects );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI23622");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CConditionalTaskPP::OnEditPaste(WORD wNotifyCode, WORD wID, HWND hWndCtl,
											BOOL &bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate ID and get appropriate list and tasks collection
		ATLControls::CListViewCtrl rList;
		IIUnknownVectorPtr ipCollection = NULL;
		getListAndTasks( m_iLastClickedResourceID, IDC_LIST_TRUE, IDC_LIST_FALSE,
			rList, ipCollection );

		// Test ClipboardManager object
		IUnknownPtr	ipObject = NULL;
		bool	bSingleObject = false;
		if (m_ipClipboardMgr->IUnknownVectorIsOWDOfType( 
			IID_IFileProcessingTask ))
		{
			// Object is a vector of ObjectWithDescription items
			// We expect each embedded object to be of type IFileProcessingTask
			ipObject = m_ipClipboardMgr->GetObjectInClipboard();
			ASSERT_RESOURCE_ALLOCATION( "ELI23623", ipObject != NULL );
		}
		else if (m_ipClipboardMgr->ObjectIsTypeWithDescription(IID_IFileProcessingTask))
		{
			// Retrieve object from ClipboardManager
			ipObject = m_ipClipboardMgr->GetObjectInClipboard();
			ASSERT_RESOURCE_ALLOCATION("ELI23624", ipObject != NULL );
			bSingleObject = true;
		}
		else
		{
			// Throw exception, object is not of expected type
			throw UCLIDException( "ELI23625", 
				"Clipboard object is not of the correct object type" );
		}

		// Get index of first selected item (if there is an item selected)
		long nNumItems = rList.GetItemCount();
		int nSelectedItemIndex = getInsertPosition(rList);

		// Handle single-object case
		if (bSingleObject)
		{
			// Retrieve object and description
			UCLID_COMUTILSLib::IObjectWithDescriptionPtr ipNewObj = ipObject;
			ASSERT_RESOURCE_ALLOCATION( "ELI23626", ipNewObj != NULL );

			// Insert into the collection vector
			ipCollection->Insert(nSelectedItemIndex, ipObject);
		}
		// Handle vector of one-or-more objects case
		else
		{
			// Get count of Objects in Clipboard vector
			UCLID_COMUTILSLib::IIUnknownVectorPtr	ipPastedObjects = ipObject;
			ASSERT_RESOURCE_ALLOCATION( "ELI23627", ipPastedObjects != NULL );
			long iCount = ipPastedObjects->Size();

			// Add each Object to the vector
			for (long i = 0; i < iCount; i++)
			{
				// Retrieve object and description
				UCLID_COMUTILSLib::IObjectWithDescriptionPtr ipNewObj = 
					ipPastedObjects->At( i );
				ASSERT_RESOURCE_ALLOCATION( "ELI23628", ipNewObj != NULL );

				// Insert into the collection vector
				ipCollection->Insert(nSelectedItemIndex, ipNewObj);
				nSelectedItemIndex++;
			}

			// Decrement the select item index by 1
			nSelectedItemIndex--;
		}

		// Refresh the task list
		refreshTasks(rList, ipCollection, nSelectedItemIndex);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI23629");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CConditionalTaskPP::OnEditDelete(WORD wNotifyCode, WORD wID, HWND hWndCtl,
											BOOL &bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Validate ID and get appropriate list and tasks collection
		ATLControls::CListViewCtrl rList;
		IIUnknownVectorPtr ipCollection = NULL;
		getListAndTasks( m_iLastClickedResourceID, IDC_LIST_TRUE, IDC_LIST_FALSE,
			rList, ipCollection );

		// Remove the selected items from the list
		removeSelectedItems(rList, ipCollection);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI23630");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CConditionalTaskPP::clearListSelection(ATLControls::CListViewCtrl &rList)
{
	// Get index of first selection
	int nSelectedItemIndex = rList.GetNextItem( -1, LVNI_ALL | LVNI_SELECTED );
	
	// Clear selection for each selected item
	while (nSelectedItemIndex  > -1)
	{
		// Clear the selection state of this item
		rList.SetItemState( nSelectedItemIndex, 0, LVIS_SELECTED );

		// Get index of next selected item
		nSelectedItemIndex = rList.GetNextItem( nSelectedItemIndex, LVNI_ALL | LVNI_SELECTED );
	}
}
//-------------------------------------------------------------------------------------------------
void CConditionalTaskPP::getDlgItemWindowRect(UINT uiDlgItemResourceID, RECT &rectWindow)
{
	// Retrieve the dialog item using its resource ID
	ATL::CWindow dlgItem = GetDlgItem( uiDlgItemResourceID );

	// Get the window rect
	dlgItem.GetWindowRect( &rectWindow );
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CConditionalTaskPP::getFalseTasks()
{
	ASSERT_RESOURCE_ALLOCATION("ELI16275", m_ipConditionalTaskFP != NULL );
	return m_ipConditionalTaskFP->TasksForConditionFalse;
}
//-------------------------------------------------------------------------------------------------
int CConditionalTaskPP::getInsertPosition(ATLControls::CListViewCtrl &rList)
{
	// Get index of first selection
	int nSelectedItemIndex = rList.GetNextItem( -1, LVNI_ALL | LVNI_SELECTED );

	// If no current selection, insert item at end of list
	if (nSelectedItemIndex == -1)
	{
		nSelectedItemIndex = rList.GetItemCount();
	}
	// Else insert after the first selection
	else
	{
		nSelectedItemIndex++;
	}

	return nSelectedItemIndex;
}
//-------------------------------------------------------------------------------------------------
void CConditionalTaskPP::getListAndTasks(WORD wID, int nID1, int nID2, 
										 ATLControls::CListViewCtrl& rList, 
										 IIUnknownVectorPtr &ripCollection)
{
	// Validate ID
	if ((wID != nID1) && (wID != nID2))
	{
		UCLIDException ue( "ELI16401", "Invalid resource ID!" );
		ue.addDebugInfo( "Requested ID", wID );
		ue.addDebugInfo( "First Provided ID", nID1 );
		ue.addDebugInfo( "Second Provided ID", nID2 );
		throw ue;
	}

	// Get appropriate list and tasks collection
	rList = (wID == nID1) ? m_listTrueTasks : m_listFalseTasks;
	ripCollection = (wID == nID1) ? getTrueTasks() : getFalseTasks();
	ASSERT_RESOURCE_ALLOCATION("ELI16403", ripCollection != NULL);
}
//-------------------------------------------------------------------------------------------------
IMiscUtilsPtr CConditionalTaskPP::getMiscUtils()
{
	if (m_ipMiscUtils == NULL)
	{
		m_ipMiscUtils.CreateInstance(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI16266", m_ipMiscUtils != NULL );
	}
	
	return m_ipMiscUtils;
}
//-------------------------------------------------------------------------------------------------
IIUnknownVectorPtr CConditionalTaskPP::getTrueTasks()
{
	ASSERT_RESOURCE_ALLOCATION("ELI16274", m_ipConditionalTaskFP != NULL );
	return m_ipConditionalTaskFP->TasksForConditionTrue;
}
//-------------------------------------------------------------------------------------------------
void CConditionalTaskPP::prepareControls()
{
	// Get controls for True tasks and False tasks
	m_btnAdd = GetDlgItem( IDC_BTN_ADD );
	m_btnRemove = GetDlgItem( IDC_BTN_REMOVE );
	m_btnModify = GetDlgItem( IDC_BTN_MODIFY );

	m_btnAdd2 = GetDlgItem( IDC_BTN_ADD2 );
	m_btnRemove2 = GetDlgItem( IDC_BTN_REMOVE2 );
	m_btnModify2 = GetDlgItem( IDC_BTN_MODIFY2 );

	// Get controls and load icons for up and down buttons - set 1
	m_btnUp.SubclassDlgItem(IDC_BTN_UP, CWnd::FromHandle(m_hWnd));
	m_btnDown.SubclassDlgItem(IDC_BTN_DOWN, CWnd::FromHandle(m_hWnd));
	m_btnUp.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_UP)));
	m_btnDown.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_DOWN)));

	// Get controls and load icons for up and down buttons - set 2
	m_btnUp2.SubclassDlgItem(IDC_BTN_UP2, CWnd::FromHandle(m_hWnd));
	m_btnDown2.SubclassDlgItem(IDC_BTN_DOWN2, CWnd::FromHandle(m_hWnd));
	m_btnUp2.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_UP)));
	m_btnDown2.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_DOWN)));

	// Get Condition items
	m_editConditionDescription = GetDlgItem( IDC_EDIT_CONDITION );
	m_btnSelectCondition = GetDlgItem( IDC_BTN_SELECT_CONDITION );

	// Get True tasks items
	m_listTrueTasks = GetDlgItem( IDC_LIST_TRUE );

	// Get False tasks items
	m_listFalseTasks = GetDlgItem( IDC_LIST_FALSE );

	// Get width of Task column for lists
	CRect rectList;
	m_listTrueTasks.GetClientRect( rectList );
	long nWidth = rectList.Width() - gnRUN_COLUMN_WIDTH;

	// Add grid lines and check boxes to lists
	m_listTrueTasks.SetExtendedListViewStyle( 
		LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT | LVS_EX_CHECKBOXES );
	m_listFalseTasks.SetExtendedListViewStyle( 
		LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT | LVS_EX_CHECKBOXES );

	// Add columns to lists
	m_listTrueTasks.InsertColumn( gnRUN_COLUMN, "Run", LVCFMT_LEFT, gnRUN_COLUMN_WIDTH, gnRUN_COLUMN );
	m_listTrueTasks.InsertColumn( gnTASK_COLUMN, "Task", LVCFMT_LEFT, nWidth, gnTASK_COLUMN );
	m_listFalseTasks.InsertColumn( gnRUN_COLUMN, "Run", LVCFMT_LEFT, gnRUN_COLUMN_WIDTH, gnRUN_COLUMN );
	m_listFalseTasks.InsertColumn( gnTASK_COLUMN, "Task", LVCFMT_LEFT, nWidth, gnTASK_COLUMN );
}
//-------------------------------------------------------------------------------------------------
void CConditionalTaskPP::refreshTasks(ATLControls::CListViewCtrl &rList, 
									  IIUnknownVectorPtr ipCollection, int iTaskForSelection)
{
	ASSERT_ARGUMENT( "ELI16637", ipCollection != NULL );

	// Clear the list
	rList.DeleteAllItems();

	// Step through collected tasks and add each one to the list
	long lCount = ipCollection->Size();
	for (int i = 0; i < lCount; i++)
	{
		// Retrieve this ObjectWithDescription
		IObjectWithDescriptionPtr ipOWD = ipCollection->At( i );
		ASSERT_RESOURCE_ALLOCATION("ELI16277", ipOWD != NULL);

		// Retrieve associated description and enabled flag
		string strDescription = asString( ipOWD->Description );
		bool bEnabled = asCppBool( ipOWD->Enabled );

		// Insert an item without text into the list
		rList.InsertItem( i, "" );

		// Update description and checkbox
		rList.SetItemText( i, gnTASK_COLUMN, strDescription.c_str() );
		rList.SetCheckState( i, asMFCBool( bEnabled ) );
	}

	// Cannot select task past the end of the list
	if (iTaskForSelection >= lCount)
	{
		iTaskForSelection = lCount - 1;
	}

	// Select the item
	if (iTaskForSelection != -1)
	{
		rList.SetItemState( iTaskForSelection, LVIS_SELECTED, LVIS_SELECTED );
		rList.SetFocus();
	}

	// Update button states
	setButtonStates(rList);
}
//-------------------------------------------------------------------------------------------------
void CConditionalTaskPP::setButtonStates(ATLControls::CListViewCtrl &rList)
{
	// Determine button association with list
	if (rList == m_listTrueTasks)
	{
		updateButtons( rList, m_btnAdd, m_btnRemove, m_btnModify, m_btnUp, m_btnDown );
	}
	else if (rList == m_listFalseTasks)
	{
		updateButtons( rList, m_btnAdd2, m_btnRemove2, m_btnModify2, m_btnUp2, m_btnDown2 );
	}
	else
	{
		THROW_LOGIC_ERROR_EXCEPTION("ELI16276");
	}
}
//-------------------------------------------------------------------------------------------------
void CConditionalTaskPP::updateButtons(ATLControls::CListViewCtrl &rList, 
									   ATLControls::CButton &rbtnAdd, ATLControls::CButton &rbtnRemove, 
									   ATLControls::CButton &rbtnModify, CImageButtonWithStyle &rbtnUp, 
									   CImageButtonWithStyle &rbtnDown)
{
	// Enable the Add button
	rbtnAdd.EnableWindow( TRUE );

	// Check count of selected file processors
	int	iSelectedCount = rList.GetSelectedCount();
	if (iSelectedCount == 0)
	{
		// Disable other buttons
		rbtnRemove.EnableWindow(FALSE);
		rbtnModify.EnableWindow(FALSE);
		rbtnUp.EnableWindow(FALSE);
		rbtnDown.EnableWindow(FALSE);
	}
	else
	{	
		// An item is selected

		// Can always remove selected item(s)
		rbtnRemove.EnableWindow(TRUE);

		// Check for multiple selection
		if (iSelectedCount > 1)
		{
			// Multiple selection, disable remaining buttons
			rbtnModify.EnableWindow(FALSE);
			rbtnUp.EnableWindow(FALSE);
			rbtnDown.EnableWindow(FALSE);
		}
		else
		{
			// Single selection, enable Commands button
			rbtnModify.EnableWindow( TRUE );

			// Enable Up button if selection not at top of list
			int iSelected = rList.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
			rbtnUp.EnableWindow( asMFCBool(iSelected > 0) );

			// Enable Down button if selection not at bottom of list
			int iCount = rList.GetItemCount();
			rbtnDown.EnableWindow( asMFCBool(iSelected < iCount - 1) );
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CConditionalTaskPP::validateLicense()
{
	static const unsigned long CONDITIONALTASK_PP_COMPONENT_ID = gnFILE_ACTION_MANAGER_OBJECTS;

	VALIDATE_LICENSE( CONDITIONALTASK_PP_COMPONENT_ID, "ELI16197", 
		"Conditional Task File Processor PP" );
}
//-------------------------------------------------------------------------------------------------
void CConditionalTaskPP::validateSettings()
{
	// Confirm valid FAM Condition object
	IObjectWithDescriptionPtr ipOWD = m_ipConditionalTaskFP->FAMCondition;
	ASSERT_RESOURCE_ALLOCATION("ELI16533", ipOWD != NULL);
	IFAMConditionPtr ipObject = ipOWD->Object;
	if (ipObject == NULL)
	{
		UCLIDException ue("ELI16292", "A Condition object must be defined!");
		throw ue;
	}

	// At least one task must be enabled
	int nTrueEnabledCount = getMiscUtils()->CountEnabledObjectsIn( getTrueTasks() );
	int nFalseEnabledCount = getMiscUtils()->CountEnabledObjectsIn( getFalseTasks() );
	if (nTrueEnabledCount + nFalseEnabledCount == 0)
	{
		UCLIDException ue("ELI16293", "At least one task must be defined and enabled!");
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void CConditionalTaskPP::removeSelectedItems(ATLControls::CListViewCtrl &rList,
											 IIUnknownVectorPtr ipCollection)
{
	try
	{
		// Get count of selected items - must be at least one
		int iCount = rList.GetSelectedCount();
		ASSERT_ARGUMENT( "ELI16285", iCount > 0 );

		// Build confirmation message
		string strText = "Delete selected item";
		if (iCount > 1)
		{
			strText += string( "s" );
		}
		strText += string( "?" );

		// Display confirmation message
		int nRes = MessageBox( strText.c_str(), "Confirm Delete", MB_YESNO);
		if (nRes == IDYES)
		{
			// Get index of first selection
			int nFirstSelection = rList.GetNextItem( -1, LVNI_ALL | LVNI_SELECTED );
			int nSelectedItemIndex = nFirstSelection;

			// Remove each selected item from the collection
			while (nSelectedItemIndex != -1)
			{
				// Remove this item from list, then from collection
				rList.DeleteItem( nSelectedItemIndex );
				ipCollection->Remove( nSelectedItemIndex );

				// Find next selected item
				nSelectedItemIndex = rList.GetNextItem( nSelectedItemIndex - 1, 
					LVNI_ALL | LVNI_SELECTED );
			}

			// Refresh the list
			refreshTasks( rList, ipCollection, nFirstSelection );
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI23632");
}
//-------------------------------------------------------------------------------------------------