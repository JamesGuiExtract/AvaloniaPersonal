// MergeAttributesPP.cpp : Implementation of CMergeAttributesPP

#include "stdafx.h"
#include "MergeAttributesPP.h"
#include "MergeAttributesPreferenceListDlg.h"
#include "..\..\AFUtils\Code\Helper.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>

//--------------------------------------------------------------------------------------------------
// CMergeAttributesPP
//--------------------------------------------------------------------------------------------------
CMergeAttributesPP::CMergeAttributesPP()
{
}
//--------------------------------------------------------------------------------------------------
CMergeAttributesPP::~CMergeAttributesPP()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI22782");
}
//--------------------------------------------------------------------------------------------------
HRESULT CMergeAttributesPP::FinalConstruct()
{
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
void CMergeAttributesPP::FinalRelease()
{
}

//--------------------------------------------------------------------------------------------------
// Windows message handlers
//--------------------------------------------------------------------------------------------------
LRESULT CMergeAttributesPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_AFOUTPUTHANDLERSLib::IMergeAttributesPtr ipRule = m_ppUnk[0];
		ASSERT_RESOURCE_ALLOCATION("ELI22783", ipRule);

		// Map controls to member variables
		m_editAttributeQuery		= GetDlgItem(IDC_EDIT_QUERY);
		m_editOverlapPercent		= GetDlgItem(IDC_EDIT_OVERLAP_PERCENT);
		m_btnSpecifyName			= GetDlgItem(IDC_RADIO_SPECIFY_NAME);
		m_btnPreserveName			= GetDlgItem(IDC_RADIO_PRESERVE_NAME);
		m_editNameList				= GetDlgItem(IDC_EDIT_NAME_LIST);
		m_btnEditNameList			= GetDlgItem(IDC_BUTTON_EDIT_NAME_LIST);
		m_btnValueFromName			= GetDlgItem(IDC_RADIO_VALUE_FROM_NAME);
		m_btnSpecifyValue			= GetDlgItem(IDC_RADIO_SPECIFY_VALUE); 
		m_btnPreserveValue			= GetDlgItem(IDC_RADIO_PRESERVE_VALUE);
		m_editValueList				= GetDlgItem(IDC_EDIT_VALUE_LIST);
		m_btnEditValueList			= GetDlgItem(IDC_BUTTON_EDIT_VALUE_LIST);
		m_btnSpecifyType			= GetDlgItem(IDC_RADIO_SPECIFY_TYPE);
		m_btnCombineType			= GetDlgItem(IDC_RADIO_COMBINE_TYPES);
		m_btnSelectType				= GetDlgItem(IDC_RADIO_SELECT_TYPE);
		m_btnTypeFromName			= GetDlgItem(IDC_CHECK_TYPE_FROM_NAME);
		m_btnPreserveType			= GetDlgItem(IDC_CHECK_PRESERVE_TYPE);
		m_editTypeList				= GetDlgItem(IDC_EDIT_TYPE_LIST);
		m_btnEditTypeList			= GetDlgItem(IDC_BUTTON_EDIT_TYPE_LIST);
		m_editSpecifiedName			= GetDlgItem(IDC_EDIT_NAME);
		m_editSpecifiedType			= GetDlgItem(IDC_EDIT_TYPE);
		m_editSpecifiedValue		= GetDlgItem(IDC_EDIT_VALUE);
		m_btnPreserveAsSubAttributes = GetDlgItem(IDC_CHECK_SUBATTRIBUTES);
		m_btnCreateMergedRegion		= GetDlgItem(IDC_RADIO_CREATE_MERGED_REGION);
		m_btnMergeIndividualZones	= GetDlgItem(IDC_RADIO_MERGE_INDIVIDUAL_ZONES);

		// Load the rule values into the property page.
		m_editAttributeQuery.SetWindowText(asString(ipRule->AttributeQuery).c_str());
		m_editOverlapPercent.SetWindowText(asString(ipRule->OverlapPercent, 0, 5).c_str());
		m_btnSpecifyName.SetCheck(asBSTChecked(ipRule->NameMergeMode == kSpecifyField));
		m_btnPreserveName.SetCheck(asBSTChecked(ipRule->NameMergeMode == kPreserveField));
		updateDelimetedList(m_vecNameMergePriority, m_editNameList, ipRule->NameMergePriority);
		m_bTreatNameListAsRegex = asCppBool(ipRule->TreatNameListAsRegex);
		m_btnValueFromName.SetCheck(asBSTChecked(ipRule->ValueMergeMode == kSelectField));
		m_btnSpecifyValue.SetCheck(asBSTChecked(ipRule->ValueMergeMode == kSpecifyField));
		m_btnPreserveValue.SetCheck(asBSTChecked(ipRule->ValueMergeMode == kPreserveField));
		updateDelimetedList(m_vecValueMergePriority, m_editValueList, ipRule->ValueMergePriority);
		m_bTreatValueListAsRegex = asCppBool(ipRule->TreatValueListAsRegex);
		m_btnSpecifyType.SetCheck(asBSTChecked(ipRule->TypeMergeMode == kSpecifyField));
		m_btnCombineType.SetCheck(asBSTChecked(ipRule->TypeMergeMode == kCombineField));
		m_btnSelectType.SetCheck(asBSTChecked(ipRule->TypeMergeMode == kSelectField));
		m_btnTypeFromName.SetCheck(asBSTChecked(ipRule->TypeFromName));
		m_btnPreserveType.SetCheck(asBSTChecked(ipRule->PreserveType));
		updateDelimetedList(m_vecTypeMergePriority, m_editTypeList, ipRule->TypeMergePriority);
		m_bTreatTypeListAsRegex = asCppBool(ipRule->TreatTypeListAsRegex);
		m_editSpecifiedName.SetWindowText(asString(ipRule->SpecifiedName).c_str());
		m_editSpecifiedType.SetWindowText(asString(ipRule->SpecifiedType).c_str());
		m_editSpecifiedValue.SetWindowText(asString(ipRule->SpecifiedValue).c_str());
		m_btnPreserveAsSubAttributes.SetCheck(asBSTChecked(ipRule->PreserveAsSubAttributes));
		m_btnCreateMergedRegion.SetCheck(asBSTChecked(ipRule->CreateMergedRegion));
		m_btnMergeIndividualZones.SetCheck(asBSTChecked(ipRule->CreateMergedRegion == VARIANT_FALSE));

		updateControls();

		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI22784");

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CMergeAttributesPP::OnBnClickedCheckOrRadioBtn(WORD wNotifyCode, WORD wID, HWND hWndCtl, 
													   BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// A radio button option has been changed.  Enable/disable controls to reflect the selected
		// option.
		updateControls();

		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI22866");

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CMergeAttributesPP::OnBnClickedButtonEditNameList(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Pop dialog allowing the user to edit the name merge priority list.
		if (CMergeAttributesPreferenceListDlg::EditList("Attribute name order of preference", 
			m_vecNameMergePriority, true, m_bTreatNameListAsRegex))
		{
			// If the dialog was ok'd apply the updated values to m_editNameList.
			updateDelimetedList(m_vecNameMergePriority, m_editNameList);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33058");

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CMergeAttributesPP::OnBnClickedButtonEditValueList(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Pop dialog allowing the user to edit the value merge priority list.
		if (CMergeAttributesPreferenceListDlg::EditList("Attribute value order of preference", 
			m_vecValueMergePriority, false, m_bTreatValueListAsRegex))
		{
			// If the dialog was ok'd apply the updated values to m_editValueList.
			updateDelimetedList(m_vecValueMergePriority, m_editValueList);

			// Warn that regex's may be necessary for the preservation list to be effective.
			if (!m_bTreatValueListAsRegex)
			{
				MessageBox("Unless the attributes being merged all have artificially assigned values, "
					"use regular expressions for the value preservation priority list for a better "
					"chance of selecting the appropriate value to use.", "Warning", MB_OK | MB_ICONWARNING);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33121");

	return 0;
}
//--------------------------------------------------------------------------------------------------
LRESULT CMergeAttributesPP::OnBnClickedButtonEditTypeList(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Pop dialog allowing the user to edit the type merge priority list.
		if (CMergeAttributesPreferenceListDlg::EditList("Attribute type order of preference",
			m_vecTypeMergePriority, true, m_bTreatTypeListAsRegex))
		{
			// If the dialog was ok'd apply the updated values to m_editNameList.
			updateDelimetedList(m_vecTypeMergePriority, m_editTypeList);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI33059");

	return 0;
}

//--------------------------------------------------------------------------------------------------
// IPropertyPage
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributesPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		ATLTRACE(_T("CMergeAttributesPP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			// Obtain interface pointer to the IMergeAttributes class
			UCLID_AFOUTPUTHANDLERSLib::IMergeAttributesPtr ipRule = m_ppUnk[i];
			ASSERT_RESOURCE_ALLOCATION("ELI22785", ipRule != __nullptr);

			// Apply settings to rule
			ipRule->AttributeQuery = verifyControlValueAsBSTR(m_editAttributeQuery,
				"Specify a query to select attributes that may be merged!");

			ipRule->OverlapPercent = verifyControlValueAsDouble(m_editOverlapPercent, 0, 100,
				"The minimum mutual overlap percentage must be in the range 0 - 100", 
				75,	"Specify a minimum mutual overlap percentage for merging");

			// Attribute name
			IVariantVectorPtr ipNamePrioirtyList(CLSID_VariantVector);
			for each (string strValue in m_vecNameMergePriority)
			{
				ipNamePrioirtyList->PushBack(strValue.c_str());
			}
			ipRule->NameMergePriority = ipNamePrioirtyList;

			ipRule->TreatNameListAsRegex = asVariantBool(m_bTreatNameListAsRegex);
			
			if ((m_btnSpecifyName.GetCheck() == BST_CHECKED))
			{
				// Get the specified name and validate it
				_bstr_t bstrName = verifyControlValueAsBSTR(m_editSpecifiedName,
					"Specify a name for the merged attribute!");
				try
				{
					validateIdentifier(asString(bstrName));
				}
				catch(...)
				{
					m_editSpecifiedName.SetFocus();
					throw;
				}

				ipRule->NameMergeMode = (EFieldMergeMode) kSpecifyField;
				ipRule->SpecifiedName = bstrName;
			}
			else
			{
				ipRule->NameMergeMode = (EFieldMergeMode) kPreserveField;
				ipRule->SpecifiedName = verifyControlValueAsBSTR(m_editSpecifiedName);

				if (m_vecNameMergePriority.size() == 0)
				{
					m_btnEditNameList.SetFocus();
					throw UCLIDException("ELI22942", "Specify at least one name to preserve!");
				}
			}

			// Attribute type
			if (m_btnSpecifyType.GetCheck() == BST_CHECKED)
			{
				_bstr_t bstrType = verifyControlValueAsBSTR(m_editSpecifiedType);

				if (bstrType.length() > 0)
				{
					try
					{
						validateIdentifier(asString(bstrType));
					}
					catch(...)
					{
						m_editSpecifiedType.SetFocus();
						throw;
					}
				}

				ipRule->TypeMergeMode = (EFieldMergeMode) kSpecifyField;
				ipRule->SpecifiedType = bstrType;
			}
			else 
			{
				if (m_btnCombineType.GetCheck() == BST_CHECKED)
				{
					ipRule->TypeMergeMode = (EFieldMergeMode) kCombineField;
				}
				else
				{
					ipRule->TypeMergeMode = (EFieldMergeMode) kSelectField;

					if (m_btnTypeFromName.GetCheck() == BST_UNCHECKED &&
						m_btnPreserveType.GetCheck() == BST_UNCHECKED)
					{
						throw UCLIDException("ELI33060", "Specify at least one type selection method!");
					}
				}

				ipRule->SpecifiedType = verifyControlValueAsBSTR(m_editSpecifiedType);
			}

			ipRule->TypeFromName = asVariantBool(m_btnTypeFromName.GetCheck() == BST_CHECKED);

			if (m_btnPreserveType.GetCheck() == BST_CHECKED)
			{
				ipRule->PreserveType = VARIANT_TRUE;
				
				if (m_btnSelectType.GetCheck() == BST_CHECKED && 
					m_vecTypeMergePriority.size() == 0)
				{
					m_btnEditTypeList.SetFocus();
					throw UCLIDException("ELI33061", "Specify at least one type to preserve!");
				}
			}
			else
			{
				ipRule->PreserveType = VARIANT_FALSE;
			}

			IVariantVectorPtr ipTypePrioirtyList(CLSID_VariantVector);
			for each (string strValue in m_vecTypeMergePriority)
			{
				ipTypePrioirtyList->PushBack(strValue.c_str());
			}
			ipRule->TypeMergePriority = ipTypePrioirtyList;

			ipRule->TreatTypeListAsRegex = asVariantBool(m_bTreatTypeListAsRegex);

			// Attribute value
			IVariantVectorPtr ipValuePrioirtyList(CLSID_VariantVector);
			for each (string strValue in m_vecValueMergePriority)
			{
				ipValuePrioirtyList->PushBack(strValue.c_str());
			}
			ipRule->ValueMergePriority = ipValuePrioirtyList;

			ipRule->TreatValueListAsRegex = asVariantBool(m_bTreatValueListAsRegex);

			if (m_btnSpecifyValue.GetCheck() == BST_CHECKED)
			{
				ipRule->ValueMergeMode = (EFieldMergeMode) kSpecifyField;
				ipRule->SpecifiedValue = verifyControlValueAsBSTR(m_editSpecifiedValue,
					"Specify a value for the merged attribute!");
			}
			else if (m_btnValueFromName.GetCheck() == BST_CHECKED)
			{
				ipRule->ValueMergeMode = (EFieldMergeMode) kSelectField;
				ipRule->SpecifiedValue = verifyControlValueAsBSTR(m_editSpecifiedValue);
			}
			else
			{
				ipRule->ValueMergeMode = (EFieldMergeMode) kPreserveField;
				ipRule->SpecifiedValue = verifyControlValueAsBSTR(m_editSpecifiedValue);

				if (m_vecValueMergePriority.size() == 0)
				{
					m_btnEditNameList.SetFocus();
					throw UCLIDException("ELI33120", "Specify at least one value to preserve!");
				}
			}
				
			ipRule->PreserveAsSubAttributes =
				asVariantBool(m_btnPreserveAsSubAttributes.GetCheck() == BST_CHECKED);

			ipRule->CreateMergedRegion =
				asVariantBool(m_btnCreateMergedRegion.GetCheck() == BST_CHECKED);
		}
		
		SetDirty(FALSE);

		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI22786");

	// If we reached here, it's because of an exception
	return S_FALSE;
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CMergeAttributesPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI22787", pbValue != __nullptr);

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
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI22788");
}

//--------------------------------------------------------------------------------------------------
// Private Methods
//--------------------------------------------------------------------------------------------------
void CMergeAttributesPP::updateControls()
{
	if (m_btnSpecifyName.GetCheck() == BST_CHECKED)
	{
		// Enable the name specification edit box.
		m_editSpecifiedName.EnableWindow(TRUE);

		// Disable controls dependent on the preserve name option.
		m_editNameList.EnableWindow(FALSE);
		m_btnEditNameList.EnableWindow(FALSE);
		if (m_btnValueFromName.GetCheck() == BST_CHECKED)
		{
			m_btnValueFromName.SetCheck(BST_UNCHECKED);
			m_btnSpecifyValue.SetCheck(BST_CHECKED);
		}
		m_btnValueFromName.EnableWindow(FALSE);
	}
	else
	{
		// Disable the name specification edit box.
		m_editSpecifiedName.EnableWindow(FALSE);

		// Enable controls dependent on the preserve name option.
		m_editNameList.EnableWindow(TRUE);
		m_btnEditNameList.EnableWindow(TRUE);
		m_btnValueFromName.EnableWindow(TRUE);
	}

	// Enable/disable the value specification edit box and priority list as appropriate.
	m_editSpecifiedValue.EnableWindow(asMFCBool(m_btnSpecifyValue.GetCheck() == BST_CHECKED));
	m_editSpecifiedType.EnableWindow(asMFCBool(m_btnSpecifyType.GetCheck() == BST_CHECKED));
	m_editValueList.EnableWindow(asMFCBool(m_btnPreserveValue.GetCheck() == BST_CHECKED));
	m_btnEditValueList.EnableWindow(asMFCBool(m_btnPreserveValue.GetCheck() == BST_CHECKED));

	// "SelectType" encompases both the type priority list an selecting the type of the attribute
	// that supplied the chosen name.
	if (m_btnSelectType.GetCheck() == BST_CHECKED)
	{
		// Enable/diable PreserveType controls.
		m_btnPreserveType.EnableWindow(TRUE);
		if (m_btnPreserveType.GetCheck() == BST_CHECKED)
		{
			m_editTypeList.EnableWindow(TRUE);
			m_btnEditTypeList.EnableWindow(TRUE);
		}
		else
		{
			m_editTypeList.EnableWindow(FALSE);
			m_btnEditTypeList.EnableWindow(FALSE);
		}
	}
	else
	{
		// Disable all PreserveType controls.
		m_btnPreserveType.EnableWindow(FALSE);
		m_editTypeList.EnableWindow(FALSE);
		m_btnEditTypeList.EnableWindow(FALSE);
	}

	// Uncheck the TypeFromName checkbox if name is not getting preserved from one of the attributes.
	if (m_btnPreserveName.GetCheck() != BST_CHECKED &&
		m_btnTypeFromName.GetCheck() == BST_CHECKED)
	{
		m_btnTypeFromName.SetCheck(BST_UNCHECKED);
	}

	bool bEnableTypeFromName = 
		(m_btnSelectType.GetCheck() == BST_CHECKED) && (m_btnPreserveName.GetCheck() == BST_CHECKED);
	m_btnTypeFromName.EnableWindow(asBSTChecked(bEnableTypeFromName));
}
//--------------------------------------------------------------------------------------------------
void CMergeAttributesPP::updateDelimetedList(vector<string>& vecList, ATLControls::CEdit& editControl,
	IVariantVectorPtr ipList/* = __nullptr*/)
{
	// If ipList is specified, use it to populate vecList.
	if (ipList != __nullptr)
	{
		vecList.clear();
		int count = ipList->Size;
		for (int i = 0; i < count; i++)
		{
			vecList.push_back(asString(_bstr_t(ipList->GetItem(i))));
		}
	}

	// Use vecList to populate editControl as a semi-colon delimited list.
	string strList;
	for each (string strValue in vecList)
	{
		if (!strList.empty())
		{
			strList += ";";
		}
		strList += strValue;
	}

	editControl.SetWindowText(strList.c_str());
}
//--------------------------------------------------------------------------------------------------
void CMergeAttributesPP::validateLicense()
{
	VALIDATE_LICENSE(gnRULESET_EDITOR_UI_OBJECT, "ELI22789", 
		"Merge attributes output handler PP");
}
//--------------------------------------------------------------------------------------------------
