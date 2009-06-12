#pragma once

#include "resource.h"

#include <ImageButtonWithStyle.h>

#include <string>

using namespace std;

//-------------------------------------------------------------------------------------------------
// MetadataOptions options
//-------------------------------------------------------------------------------------------------
struct MetadataOptions
{
	string strOutputFile;
};

//-------------------------------------------------------------------------------------------------
// CMetadataDlg dialog
//-------------------------------------------------------------------------------------------------
class CMetadataDlg : public CDialog
{
	DECLARE_DYNAMIC(CMetadataDlg)

public:

	CMetadataDlg(const MetadataOptions &options, CWnd* pParent = NULL);   // standard constructor
	virtual ~CMetadataDlg();

	void getOptions(MetadataOptions &rOptions);

// Dialog Data
	enum { IDD = IDD_METADATA_DLG };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();
	virtual void OnOK();
	afx_msg void OnBnClickedButtonMetaBrowse();
	afx_msg void OnBnClickedButtonSelectMetaTag();

	DECLARE_MESSAGE_MAP()

private:

	CEdit m_editMetaOutputName;
	CImageButtonWithStyle m_btnSelectMetaTag;

	MetadataOptions m_metadata;
};
