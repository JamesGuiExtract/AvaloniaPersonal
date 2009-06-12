// MFCClientTestDlg.cpp : implementation file
//

#include "stdafx.h"
#include "MFCClientTest.h"
#include "MFCClientTestDlg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// statics/globals
CMFCClientTestDlg* CMFCClientTestDlg ::ms_pInstance = NULL;


/////////////////////////////////////////////////////////////////////////////
// CMFCClientTestDlg dialog

CMFCClientTestDlg::CMFCClientTestDlg(CWnd* pParent /*=NULL*/)
:CDialog(CMFCClientTestDlg::IDD, pParent), m_ipSomeCtrl1(NULL), 
 m_ipSomeCtrl2(NULL), m_pPropPage(NULL), m_iCurrentPropPageControlID(0)
{
	//{{AFX_DATA_INIT(CMFCClientTestDlg)
	m_nCurrentControl = -1;
	//}}AFX_DATA_INIT
	// Note that LoadIcon does not require a subsequent DestroyIcon in Win32
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
	ms_pInstance = this;
}

CMFCClientTestDlg::~CMFCClientTestDlg()
{
	m_ipSomeCtrl1 = NULL;
	m_ipSomeCtrl2 = NULL;
	m_pPropPage = NULL;
}

void CMFCClientTestDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CMFCClientTestDlg)
	DDX_Radio(pDX, IDC_RADIO1, m_nCurrentControl);
	//}}AFX_DATA_MAP
}

BEGIN_MESSAGE_MAP(CMFCClientTestDlg, CDialog)
	//{{AFX_MSG_MAP(CMFCClientTestDlg)
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_BN_CLICKED(IDC_RADIO1, OnRadio1)
	ON_BN_CLICKED(IDC_RADIO2, OnRadio2)
	ON_BN_CLICKED(IDC_BUTTON_APPLY, OnButtonApply)
	ON_BN_CLICKED(IDC_BUTTON_GET_INFO, OnButtonGetInfo)
	ON_BN_CLICKED(IDC_BUTTON_SHOW_IN_RC_UI, OnButtonShowInRcUi)
	ON_BN_CLICKED(IDC_BUTTON_REFRESH, OnButtonRefresh)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//{
BEGIN_INTERFACE_MAP(CMFCClientTestDlg, CDialog)
    INTERFACE_PART(CMFCClientTestDlg, IID_IPropertyPageSite, PropertyPageSite)
END_INTERFACE_MAP()
//}

/////////////////////////////////////////////////////////////////////////////
// CMFCClientTestDlg message handlers

BOOL CMFCClientTestDlg::OnInitDialog()
{
	CDialog::OnInitDialog();

	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon
	
	// TODO: Add extra initialization here
	m_ipSomeCtrl1.CreateInstance(CLSID_ObjA);
	m_ipSomeCtrl2.CreateInstance(CLSID_ObjB);

	m_nCurrentControl = 0;
	OnRadio1();

	UpdateData(FALSE);
	
	return TRUE;  // return TRUE  unless you set the focus to a control
}

// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.

void CMFCClientTestDlg::OnPaint() 
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
HCURSOR CMFCClientTestDlg::OnQueryDragIcon()
{
	return (HCURSOR) m_hIcon;
}

BOOL CMFCClientTestDlg::SetCurrentPage( IUnknown * pUnknown)
{
	CRect rect;
	GetDlgItem(IDC_STATIC_PROPERTIES)->GetWindowRect(&rect);
	rect.left += 3;
	rect.top += 10;
	rect.bottom -= 3;
	rect.right -= 3;
  	ScreenToClient(&rect);

	HRESULT hr = NOERROR;

	try
	{
		// hide previous page and release page pointer
		if( m_pCurrentPage != NULL )
		{
			m_pCurrentPage->Show( SW_HIDE );
			m_pCurrentPage->Deactivate();
			m_pCurrentPage = NULL;

			if( pUnknown == NULL ) return TRUE;
		}
		
		ISpecifyPropertyPagesPtr pSpecifyPropertyPages = pUnknown;

		CAUUID pages;
		hr = pSpecifyPropertyPages->GetPages( &pages );
		if( FAILED( hr ) ) throw _com_error( hr );

		ASSERT( pages.cElems > 0 && pages.pElems != NULL );

		hr = CoCreateInstance( pages.pElems[0], NULL, CLSCTX_INPROC, IID_IPropertyPage, (void**)&m_pPropPage );
		if( FAILED( hr ) ) throw _com_error( hr );

		hr = m_pPropPage->SetPageSite( (IPropertyPageSite*) GetInterface( &IID_IPropertyPageSite ) );
		if( FAILED( hr ) ) throw _com_error( hr );

		hr = m_pPropPage->SetObjects( 1, &pUnknown );
		if( FAILED( hr ) ) throw _com_error( hr );

		hr = m_pPropPage->Activate( GetSafeHwnd(), &rect, TRUE );
		if( FAILED( hr ) ) throw _com_error( hr );

		hr = m_pPropPage->Show( SW_SHOW );
		if( FAILED( hr ) ) throw _com_error( hr );

		m_pCurrentPage = m_pPropPage;

		GetDlgItem(IDC_BUTTON_APPLY)->EnableWindow(FALSE);
	}
	catch( _com_error &e )
	{
		hr = e.Error();
		ASSERT( SUCCEEDED( hr ) );
	}

	return SUCCEEDED( hr );
}

/////////////////////////////////////////////////////////////////////////////
// CMFCClientTestDlg COM interface implementation


