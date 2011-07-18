// SRIRImageViewerDlg.cpp : implementation file
//

#include "stdafx.h"
#include "SRIRImageViewer.h"
#include "SRIRImageViewerDlg.h"

#include <LicenseMgmt.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <FileDirectorySearcher.h>
#include <XBrowseForFolder.h>
#include <IConfigurationSettingsPersistenceMgr.h>
#include <RegistryPersistenceMgr.h>
#include <ClipboardManager.h>
#include <RegConstants.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

#include <string>
#include <vector>
using namespace std;

// class ID for this Dlg which is also a CoClass
// {768BDE07-B2B2-4ab8-AB90-0F1661FADCDF}
const CLSID CLSID_SRIRImageViewerDlg = 
{ 0x768bde07, 0xb2b2, 0x4ab8, { 0xab, 0x90, 0xf, 0x16, 0x61, 0xfa, 0xdc, 0xdf } };

const char *gpszLAST_IMG_EXT_KEY = "LastImageExtension";
const char *gpszLAST_ROOT_FOLDER = "LastRootFolder";

//-------------------------------------------------------------------------------------------------
// CAboutDlg
//-------------------------------------------------------------------------------------------------
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
//-------------------------------------------------------------------------------------------------
CAboutDlg::CAboutDlg()
{
	//{{AFX_DATA_INIT(CAboutDlg)
	//}}AFX_DATA_INIT
}
//-------------------------------------------------------------------------------------------------
void CAboutDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CAboutDlg)
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CAboutDlg, CDialog)
	//{{AFX_MSG_MAP(CAboutDlg)
		// No message handlers
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CSRIRImageViewerDlg dialog
//-------------------------------------------------------------------------------------------------
CSRIRImageViewerDlg::CSRIRImageViewerDlg(ISpotRecognitionWindowPtr ipSRIR, 
										 bool bShowSearch/*false*/, 
										 CWnd* pParent /*=__nullptr*/)
: CDialog(CSRIRImageViewerDlg::IDD, pParent), 
  m_ipSRIR(ipSRIR),
  m_lRefCount(0), 
  m_bShowThisWindow(bShowSearch),
  m_eOCRTextHandlingType(kNone),
  m_strOCRTextFileName("")
{
	//{{AFX_DATA_INIT(CSRIRImageViewerDlg)
	m_zImageEnd = _T("");
	m_zImageExt = _T("");
	m_zRootFolder = _T("");
	//}}AFX_DATA_INIT
	// Note that LoadIcon does not require a subsequent DestroyIcon in Win32

	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);

	// initialize the persistence manager
	m_apCfg = unique_ptr<IConfigurationSettingsPersistenceMgr>(
		new RegistryPersistenceMgr(HKEY_CURRENT_USER,
		gstrREG_ROOT_KEY + "\\InputFunnel\\Utils\\SRIRImageViewerDlg"));
	ASSERT_RESOURCE_ALLOCATION("ELI06338", m_apCfg.get() != __nullptr);

	// This line must be last because it calls OnInitDialog
	Create(CSRIRImageViewerDlg::IDD, pParent);
}
//-------------------------------------------------------------------------------------------------
void CSRIRImageViewerDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CSRIRImageViewerDlg)
	DDX_Text(pDX, IDC_EDIT_IMAGE_END, m_zImageEnd);
	DDX_Text(pDX, IDC_EDIT_IMAGE_EXT, m_zImageExt);
	DDX_Text(pDX, IDC_EDIT_ROOT_FOLDER, m_zRootFolder);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CSRIRImageViewerDlg, CDialog)
	//{{AFX_MSG_MAP(CSRIRImageViewerDlg)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_WM_CLOSE()
	ON_WM_TIMER()
	ON_COMMAND(IDC_BUTTON_SELECT_FOLDER, OnBTNSelectRootFolder)
	ON_COMMAND(IDC_BTN_FIND, OnBTNFind)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
