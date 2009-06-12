// FindFromRSDPP.h : Declaration of the CFindFromRSDPP

#ifndef __FINDFROMRSDPP_H_
#define __FINDFROMRSDPP_H_

#include "resource.h"       // main symbols

EXTERN_C const CLSID CLSID_FindFromRSDPP;

/////////////////////////////////////////////////////////////////////////////
// CFindFromRSDPP
class ATL_NO_VTABLE CFindFromRSDPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CFindFromRSDPP, &CLSID_FindFromRSDPP>,
	public IPropertyPageImpl<CFindFromRSDPP>,
	public CDialogImpl<CFindFromRSDPP>
{
public:
	CFindFromRSDPP();

	enum {IDD = IDD_FINDFROMRSDPP};

DECLARE_REGISTRY_RESOURCEID(IDR_FINDFROMRSDPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CFindFromRSDPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
END_COM_MAP()

BEGIN_MSG_MAP(CFindFromRSDPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CFindFromRSDPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_BTN_BROWSE, BN_CLICKED, OnClickedBtnBrowse)
	COMMAND_HANDLER(IDC_BTN_SELECT_DOC_TAG, BN_CLICKED, OnClickedSelectDocTag)
END_MSG_MAP()
// Handler prototypes:
//  LRESULT MessageHandler(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
//  LRESULT CommandHandler(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
//  LRESULT NotifyHandler(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, 
		BOOL& bHandled);
	LRESULT OnClickedBtnBrowse(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedSelectDocTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

STDMETHOD(Apply)(void);

private:
	ATLControls::CEdit m_editAttributeName;
	ATLControls::CEdit m_editRSDFileName;
	ATLControls::CButton m_btnBrowse;
	ATLControls::CButton m_btnSelectDocTag;
};

#endif //__FINDFROMRSDPP_H_
