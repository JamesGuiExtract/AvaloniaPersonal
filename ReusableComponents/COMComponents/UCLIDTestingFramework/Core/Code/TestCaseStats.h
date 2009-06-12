
#pragma once


class TestCaseStats
{
public:
	// default constructor
	TestCaseStats();
	void recordFailedTestCase();
	void recordPassedTestCase();
	//void addSummaryTestCase(ITestResultLoggerPtr ipTestResultLogger);	
	void recordTestCaseStatus(bool bResult);

	// return the total time since the last call to reset() or
	// since the time this object was constructed
	unsigned long getTotalElapsedSeconds() const;

	// this method is not effected by the current lock status
	// this method resets the counters to zero and unlocks the member 
	// variables
	void reset();

	// publicly accessible counters
	unsigned long m_ulTotalTestCases;
	unsigned long m_ulPassedTestCases;

private:
	// time objects for the start/end time
	CTime m_startTime, m_endTime;
};
