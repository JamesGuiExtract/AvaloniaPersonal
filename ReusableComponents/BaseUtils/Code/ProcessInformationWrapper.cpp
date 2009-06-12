#include "stdafx.h"
#include "ProcessInformationWrapper.h"
#include "UCLIDException.h"

//-------------------------------------------------------------------------------------------------
ProcessInformationWrapper::ProcessInformationWrapper()
{
	// Explicitly clear the structure
	// otherwise unused handles are closed in debug mode destructor
	memset( &pi, 0, sizeof( PROCESS_INFORMATION ) );
}
//-------------------------------------------------------------------------------------------------
ProcessInformationWrapper::~ProcessInformationWrapper()
{
	try
	{
		// Close the handles
		if (pi.hThread != NULL)
		{
			CloseHandle( pi.hThread );
		}

		if (pi.hProcess != NULL)
		{
			CloseHandle( pi.hProcess );
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16394");
}
//-------------------------------------------------------------------------------------------------
