//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	MCRDocument.cpp
//
// PURPOSE:	This is an implementation file for CMCRDocument() class.
//			Where the CMCRDocument() class has been derived from CRichEditDoc()
//			class.  The code written in this file makes it possible to
//			interact between the view, control and document.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================
// MCRDocument.cpp : implementation file
//

#include "stdafx.h"
#include "MCRDocument.h"
#include "CntrItem.h"

#include <UCLIDException.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif


/////////////////////////////////////////////////////////////////////////////
// CMCRDocument

IMPLEMENT_DYNCREATE(CMCRDocument, CRichEditDoc)

CMCRDocument::CMCRDocument()
{

}

BOOL CMCRDocument::OnNewDocument()
{
	if (!CRichEditDoc::OnNewDocument())
		return FALSE;
	return TRUE;
}

CMCRDocument::~CMCRDocument()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20402");
}

BEGIN_MESSAGE_MAP(CMCRDocument, CRichEditDoc)
	//{{AFX_MSG_MAP(CMCRDocument)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CMCRDocument diagnostics

#ifdef _DEBUG
void CMCRDocument::AssertValid() const
{
	CDocument::AssertValid();
}

void CMCRDocument::Dump(CDumpContext& dc) const
{
	CDocument::Dump(dc);
}
#endif //_DEBUG

/////////////////////////////////////////////////////////////////////////////
// CMCRDocument serialization

CRichEditCntrItem* CMCRDocument::CreateClientItem(REOBJECT* preo) const
{
	
	// cast away constness of this
	return new CMCRCntrItem(preo, (CMCRDocument*)this);
}

/////////////////////////////////////////////////////////////////////////////
// CMCRDocument commands