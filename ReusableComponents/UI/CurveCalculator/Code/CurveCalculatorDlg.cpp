//=============================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	CurveCalculatorDlg.cpp
//
// PURPOSE:	Source code for main dialog class in Curve Calculator application.
//				Also includes App-wizard generated code for the About box.
//
// NOTES:	Uses CCE DLL and Filters DLL
//
// AUTHOR:	Wayne Lenius
//
//=============================================================================

#include "stdafx.h"
#include "CurveCalculatorDlg.h"

#include <CurveCalculationEngineImpl.h>
#include <CurveDjinni.h>
#include <CurveVariable.h>
#include <DirectionHelper.h>
#include <DistanceCore.h>
#include <TemporaryResourceOverride.h>
#include <cpputil.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// externs
extern HINSTANCE gModuleResource;

/////////////////////////////////////////////////////////////////////////////
// CAboutDlg dialog used for App About
/////////////////////////////////////////////////////////////////////////////

class CAboutDlg : public CDialog
{
public:
	CAboutDlg();

// Dialog Data
	//{{AFX_DATA(CAboutDlg)
	enum { IDD = IDD_ABOUTBOX };
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CAboutDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	//{{AFX_MSG(CAboutDlg)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

CAboutDlg::CAboutDlg() : CDialog(CAboutDlg::IDD)
{
	//{{AFX_DATA_INIT(CAboutDlg)
	//}}AFX_DATA_INIT
}

void CAboutDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CAboutDlg)
	//}}AFX_DATA_MAP
}

BEGIN_MESSAGE_MAP(CAboutDlg, CDialog)
	//{{AFX_MSG_MAP(CAboutDlg)
		// No message handlers
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()


/////////////////////////////////////////////////////////////////////////////
// CCurveCalculatorDlg dialog
/////////////////////////////////////////////////////////////////////////////

//=============================================================================
// PURPOSE: Constructs a CurveCalculatorDlg object and initializes member
//				variables to defaults.
// REQUIRE: None.
// PROMISE: Creates a Curve Calculation Engine object, a Curve Djinni object, 
//				sets all flags to false, defaults distance units to feet.
// ARGS:	None.
//=============================================================================
CCurveCalculatorDlg::CCurveCalculatorDlg(bool bHideUnits /*=false*/, 
										 bool bHideOK /*=false*/, 
										 CWnd* pParent /*=NULL*/)
	: CDialog(CCurveCalculatorDlg::IDD, pParent),
	m_pEngine(NULL),
	m_pDjinni(NULL),
	m_bCurveEnable(false),
	m_bAngleEnable(false),
	m_bReset(true),
	m_bHideUnits(bHideUnits),
	m_bHideOK(bHideOK)
{
	//{{AFX_DATA_INIT(CCurveCalculatorDlg)
	m_nDefaultUnits = -1;
	m_nConcavity = -1;
	m_nDeltaAngle = -1;
	m_cstrOutput = _T("");
	//}}AFX_DATA_INIT
	// Note that LoadIcon does not require a subsequent DestroyIcon in Win32
	TemporaryResourceOverride temp(gModuleResource);
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);

	// Initialize curve calculation engine object
	m_pEngine = new CurveCalculationEngineImpl();
	ASSERT_RESOURCE_ALLOCATION( "ELI01351", (m_pEngine != NULL) );

	// Initialize curve djinni object
	m_pDjinni = new CurveDjinni();
	ASSERT_RESOURCE_ALLOCATION( "ELI01352", (m_pDjinni != NULL) );

	// Store units choice
	if (!m_bHideUnits)
	{
		// Use feet
		m_nDefaultUnits = 0;
	}
}

//=============================================================================
// PURPOSE: Constructs a CurveCalculatorDlg object and initializes member
//				variables.  Allows calling method to specify initial combo box 
//				selections.
// REQUIRE: None.
// PROMISE: Creates a Curve Calculation Engine object, a Curve Djinni object, 
//				sets all flags to false, defaults distance units to feet.
// ARGS:	None.
//=============================================================================
CCurveCalculatorDlg::CCurveCalculatorDlg(ECurveParameterType eSelect1, 
	LPCTSTR pszString1, ECurveParameterType eSelect2, LPCTSTR pszString2, 
	ECurveParameterType eSelect3, LPCTSTR pszString3, int iConcavity, int iAngle, 
	bool bHideUnits /*=false*/, 
	bool bHideOK /*=false*/, CWnd* pParent /*=NULL*/)
	: CDialog(CCurveCalculatorDlg::IDD, pParent),
	m_pEngine(NULL),
	m_pDjinni(NULL),
	m_bCurveEnable(false),
	m_bAngleEnable(false),
	m_bReset(true),
	m_bHideUnits(bHideUnits),
	m_bHideOK(bHideOK)
{
	//{{AFX_DATA_INIT(CCurveCalculatorDlg)
	m_nDefaultUnits = -1;
	m_nConcavity = -1;
	m_nDeltaAngle = -1;
	m_cstrOutput = _T("");
	//}}AFX_DATA_INIT
	// Note that LoadIcon does not require a subsequent DestroyIcon in Win32
	TemporaryResourceOverride temp(gModuleResource);
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);

	// Initialize curve calculation engine object
	m_pEngine = new CurveCalculationEngineImpl();
	ASSERT_RESOURCE_ALLOCATION( "ELI01475", (m_pEngine != NULL) );

	// Initialize curve djinni object
	m_pDjinni = new CurveDjinni();
	ASSERT_RESOURCE_ALLOCATION( "ELI01476", (m_pDjinni != NULL) );

	// Validate CCE input parameters
	if (isInputValid( eSelect1, pszString1, eSelect2, pszString2, eSelect3, 
		pszString3, iConcavity, iAngle))
	{
		///////////////////////////////////
		// Store parameters in data members
		///////////////////////////////////

		// Combo box items
		m_eCombo1Selection = eSelect1;
		m_eCombo2Selection = eSelect2;
		m_eCombo3Selection = eSelect3;
		setUsedParameter( m_eCombo1Selection, kParameter1 );
		setUsedParameter( m_eCombo2Selection, kParameter2 );
		setUsedParameter( m_eCombo3Selection, kParameter3 );

		// Edit box items
		m_strEdit1 = pszString1;
		m_strEdit2 = pszString2;
		m_strEdit3 = pszString3;

		// Radio buttons
		if (!m_bHideUnits)
		{
			// Use feet
			m_nDefaultUnits = 0;
		}

		m_nConcavity = iConcavity;
		m_nDeltaAngle = iAngle;

		// Set flag to bypass parameter reset in OnInitDialog()
		m_bReset = FALSE;
	}
	else
	{
		// Code cleanup
		if (m_pDjinni)
		{
			delete m_pDjinni;
			m_pDjinni = NULL;
		}

		if (m_pEngine)
		{
			delete m_pEngine;
			m_pEngine = NULL;
		}

		// Throw exception
		THROW_LOGIC_ERROR_EXCEPTION( "ELI01477" );
	}
}

void CCurveCalculatorDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CCurveCalculatorDlg)
	DDX_Control(pDX, IDC_EDIT3, m_edit3);
	DDX_Control(pDX, IDC_EDIT2, m_edit2);
	DDX_Control(pDX, IDC_EDIT1, m_edit1);
	DDX_Control(pDX, IDC_COMBO3, m_cbnCombo3);
	DDX_Control(pDX, IDC_COMBO2, m_cbnCombo2);
	DDX_Control(pDX, IDC_COMBO1, m_cbnCombo1);
	DDX_Radio(pDX, IDC_RADIO_FEET, m_nDefaultUnits);
	DDX_Radio(pDX, IDC_RADIO_RIGHT, m_nConcavity);
	DDX_Radio(pDX, IDC_RADIO_LESSER, m_nDeltaAngle);
	DDX_Text(pDX, IDC_EDIT_OUTPUT, m_cstrOutput);
	//}}AFX_DATA_MAP
}

BEGIN_MESSAGE_MAP(CCurveCalculatorDlg, CDialog)
	//{{AFX_MSG_MAP(CCurveCalculatorDlg)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_CBN_SELCHANGE(IDC_COMBO1, OnSelchangeCombo1)
	ON_CBN_SELCHANGE(IDC_COMBO2, OnSelchangeCombo2)
	ON_CBN_SELCHANGE(IDC_COMBO3, OnSelchangeCombo3)
	ON_BN_CLICKED(ID_CALCULATE, OnCalculate)
	ON_EN_CHANGE(IDC_EDIT1, OnChangeEdit1)
	ON_EN_CHANGE(IDC_EDIT2, OnChangeEdit2)
	ON_EN_CHANGE(IDC_EDIT3, OnChangeEdit3)
	ON_BN_CLICKED(IDC_RADIO_LEFT, OnRadioLeft)
	ON_BN_CLICKED(IDC_RADIO_RIGHT, OnRadioRight)
	ON_BN_CLICKED(IDC_RADIO_LESSER, OnRadioLesser)
	ON_BN_CLICKED(IDC_RADIO_GREATER, OnRadioGreater)
	ON_BN_CLICKED(IDC_RADIO_FEET, OnRadioFeet)
	ON_BN_CLICKED(IDC_RADIO_METERS, OnRadioMeters)
	ON_BN_CLICKED(IDOK, OnOK)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CCurveCalculatorDlg message handlers
