//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE		:	CurveCalculationEngineImpl.cpp
//
// PURPOSE	:	This is an implementation file for CurveCalculationEngineImpl 
//				class Where the CurveCalculationEngineImpl class has been 
//				derived from ICurveCalculationEngine class.The code written 
//				in this file makes it possible to implement the various
//				pure virtual functions related to Circular Arcs
//
// NOTES	:	Nothing
//
// AUTHORS	:	G Chandra Sekhar Babu
//
//==================================================================================================
// CurveCalculationEngineImpl.cpp : implementation file
//
#include "stdafx.h"
#include "CurveCalculationEngineImpl.h"

#include "VRRinBearingToTinBearing.h"
#include "VRRoutBearingToToutBearing.h"
#include "VRDeltaToTinToutBearing.h"
#include "VRStartAngleToTinBearing.h"
#include "VREndAngleToToutBearing.h"
#include "VRMiddleOrdinateToRadiusDelta.h"
#include "VRChordBearingToTinDelta.h"
#include "VRArcLengthToRadiusDelta.h"
#include "VRDegreeCurveArcToRadius.h"
#include "VRDegreeCurveChordToRadius.h"
#include "VRTangentDistanceToRadiusDelta.h"
#include "VRExternalDistanceToRadiusDelta.h"
#include "VRCenterToStartingPointRadiusRin.h"
#include "VREndPointToStartingPointChordLengthChordBearing.h"
#include "VRMiddleToCenterPointRadiusChordBearing.h"
#include "VRCenterToEndPointRadiusRout.h"
#include "VRDeltaToChordLengthRadius.h"
#include "VRChordMidPointToStartingPointChordBearing.h"
#include "VRExternalPointToStartingPointTangentDistance.h"
#include "VRChordBearingToToutDelta.h"
#include "VRChordMidPointToStartingEndingPoint.h"
#include "VRMiddleToChordMidPointMidOrdinateChordBearing.h"
#include "VRRadiusToMidOrdinateChordLength.h"
#include "VRDeltaAngleToDeltaGreaterThan180.h"

#include <Point.h>
#include <mathUtil.h>
#include <math.h>

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

using namespace std;
/////////////////////////////////////////////////////////////////////////////
// CurveCalculationEngineImpl

