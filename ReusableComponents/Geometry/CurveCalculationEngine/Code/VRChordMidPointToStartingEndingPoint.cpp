//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE		:	VRChordMidPointToStartingEndingPoint.cpp
//
// PURPOSE	:	This is an implementation file for VRChordMidPointToStartingEndingPoint 
//				class. Where the VRChordMidPointToStartingEndingPoint class 
//				has been derived from CurveVariableRelationship class.The code 
//				written in this file makes it possible to implement functionality 
//				to add the variable to the respective variable relationship and 
//				to alculate the Curve variable
//
// NOTES	:	Nothing
//
// AUTHORS	:	G Chandra Sekhar Babu
//
//==================================================================================================
// VRChordMidPointToStartingEndingPoint.cpp : implementation file
//

#include "stdafx.h"
#include "VRChordMidPointToStartingEndingPoint.h"
#include "CurveVariable.h"

#include <math.h>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// VRChordMidPointToStartingEndingPoint

VRChordMidPointToStartingEndingPoint::VRChordMidPointToStartingEndingPoint
(
 CurveVariable *pArcChordMidPoint,CurveVariable *pArcStartingPoint,
 CurveVariable *pArcEndingPoint
 )
 :m_pArcChordMidPoint(pArcChordMidPoint),m_pArcStartingPoint(pArcStartingPoint),
 m_pArcEndingPoint(pArcEndingPoint)
{
	//adding the variables to this variable relationship
	addVariable(pArcChordMidPoint);
	addVariable(pArcStartingPoint);
	addVariable(pArcEndingPoint);
}
//==========================================================================================
double VRChordMidPointToStartingEndingPoint::calculateX(CurveVariable *pVarToCalculate,
														vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90100", "Memory not allocated for curve variable at calculateX() in VRChordMidPointToStartingEndingPoint");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Calculating the curve parameter with the respective equation 
	//and returning the same
	if(pVarToCalculate == m_pArcChordMidPoint)
	{
		return ((m_pArcStartingPoint->getPointParamXValue(vecVRToExempt)+
			m_pArcEndingPoint->getPointParamXValue(vecVRToExempt))/2.0);
	}
	else if(pVarToCalculate == m_pArcStartingPoint)
	{
		return ((2.0*m_pArcChordMidPoint->getPointParamXValue(vecVRToExempt))-
			(m_pArcEndingPoint->getPointParamXValue(vecVRToExempt)));
	}
	else if(pVarToCalculate == m_pArcEndingPoint)
	{
		return ((2.0*m_pArcChordMidPoint->getPointParamXValue(vecVRToExempt))-
			(m_pArcStartingPoint->getPointParamXValue(vecVRToExempt)));
	}
	else 
	{
		UCLIDException e("ELI90101", "This curve variable at calculateX() is not in VRChordMidPointToStartingEndingPoint variable relationship");
		throw e;
	}
}
//==========================================================================================
double VRChordMidPointToStartingEndingPoint::calculateY(CurveVariable *pVarToCalculate,
														vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90102", "Memory not allocated for curve variable at calculateY() in VRChordMidPointToStartingEndingPoint");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Calculating the curve parameter with the respective equation 
	//and returning the same
	if(pVarToCalculate == m_pArcChordMidPoint)
	{
		return ((m_pArcStartingPoint->getPointParamYValue(vecVRToExempt)
			+m_pArcEndingPoint->getPointParamYValue(vecVRToExempt))/2.0);
	}
	else if(pVarToCalculate == m_pArcStartingPoint)
	{
		return ((2.0*m_pArcChordMidPoint->getPointParamYValue(vecVRToExempt))-
			(m_pArcEndingPoint->getPointParamYValue(vecVRToExempt)));
	}
	else if(pVarToCalculate == m_pArcEndingPoint)
	{
		return ((2.0*m_pArcChordMidPoint->getPointParamYValue(vecVRToExempt))-
			(m_pArcStartingPoint->getPointParamYValue(vecVRToExempt)));
	}
	else 
	{
		UCLIDException e("ELI90103", "This curve variable at calculateY() is not in VRChordMidPointToStartingEndingPoint variable relationship");
		throw e;
	}
}
//==========================================================================================
bool VRChordMidPointToStartingEndingPoint::canCalculateX(CurveVariable *pVarToCalculate,
														 vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90104", "Memory not allocated for curve variable at calculateX() in VRChordMidPointToStartingEndingPoint");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Finding out whether the desired curve parameter can be calculable or not.
	//Checking whether all the dependent variables can be calculatable, if so 
	//return 'True' otherwise 'False'
	if(pVarToCalculate == m_pArcChordMidPoint)
	{
		return (m_pArcStartingPoint->canGetPointParamXValue(vecVRToExempt)
			&&m_pArcEndingPoint->canGetPointParamXValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcStartingPoint)
	{
		return (m_pArcChordMidPoint->canGetPointParamXValue(vecVRToExempt)
			&&m_pArcEndingPoint->canGetPointParamXValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcEndingPoint)
	{
		return (m_pArcChordMidPoint->canGetPointParamXValue(vecVRToExempt)
			&&m_pArcStartingPoint->canGetPointParamXValue(vecVRToExempt));
	}
	else 
	{
		UCLIDException e("ELI90105", "This curve variable at canCalculateX() is not in VRChordMidPointToStartingEndingPoint variable relationship");
		throw e;
	}
}
//==========================================================================================
bool VRChordMidPointToStartingEndingPoint::canCalculateY(CurveVariable *pVarToCalculate,
														 vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90106", "Memory not allocated for curve variable at calculateY() in VRChordMidPointToStartingEndingPoint");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Finding out whether the desired curve parameter can be calculable or not.
	//Checking whether all the dependent variables can be calculatable, if so 
	//return 'True' otherwise 'False'
	if(pVarToCalculate == m_pArcChordMidPoint)
	{
		return (m_pArcStartingPoint->canGetPointParamYValue(vecVRToExempt)
			&&m_pArcEndingPoint->canGetPointParamYValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcStartingPoint)
	{
		return (m_pArcChordMidPoint->canGetPointParamYValue(vecVRToExempt)
			&&m_pArcEndingPoint->canGetPointParamYValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcEndingPoint)
	{
		return (m_pArcChordMidPoint->canGetPointParamYValue(vecVRToExempt)
			&&m_pArcStartingPoint->canGetPointParamYValue(vecVRToExempt));
	}
	else 
	{
		UCLIDException e("ELI90107", "This curve variable at canCalculateY() is not in VRChordMidPointToStartingEndingPoint variable relationship");
		throw e;
	}
}
//==========================================================================================
