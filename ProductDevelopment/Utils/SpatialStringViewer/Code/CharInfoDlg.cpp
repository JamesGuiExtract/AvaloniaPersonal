// CharInfoDlg.cpp : implementation file
#include "stdafx.h"
#include "SpatialStringViewer.h"
#include "CharInfoDlg.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <CPPLetter.h>

// included for definition of gwcUNRECOGNIZED
#include "..\..\..\..\ReusableComponents\COMComponents\UCLIDRasterAndOCRMgmt\OCREngines\SSOCR2\Code\OCRConstants.h"

IMPLEMENT_DYNAMIC(CharInfoDlg, CDialog)

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const CString gzFONT_SIZE_LABEL = "Font Size";
const CString gzFONT_MODE_SIZE_LABEL = "Common Fnt Sz";
const CString gzCONFIDENCE_LABEL = "Confidence";
const CString gzCONFIDENCE_MEAN_LABEL = "Avg Confidence";

// font constants
const char gcBLANK = '_';
const int giITALIC = 0;
const int giBOLD = 1;
const int giSANSSERIF = 2;
const int giSERIF = 3;
const int giPROPORTIONAL = 4;
const int giUNDERLINE = 5;
const int giSUPERSCRIPT = 6;
const int giSUBSCRIPT = 7;

// page number and rectangle coordinate constant
const long glOUT_OF_BOUNDS = 65535;

//-------------------------------------------------------------------------------------------------
// CharInfoDlg dialog
//-------------------------------------------------------------------------------------------------
CharInfoDlg::CharInfoDlg(CWnd* pParent)
: CDialog(CharInfoDlg::IDD, pParent)
{
}
//-------------------------------------------------------------------------------------------------
CharInfoDlg::~CharInfoDlg()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20392");
}
//-------------------------------------------------------------------------------------------------
void CharInfoDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);

	DDX_Control(pDX, IDC_ZONE_END, m_chkEndOfZone);
	DDX_Control(pDX, IDC_PARAGRAPH_END, m_chkEndOfParagraph);
	DDX_Control(pDX, IDC_GUESS_1, m_edGuess1);
	DDX_Control(pDX, IDC_GUESS_2, m_edGuess2);
	DDX_Control(pDX, IDC_GUESS_3, m_edGuess3);
	DDX_Control(pDX, IDC_CONFIDENCE, m_edConfidence);
	DDX_Control(pDX, IDC_TOP, m_edTop);
	DDX_Control(pDX, IDC_BOTTOM, m_edBottom);
	DDX_Control(pDX, IDC_RIGHT, m_edRight);
	DDX_Control(pDX, IDC_LEFT, m_edLeft);
	DDX_Control(pDX, IDC_PAGE_NUM, m_edPageNumber);
	DDX_Control(pDX, IDC_FONT_SIZE, m_edFontSize);
	DDX_Control(pDX, IDC_EDIT_FONT_SIZE_LABEL, m_edFontSizeLabel);
	DDX_Control(pDX, IDC_EDIT_CONFIDENCE_LABEL, m_edConfidenceLabel);
	DDX_Control(pDX, IDC_FONT_ITALIC, m_chkFontItalic);
	DDX_Control(pDX, IDC_FONT_BOLD, m_chkFontBold);
	DDX_Control(pDX, IDC_FONT_SANSSERIF, m_chkFontSansSerif);
	DDX_Control(pDX, IDC_FONT_SERIF, m_chkFontSerif);
	DDX_Control(pDX, IDC_FONT_PROPORTIONAL, m_chkFontProportional);
	DDX_Control(pDX, IDC_FONT_UNDERLINE, m_chkFontUnderline);
	DDX_Control(pDX, IDC_FONT_SUPERSCRIPT, m_chkFontSuperScript);
	DDX_Control(pDX, IDC_FONT_SUBSCRIPT, m_chkFontSubScript);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CharInfoDlg, CDialog)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CharInfoDlg Message Handlers
