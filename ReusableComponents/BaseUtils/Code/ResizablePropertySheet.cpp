
#include "stdafx.h"
#include "ResizablePropertySheet.h"

//-------------------------------------------------------------------------------------------------
void ResizablePropertySheet::resize(const CRect& newClientRect)
{
	// get the current size
	CRect beforeRect;
	GetWindowRect(&beforeRect);
	ScreenToClient(&beforeRect);

	// Resize the sheet
	// First find relative change
	CSize sizeRelChange;
	CRect rectWindow;

	GetWindowRect(&rectWindow);
	ScreenToClient(&rectWindow);
	sizeRelChange.cx = rectWindow.Width() - newClientRect.Width();
	sizeRelChange.cy = rectWindow.Height() - newClientRect.Height();

	rectWindow.right -= sizeRelChange.cx;
	rectWindow.bottom -= sizeRelChange.cy;

	// Then resize the sheet
	MoveWindow(&newClientRect);

	// Resize the CTabCtrl
	CTabCtrl* pTab = GetTabControl();
	ASSERT(pTab);
	pTab->GetWindowRect(&rectWindow);
	ScreenToClient(&rectWindow);
	rectWindow.right -= sizeRelChange.cx;
	rectWindow.bottom -= sizeRelChange.cy;
	pTab->MoveWindow(&rectWindow);

	// Resize the active page
	CPropertyPage* pPage = GetActivePage();
	ASSERT(pPage);

	// Store page size in m_rectPage
	CRect rectPage;
	pPage->GetWindowRect(&rectPage);
	ScreenToClient(&rectPage);
	rectPage.right -= sizeRelChange.cx;
	rectPage.bottom -= sizeRelChange.cy;
	pPage->MoveWindow(&rectPage);
}
//-------------------------------------------------------------------------------------------------
