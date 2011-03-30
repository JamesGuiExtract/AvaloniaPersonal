//==================================================================================================
//
// COPYRIGHT (c) 2008 EXTRACT SYSTEMS, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	AddWatermarkTaskPP.cpp
//
// PURPOSE:	Implementation of the AddWatermarkTask property page
//
// AUTHORS:	Jeff Shergalis
//
//==================================================================================================

#include "stdafx.h"
#include "FileProcessors.h"
#include "AddWatermarkTaskPP.h"
#include "FileProcessorsUtils.h"

#include <UCLIDException.h>
#include <LoadFileDlgThread.h>
#include <ComponentLicenseIDs.h>
#include <LicenseMgmt.h>
#include <Misc.h>
#include <ComUtils.h>

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
// Default to all image files for the add watermark task
const string gstrFILE_FILTER =
	"All image files|*.bmp;*.rle;*.dib;*.rst;*.gp4;*.mil;*.cal;*.cg4;*.flc;*.fli;*.gif;"
	"*.jpg;*.jpeg;*.pcx;*.pct;*.png;*.tga;*.tif;*.tiff;*.pdf|"
	"BMP files (*.bmp;*.rle;*.dib)|*.bmp;*.rle;*.dib|"
	"GIF files (*.gif)|*.gif|"
	"JFIF files (*.jpg;*.jpeg)|*.jpg;*.jpeg|"
	"PCX files (*.pcx)|*.pcx|"
	"PICT files (*.pct)|*.pct|"
	"PNG files (*.png)|*.png|"
	"TIFF files (*.tif;*.tiff)|*.tif;*.tiff|"
	"PDF files (*.pdf)|*.pdf|"
	"All files (*.*)|*.*||";
