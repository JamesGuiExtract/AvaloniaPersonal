//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE		:	CurveCalculationEngineImpl.h
//
// PURPOSE	:	This is an header file for CurveCalculationEngineImpl class
//				where this has been derived from the ICurveCalculationEngine
//				class.  
//
// NOTES	:	Nothing
//
// AUTHORS	:	G Chandra Sekhar Babu
//
//==================================================================================================
#pragma once

#include "CCE.h"
#include "ICurveCalculationEngine.h"
#include "CurveVariable.h"
#include "CurveVariableRelationship.h"

#include <memory>
#include <map>
#include <vector>
#include <string>

/////////////////////////////////////////////////////////////////////////////
// CurveCalculationEngineImpl
//==================================================================================================
//
// CLASS		:	CurveCalculationEngineImpl
//
// PURPOSE		:	This class is used to derive set of pure virtual functions 
//					from ICurveCalculationEngine.This class is exported so that 
//					other applications can use the functionality developed here
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
class EXP_CCE_DLL CurveCalculationEngineImpl:public ICurveCalculationEngine
{
	//TODO : need to ensure that the memory allocated below is cleaned properly
// Operations
public:
	// public constructor
	CurveCalculationEngineImpl();
	~CurveCalculationEngineImpl();
	//==============================================================================================
	// PURPOSE: To assign a value to a curve parameter.
	// REQUIRE: eParameter must be a point parameter (see documentation of ECurveParameter)
	// PROMISE: To assign the specified value to the point parameter.
	//			Subsequent calls to canCalculateAllParameters() will return true if all curve 
	//			parameters are now calculatable, or false otherwise.
	// ARGS:	eParameter: the curve parameter to assign a value to.
	//			dX: the x pos of the point to be assigned to the parameter associated w/ eParameter
	//			dY: the y pos of the point to be assigned to the parameter associated w/ eParameter
	void setCurvePointParameter(ECurveParameterType eParameter, double dX, double dY);
	//==============================================================================================
	// PURPOSE: To assign a value to a curve parameter.
	// REQUIRE: eParameter must be an angle/bearing parameter (see documentation of ECurveParameter)
	//			dValue must be in the range [0, 2pi].
	// PROMISE: To assign the specified value to the Curve Angle or  Bearing parameter.
	//			Subsequent calls to canCalculateAllParameters() will return true if all curve 
	//			parameters are now calculatable, or false otherwise.
	// ARGS:	eParameter: the curve parameter to assign a value to.
	//			dValue: the angle/bearing value, represented as radians, to be assigned to the
	//			parameter associated with eParameter.
	 void setCurveAngleOrBearingParameter(ECurveParameterType eParameter, double dValue);
	//==============================================================================================
	// PURPOSE: To assign a value to a curve parameter.
	// REQUIRE: eParameter must be a distance parameter (see documentation of ECurveParameter)
	// PROMISE: To assign the specified value to the Curve Distance parameter.
	//			Subsequent calls to canCalculateAllParameters() will return true if all curve 
	//			parameters are now calculatable, or false otherwise.
	// ARGS:	eParameter: the curve parameter to assign a value to.
	//			dValue: the distance value, represented in default distance units, to be assigned
	//			to the parameter associated with eParameter.
	 void setCurveDistanceParameter(ECurveParameterType eParameter, double dValue);
	//==============================================================================================
	// PURPOSE: To assign a value to a curve parameter.
	// REQUIRE: eParameter must be a boolean parameter (see documentation of ECurveParameter)
	// PROMISE: To assign the specified value to the Curve Boolean parameter.
	//			Subsequent calls to canCalculateAllParameters() will return true if all curve 
	//			parameters are now calculatable, or false otherwise.
	// ARGS:	eParameter: the curve parameter to assign a value to.
	//			bValue: the boolean value to be assigned to the parameter represented by eParameter.
	 void setCurveBooleanParameter(ECurveParameterType eParameter, bool bValue);
	//==============================================================================================
	// PURPOSE: To determine whether all parameters associated with the curve are calculatable.
	//			using those parameters whose value has been set.
	// REQUIRE: Nothing.
	// PROMISE: To return true ONLY if all parameters associated with the curve are calculatable
	//			using those parameters whose value has been set.  If a given "unset" parameter has
	//			multiple ways of being calculated, any one of those ways may be used to calculate
	//			the value.
	 bool canCalculateAllParameters();
	//==============================================================================================
	// PURPOSE: To retrieve the curve starting, mid, and ending points.
	// REQUIRE: canCalculateAllParameters() == true
	// PROMISE: To return via rdX and rdY, the position of the requested point.
	// ARGS:	rdX: the x pos of the requested point, to be returned to the caller.
	//			rdY: the y pos of the requested point, to be returned to the caller.
	 void getStartingPoint(double& rdX, double& rdY);
	 void getMidPoint(double& rdX, double& rdY);
	 void getEndingPoint(double& rdX, double& rdY);
	//==============================================================================================
	// PURPOSE: To retrieve the curve starting, mid, and ending points.
	// REQUIRE: canCalculateAllParameters() == true
	// PROMISE: To return the value of the curve parameter represented by eCurveParamter.
	//			if eCurveParameter is a distance parameter, the returned value will contain the 
	//			distance value (in stringized form) in the default distance unit.  If
	//			eCurveParameter is an angle or bearing parameter, then the angle will be returned 
	//			in radians (in stringized form again).  If eCurveParameter is a boolean parameter,
	//			then either "0" or "1" will be returned.  If eCurveParameter is a point parameter, 
	//			then the point will be returned in the format "x,y".
	// ARGS:	eCurveParameter: the curve parameter, whose value is to be retrieved.
	 string getCurveParameter(ECurveParameterType eCurveParameter);
	//==============================================================================================
	// PURPOSE: To reset the curve parameters.
	// REQUIRE: Nothing.
	// PROMISE: To "unset" the values associated with all curve parameters.
	//			canCalculateAllParameters() will return false.
	 bool reset();
	//==============================================================================================
	// PURPOSE: To check the constraints for the curve parameters.
	// REQUIRE: Nothing.
	// PROMISE: checks for all the curve constraints
	 void CheckForConstraints();
	//==============================================================================================
	// PURPOSE: To calculate the concavity value.
	// REQUIRE: Nothing.
	// PROMISE: assigns bCurveConcaveLeft to either 'Ture' or 'False' 
	 void calcConcavity();
	//==============================================================================================
	// PURPOSE: To find out whether ArcDelta is greater than 180 degrees 
	// REQUIRE: Nothing.
	// PROMISE: assigns bDeltaOver180 to either 'Ture' or 'False' 
	 void calcDeltaOver();
	//==============================================================================================
	// PURPOSE: To find out whether a specific parameter can be calculated 
	// REQUIRE: Nothing.
	// PROMISE: return true if it is calculatable
	 bool canCalculateParameter(ECurveParameterType eCurveParameter);
	//==============================================================================================


//Attributes
	//Curve Parameter
	CurveVariable& operator [](ECurveParameterType eCurveParameter);

//Attributes
private:
	std::string m_strCurveParam;
	//mapping the ECurveParameter with the CurveVariable
	std::map<ECurveParameterType, CurveVariable*> m_mapVars;
	//vector of type CurveVariableRelationship
	std::vector<CurveVariableRelationship*> m_vecVarRelationship;

	//convert angle, bearing or distance from double format to string format
	std::string DoubleToString(double dValue);
	//convert point to string format
	std::string PointToString(double dX, double dY);
};

