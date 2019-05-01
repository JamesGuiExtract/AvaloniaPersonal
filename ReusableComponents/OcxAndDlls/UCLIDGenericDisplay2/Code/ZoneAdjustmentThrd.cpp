//-------------------------------------------------------------------------------------------------
//
// COPYRIGHT (c) 2008 EXTRACT SYSTEMS, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ZoneAdjustmentThrd.cpp
//
// PURPOSE:	Handles an interactive mouse event for moving and resizing highlights.
//
// NOTES:	
//
// AUTHORS:	Nathan Figueroa
//
//-------------------------------------------------------------------------------------------------
#include "stdafx.h"
#include "ZoneAdjustmentThrd.h"
#include "ZoneEntity.h"
#include "GenericDisplay.h"
#include "GenericDisplayFrame.h"
#include "GenericDisplayView.h"
#include "GenericDisplayCtl.h"
#include "UCLIDException.h"

#include <cmath>

//-------------------------------------------------------------------------------------------------
// Consts
//-------------------------------------------------------------------------------------------------
const int gnMINIMUM_TRACKING_DIST = 5;

//-------------------------------------------------------------------------------------------------
// Constructors/Destructor
//-------------------------------------------------------------------------------------------------
CZoneAdjustmentThrd::CZoneAdjustmentThrd()
	: m_lStartX(0), m_lStartY(0), m_lEndX(0), m_lEndY(0), m_lOriginalStartX(0), 
	m_lOriginalStartY(0), m_lOriginalEndX(0), m_lOriginalEndY(0), m_lOriginalHeight(0), 
	m_dVectorX(0.0), m_dVectorY(0.0), m_nXTrackingLimit(0), m_nYTrackingLimit(0), 
	m_bZoneAdjustmentEnabled(false), m_bZoneMoveEnabled(false), m_pZoneEntity(NULL),
	m_pGenericDisplayCtrl(NULL), m_pGenericDisplayView(NULL)
{
}
//-------------------------------------------------------------------------------------------------
CZoneAdjustmentThrd::~CZoneAdjustmentThrd()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI23476");
}

