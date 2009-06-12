//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE		:	VRMiddleToChordMidPointMidOrdinateChordBearing.cpp
//
// PURPOSE	:	This is an implementation file for VRMiddleToChordMidPointMidOrdinateChordBearing class.
//				Where the VRMiddleToChordMidPointMidOrdinateChordBearing class has been derived from 
//				CurveVariableRelationship class.The code written in this file makes 
//				it possible to implement functionality to add the variable to the 
//				respective variable relationship and to calculate the Curve variable
//
// NOTES	:	Nothing
//
// AUTHORS	:	Duan Wang
//
//==================================================================================================
// VRMiddleToChordMidPointMidOrdinateChordBearing.cpp : implementation file
//

#include "stdafx.h"
#include "VRMiddleToChordMidPointMidOrdinateChordBearing.h"
#include "CurveVariable.h"

#include <math.h>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// VRMiddleToChordMidPointMidOrdinateChordBearing

VRMiddleToChordMidPointMidOrdinateChordBearing::VRMiddleToChordMidPointMidOrdinateChordBearing
												(CurveVariable *pArcConcaveLeft,
												 CurveVariable *pArcMidPoint, 
												 CurveVariable *pArcChordMidPoint,
												 CurveVariable *pArcMiddleOrdinate,
												 CurveVariable *pArcChordBearing)
