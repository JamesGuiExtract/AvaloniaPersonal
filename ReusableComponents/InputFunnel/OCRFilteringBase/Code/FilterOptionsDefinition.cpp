#include "stdafx.h"
#include "FilterOptionsDefinition.h"

#include "InputChoiceInfo.h"

#include <UCLIDException.h>
#include <CommentedTextFileReader.h>
#include <cpputil.h>

#include <io.h>
#include <SYS\STAT.H>

using namespace std;


//==============================================================================================
// Public Functions
//==============================================================================================
FilterOptionsDefinition::FilterOptionsDefinition()
: m_strCharsAlwaysEnabled(""),
  m_strFODFileName(""),
  m_bModified(false)
{
	m_vecInputTypes.clear();
	m_mapIDToInputChoice.clear();
}
//--------------------------------------------------------------------------------------------------
FilterOptionsDefinition::FilterOptionsDefinition(const FilterOptionsDefinition& toCopy)
{
	m_strCharsAlwaysEnabled = toCopy.m_strCharsAlwaysEnabled;
	m_strFODFileName = toCopy.m_strFODFileName;
	m_bModified = toCopy.m_bModified;
	m_vecInputTypes = toCopy.m_vecInputTypes;
	m_mapIDToInputChoice = toCopy.m_mapIDToInputChoice;
}
//--------------------------------------------------------------------------------------------------
FilterOptionsDefinition& FilterOptionsDefinition::operator = (const FilterOptionsDefinition& toAssign)
{
	m_strCharsAlwaysEnabled = toAssign.m_strCharsAlwaysEnabled;
	m_strFODFileName = toAssign.m_strFODFileName;
	m_bModified = toAssign.m_bModified;
	m_vecInputTypes = toAssign.m_vecInputTypes;
	m_mapIDToInputChoice = toAssign.m_mapIDToInputChoice;

	return *this;
}
//--------------------------------------------------------------------------------------------------
FilterOptionsDefinition::FilterOptionsDefinition(const string& strFODFileFullName)
: m_strFODFileName(strFODFileFullName),
  m_bModified(false)
{
	try
	{
		// parse the file
		parse(strFODFileFullName);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI03709")
}
//--------------------------------------------------------------------------------------------------
string FilterOptionsDefinition::addInputChoiceInfo(const string& strChoiceDescription,
												   const string& strChoiceChars)
{
	// create a new unique choice id
	string strChoiceID(createUniqueChoiceID());
	m_mapIDToInputChoice[strChoiceID] = InputChoiceInfo(strChoiceDescription, strChoiceChars);

	return strChoiceID;
}
//--------------------------------------------------------------------------------------------------
void FilterOptionsDefinition::addInputType(const std::string& strInputType)
{
	m_vecInputTypes.push_back(strInputType);
	m_bModified = true;
}
//--------------------------------------------------------------------------------------------------
string FilterOptionsDefinition::getCharsForInputChoice(const string& strChoiceID)
{
	string strChoiceChars("");
	InputChoiceInfo* pChoiceInfo = getInputChoiceInfo(strChoiceID);
	if (pChoiceInfo)
	{
		// The reason we don't call sCreateUniqueCharsSet() here is
		// because when the input choice chars is put into the map,
		// the uniqueness is confirmed.
		strChoiceChars = pChoiceInfo->m_strChars;
	}
	
	return strChoiceChars;
}
//--------------------------------------------------------------------------------------------------
string FilterOptionsDefinition::getCharsForInputChoices(const vector<string>& vecChoiceIDs)
{
	string strChoicesChars("");
	// iterate through the vector of choice ids
	for (unsigned int uiVec=0; uiVec < vecChoiceIDs.size(); uiVec++)
	{
		InputChoiceInfo* pChoiceInfo = getInputChoiceInfo(vecChoiceIDs[uiVec]);
		if (pChoiceInfo)
		{
			strChoicesChars += pChoiceInfo->m_strChars;
		}
	}
	
	// get unique set of chars
	strChoicesChars = sCreateUniqueCharsSet(strChoicesChars);

	return strChoicesChars;
}
//--------------------------------------------------------------------------------------------------
string FilterOptionsDefinition::getDisplayName()
{
	// get the FOD file name without extension
	return ::getFileNameWithoutExtension(m_strFODFileName);
}
//--------------------------------------------------------------------------------------------------
string FilterOptionsDefinition::getInputChoiceDescription(const string& strChoiceID)
{
	string strDescription("");

	InputChoiceInfo* pChoiceInfo = getInputChoiceInfo(strChoiceID);
	if (pChoiceInfo)
	{
		strDescription = pChoiceInfo->m_strDescription;
	}
	
	return strDescription;
}
//--------------------------------------------------------------------------------------------------
vector<string> FilterOptionsDefinition::getInputChoiceDescriptions(const vector<string>& vecChoiceIDs)
{
	vector<string> vecChoiceDescriptions;
	// iterate through the vector of choice ids
	for (unsigned int uiVec=0; uiVec < vecChoiceIDs.size(); uiVec++)
	{
		InputChoiceInfo* pChoiceInfo = getInputChoiceInfo(vecChoiceIDs[uiVec]);
		if (pChoiceInfo)
		{
			vecChoiceDescriptions.push_back(pChoiceInfo->m_strDescription);
		}
	}

	return vecChoiceDescriptions;
}
//--------------------------------------------------------------------------------------------------
vector<string> FilterOptionsDefinition::getInputChoiceIDs()
{
	vector<string> vecChoiceIDs;

	InputChoices::iterator iterMap = m_mapIDToInputChoice.begin();
	for (; iterMap != m_mapIDToInputChoice.end(); iterMap++)
	{
		vecChoiceIDs.push_back(iterMap->first);
	}

	return vecChoiceIDs;
}
//--------------------------------------------------------------------------------------------------
bool FilterOptionsDefinition::existsInputChoiceID(const string& strInputChoiceID)
{
	// whether the choice id exists
	// iterate through the map
	InputChoices::iterator iterMap = m_mapIDToInputChoice.find(strInputChoiceID);
	if (iterMap != m_mapIDToInputChoice.end())
	{
		return true;
	}

	return false;
}
//--------------------------------------------------------------------------------------------------
void FilterOptionsDefinition::removeInputChoiceInfo(const string& strInputChoiceID)
{
	InputChoices::iterator itChoice = m_mapIDToInputChoice.find(strInputChoiceID);
	if (itChoice != m_mapIDToInputChoice.end())
	{
		// erase the choice info
		m_mapIDToInputChoice.erase(itChoice);
	
		m_bModified = true;
	}
}
//--------------------------------------------------------------------------------------------------
void FilterOptionsDefinition::removeInputType(const std::string& strInputType)
{
	vector<string>::iterator itInputTypes = find(m_vecInputTypes.begin(), m_vecInputTypes.end(), strInputType);
	if (itInputTypes != m_vecInputTypes.end())
	{
		m_vecInputTypes.erase(itInputTypes);
		m_bModified = true;
	}
}
//--------------------------------------------------------------------------------------------------
string FilterOptionsDefinition::sCreateUniqueCharsSet(const string& strInput)
{
	if (strInput.empty())
	{
		return "";
	}

	string strChars("");
	
	// remove all spaces
	string strTemp(strInput);
	::replaceVariable(strTemp, " " , "");
	::replaceVariable(strTemp, "\t" , "");

	for (unsigned int ui = 0; ui < strTemp.size(); ui++)
	{
		// as long as the single char can't be found in strChars, 
		// include this char.
		if (strChars.find(strTemp[ui]) == string::npos)
		{
			strChars += strTemp[ui];
		}
	}

	return strChars;
}
//--------------------------------------------------------------------------------------------------
void FilterOptionsDefinition::setCharsAlwaysEnabled(const string& strChars)
{
	m_strCharsAlwaysEnabled = strChars;

	m_bModified = true;
}
//--------------------------------------------------------------------------------------------------
void FilterOptionsDefinition::setInputChoiceChars(const string& strChoiceID, 
												  const string& strChoiceChars)
{
	InputChoiceInfo* pChoiceInfo = getInputChoiceInfo(strChoiceID);
	if (pChoiceInfo)
	{
		// get the unique set of chars without duplicates
		pChoiceInfo->m_strChars = sCreateUniqueCharsSet(strChoiceChars);

		m_bModified = true;
	}
}
//--------------------------------------------------------------------------------------------------
void FilterOptionsDefinition::setInputChoiceDescription(const string& strChoiceID, 
														const string& strChoiceDescription)
{
	InputChoiceInfo* pChoiceInfo = getInputChoiceInfo(strChoiceID);
	if (pChoiceInfo)
	{
		// get the unique set of chars without duplicates
		pChoiceInfo->m_strDescription = strChoiceDescription;

		m_bModified = true;
	}
}
//--------------------------------------------------------------------------------------------------
void FilterOptionsDefinition::writeToFile(const string& strFODFileFullName)
{
	// update the FOD file name
	m_strFODFileName = strFODFileFullName;

	// make sure the file has read/write permission if exists
	if (fileExistsAndIsReadOnly(strFODFileFullName))
	{
		// change the mode to read/write
		if (_chmod(strFODFileFullName.c_str(), _S_IREAD | _S_IWRITE) == -1)
		{
			CString zMsg("Failed to save ");
			zMsg += strFODFileFullName.c_str();
			zMsg += ". Please make sure the file is not shared by another program.";
			// failed to change the mode
			AfxMessageBox(zMsg);
			return;
		}
	}

	// save current info to the FOD file specified
	// Always overwrite the file no matter it exists or not
	ofstream ofs(strFODFileFullName.c_str(), ios::out | ios::trunc);

	// section for chars always enabled
	writeLine(ofs, "[" + CHARS_ALWAYS_ENABLED + "]");
	writeLine(ofs, m_strCharsAlwaysEnabled);

	// section for input types
	writeLine(ofs, "");
	writeLine(ofs, "[" + INPUT_TYPES + "]");
	for (unsigned int n = 0; n < m_vecInputTypes.size(); n++)
	{
		writeLine(ofs, m_vecInputTypes[n]);
	}

	// section for choices
	writeLine(ofs, "");
	writeLine(ofs, "[" + CHOICES + "]");
	
	string strLine("");
	// Get all choice ids
	vector<string> vecChoiceIDs = getInputChoiceIDs();
	for (unsigned int ui = 0; ui < vecChoiceIDs.size(); ui++)
	{
		string strChoiceID(vecChoiceIDs[ui]);
		// choice id first, separated by a pipe
		strLine = strChoiceID + "|";
		InputChoiceInfo* pChoiceInfo = getInputChoiceInfo(strChoiceID);
		// description second, separated by a pipe
		strLine += pChoiceInfo->m_strDescription + "|";
		// last the chars set
		strLine += pChoiceInfo->m_strChars;
		
		writeLine(ofs, strLine);
	}

	ofs.close();
	waitForFileToBeReadable(strFODFileFullName);

	m_bModified = false;
}


