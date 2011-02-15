// RedactionAppearanceDlg.cpp : implementation file
//

#include "stdafx.h"
#include "RedactionAppearanceDlg.h"
#include "RedactionCCUtils.h"
#include "RedactionCCConstants.h"

#include <TemporaryResourceOverride.h>
#include <UCLIDException.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------

// Text for the redaction text combobox's drop down menu
const int giNUM_REDACTION_TEXT_DEFAULTS = 10;
const string gstrREDACTION_TEXT_DEFAULTS[] =
{
	"[redacted]",
	"***redacted***",
	"<ExemptionCodes>",
	"<FieldType>",
	"[redacted] <ExemptionCodes>",
	"***redacted*** <ExemptionCodes>",
	"[redacted <FieldType>]",
	"[***redacted*** <FieldType>]",
	"[redacted <FieldType>] <ExemptionCodes>",
	"[***redacted*** <FieldType>] <ExemptionCodes>"
};

struct ColorOption
{
	string strColorName;
	COLORREF crColor;
};

// Options for redaction border and fill color
const int giNUM_COLOR_OPTIONS = 2;
const ColorOption gcoCOLOR_OPTIONS[] =
{
	{"Black", RGB(0,0,0)},
	{"White", RGB(255,255,255)}
};

//-------------------------------------------------------------------------------------------------
// CRedactionAppearanceOptions options
//-------------------------------------------------------------------------------------------------
RedactionAppearanceOptions::RedactionAppearanceOptions()
 : m_strText(""),
   m_strTextToReplace(""),
   m_strReplacementText(""),
   m_bAdjustTextCasing(false),
   m_crBorderColor(0),
   m_crFillColor(0),
   m_iPointSize(8)
{
	try
	{
		memset(&m_lgFont, 0, sizeof(LOGFONT));
		lstrcpyn(m_lgFont.lfFaceName, "Times New Roman", LF_FACESIZE);
		m_lgFont.lfWeight = FW_NORMAL;
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI24750")
}
//-------------------------------------------------------------------------------------------------
string RedactionAppearanceOptions::getFontAsString()
{
	string strFont = m_lgFont.lfFaceName;
	strFont += " ";
	strFont += asString(m_iPointSize);
	strFont += "pt";

	// Append the font style
	bool bBold = m_lgFont.lfWeight >= 700;
	bool bItalic = m_lgFont.lfItalic == gucIS_ITALIC;
	if (bBold || bItalic)
	{
		strFont += "; ";

		if (bBold)
		{
			strFont += "Bold ";
		}
		if (bItalic)
		{
			strFont += "Italic";
		}
	}

	return strFont;
}
//-------------------------------------------------------------------------------------------------
void RedactionAppearanceOptions::reset()
{
	m_strText = "";
	m_strTextToReplace = "";
	m_strReplacementText = "";
	m_bAdjustTextCasing = false;
	m_crBorderColor = 0;
	m_crFillColor = 0;
	memset(&m_lgFont, 0, sizeof(LOGFONT));
	lstrcpyn(m_lgFont.lfFaceName, "Times New Roman", LF_FACESIZE);
	m_lgFont.lfWeight = FW_NORMAL;
	m_iPointSize = 8;
}

//-------------------------------------------------------------------------------------------------
// CRedactionAppearanceDlg dialog
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(CRedactionAppearanceDlg, CDialog)
//-------------------------------------------------------------------------------------------------
CRedactionAppearanceDlg::CRedactionAppearanceDlg(const RedactionAppearanceOptions &options, 
												 CWnd* pParent /*=NULL*/)
	: CDialog(CRedactionAppearanceDlg::IDD, pParent),
	  m_options(options),
	  m_dwRedactionTextSelection(0)
{
}
//-------------------------------------------------------------------------------------------------
CRedactionAppearanceDlg::~CRedactionAppearanceDlg()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI24618")
}
//-------------------------------------------------------------------------------------------------
void CRedactionAppearanceDlg::getOptions(RedactionAppearanceOptions &rOptions)
{
	rOptions = m_options;
}
//-------------------------------------------------------------------------------------------------
int CRedactionAppearanceDlg::getIndexFromColor(COLORREF crColor)
{
	for (int i=0; i < giNUM_COLOR_OPTIONS; i++)
	{
		if (gcoCOLOR_OPTIONS[i].crColor == crColor)
		{
			return i;
		}
	}

	// This color was not found
	return -1;
}
//-------------------------------------------------------------------------------------------------
void CRedactionAppearanceDlg::updateFontDescription()
{
	// Append the font name and size
	string strFont = m_options.getFontAsString();

	// Set font description
	m_editFontDescription.SetWindowText(strFont.c_str());
}
//-------------------------------------------------------------------------------------------------
void CRedactionAppearanceDlg::updateSampleRedactionText()
{
	CString zText;
	m_comboText.GetWindowText(zText);
	string strText = zText;
	updateSampleRedactionText(strText);
}
//-------------------------------------------------------------------------------------------------
void CRedactionAppearanceDlg::updateSampleRedactionText(const string& strText)
{
	// Expand the tags and display the sample
	string strResult = CRedactionCustomComponentsUtils::ExpandRedactionTags(
		strText, "(b)(1)", "SSN");
	m_editSampleText.SetWindowText(strResult.c_str());
}
//-------------------------------------------------------------------------------------------------
void CRedactionAppearanceDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_COMBO_REDACTION_TEXT, m_comboText);
	DDX_Control(pDX, IDC_BUTTON_REDACTION_TEXT_TAG, m_buttonTextTag);
	DDX_Control(pDX, IDC_EDIT_SAMPLE_TEXT, m_editSampleText);
	DDX_Control(pDX, IDC_COMBO_BORDER_COLOR, m_comboBorderColor);
	DDX_Control(pDX, IDC_COMBO_FILL_COLOR, m_comboFillColor);
	DDX_Control(pDX, IDC_EDIT_FONT, m_editFontDescription);
	DDX_Control(pDX, IDC_CHECK_REPLACE_TEXT, m_checkReplaceText);
	DDX_Control(pDX, IDC_EDIT_REPLACE_TEXT, m_editTextToReplace);
	DDX_Control(pDX, IDC_EDIT_REPLACEMENT_TEXT, m_editReplacementText);
	DDX_Control(pDX, IDC_CHECK_ADJUST_CASE, m_checkAutoAdjustCase);
}
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CRedactionAppearanceDlg, CDialog)
	ON_BN_CLICKED(IDC_BUTTON_SELECT_FONT, &CRedactionAppearanceDlg::OnBnClickedButtonSelectFont)
	ON_BN_CLICKED(IDC_BUTTON_REDACTION_TEXT_TAG, &CRedactionAppearanceDlg::OnBnClickedButtonRedactionTextTag)
	ON_BN_CLICKED(IDC_CHECK_REPLACE_TEXT, &CRedactionAppearanceDlg::OnCheckChangedCheckReplaceText)
	ON_CBN_SELENDCANCEL(IDC_COMBO_REDACTION_TEXT, &CRedactionAppearanceDlg::OnCbnSelendcancelComboRedactionText)
	ON_CBN_EDITCHANGE(IDC_COMBO_REDACTION_TEXT, &CRedactionAppearanceDlg::OnCbnEditchangeComboRedactionText)
	ON_CBN_SELCHANGE(IDC_COMBO_REDACTION_TEXT, &CRedactionAppearanceDlg::OnCbnSelchangeComboRedactionText)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CRedactionAppearanceDlg message handlers
