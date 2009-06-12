// StringPatternMatcherDlg.cpp : implementation file
//

#include "stdafx.h"
#include "StringPatternMatcher.h"
#include "StringPatternMatcherDlg.h"

#include <LicenseMgmt.h>
#include <UCLIDException.hpp>

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CStringPatternMatcherDlg dialog

CStringPatternMatcherDlg::CStringPatternMatcherDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CStringPatternMatcherDlg::IDD, pParent)
{
	//{{AFX_DATA_INIT(CStringPatternMatcherDlg)
	m_zInput = _T("");
	m_zPattern = _T("");
	m_iOption = -1;
	m_zMatch1Expr = _T("");
	m_zMatch2Expr = _T("");
	//}}AFX_DATA_INIT
	// Note that LoadIcon does not require a subsequent DestroyIcon in Win32
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);

	LicenseManagement::sGetInstance().loadLicenseFilesFromFolder();

	// create instance of regular expresion parser object
	if (FAILED(m_ipRegExpr.CreateInstance(CLSID_VBScriptParser)))
	{
		MessageBox("Unable to create regular expression object!");
	}
	else
	{
		m_ipRegExpr->IgnoreCase = VARIANT_FALSE;
	}

	// create instance of string pattern matcher object
	if (FAILED(m_ipStringPatternMatcher.CreateInstance(CLSID_StringPatternMatcher)))
	{
		MessageBox("Unable to create string pattern matcher object!");
	}
}

void CStringPatternMatcherDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CStringPatternMatcherDlg)
	DDX_Control(pDX, IDC_MATCHES_LIST, m_listMatches);
	DDX_Text(pDX, IDC_EDIT_INPUT, m_zInput);
	DDX_Text(pDX, IDC_EDIT_PATTERN, m_zPattern);
	DDX_Radio(pDX, IDC_RADIO1, m_iOption);
	DDX_Text(pDX, IDC_EDIT_MATCH1NUM, m_zMatch1Expr);
	DDX_Text(pDX, IDC_EDIT_MATCH2NUM, m_zMatch2Expr);
	//}}AFX_DATA_MAP
}

BEGIN_MESSAGE_MAP(CStringPatternMatcherDlg, CDialog)
	//{{AFX_MSG_MAP(CStringPatternMatcherDlg)
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_BN_CLICKED(IDC_BUTTON_CLEAR, OnButtonClear)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CStringPatternMatcherDlg message handlers

BOOL CStringPatternMatcherDlg::OnInitDialog()
{
	CDialog::OnInitDialog();

	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon
	
	//to get the size of the list controls
	CRect rect;
	m_listMatches.GetClientRect(&rect);

	//enable  full row selection and grid lines
	m_listMatches.SetExtendedStyle(LVS_EX_GRIDLINES | LVS_EX_FULLROWSELECT );

	//now insert columns in the ELI and Text list control
	m_listMatches.InsertColumn(0,"Name",LVCFMT_LEFT,rect.Width()/3,0);
	m_listMatches.InsertColumn(1,"Value",LVCFMT_LEFT,rect.Width()*2/3,1);

	// TODO: Add extra initialization here
	m_iOption = 1;
	m_zPattern = "this|that^?^test";
	m_zInput = "which says that this is a test of Madison";
	m_zMatch1Expr = "$1";
	m_zMatch2Expr = "$2";
	UpdateData(FALSE);

	return TRUE;  // return TRUE  unless you set the focus to a control
}

// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.

void CStringPatternMatcherDlg::OnPaint() 
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

// The system calls this to obtain the cursor to display while the user drags
//  the minimized window.
HCURSOR CStringPatternMatcherDlg::OnQueryDragIcon()
{
	return (HCURSOR) m_hIcon;
}

