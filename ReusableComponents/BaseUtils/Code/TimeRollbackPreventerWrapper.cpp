#include "stdafx.h"
#include "TimeRollbackPreventerWrapper.h"
#include "TimeRollbackPreventer.h"
#include "UCLIDException.h"

//-------------------------------------------------------------------------------------------------
TimeRollbackPreventerWrapper::TimeRollbackPreventerWrapper(Win32Event& rStateIsInvalidEvent)
:m_pTRP(NULL)
{
	m_pTRP = new TimeRollbackPreventer(rStateIsInvalidEvent);
	ASSERT_RESOURCE_ALLOCATION("ELI13018", m_pTRP != __nullptr);
}
//-------------------------------------------------------------------------------------------------
TimeRollbackPreventerWrapper::~TimeRollbackPreventerWrapper()
{
	try
	{
		// delete the time-rollback preventor object if it was created
		if (m_pTRP)
		{
			delete m_pTRP;
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16413");
}
//-------------------------------------------------------------------------------------------------