:m_pArcConcaveLeft(pArcConcaveLeft),m_pArcMidPoint(pArcMidPoint),m_pArcChordMidPoint(pArcChordMidPoint),
 m_pArcMiddleOrdinate(pArcMiddleOrdinate), m_pArcChordBearing(pArcChordBearing)
{
	//adding the variables to this variable relationship
	addVariable(pArcConcaveLeft);		
	addVariable(pArcMidPoint);
	addVariable(pArcChordMidPoint);
	addVariable(pArcMiddleOrdinate);
	addVariable(pArcChordBearing);
}
//==========================================================================================
double VRMiddleToChordMidPointMidOrdinateChordBearing::calculateX(CurveVariable *pVarToCalculate,
												vector<CurveVariableRelationship*> vecVRToExempt)
{
	//Curve Point parameter 'X' value 
	double xValue=0.0;
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI01536", "Memory not allocated for curve variable at calculateX() in VRMiddleToChordMidPointMidOrdinateChordBearing");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Calculating the curve parameter with the respective equation 
	//and returning the same
	if(pVarToCalculate == m_pArcMidPoint)
	{
		bool bLeft = m_pArcConcaveLeft->getBooleanValue(vecVRToExempt);
		double dMidOrdinate = m_pArcMiddleOrdinate->getValue(vecVRToExempt);
		// chord bearing in degrees
		double dChordBearing = m_pArcChordBearing->getValue(vecVRToExempt);
		double dXMidOfChord = m_pArcChordMidPoint->getPointParamXValue(vecVRToExempt);

		if (bLeft)
		{
			xValue = dXMidOfChord + dMidOrdinate * sin(dChordBearing * gdPI/180.0);
		}
		else
		{
			xValue = dXMidOfChord - dMidOrdinate * sin(dChordBearing * gdPI/180.0);
		}
	}
	else if(pVarToCalculate == m_pArcChordMidPoint)
	{
		bool bLeft = m_pArcConcaveLeft->getBooleanValue(vecVRToExempt);
		double dMidOrdinate = m_pArcMiddleOrdinate->getValue(vecVRToExempt);
		// chord bearing in degrees
		double dChordBearing = m_pArcChordBearing->getValue(vecVRToExempt);
		double dXMidOfArc = m_pArcMidPoint->getPointParamXValue(vecVRToExempt);

		if (bLeft)
		{
			xValue = dXMidOfArc - dMidOrdinate * sin(dChordBearing * gdPI/180.0);
		}
		else
		{
			xValue = dXMidOfArc + dMidOrdinate * sin(dChordBearing * gdPI/180.0);
		}
	}
	else if(pVarToCalculate == m_pArcMiddleOrdinate)
	{
		double dXMidOfArc = m_pArcMidPoint->getPointParamXValue(vecVRToExempt);
		double dYMidOfArc = m_pArcMidPoint->getPointParamYValue(vecVRToExempt);
		double dXMidOfChord = m_pArcChordMidPoint->getPointParamXValue(vecVRToExempt);
		double dYMidOfChord = m_pArcChordMidPoint->getPointParamYValue(vecVRToExempt);

		xValue= sqrt( pow((dXMidOfArc - dXMidOfChord), 2) + pow((dYMidOfArc - dYMidOfChord), 2) );
	}
	else if(pVarToCalculate == m_pArcChordBearing)
	{
		bool bLeft = m_pArcConcaveLeft->getBooleanValue(vecVRToExempt);
		double dXMidOfArc = m_pArcMidPoint->getPointParamXValue(vecVRToExempt);
		double dYMidOfArc = m_pArcMidPoint->getPointParamYValue(vecVRToExempt);
		double dXMidOfChord = m_pArcChordMidPoint->getPointParamXValue(vecVRToExempt);
		double dYMidOfChord = m_pArcChordMidPoint->getPointParamYValue(vecVRToExempt);
		double dMidOrdinate = sqrt( pow((dXMidOfArc - dXMidOfChord), 2) + pow((dYMidOfArc - dYMidOfChord), 2) );
		
		double dSinInput=0.0;
		if (bLeft)
		{
			dSinInput = (dXMidOfArc - dXMidOfChord) / dMidOrdinate;
		}
		else
		{
			dSinInput = (dXMidOfChord - dXMidOfArc) / dMidOrdinate;
		}

		checkAndSetSinCosValue(dSinInput, "ELI05623");

		// get the angle in degrees
		xValue= asin(dSinInput) * 180/gdPI;
		
		// If the angle is actually belong to second or third quadrant
		if ( ( (dXMidOfArc < dXMidOfChord) && (dYMidOfArc > dYMidOfChord))			// second quadrant ( between 90° and 180°)
			|| ( (dXMidOfArc < dXMidOfChord) && (dYMidOfArc == dYMidOfChord) )		// should be 180°
			|| ( (dXMidOfArc < dXMidOfChord) && (dYMidOfArc < dYMidOfChord) )	)	// Or third quadrant ( between 180° and 270°)
		{
			xValue = 180.0 - xValue;
		}
	}	
	else 
	{
		UCLIDException e("ELI01537", "This curve variable at calculateX() is not in VRMiddleToChordMidPointMidOrdinateChordBearing variable relationship");
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
double VRMiddleToChordMidPointMidOrdinateChordBearing::calculateY(CurveVariable *pVarToCalculate,
												vector<CurveVariableRelationship*> vecVRToExempt)
{
	//Curve Point parameter 'Y' value 
	double yValue=0.0;
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI01538", "Memory not allocated for curve variable at calculateY() in VRMiddleToChordMidPointMidOrdinateChordBearing");
		throw e;
	}
	
	vecVRToExempt.push_back(this);

	//Calculating the curve parameter with the respective equation 
	//and returning the same
	if(pVarToCalculate == m_pArcMidPoint)
	{
		bool bLeft = m_pArcConcaveLeft->getBooleanValue(vecVRToExempt);
		double dMidOrdinate = m_pArcMiddleOrdinate->getValue(vecVRToExempt);
		// chord bearing in degrees
		double dChordBearing = m_pArcChordBearing->getValue(vecVRToExempt);
		double dYMidOfChord = m_pArcChordMidPoint->getPointParamYValue(vecVRToExempt);

		if (bLeft)
		{
			yValue = dYMidOfChord - dMidOrdinate * cos(dChordBearing * gdPI/180.0);
		}
		else
		{
			yValue = dYMidOfChord + dMidOrdinate * cos(dChordBearing * gdPI/180.0);
		}
	}
	else if(pVarToCalculate == m_pArcChordMidPoint)
	{
		bool bLeft = m_pArcConcaveLeft->getBooleanValue(vecVRToExempt);
		double dMidOrdinate = m_pArcMiddleOrdinate->getValue(vecVRToExempt);
		// chord bearing in degrees
		double dChordBearing = m_pArcChordBearing->getValue(vecVRToExempt);
		double dYMidOfArc = m_pArcMidPoint->getPointParamYValue(vecVRToExempt);

		if (bLeft)
		{
			yValue = dYMidOfArc + dMidOrdinate * cos(dChordBearing * gdPI/180.0);
		}
		else
		{
			yValue = dYMidOfArc - dMidOrdinate * cos(dChordBearing * gdPI/180.0);
		}
	}
	else if(pVarToCalculate == m_pArcMiddleOrdinate)
	{
		double dXMidOfArc = m_pArcMidPoint->getPointParamXValue(vecVRToExempt);
		double dYMidOfArc = m_pArcMidPoint->getPointParamYValue(vecVRToExempt);
		double dXMidOfChord = m_pArcChordMidPoint->getPointParamXValue(vecVRToExempt);
		double dYMidOfChord = m_pArcChordMidPoint->getPointParamYValue(vecVRToExempt);

		yValue= sqrt( pow((dXMidOfArc - dXMidOfChord), 2) + pow((dYMidOfArc - dYMidOfChord), 2) );
	}
	else if(pVarToCalculate == m_pArcChordBearing)
	{
		bool bLeft = m_pArcConcaveLeft->getBooleanValue(vecVRToExempt);
		double dXMidOfArc = m_pArcMidPoint->getPointParamXValue(vecVRToExempt);
		double dYMidOfArc = m_pArcMidPoint->getPointParamYValue(vecVRToExempt);
		double dXMidOfChord = m_pArcChordMidPoint->getPointParamXValue(vecVRToExempt);
		double dYMidOfChord = m_pArcChordMidPoint->getPointParamYValue(vecVRToExempt);
		double dMidOrdinate = sqrt( pow((dXMidOfArc - dXMidOfChord), 2) + pow((dYMidOfArc - dYMidOfChord), 2) );

		double dCosInput=0.0;
		// get the angle in degrees first
		if (bLeft)
		{
			dCosInput = (dYMidOfChord - dYMidOfArc) / dMidOrdinate;
		}
		else
		{
			dCosInput = (dYMidOfArc - dYMidOfChord) / dMidOrdinate;
		}

		checkAndSetSinCosValue(dCosInput, "ELI05633");
		
		yValue= acos(dCosInput)*(180/gdPI);
		
		// If the angle is actually belong to third or fourth quadrant
		if ( ( (dXMidOfArc < dXMidOfChord) && (dYMidOfArc < dYMidOfChord) )		// third quadrant ( between 180° and 270°)
			|| ( (dXMidOfArc == dXMidOfChord) && (dYMidOfArc < dYMidOfChord))	// == 270°
			|| ( (dXMidOfArc > dXMidOfChord) && (dYMidOfArc < dYMidOfChord)))	// Or fourth quadrant ( between 270° and 360°)
		{
			yValue = 360.0 - yValue;
		}
	}	
	else 
	{
		UCLIDException e("ELI01539", "This curve variable at calculateY() is not in VRMiddleToChordMidPointMidOrdinateChordBearing variable relationship");
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
bool VRMiddleToChordMidPointMidOrdinateChordBearing::canCalculateX(CurveVariable *pVarToCalculate,
												 vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI01540", "Memory not allocated for curve variable at calculateX() in VRMiddleToChordMidPointMidOrdinateChordBearing");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Finding out whether the desired curve parameter can be calculable or not.
	//Checking whether all the dependent variables can be calculatable, if so 
	//return 'True' otherwise 'False'
	if(pVarToCalculate == m_pArcMidPoint)
	{
		return (m_pArcChordMidPoint->canGetPointParamXValue(vecVRToExempt)
			&&m_pArcMiddleOrdinate->canGetValue(vecVRToExempt)
			&&m_pArcChordBearing->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcChordMidPoint)
	{
		return (m_pArcMidPoint->canGetPointParamXValue(vecVRToExempt)
			&&m_pArcMiddleOrdinate->canGetValue(vecVRToExempt)
			&&m_pArcChordBearing->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcMiddleOrdinate)
	{
		return (m_pArcMidPoint->canGetPointParamXValue(vecVRToExempt)
			&&m_pArcChordMidPoint->canGetPointParamXValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcChordBearing)
	{
		return (m_pArcMidPoint->canGetPointParamXValue(vecVRToExempt)
			&&m_pArcChordMidPoint->canGetPointParamXValue(vecVRToExempt));
	}	
	else 
	{
		UCLIDException e("ELI01541", "This curve variable at calculateX() is not in VRMiddleToChordMidPointMidOrdinateChordBearing variable relationship");
		throw e;
	}
}
//==========================================================================================
bool VRMiddleToChordMidPointMidOrdinateChordBearing::canCalculateY(CurveVariable *pVarToCalculate,
												 vector<CurveVariableRelationship*> vecVRToExempt)
{
	//if the Curve Variable is NULL then throw an exception
	if(pVarToCalculate==NULL)
	{
		UCLIDException e("ELI01542", "Memory not allocated for curve variable at calculateY() in VRMiddleToChordMidPointMidOrdinateChordBearing");
		throw e;
	}

	vecVRToExempt.push_back(this);

	//Finding out whether the desired curve parameter can be calculable or not.
	//Checking whether all the dependent variables can be calculatable, if so 
	//return 'True' otherwise 'False'
	if(pVarToCalculate == m_pArcMidPoint)
	{
		return (m_pArcChordMidPoint->canGetPointParamYValue(vecVRToExempt)
			&&m_pArcMiddleOrdinate->canGetValue(vecVRToExempt)
			&&m_pArcChordBearing->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcChordMidPoint)
	{
		return (m_pArcMidPoint->canGetPointParamYValue(vecVRToExempt)
			&&m_pArcMiddleOrdinate->canGetValue(vecVRToExempt)
			&&m_pArcChordBearing->canGetValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcMiddleOrdinate)
	{
		return (m_pArcMidPoint->canGetPointParamYValue(vecVRToExempt)
			&&m_pArcChordMidPoint->canGetPointParamYValue(vecVRToExempt));
	}
	else if(pVarToCalculate == m_pArcChordBearing)
	{
		return (m_pArcMidPoint->canGetPointParamYValue(vecVRToExempt)
			&&m_pArcChordMidPoint->canGetPointParamYValue(vecVRToExempt));
	}	
	else 
	{
		UCLIDException e("ELI01543", "This curve variable at calculateY() is not in VRMiddleToChordMidPointMidOrdinateChordBearing variable relationship");
		throw e;
	}
}
//==========================================================================================
bool VRMiddleToChordMidPointMidOrdinateChordBearing::isCalculatable(CurveVariable *pVarToCalculate,
												  vector<CurveVariableRelationship*> vecVRToExempt)
{
	if (pVarToCalculate == m_pArcMiddleOrdinate || pVarToCalculate == m_pArcChordBearing)
	{
		vecVRToExempt.push_back(this);

		if (!m_pArcMidPoint->isCalculatable(vecVRToExempt) 
			|| !m_pArcChordMidPoint->isCalculatable(vecVRToExempt))
		{
			return false;
		}

		return true;
	}

	return CurveVariableRelationship::isCalculatable(pVarToCalculate, vecVRToExempt);
}
