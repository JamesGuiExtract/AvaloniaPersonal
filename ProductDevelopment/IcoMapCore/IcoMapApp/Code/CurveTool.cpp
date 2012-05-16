//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	CurveTool.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	John Hurd
//
//==================================================================================================

#include "stdafx.h"
#include "CurveTool.h"

#include "CurveDjinni.h"

#include <IcoMapOptions.h>
#include <ECurveParameter.h>
#include <UCLIDException.h>

using namespace std;

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

CurveTool::CurveTool(ECurveToolID id) :
m_bToggleCurveDeltaAngleEnabled(false),
m_bToggleCurveDirectionEnabled(false),
m_eCurveToolID(id)
{
}

CurveTool::~CurveTool()
{
}

std::string CurveTool::sDescribeCurveParameter(ECurveParameterType eCurveParameter) 
{
	string strDescription;

	switch(eCurveParameter)
	{
		case kArcDelta:							// center angle for the circular arc
			strDescription = "Delta angle";
			break;
		case kArcStartAngle:					// angle from ArcCenter to ArcStartingPoint
			strDescription = "Start angle";
			break;
		case kArcEndAngle:						// angle from ArcCenter to ArcEndingpoint
			strDescription = "End angle";
			break;
		case kArcDegreeOfCurveChordDef:			// using this definition: radius = 5729.6506/DegreeOfCurve
			strDescription = "Degree of curve (chord definition)";
			break;
		case kArcDegreeOfCurveArcDef:			// using this definition: radius = 5729.5780/DegreeOfCurve
			strDescription = "Degree of curve (arc definition)";
			break;
		case kArcTangentInBearing:				// in-tangent for the arc: touching arc at ArcStartingPoint
			strDescription = "Tangent in bearing";
			break;
		case kArcTangentOutBearing:				// out-tangent for the arc: touching arc at ArcEndingPoint
			strDescription = "Tangent out bearing";
			break;
		case kArcChordBearing:					// bearing from ArcStartingPoint to ArcEndingPoint
			strDescription = "Chord bearing";
			break;
		case kArcRadialInBearing:				// bearing from ArcStartingPoint to ArcCenter
			strDescription = "Radial in bearing";
			break;
		case kArcRadialOutBearing:				// bearing from ArcCenter to ArcEndingPoint
			strDescription = "Radial out bearing";
			break;

		case kArcRadius:						// radius of the circle associated with the arc
			strDescription = "Radius";
			break;
		case kArcLength:						// length of the circular arc
			strDescription = "Arc length";
			break;
		case kArcChordLength:					// distance from ArcStartingPoint to ArcEndingPoint
			strDescription = "Chord length";
			break;
		case kArcExternalDistance:				// distance from ArcExternalPoint to ArcMidPoint
			strDescription = "Distance to external point";
			break;
		case kArcMiddleOrdinate:				// distance from ArcChordMidPoint to ArcMidPoint
			strDescription = "Distance to mid point";
			break;
		case kArcTangentDistance:				// distance from StartingPoint to ArcExternalPoint
			strDescription = "Tangent distance";
			break;

		case kArcStartingPoint:					// starting point of arc
			strDescription = "Start point of arc";
			break;
		case kArcMidPoint:						// mid point of arc
			strDescription = "Mid point of arc";
			break;
		case kArcEndingPoint:					// ending point of arc
			strDescription = "End point of arc";
			break;
		case kArcCenter:						// center of the circle related to the arc
			strDescription = "Center point of circle";
			break;
		case kArcExternalPoint:					// intersection point of the in-tangent and out-tangent
			strDescription = "Intersection point of the in-tangent and out-tangent";
			break;
		case kArcChordMidPoint:					// mid point of the curve chord
			strDescription = "Mid point of chord";
			break;
	case kArcConcaveLeft:						
	case kArcDeltaGreaterThan180Degrees:	
		ASSERT(false);		// unanticapted curve parameter
		break;
	default:
		ASSERT(false);		// Someone forgot to maintain this code.
	}

	return strDescription;
}

