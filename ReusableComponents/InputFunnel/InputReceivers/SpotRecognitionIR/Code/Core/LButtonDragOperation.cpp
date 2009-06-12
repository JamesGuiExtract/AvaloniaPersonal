//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	LButtonDragOperation.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan
//
//==================================================================================================

#include "stdafx.h"
#include "LButtonDragOperation.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

LButtonDragOperation::LButtonDragOperation(CUCLIDGenericDisplay& rUCLIDGenericDisplayCtrl,
										   ERubberbandingMode eRubberbandingMode)
:DragOperation(rUCLIDGenericDisplayCtrl), m_eRubberbandingMode(eRubberbandingMode)
{
	reset();
}

void LButtonDragOperation::onMouseDown(short Button, short Shift, long x, long y)
{
	if (Button == 1 && m_eDragState == kWaitingForLButtonDown) // left mouse button
	{
		// we have received the first drag point, which is what we were waiting for
		// convert the current mouse position to world
		// coordinates
		m_UCLIDGenericDisplayCtrl.convertClientWindowPixelToWorldCoords(x, y, 
			&m_p1.m_dX, &m_p1.m_dY, m_UCLIDGenericDisplayCtrl.getCurrentPageNumber());
		
		// enable rubberbanding if applicable
		if (m_eRubberbandingMode == kLine)
		{
			m_UCLIDGenericDisplayCtrl.setRubberbandingParameters(1, m_p1.m_dX, m_p1.m_dY);
			m_UCLIDGenericDisplayCtrl.enableRubberbanding(TRUE);
		}
		else if (m_eRubberbandingMode == kRectangle)
		{
			m_UCLIDGenericDisplayCtrl.setRubberbandingParameters(2, m_p1.m_dX, m_p1.m_dY);
			m_UCLIDGenericDisplayCtrl.enableRubberbanding(TRUE);
		}
		else
		{
			if (m_UCLIDGenericDisplayCtrl.isRubberbandingEnabled() == TRUE)
				m_UCLIDGenericDisplayCtrl.enableRubberbanding(FALSE);
		}

		// advance the internal state to waiting for second drag point
		m_eDragState = kWaitingForLButtonUp;
	}
}

void LButtonDragOperation::onMouseUp(short Button, short Shift, long x, long y)
{
	if (Button == 1 && m_eDragState == kWaitingForLButtonUp) // left mouse button
	{
		// we have received the second drag point, which is what we were waiting for
		// convert the current mouse position to world
		// coordinates
		m_UCLIDGenericDisplayCtrl.convertClientWindowPixelToWorldCoords(x, y, 
			&m_p2.m_dX, &m_p2.m_dY, m_UCLIDGenericDisplayCtrl.getCurrentPageNumber());
		
		// disable rubberbanding, and then process the drag operation
		m_UCLIDGenericDisplayCtrl.enableRubberbanding(FALSE);

		// advance the internal drag state to its default state by calling reset()
		reset();

		// process the drag operation by passing the drag start/end points
		// to the derived class that implements the processDragOperation() virtual method.
		processDragOperation(m_p1, m_p2);
	}
}

bool LButtonDragOperation::autoRepeat()
{
	 return false;
}

void LButtonDragOperation::reset()
{
	// by default we are not in a drag operation
	m_eDragState = kNotApplicable;
}

void LButtonDragOperation::init()
{
	m_eDragState = kWaitingForLButtonDown;
}

bool LButtonDragOperation::isInProcess()
{
	if (m_eDragState == kWaitingForLButtonUp)
	{
		return true;
	}

	return false;
}