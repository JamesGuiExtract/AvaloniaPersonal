
#include "..\..\BaseUtils\Code\CommonToExtractProducts.h"

#if defined( _DEBUG) && defined(NDEBUG_MFC)
    // Building debug version to be used with non-debug/Prod AutoCAD"
    #define _DEBUG_WAS_DEFINED
    #undef _DEBUG
#endif

#include <afxwin.h>
#include <afxext.h>
#include <afxcmn.h>
#include <afxdlgs.h>
#include <afxdisp.h>

#ifdef _DEBUG_WAS_DEFINED
    #define _DEBUG
    #undef _DEBUG_WAS_DEFINED
#endif

#pragma warning(disable:4786)	// disable STL warning
