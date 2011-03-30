// MergeAttributeTreesPP.cpp : Implementation of CMergeAttributeTreesPP

#include "stdafx.h"
#include "MergeAttributeTreesPP.h"
#include "..\..\AFUtils\Code\Helper.h"

#include <ComponentLicenseIDs.h>
#include <ComUtils.h>
#include <LicenseMgmt.h>
#include <UCLIDException.h>

//--------------------------------------------------------------------------------------------------
// CMergeAttributeTreesPP
//--------------------------------------------------------------------------------------------------
CMergeAttributeTreesPP::CMergeAttributeTreesPP()
{
}
//--------------------------------------------------------------------------------------------------
CMergeAttributeTreesPP::~CMergeAttributeTreesPP()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI26388");
}
//--------------------------------------------------------------------------------------------------
HRESULT CMergeAttributeTreesPP::FinalConstruct()
{
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
void CMergeAttributeTreesPP::FinalRelease()
{
}

//--------------------------------------------------------------------------------------------------
// Windows message handlers
//--------------------------------------------------------------------------------------------------
LRESULT CMergeAttributeTreesPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_AFOUTPUTHANDLERSLib::IMergeAttributeTreesPtr ipRule = m_ppUnk[0];
		ASSERT_RESOURCE_ALLOCATION("ELI26389", ipRule);

		// Create tooltip object and set the delay to 0
		m_xinfoRemoveEmpty.Create(CWnd::FromHandle(m_hWnd));
		m_xinfoRemoveEmpty.SetShowDelay(0);

		// Map controls to member variables
		m_editAttributesToMergeQuery = GetDlgItem(IDC_EDIT_MERGE_TREES_QUERY);
		m_radMergeIntoFirst = GetDlgItem(IDC_RADIO_MERGE_INTO_FIRST);
		m_radMergeIntoBiggest = GetDlgItem(IDC_RADIO_MERGE_INTO_BIGGEST);
		m_editSubAttributes = GetDlgItem(IDC_EDIT_MERGE_SUBATTRIBUTES);
		m_radDiscardNonMatch = GetDlgItem(IDC_RADIO_NOMATCH_DISCARD);
		m_radPreserveNonMatch = GetDlgItem(IDC_RADIO_NOMATCH_PRESERVE);
		m_chkCaseSensitive = GetDlgItem(IDC_CHECK_MERGE_CASESENSITIVE);
		m_chkCompareTypeInfo = GetDlgItem(IDC_CHECK_MERGE_COMPARE_TYPE);
		m_chkCompareSubAttributes = GetDlgItem(IDC_CHECK_MERGE_COMPARE_SUBATTR);
		m_chkRemoveEmptyAttributes = GetDlgItem(IDC_CHECK_MERGE_REMOVE_EMPTY);
		
		// Set the query text
		m_editAttributesToMergeQuery.SetWindowText(asString(ipRule->AttributesToBeMerged).c_str());

		// Set the appropriate radio button for the merge into group box
		CheckRadioButton(IDC_RADIO_MERGE_INTO_FIRST, IDC_RADIO_MERGE_INTO_BIGGEST,
			ipRule->MergeAttributeTreesInto == kFirstAttribute ?
			IDC_RADIO_MERGE_INTO_FIRST : IDC_RADIO_MERGE_INTO_BIGGEST);

		// Set the sub attributes text
		m_editSubAttributes.SetWindowText(asString(ipRule->SubAttributesToCompare).c_str());

		// Set the appropriate radio button for the non-matching comparison group box
		CheckRadioButton(IDC_RADIO_NOMATCH_DISCARD, IDC_RADIO_NOMATCH_PRESERVE,
			asCppBool(ipRule->DiscardNonMatchingComparisons) ?
			IDC_RADIO_NOMATCH_DISCARD : IDC_RADIO_NOMATCH_PRESERVE);

		// Set the case sensitive check box
		m_chkCaseSensitive.SetCheck(asBSTChecked(ipRule->CaseSensitive));

		// TODO: FUTURE implement this functionality completely
		// Set these values with their defaults and disable the controls until the
		// functionality is implemented (Future functionality)
		m_chkCompareTypeInfo.SetCheck(asBSTChecked(ipRule->CompareTypeInformation));
		m_chkCompareTypeInfo.EnableWindow(FALSE);
		m_chkCompareSubAttributes.SetCheck(asBSTChecked(ipRule->CompareSubAttributes));
		m_chkCompareSubAttributes.EnableWindow(FALSE);

		// Set the remove hierarchy check box
		m_chkRemoveEmptyAttributes.SetCheck(asBSTChecked(ipRule->RemoveEmptyHierarchy));

		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI26390");

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CMergeAttributeTreesPP::OnClickedRemoveAttributeInfo(WORD wNotifyCode, WORD wID,
															 HWND hWndCtl, BOOL &bHandled)
{
	try
	{
		CString zText("This setting will cause any empty attribute hierarchy to be removed.\n"
			"For example if you merge the following attributes with the query Data/Test:\n"
			"Data|Test Data\n"
			".Test|N/A\n"
			"..CollectionDate|09/08/2008\n"
			"..CollectionTime|11:34AM\n"
			"..Component|HGB\n"
			"\nData|Test Data\n"
			".Test|N/A\n"
			"..CollectionDate|09/08/2008\n"
			"..CollectionTime|11:34AM\n"
			"..Component|HCT\n"
			"\nData|Test Data\n"
			".Test|N/A\n"
			"..Component|RBC\n"
			".Tester|Jon Doe\n"
			"\nAfter the merging with this setting turned on the resulting attribute collection will be:\n"
			"Data|Test Data\n"
			".Test|N/A\n"
			"..CollectionDate|09/08/2008\n"
			"..CollectionTime|11:34AM\n"
			"..Component|HGB\n"
			"..Component|HCT\n"
			"..Component|RBC\n"
			"\nData|Test Data\n"
			".Tester|Jon Doe\n");
		m_xinfoRemoveEmpty.Show(zText);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI26459");

	return 0;
}

//--------------------------------------------------------------------------------------------------
// IPropertyPage
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributeTreesPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ATLTRACE(_T("CMergeAttributeTreesPP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			// Obtain interface pointer to the IMergeAttributeTrees class
			UCLID_AFOUTPUTHANDLERSLib::IMergeAttributeTreesPtr ipRule = m_ppUnk[i];
			ASSERT_RESOURCE_ALLOCATION("ELI26391", ipRule != __nullptr);

			// Get and validate the values in the control
			_bstr_t bstrAttributesToMerge;
			m_editAttributesToMergeQuery.GetWindowText(bstrAttributesToMerge.GetAddress());
			if(bstrAttributesToMerge.length() == 0
				|| containsInvalidQueryString(asString(bstrAttributesToMerge)))
			{
				MessageBox("The attributes to be merged query cannot be blank or contain '|'.",
					"Invalid Query", MB_OK | MB_ICONERROR);
				m_editAttributesToMergeQuery.SetFocus();
				return S_FALSE;
			}

			_bstr_t bstrSubAttributes;
			m_editSubAttributes.GetWindowText(bstrSubAttributes.GetAddress());
			if(bstrSubAttributes.length() == 0
				|| containsInvalidQueryString(asString(bstrSubAttributes)))
			{
				MessageBox("The sub attributes to be compared query cannot be blank or contain '|'.",
					"Invalid Query", MB_OK | MB_ICONERROR);
				m_editSubAttributes.SetFocus();
				return S_FALSE;
			}

			ipRule->AttributesToBeMerged = bstrAttributesToMerge;
			ipRule->SubAttributesToCompare = bstrSubAttributes;

			ipRule->MergeAttributeTreesInto = (UCLID_AFOUTPUTHANDLERSLib::EMergeAttributeTreesInto)
				(m_radMergeIntoFirst.GetCheck() == BST_CHECKED ?
				kFirstAttribute : kAttributeWithMostChildren);

			ipRule->DiscardNonMatchingComparisons =
				asVariantBool(m_radDiscardNonMatch.GetCheck() == BST_CHECKED);
			ipRule->CaseSensitive = asVariantBool(m_chkCaseSensitive.GetCheck() == BST_CHECKED);

			ipRule->RemoveEmptyHierarchy =
				asVariantBool(m_chkRemoveEmptyAttributes.GetCheck() == BST_CHECKED);

			// TODO: FUTURE implement this functionality fully
			//ipRule->CompareTypeInformation =
			//	asVariantBool(m_chkCompareTypeInfo.GetCheck() == BST_CHECKED);
			//ipRule->CompareSubAttributes =
			//	asVariantBool(m_chkCompareSubAttributes.GetCheck() == BST_CHECKED);
		}

		SetDirty(FALSE);

		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI26392");

	// If we reached here, it's because of an exception
	return S_FALSE;
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributeTreesPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI26393", pbValue != __nullptr);

		try
		{
			// Check the license
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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26394");
}

//--------------------------------------------------------------------------------------------------
// Private Methods
//--------------------------------------------------------------------------------------------------
void CMergeAttributeTreesPP::validateLicense()
{
	VALIDATE_LICENSE(gnRULESET_EDITOR_UI_OBJECT, "ELI26395", 
		"Merge attribute trees output handler PP");
}
//--------------------------------------------------------------------------------------------------
bool CMergeAttributeTreesPP::containsInvalidQueryString(const string &strAttributeQuery)
{
	return strAttributeQuery.find_first_of("|") != string::npos;
}
//--------------------------------------------------------------------------------------------------