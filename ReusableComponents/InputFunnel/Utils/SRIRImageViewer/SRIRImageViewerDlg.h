// SRIRImageViewerDlg.h : header file
//

#if !defined(AFX_SRIRIMAGEVIEWERDLG_H__E470748B_090B_4F32_98FC_3E9918F2BBA3__INCLUDED_)
#define AFX_SRIRIMAGEVIEWERDLG_H__E470748B_090B_4F32_98FC_3E9918F2BBA3__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

EXTERN_C const CLSID CLSID_SRIRImageViewerDlg;
class IConfigurationSettingsPersistenceMgr;

#include <memory>
#include <string>

enum EOCRTextHandlingType
{
	kNone,
	kWriteOCRTextToClipboard,
	kDisplayOCRTextInMessageBox,
	kWriteOCRTextToFile
};

/////////////////////////////////////////////////////////////////////////////
// CSRIRImageViewerDlg dialog
class CSRIRImageViewerDlg : public CDialog,
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass< CSRIRImageViewerDlg, &CLSID_SRIRImageViewerDlg >,
	public IDispatchImpl<IParagraphTextHandler, &IID_IParagraphTextHandler, &LIBID_UCLID_SPOTRECOGNITIONIRLib>,
	public ISupportErrorInfo,
	public IDispatchImpl<IIREventHandler, &IID_IIREventHandler, &LIBID_UCLID_INPUTFUNNELLib>
{
// Construction
public:
	CSRIRImageViewerDlg(ISpotRecognitionWindowPtr ipSRIR, bool bShowSearch = false, CWnd* pParent = __nullptr);	// standard constructor

	bool openImage(const std::string& strImageFileName);
	void execScript(const std::string& strScriptFileName);

	void writeOCRTextToClipboard();
	void writeOCRTextToMessageBox();
	void writeOCRTextToFile(const std::string strFileName);

// Dialog Data
	//{{AFX_DATA(CSRIRImageViewerDlg)
	enum { IDD = IDD_SRIRIMAGEVIEWER_DIALOG };
	CString	m_zImageEnd;
	CString	m_zImageExt;
	CString	m_zRootFolder;
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CSRIRImageViewerDlg)
	public:
	virtual BOOL PreTranslateMessage(MSG* pMsg);
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support
	//}}AFX_VIRTUAL

public:
// IUnknown
	STDMETHODIMP_(ULONG ) AddRef();
	STDMETHODIMP_(ULONG ) Release();
	STDMETHODIMP QueryInterface( REFIID iid, void FAR* FAR* ppvObj);

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IParagraphTextHandler
	STDMETHOD(raw_NotifyParagraphTextRecognized)(
		ISpotRecognitionWindow *pSourceSRWindow, ISpatialString *pText);
	STDMETHOD(raw_GetPTHDescription)(BSTR *pstrDescription);
	STDMETHOD(raw_IsPTHEnabled)(VARIANT_BOOL *pbEnabled);

// IIREventHandler
	STDMETHOD(raw_NotifyInputReceived)(UCLID_INPUTFUNNELLib::ITextInput *pText);
	STDMETHOD(raw_NotifyAboutToDestroy)(UCLID_INPUTFUNNELLib::IInputReceiver *pIR);

// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	//{{AFX_MSG(CSRIRImageViewerDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnClose();
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	afx_msg void OnTimer(UINT nIDEvent);
	afx_msg void OnBTNFind();
	afx_msg void OnBTNSelectRootFolder();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:

	ISpotRecognitionWindowPtr m_ipSRIR;
	std::unique_ptr<IConfigurationSettingsPersistenceMgr> m_apCfg;
	
	void loadSettingsFromRegistry();
	void saveSettingsToRegistry();
	void writeTextToClipboard(const std::string& strText);
	void handleRecognizedText(const std::string& strText);

	// reference count for this COM object
	long m_lRefCount;

	// this controls whether or not this dialog(the search window) is visible 
	bool m_bShowThisWindow;
	// this controls whether OCR'ed text will be displayed in a message box or 
	// copied to the clipboard
	bool m_bDisplayOCRText;

	EOCRTextHandlingType m_eOCRTextHandlingType;
	std::string m_strOCRTextFileName;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_SRIRIMAGEVIEWERDLG_H__E470748B_090B_4F32_98FC_3E9918F2BBA3__INCLUDED_)
