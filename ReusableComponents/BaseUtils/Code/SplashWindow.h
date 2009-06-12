//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	SplashWindow.h
//
// PURPOSE:	
//
// NOTES:	This code was originally downloaded from AutoDesk's ADN site, and was modified
//			thereafter.  Some copyrights of AutoDesk may still apply to this code.
//
// AUTHORS:	Arvind Ganesan
//
//==================================================================================================

#pragma once

#include "BaseUtils.h"

class EXPORT_BaseUtils SplashWindow : public CWnd
{
public:
	static void sShowSplashScreen(CBitmap *pBitmap, CWnd *pParentWnd = NULL);

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CSplashWindow)
	//}}AFX_VIRTUAL
protected:
	static const int iDISPLAY_DURATION;
	SplashWindow(CBitmap *pBitmap);
	~SplashWindow();
	virtual void PostNcDestroy();
	BOOL Create(CWnd *pParentWnd =NULL);
	void HideSplashScreen();

	//{{AFX_MSG(CSplashWindow)
	afx_msg int OnCreate(LPCREATESTRUCT lpCreateStruct);
	afx_msg void OnPaint();
	afx_msg void OnTimer(UINT nIDEvent);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP ()

private:
	CBitmap *m_pBitmap;
} ;
