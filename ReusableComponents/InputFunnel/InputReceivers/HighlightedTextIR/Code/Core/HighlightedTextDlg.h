//============================================================================
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	HighlightedTextDlg.h
//
// PURPOSE:	Declaration of HighlightedTextDlg class
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//============================================================================
#pragma once

//{{AFX_INCLUDES()
#include "uclidmcrtextviewer.h"
//}}AFX_INCLUDES

#include "ConfigManager.h"

#include <afxext.h>
#include <map>
#include <string>
#include <vector>
#include <memory>

// forward declaration
class MRUList;

/////////////////////////////////////////////////////////////////////////////
// HighlightedTextDlg dialog

class HighlightedTextDlg : public CDialog
{
public:

	//=============================================================================
	// PURPOSE: Constructor
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	pInputEntityManager - manager for entity items
	//				pParent - parent window
	HighlightedTextDlg(IInputEntityManager* pInputEntityManager, 
		CWnd* pParent = NULL);

	//=============================================================================
	// PURPOSE: Destructor
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	None.
	virtual ~HighlightedTextDlg();

	//=============================================================================
	// PURPOSE: Adds a new input finder into the map
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	
	void AddInputFinder(std::string strInputFinderName, IInputFinder* ipInputFinder);

	//=============================================================================
	// PURPOSE: Clears the text without saving any changes to the currently open 
	//				file.
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	None.
	void	Clear();

	//=============================================================================
	// PURPOSE: Disables text selection.
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	None.
	void	DisableText();

	//=============================================================================
	// PURPOSE: Enables text selection.
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	None.
	void	EnableText();

	//=============================================================================
	// PURPOSE: Enables text selection.
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	strPrompt - prompt string to be displayed in the status bar
	void	EnableText(const std::string& strPrompt);

	//=============================================================================
	// PURPOSE: Gets the text of the specified entity.
	// REQUIRE: Specified ID must be valid
	// PROMISE: Returns text string
	// ARGS:	lEntityID - zero-based ID of object whose text is to be retrieved
	std::string	GetEntityText(long lEntityID);

	//=============================================================================
	// PURPOSE: Gets the name of the Input Finder.
	// REQUIRE: Nothing
	// PROMISE: Returns Finder name or blank string if no Input Finder
	// ARGS:	None.
	std::string	GetFinderName();

	//=============================================================================
	// get indirect source file name if any
	const std::string& getIndirectSourceName() const {return m_strIndirectSourceName;}

	//=============================================================================
	// PURPOSE: Gets the name of the persistent source.
	// REQUIRE: Nothing
	// PROMISE: Returns file name or blank string if no persistent source
	// ARGS:	None.
	std::string	GetSourceName();

	//=============================================================================
	// PURPOSE: Gets the current text string from the dialog
	// REQUIRE: Nothing
	// PROMISE: Returns entire text or blank string if empty
	// ARGS:	None.
	std::string	GetText();

	//=============================================================================
	// PURPOSE: Provides indication of validity for specified entity.
	// REQUIRE: Nothing
	// PROMISE: Returns true if entity exists, false otherwise.
	// ARGS:	lEntityID - zero-based ID of object whose text is to be retrieved
	bool	IsEntityValid(long lEntityID);

	//=============================================================================
	// PURPOSE: Provides indication of text selection enable state.
	// REQUIRE: Nothing
	// PROMISE: Returns true if text selection is enabled, false otherwise.
	// ARGS:	None.
	bool	IsInputEnabled();

	//=============================================================================
	// PURPOSE: Provides indication of any changes to text in the currently open 
	//				file.
	// REQUIRE: Nothing
	// PROMISE: Returns true if unsaved changes have been made, false otherwise.
	// ARGS:	None.
	bool	IsModified();

	//=============================================================================
	// PURPOSE: Provides indication of text source.
	// REQUIRE: Nothing
	// PROMISE: Returns true if text source is a file, false otherwise.  Note that 
	//				additions and deletions to text do not change this indication.
	// ARGS:	None.
	bool	IsPersistentSource();

