//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	Point.h
//
// PURPOSE:	This is an header file for Point where this class has been 
//			declared as base class.  The code written in this file makes 
//			it possible for	initialize the combo controls.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================
#include "stdafx.h"

#ifndef __POINT_H_
#define __POINT_H_
//==================================================================================================
//
// CLASS:	Point
//
// PURPOSE:	To encapsulate the concept of a coordinate position, which is used by the GenericDisplay
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
class Point
{
public:
	//	constructor
	Point();
	Point(double , double);
	Point(Point&);

	//	destructor
	virtual ~Point();

	//	operator
	void operator =(Point Pt);
public:
	// the horizontal coordinate position associated with the point object
	double dXPos;

	// the vertical coordinate position associated with the point object.
	double dYPos;

};
#endif //__POINT_H_