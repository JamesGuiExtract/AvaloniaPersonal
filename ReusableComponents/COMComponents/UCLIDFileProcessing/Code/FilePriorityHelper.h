#pragma once

#include "stdafx.h"
#include "uclidfileprocessing.h"

#include <COMUtils.h>
#include <UCLIDException.h>

#include <string>
#include <map>

using namespace std;

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
// The default file priority (Normal)
const long glDEFAULT_FILE_PRIORITY = (long) kPriorityNormal;


// Namespace-scope statics as substitute for broken function scope static in MSVC 2010

// Function getPriorityString
namespace nsGetPriorityString
{
	// Static collection to hold the priority strings
	static map<long, string> smapPriorityToString;

	// Mutex and initialized flag to ensure thread safety over collection
	static CCriticalSection mutex;
	static bool bInitialized = false;
}

// Function getPriorityFromString
namespace nsGetPriorityFromString
{
	// Static collection to hold the priority strings
	static map<string, UCLID_FILEPROCESSINGLib::EFilePriority> smapStringToPriority;

	// Mutex and initialized flag to ensure thread safety over collection
	static CCriticalSection mutex;
	static bool bInitialized = false;
}

// Function getPrioritiesVector
namespace nsGetPrioritiesVector
{
	// Static collection to hold the priority strings
	static vector<string> svecPriorities;

	// Mutex and initialized flag to ensure thread safety over collection
	static CCriticalSection mutex;
	static bool bInitialized = false;
}

