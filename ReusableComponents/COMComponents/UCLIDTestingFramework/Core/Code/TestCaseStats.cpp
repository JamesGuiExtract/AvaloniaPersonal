
#include "stdafx.h"
#include "TestCaseStats.h"

//-------------------------------------------------------------------------------------------------
// TestCaseStats class
//-------------------------------------------------------------------------------------------------
TestCaseStats::TestCaseStats()
{
	// reset the counters
	reset();
}
//-------------------------------------------------------------------------------------------------
void TestCaseStats::recordFailedTestCase()
{
	m_ulTotalTestCases++;
}
//-------------------------------------------------------------------------------------------------
void TestCaseStats::recordPassedTestCase()
{
	m_ulTotalTestCases++;
	m_ulPassedTestCases++;
}
//-------------------------------------------------------------------------------------------------
void TestCaseStats::recordTestCaseStatus(bool bResult)
{
	if (bResult)
	{
		recordPassedTestCase();
	}
	else
	{
		recordFailedTestCase();
	}
}
//-------------------------------------------------------------------------------------------------
void TestCaseStats::reset()
{
	m_ulTotalTestCases = 0;
	m_ulPassedTestCases = 0;
	m_startTime = CTime::GetCurrentTime();
}
//-------------------------------------------------------------------------------------------------
unsigned long TestCaseStats::getTotalElapsedSeconds() const
{
	CTime endTime = CTime::GetCurrentTime();
	CTimeSpan span = endTime - m_startTime;
	return (unsigned long) span.GetTotalSeconds();
}
//-------------------------------------------------------------------------------------------------