/////////////////////////////////////////////////////////////////////////////

//=============================================================================
// PURPOSE: Initializes combo boxes and radio buttons after doing general
//				dialog setup.
// REQUIRE: Called automatically via MessageMap.
// PROMISE: Returns TRUE.
// ARGS:	None.
//=============================================================================
BOOL CCurveCalculatorDlg::OnInitDialog()
{
	try
	{
		CDialog::OnInitDialog();

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

		// Set up the collection of parameters
		m_vecPCB.clear();
		createParameterControlBlock();

		// Check to see if parameters have been provided
		if (m_bReset)
		{
			// Prepare the combo boxes
			resetCombos();

			// Default radio buttons
			m_nDefaultUnits = 0;
			// default curve to the left
			m_nConcavity = 1;
			// default curve delta angle less than 180
			m_nDeltaAngle = 0;
		}
		else
		{
			// Engine has been preloaded
			// Combo box selections are ready
			// Still need to populate the combo boxes
			// Still need to populate edit boxes
			m_edit1.SetWindowText( m_strEdit1.c_str() );
			m_edit2.SetWindowText( m_strEdit2.c_str() );
			m_edit3.SetWindowText( m_strEdit3.c_str() );

			// First combo box is full
			resetDropDownList( m_cbnCombo1, 0 );
			selectComboItem( m_cbnCombo1, m_eCombo1Selection );
			setUsedParameter( m_eCombo1Selection, kParameter1 );

			// Second combo box depends on first selection
			resetDropDownList( m_cbnCombo2, 1 );
			selectComboItem( m_cbnCombo2, m_eCombo2Selection );
			setUsedParameter( m_eCombo2Selection, kParameter2 );

			// Third combo box depends on first two selections
			resetDropDownList( m_cbnCombo3, 2 );
			selectComboItem( m_cbnCombo3, m_eCombo3Selection );
			setUsedParameter( m_eCombo3Selection, kParameter3 );

			// Create the curve matrix entry associated with the parameter selections
			CurveMatrixEntry curveEntry = m_pDjinni->createCurveMatrixEntry( 
				m_eCombo1Selection, m_eCombo2Selection, m_eCombo3Selection );

			// Check conditions for enabling buttons
			m_bCurveEnable = m_pDjinni->isToggleCurveDirectionEnabled( curveEntry );
			m_bAngleEnable = m_pDjinni->isToggleCurveDeltaAngleEnabled( curveEntry );
		}

		// Check to see if OK button should be hidden and 
		// Cancel changed to Close
		// Also disable combos, edits and radios
		if (m_bHideOK)
		{
			// Hide the OK button
			CButton*	pButton = (CButton *)GetDlgItem( IDOK );
			if (pButton != NULL)
			{
				pButton->ShowWindow( SW_HIDE );
			}

			// Change text in Cancel button
			pButton = (CButton *)GetDlgItem( IDCANCEL );
			if (pButton != NULL)
			{
				pButton->SetWindowText( "Close" );
			}

			// Disable combos, edits, and radios
			m_cbnCombo1.EnableWindow( FALSE );
			m_edit1.EnableWindow( FALSE );
			m_cbnCombo2.EnableWindow( FALSE );
			m_edit2.EnableWindow( FALSE );
			m_cbnCombo3.EnableWindow( FALSE );
			m_edit3.EnableWindow( FALSE );

			m_bCurveEnable = false;
			m_bAngleEnable = false;
			enableRadios();
		}

		// Disable the radio buttons and group boxes - based on constructor settings
		enableRadios();

		UpdateData(FALSE);
	}
	CATCH_UCLID_EXCEPTION("ELI01917")
	CATCH_UNEXPECTED_EXCEPTION( "ELI01353" )

	return TRUE;  // return TRUE  unless you set the focus to a control
}

int CCurveCalculatorDlg::DoModal() 
{
	TemporaryResourceOverride temp(gModuleResource);

	return CDialog::DoModal();
}

void CCurveCalculatorDlg::OnSysCommand(UINT iID, LPARAM lParam)
{
	if ((iID & 0xFFF0) == IDM_ABOUTBOX)
	{
		CAboutDlg dlgAbout;
		dlgAbout.DoModal();
	}
	else
	{
		CDialog::OnSysCommand(iID, lParam);
	}
}

// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.

void CCurveCalculatorDlg::OnPaint() 
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
HCURSOR CCurveCalculatorDlg::OnQueryDragIcon()
{
	return (HCURSOR) m_hIcon;
}

//=============================================================================
// PURPOSE: Updates dialog display after a selection is made in the first 
//				combo box.  Populates the second combo box and enables combo 
//				box and edit box.  Disables curve and angle radios and 
//				Calculate button.  Also clears the first edit box.
// REQUIRE: Called automatically via MessageMap.
// PROMISE: None.
// ARGS:	None.
//=============================================================================
void CCurveCalculatorDlg::OnSelchangeCombo1() 
{
	try
	{
		// Determine and store enumeration of first combo selection
		int	iIndex = 0;
		iIndex = m_cbnCombo1.GetCurSel();
		m_eCombo1Selection = static_cast<ECurveParameterType>
			(m_cbnCombo1.GetItemData( iIndex ));

		// Store choice in PCB collection
		clearUsedParameter( kParameter1 );
		setUsedParameter( m_eCombo1Selection, kParameter1 );

		// Clear other enumerations
		clearUsedParameter( kParameter2 );
		clearUsedParameter( kParameter3 );

		// Clear and enable the edit box
		m_edit1.SetWindowText( _T("") );
		m_edit1.EnableWindow( TRUE );

		// Populate second combo box with acceptable items
		resetDropDownList( m_cbnCombo2, 1 );

		// Enable second combo box but not edit box
		m_cbnCombo2.EnableWindow( TRUE );
		m_edit2.SetWindowText( _T("") );
		m_edit2.EnableWindow( FALSE );

		// Clear and disable the third combo and edit boxes
		m_cbnCombo3.ResetContent();
		m_cbnCombo3.EnableWindow( FALSE );
		m_edit3.SetWindowText( _T("") );
		m_edit3.EnableWindow( FALSE );

		// Disable the radio buttons
		m_bCurveEnable = false;
		m_bAngleEnable = false;
		enableRadios();

		// Disable the Calculate button
		checkParametersDefined();
	}
	CATCH_UCLID_EXCEPTION("ELI01918")
	CATCH_UNEXPECTED_EXCEPTION( "ELI01354" )
}

//=============================================================================
// PURPOSE: Updates dialog display after a selection is made in the second 
//				combo box.  Populates the third combo box and enables combo 
//				box and edit box.  Disables curve and angle radios and 
//				Calculate button.  Also clears the second edit box.
// REQUIRE: Called automatically via MessageMap.
// PROMISE: None.
// ARGS:	None.
//=============================================================================
void CCurveCalculatorDlg::OnSelchangeCombo2() 
{
	try
	{
		// Determine and store enumeration of second combo selection
		int	iIndex = 0;
		iIndex = m_cbnCombo2.GetCurSel();
		m_eCombo2Selection = static_cast<ECurveParameterType>
			(m_cbnCombo2.GetItemData( iIndex ));

		// Store choice in PCB collection
		clearUsedParameter( kParameter2 );
		setUsedParameter( m_eCombo2Selection, kParameter2 );

		// Clear other enumeration
		clearUsedParameter( kParameter3 );

		// Clear and enable the edit box
		m_edit2.SetWindowText( _T("") );
		m_edit2.EnableWindow( TRUE );

		// Populate third combo box with acceptable items
		resetDropDownList( m_cbnCombo3, 2 );

		// Enable third combo box but not edit box
		m_cbnCombo3.EnableWindow( TRUE );
		m_edit3.SetWindowText( _T("") );
		m_edit3.EnableWindow( FALSE );

		// Disable the radio buttons
		m_bCurveEnable = false;
		m_bAngleEnable = false;
		enableRadios();

		// Disable the Calculate button
		checkParametersDefined();
	}
	CATCH_UCLID_EXCEPTION("ELI01919")
	CATCH_UNEXPECTED_EXCEPTION( "ELI01355" )
}

