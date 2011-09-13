//=================================================================================================
//
// COPYRIGHT (c) 2011 EXTRACT SYSTEMS, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	RuleSetProfiler.h
//
// PURPOSE:	Each instance of this class can be used to either time the length of a call to any particular
// rule object, or to store the overall performance data for a particular rule object instance. An
// instance is considered a unique persistence of a rule of which there may be multiple copies in
// memory if the ruleset has been loaded from different callers.
//
// NOTES:	
//
// AUTHORS:	Steve Kurth
//
//=================================================================================================

#pragma once

#include "Export.h"

#import <msxml.tlb> named_guids

#include <string>
#include <map>
#include <stack>
#include <memory>

using namespace std;

//--------------------------------------------------------------------------------------------------
// CRuleSetProfiler
//--------------------------------------------------------------------------------------------------
class EXPORT_AFCore CRuleSetProfiler
{
public:

	// These constructors initialize an instance for data storage; they do no initialize profiling.
	CRuleSetProfiler();
	CRuleSetProfiler(const CRuleSetProfiler& source);

	// These constructors start profiling a call for the object described in the parameters.
	CRuleSetProfiler(const string& strName, const string& strType, const GUID& guid, int nSubID = 0);
	CRuleSetProfiler(const string& strName, const string& strType,
		UCLID_AFCORELib::IIdentifiableRuleObjectPtr ipIdentifiableRuleObject, int nSubID = 0);
	
	~CRuleSetProfiler();

	// Indicates whether profiling data is actively being collected on this thread.
	static bool IsProfilingActiveOnThread();

	// Outputs all profiling data to an EQATEC compatible xml file.
	static void GenerateOuput();

	// Clears all profiling data.
	static void Reset();

	// Indicates whether profiling data should be collected.
	static volatile bool ms_bEnabled;

private:

	////////////
	// Structs
	////////////

	// Uniquely identifies a RuleSetProfiler instance.
	struct ProfilerID
	{
	public:
		// Implement less than operator so this struct can be used as the key to an STL map.
		bool operator< (const ProfilerID& other) const;

		// Implement not equal to assist in checking for handle collisions.
		bool operator!= (const ProfilerID& other) const;

		// The InstanceGUID from the IIdentifiableRuleObject the CRuleSetProfiler is associated with.
		GUID m_GUID;

		// If there are more than one CRuleSetProfilers associated with a rule object, this identifies
		// which one.
		int m_nSubID;
	};

	typedef map<ProfilerID, CRuleSetProfiler> ProfilerMap;
	typedef stack<CRuleSetProfiler*> ProfilerStack;

	////////////
	// Variables
	////////////

	// The ID of this instance
	ProfilerID m_ID;

	// The handle to be used in EQATEC compatible xml output.
	int m_nHandle;

	// The object name and type that will appear in the output.
	string m_strName;
	string m_strType;

	// The total number of calls and total execution time for this instance.
	int m_nCallCount;
	int m_nTotalTime;

	// A collection of profiler instances that indicate which calls called this instance, then
	// number of times, and the total time spent in those calls.
	ProfilerMap m_mapCallerPerfData;

	// Indicates whether this instance is actively collecting data (vs being used to store data).
	bool m_bActive;

	// The time profiling started for this call.
	DWORD m_nStartTime;

	// The current stack of active calls; the object at the top of the stack is the object that
	// called this instance.
	ProfilerStack* m_pThreadProfilerStack;

	// The overall collection of profiling data for the current thread.
	ProfilerMap* m_pThreadProfilerMap;

	// Protects access to static fields.
	static CMutex ms_mutex;

	// The number of threads for which data is currently being collected.
	static volatile int m_nActiveThreadCount;

	// Contains the active call stack for each thread (by thread id)
	static map<DWORD, ProfilerStack> m_mapThreadIDToProfilerStack;

	// Contains the overall collection of profiling data for each thread (by thread id)
	static map<DWORD, ProfilerMap> m_mapThreadIDToProfilerMap;

	////////////
	// Methods
	////////////

	// Starts timing the current call.
	void startProfiling();

	// Creates a unique handle for the this instance for use by the EQATEC profiler.
	void generateHandle(map<int, ProfilerID>& rExistingHandles);

	// Collects that data from all instances in sourceData, and adds it to the data in destination
	// data.
	// rmapObjectHandles is required to allow the call to generate unique handles for the contained
	// calls.
	// bIsCallerData should only be called with true by aggregateData itself.
	static void aggregateData(const ProfilerMap& sourceData, ProfilerMap& rdestinationData, 
		map<int, ProfilerID>& rmapObjectHandles, bool bIsCallerData = false);

	// Generates an XML element to represent overall data for the specified object.
	static MSXML::IXMLDOMElementPtr createCallElement(string strName,
		MSXML::IXMLDOMDocumentPtr ipDocument, const CRuleSetProfiler& profiledObject);

	// Generates an XML element to represent call hierarchy data for the specified object.
	static MSXML::IXMLDOMElementPtr createMethodElement(MSXML::IXMLDOMDocumentPtr ipDocument,
		const CRuleSetProfiler& profiledObject);
};

// Use this macro in code to profile the object up to the point where this call goes out of scope.
// This macro also ensures as small as possible performance hit when profiling is not active.
#define PROFILE_RULE_OBJECT(strName, strType, object, nSubID) \
	unique_ptr<CRuleSetProfiler> upProfiler = __nullptr;\
	if (CRuleSetProfiler::ms_bEnabled) \
	{ \
		upProfiler.reset(new CRuleSetProfiler(strName, strType, \
			UCLID_AFCORELib::IIdentifiableRuleObjectPtr(object), nSubID)); \
	}

// Use this macro in code to add profiling data for something other than a rule object.
#define PROFILE_SPECIAL_OBJECT_IF_ACTIVE(strName, strType, guid, nSubID) \
	unique_ptr<CRuleSetProfiler> upProfiler = __nullptr;\
	if (CRuleSetProfiler::IsProfilingActiveOnThread()) \
	{ \
		upProfiler.reset(new CRuleSetProfiler(strName, strType, guid, nSubID)); \
	}