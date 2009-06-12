#pragma once

#include "BaseUtils.h"
#include "Point.h"

class EXPORT_BaseUtils LoopCurve
{
public:

	LoopCurve(const Point& startPoint, const Point& endPoint);
	LoopCurve(const LoopCurve& curveToCopy);
	LoopCurve& operator = (const LoopCurve& curveToAssign);

	const Point& getStartPoint(void) const;
	const Point& getEndPoint(void) const;

private:
	Point startPoint, endPoint;
};