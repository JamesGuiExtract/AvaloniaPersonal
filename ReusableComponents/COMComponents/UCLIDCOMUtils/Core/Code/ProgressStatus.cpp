// ProgressStatus.cpp : Implementation of CProgressStatus

#include "stdafx.h"
#include "ProgressStatus.h"

#include <UCLIDException.h>
#include <COMUtils.h>

//--------------------------------------------------------------------------------------------------
// CProgressStatus
//--------------------------------------------------------------------------------------------------
CProgressStatus::CProgressStatus()
:m_ipSubProgressStatus(NULL)
{
	try
	{
		// immediately after construction, we want the object to be in the reset state
		m_nNumItemsCompleted = 0;
		m_nNumItemsTotal = 1;
		m_nNumItemsInCurrentGroup = 0;
		m_strText = "";
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI15976")
}
//--------------------------------------------------------------------------------------------------
CProgressStatus::~CProgressStatus()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16516");
}
//--------------------------------------------------------------------------------------------------
HRESULT CProgressStatus::FinalConstruct()
{
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
void CProgressStatus::FinalRelease()
{
}

//--------------------------------------------------------------------------------------------------
// ISupportsErrorInfo interface
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CProgressStatus::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IProgressStatus
	};

	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//--------------------------------------------------------------------------------------------------
