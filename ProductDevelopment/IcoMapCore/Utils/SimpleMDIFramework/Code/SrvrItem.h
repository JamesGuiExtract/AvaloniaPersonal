// SrvrItem.h : interface of the CSimpleMDIFrameworkSrvrItem class
//

#if !defined(AFX_SRVRITEM_H__3B75B511_682B_4F87_AE1B_604DAF30DEBE__INCLUDED_)
#define AFX_SRVRITEM_H__3B75B511_682B_4F87_AE1B_604DAF30DEBE__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

class CSimpleMDIFrameworkSrvrItem : public COleServerItem
{
	DECLARE_DYNAMIC(CSimpleMDIFrameworkSrvrItem)

// Constructors
public:
	CSimpleMDIFrameworkSrvrItem(CSimpleMDIFrameworkDoc* pContainerDoc);

// Attributes
	CSimpleMDIFrameworkDoc* GetDocument() const
		{ return (CSimpleMDIFrameworkDoc*)COleServerItem::GetDocument(); }

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CSimpleMDIFrameworkSrvrItem)
	public:
	virtual BOOL OnDraw(CDC* pDC, CSize& rSize);
	virtual BOOL OnGetExtent(DVASPECT dwDrawAspect, CSize& rSize);
	//}}AFX_VIRTUAL

// Implementation
public:
	~CSimpleMDIFrameworkSrvrItem();
#ifdef _DEBUG
	virtual void AssertValid() const;
	virtual void Dump(CDumpContext& dc) const;
#endif

protected:
	virtual void Serialize(CArchive& ar);   // overridden for document i/o
};

/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_SRVRITEM_H__3B75B511_682B_4F87_AE1B_604DAF30DEBE__INCLUDED_)
