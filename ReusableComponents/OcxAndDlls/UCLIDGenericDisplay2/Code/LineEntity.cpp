//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	LineEntity.cpp
//
// PURPOSE:	This is an implementation file for LineEntity() class.
//			Where the LineEntity() class has been derived from GenericEntity() class.
//			The code written in this file makes it possible to implement the various
//			application methods in the user interface.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================
// LineEntity.cpp : implementation file
//
#include "stdafx.h"
#include "LineEntity.h"
#include "GenericDisplayView.h"
#include "GenericDisplayFrame.h"
#include "UCLIDException.h"

#include <cmath>
//==================================================================================================
//////////////////////////////////////////////////////////////////////////////
//	LineEntity message handlers

LineEntity::LineEntity(unsigned long id)
	: GenericEntity(id)
{

}
//==================================================================================================
LineEntity::LineEntity(unsigned long id, Point StPt, Point EndPt)
	: GenericEntity(id)
{
	//	set the start and end point
	m_startPoint = StPt;
	m_endPoint	= EndPt;
}
//==================================================================================================
LineEntity::LineEntity(unsigned long id, EntityAttributes& EntAttr, COLORREF color, Point StPt, 
					   Point EndPt)
	: GenericEntity(id, EntAttr, color)
{
	//	set the start and end point
	m_startPoint = StPt;
	m_endPoint	= EndPt;
}
//==================================================================================================
LineEntity::~LineEntity()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16453");
}
//==================================================================================================
void LineEntity::getExtents(GDRectangle& rBoundingRectangle)
{
	Point bottomLeft, topRt;
	bottomLeft = m_gdEntExtents.getBottomLeft();
	topRt = m_gdEntExtents.getTopRight();

	//Set the bottomleft and topright points to rBoundingRectangle
	rBoundingRectangle.setBottomLeft(bottomLeft);
	rBoundingRectangle.setTopRight(topRt);
}
//==================================================================================================
void LineEntity::ComputeEntExtents ()
{
	Point bottomLeft, topRt;

	//caluculate the bounding rectangle co-ordinates
	bottomLeft.dXPos = min(m_startPoint.dXPos, m_endPoint.dXPos);
	bottomLeft.dYPos = min(m_startPoint.dYPos, m_endPoint.dYPos);
	topRt.dXPos = max(m_startPoint.dXPos, m_endPoint.dXPos);
	topRt.dYPos = max(m_startPoint.dYPos, m_endPoint.dYPos);

	//set 
	m_gdEntExtents.setBottomLeft (bottomLeft);
	m_gdEntExtents.setTopRight (topRt);
}
//==================================================================================================
string LineEntity::getDesc()
{
	string zStrLine ("LINE");
	return zStrLine;
}
//==================================================================================================
void LineEntity::offsetBy(double dX, double dY)
{
	//	start point
	m_startPoint.dXPos += dX;
	m_startPoint.dYPos += dY;

	//	end point
	m_endPoint.dXPos += dX;
	m_endPoint.dYPos += dY;

}
//==================================================================================================
void LineEntity::EntDraw (BOOL bDraw)
{
	if (m_bVisible == FALSE)
		return;

	//	Get View for drawing the line entity
	CGenericDisplayView *pView = NULL;
	pView = m_pGenericDisplayCtrl->getUGDView();

	CDC* pDC = pView->m_ImgEdit.GetDC();

	//	Create a pen object
	CPen EntPen;
	if(bDraw == FALSE)
	{
		EntPen.CreatePen (PS_SOLID, 0, m_color);
	}
	else if (m_bSelected == TRUE)
	{
		EntPen.CreatePen (PS_DASH, 0, m_color);
		pDC->SetBkMode (TRANSPARENT);
	}
	else 
		EntPen.CreatePen (PS_SOLID, 0, m_color);

	double dX1 = m_startPoint.dXPos;
	double dY1 = m_startPoint.dYPos;

	double dX2 = m_endPoint.dXPos;
	double dY2 = m_endPoint.dYPos;

	//	Convert image co-ordinates to view
	pView->ImageInWorldToViewCoordinates (&dX1, &dY1, m_ulPageNumber);

	//	Convert image co-ordinates to view co-ordinates
	pView->ImageInWorldToViewCoordinates (&dX2, &dY2, m_ulPageNumber);

	// TESTTHIS cast to int
	int w = (int)(dX2 - dX1);
	int h = (int)(dY2 - dY1);
	
	pDC->SetROP2(R2_COPYPEN);

	//	Select pen object
	CPen* pOldPen = pDC->SelectObject(&EntPen);	

	double dBaseAng = m_pGenericDisplayCtrl->getBaseRotation(m_ulPageNumber);

	//	Move to the starting a point
	// TESTTHIS cast to int
	pDC->MoveTo((int)dX1, (int)dY1);
	
	//	draw a line to the end point
	pDC->LineTo((int)dX2, (int)dY2);

	pDC->SelectObject (pOldPen);

	pView->m_ImgEdit.ReleaseDC(pDC);
}
//==================================================================================================
int LineEntity::getEntDataString(CString &zDataString)
{
	//	Get View for drawing the line entity
	CGenericDisplayView *pView = NULL;
	pView = m_pGenericDisplayCtrl->getUGDView();

	//	set the data to the string
	zDataString.Format("%.4f:%.4f,%.4f:%.4f", (m_startPoint.dXPos)*m_pGenericDisplayCtrl->getXMultFactor(), (m_startPoint.dYPos)*m_pGenericDisplayCtrl->getYMultFactor(),
								(m_endPoint.dXPos)*m_pGenericDisplayCtrl->getXMultFactor(), (m_endPoint.dYPos)*m_pGenericDisplayCtrl->getYMultFactor());

	return zDataString.GetLength();
}
//==================================================================================================
double LineEntity::getDistanceToEntity(double dX, double dY)
{
	Point pt(dX, dY);
	Point lineStartPt (m_startPoint.dXPos, m_startPoint.dYPos);
	Point lineEndPt (m_endPoint.dXPos, m_endPoint.dYPos);

	// call base class method to return the distance between the point and the line
	return getDistanceBetweenPointAndLine (pt, lineStartPt, lineEndPt);
}
//==================================================================================================
