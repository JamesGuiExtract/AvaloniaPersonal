// WordLengthDistributionDlg.cpp : implementation file
//
#include "stdafx.h"
#include "spatialstringviewer.h"
#include "WordLengthDistributionDlg.h"

#include <cpputil.h>
#include <UCLIDException.h>

#include <map>
using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const int giLENGTH_COL = 0;
const int giCOUNT_COL = 1;
const int giPERCENT_COL = 2;

// total number of columns (NOTE: If you add columns, 
// you may want to increase the width of the dialog)
const int giNUM_COLS = 3;

//-------------------------------------------------------------------------------------------------
// WordLengthDistributionDlg dialog
//-------------------------------------------------------------------------------------------------
// need to initialize m_nStart and m_nEnd to -1 so that the data will be updated on
// the initial call to refreshDistribution
WordLengthDistributionDlg::WordLengthDistributionDlg(CSpatialStringViewerDlg* pSSVDlg, 
													SpatialStringViewerCfg* pCfgDlg,
													 CWnd* pParent)
: CDialog(WordLengthDistributionDlg::IDD, pParent),
  m_nStart(-1),
  m_nEnd(-1),
  m_lDefaultPercentWidth(0),
  m_pCfgDlg(pCfgDlg),
  m_pSSVDlg(pSSVDlg)
{
	//{{AFX_DATA_INIT(WordLengthDistributionDlg)
		// NOTE: the ClassWizard will add member initialization here
	//}}AFX_DATA_INIT
}
//-------------------------------------------------------------------------------------------------
void WordLengthDistributionDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(WordLengthDistributionDlg)
	DDX_Control(pDX, IDC_TEXT_NUMWORDS, m_staticNumWords);
	DDX_Control(pDX, IDC_LIST_WORD_LENGTH_DIST, m_listWordLengthDistribution);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(WordLengthDistributionDlg, CDialog)
	//{{AFX_MSG_MAP(WordLengthDistributionDlg)
	ON_WM_SHOWWINDOW()
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// WordLengthDistributionDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL WordLengthDistributionDlg::OnInitDialog() 
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		CDialog::OnInitDialog();
		
		// restore last window position
		int left, top;
		m_pCfgDlg->getLastDistributionWindowPos(left, top);
		SetWindowPos(NULL, left, top, 0, 0, SWP_NOSIZE | SWP_NOZORDER);

		// Add the appropriate columns to the list control
		m_listWordLengthDistribution.InsertColumn(giLENGTH_COL, "Word Length");
		m_listWordLengthDistribution.InsertColumn(giCOUNT_COL, "Count");
		m_listWordLengthDistribution.InsertColumn(giPERCENT_COL, "Percentage");

		// resize the controls appropriately
		RECT listRect;
		m_listWordLengthDistribution.GetWindowRect(&listRect);

		// compute total width for control (subtract off (ncols-1)*2 pixels
		// to make room for the column separators)
		long nWidth = (listRect.right - listRect.left) - ((giNUM_COLS-1) * 2);

		// set the default column width to nWidth/nCols - nCols
		long nColWidth = (nWidth / giNUM_COLS) - giNUM_COLS;

		// set columns to nColWidth
		m_listWordLengthDistribution.SetColumnWidth(giLENGTH_COL, nColWidth);
		m_listWordLengthDistribution.SetColumnWidth(giCOUNT_COL, nColWidth);

		// the percent column should be all remaining pixels (we allow this column
		// to be wider since it may have to shrink to allow for a scroll bar)
		m_lDefaultPercentWidth = nWidth - (2 * nColWidth);
		m_listWordLengthDistribution.SetColumnWidth(giPERCENT_COL, m_lDefaultPercentWidth);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20632");

	// add an item for each font size to the list
	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//-------------------------------------------------------------------------------------------------