//-------------------------------------------------------------------------------------------------
bool CSRIRImageViewerDlg::openImage(const std::string& strImageFileName)
{
	m_ipSRIR->OpenImageFile(_bstr_t(strImageFileName.c_str()));
	return true;
}
//-------------------------------------------------------------------------------------------------
void CSRIRImageViewerDlg::execScript(const std::string& strScriptFileName)
{
	m_ipSRIR->LoadOptionsFromFile(_bstr_t(strScriptFileName.c_str()));
}
//-------------------------------------------------------------------------------------------------
void CSRIRImageViewerDlg::writeOCRTextToClipboard()
{
	m_eOCRTextHandlingType = kWriteOCRTextToClipboard;
}
//-------------------------------------------------------------------------------------------------
void CSRIRImageViewerDlg::writeOCRTextToMessageBox()
{
	m_eOCRTextHandlingType = kDisplayOCRTextInMessageBox;
}
//-------------------------------------------------------------------------------------------------
void CSRIRImageViewerDlg::writeOCRTextToFile(const std::string strFileName)
{
	m_eOCRTextHandlingType = kWriteOCRTextToFile;
	m_strOCRTextFileName = strFileName;
}
//-------------------------------------------------------------------------------------------------
// Private
//-------------------------------------------------------------------------------------------------
void CSRIRImageViewerDlg::loadSettingsFromRegistry()
{
	if (m_apCfg->keyExists("\\", gpszLAST_ROOT_FOLDER))
	{
		m_zRootFolder = m_apCfg->getKeyValue("\\", gpszLAST_ROOT_FOLDER, "").c_str();
	}

	if (m_apCfg->keyExists("\\", gpszLAST_IMG_EXT_KEY))
	{
		m_zImageExt = m_apCfg->getKeyValue("\\", gpszLAST_IMG_EXT_KEY, "").c_str();
	}

	UpdateData(FALSE);
}
//-------------------------------------------------------------------------------------------------
void CSRIRImageViewerDlg::saveSettingsToRegistry()
{
	UpdateData(TRUE);

	m_apCfg->setKeyValue("\\", gpszLAST_ROOT_FOLDER, (LPCTSTR) m_zRootFolder);
	m_apCfg->setKeyValue("\\", gpszLAST_IMG_EXT_KEY, (LPCTSTR) m_zImageExt);
}
//-------------------------------------------------------------------------------------------------
void CSRIRImageViewerDlg::writeTextToClipboard(const std::string& strText)
{
	ClipboardManager clipboardManager(this);
	clipboardManager.writeText(strText);
}
//-------------------------------------------------------------------------------------------------
void CSRIRImageViewerDlg::handleRecognizedText(const std::string& strText)
{
	switch(m_eOCRTextHandlingType)
	{
	case kWriteOCRTextToClipboard:
		{
			writeTextToClipboard(strText);
		}
		break;
	case kDisplayOCRTextInMessageBox:
		{
			// If OCR is licensed, display the results, otherwise do nothing
			if (m_ipSRIR->IsOCRLicensed() == VARIANT_TRUE)
			{
				// Get the window handle for the ImageWindow
				IInputReceiverPtr ipSR = m_ipSRIR;
				ASSERT_RESOURCE_ALLOCATION("ELI25516", ipSR != __nullptr);
				HWND hWnd = (HWND) ipSR->WindowHandle;

				// Display the OCR results (make them modal to the ImageViewer) [LRCAU #5307]
				::MessageBox(hWnd, strText.c_str(), "OCR Result", MB_OK);
			}
		}
		break;
	case kWriteOCRTextToFile:
		{
			writeToFile(strText, m_strOCRTextFileName);
		}
		break;
	case kNone:
		{
		}
		break;
	default:
		{
			THROW_LOGIC_ERROR_EXCEPTION("ELI11864");
		}
	}
}
//-------------------------------------------------------------------------------------------------
// CSRIRImageViewerDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CSRIRImageViewerDlg::OnInitDialog()
{
	try
	{
		CDialog::OnInitDialog();

		// Add "About..." menu item to system menu.

		// IDM_ABOUTBOX must be in the system command range.
		ASSERT((IDM_ABOUTBOX & 0xFFF0) == IDM_ABOUTBOX);
		ASSERT(IDM_ABOUTBOX < 0xF000);

		CMenu* pSysMenu = GetSystemMenu(FALSE);
		if (pSysMenu != __nullptr)
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

		// Set the window Title

		// show or hide the search dialog (this window)
		ShowWindow(m_bShowThisWindow ? SW_SHOW : SW_HIDE);

		// load last used settings from registry
		loadSettingsFromRegistry();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06295")
	
	return TRUE;  // return TRUE  unless you set the focus to a control
}
//-------------------------------------------------------------------------------------------------
void CSRIRImageViewerDlg::OnSysCommand(UINT nID, LPARAM lParam)
{
	try
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11833");
}
//-------------------------------------------------------------------------------------------------
void CSRIRImageViewerDlg::OnPaint() 
{
	try
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
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11834");
}
//-------------------------------------------------------------------------------------------------
HCURSOR CSRIRImageViewerDlg::OnQueryDragIcon()
{
	return (HCURSOR) m_hIcon;
}
//-------------------------------------------------------------------------------------------------
void CSRIRImageViewerDlg::OnBTNSelectRootFolder()
{
	try
	{
		UpdateData(TRUE);

		char pszPath[MAX_PATH + 1];
		if (XBrowseForFolder(m_hWnd, m_zRootFolder, pszPath, sizeof(pszPath)))
		{
			m_zRootFolder = pszPath;
			UpdateData(FALSE);

			saveSettingsToRegistry();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06334")
}
//-------------------------------------------------------------------------------------------------
void CSRIRImageViewerDlg::OnBTNFind()
{
	try
	{
		UpdateData(TRUE);

		// save settings to registry
		saveSettingsToRegistry();

		// finding the files may take some time
		CWaitCursor wait;

		string strFileSpec = m_zRootFolder;
		strFileSpec += "\\*";
		strFileSpec += m_zImageEnd;
		strFileSpec += ".";
		strFileSpec += m_zImageExt;

		FileDirectorySearcher fs;
		vector<string> vecFiles = fs.searchFiles(strFileSpec.c_str(), true);

		long nVecSize = vecFiles.size();
		if (nVecSize == 0)
		{
			AfxMessageBox("No files found!");
		}
		else if (nVecSize == 1)
		{
			string strFile = vecFiles[0];
			m_ipSRIR->OpenImageFile(_bstr_t(strFile.c_str()));
		}
		else
		{
			string strMsg = "Multiple files found:\n\n";

			for (int i = 0; i < nVecSize; i++)
			{
				if (i != 0)
				{
					strMsg += "\n";
				}
				strMsg += vecFiles[i];
			}

			AfxMessageBox(strMsg.c_str());
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06333")
}

//-------------------------------------------------------------------------------------------------
// IUnknown
//-------------------------------------------------------------------------------------------------
STDMETHODIMP_(ULONG ) CSRIRImageViewerDlg::AddRef()
{
	InterlockedIncrement(&m_lRefCount);
	return m_lRefCount;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP_(ULONG ) CSRIRImageViewerDlg::Release()
{
	InterlockedDecrement(&m_lRefCount);
	return m_lRefCount;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSRIRImageViewerDlg::QueryInterface( REFIID iid, void FAR* FAR* ppvObj)
{
	IParagraphTextHandler *pTmp = this;

	if (iid == IID_IUnknown)
		*ppvObj = static_cast<IUnknown *>(pTmp);
	else if (iid == IID_IParagraphTextHandler)
		*ppvObj = static_cast<IParagraphTextHandler *>(pTmp);
	else if (iid == IID_IDispatch)
		*ppvObj = static_cast<IDispatch *>(pTmp);
	else if (iid == IID_IIREventHandler)
		*ppvObj = static_cast<IIREventHandler *>(this);
	else
		*ppvObj = NULL;

	if (*ppvObj != __nullptr)
	{
		AddRef();
		return S_OK;
	}
	else
	{
		return E_NOINTERFACE;
	}
}
//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSRIRImageViewerDlg::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IParagraphTextHandler,
		&IID_IIREventHandler
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}
//-------------------------------------------------------------------------------------------------
// IParagraphTextHandler
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSRIRImageViewerDlg::raw_NotifyParagraphTextRecognized(
	ISpotRecognitionWindow *pSourceSRWindow, ISpatialString *pText)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// pass the received text to the input property page
		ISpatialStringPtr ipText(pText);
		ASSERT_RESOURCE_ALLOCATION("ELI06745", ipText != __nullptr);

		_bstr_t _bstrParagraphText = ipText->String;
		
		string strText = _bstrParagraphText;
		handleRecognizedText(strText);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06331")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSRIRImageViewerDlg::raw_GetPTHDescription(BSTR *pstrDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		*pstrDescription = _bstr_t("OCR Image").copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06330")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSRIRImageViewerDlg::raw_IsPTHEnabled(VARIANT_BOOL *pbEnabled)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// this PTH is always enabled.
		*pbEnabled = VARIANT_TRUE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06329")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// IIREventHandler
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSRIRImageViewerDlg::raw_NotifyInputReceived(
	UCLID_INPUTFUNNELLib::ITextInput *pText)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		string strText = pText->GetText();
		handleRecognizedText(strText);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06303")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CSRIRImageViewerDlg::raw_NotifyAboutToDestroy(
	UCLID_INPUTFUNNELLib::IInputReceiver *pIR)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		SendMessage(WM_CLOSE);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06304")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
void CSRIRImageViewerDlg::OnTimer(UINT nIDEvent) 
{
	ShowWindow(SW_HIDE);
}
//-------------------------------------------------------------------------------------------------
void CSRIRImageViewerDlg::OnClose() 
{
	try
	{
		saveSettingsToRegistry();
		::PostQuitMessage(0);
		EndDialog(0);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11835");
}
//-------------------------------------------------------------------------------------------------
BOOL CSRIRImageViewerDlg::PreTranslateMessage(MSG* pMsg) 
{
	// TODO: Add your specialized code here and/or call the base class
/*
	if (pMsg->message == 47949)
	{
		string strImage = (const char*)pMsg->wParam;
		openImage(strImage);
		return 1;
	}
*/	
	return CDialog::PreTranslateMessage(pMsg);
}