map<string, string> CStringPatternMatcherDlg::doUCLIDMatch()
{
	map<string, string> mapResults;

	_bstr_t _bstrText = (LPCTSTR) m_zInput;
	_bstr_t _bstrPattern = (LPCTSTR) m_zPattern;
	
	// create the map
	IStrToStrMapPtr ipMap(CLSID_StrToStrMap);
	ASSERT_RESOURCE_ALLOCATION("ELI06270", ipMap != NULL);

	// since the UI has no way of specifying expressions, any expression
	// definition must be done here manually.  Below are some samples for
	// testing purposes.
	ipMap->Set(_bstr_t("NonTP"), _bstr_t(" \n\r\taAbBcCdDeEfFgGhHiIjJkKlLmMnNoOpPqQrRsStTuUvVwWxXyYzZ,."));
	ipMap->Set(_bstr_t("Test5"), _bstr_t("a|sdasdasd")); // invalid value
	ipMap->Set(_bstr_t("Test4"), _bstr_t("as@dasdasd")); // invalid value
	ipMap->Set(_bstr_t("Test3"), _bstr_t("asda?sdasd")); // invalid value
	ipMap->Set(_bstr_t("Test2"), _bstr_t("asdasd^asd")); // invalid value
	ipMap->Set(_bstr_t("GrantKeywords"), _bstr_t("grant|assign|convey|sell"));
	ipMap->Set(_bstr_t("Test1"), _bstr_t(""));  // invalid value

	// find the matches
	IStrToObjectMapPtr ipResults = m_ipStringPatternMatcher->Match1(_bstrText,
		_bstrPattern, ipMap, VARIANT_TRUE);

	// for each found match, push a string into the result vector with
	// the details of the match
	for (int i = 0; i < ipResults->Size; i++)
	{
		IUnknownPtr ipUnknown;
		CComBSTR bstrVariableName;
		ipResults->GetKeyValue(i, &bstrVariableName, &ipUnknown);
		ITokenPtr ipToken = ipUnknown;
		ASSERT_RESOURCE_ALLOCATION("ELI06269", ipToken != NULL);

		_bstr_t _bstrItem = ipToken->Value;
		string strItem = _bstrItem;
		long nStart, nEnd;
		nStart = ipToken->StartPosition;
		nEnd = ipToken->EndPosition;
		CString zTemp;
		zTemp.Format("{%s} {%d} {%d}", strItem.c_str(), nStart, nEnd);

		string strVariableName = _bstr_t(bstrVariableName.m_str);
		mapResults[strVariableName] = (LPCTSTR) zTemp;
	}

	return mapResults;
}

map<string, string> CStringPatternMatcherDlg::doRegExMatch()
{
	// find the matches using the regular expression parser
	map<string, string> mapResults;
	string strText = (LPCTSTR) m_zInput;
	string strPattern = (LPCTSTR) m_zPattern;
	
	// find first match
	m_ipRegExpr->Pattern = _bstr_t(strPattern.c_str());
	string strMatch1 = string(m_ipRegExpr->ReplaceMatches(
						_bstr_t(strText.c_str()), _bstr_t(m_zMatch1Expr), VARIANT_FALSE));
	mapResults["Match1"] = strMatch1;
	
	// find second match if appropriate
	if (m_zMatch2Expr != "")
	{
		string strMatch2 = string(m_ipRegExpr->ReplaceMatches(
						_bstr_t(strText.c_str()), _bstr_t(m_zMatch2Expr), VARIANT_FALSE));
		mapResults["Match2"] = strMatch2;
	}

	return mapResults;
}

void CStringPatternMatcherDlg::OnOK() 
{
	try
	{
		// get the input data from the UI
		UpdateData(TRUE);

		// by default assme no matches will be found
		m_listMatches.DeleteAllItems();

		// log the start time
		CTime start = CTime::GetCurrentTime();
		
		long nIterations = 1;
		//long nIterations = 10000;
		
		// do the search in a loop corresponding to nIterations
		map<string, string> mapResults;
		{
			CWaitCursor wait;
			for (int i = 0; i < nIterations; i++)
			{
				switch (m_iOption)
				{
				case 0:
					mapResults = doRegExMatch();
					break;
				case 1:
					mapResults = doUCLIDMatch();
					break;
				}
			}
		}

		// log the end time and calculate the total time duration
		CTime end = CTime::GetCurrentTime();
		CTimeSpan duration = end - start;
		
		// display the total duration elapsed
		long nSeconds = duration.GetTotalSeconds();
		CString zTemp;
		zTemp.Format("Total search time = %0.3f ms", nSeconds * 1000 / (nIterations * 1.0));
		MessageBox(zTemp);

		map<string, string>::const_iterator iter;
		int i = 0;
		for (iter = mapResults.begin(); iter != mapResults.end(); iter++)
		{
			m_listMatches.InsertItem(i, iter->first.c_str());
			m_listMatches.SetItemText(i, 1, iter->second.c_str());
			i++;
		}

		UpdateData(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05945")
}

void CStringPatternMatcherDlg::OnButtonClear() 
{
	m_listMatches.DeleteAllItems();
}
