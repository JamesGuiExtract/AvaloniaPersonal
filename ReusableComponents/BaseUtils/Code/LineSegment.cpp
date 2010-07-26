#include "stdafx.h"
#include "LineSegment.h"

#include <math.h>
#include <stdlib.h>

#include <vector>
#include <string>
using namespace std;


const double LineSegment::INFINITY = 1.7E307;
const double LineSegment::ZERO_PLUS = 1E-8;

//--------------------------------------------------------------------------------------------------
LineSegment::LineSegment()
{
}
//--------------------------------------------------------------------------------------------------
LineSegment::LineSegment(const Point& _p1, const Point& _p2)
{
	p1 = _p1;
	p2 = _p2;
}
//--------------------------------------------------------------------------------------------------
LineSegment::LineSegment(const LineSegment& lineToCopy)
{
	p1 = lineToCopy.p1;
	p2 =  lineToCopy.p2;
}
//--------------------------------------------------------------------------------------------------
LineSegment& LineSegment::operator=(const LineSegment& lineToAssign)
{
	p1 = lineToAssign.p1;
	p2 =  lineToAssign.p2;
	return *this;
}
//--------------------------------------------------------------------------------------------------
bool LineSegment::startsOrEndsAt(const Point& p, bool& bStarts) const
{
	bStarts = p1 == p;
	return (bStarts) || (p2 == p);
}
//--------------------------------------------------------------------------------------------------
bool LineSegment::contains(const Point& p) const
{
	double dMySlope = getSlope();

	if (fabs(dMySlope) == INFINITY)
	{
		// this line is vertical.. the point can lie on this line only if the x
		// values match
		if (fabs(p1.getX() - p.getX()) < ZERO_PLUS)
		{
			// the target point is on the imaginary line that extends infinitely, 
			// but we need to check to see if it is in the line segment
			if (p.getY() >= min(p1.getY(), p2.getY()) &&
				p.getY() <= max(p1.getY(), p2.getY()))
			{
				// the point lies on the line segment
				return true;
			}
			else
			{
				// the point lies in the imaginary line but not on the line segment
				return false;
			}
		}
		else
		{
			// this line is vertical, and the target point has a different x value
			// than this line, so it couldn't be on this line
			return false;
		}
	}
	else
	{
		// find the point on the line segment extended infinitely that
		// is directly above or below the given point
		double dY = p1.getY() + dMySlope * (p.getX() - p1.getX());
		Point pointOnLine(p.getX(), dY);
		
		if (pointOnLine == p)
		{
			// the specified point is someone on the imaginary line, but
			// is it on the line segment?
			if (dY >= min(p1.getY(), p2.getY()) &&
				dY <= max(p1.getY(), p2.getY()))
			{
				// the point lies on the line segment
				return true;
			}
			else
			{
				// the point lies on the imaginary line, but not on the line segment
				return false;
			}

		}
		else
		{
			// the given point is not even on the imaginary line
			return false;
		}
	}
}
//--------------------------------------------------------------------------------------------------
bool LineSegment::intersects(const LineSegment& line2, Point& intersectionPoint) const
{
	double dMySlope = getSlope();
	double dLine2Slope = line2.getSlope();

	if (fabs(dMySlope) != INFINITY && fabs(dLine2Slope) != INFINITY)
	{
		// neither of the two lines are vertical

		// suppose my staring point was p1, and s1 was my slope, and
		// the other segment's starting point was p2, and its slope was s2,
		// suppose the intersection point was (ix, iy)
		// then the intersection point Y = p1.y + s1 (ix - p1.x)
		//                       also, Y = p2.y + s2 (ix - p2.x)
		// solving for ix, we get
		// ix = (p2.y - p1.y - s2 * p2.x + s1 * p1.x) / (s1 - s2);

		// before we attempt to calculate the intersection point, we should make sure that 
		// the two lines are not parallel
		if (fabs(dMySlope - dLine2Slope) < ZERO_PLUS)
		{
			// the two lines are parallel, but they could still be intersecting because
			// they can have some points in common
			// TODO: for now return false;
			return false;
		}

		// calculate the intersection point X and Y
		double dIntersectionPointX = (line2.p1.getY() - p1.getY() - dLine2Slope * line2.p1.getX() + dMySlope * p1.getX()) / (dMySlope - dLine2Slope);
		double dIntersectionPointY = p1.getY() + dMySlope * (dIntersectionPointX - p1.getX());
		intersectionPoint = Point(dIntersectionPointX, dIntersectionPointY);

		// there's intersection only if the intersection point's Y 
		// is within my Y's and within line2's Y's
//		bool b1 = dIntersectionPointY - min(p1.getY(), p2.getY()) >= -ZERO_PLUS;
//		bool b2 = max(p1.getY(), p2.getY()) - dIntersectionPointY >= -ZERO_PLUS;
//		bool b3 = dIntersectionPointY - min(line2.p1.getY(), line2.p2.getY()) >= -ZERO_PLUS;
//		bool b4 = max(line2.p1.getY(), line2.p2.getY()) - dIntersectionPointY >= -ZERO_PLUS;

//		double d1 = dIntersectionPointY - min(p1.getY(), p2.getY());
//		double d2 = max(p1.getY(), p2.getY()) - dIntersectionPointY;
//		double d3 = dIntersectionPointY - min(line2.p1.getY(), line2.p2.getY());
//		double d4 = max(line2.p1.getY(), line2.p2.getY()) - dIntersectionPointY;

		bool bIntersectionYOK = false;
		if ((dIntersectionPointY - min(p1.getY(), p2.getY()) >= -ZERO_PLUS) &&
			(max(p1.getY(), p2.getY()) - dIntersectionPointY >= -ZERO_PLUS) &&
			(dIntersectionPointY - min(line2.p1.getY(), line2.p2.getY()) >= -ZERO_PLUS) &&
			(max(line2.p1.getY(), line2.p2.getY()) - dIntersectionPointY >= -ZERO_PLUS))
		{
			bIntersectionYOK = true;
		}

		// there's intersection only if the intersection point's Y 
		// is within my Y's and within line2's Y's
		bool bIntersectionXOK = false;
		if ((dIntersectionPointX - min(p1.getX(), p2.getX()) >= -ZERO_PLUS) &&
			(max(p1.getX(), p2.getX()) - dIntersectionPointX >= -ZERO_PLUS) &&
			(dIntersectionPointX - min(line2.p1.getX(), line2.p2.getX()) >= -ZERO_PLUS) &&
			(max(line2.p1.getX(), line2.p2.getX()) - dIntersectionPointX >= -ZERO_PLUS))
		{
			bIntersectionXOK = true;
		}

//		double d = min(line2.p1.getY(), line2.p2.getY());

		// there's intersection only if the intersection point is already part
		// of the two line segments
		return bIntersectionXOK && bIntersectionYOK;
	}
	else if (fabs(dMySlope) != INFINITY)
	{
		// i am not vertical but line 2 is.

		// if line2's x is inbetween my starting and ending x's, there's a chance 
		// that we intersesect
		if (line2.p1.getX() >= min(p1.getX(), p2.getX()) && 
			line2.p1.getX() <= max(p1.getX(), p2.getX()))
		{
			// calculate the intersection point
			double dIntersectionPointY = p1.getY() + (line2.p1.getX() - p1.getX()) * getSlope();
			intersectionPoint = Point(line2.p1.getX(), dIntersectionPointY);

			// line2's x is in the range of my x, but there's intersection only if 
			// the intersection point's Y is within my Y's and within line2's Y's
			if ((dIntersectionPointY - min(p1.getY(), p2.getY()) >= -ZERO_PLUS) &&
				(max(p1.getY(), p2.getY()) - dIntersectionPointY >= -ZERO_PLUS) &&
				(dIntersectionPointY - min(line2.p1.getY(), line2.p2.getY()) >= -ZERO_PLUS) &&
				(max(line2.p1.getY(), line2.p2.getY()) - dIntersectionPointY >= -ZERO_PLUS))
				return true;
			else
				return false;
		}
		else
		{
			// my x is not in the range of x's of line2, so there could
			// possibly be no intersection
			return false;
		}

	}
	else if (fabs(dLine2Slope) != INFINITY)
	{
		// i am vertical, but line 2 is not

		// if my x is inbetween the starting and ending x's of line2, there's a chance 
		// that we intersesect
		if (p1.getX() >= min(line2.p1.getX(), line2.p2.getX()) && 
			p1.getX() <= max(line2.p1.getX(), line2.p2.getX()))
		{
			// calculate the intersection point
			double dIntersectionPointY = line2.p1.getY() + (p1.getX() - line2.p1.getX()) * line2.getSlope();
			intersectionPoint = Point(p1.getX(), dIntersectionPointY);

			// my x is in the range of line2's x, but there's intersection only if 
			// the intersection point's Y is within my Y's and within line2's Y's
			if ((dIntersectionPointY - min(p1.getY(), p2.getY()) >= -ZERO_PLUS) &&
				(max(p1.getY(), p2.getY()) - dIntersectionPointY >= -ZERO_PLUS) &&
				(dIntersectionPointY - min(line2.p1.getY(), line2.p2.getY()) >= -ZERO_PLUS) &&
				(max(line2.p1.getY(), line2.p2.getY()) - dIntersectionPointY >= -ZERO_PLUS))
				return true;
			else
				return false;

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
		
		// if the x's are different, then we don't intersect
		if (p1.getX() != line2.p1.getX())
			return false;
		else
		{
			// the x's are the same, but that does not mean that the line
			// segments intersect because the line segments can be on top
			// of each other.
			if (min(p1.getY(), p2.getY()) > min(line2.p1.getY(), line2.p2.getY()) &&
				max(p1.getY(), p2.getY()) < max(line2.p1.getY(), line2.p2.getY()))
			{
				// TODO: what to do with the intesection point????
				return true;
			}
			else
				return false;
		}
	}
}
//--------------------------------------------------------------------------------------------------
double LineSegment::getSlope(void) const
{
	double dY = p2.getY() - p1.getY();
	double dX = p2.getX() - p1.getX();

	// if the line is vertical, then return a slope of + or - infinity as appropriate.
	if (fabs(dX) < ZERO_PLUS)
		return (dY / fabs(dY)) * INFINITY;
	else
		return dY/dX;

}
//--------------------------------------------------------------------------------------------------
Point LineSegment::getMidPoint(void) const
{
	return Point((p1.getX() + p2.getX()) / 2.0, (p1.getY() + p2.getY()) / 2.0);
}
//--------------------------------------------------------------------------------------------------
bool operator == (const LineSegment& l1, const LineSegment& l2)
{
	if ((l1.p1 == l2.p1 && l1.p2 == l2.p2) ||
   		(l1.p1 == l2.p2 && l1.p2 == l2.p1))
	{
		return true;
	}
	else
	{
		return false;
	}
}
//--------------------------------------------------------------------------------------------------
double LineSegment::getLength(void) const
{
	return sqrt(pow(p1.getX() - p2.getX(), 2) + pow(p1.getY() - p2.getY(), 2));
}
//--------------------------------------------------------------------------------------------------
