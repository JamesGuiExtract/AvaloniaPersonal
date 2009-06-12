
#pragma once

#include "IFCore.h"

#include <string>
#include <vector>

//--------------------------------------------------------------------------------------------------
class WindowIRManager : public CWnd
{
public:
	WindowIRManager(IInputManager *pInputManager);

	void NotifyInputEnabled(const std::string& strInputType, 
		const std::string& strPrompt);
	void NotifyInputEnabled(HWND hWnd);

	void NotifyInputDisabled();
	void NotifyInputDisabled(HWND hWnd);

	DECLARE_MESSAGE_MAP()

	// Generated message map functions
	//{{AFX_MSG(WindowIRManager)
	afx_msg LRESULT OnConnectWindowIR(WPARAM wParam, LPARAM lParam);
	afx_msg LRESULT OnDisconnectWindowIR(WPARAM wParam, LPARAM lParam);
	afx_msg LRESULT OnProcessInput(WPARAM wParam, LPARAM lParam);
	//}}AFX_MSG

private:
	IInputManager *m_pInputManager;
	std::vector<HWND> m_vecIRWndHandles;

	bool m_bInputEnabled;
	std::string m_strInputType, m_strPrompt;
};
//--------------------------------------------------------------------------------------------------
