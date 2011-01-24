
#include "stdafx.h"
#include "FAMDBHelperFunctions.h"

#include <UCLIDException.h>
#include <StringTokenizer.h>

//--------------------------------------------------------------------------------------------------
vector<string> getTableNamesFromCreationQueries(vector<string> vecCreationQueries)
{
	int count = vecCreationQueries.size();
	vector<string> vecTableNames(count);

	for (int i = 0; i < count; i++)
	{
		// Assuming the first two words in the query are "Create" and "Table", the 3rd word will be
		// the table name.
		vector<string> vecTokens;
		StringTokenizer::sGetTokens(vecCreationQueries[i], " )(", vecTokens, true);
		if (vecTokens.size() < 3 ||
			_stricmp(vecTokens[0].c_str(), "CREATE") != 0 ||
			_stricmp(vecTokens[1].c_str(), "TABLE") != 0)
		{
			UCLIDException ue("ELI31406", "Expected table create query.");
			ue.addDebugInfo("Query", vecCreationQueries[i], true);
			throw ue;
		}
		
		// Trim enclosing braces as well as any "dbo." prefix.
		string strTableName = trim(vecTokens[2], "[", "]");
		if (strTableName.length() > 3 && _stricmp(strTableName.substr(0, 3).c_str(), "dbo") == 0)
		{
			strTableName = trim(strTableName.substr(3), "][.", "]");
		}

		vecTableNames[i] = strTableName;
	}

	return vecTableNames;
}
//-------------------------------------------------------------------------------------------------
