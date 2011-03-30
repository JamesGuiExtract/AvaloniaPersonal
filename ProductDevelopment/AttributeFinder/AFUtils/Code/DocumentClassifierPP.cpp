// DocumentClassifierPP.cpp : Implementation of CDocumentClassifierPP
#include "stdafx.h"
#include "AFUtils.h"
#include "DocumentClassifierPP.h"
#include "SpecialStringDefinitions.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

#include <io.h>

using namespace std;

//--------------------------------------------------------------------------------------------------
// CDocumentClassifierPP
//--------------------------------------------------------------------------------------------------
CDocumentClassifierPP::CDocumentClassifierPP() 
:m_ipAFUtility(NULL)
{
	try
	{
		// Check licensing
		validateLicense();

		m_dwTitleID = IDS_TITLEDocumentClassifierPP;
		m_dwHelpFileID = IDS_HELPFILEDocumentClassifierPP;
		m_dwDocStringID = IDS_DOCSTRINGDocumentClassifierPP;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI07709")
}
//-------------------------------------------------------------------------------------------------
CDocumentClassifierPP::~CDocumentClassifierPP() 
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16331");
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CDocumentClassifierPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("CDocumentClassifierPP::Apply\n"));
		
		int nCurSelIndex = m_cmbCategoryName.GetCurSel();
		// get current selected item text
		int nLen = m_cmbCategoryName.GetLBTextLen(nCurSelIndex);
		if (nLen != CB_ERR)
		{
			LPTSTR lpszText = (LPTSTR)_alloca((nLen + 1) * sizeof(TCHAR));
			m_cmbCategoryName.GetLBText(nCurSelIndex, lpszText);
			
			for (UINT i = 0; i < m_nObjects; i++)
			{
				UCLID_AFUTILSLib::IDocumentClassifierPtr ipDocClassifier = m_ppUnk[i];
				ipDocClassifier->IndustryCategoryName = _bstr_t(lpszText);
			}
			m_bDirty = FALSE;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07118");

	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// ILicensedComponent
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CDocumentClassifierPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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

//--------------------------------------------------------------------------------------------------
// Windows message handlers
//--------------------------------------------------------------------------------------------------
LRESULT CDocumentClassifierPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_cmbCategoryName = GetDlgItem(IDC_CMB_CATEGORY_NAME);

		UCLID_AFUTILSLib::IDocumentClassifierPtr ipDocClassifier = m_ppUnk[0];
		// if current category name is not empty
		string strCurrentCategoryName = ipDocClassifier->IndustryCategoryName;
		// populate combo box drop down list
		populateComboBox(strCurrentCategoryName);

		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07119");

	return 0;
}

//--------------------------------------------------------------------------------------------------
// Private Methods
//--------------------------------------------------------------------------------------------------
void CDocumentClassifierPP::populateComboBox(const string& strCurrentText)
{
	if (m_ipAFUtility == __nullptr)
	{
		m_ipAFUtility.CreateInstance(CLSID_AFUtility);
		ASSERT_RESOURCE_ALLOCATION("ELI07121", m_ipAFUtility != __nullptr);
	}

	// get \ComponentData\DocumentClassifiers folder
	const static string strComponentDataFolder = m_ipAFUtility->GetComponentDataFolder();
	string strDocClassifiersFolder = strComponentDataFolder + "\\" + DOC_CLASSIFIERS_FOLDER;

	// make sure the directory exists
	if (!isValidFolder(strDocClassifiersFolder))
	{
		UCLIDException ue("ELI07120", "Directory doesn't exist.");
		ue.addDebugInfo("Directory", strDocClassifiersFolder);
		ue.addWin32ErrorInfo();
		throw ue;
	}

	// Find all folders' name under DocumentClassifiers folder
	vector<string> vecSubFolderNames = ::getSubFolderShortNames(strDocClassifiersFolder);

	for (unsigned int ui = 0; ui < vecSubFolderNames.size(); ui++)
	{
		m_cmbCategoryName.AddString( vecSubFolderNames[ui].c_str() );
	}

	if (!vecSubFolderNames.empty())
	{
		if (!strCurrentText.empty())
		{
			// set selection to strCurrentText
			m_cmbCategoryName.SelectString(0, strCurrentText.c_str());
		}
		// if strCurrentText is empty, then default to first item
		else 
		{
			// set first item to be selected
			m_cmbCategoryName.SetCurSel(0);
		}
	}
}
//--------------------------------------------------------------------------------------------------
void CDocumentClassifierPP::validateLicense()
{
	// Property Page requires Editor license
	VALIDATE_LICENSE( gnRULESET_EDITOR_UI_OBJECT, "ELI07678", "DocumentClassifier PP" );
}
//-------------------------------------------------------------------------------------------------
