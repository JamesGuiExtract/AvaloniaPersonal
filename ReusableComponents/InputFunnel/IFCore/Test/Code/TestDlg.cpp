// TestDlg.cpp : implementation file
//

#include "stdafx.h"
#include "Test.h"
#include "TestDlg.h"

#include <atlbase.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CAboutDlg dialog used for App About

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
// CTestDlg dialog

CTestDlg::CTestDlg(CWnd* pParent /*=NULL*/)
: CDialog(CTestDlg::IDD, pParent),
  m_ipTextInputValidator(NULL)
{
	//{{AFX_DATA_INIT(CTestDlg)
	m_editOutput = _T("");
	m_nValidator = -1;
	//}}AFX_DATA_INIT
	// Note that LoadIcon does not require a subsequent DestroyIcon in Win32
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);

}

CTestDlg::~CTestDlg()
{
}

void CTestDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CTestDlg)
	DDX_Control(pDX, IDC_INPUTMANAGER1, m_ctrlInputManager);
	DDX_Text(pDX, IDC_EDIT_OUTPUT, m_editOutput);
	DDX_Radio(pDX, IDC_RADIO_TEXT_VALIDATOR, m_nValidator);
	//}}AFX_DATA_MAP
}

BEGIN_MESSAGE_MAP(CTestDlg, CDialog)
	//{{AFX_MSG_MAP(CTestDlg)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_BN_CLICKED(IDC_BTN_SHOW, OnBtnShow)
	ON_WM_CLOSE()
	ON_BN_CLICKED(IDC_BTN_CLOSE_ALLIR, OnBtnCloseAllir)
	ON_BN_CLICKED(IDC_RADIO_NUMBER_VALIDATOR, OnRadioNumberValidator)
	ON_BN_CLICKED(IDC_RADIO_TEXT_VALIDATOR, OnRadioTextValidator)
	ON_BN_CLICKED(IDC_BTN_APP_CREATE, OnBtnAppCreate)
	ON_BN_CLICKED(IDC_BTN_APP_DESTROY, OnBtnAppDestroy)
	ON_WM_DESTROY()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

BEGIN_EVENTSINK_MAP(CTestDlg, CDialog)
    //{{AFX_EVENTSINK_MAP(CTestDlg)
	ON_EVENT(CTestDlg, IDC_INPUTMANAGER1, 1 /* OnInputReceived */, OnOnInputReceivedInputmanager1, VTS_DISPATCH)
	//}}AFX_EVENTSINK_MAP
END_EVENTSINK_MAP()

/////////////////////////////////////////////////////////////////////////////
// CTestDlg message handlers

BOOL CTestDlg::OnInitDialog()
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
	
	
	// set validator to text validator
	m_nValidator = 0;

	UpdateData(FALSE);

	// Enable input call on input manager
	enableInput();

	return TRUE;  // return TRUE  unless you set the focus to a control
}

void CTestDlg::OnSysCommand(UINT nID, LPARAM lParam)
{
	if ((nID & 0xFFF0) == IDM_ABOUTBOX)
	{
		CAboutDlg dlgAbout;
		dlgAbout.DoModal();
	}
	else
	{
		CDialog::OnSysCommand(nID, lParam);
	}
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

void CTestDlg::OnBtnShow() 
{
	m_ctrlInputManager.CreateNewInputReceiver("Number Input Receiver");
	m_ctrlInputManager.ShowWindows(VARIANT_TRUE);
}


void CTestDlg::OnOnInputReceivedInputmanager1(LPDISPATCH pTextInput) 
{
	// set text input to the edit box
	ITextInputPtr ipTextInput(pTextInput);
	if (ipTextInput)
	{
		BSTR b = ipTextInput->GetText();
		CComBSTR bstr(b);

		m_editOutput = bstr;

		UpdateData(FALSE);
	}
	
}

void CTestDlg::OnClose() 
{	
	m_ctrlInputManager.Destroy();

	CDialog::OnClose();
}

void CTestDlg::OnBtnCloseAllir() 
{
	m_ctrlInputManager.Destroy();
}

void CTestDlg::enableInput(bool bEnable)
{
	if (bEnable)
	{
		switch(m_nValidator)
		{
		case 0:		// app creates the text input validator and call EnableInput2 
			m_ctrlInputManager.EnableInput2(getTextInputValidator(), "Enter the text");
			break;
		case 1:		// input mgr creates the number input validator and call EnableInput1 
			m_ctrlInputManager.EnableInput1("Number Input Validator", "Enter a number");
			break;
		default:
			break;
		}
	}
	else
	{
		m_ctrlInputManager.DisableInput();
	}
}

IInputValidatorPtr CTestDlg::getTextInputValidator()
{
	if (m_ipTextInputValidator == NULL)
	{
		IIUnknownVectorPtr ipIUnknownVec;
		IInputManagerPtr ipInputMgr(__uuidof(InputManager));
		ipIUnknownVec = ipInputMgr->DetectInputValidators();
		if (ipIUnknownVec)
		{
			CComBSTR cbstrTexInputValidatorName("Text Input Validator");
			long nSize = ipIUnknownVec->size();
			for (int i = 0; i < nSize; i++)
			{
				// Look for strInputReceiverName under InputValidator category in Registry, 
				// then create an instance of the specified input Validator
				ICategorizedComponentInfoPtr ipCatCompInfo(ipIUnknownVec->at(i));
				if (ipCatCompInfo)
				{
					BSTR temp = ipCatCompInfo->getDescription();
					CComBSTR cbstr(temp);
					if (cbstr == cbstrTexInputValidatorName)
					{
						BSTR progID(ipCatCompInfo->getProgID());
						IInputValidatorPtr ipTextIR(progID);
						// found the registered input validator component.Create it
						m_ipTextInputValidator= ipTextIR;
												
						break;
					}
				}
			}				

		}
	}

	return m_ipTextInputValidator;
}


void CTestDlg::OnRadioNumberValidator() 
{
	UpdateData(TRUE);
	enableInput();
}

void CTestDlg::OnRadioTextValidator() 
{
	UpdateData(TRUE);
	enableInput();
}

void CTestDlg::OnBtnAppCreate() 
{
	m_ctrlInputManager.ShowWindows(TRUE);
	
	IInputReceiverPtr ipNumIR;
	ipNumIR.CreateInstance(__uuidof(NumberInputReceiver));

	// Application create input receiver
	long nID = m_ctrlInputManager.ConnectInputReceiver(ipNumIR);
	m_mapIDToNumInputReceiver[nID] = ipNumIR;
}

void CTestDlg::OnBtnAppDestroy() 
{
	map<long, IInputReceiver*>::iterator iter;
	
	while (true)
	{
		iter = m_mapIDToNumInputReceiver.begin();
		if (iter != m_mapIDToNumInputReceiver.end())
		{
			m_ctrlInputManager.DisconnectInputReceiver(iter->first);
			m_mapIDToNumInputReceiver.erase(iter->first);
		}
		else
		{
			break;
		}
	}
}

void CTestDlg::OnDestroy() 
{
	CDialog::OnDestroy();
	
	// TODO: Add your message handler code here
	
}
