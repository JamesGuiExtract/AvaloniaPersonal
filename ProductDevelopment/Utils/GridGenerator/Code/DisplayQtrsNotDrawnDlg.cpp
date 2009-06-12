// DisplayQtrsNotDrawnDlg.cpp : implementation file
//

#include "stdafx.h"
#include "DisplayQtrsNotDrawnDlg.h"


//-------------------------------------------------------------------------------------------------
// DisplayQtrsNotDrawnDlg dialog
//-------------------------------------------------------------------------------------------------

IMPLEMENT_DYNAMIC(DisplayQtrsNotDrawnDlg, CDialog)

DisplayQtrsNotDrawnDlg::DisplayQtrsNotDrawnDlg(CWnd* pParent /*=NULL*/)
	: CDialog(DisplayQtrsNotDrawnDlg::IDD, pParent)
	, m_zQtrsNotDrawn(_T(""))
{

}
//-------------------------------------------------------------------------------------------------
DisplayQtrsNotDrawnDlg::~DisplayQtrsNotDrawnDlg()
{
}
//-------------------------------------------------------------------------------------------------
void DisplayQtrsNotDrawnDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Text(pDX, IDC_EDIT_QUARTERS, m_zQtrsNotDrawn);
}
//-------------------------------------------------------------------------------------------------


BEGIN_MESSAGE_MAP(DisplayQtrsNotDrawnDlg, CDialog)
END_MESSAGE_MAP()


// DisplayQtrsNotDrawnDlg message handlers

//-------------------------------------------------------------------------------------------------
// Public Functions
//-------------------------------------------------------------------------------------------------
void DisplayQtrsNotDrawnDlg::setQuartersNotDrawnText( std::string strQuartersNotDrawn )

{
	m_zQtrsNotDrawn = strQuartersNotDrawn.c_str();
}