//=============================================================================
// PURPOSE: Updates dialog display after a selection is made in the third 
//				combo box.  Enables curve and angle radio buttons if needed.
//				Enables or disables Calculate button.  Also clears the third 
//				edit box.
// REQUIRE: Called automatically via MessageMap.
// PROMISE: None.
// ARGS:	None.
//=============================================================================
void CCurveCalculatorDlg::OnSelchangeCombo3() 
{
	try
	{
		// Determine and store enumeration of third combo selection
		int	iIndex = 0;
		iIndex = m_cbnCombo3.GetCurSel();
		m_eCombo3Selection = static_cast<ECurveParameterType>
			(m_cbnCombo3.GetItemData( iIndex ));

		// Store choice in PCB collection
		clearUsedParameter( kParameter3 );
		setUsedParameter( m_eCombo3Selection, kParameter3 );

		// Clear and enable the edit box
		m_edit3.SetWindowText( _T("") );
		m_edit3.EnableWindow( TRUE );

		///////////////////////////////////////////////
		// Determine if radio buttons should be enabled
		///////////////////////////////////////////////

		// Create the curve matrix entry associated with the parameter selections
		CurveMatrixEntry curveEntry = m_pDjinni->createCurveMatrixEntry( 
			m_eCombo1Selection, m_eCombo2Selection, m_eCombo3Selection );

		// Check conditions for enabling buttons
		m_bCurveEnable = m_pDjinni->isToggleCurveDirectionEnabled( curveEntry );
		m_bAngleEnable = m_pDjinni->isToggleCurveDeltaAngleEnabled( curveEntry );

		// Update dialog
		enableRadios();

		// Enable or disable the Calculate button
		checkParametersDefined();
	}
	CATCH_UCLID_EXCEPTION("ELI01920")
	CATCH_UNEXPECTED_EXCEPTION( "ELI01356" )
}

//=============================================================================
// PURPOSE: Cleans up internal created objects before dialog window is 
//				destroyed.
// REQUIRE: Called automatically via MessageMap.
// PROMISE: Will delete Curve Calculation Engine and Curve Djinni objects 
//				before performing base class cleanup.  Also frees the 
//				UCLID Exception Viewer.
// ARGS:	None.
//=============================================================================
BOOL CCurveCalculatorDlg::DestroyWindow() 
{
	try
	{
		// Clean up engine object
		if (m_pEngine)
		{
			delete m_pEngine;
			m_pEngine = NULL;
		}
		
		// Clean up djinni object
		if (m_pDjinni)
		{
			delete m_pDjinni;
			m_pDjinni = NULL;
		}
	}
	CATCH_UCLID_EXCEPTION("ELI01921")
	CATCH_UNEXPECTED_EXCEPTION( "ELI01357" )

	return CDialog::DestroyWindow();
}

//=============================================================================
// PURPOSE: Provides user's parameter choices and values to engine.  Calculates 
//				remaining curve parameters and displays parameters in output 
//				edit box.
// REQUIRE: Called automatically via MessageMap.
// PROMISE: None.
// ARGS:	None.
//=============================================================================
void CCurveCalculatorDlg::OnCalculate() 
{
	// Tracks progress through this method for display
	// if an exception is thrown
	int		iLastGoodState = 0;

	CEdit*	pEdit = NULL;
	CString	zComplete;		

	try
	{
		// since this might take for a while to calculate a curve
		// put an hourglass here
		CWaitCursor wait;

		// Reset engine to clear any existing parameters
		m_pEngine->reset();

		// clean the contents of the output window
		m_cstrOutput.Empty();
		UpdateData(FALSE);

		// Provide distance units to Filters DLL
		DistanceCore distanceObject;

		EDistanceUnitType eDefaultUnitType = distanceObject.getDefaultDistanceUnit();

		// only set default unit if it's not set yet
		if (!m_bHideUnits && eDefaultUnitType == kUnknownUnit)
		{
			if (m_nDefaultUnits == 0)
			{
				distanceObject.setDefaultDistanceUnit(kFeet);
			}
			else if (m_nDefaultUnits == 1)
			{
				distanceObject.setDefaultDistanceUnit(kMeters);
			}
		}

		///////////////////////////
		// Provide values to engine
		///////////////////////////

		// First parameter
		if (sendParameterValue( m_eCombo1Selection, IDC_EDIT1 ))
		{
			// Increment counter
			iLastGoodState++;
		}
		else
		{
			// Focus is already in the edit box
			return;
		}

		// Second parameter
		if (sendParameterValue( m_eCombo2Selection, IDC_EDIT2 ))
		{
			// Increment counter
			iLastGoodState++;
		}
		else
		{
			// Focus is already in the edit box
			return;
		}

		// Third parameter
		if (sendParameterValue( m_eCombo3Selection, IDC_EDIT3 ))
		{
			// Increment counter
			iLastGoodState++;
		}
		else
		{
			// Focus is already in the edit box
			return;
		}

		// Provide curve orientation to engine
		if (m_bCurveEnable)
		{
			// Check to see if curve is to the right
			if (m_nConcavity == 0)
			{
				// to the right
				setCurveEngineParameter( kArcConcaveLeft, 0.0 );
			}
			else if (m_nConcavity == 1)
			{
				// to the left
				setCurveEngineParameter( kArcConcaveLeft, 1.0 );
			}

			// Increment counter
			iLastGoodState++;
		}

		// Provide angle size to engine
		if (m_bAngleEnable)
		{
			// Check to see if angle is greater than 180 degrees
			if (m_nDeltaAngle == 1)
			{
				// delta > 180
				setCurveEngineParameter( kArcDeltaGreaterThan180Degrees, 1.0 );
			}
			else if (m_nDeltaAngle == 0)
			{
				// delta < 180
				setCurveEngineParameter( kArcDeltaGreaterThan180Degrees, 0.0 );
			}

			// Increment counter
			iLastGoodState++;
		}

		// Increment counter
		iLastGoodState++;

		// Absolute location not required - just set center point to origin
		// This or another location is required to satisfy 
		// m_pEngine->canCalculateAllParameters()
		m_pEngine->setCurvePointParameter( kArcCenter, 0.0, 0.0 );

		// Increment counter
		iLastGoodState++;

		//////////////////////////////////////
		// Modify state counter
		// Expected result: 500, 600, or 700
		// depending on radio buttons required
		//////////////////////////////////////
		iLastGoodState *= 100;

		/////////////////
		// Do computation
		/////////////////
		bool	bComputeFailed = true;
		pEdit = (CEdit *)GetDlgItem( IDC_EDIT_OUTPUT );
		if (canCalculateNeededParameters())
		{
			// Clear flag
			bComputeFailed = false;

			// Update counter
			iLastGoodState += 50;
		}
		else
		{
			// Provide failure message
			zComplete.LoadString( IDS_CANNOTCALCULATE );
			if (pEdit != NULL)
			{
				pEdit->SetWindowText( zComplete );
			}
			return;
		}

		///////////////////////////////////////
		// Populate edit box with output values
		///////////////////////////////////////
		if (pEdit != NULL)
		{
			CString	zTemp;			// text for individual curve parameter
			double	dValue = 0.0;	// dummy value

			// Check for computation failure
			if (bComputeFailed)
			{
				// Provide failure message
				zComplete.LoadString( IDS_CALCULATIONFAILED );
				pEdit->SetWindowText( zComplete );
				return;
			}

			//////////////////////////////////////////////////////////////////////
			// Step through relevant curve parameters and add lines to output text
			//////////////////////////////////////////////////////////////////////
			zComplete = _T("");

			int iIndexListBox = 0;
			int iIndexPCBCollection = 0;
			ParameterControlBlock pcb;
			for (PCBCollection::const_iterator it = m_vecPCB.begin(); it != m_vecPCB.end(); it++)
			{
				// Retrieve specific curve parameter
				pcb = *it;

				// Add line to output string
				zComplete += getOutputString( pcb.eCurveParameterID ).c_str();

				// Update state counter
				iLastGoodState++;

				// Advance to the next item in the collection
				++iIndexPCBCollection;
			}

			// Update the edit box
			pEdit->SetWindowText( zComplete );

			// Enable the OK button
			CButton*	pButton = NULL;
			pButton = (CButton *)GetDlgItem( IDOK );
			if (pButton != NULL)
			{
				pButton->EnableWindow( TRUE );
			}
		}
	}
	CATCH_UCLID_EXCEPTION("ELI01308")
	CATCH_UNEXPECTED_EXCEPTION("ELI01309")
}

