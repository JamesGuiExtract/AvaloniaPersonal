// TestResultLoggerDlg.cpp : implementation file

#include "stdafx.h"
#include "resource.h"
#include "TestResultLoggerDlg.h"

#include <UCLIDException.h>
#include <UCLIDExceptionDlg.h>
#include <cpputil.h>
#include <TemporaryFileName.h>
#include <StringTokenizer.h>
#include <RegConstants.h>

#include <fstream>
#include <io.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

TemporaryFileName g_tmpTxtFileName("recognized_", ".txt");
TemporaryFileName g_tmpTxtFileNameB("recognized_B", ".txt");

using namespace std;

const string gstrSCROLL_LOGGER_KEY = "ScrollLogger";
const string gstrRETAIN_NODES_KEY = "RetainTestCaseNodes";
const string gstrDIFF_COMMAND_LINE_KEY = "DiffCommandLine";
const string gstrDEFAULT_DIFF_CMD = "C:\\Program Files\\KDiff3\\kdiff3.exe %1 %2";
const string gstrDELIMITER = "^|&$";

//-------------------------------------------------------------------------------------------------
// TestResultLoggerDlg dialog
//-------------------------------------------------------------------------------------------------
TestResultLoggerDlg::TestResultLoggerDlg(CWnd* pParent /*=NULL*/)
:	CDialog(TestResultLoggerDlg::IDD, pParent),
	m_bCollapseOnFail(false)
{
	//{{AFX_DATA_INIT(TestResultLoggerDlg)
	m_zNote = _T("");
	//}}AFX_DATA_INIT
	
	m_bTestHarnessActive = false;
	m_bComponentTestActive = false;
	m_bTestCaseActive = false;

	// create an instance of the configuration persistence manager
	m_apCfgMgr = auto_ptr<IConfigurationSettingsPersistenceMgr>(new 
		RegistryPersistenceMgr(HKEY_CURRENT_USER,
		gstrREG_ROOT_KEY + "\\TestingFramework\\Settings"));
	ASSERT_RESOURCE_ALLOCATION("ELI08575", m_apCfgMgr.get() != NULL);
}
//-------------------------------------------------------------------------------------------------
TestResultLoggerDlg::~TestResultLoggerDlg()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16544");
}
//-------------------------------------------------------------------------------------------------
void TestResultLoggerDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(TestResultLoggerDlg)
	DDX_Control(pDX, IDC_EDIT_CURRENT_NOTE, m_editNote);
	DDX_Control(pDX, IDC_EDIT_NOTE_RIGHT, m_editNoteRight);
	DDX_Control(pDX, IDC_TREE, m_tree);
	DDX_Text(pDX, IDC_EDIT_CURRENT_NOTE, m_zNote);
	DDX_Text(pDX, IDC_EDIT_NOTE_RIGHT, m_zNoteRight);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(TestResultLoggerDlg, CDialog)
	//{{AFX_MSG_MAP(TestResultLoggerDlg)
	ON_NOTIFY(NM_DBLCLK, IDC_TREE, OnDblclkTree)
	ON_WM_SIZE()
	ON_WM_GETMINMAXINFO()
	ON_NOTIFY(TVN_SELCHANGED, IDC_TREE, OnSelchangedTree)
	ON_WM_CLOSE()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// TestResultLoggerDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL TestResultLoggerDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		CDialog::OnInitDialog();
		
		m_apImageList = auto_ptr<CImageList>(new CImageList());
		m_apImageList->Create(16, 16, ILC_MASK, 0, 1);

		//COLORREF mask = GetSysColor(COLOR_3DFACE);
		COLORREF mask = RGB(192, 192, 192);

		m_apBmpHarness = auto_ptr<CBitmap>(new CBitmap());
		m_apBmpHarness->LoadBitmap(IDB_BITMAP_HARNESS);
		m_iBitmapHarness = m_apImageList->Add(m_apBmpHarness.get(), mask);
		
		m_apBmpComponent = auto_ptr<CBitmap>(new CBitmap());
		m_apBmpComponent->LoadBitmap(IDB_BITMAP_COMPONENT);
		m_iBitmapComponent = m_apImageList->Add(m_apBmpComponent.get(), mask);
		
		m_apBmpTestCaseDoneGood = auto_ptr<CBitmap>(new CBitmap());
		m_apBmpTestCaseDoneGood->LoadBitmap(IDB_BITMAP_TEST_CASE_DONE_GOOD);
		m_iBitmapTestCaseDoneGood = m_apImageList->Add(m_apBmpTestCaseDoneGood.get(), mask);

		m_apBmpTestCaseDoneBad = auto_ptr<CBitmap>(new CBitmap());
		m_apBmpTestCaseDoneBad->LoadBitmap(IDB_BITMAP_TEST_CASE_DONE_BAD);
		m_iBitmapTestCaseDoneBad = m_apImageList->Add(m_apBmpTestCaseDoneBad.get(), mask);

		m_apBmpTestCaseInProgress = auto_ptr<CBitmap>(new CBitmap());
		m_apBmpTestCaseInProgress->LoadBitmap(IDB_BITMAP_TEST_CASE_IN_PROGRESS);
		m_iBitmapTestCaseInProgress = m_apImageList->Add(m_apBmpTestCaseInProgress.get(), mask);

		m_apBmpTestCaseDoneOtherGood = auto_ptr<CBitmap>(new CBitmap());
		m_apBmpTestCaseDoneOtherGood->LoadBitmap(IDB_BITMAP_TEST_CASE_DONE_OTHER_GOOD);
		m_iBitmapTestCaseDoneOtherGood = m_apImageList->Add(m_apBmpTestCaseDoneOtherGood.get(), mask);

		m_apBmpTestCaseDoneOtherBad = auto_ptr<CBitmap>(new CBitmap());
		m_apBmpTestCaseDoneOtherBad->LoadBitmap(IDB_BITMAP_TEST_CASE_DONE_OTHER_BAD);
		m_iBitmapTestCaseDoneOtherBad = m_apImageList->Add(m_apBmpTestCaseDoneOtherBad.get(), mask);

		m_apBmpNote = auto_ptr<CBitmap>(new CBitmap());
		m_apBmpNote->LoadBitmap(IDB_BITMAP_NOTE);
		m_iBitmapNote = m_apImageList->Add(m_apBmpNote.get(), mask);

		m_apBmpDetailNote = auto_ptr<CBitmap>(new CBitmap());
		m_apBmpDetailNote->LoadBitmap(IDB_DETAILED_NOTE);
		m_iBitmapDetailNote = m_apImageList->Add(m_apBmpDetailNote.get(), mask);

		m_apBmpMemo = auto_ptr<CBitmap>(new CBitmap());
		m_apBmpMemo->LoadBitmap(IDB_BITMAP_MEMO);
		m_iBitmapMemo = m_apImageList->Add(m_apBmpMemo.get(), mask);

		m_apBmpFile = auto_ptr<CBitmap>(new CBitmap());
		m_apBmpFile->LoadBitmap(IDB_BITMAP_FILE);
		m_iBitmapFile = m_apImageList->Add(m_apBmpFile.get(), mask);

		m_apBmpException = auto_ptr<CBitmap>(new CBitmap());
		m_apBmpException->LoadBitmap(IDB_BITMAP_EXCEPTION);
		m_iBitmapException = m_apImageList->Add(m_apBmpException.get(), mask);

		m_tree.SetImageList(m_apImageList.get(), TVSIL_NORMAL);

		//disable the window.
		m_editNoteRight.ShowWindow(FALSE);

		//enableWindow is used as a flag for resizing.
		m_editNoteRight.EnableWindow(0);

		//Disable the closing 'X'
		CMenu * cMenu = GetSystemMenu(FALSE);
		cMenu->RemoveMenu(SC_CLOSE, MF_BYCOMMAND);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18617");

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//-------------------------------------------------------------------------------------------------
UINT messageBoxProc(LPVOID pData)
{
	auto_ptr<string> apString ((string *) pData);
	AfxMessageBox(apString->c_str(), MB_ICONINFORMATION);
	return 0;
}
//-------------------------------------------------------------------------------------------------
UINT notepadProc(LPVOID pData)
{
	try
	{
		// output the memo text to a temp file in current dir
		auto_ptr<string> apString ((string *) pData);

		string strOutput = apString->c_str();
		if (!strOutput.empty())
		{
			// always overwrite if the file exists
			ofstream ofs(g_tmpTxtFileName.getName().c_str(), ios::out | ios::trunc);
			ofs << strOutput;

			// Close the file and wait for it to be readable
			ofs.close();
			waitForFileToBeReadable(g_tmpTxtFileName.getName());
			
			char pszSystemDir[MAX_PATH];
			::GetSystemDirectory(pszSystemDir, MAX_PATH);
			
			string strCommand(pszSystemDir);
			strCommand += "\\Notepad.exe ";
			strCommand += g_tmpTxtFileName.getName();
			::runEXE(strCommand);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05755")
	
	return 0;
}
//-------------------------------------------------------------------------------------------------
void TestResultLoggerDlg::OnDblclkTree(NMHDR* pNMHDR, LRESULT* pResult) 
{
	try
	{
		HTREEITEM hItem = m_tree.GetSelectedItem();
		
		// if the user double-clicked on an exception string, then display the
		// exception
		map<HTREEITEM, string>::const_iterator iter = m_mapHandleToExceptionData.find(hItem);
		if (iter != m_mapHandleToExceptionData.end())
		{
			UCLIDException ue;
			ue.createFromString("ELI02289", iter->second);

			// Display but do not log the exception [LRCAU #5188]
			ue.display(false);
		}
		
		// if the user double-clicked on a detail note, then bring up the
		// detail note in a messagebox
		iter = m_mapHandleToDetailNote.find(hItem);
		if (iter != m_mapHandleToDetailNote.end())
		{
			// depending upon whether the CTRL key is currently pressed,
			// bring up the detail information in a messagebox right here,
			// or in a different thread
			if (::isVirtKeyCurrentlyPressed(VK_CONTROL))
			{
				AfxBeginThread(messageBoxProc, new string(iter->second));
			}
			else
			{
				MessageBox(iter->second.c_str(), "Information", MB_ICONINFORMATION);
			}
		}
		
		// if the user double-clicked on a memo, then bring up the
		// memo in a notepad
		iter = m_mapHandleToMemo.find(hItem);
		if (iter != m_mapHandleToMemo.end())
		{
			AfxBeginThread(notepadProc, new string(iter->second));
		}

		// If the user double-clicked on a compare node, open the 2 different 
		// values in the diff utility specified by the registry.
		iter = m_mapHandleToCompareData.find(hItem);
		if (iter != m_mapHandleToCompareData.end() )
		{
			// Send the strings to be compared
			openCompareDiff(iter->second);
		}

		// if the user double-clicked on a file, then open the file
		// in its registered application
		iter = m_mapHandleToFile.find(hItem);
		if (iter != m_mapHandleToFile.end())
		{
			// first check to see if the file exists.  If not, 
			// prompt the user if an empty file should be created.
			if (!isValidFile(iter->second))
			{
				if (MessageBox("The specified file does not exist. "
					"Do you want to create it?", "Create file", 
					MB_YESNO) == IDYES)
				{
					ofstream outfile(iter->second.c_str());
					if (!outfile.fail())
					{
						outfile.close();
						waitForFileToBeReadable(iter->second);
					}
				}
			}

			// open the file
			ShellExecute(m_hWnd, "open", iter->second.c_str(), NULL, 
				getDirectoryFromFullPath(iter->second).c_str(), SW_SHOW);
		}

		*pResult = 0;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI05756")
}
//-------------------------------------------------------------------------------------------------
void TestResultLoggerDlg::OnSize(UINT nType, int cx, int cy) 
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
			if (nOldX)
			{		
				////////////////////////////////////////////////
				// expanding or contracting the browser control
				CRect rectTree;
				m_tree.GetWindowRect(&rectTree);	
				ScreenToClient(rectTree);
				rectTree.right += dX;
				rectTree.bottom += dY;					
				m_tree.MoveWindow(rectTree);
				m_tree.InvalidateRect(&rectTree);
				m_tree.Invalidate();
				
				CRect rectNoteLeft;
				m_editNote.GetWindowRect(&rectNoteLeft);
				ScreenToClient(rectNoteLeft);
				int nHeight = rectNoteLeft.Height();
				rectNoteLeft.top = rectTree.bottom + 3;
				//if right note window is disabled
				if(! m_editNoteRight.IsWindowEnabled() )
				{	//then left note takes up the whole window
					rectNoteLeft.right += dX;
				}
				else
				{	//otherwise it shares
					rectNoteLeft.right = (rectTree.left + rectTree.Width() / 2);
				}
				rectNoteLeft.bottom = rectNoteLeft.top + nHeight;
				m_editNote.MoveWindow(rectNoteLeft);

				//set up the right edit box
				CRect rectNoteRight;
				m_editNoteRight.GetWindowRect(&rectNoteRight);
				ScreenToClient(rectNoteRight);
				rectNoteRight.top = rectNoteLeft.top;
				rectNoteRight.left = rectNoteLeft.right;
				rectNoteRight.right = rectTree.right;
				rectNoteRight.bottom = rectNoteLeft.bottom;
				m_editNoteRight.MoveWindow(rectNoteRight);
			}
			nOldX = cx;
			nOldY = cy;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02697")
}
//-------------------------------------------------------------------------------------------------
void TestResultLoggerDlg::OnSelchangedTree(NMHDR* pNMHDR, LRESULT* pResult) 
{
	// the current selection of the item in the tree is changed
	// let's update the contents of the edit box
	try
	{
		bool bCompare = false;
		HTREEITEM hItem = m_tree.GetSelectedItem();

		// clear the contents of the edit box first
		m_zNote.Empty();
		m_zNoteRight.Empty();
		
		// if a detail note is selected
		map<HTREEITEM, string>::const_iterator iter = m_mapHandleToDetailNote.find(hItem);
		if (iter != m_mapHandleToDetailNote.end())
		{
			m_zNote = iter->second.c_str();
		}
		
		// if a memo is selected
		iter = m_mapHandleToMemo.find(hItem);
		if (iter != m_mapHandleToMemo.end())
		{
			m_zNote = iter->second.c_str();
		}

		// if a compare data item is selected
		iter = m_mapHandleToCompareData.find(hItem);
		if (iter != m_mapHandleToCompareData.end())
		{
			bCompare = true;
			CString zSplit = iter->second.c_str();

			//enable the right edit window(IDC_EDIT_NOTE_RIGHT)
			m_editNoteRight.ShowWindow(TRUE);

			//enableWindow is used as a flag for resizing.
			m_editNoteRight.EnableWindow(1);

			//set up a vector of strings
			vector<string> vStrings;
			
			//create a string tokenizer from BaseUtils
			StringTokenizer sToken("^|&$", false);
			
			//Have the tokenizer parse the string on the delimeter
			sToken.parse(iter->second.c_str(), vStrings);

			//Fill the edit boxes.
			m_zNote = vStrings.at(0).c_str();

			// Set the right edit box
			m_zNoteRight = vStrings.at(1).c_str();
		}

		// if a file is selected
		iter = m_mapHandleToFile.find(hItem);
		if (iter != m_mapHandleToFile.end())
		{
			m_zNote = iter->second.c_str();
		}

		//if we need 2 side by side edit boxes (for comparison)
		if(bCompare)
		{
			CRect rectTree;
			m_tree.GetWindowRect(&rectTree);	
			ScreenToClient(rectTree);

			//resize the left edit window(IDC_EDIT_CURRENT_NOTE)
			CRect rectNoteLeft;
			m_editNote.GetWindowRect(&rectNoteLeft);
			ScreenToClient(rectNoteLeft);
			rectNoteLeft.right = (rectTree.Width() / 2);
			m_editNote.MoveWindow(rectNoteLeft);
		
			//resize the right edit window
			CRect rectNoteRight;
			m_editNoteRight.GetWindowRect(&rectNoteRight);
			ScreenToClient(rectNoteRight);
			rectNoteRight.left = rectNoteLeft.right;
			rectNoteLeft.right = rectTree.Width();
			m_editNoteRight.MoveWindow(rectNoteRight);
		}
		else
		{	//otherwise, verify that only the left editbox is shown and sized
			CRect rectTree;
			m_tree.GetWindowRect(&rectTree);	
			ScreenToClient(rectTree);
			m_tree.MoveWindow(rectTree);
			m_tree.InvalidateRect(&rectTree);
			m_tree.Invalidate();
			
			CRect rectNoteLeft;
			m_editNote.GetWindowRect(&rectNoteLeft);
			ScreenToClient(rectNoteLeft);
			rectNoteLeft.top = rectTree.bottom + 3;
			rectNoteLeft.right = rectTree.right;
			rectNoteLeft.left = rectTree.left;
			m_editNote.MoveWindow(rectNoteLeft);
			m_editNoteRight.ShowWindow(FALSE);

			//enableWindow is used as a flag for resizing.
			m_editNoteRight.EnableWindow(0);
		}

		UpdateData(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06342")
	
	*pResult = 0;
}
//-------------------------------------------------------------------------------------------------
void TestResultLoggerDlg::OnClose() 
{
	try
	{
		// before close the window, let's destroy all windows
		// so that there's going to be no memory leak
		m_tree.SetImageList(NULL, TVSIL_NORMAL);
		m_tree.DestroyWindow();
		m_editNote.DestroyWindow();
		
		DestroyWindow();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10137")

	CDialog::OnClose();
}

//-------------------------------------------------------------------------------------------------
// Private Functions
//-------------------------------------------------------------------------------------------------
bool TestResultLoggerDlg::autoScrollEnabled() const
{
	// check the flag to scroll logger window in the registry
	if (!m_apCfgMgr->keyExists("\\", gstrSCROLL_LOGGER_KEY))
	{
		m_apCfgMgr->createKey("\\", gstrSCROLL_LOGGER_KEY, "0");
		return false;
	}

	// if we reached here, we should use default behavior (no scrolling)
	return m_apCfgMgr->getKeyValue("\\", gstrSCROLL_LOGGER_KEY) == "1";
}
//-------------------------------------------------------------------------------------------------
bool TestResultLoggerDlg::retainTestCaseNodes() const
{
	if (!m_apCfgMgr->keyExists("\\", gstrRETAIN_NODES_KEY))
	{
		// by default, always keep the nodes
		m_apCfgMgr->createKey("\\", gstrRETAIN_NODES_KEY, "1");
		return true;
	}

	return m_apCfgMgr->getKeyValue("\\", gstrRETAIN_NODES_KEY) == "1";
}
//-------------------------------------------------------------------------------------------------
string TestResultLoggerDlg::getDiffString()
{
	if( !m_apCfgMgr->keyExists("\\", gstrDIFF_COMMAND_LINE_KEY) )
	{
		// Set the default string if one doesnt exist
		m_apCfgMgr->createKey("\\", gstrDIFF_COMMAND_LINE_KEY, gstrDEFAULT_DIFF_CMD);

		// Then return the default string
		return gstrDEFAULT_DIFF_CMD;
	}

	// If a string already exists, return it
	return m_apCfgMgr->getKeyValue("\\", gstrDIFF_COMMAND_LINE_KEY);
}
//-------------------------------------------------------------------------------------------------
void TestResultLoggerDlg::startTestHarness(const string& strHarnessDescription)
{
	// if a test harness is active, end it
	if (m_bTestHarnessActive)
	{
		endTestHarness();
	}

	// create a new test harness entry
	m_bTestHarnessActive = true;
	m_hCurrentHarness = m_tree.InsertItem(strHarnessDescription.c_str());
	m_tree.SetItemImage(m_hCurrentHarness, m_iBitmapHarness, m_iBitmapHarness);
}
//-------------------------------------------------------------------------------------------------
void TestResultLoggerDlg::startComponentTest(const string& strComponentDescription)
{	
	// if a component test is active, end it
	if (m_bComponentTestActive)
	{
		endComponentTest();
	}

	// ensure that a test harness is active.
	if (!m_bTestHarnessActive)
	{
		throw UCLIDException("ELI02278", "Cannot start a component test without starting the test harness!");
	}

	// create a new component test entry
	m_bComponentTestActive = true;
	
	HTREEITEM hFirstItem = m_tree.GetFirstVisibleItem();
	m_tree.SetRedraw(FALSE);
	
	m_hCurrentComponent = m_tree.InsertItem(strComponentDescription.c_str(), m_hCurrentHarness);
	m_tree.SetItemImage(m_hCurrentComponent, m_iBitmapComponent, m_iBitmapComponent);
	m_tree.Expand(m_hCurrentHarness, TVE_EXPAND);
	
	if (!autoScrollEnabled())
	{
		m_tree.SelectSetFirstVisible(hFirstItem);
	}
	m_tree.SetRedraw(TRUE);
}
//-------------------------------------------------------------------------------------------------
void TestResultLoggerDlg::startTestCase(const string& strTestCaseID, 
										const string& strTestCaseDescription,
										ETestCaseType eTestCaseType)
{
	// if a test case is active, end it
	if (m_bTestCaseActive)
	{
		UCLIDException ue("ELI02293", "Test case did not complete - another test case started before this test case ended!");
		addTestCaseException(ue.asStringizedByteStream());
		endTestCase(false);
	}

	// ensure that a component test is active
	if (!m_bComponentTestActive)
	{
		throw UCLIDException("ELI02279", "Cannot start a test case without starting a component test!");
	}

	// create the string to represent the test case
	string strItemText = strTestCaseDescription;
	strItemText += " (";
	strItemText += strTestCaseID;
	strItemText += ")";
	
	// ensure valid test case type and remember the current test case type
	if (eTestCaseType == kInvalidTestCaseType)
	{
		UCLIDException ue("ELI07372", "Invalid test case type!");
		ue.addDebugInfo("eTestCaseType", (unsigned long) eTestCaseType);
		throw ue;
	}
	
	m_eCurrentTestCaseType = eTestCaseType;
	
	// create the new test case entry
	m_bTestCaseActive = true;
		
	// whether or not to put the test case node in the logger
	if (retainTestCaseNodes() || m_eCurrentTestCaseType == kSummaryTestCase)
	{	
		HTREEITEM hFirstItem = m_tree.GetFirstVisibleItem();
		m_tree.SetRedraw(FALSE);
		
		m_hCurrentTestCase = m_tree.InsertItem(strItemText.c_str(), m_hCurrentComponent);
		m_tree.SetItemImage(m_hCurrentTestCase, m_iBitmapTestCaseInProgress, 
			m_iBitmapTestCaseInProgress);
		m_tree.Expand(m_hCurrentComponent, TVE_EXPAND);
		
		if (!autoScrollEnabled())
		{
			m_tree.SelectSetFirstVisible(hFirstItem);
		}
		m_tree.SetRedraw(TRUE);
	}
}
//-------------------------------------------------------------------------------------------------
void TestResultLoggerDlg::addTestCaseNote(const string& strTestCaseNote)
{
	// ensure that a test case is active
	if (!m_bTestCaseActive)
	{
		throw UCLIDException("ELI02285", "Cannot add a test case note without an active test case!");
	}

	// whether or not to put the test case node in the logger
	if (retainTestCaseNodes() || m_eCurrentTestCaseType == kSummaryTestCase)
	{	
		HTREEITEM hFirstItem = m_tree.GetFirstVisibleItem();
		m_tree.SetRedraw(FALSE);
		
		// create the new test case note entry
		HTREEITEM hNote = m_tree.InsertItem(strTestCaseNote.c_str(), m_hCurrentTestCase);
		m_tree.SetItemImage(hNote, m_iBitmapNote, m_iBitmapNote);
		m_tree.Expand(m_hCurrentTestCase, TVE_EXPAND);
		
		if (!autoScrollEnabled())
		{
			m_tree.SelectSetFirstVisible(hFirstItem);
		}
		m_tree.SetRedraw(TRUE);
	}
}
//-------------------------------------------------------------------------------------------------
void TestResultLoggerDlg::addTestCaseDetailNote(const string& strTitle,
												const string& strTestCaseDetailNote)
{
	// ensure that a test case is active
	if (!m_bTestCaseActive)
	{
		throw UCLIDException("ELI05586", "Cannot add a test case detail note without an active test case!");
	}
	
	// whether or not to put the test case node in the logger
	if (retainTestCaseNodes() || m_eCurrentTestCaseType == kSummaryTestCase)
	{	
		HTREEITEM hFirstItem = m_tree.GetFirstVisibleItem();
		m_tree.SetRedraw(FALSE);
		
		// create the new test case note entry
		HTREEITEM hDetailNote = m_tree.InsertItem(strTitle.c_str(), m_hCurrentTestCase);
		m_tree.SetItemImage(hDetailNote, m_iBitmapDetailNote, m_iBitmapDetailNote);
		m_tree.Expand(m_hCurrentTestCase, TVE_EXPAND);
		
		// add the detail note to the map
		m_mapHandleToDetailNote[hDetailNote] = strTestCaseDetailNote;
		
		if (!autoScrollEnabled())
		{
			m_tree.SelectSetFirstVisible(hFirstItem);
		}
		m_tree.SetRedraw(TRUE);
	}
}
//-------------------------------------------------------------------------------------------------
void TestResultLoggerDlg::addTestCaseMemo(const string& strTitle,
										  const string& strTestCaseMemo)
{
	// ensure that a test case is active
	if (!m_bTestCaseActive)
	{
		throw UCLIDException("ELI05753", "Cannot add a test case memo without an active test case!");
	}

	// whether or not to put the test case node in the logger
	if (retainTestCaseNodes() || m_eCurrentTestCaseType == kSummaryTestCase)
	{	
		HTREEITEM hFirstItem = m_tree.GetFirstVisibleItem();
		m_tree.SetRedraw(FALSE);
		
		// create the new test case note entry
		HTREEITEM hMemo = m_tree.InsertItem(strTitle.c_str(), m_hCurrentTestCase);
		m_tree.SetItemImage(hMemo, m_iBitmapMemo, m_iBitmapMemo);
		m_tree.Expand(m_hCurrentTestCase, TVE_EXPAND);
		
		// add the detail note to the map
		m_mapHandleToMemo[hMemo] = strTestCaseMemo;
		
		if (!autoScrollEnabled())
		{
			m_tree.SelectSetFirstVisible(hFirstItem);
		}
		m_tree.SetRedraw(TRUE);
	}
}
//-------------------------------------------------------------------------------------------------
void TestResultLoggerDlg::addTestCaseFile(const string& strFileName)
{
	// ensure that a test case is active
	if (!m_bTestCaseActive)
	{
		throw UCLIDException("ELI06353", "Cannot add a test case file without an active test case!");
	}

	// whether or not to put the test case node in the logger
	if (retainTestCaseNodes() || m_eCurrentTestCaseType == kSummaryTestCase)
	{	
		string strFileExt = ::getExtensionFromFullPath(strFileName, true);
		if ((strFileExt == ".nte") && isValidFile(strFileName))
		{
			m_bCollapseOnFail = true;
		}
		HTREEITEM hFirstItem = m_tree.GetFirstVisibleItem();
		m_tree.SetRedraw(FALSE);
		
		// create the new test case note entry
		HTREEITEM hFile = m_tree.InsertItem(strFileName.c_str(), m_hCurrentTestCase);
		m_tree.SetItemImage(hFile, m_iBitmapFile, m_iBitmapFile);
		m_tree.Expand(m_hCurrentTestCase, TVE_EXPAND);
		
		// add the detail note to the map
		m_mapHandleToFile[hFile] = strFileName;
		
		if (!autoScrollEnabled())
		{
			m_tree.SelectSetFirstVisible(hFirstItem);
		}
		m_tree.SetRedraw(TRUE);
	}
}
//-------------------------------------------------------------------------------------------------
void TestResultLoggerDlg::addTestCaseException(const string& strTestCaseException)
{
	// ensure that a test case is active
	if (!m_bTestCaseActive)
	{
		throw UCLIDException("ELI02287", "Cannot add a test case exception without an active test case!");
	}

	// whether or not to put the test case node in the logger
	if (retainTestCaseNodes() || m_eCurrentTestCaseType == kSummaryTestCase)
	{	
		// generate a display string for the exception
		UCLIDException ue;
		ue.createFromString("ELI02288", strTestCaseException);
		string strText = ue.getTopText();
		strText += " (";
		strText += ue.getTopELI();
		strText += ")";
		
		HTREEITEM hFirstItem = m_tree.GetFirstVisibleItem();
		m_tree.SetRedraw(FALSE);
		
		// create the new test case exception entry
		HTREEITEM hException = m_tree.InsertItem(strText.c_str(), m_hCurrentTestCase);
		m_tree.SetItemImage(hException, m_iBitmapException, m_iBitmapException);
		m_tree.Expand(m_hCurrentTestCase, TVE_EXPAND);
		
		// add the exception data to the map
		m_mapHandleToExceptionData[hException] = strTestCaseException;
		
		if (!autoScrollEnabled())
		{
			m_tree.SelectSetFirstVisible(hFirstItem);
		}
		m_tree.SetRedraw(TRUE);
	}
}
//-------------------------------------------------------------------------------------------------
void TestResultLoggerDlg::endTestCase(bool bResult)
{
	m_bTestCaseActive = false;
	
	// whether or not to put the test case node in the logger
	if (retainTestCaseNodes() || m_eCurrentTestCaseType == kSummaryTestCase)
	{	
		// if the test case is of invalid type (used for summary, etc), use 
		// a different bitmap than for regular test cases that either failed or
		// passed
		int iBitmapToUse;
		if (m_eCurrentTestCaseType == kAutomatedTestCase || 
			m_eCurrentTestCaseType == kInteractiveTestCase)
		{
			iBitmapToUse = bResult ? m_iBitmapTestCaseDoneGood : m_iBitmapTestCaseDoneBad;
		}
		else if (m_eCurrentTestCaseType == kSummaryTestCase)
		{
			iBitmapToUse = bResult ? m_iBitmapTestCaseDoneOtherGood :
									 m_iBitmapTestCaseDoneOtherBad;
		}
		
		HTREEITEM hFirstItem = m_tree.GetFirstVisibleItem();
		m_tree.SetRedraw(FALSE);
		
		m_tree.SetItemImage(m_hCurrentTestCase, iBitmapToUse, iBitmapToUse);
		
		// if the test case was successful, then collapse the results
		if (bResult || m_bCollapseOnFail)
		{
			m_tree.Expand(m_hCurrentTestCase, TVE_COLLAPSE);
		}
		// reset for next case
		m_bCollapseOnFail = false;
		
		if (!autoScrollEnabled())
		{
			m_tree.SelectSetFirstVisible(hFirstItem);
		}
		m_tree.SetRedraw(TRUE);
	}
}
//-------------------------------------------------------------------------------------------------
void TestResultLoggerDlg::endComponentTest(bool bShowComponentLevelSummary)
{
	// end the current test case if one is active
	if (m_bTestCaseActive)
	{
		UCLIDException ue("ELI02292", "Test case did not complete - component test ended before test case ended!");
		addTestCaseException(ue.asStringizedByteStream());
		endTestCase(false);
	}

	m_bComponentTestActive = false;
}
//-------------------------------------------------------------------------------------------------
void TestResultLoggerDlg::endTestHarness()
{
	// end the current component test if one is active
	if (m_bComponentTestActive)
	{
		endComponentTest();
	}

	m_bTestHarnessActive = false;
}
//-------------------------------------------------------------------------------------------------
void TestResultLoggerDlg::OnGetMinMaxInfo(MINMAXINFO* lpMMI)
{
	try
	{
		// Minimum width/height
		lpMMI->ptMinTrackSize.x = 150;
		lpMMI->ptMinTrackSize.y = 150;
	}
	CATCH_UCLID_EXCEPTION("ELI02695")
	CATCH_UNEXPECTED_EXCEPTION("ELI02696")
}
//-------------------------------------------------------------------------------------------------
void TestResultLoggerDlg::addTestCaseCompareData(const string& strTitle,
												 const string& strLabel1, const string& strInput1,
											     const string& strLabel2, const string& strInput2)
{
	//ensure that a test case is active
	if( !m_bTestCaseActive)
	{
		throw UCLIDException("ELI13513", "Cannot add a compare data case without an active test!");
	}
	
	// whether or not to put the test case node in the logger
	if (retainTestCaseNodes() || m_eCurrentTestCaseType == kSummaryTestCase)
	{	
		HTREEITEM hFirstItem = m_tree.GetFirstVisibleItem();
		m_tree.SetRedraw(FALSE);		
		
		// create the new test case note entry
		HTREEITEM hCaseCompareData = m_tree.InsertItem(strTitle.c_str(), m_hCurrentTestCase);
		m_tree.SetItemImage(hCaseCompareData, m_iBitmapMemo, m_iBitmapMemo);
		m_tree.Expand(m_hCurrentTestCase, TVE_EXPAND);
				
		// add the detail note to the map
		m_mapHandleToCompareData[hCaseCompareData] = strLabel1 + ":\r\n" + strInput1 + gstrDELIMITER + 
													 strLabel2 + ":\r\n" + strInput2;
		
		if (!autoScrollEnabled())
		{
			m_tree.SelectSetFirstVisible(hFirstItem);
		}
		m_tree.SetRedraw(TRUE);
	}
}
//-------------------------------------------------------------------------------------------------
void TestResultLoggerDlg::openCompareDiff(const string& strSource)
{
	try
	{
		// Set up the tokenizer
		vector<string> vecStrings;
		StringTokenizer tokenizer(gstrDELIMITER);

		// Split the string at the delimiter
		tokenizer.parse( strSource, vecStrings);

		// Make strings to use to create the path for each temp file
		string strFileAPath = "";
		string strFileBPath = "";

		// Create File A and fill it
		if (!vecStrings[0].empty())
		{
			// always overwrite if the file exists
			ofstream ofs(g_tmpTxtFileName.getName().c_str(), ios::out | ios::trunc);
			ofs << vecStrings[0];
			ofs.close();

			// Store the path for the temp file
			strFileAPath += g_tmpTxtFileName.getName();
		}

		// Create File B and fill it
		if (!vecStrings[1].empty())
		{
			// always overwrite if the file exists
			ofstream ofs(g_tmpTxtFileNameB.getName().c_str(), ios::out | ios::trunc);
			ofs << vecStrings[1];
			ofs.close();
			
			// Store the path for the temp file			
			strFileBPath += g_tmpTxtFileNameB.getName();
		}

		// Get the command line from the registry
		string strToExecuteDiff = getDiffString();

		// Replace the %1 and %2 with the file paths
		if( !(replaceVariable(strToExecuteDiff, "%1", strFileAPath)) )
		{
			// If there is a problem with replacing %1, notify the user
			UCLIDException ue( "ELI14466", "Unable to replace %1 in diff command line!");
			ue.addDebugInfo( "Diff String:", strToExecuteDiff);
			ue.display();
		}
			
		if( !(replaceVariable(strToExecuteDiff, "%2", strFileBPath)) )
		{
			// If there is a problem with replacing %2, notify the user
			UCLIDException ue( "ELI14484", "Unable to replace %2 in diff command line!");
			ue.addDebugInfo( "Diff String:", strToExecuteDiff);
			ue.display();
		}

		// Ensure both files (if they exist) are readable before running the Diff utility
		if (strFileAPath != "")
		{
			waitForFileToBeReadable(strFileAPath);
		}
		if (strFileBPath != "")
		{
			waitForFileToBeReadable(strFileBPath);
		}

		// Run the command line to display the differences
		::runEXE(strToExecuteDiff);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI14465");
}
//-------------------------------------------------------------------------------------------------