#pragma once

#include "resource.h"       // main symbols
#include "FileProcessors.h"

#include <ImageButtonWithStyle.h>

#include <string>

using namespace std;

//--------------------------------------------------------------------------------------------------
// CSetActionStatusFileProcessorPP
//--------------------------------------------------------------------------------------------------
class ATL_NO_VTABLE CSetActionStatusFileProcessorPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CSetActionStatusFileProcessorPP, &CLSID_SetActionStatusFileProcessorPP>,
	public IPropertyPageImpl<CSetActionStatusFileProcessorPP>,
	public CDialogImpl<CSetActionStatusFileProcessorPP>
{
public:
	CSetActionStatusFileProcessorPP();
	~CSetActionStatusFileProcessorPP();

	DECLARE_PROTECT_FINAL_CONSTRUCT()
	HRESULT FinalConstruct();
	void FinalRelease();

	enum {IDD = IDD_SETACTIONSTATUSFILEPROCESSORPP};

	DECLARE_REGISTRY_RESOURCEID(IDR_SETACTIONSTATUSFILEPROCESSORPP)

	BEGIN_COM_MAP(CSetActionStatusFileProcessorPP)
		COM_INTERFACE_ENTRY(IPropertyPage)
	END_COM_MAP()

	BEGIN_MSG_MAP(CSetActionStatusFileProcessorPP)
		MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
		CHAIN_MSG_MAP(IPropertyPageImpl<CSetActionStatusFileProcessorPP>)
		COMMAND_HANDLER(IDC_COMBO_ACTION, CBN_SELENDCANCEL, OnCbnSelEndCancelCmbActionName)
		COMMAND_HANDLER(IDC_BTN_ACTION_TAG, BN_CLICKED, OnClickedBtnActionTag)
		REFLECT_NOTIFICATIONS()
	END_MSG_MAP()

	// IPropertyPage
	STDMETHOD(Apply)(void);

	// Message handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnCbnSelEndCancelCmbActionName(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnActionTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:

	// Action name selection
	DWORD m_dwActionSel;

	// UI controls
	ATLControls::CComboBox m_cmbActionName;
	CImageButtonWithStyle m_btnActionTag;
	ATLControls::CComboBox m_cmbActionStatus;

	// Gets the user-specified name of the action
	string getActionName();
};

OBJECT_ENTRY_AUTO(__uuidof(SetActionStatusFileProcessorPP), CSetActionStatusFileProcessorPP)
