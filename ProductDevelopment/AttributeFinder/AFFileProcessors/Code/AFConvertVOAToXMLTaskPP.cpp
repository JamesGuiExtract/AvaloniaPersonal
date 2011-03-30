// AFConvertVOAToXMLTaskPP.cpp : Implementation of CAFConvertVOAToXMLTaskPP
#include "stdafx.h"
#include "AFFileProcessors.h"
#include "AFConvertVOAToXMLTaskPP.h"
#include "AFFileProcessorsUtils.h"
#include "Common.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <LoadFileDlgThread.h>
#include <ComUtils.h>

#include <string>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const string gstrVOA_FILE_FILTER =	"VOA Files (*.voa;*.evoa;*.eav)|*.voa;*.evoa;*.eav|"
									"All Files (*.*)|*.*||";

//-------------------------------------------------------------------------------------------------
// CAFConvertVOAToXMLTaskPP
//-------------------------------------------------------------------------------------------------
CAFConvertVOAToXMLTaskPP::CAFConvertVOAToXMLTaskPP() :
m_ipOutputToXML(NULL)
{
	try
	{
		// Check licensing
		validateLicense();
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI26244")
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFConvertVOAToXMLTaskPP::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// ensure the return value pointer is non-NULL
		ASSERT_ARGUMENT("ELI26245", pbValue != __nullptr);

		try
		{
			// check license
			validateLicense();

			// if no exception was thrown, then pbValue is true
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26246");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IPropertyPage
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CAFConvertVOAToXMLTaskPP::Apply(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ATLTRACE(_T("CAFConvertVOAToXMLTaskPP::Apply\n"));

		for (UINT i = 0; i < m_nObjects; i++)
		{
			// get the output handler object
			UCLID_AFFILEPROCESSORSLib::IAFConvertVOAToXMLTaskPtr ipAFConvertTask = m_ppUnk[i];
			ASSERT_RESOURCE_ALLOCATION("ELI26444", ipAFConvertTask != __nullptr);

			// Get the VOA file name and ensure it is at least 5 characters (smallest file name)
			_bstr_t bstrVOAFileName;
			m_editVOAFileName.GetWindowText(bstrVOAFileName.GetAddress());
			if (bstrVOAFileName.length() < 5)
			{
				// Prompt, set focus to VOA file name and return S_FALSE
				MessageBox("VOA File name must be specified.", "Must Specify VOA File",
					MB_OK | MB_ICONERROR);
				m_editVOAFileName.SetFocus();
				return S_FALSE;
			}

			// Check if the XML output handler has been configured
			IMustBeConfiguredObjectPtr ipOutputConfigured = m_ipOutputToXML;
			if (ipOutputConfigured == __nullptr || ipOutputConfigured->IsConfigured() == VARIANT_FALSE)
			{
				MessageBox("XML Output handler has not been configured.",
					"Must Configure XML Output", MB_OK | MB_ICONERROR);
				return S_FALSE;
			}

			// Set the convert task properties
			ipAFConvertTask->VOAFile = bstrVOAFileName;
			ipAFConvertTask->XMLOutputHandler = m_ipOutputToXML;
		}

		SetDirty(FALSE);
		return S_OK;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI26247")

	// if we reached here, it's because of an exception
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// Windows message handlers
//-------------------------------------------------------------------------------------------------
LRESULT CAFConvertVOAToXMLTaskPP::OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, 
									 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_editVOAFileName = GetDlgItem(IDC_EDIT_INPUT_VOA_FILE);
		m_btnBrowseVOAFile = GetDlgItem(IDC_BTN_BROWSE_VOA_FILE);
		m_btnVOAFileSelectTag.SubclassDlgItem(IDC_BTN_VOA_FILE_DOC_TAGS, CWnd::FromHandle(m_hWnd));
		m_btnVOAFileSelectTag.SetIcon(::LoadIcon(_Module.m_hInstResource,
			MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));

		UCLID_AFFILEPROCESSORSLib::IAFConvertVOAToXMLTaskPtr ipAFConvertTask = m_ppUnk[0];
		if (ipAFConvertTask)
		{
			string strVOAFile = asString(ipAFConvertTask->VOAFile);
			m_editVOAFileName.SetWindowText(strVOAFile.c_str());

			// Get the XML output handler as a copyable object
			ICopyableObjectPtr ipCopy = ipAFConvertTask->XMLOutputHandler;
			ASSERT_RESOURCE_ALLOCATION("ELI26292", ipCopy != __nullptr);

			// Clone the XML output handler (this way if the user cancels any changes they
			// will not be stored in the convert task object)
			m_ipOutputToXML = (IOutputToXMLPtr) ipCopy->Clone();
			ASSERT_RESOURCE_ALLOCATION("ELI26293", m_ipOutputToXML != __nullptr);
		}
	
		SetDirty(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI26248");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CAFConvertVOAToXMLTaskPP::OnClickedBtnBrowse(WORD wNotifyCode, 
													 WORD wID, HWND hWndCtl, 
													 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{	
		// Create open file dialog to choose a VOA file
		CFileDialog fileDlg(TRUE, ".voa", NULL,
			OFN_ENABLESIZING | OFN_EXPLORER | OFN_PATHMUSTEXIST,
			gstrVOA_FILE_FILTER.c_str(), CWnd::FromHandle(m_hWnd));
		
		// Pass the pointer of dialog to create ThreadFileDlg object
		ThreadFileDlg tfd(&fileDlg);

		// If the user clicked on OK, then update the filename in the edit control
		if (tfd.doModal() == IDOK)
		{
			CString zFileName = fileDlg.GetPathName();

			m_editVOAFileName.SetWindowText(zFileName);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI26249");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CAFConvertVOAToXMLTaskPP::OnClickedBtnDocTags(WORD wNotifyCode, 
													 WORD wID, HWND hWndCtl, 
													 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{	
		// Get the rectangle for the appropriate button
		RECT rect;
		m_btnVOAFileSelectTag.GetWindowRect(&rect);

		// Get the doc tag choice
		string strChoice = CAFFileProcessorsUtils::ChooseDocTag(hWndCtl, rect.right, rect.top);
		if (strChoice != "")
		{
			// Replace the selection
			m_editVOAFileName.ReplaceSel(strChoice.c_str(), TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI26250");

	return 0;
}
//-------------------------------------------------------------------------------------------------
LRESULT CAFConvertVOAToXMLTaskPP::OnClickedBtnConfigureXMLOutput(WORD wNotifyCode, 
													 WORD wID, HWND hWndCtl, 
													 BOOL& bHandled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{	
		// Check if the XML output object has been created yet, if not, create one.
		if (m_ipOutputToXML == __nullptr)
		{
			m_ipOutputToXML.CreateInstance(CLSID_OutputToXML);
			ASSERT_RESOURCE_ALLOCATION("ELI26288", m_ipOutputToXML != __nullptr);

			// Set the doc tags drop down to restrict to FAM tags
			m_ipOutputToXML->FAMTags = VARIANT_TRUE;
		}

		// Get a Misc utils pointer for configuring the xml output
		IMiscUtilsPtr ipMiscUtils(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI26289", ipMiscUtils != __nullptr);

		// Create an object with description for configuration
		IObjectWithDescriptionPtr ipOutput(CLSID_ObjectWithDescription);
		ASSERT_RESOURCE_ALLOCATION("ELI26290", ipOutput != __nullptr);

		// Set the XML output handler as the object
		ipOutput->Object = m_ipOutputToXML;

		// Configure the xml output object
		ipMiscUtils->AllowUserToConfigureObjectProperties(ipOutput);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI26251");

	return 0;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CAFConvertVOAToXMLTaskPP::validateLicense()
{
	VALIDATE_LICENSE(gnFILE_ACTION_MANAGER_OBJECTS, "ELI26243", "Convert VOA To XML PP");
}
//-------------------------------------------------------------------------------------------------
