#include "stdafx.h"
#include "SelectFileSettings.h"

#include <cppUtil.h>
#include <UCLIDException.h>

//--------------------------------------------------------------------------------------------------
// Constructors
//--------------------------------------------------------------------------------------------------
SelectFileSettings::SelectFileSettings() :
m_bAnd(true),
m_bLimitToSubset(false),
m_bSubsetIsRandom(true),
m_bSubsetUsePercentage(true),
m_nSubsetSize(0)
{
}
//--------------------------------------------------------------------------------------------------
SelectFileSettings::SelectFileSettings(const SelectFileSettings &settings)
{
	*this = settings;
}
//--------------------------------------------------------------------------------------------------
SelectFileSettings::~SelectFileSettings()
{
	try
	{
		clearConditions();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI33788");
}

//--------------------------------------------------------------------------------------------------
// Public methods
//--------------------------------------------------------------------------------------------------
SelectFileSettings& SelectFileSettings::operator =(const SelectFileSettings &source)
{
	m_bAnd = source.m_bAnd;
	m_bLimitToSubset = source.m_bLimitToSubset;
	m_bSubsetIsRandom = source.m_bSubsetIsRandom;
	m_bSubsetUsePercentage = source.m_bSubsetUsePercentage;
	m_nSubsetSize = source.m_nSubsetSize;

	// Delete any existing conditions.
	clearConditions();

	// Populate m_vecConditions with a clone of the conditions in source.
	for each (SelectFileCondition* pCondition in source.m_vecConditions)
	{
		addCondition(pCondition->clone());
	}

	return *this;
}
//--------------------------------------------------------------------------------------------------
void SelectFileSettings::deleteCondition(int nIndex)
{
	delete m_vecConditions[nIndex];
	m_vecConditions.erase(m_vecConditions.begin() + nIndex);
}
//--------------------------------------------------------------------------------------------------
void SelectFileSettings::clearConditions()
{
	for each (SelectFileCondition* pCondition in m_vecConditions)
	{
		delete pCondition;
	}

	m_vecConditions.clear();
}
//--------------------------------------------------------------------------------------------------
string SelectFileSettings::getSummaryString()
{
	string strSummary = "All files ";

	if (m_vecConditions.empty())
	{
		strSummary += "in the database";
	}
	else
	{
		for (size_t i = 0; i < m_vecConditions.size(); i++)
		{
			bool bFirst = (i == 0);
			if (!bFirst)
			{
				strSummary += m_bAnd ? " and " : " or ";
			}

			strSummary += m_vecConditions[i]->getSummaryString(bFirst);
		}
	}

	if (m_bLimitToSubset)
	{
		string strMethod = m_bSubsetIsRandom ? " a random " : " the top ";

		if (m_bSubsetUsePercentage)
		{
			strSummary += ".\r\nThe scope of files will be further narrowed to" + strMethod
				+ asString(m_nSubsetSize) + " percent.";
		}
		else
		{
			strSummary += ".\r\nThe scope of files will be further narrowed to" + strMethod
				+ asString(m_nSubsetSize) + " file(s).";
		}
	}

	return strSummary;
}
//--------------------------------------------------------------------------------------------------
string SelectFileSettings::buildQuery(const IFileProcessingDBPtr& ipFAMDB, const string& strSelect, 
	const string& strOrderByClause)
{
	ASSERT_ARGUMENT("ELI27722", ipFAMDB != __nullptr);
	
	string strQueryPart1 = "SELECT DISTINCT " + strSelect + " FROM ";
	string strQuery;

	if (m_vecConditions.empty())
	{
		strQuery = strQueryPart1 + "FAMFile";
	}
	else
	{
		for (size_t i = 0; i < m_vecConditions.size(); i++)
		{
			if (i > 0)
			{
				strQuery += m_bAnd ? "\r\nINTERSECT\r\n" : "\r\nUNION\r\n";
			}

			strQuery += m_vecConditions[i]->buildQuery(ipFAMDB, strSelect);
		}
	}

	if (m_bLimitToSubset && m_bSubsetIsRandom)
	{
		// If choosing a random subset by specifying the number of files to select, use the query
		// parts generated thus far to create a proceedure which will randomly select the specified
		// number of results from the query while preserving the order which would have resulted
		// from the original query.
		// This proceedure has 3 limitations regarding strSelect:
		// 1) strSelect cannot be "*" if none of the select columns are an identity column (in which
		//	  case, an extra "RowNumber" column will be returned.
		// 2) If strSelect specifies a column with the identity property, the resulting row order
		//	  will be random regardless of any order by clause included in the query.
		// 3) strSelect cannot reference any table name other than FAMFile. Columns from other
		//    tables can be referenced as long as the column name is unique amongst all tables in
		//    the query and the table name is not explicitly included.

		string strRandomizedQuery =
			// Besides improving performance, SET NOCOUNT ON prevents "Operation is not allowed when
			// the object is closed" errors.
			"SET NOCOUNT ON\r\n"
			"\r\n"
			// Start a try block so we can explicitly set NOCOUNT OFF
			"BEGIN TRY\r\n"
			"\r\n"
			// Ensure the #OriginalResults table is dropped
			"IF OBJECT_ID('tempdb..#OriginalResults', N'U') IS NOT NULL\r\n"
			"	DROP TABLE #OriginalResults;\r\n"
			"\r\n"
			// This query creates table #OriginalResults with the same columns as the original query.
			"SELECT TOP 0 " + strSelect + " INTO #OriginalResults FROM\r\n"
			"(\r\n" + strQuery + "\r\n) AS FAMFile"
			"\r\n"
			// Determine if #OriginalResults contains an identity column. Add a RowNumber identity
			// if an identity column doesn't already exist.
			"DECLARE @queryHasIdentityColumn INT\r\n"
			"SELECT @queryHasIdentityColumn = COUNT(object_id) FROM tempdb.SYS.IDENTITY_COLUMNS\r\n"
			"	WHERE object_id = OBJECT_ID('tempdb..#OriginalResults')\r\n"
			"IF @queryHasIdentityColumn = 0\r\n"
			"	ALTER TABLE #OriginalResults ADD RowNumber INT IDENTITY\r\n"
			"ELSE\r\n"
			"	SET IDENTITY_INSERT #OriginalResults ON\r\n"
			"\r\n"
			"DECLARE @rowsToReturn INT\r\n"
			// Populate the table via INSERT INTO to avoid issues with ORDER BY + SELECT INTO +
			// IDENTITY (http://support.microsoft.com/kb/273586)
			"INSERT INTO #OriginalResults (" + strSelect + ") " + strQueryPart1 + "\r\n"
			"(\r\n" + strQuery + "\r\n) AS FAMFile"
			"\r\n"
			// Calculate the number to return (using SQL's PERCENT seems to be returning unexpected
			// results: 50% of 28 = 15)
			"SET @rowsToReturn = " + (m_bSubsetUsePercentage ? 
				"CEILING(@@ROWCOUNT * " + asString(m_nSubsetSize)+ ".0 / 100) " :
				asString(m_nSubsetSize)) + "\r\n"
			"\r\n"
			// If the original query has an identity column, just directly return a random subset
			// in random order since I can't come up with a good way of respecting any specified
			// order by clause.
			"IF @queryHasIdentityColumn = 1\r\n"
			"	SELECT " + strSelect + 
			" FROM (SELECT TOP (@rowsToReturn) * FROM #OriginalResults AS FAMFile ORDER BY NEWID()) AS FAMFile " +
			strOrderByClause + "\r\n"
			// If the original query doesn't have an identity column, we can select the rows
			// randomly into a table variable, then use the random row selection to select them
			// out of the #OriginalResults in the order they were inserted.
			"ELSE\r\n"
			"BEGIN\r\n"
			"	DECLARE @randomizedRows TABLE(RowNumber INT)\r\n"
			"	INSERT INTO @randomizedRows (#OriginalResults.RowNumber)\r\n"
			"		SELECT TOP (@rowsToReturn) RowNumber FROM #OriginalResults ORDER BY NEWID()\r\n"
			"\r\n"
			"	SELECT " + strSelect + " FROM #OriginalResults AS FAMFile\r\n"
			"		INNER JOIN @randomizedRows ON FAMFile.RowNumber = [@randomizedRows].RowNumber\r\n"
			"			ORDER BY FAMFile.RowNumber\r\n"
			"END\r\n"
			"\r\n"
			"DROP TABLE #OriginalResults\r\n"
			"\r\n"
			"SET NOCOUNT OFF\r\n"
			"\r\n"
			"END TRY\r\n"
			"\r\n"
			"BEGIN CATCH"
			"\r\n"
			// Ensure NOCOUNT is set to OFF
			"SET NOCOUNT OFF\r\n"
			"\r\n"
			// Get the error message, severity and state
		    "	DECLARE @ErrorMessage NVARCHAR(4000);\r\n"
		    "	DECLARE @ErrorSeverity INT;\r\n"
		    "	DECLARE @ErrorState INT;\r\n"
			"\r\n"
		    "SELECT \r\n"
	        "	@ErrorMessage = ERROR_MESSAGE(),\r\n"
	        "	@ErrorSeverity = ERROR_SEVERITY(),\r\n"
	        "	@ErrorState = ERROR_STATE();\r\n"
			"\r\n"
			// Check for state of 0 (cannot raise error with state 0, set to 1)
			"IF @ErrorState = 0\r\n"
			"	SELECT @ErrorState = 1\r\n"
			"\r\n"
			// Raise the error so that it will be caught at the outer scope
		    "RAISERROR (@ErrorMessage,\r\n"
			"	@ErrorSeverity,\r\n"
			"	@ErrorState\r\n"
			");\r\n"
			"\r\n"
			"END CATCH"
			"\r\n";
			
		return strRandomizedQuery;
	}
	else if (m_bLimitToSubset && !m_bSubsetIsRandom)
	{
		string strTopQuery =
			// Besides improving performance, SET NOCOUNT ON prevents "Operation is not allowed when
			// the object is closed" errors.
			"SET NOCOUNT ON\r\n"
			"\r\n"
			// Start a try block so we can explicitly set NOCOUNT OFF
			"BEGIN TRY\r\n"
			"\r\n"
			// Ensure the #OriginalResults table is dropped
			"IF OBJECT_ID('tempdb..#OriginalResults', N'U') IS NOT NULL\r\n"
			"	DROP TABLE #OriginalResults;\r\n"
			"\r\n"
			// This query creates table #OriginalResults with the same columns as the original query.
			"SELECT TOP 0 " + strSelect + " INTO #OriginalResults FROM\r\n"
			"(\r\n" + strQuery + "\r\n) AS FAMFile"
			"\r\n"
			// Determine if #OriginalResults contains an identity column. Add a RowNumber identity
			// if an identity column doesn't already exist.
			"DECLARE @queryHasIdentityColumn INT\r\n"
			"SELECT @queryHasIdentityColumn = COUNT(object_id) FROM tempdb.SYS.IDENTITY_COLUMNS\r\n"
			"	WHERE object_id = OBJECT_ID('tempdb..#OriginalResults')\r\n"
			"IF @queryHasIdentityColumn = 0\r\n"
			"	ALTER TABLE #OriginalResults ADD RowNumber INT IDENTITY\r\n"
			"ELSE\r\n"
			"	SET IDENTITY_INSERT #OriginalResults ON\r\n"
			"\r\n"
			"DECLARE @rowsToReturn INT\r\n"
			// Populate the table via INSERT INTO to avoid issues with ORDER BY + SELECT INTO +
			// IDENTITY (http://support.microsoft.com/kb/273586)
			"INSERT INTO #OriginalResults (" + strSelect + ") " + strQueryPart1 + "\r\n"
			"(\r\n" + strQuery + "\r\n) AS FAMFile"
			"\r\n"
			// Calculate the number to return (using SQL's PERCENT seems to be returning unexpected
			// results: 50% of 28 = 15)
			"SET @rowsToReturn = " + (m_bSubsetUsePercentage ? 
				"CEILING(@@ROWCOUNT * " + asString(m_nSubsetSize)+ ".0 / 100) " :
				asString(m_nSubsetSize)) + "\r\n"
			"SELECT " + strSelect + 
			" FROM (SELECT TOP (@rowsToReturn) * FROM #OriginalResults AS FAMFile) AS FAMFile " +
			strOrderByClause + "\r\n"
			"\r\n"
			"DROP TABLE #OriginalResults\r\n"
			"\r\n"
			"SET NOCOUNT OFF\r\n"
			"\r\n"
			"END TRY\r\n"
			"\r\n"
			"BEGIN CATCH"
			"\r\n"
			// Ensure NOCOUNT is set to OFF
			"SET NOCOUNT OFF\r\n"
			"\r\n"
			// Get the error message, severity and state
			"	DECLARE @ErrorMessage NVARCHAR(4000);\r\n"
			"	DECLARE @ErrorSeverity INT;\r\n"
			"	DECLARE @ErrorState INT;\r\n"
			"\r\n"
			"SELECT \r\n"
			"	@ErrorMessage = ERROR_MESSAGE(),\r\n"
			"	@ErrorSeverity = ERROR_SEVERITY(),\r\n"
			"	@ErrorState = ERROR_STATE();\r\n"
			"\r\n"
			// Check for state of 0 (cannot raise error with state 0, set to 1)
			"IF @ErrorState = 0\r\n"
			"	SELECT @ErrorState = 1\r\n"
			"\r\n"
			// Raise the error so that it will be caught at the outer scope
			"RAISERROR (@ErrorMessage,\r\n"
			"	@ErrorSeverity,\r\n"
			"	@ErrorState\r\n"
			");\r\n"
			"\r\n"
			"END CATCH"
			"\r\n";

		return strTopQuery;
	}
	else
	{
		// We don't need to return a sized subset-- simply combine the query parts and return
		return strQuery + strOrderByClause;
	}
}
//--------------------------------------------------------------------------------------------------
IRandomMathConditionPtr SelectFileSettings::getRandomCondition()
{
	// For the time being, SQL proceedure is taking care of all random scenarios.
	return NULL;
}
//--------------------------------------------------------------------------------------------------