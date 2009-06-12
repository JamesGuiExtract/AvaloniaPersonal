//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE		:	VRCenterToEndPointRadiusRout.cpp
//
// PURPOSE	:	This is an implementation file for VRCenterToEndPointRadiusRout class.
//				Where the VRCenterToEndPointRadiusRout class has been derived from 
//				CurveVariableRelationship class.The code written in this file makes 
//				it possible to implement functionality to add the variable to the 
//				respective variable relationship and to calculate the Curve variable
//
// NOTES	:	Nothing
//
// AUTHORS	:	G Chandra Sekhar Babu
//
//==================================================================================================
// VRCenterToEndPointRadiusRout.cpp : implementation file
//

#include "stdafx.h"
#include "VRCenterToEndPointRadiusRout.h"
#include "CurveVariable.h"

#include <math.h>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// VRCenterToEndPointRadiusRout

VRCenterToEndPointRadiusRout::VRCenterToEndPointRadiusRout
							  (
							  CurveVariable *pArcCenter,
							  CurveVariable *pArcEndingPoint, CurveVariable *pArcRadius,
							  CurveVariable *pArcRadialOutBearing
							  )
							  :m_pArcCenter(pArcCenter),m_pArcEndingPoint(pArcEndingPoint),m_pArcRadius(pArcRadius),
							   m_pArcRadialOutBearing(pArcRadialOutBearing)
{
	//adding the variables to this variable relationship
	addVariable(pArcCenter);
	addVariable(pArcEndingPoint);
	addVariable(pArcRadius);
	addVariable(pArcRadialOutBearing);
}
//==========================================================================================
double VRCenterToEndPointRadiusRout::calculateX(CurveVariable *pVarToCalculate,
												vector<CurveVariableRelationship*> vecVRToExempt)
{
	//Curve Point parameter 'X' value 
	double xValue=0.0;
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90073", "Memory not allocated for curve variable at calculateX() in VRCenterToEndPointRadiusRout");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Calculating the curve parameter with the respective equation 
	//and returning the same
	if(pVarToCalculate == m_pArcCenter)
	{
		xValue= (m_pArcEndingPoint->getPointParamXValue(vecVRToExempt))-
			((m_pArcRadius->getValue(vecVRToExempt))*
			(cos(m_pArcRadialOutBearing->getValue(vecVRToExempt)*(gdPI/180.0))));
	}
	else if(pVarToCalculate == m_pArcEndingPoint)
	{
		xValue= (m_pArcCenter->getPointParamXValue(vecVRToExempt))+
			((m_pArcRadius->getValue(vecVRToExempt))*
			(cos(m_pArcRadialOutBearing->getValue(vecVRToExempt)*(gdPI/180.0))));
	}
	else if(pVarToCalculate == m_pArcRadius)
	{
		double dXCenter = m_pArcCenter->getPointParamXValue(vecVRToExempt);
		double dYCenter = m_pArcCenter->getPointParamYValue(vecVRToExempt);
		double dXEnd = m_pArcEndingPoint->getPointParamXValue(vecVRToExempt);
		double dYEnd = m_pArcEndingPoint->getPointParamYValue(vecVRToExempt);

		xValue= sqrt( pow((dXCenter - dXEnd), 2) + pow((dYCenter - dYEnd), 2) );
	}
	else if(pVarToCalculate == m_pArcRadialOutBearing)
	{
		double dXCenter = m_pArcCenter->getPointParamXValue(vecVRToExempt);
		double dYCenter = m_pArcCenter->getPointParamYValue(vecVRToExempt);
		double dXEnd = m_pArcEndingPoint->getPointParamXValue(vecVRToExempt);
		double dYEnd = m_pArcEndingPoint->getPointParamYValue(vecVRToExempt);
		double dRadius = sqrt( pow((dXCenter - dXEnd), 2) + pow((dYCenter - dYEnd), 2) );

		double dCosInput = (dXEnd - dXCenter)/dRadius;
		checkAndSetSinCosValue(dCosInput, "ELI05624");
		
		xValue= acos(dCosInput)*(180/gdPI);
		
		// since acos only gives angle in either first or second quadrant,
		// let's get the actual value for the angle according to the end 
		// and center points positions
		
		// If the angle is actually belong to third or fourth quadrant
		if ( ( (dXEnd < dXCenter) && (dYEnd < dYCenter) )			// third quadrant ( between 180° and 270°)
			|| ( (dXEnd == dXCenter) && (dYEnd < dYCenter) ) 		// == 270°
			|| ( (dXEnd > dXCenter) && (dYEnd < dYCenter) ) )		// Or fourth quadrant ( between 270° and 360°)
		{
			xValue = 360.0 - xValue;
		}
	}	
	else 
	{
		UCLIDException e("ELI90074", "This curve variable at calculateX() is not in VRCenterToEndPointRadiusRout variable relationship");
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
double VRCenterToEndPointRadiusRout::calculateY(CurveVariable *pVarToCalculate,
												vector<CurveVariableRelationship*> vecVRToExempt)
{
	//Curve Point parameter 'Y' value 
	double yValue=0.0;
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90198", "Memory not allocated for curve variable at calculateY() in VRCenterToEndPointRadiusRout");
		throw e;
	}
	
	vecVRToExempt.push_back(this);

	//Calculating the curve parameter with the respective equation 
	//and returning the same
	if(pVarToCalculate == m_pArcCenter)
	{
		yValue= (m_pArcEndingPoint->getPointParamYValue(vecVRToExempt))-
			((m_pArcRadius->getValue(vecVRToExempt))*
			(sin(m_pArcRadialOutBearing->getValue(vecVRToExempt)*(gdPI/180.0))));
	}
	else if(pVarToCalculate == m_pArcEndingPoint)
	{
		yValue= (m_pArcCenter->getPointParamYValue(vecVRToExempt))+
			((m_pArcRadius->getValue(vecVRToExempt))*
			(sin(m_pArcRadialOutBearing->getValue(vecVRToExempt)*(gdPI/180.0))));
	}
	else if(pVarToCalculate == m_pArcRadius)
	{
		double dXCenter = m_pArcCenter->getPointParamXValue(vecVRToExempt);
		double dYCenter = m_pArcCenter->getPointParamYValue(vecVRToExempt);
		double dXEnd = m_pArcEndingPoint->getPointParamXValue(vecVRToExempt);
		double dYEnd = m_pArcEndingPoint->getPointParamYValue(vecVRToExempt);

		yValue= sqrt( pow((dXCenter - dXEnd), 2) + pow((dYCenter - dYEnd), 2) );
	}
	else if(pVarToCalculate == m_pArcRadialOutBearing)
	{
		double dXCenter = m_pArcCenter->getPointParamXValue(vecVRToExempt);
		double dYCenter = m_pArcCenter->getPointParamYValue(vecVRToExempt);
		double dXEnd = m_pArcEndingPoint->getPointParamXValue(vecVRToExempt);
		double dYEnd = m_pArcEndingPoint->getPointParamYValue(vecVRToExempt);
		double dRadius = sqrt( pow((dXCenter - dXEnd), 2) + pow((dYCenter - dYEnd), 2) );

		double dSinInput=0.0;
		dSinInput=(dYEnd - dYCenter)/dRadius;
		//Invalid input to the sin inverse function 
		checkAndSetSinCosValue(dSinInput, "ELI05615");
		
		yValue= (asin(dSinInput))*(180/gdPI);
		
		// since asin only gives angle in either first or fourth quadrant,
		// let's get the actual value for the angle according to the end 
		// and center points positions
		
		// If the angle is actually belong to second or third quadrant
		if ( ( (dXEnd < dXCenter) && (dYEnd > dYCenter) )			// second quadrant ( between 90° and 180°)
			|| ( (dXEnd < dXCenter) && (dYEnd == dYCenter) )		// should be 180°
			|| ( (dXEnd < dXCenter) && (dYEnd < dYCenter) ) )		// Or third quadrant ( between 180° and 270°)
		{
			yValue = 180.0 - yValue;
		}
	}	
	else 
	{
		UCLIDException e("ELI90075", "This curve variable at calculateY() is not in VRCenterToEndPointRadiusRout variable relationship");
		throw e;
	}
	CurveVariable var;
	var.m_pszBuffer = (char*) malloc(_CVTBUFSIZE);
	int iResult = _ecvt_s( var.m_pszBuffer, _CVTBUFSIZE, yValue, 6, 
		&var.m_iDecimal, &var.m_iSign );
	if (var.m_iDecimal < -6)
	{
		yValue=0.0;
	}
	return yValue;
}
//==========================================================================================
bool VRCenterToEndPointRadiusRout::canCalculateX(CurveVariable *pVarToCalculate,
												 vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90076", "Memory not allocated for curve variable at calculateX() in VRCenterToEndPointRadiusRout");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Finding out whether the desired curve parameter can be calculable or not.
	//Checking whether all the dependent variables can be calculatable, if so 
	//return 'True' otherwise 'False'
	if(pVarToCalculate == m_pArcCenter)
	{
		return (m_pArcEndingPoint->canGetPointParamXValue(vecVRToExempt)
			&&m_pArcRadius->canGetValue(vecVRToExempt)
			&&m_pArcRadialOutBearing->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcEndingPoint)
	{
		return (m_pArcCenter->canGetPointParamXValue(vecVRToExempt)
			&&m_pArcRadius->canGetValue(vecVRToExempt)
			&&m_pArcRadialOutBearing->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcRadius)
	{
		return (m_pArcCenter->canGetPointParamXValue(vecVRToExempt)
			&&m_pArcEndingPoint->canGetPointParamXValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcRadialOutBearing)
	{
		return (m_pArcCenter->canGetPointParamXValue(vecVRToExempt)
			&&m_pArcEndingPoint->canGetPointParamXValue(vecVRToExempt));
	}	
	else 
	{
		UCLIDException e("ELI90077", "This curve variable at calculateX() is not in VRCenterToEndPointRadiusRout variable relationship");
		throw e;
	}
}
//==========================================================================================
bool VRCenterToEndPointRadiusRout::canCalculateY(CurveVariable *pVarToCalculate,
												 vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90078", "Memory not allocated for curve variable at calculateY() in VRCenterToEndPointRadiusRout");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Finding out whether the desired curve parameter can be calculable or not.
	//Checking whether all the dependent variables can be calculatable, if so 
	//return 'True' otherwise 'False'
	if(pVarToCalculate == m_pArcCenter)
	{
		return (m_pArcEndingPoint->canGetPointParamYValue(vecVRToExempt)
			&&m_pArcRadius->canGetValue(vecVRToExempt)
			&&m_pArcRadialOutBearing->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcEndingPoint)
	{
		return (m_pArcCenter->canGetPointParamYValue(vecVRToExempt)
			&&m_pArcRadius->canGetValue(vecVRToExempt)
			&&m_pArcRadialOutBearing->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcRadius)
	{
		return (m_pArcCenter->canGetPointParamYValue(vecVRToExempt)
			&&m_pArcEndingPoint->canGetPointParamYValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcRadialOutBearing)
	{
		return (m_pArcCenter->canGetPointParamYValue(vecVRToExempt)
			&&m_pArcEndingPoint->canGetPointParamYValue(vecVRToExempt));
	}	
	else 
	{
		UCLIDException e("ELI90079", "This curve variable at calculateY() is not in VRCenterToEndPointRadiusRout variable relationship");
		throw e;
	}
}
//==========================================================================================
bool VRCenterToEndPointRadiusRout::isCalculatable(CurveVariable *pVarToCalculate,
												  vector<CurveVariableRelationship*> vecVRToExempt)
{

	// if center and end points are calculatable, so are chord length and chord bearing
	if (pVarToCalculate == m_pArcRadius || pVarToCalculate == m_pArcRadialOutBearing)
	{
		// add this variable relationship to the list of VR's exempt from being used
		// in any calculations for any call on the stack from here down.
		vecVRToExempt.push_back(this);
		
		if( !m_pArcCenter->isCalculatable(vecVRToExempt) 
			|| !m_pArcEndingPoint->isCalculatable(vecVRToExempt) )
		{
			return false;
		}

		return true;
	}

	return CurveVariableRelationship::isCalculatable(pVarToCalculate, vecVRToExempt);
}
