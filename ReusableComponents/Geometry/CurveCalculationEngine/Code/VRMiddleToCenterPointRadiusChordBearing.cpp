//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE		:	VRMiddleToCenterPointRadiusChordBearing.cpp
//
// PURPOSE	:	This is an implementation file for 
//				VRMiddleToCenterPointRadiusChordBearing class.Where the 
//				VRMiddleToCenterPointRadiusChordBearing class has been derived 
//				from CurveVariableRelationship class.The code written in this 
//				file makes it possible to implement functionality to add the 
//				variable to the respective variable relationship and calculate 
//				the Curve variable
//
// NOTES	:	Nothing
//
// AUTHORS	:	G Chandra Sekhar Babu
//
//==================================================================================================
// VRMiddleToCenterPointRadiusChordBearing.cpp : implementation file
//


#include "stdafx.h"

#include "VRMiddleToCenterPointRadiusChordBearing.h"
#include "CurveVariable.h"

#include <math.h>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// VRMiddleToCenterPointRadiusChordBearing

VRMiddleToCenterPointRadiusChordBearing::VRMiddleToCenterPointRadiusChordBearing
										(
										 CurveVariable *pArcConcaveLeft,CurveVariable *pArcMidPoint,
										 CurveVariable *pArcCenterPoint, CurveVariable *pArcRadius,
										 CurveVariable *pArcChordBearing
										)
									   :m_pArcConcaveLeft(pArcConcaveLeft),m_pArcMidPoint(pArcMidPoint),
										m_pArcCenterPoint(pArcCenterPoint),m_pArcRadius(pArcRadius),
										m_pArcChordBearing(pArcChordBearing)
{
	//adding the variables to this variable relationship
	addVariable(pArcConcaveLeft);		
	addVariable(pArcMidPoint);
	addVariable(pArcCenterPoint);
	addVariable(pArcRadius);
	addVariable(pArcChordBearing);
}
//==========================================================================================
double VRMiddleToCenterPointRadiusChordBearing::calculateX(CurveVariable *pVarToCalculate,
														   vector<CurveVariableRelationship*> vecVRToExempt)
{
	//Curve Point parameter 'X' value 
	double xValue=0.0;
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90166", "Memory not allocated for curve variable at calculateX() in VRMiddleToCenterPointRadiusChordBearing");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Calculating the curve parameter with the respective equation 
	//and returning the same
	if(pVarToCalculate == m_pArcMidPoint)
	{
		bool bLeft = m_pArcConcaveLeft->getBooleanValue(vecVRToExempt);
		double dRadius = m_pArcRadius->getValue(vecVRToExempt);
		// chord bearing in degrees
		double dChordBearing = m_pArcChordBearing->getValue(vecVRToExempt);
		double dXCenter = m_pArcCenterPoint->getPointParamXValue(vecVRToExempt);

		if (bLeft)
		{
			xValue = dXCenter + dRadius * sin(dChordBearing * gdPI/180.0);
		}
		else
		{
			xValue = dXCenter - dRadius * sin(dChordBearing * gdPI/180.0);
		}
	}
	else if(pVarToCalculate == m_pArcCenterPoint)
	{
		bool bLeft = m_pArcConcaveLeft->getBooleanValue(vecVRToExempt);
		double dRadius = m_pArcRadius->getValue(vecVRToExempt);
		double dXMidOfArc = m_pArcMidPoint->getPointParamXValue(vecVRToExempt);
		// chord bearing in degrees
		double dChordBearing = m_pArcChordBearing->getValue(vecVRToExempt);
		
		if (bLeft)
		{
			xValue = dXMidOfArc - dRadius * sin(dChordBearing * gdPI/180.0);
		}
		else
		{
			xValue = dXMidOfArc + dRadius * sin(dChordBearing * gdPI/180.0);
		}
	}
	else if(pVarToCalculate == m_pArcRadius)
	{
		double dXMidOfArc = m_pArcMidPoint->getPointParamXValue(vecVRToExempt);
		double dYMidOfArc = m_pArcMidPoint->getPointParamYValue(vecVRToExempt);
		double dXCenter = m_pArcCenterPoint->getPointParamXValue(vecVRToExempt);
		double dYCenter = m_pArcCenterPoint->getPointParamYValue(vecVRToExempt);

		xValue = sqrt( pow((dXMidOfArc - dXCenter), 2) + pow((dYMidOfArc - dYCenter), 2) );
	}
	else if(pVarToCalculate == m_pArcChordBearing)
	{
		bool bLeft = m_pArcConcaveLeft->getBooleanValue(vecVRToExempt);
		double dXMidOfArc = m_pArcMidPoint->getPointParamXValue(vecVRToExempt);
		double dYMidOfArc = m_pArcMidPoint->getPointParamYValue(vecVRToExempt);
		double dXCenter = m_pArcCenterPoint->getPointParamXValue(vecVRToExempt);
		double dYCenter = m_pArcCenterPoint->getPointParamYValue(vecVRToExempt);
		double dRadius = sqrt( pow((dXMidOfArc - dXCenter), 2) + pow((dYMidOfArc - dYCenter), 2) );

		double dSinInput=0.0;
		if (bLeft)
		{
			dSinInput = (dXMidOfArc - dXCenter) / dRadius;
		}
		else
		{
			dSinInput = (dXCenter - dXMidOfArc) / dRadius;
		}

		checkAndSetSinCosValue(dSinInput, "ELI05622");

		// get the angle in degrees
		xValue= asin(dSinInput) * 180/gdPI;
		
		// If the angle is actually belong to second or third quadrant
		if ( ( (dXMidOfArc < dXCenter) && (dYMidOfArc > dYCenter))			// second quadrant ( between 90° and 180°)
			|| ( (dXMidOfArc < dXCenter) && (dYMidOfArc == dYCenter) )		// should be 180°
			|| ( (dXMidOfArc < dXCenter) && (dYMidOfArc < dYCenter) )	)	// Or third quadrant ( between 180° and 270°)
		{
			xValue = 180.0 - xValue;
		}
	}	
	else 
	{
		UCLIDException e("ELI90167", "This curve variable at calculateX() is not in VRMiddleToCenterPointRadiusChordBearing variable relationship");
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
double VRMiddleToCenterPointRadiusChordBearing::calculateY(CurveVariable *pVarToCalculate,
														   vector<CurveVariableRelationship*> vecVRToExempt)
{
	//Curve Point parameter 'Y' value 
	double yValue=0.0;
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90168", "Memory not allocated for curve variable at calculateY() in VRMiddleToCenterPointRadiusChordBearing");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Calculating the curve parameter with the respective equation 
	//and returning the same
	if(pVarToCalculate == m_pArcMidPoint)
	{
		bool bLeft = m_pArcConcaveLeft->getBooleanValue(vecVRToExempt);
		double dRadius = m_pArcRadius->getValue(vecVRToExempt);
		// chord bearing in degrees
		double dChordBearing = m_pArcChordBearing->getValue(vecVRToExempt);
		double dYCenter = m_pArcCenterPoint->getPointParamYValue(vecVRToExempt);

		if (bLeft)
		{
			yValue= dYCenter - dRadius * cos(dChordBearing * gdPI/180.0);
		}
		else
		{
			yValue= dYCenter + dRadius * cos(dChordBearing * gdPI/180.0);
		}
	}
	else if(pVarToCalculate == m_pArcCenterPoint)
	{
		bool bLeft = m_pArcConcaveLeft->getBooleanValue(vecVRToExempt);
		double dRadius = m_pArcRadius->getValue(vecVRToExempt);
		double dYMidOfArc = m_pArcMidPoint->getPointParamYValue(vecVRToExempt);
		// chord bearing in degrees
		double dChordBearing = m_pArcChordBearing->getValue(vecVRToExempt);

		if (bLeft)
		{
			yValue= dYMidOfArc + dRadius * cos(dChordBearing * gdPI/180.0);
		}
		else
		{
			yValue= dYMidOfArc - dRadius * cos(dChordBearing * gdPI/180.0);
		}
	}
	else if(pVarToCalculate == m_pArcRadius)
	{
		double dXMidOfArc = m_pArcMidPoint->getPointParamXValue(vecVRToExempt);
		double dYMidOfArc = m_pArcMidPoint->getPointParamYValue(vecVRToExempt);
		double dXCenter = m_pArcCenterPoint->getPointParamXValue(vecVRToExempt);
		double dYCenter = m_pArcCenterPoint->getPointParamYValue(vecVRToExempt);

		yValue = sqrt( pow((dXMidOfArc - dXCenter), 2) + pow((dYMidOfArc - dYCenter), 2) );
	}
	else if(pVarToCalculate == m_pArcChordBearing)
	{
		bool bLeft = m_pArcConcaveLeft->getBooleanValue(vecVRToExempt);
		double dXMidOfArc = m_pArcMidPoint->getPointParamXValue(vecVRToExempt);
		double dYMidOfArc = m_pArcMidPoint->getPointParamYValue(vecVRToExempt);
		double dXCenter = m_pArcCenterPoint->getPointParamXValue(vecVRToExempt);
		double dYCenter = m_pArcCenterPoint->getPointParamYValue(vecVRToExempt);
		double dRadius = sqrt( pow((dXMidOfArc - dXCenter), 2) + pow((dYMidOfArc - dYCenter), 2) );

		double dCosInput=0.0;
		// get the angle in degrees first
		if (bLeft)
		{
			dCosInput = (dYCenter - dYMidOfArc) / dRadius;
		}
		else
		{
			dCosInput = (dYMidOfArc - dYCenter) / dRadius;
		}

		checkAndSetSinCosValue(dCosInput, "ELI05632");
		
		yValue= acos(dCosInput)*(180/gdPI);
		
		// since acos only gives angle in either first or second quadrant,
		// let's get the actual value for the angle according to the mid of curve 
		// and center points positions
		
		// If the angle is actually belong to third or fourth quadrant
		if ( ( (dXMidOfArc < dXCenter) && (dYMidOfArc < dYCenter) )			// third quadrant ( between 180° and 270°)
			|| ( (dXMidOfArc == dXCenter) && (dYMidOfArc < dYCenter) ) 		// == 270°
			|| ( (dXMidOfArc > dXCenter) && (dYMidOfArc < dYCenter) )	)	// Or fourth quadrant ( between 270° and 360°)
		{
			yValue = 360.0 - yValue;
		}
	}	
	else 
	{
		UCLIDException e("ELI90169", "This curve variable at calculateY() is not in VRMiddleToCenterPointRadiusChordBearing variable relationship");
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
bool VRMiddleToCenterPointRadiusChordBearing::canCalculateX(CurveVariable *pVarToCalculate,
															vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90170", "Memory not allocated for curve variable at calculateX() in VRMiddleToCenterPointRadiusChordBearing");
		throw e;
	}

	vecVRToExempt.push_back(this);

	if(pVarToCalculate == m_pArcMidPoint)
	{
		return (m_pArcCenterPoint->canGetPointParamXValue(vecVRToExempt)
			&&m_pArcRadius->canGetValue(vecVRToExempt)&&
			m_pArcChordBearing->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcCenterPoint)
	{
		return (m_pArcMidPoint->canGetPointParamXValue(vecVRToExempt)
			&&m_pArcRadius->canGetValue(vecVRToExempt)
			&&m_pArcChordBearing->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcRadius)
	{
		return (m_pArcCenterPoint->canGetPointParamXValue(vecVRToExempt)
			&&m_pArcMidPoint->canGetPointParamXValue(vecVRToExempt)
			&&m_pArcChordBearing->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcChordBearing)
	{
		return (m_pArcCenterPoint->canGetPointParamXValue(vecVRToExempt)
			&&m_pArcMidPoint->canGetPointParamXValue(vecVRToExempt)
			&&m_pArcMidPoint->canGetPointParamXValue(vecVRToExempt));
	}	
	else 
	{
		UCLIDException e("ELI90171", "This curve variable at canCalculateX() is not in VRMiddleToCenterPointRadiusChordBearing variable relationship");
		throw e;
	}
}
//==========================================================================================
bool VRMiddleToCenterPointRadiusChordBearing::canCalculateY(CurveVariable *pVarToCalculate,
															vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90172", "Memory not allocated for curve variable at calculateY() in VRMiddleToCenterPointRadiusChordBearing");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Finding out whether the desired curve parameter can be calculable or not.
	//Checking whether all the dependent variables can be calculatable, if so 
	//return 'True' otherwise 'False'
	if(pVarToCalculate == m_pArcMidPoint)
	{
		return (m_pArcCenterPoint->canGetPointParamYValue(vecVRToExempt)
			&&m_pArcRadius->canGetValue(vecVRToExempt)
			&&m_pArcChordBearing->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcCenterPoint)
	{
		return (m_pArcMidPoint->canGetPointParamYValue(vecVRToExempt)
			&&m_pArcRadius->canGetValue(vecVRToExempt)
			&&m_pArcChordBearing->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcRadius)
	{
		return (m_pArcCenterPoint->canGetPointParamYValue(vecVRToExempt)
			&&m_pArcMidPoint->canGetPointParamYValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcChordBearing)
	{
		return (m_pArcCenterPoint->canGetPointParamYValue(vecVRToExempt)
			&&m_pArcMidPoint->canGetPointParamYValue(vecVRToExempt));
	}	
	else 
	{
		UCLIDException e("ELI90173", "This curve variable at canCalculateX() is not in VRMiddleToCenterPointRadiusChordBearing variable relationship");
		throw e;
	}
}
//==========================================================================================
bool VRMiddleToCenterPointRadiusChordBearing::isCalculatable(CurveVariable *pVarToCalculate,
															 vector<CurveVariableRelationship*> vecVRToExempt)
{
	// if start and end points are calculatable, so are chord length and chord bearing
	if (pVarToCalculate == m_pArcRadius || pVarToCalculate == m_pArcChordBearing)
	{
		// add this variable relationship to the list of VR's exempt from being used
		// in any calculations for any call on the stack from here down.
		vecVRToExempt.push_back(this);
		
		if(!m_pArcCenterPoint->isCalculatable(vecVRToExempt) 
			|| !m_pArcMidPoint->isCalculatable(vecVRToExempt) )
		{
			return false;
		}
		
		return true;
	}

	return CurveVariableRelationship::isCalculatable(pVarToCalculate, vecVRToExempt);
}

