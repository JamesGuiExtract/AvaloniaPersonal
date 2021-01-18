//==================================================================================================
//
// COPYRIGHT (c) 2007 EXTRACT SYSTEMS, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	LogProcessStats.cpp
//
// PURPOSE:	Defines the entry point for a console application that will allow a user to specify
//			a list of process names and/or process ids and will then log data from the process
//			various lines of text so that the paragraph of text appears neatly indented like this
//			paragraph.
//
// AUTHORS:	Jeff Shergalis
//			Arvind Ganesan
//
//==================================================================================================

// LogProcessInfo.cpp : Defines the entry point for the console application.
//
#include "stdafx.h"
#include "ProcessInformationLogger.h"

#include <cpputil.h>
#include <UCLIDException.h>
#include <UCLIDExceptionDlg.h>
#include <StringTokenizer.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

#include <iostream>
#include <string>
#include <set>
#include <vector>
#include <memory>
#include <ctype.h>

// The one and only application object
CWinApp theApp;

using namespace std;

DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//--------------------------------------------------------------------------------------------------
// Globals
//--------------------------------------------------------------------------------------------------
unique_ptr<ProcessInformationLogger> gapLogProcessInfo; // pointer to the ProcessInformationLogger

bool bCtrlCHandled = false;

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
// these constants are used in computing the refresh intervals in the function getRefreshInterval
const long glMILLISECONDS_MULTIPLIER = 1;
const long glSECONDS_MULTIPLIER	= 1000;
const long glMINUTES_MULTIPLIER = 60000;
const long glHOURS_MULTIPLIER = 3600000;

