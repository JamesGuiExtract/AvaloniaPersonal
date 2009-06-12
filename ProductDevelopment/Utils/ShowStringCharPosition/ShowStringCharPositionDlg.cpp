// ShowStringCharPositionDlg.cpp : implementation file
//

#include "stdafx.h"
#include "ShowStringCharPosition.h"
#include "ShowStringCharPositionDlg.h"

#include <fstream>
#include <io.h>
using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CShowStringCharPositionDlg dialog

CShowStringCharPositionDlg::CShowStringCharPositionDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CShowStringCharPositionDlg::IDD, pParent)
{
	//{{AFX_DATA_INIT(CShowStringCharPositionDlg)
	m_zInput = _T("");
	//}}AFX_DATA_INIT
	// Note that LoadIcon does not require a subsequent DestroyIcon in Win32
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}

void CShowStringCharPositionDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CShowStringCharPositionDlg)
	DDX_Text(pDX, IDC_EDIT_INPUT, m_zInput);
	//}}AFX_DATA_MAP
}

BEGIN_MESSAGE_MAP(CShowStringCharPositionDlg, CDialog)
	//{{AFX_MSG_MAP(CShowStringCharPositionDlg)
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CShowStringCharPositionDlg message handlers

BOOL CShowStringCharPositionDlg::OnInitDialog()
{
	CDialog::OnInitDialog();

	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon
	
	// TODO: Add extra initialization here
	
	return TRUE;  // return TRUE  unless you set the focus to a control
}

// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.

void CShowStringCharPositionDlg::OnPaint() 
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
HCURSOR CShowStringCharPositionDlg::OnQueryDragIcon()
{
	return (HCURSOR) m_hIcon;
}

void CShowStringCharPositionDlg::OnOK() 
{
	// get the data the the user wants to show string char positions for
	UpdateData(TRUE);

	// create the output file
	string strTest = m_zInput;
	string strOutFileName = "c:\\temp\\ShowStringCharPosition.tmp";
	ofstream outfile(strOutFileName.c_str());
	if (!outfile)
	{
		string strMsg = "ERROR: Unable to create output file:\n";
		strMsg += strOutFileName;
		MessageBox(strMsg.c_str());
		return;
	}

	string strOutput;
	string strTemplate1 = "0                             1                           ";
	string strTemplate2 = "0  1  2  3  4  5  6  7  8  9  0  1  2  3  4  5  6  7  8  9";

	const int iCharsPerLine = 20;

	for (unsigned int i = 0; i <= strTest.length(); i++)
	{
		// write out printable chars to the output.  For
		// all non-printable chars (except for the space character),
		// write their hex code.
		char cTemp = strTest[i];
		if (cTemp >= 32 && cTemp <= 126)
		{
			strOutput += cTemp;
			strOutput += "  ";
		}
		else
		{
			CString zTemp;
			// if the char is one of the special chars we know about like \r\n, etc
			// then represent it as \r\n, etc (and not a hex code).  If the char
			// is one that we don't specially know about, display it as a 
			// hex code
			if (cTemp == '\r' || cTemp == '\n' || cTemp == '\0' || cTemp == '\t')
			{
				switch (cTemp)
				{
				case '\r': zTemp = "\\r"; break;
				case '\n': zTemp = "\\n"; break;
				case '\t': zTemp = "\\t"; break;
				case '\0': zTemp = "\\0"; break;
				}
			}
			else
			{
				zTemp.Format("%02X", cTemp);
			}
			strOutput += zTemp;
			strOutput += " ";
		}

		// if we are at the end of the string, or if we are at a point
		// where we need to do a line break, then write the line(s) to the
		// output file
		if (i != 0 && (i + 1) % iCharsPerLine == 0 || i == strTest.length())
		{
			CString zBase;
			zBase.Format("%05d  ", iCharsPerLine * (i / iCharsPerLine));
			CString zBaseSpaces = "       ";

			string strTemp = zBaseSpaces;
			strTemp +=strTemplate1;
			strTemp += string("\n");
			strTemp += zBase;
			strTemp += strTemplate2;
			strTemp += string("\n");
			strTemp += zBaseSpaces;
			strTemp += strOutput;
			outfile << strTemp.c_str() << endl << endl;
			strOutput = "";
		}
	}

	// Close the file
	outfile.close();
	
	// Wait for the file to be readable
	for (int i=0; i < 50; i++)
	{
		if(_access_s(strOutFileName.c_str(), 4) == 0)
		{
			break;
		}

		Sleep(100);
	}

	// NOTE: we're not using BaseUtils.dll and other UCLID dll's 
	// to reduce dependency on such dlls when we only have such simple
	// code to use.  By not using the dll, we don't have to recompile this
	// utility everytime that dll is modified.

	// launch notepad to display the temporary file
	string strCommand = "notepad ";
	strCommand += strOutFileName;
	WinExec(strCommand.c_str(), SW_SHOW);
}
