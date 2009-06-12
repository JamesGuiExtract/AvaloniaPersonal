#pragma once

#ifdef EXPORT_FILTERS_CORE_DLL
#define EXPORT_FILTERS_CORE __declspec(dllexport)
#else
#define EXPORT_FILTERS_CORE __declspec(dllimport)
#endif
