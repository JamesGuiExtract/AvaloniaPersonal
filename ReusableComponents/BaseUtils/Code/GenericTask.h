#pragma once

#include "BaseUtils.h"
#include "Win32Semaphore.h"

class UCLIDExceptionHandler;
class CWinThread;

class EXPORT_BaseUtils GenericTask : public NamedObject
{
public:
	GenericTask(const string& strObjectName = "");
	virtual ~GenericTask();

	void runSynchronously();
	void runAsynchronously(UCLIDExceptionHandler* _pExceptionHandler);
	void waitForCompletion();
	void handleException(const UCLIDException& uclidException);

	// following function makes sense to call only when runAsynchronously() has
	// been called.
	virtual void stop();

protected:
	virtual void perform() = 0;
	
private:
	UCLIDExceptionHandler* pExceptionHandler;
	Win32Semaphore lock;
	CWinThread* pThread;
};