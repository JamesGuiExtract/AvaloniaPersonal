#pragma once
// InputCorrectionDlg.h : header file
//

#include "resource.h"


/////////////////////////////////////////////////////////////////////////////
// InputCorrectionDlg dialog

class InputCorrectionDlg : public CDialog
{
// Construction
public:
	InputCorrectionDlg(IInputValidator* ipInputValidator, ITextInput* ipTextInput,CWnd* pParent = NULL);   // standard constructor

// Dialog Data
	//{{AFX_DATA(InputCorrectionDlg)
	enum { IDD = IDD_INPUT_CORRECTION_DLG };
	CButton	m_btnSaveImage;
	CStatic	m_ctrlImageLoader;
	CEdit	m_ctrlInputText;
	CString	m_editInputType;
	CString	m_editInputText;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(InputCorrectionDlg)
	public:
	virtual int DoModal();
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(InputCorrectionDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnChangeEditInputText();
	virtual void OnOK();
	virtual void OnCancel();
	afx_msg void OnBTNSaveImageAs();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	/////////////////////////////////////////////////
	// Memeber variables

	// whether or not the input is correct
	bool m_bIsInputCorrect;

	// input validator
	CComQIPtr<IInputValidator> m_ipInputValidator;

	// text input
	CComQIPtr<ITextInput> m_ipTextInput;

	// temporary text input object, will be used to hold temporary text
	// to be validated by input validator
	CComQIPtr<ITextInput> m_ipTempTextInput;

	CString m_zImageFileName;

	///////////////////////////////////////////////////
	// Helper functions

	// initialize the dialog size according to whether the image file
	// name is empty or not
	void initDialogSize(const CString& cstrImageFileName);

	// update the bitmap to reflect whether the input is correct or not
	void updateInputStatusBitmap();
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

