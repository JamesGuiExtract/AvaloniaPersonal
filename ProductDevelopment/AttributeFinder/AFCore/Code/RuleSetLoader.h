
#pragma once

#include <CachedObjectFromFile.h>

#include <string>

class RuleSetLoader : public FileObjectLoaderBase
{
public:
	void loadObjectFromFile(IRuleSetPtr ipRuleSet, const std::string& strFile);
};