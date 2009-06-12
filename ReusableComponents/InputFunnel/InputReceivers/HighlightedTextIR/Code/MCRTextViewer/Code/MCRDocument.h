//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	MCRDocument.h
//
// PURPOSE:	This is an header file for CMCRDocument 
//			where this class has been derived from the CRichEditDoc()
//			class.  The code written in this file makes it possible for
//			initialize the rich edit document properties.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================
//#if !defined(AFX_MCRDOCUMENT_H__7F4F91B4_62B9_11D4_96EE_008048FBC96E__INCLUDED_)
//#define AFX_MCRDOCUMENT_H__7F4F91B4_62B9_11D4_96EE_008048FBC96E__INCLUDED_


// MCRDocument.h : header file
//


//class CMCRView;

/////////////////////////////////////////////////////////////////////////////
// CMCRDocument document
//==================================================================================================
//
// CLASS:	CMCRDocument
//
// PURPOSE:	This class is used to derive MCRDocument from MFC class CRichEditDoc.
//			This derived control is inserted in frame.  Object of this class is 
//			created in MCRFrame.cpp
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
class CMCRDocument : public CRichEditDoc
{
protected:
	CMCRDocument();           // protected constructor used by dynamic creation
	DECLARE_DYNCREATE(CMCRDocument)

// Attributes
public:

// Operations
public:

// Overrides
	virtual CRichEditCntrItem* CreateClientItem(REOBJECT* preo) const;

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CMCRDocument)
	public:
	
	protected:
	virtual BOOL OnNewDocument();
	//}}AFX_VIRTUAL

// Implementation
public:
	virtual ~CMCRDocument();
//	virtual void PreCloseFrame(CFrameWnd* pFrameArg);
#ifdef _DEBUG
	virtual void AssertValid() const;
	virtual void Dump(CDumpContext& dc) const;
#endif

	// Generated message map functions
protected:
	//{{AFX_MSG(CMCRDocument)
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

//#endif // !defined(AFX_MCRDOCUMENT_H__7F4F91B4_62B9_11D4_96EE_008048FBC96E__INCLUDED_)
