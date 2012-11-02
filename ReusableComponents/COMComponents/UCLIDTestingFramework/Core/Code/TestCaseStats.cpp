
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
void TestCaseStats::recordOcrConfidence(long nCharCount, long nOcrConfidence)
{
	if (nCharCount > 0)
	{
		m_vecOCRConfidenceData.push_back(pair<long, long>(nCharCount, nOcrConfidence));
	}
}
//-------------------------------------------------------------------------------------------------
void TestCaseStats::getOcrConfidence(long *pnDocCount, double *pdOcrConfidence)
{
	double &dOCRConfidence(*pdOcrConfidence);
	dOCRConfidence = 0;
	long &nDocCount(*pnDocCount);
	nDocCount = m_vecOCRConfidenceData.size();

	// If any char confidence data was collected, output the average char confidence.
	if (nDocCount > 0)
	{
		double dCharCount = 0;
		for (long i = 0; i < nDocCount; i++)
		{
			double dDocCharCount = (double)m_vecOCRConfidenceData[i].first;
			dCharCount += dDocCharCount;
			dOCRConfidence += (dDocCharCount * m_vecOCRConfidenceData[i].second);
		}

		dOCRConfidence /= dCharCount;
	}
}
//-------------------------------------------------------------------------------------------------
void TestCaseStats::reset()
{
	m_ulTotalTestCases = 0;
	m_ulPassedTestCases = 0;
	m_vecOCRConfidenceData.clear();
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
