#include "stdafx.h"
#include "AFCore.h"
#include "RuleSetProfiler.h"

#include <UCLIDException.h>
#include <COMUtils.h>

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
const string gstrOutputFolder = "RuleSetProfilingData";

//--------------------------------------------------------------------------------------------------
// Statics
//--------------------------------------------------------------------------------------------------
CCriticalSection CRuleSetProfiler::ms_criticalSection;
volatile bool CRuleSetProfiler::ms_bEnabled = false;
volatile int CRuleSetProfiler::m_nActiveThreadCount = 0;
map<DWORD, CRuleSetProfiler::ProfilerStack> CRuleSetProfiler::m_mapThreadIDToProfilerStack;
map<DWORD, CRuleSetProfiler::ProfilerMap> CRuleSetProfiler::m_mapThreadIDToProfilerMap;

//--------------------------------------------------------------------------------------------------
// ProfilerID
//--------------------------------------------------------------------------------------------------
bool CRuleSetProfiler::ProfilerID::operator< (const ProfilerID& other) const
{
	if (m_nSubID != other.m_nSubID)
	{
		return m_nSubID < other.m_nSubID;
	}

	DWORD* pdwThisData = (DWORD*)&m_GUID;
	DWORD* pdwOtherData = (DWORD*)&other.m_GUID;
	for (int i = 0; i < 4; i++)
	{
		if (pdwThisData[i] != pdwOtherData[i])
		{
			return pdwThisData[i] < pdwOtherData[i];
		}
	}

	return false;
}
//--------------------------------------------------------------------------------------------------
bool CRuleSetProfiler::ProfilerID::operator!= (const ProfilerID& other) const
{
	return (m_GUID != other.m_GUID || m_nSubID != other.m_nSubID);
}

//--------------------------------------------------------------------------------------------------
// CRuleSetProfiler
//--------------------------------------------------------------------------------------------------
CRuleSetProfiler::CRuleSetProfiler()
: m_bActive(false)
, m_strName("")
, m_strType("")
, m_nCallCount(0)
, m_nTotalTime(0)
{
	// This constructor is used only for data storage; do not call startProfiling
}
//--------------------------------------------------------------------------------------------------
CRuleSetProfiler::CRuleSetProfiler(const CRuleSetProfiler& source)
: m_bActive(false)
, m_ID(source.m_ID)
, m_nHandle(source.m_nHandle)
, m_strName(source.m_strName)
, m_strType(source.m_strType)
, m_nCallCount(source.m_nCallCount)
, m_nTotalTime(source.m_nTotalTime)
{
	// This constructor is used only for data storage; do not call startProfiling
}
//--------------------------------------------------------------------------------------------------
CRuleSetProfiler::CRuleSetProfiler(const string& strName, const string& strType,
	UCLID_COMUTILSLib::IIdentifiableObjectPtr ipIdentifiableObject, int nSubID/* = 0*/)
