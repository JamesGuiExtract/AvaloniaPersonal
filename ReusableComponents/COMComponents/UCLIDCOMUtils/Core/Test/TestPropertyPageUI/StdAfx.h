// stdafx.h : include file for standard system include files,
//  or project specific include files that are used frequently, but
//      are changed infrequently
//

#if !defined(AFX_STDAFX_H__A4FB1F3F_2D6A_4FCF_B3D3_60997AD097D3__INCLUDED_)
#define AFX_STDAFX_H__A4FB1F3F_2D6A_4FCF_B3D3_60997AD097D3__INCLUDED_

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
#include <comdef.h>

#import "..\\TestObjsWithPropPages\\TestObjsWithPropPages.tlb" named_guids
using namespace TESTOBJSWITHPROPPAGESLib;

#import "..\\..\\Code\\UCLIDCOMUtils.tlb" named_guids
using namespace UCLIDCOMUTILSLib;

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_STDAFX_H__A4FB1F3F_2D6A_4FCF_B3D3_60997AD097D3__INCLUDED_)
