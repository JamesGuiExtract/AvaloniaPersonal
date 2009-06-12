#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
// NumberInputDlg.h : header file
//

//#import "..\..\..\..\..\..\ReusableComponents\COMComponents\UCLIDIUnknownVector\Code\UCLIDIUnknownVector.tlb"
//using namespace UCLIDIUNKNOWNVECTORLib;

#import "TestComponents.tlb"
using namespace TESTCOMPONENTSLib;

/////////////////////////////////////////////////////////////////////////////
// NumberInputDlg dialog
//class CNumberInputReceiver;

class NumberInputDlg : public CDialog
{
// Construction
public:
	NumberInputDlg(CWnd* pParent = NULL);   // standard constructor
	~NumberInputDlg();
	BOOL CreateModeless();

public:
	void SetParentCtrl(TESTCOMPONENTSLib::INumberInputReceiver* ipInputReceiver){m_ipInputReceiver = ipInputReceiver;}

// Dialog Data
	//{{AFX_DATA(NumberInputDlg)
	enum { IDD = IDD_DLG_INPUT };
	CEdit	m_ctrlInput;
	CString	m_editContent;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(NumberInputDlg)
	public:
	virtual BOOL Create(UINT nIDTemplate, CWnd* pParentWnd = NULL );
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual void PostNcDestroy();
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(NumberInputDlg)
	afx_msg void OnClose();
	virtual BOOL OnInitDialog();
	afx_msg void OnBtnSubmit();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	TESTCOMPONENTSLib::INumberInputReceiver *m_ipInputReceiver;

	CWnd* m_pParent;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
