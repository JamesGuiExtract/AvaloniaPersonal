//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	CurveEntity.h
//
// PURPOSE:	This is an header file for CurveEntity 
//			where this class has been derived from the GenericEntity()
//			class.  The code written in this file makes it possible for
//			initialize the combo controls.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================
#include "stdafx.h"
#include "GenericEntity.h"
#include <string>

class CGenericDisplayCtrl;

//==================================================================================================
//
// CLASS:	CurveEntity
//
// PURPOSE:	To encapsulate the concept of a curve entity, which is used by the GenericDisplay
//			class.
//
// REQUIRE:	Nothing.
// 
// INVARIANTS:
//			None.
//
// EXTENSIONS:
//			None.
//
// NOTES:	None.
//
class CurveEntity : public GenericEntity
{
public:
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To initialize the curveentity.
	// REQUIRE: Nothing.
	// PROMISE: initializes the datamembers.
	CurveEntity(unsigned long id);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To assign the argument values to data members of curveentity.
	// REQUIRE: Nothing.
	// PROMISE: initializes the datamembers.
	CurveEntity(unsigned long id, Point center, double dRadius, double dStartAng, double dEndAng);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To delete the Curveentity.
	// REQUIRE: Nothing.
	// PROMISE: deletes the the curve entity object that is created
	virtual ~CurveEntity();
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To display the Curve entity.
	// REQUIRE: variable to decide the draw true or false.
	// PROMISE: check the visibilty of an entity and return if it is false othervise draws the entity.
	//			generates the no. of points that make the curve
	virtual void EntDraw (BOOL bDraw);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To calculate the extents of the Curve entity.
	// REQUIRE: Nothing explicitly except the class data members.
	// PROMISE: calculate the rectangle that exactly bounds the entity and stores the dimensions in 
	//			data object of m_textboxDims strcuture.
	virtual void ComputeEntExtents ();
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To get the extents of the Curve entity.
	// REQUIRE: GDRectangle object to fill the extents of an entity.
	// PROMISE: Get the extents from entity and caluculates the bounding rectangle dimensions.
	void getExtents(GDRectangle& rBoundingRectangle);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To get the description of the Curve entity.
	// REQUIRE: Nothing.
	// PROMISE: return the text "curve" to let the users that this is text entity.
	string getDesc();
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To move the Curve entity.
	// REQUIRE: no. of units to move in x and y directions.
	// PROMISE: adds movex and movey to the insertions point.
	void offsetBy(double dX, double dY);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To get the string of the Curve entity.
	// REQUIRE: Must be in the format of 
	//			"CurveCenterX:CurveCenterY,CurveRadius,CurveStartAngle,CurveEndAngle"
	// PROMISE: 
	virtual int getEntDataString(CString &zDataString);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To get distance from the given point to the entity.
	// REQUIRE: x and y co-ordinates of the point .
	// PROMISE: To return the distance of the entity from the given point.
	virtual double getDistanceToEntity(double dX, double dY);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To set start angle to the datamember variable
	// REQUIRE: new startAngle .
	// PROMISE: set the given angle to the class variable.
	void setStartAngle(double dStartAngle) { m_dStartAngle = dStartAngle; }
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To set end angle to the datamember variable
	// REQUIRE: new endAngle .
	// PROMISE: set the given angle to the class variable.
	void setEndAngle(double dEndAngle) { m_dEndAngle = dEndAngle; }
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To get start angle from the entity
	// REQUIRE: Nothing .
	// PROMISE: returns the startangle.
	double getStartAngle() { return m_dStartAngle;}
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To get end angle from the entity
	// REQUIRE: Nothing .
	// PROMISE: returns the endangle.
	double getEndAngle() {return m_dEndAngle; }
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To get start point of the entity
	// REQUIRE: Nothing .
	// PROMISE: returns the start point of the curve entity.
	Point getStartPoint() {return m_stPt; }
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To get end point of the entity
	// REQUIRE: Nothing .
	// PROMISE: returns the end point of the curve entity.
	Point getEndPoint() {return m_endPt; }
	//----------------------------------------------------------------------------------------------

public:
	// the center of the circle who's arc make's this curve
	Point m_center;

	// the radius of the circle who's arc make's this curve
	double m_dRadius;
	
	// the start and end angles (in degrees) for the arc that makes this curve
	double m_dStartAngle, m_dEndAngle;

	// start point of the curve entity
	Point m_stPt;

	// End point of the curve entity
	Point m_endPt;
};
//==================================================================================================
