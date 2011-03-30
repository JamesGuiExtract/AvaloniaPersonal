// TagConditionPP.cpp : Implementation of CTagConditionPP

#include "stdafx.h"
#include "TagConditionPP.h"
#include "..\..\Code\FPCategories.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>

#include <string>
#include <vector>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
static const int giCONSIDER_MET = 0;
static const int giCONSIDER_NOT_MET = 1;

static const int giANY_TAGS = 0;
static const int giALL_TAGS = 1;

//-------------------------------------------------------------------------------------------------
// CTagConditionPP
//-------------------------------------------------------------------------------------------------
CTagConditionPP::CTagConditionPP() 
{
}
//-------------------------------------------------------------------------------------------------
CTagConditionPP::~CTagConditionPP()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI27566");
}
//-------------------------------------------------------------------------------------------------
HRESULT CTagConditionPP::FinalConstruct()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
void CTagConditionPP::FinalRelease()
{
}

//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CTagConditionPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Obtain interface pointer to the ITagCondition class
		EXTRACT_FAMCONDITIONSLib::ITagConditionPtr ipTagCondition = m_ppUnk[0];
		ASSERT_RESOURCE_ALLOCATION("ELI27567", ipTagCondition);

		// Map controls to member variables
		m_cmbConsiderMet	= GetDlgItem(IDC_TAG_COMBO_MET);
		m_cmbAnyAll			= GetDlgItem(IDC_TAG_COMBO_ANY);
		m_listTags			= GetDlgItem(IDC_TAG_LIST_TAGS);

		// Populate the consider met combo box
		m_cmbConsiderMet.InsertString(giCONSIDER_MET, "met");
		m_cmbConsiderMet.InsertString(giCONSIDER_NOT_MET, "not met");

		// Populate the any/all combo box
		m_cmbAnyAll.InsertString(giANY_TAGS, "any");
		m_cmbAnyAll.InsertString(giALL_TAGS, "all");

		IFileProcessingDBPtr ipDB(CLSID_FileProcessingDB);
		ASSERT_RESOURCE_ALLOCATION("ELI27568", ipDB != __nullptr);

		// Connect to the last used DB
		ipDB->ConnectLastUsedDBThisProcess();

		// Set up the list control
		setUpListContrl(ipDB);

		// Upate the UI from the condition object
		updateUIFromTagCondition(ipTagCondition);

		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27569");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTagConditionPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ATLTRACE(_T("CTagConditionPP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			// Obtain interface pointer to the ITagCondition class
			EXTRACT_FAMCONDITIONSLib::ITagConditionPtr ipTagCondition = m_ppUnk[i];
			ASSERT_RESOURCE_ALLOCATION("ELI27570", ipTagCondition != __nullptr);

			IVariantVectorPtr ipVecTags(CLSID_VariantVector);
			ASSERT_RESOURCE_ALLOCATION("ELI27571", ipVecTags != __nullptr);

			// Loop through each item in the list and add all checked
			// items to the vector of tags
			int lSize = m_listTags.GetItemCount();
			for (int i=0; i < lSize; i++)
			{
				if (m_listTags.GetCheckState(i) == TRUE)
				{
					_bstr_t bstrTag;
					m_listTags.GetItemText(i, 0, bstrTag.GetBSTR());

					ipVecTags->PushBack(_variant_t(bstrTag));
				}
			}

			// Ensure at least 1 item is checked
			if (ipVecTags->Size == 0)
			{
				MessageBox("At least 1 tag should be selected!", "No Tag Selected", MB_OK | MB_ICONERROR);
				m_listTags.SetFocus();

				return S_FALSE;
			}

			// Store the values to the tag condition
			ipTagCondition->Tags = ipVecTags;
			ipTagCondition->ConsiderMet = asVariantBool(m_cmbConsiderMet.GetCurSel() == giCONSIDER_MET);
			ipTagCondition->AnyTags = asVariantBool(m_cmbAnyAll.GetCurSel() == giANY_TAGS);
		}

		SetDirty(FALSE);

		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI27572");

	// If we reached here, it's because of an exception
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CTagConditionPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI27573", pbValue != __nullptr);

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

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27574");
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void CTagConditionPP::validateLicense()
{
	VALIDATE_LICENSE(gnFLEXINDEX_IDSHIELD_CORE_OBJECTS, "ELI27575", "Tag Condition PP");
}
//-------------------------------------------------------------------------------------------------
void CTagConditionPP::setUpListContrl(const IFileProcessingDBPtr& ipDB)
{
	try
	{
		// Get the list of tags from the database
		IVariantVectorPtr ipVecTags = ipDB->GetTagNames();
		ASSERT_RESOURCE_ALLOCATION("ELI27576", ipVecTags != __nullptr);

		// Get the size of the tags vector
		long lSize = ipVecTags->Size;

		// Set the tag list style
		m_listTags.SetExtendedListViewStyle( 
			LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT | LVS_EX_CHECKBOXES );

		// Check if need to add space for scroll bar
		int nVScrollWidth = 0;
		if (lSize > m_listTags.GetCountPerPage())
		{
			// Get the scroll bar width, if 0 and error occurred, just log an
			// application trace and set the width to 17
			nVScrollWidth = GetSystemMetrics(SM_CXVSCROLL);
			if (nVScrollWidth == 0)
			{
				UCLIDException ue("ELI27580", "Application Trace: Unable to determine scroll bar width.");
				ue.log();

				nVScrollWidth = 17;
			}
		}

		// Set up the tag name column (adding space for scroll bar if necessary)
		CRect rectList;
		m_listTags.GetClientRect(rectList);
		m_listTags.InsertColumn(0, "Tags", LVCFMT_LEFT, rectList.Width() - nVScrollWidth, 0);

		// Insert each tag into the list control
		for (long i = 0; i < lSize; i++)
		{
			m_listTags.InsertItem(i, asString(ipVecTags->Item[i].bstrVal).c_str());
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27579");
}
//-------------------------------------------------------------------------------------------------
void CTagConditionPP::updateUIFromTagCondition(
	const EXTRACT_FAMCONDITIONSLib::ITagConditionPtr& ipTagCondition)
{
	try
	{
		// Select the appropriate values in the combo boxes
		m_cmbConsiderMet.SetCurSel(
			ipTagCondition->ConsiderMet == VARIANT_TRUE ? giCONSIDER_MET : giCONSIDER_NOT_MET);
		m_cmbAnyAll.SetCurSel(ipTagCondition->AnyTags == VARIANT_TRUE ? giANY_TAGS : giALL_TAGS);

		// Get the tags from the object
		IVariantVectorPtr ipVecTags = ipTagCondition->Tags;
		if (ipVecTags != __nullptr)
		{
			// Get the size
			long lSize = ipVecTags->Size;

			// Set the check state for each tag in the list
			vector<string> vecTagsNotFound;
			LVFINDINFO info;
			info.flags = LVFI_STRING;
			for (long i=0; i < lSize; i++)
			{
				string strTagName = asString(ipVecTags->Item[i].bstrVal);

				// Find each value in the list
				info.psz = strTagName.c_str();
				int iIndex = m_listTags.FindItem(&info, -1);
				if (iIndex == -1)
				{
					// Tag was not found, add to the list of not found
					vecTagsNotFound.push_back(strTagName);
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
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27577");
}
//-------------------------------------------------------------------------------------------------
