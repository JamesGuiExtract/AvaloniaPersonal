//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE		:	VRExternalPointToStartingPointTangentDistance.cpp
//
// PURPOSE	:	This is an implementation file for 
//				VRExternalPointToStartingPointTangentDistance class. Where 
//				the VRExternalPointToStartingPointTangentDistance class has 
//				been derived from CurveVariableRelationship class.The code 
//				written in this file makes it possible to implement functionality 
//				to add the variable to the respective variable relationship and 
//				calculate the Curve variable
//
// NOTES	:	Nothing
//
// AUTHORS	:	G Chandra Sekhar Babu
//
//==================================================================================================
// VRExternalPointToStartingPointTangentDistance.cpp : implementation file
//


#include "stdafx.h"

#include "VRExternalPointToStartingPointTangentDistance.h"
#include "CurveVariable.h"

#include <math.h>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// VRExternalPointToStartingPointMiddleOrdinate

VRExternalPointToStartingPointTangentDistance::VRExternalPointToStartingPointTangentDistance
(
 CurveVariable *pArcDelta,
 CurveVariable *pArcExternalPoint,
 CurveVariable *pArcStartingPoint, 
 CurveVariable *pArcTangentDistance,
 CurveVariable *pArcTangentInBearing
 )
 :m_pArcDelta(pArcDelta),
 m_pArcExternalPoint(pArcExternalPoint),
 m_pArcStartingPoint(pArcStartingPoint),
 m_pArcTangentDistance(pArcTangentDistance),
 m_pArcTangentInBearing(pArcTangentInBearing)
 
