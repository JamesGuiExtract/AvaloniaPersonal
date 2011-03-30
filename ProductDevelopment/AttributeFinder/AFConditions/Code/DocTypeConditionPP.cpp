// DocTypeConditionPP.cpp : Implementation of CDocTypeConditionPP
#include "stdafx.h"
#include "AFConditions.h"
#include "DocTypeConditionPP.h"

#include <EditorLicenseID.h>
#include <SpecialStringDefinitions.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <COMUtils.h>

#include <string>

using namespace std;

//-------------------------------------------------------------------------------------------------
// CDocTypeConditionPP
//-------------------------------------------------------------------------------------------------
CDocTypeConditionPP::CDocTypeConditionPP()
{
	try
	{
		m_dwTitleID = IDS_TITLEDocTypeConditionPP;
		m_dwHelpFileID = IDS_HELPFILEDocTypeConditionPP;
		m_dwDocStringID = IDS_DOCSTRINGDocTypeConditionPP;

		m_ipDocUtils = __nullptr;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI10787");
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocTypeConditionPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("CDocTypeConditionPP::Apply\n"));

		for (unsigned int ui = 0; ui < m_nObjects; ui++)
		{
			UCLID_AFCONDITIONSLib::IDocTypeConditionPtr ipCondition = m_ppUnk[ui];
			ASSERT_RESOURCE_ALLOCATION("ELI10788", ipCondition != __nullptr);

			IVariantVectorPtr ipVec(CLSID_VariantVector);
			ASSERT_RESOURCE_ALLOCATION("ELI10804", ipVec != __nullptr);

			// At least 1 DocType must be specified
			long nCount = m_listTypes.GetCount();
			if (nCount <= 0)
			{
				UCLIDException ue("ELI10805", "Please specify at least one document type.");
				throw ue;
			}

			// get all the doc types
			for (ui = 0; ui < (unsigned int)nCount; ui++)
			{
				long nLength = m_listTypes.GetTextLen(ui);
				unique_ptr<char[]> pBuf(new char[nLength+1]);
				ASSERT_RESOURCE_ALLOCATION("ELI12925", pBuf.get() != __nullptr);
				
				m_listTypes.GetText(ui, pBuf.get());
				_bstr_t bstrType(pBuf.get());
				ipVec->PushBack(bstrType);
			}

			ipCondition->Types = ipVec;
			ipCondition->AllowTypes = m_cmbMatch.GetCurSel() == 0 ? VARIANT_TRUE : VARIANT_FALSE;

			// Store category name
			ipCondition->Category = _bstr_t( m_strCategory.c_str() );

			// Store confidence level
			EDocumentConfidenceLevel eConfidence = kMaybeLevel;
			switch (m_cmbMinProbability.GetCurSel())
			{
			case 0:
				eConfidence = kMaybeLevel;
				break;
			case 1:
				eConfidence = kProbableLevel;
				break;
			case 2:
				eConfidence = kSureLevel;
				break;
			}
			ipCondition->MinConfidence = eConfidence;
		}

		SetDirty(FALSE);
		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10789")

	// if we reached here, it's because of an exception
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CDocTypeConditionPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, 
									 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_AFCONDITIONSLib::IDocTypeConditionPtr ipCondition = m_ppUnk[0];
		if (ipCondition != __nullptr)
		{
			// Get controls
			m_cmbMatch = GetDlgItem(IDC_CMB_MATCH);
			m_btnAddTypes = GetDlgItem(IDC_BTN_ADD_TYPES);
			m_btnRemoveTypes = GetDlgItem(IDC_BTN_REMOVE_TYPES);
			m_btnClearTypes = GetDlgItem(IDC_BTN_CLEAR_TYPES);
			m_listTypes = GetDlgItem(IDC_LIST_TYPES);
			m_cmbMinProbability = GetDlgItem(IDC_CMB_MIN_PROBABILITY);

			// Store Category
			m_strCategory = ipCondition->Category;

			// populate the match combo box
			m_cmbMatch.InsertString(0, _bstr_t("match"));
			m_cmbMatch.InsertString(1, _bstr_t("not match"));
			m_cmbMatch.SetCurSel(ipCondition->AllowTypes == VARIANT_TRUE ? 0 : 1);

			// Add any types that are already on the condition
			IVariantVectorPtr ipTypes = ipCondition->Types;
			if (ipTypes != __nullptr)
			{
				int i;
				long nNumTypes = ipTypes->Size;
				for (i = 0; i < nNumTypes; i++)
				{
					_variant_t var = ipTypes->GetItem(i);
					_bstr_t bstrType = var.bstrVal;
					m_listTypes.InsertString(i, bstrType);
				}
			}

			// populate the probability combo
			m_cmbMinProbability.InsertString(0, "maybe");
			m_cmbMinProbability.InsertString(1, "probable");
			m_cmbMinProbability.InsertString(2, "sure");

			EDocumentConfidenceLevel eConfidence = ipCondition->MinConfidence;
			long nSel = 0;
			switch (eConfidence)
			{
			case kMaybeLevel:
				nSel = 0;
				break;
			case kProbableLevel:
				nSel = 1;
				break;
			case kSureLevel:
				nSel = 2;
				break;
			}
			m_cmbMinProbability.SetCurSel(nSel);

		}

		// enable/disable the remove/clear buttons
		updateButtonTypes();

		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10790");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CDocTypeConditionPP::OnClickedBtnAddTypes(WORD wNotifyCode, 
													 WORD wID, HWND hWndCtl, 
													 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Create a new Utils object if necessary
		if (m_ipDocUtils == __nullptr)
		{
			m_ipDocUtils.CreateInstance( CLSID_DocumentClassifier );
			ASSERT_RESOURCE_ALLOCATION("ELI11927", m_ipDocUtils != __nullptr);
		}

		// if the category is not set default it to the first in the industry list
		if ( m_strCategory.empty() )
		{
			// Default it to the first industry category in the category list
			IVariantVectorPtr ipIndustries = m_ipDocUtils->GetDocumentIndustries();
			ASSERT_RESOURCE_ALLOCATION("ELI15716", ipIndustries != __nullptr );

			// Make sure there is at least one industry
			if ( ipIndustries->Size > 0 )
			{
				// Get the first industry in the list
				variant_t vt = ipIndustries->GetItem(0);
				
				// Set the industry
				m_strCategory = asString(vt.bstrVal);
			}
			else
			{
				UCLIDException ue("ELI15717", "No industry categories defined");
				throw ue;
			}
		}

		// Display the dialog - with variable industry, multiple selection and special types
		_bstr_t	bstrIndustry = get_bstr_t( m_strCategory.c_str() );
		IVariantVectorPtr ipTypes = m_ipDocUtils->GetDocTypeSelection( 
			&(bstrIndustry.GetBSTR()), VARIANT_TRUE, VARIANT_TRUE, VARIANT_TRUE, VARIANT_TRUE);

		// Store the returned industry
		m_strCategory = asString(bstrIndustry);

		// Reset the content if new items have been selected
		long lCount = ipTypes->Size;
		if (lCount != 0)
		{
			m_listTypes.ResetContent();
		}

		// Display the selected types
		int i;
		for (i = 0; i < lCount; i++)
		{
			// Retrieve this type
			string strType = asString( _bstr_t(ipTypes->GetItem( i )));

			// Add the type
			m_listTypes.InsertString( -1, strType.c_str() );
		}

		// Refresh the button states
		updateButtonTypes();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10791");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CDocTypeConditionPP::OnClickedBtnRemoveTypes(WORD wNotifyCode, 
													 WORD wID, HWND hWndCtl, 
													 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Remove the selected items
		removeSelected();

		// Enable / disable buttons as appropriate
		updateButtonTypes();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15714");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CDocTypeConditionPP::OnClickedBtnClearTypes(WORD wNotifyCode, 
													 WORD wID, HWND hWndCtl, 
													 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_listTypes.ResetContent();
		updateButtonTypes();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10793");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CDocTypeConditionPP::OnSelChangeListTypes(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		updateButtonTypes();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10794");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CDocTypeConditionPP::removeSelected()
{
	// Check selection state for each item
	int iFirstSelected = -1;
	for (int i = 0; i < m_listTypes.GetCount(); i++)
	{
		if (m_listTypes.GetSel( i ) > 0)
		{
			// Remember this index if the first selected item
			if (iFirstSelected == -1)
			{
				iFirstSelected = i;
			}

			// Delete this item
			m_listTypes.DeleteString( i );

			// Decrement index to avoid missing a list item
			i--;
		}
	}

	// Update selection
	int iCount = m_listTypes.GetCount();
	if (iCount > iFirstSelected)
	{
		// Select the item at same index as previous first selection
		m_listTypes.SetSel( iFirstSelected );
	}
	else if (iCount > 0)
	{
		// Just select the last item
		m_listTypes.SetSel( iCount - 1 );
	}
}
//-------------------------------------------------------------------------------------------------
void CDocTypeConditionPP::updateButtonTypes()
{
	// Enable/Disable the clear and remove buttons
	// based on whether those options should be 
	// available

	// can't clear or remove items from an empty list
	if (m_listTypes.GetCount() > 0)
	{
		// At least one item must be selected for Remove
		if (m_listTypes.GetSelCount() > 0)
		{
			m_btnRemoveTypes.EnableWindow(TRUE);
		}
		else
		{
			m_btnRemoveTypes.EnableWindow(FALSE);
		}

		m_btnClearTypes.EnableWindow(TRUE);
	}
	else
	{
		m_btnRemoveTypes.EnableWindow(FALSE);
		m_btnClearTypes.EnableWindow(FALSE);
	}
}
//-------------------------------------------------------------------------------------------------
