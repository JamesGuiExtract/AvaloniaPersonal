//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE		:	VRRinBearingToTinBearing.cpp
//
// PURPOSE	:	This is an implementation file for VRRinBearingToTinBearing class.
//				Where the VRRinBearingToTinBearing class has been derived from 
//				CurveVariableRelationship class.The code written in this file makes 
//				it possible to implement functionality to add the variable to the 
//				respective variable relationship and calculate the Curve variable
//
// NOTES	:	Nothing
//
// AUTHORS	:	G Chandra Sekhar Babu
//
//==================================================================================================
// VRRinBearingToTinBearing.cpp : implementation file
//
#include "stdafx.h"

#include "VRRinBearingToTinBearing.h"
#include "CurveVariable.h"

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// VRRinBearingToTinBearing

VRRinBearingToTinBearing::VRRinBearingToTinBearing
						  (
						  CurveVariable *pArcConcaveLeft,CurveVariable *pArcRadialInBearing,
						  CurveVariable *pArcTangentInBearing
						  )
						  :m_pArcConcaveLeft(pArcConcaveLeft),
						   m_pArcRadialInBearing(pArcRadialInBearing),
						   m_pArcTangentInBearing(pArcTangentInBearing)
{
	//adding the variables to this variable relationship
	addVariable(pArcConcaveLeft);
	addVariable(pArcRadialInBearing);
	addVariable(pArcTangentInBearing);
}
//==========================================================================================
double VRRinBearingToTinBearing::calculateX(CurveVariable *pVarToCalculate,
											vector<CurveVariableRelationship*> vecVRToExempt)
{
	double dValue=0.0;
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90174", "Memory not allocated for curve variable at calculateX() in VRRinBearingToTinBearing");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Calculating the curve parameter with the respective equation 
	//and returning the same
	if(pVarToCalculate == m_pArcRadialInBearing)
	{
		double dArcRadialInBearing=0.0; 
		bool bCurveLeft = m_pArcConcaveLeft->getBooleanValue(vecVRToExempt);
		
		if (bCurveLeft)
		{
			dArcRadialInBearing = m_pArcTangentInBearing->getValue(vecVRToExempt) + 90;
		}
		else
		{
			dArcRadialInBearing = m_pArcTangentInBearing->getValue(vecVRToExempt) - 90;
		}
		
		dValue= dArcRadialInBearing;
	}
	else if(pVarToCalculate == m_pArcTangentInBearing)
	{
		double dArcTangentInBearing=0.0;
		bool bCurveLeft = m_pArcConcaveLeft->getBooleanValue(vecVRToExempt);

		if (bCurveLeft)
		{
			dArcTangentInBearing = m_pArcRadialInBearing->getValue(vecVRToExempt) - 90;
		}
		else
		{
			dArcTangentInBearing = m_pArcRadialInBearing->getValue(vecVRToExempt) + 90;
		}

		dValue=dArcTangentInBearing;
	}
	else
	{
		UCLIDException e("ELI90175", "vecVRToExempt curve variable at calculateX() is not in VRRinBearingToTinBearing variable relationship");
		throw e;
	}
	CurveVariable var;
	return var.checkCurveAngle(dValue);
}
//==========================================================================================
double VRRinBearingToTinBearing::calculateY(CurveVariable *pVarToCalculate,
											vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90176", "Memory not allocated for curve variable at calculateY() in VRRinBearingToTinBearing");
		throw e;
	}
	return 0.0;
}
//==========================================================================================
bool VRRinBearingToTinBearing::canCalculateX(CurveVariable *pVarToCalculate,
											 vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90177", "Memory not allocated for curve variable at calculateX() in VRRinBearingToTinBearing");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Finding out whether the desired curve parameter can be calculable or not.
	//Checking whether all the dependent variables can be calculatable, if so 
	//return 'True' otherwise 'False'
	if(pVarToCalculate == m_pArcRadialInBearing)
	{
		return (m_pArcTangentInBearing->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcTangentInBearing)
	{
		return (m_pArcRadialInBearing->canGetValue(vecVRToExempt));
	}
	else
	{
		UCLIDException e("ELI90178", "This curve variable at canCalculateX() is not in VRRinBearingToTinBearing variable relationship");
		throw e;
	}
}
//==========================================================================================
bool VRRinBearingToTinBearing::canCalculateY(CurveVariable *pVarToCalculate,
											 vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90179", "Memory not allocated for curve variable at calculateY() in VRRinBearingToTinBearing");
		throw e;
	}
	return false;
}
//==========================================================================================

