//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	BooleanExpression.cpp
//
// PURPOSE:	Parse a string containing the logic expression, and then evaluate the logic expression
//			
//
// NOTES:	
//
// AUTHORS:	Duan Wang (Sept, 2000 - Present)
//
//==================================================================================================

// TESTTHIS: this class was changed significantly during the port to VS2005
//			 thoroughly re-test this class.

#include "stdafx.h"
#include "BooleanExpression.h"
#include "cpputil.h"
#include "UCLIDException.h"
#include "UMapStrStr.h"
#include "UMapStrStrIter.h"

#include <algorithm>

vector<string> BooleanExpression::vecLogicOperators;
vector<string> BooleanExpression::vecFunctionNames;


//==================================================================================================
BooleanExpression::BooleanExpression(const string& strExpression)
{
	parse(strExpression);
}
//--------------------------------------------------------------------------------------------------
BooleanExpression::BooleanExpression(const BooleanExpression& booleanExpression)
{
	strLogicExpression = booleanExpression.strLogicExpression;
}
//--------------------------------------------------------------------------------------------------
BooleanExpression& BooleanExpression::operator=(const BooleanExpression& booleanExpression)
{
	strLogicExpression = booleanExpression.strLogicExpression;
	return *this;
}
//--------------------------------------------------------------------------------------------------
void BooleanExpression::parse(const string& strExpression)
{
	static bool bVectorsInitialized = false;
	if (!bVectorsInitialized)
	{
		//init the vec only once
		vecLogicOperators.clear();
		vecLogicOperators.push_back("AND");
		vecLogicOperators.push_back("OR");
		vecLogicOperators.push_back("XOR");
		vecLogicOperators.push_back("NOT");

		//vec for all available function calls in the logic expression
		// NOTE: the findMatch() method requires that all function names that
		// are substrings of each other appear in this vector in descending order
		// of length in the vecFunctionNames vector.  This is why isEquali is 
		// pushed to the vector before isEqual.
		vecFunctionNames.clear();
		vecFunctionNames.push_back("isEquali");
		vecFunctionNames.push_back("isEqual");
		bVectorsInitialized = true;
	}

	//remove all spaces before and after the actual content of the string
	strLogicExpression = trim(strExpression, " \t", " \t");

	//FIRST, check the validity of parenthesis
	//find the left most ")", and right most "("
	unsigned int uiRightParentPos = strLogicExpression.find_last_of(")");
	unsigned int uiLeftParentPos = strLogicExpression.find_first_of("(", 0);
	int iNumOfRightParent = 0;
	int iNumOfLeftParent = 0;
	if (uiLeftParentPos > uiRightParentPos)
	{
		throw UCLIDException("ELI00517", "Invalid logical expression!");
	}

	while (uiRightParentPos != string::npos && uiLeftParentPos != string::npos)
	{
		iNumOfRightParent++;
		iNumOfLeftParent++;

		uiRightParentPos = strLogicExpression.find_last_of(")", uiRightParentPos - 1);
		uiLeftParentPos = strLogicExpression.find_first_of("(", uiLeftParentPos + 1);
	}

	if ((uiRightParentPos != string::npos && uiLeftParentPos == string::npos)
				|| (uiRightParentPos == string::npos && uiLeftParentPos != string::npos))
	{
		throw UCLIDException("ELI00518", "Invalid number of parenthesis!");
	}

	if (iNumOfRightParent != iNumOfLeftParent)
	{
		UCLIDException uclidEx("ELI00520", "Invalid number of parenthesis!");
		uclidEx.addDebugInfo("iNumOfLeftParent", iNumOfLeftParent);
		uclidEx.addDebugInfo("iNumOfRightParent", iNumOfRightParent);
		throw uclidEx;
	}
	
	//SECOND, make sure the logic operators are valid
	//skip the "("
	unsigned int uiOperatorStartPos = strLogicExpression.find_first_not_of("(", 0);
	uiOperatorStartPos = strLogicExpression.find_first_not_of(" \t", uiOperatorStartPos);

	bool bFoundPrev = false;   //the previous variable
	bool bFoundCur = false;    // the one (current) right next to the previous
	while (uiOperatorStartPos != string::npos && uiOperatorStartPos < strLogicExpression.length())
	{
		unsigned int uiOperatorEndPos = uiOperatorStartPos;
		//validate the logical operators
		bFoundCur = findMatch(strLogicExpression, vecLogicOperators, uiOperatorEndPos);
		//if this is one of the logical operator
		if (bFoundCur)
		{
			//if there is not a space next to the end position, or this position
			//is not a "(", throw exception
			if (strLogicExpression[uiOperatorEndPos + 1] != ' ')
			{
				UCLIDException uclidEx("ELI00519", "Invalid logical operator!");
				uclidEx.addDebugInfo("strLogicExpression", strLogicExpression);
				throw uclidEx;
			}

			//and none of the logical operators can have a ")" right to it
			unsigned int uiRightCharPos = strLogicExpression.find_first_not_of(" \t", uiOperatorEndPos+1);
			if (strLogicExpression[uiRightCharPos] == ')')
			{
				UCLIDException uclidEx("ELI00523", "Invalid logical expression!");
				uclidEx.addDebugInfo("strLogicExpression", strLogicExpression);
				throw uclidEx;
			}

			//Only "NOT" can have a "(" left to it 
			unsigned int uiLeftCharPos = strLogicExpression.find_last_not_of(" \t", uiOperatorStartPos-1);
			if (strLogicExpression[uiLeftCharPos] == '(' 
				&& strLogicExpression.substr(uiOperatorStartPos, uiOperatorEndPos - uiOperatorStartPos+1) != "NOT")
			{
				UCLIDException uclidEx("ELI00522", "Invalid logical expression!");
				uclidEx.addDebugInfo("strLogicExpression", strLogicExpression);
				throw uclidEx;
			}
			//if the previous variable is a logical operator, and current is a logical operator,
			//in this scenario, only "NOT" will be allowed to follow any other logical operator
			if (bFoundPrev)
			{
				if (strLogicExpression.substr(uiOperatorStartPos, 
												uiOperatorEndPos - uiOperatorStartPos+1) != "NOT")
				{
					UCLIDException uclidEx("ELI00521", "Invalid sequence of logical operators!");
					uclidEx.addDebugInfo("strLogicExpression", strLogicExpression);
					throw uclidEx;
				}
			}
			
			uiOperatorStartPos = strLogicExpression.find_first_not_of(" \t", uiOperatorEndPos+1);
			uiOperatorStartPos = strLogicExpression.find_first_not_of("(", uiOperatorStartPos);
		}
		//find the next variable
		else
		{
			if (strLogicExpression[uiOperatorStartPos] != ' ' 
				&& strLogicExpression[uiOperatorStartPos] != '('
				&& strLogicExpression[uiOperatorStartPos] != ')')
			{
				//find the next very first space, and very first "(", take the nearest one
				unsigned int uiPos_1 = strLogicExpression.find_first_of(" \t", uiOperatorStartPos + 1);
				unsigned int uiPos_2 = strLogicExpression.find_first_of("(", uiOperatorStartPos + 1);
				unsigned int uiPos_3 = strLogicExpression.find_first_of(")", uiOperatorStartPos + 1);
				vector<int> vecPos;
				if (uiPos_1 != string::npos)
					vecPos.push_back(uiPos_1);
				if (uiPos_2 != string::npos)
					vecPos.push_back(uiPos_2);
				if (uiPos_3 != string::npos)
					vecPos.push_back(uiPos_3);
				if (vecPos.size() != 0)
				{
					//sort the vec ascendly
					sort(vecPos.begin(), vecPos.end());
					//take the smallest value
					uiOperatorStartPos = vecPos[0];
				}
				else
					uiOperatorStartPos = string::npos;
			}
			else
			{
				uiOperatorStartPos = strLogicExpression.find_first_not_of(" \t", uiOperatorStartPos+1);
				uiOperatorStartPos = strLogicExpression.find_first_not_of("(", uiOperatorStartPos);
				uiOperatorStartPos = strLogicExpression.find_first_not_of(")", uiOperatorStartPos);
			}
		}

		bFoundPrev = bFoundCur;
	}
}

