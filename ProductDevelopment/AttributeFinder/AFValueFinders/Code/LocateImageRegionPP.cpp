// LocateImageRegionPP.cpp : Implementation of CLocateImageRegionPP
#include "stdafx.h"
#include "AFValueFinders.h"
#include "LocateImageRegionPP.h"
#include "..\..\AFUtils\Code\Helper.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <comutils.h>
#include <cpputil.h>
#include <CommentedTextFileReader.h>
#include <ComponentLicenseIDs.h>

const int NUM_OF_CHARS = 4096;

const int giCMB_MATCH_PER_DOC_FIRST = 0;
const int giCMB_MATCH_PER_DOC_ALL   = 1;
const int giCMB_MATCH_PER_DOC_SIZE = 2;

const CString gpszCMB_MATCH_PER_DOC[] =
{
	"First",
	"All"
};

const CString gpszTXT_REGION_ON_INCLUDE = "Include region on";
const CString gpszTXT_REGION_ON_EXCLUDE = "Exclude region on";

//-------------------------------------------------------------------------------------------------
// CLocateImageRegionPP
//-------------------------------------------------------------------------------------------------
CLocateImageRegionPP::CLocateImageRegionPP()
: m_eCurrentSelectedClueList(kNoIndex),
  m_bCurrentListChanged(false)
{
	try
	{
		// Check licensing
		validateLicense();

		m_dwTitleID = IDS_TITLERegExprRulePP;
		m_dwHelpFileID = IDS_HELPFILERegExprRulePP;
		m_dwDocStringID = IDS_DOCSTRINGRegExprRulePP;

		// Create an IMiscUtilsPtr object
		IMiscUtilsPtr ipMiscUtils(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI14590", ipMiscUtils != __nullptr );

		// Get the file header string and its length from IMiscUtilsPtr object
		m_strFileHeader = ipMiscUtils->GetFileHeader();
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI07710")
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLocateImageRegionPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("CLocateImageRegionPP::Apply\n"));
		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_AFVALUEFINDERSLib::ILocateImageRegionPtr ipLocateRegion(m_ppUnk[i]);
			if (ipLocateRegion)
			{
				UCLID_AFVALUEFINDERSLib::EFindType eFindType = 
					(UCLID_AFVALUEFINDERSLib::EFindType) m_cmbFindType.GetCurSel();
				
				CComBSTR bstrImageRegionText;
				m_editImageRegionText.GetWindowText(&bstrImageRegionText);

				// If find type is "Image Region" ensure that "Image Region Text" is specified
				if (eFindType == kImageRegion && bstrImageRegionText.Length() == 0)
				{
					MessageBox("Image region text must be specified.", "Invalid setting");
					return S_FALSE;
				}
					
				// If current list has changed, save the changes
				if (m_bCurrentListChanged)
				{
					storeCurrentListSettings();
				}
				
				// Remove any empty clue list entry from the map
				cleanupClueLists();

				// Validate each region boundary
				RegionBoundary boundary[4];
				for (int i = 1; i <= 4; i++)
				{
					if (!tryGetRegionBoundary((EBoundary) i, boundary[i-1]))
					{
						m_ctrlBoundary[i-1].m_cmbSide.SetFocus();
						return S_FALSE;
					}

					if (i % 2 == 0)
					{
						if (!areValidOpposingRegions(boundary[i-2], boundary[i-1]))
						{
							m_ctrlBoundary[i-2].m_cmbSide.SetFocus();
							return S_FALSE;
						}
					}
				}

				// Store the find type and image region text
				ipLocateRegion->FindType = eFindType;
				ipLocateRegion->ImageRegionText = bstrImageRegionText.Detach();

				// Store the region boundaries
				for (int i = 0; i < 4; i++)
				{
					ipLocateRegion->SetRegionBoundary(
						(UCLID_AFVALUEFINDERSLib::EBoundary) boundary[i].m_eRegion, 
						(UCLID_AFVALUEFINDERSLib::EBoundary) boundary[i].m_eSide,
						(UCLID_AFVALUEFINDERSLib::EBoundaryCondition) boundary[i].m_eCondition, 
						(UCLID_AFVALUEFINDERSLib::EExpandDirection) boundary[i].m_eDirection, 
						boundary[i].m_dExpand);
				}

				if (!storeClueLists(ipLocateRegion))
				{
					return S_FALSE;
				}

				// inside/outside boundaries
				int nIndex = m_cmbInsideOutside.GetCurSel();
				ipLocateRegion->DataInsideBoundaries = asVariantBool(nIndex == 0);

				// include/exclude intersecting entities
				nIndex = m_cmbIncludeExclude.GetCurSel();
				ipLocateRegion->IncludeIntersectingEntities = asVariantBool(nIndex == 0);

				// intersecting entity type
				ESpatialEntity eEntity = (ESpatialEntity)(m_cmbIntersectingEntities.GetCurSel() + 1);
				ipLocateRegion->IntersectingEntityType = eEntity;

				// find multiple/first matching page(s) per document
				ipLocateRegion->MatchMultiplePagesPerDocument = asVariantBool( 
					m_cmbMatchMultiplePagesPerDocument.GetCurSel()==giCMB_MATCH_PER_DOC_ALL );
			}
		}

		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07776")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CLocateImageRegionPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_AFVALUEFINDERSLib::ILocateImageRegionPtr ipLocateRegion = m_ppUnk[0];

		if (ipLocateRegion)
		{
			// create tooltip object
			m_infoTip.Create(CWnd::FromHandle(m_hWnd));
			// set no delay.
			m_infoTip.SetShowDelay(0);

			setupControls();

			m_editImageRegionText.SetWindowText(asString(ipLocateRegion->ImageRegionText).c_str());

			if(ipLocateRegion->FindType == kText)
			{
				m_cmbFindType.SetCurSel((int)kText);
				m_editImageRegionText.EnableWindow(false);
			}
			else
			{
				m_cmbFindType.SetCurSel((int)kImageRegion);
				m_cmbInsideOutside.SetCurSel(0);
				m_txtIncludeExcludeRegionOn.SetWindowText(gpszTXT_REGION_ON_INCLUDE);
				m_cmbInsideOutside.EnableWindow(false);
			}

			// get settings form ipLocateRegion object
			bool bInside = asCppBool(ipLocateRegion->DataInsideBoundaries);
			bool bInclude = asCppBool(ipLocateRegion->IncludeIntersectingEntities);
			ESpatialEntity eIntersectingEntities = ipLocateRegion->IntersectingEntityType;

			// populate first 3 combo boxes, which have something to do with
			// inside/outside boundaries, include/exclude entities
			int nSelectIndex = 0;

			if (!bInside)
			{
				nSelectIndex = 1;
				m_txtIncludeExcludeRegionOn.SetWindowText("Exclude region on");
			}

			m_cmbInsideOutside.SetCurSel(nSelectIndex);
			m_txtIncludeExcludeRegionOn.SetWindowText( 
				(nSelectIndex == 0 ? gpszTXT_REGION_ON_INCLUDE : gpszTXT_REGION_ON_EXCLUDE));

			nSelectIndex = 0;

			if (!bInclude)
			{
				nSelectIndex = 1;
			}
			m_cmbIncludeExclude.SetCurSel(nSelectIndex);

			m_cmbIntersectingEntities.SetCurSel((long)eIntersectingEntities - 1);

			// populate MatchMultiplePagePerDocument combo box
			m_cmbMatchMultiplePagesPerDocument.SetCurSel( 
				(asCppBool(ipLocateRegion->MatchMultiplePagesPerDocument) ? 
					giCMB_MATCH_PER_DOC_ALL : giCMB_MATCH_PER_DOC_FIRST));

			// initialize boundaries
			UCLID_AFVALUEFINDERSLib::EBoundary eRegionBound, eSide;
			UCLID_AFVALUEFINDERSLib::EBoundaryCondition eCondition;
			UCLID_AFVALUEFINDERSLib::EExpandDirection eExpandDirection;
			double dExpandNumber;
			long n;
			for (n = (long)kTop; n <= (long)kRight; n++)
			{
				eRegionBound = (UCLID_AFVALUEFINDERSLib::EBoundary)n;
				ipLocateRegion->GetRegionBoundary(eRegionBound, &eSide, 
					&eCondition, &eExpandDirection, &dExpandNumber);
				initBoundaries((EBoundary)eRegionBound, (EBoundary)eSide, 
							   (EBoundaryCondition)eCondition, 
							   (EExpandDirection)eExpandDirection, dExpandNumber);
			}

			// initialize clue lists
			IVariantVectorPtr ipClues(NULL);
			VARIANT_BOOL bCaseSensitive, bAsRegExpr, bRestrictSearch;
			for (n = (long)kList1; n <= (long)kList4; n++)
			{
				ipLocateRegion->GetClueList((UCLID_AFVALUEFINDERSLib::EClueListIndex)n,
									&ipClues, &bCaseSensitive, &bAsRegExpr, &bRestrictSearch);
				// if only the clue exists
				if (ipClues)
				{
					initClueList((EClueListIndex)n, ipClues, asCppBool(bCaseSensitive),
						asCppBool(bAsRegExpr), asCppBool(bRestrictSearch) );
				}
			}

			// alway default selection to clue list 1
			selectClueList(kList1);
		}
		
		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07777");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CLocateImageRegionPP::OnItemchangedList(int idCtrl, LPNMHDR pnmh, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		updateListButtons();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07806");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CLocateImageRegionPP::OnClickedRadioList1(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// before changing the display to reflect the selected clue list,
		// let's store the current clue list and its associated flags
		if (m_bCurrentListChanged)
		{
			storeCurrentListSettings();
		}
		
		selectClueList(kList1);

		m_bCurrentListChanged = false;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07807");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CLocateImageRegionPP::OnClickedRadioList2(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// before changing the display to reflect the selected clue list,
		// let's store the current clue list and its associated flags
		if (m_bCurrentListChanged)
		{
			storeCurrentListSettings();
		}

		selectClueList(kList2);

		m_bCurrentListChanged = false;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07808");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CLocateImageRegionPP::OnClickedRadioList3(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// before changing the display to reflect the selected clue list,
		// let's store the current clue list and its associated flags
		if (m_bCurrentListChanged)
		{
			storeCurrentListSettings();
		}

		selectClueList(kList3);

		m_bCurrentListChanged = false;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07809");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CLocateImageRegionPP::OnClickedRadioList4(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// before changing the display to reflect the selected clue list,
		// let's store the current clue list and its associated flags
		if (m_bCurrentListChanged)
		{
			storeCurrentListSettings();
		}

		selectClueList(kList4);

		m_bCurrentListChanged = false;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07810");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CLocateImageRegionPP::OnClickedBtnAdd(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		CString zEnt;
		bool bSuccess = promptForValue(zEnt, m_listClues, m_strFileHeader.c_str(), -1);

		if (bSuccess)
		{
			int nTotal = m_listClues.GetItemCount();
			
			int nIndex = m_listClues.InsertItem(nTotal, zEnt);
			for (int n = 0; n <= nTotal; n++)
			{
				int nState = (n == nIndex) ? LVIS_SELECTED : 0;
				
				m_listClues.SetItemState(n, nState, LVIS_SELECTED);
			}

			// if one or more clues are added, update icon
			if (m_listClues.GetItemCount() == 1)
			{
				changeIcon(m_eCurrentSelectedClueList, false);
			}
			
			SetDirty(TRUE);

			m_bCurrentListChanged = true;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07811");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CLocateImageRegionPP::OnClickedBtnModify(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		int nSelectedItemIndex = m_listClues.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
		if (nSelectedItemIndex >= 0)
		{			
			char pszValue[NUM_OF_CHARS];
			// get selected text
			m_listClues.GetItemText(nSelectedItemIndex, 0, pszValue, NUM_OF_CHARS);
			CString zOld(pszValue);
			CString zEnt(pszValue);
			int nExistingItemIndex = LB_ERR;
			
			bool bSuccess = promptForValue(zEnt, m_listClues, m_strFileHeader.c_str(), nSelectedItemIndex);
			if (bSuccess)
			{
				// Get the count of items inside the list box
				int iCount = m_listClues.GetItemCount();
				// If there is no items inside list box, insert the item as the first row
				if (iCount == 0)
				{
					m_listClues.InsertItem(0, zEnt);
				}
				else
				{
					m_listClues.SetItemText(nSelectedItemIndex, 0, zEnt);
				}

				SetDirty(TRUE);

				m_bCurrentListChanged = true;
			}

			// Call updateListButtons() to update the related buttons
			updateListButtons();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07812");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CLocateImageRegionPP::OnClickedBtnRemove(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		int nItem = m_listClues.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
		
		if (nItem != -1)
		{
			int nRes = MessageBox("Delete selected item(s)?", "Confirm Delete", MB_YESNO);
			if (nRes == IDYES)
			{
				// remove selected items
				
				int nFirstItem = nItem;
				
				while(nItem != -1)
				{
					// remove from the UI listbox
					m_listClues.DeleteItem(nItem);
					
					nItem = m_listClues.GetNextItem(nItem - 1, ((nItem == 0) ? LVNI_ALL : LVNI_BELOW) | LVNI_SELECTED);
				}
				
				// if there's more item(s) below last deleted item, then set 
				// selection on the next item
				int nTotalNumOfItems = m_listClues.GetItemCount();
				if (nFirstItem < nTotalNumOfItems)
				{
					m_listClues.SetItemState(nFirstItem, LVIS_SELECTED, LVIS_SELECTED);
				}
				else if (nTotalNumOfItems > 0)
				{
					// select the last item
					m_listClues.SetItemState(nTotalNumOfItems - 1, LVIS_SELECTED, LVIS_SELECTED);
				}
				
				// if clue list is empty, update icon
				if (m_listClues.GetItemCount() == 0)
				{
					changeIcon(m_eCurrentSelectedClueList, true);
				}

				SetDirty(TRUE);
				m_bCurrentListChanged = true;

				// Call updateListButtons() to update the related buttons
				updateListButtons();
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07813");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CLocateImageRegionPP::OnClickedBtnDown(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// get current selected item index
		int nSelectedItemIndex = m_listClues.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
		if (nSelectedItemIndex >= 0)
		{
			// get the index of the item right below currently selected item
			int nBelowIndex = nSelectedItemIndex+1;

			char pszValue[NUM_OF_CHARS];
			// get selected item text from list
			m_listClues.GetItemText(nSelectedItemIndex, 0, pszValue, NUM_OF_CHARS);
			CString zEnt(pszValue);

			// then remove the selected item
			m_listClues.DeleteItem(nSelectedItemIndex);

			// now insert the item right before the item that was above
			int nActualIndex = m_listClues.InsertItem(nBelowIndex, zEnt);

			// keep this item selected
			m_listClues.SetItemState(nActualIndex, LVIS_SELECTED, LVIS_SELECTED);

			m_bCurrentListChanged = true;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07814");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CLocateImageRegionPP::OnClickedBtnUp(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// get current selected item index
		int nSelectedItemIndex = m_listClues.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
		if (nSelectedItemIndex >= 0)
		{
			// get the index of the item right above currently selected item
			int nAboveIndex = nSelectedItemIndex-1;
			if (nAboveIndex < 0)
			{
				return 0;
			}

			// get selected item text from list
			char pszValue[NUM_OF_CHARS];
			// get selected item text from list
			m_listClues.GetItemText(nSelectedItemIndex, 0, pszValue, NUM_OF_CHARS);
			CString zEnt(pszValue);
			// then remove the selected item
			m_listClues.DeleteItem(nSelectedItemIndex);

			// now insert the item right before the item that was above
			int nActualIndex = m_listClues.InsertItem(nAboveIndex, zEnt);
			
			// keep this item selected
			m_listClues.SetItemState(nActualIndex, LVIS_SELECTED, LVIS_SELECTED);

			m_bCurrentListChanged = true;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07815");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CLocateImageRegionPP::OnClickedChkCaseSensitive(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_bCurrentListChanged = true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08047");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CLocateImageRegionPP::OnClickedChkAsRegExpr(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_bCurrentListChanged = true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07821");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CLocateImageRegionPP::OnClickedChkRestrict(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_bCurrentListChanged = true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07822");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CLocateImageRegionPP::OnDblclkClueList(int idCtrl, LPNMHDR pnmh, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	return OnClickedBtnModify(pnmh->code, pnmh->idFrom, pnmh->hwndFrom, bHandled);
}
//-------------------------------------------------------------------------------------------------
LRESULT CLocateImageRegionPP::OnCbnSelchangeCmbFindType(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// if "Image Region" selected, lock "Inside/Outside" option to inside and enable
		// corresponding image region text edit box
		if (m_cmbFindType.GetCurSel() == 1)
		{
			m_cmbInsideOutside.SetCurSel(0);
			m_txtIncludeExcludeRegionOn.SetWindowText(gpszTXT_REGION_ON_INCLUDE);
			m_cmbInsideOutside.EnableWindow(false);
			m_editImageRegionText.EnableWindow(true);
		}
		else
		{
			m_cmbInsideOutside.EnableWindow(true);
			m_editImageRegionText.EnableWindow(false);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13179");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CLocateImageRegionPP::OnSelChangeInsideOutside(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		if(m_cmbInsideOutside.GetCurSel() == 0)
		{
			m_txtIncludeExcludeRegionOn.SetWindowText(gpszTXT_REGION_ON_INCLUDE);
		}
		else
		{
			m_txtIncludeExcludeRegionOn.SetWindowText(gpszTXT_REGION_ON_EXCLUDE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16814");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CLocateImageRegionPP::OnBnClickedBtnLoadList(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		if (m_listClues.GetItemCount() > 0)
		{
			// prompt for overwrite
			int nRes = MessageBox("The existing entries will be overwritten. Do you wish to continue?", "Confirm", MB_YESNO);
			if (nRes == IDNO)
			{
				return 0;
			}
		}
		
		// show pick file dialog, do not show delimiter related windows
		CFileDialog openDialog( TRUE, ".txt", NULL, 
			OFN_HIDEREADONLY | OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR,
			"Text files (*.txt;*.dat)|*.txt;*.dat|All files (*.*)|*.*||", NULL);
		
		if (openDialog.DoModal() == IDOK)
		{
			string strFileName = openDialog.GetPathName().operator LPCSTR();
			validateFileOrFolderExistence( strFileName );
		
			// Set up File list 
			ifstream ifs(strFileName.c_str());
			CommentedTextFileReader ctfrFiles( ifs );

			if ( ifs.good() )
			{
				// load the list of files
				list<string> listItems;
				convertFileToListOfStrings(ifs, listItems);
				ctfrFiles.sGetUncommentedFileContents(listItems);

				// clear the list box first
				m_listClues.DeleteAllItems();
				// populate the list box
				int n = 0;
				for each ( string str in listItems )
				{
					m_listClues.InsertItem(n, str.c_str());
					n++;
				}

				// set selection to the first item
				if (listItems.size() > 0)
				{
					m_listClues.SetItemState(0, LVIS_SELECTED, LVIS_SELECTED);
				}

				// update buttons' state
				updateListButtons();

				m_bCurrentListChanged = true;
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13369");
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CLocateImageRegionPP::OnBnClickedBtnSaveList(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
	
		// show pick file dialog, do not show delimiter related windows
		CFileDialog openDialog( TRUE, ".txt", NULL, 
			OFN_HIDEREADONLY | OFN_OVERWRITEPROMPT | OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR,
			"Text files (*.txt)|*.txt;*.dat|All files (*.*)|*.*||", NULL);
		
		if (openDialog.DoModal() == IDOK)
		{
			string strFileToSave = openDialog.GetPathName().operator LPCSTR();

			if ( isValidFile(strFileToSave ) )
			{
				string strMsgText = "The file " + strFileToSave + " exists. Do you wish to overwrite it?";
				if ( MessageBox(strMsgText.c_str(),"Overwrite file?", MB_YESNO ) != IDYES )
				{
					return 0;
				}
			}

			// always overwrite if the file exists
			ofstream ofs(strFileToSave.c_str(), ios::out | ios::trunc);

			// iterate through List of clues
			int nTotalClues = m_listClues.GetItemCount();
			char pszValue[NUM_OF_CHARS];
			for (int n = 0; n < nTotalClues; n++)
			{
				// get selected text
				m_listClues.GetItemText(n, 0, pszValue, NUM_OF_CHARS);
				string strItemValue = pszValue;

				// save the value to the file
				ofs << strItemValue << endl;
			}

			ofs.close();
			waitForFileToBeReadable(strFileToSave);
		}
		
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13370");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CLocateImageRegionPP::OnClickedClueDynamicListInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// show tooltip info
		CString zText("- Dynamically loading a string list from a file is supported.\n"
					  "- To specify a dynamic file, an entry must begin with \"file://\".\n"
					  "- A file may be specified in combination with static entries or\n"
					  "  additional dynamic lists.\n"
					  "- Path tags such as <RSDFileDir> and <ComponentDataDir> may be used.\n"
					  "- For example, if an entry in the list is file://<RSDFileDir>\\list.txt,\n"
					  "  the entry will be replaced dynamically at runtime with the contents\n"
					  "  of the file.\n");
		m_infoTip.Show(zText);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14602");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CLocateImageRegionPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		if (pbValue == NULL)
		{
			return E_POINTER;
		}

		// Check license
		validateLicense();

		// If no exception, then pbValue is true
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
bool CLocateImageRegionPP::areValidOpposingRegions(const RegionBoundary& boundary1, 
												   const RegionBoundary& boundary2)
{
	// Check if the opposing boundaries start at the same location
	if (boundary1.m_eSide == boundary2.m_eSide && boundary1.m_eCondition == boundary2.m_eCondition)
	{
		double dExpand1 = boundary1.m_dExpand;
		double dExpand2 = boundary2.m_dExpand;

		const EExpandDirection eExpandDir1 = boundary1.m_eDirection;
		const EExpandDirection eExpandDir2 = boundary2.m_eDirection;

		// Check if the expansion extends the same distance 
		// and direction from the starting location
		if ((dExpand1 == 0 && dExpand2 == 0) || (dExpand1 == dExpand2 && eExpandDir1 == eExpandDir2))
		{
			string strBoundary1 = getBoundaryName(boundary1.m_eRegion);
			string strBoundary2 = getBoundaryName(boundary2.m_eRegion);
			string strMessage = strBoundary1 + "\\" + strBoundary2;
			strMessage += " boundary cannot be the same.";

			::MessageBox(m_hWnd, strMessage.c_str(), "Invalid setting", MB_ICONEXCLAMATION);

			return false;				
		}
		else if (eExpandDir1 == eExpandDir2)
		{
			// Ensure top is above bottom and left is left of right.
			if ((eExpandDir1 == kExpandUp || eExpandDir1 == kExpandLeft) ^ (dExpand1 > dExpand2))
			{
				string strBoundary1 = getBoundaryName(boundary1.m_eRegion);
				string strBoundary2 = getBoundaryName(boundary2.m_eRegion);
				string strMessage = strBoundary1 + " boundary cannot be ";
				strMessage += (boundary1.m_eRegion == kTop ? "below" : "right of");
				strMessage += " the " + strBoundary2 + " boundary.";

				::MessageBox(NULL, strMessage.c_str(), "Invalid setting", MB_ICONEXCLAMATION);
				return false;
			}
		}
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
void CLocateImageRegionPP::changeIcon(EClueListIndex eListIndex, bool bIsListEmpty)
{
	static HICON hIconPop = ::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_POPULATED));
	static HICON hIconEmpty = ::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_EMPTY));

	HICON hChangeToIcon = hIconPop;
	if (bIsListEmpty)
	{
		hChangeToIcon = hIconEmpty;
	}

	vector<HICON> vecIcons;
	vecIcons.push_back(m_picList1.GetIcon());
	vecIcons.push_back(m_picList2.GetIcon());
	vecIcons.push_back(m_picList3.GetIcon());
	vecIcons.push_back(m_picList4.GetIcon());

	// override the one needs to be changed
	vecIcons[eListIndex-1] = hChangeToIcon;

	// load them all to avoid any overlapping of the icon images in the dialog
	m_picList1.SetIcon(vecIcons[0]);
	m_picList2.SetIcon(vecIcons[1]);
	m_picList3.SetIcon(vecIcons[2]);
	m_picList4.SetIcon(vecIcons[3]);
}
//-------------------------------------------------------------------------------------------------
void CLocateImageRegionPP::cleanupClueLists()
{
	// temporarily store empty clue list name
	vector<EClueListIndex> vecEmptyClueLists;
	map<EClueListIndex, ListInfo>::iterator it = m_mapListNameToInfo.begin();
	for (; it != m_mapListNameToInfo.end(); it++)
	{
		if (it->second.m_vecClues.empty())
		{
			vecEmptyClueLists.push_back(it->first);
		}
	}

	// remove empty lists
	if (!vecEmptyClueLists.empty())
	{
		for (unsigned int ui = 0; ui < vecEmptyClueLists.size(); ui++)
		{
			it = m_mapListNameToInfo.find(vecEmptyClueLists[ui]);
			if (it != m_mapListNameToInfo.end())
			{
				m_mapListNameToInfo.erase(it);
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------
string CLocateImageRegionPP::getBoundaryName(EBoundary eBoundary)
{
	switch(eBoundary)
	{
	case kLeft:
		return "Left";

	case kTop:
		return "Top";

	case kRight:
		return "Right";

	case kBottom:
		return "Bottom";

	default:
		throw UCLIDException("ELI25613", "Unexpected boundary type.");
	}
}
//-------------------------------------------------------------------------------------------------
bool CLocateImageRegionPP::getSpatialLines(EBoundary eBoundary, double &rdSpatialLines)
{
	// Get the text of the spatial line expansion edit box
	CComBSTR bstrSpatialLines;
	m_ctrlBoundary[eBoundary-1].m_editExpandNumber.GetWindowText(&bstrSpatialLines);
	string strSpatialLines = asString(bstrSpatialLines);

	// ensure that some text was entered
	if (strSpatialLines.empty())
	{
		::MessageBox(NULL, (getBoundaryName(eBoundary) + 
			" Boundary: please enter a valid number of spatial lines to expand.").c_str(), 
			"Error", MB_ICONEXCLAMATION);
		return false;
	}

	// convert the text to a number
	double dExpandBottom = 0;
	try
	{
		rdSpatialLines = asDouble(strSpatialLines);
	}
	catch(...)
	{
		::MessageBox(NULL, 
			(getBoundaryName(eBoundary) + " Boundary: " + strSpatialLines + 
			" is not a valid number of spatial lines.").c_str(), 
			"Error", MB_ICONEXCLAMATION);
		return false;
	}

	// ensure that the number is positive
	if (rdSpatialLines < 0)
	{
		::MessageBox(NULL, (getBoundaryName(eBoundary) + 
			" Boundary: number of spatial strings to expand cannot be negative.").c_str(),
			"Error", MB_ICONEXCLAMATION);
		return false;
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
void CLocateImageRegionPP::initBoundaries(EBoundary eRegionBoundary, 
										  EBoundary eSide,
										  EBoundaryCondition eCondition,
										  EExpandDirection eExpandDirection,
										  double dExpandNumber)
{
	int iOffset = 0;

	BoundaryControls& ctrlBoundary = m_ctrlBoundary[eRegionBoundary - 1];

	switch (eRegionBoundary)
	{
	case kTop:
	case kBottom:
		{
			iOffset = 1;
		}
		break;

	case kLeft:
	case kRight:
		{
			iOffset = 3;
		}
		break;

	default:
		return;
	}

	ctrlBoundary.m_cmbSide.SetCurSel(eSide - iOffset);
	ctrlBoundary.m_cmbCondition.SetCurSel((long)eCondition - 1);
	ctrlBoundary.m_cmbExpandDirection.SetCurSel((long)eExpandDirection - iOffset);

	CString zNumber;
	zNumber.Format("%g", dExpandNumber);
	ctrlBoundary.m_editExpandNumber.SetWindowText(zNumber);
}
//-------------------------------------------------------------------------------------------------
void CLocateImageRegionPP::initClueList(EClueListIndex eListIndex, 
										IVariantVectorPtr ipClues, 
										bool bCaseSensitive,
										bool bAsRegExpr, 
										bool bRestrictSearch)
{
	ListInfo listInfo;
	long nSize = ipClues->Size;
	for (long n=0; n<nSize; n++)
	{
		string strClue = asString(_bstr_t(ipClues->GetItem(n)));
		listInfo.m_vecClues.push_back(strClue);
	}
	listInfo.m_bCaseSensitive = bCaseSensitive;
	listInfo.m_bAsRegExpr = bAsRegExpr;
	listInfo.m_bRestrictSearch = bRestrictSearch;

	updateRestrictSearchControl(eListIndex, listInfo);

	m_mapListNameToInfo[eListIndex] = listInfo;

	// update icon 
	if (!listInfo.m_vecClues.empty())
	{
		changeIcon(eListIndex, false);
	}
}
//-------------------------------------------------------------------------------------------------
void CLocateImageRegionPP::selectClueList(EClueListIndex eListIndex)
{
	ListInfo listInfo;

	// first look for the entry in the map
	map<EClueListIndex, ListInfo>::iterator itMap = m_mapListNameToInfo.find(eListIndex);
	if (itMap != m_mapListNameToInfo.end())
	{
		listInfo = itMap->second;
	}
	// alway update the check box and its related controls before 
	// actually select the clue list
	updateRestrictSearchControl(eListIndex, listInfo);

	// update the entry in the map
	m_mapListNameToInfo[eListIndex] = listInfo;

	// depending on the contents to update the selected clue list display
	// make sure the correct radio button is selected
	UINT nOption = IDC_OPT_LIST1 + (long)eListIndex - 1;
	ATLControls::CButton checkBox = GetDlgItem(nOption);
	int nChecked = checkBox.GetCheck();
	// if current clue list radio button is not checked
	if (nChecked != 1)
	{
		// clear all checks if any, then check this radio
		for (int n=IDC_OPT_LIST1; n<IDC_OPT_LIST1 + (long)eListIndex; n++)
		{
			checkBox = GetDlgItem(n);
			// only check current clue list
			if (n-IDC_OPT_LIST1 == (long)eListIndex - 1)
			{
				checkBox.SetCheck(1);
			}
			else
			{
				checkBox.SetCheck(0);
			}
		}
	}

	// clear the list box first
	m_listClues.DeleteAllItems();
	// populate the list box
	int nSize = listInfo.m_vecClues.size();
	for (int n=0; n<nSize; n++)
	{
		m_listClues.InsertItem(n, listInfo.m_vecClues[n].c_str());
	}
	
	// set selection to the first item
	if (nSize > 0)
	{
		m_listClues.SetItemState(0, LVIS_SELECTED, LVIS_SELECTED);
	}

	// update buttons' state
	updateListButtons();

	// set checks
	m_chkCaseSensitive.SetCheck(listInfo.m_bCaseSensitive);
	m_chkAsRegExpr.SetCheck(listInfo.m_bAsRegExpr);
	m_chkRestrictSearch.SetCheck(listInfo.m_bRestrictSearch);
	m_chkRestrictSearch.EnableWindow(!listInfo.m_bDisableRestriction);
	m_txtClueListNumbers.SetWindowText(listInfo.m_zListNumbers);

	// set current selected clue list
	m_eCurrentSelectedClueList = eListIndex;
}
//-------------------------------------------------------------------------------------------------
void CLocateImageRegionPP::setupControls()
{
	// store all values for combo boxes statically
	static bool bValuesEmpty = true;
	static vector<CString> s_vecInsideOutside;
	static vector<CString> s_vecIncludeExclude;
	static vector<CString> s_vecEntities;
	static vector<CString> s_vecSides;
	static vector<CString> s_vecConditions;
	
	if (bValuesEmpty)
	{
		s_vecInsideOutside.push_back("Inside");
		s_vecInsideOutside.push_back("Outside");

		s_vecIncludeExclude.push_back("Include");
		s_vecIncludeExclude.push_back("Exclude");

		s_vecEntities.push_back("Characters");
		s_vecEntities.push_back("Words");
		s_vecEntities.push_back("Lines");

		s_vecSides.push_back("Top");
		s_vecSides.push_back("Bottom");
		s_vecSides.push_back("Left");
		s_vecSides.push_back("Right");

		s_vecConditions.push_back("Clue List 1");
		s_vecConditions.push_back("Clue List 2");
		s_vecConditions.push_back("Clue List 3");
		s_vecConditions.push_back("Clue List 4");
		s_vecConditions.push_back("Page Containing Clues");

		bValuesEmpty = false;
	}

	// initialize the type of find to perform combobox
	m_cmbFindType = GetDlgItem(IDC_CMB_FIND_TYPE);
	m_cmbFindType.AddString("Text");
	m_cmbFindType.AddString("Image Region");

	m_editImageRegionText = GetDlgItem(IDC_EDT_IMAGE_REGION_TEXT);

	m_cmbInsideOutside = GetDlgItem(IDC_CMB_INSIDE);
	unsigned int ui;
	for (ui = 0; ui < s_vecInsideOutside.size(); ui++)
	{
		m_cmbInsideOutside.AddString(s_vecInsideOutside[ui]);
	}

	m_cmbIncludeExclude = GetDlgItem(IDC_CMB_INCLUDE);
	for (ui = 0; ui < s_vecIncludeExclude.size(); ui++)
	{
		m_cmbIncludeExclude.AddString(s_vecIncludeExclude[ui]);
	}

	m_cmbIntersectingEntities = GetDlgItem(IDC_CMB_INTERSECTING);
	for (ui = 0; ui < s_vecEntities.size(); ui++)
	{
		m_cmbIntersectingEntities.AddString(s_vecEntities[ui]);
	}

	m_cmbMatchMultiplePagesPerDocument = GetDlgItem(IDC_CMB_MATCH_PER_DOC);
	for (ui = 0; ui < giCMB_MATCH_PER_DOC_SIZE; ui++)
	{
		m_cmbMatchMultiplePagesPerDocument.AddString(gpszCMB_MATCH_PER_DOC[ui]);
	}

	int iSide[] = {IDC_CMB_SIDE1, IDC_CMB_SIDE2, IDC_CMB_SIDE3, IDC_CMB_SIDE4};
	int iCondition[] = {IDC_CMB_CONDITION1, IDC_CMB_CONDITION2, IDC_CMB_CONDITION3, IDC_CMB_CONDITION4};
	int iDirection[] = {IDC_CMB_EXPAND_DIR_TOP, IDC_CMB_EXPAND_DIR_BOTTOM, IDC_CMB_EXPAND_DIR_LEFT, IDC_CMB_EXPAND_DIR_RIGHT};
	int iExpandNumbers[] = {IDC_EDIT_NUM1, IDC_EDIT_NUM2, IDC_EDIT_NUM3, IDC_EDIT_NUM4};
	for (int i = 0; i < 4; i++)
	{
		m_ctrlBoundary[i].m_cmbSide = GetDlgItem(iSide[i]);
		m_ctrlBoundary[i].m_cmbCondition = GetDlgItem(iCondition[i]);
		m_ctrlBoundary[i].m_cmbExpandDirection = GetDlgItem(iDirection[i]);
		m_ctrlBoundary[i].m_editExpandNumber = GetDlgItem(iExpandNumbers[i]);

		for (ui = 0; ui < s_vecConditions.size(); ui++)
		{
			m_ctrlBoundary[i].m_cmbCondition.AddString(s_vecConditions[ui]);
		}
	}

	for (ui = 0; ui < s_vecSides.size() - 2; ui++)
	{
		m_ctrlBoundary[0].m_cmbSide.AddString(s_vecSides[ui]);
		m_ctrlBoundary[1].m_cmbSide.AddString(s_vecSides[ui]);
		m_ctrlBoundary[2].m_cmbSide.AddString(s_vecSides[ui+2]);
		m_ctrlBoundary[3].m_cmbSide.AddString(s_vecSides[ui+2]);
	}

	// Setup expand direction controls
	const char pszExpandDirections[4][6] = { "Up", "Down", "Left", "Right" };
	for(ui = 0; ui < 2; ui++)
	{
		m_ctrlBoundary[0].m_cmbExpandDirection.AddString(pszExpandDirections[ui]);
		m_ctrlBoundary[1].m_cmbExpandDirection.AddString(pszExpandDirections[ui]);
		m_ctrlBoundary[2].m_cmbExpandDirection.AddString(pszExpandDirections[ui + 2]);
		m_ctrlBoundary[3].m_cmbExpandDirection.AddString(pszExpandDirections[ui + 2]);
	}

	m_listClues = GetDlgItem(IDC_LIST_CLUES);
	m_listClues.SetExtendedListViewStyle(LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT);
	CRect rect;
	m_listClues.GetClientRect(&rect);
	m_listClues.InsertColumn(0, "Value", LVCFMT_LEFT, rect.Width(), 0);

	m_btnAdd = GetDlgItem(IDC_BTN_ADD_LR);
	m_btnUp = GetDlgItem(IDC_BTN_UP_LR);
	m_btnDown = GetDlgItem(IDC_BTN_DOWN_LR);
	m_btnModify = GetDlgItem(IDC_BTN_MODIFY_LR);
	m_btnRemove = GetDlgItem(IDC_BTN_REMOVE_LR);
	m_btnSaveList = GetDlgItem(IDC_BTN_SAVE_LIST);
	
	m_chkCaseSensitive = GetDlgItem(IDC_CHK_CASE_SENSITIVE_LR);
	m_chkAsRegExpr = GetDlgItem(IDC_CHK_AS_REGEXPR_LR);
	m_chkRestrictSearch = GetDlgItem(IDC_CHK_RESTRICT);
	
	m_txtClueListNumbers = GetDlgItem(IDC_TEXT_LIST_NUM);
	m_txtIncludeExcludeRegionOn = GetDlgItem(IDC_STATIC_REGION_ON);

	m_btnUp.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_UP)));
	m_btnDown.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_DOWN)));

	m_picList1 = GetDlgItem(IDC_PIC_ICON_1);
	m_picList2 = GetDlgItem(IDC_PIC_ICON_2);
	m_picList3 = GetDlgItem(IDC_PIC_ICON_3);
	m_picList4 = GetDlgItem(IDC_PIC_ICON_4);
}
//-------------------------------------------------------------------------------------------------
bool CLocateImageRegionPP::storeClueLists(UCLID_AFVALUEFINDERSLib::ILocateImageRegionPtr ipLocateRegion)
{
	try
	{
		// before storing non-empty clue list(s), clear the original clue lists first
		ipLocateRegion->ClearAllClueLists();

		map<EClueListIndex, ListInfo>::iterator itMap = m_mapListNameToInfo.begin();
		for (; itMap != m_mapListNameToInfo.end(); itMap++)
		{
			EClueListIndex eClueListIndex = itMap->first;
			ListInfo listInfo = itMap->second;
			IVariantVectorPtr ipClues(CLSID_VariantVector);
			ASSERT_RESOURCE_ALLOCATION("ELI07839", ipClues != __nullptr);
			// populate the vector
			for (unsigned int ui = 0; ui < listInfo.m_vecClues.size(); ui++)
			{
				ipClues->PushBack(_bstr_t(listInfo.m_vecClues[ui].c_str()));
			}
			
			ipLocateRegion->SetClueList(
				(UCLID_AFVALUEFINDERSLib::EClueListIndex)eClueListIndex,
				ipClues, 
				asVariantBool(listInfo.m_bCaseSensitive),
				asVariantBool(listInfo.m_bAsRegExpr),
				asVariantBool(listInfo.m_bRestrictSearch) );
		}

		return true;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07840");

	return false;
}
//-------------------------------------------------------------------------------------------------
void CLocateImageRegionPP::storeCurrentListSettings()
{
	ListInfo listInfo;;
	listInfo.m_vecClues.clear();
	
	int nTotalClues = m_listClues.GetItemCount();
	char pszValue[NUM_OF_CHARS];
	for (int n=0; n<nTotalClues; n++)
	{
		// get selected text
		m_listClues.GetItemText(n, 0, pszValue, NUM_OF_CHARS);
		CString zItemValue(pszValue);
		
		listInfo.m_vecClues.push_back((LPCTSTR)zItemValue);
	}
	
	// if the clue list is not empty
	if (!listInfo.m_vecClues.empty())
	{
		listInfo.m_bCaseSensitive = m_chkCaseSensitive.GetCheck()==1;
		listInfo.m_bAsRegExpr = m_chkAsRegExpr.GetCheck()==1;
		listInfo.m_bRestrictSearch = m_chkRestrictSearch.IsWindowEnabled()==TRUE 
			&& m_chkRestrictSearch.GetCheck()==1;
	}
	else
	{
		// if the clue list is empty, then uncheck the flags
		listInfo.m_bCaseSensitive = false;
		listInfo.m_bAsRegExpr = false;
		listInfo.m_bRestrictSearch = false;
	}
	
	m_mapListNameToInfo[m_eCurrentSelectedClueList] = listInfo;
	
	// update any Restrict Search value
	updateAllRestrictSearchValue();
}
//-------------------------------------------------------------------------------------------------
bool CLocateImageRegionPP::tryGetRegionBoundary(EBoundary eBoundary, RegionBoundary& boundary)
{
	// Get the controls for this boundary
	BoundaryControls& ctrlBoundary = m_ctrlBoundary[eBoundary - 1];

	// Get the opposing boundary and offset
	EBoundary eOpposingBoundary;
	int iOffset = 1;
	if (eBoundary == kLeft)
	{
		eOpposingBoundary = kRight;
		iOffset = 3;
	}
	else if (eBoundary == kTop)
	{
		eOpposingBoundary = kBottom;
	}
	else if (eBoundary == kRight)
	{
		eOpposingBoundary = kLeft;
		iOffset = 3;
	}
	else
	{
		eOpposingBoundary = kTop;
	}

	boundary.m_eRegion = eBoundary;
	boundary.m_eSide = (EBoundary)(ctrlBoundary.m_cmbSide.GetCurSel() + iOffset);
	boundary.m_eCondition = (EBoundaryCondition) (ctrlBoundary.m_cmbCondition.GetCurSel() + 1);

	// Get the direction to expand
	if (ctrlBoundary.m_cmbExpandDirection.GetCurSel() == 0)
	{
		boundary.m_eDirection = (EExpandDirection)iOffset;
	}
	else
	{
		boundary.m_eDirection = (EExpandDirection)(iOffset + 1);
	}

	// Get the number of spatial lines to expand the boundary or return with an error message
	if( !getSpatialLines(eBoundary, boundary.m_dExpand) )
	{
		return false;
	}

	// Boundary can't be opposing side of the page (e.g. Top boundary can't be bottom of page)
	// unless it is expanded inward. [FlexIDSCore #3051]
	if (boundary.m_eSide == eOpposingBoundary && boundary.m_eCondition == kPage && 
		(boundary.m_dExpand == 0 || boundary.m_eDirection == boundary.m_eSide))
	{
		string strMessage = getBoundaryName(eBoundary);
		strMessage += " border of the region can't be defined as ";
		strMessage += getBoundaryName(eOpposingBoundary);
		strMessage += " of a page unless it is expanded inward.";
		MessageBox(strMessage.c_str(), "Invalid setting");
		return false;
	}

	// If page is defined, but there's no clue list define at all
	if (boundary.m_eCondition == kPage && m_mapListNameToInfo.empty())
	{
		string strMessage = getBoundaryName(eBoundary);
		strMessage += " Boundary: Can't select \'Page Containing Clues\' since no clue list is defined.";

		MessageBox(strMessage.c_str(), "Invalid setting");
		return false;
	}

	// Make sure the boundary condition defined exists
	if (boundary.m_eCondition <= kClueList4)
	{
		// Make sure if any of the clue lists is selected, the clue list must not be empty
		EClueListIndex eClueListIndex = (EClueListIndex)boundary.m_eCondition;
		map<EClueListIndex, ListInfo>::iterator itMap = m_mapListNameToInfo.find(eClueListIndex);
		if (itMap == m_mapListNameToInfo.end())
		{
			string strMessage = getBoundaryName(eBoundary);
			strMessage += " Boundary: the clue list is undefined.";

			MessageBox(strMessage.c_str(), "Invalid setting");
			return false;
		}
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
void CLocateImageRegionPP::updateAllRestrictSearchValue()
{
	// each item represents whether or not its associated clue list is empty
	vector<bool> vecIsNotEmpty;
	// List1 Is Not Empty
	vecIsNotEmpty.push_back(false);
	// List2 Is Not Empty
	vecIsNotEmpty.push_back(false);
	// List3 Is Not Empty
	vecIsNotEmpty.push_back(false);
	// List4 Is Not Empty
	vecIsNotEmpty.push_back(false);

	long n;
	for (n = (long)kList1; n <= (long)kList4; n++)
	{
		map<EClueListIndex, ListInfo>::iterator itMap = m_mapListNameToInfo.find((EClueListIndex)n);
		if (itMap != m_mapListNameToInfo.end())
		{
			if (!itMap->second.m_vecClues.empty())
			{
				vecIsNotEmpty[n-1] = true;
			}
		}
	}

	// iterate through each clue list info
	map<EClueListIndex, ListInfo>::iterator itMap = m_mapListNameToInfo.begin();
	for (; itMap != m_mapListNameToInfo.end(); itMap++)
	{
		// each clue list depends on whether or not
		// its prior clue lists are empty
		EClueListIndex listIndex = itMap->first;
		bool bRestrictSearch = itMap->second.m_bRestrictSearch;
		if (bRestrictSearch)
		{
			// at least one of the prior clue lists must not be empty
			bool bEnableRestrict = false;
			for (n = 0; n < (long)listIndex - 1; n++)
			{
				bEnableRestrict = bEnableRestrict || vecIsNotEmpty[n];
			}

			// both must be true
			bRestrictSearch = bRestrictSearch && bEnableRestrict;
			// reset value
			itMap->second.m_bRestrictSearch = bRestrictSearch;
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CLocateImageRegionPP::updateListButtons()
{
	// enable/disable up and down arrow key buttons appropriately
	m_btnUp.EnableWindow(FALSE);
	m_btnDown.EnableWindow(FALSE);

	// get current selected item index
	int nSelectedItemIndex = m_listClues.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
	int nSelCount = m_listClues.GetSelectedCount();
	int nCount = m_listClues.GetItemCount();
	
	if (nCount == 0)
	{
		m_btnModify.EnableWindow(FALSE);
		m_btnRemove.EnableWindow(FALSE);
	}
	else
	{
		m_btnModify.EnableWindow( asMFCBool(nSelCount == 1) );
		m_btnRemove.EnableWindow( asMFCBool(nSelCount >= 1) );

		if ((nCount > 1) && (nSelCount == 1))
		{
			if (nSelectedItemIndex == 0)
			{
				// First item selected
				// enable down button only
				m_btnUp.EnableWindow(FALSE);
				m_btnDown.EnableWindow(TRUE);
			}
			else if (nSelectedItemIndex > 0 && nSelectedItemIndex < nCount-1)
			{
				// Some item other that first and last item selected
				// enable both buttons
				m_btnUp.EnableWindow(TRUE);
				m_btnDown.EnableWindow(TRUE);
			}
			else if (nSelectedItemIndex == nCount-1)
			{
				// Last item selected
				// enable up button only
				m_btnUp.EnableWindow(TRUE);
				m_btnDown.EnableWindow(FALSE);
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CLocateImageRegionPP::updateRestrictSearchControl(EClueListIndex eCurrentListIndex,
													   ListInfo& listInfo)
{
	listInfo.m_zListNumbers.Empty();
	listInfo.m_bDisableRestriction = true;
	// determine whether or not to disable check box for restricting search
	// by checking if there's any prior clue list exists
	for (long n=(long)kList1; n<(long)eCurrentListIndex; n++)
	{
		map<EClueListIndex, ListInfo>::iterator itMap = m_mapListNameToInfo.find((EClueListIndex)n);
		if (itMap != m_mapListNameToInfo.end())
		{
			ListInfo priorListInfo = itMap->second;
			// if the clue list is not empty
			if (!priorListInfo.m_vecClues.empty())
			{
				// enable the check box
				listInfo.m_bDisableRestriction = false;
				
				CString zTemp("");
				long nNumber = (long)itMap->first;
				if (listInfo.m_zListNumbers.IsEmpty())
				{
					zTemp.Format("%ld", nNumber);
				}
				else
				{
					zTemp.Format(", %ld", nNumber);
				}
				
				listInfo.m_zListNumbers += zTemp;
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CLocateImageRegionPP::validateLicense()
{
	// Property Page requires Editor license
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI07778", "Locate Image Region PP");
}
//-------------------------------------------------------------------------------------------------
