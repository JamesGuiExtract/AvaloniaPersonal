// NumberInputDlg.cpp : implementation file
//

#include "stdafx.h"
#include "resource.h"
#include "NumberInputDlg.h"

#include <TemporaryResourceOverride.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

extern HINSTANCE g_Resource;

/////////////////////////////////////////////////////////////////////////////
// NumberInputDlg dialog


NumberInputDlg::NumberInputDlg(CWnd* pParent /*=NULL*/)
	: CDialog(NumberInputDlg::IDD, pParent), m_pParent(pParent)
{
//	AFX_MANAGE_STATE( AfxGetModuleState() );
	//{{AFX_DATA_INIT(NumberInputDlg)
	m_editContent = _T("");
	//}}AFX_DATA_INIT
}

NumberInputDlg::~NumberInputDlg()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
}

void NumberInputDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(NumberInputDlg)
	DDX_Control(pDX, IDC_EDIT_NUMBER, m_ctrlInput);
	DDX_Text(pDX, IDC_EDIT_NUMBER, m_editContent);
	//}}AFX_DATA_MAP
}


BEGIN_MESSAGE_MAP(NumberInputDlg, CDialog)
	//{{AFX_MSG_MAP(NumberInputDlg)
	ON_WM_CLOSE()
	ON_BN_CLICKED(IDC_BTN_SUBMIT, OnBtnSubmit)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// NumberInputDlg message handlers

void NumberInputDlg::OnClose() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( g_Resource );

	CDialog::OnClose();
	DestroyWindow();

	m_ipInputReceiver->OnAboutToDestroy();
}

BOOL NumberInputDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( g_Resource );

	CDialog::OnInitDialog();
	
	// TODO: Add extra initialization here
	
	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}

BOOL NumberInputDlg::Create(UINT nIDTemplate, CWnd* pParentWnd) 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( g_Resource );
	
	return CDialog::Create( nIDTemplate, pParentWnd );
}

BOOL NumberInputDlg::CreateModeless() 
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( g_Resource );
	
	return Create( NumberInputDlg::IDD, NULL );
}

void NumberInputDlg::OnBtnSubmit() 
{	
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride( g_Resource );

	UpdateData(TRUE);
	// send text to the parent control
	m_ipInputReceiver->OnInputReceived(_bstr_t(m_editContent));
}

void NumberInputDlg::PostNcDestroy() 
{	
	CDialog::PostNcDestroy();
}
