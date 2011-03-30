//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ActiveDoc.cpp
//
// PURPOSE:	This is an implementation file for CActiveXDocTemplate()
//			and CActiveXDocControl() classes. where the CActiveXDocTemplate()
//			has been derived from CSingleDocTemplate and CActiveXDocControl()
//			class.  The code written in this file makes it possible for
//			creating the frame, view and document and set the control to save
//			and open the documents.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================
// ActivDoc.cpp : implementation file
//

#include "stdafx.h"
#include "ActivDoc.h"
#include "GenericDisplayView.h"
#include "UCLIDException.h"

//==========================================================================================
CActiveXDocTemplate::CActiveXDocTemplate(CRuntimeClass* pDocClass,
    CRuntimeClass* pFrameClass, CRuntimeClass* pViewClass)
        : CSingleDocTemplate(IDR_NOTUSED, pDocClass, pFrameClass,
            pViewClass)
{
    ASSERT(pFrameClass);
}
//==========================================================================================
CFrameWnd* CActiveXDocTemplate::CreateDocViewFrame(CWnd* pParentWnd)
{
    ASSERT(pParentWnd && IsWindow(*pParentWnd));
    ASSERT_KINDOF(CActiveXDocControl, pParentWnd);
    m_pParentWnd = pParentWnd;
    m_pFrameWnd = NULL;

    // OpenDocumentFile is a virtual method (implemented in
    // the CSingleDocTemplate base class) that creates
    // the document (either empty or loaded from the specified
    // file) and calls our CreateNewFrame function. Passing
    // NULL to the function creates a new document. Incidentally,
    // the view is created by the CFrameWnd's OnCreateClient()
    // method.

    if (!OpenDocumentFile(NULL))
        return NULL;

    // Since OpenDocumentFile sets m_pFrame, we can now use it.

    ASSERT(m_pFrameWnd);
    ASSERT_KINDOF(CFrameWnd, m_pFrameWnd);
    m_pFrameWnd->ShowWindow(SW_SHOWNORMAL);
    return m_pFrameWnd;
}
//==========================================================================================
CFrameWnd* CActiveXDocTemplate::CreateNewFrame(CDocument* pDoc,
        CFrameWnd* pOther)
{
    ASSERT(pOther == NULL);
    ASSERT(m_pFrameClass != __nullptr);
    if (pDoc != __nullptr)
        ASSERT_VALID(pDoc);

    // Create a frame wired to the specified document

    CCreateContext context;
    context.m_pCurrentFrame = pOther;
    context.m_pCurrentDoc = pDoc;
    context.m_pNewViewClass = m_pViewClass;
    context.m_pNewDocTemplate = this;

    m_pFrameWnd = (CFrameWnd*)m_pFrameClass->CreateObject();
    if (m_pFrameWnd == NULL)
    {
        TRACE1("Warning: Dynamic create of frame %hs failed.\n",
            m_pFrameClass->m_lpszClassName);
        return NULL;
    }
    ASSERT_KINDOF(CFrameWnd, m_pFrameWnd);

    if (context.m_pNewViewClass == NULL)
        TRACE0("Warning: creating frame with no default view.\n");

    // The frame is created as a menu-less child of the
    // CActiveXDocControl in which it will reside.

    ASSERT_KINDOF(CActiveXDocControl, m_pParentWnd);
    if (!m_pFrameWnd->Create(NULL, "", WS_CHILD|WS_VISIBLE,
        CFrameWnd::rectDefault, m_pParentWnd, NULL, 0, &context))
    {
        TRACE0("Warning: CDocTemplate couldn't create a frame.\n");
        return NULL;
    }

    return m_pFrameWnd;
}
//==========================================================================================
CDocument* CActiveXDocTemplate::OpenDocumentFile(
    LPCTSTR lpszPathName, BOOL bVerifyExists)
{
    SaveDocumentFile();
    m_docFile = lpszPathName;

	//	check for existance of the file
    if (bVerifyExists)
    {
        DWORD dwAttrib = GetFileAttributes(m_docFile);
        if (dwAttrib == 0xFFFFFFFF ||
            dwAttrib == FILE_ATTRIBUTE_DIRECTORY)
        {
            lpszPathName = NULL;
        }
    }

    return OpenDocumentFileInternal(
        lpszPathName, TRUE);
}
//==========================================================================================
// This method's source code was copied from docsingl.cpp file's
// CSingleDocTemplate::OpenDocumentFile() method on 4/12/2006 by Arvind.
// TODO: Keep this source code in sync with the latest docsingl.cpp
// NOTE: This source code was copied from the above mentioned file shipped with VS2005.
// It should be kept synchronized with the the source code in docsingl.cpp that is 
// shipped with future versions of the IDE/libraries as we upgrade to them.
CDocument* CActiveXDocTemplate::OpenDocumentFileInternal(LPCTSTR lpszPathName,
	BOOL bMakeVisible)
	// if lpszPathName == NULL => create new file of this type
{
	CDocument* pDocument = NULL;
	CFrameWnd* pFrame = NULL;
	BOOL bCreated = FALSE;      // => doc and frame created
	BOOL bWasModified = FALSE;

	if (m_pOnlyDoc != __nullptr)
	{
		// already have a document - reinit it
		pDocument = m_pOnlyDoc;
		if (!pDocument->SaveModified())
			return NULL;        // leave the original one

		pFrame = (CFrameWnd*)AfxGetMainWnd();
		ASSERT(pFrame != __nullptr);
		ASSERT_KINDOF(CFrameWnd, pFrame);
		ASSERT_VALID(pFrame);
	}
	else
	{
		// create a new document
		pDocument = CreateNewDocument();
		ASSERT(pFrame == NULL);     // will be created below
		bCreated = TRUE;
	}

	if (pDocument == NULL)
	{
		AfxMessageBox(AFX_IDP_FAILED_TO_CREATE_DOC);
		return NULL;
	}
	ASSERT(pDocument == m_pOnlyDoc);

	if (pFrame == NULL)
	{
		ASSERT(bCreated);

		// create frame - set as main document frame
		BOOL bAutoDelete = pDocument->m_bAutoDelete;
		pDocument->m_bAutoDelete = FALSE;
					// don't destroy if something goes wrong
		pFrame = CreateNewFrame(pDocument, NULL);
		pDocument->m_bAutoDelete = bAutoDelete;
		if (pFrame == NULL)
		{
			AfxMessageBox(AFX_IDP_FAILED_TO_CREATE_DOC);
			delete pDocument;       // explicit delete on error
			return NULL;
		}
	}

	if (lpszPathName == NULL)
	{
		// create a new document
		SetDefaultTitle(pDocument);

		// avoid creating temporary compound file when starting up invisible
		if (!bMakeVisible)
			pDocument->m_bEmbedded = TRUE;

		if (!pDocument->OnNewDocument())
		{
			// user has been alerted to what failed in OnNewDocument
			TRACE(traceAppMsg, 0, "CDocument::OnNewDocument returned FALSE.\n");
			if (bCreated)
				pFrame->DestroyWindow();    // will destroy document
			return NULL;
		}
	}
	else
	{
		CWaitCursor wait;

		// open an existing document
		bWasModified = pDocument->IsModified();
		pDocument->SetModifiedFlag(FALSE);  // not dirty for open

		if (!pDocument->OnOpenDocument(lpszPathName))
		{
			// user has been alerted to what failed in OnOpenDocument
			TRACE(traceAppMsg, 0, "CDocument::OnOpenDocument returned FALSE.\n");
			if (bCreated)
			{
				pFrame->DestroyWindow();    // will destroy document
			}
			else if (!pDocument->IsModified())
			{
				// original document is untouched
				pDocument->SetModifiedFlag(bWasModified);
			}
			else
			{
				// we corrupted the original document
				SetDefaultTitle(pDocument);

				if (!pDocument->OnNewDocument())
				{
					TRACE(traceAppMsg, 0, "Error: OnNewDocument failed after trying "
						"to open a document - trying to continue.\n");
					// assume we can continue
				}
			}
			return NULL;        // open failed
		}
		pDocument->SetPathName(lpszPathName);
	}

	CWinThread* pThread = AfxGetThread();
	
	// The Following lines of code were added on 04/12/2006 by Arvind
	// based upon the documentation available at:
	// http://msdn2.microsoft.com/en-us/library/0aa1ya36.aspx
	// which mentions that the return value of AfxGetThread() is different
	// in VC++ 6.0 versus later versions.
	// This code change was required to fix defect P16#1829.
	// [BEGIN_CODE_ADDITION]
	if (pThread == NULL)
	{
		pThread = AfxGetApp();
	}
	// [END_CODE_ADDITION]

	ASSERT(pThread);
	if (bCreated && pThread->m_pMainWnd == NULL)
	{
		// set as main frame (InitialUpdateFrame will show the window)
		pThread->m_pMainWnd = pFrame;
	}
	InitialUpdateFrame(pFrame, pDocument, bMakeVisible);

	return pDocument;
}
//==========================================================================================
void CActiveXDocTemplate::SaveDocumentFile()
{
    if (m_pOnlyDoc != __nullptr)
    {
        if (!m_docFile.IsEmpty())
			//	save document file
            m_pOnlyDoc->OnSaveDocument(m_docFile);
        else
			//	set modified flag
            m_pOnlyDoc->SetModifiedFlag(FALSE);
    }
}
//==========================================================================================
/////////////////////////////////////////////////////////////////////////////
// CActiveXDocControl

