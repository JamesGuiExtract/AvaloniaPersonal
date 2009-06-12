//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE		:	VRChordBearingToToutDelta.cpp
//
// PURPOSE	:	This is an implementation file for VRChordBearingToToutDelta class.
//				Where the VRChordBearingToToutDelta class has been derived from 
//				CurveVariableRelationship class.The code written in this file makes 
//				it possible to implement functionality to add the variable to the 
//				respective variable relationship and to calculate the Curve variable
//
// NOTES	:	Nothing
//
// AUTHORS	:	G Chandra Sekhar Babu
//
//==================================================================================================
// VRChordBearingToToutDelta.cpp : implementation file
//
#include "stdafx.h"

#include "VRChordBearingToToutDelta.h"
#include "CurveVariable.h"

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// VRChordBearingToToutDelta

VRChordBearingToToutDelta::VRChordBearingToToutDelta
						  (
						  CurveVariable *pArcConcaveLeft,CurveVariable *pArcChordBearing,
						  CurveVariable *pArcTangentOutBearing, CurveVariable *pArcDelta
						  )
						  :m_pArcConcaveLeft(pArcConcaveLeft),m_pArcChordBearing(pArcChordBearing),
						   m_pArcTangentOutBearing(pArcTangentOutBearing),m_pArcDelta(pArcDelta)
{
	//adding the variables to this variable relationship
	addVariable(pArcConcaveLeft);
	addVariable(pArcChordBearing);
	addVariable(pArcTangentOutBearing);
	addVariable(pArcDelta);
}
//==========================================================================================
double VRChordBearingToToutDelta::calculateX(CurveVariable *pVarToCalculate,
											 vector<CurveVariableRelationship*> vecVRToExempt)
{
	double dValue=0.0;
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90246", "Memory not allocated for curve variable at calculateX() in VRChordBearingToToutDelta");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Calculating the curve parameter with the respective equation 
	//and returning the same
	if(pVarToCalculate == m_pArcChordBearing)
	{
		//ArcChordBearing Value
		double dArcChordBearing=0.0;
		bool bCurveLeft = m_pArcConcaveLeft->getBooleanValue(vecVRToExempt);

		if (bCurveLeft)
		{
			dArcChordBearing = m_pArcTangentOutBearing->getValue(vecVRToExempt) 
								- m_pArcDelta->getValue(vecVRToExempt) / 2;

		}
		else
		{			
			dArcChordBearing = m_pArcTangentOutBearing->getValue(vecVRToExempt)
								+ m_pArcDelta->getValue(vecVRToExempt) / 2;
		}

		dValue=dArcChordBearing;
	}
	else if(pVarToCalculate == m_pArcTangentOutBearing)
	{
		double dArcTangentOutBearing=0.0;
		bool bCurveLeft = m_pArcConcaveLeft->getBooleanValue(vecVRToExempt);

		if (bCurveLeft)
		{
			dArcTangentOutBearing = m_pArcChordBearing->getValue(vecVRToExempt)
									+ m_pArcDelta->getValue(vecVRToExempt)/2;
		}
		else
		{
			dArcTangentOutBearing = m_pArcChordBearing->getValue(vecVRToExempt)
									- m_pArcDelta->getValue(vecVRToExempt)/2;
		}

		dValue=dArcTangentOutBearing;
	}
	else if(pVarToCalculate == m_pArcDelta)
	{
		double dArcDelta=0.0; 
		bool bCurveLeft = m_pArcConcaveLeft->getBooleanValue(vecVRToExempt);

		if (bCurveLeft)
		{
			dArcDelta = 2 * ( m_pArcTangentOutBearing->getValue(vecVRToExempt)
							 - m_pArcChordBearing->getValue(vecVRToExempt) );
		}
		else
		{
			dArcDelta = 2 * ( m_pArcChordBearing->getValue(vecVRToExempt)
							 - m_pArcTangentOutBearing->getValue(vecVRToExempt) );
		}

		dValue=dArcDelta;
	}
	else 
	{
		UCLIDException e("ELI90247", "This curve variable at calculateX() is not in VRChordBearingToToutDelta variable relationship");
		throw e;
	}
	CurveVariable var;
	return var.checkCurveAngle(dValue);
}
//==========================================================================================
double VRChordBearingToToutDelta::calculateY(CurveVariable *pVarToCalculate,
											 vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90248", "Memory not allocated for curve variable at calculateY() in VRChordBearingToToutDelta");
		throw e;
	}
	return 0.0;
}
//==========================================================================================
bool VRChordBearingToToutDelta::canCalculateX(CurveVariable *pVarToCalculate,
											  vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90249", "Memory not allocated for curve variable at calculateX() in VRChordBearingToToutDelta");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Finding out whether the desired curve parameter can be calculable or not.
	//Checking whether all the dependent variables can be calculatable, if so 
	//return 'True' otherwise 'False'
	if(pVarToCalculate == m_pArcChordBearing)
	{
		return (m_pArcTangentOutBearing->canGetValue(vecVRToExempt)
			&&m_pArcDelta->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcTangentOutBearing)
	{
		return (m_pArcChordBearing->canGetValue(vecVRToExempt)
			&&m_pArcDelta->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcDelta)
	{
		return (m_pArcChordBearing->canGetValue(vecVRToExempt)
			&&m_pArcTangentOutBearing->canGetValue(vecVRToExempt));
	}
	else 
	{
		UCLIDException e("ELI90250", "This curve variable at canCalculateX() is not in VRChordBearingToToutDelta variable relationship");
		throw e;
	}
}
//==========================================================================================
bool VRChordBearingToToutDelta::canCalculateY(CurveVariable *pVarToCalculate,
											  vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90251", "Memory not allocated for curve variable at calculateY() in VRChordBearingToToutDelta");
		throw e;
	}
	return false;
}
//==========================================================================================

