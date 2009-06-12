// TestDlg.cpp : implementation file
//

#include "stdafx.h"
#include "Test.h"
#include "TestDlg.h"

#include "..\Code\CurveCalculatorDlg.h"
#include <ECurveParameter.h>
#include <UCLIDExceptionDlg.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CTestDlg dialog

CTestDlg::CTestDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CTestDlg::IDD, pParent)
{
	//{{AFX_DATA_INIT(CTestDlg)
		// NOTE: the ClassWizard will add member initialization here
	//}}AFX_DATA_INIT
	// Note that LoadIcon does not require a subsequent DestroyIcon in Win32
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);

	// Prepare viewer for UCLID exceptions
	static UCLIDExceptionDlg	dlg;
	UCLIDException::setExceptionHandler( &dlg );
}

void CTestDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CTestDlg)
		// NOTE: the ClassWizard will add DDX and DDV calls here
	//}}AFX_DATA_MAP
}

BEGIN_MESSAGE_MAP(CTestDlg, CDialog)
	//{{AFX_MSG_MAP(CTestDlg)
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_BN_CLICKED(IDC_MODAL_WITH, OnModalWith)
	ON_BN_CLICKED(IDC_MODAL_WITHOUT, OnModalWithout)
	ON_BN_CLICKED(IDC_MODELESS_WITH, OnModelessWith)
	ON_BN_CLICKED(IDC_MODELESS_WITHOUT, OnModelessWithout)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CTestDlg message handlers

BOOL CTestDlg::OnInitDialog()
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

void CTestDlg::OnPaint() 
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
HCURSOR CTestDlg::OnQueryDragIcon()
{
	return (HCURSOR) m_hIcon;
}

void CTestDlg::OnModalWith() 
{
	// Set up initializations for dialog
	ECurveParameterType eSelect1 = kArcTangentInBearing;
	ECurveParameterType eSelect2 = kArcChordBearing;
	ECurveParameterType eSelect3 = kArcRadius;
	std::string			str1 = "n45e";
	std::string			str2 = "n90e";
	std::string			str3 = "22";
	int					iConcavity = 0;
	int					iAngle = 0;
	bool				bFeet = true;
	bool				bHide = true;

	////////////////////
	// Create the dialog
	////////////////////

	try
	{
		// Dialog will use provided combo box items and values
		CCurveCalculatorDlg dlg( 
			eSelect1, str1.c_str(),		// first curve parameter and value
			eSelect2, str2.c_str(),		// second curve parameter and value
			eSelect3, str3.c_str(),		// third curve parameter and value
			iConcavity,					// angle concavity
			iAngle,						// angle size
			bFeet,						// default units
			bHide						// show or hide units controls
			);

		// Run the dialog
		int nResponse = dlg.DoModal();
		if (nResponse == IDOK)
		{
			///////////////////////////////
			// Retrieve results from dialog
			///////////////////////////////

			// Selected curve parameters
			eSelect1 = dlg.getParameter( 1 );
			eSelect2 = dlg.getParameter( 2 );
			eSelect3 = dlg.getParameter( 3 );

			// Curve parameter values as strings
			str1 = dlg.getParameterValue( 1 );
			str2 = dlg.getParameterValue( 2 );
			str3 = dlg.getParameterValue( 3 );

			// Angle concavity
			// -1 : not required
			//  0 : concave left
			//  1 : concave right
			iConcavity = dlg.getConcavity();

			// Angle size
			// -1 : not required
			//  0 : angle < PI
			//  1 : angle > PI
			iAngle = dlg.getAngleSize();

			// Units
			//  true : Units are feet
			// false : Units are meters
			bFeet = dlg.isUnitsFeet();
		}
	}
	CATCH_UCLID_EXCEPTION()
	CATCH_UNEXPECTED_EXCEPTION( "ELI01479" )
}

void CTestDlg::OnModalWithout() 
{
	// Set up initializations for dialog
	ECurveParameterType eSelect1 = kArcTangentInBearing;
	ECurveParameterType eSelect2 = kArcChordBearing;
	ECurveParameterType eSelect3 = kArcRadius;
	std::string			str1 = "";
	std::string			str2 = "";
	std::string			str3 = "";
	int					iConcavity = 0;
	int					iAngle = 0;
	bool				bFeet = true;
	bool				bHide = true;

	////////////////////
	// Create the dialog
	////////////////////

	try
	{
		// Dialog will use default units and have no combo box items selected
		// or values available
		CCurveCalculatorDlg dlg;

		// Run the dialog
		int nResponse = dlg.DoModal();
		if (nResponse == IDOK)
		{
			///////////////////////////////
			// Retrieve results from dialog
			///////////////////////////////

			// Selected curve parameters
			eSelect1 = dlg.getParameter( 1 );
			eSelect2 = dlg.getParameter( 2 );
			eSelect3 = dlg.getParameter( 3 );

			// Curve parameter values as strings
			str1 = dlg.getParameterValue( 1 );
			str2 = dlg.getParameterValue( 2 );
			str3 = dlg.getParameterValue( 3 );

			// Angle concavity
			// -1 : not required
			//  0 : concave left
			//  1 : concave right
			iConcavity = dlg.getConcavity();

			// Angle size
			// -1 : not required
			//  0 : angle < PI
			//  1 : angle > PI
			iAngle = dlg.getAngleSize();

			// Units
			//  true : Units are feet
			// false : Units are meters
			bFeet = dlg.isUnitsFeet();
		}
	}
	CATCH_UCLID_EXCEPTION()
	CATCH_UNEXPECTED_EXCEPTION( "ELI01480" )
}

void CTestDlg::OnModelessWith() 
{
	// TODO: Create the modeless dialog and provide starting parameters
}

void CTestDlg::OnModelessWithout() 
{
	// TODO: Create the modeless dialog without starting parameters
}
