//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE		:	CurveVariableRelationship.h
//
// PURPOSE	:	This is an header file for CurveVariableRelationship class
//				The code written in this file makes it possible for declaring 
//				a vector of type CurveVariable
// NOTES	:	
//
// AUTHORS	:	G Chandra Sekhar Babu
//
//==================================================================================================
#pragma once

#include <vector>

//forward declarations
class CurveVariable;

/////////////////////////////////////////////////////////////////////////////
// CurveVariableRelationship
//==================================================================================================
//
// CLASS		:	CurveVariableRelationship
//
// PURPOSE		:	This class is used to add the variable to the respective 
//					variable relationship and and for calculating variable
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
class CurveVariableRelationship
{
//Operations
public:
	virtual ~CurveVariableRelationship() {};
	//----------------------------------------------------------------------------------------------
	// PURPOSE: For adding the variable to respective variable relation ship
	// REQUIRE: CurveVariable
	// PROMISE: Addition of the variable to the variable relation ship class.
	void addVariable(CurveVariable *pVar);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To find whether the variable is calculatable
	// REQUIRE: Variable to calculate and vector of unknown variables
	// PROMISE: returns true if the respective variable is calculatable otherwise false
	virtual bool isCalculatable(CurveVariable *pVarToCalculate,
								std::vector<CurveVariableRelationship*> vecVRToExempt);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: Calculates the Curve parameter
	// REQUIRE: CurveVariable
	// PROMISE: Calcaulates the 'X' value of the Curve Point parameter
	virtual double calculateX(CurveVariable *pVarToCalculate, std::vector<CurveVariableRelationship*> vecVRToExempt)=0;
	//----------------------------------------------------------------------------------------------
	// PURPOSE: Calculates the Curve parameter
	// REQUIRE: CurveVariable
	// PROMISE: Calcaulates the 'Y' value of the Curve Point parameter
	virtual double calculateY(CurveVariable *pVarToCalculate, std::vector<CurveVariableRelationship*> vecVRToExempt)=0;
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To find whether Curve parameter 'X' value can be calculatable or not
	// REQUIRE: CurveVariable
	// PROMISE: Returns 'True' if Curve point parameter 'X' value can be calculatable otherwise 'False'
	virtual bool canCalculateX(CurveVariable *pVarToCalculate, std::vector<CurveVariableRelationship*> vecVRToExempt)=0;
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To find whether Curve parameter 'Y' value can be calculatable or not
	// REQUIRE: CurveVariable
	// PROMISE: Returns 'True' if Curve point parameter 'Y' value can be calculatable otherwise 'False'
	virtual bool canCalculateY(CurveVariable *pVarToCalculate, std::vector<CurveVariableRelationship*> vecVRToExempt)=0;
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To ensure that Sine and Cosine value will not exceed 1 or -1
	// PROMISE: If dSinCosValue goes out of (-1, 1) and within set tolerence, then
	//			set it to 1 or -1. If it exceeds set tolerence, then throw exception.
	virtual void checkAndSetSinCosValue(double &dSinCosValue, const std::string& strELICode);
	//==============================================================================================
	
//Attributes
public:
	//vector of type CurveVariable
	std::vector<CurveVariable *> m_vecVars;
};

