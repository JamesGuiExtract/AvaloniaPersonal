
#pragma once

#include "resource.h"       // main symbols
#include "FileProcessors.h"

#include <string>

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
	END_MSG_MAP()

	// IPropertyPage
	STDMETHOD(Apply)(void);

	// Message handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);

private:
	// UI controls
	ATLControls::CComboBox m_cmbActionName;
	ATLControls::CComboBox m_cmbActionStatus;

	// Methods to convert action status from string to enum and vice-a-versa
	// This functionality will be moved to UCLIDFileProcessing DLL eventually
	EActionStatus getActionStatus(const std::string& strActionStatus) const;
	const std::string& getActionStatus(EActionStatus eActionStatus) const;
};

OBJECT_ENTRY_AUTO(__uuidof(SetActionStatusFileProcessorPP), CSetActionStatusFileProcessorPP)
