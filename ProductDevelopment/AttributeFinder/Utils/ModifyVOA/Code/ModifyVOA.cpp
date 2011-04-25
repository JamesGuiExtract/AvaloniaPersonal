//==================================================================================================
//
// COPYRIGHT (c) 2008 EXTRACT SYSTEMS, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ModifyVOA.cpp
//
// PURPOSE:	An MFC application that will modify the contents of a VOA file based upon
//			command line specified values. [p16 #2899]
//
// AUTHORS:	Jeff Shergalis
//
//=================================================================================================

#include "stdafx.h"
#include "ModifyVOA.h"

#include <UCLIDException.h>
#include <UCLIDExceptionDlg.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <ComponentLicenseIDs.h>

#include <string>
#include <vector>

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//--------------------------------------------------------------------------------------------------
// CModifyVOAApp
//--------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CModifyVOAApp, CWinApp)
	ON_COMMAND(ID_HELP, &CWinApp::OnHelp)
END_MESSAGE_MAP()

//--------------------------------------------------------------------------------------------------
// CModifyVOAApp construction
//--------------------------------------------------------------------------------------------------
CModifyVOAApp::CModifyVOAApp()
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}

//--------------------------------------------------------------------------------------------------
// The one and only CModifyVOAApp object
//--------------------------------------------------------------------------------------------------
CModifyVOAApp theApp;