//=============================================================================
// PURPOSE: Checks to see if parameter definition is complete after a change
//				is noted in the first edit box.  The Calculate button is 
//				enabled or disabled as appropriate.
// REQUIRE: Called automatically via MessageMap.
// PROMISE: None.
// ARGS:	None.
//=============================================================================
void CCurveCalculatorDlg::OnChangeEdit1() 
{
	try
	{
		// Check for complete parameter definition
		checkParametersDefined();
	}
	CATCH_UCLID_EXCEPTION("ELI01922")
	CATCH_UNEXPECTED_EXCEPTION( "ELI01358" )
}

//=============================================================================
// PURPOSE: Checks to see if parameter definition is complete after a change
//				is noted in the second edit box.  The Calculate button is 
//				enabled or disabled as appropriate.
// REQUIRE: Called automatically via MessageMap.
// PROMISE: None.
// ARGS:	None.
//=============================================================================
void CCurveCalculatorDlg::OnChangeEdit2() 
{
	try
	{
		// Check for complete parameter definition
		checkParametersDefined();
	}
	CATCH_UCLID_EXCEPTION("ELI01923")
	CATCH_UNEXPECTED_EXCEPTION( "ELI01359" )
}

//=============================================================================
// PURPOSE: Checks to see if parameter definition is complete after a change
//				is noted in the third edit box.  The Calculate button is 
//				enabled or disabled as appropriate.
// REQUIRE: Called automatically via MessageMap.
// PROMISE: None.
// ARGS:	None.
//=============================================================================
void CCurveCalculatorDlg::OnChangeEdit3() 
{
	try
	{
		// Check for complete parameter definition
		checkParametersDefined();
	}
	CATCH_UCLID_EXCEPTION("ELI01924")
	CATCH_UNEXPECTED_EXCEPTION( "ELI01360" )
}

//=============================================================================
// PURPOSE: Checks to see if parameter definition is complete after the curve 
//				left radio button is selected.  The Calculate button is 
//				enabled or disabled as appropriate.
// REQUIRE: Called automatically via MessageMap.
// PROMISE: None.
// ARGS:	None.
//=============================================================================
void CCurveCalculatorDlg::OnRadioLeft() 
{
	try
	{
		// Save new setting
		UpdateData(TRUE);

		// Check for complete parameter definition
		checkParametersDefined();
	}
	CATCH_UCLID_EXCEPTION("ELI01925")
	CATCH_UNEXPECTED_EXCEPTION( "ELI01361" )
}

//=============================================================================
// PURPOSE: Checks to see if parameter definition is complete after the curve 
//				right radio button is selected.  The Calculate button is 
//				enabled or disabled as appropriate.
// REQUIRE: Called automatically via MessageMap.
// PROMISE: None.
// ARGS:	None.
//=============================================================================
void CCurveCalculatorDlg::OnRadioRight() 
{
	try
	{
		// Save new setting
		UpdateData(TRUE);

		// Check for complete parameter definition
		checkParametersDefined();
	}
	CATCH_UCLID_EXCEPTION("ELI01926")
	CATCH_UNEXPECTED_EXCEPTION( "ELI01362" )
}

//=============================================================================
// PURPOSE: Checks to see if parameter definition is complete after the angle 
//				less than 180 degrees radio button is selected.  The Calculate 
//				button is enabled or disabled as appropriate.
// REQUIRE: Called automatically via MessageMap.
// PROMISE: None.
// ARGS:	None.
//=============================================================================
void CCurveCalculatorDlg::OnRadioLesser() 
{
	try
	{
		// Save new setting
		UpdateData(TRUE);

		// Check for complete parameter definition
		checkParametersDefined();
	}
	CATCH_UCLID_EXCEPTION("ELI01927")
	CATCH_UNEXPECTED_EXCEPTION( "ELI01363" )
}

//=============================================================================
// PURPOSE: Checks to see if parameter definition is complete after the angle 
//				greater than 180 degrees radio button is selected.  The 
//				Calculate button is enabled or disabled as appropriate.
// REQUIRE: Called automatically via MessageMap.
// PROMISE: None.
// ARGS:	None.
//=============================================================================
void CCurveCalculatorDlg::OnRadioGreater() 
{
	try
	{
		// Save new setting
		UpdateData(TRUE);

		// Check for complete parameter definition
		checkParametersDefined();
	}
	CATCH_UCLID_EXCEPTION("ELI01928")
	CATCH_UNEXPECTED_EXCEPTION( "ELI01364" )
}

//=============================================================================
// PURPOSE: Stores the updated units selection.
// REQUIRE: Called automatically via MessageMap.
// PROMISE: None.
// ARGS:	None.
//=============================================================================
void CCurveCalculatorDlg::OnRadioFeet() 
{
	// Save new setting
	UpdateData(TRUE);
}

//=============================================================================
// PURPOSE: Stores the updated units selection.
// REQUIRE: Called automatically via MessageMap.
// PROMISE: None.
// ARGS:	None.
//=============================================================================
void CCurveCalculatorDlg::OnRadioMeters() 
{
	// Save new setting
	UpdateData(TRUE);
}

//=============================================================================
// PURPOSE: Saves user's input strings for review after dialog exit.
// REQUIRE: Called automatically via MessageMap.
// PROMISE: None.
// ARGS:	None.
//=============================================================================
void CCurveCalculatorDlg::OnOK() 
{
	char	pszBuffer[100];

	UpdateData( TRUE );

	// Retrieve and store strings from edit boxes
	m_edit1.GetWindowText( pszBuffer, 100 );
	m_strEdit1 = pszBuffer;

	m_edit2.GetWindowText( pszBuffer, 100 );
	m_strEdit2 = pszBuffer;

	m_edit3.GetWindowText( pszBuffer, 100 );
	m_strEdit3 = pszBuffer;

	// Call base class method to exit dialog
	CDialog::OnOK();
}

///////////////////////////////////////////
// General dialog setup and support methods
///////////////////////////////////////////

void CCurveCalculatorDlg::resetCombos(void) 
{
	try
	{
		///////////////////////////////
		// Populate the first combo box
		///////////////////////////////
		resetDropDownList( m_cbnCombo1, 0 );

		// First edit
		m_edit1.SetWindowText( _T("") );
		m_edit1.EnableWindow( FALSE );

		///////////////////////////////////////////
		// Disable the second and third combo boxes
		// and associated edit boxes
		///////////////////////////////////////////

		// Second combo
		m_cbnCombo2.ResetContent();
		m_cbnCombo2.EnableWindow( FALSE );

		// Second edit
		m_edit2.SetWindowText( _T("") );
		m_edit2.EnableWindow( FALSE );

		// Third combo
		m_cbnCombo3.ResetContent();
		m_cbnCombo3.EnableWindow( FALSE );

		// Third edit
		m_edit3.SetWindowText( _T("") );
		m_edit3.EnableWindow( FALSE );
	}
	CATCH_UCLID_EXCEPTION("ELI01929")
	CATCH_UNEXPECTED_EXCEPTION( "ELI01365" )
}

void CCurveCalculatorDlg::enableRadios(void) 
{
	try
	{
		/////////////////////////////////////
		// Handle the curve orientation group
		/////////////////////////////////////
		CButton*	pButton = NULL;
		pButton = (CButton *)GetDlgItem( IDC_RADIO_LEFT );
		if (pButton != NULL)
		{
			if (!m_bCurveEnable)
			{
				pButton->EnableWindow( FALSE );
			}
			else
			{
				pButton->EnableWindow( TRUE );
			}
		}

		pButton = (CButton *)GetDlgItem( IDC_RADIO_RIGHT );
		if (pButton != NULL)
		{
			if (!m_bCurveEnable)
			{
				pButton->EnableWindow( FALSE );
			}
			else
			{
				pButton->EnableWindow( TRUE );
			}
		}

		CStatic*	pStatic = NULL;
		pStatic = (CStatic *)GetDlgItem( IDC_STATIC_CURVE );
		if (pStatic != NULL)
		{
			if (!m_bCurveEnable)
			{
				pStatic->EnableWindow( FALSE );
			}
			else
			{
				pStatic->EnableWindow( TRUE );
			}
		}

		///////////////////////////////
		// Handle the delta angle group
		///////////////////////////////
		pButton = (CButton *)GetDlgItem( IDC_RADIO_LESSER );
		if (pButton != NULL)
		{
			if (!m_bAngleEnable)
			{
				pButton->EnableWindow( FALSE );
			}
			else
			{
				pButton->EnableWindow( TRUE );
			}
		}

		pButton = (CButton *)GetDlgItem( IDC_RADIO_GREATER );
		if (pButton != NULL)
		{
			if (!m_bAngleEnable)
			{
				pButton->EnableWindow( FALSE );
			}
			else
			{
				pButton->EnableWindow( TRUE );
			}
		}

		pStatic = (CStatic *)GetDlgItem( IDC_STATIC_ANGLE );
		if (pStatic != NULL)
		{
			if (!m_bAngleEnable)
			{
				pStatic->EnableWindow( FALSE );
			}
			else
			{
				pStatic->EnableWindow( TRUE );
			}
		}

		/////////////////////////
		// Handle the units group
		/////////////////////////
		pButton = (CButton *)GetDlgItem( IDC_RADIO_FEET );
		if (pButton != NULL)
		{
			if (m_nDefaultUnits == 0)
			{
				// Checked
				pButton->SetCheck( 1 );
			}
			else
			{
				// Unchecked
				pButton->SetCheck( 0 );
			}

			if (m_bHideUnits)
			{
				pButton->ShowWindow( SW_HIDE );
			}
			else
			{
				pButton->ShowWindow( SW_SHOW );
			}
		}

		pButton = (CButton *)GetDlgItem( IDC_RADIO_METERS );
		if (pButton != NULL)
		{
			if (m_nDefaultUnits == 1)
			{
				// Checked
				pButton->SetCheck( 1 );
			}
			else
			{
				// Unchecked
				pButton->SetCheck( 0 );
			}

			if (m_bHideUnits)
			{
				pButton->ShowWindow( SW_HIDE );
			}
			else
			{
				pButton->ShowWindow( SW_SHOW );
			}
		}

		pStatic = (CStatic *)GetDlgItem( IDC_STATIC_UNITS );
		if (pStatic != NULL)
		{
			if (m_bHideUnits)
			{
				pStatic->ShowWindow( SW_HIDE );
			}
			else
			{
				pStatic->ShowWindow( SW_SHOW );
			}
		}
	}
	CATCH_UCLID_EXCEPTION("ELI01930")
	CATCH_UNEXPECTED_EXCEPTION( "ELI01366" )
}

