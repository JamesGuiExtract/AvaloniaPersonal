// EntityNameSplitterPP.h : Declaration of the CEntityNameSplitterPP

#pragma once

#include "resource.h"       // main symbols

EXTERN_C const CLSID CLSID_EntityNameSplitterPP;

/////////////////////////////////////////////////////////////////////////////
// CEntityNameSplitterPP
class ATL_NO_VTABLE CEntityNameSplitterPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CEntityNameSplitterPP, &CLSID_EntityNameSplitterPP>,
	public IPropertyPageImpl<CEntityNameSplitterPP>,
	public CDialogImpl<CEntityNameSplitterPP>
{
public:
	CEntityNameSplitterPP();

	enum {IDD = IDD_ENTITYNAMESPLITTERPP};

DECLARE_REGISTRY_RESOURCEID(IDR_ENTITYNAMESPLITTERPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CEntityNameSplitterPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
END_COM_MAP()

BEGIN_MSG_MAP(CEntityNameSplitterPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CEntityNameSplitterPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_RADIO_IGNORE, BN_CLICKED, OnClickedRadioIgnoreAlias)
	COMMAND_HANDLER(IDC_RADIO_ATTRIBUTES, BN_CLICKED, OnClickedRadioAliasToAttribute)
	COMMAND_HANDLER(IDC_RADIO_SUBATTRIBUTES, BN_CLICKED, OnClickedRadioAliasToSubAttribute)
END_MSG_MAP()

// IPropertyPage
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnClickedRadioIgnoreAlias(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioAliasToAttribute(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioAliasToSubAttribute(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

// Handler prototypes:
//  LRESULT MessageHandler(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
//  LRESULT CommandHandler(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
//  LRESULT NotifyHandler(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);

private:
	// Current setting for alias handling
	EEntityAliasChoice	m_eAliasChoice;

	// Check licensing
	void validateLicense();
};
