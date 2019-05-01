// GenerateDotNetLicenseIDFiles.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "GenerateDotNetLicenseIDFiles.h"

#include <cpputil.h>
#include <UCLIDException.h>
#include <CommentedTextFileReader.h>

#include <string>
#include <set>
#include <iostream>

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

using namespace std;

//--------------------------------------------------------------------------------------------------
// The application object
//--------------------------------------------------------------------------------------------------
CWinApp theApp;

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
// Header and footer for the LicenseIdName.h file
const string gstrLICENSE_ID_HEADER = "// NOTE: This file is automatically generated. "
"DO NOT MODIFY IT.\n#pragma once\n\nusing namespace System;\n"
"using namespace System::Text;\nusing namespace System::Diagnostics::CodeAnalysis;"
"\n\nnamespace Extract\n{\n\tnamespace Licensing\n\t{\n"
"\t\tpublic enum class LicenseIdName\n\t\t{\n";
const string gstrLICENSE_ID_FOOTER = "\n\t\t};\n\t};\n};\n";

// Split the ELI code so that it will not show up in the duplicates list
// when the list of ELI codes is generated
const string gstrELICODE = "21839";

// Header and footer for the MapLicenseIdToComponentId.h file
const string gstrMAP_FILE_HEADER = "// NOTE: This file is automatically generated. " 
"DO NOT MODIFY IT.\n#pragma once\n\n"
"#pragma unmanaged\n\n" 
"public class ValueMapConversion\n{\npublic:\n" 
"\tstatic unsigned int getConversion(unsigned int uiLicenseID)"
"\n\t{\n\t\tunsigned int nRetVal = 0;\n\n\t\tswitch(uiLicenseID)\n\t\t{\n";
const string gstrMAP_FILE_FOOTER = "\t\tdefault:\n"
"\t\t\tUCLIDException uex(\"ELI" + gstrELICODE + "\", "
"\"Unable to find ID in map!\");\n\t\t\tuex.addDebugInfo(\"LicenseID\", uiLicenseID, true);\n"
"\t\t\tthrow uex;\n\t\t}\n\n\t\treturn nRetVal;\n\t}\n"
"};\n\n#pragma managed";

const string gstrMAP_FILE_CASE_STATEMENT1 = "\t\tcase ";
const string gstrMAP_FILE_CASE_STATEMENT2 = ":\n\t\t\tnRetVal = ";
const string gstrMAP_FILE_CASE_STATEMENT3 = ";\n\t\t\tbreak;\n\n";

// Relative path to the licensing code folder from the ComponentLicenseIDs.h location
const string gstrLICENSING_CODE_DIRECTORY = "..\\..\\..\\..\\..\\RC.Net\\Licensing\\Core\\Code\\";

// Name of the license id and maplicenseid files
const string gstrLICENSE_IDS_FILE = gstrLICENSING_CODE_DIRECTORY + "LicenseIdName.h";
const string gstrMAP_LICENSE_ID_TO_COMPONENT_IDS_FILE = gstrLICENSING_CODE_DIRECTORY +
		"MapLicenseIdsToComponentIds.h";

// Constants for the string find functions
const string gstrWHITE_SPACE = "\t ";

// Constant offset for where on a line in the ComponentLicenseIDs.h the name
// of a component constant starts
const size_t gnCONSTANT_LOCATION = 21;

