
#include "stdafx.h"
#include "RuleSetLoader.h"

#include <UCLIDException.h>

using namespace std;

//--------------------------------------------------------------------------------------------------
void RuleSetLoader::loadObjectFromFile(IRuleSetPtr ipRuleSet, const string& strFile)
{
	try
	{
		ASSERT_ARGUMENT("ELI10951", ipRuleSet != NULL);

		ipRuleSet->LoadFrom(strFile.c_str(), VARIANT_FALSE);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27086");
}
//--------------------------------------------------------------------------------------------------
