// RSDSplitterPP.h : Declaration of the CRSDSplitterPP

#pragma once

#include "resource.h"       // main symbols

EXTERN_C const CLSID CLSID_RSDSplitterPP;

/////////////////////////////////////////////////////////////////////////////
// CRSDSplitterPP
class ATL_NO_VTABLE CRSDSplitterPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CRSDSplitterPP, &CLSID_RSDSplitterPP>,
	public IPropertyPageImpl<CRSDSplitterPP>,
	public CDialogImpl<CRSDSplitterPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CRSDSplitterPP();
	~CRSDSplitterPP();

	enum {IDD = IDD_RSDSPLITTERPP};

DECLARE_REGISTRY_RESOURCEID(IDR_RSDSPLITTERPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CRSDSplitterPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CRSDSplitterPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CRSDSplitterPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_BTN_BROWSE_RSD, BN_CLICKED, OnClickedBtnBrowse)
	COMMAND_HANDLER(IDC_BTN_SELECT_DOC_TAG, BN_CLICKED, OnClickedSelectDocTag)
END_MSG_MAP()
// Handler prototypes:
//  LRESULT MessageHandler(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
//  LRESULT CommandHandler(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
//  LRESULT NotifyHandler(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);

// IProperty Page
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnClickedBtnBrowse(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedSelectDocTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
	ATLControls::CEdit m_editRSDFileName;
	ATLControls::CButton m_btnSelectDocTag;
	
	// ensure that this component is licensed
	void validateLicense();
};