// Default size for the strings that will hold the generated file contents
const size_t gnDEFAULT_STRING_SIZE = 16000;
//--------------------------------------------------------------------------------------------------
// Helper functions
//--------------------------------------------------------------------------------------------------
// PURPOSE: To show the usage of this application to the user
void displayUsage()
{
	string strUsage = "GenerateDotNetLicenseIDFiles <LicenseIDFile>\n";
	strUsage += "Usage:\n";
	strUsage += "----------\n";
	strUsage += "LicenseIDFile - the path to ComponentLicenseIDs.h (may not be relative)\n";
	strUsage += "Example:\n";
	strUsage += "----------\n";
	strUsage += "GenerateDotNetLicenseIDFiles \"D:\\Engineering\\ReusableComponents";
	strUsage += "\\COMComponents\\UCLIDComponentsLM\\COMLMCore\\Code\\ComponentLicenseIDs.h\"\n";

	AfxMessageBox(strUsage.c_str());
}
//--------------------------------------------------------------------------------------------------
// PURPOSE: To build a map of strings where the keys and values will be used
//			in a replaceVariable call to replace the key value with the data value.
//			This is a helper function used to create a list of values that need to
//			be changed in the ID file list so that FxCop will be happy with it.
//
// NOTE:	If these values are removed from the component ID list then they can
//			be removed from here.
void buildReplacementVariableList(map<string, string>& rmapReplacementVariables)
{
	// Ensure the map is empty to start
	rmapReplacementVariables.clear();

	// Add the necessary values to the list
	rmapReplacementVariables["Flexindex"] = "FlexIndex";
	rmapReplacementVariables["Inputfunnel"] = "InputFunnel";
	rmapReplacementVariables["Idshield"] = "IDShield";
	rmapReplacementVariables["Ruleset"] = "RuleSet";
	rmapReplacementVariables["Fileprocessor"] = "FileProcessor";
	rmapReplacementVariables["Autoredaction"] = "AutoRedaction";
	rmapReplacementVariables["Gridtool"] = "GridTool";
	rmapReplacementVariables["IgnoreUsbIdcheck"] = "IgnoreUsbIdCheck";
	rmapReplacementVariables["Readwrite"] = "ReadWrite";
	rmapReplacementVariables["Scansoft"] = "ScanSoft";
	rmapReplacementVariables["UiObject"] = "UIObject";
	rmapReplacementVariables["Icomap"] = "IcoMap";
	rmapReplacementVariables["Labde"] = "LabDE";
	rmapReplacementVariables["PaginationUi"] = "PaginationUI";
}
//--------------------------------------------------------------------------------------------------
void processComponentIDFile(const string& strComponentIDFile, string& rstrIdFileContents,
							string& rstrMapFileContents)
{
	ifstream fIn(strComponentIDFile.c_str(), ios::in);

	// Ensure the file was opened
	if (!fIn)
	{
		UCLIDException uex("ELI21833", "Unable to open Component ID file!");
		uex.addDebugInfo("Component ID File Name", strComponentIDFile);
		throw uex;
	}

	try
	{
		try
		{
			// Open a commented text file reader
			CommentedTextFileReader fileReader(fIn);

			unsigned long nOffSet = 0;
			set<int> setMappedIDs;

			while(!fileReader.reachedEndOfStream())
			{
				string strLine = fileReader.getLineText();

				// Ignore any blank lines
				if (strLine == "")
				{
					continue;
				}
				
				// ignore the lines that contain the string PASSWORDS
				if (strLine.find("PASSWORDS") == strLine.npos)
				{
					// If we have not found the offset yet, get that
					if (nOffSet == 0 && strLine.find("gnBASE_OFFSET") != strLine.npos)
					{
						// find the beginning of the number
						size_t numBegin = strLine.find_first_of(gstrNUMBERS, 0);

						// find the end of the number
						size_t numEnd = strLine.find_first_not_of(gstrNUMBERS, numBegin);

						// get the offset value from the number
						nOffSet = asUnsignedLong(strLine.substr(numBegin, numEnd - numBegin)); 
					}
					else
					{
						// Get the end of the constant
						size_t nEndConstant = strLine.find_first_of(gstrWHITE_SPACE,
							gnCONSTANT_LOCATION);

						// Get the constant
						string strConst = strLine.substr(gnCONSTANT_LOCATION,
							nEndConstant - gnCONSTANT_LOCATION);

						// Make title case
						makeTitleCase(strConst);

						// Remove the _
						replaceVariable(strConst, "_", "");

						// Get the beginning and ending of the value
						size_t nBeginVal = strLine.find_first_of(gstrNUMBERS, nEndConstant);
						size_t nEndVal = strLine.find(";", nBeginVal);
						
						// https://extract.atlassian.net/browse/ISSUE-12454
						// Make sure the enum values don't change from version to version as
						// new license components are added by basing them on the constant from
						// ComponentLicenseIDs.h.
						unsigned long nComponentId = asUnsignedLong(strLine.substr(nBeginVal,
							nEndVal - nBeginVal));
						unsigned long nOffsetComponentId = nComponentId + nOffSet;

						if (setMappedIDs.find(nComponentId) != setMappedIDs.end())
						{
							THROW_LOGIC_ERROR_EXCEPTION("ELI37590");
						}

						// If this is not the first constant then we want to add a , and a newline
						if (!setMappedIDs.empty())
						{
							rstrIdFileContents += ",\n";
						}

						// Add the contents to the LicenseID file string
						rstrIdFileContents += "\t\t\t" + strConst + " = " + asString(nComponentId);

						// Add the contents to the MapIdToComponentID file
						rstrMapFileContents += gstrMAP_FILE_CASE_STATEMENT1 + asString(nComponentId)
							+ gstrMAP_FILE_CASE_STATEMENT2 + asString(nOffsetComponentId)
							+ gstrMAP_FILE_CASE_STATEMENT3;
						setMappedIDs.insert(nComponentId);
					}
				}
			}

			// Build the list of spelling replacement values
			map<string, string> mapReplacementVariables;
			buildReplacementVariableList(mapReplacementVariables);

			// Need to modify the spelling of some of the values to make FXCop happy
			for (map<string, string>::iterator it = mapReplacementVariables.begin();
				it != mapReplacementVariables.end(); it++)
			{
				replaceVariable(rstrIdFileContents, it->first, it->second);
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI21840");
	}
	catch(UCLIDException& uex)
	{
		// Ensure the file is closed
		if (fIn.is_open())
		{
			fIn.close();
		}

		throw uex;
	}

	// Ensure the file is closed
	if (fIn.is_open())
	{
		fIn.close();
	}
}
//--------------------------------------------------------------------------------------------------
int _tmain(int argc, TCHAR* argv[], TCHAR* envp[])
{
	// initialize MFC and print and error on failure
	if (!AfxWinInit(::GetModuleHandle(NULL), NULL, ::GetCommandLine(), 0))
	{
		_tprintf(_T("Fatal Error: MFC initialization failed\n"));
		return EXIT_FAILURE;
	}
	else
	{
		try
		{
			try
			{
				if (argc != 2)
				{
					displayUsage();
					return EXIT_FAILURE;
				}

				CoInitializeEx(NULL, COINIT_MULTITHREADED);

				// Get the file name from the command line
				string strComponentIDFile(argv[1]);

				// Validate the file's existence
				validateFileOrFolderExistence(strComponentIDFile);

				// Get the directory from the component id file
				string strDirectoryOfComponentIDFile =
					getDirectoryFromFullPath(strComponentIDFile) + "\\";

				// Compute the full path to the license ID enum file from the directory
				string strLicenseIdEnumFile = strDirectoryOfComponentIDFile + gstrLICENSE_IDS_FILE;
				simplifyPathName(strLicenseIdEnumFile);

				// Compute the full path to the map license ID
				// to component id file from the directory
				string strMapLicenseIdToComponentIdFile = strDirectoryOfComponentIDFile +
					gstrMAP_LICENSE_ID_TO_COMPONENT_IDS_FILE;
				simplifyPathName(strMapLicenseIdToComponentIdFile);

				// strings to hold the contents of the computed files
				string strNewIdFileContents = gstrLICENSE_ID_HEADER;
				string strNewMapFileContents = gstrMAP_FILE_HEADER;

				// set the capacity of the strings to be large enough to hold the estimated size
				// of the file, this will help the efficiency of the algorithm as the strings will
				// not have to constantly resize themselves as we add lines to the file. 
				strNewIdFileContents.reserve(gnDEFAULT_STRING_SIZE);
				strNewMapFileContents.reserve(gnDEFAULT_STRING_SIZE);

				// Now process the component ID file
				processComponentIDFile(strComponentIDFile, strNewIdFileContents,
					strNewMapFileContents);

				// Add the footers to the files
				strNewMapFileContents += gstrMAP_FILE_FOOTER;
				strNewIdFileContents += gstrLICENSE_ID_FOOTER; 

				// Now need to compare the newly computed text with the old files to see if we need
				// to replace them
				if (isValidFile(strLicenseIdEnumFile) &&
					isValidFile(strMapLicenseIdToComponentIdFile))
				{
					// Files exist so get their contents
					string strIdFileContents = getTextFileContentsAsString(strLicenseIdEnumFile);
					string strMapFileContents = getTextFileContentsAsString(
						strMapLicenseIdToComponentIdFile);

					// Replace all the \r\n's with \n
					replaceVariable(strIdFileContents, "\r\n", "\n");
					replaceVariable(strMapFileContents, "\r\n", "\n");

					// Need to trim a trailing \n from each string the \n
					// is added by the call to writeToFile
					strIdFileContents.erase(strIdFileContents.length()-1);
					strMapFileContents.erase(strMapFileContents.length()-1);

					if (strIdFileContents == strNewIdFileContents
						&& strMapFileContents == strNewMapFileContents)
					{
						// Contents have not changed so just exit
						return EXIT_SUCCESS;
					}
				}

				// We need to output the files, check for readonly
				// and and set writeable if necessary
				if (isFileReadOnly(strLicenseIdEnumFile))
				{
					// Set to writeable
					SetFileAttributes(strLicenseIdEnumFile.c_str(), FILE_ATTRIBUTE_NORMAL);
				}
				if (isFileReadOnly(strMapLicenseIdToComponentIdFile))
				{
					// Set to writeable
					SetFileAttributes(strMapLicenseIdToComponentIdFile.c_str(),
						FILE_ATTRIBUTE_NORMAL);
				}

				// Ensure the folder exists
				createDirectory(getDirectoryFromFullPath(strLicenseIdEnumFile));

				// Write the files
				writeToFile(strNewIdFileContents, strLicenseIdEnumFile);
				writeToFile(strNewMapFileContents, strMapLicenseIdToComponentIdFile);
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI21828");
		}
		catch(UCLIDException& uex)
		{
			// Just log the exception and write to the error stream that an exception occurred
			// Added as per [DotNetRCAndUtils #131]
			uex.log("", false);
			cerr << "Exception occurred while processing component ID file!" << endl;
			cerr << "A copy of the exception has been written to log file." << endl;
			cerr << "Exception: ,,,,,," << uex.asStringizedByteStream() << endl;
		}
	}

	CoUninitialize();

	return EXIT_SUCCESS;
}
