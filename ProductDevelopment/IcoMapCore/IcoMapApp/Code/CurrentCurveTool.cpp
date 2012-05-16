//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	CurrentCurveTool.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	John Hurd
//
//==================================================================================================

#include "stdafx.h"
#include "CurrentCurveTool.h"

#include <UCLIDException.h>

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

using namespace std;


//--------------------------------------------------------------------------------------------------
CurrentCurveTool::CurrentCurveTool()
:CurveTool(kCurrentCurveTool)
{
	initializePromptMap();
	initializeInputTypeMap();
}
//--------------------------------------------------------------------------------------------------
CurrentCurveTool::CurrentCurveTool(const CurrentCurveTool& toCopy)
:CurveTool(kCurrentCurveTool)
{
	throw UCLIDException("ELI02201", "Internal error: copy constructor of singleton class called!");
}
//--------------------------------------------------------------------------------------------------
CurrentCurveTool& CurrentCurveTool::operator = (const CurrentCurveTool& toAssign)
{
	throw UCLIDException("ELI02202", "Internal error: assignment operator of singleton class called!");
}
//--------------------------------------------------------------------------------------------------
CurrentCurveTool::~CurrentCurveTool()
{
}
//--------------------------------------------------------------------------------------------------
EInputType CurrentCurveTool::getInputType(int iParameterID) const
{
	InputTypeMap::const_iterator it = m_mapInputType.find(getCurveParameter(iParameterID));
	if (it == m_mapInputType.end())
	{
		UCLIDException uclidException("ELI01096","Cannot map curve parameter input type. Invalid curveParameterID.");
		uclidException.addDebugInfo("curveParameterID", iParameterID);
		throw uclidException;
	}

	return (*it).second;
}
//--------------------------------------------------------------------------------------------------
std::string CurrentCurveTool::getPrompt(int iParameterID) const
{
	PromptMap::const_iterator it = m_mapPrompt.find(getCurveParameter(iParameterID));
	if (it == m_mapPrompt.end())
	{
		UCLIDException uclidException("ELI01095","Cannot map curve parameter prompt. Invalid curveParameterID.");
		uclidException.addDebugInfo("curveParameterID", iParameterID);
		throw uclidException;
	}

	return (*it).second;
}
//--------------------------------------------------------------------------------------------------
void CurrentCurveTool::initializePromptMap()
{
	m_mapPrompt[kArcConcaveLeft] = "Concave left";
	m_mapPrompt[kArcDeltaGreaterThan180Degrees] = "Delta angle greater than 180 degress";

	// angle/bearing parameters:
	m_mapPrompt[kArcDelta] = "Enter the delta angle (measured in Degrees)";
	m_mapPrompt[kArcStartAngle] = "Enter the angle (measured in Degrees) from the arc center to the arc start point";
	m_mapPrompt[kArcEndAngle] = "Enter the angle (measured in Degrees) from the arc center to the arc end point";
	m_mapPrompt[kArcDegreeOfCurveChordDef] = "Enter the degree of curve (chord def measured in Degrees)";
	m_mapPrompt[kArcDegreeOfCurveArcDef] = "Enter the degree of curve (arc def measured in Degrees)";
	m_mapPrompt[kArcTangentInBearing] = "Enter the tangent-in direction";
	m_mapPrompt[kArcTangentOutBearing] = "Enter the tangent-out direction";
	m_mapPrompt[kArcChordBearing] = "Enter the chord direction";
	m_mapPrompt[kArcRadialInBearing] = "Enter the radial-in direction";
	m_mapPrompt[kArcRadialOutBearing] = "Enter the radial-out direction";

	// distance paramters:
	m_mapPrompt[kArcRadius] = "Enter the radius";
	m_mapPrompt[kArcLength] = "Enter the arc length";
	m_mapPrompt[kArcChordLength] = "Enter the chord length";
	m_mapPrompt[kArcExternalDistance] = "Enter the distance from the arc mid point to an external point";
	m_mapPrompt[kArcMiddleOrdinate] = "Enter the distance from the arc mid point to the chord mid point";
	m_mapPrompt[kArcTangentDistance] = "Enter the distance from the arc start point to an external point";

	// point parameters:
	m_mapPrompt[kArcStartingPoint] = "Enter the arc start point";
	m_mapPrompt[kArcMidPoint] = "Enter the arc mid point";
	m_mapPrompt[kArcEndingPoint] = "Enter the arc end point";
	m_mapPrompt[kArcCenter] = "Enter the center point of the circle";
	m_mapPrompt[kArcExternalPoint] = "Enter the point where the in-tangent intersects the out-tangent";
	m_mapPrompt[kArcChordMidPoint] = "Enter the chord mid point";
}
//--------------------------------------------------------------------------------------------------
void CurrentCurveTool::initializeInputTypeMap()
{
	// toggel curve direction or bulge (either > or < 180 °)
	m_mapInputType[kArcConcaveLeft] = kToggleCurve;
	m_mapInputType[kArcDeltaGreaterThan180Degrees] = kToggleCurve;

	// angle/bearing parameters:
	m_mapInputType[kArcDelta] = kAngle;
	m_mapInputType[kArcStartAngle] = kAngle;
	m_mapInputType[kArcEndAngle] = kAngle;
	m_mapInputType[kArcDegreeOfCurveChordDef] = kAngle;
	m_mapInputType[kArcDegreeOfCurveArcDef] = kAngle;
	m_mapInputType[kArcTangentInBearing] = kBearing;
	m_mapInputType[kArcTangentOutBearing] = kBearing;
	m_mapInputType[kArcChordBearing] = kBearing;
	m_mapInputType[kArcRadialInBearing] = kBearing;
	m_mapInputType[kArcRadialOutBearing] = kBearing;


	// distance paramters:
	m_mapInputType[kArcRadius] = kDistance;
	m_mapInputType[kArcLength] = kDistance;
	m_mapInputType[kArcChordLength] = kDistance;
	m_mapInputType[kArcExternalDistance] = kDistance;
	m_mapInputType[kArcMiddleOrdinate] = kDistance;
	m_mapInputType[kArcTangentDistance] = kDistance;

	// point parameters:
	m_mapInputType[kArcStartingPoint] = kPoint;
	m_mapInputType[kArcMidPoint] = kPoint;
	m_mapInputType[kArcEndingPoint] = kPoint;
	m_mapInputType[kArcCenter] = kPoint;
	m_mapInputType[kArcExternalPoint] = kPoint;
	m_mapInputType[kArcChordMidPoint] = kPoint;
}
//--------------------------------------------------------------------------------------------------





