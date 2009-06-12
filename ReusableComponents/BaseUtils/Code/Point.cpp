#include "stdafx.h"
#include "Point.h"
#include "UCLIDException.h"
#include "mathUtil.h"

#include <math.h>

Point::Point()
{
	dX = dY = 0.0;
	ulID = 0;
	bInitialized = false;
}

Point::Point(double _dX, double _dY)
{
	dX = _dX;
	dY = _dY;
	ulID = 0;
	bInitialized = true;
}

double ZERO_PLUS_ID0 = 1e-6;

bool operator == (const Point& a, const Point& b)
{
	return a.distanceTo(b) <= ZERO_PLUS_ID0;
}

void Point::setID(unsigned long _ulID)
{
	ulID = _ulID;
}

bool operator < (const Point& a, const Point& b)
{
	// for ordering objects of this type,
	// sort by X first, and then by Y
	if (a == b)
		return false;
	else if (fabs(a.dX - b.dX) <= ZERO_PLUS_ID0)
	{
		return a.dY < b.dY;
	}
	else if (fabs(a.dY - b.dY) <= ZERO_PLUS_ID0)
	{
		return a.dX < b.dX;
	}
	else
	{
		return a.dX < b.dX;
	}
}

bool Point::isInitialized() const
{
	return bInitialized;
}

double Point::distanceTo(const Point& p) const
{
	double deltaX = p.dX - dX;
	double deltaY = p.dY - dY;
	return sqrt(deltaX * deltaX + deltaY * deltaY);
}

double Point::angleTo(const Point& p) const
{
	double dX = p.getX() - getX();
	double dY = p.getY() - getY();
	double dAbsX = fabs(dX);
//	double dAbsY = fabs(dY);
	double dH = distanceTo(p);

	if (dH < 1e-9) // definition of zero!
	{
		UCLIDException uclidException("ELI00319", "Cannot determine the angle between two coinciding points!");
		uclidException.addDebugInfo("x", getX());
		uclidException.addDebugInfo("y", getY());
		uclidException.addDebugInfo("p.x", p.getX());
		uclidException.addDebugInfo("p.y", p.getY());
		throw uclidException;
	}
	
	// note: cos(angle) = dX/dH
	// therefore angle = acos(dx/dH);
	double dAngle = acos(dAbsX/dH);

	// when we use dAbsX and dH instead, we get a value between 0 and pi/2
	// now translate depending upon quadrant
	if (dX == 0 && dY == 0)
		dAngle = 0;
	else if (dX <= 0 && dY >= 0)
		dAngle = MathVars::PI - dAngle;
	else if (dX <= 0 && dY <= 0)
		dAngle = MathVars::PI + dAngle;
	else if (dX >= 0 && dY <= 0)
		dAngle = 2*MathVars::PI - dAngle;
	
	return dAngle;
}
