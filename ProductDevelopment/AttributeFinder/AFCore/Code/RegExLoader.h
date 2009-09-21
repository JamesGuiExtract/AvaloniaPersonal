#pragma once

#include <string>

using namespace std;

class RegExLoader
{
public:
	RegExLoader(); // Constructor

	// Loads a regular expression from a file.
	// The input file is auto-encrypted if out of date and if specified by the RDT settings.
	void loadObjectFromFile(string& strRegEx, const string& strFileName);
};