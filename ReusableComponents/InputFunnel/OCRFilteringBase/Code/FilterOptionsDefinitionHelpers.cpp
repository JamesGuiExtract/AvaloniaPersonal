#include "stdafx.h"
#include "FilterOptionsDefinition.h"

#include "InputChoiceInfo.h"

#include <UCLIDException.h>
#include <CommentedTextFileReader.h>
#include <StringTokenizer.h>
#include <cpputil.h>

#include <fstream>

using namespace std;

const string FilterOptionsDefinition::CHARS_ALWAYS_ENABLED = "CharsAlwaysEnabled";
const string FilterOptionsDefinition::INPUT_TYPES = "InputTypes";
const string FilterOptionsDefinition::CHOICES = "Choices";

//==============================================================================================
// Helper Functions
//==============================================================================================
string FilterOptionsDefinition::createUniqueChoiceID()
{
	// create a unique id for this entry
	string strChoiceID("");
	// if the map is empty, then create the first id for current category
	if (m_mapIDToInputChoice.empty())
	{
		// choice id starts from 1
		strChoiceID = "0001";
	}
	else
	{
		// look for the last unique id of current sub string category
		InputChoices::iterator iter = m_mapIDToInputChoice.end();
		// move the iteractor to the last item in the map
		iter--;
		strChoiceID = iter->first;
		// increment the id
		strChoiceID = incrementNumericalSuffix(strChoiceID);
	}

	return strChoiceID;
}
//--------------------------------------------------------------------------------------------------
InputChoiceInfo* FilterOptionsDefinition::getInputChoiceInfo(const string& strChoiceID)
{
	InputChoices::iterator iter = m_mapIDToInputChoice.find(strChoiceID);
	if (iter == m_mapIDToInputChoice.end())
	{
		return NULL;
	}	

	return &(iter->second);
}
//--------------------------------------------------------------------------------------------------
bool FilterOptionsDefinition::isSectionHeader(std::string& strLine)
{
	if (strLine.find("[" + CHARS_ALWAYS_ENABLED + "]") != string::npos)
	{
		strLine = CHARS_ALWAYS_ENABLED;
		return true;
	}
	if (strLine.find("[" + INPUT_TYPES + "]") != string::npos)
	{
		strLine = INPUT_TYPES;
		return true;
	}
	if (strLine.find("[" + CHOICES + "]") != string::npos)
	{
		strLine = CHOICES;
		return true;
	}	

	return false;
}
//--------------------------------------------------------------------------------------------------
void FilterOptionsDefinition::parse(const string& strFODFileFullName)
{
	// FOD Format:
	// There are four sections in an FOD file:
	// [CharsAlwaysEnabled] for a set of chars always used in the current FOD
	// [InputTypes] includes all affected input type for the current fod
	// [Choices] includes all choices for current fod
	// Note: the sequence of these sections can be altered.

	// clear all input choices
	m_mapIDToInputChoice.clear();

	// parse FOD File
	ifstream ifs(strFODFileFullName.c_str());
	// FOD file's comment symbol is //
	CommentedTextFileReader fileReader(ifs, "////", true);

	// while reading the FOD file, we move one line at a time
	// this string will indicate the current section that we are at
	string strCurrentSection("");
	string strLine("");
	do
	{
		strLine = fileReader.getLineText();
		// trim off any leading and trailing spaces 
		strLine = ::trim(strLine, " \t", " \t");
		if (!strLine.empty())
		{
			if (isSectionHeader(strLine))
			{
				// get the section name
				strCurrentSection = strLine;
				// read next non commented line please
				continue;
			}

			// For different section, do different things
			if (strCurrentSection == CHOICES)
			{
				// This line is input choice
				parseInputChoice(strLine);
				continue;
			}
			if (strCurrentSection == INPUT_TYPES)
			{
				m_vecInputTypes.push_back(strLine);
				continue;
			}
			if (strCurrentSection == CHARS_ALWAYS_ENABLED)
			{
				// remove all spaces
				removeAllSpaces(strLine);
				m_strCharsAlwaysEnabled = strLine;
				continue;
			}
		}
	}
	while (!ifs.eof());

	m_bModified = false;
}
//--------------------------------------------------------------------------------------------------
void FilterOptionsDefinition::parseInputChoice(const string& strLine)
{
	// parse string into id, description and chars
	vector<string> vecChoiceInfo;
	StringTokenizer::sGetTokens(strLine, '|', vecChoiceInfo);
	if (vecChoiceInfo.size() < 2 || vecChoiceInfo.size() > 3)
	{
		UCLIDException uclidException("ELI03504", "Invalid string for InputChoice in FOD file.");
		uclidException.addDebugInfo("FOD file name", m_strFODFileName);
		uclidException.addDebugInfo("Line of text", strLine);
		throw uclidException;		
	}

	string strChoiceID(::trim(vecChoiceInfo[0], " \t", " \t"));
	string strDescription(::trim(vecChoiceInfo[1], " \t", " \t"));
	// the input string may or may not have the set of chars for the sub string choice
	string strChars("");
	if ( vecChoiceInfo.size() == 2 || 
		(vecChoiceInfo.size() == 3 && vecChoiceInfo[2].empty()) )
	{
		strChars = sCreateUniqueCharsSet(strDescription);
	}
	else if (vecChoiceInfo.size() == 3)
	{
		strChars = vecChoiceInfo[2];
	}

	// no need for any space in chars set
	removeAllSpaces(strChars);

	// if duplicate choice id exists, give an exception
	if (existsInputChoiceID(strChoiceID))
	{
		UCLIDException uclidException("ELI03694", "Duplicate InputChoice ID found in FOD file.");
		uclidException.addDebugInfo("FOD file name", m_strFODFileName);
		uclidException.addDebugInfo("Line of text", strLine);
		uclidException.addDebugInfo("ChoiceID", strChoiceID);
		throw uclidException;		
	}

	// populate the map
	m_mapIDToInputChoice[strChoiceID] = InputChoiceInfo(strDescription, strChars);
}
//--------------------------------------------------------------------------------------------------
void FilterOptionsDefinition::removeAllSpaces(string& strInOut)
{
	::replaceVariable(strInOut, " ", "");
	::replaceVariable(strInOut, "\t", "");
}
//--------------------------------------------------------------------------------------------------
void FilterOptionsDefinition::writeLine(ofstream& ofs, const string& strLine)
{
	ofs << strLine << endl;
}
