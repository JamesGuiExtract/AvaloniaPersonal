// MicrFinderPP.cpp : Implementation of CMicrFinderPP

#include "stdafx.h"
#include "MicrFinderPP.h"
#include "..\..\AFCore\Code\AFCategories.h"
#include "RequiredInterfaces.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CMicrFinderPP
//-------------------------------------------------------------------------------------------------
CMicrFinderPP::CMicrFinderPP()
{
	try
	{
		// Check licensing
		validateLicense();

		m_dwTitleID = IDS_TITLEMICRFINDERPP;
		m_dwHelpFileID = IDS_HELPFILEMICRFINDERPP;
		m_dwDocStringID = IDS_DOCSTRINGMICRFINDERPP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI24367");
}
//-------------------------------------------------------------------------------------------------
HRESULT CMicrFinderPP::FinalConstruct()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
void CMicrFinderPP::FinalRelease()
{
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMicrFinderPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ATLTRACE(_T("CMicrFinderPP::Apply\n"));
		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_AFVALUEFINDERSLib::IMicrFinderPtr ipMicrFinder = m_ppUnk[0];
			if (ipMicrFinder)
			{
				ipMicrFinder->SplitRoutingNumber = 
					asVariantBool(m_checkSplitRoutingNumber.GetCheck() == BST_CHECKED);
				ipMicrFinder->SplitAccountNumber = 
					asVariantBool(m_checkSplitAccountNumber.GetCheck() == BST_CHECKED);
				ipMicrFinder->SplitCheckNumber = 
					asVariantBool(m_checkSplitCheckNumber.GetCheck() == BST_CHECKED);
				ipMicrFinder->SplitAmount = 
					asVariantBool(m_checkSplitAmount.GetCheck() == BST_CHECKED);
				ipMicrFinder->Rotate0 =
					asVariantBool(m_checkRotate0.GetCheck() == BST_CHECKED);
				ipMicrFinder->Rotate90 =
					asVariantBool(m_checkRotate90.GetCheck() == BST_CHECKED);
				ipMicrFinder->Rotate180 =
					asVariantBool(m_checkRotate180.GetCheck() == BST_CHECKED);
				ipMicrFinder->Rotate270 =
					asVariantBool(m_checkRotate270.GetCheck() == BST_CHECKED);
			}
		}
		m_bDirty = FALSE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24368");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CMicrFinderPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, 
									 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		if (m_nObjects > 0)
		{
			UCLID_AFVALUEFINDERSLib::IMicrFinderPtr ipMicrFinder = m_ppUnk[0];
			if (ipMicrFinder)
			{
				// Set up controls
				m_checkSplitRoutingNumber = GetDlgItem(IDC_CHECK_SPLIT_ROUTING);
				m_checkSplitAccountNumber = GetDlgItem(IDC_CHECK_SPLIT_ACCOUNT);
				m_checkSplitCheckNumber = GetDlgItem(IDC_CHECK_SPLIT_CHECK);
				m_checkSplitAmount = GetDlgItem(IDC_CHECK_SPLIT_AMOUNT);
				m_checkRotate0 = GetDlgItem(IDC_CHECK_0_ROTATION);
				m_checkRotate90 = GetDlgItem(IDC_CHECK_90_ROTATION);
				m_checkRotate180 = GetDlgItem(IDC_CHECK_180_ROTATION);
				m_checkRotate270 = GetDlgItem(IDC_CHECK_270_ROTATION);

				m_checkSplitRoutingNumber.SetCheck(asBSTChecked(ipMicrFinder->SplitRoutingNumber));
				m_checkSplitAccountNumber.SetCheck(asBSTChecked(ipMicrFinder->SplitAccountNumber));
				m_checkSplitCheckNumber.SetCheck(asBSTChecked(ipMicrFinder->SplitCheckNumber));
				m_checkSplitAmount.SetCheck(asBSTChecked(ipMicrFinder->SplitAmount));
				m_checkRotate0.SetCheck(asBSTChecked(ipMicrFinder->Rotate0));
				m_checkRotate90.SetCheck(asBSTChecked(ipMicrFinder->Rotate90));
				m_checkRotate180.SetCheck(asBSTChecked(ipMicrFinder->Rotate180));
				m_checkRotate270.SetCheck(asBSTChecked(ipMicrFinder->Rotate270));
			}
		}			
		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24369");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CMicrFinderPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		ASSERT_ARGUMENT("ELI24370", pbValue != __nullptr);

		try
		{
			// check the license
			validateLicense();

			// If no exception, then pbValue is true
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24371");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CMicrFinderPP::validateLicense()
{
	// Property Page requires Editor license
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI24372", 
		"MICR finder PP" );
}
//-------------------------------------------------------------------------------------------------
