#include <windows.h>
#include <stdio.h>
#include <comdef.h>	// for using bstr_t class
#include <vector>
#include <winperf.h>

#define TOTALBYTES    100*1024
#define BYTEINCREMENT 10*1024

template <class T>
class CPerfCounters
{
public:
	CPerfCounters()
	{
	}
	~CPerfCounters()
	{
	}

	T GetCounterValue(unique_ptr<PERF_DATA_BLOCK> &rapPerfData, DWORD dwObjectIndex, DWORD dwCounterIndex, LPCTSTR pInstanceName = NULL)
	{
		QueryPerformanceData(rapPerfData, dwObjectIndex, dwCounterIndex);

		T lnValue = -1;

		// Get the first object type.
		PPERF_OBJECT_TYPE pPerfObj = FirstObject( rapPerfData.get() );

		// Look for the given object index

		for( DWORD i=0; i < rapPerfData->NumObjectTypes; i++ )
		{

			if (pPerfObj->ObjectNameTitleIndex == dwObjectIndex)
			{
				lnValue = GetCounterValue(pPerfObj, dwCounterIndex, pInstanceName);
				break;
			}

			pPerfObj = NextObject( pPerfObj );
		}
		return lnValue;
	}

	T GetCounterValueForProcessID(unique_ptr<PERF_DATA_BLOCK> &rapPerfData, DWORD dwObjectIndex, DWORD dwCounterIndex, DWORD dwProcessID)
	{
		QueryPerformanceData(rapPerfData, dwObjectIndex, dwCounterIndex);

		T lnValue = -1;

		// Get the first object type.
		PPERF_OBJECT_TYPE pPerfObj = FirstObject( rapPerfData.get() );

		// Look for the given object index
		for( DWORD i=0; i < rapPerfData->NumObjectTypes; i++ )
		{

			if (pPerfObj->ObjectNameTitleIndex == dwObjectIndex)
			{
				lnValue = GetCounterValueForProcessID(pPerfObj, dwCounterIndex, dwProcessID);
				break;
			}

			pPerfObj = NextObject( pPerfObj );
		}
		return lnValue;
	}

protected:

	//
	//	The performance data is accessed through the registry key 
	//	HKEY_PEFORMANCE_DATA.
	//	However, although we use the registry to collect performance data, 
	//	the data is not stored in the registry database.
	//	Instead, calling the registry functions with the HKEY_PEFORMANCE_DATA key 
	//	causes the system to collect the data from the appropriate system 
	//	object managers.
	//
	//	QueryPerformanceData allocates memory block for getting the
	//	performance data.
	//
	//
	void QueryPerformanceData(unique_ptr<PERF_DATA_BLOCK> &rapPerfData, DWORD dwObjectIndex, DWORD dwCounterIndex)
	{
		dwCounterIndex;		// unused parameter
		
		DWORD dwBufferSize = TOTALBYTES;
		rapPerfData.reset((PPERF_DATA_BLOCK)new BYTE[dwBufferSize]);
		ZeroMemory(rapPerfData.get(), dwBufferSize);

		LONG lRes;

		char keyName[32];
		sprintf_s(keyName, sizeof(keyName) * sizeof(char), "%d", dwObjectIndex);

		while( (lRes = RegQueryValueEx( HKEY_PERFORMANCE_DATA,
								   keyName,
								   NULL,
								   NULL,
								   (LPBYTE)rapPerfData.get(),
								   &dwBufferSize )) == ERROR_MORE_DATA )
		{
			// Increase the size of the buffer and try again.
			dwBufferSize += BYTEINCREMENT;
			rapPerfData.reset((PPERF_DATA_BLOCK)new BYTE[dwBufferSize]);
			ZeroMemory(rapPerfData.get(), dwBufferSize);
		}

		RegCloseKey(HKEY_PERFORMANCE_DATA);
	}

	//
	//	GetCounterValue gets performance object structure
	//	and returns the value of given counter index .
	//	This functions iterates through the counters of the input object
	//	structure and looks for the given counter index.
	//
	//	For objects that have instances, this function returns the counter value
	//	of the instance pInstanceName.
	//
	T GetCounterValue(PPERF_OBJECT_TYPE pPerfObj, DWORD dwCounterIndex, LPCTSTR pInstanceName)
	{
		PPERF_COUNTER_DEFINITION pPerfCntr = NULL;
		PPERF_INSTANCE_DEFINITION pPerfInst = NULL;
		PPERF_COUNTER_BLOCK pCounterBlock = NULL;

		// Get the first counter.

		pPerfCntr = FirstCounter( pPerfObj );

		for( DWORD j=0; j < pPerfObj->NumCounters; j++ )
		{
			if (pPerfCntr->CounterNameTitleIndex == dwCounterIndex)
				break;

			// Get the next counter.

			pPerfCntr = NextCounter( pPerfCntr );
		}

		if( pPerfObj->NumInstances == PERF_NO_INSTANCES )		
		{
			pCounterBlock = (PPERF_COUNTER_BLOCK) ((LPBYTE) pPerfObj + pPerfObj->DefinitionLength);
		}
		else
		{
			pPerfInst = FirstInstance( pPerfObj );
		
			// Look for instance pInstanceName
			_bstr_t bstrInstance;
			_bstr_t bstrInputInstance = pInstanceName;
			for( int k=0; k < pPerfObj->NumInstances; k++ )
			{
				bstrInstance = (wchar_t *)((PBYTE)pPerfInst + pPerfInst->NameOffset);
				if (!_stricmp((LPCTSTR)bstrInstance, (LPCTSTR)bstrInputInstance))
				{
					pCounterBlock = (PPERF_COUNTER_BLOCK) ((LPBYTE) pPerfInst + pPerfInst->ByteLength);
					break;
				}
				
				// Get the next instance.

				pPerfInst = NextInstance( pPerfInst );
			}
		}

		if (pCounterBlock)
		{
			T *lnValue = NULL;
			lnValue = (T*)((LPBYTE) pCounterBlock + pPerfCntr->CounterOffset);
			return *lnValue;
		}
		return -1;
	}


