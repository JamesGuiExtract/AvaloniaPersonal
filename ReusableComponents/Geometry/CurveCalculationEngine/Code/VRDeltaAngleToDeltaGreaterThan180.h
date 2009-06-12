//==================================================================================================
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE		:	VRDeltaAngleToDeltaGreaterThan180.h
//
// PURPOSE	:	This is an header file for VRDeltaAngleToDeltaGreaterThan180 class
//				where this has been derived from the CurveVariableRelationship
//				class.  The code written in this file makes it possible for
//				implementing the relationship of Curve parameters of type 
//				pArcDelta and pArcDeltaGreaterThan180Degrees
//
// NOTES	:	Nothing
//
// AUTHORS	:	Duan Wang
//
//==================================================================================================
#pragma once

#include "CurveVariableRelationship.h"

/////////////////////////////////////////////////////////////////////////////
// VRDeltaAngleToDeltaGreaterThan180
//==================================================================================================
//
// CLASS		:	VRDeltaAngleToDeltaGreaterThan180
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

class VRDeltaAngleToDeltaGreaterThan180 : public CurveVariableRelationship
{
//Operations
public:
	//----------------------------------------------------------------------------------------------
	// PURPOSE: 
	// REQUIRE: 
	// PROMISE: 
	VRDeltaAngleToDeltaGreaterThan180(CurveVariable *pDeltaAngle, CurveVariable *pDeltaGT180);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: 
	// REQUIRE: 
	// PROMISE: 
	virtual double calculateX(CurveVariable *pVarToCalculate, std::vector<CurveVariableRelationship*> vecVRToExempt);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: 
	// REQUIRE: 
	// PROMISE: 
	virtual double calculateY(CurveVariable *pVarToCalculate, std::vector<CurveVariableRelationship*> vecVRToExempt);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: 
	// REQUIRE: 
	// PROMISE: 
	virtual bool canCalculateX(CurveVariable *pVarToCalculate, std::vector<CurveVariableRelationship*> vecVRToExempt);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: 
	// REQUIRE: 
	// PROMISE: 
	virtual bool canCalculateY(CurveVariable *pVarToCalculate, std::vector<CurveVariableRelationship*> vecVRToExempt);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: 
	// REQUIRE: 
	// PROMISE: 
	virtual bool isCalculatable(CurveVariable *pVarToCalculate,
								std::vector<CurveVariableRelationship*> vecVRToExempt);
	//==============================================================================================

//Attributes
private:
	// delta angle
	CurveVariable *m_pDeltaAngle;
	// delta greater than 180 degrees
	CurveVariable *m_pDeltaGT180;
};
