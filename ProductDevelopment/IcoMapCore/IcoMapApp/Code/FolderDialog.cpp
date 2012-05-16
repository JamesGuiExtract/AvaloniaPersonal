#include "stdafx.h"
#include "FolderDialog.h"

#include <DLGS.H>
#include <WINUSER.H>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

static BOOL sIsDirectory(CString lpszName)
{
	DWORD dwRet;
	dwRet = GetFileAttributes(lpszName);
	return (dwRet != 0xFFFFFFFF) && (dwRet & FILE_ATTRIBUTE_DIRECTORY);
}

/////////////////////////////////////////////////////////////////////////////
// CFolderDialog

IMPLEMENT_DYNAMIC(CFolderDialog, CFileDialog)

WNDPROC CFolderDialog::m_wndProc = NULL;


// Function name	: CFolderDialog::CFolderDialog
// Description	    : Constructor
// Return type		: 
// Argument         : CString* pPath ; represent string where selected folder wil be saved
CFolderDialog::CFolderDialog(CString pPath, int modeIn) : CFileDialog(TRUE, NULL, NULL)
{
	m_pPath = pPath.Left(pPath.ReverseFind('\\'));
	m_mode = modeIn;
	m_ofn.lpstrInitialDir = m_pPath;
}


BEGIN_MESSAGE_MAP(CFolderDialog, CFileDialog)
	//{{AFX_MSG_MAP(CFolderDialog)
	ON_WM_CREATE()
	ON_WM_LBUTTONDBLCLK()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

// Function name	: WindowProcNew
// Description	    : Call this function when user navigate into CFileDialog.
// Return type		: LRESULT
// Argument         : HWND hwnd
// Argument         : UINT message
// Argument         : WPARAM wParam
// Argument         : LPARAM lParam
LRESULT CALLBACK WindowProcNew(HWND hwnd,UINT message, WPARAM wParam, LPARAM lParam)
{	
	if (message ==  WM_COMMAND)
	{
		if (HIWORD(wParam) == BN_CLICKED)
		{
			if (LOWORD(wParam) == IDOK)
			{
				if (CFileDialog* pDlg = (CFileDialog*)CWnd::FromHandle(hwnd))
				{
					pDlg->EndDialog(IDOK);
					return NULL;
				}
			}
		}
	}

	return CallWindowProc(CFolderDialog::m_wndProc, hwnd, message, wParam, lParam);
}

void CFolderDialog::OnFileNameChange() 
{ 
	TCHAR path[MAX_PATH]; 

	GetCurrentDirectory ( MAX_PATH, path ); 
	m_pPath = CString (path);

	CWnd* pParent = GetParent ( )->GetDlgItem ( lst2 ); 

	// get the list control 
	CListCtrl *pList = ( CListCtrl * ) pParent->GetDlgItem ( 1 ); 

	// currently selected item 
	int pos = pList->GetNextItem ( -1, LVNI_ALL | LVNI_SELECTED ); 

	if ( pos != -1 ) 
	{ 
		// create the full path... 
		CString selection = pList->GetItemText( pos, 0 ); 
		CString testStr = CString (path);
		//if the end of path string already has "\\"
		if ( m_pPath.ReverseFind('\\') == m_pPath.GetLength()-1)
		{
			testStr +=  selection; 
		}
		else
		{
			testStr += _T ( "\\" ) + selection;
		}

		if (::sIsDirectory(testStr))
		{
			m_pPath = testStr;
		}
	} 
	GetParent()->GetDlgItem(edt1)->SetWindowText(m_pPath);
} 


void CFolderDialog::OnFolderChange () 
{ 
	OnFileNameChange();
} 

// Function name	: CFolderDialog::OnInitDone
// Description	    : For update the wiew of CFileDialog
// Return type		: void 
void CFolderDialog::OnInitDone()
{
	HideControl(cmb1);
	HideControl(stc2);
	CWnd* pFD = GetParent();
	CRect rectCancel; 
	pFD->GetDlgItem(IDCANCEL)->GetWindowRect(rectCancel);
	pFD->ScreenToClient(rectCancel);
	CString titleStr;
	if (m_mode == CFolderDialog::OPEN_MODE)
	{
		titleStr = "Open";
	}
	else if (m_mode == CFolderDialog::SELECT_MODE)
	{
		titleStr = "Select";
	}
	SetControlText(IDOK, titleStr);
	
	if (m_mode == CFolderDialog::OPEN_MODE)
	{
		titleStr = "Open Folder";
	}
	else if (m_mode == CFolderDialog::SELECT_MODE)
	{
		titleStr = "Select Folder";
	}
	pFD->SetWindowText(titleStr);
	
	CRect rectList2; 
	pFD->GetDlgItem(stc3)->GetWindowRect(rectList2);
	pFD->ScreenToClient(rectList2);
	pFD->GetDlgItem(stc3)->SetWindowPos(0,0,0,rectList2.Width()+8,rectList2.Height(), SWP_NOMOVE | SWP_NOZORDER);
	titleStr = "Folder name:";
	pFD->GetDlgItem(stc3)->SetWindowText(titleStr);
	pFD->GetDlgItem(edt1)->SetWindowText(m_pPath);
	m_wndProc = (WNDPROC)SetWindowLong(pFD->m_hWnd, GWL_WNDPROC, (long)WindowProcNew);
}

void CFolderDialog::SetEdt1(CString inStr)
{
	GetParent()->GetDlgItem(edt1)->SetWindowText(inStr);
	m_pPath = inStr;
}

void CFolderDialog::GetEdt1(CString &inStr)
{
	GetParent()->GetDlgItem(edt1)->GetWindowText(inStr);
}

BOOL CFolderDialog::OnInitDialog() 
{
	CFileDialog::OnInitDialog();
	
	GetParent()->GetDlgItem(edt1)->SetWindowText(m_pPath);
	
	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}

int CFolderDialog::OnCreate(LPCREATESTRUCT lpCreateStruct) 
{
	if (CFileDialog::OnCreate(lpCreateStruct) == -1)
	{
		return -1;
	}
	
	GetParent()->GetDlgItem(edt1)->SetWindowText(m_pPath);
	
	return 0;
}

void CFolderDialog::OnLButtonDblClk(UINT nFlags, CPoint point) 
{
	// TODO: Add your message handler code here and/or call default
	
	CFileDialog::OnLButtonDblClk(nFlags, point);
}
