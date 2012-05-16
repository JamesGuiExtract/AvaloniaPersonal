//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	IcoMapDlgMFCEventHandlers.cpp
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

#include "CurrentCurveTool.h"
#include "CurveToolManager.h"
#include "CurveWizardDlg.h"
#include "DrawingToolFSM.h"
#include "IcoMapOptionsDlg.h"
#include "AboutIcoMapDlg.h"
#include "SelectFeaturesDlg.h"
#include "CfgIcoMapDlg.h"

#include <IcoMapOptions.h>
#include <UCLIDException.h>
#include <AbstractMeasurement.hpp>
#include <TemporaryResourceOverride.h>
#include <cpputil.h>
#include <RegistryPersistenceMgr.h>
#include <DistanceCore.h>
#include <RegConstants.h>
#include <SafeNetLicenseMgr.h>
#include <AfxAppMainWindowRestorer.h>

#import "IcoMapApp.tlb"
using namespace ICOMAPAPPLib;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

using namespace std;

extern HINSTANCE gModuleResource;

//--------------------------------------------------------------------------------------------------
void IcoMapDlg::onDrawingToolSelected()
{
	// first verify successful initialization
	verifyInitialization();
	
	enableDrawingDirectionsTool(TRUE);
	enableSketchOperationTools(!m_bViewEditFeatureSelected);

	ECurveToolID eCurveToolID = kCurrentCurveTool;
	
	if (m_ECurrentSelectedToolID != kNoToolSelected)
	{
		switchFocusFromDIGToCommandLine();
	}

	switch (m_ECurrentSelectedToolID)
	{
	case kNoToolSelected:	//initially no tool is selected when first time icomap dlg is created
		//set line tool is selected if user clicks this drawing tool btn
		OnSelectDrawToolsPopupMenu(ID_DRAWINGTOOL_LINE);	
		return;
		break;
	case kBtnLine:	//Line
		{
			if (m_bFeatureCreationEnabled)
			{
				ma_pDrawingTool->startLineDrawing();
			}
			m_EPreviousSelectedToolID = kBtnLine;
			return;
		}
		break;
	case kBtnLineAngle:		// Line drawn using internal/deflection angle
		{
			// draw the line with internal/deflection angle !!!
			ma_pDrawingTool->startLineDeflectionAngleDrawing();
			m_EPreviousSelectedToolID = kBtnLineAngle;
			return;
		}
		break;
	case kBtnCurve1:		//Curve1
		eCurveToolID = kCurve1;
		m_EPreviousSelectedToolID = kBtnCurve1;
		break;
	case kBtnCurve2:		//Curve2
		eCurveToolID = kCurve2;
		m_EPreviousSelectedToolID = kBtnCurve2;
		break;
	case kBtnCurve3:		//Curve3
		eCurveToolID = kCurve3;
		m_EPreviousSelectedToolID = kBtnCurve3;
		break;
	case kBtnCurve4:		//Curve4
		eCurveToolID = kCurve4;
		m_EPreviousSelectedToolID = kBtnCurve4;
		break;
	case kBtnCurve5:		//Curve5
		eCurveToolID = kCurve5;
		m_EPreviousSelectedToolID = kBtnCurve5;
		break;
	case kBtnCurve6:		//Curve6
		eCurveToolID = kCurve6;
		m_EPreviousSelectedToolID = kBtnCurve6;
		break;
	case kBtnCurve7:		//Curve7
		eCurveToolID = kCurve7;
		m_EPreviousSelectedToolID = kBtnCurve7;
		break;
	case kBtnCurve8:		//Curve8
		eCurveToolID = kCurve8;
		m_EPreviousSelectedToolID = kBtnCurve8;
		break;
	case kBtnCurveGenie:	//Curve Genie
		{
			CurveWizardDlg  dlgWizard;
			if (dlgWizard.DoModal() == IDOK)
			{
				startCurveDrawing();
			}
			else
			{
				if (m_EPreviousSelectedToolID != kNoToolSelected && m_EPreviousSelectedToolID != kBtnCurveGenie)
				{
					// roll back
					OnSelectDrawToolsPopupMenu((int)m_EPreviousSelectedToolID + ID_DRAWINGTOOL_LINE);
				}
				else if (m_EPreviousSelectedToolID == kBtnCurveGenie)
				{
					startCurveDrawing();
				}
			}
			m_EPreviousSelectedToolID = kBtnCurveGenie;
			return;
			break;
		}
	default:
		return;
		break;
	}
	
	CurveToolManager& curveToolManager = CurveToolManager::sGetInstance();
	curveToolManager.initializeCurrentCurveTool(curveToolManager.getCurveTool(eCurveToolID));
	startCurveDrawing();
}
//--------------------------------------------------------------------------------------------------
/////////////////////////////////////////////////////////////////////////////
// IcoMapApp message handlers

