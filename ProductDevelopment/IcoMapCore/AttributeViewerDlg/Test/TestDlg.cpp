// TestDlg.cpp : implementation file
//

#include "stdafx.h"
#include <comdef.h>
#include "Test.h"
#include "TestDlg.h"

#include "AttributeViewDlg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CTestDlg dialog
/////////////////////////////////////////////////////////////////////////////
CTestDlg::CTestDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CTestDlg::IDD, pParent),
	m_bCreated(FALSE)
{
	//{{AFX_DATA_INIT(CTestDlg)
	m_bProvideOriginal = FALSE;
	m_bShowOriginal = FALSE;
	m_bCurrRO = FALSE;
	m_bOrigRO = FALSE;
	//}}AFX_DATA_INIT

	// Note that LoadIcon does not require a subsequent DestroyIcon in Win32
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}

void CTestDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CTestDlg)
	DDX_Check(pDX, IDC_CHECK_PROVIDEORIG, m_bProvideOriginal);
	DDX_Check(pDX, IDC_CHECK_SHOWORIG, m_bShowOriginal);
	DDX_Check(pDX, IDC_CHECK_ROCURR, m_bCurrRO);
	DDX_Check(pDX, IDC_CHECK_ROORIG, m_bOrigRO);
	//}}AFX_DATA_MAP
}

BEGIN_MESSAGE_MAP(CTestDlg, CDialog)
	//{{AFX_MSG_MAP(CTestDlg)
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_BN_CLICKED(IDC_BUTTON1, OnButton1)
	ON_BN_CLICKED(IDC_BUTTON2, OnButton2)
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

void CTestDlg::OnButton1() 
{
	// Retrieve state of check boxes
	UpdateData( TRUE );

	// Check to see if Features have already been created
	if (!m_bCreated)
	{
		createFeatures();
	}

	//////////////////////////////////
	// Create and run the modal dialog
	//////////////////////////////////
	if (m_bCreated)
	{
		CAttributeViewDlg	dlg( m_ptrCurrFeature, m_ptrOrigFeature, 
			m_bShowOriginal, m_bCurrRO, m_bOrigRO );

		if (dlg.DoModal() == IDOK)
		{
			// Check validity of Current Feature
			BOOL bValid = dlg.isCurrentFeatureValid();
			if (bValid)
			{
				// Replace Feature
				m_ptrCurrFeature = dlg.getCurrentFeature();
			}

			// Check validity of Original Feature
			bValid = dlg.isOriginalFeatureValid();
			if (bValid)
			{
				// Replace Feature
				m_ptrOrigFeature = dlg.getOriginalFeature();
			}
		}
		else
		{
			BOOL bValid = dlg.isCurrentFeatureValid();
			bValid = dlg.isOriginalFeatureValid();
		}
	}					// end if Feature creation success
}

void CTestDlg::OnButton2() 
{
	MessageBox( "A modeless dialog is not supported at this time.", "Error" );

	// TODO: Add support for modeless dialog construction and creation 
}

BOOL CTestDlg::addLine(LPCTSTR pszBearing, LPCTSTR pszDistance, BOOL bCurrentPart)
{
	BOOL	bResult = FALSE;
	HRESULT	hr;

	// Create LineSegment object
	ILineSegmentPtr	ptrLine;
	hr = ptrLine.CreateInstance( __uuidof(LineSegment), NULL, 
		CLSCTX_INPROC_SERVER );

	// Check success of creation
	if (!FAILED(hr))
	{
		// Prepare BSTR objects for data
		_bstr_t	bstrBearing( pszBearing );
		_bstr_t	bstrDistance( pszDistance );

		// Set the bearing and distance values for this line
		ptrLine->setBearingDistance( bstrBearing, bstrDistance );

		// Add the segment to the desired part
		ISegmentPtr	ptrSegment = ptrLine;
		if (bCurrentPart)
		{
			m_ptrCurrPart->addSegment( ptrSegment );
		}
		else
		{
			m_ptrOrigPart->addSegment( ptrSegment );
		}

		// Set success flag
		bResult = TRUE;
	}

	return bResult;
}

