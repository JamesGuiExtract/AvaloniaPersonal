//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE		:	CCEHelper.cpp
//
// PURPOSE	:	 
//
// NOTES	:	Nothing
//
// AUTHORS	:	Duan Wang
//
//==================================================================================================
// CCEHelper.cpp : implementation file
//

#include "stdafx.h"
#include "CCEHelper.h"

#include "CurveCalculationEngineImpl.h"

#include <Bearing.hpp>
#include <Angle.hpp>
#include <DistanceCore.h>
#include <StringTokenizer.h>
#include <cpputil.h>


#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

using namespace std;

//==================================================================================================
CCEHelper::CCEHelper(ICurveCalculationEngine *pCCE)
:m_pCCE(pCCE)
{
}
//--------------------------------------------------------------------------------------------------
void CCEHelper::setCurveParameter(ECurveParameterType eParameterType, const string& strParamValue)
{
	if (m_pCCE)
	{
		// depending upon what the type of the parameter is, parse the
		// value accordingly
		switch (eParameterType)
		{
			// distance parameter types
		case kArcRadius:
		case kArcLength:
		case kArcChordLength:
		case kArcExternalDistance:
		case kArcMiddleOrdinate:
		case kArcTangentDistance:
			{
				// use the distance class to parse the value
				static DistanceCore dist;

				dist.evaluate(strParamValue.c_str());
				if (dist.isValid())
				{
					// get distance in current unit
					EDistanceUnitType eUnit = dist.getCurrentDistanceUnit();
					if (eUnit == kUnknownUnit)
					{ 
						eUnit = dist.getDefaultDistanceUnit();
					}

					double dDist = dist.getDistanceInUnit(eUnit);
					m_pCCE->setCurveDistanceParameter(eParameterType, dDist);
				}
				else
				{
					UCLIDException uclidException("ELI01688", "Invalid distance string.");
					uclidException.addDebugInfo("Distance", strParamValue);
					throw uclidException;
				}
				break;
			}
			
			// bearing parameter types
		case kArcTangentInBearing:
		case kArcTangentOutBearing:
		case kArcChordBearing:
		case kArcRadialInBearing:
		case kArcRadialOutBearing:
			{
				Bearing bearing;
				bearing.evaluate(strParamValue.c_str());
				if (bearing.isValid())
				{
					double dRadians = bearing.getRadians();
					m_pCCE->setCurveAngleOrBearingParameter(eParameterType, dRadians);
				}
				else
				{
					UCLIDException uclidException("ELI01689", "Invalid bearing string.");
					uclidException.addDebugInfo("Bearing", strParamValue);
					throw uclidException;					
				}
				break;
			}
			
			// angle parameter types
		case kArcDegreeOfCurveChordDef:
		case kArcDegreeOfCurveArcDef:
		case kArcDelta:
		case kArcStartAngle:
		case kArcEndAngle:
			{
				static Angle angle;
				angle.resetVariables();
				angle.evaluate(strParamValue.c_str());
				if (angle.isValid())
				{
					double dRadians = angle.getRadians();
					m_pCCE->setCurveAngleOrBearingParameter(eParameterType, dRadians);
				}
				else
				{
					UCLIDException uclidException("ELI01690", "Invalid angle string.");
					uclidException.addDebugInfo("Angle", strParamValue);
					throw uclidException;					
				}
				break;
			}
			
			// point parameter types
		case kArcStartingPoint:
		case kArcMidPoint:
		case kArcEndingPoint:
		case kArcCenter:
		case kArcExternalPoint:
		case kArcChordMidPoint:
			{
				vector<string> vecTokens;
				static StringTokenizer st;
				st.parse(strParamValue, vecTokens);
				if (vecTokens.size() == 2)
				{
					try
					{
						double dX = asDouble(vecTokens[0]);
						double dY = asDouble(vecTokens[1]);
						m_pCCE->setCurvePointParameter(eParameterType, dX, dY);
					}
					catch(UCLIDException &uclidException)
					{
						uclidException.addHistoryRecord("ELI02225", "Failed to set curve parameters.");
						throw uclidException;
					}
				}
				else
				{
					UCLIDException uclidException("ELI01691", "Invalid point string.");
					uclidException.addDebugInfo("Point", strParamValue);
					throw uclidException;					
				}
				break;
			}
			
			// boolean parameter types
		case kArcConcaveLeft:
		case kArcDeltaGreaterThan180Degrees:
			{
				if (strParamValue == "0")
				{
					m_pCCE->setCurveBooleanParameter(eParameterType, false);
				}
				else if (strParamValue == "1")
				{
					m_pCCE->setCurveBooleanParameter(eParameterType, true);
				}
				else
				{
					UCLIDException uclidException("ELI01692", "Invalid boolean string.");
					uclidException.addDebugInfo("Point", strParamValue);
					throw uclidException;					
				}
				break;
			}
			
			// unknown parameter types
		default:
			UCLIDException uclidException("ELI01693", "Unknown parameter type.");
			uclidException.addDebugInfo("ParameterEnumValue", eParameterType);
			throw uclidException;					
		}
	}
}
//--------------------------------------------------------------------------------------------------
string CCEHelper::getCurveParameter(ECurveParameterType eParameterType)
{
	string strParamValue("");

	if (m_pCCE)
	{
		strParamValue = m_pCCE->getCurveParameter(eParameterType);

		if (!strParamValue.empty())
		{
			// depending upon what the type of the parameter is, parse the
			// value accordingly
			switch (eParameterType)
			{
			// distance parameter types
			case kArcRadius:
			case kArcLength:
			case kArcChordLength:
			case kArcExternalDistance:
			case kArcMiddleOrdinate:
			case kArcTangentDistance:
				{
					static DistanceCore dist;
					EDistanceUnitType eUnit = dist.getCurrentDistanceUnit();
					if (eUnit == kUnknownUnit)
					{ 
						eUnit = dist.getDefaultDistanceUnit();
					}
					// the distance is in current unit type
					string strUnit(dist.getStringFromUnit(eUnit));
					CString cstrDistance("");
					cstrDistance.Format("%s %s", strParamValue.c_str(), strUnit.c_str());
					strParamValue = string(cstrDistance);
				}
				break;
			// point parameter types
			case kArcStartingPoint:
			case kArcMidPoint:
			case kArcEndingPoint:
			case kArcCenter:
			case kArcExternalPoint:
			case kArcChordMidPoint:
			// boolean parameter types
			case kArcConcaveLeft:
			case kArcDeltaGreaterThan180Degrees:	
				break;
			// bearing parameter types
			case kArcTangentInBearing:
			case kArcTangentOutBearing:
			case kArcChordBearing:
			case kArcRadialInBearing:
			case kArcRadialOutBearing:
				{
					static Bearing bearing;
					bearing.resetVariables();

					double dRadians = 0;
					try
					{
						// input the string in radians format
						dRadians = strParamValue.empty() ? 0.0 : asDouble(strParamValue);
					}
					catch (UCLIDException &uclidException)
					{
						uclidException.addHistoryRecord("ELI02222", "Failed to set curve parameters.");
						throw uclidException;
					}

					bearing.evaluateRadians(dRadians);
					if (bearing.isValid())
					{
						strParamValue = bearing.interpretedValueAsString();
					}
					else
					{
						UCLIDException uclidException("ELI01694", "Invalid bearing string.");
						uclidException.addDebugInfo("Bearing", strParamValue);
						throw uclidException;					
					}
					break;
				}
				
			// angle parameter types
			case kArcDegreeOfCurveChordDef:
			case kArcDegreeOfCurveArcDef:
			case kArcDelta:
			case kArcStartAngle:
			case kArcEndAngle:
				{
					static Angle angle;
					angle.resetVariables();

					double dRadians = 0;
					try
					{
						dRadians = strParamValue.empty() ? 0.0 : asDouble(strParamValue);
					}
					catch (UCLIDException &uclidException)
					{
						uclidException.addHistoryRecord("ELI02223", "Failed to get curve parameters.");
						throw uclidException;
					}

					angle.evaluateRadians(dRadians);
					if (angle.isValid())
					{
						strParamValue = angle.interpretedValueAsString();
					}
					else
					{
						UCLIDException uclidException("ELI01695", "Invalid angle string.");
						uclidException.addDebugInfo("Angle", strParamValue);
						throw uclidException;					
					}
					break;
				}
			// unknown parameter types
			default:
				UCLIDException uclidException("ELI01696", "Unknown parameter type.");
				uclidException.addDebugInfo("ParameterEnumValue", eParameterType);
				throw uclidException;		
				
			}
		}
	}

	return strParamValue;
}
//--------------------------------------------------------------------------------------------------
double CCEHelper::getCurveBearingAngleOrDistanceInDouble(ECurveParameterType eParameterType)
{
	double	dValue = 0.0;

	if (m_pCCE)
	{
		// Get the value
		string strParamValue(m_pCCE->getCurveParameter(eParameterType));

		if (!strParamValue.empty())
		{
			// If a distance, convert it back to a double
			switch (eParameterType)
			{
			// distance parameter types
			case kArcRadius:
			case kArcLength:
			case kArcChordLength:
			case kArcExternalDistance:
			case kArcMiddleOrdinate:
			case kArcTangentDistance:
			// bearing parameter types
			case kArcTangentInBearing:
			case kArcTangentOutBearing:
			case kArcChordBearing:
			case kArcRadialInBearing:
			case kArcRadialOutBearing:
			// angle parameter types
			case kArcDegreeOfCurveChordDef:
			case kArcDegreeOfCurveArcDef:
			case kArcDelta:
			case kArcStartAngle:
			case kArcEndAngle:
				{
					try
					{
						dValue = strParamValue.empty() ? 0.0 : asDouble( strParamValue );
					}
					catch (UCLIDException &uclidException)
					{
						uclidException.addHistoryRecord("ELI02907", "Failed to get value.");
						uclidException.addDebugInfo("Parameter Type", eParameterType);
						throw uclidException;
					}
				}
				break;
				
			// Point, Boolean type or Unknown parameter types
			default:
				{
					UCLIDException uclidException("ELI02908", "Unsupported parameter type.");
					uclidException.addDebugInfo("ParameterEnumValue", eParameterType);
					throw uclidException;		
				}
				
			}
		}
	}

	return dValue;
}
//--------------------------------------------------------------------------------------------------