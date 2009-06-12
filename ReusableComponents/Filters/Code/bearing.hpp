//===================================================================
//COPYRIGHT UCLID SOFTWARE, LLC. 1998
//
//FILE:		bearing.hpp
//
//PURPOSE:	Bearing string filter, evalute the bearing string
//
//NOTES:
//
//AUTHOR:	Jinsong Ye
//
//===================================================================
//
//===================================================================
#pragma once

#include "FiltersDLL.h"

#include "AbstractMeasurement.hpp"

#include <math.h>
#include <vector>
#include <afxmt.h>

class TPPoint;

class EXT_FILTERS_DLL Bearing : public AbstractMeasurement
{
public:

	// this defaults to false.  it was added for performance reasons in the fingerprint duster
	// setting it to true will disable alternate strings, and make the behavior of functions related
	// to alternate strings undefined.
	bool m_bSuppressAlternates;
	
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
	// Purpose: To initialize this instance of the bearing class and set its state to "invalid"
	//
	// Require: Nothing.
	//
	// Promise: isValid() will return false
	Bearing();
	//----------------------------------------------------------------------------------------------
	//Purpose:  copy constructor
	//
	//Require: Nothing.
	//
	//Promise: deep copy
	Bearing(const Bearing& bearing);
	//----------------------------------------------------------------------------------------------
	// Purpose: To initialize this instance of the bearing class.
	//
	// Require: Nothing.
	//
	// Promise: The same results as the evaluate() method.
	Bearing(const char *pszBearing);
	//----------------------------------------------------------------------------------------------
	// Purpose: To clean up this instance of the bearing class.
	//
	// Require: Nothing.
	//
	// Promise: To clean up any memory allocated by this instance of the bearing class.
	~Bearing();
	//----------------------------------------------------------------------------------------------
	//Purpose: Overload equal operator
	//
	//Require: Nothing
	//
	//Promise: Return bearing = b
	Bearing& operator=(const Bearing& b);
	//-----------------------------------------------------------------------------------
	// Purpose: To create a new instance of the Bearing object.
	//
	// Require: Nothing.
	//
	// Promise: Returns a new Bearing object.  The caller is responsible for deleting the
	//			newly created object.
	AbstractMeasurement* createNew();
	//----------------------------------------------------------------------------------------------
	//Purpose: To set private member to default value
	//
	//Require: Nothing
	//
	//Promise: Set variables to default value
	void resetVariables();
	//-----------------------------------------------------------------------------------
	// Purpose: To initialize this instance of the bearing class.
	//
	// Require: Nothing.
	//
	// Promise: If pszBearing is a valid bearing string, then isValid() shall return true and
	//			getDegrees() and getBearingString() shall return the 
	//			correct values as indicated by the bearing string.
	//			If pszBearing is not a valid bearing string, then isValid() shall return false.
	void evaluate(const char *pszBearing);
	//----------------------------------------------------------------------------------------------
	// Purpose: To initialize this instance of the bearing class.
	//
	// Require: Nothing.
	//
	// Promise: To convert the specified radians into an internal bearing.  isValid() will return 
	//			true.
	void evaluateRadians(const double& dRadians);
	//----------------------------------------------------------------------------------------------
	// Purpose: To initialize this instance of the Bearing class.
	//
	// Require: p1 != p2.
	//
	// Promise: To associate the bearing from p1 to p2 as the "bearing value" associated with
	//			this Bearing object.
	void evaluate(const TPPoint &p1, const TPPoint& p2);
	//----------------------------------------------------------------------------------------------
	// Purpose: To determine the internal status of this instance of the bearing class.
	//
	// Require: Nothing.
	//
	// Promise: To return true, if a valid bearing string has been passed to either the constructor
	//			or the evaluate function and if the string has been successfully parsed into its
	//			sub-components (the origin direction, the degrees, minutes, and seconds, and the
	//			the end direction).  Promise to return false otherwise.
	bool isValid(void);
	//----------------------------------------------------------------------------------------------
	// Purpose: If the bearing string that was evaluated was not valid, then this function should
	//			return a list of possible alternate strings that would make the original bearing
	//			string valid.  These options will be shown to the user in a dialog box, and the
	//			user will be requested to pick from one of these options.
	//
	// Require: Nothing.
	//
	// Promise: If isValid() is true, then the vector must contain one string which has the value
	//			getBearingString().  If isValid() is false, then the vector should contain possible
	//			alternatives that would make the entered bearing string valid (This method shall
	//			therefore take into account all the different spots where a typo could have been 
	//			made or the OCR program could have recognized the bearing wrongly.)
	std::vector<std::string> getAlternateStrings(void);
	//----------------------------------------------------------------------------------------------
	// Purpose: To return the degrees that represent this instance of a bearing.
	//
	// Require: isValid() == true;
	// 
	// Promise: To return the degrees that represent this instance of a bearing if isValid()==true.
	//			For instance, if the bearing string is N45"00'00E, then the representative degrees
	//			is 45.00; if the bearing string is N05"05'00W, then the representative degrees
	//			is 90 + 5 + 5/60 = 95.083333; if the bearing string is S30"00'00E, then the
	//			representative degrees is 270 + 30 = 300.00, etc.
	//			If isValid() == false, then promise to throw string("ERROR: Invalid bearing!");
	double getDegrees(void);
	//----------------------------------------------------------------------------------------------
	// Purpose: To return the radians that represent this instance of a bearing.
	//
	// Require: isValid() == true;
	// 
	// Promise: To return the radians that represent this instance of a bearing if isValid()==true.
	//			For instance,  
	//			If isValid() == false, then promise to throw string("ERROR: Invalid bearing!");
	double getRadians(void);
	//----------------------------------------------------------------------------------------------
	// Purpose: To return this instance of a bearing in standard bearing string form such as
	//			N32"44'00E.
	//
	// Require: isValid() == true;
	// 
	// Promise: To return this instance of a bearing in a standard bearing string format if 
	//			isValid() == true;
	//			If isValid() == false, then promise to throw string("ERROR: Invalid bearing!");
	//
	// Note:	The return value will be the user entered value in string format no matter what mode
	//			it is in.
	//			For instance, the input bearing is N32D44'00E, and reverse mode is true, 
	//			the interpreted output value for the bearing is S32D44'00"W. This method will return 
	//			N32D44'00E as string format for bearing.
	//
	std::string asString(void);
	//----------------------------------------------------------------------------------------------
	// Purpose: To return this instance of the bearing class in a string format which reflects 
	//			the actual interpreted value output.
	//
	// Require: isValid() == true;
	// 
	// Promise: To return this instance of the measurement class in a standard string format if 
	//			isValid() == true;
	//			If isValid() == false, then promise to throw string("ERROR: Invalid angle!");
	// Note:	The return value will be the interpreted value of the entered value in string 
	//			format no matter what mode it is in.
	//			For instance, the input bearing is N32D44'00E, and reverse mode is true, 
	//			the interpreted output value for the bearing is S32D44'00"W. This method will return 
	//			S32D44'00"W as string format for bearing.
	//
	std::string interpretedValueAsString(void);
	//----------------------------------------------------------------------------------------------
	// Purpose: 
	//
	// Require: 
	// 
	// Promise: 
	std::string getEvaluatedString(void);
	//----------------------------------------------------------------------------------------------
	// Purpose: get the start direction strings, ex. "North", "S", etc.
	//
	// Require: N/A
	// 
	// Promise: N/A
	const std::vector<std::string>& getStartDirStrings() const {return m_svecStrStartUnit;}
	//----------------------------------------------------------------------------------------------
	// Purpose: get the end direction strings, ex. "west", "east"
	//
	// Require: N/A
	// 
	// Promise: N/A
	const std::vector<std::string>& getEndDirStrings() const {return m_svecStrEndUnit;}

protected:
	
