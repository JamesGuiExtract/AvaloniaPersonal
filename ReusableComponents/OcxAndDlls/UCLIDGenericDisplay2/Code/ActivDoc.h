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

//==================================================================================================
//
// CLASS:	CActiveXDocTemplate
//
// PURPOSE:	This class is used to derive SingleDocTemplate from MFC class CSingleDocTemplate.
//			This derived template is attached to Frame.  Object of this class is 
//			created in GenericDisplay.cpp.  It has Open/Save document functions to the 
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

	//	to get the parent CWnd object
    CWnd* m_pParentWnd;

	//	to get the frame window
    CFrameWnd* m_pFrameWnd;

	//	string object
    CString m_docFile;

public:
	//--------------------------------------------------------
	// PURPOSE:	To create the ActiveX Document Template.
	// REQUIRE: Document, View and Frame Classes.
	// PROMISE: Initializes the ActiveXDoc Template.
    CActiveXDocTemplate(CRuntimeClass* pDocClass,
        CRuntimeClass* pFrameClass, CRuntimeClass* pViewClass);
	//--------------------------------------------------------
	// PURPOSE:	To create the Frame.
	// REQUIRE: None
	// PROMISE: Initializes the Frame.
    CFrameWnd* CreateDocViewFrame(CWnd* pParentWnd);
	//--------------------------------------------------------
	// PURPOSE:	To Save the data in document.
	// REQUIRE: None
	// PROMISE: None
    void SaveDocumentFile();
	//--------------------------------------------------------
	// PURPOSE:	To create New Frame.
	// REQUIRE: None
	// PROMISE: Initializes New Frame.
    virtual CFrameWnd* CreateNewFrame(CDocument* pDoc,
        CFrameWnd* pOther);
	//--------------------------------------------------------
	// PURPOSE:	To open the document file 
	// REQUIRE: Existance of File
	// PROMISE: None
    virtual CDocument* OpenDocumentFile(
        LPCTSTR lpszPathName, BOOL bVerifyExists = TRUE);

	// This method's source code was copied from docsingl.cpp file's
	// CSingleDocTemplate::OpenDocumentFile() method on 4/12/2006 by Arvind.
	// TODO: Keep this source code in sync with the latest docsingl.cpp
	// NOTE: This source code was copied from the above mentioned file shipped with VS2005.
	// It should be kept synchronized with the the source code in docsingl.cpp that is 
	// shipped with future versions of the IDE/libraries as we upgrade to them.
	CDocument* OpenDocumentFileInternal(LPCTSTR lpszPathName,
		BOOL bMakeVisible);
};

/////////////////////////////////////////////////////////////////////////////
//==================================================================================================
//
// CLASS:	CActiveXDocControl
//
// PURPOSE:	This class is used to derive OleControl from MFC class COleControl.
//			This derived control will handle the frame and view.  Object of this class is 
//			created in GenericDisplay.cpp.  It has Open/Save document functions to the 
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

	//	Boolean variable to know whether the document is initialized or not
    static BOOL m_bDocInitialized;

	//	ActiveXDocTemplate
    CActiveXDocTemplate* m_pDocTemplate;


    DECLARE_DYNAMIC(CActiveXDocControl)

protected:
	//	FrameWnd
    CFrameWnd* m_pFrameWnd;
	//--------------------------------------------------------
	// PURPOSE:	To Add Doc Template.
	// REQUIRE: None
	// PROMISE: None
    void AddDocTemplate(CActiveXDocTemplate* pDocTemplate);
	//--------------------------------------------------------
	// PURPOSE:	To get the DocTemplate.
	// REQUIRE: None
	// PROMISE: None
    CDocTemplate* GetDocTemplate() 
	{
		return m_pDocTemplate; 
	}

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
