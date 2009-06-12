#include "stdafx.h"
#include "LoopCurve.h"

//--------------------------------------------------------------------------------------------------
LoopCurve::LoopCurve(const Point& _startPoint, const Point& _endPoint)
{
	startPoint = _startPoint;
	endPoint = _endPoint;
}
//--------------------------------------------------------------------------------------------------
LoopCurve::LoopCurve(const LoopCurve& curveToCopy)
{
	startPoint = curveToCopy.getStartPoint();
	endPoint = curveToCopy.getEndPoint();
}
//--------------------------------------------------------------------------------------------------
LoopCurve& LoopCurve::operator=(const LoopCurve& curveToAssign)
{
	startPoint = curveToAssign.getStartPoint();
	endPoint = curveToAssign.getEndPoint();

	return *this;
}
//--------------------------------------------------------------------------------------------------
const Point& LoopCurve::getStartPoint(void) const
{
	return startPoint;
}
//--------------------------------------------------------------------------------------------------
const Point& LoopCurve::getEndPoint(void) const
{
	return endPoint;
}
//--------------------------------------------------------------------------------------------------