//--------------------------------------------------------------------------------------------------
// PURPOSE: Display a usage message to the user in a MessageBox
void displayUsage(const string& rstrError)
{
	string strMessage = "";
	if (rstrError == "")
	{
		strMessage += "LogProcessStats (c) 2021 Extract Systems, LLC.  All Rights Reserved\n";
		strMessage += "LogProcessStats allows a user to specify a list of Process Names\n";
		strMessage += "and/or Process IDs that they would like performance data recorded for.\n";
		strMessage += "This data will be recorded in a collection of csv files with names\n";
		strMessage += "unique to the selected process.  It also allows the user to specify\n";
		strMessage += "a refresh interval specifiying how often to take a snapshot of the data\n";
		strMessage += "\n";
	}
	else
	{
		strMessage += "ERROR!\n------\n";
		strMessage += rstrError + "\n------\n";
	}
	strMessage += "LogProcessStats.exe <process_id|process_name> <refresh_interval> ";
	strMessage += "<log_files_directory> [/s <serverName> /d <databaseName>] [/el]\n";
	strMessage += "Other flags: [/?]\n";
	strMessage += "\n";
	strMessage += "Usage:\n";
	strMessage += "------\n";
	strMessage += "Required Arguments:\n";
	strMessage += "<process_id|process_name>: A comma separated list of process names.\n";
	strMessage += "\tand/or process ids to monitor and log data for\n";
	strMessage += "<refresh_interval>: The delay between log entries. The interval is assumed to\n";
	strMessage += "\tbe in seconds unless ms, s, m, or h is specified (e.g. 22m would be\n";
	strMessage += "\tread as 22 minutes.\n";
	strMessage += "<log_files_directory>: The directory in which you would like the application \n";
	strMessage += "\tto place the log files.\n";
	strMessage += "\n";
	strMessage += "Optional Argmuments:\n";
	strMessage += "/s <serverName>  /d <databaseName> : Logs locks in the database, <databaseName> on server <serverName>\n ";
	strMessage += "\tData is logged to file DBLocks.csv\n";
	strMessage += "/el: Log exceptions, do not display them\n";
	strMessage += "/?:  Display this help\n";
	strMessage += "\n";
	strMessage += "Output:\n";
	strMessage += "-------\n";
	strMessage += "This application will log the following fields of data separated by commas\n";
	strMessage += "to a log file 'process_name.pid.csv'. If you select a process_name or \n";
	strMessage += "multiple p_id's there will also be a file called all.csv which contains \n";
	strMessage += "the sum of the data for all processes we are logging:\n";
	strMessage += "  Date - MM/DD/YYYY\n";
	strMessage += "  Time - HH:MM:SS\n";
	strMessage += "  Timestamp - the timestamp for the current time returned by the OS\n";
	strMessage += "  Process Name - the name of the process (without the extension)\n";
	strMessage += "  ProcessID - the ID assigned to this process by the operating system\n";
	strMessage += "  TMBytes - the total memory used by the process\n";
	strMessage += "  VMBytes - the total virtual memory allocated to\n";
	strMessage += "\t(but not necessarily used by) the process\n";
	strMessage += "  HandleCount - the total # of handles in use by the process\n";
	strMessage += "  ThreadCount - the total # of threads in use by the process\n";
	strMessage += "  BaseLineTimeStamp - a time stamp starting at 0 based off of the\n";
	strMessage += "\tinitial timestamp.\n";

	// display message box with the usage
	MessageBox(NULL, strMessage.c_str(), "Usage", MB_OK | MB_ICONINFORMATION);
}
//--------------------------------------------------------------------------------------------------
// PURPOSE: To tokenize the pid|pname list that was passed in on the command line. 
//
// REQUIRE: A comma separated list of Process ID's and Process Names.
//
// PROMISE: Will populate two sets, one of Process ID's the other of Process Names.
//			If there are no Process ID's or Process Names the corresponding set will
//			be left empty.  In the case where a Process ID or Process Name is repeated
//			it will only appear in the set once (e.g. 123,456,123 would return a Process
//			ID set {123, 456}
//
// NOTE:	There is a piece of confusing behaviour in this method.  We call the asLong
//			method on a string which either returns a long or throws an exception, in the case
//			that an exception is thrown, we catch it and assume that since it is not a number it
//			must be process name and so we insert that in the PName set
void getProcessIdsAndNames(const string& rstrListOfPidAndName, set<long>& rsetPIDs, 
						   set<string>& rsetPNames)
{
	try
	{
		vector<string> vecTokens;
		StringTokenizer myStringTokenizer(',');
		myStringTokenizer.parse(rstrListOfPidAndName, vecTokens);

		for( vector<string>::iterator vecIt = vecTokens.begin(); vecIt != vecTokens.end(); vecIt++ ) 
		{
			// asLong will throw an exception if the string is not a number
			// we catch this and then assign that data to the process name list
			// since we know that the string must contain characters
			try 
			{
				rsetPIDs.insert(asLong(*vecIt));
			}
			catch (...)
			{
				// since we need to compare process names without the .exe on the
				// end we need to check to make sure the user did not enter the process
				// name with the .exe, if they did we just trim it off
				string strTemp(*vecIt);
				string::size_type loc = strTemp.find(".exe",0);
				if (loc != string::npos)
				{
					strTemp.erase(loc);
				}
				makeLowerCase(strTemp);
				rsetPNames.insert(strTemp);
			}
		}
	}
	catch (UCLIDException& ue)
	{
		UCLIDException uexOuter("ELI16733", "Unable to parse process IDs and names!", ue);
		uexOuter.addDebugInfo("strListOfPidAndName", rstrListOfPidAndName);
		throw uexOuter;
	}
}
//--------------------------------------------------------------------------------------------------
// PURPOSE: Take the command line argument representing the refresh interval and convert it to
//			a long.  Since the user can specify what the units of the interval is we will
//			check what those units are and multiply by the appropriate value to get the
//			interval into milliseconds
//
// REQUIRE: A string representing a valid long optionally followed by one of the following 
//			time units: ms, s, m or h.
//
// PROMISE: Will return a valid refresh interval in milliseconds.
long getRefreshInterval(const string& rstrRefreshInterval)
{
	try
	{
		long i = 0;
		long lLength = (long)rstrRefreshInterval.length();
		// we need to find the tag at the end of the number, loop through the string until
		// we find a character that is not a digit.
		while (isdigit(rstrRefreshInterval[i]) && i < lLength)
		{
			i++;
		}

		// set the number part of the string (if there was only a number part this will
		// end up containing the whole string. we define these here so that they are
		// visible to the exception and return at the end of the function
		string strNumberPart = rstrRefreshInterval.substr(0,i);
		string strUnitPart("");
		long lTimeMultiplier = 0;
		// if we didn't find any characters at the end default to seconds
		if (i == lLength)
		{
			lTimeMultiplier = glSECONDS_MULTIPLIER;
		}
		else
		{
			// if we are here then the number contains a string on the end, we need to get
			// that string.
			strUnitPart = rstrRefreshInterval.substr(i,lLength-i);

			// our program needs the value in milliseconds, check to see what the
			// user specified and convert it to milliseconds
			if (strUnitPart == "ms")
			{
				lTimeMultiplier = glMILLISECONDS_MULTIPLIER;
			}
			else if (strUnitPart == "s")
			{
				lTimeMultiplier = glSECONDS_MULTIPLIER;
			}
			else if (strUnitPart == "m")
			{
				lTimeMultiplier = glMINUTES_MULTIPLIER;
			}
			else if (strUnitPart == "h")
			{
				lTimeMultiplier = glHOURS_MULTIPLIER;
			}
		}
		// check to see if we ever set the multiplier, if we did not then the user specified
		// an unrecognized time and we need to throw an exception.
		if (lTimeMultiplier == 0)
		{
			// here the user specified some unknown time specification (not ms,s,m, or h)
			UCLIDException ue("ELI16778", 
				"Refresh interval contained an unrecognized time specifier.");
			ue.addDebugInfo("Time specifier", strUnitPart);
			ue.addDebugInfo("Whole string", rstrRefreshInterval);
			throw ue;
		}

		return (lTimeMultiplier * asLong(strNumberPart));
	}
	catch (UCLIDException& ue)
	{
		UCLIDException uexOuter("ELI16671", "Unable to determine refresh interval!", ue);
		uexOuter.addDebugInfo("Refresh Interval", rstrRefreshInterval);
		throw uexOuter;
	}
}
//--------------------------------------------------------------------------------------------------
// PURPOSE: To check the user entered working directory from the command line by verifying that it 
//			is a valid directory and checking that it does not contain any csv files
//
// REQUIRE: A string that represents a valid directory that exists on the system.  If the directory
//			is not found to exist we will throw an exception
//
// PROMISE: If this method returned then you can safely write CSV files to this directory
void checkWorkingDirectory(const string& rstrWorkingDirectory)
{
	// check to see if there are .csv files in the working directory
	vector<string> vecCSVFilesInDirectory;
	getFilesInDir(vecCSVFilesInDirectory, rstrWorkingDirectory, "*.csv");		
	if (vecCSVFilesInDirectory.size() > 0) 
	{
		UCLIDException ue("ELI16670", 
			"Please delete all CSV files from the working directory before running this application!");
		ue.addDebugInfo("Directory Path", rstrWorkingDirectory);
		throw ue;
	}		
}
//--------------------------------------------------------------------------------------------------
// PURPOSE: To validate the license of this application
void validateLicense()
{
	VALIDATE_LICENSE(gnEXTRACT_CORE_OBJECTS, "ELI20431", "LogProcessStats");
}

