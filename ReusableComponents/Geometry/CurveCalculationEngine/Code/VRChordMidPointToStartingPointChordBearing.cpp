//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE		:	VRChordMidPointToStartingPointChordBearing.cpp
//
// PURPOSE	:	This is an implementation file for VRChordMidPointToStartingPointChordBearing 
//				class. Where the VRChordMidPointToStartingPointChordBearing class 
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
// VRChordMidPointToStartingPointChordBearing.cpp : implementation file
//


#include "stdafx.h"
#include "VRChordMidPointToStartingPointChordBearing.h"
#include "CurveVariable.h"

#include <math.h>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// VRChordMidPointToStartingPointChordBearing

VRChordMidPointToStartingPointChordBearing::VRChordMidPointToStartingPointChordBearing
									  (
									  CurveVariable *pArcChordMidPoint,CurveVariable *pArcStartingPoint,
									  CurveVariable *pArcChordLength, CurveVariable *pArcChordBearing
									  )
									  :m_pArcChordMidPoint(pArcChordMidPoint),m_pArcStartingPoint(pArcStartingPoint),
									   m_pArcChordLength(pArcChordLength),m_pArcChordBearing(pArcChordBearing)
{
	//adding the variables to this variable relationship
	addVariable(pArcChordMidPoint);
	addVariable(pArcStartingPoint);
	addVariable(pArcChordLength);
	addVariable(pArcChordBearing);
}
//==========================================================================================
double VRChordMidPointToStartingPointChordBearing::calculateX(CurveVariable *pVarToCalculate,
															  vector<CurveVariableRelationship*> vecVRToExempt)
{
	//Curve Point parameter 'X' value 
	double xValue=0.0;	
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90255", "Memory not allocated for curve variable at calculateX() in VRChordMidPointToStartingPointChordBearing");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Calculating the curve parameter with the respective equation 
	//and returning the same
	if(pVarToCalculate == m_pArcChordMidPoint)
	{
		double dXStart = m_pArcStartingPoint->getPointParamXValue(vecVRToExempt);
		double dChordLength = m_pArcChordLength->getValue(vecVRToExempt);
		double dChordBearing = m_pArcChordBearing->getValue(vecVRToExempt);
		
		xValue = dXStart + (dChordLength/2) * cos(dChordBearing * gdPI / 180.0);
	}
	else if(pVarToCalculate == m_pArcStartingPoint)
	{
		double dXMidOfChord = m_pArcChordMidPoint->getPointParamXValue(vecVRToExempt);
		double dChordLength = m_pArcChordLength->getValue(vecVRToExempt);
		double dChordBearing = m_pArcChordBearing->getValue(vecVRToExempt);
		
		xValue = dXMidOfChord - (dChordLength/2) * cos(dChordBearing * gdPI / 180.0);
	}
	else if(pVarToCalculate == m_pArcChordLength)
	{
		double dXMidOfChord = m_pArcChordMidPoint->getPointParamXValue(vecVRToExempt);
		double dYMidOfChord = m_pArcChordMidPoint->getPointParamYValue(vecVRToExempt);
		double dXStart = m_pArcStartingPoint->getPointParamXValue(vecVRToExempt);
		double dYStart = m_pArcStartingPoint->getPointParamYValue(vecVRToExempt);

		xValue = 2 * sqrt( pow((dXMidOfChord - dXStart), 2) + pow((dYMidOfChord - dYStart), 2) );
	}
	else if(pVarToCalculate == m_pArcChordBearing)
	{
		double dXMidOfChord = m_pArcChordMidPoint->getPointParamXValue(vecVRToExempt);
		double dYMidOfChord = m_pArcChordMidPoint->getPointParamYValue(vecVRToExempt);
		double dXStart = m_pArcStartingPoint->getPointParamXValue(vecVRToExempt);
		double dYStart = m_pArcStartingPoint->getPointParamYValue(vecVRToExempt);
		double dChordLength = 2 * sqrt( pow((dXMidOfChord - dXStart), 2) + pow((dYMidOfChord - dYStart), 2) );
		
		double dCosValue = (dXMidOfChord - dXStart) * 2 /dChordLength;
		checkAndSetSinCosValue(dCosValue, "ELI05626");

		xValue =  acos(dCosValue) * (180/gdPI);

		// since acos only gives angle in either first or second quadrant,
		// let's get the actual value for the angle according to the start 
		// and mid of chord points positions

		// If the angle is actually belong to third or fourth quadrant
		if ( ( (dXMidOfChord < dXStart) && (dYMidOfChord < dYStart) )			// third quadrant ( between 180° and 270°)
			|| ( (dXMidOfChord == dXStart) && (dYMidOfChord < dYStart) ) 		// should be 270°
			|| ( (dXMidOfChord > dXStart) && (dYMidOfChord < dYStart) )	)	// Or fourth quadrant ( between 270° and 360°)
		{
			xValue = 360.0 - xValue;
		}
	}
	else 
	{
		UCLIDException e("ELI90256", "This curve variable at calculateX() is not in VRChordMidPointToStartingPointChordBearing variable relationship");
		throw e;
	}
	CurveVariable var;
	var.m_pszBuffer = (char*) malloc(_CVTBUFSIZE);
	int iResult = _ecvt_s( var.m_pszBuffer, _CVTBUFSIZE, xValue, 6, 
		&var.m_iDecimal, &var.m_iSign );
	if (var.m_iDecimal < -6)
	{
		xValue=0.0;
	}
	return xValue;
}
//==========================================================================================
double VRChordMidPointToStartingPointChordBearing::calculateY(CurveVariable *pVarToCalculate,
															  vector<CurveVariableRelationship*> vecVRToExempt)
{
	//Curve Point parameter 'Y' value 
	double yValue=0.0;
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90257", "Memory not allocated for curve variable at calculateY() in VRChordMidPointToStartingPointChordBearing");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Calculating the curve parameter with the respective equation 
	//and returning the same
	if(pVarToCalculate == m_pArcChordMidPoint)
	{
		double dYStart = m_pArcStartingPoint->getPointParamYValue(vecVRToExempt);
		double dChordLength = m_pArcChordLength->getValue(vecVRToExempt);
		double dChordBearing = m_pArcChordBearing->getValue(vecVRToExempt);
		
		yValue = dYStart + (dChordLength/2) * sin(dChordBearing * gdPI / 180.0);
	}
	else if(pVarToCalculate == m_pArcStartingPoint)
	{
		double dYMidOfChord = m_pArcChordMidPoint->getPointParamYValue(vecVRToExempt);
		double dChordLength = m_pArcChordLength->getValue(vecVRToExempt);
		double dChordBearing = m_pArcChordBearing->getValue(vecVRToExempt);
		
		yValue = dYMidOfChord - (dChordLength/2) * sin(dChordBearing * gdPI / 180.0);
	}
	else if(pVarToCalculate == m_pArcChordLength)
	{
		double dXMidOfChord = m_pArcChordMidPoint->getPointParamXValue(vecVRToExempt);
		double dYMidOfChord = m_pArcChordMidPoint->getPointParamYValue(vecVRToExempt);
		double dXStart = m_pArcStartingPoint->getPointParamXValue(vecVRToExempt);
		double dYStart = m_pArcStartingPoint->getPointParamYValue(vecVRToExempt);

		yValue = 2 * sqrt( pow((dXMidOfChord - dXStart), 2) + pow((dYMidOfChord - dYStart), 2) );
	}
	else if(pVarToCalculate == m_pArcChordBearing)
	{
		double dXMidOfChord = m_pArcChordMidPoint->getPointParamXValue(vecVRToExempt);
		double dYMidOfChord = m_pArcChordMidPoint->getPointParamYValue(vecVRToExempt);
		double dXStart = m_pArcStartingPoint->getPointParamXValue(vecVRToExempt);
		double dYStart = m_pArcStartingPoint->getPointParamYValue(vecVRToExempt);
		double dChordLength = 2 * sqrt( pow((dXMidOfChord - dXStart), 2) + pow((dYMidOfChord - dYStart), 2) );
		
		double dSinValue = (dYMidOfChord - dYStart) * 2 /dChordLength;
		checkAndSetSinCosValue(dSinValue, "ELI05617");

		yValue= asin(dSinValue) * (180/gdPI);

		// since asin only gives angle in either first or fourth quadrant,
		// let's get the actual value for the angle according to the start 
		// and mid of chord points positions

		// If the angle is actually belong to second or third quadrant
		if ( ( (dXMidOfChord < dXStart) && (dYMidOfChord > dYStart) )			// second quadrant ( between 90° and 180°)
			|| ( (dXMidOfChord < dXStart) && (dYMidOfChord == dYStart) )		// == 180°
			|| ( (dXMidOfChord < dXStart) && (dYMidOfChord < dYStart) )	)	// Or third quadrant ( between 180° and 270°)
		{
			yValue = 180.0 - yValue;
		}
	}
	else 
	{
		UCLIDException e("ELI90258", "This curve variable at calculateY() is not in VRChordMidPointToStartingPointChordBearing variable relationship");
		throw e;
	}
	CurveVariable var;
	var.m_pszBuffer = (char*) malloc(_CVTBUFSIZE);
	int iResult =_ecvt_s( var.m_pszBuffer, _CVTBUFSIZE, yValue, 6, 
		&var.m_iDecimal, &var.m_iSign );
	if (var.m_iDecimal < -6)
	{
		yValue=0.0;
	}
	return yValue;
}
//==========================================================================================
bool VRChordMidPointToStartingPointChordBearing::canCalculateX(CurveVariable *pVarToCalculate,
															   vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90259", "Memory not allocated for curve variable at calculateX() in VRChordMidPointToStartingPointChordBearing");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Finding out whether the desired curve parameter can be calculable or not.
	//Checking whether all the dependent variables can be calculatable, if so 
	//return 'True' otherwise 'False'
	if(pVarToCalculate == m_pArcChordMidPoint)
	{
		return (m_pArcStartingPoint->canGetPointParamXValue(vecVRToExempt)
			&&m_pArcChordLength->canGetValue(vecVRToExempt)
			&&m_pArcChordBearing->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcStartingPoint)
	{
		return (m_pArcChordMidPoint->canGetPointParamXValue(vecVRToExempt)
			&&m_pArcChordLength->canGetValue(vecVRToExempt)
			&&m_pArcChordBearing->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcChordLength)
	{
		return (m_pArcChordMidPoint->canGetPointParamXValue(vecVRToExempt)
			&&m_pArcStartingPoint->canGetPointParamXValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcChordBearing)
	{
		return (m_pArcChordMidPoint->canGetPointParamXValue(vecVRToExempt)
			&&m_pArcStartingPoint->canGetPointParamXValue(vecVRToExempt));
	}	
	else 
	{
		UCLIDException e("ELI90260", "This curve variable at canCalculateX() is not in VRChordMidPointToStartingPointChordBearing variable relationship");
		throw e;
	}
}
//==========================================================================================
bool VRChordMidPointToStartingPointChordBearing::canCalculateY(CurveVariable *pVarToCalculate,
															   vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90261", "Memory not allocated for curve variable at calculateY() in VRChordMidPointToStartingPointChordBearing");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Finding out whether the desired curve parameter can be calculable or not.
	//Checking whether all the dependent variables can be calculatable, if so 
	//return 'True' otherwise 'False'
	if(pVarToCalculate == m_pArcChordMidPoint)
	{
		return (m_pArcStartingPoint->canGetPointParamYValue(vecVRToExempt)
			&&m_pArcChordLength->canGetValue(vecVRToExempt)
			&&m_pArcChordBearing->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcStartingPoint)
	{
		return (m_pArcChordMidPoint->canGetPointParamYValue(vecVRToExempt)
			&&m_pArcChordLength->canGetValue(vecVRToExempt)
			&&m_pArcChordBearing->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcChordLength)
	{
		return (m_pArcChordMidPoint->canGetPointParamYValue(vecVRToExempt)
			&&m_pArcStartingPoint->canGetPointParamYValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcChordBearing)
	{
		return (m_pArcChordMidPoint->canGetPointParamYValue(vecVRToExempt)
			&&m_pArcStartingPoint->canGetPointParamYValue(vecVRToExempt));
	}	
	else 
	{
		UCLIDException e("ELI90262", "This curve variable at canCalculateY() is not in VRChordMidPointToStartingPointChordBearing variable relationship");
		throw e;
	}
}
//==========================================================================================
bool VRChordMidPointToStartingPointChordBearing::isCalculatable(CurveVariable *pVarToCalculate,
													  vector<CurveVariableRelationship*> vecVRToExempt)
{
	// if start and center points are calculatable, so are chord length and chord bearing
	if (pVarToCalculate == m_pArcChordLength || pVarToCalculate == m_pArcChordBearing)
	{
		// add this variable relationship to the list of VR's exempt from being used
		// in any calculations for any call on the stack from here down.
		vecVRToExempt.push_back(this);
		
		if(!m_pArcChordMidPoint->isCalculatable(vecVRToExempt) 
			|| !m_pArcStartingPoint->isCalculatable(vecVRToExempt) )
		{
			return false;
		}
		
		return true;
	}

	return CurveVariableRelationship::isCalculatable(pVarToCalculate, vecVRToExempt);
}
