//==================================================================================================
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	COGOInputInterpreter.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Duan Wang
//
//==================================================================================================

#include "stdafx.h"
#include "COGOInputInterpreter.h"

#include <IcoMapOptions.h>

#include <StringTokenizer.h>
#include <UCLIDException.h>
#include <cpputil.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

using namespace std;

//--------------------------------------------------------------------------------------------------
string COGOInputInterpreter::interpretCOGOInput(const string &strInput, EInputType eInputType)
{
	string strInterpretedValue(strInput);
	
	switch (eInputType)
	{
	case kBearing:
		{
			strInterpretedValue = interpretBearing(strInput);
		}
		break;
	case kAngle:
		{
			strInterpretedValue = interpretAngle(strInput);
		}
		break;
	default:
		break;
	}
	
	return strInterpretedValue;
}
////////////////////////////
// helper functions
//--------------------------------------------------------------------------------------------------
bool COGOInputInterpreter::isNumeric(const std::string& strInput)
{
	int nSize = strInput.size();

	if (nSize == 0)
	{
		return false;
	}

	// if all characters are numeric digits
	for (unsigned int i=0; i < strInput.size(); i++)
	{
		if (strInput[i] > '9' || strInput[i] < '0')
		{
			return false;
		}
	}

	return true;
}
//--------------------------------------------------------------------------------------------------
string COGOInputInterpreter::interpretBearing(const string &strInput)
{
	string strToReturn(strInput);
	string strBearing(strInput);

	if (!strBearing.empty())
	{
		// extract the first character out from the original input and 
		// look up in the persistence store for its meaningful substitue
		string strBearingDirection(IcoMapOptions::sGetInstance().getDirection(strBearing[0]));
		if (strBearingDirection.empty())
		{
			// return the original input if can't find any
			return strInput;
		}

		// remove the first char
		strBearing.erase(0, 1);

		// if first char has some meaning...
		int nLen = strBearingDirection.size();
		switch (nLen)
		{
		case 1:	// only has start direction (i.e. N, W, S, E)
			{
				// if there's no more characters following the start direciton
				if (strBearing.empty())
				{
					return strBearingDirection;
				}
				else
				{
					return strInput;
				}
			}
			break;
		case 2:	// start direction and due direciton (i.e. NW, SW, NE, SE)
			{
				// take a look at the rest of the string
				string strTemp = interpretNumberIntoAngle(strBearing, k222Rule);
				if (_stricmp(strTemp.c_str(), strBearing.c_str()) == 0)
				{
					return strInput;
				}

				strToReturn = strBearingDirection[0] + strTemp + strBearingDirection[1];
			}
			break;
		default:
			return strInput;
		}
	}

	return strToReturn;
}
//--------------------------------------------------------------------------------------------------
string COGOInputInterpreter::interpretAngle(const string &strInput)
{
	return interpretNumberIntoAngle(strInput, k322Rule);
}
//--------------------------------------------------------------------------------------------------
string COGOInputInterpreter::interpretNumberIntoAngle(const string& strNumber, EParseRule eRule)
{
	string strToReturn(strNumber);

	// only supports at most ONE decimal point
	if (!strToReturn.empty() && strToReturn.find_first_of(".") == strToReturn.find_last_of("."))
	{
		// the integer part of string right before the decimal point (if any)
		string strIntegerPart(strToReturn);
		string strDecimalPart("");
		int nDecimalPos = strToReturn.find_first_of(".");
		if (nDecimalPos != string::npos)
		{
			// the integer part of string (doesn't include decimal point)
			strIntegerPart = strToReturn.substr(0, nDecimalPos);
			// the decimal part of string (includes the decimal point)
			strDecimalPart = strToReturn.substr(nDecimalPos);
		}
		
		// all character must be numeric except the decimal point
		// and the integer part string length shall be 7 at most
		if (isNumeric(strIntegerPart) && strIntegerPart.size() <= 7)
		{
			// if the decimal part of string (except the decimal point) 
			// contains none numeric character, just return the original strNumber
			if (!strDecimalPart.empty() && !isNumeric(strDecimalPart.substr(1)))
			{
				return strNumber;
			}

			string strDegrees("0");
			string strMinutes("0");
			string strSeconds("0");
			
			// 3-2-2 rule or 2-2-2 rule
			int nDigitsForDegrees = 3;
			int nDigitsForMinutes = 2;

			switch (eRule)
			{
			case k322Rule:	// 3-2-2 rule is usually for angles only
				break;
			case k222Rule:	// 2-2-2 rule is usually for bearings only
				{
					// if 2-2-2 rule is applied, integer part of the string
					// length shall be 6 at most
					if (strIntegerPart.size() > 6)
					{
						return strNumber;
					}

					nDigitsForDegrees = 2;
				}
				break;
			default:
				return strNumber;
			}

			if (!strIntegerPart.empty())
			{
				// get the degrees part
				strDegrees = strIntegerPart.substr(0, nDigitsForDegrees);
				// remove the degrees part from integer part
				strIntegerPart.erase(0, nDigitsForDegrees);
				if (!strIntegerPart.empty())
				{
					// get the minutes part
					strMinutes = strIntegerPart.substr(0, nDigitsForMinutes); 
					// remove the minutes part from integer part
					strIntegerPart.erase(0, nDigitsForMinutes);
					if (!strIntegerPart.empty())
					{
						strSeconds = strIntegerPart.substr(0) + strDecimalPart;
					}
					else
					{
						// TODO : Once filter supports decimal degrees and decimall minutes,
						// uncomment the following line
						// strMinutes += strDecimalPart;

						// TODO : remove the following line once filter supports decimal
						// degrees and decimal minutes
						if (!strDecimalPart.empty())
						{
							return strNumber;
						}
					}
				}
				else
				{
					// TODO : Once filter supports decimal degrees and decimall minutes,
					// uncomment the following line
					// strDegrees += strDecimalPart;

					// TODO : remove the following line once filter supports decimal
					// degrees and decimal minutes
					if (!strDecimalPart.empty())
					{
						return strNumber;
					}
				}
			}

			strToReturn = strDegrees + "d" + strMinutes + "m" + strSeconds + "s";
		}
	}

	return strToReturn;
}
//--------------------------------------------------------------------------------------------------
void COGOInputInterpreter::interpretPointInput(const string &strInput, double &dX, double &dY)
{
	vector<string> vecTokens;
	StringTokenizer tokenizer;
	tokenizer.parse(strInput, vecTokens);
	if (vecTokens.size() == 2)
	{
		// Extract X
		dX = asDouble( vecTokens[0] );
		
		// Extract Y
		dY = asDouble( vecTokens[1] );
	}
	else
	{
		UCLIDException uclidException("ELI02196", "Invalid input of a point!");
		uclidException.addDebugInfo("Input point", strInput);
		throw uclidException;
	}
}