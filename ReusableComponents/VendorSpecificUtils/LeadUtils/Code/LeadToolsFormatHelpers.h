#include "LeadUtils.h"

#include <UCLIDException.h>

#include <string>

using namespace std;

//-------------------------------------------------------------------------------------------------
// PROMISE: Returns the Leadtools format code for the specified file format string
LEADUTILS_API int getFormatFromString(const string& strFormat);
//-------------------------------------------------------------------------------------------------
// PROMISE: Returns a string representing the specified Leadtools format
LEADUTILS_API string getStringFromFormat(int nFormat);
//-------------------------------------------------------------------------------------------------
// PROMISE: Adds debug information about the file format to the specified exception
LEADUTILS_API void addFormatDebugInfo(UCLIDException& rUex, int nFormat);
//-------------------------------------------------------------------------------------------------
