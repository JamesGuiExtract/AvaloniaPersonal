//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE		:	VRRoutBearingToToutBearing.cpp
//
// PURPOSE	:	This is an implementation file for VRRoutBearingToToutBearing class.
//				Where the VRRoutBearingToToutBearing class has been derived from 
//				CurveVariableRelationship class.The code written in this file makes 
//				it possible to implement functionality to add the variable to the 
//				respective variable relationship and calculate the Curve variable
//
// NOTES	:	Nothing
//
// AUTHORS	:	G Chandra Sekhar Babu
//
//==================================================================================================
// VRRoutBearingToToutBearing.cpp : implementation file
//
#include "stdafx.h"

#include "VRRoutBearingToToutBearing.h"
#include "CurveVariable.h"

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// VRRoutBearingToToutBearing

VRRoutBearingToToutBearing::VRRoutBearingToToutBearing
							(
							CurveVariable *pArcConcaveLeft,CurveVariable *pArcRadialOutBearing, 
							CurveVariable *pArcTangentOutBearing
							)
							:m_pArcConcaveLeft(pArcConcaveLeft),m_pArcRadialOutBearing(pArcRadialOutBearing),
							 m_pArcTangentOutBearing(pArcTangentOutBearing)
{
	//adding the variables to this variable relationship
	addVariable(pArcConcaveLeft);
	addVariable(pArcRadialOutBearing);
	addVariable(pArcTangentOutBearing);
}
//==========================================================================================
double VRRoutBearingToToutBearing::calculateX(CurveVariable *pVarToCalculate,
											  vector<CurveVariableRelationship*> vecVRToExempt)
{
	double dValue=0.0;
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90180", "Memory not allocated for curve variable at calculateX() in VRRoutBearingToToutBearing");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Calculating the curve parameter with the respective equation 
	//and returning the same
	if(pVarToCalculate == m_pArcRadialOutBearing)
	{
		double dArcRadialOutBearing=0.0; 
		bool bCurveLeft = m_pArcConcaveLeft->getBooleanValue(vecVRToExempt);
		
		if (bCurveLeft)
		{
			dArcRadialOutBearing = m_pArcTangentOutBearing->getValue(vecVRToExempt) - 90;
		}
		else
		{
			dArcRadialOutBearing = m_pArcTangentOutBearing->getValue(vecVRToExempt) + 90;
		}
		
		dValue=dArcRadialOutBearing;
	}
	else if(pVarToCalculate == m_pArcTangentOutBearing)
	{
		double dArcTangentOutBearing=0.0;
		bool bCurveLeft = m_pArcConcaveLeft->getBooleanValue(vecVRToExempt);

		if (bCurveLeft)
		{
			dArcTangentOutBearing = m_pArcRadialOutBearing->getValue(vecVRToExempt) + 90;
		}
		else
		{
			dArcTangentOutBearing = m_pArcRadialOutBearing->getValue(vecVRToExempt) - 90;
		}
	
		dValue=dArcTangentOutBearing;
	}
	else
	{
		UCLIDException e("ELI90181", "This curve variable at calculateX() is not in VRRoutBearingToToutBearing variable relationship");
		throw e;
	}
	CurveVariable var;
	return var.checkCurveAngle(dValue);
}
//==========================================================================================
double VRRoutBearingToToutBearing::calculateY(CurveVariable *pVarToCalculate,
											  vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90182", "Memory not allocated for curve variable at calculateY() in VRRoutBearingToToutBearing");
		throw e;
	}
	return 0.0;
}
//==========================================================================================
bool VRRoutBearingToToutBearing::canCalculateX(CurveVariable *pVarToCalculate,
											   vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90183", "Memory not allocated for curve variable at calculateX() in VRRoutBearingToToutBearing");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Finding out whether the desired curve parameter can be calculable or not.
	//Checking whether all the dependent variables can be calculatable, if so 
	//return 'True' otherwise 'False'
	if(pVarToCalculate == m_pArcRadialOutBearing)
	{
		return (m_pArcTangentOutBearing->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcTangentOutBearing)
	{
		return (m_pArcRadialOutBearing->canGetValue(vecVRToExempt));
	}
	else
	{
		UCLIDException e("ELI90184", "This curve variable at canCalculateX() is not in VRRoutBearingToToutBearing variable relationship");
		throw e;
	}
}
//==========================================================================================
bool VRRoutBearingToToutBearing::canCalculateY(CurveVariable *pVarToCalculate,
											   vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90185", "Memory not allocated for curve variable at calculateY() in VRRoutBearingToToutBearing");
		throw e;
	}
	return false;
}
//==========================================================================================


