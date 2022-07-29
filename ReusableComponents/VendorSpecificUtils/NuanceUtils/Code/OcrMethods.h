#pragma once

#include "NuanceUtils.h"

#include <KernelAPI.h>

#include <string>
#include <UCLIDException.h>

using namespace std;

NUANCEUTILS_API void loadPageFromImageHandle(const string& strImage, HIMGFILE hImage, int iPageIndex, HPAGE* phPage);
//---------------------------------------------------------------------------------------------
// PURPOSE: To compute the folder that the current SSOCR2 will be using for temporary files
//			(this is based on the supplied PID)
NUANCEUTILS_API std::string getTemporaryDataFolder(long pid);
//---------------------------------------------------------------------------------------------
// Use the RECERR to determine if this failure is due to a Nuance licensing service problem
NUANCEUTILS_API bool isErrorFromNLSFailure(RECERR rc);
//---------------------------------------------------------------------------------------------
// Use the debug data added by loadScansoftRecErrInfo to determine if this failure is due to a
// Nuance licensing service problem
NUANCEUTILS_API bool isExceptionFromNLSFailure(UCLIDException& ue);