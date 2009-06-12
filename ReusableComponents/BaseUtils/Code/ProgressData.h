// ProgressData.h : header file

#pragma once

//#include "Win32Mutex.h"
#include "BaseUtils.h"

#include <string>

class EXPORT_BaseUtils ProgressData
{
// Construction
public:
	ProgressData(HWND hwndDialog);

	// Get/Set methods for percent complete
	int getPercentComplete();
	void setPercentComplete(int iNewPercent);

	// Get/Set methods for status
	std::string	getStatus();
	void setStatus(std::string strNewStatus);

private:
	std::string m_strStatus;
	int m_nProgress;

	bool	m_bPercentUpdated;
	bool	m_bStatusUpdated;
	DWORD	m_dwLastUpdate;

	HWND	m_hwndDialog;

	void	doRefresh();

private:
	// Mutex to prevent multiple simultaneous calls from executing concurrently
//	Win32Mutex	m_mutex;
};
