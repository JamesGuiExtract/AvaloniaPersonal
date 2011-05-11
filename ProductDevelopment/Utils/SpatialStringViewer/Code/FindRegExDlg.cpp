// FindRegExDlg.cpp : implementation file
//

#include "stdafx.h"
#include "FindRegExDlg.h"

#include "spatialstringviewer.h"
#include "SpatialStringViewerCfg.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <StringTokenizer.h>
#include <RegExLoader.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Local helper functions
//-------------------------------------------------------------------------------------------------
bool sortFoundPosFunction(const pair<size_t, size_t>& a, const pair<size_t, size_t>& b)
{
	if (a.first == b.first)
	{
		return a.second <= b.second;
	}
	
	return a.first < b.first;
}

//-------------------------------------------------------------------------------------------------
// FindRegExDlg dialog
//-------------------------------------------------------------------------------------------------
FindRegExDlg::FindRegExDlg(CSpatialStringViewerDlg* pSSVDlg,
						   SpatialStringViewerCfg* pCfgDlg,
						   CWnd* pParent /*=NULL*/)
: CDialog(FindRegExDlg::IDD, pParent),
  m_pSSVDlg(pSSVDlg),
  m_pCfgDlg(pCfgDlg),
  m_ipRegExpr(NULL),
  m_bCaseSensitive(FALSE),
  m_zRangeFrom(""),
  m_zRangeTo(""),
  m_nSearchPos(1),
  m_zPatterns(""),
  m_bSearchStarted(false),
  m_strSearchText(""),
  m_nFontMin(-1),
  m_nFontMax(-1),
  m_nDlgHeight(0)
{
}
//-------------------------------------------------------------------------------------------------
FindRegExDlg::~FindRegExDlg()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16492");
}
//-------------------------------------------------------------------------------------------------
void FindRegExDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(FindRegExDlg)
	DDX_Control(pDX, IDC_CHK_RANGE, m_chkRange);
	DDX_Control(pDX, IDC_CHK_FONTSIZERANGE, m_chkFontSizeRange);
	DDX_Control(pDX, IDC_EDIT_FONTSIZETO, m_editToFontSize);
	DDX_Control(pDX, IDC_EDIT_FONTSIZEFROM, m_editFromFontSize);
	DDX_Control(pDX, IDC_CMB_FONTSIZEINCLUDE, m_cmbIncludeFontSize);
	DDX_Control(pDX, IDC_EDIT_EXPRS, m_editPatterns);
	DDX_Control(pDX, IDC_BTN_ADVANCE, m_btnAdvance);
	DDX_Control(pDX, IDC_EDIT_TO, m_editRangeTo);
	DDX_Control(pDX, IDC_EDIT_FROM, m_editRangeFrom);
	DDX_Control(pDX, IDC_BTN_FIND, m_btnFind);
	DDX_Control(pDX, IDC_CHK_AS_REGEX, m_chkAsRegEx);
	DDX_Control(pDX, IDC_RADIO_BEGIN, m_radBegin);
	DDX_Control(pDX, IDC_RADIO_CUR_POS, m_radCurPos);
	DDX_Control(pDX, IDC_FIND_PREVIOUS, m_btnPrevious);
	DDX_Control(pDX, IDC_FIND_RESET_FIND, m_btnResetSearch);
	DDX_Check(pDX, IDC_CHK_CASE_SENSITIVE, m_bCaseSensitive);
	DDX_Text(pDX, IDC_EDIT_FROM, m_zRangeFrom);
	DDX_Text(pDX, IDC_EDIT_TO, m_zRangeTo);
	DDX_Radio(pDX, IDC_RADIO_BEGIN, m_nSearchPos);
	DDX_Text(pDX, IDC_EDIT_EXPRS, m_zPatterns);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(FindRegExDlg, CDialog)
	//{{AFX_MSG_MAP(FindRegExDlg)
	ON_BN_CLICKED(IDC_BTN_FIND, OnBtnFind)
	ON_BN_CLICKED(IDC_FIND_PREVIOUS, OnBtnPrevious)
	ON_BN_CLICKED(IDC_FIND_RESET_FIND, OnBtnResetFind)
	ON_BN_CLICKED(IDC_BTN_ADVANCE, OnBtnAdvance)
	ON_WM_SHOWWINDOW()
	ON_BN_CLICKED(IDC_CHK_RANGE, OnChkRange)
	ON_BN_CLICKED(IDC_CHK_FONTSIZERANGE, OnChkFontsizerange)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// FindRegExDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL FindRegExDlg::OnInitDialog() 
{
	CDialog::OnInitDialog();
	
	try
	{
		
		m_editRangeFrom.EnableWindow(FALSE);
		m_editRangeTo.EnableWindow(FALSE);

		// Default is regular expression
		m_chkAsRegEx.SetCheck(BST_CHECKED);

		// if Advanced needs to be shown
		showDetailSettings(m_pCfgDlg->isAdvancedShown());
		int left, top;
		m_pCfgDlg->getLastFindWindowPos(left, top);
		SetWindowPos(NULL, left, top, 0, 0, SWP_NOSIZE | SWP_NOZORDER);

		m_editPatterns.SetWindowText(m_pCfgDlg->getLastRegularExpression().c_str());
		m_editPatterns.SetFocus();

		m_chkFontSizeRange.SetCheck(0);
		OnChkRange();

		m_chkFontSizeRange.SetCheck(0);
		OnChkFontsizerange();

		m_cmbIncludeFontSize.AddString("in");
		m_cmbIncludeFontSize.AddString("not in");
		m_cmbIncludeFontSize.SetCurSel(0);

		m_btnResetSearch.EnableWindow(FALSE);
		m_btnPrevious.EnableWindow(FALSE);
		
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07402");
	
	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//-------------------------------------------------------------------------------------------------
void FindRegExDlg::OnBtnFind() 
{
	try
	{
		if (!m_bSearchStarted)
		{
			CWaitCursor wait;

			UpdateData();

			if (!validateFromToValue())
			{
				return;
			}

			// populate the vec of patterns
			readPatterns((LPCTSTR)m_zPatterns);

			m_strSearchText = calculateSearchText();
			m_currentSearchPos = 0;
			m_vecMatches.clear();

			m_bSearchStarted = true;
			computeMatches();

			// If search is starting from current cursor, get cursor position and select first
			// match that contains or is past the cursor position
			if (!m_vecMatches.empty() && m_nSearchPos == 1)
			{
				size_t cursorPos = (size_t) m_pSSVDlg->getCurrentCursorPosition();
				for(; m_currentSearchPos < m_vecMatches.size(); m_currentSearchPos++)
				{
					pair<size_t, size_t>& pos = m_vecMatches[m_currentSearchPos];
					if (cursorPos < pos.first || (cursorPos > pos.first && cursorPos <= pos.second))
					{
						break;
					}
				}
			}

			m_btnFind.SetWindowText("&Next");
			m_btnResetSearch.EnableWindow(TRUE);
		}


		if (!m_vecMatches.empty() && m_currentSearchPos < m_vecMatches.size())
		{
			pair<size_t, size_t>& pos = m_vecMatches[m_currentSearchPos++];
			m_pSSVDlg->selectText(pos.first, pos.second+1);
		}
		else
		{
			MessageBox("No more matches.", "No Matches");
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07405");
}
//-------------------------------------------------------------------------------------------------
void FindRegExDlg::OnBtnPrevious()
{
}
//-------------------------------------------------------------------------------------------------
void FindRegExDlg::OnBtnResetFind()
{
	try
	{
		m_bSearchStarted = false;
		m_btnFind.SetWindowText("&Find");
		m_btnResetSearch.EnableWindow(FALSE);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI32563");
}
//-------------------------------------------------------------------------------------------------
void FindRegExDlg::OnBtnAdvance() 
{
	try
	{
		showDetailSettings(!m_bShowAdvanced);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07411");
}
//-------------------------------------------------------------------------------------------------
void FindRegExDlg::OnChkRange() 
{
	try
	{
		if(m_chkRange.GetCheck() == 1)
		{
			m_editRangeFrom.EnableWindow(TRUE);
			m_editRangeTo.EnableWindow(TRUE);	
		}
		else
		{
			m_editRangeFrom.EnableWindow(FALSE);
			m_editRangeTo.EnableWindow(FALSE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07403");
}
//-------------------------------------------------------------------------------------------------
void FindRegExDlg::OnChkFontsizerange() 
{
	// TODO: Add your control notification handler code here
	try
	{
		if(m_chkFontSizeRange.GetCheck() == 1)
		{
			m_editFromFontSize.EnableWindow(TRUE);
			m_editToFontSize.EnableWindow(TRUE);
			m_cmbIncludeFontSize.EnableWindow(TRUE);
		}
		else
		{
			m_editFromFontSize.EnableWindow(FALSE);
			m_editToFontSize.EnableWindow(FALSE);
			m_cmbIncludeFontSize.EnableWindow(FALSE);
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10622");
}
//-------------------------------------------------------------------------------------------------
BOOL FindRegExDlg::DestroyWindow() 
{
	// TODO: Add your specialized code here and/or call the base class
	m_pCfgDlg->showAdvanced(m_bShowAdvanced);

	return CDialog::DestroyWindow();
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
string FindRegExDlg::calculateSearchText()
{
	// get the entire text first
	string strInputText = m_pSSVDlg->getEntireDocumentText();
	size_t inputLength = strInputText.length();

	auto startSearchPos = getSearchStartPosition(inputLength);
	auto endSearchPos = getSearchEndPosition(inputLength);

	// Offset is the starting position for the search
	m_nOffSet = startSearchPos;

	// if start position already passes the end position, return empty string
	if (endSearchPos <= startSearchPos)
	{
		return "";
	}

	// get the input text
	string strInputWithinRange = strInputText.substr(startSearchPos,
		endSearchPos-startSearchPos + 1);

	return strInputWithinRange;
}
//-------------------------------------------------------------------------------------------------
void FindRegExDlg::computeMatches()
{
	// if no patterns specified, return false
	if (m_vecPatterns.empty())
	{
		return;
	}

	auto parser = getRegExParser();
	parser->IgnoreCase = asVariantBool(!m_bCaseSensitive);

	// If font size needs to be checked then ipTemp != __nullptr
	ISpatialStringPtr ipTemp = __nullptr;
	if (m_chkFontSizeRange.GetCheck() == BST_CHECKED)
	{
		ipTemp = m_pSSVDlg->getSpatialString();
		ASSERT_RESOURCE_ALLOCATION("ELI15636", ipTemp != __nullptr);
	}

	for (auto it = m_vecPatterns.begin(); it != m_vecPatterns.end(); it++)
	{
		// This will keep track of how much of the original strSearchText has 
		// been chopped off so that when a valid pattern is found (one that 
		// satisfies constraints) we can map its location back to the original string
		size_t nLocalOffset = 0;

		// Set the pattern
		m_ipRegExpr->Pattern = it->c_str();

		// Get the matches
		IIUnknownVectorPtr ipMatches = m_ipRegExpr->Find(m_strSearchText.c_str(),
			VARIANT_FALSE, VARIANT_FALSE);
		long nSize = ipMatches != __nullptr ? ipMatches->Size() : 0;
		if (nSize == 0)
		{
			continue;
		}

		for (long i=0; i < nSize; i++)
		{
			// Get the start and end positions from the match
			long nStart(0), nEnd(0);
			getStartAndEndPositionForMatch(ipMatches->At(i), nStart, nEnd);

			// Compute the offset start and end positions
			nStart += m_nOffSet;
			nEnd += m_nOffSet;

			// Check if there is a font constraint
			if (ipTemp != __nullptr)
			{
				// To ensure that if necessary this string meets our font constraints
				// we will check the font size of each letter
				ISpatialStringPtr ipSubStr = ipTemp->GetSubString(nStart, nEnd);
				ASSERT_RESOURCE_ALLOCATION("ELI15637", ipSubStr != __nullptr);

				if (ipSubStr->ContainsCharacterOutsideFontRange(m_nFontMin, m_nFontMax) == VARIANT_TRUE)
				{
					continue;
				}
			}

			m_vecMatches.push_back(pair<size_t, size_t>(nStart, nEnd));
		}
	}

	// Sort the found positions
	sort(m_vecMatches.begin(), m_vecMatches.end(), sortFoundPosFunction);
}
//-------------------------------------------------------------------------------------------------
void FindRegExDlg::readPatterns(const string& strPatternsText)
{
	m_vecPatterns.clear();
	bool bAsRegex = m_chkAsRegEx.GetCheck() == BST_CHECKED;

	// delimiter is line feed
	vector<string> vecLines;
	StringTokenizer::sGetTokens(strPatternsText, "\r\n", vecLines);
	for(auto it = vecLines.begin(); it != vecLines.end(); it++)
	{
		if (!it->empty())
		{
			if (!bAsRegex)
			{
				// if it's not regular expression, 
				::convertStringToRegularExpression(*it);
			}

			m_vecPatterns.push_back(*it);
		}
	}
}
//-------------------------------------------------------------------------------------------------
void FindRegExDlg::showDetailSettings(bool bShow)
{
	m_bShowAdvanced = bShow;

	CRect dlgCurrentRect;
	GetWindowRect(&dlgCurrentRect);

	// record the entire dialog height
	if (m_nDlgHeight == 0)
	{
		m_nDlgHeight = dlgCurrentRect.Height();
	}

	if (m_bShowAdvanced)
	{
		m_btnAdvance.SetWindowText("Advanced <<");
		SetWindowPos(&wndTop, dlgCurrentRect.left, 
					 dlgCurrentRect.top, dlgCurrentRect.Width(), 
					 m_nDlgHeight, SWP_NOZORDER);
	}
	else
	{
		CRect rectRangeCheckBox;
		m_chkFontSizeRange.GetWindowRect(&rectRangeCheckBox);
		ScreenToClient(rectRangeCheckBox);

		m_btnAdvance.SetWindowText("Advanced >>");
		SetWindowPos(&wndTop, dlgCurrentRect.left, 
					 dlgCurrentRect.top, dlgCurrentRect.Width(), 
					 rectRangeCheckBox.top + 3, SWP_NOZORDER);
	}

	// Show/hide advanced controls
	BOOL showHide = asMFCBool(bShow);
	m_chkRange.ShowWindow(showHide);
	m_editRangeTo.ShowWindow(showHide);
	m_editRangeFrom.ShowWindow(showHide);
	m_chkFontSizeRange.ShowWindow(showHide);
	m_cmbIncludeFontSize.ShowWindow(showHide);
	m_radBegin.ShowWindow(showHide);
	m_radCurPos.ShowWindow(showHide);
}
//-------------------------------------------------------------------------------------------------
bool FindRegExDlg::validateFromToValue()
{
	UpdateData();
	
	// Check search range and font size settings
	if (!checkSearchRangeSettings() || !checkFontSizeSettings())
	{
		return false;
	}
	
	return true;
}
//-------------------------------------------------------------------------------------------------
void FindRegExDlg::OnShowWindow(BOOL bShow, UINT nStatus) 
{
	CDialog::OnShowWindow(bShow, nStatus);
	
	if(bShow == TRUE)
	{
		m_editPatterns.SetFocus();
		SetForegroundWindow();
	}
}
//-------------------------------------------------------------------------------------------------
size_t FindRegExDlg::getSearchStartPosition(size_t inputLength)
{
	double dStartRange = m_chkRange.GetCheck() == BST_CHECKED ?
		asDouble((LPCTSTR)m_zRangeFrom)/100.0 : 0.0;

	return (size_t)(dStartRange * inputLength);
}
//-------------------------------------------------------------------------------------------------
size_t FindRegExDlg::getSearchEndPosition(size_t inputLength)
{
	double dEndRange = m_chkRange.GetCheck() == BST_CHECKED ?
		asDouble((LPCTSTR)m_zRangeTo)/100.0 : 1.0;

	return (size_t)(dEndRange * inputLength);
}
//-------------------------------------------------------------------------------------------------
bool FindRegExDlg::checkFontSizeSettings()
{
	if (m_chkFontSizeRange.GetCheck() == BST_CHECKED)
	{
		// Retrieve settings
		CString zFrom;
		m_editFromFontSize.GetWindowText( zFrom );

		// Check for NULL entries
		if (zFrom.IsEmpty())
		{
			::MessageBox(NULL, "Please provide an integer value from 0 to 99", 
				"From Value", MB_OK | MB_ICONEXCLAMATION);
			m_editFromFontSize.SetSel(0, -1);
			m_editFromFontSize.SetFocus();
			return false;
		}
		auto nFrom = asLong((LPCTSTR)zFrom);
		if (nFrom < 0 || nFrom > 99)
		{
			::MessageBox(NULL, "Please provide an integer value from 0 to 99", 
				"From Value", MB_OK | MB_ICONEXCLAMATION);
			m_editFromFontSize.SetSel(0, -1);
			m_editFromFontSize.SetFocus();
			return false;
		}

		CString zTo;
		m_editToFontSize.GetWindowText( zTo );
		if (zTo.IsEmpty())
		{
			::MessageBox(NULL, "Please provide an integer value from 0 to 99", 
				"To Value", MB_OK | MB_ICONEXCLAMATION);
			m_editToFontSize.SetSel(0, -1);
			m_editToFontSize.SetFocus();
			return false;
		}
		auto nTo = asLong((LPCTSTR)zTo);
		if (nTo < 0 || nTo > 99)
		{
			::MessageBox(NULL, "Please provide an integer value from 0 to 99", 
				"To Value", MB_OK | MB_ICONEXCLAMATION);
			m_editToFontSize.SetSel(0, -1);
			m_editToFontSize.SetFocus();
			return false;
		}

		// Validate non-zero entries
		if (nFrom > nTo)
		{
			::MessageBox(NULL, "To value must be greater than From value.", 
				"To Value", MB_OK | MB_ICONEXCLAMATION);
			m_editToFontSize.SetSel(0, -1);
			m_editToFontSize.SetFocus();
			return false;
		}

		m_nFontMin = nFrom;
		m_nFontMax = nTo;
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
bool FindRegExDlg::checkSearchRangeSettings()
{
	// if Search scope is entire document
	if (m_chkRange.GetCheck() == BST_UNCHECKED)
	{
		return true;
	}

	if (m_zRangeFrom.IsEmpty())
	{
		::MessageBox(NULL, "Please provide an integer value from 0 to 99", 
			"From Value", MB_OK|MB_ICONEXCLAMATION);
		m_editRangeFrom.SetSel(0, -1);
		m_editRangeFrom.SetFocus();
		return false;
	}
	if (m_zRangeTo.IsEmpty())
	{
		::MessageBox(NULL, "Please provide an integer value from 1 to 100", 
			"To Value", MB_OK|MB_ICONEXCLAMATION);
		m_editRangeTo.SetSel(0, -1);
		m_editRangeTo.SetFocus();
		return false;
	}
	
	long nFromValue = ::asLong((LPCTSTR)m_zRangeFrom);
	if (nFromValue >= 100)
	{
		::MessageBox(NULL, "Please provide an integer value from 0 to 99", 
			"From Value", MB_OK|MB_ICONEXCLAMATION);
		m_editRangeFrom.SetSel(0, -1);
		m_editRangeFrom.SetFocus();
		return false;
	}

	long nToValue = ::asLong((LPCTSTR)m_zRangeTo);
	if (nToValue > 100)
	{
		::MessageBox(NULL, "Please provide an integer value from 1 to 100", 
			"To Value", MB_OK|MB_ICONEXCLAMATION);
		m_editRangeTo.SetSel(0, -1);
		m_editRangeTo.SetFocus();
		return false;
	}

	if (nFromValue >= nToValue)
	{
		// make sure the From value doesn't exceed To value
		::MessageBox(NULL, "To value must be greater than From value.", 
			"To Value", MB_OK|MB_ICONEXCLAMATION);
		m_editRangeTo.SetSel(0, -1);
		m_editRangeTo.SetFocus();
		return false;
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
IRegularExprParserPtr FindRegExDlg::getRegExParser()
{
	if (m_ipRegExpr == __nullptr)
	{
		IMiscUtilsPtr ipMiscUtils(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI13066", ipMiscUtils != __nullptr );

		m_ipRegExpr = ipMiscUtils->GetNewRegExpParserInstance("FindRegExDlg");
		ASSERT_RESOURCE_ALLOCATION("ELI07404", m_ipRegExpr != __nullptr);
	}

	return m_ipRegExpr;
}
//-------------------------------------------------------------------------------------------------
void FindRegExDlg::getStartAndEndPositionForMatch(IObjectPairPtr ipMatchPair, long& nStart,
	long& nEnd)
{
	ASSERT_ARGUMENT("ELI07409", ipMatchPair != __nullptr);
	ITokenPtr ipMatch = ipMatchPair->Object1;
	ASSERT_RESOURCE_ALLOCATION("ELI07410", ipMatch != __nullptr);

	// Get the start and end position of the match
	nStart = ipMatch->StartPosition;
	nEnd = ipMatch->EndPosition;
}
//-------------------------------------------------------------------------------------------------