BOOL CTestDlg::addCurve(LPCTSTR pszRadius, LPCTSTR pszChordBearing, 
						LPCTSTR pszChordLength, LPCTSTR pszConcaveLeft, 
						BOOL bCurrentPart)
{
	BOOL	bResult = FALSE;
	HRESULT	hr;

	// Create ArcSegment object
	IArcSegmentPtr	ptrArc;
	hr = ptrArc.CreateInstance( __uuidof(ArcSegment), NULL, 
		CLSCTX_INPROC_SERVER );

	// Check success of creation
	if (!FAILED(hr))
	{
		//////////////////////////////////////
		// Prepare the parameters for this arc
		//////////////////////////////////////
		IParameterTypeValuePairPtr	ptrParam1;
		IParameterTypeValuePairPtr	ptrParam2;
		IParameterTypeValuePairPtr	ptrParam3;
		IParameterTypeValuePairPtr	ptrParam4;

		// Temporary variable holding value strings
		_bstr_t	bstrTemp( "0" );

		// First parameter - Radius
		hr = ptrParam1.CreateInstance( __uuidof(ParameterTypeValuePair), 
			NULL, CLSCTX_INPROC_SERVER );
		if (!FAILED(hr))
		{
			// Set the parameter type and value
			bstrTemp = pszRadius;
			ptrParam1->put_eParamType( kArcRadius );
			ptrParam1->put_strValue( bstrTemp );
		}
		else
		{
			goto end;
		}

		// Second parameter - Chord Bearing
		hr = ptrParam2.CreateInstance( __uuidof(ParameterTypeValuePair), 
			NULL, CLSCTX_INPROC_SERVER );
		if (!FAILED(hr))
		{
			// Set the parameter type and value
			bstrTemp = pszChordBearing;
			ptrParam2->put_eParamType( kArcChordBearing );
			ptrParam2->put_strValue( bstrTemp );
		}
		else
		{
			goto end;
		}

		// Third parameter - Chord Length
		hr = ptrParam3.CreateInstance( __uuidof(ParameterTypeValuePair), 
			NULL, CLSCTX_INPROC_SERVER );
		if (!FAILED(hr))
		{
			// Set the parameter type and value
			bstrTemp = pszChordLength;
			ptrParam3->put_eParamType( kArcChordLength );
			ptrParam3->put_strValue( bstrTemp );
		}
		else
		{
			goto end;
		}

		// Fourth parameter - Concave Left
		hr = ptrParam4.CreateInstance( __uuidof(ParameterTypeValuePair), 
			NULL, CLSCTX_INPROC_SERVER );
		if (!FAILED(hr))
		{
			// Set the parameter type and value
			bstrTemp = pszConcaveLeft;
			ptrParam4->put_eParamType( kArcConcaveLeft );
			ptrParam4->put_strValue( bstrTemp );
		}
		else
		{
			goto end;
		}

		/////////////////////////////////////////////
		// Prepare the parameters vector for this arc
		/////////////////////////////////////////////
		IIUnknownVectorPtr	ptrVector;
		hr = ptrVector.CreateInstance( __uuidof(IUnknownVector), 
			NULL, CLSCTX_INPROC_SERVER );
		if (!FAILED(hr))
		{
			// Add parameters to vector
			ptrVector->push_back( ptrParam1 );
			ptrVector->push_back( ptrParam2 );
			ptrVector->push_back( ptrParam3 );
			ptrVector->push_back( ptrParam4 );

			// Set the parameters
			ptrArc->setParameters( ptrVector );
		}
		else
		{
			goto end;
		}

		//////////////////////////////////////
		// Add the segment to the desired part
		//////////////////////////////////////
		ISegmentPtr	ptrSegment = ptrArc;
		if (bCurrentPart)
		{
			m_ptrCurrPart->addSegment( ptrSegment );
		}
		else
		{
			m_ptrOrigPart->addSegment( ptrSegment );
		}

		///////////////////
		// Set success flag
		///////////////////
		bResult = TRUE;

	}		// end if ArcSegment creation success

end:
	return bResult;
}

