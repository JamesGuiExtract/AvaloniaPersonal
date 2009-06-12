//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE		:	VREndPointToStartingPointChordLengthChordBearing.cpp
//
// PURPOSE	:	This is an implementation file for 
//				VREndPointToStartingPointChordLengthChordBearing class. Where 
//				the VREndPointToStartingPointChordLengthChordBearing class has 
//				been derived from CurveVariableRelationship class.The code 
//				written in this file makes it possible to implement functionality 
//				to add the variable to the respective variable relationship and 
//				to calculate the Curve variable
//
// NOTES	:	Nothing
//
// AUTHORS	:	G Chandra Sekhar Babu
//
//==================================================================================================
// VREndPointToStartingPointChordLengthChordBearing.cpp : implementation file
//


#include "stdafx.h"
#include "VREndPointToStartingPointChordLengthChordBearing.h"
#include "CurveVariable.h"

#include <math.h>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// VREndPointToStartingPointChordLengthChordBearing

VREndPointToStartingPointChordLengthChordBearing::VREndPointToStartingPointChordLengthChordBearing
												  (
												  CurveVariable *pArcEndingPoint,CurveVariable *pArcStartingPoint,
												  CurveVariable *pArcChordLength,CurveVariable *pArcChordBearing
												  )
												  :m_pArcEndingPoint(pArcEndingPoint),m_pArcStartingPoint(pArcStartingPoint),
												   m_pArcChordLength(pArcChordLength),m_pArcChordBearing(pArcChordBearing)
{
	//adding the variables to this variable relationship
	addVariable(pArcEndingPoint);
	addVariable(pArcStartingPoint);
	addVariable(pArcChordLength);
	addVariable(pArcChordBearing);
}
//==========================================================================================
double VREndPointToStartingPointChordLengthChordBearing::calculateX(CurveVariable *pVarToCalculate,
																	vector<CurveVariableRelationship*> vecVRToExempt)
{
	//Curve Point parameter 'X' value 
	double xValue=0.0;	
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90138", "Memory not allocated for curve variable at calculateX() in VREndPointToStartingPointChordLengthChordBearing");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Calculating the curve parameter with the respective equation 
	//and returning the same
	if(pVarToCalculate == m_pArcEndingPoint)
	{
		xValue=(m_pArcStartingPoint->getPointParamXValue(vecVRToExempt))+
			((m_pArcChordLength->getValue(vecVRToExempt))*
			(cos(m_pArcChordBearing->getValue(vecVRToExempt)*(gdPI/180.0))));
	}
	else if(pVarToCalculate == m_pArcStartingPoint)
	{
		xValue= (m_pArcEndingPoint->getPointParamXValue(vecVRToExempt))-
			((m_pArcChordLength->getValue(vecVRToExempt))*
			(cos(m_pArcChordBearing->getValue(vecVRToExempt)*(gdPI/180.0))));
	}
	else if(pVarToCalculate == m_pArcChordLength)
	{
		double dXEnd = m_pArcEndingPoint->getPointParamXValue(vecVRToExempt);
		double dYEnd = m_pArcEndingPoint->getPointParamYValue(vecVRToExempt);
		double dXStart = m_pArcStartingPoint->getPointParamXValue(vecVRToExempt);
		double dYStart = m_pArcStartingPoint->getPointParamYValue(vecVRToExempt);

		// chord length can be calculated simply by using start and end points
		xValue= sqrt( pow((dXEnd - dXStart), 2) + pow((dYEnd - dYStart), 2) );
	}
	else if(pVarToCalculate == m_pArcChordBearing)
	{
		double dXEnd = m_pArcEndingPoint->getPointParamXValue(vecVRToExempt);
		double dYEnd = m_pArcEndingPoint->getPointParamYValue(vecVRToExempt);
		double dXStart = m_pArcStartingPoint->getPointParamXValue(vecVRToExempt);
		double dYStart = m_pArcStartingPoint->getPointParamYValue(vecVRToExempt);
		double dChordLength = sqrt( pow((dXEnd - dXStart), 2) + pow((dYEnd - dYStart), 2) );

		double dCosValue = (dXEnd - dXStart)/dChordLength;
		checkAndSetSinCosValue(dCosValue, "ELI05627");
		
		xValue = acos(dCosValue)*(180/gdPI);

		// since acos only gives angle in either first or second quadrant,
		// let's get the actual value for the angle according to the start 
		// and end points positions

		// If the angle is actually belong to third or fourth quadrant
		if ( ( (dXEnd < dXStart) && (dYEnd < dYStart) )			// third quadrant ( between 180° and 270°)
			|| ( (dXEnd == dXStart) && (dYEnd < dYStart) ) 		// == 270°
			|| ( (dXEnd > dXStart) && (dYEnd < dYStart) ) )		// Or fourth quadrant ( between 270° and 360°)
		{
			xValue = 360.0 - xValue;
		}
	}
	else 
	{
		UCLIDException e("ELI90139", "This curve variable at calculateX() is not in VREndPointToStartingPointChordLengthChordBearing variable relationship");
		throw e;
	}
	CurveVariable var;
	var.m_pszBuffer = (char*) malloc(_CVTBUFSIZE);
	int iResult =_ecvt_s( var.m_pszBuffer, _CVTBUFSIZE, xValue, 6, 
		&var.m_iDecimal, &var.m_iSign );
	if (var.m_iDecimal < -6)
	{
		xValue=0.0;
	}
	return xValue;
}
//==========================================================================================
double VREndPointToStartingPointChordLengthChordBearing::calculateY(CurveVariable *pVarToCalculate,
																	vector<CurveVariableRelationship*> vecVRToExempt)
{
	//Curve Point parameter 'Y' value 
	double yValue=0.0;
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90140", "Memory not allocated for curve variable at calculateY() in VREndPointToStartingPointChordLengthChordBearing");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Calculating the curve parameter with the respective equation 
	//and returning the same
	if(pVarToCalculate == m_pArcEndingPoint)
	{
		yValue=(m_pArcStartingPoint->getPointParamYValue(vecVRToExempt))+
			((m_pArcChordLength->getValue(vecVRToExempt))*
			(sin(m_pArcChordBearing->getValue(vecVRToExempt)*(gdPI/180.0))));
	}
	else if(pVarToCalculate == m_pArcStartingPoint)
	{
		yValue= (m_pArcEndingPoint->getPointParamYValue(vecVRToExempt))-
			((m_pArcChordLength->getValue(vecVRToExempt))*
			(sin(m_pArcChordBearing->getValue(vecVRToExempt)*(gdPI/180.0))));
	}
	else if(pVarToCalculate == m_pArcChordLength)
	{
		double dXEnd = m_pArcEndingPoint->getPointParamXValue(vecVRToExempt);
		double dYEnd = m_pArcEndingPoint->getPointParamYValue(vecVRToExempt);
		double dXStart = m_pArcStartingPoint->getPointParamXValue(vecVRToExempt);
		double dYStart = m_pArcStartingPoint->getPointParamYValue(vecVRToExempt);

		yValue= sqrt( pow((dXEnd - dXStart), 2) + pow((dYEnd - dYStart), 2) );
	}
	else if(pVarToCalculate == m_pArcChordBearing)
	{
		double dXEnd = m_pArcEndingPoint->getPointParamXValue(vecVRToExempt);
		double dYEnd = m_pArcEndingPoint->getPointParamYValue(vecVRToExempt);
		double dXStart = m_pArcStartingPoint->getPointParamXValue(vecVRToExempt);
		double dYStart = m_pArcStartingPoint->getPointParamYValue(vecVRToExempt);
		double dChordLength = sqrt( pow((dXEnd - dXStart), 2) + pow((dYEnd - dYStart), 2) );

		double dSinValue = (dYEnd - dYStart)/dChordLength;
		checkAndSetSinCosValue(dSinValue, "ELI05619");

		yValue= asin(dSinValue)*(180/gdPI);

		// since asin only gives angle in either first or fourth quadrant,
		// let's get the actual value for the angle according to the start 
		// and end points positions

		// If the angle is actually belong to third or fourth quadrant
		if ( ( (dXEnd < dXStart) && (dYEnd > dYStart) )			// second quadrant ( between 90° and 180°)
			|| ( (dXEnd < dXStart) && (dYEnd == dYStart) )		// == 180°
			|| ( (dXEnd < dXStart) && (dYEnd < dYStart) ) )		// Or third quadrant ( between 180° and 270°)
		{
			yValue = 180.0 - yValue;
		}
	}	
	else 
	{
		UCLIDException e("ELI90141", "This curve variable at calculateY() is not in VREndPointToStartingPointChordLengthChordBearing variable relationship");
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
bool VREndPointToStartingPointChordLengthChordBearing::canCalculateX(CurveVariable *pVarToCalculate,
																	 vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90142", "Memory not allocated for curve variable at calculateX() in VREndPointToStartingPointChordLengthChordBearing");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Finding out whether the desired curve parameter can be calculable or not.
	//Checking whether all the dependent variables can be calculatable, if so 
	//return 'True' otherwise 'False'
	if(pVarToCalculate == m_pArcEndingPoint)
	{
		return (m_pArcStartingPoint->canGetPointParamXValue(vecVRToExempt)
			&&m_pArcChordLength->canGetValue(vecVRToExempt)
			&&m_pArcChordBearing->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcStartingPoint)
	{
		return (m_pArcEndingPoint->canGetPointParamXValue(vecVRToExempt)
			&&m_pArcChordLength->canGetValue(vecVRToExempt)
			&&m_pArcChordBearing->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcChordLength)
	{
		return (m_pArcEndingPoint->canGetPointParamXValue(vecVRToExempt)
			&&m_pArcStartingPoint->canGetPointParamXValue(vecVRToExempt) );
	}
	else if(pVarToCalculate == m_pArcChordBearing)
	{
		return (m_pArcEndingPoint->canGetPointParamXValue(vecVRToExempt)
			&&m_pArcStartingPoint->canGetPointParamXValue(vecVRToExempt));
	}	
	else 
	{
		UCLIDException e("ELI90143", "This curve variable at canCalculateX() is not in VREndPointToStartingPointChordLengthChordBearing variable relationship");
		throw e;
	}
}
//==========================================================================================
bool VREndPointToStartingPointChordLengthChordBearing::canCalculateY(CurveVariable *pVarToCalculate,
																	 vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI90144", "Memory not allocated for curve variable at calculateY() in VREndPointToStartingPointChordLengthChordBearing");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Finding out whether the desired curve parameter can be calculable or not.
	//Checking whether all the dependent variables can be calculatable, if so 
	//return 'True' otherwise 'False'
	if(pVarToCalculate == m_pArcEndingPoint)
	{
		return (m_pArcStartingPoint->canGetPointParamYValue(vecVRToExempt)
			&&m_pArcChordLength->canGetValue(vecVRToExempt)
			&&m_pArcChordBearing->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcStartingPoint)
	{
		return (m_pArcEndingPoint->canGetPointParamYValue(vecVRToExempt)
			&&m_pArcChordLength->canGetValue(vecVRToExempt)
			&&m_pArcChordBearing->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcChordLength)
	{
		// if you can get start and end points, chord length can be calculated
		bool bCanGetStartPoint = m_pArcStartingPoint->canGetPointParamYValue(vecVRToExempt);
		bool bCanGetEndPoint = m_pArcEndingPoint->canGetPointParamYValue(vecVRToExempt);

		return (bCanGetStartPoint && bCanGetEndPoint);
	}
	else if(pVarToCalculate == m_pArcChordBearing)
	{
		return (m_pArcEndingPoint->canGetPointParamYValue(vecVRToExempt)
			&&m_pArcStartingPoint->canGetPointParamYValue(vecVRToExempt));
	}
	else 
	{
		UCLIDException e("ELI90145", "This curve variable at canCalculateY() is not in VREndPointToStartingPointChordLengthChordBearing variable relationship");
		throw e;
	}
}
//==========================================================================================
bool VREndPointToStartingPointChordLengthChordBearing::isCalculatable(CurveVariable *pVarToCalculate,
																	  vector<CurveVariableRelationship*> vecVRToExempt)
{
	// if start and end points are calculatable, so are chord length and chord bearing
	if (pVarToCalculate == m_pArcChordLength || pVarToCalculate == m_pArcChordBearing)
	{
		// add this variable relationship to the list of VR's exempt from being used
		// in any calculations for any call on the stack from here down.
		vecVRToExempt.push_back(this);
		
		if(!m_pArcStartingPoint->isCalculatable(vecVRToExempt) 
			|| !m_pArcEndingPoint->isCalculatable(vecVRToExempt) )
		{
			return false;
		}
		
		return true;
	}

	return CurveVariableRelationship::isCalculatable(pVarToCalculate, vecVRToExempt);
}
