// ExemptionCodesDlg.cpp : implementation file
//

#include "stdafx.h"
#include "ExemptionCodesDlg.h"

#include <TemporaryResourceOverride.h>
#include <UClIDException.h>

//-------------------------------------------------------------------------------------------------
// CExemptionCodesDlg dialog
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CExemptionCodesDlg, CDialog)
//-------------------------------------------------------------------------------------------------
CExemptionCodesDlg::CExemptionCodesDlg(const MasterExemptionCodeList& exemptionCodes, CWnd* pParent /*=NULL*/)
	: CDialog(CExemptionCodesDlg::IDD, pParent), 
	  m_allCodes(exemptionCodes),
	  m_bEnableApplyLast(false)
{

}
//-------------------------------------------------------------------------------------------------
CExemptionCodesDlg::~CExemptionCodesDlg()
{
}
//-------------------------------------------------------------------------------------------------
ExemptionCodeList CExemptionCodesDlg::getExemptionCodes() const
{
	return m_selectedCodes;
}
//-------------------------------------------------------------------------------------------------
void CExemptionCodesDlg::setExemptionCodes(const ExemptionCodeList& codes)
{
	m_selectedCodes = codes;
}
//-------------------------------------------------------------------------------------------------
void CExemptionCodesDlg::enableApplyLastExemption(bool bEnable)
{
	m_bEnableApplyLast = bEnable;
}
//-------------------------------------------------------------------------------------------------
void CExemptionCodesDlg::setLastAppliedExemption(const ExemptionCodeList& codes)
{
	m_lastApplied = codes;
}
//-------------------------------------------------------------------------------------------------
void CExemptionCodesDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_COMBO_EXEMPTION_CATEGORY, m_comboCategory);
	DDX_Control(pDX, IDC_LIST_EXEMPTION_CODES, m_listCodes);
	DDX_Control(pDX, IDC_EDIT_EXEMPTION_DESCRIPTION, m_editDescription);
	DDX_Control(pDX, IDC_CHECK_EXEMPTION_OTHER, m_checkOtherText);
	DDX_Control(pDX, IDC_EDIT_EXEMPTION_OTHER, m_editOtherText);
	DDX_Control(pDX, IDC_EDIT_EXEMPTION_SAMPLE, m_editSample);
	DDX_Control(pDX, IDC_BUTTON_EXEMPTION_APPLY_LAST, m_buttonApplyLast);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CExemptionCodesDlg, CDialog)
	ON_CBN_SELCHANGE(IDC_COMBO_EXEMPTION_CATEGORY, &CExemptionCodesDlg::OnCbnSelchangeComboExemptionCategory)
	ON_NOTIFY(LVN_ITEMCHANGED, IDC_LIST_EXEMPTION_CODES, &CExemptionCodesDlg::OnLvnItemchangedListExemptionCodes)
	ON_BN_CLICKED(IDC_CHECK_EXEMPTION_OTHER, &CExemptionCodesDlg::OnBnClickedCheckExemptionOther)
	ON_EN_UPDATE(IDC_EDIT_EXEMPTION_OTHER, &CExemptionCodesDlg::OnEnUpdateEditExemptionOther)
	ON_BN_CLICKED(IDC_BUTTON_EXEMPTION_CLEAR_ALL, &CExemptionCodesDlg::OnBnClickedButtonExemptionClearAll)
	ON_BN_CLICKED(IDC_BUTTON_EXEMPTION_APPLY_LAST, &CExemptionCodesDlg::OnBnClickedButtonExemptionApplyLast)
