#pragma once

#include "BaseUtils.h"

//-------------------------------------------------------------------------------------------------
// Wraps the PROCESS_INFORMATION structure used by runEXE() in its call to CreateProcess().
class EXPORT_BaseUtils ProcessInformationWrapper
{
public:
	ProcessInformationWrapper();
	~ProcessInformationWrapper();

	PROCESS_INFORMATION pi;
};
