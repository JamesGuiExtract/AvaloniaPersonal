//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	Point.cpp
//
// PURPOSE:	This is an implementation file for Point() class.
//			Where the Point() class has been declared as base class.
//			The code written in this file makes it possible to implement the various
//			application methods in the user interface.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================
// Point.cpp : implementation file
//

#include "stdafx.h"
#include "Point.h"
#include "UCLIDException.h"

//==================================================================================================
/////////////////////////////////////////////////////////////////////////////
// Point
Point::Point()
{

}
//==================================================================================================
Point::Point(double xPos, double yPos)
{
	//	set point coordinates
	dXPos	= xPos;
	dYPos	= yPos;
}
//==================================================================================================
Point::Point(Point& pt)
{
	//	set point coordinates
	dXPos	= pt.dXPos;
	dYPos	= pt.dYPos;
}
//==================================================================================================	
Point::~Point()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16454");
}
//==================================================================================================
void Point::operator = (Point Pt)
{
	//	set point coordinates
	dXPos = Pt.dXPos;
	dYPos = Pt.dYPos;
}
//==================================================================================================
