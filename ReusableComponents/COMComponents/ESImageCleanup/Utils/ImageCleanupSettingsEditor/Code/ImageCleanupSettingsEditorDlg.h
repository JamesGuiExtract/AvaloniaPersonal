//============================================================================
//
// COPYRIGHT (c) 2007 - 2008 EXCTRACT SYSTEMS, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ImageCleanupSettingsEditorDlg.h
//
// PURPOSE:	Declaration of CImageCleanupSettingsEditorDlg class
//
// NOTES:	
//
// AUTHORS:	Jeff Shergalis
//
//============================================================================

#pragma once
#include "afxcmn.h"
#include "afxwin.h"

#include <FileRecoveryManager.h>
#include <IConfigurationSettingsPersistenceMgr.h>
#include <MRUList.h>
#include <ImageButtonWithStyle.h>

#include <string>

using namespace std;

// CImageCleanupSettingsEditorDlg dialog
class CImageCleanupSettingsEditorDlg : public CDialog
{
// Construction
public:
	CImageCleanupSettingsEditorDlg(CWnd* pParent = NULL, const string& rstrFileToOpen = "");	// standard constructor
	~CImageCleanupSettingsEditorDlg();

	//----------------------------------------------------------------------------------------------
	// PURPOSE: This function is used to set the file name to open if it was passed in on the
	//			command line.  This function will have no effect on the dialog if it is called
	//			after the dialog has been displayed
	void setOpenFileName(const string& rstrCommandLineFile);


// Dialog Data
	enum { IDD = IDD_IMAGECLEANUPSETTINGSEDITOR_DIALOG };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support
	virtual BOOL PreTranslateMessage(MSG* pMsg);

	//----------------------------------------------------------------------------------------------
	// stubbed in to keep dialog from closing on escape
	afx_msg void OnCancel();
	//----------------------------------------------------------------------------------------------
	// stubbed in to keep dialog from closing on enter
	afx_msg void OnOK();
	//----------------------------------------------------------------------------------------------
	afx_msg void OnBtnAdd();
	//----------------------------------------------------------------------------------------------
	afx_msg void OnBtnRemove();
	//----------------------------------------------------------------------------------------------
	afx_msg void OnBtnConfig();
	//----------------------------------------------------------------------------------------------
	afx_msg void OnBtnDown();
	//----------------------------------------------------------------------------------------------
	afx_msg void OnBtnUp();
	//----------------------------------------------------------------------------------------------
	afx_msg void OnClose();
	//----------------------------------------------------------------------------------------------
	afx_msg void OnFileExit();
	//----------------------------------------------------------------------------------------------
	afx_msg void OnFileNew();
	//----------------------------------------------------------------------------------------------
	afx_msg void OnFileOpen();
	//----------------------------------------------------------------------------------------------
	afx_msg void OnFileSave();
	//----------------------------------------------------------------------------------------------
	afx_msg void OnFileSaveas();
	//----------------------------------------------------------------------------------------------
	afx_msg void OnHelpAbout();
	//----------------------------------------------------------------------------------------------
	afx_msg void OnEditCut();
	//----------------------------------------------------------------------------------------------
	afx_msg void OnEditCopy();
	//----------------------------------------------------------------------------------------------
	afx_msg void OnEditPaste();
	//----------------------------------------------------------------------------------------------
	afx_msg void OnEditDelete();
	//----------------------------------------------------------------------------------------------
	afx_msg void OnToolsCheckForNewComponents();
	//----------------------------------------------------------------------------------------------
	afx_msg void OnBtnBrowseInFile();
	//----------------------------------------------------------------------------------------------
	afx_msg void OnBtnBrowseOutFile();
	//----------------------------------------------------------------------------------------------
	afx_msg void OnBtnTest();
	//----------------------------------------------------------------------------------------------
	afx_msg void OnBtnOpenInImage();
	//----------------------------------------------------------------------------------------------
	afx_msg void OnBtnOpenOutImage();
	//----------------------------------------------------------------------------------------------
	afx_msg void OnChangeTestInFileName();
	//----------------------------------------------------------------------------------------------
	afx_msg void OnChangeTestOutFileName();
	//----------------------------------------------------------------------------------------------
	afx_msg void OnClickOverideCheck();
	//----------------------------------------------------------------------------------------------
	afx_msg void OnRadioPagesClicked();
	//----------------------------------------------------------------------------------------------
	afx_msg void OnChangeRangeText();
	//----------------------------------------------------------------------------------------------
	afx_msg void OnDropFiles(HDROP hDropInfo);
	//----------------------------------------------------------------------------------------------
	afx_msg void OnSelectMRUMenu(UINT nID);
	//----------------------------------------------------------------------------------------------
	afx_msg void OnTimer(UINT nIDEvent);
	//----------------------------------------------------------------------------------------------
	virtual BOOL OnInitDialog();
	//----------------------------------------------------------------------------------------------
	afx_msg void OnPaint();
	//----------------------------------------------------------------------------------------------
	afx_msg HCURSOR OnQueryDragIcon();
	//----------------------------------------------------------------------------------------------
	afx_msg void OnDblclkListTasks(NMHDR* pNMHDR, LRESULT* pResult);
	//----------------------------------------------------------------------------------------------
	// handles change in checkbox state
	afx_msg void OnLvnItemchangedListTsk(NMHDR *pNMHDR, LRESULT *pResult);
	//----------------------------------------------------------------------------------------------
	// brings up context menu when right click on list box
	afx_msg void OnRightClickTasks(NMHDR *pNMHDR, LRESULT *pResult);
	//----------------------------------------------------------------------------------------------

// Implementation
protected:
	HICON m_hIcon;

