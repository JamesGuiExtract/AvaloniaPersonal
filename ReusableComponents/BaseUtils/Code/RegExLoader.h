#pragma once

#include "BaseUtils.h"
#include "CachedObjectFromFile.h"
#include "CommentedTextFileReader.h"

#include <string>
#include <vector>
#include <set>
using namespace std;

//-------------------------------------------------------------------------------------------------
// RegExLoader Class
//-------------------------------------------------------------------------------------------------
class EXPORT_BaseUtils RegExLoader : public FileObjectLoaderBase
{
public:
	RegExLoader(const string& strAutoEncryptKey = ""); // Constructor

	// Checks to see if the data in the file has been modified. This includes checking to see if
	// referenced (dependent) files have been modified.
	virtual bool isModified(const string& strFileName);

	// Loads a regular expression from a file.
	// The input file is auto-encrypted if out of date and if specified by the RDT settings.
	void loadObjectFromFile(string& strRegEx, const string& strFileName);

	// Retrieves a regular expression from the specified folder using the provided
	// CommentedTextFileReader.
	string getRegExpFromLines(CommentedTextFileReader& ctfr, const string& strRootFile);

private:

	// Keeps track of files imported (referenced) by this file.
	map<string, CachedObjectFromFile<string, RegExLoader> > m_mapDependentFiles;
};

//-------------------------------------------------------------------------------------------------
// Public Exported Methods
//-------------------------------------------------------------------------------------------------
// PURPOSE: Load a regular expression from a string of text that is in the format of a regular
//			Expression file.
// REQUIRE: - strText must be valid regular expression file text as defined above.
//			- strRootFolder must exist if any #import statements are used in strText
//			- filename must either be absolute or relative to strRootFolder
//			
EXPORT_BaseUtils string getRegExpFromText(const string& strText, const string& strRootFolder,
								bool bAutoEncrypt = false, const string& strAutoEncryptKey = "");
//-------------------------------------------------------------------------------------------------
