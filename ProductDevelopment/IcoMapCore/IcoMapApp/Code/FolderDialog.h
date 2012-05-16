#if !defined(AFX_MYFD_H__F9CB9441_F91B_11D1_8610_0040055C08D9__INCLUDED_)
#define AFX_MYFD_H__F9CB9441_F91B_11D1_8610_0040055C08D9__INCLUDED_

#if _MSC_VER >= 1000
#pragma once
#endif // _MSC_VER >= 1000
// MyFD.h : header file
//

/////////////////////////////////////////////////////////////////////////////
// CFolderDialog dialog

class CFolderDialog : public CFileDialog
{
	DECLARE_DYNAMIC(CFolderDialog)

public:
	virtual ~CFolderDialog() {};
	enum {OPEN_MODE, SELECT_MODE};
	static WNDPROC m_wndProc;
	virtual void OnInitDone( );
	CString m_pPath;
	CFolderDialog(CString pPath, int modeIn = SELECT_MODE);
	virtual void OnFolderChange(); 
	virtual void OnFileNameChange();
	void SetEdt1(CString inStr);	
	void GetEdt1(CString &inStr);	

protected:
	//{{AFX_MSG(CFolderDialog)
	virtual BOOL OnInitDialog();
	afx_msg int OnCreate(LPCREATESTRUCT lpCreateStruct);
	afx_msg void OnLButtonDblClk(UINT nFlags, CPoint point);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

	int m_mode;

};

//{{AFX_INSERT_LOCATION}}
// Microsoft Developer Studio will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_MYFD_H__F9CB9441_F91B_11D1_8610_0040055C08D9__INCLUDED_)