//--------------------------------------------------------------------------------------------------
// CAddWatermarkTaskPP
//--------------------------------------------------------------------------------------------------
CAddWatermarkTaskPP::CAddWatermarkTaskPP() 
{
	try
	{
		// check licensing
		validateLicense();
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI19975");
}
//--------------------------------------------------------------------------------------------------
CAddWatermarkTaskPP::~CAddWatermarkTaskPP() 
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI19976");
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CAddWatermarkTaskPP::raw_IsLicensed(VARIANT_BOOL* pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// check parameter
		ASSERT_ARGUMENT("ELI19977", pbValue != __nullptr);

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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19978");

	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// IPropertyPage
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CAddWatermarkTaskPP::Apply()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
		
	try
	{
		// check licensing
		validateLicense();

		// create a FAM tag manager object
		UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr ipFAMTagManager;
		ipFAMTagManager.CreateInstance(CLSID_FAMTagManager);
		ASSERT_RESOURCE_ALLOCATION("ELI19979", ipFAMTagManager != __nullptr);

		// get the input image filename from the edit box
		_bstr_t bstrInputImage;
		m_editInputImage.GetWindowText(bstrInputImage.GetAddress());

		// ensure the input image filename is non-empty
		if(bstrInputImage.length() == 0)
		{
			AfxMessageBox("Please enter an input image filename.", MB_ICONWARNING);
			m_editInputImage.SetFocus();
			return S_FALSE;
		}

		// ensure the input image file is in a valid format
		if(ipFAMTagManager->StringContainsInvalidTags(bstrInputImage) == VARIANT_TRUE)
		{
			AfxMessageBox("Input image filename contains invalid tags.", MB_ICONWARNING);
			m_editInputImage.SetFocus();
			return S_FALSE;
		}

		_bstr_t bstrStampImage;
		m_editStampImage.GetWindowText(bstrStampImage.GetAddress());

		// ensure there is a stamp image file name entered
		if(bstrStampImage.length() == 0)
		{
			AfxMessageBox("Please enter a stamp image filename.", MB_ICONWARNING);
			m_editStampImage.SetFocus();
			return S_FALSE;
		}

		// ensure the stamp image file name has a valid format
		if(ipFAMTagManager->StringContainsInvalidTags(bstrStampImage) == VARIANT_TRUE)
		{
			AfxMessageBox("Stamp image filename contains invalid tags.", MB_ICONWARNING);
			m_editStampImage.SetFocus();
			return S_FALSE;
		}

		CString zTemp;
		
		// get and validate the horizontal percentage
		m_editHorizontalPercentage.GetWindowText(zTemp);
		double dHorizontalPercentage = 0.0;
		try
		{
			dHorizontalPercentage = asDouble(string(zTemp));

			// check for negative
			if (dHorizontalPercentage < 0.0 || dHorizontalPercentage >= 100.0)
			{
				AfxMessageBox("Horizontal percentage must be between 0 and 100!", MB_ICONWARNING);
				m_editHorizontalPercentage.SetFocus();
				return S_FALSE;
			}
		}
		catch(...)
		{
			AfxMessageBox("Horizontal percentage is an invalid decimal number!", MB_ICONWARNING);
			m_editHorizontalPercentage.SetFocus();
			return S_FALSE;
		}

		// get and validate the vertical percentage
		m_editVerticalPercentage.GetWindowText(zTemp);
		double dVerticalPercentage = 0.0;
		try
		{
			dVerticalPercentage = asDouble(string(zTemp));

			// check for negative
			if (dVerticalPercentage < 0.0 || dVerticalPercentage >= 100.0)
			{
				AfxMessageBox("Vertical percentage must be between 0 and 100!", MB_ICONWARNING);
				m_editVerticalPercentage.SetFocus();
				return S_FALSE;
			}
		}
		catch(...)
		{
			AfxMessageBox("Vertical percentage is an invalid decimal number!", MB_ICONWARNING);
			m_editVerticalPercentage.SetFocus();
			return S_FALSE;
		}

		// get the pages to stamp string
		string strPagesToStamp;
		if (m_radioFirstPage.GetCheck() == BST_CHECKED)
		{
			strPagesToStamp = "1";
		}
		else if(m_radioLastPage.GetCheck() == BST_CHECKED)
		{
			strPagesToStamp = "-1";
		}
		else if(m_radioSpecifiedPage.GetCheck() == BST_CHECKED)
		{
			m_editSpecifiedPages.GetWindowText(zTemp);

			if (zTemp.IsEmpty())
			{
				AfxMessageBox("Please enter the page(s) to stamp.", MB_ICONWARNING);
				m_editSpecifiedPages.SetFocus();
				return S_FALSE;
			}

			strPagesToStamp = (LPCTSTR) zTemp;
			try
			{
				// Validate the page number string
				validatePageNumbers(strPagesToStamp);
			}
			catch(UCLIDException& uex)
			{
				// Display the exception to the user and set focus to the pages edit box
				uex.display();
				m_editSpecifiedPages.SetFocus();
				return S_FALSE;
			}
		}
		else
		{
			THROW_LOGIC_ERROR_EXCEPTION("ELI19980");
		}

		// save the settings to the AddWatermarkTask
		for(UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_FILEPROCESSORSLib::IAddWatermarkTaskPtr ipAddWatermark(m_ppUnk[i]);
			ASSERT_RESOURCE_ALLOCATION("ELI19981", ipAddWatermark != __nullptr);

			ipAddWatermark->InputImageFile = bstrInputImage;
			ipAddWatermark->StampImageFile = bstrStampImage;
			ipAddWatermark->HorizontalPercentage = dHorizontalPercentage;
			ipAddWatermark->VerticalPercentage = dVerticalPercentage;
			ipAddWatermark->PagesToStamp = strPagesToStamp.c_str();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19982");

	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// Message Handlers
//--------------------------------------------------------------------------------------------------
LRESULT CAddWatermarkTaskPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// get the AddWatermarkTask associated with this property page
		// NOTE: this assumes only one coclass is associated with this property page
		UCLID_FILEPROCESSORSLib::IAddWatermarkTaskPtr ipAddWatermarkTask(m_ppUnk[0]);
		ASSERT_RESOURCE_ALLOCATION("ELI19983", ipAddWatermarkTask != __nullptr);

		// create tooltip object
		m_infoTip.Create(CWnd::FromHandle(m_hWnd));
		// set no delay.
		m_infoTip.SetShowDelay(0);

		// get the controls from the property pages
		m_editInputImage = GetDlgItem(IDC_EDIT_WATERMARK_INPUT_IMAGE);
		m_btnInputImageBrowse = GetDlgItem(IDC_BTN_WATERMARK_BROWSE_INPUT_IMAGE);
		m_editStampImage = GetDlgItem(IDC_EDIT_WATERMARK_STAMP_IMAGE);
		m_btnStampImageBrowse = GetDlgItem(IDC_BTN_WATERMARK_BROWSE_STAMP_IMAGE);
		m_editHorizontalPercentage = GetDlgItem(IDC_EDIT_WATERMARK_HORIZONTAL_PERCENT);
		m_editVerticalPercentage = GetDlgItem(IDC_EDIT_WATERMARK_VERTICAL_PERCENT);
		m_radioFirstPage = GetDlgItem(IDC_RADIO_WATERMARK_FIRSTPAGE);
		m_radioLastPage = GetDlgItem(IDC_RADIO_WATERMARK_LASTPAGE);
		m_radioSpecifiedPage = GetDlgItem(IDC_RADIO_WATERMARK_SPECIFIEDPAGES);
		m_editSpecifiedPages = GetDlgItem(IDC_EDIT_WATERMARK_SPECIFIEDPAGES);

		// get the doc tag buttons
		m_btnInputImageDocTag.SubclassDlgItem(IDC_BTN_WATERMARK_INPUT_IMAGE_DOC_TAG, 
			CWnd::FromHandle(m_hWnd));
		m_btnStampImageDocTag.SubclassDlgItem(IDC_BTN_WATERMARK_STAMP_IMAGE_DOC_TAG, 
			CWnd::FromHandle(m_hWnd));

		// set the icon for the doc tag buttons
		m_btnInputImageDocTag.SetIcon(::LoadIcon(_Module.m_hInstResource, 
			MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));
		m_btnStampImageDocTag.SetIcon(::LoadIcon(_Module.m_hInstResource, 
			MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));
		
		// load the property page with the date from the AddWatermarkTask
		m_editInputImage.SetWindowText(ipAddWatermarkTask->InputImageFile);
		m_editStampImage.SetWindowText(ipAddWatermarkTask->StampImageFile);

		// disable the specified pages edit box by default
		m_editSpecifiedPages.EnableWindow(FALSE);

		double dHorizontalPercentage = ipAddWatermarkTask->HorizontalPercentage;
		if ( dHorizontalPercentage >= 0.0)
		{
			m_editHorizontalPercentage.SetWindowText(asString(dHorizontalPercentage, 2).c_str());
		}

		double dVerticalPercentage = ipAddWatermarkTask->VerticalPercentage;
		if (dVerticalPercentage >= 0.0)
		{
			m_editVerticalPercentage.SetWindowText(asString(dVerticalPercentage, 2).c_str());
		}

		string strPagesToStamp = asString(ipAddWatermarkTask->PagesToStamp);
		if (strPagesToStamp == "1")
		{
			m_radioFirstPage.SetCheck(BST_CHECKED);
		}
		else if (strPagesToStamp == "-1")
		{
			m_radioLastPage.SetCheck(BST_CHECKED);
		}
		else
		{
			m_radioSpecifiedPage.SetCheck(BST_CHECKED);
			m_editSpecifiedPages.SetWindowText(strPagesToStamp.c_str());
			
			// since there is a specified page, enable the edit box
			m_editSpecifiedPages.EnableWindow(TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19984");

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CAddWatermarkTaskPP::OnClickedBtnInputImageDocTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, 
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19985");

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CAddWatermarkTaskPP::OnClickedBtnInputImageBrowse(WORD wNotifyCode, WORD wID, HWND hWndCtl, 
														 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// create the input image file dialog
		CFileDialog fileDlg(TRUE, NULL, "", 
			OFN_HIDEREADONLY | OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR,
			gstrFILE_FILTER.c_str(), CWnd::FromHandle(m_hWnd));
	
		// prompt the user to select an input image file
		ThreadFileDlg tfd(&fileDlg);
		if (tfd.doModal() == IDOK)
		{
			// set the input image filename edit control to the user-selected file
			m_editInputImage.SetWindowText( fileDlg.GetPathName() );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19986");

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CAddWatermarkTaskPP::OnClickedBtnStampImageDocTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, 
														 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// get the position of the input image doc tag button
		RECT rect;
		m_btnStampImageDocTag.GetWindowRect(&rect);

		// display the doc tag menu and get the user's selection
		string strChoice = CFileProcessorsUtils::ChooseDocTag(m_hWnd, rect.right, rect.top);

		// if the user selected a tag, add it to the input image filename edit control
		if (strChoice != "")
		{
			m_editStampImage.ReplaceSel(strChoice.c_str(), TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19987");

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CAddWatermarkTaskPP::OnClickedBtnStampImageBrowse(WORD wNotifyCode, WORD wID, HWND hWndCtl, 
														 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// create the stamp image file dialog
		CFileDialog fileDlg(TRUE, NULL, "", 
			OFN_HIDEREADONLY | OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR,
			gstrFILE_FILTER.c_str(), CWnd::FromHandle(m_hWnd));
	
		// prompt the user to select an input image file
		ThreadFileDlg tfd(&fileDlg);
		if (tfd.doModal() == IDOK)
		{
			// set the stamp image filename edit control to the user-selected file
			m_editStampImage.SetWindowText( fileDlg.GetPathName() );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19988");

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CAddWatermarkTaskPP::OnClickedBtnRadioPage(WORD wNotifyCode, WORD wID, HWND hWndCtl,
												   BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// enable or disable the specified edit box based on the whether the specified
		// pages radio button is checked
		m_editSpecifiedPages.EnableWindow(
			asMFCBool(m_radioSpecifiedPage.GetCheck() == BST_CHECKED));
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19989");

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CAddWatermarkTaskPP::OnClickedSpecificPageInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// show tooltip info
		CString zText("Specify one or more pages. Page number must be greater than\n"
					  "or equal to 1. You can specify individual page number, a collection\n"
					  "of individual page numbers, a range of page numbers, or a mixture\n"
					  "of individual page numbers and range(s) of page numbers.\n"
					  "Use an integer followed by a hyphen (eg. \"4-\") to specify a range of\n"
					  "pages that the starting page is the integer, and the ending page is the\n"
					  "last page of the image.\n"
					  "Use a hyphen followed by a positive integer (eg. \"-3\") to specify last X\n"
					  "number of pages.\n"
					  "Any duplicate entries will be only counted once. All page numbers will\n"
					  "be sorted in an ascending fashion.\n\n"
					  "For instance, \"3\", \"1,4,6\", \"2-3\", \"2, 4-7, 9\", \"3-5, 6-8\", \"1,3,5-\", \"-2\"\n"
					  "are valid page numbers; \"6-2\", \"0, 2\", \"0-1\" are invalid.\n"
					  "\"1-6,2-4\" will be counted as page 1,2,3,4,5,6. \"-2\" will be last 2 pages of\n"
					  "original image. \"5,3,2\" will be same as \"2,3,5\"");
		m_infoTip.Show(zText);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI29156");

	return 0;
}

//--------------------------------------------------------------------------------------------------
// Private functions
//--------------------------------------------------------------------------------------------------
void CAddWatermarkTaskPP::validateLicense()
{
	VALIDATE_LICENSE(gnFILE_ACTION_MANAGER_OBJECTS, "ELI19990", "AddWatermarkTask Property Page");
}
//-------------------------------------------------------------------------------------------------