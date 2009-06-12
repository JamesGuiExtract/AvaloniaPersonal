// stdafx.h : include file for standard system include files,
//      or project specific include files that are used frequently,
//      but are changed infrequently

#pragma once

#pragma warning (disable : 4786)

#define _CRT_RAND_S

#include <CommonToExtractProducts.h>

#define STRICT 1
#define _ATL_APARTMENT_THREADED

#define OEMRESOURCE

#include <afxwin.h>
#include <afxdisp.h>
#include <afxcmn.h>			// For CListCtrl class
#include <afxext.h>
#include <afxdlgs.h>
#include <afxmt.h>

#include <atlbase.h>
//You may derive a class from CComModule and use it if you want to override
//something, but do not change the name of _Module

extern CComModule _Module;
#include <atlcom.h>

#import "..\..\UCLIDCOMUtils\Core\Code\UCLIDCOMUtils.tlb" named_guids
using namespace UCLID_COMUTILSLib;

#import "..\..\UCLIDComponentsLM\COMLM\Code\COMLM.tlb" named_guids 
using namespace UCLID_COMLMLib;

#import "c:\Program Files\Common Files\System\ADO\msado27.tlb" \
	rename ("EOF", "adoEOF")
using namespace ADODB;

#import "UCLIDFileProcessing.tlb" 


//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
