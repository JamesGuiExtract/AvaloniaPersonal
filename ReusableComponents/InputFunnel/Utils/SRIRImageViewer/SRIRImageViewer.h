// SRIRImageViewer.h : main header file for the SRIRIMAGEVIEWER application
//

#if !defined(AFX_SRIRIMAGEVIEWER_H__9E074D84_ECAD_4025_AED5_90E6C962AEB2__INCLUDED_)
#define AFX_SRIRIMAGEVIEWER_H__9E074D84_ECAD_4025_AED5_90E6C962AEB2__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// main symbols
#include "SRIRImageViewerDlg.h"

#include <memory>
#include <vector>
#include <string>

using namespace std;
/////////////////////////////////////////////////////////////////////////////
// CSRIRImageViewerApp:
// See SRIRImageViewer.cpp for the implementation of this class
//
class CSRIRImageViewerApp : public CWinApp
{
public:
	CSRIRImageViewerApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CSRIRImageViewerApp)
	public:
	virtual BOOL InitInstance();
	virtual int ExitInstance();
	virtual BOOL PreTranslateMessage(MSG* pMsg);
	//}}AFX_VIRTUAL

// Implementation

	//{{AFX_MSG(CSRIRImageViewerApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:

	///////////////
	// Enums
	///////////////

	///////////////
	// Methods
	///////////////
	const string getUsage();
	const string getScriptUsage();
	const string getCtrlIdUsage();
	
	UINT registerMessage(const char* szMsgName);

	void sendWindowMsg(HWND hWnd);

	void addToWindowHandleVector(HWND hWnd);
	static BOOL CALLBACK enumSRIRImageWindows(HWND hwnd, LPARAM lParam);

	///////////////
	// Data
	///////////////
	unique_ptr<CSRIRImageViewerDlg> m_apDlg;
	UINT m_uiMsgLoadImage;
	UINT m_uiMsgExecScript;
	UINT m_uiMsgCloseViewer;
	UINT m_uiMsgOCRToFile;
	UINT m_uiMsgOCRToClipboard;
	UINT m_uiMsgOCRToMessageBox;
	bool m_bWindowCreated;

	// Messaging
	UINT m_uiMsgToSend;
	string m_strMessageFileName;
	vector<HWND> m_vecWindowHandles;

};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_SRIRIMAGEVIEWER_H__9E074D84_ECAD_4025_AED5_90E6C962AEB2__INCLUDED_)
