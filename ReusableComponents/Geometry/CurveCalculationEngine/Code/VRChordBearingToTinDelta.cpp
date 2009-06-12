//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE		:	VRChordBearingToTinDelta.cpp
//
// PURPOSE	:	This is an implementation file for VRChordBearingToTinDelta class.
//				Where the VRChordBearingToTinDelta class has been derived from 
//				CurveVariableRelationship class.The code written in this file makes 
//				it possible to implement functionality to add the variable to the 
//				respective variable relationship and to calculate the Curve variable
//
// NOTES	:	Nothing
//
// AUTHORS	:	G Chandra Sekhar Babu
//
//==================================================================================================
// VRChordBearingToTinDelta.cpp : implementation file
//


#include "stdafx.h"
#include "VRChordBearingToTinDelta.h"
#include "CurveVariable.h"

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// VRChordBearingToTinDelta

VRChordBearingToTinDelta::VRChordBearingToTinDelta
						  (
						  CurveVariable *pArcConcaveLeft,CurveVariable *pArcChordBearing,
						  CurveVariable *pArcTangentInBearing, CurveVariable *pArcDelta
						  )
						  :m_pArcConcaveLeft(pArcConcaveLeft),m_pArcChordBearing(pArcChordBearing),
						   m_pArcTangentInBearing(pArcTangentInBearing),m_pArcDelta(pArcDelta)
{
	//adding the variables to this variable relationship
	addVariable(pArcConcaveLeft);
	addVariable(pArcChordBearing);
	addVariable(pArcTangentInBearing);
	addVariable(pArcDelta);
}
//==========================================================================================
double VRChordBearingToTinDelta::calculateX(CurveVariable *pVarToCalculate,
											vector<CurveVariableRelationship*> vecVRToExempt)
{
	double dValue=0.0;
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90088", "Memory not allocated for curve variable at calculateX() in VRChordBearingToTinDelta");
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
			dArcChordBearing = m_pArcTangentInBearing->getValue(vecVRToExempt) 
								+ m_pArcDelta->getValue(vecVRToExempt)/2;
		}
		else
		{
			dArcChordBearing = m_pArcTangentInBearing->getValue(vecVRToExempt) 
								- m_pArcDelta->getValue(vecVRToExempt)/2;
		}

		dValue=dArcChordBearing;
	}
	else if(pVarToCalculate == m_pArcTangentInBearing)
	{
		double dArcTangentInBearing=0.0;
		bool bCurveLeft = m_pArcConcaveLeft->getBooleanValue(vecVRToExempt);

		if (bCurveLeft)
		{
			dArcTangentInBearing= m_pArcChordBearing->getValue(vecVRToExempt) 
									- m_pArcDelta->getValue(vecVRToExempt)/2;
		}
		else
		{
			dArcTangentInBearing= m_pArcChordBearing->getValue(vecVRToExempt) 
									+ m_pArcDelta->getValue(vecVRToExempt)/2;
		}

		dValue=dArcTangentInBearing;
	}
	else if(pVarToCalculate == m_pArcDelta)
	{
		double dArcDelta=0.0; 
		bool bCurveLeft = m_pArcConcaveLeft->getBooleanValue(vecVRToExempt);

		if (bCurveLeft)
		{
			dArcDelta= 2 * ( m_pArcChordBearing->getValue(vecVRToExempt) 
							- m_pArcTangentInBearing->getValue(vecVRToExempt) );
		}
		else
		{
			dArcDelta= 2 * (  m_pArcTangentInBearing->getValue(vecVRToExempt)
							- m_pArcChordBearing->getValue(vecVRToExempt) );
		}

		dValue=dArcDelta;
	}
	else 
	{
		UCLIDException e("ELI90089", "This curve variable at calculateX() is not in VRChordBearingToTinDelta variable relationship");
		throw e;
	}
	CurveVariable var;
	return var.checkCurveAngle(dValue);
}
//==========================================================================================
double VRChordBearingToTinDelta::calculateY(CurveVariable *pVarToCalculate,
											vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90090", "Memory not allocated for curve variable at calculateY() in VRChordBearingToTinDelta");
		throw e;
	}
	return 0.0;
}
//==========================================================================================
bool VRChordBearingToTinDelta::canCalculateX(CurveVariable *pVarToCalculate,
											 vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90091", "Memory not allocated for curve variable at calculateX() in VRChordBearingToTinDelta");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Finding out whether the desired curve parameter can be calculable or not.
	//Checking whether all the dependent variables can be calculatable, if so 
	//return 'True' otherwise 'False'
	if(pVarToCalculate == m_pArcChordBearing)
	{
		return (m_pArcTangentInBearing->canGetValue(vecVRToExempt)
			&&m_pArcDelta->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcTangentInBearing)
	{
		return (m_pArcChordBearing->canGetValue(vecVRToExempt)
			&&m_pArcDelta->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcDelta)
	{
		return (m_pArcChordBearing->canGetValue(vecVRToExempt)
			&&m_pArcTangentInBearing->canGetValue(vecVRToExempt));
	}
	else 
	{
		UCLIDException e("ELI90092", "This curve variable at canCalculateX() is not in VRChordBearingToTinDelta variable relationship");
		throw e;
	}
}
//==========================================================================================
bool VRChordBearingToTinDelta::canCalculateY(CurveVariable *pVarToCalculate,
											 vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90093", "Memory not allocated for curve variable at calculateY() in VRChordBearingToTinDelta");
		throw e;
	}
	return false;
}
//==========================================================================================