//-------------------------------------------------------------------------------------------------
BOOL CRedactionAppearanceDlg::OnInitDialog()
{
	AFX_MANAGE_STATE(AfxGetModuleState());
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	CDialog::OnInitDialog();

	try
	{
		// Initialize the document tag button
		m_buttonTextTag.SubclassDlgItem(IDC_BUTTON_SELECT_META_TAG, CWnd::FromHandle(m_hWnd));
		m_buttonTextTag.SetIcon(::LoadIcon(_Module.m_hInstResource, 
			MAKEINTRESOURCE(IDI_ICON_SELECT_DOC_TAG)));

		// Fill combo boxes
		for (int i=0; i < giNUM_REDACTION_TEXT_DEFAULTS; i++)
		{
			m_comboText.AddString(gstrREDACTION_TEXT_DEFAULTS[i].c_str());
		}
		for (int i=0; i < giNUM_COLOR_OPTIONS; i++)
		{
			const char* pszColor = gcoCOLOR_OPTIONS[i].strColorName.c_str();
			m_comboBorderColor.AddString(pszColor);
			m_comboFillColor.AddString(pszColor);
		}

		// Set redaction text
		m_comboText.SetWindowText(m_options.m_strText.c_str());
		if (!m_options.m_strTextToReplace.empty())
		{
			m_checkReplaceText.SetCheck(BST_CHECKED);
			m_editTextToReplace.SetWindowText(m_options.m_strTextToReplace.c_str());
			m_editReplacementText.SetWindowText(m_options.m_strReplacementText.c_str());
		}
		else
		{
			m_checkReplaceText.SetCheck(BST_UNCHECKED);
			m_editTextToReplace.EnableWindow(FALSE);
			m_editReplacementText.EnableWindow(FALSE);
		}
		m_checkAutoAdjustCase.SetCheck(asBSTChecked(m_options.m_bAdjustTextCasing));

		// Set colors
		m_comboBorderColor.SetCurSel( getIndexFromColor(m_options.m_crBorderColor) );
		m_comboFillColor.SetCurSel( getIndexFromColor(m_options.m_crFillColor) );

		// Set the font
		updateFontDescription();
		updateSampleRedactionText();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24620")

	return TRUE;
}
//-------------------------------------------------------------------------------------------------
void CRedactionAppearanceDlg::OnOK()
{
	try
	{
		// Set the redaction text
		CString zText;
		m_comboText.GetWindowText(zText);
		m_options.m_strText = zText;
		if (m_checkReplaceText.GetCheck() == BST_CHECKED)
		{
			m_editTextToReplace.GetWindowText(zText);
			m_options.m_strTextToReplace = zText;
			m_editReplacementText.GetWindowText(zText);
			m_options.m_strReplacementText = zText;
		}
		else
		{
			m_options.m_strTextToReplace = "";
			m_options.m_strReplacementText = "";
		}
		m_options.m_bAdjustTextCasing = m_checkAutoAdjustCase.GetCheck() == BST_CHECKED;

		// Set the border color
		int selection = m_comboBorderColor.GetCurSel();
		if (selection == CB_ERR)
		{
			MessageBox("Please select a border color.", "Error");
			m_comboBorderColor.SetFocus();
			return;
		}
		m_options.m_crBorderColor = gcoCOLOR_OPTIONS[selection].crColor;

		// Set the fill color
		selection = m_comboFillColor.GetCurSel();
		if (selection == CB_ERR)
		{
			MessageBox("Please select a fill color.", "Error");
			m_comboFillColor.SetFocus();
			return;
		}
		m_options.m_crFillColor = gcoCOLOR_OPTIONS[selection].crColor;

		CDialog::OnOK();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24621")
}
//-------------------------------------------------------------------------------------------------
void CRedactionAppearanceDlg::OnBnClickedButtonSelectFont()
{
	try
	{
		// Initialize the options for the dialog
		CHOOSEFONT cf = {0};
		cf.lStructSize = sizeof(CHOOSEFONT);
		cf.hwndOwner = m_hWnd;
		cf.lpLogFont = &(m_options.m_lgFont);
		cf.Flags = CF_INITTOLOGFONTSTRUCT | CF_SCREENFONTS;

		// Get a device context
		CDC* pDC = GetDC();
		if (pDC == NULL)
		{
			throw UCLIDException("ELI24681", "Unable to create device context.");
		}

		try
		{
			// Convert the point size to pixels for the dialog
			cf.lpLogFont->lfHeight = 
				-MulDiv(m_options.m_iPointSize, GetDeviceCaps(pDC->m_hDC, LOGPIXELSY), 72);
			ReleaseDC(pDC);
		}
		catch (...)
		{
			if (pDC != NULL)
			{
				ReleaseDC(pDC);
			}

			throw;
		}

		// Display the dialog
		if (ChooseFont(&cf) == TRUE)
		{
			// Store the point size (the LOGFONT structure is already set)
			m_options.m_iPointSize = cf.iPointSize / 10;

			updateFontDescription();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24634")
}
//-------------------------------------------------------------------------------------------------
void CRedactionAppearanceDlg::OnBnClickedButtonRedactionTextTag()
{
	try
	{
		// Get the position and dimensions of the button
		RECT rect;
		m_buttonTextTag.GetWindowRect(&rect);

		// Get the user selection
		string strChoice = 
			CRedactionCustomComponentsUtils::ChooseRedactionTextTag(m_hWnd, rect.right, rect.top);

		// Set the corresponding edit box if the user selected a tag
		if(strChoice != "")
		{
			// Replace the previously selected combobox text with the selected tag
			int iStart = LOWORD(m_dwRedactionTextSelection);
			int iEnd = HIWORD(m_dwRedactionTextSelection);
			CString zText;
			m_comboText.GetWindowText(zText);
			string strText = zText;
			string strResult = strText.substr(0, iStart) + strChoice + strText.substr(iEnd);
			m_comboText.SetWindowText(strResult.c_str());

			// Reset the selection
			m_dwRedactionTextSelection = MAKELONG(strResult.length(), strResult.length());

			// Update the sample text
			updateSampleRedactionText();
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24635")
}
//-------------------------------------------------------------------------------------------------
void CRedactionAppearanceDlg::OnCbnSelendcancelComboRedactionText()
{
	try
	{
		// Remember the currently selected text before focus is lost
		m_dwRedactionTextSelection = m_comboText.GetEditSel();

		updateSampleRedactionText();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24641")
}
//-------------------------------------------------------------------------------------------------
void CRedactionAppearanceDlg::OnCbnEditchangeComboRedactionText()
{
	try
	{
		updateSampleRedactionText();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24642")
}
//-------------------------------------------------------------------------------------------------
void CRedactionAppearanceDlg::OnCbnSelchangeComboRedactionText()
{
	try
	{
		int iCurSel = m_comboText.GetCurSel();
		CString zText;
		m_comboText.GetLBText(iCurSel, zText);
		string strText = zText;
		updateSampleRedactionText(strText);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI24955")
}
//-------------------------------------------------------------------------------------------------
void CRedactionAppearanceDlg::OnCheckChangedCheckReplaceText()
{
	try
	{
		BOOL bEnable = asMFCBool(m_checkReplaceText.GetCheck() == BST_CHECKED);
		m_editTextToReplace.EnableWindow(bEnable);
		m_editReplacementText.EnableWindow(bEnable);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI31663");
}
//-------------------------------------------------------------------------------------------------

