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

//--------------------------------------------------------------------------------------------------
// Helper methods
//--------------------------------------------------------------------------------------------------
// Method for converting EFilePriority to string
static string getPriorityString(UCLID_FILEPROCESSINGLib::EFilePriority ePriority)
{
	try
	{
		// Static collection to hold the priority strings
		static map<long, string> smapPriorityToString;

		// Mutex and initialized flag to ensure thread safety over collection
		static CMutex mutex;
		static bool bInitialized = false;
		if (!bInitialized)
		{
			CSingleLock lg(&mutex, TRUE);

			// Check initialization again (in case blocked while another thread initialized)
			if (!bInitialized)
			{
				// Ensure the collection is empty
				smapPriorityToString.clear();

				// Get a DB pointer
				UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipDB(CLSID_FileProcessingDB);
				ASSERT_RESOURCE_ALLOCATION("ELI27633", ipDB != __nullptr);

				// Get the priorities
				IVariantVectorPtr ipVecPriorities = ipDB->GetPriorities();
				ASSERT_RESOURCE_ALLOCATION("ELI27634", ipVecPriorities != __nullptr);

				// Add each priority to the map
				long lSize = ipVecPriorities->Size;
				for (long i=0; i < lSize; i++)
				{
					string strTemp = asString(ipVecPriorities->Item[i].bstrVal);
					smapPriorityToString[i+1] = strTemp;
				}

				// Map has been initialized, set initialized to true
				bInitialized = true;
			}
		}

		// Get the priority value
		long lPriority = (ePriority == kPriorityDefault ? glDEFAULT_FILE_PRIORITY : (long) ePriority);

		map<long, string>::iterator it = smapPriorityToString.find(lPriority);
		if (it == smapPriorityToString.end())
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
		// Static collection to hold the priority strings
		static map<string, UCLID_FILEPROCESSINGLib::EFilePriority> smapStringToPriority;

		// Mutex and initialized flag to ensure thread safety over collection
		static CMutex mutex;
		static bool bInitialized = false;
		if (!bInitialized)
		{
			CSingleLock lg(&mutex, TRUE);

			// Check initialization again (in case blocked while another thread initialized)
			if (!bInitialized)
			{
				// Ensure the collection is empty
				smapStringToPriority.clear();

				// Get a DB pointer
				UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipDB(CLSID_FileProcessingDB);
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
					smapStringToPriority[strTemp] = (UCLID_FILEPROCESSINGLib::EFilePriority)(i+1);
				}

				// Map has been initialized, set initialized to true
				bInitialized = true;
			}
		}

		// Make the string upper case (case insensitive search)
		string strTemp = strPriority;
		makeUpperCase(strTemp);
		map<string, UCLID_FILEPROCESSINGLib::EFilePriority>::iterator it =
			smapStringToPriority.find(strTemp);
		if (it == smapStringToPriority.end())
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
		// Static collection to hold the priority strings
		static vector<string> svecPriorities;

		// Mutex and initialized flag to ensure thread safety over collection
		static CMutex mutex;
		static bool bInitialized = false;
		if (!bInitialized)
		{
			CSingleLock lg(&mutex, TRUE);

			// Check initialization again (in case blocked while another thread initialized)
			if (!bInitialized)
			{
				// Ensure the collection is empty
				svecPriorities.clear();

				// Get a DB pointer
				UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr ipDB(CLSID_FileProcessingDB);
				ASSERT_RESOURCE_ALLOCATION("ELI27665", ipDB != __nullptr);

				// Get the priorities
				IVariantVectorPtr ipVecPriorities = ipDB->GetPriorities();
				ASSERT_RESOURCE_ALLOCATION("ELI27666", ipVecPriorities != __nullptr);

				// Add each priority to the map
				long lSize = ipVecPriorities->Size;
				for (long i=0; i < lSize; i++)
				{
					// Add the string to the vector
					svecPriorities.push_back(asString(ipVecPriorities->Item[i].bstrVal));
				}

				// Map has been initialized, set initialized to true
				bInitialized = true;
			}
		}

		// Copy the values to the return vector
		rvecPriorities = svecPriorities;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27667");
}
//--------------------------------------------------------------------------------------------------
