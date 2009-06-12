
#pragma once

#include <string>

class RuleSetLoader
{
public:
	void loadObjectFromFile(IRuleSetPtr ipRuleSet, const std::string& strFile);
};