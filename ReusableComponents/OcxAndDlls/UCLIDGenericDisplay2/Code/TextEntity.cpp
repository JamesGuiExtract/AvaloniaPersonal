//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	TextEntity.cpp
//
// PURPOSE:	This is an implementation file for TextEntity() class.
//			Where the TextEntity() class has been derived from GenericEntity() class.
//			The code written in this file makes it possible to implement the various
//			application methods in the user interface.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================
// TextEntity.cpp : implementation file
//
#include "stdafx.h"
#include "TextEntity.h"
#include "GenericDisplayView.h"
#include "GenericDisplayFrame.h"
#include "GenericDisplayDocument.h"
#include "UCLIDException.h"

#include <cmath>
#include <string>

//////////////////////////////////////////////////////////////////////////////
//	TextEntity message handlers
//==================================================================================================
TextEntity::TextEntity(unsigned long id)
	: GenericEntity(id)
{
}
//==================================================================================================
TextEntity::TextEntity(unsigned long id, Point insertionPoint, string strText, 
					   unsigned char ucAlignment, double RotationAngInDeg, 
					   double dTextHeight, string strFontName)
	: GenericEntity(id)
{
	//	set the parameters to the text entity
	m_insertionPoint = insertionPoint;
	m_strText = strText;
	m_ucAlignment = ucAlignment;
	m_dRotationAngleInDegees = RotationAngInDeg;
	m_dTextHeight = dTextHeight;
	m_strFontName = strFontName;
}
//==================================================================================================
TextEntity::~TextEntity()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16458");
}
//==================================================================================================
void TextEntity::getExtents(GDRectangle& rBoundingRectangle)
{
	Point bottomLeft, topRt;

	bottomLeft = m_gdEntExtents.getBottomLeft();
	topRt = m_gdEntExtents.getTopRight();

	//Set the bottomleft and topright points to rBoundingRectangle
	rBoundingRectangle.setBottomLeft(bottomLeft);
	rBoundingRectangle.setTopRight(topRt);
}
//==================================================================================================
string TextEntity::getDesc()
{
	//	set the data to the string
	string strTxt ("TEXT");
	return strTxt;
}
//==================================================================================================
void TextEntity::offsetBy(double dX, double dY)
{
	m_insertionPoint.dXPos += dX;
	m_insertionPoint.dYPos += dY;
}
//==================================================================================================
void TextEntity::EntDraw (BOOL bDraw)
{
	//check for the visibility of an entity
	if (m_bVisible == FALSE)
		return;

	//check alignment against values from 1 to 9
	if((m_ucAlignment < 1) || (m_ucAlignment > 9))
	{
		AfxThrowOleDispatchException (0, "ELI90016: Alignment value should be between 1 and 9.");
	}
	
	CGenericDisplayView *pView = NULL;
	pView = m_pGenericDisplayCtrl->getUGDView();

	// Get Scale factor
	double dScaleFactor = m_pGenericDisplayCtrl->getScaleFactor();

	// Get Device context pointer from Image Control
	CDC* pDC = pView->m_ImgEdit.GetDC();

	// Create a pen object
	CPen EntPen;

	if (m_bSelected == TRUE)
		EntPen.CreatePen (PS_DASH, 1, m_color);
	else
		EntPen.CreatePen (PS_SOLID, 1, m_color);

	pDC->SetTextColor(m_color);

	// Select pen object
	pDC->SelectObject(&EntPen);

	// Set Draw mode
	pDC->SetROP2(R2_NOTMERGEPEN);

	if(bDraw)
		pDC->SetROP2(R2_COPYPEN);
	else
	{
		pDC->SetROP2(R2_NOTXORPEN);
	}

	// Get the base rotation from control
	double dBaseAng = m_pGenericDisplayCtrl->getBaseRotation(m_ulPageNumber);

	// Add the base rotation angle to the TextEntity rotation to get the total angle
	double dTextAngle = dBaseAng + m_dRotationAngleInDegees;

	// Create font with the supplied details
	CFont font;
	LOGFONT lf;
	memset(&lf, 0, sizeof(LOGFONT)); 

	// Check for the validity of Text height
	if(m_dTextHeight <= 0)
	{
		AfxThrowOleDispatchException (0, "ELI90017: Text height should be greater than zero.");
	}
	else	           // request a 12-pixel-height font
		lf.lfHeight = (int)(m_dTextHeight * (pView->getZoomFactor ()/100.0));

	// angle, in tenths of degrees, btw each character's base line and 
	// the x-axis of the device
	lf.lfOrientation = (int)(dTextAngle*10);
	
	// lfEscapement and lfOrientation should have the same value
	lf.lfEscapement = (int)(dTextAngle*10);

	// font name should be the name of an installed font
	// check whether the font name is in list of installed fonts or not
	lstrcpyn(lf.lfFaceName, m_strFontName.c_str(), LF_FACESIZE);
	VERIFY(font.CreateFontIndirect(&lf)); 

	
	// Set transperant mode so that the image below the text is visible
	pDC->SetBkMode(TRANSPARENT);

	//select the font that is created
	pDC->SelectObject(&font);

	CString strText(m_strText.c_str());
	// Get Text extents
	CSize txtSize = pDC->GetOutputTextExtent(strText);

	int iX = (int)m_insertionPoint.dXPos;
	int iY = (int)m_insertionPoint.dYPos;

	// Convert image co-ordinates to view
	pView->ImageInWorldToViewCoordinates (&iX, &iY, m_ulPageNumber);

	int iDist = 0;
	double dDiagAng = 0.0;

	// Compute the angle and the distance from the bottom left corner point to the click point
	// based on the alignment option

	switch (m_ucAlignment)
	{
		case 1:	// Left, Bottom
		default:
			break;
		case 2:	// Middle, Bottom
			iDist = (int)(txtSize.cx/2.0);
			dDiagAng = 0.0;
			break;
		case 3:	//Right, Bottom
			iDist = txtSize.cx;
			dDiagAng = 0.0;
			break;
		case 4: // Left, Middle
			iDist = (int)(txtSize.cy/2.0);
			dDiagAng = MathVars::PI/2.0;
			break;
		case 5:	// Middle, Middle
			// TESTTHIS casts to long double
			iDist = (int)(sqrt((long double)txtSize.cx * (long double)txtSize.cx + (long double)txtSize.cy * (long double)txtSize.cy) / 2.0);
			dDiagAng = atan2((long double)txtSize.cy, (long double)txtSize.cx);
			break;
		case 6: // Right, Middle
			iDist = (int)(sqrt((long double)txtSize.cx * (long double)txtSize.cx + (long double)txtSize.cy / 2.0 * (long double)txtSize.cy / 2.0));
			dDiagAng = atan2((long double)txtSize.cy / 2.0, (long double)txtSize.cx);
			break;
		case 7:	// Left, Top
			iDist = (int)txtSize.cy;
			dDiagAng = MathVars::PI / 2.0;
			break;
		case 8:	// Middle, Top
			iDist = (int)(sqrt((long double)txtSize.cx / 2.0 * (long double)txtSize.cx / 2.0 + (long double)txtSize.cy * (long double)txtSize.cy));
			dDiagAng = atan2((long double)txtSize.cy, (long double)txtSize.cx / 2.0);
			break;
		case 9:	// Right, Top
			iDist = (int)(sqrt ((long double)txtSize.cx * (long double)txtSize.cx + (long double)txtSize.cy * (long double)txtSize.cy));
			dDiagAng = atan2 ((long double)txtSize.cy, (long double)txtSize.cx);
			break;
	}

	// caluculate the bottomleft co-ordinate with respective to the alignment
	int iInsX, iInsY;
	double dAng = dTextAngle * MathVars::PI/180.0;
	dAng += dDiagAng;

	// Add the base rotation to text entity rotation
	double dSinVal = sin (dAng);
	double dCosVal = cos (dAng);

	if (fabs (dSinVal) < 0.000000001)
		dSinVal = 0.0;
	if (fabs(dCosVal) < 0.000000001)
		dCosVal = 0.0;

	// TESTTHIS cast to int
	iInsX = iX + (iDist) * (int)dCosVal;
	iInsY = iY - (iDist) * (int)dSinVal;

	// Set Text allignment to the caluculate bottomleft point
	pDC->SetTextAlign(TA_LEFT | TA_BOTTOM);

	// compute the text entity extents
	ComputeEntExtents();

	if(!bDraw)
	{
		Point bottomLeft, topRt;
		bottomLeft = m_gdEntExtents.getBottomLeft();
		topRt = m_gdEntExtents.getTopRight();
		// TESTTHIS cast to int
		CRect rectToRedraw((int)bottomLeft.dXPos, (int)topRt.dYPos, (int)topRt.dXPos, (int)bottomLeft.dYPos);
		pView->InvalidateRect(rectToRedraw);
	}

	// If entity is selected display a dashed box around the text
	if (m_bSelected == TRUE)
	{
		int iX1, iY1, iX2, iY2, iX3, iY3, iX4, iY4;

		// TESTTHIS cast to int
		iX1 = (int)m_textBoxDims.leftBottom.dXPos;
		iY1 = (int)m_textBoxDims.leftBottom.dYPos;
		iX2 = (int)m_textBoxDims.rightBottom.dXPos;
		iY2 = (int)m_textBoxDims.rightBottom.dYPos;
		iX3 = (int)m_textBoxDims.rightTop.dXPos;
		iY3 = (int)m_textBoxDims.rightTop.dYPos;
		iX4 = (int)m_textBoxDims.leftTop.dXPos;
		iY4 = (int)m_textBoxDims.leftTop.dYPos;

		//convert co-ordinates from view to image
		pView->ImageInWorldToViewCoordinates(&iX1, &iY1, m_ulPageNumber);
		pView->ImageInWorldToViewCoordinates(&iX2, &iY2, m_ulPageNumber);
		pView->ImageInWorldToViewCoordinates(&iX3, &iY3, m_ulPageNumber);
		pView->ImageInWorldToViewCoordinates(&iX4, &iY4, m_ulPageNumber);

		pDC->MoveTo(iX1,iY1);
		pDC->LineTo(iX2,iY2);
		pDC->LineTo(iX3,iY3);
		pDC->LineTo(iX4,iY4);
		pDC->LineTo(iX1,iY1);
	}

	// display the text to view
	pDC->TextOut(iInsX, iInsY, strText);
	
	// Done with the font. Delete the font object.
	font.DeleteObject();

	return;
}
//==================================================================================================
void TextEntity::ComputeEntExtents ()
{
	//	check alignment against values from 1 to 9
	if((m_ucAlignment < 1) || (m_ucAlignment > 9))
	{
		AfxThrowOleDispatchException (0, "ELI90018: Alignment value should be between 1 and 9.");
	}

	CGenericDisplayView *pView = NULL;
	pView = m_pGenericDisplayCtrl->getUGDView();

	//	Get Device context pointer from Image Control
	CDC* pDC = pView->m_ImgEdit.GetDC();


	//	Get the base rotation from control
	double dBaseAng = m_pGenericDisplayCtrl->getBaseRotation(m_ulPageNumber);

	//	Add the base rotation angle to the TextEntity rotation to get the total angle
	double dTextAngle = dBaseAng + m_dRotationAngleInDegees;

	//	Create font with the supplied details
	CFont font;
	LOGFONT lf;
	memset(&lf, 0, sizeof(LOGFONT)); 

	//	Check for the validity of Text height
	if(m_dTextHeight <= 0)
	{
		AfxThrowOleDispatchException (0, "ELI90019: Text height should be greater than zero");
	}
	else	           // request a 12-pixel-height font
		lf.lfHeight = (int)(m_dTextHeight * (pView->getZoomFactor ()/100.0));

	// angle, in tenths of degrees, btw each character's base line and 
	// the x-axis of the device
	lf.lfOrientation = (int)(dTextAngle*10);

	// lfEscapement and lfOrientation should have the same value
	lf.lfEscapement = (int)(dTextAngle*10);


	// font name should be the name of an installed font
	// check whether the font name is in list of installed fonts or not
	lstrcpyn(lf.lfFaceName, m_strFontName.c_str(), LF_FACESIZE);
	VERIFY(font.CreateFontIndirect(&lf)); 
	
	//	select the font that is created
	pDC->SelectObject(&font);
	CString strText(m_strText.c_str());
	
	//	Get Text extents
	CSize txtSize = pDC->GetOutputTextExtent(strText);
	double dAngRad = (dTextAngle * MathVars::PI/180) * (-1.0);
	int x1, y1, x2, y2, x3, y3, x4, y4;

	int iX = (int)m_insertionPoint.dXPos;
	int iY = (int)m_insertionPoint.dYPos;

	// convert Image co-ordinates to View co-ordinates
	pView->ImageInWorldToViewCoordinates(&iX, &iY, m_ulPageNumber);

	int iDist = 0;
	double dDiagAng = 0.0;

	// caluculate the bottomleft co-ordinate with respective to the alignment
	switch (m_ucAlignment)
	{
		case 1:	// Left, Bottom
		default:
			break;
		case 2:	// Middle, Bottom
			iDist = (int)(txtSize.cx / 2.0);
			dDiagAng = 0.0;
			break;
		case 3:	// Right, Bottom
			iDist = txtSize.cx;
			dDiagAng = 0.0;
			break;
		case 4: // Left, Middle
			iDist = (int)(txtSize.cy / 2.0);
			dDiagAng = MathVars::PI/2.0;
			break;
		case 5:	// Middle, Middle
			iDist = (int)(sqrt((long double)txtSize.cx * (long double)txtSize.cx + (long double)txtSize.cy * (long double)txtSize.cy) / 2.0);
			dDiagAng = atan2((long double)txtSize.cy, (long double)txtSize.cx);
			break;
		case 6: // Right, Middle
			iDist = (int)(sqrt ((long double)txtSize.cx * (long double)txtSize.cx + (long double)txtSize.cy / 2.0 * (long double)txtSize.cy / 2.0));
			dDiagAng = atan2 ((long double)txtSize.cy / 2.0, (long double)txtSize.cx);
			break;
		case 7:	// Left, Top
			iDist = txtSize.cy;
			dDiagAng = MathVars::PI/2.0;
			break;
		case 8:	// Middle, Top
			iDist = (int)(sqrt((long double)txtSize.cx / 2.0 * (long double)txtSize.cx / 2.0 + (long double)txtSize.cy * (long double)txtSize.cy));
			dDiagAng = atan2((long double)txtSize.cy, (long double)txtSize.cx / 2.0);
			break;
		case 9:	// Right, Top
			iDist = (int)(sqrt((long double)txtSize.cx * (long double)txtSize.cx + (long double)txtSize.cy * (long double)txtSize.cy));
			dDiagAng = atan2((long double)txtSize.cy, (long double)txtSize.cx);
			break;
	}

	double dAng = dTextAngle * MathVars::PI / 180.0;

	dAng += dDiagAng;

	double dSinVal = sin (dAng);
	double dCosVal = cos (dAng);

	if (fabs (dSinVal) < 0.000000001)
		dSinVal = 0.0;
	if (fabs(dCosVal) < 0.000000001)
		dCosVal = 0.0;

	// TESTTHIS cast to int
	int iInsX = iX + (iDist) * (int)dCosVal;
	int iInsY = iY - (iDist) * (int)dSinVal;

	//	calculate all the four points based on the bottomleft point
	x1 = iInsX;
	y1 = iInsY;
	// TESTTHIS cast to int
	x2 = x1 + (int)(txtSize.cx * cos(dAngRad));
	y2 = y1 + (int)(txtSize.cx * sin(dAngRad));

	x3 = x2 + (int)(txtSize.cy * sin(dAngRad));
	y3 = y2 - (int)(txtSize.cy * cos(dAngRad));
	
	x4 = x3 - (int)(txtSize.cx * cos(dAngRad));
	y4 = y3 - (int)(txtSize.cx * sin(dAngRad));

	//	calculate the bounding rectangle from text box
	Point bottomLeft, topRight;

	//	convert co-ordinates from view to image
	pView->ViewToImageInWorldCoordinates(&x1, &y1, m_ulPageNumber);
	pView->ViewToImageInWorldCoordinates(&x2, &y2, m_ulPageNumber);
	pView->ViewToImageInWorldCoordinates(&x3, &y3, m_ulPageNumber);
	pView->ViewToImageInWorldCoordinates(&x4, &y4, m_ulPageNumber);

	// create a region bounding the text entity
	// This is required to test later to know whether clicked point
	// for entity selection is on the text region or not

	TEXTMETRIC tm;
	pDC->GetTextMetrics (&tm);

	int iInternalLeading = tm.tmInternalLeading; 
	int iDescent = tm.tmDescent;

	iInternalLeading /= (int)(pView->getZoomFactor() / 100);
	iDescent /= (int)(pView->getZoomFactor() / 100);

	//	calculate the bottom left point	
	bottomLeft.dXPos = min(min(x1, x2), min(x3, x4));
	bottomLeft.dYPos = min(min(y1, y2), min(y3, y4));

	//	caluculate the top right point 
	topRight.dXPos = max(max(x1, x2), max(x3, x4));
	topRight.dYPos = max(max(y1, y2), max(y3, y4));

	//	set values to textbox structure variable
	m_textBoxDims.leftBottom.dXPos = x1;
	m_textBoxDims.leftBottom.dYPos = y1 + iDescent;
	m_textBoxDims.rightBottom.dXPos = x2;
	m_textBoxDims.rightBottom.dYPos = y2 + iDescent;
	m_textBoxDims.rightTop.dXPos = x3;
	m_textBoxDims.rightTop.dYPos = y3;
	m_textBoxDims.leftTop.dXPos = x4;
	m_textBoxDims.leftTop.dYPos = y4;


	//	calculate center point for text
	m_centerPoint.dXPos = (x1 + x3)/2;
	m_centerPoint.dXPos = (y1 + y3)/2;

	//	Set the bottomleft and topright points to GDRectangle data members
	m_gdEntExtents.setBottomLeft(bottomLeft);

	m_gdEntExtents.setTopRight(topRight);

	//	delete the font object
	font.DeleteObject();

	return;
}
//==================================================================================================
BOOL TextEntity::isPtOnText(int x, int y)
{
	ComputeEntExtents();
	CRgn rgnTextBox;

	CPoint ptVertex[4];

	// TESTTHIS cast to long
	ptVertex[0].x = (long)m_textBoxDims.leftBottom.dXPos;
	ptVertex[0].y = (long)m_textBoxDims.leftBottom.dYPos;
	ptVertex[1].x = (long)m_textBoxDims.rightBottom.dXPos;
	ptVertex[1].y = (long)m_textBoxDims.rightBottom.dYPos;
	ptVertex[2].x = (long)m_textBoxDims.rightTop.dXPos;
	ptVertex[2].y = (long)m_textBoxDims.rightTop.dYPos;
	ptVertex[3].x = (long)m_textBoxDims.leftTop.dXPos;
	ptVertex[3].y = (long)m_textBoxDims.leftTop.dYPos;

		rgnTextBox.CreatePolygonRgn (ptVertex,4,ALTERNATE);
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
//==================================================================================================
int TextEntity::getEntDataString(CString &zDataString)
{
	//	set data to the string
	zDataString.Format("%.4f:%.4f,%s,%d,%.1f,%.2f,%s", 
			(m_insertionPoint.dXPos)*m_pGenericDisplayCtrl->getXMultFactor(), (m_insertionPoint.dYPos)*m_pGenericDisplayCtrl->getYMultFactor(),
			m_strText.c_str(), m_ucAlignment, 
			m_dRotationAngleInDegees, m_dTextHeight*((m_pGenericDisplayCtrl->getXMultFactor() + m_pGenericDisplayCtrl->getYMultFactor())/2.0), 
			m_strFontName.c_str());

	return zDataString.GetLength();
}
//==================================================================================================
double TextEntity::getDistanceToEntity(double dX, double dY)
{
	double dMinDistance, dDistance;
	dMinDistance = 0xFFFFFFFF;
	Point pt[4];
	
	pt[0].dXPos = m_textBoxDims.leftBottom.dXPos;
	pt[0].dYPos = m_textBoxDims.leftBottom.dYPos;
	pt[1].dXPos = m_textBoxDims.rightBottom.dXPos;
	pt[1].dYPos = m_textBoxDims.rightBottom.dYPos;
	pt[2].dXPos = m_textBoxDims.rightTop.dXPos;
	pt[2].dYPos = m_textBoxDims.rightTop.dYPos;
	pt[3].dXPos = m_textBoxDims.leftTop.dXPos;
	pt[3].dYPos = m_textBoxDims.leftTop.dYPos;
	
	int iX = (int)dX;
	int iY = (int)dY;

	//	store min distance in the dMinDistance
	for(int i=0; i<4; i++)
	{
		Point point(dX, dY);

		if(i == 3)
		{
			Point lineStartPt (pt[i].dXPos, pt[i].dYPos);
			Point lineEndPt (pt[0].dXPos, pt[0].dYPos);
			dDistance = getDistanceBetweenPointAndLine (point, lineStartPt, lineEndPt);
			if(dDistance <= dMinDistance)
				dMinDistance = dDistance;
		}
		else
		{
			Point lineStartPt (pt[i].dXPos, pt[i].dYPos);
			Point lineEndPt (pt[i+1].dXPos, pt[i+1].dYPos);
			dDistance = getDistanceBetweenPointAndLine (point, lineStartPt, lineEndPt);
			if(dDistance <= dMinDistance)
				dMinDistance = dDistance;
		}
	}

	return dMinDistance;
}
//==================================================================================================
double TextEntity::distanceToLine(int iX1, int iX2, int iY1, int iY2, int iX, int iY)
{
	// Find the equation of a line 
	// caclulate the slope of a line
	double dDeltaX = abs(iX2 - iX1);
	double dDeltaY = abs(iY2 - iY1);
	
	// calculate A, B and C
	int iA, iB, iC;

	if(iX2 == iX1)	
	{
		iA = 1;
		iB = 0;
		iC = -iX1;
	}
	else
	{
		iA = iY1 - iY2;
		iB = iX2 - iX1;
		iC = (iX1*iY2) - (iX2*iY1);
	}

	double dDistance = abs((double)(iA * iX) + (iB * iY) + iC ) / (double)sqrt((double)((iA * iA) + (iB * iB)));

	return dDistance;
}
//==================================================================================================