void CCurveCalculatorDlg::checkParametersDefined(void) 
{
	try
	{
		////////////////////////////////////////////////
		// Check that all three edit boxes are non-empty
		////////////////////////////////////////////////
		bool	bAllOkay = true;

		// First edit
		int		iLength = 0;
		iLength = m_edit1.GetWindowTextLength();
		if (iLength == 0)
		{
			bAllOkay = false;
		}

		// Second edit
		iLength = m_edit2.GetWindowTextLength();
		if (iLength == 0)
		{
			bAllOkay = false;
		}

		// Third edit
		iLength = m_edit3.GetWindowTextLength();
		if (iLength == 0)
		{
			bAllOkay = false;
		}

		CButton*	pButton = NULL;

		/////////////////////////////////////////
		// Enable or disable the Calculate button
		/////////////////////////////////////////
		pButton = (CButton *)GetDlgItem( ID_CALCULATE );
		if (pButton != NULL)
		{
			// All three edit boxes must be non-empty
			// and required radio groups must have selections
			if (bAllOkay)
			{
				// Enable the Calculate button
				pButton->EnableWindow( TRUE );
			}
			else
			{
				// Disable the Calculate button
				pButton->EnableWindow( FALSE );
			}
		}

		///////////////////////////////
		// Always disable the OK button
		///////////////////////////////
		pButton = (CButton *)GetDlgItem( IDOK );
		if (pButton != NULL)
		{
			pButton->EnableWindow( FALSE );
		}
	}
	CATCH_UCLID_EXCEPTION("ELI01931")
	CATCH_UNEXPECTED_EXCEPTION( "ELI01367" )
}

void CCurveCalculatorDlg::setCurveEngineParameter(ECurveParameterType eParam, 
												  double dValue)
{
	try
	{
		// Provide value to engine
		switch (eParam)
		{
		// center angle for the circular arc
		case kArcDelta:
		// angle from ArcCenter to ArcStartingPoint
		case kArcStartAngle:
		// angle from ArcCenter to ArcEndingpoint
		case kArcEndAngle:
		// using this definition: radius = 5729.6506/DegreeOfCurve
		case kArcDegreeOfCurveChordDef:
		// using this definition: radius = 5729.5780/DegreeOfCurve
		case kArcDegreeOfCurveArcDef:
		// in-tangent for the arc: touching arc at ArcStartingPoint
		case kArcTangentInBearing:
		// out-tangent for the arc: touching arc at ArcEndingPoint
		case kArcTangentOutBearing:
		// bearing from ArcStartingPoint to ArcEndingPoint
		case kArcChordBearing:
		// bearing from ArcStartingPoint to ArcCenter
		case kArcRadialInBearing:
		// bearing from ArcCenter to ArcEndingPoint
		case kArcRadialOutBearing:
			m_pEngine->setCurveAngleOrBearingParameter( eParam, dValue );
			break;

		// radius of the circle associated with the arc
		case kArcRadius:
		// length of the circular arc
		case kArcLength:
		// distance from ArcStartingPoint to ArcEndingPoint
		case kArcChordLength:
		// distance from ArcExternalPoint to ArcMidPoint
		case kArcExternalDistance:
		// distance from ArcChordMidPoint to ArcMidPoint
		case kArcMiddleOrdinate:
		// distance from StartingPoint to ArcExternalPoint
		case kArcTangentDistance:
			m_pEngine->setCurveDistanceParameter( eParam, dValue );
			break;

		case kArcConcaveLeft:
		case kArcDeltaGreaterThan180Degrees:
			m_pEngine->setCurveBooleanParameter( eParam, (dValue == 0.0) ? false : true );
			break;

		// NOTE: Do not include kArcStartingPoint, kArcMidPoint, 
		//       kArcEndingPoint, kArcCenter, kArcExternalPoint, 
		//       kArcChordMidPoint
		default:
			// This is an unexpected parameter
			break;
		}
	}
	CATCH_UCLID_EXCEPTION("ELI01932")
	CATCH_UNEXPECTED_EXCEPTION( "ELI01368" )
}

bool CCurveCalculatorDlg::getParsedParameterValue(ECurveParameterType eParam, 
												  CString zStr,
												  double* pdValue)
{
	try
	{
		// Determine string length
		int iLength = zStr.GetLength();

		// Extract value based on type
		switch (eParam)
		{
		///////////////////
		// Angle parameters
		///////////////////
		case kArcDelta:				// center angle for the circular arc
		case kArcStartAngle:		// angle from ArcCenter to ArcStartingPoint
		case kArcEndAngle:			// angle from ArcCenter to ArcEndingpoint
		case kArcDegreeOfCurveChordDef:	// using this definition: radius = 5729.6506/DegreeOfCurve
		case kArcDegreeOfCurveArcDef:	// using this definition: radius = 5729.5780/DegreeOfCurve
			{
				// Create a Angle object using the provided string
				Angle angleObject;
				angleObject.evaluate(zStr);

				// Check success of parse operation
				if (angleObject.isValid())
				{
					// Get value in radians
					*pdValue = angleObject.getRadians();

					return true;
				}
				else
				{
					// Provide ordinary error value
					*pdValue = -1.0;
					return false;
				}
			}
			break;

		/////////////////////
		// Bearing parameters
		/////////////////////
		case kArcTangentInBearing:		// in-tangent for the arc: touching arc at ArcStartingPoint
		case kArcTangentOutBearing:		// out-tangent for the arc: touching arc at ArcEndingPoint
		case kArcChordBearing:			// bearing from ArcStartingPoint to ArcEndingPoint
		case kArcRadialInBearing:		// bearing from ArcStartingPoint to ArcCenter
		case kArcRadialOutBearing:		// bearing from ArcCenter to ArcEndingPoint
			{
				// Create a Direction object using the provided string
				DirectionHelper direction;
				direction.evaluateDirection((LPCTSTR)zStr);

				// Check success of parse operation
				if (direction.isDirectionValid())
				{
					// Get value in radians
					*pdValue = direction.getPolarAngleRadians();

					return true;
				}
				else
				{
					// Provide ordinary error value
					*pdValue = -1.0;
					return false;
				}
			}
			break;

		//////////////////////
		// Distance parameters
		//////////////////////
		case kArcRadius:			// radius of the circle associated with the arc
		case kArcLength:			// length of the circular arc
		case kArcChordLength:		// distance from ArcStartingPoint to ArcEndingPoint
		case kArcExternalDistance:	// distance from ArcExternalPoint to ArcMidPoint
		case kArcMiddleOrdinate:	// distance from ArcChordMidPoint to ArcMidPoint
		case kArcTangentDistance:	// distance from StartingPoint to ArcExternalPoint
			{
				// Create a Distance object using the string
				// NOTE: isValid() returns false if string is passed in the ctor
				DistanceCore distanceObject;
				distanceObject.evaluate( (string)zStr );

				// Check success of parse operation
				if (distanceObject.isValid())
				{
					EDistanceUnitType eUnit = distanceObject.getCurrentDistanceUnit();
					if (eUnit == kUnknownUnit)
					{
						eUnit = distanceObject.getDefaultDistanceUnit();
					}

					// Get value using current units
					*pdValue = distanceObject.getDistanceInUnit(eUnit);

					return true;
				}
				else
				{
					// Provide ordinary error value
					*pdValue = -1.0;
					return false;
				}
			}
			break;

		// NOTE: Do not include kArcStartingPoint, kArcMidPoint, 
		//       kArcEndingPoint, kArcCenter, kArcExternalPoint, 
		//       kArcChordMidPoint
		default:
			// This is an unexpected parameter
			// Provide ordinary error value
			*pdValue = -1.0;
			return false;
			break;
		}
	}
	CATCH_UCLID_EXCEPTION("ELI01933")
	CATCH_UNEXPECTED_EXCEPTION( "ELI01369" )
	return false;
}

