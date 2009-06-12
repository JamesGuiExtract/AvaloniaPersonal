//==================================================================================================
//
// COPYRIGHT (c) 2000 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	GenericDisplayPpg.cpp
//
// PURPOSE:	This is an implementation file for CGenericDisplayPpg() class.
//			Where the CGenericDisplayPpg() class has been derived from COlePropertyPage()
//			class.
// NOTES:	
//
// AUTHORS:	
//
//==================================================================================================
// GenericDisplayPpg.cpp : Implementation of the CGenericDisplayPropPage property page class.

#include "stdafx.h"
#include "GenericDisplay.h"
#include "GenericDisplayPpg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif


IMPLEMENT_DYNCREATE(CGenericDisplayPropPage, COlePropertyPage)


/////////////////////////////////////////////////////////////////////////////
// Message map

BEGIN_MESSAGE_MAP(CGenericDisplayPropPage, COlePropertyPage)
	//{{AFX_MSG_MAP(CGenericDisplayPropPage)
	// NOTE - ClassWizard will add and remove message map entries
	//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()


/////////////////////////////////////////////////////////////////////////////
// Initialize class factory and guid

IMPLEMENT_OLECREATE_EX(CGenericDisplayPropPage, "GENERICDISPLAY.GenericDisplayPropPage.1",
	0x14981577, 0x9117, 0x11d4, 0x97, 0x25, 0, 0x80, 0x48, 0xfb, 0xc9, 0x6e)

//==========================================================================================
/////////////////////////////////////////////////////////////////////////////
// CGenericDisplayPropPage::CGenericDisplayPropPageFactory::UpdateRegistry -
// Adds or removes system registry entries for CGenericDisplayPropPage

BOOL CGenericDisplayPropPage::CGenericDisplayPropPageFactory::UpdateRegistry(BOOL bRegister)
{
	if (bRegister)
		return AfxOleRegisterPropertyPageClass(AfxGetInstanceHandle(),
			m_clsid, IDS_GENERICDISPLAY_PPG);
	else
		return AfxOleUnregisterClass(m_clsid, NULL);
}
//==========================================================================================

/////////////////////////////////////////////////////////////////////////////
// CGenericDisplayPropPage::CGenericDisplayPropPage - Constructor

CGenericDisplayPropPage::CGenericDisplayPropPage() :
	COlePropertyPage(IDD, IDS_GENERICDISPLAY_PPG_CAPTION)
{
	//{{AFX_DATA_INIT(CGenericDisplayPropPage)
	// NOTE: ClassWizard will add member initialization here
	//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_DATA_INIT
}

//==========================================================================================
/////////////////////////////////////////////////////////////////////////////
// CGenericDisplayPropPage::DoDataExchange - Moves data between page and properties

void CGenericDisplayPropPage::DoDataExchange(CDataExchange* pDX)
{
	//{{AFX_DATA_MAP(CGenericDisplayPropPage)
	// NOTE: ClassWizard will add DDP, DDX, and DDV calls here
	//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_DATA_MAP
	DDP_PostProcessing(pDX);
}

//==========================================================================================
/////////////////////////////////////////////////////////////////////////////
// CGenericDisplayPropPage message handlers
