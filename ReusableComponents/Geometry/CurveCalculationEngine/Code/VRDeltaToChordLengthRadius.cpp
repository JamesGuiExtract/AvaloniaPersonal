//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE		:	VRDeltaToChordLengthRadius.cpp
//
// PURPOSE	:	This is an implementation file for VRDeltaToChordLengthRadius class.
//				Where the VRDeltaToChordLengthRadius class has been derived from 
//				CurveVariableRelationship class.The code written in this file makes 
//				it possible to implement functionality to add the variable to the 
//				respective variable relationship and to calculate the Curve variable
//
// NOTES	:	Nothing
//
// AUTHORS	:	G Chandra Sekhar Babu	
//
//==================================================================================================
// VRDeltaToChordLengthRadius.cpp : implementation file
//


#include "stdafx.h"
#include "VRDeltaToChordLengthRadius.h"
#include "CurveVariable.h"

#include <math.h>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// VRDeltaToChordLengthRadius

VRDeltaToChordLengthRadius::VRDeltaToChordLengthRadius
							(
							CurveVariable *pArcDeltaGreaterThan180Degrees,CurveVariable *pArcDelta,
							CurveVariable *pArcChordLength, CurveVariable *pArcRadius
							)
							:m_pArcDeltaGreaterThan180Degrees(pArcDeltaGreaterThan180Degrees),
							 m_pArcDelta(pArcDelta),m_pArcChordLength(pArcChordLength),m_pArcRadius(pArcRadius)
{
	//adding the variables to this variable relationship
	addVariable(pArcDelta);
	addVariable(pArcChordLength);
	addVariable(pArcRadius);
	addVariable(pArcDeltaGreaterThan180Degrees);
}
//==========================================================================================
double VRDeltaToChordLengthRadius::calculateX(CurveVariable *pVarToCalculate,
											  vector<CurveVariableRelationship*> vecVRToExempt)
{
	//Curve Point parameter value 
	double dValue=0.0;
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90126", "Memory not allocated for curve variable at calculateX() in VRDeltaToChordLengthRadius");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Calculating the curve parameter with the respective equation 
	//and returning the same
	if(pVarToCalculate == m_pArcDelta)
	{
		bool bDeltaGT180 = m_pArcDeltaGreaterThan180Degrees->getBooleanValue(vecVRToExempt);

		double dChordLength = m_pArcChordLength->getValue(vecVRToExempt);
		double dRadius = m_pArcRadius->getValue(vecVRToExempt);
		double dSinValue = dChordLength / (2 * dRadius);
		checkAndSetSinCosValue(dSinValue, "ELI05614");

		if (!bDeltaGT180)
		{
			dValue= 2 * asin(dSinValue)*(180.0/gdPI);
		}
		else
		{
			dValue = 360.0 - 2 * asin(dSinValue)*(180.0/gdPI);
		}
	}
	else if(pVarToCalculate == m_pArcChordLength)
	{
		double dRadius = m_pArcRadius->getValue(vecVRToExempt);
		dValue= 2 * dRadius * sin( (m_pArcDelta->getValue(vecVRToExempt)/2) * (gdPI/180.0) );
	}
	else if(pVarToCalculate == m_pArcRadius)
	{
		double dChordLength = m_pArcChordLength->getValue(vecVRToExempt);
		dValue= dChordLength/(2 * sin( (m_pArcDelta->getValue(vecVRToExempt)/2) * (gdPI/180.0)) );
	}
	else 
	{
		UCLIDException e("ELI90127", "This curve variable at calculateX() is not in VRDeltaToChordLengthRadius variable relationship");
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
double VRDeltaToChordLengthRadius::calculateY(CurveVariable *pVarToCalculate,
											  vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90128", "Memory not allocated for curve variable at calculateY() in VRDeltaToChordLengthRadius");
		throw e;
	}
	return 0.0;
}
//==========================================================================================
bool VRDeltaToChordLengthRadius::canCalculateX(CurveVariable *pVarToCalculate,
											   vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90129", "Memory not allocated for curve variable at calculateX() in VRDeltaToChordLengthRadius");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Finding out whether the desired curve parameter can be calculable or not.
	//Checking whether all the dependent variables can be calculatable, if so 
	//return 'True' otherwise 'False'
	if (pVarToCalculate == m_pArcDeltaGreaterThan180Degrees)
	{
		return false;
	}
	else if (pVarToCalculate == m_pArcDelta)
	{
		return (m_pArcChordLength->canGetValue(vecVRToExempt)
			&&m_pArcRadius->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcChordLength)
	{
		return (m_pArcRadius->canGetValue(vecVRToExempt)
			&&m_pArcDelta->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcRadius)
	{
		return (m_pArcChordLength->canGetValue(vecVRToExempt)
			&&m_pArcDelta->canGetValue(vecVRToExempt));
	}
	else 
	{
		UCLIDException e("ELI90130", "This curve variable at canCalculateX() is not in VRDeltaToChordLengthRadius variable relationship");
		throw e;
	}
}
//==========================================================================================
bool VRDeltaToChordLengthRadius::canCalculateY(CurveVariable *pVarToCalculate,
											   vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90131", "Memory not allocated for curve variable at calculateY() in VRDeltaToChordLengthRadius");
		throw e;
	}
	return false;
}
//==========================================================================================
bool VRDeltaToChordLengthRadius::isCalculatable(CurveVariable *pVarToCalculate,
												vector<CurveVariableRelationship*> vecVRToExempt)
{
	if (pVarToCalculate == m_pArcDeltaGreaterThan180Degrees)
	{
		return false;
	}

	return CurveVariableRelationship::isCalculatable(pVarToCalculate, vecVRToExempt);
}