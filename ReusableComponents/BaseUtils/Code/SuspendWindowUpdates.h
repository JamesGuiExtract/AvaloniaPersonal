#pragma once

#include "BaseUtils.h"

// PURPOSE: This class is used to lock windows updates for the scope of the class
//			The LockWindowUpdate method of the passed in references is called in the constructor
//			and the UnlockWindowUpdate method is called in the destructor
class EXPORT_BaseUtils SuspendWindowUpdates
{
public:
	SuspendWindowUpdates(CWnd &rCWnd);
	~SuspendWindowUpdates();
private:
	// Member set to the reference passed in the constructor
	CWnd &m_rWnd;
};