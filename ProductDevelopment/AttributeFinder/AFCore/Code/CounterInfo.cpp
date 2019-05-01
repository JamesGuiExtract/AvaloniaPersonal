#include "stdafx.h"
#include "CounterInfo.h"

#include <UCLIDException.h>
#include <COMUtils.h>
#include <cpputil.h>

using namespace std;

//--------------------------------------------------------------------------------------------------
// Statics
//--------------------------------------------------------------------------------------------------
static map<long, string> getStandardCounters()
{
  map<long, string> mapCounters;
  mapCounters[1] = "FLEX Index - Indexing (By Document)";
  mapCounters[2] = "FLEX Index - Pagination (By Document)";
  mapCounters[3] = "ID Shield - Redaction (By Page)";
  mapCounters[4] = "ID Shield - Redaction (By Document)";
  mapCounters[5] = "FLEX Index - Indexing (By Page)";

  return mapCounters;
}
const map<long, string> gmapStandardCounters = getStandardCounters();

std::map<long, string> CounterInfo::ms_mapCounterNames;
std::map<long, CounterData> CounterInfo::ms_mapCustomCounterData;
CCriticalSection CounterInfo::ms_CriticalSection;

//--------------------------------------------------------------------------------------------------
// CounterData
//--------------------------------------------------------------------------------------------------
CounterData::CounterData()
	: m_nCountsDecrementedInProcess(0)
	, m_nLastCountValue(0)
	, m_nCountDecrementAccumulation(0)
{
}

