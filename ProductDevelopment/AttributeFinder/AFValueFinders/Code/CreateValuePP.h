// CreateValuePP.h : Declaration of the CCreateValuePP

#pragma once

#include "resource.h"       // main symbols

EXTERN_C const CLSID CLSID_CreateValuePP;

/////////////////////////////////////////////////////////////////////////////
// CCreateValuePP
class ATL_NO_VTABLE CCreateValuePP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CCreateValuePP, &CLSID_CreateValuePP>,
	public IPropertyPageImpl<CCreateValuePP>,
	public CDialogImpl<CCreateValuePP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CCreateValuePP();
	~CCreateValuePP();

	enum {IDD = IDD_CREATEVALUEPP};

DECLARE_REGISTRY_RESOURCEID(IDR_CREATEVALUEPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CCreateValuePP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
END_COM_MAP()

BEGIN_MSG_MAP(CCreateValuePP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CCreateValuePP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
END_MSG_MAP()
// Handler prototypes:
//  LRESULT MessageHandler(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
//  LRESULT CommandHandler(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
//  LRESULT NotifyHandler(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);


public:
// IPropertyPage
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, 
		BOOL& bHandled);

private:
	ATLControls::CEdit m_editValue;
	ATLControls::CEdit m_editType;

	// ensure that this component is licensed
	void validateLicense();
};
