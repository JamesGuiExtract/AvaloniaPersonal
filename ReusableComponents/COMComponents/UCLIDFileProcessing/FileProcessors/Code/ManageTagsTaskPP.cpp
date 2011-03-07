// ManageTagsTaskPP.cpp : Implementation of CManageTagsTaskPP

#include "stdafx.h"
#include "ManageTagsTaskPP.h"
#include "ManageTagsConstants.h"
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

			// Loop through the list of tags and add each checked item to the vector
			int lSize = m_listTags.GetItemCount();
			if (lSize > 0)
			{
				string strTags;
				for (int i=0; i < lSize; i++)
				{
					if (m_listTags.GetCheckState(i) == TRUE)
					{
						_bstr_t bstrTag;
						m_listTags.GetItemText(i, 0, bstrTag.GetBSTR());
						if (!strTags.empty())
						{
							strTags += gstrTAG_DELIMITER;
						}
						strTags += asString(bstrTag);
					}
				}

				// Ensure at least 1 item is checked
				if (strTags.empty())
				{
					MessageBox("At least 1 tag should be selected!", "No Tag Selected", MB_OK | MB_ICONERROR);
					m_listTags.SetFocus();

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
				ipManageTags->Tags = strTags.c_str();
				ipManageTags->Operation = eOpType;
			}
			else
			{
				// No tags in the database so list is empty, show message and do not
				// allow configuration [LRCAU #5448]
				MessageBox("There are no tags in the database, this task cannot be configured.",
					"No Tags", MB_OK | MB_ICONINFORMATION);
				return S_FALSE;
			}
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

		// Select tags from the manage tags task
		selectTags(ipManageTags);

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

		// Check if need to add space for scroll bar
		if (lSize > m_listTags.GetCountPerPage())
		{
			// Get the scroll bar width, if 0 and error occurred, just log an
			// application trace and set the width to 17
			int nVScrollWidth = GetSystemMetrics(SM_CXVSCROLL);
			if (nVScrollWidth == 0)
			{
				UCLIDException ue("ELI27581", "Application Trace: Unable to determine scroll bar width.");
				ue.log();

				nVScrollWidth = 17;
			}

			// Update the column width for the scroll bar
			m_listTags.SetColumnWidth(0, m_listTags.GetColumnWidth(0) - nVScrollWidth);
		}

		// Add the tags to the list
		for (long i=0; i < lSize; i++)
		{
			m_listTags.InsertItem(i, asString(ipVecTags->Item[i].bstrVal).c_str());
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

	// Get the tag list
	m_listTags = GetDlgItem( IDC_LIST_TAGS );

	// Set the tag list style
	m_listTags.SetExtendedListViewStyle( 
		LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT | LVS_EX_CHECKBOXES );

	// Get the list size and set up the tag name column
	CRect rectList;
	m_listTags.GetClientRect(rectList);
	m_listTags.InsertColumn(0, "Tags", LVCFMT_LEFT, rectList.Width(), 0);
}
//-------------------------------------------------------------------------------------------------
void CManageTagsTaskPP::selectTags(const UCLID_FILEPROCESSORSLib::IManageTagsTaskPtr& ipManageTags)
{
	try
	{
		ASSERT_ARGUMENT("ELI27505", ipManageTags != NULL);

		// Get the list of tags from the object
		string strTags = asString(ipManageTags->Tags);
		vector<string> vecTags;
		StringTokenizer::sGetTokens(strTags, gstrTAG_DELIMITER, vecTags);

		// Only select tags if the list is not NULL (unconfigured object starts with NULL tags)
		if (!vecTags.empty())
		{
			// Set the check state for each tag in the list
			vector<string> vecTagsNotFound;
			LVFINDINFO info;
			info.flags = LVFI_STRING;
			for (vector<string>::iterator it = vecTags.begin(); it != vecTags.end(); it++)
			{
				// Find each value in the list
				info.psz = it->c_str();
				int iIndex = m_listTags.FindItem(&info, -1);
				if (iIndex == -1)
				{
					// Tag was not found, add to the list of not found
					vecTagsNotFound.push_back(*it);
				}
				else
				{
					// Set this item as checked
					m_listTags.SetCheckState(iIndex, TRUE);
				}
			}

			// Prompt user about tags that no longer exist
			if (vecTagsNotFound.size() > 0)
			{
				string strMessage = "The following tag(s) no longer exist in the database:\n";
				for (vector<string>::iterator it = vecTagsNotFound.begin();
					it != vecTagsNotFound.end(); it++)
				{
					strMessage += (*it) + "\n";
				}

				MessageBox(strMessage.c_str(), "Tags Not Found", MB_OK | MB_ICONINFORMATION);
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27507");
}
//-------------------------------------------------------------------------------------------------
void CManageTagsTaskPP::validateLicense()
{
	VALIDATE_LICENSE( gnFILE_ACTION_MANAGER_OBJECTS, "ELI27508", 
		"Manage Tags Task PP" );
}
//-------------------------------------------------------------------------------------------------