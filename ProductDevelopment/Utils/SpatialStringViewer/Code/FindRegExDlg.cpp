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
// FindRegExDlg dialog
//-------------------------------------------------------------------------------------------------
FindRegExDlg::FindRegExDlg(CSpatialStringViewerDlg* pSSVDlg,
						   SpatialStringViewerCfg* pCfgDlg,
						   CWnd* pParent /*=NULL*/)
: CDialog(FindRegExDlg::IDD, pParent),
  m_pSSVDlg(pSSVDlg),
  m_pCfgDlg(pCfgDlg),
  m_ipRegExpr(NULL),
  m_nDlgHeight(0)
{
	//{{AFX_DATA_INIT(FindRegExDlg)
	m_bCaseSensitive = FALSE;
	m_zRangeFrom = _T("");
	m_zRangeTo = _T("");
	m_nSearchPos = 1;
	m_nOperator = 0;
	m_zPatterns = _T("");
	//}}AFX_DATA_INIT
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
	DDX_Control(pDX, IDC_CHK_MULTI_REGEX, m_chkTreatAsMultiRegex);
	DDX_Control(pDX, IDC_RADIO_OR, m_radOr);
	DDX_Control(pDX, IDC_RADIO_AND, m_radAnd);
	DDX_Control(pDX, IDC_RADIO_BEGIN, m_radBegin);
	DDX_Control(pDX, IDC_RADIO_CUR_POS, m_radCurPos);
	DDX_Check(pDX, IDC_CHK_CASE_SENSITIVE, m_bCaseSensitive);
	DDX_Text(pDX, IDC_EDIT_FROM, m_zRangeFrom);
	DDX_Text(pDX, IDC_EDIT_TO, m_zRangeTo);
	DDX_Radio(pDX, IDC_RADIO_BEGIN, m_nSearchPos);
	DDX_Radio(pDX, IDC_RADIO_OR, m_nOperator);
	DDX_Text(pDX, IDC_EDIT_EXPRS, m_zPatterns);
	//}}AFX_DATA_MAP
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(FindRegExDlg, CDialog)
	//{{AFX_MSG_MAP(FindRegExDlg)
	ON_BN_CLICKED(IDC_BTN_FIND, OnBtnFind)
	ON_BN_CLICKED(IDC_BTN_ADVANCE, OnBtnAdvance)
	ON_WM_SHOWWINDOW()
	ON_BN_CLICKED(IDC_CHK_RANGE, OnChkRange)
	ON_BN_CLICKED(IDC_CHK_FONTSIZERANGE, OnChkFontsizerange)
	ON_BN_CLICKED(IDC_CHK_AS_REGEX, OnChkAsRegex)
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
		m_chkTreatAsMultiRegex.EnableWindow(TRUE);

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
		CWaitCursor wait;

		UpdateData();

		// populate the vec of patterns
		readPatterns((LPCTSTR)m_zPatterns);

		if (!validateFromToValue())
		{
			return;
		}

		// get entire or portion of original input text
		// according to the setting
		string strInput = calculateSearchText();
		bool bFound = false;
		if (!strInput.empty())
		{
			// look for matches
			int nFoundStartPos, nFoundEndPos;
			bFound = foundPatternsInText(strInput, nFoundStartPos, nFoundEndPos);
			if (bFound)
			{
				// OR relationship
				if (nFoundStartPos >= 0 && nFoundEndPos > 0)
				{
					// select the text in the spatial string viewer
					m_pSSVDlg->selectText(nFoundStartPos, nFoundEndPos+1);
				}
				// AND relationship
				else
				{
					// only display a message
					::MessageBox(NULL, "All specified patterns are found in the document.", "Find", MB_OK);
				}
			}
		}

		if (!bFound)
		{
			// clear any selection
			m_pSSVDlg->selectText(-1,0);
			AfxMessageBox("No match found.");
		}
		string strTmp = (LPCSTR)m_zPatterns;
		m_pCfgDlg->saveLastRegularExpression(strTmp);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI07405");
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
	// TODO: Add your control notification handler code here
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
void FindRegExDlg::OnChkAsRegex()
{
	try
	{
		// Enable/disable the treat as multiple regular expression check box
		// based on whether the as regular expression checkbox is checked
		m_chkTreatAsMultiRegex.EnableWindow(asMFCBool(m_chkAsRegEx.GetCheck() == BST_CHECKED));
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24057");
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
	// where to start searching
	int nStartSearchPos = 0;
	// if user sets start search position at current cursor position
	if (m_nSearchPos == 1)
	{
		nStartSearchPos = m_pSSVDlg->getCurrentCursorPosition();
	}

	// default off set to 0
	m_nOffSet = 0;
	double dStartRange = 0.0;
	double dEndRange = 1.0;
	if (m_chkRange.GetCheck() == 1)
	{
		dStartRange = ::asDouble(LPCTSTR(m_zRangeFrom))/100.0;
		dEndRange = ::asDouble(LPCTSTR(m_zRangeTo))/100.0;
	}

	// make sure end pos greater than start pos, and all within range
	if (dStartRange >= dEndRange)
	{
		UCLIDException ue("ELI07406", "Invalid starting/ending range defined in the file.");
		ue.addDebugInfo("Starting Range", LPCTSTR(m_zRangeFrom));
		ue.addDebugInfo("Ending Range", LPCTSTR(m_zRangeTo));
		throw ue;
	}

	// get the entire text first
	string strInputText = m_pSSVDlg->getEntireDocumentText();
	int nInputSize = strInputText.size();
	// start from where in the input text
	int nStartPos = (int)(dStartRange * nInputSize);
	// compare nStartSearchPos and nStartPos, take whichever's
	// value is greater
	nStartPos = nStartSearchPos > nStartPos ? nStartSearchPos : nStartPos;

	// end at where
	int nEndPos = (int)(dEndRange * nInputSize);
	// if start position already passes the end position, return empty string
	if (nEndPos <= nStartPos)
	{
		// no text
		return "";
	}

	// set offset
	m_nOffSet = nStartPos;
	// get the input text
	string strInputWithinRange = strInputText.substr(nStartPos, nEndPos-nStartPos + 1);

	return strInputWithinRange;
}
//-------------------------------------------------------------------------------------------------
bool FindRegExDlg::foundPatternsInText(const string& strInputText, 
									   int &nFoundStartPos,
									   int &nFoundEndPos)
{
	if (m_ipRegExpr == __nullptr)
	{
		IMiscUtilsPtr ipMiscUtils(CLSID_MiscUtils);
		ASSERT_RESOURCE_ALLOCATION("ELI13066", ipMiscUtils != __nullptr );

		m_ipRegExpr = ipMiscUtils->GetNewRegExpParserInstance("FindRegExDlg");
		ASSERT_RESOURCE_ALLOCATION("ELI07404", m_ipRegExpr != __nullptr);
	}

	// if no patterns specified, return false
	if (m_vecPatterns.empty())
	{
		return false;
	}

	m_ipRegExpr->IgnoreCase = m_bCaseSensitive ? VARIANT_FALSE : VARIANT_TRUE;

	nFoundStartPos = -1;
	nFoundEndPos = -1;
	bool bIsAndRelationship = m_nOperator == 1;
	bool bOrSuccess = false;

	// When we are doing an or search we want to find the first instance of each pattern
	// and return whichever one was first
	// these vars keep track of which first instance comes first in the document as we 
	// iterate over the patterns
	nFoundStartPos = -1;
	nFoundEndPos = -1;

	for (unsigned int n = 0; n < m_vecPatterns.size(); n++)
	{

		// This string is the one we will search for the pattern
		// when we find an instance of the pattern that does not meet 
		// font size (or other) constraints we will chop this string at the end 
		// of the found pattern and search again.  This will be repeated until we find an
		// instance of the pattern that satisfies all the constraints
		string strSearchText = strInputText;
		// This will keep track of how much of the original strSearchText has 
		// been chopped off so that when a valid pattern is found (one that 
		// satisfies constraints) we can map its location back to the original string
		long nLocalOffset = 0;

		string strPattern = m_vecPatterns[n];
		m_ipRegExpr->Pattern = _bstr_t(strPattern.c_str());
		
		// These flags will be used in determining when to stop searching
		bool bFoundPattern = false;
		bool bSearchAgain = false;

		// whether or not this pattern is found in the input text
		IIUnknownVectorPtr ipMatches;
		do
		{
			ipMatches = m_ipRegExpr->Find(_bstr_t(strSearchText.c_str()), VARIANT_TRUE,
				VARIANT_FALSE);
			if(ipMatches->Size() <= 0)
			{
				// No pattern was found so the search is over and it failed
				bSearchAgain = false;
				bFoundPattern = false;
			}
			else
			{
				IObjectPairPtr ipObjPair = ipMatches->At(0);
				ASSERT_RESOURCE_ALLOCATION("ELI07409", ipObjPair != __nullptr);
				ITokenPtr ipMatch = ipObjPair->Object1;
				ASSERT_RESOURCE_ALLOCATION("ELI07410", ipMatch != __nullptr);

				// Check the font size constraint
				if(m_chkFontSizeRange.GetCheck() == 1)
				{
					long nStart = ipMatch->StartPosition + m_nOffSet + nLocalOffset;
					long nEnd = ipMatch->EndPosition + m_nOffSet + nLocalOffset;

					// To ensure that if necessary this string meets our font constraints
					// we will check the font size of each letter
					ISpatialStringPtr ipTemp = m_pSSVDlg->getSpatialString();
					ASSERT_RESOURCE_ALLOCATION("ELI15636", ipTemp != __nullptr);
					ISpatialStringPtr ipSubStr = ipTemp->GetSubString(nStart, nEnd);
					ASSERT_RESOURCE_ALLOCATION("ELI15637", ipSubStr != __nullptr);

					CPPLetter* pLetters = NULL;
					long nNumLetters;
					ipSubStr->GetOCRImageLetterArray(&nNumLetters, (void**)&pLetters);
					ASSERT_RESOURCE_ALLOCATION("ELI25968", pLetters != __nullptr);
					long i;
					for (i = 0; i < nNumLetters; i++)
					{
						const CPPLetter& letter = pLetters[i];
						// as soon as one letter in the found pattern does not
						// meet the font size constraints we need to search again for 
						// the pattern
						if(!letterInFontSizeRange(letter))
						{
							// This pattern does not meet the font size constraint
							// so we will chop the search string and search again
							
							// Update the local offset so we can map back to the original string
							nLocalOffset += ipMatch->StartPosition+i+1;
							// Chop the search string
							strSearchText = strSearchText.substr(ipMatch->StartPosition+i+1);
							//search again
							bSearchAgain = true;
							break;
						}
					}
					// if the entire string was in the font size range
					if(i == nNumLetters)
					{
						// We have found a pattern match that satisfies all constraints
						// so we have a match and the search is over
						bFoundPattern = true;
						bSearchAgain = false;
					}
				}
				else
				{
					// There are no constraints on this pattern
					// so we have found our match
					bFoundPattern = true;
					bSearchAgain = false;
				}
			}
		}
		while(bSearchAgain); 
		
		if (bFoundPattern && !bIsAndRelationship)
		{
			// get the start and end position
			IObjectPairPtr ipObjPair = ipMatches->At(0);
			ASSERT_RESOURCE_ALLOCATION("ELI19339", ipObjPair != __nullptr);
			ITokenPtr ipMatch = ipObjPair->Object1;
			ASSERT_RESOURCE_ALLOCATION("ELI19340", ipMatch != __nullptr);

			// start and end position must count the offset
		//	nFoundStartPos = ipMatch->StartPosition + m_nOffSet + nLocalOffset;
		//	nFoundEndPos = ipMatch->EndPosition + m_nOffSet + nLocalOffset;

			long nTmpFoundStartPos = ipMatch->StartPosition + m_nOffSet + nLocalOffset;
			long nTmpFoundEndPos = ipMatch->EndPosition + m_nOffSet + nLocalOffset;
			if (nTmpFoundStartPos < nFoundStartPos ||
				nFoundStartPos == -1)
			{
				nFoundStartPos = nTmpFoundStartPos;
				nFoundEndPos = nTmpFoundEndPos;
				bOrSuccess = true;
			}
			else if (nTmpFoundStartPos == nFoundStartPos && nTmpFoundEndPos > nFoundEndPos)
			{
				nFoundStartPos = nTmpFoundStartPos;
				nFoundEndPos = nTmpFoundEndPos;
			}

			// if it's OR relationship, once a pattern is found
			// return true immediately
			//return true;
		}
		else if (!bFoundPattern && bIsAndRelationship)
		{
			// if it's AND relationship, once a pattern can't be found
			// return false immediately
			return false;
		}
	}

	// once this point is reached, 
	// if bIsAndRelationship == true, return true
	// if bIsAndRelationship == false, return false
	return bIsAndRelationship || bOrSuccess;
}
//-------------------------------------------------------------------------------------------------
void FindRegExDlg::readPatterns(const string& strPatternsText)
{
	m_vecPatterns.clear();
	bool bAsRegex = m_chkAsRegEx.GetCheck() == BST_CHECKED;

	// If treat as a multi-line regular expression then just set pattern vector to
	// the pattern from the edit control
	if (bAsRegex && m_chkTreatAsMultiRegex.GetCheck() == BST_UNCHECKED)
	{
		// Load the regular expression from the string that is in the format
		// of a regular expression file (multi-line expression)
		m_vecPatterns.push_back(getRegExpFromText(strPatternsText, ""));
	}
	// plain text OR multiple regex, split at line feeds
	else
	{
		// delimiter is line feed
		vector<string> vecLines;
		StringTokenizer::sGetTokens(strPatternsText, "\r\n", vecLines);
		for (unsigned int n = 0; n < vecLines.size(); n++)
		{
			// skip any empty lines
			string strLine = vecLines[n];
			if (!strLine.empty())
			{
				if (!bAsRegex)
				{
					// if it's not regular expression, 
					::convertStringToRegularExpression(strLine);
				}

				m_vecPatterns.push_back(strLine);
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------
void FindRegExDlg::showDetailSettings(bool bShow)
{
	m_bShowAdvanced = bShow;

	CRect dlgCurrentRect;
	GetWindowRect(&dlgCurrentRect);
	CRect rectCaseSensitive;
	GetDlgItem(IDC_CHK_CASE_SENSITIVE)->GetWindowRect(&rectCaseSensitive);
	ScreenToClient(rectCaseSensitive);

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
		m_btnAdvance.SetWindowText("Advanced >>");
		SetWindowPos(&wndTop, dlgCurrentRect.left, 
					 dlgCurrentRect.top, dlgCurrentRect.Width(), 
					 rectCaseSensitive.bottom + 36, SWP_NOZORDER);

	}

	// Show/hide advanced controls
	BOOL showHide = asMFCBool(bShow);
	m_radOr.ShowWindow(showHide);
	m_radAnd.ShowWindow(showHide);
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
	// make sure the value doesn't exceed 100
	UpdateData();
	
	//////////////////
	// Check font size
	//////////////////
	if (m_chkFontSizeRange.GetCheck() == 1)
	{
		// Retrieve settings
		CString zFrom;
		CString zTo;
		int iFrom;
		int iTo;
		m_editFromFontSize.GetWindowText( zFrom );
		m_editToFontSize.GetWindowText( zTo );

		// Check for NULL entries
		if (zFrom.IsEmpty())
		{
			::MessageBox(NULL, "Please provide an integer value from 0 to 99", 
				"From Value", MB_OK | MB_ICONEXCLAMATION);
			m_editFromFontSize.SetSel(0, -1);
			m_editFromFontSize.SetFocus();
			return false;
		}
		else if (zTo.IsEmpty())
		{
			::MessageBox(NULL, "Please provide an integer value from 0 to 99", 
				"To Value", MB_OK | MB_ICONEXCLAMATION);
			m_editToFontSize.SetSel(0, -1);
			m_editToFontSize.SetFocus();
			return false;
		}

		// Validate non-zero entries
		iFrom = asLong( zFrom.operator LPCTSTR() );
		iTo = asLong( zTo.operator LPCTSTR() );
		if (iFrom > iTo)
		{
			::MessageBox(NULL, "To value must be greater than From value.", 
				"To Value", MB_OK | MB_ICONEXCLAMATION);
			m_editToFontSize.SetSel(0, -1);
			m_editToFontSize.SetFocus();
			return false;
		}
	}

	/////////////////////
	// Check search scope
	/////////////////////

	// if Search scope is entire document
	if (m_chkRange.GetCheck() == 0)
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
	long nToValue = ::asLong((LPCTSTR)m_zRangeTo);
	if (nFromValue >= 100)
	{
		::MessageBox(NULL, "Please provide an integer value from 0 to 99", 
			"From Value", MB_OK|MB_ICONEXCLAMATION);
		m_editRangeFrom.SetSel(0, -1);
		m_editRangeFrom.SetFocus();
		return false;
	}

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
void FindRegExDlg::OnShowWindow(BOOL bShow, UINT nStatus) 
{
	CDialog::OnShowWindow(bShow, nStatus);
	
	// TODO: Add your message handler code here
	if(bShow == TRUE)
	{
		m_editPatterns.SetFocus();
		SetForegroundWindow();
	}
	
}
//-------------------------------------------------------------------------------------------------
bool FindRegExDlg::letterInFontSizeRange(const CPPLetter& letter)
{
	// we will let all spatial letters pass
	if(letter.m_bIsSpatial == false)
	{
		return true;
	}
	CString zFrom;
	m_editFromFontSize.GetWindowText(zFrom);
	CString zTo;
	m_editToFontSize.GetWindowText(zTo);
	long nMinFont = asLong(string(zFrom));
	long nMaxFont = asLong(string(zTo));

	bool bInRange = false;
	if ((nMinFont == -1 || letter.m_ucFontSize >= nMinFont) &&
		(nMaxFont == -1 || letter.m_ucFontSize <= nMaxFont))
	{
		bInRange = true;
	}

	// This means we want font sizes outside "not in" the range
	if(m_cmbIncludeFontSize.GetCurSel() == 1)
	{
		bInRange = !bInRange;
	}
	return bInRange;
}

