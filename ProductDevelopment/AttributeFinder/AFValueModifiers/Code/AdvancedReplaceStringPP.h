// AdvancedReplaceStringPP.h : Declaration of the CAdvancedReplaceStringPP

#pragma once

#include "resource.h"       // main symbols

#include <XInfoTip.h>
#include <string>

EXTERN_C const CLSID CLSID_AdvancedReplaceStringPP;

/////////////////////////////////////////////////////////////////////////////
// CAdvancedReplaceStringPP
class ATL_NO_VTABLE CAdvancedReplaceStringPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CAdvancedReplaceStringPP, &CLSID_AdvancedReplaceStringPP>,
	public IPropertyPageImpl<CAdvancedReplaceStringPP>,
	public CDialogImpl<CAdvancedReplaceStringPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CAdvancedReplaceStringPP();
	~CAdvancedReplaceStringPP();

	enum {IDD = IDD_ADVANCEDREPLACESTRINGPP};

DECLARE_REGISTRY_RESOURCEID(IDR_ADVANCEDREPLACESTRINGPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CAdvancedReplaceStringPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CAdvancedReplaceStringPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CAdvancedReplaceStringPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_RADIO_ALL, BN_CLICKED, OnClickedRadioAll)
	COMMAND_HANDLER(IDC_RADIO_FIRST, BN_CLICKED, OnClickedRadioFirst)
	COMMAND_HANDLER(IDC_RADIO_LAST, BN_CLICKED, OnClickedRadioLast)
	COMMAND_HANDLER(IDC_RADIO_SPECIFIED, BN_CLICKED, OnClickedRadioSpecified)
	COMMAND_HANDLER(IDC_BTN_SELECT_FIND_DOC_TAG, BN_CLICKED, OnClickedSelectFindDocTag)
	COMMAND_HANDLER(IDC_BTN_SELECT_REPLACE_DOC_TAG, BN_CLICKED, OnClickedSelectReplaceDocTag)
	COMMAND_HANDLER(IDC_BTN_BROWSE_FIND_FILE, BN_CLICKED, OnClickedBrowseFindFile)
	COMMAND_HANDLER(IDC_BTN_BROWSE_REPLACE_FILE, BN_CLICKED, OnClickedBrowseReplaceFile)
	COMMAND_HANDLER(IDC_DYNAMIC_FIND_HELP, STN_CLICKED, OnClickedDynamicFindHelp)
	COMMAND_HANDLER(IDC_DYNAMIC_REPLACE_HELP, STN_CLICKED, OnClickedDynamicReplaceHelp)
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
	LRESULT OnClickedRadioAll(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioFirst(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioLast(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioSpecified(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedSelectFindDocTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedSelectReplaceDocTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBrowseFindFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBrowseReplaceFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedDynamicFindHelp(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedDynamicReplaceHelp(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
	ATLControls::CButton m_btnSelectFindDocTag;
	ATLControls::CButton m_btnSelectReplaceDocTag;
	ATLControls::CEdit m_editFindString;
	ATLControls::CEdit m_editReplaceString;

	CXInfoTip m_infoTip;

	// contains the file header string got from MiscUtils
	std::string m_strFileHeader;

	bool storeOccurrence(UCLID_AFVALUEMODIFIERSLib::IAdvancedReplaceStringPtr ipARS);
	bool storeToBeReplaced(UCLID_AFVALUEMODIFIERSLib::IAdvancedReplaceStringPtr ipARS);

	// update the state of specified occurrence edit box, i.e. whether
	// it shall be enable or disabled
	void updateSpecifiedOccEdit();

	// ensure that this component is licensed
	void validateLicense();
};
