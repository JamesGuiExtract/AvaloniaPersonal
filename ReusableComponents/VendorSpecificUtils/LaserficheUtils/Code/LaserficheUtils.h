// LaserficheUtils.h : main header file for the LaserficheUtils DLL
//

#pragma once

// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the LASERFICHE_EXPORTS
// symbol defined on the command line. this symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// LASERFICHE_API functions as being imported from a DLL, wheras this DLL sees symbols
// defined with this macro as being exported.
#ifdef LASERFICHE_EXPORTS
#define LASERFICHE_API __declspec(dllexport)
#else
#define LASERFICHE_API __declspec(dllimport)
#endif
