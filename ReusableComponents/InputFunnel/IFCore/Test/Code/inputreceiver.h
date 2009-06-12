#if !defined(AFX_INPUTRECEIVER_H__B40D98C1_3CFA_11D6_8266_0050DAD4FF55__INCLUDED_)
#define AFX_INPUTRECEIVER_H__B40D98C1_3CFA_11D6_8266_0050DAD4FF55__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
// Machine generated IDispatch wrapper class(es) created by Microsoft Visual C++

// NOTE: Do not modify the contents of this file.  If this class is regenerated by
//  Microsoft Visual C++, your modifications will be overwritten.

/////////////////////////////////////////////////////////////////////////////
// CInputReceiver wrapper class

class CInputReceiver : public COleDispatchDriver
{
public:
	CInputReceiver() {}		// Calls COleDispatchDriver default constructor
	CInputReceiver(LPDISPATCH pDispatch) : COleDispatchDriver(pDispatch) {}
	CInputReceiver(const CInputReceiver& dispatchSrc) : COleDispatchDriver(dispatchSrc) {}

// Attributes
public:

// Operations
public:
	BOOL GetWindowShown();
	BOOL GetInputIsEnabled();
	BOOL GetHasWindow();
	long GetWindowHandle();
	void EnableInput(LPCTSTR strInputType, LPCTSTR strPrompt);
	void DisableInput();
	void SetEventHandler(LPDISPATCH pEventHandler);
	void ShowWindow(BOOL bShow);
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_INPUTRECEIVER_H__B40D98C1_3CFA_11D6_8266_0050DAD4FF55__INCLUDED_)