	T GetCounterValueForProcessID(PPERF_OBJECT_TYPE pPerfObj, DWORD dwCounterIndex, DWORD dwProcessID)
	{
		unsigned int PROC_ID_COUNTER = 784;

		BOOL	bProcessIDExist = FALSE;
		PPERF_COUNTER_DEFINITION pPerfCntr = NULL;
		PPERF_COUNTER_DEFINITION pTheRequestedPerfCntr = NULL;
		PPERF_COUNTER_DEFINITION pProcIDPerfCntr = NULL;
		PPERF_INSTANCE_DEFINITION pPerfInst = NULL;
		PPERF_COUNTER_BLOCK pCounterBlock = NULL;

		// Get the first counter.

		pPerfCntr = FirstCounter( pPerfObj );

		for( DWORD j=0; j < pPerfObj->NumCounters; j++ )
		{
			if (pPerfCntr->CounterNameTitleIndex == PROC_ID_COUNTER)
			{
				pProcIDPerfCntr = pPerfCntr;
				if (pTheRequestedPerfCntr)
					break;
			}

			if (pPerfCntr->CounterNameTitleIndex == dwCounterIndex)
			{
				pTheRequestedPerfCntr = pPerfCntr;
				if (pProcIDPerfCntr)
					break;
			}

			// Get the next counter.

			pPerfCntr = NextCounter( pPerfCntr );
		}

		if( pPerfObj->NumInstances == PERF_NO_INSTANCES )		
		{
			pCounterBlock = (PPERF_COUNTER_BLOCK) ((LPBYTE) pPerfObj + pPerfObj->DefinitionLength);
		}
		else
		{
			pPerfInst = FirstInstance( pPerfObj );
		
			for (int k = 0; k < pPerfObj->NumInstances; k++)
			{
				pCounterBlock = (PPERF_COUNTER_BLOCK) ((LPBYTE) pPerfInst + pPerfInst->ByteLength);
				if (pCounterBlock)
				{
					unsigned int processID  = 0;
					processID = (unsigned int) *(T*)((LPBYTE) pCounterBlock + pProcIDPerfCntr->CounterOffset);
					if (processID == dwProcessID)
					{
						bProcessIDExist = TRUE;
						break;
					}
				}
				
				// Get the next instance.

				pPerfInst = NextInstance( pPerfInst );
			}
		}

		if (bProcessIDExist && pCounterBlock)
		{
			T *lnValue = NULL;
			lnValue = (T*)((LPBYTE) pCounterBlock + pTheRequestedPerfCntr->CounterOffset);
			return *lnValue;
		}
		return -1;
	}


	/*****************************************************************
	 *                                                               *
	 * Functions used to navigate through the performance data.      *
	 *                                                               *
	 *****************************************************************/

	PPERF_OBJECT_TYPE FirstObject( PPERF_DATA_BLOCK PerfData )
	{
		return( (PPERF_OBJECT_TYPE)((PBYTE)PerfData + PerfData->HeaderLength) );
	}

	PPERF_OBJECT_TYPE NextObject( PPERF_OBJECT_TYPE PerfObj )
	{
		return( (PPERF_OBJECT_TYPE)((PBYTE)PerfObj + PerfObj->TotalByteLength) );
	}

	PPERF_COUNTER_DEFINITION FirstCounter( PPERF_OBJECT_TYPE PerfObj )
	{
		return( (PPERF_COUNTER_DEFINITION) ((PBYTE)PerfObj + PerfObj->HeaderLength) );
	}

	PPERF_COUNTER_DEFINITION NextCounter( PPERF_COUNTER_DEFINITION PerfCntr )
	{
		return( (PPERF_COUNTER_DEFINITION)((PBYTE)PerfCntr + PerfCntr->ByteLength) );
	}

	PPERF_INSTANCE_DEFINITION FirstInstance( PPERF_OBJECT_TYPE PerfObj )
	{
		return( (PPERF_INSTANCE_DEFINITION)((PBYTE)PerfObj + PerfObj->DefinitionLength) );
	}

	PPERF_INSTANCE_DEFINITION NextInstance( PPERF_INSTANCE_DEFINITION PerfInst )
	{
		PPERF_COUNTER_BLOCK PerfCntrBlk;

		PerfCntrBlk = (PPERF_COUNTER_BLOCK)((PBYTE)PerfInst + PerfInst->ByteLength);

		return( (PPERF_INSTANCE_DEFINITION)((PBYTE)PerfCntrBlk + PerfCntrBlk->ByteLength) );
	}
};