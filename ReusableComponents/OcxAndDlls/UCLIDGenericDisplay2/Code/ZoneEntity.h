//-------------------------------------------------------------------------------------------------
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ZoneEntity.h
//
// PURPOSE:	This is an header file for ZoneEntity class
//			where this has been derived from the GenericEntity()
//			class.  The code written in this file makes it possible for
//			initialize Zone entity and to see it in the view.
// NOTES:	
//
// AUTHORS:	Arvind Ganesan, M.Srinivasa Rao
//
//-------------------------------------------------------------------------------------------------
#pragma once

#include "stdafx.h"
#include "GenericEntity.h"
#include "Rectangle.h"

#include <string>
#include <map>

class CGenericDisplayView;

//-------------------------------------------------------------------------------------------------
// Static/Global Variables
//-------------------------------------------------------------------------------------------------
// number of screen pixels to extend zone by to create the zone border.
static const int giZONE_BORDER_WIDTH = 5; 

// Half the width of a grip handle in client pixels
static const int giHALF_GRIP_HANDLE_WIDTH = 4;

//-------------------------------------------------------------------------------------------------
// Enums
//-------------------------------------------------------------------------------------------------
// kLeft, kRight, kTop, and kBottom can be bitwise ORed to produce other combinations.
enum EGripHandle
{
	kNone = 0,
	kLeft = 1,
	kRight = 2,
	kTop = 4,
	kLeftTop = 5,
	kRightTop = 6,
	kBottom = 8,
	kLeftBottom = 9,
	kRightBottom = 10
};

//-------------------------------------------------------------------------------------------------
//
// CLASS:	ZoneEntity
//
// PURPOSE:	To encapsulate the concept of a zone entity, which is used by the UCLIDGenericDisplay
//		    to highlight rectangular (which are possibly rotated) regions of the image.
//
// REQUIRE:	Nothing.
// 
// INVARIANTS:
//			None.
//
// EXTENSIONS:
//			None.
//
// NOTES:	The Zone parameters are all defined in terms of integer pixel positions rather
//		than "real world units" because the Zone has nothing to do with the scale of the
//		image, its resolution, etc.  The conceptual zone is just a region on the image, which
//		is independent of the scale of the image, scan resolution, color depth, etc.
//
class ZoneEntity : public GenericEntity
{
public:

