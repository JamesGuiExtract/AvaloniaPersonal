#if !defined(AFX_INPUTMANAGER_H__B5F873CD_49E8_4B7F_B48D_FC10F883B284__INCLUDED_)
#define AFX_INPUTMANAGER_H__B5F873CD_49E8_4B7F_B48D_FC10F883B284__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
// Machine generated IDispatch wrapper class(es) created by Microsoft Visual C++

// NOTE: Do not modify the contents of this file.  If this class is regenerated by
//  Microsoft Visual C++, your modifications will be overwritten.


// Dispatch interfaces referenced by this interface
class CInputReceiver;
class COCRFilterMgr;
class CIUnknownVector;

/////////////////////////////////////////////////////////////////////////////
// CInputManager wrapper class

class CInputManager : public CWnd
{
protected:
	DECLARE_DYNCREATE(CInputManager)
public:
	CLSID const& GetClsid()
	{
		static CLSID const clsid
			= { 0x775accad, 0x32ac, 0x11d6, { 0x82, 0x59, 0x0, 0x50, 0xda, 0xd4, 0xff, 0x55 } };
		return clsid;
	}
	virtual BOOL Create(LPCTSTR lpszClassName,
		LPCTSTR lpszWindowName, DWORD dwStyle,
		const RECT& rect,
		CWnd* pParentWnd, UINT nID,
		CCreateContext* pContext = NULL)
	{ return CreateControl(GetClsid(), lpszWindowName, dwStyle, rect, pParentWnd, nID); }

    BOOL Create(LPCTSTR lpszWindowName, DWORD dwStyle,
		const RECT& rect, CWnd* pParentWnd, UINT nID,
		CFile* pPersist = NULL, BOOL bStorage = FALSE,
		BSTR bstrLicKey = NULL)
	{ return CreateControl(GetClsid(), lpszWindowName, dwStyle, rect, pParentWnd, nID,
		pPersist, bStorage, bstrLicKey); }

// Attributes
public:

// Operations
public:
	BOOL GetWindowsShown();
	BOOL GetInputIsEnabled();
	void EnableInput1(LPCTSTR strInputValidatorName, LPCTSTR strPrompt, LPDISPATCH pInputContext);
	void EnableInput2(LPDISPATCH pInputValidator, LPCTSTR strPrompt, LPDISPATCH pInputContext);
	void DisableInput();
	long CreateNewInputReceiver(LPCTSTR strInputReceiverName);
	long ConnectInputReceiver(LPDISPATCH pInputReceiver);
	void DisconnectInputReceiver(long nIRHandle);
	CInputReceiver GetInputReceiver(long nIRHandle);
	void ShowWindows(BOOL bShow);
	void Destroy();
	long GetParentWndHandle();
	void SetParentWndHandle(long nNewValue);
	void ProcessTextInput(LPCTSTR strInput);
	COCRFilterMgr GetOCRFilterMgr();
	void SetOCREngine(LPDISPATCH pEngine);
	void SetInputContext(LPDISPATCH pInputContext);
	CIUnknownVector GetInputReceivers();
	long GetHWND();
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_INPUTMANAGER_H__B5F873CD_49E8_4B7F_B48D_FC10F883B284__INCLUDED_)
