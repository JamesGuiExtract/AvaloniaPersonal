
#pragma once

#ifdef EXPORT_ZLIB_UTILS_DLL

#define EXT_ZLIB_UTILS_DLL __declspec(dllexport)
#define EXPIMP_TEMPLATE_ZLIB_UTILS

#else

#define EXT_ZLIB_UTILS_DLL __declspec(dllimport)
#define EXPIMP_TEMPLATE_ZLIB_UTILS extern

#endif
