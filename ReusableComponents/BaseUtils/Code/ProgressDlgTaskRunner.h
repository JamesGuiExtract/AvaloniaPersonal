#pragma once

#include "BaseUtils.h"
#include "IProgressTask.h"
#include "ProgressDialog.h"
#include "UCLIDException.h"

#include <string>

using namespace std;

class EXPORT_BaseUtils ProgressDlgTaskRunner : public IProgress
{
public:
	ProgressDlgTaskRunner(IProgressTask* pTask, int nTaskID = 0, bool bShowDialog = true, bool bShowCancel = true);
	~ProgressDlgTaskRunner();
	
	// run the task
	// the IProgressTask that this 
	// method will run should not do any MFC
	// GUI stuff as the task is run in a user interface thread
	void run();
	void showDialog(bool bShow);

// IProgress methods
	void setPercentComplete(double fPercent);
	void setTitle(std::string strTitle);
	void setText(std::string strText);
	bool userCanceled();

private:
	static UINT threadFunc(LPVOID pParam);

	CProgressDlg m_dlgProgress;
	IProgressTask* m_pProgressTask;

	int m_nTaskID;

	bool m_bShowDialog;

	// If the task that is running throws an exception
	// it is stored here so that it can be rethrown 
	// in the main thread 
	UCLIDException m_exception;
};