//--------------------------------------------------------------------------------------------------
void BooleanExpression::LoadMap(UMapStrStr& src, map<string,string>& dst)
{
	string strKey;
	string strValue;
	UMapStrStrIter* pIter = src.CreateIter();
	while(pIter->FetchValuePair(&strKey,&strValue))
	{
		dst[strKey] = strValue;
	}
	delete pIter;
}

//--------------------------------------------------------------------------------------------------
string BooleanExpression::evaluate(UMapStrStr& mapStringString)
{
	if (strLogicExpression.length() == 0)
		throw UCLIDException("ELI00533", "Empty logical expression is not allowed!");
	
	//put the internal string into a temp string for evaluation and substitution
	string strExpression = strLogicExpression;

	map<string, string> mapData;
	LoadMap(mapStringString,mapData);

	//replace all function calls first
	replaceFunctionsInExpression(strExpression, mapData);

	//find the inner-most "("
	unsigned int uiLeftParentPos = strExpression.find_last_of("(");
	while (uiLeftParentPos != string::npos)
	{
		//the very next ")"
		unsigned int uiRightParentPos = strExpression.find_first_of( ")", uiLeftParentPos );
		if (uiRightParentPos == string::npos)
		{
			UCLIDException uclidEx("ELI00524", "Invalid logical expression!");
			uclidEx.addDebugInfo("strExpression", strExpression);
			throw uclidEx;
		}

		string strSubExpr = 
			strExpression.substr( uiLeftParentPos, uiRightParentPos - uiLeftParentPos + 1 );
		//substitute the sub logical expression with "1" or "0"
		string strReplacement = replaceString(strSubExpr, mapData);
		//replace the sub logical expression with the replacement 
		//(which should be either "1" or "0") within the strExpression
		replaceVariable(strExpression, strSubExpr, strReplacement);

		uiLeftParentPos = strExpression.find_last_of("(");
	}
	//no parenthesis
	while (strExpression.length() > 1)
	{
		strExpression = replaceString(strExpression, mapData);
	}

	return strExpression;	
}
//--------------------------------------------------------------------------------------------------
string BooleanExpression::replaceString(const string& strExpression, 
										const map<string, string> &mapData)
{
	//assume that strExpression doesn't have "(" ")" in it, but might have parenthesis on both sides
	string strTempExpr = trim(strExpression, "(", ")");
	strTempExpr = trim(strTempExpr, " \t", " \t");
	
	string strSubExpr = "";
	string strReplacement = "";

	//replace all NOT expressions
	while (strTempExpr.length() > 1)
	{
		//get the first pos of "NOT"
		unsigned int uiCurPos = strTempExpr.rfind(string("NOT"));
		unsigned int uiNextPos = 0;
		
		//there must be no alpha-numerics on both side of the "NOT"
		if (uiCurPos > 0)
		{
			if (isAlphaNumeric(strTempExpr, uiCurPos - 1) 
				|| isAlphaNumeric(strTempExpr, uiCurPos + 3))
				uiCurPos = strTempExpr.rfind(string("NOT"), uiCurPos - 1);
		}
		else if (uiCurPos == 0)
		{
			if (isAlphaNumeric(strTempExpr, uiCurPos + 3))
			{
				break;
			}
		}

		if (uiCurPos != string::npos)
		{
			//next variable must be a logical expression (ex. a boolean variable or "0" or "1")
			//find first space right after the end of the variable
			uiNextPos = strTempExpr.find_first_of(" \t", uiCurPos);
			//get the start position of the next variable
			uiNextPos = strTempExpr.find_first_not_of(" \t", uiNextPos);
			//if it's a "0" or "1"
			if (strTempExpr[uiNextPos] == '1'
				|| strTempExpr[uiNextPos] == '0')
			{
				strSubExpr = strTempExpr.substr( uiCurPos, uiNextPos - uiCurPos + 1 );
				strReplacement = replaceLogicExpressionWithString(strSubExpr);
				//replace all occurance of strSubExpr with strReplacement
				replaceVariable(strTempExpr, strSubExpr, strReplacement);
			}
			else  //it must be a boolean variable
			{
				//find the end pos of the boolean variable
				unsigned int uiBooleanVarEndPos = strTempExpr.find_first_of(" \t", uiNextPos) - 1;
				//replace the boolean variable with "0" or "1" first
				strSubExpr = strTempExpr.substr(uiNextPos, uiBooleanVarEndPos - uiNextPos + 1);
				map<string, string>::const_iterator iter;
				iter = mapData.find(strSubExpr);
				if (iter == mapData.end())
				{
					UCLIDException uclidEx("ELI00528", "No such boolean variable!");
					uclidEx.addDebugInfo("strSubExpr", strSubExpr);
					throw uclidEx;
				}
				strReplacement = iter->second;
				replaceVariable(strTempExpr, strSubExpr, strReplacement);
			}			
		}
		else
		{
			break;
		}
	}
	
	//replace all AND, OR, XOR expressions
	while (strTempExpr.length() > 1)
	{
		//assume that the first variable must be the boolean variable or value
		//replace the logical expression from left to right (no precedence among AND, OR, XOR)
		unsigned int uiCurPos = strTempExpr.find_first_of(" \t");
		if (uiCurPos != string::npos)
		{
			strSubExpr = strTempExpr.substr(0, uiCurPos);
			if (strSubExpr != "1" && strSubExpr != "0")
			{
				map<string, string>::const_iterator iter;
				iter = mapData.find(strSubExpr);
				if (iter == mapData.end())
				{
					UCLIDException uclidEx("ELI00532", "No such boolean variable!");
					uclidEx.addDebugInfo("strSubExpr", strSubExpr);
					throw uclidEx;
				}
				strReplacement = iter->second;
				replaceVariable(strTempExpr, strSubExpr, strReplacement);
			}
			else 
			{
				//pos of the next variable (should be the logical operator)
				uiCurPos = uiCurPos + 1;
				bool bFoundLogicOp = findMatch(strTempExpr, vecLogicOperators, uiCurPos);
				if (!bFoundLogicOp)
				{
					UCLIDException uclidEx("ELI00526", "Invalid logical expression!");
					uclidEx.addDebugInfo("strTempExpr", strTempExpr);
					throw uclidEx;
				}
				//start pos of the next variable right after the logical operator
				uiCurPos = uiCurPos + 2;
				unsigned int uiBoolVarEndPos = strTempExpr.find_first_of(" \t", uiCurPos);
				//reach the end of the strTempExpr
				if (uiBoolVarEndPos == string::npos && uiCurPos < strTempExpr.length())
				{
					strSubExpr = strTempExpr.substr(uiCurPos, strTempExpr.length() - uiCurPos);
					if (strSubExpr != "1" && strSubExpr != "0")
					{
						map<string, string>::const_iterator iter;
						iter = mapData.find(strSubExpr);
						if (iter == mapData.end())
						{
							UCLIDException uclidEx("ELI00527", "No such boolean variable!");
							uclidEx.addDebugInfo("strSubExpr", strSubExpr);
							throw uclidEx;
						}
						strReplacement = iter->second;
						replaceVariable(strTempExpr, strSubExpr, strReplacement);
					}
					else
					{
						//strTempExpr should look like "1 OR 0" etc
						strTempExpr = replaceLogicExpressionWithString(strTempExpr);
					}
				}
				else if (uiBoolVarEndPos != string::npos)
				{
					uiBoolVarEndPos --;
					strSubExpr = strTempExpr.substr(uiCurPos, uiBoolVarEndPos - uiCurPos + 1);
					if (strSubExpr != "1" && strSubExpr != "0")
					{
						map<string, string>::const_iterator iter;
						iter = mapData.find(strSubExpr);
						if (iter == mapData.end())
						{
							UCLIDException uclidEx("ELI00530", "No such boolean variable!");
							uclidEx.addDebugInfo("strSubExpr", strSubExpr);
							throw uclidEx;
						}
						strReplacement = iter->second;
						replaceVariable(strTempExpr, strSubExpr, strReplacement);
					}
					else
					{
						strSubExpr = strTempExpr.substr(0, uiBoolVarEndPos + 1);
						//strTempExpr should look like "1 OR 0" etc
						strReplacement = replaceLogicExpressionWithString(strSubExpr);
						replaceVariable(strTempExpr, strSubExpr, strReplacement);
					}

				}
				
			}
		}
		else  //strTempExpr length is greater than 1, it should be a boolean variable
		{
			map<string, string>::const_iterator iter;
			iter = mapData.find(strTempExpr);
			if (iter == mapData.end())
			{
				UCLIDException uclidEx("ELI00531","No such boolean variable!");
				uclidEx.addDebugInfo("strSubExpr", strSubExpr);
				throw uclidEx;
			}
			strTempExpr = iter->second;
		}
		
	}
	return strTempExpr;
}
//--------------------------------------------------------------------------------------------------
void BooleanExpression::replaceFunctionsInExpression(string& strExpression, 
													 const map<string, string> &mapData)
{

	unsigned int uiStartPos = strExpression.find_first_not_of(" \t");
	//skip the parenthesis
	while (strExpression[uiStartPos] == '(' || strExpression[uiStartPos] == ')')
	{
		uiStartPos = strExpression.find_first_not_of(" \t", uiStartPos + 1);
	}

	//replace all function calls within the logic expression
	while (uiStartPos != string::npos && uiStartPos < strExpression.length())
	{
		unsigned int uiEndPos = uiStartPos;
		bool bFoundFunction = findMatch(strExpression, vecFunctionNames, uiEndPos);
		if (bFoundFunction) 
		{
			//get the function name
			string strFunctionName = strExpression.substr(uiStartPos, uiEndPos - uiStartPos + 1);
			//find the left parent right after the function name
			unsigned int uiLeftParent = strExpression.find_first_not_of(" \t", uiEndPos + 1);
			if (uiLeftParent == string::npos || strExpression[uiLeftParent] != '(')
			{
				UCLIDException uclidEx("ELI00670", "Invalid function call within the logic expression!");
				uclidEx.addDebugInfo("strExpression", strExpression);
				throw uclidEx;
			}
			unsigned int uiRightParent = strExpression.find(")", uiLeftParent);
			if (uiRightParent == string::npos)
			{
				UCLIDException uclidEx("ELI00671", "Invalid function call within the logic expression!");
				uclidEx.addDebugInfo("strExpression", strExpression);
				throw uclidEx;
			}
			//there should not be empty between the parenthesis
			if (strExpression.find_first_not_of(" \t", uiLeftParent + 1) == uiRightParent)
			{
				UCLIDException uclidEx("ELI00672", "Invalid function call within the logic expression!");
				uclidEx.addDebugInfo("strExpression", strExpression);
				throw uclidEx;
			}

			//get the contents between the parenthesis
			string strSub = strExpression.substr(uiLeftParent + 1, uiRightParent - uiLeftParent - 1);
			unsigned int uiParamStart = 0;
			unsigned int uiParamEnd = 0;
			//vec for storing all parameters parsed out from within the parenthesis
			vector<string> vecParameters;
			
			//parameters are separated by commas
			unsigned int uiCommaPos = strSub.find(",");
			if (uiCommaPos != string::npos)
			{
				uiParamEnd = uiCommaPos - 1;
			}
			else
				uiParamEnd = strSub.length() - 1;
			
			while (uiParamEnd < strSub.length())
			{	
				string strParameter = trim(strSub.substr(uiParamStart, uiParamEnd - uiParamStart+1), " \t", " \t");
				
				//see if this parameter is a value (with quotes) or a variable (without quotes)
				unsigned int uiLen = strParameter.length();
				if (strParameter[0] == '\"' && strParameter[uiLen - 1] == '\"')
				{
					//strip off the commas
					strParameter = trim(strParameter, "\"", "\"");
					vecParameters.push_back(strParameter);
				}
				else if (strParameter[0] != '\"' && strParameter[uiLen - 1] != '\"')
				{
					map<string, string>::const_iterator iter;
					iter = mapData.find(strParameter);
					if (iter == mapData.end())
					{
						UCLIDException uclidEx("ELI00673", "No such boolean variable!");
						uclidEx.addDebugInfo("strParameter", strParameter);
						throw uclidEx;
					}
					strParameter = iter->second;
					vecParameters.push_back(strParameter);		
				}
				else
				{
					UCLIDException uclidEx("ELI00674", "Invalid function call within the logic expression!");
					uclidEx.addDebugInfo("strExpression", strExpression);
					throw uclidEx;
				}

				//find next parameter, if any
				uiParamStart = strSub.find_first_not_of(" \t", uiParamEnd + 2);
				if (uiParamStart != string::npos && uiParamStart < strSub.length())
				{
					uiCommaPos = strSub.find(",", uiParamStart);
					if (uiCommaPos != string::npos)
					{
						uiParamEnd = uiCommaPos - 1;
					}
					else
						uiParamEnd = strSub.length() - 1;
				}
				else  //no more parameters left
					uiParamEnd = strSub.length();
			}

			//replace the function call with a string value ("0" or "1")
			string strReplacement = getValueFromFunction(strFunctionName, vecParameters);
			replaceVariable(strExpression, 
				strExpression.substr(uiStartPos, uiRightParent - uiStartPos + 1), 
				strReplacement);

			// while continuing to search we need to account for the fact that the
			// string representing the function and its arguments may be different
			// from the size of the replacement string
			unsigned int uiLengthOfFunctionAndArguments = uiRightParent - uiStartPos+1;
			unsigned int uiCharsToMoveBackBy = uiLengthOfFunctionAndArguments-strReplacement.length();

			//search forward for another function call
			uiStartPos = strExpression.find_first_not_of(" \t", uiRightParent + 1 - uiCharsToMoveBackBy);
			//skip the parenthesis
			while (strExpression[uiStartPos] == '(' || strExpression[uiStartPos] == ')')
			{
				uiStartPos = strExpression.find_first_not_of(" \t", uiStartPos + 1);
			}
			
			continue;
		}
		else
		{
			unsigned int uiSpace = strExpression.find_first_of(" \t", uiStartPos + 1);
			if (uiSpace != string::npos)
			{
				//search forward for another function call
				uiStartPos = strExpression.find_first_not_of(" \t", uiSpace + 1);
				//skip the parenthesis
				while (strExpression[uiStartPos] == '(' || strExpression[uiStartPos] == ')')
				{
					uiStartPos = strExpression.find_first_not_of(" \t", uiStartPos + 1);
				}
				
			}
			else
				uiStartPos = strExpression.length();
			
			continue;
		}
	}
}
//--------------------------------------------------------------------------------------------------
string BooleanExpression::getValueFromFunction(const string& strFunctionName, 
							const vector<string>& vecParameters)
{
	//isEqual() is for case-sensitive comparison of two strings
	if (strFunctionName == "isEqual")
	{
		//validate the number of parameters.
		if (vecParameters.size() != 2)
		{
			UCLIDException uclidEx("ELI00675", "Function isEqual() has invalid number of parameters!");
			uclidEx.addDebugInfo("strLogicExpression", strLogicExpression);
			throw uclidEx;
		}
		if (vecParameters[0] == vecParameters[1])
			return "1";
		else
			return "0";
	}
	//case-insensitive comparison of two strings
	else if (strFunctionName == "isEquali")
	{
		//validate the number of parameters.
		if (vecParameters.size() != 2)
		{
			UCLIDException uclidEx("ELI00879", "Function isEquali() has invalid number of parameters!");
			uclidEx.addDebugInfo("strLogicExpression", strLogicExpression);
			throw uclidEx;
		}
		if (_strcmpi(vecParameters[0].c_str(), vecParameters[1].c_str()) == 0)
		{
			return "1";
		}
		else
		{
			return "0";
		}
	}
	else
	{
		THROW_LOGIC_ERROR_EXCEPTION("ELI00676");
	}
}
//--------------------------------------------------------------------------------------------------
//TODO: need optimize the algorithm
string BooleanExpression::replaceLogicExpressionWithString(const string& strExpression)
{
	//trim the string
	string strTempExpr = trim(strExpression, "(", ")");
	strTempExpr = trim(strTempExpr, " \t", " \t");

	if (strTempExpr == "NOT 0")
		return "1";
	else if (strTempExpr == "NOT 1")
		return "0";
	else if (strTempExpr == "0 AND 0")
		return "0";
	else if (strTempExpr == "0 AND 1")
		return "0";
	else if (strTempExpr == "1 AND 0")
		return "0";
	else if (strTempExpr == "1 AND 1")
		return "1";
	else if (strTempExpr == "0 OR 0")
		return "0";
	else if (strTempExpr == "0 OR 1")
		return "1";	
	else if (strTempExpr == "1 OR 0")
		return "1";
	else if (strTempExpr == "1 OR 1")
		return "1";
	else if (strTempExpr == "0 XOR 0")
		return "0";
	else if (strTempExpr == "0 XOR 1")
		return "1";
	else if (strTempExpr == "1 XOR 0")
		return "1";
	else if (strTempExpr == "1 XOR 1")
		return "0";
	else if (strTempExpr == "1" || strTempExpr == "0")
		return strTempExpr;
	else
		THROW_LOGIC_ERROR_EXCEPTION("ELI00529");
}
//--------------------------------------------------------------------------------------------------
//find the best match start from iPos in the string, 
//and return the pos of the end of the match string
bool BooleanExpression::findMatch(const string& strExpression, const vector<string> vecStr, unsigned int& uiPos)
{
	vector<string>::const_iterator ctIter;
	unsigned int uiStart = uiPos;
	unsigned int uiEnd = uiPos;
	unsigned int uiFound;
	bool bFound = false;

	for (ctIter = vecStr.begin(); ctIter != vecStr.end(); ctIter++)
	{
		if ((*ctIter) != "")
			uiFound = strExpression.find((*ctIter), uiStart);
		else
			continue;

		//there's a match, as long as it starts from the same pos as iPos
		if (uiFound != string::npos && uiFound == uiStart)
		{
			bFound = true;
			//need the longest string
			if((*ctIter).size() > (uiEnd - uiStart))
				//set iEnd to the end of the found string
				uiEnd = uiStart + ((*ctIter).size() - 1);
		}
	}

	//if the match string exists, and is longer than 1
	if ((uiEnd - uiStart)>0)
		//change the current position to ...
		uiPos = uiEnd;
	return bFound;
}
//--------------------------------------------------------------------------------------------------
