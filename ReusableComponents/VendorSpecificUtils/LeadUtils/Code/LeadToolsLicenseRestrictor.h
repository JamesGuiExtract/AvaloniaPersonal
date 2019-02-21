#pragma once

#include "LeadUtils.h"
#include "MiscLeadUtils.h"
#include <Win32Semaphore.h>
#include <functional>
#include <LicenseMgmt.h>

#include <set>



class LEADUTILS_API LeadToolsLicenseRestrictor
{

public:
	LeadToolsLicenseRestrictor();
	~LeadToolsLicenseRestrictor()
	{
		try
		{
			// This will not be null if this instance has the semaphore
			if (m_upleadtoolsRestrictedSemaphor.get() != nullptr)
			{
				CSingleLock lock(&cs, TRUE);
				m_upleadtoolsRestrictedSemaphor->release();
				m_upleadtoolsRestrictedSemaphor.reset(nullptr);
				threadSet.erase(GetCurrentThreadId());
			}
		}
		CATCH_AND_LOG_ALL_EXCEPTIONS("ELI46676");
	}


private:
	// Name of the Semaphore to restrict the number of calls to leadtools
	static const string strLEADTOOLS_LICENSE_RESTRICTION_SEMAPHORE_NAME;

	static const int nMAX_SERVER_THREADS = 2;
	static const int nMAX_CLIENT_PDF_THREADS = 4;

	static CCriticalSection cs;
	static std::set<DWORD> threadSet;

	std::unique_ptr<Win32Semaphore> m_upleadtoolsRestrictedSemaphor;
};

