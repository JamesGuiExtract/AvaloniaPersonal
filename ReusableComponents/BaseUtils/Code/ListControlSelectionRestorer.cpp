
#include "stdafx.h"
#include "ListControlSelectionRestorer.h"
#include "ExtractMFCUtils.h"
#include "UCLIDException.h"

//-------------------------------------------------------------------------------------------------
ListControlSelectionRestorer::ListControlSelectionRestorer(CListCtrl& rListCtrl, int iDefaultSelectionIndex)
: m_rListCtrl(rListCtrl), m_iDefaultSelectionIndex(iDefaultSelectionIndex)
{
	// Remember the position of the currently selected record, if any
	m_iSelectedIndex = getIndexOfFirstSelectedItem(m_rListCtrl);
}
//-------------------------------------------------------------------------------------------------
ListControlSelectionRestorer::~ListControlSelectionRestorer()
{
	try
	{
		// Restore the selection index if there is no selection in the list control
		// right now.
		int iTotalItems = m_rListCtrl.GetItemCount();
		if (m_rListCtrl.GetFirstSelectedItemPosition() == NULL && iTotalItems > 0)
		{
			// Our first preference is to select the index position that was selected
			// when this object was created.  If there was no selection when this object
			// was created, we want to select the default index position provided by
			// the user, if any.
			int iIndexToSelect = -1;
			if (m_iSelectedIndex != -1)
			{
				iIndexToSelect = m_iSelectedIndex;
			}
			else if (m_iDefaultSelectionIndex != -1)
			{
				iIndexToSelect = m_iDefaultSelectionIndex;
			}
			else
			{
				// If there is no default index position that is valid and no selection existed when
				// this object was created, then we don't need to do anything.
				return;
			}

			// If the index we wanted to select is no longer present, select the
			// last entry as that is the most intuitive option.
			iIndexToSelect = (iIndexToSelect < iTotalItems) ? iIndexToSelect : iTotalItems - 1;

			// Select the row at the computed index
			m_rListCtrl.SetItemState(iIndexToSelect, LVIS_SELECTED, LVIS_SELECTED);
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16952");
}
//-------------------------------------------------------------------------------------------------
