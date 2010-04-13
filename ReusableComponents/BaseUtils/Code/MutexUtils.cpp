#include "stdafx.h"
#include "MutexUtils.h"
#include "UCLIDException.h"

#include <string>

using namespace std;

// This code is based on code from:
//	http://www.unixwiz.net/tools/dbmutex-1.0.1.cpp
// This code was referenced on the following website:
//	http://www.eggheadcafe.com/software/aspnet/33838244/create-a-global-mutex-on.aspx
//
// This method will create a global named mutex and set the security such that all
// users will have access to the mutex.
CMutex* getGlobalNamedMutex(string strMutexName)
{
	try
	{
		ASSERT_ARGUMENT("ELI29990", !strMutexName.empty());

		if (strMutexName.find("Global\\") != 0)
		{
			strMutexName = "Global\\" + strMutexName;
		}

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
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29991");
}