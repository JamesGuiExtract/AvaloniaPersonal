//=================================================================================================
// COPYRIGHT UCLID SOFTWARE, LLC. 1999
//
// FILE:	UCLIDExceptionDetailsDlg.h
//
// PURPOSE: The purpose of this file is to define  the functionality of UCLIDExceptionDetailsDlg class 
//          It shows up a dialog with two list controls, in which ELI, its related text and Debug 
//          information is shown to the user.This dialog is called from IcoMapInformationDlg class
//			when one of its buttons "Debug Information" is clicked.	
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
#pragma once

#include "BaseUtils.h"
#include <map>

class UCLIDException;

//==================================================================================================
//
// CLASS:	UCLIDExceptionDetailsDlg
//
// PURPOSE:	The purpose of this class is to create a dialog to show the UCLID Exceptions that were
//			caught by the application. It takes ELI and its related text and debug information associated
//          with the UCLIDException objects and presents in two list controls. The information can also
//          be saved into a file. This facilitates the users to send the details of exceptions caught while 
//          they are running the application. It will be easy to  test the application or to fix the bugs 
//          by developers with the help of the file sent by user
//
// REQUIRE: A reference to valid UCLIDException object is required
//			
// 
// INVARIANTS: UCLIDException object passed to the class
//			
// EXTENSIONS:
//			
// NOTES:	
//==================================================================================================

class EXPORT_BaseUtils UCLIDExceptionDetailsDlg : public CDialog
{
// Construction
public:
	
	//--------------------------------------------------------------------------------------------------
	// Purpose: constructor with UCLIDException object passed to this dialog
	//          
	// Require: nothing
	// 
	// Promise: constructs the dialog
	//
	// ARGS:	UclidExceptionCaught: UCLIDException object sent to this class
	UCLIDExceptionDetailsDlg(const UCLIDException& UclidExceptionCaught,CWnd* pParent = NULL); 
	
	//--------------------------------------------------------------------------------------------------
	// Purpose: destructor
	//          
	// Require: nothing
	// 
	// Promise: destruct the dialog
	//
	// ARGS:	
	~UCLIDExceptionDetailsDlg(void); 


	//--------------------------------------------------------------------------------------------------
	// Purpose: to load the Exception information into corresponding list controls
	//          
	// 
	// Require:  UclidExceptionCaught should be valid
	// 
	// Promise: loads the Exception History as well as debug info into list controls
	//
	// ARGS:	None
	void loadDebugInfoInListCtrls();
	

	//{{AFX_DATA(UCLIDExceptionDetailsDlg)
	CListCtrl	m_ELIandTextListCtrl;
	CListCtrl	m_debugParamsListCtrl;
	CStatic		m_StackTraceStatic;
	CListCtrl	m_StackTraceListCtrl;
	CButton		m_SaveAsButton;
	CButton		m_CopyButton;
	CButton		m_CloseButton;
	CButton		m_AllExceptionsRadioButton;
	//}}AFX_DATA


	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(UCLIDExceptionDetailsDlg)
	public:
	virtual int DoModal();
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

protected:

	// Generated message map functions
	//{{AFX_MSG(UCLIDExceptionDetailsDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnButtonSaveAs();
	afx_msg void OnButtonCopy();
	afx_msg void OnRclickDebugParameters(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnEditCopy();
	afx_msg void OnItemChangedListEliAndText(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnDebugInformationClick();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	// Pointer to the UCLIDException that is being displayed.
	const UCLIDException* m_pUclidExceptonToLoad;
	
	// Keep track of text to be copied to clipboard if "copy" context menu is selected.
	CString m_zClipboardText;

	// Map exception list item number to the exception instance pointer.
	std::map<long, const UCLIDException *> m_mapItemToException;

	// Variable to hold the state of the Debug details group
	// 0 - Display debug data for the currently selected ELI code in the m_ELIandTextListCtrl
	// 1 - Display debug data for all exceptions.
	int m_iDebugData;

	// Method to setup the Details dialog for non internal use - no stack trace list.
	void resizeForNonInternalUse();

	// Method to load the exception data.
	void loadExceptionData(const UCLIDException &ex);

	// Method to load the stack trace data in the the stack list ctrl if not internal use just 
	// returns without adding anything to the list.
	void loadStackTraceData(const UCLIDException &ex);
	
	// Returns the decrypted string if it was encrypted if not just returns the string.
	std::string getDataValue(const std::string strEncrpyted);
};
