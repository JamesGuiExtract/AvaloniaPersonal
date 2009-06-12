//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE		:	VRDeltaAngleToDeltaGreaterThan180.cpp
//
// PURPOSE	:	This is an implementation file for VRDeltaAngleToDeltaGreaterThan180 class.
//				Where the VRDeltaAngleToDeltaGreaterThan180 class has been derived from 
//				CurveVariableRelationship class.The code written in this file makes 
//				it possible to implement functionality to add the variable to the 
//				respective variable relationship and to calculate the Curve variable
//
// NOTES	:	Nothing
//
// AUTHORS	:	Duan Wang
//
//==================================================================================================
// VRArcLengthToRadiusDelta.cpp : implementation file
//

#include "stdafx.h"
#include "VRDeltaAngleToDeltaGreaterThan180.h"
#include "CurveVariable.h"

#include <math.h>

using namespace std;

VRDeltaAngleToDeltaGreaterThan180::VRDeltaAngleToDeltaGreaterThan180
									(
									CurveVariable *pDeltaAngle,
									CurveVariable *pDeltaGT180
									)
:m_pDeltaAngle(pDeltaAngle), m_pDeltaGT180(pDeltaGT180)
{
	addVariable(pDeltaAngle);
	addVariable(pDeltaGT180);
}
//----------------------------------------------------------------------------------------------
double VRDeltaAngleToDeltaGreaterThan180::calculateX(CurveVariable *pVarToCalculate, 
													 vector<CurveVariableRelationship*> vecVRToExempt)
{
	vecVRToExempt.push_back(this);
	
	if (pVarToCalculate == m_pDeltaAngle)
	{
		throw UCLIDException("ELI02125", "Can't calculate delta angle from VRDeltaAngleToDeltaGreaterThan180");
	}
	else if (pVarToCalculate == m_pDeltaGT180)
	{
		return m_pDeltaAngle->getValue(vecVRToExempt) > 180;
	}
	else
	{
		throw UCLIDException("ELI02126", "No such variable defined in VRDeltaAngleToDeltaGreaterThan180");
	}
}
//----------------------------------------------------------------------------------------------
double VRDeltaAngleToDeltaGreaterThan180::calculateY(CurveVariable *pVarToCalculate, 
													 vector<CurveVariableRelationship*> vecVRToExempt)
{
	throw UCLIDException("ELI02124", "No Y value is available for this parameter");
}
//----------------------------------------------------------------------------------------------
bool VRDeltaAngleToDeltaGreaterThan180::canCalculateX(CurveVariable *pVarToCalculate, 
													  vector<CurveVariableRelationship*> vecVRToExempt)
{
	vecVRToExempt.push_back(this);
	
	if (pVarToCalculate == m_pDeltaAngle)
	{
		return false;
	}
	else if (pVarToCalculate == m_pDeltaGT180)
	{
		return m_pDeltaAngle->isCalculatable(vecVRToExempt);
	}
	else
	{
		throw UCLIDException("ELI02127", "Unknow variable caught in VRDeltaAngleToDeltaGreaterThan180 ");
	}
}
//----------------------------------------------------------------------------------------------
bool VRDeltaAngleToDeltaGreaterThan180::canCalculateY(CurveVariable *pVarToCalculate, 
													  vector<CurveVariableRelationship*> vecVRToExempt)
{
	throw UCLIDException("ELI02128", "Can calculate Y value in VRDeltaAngleToDeltaGreaterThan180");
}
//----------------------------------------------------------------------------------------------
bool VRDeltaAngleToDeltaGreaterThan180::isCalculatable(CurveVariable *pVarToCalculate,
													   vector<CurveVariableRelationship*> vecVRToExempt)
{
	vecVRToExempt.push_back(this);

	if (pVarToCalculate == m_pDeltaGT180)
	{	
		if (m_pDeltaAngle->isCalculatable(vecVRToExempt))
		{
			return true;
		}
	}
	
	return false;
}
