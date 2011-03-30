#include "stdafx.h"
#include "ProgressDlgTaskRunner.h"
#include "cpputil.h"
#include "TemporaryResourceOverride.h"

extern AFX_EXTENSION_MODULE BaseUtilsDLL;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif
//-------------------------------------------------------------------------------------------------
// ProgressDlgTaskRunner
//-------------------------------------------------------------------------------------------------
ProgressDlgTaskRunner::ProgressDlgTaskRunner(IProgressTask* pProgressTask, int nTaskID, bool bShowDialog, bool bShowCancel) 
: m_pProgressTask(pProgressTask), m_dlgProgress(NULL, false), m_nTaskID(nTaskID),m_bShowDialog(bShowDialog)
{
	m_dlgProgress.showCancel(bShowCancel);
	if (m_bShowDialog)
	{
		m_dlgProgress.show();
	}
	else
	{
		m_dlgProgress.hide();
	}
}
//-------------------------------------------------------------------------------------------------
ProgressDlgTaskRunner::~ProgressDlgTaskRunner()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16395");
}
//-------------------------------------------------------------------------------------------------
void ProgressDlgTaskRunner::showDialog(bool bShow)
{
	m_bShowDialog = bShow;
	if (m_bShowDialog)
	{
		m_dlgProgress.show();
	}
	else
	{
		m_dlgProgress.hide();
	}
}
//-------------------------------------------------------------------------------------------------
void ProgressDlgTaskRunner::run()
{
	TemporaryResourceOverride rcOverride(BaseUtilsDLL.hResource);
	if (m_bShowDialog)
	{
		CWinThread* pThread = AfxBeginThread(threadFunc, this, THREAD_PRIORITY_NORMAL, 0, CREATE_SUSPENDED);
		pThread->m_bAutoDelete = false;
		pThread->ResumeThread();

		int ret = 1;
		ret = m_dlgProgress.DoModal();

		// I think this is causing a very difficult to reproduce bug
		// The problem is that the thread exits(is destroyed) before the
		// wait executes.  When m_hThread is invalid the behavior is undefined
		// meaning it could be hanging the entire

		// make sure the thread has exited
		WaitForSingleObject(pThread->m_hThread, INFINITE);
		delete pThread;
		pThread = NULL;

		// If the return code is 0 it means an exception was thrown
		// thus we should rethrow it
		if (ret == 0)
		{
			m_exception.addDebugInfo("RethrowID", "ELI09106");
			throw m_exception;
		}
	}
	else
	{
		m_pProgressTask->runTask(this, m_nTaskID);
	}
}
//-------------------------------------------------------------------------------------------------
UINT ProgressDlgTaskRunner::threadFunc(LPVOID pParam)
{
	try
	{
		int retCode = 1;
		ProgressDlgTaskRunner* pTaskRunner = (ProgressDlgTaskRunner*)pParam;
		ASSERT_RESOURCE_ALLOCATION("ELI25239", pTaskRunner != __nullptr);
		CoInitializeEx(NULL, COINIT_MULTITHREADED);
		try
		{
			try
			{
				IProgressTask* pProgressTask = pTaskRunner->m_pProgressTask;
				ASSERT_RESOURCE_ALLOCATION("ELI25240", pProgressTask != __nullptr);
				pProgressTask->runTask(pTaskRunner, pTaskRunner->m_nTaskID);
			}
			// We need a UCLIDException that we can save to rethrow from the main thread 
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI09105");
		}
		catch(UCLIDException ue)
		{
			pTaskRunner->m_exception = ue;
			// a return code of zero will indicate that 
			// the thread exited because of an exception
			retCode = 0;
		}
		catch(...)
		{
			pTaskRunner->m_exception = UCLIDException("ELI10287", "Unexpected Exception!");
			// a return code of zero will indicate that 
			// the thread exited because of an exception
			retCode = 0;
		}
		
		// Signal the main thread dialog box that it should terminate with 
		// the specified return code
		CoUninitialize();
		// We have to wait for the dialog to be initialized otherwise it is possible that 
		// we will attempt to kill it before it is created.  If that happens the dialog will not be closed
		// and the app will hang
		pTaskRunner->m_dlgProgress.waitForInit();
		pTaskRunner->m_dlgProgress.kill(retCode);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10297");
	return 0;
}
//-------------------------------------------------------------------------------------------------
// IProgress
//-------------------------------------------------------------------------------------------------
void ProgressDlgTaskRunner::setPercentComplete(double fPercent)
{
	m_dlgProgress.setPercentComplete(fPercent);
}
//-------------------------------------------------------------------------------------------------
void ProgressDlgTaskRunner::setTitle(std::string strTitle)
{
	m_dlgProgress.setTitle(strTitle);
}
//-------------------------------------------------------------------------------------------------
void ProgressDlgTaskRunner::setText(std::string strText)
{
	m_dlgProgress.setText(strText);
}
//-------------------------------------------------------------------------------------------------
bool ProgressDlgTaskRunner::userCanceled()
{
	return m_dlgProgress.userCanceled();	
}