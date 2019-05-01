// stdafx.h : include file for standard system include files,
//      or project specific include files that are used frequently,
//      but are changed infrequently

#pragma once

#define STRICT
#ifndef _WIN32_WINNT
#define _WIN32_WINNT 0x0600
#endif
#define _ATL_APARTMENT_THREADED

#pragma warning (disable : 4786)

#include <afxwin.h>
#include <afxdisp.h>

#include <atlbase.h>
//You may derive a class from CComModule and use it if you want to override
//something, but do not change the name of _Module
extern CComModule _Module;
#include <atlcom.h>


#import "..\..\UCLIDCOMUtils\Core\Code\UCLIDCOMUtils.tlb" named_guids
using namespace UCLID_COMUTILSLib;

#import "..\..\UCLIDCurveParameter\Code\UCLIDCurveParameter.tlb" named_guids
using namespace UCLID_CURVEPARAMETERLib;

#import "..\..\UCLIDComponentsLM\COMLM\Code\COMLM.tlb" named_guids 
using namespace UCLID_COMLMLib;

#import "..\..\UCLIDDistanceConverter\Code\UCLIDDistanceConverter.tlb" named_guids
using namespace UCLID_DISTANCECONVERTERLib;

#import "..\..\UCLIDMeasurements\Code\UCLIDMeasurements.tlb" named_guids 
using namespace UCLID_MEASUREMENTSLib;

#import "UCLIDFeatureMgmt.tlb"

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
