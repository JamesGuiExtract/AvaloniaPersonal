// SimpleMDIFrameworkDoc.cpp : implementation of the CSimpleMDIFrameworkDoc class
//

#include "stdafx.h"
#include "SimpleMDIFramework.h"

#include "SimpleMDIFrameworkDoc.h"
#include "SrvrItem.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CSimpleMDIFrameworkDoc

IMPLEMENT_DYNCREATE(CSimpleMDIFrameworkDoc, COleServerDoc)

BEGIN_MESSAGE_MAP(CSimpleMDIFrameworkDoc, COleServerDoc)
	//{{AFX_MSG_MAP(CSimpleMDIFrameworkDoc)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CSimpleMDIFrameworkDoc construction/destruction

CSimpleMDIFrameworkDoc::CSimpleMDIFrameworkDoc()
{
	// Use OLE compound files
	EnableCompoundFile();

	// TODO: add one-time construction code here

}

CSimpleMDIFrameworkDoc::~CSimpleMDIFrameworkDoc()
{
}

BOOL CSimpleMDIFrameworkDoc::OnNewDocument()
{
	if (!COleServerDoc::OnNewDocument())
		return FALSE;

	// TODO: add reinitialization code here
	// (SDI documents will reuse this document)

	return TRUE;
}

/////////////////////////////////////////////////////////////////////////////
// CSimpleMDIFrameworkDoc server implementation

COleServerItem* CSimpleMDIFrameworkDoc::OnGetEmbeddedItem()
{
	// OnGetEmbeddedItem is called by the framework to get the COleServerItem
	//  that is associated with the document.  It is only called when necessary.

	CSimpleMDIFrameworkSrvrItem* pItem = new CSimpleMDIFrameworkSrvrItem(this);
	ASSERT_VALID(pItem);
	return pItem;
}



/////////////////////////////////////////////////////////////////////////////
// CSimpleMDIFrameworkDoc serialization

void CSimpleMDIFrameworkDoc::Serialize(CArchive& ar)
{
	if (ar.IsStoring())
	{
		// TODO: add storing code here
	}
	else
	{
		// TODO: add loading code here
	}
}

/////////////////////////////////////////////////////////////////////////////
// CSimpleMDIFrameworkDoc diagnostics

#ifdef _DEBUG
void CSimpleMDIFrameworkDoc::AssertValid() const
{
	COleServerDoc::AssertValid();
}

void CSimpleMDIFrameworkDoc::Dump(CDumpContext& dc) const
{
	COleServerDoc::Dump(dc);
}
#endif //_DEBUG

/////////////////////////////////////////////////////////////////////////////
// CSimpleMDIFrameworkDoc commands
