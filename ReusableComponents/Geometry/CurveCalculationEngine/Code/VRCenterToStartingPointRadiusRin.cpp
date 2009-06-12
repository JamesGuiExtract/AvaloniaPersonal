//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE		:	VRCenterToStartingPointRadiusRin.cpp
//
// PURPOSE	:	This is an implementation file for VRCenterToStartingPointRadiusRin class.
//				Where the VRCenterToStartingPointRadiusRin class has been derived from 
//				CurveVariableRelationship class.The code written in this file makes 
//				it possible to implement functionality to add the variable to the 
//				respective variable relationship and to calculate the Curve variable
//
// NOTES	:	Nothing
//
// AUTHORS	:	G Chandra Sekhar Babu
//
//==================================================================================================
// VRCenterToStartingPointRadiusRin.cpp : implementation file
//


#include "stdafx.h"
#include "VRCenterToStartingPointRadiusRin.h"
#include "CurveVariable.h"


#include <math.h>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// VRCenterToStartingPointRadiusRin

VRCenterToStartingPointRadiusRin::VRCenterToStartingPointRadiusRin
								  (
								  CurveVariable *pArcCenter,CurveVariable *pArcStartingPoint,
								  CurveVariable *pArcRadius,CurveVariable *pArcRadialInBearing
								  )
								  :m_pArcCenter(pArcCenter),m_pArcStartingPoint(pArcStartingPoint),
								   m_pArcRadius(pArcRadius),m_pArcRadialInBearing(pArcRadialInBearing)
{
	//adding the variables to this variable relationship
	addVariable(pArcCenter);
	addVariable(pArcStartingPoint);
	addVariable(pArcRadius);
	addVariable(pArcRadialInBearing);
}
//==========================================================================================
double VRCenterToStartingPointRadiusRin::calculateX(CurveVariable *pVarToCalculate,
													vector<CurveVariableRelationship*> vecVRToExempt)
{
	//Curve Point parameter 'X' value 
	double xValue=0.0;
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90080", "Memory not allocated for curve variable at calculateX() in VRCenterToStartingPointRadiusRin");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Calculating the curve parameter with the respective equation 
	//and returning the same
	if(pVarToCalculate == m_pArcCenter)
	{
		xValue=(m_pArcStartingPoint->getPointParamXValue(vecVRToExempt))+
			((m_pArcRadius->getValue(vecVRToExempt))*
			(cos(m_pArcRadialInBearing->getValue(vecVRToExempt)*(gdPI/180.0))));
	}
	else if(pVarToCalculate == m_pArcStartingPoint)
	{
		xValue= (m_pArcCenter->getPointParamXValue(vecVRToExempt))-
			((m_pArcRadius->getValue(vecVRToExempt))*
			(cos(m_pArcRadialInBearing->getValue(vecVRToExempt)*(gdPI/180.0))));
	}
	else if(pVarToCalculate == m_pArcRadius)
	{
		double dXCenter = m_pArcCenter->getPointParamXValue(vecVRToExempt);
		double dYCenter = m_pArcCenter->getPointParamYValue(vecVRToExempt);
		double dXStart = m_pArcStartingPoint->getPointParamXValue(vecVRToExempt);
		double dYStart = m_pArcStartingPoint->getPointParamYValue(vecVRToExempt);

		xValue= sqrt( pow((dXCenter - dXStart), 2) + pow((dYCenter - dYStart), 2) );
	}
	else if(pVarToCalculate == m_pArcRadialInBearing)
	{
		double dXCenter = m_pArcCenter->getPointParamXValue(vecVRToExempt);
		double dYCenter = m_pArcCenter->getPointParamYValue(vecVRToExempt);
		double dXStart = m_pArcStartingPoint->getPointParamXValue(vecVRToExempt);
		double dYStart = m_pArcStartingPoint->getPointParamYValue(vecVRToExempt);
		double dRadius = sqrt( pow((dXCenter - dXStart), 2) + pow((dYCenter - dYStart), 2) );

		double dCosValue = (dXCenter - dXStart)/dRadius;
		checkAndSetSinCosValue(dCosValue, "ELI05625");

		// get the angle in degrees
		xValue = acos(dCosValue) * (180/gdPI);

		// since acos only gives angle in either first or second quadrant,
		// let's get the actual value for the angle according to the start 
		// and center points positions

		// If the angle is actually belong to third or fourth quadrant
		if ( ( (dXCenter < dXStart) && (dYCenter < dYStart) )			// third quadrant ( between 180° and 270°)
			|| ( (dXCenter == dXStart) && (dYCenter < dYStart) ) 		// == 270°
			|| ( (dXCenter > dXStart) && (dYCenter < dYStart) )	)	// Or fourth quadrant ( between 270° and 360°)
		{
			xValue = 360.0 - xValue;
		}
	}	
	else 
	{
		UCLIDException e("ELI90081", "This curve variable at calculateX() is not in VRCenterToStartingPointRadiusRin variable relationship");
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
double VRCenterToStartingPointRadiusRin::calculateY(CurveVariable *pVarToCalculate,
													vector<CurveVariableRelationship*> vecVRToExempt)
{
	//Curve Point parameter 'Y' value 
	double yValue=0.0;
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90082", "Memory not allocated for curve variable at calculateY() in VRCenterToStartingPointRadiusRin");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Calculating the curve parameter with the respective equation 
	//and returning the same
	if(pVarToCalculate == m_pArcCenter)
	{
		yValue=(m_pArcStartingPoint->getPointParamYValue(vecVRToExempt))+
			((m_pArcRadius->getValue(vecVRToExempt))*
			(sin(m_pArcRadialInBearing->getValue(vecVRToExempt)*(gdPI/180.0))));
	}
	else if(pVarToCalculate == m_pArcStartingPoint)
	{
		yValue= (m_pArcCenter->getPointParamYValue(vecVRToExempt))-
			((m_pArcRadius->getValue(vecVRToExempt))*
			(sin(m_pArcRadialInBearing->getValue(vecVRToExempt)*(gdPI/180.0))));
	}
	else if(pVarToCalculate == m_pArcRadius)
	{
		double dXCenter = m_pArcCenter->getPointParamXValue(vecVRToExempt);
		double dYCenter = m_pArcCenter->getPointParamYValue(vecVRToExempt);
		double dXStart = m_pArcStartingPoint->getPointParamXValue(vecVRToExempt);
		double dYStart = m_pArcStartingPoint->getPointParamYValue(vecVRToExempt);
		
		yValue = sqrt( pow((dXCenter - dXStart), 2) + pow((dYCenter - dYStart), 2) );
	}
	else if(pVarToCalculate == m_pArcRadialInBearing)
	{
		double dXCenter = m_pArcCenter->getPointParamXValue(vecVRToExempt);
		double dYCenter = m_pArcCenter->getPointParamYValue(vecVRToExempt);
		double dXStart = m_pArcStartingPoint->getPointParamXValue(vecVRToExempt);
		double dYStart = m_pArcStartingPoint->getPointParamYValue(vecVRToExempt);
		double dRadius = sqrt( pow((dXCenter - dXStart), 2) + pow((dYCenter - dYStart), 2) );

		double dSinValue = (dYCenter - dYStart)/dRadius;
		checkAndSetSinCosValue(dSinValue, "ELI05616");

		yValue= asin(dSinValue)*(180/gdPI);

		// since asin only gives angle in either first or fourth quadrant,
		// let's get the actual value for the angle according to the start 
		// and center points positions

		// If the angle is actually belong to second or third quadrant
		if ( ( (dXCenter < dXStart) && (dYCenter > dYStart) )			// second quadrant ( between 90° and 180°)
			|| ( (dXCenter < dXStart) && (dYCenter == dYStart) )		// should be 180°
			|| ( (dXCenter < dXStart) && (dYCenter < dYStart) ) )		// Or third quadrant ( between 180° and 270°)
		{
			yValue = 180.0 - yValue;
		}
	}	
	else 
	{
		UCLIDException e("ELI90083", "This curve variable at calculateY() is not in VRCenterToStartingPointRadiusRin variable relationship");
		throw e;
	}
	CurveVariable var;
	var.m_pszBuffer = (char*) malloc(_CVTBUFSIZE);
	int a = sizeof(var.m_pszBuffer);
	int iResult = _ecvt_s( var.m_pszBuffer, _CVTBUFSIZE, yValue, 6, 
		&var.m_iDecimal, &var.m_iSign );
	if (var.m_iDecimal < -6)
	{
		yValue=0.0;
	}
	return yValue;
}
//==========================================================================================
bool VRCenterToStartingPointRadiusRin::canCalculateX(CurveVariable *pVarToCalculate,
													 vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90084", "Memory not allocated for curve variable at calculateX() in VRCenterToStartingPointRadiusRin");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Finding out whether the desired curve parameter can be calculable or not.
	//Checking whether all the dependent variables can be calculatable, if so 
	//return 'True' otherwise 'False'
	if(pVarToCalculate == m_pArcCenter)
	{
		return (m_pArcStartingPoint->canGetPointParamXValue(vecVRToExempt)
			&&m_pArcRadius->canGetValue(vecVRToExempt)
			&&m_pArcRadialInBearing->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcStartingPoint)
	{
		return (m_pArcCenter->canGetPointParamXValue(vecVRToExempt)
			&&m_pArcRadius->canGetValue(vecVRToExempt)
			&&m_pArcRadialInBearing->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcRadius)
	{
		return (m_pArcCenter->canGetPointParamXValue(vecVRToExempt)
			&&m_pArcStartingPoint->canGetPointParamXValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcRadialInBearing)
	{
		return (m_pArcCenter->canGetPointParamXValue(vecVRToExempt)
			&&m_pArcStartingPoint->canGetPointParamXValue(vecVRToExempt));
	}	
	else 
	{
		UCLIDException e("ELI90085", "This curve variable at calculateX() is not in VRCenterToStartingPointRadiusRin variable relationship");
		throw e;
	}
}
//==========================================================================================
bool VRCenterToStartingPointRadiusRin::canCalculateY(CurveVariable *pVarToCalculate,
													 vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90086", "Memory not allocated for curve variable at calculateY() in VRCenterToStartingPointRadiusRin");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Finding out whether the desired curve parameter can be calculable or not.
	//Checking whether all the dependent variables can be calculatable, if so 
	//return 'True' otherwise 'False'
	if(pVarToCalculate == m_pArcCenter)
	{
		return (m_pArcStartingPoint->canGetPointParamYValue(vecVRToExempt)
			&&m_pArcRadius->canGetValue(vecVRToExempt)
			&&m_pArcRadialInBearing->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcStartingPoint)
	{
		return (m_pArcCenter->canGetPointParamYValue(vecVRToExempt)
			&&m_pArcRadius->canGetValue(vecVRToExempt)
			&&m_pArcRadialInBearing->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcRadius)
	{
		return (m_pArcCenter->canGetPointParamYValue(vecVRToExempt)
			&&m_pArcStartingPoint->canGetPointParamYValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcRadialInBearing)
	{
		return (m_pArcCenter->canGetPointParamYValue(vecVRToExempt)
			&&m_pArcStartingPoint->canGetPointParamYValue(vecVRToExempt));
	}	
	else 
	{
		UCLIDException e("ELI90087", "This curve variable at calculateY() is not in VRCenterToStartingPointRadiusRin variable relationship");
		throw e;
	}
}
//==========================================================================================
bool VRCenterToStartingPointRadiusRin::isCalculatable(CurveVariable *pVarToCalculate,
													  vector<CurveVariableRelationship*> vecVRToExempt)
{
	// if start and center points are calculatable, so are chord length and chord bearing
	if (pVarToCalculate == m_pArcRadius || pVarToCalculate == m_pArcRadialInBearing)
	{
		// add this variable relationship to the list of VR's exempt from being used
		// in any calculations for any call on the stack from here down.
		vecVRToExempt.push_back(this);
		
		if(!m_pArcCenter->isCalculatable(vecVRToExempt) 
			|| !m_pArcStartingPoint->isCalculatable(vecVRToExempt) )
		{
			return false;
		}
		
		return true;
	}

	return CurveVariableRelationship::isCalculatable(pVarToCalculate, vecVRToExempt);
}
