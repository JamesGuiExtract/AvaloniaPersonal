// FontSizeDistributionDlg.cpp : implementation file
//

#include "stdafx.h"
#include "spatialstringviewer.h"
#include "FontSizeDistributionDlg.h"
#include <map>
#include <CPPLetter.h>
#include <cpputil.h>
#include <UCLIDException.h>
using std::map;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const int giNUM_COLS = 3;

//-------------------------------------------------------------------------------------------------
// FontSizeDistributionDlg dialog
//-------------------------------------------------------------------------------------------------
FontSizeDistributionDlg::FontSizeDistributionDlg(CSpatialStringViewerDlg* pSSVDlg, 
												 SpatialStringViewerCfg* pCfgDlg, CWnd* pParent)
: CDialog(FontSizeDistributionDlg::IDD, pParent),
  m_pSSVDlg(pSSVDlg),
  m_pCfgDlg(pCfgDlg)
{
	//{{AFX_DATA_INIT(FontSizeDistributionDlg)
		// NOTE: the ClassWizard will add member initialization here
	//}}AFX_DATA_INIT
}
//-------------------------------------------------------------------------------------------------
void FontSizeDistributionDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(FontSizeDistributionDlg)
	DDX_Control(pDX, IDC_TEXT_NUMCHARS, m_staticNumChars);
	DDX_Control(pDX, IDC_LIST_FONT_SIZE_DIST, m_listFontSizeDistribution);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(FontSizeDistributionDlg, CDialog)
	//{{AFX_MSG_MAP(FontSizeDistributionDlg)
	ON_WM_SHOWWINDOW()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()
//-------------------------------------------------------------------------------------------------
// FontSizeDistributionDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL FontSizeDistributionDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		CDialog::OnInitDialog();
		
		// TODO: Add extra initialization here

		// Add the appropriate columns to the list control
		m_listFontSizeDistribution.InsertColumn(0, "Font Size");
		m_listFontSizeDistribution.InsertColumn(1, "Num Chars");
		m_listFontSizeDistribution.InsertColumn(2, "Percentage");

		// resize the controls appropriately
		RECT listRect;
		m_listFontSizeDistribution.GetWindowRect(&listRect);
		long nWidth = listRect.right - listRect.left;

		// need to make room for the column separators
		// subtract (num_cols-1) * 2 pixels
		nWidth -= (giNUM_COLS-1) * 2;

		nWidth /= 3;

		m_listFontSizeDistribution.SetColumnWidth(0, nWidth);
		m_listFontSizeDistribution.SetColumnWidth(1, nWidth);
		m_listFontSizeDistribution.SetColumnWidth(2, nWidth);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI18608");

	// add an item for each font size to the list
	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//-------------------------------------------------------------------------------------------------
void FontSizeDistributionDlg::OnShowWindow(BOOL bShow, UINT nStatus) 
{
	CDialog::OnShowWindow(bShow, nStatus);
	
	// TODO: Add your message handler code here
	if(bShow == TRUE)	
	{
		refreshDistribution();
	}
	
}
//-------------------------------------------------------------------------------------------------
// Private Member Functions
//-------------------------------------------------------------------------------------------------
void FontSizeDistributionDlg::refreshDistribution()
{
	// Get the letters from the spatial string
	ISpatialStringPtr ipSS = m_pSSVDlg->getSpatialString();

	m_listFontSizeDistribution.DeleteAllItems();

	if(ipSS->GetMode() != kSpatialMode)
	{
		return;
	}

	long nNumLetters = ipSS->Size;
	CString zNumChars;
	zNumChars.Format("Total Number of Characters: %d", nNumLetters);
	m_staticNumChars.SetWindowText(zNumChars);


	ILongToLongMapPtr ipMap = ipSS->GetFontSizeDistribution();
	long nSize = ipMap->Size;

	int i;
	for(i = 0; i < nSize; i++)
	{
		long nFontSize, nNumFontChars;
		ipMap->GetKeyValue(i, &nFontSize, &nNumFontChars);
		CString zFontSize;
		zFontSize.Format("%2d pt", nFontSize);
		long nIndex = m_listFontSizeDistribution.InsertItem(0, zFontSize);
		m_listFontSizeDistribution.SetItemText( nIndex, 1, asString(nNumFontChars).c_str());
		double dPercent = (double)nNumFontChars / (double)nNumLetters * 100.0;
		CString zPercent;
		zPercent.Format("%.2f %%", dPercent);
		m_listFontSizeDistribution.SetItemText( nIndex, 2, zPercent);
	}
}