	std::string m_strEvaluatedString;

	// include any necessary protected methods/attributes here.
	//-----------------------------------------------------------------------------------------
	// Purpose: Split the bearing string to three parts. Start direction, Middle part, End direction
	//
	// Require: Nothing
	//
	// Promise: Divided the bearing string to three parts
	void tokenString();
	//-----------------------------------------------------------------------------------------
	//Purpose: Check the input bearing string
	//
	//Require: After tokenString().
	//
	//Promise: Get all information from the bearing string
	void checkString();
	//------------------------------------------------------------------------------------------
	//Purpose: Translate the bearing string to standard string, also get the information
	//
	//Require: Call by checkString().
	void translateString();
	//------------------------------------------------------------------------------------------
	//Purpose: Get the degree of pure direction
	//
	//Require: bValidPure is true
	//
	//Promise: Return the degree
	double getPureDegrees();
	//------------------------------------------------------------------------------------------
	// Purpose: Convert ^^D^^'^^" to angle ^^^^D
	//
	// Require: Nothing
	//
	// Promise: if string is valid, convert the bearing angle to AutoCad angle
	//			otherwise return -1

	//????char pszBearing[1000];
	double convert_angle(double d, double m, double s);
	//-------------------------------------------------------------------------------------------
	//Purpose: Set valid unit of start dir, end dir, degree, minute, second
	//
	//Require: Nothing
	//
	//Promise: Set valid unit to start_dir vector, end_dir vector, degree vector,
	//         minute vector, second vector 
	void setUnit();
	//---------------------------------------------------------------------------------------------
	//Purpose: Translate angle to the bearing string style
	//
	//Require: Pass double angle
	//
	//Promise: Get degree string, minute string, second string
	void radiansToString(double angle);
	//---------------------------------------------------------------------------------------------
	//Purpose: Set alternate string vector
	//
	//Require: Nothing
	//
	//Promise: Set alternate string vector, if no alternate string, push input string
	void setAltVec(std::string str1, std::string str2, std::string str3);
	//---------------------------------------------------------------------------------------------
	//Purpose: Set standard string
	//
	//Require: Nothing
	//
	//Promise: Set standard string
	void setStandString();
	//---------------------------------------------------------------------------------------------
	//Purpose: Filter char, ex 'L' change to '1', 's' change to '5'
	//
	//Require: Passin one char
	//
	//Promise: Return correct char
	char filterChar(char ch);
	//---------------------------------------------------------------------------------------------
	//Purpose: Check the degree, minute, second strings, the first char
	//			of the string should not great than 6. if it's great than 
	//          6, change the char. 6-5, 7-1, 8-3, 9-4,
	//Require: Pass string&
	//
	//Promise: First char of degree string is less than 6
	void changeChar(std::string& str);
	//---------------------------------------------------------------------------------------------
	//Purpose: Truncate double value Angle to standard format.
	//         ex. 3.14159 - 3.14 (only two digits after decimal point
	//          
	//
	//Require: Pass in double value
	//
	//Promise: Get standard format double value 
	double truncateRadians(double dValue);
	//---------------------------------------------------------------------------------------------
	//Purpose: Scan bearing string convert some confuse char ex. B should be 8
	//
	//Require: nothing
	//
	//Promise: put new Alternate string to the alternate string vector
	void moreAlternateString();

