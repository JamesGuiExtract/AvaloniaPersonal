// PromptEdit.cpp : implementation file
//
#include "stdafx.h"
#include "PromptEdit.h"

#include <UCLIDException.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//--------------------------------------------------------------------------------------------------
// PromptEdit
//--------------------------------------------------------------------------------------------------
PromptEdit::PromptEdit()
:m_strInputText(""), 
 m_strPromptText(""), 
 m_nPromptTextEndPos(0), 
 m_nLastCaretPos(0),
 m_pDIG(NULL)
{
	m_crBkColor = ::GetSysColor(COLOR_WINDOW); // Initializing background color to the system face color.
	m_brBkgnd.CreateSolidBrush(m_crBkColor); // Creating the Brush Color For the Edit Box Background
}
//--------------------------------------------------------------------------------------------------
PromptEdit::~PromptEdit()
{
}
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(PromptEdit, CEdit)
	//{{AFX_MSG_MAP(PromptEdit)
	ON_WM_KEYDOWN()
	ON_WM_LBUTTONDOWN()
	ON_WM_LBUTTONUP()
	ON_WM_SETFOCUS()
	ON_WM_KEYUP()
	ON_WM_LBUTTONDBLCLK()
	ON_WM_KILLFOCUS()
	ON_WM_ENABLE()
	ON_WM_CTLCOLOR_REFLECT()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//--------------------------------------------------------------------------------------------------
