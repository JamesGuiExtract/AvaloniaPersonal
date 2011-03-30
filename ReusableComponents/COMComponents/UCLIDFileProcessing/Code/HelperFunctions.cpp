
#include "stdafx.h"
#include "HelperFunctions.h"

#include <UCLIDException.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
static const int giPADDING_BETWEEN_CONTROLS = 6;

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void resize3LabelsAndLists(CPropertyPage *pPropPage, CListCtrl& rList1, CListCtrl& rList2, 
						   CListCtrl& rList3, const UINT nLabel1ID, const UINT nLabel2ID, 
						   const UINT nLabel3ID, const int iMinHeightOfLabelArea1, 
						   const int iMinHeightOfLabelArea2, const int iMinHeightOfLabelArea3)
{
	// Verify valid property page
	ASSERT_ARGUMENT("ELI14891", pPropPage != __nullptr);

	// Verify the property sheet pointer is available
	CWnd *pParent = pPropPage->GetParent();
	ASSERT_RESOURCE_ALLOCATION("ELI14892", pParent != __nullptr);

	CRect rectPage;
	pPropPage->GetWindowRect(rectPage);

	// Get current sizes
	CRect rectList1, rectList2, rectList3,
		rectStatic1, rectStatic2, rectStatic3;

	rList1.GetWindowRect(rectList1);
	rList2.GetWindowRect(rectList2);
	rList3.GetWindowRect(rectList3);

	// Horizontal adjustment for control spacing
	rectList1.left = rectPage.left + giPADDING_BETWEEN_CONTROLS;
	rectList2.left = rectPage.left + giPADDING_BETWEEN_CONTROLS;
	rectList3.left = rectPage.left + giPADDING_BETWEEN_CONTROLS;
	rectList1.right = rectPage.right - giPADDING_BETWEEN_CONTROLS;
	rectList2.right = rectPage.right - giPADDING_BETWEEN_CONTROLS;
	rectList3.right = rectPage.right - giPADDING_BETWEEN_CONTROLS;

	// Get the label controls and their associated window coordinates
	CWnd *pWndLabel1 = pPropPage->GetDlgItem(nLabel1ID);
	ASSERT_RESOURCE_ALLOCATION("ELI14895", pWndLabel1 != __nullptr);
	pWndLabel1->GetWindowRect(&rectStatic1);

	CWnd *pWndLabel2 = pPropPage->GetDlgItem(nLabel2ID);
	ASSERT_RESOURCE_ALLOCATION("ELI14894", pWndLabel2 != __nullptr);
	pWndLabel2->GetWindowRect(&rectStatic2);

	CWnd *pWndLabel3 = pPropPage->GetDlgItem(nLabel3ID);
	ASSERT_RESOURCE_ALLOCATION("ELI14893", pWndLabel3 != __nullptr);
	pWndLabel3->GetWindowRect(&rectStatic3);

	// Determine the total height of all the label areas above the list
	// controls, keeping in mind that controls (e.g. buttons) taller than 
	// the labels may be placed alongside the labels and we need to still
	// provide the same spacing between those controls and the neighbouring
	// controls as we would do with the labels.
	int iHeightLabelArea1 = max(rectStatic1.Height(), iMinHeightOfLabelArea1);
	int iHeightLabelArea2 = max(rectStatic2.Height(), iMinHeightOfLabelArea2);
	int iHeightLabelArea3 = max(rectStatic3.Height(), iMinHeightOfLabelArea3);
	UINT iTotalHeightOfGaps = iHeightLabelArea1 + iHeightLabelArea2 + iHeightLabelArea3;

	// By default, we want the labels associated with the controls to be closer to
	// the control than other neighboring controls.  Set the padding between the
	// labels and their associated controls to half of the standard padding
	// between controls.
	const int iPADDING_BETWEEN_LABELS_AND_CONTROLS = giPADDING_BETWEEN_CONTROLS / 2;

	// Compute the total height available to be distributed to all the lists together
	int iSumOfAllListHeights = rectPage.Height() - 
		iTotalHeightOfGaps - // for the total height of all the labels (or controls) above the lists
		3 * giPADDING_BETWEEN_CONTROLS - // for the padding between the labels and the lists above them
		giPADDING_BETWEEN_CONTROLS - // for the padding below the bottom list
		3 * giPADDING_BETWEEN_CONTROLS; // for the padding between the labels and the lists below them
	
	// Compute the default list height to be a third of the total available list height
	int iDefaultListHeight = iSumOfAllListHeights / 3;

	// Because iSumOfAllListHeights may not be a number divisible by 3, add any leftover
	// pixels to the height of the bottom list
	int iBottomListHeight = iDefaultListHeight + (iSumOfAllListHeights - 3 * iDefaultListHeight) % 3;
	
	// Set the bottom list
	rectList3.bottom = rectPage.bottom - giPADDING_BETWEEN_CONTROLS;
	rectList3.top = rectList3.bottom - iBottomListHeight;

	// Set the static text for the bottom list
	LONG nLabel3Height = rectStatic3.Height();
	rectStatic3.bottom = rectList3.top - giPADDING_BETWEEN_CONTROLS;
	rectStatic3.top = rectStatic3.bottom - nLabel3Height;
	
	// Set the middle list
	rectList2.bottom = rectList3.top - 
		giPADDING_BETWEEN_CONTROLS -	// gap between top of list3 and bottom of label3
		iHeightLabelArea3 - // height of label or another control
		giPADDING_BETWEEN_CONTROLS;		// gap between top of label3 and bottom of list2
	rectList2.top = rectList2.bottom - iDefaultListHeight;

	// Set the static text for the middle list
	LONG nLabel2Height = rectStatic2.Height();
	rectStatic2.bottom = rectList2.top - giPADDING_BETWEEN_CONTROLS;
	rectStatic2.top = rectStatic2.bottom - nLabel3Height;

	// Set the top list
	rectList1.bottom = rectList2.top - 
		giPADDING_BETWEEN_CONTROLS -	// gap between top of list2 and bottom of label2
		iHeightLabelArea2 - // height of label or another control
		giPADDING_BETWEEN_CONTROLS;		// gap between top of label2 and bottom of list1
	rectList1.top = rectList1.bottom - iDefaultListHeight;

	// Set the static text for the top list
	LONG nLabel1Height = rectStatic1.Height();
	rectStatic1.bottom = rectList1.top - giPADDING_BETWEEN_CONTROLS;
	rectStatic1.top = rectStatic1.bottom - nLabel3Height;

	// Map RECTs to Client coordiantes
	pPropPage->ScreenToClient(rectList1);
	pPropPage->ScreenToClient(rectList2);
	pPropPage->ScreenToClient(rectList3);
	pPropPage->ScreenToClient(rectStatic1);
	pPropPage->ScreenToClient(rectStatic2);
	pPropPage->ScreenToClient(rectStatic3);

	// Move the controls
	rList1.MoveWindow(rectList1);
	rList2.MoveWindow(rectList2);
	rList3.MoveWindow(rectList3);
	pWndLabel1->MoveWindow(rectStatic1);
	pWndLabel2->MoveWindow(rectStatic2);
	pWndLabel3->MoveWindow(rectStatic3);
}
//-------------------------------------------------------------------------------------------------
string getMonthDayDateString()
{
	// return today's date as MM/DD
	string strTemp;
	CTime currentTime = CTime::GetCurrentTime();
	strTemp = currentTime.Format("%m/%d").operator LPCTSTR();
	return strTemp;
}
//-------------------------------------------------------------------------------------------------
bool limitListSizeIfNeeded(CListCtrl& rList, FileProcessingConfigMgr* pCfgMgr)
{
	// get the max number of items that can be in the list
	long lMaxCount = pCfgMgr->getMaxStoredRecords();

	// if this list's size does not need to be limited, just exit
	if (rList.GetItemCount() < lMaxCount)
	{
		return false;
	}

	// delete the first item from the list
	rList.DeleteItem(0);
	return true;
}
//-------------------------------------------------------------------------------------------------
