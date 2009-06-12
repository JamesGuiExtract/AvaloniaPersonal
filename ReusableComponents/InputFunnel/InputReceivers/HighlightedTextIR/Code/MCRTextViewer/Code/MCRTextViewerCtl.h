//===========================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	MCRTextViewerCtrl.h
//
// PURPOSE:	The purpose of this file is to implement the interface that needs 
//			to be supported by the UCLID MCRTextViewer ActiveX control.  This 
//			is a header file for CUCLIDMCRTextViewerCtrl where this class has 
//			been derived from the CActiveXDocControl()class.  The code in this 
//			file implements the various methods.
// NOTES:	
//
// AUTHORS:	
//
//===========================================================================
#pragma once

#include "Activdoc.h"

#include <vector>
#include <string>
#include <map>

// The InputTokenPosition struct contains information
// about the starting and ending character positions of
// text that is considered as "MCR" text.
struct InputTokenPosition
{
	long m_lStartPos, m_lEndPos;
};

/////////////////////////////////////////////////////////////////////////////
// CUCLIDMCRTextViewerCtrl : See MCRTextViewerCtl.cpp for implementation.

class CMCRDocument;
class CMCRFrame;
class CMCRView;

//===========================================================================
//
// CLASS:	CUCLIDMCRTextViewerCtrl
//
// PURPOSE:	To provide the functionality that allows the user to select MCR 
//			text from a notepad like window that automatically highlights 
//			MCR'able text making it easier for the user to spot MCR'able 
//			text.
//
// REQUIRE:	Nothing.
// 
// INVARIANTS:
//			The ActiveX control is always in one of two modes - "edit-text" 
//			mode, or "view-text" mode.  Whenever the ActiveX control is in 
//			the "edit-text" mode, the text cursor is visible and is availble 
//			for user manipulation.  Whenever the ActiveX control is in the 
//			"edit-text" mode, the MCRTextViewer displays the text just like 
//			Notepad would - the MCR'able text is not highlighted (and neither 
//			does the ActiveX control spend time identifying MCR'able text).  
//			Whenever the ActiveX control is in "view-text" mode, all MCR'able 
//			text is automatically highlighted in yellow color (which is an 
//			internal programmable constant).
//
// EXTENSIONS:
//			None.
//
// NOTES:	When in "edit-text" mode the user shall be able to edit the text 
//			in the MCRTextViewer window just like the user will be able to 
//			edit text in Notepad.  When in "view-text" mode, the ActiveX 
//			control shall operate in "read-only" mode, and shall not allow 
//			the user to modify the text in the MCRTextViewer window.  Further, 
//			when the user switches the control from the "edit-text" mode to 
//			the "view-text" mode, the ActiveX control will use the 
//			UCLIDMCRTextFinder ActiveX control to automatically identify 
//			MCR'able text in the MCRTextViewer window, and shall automatically 
//			highlight the identified MCR'able text.
//
class CUCLIDMCRTextViewerCtrl : public CActiveXDocControl
{
	DECLARE_DYNCREATE(CUCLIDMCRTextViewerCtrl)

// Constructor
public:

	//=============================================================================
	// PURPOSE: To initialize the ActiveX control.
	// REQUIRE: Nothing.
	// PROMISE: The ActiveX control will be initialized to be in "edit-text" mode.
	// ARGS:	None.
	CUCLIDMCRTextViewerCtrl();

	//=============================================================================
	// PURPOSE: To cleanup the ActiveX control.
	// REQUIRE: Nothing.
	// PROMISE: Nothing.
	// ARGS:	None.
	virtual ~CUCLIDMCRTextViewerCtrl();

	//=============================================================================
	// PURPOSE: To set the current opened file path
	// REQUIRE: The specified file should be opened
	// PROMISE: The file path will be stored into this variable and will used for 
	//				further implementations.
	// ARGS:	None.
	void setCurrentFile(CString zFileName) { m_zCurFile = zFileName;}

	//=============================================================================
	// PURPOSE: To retrieve the current opened file path
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	None.
	const CString getCurFilePath() {return m_zCurFile;}

	//=============================================================================
	// PURPOSE: To get the file status 
	// REQUIRE: Nothing
	// PROMISE: It will get the current file status. i.e., whether any file is 
	//				opened or not.
	// ARGS:	None.
	const bool getFileStatus() { return m_bFileOpened;}

	//=============================================================================
	// PURPOSE: To set the file status 
	// REQUIRE: Nothing
	// PROMISE: It will set the current file status. i.e., whether any file is 
	//				opened or not.
	// ARGS:	None.
	void setFileStatus (bool bValue) { m_bFileOpened = bValue;}

