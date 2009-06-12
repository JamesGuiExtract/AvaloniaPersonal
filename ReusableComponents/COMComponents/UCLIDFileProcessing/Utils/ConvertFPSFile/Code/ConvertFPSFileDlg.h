// ConvertFPSFileDlg.h : header file
//

#pragma once

#include <string>


// CConvertFPSFileDlg dialog
class CConvertFPSFileDlg : public CDialog
{
// Construction
public:
	CConvertFPSFileDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	enum { IDD = IDD_CONVERTFPSFILE_DIALOG };
	CString	m_zInputFile;
	CString	m_zOutputFile;

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support


// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	afx_msg void OnBnClickedBtnInput();
	afx_msg void OnBnClickedBtnOutput();
	afx_msg void OnBnClickedOk();
	DECLARE_MESSAGE_MAP()

private:
	// Reads old version FPS file (strInput), converts data to new types, saves
	// result as new version FPS file (strOutput)
	void	convertFPSFile(std::string strInput, std::string strOutput);

	// Builds a string for use in Object With Description as 
	// ICategorizedComponent::GetComponentDescription() surrounded by <>
	std::string	makeDescription(IUnknownPtr ipObject);
};
