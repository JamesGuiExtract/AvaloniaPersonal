#pragma once

#include "BaseUtils.h"
#include "Point.h"

class EXPORT_BaseUtils LineSegment
{
public:
	LineSegment(const Point& _p1, const Point& _p2);

	LineSegment(const LineSegment& lineToCopy);

	LineSegment();

	LineSegment& operator=(const LineSegment& lineToAssign);

	bool contains(const Point& p) const;

	bool intersects(const LineSegment& line2,Point& intersectionPoint) const;

	EXPORT_BaseUtils friend bool operator == (const LineSegment& l1, const LineSegment& l2);
	
	double getSlope(void) const;

	Point getMidPoint(void) const;

	double getLength(void) const;

	bool startsOrEndsAt(const Point& p, bool& bStarts) const;

	inline const Point& LineSegment::getStartingPoint(void) const
	{
		return p1;
	}

	inline const Point& LineSegment::getEndingPoint(void) const
	{
		return p2;
	}

	EXPORT_BaseUtils friend bool operator == (const LineSegment& l1, const LineSegment& l2);

public:
	static const double ZERO_PLUS;

	Point p1, p2;
};