// IndexConverterDlg.h : header file
//

#pragma once

#include <XInfoTip.h>

#include <fstream>
#include <string>
#include <map>
#include <vector>
#include <IProgressTask.h>
/////////////////////////////////////////////////////////////////////////////
// CIndexConverterDlg dialog

class CIndexConverterDlg : public CDialog, public IProgressTask
{
// Construction
public:
	CIndexConverterDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	//{{AFX_DATA(CIndexConverterDlg)
	enum { IDD = IDD_INDEXCONVERTER_DIALOG };
	CEdit	m_editBeginString;
	CEdit	m_editEndString;
	CComboBox	m_comboConditionOp;
	CButton	m_btnReset;
	CEdit	m_editDelimiter;
	CButton	m_chkMoveFiles;
	CButton	m_btnAdd;
	CButton	m_chkConsolidate;
	CButton	m_btnBrowseEAV;
	CEdit	m_editAttrValue;
	CEdit	m_editAttrType;
	CEdit	m_editAttrName;
	CButton	m_btnDelete;
	CButton	m_btnModify;
	CEdit	m_editEAVFile;
	CEdit	m_editIndexFile;
	CButton	m_btnTo;
	CButton	m_btnFrom;
	CListCtrl	m_list;
	CButton	m_btnContinue;
	CButton	m_btnProcess;
	CString	m_zIndexFile;
	CString	m_zEAVFolder;
	CString	m_zFromFolder;
	CString	m_zToFolder;
	BOOL	m_bConsolidate;
	BOOL	m_bMoveFiles;
	CString	m_zDelimiter;
	CString	m_zName;
	CString	m_zTypeFormat;
	CString	m_zValueFormat;
	BOOL	m_bMoveFilesOnly;
	CString	m_zConditionLHS;
	CString	m_zConditionRHS;
	BOOL	m_bWriteConditionEnabled;
	CString	m_zBeginString;
	CString	m_zEndString;
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CIndexConverterDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	//{{AFX_MSG(CIndexConverterDlg)
	virtual BOOL OnInitDialog();
	virtual void OnCancel() {};
	virtual void OnOK() {};
	afx_msg void OnClose();
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	afx_msg void OnButtonIndex();
	afx_msg void OnInfoEAV();
	afx_msg void OnInfoFromFile();
	afx_msg void OnInfoToFolder();
	afx_msg void OnInfoAttrValue();
	afx_msg void OnInfoAttrType();
	afx_msg void OnButtonEav();
	afx_msg void OnButtonFrom();
	afx_msg void OnButtonTo();
	afx_msg void OnCheckMove();
	afx_msg void OnButtonProcess();
	afx_msg void OnButtonContinue();
	afx_msg void OnChangeEditDelimiter();
	afx_msg void OnChangeEditIndex();
	afx_msg void OnChangeEditEav();
	afx_msg void OnChangeEditFrom();
	afx_msg void OnChangeEditName();
	afx_msg void OnChangeEditTo();
	afx_msg void OnChangeEditValue();
	afx_msg void OnButtonClose();
	afx_msg void OnButtonAdd();
	afx_msg void OnButtonDelete();
	afx_msg void OnButtonModify();
	afx_msg void OnDblclkListTranslate(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnItemchangedListTranslate(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnCheckOnlyMove();
	afx_msg void OnButtonReset();
	afx_msg void OnCheckWriteCondition();
	afx_msg void OnInfoCondition();
	afx_msg void OnChangeEditBeginStringQualifier();
	afx_msg void OnChangeEditEndStringQualifier();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

public:
// IProgressTask
	void runTask(IProgress* pProgress, int nTaskID = 0);

private:

	/////////////
	// Variables
	/////////////
	// For balloon help
	CXInfoTip m_infoTip;

	// Whether or not the Process One is started
	bool m_bProcessOneStarted;

	// set once all the lines have been processed
	bool m_bDoneProcessing;

	std::map<std::string, std::string> m_mapTypeTranslations;

	// Input file stream
	std::ifstream m_ifs;


	////////////
	// Methods
	////////////
	bool findDuplicateEntry(const CString& zEntry, int nCurrentSelectedIndex);

	// call this method if the index file is changing.
	void indexFileChanging(const CString& zCurrentIndexFile);

	// Move one or more files from strMoveFromFolder to strMoveToFolder
	// Require: strFilesName shall not be fully qualified
	// Return : true -- if file found in move from folder and all moved successfully
	//			false -- if no file found in move from folder
	bool moveFiles(const std::string& strFilesName,
				   const std::string& strMoveFromFolder,
				   const std::string& strMoveToFolder);
				   
	// Processes the conversion according to which Process button is pressed
	void processConversion();

	bool promptForValue(CString& zEntry1, CString& zEntry2, int nCurrentSelectedIndex);

	// restart from the first record in the index file
	// and reset the caption for Process One and Process All buttons
	void reset();

	// Creates two column labels for Translate list
	void setColumnLabels();

	// translate the type values if any
	std::string translateType(const std::string& strType);

	// Enables and disables controls based on user settings
	void updateControls();

	// Update the map of translations whenever the list item changes
	void updateTranslation();

	// Validates values from edit boxes
	void validateEntries();
	
	// check whether the write condition is met for the current record
	bool isWriteConditionMet(std::vector<std::string>& rvecTokens);
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
