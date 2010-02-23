#pragma once

#include "BaseUtils.h"

#include <string>
#include <vector>

using namespace std;

//-------------------------------------------------------------------------------------------------
// RegExLoader Class
//-------------------------------------------------------------------------------------------------
class EXPORT_BaseUtils RegExLoader
{
public:
	RegExLoader(); // Constructor

	// Loads a regular expression from a file.
	// The input file is auto-encrypted if out of date and if specified by the RDT settings.
	void loadObjectFromFile(string& strRegEx, const string& strFileName);
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
