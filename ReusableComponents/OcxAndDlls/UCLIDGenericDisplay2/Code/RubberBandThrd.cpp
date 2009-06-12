//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	RubberBandThrd.cpp
//
// PURPOSE:	This is an implementation file for CRubberBandThrd() class.
//			Where the CRubberBandThrd() class has been derived from CWinThread()
//			which is derived from CCmdTarget() class.
//			The code written in this file makes it possible to implement the Thread
//			creation for rubber banding.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================
// RubberBandThrd.cpp : implementation file
//

#include "stdafx.h"
#include "GenericDisplay.h"
#include "RubberBandThrd.h"
#include "GenericDisplayView.h"
#include "GenericDisplayFrame.h"
#include "GenericDisplayCtl.h"
#include "UCLIDException.h"

#include <math.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CRubberBandThrd

IMPLEMENT_DYNCREATE(CRubberBandThrd, CWinThread)
//==================================================================================================
CRubberBandThrd::CRubberBandThrd()
: m_bEnableRubberBand(FALSE),
  m_eRubberbandType(1),
  m_bStartThread(FALSE),
  m_bSetParameters(FALSE),
  m_bIsRubberbandDrawn(false),
  m_pGenericDisplayCtrl(NULL)
{
	m_Brush.CreatePen(PS_SOLID, 0, 1L);
}
//==================================================================================================
CRubberBandThrd::~CRubberBandThrd()
{
	try
	{
		::DeleteObject((HPEN)m_Brush);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16456");
}
//==================================================================================================
BOOL CRubberBandThrd::InitInstance()
{
	//	set default values
	m_bEnableRubberBand		=	FALSE;
	m_eRubberbandType		=	1;
	m_bStartThread			=	FALSE;
	m_bSetParameters		=	FALSE;

	return TRUE;
}
//==================================================================================================
int CRubberBandThrd::ExitInstance()
{
	return CWinThread::ExitInstance();
}
//==================================================================================================
BEGIN_MESSAGE_MAP(CRubberBandThrd, CWinThread)
	//{{AFX_MSG_MAP(CRubberBandThrd)
		// NOTE - the ClassWizard will add and remove mapping macros here.
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
//==================================================================================================
/////////////////////////////////////////////////////////////////////////////
// CRubberBandThrd message handlers
//==================================================================================================	
void CRubberBandThrd::setStartPoint2(int nX, int nY)
{
	// nX and nY are in client window pixels

	// if current rubberband type is polygon
	if (m_bEnableRubberBand && m_eRubberbandType == 3)
	{
		// convert the point to real world coordinates
		m_pGenericDisplayCtrl->convertClientWindowPixelToWorldCoords(nX, nY, 
								&m_dNewStartPoint2X, &m_dNewStartPoint2Y,
								m_pGenericDisplayCtrl->getCurrentPageNumber());
		addLineEntity(m_dNewStartPoint2X, m_dNewStartPoint2Y);
	}
}
//==================================================================================================	
void CRubberBandThrd::Draw (int iEX, int iEY)
{
	if (m_bEnableRubberBand == FALSE)
		return;

	int nX = iEX, nY = iEY;

	// the iEX and iEY are coordinates in client window
	// let's convert it to world first
	m_pGenericDisplayCtrl->convertClientWindowPixelToWorldCoords(nX, nY, &m_dNewEndX, &m_dNewEndY,
			m_pGenericDisplayCtrl->getCurrentPageNumber());

	DrawRubberBanding();
}
//==================================================================================================
void CRubberBandThrd::setThreadParameters (long eRubberbandType, 
										   double dStartX, double dStartY)
{
	if(m_eRubberbandType != eRubberbandType)
		updateRubberBand();

	m_eRubberbandType = eRubberbandType;

	// store the real world coordinates to send with getThreadParameters()
	m_dStartXInRealWorld = dStartX;
	m_dStartYInRealWorld = dStartY;
	m_dStartPoint2X = dStartX;
	m_dStartPoint2Y = dStartY;
	m_dEndPointX = dStartX;
	m_dEndPointY = dStartY;

	removeLineEntities();

	m_bSetParameters = TRUE;
}
//==================================================================================================
void CRubberBandThrd::getThreadParameters (long*& peRBType, double*& pdStartX, double*& pdStartY)
{
	if (m_bSetParameters == FALSE)
	{
		AfxThrowOleDispatchException (0, "ELI90042: Rubber banding parameters are not set.");
	}
	
	// assign the thread parameters to return
	(* peRBType) = m_eRubberbandType;
	(* pdStartX) = m_dStartXInRealWorld;
	(* pdStartY) = m_dStartYInRealWorld;
}
//==================================================================================================
void CRubberBandThrd::enableRubberBand (BOOL bValue)
{
	if(!m_bSetParameters) return;	
	if(m_bEnableRubberBand == bValue) return;

	m_bEnableRubberBand = bValue;

	removeEarlierLines();

	// remove any temp line entities that are used
	// by polygon rubberband for illustration only
	if (!bValue)
	{
		removeLineEntities();
	}
}
//==================================================================================================
bool isEqual(double dX1, double dY1, double dX2, double dY2)
{
	// if two points are at the same coordinates
	if (fabs(dX1-dX2) <= 1E-8 && fabs(dY1-dY2) <= 1E-8) 
	{
		return true;
	}

	return false;
}
//==================================================================================================
void CRubberBandThrd::removeEarlierLines()
{
	// only remove old rubberband if any
	if (m_bIsRubberbandDrawn)
	{
		// Remove the previous Rubber band
		// i.e. draw another line with same length on top of the existing line. 
		// The second line must be perfectly overlay on top of the first line.
		
		int nStartX, nStartY, nPrevStart2X, nPrevStart2Y, nPrevEndX, nPrevEndY;
		convertToClientCoordinates(m_dStartXInRealWorld, m_dStartYInRealWorld, nStartX, nStartY);
		convertToClientCoordinates(m_dStartPoint2X, m_dStartPoint2Y, nPrevStart2X, nPrevStart2Y);
		convertToClientCoordinates(m_dEndPointX, m_dEndPointY, nPrevEndX, nPrevEndY);
		drawRubberbands(nStartX, nStartY, nPrevEndX, nPrevEndY, nPrevStart2X, nPrevStart2Y);
		
		//	if enableRubberbanding is false, then set the EndPoint same as StartPoint.
		if( m_bEnableRubberBand == FALSE )
		{
			m_dEndPointX = m_dStartXInRealWorld;
			m_dEndPointY = m_dStartYInRealWorld;
			m_dStartPoint2X	= m_dStartXInRealWorld;
			m_dStartPoint2Y	= m_dStartYInRealWorld;
		}
		
		m_bIsRubberbandDrawn = false;
	}
}
//==================================================================================================
void CRubberBandThrd::updateRubberBand ()
{
	if(!m_bEnableRubberBand) return;
	
	removeEarlierLines();
}
//==================================================================================================
void  CRubberBandThrd::initThreadParameters()
{
	//	intialize the parameters
	m_bEnableRubberBand		=	FALSE;
	m_eRubberbandType		=	1;
	m_bStartThread			=	FALSE;
	m_bSetParameters		=	FALSE;
}
//==================================================================================================
void CRubberBandThrd::DrawRubberBanding()
{
	// erase existing rubberband lines if any
	removeEarlierLines();

	// Now, draw the new rubberband lines.
	// Replace old end point with current new end point
	m_dEndPointX = m_dNewEndX;
	m_dEndPointY = m_dNewEndY;
	// replace old start point2 with new start point2
	m_dStartPoint2X = m_dNewStartPoint2X;
	m_dStartPoint2Y = m_dNewStartPoint2Y;

	int nStartX, nStartY, nStart2X, nStart2Y, nEndX, nEndY;
	convertToClientCoordinates(m_dStartXInRealWorld, m_dStartYInRealWorld, nStartX, nStartY);
	convertToClientCoordinates(m_dStartPoint2X, m_dStartPoint2Y, nStart2X, nStart2Y);
	convertToClientCoordinates(m_dEndPointX, m_dEndPointY, nEndX, nEndY);

	drawRubberbands(nStartX, nStartY, nEndX, nEndY, nStart2X, nStart2Y);

	m_bIsRubberbandDrawn = true;
}
//==================================================================================================
void CRubberBandThrd::addLineEntity(double dEndX, double dEndY)
{
	static double dStartX=0, dStartY=0;

	if (m_vecEntityIDs.empty())
	{
		dStartX = m_dStartXInRealWorld;
		dStartY = m_dStartYInRealWorld;
	}

	// if start point equals to end point, do not add the line
	if (isEqual(dStartX, dStartY, dEndX,dEndY)) 
	{
		return;
	}

	int nSize = m_vecEntityIDs.size();
	if (nSize > 1)
	{
		m_pGenericDisplayCtrl->deleteEntity(m_vecEntityIDs[nSize-1]);
		// if more than one segment in the vec, remove the last element
		m_vecEntityIDs.pop_back();
	}

	long nID = m_pGenericDisplayCtrl->addLineEntity(dStartX, dStartY, dEndX, dEndY, 
				m_pGenericDisplayCtrl->getCurrentPageNumber());
	// set to magenta, the color that is visible to both white and black background images
	m_pGenericDisplayCtrl->setEntityColor(nID, RGB(255,0,255));
	m_vecEntityIDs.push_back(nID);
	// always add a line from current end point to the start point of the polygon
	// and remove this line there's a new line added to the polygon
	nID = m_pGenericDisplayCtrl->addLineEntity(m_dStartXInRealWorld, m_dStartYInRealWorld, 
						dEndX, dEndY, m_pGenericDisplayCtrl->getCurrentPageNumber());
	m_pGenericDisplayCtrl->setEntityColor(nID, RGB(255,0,255));
	m_vecEntityIDs.push_back(nID);

	dStartX = dEndX;
	dStartY = dEndY;
}
//==================================================================================================
void CRubberBandThrd::convertToClientCoordinates(double dX, double dY, int& nX, int& nY)
{
	// convert to client pixels
	dX	/= m_pGenericDisplayCtrl->getXMultFactor();
	dY	/= m_pGenericDisplayCtrl->getYMultFactor();
	// convert the coordinates from ImageInWorld to view coordinates
	m_pGenView->ImageInWorldToViewCoordinates(&dX, &dY, m_pGenericDisplayCtrl->getCurrentPageNumber());

	nX = (int)dX;
	nY = (int)dY;
}
//==================================================================================================
void CRubberBandThrd::drawRubberbands(int nStartX, int nStartY, int nEndX, int nEndY, 
								int nStart2X, int nStart2Y)
{
	CDC *pDC = m_pGenView->m_ImgEdit.GetDC();

	if (pDC == NULL) return;

	//	Select pen object
	pDC->SelectObject(&m_Brush);

	//	Set Draw mode
	pDC->SetROP2(R2_NOTMERGEPEN);

	// NOTE: CDC draws lines using current view client pixels
	//	Move to the starting point and draw the rubberbanding lines
	pDC->MoveTo(nStartX, nStartY);
	switch (m_eRubberbandType)
	{
	case 1:		// line rubber band
		{
			//draw a line to the end point
			pDC->LineTo(nEndX, nEndY);
		}
		break;
	case 2:		// rectangular rubberband
		{
			pDC->LineTo (nEndX, nStartY);
			pDC->LineTo (nEndX, nEndY);
			pDC->LineTo (nStartX, nEndY);
			pDC->LineTo (nStartX, nStartY);
		}
		break;
	case 3:		// Polygon rubberband
		{
			// if the rubberband line is coincided with the segment line created as 
			// part of the polygon rubberband trace, then skip
			int nTempX, nTempY;
			convertToClientCoordinates(m_dNewStartPoint2X, m_dNewStartPoint2Y, nTempX, nTempY);
			// start point1 to the end point
			if (isEqual(nEndX, nEndY, nTempX, nTempY) && isEqual(nStartX, nStartY, nStart2X, nStart2Y))
			{
				break;
			}

			pDC->LineTo(nEndX, nEndY);
			if (!isEqual(nEndX, nEndY, nTempX, nTempY))
			{
				// and another line is from start point 2 to the end point
				if (!isEqual(nStartX, nStartY, nStart2X, nStart2Y))
				{
					pDC->MoveTo(nStart2X, nStart2Y);
					pDC->LineTo(nEndX, nEndY);
				}
			}
		}
		break;
	}

	// release the acquired DC
	m_pGenView->m_ImgEdit.ReleaseDC(pDC);
}
//==================================================================================================
void CRubberBandThrd::removeLineEntities()
{
	m_bIsRubberbandDrawn = false;

	if (!m_vecEntityIDs.empty())
	{
		for (unsigned int n = 0; n < m_vecEntityIDs.size(); n++)
		{
			m_pGenericDisplayCtrl->deleteEntity(m_vecEntityIDs[n]);
		}

		m_vecEntityIDs.clear();
	}
}
