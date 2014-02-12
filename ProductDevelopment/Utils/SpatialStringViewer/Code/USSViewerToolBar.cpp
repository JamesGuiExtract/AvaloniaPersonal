#include "stdafx.h"
#include "resource.h"
#include "USSViewerToolBar.h"
#include <UCLIDException.h>
#include <cpputil.h>

static const long gnToolbarEditBoxWidth = 125;
//--------------------------------------------------------------------------------------------------
// USSViewerToolBar Class
//--------------------------------------------------------------------------------------------------
USSViewerToolBar::USSViewerToolBar(void)
	:m_editGoto(__nullptr)
{
}
//--------------------------------------------------------------------------------------------------
USSViewerToolBar::~USSViewerToolBar(void)
{
	// if the edit box was created delete it
	if (m_editGoto != __nullptr)
	{
		delete m_editGoto;
	}
}
//--------------------------------------------------------------------------------------------------
void USSViewerToolBar::createGoToPageEditBox()
{
	int iIndex = CommandToIndex(IDC_BUTTON_GOTO_PAGE);

	// Resize the button to the desired width of the Goto edit box
	TBBUTTONINFO bi;
	bi.cbSize = sizeof(TBBUTTONINFO);
	bi.dwMask = TBIF_SIZE;
	bi.cx = gnToolbarEditBoxWidth;
	GetToolBarCtrl().SetButtonInfo(IDC_BUTTON_GOTO_PAGE, &bi);

	// Get the rect for the now resized goto button
	RECT rect;
	GetItemRect(iIndex, &rect);
	rect.bottom -= 2;

	// Create the goto edit box
	m_editGoto = new CEdit();
	if (!m_editGoto->Create(WS_CHILD | WS_VISIBLE | WS_BORDER | ES_MULTILINE | ES_WANTRETURN |
		 ES_AUTOHSCROLL | ES_CENTER, rect, this, IDC_BUTTON_GOTO_PAGE))
	{
	   TRACE0("Failed to create edit control\n");
	}
	else
	{
		// Show the newly created edit box
		m_editGoto->ShowWindow(SW_SHOW);
	}
}

//--------------------------------------------------------------------------------------------------
void USSViewerToolBar::enableGoToEditBox(bool bEnable)
{
	// Enable the goto edit box
	m_editGoto->EnableWindow(bEnable);
}
//--------------------------------------------------------------------------------------------------
string USSViewerToolBar::getCurrentGoToPageText()
{
	// Get the text from the goto edit box
	CString zText;
	m_editGoto->GetWindowText(zText);
	return string(LPCTSTR(zText)); 
}
//--------------------------------------------------------------------------------------------------
void USSViewerToolBar::clearGoToPageText()
{
	// Clear the text in the goto edit box
	m_editGoto->Clear();
}
//--------------------------------------------------------------------------------------------------
void USSViewerToolBar::setCurrentGoToPageText(const string& strText)
{
	// Set the text in the goto edit box
	m_editGoto->SetWindowText(strText.c_str());
}
//--------------------------------------------------------------------------------------------------
bool USSViewerToolBar::gotoPageHasFocus()
{
	// Get the window that currently has focus
	CWnd* pCurrInputFocus = GetFocus();
	if (pCurrInputFocus != __nullptr)
	{
		// if the window handle of the window with focus is the
		// same as the goto edit box return true
		return pCurrInputFocus->m_hWnd == m_editGoto->m_hWnd;
	}
	return false;
}
//--------------------------------------------------------------------------------------------------
