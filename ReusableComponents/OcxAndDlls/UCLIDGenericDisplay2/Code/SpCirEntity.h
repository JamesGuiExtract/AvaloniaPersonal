//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	SpecializedCircleEntity.h
//
// PURPOSE:	This is an header file for SpecializedCircleEntity 
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

//==================================================================================================
//
// CLASS:	SpecializedCircleEntity
//
// PURPOSE:	To encapsulate the concept of a special circle entity, who's radius is always defined
//			as a certain percent of the size of the window in which it is drawn.
//
// REQUIRE:	uiPercentRadius must be in the range (0, 100).
// 
// INVARIANTS:
//			None.
//
// EXTENSIONS:
//			None.
//
// NOTES:	While all other entities like the line and curve entities are being specified in
//			terms of coordinate values of starting point, ending point, etc, the size of this entity
//			is being specified relative to the pixel size of the window in which it is drawn.  This
//			feature allows for the entity to "be the same size to the eye" regardless of how much
//			you zoom in or zoom out.
//

class SpecializedCircleEntity : public GenericEntity
{
public:

	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To initialize the SpecializedCircle entity.
	// REQUIRE: Nothing.
	// PROMISE: initializes the datamembers.
	SpecializedCircleEntity(unsigned long id);
	
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To assign the argument values to data members of SpecializedCircle entity.
	// REQUIRE: Nothing.
	// PROMISE: initializes the datamembers.
	SpecializedCircleEntity(unsigned long id, Point, double);
	
	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To delete the SpecializedCircle entity.
	// REQUIRE: Nothing.
	// PROMISE: deletes the the SpecializedCircle entity object
	virtual ~SpecializedCircleEntity();

	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To display the SpecializedCircle entity.
	// REQUIRE: variable to decide the draw true or false.
	// PROMISE: check the visibilty of an entity and return if it is false othervise draws the entity.
	//			generates the no. of points and draw lines between them to make the SpecializedCircle.
	virtual void EntDraw (BOOL bDraw);

	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To calculate the extents of the SpecializedCircle entity.
	// REQUIRE: Nothing explicitly except the class data members.
	// PROMISE: calculate the rectangle that exactly bounds the entity and stores the dimensions in 
	//			data object of m_textboxDims strcuture.
	virtual void ComputeEntExtents ();

	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To get the extents of the SpecializedCircle entity.
	// REQUIRE: GDRectangle object to fill the extents of an entity.
	// PROMISE: Get the extents from entity and caluculates the bounding rectangle dimensions.
	void getExtents(GDRectangle& rBoundingRectangle);

	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To get the description of the SpecializedCircle entity.
	// REQUIRE: Nothing.
	// PROMISE: return the text "circle" to let the users that this is text entity.
	string getDesc();

	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To move the SpecializedCircle entity.
	// REQUIRE: no. of units to move in x and y directions.
	// PROMISE: adds movex and movey to the insertions point.
	void offsetBy(double dX, double dY);

	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To get the string of the SpecializedCircle entity.
	// REQUIRE: Must be in the format of 
	//			"CenterPointX:CenterPointY,PercentRadius"
	// PROMISE: 
	virtual int getEntDataString(CString &zDataString);

	//----------------------------------------------------------------------------------------------
	// PURPOSE:	To get distance from the given point to the entity.
	// REQUIRE: x and y co-ordinates of the point .
	// PROMISE: To return the distance of the entity from the given point.
	virtual double getDistanceToEntity(double dX, double dY);

public:
	// the center of the circle
	Point m_ptCenter;

	// the radius of the special circle defined in terms of the percent size of sum of the average
	// of the height and width of the window in which it is being drawn.  Regardless of the scale of
	// of the GenericDisplay view, this circle entity will always maintain the same size (in terms
	// of pixels) as long as the size of the GenericDisplay view has not changed.  For instance,
	// if uiPercentRadius = 10, and the GenericDisplay view is 150x250 pixels, the size of the
	// circle entity will always be 0.10 x (150 + 250) / 2 = 20 pixels in radius.
	double m_dPercentRadius;

	// this is actual radius with which the sp. circle is drawn 
	// in the current  view. used to calculate the distance between
	// circle and a point in the view
	double m_dCurrentRadius;

};
//==================================================================================================
