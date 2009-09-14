//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	TPPoint.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan
//
//==================================================================================================

#include "stdafx.h"
#include "TPPoint.h"

#include <UCLIDException.h>
#include <mathUtil.h>

#include <cmath>

TPPoint::TPPoint()
{
	m_dX = m_dY = 0.0;
}

TPPoint::TPPoint(double _m_dX, double _m_dY)
{
	m_dX = _m_dX;
	m_dY = _m_dY;
}

TPPoint::TPPoint(const TPPoint& p)
{
	m_dX = p.m_dX;
	m_dY = p.m_dY;
}

TPPoint& TPPoint::operator = (const TPPoint& p)
{
	m_dX = p.m_dX;
	m_dY = p.m_dY;
	return *this;
}

bool operator == (const TPPoint& a, const TPPoint& b)
{
	if ( !MathVars::isEqual(a.m_dX, b.m_dX) || !MathVars::isEqual(a.m_dY, b.m_dY) )
	{
		return false;
	}

	return true;
}

bool operator < (const TPPoint& a, const TPPoint& b)
{
	// for ordering objects of this type,
	// sort by X first, and then by Y
	if (fabs(a.m_dX - b.m_dX) > MathVars::ZERO)
	{
		return (a.m_dX < b.m_dX);
	}
	else
	{
		if (fabs(a.m_dY - b.m_dY) > MathVars::ZERO)
		{
			return (a.m_dY < b.m_dY);
		}
	}

	return false;
}

double TPPoint::distanceTo(const TPPoint& p) const
{
	double deltaX = p.m_dX - m_dX;
	double deltaY = p.m_dY - m_dY;

	return sqrt(deltaX * deltaX + deltaY * deltaY);
}

double TPPoint::angleTo(const TPPoint& p) const
{
	double dX = p.m_dX - m_dX;
	double dY = p.m_dY - m_dY;
	double dAbsX = fabs(dX);
	double dAbsY = fabs(dY);
	double dH = distanceTo(p);

	if (dH < MathVars::ZERO)
	{
		UCLIDException uclidException("ELI01215", "Cannot determine the angle between two coinciding TPPoints.");
		uclidException.addDebugInfo("x", m_dX);
		uclidException.addDebugInfo("y", m_dY);
		uclidException.addDebugInfo("p.x", p.m_dX);
		uclidException.addDebugInfo("p.y", p.m_dY);
		throw uclidException;
	}
	
	// note: cos(angle) = m_dX/dH
	// therefore angle = acos(m_dX/dH);
	double dAngle = acos(dAbsX/dH);

	// when we use dAbsX and dH instead, we get a value between 0 and MathVars::PI/2
	// now translate depending upon quadrant
	if (dX < 0 && dY >= 0)
	{
		dAngle = MathVars::PI - dAngle;
	}
	else if (dX < 0 && dY < 0)
	{
		dAngle = MathVars::PI + dAngle;
	}
	else if (dX >= 0 && dY < 0)
	{
		dAngle = 2*MathVars::PI - dAngle;
	}

	return dAngle;
}
