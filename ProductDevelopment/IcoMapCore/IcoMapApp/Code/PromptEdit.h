//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	PromptEdit.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS: Duan Wang
//
//==================================================================================================

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include "DynamicInputGridWnd.h"

#include <string>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// PromptEdit window
//==================================================================================================
//
// CLASS:	PromptEdit
//
// PURPOSE:	To inherit base functions from CEdit control and extend more functionalities. This edit
//			control will provide two parts in one edit box: a prompt part and an input part. The prompt
//			part can only be set by the programmer who's using this PromptEdit control. And the 
//			interface user can not modify the prompt part. The input part can be set either by the 
//			application end user or the programmer whenever is appilicable.
//
class PromptEdit : public CEdit
{
// Construction
public:
	PromptEdit();

// Attributes
public:

// Operations
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(PromptEdit)
	public:
	virtual BOOL PreTranslateMessage(MSG* pMsg);
	protected:
	//}}AFX_VIRTUAL

// Implementation
public:
	//whenever the input is not from the user interface. 
	//For instance, if the user press Enter key while the input part of the 
	//edit box is empty, we'll use the same default value for that input type
	void SetInputText(const string& strInputText);
	//set the prompting text
	void SetPromptText(const string& strPrompt);
	//retrieve the input part
	string GetInputText() const;
	//retrieve the prompting text from the edit box
	string GetPromptText() const;

	void SetDIG(DynamicInputGridWnd* pDIG);

	// to set the BackGround Color for the Text and the Edit Box.
	void SetBkColor(COLORREF crColor); 

	virtual ~PromptEdit();

	// Generated message map functions
protected:
	//{{AFX_MSG(PromptEdit)
	afx_msg void OnKeyDown(UINT nChar, UINT nRepCnt, UINT nFlags);
	afx_msg void OnLButtonDown(UINT nFlags, CPoint point);
	afx_msg void OnLButtonUp(UINT nFlags, CPoint point);
	afx_msg void OnSetFocus(CWnd* pOldWnd);
	afx_msg void OnKeyUp(UINT nChar, UINT nRepCnt, UINT nFlags);
	afx_msg void OnLButtonDblClk(UINT nFlags, CPoint point);
	afx_msg void OnKillFocus(CWnd* pNewWnd);
	afx_msg void OnEnable(BOOL bEnable);
	// This Function Gets Called Every Time Your Window Gets Redrawn.
	afx_msg HBRUSH CtlColor(CDC* pDC, UINT nCtlColor); 
	//}}AFX_MSG

	//update/refresh the edit box with m_strPromptText and m_strInputText
	void UpdatePromptWindowText();

	DECLARE_MESSAGE_MAP()

private:

	CBrush m_brBkgnd; // Holds Brush Color for the Edit Box
	COLORREF m_crBkColor; // Holds the Background Color for the Text

	// the end position of the prompt text, i.e. the last char position
	int m_nPromptTextEndPos;
	// last position of the caret(cursor) before the edit looses the focus
	int m_nLastCaretPos;
	// prompting text
	string m_strPromptText;
	// whatever is entered by the user
	string m_strInputText;

	DynamicInputGridWnd* m_pDIG;

	//////////////
	// Methods
	/////////////
};

/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
