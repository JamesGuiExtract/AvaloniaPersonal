// SimpleMDIFrameworkDoc.h : interface of the CSimpleMDIFrameworkDoc class
//
/////////////////////////////////////////////////////////////////////////////

#if !defined(AFX_SIMPLEMDIFRAMEWORKDOC_H__47205F34_45D7_46BE_BBE2_D4243FC28555__INCLUDED_)
#define AFX_SIMPLEMDIFRAMEWORKDOC_H__47205F34_45D7_46BE_BBE2_D4243FC28555__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000


class CSimpleMDIFrameworkSrvrItem;

class CSimpleMDIFrameworkDoc : public COleServerDoc
{
protected: // create from serialization only
	CSimpleMDIFrameworkDoc();
	DECLARE_DYNCREATE(CSimpleMDIFrameworkDoc)

// Attributes
public:
	CSimpleMDIFrameworkSrvrItem* GetEmbeddedItem()
		{ return (CSimpleMDIFrameworkSrvrItem*)COleServerDoc::GetEmbeddedItem(); }

// Operations
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CSimpleMDIFrameworkDoc)
	protected:
	virtual COleServerItem* OnGetEmbeddedItem();
	public:
	virtual BOOL OnNewDocument();
	virtual void Serialize(CArchive& ar);
	//}}AFX_VIRTUAL

// Implementation
public:
	virtual ~CSimpleMDIFrameworkDoc();
#ifdef _DEBUG
	virtual void AssertValid() const;
	virtual void Dump(CDumpContext& dc) const;
#endif

protected:

// Generated message map functions
protected:
	//{{AFX_MSG(CSimpleMDIFrameworkDoc)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_SIMPLEMDIFRAMEWORKDOC_H__47205F34_45D7_46BE_BBE2_D4243FC28555__INCLUDED_)
