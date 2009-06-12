#pragma once

#include "FiltersDLL.h"

#include "AbstractMeasurement.hpp"

#include <DistanceCore.h>
#include <string>
#include <cassert>


class TPPoint;

class EXT_FILTERS_DLL Distance: public AbstractMeasurement
{
public:
	//----------------------------------------------------------------------------------------------
	EType getType() const;
	//----------------------------------------------------------------------------------------------
	// Purpose: To get the type of the measurement in stringized form
	//
	// Require: Nothing.
	//
	// Promise: Returns the type of the measurement in stringized form
	const std::string& getTypeAsString() const;
	//----------------------------------------------------------------------------------------------
	// Purpose: To initialize this instance of the distance class.
	//
	// Require: Nothing.
	//
	// Promise: isValid() will return false;
	Distance();
	//--------------------------------------------------------------------------
	//Purpose:  copy constructor
	//
	//Require: Nothing.
	//
	//Promise: deep copy
	Distance(const Distance& distance);
	//----------------------------------------------------------------------------------------------
	// Purpose: To clean up this instance of the distance class.
	//
	// Require: Nothing.
	//
	// Promise: To clean up any memory allocated by this instance of the distance class.
	~Distance();
	//----------------------------------------------------------------------------------------------
	//Purpose: Overload equal operator
	//
	//Require: Nothing
	//
	//Promise: Return distance = d;
	Distance& operator = (const Distance& d);
	//----------------------------------------------------------------------------------------------
	// Purpose: To create a new instance of the Distance object.
	//
	// Require: Nothing.
	//
	// Promise: Returns a new Distance object.  The caller is responsible for deleting the
	//			newly created object.
	AbstractMeasurement* createNew();
	//----------------------------------------------------------------------------------------------
	// Purpose: To set private member to default value
	//
	// Require: Nothing
	//
	//Promise: Set variables to default
	void resetVariables();
	//----------------------------------------------------------------------------------------------
	// Purpose: To initialize this instance of the distance class.
	//
	// Require: Nothing.
	//
	// Promise: If pszDistance is a valid distance string, then isValid() shall return true and
	//			getDistance() and getDistanceString() shall return the 
	//			correct values as indicated by the distance string.
	//			If pszDistance is not a valid distance string, then isValid() shall return false.
	void evaluate(const char *pszDistance);
	//----------------------------------------------------------------------------------------------
	// Purpose: To initialize this instance of the Distance class.
	//
	// Require: Nothing.
	//
	// Promise: To associate the distance from p1 to p2 as the "distance value" associated with
	//			this Distance object.
	void evaluate(const TPPoint&p1, const TPPoint& p2);
	//----------------------------------------------------------------------------------------------
	// Purpose: To determine the internal status of this instance of the distance class.
	//
	// Require: Nothing.
	//
	// Promise: To return true, if a valid distance string has been passed to either the constructor
	//			or the evaluate function and if the string has been successfully parsed.
	//			Promise to return false otherwise.
	bool isValid(void);
	//----------------------------------------------------------------------------------------------
	// Purpose: If the distance string that was evaluated was not valid, then this function should
	//			return a list of possible alternate strings that would make the original distance
	//			string valid.  These options will be shown to the user in a dialog box, and the
	//			user will be requested to pick from one of these options.
	//
	// Require: Nothing.
	//
	// Promise: If isValid() is true, then the vector must contain one string which has the value
	//			getDistanceString().  If isValid() is false, then the vector should contain possible
	//			alternatives that would make the entered distance string valid (This method shall
	//			therefore take into account all the different spots where a typo could have been 
	//			made or the OCR program could have recognized the distance wrongly.)
	std::vector<std::string> getAlternateStrings(void);
	//----------------------------------------------------------------------------------------------
	// Purpose: To return the distance represented by this instance of the distance class in the
	//			the current drawing units.
	//
	// Require: isValid() == true;
	// 
	// Promise: To return the distance represented by this instance of the distance class in the
	//			the current units (i.e. whatever the current drawing units is set to be)
	//			If isValid() == false, then promise to throw string("ERROR: Invalid distance!");
	double getDistanceInUnit(EDistanceUnitType eOutUnit);
	//
	const std::vector<std::string>& getDistanceUnitStrings() const;
	//----------------------------------------------------------------------------------------------
	// Purpose: To return this instance of the distance class in a standard distance string form such as
	//			"24.234".
	//
	// Require: isValid() == true;
	// 
	// Promise: To return this instance of a distance in a standard distance string format if 
	//			isValid() == true;
	//			If isValid() == false, then promise to throw string("ERROR: Invalid distance!");
	std::string asString(void);
	//----------------------------------------------------------------------------------------------
	// Purpose: To return this instance of the Distance class in a string format which reflects 
	//			the actual value output.
	//
	// Require: isValid() == true;
	// 
	// Promise: To return this instance of the measurement class in a standard string format if 
	//			isValid() == true;
	//			If isValid() == false, then promise to throw string("ERROR: Invalid angle!");
	std::string interpretedValueAsString(void);
	//----------------------------------------------------------------------------------------------
	//
	std::string getEvaluatedString(void);
	//
	EDistanceUnitType getDefaultDistanceUnit();
	//
	std::string asStringInUnit(EDistanceUnitType eOutUnit);
	// 
	void setDefaultDistanceUnit(EDistanceUnitType eDefaultUnit);
	// for a given unit type, retrieve its standard string representation
	std::string getStringFromUnit(EDistanceUnitType eUnit);
	// for a given string, get the unit type
	EDistanceUnitType getUnitFromString(const std::string& strUnit);



private:
	// Use uclid distance filter object
	DistanceCore m_DistanceCore;

};
