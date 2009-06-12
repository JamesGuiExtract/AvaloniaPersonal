
#include "stdafx.h"
#include "mathUtil.h"
#include "UCLIDException.h"

#include <Math.h>

double MathVars::ZERO = 1E-8;
const double MathVars::INFINITY = 1.7E308;
const double MathVars::PI = 3.1415926535897932384626433832795;

bool MathVars::isZero(const double dValue)
{
	return fabs(dValue) <= ZERO;
}

bool MathVars::isInfinity(const double dValue)
{
	return isEqual(dValue, INFINITY);
}

bool MathVars::isEqual(const double dValue1, const double dValue2)
{
	return fabs(dValue1 - dValue2) <= ZERO;
}

MathVars::TemporaryZeroDefinition::TemporaryZeroDefinition(const double dNewZeroValue)
{
	// store the value of MathVars::ZERO and set it to the specified value
	m_dOldZeroValue = MathVars::ZERO;
	MathVars::ZERO = dNewZeroValue;
}

MathVars::TemporaryZeroDefinition::~TemporaryZeroDefinition()
{
	try
	{
		// restore the value of MathVars::ZERO;
		MathVars::ZERO = m_dOldZeroValue;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16389");
}
