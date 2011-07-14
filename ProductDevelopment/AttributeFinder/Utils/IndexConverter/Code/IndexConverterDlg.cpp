// IndexConverterDlg.cpp : implementation file
//

#include "stdafx.h"
#include "IndexConverter.h"
#include "IndexConverterDlg.h"

#include <cpputil.h>
#include <UCLIDException.h>
#include <XBrowseForFolder.h>
#include <StringTokenizer.h>
#include <Prompt2Dlg.h>
#include <TemporaryResourceOverride.h>
#include <ProgressDlgTaskRunner.h>
#include <LoadFileDlgThread.h>

#include <io.h>

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

const static CString PROCESS_ONE = "Process &One";
const static CString PROCESS_NEXT = "Process &Next";
const static CString PROCESS_ALL = "&Process All";
const static CString PROCESS_REMAINING = "Process &Remaining";
const static CString STR_EQUAL = "=";
const static CString STR_NOT_EQUAL = "!=";

//-------------------------------------------------------------------------------------------------
// CIndexConverterDlg dialog
//-------------------------------------------------------------------------------------------------
CIndexConverterDlg::CIndexConverterDlg(CWnd* pParent /*=NULL*/)
: CDialog(CIndexConverterDlg::IDD, pParent),
  m_bProcessOneStarted(false)
{
	//{{AFX_DATA_INIT(CIndexConverterDlg)
	m_zIndexFile = _T("");
	m_zEAVFolder = _T("");
	m_zFromFolder = _T("");
	m_zToFolder = _T("");
	m_bConsolidate = FALSE;
	m_bMoveFiles = FALSE;
	m_zDelimiter = _T("");
	m_zName = _T("");
	m_zTypeFormat = _T("");
	m_zValueFormat = _T("");
	m_bMoveFilesOnly = FALSE;
	m_zConditionLHS = _T("");
	m_zConditionRHS = _T("");
	m_bWriteConditionEnabled = FALSE;
	m_zBeginString = _T("");
	m_zEndString = _T("");
	//}}AFX_DATA_INIT
	// Note that LoadIcon does not require a subsequent DestroyIcon in Win32
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);

	m_mapTypeTranslations.clear();
}
//-------------------------------------------------------------------------------------------------
void CIndexConverterDlg::runTask(IProgress* pProgress, int nTaskID)
{
	string strLine("");

	LONGLONG nFileSize = getSizeOfFile((LPCSTR)m_zIndexFile);
	LONGLONG nCurrFilePos = 0;

	m_bDoneProcessing = false;
	
	while (!m_ifs.eof())
	{	

		if (pProgress && pProgress->userCanceled())
		{
			return;
		}

		getline(m_ifs, strLine);

		nCurrFilePos = m_ifs.tellg();
		
		double percentComplete = (double)nCurrFilePos/(double)nFileSize * 100.0;
		
		if (pProgress)
		{
			pProgress->setPercentComplete(percentComplete);
		}

		if (strLine.empty())
		{
			continue;
		}

		// parse the line into tokens, which are separated by delimiter defined
		vector<string> vecTokens;
		StringTokenizer::sGetTokens(strLine, (LPCTSTR)m_zDelimiter, (LPCTSTR)m_zBeginString, (LPCTSTR)m_zEndString, vecTokens);

		// interprete EAV file name, move file name, attribute name, 
		// attribute value, and type name
		string strEAVFileName(""), strMoveFiles(""), strToFolder(""),
			   strAttrName(""), strAttrValue(""), strType(""), strAttribute("");

		// get eav files name if this utility is not used to move files only
		bool bWriteAttribute = isWriteConditionMet(vecTokens);
		if (!m_bMoveFilesOnly && bWriteAttribute)
		{
			strEAVFileName = ::combine((LPCTSTR)m_zEAVFolder, vecTokens);
			// make sure the eav file has .eav extension
			string strExtension = ::getExtensionFromFullPath(strEAVFileName);
			if (strExtension.empty() || _strcmpi(strExtension.c_str(), ".eav") != 0)
			{
				strEAVFileName += ".eav";
			}

			if (pProgress)
			{
				pProgress->setText(strEAVFileName.c_str());
			}

			strAttrName = ::combine((LPCTSTR)m_zName, vecTokens);
			strAttrValue = ::combine((LPCTSTR)m_zValueFormat, vecTokens);
			if (m_bConsolidate)
			{
				// remove leading/trailing white space and consolidate white spaces
				strAttrValue = ::trim(strAttrValue, " \t\r\n", " \t\r\n");

				//strAttrValue = ::consolidateMultipleCharsIntoOne(strAttrValue, " ", true);
				//strAttrValue = ::consolidateMultipleCharsIntoOne(strAttrValue, "\t", true);
				//strAttrValue = ::consolidateMultipleCharsIntoOne(strAttrValue, "\r\n", true);
				strAttrValue = ::replaceMultipleCharsWithOne(strAttrValue, "\r\n\t "," ", true);
			}
			
			if (!m_zTypeFormat.IsEmpty())
			{
				// translate type if any
				strType = ::combine((LPCTSTR)m_zTypeFormat, vecTokens);
				strType = translateType(strType);
			}
			
			// Prepare the attribute
			strAttribute = strAttrName + "|" + strAttrValue;
			if (!strType.empty())
			{
				strAttribute += "|" + strType;
			}
			// output to the eav file
			{
				ofstream ofs(strEAVFileName.c_str(), ios::out | ios::app);
				ofs << strAttribute << endl;
				ofs.close();
				waitForFileToBeReadable(strEAVFileName);
			}
		}

		// move files if m_bMoveFiles is true
		bool bMoveSuccess = false;
		if (m_bMoveFiles)
		{
			strMoveFiles = ::combine((LPCTSTR)m_zFromFolder, vecTokens);
			strToFolder = ::combine((LPCTSTR)m_zToFolder, vecTokens);
		
			string strFilesToBeMoved = ::getFileNameFromFullPath(strMoveFiles);
			string strFromFolder = ::getDirectoryFromFullPath(strMoveFiles) + "\\";
			bMoveSuccess = moveFiles(strFilesToBeMoved, strFromFolder, strToFolder);
		}

		if (m_bProcessOneStarted)
		{
			// if user chose process one at a time,
			// display result after conversion
			string strMsg("");
			if (!strAttribute.empty() && bWriteAttribute)
			{
				strMsg = "Attribute : \r\n" + strAttribute + "\r\n";
			}
			else if (!bWriteAttribute)
			{
				strMsg = "Write condition not met.\r\n";
			}

			if (m_bMoveFiles)
			{
				if (bMoveSuccess)
				{
					strMsg += "File(s) moved from : \r\n" + strMoveFiles + "\r\n";
					strMsg += "To : \r\n" + strToFolder;
				}
				else
				{
					strMsg += "No more " + strMoveFiles + " file found";
				}
			}

			MessageBox(strMsg.c_str(), "Result by processing one record.");

			if (m_ifs.eof())
			{
				// if the end of file is reached, jump out of the while loop
				break;
			}
			// then return
			return;
		}
	}

	if (m_bMoveFiles && m_zIndexFile.IsEmpty())
	{
		string strMoveFiles = (LPCTSTR)m_zFromFolder;
		string strToFolder = (LPCTSTR)m_zToFolder;
		
		string strFilesToBeMoved = ::getFileNameFromFullPath(strMoveFiles);
		string strFromFolder = ::getDirectoryFromFullPath(strMoveFiles) + "\\";
		moveFiles(strFilesToBeMoved, strFromFolder, strToFolder);
	}
	m_bDoneProcessing = true;
}
//-------------------------------------------------------------------------------------------------
void CIndexConverterDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CIndexConverterDlg)
	DDX_Control(pDX, IDC_EDIT_BEGIN_STRING_QUALIFIER, m_editBeginString);
	DDX_Control(pDX, IDC_EDIT_END_STRING_QUALIFIER, m_editEndString);
	DDX_Control(pDX, IDC_COMBO_CONDITION_OP, m_comboConditionOp);
	DDX_Control(pDX, IDC_BUTTON_RESET, m_btnReset);
	DDX_Control(pDX, IDC_EDIT_DELIMITER, m_editDelimiter);
	DDX_Control(pDX, IDC_CHECK_MOVE, m_chkMoveFiles);
	DDX_Control(pDX, IDC_BUTTON_ADD, m_btnAdd);
	DDX_Control(pDX, IDC_CHECK_CONSOLIDATE, m_chkConsolidate);
	DDX_Control(pDX, IDC_BUTTON_EAV, m_btnBrowseEAV);
	DDX_Control(pDX, IDC_EDIT_VALUE, m_editAttrValue);
	DDX_Control(pDX, IDC_EDIT_TYPE, m_editAttrType);
	DDX_Control(pDX, IDC_EDIT_NAME, m_editAttrName);
	DDX_Control(pDX, IDC_BUTTON_DELETE, m_btnDelete);
	DDX_Control(pDX, IDC_BUTTON_MODIFY, m_btnModify);
	DDX_Control(pDX, IDC_EDIT_EAV, m_editEAVFile);
	DDX_Control(pDX, IDC_EDIT_INDEX, m_editIndexFile);
	DDX_Control(pDX, IDC_BUTTON_TO, m_btnTo);
	DDX_Control(pDX, IDC_BUTTON_FROM, m_btnFrom);
	DDX_Control(pDX, IDC_LIST_TRANSLATE, m_list);
	DDX_Control(pDX, IDC_BUTTON_CONTINUE, m_btnContinue);
	DDX_Control(pDX, IDC_BUTTON_PROCESS, m_btnProcess);
	DDX_Text(pDX, IDC_EDIT_INDEX, m_zIndexFile);
	DDX_Text(pDX, IDC_EDIT_EAV, m_zEAVFolder);
	DDX_Text(pDX, IDC_EDIT_FROM, m_zFromFolder);
	DDX_Text(pDX, IDC_EDIT_TO, m_zToFolder);
	DDX_Check(pDX, IDC_CHECK_CONSOLIDATE, m_bConsolidate);
	DDX_Check(pDX, IDC_CHECK_MOVE, m_bMoveFiles);
	DDX_Text(pDX, IDC_EDIT_DELIMITER, m_zDelimiter);
	DDX_Text(pDX, IDC_EDIT_NAME, m_zName);
	DDX_Text(pDX, IDC_EDIT_TYPE, m_zTypeFormat);
	DDX_Text(pDX, IDC_EDIT_VALUE, m_zValueFormat);
	DDX_Check(pDX, IDC_CHECK_ONLY_MOVE, m_bMoveFilesOnly);
	DDX_Text(pDX, IDC_EDIT_CONDITION_LHS, m_zConditionLHS);
	DDX_Text(pDX, IDC_EDIT_CONDITION_RHS, m_zConditionRHS);
	DDX_Check(pDX, IDC_CHECK_WRITE_CONDITION, m_bWriteConditionEnabled);
	DDX_Text(pDX, IDC_EDIT_BEGIN_STRING_QUALIFIER, m_zBeginString);
	DDX_Text(pDX, IDC_EDIT_END_STRING_QUALIFIER, m_zEndString);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CIndexConverterDlg, CDialog)
	//{{AFX_MSG_MAP(CIndexConverterDlg)
	ON_WM_CLOSE()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_BN_CLICKED(IDC_BUTTON_INDEX, OnButtonIndex)
	ON_BN_CLICKED(IDC_INFO_EAV, OnInfoEAV)
	ON_BN_CLICKED(IDC_INFO_FROM_FILE, OnInfoFromFile)
	ON_BN_CLICKED(IDC_INFO_TO_FOLDER, OnInfoToFolder)
	ON_BN_CLICKED(IDC_INFO_ATTR_VALUE, OnInfoAttrValue)
	ON_BN_CLICKED(IDC_INFO_ATTR_TYPE, OnInfoAttrType)
	ON_BN_CLICKED(IDC_BUTTON_EAV, OnButtonEav)
	ON_BN_CLICKED(IDC_BUTTON_FROM, OnButtonFrom)
	ON_BN_CLICKED(IDC_BUTTON_TO, OnButtonTo)
	ON_BN_CLICKED(IDC_CHECK_MOVE, OnCheckMove)
	ON_BN_CLICKED(IDC_BUTTON_PROCESS, OnButtonProcess)
	ON_BN_CLICKED(IDC_BUTTON_CONTINUE, OnButtonContinue)
	ON_EN_CHANGE(IDC_EDIT_DELIMITER, OnChangeEditDelimiter)
	ON_EN_CHANGE(IDC_EDIT_INDEX, OnChangeEditIndex)
	ON_EN_CHANGE(IDC_EDIT_EAV, OnChangeEditEav)
	ON_EN_CHANGE(IDC_EDIT_FROM, OnChangeEditFrom)
	ON_EN_CHANGE(IDC_EDIT_NAME, OnChangeEditName)
	ON_EN_CHANGE(IDC_EDIT_TO, OnChangeEditTo)
	ON_EN_CHANGE(IDC_EDIT_VALUE, OnChangeEditValue)
	ON_BN_CLICKED(IDC_BUTTON_CLOSE, OnButtonClose)
	ON_BN_CLICKED(IDC_BUTTON_ADD, OnButtonAdd)
	ON_BN_CLICKED(IDC_BUTTON_DELETE, OnButtonDelete)
	ON_BN_CLICKED(IDC_BUTTON_MODIFY, OnButtonModify)
	ON_NOTIFY(NM_DBLCLK, IDC_LIST_TRANSLATE, OnDblclkListTranslate)
	ON_NOTIFY(LVN_ITEMCHANGED, IDC_LIST_TRANSLATE, OnItemchangedListTranslate)
	ON_BN_CLICKED(IDC_CHECK_ONLY_MOVE, OnCheckOnlyMove)
	ON_BN_CLICKED(IDC_BUTTON_RESET, OnButtonReset)
	ON_BN_CLICKED(IDC_CHECK_WRITE_CONDITION, OnCheckWriteCondition)
	ON_BN_CLICKED(IDC_INFO_CONDITION, OnInfoCondition)
	ON_EN_CHANGE(IDC_EDIT_BEGIN_STRING_QUALIFIER, OnChangeEditBeginStringQualifier)
	ON_EN_CHANGE(IDC_EDIT_END_STRING_QUALIFIER, OnChangeEditEndStringQualifier)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CIndexConverterDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CIndexConverterDlg::OnInitDialog()
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		CDialog::OnInitDialog();

		// Set the icon for this dialog.  The framework does this automatically
		//  when the application's main window is not a dialog
		SetIcon(m_hIcon, TRUE);			// Set big icon
		SetIcon(m_hIcon, FALSE);		// Set small icon
		
		// Create the tooltip object with no delay
		m_infoTip.Create(this);
		m_infoTip.SetShowDelay(0);

		// Create the column labels for the list
		setColumnLabels();

		// Add full row selection and grid lines
		m_list.SetExtendedStyle( LVS_EX_FULLROWSELECT | LVS_EX_GRIDLINES );

		// add the operators to the condition combo box
		m_comboConditionOp.AddString(STR_EQUAL);
		m_comboConditionOp.AddString(STR_NOT_EQUAL);
		m_comboConditionOp.SetCurSel(0);

		// Set default enabled/disabled state
		updateControls();
		OnCheckWriteCondition();

		m_btnModify.EnableWindow(FALSE);
		m_btnDelete.EnableWindow(FALSE);
		m_btnReset.EnableWindow(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18602");

	return TRUE;  // return TRUE  unless you set the focus to a control
}
//-------------------------------------------------------------------------------------------------
// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.
void CIndexConverterDlg::OnPaint() 
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
HCURSOR CIndexConverterDlg::OnQueryDragIcon()
{
	return (HCURSOR) m_hIcon;
}
//-------------------------------------------------------------------------------------------------
void CIndexConverterDlg::OnButtonIndex() 
{
	try
	{
		// Ask user to select the index file
		CFileDialog fileDlg( TRUE, ".txt", NULL, 
			OFN_ENABLESIZING | OFN_EXPLORER | OFN_FILEMUSTEXIST | OFN_HIDEREADONLY | OFN_PATHMUSTEXIST,
			"Text files (*.txt)|*.txt|All Files (*.*)|*.*||", this);
		
		// Pass the pointer of dialog to create ThreadDataStruct object
		ThreadFileDlg tfd(&fileDlg);

		// If Cancel button is clicked
		if (tfd.doModal() != IDOK)
		{
			return;
		}

		// Save the filename and update the edit box
		m_zIndexFile = fileDlg.GetPathName();
		// update Process buttons
		indexFileChanging(m_zIndexFile);

		UpdateData(FALSE);
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06920");
}
//-------------------------------------------------------------------------------------------------
void CIndexConverterDlg::OnInfoEAV() 
{
	try
	{
		// Show tooltip
		CString zText("%Number can be used to hold place for a specific token.  Use \n"
					  "%1 to indicate the first token and %2 for the second, etc.\n"
					  "For example, if \"12|34|56|78\" is the input string, pipe (|)\n"
					  "as the delimiter, \"%1\" will be \"12\", \"%3\" will be \"56\".\n\n"
					  "No file/folder name shall contain any of the following characters:\n"
					  "back slash (\\), forward slash (/), colon (:), star (*)\n"
					  "question mark (?), quotation mark (\"), angle brackets (<>),\n"
					  "or pipe character (|)");

		// ensure that the info tip window exists before showing
		if (asCppBool(::IsWindow(m_infoTip.m_hWnd)))
		{
			m_infoTip.Show( zText );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06921");
}
//-------------------------------------------------------------------------------------------------
void CIndexConverterDlg::OnInfoFromFile() 
{
	try
	{
		// Show tooltip
		CString zText("Wildcard characters such as star (*) and question mark (?)\n"
					  "can be used to indicate a set of files.\n\n"
			          "%Number can be used to hold place for a specific token.  Use \n"
					  "%1 to indicate the first token and %2 for the second, etc.\n"
					  "For example, if \"12|34|56|78\" is the input string, pipe (|)\n"
					  "as the delimiter, \"%1\" will be \"12\", \"%3\" will be \"56\",\n"
					  "\"D:\\temp\\%1.*\" will be interpreted as \"D:\\temp\\12.*\".");

		// ensure that the info tip window exists before showing
		if (asCppBool(::IsWindow(m_infoTip.m_hWnd)))
		{
			m_infoTip.Show( zText );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06962");
}
//-------------------------------------------------------------------------------------------------
void CIndexConverterDlg::OnInfoToFolder() 
{
	try
	{
		// Show tooltip
		CString zText("%Number can be used to hold place for a specific token.  Use \n"
					  "%1 to indicate the first token and %2 for the second, etc.\n"
					  "For example, if \"12|34|56|78\" is the input string, pipe (|)\n"
					  "as the delimiter, \"%1\" will be \"12\", \"%3\" will be \"56\".\n\n"
					  "No file/folder name shall contain any of the following characters:\n"
					  "back slash (\\), forward slash (/), colon (:), star (*)\n"
					  "question mark (?), quotation mark (\"), angle brackets (<>),\n"
					  "and pipe character (|)");

		// ensure that the info tip window exists before showing
		if (asCppBool(::IsWindow(m_infoTip.m_hWnd)))
		{
			m_infoTip.Show( zText );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06963");
}
//-------------------------------------------------------------------------------------------------
void CIndexConverterDlg::OnInfoAttrValue() 
{
	try
	{
		// Show tooltip
		CString zText("%Number can be used to hold place for a specific token.  Use \n"
					  "%1 to indicate the first token and %2 for the second, etc.\n"
					  "For example, if \"12|34|56|78\" is the input string, pipe (|)\n"
					  "as the delimiter, \"%1\" will be \"12\", \"%3\" will be \"56\".");

		// ensure that the info tip window exists before showing
		if (asCppBool(::IsWindow(m_infoTip.m_hWnd)))
		{
			m_infoTip.Show( zText );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06964");
}
//-------------------------------------------------------------------------------------------------
void CIndexConverterDlg::OnInfoAttrType() 
{
	try
	{
		// Show tooltip
		CString zText("%Number can be used to hold place for a specific token.  Use \n"
					  "%1 to indicate the first token and %2 for the second, etc.\n"
					  "For example, if \"12|34|56|78\" is the input string, pipe (|)\n"
					  "as the delimiter, \"%1\" will be \"12\", \"%3\" will be \"56\".");

		// ensure that the info tip window exists before showing
		if (asCppBool(::IsWindow(m_infoTip.m_hWnd)))
		{
			m_infoTip.Show( zText );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06965");
}
//-------------------------------------------------------------------------------------------------
void CIndexConverterDlg::OnInfoCondition() 
{
	try
	{
		// Show tooltip
		CString zText("%Number can be used in either the left-hand-side (LHS) or\n"
					  "right-hand-side (RHS) of the expression to represent a specific\n"
					  "token.  Use %1 to indicate the first token and %2 for the second, etc.\n"
					  "For example, if the LHS is \"%3\" and RHS is an empty string, and the\n"
					  "operator chosen is \"!=\", then the attribute will be written to the\n"
					  "output file only if the third token is not empty.");

		// ensure that the info tip window exists before showing
		if (asCppBool(::IsWindow(m_infoTip.m_hWnd)))
		{
			m_infoTip.Show( zText );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08489");
}
//-------------------------------------------------------------------------------------------------
void CIndexConverterDlg::OnButtonEav() 
{
	try
	{
		char pszPath[MAX_PATH + 1];
		if (XBrowseForFolder( m_hWnd, m_zEAVFolder, pszPath, sizeof(pszPath) ))
		{
			// Store folder
			m_zEAVFolder = pszPath;
			UpdateData(FALSE);
			updateControls();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06922")
}
//-------------------------------------------------------------------------------------------------
void CIndexConverterDlg::OnButtonFrom() 
{
	try
	{
		char pszPath[MAX_PATH + 1];
		if (XBrowseForFolder( m_hWnd, m_zFromFolder, pszPath, sizeof(pszPath) ))
		{
			// Store folder
			m_zFromFolder = pszPath;
			UpdateData(FALSE);
			updateControls();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06923")
}
//-------------------------------------------------------------------------------------------------
void CIndexConverterDlg::OnButtonTo() 
{
	try
	{
		char pszPath[MAX_PATH + 1];
		if (XBrowseForFolder( m_hWnd, m_zToFolder, pszPath, sizeof(pszPath) ))
		{
			// Store folder
			m_zToFolder = pszPath;
			UpdateData(FALSE);
			updateControls();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06924")
}
//-------------------------------------------------------------------------------------------------
void CIndexConverterDlg::OnCheckMove() 
{
	try
	{
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06952")
}
//-------------------------------------------------------------------------------------------------
void CIndexConverterDlg::OnCheckOnlyMove() 
{	
	try
	{
		UpdateData();
		// if the user only wants to use this utility to
		// move files, then disable some of the controls
		m_editEAVFile.EnableWindow(!m_bMoveFilesOnly);
		m_btnBrowseEAV.EnableWindow(!m_bMoveFilesOnly);
		m_editAttrName.EnableWindow(!m_bMoveFilesOnly);
		m_editAttrValue.EnableWindow(!m_bMoveFilesOnly);
		m_chkConsolidate.EnableWindow(!m_bMoveFilesOnly);
		m_editAttrType.EnableWindow(!m_bMoveFilesOnly);
		m_list.EnableWindow(!m_bMoveFilesOnly);

		m_btnAdd.EnableWindow(!m_bMoveFilesOnly);
		BOOL bEnableDelete = FALSE;
		BOOL bEnableModify = FALSE;

		int nSelectedCount = m_list.GetSelectedCount();
		if (nSelectedCount == 1)
		{
			bEnableDelete = TRUE;
			bEnableModify = TRUE;
		}
		else if (nSelectedCount > 1)
		{
			bEnableDelete = TRUE;
			bEnableModify = FALSE;
		}

		m_btnDelete.EnableWindow(!m_bMoveFilesOnly && bEnableDelete);
		m_btnModify.EnableWindow(!m_bMoveFilesOnly && bEnableModify);

		// auto select Move Files check box if not checked already
		if (m_bMoveFilesOnly && !m_bMoveFiles)
		{
			m_bMoveFiles = TRUE;
			UpdateData(FALSE);
		}

		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06958")
}
//-------------------------------------------------------------------------------------------------
void CIndexConverterDlg::OnButtonAdd() 
{
	try
	{
		CString zTranslateFrom(""), zTranslateTo("");
		if (promptForValue(zTranslateFrom, zTranslateTo, -1))
		{
			int nCount = m_list.GetItemCount();
			int nNewItemIndex = m_list.InsertItem(nCount, "");
			m_list.SetItemText(nNewItemIndex, 0, zTranslateFrom);
			m_list.SetItemText(nNewItemIndex, 1, zTranslateTo);

			nCount = m_list.GetItemCount();
			for (int n = 0; n <= nCount; n++)
			{
				int nState = (n == nNewItemIndex) ? LVIS_SELECTED : 0;
				m_list.SetItemState(n, nState, LVIS_SELECTED);
			}
			
			updateTranslation();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06954")
}
//-------------------------------------------------------------------------------------------------
void CIndexConverterDlg::OnButtonDelete() 
{
	try
	{
		int nIndex = m_list.GetNextItem(-1, LVNI_ALL | LVNI_SELECTED);
		if (nIndex <= -1)
		{
			return;
		}

		if (MessageBox("Do you wish to delete selected item(s)?", 
			"Confirm Delete", MB_YESNO|MB_ICONQUESTION) == IDYES)
		{		
			int nFirstItem = nIndex;
			
			// remove selected items
			while(nIndex > -1)
			{
				// remove from the UI listbox
				m_list.DeleteItem(nIndex);
				
				nIndex = m_list.GetNextItem(nIndex - 1, ((nIndex == 0) ? LVNI_ALL : LVNI_BELOW) | LVNI_SELECTED);
			}
			
			// if there's more item(s) below last deleted item, then set 
			// selection on the next item
			int nTotalNumOfItems = m_list.GetItemCount();
			if (nFirstItem < nTotalNumOfItems)
			{
				m_list.SetItemState(nFirstItem, LVIS_SELECTED, LVIS_SELECTED);
			}
			else if (nTotalNumOfItems > 0)
			{
				// select the last item
				m_list.SetItemState(nTotalNumOfItems - 1, LVIS_SELECTED, LVIS_SELECTED);
			}
				
			// update the translation map
			updateTranslation();
		}	
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06955")
}
//-------------------------------------------------------------------------------------------------
void CIndexConverterDlg::OnButtonModify() 
{
	try
	{
		CString zTranslateFrom(""), zTranslateTo("");
		int nIndex = -1;
		POSITION pos = m_list.GetFirstSelectedItemPosition();
		if (pos != __nullptr)
		{
			// Get index of first selection
			nIndex = m_list.GetNextSelectedItem(pos);
			if (nIndex > -1)
			{
				zTranslateFrom = m_list.GetItemText(nIndex, 0);
				zTranslateTo = m_list.GetItemText(nIndex, 1);
				if (promptForValue(zTranslateFrom, zTranslateTo, nIndex))
				{
					int nCount = m_list.GetItemCount();
					m_list.SetItemText(nIndex, 0, zTranslateFrom);
					m_list.SetItemText(nIndex, 1, zTranslateTo);
					
					updateTranslation();
				}
			}
		}

	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06956")
}
//-------------------------------------------------------------------------------------------------
void CIndexConverterDlg::OnButtonProcess() 
{	
	try
	{
		validateEntries();

		// Process One is pressed first time
		if (!m_bProcessOneStarted)
		{
			m_bProcessOneStarted = true;
			m_bDoneProcessing = false;

			// change button text from "Process All" to "Process Remainder"
			m_btnContinue.SetWindowText(PROCESS_REMAINING);
			// change button text from "Process One" to "Process Next"
			m_btnProcess.SetWindowText(PROCESS_NEXT);

			if (!m_zIndexFile.IsEmpty())
			{
				// open the index file 
				m_ifs.open(m_zIndexFile, ifstream::in);
			}

			// enable Reset button
			m_btnReset.EnableWindow(TRUE);
		}
		runTask(NULL);
		if (m_bDoneProcessing)
		{
			reset();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06925")
}
//-------------------------------------------------------------------------------------------------
void CIndexConverterDlg::OnButtonContinue() 
{
	try
	{
		validateEntries();

		// Set m_bProcessOneStarted to false 
		if (m_bProcessOneStarted)
		{
			m_bProcessOneStarted = false;
		}
		else if (!m_zIndexFile.IsEmpty())
		{
			// open the index file 
			m_ifs.open(m_zIndexFile, ifstream::in);
		}

		//processConversion();
		ProgressDlgTaskRunner taskRunner(this, true, true);
		taskRunner.run();
		reset();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06926")
}
//-------------------------------------------------------------------------------------------------
void CIndexConverterDlg::OnButtonClose() 
{
	CDialog::OnCancel();
}
//-------------------------------------------------------------------------------------------------
void CIndexConverterDlg::OnClose()
{
	CDialog::OnClose();
	CDialog::OnCancel();
}
//-------------------------------------------------------------------------------------------------
void CIndexConverterDlg::OnButtonReset() 
{
	try
	{
		if (MessageBox("Do wish to restart from the first record in Index File?", 
			"Reset", MB_YESNO|MB_ICONQUESTION) == IDYES)
		{
			reset();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06959")
}
//-------------------------------------------------------------------------------------------------
void CIndexConverterDlg::OnChangeEditDelimiter() 
{	
	try
	{
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06944")
}
void CIndexConverterDlg::OnChangeEditBeginStringQualifier() 
{
	try
	{
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09039")
}
//-------------------------------------------------------------------------------------------------
void CIndexConverterDlg::OnChangeEditEndStringQualifier() 
{
	try
	{
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI09040")
}
//-------------------------------------------------------------------------------------------------
//-------------------------------------------------------------------------------------------------
void CIndexConverterDlg::OnChangeEditIndex() 
{
	try
	{
		UpdateData();

		indexFileChanging(m_zIndexFile);

		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06945")
}
//-------------------------------------------------------------------------------------------------
void CIndexConverterDlg::OnChangeEditEav() 
{	
	try
	{
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06946")
}
//-------------------------------------------------------------------------------------------------
void CIndexConverterDlg::OnChangeEditFrom() 
{
	try
	{
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06947")
}
//-------------------------------------------------------------------------------------------------
void CIndexConverterDlg::OnChangeEditName() 
{
	try
	{
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06948")
}
//-------------------------------------------------------------------------------------------------
void CIndexConverterDlg::OnChangeEditTo() 
{
	try
	{
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06949")
}
//-------------------------------------------------------------------------------------------------
void CIndexConverterDlg::OnChangeEditValue() 
{
	try
	{
		updateControls();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06950")
}
//-------------------------------------------------------------------------------------------------
void CIndexConverterDlg::OnDblclkListTranslate(NMHDR* pNMHDR, LRESULT* pResult) 
{
	OnButtonModify();
	
	*pResult = 0;
}
//-------------------------------------------------------------------------------------------------
void CIndexConverterDlg::OnItemchangedListTranslate(NMHDR* pNMHDR, LRESULT* pResult) 
{
	NM_LISTVIEW* pNMListView = (NM_LISTVIEW*)pNMHDR;

	try
	{
		BOOL bEnableDelete = FALSE;
		BOOL bEnableModify = FALSE;

		int nSelectedCount = m_list.GetSelectedCount();
		if (nSelectedCount == 1)
		{
			bEnableDelete = TRUE;
			bEnableModify = TRUE;
		}
		else if (nSelectedCount > 1)
		{
			bEnableDelete = TRUE;
			bEnableModify = FALSE;
		}

		m_btnDelete.EnableWindow(bEnableDelete);
		m_btnModify.EnableWindow(bEnableModify);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06957")
	
	*pResult = 0;
}
//-------------------------------------------------------------------------------------------------
void CIndexConverterDlg::OnCheckWriteCondition() 
{
	try
	{
		UpdateData(TRUE);

		// enable the edit boxes & combo boxes only if the
		// checkbox is enabled
		GetDlgItem(IDC_EDIT_CONDITION_LHS)->EnableWindow(m_bWriteConditionEnabled);
		GetDlgItem(IDC_COMBO_CONDITION_OP)->EnableWindow(m_bWriteConditionEnabled);
		GetDlgItem(IDC_EDIT_CONDITION_RHS)->EnableWindow(m_bWriteConditionEnabled);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08488")
}

//-------------------------------------------------------------------------------------------------
// Private methods
//--------------------------------------------------------------------------------------------------
bool CIndexConverterDlg::findDuplicateEntry(const CString& zEntry, int nCurrentSelectedIndex)
{
	// go through all entries in the list
	int nSize = m_list.GetItemCount();
	for (int n=0; n<nSize; n++)
	{
		if (n == nCurrentSelectedIndex)
		{
			continue;
		}

		// get individual list item
		CString zValue = m_list.GetItemText(n, 0);
		
		// do a case sensitive comparison
		if (strcmp(zValue, zEntry) == 0)
		{
			// we found a duplicate
			return true;
		}
	}

	return false;
}
//--------------------------------------------------------------------------------------------------
void CIndexConverterDlg::indexFileChanging(const CString& zCurrentIndexFile)
{
	// only respond to this change if Process one started
	if (!m_bProcessOneStarted)
	{
		return;
	}
	
	static CString s_zIndexFileOpen(zCurrentIndexFile);
	if (_strcmpi(s_zIndexFileOpen, zCurrentIndexFile) != 0)
	{
		reset();
		
		// reset file
		s_zIndexFileOpen = zCurrentIndexFile;
	}
}
//-------------------------------------------------------------------------------------------------
bool CIndexConverterDlg::moveFiles(const string& strFilesName,
								   const string& strMoveFromFolder,
								   const string& strMoveToFolder)
{

	vector<string> vecAllSpecifiedFiles;
	::getFilesInDir(vecAllSpecifiedFiles, strMoveFromFolder, strFilesName, false);

	if (vecAllSpecifiedFiles.empty())
	{
		return false;
	}

	// only create the directory is there's file in the move from folder
	// create the directory if not exists
	if (!::directoryExists(strMoveToFolder))
	{
		if (CreateDirectory(strMoveToFolder.c_str(), NULL) == 0)
		{
			UCLIDException ue("ELI06961", "Failed to create directory.");
			ue.addDebugInfo("Directory", strMoveToFolder);
			throw ue;
		}
	}

	for (unsigned int n = 0; n < vecAllSpecifiedFiles.size(); n++)
	{
		string strFileToMove = vecAllSpecifiedFiles[n];
		string strFileNameOnly = ::getFileNameFromFullPath(strFileToMove);
		string strNewFileName = strMoveToFolder + "\\" + strFileNameOnly;

		moveFile(strFileToMove.c_str(), strNewFileName.c_str(), true);
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
void CIndexConverterDlg::processConversion()
{
	CWaitCursor wait;
	ProgressDlgTaskRunner taskRunner(this, true, true);
	taskRunner.run();	
}
//--------------------------------------------------------------------------------------------------
bool CIndexConverterDlg::promptForValue(CString& zEntry1, CString& zEntry2, int nCurrentSelectedIndex)
{
	CString	zCopy1(zEntry1);
	CString	zCopy2(zEntry2);

	Prompt2Dlg promptDlg("Text", "Translate from : ", zEntry1, "Translate to : ", zEntry2);

	while (true)
	{
		int nRes = promptDlg.DoModal();
		if (nRes == IDOK)
		{
			zEntry1 = promptDlg.m_zInput1;
			zEntry2 = promptDlg.m_zInput2;
			if (zEntry1.IsEmpty() || zEntry2.IsEmpty())
			{
				MessageBox("Please provide non-empty string.", "Entry");

				continue;
			}

			// Check to see if the text changed
			if (zEntry1.Compare(zCopy1) == 0 && zEntry2.Compare(zCopy2) == 0)
			{
				// User did not change the string, just return false
				return false;
			}

			// check whether or not to entered the zEntry already exists in the lst
			if (findDuplicateEntry(zEntry1, nCurrentSelectedIndex))
			{
				CString zMsg("");
				zMsg.Format("<%s> already exists in the list. Please specify another clue text", zEntry1);
				MessageBox(zMsg, "Duplicate");
				continue;
			}
			
			return true;
		}

		break;
	}

	return false;
}
//-------------------------------------------------------------------------------------------------
void CIndexConverterDlg::reset()
{
	m_bProcessOneStarted = false;
	// if the file is open, close it
	if (m_ifs.is_open())
	{
		m_ifs.clear();
		m_ifs.close();
		// reset button captions
		m_btnProcess.SetWindowText(PROCESS_ONE);
		m_btnContinue.SetWindowText(PROCESS_ALL);

		// after reset, disable the button
		m_btnReset.EnableWindow(FALSE);
	}
}
//-------------------------------------------------------------------------------------------------
void CIndexConverterDlg::setColumnLabels() 
{
	// Get width
	CRect	rectList;
	m_list.GetWindowRect( &rectList );
	long lColumnWidth = (rectList.Width() - 3) / 2;

	// Add column labels
	m_list.InsertColumn( 0, "Translate From", LVCFMT_LEFT, lColumnWidth, -1 );
	m_list.InsertColumn( 1, "Translate To", LVCFMT_LEFT, lColumnWidth, -1 );
}
//-------------------------------------------------------------------------------------------------
string CIndexConverterDlg::translateType(const string& strType)
{
	string strTranslatedValue(strType);

	// look up the translation map
	if (m_mapTypeTranslations.size() > 0)
	{
		map<string, string>::iterator itMap = m_mapTypeTranslations.find(strType);
		if (itMap != m_mapTypeTranslations.end())
		{
			strTranslatedValue = itMap->second;
		}
	}

	return strTranslatedValue;
}
//-------------------------------------------------------------------------------------------------
void CIndexConverterDlg::updateControls() 
{
	// Enable Process buttons only when items are defined
	UpdateData();

	// Enable edit boxes and Browse buttons only if moving files
	GetDlgItem(IDC_EDIT_FROM)->EnableWindow(m_bMoveFiles);
	GetDlgItem(IDC_EDIT_TO)->EnableWindow(m_bMoveFiles);

	// Buttons
	m_btnFrom.EnableWindow(m_bMoveFiles);
	m_btnTo.EnableWindow(m_bMoveFiles);

	// The following edit boxes shall not be empty : 
	// - Index file
	// - Delimiter
	// eav path and file
	// Move file and to if the check box is checked
	// Attribute name and value
	BOOL bEnableProcessOne = TRUE;
	BOOL bEnableProcessAll = TRUE;
	if (!m_bMoveFilesOnly)
	{
		if ( m_zIndexFile.IsEmpty()
			|| m_zDelimiter.IsEmpty()
			|| m_zEAVFolder.IsEmpty()
			|| (m_bMoveFiles && (m_zFromFolder.IsEmpty() || m_zToFolder.IsEmpty()))
			|| m_zName.IsEmpty()
			|| m_zValueFormat.IsEmpty() )
		{
			bEnableProcessOne = FALSE;
			bEnableProcessAll = FALSE;
		}
	}
	else
	{
		if (m_zIndexFile.IsEmpty() 
			|| m_zDelimiter.IsEmpty()
			|| !m_bMoveFiles 
			|| (m_bMoveFiles && (m_zFromFolder.IsEmpty() || m_zToFolder.IsEmpty())))
		{
			bEnableProcessOne = FALSE;
		}
		if (!m_bMoveFiles 
			|| (m_bMoveFiles && (m_zFromFolder.IsEmpty() || m_zToFolder.IsEmpty())))
		{
			bEnableProcessAll = FALSE;
		}
	}

	m_btnProcess.EnableWindow(bEnableProcessOne);
	m_btnContinue.EnableWindow(bEnableProcessAll);
}
//-------------------------------------------------------------------------------------------------
void CIndexConverterDlg::updateTranslation()
{
	// check the list
	int nCount = m_list.GetItemCount();
	for (int n=0; n<nCount; n++)
	{
		CString zIndex = m_list.GetItemText(n, 0);
		CString zTranslation = m_list.GetItemText(n, 1);

		m_mapTypeTranslations[(LPCTSTR)zIndex] = (LPCTSTR)zTranslation;
	}
}
//-------------------------------------------------------------------------------------------------
void CIndexConverterDlg::validateEntries()
{
	UpdateData();

	// index file must exist
	if (!m_zIndexFile.IsEmpty())
	{
		if (!fileExistsAndIsReadable(m_zIndexFile.GetString()))
		{
			m_editIndexFile.SetFocus();
			m_editIndexFile.SetSel(0, -1);
			UCLIDException ue("ELI06951", "Index file either doesn't exist or you don't have read permission to it.");
			ue.addDebugInfo("Index file", (LPCTSTR)m_zIndexFile);
			ue.addWin32ErrorInfo();
			throw ue;
		}

		if (m_zDelimiter.IsEmpty())
		{
			m_editDelimiter.SetFocus();
			m_editDelimiter.SetSel(0, -1);
			UCLIDException ue("ELI06960", "Please specify a non-empty delimiter.");
			throw ue;
		}
	}
}
//-------------------------------------------------------------------------------------------------
bool CIndexConverterDlg::isWriteConditionMet(vector<string>& rvecTokens)
{
	bool bWriteAttribute = true;

	// check if the attribute writing is conditional
	if (m_bWriteConditionEnabled)
	{
		string strLHS = combine((LPCTSTR) m_zConditionLHS, rvecTokens);
		string strRHS = combine((LPCTSTR) m_zConditionRHS, rvecTokens);
		CString zOp;
		m_comboConditionOp.GetLBText(m_comboConditionOp.GetCurSel(), zOp);
		if (zOp != STR_EQUAL && zOp != STR_NOT_EQUAL)
		{
			THROW_LOGIC_ERROR_EXCEPTION("ELI08490");
		}
		else
		{
			bWriteAttribute = (strLHS == strRHS && zOp == STR_EQUAL) ||
				(strLHS != strRHS && zOp == STR_NOT_EQUAL);
		}
	}

	return bWriteAttribute;
}
//-------------------------------------------------------------------------------------------------
