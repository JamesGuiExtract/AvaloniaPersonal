//==================================================================================================
//
// COPYRIGHT (c) 2008 EXTRACT SYSTEMS, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	MatchData.h
//
// PURPOSE:	Defines a structure to hold match information from the find string
//			operation so that the data can be easily stored and output to
//			a file.
//
// AUTHORS:	Jeff Shergalis
//
//==================================================================================================

#include <UCLIDException.h>
#include <cpputil.h>

#include <string>

using namespace std;

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
const string gstrCOMMA = ",";

class MatchData
{
public:
	//----------------------------------------------------------------------------------------------
	// Constructor/Destructor
	//----------------------------------------------------------------------------------------------
	MatchData(const string& strDocName, const string& strMatchText, 
		unsigned long ulStartPos, unsigned long ulEndPos) 
		: m_strDocName(strDocName), 
		m_strMatchText(strMatchText), 
		m_ulStartPos(ulStartPos), 
		m_ulEndPos(ulEndPos)
	{
	}
	//----------------------------------------------------------------------------------------------
	~MatchData()
	{
		try
		{
		}
		CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20695");
	}

	//----------------------------------------------------------------------------------------------
	// Operator overloads
	//----------------------------------------------------------------------------------------------
	MatchData& operator = (const MatchData& mdNewData)
	{
		// check for self assignment
		if (this != &mdNewData)
		{
			m_strDocName = mdNewData.m_strDocName;
			m_strMatchText = mdNewData.m_strMatchText;
			m_ulStartPos = mdNewData.m_ulStartPos;
			m_ulEndPos = mdNewData.m_ulEndPos;
		}

		return *this;
	}
	//----------------------------------------------------------------------------------------------
	bool operator == (const MatchData& mdOtherData) const
	{
		return ((m_ulStartPos == mdOtherData.m_ulStartPos)
				&& (m_ulEndPos == mdOtherData.m_ulEndPos)
				&& (m_strMatchText == mdOtherData.m_strMatchText)
				&& (m_strDocName == mdOtherData.m_strDocName));
	}
	//----------------------------------------------------------------------------------------------
	bool operator == (const MatchData& mdOtherData)
	{
		return ((m_ulStartPos == mdOtherData.m_ulStartPos)
				&& (m_ulEndPos == mdOtherData.m_ulEndPos)
				&& (m_strMatchText == mdOtherData.m_strMatchText)
				&& (m_strDocName == mdOtherData.m_strDocName));
	}
	//----------------------------------------------------------------------------------------------
	bool operator != (const MatchData& mdOtherData) const
	{
		return !(*this == mdOtherData);
	}
	//----------------------------------------------------------------------------------------------
	bool operator != (const MatchData& mdOtherData)
	{
		return !(*this == mdOtherData);
	}
	//----------------------------------------------------------------------------------------------

	//----------------------------------------------------------------------------------------------
	// Public functions
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To get the document name
	inline string getDocName() { return m_strDocName; }
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To get the match text value
	inline string getMatchText() { return m_strMatchText; }
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To get the start position
	inline unsigned long getStartPos() { return m_ulStartPos; }
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To get the end position
	inline unsigned long getEndPos() { return m_ulEndPos; }
	//----------------------------------------------------------------------------------------------
	// PURPOSE: To return the match data in a string output format of the form:
	//			<DocName>,<StartPos>,<EndPos>,<MatchText>
	string toCSVString()
	{
		string strReturnString = m_strDocName + gstrCOMMA + asString(m_ulStartPos)
			+ gstrCOMMA + asString(m_ulEndPos) + gstrCOMMA + m_strMatchText;
		return strReturnString;
	}
	//----------------------------------------------------------------------------------------------

private:
	//----------------------------------------------------------------------------------------------
	// Member variables
	//----------------------------------------------------------------------------------------------
	string m_strDocName;
	string m_strMatchText;

	unsigned long m_ulStartPos;
	unsigned long m_ulEndPos;
};
