// ReturnAddrFinderPP.h : Declaration of the CReturnAddrFinderPP

#ifndef __RETURNADDRFINDERPP_H_
#define __RETURNADDRFINDERPP_H_

#include "resource.h"       // main symbols

EXTERN_C const CLSID CLSID_ReturnAddrFinderPP;

/////////////////////////////////////////////////////////////////////////////
// CReturnAddrFinderPP
class ATL_NO_VTABLE CReturnAddrFinderPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CReturnAddrFinderPP, &CLSID_ReturnAddrFinderPP>,
	public IPropertyPageImpl<CReturnAddrFinderPP>,
	public CDialogImpl<CReturnAddrFinderPP>
{
public:
	CReturnAddrFinderPP();

	enum {IDD = IDD_RETURNADDRFINDERPP};

DECLARE_REGISTRY_RESOURCEID(IDR_RETURNADDRFINDERPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CReturnAddrFinderPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
END_COM_MAP()

BEGIN_MSG_MAP(CReturnAddrFinderPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CReturnAddrFinderPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
END_MSG_MAP()

// IPropertyPage
	STDMETHOD(Apply)(void);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnClickedCheckFindNonReturnAddresses(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
// Methods

// Variables
	ATLControls::CButton m_chkFindNonReturnAddresses;
};

#endif //__RETURNADDRFINDERPP_H_
