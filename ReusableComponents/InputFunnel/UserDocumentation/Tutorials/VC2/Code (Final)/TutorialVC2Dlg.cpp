// TutorialVC2Dlg.cpp : implementation file
//

#include "stdafx.h"
#include "TutorialVC2.h"
#include "TutorialVC2Dlg.h"

#include <math.h>
#include <comdef.h>

#import "COMLM.dll"
using namespace UCLID_COMLMLib;

#import "UCLIDCOMUtils.dll"
using namespace UCLIDCOMUTILSLib;

#import "UCLIDMeasurements.dll"
using namespace UCLID_MEASUREMENTSLib;

#import "UCLIDRasterAndOCRMgmt.dll"
using namespace UCLID_RASTERANDOCRMGMTLib;

#import "IFCore.dll"
using namespace UCLID_INPUTFUNNELLib;

#import "InputContexts.dll"
using namespace UCLID_InputContextsLib;

#import "UCLIDDistanceConverter.dll"
using namespace UCLIDDISTANCECONVERTERLib;

#import "LandRecordsIV.dll"
using namespace UCLIDLANDRECORDSIVLib;



#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CTutorialVC2Dlg dialog

CTutorialVC2Dlg::CTutorialVC2Dlg(CWnd* pParent /*=NULL*/)
	: CDialog(CTutorialVC2Dlg::IDD, pParent), 
	m_ipException(__uuidof(COMUCLIDException))
{
	//{{AFX_DATA_INIT(CTutorialVC2Dlg)
	m_zEndPoint = _T("");
	m_zStartPoint = _T("");
	m_zBearing = _T("");
	m_zDistance = _T("");
	//}}AFX_DATA_INIT
	// Note that LoadIcon does not require a subsequent DestroyIcon in Win32
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
		
	m_lKeyboardInputControlID = 0;

}

CTutorialVC2Dlg::~CTutorialVC2Dlg()
{
}

void CTutorialVC2Dlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CTutorialVC2Dlg)
	DDX_Text(pDX, IDC_EDIT_END, m_zEndPoint);
	DDX_Text(pDX, IDC_EDIT_START, m_zStartPoint);
	DDX_Text(pDX, IDC_EDIT_BEARING, m_zBearing);
	DDX_Text(pDX, IDC_EDIT_DISTANCE, m_zDistance);
	DDX_Control(pDX, IDC_INPUTMANAGER1, m_InputFunnel);
	//}}AFX_DATA_MAP
}

BEGIN_MESSAGE_MAP(CTutorialVC2Dlg, CDialog)
	//{{AFX_MSG_MAP(CTutorialVC2Dlg)
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_BN_CLICKED(ID_CALCULATE, OnCalculate)
	ON_BN_CLICKED(ID_HTWINDOW, OnHtwindow)
	ON_BN_CLICKED(ID_SRWINDOW, OnSrwindow)
	ON_EN_SETFOCUS(IDC_EDIT_BEARING, OnSetfocusEditBearing)
	ON_EN_SETFOCUS(IDC_EDIT_DISTANCE, OnSetfocusEditDistance)
	ON_EN_SETFOCUS(IDC_EDIT_START, OnSetfocusEditStart)
	ON_EN_KILLFOCUS(IDC_EDIT_BEARING, OnKillfocusEditBearing)
	ON_EN_KILLFOCUS(IDC_EDIT_DISTANCE, OnKillfocusEditDistance)
	ON_EN_KILLFOCUS(IDC_EDIT_START, OnKillfocusEditStart)
	ON_EN_SETFOCUS(IDC_EDIT_END, OnSetfocusEditEnd)
	ON_WM_CLOSE()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CTutorialVC2Dlg message handlers

