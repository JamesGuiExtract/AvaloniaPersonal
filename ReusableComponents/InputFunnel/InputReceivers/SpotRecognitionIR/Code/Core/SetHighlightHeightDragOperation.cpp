//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	SetHighlightHeightDragOperation.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan
//
//==================================================================================================

#include "stdafx.h"
#include "SetHighlightHeightDragOperation.h"
#include <math.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

SetHighlightHeightDragOperation::SetHighlightHeightDragOperation(CUCLIDGenericDisplay& rUCLIDGenericDisplayCtrl)
:LButtonDragOperation(rUCLIDGenericDisplayCtrl, kLine)
{
}

void SetHighlightHeightDragOperation::processDragOperation(const CartographicPoint& p1,
														   const CartographicPoint& p2)
{
	// convert the two cartographic points that are in world coordinates into
	// image pixel coordinates.
	POINT imagePoint1, imagePoint2;
	long nPageNum = m_UCLIDGenericDisplayCtrl.getCurrentPageNumber();
	m_UCLIDGenericDisplayCtrl.convertWorldToImagePixelCoords(p1.m_dX, p1.m_dY, 
		&imagePoint1.x, &imagePoint1.y, nPageNum);
	m_UCLIDGenericDisplayCtrl.convertWorldToImagePixelCoords(p2.m_dX, p2.m_dY, 
		&imagePoint2.x, &imagePoint2.y, nPageNum);

	// calculate the pixel distance between the two imagepixel points
	long ulHighlightHeight = (long) sqrt((double) ((imagePoint2.x-imagePoint1.x) * (imagePoint2.x-imagePoint1.x) +
		(imagePoint2.y-imagePoint1.y) * (imagePoint2.y-imagePoint1.y)));

	// the zone highlight height should be an odd number - so increment if necessary
	if (ulHighlightHeight % 2 == 0)
	{
		ulHighlightHeight++;
	}

	// set the calculated highlight height in the UCLIDGenericDisplay control
	m_UCLIDGenericDisplayCtrl.setZoneHighlightHeight(ulHighlightHeight);
}
