//-------------------------------------------------------------------------------------------------
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ZoneHighlightThrd.cpp
//
// PURPOSE:	This is an header file for CZoneHighlightThrd class
//			The code written in this file makes it possible for
//			draw Zone entity interactively to see it in the view.
//          as a rubberband for zone entity
// NOTES:	
//
// AUTHORS:	M.Srnivasa Rao
//
//-------------------------------------------------------------------------------------------------
#include "stdafx.h"
#include "GenericDisplay.h"
#include "ZoneHighlightThrd.h"
#include "GenericDisplayFrame.h"
#include "GenericDisplayView.h"
#include "GenericDisplayCtl.h"
#include "UCLIDException.h"

#include <math.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// CZoneHighlightThrd
//-------------------------------------------------------------------------------------------------
CZoneHighlightThrd::CZoneHighlightThrd()
{
	// set the default color as white
	m_zoneColor = RGB (0,0,0);
	
	// set the default values
	m_lprevEndPtX = 0;
	m_lprevEndPtY = 0;
	m_bHighltHtChanged = FALSE;
	m_bzoneCreationEnabled = FALSE;
	m_bMouseMovedAfterZoneInitialization = FALSE;
}
//-------------------------------------------------------------------------------------------------
CZoneHighlightThrd::~CZoneHighlightThrd()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16460");
}
//-------------------------------------------------------------------------------------------------
// CZoneHighlightThrd message handlers
//-------------------------------------------------------------------------------------------------
void CZoneHighlightThrd::SetStartPoint (int iEX, int iEY)
{
	// store the start point in image coordinates www
	m_lstartX = iEX;
	m_lstartY = iEY;

	CGenericDisplayView* pView = m_pGenericDisplayCtrl->getUGDView();
	if (pView != __nullptr)
	{
		pView->ClientWindowToImagePixelCoordinates(
			(int*)&m_lstartX, (int*)&m_lstartY, m_pGenericDisplayCtrl->getCurrentPageNumber());
	}
}
//-------------------------------------------------------------------------------------------------
void CZoneHighlightThrd::SetEndPoint (int iEX, int iEY)
{
	// store the end point in image coordinates
	m_lendPtX = iEX;
	m_lendPtY = iEY;
	
	CGenericDisplayView* pView = m_pGenericDisplayCtrl->getUGDView();
	if (pView != __nullptr)
	{
		pView->ClientWindowToImagePixelCoordinates(
			(int*)&m_lendPtX, (int*)&m_lendPtY, m_pGenericDisplayCtrl->getCurrentPageNumber());
	}
}
//-------------------------------------------------------------------------------------------------
void CZoneHighlightThrd::GetEndPoint (int& iEX, int& iEY)
{
	iEX = (int) m_lendPtX;
	iEY = (int) m_lendPtY;
}
//-------------------------------------------------------------------------------------------------
void CZoneHighlightThrd::GetStartPoint (int& iEX, int& iEY)
{
	iEX = (int) m_lstartX;
	iEY = (int) m_lstartY;
}
//-------------------------------------------------------------------------------------------------
void CZoneHighlightThrd::SetPrevEndPoint (int iEX, int iEY)
{
	m_lprevEndPtX = iEX;
	m_lprevEndPtY = iEY;
}
//-------------------------------------------------------------------------------------------------
void CZoneHighlightThrd::GetPrevEndPoint (int& iEX, int& iEY)
{
	iEX = (int) m_lprevEndPtX;
	iEY = (int) m_lprevEndPtY;
}
//-------------------------------------------------------------------------------------------------
void CZoneHighlightThrd::SetHighlightHeight (long ht)
{
	m_lzoneHeight = ht;
}
//-------------------------------------------------------------------------------------------------
void CZoneHighlightThrd::CalculateRegion(CRgn& region, double endPtX, double endPtY)
{
	// delete the region if exists
	region.DeleteObject();

	long lzoneHeight;

	if (IsHighltHtChanged())
	{
		lzoneHeight = m_lprevZoneHeight;
		// reset the flag
		notifyHighlightHtChanged(FALSE);
	}
	else
		lzoneHeight = m_lzoneHeight;

	CPoint *pptVertex = NULL;

	pptVertex = getZoneBounds(m_lstartX, m_lstartY, endPtX, endPtY, lzoneHeight);

	// create a region
	region.CreatePolygonRgn (pptVertex, 4, ALTERNATE);
	
}
//-------------------------------------------------------------------------------------------------
void CZoneHighlightThrd::SetHighlightColor(COLORREF color)
{
	m_zoneColor = color;
}
//-------------------------------------------------------------------------------------------------
void CZoneHighlightThrd::Draw()
{
	COLORREF zoneColor = GetZoneHighlightColor();

	int iPrevEndX = 0, iPrevEndY = 0;

	GetPrevEndPoint(iPrevEndX, iPrevEndY);

	// we have to bear one point in mind. Draw() method is being called many a
	// times thorugh out the process of interactive zone creation. So before drawing 
	// the zone entity in highlight mode at new position , earlier highlighting must be
	// erased.
	
	// if the previous end points are not equal to 0
	// means this is not the first time draw() is called 
	// and there exists a previous zone region to erase.
	if(iPrevEndX != 0 && iPrevEndY != 0)
	{
		eraseOldRegion();
	}
	
	// get the device context pointer of the image edit control contained in view
	CDC* pDC = m_pGenView->m_ImgEdit.GetDC();

	//	Create a brush object
	CBrush highltBrush;
	highltBrush.CreateSolidBrush(zoneColor);

	//	Select brush object
	CBrush *pOldBrush = (CBrush*)pDC->SelectObject(&highltBrush);

	//	Set Draw mode
	pDC->SetROP2(R2_XORPEN);
	pDC->SetBkMode(TRANSPARENT);
	
	int iEndX, iEndY;
	GetEndPoint(iEndX, iEndY);

	// calculate the new region 
	CalculateRegion(m_zoneRegion, iEndX, iEndY);

	// fill the region
	pDC->FillRgn(&m_zoneRegion, &highltBrush);

	// release the DC and delete the brush created 
	m_pGenView->m_ImgEdit.ReleaseDC(pDC);
	highltBrush.DeleteObject();

	// save the current end points to use them later for deleting this region
	SetPrevEndPoint(iEndX, iEndY);

	// save the zone highlight height for later use
	m_lprevZoneHeight = m_lzoneHeight;

}
//----------------------------------------------------------------------------------------------
void CZoneHighlightThrd::stop()
{
	// Stop and cancel behave the same
	cancel();
}
//-------------------------------------------------------------------------------------------------
void CZoneHighlightThrd::cancel()
{
	// erase the previous region if it was drawn. The zone will be only drawn
	// when mouse is moved atleast once
	if(m_bMouseMovedAfterZoneInitialization)
		eraseOldRegion();

	// reset the class variables
	m_lstartX = 0;
	m_lstartY = 0;

	// this is very important
	m_lprevEndPtX = 0;
	m_lprevEndPtY = 0;

	m_lendPtX = 0;
	m_lendPtY = 0;
	
	m_bMouseMovedAfterZoneInitialization = false;
	m_bzoneCreationEnabled = false;
}
//-------------------------------------------------------------------------------------------------
void CZoneHighlightThrd::eraseOldRegion()
{
	COLORREF zoneColor = GetZoneHighlightColor();

	int iPrevEndX = 0, iPrevEndY = 0;

	GetPrevEndPoint(iPrevEndX, iPrevEndY);
	// get the device context pointer of the image edit control contained in view
	CDC* pDC = m_pGenView->m_ImgEdit.GetDC();
	//	Create a brush object
	CBrush highltBrush;
	highltBrush.CreateSolidBrush(zoneColor);
	
	//	Select brush object
	CBrush *pOldBrush = (CBrush*)pDC->SelectObject(&highltBrush);
	
	// calculate the previous region to unhighlight
	CalculateRegion(m_zoneRegion, iPrevEndX, iPrevEndY);
	
	//	Set Draw mode
	pDC->SetROP2(R2_XORPEN);
	pDC->SetBkMode(TRANSPARENT);

	// currently m_zoneRegion contains the zone bounds of previous
	// region. So the following call of FillRgn() will simply erase the 
	// previous highlighted region.	
	pDC->FillRgn(&m_zoneRegion, &highltBrush);

	m_pGenView->m_ImgEdit.ReleaseDC(pDC);

	// Select old brush
	pDC->SelectObject(pOldBrush);

	// delete the current brush
	highltBrush.DeleteObject();	
}
//-------------------------------------------------------------------------------------------------
double CZoneHighlightThrd::getZoneWidth()
{
	// return the length of the line joining the start and end points of zone
	return sqrt(pow((double)(m_lendPtX - m_lstartX), 2) + pow((double)(m_lendPtY - m_lstartY), 2));
}
//-------------------------------------------------------------------------------------------------
CPoint* CZoneHighlightThrd::getZoneBoundsForThisHeight(long lHeight)
{
	return getZoneBounds(m_lstartX, m_lstartY, m_lendPtX, m_lendPtY, lHeight);
}
//-------------------------------------------------------------------------------------------------
CPoint* CZoneHighlightThrd::getZoneBoundsForThisEndPoint(int x, int y)
{
	CGenericDisplayView* pView = m_pGenericDisplayCtrl->getUGDView();
	if (pView != __nullptr)
	{
		pView->ClientWindowToImagePixelCoordinates(&x, &y, 
			m_pGenericDisplayCtrl->getCurrentPageNumber());
	}

	return getZoneBounds(m_lstartX, m_lstartY, x, y, m_lzoneHeight);
}
//-------------------------------------------------------------------------------------------------
CPoint* CZoneHighlightThrd::getZoneBounds(double x1, double y1, double x2, double y2, long lHeight)
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
	// Zone is symmetric about the line zoining the start and end poins. The 4 vertices of the zone
	// are dependant the angle of the line joining the start and end points of the zone. The actual height of 
	// of the zone is always 1 pixel less than the m_ulTotalZoneHeight value which in image pixels

	//	Get the pointer to the active view
	CGenericDisplayView *pView = NULL;
	pView = m_pGenericDisplayCtrl->getUGDView();

	long eType = m_pGenericDisplayCtrl->getZoneEntityCreationType();
	if (eType == 1) // kAnyAngleZone
	{
		// TESTTHIS cast to long
		long lLoadedImageHt = (long)pView->getLoadedImageHeight();
		
		if (x1 == 0 && x2 == 0 && y1 == 0 && y2 == 0)
		{
			y1 = lLoadedImageHt - lLoadedImageHt/2;
			y2 = lLoadedImageHt - lLoadedImageHt/2;
		}
		else
		{
			y1 = lLoadedImageHt - y1;
			y2 = lLoadedImageHt - y2;
		}
		
		// calculate the angle of the line dy/dx
		double dAngle = atan2(y2 - y1, x2 - x1);

		// update the vertices based on the rotation angle, the highlighting
		// should be in rectangular shape symmetric about the line joining the
		// start point and end point of the zone

		zoneBoundPts[0].x = (long)(x1 - ((lHeight / 2) * sin(dAngle)));
		zoneBoundPts[0].y = (long)(y1 + ((lHeight / 2) * cos(dAngle)));

		zoneBoundPts[1].x = (long)(x2 - ((lHeight / 2) * sin(dAngle)));
		zoneBoundPts[1].y = (long)(y2 + ((lHeight / 2) * cos(dAngle)));

		zoneBoundPts[2].x = (long)(x2 + ((lHeight / 2) * sin(dAngle)));
		zoneBoundPts[2].y = (long)(y2 - ((lHeight / 2) * cos(dAngle)));

		zoneBoundPts[3].x = (long)(x1 + ((lHeight / 2) * sin(dAngle)));
		zoneBoundPts[3].y = (long)(y1 - ((lHeight / 2) * cos(dAngle)));

		zoneBoundPts[0].y = lLoadedImageHt - zoneBoundPts[0].y;
		zoneBoundPts[1].y = lLoadedImageHt - zoneBoundPts[1].y;
		zoneBoundPts[2].y = lLoadedImageHt - zoneBoundPts[2].y;
		zoneBoundPts[3].y = lLoadedImageHt - zoneBoundPts[3].y;

		
	}
	else if(eType == 2) // kRectZone
	{
		zoneBoundPts[0].x = (long)x1;
		zoneBoundPts[0].y = (long)y1;

		zoneBoundPts[1].x = (long)x1;
		zoneBoundPts[1].y = (long)y2;

		zoneBoundPts[2].x = (long)x2;
		zoneBoundPts[2].y = (long)y2;

		zoneBoundPts[3].x = (long)x2;
		zoneBoundPts[3].y = (long)y1;
	}
	else
	{
		memset(zoneBoundPts,0, 4*sizeof(CPoint)); 
	}

	pView->ImagePixelToClientWindowCoordinates((int*)&zoneBoundPts[0].x, (int*)&zoneBoundPts[0].y, 
		m_pGenericDisplayCtrl->getCurrentPageNumber());
	pView->ImagePixelToClientWindowCoordinates((int*)&zoneBoundPts[1].x, (int*)&zoneBoundPts[1].y, 
		m_pGenericDisplayCtrl->getCurrentPageNumber());
	pView->ImagePixelToClientWindowCoordinates((int*)&zoneBoundPts[2].x, (int*)&zoneBoundPts[2].y, 
		m_pGenericDisplayCtrl->getCurrentPageNumber());
	pView->ImagePixelToClientWindowCoordinates((int*)&zoneBoundPts[3].x, (int*)&zoneBoundPts[3].y, 
		m_pGenericDisplayCtrl->getCurrentPageNumber());

	return zoneBoundPts;
}
//-------------------------------------------------------------------------------------------------