BOOL CTutorialVC2Dlg::OnInitDialog()
{
	CDialog::OnInitDialog();

	try
	{
		// Declare license object
		IUCLIDComponentLMPtr ipLicense( __uuidof(UCLIDComponentLM) );
		
		// TODO: Initialize license object with valid file and 4 passwords
		ipLicense->InitializeFromFile( "Insert filename here!!!", 1, 2, 3, 4 );

		// Set default unit for Distance to feet
		IDistancePtr	ipDistance( __uuidof(Distance) );
		ipDistance->put_GlobalDefaultDistanceUnit( kFeet );
		
		// Set the icon for this dialog.  The framework does this automatically
		//  when the application's main window is not a dialog
		SetIcon(m_hIcon, TRUE);			// Set big icon
		SetIcon(m_hIcon, FALSE);		// Set small icon

		// Create the Input Context object
		IInputContextPtr ipContext( __uuidof(LandRecordsIC) );

		// Provide unchanging Input Context to Input Manager
		m_InputFunnel.SetInputContext( ipContext );

		// Set visibility of input receivers
		m_InputFunnel.ShowWindows( VARIANT_TRUE );
		
		// Create a C-Pen input receiver
//		m_InputFunnel.CreateNewInputReceiver( "C-Pen" );
		
		// Begin with the starting point
		m_lControlID = IDC_EDIT_START;
		GetDlgItem( m_lControlID )->SetFocus();
		
	}
	catch (_com_error& e)
	{
		m_ipException->CreateFromString( "12345", e.Description());

		// Display the Exception
		m_ipException->Display();
	}
	catch (CException* ex)
	{
		TCHAR   szCause[1000];

		ex->GetErrorMessage(szCause, 1000);

		m_ipException->CreateFromString( "12359", szCause);

		// Display the Exception
		m_ipException->Display();

		ex->Delete();
	}


	return FALSE;  // return TRUE  unless you set the focus to a control
}

// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.

void CTutorialVC2Dlg::OnPaint() 
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
HCURSOR CTutorialVC2Dlg::OnQueryDragIcon()
{
	return (HCURSOR) m_hIcon;
}

void CTutorialVC2Dlg::OnCalculate() 
{
	try
	{
		// Disable input
		m_InputFunnel.DisableInput();
		
		// Retrieve the data
		UpdateData( TRUE );
		
		// Compute the end point
		double dEndX, dEndY;
		dEndX = m_dStartX + cos(m_dBearingInRadians) * m_dDistanceInFeet;
		dEndY = m_dStartY + sin(m_dBearingInRadians) * m_dDistanceInFeet;
		
		// Format the result as a string and display in edit box
		m_zEndPoint.Format( "%f , %f", dEndX, dEndY );
		UpdateData( FALSE );
	}
	catch (CException* ex)
	{
		TCHAR   szCause[1000];
		ex->GetErrorMessage(szCause, 1000);

		m_ipException->CreateFromString( "12346", szCause);

		// Display the Exception
		m_ipException->Display();

		ex->Delete();
	}

}

void CTutorialVC2Dlg::OnHtwindow() 
{
	try
	{
		// Use the input funnel to create the input receiver
		m_InputFunnel.CreateNewInputReceiver( "Highlighted Text Window" );
	}
	catch (CException* ex)
	{
		TCHAR   szCause[1000];
		ex->GetErrorMessage(szCause, 1000);
		m_ipException->CreateFromString( "12347", szCause);

		// Display the Exception
		m_ipException->Display();

		ex->Delete();
	}

}

void CTutorialVC2Dlg::OnSrwindow() 
{
	try
	{
		// Use the input funnel to create the input receiver
		m_InputFunnel.CreateNewInputReceiver( "Spot recognition window" );
	}
	catch (CException* ex)
	{
		TCHAR   szCause[1000];
		ex->GetErrorMessage(szCause, 1000);
		ex->Delete();
		m_ipException->CreateFromString( "12348", szCause);

		// Display the Exception
		m_ipException->Display();

	}

}

void CTutorialVC2Dlg::OnClose() 
{
	try
	{
		// Clean up the input funnel
		m_InputFunnel.Destroy();
	}
	catch (CException* ex)
	{
		TCHAR   szCause[1000];
		ex->GetErrorMessage(szCause, 1000);
		m_ipException->CreateFromString( "12349", szCause);

		// Display the Exception
		m_ipException->Display();

		ex->Delete();
	}
	

	CDialog::OnClose();
}

