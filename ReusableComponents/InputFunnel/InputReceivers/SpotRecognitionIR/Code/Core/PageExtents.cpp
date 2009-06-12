#include "stdafx.h"
#include "PageExtents.h"

#include <math.h>
#include <mathUtil.h>

bool operator == (const PageExtents& e1, const PageExtents& e2)
{
	if (fabs(e1.dBottomLeftX - e2.dBottomLeftX) > MathVars::ZERO
		|| fabs(e1.dBottomLeftY - e2.dBottomLeftY) > MathVars::ZERO
		|| fabs(e1.dTopRightX - e2.dTopRightX) > MathVars::ZERO
		|| fabs(e1.dTopRightY - e2.dTopRightY) > MathVars::ZERO)
	{
		return false;
	}

	return true;
}