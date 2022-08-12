#pragma once

#include <vector>
#include <string>
#include <regex>
#include <algorithm>
#include <numeric>
#include <iostream>
#include <sstream>

namespace {
	//---------------------------------------------------------------------------------------------
	// Private constants
	//---------------------------------------------------------------------------------------------
	static const std::string fullRangePattern(R"((-?\d+)\.\.(-?\d+))");
	static const std::string leftRangePattern(R"((-?\d+)\.\.)");
	static const std::string rightRangePattern(R"(\.\.(-?\d+))");
	static const std::string singleNumberPattern(R"((-?\d+))");

	static const std::regex rangeRegex(
		R"(\s*(?:)" + fullRangePattern
		+ "|" + leftRangePattern
		+ "|" + rightRangePattern
		+ "|" + singleNumberPattern + R"()\s*)");

	const size_t fullRangeLeftGroup = 1;
	const size_t fullRangeRightGroup = 2;
	const size_t leftRangeGroup = 3;
	const size_t rightRangeGroup = 4;
	const size_t singleNumberGroup = 5;

	//---------------------------------------------------------------------------------------------
	// Private functions
	//---------------------------------------------------------------------------------------------
	std::vector<size_t> getNumbersFromRange(size_t totalSize, long left, long right)
	{
		if (left == 0 || abs(left) > long(totalSize))
		{
			UCLIDException ue("ELI53584", "Specified start number is out of range!");
			ue.addDebugInfo("Start number", left);
			ue.addDebugInfo("Total size", totalSize);
			throw ue;
		}
		else if (right == 0 || abs(right) > long(totalSize))
		{
			UCLIDException ue("ELI53583", "Specified end number is out of range!");
			ue.addDebugInfo("End number", right);
			ue.addDebugInfo("Total size", totalSize);
			throw ue;
		}

		size_t start = left < 0 ? totalSize + left + 1 : left;
		size_t end = right < 0 ? totalSize + right + 1 : right;

		std::vector<size_t> numbers;
		if (start < end)
			for (size_t n = start; n <= end; n++) numbers.push_back(n);
		else
			for (size_t n = start; n >= end; n--) numbers.push_back(n);

		return numbers;
	}
	//---------------------------------------------------------------------------------------------
}

namespace Range {
	//---------------------------------------------------------------------------------------------
	// Parse a list of ranges into numbers
	// E.g.:
	//		1..2 => 1,2
	//		..2 => 1,2
	//		2.. => 1,2,3,..,<totalSize>
	//		1,2-4 => 1,2,3,4
	//		-3 => <totalSize-2>
	//		-3.. => <totalSize-2>,<totalSize-1>,<totalSize>
	//---------------------------------------------------------------------------------------------
	inline std::vector<size_t> getNumbers(size_t totalSize, const std::string& rangeSpec)
	{
		// Tokenize on comma
		std::vector<std::string> ranges;
		std::stringstream rangeStream(rangeSpec);
		std::string token;
		while (std::getline(rangeStream, token, ','))
		{
			ranges.push_back(token);
		}

		// Match each token as a range or single number and accumulate the results
		std::vector<size_t> numbers = std::accumulate(ranges.cbegin(), ranges.cend(), std::vector<size_t>(),
			[&](std::vector<size_t> acc, std::string range)
			{
				std::smatch subMatches;
				if (!regex_match(range, subMatches, rangeRegex))
				{
					UCLIDException ue("ELI53582", "Could not parse range!");
					ue.addDebugInfo("Range", range);
					throw ue;
				}

				long left, right;
				if (subMatches[leftRangeGroup].matched)
				{
					left = asLong(subMatches[leftRangeGroup].str());
					right = totalSize;
				}
				else if (subMatches[rightRangeGroup].matched)
				{
					left = 1;
					right = asLong(subMatches[rightRangeGroup].str());
				}
				else if (subMatches[singleNumberGroup].matched)
				{
					left = asLong(subMatches[singleNumberGroup].str());
					right = left;
				}
				else
				{
					left = asLong(subMatches[fullRangeLeftGroup].str());
					right = asLong(subMatches[fullRangeRightGroup].str());
				}
				std::vector<size_t> numbers = getNumbersFromRange(totalSize, left, right);
				acc.insert(acc.end(), numbers.begin(), numbers.end());

				return acc;
			});

		return numbers;
	}
	//---------------------------------------------------------------------------------------------
}
