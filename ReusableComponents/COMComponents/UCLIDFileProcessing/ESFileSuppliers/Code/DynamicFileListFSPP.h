// CDynamicFileListFSPP.h : Declaration of the CDynamicFileListFSPP

#pragma once

#include "resource.h"       // main symbols
#include "ESFileSuppliers.h"

#include <ImageButtonWithStyle.h>

#include <vector>
#include <string>

EXTERN_C const CLSID CLSID_DynamicFileListFSPP;

/////////////////////////////////////////////////////////////////////////////
// CDynamicFileListFSPP
class ATL_NO_VTABLE CDynamicFileListFSPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CDynamicFileListFSPP, &CLSID_DynamicFileListFSPP>,
	public IPropertyPageImpl<CDynamicFileListFSPP>,
	public CDialogImpl<CDynamicFileListFSPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CDynamicFileListFSPP();

	enum {IDD = IDD_DYNAMICFILELISTFSPP};

DECLARE_REGISTRY_RESOURCEID(IDR_DYNAMICFILELISTFSPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CDynamicFileListFSPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CDynamicFileListFSPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CDynamicFileListFSPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_BTN_SELECT_DOC_TAG, BN_CLICKED, OnBnClickedBtnSelectDocTag)
	COMMAND_HANDLER(IDC_BTN_BROWSE_FILE, BN_CLICKED, OnBnClickedBtnBrowseFile)
	// REFLECT_NOTIFICATIONS needed by ImageButtonWithSytle
	REFLECT_NOTIFICATIONS()
END_MSG_MAP()

// IProperty Page
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnBnClickedBtnSelectDocTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnBnClickedBtnBrowseFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
	///////////
	// Variables
	////////////

	// individually defined list of files
	ATLControls::CEdit m_editFileName;
	ATLControls::CButton m_btnBrowse;
	CImageButtonWithStyle m_btnSelectDocTag;

	///////////
	// Methods
	///////////

	// Save the dynamic file list
	bool saveDynamicFileListFS(EXTRACT_FILESUPPLIERSLib::IDynamicFileListFSPtr ipDynamicFileListFS);

	// ensure that this component is licensed
	void validateLicense();
};

OBJECT_ENTRY_AUTO(__uuidof(DynamicFileListFSPP), CDynamicFileListFSPP)