// PromptEdit message handlers
//--------------------------------------------------------------------------------------------------
void PromptEdit::SetPromptText(const string &strPrompt)
{
	//store the prompting text along with a semicolon
	m_strPromptText = strPrompt + " ";

	if (m_strPromptText.length() > 0)
	{
		m_nPromptTextEndPos = m_strPromptText.length() - 1;
	}
	else
	{
		m_nPromptTextEndPos = 0;
	}

	UpdatePromptWindowText();

}
//--------------------------------------------------------------------------------------------------
string PromptEdit::GetPromptText() const
{
	return m_strPromptText.substr(0, m_nPromptTextEndPos);
}
//--------------------------------------------------------------------------------------------------
void PromptEdit::SetInputText(const string &strInputText)
{
	m_strInputText = strInputText;
	UpdatePromptWindowText();
}
//--------------------------------------------------------------------------------------------------
string PromptEdit::GetInputText() const
{
	CString cstrWindowText;
	GetWindowText(cstrWindowText);
	//retrieve the input part from the whole text
	string strInput(LPCTSTR(cstrWindowText.Right(cstrWindowText.GetLength() - m_nPromptTextEndPos)));

	return strInput;
}
//--------------------------------------------------------------------------------------------------
void PromptEdit::SetDIG(DynamicInputGridWnd* pDIG)
{
	m_pDIG = pDIG;
}
//--------------------------------------------------------------------------------------------------
void PromptEdit::SetBkColor(COLORREF crColor)
{
	// Passing the value passed by the dialog to the member varaible for Backgound Color
	m_crBkColor = crColor; 
	// Deleting any Previous Brush Colors if any existed.
	m_brBkgnd.DeleteObject(); 
	// Creating the Brush Color For the Edit Box Background
	m_brBkgnd.CreateSolidBrush(crColor); 

	RedrawWindow();
}
//--------------------------------------------------------------------------------------------------
HBRUSH PromptEdit::CtlColor(CDC* pDC, UINT nCtlColor)
{
	HBRUSH hbr;

	try
	{
		// Passing a Handle to the Brush
		hbr = (HBRUSH)m_brBkgnd;
		// Setting the Color of the Text Background to the one passed by the Dialog
		pDC->SetBkColor(m_crBkColor); 
		// Setting the Text Color to black
		pDC->SetTextColor(COLOR_WINDOWTEXT); 
		
		// To get rid of compiler warning
		if (nCtlColor)       
		{
			nCtlColor += 0;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12592")

	return hbr;
}
//--------------------------------------------------------------------------------------------------
void PromptEdit::OnKeyDown(UINT nChar, UINT nRepCnt, UINT nFlags) 
{
	CEdit::OnKeyDown(nChar, nRepCnt, nFlags);

	try
	{
		int nStartPos, nEndPos;
		//get the caret position
		GetSel(nStartPos, nEndPos);
		
		switch (nChar)
		{
		case VK_LEFT:	//left arrow key pressed
		case VK_UP:		//up arrow key
			//if the caret position goes into the prompt text part,
			//set the caret back to the first char pos of the input text part
			if (nStartPos < m_nPromptTextEndPos)
			{
				SetSel(m_nPromptTextEndPos, m_nPromptTextEndPos);
			}
			break;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12591")	
}
//--------------------------------------------------------------------------------------------------
void PromptEdit::OnLButtonDown(UINT nFlags, CPoint point) 
{	
	CEdit::OnLButtonDown(nFlags, point);

	try
	{
		int nStartPos, nEndPos;
		//get the caret position
		GetSel(nStartPos, nEndPos);
		
		if (nStartPos < m_nPromptTextEndPos)
		{
			SetSel(m_nPromptTextEndPos, m_nPromptTextEndPos);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12590")	
}
//--------------------------------------------------------------------------------------------------
void PromptEdit::OnLButtonUp(UINT nFlags, CPoint point) 
{	
	CEdit::OnLButtonUp(nFlags, point);
	
	try
	{
		int nStartPos, nEndPos;
		//get the caret position
		GetSel(nStartPos, nEndPos);
		if (nStartPos <= m_nPromptTextEndPos  && nEndPos > m_nPromptTextEndPos)
		{
			SetSel(m_nPromptTextEndPos, nEndPos);
		}
		else if (nStartPos < m_nPromptTextEndPos)
		{
			SetSel(m_nPromptTextEndPos, m_nPromptTextEndPos);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12589")	
}
//--------------------------------------------------------------------------------------------------
void PromptEdit::OnLButtonDblClk(UINT nFlags, CPoint point) 
{
	try
	{
		int nStartPos, nEndPos;
		//get the caret position
		GetSel(nStartPos, nEndPos);
		
		//disable left mouse double click if the caret wants to go into the prompt part
		if (nStartPos  > m_nPromptTextEndPos)
		{
			CEdit::OnLButtonDblClk(nFlags, point);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12588")	
}
//--------------------------------------------------------------------------------------------------
void PromptEdit::OnSetFocus(CWnd* pOldWnd) 
{
	try
	{
		// set to last record
		if (m_pDIG != NULL && pOldWnd != NULL)
		{
			if (m_pDIG->m_hWnd == pOldWnd->m_hWnd
				|| (pOldWnd->GetParent() != NULL 
					&& m_pDIG->m_hWnd == pOldWnd->GetParent()->m_hWnd))
			{
				m_pDIG->deactivateDIG();
			}
		}

		//when the focus is on this prompt edit box, 
		//set the caret at the origin of input text
		CEdit::OnSetFocus(pOldWnd);
		
		if (m_nLastCaretPos < m_nPromptTextEndPos)
		{
			m_nLastCaretPos = m_nPromptTextEndPos;
		}
		
		SetSel(m_nLastCaretPos, m_nLastCaretPos);

		// set background to yellow
		SetBkColor(RGB(255, 255, 0));
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12247")
}
//--------------------------------------------------------------------------------------------------
BOOL PromptEdit::PreTranslateMessage(MSG* pMsg) 
{
	try
	{
		//if the message is for this prompt edit box
		if (this->m_hWnd == pMsg->hwnd)
		{
			if (pMsg->message == WM_KEYDOWN) 
			{
				CString	classname;
				CWnd	*pwnd = FromHandle(pMsg->hwnd);
				
				if (GetClassName(pwnd->GetSafeHwnd(), classname.GetBuffer(20),20) != 0)
				{	//if return key is pressed inside PromptEdit box
					if (classname == "Edit")
					{
						int wParamValue = static_cast<int>(pMsg->wParam);
						
						int nStartPos, nEndPos;
						//get the caret position
						GetSel(nStartPos, nEndPos);
						switch (wParamValue)
						{
						case VK_BACK:
							{
								//if the caret position goes into the prompt text part,
								//set the caret back to the first char pos of the input text part
								if ((nStartPos <= m_nPromptTextEndPos) && (nEndPos == nStartPos))
								{
									return TRUE;
								}
							}
							break;
						case VK_HOME:
							{
								bool bShiftKeyDown = ::GetKeyState(VK_SHIFT) < 0 ? true : false;
								if (bShiftKeyDown)
								{
									SetSel(m_nPromptTextEndPos, nEndPos);
								}
								else
								{
									SetSel(m_nPromptTextEndPos, m_nPromptTextEndPos);
								}
								return TRUE;
							}
							break;
						}				
					}
				}
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12587")	

	return CEdit::PreTranslateMessage(pMsg);
}
//--------------------------------------------------------------------------------------------------
void PromptEdit::UpdatePromptWindowText()
{
	CString cstrWindowText;
	cstrWindowText = (m_strPromptText.substr(0, m_nPromptTextEndPos) + m_strInputText).c_str();
	SetWindowText(cstrWindowText);
	//set caret to the end of the text
	SetSel(-1);
}
//--------------------------------------------------------------------------------------------------
void PromptEdit::OnKeyUp(UINT nChar, UINT nRepCnt, UINT nFlags) 
{
	try
	{
		int nStartPos, nEndPos;
		//get the caret position
		GetSel(nStartPos, nEndPos);
		
		if (nStartPos  < m_nPromptTextEndPos)
		{
			SetSel(-1);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12586")	

	CEdit::OnKeyUp(nChar, nRepCnt, nFlags);
}
//--------------------------------------------------------------------------------------------------
void PromptEdit::OnKillFocus(CWnd* pNewWnd) 
{
	CEdit::OnKillFocus(pNewWnd);

	try
	{
		int nStartPos, nEndPos;
		//get the caret position
		GetSel(nStartPos, nEndPos);
		
		m_nLastCaretPos = nEndPos;
		
		// if the focus is taken by DIG
		if (m_pDIG != NULL && pNewWnd != NULL)
		{
			if (m_pDIG->m_hWnd == pNewWnd->m_hWnd
				|| (pNewWnd->GetParent() != NULL 
				&& m_pDIG->m_hWnd == pNewWnd->GetParent()->m_hWnd))
			{
				// set it back to original color
				SetBkColor(::GetSysColor(COLOR_WINDOW));
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12585")	
}
//--------------------------------------------------------------------------------------------------
void PromptEdit::OnEnable(BOOL bEnable) 
{
	try
	{
		static COLORREF s_EnabledBkColor = ::GetSysColor(COLOR_WINDOW);
		if (!bEnable)
		{
			// store the background color before changing
			s_EnabledBkColor = m_crBkColor;
			SetBkColor(::GetSysColor(COLOR_3DFACE));
		}
		else
		{
			SetBkColor(s_EnabledBkColor);
		}

		CEdit::OnEnable(bEnable);		
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12593")	
}
//--------------------------------------------------------------------------------------------------
