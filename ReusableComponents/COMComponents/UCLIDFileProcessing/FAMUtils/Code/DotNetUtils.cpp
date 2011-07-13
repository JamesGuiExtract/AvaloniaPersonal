#include "stdafx.h"
#include "DotNetUtils.h"

#include <UCLIDException.h>
#include <TemporaryFileName.h>
#include <Misc.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
static const string gstrSQLSERVER_INFO_EXE = "SQLServerInfo.exe";

//-------------------------------------------------------------------------------------------------
// Externals
//-------------------------------------------------------------------------------------------------
extern HINSTANCE gFAMUtilsModuleResource;

//-------------------------------------------------------------------------------------------------
// Local Functions
//-------------------------------------------------------------------------------------------------
// Gets a temporary filename and appends it to strArgs argument to pass to the runExtractEXE call
// to SQLServerInfo then gets the list out of the temporary file
void getList(vector<string>& rvecList, const string& strArgs)
{
	// Create a temporary file name to hold the server list
	TemporaryFileName tfn( true, "", ".txt", true );

	// build path to SQLServerInfo.exe relative to FAMUtils
	string strPathToEXE = getModuleDirectory(gFAMUtilsModuleResource);
	strPathToEXE += "\\" + gstrSQLSERVER_INFO_EXE;

	// Run the SQLServerInfo app to save the server list in the temp file
	runExtractEXE(strPathToEXE, strArgs + "\"" + tfn.getName() + "\"", INFINITE);

	// Clear the vector
	rvecList.clear();

	// Get the list of servers from the temp file
	rvecList = convertFileToLines(tfn.getName());
}

//-------------------------------------------------------------------------------------------------
// Public Functions
//-------------------------------------------------------------------------------------------------
void getServerList(vector<string>& rvecServers)
{
	try
	{
		// Get the list
		getList(rvecServers, "/s ");
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI17615");
}
//-------------------------------------------------------------------------------------------------
void getDBNameList(const string& strServer, vector<string>&rvecDBNames)
{
	try
	{
		// Create the argument string
		string strArgs = "/d " + strServer +  " ";

		// Get the list
		getList(rvecDBNames, strArgs);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI17617");
}
//-------------------------------------------------------------------------------------------------
