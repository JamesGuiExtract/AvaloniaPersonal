//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	ImageEditPpg.cpp
//
// PURPOSE:	This is an implementation file for CImageEditPropPage() class.
//			Where the CImageEditPropPage() class has been derived from COlePropertyPage() class.
//			The code written in this file makes it possible to implement the various
//			application methods in the user interface.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================
// ImageEditPpg.cpp : Implementation of the CImageEditPropPage property page class.
//
#include "stdafx.h"
#include "ImageEdit.h"
#include "ImageEditPpg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif


IMPLEMENT_DYNCREATE(CImageEditPropPage, COlePropertyPage)


/////////////////////////////////////////////////////////////////////////////
// Message map

BEGIN_MESSAGE_MAP(CImageEditPropPage, COlePropertyPage)
	//{{AFX_MSG_MAP(CImageEditPropPage)
	// NOTE - ClassWizard will add and remove message map entries
	//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()


/////////////////////////////////////////////////////////////////////////////
// Initialize class factory and guid

IMPLEMENT_OLECREATE_EX(CImageEditPropPage, "IMAGEEDIT.ImageEditPropPage.1",
	0x300a09ef, 0x869b, 0x47a8, 0x9f, 0xa7, 0xfb, 0xe, 0x4b, 0xa8, 0x1f, 0x7b)


/////////////////////////////////////////////////////////////////////////////
// CImageEditPropPage::CImageEditPropPageFactory::UpdateRegistry -
// Adds or removes system registry entries for CImageEditPropPage

BOOL CImageEditPropPage::CImageEditPropPageFactory::UpdateRegistry(BOOL bRegister)
{
	if (bRegister)
		return AfxOleRegisterPropertyPageClass(AfxGetInstanceHandle(),
			m_clsid, IDS_IMAGEEDIT_PPG);
	else
		return AfxOleUnregisterClass(m_clsid, NULL);
}
//==================================================================================================

/////////////////////////////////////////////////////////////////////////////
// CImageEditPropPage::CImageEditPropPage - Constructor

CImageEditPropPage::CImageEditPropPage() :
	COlePropertyPage(IDD, IDS_IMAGEEDIT_PPG_CAPTION)
{
	//{{AFX_DATA_INIT(CImageEditPropPage)
	// NOTE: ClassWizard will add member initialization here
	//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_DATA_INIT
}
//==================================================================================================

/////////////////////////////////////////////////////////////////////////////
// CImageEditPropPage::DoDataExchange - Moves data between page and properties

void CImageEditPropPage::DoDataExchange(CDataExchange* pDX)
{
	//{{AFX_DATA_MAP(CImageEditPropPage)
	// NOTE: ClassWizard will add DDP, DDX, and DDV calls here
	//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_DATA_MAP
	DDP_PostProcessing(pDX);
}
//==================================================================================================

/////////////////////////////////////////////////////////////////////////////
// CImageEditPropPage message handlers
