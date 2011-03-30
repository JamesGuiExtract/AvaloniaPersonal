// CMetadataDlg.cpp : implementation file
//

#include "stdafx.h"
#include "MetadataDlg.h"
#include "RedactionCCUtils.h"

#include <COMUtils.h>
#include <cpputil.h>
#include <TemporaryResourceOverride.h>
#include <UCLIDException.h>
#include <XBrowseForFolder.h>

//-------------------------------------------------------------------------------------------------
// CMetadataDlg dialog
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CMetadataDlg, CDialog)
//-------------------------------------------------------------------------------------------------
CMetadataDlg::CMetadataDlg(const MetadataOptions &options, CWnd* pParent /*=NULL*/)
	: CDialog(CMetadataDlg::IDD, pParent),
	  m_metadata(options)
{
}
//-------------------------------------------------------------------------------------------------
CMetadataDlg::~CMetadataDlg()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI24546")
}
//-------------------------------------------------------------------------------------------------
void CMetadataDlg::getOptions(MetadataOptions &rOptions)
{
	rOptions = m_metadata;
}
//-------------------------------------------------------------------------------------------------
void CMetadataDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);

	DDX_Control(pDX, IDC_EDIT_META_OUT, m_editMetaOutputName);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CMetadataDlg, CDialog)
	ON_BN_CLICKED(IDC_BUTTON_META_BROWSE, &CMetadataDlg::OnBnClickedButtonMetaBrowse)
	ON_BN_CLICKED(IDC_BUTTON_SELECT_META_TAG, &CMetadataDlg::OnBnClickedButtonSelectMetaTag)
END_MESSAGE_MAP()
//-------------------------------------------------------------------------------------------------
BOOL CMetadataDlg::OnInitDialog()
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	CDialog::OnInitDialog();

	try
	{
		// Initialize the document tag button
		m_btnSelectMetaTag.SubclassDlgItem(IDC_BUTTON_SELECT_META_TAG, CWnd::FromHandle(m_hWnd));
		m_btnSelectMetaTag.SetIcon(::LoadIcon(_Module.m_hInstResource, 
			MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));

		// Store metadata output file name
		m_editMetaOutputName.SetWindowText(m_metadata.strOutputFile.c_str());
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24534")

	return TRUE;
}
//-------------------------------------------------------------------------------------------------
void CMetadataDlg::OnOK()
{
	try
	{
		CString zMetaOutputName;
		m_editMetaOutputName.GetWindowText(zMetaOutputName);
		
		// Make sure the Metadata Output File name is not empty
		if ( zMetaOutputName.IsEmpty() )
		{
			m_editMetaOutputName.SetFocus();
			AfxMessageBox("Metadata output file name must not be empty.");
			return;
		}

		// Create a local IFAMTagManagerPtr object
		UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr ipFAMTagManager;
		ipFAMTagManager.CreateInstance(CLSID_FAMTagManager);
		ASSERT_RESOURCE_ALLOCATION("ELI24530", ipFAMTagManager != __nullptr);

		// Make sure the file name contains valid string tags
		_bstr_t bstrMetaOutputName = zMetaOutputName;
		if (ipFAMTagManager->StringContainsInvalidTags(bstrMetaOutputName) == VARIANT_TRUE)
		{
			m_editMetaOutputName.SetSel(0, -1);
			m_editMetaOutputName.SetFocus();
			MessageBox("The metadata output file name contains invalid tags.");
			return;
		}

		// Set the output file name
		m_metadata.strOutputFile = zMetaOutputName;
	
		CDialog::OnOK();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24535")
}
//-------------------------------------------------------------------------------------------------
void CMetadataDlg::OnBnClickedButtonMetaBrowse()
{
	try
	{
		CString zOutputFile;
		m_editMetaOutputName.GetWindowText( zOutputFile );
			
		// Separate folder and file portions of output file path
		long lTotal = zOutputFile.GetLength();
		string strFolder;
		string strFile;
		if (lTotal > 0)
		{
			strFolder = getDirectoryFromFullPath( LPCTSTR(zOutputFile) );
			long lLength = strFolder.length();

			// Provide trailing backslash to folder
			if (zOutputFile.GetAt( lLength ) == '\\')
			{
				lLength++;
			}
			strFile = LPCTSTR(zOutputFile.Right(lTotal - lLength));
		}

		// Display folder selection dialog
		char pszPath[MAX_PATH + 1];
		if (XBrowseForFolder(m_hWnd, strFolder.c_str(), pszPath, sizeof(pszPath)))
		{
			// Refresh the display by appending the filename to the new folder
			zOutputFile = pszPath;
			zOutputFile += "\\";
			zOutputFile += strFile.c_str();

			m_editMetaOutputName.SetWindowText(zOutputFile);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24536")
}
//-------------------------------------------------------------------------------------------------
void CMetadataDlg::OnBnClickedButtonSelectMetaTag()
{
	try
	{
		// Get the position and dimensions of the button
		RECT rect;
		m_btnSelectMetaTag.GetWindowRect(&rect);

		// Get the user selection
		string strChoice = CRedactionCustomComponentsUtils::ChooseDocTag(m_hWnd, 
			rect.right, rect.top);

		// Set the corresponding edit box if the user selected a tag
		if(strChoice != "")
		{
			m_editMetaOutputName.ReplaceSel(strChoice.c_str(), TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24537")
}
