// StringTokenizerModifierPP.h : Declaration of the CStringTokenizerModifierPP

#pragma once

#include "resource.h"       // main symbols

#include <XInfoTip.h>

EXTERN_C const CLSID CLSID_StringTokenizerModifierPP;

/////////////////////////////////////////////////////////////////////////////
// CStringTokenizerModifierPP
class ATL_NO_VTABLE CStringTokenizerModifierPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CStringTokenizerModifierPP, &CLSID_StringTokenizerModifierPP>,
	public IPropertyPageImpl<CStringTokenizerModifierPP>,
	public CDialogImpl<CStringTokenizerModifierPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CStringTokenizerModifierPP();
	~CStringTokenizerModifierPP();

	enum {IDD = IDD_STRINGTOKENIZERMODIFIERPP};

DECLARE_REGISTRY_RESOURCEID(IDR_STRINGTOKENIZERMODIFIERPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CStringTokenizerModifierPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CStringTokenizerModifierPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CStringTokenizerModifierPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_CMB_NUM_OF_TOKENS_TYPE, CBN_SELCHANGE, OnSelchangeComboNumOfTokensType)
	COMMAND_HANDLER(IDC_EXPR_INFO, BN_CLICKED, OnClickedExprInfo)
END_MSG_MAP()
// Handler prototypes:
//  LRESULT MessageHandler(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
//  LRESULT CommandHandler(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
//  LRESULT NotifyHandler(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);

// IPropertyPage
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnSelchangeComboNumOfTokensType(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedExprInfo(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
	ATLControls::CComboBox m_cmbNumOfTokensType;
	ATLControls::CEdit m_editNumOfTokens;
	ATLControls::CStatic m_picExprInfo;
	ATLControls::CEdit m_editDelimiter;

	CXInfoTip m_infoTip;

	bool storeDelimiter(UCLID_AFVALUEMODIFIERSLib::IStringTokenizerModifierPtr ipST);
	bool storeResultExpr(UCLID_AFVALUEMODIFIERSLib::IStringTokenizerModifierPtr);
	bool storeNumOfTokens(UCLID_AFVALUEMODIFIERSLib::IStringTokenizerModifierPtr);

	// ensure that this component is licensed
	void validateLicense();
};
