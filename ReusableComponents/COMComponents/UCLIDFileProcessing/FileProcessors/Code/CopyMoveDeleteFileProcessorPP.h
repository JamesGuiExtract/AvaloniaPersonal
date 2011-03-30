// CopyMoveDeleteFileProcessorPP.h : Declaration of the CCopyMoveDeleteFileProcessorPP

#pragma once

#include "resource.h"       // main symbols
#include "FileProcessorsConfigMgr.h"

#include <string>
#include <ImageButtonWithStyle.h>
#include <RegistryPersistenceMgr.h>

EXTERN_C const CLSID CLSID_CopyMoveDeleteFileProcessorPP;

/////////////////////////////////////////////////////////////////////////////
// CCopyMoveDeleteFileProcessorPP
class ATL_NO_VTABLE CCopyMoveDeleteFileProcessorPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CCopyMoveDeleteFileProcessorPP, &CLSID_CopyMoveDeleteFileProcessorPP>,
	public IPropertyPageImpl<CCopyMoveDeleteFileProcessorPP>,
	public CDialogImpl<CCopyMoveDeleteFileProcessorPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CCopyMoveDeleteFileProcessorPP();

	enum {IDD = IDD_COPYMOVEDELETEFILEPROCESSORPP};

DECLARE_REGISTRY_RESOURCEID(IDR_COPYMOVEDELETEFILEPROCESSORPP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CCopyMoveDeleteFileProcessorPP) 
	COM_INTERFACE_ENTRY(IPropertyPage)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

BEGIN_MSG_MAP(CCopyMoveDeleteFileProcessorPP)
	COMMAND_HANDLER(IDC_CMB_SRC_FILE, CBN_SELENDCANCEL, OnCbnSelEndCancelCmbSrcFile)
	COMMAND_HANDLER(IDC_CMB_DST_FILE, CBN_SELENDCANCEL, OnCbnSelEndCancelCmbDstFile)
	CHAIN_MSG_MAP(IPropertyPageImpl<CCopyMoveDeleteFileProcessorPP>)
	MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
	COMMAND_HANDLER(IDC_RADIO_MOVE, BN_CLICKED, OnClickedRadioMove)
	COMMAND_HANDLER(IDC_RADIO_COPY, BN_CLICKED, OnClickedRadioCopy)
	COMMAND_HANDLER(IDC_RADIO_DELETE, BN_CLICKED, OnClickedRadioDelete)
	COMMAND_HANDLER(IDC_BTN_SELECT_SRC_DOC_TAG, BN_CLICKED, OnClickedBtnSrcSelectTag)
	COMMAND_HANDLER(IDC_BTN_BROWSE_SRC, BN_CLICKED, OnClickedBtnSrcBrowse)
	COMMAND_HANDLER(IDC_BTN_SELECT_DST_DOC_TAG, BN_CLICKED, OnClickedBtnDstSelectTag)
	COMMAND_HANDLER(IDC_BTN_BROWSE_DST, BN_CLICKED, OnClickedBtnDstBrowse)
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
	LRESULT OnClickedBtnSrcSelectTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnSrcBrowse(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnDstSelectTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnDstBrowse(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnCbnSelEndCancelCmbSrcFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnCbnSelEndCancelCmbDstFile(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
	///////////
	// Variables
	////////////
	ATLControls::CButton m_radioMove;
	ATLControls::CButton m_radioCopy;
	ATLControls::CButton m_radioDelete;
	ATLControls::CButton m_btnAllowReadonly;
	ATLControls::CButton m_btnModifySourceDocName;

	ATLControls::CComboBox m_cmbSrc;
	CImageButtonWithStyle m_btnSrcSelectTag;
	ATLControls::CButton m_btnSrcBrowse;

	ATLControls::CComboBox m_cmbDst;
	CImageButtonWithStyle m_btnDstSelectTag;
	ATLControls::CButton m_btnDstBrowse;

	ATLControls::CButton m_btnCreateFolder;
	ATLControls::CButton m_radioSrcErr;
	ATLControls::CButton m_radioSrcSkip;
	ATLControls::CButton m_radioDstErr;
	ATLControls::CButton m_radioDstSkip;
	ATLControls::CButton m_radioDstOver;

	// Used to hold the start and end of the current selection in the m_cmbSrc combo box
	DWORD m_dwSelSrc;

	// Used to hold the start and end of the current selection in the m_cmbDst combo box
	DWORD m_dwSelDst;

	// Configuration managers to get and save the history for the source and destination file names
	std::unique_ptr<RegistryPersistenceMgr> ma_pUserCfgMgr;
	std::unique_ptr<FileProcessorsConfigMgr> ma_pCfgMgr;

	///////////
	// Methods
	///////////
	// Open the file dialog box to choose a file, the bIsSource flag is to control
	// whether to open a source file or a destination file
	const std::string chooseFile(bool bIsSource);

	// Set the history list of the two combo boxes from the registry
	// Set source list if bIsSource flag is true or destination list if false
	void setHistory(bool bIsSource);

	// Saves the history lists in combo boxes to the registry
	// Save source list if bIsSource flag is true or destination list if false
	void saveHistory(bool bIsSource);

	// Saves the current files in combo boxes to history list if not already there
	// the bIsSource flag is used control whether it is source file or destination file
	void pushCurrentFilesToHistory(bool bIsSource);

	// Updates the enabled state of the buttons using the current state of the delete radio button
	void updateEnabledState();
	
	// ensure that this component is licensed
	void validateLicense();
};
