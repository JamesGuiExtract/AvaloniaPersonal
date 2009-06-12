// stdafx.h : include file for standard system include files,
//      or project specific include files that are used frequently,
//      but are changed infrequently

#if !defined(AFX_STDAFX_H__59870863_10F9_4B02_AA7E_575EBFA1668F__INCLUDED_)
#define AFX_STDAFX_H__59870863_10F9_4B02_AA7E_575EBFA1668F__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#pragma warning(disable:4786)

#ifndef STRICT
#define STRICT
#endif

#ifndef _WIN32_WINNT
#define _WIN32_WINNT 0x0400
#endif
#define _ATL_APARTMENT_THREADED

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

#include <afxmt.h>

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDComponentsLM\COMLM\Code\COMLM.tlb" named_guids 
using namespace UCLID_COMLMLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDCOMUtils\Core\Code\UCLIDCOMUtils.tlb" named_guids
using namespace UCLID_COMUTILSLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDRasterAndOCRMgmt\Core\Code\UCLIDRasterAndOCRMgmt.tlb" named_guids \
	rename("LoadImage", "LoadRasterImage")
using namespace UCLID_RASTERANDOCRMGMTLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDRasterAndOCRMgmt\OCREngines\SSOCR\Code\SSOCR.tlb" named_guids
using namespace UCLID_SSOCRLib;

#import "..\..\..\..\ReusableComponents\COMComponents\ESMessageUtils\Code\ESMessageUtils.tlb" named_guids
using namespace ESMESSAGEUTILSLib;

#import "..\..\..\..\ReusableComponents\InputFunnel\IFCore\Code\IFCore.tlb" named_guids
using namespace UCLID_INPUTFUNNELLib;

#import "..\..\..\..\ReusableComponents\InputFunnel\InputReceivers\SpotRecognitionIR\Code\Core\SpotRecognitionIR.tlb" named_guids
using namespace UCLID_SPOTRECOGNITIONIRLib;

#import "AFCore.tlb"

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_STDAFX_H__59870863_10F9_4B02_AA7E_575EBFA1668F__INCLUDED)
