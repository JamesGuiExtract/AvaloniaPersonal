//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	IcoMapDlg.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	John Hurd
//			Duan Wang
//
//==================================================================================================
#pragma once

#include "Resource.h"

#include "IIcoMapUI.h"
#include "CommandPrompt.h"
#include "PromptEdit.h"
#include "DynamicInputGridWnd.h"

#include <EShortcutType.h>
#include <BCMenu.h>

#include <string>
#include <vector>
#include <memory>

#include "..\..\..\InputFunnel\IFCore\Code\InputManagerEventHandler.h"

class CToolTipCtrl;
class CIcoDrawingCtl;
class DrawingToolFSM;
class UCLIDExceptionDlg;
class CfgAttributeViewer;
class CfgIcoMapDlg;

// TODO: cleanup the following during the IcoMapCore project

class IcoMapDlg : public CDialog, public CommandPrompt, public IIcoMapUI,
				  public IUnknown,
				  public InputManagerEventHandler
{
public:

	// IUnknown
	STDMETHODIMP_(ULONG) AddRef();
	STDMETHODIMP_(ULONG) Release();
	STDMETHODIMP QueryInterface( REFIID iid, void FAR* FAR* ppvObj);

	static IcoMapDlg* sGetInstance();

	bool isFeatureSelectionEnabled(){return m_bViewEditFeatureSelected;}
	void onFeatureSelected(bool bFeatureReadOnly = false);

	virtual ~IcoMapDlg();

 	virtual void OnCancel(void);
	virtual void OnOK(void);

	virtual void setCommandInput(const std::string& strInput);
	// bShowDefaultValue -- whether or not to show the default value for the current prompt
	virtual void setCommandPrompt(const std::string& strText, bool bShowDefaultValue = true);

	// enable/disable toggling
	virtual void enableToggle(bool bEnableToggleDirection, bool bEnableToggleDeltaAngle);
	// enable/disable internal/deflection angle drawing line tool
	// Note: this tool will only be enabled if there's at least one segment
	// in the sketch drawing. This behavior is controlled by drawing tool FSM
	// since it's the one who observes the current sketch drawing process.
	virtual void enableDeflectionAngleTool(bool bEnable);
	virtual void enableInput(IInputValidator *pInputValidator, const char *pszPrompt);
	virtual void disableInput();
	// notify icomap about input received from sources other than SRIR, HTIR
	// or IcoMap command
	virtual void setInput(const std::string& strInput);
	// will set the toggle direction button and menu item state
	virtual void setToggleDirectionState(bool bLeft);
	// will set the toggle delta angle button menu item state
	virtual void setToggleDeltaAngleState(bool bGreaterThan180);

	void setPoint(const std::string& strPointInput);
	void setText(std::string& strText);
	void setDisplayAdapter(IDisplayAdapter* pDisplayAdapter);
	void setAttributeManager(IAttributeManager* pAttributeManager);
	void notifySketchModified(long nActualNumOfSegments);		// notify DrawingToolFSM about the sketch is modified
	void reset();
	void enableFeatureCreation(bool bEnable);
	void enableViewEditTool(BOOL bEnable);
	
	// whether or not icomap tool is the current active tool in application
	void setIcoMapAsCurrentTool(bool bIsCurrent);

	bool isIcoMapCurrentTool() {return m_bIsIcoMapCurrentTool;}
	// only process key down if focus is not on IcoMap dlg
	void processKeyDown(long lKeyCode, long lShiftKey);
	// only process key up if focus is not on IcoMap dlg
	void processKeyUp(long lKeyCode, long lShiftKey);
	
	BOOL createModelessDlg(void);

	BOOL ShowWindow(int nCmdShow);

	//////////////////////////////////////////
	// IInputReceiver related functions:

	// as input receiver, the command needs to notify the input manager
	// about input received event
	void setIREventHandler(IIREventHandler *pEventHandler);

	// whether or not enable the command line as the input receiver
	void enableCommandInputReceiver(bool bEnable);

	// set input entity manager
	void setInputEntityManager(IInputEntityManager *ipInputEntityMgr);

	// Promise: Connect icomap dlg as input receiver to the input manager
	// Require: Make sure the input receiver only be connected once
	void connectCommandInputReceiver(IInputReceiver *pInputReceiver);

	// Promise: Disconnect the icomap dlg as input receiver from the input manager
	// Require: This call must be made before IcoMap dlg is deleted.
	void disconnectCommandInputReceiver();

	///////////////////////
	// IInputTarget related functions
	void activateInputTarget();
	void deactivateInputTarget();
	bool isInputTargetWindowVisible();

	void setInputTarget(IInputTarget* pInputTarget);

// Dialog Data
	//{{AFX_DATA(IcoMapDlg)
	enum { IDD = IDD_DLG_IcoMap };
	PromptEdit	m_commandLine;
	//}}AFX_DATA
//	CStatusBarCtrl m_statusBarCtrl;


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(IcoMapDlg)
	public:
	virtual void OnFinalRelease();
	virtual BOOL PreTranslateMessage(MSG* pMsg);
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual void CalcWindowRect(LPRECT lpClientRect, UINT nAdjustType = adjustBorder);
	//}}AFX_VIRTUAL

// Implementation
protected:
	void OnBTNFinishPart();
	// Generated message map functions
	//{{AFX_MSG(IcoMapDlg)
	virtual BOOL OnInitDialog();
	afx_msg void OnClose();
	afx_msg void OnBTNOptions();
	afx_msg void OnBTNDrawingTools();
	afx_msg void OnBTNReverseMode();
	afx_msg void OnBTNToggleCurveDirection();
	afx_msg void OnBTNToggleDeltaAngle();
	afx_msg void OnHelpAbouticomap();
	afx_msg void OnToolsActionsDeletesketch();
	afx_msg void OnFileClose();
	afx_msg void OnHelpIcomaphelp();
	afx_msg void OnToolsActionsFinishpart();
	afx_msg void OnToolsActionsFinishsketch();
	afx_msg void OnToolbarDropDown(NMHDR* pNMHDR, LRESULT *plr);
	afx_msg void OnToolsOptions();
	afx_msg void OnBTNSelectFeatures();
	afx_msg void OnToolsEditfeatures();
	afx_msg void OnToolsSelectfeatures();
	afx_msg void OnToolsDrawingdirectionReverse();
	afx_msg void OnToolsDrawingdirectionNormal();
	afx_msg void OnCurveoperationsCurvedirectionConcaveleft();
	afx_msg void OnCurveoperationsCurvedirectionConcaveright();
	afx_msg void OnCurveoperationsDeltaangleGreaterthan180();
	afx_msg void OnCurveoperationsDeltaangleLessthan180();
	afx_msg void OnBTNViewEditAttributes();
	afx_msg void OnBTNFinishSketch();
	afx_msg void OnBTNDeleteSketch();
	afx_msg void OnBtnDig();
	afx_msg void OnViewDig();
	afx_msg void OnViewStatus();
	afx_msg void OnDestroy();
	afx_msg void OnToolsOcrschemes();
	afx_msg void OnActivate(UINT nState, CWnd* pWndOther, BOOL bMinimized);
	afx_msg void OnMove(int x, int y);
	afx_msg void OnShowWindow(BOOL bShow, UINT nStatus);
	afx_msg void OnSize(UINT nType, int cx, int cy);
	afx_msg void OnGetMinMaxInfo(MINMAXINFO* pMMI);
	afx_msg void OnPaint();
	DECLARE_EVENTSINK_MAP()
	//}}AFX_MSG

	afx_msg void OnSelectToolsDrawToolsMenu(UINT nID);		
	afx_msg void OnSelectDrawToolsPopupMenu(UINT nID);		
	afx_msg void OnMenuSelect(UINT nItemID, UINT nFlags, HMENU hSysMenu);
	afx_msg LRESULT OnGetWindowIRManager(WPARAM wParam, LPARAM lParam);
	afx_msg LRESULT OnExecuteCommand(WPARAM wParam, LPARAM lParam);
	BOOL OnToolTipNotify(UINT nID, NMHDR* pNMHDR, LRESULT* pResult);
	DECLARE_MESSAGE_MAP()
	
	//list of drawing tools button
	enum EDrawingToolsBtnID
	{
		kNoToolSelected = -1,
		kBtnLine = 0,
		kBtnLineAngle,
		kBtnCurve1,
		kBtnCurve2,
		kBtnCurve3,
		kBtnCurve4,
		kBtnCurve5,
		kBtnCurve6,
		kBtnCurve7,
		kBtnCurve8,
		kBtnCurveGenie,
		kTotalNumOfTools
	};

	// reference count for this COM object
	long m_lRefCount;

private:

	///////////
	// Enums
	///////////
	// What are those buttons on toolbar.
	// Note: If there's new button to be added, a new enum 
	// value must be added to this enum in the order
	// of their apperance on toolbar
	enum EToolbarBtn
	{
		kSelectFeaturesBtn = 0,
		kViewEditAttributeBtn,
		kDrawingToolsBtn,
		kDrawingDirectionBtn,
		kCurveConcavityBtn,
		kCurveDeltaAngleBtn,
		kFinishSketchBtn,
		kDeleteSketchBtn,
		kDIGBtn,
		kOptionsBtn,
		kHelpBtn,
		kNumOfBtnsOnToolbar		// this is the total number of buttons
	};

	//list of toolbar buttons
	// Note: this enum is set up in the sequence of alternative bitmaps in the original image list.
	enum EAlternativeBitmaps
	{
		kLineBmp = 0,
		kLineAngleBmp,
		kCurve1Bmp,
		kCurve2Bmp,
		kCurve3Bmp,
		kCurve4Bmp,
		kCurve5Bmp,
		kCurve6Bmp,
		kCurve7Bmp,
		kCurve8Bmp,
		kCurveGenieBmp,
		kReverseBmp,
		kConcaveRightBmp,
		kGT180Bmp
	};

	////////////
	// Constants
	////////////
	const static std::string ICOMAP_TOOL_NAME;

	// prevent construction to enforce singleton pattern
	IcoMapDlg(CWnd* pParent = NULL);   // standard constructor
	static IcoMapDlg* ms_pInstance;

	////////////
	// Variables
	////////////
	bool m_bDeltaAngleGT180;				// whether or not curve delta angles are greater than 180 or greater than 180
	bool m_bInitialized;					// whether or not OnInitDialog has completed
	bool m_bPointInputEnabled;				// whether or not to accept point input
	bool m_bTextInputEnabled;				// whether or not to accept text input
	bool m_bToggleDirectionEnabled;			// whether or not toggle direction is enabled
	bool m_bToggleDeltaAngleEnabled;		// whether or not toggle delta angle is enabled
	bool m_bToggleLeft;						// whether or not curves face left or right
	bool m_bFeatureCreationEnabled;			// whether or not current icomap control is enabled
	bool m_bViewEditFeatureSelected;		// whether or not view/edit feature is selected
	bool m_bIsIcoMapCurrentTool;			// whether or not icomap is the current active tool in the application
	bool m_bDeflectionAngleToolEnabled;		// whether or not internal/deflection angle drawing tool is enabled
	bool m_bIcoMapIsActiveInputTarget;		// whether or not IcoMap is the current active input target
	// whether or not finish sketch command shall enabled
	// It shall only be enabled if there's at least one segment in the drawing
	bool m_bFinishSketchEnabled;
	// whether or not delete sketch command shall enabled
	// It shall only be enabled if there's at least a starting point in the drawing
	bool m_bDeleteSketchEnabled;

	EDrawingToolsBtnID m_ECurrentSelectedToolID;	// current selected drawing tool
	EDrawingToolsBtnID m_EPreviousSelectedToolID;	// previous selected drawing tool
	CToolBar m_toolBar;						// has buttons on it
	BCMenu m_menuDrawingTools;				// Popup menu for drawing tools

	// ID for IcoMap dlg as command input receiver connected to the input manager
	long m_lIcoMapCommandIR;

	IDisplayAdapterPtr m_ipDisplayAdapter;		// interface pointer to the DisplayAdapter component
	IAttributeManagerPtr m_ipAttributeManager;	// interface pointer to the AttributeManager component
	std::auto_ptr<DrawingToolFSM> ma_pDrawingTool;		// processes received input

	std::auto_ptr<CToolTipCtrl> ma_pToolTipCtrl;			// provides tooltips for buttons on the dialog

	// Handles configuration persistence for this dialog
	std::auto_ptr<CfgIcoMapDlg>	ma_pCfgIcoMapDlg;

	// Handles configuration persistence for Feature Attributes dialog
	std::auto_ptr<CfgAttributeViewer> ma_pCfgAttributeViewer;

	// Handles input receiver events
	IIREventHandler* m_ipIREventHandler;

	IInputEntityManager* m_ipInputEntityMgr;

	// pointer to the singleton highlight window
	IHighlightWindowPtr m_ipHighlightWindow;

	// pointer to the singleton input target manager
	IInputTargetManagerPtr m_ipInputTargetManager;

	// pointer to the icomap input target, i.e. IcoMapCtl which implements IInputTarget
	IInputTarget* m_ipIcoMapInputTarget;

	// DIG
	std::auto_ptr<DynamicInputGridWnd> ma_pDIGWnd;

	// status bar
	CStatusBarCtrl m_statusBar;

	// static info box
	CStatic m_staticInfo;

	unsigned int m_nMinDlgHeight;
	unsigned int m_nMinDIGHeight;
	unsigned int m_nStatusBarHeight;
	unsigned int m_nCommandLineHeight;
	unsigned int m_nToolBarHeight;

	// both DIG and Status bar are invisible
	unsigned int m_nDlgHeight1;
	// DIG visible, status bar invisible
	int m_nDlgHeight2;
	// DIG invisible, status bar visible
	unsigned int m_nDlgHeight3;
	// both DIG and Status bar are visible
	int m_nDlgHeight4;

	///////////
	// Methods
	///////////
	// show splash screen, init CfgIcoMapDlg
	void init(void);

	void createToolBar(void);

	void createStatusBar();

	// after creating control bars (eg. toolbar), we need to make room 
	// for these windows in IcoMap dlg
	void repositionControlBars();

	// update the size and position of controls and windows of IcoMapDlg
	// after showing/hiding certain windows and/or resizing the dialog
	void updateWindowPosition();

	void initButtonsAndMenus(void);
	// update CurveToolsMenu text string for IDR_MNU_DRAWING_TOOLS menu
	// ex. curve1 now has tangent-in, chord bearing and radius parameters, the text
	// for this menu item should be set to <tangent-in><chord bearing><radius>.
	void updateCurveToolsMenuText(void);

	void initializeInputProcessing(void);

	void startCurveDrawing(void);
	
	void updateStateOfCurveToggleButtons(void);

	// set curve direction back to left, and delta angle back to less than 180
	void resetToggleButtonPictures();
	bool isFeatureCreationEnabled() {return m_bFeatureCreationEnabled;}

	void verifyInitialization();


	BOOL isToolbarButtonPressed(UINT nButtonID);
	// to press down/up a button on toolbar
	void pressToolbarButton(UINT nButtonID, BOOL bPressDown);
	// enable/disable a menu item from the main menu of the icomap dlg
	void enableMainMenuItem(UINT nMenuItemID, bool bEnable);
	// check/uncheck a menu item from the main menu of the icomap dlg
	void checkMainMenuItem(UINT nMenuItemID, bool bCheck);
	// update the states (press/depress, check/uncheck) for the drawing tool and view/edit 
	// attribute buttons and menu item
	// This method only updates the state of aforementioned tools, it will not call 
	// onDrawingToolSelected() or onEditAttributeSelected(), i.e. no operation will
	// be done here
	void updateDrawingToolsAndEditAttributeStates();
	// when one of the drawing tools is selected either from the toolbar or the menu
	void onDrawingToolSelected();
	// when the view/edit attribute is selected either from the toolbar or the menu
	void onEditAttributeSelected();

	// enable/disable tools whichever is applicable according to the current state
	// for instance, if it's not in editing mode, all tools need to be disabled except
	// mcr text viewer, image recognition, select feature, options, help, view/edit 
	// attributes (partially, i.e. view only)
	void enableDrawingTools(BOOL bEnable);
	void enableDrawingDirectionsTool(BOOL bEnable);
	// Finish sketch, delete sketch and finish part items
	void enableSketchOperationTools(BOOL bEnable);
	// update these tools based on m_bFinishSketchEnabled and m_bDeleteSketchEnabled
	void updateSketchOperationTools();
	void enableToggleCurveDirectionTools(BOOL bEnable);
	void enableToggleCurveDeltaAngleTools(BOOL bEnable);
	// sets the toggle direction buttons and menu items state
	void setToggleDirectionButtonState();
	// sets the toggle delta angle buttons and menu items state
	void setToggleDeltaAngleButtonState();

	// updates status text
	void setStatusText(LPCTSTR pszText);

	// take the EShortcutType and convert and execute it as real command
	// Return true if succeedes
	bool executeShortcutCommand(EShortcutType eShortcutType);

	// process whatever's been entered from command line
	void processInput(const std::string &strOriginInput);

	// show/hide highlight window
	void showHighlightWindow(bool bShow = true);

	// IInputReceiver related function
	// Once text input is received, fire OnInputRecieved event
	void fireOnInputReceived(const std::string& strTextInput);

	HRESULT __stdcall NotifyInputReceived(ITextInput* pTextInput);

	// pointer to the icomap input context object
	IInputContextPtr m_ipIcoMapInputContext;

///////////////////////
// Dynamic Input Grid
///////////////////////
	// initialize the DIG
	void initGrid();

	// set focus on command line if current focus is on DIG
	void switchFocusFromDIGToCommandLine();
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
