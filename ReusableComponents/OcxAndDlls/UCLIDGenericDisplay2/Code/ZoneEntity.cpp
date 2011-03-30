//-------------------------------------------------------------------------------------------------
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ZoneEntity.cpp
//
// PURPOSE:	This is an header file for ZoneEntity class
//			where this has been derived from the GenericEntity()
//			class.  The code written in this file makes it possible for
//			initialize Zone entity and to see it in the view.
// NOTES:	
//
// AUTHORS:	M.Srnivasa Rao
//
//-------------------------------------------------------------------------------------------------
#include "stdafx.h"
#include "GenericEntity.h"
#include "ZoneEntity.h"
#include "Rectangle.h"
#include "GenericDisplayFrame.h"
#include "GenericDisplayView.h"
#include "GenericDisplayCtl.h"
#include "UCLIDException.h"

#include <cmath>
#include <string>

//-------------------------------------------------------------------------------------------------
ZoneEntity::ZoneEntity(unsigned long id, long startPtX, long startPtY, long endPtX, 
					   long endPtY, long zoneHeight, bool bFill,
					   bool bBorderVisible, COLORREF borderColor)
: GenericEntity(id), m_lStartPosX(startPtX), m_lStartPosY(startPtY), m_lEndPosX(endPtX), m_lEndPosY(endPtY),
  m_ulTotalZoneHeight(zoneHeight), m_bBorderVisible(bBorderVisible), m_borderColor(borderColor), 
  m_bFill(bFill), m_bZonePtsCalculated(false), m_bZoneSelectionPtsCalculated(false), 
  m_nBorderStyle(PS_SOLID), m_bBorderIsSpecial(false), m_dBaseAngle(0)
{
	try
	{
		// Determine the orientaion of the points.
		int orientation = getOrientation(startPtX, endPtX, startPtY, endPtY, false);

		// Calculate the angle of specified points.
		m_dBaseAngle = atan2((double)(m_lEndPosY - m_lStartPosY), (double)(m_lEndPosX  - m_lStartPosX));

		// Use the orientation to normalize the base zone angle so that it represents the midpoint
		// vector closest to left to right.
		m_dBaseAngle -= (MathVars::PI / 2.0 * orientation);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25312");	
}
//-------------------------------------------------------------------------------------------------
ZoneEntity::~ZoneEntity()
{
	try
	{
		// add cleanup code here
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16459");
}
//-------------------------------------------------------------------------------------------------
void ZoneEntity::getExtents(GDRectangle& rBoundingRectangle)
{
	// pop up rBoundingRectangle with the bounds 
	getZoneBoundingRect(rBoundingRectangle);
}
//-------------------------------------------------------------------------------------------------
string ZoneEntity::getDesc()
{
	string strZone("ZONE");
	return strZone;
}
//-------------------------------------------------------------------------------------------------
void ZoneEntity::offsetBy(double dX, double dY)
{
	// the offset given here is in image pixel values
	// offset the start point
	// TESTTHIS cast to unsigned long
	m_lStartPosX += (long)dX;
	m_lStartPosY += (long)dY;
	
	// offset the end point
	m_lEndPosX += (long)dX;
	m_lEndPosY += (long)dY;

	// update other variables related to the change in zone extents
	updateWorldUnitVariables();

	// as the start or end point is changed, we need to recalculate the zone bounds and grip points
	m_bZonePtsCalculated = false;
	m_bZoneSelectionPtsCalculated = false;
}
//-------------------------------------------------------------------------------------------------
void ZoneEntity::updateWorldUnitVariables()
{
	m_pGenericDisplayCtrl->convertImagePixelToWorldCoords(m_lStartPosX, m_lStartPosY, 
		&dStartPosX, &dStartPosY, m_ulPageNumber);

	m_pGenericDisplayCtrl->convertImagePixelToWorldCoords(m_lEndPosX, m_lEndPosY, 
		&dEndPosX, &dEndPosY, m_ulPageNumber);
}
//-------------------------------------------------------------------------------------------------
Point ZoneEntity::getStartPointInWorldUnits() 
{
	// have to decide whether we should update the world coordinates before returning the point
	// the parameters that affects world coordinates of zone entity is Scale factor of the control
	// and the offsetBy() call of zone entity class. So try to get a notification whenever scale factor is changed.
	if (m_pGenericDisplayCtrl->isScaleFactorChanged())
	{
		m_pGenericDisplayCtrl->notifyScaleFactorChanged(FALSE);
	}
	
	Point pt(dStartPosX, dStartPosY);
	return pt;
}
//-------------------------------------------------------------------------------------------------
Point ZoneEntity::getEndPointInWorldUnits() 
{
	// have to decide whether we should update the world coordinates before returning the point
	// the parameters that affects world coordinates of zone entity is Scale factor of the control
	// and the offsetBy() call of zone entity class. So try to get a notification whenever scale factor is changed.
	if (m_pGenericDisplayCtrl->isScaleFactorChanged())
	{
		m_pGenericDisplayCtrl->notifyScaleFactorChanged(FALSE);
	}

	Point pt(dEndPosX, dEndPosY);
	return pt;
}
//-------------------------------------------------------------------------------------------------
Point ZoneEntity::getStartPointInImageUnits() 
{
	Point pt(m_lStartPosX, m_lStartPosY);
	return pt;
}
//-------------------------------------------------------------------------------------------------
Point ZoneEntity::getEndPointInImageUnits() 
{
	Point pt(m_lEndPosX, m_lEndPosY);
	return pt;
}
//-------------------------------------------------------------------------------------------------
void ZoneEntity::setEndPointInImageUnits(long lX, long lY)
{
	m_lEndPosX = lX;
	m_lEndPosY = lY;

	// Update other variables related to the change in zone extents
	updateWorldUnitVariables();

	// Recalculate the zone bounds and grip points
	m_bZonePtsCalculated = false;
	m_bZoneSelectionPtsCalculated = false;
}
//-------------------------------------------------------------------------------------------------
void ZoneEntity::ComputeEntExtents()
{
	// it is the most optimized way of calculating the zone bounds as a region.Calculate the region in the 
	// image coordinates. Whenever zone entity is to be drawn in the view, convert the zone bounds vertices 
	// into view coordinates and then draw in the view.So calculateZoneVertices() method is called only once.
	// and only if zone start or end points changed through offsetBy() method
	calculateZoneVertices();

	// no need to implement this method as the extents need to be computed on fly
	// and is being implemented in getExtents() method.
	// But to update the world 
	// coordinate variables the following call is made
	updateWorldUnitVariables();
}
//-------------------------------------------------------------------------------------------------
void ZoneEntity::EntDraw(BOOL bDraw)
{
	if (m_bVisible == FALSE)
		return;

	CGenericDisplayView *pView = m_pGenericDisplayCtrl->getUGDView(); 

	CDC* pDC = NULL;

	try
	{
		// calculate the view coordinates associated with each of the zones
		CalculateViewCoords();

		if (bDraw == FALSE)
		{
			// Calculate the edges of the zone bounds
			long minX = m_zoneBoundViewPts[0].x;
			long minY = m_zoneBoundViewPts[0].y;
			long maxX = minX;
			long maxY = minY;

			for (int i=1; i<4; i++)
			{
				long x = m_zoneBoundViewPts[i].x;
				if (x < minX)
				{
					minX = x;
				}
				else if (x > maxX)
				{
					maxX = x;
				}

				long y = m_zoneBoundViewPts[i].y;
				if (y < minY)
				{
					minY = y;
				}
				else if (y > maxY)
				{
					maxY = y;
				}
			}

			// Expand the bounds as necessary
			long lDelta = 0;
			if (m_bSelected)
			{
				lDelta = max(giZONE_BORDER_WIDTH * 4, lDelta);
			}
			if (m_bBorderVisible)
			{
				lDelta = max(giZONE_BORDER_WIDTH, lDelta);
			}

			RECT rect = {minX-lDelta, minY-lDelta, maxX+lDelta, maxY+lDelta};

			// Invalidate the region
			pView->InvalidateRect(&rect);
			return;
		}

		// Get a device context
		pDC = pView->m_ImgEdit.GetDC();

		// draw the filled part of the zone if applicable
		if (m_bFill)
		{
			// set the drawing mode
			int iOldDrawMode = pDC->SetROP2(R2_MASKNOTPEN);

			//	Create a brush object
			CBrush highltBrush;
			highltBrush.CreateSolidBrush(m_color);
			CBrush *pOldBrush = (CBrush *) pDC->SelectObject(&highltBrush);

			// create region to highlight
			CRgn zoneRegion;
			zoneRegion.CreatePolygonRgn(m_zoneBoundViewPts, 4, ALTERNATE);

			// fill the zone region
			pDC->FillRgn(&zoneRegion, &highltBrush);

			// restore the old brush and drawing mode
			pDC->SelectObject(pOldBrush);
			pDC->SetROP2(iOldDrawMode);

			//delete the current brush and region
			highltBrush.DeleteObject();
			zoneRegion.DeleteObject();
		}

		if (m_bBorderVisible)
		{
			// set the drawing mode
			int iOldDrawMode = pDC->SetROP2(R2_MASKPEN);

			//	Create a pen object		
			LOGBRUSH penLB;
			penLB.lbColor = m_borderColor;
			penLB.lbStyle = BS_SOLID;
			
			// it is more efficient to draw using a non-geometric pen
			// and non-geometric pens only work with PS_SOLID style.
			// So, if the style is not PS_SOLID, then only add the PS_GEOMETRIC
			// flag to the style.
			CPen borderPen;
			if (m_nBorderStyle == PS_SOLID)
			{
				borderPen.CreatePen(PS_SOLID, giZONE_BORDER_WIDTH, m_borderColor);
			}
			else
			{
				borderPen.CreatePen(m_nBorderStyle | PS_GEOMETRIC, 
					giZONE_BORDER_WIDTH, &penLB, 0, 0);
			}

			// draw the polyline
			CPen *pOldPen = (CPen *) pDC->SelectObject(&borderPen);
			pDC->Polyline(m_zoneBorderViewPts, 5);

			// if the zone is supposed to have the special border, make the 
			// border special with the extra linework.
			if (m_bBorderIsSpecial)
			{
				const int iLen = 15;
				POINT t = m_zoneBorderViewPts[0];
				pDC->MoveTo(t);
				t.x -= iLen;
				t.y += iLen;
				pDC->LineTo(t);

				t = m_zoneBorderViewPts[1];
				pDC->MoveTo(t);
				t.x += iLen;
				t.y += iLen;
				pDC->LineTo(t);

				t = m_zoneBorderViewPts[2];
				pDC->MoveTo(t);
				t.x += iLen;
				t.y -= iLen;
				pDC->LineTo(t);

				t = m_zoneBorderViewPts[3];
				pDC->MoveTo(t);
				t.x -= iLen;
				t.y -= iLen;
				pDC->LineTo(t);
			}

			// restore the old pen and drawing mode
			pDC->SetROP2(iOldDrawMode);
			pDC->SelectObject(pOldPen);
		}

		// Draw a border if this entity is selected
		if (m_bSelected)
		{
			// Get the selection border color
			COLORREF crSelection = m_pGenericDisplayCtrl->getSelectionColor();

			// Draw the selection border
			CPen borderPen(PS_SOLID, giZONE_BORDER_WIDTH, crSelection);
			pDC->SelectObject(&borderPen);
			pDC->MoveTo(m_zoneSelectionBorderViewPts[3]);
			pDC->PolylineTo(m_zoneSelectionBorderViewPts, 4);

			// Draw grip handles if zone entity adjustment is enabled
			if (m_pGenericDisplayCtrl->isZoneEntityAdjustmentEnabled())
			{
				// Draw the grip handles
				CBrush brush( RGB(255, 255, 255) );
				CPen pen(PS_SOLID, 1, RGB(0, 0, 0) );
				pDC->SelectObject(&pen);
		
				for (int i=0; i < 4; i++)
				{
					// Calculate the grip handle dimensions
					CPoint& gripPoint = m_zoneGripViewPoints[i];
					RECT gripHandle = 
					{
						gripPoint.x - giHALF_GRIP_HANDLE_WIDTH, 
						gripPoint.y - giHALF_GRIP_HANDLE_WIDTH, 
						gripPoint.x + giHALF_GRIP_HANDLE_WIDTH, 
						gripPoint.y + giHALF_GRIP_HANDLE_WIDTH
					};

					// Draw the grip handle
					pDC->FillRect(&gripHandle, &brush);

					// Shrink the rectangle by one pixel on all sides to draw the border
					// Note: This leaves a pixel of white and then a pixel of black for the border.
					gripHandle.left += 1;
					gripHandle.top += 1;
					gripHandle.right -= 1;
					gripHandle.bottom -= 1;

					// Draw the grip handle's border
					pDC->Rectangle(&gripHandle);
				}
			}
		}
	}
	catch(...)
	{
		if (pDC != __nullptr)
		{
			pView->m_ImgEdit.ReleaseDC(pDC);
		}
		throw;
	}

	if (pDC != __nullptr)
	{
		pView->m_ImgEdit.ReleaseDC(pDC);
	}
}
//-------------------------------------------------------------------------------------------------
void ZoneEntity::CalculateViewCoords()
{
	CGenericDisplayView *pView = m_pGenericDisplayCtrl->getUGDView(); 

	// ensure zone vertices are up to date.
	calculateZoneVertices();
	
	// We need to convert the zone bounds from image 
	// coordinates to view coordinates. Ultimately we have to draw the zone in view.
	for (int i = 0; i < 4; i++)
	{
		// this assignment is required to convert image coordinates to view cordinates
		m_zoneBoundViewPts[i] = m_zoneBoundPts[i];

		// translate the image pixel coordinates into view coordinates
		pView->ImageInWorldToViewCoordinates((int *) &m_zoneBoundViewPts[i].x,
			(int *) &m_zoneBoundViewPts[i].y, m_ulPageNumber);
	}

	// repeat the same type of calculations for the zone border
	if (m_bBorderVisible)
	{
		expandCornerPoints(m_zoneBoundViewPts, giZONE_BORDER_WIDTH, m_zoneBorderViewPts);
		m_zoneBorderViewPts[4] = m_zoneBorderViewPts[0];
	}

	if (m_bSelected)
	{
		calculateZoneSelectionPoints();
	}
}
//-------------------------------------------------------------------------------------------------
double ZoneEntity::getDistanceToEntity(double dX, double dY)
{
	// ensure bounds are updated
	calculateZoneVertices();

    double dMinDistance, dDistance;
	dMinDistance = 0xFFFFFFFF;
	
	int iX = (int)dX;
	int iY = (int)dY;
	
	//	store min distance in the dMinDistance
	for(int i = 0; i < 4; i++)
	{
		Point point(dX, dY);
		
		int iStart = i, iEnd;
		
		if(i == 3)
		{
			iEnd = 0;
		}
		else
			iEnd = i+1;
		
		Point lineStartPt (m_zoneBoundPts[iStart].x, m_zoneBoundPts[iStart].y);
		Point lineEndPt (m_zoneBoundPts[iEnd].x, m_zoneBoundPts[iEnd].y);
		dDistance = getDistanceBetweenPointAndLine (point, lineStartPt, lineEndPt);
		if(dDistance <= dMinDistance)
			dMinDistance = dDistance;		
	}

	return dMinDistance;
}
//-------------------------------------------------------------------------------------------------
void ZoneEntity::getZoneEntParameters(long& lStPosX, long& lStPosY, long& lEndPosX, long& lEndPosY, 
									  long& lHeight)
{
	lStPosX = m_lStartPosX;
	lStPosY = m_lStartPosY;
	lEndPosX = m_lEndPosX;
	lEndPosY = m_lEndPosY;
	lHeight = m_ulTotalZoneHeight;
}
//-------------------------------------------------------------------------------------------------
void ZoneEntity::setZoneEntParameters(long lStPosX, long lStPosY, long lEndPosX, long lEndPosY, 
									  long lHeight)
{
	m_lStartPosX = lStPosX;
	m_lStartPosY = lStPosY;
	m_lEndPosX = lEndPosX;
	m_lEndPosY = lEndPosY;
	m_ulTotalZoneHeight = lHeight;

	// Update other variables related to the change in zone extents
	updateWorldUnitVariables();

	// Recalculate the zone bounds and grip points
	m_bZonePtsCalculated = false;
	m_bZoneSelectionPtsCalculated = false;
}
//-------------------------------------------------------------------------------------------------
void ZoneEntity::calculateZoneVertices()
{
	// calculate the zone vertices.zone is rectangular bounding box which do have a color.
	// It is being called as zone rather than rectangle as it may be angular highlighting 
	// any angular text.
	//
	//				  (0,0)	*-----------------------* (1,1)
	//						|						|
	//     zone start point	+                       + zone end point
	//						|						|
	//				  (3,3)	*-----------------------* (2,2)
	//  
	// Zone is symmetric about the line joining the start and end poins. The 4 vertices of the zone
	// are dependant on the angle of the line joining the start and end points of the zone. The actual
	// height of the zone is always 1 pixel less than the m_ulTotalZoneHeight value which is in image pixels

	// calculate the zone vertices if they haven't already been calculated.
	if (!m_bZonePtsCalculated)
	{
		// calculate the vertices of the zone
		long x1,x2,y1,y2,ht;

		x1 = m_lStartPosX;
		x2 = m_lEndPosX;
		y1 = m_lStartPosY;
		y2 = m_lEndPosY;
		
		CGenericDisplayView *pView = m_pGenericDisplayCtrl->getUGDView();

		ht = m_ulTotalZoneHeight;

		// TESTTHIS cast to long
		y1 = (long)pView->getLoadedImageHeight() - y1;
		y2 = (long)pView->getLoadedImageHeight() - y2;

		getZoneCornerPoints(x1, x2, y1, y2, ht, true, m_zoneBoundPts);
		
		m_bZonePtsCalculated = true;
	}
}
//-------------------------------------------------------------------------------------------------
void ZoneEntity::calculateZoneSelectionPoints()
{
	// Calculate the selection corner points
	expandCornerPoints(m_zoneBoundViewPts, giZONE_BORDER_WIDTH * 3 - 2, m_zoneSelectionBorderViewPts);

	// Calculate the selection midpoints
	for (int i = 0; i < 4; i++)
	{
		int iNext = (i+1) % 4;
		m_zoneGripViewPoints[i] = CPoint((m_zoneSelectionBorderViewPts[i].x + m_zoneSelectionBorderViewPts[iNext].x) / 2, 
			(m_zoneSelectionBorderViewPts[i].y + m_zoneSelectionBorderViewPts[iNext].y) / 2);
	}

	// Calculate the zone points if they haven't already been calculated.
	if (!m_bZoneSelectionPtsCalculated)
	{
		// Get the image height
		CGenericDisplayView* pView = m_pGenericDisplayCtrl->getUGDView();
		long lImageHeight = (long) pView->getLoadedImageHeight();

		// Convert the zone selection points from view coordinates to image coordinates. 
		for (int i = 0; i < 4; i++)
		{
			// Convert the vertices
			m_zoneSelectionBorderPts[i] = m_zoneSelectionBorderViewPts[i];
			pView->ViewToImageInWorldCoordinates((int*) &m_zoneSelectionBorderPts[i].x,
				(int*) &m_zoneSelectionBorderPts[i].y, m_ulPageNumber);
		}

		// Calculate the midpoints of the zone
		getZoneMidPoints(m_lStartPosX, m_lEndPosX, m_lStartPosY, m_lEndPosY, 
			m_ulTotalZoneHeight, m_zoneGripPoints);
		
		m_bZoneSelectionPtsCalculated = true;
	}
}
//-------------------------------------------------------------------------------------------------
void ZoneEntity::getZoneBoundingRect(GDRectangle& rBoundingRectangle)
{
	// ensure that the coords are up-to-date
	calculateZoneVertices();

	Point bottomLeft, topRt;

	// find the minimum x coordinate among all the vetices of the zone
	bottomLeft.dXPos = m_zoneBoundPts[0].x;
	for (int i = 1; i < 4; i++)
	{
		if (m_zoneBoundPts[i].x < bottomLeft.dXPos)
			bottomLeft.dXPos = m_zoneBoundPts[i].x;
	}

	// find the minimum y coordinte among all the vertices of the zone
	bottomLeft.dYPos = m_zoneBoundPts[0].y;
	for (unsigned int i = 1; i < 4; i++)
	{
		if (m_zoneBoundPts[i].y < bottomLeft.dYPos)
		{
			bottomLeft.dYPos = m_zoneBoundPts[i].y;
		}
	}

	// find the maximum x coordinate among all the vetices of the zone
	topRt.dXPos = m_zoneBoundPts[0].x;
	for (unsigned int i = 1; i < 4; i++)
	{
		if (m_zoneBoundPts[i].x > topRt.dXPos)
		{
			topRt.dXPos = m_zoneBoundPts[i].x;
		}
	}
	// find the maximum y coordinte among all the vertices of the zone
	topRt.dYPos = m_zoneBoundPts[0].y;
	for (unsigned int i = 1; i < 4; i++)
	{
		if (m_zoneBoundPts[i].y > topRt.dYPos)
		{
			topRt.dYPos = m_zoneBoundPts[i].y;
		}
	}

	// set minimum x and minimum y as the Bottom Left point for the GDRectangle
	rBoundingRectangle.setBottomLeft(bottomLeft);

	// set maximum x and maximum y as the Top Right point for the GDRectangle
	rBoundingRectangle.setTopRight(topRt);
}
//-------------------------------------------------------------------------------------------------
BOOL ZoneEntity::isPtOnZone(int x, int y)
{
	// get the zone vertices which are in image world coordinates
	calculateZoneVertices();

	CRgn rgnTextBox;
	CPoint ptVertex[5];

	//	get all the four co-ordinates of textbox
	ptVertex[0] = m_zoneBoundPts[0];
	ptVertex[1] = m_zoneBoundPts[1];
	ptVertex[2] = m_zoneBoundPts[2];
	ptVertex[3] = m_zoneBoundPts[3];
	ptVertex[4] = m_zoneBoundPts[0];

	// create text box region
	rgnTextBox.CreatePolygonRgn( ptVertex, 5, ALTERNATE);

	//	check whether the point is lying in the region or not
	if(rgnTextBox.PtInRegion(x, y))
	{
		rgnTextBox.DeleteObject();
		return TRUE;
	}

	//	Done with the Region. Delete the object
	rgnTextBox.DeleteObject();

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
int ZoneEntity::getGripHandleId(double x, double y)
{
	// Get the zone vertices which are in image world coordinates
	calculateZoneSelectionPoints();

	// Convert the coordinates to client pixels
	double dX = x, dY = y;
	CGenericDisplayView* pView = m_pGenericDisplayCtrl->getUGDView();
	pView->ImageInWorldToViewCoordinates(&dX, &dY, m_pGenericDisplayCtrl->getCurrentPageNumber());

	// Check if the point is contained by a grip handle
	for (int i = 0; i < 4; i++)
	{
		CPoint& gripPoint = m_zoneGripViewPoints[i];
		if (abs(gripPoint.x - dX) <= giHALF_GRIP_HANDLE_WIDTH && 
			abs(gripPoint.y - dY) <= giHALF_GRIP_HANDLE_WIDTH)
		{
			return i;
		}
	}

	return -1;
}
//-------------------------------------------------------------------------------------------------
EGripHandle ZoneEntity::getGripHandle(int iGripHandleId)
{
	// Get the center point in world coordinates
	CGenericDisplayView* pView = m_pGenericDisplayCtrl->getUGDView();
	int dCenterX = (m_lStartPosX + m_lEndPosX) / 2;
	int dCenterY = (int)pView->getLoadedImageHeight() - ((m_lStartPosY + m_lEndPosY) / 2);
	
	// Convert the center to view coordinates
	pView->ImageInWorldToViewCoordinates(&dCenterX, &dCenterY, (long) m_ulPageNumber);

	// Check if this is a corner
	CPoint& viewGripPoint = m_zoneGripViewPoints[iGripHandleId];
    if (iGripHandleId >= 4)
    {
		return (EGripHandle)((viewGripPoint.x < dCenterX ? kLeft : kRight) | 
			(viewGripPoint.y < dCenterY ? kTop : kBottom));
    }

    // Get the angle in degrees between the highlight's 
    // center and the center of the selected grip handle.
    double dAngle = convertRadiansToDegrees(
		atan2((double)(viewGripPoint.y - dCenterY), (double)(viewGripPoint.x - dCenterX)) );

    // Express the angle as a positive number greater than 22.5 degrees
    // NOTE: This is done so that when the number is divided by 45 degrees, it will round 
	// to a number between one and eight.
	if (dAngle < 22.5)
    {
        dAngle += 360;
    }

    // Set the cursor based on the nearest 45 degree angle
    // NOTE: The degree measured is expressed in a Cartesian coordinate system, not the 
	// top-left client coordinate system. For this reason the result is mirrored on the 
    // x-axis, and the first & third quadrants become the second & fourth quadrants 
	// respectively and vice versa.
    switch (round(dAngle / 45.0))
    {
		// 45 degrees
        case 1:
			return kRightBottom;

		// 90 degrees 
        case 2:
			return kBottom;

		// 135 degrees 
        case 3:
			return kLeftBottom;

		// 180 degrees 
        case 4:
			return kLeft;

		// 225 degrees
        case 5:
            return kLeftTop;
	
		// 270 degrees
        case 6:
			return kTop;

		// 315 degrees
        case 7:
            return kRightTop;

		// 0 degrees
        case 8:
            return kRight;

        default:

            // This is a non-serious logic error. Return no grip handle.
            return kNone;
    }
}
//-------------------------------------------------------------------------------------------------
void ZoneEntity::makeGripHandleEndPoint(int iGripHandleId)
{
	// If this grip handle is already the end point, we are done.
    CPoint& gripPoint = m_zoneGripPoints[iGripHandleId];
	if (gripPoint.x == m_lEndPosX && gripPoint.y == m_lEndPosY)
    {
        return;
    }

    // Check if this grip handle is the start point
    if (gripPoint.x == m_lStartPosX && gripPoint.y == m_lStartPosY)
    {
        // Swap the start point and end point
		swap(m_lStartPosX, m_lEndPosX);
		swap(m_lStartPosY, m_lEndPosY);

        return;
    }

    // This is an endpoint of the height.
    // Find the other endpoint.
    for (int i = 0; i < 4; i++)
    {
		// Don't compare this grip handle to itself
		if (iGripHandleId == i)
		{
			continue;
		}

		// Check if this is the opposing grip point
		CPoint& opposingGripPoint = m_zoneGripPoints[i];
        if ((opposingGripPoint.x != m_lStartPosX || opposingGripPoint.y != m_lStartPosY) && 
			(opposingGripPoint.x != m_lEndPosX || opposingGripPoint.y != m_lEndPosY))
        {
            // Define the new highlight
			long lDeltaX = m_lStartPosX - m_lEndPosX;
			long lDeltaY = m_lStartPosY - m_lEndPosY;
			m_ulTotalZoneHeight = (long) sqrt((double)lDeltaX * lDeltaX + lDeltaY * lDeltaY);
			m_lStartPosX = opposingGripPoint.x;
			m_lStartPosY = opposingGripPoint.y;
			m_lEndPosX = gripPoint.x;
			m_lEndPosY = gripPoint.y;

			// The zone height must be odd and greater than 5 [FlexIDSCore #3280]
			if (m_ulTotalZoneHeight < 5)
			{
				m_ulTotalZoneHeight = 5;
			}
			else if (m_ulTotalZoneHeight % 2 == 0)
			{
				m_ulTotalZoneHeight++;
			}

            return;
        }
    }
}
//-------------------------------------------------------------------------------------------------
bool ZoneEntity::isValid()
{
	// Ensure coordinates are positive [FlexIDSCore #3377-3379]
	if (m_lStartPosX < 0 || m_lStartPosY < 0 || m_lEndPosX < 0 || m_lEndPosY < 0)
	{
		return false;
	}

	// Calculate the zone vertices
	calculateZoneVertices();

	// Calculate the minimum and maximum coordinates
	long lMinX = m_zoneBoundPts[0].x;
	long lMinY = m_zoneBoundPts[0].y;
	long lMaxX = lMinX;
	long lMaxY = lMinY;

	for (int i = 1; i < 4; i++)
	{
		CPoint &pt = m_zoneBoundPts[i];
		if (pt.x < lMinX)
		{
			lMinX = pt.x;
		}
		else if (pt.x > lMaxX)
		{
			lMaxX = pt.x;
		}

		if (pt.y < lMinY)
		{
			lMinY = pt.y;
		}
		else if (pt.y > lMaxY)
		{
			lMaxY = pt.y;
		}
	}

	// Highlight is valid only if it is fully contained on the image [FlexIDSCore #3376]
	CGenericDisplayView* pView = m_pGenericDisplayCtrl->getUGDView();
	if (lMinX < 0 || lMinY < 0 || lMaxX > pView->getLoadedImageWidth() || 
		lMaxY > pView->getLoadedImageHeight())
	{
		return false;
	}

	// Is valid if it spans at least one pixel
	return (lMaxX - lMinX) >= 1 && (lMaxY - lMinY) >= 1;
}
//-------------------------------------------------------------------------------------------------
double ZoneEntity::getAngle(long nX1, long nX2, long nY1, long nY2, 
							bool bInvertYCoordinates)
{
	// Determine the orientation of the specified points.
	int orientation = getOrientation(nX1, nX2, nY1, nY2, bInvertYCoordinates);

	// Calculate the angle to use based on the zone's base angle.
	double dAngle = m_dBaseAngle + (MathVars::PI / 2.0 * orientation);

	// Invert the angle as required
	if (bInvertYCoordinates)
	{
		dAngle = -dAngle;
	}

	return dAngle;
}
//-------------------------------------------------------------------------------------------------
int ZoneEntity::getEntDataString(CString &zDataString)
{
	// format the attributes in the sequence of startpoint xvalue, yvalue, endpoint xvalue, yvalue, zone height
	// write the data into the file	
	zDataString.Format("%d:%d,%d:%d,%d", m_lStartPosX, m_lStartPosY, m_lEndPosX, m_lEndPosY, m_ulTotalZoneHeight);
	
	return zDataString.GetLength();
}
//-------------------------------------------------------------------------------------------------
void ZoneEntity::expandCornerPoints(CPoint cornerPoints[4], long lExpand, CPoint *pPoints)
{
	// Determine the start point, end point and height of the zone
	// represented by the view coordinates of the zone
	long nX1, nX2, nY1, nY2, nHeight;

	nX1 = (cornerPoints[0].x + cornerPoints[3].x) / 2;
	nX2 = (cornerPoints[1].x + cornerPoints[2].x) / 2;
	nY1 = (cornerPoints[0].y + cornerPoints[3].y) / 2;
	nY2 = (cornerPoints[1].y + cornerPoints[2].y) / 2;
	nHeight = (long)sqrt((long double)((cornerPoints[0].x - cornerPoints[3].x)
		* (cornerPoints[0].x - cornerPoints[3].x) + 
		(cornerPoints[0].y - cornerPoints[3].y) * 
		(cornerPoints[0].y - cornerPoints[3].y)));

	// Determine the horizontal and vertical displacement for the
	// start and end points along the centerline of the zone on which
	// we are going to expand the zone by lExpand pixels
	// on each side
	double dAngle = getAngle(nX1, nX2, nY1, nY2, false);

	long nDeltaX = (long) (lExpand / 2.0 * cos(dAngle));
	long nDeltaY = (long) (lExpand / 2.0 * sin(dAngle));
	
	// Expand the zone by lExpand pixels on all sides
	nX1 -= nDeltaX;
	nY1 -= nDeltaY;
	nX2 += nDeltaX;
	nY2 += nDeltaY;
	nHeight += (long)(lExpand);
	getZoneCornerPoints(nX1, nX2, nY1, nY2, nHeight, false, pPoints);
}
//-------------------------------------------------------------------------------------------------
void ZoneEntity::getZoneCornerPoints(long nX1, long nX2, long nY1, long nY2, long nHeight,
									 bool bInvertYCoordinates, CPoint *pPoints)
{
	// Calculate the angle of the line dy/dx
	double dAngle = getAngle(nX1, nX2, nY1, nY2, bInvertYCoordinates);

	// Calculate the x and y modifiers
	long lModifierX = (long)((nHeight / 2) * sin(dAngle));
	long lModifierY = (long)((nHeight / 2) * cos(dAngle));

	// update the vertices based on the rotation angle, the highlighting
	// should be in rectangular shape symmetric about the line joining the
	// start point and end point of the zone
	// TESTTHIS cast to long
	pPoints[0].x = nX1 - lModifierX;
	pPoints[0].y = nY1 + lModifierY;

	pPoints[1].x = nX2 - lModifierX;
	pPoints[1].y = nY2 + lModifierY;

	pPoints[2].x = nX2 + lModifierX;
	pPoints[2].y = nY2 - lModifierY;

	pPoints[3].x = nX1 + lModifierX;
	pPoints[3].y = nY1 - lModifierY;
}
//-------------------------------------------------------------------------------------------------
void ZoneEntity::getZoneMidPoints(long nX1, long nX2, long nY1, long nY2, long nHeight, 
								  CPoint *pPoints)
{
	// Calculate the center point of the zone
	long lCenterX = (nX1 + nX2) / 2;
	long lCenterY = (nY1 + nY2) / 2;

	// Calculate the angle of the line (dy/dx)
	double dAngle = getAngle(nX1, nX2, nY1, nY2, false);

	// Calculate the x and y modifiers
	long lModifierX = (long)((nHeight / 2) * sin(dAngle));
	long lModifierY = (long)((nHeight / 2) * cos(dAngle));

	// Update the midpoints based on the rotation angle
	pPoints[0] = CPoint(lCenterX - lModifierX, lCenterY + lModifierY);
	pPoints[1] = CPoint(nX2, nY2);
	pPoints[2] = CPoint(lCenterX + lModifierX, lCenterY - lModifierY);
	pPoints[3] = CPoint(nX1, nY1);
}
//-------------------------------------------------------------------------------------------------
int ZoneEntity::getOrientation(long nX1, long nX2, long nY1, long nY2, bool bInvertYCoordinates)
{
	// Invert the Y coordinates as necessary before checking the orientation.
	if (bInvertYCoordinates)
	{
		long nTempY1 = nY1;
		nY1 = nY2;
		nY2 = nTempY1;
	}

	// Start by getting the angle of the vector connecting points X and Y
	double dAngle = atan2((double)(nY2 - nY1), (double)(nX2 - nX1));
	
	// Normalize the angle to the raster zone's coordinate system
	dAngle -= m_dBaseAngle;

	// Add 45 degrees (rounding to the nearest 90 degree vector).
	dAngle += MathVars::PI / 4.0;

	// Make sure the angle is positive by adding a full revolution
	dAngle += (2 * MathVars::PI);

	// Divide by 90 degrees to obtain the number of 90 rotations past zero that the incoming
	// vector is closest to.
	dAngle /= MathVars::PI / 2.0;
	
	// The modulus is the number to be used as the orientation identifier.
	return (int)dAngle % 4;
}
//-------------------------------------------------------------------------------------------------