	DECLARE_MESSAGE_MAP()

private:
	// variables for the window controls
	CListCtrl m_lstOperationList;
	CButton m_btnAdd;
	CButton m_btnRemove;
	CButton m_btnConfigure;
	CImageButtonWithStyle m_btnUp;
	CImageButtonWithStyle m_btnDown;
	CButton m_radioAllPages;
	CEdit m_editFirstPages;
	CEdit m_editLastPages;
	CEdit m_editSpecifiedPages;
	CButton m_btnTest;
	CButton m_btnOpenInFile;
	CButton m_btnOpenOutFile;
	CEdit m_editInFile;
	CEdit m_editOutFile;
	CButton m_chkOutFile;
	CButton m_chkOverwrite;
	CButton m_radioExtract;
	CButton m_btnBrowseOutFile;

	// input image
	string m_strInImageFile;

	// output image
	string m_strOutImageFile;

	// dirty flag to know if the file needs to be saved
	bool m_bDirty;
	
	// ImageCleanup settings
	IImageCleanupSettingsPtr m_ipSettings;

	// path to ImageViewer.exe
	string m_strImageViewerExePath;

	// current opened file
	string m_strCurrentFileName;

	// last file opened
	string m_strLastFileOpened;

	// bin folder for the application
	string m_strBinFolder;

	// object for handling plug in object selection and configuration
	IMiscUtilsPtr m_ipMiscUtils;

	// object that manages file recovery functionality
	FileRecoveryManager m_FRM;

	// the menu pointer to the main menu -> "File" menu -> Recent Files sub menu
	CMenu* m_pMRUFilesMenu;

	// recent used files list
	unique_ptr<MRUList> ma_pMRUList;

	// persistent manager
	unique_ptr<IConfigurationSettingsPersistenceMgr> ma_pUserCfgMgr;

	// clipboard manager
	IClipboardObjectManagerPtr m_ipClipboardMgr;

	//----------------------------------------------------------------------------------------------
	// PURPOSE: To setup the list control
	void prepareList();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To set the button activated and deactivated states
	void setButtonStates();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To set the menu activated and deactivated states
	void setMenuStates();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To set the radio button and edit box states in the scope of operations area
	void setScopeOfCleanupOperations();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To refresh the list control
	void refresh();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To remove selected tasks from the list
	void deleteSelectedTasks();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To clear all of the selections (when returns all items in the list are unselected)
	void clearListSelection();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To replace the cleanup operation at a specific index
	void replaceCleanupOperationAt(int iIndex, IObjectWithDescriptionPtr ipNewFP);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: returns a valid misc. utils object.
	// PROMISE: if m_ipMiscUtils is NULL, creates a new MiscUtils Object
	//          and points m_ipMiscUtils to it. returns m_ipMiscUtils.
	IMiscUtilsPtr getMiscUtils();
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To get the image cleanup operations vector for brief use
	IIUnknownVectorPtr getImageCleanupOperations();
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To add a new image cleanup operation to the image cleanup operations vector
	void addImageCleanupOperation(IObjectWithDescriptionPtr ipObject);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To get the rectangle for the appications window
	void getDlgItemWindowRect(UINT uiDlgItemResourceID, RECT &rectWindow);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To check if the setting have been modified and prompt the user to save if they have
	bool checkModification();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To update the caption of the dialog window
	void updateWindowCaption();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To open the given file name and load the settings contained in it
	void openFile(string strFileName);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To add the file to the MRU file list
	void addFileToMRUList(const string& rstrFileToBeAdded);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To remove a file from the MRU file list
	void removeFileFromMRUList(const string& rstrFileToBeRemoved);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To refresh the MRU file list
	void refreshFileMRU();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To update the UI from the settings vector
	void refreshUIFromImageCleanupSettings();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To enable and disable the editing features
	void enableEditFeatures(bool bEnable);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To update the component cache file with new image cleanup operations if any new ones
	//			are available
	void updateComponentCacheFile();
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To check an IUnknownVector and return true if all of the items in the
	//			vector are ObjectsWithDescription and that each of the underlying
	//			Objects are ImageCleanupOperations
	bool isVectorOfOWDOfCleanupOperations(IIUnknownVectorPtr ipVector);
	//----------------------------------------------------------------------------------------------
	void processDroppedFile(char* pszFile);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To display an image file (either the input or output image based on the bShowInImage
	//			flag
	void openImageFile(const string& strImageFileName);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To build the relative path to the location of SpotRecognitionWindow.exe
	//			First checks the current working directory (debug mode), if this fails
	//			moves up 2 directories to try to find the InputFunnelComponents directory
	//			which should contain SRW.exe when installed the software is installed
	string getPathToSpotRecognitionWindowExe();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To store the page range settings from the scope dialog box to the
	//			ImageCleanupSettingsObject.  If a range has been left blank will
	//			warn the user and return false.
	bool storePageRangeSettings();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To check if an enabled page range is blank.
	bool isPageRangeEmpty();
	//----------------------------------------------------------------------------------------------
};
