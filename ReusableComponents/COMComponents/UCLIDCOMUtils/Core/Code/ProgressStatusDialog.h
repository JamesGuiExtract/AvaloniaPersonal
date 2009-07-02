// ProgressStatusDialog.h : Declaration of the CProgressStatusDialog

#pragma once

#include "resource.h"       // main symbols
#include "UCLIDCOMUtils.h"
#include "ProgressStatusMFCDlg.h"

#include <memory>
#include <string>

using namespace std;

//--------------------------------------------------------------------------------------------------
// CProgressStatusDialog
//--------------------------------------------------------------------------------------------------
class ATL_NO_VTABLE CProgressStatusDialog :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CProgressStatusDialog, &CLSID_ProgressStatusDialog>,
	public ISupportErrorInfo,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IProgressStatusDialog, &IID_IProgressStatusDialog, &LIBID_UCLID_COMUTILSLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
public:
	// Standard constructor/destructor
	CProgressStatusDialog();
	~CProgressStatusDialog();

	// COM level "constructor"/"destructor"
	HRESULT FinalConstruct();
	void FinalRelease();

DECLARE_REGISTRY_RESOURCEID(IDR_PROGRESSSTATUSDIALOG)

BEGIN_COM_MAP(CProgressStatusDialog)
	COM_INTERFACE_ENTRY(IProgressStatusDialog)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY2(IDispatch, IProgressStatusDialog)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
END_COM_MAP()

	DECLARE_PROTECT_FINAL_CONSTRUCT()

public:

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IProgressStatusDialog
	STDMETHOD(ShowModelessDialog)(/*[in]*/ HANDLE hWndParent, /*[in]*/ BSTR strWindowTitle, 
		/*[in]*/ IProgressStatus *pProgressStatus, /*[in]*/ long nNumProgressLevels, 
		/*[in]*/ long nDelayBetweenRefreshes, /*[in]*/ VARIANT_BOOL bShowCloseButton, 
		/*[in]*/ HANDLE hStopEvent, /*[out, retval]*/ long *phWndProgressStatusDialog);
	STDMETHOD(get_ProgressStatusObject)(/*[out, retval]*/ IProgressStatus **pVal);
	STDMETHOD(put_ProgressStatusObject)(/*[in]*/ IProgressStatus *newVal);
	STDMETHOD(get_Title)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_Title)(/*[in]*/ BSTR newVal);
	STDMETHOD(Close)();

private:
	// The actual MFC progress status dialog pointer
	std::auto_ptr<CProgressStatusMFCDlg> m_apProgressStatusMFCDlg;

	// Check license
	void validateLicense();

	void setTitle(string strTitle);

	void setProgressStatusObject(const UCLID_COMUTILSLib::IProgressStatusPtr& ipVal);
};
//--------------------------------------------------------------------------------------------------

OBJECT_ENTRY_AUTO(__uuidof(ProgressStatusDialog), CProgressStatusDialog)
