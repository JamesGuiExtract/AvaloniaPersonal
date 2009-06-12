// ModifyAttributeValuePP.h : Declaration of the CModifyAttributeValuePP

#pragma once

#include "resource.h"       // main symbols

EXTERN_C const CLSID CLSID_ModifyAttributeValuePP;

/////////////////////////////////////////////////////////////////////////////
// CModifyAttributeValuePP
class ATL_NO_VTABLE CModifyAttributeValuePP : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CModifyAttributeValuePP, &CLSID_ModifyAttributeValuePP>,
	public IPropertyPageImpl<CModifyAttributeValuePP>,
	public CDialogImpl<CModifyAttributeValuePP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CModifyAttributeValuePP();

	enum {IDD = IDD_MODIFYATTRIBUTEVALUE};

DECLARE_REGISTRY_RESOURCEID(IDR_MODIFYATTRIBUTEVALUEPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CModifyAttributeValuePP)
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CModifyAttributeValuePP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CModifyAttributeValuePP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_CHECK_UPDATENAME, BN_CLICKED, OnClickedCheckUpdateName)
	COMMAND_HANDLER(IDC_CHECK_UPDATEVALUE, BN_CLICKED, OnClickedCheckUpdateValue)
	COMMAND_HANDLER(IDC_CHECK_UPDATETYPE, BN_CLICKED, OnClickedCheckUpdateType)
	COMMAND_HANDLER(IDC_RADIO_CREATE_SUB_ATTRIBUTE, BN_CLICKED, OnClickedRadioCreateSubAttribute)
	COMMAND_HANDLER(IDC_RADIO_MODIFY_SELECTED, BN_CLICKED, OnClickedModifySelected)
END_MSG_MAP()

public:
// IPropertyPage
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnClickedCheckUpdateName(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedCheckUpdateValue(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedCheckUpdateType(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioCreateSubAttribute(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedModifySelected(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);


private:
	/////////////
	// Variables
	/////////////

	// Dialog controls
	bool m_bUpdateName;
	bool m_bUpdateValue;
	bool m_bUpdateType;

	ATLControls::CEdit m_editAttributeQuery;
	ATLControls::CEdit m_editAttributeName;
	ATLControls::CEdit m_editAttributeValue;
	ATLControls::CEdit m_editAttributeType;
	ATLControls::CButton m_radioModifySelected;
	ATLControls::CButton m_radioCreateSubAttr;

	// Check licensing
	void validateLicense();
};
