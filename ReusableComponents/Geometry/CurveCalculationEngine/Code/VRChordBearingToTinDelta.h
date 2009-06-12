//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE		:	VRChordBearingToTinDelta.h
//
// PURPOSE	:	This is an header file for VRChordBearingToTinDelta class
//				where this has been derived from the CurveVariableRelationship
//				class.  The code written in this file makes it possible for
//				implementing the relationship of Curve parameters of type 
//				pArcChordBearing,pArcTangentInBearing and pArcDelta
//
// NOTES	:	Nothing
//
// AUTHORS	:	G Chandra Sekhar Babu
//
//==================================================================================================
#pragma once

#include "CurveVariableRelationship.h"

/////////////////////////////////////////////////////////////////////////////
// VRChordBearingToTinDelta
//==================================================================================================
//
// CLASS		:	VRChordBearingToTinDelta
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
class VRChordBearingToTinDelta:public CurveVariableRelationship
{
//Operations
public:
	VRChordBearingToTinDelta(CurveVariable *pArcConcaveLeft,CurveVariable *pArcChordBearing,CurveVariable *pArcTangentInBearing, CurveVariable *pArcDelta);
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
	//==============================================================================================
//Attributes
private:
	//Concavity of the variable
	CurveVariable *m_pArcConcaveLeft;
	//Arc Chord Bearing
	CurveVariable *m_pArcChordBearing;
	//Arc Tangent In Bearing
	CurveVariable *m_pArcTangentInBearing;
	//Arc Delta
	CurveVariable *m_pArcDelta;
};


			