void IcoMapDlg::OnBTNDrawingTools() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		// make sure icomap drawing tool is the current active tool selected 
		// in the application platform
		if (!m_bIsIcoMapCurrentTool)
		{
			m_ipDisplayAdapter->SelectTool(_bstr_t(ICOMAP_TOOL_NAME.c_str()));
		}
		
		if (!isToolbarButtonPressed(IDC_BTN_DrawingTools))
		{
			m_bViewEditFeatureSelected = false;

			// update the drawing tool view/edit attributes button state
			updateDrawingToolsAndEditAttributeStates();

			// do some operations here...
			onDrawingToolSelected();
		}
		// bring up the curve genie even if the button is pressed down
		else if (m_ECurrentSelectedToolID == kBtnCurveGenie)
		{
			onDrawingToolSelected();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI01876")
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnBTNOptions() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		int nPageNum = IcoMapOptions::sGetInstance().getActiveOptionPageNum();
		IcoMapOptionsDlg optionsSheet("IcoMap Options", this, nPageNum);

		if (optionsSheet.DoModal() == IDOK)
		{
			// if the distance unit setting has been changed
			EDistanceUnitType eNewUnit = IcoMapOptions::sGetInstance().getDefaultDistanceUnitType();
			if (eNewUnit != DistanceCore::getDefaultDistanceUnit())
			{
				DistanceCore::setDefaultDistanceUnit(eNewUnit);
			}

			// if and only if feature create is enabled, we shall refresh the command prompt
			if (m_bFeatureCreationEnabled && ! m_bViewEditFeatureSelected)
			{
				ma_pDrawingTool->refreshPrompt();
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI01879")
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnBTNReverseMode() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		int nImage;
		UINT nResourceID, nStyle;
		//index of the drawing tools btn on the toolbar
		int nIndex = m_toolBar.CommandToIndex(IDC_BTN_ReverseMode);
		m_toolBar.GetButtonInfo(nIndex, nResourceID, nStyle, nImage);

		// toggle reverse/normal mode accordingly
		AbstractMeasurement::toggleReverseMode();
		bool bIsReverse = AbstractMeasurement::isInReverseMode();
		if (bIsReverse)
		{
			// !!!NOTE: be very careful here... fourth parameter value
			// must be accurate in order to get the proper image for the button.
			// (The fourth parameter is the index for the button's image within the toolbar image list.)
			m_toolBar.SetButtonInfo(nIndex, nResourceID, nStyle, (int)kNumOfBtnsOnToolbar + (int)kReverseBmp);

		}
		else
		{
			// set the image back to the original
			m_toolBar.SetButtonInfo(nIndex, nResourceID, nStyle, (int)kDrawingDirectionBtn);
		}
		
		//update the menu
		checkMainMenuItem(ID_TOOLS_DRAWINGDIRECTION_NORMAL, !bIsReverse);
		checkMainMenuItem(ID_TOOLS_DRAWINGDIRECTION_REVERSE, bIsReverse);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI01881")
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnToolsDrawingdirectionReverse() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		if (!AbstractMeasurement::isInReverseMode())
		{
			OnBTNReverseMode();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI01882")
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnToolsDrawingdirectionNormal() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		if (AbstractMeasurement::isInReverseMode())
		{
			OnBTNReverseMode();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI01883")
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnBTNSelectFeatures() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		// first verify successful initialization
		verifyInitialization();

		SelectFeaturesDlg selectFeaturesDlg;
		if (selectFeaturesDlg.DoModal() == IDOK)
		{
			string strSourceDocName = selectFeaturesDlg.GetSourceDocName();
			if (ma_pDrawingTool.get() != NULL)
			{
				ma_pDrawingTool->selectFeatures(strSourceDocName);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI01884")
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnBTNViewEditAttributes() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		// first verify successful initialization
		verifyInitialization();

		if (!isToolbarButtonPressed(IDC_BTN_ViewEditAttributes))
		{
			m_bViewEditFeatureSelected = true;
		}
		// disable the input as no input is to be accepted during
		// the feature selection mode
		disableInput();

		// Two possibilities:
		// 1) the drawing tool is pressed down
		// 2) none of the buttons are pressed down, icomap is not the current active tool
		// first check if icomap is the current active tool
		if (!m_bIsIcoMapCurrentTool)
		{
			if (m_ipDisplayAdapter)
			{
				m_ipDisplayAdapter->SelectTool(_bstr_t(ICOMAP_TOOL_NAME.c_str()));
			}
		}
		
		if (!isToolbarButtonPressed(IDC_BTN_ViewEditAttributes))
		{
			updateDrawingToolsAndEditAttributeStates();

			// do some operations here
			onEditAttributeSelected();
			
			// see onFeatureSelected()
			// Get IUCLDFeature from DisplayAdapter as well as from AttributeManager (if any),
			//	  and pass it to the attribute dlg and then bring up the attribute dlg
			// When attribute dlg is OnOK, get the two IUCLDFeaturePtr from the attribute dlg,
			//	  store original one to the database through AttributeManager, and update 
			//    the currently selected feature in the ArcMap with the current one
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI01885")
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnBTNToggleCurveDirection() 
{		
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		CWaitCursor wait;

		m_bToggleLeft = !m_bToggleLeft;	

		setToggleDirectionButtonState();

		// process the toggle state
		ma_pDrawingTool->processExpectedInput(NULL);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI01886")
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnBTNToggleDeltaAngle() 
{		
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		CWaitCursor wait;

		// if it's toggling curve, then m_bDeltaAngleGT180 means
		// the delta angle for the curve is toggled to be greater than 180°;
		m_bDeltaAngleGT180 = !m_bDeltaAngleGT180;
	
		setToggleDeltaAngleButtonState();

		// process the toggle state
		ma_pDrawingTool->processExpectedInput(NULL);

	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI01887")
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnBtnDig() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		// show/hide grid
		bool bShow = IcoMapOptions::sGetInstance().isDIGVisible();
		bShow = !bShow;
		if (!bShow)
		{
			m_commandLine.SetFocus();
		}
		IcoMapOptions::sGetInstance().showDIG(bShow);
		// update the window controls
		updateWindowPosition();

		// update the state of the DIG button and menu item
		// press down the button
		pressToolbarButton(IDC_BTN_DIG, bShow ? TRUE : FALSE);

		// check/uncheck the menu item
		CMenu *pMenu = GetMenu();
		UINT nCheck = bShow ? MF_CHECKED : MF_UNCHECKED;
		pMenu->CheckMenuItem(ID_VIEW_DIG, nCheck);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11978")
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnViewDig() 
{
	OnBtnDig();
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnViewStatus() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		// show/hide status bar
		bool bShow = IcoMapOptions::sGetInstance().isStatusBarVisible();
		bShow = !bShow;
		IcoMapOptions::sGetInstance().showStatusBar(bShow);
		// update the window controls
		updateWindowPosition();

		// check/uncheck the menu item
		CMenu *pMenu = GetMenu();
		UINT nCheck = bShow ? MF_CHECKED : MF_UNCHECKED;
		pMenu->CheckMenuItem(ID_VIEW_STATUS, nCheck);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11980")
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnCurveoperationsCurvedirectionConcaveleft() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		if (!m_bToggleLeft)
		{
			OnBTNToggleCurveDirection();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI01890")
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnCurveoperationsCurvedirectionConcaveright() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		if (m_bToggleLeft)
		{
			OnBTNToggleCurveDirection();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI01891")
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnCurveoperationsDeltaangleGreaterthan180() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		if (!m_bDeltaAngleGT180)
		{
			OnBTNToggleDeltaAngle();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI01892")
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnCurveoperationsDeltaangleLessthan180() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		if (m_bDeltaAngleGT180)
		{
			OnBTNToggleDeltaAngle();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI01893")
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnCancel() 
{
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnClose() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		m_ipDisplayAdapter->SelectDefaultTool();

		// Retrieve and store size and position
		CRect rect;
		GetWindowRect(&rect);
		ma_pCfgIcoMapDlg->setWindowPos(rect.left, rect.top);
		ma_pCfgIcoMapDlg->setWindowSize(rect.Width(), rect.Height());

		// Just hide the window
		ShowWindow(SW_HIDE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI01895")
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnFinalRelease()
{
	// When the last reference for an automation object is released
	// OnFinalRelease is called.  The base class will automatically
	// deletes the object.  Add additional cleanup required for your
	// object before calling the base class.

	CDialog::OnFinalRelease();
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnDestroy() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		// before the input manager control is destructed, destroy all the input
		// receivers first
		IInputManagerPtr ipInputManager = getInputManager();
		ipInputManager->Destroy();
		
		// get window normal placement (size and position)
		WINDOWPLACEMENT windowPlacement;
		GetWindowPlacement(&windowPlacement);
		
		CRect rect;
		rect = windowPlacement.rcNormalPosition;
		
		ma_pCfgIcoMapDlg->setWindowPos(rect.left, rect.top);
		ma_pCfgIcoMapDlg->setWindowSize(rect.Width(), rect.Height());
		
		// release any concurrent license that may have been obtained.
		// This is called here because it is known to be executed everytime the IcoMapDlg window closes,
		// so any concurrent license will be released
		IcoMapOptions::sGetInstance().releaseConcurrentIcoMapLicense();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02839")

	CDialog::OnDestroy();
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnOK() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		CWaitCursor wait;

		// only proceed if it is not in view/edit mode
		if (!m_bViewEditFeatureSelected)
		{
			string strInput = m_commandLine.GetInputText();
			
			processInput(strInput);
		}

		m_commandLine.SetInputText("");
	}
	catch (UCLIDException &ue)
	{
		ue.display();
		m_commandLine.SetInputText("");
	}
	CATCH_COM_EXCEPTION("ELI02842")
	CATCH_UNEXPECTED_EXCEPTION("ELI01392");
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnActivate(UINT nState, CWnd* pWndOther, BOOL bMinimized)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		CDialog::OnActivate(nState, pWndOther, bMinimized);

		if (m_ipIcoMapInputTarget)
		{
			// capture activate and deactivate messages
			switch (nState)
			{
			case WA_ACTIVE:
			case WA_CLICKACTIVE:
				{
					if (::IsWindow(m_hWnd) && IsWindowVisible())
					{
						if (!m_bIcoMapIsActiveInputTarget)
						{
							m_ipInputTargetManager->NotifyInputTargetWindowActivated(m_ipIcoMapInputTarget);
							activateInputTarget();
						}
						else
						{
							// update the highlight window to be on top
							showHighlightWindow();
						}

						// if icomap is not current tool and command line is set focus
						// set icomap as current tool
						if (m_bIcoMapIsActiveInputTarget
							&& m_bFeatureCreationEnabled
							&& !m_bIsIcoMapCurrentTool 
							&& m_ipDisplayAdapter != NULL)
						{
							m_ipDisplayAdapter->SelectTool(_bstr_t(ICOMAP_TOOL_NAME.c_str()));
						}
					}
				}
				break;
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04045")
}
//--------------------------------------------------------------------------------------------------
/*void IcoMapDlg::OnNcLButtonDown(UINT nHitTest, CPoint point)
{
	try
	{
		if (m_bIcoMapIsActiveInputTarget
			&& m_bPointInputEnabled
			&& !m_bIsIcoMapCurrentTool 
			&& m_ipDisplayAdapter != NULL)
		{
			m_ipDisplayAdapter->SelectTool(_bstr_t(ICOMAP_TOOL_NAME.c_str()));
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07605")

	CDialog::OnNcLButtonDown(nHitTest, point);
}*/
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnMove(int x, int y)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		m_ipHighlightWindow->Refresh();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04046")

	CDialog::OnMove(x, y);
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnPaint()
{
		bool bDIGVisible = IcoMapOptions::sGetInstance().isDIGVisible();
		// draw a line below toolbar if the static info box is visible
		if (bDIGVisible && m_staticInfo.IsWindowVisible())
		{
			CPaintDC dc( this ); // device context for painting

			// Get the toolbar height and the dialog width
			CRect rectDlg;
			GetWindowRect( &rectDlg );

			CRect rectToolBar;
			m_toolBar.GetWindowRect( &rectToolBar );

			int iToolBarHeight = rectToolBar.Height();
			int iDialogWidth = rectDlg.Width();

			// With gray and white pens, draw horizontal lines that span the entire width
			// of the dialog, and that are just below the toolbar buttons
			CPen penGray;
			CPen penWhite;
			penGray.CreatePen(  PS_SOLID, 0, RGB( 128, 128, 128 ) );
			penWhite.CreatePen( PS_SOLID, 0, RGB( 255, 255, 255 ) );

			// First the gray line
			dc.SelectObject( &penGray );
			dc.MoveTo( 0, iToolBarHeight + 2 );
			dc.LineTo( iDialogWidth, iToolBarHeight + 2 );

			// Next the white line, one pixel below the gray
			dc.SelectObject( &penWhite );
			dc.MoveTo( 0, iToolBarHeight + 3 );
			dc.LineTo( iDialogWidth, iToolBarHeight + 3 );
		}
		CPaintDC dc(this); // device context for painting
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnToolbarDropDown(NMHDR* pNMHDR, LRESULT *plr)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		// If AfxGetApp()->m_pMainWnd returns a NULL inside BCMenu.cpp and CWnd LoadMenu will 
		// throw an exception so we will temporarily set the CWinApp mainWnd ptr to this so 
		// that AfxGetApp()->m_pMainWnd never returns NULL (P16: 2261)
		// Please read http://www.experts-exchange.com/Programming/Programming_Languages/MFC/Q_20102369.html
		// for detailed description
		AfxAppMainWindowRestorer tempRestorer;
		AfxGetApp()->m_pMainWnd = this;

		CWnd *pWnd;
		
		NMTOOLBAR *pTB = (NMTOOLBAR *)pNMHDR;
		UINT nID = pTB->iItem;
		
		// Switch on button command id's.
		switch (nID)
		{
		case IDC_BTN_DrawingTools:
			pWnd = &m_toolBar;
			nID  = IDR_MNU_DRAWING_TOOLS;
			break;
		default:
			return;
		}
		
		updateCurveToolsMenuText();

		CMenu* pPopup = m_menuDrawingTools.GetSubMenu(0);
		ASSERT(pPopup);
		
		CRect rc;
		pWnd->SendMessage(TB_GETRECT, pTB->iItem, (LPARAM)&rc);
		pWnd->ClientToScreen(&rc);
		
		pPopup->TrackPopupMenu( TPM_LEFTALIGN | TPM_LEFTBUTTON | TPM_VERTICAL,
			rc.left, rc.bottom, this, &rc);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI01897")
}
//--------------------------------------------------------------------------------------------------
BOOL IcoMapDlg::OnToolTipNotify( UINT id, NMHDR * pNMHDR, LRESULT * pResult )
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	BOOL retCode = FALSE;
		
	try
	{
		TOOLTIPTEXT* pTTT = (TOOLTIPTEXT*)pNMHDR;
		UINT nID = pNMHDR->idFrom;
		if (pNMHDR->code == TTN_NEEDTEXT && (pTTT->uFlags & TTF_IDISHWND))
		{
			// idFrom is actually the HWND of the tool, ex. button control, edit control, etc.
			nID = ::GetDlgCtrlID((HWND)nID);
		}
		
		if(nID)
		{
			retCode = TRUE;
			pTTT->hinst = AfxGetResourceHandle();
			switch(nID)
			{
			case IDC_BTN_ReverseMode:
				if (!AbstractMeasurement::isInReverseMode())
				{
					nID = IDC_BTN_NormalMode;
				}
				break;
			case IDC_BTN_ToggleCurveDirection:
				{
					if (!m_bToggleDirectionEnabled)
					{
						nID = IDC_BTN_ToggleCurveDirection;
					}
					else
					{
						if (m_bToggleLeft)
						{
							nID = IDC_BTN_ToggleLeft;
						}
						else
						{
							nID = IDC_BTN_ToggleRight;
						}
					}
				}
				break;
			case IDC_BTN_ToggleDeltaAngle:
				{
					if (! m_bToggleDeltaAngleEnabled)
					{
						nID = IDC_BTN_ToggleDeltaAngle;
					}
					else
					{
						if (m_bDeltaAngleGT180)
						{
							nID = IDC_BTN_DeltaAngle_GT180;
						}
						else
						{
							nID = IDC_BTN_DeltaAngle_LT180;
						}
					}
				}
				break;
			case IDC_BTN_DrawingTools:
				{
					if (m_ECurrentSelectedToolID >= kBtnCurve1 && m_ECurrentSelectedToolID <= kBtnCurve8)
					{
						// get the current tool tip
						CString zTipText = CurrentCurveTool::sGetInstance().getToolTip().c_str();
						lstrcpyn(pTTT->szText, zTipText, sizeof(pTTT->szText));
						return retCode;
					}
					else if (m_ECurrentSelectedToolID == kBtnLine)
					{
						nID = ID_DRAWINGTOOL_LINE;
					}
					else if (m_ECurrentSelectedToolID == kBtnLineAngle)
					{
						nID = ID_DRAWINGTOOL_LINE_ANGLE;
					}
					else if (m_ECurrentSelectedToolID == kBtnCurveGenie)
					{
						nID = ID_DRAWINGTOOL_GENIE;
					}
				}
				break;
			default:
				break;
				
			}
			pTTT->lpszText = MAKEINTRESOURCE(nID);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07604")

	return retCode;
}
//--------------------------------------------------------------------------------------------------
BOOL IcoMapDlg::PreTranslateMessage(MSG* pMsg) 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	BOOL bRetCode(FALSE);
	
	try
	{
		// key press in the dialog
		if (m_bFinishSketchEnabled && pMsg->message == WM_KEYDOWN) 
		{
			int wParamValue = static_cast<int>(pMsg->wParam);

			// F2 for finishing sketch
			if (wParamValue == VK_F2)
			{
				// if focus is on grid, let grid process F2 first
				if (pMsg->hwnd == ma_pDIGWnd->m_hWnd 
					|| ::IsChild(ma_pDIGWnd->m_hWnd, pMsg->hwnd) == TRUE)
				{
					bRetCode = ma_pDIGWnd->OnGridKeyDown(VK_F2, 1, 60);
				}

				// if Grid hasn't processed F2, then process key down here
				if (!bRetCode)
				{
					processKeyDown(wParamValue, 0);
				}

				// no more dispatch
				return TRUE;
			}

			// If "Enter" key is pressed
			if (wParamValue == VK_RETURN)
			{
				// if focus is on grid, set focus to command line [P10: 3092]
				if (pMsg->hwnd == ma_pDIGWnd->m_hWnd 
					|| ::IsChild(ma_pDIGWnd->m_hWnd, pMsg->hwnd) == TRUE)
				{
					m_commandLine.SetFocus();
				}
				else
				{
					CDialog::PreTranslateMessage(pMsg);
				}

				// no more dispatch
				return TRUE;
			}

			// If "Shift + TAB" key is pressed and the focus is on the command line
			// set the focus to previous selected cell on Grid and highlight that row
			// for editing. If only "Tab" key is pressed and the focus is on the 
			// command line, do nothing [P10: 3095]
			if (wParamValue == VK_TAB)
			{
				// Get the row and col number of last row and last column
				unsigned long nCellRow, nCellCol;
				nCellRow = ma_pDIGWnd->GetRowCount();
				nCellCol = ma_pDIGWnd->GetColCount();

				// Get the data of the last two editable cell on the last row
				CString zData1 = ma_pDIGWnd->GetValueRowCol(nCellRow, nCellCol);
				CString zData2 = ma_pDIGWnd->GetValueRowCol(nCellRow, nCellCol - 2);

				// if focus is on command line and the "Shift + Tab" key is pressed
				if ( (pMsg->hwnd == m_commandLine.m_hWnd 
					|| ::IsChild(m_commandLine.m_hWnd, pMsg->hwnd) == TRUE) 
					&& ::GetKeyState(VK_SHIFT) < 0)
				{
					// Create a dummy CPoint obj
					CPoint point;

					if (zData2.IsEmpty())
					{
						// If the one before last editable one is empty, 
						// prepare to set the focus to the previous data cell
						nCellCol = nCellCol - 4;
					}
					else if (zData1.IsEmpty())
					{
						// If the last cell is empty and the editable 
						// one before last is not empty, prepare to set the focus 
						// to the editable one before last
						nCellCol = nCellCol - 2;
					}

					// Set the current cell on focus
					ma_pDIGWnd->SetCurrentCell(nCellRow, nCellCol);
					ma_pDIGWnd->OnLButtonClickedRowCol(nCellRow, nCellCol, MK_LBUTTON, point);

					// no more dispatch
					return TRUE;
				}
				// if only "Tab" key is clicked and the focus is on command line, do nothing
				else if ( (pMsg->hwnd == m_commandLine.m_hWnd 
					|| ::IsChild(m_commandLine.m_hWnd, pMsg->hwnd) == TRUE) 
					&& ::GetKeyState(VK_SHIFT) >= 0)
				{
					return TRUE;
				}
				// if only "Tab" key is clicked and the focus is on grid,
				// Check if the focus is on the last data cell, if it is,
				// move the focus to the prompt command line
				else if ( (pMsg->hwnd == ma_pDIGWnd->m_hWnd 
					|| ::IsChild(ma_pDIGWnd->m_hWnd, pMsg->hwnd) == TRUE) 
					&& ::GetKeyState(VK_SHIFT) >= 0)
				{
					// Get the current cell
					unsigned long nCurCellRow, nCurCellCol;
					ma_pDIGWnd->GetCurrentCell(&nCurCellRow, &nCurCellCol);

					// If current row is the last row
					if (nCurCellRow == nCellRow)
					{
						// If the data cell before the last data cell is empty, which means the
						// focus is on the first data cell on the last row, press tab will move the focus to prompt line
						if (zData2.IsEmpty() 
							// If the last data cell is empty and the current focus is
							// on the data cell before the last data cell, press tab will move the focus to the prompt line
							|| (zData1.IsEmpty() && nCurCellCol == nCellCol - 2)
							// If the last data cell is not empty and the current focus is
							// on that cell, press tab will move the focus to the prompt line
							|| (!zData1.IsEmpty() && nCurCellCol == nCellCol)
							)
						{
							// Set the focus to prompt line and return
							m_commandLine.SetFocus();
							return TRUE;
						}// End of inner if
					}// End of mid if
				}// End of else if
			}// End of out if
		}

		if (m_bInitialized)
		{
			if (ma_pToolTipCtrl.get())
			{
				ma_pToolTipCtrl->RelayEvent(pMsg);
			}

			//prompt edit box wants to pre-translate backspace and home key-pressed event
			bRetCode = m_commandLine.PreTranslateMessage(pMsg);
			
		}
		
		if (!bRetCode)
		{
			bRetCode = ma_pDIGWnd->preTranslateMsg(pMsg);
		}

		// only let the dialog proccess the message if and only if
		// the message belongs to the icomap dlg or its child window!!!
		if (pMsg->hwnd == this->m_hWnd || pMsg->hwnd == m_commandLine.m_hWnd)
		{
			//if bRetCode is TRUE, do not pass the message along anymore
			if (!bRetCode)
			{
				bRetCode =  CDialog::PreTranslateMessage(pMsg);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07603")
	
	return bRetCode;   
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::resetToggleButtonPictures()
{
	int nImage;
	UINT nResourceID, nStyle;
	//index of the drawing tools btn on the toolbar
	int nIndex;
	CMenu *pMenu = GetMenu();

	// set curve direction back to left, and delta angle back to less than 180
	if (!m_bToggleLeft)
	{
		nIndex = m_toolBar.CommandToIndex(IDC_BTN_ToggleCurveDirection);
		m_toolBar.GetButtonInfo(nIndex, nResourceID, nStyle, nImage);
		// set the image back to the original
		m_toolBar.SetButtonInfo(nIndex, nResourceID, nStyle, (int)kCurveConcavityBtn);

		//update the menu
		pMenu->CheckMenuItem(ID_CURVEOPERATIONS_CURVEDIRECTION_CONCAVELEFT, MF_CHECKED);
		pMenu->CheckMenuItem(ID_CURVEOPERATIONS_CURVEDIRECTION_CONCAVERIGHT, MF_UNCHECKED);
		
		m_bToggleLeft = true;
	}

	if (m_bDeltaAngleGT180)
	{
		nIndex = m_toolBar.CommandToIndex(IDC_BTN_ToggleDeltaAngle);
		m_toolBar.GetButtonInfo(nIndex, nResourceID, nStyle, nImage);
		// set the image back to the original
		m_toolBar.SetButtonInfo(nIndex, nResourceID, nStyle, (int)kCurveDeltaAngleBtn);

		//update the menu
		pMenu->CheckMenuItem(ID_CURVEOPERATIONS_DELTAANGLE_LESSTHAN180, MF_CHECKED);
		pMenu->CheckMenuItem(ID_CURVEOPERATIONS_DELTAANGLE_GREATERTHAN180, MF_UNCHECKED);
		
		m_bDeltaAngleGT180 = true;
	}

	// reset current toggle inputs
	ma_pDrawingTool->setCurrentToggleInputs(m_bToggleLeft, m_bDeltaAngleGT180);
}
//--------------------------------------------------------------------------------------------------
BOOL IcoMapDlg::ShowWindow(int nCmdShow)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		bool bExceptionCaught = true;
		try
		{
			// if ocr filter scheme has been set yet, set to simple maps
			static bool bExists = false;
			if (!bExists)
			{
				IInputManagerPtr ipInputManager = getInputManager();
				IOCRFilterMgrPtr ipOCRFilterMgr(ipInputManager->GetOCRFilterMgr());
				if (ipOCRFilterMgr)
				{
					string strRoot(gstrREG_ROOT_KEY + "\\InputFunnel");
					string strFolder("\\ApplicationSpecificSettings");
					strFolder = strFolder + "\\" + ::getFileNameWithoutExtension(getCurrentProcessEXEFullPath());
					string strKeyName = "LastUsedScheme";
					// lazy init
					RegistryPersistenceMgr pCfgMgr = RegistryPersistenceMgr(HKEY_CURRENT_USER, strRoot);
					if (!pCfgMgr.keyExists(strFolder, strKeyName))
					{
						ipOCRFilterMgr->SetCurrentScheme(_bstr_t("Simple Maps"));
						// Message to the user about the current ocr filtering scheme
						CString zMsg = "For your convenience, the default OCR Filter Scheme has been set to \"Simple Maps\".\n\nThis setting optimizes the OCR engine for recognizing particular formats\nfor angles (e.g. 32°45'43\"), bearings (e.g. N32°45'43\"E), and\ndistances (e.g. 133.24').\n\nIcoMap supports recognizing data in many other formats.  Please remember\nto modify or create new OCR Filter schemes if you need to recognize data\nin other formats.  For more information, please refer to \"OCR Filter\nSchemes\" in the help file.";
						MessageBox(zMsg, "IcoMap", MB_OK|MB_ICONINFORMATION);
					}
				}				
				bExists = true;
			}
			
			// if succeeds, continue showing/hiding the dialog
			// if exception thrown, hide the dialog
			bExceptionCaught = false;
		}
		CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03761");
		
		// if an exception was caught, then select the default control and
		// do not show the dialog
		if (bExceptionCaught)
		{
			if (m_ipDisplayAdapter)
			{
				m_ipDisplayAdapter->SelectDefaultTool();
			}
			
			return CDialog::ShowWindow(SW_HIDE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04133")

	// update the display of this window
	return CDialog::ShowWindow(nCmdShow);
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnShowWindow(BOOL bShow, UINT nStatus) 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	CDialog::OnShowWindow(bShow, nStatus);
	
	try
	{
		BOOL bVisible = IsWindowVisible();
		if (bShow && !bVisible)
		{
			if (!m_bIcoMapIsActiveInputTarget)
			{
				m_ipInputTargetManager->NotifyInputTargetWindowActivated(m_ipIcoMapInputTarget);
				activateInputTarget();
			}
		}
		else if (!bShow && bVisible)
		{
			if (m_bIcoMapIsActiveInputTarget)
			{
				// deactivate this input target, and notify the input target
				// manager that this input target has been closed
				deactivateInputTarget();
				m_ipInputTargetManager->NotifyInputTargetWindowClosed(m_ipIcoMapInputTarget);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04134")
}
//--------------------------------------------------------------------------------------------------
//--------------------------------------------------------------------------------------------------
//********************************************************************
// IcoMap Menu items
void IcoMapDlg::OnFileClose() 
{
	OnClose();
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnHelpIcomaphelp() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		// Build executable string
		string	strExe( "hh.exe " );
		string	strHelpPath = IcoMapOptions::sGetInstance().getHelpDirectory();

		// Make sure path was found in INI file
		if (strHelpPath.length() > 0)
		{
			string	strHelpFile( "\\IM4ArcGIS.chm" );

			string	strCombined = strExe + strHelpPath + strHelpFile;

			// Prepare information for CreateProcess()
			STARTUPINFO			si;
			PROCESS_INFORMATION	pi;
			memset( &si, 0, sizeof(STARTUPINFO) );
			si.cb = sizeof(STARTUPINFO);
			si.wShowWindow = SW_SHOW;

			// Start Help application
			if (!CreateProcess( NULL, (char *)strCombined.data(), 
				NULL, NULL, TRUE, DETACHED_PROCESS, NULL, NULL, 
				&si, &pi ))
			{
				UCLIDException ue("ELI02148", "Unable to run Help!");
				throw ue;
			}
		}
		else
		{
			// Display error message
			::MessageBox( NULL, 
				"Could not find path to IcoMap help file.",
				"IcoMap", MB_ICONEXCLAMATION | MB_OK );
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI01901")
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnHelpAbouticomap() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		// Bring the About IcoMap dlg
		AboutIcoMapDlg aboutIcoMapDlg;
		aboutIcoMapDlg.DoModal();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI01902")
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnBTNFinishSketch() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		CWaitCursor wait;

		// first verify successful initialization
		verifyInitialization();

		switchFocusFromDIGToCommandLine();

		ma_pDrawingTool->finishSketch();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI01903")
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnBTNFinishPart() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		// first verify successful initialization
		verifyInitialization();

		switchFocusFromDIGToCommandLine();

		ma_pDrawingTool->finishPart();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI01904")
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnBTNDeleteSketch() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		// first verify successful initialization
		verifyInitialization();

		// Provide confirmation message to user
		int iReturn = MessageBox("Delete current sketch?", "Confirm Delete", 
			MB_YESNO | MB_ICONQUESTION | MB_DEFBUTTON2 );

		// Only delete the sketch if user agrees
		if (iReturn == IDYES)
		{
			// Delete the sketch
			ma_pDrawingTool->deleteSketch();

			// Set the focus back to prompt dialog box
			m_commandLine.SetFocus();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI01905")
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnToolsActionsFinishpart() 
{	
	OnBTNFinishPart();
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnToolsActionsFinishsketch() 
{
	OnBTNFinishSketch();
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnToolsActionsDeletesketch() 
{
	OnBTNDeleteSketch();
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnSelectToolsDrawToolsMenu(UINT nID)
{//this is from the menu bar of the IcoMapDlg
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		// if and only if the selected tool is within line to curve8 range
		if (nID >= ID_TOOLS_DRAWINGTOOLS_LINE && nID <= ID_TOOLS_DRAWINGTOOLS_GENIE)
		{
			EDrawingToolsBtnID eTempBtnID = static_cast<EDrawingToolsBtnID>(nID - ID_TOOLS_DRAWINGTOOLS_LINE);
			OnSelectDrawToolsPopupMenu((int)eTempBtnID + ID_DRAWINGTOOL_LINE);
		}
		else
		{
			UCLIDException uclidException("ELI02220", "Invalid control id.");
			uclidException.addDebugInfo("nID", nID);
			throw uclidException;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI01909")
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnSelectDrawToolsPopupMenu(UINT nID)
{//this is from the popup menu from the toolbar-drawingtools btn

	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		// if and only if the selected tool is within line to curve8 range
		if (nID >= ID_DRAWINGTOOL_LINE && nID <= ID_DRAWINGTOOL_GENIE)
		{
			m_bViewEditFeatureSelected = false;

			//menu from the menu bar
			CMenu *pToolsDrawingToolMenu = GetMenu();
			
			// remove check (from previously checked item) for both menu items from menu bar and tool bar
			if (m_ECurrentSelectedToolID != kNoToolSelected)
			{
				m_EPreviousSelectedToolID = m_ECurrentSelectedToolID;
				UINT nUncheckID = ID_TOOLS_DRAWINGTOOLS_LINE + (int)m_EPreviousSelectedToolID;
				pToolsDrawingToolMenu->CheckMenuItem(nUncheckID, MF_UNCHECKED);
				nUncheckID = ID_DRAWINGTOOL_LINE + (int)m_EPreviousSelectedToolID;
				m_menuDrawingTools.CheckMenuItem(nUncheckID, MF_UNCHECKED);
			}

			//update current select tool id (0-based) 
			UINT nLineToolID = ID_DRAWINGTOOL_LINE;
			UINT nCurrentTool = nID - nLineToolID;
			m_ECurrentSelectedToolID = static_cast<EDrawingToolsBtnID>(nCurrentTool);
			Invalidate();
						
			//set check for both menu items from menu bar and tool bar
			pToolsDrawingToolMenu->CheckMenuItem(
				ID_TOOLS_DRAWINGTOOLS_LINE + (int)m_ECurrentSelectedToolID, MF_CHECKED);
			m_menuDrawingTools.CheckMenuItem(nID, MF_CHECKED);
			
			// replace the bitmap of the drawing tool btn on toolbar with the currently
			// seleced tool's bitmap
			// First, update the image list
			int nImage;
			UINT nResourceID, nStyle;
			//index of the drawing tools btn on the toolbar
			int nIndex = m_toolBar.CommandToIndex(IDC_BTN_DrawingTools);
			m_toolBar.GetButtonInfo(nIndex, nResourceID, nStyle, nImage);
			// !!!NOTE: be very careful here... fourth parameter value
			// must be accurate in order to get the proper image for the button.
			// (The fourth parameter is the index for the button's image within the toolbar image list.)
			EAlternativeBitmaps eAltBmp = static_cast<EAlternativeBitmaps>(m_ECurrentSelectedToolID);
			m_toolBar.SetButtonInfo(nIndex, nResourceID, nStyle, (int)kNumOfBtnsOnToolbar + (int)eAltBmp);

			// make sure drawing tool btn and view/edit feature btn are in correct state
			updateDrawingToolsAndEditAttributeStates();

			//invoke the drawing tools button pressed call
			onDrawingToolSelected();

			if (!m_bIsIcoMapCurrentTool)
			{
				m_ipDisplayAdapter->SelectTool(_bstr_t(ICOMAP_TOOL_NAME.c_str()));
			}
		}
		else
		{
			UCLIDException uclidException("ELI02221", "Invalid control id.");
			uclidException.addDebugInfo("nID", nID);
			throw uclidException;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI01910")
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnToolsOptions() 
{
	OnBTNOptions();
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnToolsEditfeatures() 
{
	OnBTNViewEditAttributes();
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnToolsSelectfeatures() 
{
	OnBTNSelectFeatures();
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnToolsOcrschemes() 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		IInputManagerPtr ipInputManager = getInputManager();
		IOCRFilterMgrPtr ipOCRFilterMgr(ipInputManager->GetOCRFilterMgr());
		if (ipOCRFilterMgr == NULL)
		{
			throw UCLIDException("ELI03626", "Failed to get OCRFilterMgr.");
		}

		ipOCRFilterMgr->ShowFilterSchemesDlg();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03561")
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::OnMenuSelect(UINT nItemID, UINT nFlags, HMENU hSysMenu) 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);
	
	try
	{
		CString cstr("");
		
		switch (nItemID)
		{
		case ID_TOOLS_DRAWINGTOOLS_CURVE1:
		case ID_DRAWINGTOOL_CURVE1:
			cstr = CurveToolManager::sGetInstance().getCurveTool(kCurve1)->generateToolTip().c_str();
			break;
		case ID_TOOLS_DRAWINGTOOLS_CURVE2:
		case ID_DRAWINGTOOL_CURVE2:
			cstr = CurveToolManager::sGetInstance().getCurveTool(kCurve2)->generateToolTip().c_str();
			break;
		case ID_TOOLS_DRAWINGTOOLS_CURVE3:
		case ID_DRAWINGTOOL_CURVE3:
			cstr = CurveToolManager::sGetInstance().getCurveTool(kCurve3)->generateToolTip().c_str();
			break;
		case ID_TOOLS_DRAWINGTOOLS_CURVE4:
		case ID_DRAWINGTOOL_CURVE4:
			cstr = CurveToolManager::sGetInstance().getCurveTool(kCurve4)->generateToolTip().c_str();
			break;
		case ID_TOOLS_DRAWINGTOOLS_CURVE5:
		case ID_DRAWINGTOOL_CURVE5:
			cstr = CurveToolManager::sGetInstance().getCurveTool(kCurve5)->generateToolTip().c_str();
			break;
		case ID_TOOLS_DRAWINGTOOLS_CURVE6:
		case ID_DRAWINGTOOL_CURVE6:
			cstr = CurveToolManager::sGetInstance().getCurveTool(kCurve6)->generateToolTip().c_str();
			break;
		case ID_TOOLS_DRAWINGTOOLS_CURVE7:
		case ID_DRAWINGTOOL_CURVE7:
			cstr = CurveToolManager::sGetInstance().getCurveTool(kCurve7)->generateToolTip().c_str();
			break;
		case ID_TOOLS_DRAWINGTOOLS_CURVE8:
		case ID_DRAWINGTOOL_CURVE8:
			cstr = CurveToolManager::sGetInstance().getCurveTool(kCurve8)->generateToolTip().c_str();
			break;
		default:
			cstr.LoadString(nItemID);			
			break;
			
		}
		
		//	m_statusBarCtrl.SetText(cstr, 0, SBT_NOBORDERS);
		setStatusText( cstr.operator LPCTSTR() );
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07602")
}
//--------------------------------------------------------------------------------------------------
LRESULT IcoMapDlg::OnGetWindowIRManager(WPARAM wParam, LPARAM lParam) 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	LRESULT lResult = 0;

	try
	{
		IInputManagerPtr ipInputManager = getInputManager();
		lResult = ipInputManager->GetHWND();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI04949")

	return lResult;
}
//--------------------------------------------------------------------------------------------------
LRESULT IcoMapDlg::OnExecuteCommand(WPARAM wParam, LPARAM lParam) 
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	LRESULT lResult = -1;

	try
	{
		if (executeShortcutCommand((EShortcutType)wParam))
		{
			lResult = 0;
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06344")

	return lResult;
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::CalcWindowRect(LPRECT lpClientRect, UINT nAdjustType) 
{
	// TODO: Add your specialized code here and/or call the base class
	
	CDialog::CalcWindowRect(lpClientRect, nAdjustType);
}
//--------------------------------------------------------------------------------------------------
