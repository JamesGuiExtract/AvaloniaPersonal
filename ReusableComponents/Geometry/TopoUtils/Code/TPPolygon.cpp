//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	TPPolygon.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan
//
//==================================================================================================

#include "stdafx.h"
#include "TPPolygon.h"
#include "HelperClasses.h"

#include <mathUtil.h>
#include <UCLIDException.h>

#include <algorithm>

using namespace std;

//--------------------------------------------------------------------------------------------------
// initialize the max to 0.0 and the min to INFINITY
TPPolygon::TPPolygon() :
m_dMinX(HUGE_VAL),
m_dMinY(HUGE_VAL),
m_dMaxX(0.0),
m_dMaxY(0.0)
{
}
//--------------------------------------------------------------------------------------------------
void TPPolygon::addPoint(const TPPoint& point)
{
	// add the point to the underlying vector
	m_vecPoints.push_back(point);

	// recalculate the extents
	calculateExtents(point);
}
//--------------------------------------------------------------------------------------------------
bool TPPolygon::contains(const TPPolygon& poly) const
{
	vector<TPPoint>::const_iterator inputTPPolygonIter;

	for (inputTPPolygonIter = poly.m_vecPoints.begin(); 
		inputTPPolygonIter != poly.m_vecPoints.end(); 
		inputTPPolygonIter++)
	{
		if (!encloses(*inputTPPolygonIter, true))
		{
			return false;
		}
	}

	return true;
}
//--------------------------------------------------------------------------------------------------
bool TPPolygon::encloses(const TPPoint& targetPoint, bool bValueToReturnIfPointOnBorder) const
{
	// first see if the target point is on the boundary itself
	{
		// get the vector of line segments
		const vector<TPLineSegment> vecSegments = getSegments();

		// for all line segments, check if the segment includes the
		// targetPoint
		for (unsigned int i = 0; i < vecSegments.size(); i++)
		{
			const TPLineSegment& currSegment = vecSegments[i];

			// if the point is on the current line segment, then return 
			// what ever value the user wanted returned.
			if (currSegment.contains(targetPoint))
			{
				return bValueToReturnIfPointOnBorder;
			}
		}
	}
	
	// create a horizontal line segment that spans horizontally to the right of the specified point
	TPLineSegment horizontalSegment(TPPoint(targetPoint.m_dX, targetPoint.m_dY),
		TPPoint(m_dMaxX, targetPoint.m_dY));

	// create a vertical line segment that spans vertically top the top of the specified point
	
	TPLineSegment verticalSegment(TPPoint(targetPoint.m_dX, targetPoint.m_dY), 
		TPPoint(targetPoint.m_dX, m_dMaxY));

	// look at the number of times that the horizontal and vertical segments cross the boundary
	vector<TPPoint> horizontalIntersectionPoints;
	vector<TPPoint> verticalIntersectionPoints;
	int iHorz = 0, iVert = 0;
	{
		for (unsigned int i = 0; i < getSegments().size(); i++)
		{
			TPLineSegment currSegment = getSegments()[i];

			TPPoint intersectionPoint;

			if (currSegment.intersects(horizontalSegment, intersectionPoint))
			{
				// if the intersection point has not yet been found, then add it to the list of
				// points that were found and increment the counter
				if (find(horizontalIntersectionPoints.begin(), horizontalIntersectionPoints.end(),
					intersectionPoint) == horizontalIntersectionPoints.end())
				{
					horizontalIntersectionPoints.push_back(intersectionPoint);
					iHorz++;
				}
			}

			if (currSegment.intersects(verticalSegment, intersectionPoint))
			{
				// if the intersection point has not yet been found, then add it to the list of
				// points that were found and increment the counter
				if (find(verticalIntersectionPoints.begin(), verticalIntersectionPoints.end(),
					intersectionPoint) == verticalIntersectionPoints.end())
				{
					verticalIntersectionPoints.push_back(intersectionPoint);
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
bool TPPolygon::overlaps(const TPPolygon& poly) const
{
	TPPoint dummyIntersectionPoint;

	// TPPolygon A overlaps TPPolygon B if any of their lines intersect.
	for (unsigned int i = 0; i < poly.getSegments().size(); i++)
	{
		for (unsigned int j = 0; j < getSegments().size(); j++)
		{
			if (getSegments()[j].intersects(poly.getSegments()[i], 
				dummyIntersectionPoint))
			{
				return true;
			}
		}
	}

	// TPPolygon A overlaps TPPolygon B if either one is contained in the other
	if (contains(poly) || poly.contains(*this))
	{
		return true;
	}

	return false;
}
//--------------------------------------------------------------------------------------------------
bool operator == (const TPPolygon& a, const TPPolygon& b)
{
	// TODO: make more efficient

	if (a.m_vecPoints.size() != b.m_vecPoints.size())
	{
		return false;
	}

	vector<TPPoint>::const_iterator itera;
	for (itera = a.m_vecPoints.begin(); itera != a.m_vecPoints.end(); itera++)
	{
		bool iterAValueFoundInVectorB = false;
		vector<TPPoint>::const_iterator iterb;
		for (iterb = b.m_vecPoints.begin(); iterb != b.m_vecPoints.end() && !iterAValueFoundInVectorB; iterb++)
		{
			if (*itera == *iterb)
			{
				iterAValueFoundInVectorB = true;
			}
		}
		
		if (!iterAValueFoundInVectorB)
		{
			return false;
		}
	}

	vector<TPPoint>::const_iterator iterb;
	for (iterb = b.m_vecPoints.begin(); iterb != b.m_vecPoints.end(); iterb++)
	{
		bool iterBValueFoundInVectorA = false;
		vector<TPPoint>::const_iterator itera;
		for (itera = a.m_vecPoints.begin(); itera != a.m_vecPoints.end() && !iterBValueFoundInVectorA; itera++)
		{
			if (*itera == *iterb)
			{
				iterBValueFoundInVectorA = true;
			}
		}

		if (!iterBValueFoundInVectorA)
		{
			return false;
		}
	}

	return true;
}
//--------------------------------------------------------------------------------------------------
bool operator < (const TPPolygon& TPPolygon1, const TPPolygon& TPPolygon2)
{
	// sort TPPolygons based upon the extents as follows: from left to right, top to bottom.
	if (TPPolygon1.m_dMinX != TPPolygon2.m_dMinX)
	{
		// the x's are not equal - sort left to right
		return TPPolygon1.m_dMinX < TPPolygon2.m_dMinX;
	}
	else
	{
		// the x min's are equal...
		if (TPPolygon1.m_dMaxY != TPPolygon2.m_dMaxY)
		{
			// the maxy's are not equal...sort top to bottom
			return TPPolygon1.m_dMaxY > TPPolygon2.m_dMaxY;
		}
		else
		{
			// the x'mins and ymax's are equal 
			if (TPPolygon1.m_dMaxX != TPPolygon2.m_dMaxX)
			{
				// sort by horizontal width
				return TPPolygon1.m_dMaxX < TPPolygon2.m_dMaxX;
			}
			else
			{
				// the horizontal widths are the same - sort by vertical height
				return TPPolygon1.m_dMinY > TPPolygon2.m_dMinY;
			}
		}
	}
	
	// if all of the above failed, the TPPolygons must be the same therfore return false
	return false;
}
//--------------------------------------------------------------------------------------------------
double TPPolygon::getIntersectionArea(const TPPolygon& polySecond)
{
	try
	{
		// Get the points and segments vector of polySecond
		const vector<TPPoint>& vecPointsSecond = polySecond.m_vecPoints;
		const vector<TPLineSegment>& vecSegmentsSecond = polySecond.getSegments();

		// Intersection polygon
		TPPolygon polyIntersection;

		// Get the points of the second polygon that is inside the first polygon
		for (unsigned int i = 0; i < vecPointsSecond.size(); i++)
		{
			// Initialize bIfPointOnBorder to true
			// so that the point on border will be treated as inside polygon
			bool bIfPointOnBorder = true;

			// Check if one point of the second polygon is inside the first polygon
			bool bInside = encloses(vecPointsSecond[i], bIfPointOnBorder);

			if (bInside)
			{
				// Add this point to collection of intersection polygon vertices
				polyIntersection.addPoint(vecPointsSecond[i]);
			}
		}

		// Get the points of the first polygon that is inside the second polygon
		for (unsigned int i = 0; i < m_vecPoints.size(); i++)
		{
			// Initialize bIfPointOnBorder to true
			// so that the point on border will be treated as inside polygon
			bool bIfPointOnBorder = true;

			// Check if one point of the first polygon is inside the second polygon
			bool bInside = polySecond.encloses(m_vecPoints[i], bIfPointOnBorder);

			if (bInside)
			{
				// Add this point to collection of intersection polygon vertices
				polyIntersection.addPoint(m_vecPoints[i]);
			}
		}

		// Go through each line segment from one polygon and find all the intersection 
		// points with line segments from the other polygon
		for (unsigned int i = 0; i < getSegments().size(); i++)
		{
			for (unsigned int j = 0; j < vecSegmentsSecond.size(); j++)
			{
				// Get intersect point
				TPPoint interPoint;
				bool bResult = getSegments()[i].intersects(vecSegmentsSecond[j], interPoint);

				if (bResult)
				{
					// If the two lines intersect, add the intersection point to 
					// collection of intersection polygon vertices
					polyIntersection.addPoint(interPoint);
				}
			}
		}

		// Add the closing segment
		// polyIntersection.close();

		// Define the area of the intersection polygon
		double dAreaOfIntersection = 0;

		// The intersection should have at least three lines to
		// form a polygon
		if (polyIntersection.getNumOfVertex() > 2)
		{
			// Call to make all the points form a convex polygon
			// and the points are set to counterclock-wise order
			polyIntersection.reOrderPointsInPolygon();

			// Get the area of the intersection polygon 
			dAreaOfIntersection = polyIntersection.getArea();
		}

		return dAreaOfIntersection;
	}
	catch(...)
	{
		UCLIDException ue("ELI15072", "Error calculating the intersection area of two polygons.");
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
long TPPolygon::getNumOfVertex() const
{
	return m_vecPoints.size();
}
//--------------------------------------------------------------------------------------------------
void TPPolygon::getExtents(double& rdMinX, double& rdMinY, double& rdMaxX, double& rdMaxY)
{
	rdMinX = m_dMinX;
	rdMinY = m_dMinY;
	rdMaxX = m_dMaxX;
	rdMaxY = m_dMaxY;
}
//--------------------------------------------------------------------------------------------------
const vector<TPLineSegment> TPPolygon::getSegments() const
{
	vector<TPLineSegment> vecSegments;
	if (m_vecPoints.size() >= 3)
	{
		for (unsigned int i = 0; i < m_vecPoints.size(); i++)
		{
			// The second point
			int j = i + 1;

			// if i is the last point, then the second
			// point should be the first point in m_vecPoints;
			if (i == m_vecPoints.size() - 1)
			{
				j = 0;
			}

			// create a new line segment
			vecSegments.push_back(TPLineSegment(m_vecPoints[i], m_vecPoints[j]));
		}
	}
	else
	{
		UCLIDException ue("ELI15086", 
			"A polygon should have at least 3 vertices.");
		throw ue;
	}

	return vecSegments;
}
//--------------------------------------------------------------------------------------------------
double TPPolygon::getArea() const
{
    double dArea = 0.0;
	
	// Get the number of lines inside the polygon
	long nLines = getNumOfVertex();

	// Project lines from each vertex to some horizontal line below the lowest part of the polygon. 
	// The enclosed region from each line segment is made up of a triangle and rectangle.
	// Sum these areas together noting that the areas outside the polygon eventually 
	// cancel as the polygon loops around to the beginning.

	// Note: The only restriction that will be placed on the polygon for this technique to work is 
	// that the polygon must not be self intersecting

	// Area = 1/2 * sum(x[i]*y[i+1] - x[i+1]*y[i]) (i = 0...nLines - 2)

	// for detailed description, please refer to:
	// http://local.wasp.uwa.edu.au/~pbourke/geometry/polyarea/
	for (long i = 0; i < nLines; i++) 
	{
		long j = (i + 1) % nLines;
		dArea += m_vecPoints[i].m_dX * m_vecPoints[j].m_dY;
		dArea -= m_vecPoints[i].m_dY * m_vecPoints[j].m_dX;
    }
    dArea /= 2.0;

    return (dArea < 0.0 ? -dArea : dArea);
}

//--------------------------------------------------------------------------------------------------
// Protected methods
//--------------------------------------------------------------------------------------------------
void TPPolygon::calculateExtents(const TPPoint& p)
{
	if (m_vecPoints.empty())
	{
		m_dMinX = HUGE_VAL;
		m_dMinY = HUGE_VAL;
		m_dMaxX = -HUGE_VAL;
		m_dMaxY = -HUGE_VAL;
	}

	if (p.m_dX < m_dMinX)
	{
		m_dMinX = p.m_dX;
	}

	if (p.m_dY < m_dMinY)
	{
		m_dMinY = p.m_dY;
	}

	if (p.m_dX > m_dMaxX)
	{
		m_dMaxX = p.m_dX;
	}

	if (p.m_dY > m_dMaxY)
	{
		m_dMaxY = p.m_dY;
	}
}
//--------------------------------------------------------------------------------------------------
void TPPolygon::reOrderPointsInPolygon()
{
	YValueIsSmaller sorter1;

	// Get the point whose Y coordinate is the smallest
	TPPoint minYPoint = *min_element(m_vecPoints.begin(), m_vecPoints.end(), sorter1);

	CosValueIsSmaller sorter2(minYPoint);

	// reorder the pointers according to the cos value with m_MinYInPolygon 
	sort(m_vecPoints.begin(), m_vecPoints.end(), sorter2);
}
//--------------------------------------------------------------------------------------------------
