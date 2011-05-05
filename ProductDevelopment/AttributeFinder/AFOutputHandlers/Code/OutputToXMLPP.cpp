// OutputToXMLPP.cpp : Implementation of COutputToXMLPP
#include "stdafx.h"
#include "AFOutputHandlers.h"
#include "OutputToXMLPP.h"
#include "..\..\AFCore\Code\EditorLicenseID.h"

#include <LicenseMgmt.h>
#include <UCLIDException.h>
#include <AFTagManager.h>
#include <ComponentLicenseIDs.h>
#include <ComUtils.h>
#include <TextFunctionExpander.h>
#include <DocTagUtils.h>

//-------------------------------------------------------------------------------------------------
// COutputToXMLPP
//-------------------------------------------------------------------------------------------------
COutputToXMLPP::COutputToXMLPP() 
:	m_iXMLFormat(kXMLSchema)
{
	m_dwTitleID = IDS_TITLEOutputToXMLPP;
	m_dwHelpFileID = IDS_HELPFILEOutputToXMLPP;
	m_dwDocStringID = IDS_DOCSTRINGOutputToXMLPP;
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToXMLPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ATLTRACE(_T("COutputToXMLPP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			// get the output handler object
			UCLID_AFOUTPUTHANDLERSLib::IOutputToXMLPtr ipOutputToXML = m_ppUnk[i];

			// if the edit box does not have text, then it's an error conditition
			CComBSTR bstrFileName;
			GetDlgItemText(IDC_EDIT_FILENAME, bstrFileName.m_str);
			_bstr_t _bstrFileName(bstrFileName);
			if (_bstrFileName.length() == 0)
			{
				throw UCLIDException("ELI07897", "Please specify an output filename!");
			}

			// store the filename
			ipOutputToXML->FileName = _bstrFileName;

			// Store the XML format option
			ipOutputToXML->Format = (UCLID_AFOUTPUTHANDLERSLib::EXMLOutputFormat)m_iXMLFormat;

			// Store the XML named-attributes option
			// Store the XML schema name settings (flag and name)
			if ((UCLID_AFOUTPUTHANDLERSLib::EXMLOutputFormat)m_iXMLFormat == kXMLOriginal)
			{
				_bstr_t _bstrNoSchema( "" );

				// For Original format:
				// - Never use named attributes
				// - Never use schema name (flag or text)
				ipOutputToXML->NamedAttributes = VARIANT_FALSE;
				ipOutputToXML->UseSchemaName = VARIANT_FALSE;
				ipOutputToXML->SchemaName = _bstrNoSchema;
			}
			else
			{
				bool bUseSchema = m_btnSchema.GetCheck() == BST_CHECKED;
				ipOutputToXML->NamedAttributes = asVariantBool(m_btnNames.GetCheck() == BST_CHECKED);
				ipOutputToXML->UseSchemaName = asVariantBool(bUseSchema);

				// Cannot use an empty schema name
				_bstr_t bstrSchemaName;
				m_editSchemaName.GetWindowText(bstrSchemaName.GetAddress());
				if (bUseSchema && (bstrSchemaName.length() == 0))
				{
					throw UCLIDException("ELI12915", "Please specify a schema name!");
				}
				else
				{
					// Store the schema name (can be empty if m_bSchemaName == false)
					ipOutputToXML->SchemaName = bstrSchemaName;
				}
			}

			// Set the remove spatial info value
			ipOutputToXML->RemoveSpatialInfo =
				asVariantBool(m_chkRemoveSpatialInfo.GetCheck() == BST_CHECKED);
		}

		SetDirty(FALSE);
		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07896")

	// if we reached here, it's because of an exception
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COutputToXMLPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
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
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT COutputToXMLPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, 
									 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_AFOUTPUTHANDLERSLib::IOutputToXMLPtr ipOutputToXML = m_ppUnk[0];
		if (ipOutputToXML)
		{
			// initialize controls
			m_editFileName = GetDlgItem(IDC_EDIT_FILENAME);
			m_btnBrowse = GetDlgItem(IDC_BTN_BROWSE_FILE);
			m_btnSelectDocTag.SubclassDlgItem(IDC_BTN_SELECT_DOC_TAG, CWnd::FromHandle(m_hWnd));
			m_btnSelectDocTag.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));
			m_btnNames = GetDlgItem( IDC_CHECK_NAMES );
			m_btnSchema = GetDlgItem( IDC_CHECK_SCHEMA );
			m_editSchemaName = GetDlgItem( IDC_EDIT_SCHEMANAME );
			m_chkRemoveSpatialInfo = GetDlgItem(IDC_CHK_XML_OUT_REMOVE_SPATIAL);

			string strFileName = asString(ipOutputToXML->FileName);
			m_editFileName.SetWindowText(strFileName.c_str());

			// Default checkboxes to unchecked and schema name to empty
			CheckDlgButton( IDC_CHECK_NAMES, BST_UNCHECKED );
			CheckDlgButton( IDC_CHECK_SCHEMA, BST_UNCHECKED );
			m_editSchemaName.SetWindowText( "" );
			m_editSchemaName.EnableWindow( FALSE );

			// Determine which set of doc tags to display
			m_bFAMTags = asCppBool(ipOutputToXML->FAMTags);

			// Set radio button
			m_iXMLFormat = (int)ipOutputToXML->Format;
			if (m_iXMLFormat == (int)kXMLOriginal)
			{
				CheckRadioButton( IDC_RADIO_ORIGINAL, IDC_RADIO_SCHEMA, IDC_RADIO_ORIGINAL );

				// Disable checkboxes and edit box [FlexIDSCore #3559]
				m_btnNames.EnableWindow( FALSE );
				m_btnSchema.EnableWindow( FALSE );
				m_editSchemaName.EnableWindow( FALSE );
			}
			else if (m_iXMLFormat == (int)kXMLSchema)
			{
				CheckRadioButton( IDC_RADIO_ORIGINAL, IDC_RADIO_SCHEMA, IDC_RADIO_SCHEMA );

				// Set Names checkbox
				m_btnNames.SetCheck(asBSTChecked(ipOutputToXML->NamedAttributes));

				// Set Schema checkbox
				bool bUseSchema = asCppBool(ipOutputToXML->UseSchemaName);
				if (bUseSchema)
				{
					m_btnSchema.SetCheck(asBSTChecked(bUseSchema));

					// Set Schema name edit box
					m_editSchemaName.EnableWindow( TRUE );
					string strSchemaName = asString(ipOutputToXML->SchemaName);
					m_editSchemaName.SetWindowText( strSchemaName.c_str() );
				}
				// else retain default settings of unchecked and empty
			}

			// Set the checked state on the remove spatial info check box
			m_chkRemoveSpatialInfo.SetCheck(asBSTChecked(ipOutputToXML->RemoveSpatialInfo));

			// set focus to the editbox
			m_editFileName.SetSel(0, -1);
			m_editFileName.SetFocus();
		}
	
		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07893");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT COutputToXMLPP::OnClickedBtnBrowseFile(WORD wNotifyCode, 
											   WORD wID, HWND hWndCtl, 
											   BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		const static string s_strAllFiles = "XML Files (*.xml)|*.xml"
											"|All Files (*.*)|*.*||";

		// bring open file dialog
		string strFileExtension(s_strAllFiles);
		CFileDialog fileDlg(FALSE, ".xml", NULL, OFN_ENABLESIZING | OFN_EXPLORER | 
			OFN_HIDEREADONLY | OFN_PATHMUSTEXIST | OFN_OVERWRITEPROMPT,
			strFileExtension.c_str(), NULL);
		
		// if the user clicked on OK, then update the filename in the editbox
		if (fileDlg.DoModal() == IDOK)
		{
			// get the file name
			m_editFileName.SetWindowText(fileDlg.GetPathName());
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07894");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT COutputToXMLPP::OnClickedSelectDocTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		if (m_bFAMTags)
		{
			ChooseDocTagForEditBox(IFAMTagManagerPtr(CLSID_FAMTagManager), m_btnSelectDocTag, m_editFileName);
		}
		else
		{
			ChooseDocTagForEditBox(IAFUtilityPtr(CLSID_AFUtility), m_btnSelectDocTag, m_editFileName);
		}
		
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12005");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT COutputToXMLPP::OnBnClickedRadioOriginal(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Save setting
		m_iXMLFormat = kXMLOriginal;

		// Disable checkboxes and edit box
		m_btnNames.EnableWindow( FALSE );
		m_btnSchema.EnableWindow( FALSE );
		m_editSchemaName.EnableWindow( FALSE );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12893");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT COutputToXMLPP::OnBnClickedRadioSchema(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Save setting
		m_iXMLFormat = kXMLSchema;

		// Enable checkboxes
		m_btnNames.EnableWindow( TRUE );
		m_btnSchema.EnableWindow( TRUE );

		// Enable or disable the edit box
		m_editSchemaName.EnableWindow(asMFCBool(m_btnSchema.GetCheck() == BST_CHECKED));
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12894");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT COutputToXMLPP::OnBnClickedCheckSchema(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Enable or disable the edit box
		m_editSchemaName.EnableWindow(asMFCBool(m_btnSchema.GetCheck() == BST_CHECKED));
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12909");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void COutputToXMLPP::validateLicense()
{
	// Property page used to require editor license, but now just requires general FAM license
	// since this property page may be displayed in both the rule set editor and the
	// FAM ConvertVOAToXML task.
	VALIDATE_LICENSE( gnFILE_ACTION_MANAGER_OBJECTS, "ELI07913", 
		"OutputToXML PP" );
}
//-------------------------------------------------------------------------------------------------