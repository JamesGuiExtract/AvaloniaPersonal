// OpenSaveFileDlg.cpp : implementation file
//

#include "stdafx.h"
#include "OpenSaveFileDlg.h"

#include <TemporaryResourceOverride.h>
#include <UCLIDException.h>
#include <cpputil.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

extern AFX_EXTENSION_MODULE OCRFilteringBaseModule;

/////////////////////////////////////////////////////////////////////////////
// OpenSaveFileDlg dialog


OpenSaveFileDlg::OpenSaveFileDlg(const CString& zOpenDirectory, 
								 const CString& zFileExtension,		// usually "FSD" or "FOD"
								 bool bOpenDialog,
								 CWnd* pParent)
:CDialog(OpenSaveFileDlg::IDD, pParent),
 m_bOpenDialog(bOpenDialog),
 m_zOpenDirectory(zOpenDirectory),
 m_zFileExtension(zFileExtension)
{
	//{{AFX_DATA_INIT(OpenSaveFileDlg)
	//}}AFX_DATA_INIT
}


void OpenSaveFileDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(OpenSaveFileDlg)
	DDX_Control(pDX, IDC_LIST_Files, m_listFiles);
	DDX_Control(pDX, IDC_EDIT_FileName, m_editFileName);
	DDX_Text(pDX, IDC_STATIC_Directory, m_zOpenDirectory);
	DDX_Text(pDX, IDC_EDIT_FileName, m_zFileName);
	//}}AFX_DATA_MAP
}


BEGIN_MESSAGE_MAP(OpenSaveFileDlg, CDialog)
	//{{AFX_MSG_MAP(OpenSaveFileDlg)
	ON_LBN_DBLCLK(IDC_LIST_Files, OnDblclkLISTFiles)
	ON_LBN_SELCHANGE(IDC_LIST_Files, OnSelchangeLISTFiles)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// OpenSaveFileDlg message handlers

void OpenSaveFileDlg::OnSelchangeLISTFiles() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(OCRFilteringBaseModule.hResource);
	
	try
	{
		// get current selected file name
		int nCurrentIndex = m_listFiles.GetCurSel();
		m_listFiles.GetText(nCurrentIndex, m_zFileName);
		
		UpdateData(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03710")
}
//--------------------------------------------------------------------------------------------------
void OpenSaveFileDlg::OnDblclkLISTFiles() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(OCRFilteringBaseModule.hResource);
	
	try
	{
		// get current selected file name
		int nCurrentIndex = m_listFiles.GetCurSel();
		m_listFiles.GetText(nCurrentIndex, m_zFileName);
		
		UpdateData(FALSE);
		
		// close the dialog and return IDOK
		OnOK();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03711")
}
//--------------------------------------------------------------------------------------------------
void OpenSaveFileDlg::OnOK() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(OCRFilteringBaseModule.hResource);
	
	try
	{
		UpdateData(TRUE);
		
		
		// Make sure the file name has an extension
		m_zFileName = ::getFileNameWithoutExtension((LPCSTR)m_zFileName).c_str();

		// make sure the file name is not empty or invalid
		if (m_zFileName.IsEmpty() || m_zFileName.FindOneOf("\\/:*?\"<>|") != -1)
		{
			AfxMessageBox("Please provide a valid file name.");
			
			GetDlgItem(IDC_EDIT_FileName)->SetFocus();
			m_editFileName.SetSel(0, -1);
			
			return;
		}
		m_zFileName += "." + m_zFileExtension;
		
		// update the display
		UpdateData(FALSE);
		
		// For save as, if the file name already exists, prompt for overwriting
		if (!m_bOpenDialog)
		{
			int nIndex = m_listFiles.FindStringExact(-1, m_zFileName);
			if (nIndex != LB_ERR)
			{
				int nRes = MessageBox("Same file already exists in the directory. Do you wish to overwrite the file?", "Overwrite", MB_YESNO | MB_ICONQUESTION);
				if (nRes == IDNO)
				{
					GetDlgItem(IDC_EDIT_FileName)->SetFocus();	
					return;
				}
			}
		}
		
		CDialog::OnOK();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03712")
}
//--------------------------------------------------------------------------------------------------
BOOL OpenSaveFileDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(OCRFilteringBaseModule.hResource);
	
	try
	{
		CDialog::OnInitDialog();
		
		// populate the dialog
		if (!m_bOpenDialog)
		{
			// set window title and the Button to "Save As"
			SetWindowText("Save As");
			GetDlgItem(IDOK)->SetWindowText("Save");
		}
		
		// enable/disable the edit box for file name
		GetDlgItem(IDC_EDIT_FileName)->EnableWindow(m_bOpenDialog?FALSE:TRUE);
		
		// search for all files with specified type
		CString zFiles = m_zOpenDirectory + "*." + m_zFileExtension;
		CFileFind finder;
		int nIndex = 0;
		BOOL bSuccess = finder.FindFile(zFiles);
		while (bSuccess)
		{
			// look for the next file
			bSuccess = finder.FindNextFile();
			CString zFileName(finder.GetFileName());
			
			// insert the file name into the list
			m_listFiles.InsertString(nIndex, zFileName);
			
			nIndex++;
		}
		
		UpdateData(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03713")

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//--------------------------------------------------------------------------------------------------
