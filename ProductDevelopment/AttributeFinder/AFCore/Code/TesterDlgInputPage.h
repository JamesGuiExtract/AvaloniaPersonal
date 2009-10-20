
#pragma once

#include "resource.h"

class TesterConfigMgr;

#include <string>
#include <map>

/////////////////////////////////////////////////////////////////////////////
// TesterDlgInputPage dialog

class TesterDlgInputPage : public CPropertyPage
{
	DECLARE_DYNCREATE(TesterDlgInputPage)

// Construction
public:
	TesterDlgInputPage();
	~TesterDlgInputPage();
	void setTesterConfigMgr(TesterConfigMgr *pTesterConfigMgr);
	void notifyImageWindowInputReceived(ISpatialStringPtr ipSpatialText);

	// PROMISE: To return true only if the specified file was successfully
	//			read in.
	bool notifyFileDropped(const std::string& strFileName);
	
	// get current text from the input page
	ISpatialStringPtr getText();

// Dialog Data
	//{{AFX_DATA(TesterDlgInputPage)
	enum { IDD = IDD_TESTERDLG_INPUT_PAGE };
	CComboBox	m_comboInput;
	CEdit	m_editInput;
	CEdit	m_editFile;
	CButton m_checkOCR;
	CButton m_btnBrowse;
	CStatic m_labelInput;
	CStatic m_labelFileName;
	//}}AFX_DATA


// Overrides
	// ClassWizard generate virtual function overrides
	//{{AFX_VIRTUAL(TesterDlgInputPage)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	ISpatialStringPtr openFile(const std::string& strFileName);

	// Generated message map functions
	//{{AFX_MSG(TesterDlgInputPage)
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg void OnBrowse();
	virtual BOOL OnInitDialog();
	afx_msg void OnSelchangeComboInput();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:

	//////////////////
	// Methods
	//////////////////

	void resizeControls();

	//------------------------------------------------------------------------------------------------
	// PURPOSE: returns a pointer to a valid SSOCR engine.
	// PROMISE: creates and licenses a new SSOCR engine if m_ipOCREngine is NULL.
	//          sets the new SSOCR engine to the default for the input manager.
	//          returns m_ipOCREngine.
	IOCREnginePtr getOCREngine();

	//////////////////
	// Variables
	//////////////////

	std::map<std::string, ISpatialStringPtr> m_mapInputTypeToText;
	std::string m_strCurrentInputType;

	TesterConfigMgr *m_pTesterConfigMgr;

	IOCREnginePtr m_ipOCREngine;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
