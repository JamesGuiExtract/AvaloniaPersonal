//=================================================================================================
// COPYRIGHT UCLID SOFTWARE, LLC. 1999
//
// FILE:	UCLIDExceptionDetailsDlg.cpp
//
// PURPOSE: The purpose of this file is to implement the functionality of UCLIDExceptionDetailsDlg class 
//          It shows up a dialog with two list controls, in which ELI, its related text and Debug 
//          information is shown to the user. 
//
// NOTES:	
//			
// WARNING: 		
//			
//
// AUTHOR:	M.Srinivasa Rao (Infotech - 21st Aug to Nov 2000)
//			John Hurd
//
//=================================================================================================

#include "stdafx.h"
#include "resource.h"
#include "UCLIDExceptionDetailsDlg.h"
#include "UCLIDException.h"
#include "cpputil.h"
#include "TemporaryResourceOverride.h"
#include "NamedValueTypePair.h"
#include "LoadFileDlgThread.h"
#include "LicenseUtils.h"
#include "EncryptionEngine.h"
#include "ByteStream.h"
#include "ByteStreamManipulator.h"
#include "ClipboardManager.h"

#include <string>
using namespace std;

extern HINSTANCE gModuleResource;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
// Constants for Exception history columns
const int giELI_CODE_COLUMN = 0;
const int giMESSAGE_COLUMN = 1;
const int giNUMBER_DEBUG_COLUMN = 2;
const int giSTACK_TRACE_COLUMN =3;

// Constants Debug information columns
const int giDEBUG_NAME_COLUMN = 0;
const int giDEBUG_TYPE_COLUMN = 1;
const int giDEBUG_VALUE_COLUMN = 2;

// Constants for Stack trace column
const int giSTACK_TRACE_LINE_COLUMN = 0;

// Constants for values of exception debug detail group
const int giSELECTED_EXCEPTION = 0;
const int giALL_EXCEPTIONS = 1;

