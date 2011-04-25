// CImageOutputDlg.cpp : implementation file
//

#include "stdafx.h"
#include "ImageOutputDlg.h"
#include "RedactionCCUtils.h"

#include <LicenseMgmt.h>
#include <TemporaryResourceOverride.h>
#include <XBrowseForFolder.h>

//-------------------------------------------------------------------------------------------------
// CImageOutputDlg dialog
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CImageOutputDlg, CDialog)
//-------------------------------------------------------------------------------------------------
CImageOutputDlg::CImageOutputDlg(const ImageOutputOptions &options, CWnd* pParent /*=NULL*/)
	: CDialog(CImageOutputDlg::IDD, pParent),
	  m_options(options)
{

}
//-------------------------------------------------------------------------------------------------
CImageOutputDlg::~CImageOutputDlg()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI24547")
}
//-------------------------------------------------------------------------------------------------
void CImageOutputDlg::getOptions(ImageOutputOptions &rOptions)
{
	rOptions = m_options;
}
//-------------------------------------------------------------------------------------------------
void CImageOutputDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);

	DDX_Control(pDX, IDC_EDIT_IMG_OUT, m_editOutputImageName);
	DDX_Control(pDX, IDC_CHECK_CARRY_ANNOTATIONS, m_chkCarryAnnotation);
	DDX_Control(pDX, IDC_CHECK_REDACTIONS_AS_ANNOTATIONS, m_chkRedactionAsAnnotation);
	DDX_Control(pDX, IDC_RADIO_RETAIN_REDACTIONS, m_radioRedactedImage);
	DDX_Control(pDX, IDC_RADIO_USE_ORIGINAL_IMAGE, m_radioOriginalImage);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CImageOutputDlg, CDialog)
	ON_BN_CLICKED(IDC_BUTTON_IMG_BROWSE, &CImageOutputDlg::OnBnClickedButtonImgBrowse)
	ON_BN_CLICKED(IDC_BUTTON_SELECT_IMAGE_TAG, &CImageOutputDlg::OnBnClickedButtonSelectImageTag)
END_MESSAGE_MAP()
//-------------------------------------------------------------------------------------------------
BOOL CImageOutputDlg::OnInitDialog()
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	CDialog::OnInitDialog();

	try
	{
		// Initialize the document tag
		m_btnSelectImgTag.SubclassDlgItem(IDC_BUTTON_SELECT_IMAGE_TAG, CWnd::FromHandle(m_hWnd));
		m_btnSelectImgTag.SetIcon(::LoadIcon(_Module.m_hInstResource, 
				MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));

		m_editOutputImageName.SetWindowTextA(m_options.strOutputFile.c_str());

		// Set Annotation items
		m_chkCarryAnnotation.SetCheck(asBSTChecked(m_options.bRetainAnnotations));
		m_chkRedactionAsAnnotation.SetCheck(asBSTChecked(m_options.bApplyAsAnnotations));

		// Disable the checkboxes if not licensed
		if (!LicenseManagement::isAnnotationLicensed() )
		{
			m_chkCarryAnnotation.EnableWindow( FALSE );
			m_chkRedactionAsAnnotation.EnableWindow( FALSE );
		}

		// Set the retain redactions option
		m_radioRedactedImage.SetCheck( asBSTChecked(m_options.bRetainRedactions) );
		m_radioOriginalImage.SetCheck( asBSTChecked(!m_options.bRetainRedactions) );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24538")

	return TRUE;
}

//-------------------------------------------------------------------------------------------------
// CImageOutputDlg message handlers
//-------------------------------------------------------------------------------------------------
void CImageOutputDlg::OnOK()
{
	try
	{
		CString zOutputImageName;
		m_editOutputImageName.GetWindowText(zOutputImageName);
		
		// Make sure the Output Image name is not empty
		if ( zOutputImageName.IsEmpty() )
		{
			m_editOutputImageName.SetFocus();
			AfxMessageBox("Output Image Name must not be empty.");
			return;
		}

		// Create a local IFAMTagManagerPtr object
		UCLID_FILEPROCESSINGLib::IFAMTagManagerPtr ipFAMTagManager;
		ipFAMTagManager.CreateInstance(CLSID_FAMTagManager);
		ASSERT_RESOURCE_ALLOCATION("ELI15034", ipFAMTagManager != __nullptr);

		// Make sure the file name contains valid string tags
		_bstr_t bstrFileName = zOutputImageName;
		if (ipFAMTagManager->StringContainsInvalidTags(bstrFileName) == VARIANT_TRUE)
		{
			m_editOutputImageName.SetSel(0, -1);
			m_editOutputImageName.SetFocus();
			MessageBox("The output file name contains invalid tags.");
			return;
		}

		// Set the output file name
		m_options.strOutputFile = zOutputImageName;

		// Retrieve and store Annotation settings
		m_options.bRetainAnnotations = (m_chkCarryAnnotation.GetCheck() == BST_CHECKED);
		m_options.bApplyAsAnnotations = (m_chkRedactionAsAnnotation.GetCheck() == BST_CHECKED);

		// Store the retain redactions setting
		m_options.bRetainRedactions = m_radioRedactedImage.GetCheck() == BST_CHECKED;

		CDialog::OnOK();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24539")
}
//-------------------------------------------------------------------------------------------------
void CImageOutputDlg::OnBnClickedButtonImgBrowse()
{
	try
	{
		CString zOutputFile;
		m_editOutputImageName.GetWindowText( zOutputFile );
		
		// Separate folder and file portions of output file path
		long lTotal = zOutputFile.GetLength();
		std::string strFolder;
		std::string strFile;
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
		if (XBrowseForFolder( m_hWnd, strFolder.c_str(), pszPath, sizeof(pszPath) ))
		{
			// Refresh the display by appending the filename to the new folder
			zOutputFile = pszPath;
			zOutputFile += "\\";
			zOutputFile += strFile.c_str();

			m_editOutputImageName.SetWindowTextA( zOutputFile );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24540")
}
//-------------------------------------------------------------------------------------------------
void CImageOutputDlg::OnBnClickedButtonSelectImageTag()
{
	try
	{
		// Get the position and dimensions of the button
		RECT rect;
		m_btnSelectImgTag.GetWindowRect(&rect);

		// Get the user selection
		string strChoice = CRedactionCustomComponentsUtils::ChooseDocTag(m_hWnd, 
			rect.right, rect.top);

		// Set the corresponding edit box if the user selected a tag
		if(strChoice != "")
		{
			m_editOutputImageName.ReplaceSel(strChoice.c_str(), TRUE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24541")
}
