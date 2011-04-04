// stdafx.h : include file for standard system include files,
//      or project specific include files that are used frequently,
//      but are changed infrequently

#pragma once

#define _CRT_RAND_S

#include <CommonToExtractProducts.h>

#define STRICT 1
#define _ATL_APARTMENT_THREADED

#define OEMRESOURCE

#include <afxwin.h>
#include <afxole.h>  // Needed for UltimateGrid
					 // (see http://www.codeproject.com/Messages/2869965/Re-Run-Time-Check-Failure-sharp0.aspx)
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

#import "..\..\UCLIDComponentsLM\COMLM\Code\COMLM.tlb" named_guids 
using namespace UCLID_COMLMLib;

#import "..\..\UCLIDCOMUtils\Core\Code\UCLIDCOMUtils.tlb" named_guids
using namespace UCLID_COMUTILSLib;

#import "..\..\..\..\RC.Net\Utilities\Email\Core\Code\Extract.Utilities.Email.tlb" named_guids 
using namespace Extract_Utilities_Email;

#import "c:\Program Files\Common Files\System\ADO\msado27.tlb" \
	rename ("EOF", "adoEOF")
using namespace ADODB;

#import "UCLIDFileProcessing.tlb" 