//-------------------------------------------------------------------------------------------------
// Methods
//-------------------------------------------------------------------------------------------------
void CZoneAdjustmentThrd::start(ZoneEntity* pZoneEntity, int iEX, int iEY, int iGripHandleId)
{
	if (m_pGenericDisplayView == NULL)
	{
		AfxThrowOleDispatchException(0, 
			"ELI23488: Unable to set start point without generic display view.");
	}
	if (m_pGenericDisplayCtrl == NULL)
	{
		AfxThrowOleDispatchException(0, 
			"ELI23489: Unable to set start point without generic display control.");
	}
	if (pZoneEntity == NULL)
	{
		AfxThrowOleDispatchException(0, "ELI23483: Zone entity cannot be null.");
	}

	// Store the start point in image coordinates
	m_lStartX = iEX;
	m_lStartY = iEY;
	m_pGenericDisplayView->ClientWindowToImagePixelCoordinates(
		(int*)&m_lStartX, (int*)&m_lStartY, m_pGenericDisplayCtrl->getCurrentPageNumber());

	// Restructure the zone entity if necessary to make calculations easier
	m_bZoneMoveEnabled = iGripHandleId < 0;
	if (!m_bZoneMoveEnabled)
	{
		pZoneEntity->makeGripHandleEndPoint(iGripHandleId);
	}

	// Store the zone entity and its original parameters
	m_pZoneEntity = pZoneEntity;
	m_pZoneEntity->getZoneEntParameters(m_lOriginalStartX, m_lOriginalStartY, 
		m_lOriginalEndX, m_lOriginalEndY, m_lOriginalHeight);

	// Check whether this is a move operation
	if (!m_bZoneMoveEnabled)
	{
		// Retrieve the angle of the nearest vector that connects the midpoints of opposing sides
		double dAngle = m_pZoneEntity->getAngle(m_lOriginalStartX, m_lOriginalEndX,
												m_lOriginalStartY, m_lOriginalEndY, false);

		// Compute the components of the unit vector for the angle
		m_dVectorX = cos(dAngle);
		m_dVectorY = sin(dAngle);

		// Determine the coordinates at which the vector connecting the start and end points results
		// in the minimum allowed length of the zone.
		m_nXTrackingLimit = (int)lround(gnMINIMUM_TRACKING_DIST * m_dVectorX);
		m_nYTrackingLimit = (int)lround(gnMINIMUM_TRACKING_DIST * m_dVectorY);
	}

	// Start tracking zone adjustment
	m_bZoneAdjustmentEnabled = true;
}
//-------------------------------------------------------------------------------------------------
void CZoneAdjustmentThrd::update(int iEX, int iEY)
{
	if (m_pGenericDisplayView == NULL)
	{
		AfxThrowOleDispatchException(0, 
			"ELI23486: Unable to set end point without generic display view.");
	}
	if (m_pGenericDisplayCtrl == NULL)
	{
		AfxThrowOleDispatchException(0, 
			"ELI23487: Unable to set end point without generic display control.");
	}
	if (m_pZoneEntity == NULL)
	{
		AfxThrowOleDispatchException(0, "ELI23490: Invalid zone entity.");
	}

	// Store the end point in image coordinates
	m_lEndX = iEX;
	m_lEndY = iEY;
	m_pGenericDisplayView->ClientWindowToImagePixelCoordinates(
			(int*)&m_lEndX, (int*)&m_lEndY, m_pGenericDisplayCtrl->getCurrentPageNumber());

	if (m_bZoneMoveEnabled)
	{
		long lDeltaX = m_lEndX - m_lStartX;
		long lDeltaY = m_lEndY - m_lStartY;

		long lNewX = m_lOriginalStartX + lDeltaX;
		long lNewY = m_lOriginalStartY + lDeltaY;

		Point startPoint = m_pZoneEntity->getStartPointInImageUnits();

		double dOffsetX = lNewX - startPoint.dXPos;
		double dOffsetY = lNewY - startPoint.dYPos;

		m_pZoneEntity->EntDraw(FALSE);
		m_pZoneEntity->offsetBy(dOffsetX, dOffsetY);
	}
	else
	{
		// Construct vector components for a vector from the start point to the specified point.
        int x = m_lEndX - m_lOriginalStartX;
        int y = m_lEndY - m_lOriginalStartY;

		// Compute the dot product of the vectors
		// NOTE: Because the active highlight vector is a unit vector, this is equivalent 
		// to the the scalar projection of the mouse vector onto the active highlight vector.
		double dotProduct = x * m_dVectorX + y * m_dVectorY;

		// Determine the distance in the X and Y directions this projection will be
		int nDeltaX = (int)(dotProduct * m_dVectorX);
		int nDeltaY = (int)(dotProduct * m_dVectorY);

		// If the projection along either axis does not match the sign of its tracking limit or
		// the current projection is closer to zero than its tracking limit, set the projection to
		// the tracking limit to prevent the zone from becoming smaller than is allowed.
		if ((nDeltaX != 0 && m_nXTrackingLimit != 0 && (nDeltaX > 0 != m_nXTrackingLimit > 0)) ||
			(nDeltaY != 0 && m_nYTrackingLimit != 0 && (nDeltaY > 0 != m_nYTrackingLimit > 0)) ||
			abs(nDeltaX) < abs(m_nXTrackingLimit) ||
			abs(nDeltaY) < abs(m_nYTrackingLimit))
		{
			nDeltaX = m_nXTrackingLimit;
			nDeltaY = m_nYTrackingLimit;
		}

		// Compute the new end point using the projection vector
		m_pZoneEntity->EntDraw(FALSE);
		m_pZoneEntity->setEndPointInImageUnits(
			(long)(m_lOriginalStartX + nDeltaX), 
			(long)(m_lOriginalStartY + nDeltaY));
	}
}
//-------------------------------------------------------------------------------------------------
void CZoneAdjustmentThrd::cancel()
{
	if (m_pZoneEntity == NULL)
	{
		AfxThrowOleDispatchException(0, "ELI23491: Invalid zone entity.");
	}

	m_pZoneEntity->setZoneEntParameters(m_lOriginalStartX, m_lOriginalStartY, 
		m_lOriginalEndX, m_lOriginalEndY, m_lOriginalHeight);

	CGenericDisplayView* pView = m_pGenericDisplayCtrl->getUGDView();
	pView->Invalidate();

	stop();
}
//-------------------------------------------------------------------------------------------------
void CZoneAdjustmentThrd::stop()
{
	m_lStartX = 0;
	m_lStartY = 0;
	m_lEndX = 0;
	m_lEndY = 0;

	m_lOriginalStartX = 0;
	m_lOriginalStartY = 0;
	m_lOriginalEndX = 0;
	m_lOriginalEndY = 0;
	m_lOriginalHeight = 0;

	m_dVectorX = 0;
	m_dVectorY = 0;

	m_nXTrackingLimit = 0;
	m_nYTrackingLimit = 0;
	
	m_bZoneAdjustmentEnabled = false;
	m_bZoneMoveEnabled = false;

	// Fire the zone entity moved event
	m_pGenericDisplayCtrl->FireZoneEntityMoved(m_pZoneEntity->getID());

	m_pZoneEntity = NULL;
}
//-------------------------------------------------------------------------------------------------
bool CZoneAdjustmentThrd::isValid()
{
	return m_pZoneEntity->isValid();
}
//-------------------------------------------------------------------------------------------------
bool CZoneAdjustmentThrd::isInteractiveZoneEntAdjustmentEnabled() 
{
	return m_bZoneAdjustmentEnabled;
}
//-------------------------------------------------------------------------------------------------
void CZoneAdjustmentThrd::setGenericDisplayCtrl(CGenericDisplayCtrl* pGenericDisplayCtrl) 
{
	if (pGenericDisplayCtrl == NULL)
	{
		AfxThrowOleDispatchException(0, "ELI23484: Invalid generic display control.");
	}

	m_pGenericDisplayCtrl = pGenericDisplayCtrl;
}
//-------------------------------------------------------------------------------------------------
void CZoneAdjustmentThrd::setGenericDisplayView(CGenericDisplayView* pGenericDisplayView) 
{
	if (pGenericDisplayView == NULL)
	{
		AfxThrowOleDispatchException(0, "ELI23485: Invalid generic display view.");
	}

	m_pGenericDisplayView = pGenericDisplayView;
}
//-------------------------------------------------------------------------------------------------