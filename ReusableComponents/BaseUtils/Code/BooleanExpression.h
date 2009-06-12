//==================================================================================================
//
// COPYRIGHT (c) 2000 - 2008 EXTRACT SYSTEMS LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	BooleanExpression.h
//
// PURPOSE:	Parse a string containing the logic expression, and then evaluate the logic expression
//			
//
// NOTES:	
//
// AUTHORS:	Duan Wang (Sept, 2000 - Present)
//
//==================================================================================================
#pragma once

#include "BaseUtils.h"

#include <string>
#include <vector>
#include <map>

class UMapStrStr;

using namespace std;


//==================================================================================================
//
// CLASS:	BooleanExpression
//
// PURPOSE:	Parse a string containing the logic expression, and then evaluate the logic expression
//
// REQUIRE:	
//
// INVARIANTS:
//			
// EXTENSIONS:
//			
// NOTES:	
//
//==================================================================================================
class EXPORT_BaseUtils BooleanExpression
{
public:
	//----------------------------------------------------------------------------------------------
	// PURPOSE: Default constructor
	//
	// REQUIRE: 
	//
	// PROMISE: 
	//
	// ARGS:	
	//
	BooleanExpression(){}
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To allow user directly pass in the expression without calling parse() method
	//
	// REQUIRE: Call parse() method within the constructor
	//
	// PROMISE: 
	//
	// ARGS:	strExpression: The string format of the logic expression
	//
	BooleanExpression(const string& strExpression);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To allow copy construction of BooleanExpression object
	//
	// REQUIRE: 
	//
	// PROMISE: To construct a new BooleanExpression object initialized to the same state as the
	//			BooleanExpression object being copied.
	//
	// ARGS:	booleanExpression: The object to be copied
	//
	BooleanExpression(const BooleanExpression& booleanExpression);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To allow assignment of one BooleanExpression object to another.
	//
	// REQUIRE: 
	//
	// PROMISE: To ensure that the data associated with this object is the same as the data
	//			of the assigned object.
	//
	// ARGS:	booleanExpression: A copy of whose data is to be assigned to the object
	//
	BooleanExpression& operator=(const BooleanExpression& booleanExpression);	
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To parse the logic expression and ensure the validity of the expression
	//
	// REQUIRE: 
	//
	// PROMISE: Ensure that the logic expression is valid
	//
	// ARGS:	strExpression: The string format of the logic expression
	//
	void parse(const string& strExpression);

	//----------------------------------------------------------------------------------------------
	// PURPOSE: To evaluate the valid logic expression and substitute the valid boolean variable 
	//			and logic operators with appropriate value (obtained from the pass-in map parameter).
	//
	// REQUIRE: All boolean variables contained in strLogicExpression must have related value 
	//			in mapData
	//
	// PROMISE: Return the result of the logic expression in string ("1" or "0")
	//
	// ARGS:	mapData: Contains values for the boolean variables that will be used in boolean 
	//					 expression substitution
	//
	string evaluate(UMapStrStr& mapStringString);

private:
	//----------------------------------------------------------------------------------------------
	//string containing the valid logic expression to be evaluated
	string strLogicExpression;
	//----------------------------------------------------------------------------------------------
	//vector containing all possible logical operators, ex. AND, OR, etc
	static vector<string> vecLogicOperators;
	//----------------------------------------------------------------------------------------------
	//vector containing all function call names, ex. isEqual, etc.
	static vector<string> vecFunctionNames;
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To replace the pass-in logic expression with a boolean value in string format
	//
	// REQUIRE: 
	//
	// PROMISE: Return the replacement string (must be either "0" or "1")
	//
	// ARGS:	strExpression: The string needs to be replaced
	//
	string replaceLogicExpressionWithString(const string& strExpression);	
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To replace the strExpression with "1" or "0"
	//
	// REQUIRE: strExpression doesn't have any parenthesis
	//
	// PROMISE: Return the replacement string (either "0" or "1")
	//
	// ARGS:	strExpression: The string needs to be replaced
	//			mapData: contains boolean variables' value (in string format)
	//
	string replaceString(const string& strExpression, const map<string, string> &mapData);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To replace the functions with "1" or "0"
	//
	// REQUIRE: 
	//
	// PROMISE: Return replaced function calls with strings (either "0" or "1")
	//
	// ARGS:	strExpression: expression to be replaced
	//			mapData: contains boolean variables' value (in string format)
	//
	void replaceFunctionsInExpression(string& strExpression, const map<string, string> &mapData);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To replace the functions with "1" or "0"
	//
	// REQUIRE: validate # of parameters for a certain function
	//
	// PROMISE: Return the value in string (either "0" or "1")
	//
	// ARGS:	strFunctionName: the name of the function call
	//			vecParameters: parameters for the function
	//
	string getValueFromFunction(const string& strFunctionName, const vector<string>& vecParameters);
	//----------------------------------------------------------------------------------------------
	// PURPOSE: Find a match logical operator within the pass-in string start from the iPos
	//
	// REQUIRE: 
	//
	// PROMISE: Return true if found and iPos will return the end position for the found operator
	//			in the string
	//
	// ARGS:	strExpression: The string for search
	//			vecStr: vector containing all logical operators
	//			iPos: the start position for search
	//
	bool findMatch(const string& strExpression, const vector<string> vecStr, unsigned int& uiPos);

	//----------------------------------------------------------------------------------------------
	// PURPOSE:	Copies the contents of a UMapStrStr object to an STL map
	//
	// REQUIRE: 
	//
	//
	// ARGS:	
	//			src		[in]	the UMapString object to copy from
	//			dst		[out]	the STL map to copy to
	//
	void LoadMap(UMapStrStr& src, map<string,string>& dst);

};
