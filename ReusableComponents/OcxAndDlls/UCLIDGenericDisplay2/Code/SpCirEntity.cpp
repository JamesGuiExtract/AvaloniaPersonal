//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	SpCirEntity.cpp
//
// PURPOSE:	This is an implementation file for SpCirEntity() class.
//			Where the SpCirEntity() class has been derived from GenericEntity() class.
//			The code written in this file makes it possible to implement the various
//			application methods in the user interface.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================
// SpCirEntity.cpp : implementation file
//
#include "stdafx.h"
#include "SpCirEntity.h"
#include "GenericDisplayView.h"
#include "GenericDisplayFrame.h"
#include "UCLIDException.h"

#include <cmath>

//////////////////////////////////////////////////////////////////////////////
//	SpecializedCircleEntity message handlers
//==================================================================================================
SpecializedCircleEntity::SpecializedCircleEntity(unsigned long id)
	: GenericEntity(id)
{

}
//==================================================================================================
SpecializedCircleEntity::SpecializedCircleEntity(unsigned long id, Point center, double dRadius)
	: GenericEntity(id)
{
	m_ptCenter			= center;
	m_dPercentRadius	= dRadius;
	m_dCurrentRadius = 0.0;
}

//==================================================================================================
SpecializedCircleEntity::~SpecializedCircleEntity()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16457");
}
//==================================================================================================
void SpecializedCircleEntity::getExtents(GDRectangle& rBoundingRectangle)
{
	// specialized circle radius is calculated based on the view extents.
	// so whenever circle extents are requested, compute with the current
	// view extents and then return the updated extents
	ComputeEntExtents();

	Point bottomLeft, topRt;
	bottomLeft = m_gdEntExtents.getBottomLeft();
	topRt = m_gdEntExtents.getTopRight();

	//Set the bottomleft and topright points to rBoundingRectangle
	rBoundingRectangle.setBottomLeft(bottomLeft);
	rBoundingRectangle.setTopRight(topRt);
}
//==================================================================================================
void SpecializedCircleEntity::ComputeEntExtents ()
{
	Point Pt1, Pt2;

	//	assign the center
	Pt1 = m_ptCenter;
	Pt2 = m_ptCenter;

	//	get pointer to the view object 
	CGenericDisplayView* pView = NULL;
	//	set the view pointer
	pView = m_pGenericDisplayCtrl->getUGDView();


	CRect wndRect;
	pView->GetWindowRect(wndRect);
	int iWidth = wndRect.right - wndRect.left;
	int iHeight = wndRect.bottom - wndRect.top;

	double dRadius = (m_dPercentRadius/100.0) * (iWidth + iHeight)/2;

	pView->ImageInWorldToViewCoordinates (&Pt1.dXPos, &Pt1.dYPos, m_ulPageNumber);
	pView->ImageInWorldToViewCoordinates (&Pt2.dXPos, &Pt2.dYPos, m_ulPageNumber);

	Pt1.dXPos -= dRadius;
	Pt2.dXPos += dRadius;

	Pt1.dYPos -= dRadius;
	Pt2.dYPos += dRadius;

	pView->ViewToImageInWorldCoordinates (&Pt1.dXPos, &Pt1.dYPos, m_ulPageNumber);
	pView->ViewToImageInWorldCoordinates (&Pt2.dXPos, &Pt2.dYPos, m_ulPageNumber);

	double dMinX = min (Pt1.dXPos, Pt2.dXPos);
	double dMinY = min (Pt1.dYPos, Pt2.dYPos);
	double dMaxX = max (Pt1.dXPos, Pt2.dXPos);
	double dMaxY = max (Pt1.dYPos, Pt2.dYPos);
	
	Point bottomLeftPt (dMinX,dMinY);
	Point topRightPt (dMaxX,dMaxY); 

	//	set the bottom left and top right
	m_gdEntExtents.setBottomLeft (bottomLeftPt);
	m_gdEntExtents.setTopRight (topRightPt);

}
//==================================================================================================
string SpecializedCircleEntity::getDesc()
{
	//	set the data to string
	string strCircle ("CIRCLE");
	return strCircle;
}
//==================================================================================================
void SpecializedCircleEntity::offsetBy(double dX, double dY)
{
	m_ptCenter.dXPos += dX;
	m_ptCenter.dYPos += dY;
}
//==================================================================================================
void SpecializedCircleEntity::EntDraw (BOOL bDraw)
{
	// check for the entity's visibility condition
	if (m_bVisible == FALSE)
		return;

	CGenericDisplayView *pView = NULL;
	pView = m_pGenericDisplayCtrl->getUGDView();

	// Get Scale factor
	double dScaleFactor = m_pGenericDisplayCtrl->getScaleFactor();

	CDC* pDC = pView->m_ImgEdit.GetDC();

	CRect wndRect;
	pView->GetWindowRect(wndRect);
	int iWidth = wndRect.right - wndRect.left;
	int iHeight = wndRect.bottom - wndRect.top;

	double dRadius = (m_dPercentRadius/100.0) * (iWidth + iHeight)/2;

	m_dCurrentRadius = dRadius;


	double dIncludedAngInRad = 0.0, dIncludedAngInDeg = 0.0, dIncAng = 0.0; 
	int iPoints; 

	//	caluculate the increment angle
	dIncAng	= incrementAngle(dRadius);

	//	convert included angle to radians
	dIncludedAngInRad = 360 * MathVars::PI/180;

	//	calulate no. of points and add 1
	// TESTTHIS cast to int
	iPoints = (int)(dIncludedAngInRad/dIncAng) + 1;

	//	calculate increment angle with no. of points obtained
	dIncAng	= dIncludedAngInRad/iPoints;

	double dCx = m_ptCenter.dXPos;
	double dCy = m_ptCenter.dYPos;

	pView->ImageInWorldToViewCoordinates (&dCx, &dCy, m_ulPageNumber);

	//	compute last point 
	double dXn = dCx + dRadius;
	double dYn = dCy;

	int iCount = 0;
	double dX1 = 0, dY1 = 0;


	CPen EntPen;
	if (m_bSelected == TRUE)
	{
		EntPen.CreatePen (PS_DASH, 1, m_color);
		pDC->SetBkMode (TRANSPARENT);
	}
	else
		EntPen.CreatePen (PS_SOLID, 1, m_color);

	// Set Draw mode
	pDC->SetROP2(R2_COPYPEN);

	// Select pen object
	pDC->SelectObject(&EntPen);

	double dTeta, dX0, dY0;

	for(dTeta = 0, iCount =0; iCount < iPoints; dTeta += dIncAng,iCount++)
	{
		dX0 = dCx + (dRadius*cos(dTeta));
		dY0 = dCy + (dRadius*sin(dTeta));
		if(iCount == 0)
		{
			// TESTTHIS cast to int
			pDC->MoveTo((int)dX0, (int)dY0);
		}
		else
		{
			// TESTTHIS cast to int
			pDC->LineTo((int)dX0, (int)dY0);
		}
	}

	// TESTTHIS cast to int
	pDC->LineTo((int)dXn, (int)dYn);

	return;
}
//==================================================================================================
int SpecializedCircleEntity::getEntDataString(CString &zDataString)
{
	//	Get View for drawing the line entity
	CGenericDisplayView *pView = NULL;
	pView = m_pGenericDisplayCtrl->getUGDView();

	// TESTTHIS cast to int
	int iPercentRadius = (int)m_dPercentRadius;

	//	set the data to the string
	zDataString.Format("%.4f:%.4f,%d", (m_ptCenter.dXPos)*m_pGenericDisplayCtrl->getXMultFactor(), 
						(m_ptCenter.dYPos)*m_pGenericDisplayCtrl->getYMultFactor(),iPercentRadius);

	return zDataString.GetLength();
}
//==================================================================================================
double SpecializedCircleEntity::getDistanceToEntity(double dX, double dY)
{
	double dDistance;
	//to store the radius value
	double dCurrentRadius=0;
	double yTemp = 0;
	
	double pointX, pointY, centreX, centreY;

	pointX = dX;
	pointY = dY;

	centreX = m_ptCenter.dXPos;
	centreY = m_ptCenter.dYPos;
	
	m_pGenericDisplayCtrl->getUGDView ()->ImageInWorldToViewCoordinates (&pointX, &pointY, m_ulPageNumber);
	m_pGenericDisplayCtrl->getUGDView ()->ImageInWorldToViewCoordinates (&centreX, &centreY, m_ulPageNumber);


	// TESTTHIS decl from int to doubles
	double dDx = pointX - centreX;
	double dDy = pointY - centreY;

	//int dDx = dX - m_ptCenter.dXPos;
	//int dDy = dY - m_ptCenter.dYPos;

	//calculate distance from given point to center
	dDistance = sqrt( dDx * dDx + dDy * dDy);

	dCurrentRadius = m_dCurrentRadius;
	//converting the radius value from view to the image pixel coordinate
	//m_pGenericDisplayCtrl->getUGDView ()->ViewToImageInWorldCoordinates (&dCurrentRadius, &yTemp);

	// if the point(dx,dy) is out side the cirle, subtract the
	// radius of the cirle from the distance. The assuption 
	// made here is, distance is always calculated to the circufrence
	// of the circle and not to the radius of the circle

	if (dDistance > dCurrentRadius)

	{
		//subtract radius from distance
		dDistance -= dCurrentRadius;
	}
	else
	{
		// if the point is inside the circle, distance is radius - distance
		dDistance = dCurrentRadius - dDistance;
	}

	dDistance /= m_pGenericDisplayCtrl->getUGDView ()->getZoomFactor()/100;
	return dDistance;

}
//==================================================================================================