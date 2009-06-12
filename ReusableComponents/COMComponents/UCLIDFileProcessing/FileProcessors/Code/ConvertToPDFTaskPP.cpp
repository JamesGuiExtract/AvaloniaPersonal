// ConvertToPDFTaskPP.cpp : Implementation of the CConvertToPDFTaskPP property page class.

#include "stdafx.h"
#include "FileProcessors.h"
#include "ConvertToPDFTaskPP.h"
#include "FileProcessorsUtils.h"

#include <UCLIDException.h>
#include <FileDialogEx.h>
#include <LoadFileDlgThread.h>
#include <ComponentLicenseIDs.h>
#include <LicenseMgmt.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------

// PDF file extensions string
const char* gpszPDF_FILE_EXTS = "PDF Files (*.pdf)|*.pdf||";

//-------------------------------------------------------------------------------------------------
// CConvertToPDFTaskPP
//-------------------------------------------------------------------------------------------------
CConvertToPDFTaskPP::CConvertToPDFTaskPP() 
{
	try
	{
		// check licensing
		validateLicense();
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI18776");
}
//-------------------------------------------------------------------------------------------------
CConvertToPDFTaskPP::~CConvertToPDFTaskPP() 
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI18777");
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConvertToPDFTaskPP::raw_IsLicensed(VARIANT_BOOL* pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// check parameter
		ASSERT_ARGUMENT("ELI18778", pbValue != NULL);

		try
		{
			// check license
			validateLicense();

			// if no exception was thrown, then the license is valid
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18779");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CConvertToPDFTaskPP::Apply()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
		
	try
	{
		// check licensing
		validateLicense();

		// get the input image filename from the edit box
		_bstr_t bstrInputImage;
		m_editInputImage.GetWindowText(bstrInputImage.GetAddress());

		// ensure the input image filename is non-empty
		if(bstrInputImage.length() == 0)
		{
			AfxMessageBox("Please enter an input image filename.", MB_ICONWARNING);
			return S_FALSE;
		}

		// create a FAM tag manager object
		UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr ipFAMTagManager;
		ipFAMTagManager.CreateInstance(CLSID_FAMTagManager);
		ASSERT_RESOURCE_ALLOCATION("ELI18780", ipFAMTagManager != NULL);

		// ensure the input image file is in a valid format
		if(ipFAMTagManager->StringContainsInvalidTags(bstrInputImage) == VARIANT_TRUE)
		{
			AfxMessageBox("Input image filename contains invalid tags.", MB_ICONWARNING);
			return S_FALSE;
		}

		// set the options of the associated objects accordingly
		for(UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_FILEPROCESSORSLib::IConvertToPDFTaskPtr ipConvertToPDFTask(m_ppUnk[i]);
			ASSERT_RESOURCE_ALLOCATION("ELI18781", ipConvertToPDFTask != NULL);

			ipConvertToPDFTask->SetOptions(bstrInputImage);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18782");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Message Handlers
//-------------------------------------------------------------------------------------------------
LRESULT CConvertToPDFTaskPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// get the ConvertToPDFTask associated with this property page
		// NOTE: this assumes only one coclass is associated with this property page
		UCLID_FILEPROCESSORSLib::IConvertToPDFTaskPtr ipConvertToPDFTask(m_ppUnk[0]);
		ASSERT_RESOURCE_ALLOCATION("ELI18783", ipConvertToPDFTask != NULL);

		// get the input image file controls
		m_editInputImage = GetDlgItem(IDC_EDIT_CONVERT_TO_PDF_INPUT_IMAGE);
		m_btnInputImageDocTag.SubclassDlgItem(IDC_BTN_CONVERT_TO_PDF_INPUT_IMAGE_DOC_TAG,
			CWnd::FromHandle(m_hWnd));
		m_btnInputImageDocTag.SetIcon(::LoadIcon(_Module.m_hInstResource, 
			MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));
		m_btnInputImageBrowse = GetDlgItem(IDC_BTN_CONVERT_TO_PDF_BROWSE_INPUT_IMAGE);

		// get the Convert to PDF task's options
		_bstr_t bstrInputImage;
		ipConvertToPDFTask->GetOptions( bstrInputImage.GetAddress() );

		// set the input image filename
		m_editInputImage.SetWindowText(bstrInputImage);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18784");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CConvertToPDFTaskPP::OnClickedBtnInputImageDocTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, 
														 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// get the position of the input image doc tag button
		RECT rect;
		m_btnInputImageDocTag.GetWindowRect(&rect);

		// display the doc tag menu and get the user's selection
		string strChoice = CFileProcessorsUtils::ChooseDocTag(m_hWnd, rect.right, rect.top);

		// if the user selected a tag, add it to the input image filename edit control
		if (strChoice != "")
		{
			m_editInputImage.ReplaceSel(strChoice.c_str(), TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19217");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CConvertToPDFTaskPP::OnClickedBtnInputImageBrowse(WORD wNotifyCode, WORD wID, HWND hWndCtl, 
														 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// create the input image file dialog
		CFileDialogEx fileDlg(TRUE, NULL, "", OFN_HIDEREADONLY | OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR,
			gpszPDF_FILE_EXTS, CWnd::FromHandle(m_hWnd));
	
		// prompt the user to select an input image file
		ThreadFileDlg tfd(&fileDlg);
		if (tfd.doModal() == IDOK)
		{
			// set the input image filename edit control to the user-selected file
			m_editInputImage.SetWindowText( fileDlg.GetPathName() );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19218");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CConvertToPDFTaskPP::validateLicense()
{
	VALIDATE_LICENSE(gnCREATE_SEARCHABLE_PDF_FEATURE, "ELI18787", "ConvertToPDFTask Property Page");
}
//-------------------------------------------------------------------------------------------------