#include "stdafx.h"
#include "LoopTrace.h"
#include "UCLIDException.h"

//--------------------------------------------------------------------------------------------------
const Point& LoopTrace::getFirstPoint(void) const
{
	if (empty())
		throw UCLIDException("ELI00488", "Cannot call LoopTrace::getFirstPoint() on a loop with zero points!");

	return (*this)[0];
}
//--------------------------------------------------------------------------------------------------
const Point& LoopTrace::getLastPoint(void) const
{
	if (empty())
		throw UCLIDException("ELI00489", "Cannot call LoopTrace::getLastPoint() on a loop with zero points!");

	return back();
}
//--------------------------------------------------------------------------------------------------
bool LoopTrace::previousLastPointWas(const Point& point) const
{
	unsigned long ulSize = size();

	if (ulSize < 2)
		return false;
	else
		return ((*this)[ulSize - 2]) == point;
}
//--------------------------------------------------------------------------------------------------
const Point& LoopTrace::getPreviousLastPoint() const
{
	unsigned long ulSize = size();

	if (ulSize < 2)
		throw UCLIDException("ELI00490", "Internal error: LoopTrace::getPreviousLastPoint() called!");
	else
		return ((*this)[ulSize - 2]);
}
//--------------------------------------------------------------------------------------------------
bool LoopTrace::containsLoop(const LoopTrace& loopTrace) const
{
	const_iterator inputLoopIter;
	for (inputLoopIter = loopTrace.begin(); inputLoopIter != loopTrace.end(); inputLoopIter++)
	{
		bool inputLoopItemValueFound = false;
		const_iterator thisLoopIter;
		for (thisLoopIter = begin(); thisLoopIter != end() && !inputLoopItemValueFound; thisLoopIter++)
		{
			if (*inputLoopIter == *thisLoopIter)
				inputLoopItemValueFound = true;
		}
		
		if (!inputLoopItemValueFound)
			return false;
	}

	return true;
}
//--------------------------------------------------------------------------------------------------
bool operator == (const LoopTrace& a, const LoopTrace& b)
{
	// TODO: make more efficient

	if (a.size() != b.size())
		return false;

	LoopTrace::const_iterator itera;
	for (itera = a.begin(); itera != a.end(); itera++)
	{
		bool iterAValueFoundInVectorB = false;
		LoopTrace::const_iterator iterb;
		for (iterb = b.begin(); iterb != b.end() && !iterAValueFoundInVectorB; iterb++)
		{
			if (*itera == *iterb)
				iterAValueFoundInVectorB = true;
		}
		
		if (!iterAValueFoundInVectorB)
			return false;
	}

	LoopTrace::const_iterator iterb;
	for (iterb = b.begin(); iterb != b.end(); iterb++)
	{
		bool iterBValueFoundInVectorA = false;
		LoopTrace::const_iterator itera;
		for (itera = a.begin(); itera != a.end() && !iterBValueFoundInVectorA; itera++)
		{
			if (*itera == *iterb)
				iterBValueFoundInVectorA = true;
		}

		if (!iterBValueFoundInVectorA)
			return false;
	}

	return true;
}
//--------------------------------------------------------------------------------------------------
const vector<LineSegment>& LoopTrace::getSegments() const
{
	return vecSegments;
}
//--------------------------------------------------------------------------------------------------
void LoopTrace::closeLoop()
{
	addPoint(getFirstPoint());
}
//--------------------------------------------------------------------------------------------------
LoopTrace::LoopTrace()
{
	dMinX = dMinY = dMaxX = dMaxY = 0.0;
}
//--------------------------------------------------------------------------------------------------
void LoopTrace::calculateExtents(void)
{
	dMinX = HUGE_VAL;
	dMinY = HUGE_VAL;
	dMaxX = -HUGE_VAL;
	dMaxY = -HUGE_VAL;

	vector<LineSegment>::const_iterator iter;
	for (iter = vecSegments.begin(); iter != vecSegments.end(); iter++)
	{
		LineSegment currSegment = *iter;

		// p1
		if (currSegment.p1.getX() < dMinX)
			dMinX = currSegment.p1.getX();

		if (currSegment.p1.getY() < dMinY)
			dMinY = currSegment.p1.getY();

		if (currSegment.p1.getX() > dMaxX)
			dMaxX = currSegment.p1.getX();

		if (currSegment.p1.getY() > dMaxY)
			dMaxY = currSegment.p1.getY();

		// p2
		if (currSegment.p2.getX() < dMinX)
			dMinX = currSegment.p2.getX();

		if (currSegment.p2.getY() < dMinY)
			dMinY = currSegment.p2.getY();

		if (currSegment.p2.getX() > dMaxX)
			dMaxX = currSegment.p2.getX();

		if (currSegment.p2.getY() > dMaxY)
			dMaxY = currSegment.p2.getY();
	}
}
//--------------------------------------------------------------------------------------------------
bool LoopTrace::encloses(const Point& targetPoint, bool bValueToReturnIfPointOnBorder, 
						 bool bCalculateExtents)
{
	// first see if the target point is on the boundary itself
	{
		vector<LineSegment>::const_iterator iter;
		for (iter = vecSegments.begin(); iter != vecSegments.end(); iter++)
		{
			const LineSegment& currSegment = *iter;

			// if the point is on the current line segment, then return 
			// what ever value the user wanted returned.
			if (currSegment.contains(targetPoint))
				return bValueToReturnIfPointOnBorder;
		}
	}

	// calculate the bounds of the boundary if asked to do so
	if (bCalculateExtents)
		calculateExtents();
	
	// create a horizontal line segment that spans horizontally to the right of the specified point
	LineSegment horizontalSegment(Point(targetPoint.getX(), targetPoint.getY()),
								  Point(dMaxX, targetPoint.getY()));

	// create a vertical line segment that spans vertically top the top of the specified point
	
	LineSegment verticalSegment(Point(targetPoint.getX(), targetPoint.getY()),
								Point(targetPoint.getX(), dMaxY));

	// look at the number of times that the horizontal and vertical segments cross the boundary
	Points horizontalIntersectionPoints;
	Points verticalIntersectionPoints;
	int iHorz = 0, iVert = 0;
	{
		vector<LineSegment>::const_iterator iter;
		for (iter = vecSegments.begin(); iter != vecSegments.end(); iter++)
		{
			LineSegment currSegment = *iter;

			Point intersectionPoint;

			if (currSegment.intersects(horizontalSegment, intersectionPoint))
			{
				// if the intersection point has not yet been found, then add it to the list of
				// points that were found and increment the counter
				if (!horizontalIntersectionPoints.contains(intersectionPoint))
				{
					horizontalIntersectionPoints.addPoint(intersectionPoint);
					iHorz++;
				}
			}

			if (currSegment.intersects(verticalSegment, intersectionPoint))
			{
				// if the intersection point has not yet been found, then add it to the list of
				// points that were found and increment the counter
				if (!verticalIntersectionPoints.contains(intersectionPoint))
				{
					verticalIntersectionPoints.addPoint(intersectionPoint);
					iVert++;
				}
			}
		}
	}

	// the point lies within the boundary if the number of horizontal and vertical intersections
	// are both odd numbers
	return (iHorz > 0)  && (iVert > 0) && (iHorz % 2 == 1) && (iVert % 2 == 1);
}
//--------------------------------------------------------------------------------------------------
bool LoopTrace::enclosesAll(const Points& points, bool bValueToReturnIfPointOnBorder)
{
	calculateExtents();

	Points::const_iterator pointIter;
	for (pointIter = points.begin(); pointIter != points.end(); pointIter++)
	{
		// if one of the specified points is not enclosed in the given loop
		// then return false;
		if (!encloses(*pointIter, bValueToReturnIfPointOnBorder, false))
			return false;
	}

	// all of the specified points are enclosed in the given loop
	return true;
}
//--------------------------------------------------------------------------------------------------
bool LoopTrace::enclosesOneOf(const Points& points, bool bValueToReturnIfPointOnBorder)
{
	calculateExtents();

	Points::const_iterator pointIter;
	for (pointIter = points.begin(); pointIter != points.end(); pointIter++)
	{
		// if one of the specified points is enclosed in the given loop
		// then return true;
		if (encloses(*pointIter, bValueToReturnIfPointOnBorder, false))
			return true;
	}

	// none of the specified points are enclosed in the given loop
	return false;
}
//--------------------------------------------------------------------------------------------------
void LoopTrace::addPoint(const Point& point)
{
	if (!empty())
	{
		// create a new line segment from the last point to the current point, 
		// if there exists a last point
		vecSegments.push_back(LineSegment(back(), point));
	}

	// add the point to the underlying vector
	Points::addPoint(point);
}
//--------------------------------------------------------------------------------------------------
bool operator < (const LoopTrace& loop1, const LoopTrace& loop2)
{
	// sort loops based upon the extents as follows: from left to right, top to bottom.
	if (loop1.dMinX != loop2.dMinX)
	{
		// the x's are not equal - sort left to right
		return loop1.dMinX < loop2.dMinX;
	}
	else
	{
		// the x min's are equal...
		if (loop1.dMaxY != loop2.dMaxY)
		{
			// the maxy's are not equal...sort top to bottom
			return loop1.dMaxY > loop2.dMaxY;
		}
		else
		{
			// the x'mins and ymax's are equal 
			if (loop1.dMaxX != loop2.dMaxX)
			{
				// sort by horizontal width
				return loop1.dMaxX < loop2.dMaxX;
			}
			else
			{
				// the horizontal widths are the same - sort by vertical height
				return loop1.dMinY > loop2.dMinY;
			}
		}
	}
}
//--------------------------------------------------------------------------------------------------
void LoopTrace::getExtents(double& _dMinX, double& _dMinY, double& _dMaxX, double& _dMaxY)
{
	_dMinX = dMinX;
	_dMinY = dMinY;
	_dMaxX = dMaxX;
	_dMaxY = dMaxY;
}
//--------------------------------------------------------------------------------------------------
				