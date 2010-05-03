// RegExprIVPP.h : Declaration of the CRegExprIVPP

#pragma once
#include "resource.h"       // main symbols

EXTERN_C const CLSID CLSID_RegExprIVPP;

/////////////////////////////////////////////////////////////////////////////
// CRegExprIVPP
class ATL_NO_VTABLE CRegExprIVPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CRegExprIVPP, &CLSID_RegExprIVPP>,
	public IPropertyPageImpl<CRegExprIVPP>,
	public CDialogImpl<CRegExprIVPP>
{
public:
	CRegExprIVPP();

	enum {IDD = IDD_REGEXPRIVPP};

DECLARE_REGISTRY_RESOURCEID(IDR_REGEXPRIVPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CRegExprIVPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
END_COM_MAP()

BEGIN_MSG_MAP(CRegExprIVPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CRegExprIVPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
END_MSG_MAP()

	STDMETHOD(Apply)(void);
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
};
