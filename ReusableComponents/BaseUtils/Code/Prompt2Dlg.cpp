// Prompt2Dlg.cpp : implementation file
//

#include "stdafx.h"
#include "Prompt2Dlg.h"
#include "TemporaryResourceOverride.h"
#include "UCLIDException.h"

extern AFX_EXTENSION_MODULE BaseUtilsDLL;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// Prompt2Dlg dialog


Prompt2Dlg::Prompt2Dlg(const CString& zTitle, 
					   const CString& zPrompt1, 
					   const CString& zInput1,
					   const CString& zPrompt2, 
					   const CString& zInput2,
					   bool bDefaultFocusInput1 /* = true */,
					   const CString& zHeader,
					   CWnd* pParent /*=NULL*/)
: CDialog(Prompt2Dlg::IDD, pParent), m_zTitle(zTitle), 
  m_bDefaultFocusInput1(bDefaultFocusInput1)
{
	//{{AFX_DATA_INIT(Prompt2Dlg)
	m_zInput1 = _T("");
	m_zInput2 = _T("");
	m_zPrompt1 = _T("");
	m_zPrompt2 = _T("");
	//}}AFX_DATA_INIT

	m_zInput1 = zInput1;
	m_zInput2 = zInput2;
	m_zPrompt1 = zPrompt1;
	m_zPrompt2 = zPrompt2;
	m_zHeader = zHeader;
}
//-------------------------------------------------------------------------------------------------
void Prompt2Dlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(Prompt2Dlg)
	DDX_Control(pDX, IDC_EDIT_INPUT2, m_editInput2);
	DDX_Control(pDX, IDC_EDIT_INPUT1, m_editInput1);
	DDX_Text(pDX, IDC_EDIT_INPUT1, m_zInput1);
	DDX_Text(pDX, IDC_EDIT_INPUT2, m_zInput2);
	DDX_Text(pDX, IDC_STATIC_PROMPT1, m_zPrompt1);
	DDX_Text(pDX, IDC_STATIC_PROMPT2, m_zPrompt2);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(Prompt2Dlg, CDialog)
	//{{AFX_MSG_MAP(Prompt2Dlg)
		// NOTE: the ClassWizard will add message map macros here
	//}}AFX_MSG_MAP
	ON_EN_CHANGE(IDC_EDIT_INPUT1, OnChangeEditInput1)
	ON_BN_CLICKED(IDOK, OnBnClickOK)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// Prompt2Dlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL Prompt2Dlg::OnInitDialog() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		CDialog::OnInitDialog();
			
		// update the data for the prompt, input data, and title
		SetWindowText(m_zTitle);
		UpdateData(TRUE);
		
		// select the entire text in the input box, and set focus to it
		if (m_bDefaultFocusInput1)
		{
			m_editInput1.SetSel(0, -1);
			m_editInput1.SetFocus();
		}
		else
		{
			m_editInput2.SetSel(0, -1);
			m_editInput2.SetFocus();
		}

		// Update the second edit box status
		updateControl();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18596");
	
	return FALSE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//-------------------------------------------------------------------------------------------------
void Prompt2Dlg::OnChangeEditInput1() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		// Call updateControler() to update the status of the second edit box
		updateControl();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14649")
}
//-------------------------------------------------------------------------------------------------
void Prompt2Dlg::OnBnClickOK()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		UpdateData(TRUE);

		// Get the length of the header
		int iLengthHeader = m_zHeader.GetLength();

		// If the length of the header is zero, which means the dialog
		// created doesn't support file:// function, so just return
		if (iLengthHeader == 0)
		{
			OnOK();
			return;
		}

		// Check for file header
		if(m_zInput1.GetLength() > iLengthHeader &&
			m_zInput1.Left(iLengthHeader).CompareNoCase(m_zHeader) == 0)
		{
			// Find the index of the separator
			int nIndexOfSeparator = m_zInput1.Find(';');
			if (nIndexOfSeparator < 0)
			{
				// If the separator doesn't exist
				CString zPrompt = "You should use a \";\" to separate the file name and the delimiter!";
				MessageBox(zPrompt, "Missing Separator", MB_ICONEXCLAMATION);
				m_editInput1.SetFocus();
				return;
			}

			// Get the file name
			CString zFileName = m_zInput1.Mid(iLengthHeader, nIndexOfSeparator - iLengthHeader);
			zFileName.Trim();
			if (zFileName.GetLength() < 1)
			{
				// If the file name contains no characters
				CString zPrompt = "File name can not be empty!";
				MessageBox(zPrompt, "Invalid File Name", MB_ICONEXCLAMATION);
				m_editInput1.SetFocus();
				return;
			}

			// Get the delimiter
			CString zDelimiter = m_zInput1.Right(m_zInput1.GetLength() - nIndexOfSeparator - 1);
			zDelimiter.Trim();
			if (zDelimiter.GetLength() != 1)
			{
				// If the delimiter doesn't exist or contains more than one character
				CString zPrompt = "You should specify a ONE character delimiter at the end of the file name \nseparated by \";\" and the delimiter cannot be whitespace.";
				MessageBox(zPrompt, "Invalid Delimiter", MB_ICONEXCLAMATION);
				m_editInput1.SetFocus();
				m_editInput1.SetSel(nIndexOfSeparator + 1, -1);
				return;
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14648")

	OnOK();
}
//-------------------------------------------------------------------------------------------------
int Prompt2Dlg::DoModal()
{
	TemporaryResourceOverride rcOverride(BaseUtilsDLL.hResource);

	// call the base class member
	return CDialog::DoModal();
}

//-------------------------------------------------------------------------------------------------
// Private method
//-------------------------------------------------------------------------------------------------
void Prompt2Dlg::updateControl()
{
	UpdateData(TRUE);

	// Get the length of the header;
	int iLengthHeader = m_zHeader.GetLength();

	// If the length of the header is zero, which means the dialog
	// created doesn't support file:// function, so just return
	if (iLengthHeader == 0)
	{
		return;
	}

	if(m_zInput1.GetLength() > iLengthHeader &&
		m_zInput1.Left(iLengthHeader).CompareNoCase(m_zHeader) == 0)
	{
		// Disable the second edit box if the first edit box contains a file name
		m_editInput2.EnableWindow(FALSE);
	}
	else
	{
		// Enable the second edit box if the first edit box does not contain a file name
		m_editInput2.EnableWindow(TRUE);
	}
}
//-------------------------------------------------------------------------------------------------
