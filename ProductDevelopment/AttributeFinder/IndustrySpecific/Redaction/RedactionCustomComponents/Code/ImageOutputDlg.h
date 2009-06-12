#pragma once

#include "resource.h"

#include <ImageButtonWithStyle.h>

#include <string>

using namespace std;

//-------------------------------------------------------------------------------------------------
// ImageOutputOptions options
//-------------------------------------------------------------------------------------------------
struct ImageOutputOptions
{
	bool bRetainAnnotations;
	bool bApplyAsAnnotations;
	string strOutputFile;
	bool bRetainRedactions;
};

//-------------------------------------------------------------------------------------------------
// CImageOutputDlg dialog
//-------------------------------------------------------------------------------------------------
class CImageOutputDlg : public CDialog
{
	DECLARE_DYNAMIC(CImageOutputDlg)

public:
	CImageOutputDlg(const ImageOutputOptions &options, CWnd* pParent = NULL);   // standard constructor
	virtual ~CImageOutputDlg();

	void getOptions(ImageOutputOptions &rOptions);

// Dialog Data
	enum { IDD = IDD_IMAGE_OUTPUT_DLG };

	afx_msg void OnBnClickedButtonImgBrowse();
	afx_msg void OnBnClickedButtonSelectImageTag();

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();
	virtual void OnOK();

	DECLARE_MESSAGE_MAP()

private:

	ImageOutputOptions m_options;

	// Annotation options
	CButton m_chkCarryAnnotation;
	CButton m_chkRedactionAsAnnotation;

	// Output image
	CEdit m_editOutputImageName;
	CImageButtonWithStyle m_btnSelectImgTag;

	// Input image
	CButton m_radioRedactedImage;
	CButton m_radioOriginalImage;
};