//--------------------------------------------------------------------------------------------------
// UCLIDExceptionDetailsDlg
//--------------------------------------------------------------------------------------------------
UCLIDExceptionDetailsDlg::UCLIDExceptionDetailsDlg(
	const UCLIDException& exception,
	CWnd* pParent /*=NULL*/)
	: CDialog(IDD_DIALOG_DEBUG_INFO, pParent),
	m_iDebugData(0)
{
	//get the reference of uclid exception to load its details 
	//into list controls upon dialog initialisation
	m_pUclidExceptonToLoad = &exception;
}
//--------------------------------------------------------------------------------------------------
UCLIDExceptionDetailsDlg::~UCLIDExceptionDetailsDlg(void)
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16574");
}
//--------------------------------------------------------------------------------------------------
void UCLIDExceptionDetailsDlg::DoDataExchange(CDataExchange* pDX)
{
	try
	{
		CDialog::DoDataExchange(pDX);
		DDX_Control(pDX, IDC_LIST_ELI_AND_TEXT, m_ELIandTextListCtrl);
		DDX_Control(pDX, IDC_LIST_DEBUG_PARAMETERS, m_debugParamsListCtrl);
		DDX_Control(pDX, IDC_LIST_STACK_TRACE, m_StackTraceListCtrl);
		DDX_Control(pDX, IDC_STATIC_STACK_TRACE, m_StackTraceStatic);
		DDX_Control(pDX, IDC_BUTTON_SAVEAS, m_SaveAsButton);
		DDX_Control(pDX, IDC_BUTTON_COPY, m_CopyButton);
		DDX_Control(pDX, IDOK, m_CloseButton);
		DDX_Radio(pDX, IDC_RADIO_SELECTED_EXCEPTION, m_iDebugData);
		DDX_Control(pDX, IDC_RADIO_ALL_EXCEPTIONS, m_AllExceptionsRadioButton);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20282");
}
//--------------------------------------------------------------------------------------------------

BEGIN_MESSAGE_MAP(UCLIDExceptionDetailsDlg, CDialog)
	ON_BN_CLICKED(IDC_BUTTON_SAVEAS, OnButtonSaveAs)
	ON_BN_CLICKED(IDC_BUTTON_COPY, OnButtonCopy)
	ON_NOTIFY(NM_RCLICK, IDC_LIST_DEBUG_PARAMETERS, OnRclickDebugParameters)
	ON_NOTIFY(LVN_ITEMCHANGED, IDC_LIST_ELI_AND_TEXT, OnItemChangedListEliAndText)
	ON_BN_CLICKED(IDC_RADIO_SELECTED_EXCEPTION, OnDebugInformationClick)
	ON_BN_CLICKED(IDC_RADIO_ALL_EXCEPTIONS, OnDebugInformationClick)
	ON_COMMAND(ID_CONTEXT_COPYNAME, OnContextCopyName)
	ON_COMMAND(ID_CONTEXT_COPYVALUE, OnContextCopyValue)
END_MESSAGE_MAP()

//--------------------------------------------------------------------------------------------------
// UCLIDExceptionDetailsDlg message handlers
//--------------------------------------------------------------------------------------------------
BOOL UCLIDExceptionDetailsDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		CDialog::OnInitDialog();

		//to get the size of the list controls
		CRect rect;

		//get the width of the ELI and Text List control
		m_ELIandTextListCtrl.GetClientRect(&rect);

		//enable  full row selection and grid lines
		m_ELIandTextListCtrl.SetExtendedStyle(LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT );

		bool bInternalToolsLicensed = isInternalToolsLicensed();
		
		int iMinColWidth = rect.Width() / 10;

		//now insert columns in the ELI and Text list control
		m_ELIandTextListCtrl.InsertColumn(giELI_CODE_COLUMN,"ELI Code",LVCFMT_LEFT,iMinColWidth,0);
		m_ELIandTextListCtrl.InsertColumn(giMESSAGE_COLUMN,"Message",LVCFMT_LEFT,
			iMinColWidth*((bInternalToolsLicensed) ? 7 : 8),1);
		m_ELIandTextListCtrl.InsertColumn(giNUMBER_DEBUG_COLUMN,"# Debug", LVCFMT_LEFT,
			iMinColWidth,2);

		//get the width of the Debug Parameters List control
		m_debugParamsListCtrl.GetClientRect(&rect);

		//enable  full row selection and grid lines
		m_debugParamsListCtrl.SetExtendedStyle(LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT );

		//now insert columns in the Debug Parameters list control
		m_debugParamsListCtrl.InsertColumn(giDEBUG_NAME_COLUMN,"Name",LVCFMT_LEFT,rect.Width()/5,0);
		m_debugParamsListCtrl.InsertColumn(giDEBUG_TYPE_COLUMN,"Type",LVCFMT_LEFT,rect.Width()/8,1);
		m_debugParamsListCtrl.InsertColumn(giDEBUG_VALUE_COLUMN,"Value",LVCFMT_LEFT,rect.Width()*27/40,2);

		// If this is not running internal to Extract Systems the Stack trace control is not visible
		if (bInternalToolsLicensed)
		{
			// Add Column to indicate if a stack trace is available for the top list box.
			m_ELIandTextListCtrl.InsertColumn(giSTACK_TRACE_COLUMN,"Stack trace", LVCFMT_LEFT,iMinColWidth,3);

			// Get the rect for the Stack trace control
			m_StackTraceListCtrl.GetClientRect(&rect);
			
			// Enable full row selection and grid lines
			m_StackTraceListCtrl.SetExtendedStyle(LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT );

			// Insert a single column that is as wide as the control
			m_StackTraceListCtrl.InsertColumn(giSTACK_TRACE_LINE_COLUMN, NULL, LVCFMT_LEFT, rect.Width());
		}
		else
		{
			// Hide the stack trace list control and resize the dialog.
			resizeForNonInternalUse();
		}

		//read information from UCLIDException and load into list controls
		loadDebugInfoInListCtrls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17294");

	return TRUE;  
}
//--------------------------------------------------------------------------------------------------
void UCLIDExceptionDetailsDlg::OnButtonSaveAs() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		//open File dialog and get the file name selected by user
		//default extension is .uex ie uclid exception file
		CString zFileName;
		CFileDialog fileSaveAsDlg(FALSE,"uex" , zFileName ,(OFN_OVERWRITEPROMPT | OFN_EXTENSIONDIFFERENT  |  
			OFN_LONGNAMES | OFN_NOREADONLYRETURN) ,"UCLID Exception files(*.uex)|*.uex||");
		fileSaveAsDlg.m_ofn.lpstrTitle = "Save exception file as";
		
		// Pass the pointer of dialog to create ThreadDataStruct object
		ThreadFileDlg tfd(&fileSaveAsDlg);

		//if user clicks to save it get the path name
		if (tfd.doModal() == IDOK)
		{
			zFileName = fileSaveAsDlg.GetPathName();
			
			//use getVersion function and write it into the exception file
			string strFileName(zFileName.operator LPCTSTR());
			
			//call saveTo method of UCLIDException to save the file
			m_pUclidExceptonToLoad->saveTo(strFileName);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17293");
}
//--------------------------------------------------------------------------------------------------
int UCLIDExceptionDetailsDlg::DoModal() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);
	
	try
	{
		return CDialog::DoModal();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17292")

	// Indicate that an error occurred
	return -1;
}
//--------------------------------------------------------------------------------------------------
void UCLIDExceptionDetailsDlg::OnButtonCopy() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		// Create the stringized data
		string strData = m_pUclidExceptonToLoad->asStringizedByteStream();

		ClipboardManager clipboardMgr(this);
		clipboardMgr.writeText(strData);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17291")
}
//--------------------------------------------------------------------------------------------------
void UCLIDExceptionDetailsDlg::OnRclickDebugParameters(NMHDR* pNMHDR, LRESULT* pResult)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{	
		// Only need to display the context menu if one item is selected
		if (getSelectedDebugParamsIndex() >= 0)
		{
			// Load the context menu
			CMenu menu;
			menu.LoadMenu(IDR_BASE_MNU_CONTEXT);
			CMenu *pContextMenu = menu.GetSubMenu(0);

			// Map the point to the correct position
			CPoint	point;
			GetCursorPos( &point );

			// Display and manage the context menu
			pContextMenu->TrackPopupMenu(TPM_LEFTALIGN|TPM_LEFTBUTTON|TPM_RIGHTBUTTON, 
				point.x, point.y, this);
		}

		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17287")
}
//-------------------------------------------------------------------------------------------------
void UCLIDExceptionDetailsDlg::OnItemChangedListEliAndText(NMHDR *pNMHDR, LRESULT *pResult)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		LPNMLISTVIEW lpStateChange = (LPNMLISTVIEW ) pNMHDR;

		// If this is not for the newly selected item there is nothing to do so return.
		if ( (lpStateChange->uNewState & LVIS_SELECTED) == 0 )
		{
			return;
		}

		// Get the number exception items
		int nExceptionCount = m_ELIandTextListCtrl.GetItemCount();

		// If there are exceptions load the data for the execeptions
		if ( nExceptionCount > 0 )
		{
			// Get the item that chenged.
			int nNewItem = lpStateChange->iItem;

			// Make sure the item is in the valid range.
			if (nNewItem < 0 || nNewItem >= nExceptionCount)
			{
				// Set to first item in list if not in valid range.
				nNewItem = 0;
			}

			// Clear the data in the debug params list control.
			m_debugParamsListCtrl.DeleteAllItems();

			// Check if displaying data for currently selected exception or all exceptions.
			if (m_iDebugData == giSELECTED_EXCEPTION)
			{
				// Load only data for current selection	.	
				loadExceptionData(*m_mapItemToException[nNewItem]);
			}
			else if (m_iDebugData == giALL_EXCEPTIONS)
			{
				// Load debug data for all of the exceptions.
				for ( unsigned int i = 0;  i < m_mapItemToException.size(); i++)
				{
					// Load exception data for the given exception. 
					loadExceptionData(*m_mapItemToException[i]);
				}
			}
			else
			{
				// Should not get here but log an exception to track a problem.
				UCLIDException ue("ELI21862", "Invalid group control value");
				ue.addDebugInfo("GroupValue", m_iDebugData);
				ue.log();
			}

			// Load the stack trace data
			loadStackTraceData(*m_mapItemToException[nNewItem]);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI21964")
}
//-------------------------------------------------------------------------------------------------
void UCLIDExceptionDetailsDlg::OnDebugInformationClick()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		// Save the old setting of the debug data to display.
		int iSaveDebugSetting = m_iDebugData;

		// Get new value of controls.
		UpdateData();

		// if the debug data group setting has not changed return.
		if ( iSaveDebugSetting == m_iDebugData)
		{
			return;
		}

		// Clear the data in the debug params list control.
		m_debugParamsListCtrl.DeleteAllItems();
		if ( m_ELIandTextListCtrl.GetItemCount() > 0)
		{
			if (m_iDebugData == giSELECTED_EXCEPTION)
			{
				int nCurrSelect = m_ELIandTextListCtrl.GetSelectionMark();
				loadExceptionData(*m_mapItemToException[nCurrSelect]);
			}
			else if (m_iDebugData == giALL_EXCEPTIONS)
			{
				// Add exception data for all of the exceptions.
				for ( unsigned int i = 0;  i < m_mapItemToException.size(); i++)
				{
					// Load exception data for the given exception. 
					loadExceptionData(*m_mapItemToException[i]);
				}
			}
			else
			{
				// Should not get here but log an exception to track a problem.
				UCLIDException ue("ELI21874", "Invalid group control value");
				ue.addDebugInfo("GroupValue", m_iDebugData);
				ue.log();
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI21965")
}
//-------------------------------------------------------------------------------------------------
void UCLIDExceptionDetailsDlg::OnContextCopyName()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		copyDebugParamsColumnToClipboard(0);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI28714")
}
//-------------------------------------------------------------------------------------------------
void UCLIDExceptionDetailsDlg::OnContextCopyValue()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		copyDebugParamsColumnToClipboard(2);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI28713")
}

