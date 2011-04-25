#include <CommonToExtractProducts.h>
#include <afxwin.h>

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDCOMUtils\Core\Code\UCLIDCOMUtils.tlb" named_guids
using namespace UCLID_COMUTILSLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDComponentsLM\COMLM\Code\COMLM.tlb" named_guids 
using namespace UCLID_COMLMLib;

#include <stdlib.h>
#include <string>
#include <iostream>
#include <fstream>

using namespace std;

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <FileDirectorySearcher.h>

IRegularExprParserPtr g_ipRegExParser = NULL;
bool g_bUseRegExp = false;
bool g_bVerbose = false;
bool g_bRecursive = false;
bool g_bDirectory = false;
unsigned long g_ulReplaceCount = 0;

DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//------------------------------------------------------------------------------
bool replaceVariableInString(string& s, const string& t1, const string& t2)
{
  // this function replaces all occurrences of t1 in S by t2
  
  size_t findpos;
  bool bReturnType;

  findpos = s.find(t1);
  if(findpos == string::npos)
	  bReturnType = 0;
  else
  {
	  bReturnType = 1;
	  while (findpos != string::npos)
	  {
		 s.replace(findpos, t1.length(), t2);
		 findpos = s.find(t1, findpos + t2.length());
		 g_ulReplaceCount++;
	  }
  }
  return bReturnType;

}
//------------------------------------------------------------------------------
int replaceVariableInFile(const string& strFileName, const std::string& t1, 
						  const std::string& t2)
{
	std::string strFileContents;
	// open the input file and get its contents
	ifstream infile(strFileName.c_str());
	if (!infile)
	{
		cout << "Unable to open file " << strFileName.c_str() << " in read mode!" << endl;
		return EXIT_FAILURE;
	}
	// iterate though the input file, get each line, perform the substitution
	// and write to the output file
	while (infile.good())
	{
		char c;
		infile.get(c);
		strFileContents += c;
	}
	infile.close();

	// search and replace in the fileContents
	string result;
	if(g_bUseRegExp)
	{
		if(g_ipRegExParser)
		{
			result = g_ipRegExParser->ReplaceMatches(_bstr_t(strFileContents.c_str()), _bstr_t(t2.c_str()), VARIANT_FALSE);	
		}
	}
	else
	{
		result = strFileContents;
		replaceVariableInString(result, t1, t2);
	}

	// if the file contents haven't changed we don't need to 
	// write it back out
	if(result == strFileContents)
	{
		return EXIT_SUCCESS;
	}

	// Write the modfified contents back out
	ofstream outfile(strFileName.c_str());
	if (!outfile)
	{
		cout << "Unable to open file " << strFileName.c_str() << " in write mode!" << endl;
		return EXIT_FAILURE;
	}
	
	for(unsigned int i = 0; i < result.length(); i++)
	{
		outfile.put(result.at(i));
	}
	
	if (!outfile)
	{
		cout << "Error writing to modified output to file " << strFileName.c_str() << "." << endl;
		return EXIT_FAILURE;
	}
	outfile.close();
	waitForFileAccess(strFileName, giMODE_READ_ONLY);

	return EXIT_SUCCESS;
}
//------------------------------------------------------------------------------
void displayUsage(void)
{
	// there has to be at least three arguments
	cout << endl;
	cout << "Usage: <filename> <search text> <replace text> [OPTIONS]" << endl;
	cout << "OPTIONS:" << endl;
	cout << "\t/e - Treat search and replace text as regular expressions" << endl;
	cout << "\t/v - Provide verbose output" << endl;
	cout << "\t/d - Treat filename as a directory search" << endl;
	cout << "\t     i.e. c:\\tmp\\*.txt" << endl;
	cout << "\t/r - Recursively search subdirectories" << endl;
	cout << "\t     only has an effect when /d is also specified" << endl;

}
//------------------------------------------------------------------------------
string getFirstArgument(char *pszParamStart, char* &pszParamEnd)
{
	string strToFind = "";

	// determine the second argument
	if (pszParamStart[0] == '"')
	{
		// the second argument may contain spaces
		pszParamStart++;
		
		// determine the ending quote
		pszParamEnd = pszParamStart;
		
		do
		{
			while (*pszParamEnd != '"' && *pszParamEnd != NULL)
				pszParamEnd++;

			// if the pointer is pointing to NULL, then we have a problem
			if (*pszParamEnd == NULL)
			{
				throw string("ERROR: No terminating quote (\") found!");
			}

			// ok, so, the ending quote was there....but we need to make sure
			// that the next character is a space - if the next character is a quote,
			// then the user is using double quotes to indicate the quote character
			char *pszParamEnd2 = pszParamEnd;
			pszParamEnd2++;
			if (*pszParamEnd2 == '"')
			{
				pszParamEnd2++;
				pszParamEnd = pszParamEnd2;
			}
			else if (*pszParamEnd2 == ' ')
			{
				break;
			}
		} while (true);

		*pszParamEnd = NULL;
		pszParamEnd++;

		strToFind = pszParamStart;
		replaceVariableInString(strToFind, "\"\"", "\""); // replace double quotes with single quotes
		return strToFind;
	}
	else if (pszParamStart[0] != NULL)
	{
		// the second argument does not contain spaces
		pszParamEnd = pszParamStart + 1;
		while (*pszParamEnd != ' ' && *pszParamEnd != NULL)
			pszParamEnd++;

		if (*pszParamEnd == ' ')
		{
			*pszParamEnd = NULL;
			pszParamEnd++;
		}

		strToFind = pszParamStart;
		return strToFind;
	}

	throw string("Null argument");
}
//------------------------------------------------------------------------------
// e.g. of argument
// ReplaceString test.txt "A" "A B"
// notes: the first argument is a file name
// notes: the second argument is the string to be searched for
int main(int argc, char *argv[])
{
	string strFileName, strToFind, strToReplace;

	if (argc < 4)
	{
		displayUsage();
		return EXIT_FAILURE;
	}
	else
	{
	
		// the file name is always the first argument
		strFileName = argv[1];
		strToFind = argv[2];
		strToReplace = argv[3];

		int i = 4;
		for(i = 4; i < argc; i++)
		{
			string arg = argv[i];
			if(arg == "/e")
			{
				g_bUseRegExp = true;
			}
			else if(arg == "/v")
			{
				g_bVerbose = true;
			}
			else if(arg == "/r")
			{
				g_bRecursive = true;
			}
			else if(arg == "/d")
			{
				g_bDirectory = true;
			}
			else
			{
				displayUsage();
				return EXIT_FAILURE;
			}
		}
	}

	if(g_bUseRegExp)
	{
		CoInitializeEx(NULL, COINIT_MULTITHREADED);
	}
	try
	{
		if( g_bUseRegExp )
		{
			LicenseManagement::loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);

			// Get a regular expression parser.
			IMiscUtilsPtr ipMiscUtils(CLSID_MiscUtils);
			ASSERT_RESOURCE_ALLOCATION("ELI22294", ipMiscUtils != NULL);
			g_ipRegExParser = ipMiscUtils->GetNewRegExpParserInstance("ReplaceStringApp");
			ASSERT_RESOURCE_ALLOCATION("ELI22279", g_ipRegExParser != NULL);

			// Set the pattern to match.
			g_ipRegExParser->PutPattern(_bstr_t(strToFind.c_str()));
		}


		if(g_bVerbose)
		{
			cout << "Replace String Utility:" << endl;
			cout << "  strToFind    = <" << strToFind.c_str() << ">"  << endl;
			cout << "  strToReplace = <" << strToReplace.c_str() << ">" << endl;
		}

		int success = EXIT_FAILURE;
		if(g_bDirectory)
		{
			FileDirectorySearcher fsd;
			vector<string> files = fsd.searchFiles(strFileName, g_bRecursive);

			for(unsigned int i = 0; i < files.size(); i++)
			{
				if(g_bVerbose)
				{
					cout << "  strFileName  = <" << files[i].c_str() << ">"  << endl;
				}
				success = replaceVariableInFile(files[i], strToFind, strToReplace);
			}
		}
		else
		{
			if(g_bVerbose)
			{
				cout << "  strFileName  = <" << strFileName.c_str() << ">"  << endl;
			}
			success = replaceVariableInFile(strFileName, strToFind, strToReplace);
		}
		return success;
	}
	catch(UCLIDException e)
	{
		string tmp;
		e.asString(tmp);
		cout << tmp;
		return EXIT_FAILURE;
	}

	catch(...)
	{
		cout <<"Unexpected Exception"<<endl;
		return EXIT_FAILURE;
	}
	if(g_bUseRegExp)
	{
		CoUninitialize();
	}	

}
