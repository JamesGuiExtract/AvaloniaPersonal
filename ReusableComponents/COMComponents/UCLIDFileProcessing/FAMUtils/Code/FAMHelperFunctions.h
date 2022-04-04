// FAMHelperFunctions.h

#pragma once

#include "FAMUtils.h"

// Returns true if any of the objects in the vector require admin access
FAMUTILS_API bool checkForRequiresAdminAccess(IIUnknownVectorPtr ipObjects);
//-------------------------------------------------------------------------------------------------
// Returns true if the object in the ObjectWithDescription requires admin access
FAMUTILS_API bool checkForRequiresAdminAccess(IObjectWithDescriptionPtr ipObject);
// PURPOSE: To fill the given combo box with data from the map the key will be shown and the value
//			will be set ast the item data
FAMUTILS_API void fillComboBoxFromMap(CComboBox& rCombo, IStrToStrMapPtr ipMapData);