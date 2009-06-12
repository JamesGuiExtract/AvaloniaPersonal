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
// cntritem.cpp : implementation of the CGenericDisplayCntrItem class

#include "stdafx.h"
#include "GenericDisplay.h"
#include "GenericDisplayDocument.h"
#include "GenericDisplayView.h"
#include "cntritem.h"

#ifdef _DEBUG
#undef THIS_FILE
static char BASED_CODE THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CGenericDisplayCntrItem implementation

IMPLEMENT_SERIAL(CGenericDisplayCntrItem, COleClientItem, 0)
//==========================================================================================
CGenericDisplayCntrItem::CGenericDisplayCntrItem(REOBJECT *preo, CGenericDisplayDocument* pContainer)
	: COleClientItem(/*preo,*/ pContainer)
{
}
//==========================================================================================
/////////////////////////////////////////////////////////////////////////////
// CGenericDisplayCntrItem diagnostics

#ifdef _DEBUG
void CGenericDisplayCntrItem::AssertValid() const
{
	COleClientItem::AssertValid();
}
//==========================================================================================
void CGenericDisplayCntrItem::Dump(CDumpContext& dc) const
{
	COleClientItem::Dump(dc);
}
#endif
//==========================================================================================
/////////////////////////////////////////////////////////////////////////////