//--------------------------------------------------------------------------------------------------
// Helper methods
//--------------------------------------------------------------------------------------------------
// Method for converting EFilePriority to string
static string getPriorityString(UCLID_FILEPROCESSINGLib::EFilePriority ePriority)
{
	try
	{
		if (!nsGetPriorityString::bInitialized)
		{
			CSingleLock lg(&nsGetPriorityString::mutex, TRUE);

			// Check initialization again (in case blocked while another thread initialized)
			if (!nsGetPriorityString::bInitialized)
			{
				// Ensure the collection is empty
				nsGetPriorityString::smapPriorityToString.clear();

				// Get a DB pointer
				UCLID_FILEPROCESSINGLib::IFAMDBUtilsPtr ipFAMDBUtils(CLSID_FAMDBUtils);	
				ASSERT_RESOURCE_ALLOCATION("ELI34524", ipFAMDBUtils != __nullptr);
	
				UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipDB((LPCSTR)ipFAMDBUtils->GetFAMDBProgId());
				ASSERT_RESOURCE_ALLOCATION("ELI27633", ipDB != __nullptr);

				// Get the priorities
				IVariantVectorPtr ipVecPriorities = ipDB->GetPriorities();
				ASSERT_RESOURCE_ALLOCATION("ELI27634", ipVecPriorities != __nullptr);

				// Add each priority to the map
				long lSize = ipVecPriorities->Size;
				for (long i=0; i < lSize; i++)
				{
					string strTemp = asString(ipVecPriorities->Item[i].bstrVal);
					nsGetPriorityString::smapPriorityToString[i+1] = strTemp;
				}

				// Map has been initialized, set initialized to true
				nsGetPriorityString::bInitialized = true;
			}
		}

		// Get the priority value
		long lPriority = (ePriority == kPriorityDefault ? glDEFAULT_FILE_PRIORITY : (long) ePriority);

		map<long, string>::iterator it = nsGetPriorityString::smapPriorityToString.find(lPriority);
		if (it == nsGetPriorityString::smapPriorityToString.end())
		{
			UCLIDException uex("ELI27635", "Invalid priority.");
			uex.addDebugInfo("Priority", lPriority);
			throw uex;
		}

		return it->second;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27636");
}
//--------------------------------------------------------------------------------------------------
// Method for converting string to EFilePriority
static UCLID_FILEPROCESSINGLib::EFilePriority getPriorityFromString(const string& strPriority)
{
	try
	{
		if (!nsGetPriorityFromString::bInitialized)
		{
			CSingleLock lg(&nsGetPriorityFromString::mutex, TRUE);

			// Check initialization again (in case blocked while another thread initialized)
			if (!nsGetPriorityFromString::bInitialized)
			{
				// Ensure the collection is empty
				nsGetPriorityFromString::smapStringToPriority.clear();

				// Get a DB pointer
				UCLID_FILEPROCESSINGLib::IFAMDBUtilsPtr ipFAMDBUtils(CLSID_FAMDBUtils);
				ASSERT_RESOURCE_ALLOCATION("ELI34525", ipFAMDBUtils != __nullptr);
	
				UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipDB((LPCSTR)ipFAMDBUtils->GetFAMDBProgId());
				ASSERT_RESOURCE_ALLOCATION("ELI27660", ipDB != __nullptr);

				// Get the priorities
				IVariantVectorPtr ipVecPriorities = ipDB->GetPriorities();
				ASSERT_RESOURCE_ALLOCATION("ELI27661", ipVecPriorities != __nullptr);

				// Add each priority to the map
				long lSize = ipVecPriorities->Size;
				for (long i=0; i < lSize; i++)
				{
					// Get the string
					string strTemp = asString(ipVecPriorities->Item[i].bstrVal);

					// Make it upper case
					makeUpperCase(strTemp);

					// Add the priority to the map
					nsGetPriorityFromString::smapStringToPriority[strTemp] = (UCLID_FILEPROCESSINGLib::EFilePriority)(i+1);
				}

				// Map has been initialized, set initialized to true
				nsGetPriorityFromString::bInitialized = true;
			}
		}

		// Make the string upper case (case insensitive search)
		string strTemp = strPriority;
		makeUpperCase(strTemp);
		map<string, UCLID_FILEPROCESSINGLib::EFilePriority>::iterator it =
			nsGetPriorityFromString::smapStringToPriority.find(strTemp);
		if (it == nsGetPriorityFromString::smapStringToPriority.end())
		{
			UCLIDException uex("ELI27662", "Invalid priority string.");
			uex.addDebugInfo("Priority String", strPriority);
			throw uex;
		}

		return it->second;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27663");
}
//--------------------------------------------------------------------------------------------------
// Method for populating a vector with the EFilePriority string values
static void getPrioritiesVector(vector<string>& rvecPriorities)
{
	try
	{
		if (!nsGetPrioritiesVector::bInitialized)
		{
			CSingleLock lg(&nsGetPrioritiesVector::mutex, TRUE);

			// Check initialization again (in case blocked while another thread initialized)
			if (!nsGetPrioritiesVector::bInitialized)
			{
				// Ensure the collection is empty
				nsGetPrioritiesVector::svecPriorities.clear();

				// Get a DB pointer
				UCLID_FILEPROCESSINGLib::IFAMDBUtilsPtr ipFAMDBUtils(CLSID_FAMDBUtils);
				ASSERT_RESOURCE_ALLOCATION("ELI34526", ipFAMDBUtils != __nullptr);
	
				UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipDB((LPCSTR)ipFAMDBUtils->GetFAMDBProgId());
				ASSERT_RESOURCE_ALLOCATION("ELI27665", ipDB != __nullptr);

				// Get the priorities
				IVariantVectorPtr ipVecPriorities = ipDB->GetPriorities();
				ASSERT_RESOURCE_ALLOCATION("ELI27666", ipVecPriorities != __nullptr);

				// Add each priority to the map
				long lSize = ipVecPriorities->Size;
				for (long i=0; i < lSize; i++)
				{
					// Add the string to the vector
					nsGetPrioritiesVector::svecPriorities.push_back(asString(ipVecPriorities->Item[i].bstrVal));
				}

				// Map has been initialized, set initialized to true
				nsGetPrioritiesVector::bInitialized = true;
			}
		}

		// Copy the values to the return vector
		rvecPriorities = nsGetPrioritiesVector::svecPriorities;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27667");
}
//--------------------------------------------------------------------------------------------------
