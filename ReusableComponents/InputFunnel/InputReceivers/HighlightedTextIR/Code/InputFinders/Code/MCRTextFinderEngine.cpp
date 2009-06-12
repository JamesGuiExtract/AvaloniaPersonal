//-------------------------------------------------------------------------------------------------
//COPYRIGHT UCLID SOFTWARE, LLC. 2002
//
//FILE:		MCRTextFinderEngine.cpp
//
//PURPOSE:	parse a string (read in from some input file) into bearings,
//          distances and angles strings, then put them into a vector 
//          accordingly. It is utilizing the Bearing, Distance and Angle 
//          classes to recognize specific strings (characters) in the text file.
//
//NOTES:
//
//AUTHOR:	Duan Wang
//
//-------------------------------------------------------------------------------------------------

#include "stdafx.h"
#include "MCRTextFinderEngine.h"

#include <cpputil.h>
#include <Angle.hpp>
#include <bearing.hpp>
#include <Distance.hpp>
#include <UCLIDException.h>

#include <algorithm>

using namespace std;

//-------------------------------------------------------------------------------------------------
//public member functions of MCRTextFinderEngine class
//-------------------------------------------------------------------------------------------------
unsigned long MCRTextFinderEngine::parseString(const string& strInput)
{
	//replace all \n, \r with spaces
	unsigned int iLen = strInput.size();
	
	char *pszStr = new char[iLen + 1];
	std::string strStr("");

	try
 	{
		strcpy_s(pszStr, iLen+1, strInput.data());
		
		char *pDest = NULL;
		int iValueArr[] = {'\n', '\r'};

		for (int i = 0; i < 2; i++)
		{
			pDest = strchr(pszStr, iValueArr[i]);

			while (pDest != NULL)
			{
				int iResultPos = pDest - pszStr;

				//replace with the space
				pszStr[iResultPos] = ' ';
				pDest = strchr(pszStr, iValueArr[i]);
			}
		}

		strStr = pszStr;
	}
	catch(...)
	{
		delete [] pszStr;
		throw;
	}

	delete [] pszStr;

///////////////////////////////////////////////////////
// Initialize the strings and vector
///////////////////////////////////////////////////////
	init(strStr);
	
	if (strInput == "")
	{
		return 0;
	}

	if(m_strInputStrMask.size() != strInput.size()
		|| m_strForParse.size() != strInput.size())
	{
		return 0;
	}

//////////////////////////////////////////////////////
// Find bearing, distance, angle, number strings
//////////////////////////////////////////////////////
	//ulCur is the current cursor position
	unsigned long ulCur = 0;
	//ulTempEnd is the mark as the start and end of each "section"
	//i.e. divide the original string into many sections according to 
	//every bearing start direction unit.
	unsigned long ulTempStart = ulCur;
	unsigned long ulTempEnd = m_ulLen;
	
	//space, tab or line feed
	char* blank = " \t";  

	//each iteration will mark out a "section" for search
	while(ulCur < m_ulLen)
	{
		//get pos right before the first letter position of 
		//next bearing start direction unit 
		ulTempEnd = getNextStartDir(ulTempStart+1) - 1;
		
		//find bearings first
		findBearings(ulTempStart, ulTempEnd);

		//all character in the section has been searched 
		//for all those possible MCR Text, move forward to
		//next "section"
		ulTempStart = ulTempEnd + 1;
		ulCur = ulTempStart;

		//if everthing after ulTempEnd are all spaces, set the ulCur to m_ulLen
		//this is the end of the string
		int iFind = m_strForParse.find_first_not_of(blank, ulTempEnd+2);
		if(iFind==string::npos)
			ulCur = m_ulLen;
	}
	//ulCur is out of the range
		
	//then find angles, distances, numbers and single bearings starting
	// from the beginning of the original m_strForParse to the end of it
	findDistAngNum(0, m_ulLen - 1);
	findSingleBearings(0, m_ulLen - 1);

	//sort the vector according to the position(ascending)
	sort(m_vecMCRInfo.begin(), m_vecMCRInfo.end());

	// Remove any duplicate entries such that shorter items with duplicate 
	// start positions are removed (P10 #3102)
	removeDuplicateStarts();

	return m_vecMCRInfo.size();
}
//-------------------------------------------------------------------------------------------------
const vector<MCRStringInfo>& MCRTextFinderEngine::getMCRStringsInfo() const
{
	//not to modify any data member of the class
	return m_vecMCRInfo;
}

