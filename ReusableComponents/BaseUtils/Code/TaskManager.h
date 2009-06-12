#pragma once

#include "BaseUtils.h"

//#include "Win32Semaphore.h"
#include <afxmt.h>
#include <vector>

using namespace std;

class GenericTask;

class EXPORT_BaseUtils TaskManager
{
public:
	TaskManager();
	~TaskManager();
	void addTask(GenericTask* pTask);
	void run();
	void clear();
	GenericTask* getNextTask();

private:
	CMutex lock;
	vector <GenericTask*> vecTasks;
};
