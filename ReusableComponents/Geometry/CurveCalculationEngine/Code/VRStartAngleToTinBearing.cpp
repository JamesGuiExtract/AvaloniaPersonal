//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE		:	VRStartAngleToTinBearing.cpp
//
// PURPOSE	:	This is an implementation file for VRStartAngleToTinBearing class.
//				Where the VRStartAngleToTinBearing class has been derived from 
//				CurveVariableRelationship class.The code written in this file makes 
//				it possible to implement functionality to add the variable to the 
//				respective variable relationship and calculate the Curve variable
//
// NOTES	:	Nothing
//
// AUTHORS	:	G Chandra Sekhar Babu
//
//==================================================================================================
// VRStartAngleToTinBearing.cpp : implementation file
//
#include "stdafx.h"

#include "VRStartAngleToTinBearing.h"
#include "CurveVariable.h"

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// VRStartAngleToTinBearing

VRStartAngleToTinBearing::VRStartAngleToTinBearing
						  (
						  CurveVariable *pArcConcaveLeft,CurveVariable *pArcStartAngle,
						  CurveVariable *pArcTangentInBearing
						  )
						  :m_pArcConcaveLeft(pArcConcaveLeft),m_pArcStartAngle(pArcStartAngle),
						   m_pArcTangentInBearing(pArcTangentInBearing)
{
	//adding the variables to this variable relationship
	addVariable(pArcConcaveLeft);
	addVariable(pArcStartAngle);
	addVariable(pArcTangentInBearing);
}
//==========================================================================================
double VRStartAngleToTinBearing::calculateX(CurveVariable *pVarToCalculate,
											vector<CurveVariableRelationship*> vecVRToExempt)
{
	double dValue=0.0;
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90186", "Memory not allocated for curve variable at calculateX() in VRStartAngleToTinBearing");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Calculating the curve parameter with the respective equation 
	//and returning the same
	if(pVarToCalculate == m_pArcStartAngle)
	{
		double dArcStartAngle; 
		bool bCurveLeft = m_pArcConcaveLeft->getBooleanValue(vecVRToExempt);

		if (bCurveLeft)
		{
			dArcStartAngle = m_pArcTangentInBearing->getValue(vecVRToExempt) - 90;
		}
		else
		{
			dArcStartAngle = m_pArcTangentInBearing->getValue(vecVRToExempt) + 90;
		}
		
		dValue=dArcStartAngle;
	}
	else if(pVarToCalculate == m_pArcTangentInBearing)
	{
		double dArcTangentInBearing=0.0;
		bool bCurveLeft = m_pArcConcaveLeft->getBooleanValue(vecVRToExempt);

		if (bCurveLeft)
		{
			dArcTangentInBearing = m_pArcStartAngle->getValue(vecVRToExempt) + 90;
		}
		else
		{
			dArcTangentInBearing = m_pArcStartAngle->getValue(vecVRToExempt) - 90;
		}

		dValue=dArcTangentInBearing;
	}
	else
	{
		UCLIDException e("ELI90187", "This curve variable at calculateX() is not in VRStartAngleToTinBearing variable relationship");
		throw e;
	}
	CurveVariable var;
	return var.checkCurveAngle(dValue);
}
//==========================================================================================
double VRStartAngleToTinBearing::calculateY(CurveVariable *pVarToCalculate,
											vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90188", "Memory not allocated for curve variable at calculateY() in VRStartAngleToTinBearing");
		throw e;
	}
	return 0.0;
}
//==========================================================================================
bool VRStartAngleToTinBearing::canCalculateX(CurveVariable *pVarToCalculate,
											 vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90189", "Memory not allocated for curve variable at calculateX() in VRStartAngleToTinBearing");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Finding out whether the desired curve parameter can be calculable or not.
	//Checking whether all the dependent variables can be calculatable, if so 
	//return 'True' otherwise 'False'
	if(pVarToCalculate == m_pArcStartAngle)
	{
		return (m_pArcTangentInBearing->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcTangentInBearing)
	{
		return (m_pArcStartAngle->canGetValue(vecVRToExempt));
	}
	else
	{
		UCLIDException e("ELI90190", "This curve variable at canCalculateX() is not in VRStartAngleToTinBearing variable relationship");
		throw e;
	}
}
//==========================================================================================
bool VRStartAngleToTinBearing::canCalculateY(CurveVariable *pVarToCalculate,
											 vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90191", "Memory not allocated for curve variable at calculateY() in VRStartAngleToTinBearing");
		throw e;
	}
	return false;
}
//==========================================================================================

