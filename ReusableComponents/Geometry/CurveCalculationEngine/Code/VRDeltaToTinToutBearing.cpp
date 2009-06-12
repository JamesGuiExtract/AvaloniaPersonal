//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE		:	VRDeltaToTinToutBearing.cpp
//
// PURPOSE	:	This is an implementation file for VRDeltaToTinToutBearing class.
//				Where the VRDeltaToTinToutBearing class has been derived from 
//				CurveVariableRelationship class.The code written in this file makes 
//				it possible to implement functionality to add the variable to the 
//				respective variable relationship and calculate the Curve variable
//
// NOTES	:	Nothing
//
// AUTHORS	:	G Chandra Sekhar Babu	
//
//==================================================================================================
// VRDeltaToTinToutBearing.cpp : implementation file
//


#include "stdafx.h"
#include "VRDeltaToTinToutBearing.h"
#include "CurveVariable.h"

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// VRDeltaToTinToutBearing

VRDeltaToTinToutBearing::VRDeltaToTinToutBearing
						 (
						 CurveVariable *pArcConcaveLeft,CurveVariable *pArcDelta,
						 CurveVariable *pArcTangentInBearing,CurveVariable *pArcTangentOutBearing
						 )
						 :m_pArcConcaveLeft(pArcConcaveLeft),m_pArcDelta(pArcDelta),
						  m_pArcTangentInBearing(pArcTangentInBearing),
						  m_pArcTangentOutBearing(pArcTangentOutBearing)
{
	//adding the variables to this variable relationship
	addVariable(pArcConcaveLeft);
	addVariable(pArcDelta);
	addVariable(pArcTangentInBearing);
	addVariable(pArcTangentOutBearing);
}
//==========================================================================================
double VRDeltaToTinToutBearing::calculateX(CurveVariable *pVarToCalculate,
										   vector<CurveVariableRelationship*> vecVRToExempt)
{
	double dValue=0.0;
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90120", "Memory not allocated for curve variable at calculateX() in VRDeltaToTinToutBearing");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Calculating the curve parameter with the respective equation 
	//and returning the same
	if(pVarToCalculate == m_pArcDelta)
	{
		//ArcDelta Value
		double dArcDelta=0.0; 
		bool bCurveLeft = m_pArcConcaveLeft->getBooleanValue(vecVRToExempt);

		if (bCurveLeft)
		{
			dArcDelta = m_pArcTangentOutBearing->getValue(vecVRToExempt)
						- m_pArcTangentInBearing->getValue(vecVRToExempt);
		}
		else
		{
			dArcDelta = m_pArcTangentInBearing->getValue(vecVRToExempt)
						- m_pArcTangentOutBearing->getValue(vecVRToExempt);
		}
		
		dValue=dArcDelta;
	}
	else if(pVarToCalculate == m_pArcTangentOutBearing)
	{
		double dArcTangentOutBearing=0.0;
		bool bCurveLeft = m_pArcConcaveLeft->getBooleanValue(vecVRToExempt);

		if (bCurveLeft)
		{
			dArcTangentOutBearing = m_pArcDelta->getValue(vecVRToExempt) 
									+ m_pArcTangentInBearing->getValue(vecVRToExempt);
		}
		else
		{
			dArcTangentOutBearing = m_pArcTangentInBearing->getValue(vecVRToExempt)
									- m_pArcDelta->getValue(vecVRToExempt);
		}

		dValue=dArcTangentOutBearing;
	}
	else if(pVarToCalculate == m_pArcTangentInBearing)
	{
		double dArcTangentInBearing=0.0;
		bool bCurveLeft = m_pArcConcaveLeft->getBooleanValue(vecVRToExempt);

		if (bCurveLeft)
		{
			dArcTangentInBearing = m_pArcTangentOutBearing->getValue(vecVRToExempt)
									- m_pArcDelta->getValue(vecVRToExempt);
		}
		else
		{
			dArcTangentInBearing = m_pArcTangentOutBearing->getValue(vecVRToExempt)
									+ m_pArcDelta->getValue(vecVRToExempt);
		}
		
		dValue=dArcTangentInBearing;
	}
	else 
	{
		UCLIDException e("ELI90121", "This curve variable at calculateX() is not in VRDeltaToTinToutBearing variable relationship");
		throw e;
	}
	CurveVariable var;
	return var.checkCurveAngle(dValue);
}
//==========================================================================================
double VRDeltaToTinToutBearing::calculateY(CurveVariable *pVarToCalculate,
										   vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90122", "Memory not allocated for curve variable at calculateY() in VRDeltaToTinToutBearing");
		throw e;
	}
	return 0.0;
}
//==========================================================================================
bool VRDeltaToTinToutBearing::canCalculateX(CurveVariable *pVarToCalculate,
											vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90123", "Memory not allocated for curve variable at calculateX() in VRDeltaToTinToutBearing");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Finding out whether the desired curve parameter can be calculable or not.
	//Checking whether all the dependent variables can be calculatable, if so 
	//return 'True' otherwise 'False'
	if(pVarToCalculate == m_pArcDelta)
	{
		return (m_pArcTangentOutBearing->canGetValue(vecVRToExempt)
			&&m_pArcTangentInBearing->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcTangentOutBearing)
	{
		return (m_pArcDelta->canGetValue(vecVRToExempt)
			&&m_pArcTangentInBearing->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcTangentInBearing)
	{
		return (m_pArcTangentOutBearing->canGetValue(vecVRToExempt)
			&&m_pArcDelta->canGetValue(vecVRToExempt));
	}
	else 
	{
		UCLIDException e("ELI90124", "This curve variable at canCalculateX() is not in VRDeltaToTinToutBearing variable relationship");
		throw e;
	}
}
//==========================================================================================
bool VRDeltaToTinToutBearing::canCalculateY(CurveVariable *pVarToCalculate,
											vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90125", "Memory not allocated for curve variable at calculateY() in VRDeltaToTinToutBearing");
		throw e;
	}
	return false;
}
//==========================================================================================

