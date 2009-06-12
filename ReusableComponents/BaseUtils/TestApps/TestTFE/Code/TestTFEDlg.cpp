// TestTFEDlg.cpp : implementation file
//

#include "stdafx.h"
#include "TestTFE.h"
#include "TestTFEDlg.h"

#include <TextFunctionExpander.h>
#include <UCLIDException.hpp>

#include <string>
#include <vector>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

using namespace std;

//-------------------------------------------------------------------------------------------------
// CTestTFEDlg dialog
//-------------------------------------------------------------------------------------------------
CTestTFEDlg::CTestTFEDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CTestTFEDlg::IDD, pParent)
{
	//{{AFX_DATA_INIT(CTestTFEDlg)
	m_zInput = _T("");
	m_zOutput = _T("");
	m_zParam = _T("");
	//}}AFX_DATA_INIT
	// Note that LoadIcon does not require a subsequent DestroyIcon in Win32
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}
//-------------------------------------------------------------------------------------------------
void CTestTFEDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CTestTFEDlg)
	DDX_Control(pDX, IDC_COMBO_FUNCTIONS, m_cmbFunctions);
	DDX_Control(pDX, IDOK, m_btnTest);
	DDX_Control(pDX, IDC_BUTTON_CLEAR, m_btnClear);
	DDX_Text(pDX, IDC_EDIT_INPUT, m_zInput);
	DDX_Text(pDX, IDC_EDIT_OUTPUT, m_zOutput);
	DDX_Text(pDX, IDC_EDIT_PARAM, m_zParam);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CTestTFEDlg, CDialog)
	//{{AFX_MSG_MAP(CTestTFEDlg)
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_BN_CLICKED(IDC_BUTTON_CLEAR, OnButtonClear)
	ON_CBN_EDITCHANGE(IDC_COMBO_FUNCTIONS, OnEditchangeComboFunctions)
	ON_EN_CHANGE(IDC_EDIT_INPUT, OnChangeEditInput)
	ON_EN_CHANGE(IDC_EDIT_PARAM, OnChangeEditParam)
	ON_CBN_SELCHANGE(IDC_COMBO_FUNCTIONS, OnSelchangeComboFunctions)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CTestTFEDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CTestTFEDlg::OnInitDialog()
{
	CDialog::OnInitDialog();

	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon
	
	// Populate combo box and select first item
	populateCombo();
	m_cmbFunctions.SetCurSel( 0 );
	updateButtons();
	
	return TRUE;  // return TRUE  unless you set the focus to a control
}
//-------------------------------------------------------------------------------------------------
// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.
void CTestTFEDlg::OnPaint() 
{
	if (IsIconic())
	{
		CPaintDC dc(this); // device context for painting

		SendMessage(WM_ICONERASEBKGND, (WPARAM) dc.GetSafeHdc(), 0);

		// Center icon in client rectangle
		int cxIcon = GetSystemMetrics(SM_CXICON);
		int cyIcon = GetSystemMetrics(SM_CYICON);
		CRect rect;
		GetClientRect(&rect);
		int x = (rect.Width() - cxIcon + 1) / 2;
		int y = (rect.Height() - cyIcon + 1) / 2;

		// Draw the icon
		dc.DrawIcon(x, y, m_hIcon);
	}
	else
	{
		CDialog::OnPaint();
	}
}
//-------------------------------------------------------------------------------------------------
// The system calls this to obtain the cursor to display while the user drags
//  the minimized window.
HCURSOR CTestTFEDlg::OnQueryDragIcon()
{
	return (HCURSOR) m_hIcon;
}
//-------------------------------------------------------------------------------------------------
void CTestTFEDlg::OnButtonClear() 
{
	// Clear input, output and parameters
	m_zInput = "";
	m_zOutput = "";
	m_zParam = "";

	UpdateData( FALSE );
	updateButtons();
}
//-------------------------------------------------------------------------------------------------
void CTestTFEDlg::OnEditchangeComboFunctions() 
{
	// Clear any existing output
	m_zOutput = "";
	UpdateData( FALSE );

	updateButtons();
}
//-------------------------------------------------------------------------------------------------
void CTestTFEDlg::OnSelchangeComboFunctions() 
{
	// Clear any existing output
	m_zOutput = "";
	UpdateData( FALSE );

	updateButtons();
}
//-------------------------------------------------------------------------------------------------
void CTestTFEDlg::OnChangeEditInput() 
{
	updateButtons();
}
//-------------------------------------------------------------------------------------------------
void CTestTFEDlg::OnChangeEditParam() 
{
	updateButtons();
}
//-------------------------------------------------------------------------------------------------
void CTestTFEDlg::OnOK() 
{
	try
	{
		// Get current TFE item
		CString	zText;
		int nIndex = m_cmbFunctions.GetCurSel();
		m_cmbFunctions.GetLBText( nIndex, zText );
		int lLength = zText.GetLength();

		///////////////////////////////////////////
		// Build the function string to be expanded
		///////////////////////////////////////////

		// Delete trailing )
		zText.Delete( lLength - 1, 1 );
		string strTotal = zText;
		if (m_zInput != "")
		{
			strTotal += m_zInput.operator LPCTSTR();
		}

		if (m_zParam != "")
		{
			strTotal += ",";
			strTotal += m_zParam.operator LPCTSTR();
		}

		// Replace trailing )
		strTotal += ")";

		// Exercise the function
		TextFunctionExpander tfe;
		string strResult = tfe.expandFunctions( strTotal );
		m_zOutput = strResult.c_str();

		UpdateData( FALSE );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI12728")
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
bool CTestTFEDlg::isParameterMissing() 
{
	// Get current TFE item
	CString	zText;
	int nIndex = m_cmbFunctions.GetCurSel();
	m_cmbFunctions.GetLBText( nIndex, zText );

	// Remove leading $ and trailing ()
	if (zText.GetLength() > 3)
	{
		long lLength = zText.GetLength();
		zText.Delete( 0, 1 );
		zText.Delete( lLength - 3, 2 );

		// Test this function with provided argument and possible parameter(s)
		TextFunctionExpander tfe;
		bool bResult = tfe.isValidParameters( zText.operator LPCTSTR(), 
			m_zInput.operator LPCTSTR(), m_zParam.operator LPCTSTR() );

		return bResult ? false : true;
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
void CTestTFEDlg::populateCombo() 
{
	// Retrieve the TextFunctionExpander functions
	TextFunctionExpander tfe;
	vector<string> vecFunctions = tfe.getAvailableFunctions();
	tfe.formatFunctions(vecFunctions);

	// Add items to combo box
	int i;
	for (i = 0; i < vecFunctions.size(); i++)
	{
		m_cmbFunctions.AddString( vecFunctions[i].c_str() );
	}
}
//-------------------------------------------------------------------------------------------------
void CTestTFEDlg::updateButtons() 
{
	UpdateData( TRUE );

	if (m_zInput.IsEmpty())
	{
		m_btnTest.EnableWindow( FALSE );
	}
	else if (isParameterMissing())
	{
		m_btnTest.EnableWindow( FALSE );
	}
	else
	{
		m_btnTest.EnableWindow( TRUE );
	}
}
//-------------------------------------------------------------------------------------------------