	//=============================================================================
	// PURPOSE: Opens the specified file without saving any changes to the 
	//				currently open file.
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	strFilename - filename
	void	Open(const std::string& strFilename);

	//=============================================================================
	// PURPOSE: Saves the current text to the current file.
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	None
	void	Save();

	//=============================================================================
	// PURPOSE: Saves the current text to the specified file.
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	strFilename - filename
	void	SaveAs(const std::string& strFilename);

	//=============================================================================
	// PURPOSE: Sets the text as given for the specified entity without saving any 
	//				changes to the currently open file.
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	lEntityID - the entity to replace
	//				strNewText - new text for the specified entity
	void	SetEntityText(long lEntityID, const std::string& strNewText);

	//=============================================================================
	// PURPOSE: Sets the event handler for text selection events.
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	ipEventHandler - pointer to handler
	void	SetEventHandler(IIREventHandler* ipEventHandler);

	//=============================================================================
	// set indirect source file name
	void setIndirectSourceName(const std::string& strIndirectSourceName);

	//=============================================================================
	// PURPOSE: Uses the specified Input Finder for text parsing
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	strNewName - name of new Input Finder
	void	SetInputFinder(const std::string& strNewName);

	//=============================================================================
	// PURPOSE: Sets the displayed text as specified without saving any changes to 
	//				the currently open file.
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	strNewText - text to be displayed in the window
	void	SetText(std::string strNewText);

	//=============================================================================
	// PURPOSE: Provides state of specified entity - whether or not it has been 
	//				used.
	// REQUIRE: Nothing
	// PROMISE: Returns true if the entity ID is valid AND the entity has been 
	//				used, false otherwise.
	// ARGS:	lEntityID - the entity whose used status is being queried
	bool IsMarkedAsUsed(long lEntityID);

	//=============================================================================
	// PURPOSE: Sets the state of the specified entity - used or not.
	// REQUIRE: Valid entity ID
	// PROMISE: None.
	// ARGS:	lEntityID - the entity whose used status is to be set
	//				bMarkAsUsed - the used status
	void MarkAsUsed(long lEntityID, bool bMarkAsUsed);

	// set/clear text processors
	void setTextProcessors(IIUnknownVector *pvecTextProcessors);
	void clearTextProcessors();

	// emmulates the open button clicking event
	void showOpenDialog();

