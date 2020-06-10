// EAVGeneratorDlg.h : header file
//

#pragma once

#include "..\..\..\..\InputFunnel\IFCore\Code\InputManagerEventHandler.h"
#include <ImageButtonWithStyle.h>
#include <WindowPersistenceMgr.h>

#include <memory>
#include <set>
#include <string>
#include <vector>
#include <afxwin.h>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CEAVGeneratorDlg dialog

class CEAVGeneratorDlg : public CDialog, 
	public InputManagerEventHandler,
	public IDispatchImpl<IParagraphTextHandler, &IID_IParagraphTextHandler, &LIBID_UCLID_SPOTRECOGNITIONIRLib>
{
// Construction
public:
	CEAVGeneratorDlg(CWnd* pParent = NULL);	// standard constructor
	~CEAVGeneratorDlg();

// Dialog Data
	//{{AFX_DATA(CEAVGeneratorDlg)
	enum { IDD = IDD_EAVGENERATOR_DIALOG };
	CEdit	m_editValue;
	CEdit	m_editType;
	CEdit	m_editName;
	CEdit	m_editAttributePath;
	CEdit	m_editAttributeGUID;
	CListCtrl	m_listAttributes;
	CImageButtonWithStyle	m_btnUp;
	CButton	m_btnAdd;
	CButton	m_btnCopy;
	CImageButtonWithStyle	m_btnDown;
	CButton	m_btnDelete;
	CButton	m_btnSplit;
	CString	m_zName;
	CString	m_zType;
	CString	m_zValue;
	CButton m_btnMerge;
	CString m_zAttributePath;
	CString m_zAttributeGUID;
	CString m_zAttributeSDN;
	CStatic m_labelFilename;
	CButton m_radioReplace;
	CButton m_radioAppend;
	CStatic m_labelAttributePath;
	CStatic m_currentFilename;
	CButton m_valueGroup;
	CStatic m_labelName;
	CStatic m_labelType;
	CStatic m_labelValue;
	CStatic m_labelAttributeGUID;
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CEAVGeneratorDlg)
	public:
	virtual BOOL PreTranslateMessage(MSG* pMsg);
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support
	//}}AFX_VIRTUAL

public:
// IUnknown
	STDMETHODIMP_(ULONG ) AddRef();
	STDMETHODIMP_(ULONG ) Release();
	STDMETHODIMP QueryInterface( REFIID iid, void FAR* FAR* ppvObj);

// IParagraphTextHandler
	STDMETHOD(raw_NotifyParagraphTextRecognized)(ISpotRecognitionWindow *pSourceSRWindow, ISpatialString *pText);
	STDMETHOD(raw_GetPTHDescription)(BSTR *pstrDescription);
	STDMETHOD(raw_IsPTHEnabled)(VARIANT_BOOL *pbEnabled);

// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	//{{AFX_MSG(CEAVGeneratorDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnDropFiles( HDROP hDropInfo );
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	afx_msg void OnBtnAdd();
	afx_msg void OnBtnCopy();
	afx_msg void OnBtnDelete();
	afx_msg void OnBtnSplit();
	afx_msg void OnBtnDown();
	afx_msg void OnBtnUp();
	afx_msg void OnItemchangedListDisplay(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnChangeEditName();
	afx_msg void OnChangeEditType();
	afx_msg void OnChangeEditValue();
	afx_msg void OnClose();
	afx_msg void OnBtnReplace();
	afx_msg void OnBtnAppend();
	afx_msg void OnBtnImagewindow();
	afx_msg void OnBtnNew();
	afx_msg void OnBtnOpen();
	afx_msg void OnBtnSavefile();
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg void OnBtnHighlight();
	afx_msg void OnBtnMerge();
	afx_msg void OnOK();
	afx_msg void OnCancel();
	afx_msg void OnKeydownListDisplay(NMHDR* pNMHDR, LRESULT* pResult);
	afx_msg void OnBtnSaveas();
	afx_msg void OnNMDblclkListDisplay(NMHDR *pNMHDR, LRESULT *pResult);
	afx_msg void OnGetMinMaxInfo(MINMAXINFO* lpMMI);
	//}}AFX_MSG
	BOOL OnToolTipNotify(UINT nID, NMHDR* pNMHDR, LRESULT* pResult);
	DECLARE_MESSAGE_MAP()

protected:

//  InputManagerEventHandler
	virtual HRESULT __stdcall NotifyInputReceived(ITextInput* pTextInput);

private:

	//----------------------------------------------------------------------------------------------
	// Constants
	//----------------------------------------------------------------------------------------------
	// static identifiers for the columns in the display list
	static const int	NAME_COLUMN	= 0;
	static const int	VALUE_COLUMN = 1;
	static const int	TYPE_COLUMN = 2;
	static const int	SPATIALNESS_COLUMN = 3;
	static const int	PAGE_COLUMN = 4;

	// static constants used to specify whether to open or not open the SRW
	static const bool gbOPEN_SRW = true;
	static const bool gbDO_NOT_OPEN_SRW = false;

	// Dialog size bounds
	static const int giEAVGENDLG_MIN_WIDTH = 638;
	static const int giEAVGENDLG_MIN_HEIGHT = 608;

	//----------------------------------------------------------------------------------------------
	// Data
	//----------------------------------------------------------------------------------------------
	bool m_bAutoOpenImageEnabled;

	// Names of image files with currently selected highlights
	vector<string> m_vecstrCurrentImageWithHighlights;

	// whether or not an SRIR is opened with ""
	bool m_bEmptyStringOpened;

	// whether or not current file is modified
	bool m_bFileModified;

	// Will new text replace existing text in Value edit box
	bool m_bReplaceValueText;

	// Toolbar
	unique_ptr<CToolBar> m_apToolBar;

	// Manages tooltips
	CToolTipCtrl m_ToolTipCtrl;

	// reference count for this COM object
	long m_lRefCount;

	// currently opened file name
	string m_strCurrentFileName;

	// Collection of current nodes for storing new Attributes
	vector<IIUnknownVectorPtr> m_vecCurrentLevels;

	// Used to open new instances of the spot recognition window
	ISRIRUtilsPtr m_ipSRIRUtils;
	
	// Whether the dialog controls have been initialized.  This allows
	// for the controls to be moved and resized before viewing
	bool m_bInitialized;

	// Initial width and heights used to dock controls
	int m_nDefaultW;
	int m_nDefaultH;

	// Window position mgr
	WindowPersistenceMgr m_wMgr;

	// GUID's of attributes currently loaded
	set<string> m_setOfGUIDs;

	// Handle of timer used for scheduling attribute selection
	UINT_PTR m_nTimer;

	//////////
	// Methods
	//////////

	// Add specified Attribute to active collection of Attributes or 
	// SubAttributes at the specified level
	void addAttributeLevel(IAttributePtr ipNewAttribute, int iLevel);

	// Adds the specified string to the specified set if the string is non-empty
	static void addToSetIfNonEmpty(set<string>& rsetStrings, const _bstr_t& bstrString);

	// Adds sub-attributes of specified Attribute to the list at the position
	// where the number of items under the insertion point is always as 
	// specified.  Subattribute Names are prefaced with one period for each 
	// specified sublevel.
	void addSubAttributes(IAttributePtr ipAttribute, 
						  int iNumItemsUnderInsertionPoint, 
						  int iSubLevel);

	// Moves subattributes at a level >= uiLevelToMove starting at the nMoveFrom position 
	// to after the nInsertAfter position.  The move stops at the first item < uiLevelToMove
	void moveSubAttributes (int nInsertAfter, int nMoveFrom, unsigned int uiLevelToMove);

	// Creates the toolbar
	void createToolBar();

	//---------------------------------------------------------------------------------------------
	// PURPOSE: Deletes the temporary highlights in currently open images
	void deleteTemporaryHighlights();

	// Displays the specified Attributes in the list box
	void displayAttributes(IIUnknownVectorPtr ipAttributes);

	// Gets the level of the specified item in the list control
	// Root attributes are 0, the children of root attributes are 1, grandchildren are 2, etc.
	unsigned int getAttributeLevel(int iIndex);

	// return current selected list item
	int getCurrentSelectedListItemIndex();

	//---------------------------------------------------------------------------------------------
	// PURPOSE: Lazily instantiates an image utilities object.
	ISRIRUtilsPtr getImageUtils();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Returns the image viewer corresponding to the specified image file.
	// PARAMS:  strImage - The name of the image file to open
	//          bOpenWindow - If the image viewer is not already open, true opens and returns a new 
	//          image viewer and false returns NULL. If the image viewer is already open, both 
	//          true and false return the open image viewer.
	ISpotRecognitionWindowPtr getImageViewer(const string& strImage, bool bOpenWindow);

	// Checks each attribute for any that are completely empty
	bool isAnyEmptyAttribute();

	// Returns true if all selected attributes are siblings (same parent); false otherwise.
	bool isSiblingsSelected();

	// Combines the specified set of strings into a single string using the specified delimiter.
	static string joinStrings(const set<string>& setStringsToJoin, char cDelimiter);

	// read input EAV file and display Attributes in the list box
	void openEAVFile(const CString& zFileName);

	// read input VOA file and display Attributes in the list box
	void openVOAFile(const CString& zFileName);

	// prompt the user to save current file before proceeding to the next task
	bool promptForSaving();

	// save all attributes from the list
	void saveAttributes(const CString& zFileName);

	// save all attributes from the list into an EAV file
	void saveAttributesToEAV(const CString& zFileName);

	// save all attributes from the list into a VOA file
	void saveAttributesToVOA(const CString& zFileName);

	// selects the specified item (and deselects all other items) in the list control
	void selectListItem(int iIndex);

	// updates the state for Add, Delete, Up, Down and Save buttons
	void updateButtons();

	// updates the list item
	// nColumnNumber -- which column to update
	void updateList(int nColumnNumber, const CString& zText);

	// Takes care of locking the file and updating the UI to reflect having the specified filename
	// open.
	void setCurrentFileName(const CString& zFileName);

	// a method to update the window caption depending upon the currently
	// loaded file (if any)
	void updateWindowCaption(const CString& zFileName);

	// validate the attributes in the list
	bool validateAttributes();

	// PURPOSE: To highlight the selected attribute in the SRW. if bOpenWindow==true open the 
	// SRW if it is not already open
	void highlightAttributeInRow(bool bOpenWindow);

	// PURPOSE: to build the full path to the source image for the current attribute
	//
	// PROMISE: to throw an exception if no valid file can be found
	//
	// NOTE: by default will look for the image file in the local directory, and if that is not found
	//		 will use the source document stored in the spatial string
	//		 (i.e. if ipValue->SourceDocName = K:\Customer\images\123.tif 
	//		 and current directory is C:\test will return C:\test\123.tif 
	//		 if C:\test\123.tif exists otherwise will return K:\Customer\images\123.tif)
	string getSourceImageName(ISpatialStringPtr ipValue);

	// PURPOSE: To return the HWND for the SpotRecognitionWindow pointed to by ipSRIR
	HWND getWindowHandleFromSRIR(ISpotRecognitionWindowPtr ipSRIR);

	// PURPOSE: To generate an image name from the currently opened VOA file name.
	//			Attempts to generate the file name by dropping the .voa extension.
	//			(e.g. VOA file name = C:\test\123.tif.voa will return C:\test\123.tif).
	//			If there is no open VOA file returns ""
	//
	// NOTE:	Does not check for file existence. 
	string generateImageNameFromOpenVOAFile();

	// PURPOSE: To create an instance of the SpotRecognitionWindow opened with the
	//			ImageFile name
	void openSRWWithImage(const string& strImageFile);

	// PURPOSE: To clear the list box
	//
	// PROMISE: To make a call to release for each IAttributePtr in the list control
	//			and to clear all items from the list control
	void clearListControl();

	// PURPOSE: To either append to or replace the current attribute in the list with a new attribute
	//			from a text swipe in the SRW
	void appendOrReplaceAttribute(IAttributePtr ipNewAttribute);

	// PURPOSE: To enable or disable the input manager based on the list selection state
	void enableOrDisableInputManager();

	// PURPOSE: To open the SRW with an image file name generated from the currently open VOA file
	//			if a valid file can be found
	void openSRWFromVOAFileName();

	// PURPOSE: To convert the specified spatial mode to a string
	string getModeAsString(ESpatialStringMode eMode);

	// PURPOSE: To return the attribute path in the parameter strAttributePath at the index iIndex
	string getAttributePath(int iIndex);

	// PURPOSE: To return the attribute level of the attribute at index iIndex
	int getLevel(int iIndex);

	// PURPOSE: To return the name of the attribute at index iIndex
	string getName(int iIndex);

	// PURPOSE: To handle moving/resizing the controls
	void doResize();

	// PURPOSE: To add attribute instanceGUID's to the set of GUID's and log an exception if
	// a GUID is duplicated
	void addGUIDToSet(IIdentifiableObjectPtr ipIdentityObject);

	void showUsage();
public:
	afx_msg void OnTimer(UINT_PTR nIDEvent);
};
//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