bool CCurveCalculatorDlg::sendParameterValue(ECurveParameterType eParam, 
											 int iEditControlID)
{
	try
	{
		// Provide value to engine
		CString	zTemp;
		CEdit*	pEdit = NULL;
		double	dValue = 0.0;

		// Convert parameter string to value
		pEdit = (CEdit *)GetDlgItem( iEditControlID );
		if (pEdit != NULL)
		{
			// Retrieve the string from the edit box and
			// check that it has non-zero length
			pEdit->GetWindowText( zTemp );
			if (zTemp.GetLength() > 0)
			{
				// Attempt to parse the string
				if (getParsedParameterValue( eParam, zTemp, &dValue))
				{
					// Send value to engine
					setCurveEngineParameter( eParam, dValue );

					// Success
					return true;
				}
				else
				{
					// Display error indication
					pEdit = (CEdit *)GetDlgItem( IDC_EDIT_OUTPUT );
					if (pEdit != NULL)
					{
						CString	zComplete;		// entire text for edit control

						// Provide failure message
						zComplete.LoadString( IDS_BADPARAMETER );
						pEdit->SetWindowText( zComplete );
					}

					// Set focus to the bad string
					pEdit = (CEdit *)GetDlgItem( iEditControlID );
					if (pEdit != NULL)
					{
						// Select entire string
						pEdit->SetSel( 0, -1 );
						// Move to this control
						pEdit->SetFocus();
					}

					// Failure
					return false;
				}			// end else couldn't parse string
			}				// end if string has length
		}					// end if pEdit != NULL
	}
	CATCH_UCLID_EXCEPTION("ELI01934")
	CATCH_UNEXPECTED_EXCEPTION( "ELI01370" )
	return false;
}

string CCurveCalculatorDlg::describeCurveParameter(ECurveParameterType eParam)
{
	try
	{
		char	pszBuffer[100];

		switch (eParam)
		{
			// center angle for the circular arc
			case kArcDelta:
				sprintf_s( pszBuffer, sizeof(pszBuffer), "%s", _T("Delta angle") );
				break;

			// angle from ArcCenter to ArcStartingPoint
			case kArcStartAngle:
				sprintf_s( pszBuffer, sizeof(pszBuffer), "%s", _T("Start angle") );
				break;

			// angle from ArcCenter to ArcEndingpoint
			case kArcEndAngle:
				sprintf_s( pszBuffer, sizeof(pszBuffer), "%s", _T("End angle") );
				break;

			// using this definition: radius = 5729.6506/DegreeOfCurve
			case kArcDegreeOfCurveChordDef:
				sprintf_s( pszBuffer, sizeof(pszBuffer), "%s", 
					_T("Degree of curve (chord definition)") );
				break;

			// using this definition: radius = 5729.5780/DegreeOfCurve
			case kArcDegreeOfCurveArcDef:
				sprintf_s( pszBuffer, sizeof(pszBuffer), "%s", 
					_T("Degree of curve (arc definition)") );
				break;

			// in-tangent for the arc: touching arc at ArcStartingPoint
			case kArcTangentInBearing:
				sprintf_s( pszBuffer, sizeof(pszBuffer), "%s", _T("Tangent in bearing") );
				break;

			// out-tangent for the arc: touching arc at ArcEndingPoint
			case kArcTangentOutBearing:
				sprintf_s( pszBuffer, sizeof(pszBuffer), "%s", _T("Tangent out bearing") );
				break;

			// bearing from ArcStartingPoint to ArcEndingPoint
			case kArcChordBearing:
				sprintf_s( pszBuffer, sizeof(pszBuffer), "%s", _T("Chord bearing") );
				break;

			// bearing from ArcStartingPoint to ArcCenter
			case kArcRadialInBearing:
				sprintf_s( pszBuffer, sizeof(pszBuffer), "%s", _T("Radial in bearing") );
				break;

			// bearing from ArcCenter to ArcEndingPoint
			case kArcRadialOutBearing:
				sprintf_s( pszBuffer, sizeof(pszBuffer), "%s", _T("Radial out bearing") );
				break;

			// radius of the circle associated with the arc
			case kArcRadius:
				sprintf_s( pszBuffer, sizeof(pszBuffer), "%s", _T("Radius") );
				break;

			// length of the circular arc
			case kArcLength:
				sprintf_s( pszBuffer, sizeof(pszBuffer), "%s", _T("Arc length") );
				break;

			// distance from ArcStartingPoint to ArcEndingPoint
			case kArcChordLength:
				sprintf_s( pszBuffer, sizeof(pszBuffer), "%s", _T("Chord length") );
				break;

			// distance from ArcExternalPoint to ArcMidPoint
			case kArcExternalDistance:
				sprintf_s( pszBuffer, sizeof(pszBuffer), "%s", _T("Distance to external point") );
				break;

			// distance from ArcChordMidPoint to ArcMidPoint
			case kArcMiddleOrdinate:
				sprintf_s( pszBuffer, sizeof(pszBuffer), "%s", _T("Distance to mid point") );
				break;

			// distance from StartingPoint to ArcExternalPoint
			case kArcTangentDistance:
				sprintf_s( pszBuffer, sizeof(pszBuffer), "%s", _T("Tangent distance") );
				break;

			// NOTE: Do not include: 
			//			kArcStartingPoint, kArcMidPoint, 
			//			kArcEndingPoint, kArcCenter, kArcExternalPoint, 
			//			kArcChordMidPoint, kArcConcaveLeft, 
			//			kArcDeltaGreaterThan180Degrees
			default:
				// This is an unexpected parameter
				sprintf_s( pszBuffer, sizeof(pszBuffer), "%s", _T("Unexpected parameter") );
				break;
		}

		return string( pszBuffer );
	}
	CATCH_UCLID_EXCEPTION("ELI01935")
	CATCH_UNEXPECTED_EXCEPTION( "ELI01371" )
	return string( "" );
}

