
#pragma once

#include <stringCSIS.h>

#include <vector>

// globals
// the bin size constant represents the block size that a document
// gets chopped into when we are trying to find matches for a
// particular word.
// For instance, if we are searching for the first "and" beginning
// at position 3567, and the document contains 10000 chars, and
// the bin size is 2000, we will need to search in the second bin (binIndex = 1)
// for any match positions >= 3567.  If not found, we look in the
// 3rd, 4th, and 5th bins.
const unsigned long gulSPM_BIN_SIZE = 2000;

class SPMBinData
{
public:
	// ctor, copy ctor, and assignment operator
	SPMBinData();
	SPMBinData(long nBinIndex);
	SPMBinData(const SPMBinData& data);
	SPMBinData& operator=(const SPMBinData& data);

	long find(const stringCSIS& strText, const stringCSIS& strFind, long nStartPos);

private:	
	long m_nBinIndex;	// index of this bin
	std::vector<long> m_vecPositions; // match positions in this bin
	
	// this bool indicates whether search has been completed for this bin
	bool m_bSearchComplete;	
	
	// size of m_vecPositions.  This variable should only be read 
	// if m_bSearchComplete == true
	unsigned long m_ulVecSize;

	// methods to execute a binary or linear search on m_vecPositions
	long execBinarySearch(long nStartPos);
	long execLinearSearch(long nStartPos);
};
