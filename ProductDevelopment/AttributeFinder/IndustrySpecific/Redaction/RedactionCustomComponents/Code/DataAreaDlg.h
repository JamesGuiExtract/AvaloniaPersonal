
#pragma once

// DataAreaDlg.h : header file
//

#include "DataConfidenceLevel.h"
#include "DataDisplaySettings.h"
#include "RedactionVerificationOptionsDlg.h"
#include "RedactionUISettings.h"
#include "StopWatch.h"
#include "IDShieldData.h"
#include "ExemptionCodeList.h"
#include "ExemptionCodesDlg.h"

#include <memory>
#include <TreeGridWnd.h>
#include <Notifications.h>

#include <string>
#include <map>
#include <vector>
#include <set>

/////////////////////////////////////
// Messages for communication between
// Dialog, Thread, File Processor
/////////////////////////////////////

// File Processor posts this message to Thread to 
// indicate that a new file is ready for Verification
#define WM_NEW_FILE_READY		WM_USER + 3141

// Dialog posts this message to Thread to indicate that 
// processing of current file is complete
#define WM_FILE_COMPLETE		WM_USER + 3142

// Dialog posts this message to Thread to indicate 
// that the user is cancelling verification
#define WM_CLOSE_VERIFICATION_DLG		WM_USER + 3143

// Dialog posts this message to Thread to indicate that
// a file failed. The WPARAM contains a pointer to a char array
// that contains a stringized byte stream of the exception that
// occurred. LPARAM contains the length of the string.  This
// array must be deleted by the receiver of this message.
#define WM_FILE_FAILED			WM_USER + 3144

EXTERN_C const CLSID CLSID_DataAreaDlg;

/////////////////////////////////////////////////////////////////////////////
// CDataAreaDlg dialog

