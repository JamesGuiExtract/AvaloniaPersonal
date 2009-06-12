#pragma once

#include "FiltersDLL.h"

#include "AbstractMeasurement.hpp"

#include <vector>
#include <string>



class EXT_FILTERS_DLL UnknownMeasurement : public AbstractMeasurement
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
	// Purpose: To create a new instance of the UnknownMeasurement object.
	//
	// Require: Nothing.
	//
	// Promise: Returns a new UnknownMeasurement object.  The caller is responsible for deleting the
	//			newly created object.
	AbstractMeasurement* createNew();
	//----------------------------------------------------------------------------------------------
	void evaluate(const char *pszText);
	// Purpose: To initialize this instance of the abstract measurement class.
	//
	// Require: Nothing.
	//
	// Promise: Returns true always.
	//----------------------------------------------------------------------------------------------
	void resetVariables();
	// Purpose: To clear the status of the measurement.
	//
	// Require: Nothing
	//
	// Promise: Returns the measurement to exactly the same state as it is when you first create it 
	//			with a default constructor.
	//----------------------------------------------------------------------------------------------
	bool isValid(void);
	// Purpose: To determine the internal status of this instance of the abstract measurement class.
	//
	// Require: Nothing.
	//
	// Promise: Returns true always.
	//----------------------------------------------------------------------------------------------
	std::vector<std::string> getAlternateStrings(void);
	// Purpose: To get a set of alternate strings.
	//
	// Require: Nothing.
	//
	// Promise: Returns only the last string passed in to evaluate(), or an empty string if evaluate
	//			has not been called.
	//----------------------------------------------------------------------------------------------
	std::string asString(void);
	// Purpose: To return this instance of the measurement class in a string format.
	//
	// Require: isValid() == true;
	// 
	// Promise: Returns the last string passed in to evaluate(), or an empty string if evalutate has
	//			not been called
	//----------------------------------------------------------------------------------------------
	std::string interpretedValueAsString(){ return asString();}

protected:
	
	std::string strEvaluatedString;

};
