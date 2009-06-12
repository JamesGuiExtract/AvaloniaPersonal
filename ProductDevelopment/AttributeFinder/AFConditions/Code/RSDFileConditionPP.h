// RSDFileConditionPP.h : Declaration of the CRSDFileConditionPP

#ifndef __RSDFILECONDITIONPP_H_
#define __RSDFILECONDITIONPP_H_

#include "resource.h"       // main symbols
EXTERN_C const CLSID CLSID_RSDFileConditionPP;

/////////////////////////////////////////////////////////////////////////////
// CRSDFileConditionPP
class ATL_NO_VTABLE CRSDFileConditionPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CRSDFileConditionPP, &CLSID_RSDFileConditionPP>,
	public IPropertyPageImpl<CRSDFileConditionPP>,
	public CDialogImpl<CRSDFileConditionPP>
{
public:
	CRSDFileConditionPP();

	enum {IDD = IDD_RSDFILECONDITIONPP};

DECLARE_REGISTRY_RESOURCEID(IDR_RSDFILECONDITIONPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CRSDFileConditionPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
END_COM_MAP()

BEGIN_MSG_MAP(CRSDFileConditionPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CRSDFileConditionPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_BTN_BROWSE, BN_CLICKED, OnClickedBtnBrowse)
	COMMAND_HANDLER(IDC_BTN_SELECT_DOC_TAG, BN_CLICKED, OnClickedSelectDocTag)
END_MSG_MAP()

// IPropertyPage
	STDMETHOD(Apply)(void);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnClickedBtnBrowse(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedSelectDocTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
	///////////////
	// Methods
	///////////////

	///////////////
	// Variables
	///////////////
	ATLControls::CEdit m_editRSDFileName;
	ATLControls::CButton m_btnBrowse;
	ATLControls::CButton m_btnSelectDocTag;

	IAFUtilityPtr m_ipAFUtility;
};

#endif //__RSDFILECONDITIONPP_H_
