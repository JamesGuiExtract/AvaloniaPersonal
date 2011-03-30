
#include "stdafx.h"
#include "ExtractMFCUtils.h"

#include "cpputil.h"
#include "UCLIDException.h"

//-------------------------------------------------------------------------------------------------
bool makeWindowTransparent(CWnd *pWnd, bool bTransparent, BYTE byteTransparency)
{
	if (pWnd == NULL)
	{
		throw UCLIDException("ELI03987", "Window handle cannot be NULL!");
	}

	return makeWindowTransparent(pWnd->m_hWnd, bTransparent, byteTransparency);
}
//-------------------------------------------------------------------------------------------------
CTime getFileModificationTimeStamp(const string& strFileName)
{
	CFileFind	ffFileFind;
	FILETIME	ftFileTime;
	if (ffFileFind.FindFile(strFileName.c_str()))
	{
		// Find the file
		ffFileFind.FindNextFile();
		
		if (ffFileFind.GetLastWriteTime(&ftFileTime) == 0)
		{
			// Error
			UCLIDException ue("ELI07351", "Failed to get file timestamp on last modification.");
			ue.addDebugInfo("FileName", strFileName);
			throw ue;
		}
	}
	else
	{
		// File was not found
		UCLIDException ue ("ELI13592", "File was not found." );
		ue.addDebugInfo("FileName", strFileName );
		throw ue;
	}
	
	return CTime(ftFileTime);
}
//-------------------------------------------------------------------------------------------------
bool recreateListBox(CListBox* pList, LPVOID lpParam/*=NULL*/)
{
	// Validate ListBox settings
	if (pList == NULL)
	{
		return false;
	}

	if (pList->GetSafeHwnd() == NULL)
	{
		return false;
	}

	CWnd* pParent = pList->GetParent();
	if (pParent == NULL)
	{
		return false;
	}

	// Get current styles and attributes
	DWORD dwStyle = pList->GetStyle();
	DWORD dwStyleEx = pList->GetExStyle();
	CRect rc;
	pList->GetWindowRect(&rc);
	pParent->ScreenToClient(&rc);	// map to client co-ords
	UINT nID = pList->GetDlgCtrlID();
	CFont* pFont = pList->GetFont();
	CWnd* pWndAfter = pList->GetNextWindow(GW_HWNDPREV);

	// Create the new list box and copy the old list box items 
	// into a new listbox along with each item's data, and selection state
	CListBox listNew;
	if (!listNew.CreateEx(dwStyleEx, _T("LISTBOX"), _T(""), dwStyle, 
		rc, pParent, nID, lpParam))
	{
		return false;
	}

	// Apply previous settings to the new CListBox
	listNew.SetFont(pFont);
	int nNumItems = pList->GetCount();
	BOOL bMultiSel = (dwStyle & LBS_MULTIPLESEL || dwStyle & LBS_EXTENDEDSEL);
	for (int n = 0; n < nNumItems; n++)
	{
		CString sText;
		pList->GetText(n, sText);
		int nNewIndex = listNew.AddString(sText);
		listNew.SetItemData(nNewIndex, pList->GetItemData(n));
		if (bMultiSel && pList->GetSel(n))
		{
			listNew.SetSel(nNewIndex);
		}
	}

	if (!bMultiSel && nNumItems)
	{
		int nCurSel = pList->GetCurSel();
		if (nCurSel != -1)
		{
			CString sSelText;
			// Get the selection in the old list
			pList->GetText(nCurSel, sSelText);
			// Now find and select it in the new list
			listNew.SetCurSel(listNew.FindStringExact(-1, sSelText));
		}
	}

	// Destroy the existing window, then attach the new one
	pList->DestroyWindow();
	HWND hwnd = listNew.Detach();
	pList->Attach(hwnd);

	// Position correctly in z-order
	pList->SetWindowPos( pWndAfter == NULL ? &CWnd::wndBottom : pWndAfter, 0, 0, 0, 0, 
		SWP_NOMOVE | SWP_NOSIZE);

	return true;
}
//-------------------------------------------------------------------------------------------------
void setItemTextIfDifferent(CListCtrl &rListCtrl, int iItem, int iSubItem, const string& strText)
{
	// Get the current value in the particular cell of the list box
	CString zCurrentValue = rListCtrl.GetItemText(iItem, iSubItem);
	CString zNewValue = strText.c_str();

	// To prevent "flickering" in the list control, update the text, only if it is different
	if (zCurrentValue != zNewValue)
	{
		rListCtrl.SetItemText(iItem, iSubItem, strText.c_str());
	}
}
//-------------------------------------------------------------------------------------------------
int getIndexOfFirstSelectedItem(CListCtrl& rListCtrl)
{
	// Determine if there is a selection in the list control.
	POSITION pos = rListCtrl.GetFirstSelectedItemPosition();

	if (pos != __nullptr)
	{
		// There is at least one item selected in the list control.
		// Return the index of the first selected item.
		return rListCtrl.GetNextSelectedItem(pos);
	}
	else
	{
		// There is no item currently selected in the list control.
		return -1;
	}
}
//-------------------------------------------------------------------------------------------------
void loadComboBoxFromVector(CComboBox& rComboBox, std::vector<std::string>& vecItems)
{
	// Reset the combo box list
	rComboBox.ResetContent();
	for each ( string s in vecItems)
	{
		// Add the string to the combo list box
		rComboBox.AddString(s.c_str());
	}
}
//-------------------------------------------------------------------------------------------------
vector<int> getIndexOfAllSelectedItems(CListCtrl& rListCtrl)
{
	vector<int> vecSelectedItems;

	// Determine if there is a selection in the list control.
	POSITION pos = rListCtrl.GetFirstSelectedItemPosition();
	while (pos != __nullptr)
	{
		vecSelectedItems.push_back(rListCtrl.GetNextSelectedItem(pos));
	}

	return vecSelectedItems;
}
//-------------------------------------------------------------------------------------------------
