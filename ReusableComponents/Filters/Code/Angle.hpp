//#pragma once, never use it except for microsoft compiler only
//use ifndef...define to speed up compilation
#pragma once

#include "FiltersDLL.h"

#include "AbstractMeasurement.hpp"

#include <math.h>
#include <string.h>
#include <iostream>
#include <fstream>
#include <iomanip>

#include <vector>
#include <string>
#include <cassert>

class TPPoint;

class EXT_FILTERS_DLL Angle : public AbstractMeasurement
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
	// Purpose: To initialize this instance of the angle class.
	//
	// Require: Nothing.
	//
	// Promise: isValid() will return false;
	Angle();
	//----------------------------------------------------------------------------------------------
	//Purpose:  copy constructor
	//
	//Require: Nothing.
	//
	//Promise: deep copy
	Angle(const Angle &angle);
	//------------------------------------------------------------------------
	// Purpose: To initialize this instance of the angle class.
	//
	// Require: Nothing.
	//
	// Promise: The same results as the evaluate() method.
	Angle(const char *pszAngle);
	//----------------------------------------------------------------------------------------------
	// Purpose: To clean up this instance of the angle class.
	//
	// Require: Nothing.
	//
	// Promise: To clean up any memory allocated by this instance of the angle class.
	~Angle();
	//----------------------------------------------------------------------------------------------
	//Purpose: Overload equal operator
	//
	//Require: Nothing
	//
	//Promise: Return angle = a;
	Angle& operator=(const Angle& a);
	//----------------------------------------------------------------------------------------------
	// Purpose: To create a new instance of the Angle object.
	//
	// Require: Nothing.
	//
	// Promise: Returns a new Angle object.  The caller is responsible for deleting the
	//			newly created object.
	AbstractMeasurement* createNew();
	//----------------------------------------------------------------------------------------------
	//Purpose: To set private member to default value
	//
	//Require: Nothing
	//
	//Promise: Set variables to default value
	void resetVariables();
	//----------------------------------------------------------------------------------------------
	// Purpose: To initialize this instance of the angle class.
	//
	// Require: Nothing.
	//
	// Promise: If pszAngle is a valid angle string, then isValid() shall return true and
	//			getDegrees() and getAngleString() shall return the 
	//			correct values as indicated by the angle string.
	//			If pszAngle is not a valid angle string, then isValid() shall return false.
	void evaluate(const char *pszAngle);
	//----------------------------------------------------------------------------------------------
	// Purpose: To initialize this instance of the bearing class.
	//
	// Require: Nothing.
	//
	// Promise: To convert the specified radians into an internal angle.  isValid() will return 
	//			true.
	void evaluateRadians(const double& dRadians);
	//----------------------------------------------------------------------------------------------
	// Purpose: To initialize this instance of the Angle class.
	//
	// Require: p1 != p2.
	//
	// Promise: To associate the angle from p1 to p2 as the "angle value" associated with
	//			this Angle object.
	void evaluate(const TPPoint&p1, const TPPoint& p2);
	//----------------------------------------------------------------------------------------------
	// Purpose: To determine the internal status of this instance of the angle class.
	//
	// Require: Nothing.
	//
	// Promise: To return true, if a valid angle string has been passed to either the constructor
	//			or the evaluate function and if the string has been successfully parsed.
	//			Promise to return false otherwise.
	bool isValid(void);
	//----------------------------------------------------------------------------------------------
	// Purpose: If the angle string that was evaluated was not valid, then this function should
	//			return a list of possible alternate strings that would make the original angle
	//			string valid.  These options will be shown to the user in a dialog box, and the
	//			user will be requested to pick from one of these options.
	//
	// Require: Nothing.
	//
	// Promise: If isValid() is true, then the vector must contain one string which has the value
	//			getAngleString().  If isValid() is false, then the vector should contain possible
	//			alternatives that would make the entered angle string valid (This method shall
	//			therefore take into account all the different spots where a typo could have been 
	//			made or the OCR program could have recognized the angle wrongly.)
	std::vector<std::string> getAlternateStrings(void);
	//----------------------------------------------------------------------------------------------
	// Purpose: To return the degrees that represent this instance of a angle.
	//
	// Require: isValid() == true;
	// 
	// Promise: To return the angle that represent this instance of the angle class if 
	//			isValid()==true.  For instance, if the angle string was "24.23" then the 
	//			representative degrees (double) 24.23, etc.
	//			If isValid() == false, then promise to throw string("ERROR: Invalid angle!");
	double getDegrees(void);
	//----------------------------------------------------------------------------------------------
	// Purpose: To return the radians that represent this instance of a angle.
	//
	// Require: isValid() == true;
	// 
	// Promise: To return the angle that represent this instance of the angle class if 
	//			isValid()==true.  For instance, if the angle string was "24.23" then the 
	//			representative radians (double) , etc.
	//			If isValid() == false, then promise to throw string("ERROR: Invalid angle!");
	double getRadians();
	//----------------------------------------------------------------------------------------------
	// Purpose: To return this instance of the angle class in a standard angle string form such as
	//			"24.234".
	//
	// Require: isValid() == true;
	// 
	// Promise: To return this instance of a angle in a standard angle string format if 
	//			isValid() == true;
	//			If isValid() == false, then promise to throw string("ERROR: Invalid angle!");
	// Note:	This method always returns original input value, not the interpreted value.
	//
	std::string asString(void);
	//----------------------------------------------------------------------------------------------
	// Purpose: To return this instance of the angle class in a standard angle string form such as
	//			"24.234" which is the interpreted value for the entered value.
	//
	// Require: isValid() == true;
	// 
	// Promise: To return this instance of a angle in a standard angle string format if 
	//			isValid() == true;
	//			If isValid() == false, then promise to throw string("ERROR: Invalid angle!");
	std::string interpretedValueAsString(void);
	//----------------------------------------------------------------------------------------------
	// Purpose: To return this angle as string in the format of decimal degrees
	//
	// Require: 
	// 
	// Promise: 
	std::string asStringDecimalDegree(void);
	//----------------------------------------------------------------------------------------------
	// Purpose: To return the angle as string in the format of degree, minutes and second (ex. 23d12m14s)
	//
	// Require: 
	// 
	// Promise: 
	std::string asStringDMS(void);
	//----------------------------------------------------------------------------------------------	
	// Purpose: 
	//
	// Require: 
	// 
	// Promise: 
	std::string getEvaluatedString(void);
	//----------------------------------------------------------------------------------------------	
	// Purpose: get the angle degree unit strings
	//
	// Require: 
	// 
	// Promise: 
	const std::vector<std::string>& getAngleDegStrings() const {return strVecDegUnit;}
	//----------------------------------------------------------------------------------------------
	// Purpose: get the angle minute unit strings
	//
	// Require: 
	// 
	// Promise: 
	const std::vector<std::string>& getAngleMinStrings() const {return strVecMinUnit;}
	//----------------------------------------------------------------------------------------------
	// Purpose: get the angle second unit strings
	//
	// Require: 
	// 
	// Promise: 
	const std::vector<std::string>& getAngleSecStrings() const {return strVecSecUnit;}
	//----------------------------------------------------------------------------------------------

