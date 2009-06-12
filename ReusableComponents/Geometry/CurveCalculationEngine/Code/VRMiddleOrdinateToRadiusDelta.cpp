//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE		:	VRMiddleOrdinateToRadiusDelta.cpp
//
// PURPOSE	:	This is an implementation file for VRMiddleOrdinateToRadiusDelta class.
//				Where the VRMiddleOrdinateToRadiusDelta class has been derived from 
//				CurveVariableRelationship class.The code written in this file makes 
//				it possible to implement functionality to add the variable to the 
//				respective variable relationship and calculate the Curve variable
//
// NOTES	:	Nothing
//
// AUTHORS	:	G Chandra Sekhar Babu
//				Duan Wang
//
//==================================================================================================
// VRMiddleOrdinateToRadiusDelta.cpp : implementation file
//


#include "stdafx.h"

#include "VRMiddleOrdinateToRadiusDelta.h"
#include "CurveVariable.h"

#include <math.h>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// VRMiddleOrdinateToRadiusDelta

VRMiddleOrdinateToRadiusDelta::VRMiddleOrdinateToRadiusDelta
							   (
								CurveVariable *pArcMiddleOrdinate,
								CurveVariable *pArcRadius,
								CurveVariable *pArcDelta
							   )
:m_pArcMiddleOrdinate(pArcMiddleOrdinate),m_pArcRadius(pArcRadius),
 m_pArcDelta(pArcDelta)
{
	//adding the variables to this variable relationship
	addVariable(pArcMiddleOrdinate);
	addVariable(pArcRadius);
	addVariable(pArcDelta);
}
//==========================================================================================
double VRMiddleOrdinateToRadiusDelta::calculateX(CurveVariable *pVarToCalculate,
												 vector<CurveVariableRelationship*> vecVRToExempt)
{
	//Curve Point parameter value 
	double dValue=0.0;
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90160", "Memory not allocated for curve variable at calculateX() in VRMiddleOrdinateToRadiusDelta");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Calculating the curve parameter with the respective equation 
	//and returning the same
	if(pVarToCalculate == m_pArcMiddleOrdinate)
	{
		double dDelta = m_pArcDelta->getValue(vecVRToExempt);
		double dRadius = m_pArcRadius->getValue(vecVRToExempt);
		
		dValue= dRadius * ( 1 - cos(dDelta*gdPI/180.0 / 2) );
	}
	else if(pVarToCalculate == m_pArcRadius)
	{
		double dMidOrdinate = m_pArcMiddleOrdinate->getValue(vecVRToExempt);
		
		double dDelta = m_pArcDelta->getValue(vecVRToExempt);
		
		dValue= dMidOrdinate / ( 1 - cos(dDelta*gdPI/180.0 / 2) );
	}
	else if(pVarToCalculate == m_pArcDelta)
	{
		double dCosInput=0.0;

		double dMidOrdinate = m_pArcMiddleOrdinate->getValue(vecVRToExempt);
		double dRadius = m_pArcRadius->getValue(vecVRToExempt);
		
		dCosInput= 1 - dMidOrdinate/dRadius;

		checkAndSetSinCosValue(dCosInput, "ELI05631");
		
		dValue= 2*acos(dCosInput)*(180.0/gdPI);
	}
	else 
	{
		UCLIDException e("ELI90161", "This curve variable at calculateX() is not in VRMiddleOrdinateToRadiusDelta variable relationship");
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
double VRMiddleOrdinateToRadiusDelta::calculateY(CurveVariable *pVarToCalculate,
												 vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90162", "Memory not allocated for curve variable at calculateY() in VRMiddleOrdinateToRadiusDelta");
		throw e;
	}
	return 0.0;
}
//==========================================================================================
bool VRMiddleOrdinateToRadiusDelta::canCalculateX(CurveVariable *pVarToCalculate,
												  vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90163", "Memory not allocated for curve variable at calculateX() in VRMiddleOrdinateToRadiusDelta");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Finding out whether the desired curve parameter can be calculable or not.
	//Checking whether all the dependent variables can be calculatable, if so 
	//return 'True' otherwise 'False'
	if(pVarToCalculate == m_pArcMiddleOrdinate)
	{
		return (m_pArcRadius->canGetValue(vecVRToExempt)
			&&m_pArcDelta->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcRadius)
	{
		return (m_pArcMiddleOrdinate->canGetValue(vecVRToExempt)
			&& m_pArcDelta->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcDelta)
	{
		return (m_pArcMiddleOrdinate->canGetValue(vecVRToExempt)
			&&m_pArcRadius->canGetValue(vecVRToExempt));
	}
	else 
	{
		UCLIDException e("ELI90164", "This curve variable at canCalculateX() is not in VRMiddleOrdinateToRadiusDelta variable relationship");
		throw e;
	}
}
//==========================================================================================
bool VRMiddleOrdinateToRadiusDelta::canCalculateY(CurveVariable *pVarToCalculate,
												  vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90165", "Memory not allocated for curve variable at calculateY() in VRMiddleOrdinateToRadiusDelta");
		throw e;
	}
	return false;
}
//==========================================================================================
