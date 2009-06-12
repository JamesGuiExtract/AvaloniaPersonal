// FileExistencePP.h : Declaration of the CFileExistencePP

#pragma once

#include "resource.h"       // main symbols

#include <ImageButtonWithStyle.h>
#include <string>

EXTERN_C const CLSID CLSID_FileExistencePP;

/////////////////////////////////////////////////////////////////////////////
// CFileExistencePP
class ATL_NO_VTABLE CFileExistencePP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CFileExistencePP, &CLSID_FileExistencePP>,
	public IPropertyPageImpl<CFileExistencePP>,
	public CDialogImpl<CFileExistencePP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CFileExistencePP();

	enum {IDD = IDD_FILEEXISTENCEPP};

DECLARE_REGISTRY_RESOURCEID(IDR_FILEEXISTENCEPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CFileExistencePP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CFileExistencePP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CFileExistencePP>)
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
	ATLControls::CComboBox m_cmbDoes;
	ATLControls::CEdit m_FileName;
	ATLControls::CButton m_btnBrowse;
	CImageButtonWithStyle m_btnSelectDocTag;

	///////////
	// Methods
	///////////
	// Save the file FAM condition
	bool saveFileFAMCondition(EXTRACT_FAMCONDITIONSLib::IFileExistenceFAMConditionPtr ipFileExistence);

	// ensure that this component is licensed
	void validateLicense();
};
