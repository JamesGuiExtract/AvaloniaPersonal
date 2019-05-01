#pragma once

#include <string>
#include <map>

// The IDs of the standard Extract rule execution counters
const int giINDEXING_DOCS_COUNTERID = 1;
const int giPAGINATION_DOCS_COUNTERID = 2;
const int giREDACTION_PAGES_COUNTERID = 3;
const int giREDACTION_DOCS_COUNTERID = 4;
const int giINDEXING_PAGES_COUNTERID = 5;

// Used to track data for each counter in order to determine the number of counts that may be
// accumulated prior to deducting them from the USB key. [LegacyRCAndUtils:6170]
struct CounterData
{
	CounterData();

	int m_nCountsDecrementedInProcess;
	int m_nLastCountValue;
	int m_nCountDecrementAccumulation;
};

// Static CounterData instances to be used to track counts for the standard Extract counters. 
static CounterData ms_indexingByDocCounterData;
static CounterData ms_indexingByPageCounterData;
static CounterData ms_paginationCounterData;
static CounterData ms_redactionByDocCounterData;
static CounterData ms_redactionByPageCounterData;

// Information about a rule execution counter.
class CounterInfo
{
public:
	long m_nID;
	std::string m_strName;
	// true to decrement by page, false to decrement by document.
	bool m_bByPage;
	// Whether the counter is checked in the RuleSet properties counter grid.
	bool m_bEnabled;
	// The row index the counter appears in the RuleSet properties counter grid.
	long m_nIndex;

	CounterInfo();
	CounterInfo(long nID, std::string strName);
	CounterInfo(const CounterInfo& counterInfo);
	CounterInfo(IVariantVectorPtr ipPropVector);

	/////////////////////
	// Instance methods
	/////////////////////

	// Retrieves the ruleset properties as a variant vector (ID, Name, ByPage, Enabled)
	IVariantVectorPtr GetProperties();

	// Assigns any ISecureCounter instance that should be used for decrementing this counter.
	void SetSecureCounter(ISecureCounterPtr ipSecureCounter);

	// Retrieves any ISecureCounter instance that should be used for decrementing this counter.
	ISecureCounterPtr GetSecureCounter() { return m_ipSecureCounter; }

	// Gets the static CounterData instance used to track counter accumulation in the process
	CounterData& GetCounterData();

	/////////////////////
	// Static methods
	/////////////////////

	// Retrieves a map of counter ID to CounterInfo for all counters defined in ipRuleSet.
	static std::map<long, CounterInfo> GetCounterInfo(UCLID_AFCORELib::IRuleSetPtr ipRuleSet);

	// Persists the counters as defined in mapCounterInfo into ipRuleSet.
	static void ApplyCounterInfo(std::map<long, CounterInfo> mapCounterInfo,
		UCLID_AFCORELib::IRuleSetPtr ipRuleSet);

private:

	///////////////////////
	// Instance Variables
	///////////////////////

	// Any ISecureCounter that has been assigned to use for decrementing.
	ISecureCounterPtr m_ipSecureCounter;

	///////////////////////
	// Static Variables
	///////////////////////

	// Keeps track of all counter names that have been used to ensure different threads aren't
	// attempting to decrement the same counter ID, but with different names.
	static std::map<long, std::string> ms_mapCounterNames;

	// Keeps track of the CounterData instance for tracking counter accumulation across all threads.
	static std::map<long, CounterData> ms_mapCustomCounterData;

	// Protects access to ms_mapCounterNames and ms_mapCustomCounterData
	static CCriticalSection ms_CriticalSection;
};
