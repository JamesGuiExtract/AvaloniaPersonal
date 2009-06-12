//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE		:	VRDegreeCurveChordToRadius.cpp
//
// PURPOSE	:	This is an implementation file for VRDegreeCurveChordToRadius class.
//				Where the VRDegreeCurveChordToRadius class has been derived from 
//				CurveVariableRelationship class.The code written in this file makes 
//				it possible to implement functionality to add the variable to the 
//				respective variable relationship and calculate the Curve variable
//
// NOTES	:	Nothing
//
// AUTHORS	:	G Chandra Sekhar Babu
//
//==================================================================================================
// VRDegreeCurveChordToRadius.cpp : implementation file
//


#include "stdafx.h"

#include "VRDegreeCurveChordToRadius.h"
#include "CurveVariable.h"

#include <math.h>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// VRDegreeCurveChordToRadius

VRDegreeCurveChordToRadius::VRDegreeCurveChordToRadius
							(
							CurveVariable *pArcDegreeOfCurveChordDef,
							CurveVariable *pArcRadius
							)
							:m_pArcDegreeOfCurveChordDef(pArcDegreeOfCurveChordDef),
							 m_pArcRadius(pArcRadius)
{
	//adding the variables to this variable relationship
	addVariable(pArcDegreeOfCurveChordDef);
	addVariable(pArcRadius);
}
//==========================================================================================
double VRDegreeCurveChordToRadius::calculateX(CurveVariable *pVarToCalculate,
											  vector<CurveVariableRelationship*> vecVRToExempt)
{
	//Curve Point parameter value 
	double dValue=0.0;
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90114", "Memory not allocated for curve variable at calculateX() in VRDegreeCurveChordToRadius");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Calculating the curve parameter with the respective equation 
	//and returning the same
	if(pVarToCalculate == m_pArcDegreeOfCurveChordDef)
	{
		if((m_pArcRadius->getValue(vecVRToExempt))<50)
		{
			UCLIDException e("ELI90244", "Radius should be more than 50 for calculating the Degree of Curve(Chord definition)");
			throw e;
		}
		
		double dSinInput = 50/(m_pArcRadius->getValue(vecVRToExempt));
		checkAndSetSinCosValue(dSinInput, "ELI05618");
		
		dValue= (2*asin(dSinInput)*(180/gdPI));
	}
	else if(pVarToCalculate == m_pArcRadius)
	{
		dValue= (50/
			(sin(((m_pArcDegreeOfCurveChordDef->getValue(vecVRToExempt))/2)*(gdPI/180))));
	}
	else
	{
		UCLIDException e("ELI90115", "This curve variable at calculateX() is not in VRDegreeCurveChordToRadius variable relationship");
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
double VRDegreeCurveChordToRadius::calculateY(CurveVariable *pVarToCalculate,
											  vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90116", "Memory not allocated for curve variable at calculateY() in VRDegreeCurveChordToRadius");
		throw e;
	}
	return 0.0;
}
//==========================================================================================
bool VRDegreeCurveChordToRadius::canCalculateX(CurveVariable *pVarToCalculate,
											   vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90117", "Memory not allocated for curve variable at calculateX() in VRDegreeCurveChordToRadius");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Finding out whether the desired curve parameter can be calculable or not.
	//Checking whether all the dependent variables can be calculatable, if so 
	//return 'True' otherwise 'False'
	if(pVarToCalculate == m_pArcDegreeOfCurveChordDef)
	{
		return (m_pArcRadius->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcRadius)
	{
		return (m_pArcDegreeOfCurveChordDef->canGetValue(vecVRToExempt));
	}
	else
	{
		UCLIDException e("ELI90118", "This curve variable at canCalculateX() is not in VRDegreeCurveChordToRadius variable relationship");
		throw e;
	}
}
//==========================================================================================
bool VRDegreeCurveChordToRadius::canCalculateY(CurveVariable *pVarToCalculate,
											   vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90119", "Memory not allocated for curve variable at calculateY() in VRDegreeCurveChordToRadius");
		throw e;
	}
	return false;
}
//==========================================================================================

