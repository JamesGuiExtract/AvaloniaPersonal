//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	cntritem.cpp
//
// PURPOSE:	This is an implementation file for CMCRCntrItem() class.
//			Where the CMCRCntrItem() class has been derived from CRichEditCntrItem()
//			class.  The code written in this file makes it possible to
//			control the frame, view and document.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================
// cntritem.cpp : implementation of the CMCRCntrItem class

#include "stdafx.h"
#include "cntritem.h"

#include "MCRDocument.h"

#ifdef _DEBUG
#undef THIS_FILE
static char BASED_CODE THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CWordPadCntrItem implementation

IMPLEMENT_SERIAL(CMCRCntrItem, CRichEditCntrItem, 0)

CMCRCntrItem::CMCRCntrItem(REOBJECT *preo, CMCRDocument* pContainer)
	: CRichEditCntrItem(preo, pContainer)
{
}

/////////////////////////////////////////////////////////////////////////////
// CWordPadCntrItem diagnostics

#ifdef _DEBUG
void CMCRCntrItem::AssertValid() const
{
	CRichEditCntrItem::AssertValid();
}

void CMCRCntrItem::Dump(CDumpContext& dc) const
{
	CRichEditCntrItem::Dump(dc);
}
#endif

/////////////////////////////////////////////////////////////////////////////