void CCurveCalculatorDlg::createParameterControlBlock(void)
{
	try
	{
		ParameterControlBlock pcb;

		// center angle for the circular arc
		pcb.eCurveParameterID = kArcDelta;
		pcb.strParameterDescription = describeCurveParameter(pcb.eCurveParameterID);
		pcb.eUsed = kUnused;
		m_vecPCB.push_back(pcb);

		// angle from ArcCenter to ArcStartingPoint
		pcb.eCurveParameterID = kArcStartAngle;
		pcb.strParameterDescription = describeCurveParameter(pcb.eCurveParameterID);
		pcb.eUsed = kUnused;
		m_vecPCB.push_back(pcb);

		// angle from ArcCenter to ArcEndingpoint
		pcb.eCurveParameterID = kArcEndAngle;
		pcb.strParameterDescription = describeCurveParameter(pcb.eCurveParameterID);
		pcb.eUsed = kUnused;
		m_vecPCB.push_back(pcb);

		// using this definition: radius = 5729.6506/DegreeOfCurve
		pcb.eCurveParameterID = kArcDegreeOfCurveChordDef;
		pcb.strParameterDescription = describeCurveParameter(pcb.eCurveParameterID);
		pcb.eUsed = kUnused;
		m_vecPCB.push_back(pcb);

		// using this definition: radius = 5729.5780/DegreeOfCurve
		pcb.eCurveParameterID = kArcDegreeOfCurveArcDef;
		pcb.strParameterDescription = describeCurveParameter(pcb.eCurveParameterID);
		pcb.eUsed = kUnused;
		m_vecPCB.push_back(pcb);

		// in-tangent for the arc: touching arc at ArcStartingPoint
		pcb.eCurveParameterID = kArcTangentInBearing;
		pcb.strParameterDescription = describeCurveParameter(pcb.eCurveParameterID);
		pcb.eUsed = kUnused;
		m_vecPCB.push_back(pcb);

		// out-tangent for the arc: touching arc at ArcEndingPoint
		pcb.eCurveParameterID = kArcTangentOutBearing;
		pcb.strParameterDescription = describeCurveParameter(pcb.eCurveParameterID);
		pcb.eUsed = kUnused;
		m_vecPCB.push_back(pcb);

		// bearing from ArcStartingPoint to ArcEndingPoint
		pcb.eCurveParameterID = kArcChordBearing;
		pcb.strParameterDescription = describeCurveParameter(pcb.eCurveParameterID);
		pcb.eUsed = kUnused;
		m_vecPCB.push_back(pcb);

		// bearing from ArcStartingPoint to ArcCenter
		pcb.eCurveParameterID = kArcRadialInBearing;
		pcb.strParameterDescription = describeCurveParameter(pcb.eCurveParameterID);
		pcb.eUsed = kUnused;
		m_vecPCB.push_back(pcb);

		// bearing from ArcCenter to ArcEndingPoint
		pcb.eCurveParameterID = kArcRadialOutBearing;
		pcb.strParameterDescription = describeCurveParameter(pcb.eCurveParameterID);
		pcb.eUsed = kUnused;
		m_vecPCB.push_back(pcb);

		// radius of the circle associated with the arc
		pcb.eCurveParameterID = kArcRadius;
		pcb.strParameterDescription = describeCurveParameter(pcb.eCurveParameterID);
		pcb.eUsed = kUnused;
		m_vecPCB.push_back(pcb);

		// length of the circular arc
		pcb.eCurveParameterID = kArcLength;
		pcb.strParameterDescription = describeCurveParameter(pcb.eCurveParameterID);
		pcb.eUsed = kUnused;
		m_vecPCB.push_back(pcb);

		// distance from ArcStartingPoint to ArcEndingPoint
		pcb.eCurveParameterID = kArcChordLength;
		pcb.strParameterDescription = describeCurveParameter(pcb.eCurveParameterID);
		pcb.eUsed = kUnused;
		m_vecPCB.push_back(pcb);

		// distance from ArcExternalPoint to ArcMidPoint
		pcb.eCurveParameterID = kArcExternalDistance;
		pcb.strParameterDescription = describeCurveParameter(pcb.eCurveParameterID);
		pcb.eUsed = kUnused;
		m_vecPCB.push_back(pcb);

		// distance from ArcChordMidPoint to ArcMidPoint
		pcb.eCurveParameterID = kArcMiddleOrdinate;
		pcb.strParameterDescription = describeCurveParameter(pcb.eCurveParameterID);
		pcb.eUsed = kUnused;
		m_vecPCB.push_back(pcb);

		// distance from StartingPoint to ArcExternalPoint
		pcb.eCurveParameterID = kArcTangentDistance;
		pcb.strParameterDescription = describeCurveParameter(pcb.eCurveParameterID);
		pcb.eUsed = kUnused;
		m_vecPCB.push_back(pcb);

		// NOTE: Do not include: 
		//			kArcStartingPoint, kArcMidPoint, 
		//			kArcEndingPoint, kArcCenter, kArcExternalPoint, 
		//			kArcChordMidPoint, kArcConcaveLeft, 
		//			kArcDeltaGreaterThan180Degrees
	}
	CATCH_UCLID_EXCEPTION("ELI01936")
	CATCH_UNEXPECTED_EXCEPTION( "ELI01372" )
}

void CCurveCalculatorDlg::resetDropDownList(CComboBox& rComboBox, int iNumSelectionsMade)
{
	// Check argument
	ASSERT_ARGUMENT( "ELI19475", (iNumSelectionsMade >= 0) );
	ASSERT_ARGUMENT( "ELI19476", (iNumSelectionsMade < 3) );

	try
	{
		// Clear the combo box
		rComboBox.ResetContent();

		// Retrieve the basic curve matrix from djinni
		CurveMatrix	curveMatrix = m_pDjinni->getCurveMatrix();

		// Filter the curve matrix as needed
		if (iNumSelectionsMade >= 1)
		{
			// Filter against the first selection
			curveMatrix	= m_pDjinni->filterCurveMatrix(	m_eCombo1Selection, curveMatrix );

			if (iNumSelectionsMade == 2)
			{
				// Further filter against the second selection
				curveMatrix	= m_pDjinni->filterCurveMatrix(	m_eCombo2Selection, curveMatrix );
			}
		}

		// Step through collected curve parameters and examine the curve
		// matrix.  Only display included items that are still unused
		int iIndexListBox = 0;
		int iIndexPCBCollection = 0;
		ParameterControlBlock pcb;
		for (PCBCollection::const_iterator it = m_vecPCB.begin(); it != m_vecPCB.end(); it++)
		{
			// Retrieve specific curve parameter
			pcb = *it;

			// Only check curve matrix if parameter has not been used
			if ((pcb.eUsed == kUnused) &&
				(m_pDjinni->doesParameterExistInCurveMatrix( pcb.eCurveParameterID, curveMatrix )))
			{
				// Add the item to the combo box
				iIndexListBox = rComboBox.AddString( pcb.strParameterDescription.c_str() );

				// Set item data to curve parameter ID
				rComboBox.SetItemData( iIndexListBox, pcb.eCurveParameterID );
			}

			// Advance to the next item in the collection
			++iIndexPCBCollection;
		}
	}
	CATCH_UCLID_EXCEPTION("ELI01937")
	CATCH_UNEXPECTED_EXCEPTION( "ELI01373" )
}

void CCurveCalculatorDlg::clearUsedParameter(EUsedParameterID eID)
{
	try
	{
		if (eID != kUnused)
		{
			// Step through PCB collection
			for (PCBCollection::iterator it = m_vecPCB.begin(); it != m_vecPCB.end(); it++)
			{
				// Check to see if this parameter had been set to the given selection
				if ((*it).eUsed == eID)
				{
					// Clear the setting
					(*it).eUsed = kUnused;
				}
			}
		}
	}
	CATCH_UCLID_EXCEPTION("ELI01938")
	CATCH_UNEXPECTED_EXCEPTION( "ELI01472" )
}

void CCurveCalculatorDlg::setUsedParameter(ECurveParameterType eParam, EUsedParameterID eID)
{
	try
	{
		// Step through PCB collection
		for (PCBCollection::iterator it = m_vecPCB.begin(); it != m_vecPCB.end(); it++)
		{
			// Check to see if this is the desired curve parameter
			if ((*it).eCurveParameterID == eParam)
			{
				// Set the used field as specified
				(*it).eUsed = eID;
			}
		}
	}
	CATCH_UCLID_EXCEPTION("ELI01939")
	CATCH_UNEXPECTED_EXCEPTION( "ELI01471" )
}

string CCurveCalculatorDlg::getOutputString(ECurveParameterType eParam)
{
	string	strInitial( "" );
	string	strFinal( "" );
	
	// Retrieve output value from Engine
	// NOTE: Assumes that canCalculateNeededParameters()
	//       has already been called
	strInitial = m_pEngine->getCurveParameter( eParam );
	
	// Convert to user-meaningful form if appropriate
	switch (eParam)
	{
		///////////////////
		// Angle parameters
		///////////////////
	case kArcDelta:				// center angle for the circular arc
	case kArcStartAngle:		// angle from ArcCenter to ArcStartingPoint
	case kArcEndAngle:			// angle from ArcCenter to ArcEndingpoint
	case kArcDegreeOfCurveChordDef:	// using this definition: radius = 5729.6506/DegreeOfCurve
	case kArcDegreeOfCurveArcDef:	// using this definition: radius = 5729.5780/DegreeOfCurve
		{
			// Convert string to double
			double	dTemp = atof( strInitial.c_str() );
			
			// Create a Angle object using the double
			Angle angleObject;
			angleObject.evaluateRadians( dTemp );
			
			// Check success of parse operation
			if (angleObject.isValid())
			{
				// Provide output string
				strFinal = describeCurveParameter( eParam );
				strFinal += ": ";
				// display both degree-minute-second and decimal degrees
				strFinal += angleObject.asStringDMS();
				strFinal += " ( OR " + angleObject.asStringDecimalDegree() + ")";
				strFinal += "\r\n";
			}
			else
			{
				// Provide error indication
				strFinal = describeCurveParameter( eParam );
				strFinal += ": ";
				strFinal += "invalid";
				strFinal += "\r\n";
			}
		}
		break;
		
		/////////////////////
		// Bearing parameters
		/////////////////////
	case kArcTangentInBearing:		// in-tangent for the arc: touching arc at ArcStartingPoint
	case kArcTangentOutBearing:		// out-tangent for the arc: touching arc at ArcEndingPoint
	case kArcChordBearing:			// bearing from ArcStartingPoint to ArcEndingPoint
	case kArcRadialInBearing:		// bearing from ArcStartingPoint to ArcCenter
	case kArcRadialOutBearing:		// bearing from ArcCenter to ArcEndingPoint
		{
			// Convert string to double
			double	dTemp = asDouble( strInitial );
			
			// Create a DirectionHelper using the radians
			DirectionHelper direction;
			
			// Display the output direction based on the current direction type
			strFinal = describeCurveParameter( eParam );
			strFinal += ": ";
			strFinal += direction.polarAngleInRadiansToDirectionInString(dTemp);
			strFinal += "\r\n";

		}
		break;
		
		//////////////////////
		// Distance parameters
		//////////////////////
	case kArcRadius:			// radius of the circle associated with the arc
	case kArcLength:			// length of the circular arc
	case kArcChordLength:		// distance from ArcStartingPoint to ArcEndingPoint
	case kArcExternalDistance:	// distance from ArcExternalPoint to ArcMidPoint
	case kArcMiddleOrdinate:	// distance from ArcChordMidPoint to ArcMidPoint
	case kArcTangentDistance:	// distance from StartingPoint to ArcExternalPoint
		{
			// get current distance unit string
			static DistanceCore distanceCore;
			static const string strUnit(distanceCore.getStringFromUnit(distanceCore.getCurrentDistanceUnit()));
			// Conversion is not needed, just create the output string
			strFinal = describeCurveParameter( eParam );
			strFinal += ": ";
			strFinal += strInitial;
			strFinal += " " + strUnit;  // append the distance unit to the end of the distance value
			strFinal += "\r\n";
		}
		break;
		
		// NOTE: Do not include kArcStartingPoint, kArcMidPoint, 
		//       kArcEndingPoint, kArcCenter, kArcExternalPoint, 
		//       kArcChordMidPoint
	default:
		// This is an unexpected parameter
		// Provide error indication
		strFinal = "Unexpected parameter";
		strFinal += ": ";
		strFinal += "invalid";
		strFinal += "\r\n";
		break;
		}
		
	return strFinal;
}

