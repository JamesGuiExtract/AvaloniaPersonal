// FilterIDShieldDataFileTaskPP.cpp : Implementation of CFilterIDShieldDataFileTaskPP
#include "stdafx.h"
#include "RedactionCustomComponents.h"
#include "FilterIDShieldDataFileTaskPP.h"
#include "RedactionCCUtils.h"
#include "..\..\..\..\AFCore\Code\Common.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <StringTokenizer.h>
#include <COMUtils.h>
#include <ComponentLicenseIDs.h>
#include <LoadFileDlgThread.h>

#include <string>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// Pre-defined data types for check boxes
const string gstrSSN_TYPE = "ssn";
const string gstrTAX_ID_TYPE = "taxid";
const string gstrCREDIT_DEBIT_TYPE = "ccn";
const string gstrDL_TYPE = "dln";
const string gstrBANK_TYPE = "bank";
const string gstrACCOUNT_TYPE = "account";
const string gstrDOB_TYPE = "dob";

// File filter string for the file browse dialogs
const string gstrVOAFILE_FILTER = "VOA Files (*.voa, *.evoa)|*.voa;*.evoa|All Files (*.*)|*.*||";

//-------------------------------------------------------------------------------------------------
// CFilterIDShieldDataFileTaskPP
//-------------------------------------------------------------------------------------------------
CFilterIDShieldDataFileTaskPP::CFilterIDShieldDataFileTaskPP() 
{
	try
	{
		// Check licensing
		validateLicense();
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI24829")
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFilterIDShieldDataFileTaskPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check parameter
		ASSERT_ARGUMENT("ELI24830", pbValue != NULL);

		try
		{
			// Check license
			validateLicense();

			// If no exception, then pbValue is true
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI24831");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFilterIDShieldDataFileTaskPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	ATLTRACE(_T("CFilterIDShieldDataFileTaskPP::Apply\n"));
	int nResult = S_OK;
	try
	{
		for (UINT i = 0; i < m_nObjects; i++)
		{
			// Get the filter task object
			UCLID_REDACTIONCUSTOMCOMPONENTSLib::IFilterIDShieldDataFileTaskPtr ipFilterTask(m_ppUnk[i]);
			ASSERT_RESOURCE_ALLOCATION("ELI24832", ipFilterTask != NULL);

			// Get the VOA file to read
			_bstr_t bstrVOAFileToRead;
			m_editVOAFileToRead.GetWindowText(bstrVOAFileToRead.GetAddress());
			if (bstrVOAFileToRead.length() == 0)
			{
				MessageBox("Please specify an input data file.", "No Input File Defined",
					MB_ICONERROR | MB_OK);
				m_editVOAFileToRead.SetFocus();
				return S_FALSE;
			}

			// Get the VOA file to write
			_bstr_t bstrVOAFileToWrite;
			m_editVOAFileToWrite.GetWindowText(bstrVOAFileToWrite.GetAddress());
			if (bstrVOAFileToWrite.length() == 0)
			{
				MessageBox("Please specify an output data file.", "No Output File Defined",
					MB_ICONERROR | MB_OK);
				m_editVOAFileToWrite.SetFocus();
				return S_FALSE;
			}

			// Check for matching input and output VOA files
			if (doInputOutputFilesMatch(asString(bstrVOAFileToRead), asString(bstrVOAFileToWrite)))
			{
				MessageBox("The input and output data files cannot match!",
					"Input/Output Must Be Different", MB_ICONERROR | MB_OK);
				m_editVOAFileToWrite.SetFocus();
				return S_FALSE;
			}

			// Get the data filter settings
			IVariantVectorPtr ipDataTypes = getDataTypeCheckBoxes();
			ASSERT_RESOURCE_ALLOCATION("ELI24833", ipDataTypes);

			// Ensure at least one data type is specified
			if (ipDataTypes->Size == 0)
			{
				MessageBox("At least one data type to filter must be specified!",
					"No Data Type Filter Selected", MB_ICONERROR | MB_OK);
				m_checkSSN.SetFocus();
				return S_FALSE;
			}

			// Set the input and output VOA files
			ipFilterTask->VOAFileToRead = bstrVOAFileToRead;
			ipFilterTask->VOAFileToWrite = bstrVOAFileToWrite;

			// Set the data types
			ipFilterTask->DataTypes = ipDataTypes;
		}

		m_bDirty = FALSE;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24834");

	return nResult;
}

//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CFilterIDShieldDataFileTaskPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, 
									 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Prepare control members
		m_editVOAFileToRead = GetDlgItem(IDC_EDIT_FILTER_INPUT_FILE);
		m_btnInputBrowse = GetDlgItem(IDC_BUTTON_FILTER_INPUT_BROWSE);
		m_btnInputTags.SubclassDlgItem(IDC_BUTTON_FILTER_INPUT_TAGS, CWnd::FromHandle(m_hWnd));
		m_btnInputTags.SetIcon(::LoadIcon(_Module.m_hInstResource,
			MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));
		m_checkSSN = GetDlgItem(IDC_CHECK_FILTER_SSN);
		m_checkTaxID = GetDlgItem(IDC_CHECK_FILTER_TAXID);
		m_checkCreditDebit = GetDlgItem(IDC_CHECK_FILTER_CREDIT);
		m_checkDriversLicense = GetDlgItem(IDC_CHECK_FILTER_DL);
		m_checkBankAccountNumbers = GetDlgItem(IDC_CHECK_FILTER_BANK);
		m_checkOtherAccount = GetDlgItem(IDC_CHECK_FILTER_ACCOUNT);
		m_checkDOB = GetDlgItem(IDC_CHECK_FILTER_DOB);
		m_checkOther = GetDlgItem(IDC_CHECK_FILTER_OTHER);
		m_editOtherData = GetDlgItem(IDC_EDIT_FILTER_OTHER);
		m_editVOAFileToWrite = GetDlgItem(IDC_EDIT_FILTER_OUTPUT_FILE);
		m_btnOutputBrowse = GetDlgItem(IDC_BUTTON_FILTER_OUTPUT_BROWSE);
		m_btnOutputTags.SubclassDlgItem(IDC_BUTTON_FILTER_OUTPUT_TAGS, CWnd::FromHandle(m_hWnd));
		m_btnOutputTags.SetIcon(::LoadIcon(_Module.m_hInstResource,
			MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));

		// Get Redaction File Processor object
		UCLID_REDACTIONCUSTOMCOMPONENTSLib::IFilterIDShieldDataFileTaskPtr ipFilterTask(m_ppUnk[0]);
		ASSERT_RESOURCE_ALLOCATION("ELI24835", ipFilterTask != NULL);

		//////////////////////////
		// Initialize data members
		//////////////////////////
		// VOA file to read
		m_editVOAFileToRead.SetWindowText(ipFilterTask->VOAFileToRead);

		// Default the check boxes to unchecked
		m_checkSSN.SetCheck(BST_UNCHECKED);
		m_checkTaxID.SetCheck(BST_UNCHECKED);
		m_checkCreditDebit.SetCheck(BST_UNCHECKED);
		m_checkDriversLicense.SetCheck(BST_UNCHECKED);
		m_checkBankAccountNumbers.SetCheck(BST_UNCHECKED);
		m_checkOtherAccount.SetCheck(BST_UNCHECKED);
		m_checkDOB.SetCheck(BST_UNCHECKED);
		m_checkOther.SetCheck(BST_UNCHECKED);
		m_editOtherData.EnableWindow(FALSE);

		// Get the data types
		IVariantVectorPtr ipVarDataTypes = ipFilterTask->DataTypes;
		ASSERT_RESOURCE_ALLOCATION("ELI24836", ipVarDataTypes != NULL);
		setDataTypeCheckBoxes(ipVarDataTypes);

		// VOA file to write
		m_editVOAFileToWrite.SetWindowText(ipFilterTask->VOAFileToWrite);

		// Clear dirty flag
		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24837");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CFilterIDShieldDataFileTaskPP::OnClickedButtonInputBrowse(WORD wNotifyCode, WORD wID,
																  HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// bring open file dialog
		CFileDialog fileDlg(TRUE, 0, NULL, OFN_ENABLESIZING | OFN_EXPLORER | 
			OFN_PATHMUSTEXIST, gstrVOAFILE_FILTER.c_str() , CWnd::FromHandle(m_hWnd));
		
		// Pass the pointer of dialog to create ThreadDataStruct object
		ThreadFileDlg tfd(&fileDlg);

		// if the user clicked on OK, then update the filename in the editbox
		if (tfd.doModal() == IDOK)
		{
			// get the file name
			m_editVOAFileToRead.SetWindowText(fileDlg.GetPathName());
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24838");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CFilterIDShieldDataFileTaskPP::OnClickedButtonInputTags(WORD wNotifyCode, WORD wID,
																HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Display the tags and set the selected to the edit box
		RECT rect;
		m_btnInputTags.GetWindowRect(&rect);
		string strChoice = CRedactionCustomComponentsUtils::ChooseDocTag(m_hWnd,
			rect.right, rect.top);
		if (strChoice != "")
		{
			m_editVOAFileToRead.ReplaceSel(strChoice.c_str(), TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24839");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CFilterIDShieldDataFileTaskPP::OnClickedButtonOutputBrowse(WORD wNotifyCode, WORD wID,
																   HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// bring open file dialog
		CFileDialog fileDlg(TRUE, 0, NULL, OFN_ENABLESIZING | OFN_EXPLORER | 
			OFN_PATHMUSTEXIST, gstrVOAFILE_FILTER.c_str() , CWnd::FromHandle(m_hWnd));
		
		// Pass the pointer of dialog to create ThreadDataStruct object
		ThreadFileDlg tfd(&fileDlg);

		// if the user clicked on OK, then update the filename in the editbox
		if (tfd.doModal() == IDOK)
		{
			// get the file name
			m_editVOAFileToWrite.SetWindowText(fileDlg.GetPathName());
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24840");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CFilterIDShieldDataFileTaskPP::OnClickedButtonOutputTags(WORD wNotifyCode, WORD wID,
																HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Display the tags and set the selected to the edit box
		RECT rect;
		m_btnOutputTags.GetWindowRect(&rect);
		string strChoice = CRedactionCustomComponentsUtils::ChooseDocTag(m_hWnd,
			rect.right, rect.top);
		if (strChoice != "")
		{
			m_editVOAFileToWrite.ReplaceSel(strChoice.c_str(), TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24841");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CFilterIDShieldDataFileTaskPP::OnClickedCheckOther(WORD wNotifyCode, WORD wID,
														   HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		if (m_checkOther.GetCheck() == BST_CHECKED)
		{
			// Enable the edit box
			m_editOtherData.EnableWindow(TRUE);
		}
		else
		{
			// Clearing check, disable edit box and clear it
			m_editOtherData.EnableWindow(FALSE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24842");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CFilterIDShieldDataFileTaskPP::validateLicense()
{
	VALIDATE_LICENSE(gnIDSHIELD_CORE_OBJECTS, "ELI24843", "Filter ID Shield Data File Task PP");
}
//-------------------------------------------------------------------------------------------------
void CFilterIDShieldDataFileTaskPP::setDataTypeCheckBoxes(IVariantVectorPtr ipVarDataTypes)
{
	try
	{
		ASSERT_ARGUMENT("ELI24845", ipVarDataTypes != NULL);

		// Set the appropriate check boxes (and the other text field)
		string strOther = "";
		long lSize = ipVarDataTypes->Size;
		for (long i=0; i < lSize; i++)
		{
			// Get the string from the collection (and ensure lower case)
			string strTemp = asString(ipVarDataTypes->Item[i].bstrVal);
			makeLowerCase(strTemp);

			if (strTemp == gstrSSN_TYPE)
			{
				m_checkSSN.SetCheck(BST_CHECKED);
			}
			else if (strTemp == gstrTAX_ID_TYPE)
			{
				m_checkTaxID.SetCheck(BST_CHECKED);
			}
			else if (strTemp == gstrCREDIT_DEBIT_TYPE)
			{
				m_checkCreditDebit.SetCheck(BST_CHECKED);
			}
			else if (strTemp == gstrDL_TYPE)
			{
				m_checkDriversLicense.SetCheck(BST_CHECKED);
			}
			else if (strTemp == gstrBANK_TYPE)
			{
				m_checkBankAccountNumbers.SetCheck(BST_CHECKED);
			}
			else if (strTemp == gstrACCOUNT_TYPE)
			{
				m_checkOtherAccount.SetCheck(BST_CHECKED);
			}
			else if (strTemp == gstrDOB_TYPE)
			{
				m_checkDOB.SetCheck(BST_CHECKED);
			}
			else
			{
				if (strOther.empty())
				{
					strOther = strTemp;
				}
				else
				{
					strOther += ",";
					strOther += strTemp;
				}
			}
		}
		if (!strOther.empty())
		{
			m_checkOther.SetCheck(BST_CHECKED);
			m_editOtherData.SetWindowText(strOther.c_str());
			m_editOtherData.EnableWindow(TRUE);
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24846");
}
//-------------------------------------------------------------------------------------------------
IVariantVectorPtr CFilterIDShieldDataFileTaskPP::getDataTypeCheckBoxes()
{
	try
	{
		// Create a variant vector to hold the return values
		IVariantVectorPtr ipVarDataTypes(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI24847", ipVarDataTypes != NULL);

		// Add data type for each check box that is checked
		if (m_checkSSN.GetCheck() == BST_CHECKED)
		{
			ipVarDataTypes->PushBack(_variant_t(gstrSSN_TYPE.c_str()));
		}
		if (m_checkTaxID.GetCheck() == BST_CHECKED)
		{
			ipVarDataTypes->PushBack(_variant_t(gstrTAX_ID_TYPE.c_str()));
		}
		if (m_checkCreditDebit.GetCheck() == BST_CHECKED)
		{
			ipVarDataTypes->PushBack(_variant_t(gstrCREDIT_DEBIT_TYPE.c_str()));
		}
		if (m_checkDriversLicense.GetCheck() == BST_CHECKED)
		{
			ipVarDataTypes->PushBack(_variant_t(gstrDL_TYPE.c_str()));
		}
		if (m_checkBankAccountNumbers.GetCheck() == BST_CHECKED)
		{
			ipVarDataTypes->PushBack(_variant_t(gstrBANK_TYPE.c_str()));
		}
		if (m_checkOtherAccount.GetCheck() == BST_CHECKED)
		{
			ipVarDataTypes->PushBack(_variant_t(gstrACCOUNT_TYPE.c_str()));
		}
		if (m_checkDOB.GetCheck() == BST_CHECKED)
		{
			ipVarDataTypes->PushBack(_variant_t(gstrDOB_TYPE.c_str()));
		}

		// If the other check box is checked then parse the string in the edit box
		if (m_checkOther.GetCheck() == BST_CHECKED)
		{
			_bstr_t bstrTemp;
			m_editOtherData.GetWindowText(bstrTemp.GetAddress());
			string strOtherData = asString(bstrTemp);

			// Allow for either ';' or ',' to separate data types
			replaceVariable(strOtherData, ";", ",");

			// Now tokenize the data types
			vector<string> vecTokens;
			StringTokenizer::sGetTokens(strOtherData, ',', vecTokens);

			// Add each token to the variant vector
			for (vector<string>::iterator it = vecTokens.begin(); it != vecTokens.end(); it++)
			{
				makeLowerCase(*it);
				ipVarDataTypes->PushBack(_variant_t(it->c_str()));
			}
		}

		return ipVarDataTypes;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24848");
}
//-------------------------------------------------------------------------------------------------
bool CFilterIDShieldDataFileTaskPP::doInputOutputFilesMatch(const string &strInput,
															const string &strOutput)
{
	try
	{
		// Get a FAM tag manager and set the FPS file directory
		IFAMTagManagerPtr ipFamTagManager(CLSID_FAMTagManager);
		ASSERT_RESOURCE_ALLOCATION("ELI24850", ipFamTagManager != NULL);
		ipFamTagManager->FPSFileDir = "C:\\Validation";
		
		// Create a fake source doc name
		string strFakeSource = "C:\\Images\\123.tif";

		// Get the expanded names for both the input and output files
		string strInputFile = CRedactionCustomComponentsUtils::ExpandTagsAndTFE(ipFamTagManager,
			strInput, strFakeSource);
		string strOutputFile = CRedactionCustomComponentsUtils::ExpandTagsAndTFE(ipFamTagManager,
			strOutput, strFakeSource);

		// Make lower case for case insensitive compare
		makeLowerCase(strInputFile);
		makeLowerCase(strOutputFile);

		return strInputFile == strOutputFile;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI24851");
}
//-------------------------------------------------------------------------------------------------