#pragma once

#include "BaseUtils.h"
#include "Points.h"
#include "LineSegment.h"

class EXPORT_BaseUtils LoopTrace : public Points
{
public:

	void addPoint(const Point& point);

	LoopTrace();

	const Point& getFirstPoint(void) const;

	const Point& getLastPoint(void) const;

	bool previousLastPointWas(const Point& point) const;

	bool containsLoop(const LoopTrace& loopTrace) const;

	// the default parameter is used for internal optimization.
	// when this method is called from outside this class, only 
	// call it with one parameter!
	bool encloses(const Point& point, bool bValueToReturnIfPointOnBorder, bool bCalculateExtents = true);

	bool enclosesAll(const Points& points,bool bValueToReturnIfPointOnBorder);

	bool enclosesOneOf(const Points& points, bool bValueToReturnIfPointOnBorder);

	EXPORT_BaseUtils friend bool operator == (const LoopTrace& a, const LoopTrace& b);

	const vector<LineSegment>& getSegments() const;

	const Point& getPreviousLastPoint() const;

	void closeLoop();

	void calculateExtents(void);

	// requires calculateExtents() to be called on each parameter before this function is called.
	EXPORT_BaseUtils friend bool operator < (const LoopTrace& loop1, const LoopTrace& loop2);

	void getExtents(double& _dMinX, double& _dMinY, double& _dMaxX, double& _dMaxY);

protected:

	double dMinX;
	double dMinY;
	double dMaxX;
	double dMaxY;

	vector<LineSegment> vecSegments;
};