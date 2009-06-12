 #if !defined(AFX_QUERYDLG_H__555AA481_0EA1_49FB_808F_0ABA8FD38F25__INCLUDED_)
#define AFX_QUERYDLG_H__555AA481_0EA1_49FB_808F_0ABA8FD38F25__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
// QueryDlg.h : header file
//

#include <string>

/////////////////////////////////////////////////////////////////////////////
// QueryDlg dialog

class QueryDlg : public CDialog
{
// Construction
public:
	QueryDlg(IApplicationPtr ipApp, CWnd* pParent = NULL);   // standard constructor
	~QueryDlg();

// Dialog Data
	//{{AFX_DATA(QueryDlg)
	enum { IDD = IDD_DLG_QUERY_FEATURE };
	CEdit	m_editQQQ;
	CEdit	m_editQQQQ;
	CEdit	m_editQQ;
	CEdit	m_editQuarter;
	CEdit	m_editSectionNum;
	CComboBox	m_cmbLayer;
	int		m_nRangeDir;
	int		m_nTownshipDir;
	CString	m_zCountyCode;
	CString	m_zQQ;
	CString	m_zQQQQ;
	CString	m_zQuarter;
	CString	m_zRange;
	CString	m_zSectionNum;
	CString	m_zTownship;
	int		m_nLayer;
	CString	m_zQQQ;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(QueryDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(QueryDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnSelchangeCmbLayer();
	afx_msg void OnBtnQuery();
	afx_msg void OnOK();
	afx_msg void OnCancel();
	afx_msg void OnClose();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:

	///////////
	// Variables
	///////////

	IApplicationPtr m_ipApp;


	///////////
	// Methods
	///////////

	// Get the feature layer with the specified name
	ILayerPtr getLayer(const std::string& strLayerName);

	// get current where clause based on the user input from the dlg
	std::string getWhereClause();

	// select the specified feature
	void selectFeature();

	// Updates the edit boxes' state based on the layer selection in the combo box
	void updateEditControls();

	// zoom into the selected feature(s)
	void zoomToSelectedFeature(IActiveViewPtr ipActiveView);

	// Looks up strLayerIDName in GridGenerator.ini file if found loads that name if not loads strLayerIDName in List box
	void addLayerFromINItoLB( std::string strLayerIDName);


	// The fieldType is assumed to be either esriFieldTypeSmallInteger or esriFieldTypeString
	void addANDClause( std::string& strWhereClause, const std::string &strFldName, std::string strValue, esriFieldType fieldType = esriFieldTypeSmallInteger );
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_QUERYDLG_H__555AA481_0EA1_49FB_808F_0ABA8FD38F25__INCLUDED_)
