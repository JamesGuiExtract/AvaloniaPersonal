
#include "stdafx.h"
#include "mathUtil.h"
#include "UCLIDException.h"

#include <Math.h>

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
double MathVars::ZERO = 1E-8;
const double MathVars::PI = 3.1415926535897932384626433832795;

//--------------------------------------------------------------------------------------------------
// MathVars implementation
//--------------------------------------------------------------------------------------------------
bool MathVars::isZero(const double dValue)
{
	return fabs(dValue) <= ZERO;
}
//--------------------------------------------------------------------------------------------------
bool MathVars::isInfinity(const double dValue)
{
	return isEqual(dValue, INFINITY);
}
//--------------------------------------------------------------------------------------------------
bool MathVars::isEqual(const double dValue1, const double dValue2)
{
	return fabs(dValue1 - dValue2) <= ZERO;
}
//--------------------------------------------------------------------------------------------------
MathVars::TemporaryZeroDefinition::TemporaryZeroDefinition(const double dNewZeroValue)
{
	// store the value of MathVars::ZERO and set it to the specified value
	m_dOldZeroValue = MathVars::ZERO;
	MathVars::ZERO = dNewZeroValue;
}
//--------------------------------------------------------------------------------------------------
MathVars::TemporaryZeroDefinition::~TemporaryZeroDefinition()
{
	try
	{
		// restore the value of MathVars::ZERO;
		MathVars::ZERO = m_dOldZeroValue;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16389");
}

//--------------------------------------------------------------------------------------------------
// Other exported functions
//--------------------------------------------------------------------------------------------------
void rotateRectangle(long& rnLeft, long& rnTop, long& rnRight, long& rnBottom, long nXLimit,
					 long nYLimit, long nAngleInDegrees)
{
	try
	{
		// Build a rectangle from the points
		RECT rect;
		rect.left = rnLeft;
		rect.top = rnTop;
		rect.right = rnRight;
		rect.bottom = rnBottom;

		// Rotate the rectangle
		rotateRectangle(rect, nXLimit, nYLimit, nAngleInDegrees);

		// Get the new points from the rotated rectangle
		rnLeft = rect.left;
		rnTop = rect.top;
		rnRight = rect.right;
		rnBottom = rect.bottom;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26662");
}
//--------------------------------------------------------------------------------------------------
void rotateRectangle(RECT& rRect, long nXLimit, long nYLimit, long nAngleInDegrees)
{
	try
	{
		// Store original settings
		long lOrigTop    = rRect.top;
		long lOrigLeft   = rRect.left;
		long lOrigBottom = rRect.bottom;
		long lOrigRight  = rRect.right;

		// Validate nAngleInDegrees
		switch (nAngleInDegrees)
		{
		case 0:
		case 360:
			// No rotation is needed
			break;

		case 90:
		case -270:
			// Rotate the rectangle 90-degrees clockwise
			rRect.top = lOrigLeft;
			rRect.left = nYLimit - lOrigBottom;
			rRect.right = nYLimit - lOrigTop;
			rRect.bottom = lOrigRight;
			break;

		case 180:
		case -180:
			// Turn the rectangle upside down
			rRect.top = nYLimit - lOrigBottom;
			rRect.left = nXLimit - lOrigRight;
			rRect.right = nXLimit - lOrigLeft;
			rRect.bottom = nYLimit - lOrigTop;
			break;

		case 270:
		case -90:
			// Rotate the rectangle 90-degrees counterclockwise
			rRect.top = nXLimit - lOrigRight;
			rRect.left = lOrigTop;
			rRect.right = lOrigBottom;
			rRect.bottom = nXLimit - lOrigLeft;
			break;

		default:
			UCLIDException ue("ELI26663", "Invalid rotation angle for rectangle!");
			ue.addDebugInfo("Angle", nAngleInDegrees);
			throw ue;
		}

		// Check that bounds are >= 0
		if (rRect.top < 0 || rRect.left < 0 || rRect.bottom < 0 || rRect.right < 0)
		{
			UCLIDException ue("ELI26664", "Invalid bounds for Long Rectangle after rotation!");
			ue.addDebugInfo("Angle", nAngleInDegrees);
			ue.addDebugInfo("Original Top", lOrigTop);
			ue.addDebugInfo("Original Left", lOrigLeft);
			ue.addDebugInfo("Original Bottom", lOrigBottom);
			ue.addDebugInfo("Original Right", lOrigRight);
			ue.addDebugInfo("New Top", rRect.top);
			ue.addDebugInfo("New Left", rRect.left);
			ue.addDebugInfo("New Bottom", rRect.bottom);
			ue.addDebugInfo("New Right", rRect.right);
			throw ue;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26661");
}
//--------------------------------------------------------------------------------------------------
