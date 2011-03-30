#include "stdafx.h"
#include "GenericTask.h"
#include "UCLIDException.h"

#include <string>

using namespace std;

GenericTask::GenericTask(const string& strObjectName)
:NamedObject(strObjectName)
{
	pExceptionHandler = NULL;
	pThread = NULL;
	lock.acquire();
}

GenericTask::~GenericTask()
{
	try
	{
		// TODO: delete the thread?
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16382");
}

void GenericTask::runSynchronously()
{
	Win32SemaphoreLockGuard lockGuard(lock, false);
	perform();
}

UINT genericTaskRunnerThread(LPVOID pParam)
{
	GenericTask *pGenericTask = (GenericTask *) pParam;

	try
	{
		try
		{
			pGenericTask->runSynchronously();
		}
		catch (UCLIDException& uclidException)
		{
			pGenericTask->handleException(uclidException);
		}
		catch (...)
		{
			UCLIDException uclidException("ELI00515", "Unexpected exception caught in genericTaskRunnerThread()!");
			pGenericTask->handleException(uclidException);
		}
	}
	catch (UCLIDException& uclidException)
	{
		string strMsg = "Internal error: Exception handler was unable to handle exception gracefully!";
		strMsg += "\n";
		strMsg += uclidException.getTopText();
		strMsg += "\nCode: ";
		string sErrMsg;
		uclidException.asString(sErrMsg);
		strMsg += sErrMsg;
		AfxMessageBox(strMsg.c_str());
	}
	catch (...)
	{
		string strMsg = "Internal error: Exception handler was unable to handle exception gracefully!";
		strMsg += "\nUnexpected exception caught!";
		AfxMessageBox(strMsg.c_str());
	}

	return 0;
}

void GenericTask::runAsynchronously(UCLIDExceptionHandler *_pExceptionHandler)
{
	ASSERT_ARGUMENT("ELI00516", _pExceptionHandler != __nullptr);
	pExceptionHandler = _pExceptionHandler;

	try
	{
		pThread = AfxBeginThread(genericTaskRunnerThread, this);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI02980")
}

void GenericTask::waitForCompletion()
{
	Win32SemaphoreLockGuard lockGuard(lock);
}

void GenericTask::handleException(const UCLIDException& uclidException)
{
	pExceptionHandler->handleException(uclidException);
}

void GenericTask::stop()
{
	if (pThread != __nullptr)
	{
		pThread->SuspendThread();
	}
}