END_MESSAGE_MAP()
//-------------------------------------------------------------------------------------------------
ExemptionCodeList CExemptionCodesDlg::getDisplayedExemptionCodes() const
{
	// Create a list to hold the exemption codes
	ExemptionCodeList codes;

	// Store the abbreviated form of the currently selected exemption category
	string strCategory = m_allCodes.getCategoryAbbreviation( getSelectedCategory() );
	codes.setCategory(strCategory);

	// Get the checked codes in this category
	int iCount = m_listCodes.GetItemCount();
	for (int i = 0; i < iCount; i++)
	{
		if (m_listCodes.GetCheck(i) == BST_CHECKED)
		{
			// Add this code to the list
			codes.addCode( getItemCode(i) );
		}
	}

	// Check if the other text checkbox is checked
	if (m_checkOtherText.GetCheck() == BST_CHECKED)
	{
		// Add the text of the other text box
		CString zText;
		m_editOtherText.GetWindowText(zText);
		string strText = zText;
		codes.setOtherText(strText);
	}

	// Return the result
	return codes;
}
//-------------------------------------------------------------------------------------------------
string CExemptionCodesDlg::getSelectedCategory() const
{
	// Get the name of the newly selected category
	int iCurSel = m_comboCategory.GetCurSel();
	if (iCurSel < 0)
	{
		return "";
	}
	CString zCategory;
	m_comboCategory.GetLBText(iCurSel, zCategory);
	return (string)zCategory;
}
//-------------------------------------------------------------------------------------------------
string CExemptionCodesDlg::getItemCode(int i) const
{
	return (string) m_listCodes.GetItemText(i, 1);
}
//-------------------------------------------------------------------------------------------------
void CExemptionCodesDlg::selectExemptionCodes(const ExemptionCodeList& codes)
{
	// Check if the selected codes are from a category in the master list
	bool bHasCategory = codes.hasCategory();
	int iSel = 0;
	if (bHasCategory)
	{
		// Check for the category in the combo box
		string strAbbr = codes.getCategory();
		string strCategory = m_allCodes.getFullCategoryName(strAbbr);
		iSel = m_comboCategory.FindStringExact(-1, strCategory.c_str());

		if (iSel == CB_ERR)
		{
			// The category was not found in the master list
			bHasCategory = false;

			// Default to the first category
			iSel = 0;
		}
	}

	// Select the appropriate category in the combo box
	int iCount = m_comboCategory.GetCount();
	if (iCount > 0)
	{
		m_comboCategory.SetCurSel(iSel);
	}
	OnCbnSelchangeComboExemptionCategory();

	// Check if the category wasn't found
	if (bHasCategory)
	{
		// Set the checkboxes for the selected codes
		int iCount = m_listCodes.GetItemCount();
		for (int i = 0; i < iCount; i++)
		{
			// Check this checkbox if the item is in the code list
			bool bChecked = codes.hasCode( getItemCode(i) );
			if (bChecked)
			{
				m_listCodes.SetCheck(i);
			}
		}
	}

	// Get other text associated with the exemption code
	string strOtherText = codes.getOtherText();

	// If other text is specified, check the checkbox
	bool bOtherText = !strOtherText.empty();
	m_checkOtherText.SetCheck(asBSTChecked(bOtherText));

	// Set the other text edit box
	m_editOtherText.SetWindowText(strOtherText.c_str());
	m_editOtherText.EnableWindow(asMFCBool(bOtherText));
}
//-------------------------------------------------------------------------------------------------
void CExemptionCodesDlg::updateDescription()
{
	// If no item is selected clear the description
	POSITION pos = m_listCodes.GetFirstSelectedItemPosition();
	if (pos == NULL)
	{
		updateDescription("");
		return;
	}
	
	// Get the selected text
	int iSelected = m_listCodes.GetNextSelectedItem(pos);
	string strCode = getItemCode(iSelected);

	// Get the exemption code description
	string strDescription = m_allCodes.getDescription(getSelectedCategory(), strCode);

	// Update the description
	updateDescription(strDescription);
}
//-------------------------------------------------------------------------------------------------
void CExemptionCodesDlg::updateDescription(const string& strDescription)
{
	m_editDescription.SetWindowText(strDescription.c_str());
}
//-------------------------------------------------------------------------------------------------
void CExemptionCodesDlg::updateSample()
{
	// Get the displayed exemption codes as a string
	ExemptionCodeList& codes = getDisplayedExemptionCodes();
	string strText = codes.getAsString();

	// Set the sample text
	m_editSample.SetWindowText(strText.c_str());
}

