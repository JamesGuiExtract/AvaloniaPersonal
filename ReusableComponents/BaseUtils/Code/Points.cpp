#include "stdafx.h"
#include "Points.h"

#include <algorithm>

//--------------------------------------------------------------------------------------------------
void Points::addPoint(const Point& point)
{
	push_back(point);
}
//--------------------------------------------------------------------------------------------------
bool Points::contains(const Point& point) const
{
	return find(begin(), end(), point) != end();
}
//--------------------------------------------------------------------------------------------------
Points operator && (const Points& p1, const Points& p2)
{
	Points commonPoints;

	Points::const_iterator p1Iter;
	
	for (p1Iter = p1.begin(); p1Iter != p1.end(); p1Iter++)
	{
		Points::const_iterator p2Iter;

		for (p2Iter = p2.begin(); p2Iter != p2.end(); p2Iter++)
		{

			if (*p1Iter == *p2Iter)
				if (!commonPoints.contains(*p1Iter))
					commonPoints.push_back(*p1Iter);
		}

	}

	return commonPoints;
}
//--------------------------------------------------------------------------------------------------