CurveCalculationEngineImpl::CurveCalculationEngineImpl()
{
	
	//mapping the ECurveParameterType with the CurveVariable
	//Curve boolean parameters:
	m_mapVars[kArcConcaveLeft] = new CurveVariable();
	m_mapVars[kArcDeltaGreaterThan180Degrees]=new CurveVariable();
	//Curve angle/bearing parameters:(bearings are a type of angle)
	m_mapVars[kArcDelta]=new CurveVariable();
	m_mapVars[kArcStartAngle]=new CurveVariable();
	m_mapVars[kArcEndAngle]=new CurveVariable();
	m_mapVars[kArcDegreeOfCurveChordDef]=new CurveVariable();
	m_mapVars[kArcDegreeOfCurveArcDef]=new CurveVariable();
	m_mapVars[kArcTangentInBearing]=new CurveVariable();
	m_mapVars[kArcTangentOutBearing]=new CurveVariable();
	m_mapVars[kArcChordBearing]=new CurveVariable();
	m_mapVars[kArcRadialInBearing]=new CurveVariable();
	m_mapVars[kArcRadialOutBearing]=new CurveVariable();
	//Curve distance parameters:
	m_mapVars[kArcRadius]=new CurveVariable();
	m_mapVars[kArcLength]=new CurveVariable();
	m_mapVars[kArcChordLength]=new CurveVariable();
	m_mapVars[kArcExternalDistance]=new CurveVariable();
	m_mapVars[kArcMiddleOrdinate]=new CurveVariable();
	m_mapVars[kArcTangentDistance]=new CurveVariable();
	//Curve point parameters:
	m_mapVars[kArcStartingPoint]=new CurveVariable();
	m_mapVars[kArcMidPoint]=new CurveVariable();
	m_mapVars[kArcEndingPoint]=new CurveVariable();
	m_mapVars[kArcCenter]=new CurveVariable();
	m_mapVars[kArcExternalPoint]=new CurveVariable();
	m_mapVars[kArcChordMidPoint]=new CurveVariable();
	
	//Initialization of the Curve Parameters
	//Curve boolean parameters:
	CurveVariable* pArcConcaveLeft = m_mapVars[kArcConcaveLeft];
	CurveVariable* pArcDeltaGreaterThan180Degrees = m_mapVars[kArcDeltaGreaterThan180Degrees];
	// Curve angle/bearing parameters:
	// (bearings are a type of angle)
	CurveVariable* pArcDelta=m_mapVars[kArcDelta];
	CurveVariable* pArcStartAngle=m_mapVars[kArcStartAngle];
	CurveVariable* pArcEndAngle=m_mapVars[kArcEndAngle];
	CurveVariable* pArcDegreeOfCurveChordDef=m_mapVars[kArcDegreeOfCurveChordDef];
	CurveVariable* pArcDegreeOfCurveArcDef=m_mapVars[kArcDegreeOfCurveArcDef];
	CurveVariable* pArcTangentInBearing=m_mapVars[kArcTangentInBearing];
	CurveVariable* pArcTangentOutBearing=m_mapVars[kArcTangentOutBearing];
	CurveVariable* pArcChordBearing=m_mapVars[kArcChordBearing];
	CurveVariable* pArcRadialInBearing=m_mapVars[kArcRadialInBearing];
	CurveVariable* pArcRadialOutBearing=m_mapVars[kArcRadialOutBearing];
	//Curve distance parameters:
	CurveVariable* pArcRadius=m_mapVars[kArcRadius];
	CurveVariable* pArcLength=m_mapVars[kArcLength];
	CurveVariable* pArcChordLength=m_mapVars[kArcChordLength];
	CurveVariable* pArcExternalDistance=m_mapVars[kArcExternalDistance];
	CurveVariable* pArcMiddleOrdinate=m_mapVars[kArcMiddleOrdinate];
	CurveVariable* pArcTangentDistance=m_mapVars[kArcTangentDistance];
	//Curve point parameters:
	CurveVariable* pArcStartingPoint=m_mapVars[kArcStartingPoint];
	CurveVariable* pArcMidPoint=m_mapVars[kArcMidPoint];
	CurveVariable* pArcEndingPoint=m_mapVars[kArcEndingPoint];
	CurveVariable* pArcCenter=m_mapVars[kArcCenter];
	CurveVariable* pArcExternalPoint=m_mapVars[kArcExternalPoint];
	CurveVariable* pArcChordMidPoint=m_mapVars[kArcChordMidPoint];

	//Instantiation of All the Variable Relation ship classes to call the 
	//respective constructors
	
	//1) ArcDelta,ArcTangentInBearing,ArcTangentOutBearing and ArcConcaveLeft	
	//pushing this variable relationship in to 'm_vecVarRelationship' vector
	m_vecVarRelationship.push_back(
		new VRDeltaToTinToutBearing(
		pArcConcaveLeft,pArcDelta,pArcTangentInBearing,pArcTangentOutBearing) );
	
	//2) ArcDelta,ArcChordLength,ArcRadius,ArcDeltaGreaterThan180Degrees
	//pushing this variable relationship in to 'm_vecVarRelationship' vector
	m_vecVarRelationship.push_back(
		new VRDeltaToChordLengthRadius(
		pArcDeltaGreaterThan180Degrees,pArcDelta,pArcChordLength,pArcRadius) );

	//3) ArcRadialOutBearing,ArcTangentOutBearing and ArcConcaveLeft
	//pushing this variable relationship in to 'm_vecVarRelationship' vector
	m_vecVarRelationship.push_back(
		new VRRoutBearingToToutBearing(pArcConcaveLeft,pArcRadialOutBearing,pArcTangentOutBearing) );
	
	//4) ArcRadialInBearing,ArcTangentInBearing and ArcConcaveLeft
	//pushing this variable relationship in to 'm_vecVarRelationship' vector
	m_vecVarRelationship.push_back(
		new VRRinBearingToTinBearing(pArcConcaveLeft,pArcRadialInBearing,pArcTangentInBearing) );

	//5) ArcChordBearing,ArcTangentInBearing,ArcDelta,ArcConcaveLeft
	//pushing this variable relationship in to 'm_vecVarRelationship' vector
	m_vecVarRelationship.push_back(
		new VRChordBearingToTinDelta(pArcConcaveLeft,pArcChordBearing,pArcTangentInBearing,pArcDelta) );
	
	//6) ArcChordBearing,ArcTangentOutBearing,ArcDelta,ArcConcaveLeft
	//pushing this variable relationship in to 'm_vecVarRelationship' vector
	m_vecVarRelationship.push_back(
		new VRChordBearingToToutDelta(pArcConcaveLeft,pArcChordBearing,pArcTangentOutBearing,pArcDelta) );
	
	//7) ArcStartAngle,ArcTangentInBearing,ArcConcaveLeft
	//pushing this variable relationship in to 'm_vecVarRelationship' vector
	m_vecVarRelationship.push_back(
		new VRStartAngleToTinBearing(pArcConcaveLeft,pArcStartAngle,pArcTangentInBearing) );
	
	//8) ArcEndAngle,ArcTangentOutBearing,ArcConcaveLeft
	//pushing this variable relationship in to 'm_vecVarRelationship' vector
	m_vecVarRelationship.push_back(
		new VREndAngleToToutBearing(pArcConcaveLeft,pArcEndAngle,pArcTangentOutBearing) );
	
	//9) ArcMiddleOrdinate,ArcRadius,ArcDelta
	//pushing this variable relationship in to 'm_vecVarRelationship' vector
	m_vecVarRelationship.push_back(
		new VRMiddleOrdinateToRadiusDelta(pArcMiddleOrdinate,pArcRadius,pArcDelta) );
	
	//11) ArcLength,ArcRadius,ArcDelta
	//pushing this variable relationship in to 'm_vecVarRelationship' vector
	m_vecVarRelationship.push_back(
		new VRArcLengthToRadiusDelta(pArcLength,pArcRadius,pArcDelta) );
	
	//12) ArcCenter,ArcStartingPoint,ArcRadius,ArcRadialInBearing
	//pushing this variable relationship in to 'm_vecVarRelationship' vector
	m_vecVarRelationship.push_back(
		new VRCenterToStartingPointRadiusRin(pArcCenter,pArcStartingPoint,pArcRadius,pArcRadialInBearing) );
	
	//13) ArcCenter,ArcEndingPoint,ArcRadius,ArcRadialOutBearing
	//pushing this variable relationship in to 'm_vecVarRelationship' vector
	m_vecVarRelationship.push_back(
		new VRCenterToEndPointRadiusRout(pArcCenter,pArcEndingPoint,pArcRadius,pArcRadialOutBearing) );
	
	//14) ArcEndingPoint,ArcStartingPoint,ArcChordLength,ArcChordBearing
	//pushing this variable relationship in to 'm_vecVarRelationship' vector
	m_vecVarRelationship.push_back(
		new VREndPointToStartingPointChordLengthChordBearing(
		pArcEndingPoint,pArcStartingPoint,pArcChordLength,pArcChordBearing) );
	
	//15) ArcMidPoint,ArcCenter,ArcRadius,ArcChordBearing,ArcConcaveLeft
	//pushing this variable relationship in to 'm_vecVarRelationship' vector
	m_vecVarRelationship.push_back(
		new VRMiddleToCenterPointRadiusChordBearing(
		pArcConcaveLeft,pArcMidPoint,pArcCenter,pArcRadius,pArcChordBearing) );
	
	//16) ArcDegreeOfCurveArcDef,ArcRadius
	//pushing this variable relationship in to 'm_vecVarRelationship' vector
	m_vecVarRelationship.push_back(
		new VRDegreeCurveArcToRadius(pArcDegreeOfCurveArcDef,pArcRadius) );
	
	//17) ArcDegreeOfCurveChordDef,ArcRadius
	//pushing this variable relationship in to 'm_vecVarRelationship' vector
	m_vecVarRelationship.push_back(
		new VRDegreeCurveChordToRadius(pArcDegreeOfCurveChordDef,pArcRadius) );
	
	//18) ArcTangentDistance,ArcRadius,ArcDelta
	//pushing this variable relationship in to 'm_vecVarRelationship' vector
	m_vecVarRelationship.push_back(
		new VRTangentDistanceToRadiusDelta(pArcTangentDistance,pArcRadius,pArcDelta) );
	
	//19) ArcExternalDistance,ArcRadius,ArcDelta
	//pushing this variable relationship in to 'm_vecVarRelationship' vector
	m_vecVarRelationship.push_back(
		new VRExternalDistanceToRadiusDelta(pArcExternalDistance,pArcRadius,pArcDelta) );
	
	//20) ArcChordMidPoint,ArcStartingPoint,ArcChordLength,ArcChordBearing	
	//pushing this variable relationship in to 'm_vecVarRelationship' vector
	m_vecVarRelationship.push_back(
		new VRChordMidPointToStartingPointChordBearing(
		pArcChordMidPoint,pArcStartingPoint,pArcChordLength,pArcChordBearing) );
	
	//21) ArcExternalPoint,ArcStartingPoint,ArcTangentDistance,ArcTangentInBearing
	//pushing this variable relationship in to 'm_vecVarRelationship' vector
	m_vecVarRelationship.push_back(
		new VRExternalPointToStartingPointTangentDistance(
		pArcDelta,pArcExternalPoint,pArcStartingPoint,pArcTangentDistance,pArcTangentInBearing) );
	
	//22) ArcChordMidPoint,ArcStartingPoint,ArcEndingPoint	
	//pushing this variable relationship in to 'm_vecVarRelationship' vector
	m_vecVarRelationship.push_back(
		new VRChordMidPointToStartingEndingPoint(
		pArcChordMidPoint,pArcStartingPoint,pArcEndingPoint) );

	//23)Mid point of arc, Mid point of chord, Mid ordinate 
	//pushing this variable relationship in to 'm_vecVarRelationship' vector
	m_vecVarRelationship.push_back(
		new VRMiddleToChordMidPointMidOrdinateChordBearing(
		pArcConcaveLeft,pArcMidPoint, pArcChordMidPoint,pArcMiddleOrdinate, pArcChordBearing) );

	//24)Mid ordinate, radius and chord bearing
	//pushing this variable relationship in to 'm_vecVarRelationship' vector
	m_vecVarRelationship.push_back(
		new VRRadiusToMidOrdinateChordLength(
		pArcMiddleOrdinate,pArcRadius,pArcChordLength,pArcDeltaGreaterThan180Degrees) );

	// 25) delta angle and bDeltaGreaterThan180 degrees
	//pushing this variable relationship in to 'm_vecVarRelationship' vector
	m_vecVarRelationship.push_back(
		new VRDeltaAngleToDeltaGreaterThan180(pArcDelta, pArcDeltaGreaterThan180Degrees) );

	//reset the Curve Parameters
	reset();
}
//==========================================================================================
CurveCalculationEngineImpl::~CurveCalculationEngineImpl()
{
	// clean the pointers on the heap
	int nSize = m_vecVarRelationship.size();
	for (int n=0; n<nSize; n++)
	{
		delete m_vecVarRelationship[n];
	}
	m_vecVarRelationship.clear();

	delete m_mapVars[kArcConcaveLeft];
	delete m_mapVars[kArcDeltaGreaterThan180Degrees];
	//Curve angle/bearing parameters:(bearings are a type of angle)
	delete m_mapVars[kArcDelta];
	delete m_mapVars[kArcStartAngle];
	delete m_mapVars[kArcEndAngle];
	delete m_mapVars[kArcDegreeOfCurveChordDef];
	delete m_mapVars[kArcDegreeOfCurveArcDef];
	delete m_mapVars[kArcTangentInBearing];
	delete m_mapVars[kArcTangentOutBearing];
	delete m_mapVars[kArcChordBearing];
	delete m_mapVars[kArcRadialInBearing];
	delete m_mapVars[kArcRadialOutBearing];
	//Curve distance parameters:
	delete m_mapVars[kArcRadius];
	delete m_mapVars[kArcLength];
	delete m_mapVars[kArcChordLength];
	delete m_mapVars[kArcExternalDistance];
	delete m_mapVars[kArcMiddleOrdinate];
	delete m_mapVars[kArcTangentDistance];
	//Curve point parameters:
	delete m_mapVars[kArcStartingPoint];
	delete m_mapVars[kArcMidPoint];
	delete m_mapVars[kArcEndingPoint];
	delete m_mapVars[kArcCenter];
	delete m_mapVars[kArcExternalPoint];
	delete m_mapVars[kArcChordMidPoint];
	m_mapVars.clear();
}
//==========================================================================================
void CurveCalculationEngineImpl::setCurvePointParameter(ECurveParameterType eParameter,double dX,double dY)
{
	//assign value to the Curve Point Parameter
	m_mapVars[eParameter]->assignPointParameter(dX,dY);
}
//==========================================================================================
void CurveCalculationEngineImpl::setCurveAngleOrBearingParameter(ECurveParameterType eParameter,double dValue)
{
	//converting the radians to degrees
	dValue=dValue*(180/gdPI);
	
	//checking for 0<Curve Parameter Angle<360
	double dMult = floor(dValue / 360);
	dValue = dValue - dMult * 360;
	
	//assign value to the Curve Angle or Bearing Parameter
	m_mapVars[eParameter]->assign(dValue);
}
//==========================================================================================
void CurveCalculationEngineImpl::setCurveDistanceParameter(ECurveParameterType eParameter,double dValue)
{
	//assign value to the Curve Distance Parameter
	m_mapVars[eParameter]->assign(dValue);
}
//==========================================================================================
void CurveCalculationEngineImpl::setCurveBooleanParameter(ECurveParameterType eParameter,bool bValue)
{
	//assing value to the Curve Boolean Parameter
	if (bValue)
		m_mapVars[eParameter]->assign(1.0);
	else
		m_mapVars[eParameter]->assign(0.0);
}
//==========================================================================================
bool CurveCalculationEngineImpl::canCalculateAllParameters()
{
	vector<CurveVariableRelationship*> vecVRToExempt;
	vecVRToExempt.clear();

	//Check whether all the variable values can be calculated,if so return 'True' 
	//otherwise return 'False'
	if((m_mapVars[kArcDelta]->canGetValue(vecVRToExempt))&&
		(m_mapVars[kArcStartAngle]->canGetValue(vecVRToExempt))&&
		(m_mapVars[kArcEndAngle]->canGetValue(vecVRToExempt))&&
		(m_mapVars[kArcDegreeOfCurveChordDef]->canGetValue(vecVRToExempt))&&
		(m_mapVars[kArcDegreeOfCurveArcDef]->canGetValue(vecVRToExempt))&&
		(m_mapVars[kArcTangentInBearing]->canGetValue(vecVRToExempt))&&
		(m_mapVars[kArcTangentOutBearing]->canGetValue(vecVRToExempt))&&
		(m_mapVars[kArcChordBearing]->canGetValue(vecVRToExempt))&&
		(m_mapVars[kArcRadialInBearing]->canGetValue(vecVRToExempt))&&
		(m_mapVars[kArcRadialOutBearing]->canGetValue(vecVRToExempt))&&
		(m_mapVars[kArcRadius]->canGetValue(vecVRToExempt))&&
		(m_mapVars[kArcLength]->canGetValue(vecVRToExempt))&&
		(m_mapVars[kArcChordLength]->canGetValue(vecVRToExempt))&&
		(m_mapVars[kArcExternalDistance]->canGetValue(vecVRToExempt))&&
		(m_mapVars[kArcMiddleOrdinate]->canGetValue(vecVRToExempt))&&
		(m_mapVars[kArcTangentDistance]->canGetValue(vecVRToExempt))&&
		(m_mapVars[kArcStartingPoint]->canGetPointParamXValue(vecVRToExempt))&&
		(m_mapVars[kArcMidPoint]->canGetPointParamXValue(vecVRToExempt))&&
		(m_mapVars[kArcEndingPoint]->canGetPointParamXValue(vecVRToExempt))&&
		(m_mapVars[kArcCenter]->canGetPointParamXValue(vecVRToExempt))&&
		(m_mapVars[kArcChordMidPoint]->canGetPointParamXValue(vecVRToExempt))&&
		(m_mapVars[kArcExternalPoint]->canGetPointParamXValue(vecVRToExempt)))
	{
		return true;
	}

	return false;
}
//==========================================================================================
void CurveCalculationEngineImpl::getStartingPoint(double& rdX,double& rdY)
{
	//Validating all the constraints
	CheckForConstraints();
	//value of Concavity based on the other Curve Parameters
	calcConcavity();
	//value of 'bDeltaOver180' based on ArcDelta 
	calcDeltaOver();

	vector<CurveVariableRelationship*> vecVRToExempt;
	vecVRToExempt.clear();

	//The 'X' position of the Starting Point, to be returned to the caller
	rdX=(m_mapVars[kArcStartingPoint]->getPointParamXValue(vecVRToExempt));
	//The 'Y' position of the Starting Point, to be returned to the caller
	rdY=(m_mapVars[kArcStartingPoint]->getPointParamYValue(vecVRToExempt));
}
//==========================================================================================
void CurveCalculationEngineImpl::getMidPoint(double& mdX,double& mdY)
{
	//Validating all the constraints
	CheckForConstraints();
	//value of Concavity based on the other Curve Parameters
	calcConcavity();
	//value of 'bDeltaOver180' based on ArcDelta 
	calcDeltaOver();

	vector<CurveVariableRelationship*> vecVRToExempt;
	vecVRToExempt.clear();

	//The 'X' position of the Mid Point, to be returned to the caller
	mdX=(m_mapVars[kArcMidPoint]->getPointParamXValue(vecVRToExempt));
	//The 'Y' position of the Mid Point, to be returned to the caller
	mdY=(m_mapVars[kArcMidPoint]->getPointParamYValue(vecVRToExempt));
}
//==========================================================================================
void CurveCalculationEngineImpl::getEndingPoint(double& edX,double& edY)
{

	//Validating all the constraints
	CheckForConstraints();
	//value of Concavity based on the other Curve Parameters
	calcConcavity();
	//value of 'bDeltaOver180' based on ArcDelta 
	calcDeltaOver();

	vector<CurveVariableRelationship*> vecVRToExempt;
	vecVRToExempt.clear();

	//The 'X' position of the Ending Point, to be returned to the caller
	edX=(m_mapVars[kArcEndingPoint]->getPointParamXValue(vecVRToExempt));
	//The 'Y' position of the Ending Point, to be returned to the caller
	edY=(m_mapVars[kArcEndingPoint]->getPointParamYValue(vecVRToExempt));
}
//==========================================================================================
string CurveCalculationEngineImpl::getCurveParameter(ECurveParameterType eCurveParameter)
{
	//Validating all the constraints
	CheckForConstraints();
	//value of Concavity based on the other Curve Parameters
	calcConcavity();
	//value of 'bDeltaOver180' based on ArcDelta 
	calcDeltaOver();
	
	vector<CurveVariableRelationship*> vecVRToExempt;
	vecVRToExempt.clear();

	//getting the required Curve Parameter 
	switch(eCurveParameter)
	{
		//Curve boolean parameters
	case kArcConcaveLeft:
		{
			if((m_mapVars[kArcConcaveLeft]->getBooleanValue(vecVRToExempt)))
			{
				m_strCurveParam="1";
				return m_strCurveParam;
			}
			else
			{
				m_strCurveParam="0";
				return m_strCurveParam;
			}
			break;
		}
	case kArcDeltaGreaterThan180Degrees:
		{
			if((m_mapVars[kArcDeltaGreaterThan180Degrees]->getBooleanValue(vecVRToExempt)))
			{
				m_strCurveParam="1";
				return m_strCurveParam;
			}
			else
			{
				m_strCurveParam="0";
				return m_strCurveParam;
			}
			break;
		}
		//Curve angle/bearing parameters(bearings are a type of angle)
		//To convert angle from Degrees to Radians, multiply angle in degrees by (pi/180)
	case kArcDelta:
		{
			m_strCurveParam = DoubleToString(m_mapVars[kArcDelta]->getValue(vecVRToExempt)*(gdPI/180));
			return m_strCurveParam;
			break;
		}
	case kArcStartAngle:
		{
			m_strCurveParam = DoubleToString(m_mapVars[kArcStartAngle]->getValue(vecVRToExempt)*(gdPI/180));
			return m_strCurveParam;
			break;
		}
	case kArcEndAngle:
		{
			m_strCurveParam = DoubleToString(m_mapVars[kArcEndAngle]->getValue(vecVRToExempt)*(gdPI/180));
			return m_strCurveParam;
			break;
		}
	case kArcDegreeOfCurveChordDef:
		{
			m_strCurveParam = DoubleToString(m_mapVars[kArcDegreeOfCurveChordDef]->getValue(vecVRToExempt)*(gdPI/180));
			return m_strCurveParam;
			break;
		}
	case kArcDegreeOfCurveArcDef:
		{
			m_strCurveParam = DoubleToString(m_mapVars[kArcDegreeOfCurveArcDef]->getValue(vecVRToExempt)*(gdPI/180));
			return m_strCurveParam;
			break;
		}
	case kArcTangentInBearing:
		{
			m_strCurveParam = DoubleToString(m_mapVars[kArcTangentInBearing]->getValue(vecVRToExempt)*(gdPI/180));
			return m_strCurveParam;
			break;
		}
	case kArcTangentOutBearing:
		{
			m_strCurveParam = DoubleToString(m_mapVars[kArcTangentOutBearing]->getValue(vecVRToExempt)*(gdPI/180));
			return m_strCurveParam;
			break;
		}
	case kArcChordBearing:
		{
			m_strCurveParam = DoubleToString(m_mapVars[kArcChordBearing]->getValue(vecVRToExempt)*(gdPI/180));
			return m_strCurveParam;
			break;
		}
	case kArcRadialInBearing:
		{
			m_strCurveParam = DoubleToString(m_mapVars[kArcRadialInBearing]->getValue(vecVRToExempt)*(gdPI/180));
			return m_strCurveParam;
			break;
		}
	case kArcRadialOutBearing:
		{
			m_strCurveParam = DoubleToString(m_mapVars[kArcRadialOutBearing]->getValue(vecVRToExempt)*(gdPI/180));
			return m_strCurveParam;
			break;
		}
		
		//Curve distance parameters
	case kArcRadius:
		{
			// if starting point and center are already known 
			if((m_mapVars[kArcStartingPoint]->isAssigned())&&(m_mapVars[kArcCenter]->isAssigned()))
			{
				m_strCurveParam = DoubleToString(sqrt((((m_mapVars[kArcStartingPoint]->getPointParamXValue(vecVRToExempt)-m_mapVars[kArcCenter]->getPointParamXValue(vecVRToExempt))*
					(m_mapVars[kArcStartingPoint]->getPointParamXValue(vecVRToExempt)-m_mapVars[kArcCenter]->getPointParamXValue(vecVRToExempt)))
					+((m_mapVars[kArcStartingPoint]->getPointParamYValue(vecVRToExempt)-m_mapVars[kArcCenter]->getPointParamYValue(vecVRToExempt))*
					(m_mapVars[kArcStartingPoint]->getPointParamYValue(vecVRToExempt)-m_mapVars[kArcCenter]->getPointParamYValue(vecVRToExempt))))));
				return m_strCurveParam;
			}
			// if ending point and center are already known 
			if((m_mapVars[kArcEndingPoint]->isAssigned())&&(m_mapVars[kArcCenter]->isAssigned()))
			{
				m_strCurveParam = DoubleToString(sqrt((((m_mapVars[kArcEndingPoint]->getPointParamXValue(vecVRToExempt)-m_mapVars[kArcCenter]->getPointParamXValue(vecVRToExempt))*
					(m_mapVars[kArcEndingPoint]->getPointParamXValue(vecVRToExempt)-m_mapVars[kArcCenter]->getPointParamXValue(vecVRToExempt)))
					+((m_mapVars[kArcEndingPoint]->getPointParamYValue(vecVRToExempt)-m_mapVars[kArcCenter]->getPointParamYValue(vecVRToExempt))*
					(m_mapVars[kArcEndingPoint]->getPointParamYValue(vecVRToExempt)-m_mapVars[kArcCenter]->getPointParamYValue(vecVRToExempt))))));
				return m_strCurveParam;
			}
			m_strCurveParam = DoubleToString(m_mapVars[kArcRadius]->getValue(vecVRToExempt));
			return m_strCurveParam;
			break;
		}
	case kArcLength:
		{
			m_strCurveParam = DoubleToString(m_mapVars[kArcLength]->getValue(vecVRToExempt));
			return m_strCurveParam;
			break;
		}
	case kArcChordLength:
		{
			// if starting point and ending point are already known 
			if((m_mapVars[kArcStartingPoint]->isAssigned())&&(m_mapVars[kArcEndingPoint]->isAssigned()))
			{
				m_strCurveParam = DoubleToString(sqrt((((m_mapVars[kArcStartingPoint]->getPointParamXValue(vecVRToExempt)-m_mapVars[kArcEndingPoint]->getPointParamXValue(vecVRToExempt))*
					(m_mapVars[kArcStartingPoint]->getPointParamXValue(vecVRToExempt)-m_mapVars[kArcEndingPoint]->getPointParamXValue(vecVRToExempt)))
					+((m_mapVars[kArcStartingPoint]->getPointParamYValue(vecVRToExempt)-m_mapVars[kArcEndingPoint]->getPointParamYValue(vecVRToExempt))*
					(m_mapVars[kArcStartingPoint]->getPointParamYValue(vecVRToExempt)-m_mapVars[kArcEndingPoint]->getPointParamYValue(vecVRToExempt))))));
				return m_strCurveParam;
			}
			m_strCurveParam = DoubleToString(m_mapVars[kArcChordLength]->getValue(vecVRToExempt));
			return m_strCurveParam;
			break;
		}
	case kArcExternalDistance:
		{
			// if chord mid point and external point are already known 
			if((m_mapVars[kArcChordMidPoint]->isAssigned())&&(m_mapVars[kArcExternalPoint]->isAssigned()))
			{
				m_strCurveParam = DoubleToString(sqrt((((m_mapVars[kArcChordMidPoint]->getPointParamXValue(vecVRToExempt)-m_mapVars[kArcExternalPoint]->getPointParamXValue(vecVRToExempt))*
					(m_mapVars[kArcChordMidPoint]->getPointParamXValue(vecVRToExempt)-m_mapVars[kArcExternalPoint]->getPointParamXValue(vecVRToExempt)))
					+((m_mapVars[kArcChordMidPoint]->getPointParamYValue(vecVRToExempt)-m_mapVars[kArcExternalPoint]->getPointParamYValue(vecVRToExempt))*
					(m_mapVars[kArcChordMidPoint]->getPointParamYValue(vecVRToExempt)-m_mapVars[kArcExternalPoint]->getPointParamYValue(vecVRToExempt))))));
				return m_strCurveParam;
			}
			m_strCurveParam = DoubleToString(m_mapVars[kArcExternalDistance]->getValue(vecVRToExempt));
			return m_strCurveParam;
			break;
		}
	case kArcMiddleOrdinate:
		{
			m_strCurveParam = DoubleToString(m_mapVars[kArcMiddleOrdinate]->getValue(vecVRToExempt));
			return m_strCurveParam;
			break;
		}
	case kArcTangentDistance:
		{
			// if starting point and external point are already known 
			if((m_mapVars[kArcStartingPoint]->isAssigned())&&(m_mapVars[kArcExternalPoint]->isAssigned()))
			{
				m_strCurveParam = DoubleToString(sqrt((((m_mapVars[kArcStartingPoint]->getPointParamXValue(vecVRToExempt)-m_mapVars[kArcExternalPoint]->getPointParamXValue(vecVRToExempt))*
					(m_mapVars[kArcStartingPoint]->getPointParamXValue(vecVRToExempt)-m_mapVars[kArcExternalPoint]->getPointParamXValue(vecVRToExempt)))
					+((m_mapVars[kArcStartingPoint]->getPointParamYValue(vecVRToExempt)-m_mapVars[kArcExternalPoint]->getPointParamYValue(vecVRToExempt))*
					(m_mapVars[kArcStartingPoint]->getPointParamYValue(vecVRToExempt)-m_mapVars[kArcExternalPoint]->getPointParamYValue(vecVRToExempt))))));
				return m_strCurveParam;
			}
			// if ending point and external points are already known 
			if((m_mapVars[kArcEndingPoint]->isAssigned())&&(m_mapVars[kArcExternalPoint]->isAssigned()))
			{
				m_strCurveParam = DoubleToString(sqrt((((m_mapVars[kArcEndingPoint]->getPointParamXValue(vecVRToExempt)-m_mapVars[kArcExternalPoint]->getPointParamXValue(vecVRToExempt))*
					(m_mapVars[kArcEndingPoint]->getPointParamXValue(vecVRToExempt)-m_mapVars[kArcExternalPoint]->getPointParamXValue(vecVRToExempt)))
					+((m_mapVars[kArcEndingPoint]->getPointParamYValue(vecVRToExempt)-m_mapVars[kArcExternalPoint]->getPointParamYValue(vecVRToExempt))*
					(m_mapVars[kArcEndingPoint]->getPointParamYValue(vecVRToExempt)-m_mapVars[kArcExternalPoint]->getPointParamYValue(vecVRToExempt))))));
				return m_strCurveParam;
			}
			m_strCurveParam = DoubleToString(m_mapVars[kArcTangentDistance]->getValue(vecVRToExempt));
			return m_strCurveParam;
			break;
		}
		
		//Curve point parameters:
	case kArcStartingPoint:
		{
			m_strCurveParam = PointToString(m_mapVars[kArcStartingPoint]->getPointParamXValue(vecVRToExempt)
				,m_mapVars[kArcStartingPoint]->getPointParamYValue(vecVRToExempt));
			return m_strCurveParam;
			break;
		}
	case kArcMidPoint:
		{
			m_strCurveParam = PointToString(m_mapVars[kArcMidPoint]->getPointParamXValue(vecVRToExempt)
				,m_mapVars[kArcMidPoint]->getPointParamYValue(vecVRToExempt));
			return m_strCurveParam;
			break;
		}
	case kArcEndingPoint:
		{
			m_strCurveParam = PointToString(m_mapVars[kArcEndingPoint]->getPointParamXValue(vecVRToExempt)
				,m_mapVars[kArcEndingPoint]->getPointParamYValue(vecVRToExempt));
			return m_strCurveParam;
			break;
		}
	case kArcCenter:
		{
			m_strCurveParam = PointToString(m_mapVars[kArcCenter]->getPointParamXValue(vecVRToExempt)
				,m_mapVars[kArcCenter]->getPointParamYValue(vecVRToExempt));
			return m_strCurveParam;
			break;
		}
	case kArcExternalPoint:
		{
			if(!(((int)m_mapVars[kArcDelta]->getValue(vecVRToExempt))%180))
			{
				UCLIDException e("ELI90245","The External Point doesn't exist for the given Delta Value");
				throw e;
			}
			else
				m_strCurveParam = PointToString(m_mapVars[kArcExternalPoint]->getPointParamXValue(vecVRToExempt)
				,m_mapVars[kArcExternalPoint]->getPointParamYValue(vecVRToExempt));
			return m_strCurveParam;
			break;
		}
	case kArcChordMidPoint:
		{
			m_strCurveParam = PointToString(m_mapVars[kArcChordMidPoint]->getPointParamXValue(vecVRToExempt)
				,m_mapVars[kArcChordMidPoint]->getPointParamYValue(vecVRToExempt));
			return m_strCurveParam;
			break;
		}
	}
	return "The Requested Parameter cannot be Calculatable";
}
//==========================================================================================
CurveVariable& CurveCalculationEngineImpl::operator [](ECurveParameterType eCurveParameter)
{
	return *(m_mapVars[eCurveParameter]);
}
//==========================================================================================
bool CurveCalculationEngineImpl::reset()
{
	//Unset the values associated with all Curve Parameters
	
	//Curve boolean parameters:
	m_mapVars[kArcConcaveLeft]->reset();
	m_mapVars[kArcDeltaGreaterThan180Degrees]->reset();
	
	//Curve angle/bearing parameters:
	// (bearings are a type of angle)
	m_mapVars[kArcDelta]->reset();
	m_mapVars[kArcStartAngle]->reset();
	m_mapVars[kArcEndAngle]->reset();
	m_mapVars[kArcDegreeOfCurveChordDef]->reset();
	m_mapVars[kArcDegreeOfCurveArcDef]->reset();
	m_mapVars[kArcTangentInBearing]->reset();
	m_mapVars[kArcTangentOutBearing]->reset();
	m_mapVars[kArcChordBearing]->reset();
	m_mapVars[kArcRadialInBearing]->reset();
	m_mapVars[kArcRadialOutBearing]->reset();
	
	//Curve distace parameters
	m_mapVars[kArcRadius]->reset();
	m_mapVars[kArcLength]->reset();
	m_mapVars[kArcChordLength]->reset();
	m_mapVars[kArcExternalDistance]->reset();
	m_mapVars[kArcMiddleOrdinate]->reset();
	m_mapVars[kArcTangentDistance]->reset();
	
	//Curve point parameters:
	m_mapVars[kArcStartingPoint]->reset();
	m_mapVars[kArcMidPoint]->reset();
	m_mapVars[kArcEndingPoint]->reset();
	m_mapVars[kArcCenter]->reset();
	m_mapVars[kArcExternalPoint]->reset();
	m_mapVars[kArcChordMidPoint]->reset();
		
	return true;
}
//==========================================================================================
string CurveCalculationEngineImpl::DoubleToString(double dValue)
{
	char pszBuffer[100];
	sprintf_s(pszBuffer, sizeof(pszBuffer), "%.15f", dValue);
	return string(pszBuffer);
}
//==========================================================================================
string CurveCalculationEngineImpl::PointToString(double dX, double dY)
{
	char pszBuffer[100];
	sprintf_s(pszBuffer, sizeof(pszBuffer), "%.15f,%.15f", dX, dY);
	return string(pszBuffer);
}
//==========================================================================================
void CurveCalculationEngineImpl::CheckForConstraints()
{
	vector<CurveVariableRelationship*> vecVRToExempt;
	vecVRToExempt.clear();

	//Validating the Constraint : (ChordLength <= 2 * Radius)
	if((m_mapVars[kArcRadius]->isAssigned())&&(m_mapVars[kArcChordLength]->isAssigned()))
	{
		double dChordLen = m_mapVars[kArcChordLength]->getValue(vecVRToExempt);
		double dRadius = m_mapVars[kArcRadius]->getValue(vecVRToExempt);
		if (dChordLen - 2*dRadius > 0.0)
		{
			UCLIDException e("ELI90221","Chord length shall NOT be greater than the curve diameter. Please try this curve again with proper values.");
			throw e;
		}
	}
	//Validating the constraint : Tin != Tout
	if((m_mapVars[kArcTangentInBearing]->isAssigned())&&(m_mapVars[kArcTangentOutBearing]->isAssigned()))
	{
		if((m_mapVars[kArcTangentInBearing]->getValue(vecVRToExempt))==(m_mapVars[kArcTangentOutBearing]->getValue(vecVRToExempt)))
		{
			UCLIDException e("ELI90226","Tangent-in bearing shall not be equal to Tangent-out bearing. Please try this curve again with proper values.");
			throw e;
		}
	}
	//Validating the constraint : Tin != CB
	if((m_mapVars[kArcTangentInBearing]->isAssigned())&&(m_mapVars[kArcChordBearing]->isAssigned()))
	{
		if((m_mapVars[kArcTangentInBearing]->getValue(vecVRToExempt))==(m_mapVars[kArcChordBearing]->getValue(vecVRToExempt)))
		{
			UCLIDException e("ELI90227","Tangent-in bearing shall not be equal to Chord bearing. Please try this curve again with proper values.");
			throw e;
		}
	}
	//Validating the constraint : Tout != ChordBearing
	if((m_mapVars[kArcTangentOutBearing]->isAssigned())&&(m_mapVars[kArcChordBearing]->isAssigned()))
	{
		if((m_mapVars[kArcTangentOutBearing]->getValue(vecVRToExempt))==(m_mapVars[kArcChordBearing]->getValue(vecVRToExempt)))
		{
			UCLIDException e("ELI90228","Tangent-out shall not be equal to Chord bearing. Please try this curve again with proper values.");
			throw e;
		}
	}
	//Validating the constraint : ArcLength > ChordLength
	if((m_mapVars[kArcLength]->isAssigned())&&(m_mapVars[kArcChordLength]->isAssigned()))
	{
		double dArcLen = m_mapVars[kArcLength]->getValue(vecVRToExempt);
		double dChordLen = m_mapVars[kArcChordLength]->getValue(vecVRToExempt);
		if(dArcLen - dChordLen <= MathVars::ZERO)
		{
			UCLIDException e("ELI90240","ArcLength should be greater than ChordLength. Please try this curve again with proper values.");
			throw e;
		}
	}
	//Validating the constraint : abs(Rin-CB) < 90°
	if((m_mapVars[kArcRadialInBearing]->isAssigned())&&(m_mapVars[kArcChordBearing]->isAssigned()))
	{
		double dDiff = fabs( m_mapVars[kArcRadialInBearing]->getValue(vecVRToExempt) - m_mapVars[kArcChordBearing]->getValue(vecVRToExempt));
		if (dDiff - 90.0 >= 0.0 && dDiff - 270.0 <= MathVars::ZERO)
		{
			UCLIDException e("ELI90241","Angle difference between Radial-in bearing and Chord bearing shall not be greater than or equal to 90 degrees. Please try this curve again with proper values.");
			e.addDebugInfo("Angle Difference", dDiff);
			throw e;
		}
	}
	//Validating the constraint : abs(Rout-CB) < 90°
	if((m_mapVars[kArcRadialOutBearing]->isAssigned())&&(m_mapVars[kArcChordBearing]->isAssigned()))
	{
		double dDiff = fabs( m_mapVars[kArcRadialOutBearing]->getValue(vecVRToExempt) - m_mapVars[kArcChordBearing]->getValue(vecVRToExempt) );
		if (dDiff - 90.0 >= 0.0 && dDiff - 270.0 <= MathVars::ZERO)
		{
			UCLIDException e("ELI90242","Angle difference between Radial-out bearing and Chord bearing shall not be greater than or equal to 90 degrees. Please try this curve again with proper values.");
			e.addDebugInfo("Angle Difference", dDiff);
			throw e;
		}
	}

	//Validating the Constraint : (Radius (calculated from Degree of Curve Arc Def) >= ChordLength/2)
	if (m_mapVars[kArcDegreeOfCurveArcDef]->isAssigned() && m_mapVars[kArcChordLength]->isAssigned())
	{
		double dChordLen = m_mapVars[kArcChordLength]->getValue(vecVRToExempt);
		double dDegreeOfCurve = m_mapVars[kArcDegreeOfCurveArcDef]->getValue(vecVRToExempt);
		double dRadius = 18000.0/(dDegreeOfCurve * MathVars::PI);
		if (dChordLen > 2*dRadius)
		{
			UCLIDException e("ELI13438", "Calculated curve Diameter is less than Chord length. Either the value of Chord length or Degree of curve (Arc Def) is too large. Please try this curve again with proper values.");
			e.addDebugInfo("Chord length", dChordLen);
			e.addDebugInfo("Calculated Diameter", 2*dRadius);
			throw e;
		}
	}

	//Validating the Constraint : (Radius (calculated from Degree of Curve Chord Def) >= ChordLength/2)
	if (m_mapVars[kArcDegreeOfCurveChordDef]->isAssigned() && m_mapVars[kArcChordLength]->isAssigned())
	{
		double dChordLen = m_mapVars[kArcChordLength]->getValue(vecVRToExempt);
		double dDegreeOfCurve = m_mapVars[kArcDegreeOfCurveChordDef]->getValue(vecVRToExempt);
		double dRadius = 50.0 / sin (dDegreeOfCurve/2.0 * (MathVars::PI/180.0));
		if (dChordLen > 2*dRadius)
		{
			UCLIDException e("ELI13439", "Calculated curve Diameter is less than Chord length. Either the value of Chord length or Degree of curve (Chord Def) is too large. Please try this curve again with proper values.");
			e.addDebugInfo("Chord length", dChordLen);
			e.addDebugInfo("Calculated Diameter", 2*dRadius);
			throw e;
		}
	}

/*	//Validating the constraint : Rin != Rout   ------- actually, they can be the same
	if((m_mapVars[kArcRadialInBearing]->isAssigned())&&(m_mapVars[kArcRadialOutBearing]->isAssigned()))
	{
		if((m_mapVars[kArcRadialInBearing]->getValue(vecVRToExempt))==(m_mapVars[kArcRadialOutBearing]->getValue(vecVRToExempt)))
		{
			UCLIDException e("ELI90243","Radial-in bearing should not be equal to Radial-out bearing. Please try this curve again with proper values.");
			throw e;
		}
	}
*/
}
//==========================================================================================
void CurveCalculationEngineImpl::calcConcavity()
{
	// only calculate the concavity if it's unassigned
	if (!m_mapVars[kArcConcaveLeft]->isAssigned())
	{
		vector<CurveVariableRelationship*> vecVRToExempt;
		vecVRToExempt.clear();
		
		//Value of concavity based on Tin and Chord bearings
		if((m_mapVars[kArcTangentInBearing]->isAssigned())&&(m_mapVars[kArcChordBearing]->isAssigned()))
		{
			double dCheckRange=((m_mapVars[kArcTangentInBearing]->getValue(vecVRToExempt))-(m_mapVars[kArcChordBearing]->getValue(vecVRToExempt)));
			if(((0<dCheckRange)&&(dCheckRange<180))||((-360<dCheckRange)&&(dCheckRange<-180)))
				m_mapVars[kArcConcaveLeft]->assign(0);
			else if(((-180<dCheckRange)&&(dCheckRange<0))||((180<dCheckRange)&&(dCheckRange<360)))
				m_mapVars[kArcConcaveLeft]->assign(1);
		}
		//Value of concavity based on Tout and Chord bearings
		else if((m_mapVars[kArcTangentOutBearing]->isAssigned())&&(m_mapVars[kArcChordBearing]->isAssigned()))
		{
			double dCheckRange=((m_mapVars[kArcChordBearing]->getValue(vecVRToExempt))-(m_mapVars[kArcTangentOutBearing]->getValue(vecVRToExempt)));
			if(((0<dCheckRange)&&(dCheckRange<180))||((-360<dCheckRange)&&(dCheckRange<-180)))
				m_mapVars[kArcConcaveLeft]->assign(0);
			else if(((-180<dCheckRange)&&(dCheckRange<0))||((180<dCheckRange)&&(dCheckRange<360)))
				m_mapVars[kArcConcaveLeft]->assign(1);
		}
		else if( (m_mapVars[kArcStartingPoint]->isAssigned())||(m_mapVars[kArcMidPoint]->isAssigned())
				||(m_mapVars[kArcEndingPoint]->isAssigned())||(m_mapVars[kArcCenter]->isAssigned())
				||(m_mapVars[kArcRadius]->isAssigned())||(m_mapVars[kArcChordLength]->isAssigned())
				||(m_mapVars[kArcDegreeOfCurveChordDef]->isAssigned())||(m_mapVars[kArcDegreeOfCurveArcDef]->isAssigned()))
		{

			//if start, mid and end point are given, concavity can be calculated
			if (m_mapVars[kArcStartingPoint]->isAssigned()
				&& m_mapVars[kArcMidPoint]->isAssigned()
				&& m_mapVars[kArcEndingPoint]->isAssigned())
			{
				double dStartX = m_mapVars[kArcStartingPoint]->getPointParamXValue(vecVRToExempt);
				double dStartY = m_mapVars[kArcStartingPoint]->getPointParamYValue(vecVRToExempt);
				double dMidX = m_mapVars[kArcMidPoint]->getPointParamXValue(vecVRToExempt);
				double dMidY = m_mapVars[kArcMidPoint]->getPointParamYValue(vecVRToExempt);
				double dEndX = m_mapVars[kArcEndingPoint]->getPointParamXValue(vecVRToExempt);
				double dEndY = m_mapVars[kArcEndingPoint]->getPointParamYValue(vecVRToExempt);
				Point start(dStartX, dStartY);
				Point mid(dMidX, dMidY);
				Point end(dEndX, dEndY);

				// get the angle from start to mid and mid to end, then compare them
				// if angle start to mid is greater than end to mid, the curve is going to the left
				double dDiff = start.angleTo(mid) - end.angleTo(mid);
				bool bCurveLeft = true;
				if ( (dDiff > 0 && dDiff < gdPI ) || dDiff < -gdPI)
				{
					bCurveLeft = true;
					m_mapVars[kArcConcaveLeft]->assign(1);
				}
				else if (dDiff > gdPI || (dDiff < 0 && dDiff > -gdPI))
				{
					bCurveLeft = false;
					m_mapVars[kArcConcaveLeft]->assign(0);
				}
				else
				{
					// this is not a curve
					UCLIDException uclidException("ELI01807", "Unable to get concavity for this curve!");
					uclidException.addDebugInfo("Start point of curve(X)", start.getX());
					uclidException.addDebugInfo("Start point of curve(Y)", start.getY());
					uclidException.addDebugInfo("Mid point of curve(X)", mid.getX());
					uclidException.addDebugInfo("Mid point of curve(Y)", mid.getY());
					uclidException.addDebugInfo("End point of curve(X)", end.getX());
					uclidException.addDebugInfo("End point of curve(Y)", end.getY());
					throw uclidException;
				}

				// now get the delta over as well
				double dDiffDelta = 0.0;
				if (bCurveLeft)
				{
					// if curve to the left
					dDiffDelta = mid.angleTo(start) - mid.angleTo(end);
				}
				else
				{
					dDiffDelta = mid.angleTo(end) - mid.angleTo(start);
				}
				
				// if the difference is less than 0, add 2*gdPI to it
				if (dDiffDelta < 0)
				{
					dDiffDelta += 2 * gdPI;
				}
				else if (dDiffDelta > 2*gdPI)
				{
					dDiffDelta -= 2 * gdPI;
				}
				
				if (dDiffDelta >= gdPI/2)
				{
					// this curve is delta less than 180
					m_mapVars[kArcDeltaGreaterThan180Degrees]->assign(0.0);
				}
				else
				{
					// this curve is delta greater than 180
					m_mapVars[kArcDeltaGreaterThan180Degrees]->assign(1.0);
				}	
			}

			//if start, center and end point and bulginess are given, concavity can be calculated
			if (m_mapVars[kArcStartingPoint]->isAssigned()
				&& m_mapVars[kArcCenter]->isAssigned()
				&& m_mapVars[kArcEndingPoint]->isAssigned()
				&& m_mapVars[kArcDeltaGreaterThan180Degrees]->isAssigned())
			{
				double dStartX = m_mapVars[kArcStartingPoint]->getPointParamXValue(vecVRToExempt);
				double dStartY = m_mapVars[kArcStartingPoint]->getPointParamYValue(vecVRToExempt);
				double dCenterX = m_mapVars[kArcCenter]->getPointParamXValue(vecVRToExempt);
				double dCenterY = m_mapVars[kArcCenter]->getPointParamYValue(vecVRToExempt);
				double dEndX = m_mapVars[kArcEndingPoint]->getPointParamXValue(vecVRToExempt);
				double dEndY = m_mapVars[kArcEndingPoint]->getPointParamYValue(vecVRToExempt);
				Point start(dStartX, dStartY);
				Point center(dCenterX, dCenterY);
				Point end(dEndX, dEndY);

				// angle difference between center to end and center to start
				double dDiff = center.angleTo(end) - center.angleTo(start);

				// if the difference is less than 0, add 2*gdPI to it
				if (dDiff < 0)
				{
					dDiff += 2 * gdPI;
				}
				else if (dDiff > 2*gdPI)
				{
					dDiff -= 2 * gdPI;
				}

				bool bDeltaGreaterThan180 = m_mapVars[kArcDeltaGreaterThan180Degrees]->getBooleanValue(vecVRToExempt);
				bool bDiffIsGreaterThan180 = (dDiff >= gdPI) ? true : false;
				// if center to end angle subtract by center to start angle is equal to 
				// delta angle, then the curve is to the left
				if ( bDeltaGreaterThan180 == bDiffIsGreaterThan180) 
				{
					m_mapVars[kArcConcaveLeft]->assign(1);
					m_mapVars[kArcDelta]->assign((dDiff * 180.0 / gdPI));
				}
				else
				{
					m_mapVars[kArcConcaveLeft]->assign(0);
					m_mapVars[kArcDelta]->assign((2 * 180.0 - dDiff * 180.0 / gdPI));
				}
			}

		}
	}
	else if (m_mapVars[kArcConcaveLeft]->isAssigned())
	{
		// if tangent-in, radial-out form a 90° (gdPI/2) angle, curve direction is set in stone
		if ( m_mapVars[kArcTangentInBearing]->isAssigned() 
			&& m_mapVars[kArcRadialOutBearing]->isAssigned() )
		{
			vector<CurveVariableRelationship*> vecVRToExempt;
			vecVRToExempt.clear();
			double dTangentInAngle = m_mapVars[kArcTangentInBearing]->getValue(vecVRToExempt);
			double dRadianOutAngle = m_mapVars[kArcRadialOutBearing]->getValue(vecVRToExempt);
			double dDiff = (dTangentInAngle - dRadianOutAngle);
			if (dDiff < 0)
			{
				dDiff += 360;
			}
			else if (dDiff > 360.0)
			{
				dDiff -= 360;
			}

			if ( fabs(dDiff - 90.0) < 0.00000005
				&& m_mapVars[kArcConcaveLeft]->getBooleanValue(vecVRToExempt) )
			{
				// go right
				m_mapVars[kArcConcaveLeft]->assign(0);
			}
			else if ( fabs(dDiff - 270.0) < 0.00000005
				&& !m_mapVars[kArcConcaveLeft]->getBooleanValue(vecVRToExempt) )
			{
				// go left
				m_mapVars[kArcConcaveLeft]->assign(1);
			}
		}
		else if (m_mapVars[kArcTangentOutBearing]->isAssigned() 
			&& m_mapVars[kArcRadialInBearing]->isAssigned() )
		{
			vector<CurveVariableRelationship*> vecVRToExempt;
			vecVRToExempt.clear();
			double dTangentOutAngle = m_mapVars[kArcTangentOutBearing]->getValue(vecVRToExempt);
			double dRadianInAngle = m_mapVars[kArcRadialInBearing]->getValue(vecVRToExempt);
			double dDiff = dTangentOutAngle - dRadianInAngle;
			if (dDiff < 0)
			{
				dDiff += 360;
			}
			else if (dDiff > 360.0)
			{
				dDiff -= 360;
			}

			if ( fabs(dDiff - 90.0) < 0.00000005
				&& !m_mapVars[kArcConcaveLeft]->getBooleanValue(vecVRToExempt) )
			{
				// go left
				m_mapVars[kArcConcaveLeft]->assign(1);
			}
			else if ( fabs(dDiff - 270.0) < 0.00000005
				&& m_mapVars[kArcConcaveLeft]->getBooleanValue(vecVRToExempt) )
			{
				// go right
				m_mapVars[kArcConcaveLeft]->assign(0);
			}
		}
	}
}
//==========================================================================================
void CurveCalculationEngineImpl::calcDeltaOver()
{
	if (!m_mapVars[kArcDeltaGreaterThan180Degrees]->isAssigned())
	{
		vector<CurveVariableRelationship*> vecVRToExempt;
		vecVRToExempt.clear();
		
		//setting the 'pArcDeltaGreaterThan180Degrees' based on the value of pArcDelta
		if (   ( m_mapVars[kArcDelta]->isAssigned() )
			|| ( m_mapVars[kArcTangentOutBearing]->isAssigned()
				 && m_mapVars[kArcChordBearing]->isAssigned()
				 && m_mapVars[kArcConcaveLeft]->isAssigned() 
				)
			|| ( m_mapVars[kArcTangentInBearing]->isAssigned()
				 && m_mapVars[kArcChordBearing]->isAssigned()
				 && m_mapVars[kArcConcaveLeft]->isAssigned() 
				)
			)
		{
			if ((m_mapVars[kArcDelta]->getValue(vecVRToExempt))<=180)
			{
				m_mapVars[kArcDeltaGreaterThan180Degrees]->assign(0);
			}
			else
			{
				m_mapVars[kArcDeltaGreaterThan180Degrees]->assign(1);
			}
		}
		else if (m_mapVars[kArcStartingPoint]->isAssigned()
			&& m_mapVars[kArcCenter]->isAssigned()
			&& m_mapVars[kArcEndingPoint]->isAssigned()
			&& m_mapVars[kArcConcaveLeft]->isAssigned())
		{
			double dStartX = m_mapVars[kArcStartingPoint]->getPointParamXValue(vecVRToExempt);
			double dStartY = m_mapVars[kArcStartingPoint]->getPointParamYValue(vecVRToExempt);
			double dCenterX = m_mapVars[kArcCenter]->getPointParamXValue(vecVRToExempt);
			double dCenterY = m_mapVars[kArcCenter]->getPointParamYValue(vecVRToExempt);
			double dEndX = m_mapVars[kArcEndingPoint]->getPointParamXValue(vecVRToExempt);
			double dEndY = m_mapVars[kArcEndingPoint]->getPointParamYValue(vecVRToExempt);
			Point start(dStartX, dStartY);
			Point center(dCenterX, dCenterY);
			Point end(dEndX, dEndY);
			
			// angle difference between center to end and center to start
			double dDiff = center.angleTo(end) - center.angleTo(start);
			
			// if the difference is less than 0, add 2*gdPI to it
			if (dDiff < 0)
			{
				dDiff += 2 * gdPI;
			}
			else if (dDiff > 2*gdPI)
			{
				dDiff -= 2 * gdPI;
			}
			
			bool bConcaveLeft = m_mapVars[kArcConcaveLeft]->getBooleanValue(vecVRToExempt);
			bool bDiffIsGreaterThan180 = (dDiff >= gdPI) ? true : false;
			// if center to end angle subtract by center to start angle is equal to 
			// delta angle, then the curve is to the left
			if ( bConcaveLeft) 
			{
				m_mapVars[kArcDeltaGreaterThan180Degrees]->assign(bDiffIsGreaterThan180 ? 1 : 0);
				m_mapVars[kArcDelta]->assign( (dDiff * 180.0 / gdPI));
			}
			else
			{
				m_mapVars[kArcDeltaGreaterThan180Degrees]->assign(!bDiffIsGreaterThan180 ? 1 : 0);
				m_mapVars[kArcDelta]->assign((2 *180.0 - dDiff * 180.0 / gdPI));
			}
		}
	}
}
//==========================================================================================
bool CurveCalculationEngineImpl::canCalculateParameter(ECurveParameterType eCurveParameter)
{
	//value of Concavity based on the other Curve Parameters
	calcConcavity();
	//value of 'bDeltaOver180' based on ArcDelta 
	calcDeltaOver();

	vector<CurveVariableRelationship*> vecVRToExempt;
	vecVRToExempt.clear();

	switch (eCurveParameter)
	{
	case kArcDelta:
	case kArcStartAngle:
	case kArcEndAngle:
	case kArcDegreeOfCurveChordDef:
	case kArcDegreeOfCurveArcDef:
	case kArcTangentInBearing:
	case kArcTangentOutBearing:
	case kArcChordBearing:
	case kArcRadialInBearing:
	case kArcRadialOutBearing:
	case kArcRadius:
	case kArcLength:
	case kArcChordLength:
	case kArcExternalDistance:
	case kArcMiddleOrdinate:
	case kArcTangentDistance:
		return m_mapVars[eCurveParameter]->canGetValue(vecVRToExempt);
		break;
	case kArcStartingPoint:
	case kArcMidPoint:
	case kArcEndingPoint:
	case kArcCenter:
	case kArcChordMidPoint:
	case kArcExternalPoint:
		return m_mapVars[eCurveParameter]->canGetPointParamXValue(vecVRToExempt);
		break;
	}

	return m_mapVars[eCurveParameter]->isCalculatable(vecVRToExempt);
}