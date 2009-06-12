#include "HelperClasses.h"

#include <UCLIDException.h>

//--------------------------------------------------------------------------------------------------
// class YValueIsSmaller
//--------------------------------------------------------------------------------------------------
bool YValueIsSmaller::operator()(const TPPoint& p1, const TPPoint& p2)
{
	// Return true if p1's Y coord is smaller than p2's Y coord
	return p1.m_dY < p2.m_dY;
}

///--------------------------------------------------------------------------------------------------
// class CosValueIsSmaller
//--------------------------------------------------------------------------------------------------
CosValueIsSmaller::CosValueIsSmaller(const TPPoint& minYPoint)
:m_MinYPoint(minYPoint)
{
}
//--------------------------------------------------------------------------------------------------
bool CosValueIsSmaller::operator()(const TPPoint& p1, const TPPoint& p2)
{
	// the cosine value of the first point with m_MinYPoint
	double dCosValue1 = calCosine(p1);

	// the cosine value of the first point with m_MinYPoint
	double dCosValue2 = calCosine(p2);

	return dCosValue1 < dCosValue2;
}
//--------------------------------------------------------------------------------------------------
double CosValueIsSmaller::calCosine(const TPPoint& point)
{
	// Get the length between one point with the point who has minimum y value
	double dLength = point.distanceTo(m_MinYPoint);

	// Initialize the cos value to some impossibly large value
	// so that if the point that needs to be compared is m_MinYPoint itself
	// its cos value will be the largest and it will be the last point in the 
	// polygon of intersection
	double dCosValue = 2;

	// If it is not the same point, get the cos value of the angle of the line
	// whose starting point is m_MinYPoint and end point is p2
	if (dLength > 0.00001)
	{
		dCosValue = (point.m_dX - m_MinYPoint.m_dX)/dLength;
		if (dCosValue > 1 || dCosValue < -1 )
		{
			UCLIDException ue("ELI15073", 
				"Cosine value of an angle between two points must be within [-1, 1].");
			ue.addDebugInfo("CosValue", dCosValue);
			ue.addDebugInfo("point1.m_dX", point.m_dX);
			ue.addDebugInfo("point1.m_dY", point.m_dY);
			ue.addDebugInfo("m_MinYPoint.m_dX", m_MinYPoint.m_dX);
			ue.addDebugInfo("m_MinYPoint.m_dX", m_MinYPoint.m_dY);
			throw ue;
		}
	}

	return dCosValue;
}
//--------------------------------------------------------------------------------------------------
