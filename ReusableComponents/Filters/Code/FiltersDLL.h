#pragma once


#ifdef EXPORT_FILTERS_DLL
#define EXT_FILTERS_DLL __declspec(dllexport)
#else
#define EXT_FILTERS_DLL __declspec(dllimport)
#endif

