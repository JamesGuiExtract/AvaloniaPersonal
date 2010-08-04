// ReturnAddrFinderPP.cpp : Implementation of CReturnAddrFinderPP
#include "stdafx.h"
#include "AFValueFinders.h"
#include "ReturnAddrFinderPP.h"

#include "..\..\AFCore\Code\AFCategories.h"
#include "..\..\AFUtils\Code\Helper.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <EditorLicenseID.h>

#include <string>

using namespace std;

const int NUM_OF_CHARS = 4096;

//-------------------------------------------------------------------------------------------------
// CReturnAddrFinderPP
//-------------------------------------------------------------------------------------------------
CReturnAddrFinderPP::CReturnAddrFinderPP()
{
	m_dwTitleID = IDS_TITLEReturnAddrFinderPP;
	m_dwHelpFileID = IDS_HELPFILEReturnAddrFinderPP;
	m_dwDocStringID = IDS_DOCSTRINGReturnAddrFinderPP;
}
//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CReturnAddrFinderPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		ATLTRACE(_T("CReturnAddrFinderPP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			// get the SPM finder
			UCLID_AFVALUEFINDERSLib::IReturnAddrFinderPtr ipRetAddrFinder = m_ppUnk[i];

			// update the FindNonReturnAddresses
			//	ipSPMFinder->IsPatternsFromFile = m_bIsPatternsFromFile ? VARIANT_TRUE : VARIANT_FALSE;
			ipRetAddrFinder->PutFindNonReturnAddresses( (m_chkFindNonReturnAddresses.GetCheck() == 0) ? VARIANT_FALSE : VARIANT_TRUE );

		}
			
		// if we reached here, then the data was successfully transfered
		// from the UI to the object.
		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08939")

	// if we reached here, it's because of an exception
	// An error message has already been displayed to the user.
	// Return S_FALSE to indicate to the outer scope that the
	// Apply was not successful.
	return S_FALSE;
}
//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CReturnAddrFinderPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{

		m_chkFindNonReturnAddresses = GetDlgItem(IDC_CHECK_FIND_NON_RETURN_ADDRESSES);

		UCLID_AFVALUEFINDERSLib::IReturnAddrFinderPtr ipRetAddrFinder = m_ppUnk[0];

		if (ipRetAddrFinder)
		{
			// read patterns options
			bool bTmp = ipRetAddrFinder->GetFindNonReturnAddresses() == VARIANT_TRUE;
			CheckDlgButton(IDC_CHECK_FIND_NON_RETURN_ADDRESSES, bTmp?BST_CHECKED:BST_UNCHECKED);
		}

	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08941");
	return 0;
}
