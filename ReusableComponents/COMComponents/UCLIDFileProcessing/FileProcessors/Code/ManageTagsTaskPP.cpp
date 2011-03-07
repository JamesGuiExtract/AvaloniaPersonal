// ManageTagsTaskPP.cpp : Implementation of CManageTagsTaskPP

#include "stdafx.h"
#include "ManageTagsTaskPP.h"
#include "ManageTagsConstants.h"
#include "FileProcessorsUtils.h"
#include "..\..\..\UCLIDFileProcessing\Code\FPCategories.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <COMUtils.h>
#include <cpputil.h>
#include <StringTokenizer.h>

//-------------------------------------------------------------------------------------------------
// CManageTagsTaskPP
//-------------------------------------------------------------------------------------------------
CManageTagsTaskPP::CManageTagsTaskPP()
{
	try
	{
		// Check licensing
		validateLicense();
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI27492")
}
//--------------------------------------------------------------------------------------------------
CManageTagsTaskPP::~CManageTagsTaskPP()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI27493")
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CManageTagsTaskPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Ensure the return value pointer is non-NULL
		ASSERT_ARGUMENT("ELI27494", pbValue != NULL);

		try
		{
			// Check license
			validateLicense();

			// If no exception was thrown, then pbValue is true
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27495");
}

//--------------------------------------------------------------------------------------------------
// IPropertyPage
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CManageTagsTaskPP::Apply()
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ATLTRACE(_T("CManageTagsTaskPP::Apply\n"));

		// Update the settings in each of the objects associated with this UI
		for (UINT i = 0; i < m_nObjects; i++)
		{
			// Get the ManageTagsTask associated with this property page
			// NOTE: this assumes only one coclass is associated with this property page
			UCLID_FILEPROCESSORSLib::IManageTagsTaskPtr ipManageTags(m_ppUnk[0]);
			ASSERT_RESOURCE_ALLOCATION("ELI27496", ipManageTags != NULL);

			CString zTags;
			m_comboTags.GetWindowText(zTags);

			if (zTags.IsEmpty())
			{
				MessageBox("At least one tag should be selected.", "No Tag Selected",
					MB_OK | MB_ICONERROR);
				m_comboTags.SetFocus();

				return S_FALSE;
			}

			// Get the appropriate operation
			UCLID_FILEPROCESSORSLib::EManageTagsOperationType eOpType;
			if (m_radioAddTags.GetCheck() == BST_CHECKED)
			{
				eOpType = (UCLID_FILEPROCESSORSLib::EManageTagsOperationType) kOperationApplyTags;
			}
			else if (m_radioRemoveTags.GetCheck() == BST_CHECKED)
			{
				eOpType = (UCLID_FILEPROCESSORSLib::EManageTagsOperationType) kOperationRemoveTags;
			}
			else if (m_radioToggleTags.GetCheck() == BST_CHECKED)
			{
				eOpType = (UCLID_FILEPROCESSORSLib::EManageTagsOperationType) kOperationToggleTags;
			}
			else
			{
				THROW_LOGIC_ERROR_EXCEPTION("ELI27498");
			}

			// Set the tags and the operation
			ipManageTags->Tags = (LPCTSTR)zTags;
			ipManageTags->Operation = eOpType;
		}

		SetDirty(FALSE);
		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27499")

	// An exception was caught
	return S_FALSE;
}

