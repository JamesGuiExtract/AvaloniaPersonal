#include "stdafx.h"
#include "FileSetConditionDlg.h"

#include <COMUtils.h>
#include <UCLIDException.h>

//-------------------------------------------------------------------------------------------------
// FileSetConditionDlg
//-------------------------------------------------------------------------------------------------
FileSetConditionDlg::FileSetConditionDlg(const UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr& ipFAMDB)
: CDialog(FileSetConditionDlg::IDD),
m_ipFAMDB(ipFAMDB)
{
}
//-------------------------------------------------------------------------------------------------
FileSetConditionDlg::FileSetConditionDlg(const UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr& ipFAMDB,
										 const FileSetCondition& settings)
: CDialog(FileSetConditionDlg::IDD),
m_ipFAMDB(ipFAMDB),
m_settings(settings)
{
}
//-------------------------------------------------------------------------------------------------
FileSetConditionDlg::~FileSetConditionDlg()
{
	try
	{
		m_ipFAMDB = __nullptr;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI37325");
}
//-------------------------------------------------------------------------------------------------
void FileSetConditionDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_COMBO_FILE_SET, m_cmbFileSet);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(FileSetConditionDlg, CDialog)
	ON_BN_CLICKED(IDC_SELECT_BTN_OK, &FileSetConditionDlg::OnClickedOK)
	ON_BN_CLICKED(IDC_SELECT_BTN_CANCEL, &FileSetConditionDlg::OnClickedCancel)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// FileSetConditionDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL FileSetConditionDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		CDialog::OnInitDialog();

		IVariantVectorPtr ipFileSets = m_ipFAMDB->GetFileSets();
		ASSERT_RESOURCE_ALLOCATION("ELI37326", ipFileSets != __nullptr);

		long nCount = ipFileSets->Size;
		for (long i = 0; i < nCount; i++)
		{
			string strFileSetName = asString(ipFileSets->Item[i].bstrVal);
			m_cmbFileSet.AddString(strFileSetName.c_str());
		}

		m_cmbFileSet.SelectString(0, m_settings.getFileSetName().c_str());

		UpdateData(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI37327")

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
void FileSetConditionDlg::OnOK()
{
	// Allow the user to use the enter key to close the dialog
	OnClickedOK();
}
//-------------------------------------------------------------------------------------------------
void FileSetConditionDlg::OnClose()
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	try
	{
		EndDialog(IDCLOSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI37328");
}
//-------------------------------------------------------------------------------------------------
void FileSetConditionDlg::OnClickedCancel()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		CDialog::OnCancel();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI37329");
}
//-------------------------------------------------------------------------------------------------
void FileSetConditionDlg::OnClickedOK()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		// Save the settings
		if (saveSettings())
		{
			// If settings saved successfully, close the dialog
			CDialog::OnOK();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI37330");
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
bool FileSetConditionDlg::saveSettings()
{
	try
	{
		int nSelIndex = m_cmbFileSet.GetCurSel();
		if (nSelIndex >= 0)
		{
			CString	zFileSetName;
			m_cmbFileSet.GetLBText(nSelIndex, zFileSetName);
			m_settings.setFileSetName((LPCTSTR)zFileSetName);

			return true;
		}
		else
		{
			MessageBox("You must select a file set.", "No File Set Selected",
				MB_OK | MB_ICONERROR);

			m_cmbFileSet.SetFocus();

			return false;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI37331")
}
