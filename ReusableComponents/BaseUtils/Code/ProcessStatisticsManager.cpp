
#include "stdafx.h"
#include "BaseUtils.h"
#include "ProcessStatisticsManager.h"
#include "UCLIDException.h"
#include "cpputil.h"
#include "COMUtils.h"

#include <psapi.h>
#include <comutil.h>
#include <atlconv.h>
#include <set>
#include <vector>

using namespace std;

#pragma comment(lib, "wbemuuid.lib")

//--------------------------------------------------------------------------------------------------
// IndividualProcessStatistics class
//--------------------------------------------------------------------------------------------------
IndividualProcessStatistics::IndividualProcessStatistics() :
m_strProcessName(""),
m_dwProcessID(0),
m_tCurrentTime(0),
m_dwTotalMemoryBytes(0),
m_dwAllocatedVirtualMemoryBytes(0),
m_dwHandleCount(0),
m_dwThreadCount(0)
{
}
//--------------------------------------------------------------------------------------------------
IndividualProcessStatistics::IndividualProcessStatistics(const string& rstrProcessName, 
														 DWORD dwProcessID,
														 __time64_t tCurrentTime,
														 DWORD dwTotalMemory, 
														 DWORD dwAllocatedVirtualMemory,
														 DWORD dwHandleCount, 
														 DWORD dwThreadCount) :
m_strProcessName(rstrProcessName),
m_dwProcessID(dwProcessID),
m_tCurrentTime(tCurrentTime),
m_dwTotalMemoryBytes(dwTotalMemory),
m_dwAllocatedVirtualMemoryBytes(dwAllocatedVirtualMemory),
m_dwHandleCount(dwHandleCount),
m_dwThreadCount(dwThreadCount)
{
}
//--------------------------------------------------------------------------------------------------
string IndividualProcessStatistics::getKeyValue()
{
	string strKeyValue= m_strProcessName + "." + asString(m_dwProcessID);
	return strKeyValue;
}
//--------------------------------------------------------------------------------------------------
IndividualProcessStatistics& IndividualProcessStatistics::operator += (
													const IndividualProcessStatistics& ripsNewStats)
{
	m_dwTotalMemoryBytes = ripsNewStats.m_dwTotalMemoryBytes + m_dwTotalMemoryBytes;
	m_dwAllocatedVirtualMemoryBytes = ripsNewStats.m_dwAllocatedVirtualMemoryBytes +
		m_dwAllocatedVirtualMemoryBytes;
	m_dwHandleCount = ripsNewStats.m_dwHandleCount + m_dwHandleCount;
	m_dwThreadCount = ripsNewStats.m_dwThreadCount + m_dwThreadCount;

	return *this;
}
//--------------------------------------------------------------------------------------------------
bool IndividualProcessStatistics::operator == (const IndividualProcessStatistics& ripsNewStats)
{
	bool bResult = (ripsNewStats.m_strProcessName == m_strProcessName) &&
		(ripsNewStats.m_dwProcessID == m_dwProcessID) &&
		(ripsNewStats.m_dwTotalMemoryBytes == m_dwTotalMemoryBytes) &&
		(ripsNewStats.m_dwAllocatedVirtualMemoryBytes == m_dwAllocatedVirtualMemoryBytes) &&
		(ripsNewStats.m_dwHandleCount == m_dwHandleCount) &&
		(ripsNewStats.m_dwThreadCount == m_dwThreadCount);

	return bResult;
}
//--------------------------------------------------------------------------------------------------
bool IndividualProcessStatistics::operator != (const IndividualProcessStatistics& ripsNewStats)
{
	IndividualProcessStatistics& rThis = *this;
	bool rThisEqualsArgument = (rThis == ripsNewStats);
	return (!rThisEqualsArgument);
}