//--------------------------------------------------------------------------------------------------
// CounterInfo
//--------------------------------------------------------------------------------------------------
CounterInfo::CounterInfo()
	: m_nID(0)
	, m_bByPage(false)
	, m_bEnabled(false)
	, m_nIndex(-1)
{
}
//--------------------------------------------------------------------------------------------------
CounterInfo::CounterInfo(long nID, string strName)
	: m_nID(nID)
	, m_strName(strName)
	, m_bByPage(false)
	, m_bEnabled(false)
	, m_nIndex(-1)
{
	// Any counter whose name ends "(By Page)" shall be decremented by page
	if (m_strName.length() >= 9 &&
		_strcmpi(m_strName.substr(m_strName.length() - 9).c_str(), "(By Page)") == 0)
	{
		m_bByPage = true;
	}
}
//--------------------------------------------------------------------------------------------------
CounterInfo::CounterInfo(const CounterInfo& counterInfo)
	: m_nID(counterInfo.m_nID)
	, m_strName(counterInfo.m_strName)
	, m_bByPage(counterInfo.m_bByPage)
	, m_bEnabled(counterInfo.m_bEnabled)
	, m_nIndex(-1)
{
}
//--------------------------------------------------------------------------------------------------
CounterInfo::CounterInfo(IVariantVectorPtr ipPropVector)
	: m_nIndex(-1)
{
	try
	{
		ASSERT_RUNTIME_CONDITION("ELI39009", ipPropVector != nullptr && ipPropVector->Size == 4,
			"Invalid counter info.");

		m_nID =  ipPropVector->GetItem(0).lVal;
		m_strName = asString(ipPropVector->GetItem(1).bstrVal);
		m_bByPage = asCppBool(ipPropVector->GetItem(2).boolVal);
		m_bEnabled = asCppBool(ipPropVector->GetItem(3).boolVal);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI39010");
}
//--------------------------------------------------------------------------------------------------
IVariantVectorPtr CounterInfo::GetProperties()
{
	IVariantVectorPtr ipPropVector(CLSID_VariantVector);
	ASSERT_RESOURCE_ALLOCATION("ELI39011", ipPropVector != nullptr);

	ipPropVector->PushBack(_variant_t(m_nID));
	ipPropVector->PushBack(_variant_t(m_strName.c_str()));
	ipPropVector->PushBack(_variant_t(m_bByPage));
	ipPropVector->PushBack(_variant_t(m_bEnabled));

	return ipPropVector;
}
//--------------------------------------------------------------------------------------------------
void CounterInfo::SetSecureCounter(ISecureCounterPtr ipSecureCounter)
{
	if (_strcmpi(asString(ipSecureCounter->Name).c_str(), m_strName.c_str()) != 0)
	{
		UCLIDException ue("ELI39012", "Counter discrepancy encountered.");
		ue.addDebugInfo("Expected counter name", m_strName);
		ue.addDebugInfo("Counter Name", asString(ipSecureCounter->Name));
		throw ue;
	}

	CSingleLock lg(&ms_CriticalSection, TRUE);

	if (ms_mapCounterNames.find(m_nID) == ms_mapCounterNames.end())
	{
		ms_mapCounterNames[m_nID] = m_strName;
	}
	else
	{
		// The same name should be used for any given counter ID across any thread.
		if (_strcmpi(ms_mapCounterNames[m_nID].c_str(), m_strName.c_str()) != 0)
		{
			UCLIDException ue("ELI40365", "Counter discrepancy encountered.");
			ue.addDebugInfo("Expected counter name", m_strName);
			ue.addDebugInfo("Counter Name", ms_mapCounterNames[m_nID]);
			throw ue;
		}
	}

	m_ipSecureCounter = ipSecureCounter;
}
//--------------------------------------------------------------------------------------------------
CounterData& CounterInfo::GetCounterData()
{
	// Standard counters
	if (m_nID < 100)
	{
		switch (m_nID)
		{
			case giINDEXING_DOCS_COUNTERID:		return ms_indexingByDocCounterData;
			case giPAGINATION_DOCS_COUNTERID:	return ms_paginationCounterData;
			case giREDACTION_PAGES_COUNTERID:	return ms_redactionByPageCounterData;
			case giREDACTION_DOCS_COUNTERID:	return ms_redactionByDocCounterData;
			case giINDEXING_PAGES_COUNTERID:	return ms_indexingByPageCounterData;

			default:	THROW_LOGIC_ERROR_EXCEPTION("ELI39014");
		}
	}
	// Custom counters
	else
	{
		CSingleLock lg(&ms_CriticalSection, TRUE);

		if (m_nID >= 100 && ms_mapCustomCounterData.find(m_nID) == ms_mapCustomCounterData.end())
		{
			ms_mapCustomCounterData.emplace(make_pair(m_nID, CounterData()));
		}

		return ms_mapCustomCounterData[m_nID];
	}
}
//--------------------------------------------------------------------------------------------------
map<long, CounterInfo> CounterInfo::GetCounterInfo(UCLID_AFCORELib::IRuleSetPtr ipRuleSet)
{
	ASSERT_ARGUMENT("ELI39015", ipRuleSet != nullptr);

	map<long, CounterInfo> mapCounters;

	// Generate a CounterInfo instance for each of the standard Extract counters.
	for each (pair<int, string> counter in gmapStandardCounters)
	{
		mapCounters.emplace(make_pair(counter.first, CounterInfo(counter.first, counter.second)));
		CounterInfo& counterInfo = mapCounters[counter.first];

		switch (counterInfo.m_nID)
		{
			case giINDEXING_DOCS_COUNTERID:
				counterInfo.m_bEnabled = asCppBool(ipRuleSet->UseDocsIndexingCounter);
				break;
			
			case giPAGINATION_DOCS_COUNTERID:
				counterInfo.m_bEnabled = asCppBool(ipRuleSet->UsePaginationCounter);
				break;

			case giREDACTION_PAGES_COUNTERID:
				counterInfo.m_bEnabled = asCppBool(ipRuleSet->UsePagesRedactionCounter);
				counterInfo.m_bByPage = true;
				break;

			case giREDACTION_DOCS_COUNTERID:
				counterInfo.m_bEnabled = asCppBool(ipRuleSet->UseDocsRedactionCounter);
				break;

			case giINDEXING_PAGES_COUNTERID:
				counterInfo.m_bEnabled = asCppBool(ipRuleSet->UsePagesIndexingCounter);
				counterInfo.m_bByPage = true;
				break;
		}
	}

	// Generate a CounterInfo instance for every custom counter defined in ipRuleSet.
	IIUnknownVectorPtr ipCustomCounters = ipRuleSet->CustomCounters;
	ASSERT_RESOURCE_ALLOCATION("ELI39016", ipCustomCounters != nullptr);

	long nCount = ipCustomCounters->Size();
	for (long i = 0; i < nCount; i++)
	{
		IVariantVectorPtr ipPropVector = ipCustomCounters->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI39017", ipPropVector != nullptr);

		long nID = ipPropVector->GetItem(0).lVal;
		mapCounters.emplace(make_pair(nID, CounterInfo(ipPropVector)));
	}

	return mapCounters;
}
//--------------------------------------------------------------------------------------------------
void CounterInfo::ApplyCounterInfo(std::map<long, CounterInfo> mapCounterInfo,
								   UCLID_AFCORELib::IRuleSetPtr ipRuleSet)
{
	IIUnknownVectorPtr ipCustomCounters(CLSID_IUnknownVector);
	ASSERT_RESOURCE_ALLOCATION("ELI39018", ipCustomCounters != nullptr);

	// Ensure there is never more than one of the standard indexing or redaction counters used at
	// once.
	int nIndexingCounter = 0;
	int nRedactionCounter = 0;

	// Apply each CounterInfo instance from mapCounterInfo to ipRuleSet
	for each (pair<long, CounterInfo> entry in mapCounterInfo)
	{
		long nID = entry.first;
		CounterInfo& counterInfo = entry.second;

		switch (nID)
		{
			case giINDEXING_DOCS_COUNTERID:
				ipRuleSet->UseDocsIndexingCounter = asVariantBool(counterInfo.m_bEnabled);
				nIndexingCounter += counterInfo.m_bEnabled ? 1 : 0;
				break;

			case giPAGINATION_DOCS_COUNTERID:
				ipRuleSet->UsePaginationCounter = asVariantBool(counterInfo.m_bEnabled);
				break;

			case giREDACTION_PAGES_COUNTERID:
				ipRuleSet->UsePagesRedactionCounter = asVariantBool(counterInfo.m_bEnabled);
				nRedactionCounter += counterInfo.m_bEnabled ? 1 : 0;
				break;

			case giREDACTION_DOCS_COUNTERID:
				ipRuleSet->UseDocsRedactionCounter = asVariantBool(counterInfo.m_bEnabled);
				nRedactionCounter += counterInfo.m_bEnabled ? 1 : 0;
				break;

			case giINDEXING_PAGES_COUNTERID:
				ipRuleSet->UsePagesIndexingCounter = asVariantBool(counterInfo.m_bEnabled);
				nIndexingCounter += counterInfo.m_bEnabled ? 1 : 0;
				break;

			default:
				ASSERT_RUNTIME_CONDITION("ELI39019", nID >= 100, "Custom counter IDs must be >= 100");
				ipCustomCounters->PushBack(counterInfo.GetProperties());
		}
	}

	if (nIndexingCounter > 1)
	{
		throw UCLIDException("ELI39020", "Cannot select indexing by pages and documents!");
	}

	if (nRedactionCounter > 1)
	{
		throw UCLIDException("ELI39021", "Cannot select redaction by pages and documents!");
	}

	ipRuleSet->CustomCounters = ipCustomCounters;
}
//--------------------------------------------------------------------------------------------------