IMPLEMENT_DYNAMIC(CActiveXDocControl, COleControl)
BEGIN_MESSAGE_MAP(CActiveXDocControl, COleControl)
    //{{AFX_MSG_MAP(CActiveXDocControl)
    ON_WM_CREATE()
    ON_WM_SIZE()
    ON_WM_TIMER()
    ON_WM_DESTROY()
    //}}AFX_MSG_MAP
    ON_OLEVERB(AFX_IDS_VERB_PROPERTIES, OnProperties)
END_MESSAGE_MAP()
BEGIN_DISPATCH_MAP(CActiveXDocControl, COleControl)
    //{{AFX_DISPATCH_MAP(CActiveXDocControl)
    //}}AFX_DISPATCH_MAP
END_DISPATCH_MAP()
BEGIN_EVENT_MAP(CActiveXDocControl, COleControl)
    //{{AFX_EVENT_MAP(COleFrameCtrl)
    //}}AFX_EVENT_MAP
END_EVENT_MAP()

int CActiveXDocControl::m_bDocInitialized = FALSE;
//==========================================================================================
CActiveXDocControl::CActiveXDocControl()
{
    m_pDocTemplate = NULL;
    m_pFrameWnd = NULL;

    // Since we're in an OCX, CWinApp::InitApplication() is
    // not called by the framework. Unfortunately, that method
    // performs CDocManager initialization that is necessary
    // in order for our DocTemplate to clean up after itself.
    // We simulate the same initialization by calling the
    // following code the first time we create a control.

    if (!m_bDocInitialized)
    {
        CDocManager docManager;
        docManager.AddDocTemplate(NULL);
        m_bDocInitialized = TRUE;
    }
}
//==========================================================================================
CActiveXDocControl::~CActiveXDocControl()
{
	try
	{
		// Note that the frame, the document, and the view are
		// all deleted automatically by the framework!

		delete m_pDocTemplate;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16446");
}
//==========================================================================================
void CActiveXDocControl::AddDocTemplate(CActiveXDocTemplate* pDocTemplate)
{
    // I've decided to call this function AddDocTemplate to
    // be consistent with naming of CWinApp::AddDocTemplate.
    // However, only one DocTemplate is allowed per control.
    
    ASSERT(pDocTemplate);
    ASSERT(m_pDocTemplate == NULL);
    m_pDocTemplate = pDocTemplate;
}
//==========================================================================================
int CActiveXDocControl::OnCreate(LPCREATESTRUCT lpCreateStruct) 
{
    if (COleControl::OnCreate(lpCreateStruct) == -1)
        return -1;

    // The CActiveXDocTemplate object will create the
    // document, the view, and the frame inside the
    // control. The reason we need a handle to the frame
    // is so that we can resize it when the control is
    // resized.
    
    ASSERT(m_pDocTemplate);    // Set in call to AddDocTemplate
    m_pFrameWnd = m_pDocTemplate->CreateDocViewFrame(this);
    ASSERT_KINDOF(CFrameWnd, m_pFrameWnd);

    // By default, we'll create the control with a border,
    // since it looks better for frames containing a toolbar.

    SetBorderStyle(TRUE);
    SetTimer(WM_IDLEUPDATECMDUI, 300, NULL);
    return 0;
}
//==========================================================================================
void CActiveXDocControl::OnSize(UINT nType, int cx, int cy) 
{
    COleControl::OnSize(nType, cx, cy);

    // The CFrameWnd should always fill up the entire client
    // area of the control.

    if (m_pFrameWnd != __nullptr)
    {
        ASSERT(IsWindow(*m_pFrameWnd));
        CRect area;
        GetClientRect(area);
        m_pFrameWnd->MoveWindow(area.left, area.top, area.right,
            area.bottom);
    }
}
//==========================================================================================
void CActiveXDocControl::OnTimer(UINT nIDEvent) 
{
    // Since we're in an OCX, we don't control the message loop,
    // so CWinThread::OnIdle is never called. That means we have
    // to periodically pump the ON_UPDATE_COMMAND_UI messages
    // by hand.
    
    SendMessageToDescendants(WM_IDLEUPDATECMDUI, TRUE);
    COleControl::OnTimer(nIDEvent);
}
//==========================================================================================
void CActiveXDocControl::OnDestroy() 
{
  	if (m_pFrameWnd->m_hWnd)
	{
		delete m_pFrameWnd;
		m_pFrameWnd = NULL;
	}

    m_pDocTemplate->SaveDocumentFile();
    COleControl::OnDestroy();
}
//==========================================================================================
