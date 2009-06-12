// PromptDlg.cpp : implementation file

#include "stdafx.h"
#include "BaseUtils.h"
#include "Resource.h"
#include "PromptDlg.h"
#include "TemporaryResourceOverride.h"
#include "UCLIDException.h"

extern AFX_EXTENSION_MODULE BaseUtilsDLL;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// PromptDlg dialog
/////////////////////////////////////////////////////////////////////////////
PromptDlg::PromptDlg(const CString& zTitle, const CString& zPrompt, 
					 const CString& zInput, bool bAllowEmptyString, 
					 bool bRemoveLeadingWhitespace, 
					 bool bRemoveTrailingWhitespace, CWnd* pParent /*=NULL*/)
	:m_zTitle(zTitle), 
	m_bAllowEmptyString(bAllowEmptyString), 
	m_bRemoveLeadingWhitespace(bRemoveLeadingWhitespace), 
	m_bRemoveTrailingWhitespace(bRemoveTrailingWhitespace), 
	CDialog(PromptDlg::IDD, pParent)
{
	//{{AFX_DATA_INIT(PromptDlg)
	m_zPrompt = _T("");
	m_zInput = _T("");
	//}}AFX_DATA_INIT
	
	m_zInput = zInput;
	m_zPrompt = zPrompt;
}

/////////////////////////////////////////////////////////////////////////////
void PromptDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(PromptDlg)
	DDX_Control(pDX, IDOK, m_btnOK);
	DDX_Control(pDX, IDC_EDIT_INPUT, m_editInput);
	DDX_Text(pDX, IDC_PROMPT, m_zPrompt);
	DDX_Text(pDX, IDC_EDIT_INPUT, m_zInput);
	//}}AFX_DATA_MAP
}

/////////////////////////////////////////////////////////////////////////////
BEGIN_MESSAGE_MAP(PromptDlg, CDialog)
	//{{AFX_MSG_MAP(PromptDlg)
	ON_EN_CHANGE(IDC_EDIT_INPUT, OnChangeEditInput)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// PromptDlg message handlers
/////////////////////////////////////////////////////////////////////////////
BOOL PromptDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		CDialog::OnInitDialog();
			
		// update the data for the prompt, input data, and title
		SetWindowText(m_zTitle);
		UpdateData(TRUE);
		
		// If empty string is not allowed and input is empty, disable OK button
		if (!m_bAllowEmptyString && m_zInput.IsEmpty())
		{
			m_btnOK.EnableWindow( FALSE );
		}

		// select the entire text in the input box, and set focus to it
		m_editInput.SetSel(0, -1);
		m_editInput.SetFocus();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18597");
	
	return FALSE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}

/////////////////////////////////////////////////////////////////////////////
int PromptDlg::DoModal()
{
	TemporaryResourceOverride rcOverride(BaseUtilsDLL.hResource);

	// call the base class member
	return CDialog::DoModal();
}

/////////////////////////////////////////////////////////////////////////////
void PromptDlg::OnChangeEditInput() 
{
	UpdateData( TRUE );

	// Is an empty string allowed
	if (!m_bAllowEmptyString)
	{
		// Check for an empty string
		if (m_zInput.IsEmpty())
		{
			// Disable the OK button
			m_btnOK.EnableWindow( FALSE );
		}
		else
		{
			// Perhaps also consider whitespace
			CString	zFinal = m_zInput;
			if (m_bRemoveLeadingWhitespace)
			{
				zFinal.TrimLeft();
			}

			if (m_bRemoveTrailingWhitespace)
			{
				zFinal.TrimRight();
			}

			// Check resulting string
			if (zFinal.IsEmpty())
			{
				// Disable the OK button
				m_btnOK.EnableWindow( FALSE );
			}
			else
			{
				// Enable the OK button
				m_btnOK.EnableWindow( TRUE );
			}
		}
	}
}

/////////////////////////////////////////////////////////////////////////////
void PromptDlg::OnOK() 
{
	UpdateData( TRUE );

	// Perhaps also consider whitespace
	if (m_bRemoveLeadingWhitespace)
	{
		m_zInput.TrimLeft();
	}

	if (m_bRemoveTrailingWhitespace)
	{
		m_zInput.TrimRight();
	}

	// Replace string
	UpdateData( FALSE );
	
	CDialog::OnOK();
}

/////////////////////////////////////////////////////////////////////////////
