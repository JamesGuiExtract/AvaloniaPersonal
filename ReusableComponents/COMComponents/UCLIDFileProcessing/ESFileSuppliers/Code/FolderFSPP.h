// CFolderFSPP.h : Declaration of the CFolderFSPP

#pragma once

#include "resource.h"       // main symbols
#include "ESFileSuppliers.h"

#include <vector>
#include <string>

#include <QuickMenuChooser.h>
#include <ImageButtonWithStyle.h>
#include "FileSupplierConfigMgr.h"

EXTERN_C const CLSID CLSID_FolderFSPP;

/////////////////////////////////////////////////////////////////////////////
// CFolderFSPP
class ATL_NO_VTABLE CFolderFSPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CFolderFSPP, &CLSID_FolderFSPP>,
	public IPropertyPageImpl<CFolderFSPP>,
	public CDialogImpl<CFolderFSPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CFolderFSPP();

	enum {IDD = IDD_FOLDERFSPP};

DECLARE_REGISTRY_RESOURCEID(IDR_FOLDERFSPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CFolderFSPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CFolderFSPP)
	CHAIN_MSG_MAP(IPropertyPageImpl<CFolderFSPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_CHK_CONTINUOUS, BN_CLICKED, OnBnClickedBtnContinuous)
	COMMAND_HANDLER(IDC_BTN_SELECT_FOLDER_DOC_TAG, BN_CLICKED, OnBnClickedBtnSelectFolderTag)
	COMMAND_HANDLER(IDC_CMB_FOLDER, CBN_SELENDCANCEL, OnCbnSelendcancelCmbFolder)
	COMMAND_HANDLER(IDC_BTN_BROWSE, BN_CLICKED, OnBnClickedBtnBrowse)
	// REFLECT_NOTIFICATIONS needed by ImageButtonWithSytle
	REFLECT_NOTIFICATIONS()
END_MSG_MAP()

// IProperty Page
	STDMETHOD(Apply)(void);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// Windows Message Handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnBnClickedBtnContinuous(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);
	LRESULT OnBnClickedBtnSelectFolderTag(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);
	LRESULT OnCbnSelendcancelCmbFolder(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);
	LRESULT OnBnClickedBtnBrowse(WORD /*wNotifyCode*/, WORD /*wID*/, HWND /*hWndCtl*/, BOOL& /*bHandled*/);
private:
	///////////
	// Variables
	////////////
	CComboBox	m_cmbFolder;
	CImageButtonWithStyle m_btnSelectFolderDocTag;
	CComboBox	m_cmbFileExtension;
	CButton		m_chkRecursive;
	CButton		m_chkContinuousProcess;
	CButton		m_chkAdded;
	CButton		m_chkModified;
	CButton		m_chkTargetForRenameOrMove;
	CButton		m_chkNoExistingFiles;

	QuickMenuChooser m_menuSelectFolderDocTag;

	// Used to hold the start and end of the current selection in the m_cmbFolder edit box
	DWORD m_dwSel;

	// Configuration managers to get and save the history for the folders and the file extensions
	std::unique_ptr<IConfigurationSettingsPersistenceMgr> ma_pUserCfgMgr;
	std::unique_ptr<FileSupplierConfigMgr> ma_pCfgMgr;

	///////////
	// Methods
	///////////
	// Update the button states
	void updateButtons();

	// Sets the file extension combo box list from registry and histroy in the registry
	void setFileExtensions();

	// Sets the history list of the folder combo box from the registry
	void setFolderHistory();

	// Saves the folder history list to the registry
	void saveFolderHistory();

	// Saves the file extension history to the registry
	void saveFileExtensionHistory();

	// Saves the current folder to history list is not there already
	void pushCurrentFolderToHistory();

	// Saves the current file extention to history
	void pushCurrentFileExtensionToHistory();

	// Save the folder setting
	bool saveFolderFS(EXTRACT_FILESUPPLIERSLib::IFolderFSPtr ipFileSup);

	// ensure that this component is licensed
	void validateLicense();
};

OBJECT_ENTRY_AUTO(__uuidof(FolderFSPP), CFolderFSPP)
