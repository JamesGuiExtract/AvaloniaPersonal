
#include "stdafx.h"
#include "ExceptionListControlHelper.h"

//-------------------------------------------------------------------------------------------------
// Sort functions for each column
//-------------------------------------------------------------------------------------------------
// Serial number comparison
int CALLBACK 
SerialCompareProc(LPARAM lParam1, LPARAM lParam2, LPARAM lParamSort)
{
	// lParamSort contains a pointer to the list view control.
	// The lParam of an item is just its index.
	CListCtrl* pList = (CListCtrl*) lParamSort;

	int	iIndex1 = ((ITEMINFO *)lParam1)->iIndex;
	int	iIndex2 = ((ITEMINFO *)lParam2)->iIndex;

	CString    zItem1 = pList->GetItemText( iIndex1, SERIAL_LIST_COLUMN );
	CString    zItem2 = pList->GetItemText( iIndex2, SERIAL_LIST_COLUMN );

   // Simple string comparison
	return zItem1.Compare( zItem2 );
}
//-------------------------------------------------------------------------------------------------
// Application comparison
int CALLBACK 
ApplicationCompareProc(LPARAM lParam1, LPARAM lParam2, LPARAM lParamSort)
{
	// lParamSort contains a pointer to the list view control.
	// The lParam of an item is just its index.
	CListCtrl* pList = (CListCtrl*) lParamSort;

	int	iIndex1 = ((ITEMINFO *)lParam1)->iIndex;
	int	iIndex2 = ((ITEMINFO *)lParam2)->iIndex;

	CString    zItem1 = pList->GetItemText( iIndex1, APPLICATION_LIST_COLUMN );
	CString    zItem2 = pList->GetItemText( iIndex2, APPLICATION_LIST_COLUMN );

	// Simple string comparison
	return zItem1.Compare( zItem2 );
}
//-------------------------------------------------------------------------------------------------
// ELI Code comparison
int CALLBACK 
ELICompareProc(LPARAM lParam1, LPARAM lParam2, LPARAM lParamSort)
{
	// lParamSort contains a pointer to the list view control.
	// The lParam of an item is just its index.
	CListCtrl* pList = (CListCtrl*) lParamSort;

	int	iIndex1 = ((ITEMINFO *)lParam1)->iIndex;
	int	iIndex2 = ((ITEMINFO *)lParam2)->iIndex;

	CString    zItem1 = pList->GetItemText( iIndex1, TOP_ELI_COLUMN );
	CString    zItem2 = pList->GetItemText( iIndex2, TOP_ELI_COLUMN );

	// Simple string comparison
	return zItem1.Compare( zItem2 );
}
//-------------------------------------------------------------------------------------------------
// Exception comparison
int CALLBACK 
ExceptionCompareProc(LPARAM lParam1, LPARAM lParam2, LPARAM lParamSort)
{
	// lParamSort contains a pointer to the list view control.
	// The lParam of an item is just its index.
	CListCtrl* pList = (CListCtrl*) lParamSort;

	int	iIndex1 = ((ITEMINFO *)lParam1)->iIndex;
	int	iIndex2 = ((ITEMINFO *)lParam2)->iIndex;

	CString    zItem1 = pList->GetItemText( iIndex1, TOP_EXCEPTION_COLUMN );
	CString    zItem2 = pList->GetItemText( iIndex2, TOP_EXCEPTION_COLUMN );

	// Simple string comparison
	return zItem1.Compare( zItem2 );
}
//-------------------------------------------------------------------------------------------------
// Computer comparison
int CALLBACK 
ComputerCompareProc(LPARAM lParam1, LPARAM lParam2, LPARAM lParamSort)
{
	// lParamSort contains a pointer to the list view control.
	// The lParam of an item is just its index.
	CListCtrl* pList = (CListCtrl*) lParamSort;

	int	iIndex1 = ((ITEMINFO *)lParam1)->iIndex;
	int	iIndex2 = ((ITEMINFO *)lParam2)->iIndex;

	CString    zItem1 = pList->GetItemText( iIndex1, COMPUTER_LIST_COLUMN );
	CString    zItem2 = pList->GetItemText( iIndex2, COMPUTER_LIST_COLUMN );

	// Simple string comparison
	return zItem1.Compare( zItem2 );
}
//-------------------------------------------------------------------------------------------------
// User comparison
int CALLBACK 
UserCompareProc(LPARAM lParam1, LPARAM lParam2, LPARAM lParamSort)
{
	// lParamSort contains a pointer to the list view control.
	// The lParam of an item is just its index.
	CListCtrl* pList = (CListCtrl*) lParamSort;

	int	iIndex1 = ((ITEMINFO *)lParam1)->iIndex;
	int	iIndex2 = ((ITEMINFO *)lParam2)->iIndex;

	CString    zItem1 = pList->GetItemText( iIndex1, USER_LIST_COLUMN );
	CString    zItem2 = pList->GetItemText( iIndex2, USER_LIST_COLUMN );

	// Simple string comparison
	return zItem1.Compare( zItem2 );
}
//-------------------------------------------------------------------------------------------------
// Process ID comparison
int CALLBACK 
PidCompareProc(LPARAM lParam1, LPARAM lParam2, LPARAM lParamSort)
{
	// lParamSort contains a pointer to the list view control.
	// The lParam of an item is just its index.
	CListCtrl* pList = (CListCtrl*) lParamSort;

	int	iIndex1 = ((ITEMINFO *)lParam1)->iIndex;
	int	iIndex2 = ((ITEMINFO *)lParam2)->iIndex;

	CString    zItem1 = pList->GetItemText( iIndex1, PID_LIST_COLUMN );
	CString    zItem2 = pList->GetItemText( iIndex2, PID_LIST_COLUMN );

	// Convert strings to long integers
	long lItem1 = atol( zItem1.operator LPCTSTR() );
	long lItem2 = atol( zItem2.operator LPCTSTR() );

	// Comparison
	if (lItem1 < lItem2)
	{
		// First item should come before second item
		return -1;
	}
	else if (lItem1 > lItem2)
	{
		// Second item should come before first item
		return 1;
	}
	else
	{
		// Items are equal
		return 0;
	}
}
//-------------------------------------------------------------------------------------------------
// Time comparison
int CALLBACK 
TimeCompareProc(LPARAM lParam1, LPARAM lParam2, LPARAM lParamSort)
{
	// lParamSort contains a pointer to the list view control.
	// The lParam of an item is just its index.
	CListCtrl* pList = (CListCtrl*) lParamSort;

	// Retrieve time strings from ItemData
	long	lItem1 = ((ITEMINFO *)lParam1)->ulTime;
	long	lItem2 = ((ITEMINFO *)lParam2)->ulTime;

	// Comparison
	if (lItem1 < lItem2)
	{
		// First item should come before second item
		return -1;
	}
	else if (lItem1 > lItem2)
	{
		// Second item should come before first item
		return 1;
	}
	else
	{
		// Items are equal
		return 0;
	}
}
//-------------------------------------------------------------------------------------------------
