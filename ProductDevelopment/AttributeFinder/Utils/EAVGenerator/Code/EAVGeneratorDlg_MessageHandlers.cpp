// EAVGeneratorDlg_MessageHandlers.cpp : implementation EAVGeneratorDlg message handlers

#include "stdafx.h"
#include "EAVGenerator.h"
#include "EAVGeneratorDlg.h"

#include <UCLIDException.h>
#include <RequiredInterfaces.h>
#include <AFCategories.h>
#include <FileDialogEx.h>
#include <cpputil.h>
#include <comutils.h>
#include <LicenseMgmt.h>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const string gstrOPEN_FILE_FILTER =
	"Attribute files (*.eav; *.voa; *.evoa)|*.eav;*.voa;*.evoa"
	"|EAV files (*.eav)|*.eav|VOA files (*.voa; *.evoa)|*.voa;*.evoa||";
const string gstrSAVE_FILE_FILTER =
	"EAV files (*.eav)|*.eav|VOA files (*.voa)|*.voa|Expected VOA Files (*.evoa)|*.evoa||";

// add license management function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// CEAVGeneratorDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CEAVGeneratorDlg::OnInitDialog()
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	
	try
	{
		try
		{
			CDialog::OnInitDialog();

			// Set the icon for this dialog.  The framework does this automatically
			//  when the application's main window is not a dialog
			SetIcon(m_hIcon, TRUE);			// Set big icon
			SetIcon(m_hIcon, FALSE);		// Set small icon
	
			// create the toolbar associated with this window
			createToolBar();

			// load icons for all buttons
			HINSTANCE hInst = AfxGetInstanceHandle();
			m_btnUp.SetIcon(LoadIcon(hInst, MAKEINTRESOURCE(IDI_ICON_UP)));
			m_btnDown.SetIcon(LoadIcon(hInst, MAKEINTRESOURCE(IDI_ICON_DOWN)));
			
			CRect rectList;
			m_listAttributes.GetWindowRect(rectList);
			ScreenToClient(rectList);
			long nWidth1 = rectList.Width() / 5;
			long nWidth4 = nWidth1;
			long nWidth2 = 2 * nWidth1;
			long nWidth3 = rectList.Width() - nWidth1 - nWidth2 - nWidth4 - 5;
			
			// add column headers to the list ctrl
			m_listAttributes.SetExtendedStyle(LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT);
			m_listAttributes.InsertColumn(NAME_COLUMN, "Name", LVCFMT_LEFT, nWidth1, NAME_COLUMN);
			m_listAttributes.InsertColumn(VALUE_COLUMN, "Value", LVCFMT_LEFT, nWidth2, VALUE_COLUMN);
			m_listAttributes.InsertColumn(TYPE_COLUMN, "Type", LVCFMT_LEFT, nWidth3, TYPE_COLUMN);
			m_listAttributes.InsertColumn(SPATIALNESS_COLUMN, "Spatialness", LVCFMT_LEFT,
				nWidth4, SPATIALNESS_COLUMN);
			
			// Select the appropriate radio button
			CheckRadioButton( IDC_RADIO_REPLACE, IDC_RADIO_APPEND, 
				m_bReplaceValueText ? IDC_RADIO_REPLACE : IDC_RADIO_APPEND );

			// update Up and Down buttons
			updateButtons();
			
			// Create tooltip control
			m_ToolTipCtrl.Create(this, TTS_ALWAYSTIP);

			setModified(false);

			// get command line if any
			string	strCmdLine;
			strCmdLine = AfxGetApp()->m_lpCmdLine;
			if (!strCmdLine.empty())
			{
				// remove leading and trailing quotes
				strCmdLine = ::trim(strCmdLine, "\"", "\"");

				// make sure it is the file name and the file exists
				validateFileOrFolderExistence( strCmdLine );

				// Check file type
				string strExt = getExtensionFromFullPath( strCmdLine, true );
				if (strExt == ".eav")
				{
					// open the eav file
					openEAVFile(strCmdLine.c_str());
				}
				else if (strExt == ".voa" || strExt == ".evoa")
				{
					// Open the voa file
					openVOAFile( strCmdLine.c_str() );
				}
				else
				{
					// Throw exception
					UCLIDException ue( "ELI07885", "Unable to open file.");
					ue.addDebugInfo( "File To Open", strCmdLine );
					throw ue;
				}
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI06220");
	}
	catch(UCLIDException& ue)
	{
		// display the exception
		ue.display();

		// if essential controls have not been initialized then exit application
		if (!m_apToolBar.get())
		{
			CDialog::OnCancel();
		}
	}

	return TRUE;  // return TRUE  unless you set the focus to a control
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::OnClose() 
{	
	AFX_MANAGE_STATE(AfxGetModuleState());
	
	try
	{
		if (!promptForSaving())
		{
			return;
		}

		// clear the list control if there are items in the list
		// (this will call release for each of the Attribute pointers)
		if (m_listAttributes.GetItemCount() > 0)
		{
			clearListControl();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06219")

	CDialog::OnClose();
	// this function is usually called by clicking the X on the
	// top-right corner of the dialog to close the dialog.
	// Since we leave OnCancel() implementation empty, this is
	// the place to call CDialog::OnCancel() to close the window.
	CDialog::OnCancel();
}
//-------------------------------------------------------------------------------------------------
// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.
void CEAVGeneratorDlg::OnPaint() 
{
	if (IsIconic())
	{
		CPaintDC dc(this); // device context for painting

		SendMessage(WM_ICONERASEBKGND, (WPARAM) dc.GetSafeHdc(), 0);

		// Center icon in client rectangle
		int cxIcon = GetSystemMetrics(SM_CXICON);
		int cyIcon = GetSystemMetrics(SM_CYICON);
		CRect rect;
		GetClientRect(&rect);
		int x = (rect.Width() - cxIcon + 1) / 2;
		int y = (rect.Height() - cyIcon + 1) / 2;

		// Draw the icon
		dc.DrawIcon(x, y, m_hIcon);
	}
	else
	{
		CPaintDC dc( this ); // device context for painting
		
		// Get the toolbar height and the dialog width
		CRect rectDlg;
		GetWindowRect( &rectDlg );
		CRect rectToolBar;
		m_apToolBar->GetWindowRect( &rectToolBar );
		int iToolBarHeight = rectToolBar.Height();
		int iDialogWidth = rectDlg.Width();
		
		// With gray and white pens, draw horizontal lines that span the entire width
		// of the dialog, and that are just below the toolbar buttons
		CPen penGray;
		CPen penWhite;
		penGray.CreatePen(  PS_SOLID, 0, RGB( 128, 128, 128 ) );
		penWhite.CreatePen( PS_SOLID, 0, RGB( 255, 255, 255 ) );

		// First the gray line
		dc.SelectObject( &penGray );
		dc.MoveTo( 0, iToolBarHeight );
		dc.LineTo( iDialogWidth, iToolBarHeight );

		// Next the white line, one pixel below the gray
		dc.SelectObject( &penWhite );
		dc.MoveTo( 0, iToolBarHeight + 1 );
		dc.LineTo( iDialogWidth, iToolBarHeight + 1 );
	}
}
//-------------------------------------------------------------------------------------------------
// The system calls this to obtain the cursor to display while the user drags
//  the minimized window.
HCURSOR CEAVGeneratorDlg::OnQueryDragIcon()
{
	return (HCURSOR) m_hIcon;
}
//-------------------------------------------------------------------------------------------------
BOOL CEAVGeneratorDlg::PreTranslateMessage(MSG* pMsg) 
{
	// make sure the tool tip control is a valid window before passing messages to it
	if (asCppBool(::IsWindow(m_ToolTipCtrl.m_hWnd)))
	{
		// Show tooltips
		m_ToolTipCtrl.RelayEvent( pMsg );
	}

	return CDialog::PreTranslateMessage(pMsg);
}
//-------------------------------------------------------------------------------------------------
BOOL CEAVGeneratorDlg::OnToolTipNotify(UINT id, NMHDR * pNMHDR, LRESULT *pResult)
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	BOOL retCode = FALSE;
	
    TOOLTIPTEXT* pTTT = (TOOLTIPTEXT*)pNMHDR;
    UINT nID = pNMHDR->idFrom;
    if (pNMHDR->code == TTN_NEEDTEXT && (pTTT->uFlags & TTF_IDISHWND))
    {
        // idFrom is actually the HWND of the tool, ex. button control, edit control, etc.
        nID = ::GetDlgCtrlID((HWND)nID);
	}

	if (nID)
	{
		retCode = TRUE;
		pTTT->hinst = AfxGetResourceHandle();
		pTTT->lpszText = MAKEINTRESOURCE(nID);
	}

	return retCode;
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::OnDropFiles(HDROP hDropInfo)
{
	try
	{
		if (!promptForSaving())
		{
			return;
		}

		unsigned int iNumFiles = DragQueryFile( hDropInfo, 0xFFFFFFFF, NULL, NULL );
		if (iNumFiles > 1)
		{
			MessageBox( "Please only drag-and-drop one file into this window!" );
			return;
		}
		
		// Get the full path to the dragged filename
		char pszFile[MAX_PATH+1];
		DragQueryFile( hDropInfo, 0, pszFile, MAX_PATH );

		// Check file type
		string strExt = getExtensionFromFullPath( pszFile, true );

		// Load the dragged-and-dropped file
		if (strExt == ".eav")
		{
			openEAVFile( pszFile );
		}
		else if (strExt == ".voa" || strExt == ".evoa")
		{
			openVOAFile( pszFile );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07355")
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::OnBtnImagewindow() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		openSRWWithImage("");
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19317");
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::OnBtnAdd() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	
	try
	{
		// Create a new record
		int nCount = m_listAttributes.GetItemCount();
		int nNewItemIndex = m_listAttributes.InsertItem( nCount, "" );

		// create a new blank IAttribute and add it to the attribute vector
		IAttributePtr ipNewAttrib(CLSID_Attribute);
		ASSERT_RESOURCE_ALLOCATION("ELI18133", ipNewAttrib != NULL);

		// get the attribute from the smart pointer
		IAttribute* pipNewAttrib = ipNewAttrib.Detach();
		ASSERT_RESOURCE_ALLOCATION("ELI18271", pipNewAttrib != NULL);

		// set the item data to be the empty attribute
		m_listAttributes.SetItemData(nNewItemIndex, (DWORD_PTR)pipNewAttrib);

		// set the spatialness column to non-spatial
		m_listAttributes.SetItemText(nNewItemIndex, SPATIALNESS_COLUMN,
			getModeAsString(kNonSpatialMode).c_str());

		// Select the new record
		selectListItem(nNewItemIndex);
		setModified(true);

		// Set focus to the Name edit box
		m_editName.SetFocus();
		m_editName.SetSel( 0, -1 );

		// Update button states
		updateButtons();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06200")	
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::OnBtnCopy() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	
	try
	{
		// get the text and attribute for the current selected record
		int nSelectedItemIndex = getCurrentSelectedListItemIndex();
		CString zName = m_listAttributes.GetItemText(nSelectedItemIndex, NAME_COLUMN);
		CString zValue = m_listAttributes.GetItemText(nSelectedItemIndex, VALUE_COLUMN);
		CString zType = m_listAttributes.GetItemText(nSelectedItemIndex, TYPE_COLUMN);
		CString zMode = m_listAttributes.GetItemText(nSelectedItemIndex, SPATIALNESS_COLUMN);
		IAttributePtr ipOriginal(
			(IAttribute*) m_listAttributes.GetItemData(nSelectedItemIndex));
		ASSERT_RESOURCE_ALLOCATION("ELI18135", ipOriginal != NULL);

		// create a new IAttribute
		IAttributePtr ipAttribute(CLSID_Attribute);
		ASSERT_RESOURCE_ALLOCATION("ELI18134", ipAttribute != NULL);

		// copy the old attribute to the new attribute
		ICopyableObjectPtr ipCopy = ipAttribute;
		ASSERT_RESOURCE_ALLOCATION("ELI18203", ipCopy != NULL);
		ipCopy->CopyFrom(ipOriginal);

		IAttribute* pipAttribute = ipAttribute.Detach();
		ASSERT_RESOURCE_ALLOCATION("ELI18273", pipAttribute != NULL);

		// Create a new record and insert the values
		int nCount = m_listAttributes.GetItemCount();
		int nNewItemIndex = m_listAttributes.InsertItem( nCount, zName );
		m_listAttributes.SetItemText( nNewItemIndex, VALUE_COLUMN, zValue );
		m_listAttributes.SetItemText( nNewItemIndex, TYPE_COLUMN, zType );
		m_listAttributes.SetItemText( nNewItemIndex, SPATIALNESS_COLUMN, zMode );
		m_listAttributes.SetItemData(nNewItemIndex, (DWORD_PTR) pipAttribute);

		// Select the new record
		selectListItem(nNewItemIndex);

		// Set modified flag
		setModified(true);

		// Set focus to the Name edit box
		m_editName.SetFocus();
		m_editName.SetSel( 0, -1 );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07358")	
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::OnBtnDelete() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		int iSelectedCount = m_listAttributes.GetSelectedCount();
		if (iSelectedCount > 0)
		{
			char* pszWarning = iSelectedCount == 1 ? "Delete this item?" : "Delete these items?";
			if (::MessageBox(m_hWnd, pszWarning, "Delete", MB_YESNO) == IDNO)
			{
				return;
			}
			
			// Iterate over the selected items
			int nSelectedItemIndex = getCurrentSelectedListItemIndex();
			int nLastDeletedItem = nSelectedItemIndex;
			while (nSelectedItemIndex != -1)
			{
				// get the attribute pointer from the list
				IAttribute* pipAttribute = 
					(IAttribute*) m_listAttributes.GetItemData(nSelectedItemIndex);
				ASSERT_RESOURCE_ALLOCATION("ELI18204", pipAttribute != NULL);
				
				// release the attribute pointer
				pipAttribute->Release();

				// delete the item
				m_listAttributes.DeleteItem(nSelectedItemIndex);

				// Get the next selected item
				nLastDeletedItem = nSelectedItemIndex;
				nSelectedItemIndex = getCurrentSelectedListItemIndex();
			}

			// select the next item in the list if any
			int nCount = m_listAttributes.GetItemCount();
			if (nCount > 0)
			{
				int nSelectNextItemIndex = nLastDeletedItem;
				// if the deleted item was the last item the list
				if (nSelectedItemIndex == nCount)
				{
					// select the last item of the current list
					nSelectNextItemIndex = nCount - 1;
				}

				selectListItem(nSelectNextItemIndex);
			}

			setModified(true);
			
			// Update button states
			updateButtons();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06208")	
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::OnBtnSplit() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Get the current selected item
		int nSelectedItemIndex = getCurrentSelectedListItemIndex();

		if (nSelectedItemIndex >= 0)
		{
			// Create Object With Description pointer
			IObjectWithDescriptionPtr ipASWithDesc( CLSID_ObjectWithDescription );

			// Create the IObjectSelectorUI object
			IObjectSelectorUIPtr	ipObjSelect( CLSID_ObjectSelectorUI );
			ASSERT_RESOURCE_ALLOCATION("ELI10317", ipObjSelect != NULL );

			// initialize private license for the object
			IPrivateLicensedComponentPtr ipPLComponent = ipObjSelect;
			ASSERT_RESOURCE_ALLOCATION("ELI10318", ipPLComponent != NULL);
			_bstr_t _bstrKey = LICENSE_MGMT_PASSWORD.c_str();
			ipPLComponent->InitPrivateLicense(_bstrKey);

			// Prepare the prompts for object selector
			_bstr_t	bstrTitle("Attribute Splitter - description is ignored");
			_bstr_t	bstrDesc("Attribute Splitter description");
			_bstr_t	bstrSelect("Select Attribute Splitter");
			_bstr_t	bstrCategory(AFAPI_ATTRIBUTE_SPLITTERS_CATEGORYNAME.c_str());
			
			// Show the UI
			// NOTE: Splitter description will be ignored
			VARIANT_BOOL	vbResult = ipObjSelect->ShowUI2( bstrTitle, bstrDesc, 
				bstrSelect, bstrCategory, ipASWithDesc, VARIANT_TRUE, 
				gRequiredInterfaces.ulCount, gRequiredInterfaces.pIIDs );
			
			// If the user clicks the OK button
			if (vbResult == VARIANT_TRUE)
			{
				// attribute to split
				IAttributePtr ipAttribToSplit( 
					(IAttribute*) m_listAttributes.GetItemData(nSelectedItemIndex));
				ASSERT_RESOURCE_ALLOCATION("ELI07381",  ipAttribToSplit != NULL);

				// Get chosen Attribute Splitter and split the Attribute
				IAttributeSplitterPtr ipSplitter = ipASWithDesc->GetObject();
				ASSERT_RESOURCE_ALLOCATION("ELI18180", ipSplitter != NULL);

				ipSplitter->SplitAttribute(ipAttribToSplit, NULL, NULL);

				// get the level of the attribute that was split [P16 #2863]
				// (ie. the number of periods preceeding its name)
				string strName = (LPCTSTR) 
					m_listAttributes.GetItemText(nSelectedItemIndex, NAME_COLUMN);
				int iLevel = strName.find_first_not_of('.', 0);

				// Add new sub-attributes to list under selected Attribute
				long lCount = m_listAttributes.GetItemCount();
				addSubAttributes(ipAttribToSplit, lCount - nSelectedItemIndex - 1, iLevel + 1);

				setModified(true);
			}

			// Retain selection
			selectListItem(nSelectedItemIndex);
		}

		// Update button states
		updateButtons();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07374")	
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::OnBtnSavefile() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// before saving the file, let's validate it
		if (!validateAttributes())
		{
			return;
		}

		if (m_strCurrentFileName.empty())
		{
			OnBtnSaveas();
			return;
		}

		saveAttributes(m_strCurrentFileName.c_str());

		setModified(false);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06209")
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::OnBtnSaveas() 
{
	try
	{
		// before saving the file, let's validate it
		if (!validateAttributes())
		{
			return;
		}

		string strEAVFileName("");

		// Check for single Image Window
		VARIANT_BOOL bOneImageOpen = VARIANT_FALSE;
		CComBSTR bstrImageFile;
		ISpotRecognitionWindowPtr ipSRIR(NULL);
		IInputManagerPtr ipInputManager = getInputManager();
		getImageUtils()->IsExactlyOneImageOpen(ipInputManager, &bOneImageOpen, &bstrImageFile, &ipSRIR);
		if (bOneImageOpen == VARIANT_TRUE)
		{
			strEAVFileName = asString(bstrImageFile) + ".eav";
		}

		// if the eav file name is empty, get the current file name
		if (strEAVFileName.empty())
		{
			strEAVFileName = m_strCurrentFileName;
		}

		// Remove the file extension
		strEAVFileName = getFileNameWithoutExtension( strEAVFileName );
		
		// save file dialog
		CFileDialogEx saveFileDlg(FALSE, ".eav", strEAVFileName.c_str(), 
			OFN_FILEMUSTEXIST | OFN_OVERWRITEPROMPT | OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR,
			gstrSAVE_FILE_FILTER.c_str(), NULL);

		if (saveFileDlg.DoModal() == IDOK)
		{
			// Get the selected file complete path
			CString zFileName = saveFileDlg.GetPathName();

			// [DataEntry:326] Ensure the output filename matches the extension from the selected
			// filter to prevent confusion concerning why a file failed to save.
			CString zExt;
			if (AfxExtractSubString(zExt, saveFileDlg.m_ofn.lpstrFilter,
					2 * saveFileDlg.m_ofn.nFilterIndex - 1, (TCHAR)'\0'))
			{
				zExt.TrimLeft('*');
				if (zFileName.Right(zExt.GetLength()).CompareNoCase(zExt) != 0)
				{
					zFileName += zExt;
				}
			}

			// save all attributes accordingly
			saveAttributes(zFileName);

			m_strCurrentFileName = (LPCTSTR)zFileName;

			// Set window caption
			updateWindowCaption(zFileName);

			setModified(false);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07414");
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::OnBtnOpen() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		if (!promptForSaving())
		{
			return;
		}

		// show open file dialog
		CFileDialogEx openFileDlg( TRUE, NULL, NULL, 
			OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR,
			gstrOPEN_FILE_FILTER.c_str(), NULL);
		if (openFileDlg.DoModal() == IDOK)
		{
			CWaitCursor waitCursor;

			// Get the selected file complete path
			CString zFileName = openFileDlg.GetPathName();

			string strExt = getExtensionFromFullPath( zFileName.operator LPCTSTR(), true );
			if (strExt == ".eav")
			{
				// list all attributes from eav file to the list box
				openEAVFile(zFileName);
			}
			else if (strExt == ".voa" || strExt == ".evoa")
			{
				openVOAFile( zFileName );
			}
			else
			{
				// Throw exception
				UCLIDException ue("ELI24186", "Unable to open file.");
				ue.addDebugInfo( "File To Open", (LPCTSTR)zFileName );
				throw ue;
			}

		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06227")	
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::OnBtnNew() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	INIT_EXCEPTION_AND_TRACING("MLI00015");

	try
	{
		if (!promptForSaving())
		{
			return;
		}
		_lastCodePos = "10";

		// clear the list control
		clearListControl();
		_lastCodePos = "20";

		m_zName.Empty();
		m_zValue.Empty();
		m_zType.Empty();
		_lastCodePos = "40";

		// clear current file name
		m_strCurrentFileName = "";
		_lastCodePos = "50";

		updateButtons();
		_lastCodePos = "60";

		updateWindowCaption("");
		_lastCodePos = "70";

		UpdateData(FALSE);
		_lastCodePos = "80";

		setModified(false);
		_lastCodePos = "90";
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06229")	
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::OnBtnDown() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		int nSelectedItemIndex = getCurrentSelectedListItemIndex();

		if (nSelectedItemIndex >= 0)
		{
			// get the index of the item right below currently selected item
			int nBelowIndex = nSelectedItemIndex+1;

			// get selected item text from list
			CString zName = m_listAttributes.GetItemText(nSelectedItemIndex, NAME_COLUMN);
			CString zValue = m_listAttributes.GetItemText(nSelectedItemIndex, VALUE_COLUMN);
			CString zType = m_listAttributes.GetItemText(nSelectedItemIndex, TYPE_COLUMN);
			CString zMode = m_listAttributes.GetItemText(nSelectedItemIndex, SPATIALNESS_COLUMN);
			DWORD_PTR dwpAttributePtr = m_listAttributes.GetItemData(nSelectedItemIndex);

			// then remove the selected item
			m_listAttributes.DeleteItem(nSelectedItemIndex);

			// now insert the item right before the item that was above
			int nActualIndex = m_listAttributes.InsertItem(nBelowIndex, zName);
			m_listAttributes.SetItemText(nActualIndex, VALUE_COLUMN, zValue);
			m_listAttributes.SetItemText(nActualIndex, TYPE_COLUMN, zType);
			m_listAttributes.SetItemText(nActualIndex, SPATIALNESS_COLUMN, zMode);
			m_listAttributes.SetItemData(nActualIndex, dwpAttributePtr);
			
			// keep this item selected
			selectListItem(nActualIndex);

			setModified(true);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06210")	
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::OnBtnUp() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		int nSelectedItemIndex = getCurrentSelectedListItemIndex();

		if (nSelectedItemIndex >= 0)
		{
			// get the index of the item right above currently selected item
			int nAboveIndex = nSelectedItemIndex-1;
			if (nAboveIndex < 0)
			{
				return;
			}

			// get selected item text from list
			CString zName = m_listAttributes.GetItemText(nSelectedItemIndex, NAME_COLUMN);
			CString zValue = m_listAttributes.GetItemText(nSelectedItemIndex, VALUE_COLUMN);
			CString zType = m_listAttributes.GetItemText(nSelectedItemIndex, TYPE_COLUMN);
			CString zMode = m_listAttributes.GetItemText(nSelectedItemIndex, SPATIALNESS_COLUMN);
			DWORD_PTR dwpAttributePtr = m_listAttributes.GetItemData(nSelectedItemIndex);

			// then remove the selected item
			m_listAttributes.DeleteItem(nSelectedItemIndex);

			// now insert the item right before the item that was above
			int nActualIndex = m_listAttributes.InsertItem(nAboveIndex, zName);
			m_listAttributes.SetItemText(nActualIndex, VALUE_COLUMN, zValue);
			m_listAttributes.SetItemText(nActualIndex, TYPE_COLUMN, zType);
			m_listAttributes.SetItemText(nActualIndex, SPATIALNESS_COLUMN, zMode);
			m_listAttributes.SetItemData(nActualIndex, dwpAttributePtr);
			
			// keep this item selected
			selectListItem(nActualIndex);

			setModified(true);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06211")	
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::OnBtnReplace() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	
	try
	{
		// Set the flag
		m_bReplaceValueText = true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06246")
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::OnBtnAppend() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	
	try
	{
		// Clear the flag
		m_bReplaceValueText = false;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06247")
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::OnBtnHighlight()
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	
	try
	{
		// add the new highlight (if the SRW is not open, open it).
		highlightAttributeInRow(gbOPEN_SRW);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18078");
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::OnBtnMerge()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Get the position of the first attribute to merge
		POSITION pos = m_listAttributes.GetFirstSelectedItemPosition();
		if (pos == NULL)
		{
			// No items are selected, we are done.
			return;
		}

		// Get the index of the first attribute to merge
		int i = m_listAttributes.GetNextSelectedItem(pos);
		if (pos == NULL)
		{
			// Only one item was selected, we are done.
			return;
		}

		// Get the first attribute to merge
		IAttributePtr ipMainAttribute = (IAttribute*) m_listAttributes.GetItemData(i);
		ASSERT_RESOURCE_ALLOCATION("ELI25054", ipMainAttribute != NULL);

		// Get the level of this attribute
		unsigned int uiLevel = getAttributeLevel(i);

		// Get the main attributes' spatial string
		ISpatialStringPtr ipMainString = ipMainAttribute->Value;
		ASSERT_RESOURCE_ALLOCATION("ELI25055", ipMainString != NULL);

		// Get the computed source image name
		string strMainSourceDoc = getSourceImageName(ipMainString);
		makeLowerCase(strMainSourceDoc);

		// Create sets to hold the names and types of the attributes
		set<string> setNames;
		set<string> setTypes;
		addToSetIfNonEmpty(setNames, ipMainAttribute->Name);
		addToSetIfNonEmpty(setTypes, ipMainAttribute->Type);

		// Find the last subattribute of the first attribute being merged to be used
		// as the insert after position for moving subattributes
		int nLastSubAttribute = i;
		int nListCount = m_listAttributes.GetItemCount();
		for ( int x = nLastSubAttribute + 1; x <nListCount; x++)
		{
			if ( getAttributeLevel(x) == uiLevel)
			{
				break;
			}
			nLastSubAttribute++;
		}

		// Iterate over the remaining attributes
		while (true)
		{
			// Get the next selected item
			i = m_listAttributes.GetNextSelectedItem(pos);
			IAttributePtr ipNextAttribute = (IAttribute*) m_listAttributes.GetItemData(i);
			ASSERT_RESOURCE_ALLOCATION("ELI25057", ipNextAttribute != NULL);

			ISpatialStringPtr ipNextSS = ipNextAttribute->Value;
			ASSERT_RESOURCE_ALLOCATION("ELI25595", ipNextSS != NULL);

			// Get the computed source image name and ensure that both spatial strings
			// point to the same "image"
			string strNextSourceDoc = getSourceImageName(ipNextSS);
			makeLowerCase(strNextSourceDoc);
			if (strNextSourceDoc != strMainSourceDoc)
			{
				UCLIDException ue("ELI25596", "Cannot merge attributes from different documents!");
				ue.addDebugInfo("Main Source Document", asString(ipMainString->SourceDocName));
				ue.addDebugInfo("Next Source Document", asString(ipNextSS->SourceDocName));
				ue.addDebugInfo("Current Source Image", strMainSourceDoc);
				ue.addDebugInfo("Current Next Image", strNextSourceDoc);
				throw ue;
			}

			// Since both strings point to the same computed source image, ensure
			// that the source doc names are the same so that the strings can
			// be merged together [FlexIDSCore #3497]
			ipNextSS->SourceDocName = ipMainString->SourceDocName;

			// Store its name and type
			addToSetIfNonEmpty(setNames, ipNextAttribute->Name);
			addToSetIfNonEmpty(setTypes, ipNextAttribute->Type);

			// Append this string to the main one
			ipMainString->AppendString("\r\n");

			// [LegacyRCAndUtils:5361]
			// Use MergeAsHybridString which will ensure the spatial page infos are compatible
			// before combining the strings.
			ipMainString->MergeAsHybridString(ipNextSS);

			// Remove this attribute from the list control
			ipNextAttribute->Release();
			m_listAttributes.DeleteItem(i);

			// Move the subattributes to the item the attribute was merged with
			// Subattributes of the just deleted item i will now be at position i
			moveSubAttributes (nLastSubAttribute, i, uiLevel+1);
			
			// Iterate until position is NULL
			if (pos == NULL)
			{
				break;
			}
			else
			{
				// Decrement position since an attribute was deleted
				pos--;
			}
		}

		// Get the new attribute name, type, and value
		string strName = joinStrings(setNames, '_');
		string strType = joinStrings(setTypes, '+');
		string strValue = asString(ipMainString->String);

		// Update the main attribute
		ipMainAttribute->Name = strName.c_str();
		ipMainAttribute->Type = strType.c_str();

		// Prepare the attribute properties for display
		strName = string(uiLevel, '.') + strName;

		// Update the list control
		updateList(NAME_COLUMN, strName.c_str());
		updateList(TYPE_COLUMN, strType.c_str());
		updateList(VALUE_COLUMN, strValue.c_str());

		// Update the edit boxes
		m_editName.SetWindowText(strName.c_str());
		m_editType.SetWindowText(strType.c_str());
		m_editValue.SetWindowText(strValue.c_str());
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI25052")
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::OnChangeEditName() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		UpdateData();

		updateList(NAME_COLUMN, m_zName);

		setModified(true);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06215")
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::OnChangeEditType() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		UpdateData();

		updateList(TYPE_COLUMN, m_zType);

		setModified(true);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06216")
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::OnChangeEditValue() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		UpdateData();

		updateList(VALUE_COLUMN, m_zValue);

		setModified(true);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06217")
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::OnItemchangedListDisplay(NMHDR* pNMHDR, LRESULT* pResult) 
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		NM_LISTVIEW* pNMListView = (NM_LISTVIEW*)pNMHDR;

		// check to be sure the message came from the display list
		if (pNMHDR->idFrom == IDC_LIST_DISPLAY)
		{
			// check if this message is for a selection change
			if ((pNMListView->uChanged & LVIF_STATE) && (pNMListView->uNewState & LVIS_SELECTED))
			{
				// selection changed - just highlight the selected attribute, 
				// do not try to open the SRW if it is not open.
				highlightAttributeInRow(gbDO_NOT_OPEN_SRW);

			}

			// enable or disable the input manager on selection change
			enableOrDisableInputManager();

			// update the buttons based on the selection change
			updateButtons();
		}

		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06214")		
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::OnKeydownListDisplay(NMHDR* pNMHDR, LRESULT* pResult) 
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		NMLVKEYDOWN* nmkd = (NMLVKEYDOWN*)pNMHDR;

		// check what key was pressed (we only care about delete, up arrow, and down arrow
		switch (nmkd->wVKey)
		{
		case VK_DELETE:
			OnBtnDelete();
			break;

		case VK_UP:
			{
				// up arrow - get the current selection
				int nItemSelected = getCurrentSelectedListItemIndex();

				// selection will be < 0 if nothing is selected, in which case do nothing
				if (!(nItemSelected < 0))
				{
					// check for top of the list
					if (nItemSelected > 0)
					{
						// not at the top of the list so move selection up one
						nItemSelected--;
					}

					// set the new selection
					selectListItem(nItemSelected);
				}
			}
			break;

		case VK_DOWN:
			{
				// down arrow - get the current selection
				int nItemSelected = getCurrentSelectedListItemIndex();
				
				// selection will be < 0 if nothing is selected, in which case do nothing
				if (!(nItemSelected < 0))
				{
					// check for bottom of the list
					if (nItemSelected < (m_listAttributes.GetItemCount() - 1))
					{
						// not at the bottom of the list so move selection down one
						nItemSelected++;
					}

					// set the new selection
					selectListItem(nItemSelected);
				}
			}
			break;
		}
			
		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07382")		
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::OnCancel()
{
	// purpose of having this function here is to prevent
	// user from closing the dialog by pressing Escape key
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::OnOK()
{
	// purpose of having this function here is to prevent
	// user from closing the dialog by pressing Enter key
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::OnNMClickListDisplay(NMHDR *pNMHDR, LRESULT *pResult)
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// just highlight the selected attribute, do not try to open the
		// SRW if it is not open.
		highlightAttributeInRow(gbDO_NOT_OPEN_SRW);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18079");
	*pResult = 0;
}
//-------------------------------------------------------------------------------------------------
void CEAVGeneratorDlg::OnNMDblclkListDisplay(NMHDR *pNMHDR, LRESULT *pResult)
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// highlight the selected attribute (if the SRW is not open, then
		// attempt to open it).
		highlightAttributeInRow(gbOPEN_SRW);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18080");
	*pResult = 0;
}
//-------------------------------------------------------------------------------------------------
