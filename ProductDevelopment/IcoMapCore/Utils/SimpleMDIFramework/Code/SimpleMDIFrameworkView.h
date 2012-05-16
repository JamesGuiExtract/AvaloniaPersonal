// SimpleMDIFrameworkView.h : interface of the CSimpleMDIFrameworkView class
//
/////////////////////////////////////////////////////////////////////////////

#if !defined(AFX_SIMPLEMDIFRAMEWORKVIEW_H__CC5EE824_C0E7_4A9A_AF18_205095E090AE__INCLUDED_)
#define AFX_SIMPLEMDIFRAMEWORKVIEW_H__CC5EE824_C0E7_4A9A_AF18_205095E090AE__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000


class CSimpleMDIFrameworkView : public CView
{
protected: // create from serialization only
	CSimpleMDIFrameworkView();
	DECLARE_DYNCREATE(CSimpleMDIFrameworkView)

// Attributes
public:
	CSimpleMDIFrameworkDoc* GetDocument();

// Operations
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CSimpleMDIFrameworkView)
	public:
	virtual void OnDraw(CDC* pDC);  // overridden to draw this view
	virtual BOOL PreCreateWindow(CREATESTRUCT& cs);
	protected:
	virtual BOOL OnPreparePrinting(CPrintInfo* pInfo);
	virtual void OnBeginPrinting(CDC* pDC, CPrintInfo* pInfo);
	virtual void OnEndPrinting(CDC* pDC, CPrintInfo* pInfo);
	//}}AFX_VIRTUAL

// Implementation
public:
	virtual ~CSimpleMDIFrameworkView();
#ifdef _DEBUG
	virtual void AssertValid() const;
	virtual void Dump(CDumpContext& dc) const;
#endif

protected:

// Generated message map functions
protected:
	//{{AFX_MSG(CSimpleMDIFrameworkView)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	afx_msg void OnCancelEditSrvr();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

#ifndef _DEBUG  // debug version in SimpleMDIFrameworkView.cpp
inline CSimpleMDIFrameworkDoc* CSimpleMDIFrameworkView::GetDocument()
   { return (CSimpleMDIFrameworkDoc*)m_pDocument; }
#endif

/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_SIMPLEMDIFRAMEWORKVIEW_H__CC5EE824_C0E7_4A9A_AF18_205095E090AE__INCLUDED_)
