//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	TPPolygon.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Arvind Ganesan
//
//==================================================================================================

#ifndef TPPolygon_H
#define TPPolygon_H

#include "TopoUtils.h"
#include "TPLineSegment.h"

#include <vector>

#pragma warning (disable: 4231)

// TESTTHIS Warning given for exporting STL template class 
// Please refer to Q168958 - HOWTO: Exporting STL Components Inside & Outside of a class
// http://support.microsoft.com/kb/q168958/
EXPIMP_TEMPLATE_TOPOUTILS template class EXPORT_TopoUtils std::vector<TPPoint>;
EXPIMP_TEMPLATE_TOPOUTILS template class EXPORT_TopoUtils std::vector<TPLineSegment>;

class EXPORT_TopoUtils TPPolygon
{
public:
	TPPolygon();
	// Add point squentially
	void addPoint(const TPPoint& point);

	bool contains(const TPPolygon& poly) const;
	bool overlaps(const TPPolygon& poly) const;
	bool encloses(const TPPoint& point, bool bValueToReturnIfPointOnBorder) const;

	// When you export an STL container parameterized with a user-defined type (UDT),
	// you must define the operators < and  == for your UDT.
	EXPORT_TopoUtils friend bool operator == (const TPPolygon& a, const TPPolygon& b);
	EXPORT_TopoUtils friend bool operator < (const TPPolygon& loop1, const TPPolygon& loop2);

	void getExtents(double& rdMinX, double& rdMinY, double& rdMaxX, double& rdMaxY);

	// Get the line segments of the polygon
	const std::vector<TPLineSegment> getSegments() const;

	// Get the intersection area of the two polygon
	// NOTE: this method should only be called for Convex polygons!!!!
	// NOTE: this method will reorder the points associated with the polygon
	//		assuming that this polygon is a convex polygon
	double getIntersectionArea(const TPPolygon& polySecond);

	// Number of points in the polygon
	long getNumOfVertex() const;

	// Get the area of the polygon
	double getArea() const;

protected:
	// Methods
	void calculateExtents(const TPPoint& p);

	// Reorder the points in the polygon to make it does not intersect with itself
	void reOrderPointsInPolygon();

	// Variables
	double m_dMinX;
	double m_dMinY;
	double m_dMaxX;
	double m_dMaxY;

	std::vector<TPPoint> m_vecPoints;
};
#endif // TPPolygon_H