// FAMHelperFunctions.cpp:  Implementation of helper functions used by object used in the FAM
#include "stdafx.h"
#include "FAMHelperFunctions.h"
#include <UCLIDException.h>
#include <COMUtils.h>

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
void fillComboBoxFromMap(CComboBox& rCombo, IStrToStrMapPtr ipMapData)
{
	try
	{
		long lSize = ipMapData->Size;
		for (long i = 0; i < lSize; i++)
		{
			// Get the name and ID of the action
			_bstr_t bstrKey, bstrValue;
			ipMapData->GetKeyValue(i, bstrKey.GetAddress(), bstrValue.GetAddress());
			string strAction = asString(bstrKey);
			DWORD nID = asUnsignedLong(asString(bstrValue));

			// Insert this action name into the combo box
			int iIndexActionUnderCondition = rCombo.InsertString(-1, strAction.c_str());

			// Set the index of the item inside the combo box same as the ID of the action
			rCombo.SetItemData(iIndexActionUnderCondition, nID);

			// Select the first item in the combo box
			rCombo.SetCurSel(0);
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI53355");
}