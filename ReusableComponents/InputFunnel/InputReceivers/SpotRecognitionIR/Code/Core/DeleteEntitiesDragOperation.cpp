//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	DeleteEntitiesDragOperation.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan
//
//==================================================================================================

#include "stdafx.h"
#include "DeleteEntitiesDragOperation.h"
#include <math.h>

#include <TPPolygon.h>
#include <StringTokenizer.h>
#include <mathUtil.h>
#include <cpputil.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

DeleteEntitiesDragOperation::DeleteEntitiesDragOperation(CUCLIDGenericDisplay& rUCLIDGenericDisplayCtrl)
:LButtonDragOperation(rUCLIDGenericDisplayCtrl, kRectangle)
{
}
//--------------------------------------------------------------------------------------------------
void DeleteEntitiesDragOperation::processDragOperation(const CartographicPoint& p1,
													   const CartographicPoint& p2)
{
	// get the current page number
	unsigned long ulCurrentPageNumber = m_UCLIDGenericDisplayCtrl.getCurrentPageNumber();

	// convert the two cartographic points that are in world coordinates into
	// image pixel coordinates.
	POINT imagePoint1, imagePoint2;
	m_UCLIDGenericDisplayCtrl.convertWorldToImagePixelCoords(p1.m_dX, p1.m_dY, 
		&imagePoint1.x, &imagePoint1.y, ulCurrentPageNumber);
	m_UCLIDGenericDisplayCtrl.convertWorldToImagePixelCoords(p2.m_dX, p2.m_dY, 
		&imagePoint2.x, &imagePoint2.y, ulCurrentPageNumber);

	// flip the coordinate system so that the Y axis is mirrored
	// NOTE: the UGD OCX deals with coordiantes with top-left = (0,0), whereas
	// the topology classes assume a bottom-left = (0,0) like in Autocad.
	imagePoint1.y = -imagePoint1.y;
	imagePoint2.y = -imagePoint2.y;

	// create a polygon that represents the crossing selection window
	TPPolygon crossingSelectionWindow;
	crossingSelectionWindow.addPoint(TPPoint(imagePoint1.x, imagePoint1.y));
	crossingSelectionWindow.addPoint(TPPoint(imagePoint1.x, imagePoint2.y));
	crossingSelectionWindow.addPoint(TPPoint(imagePoint2.x, imagePoint2.y));
	crossingSelectionWindow.addPoint(TPPoint(imagePoint2.x, imagePoint1.y));

	// iterate through all the zone entities, create polygons for each of
	// the zone entities, and determine which polygons are overlapping
	// with the crossing-window selection polygon
	string strEntities = m_UCLIDGenericDisplayCtrl.queryEntities("Type=ZONE");
	vector<string> vecTokens;
	StringTokenizer st;
	st.parse(strEntities, vecTokens);
	for (unsigned int ui = 0; ui < vecTokens.size(); ui++)
	{
		if (!vecTokens[ui].empty())
		{
			// get the ID of the zone
			unsigned long ulEntityID = asUnsignedLong( vecTokens[ui] );
		
			deleteEntity(ulEntityID, crossingSelectionWindow);
		}
	}
}
//--------------------------------------------------------------------------------------------------
void DeleteEntitiesDragOperation::deleteEntity(unsigned long ulEntityID, 
											   const TPPolygon& crossingSelectionWindow)
{	
	// get the current page number
	unsigned long ulCurrentPageNumber = m_UCLIDGenericDisplayCtrl.getCurrentPageNumber();

	// get the page number associated with the zone
	unsigned long ulZoneEntityPageNumber = asUnsignedLong( (LPCTSTR)
		m_UCLIDGenericDisplayCtrl.getEntityAttributeValue(ulEntityID, "Page"));
	
	// if the entity is on a different page than the current one, then
	// no further processing is required as the user only intends to delete
	// entities on the current page.
	if (ulZoneEntityPageNumber != ulCurrentPageNumber)
	{
		return;
	}
	
	// get the zone entity attributes.
	long lStartPosX, lStartPosY, lEndPosX, lEndPosY, lTotalZoneHeight;
	m_UCLIDGenericDisplayCtrl.getZoneEntityParameters(ulEntityID, &lStartPosX, &lStartPosY, 
		&lEndPosX, &lEndPosY, &lTotalZoneHeight);
	
	// flip the coordinate system so that the Y axis is mirrored
	// NOTE: the UGD OCX deals with coordiantes with top-left = (0,0), whereas
	// the topology classes assume a bottom-left = (0,0) like in Autocad.
	lStartPosY = -lStartPosY;
	lEndPosY = -lEndPosY;
	
	// convert the zone coordinates to real world coordinates.
	//continue here
	
	// determine the points of the polygon around the zone
	double dZoneAngle = TPPoint(lStartPosX, lStartPosY).angleTo(TPPoint(lEndPosX, lEndPosY));
	TPPoint tp1, tp2, tp3, tp4;
	double dSinA = sin(dZoneAngle + MathVars::PI/2.0);
	double dCosA = cos(dZoneAngle + MathVars::PI/2.0);
	tp1.m_dX = lStartPosX + (lTotalZoneHeight/2.0) * dCosA;
	tp1.m_dY = lStartPosY + (lTotalZoneHeight/2.0) * dSinA;
	tp2.m_dX = lStartPosX - (lTotalZoneHeight/2.0) * dCosA;
	tp2.m_dY = lStartPosY - (lTotalZoneHeight/2.0) * dSinA;
	tp3.m_dX = lEndPosX - (lTotalZoneHeight/2.0) * dCosA;
	tp3.m_dY = lEndPosY - (lTotalZoneHeight/2.0) * dSinA;
	tp4.m_dX = lEndPosX + (lTotalZoneHeight/2.0) * dCosA;
	tp4.m_dY = lEndPosY + (lTotalZoneHeight/2.0) * dSinA;
	
	// create a polygon
	TPPolygon zonePoly;
	zonePoly.addPoint(tp1);
	zonePoly.addPoint(tp2);
	zonePoly.addPoint(tp3);
	zonePoly.addPoint(tp4);
	
	if (crossingSelectionWindow.overlaps(zonePoly))
	{
		m_UCLIDGenericDisplayCtrl.deleteEntity(ulEntityID);
	}
}