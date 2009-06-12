//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE		:	VRArcLengthToRadiusDelta.cpp
//
// PURPOSE	:	This is an implementation file for VRArcLengthToRadiusDelta class.
//				Where the VRArcLengthToRadiusDelta class has been derived from 
//				CurveVariableRelationship class.The code written in this file makes 
//				it possible to implement functionality to add the variable to the 
//				respective variable relationship and to calculate the Curve variable
//
// NOTES	:	Nothing
//
// AUTHORS	:	G Chandra Sekhar Babu
//
//==================================================================================================
// VRArcLengthToRadiusDelta.cpp : implementation file
//

#include "stdafx.h"
#include "VRArcLengthToRadiusDelta.h"
#include "CurveVariable.h"

#include <math.h>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// VRArcLengthToRadiusDelta

VRArcLengthToRadiusDelta::VRArcLengthToRadiusDelta
						  (
						  CurveVariable *pArcLength,CurveVariable *pArcRadius,
						  CurveVariable *pArcDelta
						  )
						  :m_pArcLength(pArcLength),m_pArcRadius(pArcRadius),
						   m_pArcDelta(pArcDelta)
{
	//adding the variables to this variable relationship
	addVariable(pArcLength);
	addVariable(pArcRadius);
	addVariable(pArcDelta);
}
//==========================================================================================
double VRArcLengthToRadiusDelta::calculateX(CurveVariable *pVarToCalculate, 
											vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90067", "Memory not allocated for curve variable at calculateX() in VRArcLengthToRadiusDelta");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Calculating the curve parameter with the respective equation 
	//and returning the same
	if(pVarToCalculate == m_pArcLength)
	{
		return ((gdPI*(m_pArcRadius->getValue(vecVRToExempt))*
				(m_pArcDelta->getValue(vecVRToExempt)))/180);
	}
	else if(pVarToCalculate == m_pArcRadius)
	{
		return (180*(m_pArcLength->getValue(vecVRToExempt)))/
			(gdPI*(m_pArcDelta->getValue(vecVRToExempt)));
	}
	else if(pVarToCalculate == m_pArcDelta)
	{
		return (180*(m_pArcLength->getValue(vecVRToExempt)))/
			(gdPI*(m_pArcRadius->getValue(vecVRToExempt)));
	}
	else 
	{
		UCLIDException e("ELI90068", "This curve variable at calculateX() is not in VRArcLengthToRadiusDelta variable relationship");
		throw e;
	}
}
//==========================================================================================
double VRArcLengthToRadiusDelta::calculateY(CurveVariable *pVarToCalculate,
											vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90069", "Memory not allocated for curve variable at calculateY() in VRArcLengthToRadiusDelta");
		throw e;
	}
	return 0.0;
}
//==========================================================================================
bool VRArcLengthToRadiusDelta::canCalculateX(CurveVariable *pVarToCalculate,
											 vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90070", "Memory not allocated for curve variable at calculateX() in VRArcLengthToRadiusDelta");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Finding out whether the desired curve parameter can be calculable or not.
	//Checking whether all the dependent variables can be calculatable, if so 
	//return 'True' otherwise 'False'
	if(pVarToCalculate == m_pArcLength)
	{
		return (m_pArcRadius->canGetValue(vecVRToExempt)&&m_pArcDelta->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcRadius)
	{
		return (m_pArcLength->canGetValue(vecVRToExempt)&&m_pArcDelta->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcDelta)
	{
		return (m_pArcLength->canGetValue(vecVRToExempt)&&m_pArcRadius->canGetValue(vecVRToExempt));
	}
	else 
	{
		UCLIDException e("ELI90071", "This curve variable at canCalculateX() is not in VRArcLengthToRadiusDelta variable relationship");
		throw e;
	}
}
//==========================================================================================
bool VRArcLengthToRadiusDelta::canCalculateY(CurveVariable *pVarToCalculate,
											 vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90072", "Memory not allocated for curve variable at calculateY() in VRArcLengthToRadiusDelta");
		throw e;
	}
	return false;
}
//==========================================================================================

