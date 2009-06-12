
#pragma once

#include "BaseUtils.h"

//---------------------------------------------------------------------------------------------
// ListControlSelectionRestorer class
//
// PURPOSE:	To remember the currently selected index in a particular list control, and restore
//			the selection to that index (or some other specified default index) when this 
//			object goes out of scope.
// NOTE:	This object does some "useful" work only in the destructor.  Also, the code in
//			the destructor does nothing if the list control has a selected item when the 
//			destructor code is being executed (i.e. the destructor tries to restore a selection
//			index only if there is no selection in the list control).
//---------------------------------------------------------------------------------------------
class EXPORT_BaseUtils ListControlSelectionRestorer
{
public:
	//---------------------------------------------------------------------------------------------
	// REQUIRE: rListCtrl is a single-select list control with or without an item currently
	//			selected.
	// PROMISE: If rListCtrl has an item selected, the selection index will be remembered and 
	//			restored when this object goes out of scope.  If that selection index is not a 
	//			valid index when this object goes out of scope and the list control has at least
	//			one item, the last item in the list will be selected.
	//			If rListCtrl does not have an item selected, and iDefaultSelectionIndex != -1, then
	//			the selection index will be restored to iDefaultSelectionIndex when this object
	//			goes out of scope. If iDefaultSelectionIndex is not a valid index when this object
	//			goes out of scope and the list control has at least one item, the last item in the
	//			list will be selected.
	//			If there is no currently selected item, and if iDefaultSelectionIndex == -1, then
	//			the destructor does nothing.
	ListControlSelectionRestorer(CListCtrl& rListCtrl, int iDefaultSelectionIndex = -1);
	//---------------------------------------------------------------------------------------------
	// PURPOSE:	The destructor will restore the selection index in the list control passed to the
	//			constructor, according to the logic described in the constructor.
	// NOTE:	The destructor does not do anything if the list control passed to the constructor
	//			has an item selected when this object goes out of scope.  The automatic
	//			selection-index-restoring happens only if there is no item selected in the list
	//			control when this object goes out of scope.
	~ListControlSelectionRestorer();
	//---------------------------------------------------------------------------------------------

private:
	// The list control whose selected index will be restored when this object goes out of scope
	CListCtrl& m_rListCtrl;

	// The index to restore the selection to, if the list control does not have a selection
	// when this object goes out of scope.  If this is set to -1, that means there was no 
	// selection in the list control when this object was created.
	int m_iSelectedIndex;

	// The index to restore the selection to, if this list control did not have any selection
	// when this object was created, and did not have any selection when this object goes out of
	// scope.
	int m_iDefaultSelectionIndex;
};
//-------------------------------------------------------------------------------------------------