//--------------------------------------------------------------------------------------------------
// Message Handlers
//--------------------------------------------------------------------------------------------------
// PURPOSE: To handle keyboard commands from the console
BOOL WINAPI ConsoleHandler(DWORD dEvent)
{
	BOOL bHandled = FALSE;

	switch(dEvent)
	{
	case CTRL_C_EVENT:
		{
			if (!bCtrlCHandled && (gapLogProcessInfo.get() != NULL))
			{
				cout << "Ending statistics logging.  Please wait a moment.\n";

				// signal the logger to stop logging
				gapLogProcessInfo->end();

				bCtrlCHandled = true;
			}

			// set message to handled
			bHandled = TRUE;
		}
		break;
	}

	return bHandled;
}

//--------------------------------------------------------------------------------------------------
// Main Application
//--------------------------------------------------------------------------------------------------
int _tmain(int argc, TCHAR* argv[], TCHAR* envp[])
{
	try
	{
		// declared here so it is accesible in the try catch block
		bool bDisplayExceptions = true;

		try
		{
			try
			{
				// initialize MFC and print and error on failure
				if (!AfxWinInit(::GetModuleHandle(NULL), NULL, ::GetCommandLine(), 0))
				{
					// TODO: change error code to suit your needs
					_tprintf(_T("Fatal Error: MFC initialization failed\n"));
					return EXIT_FAILURE;
				}
				else
				{
					// Setup exception handling
					UCLIDExceptionDlg exceptionDlg;
					UCLIDException::setExceptionHandler( &exceptionDlg );

					string databaseName = "";
					string databaseServer= "";

					// init license management [p13 #4882]
					LicenseManagement::loadLicenseFilesFromFolder(
						LICENSE_MGMT_PASSWORD);

					// check license
					validateLicense();

					// install keyboard message handler
					if (!asCppBool(SetConsoleCtrlHandler((PHANDLER_ROUTINE)ConsoleHandler, TRUE)))
					{
						UCLIDException ue("ELI20435", "Unable to install keystroke handler!");
						throw ue;
					}

					// check to see if the user only specified one argument, if they did then 
					// we need to check to make sure it was /?.  if it was something else 
					// then flag as an error and quit
					if (argc == 2)
					{
						string strTemp(argv[1]);
						if (strTemp == "/?")
						{
							displayUsage("");
							return EXIT_SUCCESS;
						}
						displayUsage("Should have at least one argument on the command line!");
						return EXIT_FAILURE;
					}

					// check to ensure there were between 3 and 4 command line arguments (since
					// we have the optional /el that can appear at the end of the argument list
					if (argc < 4 || argc > 9)
					{
						displayUsage("Incorrect number of arguments on the command line!");
						return EXIT_FAILURE;
					}
					
					// Check for optional arguments
					if (argc >= 5)
					{
						for (int i = 4; i < argc; i++)
						{
							string strTemp(argv[i]);
							if (strTemp == "/el")
							{
								bDisplayExceptions = false;
							}
							else if (strTemp == "/d")
							{
								i++;
								if (i < argc)
								{
									databaseName = argv[i];
								}
							}
							else if (strTemp == "/s")
							{
								i++;
								if (i < argc)
								{
									databaseServer = argv[i];
								}
							}
							else
							{
								string strTemp("Invalid command line string.\nUnrecognized argument: ");
								strTemp.append(argv[i]);
								displayUsage(strTemp);
								return EXIT_FAILURE;
							}
						}
					}

					// initialize our variables and get command line arguments
					set<long> setProcessIDs;
					set<string> setProcessNames;

					getProcessIdsAndNames(string(argv[1]),setProcessIDs, setProcessNames);

					long lRefreshInterval = getRefreshInterval(string(argv[2]));

					// get the directory from the command line
					// allow relative path [p13 #4500]
					string strWorkingDirectory = buildAbsolutePath(string(argv[3]));

					// check if the directory exists and if not, create it
					// as per P13 #4499
					if (!isFileOrFolderValid(strWorkingDirectory))
					{
						// this will throw an exception if it cannot create the directory
						createDirectory(strWorkingDirectory);
					}
					else
					{
						// only need to check for csv files if the directory already existed
						checkWorkingDirectory(strWorkingDirectory);
					}

					// initialize COM
					HRESULT hr=0;
					if (FAILED (hr = CoInitialize(NULL)))
					{
						UCLIDException ue("ELI16679", "FAILED CoInitialize.");
						ue.addHresult(hr);
						throw ue;
					}

					// load the user arguments into the ProcessInformationLogger
					gapLogProcessInfo.reset(new ProcessInformationLogger(setProcessIDs,
						setProcessNames, lRefreshInterval, strWorkingDirectory));

					cout << "Beginning logging - Ctrl+C to end. " << endl;

					ILogDBInfoPtr ipDbLocksLogger = __nullptr;
					ILogDBInfoPtr ipDbConnectionsLogger = __nullptr;

					// if /d and /s where specified start the logging
					if (!databaseName.empty() && !databaseServer.empty())
					{
						ipDbLocksLogger.CreateInstance(CLSID_LogDBLocks);
						ASSERT_RESOURCE_ALLOCATION("ELI46821", ipDbLocksLogger != __nullptr);
						ipDbLocksLogger->DatabaseServer = databaseServer.c_str();
						ipDbLocksLogger->DatabaseName = databaseName.c_str();
						ipDbLocksLogger->PollingTime = lRefreshInterval;
						ipDbLocksLogger->LogDirectory = strWorkingDirectory.c_str();
						ipDbLocksLogger->LogFileName = "DBLocks.csv";

						ipDbLocksLogger->StartLogging();

						ipDbConnectionsLogger.CreateInstance(CLSID_LogDBConnections);
						ASSERT_RESOURCE_ALLOCATION("ELI48334", ipDbLocksLogger != __nullptr);
						ipDbConnectionsLogger->DatabaseServer = databaseServer.c_str();
						ipDbConnectionsLogger->DatabaseName = databaseName.c_str();
						ipDbConnectionsLogger->PollingTime = lRefreshInterval;
						ipDbConnectionsLogger->LogDirectory = strWorkingDirectory.c_str();
						ipDbConnectionsLogger->LogFileName = "DBConnections.csv";

						ipDbConnectionsLogger->StartLogging();
					}

					// start logging
					// NOTE: this will not return until user presses Ctrl+C
					gapLogProcessInfo->start();

					// reset the unique_ptr to NULL (must do this before call to CoUninitialize)
					gapLogProcessInfo.reset();

					if (ipDbLocksLogger != __nullptr)
					{
						ipDbLocksLogger->StopLogging();
					}

					CoUninitialize();		
				}
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI16774");
		}
		catch (UCLIDException& ue)
		{
			ue.log("", !bDisplayExceptions);
			if(bDisplayExceptions)
			{
				throw;
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16673");

	return EXIT_SUCCESS;
}
//--------------------------------------------------------------------------------------------------