BEGIN_EVENTSINK_MAP(CTutorialVC2Dlg, CDialog)
    //{{AFX_EVENTSINK_MAP(CTutorialVC2Dlg)
	ON_EVENT(CTutorialVC2Dlg, IDC_INPUTMANAGER1, 1 /* NotifyInputReceived */, OnNotifyInputReceivedInputmanager1, VTS_DISPATCH)
	//}}AFX_EVENTSINK_MAP
END_EVENTSINK_MAP()


void CTutorialVC2Dlg::OnSetfocusEditBearing() 
{
	try
	{
		// Set the control ID
		m_lControlID = IDC_EDIT_BEARING;
		
		// Enable input from funnel
		m_InputFunnel.EnableInput1( "Bearing", "Please specify bearing...", NULL );
	}
	catch (CException* ex)
	{
		GetDlgItem(IDOK)->SetFocus();

		TCHAR   szCause[1000];
		ex->GetErrorMessage(szCause, 1000);
		m_ipException->CreateFromString( "12350", szCause);
		
		// Display the Exception
		m_ipException->Display();

		ex->Delete();
	}

}

void CTutorialVC2Dlg::OnSetfocusEditDistance() 
{
	try
	{
		// Set the control ID
		m_lControlID = IDC_EDIT_DISTANCE;
		
		// Enable input from funnel
		m_InputFunnel.EnableInput1( "Distance", "Please specify distance...", NULL );
	}
	catch (CException* ex)
	{
		GetDlgItem(IDOK)->SetFocus();
		TCHAR   szCause[1000];
		ex->GetErrorMessage(szCause, 1000);
		m_ipException->CreateFromString( "12351", szCause);
		
		// Display the Exception
		m_ipException->Display();

		ex->Delete();
	}
}

void CTutorialVC2Dlg::OnSetfocusEditStart() 
{
	try
	{
		// Set the control ID
		m_lControlID = IDC_EDIT_START;
		
		// Enable input from funnel
		m_InputFunnel.EnableInput1( "Cartographic Point", "Please specify starting point...", NULL );
	}
	catch (CException* ex)
	{
		GetDlgItem(IDOK)->SetFocus();

		TCHAR   szCause[1000];
		ex->GetErrorMessage(szCause, 1000);
		m_ipException->CreateFromString( "12352", szCause);
		
		// Display the Exception
		m_ipException->Display();

		ex->Delete();
	}

}

void CTutorialVC2Dlg::OnKillfocusEditBearing() 
{
	try
	{
		// Retrieve text
		UpdateData( TRUE );
		
		// Send data to input funnel
		if (!m_zBearing.IsEmpty())
		{
			m_lKeyboardInputControlID = IDC_EDIT_BEARING;
			m_InputFunnel.ProcessTextInput( m_zBearing );
			m_lKeyboardInputControlID = 0;
		}
	}
	catch (CException* ex)
	{
		GetDlgItem(IDOK)->SetFocus();

		TCHAR   szCause[1000];
		ex->GetErrorMessage(szCause, 1000);
		m_ipException->CreateFromString( "12353", szCause);
		
		// Display the Exception
		m_ipException->Display();

		ex->Delete();
	}

}

void CTutorialVC2Dlg::OnKillfocusEditDistance() 
{
	try
	{
		// Retrieve text
		UpdateData( TRUE );
		
		// Send data to input funnel
		if (!m_zDistance.IsEmpty())
		{
			m_lKeyboardInputControlID = IDC_EDIT_DISTANCE;
			m_InputFunnel.ProcessTextInput( m_zDistance );
			m_lKeyboardInputControlID = 0;
		}
	}
	catch (CException* ex)
	{
		GetDlgItem(IDOK)->SetFocus();

		TCHAR   szCause[1000];
		ex->GetErrorMessage(szCause, 1000);
		m_ipException->CreateFromString( "12354", szCause);
		
		// Display the Exception
		m_ipException->Display();

		ex->Delete();
	}
	
}

