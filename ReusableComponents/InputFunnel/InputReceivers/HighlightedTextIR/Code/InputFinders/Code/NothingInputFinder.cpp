// NothingInputFinder.cpp : Implementation of CNothingInputFinder
#include "stdafx.h"
#include "InputFinders.h"
#include "NothingInputFinder.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CNothingInputFinder
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CNothingInputFinder::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IInputFinder,
		&IID_ICategorizedComponent,
		&IID_ILicensedComponent
	};

	for (int i = 0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
		{
			return S_OK;
		}
	}

	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IInputFinder
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CNothingInputFinder::ParseString(BSTR strInput, IIUnknownVector ** ippTokenPositions)
{		
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// Check license state
		validateLicense();

		// Create the vector
		CComPtr<IIUnknownVector> ipUnknownVec;
		HRESULT hr = ipUnknownVec.CoCreateInstance( __uuidof(IUnknownVector) );
		if (FAILED(hr))
		{
			UCLIDException ue( "ELI04892", "Failed to create IUnknownVector" );
			ue.addDebugInfo( "hr", hr );
			throw ue;
		}

		// Provide empty vector to caller
		*ippTokenPositions = ipUnknownVec.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04893")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICategorizedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CNothingInputFinder::raw_GetComponentDescription(BSTR * pbstrComponentDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())	

	try
	{
		// Check license state
		validateLicense();

		// Set description
		*pbstrComponentDescription = CComBSTR( "Nothing" );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI04894")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CNothingInputFinder::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())		
	try
	{
		// Check license state
		validateLicense();

		// If validateLicense doesn't throw any exception, then pbValue is true
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Helper functions
//-------------------------------------------------------------------------------------------------
void CNothingInputFinder::validateLicense()
{
	static const unsigned long NOTHING_INPUT_FINDER_COMPONENT_ID = gnINPUTFUNNEL_CORE_OBJECTS;

	VALIDATE_LICENSE( NOTHING_INPUT_FINDER_COMPONENT_ID, "ELI04891", 
		"Nothing Input Finder" );
}
//-------------------------------------------------------------------------------------------------