void WordLengthDistributionDlg::OnShowWindow(BOOL bShow, UINT nStatus) 
{
	AFX_MANAGE_STATE(AfxGetModuleState());

	try
	{
		CDialog::OnShowWindow(bShow, nStatus);
		
		if(bShow == TRUE)	
		{
			// get the edit control from the spatial string dialog
			CEdit* pEditCtl = (CEdit*) m_pSSVDlg->GetDlgItem(IDC_EDIT_TEXT); 
			ASSERT_RESOURCE_ALLOCATION("ELI20635", pEditCtl != __nullptr);

			// get the start and end points of the selection
			int nStart(0), nEnd(0);
			pEditCtl->GetSel(nStart, nEnd);

			refreshDistribution(nStart, nEnd);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20633");
}

//-------------------------------------------------------------------------------------------------
// Public Member Functions
//-------------------------------------------------------------------------------------------------
void WordLengthDistributionDlg::refreshDistribution(int nStart, int nEnd)
{
	// get the spatial string from the dialog box
	ISpatialStringPtr ipSS = m_pSSVDlg->getSpatialString();
	ASSERT_RESOURCE_ALLOCATION("ELI20634", ipSS != __nullptr);

	// only refresh data if either the start or end value has changed
	if ((nStart != m_nStart) || (nEnd != m_nEnd))
	{
		m_nStart = nStart;
		m_nEnd = nEnd;

		// make sure the percent column is reset to default width
		m_listWordLengthDistribution.SetColumnWidth(giPERCENT_COL, m_lDefaultPercentWidth);

		// check for selection
		if ((abs(nStart-nEnd) >= 1))
		{
			// since we have a selection, set our string to be a substring
			ipSS = ipSS->GetSubString(nStart, nEnd-1);
			ASSERT_RESOURCE_ALLOCATION("ELI20636", ipSS != __nullptr);
		}
		
		m_listWordLengthDistribution.DeleteAllItems();

		// get the word length distribution and the total number of words
		long nNumWords;
		ILongToLongMapPtr ipMap = ipSS->GetWordLengthDist(&nNumWords);
		ASSERT_RESOURCE_ALLOCATION("ELI20637", ipMap != __nullptr);

		CString zNumWords;
		zNumWords.Format("Total Number of Words: %d", nNumWords);
		m_staticNumWords.SetWindowText(zNumWords);

		long nSize = ipMap->Size;
		for(int i = 0; i < nSize; i++)
		{
			// get the data from the map
			long nWordLength, nWordCount;
			ipMap->GetKeyValue(i, &nWordLength, &nWordCount);

			// insert the word length
			long nIndex = m_listWordLengthDistribution.InsertItem(nWordLength, 
				asString(nWordLength).c_str());

			// insert the word count
			m_listWordLengthDistribution.SetItemText( nIndex, giCOUNT_COL, 
				asString(nWordCount).c_str());

			// insert the percentage
			double dPercent = (double)nWordCount / (double)nNumWords * 100.0;
			CString zPercent;
			zPercent.Format("%.2f %%", dPercent);
			m_listWordLengthDistribution.SetItemText( nIndex, giPERCENT_COL, zPercent);
		}

		// if the scroll limit for vertical bar > 0 then it is displayed
		// need to adjust the list to make room for the scroll bar
		if (m_listWordLengthDistribution.GetScrollLimit(SB_VERT) > 0)
		{
			// get the width of the scroll bar
			int nVScrollWidth = GetSystemMetrics(SM_CXVSCROLL);

			// compute the new column width to account for the scroll bar
			int nColumnWidth = m_listWordLengthDistribution.GetColumnWidth(giPERCENT_COL);
			m_listWordLengthDistribution.SetColumnWidth(giPERCENT_COL, 
				(nColumnWidth - nVScrollWidth));
		}
	}
}
//-------------------------------------------------------------------------------------------------
void WordLengthDistributionDlg::resetAndRefresh()
{
	// set the start and end values to initial defaults
	m_nStart = -1;
	m_nEnd = -1;

	// refresh the distribution list
	refreshDistribution(0,0);
}
//-------------------------------------------------------------------------------------------------
