//==================================================================================================
//
// COPYRIGHT (c) 1998 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// $Archive: /Engineering/ReusableComponents/Geometry/CurveCalculationEngine/Code/ICurveCalculationEngine.h $
// $Revision: 11 $
// $Modtime: 4/30/02 5:54p $
//
// AUTHORS:	Arvind Ganesan
//
//==================================================================================================

#pragma once

#include "CCE.h"
#include "ECurveParameter.h"

#include <string>

//==================================================================================================
//
// CLASS:	ICurveCalculationEngine
//
// PURPOSE:	Circular arcs can be defined using many parameters.  Only a subset of thes parameters
//			required to uniquely specify a circular arc.  And using specific combination of 
//			parameters that uniquely specify the circular arc, all other parameters of the circular
//			arc can be determined.  This class allows its user to specify known parameters of a
//			circular arc, and determine any of the unknown related parameters. For instance, the
//			caller can specify the starting point, tangent-in-bearing, radius, delta angle, and 
//			curve concavity and request the value of any of the other circular arc parameters (like
//			tangent-out-bearing, arc length, chord bearing, etc).
//
// REQUIRE:	Classes implementing this interface shall call the following methods in the last lines
//			of their constructor:
//			1.  reset()
//			2.  setDefaultDistanceUnit(1.0)
// 
// INVARIANTS:
//			
// EXTENSIONS:
//			
//
// NOTES:	Several methods of this class take as argument, or return as their result, distance
//			values which are expressed in the current default unit.  See the specification of
//			the setDefaultDistanceUnit() method for details about the default unit.
//
//==================================================================================================

class EXP_CCE_DLL ICurveCalculationEngine
{
public:
	//==============================================================================================
	// PURPOSE: To destruct this object.
	// REQUIRE: Nothing.
	// PROMISE: To destruct this object and release any resources used by this object.
	// ARGS:	None.
	virtual ~ICurveCalculationEngine(){};
	//==============================================================================================
	// PURPOSE: To assign a value to a curve parameter.
	// REQUIRE: eParameter must be a point parameter (see documentation of ECurveParameterType)
	// PROMISE: To assign the specified value to the point parameter.
	//			Subsequent calls to canCalculateAllParameters() will return true if all curve 
	//			parameters are now calculatable, or false otherwise.
	// ARGS:	eParameter: the curve parameter to assign a value to.
	//			dX: the x pos of the point to be assigned to the parameter associated w/ eParameter
	//			dY: the y pos of the point to be assigned to the parameter associated w/ eParameter
	virtual void setCurvePointParameter(ECurveParameterType eParameter, double dX, double dY) = 0;
	//==============================================================================================
	// PURPOSE: To assign a value to a curve parameter.
	// REQUIRE: eParameter must be an angle/bearing parameter (see documentation of ECurveParameterType)
	//			dValue must be in the range [0, 2pi].
	// PROMISE: To assign the specified value to the point parameter.
	//			Subsequent calls to canCalculateAllParameters() will return true if all curve 
	//			parameters are now calculatable, or false otherwise.
	// ARGS:	eParameter: the curve parameter to assign a value to.
	//			dValue: the angle/bearing value, represented as radians, to be assigned to the
	//			parameter associated with eParameter.
	virtual void setCurveAngleOrBearingParameter(ECurveParameterType eParameter, double dValue) = 0;
	//==============================================================================================
	// PURPOSE: To assign a value to a curve parameter.
	// REQUIRE: eParameter must be a distance parameter (see documentation of ECurveParameterType)
	// PROMISE: To assign the specified value to the point parameter.
	//			Subsequent calls to canCalculateAllParameters() will return true if all curve 
	//			parameters are now calculatable, or false otherwise.
	// ARGS:	eParameter: the curve parameter to assign a value to.
	//			dValue: the distance value, represented in default distance units, to be assigned
	//			to the parameter associated with eParameter.
	virtual void setCurveDistanceParameter(ECurveParameterType eParameter, double dValue) = 0;
	//==============================================================================================
	// PURPOSE: To assign a value to a curve parameter.
	// REQUIRE: eParameter must be a boolean parameter (see documentation of ECurveParameterType)
	// PROMISE: To assign the specified value to the point parameter.
	//			Subsequent calls to canCalculateAllParameters() will return true if all curve 
	//			parameters are now calculatable, or false otherwise.
	// ARGS:	eParameter: the curve parameter to assign a value to.
	//			bValue: the boolean value to be assigned to the parameter represented by eParameter.
	virtual void setCurveBooleanParameter(ECurveParameterType eParameter, bool bValue) = 0;
	//==============================================================================================
	// PURPOSE: To determine whether all parameters associated with the curve are calculatable.
	//			using those parameters whose value has been set.
	// REQUIRE: Nothing.
	// PROMISE: To return true ONLY if all parameters associated with the curve are calculatable
	//			using those parameters whose value has been set.  If a given "unset" parameter has
	//			multiple ways of being calculated, any one of those ways may be used to calculate
	//			the value.
	virtual bool canCalculateAllParameters() = 0;
	//==============================================================================================
	// PURPOSE: To determine whether a specific parameter associated with the curve is calculatable
	//			using those parameters whose value has been set.
	// REQUIRE: Nothing.
	// PROMISE: To return true if the specified parameter is calculatable using those parameters 
	//			whose value has been set.
	virtual bool canCalculateParameter(ECurveParameterType eType) = 0;
	//==============================================================================================
	// PURPOSE: To retrieve the curve starting, mid, and ending points.
	// REQUIRE: canCalculateAllParameters() == true
	// PROMISE: To return via rdX and rdY, the position of the requested point.
	// ARGS:	rdX: the x pos of the requested point, to be returned to the caller.
	//			rdY: the y pos of the requested point, to be returned to the caller.
	virtual void getStartingPoint(double& rdX, double& rdY) = 0;
	virtual void getMidPoint(double& rdX, double& rdY) = 0;
	virtual void getEndingPoint(double& rdX, double& rdY) = 0;
	//==============================================================================================
	// PURPOSE: To retrieve the curve starting, mid, and ending points.
	// REQUIRE: canCalculateAllParameters() == true
	// PROMISE: To return the value of the curve parameter represented by eCurveParamter.
	//			if ECurveParameterType is a distance parameter, the returned value will contain the 
	//			distance value (in stringized form) in the default distance unit.  If
	//			ECurveParameterType is an angle or bearing parameter, then the angle will be returned 
	//			in radians (in stringized form again).  If ECurveParameterType is a boolean parameter,
	//			then either "0" or "1" will be returned.  If ECurveParameterType is a point parameter, 
	//			then the point will be returned in the format "x,y".
	// ARGS:	ECurveParameterType: the curve parameter, whose value is to be retrieved.
	virtual std::string getCurveParameter(ECurveParameterType ECurveParameterType) = 0;
	//==============================================================================================
	// PURPOSE: To reset the curve parameters.
	// REQUIRE: Nothing.
	// PROMISE: To "unset" the values associated with all curve parameters.
	//			canCalculateAllParameters() will return false.
	virtual bool reset() = 0;
	//==============================================================================================
};

