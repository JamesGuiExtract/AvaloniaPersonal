//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	PanWindowDragOperation.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan
//
//==================================================================================================

#include "stdafx.h"
#include "PanWindowDragOperation.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

PanWindowDragOperation::PanWindowDragOperation(CUCLIDGenericDisplay& rUCLIDGenericDisplayCtrl)
:LButtonDragOperation(rUCLIDGenericDisplayCtrl, LButtonDragOperation::kNone)
{
}

void PanWindowDragOperation::onMouseMove(short Button, short Shift, long x, long y)
{
	// if mouse coordinates are negative, make them 0
	if (x < 0)
		x = 0;
	if (y < 0)
		y = 0;

	if (Button == 1 && m_eDragState == kWaitingForLButtonUp) // left mouse button
	{
		// we have received the first drag point, which is what we were waiting for
		// convert the current mouse position to world
		// coordinates
		CartographicPoint tempPoint;
		m_UCLIDGenericDisplayCtrl.convertClientWindowPixelToWorldCoords(x, y, 
			&tempPoint.m_dX, &tempPoint.m_dY, m_UCLIDGenericDisplayCtrl.getCurrentPageNumber());

		// process the drag operation so that we can see the panning in realtime
		processDragOperation(m_p1, tempPoint);
	}
		
}

void PanWindowDragOperation::processDragOperation(const CartographicPoint& p1,
												   const CartographicPoint& p2)
{
	// if the drag start point and the drag end point are the
	// same, then do nothing
	if (p1 == p2)
		return;

	// calculate the displacement of the current view
	CartographicPoint displacement;
	displacement.m_dX = p2.m_dX - p1.m_dX;
	displacement.m_dY = p2.m_dY - p1.m_dY;

	// get the current view extents
	double dBottomLeftX, dBottomLeftY, dTopRightX, dTopRightY;
	m_UCLIDGenericDisplayCtrl.getCurrentViewExtents(&dBottomLeftX, &dBottomLeftY, 
		&dTopRightX, &dTopRightY);

	// calculate the current view's center
	double dOldCenterX = (dBottomLeftX + dTopRightX) / 2.0;
	double dOldCenterY = (dBottomLeftY + dTopRightY) / 2.0;

	// calculate the new view's center
	double dNewCenterX = dOldCenterX - displacement.m_dX;
	double dNewCenterY = dOldCenterY - displacement.m_dY;
	
	// zoom center around the newly calculated center point
	m_UCLIDGenericDisplayCtrl.zoomCenter(dNewCenterX, dNewCenterY);
}