// IProgressStatus interface
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CProgressStatus::InitProgressStatus(/*[in]*/ BSTR strText, 
	/*[in]*/ long nNumItemsCompleted, /*[in]*/ long nNumItemsTotal,
	/*[in]*/ VARIANT_BOOL bCreateOrResetSubProgressStatus)
{
	try
	{
		CSingleLock lock(&m_objLock, TRUE);

		// Update the text
		m_strText = asString(strText);

		// Update the counts
		m_nNumItemsCompleted = nNumItemsCompleted;
		m_nNumItemsTotal = nNumItemsTotal;
		m_nNumItemsInCurrentGroup = 0;

		validateItemCounts();

		// If the caller wants to ensure that a default sub progress status exists,
		// create the sub progress status object, or reset it if it already exists.
		if (bCreateOrResetSubProgressStatus == VARIANT_TRUE)
		{
			if (m_ipSubProgressStatus == __nullptr)
			{
				// The sub progress status object does not exist.  So, create it.
				m_ipSubProgressStatus.CreateInstance(CLSID_ProgressStatus);
				ASSERT_RESOURCE_ALLOCATION("ELI15973", m_ipSubProgressStatus != __nullptr);
			}
			else
			{
				// The sub progress status object already exists.  Just reset it.
				getThisAsCOMPtr()->ResetSubProgressStatus();
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15962")
	
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CProgressStatus::StartNextItemGroup(/*[in]*/ BSTR strNextGroupText, 
												 /*[in]*/ long nNumItemsInNextGroup)
{
	try
	{
		CSingleLock lock(&m_objLock, TRUE);

		UCLID_COMUTILSLib::IProgressStatusPtr ipThis = getThisAsCOMPtr();

		// Complete the current group, if any
		ipThis->CompleteCurrentItemGroup();

		// Store the number of items in the next group
		m_nNumItemsInCurrentGroup = nNumItemsInNextGroup;

		// Update the text
		m_strText = asString(strNextGroupText);

		// Reset the sub progress status object if it exists
		ipThis->ResetSubProgressStatus();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15967")
	
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CProgressStatus::CompleteCurrentItemGroup()
{
	try
	{
		CSingleLock lock(&m_objLock, TRUE);

		// If there is a previous group of items that were started earlier, automatically complete it
		if (m_nNumItemsInCurrentGroup > 0)
		{
			m_nNumItemsCompleted += m_nNumItemsInCurrentGroup;
			m_nNumItemsInCurrentGroup = 0;

			validateItemCounts();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16100")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CProgressStatus::get_Text(/*[out, retval]*/ BSTR *pVal)
{
	try
	{
		CSingleLock lockGuard(&m_objLock, TRUE);

		// convert the STL string to a BSTR and return to the caller
		_bstr_t	bstrText(m_strText.c_str());
		*pVal = bstrText.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15954")

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CProgressStatus::get_NumItemsTotal(/*[out, retval]*/ long *pVal)
{
	try
	{
		CSingleLock lockGuard(&m_objLock, TRUE);

		// return the current value of NumItemsTotal
		*pVal = m_nNumItemsTotal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15956")
	
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CProgressStatus::get_NumItemsCompleted(/*[out, retval]*/ long *pVal)
{
	try
	{
		CSingleLock lockGuard(&m_objLock, TRUE);

		// return the current value of NumItemsCompleted
		*pVal = m_nNumItemsCompleted;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15958")
	
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CProgressStatus::get_NumItemsInCurrentGroup(/*[out, retval]*/ long *pVal)
{
	try
	{
		CSingleLock lockGuard(&m_objLock, TRUE);

		// return the current value of NumItemsCompleted
		*pVal = m_nNumItemsInCurrentGroup;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16101")
	
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CProgressStatus::get_SubProgressStatus(/*[out, retval]*/ IProgressStatus* *pVal)
{
	try
	{
		// whatever the SubProgress object pointer is (regardless of null or not), return
		// it to the caller
		if (m_ipSubProgressStatus != __nullptr)
		{
			// return a shallow copy of the smart pointer
			CComQIPtr<IProgressStatus> ipShallowCopy = m_ipSubProgressStatus;
			*pVal = ipShallowCopy.Detach();
		}
		else
		{
			*pVal = NULL;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15960")
	
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CProgressStatus::put_SubProgressStatus(/*[in]*/ IProgressStatus* newVal)
{
	try
	{
		// the outer scope is allowed to set the SubProgressStatus to null, or to a valid object
		// reference
		m_ipSubProgressStatus = newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15961")
	
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CProgressStatus::GetProgressPercent(/*[out, retval]*/ double *pVal)
{
	try
	{
		CSingleLock lockGuard(&m_objLock, TRUE);

		if (m_nNumItemsTotal == 0)
		{
			// [P13:4807] m_nNumItemsTotal == 0 is a valid value.  But don't try to divide by zero; 
			// return zero to handle the situation gracefully.
			*pVal = 0.0;
			return S_OK;
		}

		// since the number of total items is in the denominator, we want to make it a double type
		// (so that integer division is not performed).  Another benefit of copying the value at
		// this point is that we can guarantee that the same # of items is used in the two 
		// different calculations below (since another thread could have modified m_nNumItemsTotal
		// between the first and second calculations)
		double dNumItemsTotal = m_nNumItemsTotal;
		double dProgressPercent = m_nNumItemsCompleted / dNumItemsTotal;
		
		// if there is a SubProgressStatus, then add on the progress represented by the sub progress
		if (m_ipSubProgressStatus != __nullptr)
		{
			dProgressPercent += m_ipSubProgressStatus->GetProgressPercent() * 
				m_nNumItemsInCurrentGroup / dNumItemsTotal;
		}

		// Before returning, perform a sanity check to make sure the result is in the 0 - 100 range
		if (dProgressPercent < -0.1 || dProgressPercent > 100.1)
		{
#ifdef _DEBUG
			UCLIDException ue("ELI20268", "Internal error: GetProgressPercent return value out-of-range!");
			ue.addDebugInfo("Percent Value", asString(dProgressPercent));
			throw ue;
#endif
			if (dProgressPercent < 0.0)
			{
				dProgressPercent = 0.0;
			}
			else
			{
				dProgressPercent = 100.0;
			}
		}

		// return the calculated progress status
		*pVal = dProgressPercent;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15963")
	
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CProgressStatus::ResetSubProgressStatus()
{
	try
	{
		// if the sub progress status object pointer is not null, then reset its counts, and also
		// ask it to reset all its child progress status objects that exist
		if (m_ipSubProgressStatus != __nullptr)
		{
			// reset the main members of the sub progress status object
			// passing in VARIANT_FALSE as the last argument ensures that a sub progress status
			// object is not created when it doesn't already exist
			m_ipSubProgressStatus->InitProgressStatus("", 0, 1, VARIANT_FALSE);

			// if a sub-progress status object exists, we want that to be reset too
			m_ipSubProgressStatus->ResetSubProgressStatus();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI15964")
	
	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CProgressStatus::CompleteProgressItems(/*[in]*/ BSTR strNextGroupText, 
													/*[in]*/ long nNumItemsCompleted)
{
	try
	{
		CSingleLock lock(&m_objLock, TRUE);

		// Update the number of items completed
		m_nNumItemsCompleted += nNumItemsCompleted;
		
		// Update the text
		m_strText = asString(strNextGroupText);

		// Reset the number of items in the current group
		m_nNumItemsInCurrentGroup = 0;

		validateItemCounts();

		// Reset the sub progress status object if it exists
		getThisAsCOMPtr()->ResetSubProgressStatus();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16211")
	
	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// Private helper functions
//--------------------------------------------------------------------------------------------------
void CProgressStatus::validateItemCounts()
{
	if (m_nNumItemsCompleted > m_nNumItemsTotal)
	{
		// Relating to the fix for [P13:4807], m_nNumItemsTotal == 0 is legal, but  
		// m_nNumItemsCompleted should not be > m_nNumItemsTotal.  If it is, 
		// throw an exception in debug mode, or set m_nNumItemsTotal = 
		// m_nNumItemsCompleted in release.
#ifdef _DEBUG
		UCLIDException ue("ELI20256", "Internal error: m_nNumItemsTotal < m_nNumItemsCompleted!");
		ue.addDebugInfo("m_nNumItemsTotal", asString(m_nNumItemsTotal));
		ue.addDebugInfo("m_nNumItemsCompleted", asString(m_nNumItemsCompleted));
		throw ue;
#endif
		m_nNumItemsTotal = m_nNumItemsCompleted;
	}
}
//--------------------------------------------------------------------------------------------------
UCLID_COMUTILSLib::IProgressStatusPtr CProgressStatus::getThisAsCOMPtr()
{
	UCLID_COMUTILSLib::IProgressStatusPtr ipThis(this);
	ASSERT_RESOURCE_ALLOCATION("ELI16974", ipThis != __nullptr);

	return ipThis;
}
//--------------------------------------------------------------------------------------------------
