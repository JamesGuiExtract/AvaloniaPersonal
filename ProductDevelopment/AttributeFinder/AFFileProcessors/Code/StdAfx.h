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
#include "..\..\..\..\ReusableComponents\APIs\Microsoft Visual Studio\VC98\ATL\Include\atlcontrols.h"

#include <MemLeakDetection.h>

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDCOMUtils\Core\Code\UCLIDCOMUtils.tlb" named_guids
using namespace UCLID_COMUTILSLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDComponentsLM\COMLM\Code\COMLM.tlb" named_guids 
using namespace UCLID_COMLMLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDRasterAndOCRMgmt\Core\Code\UCLIDRasterAndOCRMgmt.tlb" named_guids \
	rename("LoadImage", "LoadRasterImage")
using namespace UCLID_RASTERANDOCRMGMTLib;

#import "..\..\..\..\ReusableComponents\InputFunnel\IFCore\Code\IFCore.tlb" named_guids
using namespace UCLID_INPUTFUNNELLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDRasterAndOCRMgmt\OCREngines\SSOCR\Code\SSOCR.tlb" named_guids
using namespace UCLID_SSOCRLib;

#import "..\..\AFCore\Code\AFCore.tlb" named_guids
using namespace UCLID_AFCORELib;

#import "..\..\AFUtils\Code\AFUtils.tlb" named_guids
using namespace UCLID_AFUTILSLib;

#import "..\..\AFOutputHandlers\Code\AFOutputHandlers.tlb" named_guids
using namespace UCLID_AFOUTPUTHANDLERSLib;

#import "..\..\AFValueModifiers\Code\AFValueModifiers.tlb" named_guids
using namespace UCLID_AFVALUEMODIFIERSLib;

#import "C:\Program Files (x86)\Common Files\System\ADO\msado28.tlb" \
	rename ("EOF", "adoEOF")
using namespace ADODB;

#import "..\..\..\..\..\RC.Net\Interfaces\Core\Code\Extract.Interfaces.tlb" named_guids
using namespace Extract_Interfaces;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDFileProcessing\Code\UCLIDFileProcessing.tlb" named_guids
using namespace UCLID_FILEPROCESSINGLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDImageUtils\Code\UCLIDImageUtils.tlb" named_guids
using namespace UCLID_IMAGEUTILSLib;

#import "..\..\..\..\..\RC.Net\FileActionManager\Database\Core\Code\Extract.FileActionManager.Database.tlb" named_guids
using namespace Extract_FileActionManager_Database;

#import "AFFileProcessors.tlb"

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
