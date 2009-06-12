#pragma once

#include "BaseUtils.h"
#include "IProgress.h"

// Any class that implements IProgressTask is responsible for
// making regular updates to pProgress from within runTask 

class EXPORT_BaseUtils IProgressTask
{
public:

	// the idea is that runTask will perform some lengthy task making regular updates
	// to pProgress
	// nTaskID allows the runTask function to implement multiple tasks and execute them 
	// based on the taskID
	virtual void runTask(IProgress* pProgress, int nTaskID) = 0;
};