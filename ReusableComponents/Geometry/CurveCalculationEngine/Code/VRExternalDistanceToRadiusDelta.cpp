//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE		:	VRExternalDistanceToRadiusDelta.cpp
//
// PURPOSE	:	This is an implementation file for VRExternalDistanceToRadiusDelta class.
//				Where the VRExternalDistanceToRadiusDelta class has been derived from 
//				CurveVariableRelationship class.The code written in this file makes 
//				it possible to implement functionality to add the variable to the 
//				respective variable relationship and to calculate the Curve variable
//
// NOTES	:	Nothing
//
// AUTHORS	:	G Chandra Sekhar Babu
//
//==================================================================================================
// VRExternalDistanceToRadiusDelta.cpp : implementation file
//


#include "stdafx.h"

#include "VRExternalDistanceToRadiusDelta.h"
#include "CurveVariable.h"

#include <math.h>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// VRExternalDistanceToRadiusDelta

VRExternalDistanceToRadiusDelta::VRExternalDistanceToRadiusDelta
								 (
								 CurveVariable *pArcExternalDistance,CurveVariable *pArcRadius,
								 CurveVariable *pArcDelta
								 )
								 :m_pArcExternalDistance(pArcExternalDistance),m_pArcRadius(pArcRadius),
								  m_pArcDelta(pArcDelta)
{
	//adding the variables to this variable relationship
	addVariable(pArcExternalDistance);
	addVariable(pArcRadius);
	addVariable(pArcDelta);
}
//==========================================================================================
double VRExternalDistanceToRadiusDelta::calculateX(CurveVariable *pVarToCalculate,
												   vector<CurveVariableRelationship*> vecVRToExempt)
{
	//Curve Point parameter value 
	double dValue=0.0;
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90146", "Memory not allocated for curve variable at calculateX() in VRExternalDistanceToRadiusDelta");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Calculating the curve parameter with the respective equation 
	//and returning the same
	if(pVarToCalculate == m_pArcExternalDistance)
	{
		double dArcDelta=0.0;
		dArcDelta=m_pArcDelta->getValue(vecVRToExempt);
		if (((int)dArcDelta % 180) == 0)
		{
			return 0.0;
		}
		else 
		dValue= fabs((m_pArcRadius->getValue(vecVRToExempt))*
			((1/(cos((m_pArcDelta->getValue(vecVRToExempt))/2*(gdPI/180.0))))-1));
	}
	else if(pVarToCalculate == m_pArcRadius)
	{
		dValue= fabs((m_pArcExternalDistance->getValue(vecVRToExempt))/
			((1/(cos((m_pArcDelta->getValue(vecVRToExempt))/2*(gdPI/180.0))))-1));
	}
	else if(pVarToCalculate == m_pArcDelta)
	{
		double dCosInput = (m_pArcRadius->getValue(vecVRToExempt))/
						   ((m_pArcRadius->getValue(vecVRToExempt))+
						   (m_pArcExternalDistance->getValue(vecVRToExempt)));
		checkAndSetSinCosValue(dCosInput, "ELI05628");
		
		dValue= (2*acos(dCosInput))*(180.0/gdPI);
	}
	else 
	{
		UCLIDException e("ELI90147", "This curve variable at calculateX() is not in VRExternalDistanceToRadiusDelta variable relationship");
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
double VRExternalDistanceToRadiusDelta::calculateY(CurveVariable *pVarToCalculate,
												   vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90148", "Memory not allocated for curve variable at calculateY() in VRExternalDistanceToRadiusDelta");
		throw e;
	}
	return 0.0;
}
//==========================================================================================
bool VRExternalDistanceToRadiusDelta::canCalculateX(CurveVariable *pVarToCalculate,
													vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90149", "Memory not allocated for curve variable at calculateX() in VRExternalDistanceToRadiusDelta");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Finding out whether the desired curve parameter can be calculable or not.
	//Checking whether all the dependent variables can be calculatable, if so 
	//return 'True' otherwise 'False'
	if(pVarToCalculate == m_pArcExternalDistance)
	{
		return (m_pArcRadius->canGetValue(vecVRToExempt)
			&&m_pArcDelta->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcRadius)
	{
		return (m_pArcExternalDistance->canGetValue(vecVRToExempt)
			&&m_pArcDelta->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcDelta)
	{
		return (m_pArcRadius->canGetValue(vecVRToExempt)
			&&m_pArcExternalDistance->canGetValue(vecVRToExempt));
	}
	else 
	{
		UCLIDException e("ELI90150", "This curve variable at canCalculateX() is not in VRExternalDistanceToRadiusDelta variable relationship");
		throw e;
	}
}
//==========================================================================================
bool VRExternalDistanceToRadiusDelta::canCalculateY(CurveVariable *pVarToCalculate,
													vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90151", "Memory not allocated for curve variable at calculateY() in VRExternalDistanceToRadiusDelta");
		throw e;
	}
	return false;
}
//==========================================================================================