protected:
	
	std::string strEvaluatedString;

	// include any necessary protected methods/attributes here.
	void tokenString();
	// Purpose: Split the angle string, (it's like the bearing middle part)
	//
	//Require: Nothing
	//
	// Promise: Divided the angle string
	//------------------------------------------------------------------------------------------
 
	void setUnit();
	//Purpose: Set valid unit of  degree, minute, second
	//
	//Require: Nothing
	//
	//Promise: Set valid unit to degree vector, minute vector, second vector 
	void setAltVec();
	//Purpose: Set alternate string vector
	//
	//Require: Nothing
	//
	//Promise: Set alternate string vector, if no alternate string, push input string
	 
	char filterChar(char ch);
	//Purpose: Filter char, ex 'L' change to '1', 's' change to '5'
	//
	//Require: Passin one char
	//
	//Promise: Return correct char
	 
	void checkValid();
	//Purpose: Check the digits string and unit of degree, minute, second
	//         Set the valid flag
	//Require: Nothing
	//
	//Promise: Set each valid flag
	char checkDigit(char ch);
	//Purpose: Check the first digit of minute and second if they have two
	//         digits, the first digit should less than 6
	//Require: Pass in a char
	//
	//Promise: Return char must less than '6'
	void truncateAngle(double d);
	//Purpose: Truncate double value Angle to standard format.
	//         ex. 3.14159 - 3.14 (only two digits after decimal point
	//         also get the stand angle string
	//
	//Require: Pass in double value
	//
	//Promise: Get standard format double value, and stand string
	double truncateRadians(double d);
	//Purpose: Truncate double value Angle to standard format.
	//         ex. 3.14159 - 3.14 (only two digits after decimal point
	//          
	//
	//Require: Pass in double value
	//
	//Promise: Get standard format double value 
	bool checkUnit(const std::string& str, const std::vector<std::string> &strVecUnit);
	//Purpose: Check the string of unit is in unit vector
	//
	//Require: Pass in a string
	//
	//Promise: Return true if string is in unit vector

private:
	// include any private methods/attributes here.
	 
	double  dDegree;       //digit num for degree
	double  dMinute;       //digit num for minute 
	double  dSecond;       //digit num for second

	double  dAngle;        //digit angle for auto cad
	double  dRadians;      //radians value

	bool bValid;
	bool bNegative;
	

	std::string strAngleString;  //input string
	std::string strStandString;  //stand string

	std::string strDegString;   // degree string
	std::string strMinString;    //minute string
	std::string strSecString;    //second string

	std::string strDegUnit;      //degree unit string "deg"
	std::string strMinUnit;     //minute unit string "  '  "
	std::string strSecUnit;     //second unit string "  "  "

	std::string strTempAlt;    //hold alternate string

	//vector hold the valid unit for degree, minute, second
	 
	std::vector<std::string> strVecDegUnit;    //vector hold all valid degree unit " deg ", "D" ...
	std::vector<std::string> strVecMinUnit;    //vector hold all valid minute unit " min ", minutes"
	std::vector<std::string> strVecSecUnit;    //vectro hold all valid second unit " SECOND " "s"..
	
	// ARVIND COMMENTED ON 7/18/1999
	// vector<string> strVecAlt;        //hold alternate string

	// ARVIND ADDED ON 7/18/1999
	std::vector<std::string> vecStrAlt;        //hold alternate string

	// ARVIND ADDED 7/18/1999:
	bool bIsOriginalAngleValid;  // to store the state of whether
	// the original string is valid.
};

