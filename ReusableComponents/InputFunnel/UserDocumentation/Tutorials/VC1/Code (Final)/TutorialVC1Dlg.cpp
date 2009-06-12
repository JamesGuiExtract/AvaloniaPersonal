// TutorialVC1Dlg.cpp : implementation file
//

#include "stdafx.h"
#include "TutorialVC1.h"
#include "TutorialVC1Dlg.h"
#include <math.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CTutorialVC1Dlg dialog

CTutorialVC1Dlg::CTutorialVC1Dlg(CWnd* pParent /*=NULL*/)
	: CDialog(CTutorialVC1Dlg::IDD, pParent)
{
	//{{AFX_DATA_INIT(CTutorialVC1Dlg)
	m_dBearingInRadians = 0.0;
	m_dDistanceInFeet = 0.0;
	m_zEndPoint = _T("");
	m_zStartPoint = _T("");
	//}}AFX_DATA_INIT
	// Note that LoadIcon does not require a subsequent DestroyIcon in Win32
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}

void CTutorialVC1Dlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CTutorialVC1Dlg)
	DDX_Text(pDX, IDC_EDIT_BEARING, m_dBearingInRadians);
	DDX_Text(pDX, IDC_EDIT_DISTANCE, m_dDistanceInFeet);
	DDX_Text(pDX, IDC_EDIT_END, m_zEndPoint);
	DDX_Text(pDX, IDC_EDIT_START, m_zStartPoint);
	//}}AFX_DATA_MAP
}

BEGIN_MESSAGE_MAP(CTutorialVC1Dlg, CDialog)
	//{{AFX_MSG_MAP(CTutorialVC1Dlg)
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_BN_CLICKED(ID_CALCULATE, OnCalculate)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CTutorialVC1Dlg message handlers

BOOL CTutorialVC1Dlg::OnInitDialog()
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

void CTutorialVC1Dlg::OnPaint() 
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
HCURSOR CTutorialVC1Dlg::OnQueryDragIcon()
{
	return (HCURSOR) m_hIcon;
}

void CTutorialVC1Dlg::OnCalculate() 
{
	// Retrieve the data
	UpdateData( TRUE );

	// Parse the start point string into X and Y elements
	int iCommaPos = strchr( m_zStartPoint, ',' ) - (LPCTSTR)m_zStartPoint;

	// Convert X and Y strings into numbers
	double dStartX, dStartY;
	dStartX = atof( m_zStartPoint.Left(iCommaPos) );
	dStartY = atof( m_zStartPoint.Right(
		m_zStartPoint.GetLength() - iCommaPos - 1) );

	// Compute the end point
	double dEndX, dEndY;
	dEndX = dStartX + cos(m_dBearingInRadians) * m_dDistanceInFeet;
	dEndY = dStartY + sin(m_dBearingInRadians) * m_dDistanceInFeet;

	// Format the result as a string and display in edit box
	m_zEndPoint.Format( "%f , %f", dEndX, dEndY );
	UpdateData( FALSE );
}
