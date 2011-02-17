#pragma once

#include "resource.h"
#include "afxwin.h"

#include <string>
#include <vector>
#include <utility>

using namespace std;

//-------------------------------------------------------------------------------------------------
// RedactionAppearanceOptions options
//-------------------------------------------------------------------------------------------------
struct RedactionAppearanceOptions
{
	// Variables
	string m_strText;
	vector<pair<string,string>> m_vecReplacements;
	string m_strTextToReplace;
	string m_strReplacementText;
	bool m_bAdjustTextCasing;
	COLORREF m_crBorderColor;
	COLORREF m_crFillColor;
	LOGFONT m_lgFont;
	int m_iPointSize;

	// Default constructor
	RedactionAppearanceOptions();

	// Creates a string containing the font family, size, and style
	string getFontAsString();

	// Reset all values to their defaults
	void reset();
};

//-------------------------------------------------------------------------------------------------
// CRedactionAppearanceDlg dialog
//-------------------------------------------------------------------------------------------------
class CRedactionAppearanceDlg : public CDialog
{
	DECLARE_DYNAMIC(CRedactionAppearanceDlg)

public:
	CRedactionAppearanceDlg(const RedactionAppearanceOptions &options, CWnd* pParent = NULL);
	virtual ~CRedactionAppearanceDlg();

	void getOptions(RedactionAppearanceOptions &rOptions);

// Dialog Data
	enum { IDD = IDD_REDACTION_APPEARANCE_DLG };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();
	virtual void OnOK();
	afx_msg void OnBnClickedButtonSelectFont();
	afx_msg void OnBnClickedButtonRedactionTextTag();
	afx_msg void OnCbnSelendcancelComboRedactionText();
	afx_msg void OnCbnEditchangeComboRedactionText();
	afx_msg void OnCbnSelchangeComboRedactionText();
	afx_msg void OnCheckChangedCheckReplaceText();

	DECLARE_MESSAGE_MAP()

private:

	//---------------------------------------------------------------------------------------------
	// Variables
	//---------------------------------------------------------------------------------------------
	
	// Text
	CComboBox m_comboText;
	CButton m_buttonTextTag;
	CButton m_checkReplaceText;
	CButton m_checkAutoAdjustCase;
	CEdit m_editTextToReplace;
	CEdit m_editReplacementText;
	CEdit m_editSampleText;

	// Color
	CComboBox m_comboBorderColor;
	CComboBox m_comboFillColor;

	// Font
	CEdit m_editFontDescription;

	// Options
	RedactionAppearanceOptions m_options;

	// The currently selected text in redaction text combo box
	DWORD m_dwRedactionTextSelection;

	//---------------------------------------------------------------------------------------------
	// Methods
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Returns the index of the specified color in the color comboboxes. Returns -1 if 
	//          the color does not correspond to a value in the comoboxes.
	int getIndexFromColor(COLORREF crColor);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Sets the font description edit box based on the currently selected font.
	void updateFontDescription();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Sets the sample redaction text edit box.
	void updateSampleRedactionText();
	void updateSampleRedactionText(const string& strText);
};
