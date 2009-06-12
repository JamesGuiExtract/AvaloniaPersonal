// OutputToVOAPP.h : Declaration of the COutputToVOAPP

#pragma once

#include "resource.h"       // main symbols

EXTERN_C const CLSID CLSID_OutputToVOAPP;

/////////////////////////////////////////////////////////////////////////////
// COutputToVOAPP
class ATL_NO_VTABLE COutputToVOAPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<COutputToVOAPP, &CLSID_OutputToVOAPP>,
	public IPropertyPageImpl<COutputToVOAPP>,
	public CDialogImpl<COutputToVOAPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	COutputToVOAPP(); 

	enum {IDD = IDD_OUTPUTTOVOAPP};

DECLARE_REGISTRY_RESOURCEID(IDR_OUTPUTTOVOAPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(COutputToVOAPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(COutputToVOAPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<COutputToVOAPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_BTN_BROWSE_FILE, BN_CLICKED, OnClickedBtnBrowseFile)
	COMMAND_HANDLER(IDC_BTN_SELECT_DOC_TAG, BN_CLICKED, OnClickedSelectDocTag)
END_MSG_MAP()

public:
// IPropertyPage
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnClickedBtnBrowseFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedSelectDocTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
	// controls
	ATLControls::CEdit m_editFileName;
	ATLControls::CButton m_btnBrowse;
	ATLControls::CButton m_btnSelectDocTag;

	// helper functions
	void validateLicense();

};
