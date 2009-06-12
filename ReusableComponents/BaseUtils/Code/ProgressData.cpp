#include "stdafx.h"
#include "ProgressData.h"
#include "cpputil.h"

// Interval for periodic refresh
const unsigned long gulREFRESHTIMER_MS = 250;

//-------------------------------------------------------------------------------------------------
// ProgressData
//-------------------------------------------------------------------------------------------------
ProgressData::ProgressData(HWND hwndDialog) :
  m_nProgress(0),
  m_hwndDialog(hwndDialog),
  m_dwLastUpdate(0),
  m_bPercentUpdated(false),
  m_bStatusUpdated(false)
{
}
//-------------------------------------------------------------------------------------------------
int ProgressData::getPercentComplete()
{
	// Prevent concurrent execution of methods associated with this object.
//	Win32MutexLockGuard lock( m_mutex );

	return m_nProgress;
}
//-------------------------------------------------------------------------------------------------
void ProgressData::setPercentComplete(int iNewPercent)
{
	// Prevent concurrent execution of methods associated with this object.
//	Win32MutexLockGuard lock( m_mutex );

	// Update setting
	m_nProgress = iNewPercent;

	// Refresh the dialog
	m_bPercentUpdated = true;
	doRefresh();
}
//-------------------------------------------------------------------------------------------------
std::string ProgressData::getStatus()
{
	// Prevent concurrent execution of methods associated with this object.
//	Win32MutexLockGuard lock( m_mutex );

	return m_strStatus;
}
//-------------------------------------------------------------------------------------------------
void ProgressData::setStatus(std::string strNewStatus)
{
	// Prevent concurrent execution of methods associated with this object.
//	Win32MutexLockGuard lock( m_mutex );

	// Update setting
	m_strStatus = strNewStatus;

	// Refresh the dialog
	m_bStatusUpdated = true;
	doRefresh();
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void ProgressData::doRefresh()
{
	// Check tick count
	DWORD dwTick = GetTickCount();

	// Only refresh every 250 ms
	if ((m_dwLastUpdate == 0) || (dwTick - m_dwLastUpdate > gulREFRESHTIMER_MS))
	{
		// Update tick count
		m_dwLastUpdate = dwTick;

		// Pump message queue
		pumpMessageQueue();

		// Provide update to dialog
		// TODO: Also consider m_bStatusUpdated flag
		if (m_bPercentUpdated)
		{
			::PostMessage( m_hwndDialog, WM_APP+124, m_nProgress, NULL );

			m_bPercentUpdated = false;
		}
	}
}
//-------------------------------------------------------------------------------------------------
