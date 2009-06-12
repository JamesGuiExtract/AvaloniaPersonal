//===========================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	MCRTextViewerPpg.cpp
//
// PURPOSE:	This is an implementation file for CMCRTextViewerPropPage() class.
//			The CMCRTextViewerPropPage() class has been derived from the 
//			COlePropertyPage() class.
// NOTES:	
//
// AUTHORS:	
//
//===========================================================================
// MCRTextViewerPpg.cpp : Implementation of the CMCRTextViewerPropPage class.

#include "stdafx.h"
#include "MCRTextViewer.h"
#include "MCRTextViewerPpg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif


IMPLEMENT_DYNCREATE(CMCRTextViewerPropPage, COlePropertyPage)


/////////////////////////////////////////////////////////////////////////////
// Message map

BEGIN_MESSAGE_MAP(CMCRTextViewerPropPage, COlePropertyPage)
	//{{AFX_MSG_MAP(CMCRTextViewerPropPage)
	// NOTE - ClassWizard will add and remove message map entries
	//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()


/////////////////////////////////////////////////////////////////////////////
// Initialize class factory and guid

IMPLEMENT_OLECREATE_EX(CMCRTextViewerPropPage, "UCLIDMCRTEXTVIEWER.UCLIDMCRTextViewerPropPage.1",
	0x54d86573, 0x3d32, 0x4cae, 0xad, 0x7d, 0xe, 0x8, 0x73, 0x44, 0xed, 0x7a)


/////////////////////////////////////////////////////////////////////////////
// CMCRTextViewerPropPage::CMCRTextViewerPropPageFactory::UpdateRegistry -
// Adds or removes system registry entries for CMCRTextViewerPropPage

BOOL CMCRTextViewerPropPage::CMCRTextViewerPropPageFactory::UpdateRegistry(BOOL bRegister)
{
	if (bRegister)
		return AfxOleRegisterPropertyPageClass(AfxGetInstanceHandle(),
			m_clsid, IDS_UCLIDMCRTEXTVIEWER_PPG);
	else
		return AfxOleUnregisterClass(m_clsid, NULL);
}


/////////////////////////////////////////////////////////////////////////////
// CMCRTextViewerPropPage::CMCRTextViewerPropPage - Constructor

CMCRTextViewerPropPage::CMCRTextViewerPropPage() :
	COlePropertyPage(IDD, IDS_UCLIDMCRTEXTVIEWER_PPG_CAPTION)
{
	//{{AFX_DATA_INIT(CMCRTextViewerPropPage)
	// NOTE: ClassWizard will add member initialization here
	//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_DATA_INIT
}


/////////////////////////////////////////////////////////////////////////////
// CMCRTextViewerPropPage::DoDataExchange - Moves data between page and properties

void CMCRTextViewerPropPage::DoDataExchange(CDataExchange* pDX)
{
	//{{AFX_DATA_MAP(CMCRTextViewerPropPage)
	// NOTE: ClassWizard will add DDP, DDX, and DDV calls here
	//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_DATA_MAP
	DDP_PostProcessing(pDX);
}


/////////////////////////////////////////////////////////////////////////////
// CMCRTextViewerPropPage message handlers
