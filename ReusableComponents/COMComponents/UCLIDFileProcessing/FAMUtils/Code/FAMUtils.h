// FAMUtils.h : main header file for the FAMUtils DLL
//

#pragma once

#ifndef __AFXWIN_H__
	#error "include 'stdafx.h' before including this file for PCH"
#endif

#include "resource.h"		// main symbols

#ifdef FAMUTILS_EXPORTS
#define FAMUTILS_API __declspec(dllexport)
#else
#define FAMUTILS_API __declspec(dllimport)
#endif