	BOOL ShowWindow(int nCmdShow);

// Dialog Data
	//{{AFX_DATA(HighlightedTextDlg)
	enum { IDD = IDD_TV_DIALOG };
	CUCLIDMCRTextViewer	m_TV;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(HighlightedTextDlg)
	public:
	virtual BOOL Create(UINT nIDTemplate, CWnd* pParentWnd = NULL );
	virtual void OnFinalRelease();
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(HighlightedTextDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg void OnAppend();
	afx_msg void OnEditDecrease();
	afx_msg void OnEditIncrease();
	afx_msg void OnEditPaste();
	afx_msg void OnEditCopy();
	afx_msg void OnFileNew();
	afx_msg void OnFileOpen();
	afx_msg void OnFilePrint();
	afx_msg void OnFileSave();
	afx_msg void OnClose();
	afx_msg void OnTextSelected(long lEntityID);
	afx_msg void OnSelectedText(LPCTSTR strSelectedText);
	afx_msg void OnToolbarDropDown(NMHDR* pNMHDR, LRESULT *plr);
	afx_msg void OnBTNInputFinder();
	afx_msg void OnProcessText();
	afx_msg void OnTPMenuItemSelected(UINT nID);
	DECLARE_EVENTSINK_MAP()
	//}}AFX_MSG
	afx_msg BOOL OnToolTipText(UINT nID, NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnSelectMRUPopupMenu(UINT nID);
	afx_msg void OnSelectInputFinderPopupMenu(UINT nID);
	DECLARE_MESSAGE_MAP()

	//=============================================================================
	// PURPOSE: To create the toolbar
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	None.
	void	createToolBar();

	//=============================================================================
	// PURPOSE: Get the list file name which contains a list of COM objects' prog 
	//				IDs and descriptions.
	// REQUIRE: The two files must be in the same directory
	// PROMISE: Returns the full path to the list file
	// ARGS:	strModuleName - name, without path, of this DLL
	//				strListFileName - name, without path, of list file
	std::string getListFileName(const std::string& strModuleName, 
		const std::string& strListFileName);

	//=============================================================================
	// PURPOSE: To enable or disable the process text button's state in the toolbar
	//			depending upon whether text processors have been setup for this
	//			instance.
	void updateStateOfProcessTextButton();

	//=============================================================================
	// PURPOSE: Parse text based on current Input Finder
	// REQUIRE: Nothing
	// PROMISE: Will reset all used/unused text indications.
	// ARGS:	None.
	void	parseText();

	//=============================================================================
	// PURPOSE: Resize and/or move dialog controls
	// REQUIRE: Nothing
	// PROMISE: OCX will expand to fill available window area.
	// ARGS:	None.
	void	resizeItems();

	//=============================================================================
	// PURPOSE: To set the text in the status bar
	// REQUIRE: Nothing
	// PROMISE: Current status text will be replaced by the provided text.
	// ARGS:	strText - new text for the status bar
	void	setStatusText(const std::string& strText);

	//=============================================================================
	// PURPOSE: Update the title bar text to have "filename - Legal Text" or "Legal Text"
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	None.
	void	updateTitle();

	//=============================================================================
	// PURPOSE: Set the title bar text
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	strTitleText - text to be displayed in the title bar
	void	setTitleText(const std::string& strTitleText);

private:

	enum ETextProcessingScope {kProcessEntireText=0, kProcessSelectedText};

	// how many text processing options are there.
	int m_nTPOptionSpanLen;

	// last position where the text processors menu was shown
	POINT m_lastTPMenuPos;

	// text processors associated with this window
	IIUnknownVectorPtr m_ipTextProcessors;

	std::auto_ptr<IConfigurationSettingsPersistenceMgr> ma_pUserCfgMgr;
	// handles configuration persistence
	std::auto_ptr<ConfigManager> ma_pCfgHTIRMgr;

	// Input entity manager - the COM object which owns this dialog
	IInputEntityManager*	m_pInputEntityManager;

	// Event handler, to which all text selection events are to be sent
	IIREventHandler* m_ipEventHandler;

	// the parent window of this dialog
	CWnd* m_pParent;		

	// Icon displayed in the title bar
	HICON m_hIcon;

	// Status bar for the dialog
	CStatusBar m_statusBar;

	CToolBar m_toolBar;

	// Tooltips for the toolbar buttons
	std::auto_ptr<CToolTipCtrl> ma_pToolTipCtrl;

	// Height of the toolbar
	int	m_iBarHeight;

	// Height of the status bar
	int	m_iStatusHeight;

	// Whether the dialog controls have been initialized.  This allows
	// for the controls to be moved and resized before viewing
	bool m_bInitialized;

	// Whether text selection in the control is enabled or not.
	bool m_bTextEnabled;

	// Whether text is from a (persistent) file or not
	bool m_bPersistentSource;

	// Name of current Input Finder
	std::string m_strFinderName;

	// indirect source file name if any
	std::string m_strIndirectSourceName;

	// Collection of dialog instances
	static std::vector<HighlightedTextDlg *> ms_vecInstances;

	// Display color indicating that the entity has been used
	static const COLORREF ms_USED_ENTITY_COLOR;

	// Display color indicating that the entity has not been used
	static const COLORREF ms_UNUSED_ENTITY_COLOR;

	// Stores collection of most recently opened files
	std::auto_ptr<MRUList>	ma_pRecentFiles;

	std::map<std::string, IInputFinderPtr> m_mapIFNameToObj;
	static IStrToStrMapPtr ms_mapInputFinderNameToProgId;
	//////////////////////////////////////////////
	// Helper functions

	// show the text processors menu
	void showTPMenu(POINT menuPos);

	// add file to the MRU list
	void addFileToMRUList(const std::string& strFileToBeAdded);

	// remove file from the MRU list
	void removeFileFromMRUList(const std::string& strFileToBeRemoved);
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
