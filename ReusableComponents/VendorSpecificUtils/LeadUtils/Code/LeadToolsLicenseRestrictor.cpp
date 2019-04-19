#include "stdafx.h"
#include "LeadToolsLicenseRestrictor.h"

#include <ComponentLicenseIDs.h>

#include <set>

// Name of the Semaphore to restrict the number of calls to leadtools
const string LeadToolsLicenseRestrictor::strLEADTOOLS_LICENSE_RESTRICTION_SEMAPHORE_NAME = "3AACED70-45B2-4288-967C-1B24540856D9";

CCriticalSection LeadToolsLicenseRestrictor::cs;
std::set<DWORD> LeadToolsLicenseRestrictor::threadSet;

LeadToolsLicenseRestrictor::LeadToolsLicenseRestrictor()
{
	try
	{
		if (!LicenseManagement::isLicensed(gnLEADTOOLS_ALL_CORES))
		{
			static bool clientLicense = !(LicenseManagement::isLicensed(gnFLEXINDEX_SERVER_OBJECTS) ||
				LicenseManagement::isLicensed(gnFLEXINDEX_IDSHIELD_SERVER_CORE)) || LicenseManagement::isPDFLicensed();
			static int threads = (clientLicense) ? nMAX_CLIENT_PDF_THREADS : nMAX_SERVER_THREADS;

			DWORD threadID = GetCurrentThreadId();
			{
				CSingleLock lock(&cs, TRUE);
				if (threadSet.find(threadID) != threadSet.end())
				{
					// This thread already has the semaphore
					return;
				}
			}

			// Check for extra licensed cores
			if (LicenseManagement::isLicensed(gnLEADTOOLS_2_EXTRA_CORES)) 
			{
				CSingleLock lock(&cs, TRUE);
				threads += 2;
			}

			if (LicenseManagement::isLicensed(gnLEADTOOLS_4_EXTRA_CORES))
			{
				CSingleLock lock(&cs, TRUE);
				threads += 4;
			}

			m_upleadtoolsRestrictedSemaphor.reset(new Win32Semaphore(threads, threads, strLEADTOOLS_LICENSE_RESTRICTION_SEMAPHORE_NAME));
			m_upleadtoolsRestrictedSemaphor->acquire();

			CSingleLock lock(&cs, TRUE);
			threadSet.emplace(threadID);
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI46675");
}

