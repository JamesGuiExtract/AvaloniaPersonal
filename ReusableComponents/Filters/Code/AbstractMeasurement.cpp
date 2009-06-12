#include "stdafx.h"
#include "AbstractMeasurement.hpp"

#include <TPPoint.h>
#include <cpputil.h>
using namespace std;

//--------------------------------------------------------------------------------------------------
bool AbstractMeasurement::m_sbReverseMode = false;
bool AbstractMeasurement::m_sbPrevReverseMode = false;
CMutex AbstractMeasurement::m_sReverseModeMutex;

//--------------------------------------------------------------------------------------------------
AbstractMeasurement& AbstractMeasurement::operator=(const AbstractMeasurement& valueToAssign)
{
	vecInvalidPositions = valueToAssign.vecInvalidPositions;

	return *this;
}
//--------------------------------------------------------------------------------------------------
string AbstractMeasurement::checkTilde(string str)
{
	string strTemp("");
	int iLength = str.length();

	for(int i = 0; i < iLength; i++)
	{
		if (str[i] != '~')
		{
			strTemp += str[i];
		}
	}

	return strTemp;
}
//--------------------------------------------------------------------------------------------------
void AbstractMeasurement::preProcessString(string& strStringToProcess)
{
	//replace specia degree symbol with a "D"
	replaceVariable(strStringToProcess, "°", "D");	//unicode 0x00B0 ( == 176 <ANSI>)
	replaceVariable(strStringToProcess, "º", "D");	//unicode 0x00BA ( == 186 <ANSI>)
	replaceVariable(strStringToProcess, "ø", "D");	//unicode 0x00B0 ( == 248 <ASCII>)
	
	//replace special single quote
	replaceVariable(strStringToProcess, "‘", "'");	//(145 <ANSI>)
	replaceVariable(strStringToProcess, "’", "'");	//(146 <ANSI>)

	//replace special double quote
	replaceVariable(strStringToProcess, "“", "\"");	//(147 <ANSI>)
	replaceVariable(strStringToProcess, "”", "\"");	//(148 <ANSI>)

	replaceVariable(strStringToProcess, "''", "\""); // replace two single quotes with a double quote
	replaceVariable(strStringToProcess, "\"\"", "\""); // replace two double quotes with a double quote
	replaceVariable(strStringToProcess, "'\"", "\"");
	replaceVariable(strStringToProcess, "\"'", "\"");

	// Replace carriage returns with blank spaces
	replaceVariable(strStringToProcess, "\r", " ");
	replaceVariable(strStringToProcess, "\n", " ");
}
//--------------------------------------------------------------------------------------------------
void AbstractMeasurement::setInvalidPositions(const char *pszText)
{
	/////////////////////////////
	// Code commented out since 
	// vecInvalidPositions is not 
	// useful now - WEL 10/18/01
	/////////////////////////////
	/*
	// start with an empty vector of invalid positions
    vecInvalidPositions.clear();

    int iPos = 0;
    do
    {
		// get the next position of an invalid character
		iPos = getNextDelimiterPosition(pszText, iPos, "~()[]<>");
        if (iPos != -1) 
        {	// there is an invalid character
			// add its position to the vector and increment the index so we don't find the same one again
            vecInvalidPositions.push_back(iPos++);
        }
    }
    while (iPos != -1); 
	*/
}	
//--------------------------------------------------------------------------------------------------
vector<int> AbstractMeasurement::getInvalidPositions() const
{
    return vecInvalidPositions;
}
//--------------------------------------------------------------------------------------------------
AbstractMeasurement::EType AbstractMeasurement::guessType(const string &strText)
{
	string strWithoutTildas = checkTilde(strText);
	
	// commas and periods can occur within a single number, so strip them out to make it easier to calculate the number of numbers in the string
	replaceVariable(strWithoutTildas, ",", ""); 
	replaceVariable(strWithoutTildas, ".", "");

	int iAlphaPos, iNumberPos;
	iNumberPos = getPositionOfFirstNumeric(strWithoutTildas, 0);
	if (iNumberPos == -1)
	{
		// not even a single number in the string, so default to unknown
		return kUnknown; 
	}
	iAlphaPos = getPositionOfFirstAlpha(strWithoutTildas, iNumberPos+1);
	if (iAlphaPos == -1)
	{
		// no alpha characters after the first number character, so it must be a distance
		return kDistance;
	}
	// now find the NEXT number position after the first alpha position
	iNumberPos = getPositionOfFirstNumeric(strWithoutTildas, iAlphaPos+1);
	if (iNumberPos == -1)
	{
		// there is only one run of numeric characters in the string, must be a distance
		return kDistance;
	}
	
	// otherwise it's either a bearing or an angle, so search for n,s,e, and w characters
	if (strWithoutTildas.find('n') != -1 || strWithoutTildas.find('N') != -1 ||
		strWithoutTildas.find('s') != -1 || strWithoutTildas.find('S') != -1 ||
		strWithoutTildas.find('e') != -1 || strWithoutTildas.find('E') != -1 ||
		strWithoutTildas.find('w') != -1 || strWithoutTildas.find('W') != -1)
	{
		// one of the characters was found, so it must be a bearing
		return kBearing;
	}
	return kAngle;
}
//--------------------------------------------------------------------------------------------------
void AbstractMeasurement::evaluate(const TPPoint& p1, const TPPoint& p2)
{
}
//--------------------------------------------------------------------------------------------------


