#pragma once

#include "stdafx.h"
#include "BaseUtils.h"
#include "cpputil.h"
#include "UCLIDException.h"

#include <string>
#include <vector>

class EXPORT_BaseUtils LevenshteinDistance
{
public:
	LevenshteinDistance();
	~LevenshteinDistance();

	//=======================================================================
	// PURPOSE: Given 3 booleans, this method will set the variables to 
	//				remove whitespace, check for case, and update Text Boxes.
	// REQUIRE: 3x bool 
	// PROMISE: will set the values in the LevenshteinDistance object that calls
	// ARGS:	-bRemWS: true removes whitespace. This includes \r\n\t and " "
	//			-bCaseSensitive: true compares the strings with their current case
	//			-bUpdate: true updates the parameters passed to GetDistance & GetPercent to 
	//          reflect the above changes.
	void SetFlags(bool bRemWS, bool bCaseSensitive, bool bUpdate = false);

	//=======================================================================
	// PURPOSE: Given 2 strings, this method will calculate the Levenshtein Distance 
	//				between the 2 of them.
	// REQUIRE: 2x string 
	// PROMISE: Provides an integer that is the measure the Levenshtein Distance
	// ARGS:	2 strings to find the Levenshtein distance of
	int GetDistance(string& strExpected, string& strFound);

	//=======================================================================
	// PROMISE: Returns the percentage of difference between the two strings.
	double GetPercent(string& strExpected, string& strFound);

private:
	//member variables
	int m_iExpectedLen;
	int m_iFoundLen;
	bool m_bUpdate;
	bool m_bCaseSensitive;
	bool m_bRemWS;
};
