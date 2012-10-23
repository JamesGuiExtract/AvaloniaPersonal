#pragma once

//////////////////////////////////////////////////////
// FileProcessingAdvQueueDlg dialog
//////////////////////////////////////////////////////
class FileProcessingAdvQueueDlg : public CDialogEx
{
	DECLARE_DYNAMIC(FileProcessingAdvQueueDlg)

public:
	FileProcessingAdvQueueDlg(CWnd* pParent = NULL);   // standard constructor
	virtual ~FileProcessingAdvQueueDlg();
	virtual BOOL OnInitDialog();

	afx_msg void OnBnClickedOk();
	afx_msg void OnBnClickedCancel();

// Dialog Data
	enum { IDD = IDD_DLG_QUEUE_ADV_PROP };

	// Whether the page count check should be skipped when queuing files.
	BOOL m_bSkipPageCount;

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

	DECLARE_MESSAGE_MAP()
	virtual void OnOK();
};
