// stdafx.h : include file for standard system include files,
//      or project specific include files that are used frequently,
//      but are changed infrequently

#pragma once

#define STRICT
#define _ATL_APARTMENT_THREADED

#include <CommonToExtractProducts.h>

#include <afxwin.h>
#include <afxdisp.h>
#include <afxcmn.h>			// For CListCtrl class
#include <afxext.h>
#include <atlbase.h>

//You may derive a class from CComModule and use it if you want to override
//something, but do not change the name of _Module
extern CComModule _Module;
#include <atlcom.h>
#include <atlctl.h>
#include "..\..\..\..\APIs\Microsoft Visual Studio\VC98\ATL\Include\atlcontrols.h"

#include <MemLeakDetection.h>

//using namespace ATL;

#import "..\..\..\UCLIDComponentsLM\COMLM\Code\COMLM.tlb" named_guids
using namespace UCLID_COMLMLib;

#import "..\..\..\UCLIDCOMUtils\Core\Code\UCLIDCOMUtils.tlb" named_guids
using namespace UCLID_COMUTILSLib;

#import "Common Files\System\ADO\msado27.tlb" \
	rename ("EOF", "adoEOF")
using namespace ADODB;

#import "..\..\..\..\..\RC.Net\Interfaces\Core\Code\Extract.Interfaces.tlb" named_guids
using namespace Extract_Interfaces;

#import "..\..\Code\UCLIDFileProcessing.tlb" named_guids
using namespace UCLID_FILEPROCESSINGLib;

#import "ESFileSuppliers.tlb"

#import "..\..\FileProcessors\Code\FileProcessors.tlb"