//-------------------------------------------------------------------------------------------------
// CExemptionCodesDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CExemptionCodesDlg::OnInitDialog()
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	CDialog::OnInitDialog();

	try
	{
		// Set the extended list view style
		m_listCodes.SetExtendedStyle(LVS_EX_FULLROWSELECT | LVS_EX_CHECKBOXES);
		m_listCodes.InsertColumn(0, "", LVCFMT_LEFT, 20, 0);
		m_listCodes.InsertColumn(1, "Code", LVCFMT_LEFT, 70, 1);
		m_listCodes.InsertColumn(2, "Summary", LVCFMT_LEFT, 285, 2);

		// Get the names of the exemption categories
		vector<string> vecCategories;
		m_allCodes.getCategoryNames(vecCategories);

		// Add the names of the categories to combo box
		for (unsigned int i = 0; i < vecCategories.size(); i++)
		{
			m_comboCategory.AddString(vecCategories[i].c_str());
		}

		// Enable/disable the 'apply last' button
		m_buttonApplyLast.EnableWindow(m_bEnableApplyLast);

		// Use last used exemption code category for empty exemption codes [FlexIDSCore #5232]
		ExemptionCodeList initialCodes = m_selectedCodes;
		if (initialCodes.isEmpty() && !m_strLastExemptionCategory.empty())
		{
			initialCodes.setCategory(m_strLastExemptionCategory);
		}

		// Set the control states to match the initial exemption codes
		selectExemptionCodes(initialCodes);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24930")

	return TRUE;
}
//-------------------------------------------------------------------------------------------------
void CExemptionCodesDlg::OnOK()
{
	try
	{
		m_selectedCodes = getDisplayedExemptionCodes();

		// Store the last used exemption code category
		if (!m_selectedCodes.isEmpty())
		{
			m_strLastExemptionCategory = m_selectedCodes.getCategory();
		}

		CDialog::OnOK();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24956")
}
//-------------------------------------------------------------------------------------------------
void CExemptionCodesDlg::OnCbnSelchangeComboExemptionCategory()
{
	try
	{
		// Load the names of the exemption codes in the selected category
		vector<ExemptionCodeInfo> vecCodes;
		m_allCodes.getCodesInCategory(vecCodes, getSelectedCategory());

		// Populate the list view
		m_listCodes.DeleteAllItems();
		m_listCodes.SetItemCount(vecCodes.size());
		for (unsigned int i = 0; i < vecCodes.size(); i++)
		{
			m_listCodes.InsertItem(i, "");
			m_listCodes.SetItemText(i, 1, vecCodes[i].m_strCode.c_str());
			m_listCodes.SetItemText(i, 2, vecCodes[i].m_strSummary.c_str());
		}

		// Select the first item in the list
		if (vecCodes.size() > 0)
		{
			m_listCodes.SetItemState(0, LVIS_SELECTED | LVIS_FOCUSED , LVIS_SELECTED | LVIS_FOCUSED);
		}
	}
		CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24966")
}
//-------------------------------------------------------------------------------------------------
void CExemptionCodesDlg::OnLvnItemchangedListExemptionCodes(NMHDR *pNMHDR, LRESULT *pResult)
{
	try
	{
		LPNMLISTVIEW pNMLV = reinterpret_cast<LPNMLISTVIEW>(pNMHDR);

		// Check if the state changed
		if ((pNMLV->uChanged & LVIF_STATE) > 0)
		{
			// Determine which properties changed
			unsigned int uChangedState = pNMLV->uOldState ^ pNMLV->uNewState;

			// Check if the selection changed
			if ((uChangedState & LVIS_SELECTED) > 0)
			{
				updateDescription();
			}

			// Check if the check state changed
			if ((uChangedState & LVIS_STATEIMAGEMASK) > 0)
			{
				updateSample();
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24967")

	*pResult = 0;
}
//-------------------------------------------------------------------------------------------------
void CExemptionCodesDlg::OnBnClickedCheckExemptionOther()
{
	try
	{
		// Enable/disable the "other text" edit box based on the state of corresponding check box
		BOOL bChecked = asMFCBool(m_checkOtherText.GetCheck() == BST_CHECKED);
		m_editOtherText.EnableWindow(bChecked);

		// Update the sample exemption code edit box
		updateSample();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24968")
}
//-------------------------------------------------------------------------------------------------
void CExemptionCodesDlg::OnEnUpdateEditExemptionOther()
{
	try
	{
		// Update the sample exemption code edit box
		updateSample();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24969")
}
//-------------------------------------------------------------------------------------------------
void CExemptionCodesDlg::OnBnClickedButtonExemptionClearAll()
{
	try
	{
		// Uncheck all the codes in this category
		int iCount = m_listCodes.GetItemCount();
		for (int i = 0; i < iCount; i++)
		{
			m_listCodes.SetCheck(i, BST_UNCHECKED);
		}

		// Uncheck the other text box
		m_checkOtherText.SetCheck(BST_UNCHECKED);
		m_editOtherText.EnableWindow(FALSE);

		// Update the sample exemption code edit box
		updateSample();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24970")
}
//-------------------------------------------------------------------------------------------------
void CExemptionCodesDlg::OnBnClickedButtonExemptionApplyLast()
{
	try
	{
		// Select the last applied exemption codes
		selectExemptionCodes(m_lastApplied);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24992")
}