	//---------------------------------------------------------------------------------------------
	bool inEndVector(const std::string& vecStr);

private:
	double m_dRad;  //radians value 
	double m_dDeg;      //deg value
	 
	//double value for degree, minute, second
	double   m_dDegree, m_dMinute, m_dSecond;

	//start dir and end dir for standard string. ex. N^^^^'^^W
	char     m_chStartDir, m_chEndDir;

	bool m_bValid;         //for bearing string
	bool m_bPureFlag;      //for pure string, maybe not pure dir ("hello" is not dir but pure string)
	bool m_bValidPure;     //for pure direction, east ...
	bool m_bPossibleFlag;  //start dir or end dir is ""
	bool m_bStartOption;   //start dir is ""
	bool m_bEndOption;     //end dir is ""

	//example:  " North 35 deg 12' 24" W "
	//split string to three parts
	std::string   m_strStartDir;    //start direction string " North "
	std::string   m_strEndDir;      //end direction string   " W "
	std::string   m_strMidPart;     //middle part string     " 35deg12'34" "

	std::string   m_strPure;        //pure string " Hello "

	std::string   m_strBearString;  //input bearing string     " North 35 deg 12' 24" W "
	std::string   m_strStandString; //standard bearing string  " N35D12'24"W "

	//
	std::string   m_strDegString;   //degree string (35 is string)
	std::string   m_strMinString;   //minute string (12 is string)
	std::string   m_strSecString;   //second string (24 is string)
	std::string   m_strDegUnit;     //degree unit string " deg "
	std::string   m_strMinUnit;     //minute unit string "  '  "
	std::string   m_strSecUnit;     //second unit string "  "  "
	
	//vector is like library, hold the valid units for start, end, degree, minute, second
	static std::vector<std::string> m_svecStrStartUnit;  //vector hold all valid start direction "North" "nor"...
	static std::vector<std::string> m_svecStrEndUnit;    //vector hold all valid end direction   "east" "E"....
	static std::vector<std::string> m_svecStrDegUnit;    //vector hold all valid degree unit " deg ", "D" ...
	static std::vector<std::string> m_svecStrMinUnit;    //vector hold all valid minute unit " min ", minutes"
	static std::vector<std::string> m_svecStrSecUnit;    //vectro hold all valid second unit " SECOND " "s"..
	static std::vector<std::string> m_svecStrPure;       //vector hold all valid pure direction "east" "W" "nor"...
	
	static CMutex m_sMutex;

	std::vector<std::string> m_vecStrAlt;		 //vector hold alternate string.

	// ARVIND ADDED 7/18/1999:
	bool m_bIsOriginalBearingValid;  // to store the state of whether
	// the original string is valid.
};
