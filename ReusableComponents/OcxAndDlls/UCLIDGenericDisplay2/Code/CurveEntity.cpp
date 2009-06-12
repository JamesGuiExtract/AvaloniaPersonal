//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	CurveEntity.cpp
//
// PURPOSE:	This is an implementation file for CurveEntity() class.
//			Where the CurveEntity() class has been derived from GenericEntity() class.
//			The code written in this file makes it possible to implement the various
//			application methods in the user interface.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================
// CurveEntity.cpp : implementation file
//
#include "stdafx.h"
#include "CurveEntity.h"
#include "GenericDisplayView.h"
#include "GenericDisplayFrame.h"
#include "UCLIDException.h"

#include <cmath>

//////////////////////////////////////////////////////////////////////////////
//	CurveEntity message handlers

//==========================================================================================
CurveEntity::CurveEntity(unsigned long id)
	: GenericEntity(id)
{
	//	set curve entity default properties
	m_dRadius		= 1.0;
	m_dEndAngle		= 0.0;
	m_dStartAngle	= 360.0;
	m_stPt.dXPos	= 0.0;
	m_stPt.dYPos	= 0.0;
	m_endPt.dXPos	= 0.0;
	m_endPt.dYPos   = 0.0;

//	m_pGenericDisplayCtrl = NULL;
}
//==========================================================================================
CurveEntity::CurveEntity(unsigned long id, Point center, double dRadius, double dStartAng, 
						 double dEndAng)
	: GenericEntity(id)
{
	//	set Curve Entity parameters
	m_center		= center;
	m_dRadius		= dRadius;
	m_dEndAngle		= dEndAng;
	m_dStartAngle	= dStartAng;
}
//==========================================================================================
CurveEntity::~CurveEntity()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16447");
}
//==========================================================================================
void CurveEntity::getExtents(GDRectangle& rBoundingRectangle)
{
	Point bottomLeft, topRt;

	//	get the bottom left and top right points
	bottomLeft = m_gdEntExtents.getBottomLeft();
	topRt = m_gdEntExtents.getTopRight();

	//Set the bottomleft and topright points to rBoundingRectangle
	rBoundingRectangle.setBottomLeft(bottomLeft);
	rBoundingRectangle.setTopRight(topRt);
}
//==========================================================================================
void CurveEntity::ComputeEntExtents ()
{
	Point bottomLeftPt, topRightPt;

	double dStartAng = m_dStartAngle;
	double dEndAng = m_dEndAngle;

	if (dStartAng <0.0 || dEndAng <0.0)
	{
		if (dStartAng <0.0) dStartAng += 360;
		if (dEndAng <0.0) dEndAng += 360;
	}

	// get the start point of the curve
	double dStPointX = m_center.dXPos + (m_dRadius * cos(dStartAng*MathVars::PI/180));
	double dStPointY = m_center.dYPos + (m_dRadius * sin(dStartAng*MathVars::PI/180));

	// get the end point of the curve
	double dEndPointX = m_center.dXPos + (m_dRadius * cos(dEndAng*MathVars::PI/180));
	double dEndPointY = m_center.dYPos + (m_dRadius * sin(dEndAng*MathVars::PI/180));

	//	set the dX position
	bottomLeftPt.dXPos = min (dStPointX, dEndPointX); 
	topRightPt.dXPos = max (dStPointX, dEndPointX); 

	//	set the dY position
	bottomLeftPt.dYPos = min (dStPointY, dEndPointY);
	topRightPt.dYPos = max (dStPointY, dEndPointY);

	// at this point the bottomLeft and topRight are assigned with the minimum
	// and maximum values among the two end points. But if the curve passes through
	// angle positions 90,180, 270 and 360 the minimum and maximum of X and Y extents 
	// will change. So check for those positions and update the bottomLeft and topRight points

	// if the start angle is greater than the end angle. For example start angle is 270 and end angle is 30
	// curve is to be drawn from 270 degrees to 30 degrees ( curve direction is always CCW).
	if (dStartAng > dEndAng)
	{
		if (dStartAng >= 90 && dEndAng >= 90)
		{
			// here topRight.Y will be equal to center + radius
			topRightPt.dYPos = m_center.dYPos + m_dRadius;
		}
		if (dStartAng >= 180 && dEndAng >= 180)
		{
			// here leftBottom.X will be equal to center - radius
			bottomLeftPt.dXPos = m_center.dXPos - m_dRadius;
		}
		if (dStartAng >= 270 && dEndAng >= 270)
		{
			// here bottomLeft.Y will be equal to center - radius
			bottomLeftPt.dYPos = m_center.dYPos - m_dRadius;
		}
		if (dStartAng <= 360 && dEndAng >= 0 || dEndAng == 360)
		{
			// here topRight.X will be equal to center + radius
			topRightPt.dXPos = m_center.dXPos + m_dRadius;
		}
	}
	else
	{
		if (dStartAng <= 90 && dEndAng >= 90)
		{
			// here topRight.Y will be equal to center + radius
			topRightPt.dYPos = m_center.dYPos + m_dRadius;
		}
		if (dStartAng <= 180 && dEndAng >= 180)
		{
			// here leftBottom.X will be equal to center - radius
			bottomLeftPt.dXPos = m_center.dXPos - m_dRadius;
		}
		if (dStartAng <= 270 && dEndAng >= 270)
		{
			// here bottomLeft.Y will be equal to center - radius
			bottomLeftPt.dYPos = m_center.dYPos - m_dRadius;
		}
		if (dStartAng >= 0 && dEndAng == 0 || dEndAng == 360)
		{
			// here topRight.X will be equal to center + radius
			topRightPt.dXPos = m_center.dXPos + m_dRadius;
		}
	}



	//	set the bottom left and top right
	m_gdEntExtents.setBottomLeft (bottomLeftPt);
	m_gdEntExtents.setTopRight (topRightPt);
}
//==========================================================================================
string CurveEntity::getDesc()
{
	//	set the string
	string strCurve ("CURVE");
	return strCurve;
}
//==========================================================================================
void CurveEntity::offsetBy(double dX, double dY)
{
	//	set the offset
	m_center.dXPos += dX;
	m_center.dYPos += dY;

}
//==========================================================================================
void CurveEntity::EntDraw (BOOL bDraw)
{
	// check for the entity's visibility condition
	if (m_bVisible == FALSE)
		return;

	double dIncludedAngInRad = 0.0, dIncludedAngInDeg = 0.0, dIncAng = 0.0; 
	int iPoints; 

	// caluculate the increment angle
	dIncAng	= incrementAngle(m_dRadius);

	BOOL bReverseAngle = FALSE;

	// if start angle is greater than end angle. For example start angle is 270 and 
	// end angle is 30. It is a reverse angle
	if(m_dStartAngle>m_dEndAngle)
	{
		dIncludedAngInDeg = 360-m_dStartAngle+m_dEndAngle;
		bReverseAngle = TRUE;
	}
	else
	{
		// calculate included angle in degrees
		dIncludedAngInDeg = m_dEndAngle-m_dStartAngle;
		
		// if it is negative then add 360 degrees
		if(dIncludedAngInDeg < 0)
			dIncludedAngInDeg += 360;
	}

	// convert included angle to radians
	dIncludedAngInRad = dIncludedAngInDeg * MathVars::PI/180;

	// calulate no. of points and add 1
	// TESTTHIS cast to int
	iPoints = (int)(dIncludedAngInRad / dIncAng) + 1;

	// calculate increment angle with no. of points obtained
	// TESTTHIS cast to int
	dIncAng	= (int) dIncludedAngInRad/iPoints;

	// Convert start angle and end angle, which are in degrees to radians
	double dStartAngInRad = m_dStartAngle * MathVars::PI/180;
	double dEndAngInRad = m_dEndAngle * MathVars::PI/180;

	// Get View for drawing the line entity
	CGenericDisplayView *pView = NULL;
	// Get Scale factor
	double dScaleFactor = m_pGenericDisplayCtrl->getScaleFactor();

	pView = m_pGenericDisplayCtrl->getUGDView();

	// Get Device context pointer
	CDC* pDC = pView->m_ImgEdit.GetDC();
	
	//Get the center point and convert it into view co-ordinates
	// TESTTHIS cast to int
	int iCx = (int)m_center.dXPos;
	int iCy = (int)m_center.dYPos;

	int iCount = 0;
	double dX0=0.0, dY0=0.0, dX1=0.0, dY1=0.0;
	
	// create pen
	CPen EntPen;
	if (m_bSelected == TRUE)
	{
		EntPen.CreatePen (PS_DASH, 1, m_color);
		pDC->SetBkMode (TRANSPARENT);
	}
	else
		EntPen.CreatePen (PS_SOLID, 1, m_color);
	
	// Select pen object
	pDC->SelectObject(&EntPen);
	
	//pDC->SetROP2(R2_NOTMERGEPEN);
	pDC->SetROP2(R2_COPYPEN);
	
	// calculate the points that make up the curve
	double dTeta, dXn, dYn;
	
	if(bReverseAngle)
	{
		// if the start angle is greater than the end angle, the curve direction being CCW, curve passes
		// through 0 or 360 degree point. So to draw the curve from the start point to 0 or 360 degrees point
		// dTeta (increment angle in radians) value need to be incremented ( upto 2*PI value). At zero degrees
		// point dTeta value need to be reset to 0.0 and till the end point curve is to be drawn by incrementing
		// the value of dTeta.
		dTeta = dStartAngInRad;
		while(dTeta<= 2*MathVars::PI)
		{
			dX0 = m_center.dXPos + (m_dRadius*cos(dTeta));
			dY0 = m_center.dYPos + (m_dRadius*sin(dTeta));
			
			pView->ImageInWorldToViewCoordinates (&dX0, &dY0, m_ulPageNumber);
			
			//move to the first point and then draw lines to each and every successive point
			if(iCount == 0)
			{
				dXn = dX0;
				dYn = dY0;
				
				// update the class variable for the start point of the curve here
				m_stPt.dXPos	= dX0;
				m_stPt.dYPos	= dY0;
				pView->ViewToImageInWorldCoordinates (&m_stPt.dXPos, &m_stPt.dYPos, m_ulPageNumber);
				
				// TESTTHIS cast to int
				pDC->MoveTo((int)dX0, (int)dY0);
			}
			else
			{
				// TESTTHIS cast to int
				pDC->LineTo((int)dX0, (int)dY0);
				
				//update the class variable for the end point of the curve here
				m_endPt.dXPos = dX0;
				m_endPt.dYPos = dY0;
				pView->ViewToImageInWorldCoordinates (&m_endPt.dXPos, &m_endPt.dYPos, m_ulPageNumber);
			}
			dTeta += dIncAng;
			iCount++;
		}
		
		dTeta = 0.0;
		while (dTeta <= dEndAngInRad)
		{
			dX0 = m_center.dXPos + (m_dRadius*cos(dTeta));
			dY0 = m_center.dYPos + (m_dRadius*sin(dTeta));
			
			pView->ImageInWorldToViewCoordinates (&dX0, &dY0, m_ulPageNumber);
			
			// TESTTHIS cast to int
			pDC->LineTo((int)dX0, (int)dY0);
			
			//update the class variable for the end point of the curve here
			m_endPt.dXPos = dX0;
			m_endPt.dYPos = dY0;
			pView->ViewToImageInWorldCoordinates (&m_endPt.dXPos, &m_endPt.dYPos, m_ulPageNumber);
			
			if((dEndAngInRad == 0) && (dStartAngInRad == 2*MathVars::PI))
			{
				// TESTTHIS cast to int
				pDC->LineTo((int)dXn, (int)dYn);
				return ;
			}
			
			dTeta += dIncAng;
			iCount++;
			
		}
		
	}
	else
	{
		for(dTeta = dStartAngInRad, iCount =0; dTeta <= dEndAngInRad; dTeta += dIncAng,iCount++)
		{
			dX0 = m_center.dXPos + (m_dRadius*cos(dTeta));
			dY0 = m_center.dYPos + (m_dRadius*sin(dTeta));
			
			pView->ImageInWorldToViewCoordinates (&dX0, &dY0, m_ulPageNumber);
			
			//move to the first point and then draw lines to each and every successive point
			if(iCount == 0)
			{
				dXn = dX0;
				dYn = dY0;
				
				// update the class variable for the start point of the curve here
				m_stPt.dXPos	= dX0;
				m_stPt.dYPos	= dY0;
				pView->ViewToImageInWorldCoordinates (&m_stPt.dXPos, &m_stPt.dYPos, m_ulPageNumber);
				
				// TESTTHIS cast to int
				pDC->MoveTo((int)dX0, (int)dY0);
			}
			else
			{
				// TESTTHIS cast to int
				pDC->LineTo((int)dX0, (int)dY0);
				
				//update the class variable for the end point of the curve here
				m_endPt.dXPos = dX0;
				m_endPt.dYPos = dY0;
				pView->ViewToImageInWorldCoordinates (&m_endPt.dXPos, &m_endPt.dYPos, m_ulPageNumber);
			}
		}
		if((dStartAngInRad == 0) && (dEndAngInRad == 2*MathVars::PI))
		{
			// TESTTHIS cast to int
			pDC->LineTo((int)dXn, (int)dYn);
			return ;
		}
	}
}
//==========================================================================================
int CurveEntity::getEntDataString(CString &zDataString)
{
	// Get View for drawing the line entity
	CGenericDisplayView *pView = NULL;
	pView =  m_pGenericDisplayCtrl->getUGDView();

	//	set the data to the string
	zDataString.Format("%.4f:%.4f,%.4f,%.4f,%.4f", 
								(m_center.dXPos)*m_pGenericDisplayCtrl->getXMultFactor(), (m_center.dYPos)*m_pGenericDisplayCtrl->getYMultFactor(),
								m_dRadius*((m_pGenericDisplayCtrl->getXMultFactor() + m_pGenericDisplayCtrl->getXMultFactor())/2.0), m_dStartAngle, m_dEndAngle);

	//	return the length of the string
	return zDataString.GetLength();
}
//==========================================================================================
double CurveEntity::getDistanceToEntity(double dX, double dY)
{
	double dDistance;

	//calculate the distance from the given pt to center
	double dDx = dX - m_center.dXPos;
	double dDy = dY - m_center.dYPos;

	// calculate the angle of a point and convert it into degrees
	double dAngOfPt = atan2(dDy, dDx);
	double dAngOfPtInDeg = dAngOfPt * 180 / MathVars::PI;

	//calculate min and max angles among those two start and end angles
	double dMinAng = min(m_dStartAngle, m_dEndAngle);
	double dMaxAng = max(m_dStartAngle, m_dEndAngle);

	// if the minimum angle and maximum angle are positive and the angle
	// of the point is negative ( that means it is greater than 180), make the 
	// angle of the point positive ( in the range of 0 to 360). It is required for 
	// checking whether the angle of point lies in the range
	if (dMinAng >= 0.0 && dMaxAng >= 0.0 && dAngOfPtInDeg <0)
	{
		dAngOfPtInDeg = 360 + dAngOfPtInDeg;
	}

	// calculate distance from point to center
	dDistance = sqrt( dDx * dDx + dDy * dDy);

	// check whether the angle of point lies in that range
	if((dAngOfPtInDeg >= dMinAng) && (dAngOfPtInDeg <= dMaxAng) && (dDistance > m_dRadius))
	{
		//subtract radius from distance
		dDistance -= m_dRadius;
	}
	else if((dAngOfPtInDeg >= dMinAng) && (dAngOfPtInDeg <= dMaxAng) && (dDistance < m_dRadius))
	{
		// if the point is within the angle range but it is between the centre of the curve and
		// the curve entity the distance will be less than radius and equals to distance between
		// curve entity and the point.
		dDistance = m_dRadius - dDistance;
	}
	else
	{
		// the point is outside the curve angle range. So the distance will be the minimum
		// among distance between point and start point of the curve and point and the end point
		// point of the curve.

		if (dMinAng <0.0 || dMaxAng <0.0)
		{
			if (dMinAng <0.0) dMinAng += 360;
			if (dMaxAng <0.0) dMaxAng += 360;
		}

		// get the start point of the curve
		double dStPointX = m_center.dXPos + (m_dRadius * cos(dMinAng*MathVars::PI/180));
		double dStPointY = m_center.dYPos + (m_dRadius * sin(dMinAng*MathVars::PI/180));

		// get the end point of the curve
		double dEndPointX = m_center.dXPos + (m_dRadius * cos(dMaxAng*MathVars::PI/180));
		double dEndPointY = m_center.dYPos + (m_dRadius * sin(dMaxAng*MathVars::PI/180));

		double dStPtDistance = sqrt (pow((dStPointX - dX),2)  + pow ((dStPointY - dY), 2));
		double dEndPtDistance = sqrt(pow((dEndPointX - dX),2) + pow ((dEndPointY - dY), 2));

		// assign the distance which is minimum		
		dDistance = min (dStPtDistance, dEndPtDistance);
	}

	return dDistance;
}
//==========================================================================================
