//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE		:	VRTangentDistanceToRadiusDelta.cpp
//
// PURPOSE	:	This is an implementation file for VRTangentDistanceToRadiusDelta class.
//				Where the VRTangentDistanceToRadiusDelta class has been derived from 
//				CurveVariableRelationship class.The code written in this file makes 
//				it possible to implement functionality to add the variable to the 
//				respective variable relationship and calculate the Curve variable
//
// NOTES	:	Nothing
//
// AUTHORS	:	G Chandra Sekhar Babu
//
//==================================================================================================
// VRTangentDistanceToRadiusDelta.cpp : implementation file
//


#include "stdafx.h"

#include "VRTangentDistanceToRadiusDelta.h"
#include "CurveVariable.h"

#include <math.h>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// VRTangentDistanceToRadiusDelta

VRTangentDistanceToRadiusDelta::VRTangentDistanceToRadiusDelta
								(
								CurveVariable *pArcTangentDistance,CurveVariable *pArcRadius, 
								CurveVariable *pArcDelta
								)
								:m_pArcTangentDistance(pArcTangentDistance),m_pArcRadius(pArcRadius),
								 m_pArcDelta(pArcDelta)
{
	//adding the variables to this variable relationship
	addVariable(pArcTangentDistance);
	addVariable(pArcRadius);
	addVariable(pArcDelta);
}
//==========================================================================================
double VRTangentDistanceToRadiusDelta::calculateX(CurveVariable *pVarToCalculate,
												  vector<CurveVariableRelationship*> vecVRToExempt)
{
	//Curve Point parameter value 
	double dValue=0.0;
	//ArcDelta value
	double dArcDelta=0.0;
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90192", "Memory not allocated for curve variable at calculateX() in VRTangentDistanceToRadiusDelta");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Calculating the curve parameter with the respective equation 
	//and returning the same
	if(pVarToCalculate == m_pArcTangentDistance)
	{
		dArcDelta=m_pArcDelta->getValue(vecVRToExempt);
		//if the ArcDelta is multiples of 180 degrees then the tangent distance will be zero
		if (((int)dArcDelta % 180) == 0)
		{
			return 0.0;
		}
		else 
		dValue= (m_pArcRadius->getValue(vecVRToExempt))*
			fabs(tan((m_pArcDelta->getValue(vecVRToExempt))/2*(gdPI/180.0)));
	}
	else if(pVarToCalculate == m_pArcRadius)
	{
		dValue= (m_pArcTangentDistance->getValue(vecVRToExempt))/
			fabs((tan((m_pArcDelta->getValue(vecVRToExempt))/2*(gdPI/180.0))));
	}
	else if(pVarToCalculate == m_pArcDelta)
	{
		dArcDelta= (2*atan((m_pArcTangentDistance->getValue(vecVRToExempt))/
			(m_pArcRadius->getValue(vecVRToExempt))))*(180.0/gdPI);

		CurveVariable var;
		dValue=var.checkCurveAngle(dArcDelta);
	}
	else 
	{
		UCLIDException e("ELI90193", "This curve variable at calculateX() is not in VRTangentDistanceToRadiusDelta relationship");
		throw e;
	}
	CurveVariable var;
	var.m_pszBuffer = (char*) malloc(_CVTBUFSIZE);
	int iResult =_ecvt_s( var.m_pszBuffer, _CVTBUFSIZE, dValue, 6, 
		&var.m_iDecimal, &var.m_iSign );
	if (var.m_iDecimal < -6)
	{
		dValue=0.0;
	}
	return dValue;
}
//==========================================================================================
double VRTangentDistanceToRadiusDelta::calculateY(CurveVariable *pVarToCalculate,
												  vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90194", "Memory not allocated for curve variable at calculateY() in VRTangentDistanceToRadiusDelta");
		throw e;
	}
	return 0.0;
}
//==========================================================================================
bool VRTangentDistanceToRadiusDelta::canCalculateX(CurveVariable *pVarToCalculate,
												   vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90195", "Memory not allocated for curve variable at calculateX() in VRTangentDistanceToRadiusDelta");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Finding out whether the desired curve parameter can be calculable or not.
	//Checking whether all the dependent variables can be calculatable, if so 
	//return 'True' otherwise 'False'
	if(pVarToCalculate == m_pArcTangentDistance)
	{
		return (m_pArcRadius->canGetValue(vecVRToExempt)
			&&m_pArcDelta->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcRadius)
	{
		return (m_pArcTangentDistance->canGetValue(vecVRToExempt)
			&&m_pArcDelta->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcDelta)
	{
		return (m_pArcTangentDistance->canGetValue(vecVRToExempt)
			&&m_pArcRadius->canGetValue(vecVRToExempt));
	}
	else 
	{
		UCLIDException e("ELI90196", "This curve variable at canCalculateX() is not in VRTangentDistanceToRadiusDelta variable relationship");
		throw e;
	}
}
//==========================================================================================
bool VRTangentDistanceToRadiusDelta::canCalculateY(CurveVariable *pVarToCalculate,
												   vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90197", "Memory not allocated for curve variable at calculateY() in VRTangentDistanceToRadiusDelta");
		throw e;
	}
	return false;
}
//==========================================================================================

