
#include "stdafx.h"
#include "SPMLiteralSearchData.h"
#include "UCLIDException.h"

//------------------------------------------------------------------------------------------------
SPMLiteralSearchData::SPMLiteralSearchData()
{
}
//------------------------------------------------------------------------------------------------
SPMLiteralSearchData::SPMLiteralSearchData(const SPMLiteralSearchData& data)
{
	// invoke the assignment operator
	*this = data;
}
//------------------------------------------------------------------------------------------------
SPMLiteralSearchData& SPMLiteralSearchData::operator=(const SPMLiteralSearchData& data)
{
	// copy member variables
	m_SearchData = data.m_SearchData;

	return *this;
}
//------------------------------------------------------------------------------------------------
long SPMLiteralSearchData::find(const stringCSIS& strText, const stringCSIS& strFind, 
								long nStartPos)
{
	if ( strText.isCaseSensitive() != strFind.isCaseSensitive())
	{
		// if the case sensitivity of the 2 strings does not match
		// log exception. The strText case sensitivity will be the 
		// value that is used so method should behave correctly
		UCLIDException ue("ELI15929", "Non critical internal error.");
		ue.addDebugInfo("TextCase", strText.isCaseSensitive());
		ue.addDebugInfo("FindCase", strFind.isCaseSensitive());
		ue.log();
	}

	// get the lowest bin # that might contain a match
	long nBin = nStartPos / gulSPM_BIN_SIZE;
	long nMaxBins = strText.length() / gulSPM_BIN_SIZE + 1;

	// NOTE: we need the i==0 in the following two for loops
	// because there's always at least 1 bin.

	// if we have not yet initialized the bin data structures, do so now
	if (m_SearchData.size() == 0)
	{
		for (int i = 0; (i < nMaxBins) || (i == 0); i++)
		{
			m_SearchData[i] = SPMBinData(i);
		}
	}

	// go through the bins and see if there's a match
	for (int i = nBin; (i < nMaxBins) || (i == 0); i++)
	{
		// get the bin data
		SPMBinData& bd = m_SearchData[i];

		// if the bin contains a match, return it.
		// otherwise try subsequent bins
		long nResult = bd.find(strText, strFind, nStartPos);
		if (nResult != -1)
		{
			return nResult;
		}
	}

	// if we reached here, that's because we didn't find a match
	return string::npos;
}
//------------------------------------------------------------------------------------------------
