
#include "stdafx.h"
#include "IcoMapDlg.h"
#include "IcoMapApp.h"
#include "DrawingToolFSM.h"

#include <UCLIDException.h>

using namespace std;


void IcoMapDlg::enableDeflectionAngleTool(bool bEnable)
{
	static bool bInit = false;
	if (!bInit)
	{
		m_bDeflectionAngleToolEnabled = !bEnable;
		bInit = true;
	}

	if (m_bDeflectionAngleToolEnabled != bEnable)
	{
		m_bDeflectionAngleToolEnabled = bEnable;

		// if this tool is to be disabled, and it's the current tool
		// swap it with the regular line drawing tool
		if (!bEnable && m_ECurrentSelectedToolID == kBtnLineAngle)
		{
			OnSelectDrawToolsPopupMenu(ID_DRAWINGTOOL_LINE);
		}

		UINT nEnable;
		if (bEnable)
		{
			nEnable = (MF_BYCOMMAND | MF_ENABLED);
		}
		else
		{
			nEnable = (MF_BYCOMMAND | MF_GRAYED);
		}

		// enable/disable the internal/deflection angle tool
		m_menuDrawingTools.EnableMenuItem(ID_DRAWINGTOOL_LINE_ANGLE, nEnable);
		enableMainMenuItem(ID_TOOLS_DRAWINGTOOLS_LINEANGLE, bEnable);
	}
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::enableToggle(bool bEnableToggleDirection, bool bEnableToggleDeltaAngle)
{
	// if appropriate, update UIs to reflect the enable/disable toggling
	if (m_bToggleDirectionEnabled == bEnableToggleDirection 
		&&  m_bToggleDeltaAngleEnabled == bEnableToggleDeltaAngle)
	{
		// no change
		return;
	}

	m_bToggleDirectionEnabled = bEnableToggleDirection;
	m_bToggleDeltaAngleEnabled = bEnableToggleDeltaAngle;
		
	updateStateOfCurveToggleButtons();
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::setInput(const std::string& strInput)
{
	// notify icomap about input received from sources other 
	// than SRIR, HTIR or IcoMap command line
	fireOnInputReceived(strInput);
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::setToggleDirectionState(bool bLeft)
{
	m_bToggleLeft = bLeft;
	updateStateOfCurveToggleButtons();

	if (ma_pDrawingTool.get() != NULL)
	{
		ma_pDrawingTool->setCurrentToggleInputs(m_bToggleLeft, m_bDeltaAngleGT180);
	}
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::setToggleDeltaAngleState(bool bGreaterThan180)
{
	m_bDeltaAngleGT180 = bGreaterThan180;
	updateStateOfCurveToggleButtons();

	if (ma_pDrawingTool.get() != NULL)
	{
		ma_pDrawingTool->setCurrentToggleInputs(m_bToggleLeft, m_bDeltaAngleGT180);
	}
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::activateInputTarget()
{
	m_bIcoMapIsActiveInputTarget = true;

	showHighlightWindow();

	// if the input target has been activated and we are
	// currently in view/edit features mode (or feature creation
	// is not enabled), then disable input
	// if we are in drawing mode, then enable input as appropriate.
	if (m_bViewEditFeatureSelected || !m_bFeatureCreationEnabled)
	{
		disableInput();
	}
	else
	{
		// set correct input validator for input receivers
		ma_pDrawingTool->enableInput();
	}
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::deactivateInputTarget()
{
	showHighlightWindow(false);

	disableInput();

	m_bIcoMapIsActiveInputTarget= false;

	// set default tool
	if (m_bIsIcoMapCurrentTool && m_ipDisplayAdapter  != NULL)
	{
		m_ipDisplayAdapter->SelectDefaultTool();
	}
}
//--------------------------------------------------------------------------------------------------
bool IcoMapDlg::isInputTargetWindowVisible()
{
	if (::IsWindow(m_hWnd))
	{
		return ::IsWindowVisible(m_hWnd)?true:false;
	}

	return false;
}
//--------------------------------------------------------------------------------------------------
void IcoMapDlg::setInputTarget(IInputTarget* pInputTarget)
{
	m_ipIcoMapInputTarget = pInputTarget;
	
	// IcoMapCtl implements IIcoMapApplication and IInputTarget
	IInputTargetPtr ipInputTarget(m_ipIcoMapInputTarget);
	ICOMAPAPPLib::IIcoMapApplicationPtr ipIcoMapApplication(ipInputTarget);
	if (ipIcoMapApplication != NULL && m_ipIcoMapInputContext != NULL)
	{
		ICOMAPAPPLib::IIcoMapInputContextPtr ipIcoMapInputContext(m_ipIcoMapInputContext);
		ipIcoMapInputContext->SetIcoMapApplication(ipIcoMapApplication);
	}
}
//--------------------------------------------------------------------------------------------------
