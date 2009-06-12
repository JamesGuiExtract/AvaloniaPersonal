// InputCorrectionUI.cpp : Implementation of CInputCorrectionUI
#include "stdafx.h"
#include "IFCore.h"
#include "InputCorrectionUI.h"
#include "InputCorrectionDlg.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputCorrectionUI::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IInputCorrectionUI
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IInputCorrectionUI
//-------------------------------------------------------------------------------------------------
CInputCorrectionUI::CInputCorrectionUI()
:m_lParentWndHandle(0)
{
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputCorrectionUI::PromptForCorrection(IInputValidator *pValidator, 
													 ITextInput *pTextInput, 
													 VARIANT_BOOL *pbSuccess)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check for valid license
		validateLicense();

		//bring up the input correction dialog
		CWnd *pParentWnd = m_lParentWndHandle == NULL ? NULL : CWnd::FromHandle((HWND) m_lParentWndHandle);
		InputCorrectionDlg inputCorrection(pValidator, pTextInput, pParentWnd);
		int res = inputCorrection.DoModal();
		
		if (res == IDOK)
		{
			*pbSuccess = VARIANT_TRUE;
		}
		else
		{
			*pbSuccess = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03806")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputCorrectionUI::get_ParentWndHandle(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check for valid license
		validateLicense();

		*pVal = m_lParentWndHandle;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03782")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputCorrectionUI::put_ParentWndHandle(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check for valid license
		validateLicense();

		m_lParentWndHandle = newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03783")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputCorrectionUI::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	try
	{
		validateLicense();

		// If validateLicense doesn't throw any exception, then pbValue is true
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CInputCorrectionUI::validateLicense()
{
	static const unsigned long INPUT_CORRECTION_UI_COMPONENT_ID = gnINPUTFUNNEL_CORE_OBJECTS;

	VALIDATE_LICENSE( INPUT_CORRECTION_UI_COMPONENT_ID, 
		"ELI02602", "Input Correction UI" );
}
//-------------------------------------------------------------------------------------------------
