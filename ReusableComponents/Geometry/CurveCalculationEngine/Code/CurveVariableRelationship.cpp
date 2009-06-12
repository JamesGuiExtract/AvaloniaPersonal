//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE		:	CurveVariableRelationship.cpp
//
// PURPOSE	:	This is an implementation file for CurveVariableRelationship 
//				class.The code written in this file makes it possible to 
//				implement functionality to add the variable to the respective 
//				variable relation ship and to find out whether a variable is 
//				calculatable or not
//
// NOTES	:	Nothing
//
// AUTHORS	:	G Chandra Sekhar Babu
//
//==================================================================================================
// CurveVariableRelationship.cpp : implementation file
//
#include "stdafx.h"
#include "CurveVariableRelationship.h"
#include "CurveVariable.h"

#include <mathUtil.h>

#include <math.h>
#include <algorithm>

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

using namespace std;

//----------------------------------------------------------------------------------------------
// CurveVariableRelationship
//----------------------------------------------------------------------------------------------
void CurveVariableRelationship::addVariable(CurveVariable *pVar)
{
	//adding the variable to the vector
	m_vecVars.push_back(pVar);
	pVar->addVariableRelationship(this);
}
//----------------------------------------------------------------------------------------------
bool CurveVariableRelationship::isCalculatable(CurveVariable *pVarToCalculate,
											   vector<CurveVariableRelationship*> vecVRToExempt)
{
	// add this variable relationship to the list of VR's exempt from being used
	// in any calculations for any call on the stack from here down.
	vecVRToExempt.push_back(this);

	//Create a vector of dependent variables by removing the variable we want 
	//to calculate from the list of variables in this relationship
	vector<CurveVariable *> vecDependentVars=m_vecVars;
	vecDependentVars.erase(find(vecDependentVars.begin(),vecDependentVars.end(),
								pVarToCalculate));
	vector<CurveVariable *>::const_iterator iter;
	for(iter=vecDependentVars.begin();iter!=vecDependentVars.end();iter++)
	{
		
		if(!(*iter)->isCalculatable(vecVRToExempt))
		{
			return false;
		}
	}

	return true;
}
//----------------------------------------------------------------------------------------------
void CurveVariableRelationship::checkAndSetSinCosValue(double &dSinCosValue, const string& strELICode)
{
	if (dSinCosValue > 1.0 || dSinCosValue < -1.0) 
	{
		if (fabs(dSinCosValue)-1.0 > MathVars::ZERO)
		{
			UCLIDException ue(strELICode, "Invalid Sine/Cosine value.");
			ue.addDebugInfo("Sine/Cosine value", dSinCosValue);
			throw ue;
		}
		
		dSinCosValue = dSinCosValue > 0.0 ? 1 : -1;
	}
}