	// PURPOSE: Constructor 
	// REQUIRE: Needs start and end points with the height of the zone
	// PROMISE: 
	// NOTES:	See ODL file's addZoneEntity() method for additional documentation
	//			on bFill and bTransparentInside
	ZoneEntity(unsigned long id, long startPtX, long startPtY, long endPtX, long endPtY, 
		long zoneHeight, bool bFill, bool bBorderVisible, COLORREF borderColor);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Destructor
	// REQUIRE: 
	// PROMISE: 
	~ZoneEntity();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: copy constructor
	// REQUIRE: 
	// PROMISE: 
	ZoneEntity(const ZoneEntity& );
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Equal operator for the class
	// REQUIRE: 
	// PROMISE: 
	void operator =(const ZoneEntity& zoneEntToAssign);
	//---------------------------------------------------------------------------------------------
	// the following three methods are virtual functions overridden from the base class.
	// PURPOSE: To retrieve the graphical extents associated with this entity.
	// REQUIRE: Nothing.
	// PROMISE: To return the topLeft and bottomRight coordinates of the smallest rectangle that
	//			can bound this entity.
	void getExtents(GDRectangle& rBoundingRectangle);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To retrieve the description of this entity.
	// REQUIRE: Nothing.
	// PROMISE: To return a string that describes the type of this entity.
	string getDesc();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To offset this entity by a given amount. The values are in image pixels
	// REQUIRE: Nothing.
	// PROMISE: To move this entity horizontally and vertically by dX and dY respectively.
	void offsetBy(double dX, double dY);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: to update world coordinate variables.
	// REQUIRE: Nothing
	// PROMISE: this method updates the world-unit member variables of this class, 
	//          which are calculated from their corresponding image-pixel unit variables.
	void updateWorldUnitVariables();
	//---------------------------------------------------------------------------------------------
	//   These two methods are optimized - i.e. they only call 
	// updateWorldUnitVariables() if the image pixel unit variables have been modified since
	// the last call to updateWorldUnitVariables().
	// PURPOSE: 
	// REQUIRE: 
	// PROMISE: this  method retrieves the starting point in world units.
	Point getStartPointInWorldUnits();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: to return end point in world coordinates
	// REQUIRE: Nothing
	// PROMISE: this  method retrieves ending point in world units.
	Point getEndPointInWorldUnits();
	//---------------------------------------------------------------------------------------------
	// PROMISE: Retrieves the starting point in image units.
	Point getStartPointInImageUnits();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Retrieves the ending point in image units.
	Point getEndPointInImageUnits();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Sets the ending point in image units.
	void setEndPointInImageUnits(long lX, long lY);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To calculate the view coordinates where the zone is going to be drawn
	// REQUIRE: Nothing
	// PROMISE: builds the zone region with the current zone coordinates and height
	void CalculateViewCoords();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To compute the entity extents
	// REQUIRE: Nothing.
	// PROMISE: To compute the entity extents for zone entity;
	virtual void ComputeEntExtents();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To draw the entities
	// REQUIRE: calculateZoneVertices() must have been called once.
	// PROMISE: To draw the entity
	virtual void EntDraw(BOOL bDraw);
	//---------------------------------------------------------------------------------------------
	// PURPOSE:	To get the string of the Zone entity.
	// REQUIRE: Must be in the format of 
	//			"ZoneStartPointX:ZoneStartPointY,ZoneEndPointX:ZoneEndPointY,ZoneHeight"
	// PROMISE: 
	virtual int getEntDataString(CString &zDataString);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To get the distance from entity
	// REQUIRE: Nothing.
	// PROMISE: None
	virtual double getDistanceToEntity(double dX, double dY);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To set or get the zone attribute values
	// REQUIRE: The values must be in image coordinates
	// PROMISE: Sets or returns the start, end and height of the zone 
	void getZoneEntParameters(long& lStPosX, long&  lStPosY, long& lEndPosX, long& lEndPosY, long& lHeight);
	void setZoneEntParameters(long lStPosX, long lStPosY, long lEndPosX, long lEndPosY, long lHeight);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To calculate the zone bounds and store them in the member variables.
	// REQUIRE: Valid start, end points of zone and zone height
	// PROMISE: The zone bound member variables will be updated. The coordinates will be 
	//          recalculated only if the object has changed (m_bZonePtsCalculated == false).
	void calculateZoneVertices();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To calculate the corner points of the selection rectangle and the center points of 
	//          the grip handles and store them in the member variables.
	// REQUIRE: Valid start, end points of zone and zone height
	// PROMISE: The zone selection point member variables will be updated. The coordinates will be 
	//          recalculated only if the object has changed (m_bZoneSelectionPtsCalculated == false).
	void calculateZoneSelectionPoints();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To calculate the bounding rectangle for the zone entity
	// REQUIRE: pzoneVertices != __nullptr and shoild point to an array of 4 points size
	// PROMISE: returns the bounds of zone as a rectangle through rBoundingRectangle
	void getZoneBoundingRect(GDRectangle& rBoundingRectangle);
	//---------------------------------------------------------------------------------------------
	// PURPOSE:	To check whether the given point is within the zone entity bounds.
	// REQUIRE: Nothing.
	// PROMISE: returns true if the given point is within zone entity bounds otherwise false.
	BOOL isPtOnZone(int x, int y);
	//---------------------------------------------------------------------------------------------
	// PURPOSE:	Retrieves the zero-based grip handle id that contains the specified point in world 
	//          coordinates or -1 if no grip handle contains the specified point.
	int getGripHandleId(double x, double y);
	//---------------------------------------------------------------------------------------------
	// PURPOSE:	Retrieves the grip handle that corresponds to the specified grip handle id.
	EGripHandle getGripHandle(int iGripHandleId);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Restructures the zone entity so that the specified grip handle id corresponds to 
	//          end point of the zone entity.
	void makeGripHandleEndPoint(int iGripHandleId);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Returns true if the zone entity has a reasonable width and height; false otherwise.
	bool isValid();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To retrieve the closest angle to that which connects point X and Y that represents
	//			the angle of one of the four vectors connecting the midpoints of opposing sides
	//			of the zone.
	// PROMISE: The angle (in radians).
	// NOTE:	If bInvertYCoordinates is true, perform the calculation based on inverted Y
	//			coordinates.
	double getAngle(long nX1, long nX2, long nY1, long nY2, bool bInvertYCoordinates);

	//----------------------
	// PUBLIC DATA MEMBERS
	//----------------------
	// a flag to indicate if the zone has a visible border
	bool m_bBorderVisible;
	
	// color of the border, if the border is visible
	COLORREF m_borderColor;

	// a flag to indicate if the zone is filled.
	bool m_bFill;

	// border style.  Note, the actual border style used for drawing will
	// be m_nBorderStyle | PS_GEOMETRIC so that non-solid borders can be
	// drawn at a width > 1.
	long m_nBorderStyle;

