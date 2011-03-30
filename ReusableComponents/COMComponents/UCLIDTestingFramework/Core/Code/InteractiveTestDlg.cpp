// InteractiveTestDlg.cpp : implementation file
//

#include "stdafx.h"
#include "resource.h"
#include "InteractiveTestDlg.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <PromptDlg.h>

#include <io.h>
#include <fstream>
#include <algorithm>
using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// InteractiveTestDlg dialog


InteractiveTestDlg::InteractiveTestDlg(ITestResultLoggerPtr ipResultLogger,
									   const char *pszITCFile, CWnd* pParent /*=NULL*/)
:CDialog(InteractiveTestDlg::IDD, pParent),
 m_ipResultLogger(ipResultLogger),
 m_bOnSizeInit(false)
{
	//{{AFX_DATA_INIT(InteractiveTestDlg)
	m_zDescription = _T("");
	m_zTestCaseID = _T("");
	//}}AFX_DATA_INIT

	ASSERT_ARGUMENT("ELI02260", pszITCFile != __nullptr);

	// process the input file
	processInputFile(pszITCFile);
}


void InteractiveTestDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(InteractiveTestDlg)
	DDX_Text(pDX, IDC_EDIT_DESCRIPTION, m_zDescription);
	DDX_Text(pDX, IDC_EDIT_TEST_CASE_ID, m_zTestCaseID);
	DDX_Control(pDX, IDC_WEB_BROWSER, m_browser);
	//}}AFX_DATA_MAP
}


BEGIN_MESSAGE_MAP(InteractiveTestDlg, CDialog)
	//{{AFX_MSG_MAP(InteractiveTestDlg)
	ON_BN_CLICKED(IDB_ADD_EXCEPTION, OnAddException)
	ON_BN_CLICKED(IDB_ADD_NOTE, OnAddNote)
	ON_BN_CLICKED(IDB_FAIL, OnFail)
	ON_BN_CLICKED(IDB_PASS, OnPass)
	ON_WM_SIZE()
	ON_WM_GETMINMAXINFO()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// InteractiveTestDlg message handlers

void InteractiveTestDlg::OnAddException() 
{
	PromptDlg dlg("Add Exception", "Stringized exception");
	if (dlg.DoModal() == IDOK)
	{
		_bstr_t bstrException = dlg.m_zInput;
		m_ipResultLogger->AddTestCaseException(bstrException, VARIANT_FALSE);
	}
}

void InteractiveTestDlg::OnAddNote() 
{
	PromptDlg dlg("Add Note", "Note");
	if (dlg.DoModal() == IDOK)
	{
		_bstr_t bstrNote = dlg.m_zInput;
		m_ipResultLogger->AddTestCaseNote(bstrNote);
	}
}

void InteractiveTestDlg::OnFail() 
{
	processTestCaseCompletion(false);
}

void InteractiveTestDlg::OnPass() 
{
	processTestCaseCompletion(true);
}

void InteractiveTestDlg::processTestCaseCompletion(bool bResult)
{
	// tell the result logger about the result associated with this test case
	m_ipResultLogger->EndTestCase(bResult ? VARIANT_TRUE : VARIANT_FALSE);

	if (m_ulCurrentTestCase != m_vecTestCaseInfo.size() - 1)
	{
		setCurrentTestCase(m_ulCurrentTestCase + 1);
	}
	else
	{
		// we have shown all test cases.  close this dialog box
		CDialog::OnOK();
		MessageBox("All test cases executed.", "Information", MB_ICONINFORMATION);
	}
}

BOOL InteractiveTestDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		CDialog::OnInitDialog();
		
		CString zTitle = "Interactive Test - ";
		zTitle += getFileNameFromFullPath(m_strITCFile).c_str();
		SetWindowText(zTitle);

		setCurrentTestCase(0);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18627");
	
	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}

void InteractiveTestDlg::processInputFile(const char *pszITCFile)
{
	m_strITCFile = pszITCFile;

	// clear the vector
	m_ulCurrentTestCase = 0;
	m_vecTestCaseInfo.clear();

	// ensure that the file exists
	if (!isValidFile(m_strITCFile))
	{
		UCLIDException ue("ELI02255", "The specified file name is not valid!");
		ue.addDebugInfo("Filename", m_strITCFile);
		ue.addWin32ErrorInfo();
		throw ue;
	}

	// open the input file and start reading test case items
	ifstream itcFile(m_strITCFile.c_str());
	if (!itcFile)
	{
		UCLIDException ue("ELI02248", "Unable to open specified file!");
		ue.addDebugInfo("Filename", m_strITCFile);
		throw ue;
	}

	do
	{
		TestCaseInfo testCaseInfo;
		itcFile >> testCaseInfo;
		
		// make sure there was no problem reading data from the file
		if (itcFile && testCaseInfo.m_strTestCaseID != "")
		{
			// make sure that each test case has a unique id
			if (find(m_vecTestCaseInfo.begin(), m_vecTestCaseInfo.end(), testCaseInfo) != 
				m_vecTestCaseInfo.end())
			{
				UCLIDException ue("ELI02244", "Duplicate test case definition - test case ID's not unique!");
				ue.addDebugInfo("TestCaseID", testCaseInfo.m_strTestCaseID);
				throw ue;
			}

			// the test case is unique - add it to the vector
			m_vecTestCaseInfo.push_back(testCaseInfo);
		}

	} 
	while (!itcFile.eof());

	// ensure that the ITC file contains at least one test case
	if (m_vecTestCaseInfo.empty())
	{
		UCLIDException ue("ELI02245", "No test cases defined in specified file.");
		ue.addDebugInfo("File", m_strITCFile);
		throw ue;
	}
}

