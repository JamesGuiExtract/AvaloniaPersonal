// LaunchAppFileProcessorPP.h : Declaration of the CLaunchAppFileProcessorPP
// LaunchAppFileProcessorPP.h : Declaration of the CLaunchAppFileProcessorPP

#pragma once

#include "resource.h"       // main symbols

#include <ImageButtonWithStyle.h>
#include <string>

using namespace std;

EXTERN_C const CLSID CLSID_LaunchAppFileProcessorPP;

/////////////////////////////////////////////////////////////////////////////
// CLaunchAppFileProcessorPP
class ATL_NO_VTABLE CLaunchAppFileProcessorPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CLaunchAppFileProcessorPP, &CLSID_LaunchAppFileProcessorPP>,
	public IPropertyPageImpl<CLaunchAppFileProcessorPP>,
	public CDialogImpl<CLaunchAppFileProcessorPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CLaunchAppFileProcessorPP();

	enum {IDD = IDD_LAUNCHAPPFILEPROCESSORPP};

DECLARE_REGISTRY_RESOURCEID(IDR_LAUNCHAPPFILEPROCESSORPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CLaunchAppFileProcessorPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CLaunchAppFileProcessorPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CLaunchAppFileProcessorPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_BTN_SELECT_CMD_LINE_DOC_TAG, BN_CLICKED, OnClickedBtnCmdLineSelectTag)
	COMMAND_HANDLER(IDC_BTN_BROWSE_CMD_LINE, BN_CLICKED, OnClickedBtnCmdLineBrowse)
	COMMAND_HANDLER(IDC_BTN_SELECT_WORKING_DIR_DOC_TAG, BN_CLICKED, OnClickedBtnWorkingDirSelectTag)
	COMMAND_HANDLER(IDC_BTN_BROWSE_WORKING_DIR, BN_CLICKED, OnClickedBtnWorkingDirBrowse)
	COMMAND_HANDLER(IDC_BTN_PARAMETERS_DOC_TAG, BN_CLICKED, OnClickedBtnParametersSelectTag)
	COMMAND_HANDLER(IDC_BTN_PARAMETERS_BROWSE, BN_CLICKED, OnClickedBtnParametersBrowse)
	COMMAND_HANDLER(IDC_CHECK_PROP_ERRORS, BN_CLICKED, OnClickedCheckPropogateErrors)
	// REFLECT_NOTIFICATIONS needed by ImageButtonWithSytle
	REFLECT_NOTIFICATIONS()
END_MSG_MAP()

// IProperty Page
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnClickedRadioMove(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioCopy(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedRadioDelete(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnCmdLineSelectTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnCmdLineBrowse(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnWorkingDirSelectTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnWorkingDirBrowse(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnParametersSelectTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnParametersBrowse(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedCheckPropogateErrors(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
	///////////
	// Variables
	////////////	
	ATLControls::CEdit m_editCmdLine;
	CImageButtonWithStyle m_btnCmdLineSelectTag;
	ATLControls::CButton m_btnCmdLineBrowse;

	ATLControls::CEdit m_editWorkingDir;
	CImageButtonWithStyle m_btnWorkingDirSelectTag;
	ATLControls::CButton m_btnWorkingDirBrowse;

	ATLControls::CEdit m_editParameters;
	CImageButtonWithStyle m_btnParametersSelectTag;
	ATLControls::CButton m_btnParametersBrowse;

	ATLControls::CButton m_radioBlocking;
	ATLControls::CButton m_radioNonBlocking;

	ATLControls::CButton m_checkPropErrors;

	///////////
	// Methods
	///////////
	const string chooseFile();

	// ensure that this component is licensed
	void validateLicense();
};