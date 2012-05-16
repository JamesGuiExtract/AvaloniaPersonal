// TestDlg.h : header file
//

#if !defined(AFX_TESTDLG_H__31E615F8_3C2B_4550_87AD_743E79A4D359__INCLUDED_)
#define AFX_TESTDLG_H__31E615F8_3C2B_4550_87AD_743E79A4D359__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

/////////////////////////////////////////////////////////////////////////////
// CTestDlg dialog
/////////////////////////////////////////////////////////////////////////////

// COM Object information
#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDIUnknownVector\Code\UCLIDIUnknownVector.tlb"
using namespace UCLIDIUNKNOWNVECTORLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDCurveParameter\Code\UCLIDCurveParameter.tlb"
using namespace UCLIDCURVEPARAMETERLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDFeatureMgmt\Code\UCLIDFeatureMgmt.tlb"
using namespace UCLIDFEATUREMGMTLib;

// Dialog class
class CTestDlg : public CDialog
{
// Construction
public:
	CTestDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	//{{AFX_DATA(CTestDlg)
	enum { IDD = IDD_TEST_DIALOG };
	BOOL	m_bProvideOriginal;
	BOOL	m_bShowOriginal;
	BOOL	m_bCurrRO;
	BOOL	m_bOrigRO;
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CTestDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Creates one or both Features needed for Attribute Viewer dialog
	void	createFeatures();

	// Adds a Line segment to the single part belonging to the specified Feature
	BOOL	addLine(LPCTSTR pszBearing, LPCTSTR pszDistance, BOOL bCurrentPart);

	// Adds an Arc segment to the single part belonging to the specified Feature
	BOOL	addCurve(LPCTSTR pszRadius, LPCTSTR pszChordBearing, 
		LPCTSTR pszChordLength, LPCTSTR pszConcaveLeft, BOOL bCurrentPart);

	// Generated message map functions
	//{{AFX_MSG(CTestDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	afx_msg void OnButton1();
	afx_msg void OnButton2();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	// Icon for dialog
	HICON		m_hIcon;

	// The desired Features have been created
	BOOL		m_bCreated;

	// Holds data for Current Attributes
	IFeaturePtr	m_ptrCurrFeature;			

	// Holds data for Original Attributes
	IFeaturePtr	m_ptrOrigFeature;			
	
	// Single part within Current Attributes Feature
	IPartPtr	m_ptrCurrPart;				
	
	// Single part within Original Attributes Feature
	IPartPtr	m_ptrOrigPart;				
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_TESTDLG_H__31E615F8_3C2B_4550_87AD_743E79A4D359__INCLUDED_)
