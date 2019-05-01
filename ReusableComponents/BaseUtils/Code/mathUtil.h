#pragma once

#include "BaseUtils.h"

#include <windows.h>

class EXPORT_BaseUtils MathVars
{
public:
	static double ZERO;
	static const double PI;

	static bool isEqual(const double dValue1, const double dValue2);
	static bool isZero(const double dValue);
	static bool isInfinity(const double dValue);

	// create an instance of this nested class to temporarily change
	// the value of MathVars::ZERO
	// NOTE: the following class is not thread safe - it temporarily changes
	// the value of ZERO for all threads of execution (not just
	// the current thread) - use cautiously!
	class EXPORT_BaseUtils TemporaryZeroDefinition
	{
	public:

		TemporaryZeroDefinition(const double dNewZeroValue);
		~TemporaryZeroDefinition();

	private:
		double m_dOldZeroValue;
	};
};

//-------------------------------------------------------------------------------------------------
// PURPOSE: To convert from degrees to radians
//
// NOTE:	There is no constraint on the return value
EXPORT_BaseUtils inline double convertDegreesToRadians(const double& rdDegrees)
{
	return ( (rdDegrees * MathVars::PI) / 180.0 );
}
//-------------------------------------------------------------------------------------------------
// PURPOSE: To convert from radians to degrees
//
// NOTE:	There is no constraint on the return value
EXPORT_BaseUtils inline double convertRadiansToDegrees(const double& rdRadians)
{
	return ( (rdRadians * 180.0) / MathVars::PI );
}
//-------------------------------------------------------------------------------------------------
// PURPOSE: To rotate the specified rectangle by the specified number of degrees.
// REQUIRE: Degrees must be in increments of 90 and can range from -270 to 360
EXPORT_BaseUtils void rotateRectangle(long& rnLeft, long& rnTop, long& rnRight, long& rnBottom,
									  long nXLimit, long nYLimit, long nAngleInDegrees);
EXPORT_BaseUtils void rotateRectangle(RECT& rRect, long nXLimit, long nYLimit, long nAngleInDegrees);
//-------------------------------------------------------------------------------------------------