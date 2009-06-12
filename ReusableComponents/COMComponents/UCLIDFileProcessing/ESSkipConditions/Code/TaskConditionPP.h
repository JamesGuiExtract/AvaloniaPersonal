// TaskConditionPP.h : Declaration of the CTaskConditionPP

#pragma once
#include "resource.h"       // main symbols
#include "ESSkipConditions.h"

#include <string>
using namespace std;

////////////////////////////////////////////////////////////////////////////////////////////////////
// CTaskConditionPP
////////////////////////////////////////////////////////////////////////////////////////////////////
class ATL_NO_VTABLE CTaskConditionPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CTaskConditionPP, &CLSID_TaskConditionPP>,
	public IPropertyPageImpl<CTaskConditionPP>,
	public CDialogImpl<CTaskConditionPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CTaskConditionPP();
	~CTaskConditionPP();

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();
	void FinalRelease();

	enum {IDD = IDD_TASKCONDITIONPP};

	DECLARE_REGISTRY_RESOURCEID(IDR_TASKCONDITIONPP)

	BEGIN_COM_MAP(CTaskConditionPP)
		COM_INTERFACE_ENTRY(IPropertyPage)
		COM_INTERFACE_ENTRY(IDispatch)
		COM_INTERFACE_ENTRY(ILicensedComponent)
	END_COM_MAP()

	BEGIN_MSG_MAP(CTaskConditionPP)
		COMMAND_HANDLER(IDC_COMBO_TASK_SELECT, CBN_SELCHANGE, OnSelChangeTask)
		COMMAND_HANDLER(IDC_BTN_CONFIGURE, BN_CLICKED, OnBnClickedBtnConfigure)
		CHAIN_MSG_MAP(IPropertyPageImpl<CTaskConditionPP>)
		MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	END_MSG_MAP()

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnSelChangeTask(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedBtnConfigure(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

// IPropertyPage
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	////////////
	// Variables
	////////////

	// Controls
	ATLControls::CComboBox m_cmbTasks;
	ATLControls::CButton m_btnConfigure;
	ATLControls::CStatic m_txtMustConfigure;
	ATLControls::CButton m_chkLogExceptions;

	// Stores association between registered task names and ProgIDs
	UCLID_COMUTILSLib::IStrToStrMapPtr m_ipTaskMap;

	///////////
	// Methods
	///////////

	// Retrieve and validate the task map
	IStrToStrMapPtr getTaskMap();

	// Retrieve a pointer an instance of the currently selected task
	IFileProcessingTaskPtr getSelectedTask();

	// Returns true if the task needs configuration
	bool updateRequiresConfig(IFileProcessingTaskPtr ipTask);
	
	void validateLicense();
};

OBJECT_ENTRY_AUTO(__uuidof(TaskConditionPP), CTaskConditionPP)