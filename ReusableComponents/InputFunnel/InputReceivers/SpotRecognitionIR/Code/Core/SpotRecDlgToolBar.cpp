//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	SpotRecDlgToolBar.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan
//
//==================================================================================================

#include "stdafx.h"
#include "resource.h"
#include "SpotRecognitionDlg.h"

#include "SpotRecDlgToolBar.h"
#include <UCLIDException.h>
#include <cpputil.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

using namespace std;

static const long gnToolbarEditBoxWidth = 75;

//--------------------------------------------------------------------------------------------------
SpotRecDlgToolBar::SpotRecDlgToolBar()
{
	m_wndSnap = NULL;
}
//--------------------------------------------------------------------------------------------------
SpotRecDlgToolBar::~SpotRecDlgToolBar()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		if (m_wndSnap)
		{
			delete m_wndSnap;
			m_wndSnap = NULL;
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16500");
}
//--------------------------------------------------------------------------------------------------
void SpotRecDlgToolBar::createGoToPageEditBox()
{
	int iIndex = CommandToIndex(IDC_BTN_GoToPage);
	SetButtonInfo(iIndex, IDC_BTN_GoToPage, TBBS_BUTTON, 20);

	TBBUTTONINFO bi;
	bi.cbSize = sizeof(TBBUTTONINFO);
	bi.dwMask = TBIF_SIZE;
	bi.cx = gnToolbarEditBoxWidth;
	GetToolBarCtrl().SetButtonInfo(IDC_BTN_GoToPage, &bi);

	RECT rect;
	GetItemRect(iIndex, &rect);
	rect.top += 1;
	rect.bottom -= 1;
	rect.left += 26;
	rect.right += 26;

	m_wndSnap = new CEdit();
	if (!m_wndSnap->Create(WS_CHILD | WS_VISIBLE | WS_BORDER | ES_MULTILINE | ES_WANTRETURN |
		 ES_AUTOHSCROLL | ES_CENTER, rect, this, IDC_BTN_GoToPage))
	{
	   TRACE0("Failed to create edit control\n");
	}
	else
	{
		m_wndSnap->ShowWindow(SW_SHOW);
	}
}
//--------------------------------------------------------------------------------------------------
void SpotRecDlgToolBar::enableGoToEditBox(bool bEnable)
{
	m_wndSnap->EnableWindow(bEnable);
}
//--------------------------------------------------------------------------------------------------
string SpotRecDlgToolBar::getCurrentGoToPageText()
{
	CString zText;
	m_wndSnap->GetWindowText(zText);
	return string(LPCTSTR(zText)); 
}
//--------------------------------------------------------------------------------------------------
void SpotRecDlgToolBar::clearGoToPageText()
{
	m_wndSnap->Clear();
}
//--------------------------------------------------------------------------------------------------
void SpotRecDlgToolBar::setCurrentGoToPageText(const string& strText)
{
	m_wndSnap->SetWindowText(strText.c_str());
}
//--------------------------------------------------------------------------------------------------
void SpotRecDlgToolBar::updateGotoEditBoxPos()
{
	int iIndex = CommandToIndex(IDC_BTN_GoToPage);
	RECT rect;
	GetItemRect(iIndex, &rect);
	m_wndSnap->MoveWindow(&rect);
}
//--------------------------------------------------------------------------------------------------
void SpotRecDlgToolBar::showToolbarCtrl(ESRIRToolbarCtrl eCtrl, bool bShow)
{
	// Get the button id
	int iButtonId = getButtonId(eCtrl);

	// Handle the special case of the go to page edit box
	if (eCtrl == kEditPageNum)
	{
		m_wndSnap->ShowWindow(bShow);
	}

	// Show/hide the button
	CToolBarCtrl& toolbar = GetToolBarCtrl();
	toolbar.HideButton(iButtonId, asMFCBool(!bShow));

	// Get the index of the button that was shown/hidden
	int iButtonIndex = CommandToIndex(iButtonId);

	// If this was the last button to be hidden or the first button to be shown in a group, 
	// then hide or show the adjacent separators. [LegacyRCAndUtils #5184]
	showAdjacentSeparators(iButtonIndex, bShow);

	// Update the go to page edit box position
	updateGotoEditBoxPos();
}
//--------------------------------------------------------------------------------------------------
void SpotRecDlgToolBar::showAdjacentSeparators(int iButtonIndex, bool bShow)
{
	// Get the toolbar
	CToolBarCtrl& toolbar = GetToolBarCtrl();

	// If there are no other visible buttons in this group, update the separator. 
	TBBUTTONINFO buttonInfo = {0};
	buttonInfo.cbSize = sizeof(TBBUTTONINFO);
	buttonInfo.dwMask = TBIF_BYINDEX | TBIF_STATE | TBIF_STYLE;
	for (int i = iButtonIndex - 1; i >= 0; i--)
	{
		// Get the state and style for this button
		toolbar.GetButtonInfo(i, &buttonInfo);
		
		// If this is a separator we are done searching this group
		if ((buttonInfo.fsStyle & TBBS_SEPARATOR) > 0)
		{
			break;
		}

		// If this button is visible, there is no need to update the separator
		if ((buttonInfo.fsState & TBSTATE_HIDDEN) > 0)
		{
			return;
		}
	}
	int iButtonCount = toolbar.GetButtonCount();
	for (int i = iButtonIndex + 1; i < iButtonCount; i++)
	{
		// Get the state and style for this button
		toolbar.GetButtonInfo(i, &buttonInfo);
		
		// Check if this is a separator
		if ((buttonInfo.fsStyle & TBBS_SEPARATOR) > 0)
		{
			// Show/hide the separator
			buttonInfo.dwMask = TBIF_BYINDEX | TBIF_STATE;
			if (bShow)
			{
				buttonInfo.fsState &= ~TBSTATE_HIDDEN;
			}
			else
			{
				buttonInfo.fsState |= TBSTATE_HIDDEN;
			}
			toolbar.SetButtonInfo(i, &buttonInfo);

			// For some unknown reason hiding separators makes all the buttons larger (?!)
			// Explicitly set the button sizes back to normal.
			SIZE sizeButton = {23, 21};
			SIZE sizeImage = {16, 15};
			SetSizes(sizeButton, sizeImage);
			
			return;
		}

		// If this button is visible, there is no need to update the separator
		if ((buttonInfo.fsState & TBSTATE_HIDDEN) > 0)
		{
			return;
		}
	}
}
//--------------------------------------------------------------------------------------------------
int SpotRecDlgToolBar::getButtonId(ESRIRToolbarCtrl eCtrl)
{
	switch(eCtrl)
	{
	case kBtnOpenImage:
		return IDC_BTN_OpenImage;

	case kBtnSave:
		return IDC_BTN_Save;

	case kBtnPrint:
		return IDC_BTN_Print;

	case kBtnZoomWindow:
		return IDC_BTN_ZoomWindow;

	case kBtnZoomIn:
		return IDC_BTN_ZoomIn;

	case kBtnZoomOut:
		return IDC_BTN_ZoomOut;

	case kBtnZoomPrevious:
		return IDC_BTN_ZoomPrev;

	case kBtnZoomNext:
		return IDC_BTN_ZoomNext;

	case kBtnFitPage:
		return IDC_BTN_FitPage;

	case kBtnFitWidth:
		return IDC_BTN_FitWidth;

	case kBtnPan:
		return IDC_BTN_Pan;

	case kBtnSelectText:
		return IDC_BTN_SelectText;

	case kBtnSelectHighlight:
		return IDC_BTN_SELECT_ENTITIES;

	case kBtnSetHighlightHeight:
		return IDC_BTN_SetHighlightHeight;

	case kBtnEditZoneText:
		return IDC_BTN_EditZoneText;

	case kBtnDeleteEntities:
		return IDC_BTN_DeleteEntities;

	case kBtnPTH:
		return IDC_BTN_RecognizeTextAndProcess;

	case kBtnOpenSubImgInWindow:
		return IDC_BTN_OPENSUBIMAGE;

	case kBtnRotateCounterClockwise:
		return IDC_BTN_RotateLeft;

	case kBtnRotateClockwise:
		return IDC_BTN_RotateRight;

	case kBtnFirstPage:
		return IDC_BTN_FirstPage;

	case kBtnLastPage:
		return IDC_BTN_LastPage;

	case kBtnPrevPage:
		return IDC_BTN_PreviousPage;

	case kBtnNextPage:
		return IDC_BTN_NextPage;

	case kEditPageNum:
		return IDC_BTN_GoToPage;
	}

	UCLIDException ue("ELI11289", "Invalid toolbar control.");
	ue.addDebugInfo("Toolbar number", eCtrl);
	throw ue;
}
//--------------------------------------------------------------------------------------------------