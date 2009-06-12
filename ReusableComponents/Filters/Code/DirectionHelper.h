////////////////////////////////////////////////////////////////////////////////////////////
// Class:	DirectionHelper
// 
// Purpose:	In order to support bearing, polar angle and azimuth input as input direction, 
//			this class will evaluate the acutal input string into a polar angle value for 
//			internal calculation, and furthermore, it can also convert polar angle value 
//			into a double value in accordance with the current direction type. 

#pragma once

#include "FiltersDLL.h"
#include "Angle.hpp"
#include "Bearing.hpp"


// type for input direction
enum EDirection{kUnknownDirection = 0, kBearingDir, kPolarAngleDir, kAzimuthDir};

class AbstractMeasurement;

class EXT_FILTERS_DLL DirectionHelper
{
public:
	//==============================================================================================
	// Purpose: Evaluates the input direction string, and then outputs the direction in string format
	//			
	// Require: No conversion is made in this function. The input direction string must be valid
	//
	// Note:	This function does no checking on whether the input is in reverse mode or not
	//
	std::string DirectionHelper::directionAsString();
	//==============================================================================================
	// Purpose: Evaluates the input string as direction using Bearing or Angle classes
	//			
	// Require: No conversion is made in this function.
	//
	// Note:	The input string is in the format of one of the three input directions 
	//			(i.e. bearing, polar angle, azimuth). This function checks the current 
	//			input direction type, then uses the appropriate filter class to process 
	//			the evaluation of the input string.
	//
	void evaluateDirection(const std::string& strInputDirection);
	//==============================================================================================
	// Purpose: If the input string is invalid, give a vector of possible alternative direction 
	//			strings in the format of current direction type (Bearing, Polar Angle or Azimuth). 
	//			
	// Require: In order to use this function, a prior call to the evaluateDirection() must be made.
	//			No conversion is made in this function.
	// 
	std::vector<std::string> getAlternateStringsAsDirection(void);
	//==============================================================================================
	// Purpose: Gets current measurement object
	//
	AbstractMeasurement* getCurrentMeasurement();
	//==============================================================================================
	// Purpose: Returns the input direction in degrees in terms of polar angle (0° = East) since
	//			internally we are using polar angle to do the calculation
	//
	// Require: In order to use this function, a prior call to the evaluateDirection() must be made.
	//			Conversion must be made if input direction type is Azimuth. 
	// 
	double getPolarAngleDegrees();
	//==============================================================================================
	// Purpose: Returns the input direction in radians in terms of polar angle (0° = East) since
	//			internally we are using polar angle to do the calculation
	//
	// Require: In order to use this function, a prior call to the evaluateDirection() must be made.
	//			Conversion must be made if input direction type is Azimuth. 
	// 
	double getPolarAngleRadians();
	//==============================================================================================
	// Purpose: Evaluates the input direction string, and then outputs the direction in string format
	//			
	// Require: No conversion is made in this function. The input direction string must be valid
	//
	// Note:	This function checks whether the input is in reverse mode or not. Therefore, the return
	//			string reflects the actual value that is interpreted for calculation. For example, 
	//			if the input direction is "23 degrees" (Azimuth), and it's in reverse mode, the 
	//			interpreted direction as string shall be "203 degrees" (Azimuth). If it's in normal 
	//			mode, then the interpreted direction as string shall be the same as the result from 
	//			directionAsString(), which is "23 degrees" (Azimuth)
	//
//	std::string DirectionHelper::interpretedDirectionAsString();
	//==============================================================================================
	// Purpose: Whether or not the input string is a kind of direction (i.e. Bearing,
	//			Polar Angle or Azimuth)
	//
	// Require: In order to use this function, a prior call to the evaluateDirection() must be made.
	//
	bool isDirectionValid();
	//==============================================================================================
	// Purpose:	Converts polar angle value (in radians) to current direction type format (in string)
	//			
	// Promise: Returns string format for the current direction type. For instance, if direction type
	//			is Bearing, then 1.43 will be converted to N8d4m1sE. If direction type is Azimuth,
	//			then 1.43 will be converted to 8d4m1s. If direction type is Polar angle, then 1.43
	//			will be converted to 81d55m59s. 
	//
	// Require: Call polarAngleToCurrentDirectionInRadians() internally
	//
	std::string polarAngleInRadiansToDirectionInString(double dRadians);
	//==============================================================================================
	// Purpose:	Converts polar angle value (in degrees) to current direction type value (in degrees)
	//			
	// Require: Conversion must be made if current direction type is azimuth
	//
	// Args:	dPolarAngleDegrees: The value of angle in degrees. Its base is East. (i.e. 
	//			0° = X axis positive)
	// Note:	Since internally we always deal with angles in terms of polar angle, this conversion
	//			function is needed for output. For instance, at UI level, user chooses Azimuth
	//			as the input direction type, first evaluateDirection() shall be called (see 
	//			evaluateDirection() ) to get the angle (in terms of polar angle) value out, and then
	//			use it for internal calculation. Whenever an output to the UI is requested, the 
	//			actual polar angle value shall be converted to the angle value in terms of current
	//			input direction type.
	//
	double polarAngleToCurrentDirectionInDegrees(double dPolarAngleDegrees);
	//==============================================================================================
	// Purpose:	Converts polar angle value (in radians) to current direction type value (in radians)
	//			
	// Require: Conversion must be made if current direction type is azimuth
	//
	// Args:	dPolarAngleRadians: The value of angle in degrees. Its base angle is East. (i.e. 
	//			0 = X axis positive (East) )
	//
	// Note:	Since internally we always deal with angles in terms of polar angle, this conversion
	//			function is needed for output. For instance, at UI level, user chooses Azimuth
	//			as the input direction type, first evaluateDirection() shall be called (see 
	//			evaluateDirection() ) to get the angle (in terms of polar angle) value out, and then
	//			use it for internal calculation. Whenever an output to the UI is requested, the 
	//			actual polar angle value shall be converted to the angle value in terms of current
	//			input direction type.
	//
	double polarAngleToCurrentDirectionInRadians(double dPolarAngleRadians);
	//==============================================================================================
	// Purpose: Set current direction input type	
	//
	static void sSetDirectionType(EDirection eDirection){m_sEDirection = eDirection;}
	//==============================================================================================
	// Purpose: Get current direction input type	
	//
	static EDirection sGetDirectionType(){return m_sEDirection;}


private:
	static EDirection m_sEDirection;

	Bearing m_BearingDirection;
	Angle m_PolarAngleDirection;
	Angle m_AzimuthDirection;

	//********** Helper funcitons ********************
	//==============================================================================================
	// Purpose:	Convert azimuth to polar angle, and polar angle to azimuth
	//
	// Note:	These two conversions are using the same calculation expression
	//
	double azimuthPolarAngleConversionsInDegrees(double dDegrees);
	//==============================================================================================
	// Purpose:	Convert azimuth to polar angle, and polar angle to azimuth
	//
	// Note:	These two conversions are using the same calculation expression
	//
	double azimuthPolarAngleConversionsInRadians(double dRadians);
};