
#pragma once

#include <vector>

using namespace std;

class TestCaseStats
{
public:
	// default constructor
	TestCaseStats();
	void recordFailedTestCase();
	void recordPassedTestCase();
	//void addSummaryTestCase(ITestResultLoggerPtr ipTestResultLogger);	
	void recordTestCaseStatus(bool bResult);
	void recordOcrConfidence(long nCharCount, long nOcrConfidence);
	void getOcrConfidence(long *pnDocCount, double *pdOcrConfidence);

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

	// Keeps track of the number of characters and the average character confidence of each
	// document.
	vector<pair<long, long>> m_vecOCRConfidenceData;

private:
	// time objects for the start/end time
	CTime m_startTime, m_endTime;
};
