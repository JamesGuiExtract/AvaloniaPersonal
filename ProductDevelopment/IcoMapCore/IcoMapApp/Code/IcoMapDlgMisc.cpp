//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	IcoMapDlgMisc.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Duan Wang (June 2001 - present)
//			Arvind Ganesan (Aug 2001 - present)
//			John Hurd (till July 2001)
//
//==================================================================================================

#include "stdafx.h" 
#include "IcoMapDlg.h"
#include "IcoMapApp.h"

#include "COGOInputInterpreter.h"
#include "ShortcutInterpreter.h"
#include "CurrentCurveTool.h"
#include "CurveToolManager.h"
#include "DrawingToolFSM.h"

#include <IcoMapOptions.h>
#include <UCLIDException.h>
#include <AbstractMeasurement.hpp>
#include <LicenseMgmt.h>
#include <Win32Util.h>
#include <AfxAppMainWindowRestorer.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

using namespace std;

//--------------------------------------------------------------------------------------------------
void IcoMapDlg::checkMainMenuItem(UINT nMenuItemID, bool bCheck)
{
	UINT nCheck;
	if (bCheck)
	{
		nCheck = (MF_CHECKED | MF_BYCOMMAND);
	}
	else
	{
		nCheck = (MF_UNCHECKED | MF_BYCOMMAND);
	}

	CMenu*	pMenu = GetMenu();
	if (pMenu)
	{
		pMenu->CheckMenuItem(nMenuItemID, nCheck);
	}
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::enableDrawingTools(BOOL bEnable)
{
	m_toolBar.GetToolBarCtrl().EnableButton(IDC_BTN_SelectFeatures, bEnable);
	m_toolBar.GetToolBarCtrl().EnableButton(IDC_BTN_ViewEditAttributes, bEnable);

	m_toolBar.GetToolBarCtrl().EnableButton(IDC_BTN_DrawingTools, bEnable);

	enableMainMenuItem(ID_TOOLS_DRAWINGTOOLS_LINE, bEnable == TRUE ? true : false);
	enableMainMenuItem(ID_TOOLS_DRAWINGTOOLS_LINEANGLE, 
				(bEnable == TRUE && m_bDeflectionAngleToolEnabled)? true : false);
	enableMainMenuItem(ID_TOOLS_DRAWINGTOOLS_CURVE1, bEnable == TRUE ? true : false);
	enableMainMenuItem(ID_TOOLS_DRAWINGTOOLS_CURVE2, bEnable == TRUE ? true : false);
	enableMainMenuItem(ID_TOOLS_DRAWINGTOOLS_CURVE3, bEnable == TRUE ? true : false);
	enableMainMenuItem(ID_TOOLS_DRAWINGTOOLS_CURVE4, bEnable == TRUE ? true : false);
	enableMainMenuItem(ID_TOOLS_DRAWINGTOOLS_CURVE5, bEnable == TRUE ? true : false);
	enableMainMenuItem(ID_TOOLS_DRAWINGTOOLS_CURVE6, bEnable == TRUE ? true : false);
	enableMainMenuItem(ID_TOOLS_DRAWINGTOOLS_CURVE7, bEnable == TRUE ? true : false);
	enableMainMenuItem(ID_TOOLS_DRAWINGTOOLS_CURVE8, bEnable == TRUE ? true : false);
	enableMainMenuItem(ID_TOOLS_DRAWINGTOOLS_GENIE, bEnable == TRUE ? true : false);
	enableMainMenuItem(ID_TOOLS_EDITFEATURES, bEnable == TRUE ? true : false);
	enableMainMenuItem(ID_TOOLS_SELECTFEATURES, bEnable == TRUE ? true : false);
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::enableDrawingDirectionsTool(BOOL bEnable)
{
	m_toolBar.GetToolBarCtrl().EnableButton(IDC_BTN_ReverseMode, bEnable);

	enableMainMenuItem(ID_TOOLS_DRAWINGDIRECTION_NORMAL, bEnable == TRUE ? true : false);
	enableMainMenuItem(ID_TOOLS_DRAWINGDIRECTION_REVERSE, bEnable == TRUE ? true : false);
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::enableMainMenuItem(UINT nMenuItemID, bool bEnable)
{
	CMenu *pMenu = GetMenu();
	UINT nEnable;
	if (bEnable)
	{
		nEnable = (MF_BYCOMMAND | MF_ENABLED);
	}
	else
	{
		nEnable = (MF_BYCOMMAND | MF_GRAYED);
	}

	pMenu->EnableMenuItem(nMenuItemID, nEnable);
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::enableSketchOperationTools(BOOL bEnable)
{
	bool bEnableFinishSketch = (bEnable ==TRUE? true : false) && m_bFinishSketchEnabled;
	bool bEnableDeleteSketch = (bEnable ==TRUE? true : false) && m_bDeleteSketchEnabled;
	m_toolBar.GetToolBarCtrl().EnableButton(IDC_BTN_FinishSketch, 
								bEnableFinishSketch ? TRUE : FALSE);
	m_toolBar.GetToolBarCtrl().EnableButton(IDC_BTN_DeleteSketch, 
								bEnableDeleteSketch ? TRUE : FALSE);

	enableMainMenuItem(ID_TOOLS_ACTIONS_FINISHPART, bEnableFinishSketch);
	enableMainMenuItem(ID_TOOLS_ACTIONS_FINISHSKETCH, bEnableFinishSketch);
	enableMainMenuItem(ID_TOOLS_ACTIONS_DELETESKETCH, bEnableDeleteSketch);
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::updateSketchOperationTools()
{
	m_toolBar.GetToolBarCtrl().EnableButton(IDC_BTN_FinishSketch, m_bFinishSketchEnabled ? TRUE : FALSE);
	m_toolBar.GetToolBarCtrl().EnableButton(IDC_BTN_DeleteSketch, m_bDeleteSketchEnabled ? TRUE : FALSE);

	enableMainMenuItem(ID_TOOLS_ACTIONS_FINISHPART, m_bFinishSketchEnabled);
	enableMainMenuItem(ID_TOOLS_ACTIONS_FINISHSKETCH, m_bFinishSketchEnabled);
	enableMainMenuItem(ID_TOOLS_ACTIONS_DELETESKETCH, m_bDeleteSketchEnabled);
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::enableToggleCurveDirectionTools(BOOL bEnable)
{
	// only go into the IF block if the bool value is different
	if (m_toolBar.GetToolBarCtrl().IsButtonEnabled(IDC_BTN_ToggleCurveDirection) != bEnable)
	{
		m_toolBar.GetToolBarCtrl().EnableButton(IDC_BTN_ToggleCurveDirection, bEnable);
		
		enableMainMenuItem(ID_CURVEOPERATIONS_CURVEDIRECTION_CONCAVELEFT, bEnable == TRUE ? true : false);
		enableMainMenuItem(ID_CURVEOPERATIONS_CURVEDIRECTION_CONCAVERIGHT, bEnable == TRUE ? true : false);

		// update the state of the toggle direction
		setToggleDirectionButtonState();
	}
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::enableToggleCurveDeltaAngleTools(BOOL bEnable)
{	
	if (m_toolBar.GetToolBarCtrl().IsButtonEnabled(IDC_BTN_ToggleDeltaAngle) != bEnable)
	{
		m_toolBar.GetToolBarCtrl().EnableButton(IDC_BTN_ToggleDeltaAngle, bEnable);
		
		enableMainMenuItem(ID_CURVEOPERATIONS_DELTAANGLE_GREATERTHAN180, bEnable == TRUE ? true : false);
		enableMainMenuItem(ID_CURVEOPERATIONS_DELTAANGLE_LESSTHAN180, bEnable == TRUE ? true : false);

		// update the state of toggle delta angle
		setToggleDeltaAngleButtonState();
	}
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::setToggleDirectionButtonState()
{
	int nImage;
	UINT nResourceID, nStyle;
	//index of the drawing tools btn on the toolbar
	int nIndex = m_toolBar.CommandToIndex(IDC_BTN_ToggleCurveDirection);
	m_toolBar.GetButtonInfo(nIndex, nResourceID, nStyle, nImage);
	
	// set current toggle inputs
	ma_pDrawingTool->setCurrentToggleInputs(m_bToggleLeft, m_bDeltaAngleGT180);
	
	if (m_bToggleLeft)
	{
		m_toolBar.SetButtonInfo(nIndex, nResourceID, nStyle, (int)kCurveConcavityBtn);
	}
	else
	{
		// !!!NOTE: be very careful here... fourth parameter value
		// must be accurate in order to get the proper image for the button.
		// (The fourth parameter is the index for the button's image within the toolbar image list.)
		m_toolBar.SetButtonInfo(nIndex, nResourceID, nStyle, (int)kNumOfBtnsOnToolbar + (int)kConcaveRightBmp);
	}
	
	checkMainMenuItem(ID_CURVEOPERATIONS_CURVEDIRECTION_CONCAVELEFT, m_bToggleLeft);
	checkMainMenuItem(ID_CURVEOPERATIONS_CURVEDIRECTION_CONCAVERIGHT, !m_bToggleLeft);
	
	// set curve toggle
	ECurveToggleInput eToggleDirection = kToggleCurveDirectionIsLeft;
	if (!m_bToggleLeft)
	{
		eToggleDirection = kToggleCurveDirectionIsRight;
	}
	ma_pDrawingTool->setCurrentCurveToggleInputType(eToggleDirection);
	
	// set angle toggle
	EDeflectionAngleToggleInput eAngleToggleType = kToggleAngleLeft;
	if (!m_bToggleLeft)
	{
		eAngleToggleType = kToggleAngleRight;
	}
	ma_pDrawingTool->setCurrentAngleToggleInputType(eAngleToggleType);
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::setToggleDeltaAngleButtonState()
{
	int nImage;
	UINT nResourceID, nStyle;
	int nIndex = m_toolBar.CommandToIndex(IDC_BTN_ToggleDeltaAngle);
	m_toolBar.GetButtonInfo(nIndex, nResourceID, nStyle, nImage);
	
	// set current toggle inputs
	ma_pDrawingTool->setCurrentToggleInputs(m_bToggleLeft, m_bDeltaAngleGT180);
	
	// update the button and menu item for the toggle curve/angle
	if (!m_bDeltaAngleGT180)
	{
		m_toolBar.SetButtonInfo(nIndex, nResourceID, nStyle, kCurveDeltaAngleBtn);
	}
	else
	{			
		// !!!NOTE: be very careful here... fourth parameter value
		// must be accurate in order to get the proper image for the button.
		// (The fourth parameter is the index for the button's image within the toolbar image list.)
		m_toolBar.SetButtonInfo(nIndex, nResourceID, nStyle, (int)kNumOfBtnsOnToolbar + (int)kGT180Bmp);
	}
	
	checkMainMenuItem(ID_CURVEOPERATIONS_DELTAANGLE_LESSTHAN180, !m_bDeltaAngleGT180);
	checkMainMenuItem(ID_CURVEOPERATIONS_DELTAANGLE_GREATERTHAN180, m_bDeltaAngleGT180);
	
	ECurveToggleInput eToggleDelta = kToggleDeltaIsLessThan180Degrees;
	if (m_bDeltaAngleGT180)
	{
		eToggleDelta = kToggleDeltaIsGreaterThan180Degrees;
	}
	ma_pDrawingTool->setCurrentCurveToggleInputType(eToggleDelta);
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::enableViewEditTool(BOOL bEnable)
{
	m_toolBar.GetToolBarCtrl().EnableButton(IDC_BTN_ViewEditAttributes, bEnable);
	enableMainMenuItem(ID_TOOLS_EDITFEATURES, bEnable == TRUE ? true : false);
}
//--------------------------------------------------------------------------------------------------
bool IcoMapDlg::executeShortcutCommand(EShortcutType eShortcutType)
{
	bool bSuccess = false;

	UINT nToolID = 0;

	// remember the window which has the focus, so that we can restore focus later

	ForegroundWindowRestorer();

	switch (eShortcutType)
	{
	case kShortcutCurve1:
		{
			nToolID = kBtnCurve1;
			OnSelectDrawToolsPopupMenu((int)nToolID + ID_DRAWINGTOOL_LINE);
			bSuccess = true;
		}
		break;
	case kShortcutCurve2:
		{
			nToolID = kBtnCurve2;
			OnSelectDrawToolsPopupMenu((int)nToolID + ID_DRAWINGTOOL_LINE);
			bSuccess = true;
		}
		break;
	case kShortcutCurve3:
		{
			nToolID = kBtnCurve3;
			OnSelectDrawToolsPopupMenu((int)nToolID + ID_DRAWINGTOOL_LINE);
			bSuccess = true;
		}
		break;
	case kShortcutCurve4:
		{
			nToolID = kBtnCurve4;
			OnSelectDrawToolsPopupMenu((int)nToolID + ID_DRAWINGTOOL_LINE);
			bSuccess = true;
		}
		break;
	case kShortcutCurve5:
		{
			nToolID = kBtnCurve5;
			OnSelectDrawToolsPopupMenu((int)nToolID + ID_DRAWINGTOOL_LINE);
			bSuccess = true;
		}
		break;
	case kShortcutCurve6:
		{
			nToolID = kBtnCurve6;
			OnSelectDrawToolsPopupMenu((int)nToolID + ID_DRAWINGTOOL_LINE);
			bSuccess = true;
		}
		break;
	case kShortcutCurve7:
		{
			nToolID = kBtnCurve7;
			OnSelectDrawToolsPopupMenu((int)nToolID + ID_DRAWINGTOOL_LINE);
			bSuccess = true;
		}
		break;
	case kShortcutCurve8:
		{
			nToolID = kBtnCurve8;
			OnSelectDrawToolsPopupMenu((int)nToolID + ID_DRAWINGTOOL_LINE);
			bSuccess = true;
		}
		break;
	case kShortcutLine:
		{
			nToolID = kBtnLine;
			OnSelectDrawToolsPopupMenu((int)nToolID + ID_DRAWINGTOOL_LINE);
			bSuccess = true;
		}
		break;
	case kShortcutLineAngle:
		{
			UINT nMenuState = m_menuDrawingTools.GetMenuState(ID_DRAWINGTOOL_LINE_ANGLE, MF_BYCOMMAND);
			// if the menu item is disabled
			if (nMenuState & MF_GRAYED)
			{
				throw UCLIDException("ELI02936", "Line Tool for Internal/Deflection Angle is disabled at this time.");
			}
			
			nToolID = kBtnLineAngle;
			OnSelectDrawToolsPopupMenu((int)nToolID + ID_DRAWINGTOOL_LINE);
			bSuccess = true;
		}
		break;
	case kShortcutGenie:
		{
			nToolID = kBtnCurveGenie;
			OnSelectDrawToolsPopupMenu((int)nToolID + ID_DRAWINGTOOL_LINE);
			bSuccess = true;
		}
		break;
	case kShortcutRight:
		{
			// only available if the current tool is curve and it can be toggled left/right
			if (m_toolBar.GetToolBarCtrl().IsButtonEnabled(IDC_BTN_ToggleCurveDirection))
			{
				if (m_bToggleLeft)
				{
					// only toggle the curve to the right if it's currently to the left
					OnBTNToggleCurveDirection();
				}

				bSuccess = true;
			}
			else
			{
				throw UCLIDException("ELI01860", "Toggling curve direction is disabled at this time.");
			}
		}
		break;
	case kShortcutLeft:
		{
			// only available if the current tool is curve and it can be toggled left/right
			if (m_toolBar.GetToolBarCtrl().IsButtonEnabled(IDC_BTN_ToggleCurveDirection))
			{
				if (!m_bToggleLeft)
				{
					// only toggle the curve to the left if it's currently to the right
					OnBTNToggleCurveDirection();
				}
				
				bSuccess = true;
			}
			else
			{
				throw UCLIDException("ELI01862", "Toggling curve direction is disabled at this time.");
			}
		}
		break;
	case kShortcutGreater:
		{
			if (m_toolBar.GetToolBarCtrl().IsButtonEnabled(IDC_BTN_ToggleDeltaAngle))
			{
				if (!m_bDeltaAngleGT180)
				{
					OnBTNToggleDeltaAngle();
				}
				
				bSuccess = true;
			}
			else
			{
				throw UCLIDException("ELI01861", "Toggling curve delta angle is disabled at this time.");
			}
		}
		break;
	case kShortcutLess:
		{
			if (m_toolBar.GetToolBarCtrl().IsButtonEnabled(IDC_BTN_ToggleDeltaAngle))
			{
				if (m_bDeltaAngleGT180)
				{
					OnBTNToggleDeltaAngle();
				}

				bSuccess = true;
			}
			else
			{
				throw UCLIDException("ELI01863", "Toggling curve delta angle is disabled at this time.");
			}
		}
		break;
	case kShortcutForward:
		{
			if (m_toolBar.GetToolBarCtrl().IsButtonEnabled(IDC_BTN_ReverseMode))
			{
				// only toggle to forward if it was reverse mode
				if (AbstractMeasurement::isInReverseMode())
				{
					// toggle reverse/normal
					OnBTNReverseMode();
				}
				bSuccess = true;
			}
			else
			{
				throw UCLIDException("ELI01865", "Can not set drawing direction at this time.");
			}
		}
		break;
	case kShortcutReverse:
		{
			if (m_toolBar.GetToolBarCtrl().IsButtonEnabled(IDC_BTN_ReverseMode))
			{
				// only toggle to reverse if it was forward mode
				if (!AbstractMeasurement::isInReverseMode())
				{
					// toggle reverse/normal
					OnBTNReverseMode();
				}
				bSuccess = true;
			}
			else
			{
				throw UCLIDException("ELI01866", "Can not set drawing direction at this time.");
			}
		}
		break;
	case kShortcutFinishSketch:
		{
			OnBTNFinishSketch();
			bSuccess = true;
		}
		break;
	case kShortcutFinishPart:
		{
			OnBTNFinishPart();
			bSuccess = true;
		}
		break;
	case kShortcutDeleteSketch:
		{
			OnBTNDeleteSketch();
			bSuccess = true;
		}
		break;
//	These shortcuts do not have keyboard equivalents: they are used only
//	in the context of speech recognition
	case kShortcutUndo:
		{
			m_ipDisplayAdapter->Undo();
			bSuccess = true;
		}
		break;
	case kShortcutRedo:
		{
			m_ipDisplayAdapter->Redo();
			bSuccess = true;
		}
		break;
	case kShortcutEnter:
		{
			processInput("");
			bSuccess = true;
		}
		break;
	default:
		break;
	}

	return bSuccess;
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::initButtonsAndMenus()
{	
	CurveToolManager& curveToolMgr = CurveToolManager::sGetInstance();
	// getting menu strings from curve tools' settings
	CString cstrMenuString("");
	// init popup menu for drawing tools 
	// If AfxGetApp()->m_pMainWnd returns a NULL inside BCMenu.cpp and CWnd LoadMenu will 
	// throw an exception so we will temporarily set the CWinApp mainWnd ptr to this so 
	// that AfxGetApp()->m_pMainWnd never returns NULL (P16: 2261)
	// Please read http://www.experts-exchange.com/Programming/Programming_Languages/MFC/Q_20102369.html
	// for detailed description
	AfxAppMainWindowRestorer tempRestorer;
	AfxGetApp()->m_pMainWnd = this;

	m_menuDrawingTools.LoadMenu(IDR_MNU_DRAWING_TOOLS);
	m_menuDrawingTools.ModifyODMenu(NULL, ID_DRAWINGTOOL_LINE, IDB_BMP_LINE);
	m_menuDrawingTools.ModifyODMenu(NULL, ID_DRAWINGTOOL_LINE_ANGLE, IDB_BMP_LINE_ANGLE);
	cstrMenuString = curveToolMgr.getCurveTool(kCurve1)->generateToolTip().c_str();
	m_menuDrawingTools.ModifyODMenu(cstrMenuString, ID_DRAWINGTOOL_CURVE1, IDB_BMP_Curve1);
	cstrMenuString = curveToolMgr.getCurveTool(kCurve2)->generateToolTip().c_str();
	m_menuDrawingTools.ModifyODMenu(cstrMenuString, ID_DRAWINGTOOL_CURVE2, IDB_BMP_Curve2);
	cstrMenuString = curveToolMgr.getCurveTool(kCurve3)->generateToolTip().c_str();
	m_menuDrawingTools.ModifyODMenu(cstrMenuString, ID_DRAWINGTOOL_CURVE3, IDB_BMP_Curve3);
	cstrMenuString = curveToolMgr.getCurveTool(kCurve4)->generateToolTip().c_str();
	m_menuDrawingTools.ModifyODMenu(cstrMenuString, ID_DRAWINGTOOL_CURVE4, IDB_BMP_Curve4);
	cstrMenuString = curveToolMgr.getCurveTool(kCurve5)->generateToolTip().c_str();
	m_menuDrawingTools.ModifyODMenu(cstrMenuString, ID_DRAWINGTOOL_CURVE5, IDB_BMP_Curve5);
	cstrMenuString = curveToolMgr.getCurveTool(kCurve6)->generateToolTip().c_str();
	m_menuDrawingTools.ModifyODMenu(cstrMenuString, ID_DRAWINGTOOL_CURVE6, IDB_BMP_Curve6);
	cstrMenuString = curveToolMgr.getCurveTool(kCurve7)->generateToolTip().c_str();
	m_menuDrawingTools.ModifyODMenu(cstrMenuString, ID_DRAWINGTOOL_CURVE7, IDB_BMP_Curve7);
	cstrMenuString = curveToolMgr.getCurveTool(kCurve8)->generateToolTip().c_str();
	m_menuDrawingTools.ModifyODMenu(cstrMenuString, ID_DRAWINGTOOL_CURVE8, IDB_BMP_Curve8);
	m_menuDrawingTools.ModifyODMenu(NULL, ID_DRAWINGTOOL_GENIE, IDB_BMP_GENIE);

	// Very first time icomap dlg is loaded, line tool is the default drawing tool and the 
	// drawing tool button must be pushed down. 
	pressToolbarButton(IDC_BTN_DrawingTools, TRUE);
	checkMainMenuItem(ID_TOOLS_DRAWINGTOOLS_LINE, true);
	m_menuDrawingTools.CheckMenuItem(ID_DRAWINGTOOL_LINE, MF_CHECKED);
	
	// initially, disable internal/deflection angle tool 
	enableDeflectionAngleTool(false);

	//Also make sure the view/edit button is up
	pressToolbarButton(IDC_BTN_ViewEditAttributes, FALSE);

	// sketch operation tools
	updateSketchOperationTools();

	// update state of DIG and status bar button and menu items
	bool bDIGVisible = IcoMapOptions::sGetInstance().isDIGVisible();
	bool bStatusVisible = IcoMapOptions::sGetInstance().isStatusBarVisible();

	pressToolbarButton(IDC_BTN_DIG, bDIGVisible ? TRUE : FALSE);

	// check/uncheck the menu item
	CMenu *pMenu = GetMenu();
	UINT nCheckDIG = bDIGVisible ? MF_CHECKED : MF_UNCHECKED;
	UINT nCheckStatusBar = bStatusVisible ? MF_CHECKED : MF_UNCHECKED;
	pMenu->CheckMenuItem(ID_VIEW_DIG, nCheckDIG);
	pMenu->CheckMenuItem(ID_VIEW_STATUS, nCheckStatusBar);
}
//--------------------------------------------------------------------------------------------------
BOOL IcoMapDlg::isToolbarButtonPressed(UINT nButtonID)
{
	BOOL bPressed = m_toolBar.GetToolBarCtrl().IsButtonChecked(nButtonID) 
					|| m_toolBar.GetToolBarCtrl().IsButtonPressed(nButtonID);

	return bPressed;
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::onEditAttributeSelected()
{
	// first verify successful initialization
	verifyInitialization();

	// Disable some buttons if view/edit is active
	enableDrawingDirectionsTool(FALSE);
	enableToggleCurveDirectionTools(FALSE);
	enableToggleCurveDeltaAngleTools(FALSE);
	enableSketchOperationTools(!m_bViewEditFeatureSelected);

	// before disable DIG, make sure it doesn't have the focus
	if (m_bViewEditFeatureSelected)
	{
		switchFocusFromDIGToCommandLine();
	}

	// enable the command prompt window only if it's not in view/edit mode
	m_commandLine.EnableWindow(m_bViewEditFeatureSelected ? FALSE : TRUE);
	ma_pDIGWnd->EnableWindow(m_bViewEditFeatureSelected ? FALSE : TRUE);

	// first clear the command line input text
	m_commandLine.SetInputText("");
	m_commandLine.SetPromptText("Select a feature for viewing/editing its attributes.");
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::pressToolbarButton(UINT nButtonID, BOOL bPressDown)
{
	if (m_toolBar)
	{
		m_toolBar.GetToolBarCtrl().CheckButton(nButtonID, bPressDown);
	}
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::processInput(const string &strOriginInput)
{
	string strInput(strOriginInput);
	COGOInputInterpreter cogoInterpreter;
	ShortcutInterpreter shortcutInterpreter;
	

	// 1) if input is not empty, first check if it is a shortcut command;
	// interpret shortcut commands first
	if (!strInput.empty())
	{
		EShortcutType eShortcutType = shortcutInterpreter.interpretShortcutCommand(strInput);
		// if it is some kind of shortcut command and the command can be executed
		if (eShortcutType != kShortcutNull && executeShortcutCommand(eShortcutType))
		{
			// if it's a shortcut command, cleanup the command line a little bit
			m_commandLine.SetInputText("");
 		
			// don't want to do the rest part of the function
			return;
		}
	}

	EInputType eExpectedInputType = ma_pDrawingTool->getCurrentExpectedInputType();

	// 2) if input is COGO shortcut for bearing or angle
	// if only current direction type is defined as bearing
	if (!strInput.empty())
	{
		switch (eExpectedInputType)
		{
		case kBearing:
			switch (IcoMapOptions::sGetInstance().getInputDirection())
			{
			case 1:		// kBearingDir
				{
					// in case the input might be the COGO shortcut symbols for bearing or angle
					strInput = cogoInterpreter.interpretCOGOInput(strInput, eExpectedInputType);
				}
				break;
			case 2:		// kPolarAngleDir
			case 3:		// kAzimuthDir
				{
					// even though these two are directions, still, they are type of angle
					strInput = cogoInterpreter.interpretCOGOInput(strInput, kAngle);
				}
				break;
			}
			break;
		case kAngle:
			// in case the input might be the COGO shortcut symbols for bearing or angle
			strInput = cogoInterpreter.interpretCOGOInput(strInput, eExpectedInputType);
			break;
		}
	}
		
	// 3) if input is not empty, check if current state is expecting point input
	// if users type in coordinates instead of using mouse to select
	// a starting point in the drawing, we should allow them to do that	
	if (eExpectedInputType == kPoint) 
	{
		if (strInput.empty())
		{
			// get default point value
			strInput = ma_pDrawingTool->getCurrentDefaultCurveParameterValue();
		}

		// set the starting point
		setPoint(strInput);
		
		// don't want to do the rest part of the function
		return;
	}
	
	// echo default value for current type of input
	if (strInput.empty())
	{
		// get default value from input manager
		strInput = ma_pDrawingTool->getCurrentDefaultCurveParameterValue();
	}
	
	if (!strInput.empty())
	{
		// notify input manager about input received.
		// At this point, the text input string has only been pre-processed (i.e.
		// translated into formal input string, for instance, formal bearing
		// input, angle input etc.). The text string has not been validated yet.
		fireOnInputReceived(strInput);
	}
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::setStatusText(LPCTSTR pszText)
{
	string	strTitle( "IcoMap" );

	// Check for blank string
	if (pszText[0] == 0)
	{
		// Just load the default string
		SetWindowText( strTitle.c_str() );
	}
	else
	{
		// Create a combination string for the title bar
		strTitle = strTitle + " - " + pszText;

		SetWindowText( strTitle.c_str() );
	}
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::showHighlightWindow(bool bShow)
{
	if (::IsWindow(m_hWnd))
	{
		if (bShow)
		{
			m_ipHighlightWindow->Show((long) m_hWnd, NULL);
			
			// check if the dlg is disabled
			if (!m_bViewEditFeatureSelected 
				&& m_bFeatureCreationEnabled)
			{
				// use the default color, which is for "input enabled"
				m_ipHighlightWindow->UseDefaultColor();
			}
			else
			{
				// use the alternate color 1, which is for "input disabled"
				m_ipHighlightWindow->UseAlternateColor1();
			}
			
			::RedrawWindow(m_hWnd, NULL, NULL, 
				RDW_ERASE|RDW_INVALIDATE|RDW_FRAME|RDW_INTERNALPAINT|RDW_UPDATENOW);
		}
		else
		{
			// hide the highlight window
			m_ipHighlightWindow->HideAndForget();
		}
	}
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::updateCurveToolsMenuText(void)
{
	// before popping up the drawing tool menu on toolbar,
	// let's check if there's any curve tool's parameter 
	// setting changed
	CurveToolManager& curveToolManager = CurveToolManager::sGetInstance();
	// if one or more curve tools have their parameter set changed (usually through curve genie),
	// get the ids of these updated curve tools for updating text of menu items for each curve tool.
	vector<ECurveToolID> vecUpdatedCurveToolIDs = curveToolManager.getUpdatedCurveToolIDs();
	if (!vecUpdatedCurveToolIDs.empty())
	{
		vector<ECurveToolID>::iterator iter;
		for (iter = vecUpdatedCurveToolIDs.begin(); iter != vecUpdatedCurveToolIDs.end(); iter++)
		{
			// retrieve the curve tool's parameter setting
			CString cstrMenuString = curveToolManager.getCurveTool(*iter)->generateToolTip().c_str();
			UINT nChecked;
			
			switch (*iter)
			{
			case kCurve1:
				{
					// since modify menu will reset the state of the menu item, get the state first
					nChecked = m_menuDrawingTools.GetMenuState(ID_DRAWINGTOOL_CURVE1, MF_BYCOMMAND);
					m_menuDrawingTools.ModifyODMenu(cstrMenuString, ID_DRAWINGTOOL_CURVE1, IDB_BMP_Curve1);
					m_menuDrawingTools.CheckMenuItem(ID_DRAWINGTOOL_CURVE1, nChecked);
				}
				break;
			case kCurve2:
				{
					nChecked = m_menuDrawingTools.GetMenuState(ID_DRAWINGTOOL_CURVE2, MF_BYCOMMAND);
					m_menuDrawingTools.ModifyODMenu(cstrMenuString, ID_DRAWINGTOOL_CURVE2, IDB_BMP_Curve2);
					m_menuDrawingTools.CheckMenuItem(ID_DRAWINGTOOL_CURVE2, nChecked);
				}
				break;
			case kCurve3:
				{
					nChecked = m_menuDrawingTools.GetMenuState(ID_DRAWINGTOOL_CURVE3, MF_BYCOMMAND);
					m_menuDrawingTools.ModifyODMenu(cstrMenuString, ID_DRAWINGTOOL_CURVE3, IDB_BMP_Curve3);
					m_menuDrawingTools.CheckMenuItem(ID_DRAWINGTOOL_CURVE3, nChecked);
				}
				break;
			case kCurve4:
				{
					nChecked = m_menuDrawingTools.GetMenuState(ID_DRAWINGTOOL_CURVE4, MF_BYCOMMAND);
					m_menuDrawingTools.ModifyODMenu(cstrMenuString, ID_DRAWINGTOOL_CURVE4, IDB_BMP_Curve4);
					m_menuDrawingTools.CheckMenuItem(ID_DRAWINGTOOL_CURVE4, nChecked);
				}
				break;
			case kCurve5:
				{
					nChecked = m_menuDrawingTools.GetMenuState(ID_DRAWINGTOOL_CURVE5, MF_BYCOMMAND);
					m_menuDrawingTools.ModifyODMenu(cstrMenuString, ID_DRAWINGTOOL_CURVE5, IDB_BMP_Curve5);
					m_menuDrawingTools.CheckMenuItem(ID_DRAWINGTOOL_CURVE5, nChecked);
				}
				break;
			case kCurve6:
				{
					nChecked = m_menuDrawingTools.GetMenuState(ID_DRAWINGTOOL_CURVE6, MF_BYCOMMAND);
					m_menuDrawingTools.ModifyODMenu(cstrMenuString, ID_DRAWINGTOOL_CURVE6, IDB_BMP_Curve6);
					m_menuDrawingTools.CheckMenuItem(ID_DRAWINGTOOL_CURVE6, nChecked);
				}
				break;
			case kCurve7:
				{
					nChecked = m_menuDrawingTools.GetMenuState(ID_DRAWINGTOOL_CURVE7, MF_BYCOMMAND);
					m_menuDrawingTools.ModifyODMenu(cstrMenuString, ID_DRAWINGTOOL_CURVE7, IDB_BMP_Curve7);
					m_menuDrawingTools.CheckMenuItem(ID_DRAWINGTOOL_CURVE7, nChecked);
				}
				break;
			case kCurve8:
				{
					nChecked = m_menuDrawingTools.GetMenuState(ID_DRAWINGTOOL_CURVE8, MF_BYCOMMAND);
					m_menuDrawingTools.ModifyODMenu(cstrMenuString, ID_DRAWINGTOOL_CURVE8, IDB_BMP_Curve8);
					m_menuDrawingTools.CheckMenuItem(ID_DRAWINGTOOL_CURVE8, nChecked);
				}
				break;
			default:
				{
					UCLIDException uclidException("ELI01485", "Unexcepted ECurveToolID here!");
					uclidException.addDebugInfo("eCurveToolID", *iter);
					throw uclidException;
				}
				break;
			}
		}

		//clear the vec of updated tool ids since the text for the curve tool(s) has just been updated.
		// By this way, the vector is always holding most recent curve tool id(s) that has new set of parameters.
		curveToolManager.clearUpdatedCurveToolIDs();
	}
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::updateDrawingToolsAndEditAttributeStates()
{
	// since we only have two modes here: either draw sketch or view/edit attributes
	// check if view/edit is enabled.

	// view/edit button
	pressToolbarButton(IDC_BTN_ViewEditAttributes, m_bViewEditFeatureSelected);
	// update menu items: view/edit menu item and drawing tool menu item
	checkMainMenuItem(ID_TOOLS_EDITFEATURES, m_bViewEditFeatureSelected);
	
	// the drawing tool button
	pressToolbarButton(IDC_BTN_DrawingTools, !m_bViewEditFeatureSelected);
	// check/uncheck current tool menu item: line or curve1 or curve2, etc...
	// only check/uncheck the current tool

	// menu item from the menu bar
	checkMainMenuItem(ID_TOOLS_DRAWINGTOOLS_LINE + (int) m_ECurrentSelectedToolID, 
		!m_bViewEditFeatureSelected);
	
	// menu item from the drawing tools popup menu
	UINT nCheck = MF_UNCHECKED;
	if (!m_bViewEditFeatureSelected)
	{
		nCheck = MF_CHECKED;
	}
	m_menuDrawingTools.CheckMenuItem(ID_DRAWINGTOOL_LINE + (int)m_ECurrentSelectedToolID, nCheck);
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::updateStateOfCurveToggleButtons(void)
{
	// toggle buttons can't be enabled if it's not in feature creation mode
	bool bToggleDirectionEnabled = 
		(m_bToggleDirectionEnabled 
		&& !m_bViewEditFeatureSelected 
		&& m_bFeatureCreationEnabled ) ? true : false;
	
	enableToggleCurveDirectionTools(bToggleDirectionEnabled ? TRUE : FALSE);
	enableToggleCurveDeltaAngleTools(m_bToggleDeltaAngleEnabled ? TRUE : FALSE);
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::switchFocusFromDIGToCommandLine()
{
	// if focus is on grid, deactivate it
	CWnd* pFocusedWnd = GetFocus();
	if (ma_pDIGWnd.get() != NULL && pFocusedWnd != NULL && 
		(ma_pDIGWnd->m_hWnd == pFocusedWnd->m_hWnd || 
		 (pFocusedWnd->GetParent() != NULL && 
		 ma_pDIGWnd->m_hWnd == pFocusedWnd->GetParent()->m_hWnd)
		)
	   )
	{
		m_commandLine.SetFocus();
	}
}
//--------------------------------------------------------------------------------------------------
