
#include "stdafx.h"
#include "SuspendWindowupdates.h"
#include "UCLIDException.h"

//--------------------------------------------------------------------------------------------------
// SuspendWindowUpdates
//--------------------------------------------------------------------------------------------------
SuspendWindowUpdates::SuspendWindowUpdates(CWnd &rCWnd)
:m_rWnd(rCWnd)
{
	m_rWnd.LockWindowUpdate();
};
//--------------------------------------------------------------------------------------------------
SuspendWindowUpdates::~SuspendWindowUpdates()
{
	try
	{
		m_rWnd.UnlockWindowUpdate();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI17025");
};
//--------------------------------------------------------------------------------------------------