STDMETHODIMP_( ULONG ) CMFCClientTestDlg::XPropertyPageSite::AddRef()
{
    METHOD_PROLOGUE( CMFCClientTestDlg, PropertyPageSite )
	return pThis->ExternalAddRef();
}

STDMETHODIMP_( ULONG ) CMFCClientTestDlg::XPropertyPageSite::Release()
{
    METHOD_PROLOGUE( CMFCClientTestDlg, PropertyPageSite )
	return pThis->ExternalRelease();
}

STDMETHODIMP CMFCClientTestDlg::XPropertyPageSite::QueryInterface( REFIID iid, void FAR* FAR* ppvObj)
{
    METHOD_PROLOGUE( CMFCClientTestDlg, PropertyPageSite )
    return (HRESULT)pThis->ExternalQueryInterface( &iid, ppvObj );
}


STDMETHODIMP CMFCClientTestDlg::XPropertyPageSite::OnStatusChange( DWORD dwFlags )
{
    METHOD_PROLOGUE( CMFCClientTestDlg, PropertyPageSite )
	CMFCClientTestDlg::ms_pInstance->GetDlgItem(IDC_BUTTON_APPLY)->EnableWindow(TRUE);
	return NOERROR;
}

STDMETHODIMP CMFCClientTestDlg::XPropertyPageSite::GetLocaleID( LCID *pLocaleID )
{
    METHOD_PROLOGUE( CMFCClientTestDlg, PropertyPageSite )
	*pLocaleID = GetThreadLocale();
	return NOERROR;
}

STDMETHODIMP CMFCClientTestDlg::XPropertyPageSite::GetPageContainer( IUnknown **ppUnk )
{
    METHOD_PROLOGUE( CMFCClientTestDlg, PropertyPageSite )
	return E_FAIL;
}

STDMETHODIMP CMFCClientTestDlg::XPropertyPageSite::TranslateAccelerator( MSG *pMsg )
{
    METHOD_PROLOGUE( CMFCClientTestDlg, PropertyPageSite )

	return pThis->PreTranslateMessage( pMsg ) ? S_OK : S_FALSE;
}

void CMFCClientTestDlg::showPropertyPage(IUnknown *pNewControlUnknown, int iNewControlID) 
{
	if (m_pPropPage != NULL && m_pPropPage->IsPageDirty() == 0)
	{
		if (AfxMessageBox("You have changed one or more of the settings on this page.\nDo you want to save them?", MB_YESNO) == IDYES)
		{
			// The call to Apply() fails if the settings could not
			// be validated.  If the settings could not be validated, then
			// just return and select the radio button corresponding to the
			// currently shown property page;
			if (m_pPropPage->Apply() == S_FALSE)
			{
				m_nCurrentControl = m_iCurrentPropPageControlID;
				UpdateData(FALSE);
				return;
			}
		}
	}

	SetCurrentPage(pNewControlUnknown);
	m_iCurrentPropPageControlID = iNewControlID;
}

void CMFCClientTestDlg::OnRadio1() 
{

	showPropertyPage(m_ipSomeCtrl1, 0);
}

void CMFCClientTestDlg::OnRadio2() 
{
	showPropertyPage(m_ipSomeCtrl2, 1);
}

void CMFCClientTestDlg::OnButtonApply() 
{
	// The call to Apply() fails if the settings could not
	// be validated.  If the settings could not be validated, then
	// just return;
	if (m_pPropPage != NULL && m_pPropPage->Apply() == S_FALSE)
		return;

	// if the apply button worked successfully, then disable it
	GetDlgItem(IDC_BUTTON_APPLY)->EnableWindow(FALSE);
}

void CMFCClientTestDlg::OnButtonGetInfo() 
{
	// show the information associated with the object whose
	// property sheet is currently shown
	UpdateData(TRUE);
	if (m_nCurrentControl == 0)
	{
		CComQIPtr<IObjA> ipA = m_ipSomeCtrl1;
		CComBSTR bstrText;
		ipA->get_RegExpr(&bstrText);
		CString zTemp = bstrText;
		zTemp.Insert(0, "RegExpr = ");
		AfxMessageBox(zTemp);
	}
	else
	{
		CComQIPtr<IObjB> ipB = m_ipSomeCtrl2;
		long nStartPos, nEndPos;
		ipB->get_StartPos(&nStartPos);
		ipB->get_EndPos(&nEndPos);
		CString zTemp;
		zTemp.Format("StartPos = %d\nEndPos = %d", nStartPos, nEndPos);
		AfxMessageBox(zTemp);
	}
}

void CMFCClientTestDlg::OnOK() 
{
	// The call to Apply() fails if the settings could not
	// be validated.  If the settings could not be validated, then
	// just return;
	if (m_pPropPage != NULL && m_pPropPage->Apply() == S_FALSE)
		return;
	
	// the call to Apply() worked fine...so it's ok to close the dialog now
	CDialog::OnOK();
}

void CMFCClientTestDlg::OnButtonShowInRcUi() 
{
	IObjectPropertiesUIPtr ipUI(CLSID_ObjectPropertiesUI);

	UpdateData(TRUE);
	if (m_nCurrentControl == 0)
	{
		ipUI->DisplayProperties1(m_ipSomeCtrl1, "Object A properties");
	}
	else
	{
		ipUI->DisplayProperties1(m_ipSomeCtrl2, "Object B properties");
	}
}

void CMFCClientTestDlg::OnButtonRefresh() 
{
	UpdateData(TRUE);
	if (m_nCurrentControl == 0)
	{
		showPropertyPage(m_ipSomeCtrl1, 0);
	}
	else
	{
		showPropertyPage(m_ipSomeCtrl2, 1);
	}
	
}