//--------------------------------------------------------------------------------------------------
// ProcessStatisticsManager class
//--------------------------------------------------------------------------------------------------
ProcessStatisticsManager::ProcessStatisticsManager()
:m_pRefresher(NULL), m_pEnum(NULL)
{
	// This is 1/2 the magic from the MSDN link http://msdn2.microsoft.com/en-us/library/aa384724.aspx
	// To add error checking,
	// check returned HRESULT below where collected.

	HRESULT hr = 0;

	if (FAILED (hr = CoInitializeSecurity( NULL, -1, NULL, NULL, RPC_C_AUTHN_LEVEL_NONE, 
		RPC_C_IMP_LEVEL_IMPERSONATE, NULL, EOAC_NONE, 0)))
	{
		UCLIDException ue("ELI16680", "FAILED CoInitializeSecurity!");
		ue.addHresult(hr);
		throw ue;
	}

	CComQIPtr<IWbemLocator> ipWbemLocator;
	if (FAILED (hr = CoCreateInstance(CLSID_WbemLocator, NULL, CLSCTX_INPROC_SERVER, 
		IID_IWbemLocator, (void**) &ipWbemLocator)))
	{
		UCLIDException ue("ELI16681", "FAILED CoCreateInstance!");
		ue.addHresult(hr);
		throw ue;
	}

	CComBSTR bstrNameSpace;
	bstrNameSpace.Attach(SysAllocString(L"\\\\.\\root\\cimv2"));
	// Connect to the desired namespace.
	CComQIPtr<IWbemServices> ipNameSpace;

	if (FAILED (hr = ipWbemLocator->ConnectServer(bstrNameSpace, NULL, NULL, NULL, 0L, 
		NULL, NULL, &ipNameSpace)))
	{
		UCLIDException ue("ELI16683", "FAILED Connecting to server!");
		ue.addHresult(hr);
		throw ue;
	}

	if (FAILED (hr = CoCreateInstance(CLSID_WbemRefresher, NULL, CLSCTX_INPROC_SERVER, 
		IID_IWbemRefresher, (void**) &m_pRefresher)))
	{
		UCLIDException ue("ELI16684", "FAILED CoCreateInstance!");
		ue.addHresult(hr);
		throw ue;
	}
	
	CComQIPtr<IWbemConfigureRefresher> ipConfig;
	if (FAILED (hr = m_pRefresher->QueryInterface(IID_IWbemConfigureRefresher, (void **)&ipConfig)))
	{
		UCLIDException ue("ELI16685", "FAILED Getting Refresher Query Interface!");
		ue.addHresult(hr);
		throw ue;
	}

	// Add an enumerator to the refresher.
	// http://msdn2.microsoft.com/en-us/library/aa394323.aspx shows you what fields are available in
	// Win32_PerfRawData_PerfProc_Process
	long lID=0;
	if (FAILED (hr = ipConfig->AddEnum(ipNameSpace, L"Win32_PerfRawData_PerfProc_Process",0, 
		NULL, &m_pEnum, &lID)))
	{
		UCLIDException ue("ELI16686", "FAILED Adding enumerator to refresher!");
		ue.addHresult(hr);
		throw ue;
	}
}

