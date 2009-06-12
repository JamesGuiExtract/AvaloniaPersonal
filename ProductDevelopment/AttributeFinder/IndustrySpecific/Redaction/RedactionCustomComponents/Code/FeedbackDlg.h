#pragma once

#include "resource.h"

#include <ImageButtonWithStyle.h>

#include <string>
#include "afxwin.h"

using namespace std;

//-------------------------------------------------------------------------------------------------
// FeedbackOptions options
//-------------------------------------------------------------------------------------------------
struct FeedbackOptions
{
	string strDataFolder;
	bool bCollectImage;
	bool bOriginalFilenames;
	UCLID_REDACTIONCUSTOMCOMPONENTSLib::EFeedbackCollectOption eCollectionOptions;
};

//-------------------------------------------------------------------------------------------------
// CFeedbackDlg dialog
//-------------------------------------------------------------------------------------------------
class CFeedbackDlg : public CDialog
{
	DECLARE_DYNAMIC(CFeedbackDlg)

public:
	CFeedbackDlg(const FeedbackOptions &options, CWnd* pParent = NULL);   // standard constructor
	virtual ~CFeedbackDlg();

	void getOptions(FeedbackOptions &rOptions);

// Dialog Data
	enum { IDD = IDD_FEEDBACK_DLG };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();
	virtual void OnOK();
	afx_msg void OnBnClickedButtonFeedbackFolderBrowse();
	afx_msg void OnBnClickedButtonSelectFeedbackFolderTag();

	DECLARE_MESSAGE_MAP()

private:

	// Feedback data storage and options
	CEdit m_editFeedbackFolder;
	CImageButtonWithStyle m_btnFeedbackFolderTag;
	CButton m_btnBrowseFeedbackFolder;
	CButton m_chkIncludeRedactInfo;
	CButton m_chkCollectFeedbackImage;

	// Filenames to use for feedback data
	CButton m_optionOriginalFeedbackFilenames;
	CButton m_optionGenerateFeedbackFilenames;

	// Collect feedback for
	CButton m_chkAllFeedback;
	CButton m_chkRedactFeedback;
	CButton m_chkCorrectFeedback;

	FeedbackOptions m_options;

	//---------------------------------------------------------------------------------------------
	// PURPOSE: Ensure the feedback data folder is non-empty and contains valid tags.
	// PROMISE: Displays an error message and returns false if folder is invalid. Returns true if
	//          the folder is valid.
	bool isValidFeedbackDataFolder(const string& strFeedbackFolder);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Enable and disable the correct fields
	void updateControls();
};
