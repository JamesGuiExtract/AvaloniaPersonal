//==================================================================================================
//COPYRIGHT UCLID SOFTWARE, LLC. 2002
//
//FILE:		MCRTextFinderEngine.hpp
//
//PURPOSE:	parse a string (read in from some input file) into bearings,
//          distances and angles strings, then put them into a vector 
//          accordingly. It is utilizing the Bearing, Distance and Angle 
//          classes to recognize specific strings (characters) in the text file.
//
//NOTES:	This is a regular class, not a COM object
//
//AUTHOR:	Duan Wang
//
//==================================================================================================
//
//==================================================================================================

#pragma once

#include <vector>
#include <string>


//==============================================================================================
// enumerated type variable to hold constants for different type of string
enum EStrType {kBearing, kDistance, kAngle, kNumber};

//==================================================================================================
// CLASS:	MCRStringInfo
// PURPOSE: Contains the start and end position of a bearing, distance or 
//          an angle string in the input string (text)
//==================================================================================================
class MCRStringInfo
{
public:
	//stores the starting position of the bearing, distance or angle string
	unsigned long	ulStartCharPos;
	
	//stores the ending position of the bearing, distance or angle string
	unsigned long	ulEndCharPos;
	
	//indicates the type of the parsed string
//	EStrType eType;
	std::string		strTypeInfo;
	
	// actual character string
	std::string		strText;

	//overload < for the purpose of sorting
	friend bool operator < (const MCRStringInfo& a, const MCRStringInfo& b)
	{
		return (a.ulStartCharPos < b.ulStartCharPos);
	}
};
	

//==================================================================================================
// CLASS:      MCRTextFinderEngine
// PURPOSE:    provides a way to recognize all the bearing, distances and angles
//             in the string
// REQUIRE:    utlizes existing classes (i.e. Bearing, Distance, Angle) to evaluate
//			   a string whether it is a valid form (of Bearing, Distance or Angle).
// INVARIANTS:
// EXTENSIONS:
// NOTES:
//==================================================================================================
class MCRTextFinderEngine
{
public:
	//==============================================================================================
	//  PURPOSE:  set the data member m_strForParse equal to the input string
	//            so that the object MCRTextFinderEngine can be reused every time 
	//            there's a new string read in.
	//			  find out all the MCR Text strings
	//  REQUIRE:  
	//  PROMISE:  return the length of the m_vecMCRInfo
	//  PARAMETERS:  
	//       strInput: holds the contents of the read in text from the input file
    unsigned long parseString(const std::string& strInput); 
	//==============================================================================================
	//  PURPOSE:  return the vector holding all MCR text strings position info
	//  REQUIRE:  cann't use and methods that will modify the class(or data member)
	//  PROMISE:  
	//  PARAMETERS:  
	const std::vector<MCRStringInfo>& getMCRStringsInfo() const;
	//==============================================================================================
	//  PURPOSE:  removes smaller strings from m_vecMCRInfo where start positions match
	//  REQUIRE:  m_vecMCRInfo should be sorted before duplicates are removed
	//  PROMISE:  
	//  PARAMETERS:  
	void removeDuplicateStarts();
	//==============================================================================================


private:
	//==============================================================================================
	// DATA MEMBERS:
	//==============================================================================================
	//==============================================================================================
	//stores the input string for parsing purpose
	std::string m_strForParse;

	//==============================================================================================
	//has the same length as m_strForParse, used for hold status of each character
	//in m_strForParse
	std::string m_strInputStrMask;
	//==============================================================================================
	// stores all the found bearings, distances and angles (also includes 
	// some unknown numbers
	std::vector<MCRStringInfo> m_vecMCRInfo;
	//==============================================================================================
	//the length of the input string
	unsigned long m_ulLen;
	//==============================================================================================


	//==============================================================================================
	// HELPER METHODS: 
	//==============================================================================================

