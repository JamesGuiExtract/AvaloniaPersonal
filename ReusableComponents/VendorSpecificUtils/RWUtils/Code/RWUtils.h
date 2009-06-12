
#pragma once

// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the RW_UTILS_EXPORTS
// symbol defined on the command line. this symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// RW_UTILS_API functions as being imported from a DLL, wheras this DLL sees symbols
// defined with this macro as being exported.
#ifdef RW_UTILS_EXPORTS
#define RW_UTILS_API __declspec(dllexport)
#else
#define RW_UTILS_API __declspec(dllimport)
#endif

// This class is exported
class RW_UTILS_API RWInitializer 
{
public:
	RWInitializer();
};

// This class is exported
class RW_UTILS_API RWCleanup 
{
public:
	RWCleanup();
};


