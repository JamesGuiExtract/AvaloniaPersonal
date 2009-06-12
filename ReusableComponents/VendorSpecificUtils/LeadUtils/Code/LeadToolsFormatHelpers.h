#include "LeadUtils.h"

#include <UCLIDException.h>
#include <l_bitmap.h>

#include <string>

using namespace std;

//-------------------------------------------------------------------------------------------------
// PROMISE: Returns the Leadtools format code for the specified file format string
LEADUTILS_API L_INT getFormatFromString(string strFormat);
//-------------------------------------------------------------------------------------------------
// PROMISE: Returns a string representing the specified Leadtools format
LEADUTILS_API string getStringFromFormat(L_INT nFormat);
//-------------------------------------------------------------------------------------------------
// PROMISE: Adds debug information about the file format to the specified exception
LEADUTILS_API void addFormatDebugInfo(UCLIDException& rUex, L_INT nFormat);
//-------------------------------------------------------------------------------------------------
