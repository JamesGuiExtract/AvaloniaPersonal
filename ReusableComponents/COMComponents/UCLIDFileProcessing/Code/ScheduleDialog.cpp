// ScheduleDialog.cpp : implementation file
//

#include "stdafx.h"
#include "ScheduleDialog.h"

#include <UCLIDException.h>
#include <TemporaryResourceOverride.h>

using namespace std;
//-------------------------------------------------------------------------------------------------
// ScheduleDialog dialog
//-------------------------------------------------------------------------------------------------
IMPLEMENT_DYNAMIC(ScheduleDialog, CDialog)

//-------------------------------------------------------------------------------------------------
ScheduleDialog::ScheduleDialog(CWnd* pParent /*=NULL*/)
	: CDialog(ScheduleDialog::IDD, pParent)
{

}
//-------------------------------------------------------------------------------------------------
ScheduleDialog::~ScheduleDialog()
{
}
//-------------------------------------------------------------------------------------------------
void ScheduleDialog::DoDataExchange(CDataExchange* pDX)
{
	try
	{
		CDialog::DoDataExchange(pDX);
		DDX_Control(pDX, IDC_LIST_SCHEDULE, m_listSchedule);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI28066");
}
//-------------------------------------------------------------------------------------------------

BEGIN_MESSAGE_MAP(ScheduleDialog, CDialog)
	ON_BN_CLICKED(IDC_BUTTON_SELECT_ALL, &ScheduleDialog::OnBnClickedButtonSelectAll)
	ON_BN_CLICKED(IDC_BUTTON_SELECT_NONE, &ScheduleDialog::OnBnClickedButtonSelectNone)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// ScheduleDialog message handlers
//-------------------------------------------------------------------------------------------------
BOOL ScheduleDialog::OnInitDialog(void)
{
	AFX_MANAGE_STATE( AfxGetModuleState() );
	TemporaryResourceOverride resourceOverride(_Module.m_hInstResource);

	try
	{	
		CDialog::OnInitDialog();

		// Initialize the schedule grid control
		m_listSchedule.PrepareGrid();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI28064")

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
//-------------------------------------------------------------------------------------------------
void ScheduleDialog::OnBnClickedButtonSelectAll()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		vector<bool> vecHours;
		
		// Set all of the hours to active
		vecHours.resize(giNUMBER_OF_HOURS_IN_WEEK, true);
		m_listSchedule.SetScheduledHours(vecHours);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI28067");
}
//-------------------------------------------------------------------------------------------------
void ScheduleDialog::OnBnClickedButtonSelectNone()
{
	AFX_MANAGE_STATE( AfxGetModuleState() );

	try
	{
		vector<bool> vecHours;

		// Set all of the hours to inactive
		vecHours.resize(giNUMBER_OF_HOURS_IN_WEEK, false);
		m_listSchedule.SetScheduledHours(vecHours);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI28068");
}

//-------------------------------------------------------------------------------------------------
// Public Methods
//-------------------------------------------------------------------------------------------------
void ScheduleDialog::SetScheduledHours(const vector<bool>& vecSchedule)
{
	// Set the schedule for the grid control
	m_listSchedule.SetScheduledHours(vecSchedule);
}
//-------------------------------------------------------------------------------------------------
void ScheduleDialog::GetScheduledHours(vector<bool>& rvecSchedule)
{
	// Get the schedule from the grid control
	rvecSchedule = m_listSchedule.GetScheduledHours();
}
//-------------------------------------------------------------------------------------------------
