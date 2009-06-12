#include <CommonToExtractProducts.h>
#include <afxwin.h>
#include <afxdisp.h>
#include <afxdlgs.h>

#include <string.h>
#include <stdlib.h>
#include <io.h>

#include <string>
#include <iostream>
#include <fstream>

#include <TemporaryFileName.h>
#include <cpputil.h>

using namespace std;

//------------------------------------------------------------------------------
void displayUsage(void)
{
	// there has to be at least three arguments
	cout << endl;
	cout << "Please provide three arguments:" << endl;
	cout << "AutoIncrement Vss Label Utility:" << endl;
	cout << " - /p<Vss Project>"  << endl;
	cout << " - [Optional]/l<Label prefix>. eg. \"AttributeFinder Ver.\"."  << endl;
	cout << "   Note: if this argument is not provided, the application will only" << endl;
	cout << "   read last labeled version. If the argument is specified, it will read" << endl;
	cout << "   all labeled versions of the given project." << endl;
	cout << " - [Optional]/i -- force incrementing the label regardless" << endl;
	cout << "   whether there are activities since last label."  << endl;
	cout << endl;
	cout << "Please note that the argument can contain spaces if it " << endl;
	cout << "starts and ends with a quote (\")" << endl;
	cout << endl;
}
//------------------------------------------------------------------------------
string getCurrentDate()
{
	// get current date
	__time64_t currTime;
	tm tmToday;
	time( &currTime );
	int iError = _localtime64_s( &tmToday, &currTime );

	// Convert time to string
	int iMonth = tmToday.tm_mon + 1;
	int iDay = tmToday.tm_mday;
	int iYear = tmToday.tm_year + 1900;
	string strMon = asString(iMonth);
	string strDay = asString(iDay);
	string strYear = asString(iYear);

	// return in the format of d(d)?/m(m)?/yyyy
	return strMon + "/" + strDay + "/" + strYear;
}
//------------------------------------------------------------------------------
// reading the specified project history from last labeled date to today
// it also return the last label string
bool readProjectHistory(const string& strProjectName,
						const string& strLabelPrefix,
						const string& strTempFileName,
						string& strLastLabel)
{
	TemporaryFileName tempFile("History_", ".tmp", true);
	// first get last labeled version
	string strCommand;
	strCommand = "ss history ";
	strCommand += strProjectName;
	// default
	string strOption(" -L -#1 -O");
	if (!strLabelPrefix.empty())
	{
		// if label prefix is not empty, get all labeled versions
		strOption = " -L -O";
	}
	strCommand += strOption;
	strCommand += tempFile.getName();
	
	if (system(strCommand.c_str()) == -1)
	{
		displayUsage();
		cout << "Unable to determine history of specified project: " << endl;
		system((string("type ") + tempFile.getName()).c_str());
		return false;
	}

	string strFromDate("");
	// get the date of this version
	ifstream infile(tempFile.getName().c_str());
	string strLine;
	// whether or not to find date
	bool bSearchDateStart = false;
	while (infile)
	{
		getline(infile, strLine);
		if (strLine.empty())
		{
			continue;
		}

		string strLabelToFind = ("Label:");
		if (!strLabelPrefix.empty())
		{
			strLabelToFind = "Label: \"" + strLabelPrefix;
		}

		int nLastLabelPos = strLine.find(strLabelToFind);
		if (nLastLabelPos != string::npos)
		{
			// get the label
			strLastLabel = strLine.substr(nLastLabelPos+6);
			// trim off leading/trailing space and "
			strLastLabel = ::trim(strLastLabel, " \t\"", " \t\"");
			bSearchDateStart = true;
			continue;
		}

		// only if it's time to search for the Date
		if (bSearchDateStart)
		{
			int nDatePos = strLine.find("Date:");
			int nTimePos = strLine.find("Time:");
			if (nDatePos == string::npos 
				|| nTimePos == string::npos
				|| nTimePos <= nDatePos
				|| nDatePos == 0)
			{
				continue;
			}
			
			// get the date
			strFromDate = strLine.substr(nDatePos+5, nTimePos-nDatePos-5);
			// trim off any leading/trailing spaces
			strFromDate = ::trim(strFromDate, " \t", " \t");
			// the Date is found, let's get out of the loop
			break;
		}
	}

	if (strFromDate.empty())
	{
		displayUsage();
		cout << "Unable to get history of specified project: " << endl;
		system((string("type ") + tempFile.getName()).c_str());
		return false;
	}

	string strCurrentDate = getCurrentDate();

	// query for history from strFromDate till strCurrentDate for this project
	strCommand = "ss history ";
	strCommand += strProjectName;
	strCommand += " -Vd" + strCurrentDate + "~" + strFromDate + ";8:00a";
	strCommand += " -R -O";
	strCommand += strTempFileName;
	
	if (system(strCommand.c_str()) == -1)
	{
		displayUsage();
		cout << "Unable to determine history of specified project: " << endl;
		system((string("type ") + strTempFileName).c_str());
		return false;
	}

	return true;
}
//------------------------------------------------------------------------------
// REQUIRE : strNewLabel shall contain no quotes around
bool labelProject(const string& strProjectName, 
				  const string& strNewLabel,
				  bool bAlwaysIncrement = false)
{
	string strCommand;
	strCommand += "ss label ";
	strCommand += strProjectName;
	strCommand += " -I- -L\"";
	strCommand += strNewLabel;
	strCommand += "\" -O";
	
	if (!bAlwaysIncrement)
	{
		// ask the user if they would like to actually perform the labeling:
		bool bInputObtained = false;
		bool bApplyLabel = false;
		do
		{
			cout << endl;
			cout << "Apply new label " << strNewLabel << "? [Y/N]";
			int c = getchar();
			if (c == 'y' || c == 'Y')
			{
				bInputObtained = true;
				bApplyLabel = true;
			}
			else if (c == 'n' || c == 'N')
			{
				bInputObtained = true;
				bApplyLabel = false;
			}
		} while (!bInputObtained);

		if (!bApplyLabel)
		{
			// no label update
			return true;
		}
	}
	else
	{
		cout << "Apply new label " << strNewLabel << endl;
	}
	
	if (system(strCommand.c_str()) == -1)
	{
		displayUsage();
		cout << "Unable to update label of specified project: " << endl;
		return false;
	}
	
	return true;
}
//--------------------------------------------------------------------------------------------------
// Whether or not there's activities since last label
// strProjHistoryFile - the file contains project history
bool isActiveSinceLastLabel(const string& strProjHistoryFile,
							const string& strLabelPrefix)
{
	string strLine("");
	string strUser("User:");
	string strLabel("Label:");

	// the string "User:" found position
	int nUserPos = string::npos;
	// the string "Label:" found position
	int nLabelPos = string::npos;

	// the way to tell if there's activity depends on whether
	// or not the label prefix is empty
	if (strLabelPrefix.empty())
	{
		ifstream infile(strProjHistoryFile.c_str());
		while (infile)
		{
			getline(infile, strLine);
			if (strLine.empty())
			{
				continue;
			}
			
			if (nUserPos == string::npos)
			{
				nUserPos = strLine.find(strUser);
			}
			
			if (nLabelPos == string::npos)
			{
				nLabelPos = strLine.find(strLabel);
			}
			
			if (nUserPos != string::npos && nLabelPos == string::npos)
			{
				return true;
			}
			if (nUserPos == string::npos && nLabelPos != string::npos)
			{
				return false;
			}
		}
	}
	else
	{
		string strLabelWithPrefix = strLabel + " \"" + strLabelPrefix;
		// how many "User:" is found before the prefixed label is found.
		// exclude any "User:" that is right under "Label:"
		int nNumOfUserKeywords = 0;
		int nLabelWithPrefixPos = string::npos;
		bool bRecordNumOfUserKeyword = true;
		ifstream infile(strProjHistoryFile.c_str());
		while (infile)
		{
			getline(infile, strLine);
			if (strLine.empty())
			{
				continue;
			}

			nLabelPos = strLine.find(strLabel);
			nLabelWithPrefixPos = strLine.find(strLabelWithPrefix);
			if (nLabelWithPrefixPos != string::npos)
			{
				if (nNumOfUserKeywords > 0)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else if (nLabelPos != string::npos)
			{
				// if a "Label:" is found, don't count the following "User:"
				bRecordNumOfUserKeyword = false;
				continue;
			}

			nUserPos = strLine.find(strUser);
			if (nUserPos != string::npos && bRecordNumOfUserKeyword)
			{
				nNumOfUserKeywords++;
			}
			else if (nUserPos != string::npos && !bRecordNumOfUserKeyword)
			{
				// don't count the number, set flag to true
				// so that next user will be counted
				bRecordNumOfUserKeyword = true;
			}
		}
	}

	return false;
}
//------------------------------------------------------------------------------
// Get only labeled history for specified project path, no recursive search
// is performed. Get the latest label, and increment the number by 1.
int IncrementProjectLabel(const string& strVssProject, 
						  const string& strLabelPrefix,
						  bool bAlwaysIncrement = false)
{
	TemporaryFileName tempFile("Label_", ".tmp", true);

	// status update on the console
	cout << "Getting project history from Visual SourceSafe..." << endl;
	
	string strLastLabel("");
	// read project history
	if (!readProjectHistory(strVssProject, strLabelPrefix, tempFile.getName(), strLastLabel))
	{
		return EXIT_FAILURE;
	}

	// status update on the console
	cout << "Processing project history information..." << endl;

	// if increment label or not depends on the fact
	// 1) there are some activities since last label
	// 2) need to ask the user for incrementing the label or not if 
	//    there's activity since last label
	if (!bAlwaysIncrement)
	{
		bool bActivitySinceLastLabel = isActiveSinceLastLabel(tempFile.getName(), strLabelPrefix);
		if (bActivitySinceLastLabel)
		{
			// there has been activity since the last label
			cout << endl;
			cout << "Last label: " << strLastLabel << endl;
			cout << endl;
			
			string strNewLabel = ::incrementNumericalSuffix(strLastLabel);
			
			// label or not
			if (!labelProject(strVssProject, strNewLabel, bAlwaysIncrement))
			{
				return EXIT_FAILURE;
			}
		}
		else
		{
			// there has been no activity since the last label
			cout << endl;
			cout << "Last label: " << strLastLabel.c_str() << endl;
			cout << "There has been no activity since the last label!" << endl;
			cout << endl;
		}
	}
	else
	{
		string strNewLabel = ::incrementNumericalSuffix(strLastLabel);
		// if always force incrementing label, no need to check
		// the activity or ask the user
		if (!labelProject(strVssProject, strNewLabel, bAlwaysIncrement))
		{
			return EXIT_FAILURE;
		}
	}

	return EXIT_SUCCESS;
}
//------------------------------------------------------------------------------
int main(int argc, char *argv[])
{
	if (argc != 2 && argc != 3 && argc != 4)
	{
		displayUsage();
		return EXIT_FAILURE;
	}

	string strVssProject("");
	string strLabelPrefix("");
	string strAlwaysIncrement("");
	bool bAlwaysIncrementLabel = false;
	for (int n=1; n<argc; n++)
	{
		string strArgument = argv[n];
		if (strArgument.find("/p") != string::npos 
			|| strArgument.find("/P") != string::npos)
		{
			strVssProject = strArgument.substr(2);
		}
		else if (strArgument.find("/l") != string::npos
			|| strArgument.find("/L") != string::npos)
		{
			strLabelPrefix = strArgument.substr(2);
		}
		else if (strArgument.find("/i") != string::npos 
			|| strArgument.find("/I") != string::npos)
		{
			bAlwaysIncrementLabel = true;
		}
		else
		{
			displayUsage();
			return EXIT_FAILURE;
		}
	}
	
	cout << endl;
	cout << "AutoIncrement Vss Label Utility:" << endl;
	cout << "Vss Project = " << strVssProject << endl;
	if (!strLabelPrefix.empty())
	{
		cout << "Label Prefix = " << strLabelPrefix << endl;
	}
	cout << "Always increment label = " << bAlwaysIncrementLabel << endl;
	cout << endl;

	int nRes = IncrementProjectLabel(strVssProject, strLabelPrefix, bAlwaysIncrementLabel);

	return nRes;
}
