#include "stdafx.h"
#include "RegExLoader.h"

#include "Misc.h"
#include "UCLIDException.h"
#include "EncryptedFileManager.h"
#include "StringTokenizer.h"

//-------------------------------------------------------------------------------------------------
// Consts
//-------------------------------------------------------------------------------------------------
const static string gstrIMPORT = "#import";

//--------------------------------------------------------------------------------------------------
// Local Methods
//--------------------------------------------------------------------------------------------------
void convertFileToLines(const string& strInput, bool bIsFilename, vector<string>& rvecLines)
{
	if (bIsFilename)
	{
		// first make sure the file exists
		if (!isValidFile(strInput))
		{
			UCLIDException ue("ELI07158", "Input file doesn't exist.");
			ue.addDebugInfo("Input File", strInput);
			ue.addWin32ErrorInfo();
			throw ue;
		}

		// If input file is an etf file
		if (getExtensionFromFullPath(strInput, true) == ".etf")
		{
			// Open an input file, which is encrypted
			EncryptedFileManager efm;
			// decrypt the file
			rvecLines = efm.decryptTextFile(strInput);
		}
		else
		{
			// treat the input file as ASCII text file
			ifstream ifs(strInput.c_str());
			
			while (!ifs.eof())
			{
				string strLine("");

				getline(ifs, strLine);

				if (strLine.empty())
				{
					continue;
				}

				// save the line in the vector
				rvecLines.push_back(strLine);
			}
		}
	}
	else	// if input is a block of text
	{
		// delimiter is line feed
		StringTokenizer::sGetTokens(strInput, "\r\n", rvecLines);
	}
}
//-------------------------------------------------------------------------------------------------
vector<string> convertFileToLines(const string& strFilename)
{
	vector<string> vecLines;
	convertFileToLines( strFilename, true, vecLines );
	return vecLines;
}

//-------------------------------------------------------------------------------------------------
// Public Exported Methods
//-------------------------------------------------------------------------------------------------
string getRegExpFromText(const string& strText, const string& strRootFolder,
						 bool bAutoEncrypt/* = false*/, const string& strAutoEncryptKey)
{
	// String to contain the return value
	string strRegExp = "";

	vector<string> vecLines;
	convertFileToLines(strText, false, vecLines);
	
	// Extract comments from the loaded vector of lines
	CommentedTextFileReader ctfr( vecLines, "//");
	RegExLoader loader(strAutoEncryptKey);
	strRegExp = loader.getRegExpFromLines(ctfr, strRootFolder + "\\");

	return strRegExp;
}

//--------------------------------------------------------------------------------------------------
// RegExLoader Class
//--------------------------------------------------------------------------------------------------
RegExLoader::RegExLoader(const string& strAutoEncryptKey/* = ""*/)
: FileObjectLoaderBase(strAutoEncryptKey)
{
}
//--------------------------------------------------------------------------------------------------
void RegExLoader::loadObjectFromFile(string& strRegEx, const string& strFileName)
{
	strRegEx = "";

	vector<string> vecLines;
	convertFileToLines(strFileName, true, vecLines);
	
	// Extract comments from the loaded vector of lines
	CommentedTextFileReader ctfr(vecLines, "//");
	strRegEx = getRegExpFromLines(ctfr, strFileName);
}
//--------------------------------------------------------------------------------------------------
bool RegExLoader::isModified(const string& strFileName)
{
	if (FileObjectLoaderBase::isModified(strFileName))
	{
		return true;
	}
	else
	{
		for (map<string, CachedObjectFromFile<string, RegExLoader>>::iterator iter = 
				m_mapDependentFiles.begin();
			 iter != m_mapDependentFiles.end();
			 iter++)
		{
			if (iter->second.isModified(strFileName))
			{
				return true;
			}
		}
	}

	return false;
}
//--------------------------------------------------------------------------------------------------
string RegExLoader::getRegExpFromLines(CommentedTextFileReader& ctfr, const string& strRootFile)
{
	string strRegExp = "";
	vector<string> vecLineNoComments;

	// Keep track of loaders for files previously referenced in case they are no longer referenced.
	set<string> setFilesNoLongerReferenced;
	for (map<string, CachedObjectFromFile<string, RegExLoader>>::iterator iter = 
			m_mapDependentFiles.begin();
		 iter != m_mapDependentFiles.end();
		 iter++)
	{
		setFilesNoLongerReferenced.insert(iter->first);
	}

	while ( !ctfr.reachedEndOfStream())
	{
		string strCurrent = ctfr.getLineText();
		// check for #import statements
		unsigned long ulStartImport = strCurrent.find(gstrIMPORT);
		if (ulStartImport != string::npos)
		{
			unsigned long ulStartFileName = strCurrent.find_first_not_of(" \t", 
				ulStartImport + gstrIMPORT.size());
			if (ulStartFileName == string::npos)
			{
				UCLIDException ue("ELI09752", "Invalid #import statememt.");
				ue.addDebugInfo("string", strCurrent);
				throw ue;
			}

			// if no root folder is specified then #import statements are invalid
			// and we throw an exception
			if (strRootFile == "\\")
			{
				UCLIDException ue("ELI10018", "#import specified with no root folder.");
				throw ue;
			}

			string strRelName = strCurrent.substr(ulStartFileName);
			string strFullFileName = getAbsoluteFileName(strRootFile, strRelName, false);
			string strFullFileNameUpper = strFullFileName;
			makeUpperCase(strFullFileNameUpper);
			
			setFilesNoLongerReferenced.erase(strFullFileNameUpper);
			if (m_mapDependentFiles.find(strFullFileNameUpper) == m_mapDependentFiles.end())
			{
				m_mapDependentFiles[strFullFileNameUpper] =
					CachedObjectFromFile<string, RegExLoader>(m_strAutoEncryptKey);
			}

			m_mapDependentFiles[strFullFileNameUpper].loadObjectFromFile(strFullFileName);
			string strExp = (string)m_mapDependentFiles[strFullFileNameUpper].m_obj;
			
			// include anything on the line before the #import
			string strLineBegin = strCurrent.substr(0, ulStartImport);
			vecLineNoComments.push_back(strLineBegin + strExp);
		}
		else if ( !strCurrent.empty())
		{
			vecLineNoComments.push_back( strCurrent );
		}
	}

	// Erase any loaders for files that are no longer referenced.
	for (set<string>::iterator iter = setFilesNoLongerReferenced.begin();
		 iter != setFilesNoLongerReferenced.end();
		 iter++)
	{
		m_mapDependentFiles.erase(*iter);
	}

	// Build the regular expression string, removing any whitespace( \f\n\r\t\v)
	strRegExp = asString(vecLineNoComments, true );

	return strRegExp;
}
//--------------------------------------------------------------------------------------------------