{
	//adding the variables to this variable relationship
	addVariable(pArcDelta);
	addVariable(pArcExternalPoint);
	addVariable(pArcStartingPoint);
	addVariable(pArcTangentDistance);
	addVariable(pArcTangentInBearing);
}
//==========================================================================================
double VRExternalPointToStartingPointTangentDistance::calculateX(CurveVariable *pVarToCalculate,
																 vector<CurveVariableRelationship*> vecVRToExempt)
{
	//Curve Point parameter 'X' value 
	double xValue=0.0;
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90152", "Memory not allocated for curve variable at calculateX() in VRExternalPointToStartingPointMiddleOrdinate");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Calculating the curve parameter with the respective equation 
	//and returning the same
	
	if(pVarToCalculate == m_pArcExternalPoint)
	{
		double dDelta = m_pArcDelta->getValue(vecVRToExempt);
		double dXStart = m_pArcStartingPoint->getPointParamXValue(vecVRToExempt);
		double dTangentDist = m_pArcTangentDistance->getValue(vecVRToExempt);
		double dTin = m_pArcTangentInBearing->getValue(vecVRToExempt);

		if (dDelta < 180.0)
		{
			xValue= dXStart + ( dTangentDist * (cos(dTin * gdPI/180.0)));
		}
		else if(dDelta > 180.0)
		{
			xValue= dXStart - ( dTangentDist * (cos(dTin * gdPI/180.0)));
		}
	}
	else if(pVarToCalculate == m_pArcStartingPoint)
	{
		double dDelta = m_pArcDelta->getValue(vecVRToExempt);
		double dXExternalPoint = m_pArcExternalPoint->getPointParamXValue(vecVRToExempt);
		double dTangentDist = m_pArcTangentDistance->getValue(vecVRToExempt);
		double dTin = m_pArcTangentInBearing->getValue(vecVRToExempt);

		if (dDelta < 180.0)
		{
			xValue= dXExternalPoint - (dTangentDist *(cos(dTin * gdPI/180.0)));
		}
		else if (dDelta > 180.0)
		{
			xValue= dXExternalPoint + (dTangentDist *(cos(dTin * gdPI/180.0)));
		}
	}
	else if(pVarToCalculate == m_pArcTangentDistance)
	{
		double dDelta = m_pArcDelta->getValue(vecVRToExempt);
		
		// if delta is multiple of 180.0°, return 0 for the tangent distance
		double dMultiple = dDelta / 180.0;
		if (dMultiple == floor(dMultiple) || dMultiple == ceil(dMultiple))
		{
			xValue = 0.0;
		}
		else
		{
			double dXStart = m_pArcStartingPoint->getPointParamXValue(vecVRToExempt);
			double dYStart = m_pArcStartingPoint->getPointParamYValue(vecVRToExempt);
			double dXExternalPoint = m_pArcExternalPoint->getPointParamXValue(vecVRToExempt);
			double dYExternalPoint = m_pArcExternalPoint->getPointParamYValue(vecVRToExempt);

			xValue = sqrt( pow((dXExternalPoint - dXStart), 2) + pow((dYExternalPoint - dYStart), 2) );
		}
	}
	else if(pVarToCalculate == m_pArcTangentInBearing)
	{
		double dDelta = m_pArcDelta->getValue(vecVRToExempt);
		double dXStart = m_pArcStartingPoint->getPointParamXValue(vecVRToExempt);
		double dYStart = m_pArcStartingPoint->getPointParamYValue(vecVRToExempt);
		double dXExternalPoint = m_pArcExternalPoint->getPointParamXValue(vecVRToExempt);
		double dYExternalPoint = m_pArcExternalPoint->getPointParamYValue(vecVRToExempt);
		double dTangentDistance = sqrt( pow((dXExternalPoint - dXStart), 2) + pow((dYExternalPoint - dYStart), 2) );

		double dCosInput=0.0;
		if( dDelta < 180.0)
		{
			dCosInput = (dXExternalPoint - dXStart) / dTangentDistance;
			checkAndSetSinCosValue(dCosInput, "ELI05629");
			
			xValue= acos(dCosInput)*(180/gdPI);
			
			// If the angle is actually belong to third or fourth quadrant
			if ( ( (dXExternalPoint < dXStart) && (dYExternalPoint < dYStart) )		// third quadrant ( between 180° and 270°)
				|| ( (dXExternalPoint > dXStart) && (dYExternalPoint < dYStart) ) )	// Or fourth quadrant ( between 270° and 360°)
			{
				xValue = 360.0 - xValue;
			}
		}
		else if ( dDelta > 180.0)
		{
			dCosInput = (dXStart - dXExternalPoint) / dTangentDistance;
			checkAndSetSinCosValue(dCosInput, "ELI05630");

			xValue= (acos(dCosInput))*(180/gdPI);
			
			// If the angle is actually belong to third or fourth quadrant
			if ( ( (dXStart < dXExternalPoint) && (dYStart < dYExternalPoint) )		// third quadrant ( between 180° and 270°)
				|| ( (dXStart > dXExternalPoint) && (dYStart < dYExternalPoint) ) )	// Or fourth quadrant ( between 270° and 360°)
			{
				xValue = 360.0 - xValue;
			}
		}
	}		
	else 
	{
		UCLIDException e("ELI90153", "This curve variable at calculateX() is not in VRExternalPointToStartingPointMiddleOrdinate variable relationship");
		throw e;
	}
	CurveVariable var;
	var.m_pszBuffer = (char*) malloc(_CVTBUFSIZE);
	int iResult =_ecvt_s( var.m_pszBuffer, _CVTBUFSIZE, xValue, 6, 
		&var.m_iDecimal, &var.m_iSign );
	if (var.m_iDecimal < -6)
	{
		xValue=0.0;
	}
	return xValue;
}
//==========================================================================================
double VRExternalPointToStartingPointTangentDistance::calculateY(CurveVariable *pVarToCalculate,
																 vector<CurveVariableRelationship*> vecVRToExempt)
{
	//Curve Point parameter 'X' value 
	double yValue=0.0;
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90154", "Memory not allocated for curve variable at calculateX() in VRExternalPointToStartingPointMiddleOrdinate");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Calculating the curve parameter with the respective equation 
	//and returning the same
	if(pVarToCalculate == m_pArcExternalPoint)
	{
		double dDelta = m_pArcDelta->getValue(vecVRToExempt);
		double dYStart = m_pArcStartingPoint->getPointParamYValue(vecVRToExempt);
		double dTangentDist = m_pArcTangentDistance->getValue(vecVRToExempt);
		double dTin = m_pArcTangentInBearing->getValue(vecVRToExempt);

		if (dDelta < 180.0)
		{
			yValue= dYStart + (dTangentDist * sin(dTin * gdPI/180.0));
		}
		else if(dDelta > 180.0)
		{
			yValue= dYStart - (dTangentDist * sin(dTin * gdPI/180.0));
		}
	}
	else if(pVarToCalculate == m_pArcStartingPoint)
	{
		double dDelta = m_pArcDelta->getValue(vecVRToExempt);
		double dYExternalPoint = m_pArcExternalPoint->getPointParamYValue(vecVRToExempt);
		double dTangentDist = m_pArcTangentDistance->getValue(vecVRToExempt);
		double dTin = m_pArcTangentInBearing->getValue(vecVRToExempt);

		if (dDelta < 180.0)
		{
			yValue= dYExternalPoint - dTangentDist*(sin(dTin * gdPI/180.0));
		}
		else if (dDelta > 180.0)
		{
			yValue= dYExternalPoint + dTangentDist*(sin(dTin * gdPI/180.0));
		}
	}
	else if(pVarToCalculate == m_pArcTangentDistance)
	{
		double dDelta = m_pArcDelta->getValue(vecVRToExempt);
		
		// if delta is multiple of 180.0°, return 0 for the tangent distance
		double dMultiple = dDelta / 180.0;
		if (dMultiple == floor(dMultiple) || dMultiple == ceil(dMultiple))
		{
			yValue = 0.0;
		}
		else
		{
			double dXStart = m_pArcStartingPoint->getPointParamXValue(vecVRToExempt);
			double dYStart = m_pArcStartingPoint->getPointParamYValue(vecVRToExempt);
			double dXExternalPoint = m_pArcExternalPoint->getPointParamXValue(vecVRToExempt);
			double dYExternalPoint = m_pArcExternalPoint->getPointParamYValue(vecVRToExempt);

			yValue = sqrt( pow((dXExternalPoint - dXStart), 2) + pow((dYExternalPoint - dYStart), 2) );
		}
	}
	else if(pVarToCalculate == m_pArcTangentInBearing)
	{
		double dDelta = m_pArcDelta->getValue(vecVRToExempt);

		double dXStart = m_pArcStartingPoint->getPointParamXValue(vecVRToExempt);
		double dYStart = m_pArcStartingPoint->getPointParamYValue(vecVRToExempt);
		double dXExternalPoint = m_pArcExternalPoint->getPointParamXValue(vecVRToExempt);
		double dYExternalPoint = m_pArcExternalPoint->getPointParamYValue(vecVRToExempt);
		double dTangentDistance = sqrt( pow((dXExternalPoint - dXStart), 2) + pow((dYExternalPoint - dYStart), 2) );

		double dSinInput=0.0;
		if (dDelta < 180.0)
		{
			dSinInput = (dYExternalPoint - dYStart) / dTangentDistance;
			checkAndSetSinCosValue(dSinInput, "ELI05620");

			yValue= asin(dSinInput)*(180/gdPI);
			
			// If the angle is actually belong to third or fourth quadrant
			if ( ( (dXExternalPoint < dXStart) && (dYExternalPoint > dYStart) )			// second quadrant ( between 90° and 180°)
				|| ( (dXExternalPoint < dXStart) && (dYExternalPoint < dYStart) ) )		// Or third quadrant ( between 180° and 270°)
			{
				yValue = 180.0 - yValue;
			}
		}
		else if (dDelta > 180)
		{
			dSinInput = (dYStart - dYExternalPoint) / dTangentDistance;
			checkAndSetSinCosValue(dSinInput, "ELI05621");

			yValue= asin(dSinInput)*(180/gdPI);
			
			// If the angle is actually belong to second or third quadrant
			if ( ( ( dXStart < dXExternalPoint ) && ( dYStart > dYExternalPoint) )			// second quadrant ( between 90° and 180°)
				|| ( (dXStart < dXExternalPoint) && (dYStart < dYExternalPoint) ) )		// Or third quadrant ( between 180° and 270°)
			{
				yValue = 180.0 - yValue;
			}
		}
	}		
	else 
	{
		UCLIDException e("ELI90155", "This curve variable at calculateY() is not in VRExternalPointToStartingPointMiddleOrdinate variable relationship");
		throw e;
	}
	CurveVariable var;
	var.m_pszBuffer = (char*) malloc(_CVTBUFSIZE);
	int iResult =_ecvt_s( var.m_pszBuffer, _CVTBUFSIZE, yValue, 6, 
		&var.m_iDecimal, &var.m_iSign );
	if (var.m_iDecimal < -6)
	{
		yValue=0.0;
	}
	return yValue;
}
//==========================================================================================
bool VRExternalPointToStartingPointTangentDistance::canCalculateX(CurveVariable *pVarToCalculate,
																  vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90156", "Memory not allocated for curve variable at calculateX() in VRExternalPointToStartingPointMiddleOrdinate");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Finding out whether the desired curve parameter can be calculable or not.
	//Checking whether all the dependent variables can be calculatable, if so 
	//return 'True' otherwise 'False'
	if(pVarToCalculate == m_pArcExternalPoint)
	{
		return (m_pArcStartingPoint->canGetPointParamXValue(vecVRToExempt)
			&&m_pArcTangentDistance->canGetValue(vecVRToExempt)
			&&m_pArcTangentInBearing->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcStartingPoint)
	{
		return (m_pArcExternalPoint->canGetPointParamXValue(vecVRToExempt)
			&&m_pArcTangentDistance->canGetValue(vecVRToExempt)
			&&m_pArcTangentInBearing->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcTangentDistance)
	{
		return (m_pArcExternalPoint->canGetPointParamXValue(vecVRToExempt)
			&&m_pArcStartingPoint->canGetPointParamXValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcTangentInBearing)
	{
		return (m_pArcExternalPoint->canGetPointParamXValue(vecVRToExempt)
			&&m_pArcStartingPoint->canGetPointParamXValue(vecVRToExempt));
	}	
	else 
	{
		UCLIDException e("ELI90157", "This curve variable at canCalculateX() is not in VRExternalPointToStartingPointMiddleOrdinate variable relationship");
		throw e;
	}
}
//==========================================================================================
bool VRExternalPointToStartingPointTangentDistance::canCalculateY(CurveVariable *pVarToCalculate,
																  vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90158", "Memory not allocated for curve variable at calculateY() in VRExternalPointToStartingPointMiddleOrdinate");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Finding out whether the desired curve parameter can be calculable or not.
	//Checking whether all the dependent variables can be calculatable, if so 
	//return 'True' otherwise 'False'
	if(pVarToCalculate == m_pArcExternalPoint)
	{
		return (m_pArcStartingPoint->canGetPointParamYValue(vecVRToExempt)
			&&m_pArcTangentDistance->canGetValue(vecVRToExempt)
			&&m_pArcTangentInBearing->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcStartingPoint)
	{
		return (m_pArcExternalPoint->canGetPointParamYValue(vecVRToExempt)
			&&m_pArcTangentDistance->canGetValue(vecVRToExempt)
			&&m_pArcTangentInBearing->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcTangentDistance)
	{
		return (m_pArcExternalPoint->canGetPointParamYValue(vecVRToExempt)
			&&m_pArcStartingPoint->canGetPointParamYValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcTangentInBearing)
	{
		return (m_pArcExternalPoint->canGetPointParamYValue(vecVRToExempt)
			&&m_pArcStartingPoint->canGetPointParamYValue(vecVRToExempt));
	}	
	else 
	{
		UCLIDException e("ELI90159", "This curve variable at canCalculateY() is not in VRExternalPointToStartingPointMiddleOrdinate variable relationship");
		throw e;
	}
}
//==========================================================================================
bool VRExternalPointToStartingPointTangentDistance::isCalculatable(CurveVariable *pVarToCalculate,
																	vector<CurveVariableRelationship*> vecVRToExempt)
{
	// if start and end points are calculatable, so are chord length and chord bearing
	if (pVarToCalculate == m_pArcTangentDistance || pVarToCalculate == m_pArcTangentInBearing)
	{
		// add this variable relationship to the list of VR's exempt from being used
		// in any calculations for any call on the stack from here down.
		vecVRToExempt.push_back(this);
		
		if(!m_pArcStartingPoint->isCalculatable(vecVRToExempt) 
			|| !m_pArcExternalPoint->isCalculatable(vecVRToExempt) )
		{
			return false;
		}
		else if (m_pArcDelta->isCalculatable(vecVRToExempt))
		{
			// if delta angle is multiple of 180°, the other parameters can't 
			// be calculated using this relationship
			double dMulti = m_pArcDelta->getValue(vecVRToExempt) / 180.0;
			if (dMulti == floor(dMulti) || dMulti == ceil(dMulti) )
			{
				return false;
			}
		}	
		return true;
	}

	return CurveVariableRelationship::isCalculatable(pVarToCalculate, vecVRToExempt);
}

