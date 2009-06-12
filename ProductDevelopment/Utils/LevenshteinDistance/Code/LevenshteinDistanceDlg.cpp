//============================================================================
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	LevenshteinDistanceDlg.cpp
//
// PURPOSE:	Implementation of CLevenshteinDistanceDlg class
//
// NOTES:	
//
// AUTHORS:	Ryan Mulder
//
//============================================================================
#include "stdafx.h"
#include "LevenshteinDistanceDlg.h"
#include "ClipboardManager.h"

#include <LevenshteinDistance.h>

#include <cpputil.hpp>
#include <UCLIDException.hpp>

#include <string.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

// CAboutDlg dialog used for App About

class CAboutDlg : public CDialog
{
public:
	CAboutDlg();

// Dialog Data
	enum { IDD = IDD_ABOUTBOX };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

// Implementation
protected:
	DECLARE_MESSAGE_MAP()
};

CAboutDlg::CAboutDlg() : CDialog(CAboutDlg::IDD)
{
}

void CAboutDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
}

BEGIN_MESSAGE_MAP(CAboutDlg, CDialog)
END_MESSAGE_MAP()
// CLevenshteinDistanceDlg dialog

CLevenshteinDistanceDlg::CLevenshteinDistanceDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CLevenshteinDistanceDlg::IDD, pParent)
{
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
	m_zExpected = _T("");
	m_zFound = _T("");
	m_zLDPercent = "0.0";
	m_iCaseSensitive = 0;
	m_iRemWS = 0;
	m_iUpdate = 0;
	m_iLDist = 0;
	m_iExpectedLen = 0;
	m_iFoundLen = 0;
	m_dLDPercent = 0.0;
}

void CLevenshteinDistanceDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Text(pDX, IDC_EDIT_TOPWORD, m_zExpected );
	DDX_Text(pDX, IDC_EDIT_BOTWORD, m_zFound );
	DDX_Text(pDX, IDC_EDIT_LDPERCENT, m_zLDPercent );
	DDX_Control(pDX, IDC_COPY_BTN, m_btnCopy);
	DDX_Text(pDX, IDC_EDIT_LDIST, m_iLDist );
	DDX_Text(pDX, IDC_EDIT_EXPECTED_LEN, m_iExpectedLen);
	DDX_Text(pDX, IDC_EDIT_FOUND_LEN, m_iFoundLen);
	DDX_Check(pDX, IDC_CHECK_REMWS, m_iRemWS);
	DDX_Check(pDX, IDC_CHECK_CASE, m_iCaseSensitive);
	DDX_Check(pDX, IDC_CHECK_UPDATE, m_iUpdate);
}

BEGIN_MESSAGE_MAP(CLevenshteinDistanceDlg, CDialog)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	//}}AFX_MSG_MAP
	ON_BN_CLICKED(IDC_GET_LEV_BTN, &CLevenshteinDistanceDlg::OnBnClickedGetLevBtn)
	ON_BN_CLICKED(IDC_COPY_BTN, &CLevenshteinDistanceDlg::OnBnClickedCopyBtn)
	ON_BN_CLICKED(IDC_EXIT_BTN, &CLevenshteinDistanceDlg::OnBnClickedExitBtn)
	ON_STN_CLICKED(IDC_STATIC_TRUEBOXES_INFO, &CLevenshteinDistanceDlg::OnStnClickedStaticTrueboxesInfo)
END_MESSAGE_MAP()

// CLevenshteinDistanceDlg message handlers
BOOL CLevenshteinDistanceDlg::OnInitDialog()
{
	CDialog::OnInitDialog();

	// create tooltip object
	m_infoTip.Create(CWnd::FromHandle(m_hWnd));
	// set no delay.
	m_infoTip.SetShowDelay(0);

	// Add "About..." menu item to system menu.
	// IDM_ABOUTBOX must be in the system command range.
	ASSERT((IDM_ABOUTBOX & 0xFFF0) == IDM_ABOUTBOX);
	ASSERT(IDM_ABOUTBOX < 0xF000);

	CMenu* pSysMenu = GetSystemMenu(FALSE);
	if (pSysMenu != NULL)
	{
		CString strAboutMenu;
		strAboutMenu.LoadString(IDS_ABOUTBOX);
		if (!strAboutMenu.IsEmpty())
		{
			pSysMenu->AppendMenu(MF_SEPARATOR);
			pSysMenu->AppendMenu(MF_STRING, IDM_ABOUTBOX, strAboutMenu);
		}
	}

	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon

	// TODO: Add extra initialization here
	CEdit *pEdit;
	pEdit = (CEdit *)GetDlgItem( IDC_EDIT_TOPWORD );
	ASSERT_RESOURCE_ALLOCATION("ELI13453", pEdit != NULL);
	pEdit->SetFocus();

	return FALSE;  // return TRUE  unless you set the focus to a control
}

