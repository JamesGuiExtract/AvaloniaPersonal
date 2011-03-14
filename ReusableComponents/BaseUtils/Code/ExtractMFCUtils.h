
#pragma once

#include "BaseUtils.h"

#include <string>
#include <vector>

#include <afxcmn.h>

//-------------------------------------------------------------------------------------------------
// PROMISE: See documentation for makeWindowTransparent(HWND)
EXPORT_BaseUtils bool makeWindowTransparent(CWnd *pWnd, 
											bool bTransparent,
											BYTE byteTransparency = 64);
//-------------------------------------------------------------------------------------------------
// return the timestamp for last modification of the specified file
EXPORT_BaseUtils CTime getFileModificationTimeStamp(const std::string& strFileName);
//-------------------------------------------------------------------------------------------------
// PROMISE: This will create a new CListBox object with data and settings from pList.  The original 
//          pList is removed and replaced with the new.  This function must be called after 
//          calling pList->ModifyStyle() or pList->ModifyStyleEx().
//          Returns true if recreation succeeds, otherwise false.
// NOTE:    From http://www.codeproject.com/combobox/recreatelistbox.asp#xxxx
EXPORT_BaseUtils bool recreateListBox(CListBox* pList, 
	LPVOID lpParam = NULL);
//-------------------------------------------------------------------------------------------------
// PURPOSE:	To update items in the list control in an optimal way to prevent flickering.
// PROMISE: To update the cell of the control identified by iItem and iSubItem with the specified
//			text, if and only if the current text in that cell is different than strText.
EXPORT_BaseUtils void setItemTextIfDifferent(CListCtrl &rListCtrl, int iItem, int iSubItem, 
	const std::string& strText);
//-------------------------------------------------------------------------------------------------
// PURPOSE: To determine the index of the first selected item in a list control
// PROMISE:	If rListCtrl has at least one item selected, then:
//			(a) If rListCtrl is a single-select list control, the index of the selected item will
//				be returned.
//			(b) If rListCtrl is a multi-select list control, the index of the first selected item
//				will be returned.
//			If there is no selection in rListCtrl, then -1 will be returned.
// NOTE:	The CListCtrl's GetSelectionMark() method can return the first selected index, but
//			it only works for multi-select list controls, and always returns -1 for single-select
//			list controls.  This function will return the first selected index for both single and
//			multi select list controls, and will return -1 if there is no current selection.
EXPORT_BaseUtils int getIndexOfFirstSelectedItem(CListCtrl& rListCtrl);
//-------------------------------------------------------------------------------------------------
// PURPOSE: To load the vector of strings vecItems into  rComboBox list.
// NOTE:	The combo box list is reset before the items in the vector are added.
EXPORT_BaseUtils void loadComboBoxFromVector(CComboBox& rComboBox, std::vector<std::string>& vecItems);
//-------------------------------------------------------------------------------------------------
// PURPOSE: To determine the index of all selected items in a list control
// PROMISE:	If rListCtrl has at least one item selected, then:
//			(a) If rListCtrl is a single-select list control, the index of the selected item will
//				be returned.
//			(b) If rListCtrl is a multi-select list control, the index of all selected items
//				will be returned.
//			If there is no selection in rListCtrl, then an empty vector will be returned
EXPORT_BaseUtils std::vector<int> getIndexOfAllSelectedItems(CListCtrl& rListCtrl);
//-------------------------------------------------------------------------------------------------
