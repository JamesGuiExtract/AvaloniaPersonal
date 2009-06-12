
#pragma once

#include "SPMBinData.h"
#include <map>

class SPMLiteralSearchData
{
public:
	// ctor, assignment operator, and copy ctor
	SPMLiteralSearchData();
	SPMLiteralSearchData(const SPMLiteralSearchData& data);
	SPMLiteralSearchData& operator=(const SPMLiteralSearchData& data);

	long find(const stringCSIS& strText, const stringCSIS& strFind, long nStartPos);

private:
	std::map<long, SPMBinData> m_SearchData;
};
