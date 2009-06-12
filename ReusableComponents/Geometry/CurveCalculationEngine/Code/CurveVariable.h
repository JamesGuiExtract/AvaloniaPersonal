//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE		:	CurveVariable.h
//
// PURPOSE	:	This is an header file for CurveVariable class
//				The code written in this file makes it possible for 
//				initializing the CurveVariable and declaring a vector 
//				of type CurveVariableRelationship
//
// NOTES	:	Nothing
//
// AUTHORS	:	G Chandra Sekhar Babu
//
//==================================================================================================
#pragma once

#include <vector>
#include <UCLIDException.h>

const double gdPI = 3.1415926535897932384626433832795;


//forward declarations
class CurveVariableRelationship;

/////////////////////////////////////////////////////////////////////////////
// CurveVariable
//==================================================================================================
//
// CLASS		:	CurveVariable
//
// PURPOSE		:	This class is used to add the variable relationships,
//					initialize the CurveVariable,getting the respective values
//					of the variables and to find out whether the variable 
//					is calculatable or not
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
class CurveVariable
{
//Operations
public:
	CurveVariable();// public constructor

	//CurveVariable(const CurveVariable& cv); // copy constructor
	
	//----------------------------------------------------------------------------------------------
	// PURPOSE: For adding several variable relation ships
	// REQUIRE: CurveVariableRelationship
	// PROMISE: Addition of variable relation ships
	void addVariableRelationship(CurveVariableRelationship *pVR);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To reset the Curve Parameters
	// REQUIRE: Nothing
	// PROMISE: To unset the values associated with all the Curve parameters
	void reset();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To find out whether the variable is Calculatable or not
	// REQUIRE: 'CurveVariable' vector
	// PROMISE: Teturns true if the variable is calculatable otherwise false
	bool isCalculatable(std::vector<CurveVariableRelationship*> vecVRToExempt);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To check whether the Curve variable has been assigned a value or not
	// REQUIRE: Nothing
	// PROMISE: Returns 'true' if the Curve variable has been assigned a value otherwise false 
	bool isAssigned();
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To assign a value to the boolean Curve Parameters,angle/bearing 
	//			Curve Parameters and distance Curve Parameters  
	// REQUIRE: angle/bearing or distance parameter value and boolean parameter value
	// PROMISE: Assigns the value to the CurveParameter and sets 'm_bAssigned' to 'true'
    void assign(double dValue);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To assign a value to the Curve Point Parameters
	// REQUIRE: X and Y coordinate values
	// PROMISE: Assigns (X,Y) to Curve Point Parameters and sets 'm_bAssigned' to 'true'
	void assignPointParameter(double dxValue,double dyValue);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To get the boolean value associated with the boolean Curve Parameter
	// REQUIRE: Nothing
	// PROMISE: Returns the Value associated with the boolean Curve Parameter
    bool getBooleanValue(std::vector<CurveVariableRelationship*> vecVRToExempt);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To get the Value associated with the Curve Parameter(angle/bearing or distance)
	// REQUIRE: Nothing
	// PROMISE: Returns the Value associated with the respective Curve Parameter 
	// PARAM:	pVRToExempt: this relationship will be excluded from all relationships this variable
	//			has such that it will not go into an infinite loop if the value for a variable can be
	//			obtained through more than one relationship.
	double getValue(std::vector<CurveVariableRelationship*> vecVRToExempt);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To find out whether Curve Parameter can be calculatable or not
	// REQUIRE: Nothing
	// PROMISE: Returns 'True' if Curve Parameter can be calculatable otherwise 'False'
	bool canGetValue(std::vector<CurveVariableRelationship*> vecVRToExempt);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To get the 'X' Value associated with the Curve point parameter
	// REQUIRE: Nothing
	// PROMISE: Returns the 'X' Value associated with Curve point parameter 
	double getPointParamXValue(std::vector<CurveVariableRelationship*> vecVRToExempt);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To find whether Curve point parameter 'X' Value can be calculatable or not
	// REQUIRE: Nothing
	// PROMISE: Returns 'True' if Curve point parameter 'X' value can be calculatable otherwise 'False'
	bool canGetPointParamXValue(std::vector<CurveVariableRelationship*> vecVRToExempt);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To get the 'Y' Value associated with the Curve point parameter
	// REQUIRE: Nothing
	// PROMISE: Returns the 'Y' Value associated with Curve point parameter 
	double getPointParamYValue(std::vector<CurveVariableRelationship*> vecVRToExempt);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To find whether Curve point parameter 'Y' Value can be calculatable or not
	// REQUIRE: Nothing
	// PROMISE: Returns 'True' if Curve point parameter 'Y' value can be calculatable otherwise 'False'
	bool canGetPointParamYValue(std::vector<CurveVariableRelationship*> vecVRToExempt);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To check 0 < CurveAngle <360
	// REQUIRE: Curve Angle
	// PROMISE: Returns Curve Angle( 0 < CurveAngle < 360)
	double checkCurveAngle(double dValue);
	//==============================================================================================

//Attributes
public:
	//Curve parameter value
	double m_dValue;
	//Curve Point parameter 'X' value 
	double m_xValue;
	//Curve Point parameter 'Y' value
	double m_yValue;
	//To check whether the variable is assigned a particualar value or not
	bool m_bAssigned;
	//Pointer to stored decimal-point position
	int m_iDecimal;
	//Pointer to stored sign indicator
	int m_iSign;
	//Pointer to the CurveParameter
	char *m_pszBuffer;

private:
	//vector of type CurveVariableRelationship
	std::vector<CurveVariableRelationship *> vecVarRelationships;
};