//-------------------------------------------------------------------------------------------------
//private member functions of MCRTextFinderEngine class
//-------------------------------------------------------------------------------------------------
void MCRTextFinderEngine::init(const string& strInput)
{
	//clean the contents of strings and vector first. (no matter what's in there)
	m_strInputStrMask = "";
	m_vecMCRInfo.clear();

	m_strForParse = strInput;

	//make them all in upper case for comparison
	makeUpperCase(m_strForParse);

	//get the length of the input string
	m_ulLen = strInput.size();
	
	//'0' -- not the char we want, or has not been put to the vector yet 
	//'1' -- the char we want and has been put to the vector
	char c = '0';  
	
	//initialize m_strInputStrMask with m_ulLen copies of '0'
	m_strInputStrMask.assign(m_ulLen, c);
}
//-------------------------------------------------------------------------------------------------
bool MCRTextFinderEngine::isUnit(const vector<string>& vecStrUnit, const string& strIn)
{
	//using a constant iterator since the vector is passed in const form
	vector<string>::const_iterator ctIter;
	
	//iterate through the vector, see if there's a match
	for(ctIter = vecStrUnit.begin(); ctIter != vecStrUnit.end(); ctIter++)
	{
		if((*ctIter) == strIn)
		{
			return true;
		}
	}
	return false;
}
//-------------------------------------------------------------------------------------------------
bool MCRTextFinderEngine::isNonAlpha(const unsigned long& ulPos)
{

	if (isPosAvail(ulPos))
	{
		if ((m_strForParse[ulPos] >= 'A' && m_strForParse[ulPos] <= 'Z') || 
			(m_strForParse[ulPos] >= 'a' && m_strForParse[ulPos] <= 'z'))
		{
			return false;
		}
		else
		{
			return true;
		}
	}
	else
	{
		return false;
	}
}
//-------------------------------------------------------------------------------------------------
unsigned long MCRTextFinderEngine::getFirstAvailPos(const unsigned long& ulCurPos)
{
	unsigned int iPos = 0;
	unsigned long ulPos = ulCurPos;
			
	//search start from the current pos
	if (isPosAvail(ulPos))
	{
		return ulPos;
	}
	else
	{
		//find the first "0" after the ulPos (the next pos to the current)
		iPos = m_strInputStrMask.find_first_not_of("1", ulPos);

		if ((iPos != string::npos) && (iPos >= 0))  //not negative
		{
			return iPos;
		}
	}

	//no more positions available
	return m_ulLen; //implies the end of the string
}
//-------------------------------------------------------------------------------------------------
bool MCRTextFinderEngine::isPosAvail(const unsigned long& ulPos)
{
	if (ulPos < m_strInputStrMask.size())
	{
		return true;
	}
	else
	{
		return false;
	}
}
//-------------------------------------------------------------------------------------------------
void MCRTextFinderEngine::setVec(const unsigned long& ulStartPos, 
						   const unsigned long& ulEndPos,
						   const EStrType& eT, const std::string strText)
{
/*	// It was decided in an engineering meeting on 1-11-2002 that any MCR text
	// such as bearings, distances, and angles should be a minimum of two 
	// characters long so that unnecessary MCR-like noise can be eliminated
	// if the length of the MCR-text to be added is not a minimum of two
	// characters, then do not add it to the vector
	if (ulEndPos == ulStartPos)
		return;
*/

	//set up the string info for storing bearing information
	MCRStringInfo mcrStrInfo;

	// Store start and end positions
	mcrStrInfo.ulStartCharPos = ulStartPos;
	mcrStrInfo.ulEndCharPos = ulEndPos;

	// Store string type
//	mcrStrInfo.eType = eT;
	switch (eT)
	{
	case kBearing:
		mcrStrInfo.strTypeInfo = "Bearing";
		break;

	case kDistance:
		mcrStrInfo.strTypeInfo = "Distance";
		break;
	
	case kAngle:
		mcrStrInfo.strTypeInfo = "Angle";
		break;
	
	case kNumber:
		mcrStrInfo.strTypeInfo = "Number";
		break;
	
	default:
		// Throw exception
		break;
	}

	// Store actual substring
	mcrStrInfo.strText = strText;

	//store the struct into the vector
	m_vecMCRInfo.push_back(mcrStrInfo);

	//change the counter-part position charaters in m_strInputStrMask
	//from '0' to '1'
	m_strInputStrMask.replace(ulStartPos, (ulEndPos-ulStartPos+1), 
								(ulEndPos-ulStartPos+1), '1');
}
//-------------------------------------------------------------------------------------------------
bool MCRTextFinderEngine::isNumber(const unsigned long& ulPos)
{
	if (ulPos < m_strForParse.length())
	{
		//between 0 -9 
		if(m_strForParse[ulPos] >= '0' && m_strForParse[ulPos] <= '9')
		{
			return true;
		}
		else
		{
			return false;
		}
	}
	else
	{
		UCLIDException ue("ELI13065", "Index out of bounds!");
		ue.addDebugInfo("Index", ulPos);
		ue.addDebugInfo("Size", m_strForParse.length());
		ue.addDebugInfo("String", m_strForParse);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
unsigned long MCRTextFinderEngine::getNumEnd(const unsigned long& ulTempEnd, const unsigned long& ulNumStart)
{
	unsigned long ulNumEnd = ulNumStart;
	//ul is cursor pos
	unsigned long ul = ulNumStart + 1;
	//a number can only have one decimal place
	bool bHasDecimal = false;

	while (ul <= ulTempEnd)
	{
		if (isNumber(ul) && isPosAvail(ul))
		{
			ulNumEnd = ul;
			ul++;
		}

		// if it's a decimal place symbol
		// Make sure that index ul does not go out of boundary [P10: 3096]
		else if (m_strForParse[ul]=='.' && isPosAvail(ul) && ul < m_strForParse.size() - 1)
		{
			if (bHasDecimal)
			{
				break;
			}
			//if there is no decimal place found in the previous positioins
			//and the char after the decimal place is a number as well
			else 
			{
				if (isNumber(ul+1) && isPosAvail(ul+1))
				{
					bHasDecimal = true;
					//advance the ulNumEnd to the number pos
					ulNumEnd = ul + 1;
					ul = ul +2;
				}
				else // that is not a decimal place symbol (might be a stop symbol)
				{
					break;
				}
			}
		}
		//if it's a comma within a number
		// Make sure that index ul does not go out of boundary [P10: 3096]
		else if (m_strForParse[ul] == ',' && ul < m_strForParse.size() - 4)
		{
			//the next three charactors right after this comma must be numbers as well
			if (isNumber(ul+1) && isNumber(ul+2) 
				&& (isNumber(ul+3) && isPosAvail(ul+3))  //must be a number here
				&& ((ul+4 >= m_ulLen) || (isPosAvail(ul+4) && !isNumber(ul+4)))  //must not be a number here
				&& ((ul-4 < ulNumStart) || ((ul-4 >= ulNumStart) && !isNumber(ul-4)))  //must not be a number here
				&& !bHasDecimal)
			{
				ulNumEnd = ul +3;
				ul = ul+4;
			}
			else
				break;
		}
		else
		{
			break;
		}
	}

	return ulNumEnd;
}
//-------------------------------------------------------------------------------------------------
bool MCRTextFinderEngine::findMatch(const vector<string>& vecStr, unsigned long& ulPos)
{
	vector<string>::const_iterator ctIter;
	unsigned long ulStart = ulPos;
	unsigned long ulEnd = ulPos;
	int iFound;
	bool bFound = false;

	for(ctIter = vecStr.begin(); ctIter != vecStr.end(); ctIter++)
	{
		if((*ctIter) != "")
		{
			iFound = m_strForParse.find((*ctIter), ulStart);
		}
		else
		{
			continue;
		}

		//there's a match, as long as it starts from the same pos as ulPos
		if(iFound != string::npos && iFound == ulStart)
		{
			bFound = true;
			//need the longest string
			if((*ctIter).size()>(ulEnd-ulStart))
			{
				//set ulEnd to the end of the found string
				ulEnd = ulStart + ((*ctIter).size() - 1);
			}
		}
	}

	//if the match string exists, and is longer than 1
	if((ulEnd-ulStart)>0)
	{
		//change the current position to ...
		ulPos = ulEnd;
	}

	return bFound;
}
//-------------------------------------------------------------------------------------------------
unsigned long MCRTextFinderEngine::getNextStartDir(const unsigned long& ulPos)
{
	Bearing objBearing;
	//space, tab or return
	string blank = " \t"; 
	// get bearing start direction strings
	vector<string> vecStrBearingStartDir = objBearing.getStartDirStrings();

	//go for the first bearing unit starting from ulFirstPos
	//i is the current cursor position
	unsigned long i = ulPos;
	while(i<m_ulLen)
	{
		//skip the spaces
		int k = m_strForParse.find_first_not_of(blank, i);
		
		if(k!=string::npos)
		{
			i = k;
		}
		//reach the end of m_strForParse
		else
		{
			return m_ulLen;
		}

		//the start unit char must be isolated(i.e. no alphabetic) on its left
		if(isNonAlpha(i-1) && isUnit(vecStrBearingStartDir, m_strForParse.substr(i,1)))
		{
			//check if the letter is actually one of the start direction unit strings
			unsigned long ulTemp = i;  //temp position to hold the end of a unit
			
			//if there's a better match... ulTemp might or might not be changed
			findMatch(vecStrBearingStartDir, ulTemp);
			
			//search for the next non-space letter after pos ulTemp
			int iNext = m_strForParse.find_first_not_of(blank, ulTemp +1);
			//if the next first non-space letter is a number...
			if(iNext != string::npos && isNumber(iNext))
			{
				return i;  //find the start direction string unit
			}
		}			

		//not a start direction unit string, move forward....
		i++;
	}

	//now i > =m_ulLen, which means it reaches the end 
	//without getting any start dir unit
	return m_ulLen;
}
//-------------------------------------------------------------------------------------------------
void MCRTextFinderEngine::findBearings(const unsigned long& ulTempStart, 
								 const unsigned long& ulTempEnd)
{	
	//for each string (ex. bearing, distance, angle,etc) start and end position
	unsigned long ulUnitStartPos = 0;
	//store the end position of the start direction unit string
	unsigned long ulTempUnitStart_End = 0;
	unsigned long ulUnitEndPos = 0;
	//store the end position of the end direction unit string
	unsigned long ulTempUnitEnd_End = 0;

	//i is current pos
	unsigned long i = ulTempStart;

	//for spaces, tabs
	string blank = " \t";

	Bearing objBearing;
	//vectors for bearing units strings
	vector<string> vecStrBearingStartDir = objBearing.getStartDirStrings();
	vector<string> vecStrBearingEndDir = objBearing.getEndDirStrings();

/////////////////////////////////////////////////////////////////////////
// Looking for Bearings (not including single bearings (ex. North, wst, etc.))
/////////////////////////////////////////////////////////////////////////

	//searching for bearings( includes all the bearings with 
	//start & end units
	while(i <= ulTempEnd)
	{
		if(isUnit(vecStrBearingStartDir, m_strForParse.substr(i,1)))
		{
			ulUnitStartPos = i;

			ulTempUnitStart_End = i;

			//look for the real bearing start(i.e. a start symbol followed by a number)
			//if find match ulTempUnitStart_End will be the last char pos of the start 
			//direction unit string
			findMatch(vecStrBearingStartDir, ulTempUnitStart_End);
			
			//search for the next non-space letter after the pos ulTempUnitStart_End
			int iNext = 
				m_strForParse.find_first_not_of(" \t", 
												(ulTempUnitStart_End + 1), 
												(ulTempEnd - ulTempUnitStart_End));
			//find a non-space letter and this letter( at pos iNext) is a number
			//i.e. the string is a real start direction unit
			if(iNext!=string::npos && isNumber(iNext))
			{
				//skip the start unit and the number
				//we're going to search for end direction unit from here onwards
				i = iNext + 1; 
			}
			//not a bering start, not a single bearing, get out of the loop
			else if(iNext == string::npos)
			{
				break;
			}
		}
		else
		{
			//move forward to search for next start 
			i++;
			continue; 
		}

		//now we have the start pos for bearing, look for the end symbol
		while(i<=ulTempEnd)
		{
			//end unit found (might or might not be the bearing end dir unit)
			if(isUnit(vecStrBearingEndDir, m_strForParse.substr(i,1)))
			{
				ulUnitEndPos = i;
				ulTempUnitEnd_End = i;

				string strSub = m_strForParse.substr(ulUnitEndPos-1,3);

				//check if it's the real end unit ( exception: no "SEC", no "DEG")
				if(strSub == "SEC" || strSub == "DEG")
				{
					i = i +2;
					//continue;
				}
				//as long as one of the conditions is false
				else
				{
					//ulTempUnitEnd_End might or not be changed
					findMatch(vecStrBearingEndDir, ulTempUnitEnd_End);
						
					ulUnitEndPos = ulTempUnitEnd_End;
					string	strTest = m_strForParse.substr( ulUnitStartPos, 
						ulUnitEndPos-ulUnitStartPos+1 ).c_str();
					objBearing.evaluate( strTest.c_str() );
					if(objBearing.isValid())
					{
						setVec( ulUnitStartPos, ulUnitEndPos, kBearing, 
							strTest );
						i = ulUnitEndPos + 1;
						break; //get out the inner while loop
					}
					else
					{
						//move forward to search for next end string
						i = ulTempUnitEnd_End + 1;
						//continue;
					}
				}
			}
			//not a possible end direction unit string 
			else
			{
				i++;
				//continue;
			}

		}
		//i>ulTempEnd for inner while loop
		//as long as there's string after the start unit string,we
		//need to reset the current cursor back to the pos after the start
		//unit, and look for next possible start unit as well as the possible
		//end unit
		unsigned int iSearch = m_strForParse.find_first_not_of(blank, ulTempUnitStart_End+1);
		if (iSearch != string::npos && i < iSearch)
		{
			i = iSearch;
		}
		//else leave i the way it is and end the function, return to its caller

	}
	//i>ulTempEnd for outer while loop, we've exhausted the search through this
	//"section" of string for bearings (not including all the single bearings)

	//reach the end of the "section" for searching, return to the caller
}
//-------------------------------------------------------------------------------------------------
void MCRTextFinderEngine::findSingleBearings(const unsigned long& ulTempStart, 
									   const unsigned long& ulTempEnd)
{
	//for each string (ex. bearing, distance, angle,etc) start and end position
	unsigned long ulUnitStartPos = 0;
	//store the end position of the start direction unit string
	unsigned long ulTempUnitStart_End = 0;
	unsigned long ulUnitEndPos = 0;
	//store the end position of the end direction unit string
	unsigned long ulTempUnitEnd_End = 0;

	//i is current pos
	unsigned long i = ulTempStart;

	Bearing objBearing;
	//vectors for bearing units strings
	vector<string> vecStrBearingStartDir = objBearing.getStartDirStrings();
	vector<string> vecStrBearingEndDir = objBearing.getEndDirStrings();

/////////////////////////////////////////////////////////////////////////
// Looking for single Bearings (ex. North, wst, etc.)
/////////////////////////////////////////////////////////////////////////

	//restart from ulTempStart
	ulUnitStartPos = 0;
	ulUnitEndPos = 0;
	i = ulTempStart;

	//get the first available pos found starting from current pos i
	i = getFirstAvailPos(i);

	while(i <= ulTempEnd)
	{
		unsigned long ulTemp = 0;
	
		//if this letter is isolated on the left side, and either is
		//a start or an end unit letter
		if((i != 0) && isNonAlpha(i - 1))
		{
			ulUnitStartPos = i;
			ulTemp = i;
			//contain status for might-be start or end unit 
			//('s' -- start, 'e' -- end, 'n' -- none)
			char cStartOrEnd = 'n';

			if(isUnit(vecStrBearingStartDir, m_strForParse.substr(i,1)))
			{
				cStartOrEnd = 's';
			}
			else if(isUnit(vecStrBearingEndDir,m_strForParse.substr(i,1)))
			{
				cStartOrEnd = 'e';
			}

			switch(cStartOrEnd)
			{
			case 's':
				findMatch(vecStrBearingStartDir, ulTemp);
				break;
			case 'e':
				findMatch(vecStrBearingEndDir, ulTemp);
				break;
			case 'n':
			default:
				break;
			}

			//not a unit of either start or end direction strings
			if(cStartOrEnd=='n')
			{
				i = getFirstAvailPos(i + 1);
				continue;
			}

			//might be a unit, check if it's also isolated on the right side
			// and the end pos must not be modified before
			if(isNonAlpha(ulTemp + 1) && isPosAvail(ulTemp))
			{
				ulUnitEndPos = ulTemp;
				//is a start or end direction unit string, put the string to the vec
				string	strTest = m_strForParse.substr(ulUnitStartPos, ulUnitEndPos - ulUnitStartPos + 1).c_str();
				setVec( ulUnitStartPos, ulUnitEndPos, kBearing, strTest );
				
				//continue the search
				i = getFirstAvailPos(ulUnitEndPos + 1);
			}
			//not a single bearing, move forward
			else
			{
				i = getFirstAvailPos(ulTemp + 1);
			}
		}
		//not even isolated on the left side
		else 
		{
			i = getFirstAvailPos(i + 1);
		}
	}
}
//-------------------------------------------------------------------------------------------------
void MCRTextFinderEngine::findDistAngNum(const unsigned long& ulTempStart, 
								   const unsigned long& ulTempEnd)
{
	//temprary positions for dist, angle, number
	unsigned long ulDistStart =0;
	unsigned long ulDistEnd = 0;
	unsigned long ulAngleStart = 0;
	unsigned long ulAngleEnd = 0;
	unsigned long ulNumStart = 0;
	unsigned long ulNumEnd = 0;

	//i is current cursor pos
	unsigned long i = ulTempStart;

	i = getFirstAvailPos(i);

	while(i <= ulTempEnd)
	{
		//looking for the number first
		if(isNumber(i))
		{
			//now it's an eligible number, might be a part of a distance or an angle
			ulNumStart = i;
			//the availability of ulNumEnd will be checked in getNumEnd()
			ulNumEnd = getNumEnd(ulTempEnd, ulNumStart);

			//see if there's distance unit after the number
			ulDistStart = ulNumStart;
			ulAngleStart = ulNumStart;

			// Extract substring
			string	strTest = m_strForParse.substr( ulAngleStart, 
				ulAngleEnd - ulAngleStart + 1 ).c_str();

			//look for angle
			if(isAngle(ulTempEnd, ulAngleStart, ulAngleEnd))
			{
				setVec( ulAngleStart, ulAngleEnd, kAngle, strTest );
				i = getFirstAvailPos(ulAngleEnd+1);
			}
			//the value and availability of ulDistEnd will be given and checked 
			//in the isDistance() if it's true
			else if(isDistance(ulTempEnd, ulDistStart, ulDistEnd))
			{
				ulAngleStart = 0;
				ulAngleEnd = 0;
				//set the distance to the m_vecMCRInfo
				setVec( ulDistStart, ulDistEnd, kDistance, strTest );
				i = getFirstAvailPos(ulDistEnd+1);
			}
			//next non-space char right after the number is not available
			//which means that this number doesn't belong to any distance or angle
			else
			{
				ulDistStart = 0;
				ulDistEnd = 0;
				ulAngleStart = 0;
				ulAngleEnd = 0;
				//put the number to the m_vecMCRInfo
				setVec( ulNumStart, ulNumEnd, kNumber, strTest );
				//forward the search pos
				i = getFirstAvailPos(ulNumEnd + 1);
			}
		}
		else
		{
			i = getFirstAvailPos(i + 1);
		}
	}
}
//-------------------------------------------------------------------------------------------------
bool MCRTextFinderEngine::isDistance(const unsigned long& ulTempEnd,
							   const unsigned long& ulDistStart, 
							   unsigned long& ulDistEnd)
{
	Distance objDist;
	//vectors for distance units strings	
	vector<string> vecDistUnit = objDist.getDistanceUnitStrings();

	bool bIsDist = false;
	unsigned long ulTemp = ulDistStart;
	//cursor pos
	unsigned long i = ulDistStart;
	//each distance can have maximum number of 4 units
	int iMax = 4;

	while(i<=ulTempEnd && iMax>0)
	{
		if(isPosAvail(i) && isNumber(i))
		{
			//start from each number start pos i, get the number
			ulTemp = getNumEnd(ulTempEnd, i);
			//look for the distance unit
			ulTemp = m_strForParse.find_first_not_of(" \t", ulTemp+1);

			if(isPosAvail(ulTemp))
			{
				if(findMatch(vecDistUnit, ulTemp))
				{
					if(isPosAvail(ulTemp) && isNonAlpha(ulTemp+1))
					{
						objDist.evaluate(m_strForParse.substr(
								ulDistStart, ulTemp-ulDistStart+1).c_str());
						if(objDist.isValid())
						{
							iMax--;
							//this is a valid distance
							bIsDist = true;
							//ulDistEnd will be set here
							ulDistEnd = ulTemp;
							//move forward for any following possible distance units
							i = m_strForParse.find_first_not_of(" \t", ulDistEnd+1);

							continue;
						}
					}
				}
			}
		}				
		return bIsDist;
	}
	return bIsDist;
}
//-------------------------------------------------------------------------------------------------
bool MCRTextFinderEngine::isAngle(const unsigned long& ulTempEnd,
							const unsigned long& ulAngleStart, 
							unsigned long& ulAngleEnd)
{
	Angle objAngle;
	//vectors for angle units strings
	vector<string> vecDeg = objAngle.getAngleDegStrings();
	vector<string> vecMin = objAngle.getAngleMinStrings();
	vector<string> vecSec = objAngle.getAngleSecStrings();
	//according to the angle status, vecAngle will be set to 
	//one of the three vectors (vecDeg, vecMin, vecSec)
	vector<string> vecAngle = vecDeg; //set to degree first

	bool bIsAng=false;
	unsigned long ulTemp = ulAngleStart;
	//cursor pos
	unsigned long i = ulAngleStart;
	//store status of angle
	// 'n' -- none
	// 'd' -- degree
	// 'm' -- minute
	// 's' -- second
	char cStatus = 'n'; 

	//look for degree sign first, if it's not there, not even an angle
	//
	while(i<=ulTempEnd && cStatus != 's')
	{
		if(isPosAvail(i) && isNumber(i))
		{
			//start from each number start pos i, get the number
			ulTemp = getNumEnd(ulTempEnd, i);
			//look for the angle degree
			ulTemp = m_strForParse.find_first_not_of(" \t", ulTemp+1);
		
			if(isPosAvail(ulTemp))
			{
				if(findMatch(vecAngle, ulTemp))
				{
					if(isPosAvail(ulTemp) && isNonAlpha(ulTemp+1))
					{
						objAngle.evaluate(m_strForParse.substr(
								ulAngleStart, ulTemp-ulAngleStart+1).c_str());
						if(objAngle.isValid())
						{
							//this is a valid distance
							bIsAng = true;
							//ulDistEnd will be set here
							ulAngleEnd = ulTemp;

							//set status
							switch(cStatus)
							{
							//found a degree 
							case 'n':
								cStatus = 'd';
								//next look for minute,
								//if minute isn't there, see "else if" below
								vecAngle = vecMin;
								break;
							//found a minute
							case 'd':
								cStatus = 'm';
								//next look for a second
								vecAngle = vecSec;
								break;
							//found a second
							case 'm':
								cStatus = 's';
								break;
							default:
								break;
							}
							//move forward for any following possible distance units
							i = m_strForParse.find_first_not_of(" \t", ulAngleEnd+1);

							continue;
						}
					}
				}
				//if the degree has already been found in the string
				//and minute is not there, now we look for second
				else if(cStatus=='d')
				{
					vecAngle = vecSec;
					if(findMatch(vecAngle, ulTemp))
					{
						if(isPosAvail(ulTemp) && isNonAlpha(ulTemp+1))
						{
							objAngle.evaluate(m_strForParse.substr(
									ulAngleStart, ulTemp-ulAngleStart+1).c_str());
							if(objAngle.isValid())
							{
								//this is a valid distance
								bIsAng = true;
								//ulDistEnd will be set here
								ulAngleEnd = ulTemp;
								//found a second
								cStatus = 's';
								//move forward for any following possible distance units
								i = m_strForParse.find_first_not_of(" \t", ulAngleEnd+1);

								continue;
							}
						}
					}
				}
			}
		}
		return bIsAng;
	}
	return bIsAng;
}
//-------------------------------------------------------------------------------------------------
void MCRTextFinderEngine::removeDuplicateStarts()
{
	// Step through each entry in collection
	int nPreviousStart = -1;
	int nPreviousEnd = -1;
	for (unsigned int i = 0; i < m_vecMCRInfo.size(); i++)
	{
		// Check start position of this item
		if ((int)m_vecMCRInfo[i].ulStartCharPos > nPreviousStart)
		{
			// This is a new position, update positions
			nPreviousStart = m_vecMCRInfo[i].ulStartCharPos;
			nPreviousEnd = m_vecMCRInfo[i].ulEndCharPos;
		}
		else if ((int)m_vecMCRInfo[i].ulStartCharPos == nPreviousStart)
		{
			// This is the same position (or a subset), delete the shorter entry
			if ((int)m_vecMCRInfo[i].ulEndCharPos > nPreviousEnd)
			{
				// Update end position
				nPreviousEnd = m_vecMCRInfo[i].ulEndCharPos;

				// This entry is longer, delete the previous entry
				ASSERT_ARGUMENT("ELI15895", i > 0);
				m_vecMCRInfo.erase( m_vecMCRInfo.begin() + i - 1 );
			}
			else
			{
				// The previous entry was longer, delete this one
				m_vecMCRInfo.erase( m_vecMCRInfo.begin() + i );
			}

			// Update index
			i--;
		}
	}
}
//-------------------------------------------------------------------------------------------------
