//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ThreePoints.h
//
// PURPOSE:	To store start, mid and end point (of a curve)
//
// NOTES:	
//
// AUTHORS:	Duan Wang
//
//==================================================================================================

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include <TPPoint.h>

class ThreePoints
{
public:
	ThreePoints(const TPPoint& startPoint, const TPPoint& midPoint,const TPPoint& endPoint);
	~ThreePoints() {};
	const TPPoint& getStartPoint() const {return m_startPoint;} 
	const TPPoint& getMidPoint() const {return m_midPoint;}
	const TPPoint& getEndPoint() const {return m_endPoint;}
	// overload == and < to compare two ThreePoints objects
	friend bool operator == (const ThreePoints& TP1, const ThreePoints& TP2);
	friend bool operator < (const ThreePoints& TP1, const ThreePoints& TP2);

private:
	TPPoint m_startPoint;
	TPPoint m_midPoint;
	TPPoint m_endPoint;
};