//==================================================================================================
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	UEXViewerDlg.h
//
// PURPOSE:	Provide a UI for UCLID Exception files.
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//==================================================================================================

#pragma once

#include <string>
#include <vector>
#include <IConfigurationSettingsPersistenceMgr.h>
#include <memory>
#include <UCLIDException.h>

using namespace std;

class CUEXFindDlg;
class CExportDebugDataDlg;

/////////////////////////////////////////////////////////////////////////////
// CUEXViewerDlg dialog

class CUEXViewerDlg : public CDialog
{
// Construction
public:
	//=======================================================================
	// PURPOSE: Constructs the modal dialog.
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	pParent - pointer to parent window 
	CUEXViewerDlg(CWnd* pParent = NULL);	// standard constructor
	~CUEXViewerDlg();

	// Returns number of Exceptions in list
	int		GetExceptionCount();

	// Returns zero-based index of first selected exception
	int		GetFirstSelectionIndex();

	// Returns UE.asString() for the specified exception
	string GetWholeExceptionString(int nIndex);

	// Selects each specified exception
	void	SelectExceptions(std::vector<int> &rvecExceptionIndices);

// Dialog Data
	enum { IDD = IDD_UEXVIEWER_DIALOG };
	CButton	m_find;
	CListCtrl	m_listUEX;
	CComboBox m_comboExceptionsList;

	// virtual function overrides
	public:
	virtual BOOL DestroyWindow();
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support
	virtual BOOL PreTranslateMessage(MSG* pMsg);

// Implementation
protected:
	// Handle to application icon
	HICON		m_hIcon;

	// Handles storage and retrieval of persistence information to and from 
	// the registry
	IConfigurationSettingsPersistenceMgr *m_pCfgMgr;

	// the currently open exception file
	std::string m_strCurrentFile;
	void setNewCurrentFile(std::string strNewCurrentFile);

	// Generated message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	afx_msg void OnDblclkListUex(NMHDR* pNMHDR, LRESULT* pResult);
	virtual void OnOK();
	virtual void OnCancel();
	afx_msg void OnFileOpen();
	afx_msg void OnEditPaste();
	afx_msg void OnEditClear();
	afx_msg void OnEditViewDetails();
	afx_msg void OnClickListUex(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnColumnclickListUex(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnEditDeleteSelection();
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg void OnGetMinMaxInfo(MINMAXINFO* pMMI);
	afx_msg void OnFileExport();
	afx_msg void OnEditInvertSelection();
	afx_msg void OnEditFind();
	afx_msg void OnDropFiles( HDROP hDropInfo );
	afx_msg void OnFileExit();
	afx_msg void OnFileOpenPrevLogFile();
	afx_msg void OnFileOpenNextLogFile();
	afx_msg void OnFileRefreshCurrentLogfile();
	afx_msg void OnHelpAbout();
	afx_msg void OnBnClickedBtnPrevLogFile();
	afx_msg void OnBnClickedBtnNextLogFile();
	afx_msg void OnClose();
	afx_msg void OnCbnSelchangeComboExceptionFileList();
	afx_msg void OnCopyELICode();
	afx_msg void OnNMRclickListUex(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnToolsExportDebugData();

	DECLARE_MESSAGE_MAP()

	//=======================================================================
	// PURPOSE: Retrieves registry information for working directory, size 
	//				and position of dialog, and column widths for UEX list.
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	None.
	void initPersistent();
	
	//=======================================================================
	// PURPOSE: Sets up list control properties and creates column headers 
	//				for the UEX list.
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	None.
	void prepareList();
	
	//=======================================================================
	// PURPOSE: Updates enabled/disabled state for each button in the dialog.
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	None.
	void updateEnabledStateForControls();
	
	//=======================================================================
	// PURPOSE: Parses the specified UEX file and adds the extracted 
	//				exception information to the UEX list.
	// REQUIRE: Nothing
	// PROMISE: If bReplaceMode == true, the contents of strUEXFile will
	//			replace the existing contents of dialog and strUEXFile will
	//			become the new current file and the caption will be updated, etc.
	//			If bReplaceMode == false, the contents of strUEX file will
	//			be appended to the existing contents, but the name of the current
	//			file, and the associated caption, etc will not change.
	// ARGS:	strUEXFile - Full path to selected UEX file 
	void addExceptions(string strUEXFile, bool bReplaceMode);
	
	//=======================================================================
	// PURPOSE: Retrieves persistent column width from registry.
	// REQUIRE: Nothing
	// PROMISE: Returns width from registry or 80 pixels if not found or 
	//			out of range.
	// ARGS:	strColumn - name of registry key 
	long getColumnWidth(string strColumn);
	
	//=======================================================================
	// PURPOSE: Extracts UCLID Exception properties from specified line of 
	//				text.  Adds new line to UEX list.  Populates ITEMINFO 
	//				structure as appropriate. If the line is unparsable
	//				the method will return false.
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	strText - Single line from UEX file
	bool parseLine(const string& strText);

	//=======================================================================
	// PURPOSE: Updates iIndex field in ITEMINFO structure for each item in 
	//			UEX list.  This is required after a Sort operation.
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	None.
	void refreshIndices();

	//=======================================================================
	// PURPOSE: Writes the specified text to the appropriate cell.
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	iIndex - Row in list
	//			iColumn - Column in list
	//			strText - Text for cell
	void setItemText(int iIndex, int iColumn, string strText);

private:
	// Width of each column
	int		m_iSerialColumnWidth;
	int		m_iApplicationColumnWidth;
	int		m_iComputerColumnWidth;
	int		m_iUserColumnWidth;
	int		m_iPidColumnWidth;
	int		m_iTimeColumnWidth;
	int		m_iELIColumnWidth;
	int		m_iExceptionColumnWidth;

	// Folder containing opened UEX file
	CString	m_zDirectory;

	// Modeless Find dialog
	auto_ptr<CUEXFindDlg> ma_pFindDlg;
	auto_ptr<CExportDebugDataDlg> ma_pExportDebugDataDlg;

	// Have the dialog's controls been instantiated yet - allows for resize
	// and repositioning
	bool	m_bInitialized;
};
