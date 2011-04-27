#include "stdafx.h"
#include "MutexUtils.h"
#include "UCLIDException.h"

#include <string>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Non-exported helper methods
//-------------------------------------------------------------------------------------------------
// This code is based on code from:
//	http://www.unixwiz.net/tools/dbmutex-1.0.1.cpp
// This code was referenced on the following website:
//	http://www.eggheadcafe.com/software/aspnet/33838244/create-a-global-mutex-on.aspx
//
// Gets a named mutex with its security setting such that all users have
// synchronize and modify access to it.
CMutex* getNamedMutex(const string& strMutexName)
{
		// Initialize a security descriptor
		SECURITY_DESCRIPTOR sd;
		InitializeSecurityDescriptor(&sd, SECURITY_DESCRIPTOR_REVISION);

		// Set the DACL to wide open (say DACL is present but pass NULL and turn off
		// setting of default DACL)
		SetSecurityDescriptorDacl(&sd, TRUE, NULL, FALSE);

		// Zero out a security attributes object
		SECURITY_ATTRIBUTES sa;
		ZeroMemory(&sa, sizeof(sa));

		sa.nLength = sizeof(sa);
		sa.lpSecurityDescriptor = &sd;
		sa.bInheritHandle = FALSE;

		return new CMutex(FALSE, strMutexName.c_str(), &sa);
}

//-------------------------------------------------------------------------------------------------
// Exported methods
//-------------------------------------------------------------------------------------------------
CMutex* getGlobalNamedMutex(string strMutexName)
{
	try
	{
		ASSERT_ARGUMENT("ELI29990", !strMutexName.empty());

		// If defined as a local mutex, make it a global mutex
		if (strMutexName.find("Local\\") == 0)
		{
			strMutexName.replace(0,6, "Global\\");
		}
		else if (strMutexName.find("Global\\") != 0)
		{
			strMutexName = "Global\\" + strMutexName;
		}

		return getNamedMutex(strMutexName);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29991");
}
//-------------------------------------------------------------------------------------------------
CMutex* getLocalNamedMutex(string strMutexName)
{
	try
	{
		ASSERT_ARGUMENT("ELI32451", !strMutexName.empty());

		// If defined as a global mutex, make it a local mutex
		if (strMutexName.find("Global\\") == 0)
		{
			strMutexName.replace(0,7, "Local\\");
		}
		else if (strMutexName.find("Local\\") != 0)
		{
			strMutexName = "Local\\" + strMutexName;
		}

		return getNamedMutex(strMutexName);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI32452");
}
//-------------------------------------------------------------------------------------------------