void InteractiveTestDlg::setCurrentTestCase(unsigned long ulIndex)
{
	m_ulCurrentTestCase = ulIndex;

	// update the test case id and description
	m_zTestCaseID = m_vecTestCaseInfo[ulIndex].m_strTestCaseID.c_str();
	m_zDescription = m_vecTestCaseInfo[ulIndex].m_strTestCaseDescription.c_str();
	UpdateData(FALSE);

	// tell the result logger that we are starting this test case
	_bstr_t bstrTestCaseID = m_zTestCaseID;
	_bstr_t bstrTestCaseDescription = m_zDescription;
	m_ipResultLogger->StartTestCase(bstrTestCaseID, bstrTestCaseDescription, kInteractiveTestCase);

	// update the html documentation for the test case
	string strHTMDocFile = ::getAbsoluteFileName(m_strITCFile, m_vecTestCaseInfo[ulIndex].m_strTestCaseHtmlDoc);
	m_browser.Navigate(strHTMDocFile.c_str(), NULL, NULL, NULL, NULL);

	// update the title of the dialog box
	string strTitle = "Interactive test - ";
	strTitle += bstrTestCaseDescription;
	SetWindowText(strTitle.c_str());
}

void InteractiveTestDlg::OnSize(UINT nType, int cx, int cy) 
{
	try
	{		
		CDialog::OnSize(nType, cx, cy);
		
		// do nothing if the window is minimized
		if (!IsIconic())
		{
			// initially, there's no old x or old y
			static int nOldX = 0;
			static int nOldY = 0;
			
			// get the offset as the result of the resizing
			int dX = cx - nOldX;
			int dY = cy - nOldY;
			
			// if there's an nOldX (or nOldY)
			if (m_bOnSizeInit)
			{		
				////////////////////////////////////////////////
				// expanding or contracting the browser control
				CRect WndRect;
				m_browser.GetWindowRect(&WndRect);	
				ScreenToClient(WndRect);
				WndRect.right += dX;
				WndRect.bottom += dY;					
				m_browser.MoveWindow(WndRect);
				m_browser.InvalidateRect(&WndRect);
				m_browser.Invalidate();
				
				/////////////////////////////////////////////////
				// edit box of IDC_EDIT_TEST_CASE_ID can only be
				// strentched horizontally
				resizeEditControl(IDC_EDIT_TEST_CASE_ID, dX);
				
				/////////////////////////////////////////////////
				// edit box of IDC_EDIT_DESCRIPTION can only be
				// strentched horizontally
				resizeEditControl(IDC_EDIT_DESCRIPTION, dX);
				
				/////////////////////////////////////////////////
				// button of IDB_PASS can only be
				// moved horizontally or verically, no strentching
				moveButtonControl(IDB_PASS, dX, dY);
				
				/////////////////////////////////////////////////
				// button of IDB_FAIL can only be
				// moved horizontally or verically, no strentching
				moveButtonControl(IDB_FAIL, dX, dY);
				
				/////////////////////////////////////////////////
				// button of IDB_ADD_NOTE can only be
				// moved horizontally or verically, no strentching
				moveButtonControl(IDB_ADD_NOTE, dX, dY);
				
				/////////////////////////////////////////////////
				// button of IDB_ADD_EXCEPTION can only be
				// moved horizontally or verically, no strentching
				moveButtonControl(IDB_ADD_EXCEPTION, dX, dY);
			}
			
			m_bOnSizeInit = true;

			nOldX = cx;
			nOldY = cy;
		}
	}
	CATCH_UCLID_EXCEPTION("ELI02685")
	CATCH_UNEXPECTED_EXCEPTION("ELI02686")
}

void InteractiveTestDlg::OnGetMinMaxInfo(MINMAXINFO* lpMMI)
{
	try
	{
		// Minimum width/height
		lpMMI->ptMinTrackSize.x = 432;
		lpMMI->ptMinTrackSize.y = 280;
	}
	CATCH_UCLID_EXCEPTION("ELI02687")
	CATCH_UNEXPECTED_EXCEPTION("ELI02688")
}

//////////////////////////////////////////////////////////////////////////////////////////////////
//		Helper Functions
//////////////////////////////////////////////////////////////////////////////////////////////////
void InteractiveTestDlg::moveButtonControl(UINT nControlID, int nMoveXUnits, int nMoveYUnits)
{
	CRect WndRect;
	CWnd *pWnd;
	
	pWnd = GetDlgItem (nControlID);
	pWnd->GetWindowRect(&WndRect);  
	
	ScreenToClient(&WndRect);
	
	WndRect.OffsetRect(nMoveXUnits, nMoveYUnits);
	pWnd->MoveWindow(&WndRect);
	pWnd->InvalidateRect(&WndRect);
	pWnd->Invalidate(false);
}

void InteractiveTestDlg::resizeEditControl(UINT nControlID, int nXUnits)
{
	CRect WndRect;
	CWnd *pWnd;

	pWnd = GetDlgItem (nControlID);
	pWnd->GetWindowRect(&WndRect);  

	ScreenToClient(&WndRect);

	WndRect.right += nXUnits;

	pWnd->MoveWindow(&WndRect);
	pWnd->InvalidateRect(&WndRect);
	pWnd->Invalidate(false);
}

