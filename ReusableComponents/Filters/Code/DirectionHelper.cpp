#include "stdafx.h"
#include "DirectionHelper.h"

#include "AbstractMeasurement.hpp"

#include <cpputil.h>
#include <UCLIDException.h>

using namespace std;

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

EDirection DirectionHelper::m_sEDirection = kUnknownDirection;


//----------------------------------------------------------------------------------------------
string DirectionHelper::directionAsString()
{
	string strRet("");
	
	switch (m_sEDirection)
	{
	case kBearingDir:
		{
			strRet = m_BearingDirection.asString();
		}
		break;
	case kPolarAngleDir:
		{
			strRet = m_PolarAngleDirection.asString();
		}
		break;
	case kAzimuthDir:
		{
			strRet = m_AzimuthDirection.asString();
		}
		break;
	default:
		{
			throw UCLIDException("ELI02861", "Invalid Direction Type!");
		}
		break;
	}

	return strRet;
}
//----------------------------------------------------------------------------------------------
void DirectionHelper::evaluateDirection(const std::string& strInputDirection)
{
	switch (m_sEDirection)
	{
	case kBearingDir:
		{
			m_BearingDirection.resetVariables();
			m_BearingDirection.evaluate(strInputDirection.c_str());
		}
		break;
	case kPolarAngleDir:
		{
			m_PolarAngleDirection.resetVariables();
			m_PolarAngleDirection.evaluate(strInputDirection.c_str());
		}
		break;
	case kAzimuthDir:
		{
			m_AzimuthDirection.resetVariables();
			m_AzimuthDirection.evaluate(strInputDirection.c_str());
		}
		break;
	default:
		{
			throw UCLIDException("ELI02862", "Invalid Direction Type!");
		}
		break;
	}
}
//----------------------------------------------------------------------------------------------
vector<string> DirectionHelper::getAlternateStringsAsDirection()
{	
	vector<string> vecTemp;
	
	switch (m_sEDirection)
	{
	case kBearingDir:
		{
			vecTemp = m_BearingDirection.getAlternateStrings();
		}
		break;
	case kPolarAngleDir:
		{
			vecTemp = m_PolarAngleDirection.getAlternateStrings();
		}
		break;
	case kAzimuthDir:
		{
			vecTemp = m_AzimuthDirection.getAlternateStrings();
		}
		break;
	}

	return vecTemp;
}
//----------------------------------------------------------------------------------------------
AbstractMeasurement* DirectionHelper::getCurrentMeasurement()
{
	AbstractMeasurement *pMeasurement = NULL;

	switch (m_sEDirection)
	{
	case kBearingDir:
		{
			pMeasurement = &m_BearingDirection;
		}
		break;
	case kPolarAngleDir:
		{
			pMeasurement = &m_PolarAngleDirection;
		}
		break;
	case kAzimuthDir:
		{
			pMeasurement = &m_AzimuthDirection;
		}
		break;
	default:
		{
			throw UCLIDException("ELI02876", "Invalid Direction Type!");
		}
		break;
	}

	return pMeasurement;
}
//----------------------------------------------------------------------------------------------
double DirectionHelper::getPolarAngleDegrees()
{
	double dDegrees = 0.0;
	
	switch (m_sEDirection)
	{
	case kBearingDir:
		{
			dDegrees = m_BearingDirection.getDegrees();
		}
		break;
	case kPolarAngleDir:
		{
			// Polar angle 0° = East, then increase angle counter-clock-wisely
			// get the degrees out of the input angle
			// This angle needs no translation to get the polar angle value, since they
			// are the same. For instance, if the input anlge is 56 degrees, then the polar
			// angle value is 56 degrees
			dDegrees = m_PolarAngleDirection.getDegrees();

			// if it's reverse mode, flip it
			if (Angle::isInReverseMode())
			{
				dDegrees += 180.0;
				if (dDegrees >= 360.0)
				{
					dDegrees = dDegrees - 360.0 ;
				}
			}
		}
		break;
	case kAzimuthDir:
		{
			// Azimuth angle 0° = North (i.e. 90°), and increase angle clock-
			// wisely. Our calculation is based on polar angle (which is 0°-based), 
			// the Azimuth angle needs to be converted into polar angle.
			// First get the input azimuth angle value
			// Then convert it to the actual polar angle.
			// For instance, if input is 80 degrees(Azimuth), the interpreted angle value
			// is 90 - 80 = 10 degrees.
			// Input string: 225° (Azimuth), Polar Angle value: 90 - 210 = -120 = 240 
			dDegrees = azimuthPolarAngleConversionsInDegrees(m_AzimuthDirection.getDegrees());

			// if it's reverse mode, flip it
			if (Angle::isInReverseMode())
			{
				dDegrees += 180.0;
				if (dDegrees >= 360.0)
				{
					dDegrees = dDegrees - 360.0;
				}
			}
		}
		break;
	default:
		{
			throw UCLIDException("ELI02859", "Invalid Direction Type!");
		}
		break;
	}

	return dDegrees;
}
//----------------------------------------------------------------------------------------------
double DirectionHelper::getPolarAngleRadians()
{
	double dRadians = 0.0;

	switch (m_sEDirection)
	{
	case kBearingDir:
		{
			dRadians = m_BearingDirection.getRadians();
		}
		break;
	case kPolarAngleDir:
		{
			dRadians = m_PolarAngleDirection.getRadians();
			// if it's reverse mode, flip it
			if (Angle::isInReverseMode())
			{
				dRadians += MathVars::PI;
				if (dRadians >= MathVars::PI * 2)
				{
					dRadians = dRadians - MathVars::PI * 2;
				}
			}
		}
		break;
	case kAzimuthDir:
		{
			// get the azimuth angle (0° = North, increase clock-wisely)
			// convert it to polar angle in radians
			dRadians = azimuthPolarAngleConversionsInRadians(m_AzimuthDirection.getRadians());		
			// if it's reverse mode, flip it
			if (Angle::isInReverseMode())
			{
				dRadians += MathVars::PI;
				if (dRadians >= MathVars::PI * 2)
				{
					dRadians = dRadians - MathVars::PI * 2;
				}
			}
		}
		break;
	default:
		{
			throw UCLIDException("ELI02860", "Invalid Direction Type!");
		}
		break;
	}

	return dRadians;
}
//----------------------------------------------------------------------------------------------
/*
string DirectionHelper::interpretedDirectionAsString()
{
	string strRet("");
	
	switch (m_sEDirection)
	{
	case kBearingDir:
		{
			strRet = m_BearingDirection.interpretedValueAsString();
		}
		break;
	case kPolarAngleDir:
		{
			strRet = m_PolarAngleDirection.interpretedValueAsString();
		}
		break;
	case kAzimuthDir:
		{
			strRet = m_AzimuthDirection.interpretedValueAsString();
		}
		break;
	default:
		{
			throw UCLIDException("ELI02863", "Invalid Direction Type!");
		}
		break;
	}

	return strRet;

}*/
//----------------------------------------------------------------------------------------------
bool DirectionHelper::isDirectionValid()
{
	bool bValid = false;

	switch (m_sEDirection)
	{
	case kBearingDir:
		bValid = m_BearingDirection.isValid();
		break;
	case kPolarAngleDir:
		bValid = m_PolarAngleDirection.isValid();
		break;
	case kAzimuthDir:
		bValid = m_AzimuthDirection.isValid();
		break;
	default:
		{
			throw UCLIDException("ELI19482", "Invalid Direction Type!");
		}
		break;
	}

	return bValid;
}
//----------------------------------------------------------------------------------------------
string DirectionHelper::polarAngleInRadiansToDirectionInString(double dRadians)
{
	string strDirection("");

	switch (m_sEDirection)
	{
	case kBearingDir:
		{
			m_BearingDirection.resetVariables();
			// no need for conversion here
			m_BearingDirection.evaluateRadians(dRadians);
			strDirection = m_BearingDirection.getEvaluatedString();
		}
		break;
	case kPolarAngleDir:
		{
			m_PolarAngleDirection.resetVariables();
			// no need for conversion here
			m_PolarAngleDirection.evaluateRadians(dRadians);
			// require the output angle in degree-minute-second format
			strDirection = m_PolarAngleDirection.asStringDMS();
		}
		break;
	case kAzimuthDir:
		{
			m_AzimuthDirection.resetVariables();
			// convert the polar angle value to azimuth angle value in radians
			double dAzimuthAngle = polarAngleToCurrentDirectionInRadians(dRadians);
			m_AzimuthDirection.evaluateRadians(dAzimuthAngle);
			// require the output angle in degree-minute-second format
			strDirection = m_AzimuthDirection.asStringDMS();
		}
		break;
	default:
		{
			throw UCLIDException("ELI02875", "Invalid Direction Type!");
		}
		break;
	}

	return strDirection;
}
//----------------------------------------------------------------------------------------------
double DirectionHelper::polarAngleToCurrentDirectionInDegrees(double dPolarAngleDegrees)
{
	double dDegrees = dPolarAngleDegrees;

	if (m_sEDirection == kAzimuthDir)
	{
		// only convert the value if the current direction type is azimuth
		dDegrees = azimuthPolarAngleConversionsInDegrees(dPolarAngleDegrees);
	}

	return dDegrees;
}
//----------------------------------------------------------------------------------------------
double DirectionHelper::polarAngleToCurrentDirectionInRadians(double dPolarAngleRadians)
{
	double dRadians = dPolarAngleRadians;

	if (m_sEDirection == kAzimuthDir)
	{
		// only convert the value if the current direction type is azimuth
		dRadians = azimuthPolarAngleConversionsInRadians(dPolarAngleRadians);
	}

	return dRadians;
}
//----------------------------------------------------------------------------------------------
// **************************  Helper Functions  **********************************
//----------------------------------------------------------------------------------------------
double DirectionHelper::azimuthPolarAngleConversionsInDegrees(double dDegrees)
{
	double dRet = 90.0 - dDegrees;
	// make sure the angle is in between 0° and 360.0°
	double dMult = floor( dRet / 360.0 );
	dRet = dRet - dMult * 360.0;			

	return dRet;
}
//----------------------------------------------------------------------------------------------
double DirectionHelper::azimuthPolarAngleConversionsInRadians(double dRadians)
{
	double dRet = MathVars::PI/2 - dRadians;
	// make sure the angle is in between 0 and 2*PI
	double dMult = floor( dRet / (MathVars::PI * 2) );
	dRet = dRet - dMult * (MathVars::PI * 2);			

	return dRet;
}
//----------------------------------------------------------------------------------------------
