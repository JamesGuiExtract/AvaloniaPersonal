// MoveAndModifyAttributesPP.h : Declaration of the CMoveAndModifyAttributesPP

#pragma once

#include "resource.h"       // main symbols

EXTERN_C const CLSID CLSID_MoveAndModifyAttributesPP;

/////////////////////////////////////////////////////////////////////////////
// CMoveAndModifyAttributesPP
class ATL_NO_VTABLE CMoveAndModifyAttributesPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CMoveAndModifyAttributesPP, &CLSID_MoveAndModifyAttributesPP>,
	public IPropertyPageImpl<CMoveAndModifyAttributesPP>,
	public CDialogImpl<CMoveAndModifyAttributesPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CMoveAndModifyAttributesPP();

	enum {IDD = IDD_MOVEANDMODIFYATTRIBUTESPP};

DECLARE_REGISTRY_RESOURCEID(IDR_MOVEANDMODIFYATTRIBUTESPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CMoveAndModifyAttributesPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CMoveAndModifyAttributesPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CMoveAndModifyAttributesPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_RADIO_MOVE_TO_ROOT, BN_CLICKED, OnClickedRadioMoveToRoot)
	COMMAND_HANDLER(IDC_RADIO_MOVE_TO_PARENT, BN_CLICKED, OnClickedRadioMoveToParent)
	COMMAND_HANDLER(IDC_RADIO_SPECIFY_NAME, BN_CLICKED, OnClickedRadioSpecifyName)
	COMMAND_HANDLER(IDC_RADIO_DO_NOT_CHANGE_NAME, BN_CLICKED, OnClickedRadioDoNotChangeName)
	COMMAND_HANDLER(IDC_RADIO_ROOT_NAME, BN_CLICKED, OnClickedRadioRootName)
	COMMAND_HANDLER(IDC_CHECK_ADD_SPECIFIED_TYPE, BN_CLICKED, OnClickedCheckAddSpecifiedType)
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
	LRESULT OnClickedRadioMoveToRoot(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioMoveToParent(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioSpecifyName(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioDoNotChangeName(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioRootName(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedCheckAddSpecifiedType(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:

	//////////////
	// Variables
	//////////////
	ATLControls::CEdit m_editAttributeQuery;

	ATLControls::CButton m_radioMoveToRoot;
	ATLControls::CButton m_radioMoveToParent;

	ATLControls::CButton m_radioDoNotChangeName;
	ATLControls::CButton m_radioUseRootOrParentName;
	ATLControls::CButton m_radioUseSpecifiedName;
	ATLControls::CEdit m_editSpecifiedName;

	ATLControls::CButton m_radioRetainType;
	ATLControls::CButton m_radioDoNotRetainType;
	ATLControls::CButton m_chkAddRootOrParentType;
	ATLControls::CButton m_chkAddNameToType;
	ATLControls::CButton m_chkAddSpecifiedType;
	ATLControls::CEdit m_editSpecifiedType;

	ATLControls::CButton m_chkDeleteRootOrParentIfAllChildrenMoved;

	///////////
	// Methods
	///////////

	// depends on which movement (i.e. move to root or move to parent) is picked
	// by the user, update the display text for radio buttons and check boxes
	void updateDisplayText();

	// Check licensing
	void validateLicense();
	
};