	//=============================================================================
	// PURPOSE: To get the Background color of the MCR Text
	// REQUIRE: The control should be in the view mode
	// PROMISE: It will get the current Background color of the MCR Text
	// ARGS:	None.
	const COLORREF getMCRBkColor() {return m_MCRBkColor;}

	//=============================================================================
	// PURPOSE: To set the Background color of the MCR Text
	// REQUIRE: The control should be in the view mode
	// PROMISE: It will set the current Background color of the MCR Text
	// ARGS:	newMCRBkColor: the new background color.
	void setMCRBkColor(COLORREF newMCRBkColor) {m_MCRBkColor = newMCRBkColor;}

	//=============================================================================
	// PURPOSE: To get the Highlight color of the MCR Text
	// REQUIRE: The control should be in the view mode
	// PROMISE: It will get the current HighLight color of the MCR Text
	// ARGS:	None.
	const COLORREF getMCRHighlightColor() {return m_MCRHighlightColor;}

	//=============================================================================
	// PURPOSE: To get the mode of selection
	// REQUIRE: Nothing
	// PROMISE: It will get selection mode of the current text
	// ARGS:	None.
	bool getTextSelectionMode() {return m_bEnableSelection;}

	//=============================================================================
	// PURPOSE: To reset current text font and size
	// REQUIRE:	This function should be called every time increaseTextSize, 
	//				decreaseTextSize, setTextFontName or setTextSize is called
	// PROMISE: 
	// ARGS:	None.
	void resetTextFontAndSize();

	//=============================================================================
	// PURPOSE: To get the mode of view
	// REQUIRE: Nothing
	// PROMISE: It will get mode of view. i.e., whether it is in view mode or not
	bool getViewMode() {return m_bViewMode;}

	//=============================================================================
	// PURPOSE: To set the mode of view
	// REQUIRE: Nothing
	// PROMISE: It will set mode of view. i.e., whether it should be in view mode or not
	void setViewMode(bool bValue) {m_bViewMode = bValue;}

	//=============================================================================
	// PURPOSE: To get current related MCRFrame object
	// REQUIRE:	
	// PROMISE:
	// ARGS:	None.
	CMCRFrame* getCMCRFrame(){return m_pMCRFrame;}

	//=============================================================================
	// PURPOSE: To get current related MCRView object
	// REQUIRE:	
	// PROMISE:
	// ARGS:	None.
	CMCRView* getCMCRView(){return m_pMCRView;}

	//=============================================================================
	// PURPOSE: To get the current set of start/end positions for all highlighted text
	// REQUIRE:	Nothing
	// PROMISE: To return the set of start/end positions for all highlighted text
	//			that corresponding to the currently selected InputFinder.
	// ARGS:	None.
	inline std::vector<InputTokenPosition>& getCurrentTokenPositions()
	{
		if (m_ipFinder == NULL)
		{
			AfxThrowOleDispatchException(0, "ELI03883: No input finder object associated with this control!");
		}

		return m_mapInputFinderToTokenPositions[m_ipFinder];
	}

	//=============================================================================
	// PURPOSE: Parses text using current Input Finder object
	// REQUIRE:	
	// PROMISE:
	// ARGS:	None.
//	void parseText();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CUCLIDMCRTextViewerCtrl)
	public:
	virtual void OnDraw(CDC* pdc, const CRect& rcBounds, const CRect& rcInvalid);
	virtual void DoPropExchange(CPropExchange* pPX);
	virtual void OnResetState();
	//}}AFX_VIRTUAL

// Implementation
protected:

