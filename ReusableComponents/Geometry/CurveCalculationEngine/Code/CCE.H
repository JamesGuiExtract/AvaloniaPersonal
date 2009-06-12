#ifndef CCE_H
#define CCE_H

#ifdef EXPORT_CCE_DLL
#define EXP_CCE_DLL __declspec(dllexport)
#else
#define EXP_CCE_DLL __declspec(dllimport)
#endif

#endif
