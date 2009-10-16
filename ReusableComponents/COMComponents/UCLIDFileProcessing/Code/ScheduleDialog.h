#pragma once

#include "resource.h"
#include "afxcmn.h"
#include "ScheduleGrid.h"

//-------------------------------------------------------------------------------------------------
// ScheduleDialog dialog
//-------------------------------------------------------------------------------------------------
class ScheduleDialog : public CDialog
{
	DECLARE_DYNAMIC(ScheduleDialog)

public:
	ScheduleDialog(CWnd* pParent = NULL);   // standard constructor
	virtual ~ScheduleDialog();

	// Set the scheduled hours for the grid
	void SetScheduledHours(const vector<bool>& vecSchedule);

	// Get the scheduled hours from the grid
	void GetScheduledHours(vector<bool>& rvecSchedule);

// Dialog Data
	enum { IDD = IDD_DIALOG_SET_SCHEDULE };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	virtual BOOL OnInitDialog();
	afx_msg void OnBnClickedButtonSelectAll();
	afx_msg void OnBnClickedButtonSelectNone();

	DECLARE_MESSAGE_MAP()

private:

	// Grid control
	ScheduleGrid m_listSchedule;
};
