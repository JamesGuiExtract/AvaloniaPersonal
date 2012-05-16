//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	AboutIcoMapDlg.cpp
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS:	Duan Wang
//
//==================================================================================================

#include "stdafx.h"
#include "AboutIcoMapDlg.h"
#include "icomapapp.h"


#include <IcoMapOptions.h>
#include <cpputil.h>
#include <UCLIDException.h>
#include <LicenseMgmt.h>

#include <string>
using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// AboutIcoMapDlg dialog


AboutIcoMapDlg::AboutIcoMapDlg(CWnd* pParent /*=NULL*/)
	: CDialog(AboutIcoMapDlg::IDD, pParent)
{
	//{{AFX_DATA_INIT(AboutIcoMapDlg)
	m_zProductVersion = _T("");
	//}}AFX_DATA_INIT
}


void AboutIcoMapDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(AboutIcoMapDlg)
	DDX_Text(pDX, IDC_EDIT_VERSION, m_zProductVersion);
	//}}AFX_DATA_MAP
}


BEGIN_MESSAGE_MAP(AboutIcoMapDlg, CDialog)
	//{{AFX_MSG_MAP(AboutIcoMapDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// AboutIcoMapDlg message handlers

BOOL AboutIcoMapDlg::OnInitDialog() 
{
	CDialog::OnInitDialog();
		
	try
	{
		// disable the pen-input-module-is-enabled checkbox
		// NOTE: we are making this call right away before any of the following code, as 
		// the following code may throw exceptions, and we want this checkbox to be 
		// disabled.

		// update the version #
		try
		{
			m_zProductVersion = IcoMapOptions::sGetInstance().getProductVersion().c_str();
		}
		catch (...)
		{
			m_zProductVersion = "Not Available";
		}

		// flush the updated data to the screen
		// NOTE: we are doing the updatedata here, and not inside the try block because
		// sometimes some of the edit box's values are available, but an exception gets thrown
		// when we try to query the value for some of the other edit-boxes.  Putting the updatedata
		// call here causes all available data to be shown
		UpdateData(FALSE);
		
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07467")

	return TRUE;  // return TRUE unless you set the focus to a control
	// EXCEPTION: OCX Property Pages should return FALSE	
}
