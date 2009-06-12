#include "LevenshteinDistance.h"

//-------------------------------------------------------------------------------------------------
// LevenshteinDistance.cpp 
// Includes algorithm taken from: http://www.merriampark.com/ldcpp.htm
//-------------------------------------------------------------------------------------------------
LevenshteinDistance::LevenshteinDistance(void)
: m_iExpectedLen(0),
  m_iFoundLen(0),
  m_bUpdate(false),
  m_bCaseSensitive(false),
  m_bRemWS(false)
{
}
//-------------------------------------------------------------------------------------------------
LevenshteinDistance::~LevenshteinDistance(void)
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16387");
}
//-------------------------------------------------------------------------------------------------
void LevenshteinDistance::SetFlags(bool bRemWS, bool bCaseSensitive, bool bUpdate)
{
	m_bRemWS = bRemWS;
	m_bCaseSensitive = bCaseSensitive;
	m_bUpdate = bUpdate;
	return;
}
//-------------------------------------------------------------------------------------------------
int LevenshteinDistance::GetDistance(string& strExpectedInc, string& strFoundInc) 
{
	//make a copy to manipulate
	string strExpected(strExpectedInc);
	string strFound(strFoundInc);

	//If Case Sensitive Box is checked, uppercase both strings.
	if(!m_bCaseSensitive)
	{
		makeUpperCase(strExpected);
		makeUpperCase(strFound);
	}

	//If Remove Whitespace box is checked, Remove the WS.
	if(m_bRemWS)
	{
		//trim white space on left and right.
		strExpected = trim(strExpected, "  \r\t\n", "  \r\t\n");
		strFound = trim(strFound, "  \r\t\n", "  \r\t\n");

		//remove all the other white spaces
		strExpected = replaceMultipleCharsWithOne(strExpected, "\r\n\t ", " ", 1);
		strFound = replaceMultipleCharsWithOne(strFound, "\r\n\t ", " ", 1);
	}

	//If True Boxes enabled, update the boxes.
	if (m_bUpdate)
	{
		strExpectedInc = strExpected;
		strFoundInc = strFound;
	}
	m_iExpectedLen = static_cast<int> (strExpected.size());
	m_iFoundLen = static_cast<int> (strFound.size());

	//The following algorithm taken from: http://www.merriampark.com/ldcpp.htm
	// by Ryan Mulder July 12, 2006.
	//step 1
	int n = static_cast<int> (strExpected.length());
	int m = static_cast<int> (strFound.length());

	if(n == 0)
	{
		return m;
	}
	if(m == 0)
	{
		return n;
	}

	//typedef to make a 2-D vector that will act like a 2D array.
	typedef vector< vector<int> > Tmatrix; 
	Tmatrix vecMatrix(n+1);
	
	//size the vector
	for (int i = 0; i <= n; i++) 
	{
		vecMatrix[i].resize(m+1);
	}

	//step 2: Initialize the first row to 0...m
	//		  and the first column to 0...n
	for (int i = 0; i <= n; i++) 
	{
		vecMatrix[i][0]=i;
	}
	for (int j = 0; j <= m; j++) 
	{
		vecMatrix[0][j]=j;
	}

	//grab characters and compare them
	for (int i = 1; i <= n; i++) 
	{
	    const char s_i = strExpected[i-1];
	    // Step 4
		for (int j = 1; j <= m; j++) 
		{
			const char t_j = strFound[j-1];
			// Step 5

			int cost;
			if (s_i == t_j) 
			{
				cost = 0;
			}
			else 
			{
				cost = 1;
			}

			// Step 6
			const int above = vecMatrix[i-1][j];
			const int left = vecMatrix[i][j-1];
			const int diag = vecMatrix[i-1][j-1];
			int cell = min( above + 1, min(left + 1, diag + cost));

			// Step 6A: Cover transposition, in addition to deletion,
			// insertion and substitution. This step is taken from:
			// Berghel, Hal ; Roach, David : "An Extension of Ukkonen's 
			// Enhanced Dynamic Programming ASM Algorithm"
			// (http://www.acm.org/~hlb/publications/asm/asm.html)
			if (i>2 && j>2) 
			{
				int trans = vecMatrix[i-2][j-2]+1;
				if (strExpected[i-2] != t_j)
				{
					trans++;
				}
				if (s_i != strFound[j-2])
				{
					trans++;
				}
				if (cell > trans)
				{
					cell = trans;
				}
			}
				vecMatrix[i][j]=cell;
		}
	}

	return vecMatrix[n][m];
}
//-------------------------------------------------------------------------------------------------
double LevenshteinDistance::GetPercent(string& strExpected, string& strFound)
{
	double dEditDistance = GetDistance(strExpected, strFound);
	double dLongestLength = max(m_iExpectedLen, m_iFoundLen);
	return dEditDistance / dLongestLength * 100.0;
}
//-------------------------------------------------------------------------------------------------