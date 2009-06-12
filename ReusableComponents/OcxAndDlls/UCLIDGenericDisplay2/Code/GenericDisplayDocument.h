//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	GenericDisplayDocument.h
//
// PURPOSE:	This is an header file for CGenericDisplayDocument 
//			where this class has been derived from the CDocument()
//			class.  The code written in this file makes it possible for
//			initialize the rich edit document properties.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================

#if !defined(AFX_GENERICDISPLAYDOCUMENT_H__14981589_9117_11D4_9725_008048FBC96E__INCLUDED_)
#define AFX_GENERICDISPLAYDOCUMENT_H__14981589_9117_11D4_9725_008048FBC96E__INCLUDED_

// GenericDisplayDocument.h : header file
//
#include "GenericDisplayCtl.h"
#include "GenericDisplayView.h"

//==================================================================================================
//
// CLASS:	CGenericDisplayDocument
//
// PURPOSE:	This class is used to derive GenericDisplayDocument from MFC class CDocument.
//			This derived control is inserted in frame.  Object of this class is 
//			created in GenericDisplayFrame.cpp
// REQUIRE:	Nothing
// 
// INVARIANTS:
//			
// EXTENSIONS:	Nothing
//			
//
// NOTES:	
//
//==================================================================================================

class CGenericDisplayDocument : public COleDocument
{
protected:
	CGenericDisplayDocument();           // protected constructor used by dynamic creation
	DECLARE_DYNCREATE(CGenericDisplayDocument)

// Attributes
public:

// Operations
public:

// Overrides

	virtual COleClientItem* CreateClientItem(REOBJECT* preo) const;

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CGenericDisplayDocument)
	public:
//	virtual void Serialize(CArchive& ar);   // overridden for document i/o
	protected:
	virtual BOOL OnNewDocument();
	//}}AFX_VIRTUAL

// Implementation
public:
	virtual ~CGenericDisplayDocument();
#ifdef _DEBUG
	virtual void AssertValid() const;
	virtual void Dump(CDumpContext& dc) const;
#endif

	// Generated message map functions
protected:
	//{{AFX_MSG(CGenericDisplayDocument)
		// NOTE - the ClassWizard will add and remove member functions here.
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_GENERICDISPLAYDOCUMENT_H__14981589_9117_11D4_9725_008048FBC96E__INCLUDED_)