: m_bActive(false)
, m_strName(strName)
, m_strType(strType)
, m_nCallCount(0)
, m_nTotalTime(0)
{
	try
	{
		if (ipIdentifiableObject == __nullptr)
		{
			// If this isn't an ipIdentifiableObject, it can't be profiled.
			return;
		}

		// Initialize the ID
		m_ID.m_GUID = ipIdentifiableObject->InstanceGUID;
		m_ID.m_nSubID = nSubID; 

		if (m_strType.empty())
		{
			// If the type is empty but the name ends with a string in angle brackets, treat that as
			// the type.
			int nTypePos = m_strName.find('<');
			if (nTypePos != string::npos && m_strName[m_strName.length() - 1] == '>')
			{
				m_strType = m_strName.substr(nTypePos + 1, m_strName.length() - nTypePos - 2);
				m_strName = m_strName.substr(0, nTypePos);
			}
			else
			{
				// Else, if the object is a categorized component, use the component description.
				ICategorizedComponentPtr ipCategorizedComponent(ipIdentifiableObject);
				if (ipCategorizedComponent != __nullptr)
				{
					m_strType = asString(ipCategorizedComponent->GetComponentDescription());
				}
			}
		}
	
		// If the name is empty, use the type as the name as well.
		if (m_strName.empty())
		{
			m_strName = m_strType;
		}

		startProfiling();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI33633");
}
//--------------------------------------------------------------------------------------------------
CRuleSetProfiler::CRuleSetProfiler(const string& strName, const string& strType, const GUID& guid,
	int nSubID/* = 0*/)
: m_bActive(false)
, m_strName(strName)
, m_strType(strType)
, m_nCallCount(0)
, m_nTotalTime(0)
{
	try
	{
		m_ID.m_GUID = guid;
		m_ID.m_nSubID = nSubID;

		startProfiling();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI33634");
}
//--------------------------------------------------------------------------------------------------
void CRuleSetProfiler::startProfiling()
{
	// Retrieve the call stack and object data for the current thread. After retrieving these,
	// we won't need to lock for this instance again.
	{
		CSingleLock lg(&ms_criticalSection, TRUE);

		// get the stack associated with the current thread
		// or create a new stack if this is the first time we're in the current thread
		DWORD dwThreadID = GetCurrentThreadId();
		m_pThreadProfilerStack = &m_mapThreadIDToProfilerStack[dwThreadID];
		m_pThreadProfilerMap = &m_mapThreadIDToProfilerMap[dwThreadID];

		// If the stack is currently empty, we are staring execution on a new thread. Increment
		// m_nActiveThreadCount.
		if (m_pThreadProfilerStack->empty())
		{
			m_nActiveThreadCount++;
		}
	}

	// Push this onto the call stack.
	m_pThreadProfilerStack->push(this);

	m_bActive = true;

	// Start the timer.
	m_nStartTime = GetTickCount();
}
//--------------------------------------------------------------------------------------------------
CRuleSetProfiler::~CRuleSetProfiler()
{
	try
	{
		// If this instance was actively collecting data, apply it now.
		if (m_bActive)
		{
			m_nCallCount++;
			m_nTotalTime = GetTickCount() - m_nStartTime;

			if (m_pThreadProfilerStack->top() != this)
			{
				throw UCLIDException("ELI33610", "Unbalanced profiler stack");
			}

			// Remove this call from the stack.
			m_pThreadProfilerStack->pop();

			// Find or create a persistent instance to collect data for all calls to this object.
			ProfilerMap::iterator iterPersistentCopy = m_pThreadProfilerMap->find(m_ID);
			if (iterPersistentCopy == m_pThreadProfilerMap->end())
			{
				(*m_pThreadProfilerMap)[m_ID] = CRuleSetProfiler(*this);
				iterPersistentCopy = m_pThreadProfilerMap->find(m_ID);
			}
			else
			{
				iterPersistentCopy->second.m_nCallCount++;
				iterPersistentCopy->second.m_nTotalTime += m_nTotalTime;
			}

			// If this call was not the last on the stack, record data concerning which objects
			// called this one and the total amount of time spent in those calls.
			if (!m_pThreadProfilerStack->empty())
			{
				ProfilerID callerID = m_pThreadProfilerStack->top()->m_ID;
				ProfilerMap::iterator callerData =
					iterPersistentCopy->second.m_mapCallerPerfData.find(callerID);
				if (callerData == iterPersistentCopy->second.m_mapCallerPerfData.end())
				{
					iterPersistentCopy->second.m_mapCallerPerfData[callerID] =
						CRuleSetProfiler(*m_pThreadProfilerStack->top());
					callerData = iterPersistentCopy->second.m_mapCallerPerfData.find(callerID);
				}
					
				callerData->second.m_nCallCount++;
				callerData->second.m_nTotalTime += m_nTotalTime;
			}
			else
			{
				// If this was the last object on the stack, we are not longer actively collecting data
				// for this thread; decrement m_nActiveThreadCount;
				CSingleLock lg(&ms_criticalSection, TRUE);
				m_nActiveThreadCount--;
			}
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI33620");
}
//--------------------------------------------------------------------------------------------------
bool CRuleSetProfiler::IsProfilingActiveOnThread()
{
	try
	{
		CSingleLock lg(&ms_criticalSection, TRUE);

		// Retrieve the call stack for the current thread.
		DWORD dwThreadID = GetCurrentThreadId();
		ProfilerStack *pThreadProfilerStack = &m_mapThreadIDToProfilerStack[dwThreadID];

		// If the stack is not empty, there is processing occurring on this thread.
		return !pThreadProfilerStack->empty();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI33636");
}
//--------------------------------------------------------------------------------------------------
void CRuleSetProfiler::GenerateOuput()
{
	try
	{
		CSingleLock lg(&ms_criticalSection, TRUE);

		// If profiling is not enabled, don't output anything.
		if (!ms_bEnabled)
		{
			return;
		}

		if (m_nActiveThreadCount > 0)
		{
			throw UCLIDException("ELI33611", "Cannot generate output while actively profiling");
		}

		// Collect the data from all threads.
		map<int, ProfilerID> objectHandles;
		ProfilerMap aggregateDataMap;
		for (map<DWORD, ProfilerMap>::iterator iterThreadData = m_mapThreadIDToProfilerMap.begin();
			 iterThreadData != m_mapThreadIDToProfilerMap.end();
			 iterThreadData++)
		{
			aggregateData(iterThreadData->second, aggregateDataMap, objectHandles);
		}

		// If there is no data, don't output anything.
		if (aggregateDataMap.empty())
		{
			return;
		}

		// Create an XML document object, and add "report", "summary" and "call" nodes.
		MSXML::IXMLDOMDocumentPtr ipDocument(CLSID_DOMDocument);
		ASSERT_RESOURCE_ALLOCATION("ELI33612", ipDocument != __nullptr);

		MSXML::IXMLDOMElementPtr ipReportNode = ipDocument->createElement("report");
		ASSERT_RESOURCE_ALLOCATION("ELI33613", ipReportNode != __nullptr);
		ipDocument->appendChild(ipReportNode);
		ipReportNode->setAttribute("version", 3);

		MSXML::IXMLDOMElementPtr ipSummaryNode = ipDocument->createElement("summary");
		ASSERT_RESOURCE_ALLOCATION("ELI33614", ipSummaryNode != __nullptr);
		ipReportNode->appendChild(ipSummaryNode);

		MSXML::IXMLDOMElementPtr ipCallsNode = ipDocument->createElement("calls");
		ASSERT_RESOURCE_ALLOCATION("ELI33615", ipCallsNode != __nullptr);
		ipReportNode->appendChild(ipCallsNode);

		// Populate the "summary" and "call" nodes.
		for (ProfilerMap::iterator iter = aggregateDataMap.begin();
			 iter != aggregateDataMap.end();
			 iter++)
		{
			ipSummaryNode->appendChild(createCallElement("total", ipDocument, iter->second));
			ipCallsNode->appendChild(createMethodElement(ipDocument, iter->second));
		}

		// Save the output, then prompt that data was output.
		string strOutputPath = getExtractApplicationDataPath() + "\\" + gstrOutputFolder;
		createDirectory(strOutputPath);

		CString zFileName = COleDateTime::GetCurrentTime().Format("%Y-%m-%d %H-%M-%S");
		string strOutputFileName = strOutputPath + "\\" + string((LPCTSTR)zFileName) + ".eqlog";
	
		ipDocument->save(strOutputFileName.c_str());

		MessageBox(__nullptr, string("Profiling data has been output output to:\r\n" + 
			strOutputFileName).c_str(), "Profiling Complete", MB_OK);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI33622");
}
//--------------------------------------------------------------------------------------------------
void CRuleSetProfiler::Reset()
{
	try
	{
		CSingleLock lg(&ms_criticalSection, TRUE);

		if (m_nActiveThreadCount > 0)
		{
			throw UCLIDException("ELI33616", "Cannot clear profiling data while actively profiling");
		}

		m_mapThreadIDToProfilerMap.clear();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI33623");
}
//--------------------------------------------------------------------------------------------------
void CRuleSetProfiler::aggregateData(const ProfilerMap& sourceData, ProfilerMap& rDestinationData,
	map<int, ProfilerID>& rmapObjectHandles, bool bIsCallerData/* = false*/)
{
	// Loop for every object in sourceData.
	for (ProfilerMap::const_iterator iterSourceData = sourceData.begin();
		 iterSourceData != sourceData.end();
		 iterSourceData++)
	{
		// Find or create a corresponding object in rDestinationData.
		ProfilerMap::iterator iterDestinationData = rDestinationData.find(iterSourceData->first);
		if (iterDestinationData == rDestinationData.end())
		{
			// Create the destination object with the same data as the source object
			rDestinationData[iterSourceData->first] = CRuleSetProfiler(iterSourceData->second);
			iterDestinationData = rDestinationData.find(iterSourceData->first);
			
			iterDestinationData->second.generateHandle(rmapObjectHandles);
		}
		else
		{
			// Add the source data to the destination data.
			iterDestinationData->second.m_nCallCount += iterSourceData->second.m_nCallCount;
			iterDestinationData->second.m_nTotalTime += iterSourceData->second.m_nTotalTime;
		}

		// If this is the initial call to aggregateData (to compile the overall call stats),
		// recurse to aggregate the caller data for this instance.
		if (!bIsCallerData)
		{
			aggregateData(iterSourceData->second.m_mapCallerPerfData,
				iterDestinationData->second.m_mapCallerPerfData, rmapObjectHandles, true);
		}
	}
}
//--------------------------------------------------------------------------------------------------
MSXML::IXMLDOMElementPtr CRuleSetProfiler::createCallElement(string strName,
	MSXML::IXMLDOMDocumentPtr ipDocument, const CRuleSetProfiler& profiledObject)
{
	MSXML::IXMLDOMElementPtr ipCallElement = ipDocument->createElement(strName.c_str());
	ASSERT_RESOURCE_ALLOCATION("ELI33618", ipCallElement != __nullptr);

	ipCallElement->setAttribute("calls", profiledObject.m_nCallCount);
	ipCallElement->setAttribute("totaltime", profiledObject.m_nTotalTime);
	ipCallElement->setAttribute("handle", profiledObject.m_nHandle);
	ipCallElement->setAttribute("name", profiledObject.m_strName.c_str());
	ipCallElement->setAttribute("param", profiledObject.m_strType.c_str());

	return ipCallElement;
}
//--------------------------------------------------------------------------------------------------
MSXML::IXMLDOMElementPtr CRuleSetProfiler::createMethodElement(
	MSXML::IXMLDOMDocumentPtr ipDocument, const CRuleSetProfiler& profiledObject)
{
	MSXML::IXMLDOMElementPtr ipMethodElement = ipDocument->createElement("method");
	ASSERT_RESOURCE_ALLOCATION("ELI33619", ipMethodElement != __nullptr);

	ipMethodElement->setAttribute("handle", profiledObject.m_nHandle);
	ipMethodElement->setAttribute("name", profiledObject.m_strName.c_str());
	ipMethodElement->setAttribute("param", profiledObject.m_strType.c_str());

	for (ProfilerMap::const_iterator iter = profiledObject.m_mapCallerPerfData.begin();
		 iter != profiledObject.m_mapCallerPerfData.end();
		 iter++)
	{
		ipMethodElement->appendChild(createCallElement("called-by", ipDocument, iter->second));
	}

	return ipMethodElement;
}
//--------------------------------------------------------------------------------------------------
void CRuleSetProfiler::generateHandle(map<int, ProfilerID>& rExistingHandles)
{
	// Generate an initial handle candidate by XOR'ing all 4 32bit blocks of the guid with each
	// other, then XOR'ing the m_nSubID. This should be very likely to be unique to this object
	// and will guarantee that it is not possible for two sub IDs for the same GUID to share the
	// same handle.
	int* pdwGUIDData = (int*)&m_ID.m_GUID;
	for (int i = 0; i < 4; i++)
	{
		m_nHandle ^= pdwGUIDData[i];
	}
	m_nHandle ^= m_ID.m_nSubID;

	// In the event of a handle collision with a different object, attempt to resolve the collision
	// by flipping each bit from left to right until a unique handle is found. (Start with the most
	// significant bit since flipping the least significant bit is likely to collide with another
	// sub-id for this GUID.
	int i = 0;
	map<int, ProfilerID>::iterator existingEntry = rExistingHandles.find(m_nHandle);
	while (i < 32 &&
		   existingEntry != rExistingHandles.end() &&
		   m_ID != existingEntry->second)
	{
		m_nHandle ^= (1 >> i++);
		existingEntry = rExistingHandles.find(m_nHandle);
	}

	// If we can't find a unique handle in 32 attempts, give up.
	if (i == 32)
	{
		throw UCLIDException("ELI33617", "Failed to create unique hash value");
	}

	if (existingEntry == rExistingHandles.end())
	{
		rExistingHandles[m_nHandle] = m_ID;
	}
}