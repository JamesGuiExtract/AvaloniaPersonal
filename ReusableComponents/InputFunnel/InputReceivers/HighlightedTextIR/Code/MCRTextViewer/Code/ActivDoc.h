//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ActiveDoc.h
//
// PURPOSE:	This is an header file for CActiveXDocTemplate and 
//			CActiveXDocControl classes where these classes have been
//			derived from the CSingleDocTemplate() and COleControl
//			classes.  The code written in this file makes it possible for
//			initialization of controls and creating the frame, view and document
//			and set the control to save	and open the documents
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================

//#include "stdafx.h"

//==================================================================================================
//
// CLASS:	CActiveXDocTemplate
//
// PURPOSE:	This class is used to derive SingleDocTemplate from MFC class CSingleDocTemplate.
//			This derived template is attached to Frame.  Object of this class is 
//			created in MCRTextViewer.cpp.  It has Open/Save document functions to the 
//			text files.
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

class CActiveXDocTemplate : public CSingleDocTemplate
{
    enum { IDR_NOTUSED = 0x7FFF };

    CWnd* m_pParentWnd;
    CFrameWnd* m_pFrameWnd;
    CString m_docFile;

public:
    CActiveXDocTemplate(CRuntimeClass* pDocClass,
        CRuntimeClass* pFrameClass, CRuntimeClass* pViewClass);

    CFrameWnd* CreateDocViewFrame(CWnd* pParentWnd);

	//----------------------------------------------------------------------------------------------
	// PURPOSE: To Save Document file
	// REQUIRE: Nothing
	// PROMISE: Nothing
    void SaveDocumentFile();

    virtual CFrameWnd* CreateNewFrame(CDocument* pDoc,
        CFrameWnd* pOther);
    virtual CDocument* OpenDocumentFile(
        LPCTSTR lpszPathName, BOOL bVerifyExists = TRUE);
};

/////////////////////////////////////////////////////////////////////////////
//==================================================================================================
//
// CLASS:	CActiveXDocControl
//
// PURPOSE:	This class is used to derive OleControl from MFC class COleControl.
//			This derived control will handle the frame and view.  Object of this class is 
//			created in MCRTextViewer.cpp.  It has Open/Save document functions to the 
//			text files and interaction between the different document - view objects.
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
class CActiveXDocControl : public COleControl
{
    enum { WM_IDLEUPDATECMDUI = 0x0363 };

    static BOOL m_bDocInitialized;
    CActiveXDocTemplate* m_pDocTemplate;

    DECLARE_DYNAMIC(CActiveXDocControl)

protected:
    CFrameWnd* m_pFrameWnd;
    void AddDocTemplate(CActiveXDocTemplate* pDocTemplate);
    CDocTemplate* GetDocTemplate() { return m_pDocTemplate; }

    //{{AFX_MSG(CActiveXDocControl)
    afx_msg int OnCreate(LPCREATESTRUCT lpCreateStruct);
    afx_msg void OnSize(UINT nType, int cx, int cy);
    afx_msg void OnTimer(UINT nIDEvent);
    afx_msg void OnDestroy();
    //}}AFX_MSG
    //{{AFX_DISPATCH(CActiveXDocControl)
    //}}AFX_DISPATCH
    //{{AFX_EVENT(CActiveXDocControl)
    //}}AFX_EVENT

    DECLARE_MESSAGE_MAP()
    DECLARE_DISPATCH_MAP()
    DECLARE_EVENT_MAP()

public:
    CActiveXDocControl();
    virtual ~CActiveXDocControl();

    enum {
    //{{AFX_DISP_ID(CActiveXDocControl)
    //}}AFX_DISP_ID
    };
};
