// ProgressStatusDialog.cpp : Implementation of CProgressStatusDialog

#include "stdafx.h"
#include "Resource.h"
#include "ProgressStatusDialog.h"

#include <cpputil.h>
#include <COMUtils.h>
#include <TemporaryResourceOverride.h>
#include <UCLIDException.h>
#include <ComponentLicenseIDs.h>
#include <LicenseMgmt.h>

using namespace std;

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
const static string gstrDEFAULT_WINDOW_TITLE = "Progress Status";

//--------------------------------------------------------------------------------------------------
// CProgressStatusDialog
//--------------------------------------------------------------------------------------------------
CProgressStatusDialog::CProgressStatusDialog()
:m_apProgressStatusMFCDlg(NULL)
{
	try
	{
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI16254")
}
//--------------------------------------------------------------------------------------------------
CProgressStatusDialog::~CProgressStatusDialog()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16601")
}
//--------------------------------------------------------------------------------------------------
HRESULT CProgressStatusDialog::FinalConstruct()
{
	try
	{
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16253")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
void CProgressStatusDialog::FinalRelease()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16602")
}

//--------------------------------------------------------------------------------------------------
// ISupportErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CProgressStatusDialog::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IProgressStatusDialog,
		&IID_ILicensedComponent
	};

	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CProgressStatusDialog::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	if (pbValue == NULL)
	{
		return E_POINTER;
	}

	try
	{
		// validate license
		validateLicense();

		// Set to VARIANT_TRUE
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// IProgressStatusDialog
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CProgressStatusDialog::ShowModelessDialog(HANDLE hWndParent, BSTR strWindowTitle, 
	IProgressStatus *pProgressStatus, long nNumProgressLevels, long nDelayBetweenRefreshes, 
	VARIANT_BOOL bShowCloseButton, HANDLE hStopEvent, long *phWndProgressStatusDialog)
{
	try
	{
		// Validate the license
		validateLicense();

		// Create the modeless dialog if it hasn't already been created
		if (m_apProgressStatusMFCDlg.get() == NULL)
		{
			// Temporarily override the current resource instance so that the local resources 
			// can be used for creating the dialog
			TemporaryResourceOverride ro(_Module.m_hInstResource);

			// Compute the appropriate value for the parent window handle
			HWND hParent = (HWND) hWndParent;
			CWnd *pParentWnd = NULL;
			if (hParent)
			{
				pParentWnd = CWnd::FromHandle(hParent);
			}

			// Create the dialog object
			m_apProgressStatusMFCDlg = auto_ptr<CProgressStatusMFCDlg>(new CProgressStatusMFCDlg(pParentWnd, 
				nNumProgressLevels, nDelayBetweenRefreshes, asCppBool(bShowCloseButton), hStopEvent));
			ASSERT_RESOURCE_ALLOCATION("ELI16255", m_apProgressStatusMFCDlg.get() != NULL);
			
			// Call the Create method on the MFC dialog to create/instantiate it as a modeless dialog
			BOOL bRet = m_apProgressStatusMFCDlg->Create(CProgressStatusMFCDlg::IDD, pParentWnd);
			ASSERT_RESOURCE_ALLOCATION("ELI16256", bRet != FALSE);
		}

		// Set the window title
		getThisAsCOMPtr()->Title = strWindowTitle;

		// Set the progress status object
		getThisAsCOMPtr()->ProgressStatusObject = 
			(UCLID_COMUTILSLib::IProgressStatus*) pProgressStatus;

		// By default, the create method does not show the dialog
		// Show the MFC dialog
		m_apProgressStatusMFCDlg->ShowWindow(SW_SHOW);

		// Return the window handle
		*phWndProgressStatusDialog = (long) m_apProgressStatusMFCDlg->m_hWnd;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16247")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CProgressStatusDialog::get_ProgressStatusObject(IProgressStatus **pVal)
{
	try
	{
		// Make sure the dialog box object exists
		if (m_apProgressStatusMFCDlg.get() == NULL)
		{
			// This method should only be called after the ShowModelessDialog() call
			// has been made.
			throw UCLIDException("ELI16592", "Method called in wrong sequence!");
		}

		// Return a reference to the progress status object
		CComQIPtr<IProgressStatus> ipProgressStatus = m_apProgressStatusMFCDlg->getProgressStatusObject();
		*pVal = ipProgressStatus.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16248")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CProgressStatusDialog::put_ProgressStatusObject(IProgressStatus *newVal)
{
	try
	{
		// Make sure the dialog box object exists
		if (m_apProgressStatusMFCDlg.get() == NULL)
		{
			// This method should only be called after the ShowModelessDialog() call
			// has been made.
			throw UCLIDException("ELI16589", "Method called in wrong sequence!");
		}

		// Update the progress status object
		m_apProgressStatusMFCDlg->setProgressStatusObject(newVal);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16249")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CProgressStatusDialog::get_Title(BSTR *pVal)
{
	try
	{
		// Make sure the dialog box object exists
		if (m_apProgressStatusMFCDlg.get() == NULL)
		{
			// This method should only be called after the ShowModelessDialog() call
			// has been made.
			throw UCLIDException("ELI16591", "Method called in wrong sequence!");
		}

		// Return the window title
		CString zTitle;
		m_apProgressStatusMFCDlg->GetWindowTextA(zTitle);
		*pVal = _bstr_t((LPCTSTR) zTitle).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16250")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CProgressStatusDialog::put_Title(BSTR newVal)
{
	try
	{
		// Make sure the dialog box object exists
		if (m_apProgressStatusMFCDlg.get() == NULL)
		{
			// This method should only be called after the ShowModelessDialog() call
			// has been made.
			throw UCLIDException("ELI16590", "Method called in wrong sequence!");
		}
	
		// If the user passed in an empty string for the title, use a default
		// title string
		string strTitle = asString(newVal);
		if (strTitle.empty())
		{
			strTitle = gstrDEFAULT_WINDOW_TITLE;
		}

		// Update the dialog title
		m_apProgressStatusMFCDlg->SetWindowText(strTitle.c_str());
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16251")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CProgressStatusDialog::Close()
{
	try
	{
		// Make sure the dialog box object exists
		if (m_apProgressStatusMFCDlg.get() == NULL)
		{
			// This method should only be called after the ShowModelessDialog() call
			// has been made.
			throw UCLIDException("ELI16597", "Method called in wrong sequence!");
		}

		// Hide the progress status dialog
		m_apProgressStatusMFCDlg->ShowWindow(SW_HIDE);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16252")

	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// Private methods
//--------------------------------------------------------------------------------------------------
UCLID_COMUTILSLib::IProgressStatusDialogPtr CProgressStatusDialog::getThisAsCOMPtr()
{
	UCLID_COMUTILSLib::IProgressStatusDialogPtr ipThis(this);
	ASSERT_RESOURCE_ALLOCATION("ELI17773", ipThis != NULL);

	return ipThis;
}
//--------------------------------------------------------------------------------------------------
void CProgressStatusDialog::validateLicense()
{
	VALIDATE_LICENSE( gnEXTRACT_CORE_OBJECTS, "ELI16598", "ProgressStatusDialog" );
}
//-------------------------------------------------------------------------------------------------
