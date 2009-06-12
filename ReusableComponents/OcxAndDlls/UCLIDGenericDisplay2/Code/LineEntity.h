//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	LineEntity.h
//
// PURPOSE:	This is an header file for LineEntity class
//			where this has been derived from the GenericEntity()
//			class.  The code written in this file makes it possible for
//			initialize to set the view.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================
#include "stdafx.h"
#include "GenericEntity.h"
#include <string>

//==================================================================================================
//
// CLASS:	LineEntity
//
// PURPOSE:	To encapsulate the concept of a line entity, which is used by the GenericDisplay
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

class LineEntity : public GenericEntity
{
public:

	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To initialize the Line entity.
	// REQUIRE: Nothing.
	// PROMISE: initializes the datamembers.
	LineEntity(unsigned long id);
	
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To assign the argument values to data members of Line entity.
	// REQUIRE: Nothing.
	// PROMISE: initializes the datamembers.
	LineEntity(unsigned long id, Point, Point);
	
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To assign the argument values to data members of Line entity.
	// REQUIRE: Nothing.
	// PROMISE: initializes the datamembers.
	LineEntity(unsigned long id, EntityAttributes& , COLORREF, Point, Point);

	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To delete the Line entity.
	// REQUIRE: Nothing.
	// PROMISE: deletes the the Line entity object
	virtual ~LineEntity();	

	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To display the Line entity.
	// REQUIRE: variable to decide the draw true or false.
	// PROMISE: check the visibilty of an entity and return if it is false othervise draws the entity.
	//			generates the no. of points and draw lines between them to make the Line.
	virtual void EntDraw (BOOL bDraw);

	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To calculate the extents of the Line entity.
	// REQUIRE: Nothing explicitly except the class data members.
	// PROMISE: calculate the rectangle that exactly bounds the entity and stores the dimensions in 
	//			data object of m_textboxDims strcuture.
	virtual void ComputeEntExtents ();

	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To get the extents of the Line entity.
	// REQUIRE: GDRectangle object to fill the extents of an entity.
	// PROMISE: Get the extents from entity and caluculates the bounding rectangle dimensions.
	void getExtents(GDRectangle& rBoundingRectangle);

	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To get the description of the Line entity.
	// REQUIRE: Nothing.
	// PROMISE: return the text "Line" to let the users that this is text entity.
	string getDesc();

	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To move the Line entity.
	// REQUIRE: no. of units to move in x and y directions.
	// PROMISE: adds movex and movey to the insertions point.
	void offsetBy(double dX, double dY);	

	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To get the string of the Line entity.
	// REQUIRE: Must be in the format of 
	//			"LineStartPointX:LineStartPointY,LineEndPointX:LineEndPointY"
	// PROMISE: 
	virtual int getEntDataString(CString &zDataString);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To get distance from the given point to the entity.
	// REQUIRE: x and y co-ordinates of the point .
	// PROMISE: To return the distance of the entity from the given point.
	virtual double getDistanceToEntity(double dX, double dY);
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To get start point of the entity
	// REQUIRE: Nothing .
	// PROMISE: returns the start point of the line entity.
	Point getStartPoint() {return m_startPoint; }
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To get end point of the entity
	// REQUIRE: Nothing .
	// PROMISE: returns the end point of the line entity.
	Point getEndPoint() {return m_endPoint; }
	//----------------------------------------------------------------------------------------------
public:
	// the starting point of the line
	Point m_startPoint;
	
	// the ending point of the line
	Point m_endPoint;

};
//==================================================================================================
