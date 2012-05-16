#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	COGOInputInterpreter.h
//
// PURPOSE:	To interpret COGO input according to the input type, and convert the input properly
//
// NOTES:	
//
// AUTHORS: Duan Wang (08/2001)
//
//==================================================================================================
#include <EInputType.h>

#include <string>

class COGOInputInterpreter
{
public:
	std::string interpretCOGOInput(const std::string &strInput, EInputType eInputType);
	// parse the string into x and y coordinate
	void interpretPointInput(const std::string &strInput, double &dX, double &dY);

protected:
	// rule to parse a number string
	// k322Rule - 123456 will be parsed into 123, 45 and 6
	// k222Rule - 123456 will be parsed into 12, 34 and 56
	enum EParseRule{kNoRule=0, k322Rule, k222Rule};

	std::string interpretBearing(const std::string &strInput);
	std::string interpretAngle(const std::string &strInput);
	// determine whether or not the input string has all numeric characters
	bool isNumeric(const std::string& strInput);
	// parse input number string into an angle according to the parse rule
	// ex. if k322Rule, "123456" will be interpreted as "123d45m6s"; "0123456.1254"
	// will be "012d34m56.1254s"
	std::string interpretNumberIntoAngle(const std::string& strNumber, EParseRule eRule);

};