//-------------------------------------------------------------------------------------------------
// Private methods
//--------------------------------------------------------------------------------------------------
void UCLIDExceptionDetailsDlg::loadDebugInfoInListCtrls()
{
	INIT_EXCEPTION_AND_TRACING("MLI00038");

	try
	{
		// Initialize the current exception
		const UCLIDException *puexCurr = m_pUclidExceptonToLoad;
		_lastCodePos = "10";

		//index of the record in the array
		int iIndex = 0;
		int iDebugIndex = 0;		//reset the index for getting debug information 

		_lastCodePos = "20";
		while (puexCurr != __nullptr)
		{
			// Add the exception pointer to the Exception map
			m_mapItemToException[iIndex] = puexCurr;
			_lastCodePos = "30-" + asString(iIndex);

			//get the ELI
			string strELI = puexCurr->getTopELI();

			//get the ELI Text
			string strText = puexCurr->getTopText();
			_lastCodePos = "40";

			//insert ELI and Text into the list control
			// Set ELICode column text
			m_ELIandTextListCtrl.InsertItem(iIndex,strELI.c_str());
			_lastCodePos = "60";

			// Set Message text
			m_ELIandTextListCtrl.SetItemText(iIndex,giMESSAGE_COLUMN,strText.c_str());
			_lastCodePos = "70";

			// Get the number of debug items.
			int nNumberDebugItems = puexCurr->getDebugVector().size();
			_lastCodePos = "71";

			// Set the # of debug items column
			m_ELIandTextListCtrl.SetItemText(iIndex,giNUMBER_DEBUG_COLUMN,
				asString(nNumberDebugItems).c_str());

			_lastCodePos = "72";
			// Set value for the stack trace column if internal tools are licensed
			if ( isInternalToolsLicensed())
			{
				_lastCodePos = "73";
				if ( puexCurr->getStackTrace().size()>0 )
				{
					m_ELIandTextListCtrl.SetItemText(iIndex,giSTACK_TRACE_COLUMN, "Yes");
				}
				else
				{
					m_ELIandTextListCtrl.SetItemText(iIndex,giSTACK_TRACE_COLUMN, "No");
				}
			}

			iIndex++;

			_lastCodePos = "80";

			// Get the next inner exception.
			puexCurr = puexCurr->getInnerException();
		}
		
		m_AllExceptionsRadioButton.EnableWindow(asMFCBool(iIndex > 1));
		if ( m_ELIandTextListCtrl.GetItemCount() > 0)
		{
			// Make the top exception the selected
			m_ELIandTextListCtrl.SetItemState(0, LVIS_SELECTED | LVIS_FOCUSED, LVIS_SELECTED | LVIS_FOCUSED);
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20283");
}
//--------------------------------------------------------------------------------------------------
void UCLIDExceptionDetailsDlg::loadExceptionData(const UCLIDException &ex)
{
	// Initialize debug index to 0;
	int iDebugIndex = 0;
	string strName;
	string strType;
	string strValue;

	// Get the exceptions debug vector
	vector<NamedValueTypePair> vecDebug = ex.getDebugVector();

	// Iterate through all values in the vector
	vector<NamedValueTypePair>::iterator iterDebug;
	for (iterDebug = vecDebug.begin(); iterDebug != vecDebug.end(); iterDebug++)
	{
		strName = (*iterDebug).GetName();
		ValueTypePair valTypePair = (*iterDebug).GetPair();
		switch(valTypePair.getType())
		{
		case ValueTypePair::kBoolean:
			strType = "boolean";
			if (valTypePair.getBooleanValue())
			{
				strValue = "TRUE";
			}
			else
			{
				strValue = "FALSE";
			}
			break;
		case ValueTypePair::kString:
			strType = "string";
			strValue = valTypePair.getStringValue();
			break;
		case ValueTypePair::kDouble:
			strType = "double";
			strValue = asString(valTypePair.getDoubleValue());
			break;
		case ValueTypePair::kInt:
			strType = "integer";
			strValue = asString(valTypePair.getIntValue());
			break;
		case ValueTypePair::kInt64:
			strType = "int64";
			strValue = asString(valTypePair.getInt64Value());
			break;
		case ValueTypePair::kLong:
			strType = "long";
			strValue = asString(valTypePair.getLongValue());
			break;
		case ValueTypePair::kUnsignedLong:
			strType = "unsigned long";
			strValue = asString(valTypePair.getUnsignedLongValue());
			break;
		case ValueTypePair::kOctets:
			ASSERT(false);	//this functionality is not avilable
			break;
		case ValueTypePair::kGuid:
			strType = "GUID";
			strValue = asString(valTypePair.getGuidValue());
			break;
		default:
			strType = "Unknown";
			strValue = "Unknown";
			break;
		}

		// Check Value for Encryption
		strValue = UCLIDException::sGetDataValue(strValue);

		// now you have got Name, Type and Value
		//load them into the debug parameters list control
		m_debugParamsListCtrl.InsertItem (iDebugIndex,strName.c_str());
		m_debugParamsListCtrl.SetItemText(iDebugIndex,giDEBUG_TYPE_COLUMN,strType.c_str());
		m_debugParamsListCtrl.SetItemText(iDebugIndex,giDEBUG_VALUE_COLUMN, strValue.c_str());
		iDebugIndex++;
	}

	// Only resize the value column if there was debug data placed in the control.
	if (iDebugIndex != 0)
	{
		m_debugParamsListCtrl.SetColumnWidth(giDEBUG_VALUE_COLUMN, LVSCW_AUTOSIZE);
	}
}
//-------------------------------------------------------------------------------------------------
void UCLIDExceptionDetailsDlg::loadStackTraceData(const UCLIDException &ex)
{
	// If internal tools are not licensed don't load the list box
	if (!isInternalToolsLicensed())
	{
		return;
	}

	// Clear the stack trace list.
	m_StackTraceListCtrl.DeleteAllItems();
	
	// Add Stack trace to the debug info
	string strStackTrace = "";
	const vector<string> &vecStackTrace = ex.getStackTrace();
	for each ( string s in vecStackTrace )
	{
		// Insert the stack trace line into the list.
		m_StackTraceListCtrl.InsertItem(m_StackTraceListCtrl.GetItemCount(),
			UCLIDException::sGetDataValue(s).c_str());
	}

	// Set the column width to the size of the widest item.
	m_StackTraceListCtrl.SetColumnWidth(giSTACK_TRACE_LINE_COLUMN, LVSCW_AUTOSIZE);
}
//-------------------------------------------------------------------------------------------------
void UCLIDExceptionDetailsDlg::resizeForNonInternalUse()
{
	// Hide the stack trace lable and list control
	m_StackTraceListCtrl.ShowWindow(SW_HIDE);
	m_StackTraceStatic.ShowWindow(SW_HIDE);

	// Get the client window for the debug params list - to use for repositioning the buttons
	CRect rect;
	m_debugParamsListCtrl.GetWindowRect(rect);

	// Get the save button rect
	CRect rectButton;
	m_SaveAsButton.GetWindowRect(rectButton);

	// Get the Height of the button
	long nHeight = rectButton.Height();

	// Set top of the buttons to bottom of debug params list + 5
	long nTop = rect.bottom + 5;

	// Set bottom of the buttons to top + height
	long nBottom = nTop + nHeight;

	// Save the original bottom of button for calculated the new dialog window size
	long nOriginalBottomOfButton = rectButton.bottom;

	// Set the position of the save button
	rectButton.top = nTop;
	rectButton.bottom = nBottom;
	ScreenToClient(rectButton);
	m_SaveAsButton.MoveWindow(rectButton);

	// Set the position of the Copy button
	m_CopyButton.GetWindowRect(rectButton);
	rectButton.top = nTop;
	rectButton.bottom = nBottom;
	ScreenToClient(rectButton);
	m_CopyButton.MoveWindow(rectButton);

	// Set the position of the Close button
	m_CloseButton.GetWindowRect(rectButton);
	rectButton.top = nTop;
	rectButton.bottom = nBottom;
	ScreenToClient(rectButton);
	m_CloseButton.MoveWindow(rectButton);

	// Resize the dialog window
	GetWindowRect(rect);
	rect.bottom = nBottom + rect.bottom - nOriginalBottomOfButton;
	MoveWindow(rect);
}
//-------------------------------------------------------------------------------------------------
int UCLIDExceptionDetailsDlg::getSelectedDebugParamsIndex()
{
	// Get first selected item from debug list ctrl
	POSITION pos = m_debugParamsListCtrl.GetFirstSelectedItemPosition();

	int iIndex = -1;
	int iSelectionCount = m_debugParamsListCtrl.GetSelectedCount();
	
	// if the pos is valid and only one item is selected get the index otherwise will
	// return the default (-1)
	if (pos != __nullptr && iSelectionCount == 1)
	{
		iIndex = m_debugParamsListCtrl.GetNextSelectedItem(pos);
	}
	return iIndex;
}
//-------------------------------------------------------------------------------------------------
void UCLIDExceptionDetailsDlg::copyDebugParamsColumnToClipboard(int iColumn)
{
	// There are only 3 columns in the list control
	ASSERT_ARGUMENT("ELI28729", iColumn >=0 && iColumn < 3);

	// Get the selected item
	int iSelectedItem = getSelectedDebugParamsIndex();

	// The selected item should be greater than or equal to 0
	if ( iSelectedItem >= 0)
	{
		// Create the clipboard manager
		ClipboardManager clipboardMgr( this );

		// Get the selected column
		string strValueText = m_debugParamsListCtrl.GetItemText(iSelectedItem, iColumn);
		
		// put value on the clipboard, if the value is encrypted the
		// sGetDataValue will decrypt it if licensing allows it
		clipboardMgr.writeText(UCLIDException::sGetDataValue(strValueText));
	}
}
//-------------------------------------------------------------------------------------------------
