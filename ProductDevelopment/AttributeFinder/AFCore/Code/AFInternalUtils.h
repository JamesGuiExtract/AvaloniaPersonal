
#pragma once

#include <string>

class UCLIDException;

//--------------------------------------------------------------------------------------------------
// PURPOSE: Adds the name of the currently executing rule file to the debug info of the specified
//			UCLIDException.
void addCurrentRSDFileToDebugInfo(UCLIDException &ue);
//--------------------------------------------------------------------------------------------------
