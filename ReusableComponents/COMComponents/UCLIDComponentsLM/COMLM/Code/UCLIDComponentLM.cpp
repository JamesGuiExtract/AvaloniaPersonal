//-------------------------------------------------------------------------------------------------
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	UCLIDComponentLM.cpp
//
// PURPOSE:	Implementation of the CUCLIDComponentLM class
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//-------------------------------------------------------------------------------------------------

#include "stdafx.h"
#include "COMLM.h"
#include "UCLIDComponentLM.h"
#include "LicenseMgmt.h"

#include <SafeArrayAccessGuard.h>
#include <UCLIDException.h>
#include <comdef.h>
#include <COMUtils.h>
#include <EnvironmentInfo.h>

#include <vector>
#include <atlsafe.h>

using namespace std;

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CUCLIDComponentLM::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IUCLIDComponentLM,
		&__uuidof(ILicenseInfo)
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// CUCLIDComponentLM
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CUCLIDComponentLM::InitializeFromFile(BSTR bstrLicenseFile, 
												   long ulKey1, long ulKey2, 
												   long ulKey3, long ulKey4)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Convert parameter to string type
		std::string	strLicenseFile = asString( bstrLicenseFile );
		
		// Initialize the license management singleton
		// using the User String
		LicenseManagement::initializeLicenseFromFile( 
			strLicenseFile, ulKey1, ulKey2, ulKey3, ulKey4, true );

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03768")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CUCLIDComponentLM::IgnoreLockConstraints(long lKey)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Pass the key to License Management
		LicenseManagement::ignoreLockConstraints( lKey );
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03775")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CUCLIDComponentLM::IsLicensed(long ulComponentID, VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		*pbValue = LicenseManagement::isLicensed( ulComponentID ) ? VARIANT_TRUE : VARIANT_FALSE;
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10637")
}

//-------------------------------------------------------------------------------------------------
// ILicenseInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CUCLIDComponentLM::raw_GetLicensedComponents(SAFEARRAY** psaExpirationDates, SAFEARRAY** psaComponents)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		vector<pair<unsigned long, CTime>> vecComponents = LicenseManagement::getLicensedComponents();

		CComSafeArray<unsigned long> saComponents(vecComponents.size(), 0);
		CComSafeArray<DATE> saExpirationDates(vecComponents.size(), 0);

		long nCount = (long)vecComponents.size();
		for (long i = 0; i < nCount; i++)
		{
			unsigned long ulComponent = vecComponents[i].first;
			saComponents.SetAt(i, ulComponent);

			if (psaExpirationDates != __nullptr)
			{
				COleDateTime dateExpiration(vecComponents[i].second.GetTime());
				saExpirationDates.SetAt(i, dateExpiration);
			}
		}

		*psaComponents = saComponents.Detach();
		*psaExpirationDates = saExpirationDates.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI49764")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CUCLIDComponentLM::raw_GetLicensedPackageNames(SAFEARRAY** psaExpirationDates, SAFEARRAY** psaPackageNames)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		vector<pair<string, CTime>> vecPackageNames = LicenseManagement::getLicensedPackageNames();

		CComSafeArray<BSTR> saPackageNames(vecPackageNames.size(), 0);
		CComSafeArray<DATE> saExpirationDates(vecPackageNames.size(), 0);

		long nCount = (long)vecPackageNames.size();
		for (long i = 0; i < nCount; i++)
		{
			saPackageNames.SetAt(i, _bstr_t(vecPackageNames[i].first.c_str()));

			if (psaExpirationDates != __nullptr)
			{
				if (vecPackageNames[i].second == 0)
				{
					saExpirationDates.SetAt(i, 0);
				}
				else
				{
					COleDateTime dateExpiration(vecPackageNames[i].second.GetTime());
					saExpirationDates.SetAt(i, dateExpiration);
				}
			}
		}
		
		*psaPackageNames = saPackageNames.Detach();
		*psaExpirationDates = saExpirationDates.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI49765")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CUCLIDComponentLM::raw_GetLicensedPackageNamesWithExpiration(SAFEARRAY** psaPackageNames)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		EnvironmentInfo envInfo;
		vector<string> vecPackageNames = envInfo.GetLicensedPackages(true);

		CComSafeArray<BSTR> saPackageNames(vecPackageNames.size(), 0);
		for (size_t i = 0; i < vecPackageNames.size(); i++)
		{
			saPackageNames.SetAt(i, _bstr_t(vecPackageNames[i].c_str()));
		}

		*psaPackageNames = saPackageNames.Detach();
		
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI49766")
}