//-------------------------------------------------------------------------------------------------
BOOL CharInfoDlg::OnInitDialog()
{
	try
	{
		CDialog::OnInitDialog();

		// clear the edit boxes
		m_edGuess1.SetWindowText("");
		m_edGuess2.SetWindowText("");
		m_edGuess3.SetWindowText("");
		m_edConfidence.SetWindowText("");
		m_edTop.SetWindowText("");
		m_edBottom.SetWindowText("");
		m_edLeft.SetWindowText("");
		m_edRight.SetWindowText("");
		m_edPageNumber.SetWindowText("");
		m_edFontSize.SetWindowText("");

		// set the two dynamic labels
		m_edFontSizeLabel.SetWindowText(gzFONT_SIZE_LABEL);
		m_edConfidenceLabel.SetWindowText(gzCONFIDENCE_LABEL);

		// set checkboxes to default state of unchecked
		m_chkEndOfZone.SetCheck(BST_UNCHECKED);
		m_chkEndOfParagraph.SetCheck(BST_UNCHECKED);
		m_chkFontItalic.SetCheck(BST_UNCHECKED);
		m_chkFontBold.SetCheck(BST_UNCHECKED);
		m_chkFontSansSerif.SetCheck(BST_UNCHECKED);
		m_chkFontSerif.SetCheck(BST_UNCHECKED);
		m_chkFontProportional.SetCheck(BST_UNCHECKED);
		m_chkFontUnderline.SetCheck(BST_UNCHECKED);
		m_chkFontSuperScript.SetCheck(BST_UNCHECKED);
		m_chkFontSubScript.SetCheck(BST_UNCHECKED);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI16838")
	
	return TRUE;
}

//-------------------------------------------------------------------------------------------------
// CharInfoDlg Public Methods
//-------------------------------------------------------------------------------------------------
void CharInfoDlg::setCharacterData(const ILetterPtr& ripLetter)
{
	// Get the CPP letter from the ILetter object
	CPPLetter letter;
	ripLetter->GetCppLetter((void*)&letter);

		// set the guesses' edit boxes
	m_edGuess1.SetWindowText( processGuess(letter.m_usGuess1) );
	m_edGuess2.SetWindowText( processGuess(letter.m_usGuess2) );
	m_edGuess3.SetWindowText( processGuess(letter.m_usGuess3) );

	// set the rectangle coordinate's edit boxes
	m_edTop.SetWindowText( processRectangleCoordinate(letter.m_ulTop).c_str() );
	m_edBottom.SetWindowText( processRectangleCoordinate(letter.m_ulBottom).c_str() );
	m_edLeft.SetWindowText( processRectangleCoordinate(letter.m_ulLeft).c_str() );
	m_edRight.SetWindowText( processRectangleCoordinate(letter.m_ulRight).c_str() );


	// format the confidence
	CString zConfidence("");
	zConfidence.Format("%d", letter.m_ucCharConfidence);
	
	// set the confidence
	m_edConfidence.SetWindowText(zConfidence);
	

	// set the page number
	m_edPageNumber.SetWindowText( processPageNumber(letter.m_usPageNumber).c_str() );
	
	// format the font size
	CString zFontSize("");
	zFontSize.Format("%d pt", letter.m_ucFontSize);
	
	// set the font size
	m_edFontSize.SetWindowText(zFontSize);
	
	// set the font information check boxes
	m_chkFontItalic.SetCheck(letter.isItalic() ? BST_CHECKED : BST_UNCHECKED );
	m_chkFontBold.SetCheck(letter.isBold() ? BST_CHECKED : BST_UNCHECKED );
	m_chkFontSansSerif.SetCheck(letter.isSansSerif() ? BST_CHECKED : BST_UNCHECKED );
	m_chkFontSerif.SetCheck(letter.isSerif() ? BST_CHECKED : BST_UNCHECKED );
	m_chkFontProportional.SetCheck(letter.isProportional() ? BST_CHECKED : BST_UNCHECKED );
	m_chkFontUnderline.SetCheck(letter.isUnderline() ? BST_CHECKED : BST_UNCHECKED );
	m_chkFontSuperScript.SetCheck(letter.isSuperScript() ? BST_CHECKED : BST_UNCHECKED );
	m_chkFontSubScript.SetCheck(letter.isSubScript() ? BST_CHECKED : BST_UNCHECKED );

	// set the end of zone and paragraph check buttons
	m_chkEndOfZone.SetCheck(letter.m_bIsEndOfZone ? BST_CHECKED : BST_UNCHECKED);
	m_chkEndOfParagraph.SetCheck(letter.m_bIsEndOfParagraph ? BST_CHECKED : BST_UNCHECKED);

	// set our dynamic labels for the font size and confidence
	m_edFontSizeLabel.SetWindowText( gzFONT_SIZE_LABEL );
	m_edConfidenceLabel.SetWindowText( gzCONFIDENCE_LABEL );
}
//-------------------------------------------------------------------------------------------------
void CharInfoDlg::setCharacterData(const long& rlFontSize, const long& rlConfidence, 
						const long& rlPage, const CString& rzFont)
{
	// format the font size
	CString zFontSize("");
	zFontSize.Format("%d pt", rlFontSize);
	
	// set the font size
	m_edFontSize.SetWindowText(zFontSize);

	// format the confidence
	CString zConfidence("");
	zConfidence.Format("%d", rlConfidence);
	
	// set the confidence
	m_edConfidence.SetWindowText(zConfidence);
	
	// set the page number
	m_edPageNumber.SetWindowText( processPageNumber(rlPage).c_str() );
	
	// set the font check boxes
	m_chkFontItalic.SetCheck( (rzFont[giITALIC] != gcBLANK) ? BST_CHECKED : BST_UNCHECKED );
	m_chkFontBold.SetCheck( (rzFont[giBOLD] != gcBLANK) ? BST_CHECKED : BST_UNCHECKED );
	m_chkFontSansSerif.SetCheck( (rzFont[giSANSSERIF] != gcBLANK) ? BST_CHECKED : BST_UNCHECKED );
	m_chkFontSerif.SetCheck( (rzFont[giSERIF] != gcBLANK) ? BST_CHECKED : BST_UNCHECKED );
	m_chkFontProportional.SetCheck( (rzFont[giPROPORTIONAL] != gcBLANK) ? BST_CHECKED : BST_UNCHECKED );
	m_chkFontUnderline.SetCheck( (rzFont[giUNDERLINE] != gcBLANK) ? BST_CHECKED : BST_UNCHECKED );
	m_chkFontSuperScript.SetCheck( (rzFont[giSUPERSCRIPT] != gcBLANK) ? BST_CHECKED : BST_UNCHECKED );
	m_chkFontSubScript.SetCheck( (rzFont[giSUBSCRIPT] != gcBLANK) ? BST_CHECKED : BST_UNCHECKED );


	// set the rest of the data to empty
	CString zEmpty("");
	m_edGuess1.SetWindowText( zEmpty );
	m_edGuess2.SetWindowText( zEmpty );
	m_edGuess3.SetWindowText( zEmpty );
	m_edTop.SetWindowText( zEmpty );
	m_edBottom.SetWindowText( zEmpty );
	m_edLeft.SetWindowText( zEmpty );
	m_edRight.SetWindowText( zEmpty );
	
	// set the end of zone and paragraph check buttons to false
	m_chkEndOfZone.SetCheck( BST_UNCHECKED );
	m_chkEndOfParagraph.SetCheck( BST_UNCHECKED );

	// set our dynamic labels for the font size and confidence
	m_edFontSizeLabel.SetWindowText( gzFONT_MODE_SIZE_LABEL );
	m_edConfidenceLabel.SetWindowText( gzCONFIDENCE_MEAN_LABEL );
}

//-------------------------------------------------------------------------------------------------
// CharInfoDlg Private Methods
//-------------------------------------------------------------------------------------------------
CString CharInfoDlg::processGuess(const long& rlGuess)
{
	CString zReturn = "";
	
	// check to see if the character is non-printable but has a "C" equivalent
	// return the "C" equivalent (e.g. if \n return "\\n")
	if (rlGuess == '\a')
	{
		zReturn = "\\a";
	}
	else if (rlGuess == '\b')
	{
		zReturn = "\\b";
	}
	else if (rlGuess == '\f')
	{
		zReturn = "\\f";
	}
	else if (rlGuess == '\n')
	{
		zReturn = "\\n";
	}
	else if (rlGuess == '\r')
	{
		zReturn = "\\r";
	}
	else if (rlGuess == '\t')
	{
		zReturn = "\\t";
	}
	else if (rlGuess == '\v')
	{
		zReturn = "\\v";
	}
	// check for the case of the unrecognized character from the OCR engine
	else if (rlGuess == gwcUNRECOGNIZED)
	{
		zReturn = "unknown";
	}
	// space character
	else if (rlGuess == 32)
	{
		zReturn = "\\s";
	}
	// check for other non-printable characters (127 is DELETE)
	else if ( (rlGuess > 32 && rlGuess < 255) && (rlGuess != 127) )
	{
		zReturn = (char) rlGuess;
	}
	// still have not recognized the character, so output our best effort
	// at printing it and its actual value
	else
	{
		zReturn.Format("%c %d",(char) rlGuess, rlGuess);
	}

	return zReturn;
}
//-------------------------------------------------------------------------------------------------
string CharInfoDlg::processRectangleCoordinate(const long& rlCoordinate)
{
	string strReturn("");

	// if out of bounds return the empty string, otherwise set
	// the return string to the coordinate
	if (rlCoordinate != glOUT_OF_BOUNDS)
	{
		strReturn = asString(rlCoordinate);
	}

	return strReturn;
}
//-------------------------------------------------------------------------------------------------
string CharInfoDlg::processPageNumber(const long& rlPageNumber)
{
	string strReturn("");
	
	// if out of bounds return the empty string, otherwise set
	// the return string to the page number
	if (rlPageNumber != glOUT_OF_BOUNDS)
	{
		strReturn = asString(rlPageNumber);
	}

	return strReturn;
}
//-------------------------------------------------------------------------------------------------
