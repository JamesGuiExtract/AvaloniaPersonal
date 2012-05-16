//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	IcoMapOptionsDlg.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS: Duan Wang
//
//==================================================================================================

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include "KeyboardInputSettingsDlg.h"	// Added by ClassView
#include "GeneralSettingsDlg.h"	// Added by ClassView
#include "ShortcutSettings.h"
#include "DirectionSettingsDlg.h"

/////////////////////////////////////////////////////////////////////////////
// IcoMapOptionsDlg

class IcoMapOptionsDlg : public CPropertySheet
{
	DECLARE_DYNAMIC(IcoMapOptionsDlg)

// Construction
public:
	//IcoMapOptionsDlg(UINT nIDCaption, CWnd* pParentWnd = NULL, UINT iSelectPage = 0);
	IcoMapOptionsDlg(LPCTSTR pszCaption, CWnd* pParentWnd = NULL, UINT iSelectPage = 0);

// Attributes
public:

// Operations
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(IcoMapOptionsDlg)
	public:
	virtual void OnFinalRelease();
	virtual int DoModal();
	virtual BOOL OnInitDialog();
	//}}AFX_VIRTUAL

// Implementation
public:
	virtual ~IcoMapOptionsDlg();

	// Generated message map functions
protected:
	GeneralSettingsDlg			m_GeneralTab;
	KeyboardInputSettingsDlg	m_KeyboardInputTab;
	CShortcutSettings			m_ShortcutTab;
	DirectionSettingsDlg		m_DirectionTab;

	//{{AFX_MSG(IcoMapOptionsDlg)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
	// Generated OLE dispatch map functions
	//{{AFX_DISPATCH(IcoMapOptionsDlg)
		// NOTE - the ClassWizard will add and remove member functions here.
	//}}AFX_DISPATCH
	DECLARE_DISPATCH_MAP()
	DECLARE_INTERFACE_MAP()
private:

	// Icon to be displayed on title bar
	HICON			m_hIcon;

};

/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.
