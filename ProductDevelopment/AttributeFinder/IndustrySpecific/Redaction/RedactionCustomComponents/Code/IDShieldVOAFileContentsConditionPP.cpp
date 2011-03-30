// IDShieldVOAFileContentsConditionPP.cpp : Implementation of CIDShieldVOAFileContentsConditionPP
#include "stdafx.h"
#include "IDShieldVOAFileContentsConditionPP.h"
#include "IDShieldVOAFileContentsCondition.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <COMUtils.h>

//-------------------------------------------------------------------------------------------------
// CIDShieldVOAFileContentsConditionPP
//-------------------------------------------------------------------------------------------------
CIDShieldVOAFileContentsConditionPP::CIDShieldVOAFileContentsConditionPP() :
	m_strDocCategory(""),
	m_ipDocTypes(NULL),
	m_csTargetDataFile(gstrDEFAULT_TARGET_FILENAME)
	{
		m_dwTitleID = IDS_TITLEIDSHIELDVOAFILECONTENTSCONDITIONPP;
		m_dwHelpFileID = IDS_HELPFILEIDSHIELDVOAFILECONTENTSCONDITIONPP;
		m_dwDocStringID = IDS_DOCSTRINGIDSHIELDVOAFILECONTENTSCONDITIONPP;
	}