//--------------------------------------------------------------------------------------------------
ProcessStatisticsManager::~ProcessStatisticsManager()
{
	try
	{
		if (m_pEnum != __nullptr)
		{
			m_pEnum->Release();
			m_pEnum = NULL;
		}
		if (m_pRefresher != __nullptr)
		{
			m_pRefresher->Release();
			m_pRefresher = NULL;
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16953");
}
//--------------------------------------------------------------------------------------------------
// Public Methods
//--------------------------------------------------------------------------------------------------
__time64_t ProcessStatisticsManager::getProcessStatistics(
										vector<IndividualProcessStatistics>& rvecProcessStats,
										const set<long>& rsetPID, const set<string>& rsetPName)
{
	// declared outside try catch block because if we hit the catch we need to release this memory
	IWbemObjectAccess **apEnumAccess = NULL;
	DWORD dwNumReturned = 0;

	// this is the other 1/2 of the magic from the MSDN link http://msdn2.microsoft.com/en-us/library/aa384724.aspx
	try
	{
		HRESULT hr=0;
		if (FAILED (hr = m_pRefresher->Refresh(0L)))
		{
			UCLIDException ue("ELI16688", "FAILED While refreshing the refresher!");
			ue.addHresult(hr);
			throw ue;
		}

		DWORD dwNumObjects = 0;

		// dummy call to GetObjects so we know how much memory to allocate
		// dwNumReturned will be set to the number of objects that we should get
		hr = m_pEnum->GetObjects(0L, dwNumObjects, apEnumAccess, &dwNumReturned);
		
		// dwNumReturned should never be 0. a 0 value would indicate no running processes
		if ( dwNumReturned == 0 )
		{
			THROW_LOGIC_ERROR_EXCEPTION("ELI17018");
		}

		// the error should be WBEM_E_BUFFER_TOO_SMALL since our buffer size was 0
		if ( (hr == WBEM_E_BUFFER_TOO_SMALL) && (dwNumReturned > dwNumObjects) )
		{
			// allocate the correct amount of space for the call to GetObjects
			apEnumAccess = new IWbemObjectAccess*[dwNumReturned];
			ASSERT_RESOURCE_ALLOCATION("ELI16689", apEnumAccess != __nullptr);

			// here we just make sure that the memory we have allocated is all set to zeroes
			SecureZeroMemory(apEnumAccess, dwNumReturned*sizeof(IWbemObjectAccess*));
			dwNumObjects = dwNumReturned;

			// now we make a real call to GetObjects, we should have enough space to handle them all
			if (FAILED (hr = m_pEnum->GetObjects(0L, dwNumObjects, apEnumAccess, &dwNumReturned)))
			{
				UCLIDException ue("ELI16690", "FAILED GetObjects from refresher enum!");
				ue.addHresult(hr);
				throw ue;
			}
		}
		else
		{
			if (hr == WBEM_S_NO_ERROR)
			{
				hr = WBEM_E_NOT_FOUND;
				UCLIDException ue("ELI16691", "FAILED GetObjects from refresher!");
				ue.addHresult(hr);
				throw ue;
			}

			UCLIDException ue("ELI25360", "Error getting objects from refresher!");
			ue.addHresult(hr);
			throw ue;
		}
	
		// get property handles - if you want more data, just add more handles - refer to
		// http://msdn2.microsoft.com/en-us/library/aa394323.aspx for a list of values
		// Modified 06/25/2008 - JDS - as per [LegacyRCAndUtils #4989] - changed to
		// get the VirtualBytes and PrivateBytes values as opposed to PageFileBytes
		// and WorkingSet.
		CIMTYPE VMSizeBytesType;
		long lVMSizeBytesHandle=0;
		getPropertyHandle(apEnumAccess[0], L"VirtualBytes", VMSizeBytesType, lVMSizeBytesHandle);

		CIMTYPE MemUsageBytesType;
		long lMemUsageBytesHandle=0;
		getPropertyHandle(apEnumAccess[0], L"PrivateBytes", MemUsageBytesType, lMemUsageBytesHandle);

		CIMTYPE ProcessHandleType;
		long lIDProcessHandle=0;
		getPropertyHandle(apEnumAccess[0], L"IDProcess", ProcessHandleType, lIDProcessHandle);

		CIMTYPE HandleCountType;
		long lHandleCountHandle=0;
		getPropertyHandle(apEnumAccess[0], L"HandleCount", HandleCountType, lHandleCountHandle);

		CIMTYPE ThreadCountType;
		long lThreadCountHandle=0;
		getPropertyHandle(apEnumAccess[0], L"ThreadCount", ThreadCountType, lThreadCountHandle);

		// get the current time once and use the same time for each IndividualProcessStatistic
		__time64_t tCurrentTime;
		_time64(&tCurrentTime);
		
		// loop through each of the processes and get the data
		for (DWORD i = 0; i < dwNumReturned; i++)
		{
			// the process name is stored in a variant
			_variant_t vProcessName;
			if (FAILED (hr = apEnumAccess[i]->Get(L"Name", 0, &vProcessName, 0, 0)))
			{
				UCLIDException ue("ELI16698", "FAILED getting process name!");
				ue.addHresult(hr);
				throw ue;
			}

			// Get(L"Name"...) call above returns a VARIANT which holds a BSTR containing
			// the process name, convert this to a stl::string so we can process it
			string strProcessName = asString(vProcessName.bstrVal);

			// on Server 2008 the OS will assign #\d+ to running processes if they
			// have the same name.  we need to check for #\d+ at the end
			// of a process name and if we find it, remove it before doing our
			// comparison.
			size_t szPoundPos = strProcessName.find_last_of("#");
		
			// found a # in the process name
			if (szPoundPos != string::npos)
			{
				try
				{
					// get the characters after the # 
					string strEnd = strProcessName.substr(szPoundPos+1);

					// try converting to an unsigned long, if it works then it is a number
					asUnsignedLong(strEnd);

					// since we found #\d+ strip these characters
					strProcessName.erase(szPoundPos);
				}
				catch(...)
				{
					// exception indicates the characters after the # were not all digits
					// or # is the last character, either way we can eat the exception and leave 
					// the strProcessName intact
				}
			}

			// make it lowercase so that we can do a case insensitive compare
			makeLowerCase(strProcessName);
			
			// now get the processID
			DWORD dwIDProcess = 0;
			readWord(apEnumAccess[i],lIDProcessHandle, dwIDProcess);

			// now we check to see if we are logging data for the current
			// process in the list, if it is not one we are monitoring
			// we just continue, no sense gathering data for processes we don't care about
			if( (rsetPID.find(dwIDProcess) == rsetPID.end() ) &&
				(rsetPName.find(strProcessName) == rsetPName.end()) )
			{
				// since we are going to continue we will miss the cleanup code at the bottom
				// clean up then continue
				apEnumAccess[i]->Release();
				apEnumAccess[i] = NULL;
				continue;
			}

			// now read all the data we are tracking
			DWORD dwMemUsageBytes = 0;
			readWord(apEnumAccess[i], lMemUsageBytesHandle, dwMemUsageBytes);
			DWORD dwVMSizeBytes = 0;
			readWord(apEnumAccess[i], lVMSizeBytesHandle, dwVMSizeBytes);
			DWORD dwHandleCount = 0;
			readWord(apEnumAccess[i], lHandleCountHandle, dwHandleCount);
			DWORD dwThreadCount = 0;
			readWord(apEnumAccess[i], lThreadCountHandle, dwThreadCount);

			// make a new IndividualProcessStatistic and store it in our vector
			IndividualProcessStatistics ipsThisPIDStat(strProcessName, dwIDProcess, tCurrentTime,
				dwMemUsageBytes, dwVMSizeBytes, dwHandleCount, dwThreadCount);
			rvecProcessStats.push_back(ipsThisPIDStat);		

			// clean up as we go
			apEnumAccess[i]->Release();
			apEnumAccess[i] = NULL;
		}

		if (apEnumAccess == NULL)
		{
			UCLIDException ue("ELI16779", "apEnumAccess was NULL!");
			throw ue;
		}

		delete [] apEnumAccess;
		apEnumAccess = NULL;

		return tCurrentTime;
	}
	catch(...)
	{
		if (apEnumAccess != __nullptr)
		{
			for (DWORD i = 0; i < dwNumReturned; i++)
			{
				// we need to release the memory allocated we include a try catch that will
				// simply eat any exceptions thrown during the release, this way if one
				// fails we at least attempt to clean up the rest
				try
				{
					if (apEnumAccess[i] != __nullptr)
					{
						apEnumAccess[i]->Release();
						apEnumAccess[i] = NULL;
					}
				}
				catch(...) {}
			}
			delete [] apEnumAccess;
		}
		throw;
	}
}
//--------------------------------------------------------------------------------------------------
// Private Methods
//--------------------------------------------------------------------------------------------------
void ProcessStatisticsManager::getPropertyHandle(IWbemObjectAccess* pEnumAccess, LPCWSTR wszPropertyName,
											CIMTYPE& rctPropertyType, long& rlPropertyHandle)
{
	HRESULT hr=0;

	if (FAILED (hr = pEnumAccess->GetPropertyHandle(wszPropertyName, &rctPropertyType, &rlPropertyHandle)))
	{
		UCLIDException ue("ELI16692", "FAILED getting handle!");
		// need to conver the wszPropertyName to a std::string
		USES_CONVERSION;
		CComBSTR bsTemp = W2BSTR(wszPropertyName);
		string strPropertyName = asString(bsTemp);
		ue.addDebugInfo("Property Name", strPropertyName);
		ue.addHresult(hr);
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
void ProcessStatisticsManager::readWord(IWbemObjectAccess* pEnumAccess, long& rlHandle, DWORD& rdwData)
{
	HRESULT hr = 0;
	if (FAILED (hr = pEnumAccess->ReadDWORD(rlHandle, &rdwData)))
	{
		UCLIDException ue("ELI16699", "FAILED reading DWORD!");
		ue.addDebugInfo("Handle", rlHandle);
		ue.addHresult(hr);
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------