#include "StdAfx.h"
#include "UCLIDCOMUtils.h"
#include "COMUtilsMethods.h"

#include <UCLIDException.h>

//--------------------------------------------------------------------------------------------------
IUnknownPtr cloneObject(string strELI, IUnknownPtr ipObject, bool bWithCloneIdentifiableObject)
{
	// if the object passed in is null just return null
	if (ipObject == __nullptr)
	{
		return __nullptr;
	}

	// Check if cloning using the ICloneIdentifiableObject interface if it is defined 
	if (bWithCloneIdentifiableObject)
	{
		UCLID_COMUTILSLib::ICloneIdentifiableObjectPtr ipCloneIdentifiable(ipObject);
		if (ipCloneIdentifiable != __nullptr)
		{
			return ipCloneIdentifiable->CloneIdentifiableObject();
		}
	}

	// Clone using the ICopyableObject interface
	UCLID_COMUTILSLib::ICopyableObjectPtr ipCopyObj(ipObject);
	ASSERT_RESOURCE_ALLOCATION(strELI, ipCopyObj != __nullptr);

	return ipCopyObj->Clone();
}
//--------------------------------------------------------------------------------------------------

