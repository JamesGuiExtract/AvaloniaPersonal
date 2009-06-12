#ifndef COMLMCORE_DLL_H
#define COMLMCORE_DLL_H

#ifdef EXPORT_LM_DLL
#define EXPORT_LM __declspec(dllexport)
#else
#define EXPORT_LM __declspec(dllimport)
#endif


#endif
