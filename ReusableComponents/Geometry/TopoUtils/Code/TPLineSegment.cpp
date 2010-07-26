//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	TPLineSegment.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan
//
//==================================================================================================

#include "stdafx.h"
#include "TPLineSegment.h"

#include <mathUtil.h>

#include <math.h>
#include <stdlib.h>
#include <vector>
#include <string>
using namespace std;

//--------------------------------------------------------------------------------------------------
TPLineSegment::TPLineSegment()
{
}
//--------------------------------------------------------------------------------------------------
TPLineSegment::TPLineSegment(const TPPoint& _p1, const TPPoint& _p2)
{
	m_p1 = _p1;
	m_p2 = _p2;
}
//--------------------------------------------------------------------------------------------------
TPLineSegment::TPLineSegment(const TPLineSegment& lineToCopy)
{
	m_p1 = lineToCopy.m_p1;
	m_p2 =  lineToCopy.m_p2;
}
//--------------------------------------------------------------------------------------------------
TPLineSegment& TPLineSegment::operator=(const TPLineSegment& lineToAssign)
{
	m_p1 = lineToAssign.m_p1;
	m_p2 =  lineToAssign.m_p2;
	return *this;
}
//--------------------------------------------------------------------------------------------------
bool TPLineSegment::startsOrEndsAt(const TPPoint& p, bool& bStarts) const
{
	bStarts = m_p1 == p;
	return (bStarts) || (m_p2 == p);
}
//--------------------------------------------------------------------------------------------------
bool TPLineSegment::contains(const TPPoint& p) const
{
	double dMySlope = getSlope();

	if (fabs(dMySlope) == MathVars::INFINITY)
	{
		// this line is vertical.. the TPPoint can lie on this line only if the x
		// values match
		if (fabs(m_p1.m_dX - p.m_dX) < MathVars::ZERO)
		{
			// the target TPPoint is on the imaginary line that extends infinitely, 
			// but we need to check to see if it is in the line segment
			if (p.m_dY >= min(m_p1.m_dY, m_p2.m_dY) &&
				p.m_dY <= max(m_p1.m_dY, m_p2.m_dY))
			{
				// the TPPoint lies on the line segment
				return true;
			}
			else
			{
				// the TPPoint lies in the imaginary line but not on the line segment
				return false;
			}
		}
		else
		{
			// this line is vertical, and the target TPPoint has a different x value
			// than this line, so it couldn't be on this line
			return false;
		}
	}
	else
	{
		// find the TPPoint on the line segment extended infinitely that
		// is directly above or below the given TPPoint
		double dY = m_p1.m_dY + dMySlope * (p.m_dX - m_p1.m_dX);
		TPPoint TPPointOnLine(p.m_dX, dY);
		
		if (TPPointOnLine == p)
		{
			// the specified TPPoint is somewhere on the imaginary line, but
			// is it on the line segment?
			// check the Y coordinates first
			if (dY >= min(m_p1.m_dY, m_p2.m_dY) &&
				dY <= max(m_p1.m_dY, m_p2.m_dY))
			{
				// need to also check if it is within the X boundaries of the segment
				// [p16 #2691 & #2692]
				if (p.m_dX >= min(m_p1.m_dX, m_p2.m_dX) &&
					p.m_dX <= max(m_p1.m_dX, m_p2.m_dX))
				{
					// the TPPoint lies on the line segment
					return true;
				}
				else
				{
					// the TPPoint lies on the imaginary line, but not on the line segment
					return false;
				}
			}
			else
			{
				// the TPPoint lies on the imaginary line, but not on the line segment
				return false;
			}
		}
		else
		{
			// the given TPPoint is not even on the imaginary line
			return false;
		}
	}
}
//--------------------------------------------------------------------------------------------------
bool TPLineSegment::intersects(const TPLineSegment& line2, TPPoint& intersectionTPPoint) const
{
	double dMySlope = getSlope();
	double dLine2Slope = line2.getSlope();

	if (fabs(dMySlope) != MathVars::INFINITY && fabs(dLine2Slope) != MathVars::INFINITY)
	{
		// neither of the two lines are vertical

		// suppose my staring TPPoint was p1, and s1 was my slope, and
		// the other segment's starting TPPoint was p2, and its slope was s2,
		// suppose the intersection TPPoint was (ix, iy)
		// then the intersection TPPoint Y = p1.y + s1 (ix - p1.x)
		//                       also, Y = p2.y + s2 (ix - p2.x)
		// solving for ix, we get
		// ix = (p2.y - p1.y - s2 * p2.x + s1 * p1.x) / (s1 - s2);

		// before we attempt to calculate the intersection TPPoint, we should make sure that 
		// the two lines are not parallel
		if (fabs(dMySlope - dLine2Slope) < MathVars::ZERO)
		{
			// the two lines are parallel, but they could still be intersecting because
			// they can have some TPPoints in common
			// TODO: for now return false;
			return false;
		}

		// calculate the intersection TPPoint X and Y
		double dIntersectionTPPointX = (line2.m_p1.m_dY - m_p1.m_dY - dLine2Slope * line2.m_p1.m_dX + dMySlope * m_p1.m_dX) / (dMySlope - dLine2Slope);
		double dIntersectionTPPointY = m_p1.m_dY + dMySlope * (dIntersectionTPPointX - m_p1.m_dX);
		intersectionTPPoint = TPPoint(dIntersectionTPPointX, dIntersectionTPPointY);

		// there's intersection only if the intersection TPPoint's Y 
		// is within my Y's and within line2's Y's
		bool b1 = dIntersectionTPPointY - min(m_p1.m_dY, m_p2.m_dY) >= -MathVars::ZERO;
		bool b2 = max(m_p1.m_dY, m_p2.m_dY) - dIntersectionTPPointY >= -MathVars::ZERO;
		bool b3 = dIntersectionTPPointY - min(line2.m_p1.m_dY, line2.m_p2.m_dY) >= -MathVars::ZERO;
		bool b4 = max(line2.m_p1.m_dY, line2.m_p2.m_dY) - dIntersectionTPPointY >= -MathVars::ZERO;

		double d1 = dIntersectionTPPointY - min(m_p1.m_dY, m_p2.m_dY);
		double d2 = max(m_p1.m_dY, m_p2.m_dY) - dIntersectionTPPointY;
		double d3 = dIntersectionTPPointY - min(line2.m_p1.m_dY, line2.m_p2.m_dY);
		double d4 = max(line2.m_p1.m_dY, line2.m_p2.m_dY) - dIntersectionTPPointY;

		bool bIntersectionYOK = false;
		if ((dIntersectionTPPointY - min(m_p1.m_dY, m_p2.m_dY) >= -MathVars::ZERO) &&
			(max(m_p1.m_dY, m_p2.m_dY) - dIntersectionTPPointY >= -MathVars::ZERO) &&
			(dIntersectionTPPointY - min(line2.m_p1.m_dY, line2.m_p2.m_dY) >= -MathVars::ZERO) &&
			(max(line2.m_p1.m_dY, line2.m_p2.m_dY) - dIntersectionTPPointY >= -MathVars::ZERO))
		{
			bIntersectionYOK = true;
		}

		// there's intersection only if the intersection TPPoint's Y 
		// is within my Y's and within line2's Y's
		bool bIntersectionXOK = false;
		if ((dIntersectionTPPointX - min(m_p1.m_dX, m_p2.m_dX) >= -MathVars::ZERO) &&
			(max(m_p1.m_dX, m_p2.m_dX) - dIntersectionTPPointX >= -MathVars::ZERO) &&
			(dIntersectionTPPointX - min(line2.m_p1.m_dX, line2.m_p2.m_dX) >= -MathVars::ZERO) &&
			(max(line2.m_p1.m_dX, line2.m_p2.m_dX) - dIntersectionTPPointX >= -MathVars::ZERO))
		{
			bIntersectionXOK = true;
		}

		double d = min(line2.m_p1.m_dY, line2.m_p2.m_dY);

		// there's intersection only if the intersection TPPoint is already part
		// of the two line segments
		return bIntersectionXOK && bIntersectionYOK;
	}
	else if (fabs(dMySlope) != MathVars::INFINITY)
	{
		// i am not vertical but line 2 is.

		// if line2's x is inbetween my starting and ending x's, there's a chance 
		// that we intersesect
		if (line2.m_p1.m_dX >= min(m_p1.m_dX, m_p2.m_dX) && 
			line2.m_p1.m_dX <= max(m_p1.m_dX, m_p2.m_dX))
		{
			// calculate the intersection TPPoint
			double dIntersectionTPPointY = m_p1.m_dY + (line2.m_p1.m_dX - m_p1.m_dX) * getSlope();
			intersectionTPPoint = TPPoint(line2.m_p1.m_dX, dIntersectionTPPointY);

			// line2's x is in the range of my x, but there's intersection only if 
			// the intersection TPPoint's Y is within my Y's and within line2's Y's
			if ((dIntersectionTPPointY - min(m_p1.m_dY, m_p2.m_dY) >= -MathVars::ZERO) &&
				(max(m_p1.m_dY, m_p2.m_dY) - dIntersectionTPPointY >= -MathVars::ZERO) &&
				(dIntersectionTPPointY - min(line2.m_p1.m_dY, line2.m_p2.m_dY) >= -MathVars::ZERO) &&
				(max(line2.m_p1.m_dY, line2.m_p2.m_dY) - dIntersectionTPPointY >= -MathVars::ZERO))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		else
		{
			// my x is not in the range of x's of line2, so there could
			// possibly be no intersection
			return false;
		}

	}
	else if (fabs(dLine2Slope) != MathVars::INFINITY)
	{
		// i am vertical, but line 2 is not

		// if my x is inbetween the starting and ending x's of line2, there's a chance 
		// that we intersesect
		if (m_p1.m_dX >= min(line2.m_p1.m_dX, line2.m_p2.m_dX) && 
			m_p1.m_dX <= max(line2.m_p1.m_dX, line2.m_p2.m_dX))
		{
			// calculate the intersection TPPoint
			double dIntersectionTPPointY = line2.m_p1.m_dY + (m_p1.m_dX - line2.m_p1.m_dX) * line2.getSlope();
			intersectionTPPoint = TPPoint(m_p1.m_dX, dIntersectionTPPointY);

			// my x is in the range of line2's x, but there's intersection only if 
			// the intersection TPPoint's Y is within my Y's and within line2's Y's
			if ((dIntersectionTPPointY - min(m_p1.m_dY, m_p2.m_dY) >= -MathVars::ZERO) &&
				(max(m_p1.m_dY, m_p2.m_dY) - dIntersectionTPPointY >= -MathVars::ZERO) &&
				(dIntersectionTPPointY - min(line2.m_p1.m_dY, line2.m_p2.m_dY) >= -MathVars::ZERO) &&
				(max(line2.m_p1.m_dY, line2.m_p2.m_dY) - dIntersectionTPPointY >= -MathVars::ZERO))
			{
				return true;
			}
			else
			{
				return false;
			}

		}
		else
		{
			// my x is not in the range of x's of line2, so there could
			// possibly be no intersection
			return false;
		}
	}
	else
	{
		// both i and the other line are vertical.
		// Simply return false if the two line overlap with each other
		return false;
	}
}
//--------------------------------------------------------------------------------------------------
double TPLineSegment::getSlope(void) const
{
	double dY = m_p2.m_dY - m_p1.m_dY;
	double dX = m_p2.m_dX - m_p1.m_dX;

	// if the line is vertical, then return a slope of + or - infinity as appropriate.
	if (fabs(dX) < MathVars::ZERO)
	{
		return (dY / fabs(dY)) * MathVars::INFINITY;
	}
	else
	{
		return dY/dX;
	}
}
//--------------------------------------------------------------------------------------------------
TPPoint TPLineSegment::getMidTPPoint(void) const
{
	return TPPoint((m_p1.m_dX + m_p2.m_dX) / 2.0, (m_p1.m_dY + m_p2.m_dY) / 2.0);
}
//--------------------------------------------------------------------------------------------------
double TPLineSegment::getLength(void) const
{
	return sqrt(pow(m_p1.m_dX - m_p2.m_dX, 2) + pow(m_p1.m_dY - m_p2.m_dY, 2));
}
//--------------------------------------------------------------------------------------------------
bool operator == (const TPLineSegment& l1, const TPLineSegment& l2)
{
	if ((l1.m_p1 == l2.m_p1 && l1.m_p2 == l2.m_p2) ||
   		(l1.m_p1 == l2.m_p2 && l1.m_p2 == l2.m_p1))
	{
		return true;
	}
	else
	{
		return false;
	}
}
//--------------------------------------------------------------------------------------------------
bool operator < (const TPLineSegment& l1, const TPLineSegment& l2)
{
	if (l1.m_p1 < l2.m_p1)
	{
		return true;
	}
	else if (l1.m_p1 == l2.m_p1 && l1.m_p2 < l2.m_p2)
	{
		return true;
	}

	return false;
}