std::string CurveTool::generateToolTip(void)
{
	string strToolTip;

	CurveParameters curveParameters = getCurveParameters();
	for (CurveParameters::const_iterator it = curveParameters.begin(); it != curveParameters.end(); it++)
	{
		if (it != curveParameters.begin())
		{
			strToolTip += ", ";
		}

		strToolTip += sDescribeCurveParameter(*it);
	}

	m_strToolTip = strToolTip;
	return m_strToolTip;
}

ECurveParameterType CurveTool::getCurveParameter(int iParameterID) const
{
	ParameterMap::const_iterator it = m_mapParameter.find(iParameterID);
	if (it == m_mapParameter.end())
	{
		UCLIDException uclidException("ELI01097","curveParameterID out of range.");
		uclidException.addDebugInfo("curveParameterID", iParameterID);
		throw uclidException;
	}

	return (*it).second;
}

CurveParameters CurveTool::getCurveParameters(void) const
{
	CurveParameters curveParameters;

	list<int> listParameterID = getSortedCurveParameterIDList();

	for (list<int>::const_iterator it = listParameterID.begin(); it != listParameterID.end(); it++)
	{
		curveParameters.push_back(getCurveParameter(*it));
	}

	return curveParameters;
}


std::list<int> CurveTool::getSortedCurveParameterIDList(void) const
{
	list<int> listParameterID;

	// Create a list of curveParameterIDs sorted in ascending order.
	for (ParameterMap::const_iterator item = m_mapParameter.begin();item != m_mapParameter.end(); item++)
	{
		listParameterID.push_back((*item).first);
	}
	listParameterID.sort();

	return listParameterID;
}

void CurveTool::reset(void)
{
	m_mapParameter.clear();
}

bool CurveTool::restoreState(void)
{
	bool bSuccess = false;
	
	try
	{
		//read object's state from persistent store
		vector<ECurveParameterType> vecCurveParams
			= IcoMapOptions::sGetInstance().getCurveToolParameters(m_eCurveToolID);
		//get the parameter IDs sorted in ascending order
		for (unsigned int i = 1; i<= vecCurveParams.size(); i++)
		{
			m_mapParameter[i] = vecCurveParams.at(i-1);
		}

		bSuccess = true;
	}
	catch(UCLIDException &uclidException)
	{
		uclidException.addHistoryRecord("ELI01196", "Failed to restore default state for the curve tool.");
		uclidException.addDebugInfo("CurveToolID", (long)m_eCurveToolID);
		throw uclidException;
	}

	return bSuccess;
}

bool CurveTool::saveState(void) const
{
	bool bSuccess = false;
	
	try
	{
		//write object's state to persistent store
		ParameterMap::const_iterator curveParamIter;
		for (curveParamIter = m_mapParameter.begin(); curveParamIter != m_mapParameter.end(); curveParamIter++)
		{
			IcoMapOptions::sGetInstance().setCurveToolParameter(m_eCurveToolID, curveParamIter->first, curveParamIter->second);
		}

		bSuccess = true;
	}
	catch (UCLIDException &uclidException)
	{
		uclidException.addHistoryRecord("ELI01197", "Failed to save state for the curve tool.");
		uclidException.addDebugInfo("CurveToolID", (long)m_eCurveToolID);
		throw uclidException;

	}

	return bSuccess;
}

void CurveTool::setCurveParameter(int iParameterID, ECurveParameterType eParameterType)
{
	m_mapParameter[iParameterID] = eParameterType;
}

void CurveTool::updateStateOfCurveToggles(void)
{
	CurveDjinni djinni;
	CurveMatrixEntry entry = djinni.createCurveMatrixEntry(getCurveParameter(1),getCurveParameter(2),getCurveParameter(3));
	enableToggleCurveDeltaAngle(djinni.isToggleCurveDeltaAngleEnabled(entry));
	enableToggleCurveDirection(djinni.isToggleCurveDirectionEnabled(entry));
}