void CLevenshteinDistanceDlg::OnSysCommand(UINT nID, LPARAM lParam)
{
	if ((nID & 0xFFF0) == IDM_ABOUTBOX)
	{
		CAboutDlg dlgAbout;
		dlgAbout.DoModal();
	}
	else
	{
		CDialog::OnSysCommand(nID, lParam);
	}
}

// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.
void CLevenshteinDistanceDlg::OnPaint()
{
	if (IsIconic())
	{
		CPaintDC dc(this); // device context for painting

		SendMessage(WM_ICONERASEBKGND, reinterpret_cast<WPARAM>(dc.GetSafeHdc()), 0);

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

// The system calls this function to obtain the cursor to display while the user drags
//  the minimized window.
HCURSOR CLevenshteinDistanceDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}

void CLevenshteinDistanceDlg::OnBnClickedGetLevBtn()
{
	if(! UpdateData( TRUE ))
	{
		UCLIDException ue("ELI13455", "Update Data Failed!");
		ue.display();
	}
	// TODO: Add your control notification handler code here
	//convert the edit box text to a string
	std::string strExpected((LPCTSTR)m_zExpected);
	std::string strFound((LPCTSTR)m_zFound);
		
	LevenshteinDistance LD;

	//prepare bools to SetFlags
	bool bRemWS = false;
	bool bCase = false;
	bool bUpdate = false;
	if(m_iRemWS == 1)
	{
		bRemWS = true;
	}
	if(m_iCaseSensitive == 1)
	{
		bCase = true;
	}
	if(m_iUpdate == 1)
	{
		bUpdate = true;
	}

	//set the member variables to reflect the checkboxes.
	LD.SetFlags(bRemWS, bCase, bUpdate);

	//use the strings to compute the Lev Distance.
	int iLevDistance = -1;
	iLevDistance = LD.GetLevDistance(strExpected, strFound);	

	//verify that a meaningful value was returned.
	if ( iLevDistance == -1 ) 
	{
		UCLIDException ue("ELI13460", "GetLevDistance did not return a value!");
		ue.display();
	}
	
	//put the string back in the box (update is controlled by a bool inside
	//GetLevDistance.) 
	m_zExpected = strExpected.c_str();
	m_zFound = strFound.c_str();

	//put the Levenshtein Distance in the text box
	m_iLDist = iLevDistance;
	
	//Get the Lev Percentage
	m_dLDPercent = LD.GetLevPercent();

	//Turn it into a string
	string strTemp = asString(m_dLDPercent);
	
	//Trim some decimal places
	strTemp.erase(5,strTemp.size());

	//put it into the textbox
	m_zLDPercent = strTemp.c_str();

	//update the size of the strings in the characters: boxes
	m_iExpectedLen = static_cast<int>( strExpected.size() );
	m_iFoundLen = static_cast<int>( strFound.size() );


	if(! UpdateData( FALSE ))
	{
		UCLIDException ue("ELI13462", "Update Data Failed!");
		ue.display();
	}

	//enable the copy button
	m_btnCopy.EnableWindow( TRUE );
}
//-------------------------------------------------------------------------------------------------
//This copies the 2 textboxes and the LDistance to the clipboard (with labels)
//for pasting into a text document. 
void CLevenshteinDistanceDlg::OnBnClickedCopyBtn()
{
	// TODO: Add your control notification handler code here
	string strCombined = "";

	if(! UpdateData( TRUE ))
	{
		UCLIDException ue("ELI13463", "Update Data Failed!");
		ue.display();
	}

	//This is all needed for the included clipboard manager
	CWnd* pWnd = this;
	ClipboardManager cMan(pWnd);

	//put all the pieces together with "\r\n" as the delimeter
	strCombined = "Expected: \r\n"  + m_zExpected + "\r\n" + "Found: \r\n" +
					m_zFound + "\r\n" + "Levenshtein Distance: " ;

	//convert the Ldist and LDPercent to strings and concatenate them to the big string
	strCombined = strCombined + asString(m_iLDist);
	strCombined = strCombined + "\r\nLevenshtein Percent: " + asString(m_dLDPercent);

	//ClipboardManager will take care of copying the string to the clipboard and
	//throwing any appropriate exceptions.
	cMan.writeText(strCombined);
}
//-------------------------------------------------------------------------------------------------
void CLevenshteinDistanceDlg::OnBnClickedExitBtn()
{
	// TODO: Add your control notification handler code here
	exit(0);
}
//-------------------------------------------------------------------------------------------------
void CLevenshteinDistanceDlg::OnStnClickedStaticTrueboxesInfo()
{
	// TODO: Add your control notification handler code here
	try
	{
		m_infoTip.Show("What are True Boxes?\n\n"
					"True Boxes will update the Expected and Found text-\n"
					"boxes with the data that is being tested. This data\n"
					"may be altered from the originally entered text based\n"
					"on the checkboxes that the user has selected.");
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13464");
	return;

}

