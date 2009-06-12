// AdjustImageResolutionDlg.h : header file
//

#pragma once


// CAdjustImageResolutionDlg dialog
class CAdjustImageResolutionDlg : public CDialog
{
// Construction
public:
	CAdjustImageResolutionDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	enum { IDD = IDD_ADJUSTIMAGERESOLUTION_DIALOG };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support


// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	DECLARE_MESSAGE_MAP()
};