	DECLARE_OLECREATE_EX(CUCLIDMCRTextViewerCtrl)    // Class factory and guid
	DECLARE_OLETYPELIB(CUCLIDMCRTextViewerCtrl)      // GetTypeInfo
	DECLARE_PROPPAGEIDS(CUCLIDMCRTextViewerCtrl)     // Property page IDs
	DECLARE_OLECTLTYPE(CUCLIDMCRTextViewerCtrl)		// Type name and misc status

// Message maps
	//{{AFX_MSG(CUCLIDMCRTextViewerCtrl)
	afx_msg int OnCreate(LPCREATESTRUCT lpCreateStruct);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

public:
// Dispatch maps
	//{{AFX_DISPATCH(CUCLIDMCRTextViewerCtrl)
	afx_msg void open(LPCTSTR strFileName);
	afx_msg void save();
	afx_msg void saveAs(LPCTSTR strFileName);
	afx_msg void clear();
	afx_msg void parseText();
	afx_msg void enableTextSelection(long ulValue);
	afx_msg BSTR getEntityText(long ulTextEntityID);
	afx_msg void pasteTextFromClipboard();
	afx_msg void copyTextToClipboard();
	afx_msg void setTextFontName(LPCTSTR strFontName);
	afx_msg BSTR getTextFontName();
	afx_msg void setTextSize(long ulTextSize);
	afx_msg long getTextSize();
	afx_msg void increaseTextSize();
	afx_msg void decreaseTextSize();
	afx_msg void appendTextFromClipboard();
	afx_msg void setText(LPCTSTR strNewText);
	afx_msg void appendText(LPCTSTR strNewText);
	afx_msg long isModified();
	afx_msg void print();
	afx_msg BSTR getFileName();
	afx_msg void setEntityText(long ulTextEntityID, LPCTSTR strNewText);
	afx_msg void setTextHighlightColor(long ulTextEntityID, OLE_COLOR newTextHighlightColor);
	afx_msg long isEntityIDValid(unsigned long ulTextEntityID);
	afx_msg void setInputFinder(LPUNKNOWN pInputFinder);
	afx_msg OLE_COLOR getEntityColor(long ulTextEntityID);
	afx_msg void setEntityColor(long ulTextEntityID, OLE_COLOR newTextColor);
	afx_msg BSTR getText();
	afx_msg BSTR getSelectedText();
	//}}AFX_DISPATCH
	DECLARE_DISPATCH_MAP()

public:
// Event maps
	//{{AFX_EVENT(CUCLIDMCRTextViewerCtrl)
	void FireTextSelected(long ulTextEntityID)
		{FireEvent(eventidTextSelected,EVENT_PARAM(VTS_I4), ulTextEntityID);}
	void FireSelectedText(LPCTSTR strText)
		{FireEvent(eventidSelectedText,EVENT_PARAM(VTS_BSTR), strText);}
	//}}AFX_EVENT
	DECLARE_EVENT_MAP()

// Dispatch and event IDs
public:
	enum {
	//{{AFX_DISP_ID(CUCLIDMCRTextViewerCtrl)
	dispidOpen = 1L,
	dispidSave = 2L,
	dispidSaveAs = 3L,
	dispidClear = 4L,
	dispidParseText = 5L,
	dispidEnableTextSelection = 6L,
	dispidGetEntityText = 7L,
	dispidPasteTextFromClipboard = 8L,
	dispidCopyTextToClipboard = 9L,
	dispidSetTextFontName = 10L,
	dispidGetTextFontName = 11L,
	dispidSetTextSize = 12L,
	dispidGetTextSize = 13L,
	dispidIncreaseTextSize = 14L,
	dispidDecreaseTextSize = 15L,
	dispidAppendTextFromClipboard = 16L,
	dispidSetText = 17L,
	dispidAppendText = 18L,
	dispidIsModified = 19L,
	dispidPrint = 20L,
	dispidGetFileName = 21L,
	dispidSetEntityText = 22L,
	dispidSetTextHighlightColor = 23L,
	dispidIsEntityIDValid = 24L,
	dispidSetInputFinder = 25L,
	dispidGetEntityColor = 26L,
	dispidSetEntityColor = 27L,
	dispidGetText = 28L,
	dispidGetSelectedText = 29L,
	eventidTextSelected = 1L,
	eventidSelectedText = 2L,
	//}}AFX_DISP_ID
	};

private:
	//=============================================================================
	// PURPOSE: To remove all background color from text.
	// REQUIRE:	This function should be called every time parseText() is called.
	// PROMISE: 
	// ARGS:	None.
	void clearTextBackground();

	// Current MCRFrame object
	CMCRFrame*	m_pMCRFrame;

	// Current MCRView object from MCRFrame object
	CMCRView*	m_pMCRView;

	// Background color of the MCR text
	COLORREF m_MCRBkColor;

	// Highlight color of the MCR text
	COLORREF m_MCRHighlightColor;

	// Font style of the text in control
	std::string m_strFontName;

	// Font size of the text in control
	int m_iTextSize;

	// To enable or disable text selection
	bool m_bEnableSelection;

	// since the user may switch between input finders at any time
	// we don't want to reparse the text and find MCR text every time
	// a switch is made.  We only want to do a new parse when we are switching
	// to an inputfinder for the first time with a given piece of input text
	// the following map keeps track of the start/end positions of MCR text
	// for each input finder.  The map is cleared (and repopulated as necessary) 
	// whenver the textual contents of the window is modified.
	std::map<IInputFinder *, std::vector<InputTokenPosition> > m_mapInputFinderToTokenPositions;

	// Current file name in the view window
	CString m_zCurFile;

	// To know whether a file is opened or not
	bool m_bFileOpened;

	// To know whether control is in View-mode or Edit-mode
	bool m_bViewMode;

	// Input Finder used for parsing text as appropriate
	IInputFinderPtr m_ipFinder;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
