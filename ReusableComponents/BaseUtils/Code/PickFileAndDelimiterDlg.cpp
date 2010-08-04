// PickFileAndDelimiterDlg.cpp : implementation file

#include "stdafx.h"
#include "resource.h"
#include "PickFileAndDelimiterDlg.h"
#include "TemporaryResourceOverride.h"
#include "UCLIDException.h"
#include "cpputil.h"

#include <io.h>
#include <SYS\STAT.H>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

extern AFX_EXTENSION_MODULE BaseUtilsDLL;

//-------------------------------------------------------------------------------------------------
// PickFileAndDelimiterDlg dialog
//-------------------------------------------------------------------------------------------------
PickFileAndDelimiterDlg::PickFileAndDelimiterDlg(const CString& zFileName,
												 const CString& zDelimiter,
												 bool bShowDelimiter, /*=true*/
												 bool bOpenFile, /*=true*/
												 CWnd* pParent/*=NULL*/)
	: CDialog(PickFileAndDelimiterDlg::IDD, pParent),
	  m_zDelimiter(zDelimiter), 
	  m_zFileName(zFileName),
	  m_bShowDelimiter(bShowDelimiter),
	  m_bOpenFile(bOpenFile),
	  m_bConfirmed(false)
{
	//{{AFX_DATA_INIT(PickFileAndDelimiterDlg)
	//}}AFX_DATA_INIT
}
//-------------------------------------------------------------------------------------------------
void PickFileAndDelimiterDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(PickFileAndDelimiterDlg)
	DDX_Control(pDX, IDC_STATIC_DELIMITER, m_promptDelimiter);
	DDX_Control(pDX, IDC_EDIT_FILENAME, m_editFileName);
	DDX_Control(pDX, IDC_EDIT_DELIM, m_editDelimiter);
	DDX_Text(pDX, IDC_EDIT_DELIM, m_zDelimiter);
	DDV_MaxChars(pDX, m_zDelimiter, 1);
	DDX_Text(pDX, IDC_EDIT_FILENAME, m_zFileName);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(PickFileAndDelimiterDlg, CDialog)
	//{{AFX_MSG_MAP(PickFileAndDelimiterDlg)
	ON_BN_CLICKED(IDC_BTN_BROWSE, OnBtnBrowse)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// PickFileAndDelimiterDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL PickFileAndDelimiterDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	
	try
	{
		CDialog::OnInitDialog();
		
		UpdateData(TRUE);
		
		// select the entire text in the input box, and set focus to it
		m_editFileName.SetSel(0, -1);
		m_editFileName.SetFocus();

		if (!m_bShowDelimiter)
		{
			// hide delimiter related windows
			m_promptDelimiter.ShowWindow(SW_HIDE);
			m_editDelimiter.ShowWindow(SW_HIDE);
		}

		// set window title according to the use of the dialog
		if (m_bOpenFile)
		{
			SetWindowText("Open");
		}
		else
		{
			SetWindowText("Save");
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18594");
	
	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//-------------------------------------------------------------------------------------------------
void PickFileAndDelimiterDlg::OnBtnBrowse() 
{
	TemporaryResourceOverride resourceOverride(BaseUtilsDLL.hResource);

	try
	{	
		UpdateData(TRUE);

		CFileDialog openDialog(m_bOpenFile, NULL, NULL, OFN_HIDEREADONLY 
			| (m_bOpenFile ? OFN_FILEMUSTEXIST : OFN_OVERWRITEPROMPT)
			| OFN_PATHMUSTEXIST 
			| OFN_NOCHANGEDIR,
			"All files (*.*)|*.*||", NULL);
		
		if (openDialog.DoModal() == IDOK)
		{
			// get the path to the image that the user wants to open
			m_zFileName = openDialog.GetPathName();

			// Set flag to avoid repeat confirmation
			m_bConfirmed = true;
			
			UpdateData(FALSE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04267");
}
//-------------------------------------------------------------------------------------------------
void PickFileAndDelimiterDlg::OnOK() 
{
	TemporaryResourceOverride resourceOverride(BaseUtilsDLL.hResource);

	try
	{
		// make sure the delimiter is not empty
		UpdateData(TRUE);

		if (m_bShowDelimiter)
		{
			if (m_zDelimiter.IsEmpty() || m_zDelimiter.GetLength() > 1)
			{
				AfxMessageBox("Please provide a non-empty character as delimiter");
				m_editDelimiter.SetSel(0, -1);
				m_editDelimiter.SetFocus();
				return;
			}
		}

		if (m_bOpenFile)
		{
			// if this dialog is served as an open dialog.
			// make sure the file exists
			if (!isValidFile((LPCTSTR)m_zFileName))
			{
				AfxMessageBox("The file specified does not exist. Please enter an existing file name.");
				m_editFileName.SetSel(0, -1);
				m_editFileName.SetFocus();
				return;
			}
		}
		else
		{
			// Check file specification
			if (m_zFileName.IsEmpty())
			{
				MessageBox( "Please provide an output file.", "Error", MB_ICONEXCLAMATION|MB_OK );
				return;
			}
			
			// Check if it is a folder name
			if (isValidFolder( m_zFileName.operator LPCTSTR() ))
			{
				CString zMsg("The name specified is an existing folder.\nPlease specify a valid file name.");
				MessageBox( zMsg, "Error", MB_ICONEXCLAMATION|MB_OK );
				return;
			}

			// Check if the folder specified inside the name string is valid
			string strDir = getDirectoryFromFullPath( m_zFileName.operator LPCTSTR() );
			if (strDir != "" && !isValidFolder( strDir ))
			{
				CString zMsg("The folder specified is not accessible.");
				MessageBox( zMsg, "Error", MB_ICONEXCLAMATION|MB_OK );
				return;
			}

			// Confirm overwrite of existing file if not already confirmed via Browse
			if (!m_bConfirmed && isFileOrFolderValid( m_zFileName.operator LPCTSTR() ))
			{
				CString zMsg("File \"");
				zMsg += m_zFileName;
				zMsg += "\" already exists.";

				int iReturn = MessageBox( zMsg, "Confirm overwrite?", MB_YESNOCANCEL );
				if ((iReturn == IDNO) || (iReturn == IDCANCEL))
				{
					return;
				}
			}

			// if this dialog is served as a close dialog
			// make sure the file has read/write permission if exists
			if (fileExistsAndIsReadOnly(m_zFileName.GetString()))
			{
				// change the mode to read/write
				if (_chmod(m_zFileName, _S_IREAD | _S_IWRITE) == -1)
				{
					CString zMsg("Failed to save ");
					zMsg += m_zFileName;
					zMsg += ". Please make sure the file is not shared by another program.";
					// failed to change the mode
					AfxMessageBox(zMsg);
					return;
				}
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04268");
	
	CDialog::OnOK();
}
//-------------------------------------------------------------------------------------------------
int PickFileAndDelimiterDlg::DoModal()
{
	TemporaryResourceOverride rcOverride(BaseUtilsDLL.hResource);
	
	// call the base class member
	return CDialog::DoModal();
}
//-------------------------------------------------------------------------------------------------
