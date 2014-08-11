#pragma once

#include "BaseUtils.h"
#include "UCLIDException.h"

#include <ComDef.h>

// This class manages the locking of the memory of the SAFEARRAY so that it can be
// used to copy data to and from the SAFEARRAY in bulk
template <class T> class SafeArrayAccessGuard
{
private:
	SAFEARRAY *m_psa;
	bool m_bIsLocked;
	T *m_pData;

public:

	SafeArrayAccessGuard(SAFEARRAY *psa): m_psa(psa), m_bIsLocked(false)
	{
		try
		{
			ASSERT_ARGUMENT("ELI37194", psa != __nullptr);
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI37195");
	};

	// Calls UnaccessData so the SAFEARRAY will be unlocked if it is locked
	~SafeArrayAccessGuard()
	{
		try
		{
			UnaccessData();
		}
		CATCH_AND_LOG_ALL_EXCEPTIONS("ELI37196");
	}
	
	// PURPOSE: To return a pointer to the data in the SAFEARRAY
	T * AccessData()
	{
		try
		{
			// if it is locked return the data pointer
			if (m_bIsLocked)
			{
				return m_pData;
			}

			// Lock the memory and get the pointer to the data
			HRESULT hr = SafeArrayAccessData(m_psa, (void **) &m_pData);
			if (SUCCEEDED(hr))
			{
				// Mark as locked and return the pointer to the data
				m_bIsLocked = true;
				return m_pData;
			}
			UCLIDException ue("ELI37199", "Unable to access data in SAFEARRAY.");
			ue.addHresult(hr);
			throw ue;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI37197");
	}
	
	// PURPOSE: To unaccess the data in the SAFEARRAY
	// NOTE:	This invalidates the pointer returned by AccessData
	void UnaccessData()
	{
		try
		{
			// if the data is locked unlock it
			if (m_bIsLocked)
			{
				HRESULT hr = SafeArrayUnaccessData(m_psa);
				if (FAILED(hr))
				{
					UCLIDException ue("ELI37200", "Failed to unaccess data in SAFEARRAY.");
					ue.addHresult(hr);
					throw ue;
				}
				m_pData = __nullptr;
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI37198");
	}
};

