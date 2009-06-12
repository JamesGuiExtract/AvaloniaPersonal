//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE		:	VRDegreeCurveArcToRadius.cpp
//
// PURPOSE	:	This is an implementation file for VRDegreeCurveArcToRadius class.
//				Where the VRDegreeCurveArcToRadius class has been derived from 
//				CurveVariableRelationship class.The code written in this file makes 
//				it possible to implement functionality to add the variable to the 
//				respective variable relationship and to calculate the Curve variable
//
// NOTES	:	Nothing
//
// AUTHORS	:	G Chandra Sekhar Babu
//
//==================================================================================================
// VRDegreeCurveArcToRadius.cpp : implementation file
//


#include "stdafx.h"
#include "VRDegreeCurveArcToRadius.h"
#include "CurveVariable.h"

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// VRDegreeCurveArcToRadius

VRDegreeCurveArcToRadius::VRDegreeCurveArcToRadius
						  (
						  CurveVariable *pArcDegreeOfCurveArcDef,
						  CurveVariable *pArcRadius
						  )
						  :m_pArcDegreeOfCurveArcDef(pArcDegreeOfCurveArcDef),
						   m_pArcRadius(pArcRadius)
{
	//adding the variables to this variable relationship
	addVariable(pArcDegreeOfCurveArcDef);
	addVariable(pArcRadius);
}
//==========================================================================================
double VRDegreeCurveArcToRadius::calculateX(CurveVariable *pVarToCalculate,
											vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90108", "Memory not allocated for curve variable at calculateX() in VRDegreeCurveArcToRadius");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Calculating the curve parameter with the respective equation 
	//and returning the same
	if(pVarToCalculate == m_pArcDegreeOfCurveArcDef)
	{
		if((m_pArcRadius->getValue(vecVRToExempt))<=(50/gdPI))
		{
			UCLIDException e("ELI90254", "radius > (50/pi) for Calculating DegreeofCurveArc");
			throw e;
		}

		return (18000/(gdPI*(m_pArcRadius->getValue(vecVRToExempt))));
	}
	else if(pVarToCalculate == m_pArcRadius)
	{
		return (18000/(gdPI*(m_pArcDegreeOfCurveArcDef->getValue(vecVRToExempt))));
	}
	else
	{
		UCLIDException e("ELI90109", "This curve variable at calculateX() is not in VRDegreeCurveArcToRadius variable relationship");
		throw e;
	}
}
//==========================================================================================
double VRDegreeCurveArcToRadius::calculateY(CurveVariable *pVarToCalculate,
											vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90110", "Memory not allocated for curve variable at calculateY() in VRDegreeCurveArcToRadius");
		throw e;
	}
	return 0.0;
}
//==========================================================================================
bool VRDegreeCurveArcToRadius::canCalculateX(CurveVariable *pVarToCalculate,
											 vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90111", "Memory not allocated for curve variable at calculateX() in VRDegreeCurveArcToRadius");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Finding out whether the desired curve parameter can be calculable or not.
	//Checking whether all the dependent variables can be calculatable, if so 
	//return 'True' otherwise 'False'
	if(pVarToCalculate == m_pArcDegreeOfCurveArcDef)
	{
		return (m_pArcRadius->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcRadius)
	{
		return (m_pArcDegreeOfCurveArcDef->canGetValue(vecVRToExempt));
	}
	else
	{
		UCLIDException e("ELI90112", "This curve variable at canCalculateX() is not in VRDegreeCurveArcToRadius variable relationship");
		throw e;
	}
}
//==========================================================================================
bool VRDegreeCurveArcToRadius::canCalculateY(CurveVariable *pVarToCalculate,
											 vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90113", "Memory not allocated for curve variable at calculateY() in VRDegreeCurveArcToRadius");
		throw e;
	}
	return false;
}
//==========================================================================================


