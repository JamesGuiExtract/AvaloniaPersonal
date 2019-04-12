#pragma once

#ifdef EXPORT_NUANCEUTILS_DLL
#define NUANCEUTILS_API _declspec(dllexport)
#else
#define NUANCEUTILS_API _declspec(dllimport)
#endif