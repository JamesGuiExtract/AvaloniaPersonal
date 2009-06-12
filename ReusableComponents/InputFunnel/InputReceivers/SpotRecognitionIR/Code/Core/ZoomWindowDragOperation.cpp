//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ZoomWindowDragOperation.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan
//
//==================================================================================================

#include "stdafx.h"
#include "ZoomWindowDragOperation.h"
#include "UCLIDException.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//--------------------------------------------------------------------------------------------------
ZoomWindowDragOperation::ZoomWindowDragOperation(CUCLIDGenericDisplay& rUCLIDGenericDisplayCtrl)
:LButtonDragOperation(rUCLIDGenericDisplayCtrl, LButtonDragOperation::kRectangle)
{
}
//--------------------------------------------------------------------------------------------------
ZoomWindowDragOperation::~ZoomWindowDragOperation()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16503");
}
//--------------------------------------------------------------------------------------------------
bool ZoomWindowDragOperation::autoRepeat()
{
	return true;
}
//--------------------------------------------------------------------------------------------------
void ZoomWindowDragOperation::processDragOperation(const CartographicPoint& p1,
												   const CartographicPoint& p2)
{
	// depending upon how the user dragged the window, p1 and p2 could be any of the
	// following: bottom left, top left, bottom right, or top right.
	// however, the zoomWindow method requires the first set of arguments to be
	// bottom left, and the second set of arguments to be top-right.
	// so, determine the bottom-left and the top-right coordinates
	CartographicPoint bottomLeft, topRight;
	bottomLeft.m_dX = min(p1.m_dX, p2.m_dX);
	bottomLeft.m_dY = min(p1.m_dY, p2.m_dY);
	topRight.m_dX = max(p1.m_dX, p2.m_dX);
	topRight.m_dY = max(p1.m_dY, p2.m_dY);

	// lStatus is used to get the fit-to status after zooming window
	// If the return lStatus is 1, then the image after zooming is in fit to page status
	// If the return lSatatus is 0, then the image after zooming is in fit to width status
	long lStatus = -1;
	// invoke the zoom-window operation
	m_UCLIDGenericDisplayCtrl.zoomWindow(bottomLeft.m_dX, bottomLeft.m_dY, 
		topRight.m_dX, topRight.m_dY, &lStatus);
}
//--------------------------------------------------------------------------------------------------