//--------------------------------------------------------------------------------------------------
// Helper functions
//--------------------------------------------------------------------------------------------------
// PURPOSE: To display the proper program usage to the user
void displayUsage()
{
	string strUsage = "ModifyVOA.exe <inputFile|inputDirectory> [/p <NewPath>] [/ef <logfile>]";
	strUsage += "[/?]\n";
	strUsage += "Usage:\n";
	strUsage += "--------\n";
	strUsage += "<inputFile|inputDirectory>: either the name of a .voa file to modify\n";
	strUsage += "\tor a directory of voa files to modify.\n";
	strUsage += "\tNOTE:If directory is specified all voa files in directory will be modified.\n";
	strUsage += "[/p <NewPath>]: if specified will modify the source image path for all \n";
	strUsage += "\tattributes in the voa file to be the supplied NewPath.\n";
	strUsage += "[/ef <logfile>]: if specified will log all exceptions to the specified\n";
	strUsage += "\tlogfile, otherwise all exceptions will be displayed.\n";
	strUsage += "[/?]: if specified will display this help message.";

	MessageBox(NULL, strUsage.c_str(), "ModifyVOA Usage", MB_OK | MB_ICONINFORMATION);
}
//--------------------------------------------------------------------------------------------------
// PURPOSE: To process the input file/folder argument and fill the vector with the appropriate
//			files to modify
void processInputFileOrFolder(string& rstrInput, vector<string>& rvecInputFiles)
{
	// get the absolute path from the input
	rstrInput = buildAbsolutePath(rstrInput);

	// check if the input is a valid folder, in which case get all .voa files 
	// from the specified folder
	if (isValidFolder(rstrInput))
	{
		getFilesInDir(rvecInputFiles, rstrInput, "*.voa");
	}
	// check if it is a valid file
	else if (isValidFile(rstrInput))
	{
		rvecInputFiles.push_back(rstrInput);
	}
	// it is neither a valid folder or file so throw an exception
	else
	{
		UCLIDException ue("ELI20352", "Cannot find the specified file or folder!");
		ue.addDebugInfo("File/Folder specified", rstrInput);
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
// PURPOSE: To process the path to change argument
void processPathToChange(string& rstrPathToChange)
{
	ASSERT_ARGUMENT("ELI20357", !rstrPathToChange.empty());

	// get the absolute path for the path to change
	rstrPathToChange = buildAbsolutePath(rstrPathToChange);

	// require valid path
	if (!isValidFolder(rstrPathToChange))
	{
		UCLIDException ue("ELI20353", "New specified path must be a valid path!");
		ue.addDebugInfo("Specified path", rstrPathToChange);
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
// PURPOSE: To process the exception log file argument
void processExceptionLogFile(string& rstrExceptionLogFile)
{
	if (!rstrExceptionLogFile.empty())
	{
		rstrExceptionLogFile = buildAbsolutePath(rstrExceptionLogFile);
	}
}
//--------------------------------------------------------------------------------------------------
// PURPOSE: To ensure this application is licensed
void validateLicense()
{
	VALIDATE_LICENSE(gnEXTRACT_CORE_OBJECTS, "ELI20347", "ModifyVOA Application" );
}
//--------------------------------------------------------------------------------------------------
// PURPOSE: To iterate through all VOA files in the vector and for each VOA file to
//			iterate through all of the attributes and to modify the current path
//			in the attribute to be the new path and to then save the modified
//			voa file.
// NOTE:	This will overwrite the VOA file.
void changeAttributePath(const string& strNewPath, const vector<string>& vecVOAFiles)
{
	ASSERT_ARGUMENT("ELI20348", !strNewPath.empty());

	INIT_EXCEPTION_AND_TRACING("MLI00040");

	try
	{
		// make strPath a non const copy of strNewPath, and make sure it ends in a slash
		string strPath = strNewPath;
		if (strPath[strPath.length() - 1] != '\\' || strPath[strPath.length() - 1] != '/')
		{
			strPath += '\\';
		}
		_lastCodePos = "10";

		// iterate through all of the VOA files
		for (vector<string>::const_iterator it = vecVOAFiles.begin(); it != vecVOAFiles.end(); it++)
		{
			// create an IUnknownVector
			IIUnknownVectorPtr ipAttributes(CLSID_IUnknownVector);
			ASSERT_RESOURCE_ALLOCATION("ELI20349", ipAttributes != __nullptr);
			_lastCodePos = "20";

			// get the VOA file name
			_bstr_t bstrVOAFileName = (*it).c_str();
			_lastCodePos = "30";

			// load the VOA file into the new IUnknownVector
			ipAttributes->LoadFrom(bstrVOAFileName, VARIANT_FALSE);
			_lastCodePos = "40";

			// get the vector size
			long lSize = ipAttributes->Size();
			_lastCodePos = "50";

			// iterate through each item in the vector
			for (long i=0; i < lSize; i++)
			{
				// get the attribute
				IAttributePtr ipAttribute = ipAttributes->At(i);
				ASSERT_RESOURCE_ALLOCATION("ELI20350", ipAttribute != __nullptr);
				_lastCodePos = "60";

				// get the spatial string for the attribute
				ISpatialStringPtr ipValue = ipAttribute->Value;
				ASSERT_RESOURCE_ALLOCATION("ELI20351", ipValue != __nullptr);
				_lastCodePos = "70";

				// get the underlyinh SourceDocName
				string strNewFileName = asString(ipValue->SourceDocName);
				_lastCodePos = "80";

				// check if the original source doc name was empty, if not
				// change the path to the new path and store it back in the
				// spatial string
				if (!strNewFileName.empty())
				{
					// change the path
					strNewFileName = strPath + getFileNameFromFullPath(strNewFileName);
					_lastCodePos = "90";

					// store it back
					ipValue->SourceDocName = strNewFileName.c_str();
					_lastCodePos = "100";
				}
			}

			// save the modified IUnknownVector back to the VOA file
			ipAttributes->SaveTo(bstrVOAFileName, VARIANT_TRUE);
			_lastCodePos = "110";
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20363");
}

//--------------------------------------------------------------------------------------------------
// CModifyVOAApp initialization
//--------------------------------------------------------------------------------------------------
BOOL CModifyVOAApp::InitInstance()
{
	// declare the exception log file here so we can check it inside the try catch blocks
	string strExceptionLogFile = "";

	CoInitializeEx(NULL, COINIT_APARTMENTTHREADED);

	try
	{
		try
		{
			// InitCommonControlsEx() is required on Windows XP if an application
			// manifest specifies use of ComCtl32.dll version 6 or later to enable
			// visual styles.  Otherwise, any window creation will fail.
			INITCOMMONCONTROLSEX InitCtrls;
			InitCtrls.dwSize = sizeof(InitCtrls);
			// Set this to include all the common control classes you want to use
			// in your application.
			InitCtrls.dwICC = ICC_WIN95_CLASSES;
			InitCommonControlsEx(&InitCtrls);

			CWinApp::InitInstance();

			// Set up the exception handling aspect.
			static UCLIDExceptionDlg exceptionDlg;
			UCLIDException::setExceptionHandler( &exceptionDlg );			

			// init license
			LicenseManagement::loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);

			// Check license
			validateLicense();

			// check command line arguments
			if (__argc == 2 || __argc < 2 || __argc > 7)
			{
				displayUsage();
				return FALSE;
			}

			// get the input file/directory 
			string strInput(__argv[1]);

			if (strInput == "/?")
			{
				displayUsage();
				return FALSE;
			}

			string strPathToChange = "";

			// get the remaining command line arguments
			for (int i=2; i < __argc; i++)
			{
				string strArg(__argv[i]);

				// make the argument lower case
				makeLowerCase(strArg);

				// if /? then display the usage and exit
				if (strArg == "/?")
				{
					displayUsage();
					return FALSE;
				}

				// if /p then look for path specified as next argument, if there
				// is no next argument then display usage
				else if (strArg == "/p")
				{
					i++;
					if (i < __argc)
					{
						strPathToChange = __argv[i];
					}
					else
					{
						displayUsage();
						return FALSE;
					}
				}

				// if /ef look for exception log file specified as next argument, if
				// there is no next argument then display usage
				else if (strArg == "/ef")
				{
					i++;
					if (i < __argc)
					{
						// if an exception log file is specified, process it first
						// so that any exceptions that are thrown after this point will
						// be logged and not displayed
						strExceptionLogFile = __argv[i];
						processExceptionLogFile(strExceptionLogFile);
					}
					else
					{
						displayUsage();
						return FALSE;
					}
				}
				else
				{
					displayUsage();
					return FALSE;
				}
			}

			// vector of files to be modified
			vector<string> vecInputFiles;

			// process the input file (will fill the vector with the files to be modified)
			processInputFileOrFolder(strInput, vecInputFiles);

			// process the path to change argument
			processPathToChange(strPathToChange);

			// if the path to change is not empty then modify the attributes with the new path
			if (!strPathToChange.empty())
			{
				changeAttributePath(strPathToChange, vecInputFiles);
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20355");
	}
	catch(UCLIDException& ue)
	{
		// check for exception log file specified
		if (!strExceptionLogFile.empty())
		{
			// log the exception to the specified file
			ue.log(strExceptionLogFile);
		}
		else
		{
			// no log file specified so display the exception
			ue.display();
		}
	}

	CoUninitialize();

	// Since the dialog has been closed, return FALSE so that we exit the
	//  application, rather than start the application's message pump.
	return FALSE;
}
//--------------------------------------------------------------------------------------------------