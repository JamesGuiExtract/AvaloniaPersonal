#pragma once

// CFlexDataEntryDlg.h : header file
//

#include "resource.h"
#include "DataEntryGrid.h"

#include <memory>

#include "..\..\..\..\InputFunnel\IFCore\Code\InputManagerEventHandler.h"

#include <string>
#include <vector>
#include <map>

/////////////////////////////////////////////////////////////////////////////
// CFlexDataEntryDlg dialog

class CFlexDataEntryDlg : public CDialog,
	public InputManagerEventHandler,
	public IDispatchImpl<ISRWEventHandler, &IID_ISRWEventHandler, &LIBID_UCLID_SPOTRECOGNITIONIRLib>,
	public IDispatchImpl<IParagraphTextHandler, &IID_IParagraphTextHandler, &LIBID_UCLID_SPOTRECOGNITIONIRLib>
{
// Construction
public:
	CFlexDataEntryDlg(std::string strImage, CWnd* pParent = NULL);
	~CFlexDataEntryDlg();

// Dialog Data
	//{{AFX_DATA(CFlexDataEntryDlg)
	enum { IDD = IDD_FLEXDATAENTRY_DIALOG };
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CFlexDataEntryDlg)
	public:
	virtual LRESULT OnLClickGrid(WPARAM wParam, LPARAM lParam);
	virtual BOOL PreTranslateMessage(MSG* pMsg);
	virtual BOOL DestroyWindow();
	virtual LRESULT OnCellLeft(WPARAM wParam, LPARAM lParam);
	virtual LRESULT OnCellRight(WPARAM wParam, LPARAM lParam);
	virtual LRESULT OnCellUp(WPARAM wParam, LPARAM lParam);
	virtual LRESULT OnCellDown(WPARAM wParam, LPARAM lParam);
	//}}AFX_VIRTUAL

public:
// IUnknown
	STDMETHODIMP_(ULONG ) AddRef();
	STDMETHODIMP_(ULONG ) Release();
	STDMETHODIMP QueryInterface( REFIID iid, void FAR* FAR* ppvObj);

// ISRWEventHandler
	STDMETHOD(raw_AboutToRecognizeParagraphText)();
	STDMETHOD(raw_AboutToRecognizeLineText)();
	STDMETHOD(raw_NotifyKeyPressed)(long nKeyCode, short shiftState);
	STDMETHOD(raw_NotifyCurrentPageChanged)();
	STDMETHOD(raw_NotifyEntitySelected)(long nZoneID);
	STDMETHOD(raw_NotifyFileOpened)(BSTR bstrFileFullPath);
	STDMETHOD(raw_NotifyOpenToolbarButtonPressed)(VARIANT_BOOL *pbContinueWithOpen);
	STDMETHOD(raw_NotifyZoneEntityMoved)(long nZoneID);
	STDMETHOD(raw_NotifyZoneEntitiesCreated)(IVariantVector *pZoneIDs);

// IParagraphTextHandler
	STDMETHOD(raw_NotifyParagraphTextRecognized)(ISpotRecognitionWindow *pSourceSRWindow, ISpatialString *pText);
	STDMETHOD(raw_GetPTHDescription)(BSTR *pstrDescription);
	STDMETHOD(raw_IsPTHEnabled)(VARIANT_BOOL *pbEnabled);

// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	//{{AFX_MSG(CFlexDataEntryDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	afx_msg void OnClose();
	afx_msg void OnBtnClear();
	afx_msg void OnBtnFind();
	afx_msg void OnBtnSave();
	afx_msg void OnBtnHelpAbout();
	afx_msg void OnOK();
	afx_msg void OnCancel();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnVScroll(UINT nSBCode, UINT nPos, CScrollBar* pScrollBar);
	//}}AFX_MSG
    afx_msg void OnAddButton(UINT nID);
    afx_msg void OnDeleteButton(UINT nID);
    afx_msg void OnPreviousButton(UINT nID);
    afx_msg void OnNextButton(UINT nID);
	BOOL OnToolTipNotify(UINT nID, NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnFileExit();
	DECLARE_MESSAGE_MAP()

protected:

//  InputManagerEventHandler
	virtual HRESULT __stdcall NotifyInputReceived(ITextInput* pTextInput);

private:
	//////////
	// Methods
	//////////

	// Sets selection to first cell of first non-empty row
	void	activateFirstCell();

	// Adds default items to m_mapGridCellsToValues
	// Items are found in strText as substrings between strLeft and strRight pairs.
	// strDefault is used as the right-hand entry in the map.
	void	addDefaultMapItems(std::string strText, std::string strLeft, 
		std::string strRight, std::string strDefault);

	// Uses map and vectors to populate strPXTFile
	void	buildPXTFile(std::string strPXTFile);

	// Returns True iff strText contains one or more expandable functions
	//    i.e. "$DirOf(", "$FileNoExtOf(", etc
	bool	containsTFEFunction(std::string strText);

	// Creates and positions the grids defined by INI file
	// Returns: Y-position of bottom of last grid + 
	//          height of single row as offset to be used as scroll bar height
	long	createGrids();

	// Creates the toolbar
	void	createToolBar();

	// Close the application automatically
	// if the INI file has been set to do so
	void doAutoClose();

	// Saves information from grids to output file
	// Assumes that validateSave() returned true
	// "bIsAutoCloseAllowed" is not specified in INI file. 
	// It is specified according to different situations
	// whether close after save is allowed or not, for instance,
	// auto close is not allowed when try to save a fille before
	// open another image file.
	void	doSave(bool bIsAutoCloseAllowed = true);

	// Enables or disables the toolbar or menu item for the specified control
	// Expected to be { IDC_BTN_FIND, IDC_BTN_SAVE }
	void	enableButton(int nID, bool bEnable);

	// Returns pointer to active Grid or NULL if none active
	CDataEntryGrid*	getActiveGrid();

	// Gets folder path within INI file
	std::string	getFolder();

	// Reads strINIFile to determine how many Grid items are defined.
	// Stores section names into m_vecSectionNames and section heights
	// into m_vecSectionHeights
	long	getGridSections(std::string strINIFile);

	// Gets full path to INI file
	std::string	getINIPath();

	// Creates the MiscUtils object if necessary and returns it
	IMiscUtilsPtr getMiscUtils();

	// Gets full path to output file
	std::string	getOutputFile();

	// Gets height, in pixels, of one row in a grid.
	long getRowHeightInPixels();

	// Gets full path to RSD File as specified in INI file.
	// Returns empty string if not defined.
	std::string	getRSDFile();

	// Gets specified setting from INI file.
	// Returns empty string if not found.
	// Throws exception if not found AND bRequired = true
	std::string	getSetting(std::string strKey, bool bRequired);

	// Handles keystrokes received by Spot Recognition Window
	//    Valid keys:
	//      Tab        --->  Cell Right
	//      Shift+Tab  --->  Cell Left
	//      Up Arrow   --->  Cell Up
	//      Down Arrow --->  Cell Down
	void handleCharacter(long nKeyCode);

	// Processes characters received from keyboard
	// Returns True if this a valid shortcut key even if the command is ignored
	//    Valid shortcut keys:
	//      Ctrl+F  --->  Find
	//      Ctrl+S  --->  Save
	//      Ctrl+W  --->  Clear
	bool	handleShortcutKey(WPARAM wParam);

	// Handles Swiped text
	void	handleSwipeInput(std::string strInput);

	//=============================================================================
	// PURPOSE: Highlights the specified attribute
	// REQUIRE: None
	// PROMISE: If the specified attribute is spatial, then it will be 
	//			highlighted.  If the attribute is not a spatial attribute, 
	//			then all currently highlightly spatial attributes (if any) will 
	//			be "unhighlighted".
	// ARGS:	ipAttr - the IAttribute to be highlighted
	void	highlightAttribute(IAttributePtr ipAttr);

	// Returns true if Find button on toolbar should be hidden.
	bool	isFindHidden();

	// Returns true if Save button on toolbar should be hidden.
	bool	isSaveHidden();

	// Loads menu resource and removes unwanted entries
	void	loadMenu();

	// Parse the Output Template specification
	void	parseOutputTemplate(std::string strPath);

	// Provides ipAttributes to each defined grid
	void	populateGrids(IIUnknownVectorPtr ipAttributes);

	//=============================================================================
	// PURPOSE: Creates and positions the specified grid object. Applies settings
	//			defined in m_vecSectionNames[iIndex].  Initial vertical position 
	//			in the dialog is lStartOffset.
	// REQUIRE: None
	// PROMISE: Returns the new strting offset for the next grid.
	// ARGS:	pGrid - the grid to be created
	//			lGridControlID - resource ID of this grid
	//			iIndex - Index of settings within m_vecSectionNames
	//			lRowHeight - Height of one row in pixels
	//			lStartOffset - Initial vertical position of the grid object
	long	prepareGrid(CDataEntryGrid* pGrid, long lGridControlID, int iIndex, 
		long lRowHeight, long lStartOffset, CWnd* pParent);

	// Adds format line to raw data map
	void	processFormatLine(std::string strLine);

	// Reads strINIFile to determine format of output information.
	// Sets or clears m_bOutputFormatDefined
	void	readOutputFormatTemplate(std::string strINIFile);

	// Removes any selections in other grids
	void	setActiveGrid(CDataEntryGrid* pGrid);

	// Enables and disables the Remove buttons
	void	updateButtons();

	// Enables, disables and updates the Previous, Actual and Next controls
	void	updateNavigationControls(CDataEntryGrid* pGrid);

	// Shows or hides various Image Window toolbar buttons
	// Those below are configurable in the INI file
	//    ImageToolbar_RotateCCW
	//    ImageToolbar_RotateCW
	//    ImageToolbar_PageFirst
	//    ImageToolbar_PageLast
	void	updateSRIRToolbar();

	// Returns true iff data in grids can be Saved acceptably as is.
	// This allows for a user to approve unusual conditions if warned 
	// based on INI settings.
	// If false is returned, doSave() should NOT be called.
	bool	validateSave();

	///////////////
	// Data Members
	///////////////

	// Toolbar
	std::auto_ptr<CToolBar>	m_apToolBar;

	// Manages tooltips
	CToolTipCtrl	m_ToolTipCtrl;

	// Grid windows
	std::vector<std::string>		m_vecSectionNames;
	std::vector<std::string>		m_vecSectionHeights;
	std::vector<CDataEntryGrid*>	m_vecGrids;
	CDataEntryGrid*					m_pActiveGrid;

	// Command-line string - treated as Image file
	std::string						m_strCommandLine;

	// Attribute Finder Engine
	IAttributeFinderEnginePtr		m_ipEngine;

	// Handles auto-encryption
	IMiscUtilsPtr					m_ipMiscUtils;

	// Spot Recognition Window associated with the dialog
	ISpotRecognitionWindowPtr		m_ipSRIR;

	// paragraph text handler to be associated with the SRIR
	IIUnknownVectorPtr m_ipParagraphTextHandlers;

	// Reference count for this COM object
	long			m_lRefCount;

	// Have the dialog's controls been instantiated yet - allows for resize
	// and repositioning
	bool			m_bInitialized;

	// Dialog rectangle and Scroll-bar position details
	CRect			m_rect;
	int				m_nScrollPos;
	long			m_lMinScrollPos;
	long			m_lScrollStep;
	long			m_lTotalGridHeight;

	// Has an output format been defined in the INI file
	bool			m_bOutputFormatDefined;

	// Open VOA file, if present, when image file opened
	bool			m_bOpenVOAFile;

	// Map of output tags and string definitions
//	std::map<std::string, std::string> m_mapOutputTagsToDefinitions;
	// Vectors of output tags and string definitions
	std::vector<std::string>	m_vecPXTTags;
	std::vector<std::string>	m_vecPXTDefs;

	// Map of grid cells to grid values
	std::map<std::string, std::string> m_mapGridCellsToValues;
};
