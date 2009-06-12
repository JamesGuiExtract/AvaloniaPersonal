///==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE		:	VREndAngleToToutBearing.cpp
//
// PURPOSE	:	This is an implementation file for VREndAngleToToutBearing class.
//				Where the VREndAngleToToutBearing class has been derived from 
//				CurveVariableRelationship class.The code written in this file makes 
//				it possible to implement functionality to add the variable to the 
//				respective variable relationship and to calculate the Curve variable
//
// NOTES	:	Nothing
//
// AUTHORS	:	G Chandra Sekhar Babu
//
//==================================================================================================
// VREndAngleToToutBearing.cpp : implementation file
//


#include "stdafx.h"
#include "VREndAngleToToutBearing.h"
#include "CurveVariable.h"

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// VREndAngleToToutBearing

VREndAngleToToutBearing::VREndAngleToToutBearing
						 (
						 CurveVariable *pArcConcaveLeft,CurveVariable *pArcEndAngle,
						 CurveVariable *pArcTangentOutBearing
						 )
						 :m_pArcConcaveLeft(pArcConcaveLeft),m_pArcEndAngle(pArcEndAngle),
						  m_pArcTangentOutBearing(pArcTangentOutBearing)
{
	//adding the variables to this variable relationship
	addVariable(pArcConcaveLeft);
	addVariable(pArcEndAngle);
	addVariable(pArcTangentOutBearing);
}
//==========================================================================================
double VREndAngleToToutBearing::calculateX(CurveVariable *pVarToCalculate,
										   vector<CurveVariableRelationship*> vecVRToExempt)
{
	double dValue=0.0;
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90132", "Memory not allocated for curve variable at calculateX() in VREndAngleToToutBearing");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Calculating the curve parameter with the respective equation 
	//and returning the same
	if(pVarToCalculate == m_pArcEndAngle)
	{
		//ArcEndAngle Value
		double dArcEndAngle;
		bool bCurveLeft = m_pArcConcaveLeft->getBooleanValue(vecVRToExempt);

		if (bCurveLeft)
		{
			dArcEndAngle = m_pArcTangentOutBearing->getValue(vecVRToExempt) - 90;
		}
		else
		{
			dArcEndAngle = m_pArcTangentOutBearing->getValue(vecVRToExempt) + 90;
		}
		
		dValue=dArcEndAngle;
	}
	else if(pVarToCalculate == m_pArcTangentOutBearing)
	{
		double dArcTangentOutBearing=0.0;
		bool bCurveLeft = m_pArcConcaveLeft->getBooleanValue(vecVRToExempt);

		if (bCurveLeft)
		{
			dArcTangentOutBearing = m_pArcEndAngle->getValue(vecVRToExempt) + 90;
		}
		else
		{
			dArcTangentOutBearing = m_pArcEndAngle->getValue(vecVRToExempt) - 90;
		}

		dValue=dArcTangentOutBearing;
	}
	else
	{
		UCLIDException e("ELI90133", "This curve variable at calculateX() is not in VREndAngleToToutBearing variable relationship");
		throw e;
	}
	CurveVariable var;
	return var.checkCurveAngle(dValue);
}
//==========================================================================================
double VREndAngleToToutBearing::calculateY(CurveVariable *pVarToCalculate,
										   vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90134", "Memory not allocated for curve variable at calculateY() in VREndAngleToToutBearing");
		throw e;
	}
	return 0.0;
}
//==========================================================================================
bool VREndAngleToToutBearing::canCalculateX(CurveVariable *pVarToCalculate,
											vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90135", "Memory not allocated for curve variable at calculateX() in VREndAngleToToutBearing");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Finding out whether the desired curve parameter can be calculable or not.
	//Checking whether all the dependent variables can be calculatable, if so 
	//return 'True' otherwise 'False'
	if(pVarToCalculate == m_pArcEndAngle)
	{
		return (m_pArcTangentOutBearing->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcTangentOutBearing)
	{
		return (m_pArcEndAngle->canGetValue(vecVRToExempt));
	}
	else
	{
		UCLIDException e("ELI90136", "This curve variable at canCalculateX() is not in VREndAngleToToutBearing variable relationship");
		throw e;
	}
}
//==========================================================================================
bool VREndAngleToToutBearing::canCalculateY(CurveVariable *pVarToCalculate,
											vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90137", "Memory not allocated for curve variable at calculateY() in VREndAngleToToutBearing");
		throw e;
	}
	return false;
}
//==========================================================================================

