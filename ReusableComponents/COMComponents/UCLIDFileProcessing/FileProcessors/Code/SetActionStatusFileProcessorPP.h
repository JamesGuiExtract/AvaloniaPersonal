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
        COMMAND_HANDLER(IDC_BTN_DOCUMENT_TAG, BN_CLICKED, OnBnClickedBtnDocumentTag)
        COMMAND_HANDLER(IDC_BTN_FILE_SELECTOR, BN_CLICKED, OnBnClickedBtnFileSelector)
		COMMAND_HANDLER(IDC_COMBO_WORKFLOW, CBN_SELENDOK, OnCbnSelendokComboWorkflow)
        COMMAND_HANDLER(IDC_BTN_TARGET_USER_TAG, BN_CLICKED, OnClickedBtnTargetUserTag)
        COMMAND_HANDLER(IDC_COMBO_SELECT_USER, CBN_SELENDCANCEL, OnCbnSelEndCancelCmbTargetUser)
        REFLECT_NOTIFICATIONS()
	END_MSG_MAP()

    // IPropertyPage
    STDMETHOD(Apply)(void);

private:
    // Message handlers
    LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
    LRESULT OnCbnSelEndCancelCmbActionName(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
    LRESULT OnClickedBtnActionTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
    LRESULT OnCbnSelEndCancelCmbTargetUser(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
    LRESULT OnClickedBtnTargetUserTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

    LRESULT OnBnClickedBtnDocumentTag(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);
    LRESULT OnBnClickedBtnFileSelector(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);
	LRESULT OnCbnSelendokComboWorkflow(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);

    // Gets the user-specified name of the action
    string getActionName();
    string GetDocumentName();
    string getTargetUserName(); 

	void loadActionCombo(string strActionName);
    void loadTargetUserCombo(string strTargetUser);

    void loadCombo(ATLControls::CComboBox combo, IStrToStrMapPtr ipDataMap);

    bool isValidTargetUser();

    // Action name selection
    DWORD m_dwActionSel;

	// Workflow selection
	DWORD m_dwWorkflowSel;

    DWORD m_dwTargetUserSel;

    // UI controls
    ATLControls::CComboBox m_cmbActionName;
    CImageButtonWithStyle m_btnActionTag;

    ATLControls::CComboBox m_cmbActionStatus;

    ATLControls::CEdit m_editDocumentName;
    CImageButtonWithStyle m_btnDocumentTag;

    ATLControls::CButton m_btnFileSelector;

    ATLControls::CButton m_radioBtnReportError;
    ATLControls::CButton m_radioBtnQueueFiles;

	ATLControls::CComboBox m_cmbWorkflow;

    ATLControls::CComboBox m_cmbTargetUser;
    CImageButtonWithStyle m_btnTargetUserTag;

	// This will be created using the ConnectLastUsedDBThisProcess
	IFileProcessingDBPtr m_ipFAMDB;
};

OBJECT_ENTRY_AUTO(__uuidof(SetActionStatusFileProcessorPP), CSetActionStatusFileProcessorPP)
