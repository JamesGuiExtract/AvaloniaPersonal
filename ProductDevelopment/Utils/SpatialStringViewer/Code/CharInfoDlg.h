//==================================================================================================
//
// COPYRIGHT (c) 2007 - 2008 EXTRACT SYSTEMS, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	CharInfoDlg.h
//
// PURPOSE:	The purpose of the CharInfoDlg is to display all of the known information
//			about the current CLetter from the SpatialStringViewerDlg.  This is a
//			modeless dialog box, when it is displayed it will be updated with the
//			current information.
//
// AUTHORS:	Jeff Shergalis
//
//==================================================================================================
#pragma once
#include "SpatialStringViewerDlg.h"
#include "afxwin.h"

#include <string>

//-------------------------------------------------------------------------------------------------
// CharInfoDlg class
//-------------------------------------------------------------------------------------------------
class CharInfoDlg : public CDialog
{
	DECLARE_DYNAMIC(CharInfoDlg)

public:
	// Dialog Data
	enum { IDD = IDD_CHAR_INFO };
	CharInfoDlg(CWnd* pParent = NULL);
	virtual ~CharInfoDlg();

	// PURPOSE: Set all the values in our display window from an ILetterPtr.
	//			In this case we have all the data and will set all fields
	void setCharacterData(const ILetterPtr& ripLetter);

	// PURPOSE: Set the known values in our dialog from the passed in data.
	//
	// REQUIRE: rzFont is a well formed 8 character Font mask
	//
	// NOTE:	We will reset our dynamic labels to the Mode and Mean font
	//			format since this function is called when we are looking
	//			at a selection of data not just an individual character
	void setCharacterData(const long& rlFontSize, const long& rlConfidence, 
						const long& rlPage, const CString& rzFont);
	//-------------------------------------------------------------------------------------------------
protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();
	DECLARE_MESSAGE_MAP()
	//-------------------------------------------------------------------------------------------------
private:
	// these methods are used to process the data stored in the ILetter struct
	// and format it for display in the dialog box
	CString processGuess(const long& rlGuess);
	std::string processRectangleCoordinate(const long& rlCoordinate);
	std::string processPageNumber(const long& rlPageNumber);

	// variables for the edit boxes
	CEdit m_edGuess1;
	CEdit m_edGuess2;
	CEdit m_edGuess3;
	CEdit m_edConfidence;
	CEdit m_edTop;
	CEdit m_edBottom;
	CEdit m_edRight;
	CEdit m_edLeft;
	CEdit m_edPageNumber;
	CEdit m_edFontSize;

	// variables for our dynamic labels
	CEdit m_edFontSizeLabel;
	CEdit m_edConfidenceLabel;

	// variables for the check boxes (we use these for true and false values)
	CButton m_chkEndOfZone;
	CButton m_chkEndOfParagraph;
	CButton m_chkFontItalic;
	CButton m_chkFontBold;
	CButton m_chkFontSansSerif;
	CButton m_chkFontSerif;
	CButton m_chkFontProportional;
	CButton m_chkFontUnderline;
	CButton m_chkFontSuperScript;
	CButton m_chkFontSubScript;
};
//-------------------------------------------------------------------------------------------------