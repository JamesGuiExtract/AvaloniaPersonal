#if !defined(AFX_OBJPROPERTIESDLG_H__D32729FB_AA24_49DB_A261_3673F2D04151__INCLUDED_)
#define AFX_OBJPROPERTIESDLG_H__D32729FB_AA24_49DB_A261_3673F2D04151__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
// ObjPropertiesDlg.h : header file
//
#include <string>
/////////////////////////////////////////////////////////////////////////////
// ObjPropertiesDlg dialog

class ObjPropertiesDlg : public CDialog
{
// Construction
public:
	ObjPropertiesDlg(IUnknown *pObjWithPropPage, const char *pszWindowTitle, 
		CWnd* pParent = NULL);

// Dialog Data
	//{{AFX_DATA(ObjPropertiesDlg)
	enum { IDD = IDD_DLG_OBJ_PROPERTIES };
		// NOTE: the ClassWizard will add data members here
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(ObjPropertiesDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

protected:

	DECLARE_INTERFACE_MAP()

	BEGIN_INTERFACE_PART( PropertyPageSite, IPropertyPageSite )
        STDMETHOD( OnStatusChange )( DWORD dwFlags );
        STDMETHOD( GetLocaleID )( LCID *pLocaleID );
        STDMETHOD( GetPageContainer )( IUnknown **ppUnk );
        STDMETHOD( TranslateAccelerator )( MSG *pMsg );
	END_INTERFACE_PART( PropertyPageSite )

	// the title for the property page container window
	std::string m_strWindowTitle;

	// pointer to the object whose property page is to be shown
	IUnknownPtr m_ipObjWithPropPage;

	// the currently displayed property page object
	IPropertyPagePtr m_pCurrentPropPage;

	void SetCurrentPage(IUnknown * pUnknown);

	// Generated message map functions
	//{{AFX_MSG(ObjPropertiesDlg)
	virtual void OnOK();
	virtual BOOL OnInitDialog();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
	// according to each property page size, update the dialog to
	// fit the property page
	void updateDialogSize(SIZE propPageSize, RECT &rectForHoldingPP);
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_OBJPROPERTIESDLG_H__D32729FB_AA24_49DB_A261_3673F2D04151__INCLUDED_)