//--------------------------------------------------------------------------------------------------
// Message handlers
//--------------------------------------------------------------------------------------------------
LRESULT CManageTagsTaskPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Get the ManageTagsTask associated with this property page
		// NOTE: this assumes only one coclass is associated with this property page
		UCLID_FILEPROCESSORSLib::IManageTagsTaskPtr ipManageTags(m_ppUnk[0]);
		ASSERT_RESOURCE_ALLOCATION("ELI27500", ipManageTags != NULL);

		// Create a database manager object so that we can retrieve
		// the tags stored in the database
		IFileProcessingDBPtr ipDB(CLSID_FileProcessingDB);
		ASSERT_RESOURCE_ALLOCATION("ELI27501", ipDB != NULL);

		// Connect database using last used settings in this instance
		ipDB->ConnectLastUsedDBThisProcess();

		// Prepare controls
		prepareControls();

		// Load the tags list from the database
		loadTagsFromDatabase(ipDB);

		// Set the current tag value in the drop down
		m_comboTags.SetWindowText(asString(ipManageTags->Tags).c_str());

		// Select the appropriate radio button
		switch(ipManageTags->Operation)
		{
		case kOperationApplyTags:
			m_radioAddTags.SetCheck(BST_CHECKED);
			break;

		case kOperationRemoveTags:
			m_radioRemoveTags.SetCheck(BST_CHECKED);
			break;

		case kOperationToggleTags:
			m_radioToggleTags.SetCheck(BST_CHECKED);
			break;

		default:
			THROW_LOGIC_ERROR_EXCEPTION("ELI27509");
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27502")

	return TRUE;
}
//-------------------------------------------------------------------------------------------------
LRESULT CManageTagsTaskPP::OnClickedBtnTagsDocTags(WORD wNotifyCode, WORD wID,
													  HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// get the position of the input image doc tag button
		RECT rect;
		m_btnTagsDocTags.GetWindowRect(&rect);

		// display the doc tag menu and get the user's selection
		string strChoice = CFileProcessorsUtils::ChooseDocTag(m_hWnd, rect.right, rect.top);

		// if the user selected a tag, add it to the input image filename edit control
		if (strChoice != "")
		{
			m_comboTags.Clear();
			CString zText;
			m_comboTags.GetWindowText(zText);
			zText.Delete(LOWORD(m_dwComboTagsSel),
				HIWORD(m_dwComboTagsSel) - LOWORD(m_dwComboTagsSel));
			zText.Insert(LOWORD(m_dwComboTagsSel), strChoice.c_str());
			m_comboTags.SetWindowText(zText);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI31988");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CManageTagsTaskPP::OnCbnSelEndCancelCmbTags(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	// Save the location of the current edit selection
	// It includes the starting and end position of the selection
	m_dwComboTagsSel = m_comboTags.GetEditSel();

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CManageTagsTaskPP::loadTagsFromDatabase(const IFileProcessingDBPtr& ipDB)
{
	try
	{
		// Get the tags from the database
		IVariantVectorPtr ipVecTags = ipDB->GetTagNames();
		ASSERT_RESOURCE_ALLOCATION("ELI27503", ipVecTags != NULL);

		// Get the tag count
		long lSize = ipVecTags->Size;

		// Add the tags to the dropdown list
		for (long i=0; i < lSize; i++)
		{
			m_comboTags.AddString(asString(ipVecTags->Item[i].bstrVal).c_str());
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27504");
}
//-------------------------------------------------------------------------------------------------
void CManageTagsTaskPP::prepareControls()
{
	// Get radio buttons
	m_radioAddTags = GetDlgItem(IDC_RADIO_APPLY_TAG);
	m_radioRemoveTags = GetDlgItem(IDC_RADIO_REMOVE_TAG);
	m_radioToggleTags = GetDlgItem(IDC_RADIO_TOGGLE_TAG);
	m_comboTags = GetDlgItem(IDC_COMBO_TAGS);

	// get the doc tag button
	m_btnTagsDocTags.SubclassDlgItem(IDC_BTN_TAG_DOC_TAG,
		CWnd::FromHandle(m_hWnd));

	// set the icon for the doc tag buttons
	m_btnTagsDocTags.SetIcon(::LoadIcon(_Module.m_hInstResource, 
		MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));
}
//-------------------------------------------------------------------------------------------------
void CManageTagsTaskPP::validateLicense()
{
	VALIDATE_LICENSE( gnFILE_ACTION_MANAGER_OBJECTS, "ELI27508", 
		"Manage Tags Task PP" );
}
//-------------------------------------------------------------------------------------------------