
#pragma once

#include "BaseUtils.h"

#include <afxmt.h>
#include <string>

using namespace std;

// Gets a global named mutex object.  If strMutexName does not start with
// the word "Global\" then "Global\" will be prepended to the mutex name.
// This method will return a mutex that has its security settings set
// such that all users (the "Everyone" SID) have synhchronize and modify
// access to the mutex.
EXPORT_BaseUtils CMutex* getGlobalNamedMutex(string strMutexName);