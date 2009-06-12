//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	Rectangle.cpp
//
// PURPOSE:	This is an implementation file for Rectangle() class.
//			Where the Rectangle() class has been declared as base class.
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
#include "Rectangle.h"
#include "UCLIDException.h"

//==================================================================================================
/////////////////////////////////////////////////////////////////////////////
// Rectangle

GDRectangle::GDRectangle()
{
	//	set the rectangle coordinates
	bottomLeft.dXPos = 0.0;
	bottomLeft.dYPos = 0.0;
	topRight.dXPos = 0.0;
	topRight.dYPos = 0.0;
}
//==================================================================================================
GDRectangle::GDRectangle(Point ptBottomLeft, Point ptTopRight)
{
	//	rectangle with two points
	bottomLeft	= ptBottomLeft;
	topRight	= ptTopRight;
}
//==================================================================================================
GDRectangle::GDRectangle(GDRectangle& rect)
{
	//	rectangle with rect
	bottomLeft	= rect.bottomLeft;
	topRight	= rect.topRight;
}
//==================================================================================================	
GDRectangle::~GDRectangle()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16455");
}
//==================================================================================================
void GDRectangle::setBottomLeft (Point Pt)
{
	bottomLeft = Pt;
}
//==================================================================================================
void GDRectangle::setTopRight (Point Pt)
{
	topRight = Pt;
}
//==================================================================================================
