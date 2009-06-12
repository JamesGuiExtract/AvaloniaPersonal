// stdafx.h : include file for standard system include files,
//  or project specific include files that are used frequently, but
//      are changed infrequently
//

#if !defined(AFX_STDAFX_H__487A8E5F_BAE7_4BF8_8547_95F364B4A000__INCLUDED_)
#define AFX_STDAFX_H__487A8E5F_BAE7_4BF8_8547_95F364B4A000__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#define VC_EXTRALEAN		// Exclude rarely-used stuff from Windows headers

#include <afxwin.h>         // MFC core and standard components
#include <afxext.h>         // MFC extensions
#include <afxdisp.h>        // MFC Automation classes
#include <afxdtctl.h>		// MFC support for Internet Explorer 4 Common Controls
#ifndef _AFX_NO_AFXCMN_SUPPORT
#include <afxcmn.h>			// MFC support for Windows Common Controls
#endif // _AFX_NO_AFXCMN_SUPPORT

#include <atlbase.h>

#pragma warning(disable:4786)

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDCOMUtils\Core\Code\UCLIDCOMUtils.tlb" named_guids
using namespace UCLIDCOMUTILSLib;

#import "..\..\..\..\ReusableComponents\COMComponents\UCLIDCOMUtils\RegExprParsers\IEVBScriptParser\Code\IEVBScriptParser.tlb" named_guids
using namespace UCLID_IEVBSCRIPTPARSERLib;

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_STDAFX_H__487A8E5F_BAE7_4BF8_8547_95F364B4A000__INCLUDED_)
