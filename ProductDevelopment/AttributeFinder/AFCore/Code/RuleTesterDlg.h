
#pragma once

#include "resource.h"
#include "TesterConfigMgr.h"
#include "TesterDlgInputPage.h"
#include "TesterDlgSettingsPage.h"
#include "TesterDlgRulesetPage.h"

#include <SplitterControl.h>
#include <ResizablePropertySheet.h>
#include <TLFrame.h>

#include <string>
#include <vector>
#include <map>
#include <memory>

EXTERN_C const CLSID CLSID_RuleTesterDlg;
class FileRecoveryManager;

extern const int giCONTROL_SPACING;

/////////////////////////////////////////////////////////////////////////////
// RuleTesterDlg dialog

class RuleTesterDlg :	public CDialog,
	public CComCoClass< RuleTesterDlg, &CLSID_RuleTesterDlg >,
	public IDispatchImpl<IParagraphTextHandler, &IID_IParagraphTextHandler, &LIBID_UCLID_SPOTRECOGNITIONIRLib>
{
// Construction
public:
	RuleTesterDlg(FileRecoveryManager *pFRM, 
		UCLID_AFCORELib::IRuleSetPtr& ipRuleSet, CWnd* pParent = __nullptr);   // standard constructor
	~RuleTesterDlg();

	void setCurrentAttributeName(const std::string& strAttributeName);
	
	// Getting the internal attribute finder engine
	UCLID_AFCORELib::IAttributeFinderEnginePtr getAFEngine();

// Dialog Data
	//{{AFX_DATA(RuleTesterDlg)
	enum { IDD = IDD_DLG_TESTER };
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(RuleTesterDlg)
	public:
	virtual BOOL PreTranslateMessage(MSG* pMsg);
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual LRESULT DefWindowProc(UINT message, WPARAM wParam, LPARAM lParam);
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

	enum ERuleTesterDialogType
	{
		kWithSettingsTab,
		kWithRulesetTab
	};

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(RuleTesterDlg)
	virtual BOOL OnInitDialog();
	virtual void OnCancel() {};
	virtual void OnOK() {};
	afx_msg void OnClose();
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg void OnGetMinMaxInfo(MINMAXINFO* pMMI);
	afx_msg void OnDropFiles( HDROP hDropInfo );
	afx_msg void OnPaint();
	afx_msg void OnButtonSRIR();
	afx_msg void OnButtonClear();
	afx_msg void OnButtonExecute();
	afx_msg void OnButtonVoa();
	afx_msg void OnButtonEnableSRIR();
	afx_msg void OnButtonAbout();
	//}}AFX_MSG
	BOOL OnToolTipNotify(UINT nID, NMHDR* pNMHDR, LRESULT* pResult);
	DECLARE_MESSAGE_MAP()

protected:
	HICON m_hIcon;

private:
	// Make the RuleSet editor class into a friend so that
	// protected members like OnDropFiles() can be accessed
	friend class CRuleSetEditor;

	///////////////
	// Data members
	///////////////
	// property sheet and associated pages/variables
	ResizablePropertySheet m_propSheet;
	TesterDlgInputPage m_testerDlgInputPage;
	TesterDlgSettingsPage m_testerDlgSettingsPage;
	TesterDlgRulesetPage m_testerDlgRulesetPage;
	CSize m_propPageMinSize;
	long m_nCurrentBottomOfPropPage;

	// Tree List window
	CTLFrame		m_wndTreeList;

	// splitter control and related methods
	CSplitterControl m_splitterCtrl;
	void doResize();

	UCLID_AFCORELib::IAttributeFinderEnginePtr m_ipEngine;
	
	// Stores resulting attributes
	IIUnknownVectorPtr m_ipAttributes;

	// process dropped files
	void processDroppedFile(char *pszFile);

	// a string to store the source document filename associated with
	// the last result obtained (m_ipAttributes)
	std::string strResultAttrSourceDoc;

	// Handles configuration persistence
	std::unique_ptr<IConfigurationSettingsPersistenceMgr> ma_pUserCfgMgr;
	std::unique_ptr<TesterConfigMgr> ma_pCfgTesterMgr;

	// reference count for this COM object
	long m_lRefCount;

	std::unique_ptr<CToolBar>	m_apToolBar;

	UCLID_AFCORELib::IRuleSetPtr& m_ipRuleSet;

	FileRecoveryManager *m_pFRM;

	// Height of the toolbar
	int					m_iBarHeight;

	// Vertical space between edit box and Browse button
	int					m_iBrowseSpace;

	// Whether the dialog controls have been initialized.  This allows
	// for the controls to be moved and resized before viewing
	bool				m_bInitialized;

	CToolTipCtrl m_ToolTipCtrl;

	// Mode of the Tester dialog
	ERuleTesterDialogType	m_eMode;

	// Current Rule Execution ID
	std::string			m_strRuleID;

	// Flag to indicate tree control is being updated and do not highlight new item
	bool				m_bNoHighlight;

	std::string m_strCurHighlightImage;

	// Whether to display spot recognition or not
	bool m_bNoSpotRecognition;

	//////////
	// Methods
	//////////

	//=============================================================================
	// PURPOSE: Convert a probability long value to a readable string
	// ARGS:	The document probability in the form of a string of a long value
	// RETURNS: "Sure", "Probable", "Maybe", or "Zero" if the input parameter is
	//			valid. "Unknown" otherwise
	string docProbabilityAsText(const string &strValueAsLong);

	//=============================================================================
	// PURPOSE: To add a root-level item to the Tree List
	// REQUIRE: strName != ""
	// PROMISE: None.
	// ARGS:	strName - Text for Name column
	//				strValue - Text for Value column
	//				strType - Text for Type column
	HTREEITEM	addRootItem(const string& strName, const string& strValue, const string& strType);

	//=============================================================================
	// PURPOSE: Adds sub-attributes of specified Attribute to the list under
	//             the specified HTREEITEM
	// REQUIRE: ipAttribute != __nullptr
	// PROMISE: None.
	// ARGS:	ipAttribute - parent Attribute
	//             hItem - element in Tree List associated with ipAttribute
	//			bAutoExpand - whether elements should be auto-expanded or not
	void	addSubAttributes(UCLID_AFCORELib::IAttributePtr ipAttribute, HTREEITEM hItem,
							bool bAutoExpand);

	//=============================================================================
	// PURPOSE: To create a brand new spot rec window
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	None.
	ISpotRecognitionWindowPtr createNewSRIRWindow();
	//=============================================================================
	// PURPOSE: To create the toolbar
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	None.
	void	createToolBar();

	//=============================================================================
	// PURPOSE: Displays an Edit dialog for Attribute selected in the Tree List
	// REQUIRE: Nothing
	// PROMISE: Will display Edit dialog for the associated Attribute
	// ARGS:	None.
	void	editSelectedAttribute();

	//=============================================================================
	// PURPOSE: Load input items into combo box
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	None.
	void	loadCombo();

	//=============================================================================
	// PURPOSE: Creates Name, Value, Type columns for the Tree List
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	None.
	void	prepareTreeList();

	// Updates the state of all tool strip buttons based on whether there are found
	// attributes in the list and whether the rules are currently executing.
	void	updateButtonStates(bool bRulesExecuting = false);

	//=============================================================================
	// PURPOSE: Refresh Grid with Attribute information
	// REQUIRE: Nothing
	// PROMISE: None.
	// ARGS:	None.
	void	updateList(UCLID_AFCORELib::IAFDocumentPtr ipAFDoc);
	//=============================================================================
	// PURPOSE: Highlight the currently selected attribute
	// REQUIRE: None
	// PROMISE: If the selected attribute is a spatial attribute, then it will 
	//			be highlighted.  If the attribute is not a spatial attribute, 
	//			then all currently highlightly spatial attributes (if any) will 
	//			be "unhighlighted".
	// ARGS:	None.
	void	highlightAttributeInRow();
	//=============================================================================
	// PURPOSE: To open the specified image or GDD file
	// REQUIRE: pszFile points to a valid image or GDD file
	//			bIsGDD is true if pszFile points to a GDD file
	// PROMISE: If an image window is already open with the specified image or
	//			GDD file, it will be flashed and an error message will be shown.
	//			If the image or GDD file is not currently open, it will be
	//			opened in a new image window, the pointer to which is returned.
	ISpotRecognitionWindowPtr openImageOrGDDFile(char *pszFile, bool bIsGDD);
	//=============================================================================
	// PROMISE: Will return an input manager
	IInputManagerPtr getInputManager();
	//=============================================================================
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