	//==============================================================================================
	//  PURPOSE:  initialize m_strForParse, m_strInputStrMask, m_vecMCRInfo
	//  REQUIRE:  
	//  PROMISE:  
	//  PARAMETERS:
	//       strInput: the original string
	void init(const std::string& strInput);
	//==============================================================================================
	//  PURPOSE:  check if the pass-in string is one of the strings in the units vector   
	//  REQUIRE:  const iterator to traverse the vector, compare the strIn with every unit 
	//			  in the vector
	//  PROMISE:   
	//  PARAMETERS:
	//       vecStrUnit: holds all units information (ex. "N", "east", "degree", etc.)
	//			  strIn: holds one or more charactors from the input string (text)
	bool isUnit(const std::vector<std::string>& vecStrUnit, const std::string& strIn);
	//==============================================================================================
	//  PURPOSE:  skip all found strings' position, get the first available position
	//			  starting from the current position
	//  REQUIRE: 
	//  PROMISE:  speed up the entire search without go through the found strings' positions
	//  PARAMETERS:
	//          ulCurPos: the current position of the search
	unsigned long getFirstAvailPos(const unsigned long& ulCurPos);	
	//==============================================================================================
	//  PURPOSE:  check to see if this position is available
	//  REQUIRE:  
	//  PROMISE:  
	//  PARAMETERS:
	//          ulPos: this position
	bool isPosAvail(const unsigned long& ulPos);
	//==============================================================================================
	//  PURPOSE:  put each bearing, distance or angle to the vector<MCRStringIfo>
	//  REQUIRE:  m_strInputStrMask needs to be changed at the counter-part positions
	//  PROMISE:  
	//  PARAMETERS:
	//          ulStartPos: start position of the string
	//			ulEndPos:   end pos of the string
	//			eT:			the string type (kBearing, kDistance, etc.)
	//			strText:    the actual substring
	void setVec(const unsigned long& ulStartPos, const unsigned long& ulEndPos,
		const EStrType& eT, const std::string strText);
	//==============================================================================================
	//  PURPOSE:  retrieve every string in the passing in vector, and search each one
	//            of them in the whole string(input text, i.e.m_strForParse) start from
	//            the passing in position
	//  REQUIRE:
	//  PROMISE:  ulPos will be changed to the pos at the end of the matched string 
	//			  in the whole string if there's a better match, else ulPos remains
	//			  the same.
	//  PARAMETERS:  
	//			const vector<string>& vecStr: holds the strings that you want to search for
	//			unsigned long& ulPos: stores the start position for search, as
	//								  well as the end pos if there's a perfect match
	//  NOTE:	   be careful! ulPos might be changed!
	bool findMatch(const std::vector<std::string>& vecStr, unsigned long& ulPos);
	//==============================================================================================
	//  PURPOSE:  see if the letter at pos is a number
	//  REQUIRE:  
	//  PROMISE:  
	//  PARAMETERS:
	//          const unsigned long& ulPos: the position for testing
	bool isNumber(const unsigned long& ulPos);
	//==============================================================================================
	//  PURPOSE:  find the end pos of a number according to the start pos of the number
	//  REQUIRE:  start pos of the number
	//  PROMISE:  the number start from ulNumStart to ulNumEnd 
	//			  will be a valid number as a whole
	//  PARAMETERS:
	//			ulTempEnd: the end pos of the string
	//          ulNumStart: start pos of the number
	unsigned long getNumEnd(const unsigned long& ulTempEnd, const unsigned long& ulNumStart);
	//==============================================================================================
	//  PURPOSE:  see if the letter at pos is not an alphabetical char
	//  REQUIRE:  
	//  PROMISE:  
	//  PARAMETERS:
	//          const unsigned long& ulPos: the position for testing
	bool isNonAlpha(const unsigned long& ulPos);
	//==============================================================================================
	//  PURPOSE:  find the next bearing unit(might be a single bearing)
	//  REQUIRE:  the original string(m_strForParse) length >0, the bearing start
	//			  unit must be followed by a number
	//  PROMISE:  
	//  PARAMETERS:
	//			unsigned long& ulPos: return the pos right before the first 
	//									  occurrence of next bearing start 
	//									  unit's first letter
	unsigned long getNextStartDir(const unsigned long& ulPos);
	//==============================================================================================
	//  PURPOSE:  find all the bearings (not including single bearings
	//			  within the given positions, and put
	//			  them in the vector<MCRStringInfo>
	//  REQUIRE:  ulTempEnd < m_ulLen; 
	//  PROMISE:  help getMCRStringsInfo() to get the bearings
	//  PARAMETERS:
	//			ulTempStart: start pos in the given positions 
	//			ulTempEnd: end pos in the given positions									  
	void findBearings(const unsigned long& ulTempStart, const unsigned long& ulTempEnd);
	//==============================================================================================
	//  PURPOSE:  find all the single bearings within the given positions, and put
	//			  them in the vector<MCRStringInfo>
	//  REQUIRE:  ulTempEnd < m_ulLen; 
	//  PROMISE:  help getMCRStringsInfo() to get the bearings
	//  PARAMETERS:
	//			ulTempStart: start pos in the given positions 
	//			ulTempEnd: end pos in the given positions									  
	void findSingleBearings(const unsigned long& ulTempStart, const unsigned long& ulTempEnd);
	//==============================================================================================
	//  PURPOSE:  find all distances, angles and stand-alone numbers within 
	//			  the given positions, and put them in the vector<MCRStringInfo>
	//  REQUIRE:  ulTempEnd < m_ulLen; 
	//  PROMISE:  help getMCRStringsInfo() to get the dist, angle, number
	//  PARAMETERS:
	//			ulTempStart: start pos in the given positions 
	//			ulTempEnd: end pos in the given positions									  
	void findDistAngNum(const unsigned long& ulTempStart, const unsigned long& ulTempEnd);
	//==============================================================================================
	//  PURPOSE:  from the start pos, looking for a valid distance
	//  REQUIRE:  
	//  PROMISE:  return true if there's one, and set the ulDistEnd to the current end
	//			  pos of the distance
	//  PARAMETERS:
	//			ulTempEnd: end pos of the current string for searching
	//			ulDistStart: pos to begin search for a valid distance 
	//			ulDistEnd: end of the found distance unit pos  
	bool isDistance(const unsigned long& ulTempEnd,
					const unsigned long& ulDistStart, 
					unsigned long& ulDistEnd);
	//==============================================================================================
	//  PURPOSE:  from the start pos, looking for a valid angle
	//  REQUIRE:  
	//  PROMISE:  return true if there's one, and set the ulAngleEnd to the current end
	//			  pos of the angle
	//  PARAMETERS:
	//			ulTempEnd: end pos of the current string for searching
	//			ulAngleStart: pos to begin search for a valid angle
	//			ulAngleEnd: end of the found angle unit string pos
	bool isAngle(const unsigned long& ulTempEnd,
				 const unsigned long& ulAngleStart, 
				 unsigned long& ulAngleEnd);
	//==============================================================================================

};