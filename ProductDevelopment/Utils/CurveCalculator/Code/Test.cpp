// Test.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "Test.h"
#include <CurveCalculatorDlg.h>
#include <ECurveParameter.h>
#include <UCLIDExceptionDlg.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

HINSTANCE gModuleResource = NULL;

/////////////////////////////////////////////////////////////////////////////
// CTestApp

BEGIN_MESSAGE_MAP(CTestApp, CWinApp)
	//{{AFX_MSG_MAP(CTestApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG
	ON_COMMAND(ID_HELP, CWinApp::OnHelp)
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CTestApp construction

CTestApp::CTestApp()
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}

/////////////////////////////////////////////////////////////////////////////
// The one and only CTestApp object

CTestApp theApp;

/////////////////////////////////////////////////////////////////////////////
// CTestApp initialization

BOOL CTestApp::InitInstance()
{
	AfxEnableControlContainer();

	// Standard initialization
	// If you are not using these features and wish to reduce the size
	//  of your final executable, you should remove from the following
	//  the specific initialization routines you do not need.

#ifdef _AFXDLL
	Enable3dControls();			// Call this when using MFC in a shared DLL
#else
	Enable3dControlsStatic();	// Call this when linking to MFC statically
#endif

	// Store instance for resources
	gModuleResource = AfxGetResourceHandle();

	// Prepare viewer for UCLID exceptions
	static UCLIDExceptionDlg	dlg;
	UCLIDException::setExceptionHandler( &dlg );

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
	bool				bHideUnits = true;
	bool				bHideOK = false;

	////////////////////
	// Create the dialog
	////////////////////

	try
	{
#if 1
		// Dialog will use default units and have no combo box items selected
		// or values available.  Units are visible.  OK button is hidden.
		CCurveCalculatorDlg dlg( true, false, true );
#else
		// Dialog will use provided combo box items and values
		CCurveCalculatorDlg dlg( 
			eSelect1, str1.c_str(),		// first curve parameter and value
			eSelect2, str2.c_str(),		// second curve parameter and value
			eSelect3, str3.c_str(),		// third curve parameter and value
			iConcavity,					// angle concavity
			iAngle,						// angle size
			bFeet,						// default units
			bHideUnits,					// show or hide units controls
			bHideOK						// show or hide OK button
			);
#endif

		// Assocaite the dialog with our main window
		m_pMainWnd = &dlg;

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
			//  0 : concave right
			//  1 : concave left
			iConcavity = dlg.getConcaveLeft();

			// Angle size
			// -1 : not required
			//  0 : angle > PI
			//  1 : angle < PI
			iAngle = dlg.getAngleLessThanPi();

			// Units
			//  true : Units are feet
			// false : Units are meters
			bFeet = dlg.isUnitsFeet();
		}
	}
	CATCH_UCLID_EXCEPTION()
	CATCH_UNEXPECTED_EXCEPTION( "ELI01478" )

	// Since the dialog has been closed, return FALSE so that we exit the
	//  application, rather than start the application's message pump.
	return FALSE;
}
