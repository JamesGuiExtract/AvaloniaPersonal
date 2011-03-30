#include "stdafx.h"
#include "TaskManager.h"
#include "UCLIDException.h"

#include "GenericTask.h"

TaskManager::TaskManager()
{
}

TaskManager::~TaskManager()
{
	try
	{
		vector<GenericTask *>::const_iterator iter;
		for (iter = vecTasks.begin(); iter != vecTasks.end(); iter++)
			delete (*iter);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16400");
}

void TaskManager::addTask(GenericTask *pTask)
{
	CSingleLock guard( &lock, TRUE );

	vecTasks.push_back(pTask);
}

void TaskManager::run()
{
	GenericTask *pNextTask = NULL;
	do
	{
		// get the next task
		pNextTask = getNextTask();

		// if the next task is available, then run it, otherwise, break from this loop.
		if (pNextTask != __nullptr)
			pNextTask->runSynchronously();
		else
			break;

	} while (true);
}

GenericTask* TaskManager::getNextTask()
{
	CSingleLock guard( &lock, TRUE );

	if (vecTasks.empty())
		return NULL;
	else
	{
		vector<GenericTask *>::iterator iter;
		iter = vecTasks.begin();
		GenericTask *pNextTask = *iter;
		vecTasks.erase(iter);
		return pNextTask;
	}
}
