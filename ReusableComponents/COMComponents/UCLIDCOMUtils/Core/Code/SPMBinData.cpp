
#include "stdafx.h"
#include "SPMBinData.h"
#include "UCLIDException.h"

#include <algorithm>

using namespace std;

//------------------------------------------------------------------------------------------------
SPMBinData::SPMBinData()
:m_nBinIndex(-1), m_bSearchComplete(false), m_ulVecSize(0)
{
}
//------------------------------------------------------------------------------------------------
SPMBinData::SPMBinData(long nBinIndex)
:m_nBinIndex(nBinIndex), m_bSearchComplete(false), m_ulVecSize(0)
{
}
//------------------------------------------------------------------------------------------------
SPMBinData::SPMBinData(const SPMBinData& data)
{
	// invoke the assignment operator
	*this = data;
}
//------------------------------------------------------------------------------------------------
SPMBinData& SPMBinData::operator=(const SPMBinData& data)
{
	// copy data members
	m_vecPositions = data.m_vecPositions;
	m_nBinIndex = data.m_nBinIndex;
	m_bSearchComplete = data.m_bSearchComplete;
	m_ulVecSize = data.m_ulVecSize;

	return *this;
}
//------------------------------------------------------------------------------------------------
long SPMBinData::execBinarySearch(long nStartPos)
{
	// execute binary search for the smallest match position that is
	// greater than or equal to nStartPos;
	vector<long>::iterator iter = lower_bound(m_vecPositions.begin(), 
		m_vecPositions.end(), nStartPos);

	if (iter != m_vecPositions.end())
	{
		long nPos = iter - m_vecPositions.begin();
		return m_vecPositions[nPos];
	}

	// if we reached here, there's no match
	return string::npos;
}
//------------------------------------------------------------------------------------------------
long SPMBinData::execLinearSearch(long nStartPos)
{
	// execute linear search for the smallest match position that is
	// greater than or equal to nStartPos;

	if (m_ulVecSize != 0 && m_vecPositions[m_ulVecSize - 1] >= nStartPos)
	{
		// there is a match to be found - find the first match
		// where the position is >= nStartPos
		for (unsigned int i = 0; i < m_ulVecSize; i++)
		{
			long nPos = m_vecPositions[i];
			if (nPos >= nStartPos)
			{
				return nPos;
			}
		}
	}

	// if we reached here, there's no match
	return string::npos;
}
//------------------------------------------------------------------------------------------------
long SPMBinData::find(const stringCSIS& strText, 
	const stringCSIS& strFind, long nStartPos)
{
	if ( strText.isCaseSensitive() != strFind.isCaseSensitive() )
	{
		// Log error but don't need to throw exception since
		// the string the find is called on is the case flag
		// that is used
		UCLIDException ue("ELI15928", "Non critical internal error.");
		ue.addDebugInfo("TextCase", strText.isCaseSensitive());
		ue.addDebugInfo("FindCase", strFind.isCaseSensitive());
		ue.log();
	}

	// if we haven't searched for this literal before in this bin
	// search now
	if (!m_bSearchComplete)
	{
		long nFindLength = strFind.length();

		// TODO: if this bin already contains some entries
		// (due to searches in the previous bin which overflowed)
		// then set nCurPos to a more optimal search location
		// so that we're not searching over regions that we
		// have already searched

		long nCurPos = m_nBinIndex * gulSPM_BIN_SIZE;
		long nNextBinStartPos = (m_nBinIndex + 1) * gulSPM_BIN_SIZE;
		do
		{
			nCurPos = strText.find(strFind, nCurPos);
			if (nCurPos != string::npos && nCurPos < nNextBinStartPos)
			{
				m_vecPositions.push_back(nCurPos);
				nCurPos += nFindLength;
			}

			// TODO:
			// if (nCurPos >= nNextBinStartPos)
			//{
			//	store this information in one of the future bins
			//	so that we don't have to search for this one again
			//	NOTE: the bin that this position needs to be stored
			//	in may not be the next bin - it may be a few bins away!
			//	(in which case all the intermediate bins need to be "zeroed out"
			//	and marked as "search complete"
			//}
		}
		while (nCurPos != string::npos && nCurPos < nNextBinStartPos);

		m_bSearchComplete = true;
		m_ulVecSize = m_vecPositions.size();
	}

	// depending upon # of items in the vector, execute a linear
	// or binary search
	// NOTE: Some preliminary research done by Arvind indicates that
	// a linear search is faster than a binary search for list sizes
	// of approximately 25 or less.
	if (m_ulVecSize == 0)
	{
		return string::npos;
	}
	else if (m_ulVecSize < 25)
	{
		return execLinearSearch(nStartPos);
	}
	else
	{
		return execBinarySearch(nStartPos);
	}
}
//------------------------------------------------------------------------------------------------
