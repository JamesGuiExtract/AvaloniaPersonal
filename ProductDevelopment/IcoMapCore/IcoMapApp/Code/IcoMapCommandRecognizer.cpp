// IcoMapCommandRecognizer.cpp : Implementation of CIcoMapCommandRecognizer
#include "stdafx.h"
#include "IcoMapApp.h"
#include "IcoMapCommandRecognizer.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// CIcoMapCommandRecognizer
//-------------------------------------------------------------------------------------------------

//-------------------------------------------------------------------------------------------------
// ISupportErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapCommandRecognizer::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
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
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CIcoMapCommandRecognizer::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
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
// Private functions
//-------------------------------------------------------------------------------------------------
void CIcoMapCommandRecognizer::validateLicense()
{
	static const unsigned long THIS_ID = gnICOMAP_CORE_OBJECTS;

	VALIDATE_LICENSE(THIS_ID, "ELI07462", "IcoMap Command Recognizer");
}
//-------------------------------------------------------------------------------------------------
