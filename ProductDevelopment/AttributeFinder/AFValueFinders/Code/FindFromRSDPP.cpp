// FindFromRSDPP.cpp : Implementation of CFindFromRSDPP
#include "stdafx.h"
#include "AFValueFinders.h"
#include "FindFromRSDPP.h"


#include <UCLIDException.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <EditorLicenseID.h>
#include "..\..\AFCore\Code\Common.h"
#include <AFTagManager.h>
#include <DocTagUtils.h>
#include <regex>

//-------------------------------------------------------------------------------------------------
// CFindFromRSDPP
//-------------------------------------------------------------------------------------------------
CFindFromRSDPP::CFindFromRSDPP() 
{
	m_dwTitleID = IDS_TITLEFindFromRSDPP;
	m_dwHelpFileID = IDS_HELPFILEFindFromRSDPP;
	m_dwDocStringID = IDS_DOCSTRINGFindFromRSDPP;
}
//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFindFromRSDPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())
		
	try
	{
		ATLTRACE(_T("CFindFromRSDPP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			// get the finder
			UCLID_AFVALUEFINDERSLib::IFindFromRSDPtr ipFinder = m_ppUnk[i];

			CComBSTR bstrName;
			GetDlgItemText(IDC_EDIT_ATTRIBUTE_NAME, bstrName.m_str);
			string strAttributes = asString(bstrName);
			IVariantVectorPtr ipAttributeNames(CLSID_VariantVector);
			ASSERT_RESOURCE_ALLOCATION("ELI39949", ipAttributeNames != __nullptr);

			// Test if this is a valid attribute-name list
			// Allow <All> or empty string
			string pattern =
				"\\s*(?:"
					"(<All>)"
					"|[_[:alpha:]]\\w*(?:\\s*\\n\\s*[_[:alpha:]]\\w*)*"
				")\\s*";
			regex rgxValidAttributeList(pattern, std::regex_constants::icase);

			// Capture sub-match to see if special value occurred
			smatch subMatches;
			if (!regex_match(strAttributes, subMatches, rgxValidAttributeList))
			{
				throw UCLIDException("ELI39982", 
					"Could not parse attribute name list!\r\n"
					"Specify one attribute name per line. Use '<All>' to find all defined attributes.");
			}

			// Leave the vector empty if '<All>' was matched
			// (first sub-match is entire match)
			if (subMatches[1] == "")
			{
				// Split string list format into parts and add to vector.
				regex rgxAttribute("\\b[_[:alpha:]]\\w*");
				regex_iterator<string::iterator> it(strAttributes.begin(), strAttributes.end(), rgxAttribute);
				regex_iterator<string::iterator> end;
				for (; it != end; ++it)
				{
					ipAttributeNames->PushBack(_bstr_t(it->str().c_str()));
				}
			}

			ipFinder->AttributeNames = ipAttributeNames;
			
			CComBSTR bstrRSDFileName;
			GetDlgItemText(IDC_EDIT_RSD_FILE, bstrRSDFileName.m_str);

			IAFUtilityPtr ipAFUtility(CLSID_AFUtility);
			ASSERT_RESOURCE_ALLOCATION("ELI33848", ipAFUtility != __nullptr);

			ipAFUtility->ValidateAsExplicitPath("ELI33648", bstrRSDFileName.m_str);

			ipFinder->RSDFileName = _bstr_t(bstrRSDFileName);
		}
			
		// if we reached here, then the data was successfully transfered
		// from the UI to the object.
		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10221")

	// if we reached here, it's because of an exception
	// An error message has already been displayed to the user.
	// Return S_FALSE to indicate to the outer scope that the
	// Apply was not successful.
	return S_FALSE;
}
//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CFindFromRSDPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_AFVALUEFINDERSLib::IFindFromRSDPtr ipFinder = m_ppUnk[0];

		if (ipFinder)
		{
			// create tooltip object
			m_infoTip.Create(CWnd::FromHandle(m_hWnd));
			// set no delay.
			m_infoTip.SetShowDelay(0);

			m_editAttributeName = GetDlgItem(IDC_EDIT_ATTRIBUTE_NAME);
			m_editRSDFileName = GetDlgItem(IDC_EDIT_RSD_FILE);
			m_btnSelectDocTag = GetDlgItem(IDC_BTN_SELECT_DOC_TAG);
			m_btnSelectDocTag.SetIcon(::LoadIcon(_Module.m_hInstResource, MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));

			IVariantVectorPtr ipAttributeNames = ipFinder->AttributeNames;
			if (ipAttributeNames->Size > 0)
			{
				string strAttributeNames = asString(_bstr_t(ipAttributeNames->GetItem(0)));
				for(int i=1; i < ipAttributeNames->Size; i++)
				{
					strAttributeNames += "\r\n" + asString(_bstr_t(ipAttributeNames->GetItem(i)));
				}
				m_editAttributeName.SetWindowText(strAttributeNames.c_str());
			}
			else
			{
				m_editAttributeName.SetWindowText("<All>");
			}

			string strRSDFileName = ipFinder->RSDFileName;
			m_editRSDFileName.SetWindowText( strRSDFileName.c_str());
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10222");
	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CFindFromRSDPP::OnClickedBtnBrowse(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// bring open file dialog
		CFileDialog fileDlg(TRUE, NULL, "", 
			OFN_HIDEREADONLY | OFN_FILEMUSTEXIST | OFN_PATHMUSTEXIST | OFN_NOCHANGEDIR,
			gstrRSD_FILE_OPEN_FILTER.c_str(), NULL);
		
		if (fileDlg.DoModal() == IDOK)
		{
			// get the file name
			m_editRSDFileName.SetWindowText(fileDlg.GetPathName());
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10247");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CFindFromRSDPP::OnClickedSelectDocTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ChooseDocTagForEditBox(ITagUtilityPtr(CLSID_AFUtility), m_btnSelectDocTag, m_editRSDFileName);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12007");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CFindFromRSDPP::OnClickedAttributeNameInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// show tooltip info
		CString zText("Specify one attribute name per line.\r\n"
			"Use '<All>' to find all defined attributes.\r\n"
			"NOTE: Attribute names are case-sensitive.");
   
		m_infoTip.Show(zText);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07536");

	return 0;
}