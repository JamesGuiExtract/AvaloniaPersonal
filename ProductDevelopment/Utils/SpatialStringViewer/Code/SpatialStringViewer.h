// SpatialStringViewer.h : main header file for the SPATIALSTRINGVIEWER application
//

#if !defined(AFX_SPATIALSTRINGVIEWER_H__28AA379A_8EA5_4692_AF45_90F506854514__INCLUDED_)
#define AFX_SPATIALSTRINGVIEWER_H__28AA379A_8EA5_4692_AF45_90F506854514__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// main symbols

/////////////////////////////////////////////////////////////////////////////
// CSpatialStringViewerApp:
// See SpatialStringViewer.cpp for the implementation of this class
//

class CSpatialStringViewerApp : public CWinApp
{
public:
	CSpatialStringViewerApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CSpatialStringViewerApp)
	public:
	virtual BOOL InitInstance();
	virtual BOOL ProcessMessageFilter(int code, LPMSG lpMsg);
	//}}AFX_VIRTUAL

// Implementation

	//{{AFX_MSG(CSpatialStringViewerApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	HACCEL m_hAccel;
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_SPATIALSTRINGVIEWER_H__28AA379A_8EA5_4692_AF45_90F506854514__INCLUDED_)