void CTestDlg::createFeatures() 
{
	HRESULT hr;

	/////////////////////////////////////////
	// Create Original Attributes, if desired
	/////////////////////////////////////////
	if (m_bProvideOriginal)
	{
		hr = m_ptrOrigFeature.CreateInstance( __uuidof(Feature), NULL, 
			CLSCTX_INPROC_SERVER );
		if( FAILED( hr ) )
		{
			MessageBox( "Could not create Original Attributes Feature data", "Error" );
			m_bCreated = FALSE;
		}
		else
		{
			// Define feature type
			m_ptrOrigFeature->setFeatureType( kPolygon );

			// Create the part to add to this feature
			hr = m_ptrOrigPart.CreateInstance( __uuidof(Part), NULL, CLSCTX_INPROC_SERVER );
			if (!FAILED(hr))
			{
				// Create new CartographicPoint object
				ICartographicPointPtr	ptrPoint;
				hr = ptrPoint.CreateInstance( __uuidof(CartographicPoint), NULL, 
					CLSCTX_INPROC_SERVER );
				if (!FAILED(hr))
				{
					// Initialize the point
					ptrPoint->PutdX( 1.0 );
					ptrPoint->PutdY( 1.0 );

					// Attach this point to Part as the starting point
					m_ptrOrigPart->setStartingPoint( ptrPoint );

					// Add some lines - from Amy's sample dialog
					addLine( "N801012W", "212.58", FALSE );
					addLine( "S120709E", "197.5", FALSE );

					// Add a curve
					addCurve( "79", "S254023E", "99.425", "0", FALSE );

					//////////////////////////////
					// Add the part to the feature
					//////////////////////////////
					m_ptrOrigFeature->addPart( m_ptrOrigPart );

				}			// end if Point creation success
			}				// end if Part creation success
		}

		m_bCreated = TRUE;
	}

	// TODO: Test this technique for COM object construction
	// instead of using CreateInstance()
//	IFeaturePtr	m_ptrCurrFeature("UCLIDFeatureMgmt.Feature");
//	if (m_ptrCurrFeature == NULL)
//	{
//		MessageBox( "Could not create Feature data", "Error" );
//	}
//	else
	////////////////////////////
	// Create Current Attributes
	////////////////////////////
	hr = m_ptrCurrFeature.CreateInstance( __uuidof(Feature), NULL, 
		CLSCTX_INPROC_SERVER );
	if( FAILED( hr ) )
	{
		MessageBox( "Could not create Current Attributes Feature data", "Error" );
		m_bCreated = FALSE;
	}
	else
	{
		// Define feature type
		m_ptrCurrFeature->setFeatureType( kPolygon );

		// Create the part to add to this feature
		hr = m_ptrCurrPart.CreateInstance( __uuidof(Part), NULL, CLSCTX_INPROC_SERVER );
		if (!FAILED(hr))
		{
			// Create new CartographicPoint object
			ICartographicPointPtr	ptrPoint;
			hr = ptrPoint.CreateInstance( __uuidof(CartographicPoint), NULL, 
				CLSCTX_INPROC_SERVER );
			if (!FAILED(hr))
			{
				// Initialize the point
				ptrPoint->PutdX( 1.0 );
				ptrPoint->PutdY( 1.0 );

				// Attach this point to Part as the starting point
				m_ptrCurrPart->setStartingPoint( ptrPoint );

				// Add some lines - from legal04.txt
				addLine( "N892302E", "591.50", TRUE );
				addLine( "S185658E", "268.60", TRUE );
				addLine( "S893202W", "38.23", TRUE );
				addLine( "S023900E", "237.64", TRUE );
				addLine( "S570100W", "163.00", TRUE );
				addLine( "S151742W", "241.14", TRUE );

				// Add a curve
				addCurve( "233.00", "N654029W", "141.16", "0", TRUE );

				// Add another line
				addLine( "N831829W", "487.66", TRUE );

				// Add another curve
				addCurve( "558.17", "N78d59m14.5sW", "84.10", "0", TRUE );

				// Add more lines
				addLine( "N211617E", "343.09", TRUE );
				addLine( "N183722E", "375.63", TRUE );

				//////////////////////////////
				// Add the part to the feature
				//////////////////////////////
				m_ptrCurrFeature->addPart( m_ptrCurrPart );

			}			// end if Point creation success
		}				// end if Part creation success
	}					// end if Feature creation success

	m_bCreated = TRUE;
}
