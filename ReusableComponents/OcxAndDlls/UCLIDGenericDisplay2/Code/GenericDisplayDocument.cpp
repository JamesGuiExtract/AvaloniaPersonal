//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	GenericDisplayDocument.cpp
//
// PURPOSE:	This is an implementation file for GenericDisplayDocument() class.
//			Where the GenericDisplayDocument() class has been derived from CDocument()
//			class.  The code written in this file makes it possible to
//			interact between the view, control and document.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================
// GenericDisplayDocument.cpp : implementation file
//

#include "stdafx.h"
#include "GenericDisplay.h"
#include "GenericDisplayDocument.h"
#include "CntrItem.h"
#include "UCLIDException.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CGenericDisplayDocument

IMPLEMENT_DYNCREATE(CGenericDisplayDocument, COleDocument)
//==========================================================================================
CGenericDisplayDocument::CGenericDisplayDocument()
{
}
//==========================================================================================
BOOL CGenericDisplayDocument::OnNewDocument()
{
	if (!COleDocument::OnNewDocument())
		return FALSE;
	return TRUE;
}
//==========================================================================================
CGenericDisplayDocument::~CGenericDisplayDocument()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16450");
}
//==========================================================================================

BEGIN_MESSAGE_MAP(CGenericDisplayDocument, COleDocument)
	//{{AFX_MSG_MAP(CGenericDisplayDocument)
		// NOTE - the ClassWizard will add and remove mapping macros here.
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CGenericDisplayDocument diagnostics
//==========================================================================================
#ifdef _DEBUG
void CGenericDisplayDocument::AssertValid() const
{
	COleDocument::AssertValid();
}
//==========================================================================================
void CGenericDisplayDocument::Dump(CDumpContext& dc) const
{
	COleDocument::Dump(dc);
}
#endif //_DEBUG
//==========================================================================================
/////////////////////////////////////////////////////////////////////////////
// CGenericDisplayDocument commands

COleClientItem* CGenericDisplayDocument::CreateClientItem(REOBJECT* preo) const
{
	// cast away constness of this
	return new CGenericDisplayCntrItem(preo, (CGenericDisplayDocument*)this);
}
//==========================================================================================
