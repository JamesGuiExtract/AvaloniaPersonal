#include "stdafx.h"
#include "FilterSchemeDefinition.h"

#include <cpputil.h>
#include <CommentedTextFileReader.h>
#include <UCLIDException.h>

#include <algorithm>
#include <fstream>
#include <io.h>
#include <SYS\STAT.H>

using namespace std;

const string FilterSchemeDefinition::FILTERING_DISABLED = "FilteringDisabled";
const string FilterSchemeDefinition::EXACT_CASE = "ExactCase";
const string FilterSchemeDefinition::UPPER_CASE = "UpperCase";
const string FilterSchemeDefinition::LOWER_CASE = "LowerCase";

//==============================================================================================
// Publid functions
//==============================================================================================
FilterSchemeDefinition::FilterSchemeDefinition()
:m_pFODs(NULL),
 m_strFSDFileName(""),
 m_bModified(false),
 m_bCurrentSchemeDisabled(true)
{
	m_mapFODNameToFilteringChoices.clear();
}
//--------------------------------------------------------------------------------------------------
FilterSchemeDefinition::FilterSchemeDefinition(const FilterSchemeDefinition& toCopy)
: m_pFODs(toCopy.m_pFODs)
{
	m_bModified = toCopy.m_bModified;
	m_mapFODNameToFilteringChoices = toCopy.m_mapFODNameToFilteringChoices;
	m_strFSDFileName = toCopy.m_strFSDFileName;
	m_bCurrentSchemeDisabled = toCopy.m_bCurrentSchemeDisabled;
}
//--------------------------------------------------------------------------------------------------
FilterSchemeDefinition& FilterSchemeDefinition::operator = (const FilterSchemeDefinition& toAssign)
{
	m_pFODs = toAssign.m_pFODs;
	m_bModified = toAssign.m_bModified;
	m_mapFODNameToFilteringChoices = toAssign.m_mapFODNameToFilteringChoices;
	m_strFSDFileName = toAssign.m_strFSDFileName;
	m_bCurrentSchemeDisabled = toAssign.m_bCurrentSchemeDisabled;

	return *this;
}
//--------------------------------------------------------------------------------------------------
FilterSchemeDefinition::FilterSchemeDefinition(FilterOptionsDefinitions* pFODs)
:m_pFODs(pFODs),
 m_strFSDFileName(""),
 m_bModified(false),
 m_bCurrentSchemeDisabled(true)
{
	m_mapFODNameToFilteringChoices.clear();
}
//--------------------------------------------------------------------------------------------------
void FilterSchemeDefinition::createDefaultScheme()
{
	enableFiltering(true);

	map<string, FilterOptionsDefinition>::iterator itFOD = m_pFODs->begin();
	for (; itFOD != m_pFODs->end(); itFOD++)
	{
		// get all choice ids from the fod
		vector<string> vecChoiceIDs = itFOD->second.getInputChoiceIDs();
		// enable all fod choices
		for (unsigned int ui = 0; ui < vecChoiceIDs.size(); ui++)
		{
			enableInputChoice(itFOD->first, vecChoiceIDs[ui], true);
			setCaseSensitivities(itFOD->first, false, true, true);
		}
	}

	m_strFSDFileName = DEFAULT_SCHEME;
	addDefaultFilteringChoicesForFODs();
	m_bModified = false;
}
//--------------------------------------------------------------------------------------------------
void FilterSchemeDefinition::enableFiltering(bool bEnable)
{
	m_bCurrentSchemeDisabled = !bEnable;
	
	m_bModified = true;
}
//--------------------------------------------------------------------------------------------------
void FilterSchemeDefinition::enableInputChoice(const string& strFODName, 
											   const string& strChoiceID,
											   bool bEnable)
{
	map<string, FilteringChoices>::iterator itMap = m_mapFODNameToFilteringChoices.find(strFODName);
	// FOD exists in the FSD file
	if (itMap != m_mapFODNameToFilteringChoices.end())
	{
		vector<string> &vecChoiceIDs = itMap->second.vecChoiceIDs;
		// whether or not strChoiceID exists in the vec
		vector<string>::iterator itVec = find(vecChoiceIDs.begin(), vecChoiceIDs.end(), strChoiceID);
		// the choice id is found in the vector of choice ids
		if (itVec != vecChoiceIDs.end() && !bEnable)
		{
			// remove the choice id entry from FSD if bEnable is false
			vecChoiceIDs.erase(itVec);
			m_bModified = true;
		}
		else if (itVec == vecChoiceIDs.end() && bEnable)
		{
			// add the choice id if bEnable is true and the choice id can't be found in FSD
			vecChoiceIDs.push_back(strChoiceID);
			m_bModified = true;
		}
	}
	// if the FOD name doesn't exist in current FSD, add if bEnable is true
	else if (bEnable)
	{
		FilteringChoices filteringChoices;
		// add strChoiceID to the vec
		filteringChoices.vecChoiceIDs.push_back(strChoiceID);
		m_mapFODNameToFilteringChoices[strFODName] = filteringChoices;

		m_bModified = true;
	}
}
//--------------------------------------------------------------------------------------------------
void FilterSchemeDefinition::getCaseSensitivities(const string& strFODName, 
												  bool& bExactCase,
												  bool& bUpperCase,
												  bool& bLowerCase)
{
	// by default, it should be exact case
	bExactCase = true;
	bUpperCase = false;
	bLowerCase = false;
	map<string, FilteringChoices>::iterator itMap = m_mapFODNameToFilteringChoices.find(strFODName);
	if (itMap != m_mapFODNameToFilteringChoices.end())
	{
		bExactCase = itMap->second.bExactCase;
		bUpperCase = itMap->second.bUpperCase;
		bLowerCase = itMap->second.bLowerCase;
	}
	// if all three are false, then set it back to default
	if (!bExactCase && !bUpperCase && !bLowerCase)
	{
		bExactCase = true;
	}
}
//--------------------------------------------------------------------------------------------------
vector<string> FilterSchemeDefinition::getFODNames()
{
	vector<string> vecFODNames;
	map<string, FilteringChoices>::iterator itMap = m_mapFODNameToFilteringChoices.begin();
	for (; itMap != m_mapFODNameToFilteringChoices.end(); itMap++)
	{
		vecFODNames.push_back(itMap->first);
	}

	return vecFODNames;
}
//--------------------------------------------------------------------------------------------------
vector<string> FilterSchemeDefinition::getEnabledInputChoiceIDs(const string& strFODName)
{
	vector<string> vecChoiceIDs;

	// find the input type in current FSD
	map<string, FilteringChoices>::iterator itMap = m_mapFODNameToFilteringChoices.find(strFODName);
	if (itMap != m_mapFODNameToFilteringChoices.end())
	{
		vecChoiceIDs = itMap->second.vecChoiceIDs;
	}

	return vecChoiceIDs;
}
//--------------------------------------------------------------------------------------------------
bool FilterSchemeDefinition::isInputChoiceEnabled(const string& strFODName, 
													  const string& strChoiceID)
{
	map<string, FilteringChoices>::iterator itMap = m_mapFODNameToFilteringChoices.find(strFODName);
	if (itMap != m_mapFODNameToFilteringChoices.end())
	{
		vector<string> &vecChoiceIDs = itMap->second.vecChoiceIDs;
		// whether or not strChoiceID exists in the vec
		vector<string>::iterator itVec = find(vecChoiceIDs.begin(), vecChoiceIDs.end(), strChoiceID);

		if (itVec != vecChoiceIDs.end())
		{
			return true;
		}
	}

	return false;
}
//--------------------------------------------------------------------------------------------------
void FilterSchemeDefinition::readFromFile(const string& strFSDFileFullName)
{
	/////////////////////////////
	// FSD file format
	// FilteringEnabled=(0 or 1)
	// 0-many [FOD Name]
	// ExactCase=
	// UpperCase=
	// LowerCase=
	// 0-many choiceids
	/////////////////////////////

	m_strFSDFileName = strFSDFileFullName;

	// reset the contents
	m_mapFODNameToFilteringChoices.clear();

	ifstream ifs(strFSDFileFullName.c_str());
	CommentedTextFileReader fileReader(ifs, "////", true);

	string strLine(::trim(fileReader.getLineText(), " \t", " \t"));
	// looking for the FilteringEnabled value
	// Note that this option must appear as the very first non-commented line
	// in the FSD file
	int nVariabePos = strLine.find(FILTERING_DISABLED+"=");
	if (nVariabePos != string::npos)
	{
		string strSub = strLine.substr(nVariabePos);
		int nValuePos = strLine.find("=");
		if (nValuePos != string::npos)
		{
			m_bCurrentSchemeDisabled = (strSub.substr(nValuePos+1) == "1");
		}
	}

	string strFODName("");
	do
	{
		// trim off any leading and trailing spaces
		strLine = ::trim(fileReader.getLineText(), " \t", " \t");
		if (!strLine.empty())
		{
			// if this line starts with [, then this should be the FOD name
			if (strLine.find("[") == 0)
			{
				::replaceVariable(strLine, "[", "");
				::replaceVariable(strLine, "]", "");
				// get the FOD Name 
				strFODName = strLine;
				// validate the FOD name
				if (m_pFODs->find(strFODName) == m_pFODs->end())
				{
					// can't find the input type in FODs, which means that
					// the [InputType].FOD is no longer exist. We shall
					// skip this input type totally and go on with the next input type
					CString zMsg("Invalid FOD name encountered. Please make sure this input type FOD file exists in the Bin directory.");
					zMsg = zMsg + "\r\n--FOD name: " + strFODName.c_str() + "\r\n--FSD file name:" + strFSDFileFullName.c_str();
					AfxMessageBox(zMsg);
					continue;
				}
			}
			else
			{
				parseFilteringChoices(strFODName, strLine);
			}
		}
	}
	while (!ifs.eof());

	// if there's any fod that's not included in the map
	addDefaultFilteringChoicesForFODs();

	// reset modification flag
	m_bModified = false;
}
//--------------------------------------------------------------------------------------------------
void FilterSchemeDefinition::setCaseSensitivities(const string& strFODName, 
												  bool bExactCase,
												  bool bUpperCase,
												  bool bLowerCase)
{
	// if all three are false, then set it back to default
	if (!bExactCase && !bUpperCase && !bLowerCase)
	{
		bExactCase = true;
	}

	map<string, FilteringChoices>::iterator itMap = m_mapFODNameToFilteringChoices.find(strFODName);
	if (itMap != m_mapFODNameToFilteringChoices.end())
	{
		itMap->second.bExactCase = bExactCase;
		itMap->second.bUpperCase = bUpperCase;
		itMap->second.bLowerCase = bLowerCase;

	}
	else
	{
		FilteringChoices filterChoice;
		filterChoice.bExactCase = bExactCase;
		filterChoice.bUpperCase = bUpperCase;
		filterChoice.bLowerCase = bLowerCase;
		m_mapFODNameToFilteringChoices[strFODName] = filterChoice;
	}

	m_bModified = true;
}
//--------------------------------------------------------------------------------------------------
void FilterSchemeDefinition::writeToFile(const string& strFSDFileFullName)
{
	m_strFSDFileName = strFSDFileFullName;

	// make sure the file has read/write permission if exists
	if (fileExistsAndIsReadOnly(strFSDFileFullName))
	{
		// change the mode to read/write
		if (_chmod(strFSDFileFullName.c_str(), _S_IREAD | _S_IWRITE) == -1)
		{
			CString zMsg("Failed to save ");
			zMsg += strFSDFileFullName.c_str();
			zMsg += ". Please make sure the file is not shared by another program.";
			// failed to change the mode
			AfxMessageBox(zMsg);
			return;
		}
	}

	// always overwrite if the file exists
	ofstream ofs(strFSDFileFullName.c_str(), ios::out | ios::trunc);

	// first line is for FilteringDisabled
	string strDisabled(m_bCurrentSchemeDisabled?"1":"0"); 
	ofs << FILTERING_DISABLED << "=" << strDisabled << endl;

	// iterate through the map 
	string strFlag("");
	map<string, FilteringChoices>::iterator itMap = m_mapFODNameToFilteringChoices.begin();
	for (; itMap != m_mapFODNameToFilteringChoices.end(); itMap++)
	{
		// the input type
		ofs << endl << "[" << itMap->first << "]" << endl;
		// the flags
		strFlag = itMap->second.bExactCase ? "1" : "0";
		ofs << EXACT_CASE << "=" << strFlag << endl;
		strFlag = itMap->second.bUpperCase ? "1" : "0";
		ofs << UPPER_CASE << "=" << strFlag << endl;
		strFlag = itMap->second.bLowerCase ? "1" : "0";
		ofs << LOWER_CASE << "=" << strFlag << endl;
		// the choice ids
		vector<string> &vecChoiceIDs = itMap->second.vecChoiceIDs;
		for (unsigned int ui = 0; ui < vecChoiceIDs.size(); ui++)
		{
			ofs << vecChoiceIDs[ui] << endl;
		}
	}

	// Close the file and wait for it to be readable
	ofs.close();
	waitForFileToBeReadable(strFSDFileFullName);

	// reset modification flag
	m_bModified = false;
}


