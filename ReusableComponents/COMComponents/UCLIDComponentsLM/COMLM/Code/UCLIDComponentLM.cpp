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

#include <UCLIDException.h>
#include <comdef.h>
#include <COMUtils.h>

using namespace std;

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CUCLIDComponentLM::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IUCLIDComponentLM
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
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03768")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CUCLIDComponentLM::IgnoreLockConstraints(long lKey)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Pass the key to License Management
		LicenseManagement::ignoreLockConstraints( lKey );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03775")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------

STDMETHODIMP CUCLIDComponentLM::IsLicensed(long ulComponentID, VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		*pbValue = LicenseManagement::isLicensed( ulComponentID ) ? VARIANT_TRUE : VARIANT_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10637")

	return S_OK;
}
