//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE		:	VRRadiusToMidOrdinateChordLength.cpp
//
// PURPOSE	:	This is an implementation file for VRRadiusToMidOrdinateChordLength class.
//				Where the VRRadiusToMidOrdinateChordLength class has been derived from 
//				CurveVariableRelationship class.The code written in this file makes 
//				it possible to implement functionality to add the variable to the 
//				respective variable relationship and calculate the Curve variable
//
// NOTES	:	Nothing
//
// AUTHORS	:	Duan Wang
//
//==================================================================================================
// VRRadiusToMidOrdinateChordLength.cpp : implementation file
//


#include "stdafx.h"

#include "VRRadiusToMidOrdinateChordLength.h"
#include "CurveVariable.h"

#include <math.h>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// VRRadiusToMidOrdinateChordLength

VRRadiusToMidOrdinateChordLength::VRRadiusToMidOrdinateChordLength
							   (
								CurveVariable *pArcMiddleOrdinate,
								CurveVariable *pArcRadius,
								CurveVariable *pArcChordLength,
								CurveVariable *pDeltaGT180
							   )
:m_pArcMiddleOrdinate(pArcMiddleOrdinate),
 m_pArcRadius(pArcRadius),
 m_pArcChordLength(pArcChordLength),
 m_pDeltaGT180(pDeltaGT180)
{
	//adding the variables to this variable relationship
	addVariable(pArcMiddleOrdinate);
	addVariable(pArcRadius);
	addVariable(pArcChordLength);
	addVariable(pDeltaGT180);
}
//==========================================================================================
double VRRadiusToMidOrdinateChordLength::calculateX(CurveVariable *pVarToCalculate,
												 vector<CurveVariableRelationship*> vecVRToExempt)
{
	//Curve Point parameter value 
	double dValue=0.0;
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI01559", "Memory not allocated for curve variable at calculateX() in VRRadiusToMidOrdinateChordLength");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Calculating the curve parameter with the respective equation 
	//and returning the same
	if (pVarToCalculate == m_pArcRadius)
	{
		double dMidOrdinate = m_pArcMiddleOrdinate->getValue(vecVRToExempt);
		
		double dChordLength = m_pArcChordLength->getValue(vecVRToExempt);
		
		dValue = (pow(dMidOrdinate, 2) + pow(dChordLength/2, 2))/(2 * dMidOrdinate);
	}
	else if (pVarToCalculate == m_pArcChordLength)
	{
		double dRadius = m_pArcRadius->getValue(vecVRToExempt);
		// use radius, delta to get chord length
		double dMidOrdinate = m_pArcMiddleOrdinate->getValue(vecVRToExempt);
		
		if (2*dRadius > dMidOrdinate)
		{
			dValue = 2 * sqrt( 2 * dRadius * dMidOrdinate - pow(dMidOrdinate, 2));
		}
		else
		{
			dValue = 0.0;
		}
	}
	else if (pVarToCalculate == m_pArcMiddleOrdinate)
	{
		double dRadius = m_pArcRadius->getValue(vecVRToExempt);
		double dChordLength = m_pArcChordLength->getValue(vecVRToExempt);
		bool bDeltaGT180 = m_pDeltaGT180->getBooleanValue(vecVRToExempt);

		if (!bDeltaGT180)
		{
			dValue = dRadius - sqrt( pow(dRadius, 2) - pow((dChordLength/2), 2) );
		}
		else
		{
			dValue = dRadius + sqrt( pow(dRadius, 2) - pow((dChordLength/2), 2) );
		}
	}
	else 
	{
		UCLIDException e("ELI01560", "This curve variable at calculateX() is not in VRRadiusToMidOrdinateChordLength variable relationship");
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
double VRRadiusToMidOrdinateChordLength::calculateY(CurveVariable *pVarToCalculate,
												 vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI01561", "Memory not allocated for curve variable at calculateY() in VRRadiusToMidOrdinateChordLength");
		throw e;
	}
	return 0.0;
}
//==========================================================================================
bool VRRadiusToMidOrdinateChordLength::canCalculateX(CurveVariable *pVarToCalculate,
												  vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI01562", "Memory not allocated for curve variable at calculateX() in VRRadiusToMidOrdinateChordLength");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Finding out whether the desired curve parameter can be calculable or not.
	//Checking whether all the dependent variables can be calculatable, if so 
	//return 'True' otherwise 'False'
	if (pVarToCalculate == m_pDeltaGT180)
	{
		return false;
	}
	else if (pVarToCalculate == m_pArcMiddleOrdinate)
	{
		return ( m_pArcRadius->canGetValue(vecVRToExempt) 
			&& m_pArcChordLength->canGetPointParamXValue(vecVRToExempt)
			&& m_pDeltaGT180->canGetValue(vecVRToExempt));
	}
	else if (pVarToCalculate == m_pArcRadius)
	{
		return (m_pArcMiddleOrdinate->canGetValue(vecVRToExempt)
			&& m_pArcChordLength->canGetPointParamXValue(vecVRToExempt));
	}
	else if (pVarToCalculate == m_pArcChordLength)
	{
		return (m_pArcRadius->canGetValue(vecVRToExempt)
			&& m_pArcMiddleOrdinate->canGetValue(vecVRToExempt));
	}
	else 
	{
		UCLIDException e("ELI01563", "This curve variable at canCalculateX() is not in VRRadiusToMidOrdinateChordLength variable relationship");
		throw e;
	}
}
//==========================================================================================
bool VRRadiusToMidOrdinateChordLength::canCalculateY(CurveVariable *pVarToCalculate,
												  vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI01564", "Memory not allocated for curve variable at calculateY() in VRRadiusToMidOrdinateChordLength");
		throw e;
	}
	return false;
}
//==========================================================================================
bool VRRadiusToMidOrdinateChordLength::isCalculatable(CurveVariable *pVarToCalculate, 
													  vector<CurveVariableRelationship*> vecVRToExempt)
{
	if (pVarToCalculate == m_pDeltaGT180)
	{
		return false;
	}

	return CurveVariableRelationship::isCalculatable(pVarToCalculate, vecVRToExempt);
}