void CCurveCalculatorDlg::selectComboItem(CComboBox& rComboBox, ECurveParameterType eParam)
{
	try
	{
		// Determine number of items in combo box
		int iCount = rComboBox.GetCount();

		// Loop through items
		for (int i = 0; i < iCount; i++)
		{
			// Examine this item's data
			if (rComboBox.GetItemData( i ) == eParam)
			{
				// Select this item and return
				rComboBox.SetCurSel( i );
				return;
			}
		}
	}
	CATCH_UCLID_EXCEPTION("ELI01941")
	CATCH_UNEXPECTED_EXCEPTION( "ELI01470" )
}

bool CCurveCalculatorDlg::isInputValid(ECurveParameterType eSelect1, 
									   LPCTSTR pszString1, 
									   ECurveParameterType eSelect2, 
									   LPCTSTR pszString2, 
									   ECurveParameterType eSelect3, 
									   LPCTSTR pszString3, 
									   int iConcavity, int iAngle)
{
	/////////////////////////////////////////////
	// Provide values to Curve Calculation Engine
	/////////////////////////////////////////////
	CString	zTemp;
	double	dValue = 0.0;
	bool	bAllValid = true;

	try
	{
		// Parse the first string
		zTemp = pszString1;
		if (!getParsedParameterValue( eSelect1, zTemp, &dValue ))
		{
			bAllValid = false;
		}
		else
		{
			// Send the string to the engine
			setCurveEngineParameter( eSelect1, dValue );
		}

		// Parse the second string
		zTemp = pszString2;
		if (!getParsedParameterValue( eSelect2, zTemp, &dValue ))
		{
			bAllValid = false;
		}
		else
		{
			// Send the string to the engine
			setCurveEngineParameter( eSelect2, dValue );
		}

		// Parse the third string
		zTemp = pszString3;
		if (!getParsedParameterValue( eSelect3, zTemp, &dValue ))
		{
			bAllValid = false;
		}
		else
		{
			// Send the string to the engine
			setCurveEngineParameter( eSelect3, dValue );
		}

		// Create the curve matrix entry associated with the parameter selections
		CurveMatrixEntry curveEntry = m_pDjinni->createCurveMatrixEntry( 
			eSelect1, eSelect2, eSelect3 );

		// Check need for curve and angle selections
		bool bCurve = m_pDjinni->isToggleCurveDirectionEnabled( curveEntry );
		bool bAngle = m_pDjinni->isToggleCurveDeltaAngleEnabled( curveEntry );

		if (bCurve)
		{
			// Check to see if curve is to the right 
			if (iConcavity == 0)
			{
				// to the right
				setCurveEngineParameter( kArcConcaveLeft, 0.0 );
			}
			else if (iConcavity == 1)
			{
				// to the left
				setCurveEngineParameter( kArcConcaveLeft, 1.0 );
			}
		}

		if (bAngle)
		{
			// Check to see if angle is greater than 180 degrees
			if (iAngle == 1)
			{
				// delta > 180
				setCurveEngineParameter( kArcDeltaGreaterThan180Degrees, 1.0 );
			}
			else if (iAngle == 0)
			{
				// delta < 100
				setCurveEngineParameter( kArcDeltaGreaterThan180Degrees, 0.0 );
			}
		}


		// Absolute location not required - just set center point to origin
		// This or another location is required to satisfy 
		// canCalculateNeededParameters()
		m_pEngine->setCurvePointParameter( kArcCenter, 0.0, 0.0 );

		/////////////////////////////////////////////////
		// Test if parameters are valid and return result
		/////////////////////////////////////////////////
		if (bAllValid)
		{
			return canCalculateNeededParameters();
		}
		else
		{
			return false;
		}
	}
	CATCH_UCLID_EXCEPTION("ELI01942")
	CATCH_UNEXPECTED_EXCEPTION( "ELI01469" )

	return false;
}

// Provides curve parameter selected in combo box #1, #2, or #3
ECurveParameterType CCurveCalculatorDlg::getParameter(int iItem)
{
	// Check argument
	ASSERT_ARGUMENT( "ELI01473", (iItem >= 1) );
	ASSERT_ARGUMENT( "ELI19477", (iItem < 4) );

	// Which combo box?
	switch (iItem)
	{
	case 1:
		return m_eCombo1Selection;
		break;

	case 2:
		return m_eCombo2Selection;
		break;

	case 3:
		return m_eCombo3Selection;
		break;
	}

	// Error condition, throw exception
	THROW_LOGIC_ERROR_EXCEPTION( "ELI19478" );
}

// Provides curve parameter value in edit box #1, #2, or #3
std::string CCurveCalculatorDlg::getParameterValue(int iItem)
{
	// Check argument
	ASSERT_ARGUMENT( "ELI01474", (iItem >= 1) );
	ASSERT_ARGUMENT( "ELI19479", (iItem < 4) );

	// Which combo box?
	switch (iItem)
	{
	case 1:
		return m_strEdit1;
		break;

	case 2:
		return m_strEdit2;
		break;

	case 3:
		return m_strEdit3;
		break;

	default:
		// Error condition, throw exception
		THROW_LOGIC_ERROR_EXCEPTION( "ELI19480" );
		break;
	}

	return( "" );
}

int CCurveCalculatorDlg::getConcavity()
{
	int iResult = -1;		// not required

	// Check to see if concavity selection was required
	if (m_bCurveEnable)
	{
		iResult = m_nConcavity;
	}

	return iResult;
}

int CCurveCalculatorDlg::getAngleSize()
{
	int iResult = -1;		// not required

	// Check to see if angle selection was required
	if (m_bAngleEnable)
	{
		iResult = m_nDeltaAngle;
	}

	return iResult;
}

bool CCurveCalculatorDlg::isUnitsFeet()
{
	// Check button
	if (m_nDefaultUnits == 0)
	{
		// Retain default of feet
		return true;
	}
	else if (m_nDefaultUnits == 1)
	{
		// Use units of meters
		return false;
	}
	else
	{
		// Default to feet
		return true;
	}
}

bool CCurveCalculatorDlg::canCalculateNeededParameters()
{
	bool	bReturn = true;

	// Check that the Engine is initialized
	if (m_pEngine == NULL)
	{
		return false;
	}

	// Check that the collection of parameters has been populated
	if (m_vecPCB.size() == 0)
	{
		createParameterControlBlock();
	}

	//////////////////////////////////////////
	// Check each parameter desired for output
	//////////////////////////////////////////
	ParameterControlBlock pcb;
	int iIndexPCBCollection = 0;
	for (PCBCollection::const_iterator it = m_vecPCB.begin(); it != m_vecPCB.end(); it++)
	{
		// Retrieve specific curve parameter
		pcb = *it;

		// Check if this parameter can be calculated
		if (!m_pEngine->canCalculateParameter( pcb.eCurveParameterID ))
		{
			// Failure condition
			bReturn = false;

			// Quit checking parameters
			break;
		}

		// Advance to the next item in the collection
		++iIndexPCBCollection;
	}

	return bReturn;
}
