#pragma once


#ifdef EXPORT_CLREXCEPTION_DLL
#define EXPORT_ClrException __declspec(dllexport)
#define EXPIMP_TEMPLATE_CLREXCEPTION
#else
#define EXPORT_ClrException __declspec(dllimport)
#define EXPIMP_TEMPLATE_CLREXCEPTION extern
#endif

