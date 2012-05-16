// SimpleMDIFrameworkView.cpp : implementation of the CSimpleMDIFrameworkView class
//

#include "stdafx.h"
#include "SimpleMDIFramework.h"

#include "SimpleMDIFrameworkDoc.h"
#include "SimpleMDIFrameworkView.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CSimpleMDIFrameworkView

IMPLEMENT_DYNCREATE(CSimpleMDIFrameworkView, CView)

BEGIN_MESSAGE_MAP(CSimpleMDIFrameworkView, CView)
	//{{AFX_MSG_MAP(CSimpleMDIFrameworkView)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	ON_COMMAND(ID_CANCEL_EDIT_SRVR, OnCancelEditSrvr)
	//}}AFX_MSG_MAP
	// Standard printing commands
	ON_COMMAND(ID_FILE_PRINT, CView::OnFilePrint)
	ON_COMMAND(ID_FILE_PRINT_DIRECT, CView::OnFilePrint)
	ON_COMMAND(ID_FILE_PRINT_PREVIEW, CView::OnFilePrintPreview)
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CSimpleMDIFrameworkView construction/destruction

CSimpleMDIFrameworkView::CSimpleMDIFrameworkView()
{
	// TODO: add construction code here

}

CSimpleMDIFrameworkView::~CSimpleMDIFrameworkView()
{
}

BOOL CSimpleMDIFrameworkView::PreCreateWindow(CREATESTRUCT& cs)
{
	// TODO: Modify the Window class or styles here by modifying
	//  the CREATESTRUCT cs

	return CView::PreCreateWindow(cs);
}

/////////////////////////////////////////////////////////////////////////////
// CSimpleMDIFrameworkView drawing

void CSimpleMDIFrameworkView::OnDraw(CDC* pDC)
{
	CSimpleMDIFrameworkDoc* pDoc = GetDocument();
	ASSERT_VALID(pDoc);
	// TODO: add draw code for native data here
}

/////////////////////////////////////////////////////////////////////////////
// CSimpleMDIFrameworkView printing

BOOL CSimpleMDIFrameworkView::OnPreparePrinting(CPrintInfo* pInfo)
{
	// default preparation
	return DoPreparePrinting(pInfo);
}

void CSimpleMDIFrameworkView::OnBeginPrinting(CDC* /*pDC*/, CPrintInfo* /*pInfo*/)
{
	// TODO: add extra initialization before printing
}

void CSimpleMDIFrameworkView::OnEndPrinting(CDC* /*pDC*/, CPrintInfo* /*pInfo*/)
{
	// TODO: add cleanup after printing
}

/////////////////////////////////////////////////////////////////////////////
// OLE Server support

// The following command handler provides the standard keyboard
//  user interface to cancel an in-place editing session.  Here,
//  the server (not the container) causes the deactivation.
void CSimpleMDIFrameworkView::OnCancelEditSrvr()
{
	GetDocument()->OnDeactivateUI(FALSE);
}

/////////////////////////////////////////////////////////////////////////////
// CSimpleMDIFrameworkView diagnostics

#ifdef _DEBUG
void CSimpleMDIFrameworkView::AssertValid() const
{
	CView::AssertValid();
}

void CSimpleMDIFrameworkView::Dump(CDumpContext& dc) const
{
	CView::Dump(dc);
}

CSimpleMDIFrameworkDoc* CSimpleMDIFrameworkView::GetDocument() // non-debug version is inline
{
	ASSERT(m_pDocument->IsKindOf(RUNTIME_CLASS(CSimpleMDIFrameworkDoc)));
	return (CSimpleMDIFrameworkDoc*)m_pDocument;
}
#endif //_DEBUG

/////////////////////////////////////////////////////////////////////////////
// CSimpleMDIFrameworkView message handlers