class CDataAreaDlg : public CDialog,
	public CComCoClass< CDataAreaDlg, &CLSID_DataAreaDlg >,
	public IDispatchImpl<ISRWEventHandler, &IID_ISRWEventHandler, &LIBID_UCLID_SPOTRECOGNITIONIRLib>
{
// Construction
public:

	CDataAreaDlg(RedactionUISettings UISettings, CWnd* pParent = NULL);
	~CDataAreaDlg();

	// Gets the full path to exemption code directory
	static string getExemptionCodeDirectory();

	// Gets full path to INI file - no validation
	string	getINIPath();

	// Is the INI file present?
	bool	IsValidINI();

	// Opens the VOA file in the dialog, opens the associated image file in the 
	// Spot Recognition Window, resets dialog and prepares for processing
	void	SetInputFile(const string& strInputFile);

	// Set the FAM tag manager pointer
	void	InitFAMTagManager(IFAMTagManagerPtr ipFAMTagManager);

	// Set the FAM DB manager
	void SetFAMDB(IFileProcessingDBPtr ipFAMDB);

	//---------------------------------------------------------------------------------------------
	// PURPOSE: To check whether the dialog has been successfully initialized or not
	inline bool	isDialogInitialized() { return m_bInitialized; }

// Dialog Data
	//{{AFX_DATA(CDataAreaDlg)
	enum { IDD = IDD_DATA_AREA_DLG };
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CDataAreaDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL PreTranslateMessage(MSG* pMsg);
	virtual BOOL DestroyWindow();
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

// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	//{{AFX_MSG(CDataAreaDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnPaint();
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg void OnGetMinMaxInfo(MINMAXINFO* pMMI);
	afx_msg void OnButtonSave();
	afx_msg void OnButtonToggleRedact();
	afx_msg void OnButtonPreviousItem();
	afx_msg void OnButtonNextItem();
	afx_msg void OnButtonZoom();
	afx_msg void OnButtonStop();
	afx_msg LRESULT OnLButtonLClkRowCol(WPARAM wParam, LPARAM lParam);
	afx_msg LRESULT OnDoubleClickRowCol(WPARAM wParam, LPARAM lParam);
	afx_msg LRESULT OnModifyCell(WPARAM wParam, LPARAM lParam);
	afx_msg void OnButtonOptions();
	afx_msg void OnButtonHelpAbout();
	afx_msg void OnClose();
	afx_msg void OnTimer(UINT nIDEvent);
	afx_msg void OnButtonPreviousDoc();
	afx_msg void OnButtonNextDoc();
	afx_msg BOOL OnNcActivate(BOOL bActive); // added as per P16 #2720
	afx_msg void OnButtonApplyExemptions();
	afx_msg void OnButtonApplyAllExemptions();
	afx_msg void OnButtonLastExemptions();
	//}}AFX_MSG
	BOOL OnToolTipNotify(UINT nID, NMHDR* pNMHDR, LRESULT* pResult);
	DECLARE_MESSAGE_MAP()

private:

	//////////
	// Classes
	//////////

	struct DocumentData
	{
		DocumentData(const string& strOriginal="", const string& strInput="", 
			const string& strOutput="", const string& strVoa="", const string& strDocType="", 
			long lFileId=-1);

		// The full path to the image that was originally supplied
		string m_strOriginal;

		// The full path to the image to use as input for verification
		string m_strInput;

		// The full path to the output path for the verified image
		string m_strOutput;

		// The full path to the voa file used as input
		string m_strVoa;

		// The document type
		string m_strDocType;

		// The FAM file ID that corresponds to this document
		long m_lFileId;
	};

	class DataItem
	{
	public:
		// Constructor
		DataItem();

		~DataItem();

		//-----------------------------------------------------------------------------------------
		// PURPOSE: Gets the first raster zone of the data item
		IRasterZonePtr getFirstRasterZone();
		//-----------------------------------------------------------------------------------------
		// PURPOSE: Gets/sets the exemption codes associated with the data item
		ExemptionCodeList getExemptionCodes();
		void setExemptionCodes(const ExemptionCodeList& codes);

		// The attribute this DataItem represents
		IAttributePtr m_ipAttribute;

		// Information to display this element
		DataDisplaySettings* m_pDisplaySettings;

	private:

		// Exemption codes associated with this data item
		ExemptionCodeList m_codes;
	};

	class CompareDataItems
	{
	public:
		CompareDataItems(vector<DataItem>* pvecDataItems);

		bool operator()(long& nIndex0, long& nIndex1);

		vector<DataItem>* m_pvecDataItems;
	};

	//////////
	// Methods
	//////////

	// Adds an attribute that corresponds to the manual redactions with the specified IDs.
	void addManualRedactions(vector<long> &vecZoneIDs);

	// Applies the last applied exemption codes to the currently selected item.
	void applyLastExemptionCodes();

	// Creates the output files
	void	createOutputFiles();

	// Creates the toolbar
	void	createToolBar();

	//---------------------------------------------------------------------------------------------
	// PURPOSE: Redacts the specified image. Returns true if at least one redaction was made, 
	//          returns false otherwise
	bool doRedaction(const DocumentData& document);

	// Moves grids around based on dialog height
	void	doResize(int cx, int cy);

	//---------------------------------------------------------------------------------------------
	// PURPOSE: Gets the document currently being viewed, which could be a document from the 
	//          document history queue or the most recently processed document (not yet in history).
	DocumentData getCurrentDocument();

	// Gets DocType Attribute from ipVOA
	string	getDocumentType(IIUnknownVectorPtr ipVOA);

	//---------------------------------------------------------------------------------------------
	// PURPOSE: Gets the exemption codes dialog that allows the user to select exemption codes.
	CExemptionCodesDlg* getExemptionCodesDlg();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To return the full path of the destination of the image file being collected.
	//          Creates the feedback directory, if it is not already created.
	// PARAMS:  strSourceDoc - the full path to the original image file
	// EXAMPLE: strSourceDoc - "C:\Images\abc.tif"
	//          feedback data folder - "$DirOf(<SourceDocName>)\Feedback"
	//          m_lFileID - 123
	//          (1) If using original filenames:    "C:\Images\Feedback\abc.tif"
	//          (2) If generating unique filenames: "C:\Images\Feedback\123.tif"
	string getFeedbackImageFileName(const DocumentData& document);

	// Returns row number of next data item after iRow that has not been viewed.
	// Returns 0 if all data items have been viewed.
	int		getNextUnviewedItem(int iRow);

	// Retrieves page number of nRow item in Data grid
	int		getGridItemPageNumber(int nRow);

	// Returns prioritized result from GetAsyncKeyState
	//   VK_CONTROL if Control key is pressed
	//   VK_SHIFT   if Shift key is pressed
	//   VK_MENU    if Alt key is pressed
	//   0          if none of the above are pressed
	int		getKeyState();

	//---------------------------------------------------------------------------------------------
	// PURPOSE: Gets the document data for the specified image, voa, and file id.
	// PARAMS:  strInputImageFile - The name of the original image file supplied for verification.
	//          strVoa - The full path to the voa file used for input.
	//          ipVOA - A vector of attributes corresponding to strInputImageFile.
	//          lFileId - The FAM file ID corresponding to the document.
	DocumentData getDocumentData(const string& strOriginalImage, const string& strVoa, 
		IIUnknownVectorPtr ipVOA, long lFileId);

	// Gets name of metadata file
	string	getMetadataFileName(string strInputFile);

	//---------------------------------------------------------------------------------------------
	// PROMISE: Determines page number of next page to be reviewed.  Moves 
	// through pages from front to back.  After the last page, checks for 
	// any skipped earlier pages if they contain viewable data or if 
	// settings indicate that all pages should be viewed.
	// REQUIRES: The last data item is currently selected.
	int			getNextPageToView();

	// Gets name of redacted image
	string	getOutputFileName(string strInputFile);

	// Returns number of items in m_vecDataItems where redaction is ON
	long		getRedactionsCount();

	// Retrieves additional settings from registry
	void		getRegistrySettings();

	// Gets / sets specified setting from specified section of INI file.
	// Returns empty string if not found.
	// Get throws exception if not found AND bRequired = true
	string	getSetting(const string& strSection, const string& strKey, 
					bool bRequired);
	void		setSetting(const string& strSection, const string& strKey, 
					const string& strValue);

	//---------------------------------------------------------------------------------------------
	// PURPOSE: Gets name of image file from Spatial Strings within ipVOA. Returns empty string
	//          if no source document is found.
	string	getSourceDocName(IIUnknownVectorPtr ipVOA);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Gets the full path of the voa file that corresponds to the specified document.
	string getVoaFileName(const string& strSourceDocName);

	// Creates IRasterZone from parameters provided by Generic Display OCX
	IRasterZonePtr	getZoneFromID(long lID);

	// Processes characters received from keyboard or via SRIR
	void	handleCharacter(WPARAM wParam);

	//---------------------------------------------------------------------------------------------
	// PURPOSE: Determines whether feedback collection should occur and performs all necessary
	//          feedback collection procedures.
	// PARAMS:  document - the document from which to collect feedback
	//          bRedacted - whether the image contained at least one redaction
	void handleFeedback(const DocumentData& document, bool bRedacted);

	// Resets Summary grid to include lPageCount pages
	// with each page bold and white background
	void	initPages(int iPageCount);

	// Returns true if the verifier is currently on a document in the history queue;
	// false if the verifier is on the most recent document (not yet in the history queue).
	inline bool isInHistoryQueue() { return m_nPositionInQueue < m_nNumPreviousDocsQueued; }

	// Returns true if iRow is a Manual redaction item, otherwise false
	bool	isManualRow(int iRow);

	// Returns true if iPage has already been reviewed according to 
	// background color of appropriate cell in Pages grid
	bool	isPageViewed(int iPage);

	// Loads strFile and prepares Summary Grid
	// Updates document type field 
	//   called by SetInputFile() and during document-level navigation
	void loadImage(const DocumentData& document);

	//---------------------------------------------------------------------------------------------
	// PURPOSE: Opens the specified voa file in the VOA File Viewer.
	void openVoa(const string& strVoaFile);

	// Loads strFile and saved Data Items from history vectors
	//   called by OnButtonSave() if navigating to or from a history item
	//   called by OnButtonPreviousDoc()
	//   called by OnButtonNextDoc()
	void	navigateToHistoryItem(int nIndex);

	// Creates Spot Recognition Window, displays empty window, enables text input
	// [p16 #2627] - added iXOffSet and iYOffSet to compensate for different Start Menu positions
	void	prepareAndShowSRIR(int iWidth, int iHeight, int iXOffSet, int iYOffSet);

	// Adds appropriate Attributes to m_ipVerifyAttributes
	// Populates m_vecDataItems
	void	populateDataGrid(IIUnknownVectorPtr ipAllAttributes);

	// Creates and positions the grids
	void	prepareGrids();

	// Reads and processes settings in INI file
	void	processINIFile();

	//---------------------------------------------------------------------------------------------
	// PURPOSE: If applying the specified exemption codes to all data items would result in 
	//          changing (overwriting) existing codes, prompts the user with a warning message.
	// PROMISE: Returns true if no existing codes are being overwritten with new data or if the 
	//          user chose to continue when given the warning prompt; return false if the user 
	//          chose to cancel after being shown the warning prompt.
	bool promptBeforeApplyAllExemptions(const ExemptionCodeList& codesToApply);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Prompts the user if required fields aren't specified before changing documents.
	// PARAMS:  strAction - the name of the action that is being performed to change documents.
	//                      (e.g. "save file", "move to next document")
	// PROMISE: Displays a warning message and returns true if the user has not specified the 
	//          required redaction type or the required exemption codes; returns false without 
	//          displaying a message if the user has specified the required data or if the data 
	//          is not required.
	bool promptBeforeNewDocument(const string& strAction);

	// Updates Data Grid display from m_ipVerifyAttributes
	void	refreshDataGrid();

	// Updates specified row of Data Grid based on Settings
	// Text is displayed in Bold font or Normal per bBold.
	// Text is always displayed Normal if bManual = true
	void	refreshDataRow(int nRow, DataDisplaySettings* pDDS, bool bBold, bool bManual=false);

	// Clears grids and SRIR
	void	reset();

	// Writes settings from Options dialog to INI file
	void	saveOptions();

	//---------------------------------------------------------------------------------------------
	// PURPOSE: Determines whether feedback should be collected based on whether the current 
	//          document being processed meets the requirements of the feedback collection options.
	// PARAMS:  bDocContainsRedactions - whether the document contains redactions after user
	//             corrections (if any) have been made
	bool shouldCollectFeedback(bool bDocContainsRedactions);

	// Selects 1-relative nRow item in Data grid.  Also sets text to 
	// Normal and updates active index
	void	selectDataRow(int nRow, bool bAllowAutozoom = true);

	//---------------------------------------------------------------------------------------------
	// PURPOSE: Displays a prompt allowing the user to select exemption codes.
	// PARAMS:  bApplyToAll - Applies the selected exemption codes to all data rows if true;
	//          applies codes only to selected data row if false.
	void selectExemptionCodes(bool bApplyToAll=false);

	// Sets status icon in Redact column
	//   nRow > 0
	//   eChoice == kRedactYes : Show Redact icon
	//   eChoice == kRedactNo  : Show No Redact icon
	//   eChoice == kRedactNA  : Show N/A icon
	void	setDataStatus(int nRow, ERedactChoice eChoice);

	// Shows iPage in SRIR, Sets appropriate background color in Pages grid, 
	// Highlights first Attribute on iPage within Data grid if bSelectFirst = true.
	// If bSelectFirst = false, selection state is not modified
	void	setPage(int iPage, bool bSelectFirst);

	// Sets background color in appropriate cell
	void	setPageBackground(int iPage);

	// Each item in Data grid on iPage will be set to Normal font
	// m_ipVerifyAttributes is assumed sorted
	void	setPageNormal(int iPage);

	//---------------------------------------------------------------------------------------------
	// PURPOSE: Sets ripAttribute's SourceDocName to strSourceDocName and recursively sets
	//          ripAttribute's subattributes to SourceDocName
	void setSourceDocName(IAttributePtr& ripAttribute, const string& strSourceDocName);

	// Enables and disables toolbar buttons
	void	updateButtons();

	// Updates m_pActiveDDS based on iDataIndex by 
	// finding Attribute for iDataIndex within m_vecAttributes.
	void	updateSettings(int iDataIndex);

	// Adds filename to dialog title bar 
	void	updateTitleBar(string strFile);

	// given the index into m_vecDataItems
	// this will find the index of the item in 
	// the grid\sorted-index-array
	// if the item is not in the list -1 is returned
	long	getGridIndex(long nItem);

	// Stores current data to appropriate history collections for SourceDocument, 
	// Pages Reviewed and if bSaveDataItemsToHistory is true the Data Items
	// Uses m_nPositionInQueue to specify  position in history. 
	// ADDED bSaveDataItemsToHistory for P16 2902
	void	updateCurrentHistoryItems(bool bSaveDataItemsToHistory = true);

	void	updateDataItemHighlights(DataDisplaySettings* pDDS, bool bIsSelected = false);

	// Writes Metadata output to appropriate output file
	void writeMetadata(const DocumentData& document);

	// updates the list in the type column of the display grid
	void	setGridTypeColumnList();

	// will clear the set of attribute types and reset to the default state containing
	// only the types listed in the INI file, empty string, and Manual [p16 #2385]
	void resetAttributeTypes();

	/////////////////////
	// XML helper methods
	/////////////////////
	// Creates and returns node with redaction information - built from di
	MSXML::IXMLDOMNodePtr getRedactionNode(MSXML::IXMLDOMDocumentPtr ipXMLDOMDocument, 
		DataItem &di);

	// Adds Attribute nodes for each level from INI file where Verify = 1
	void addXMLDataNodes(MSXML::IXMLDOMDocumentPtr ipXMLDOMDocument, MSXML::IXMLDOMNodePtr ipMain);

	// Adds DocumentInfo node
	void addXMLDocumentNode(MSXML::IXMLDOMDocumentPtr ipXMLDOMDocument, MSXML::IXMLDOMNodePtr ipMain, 
		const DocumentData& document);

	//---------------------------------------------------------------------------------------------
	// PURPOSE: Adds RedactionTextAndColorSettings
	void addXMLRedactionAppearanceNode(MSXML::IXMLDOMDocumentPtr ipXMLDOMDocument,
		MSXML::IXMLDOMNodePtr ipMain);

	// Adds VerificationInfo node
	void addXMLVerificationNode(MSXML::IXMLDOMDocumentPtr ipXMLDOMDocument, 
		MSXML::IXMLDOMNodePtr ipMain, const DocumentData& document);

	//---------------------------------------------------------------------------------------------
	// PURPOSE: Gets a string describing the font type, style, and size
	static string getFontDescription(const LOGFONT& lgFont, int iPointSize);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Gets or creates the metadata root node for the specified meta data file
	static MSXML::IXMLDOMNodePtr getRootNode(MSXML::IXMLDOMDocumentPtr ipXMLDOMDocument, 
		const string& strMetadataFile);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Creates a node with the specified name and text
	static MSXML::IXMLDOMNodePtr getTextNode(MSXML::IXMLDOMDocumentPtr ipXMLDOMDocument,
												const string& strName, const string& strText);
	// updates the m_strCurrentFile variable and the corresponding update timestamp
	void setCurrentFileName(string strFile);

	//---------------------------------------------------------------------------------------------
	// PURPOSE: Create a VOA file filled with all the attributes of the document currently being
	//          processed. Set the SourceDocName of the VOA attributes to strImageFile.
	// PARAMS:  strImageFile - the SourceDocName to which to set all the VOA attributes
	//          strVOAFile - full path to the VOA filename
	void writeVOAFile(const string& strImageFile, const string& strVOAFile);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To look through all of the exemption codes in the list and find the index
	//			of the first blank one
	// PROMISE: To return -1 if none of the exemption codes are blank otherwise
	//			to return the row index of the first blank exemption code found
	long findFirstEmptyExemptionCode();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To look through all of the redaction data types in the list and find the index
	//			of the first blank one
	// PROMISE: To return -1 if none of the redaction data types are blank otherwise
	//			to return the row index of the first blank redaction data type found
	long findFirstEmptyRedactionType();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Returns the index of the first redaction that is toggled on.
	// PROMISE: To return -1 if no redactions are toggled on; otherwise to return the row
	//          of the first toggled redaction.
	long findFirstToggledRedaction();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To look through all of the exemption codes in the list and find the index
	//			of the first one where the associated item is not redacted
	// PROMISE: To return -1 if none of the exemption codes are unwanted, 
	//			to return the row index of the first unwanted exemption code found
	long findFirstUnwantedExemptionCode();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To drop down the type data list in the combo list control for a particular row
	void showDropDownList(int nRow);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To store the new type value in the attribute and DataDisplaySetting for a
	//			particular row
	void updateCurrentlyModifiedAttribute(int nRow, string strNewType);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: If redaction types are required, checks for empty redaction types, 
	//          prompts the user about it, and navigate to the first found empty redaction.
	// PROMISE:	To return true if an empty redaction type was found.
	bool checkAndPromptForEmptyRedactionTypes(const string& strAction);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: If exemption codes are required, checks for empty exemption codes, 
	//          prompts the user about it, and navigate to the first found empty exemption.
	// PROMISE:	To return true if an empty exemption code was found.
	bool checkAndPromptForEmptyExemptionCodes(const string& strAction);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Checks for non-empty exemption codes for unredacted items, 
	//          prompts the user about it, and navigates to the first found empty exemption.
	// PROMISE:	To return true if an unwanted exemption code was found.
	bool checkAndPromptForUnwantedExemptionCodes(const string& strAction);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Displays an error message indicating that the specified action can't be performed
	//          until the specified data is filled out, and selects the specified row.
	void displayMissingDataMessage(const string& strAction, const string& strData, int iRow);

	// Delete the DataDisplaySettings objects and empty the vector
	void clearDataItemVector(vector<DataItem>& rvecDataItems);

	UCLID_REDACTIONCUSTOMCOMPONENTSLib::IIDShieldProductDBMgrPtr getIDShieldDB();

	// PURPOSE: To save a record to the IDShieldData table that uses the m_idsCurrData and
	//			m_swCurrTimeViewed
	// ADDED for (P16 2901)
	void saveIDShieldData();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To get the window handle of the spot recognition window
	HWND getSRIRWindowHandle();

	///////////////
	// Data Members
	///////////////

	// Toolbar
	auto_ptr<CToolBar>	m_apToolBar;

	// Manages tooltips
	CToolTipCtrl		m_ToolTipCtrl;

	// Grid objects for data and pages
	CTreeGridWnd		m_GridData;
	CTreeGridWnd		m_GridPages;

	// Total number of pages in active document
	int					m_iTotalPages;

	// Reference count for this COM object
	long				m_lRefCount;

	// Spot Recognition Window, OCX, and HWND associated with the dialog
	ISpotRecognitionWindowPtr	m_ipSRIR;
	_DUCLIDGenericDisplayPtr	m_ipOCX;
	HWND						m_hWndSRIR;

	// Utility object to perform Queries on collected Attributes
	IAFUtilityPtr		m_ipAFUtility;

	// Attribute Finder Engine
	IAttributeFinderEnginePtr		m_ipEngine;

	// Collection of DataConfidenceLevel items each associated 
	// with one level in INI file.  
	map<string, DataConfidenceLevel*>	m_mapDCLevels;

	// Collection of DataItem objects to contain Attributes and 
	// associated DataDisplaySettings
	vector<DataItem>				m_vecDataItems;

	// [p16 #2385]
	// a vector containing each of the attribute types loaded from the INI file
	vector<string>		m_vecAttributesFromINIFile;

	// The color for the selection border
	COLORREF m_crSelection;

	// Whether to open the voa file viewer when saving with the SHIFT key held down
	bool m_bOpenVoaOnShiftSave;

	// [p16 #2385]
	// set to hold the currently available attribute types
	set<string>			m_setAttributeTypes;

	// Stores index of last selected item within Data grid.  Allows 
	// up and down arrows to modify selection even if active page
	// contains no Attributes
	int			m_iDataItemIndex;

	// Page number of page being reviewed
	// and collection of page numbers already reviewed
	int					m_iActivePage;
	vector<int>	m_vecPagesReviewed;

	// Settings for selected Data item
	DataDisplaySettings*	m_pActiveDDS;

	// Sorted Collection of indices into m_vecDataItems
	// Always parallel to the items in the DDA Grid i.e.
	// m_vecVerifyAttributes[i] is associated with row i+1 
	// of the grid
	vector<long>		m_vecVerifyAttributes;

	// Have the dialog's controls been instantiated yet - allows for resize
	// and repositioning
	bool				m_bInitialized;

	// True if Confirmation dialog is active - disallow many functions
	volatile bool		m_bActiveConfirmationDlg;

	// Has the current image been verified
	bool				m_bVerified;

	// Manages enable/disable of relevant toolbar buttons and shortcut keys
	bool m_bEnableGeneral;
	bool m_bEnableToggle;
	bool m_bEnablePrevious;
	bool m_bEnableApplyExemptions;
	bool m_bEnableLastExemptions;
	bool m_bEnableApplyAllExemptions;

	// Options dialog
	RedactionVerificationOptionsDlg m_OptionsDlg;
	auto_ptr<CExemptionCodesDlg> m_apExemptionsDlg;

	// Last used exemption codes
	bool m_bHasLastExemptions;
	ExemptionCodeList m_lastExemptions;

	// Filename and path to INI file
	string					m_strINI;
	string					m_strINIPath;
	
	RedactionUISettings			m_UISettings;

	// string to hold the default redaction type to select
	// added as per [p16 #2834 & 2835]
	string m_strDefaultRedactionType;

	// string to hold the last redaction type selected in the drop down list
	// added as per [p16 #2835]
	string m_strLastSelectedRedactionType;

	// Start time for current file
	COleDateTime				m_tmStarted;

	// Stop watch to track time current file has be viewed
	StopWatch m_swCurrTimeViewed;

	// Contains the redaction data from the last save of the document being viewed.
	IDShieldData m_idsCurrData;

	// TimerID to set up the stopwatch
	long m_nTimerID;

	// Keeps track of the latest file to be processed. Also set to "" at the end of
	// HandleNonVerifiedInput to prompt the timer to reset it to "Waiting" on each tick
	// if there are no other files currently being processed.
	string m_strSourceDocName;

	// The document currently being processed
	DocumentData m_document;

	// the FAM file ID of the file being processed
	long m_lFileID;

	// FAM tag manager pointer
	IFAMTagManagerPtr m_ipFAMTagManager;

	IFileProcessingDBPtr m_ipFAMDB;
	
	// IDShield database to collect stats in
	UCLID_REDACTIONCUSTOMCOMPONENTSLib::IIDShieldProductDBMgrPtr m_ipIDShieldDB;

	// time the m_strCurrentFile variable was last updated
	time_t m_timeCurrentFileLastUpdated; 

	// Changes have been made to this history document
	bool m_bChangesMadeForHistory;

	// Changes have made to most recent document (ie. the document that is not in the history queue yet.)
	// Note that this flag is only updated when navigating away from the most recent document.
	bool m_bChangesMadeToMostRecentDocument;

	// Number of previously reviewed documents to store in local queue to be available
	// for user re-review.  Also, number of documents queued so far and position in queue.
	long m_nNumPreviousDocsToQueue;
	long m_nNumPreviousDocsQueued;
	long m_nPositionInQueue;

	// Indicates whether the current document has been committed to history
	bool m_bDocCommittedToHistory;

	// Collections of previously reviewed documents and their settings
	vector<DocumentData>      m_vecDocumentHistory;
	vector<vector<DataItem>>  m_vecDataItemHistory;
	vector<vector<int>>       m_vecReviewedPagesHistory;
	vector<StopWatch>         m_vecDurationsHistory;
	vector<IDShieldData>      m_vecIDShieldDataHistory;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
