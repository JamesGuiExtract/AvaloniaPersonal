//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE		:	VRMiddleToCenterPointRadiusChordBearing.h
//
// PURPOSE	:	This is an header file for VRMiddleToCenterPointRadiusChordBearing class
//				where this has been derived from the CurveVariableRelationship
//				class.  The code written in this file makes it possible for
//				implementing the relationship of Curve parameters of type 
//				pArcMidPoint,pArcCenter,pArcRadius and pArcChordBearing
//
// NOTES	:	Nothing
//
// AUTHORS	:	G Chandra Sekhar Babu
//
//==================================================================================================
#pragma once

#include "CurveVariableRelationship.h"

/////////////////////////////////////////////////////////////////////////////
// VRMiddleToCenterPointRadiusChordBearing
//==================================================================================================
//
// CLASS		:	VRMiddleToCenterPointRadiusChordBearing
//
// PURPOSE		:	This class is used to add the variables to the respective 
//					variable relationship and calculates the desired variable 
//			
// REQUIRE		:	Nothing
// 
// INVARIANTS	:	Nothing
//			
// EXTENSIONS	:	Nothing
//			
// NOTES		:	Nothing
//
//==================================================================================================
class VRMiddleToCenterPointRadiusChordBearing:public CurveVariableRelationship
{
//Operations
public:
	VRMiddleToCenterPointRadiusChordBearing(CurveVariable *pArcConcaveLeft,CurveVariable *pArcMidPoint,CurveVariable *pArcCenterPoint, CurveVariable *pArcRadius,CurveVariable *pArcChordBearing);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To calculate the Curve Variable
	// REQUIRE: CurveVariable
	// PROMISE: Returns Value of the Curve Variable
	virtual double calculateX(CurveVariable *pVarToCalculate, std::vector<CurveVariableRelationship*> vecVRToExempt);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To calculate the Curve Variable
	// REQUIRE: CurveVariable
	// PROMISE: Returns Value of the Curve Variable
	virtual double calculateY(CurveVariable *pVarToCalculate, std::vector<CurveVariableRelationship*> vecVRToExempt);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To find whether Curve parameter 'X' value can be calculatable or not
	// REQUIRE: CurveVariable
	// PROMISE: Returns 'True' if Curve point parameter 'X' value can be calculatable otherwise 'False'
	virtual bool canCalculateX(CurveVariable *pVarToCalculate, std::vector<CurveVariableRelationship*> vecVRToExempt);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To find whether Curve parameter 'Y' value can be calculatable or not
	// REQUIRE: CurveVariable
	// PROMISE: Returns 'True' if Curve point parameter 'Y' value can be calculatable otherwise 'False'
	virtual bool canCalculateY(CurveVariable *pVarToCalculate, std::vector<CurveVariableRelationship*> vecVRToExempt);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To find whether the variable is calculatable
	// REQUIRE: Variable to calculate and vector of unknown variables
	// PROMISE: returns true if the respective variable is calculatable otherwise false
	virtual bool isCalculatable(CurveVariable *pVarToCalculate,
								std::vector<CurveVariableRelationship*> vecVRToExempt);
	//==============================================================================================

//Attributes
private:
	//Concavity of the Arc 
	CurveVariable *m_pArcConcaveLeft;
	//Arc Mid Point
	CurveVariable *m_pArcMidPoint;
	//Arc Center Point
	CurveVariable *m_pArcCenterPoint;
	//Arc Radius
	CurveVariable *m_pArcRadius;
	//Arc Chord Bearing
	CurveVariable *m_pArcChordBearing;
};



			