void CTutorialVC2Dlg::OnKillfocusEditStart() 
{
	try
	{
		// Retrieve text
		UpdateData( TRUE );
		
		// Send data to input funnel
		if (!m_zStartPoint.IsEmpty())
		{
			m_lKeyboardInputControlID = IDC_EDIT_START;
			m_InputFunnel.ProcessTextInput( m_zStartPoint );
			m_lKeyboardInputControlID = 0;
		}
	}
	catch (CException* ex)
	{
		GetDlgItem(IDOK)->SetFocus();

		TCHAR   szCause[1000];
		ex->GetErrorMessage(szCause, 1000);
		m_ipException->CreateFromString( "12355", szCause);

		// Display the Exception
		m_ipException->Display();

		ex->Delete();
	}

}

void CTutorialVC2Dlg::OnSetfocusEditEnd() 
{
	try
	{
		// Disable input
		m_InputFunnel.DisableInput();
	}
	catch (CException* ex)
	{
		GetDlgItem(IDOK)->SetFocus();

		TCHAR   szCause[1000];
		ex->GetErrorMessage(szCause, 1000);
		m_ipException->CreateFromString( "12356", szCause);
		
		// Display the Exception
		m_ipException->Display();

		ex->Delete();
	}

}

void CTutorialVC2Dlg::OnNotifyInputReceivedInputmanager1(LPDISPATCH pTextInput) 
{
	try
	{
		// Create the text input object
		ITextInputPtr	ipTextInput = pTextInput;
		
		// Retrieve the received text
		BSTR	_bstrText( ipTextInput->GetText() );
		
		// Determine the appropriate control ID
		long lID = m_lKeyboardInputControlID == 0 ? m_lControlID 
			: m_lKeyboardInputControlID;
		
		// Retrieve the validated input object, if any
		IUnknownPtr ipValidatedInputObj = ipTextInput->GetValidatedInput();
		switch( lID )
		{
		case IDC_EDIT_START:
			// Retrieve X and Y components of the start point
			if (ipValidatedInputObj.GetInterfacePtr())
			{
				ICartographicPointPtr ipPoint = ipValidatedInputObj;
				if (ipPoint != NULL)
				{
					ipPoint->GetPointInXY( &m_dStartX, &m_dStartY );
				}
			}
			else
			{
				MessageBox("UNEXPECTED ERROR: No validated input object was received!");
			}
			break;
			
		case IDC_EDIT_BEARING:
			// Retrieve bearing value in radians
			if (ipValidatedInputObj.GetInterfacePtr())
			{
				IBearingPtr ipBearing = ipValidatedInputObj;
				if (ipBearing != NULL)
				{
					m_dBearingInRadians = ipBearing->GetBearingInRadians();
				}
			}
			else
			{
				MessageBox("UNEXPECTED ERROR: No validated input object was received!");
			}
			break;
			
		case IDC_EDIT_DISTANCE:
			// Retrieve distance in feet
			if (ipValidatedInputObj.GetInterfacePtr())
			{
				IDistancePtr ipDistance = ipValidatedInputObj;
				if (ipDistance != NULL)
				{
					m_dDistanceInFeet = ipDistance->GetDistanceInUnit( kFeet );
				}
			}
			else
			{
				MessageBox("UNEXPECTED ERROR: No validated input object was received!");
			}
			break;
		}
		
		// Display the received text in the active edit box
		CString	zText( _bstrText );
		GetDlgItem( lID )->SetWindowText( zText );
		
		// Automatically set focus to the next field
		if (!m_lKeyboardInputControlID)
		{
			GetDlgItem( lID )->PostMessage( WM_KEYDOWN, VK_TAB, 1 );
		}
	}
	catch (_com_error& e)
	{
		_bstr_t bstr(e.Description());
		m_ipException->CreateFromString( "12357", bstr);
		
		// Display the Exception
		m_ipException->Display();
	}
	catch (CException* ex)
	{
		TCHAR   szCause[1000];
		ex->GetErrorMessage(szCause, 1000);
		m_ipException->CreateFromString( "12358", szCause);
		
		// Display the Exception
		m_ipException->Display();

		ex->Delete();
	}	
}

