//-------------------------------------------------------------------------------------------------
//
// COPYRIGHT (c) 2008 EXTRACT SYSTEMS, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ZoneAdjustmentThrd.h
//
// PURPOSE:	Handles an interactive mouse event for moving and resizing highlights.
//
// NOTES:	
//
// AUTHORS:	Nathan Figueroa
//
//-------------------------------------------------------------------------------------------------
#pragma once

class CGenericDisplayView;
class CGenericDisplayCtrl;
class ZoneEntity;

class CZoneAdjustmentThrd 
{

public:
	CZoneAdjustmentThrd(); 
	~CZoneAdjustmentThrd();  

	//---------------------------------------------------------------------------------------------
	// PURPOSE: Starts the interactive zone adjustment
	void start(ZoneEntity* pZoneEntity, int iEX, int iEY, int iGripHandleId = -1);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Updates the interactive zone adjustment
	void update(int iEX, int iEY);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To reset the thread class variables as though the zone entity had not been 
	//          adjusted; to be called when the zone entity has not been successfully adjusted.
	void cancel();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To reset the thread class variables to a state where they can be reused. Meant to 
	//          be called when the zone entity has successfully been adjusted.
	void stop();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Returns whether the resultant zone entity is valid.
	bool isValid();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To return status of the interactive zone entity adjustment
	bool isInteractiveZoneEntAdjustmentEnabled();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To set current related CGenericDisplayCtrl object. This is required to enable UGD 
	//			to create multiple instances and to link view and frame of corresponding ctrl object
	void setGenericDisplayCtrl(CGenericDisplayCtrl* pGenericDisplayCtrl);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To set current related CGenericDisplayView object.
	void setGenericDisplayView(CGenericDisplayView* pGenericDisplayView);
	//---------------------------------------------------------------------------------------------

private:

	// X coordinate of the start point of the zone adjustment
	long m_lStartX;

	// Y coordinate of the start point of the zone adjustment
	long m_lStartY;

	// X coordinate of the end point of the zone adjustment
	long m_lEndX;

	// Y coordinate of the end point of the zone adjustment
	long m_lEndY;

	// Original x coordinate of the start point of the zone entity that is being adjusted
	long m_lOriginalStartX;

	// Original y coordinate of the start point of the zone entity that is being adjusted
	long m_lOriginalStartY;

	// Original x coordinate of the end point of the zone entity that is being adjusted
	long m_lOriginalEndX;

	// Original y coordinate of the end point of the zone entity that is being adjusted
	long m_lOriginalEndY;

	// Original zone highlight height
	long m_lOriginalHeight;

	// A unit vector representing the angle of the zone entity
	double m_dVectorX;
	double m_dVectorY;

	// The coordinates relative to the zone start point at which the vector connecting the start 
	// and end points results in the minimum allowed length of the zone.
	long m_nXTrackingLimit;
	long m_nYTrackingLimit;

	// Indicates whether interactive zone adjustment is enabled or disabled
	bool m_bZoneAdjustmentEnabled;

	// Indicates whether interactive zone moving is enabled or disabled
	bool m_bZoneMoveEnabled;

	// The zone entity that is being adjusted, or null if no zone adjustment is underway
	ZoneEntity* m_pZoneEntity;

	// The control on which the zone appears
	CGenericDisplayCtrl* m_pGenericDisplayCtrl;

	// The display view on which the zone appears
	CGenericDisplayView* m_pGenericDisplayView;
};