	// flag indicating whether zone border is special or normal.
	// see documentation for "isZoneBorderSpecial" in ODL file for 
	// meaning of "special" vs "Normal"
	bool m_bBorderIsSpecial;

protected:
	// the following member variable's values are described in terms of image pixels.  Image pixels
	// are different from client window pixels.  For instance, when we zoom really
	// close into an image, one image pixel may consume 10x10 square of 100 client window pixels.

	// the starting point and ending point mouse positions (specified in terms of corresponding
	// image pixel positions) that would be used to create this ZoneEntity interactively.  The
	// starting point is the middle point on the left edge of the rectangular zone.  The ending 
	// point is the middle point on the right edge of the rectangular zone.
	long m_lStartPosX, m_lStartPosY, m_lEndPosX, m_lEndPosY;

	// the total height of the zone, specified in image pixels.  ulTotalZoneHeight is always an
	// odd number greater than or equal to 5.  ulTotalZoneHeight/2 pixels of the zone are above the
	// line from (ulStartPosX,ulStartPosY) to (ulEndPosX,ulEndPosY) and the other half is below the
	// specified line.
	unsigned long m_ulTotalZoneHeight;

	// the following world unit variables represent the zone in world units, and are always 
	// calculated using the updateWorldUnitVariables() method, which calculates these values based
	// upon the current value of the image pixel unit parameters above.
	double dStartPosX, dStartPosY, dEndPosX, dEndPosY, dTotalZoneHeight;

	// the height of the zone in current view
	long m_ulCurZoneHtInView;

	// contains zone bounding points in world and view coordinates
	CPoint m_zoneBoundPts[4];
	CPoint m_zoneBoundViewPts[4];

	// coordinates of the zone border which envelops the zone with a certain
	// pen width
	// NOTE: point #4 = point #0 for zone border
	CPoint m_zoneBorderViewPts[5]; 

	// Coordinates of the zone selection border in world and view coordinates.
	CPoint m_zoneSelectionBorderPts[4];
	CPoint m_zoneSelectionBorderViewPts[4];

	// Coordinates of the grip points of the zone in world and view coordinates.
	CPoint m_zoneGripPoints[4];
	CPoint m_zoneGripViewPoints[4];

	// while reading the .gdd file, image heights and widths of all pages(in case of
	// multiple page images) are not yet known. So calculate the zone extents while drawing 
	// for the first time. Have a flag to know whether zone extents are calculated. PVCS SCR#235
	bool m_bZonePtsCalculated;
	bool m_bZoneSelectionPtsCalculated;

	// The angle of the zone entity as taken from the midpoint of its top-left side to the midpoint
	// of its bottom-right side.
	double m_dBaseAngle;

	//---------------------------------------------------------------------------------------------
	// PURPOSE: Calculates the locations of the specified cornerPoints expanded outward by lExpand.
	// REQUIRE: pPoints points to an array of at least 4 CPoint objects.
	// PROMISE: pPoints will be updated with the expanded corner points.
	void expandCornerPoints(CPoint cornerPoints[4], long lExpand, CPoint *pPoints);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To determine the corner points of a zone represented by a 
	//			start point, end point, and height.
	// REQUIRE: pPoints points to an array of at least 4 CPoint objects.
	// PROMISE: pPoints will be updated with the four corner points that make
	//			up the zone.
	// NOTE:	If bInvertYCoordinates is true, perform the calculation based on inverted Y
	// coordinates.
	void getZoneCornerPoints(long nX1, long nX2, long nY1, long nY2,
		long nHeight, bool bInvertYCoordinates, CPoint *pPoints);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To determine the midpoints of the sides of a zone represented by a start point, 
	//          end point, and height.
	// REQUIRE: pPoints points to an array of at least 4 CPoint objects.
	// PROMISE: pPoints will be updated with the four corner points that make up the zone.
	void getZoneMidPoints(long nX1, long nX2, long nY1, long nY2, long nHeight, CPoint *pPoints);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To classify the orientation of the vector connecting the two points with an ID
	//			indicating which of the four vectors crossing through the raster zone at the 
	//			midpoints of opposing sides is closest to the one specified.
	// PROMISE: An indentifier will be returned specifying the nearest midpoint vector:
	//				0 = Closest to the left to right midpoint vector
	//				1 = Closest to the top to bottom midpoint vector
	//				2 = Closest to the right to left midpoint vector
	//				3 = Closest to the bottom to top midpoint vector
	// NOTE:	If bInvertYCoordinates is true, perform the calculation based on inverted Y
	// coordinates.
	int getOrientation(long nX1, long nX2, long nY1, long nY2, bool bInvertYCoordinates);
};