//-------------------------------------------------------------------------------------------------
HRESULT CIDShieldVOAFileContentsConditionPP::FinalConstruct()
{
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
void CIDShieldVOAFileContentsConditionPP::FinalRelease()
{
}

//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CIDShieldVOAFileContentsConditionPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Setup controls
		m_checkContainsData				= GetDlgItem(IDC_CHECK_CONTAINS_DATA);
		m_comboAttributeQuantifier      = GetDlgItem(IDC_COMBO_CONTAINS);
		m_checkContainsHCData			= GetDlgItem(IDC_CHECK_CONTAINS_HIGH);
		m_checkContainsMCData			= GetDlgItem(IDC_CHECK_CONTAINS_MEDIUM);
		m_checkContainsLCData			= GetDlgItem(IDC_CHECK_CONTAINS_LOW);
		m_checkContainsManualData		= GetDlgItem(IDC_CHECK_CONTAINS_MANUAL);
		m_checkContainsClues			= GetDlgItem(IDC_CHECK_CONTAINS_CLUES);
		m_checkIndicatesDocType			= GetDlgItem(IDC_CHECK_DOCTYPE);
		m_listDocTypes					= GetDlgItem(IDC_LIST_DOCTYPE);
		m_btnSelectDocType				= GetDlgItem(IDC_BTN_SELECT_DOCTYPE);
		m_groupDataFileOptions			= GetDlgItem(IDC_GROUP_DATA_FILE_OPTIONS);
		m_txtDataFileStatus				= GetDlgItem(IDC_STATIC_DATAFILE_STATUS);
		m_btnConfigDataFile				= GetDlgItem(IDC_BTN_CONFIG_DATAFILE);
		m_groupMissingFileOptions		= GetDlgItem(IDC_GROUP_MISSING_FILE_OPTIONS);
		m_btnErrorOnMissingData			= GetDlgItem(IDC_RECORD_ERROR);
		m_btnSatisfiedOnMissingData		= GetDlgItem(IDC_CONDITION_SATISFIED);
		m_btnUnsatisfiedOnMissingData	= GetDlgItem(IDC_CONDITION_UNSATISFIED);

		UCLID_REDACTIONCUSTOMCOMPONENTSLib::IIDShieldVOAFileContentsConditionPtr ipVoaCondition = m_ppUnk[0];
		ASSERT_RESOURCE_ALLOCATION("ELI17430", ipVoaCondition != __nullptr);

		// Should we look for voa attributes?
		bool bCheckDataContents = asCppBool(ipVoaCondition->CheckDataContents);

		// Populate the attribute quantifier combo box
		long lSelected = ipVoaCondition->AttributeQuantifier;
		for (long i = 0; i < UCLID_REDACTIONCUSTOMCOMPONENTSLib::kNumQuantifiers; i++)
		{
			// Set the text and data for this item
			int iIndex = m_comboAttributeQuantifier.AddString( getAttributeQuantifierName(i).c_str() );
			m_comboAttributeQuantifier.SetItemData(iIndex, i);

			// Select this item if necessary
			if (i == lSelected)
			{
				m_comboAttributeQuantifier.SetCurSel(iIndex);
			}
		}

		// Check/Enable/diable data check boxes accordingly
		m_checkContainsData.SetCheck(bCheckDataContents ? BST_CHECKED : BST_UNCHECKED);
		m_checkContainsHCData.EnableWindow(bCheckDataContents);
		m_checkContainsMCData.EnableWindow(bCheckDataContents);
		m_checkContainsLCData.EnableWindow(bCheckDataContents);
		m_checkContainsManualData.EnableWindow(bCheckDataContents);
		m_checkContainsClues.EnableWindow(bCheckDataContents);

		// Set data contents checks
		m_checkContainsHCData.SetCheck(asCppBool(ipVoaCondition->LookForHCData) ? BST_CHECKED : BST_UNCHECKED);
		m_checkContainsMCData.SetCheck(asCppBool(ipVoaCondition->LookForMCData) ? BST_CHECKED : BST_UNCHECKED);
		m_checkContainsLCData.SetCheck(asCppBool(ipVoaCondition->LookForLCData) ? BST_CHECKED : BST_UNCHECKED);
		m_checkContainsManualData.SetCheck(asCppBool(ipVoaCondition->LookForManualData) ? BST_CHECKED : BST_UNCHECKED);
		m_checkContainsClues.SetCheck(asCppBool(ipVoaCondition->LookForClues) ? BST_CHECKED : BST_UNCHECKED);

		// Should we look for voa doc type?
		bool bCheckDocType = asCppBool(ipVoaCondition->CheckDocType);

		// Check/Enable/diable controls accordingly
		m_checkIndicatesDocType.SetCheck(bCheckDocType ? BST_CHECKED : BST_UNCHECKED);
		m_listDocTypes.EnableWindow(bCheckDocType);
		m_btnSelectDocType.EnableWindow(bCheckDocType);

		// Retrieve doc types setting
		m_ipDocTypes = ipVoaCondition->DocTypes;
		ASSERT_RESOURCE_ALLOCATION("ELI17455", m_ipDocTypes != __nullptr);

		// Populate doc type list UI element
		loadDocTypesList();

		// Load current data file setting
		m_csTargetDataFile = asString(ipVoaCondition->TargetFileName);

		// Set target file status message. If using default setting, set control to a user
		// friendly message instead of the path value.
		if (m_csTargetDataFile == gstrDEFAULT_TARGET_FILENAME)
		{
			m_txtDataFileStatus.SetWindowText(gstrDEFAULT_TARGET_MESSAGE.c_str());
		}
		else
		{
			m_txtDataFileStatus.SetWindowText(m_csTargetDataFile.c_str());
		}

		// Initialize the radio buttons for the missing data condition
		m_btnErrorOnMissingData.SetCheck(
			ipVoaCondition->MissingFileBehavior == UCLID_REDACTIONCUSTOMCOMPONENTSLib::kThrowError? BST_CHECKED : BST_UNCHECKED);
		m_btnSatisfiedOnMissingData.SetCheck(
			ipVoaCondition->MissingFileBehavior == UCLID_REDACTIONCUSTOMCOMPONENTSLib::kConsiderSatisfied ? BST_CHECKED : BST_UNCHECKED);
		m_btnUnsatisfiedOnMissingData.SetCheck(
			ipVoaCondition->MissingFileBehavior == UCLID_REDACTIONCUSTOMCOMPONENTSLib::kConsiderUnsatisfied ? BST_CHECKED : BST_UNCHECKED);

		// If only the condition is to be configured, hide the options for handling a missing data
		// file or specifying the data file location.
		if (asCppBool(ipVoaCondition->ConfigureConditionsOnly))
		{
			m_groupDataFileOptions.ShowWindow(SW_HIDE);
			m_txtDataFileStatus.ShowWindow(SW_HIDE);
			m_btnConfigDataFile.ShowWindow(SW_HIDE);
			m_btnErrorOnMissingData.ShowWindow(SW_HIDE);
			m_btnSatisfiedOnMissingData.ShowWindow(SW_HIDE);
			m_btnUnsatisfiedOnMissingData.ShowWindow(SW_HIDE);
			m_groupMissingFileOptions.ShowWindow(SW_HIDE);
		}

		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17399");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CIDShieldVOAFileContentsConditionPP::OnShowWindow(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		if (wParam == FALSE)
		{
			return S_OK;
		}

		UCLID_REDACTIONCUSTOMCOMPONENTSLib::IIDShieldVOAFileContentsConditionPtr ipVoaCondition = m_ppUnk[0];
		ASSERT_RESOURCE_ALLOCATION("ELI28650", ipVoaCondition != __nullptr);

		// Adjust the size of the screen to account for options that have been hidden.
		if (asCppBool(ipVoaCondition->ConfigureConditionsOnly))
		{
			// The amount the screen needs to shrink is the distance between the bottoms of the
			// doc type list box and missing file option group box
			RECT rectListDocTypes;
			m_listDocTypes.GetWindowRect(&rectListDocTypes);

			RECT rectMissingFileOptions;
			m_groupMissingFileOptions.GetWindowRect(&rectMissingFileOptions);

			long nAmountToShrink = rectMissingFileOptions.bottom - rectListDocTypes.bottom;

			// Calculate the new window size and position of the parent window.
			CRect rect;
			HWND hwndParent = this->GetParent();
			::GetWindowRect(hwndParent, &rect);
			rect.bottom -= nAmountToShrink;
			::SetWindowPos(hwndParent, NULL, rect.left, rect.top, rect.Width(), rect.Height(),
				SWP_NOZORDER);

			// Adjust the size/position all siblings of the property page (beneath the parent).
			HWND hwndSibling = ::GetWindow(hwndParent, GW_CHILD);
			while (hwndSibling != __nullptr)
			{
				::GetWindowRect(hwndSibling, &rect);
				CPoint ptTopLeft(rect.left, rect.top);
				::ScreenToClient(hwndParent, &ptTopLeft);
				rect.MoveToXY(ptTopLeft.x, ptTopLeft.y);

				if (::GetWindowTextLength(hwndSibling) > 0)
				{
					// A control with text is a button that should move up, not shrink
					rect.OffsetRect(0, -nAmountToShrink);
				}
				else
				{
					// Otherwise this is the property page itself of the group box that contains it.
					// Shrink it.
					rect.bottom -= nAmountToShrink;
				}

				::SetWindowPos(hwndSibling, NULL, rect.left, rect.top, rect.Width(), rect.Height(),
					SWP_NOZORDER);

				hwndSibling = ::GetWindow(hwndSibling, GW_HWNDNEXT);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI28651");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CIDShieldVOAFileContentsConditionPP::OnClickedContainsData(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Should we look at voa data contents?
		bool bCheckDataContents = (m_checkContainsData.GetCheck() == BST_CHECKED);

		// Enable/diable data check boxes accordingly
		m_checkContainsHCData.EnableWindow(bCheckDataContents);
		m_checkContainsMCData.EnableWindow(bCheckDataContents);
		m_checkContainsLCData.EnableWindow(bCheckDataContents);
		m_checkContainsManualData.EnableWindow(bCheckDataContents);
		m_checkContainsClues.EnableWindow(bCheckDataContents);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17400");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CIDShieldVOAFileContentsConditionPP::OnClickedIndicatesDocType(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Should we look for voa doc type?
		bool bCheckDocType = (m_checkIndicatesDocType.GetCheck() == BST_CHECKED);

		// Enable/diable controls accordingly
		m_listDocTypes.EnableWindow(bCheckDocType);
		m_btnSelectDocType.EnableWindow(bCheckDocType);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17456");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CIDShieldVOAFileContentsConditionPP::OnClickedDocType(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Show wait cursor
		CWaitCursor cw;

		// Create a new Utils
		IDocumentClassificationUtilsPtr ipDocUtils (CLSID_DocumentClassifier);
		ASSERT_RESOURCE_ALLOCATION("ELI17442", ipDocUtils != __nullptr);

		// if the category is not set default it to the first in the industry list
		if (m_strDocCategory.empty())
		{
			// Default it to the first industry category in the category list
			IVariantVectorPtr ipIndustries = ipDocUtils->GetDocumentIndustries();
			ASSERT_RESOURCE_ALLOCATION("ELI17443", ipIndustries != __nullptr);

			// Make sure there is at least one industry
			if (ipIndustries->Size > 0)
			{
				// Get the first industry in the list
				variant_t vt = ipIndustries->GetItem(0);
				
				// Set the industry
				m_strDocCategory = asString(vt.bstrVal);
			}
			else
			{
				UCLIDException ue("ELI17444", "No industry categories defined");
				throw ue;
			}
		}

		// Display the dialog - with variable industry, multiple selection and special types
		_bstr_t	bstrIndustry = get_bstr_t(m_strDocCategory.c_str());
		IVariantVectorPtr ipTypes = ipDocUtils->GetDocTypeSelection(
			&(bstrIndustry.GetBSTR()), VARIANT_TRUE, VARIANT_TRUE, VARIANT_TRUE, VARIANT_FALSE);

		// if cancel or nothing selected ipTypes will be NULL or have a size of zero 
		if (ipTypes != __nullptr && ipTypes->Size > 0)
		{
			m_ipDocTypes = ipTypes;
		}
		else
		{
			// No new selection
			return 0;
		}

		// load selected doc type to list UI element
		loadDocTypesList();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17406");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CIDShieldVOAFileContentsConditionPP::OnClickedConfigDataFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Get SelectTargetFile object
		UCLID_REDACTIONCUSTOMCOMPONENTSLib::ISelectTargetFileUIPtr ipFileSelector(CLSID_SelectTargetFileUI);
		ASSERT_RESOURCE_ALLOCATION("ELI17477", ipFileSelector != __nullptr);

		// Initialize parameters
		ipFileSelector->Title = get_bstr_t("Customize ID Shield Data File Path");
		ipFileSelector->Instructions = get_bstr_t("Evaluate the following ID Shield Data File");
		ipFileSelector->DefaultFileName = get_bstr_t(gstrDEFAULT_TARGET_FILENAME);
		ipFileSelector->DefaultExtension = get_bstr_t(".voa");
		ipFileSelector->FileTypes = get_bstr_t("VOA Files (*.voa)|*.voa||");
		ipFileSelector->FileName = get_bstr_t(m_csTargetDataFile);
		
		// Prompt for new data file setting
		if (asCppBool(ipFileSelector->PromptForFile()))
		{
			m_csTargetDataFile = asString(ipFileSelector->FileName);
		}

		// Update target file status message.
		if (m_csTargetDataFile == gstrDEFAULT_TARGET_FILENAME)
		{
			m_txtDataFileStatus.SetWindowText(gstrDEFAULT_TARGET_MESSAGE.c_str());
		}
		else
		{
			m_txtDataFileStatus.SetWindowText(m_csTargetDataFile.c_str());
		}

	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17466");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsConditionPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		if (pbValue == NULL)
		{
			return E_POINTER;
		}

		// Check license
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

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIDShieldVOAFileContentsConditionPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
	int nResult = S_OK;

	try
	{
		ATLTRACE(_T("CIDShieldVOAFileContentsConditionPP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			UCLID_REDACTIONCUSTOMCOMPONENTSLib::IIDShieldVOAFileContentsConditionPtr ipVoaCondition = m_ppUnk[i];
			ASSERT_RESOURCE_ALLOCATION("ELI17402", ipVoaCondition != __nullptr);

			// Set data contents conditions
			ipVoaCondition->CheckDataContents = asVariantBool(m_checkContainsData.GetCheck() == BST_CHECKED);

			// Set the attribute quantifier
			int iSelected = m_comboAttributeQuantifier.GetCurSel();
			ipVoaCondition->AttributeQuantifier = 
				(UCLID_REDACTIONCUSTOMCOMPONENTSLib::EAttributeQuantifier)
				m_comboAttributeQuantifier.GetItemData(iSelected);

			// Set attributes to look for
			ipVoaCondition->LookForHCData		= asVariantBool(m_checkContainsHCData.GetCheck() == BST_CHECKED);
			ipVoaCondition->LookForMCData		= asVariantBool(m_checkContainsMCData.GetCheck() == BST_CHECKED);
			ipVoaCondition->LookForLCData		= asVariantBool(m_checkContainsLCData.GetCheck() == BST_CHECKED);
			ipVoaCondition->LookForManualData	= asVariantBool(m_checkContainsManualData.GetCheck() == BST_CHECKED);
			ipVoaCondition->LookForClues		= asVariantBool(m_checkContainsClues.GetCheck() == BST_CHECKED);

			// Set doc type conditions
			ipVoaCondition->CheckDocType = asVariantBool(m_checkIndicatesDocType.GetCheck() == BST_CHECKED);
			ipVoaCondition->DocTypes = m_ipDocTypes;
			ipVoaCondition->DocCategory = get_bstr_t(m_strDocCategory);

			// Save target data file
			ipVoaCondition->TargetFileName = get_bstr_t(m_csTargetDataFile);

			// Save missing data file action
			if (m_btnErrorOnMissingData.GetCheck() == BST_CHECKED)
			{
				ipVoaCondition->MissingFileBehavior	= UCLID_REDACTIONCUSTOMCOMPONENTSLib::kThrowError;
			}
			else if (m_btnSatisfiedOnMissingData.GetCheck() == BST_CHECKED)
			{
				ipVoaCondition->MissingFileBehavior	= UCLID_REDACTIONCUSTOMCOMPONENTSLib::kConsiderSatisfied;
			}
			else if (m_btnUnsatisfiedOnMissingData.GetCheck() == BST_CHECKED)
			{
				ipVoaCondition->MissingFileBehavior	= UCLID_REDACTIONCUSTOMCOMPONENTSLib::kConsiderUnsatisfied;
			}

			// If CheckDataContents but no target attributes are selected, prompt
			if (ipVoaCondition->CheckDataContents == VARIANT_TRUE)
			{
				if (ipVoaCondition->LookForHCData == VARIANT_FALSE &&
					ipVoaCondition->LookForMCData == VARIANT_FALSE &&
					ipVoaCondition->LookForLCData == VARIANT_FALSE &&
					ipVoaCondition->LookForManualData == VARIANT_FALSE &&
					ipVoaCondition->LookForClues == VARIANT_FALSE)
				{
					m_checkContainsData.SetFocus();	
					UCLIDException ue("ELI17528", "There must be a data type selected.");
					ue.display();
					return S_FALSE;
				}
			}

			// If CheckDocType but no doc types are selected, prompt
			if (ipVoaCondition->CheckDocType == VARIANT_TRUE && (m_ipDocTypes == __nullptr || m_ipDocTypes->Size < 1))
			{
				m_checkIndicatesDocType.SetFocus();	
				UCLIDException ue("ELI17529", "There must be a document type selected.");
				ue.display();
				return S_FALSE;
			}

			// If CheckDataContents but no target attributes are selected, prompt
			if (ipVoaCondition->CheckDataContents == VARIANT_FALSE && ipVoaCondition->CheckDocType == VARIANT_FALSE)
			{
				m_checkContainsData.SetFocus();	
				UCLIDException ue("ELI17531", "There must be a selected condition");
				ue.display();
				return S_FALSE;
			}
		}

		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI17401");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
string CIDShieldVOAFileContentsConditionPP::getAttributeQuantifierName(long eQuantifier)
{
	switch (eQuantifier)
	{
	case kNone:
		return "none";

	case kAny:
		return "any";

	case kOneOfEach:
		return "at least one of each";

	case kOnlyAny:
		return "only any";
	}

	UCLIDException ue("ELI25133", "Unexpected attribute quantifier");
	ue.addDebugInfo("Quantifier number", eQuantifier);
	throw ue;
}
//-------------------------------------------------------------------------------------------------
void CIDShieldVOAFileContentsConditionPP::loadDocTypesList()
{
	// Clear existing items
	m_listDocTypes.ResetContent();

	// Load currently selected doc types
	long lCount = m_ipDocTypes->Size;
	for (long i = 0; i < lCount; i++)
	{
		string strType = asString(_bstr_t(m_ipDocTypes->GetItem(i)));
		m_listDocTypes.AddString(strType.c_str());
	}
}
//-------------------------------------------------------------------------------------------------
void CIDShieldVOAFileContentsConditionPP::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnIDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI17398", "ID Shield VOA File Contents Condition PP");
}
//-------------------------------------------------------------------------------------------------