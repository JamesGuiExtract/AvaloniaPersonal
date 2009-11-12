//==================================================================================================
//
// COPYRIGHT (c) 2009 EXTRACT SYSTEMS, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ArchiveRestoreTaskPP.h
//
// PURPOSE:	Header file for Archive Restore file processing property page
//
// AUTHORS:	Jeff Shergalis
//
//==================================================================================================

#pragma once
#include "resource.h"       // main symbols
#include "FileProcessors.h"

#include <ImageButtonWithStyle.h>

#include <string>

EXTERN_C const CLSID CLSID_ArchiveRestoreTaskPP;

/////////////////////////////////////////////////////////////////////////////
// CArchiveRestoreTaskPP
class ATL_NO_VTABLE CArchiveRestoreTaskPP :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CArchiveRestoreTaskPP, &CLSID_ArchiveRestoreTaskPP>,
	public IPropertyPageImpl<CArchiveRestoreTaskPP>,
	public CDialogImpl<CArchiveRestoreTaskPP>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CArchiveRestoreTaskPP();
	~CArchiveRestoreTaskPP();

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	enum {IDD = IDD_ARCHIVE_RESTORE_TASKPP};

	DECLARE_REGISTRY_RESOURCEID(IDR_ARCHIVE_RESTORE_TASKPP)

	BEGIN_COM_MAP(CArchiveRestoreTaskPP)
		COM_INTERFACE_ENTRY(IPropertyPage)
	END_COM_MAP()

	BEGIN_MSG_MAP(CArchiveRestoreTaskPP)
		CHAIN_MSG_MAP(IPropertyPageImpl<CArchiveRestoreTaskPP>)
		MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
		COMMAND_HANDLER(IDC_RADIO_ARCHIVE, BN_CLICKED, OnClickedBtnRadioOperation)
		COMMAND_HANDLER(IDC_RADIO_RESTORE, BN_CLICKED, OnClickedBtnRadioOperation)
		COMMAND_HANDLER(IDC_BTN_ARCHIVE_FOLDER_DOC_TAG, BN_CLICKED, OnClickedBtnArchiveDocTag)
		COMMAND_HANDLER(IDC_BTN_ARCHIVE_FOLDER_BROWSE, BN_CLICKED, OnClickedBtnArchiveBrowse)
		COMMAND_HANDLER(IDC_BTN_ARCHIVE_FILE_DOC_TAG, BN_CLICKED, OnClickedBtnFileDocTag)
		COMMAND_HANDLER(IDC_BTN_ARCHIVE_FILE_BROWSE, BN_CLICKED, OnClickedBtnFileBrowse)
		COMMAND_HANDLER(IDC_EDIT_ARCHIVE_FOLDER, EN_CHANGE, OnEnChangeEditArchiveFolder)
		// REFLECT_NOTIFICATIONS needed by ImageButtonWithSytle
		REFLECT_NOTIFICATIONS()
	END_MSG_MAP()

// Handler prototypes:
//  LRESULT MessageHandler(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
//  LRESULT CommandHandler(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
//  LRESULT NotifyHandler(int idCtrl, LPNMHDR pnmh, BOOL& bHandled);

	// IPropertyPage
	STDMETHOD(Apply)(void);

	// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

	// Message handlers
	LRESULT OnInitDialog(UINT uMsg, WPARAM wParam, LPARAM lParam, BOOL& bHandled);
	LRESULT OnClickedBtnRadioOperation(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnArchiveDocTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnArchiveBrowse(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnFileDocTag(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnClickedBtnFileBrowse(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);
	LRESULT OnEnChangeEditArchiveFolder(WORD wNotifyCode, WORD wID, HWND hWndCtl, BOOL& bHandled);

private:
	// Ensures that this component is licensed
	void	validateLicense();

	// Updates the labels and enables/disables the checkboxes based on
	// whether the current operation is archive or restore
	void	updateWindowState();

	// Loads the tags from the specified file and adds them to the combo box
	void	loadTagsFromFile(const std::string& strTagsFile);

	// Saves the current tag to the tags file.
	// If the tag already exists in the file but with a different
	// casing, the casing in the file will be replaced
	void	saveTagToTagsFile(const std::string& strTag, const std::string& strTagsFile);

	// Builds the string for the tag file name based on the specified
	// archive directory
	std::string	getTagFile(const std::string& strArchiveDirectory);

	///////
	// Data
	///////
	// Various controls
	ATLControls::CButton m_radioArchive;
	ATLControls::CButton m_radioRestore;
	ATLControls::CEdit m_editArchiveFolder;
	CImageButtonWithStyle m_btnArchiveFolderDocTags;
	ATLControls::CButton m_btnArchiveFolderBrowse;
	ATLControls::CStatic m_labelFileTag;
	ATLControls::CComboBox m_cmbFileTag;
	ATLControls::CStatic m_labelSourceFile;
	ATLControls::CEdit m_editSourceFile;
	CImageButtonWithStyle m_btnSourceFileDoctTags;
	ATLControls::CButton m_btnSourceFileBrowse;
	ATLControls::CButton m_checkDeleteFile;
	ATLControls::CStatic m_groupOverwriteOrFail;
	ATLControls::CButton m_radioOverwriteFile;
	ATLControls::CButton m_radioFailFile;
};

OBJECT_ENTRY_AUTO(__uuidof(ArchiveRestoreTaskPP), CArchiveRestoreTaskPP)