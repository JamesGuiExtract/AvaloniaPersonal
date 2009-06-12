#include "stdafx.h"
#include "InputTargetFramework.h"
#include "InputTargetManager.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

#include <algorithm>

using namespace std;

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputTargetManager::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IInputTargetManager,
		&IID_ILicensedComponent
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// CInputTargetManager
//-------------------------------------------------------------------------------------------------
CInputTargetManager::CInputTargetManager()
:m_pLastActiveInputTarget(NULL)
{
	// get access to the singleton highlight window
	m_ipHighlightWindow.CreateInstance(CLSID_HighlightWindow);
}
//-------------------------------------------------------------------------------------------------
CInputTargetManager::~CInputTargetManager()
{
	try
	{
		m_vecInputTargets.clear();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20398");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputTargetManager::AddInputTarget(IInputTarget *pInputTarget)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// if the input target is not already in the vector, add it
		vector<IInputTarget*>::iterator itInputTargets = 
			find(m_vecInputTargets.begin(), m_vecInputTargets.end(), pInputTarget);
		if (itInputTargets == m_vecInputTargets.end())
		{
			m_vecInputTargets.push_back(pInputTarget);
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04029")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputTargetManager::NotifyInputTargetWindowActivated(IInputTarget *pInputTarget)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// ensure that the input target that got activated is in our vector of
		// input targets
		getThisAsCOMPtr()->AddInputTarget(
			(UCLID_INPUTTARGETFRAMEWORKLib::IInputTarget*) pInputTarget);

		// deactivate the last active input if applicable
		if (m_pLastActiveInputTarget != NULL && 
			m_pLastActiveInputTarget != pInputTarget)
		{
			m_pLastActiveInputTarget->Deactivate();
		}

		// update the last active input target pointer to be the 
		// currently active input target
		m_pLastActiveInputTarget = pInputTarget;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04030")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputTargetManager::NotifyInputTargetWindowClosed(IInputTarget *pInputTarget)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		// ensure that the input target that got closed is in our vector of
		// input targets
		// NOTE: if pInputTarget == NULL, the understanding is that one or
		// more InputTarget windows *** may *** have been closed
		if (pInputTarget)
		{
			getThisAsCOMPtr()->AddInputTarget(
				(UCLID_INPUTTARGETFRAMEWORKLib::IInputTarget*) pInputTarget);
		}

		// just in case, if no new input target is going to be activated,
		// set the last input target pointer to NULL
		m_pLastActiveInputTarget = NULL;
		
		// activate whoever is the first input target that's inactive but visible
		vector<IInputTarget*>::iterator itInputTargets;
		for (itInputTargets = m_vecInputTargets.begin(); 
			 itInputTargets != m_vecInputTargets.end(); itInputTargets++)
		{
			UCLID_INPUTTARGETFRAMEWORKLib::IInputTargetPtr ipITActive(*itInputTargets);
			VARIANT_BOOL bWindowVisible = ipITActive->IsVisible();
		
			// it must be visible
			if (bWindowVisible == VARIANT_TRUE && 
				*itInputTargets != pInputTarget)
			{
				// activate the window and update the internal pointer to the
				// last active input target window
				ipITActive->Activate();
				m_pLastActiveInputTarget = *itInputTargets;

				m_ipHighlightWindow->Refresh();

				// break out of the loop since we've just had an active window
				break;
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04031")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CInputTargetManager::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();
		// if validateLicense doesn't throw any exception, then pbValue is true
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private method
//-------------------------------------------------------------------------------------------------
UCLID_INPUTTARGETFRAMEWORKLib::IInputTargetManagerPtr CInputTargetManager::getThisAsCOMPtr()
{
	UCLID_INPUTTARGETFRAMEWORKLib::IInputTargetManagerPtr ipThis(this);
	ASSERT_ARGUMENT("ELI17774", ipThis != NULL);

	return ipThis;
}
//-------------------------------------------------------------------------------------------------
void CInputTargetManager::validateLicense()
{
	// Ignore any pre-existing magic key
	static const unsigned long INPUT_TARGET_MGR_COMPONENT_ID = gnINPUTFUNNEL_CORE_OBJECTS;

	VALIDATE_LICENSE( INPUT_TARGET_MGR_COMPONENT_ID, "ELI04028",
		"Input Target Manager" );
}
//-------------------------------------------------------------------------------------------------
