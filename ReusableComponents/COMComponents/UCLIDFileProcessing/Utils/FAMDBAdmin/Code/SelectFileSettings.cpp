#include "stdafx.h"
#include "SelectFileSettings.h"

#include <cppUtil.h>
#include <FAMUtilsConstants.h>
#include <UCLIDException.h>

//--------------------------------------------------------------------------------------------------
// Constructors
//--------------------------------------------------------------------------------------------------
SelectFileSettings::SelectFileSettings() :
m_scope(eAllFiles),
m_bLimitByRandomCondition(false),
m_bRandomSubsetUsePercentage(true),
m_nRandomAmount(0)
{
}
//--------------------------------------------------------------------------------------------------
SelectFileSettings::SelectFileSettings(const SelectFileSettings &settings)
{
	*this = settings;
}

//--------------------------------------------------------------------------------------------------
// Public methods
//--------------------------------------------------------------------------------------------------
string SelectFileSettings::getSummaryString()
{
	string strSummary = "All files ";
	switch (m_scope)
	{
	case eAllFiles:
		strSummary += "in the database";
		break;

	case eAllFilesForWhich:
		{
			strSummary += "for which the \"" + m_strAction + "\" action's status is \""
				+ m_strStatus + "\"";
			if (m_nStatus == kActionSkipped)
			{
				strSummary += " by " + m_strUser;
			}
		}
		break;

	case eAllFilesTag:
		{
			strSummary += "associated with ";
			switch(m_eTagType)
			{
			case eAnyTag:
				strSummary += "any";
				break;
			case eAllTag:
				strSummary += "all";
				break;
			case eNoneTag:
				strSummary += "none";
				break;
			default:
				THROW_LOGIC_ERROR_EXCEPTION("ELI29964");
			}
			strSummary += " of the following tags: ";
			string strTagString = "";
			for (vector<string>::iterator it = m_vecTags.begin(); it != m_vecTags.end(); it++)
			{
				if (!strTagString.empty())
				{
					strTagString += ", ";
				}
				strTagString += (*it);
			}
			strSummary += strTagString;
		}
		break;

	case eAllFilesQuery:
		strSummary += "selected by this custom query: SELECT FAMFile.ID FROM " + m_strSQL;
		break;

	case eAllFilesPriority:
		strSummary += "with a file processing priority of " + m_strPriority;
		break;

	default:
		THROW_LOGIC_ERROR_EXCEPTION("ELI26906");
	}

	if (m_bLimitByRandomCondition)
	{
		if (m_bRandomSubsetUsePercentage)
		{
			strSummary += ".\r\nThe scope of files will be further narrowed to a random "
				+ asString(m_nRandomAmount) + " percent subset.";
		}
		else
		{
			strSummary += ".\r\nThe scope of files will be further narrowed to a random "
				+ asString(m_nRandomAmount) + " file(s) subset.";
		}
	}

	return strSummary;
}
//--------------------------------------------------------------------------------------------------
string SelectFileSettings::buildQuery(const IFileProcessingDBPtr& ipFAMDB, const string& strSelect)
{
	ASSERT_ARGUMENT("ELI27722", ipFAMDB != NULL);
	
	string strQueryPart1 = "SELECT " + strSelect + " FROM ";
	string strQueryPart2;

	switch(m_scope)
	{
		// Query based on the action status
	case eAllFilesForWhich:
		{
			strQueryPart2 += "FAMFile ";

			// Check if comparing skipped status
			if (m_nStatus == kActionSkipped)
			{
				strQueryPart2 += "INNER JOIN SkippedFile ON FAMFile.ID = SkippedFile.FileID WHERE "
					"(SkippedFile.ActionID = " + asString(m_nActionID);
				string strUser = m_strUser;
				if (strUser != gstrANY_USER)
				{
					strQueryPart2 += " AND SkippedFile.UserName = '" + strUser + "'";
				}
				strQueryPart2 += ")";
			}
			else
			{
				// Get the status as a string
				string strStatus = ipFAMDB->AsStatusString((EActionStatus)m_nStatus);

				strQueryPart2 += "WHERE (ASC_" + m_strAction + " = '"
					+ strStatus + "')";
			}
		}
		break;

		// Query to export all the files
	case eAllFiles:
		{
			strQueryPart2 += "FAMFile";
		}
		break;

		// Export based on customer query
	case eAllFilesQuery:
		{
			// Get the query input by the user
			strQueryPart2 += m_strSQL;
		}
		break;

	case eAllFilesTag:
		{
			// Get the size and ensure there is at least 1 tag
			size_t nSize = m_vecTags.size();
			if (nSize == 0)
			{
				UCLIDException uex("ELI27724", "No tags specified.");
				throw uex;
			}

			string strMainQueryTemp = gstrQUERY_FILES_WITH_TAGS;

			replaceVariable(strMainQueryTemp, gstrTAG_QUERY_SELECT,
				strSelect);

			// Get the conjunction for the where clause (want the "any" behavior for
			// both the "any" and "none" case - to achieve none just negate the any)
			string strConjunction =
				m_eTagType == eAnyTag || m_eTagType == eNoneTag ? "\nUNION\n" : "\nINTERSECT\n";

			// For the "none" case select all files NOT in the "any" list
			if (m_eTagType == eNoneTag)
			{
				strQueryPart2 =
					"(SELECT [FileName] FROM [FAMFile] WHERE [FAMFile].[FileName] NOT IN ";
			}
			strQueryPart2 += "(" + strMainQueryTemp;
			replaceVariable(strQueryPart2, gstrTAG_NAME_VALUE, m_vecTags[0]);

			// Build the rest of the query
			for (size_t i=1; i < nSize; i++)
			{
				string strTemp = strMainQueryTemp;
				replaceVariable(strTemp, gstrTAG_NAME_VALUE, m_vecTags[i]);
				strQueryPart2 += strConjunction + strTemp;
			}

			// Need to add an extra paren in the "none" case
			if (m_eTagType == eNoneTag)
			{
				strQueryPart2 += ")";
			}
			strQueryPart2 += ") AS FAMFile";
		}
		break;

	case eAllFilesPriority:
		{
			strQueryPart2 += "FAMFile WHERE FAMFile.Priority = "
				+ asString((long)m_ePriority);
		}
		break;

	default:
		THROW_LOGIC_ERROR_EXCEPTION("ELI27723");
	}

	if (m_bLimitByRandomCondition)
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
			"SELECT TOP 0 " + strSelect + " INTO #OriginalResults FROM " + strQueryPart2 + "\r\n"
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
			"INSERT INTO #OriginalResults (" + strSelect + ") " + strQueryPart1 +
				strQueryPart2 + "\r\n"
			// Calculate the number to return (using SQL's PERCENT seems to be returning unexpected
			// results: 50% of 28 = 15)
			"SET @rowsToReturn = " + (m_bRandomSubsetUsePercentage ? 
				"CEILING(@@ROWCOUNT * " + asString(m_nRandomAmount)+ ".0 / 100) " :
				asString(m_nRandomAmount)) + "\r\n"
			"\r\n"
			// If the original query has an identity column, just directly return a random subset
			// in random order since I can't come up with a good way of respecting any specified
			// order by clause.
			"IF @queryHasIdentityColumn = 1\r\n"
			"	SELECT TOP (@rowsToReturn) * FROM #OriginalResults AS FAMFile ORDER BY NEWID()\r\n"
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
	else
	{
		// We don't need to return a sized randomized subset-- simply combine the query parts and
		// return;
		return strQueryPart1 + strQueryPart2;
	}
}
//--------------------------------------------------------------------------------------------------
IRandomMathConditionPtr SelectFileSettings::getRandomCondition()
{
	// For the time being, SQL proceedure is taking care of all random scenarios.
	return NULL;
}
//--------------------------------------------------------------------------------------------------