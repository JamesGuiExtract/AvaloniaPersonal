#pragma once

#include "Resource.h"

//--------------------------------------------------------------------------------------------------
// DiffStatusDlg dialog
//--------------------------------------------------------------------------------------------------
class DiffStatusDlg : public CDialog
{
public:
	DiffStatusDlg(CWnd* pParent = NULL);   // standard constructor
	virtual ~DiffStatusDlg();

// Dialog Data
	enum { IDD = IDD_COMPARETOVSSWITHCUSTOMDIFFTOOL_DIALOG };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

private:
	DECLARE_MESSAGE_MAP()
	DECLARE_DYNAMIC(DiffStatusDlg)
};
//--------------------------------------------------------------------------------------------------
