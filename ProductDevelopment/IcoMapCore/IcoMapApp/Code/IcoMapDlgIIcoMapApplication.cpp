//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	IcoMapDlgIIcoMapApplication.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Duan Wang (June 2001 - present)
//
//==================================================================================================

#include "stdafx.h"
#include "IcoMapDlg.h"
#include "IcoMapApp.h"

#include "DrawingToolFSM.h"

#include <IcoMapOptions.h>
#include <TemporaryResourceOverride.h>
#include <UCLIDException.h>
#include <AbstractMeasurement.hpp>
#include <AttributeViewDlg.h>
#include <CfgAttributeViewer.h>
#include <ValueRestorer.h>
#include <io.h>
#include <cpputil.h>
#include <DistanceCore.h>

extern HINSTANCE gModuleResource;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif


using namespace std;

//--------------------------------------------------------------------------------------------------
void IcoMapDlg::enableFeatureCreation(bool bEnable)
{
	if (ma_pDrawingTool.get() != NULL)
	{
		m_bFeatureCreationEnabled = bEnable;
		enableDrawingTools(bEnable);
		enableSketchOperationTools(bEnable);
		enableDrawingDirectionsTool(bEnable);

		BOOL bEnableCommandLine = m_bFeatureCreationEnabled 
								&& !m_bViewEditFeatureSelected ? TRUE : FALSE;
		// before disable DIG, make sure it doesn't have the focus
		if (!bEnableCommandLine)
		{
			switchFocusFromDIGToCommandLine();
		}

		m_commandLine.EnableWindow(bEnableCommandLine);
		ma_pDIGWnd->EnableWindow(bEnableCommandLine);
				
		if (m_bFeatureCreationEnabled && !m_bViewEditFeatureSelected)
		{
			// delegate the call to the drawing tool FSM since DrawingToolFSM has
			// the knowledge of what's the current state and what's the current 
			// input validator to be applied.
			ma_pDrawingTool->enableInput();
		}
		else if (!bEnable)
		{
			// disable input and modify highlight window color accordingly
			disableInput();
		}
	}
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::initializeInputProcessing(void)
{
	if (ma_pDrawingTool.get() == NULL)
	{
		// Set up the input processing objects.
		IInputManagerPtr ipInputManager = getInputManager();
		ma_pDrawingTool = auto_ptr<DrawingToolFSM>(new DrawingToolFSM(ipInputManager, 
			static_cast<CommandPrompt*>(this), ma_pDIGWnd.get()));
		ASSERT_RESOURCE_ALLOCATION("ELI02210", ma_pDrawingTool.get() != NULL);
	}

	ma_pDrawingTool->setIcoMapUI(this);

	// set current default toggle inputs
	resetToggleButtonPictures();

	// set default distance unit
	DistanceCore::setDefaultDistanceUnit(IcoMapOptions::sGetInstance().getDefaultDistanceUnitType());

	// set curve toggle buttons' state (i.e. enabled/disabled)
	updateStateOfCurveToggleButtons();
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::notifySketchModified(long nActualNumOfSegments)
{
	if (ma_pDrawingTool.get() != NULL)
	{
		ma_pDrawingTool->notifySketchModified(nActualNumOfSegments);

		// update the status bar with current error segment info
		string strErrorSegmentReport = ma_pDIGWnd->getErrorSegmentReport();
		m_statusBar.SetWindowText(strErrorSegmentReport.c_str());
	}

	m_bFinishSketchEnabled = true;
	m_bDeleteSketchEnabled = true;
	// if actual number of segment is less than 2, check for
	// if there's a point or not
	if (nActualNumOfSegments < 2)
	{
		if (m_ipDisplayAdapter)
		{
			// see if we can get the tangent out of last segment
			double dDumb = 0.0;
			bool bSuccess = 
				m_ipDisplayAdapter->GetLastSegmentTanOutAsPolarAngleInRadians(&dDumb) == VARIANT_TRUE;
			// there's no more segment in the drawing
			if (!bSuccess)
			{
				m_bFinishSketchEnabled = false;
				// see if there's a start point in the drawing
				bSuccess = m_ipDisplayAdapter->GetLastPoint(&dDumb, &dDumb) == VARIANT_TRUE;
				if (!bSuccess)
				{
					m_bDeleteSketchEnabled = false;
				}
			}
		}
	}

	// only if grid is visible
	if (ma_pDIGWnd.get() != NULL && IcoMapOptions::sGetInstance().isDIGVisible())
	{
		// The track is on if DIG is currently tracking, or
		// there's no segment nor starting point in the drawing
		bool bTrackingOn = ma_pDIGWnd->needTracking() 
				|| (nActualNumOfSegments == 0 && !(m_bDeleteSketchEnabled && !m_bFinishSketchEnabled));
		// whether or not last time tracking is on
		static bool s_bLastTrackingOn = !bTrackingOn;
		// only make change to the grid/message box when last
		// time value is different than the current value.
		// This way, we don't have to constantly call Show/hide
		// everytime a segment is added to/removed from the sketch.
		if (s_bLastTrackingOn != bTrackingOn)
		{
			// get toolbar rect
			CRect rectNeedRedraw;
			m_toolBar.GetClientRect(&rectNeedRedraw);

			// rect of the line need to redraw
			rectNeedRedraw.top = rectNeedRedraw.bottom - 5;
			rectNeedRedraw.bottom = rectNeedRedraw.bottom + 5;

			if (bTrackingOn)
			{
				m_staticInfo.ShowWindow(SW_HIDE);
				ma_pDIGWnd->ShowWindow(SW_SHOW);
				RedrawWindow(rectNeedRedraw);
			}
			else
			{
				m_staticInfo.ShowWindow(SW_SHOW);	
				ma_pDIGWnd->ShowWindow(SW_HIDE);
				RedrawWindow(rectNeedRedraw);
			}

			// update whether the last tracking is on
			s_bLastTrackingOn = bTrackingOn;
		}
	}
	
	updateSketchOperationTools();
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::onFeatureSelected(bool bFeatureReadOnly)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	TemporaryResourceOverride resourceOverride(gModuleResource);

	try
	{
		// since this might take for a while to get info of a feature
		// put an hourglass here
		CWaitCursor wait;

		// Get the selected feature from display adapter
		if (m_ipDisplayAdapter)
		{
			const static string strIcoMapFieldName("IcoMapAttr");

			// set current distance unit type
			DistanceCore::setCurrentDistanceUnit(ma_pDrawingTool->getCurrentDistanceUnitType());

			// no unique feature id is available now
			_bstr_t bstrFeatureID("");
			// get current feature
			IUCLDFeaturePtr ipUCLIDCurrentFeature(m_ipDisplayAdapter->GetFeatureGeometry(bstrFeatureID));
		
			// get original feature if any
			IUCLDFeaturePtr ipUCLIDOriginalFeature = 
				m_ipAttributeManager->GetFeatureAttribute(bstrFeatureID, _bstr_t(strIcoMapFieldName.c_str()));

			if (ipUCLIDCurrentFeature)
			{	
				// Create configuration object, if needed
				if (!ma_pCfgAttributeViewer.get())
				{
					// lazy initialization
					ma_pCfgAttributeViewer = auto_ptr<CfgAttributeViewer>(
						new CfgAttributeViewer(
						IcoMapOptions::sGetInstancePtr()->getUserPersistenceMgr(),
						"\\IcoMap for ArcGIS\\Dialogs\\AttributeViewer"));
					ASSERT_RESOURCE_ALLOCATION("ELI02213", ma_pCfgAttributeViewer.get() != NULL);
				}

				// determine from the target platform whether the currently selected feature
				// can store attributes
				VARIANT_BOOL bCanStoreOriginalAttributes 
					= m_ipAttributeManager->CanStoreAttributes(bstrFeatureID, _bstr_t(strIcoMapFieldName.c_str()));

				// if feature(s) is(are) read only
				bool bReadOnly = bFeatureReadOnly;
				
				// unless the user has used a "back door" provided by UCLID to allow editing of
				// current attributes in coverages, disable editing of current attributes
				// if original attributes cannot be stored.
				// NOTE: this "back door" is a means to allow editing of current attributes in 
				// line coverages for those users for whom this feature is important.
				if (!IcoMapOptions::sGetInstance().alwaysAllowEditingOfCurrentAttributes())
				{
					bReadOnly = bReadOnly || (bCanStoreOriginalAttributes == VARIANT_FALSE);
				}

				// Pop up the attribute dialog
				CAttributeViewDlg attributeViewerDlg(ma_pCfgAttributeViewer.get(), 
					ipUCLIDCurrentFeature, ipUCLIDOriginalFeature, 
					bReadOnly, bReadOnly, bCanStoreOriginalAttributes == VARIANT_TRUE, NULL);
				
				// Before open attribute dlg, set drawing mode to forward since this variable
				// is a static boolean, it's applied to all abstractmeasurement instances
				// store it temporarily so that we can set it back after attribute dialog is dismissed
				{	
					// store the original mode, then set it back once the attribute dialog is dismissed
					ReverseModeValueRestorer rmvr;
					
					// always work in normal mode with attribute dialog
					AbstractMeasurement::workInReverseMode(false);

					// disable the IcoMap window when feature attributes dialog is on
					EnableWindow(FALSE);
					int res = -1;
					try
					{
						res = attributeViewerDlg.DoModal();
					}
					catch(...)
					{
						// enable IcoMap window if there is an exception when open feature attributes dialog
						EnableWindow(TRUE);
						throw;
					}
					// enable IcoMap window
					EnableWindow(TRUE);

					// only update the feature if it's not read only
					if(!bFeatureReadOnly && res == IDOK)
					{
						// if current feature is valid, get the feature back and update
						// the feature in ArcMap
						if (attributeViewerDlg.isCurrentFeatureValid())
						{
							if (!attributeViewerDlg.isCurrentFeatureEmpty())
							{
								ipUCLIDCurrentFeature = attributeViewerDlg.getCurrentFeature();
								m_ipDisplayAdapter->SetFeatureGeometry(bstrFeatureID, ipUCLIDCurrentFeature);
								
								// if original feature is valid, store it in database
								if (attributeViewerDlg.isOriginalFeatureValid())
								{
									if (!attributeViewerDlg.isOriginalFeatureEmpty())
									{
										ipUCLIDOriginalFeature = attributeViewerDlg.getOriginalFeature();
										m_ipAttributeManager->SetFeatureAttribute(bstrFeatureID, 
											_bstr_t(strIcoMapFieldName.c_str()), ipUCLIDOriginalFeature);
									}
									else // remove all contents in the attribute field
									{
										m_ipAttributeManager->SetFeatureAttribute(bstrFeatureID, 
											_bstr_t(strIcoMapFieldName.c_str()), NULL);
									}
								}
							}
							else // if the current feature is empty, delete the feature
							{
								m_ipDisplayAdapter->SetFeatureGeometry(bstrFeatureID, NULL);
							}
						}
					}
				}
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI01914")
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::processKeyDown(long lKeyCode, long lShiftKey)
{
	// only process the key if command line is enabled
	if (!m_commandLine.IsWindowEnabled())
	{
		return;
	}

	// if shift, ctrl key is pressed, no action will be taken
	if (lShiftKey == 0)
	{
		// Now the focus is on ArcMap, IcoMap drawing tool is the current active tool		
		// Adjust lKeyCode to appropriate ASCII value as needed 
		switch (lKeyCode)
		{
		case VK_F2:
			{
				if (m_bFinishSketchEnabled)
				{
					// finish sketch
					ma_pDrawingTool->finishSketch();
				}

				return;
			}
			break;
		case 8:		// Backspace
		case 9:		// Tab
		case 12:	// Clear (KEY 5 from Num pad when num lock is off)
		case 19:	// Pause break
		case 20:	// Caps lock
		case 27:	// Escape
		case 32:	// Space
		case 33:	// Page up
		case 34:	// Page down
		case 35:	// End
		case 36:	// Home
		case 37:	// Left Arrow
		case 38:	// Up Arrow
		case 39:	// Right Arrow
		case 40:	// Down Arrow
		case 45:	// Insert
		case 92:	// Windows key
		case 93:	// Context menu key if available from keyboard
		case 114:	// F3
		case 115:	// F4
		case 116:	// F5
		case 117:	// F6
		case 118:	// F7
		case 119:	// F8
		case 120:	// F9
		case 121:	// F10
		case 122:	// F11
		case 123:	// F12
		case 144:	// Num lock
		case 145:	// Scroll lock
			{
				return;
			}
			break;

		case 13:	// return
			{
				m_commandLine.SetFocus();
				return;
			}
			break;
			
			// Special characters from the key pad
		case 106:		// character *
		case 107:		// character +
		case 109:		// character -
		case 110:		// character .
		case 111:		// character /
			lKeyCode -= 64;
			break;
			
		case 188:		// character ,
		case 189:		// character -
		case 190:		// character .
		case 191:		// character /
			lKeyCode -= 144;
			break;
			
		case 219:		// character [
		case 220:		// character '\'
		case 221:		// character ]
			lKeyCode -= 128;
			break;
			
		case 192:		// character `
			lKeyCode -= 96;
			break;
			
		case 187:		// character =
			lKeyCode -= 126;
			break;
			
		case 186:		// character ;
			lKeyCode -= 127;
			break;
			
		case 222:		// character '
			lKeyCode -= 183;
			break;
			
			// Standard unshifted ASCII alphanumerics
		default:
			// Post the input ASCII key value onto the command line
			if ((lKeyCode >= 65 && lKeyCode <= 90)	||			// A - Z
				(lKeyCode >= 97 && lKeyCode <= 122) ||			// a - z
				(lKeyCode >= 48 && lKeyCode <= 57) ||			// 0 - 9
				(lKeyCode >= VK_NUMPAD0 && lKeyCode <= VK_NUMPAD9 ) )	// 0 - 9 from Numeric Keypad
			{
				// Special handling if NUM_LOCK key is down
				if (VK_NUMLOCK && (lKeyCode >= VK_NUMPAD0 && lKeyCode <= VK_NUMPAD9))
				{
					lKeyCode -= 48;
				}
			}
			break;
		}
		
		// Set the focus to the command line
		m_commandLine.SetFocus();
		
		// Apply the (shifted) text to the command line
		char cKeyCode = (char) lKeyCode;
		CString zKey( "" );
		zKey.Format( "%c", cKeyCode );
		m_commandLine.SetInputText( zKey.operator LPCTSTR() );
	}
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::processKeyUp(long lKeyCode, long lShiftKey)
{
	// up-arrow key will be captured here!!!
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::reset()
{
	if (ma_pDrawingTool.get() != NULL)
	{
		ma_pDrawingTool->reset();
	}

	// make sure the command prompt and toolbar buttons are in correct state
	if (m_bViewEditFeatureSelected)
	{
		onEditAttributeSelected();
	}
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::setAttributeManager(IAttributeManager* pAttributeManager)
{
	m_ipAttributeManager = pAttributeManager;
	
	if (ma_pDrawingTool.get() != NULL)
	{
		ma_pDrawingTool->setAttributeManager(m_ipAttributeManager);
	}
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::setCommandInput(const std::string& strInput)
{
	m_commandLine.SetInputText(strInput);
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::setDisplayAdapter(IDisplayAdapter* pDisplayAdapter)
{
	m_ipDisplayAdapter = pDisplayAdapter;

	if (ma_pDrawingTool.get() != NULL)
	{
		ma_pDrawingTool->setDisplayAdapter(m_ipDisplayAdapter);
	}
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::setCommandPrompt(const std::string& strText, bool bShowDefaultValue)
{
	if (m_commandLine.m_hWnd)
	{
		string strPromptText(strText);
		
		if (bShowDefaultValue)
		{
			// if there is a default value for the current input
			string strDefaultValue(ma_pDrawingTool->getCurrentDefaultCurveParameterValue());
			// Always echo default input
			if (!strDefaultValue.empty())
			{
				strPromptText.append(" <" + strDefaultValue + ">");
			}
		}
		
		strPromptText.append(" : ");
		
		m_commandLine.SetInputText("");
		
		m_commandLine.SetPromptText(strPromptText);
	}
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::setIcoMapAsCurrentTool(bool bIsCurrent)
{
	// only set the flag if they are different
	m_bIsIcoMapCurrentTool = bIsCurrent;

	// make sure buttons are in correct state (i.e. pressed down or not)
	updateDrawingToolsAndEditAttributeStates();
	
	if (m_bViewEditFeatureSelected)
	{
		// do some operations here
		onEditAttributeSelected();
	}

/*	bool bEnable = m_bIsIcoMapCurrentTool && !m_bViewEditFeatureSelected;
	BOOL bEnableTools = bEnable ? TRUE : FALSE;

	if (m_bFeatureCreationEnabled && !m_bViewEditFeatureSelected)
	{
		// delegate the call to the drawing tool FSM since DrawingToolFSM has
		// the knowledge of what's the current state and what's the current 
		// input validator to be applied.
		ma_pDrawingTool->enableInput();
	}
	else
	{
		// disable input and modify highlight window color accordingly
		disableInput();
	}

	if (bIsCurrent)
	{
		SetFocus();
	}*/
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::setPoint(const string& strPointInput)
{
	if (m_bPointInputEnabled)
	{
		// notify observers
		fireOnInputReceived(strPointInput);
	}
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::setText(std::string& strText)
{
	if (m_bTextInputEnabled)
	{
		fireOnInputReceived(strText);
	}
}
//--------------------------------------------------------------------------------------------------
