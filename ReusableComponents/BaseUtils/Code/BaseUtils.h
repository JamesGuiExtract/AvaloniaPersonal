#ifndef BASE_UTIL_H
#define BASE_UTIL_H

#ifdef EXPORT_BASEUTILS_DLL
#define EXPORT_BaseUtils __declspec(dllexport)
#define EXPIMP_TEMPLATE_BASEUTILS
#elif NO_EXPORT_IMPORT
#define EXPORT_BaseUtils 
#else
#define EXPORT_BaseUtils __declspec(dllimport)
#define EXPIMP_TEMPLATE_BASEUTILS extern
#endif

#endif