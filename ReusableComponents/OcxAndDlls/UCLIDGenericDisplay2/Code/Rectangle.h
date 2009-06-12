//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	Rectangle.h
//
// PURPOSE:	This is an header file for Rectangle where this class has been 
//			declared as base class.  The code written in this file makes 
//			it possible for	initialize the combo controls.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================
#include "stdafx.h"

#ifndef _Rectangle_h_
#define _Rectangle_h_

#include "Point.h"

//==================================================================================================
//
// CLASS:	Rectangle
//
// PURPOSE:	To encapsulate the concept of a Rectangle, which is used by the classes below.
//
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
class GDRectangle
{
public:
	//	constructor
	GDRectangle();
	GDRectangle(Point , Point);
	GDRectangle(GDRectangle&);

	//-------------------------------------------------------
	// PURPOSE:	To set bottom left corner of the rectangle
	// REQUIRE: None
	// PROMISE: None
	void setBottomLeft (Point pt);
	//-------------------------------------------------------
	// PURPOSE:	To set top right corner of the rectangle
	// REQUIRE: None
	// PROMISE: None
	void setTopRight (Point pt);
	//-------------------------------------------------------
	// PURPOSE:	To get bottom left corner of the rectangle
	// REQUIRE: None
	// PROMISE: None
	Point getBottomLeft()  
	{ 
		return bottomLeft; 
	}
	//-------------------------------------------------------
	// PURPOSE:	To get top right corner of the rectangle
	// REQUIRE: None
	// PROMISE: None
	Point getTopRight()  
	{
		return topRight;
	}

protected:
	// the bottom left corner of the Rectangle
	Point bottomLeft;

	// the top right corner of the rectangle;
	Point topRight;

public:
	virtual ~GDRectangle();
};

#endif	//_Rectangle_h_