//==============================================================================================
// Private functions
//==============================================================================================
void FilterSchemeDefinition::addDefaultFilteringChoicesForFODs()
{
	FilterOptionsDefinitions::iterator itFOD = m_pFODs->begin();
	for (; itFOD != m_pFODs->end(); itFOD++)
	{
		string strFODName(itFOD->first);
		// compare FOD names in m_pFODs with FOD names in m_mapFODNameToFilteringChoices
		// if FOD name only exist in m_pFODs, then add the default entry to 
		// m_mapFODNameToFilteringChoices
		map<string, FilteringChoices>::iterator itFODToFilteringChoices 
			= m_mapFODNameToFilteringChoices.find(strFODName);
		if (itFODToFilteringChoices == m_mapFODNameToFilteringChoices.end())
		{
			FilteringChoices filteringChoices;
			filteringChoices.bExactCase = false;
			filteringChoices.bLowerCase = true;
			filteringChoices.bUpperCase = true;
			vector<string> vecChoiceIDs = itFOD->second.getInputChoiceIDs();
			for (unsigned int ui = 0; ui < vecChoiceIDs.size(); ui++)
			{
				filteringChoices.vecChoiceIDs.push_back(vecChoiceIDs[ui]);
			}
			m_mapFODNameToFilteringChoices[strFODName] = filteringChoices;
		}
	}
}
//--------------------------------------------------------------------------------------------------
void FilterSchemeDefinition::parseFilteringChoices(const string& strFODName, const string& strForParse)
{
	if (strFODName.empty())
	{
		UCLIDException uclidException("ELI03505", "FOD name can't be empty.");
		uclidException.addDebugInfo("LineText", strForParse);
		throw uclidException;
	}
	
	// assume the input type exists in the map
	map<string, FilteringChoices>::iterator itMap = m_mapFODNameToFilteringChoices.find(strFODName);
	// add the fod entry if not already in the map
	if (itMap == m_mapFODNameToFilteringChoices.end())
	{
		FilteringChoices filterChoices;
		m_mapFODNameToFilteringChoices[strFODName] = filterChoices;
		itMap = m_mapFODNameToFilteringChoices.find(strFODName);
	}

	int nFoundPos = strForParse.find_first_of("=");
	// this is choice id
	if (nFoundPos == string::npos)
	{
		// validate the choice id
		FilterOptionsDefinitions::iterator itFOD = m_pFODs->find(strFODName);
		if (itFOD == m_pFODs->end())
		{
			// the FOD doesn't exist, skip it
			return;
		}
		
		if (!itFOD->second.existsInputChoiceID(strForParse))
		{
			return;
		}
		
		itMap->second.vecChoiceIDs.push_back(strForParse);
	}
	else if (strForParse.find(EXACT_CASE) != string::npos)
	{				// take the string after the "="
		itMap->second.bExactCase = strForParse.substr(nFoundPos+1) == "1";
	}
	else if (strForParse.find(UPPER_CASE) != string::npos)
	{
		// take the string after the "="
		itMap->second.bUpperCase = strForParse.substr(nFoundPos+1) == "1";
	}
	else if (strForParse.find(LOWER_CASE) != string::npos)
	{		
		// take the string after the "="
		itMap->second.bLowerCase = strForParse.substr(nFoundPos+1) == "1";
	}
	
}
//--------------------------------------------------------------------------------------------------
