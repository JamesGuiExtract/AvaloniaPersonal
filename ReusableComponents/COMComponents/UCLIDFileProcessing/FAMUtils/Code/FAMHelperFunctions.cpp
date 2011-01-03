// FAMHelperFunctions.cpp:  Implementation of helper functions used by object used in the FAM
#include "stdafx.h"
#include "FAMHelperFunctions.h"

//-------------------------------------------------------------------------------------------------
bool checkForRequiresAdminAccess(IIUnknownVectorPtr ipObjects)
{
	// if ipObjects is NULL then it does not require admin access so return VARIANT_FALSE
	if (ipObjects == __nullptr)
	{
		return false;
	}

	bool bReturnValue = false;

	// Check if any of the opjects in the vector require admin access
	int nTaskCount = ipObjects->Size();
	for (int i = 0; !bReturnValue && i < nTaskCount ; i++)
	{
		// Get the current object as ObjectWithDescription
		IObjectWithDescriptionPtr ipOWD(ipObjects->At(i));

		// Check the object for requires admin access
		bReturnValue = checkForRequiresAdminAccess(ipOWD);
	}
	return bReturnValue;
}
//-------------------------------------------------------------------------------------------------
bool checkForRequiresAdminAccess(IObjectWithDescriptionPtr ipObject)
{
	// if the object is not a object with description then the requires admin access is
	// assumed to be false
	if (ipObject != __nullptr && ipObject->Enabled == VARIANT_TRUE)
	{
		// Retrieve the object
		UCLID_COMUTILSLib::IAccessRequiredPtr ipAccess(ipObject->Object);

		// if the IAccessRequired interface is not implemented then requires admin access is 
		// assumed to be false
		if (ipAccess != __nullptr)
		{
			return ipAccess->RequiresAdminAccess() == VARIANT_TRUE;
		}
	}
	return false;
}
//-------------------------------------------------------------------------------------------------