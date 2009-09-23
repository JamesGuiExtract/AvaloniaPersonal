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
m_nRandomPercent(0)
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
			strSummary += (m_bAnyTags ? "any" : "all");
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
		strSummary += ".\r\nThe scope of files will be further narrowed to a random "
			+ asString(m_nRandomPercent) + "% subset.";
	}

	return strSummary;
}
//--------------------------------------------------------------------------------------------------
string SelectFileSettings::buildQuery(const IFileProcessingDBPtr& ipFAMDB, const string& strSelect)
{
	ASSERT_ARGUMENT("ELI27722", ipFAMDB != NULL);

	string strQuery = "SELECT " + strSelect + " FROM ";
	switch(m_scope)
	{
		// Query based on the action status
	case eAllFilesForWhich:
		{
			strQuery += "FAMFile ";

			// Check if comparing skipped status
			if (m_nStatus == kActionSkipped)
			{
				strQuery += "INNER JOIN SkippedFile ON FAMFile.ID = SkippedFile.FileID WHERE "
					"(SkippedFile.ActionID = " + m_nActionID;
				string strUser = m_strUser;
				if (strUser != gstrANY_USER)
				{
					strQuery += " AND SkippedFile.UserName = '" + strUser + "'";
				}
				strQuery += ")";
			}
			else
			{
				// Get the status as a string
				string strStatus = ipFAMDB->AsStatusString((EActionStatus)m_nStatus);

				strQuery += "WHERE (ASC_" + m_strAction + " = '"
					+ strStatus + "')";
			}
		}
		break;

		// Query to export all the files
	case eAllFiles:
		{
			strQuery += "FAMFile";
		}
		break;

		// Export based on customer query
	case eAllFilesQuery:
		{
			// Get the query input by the user
			strQuery += m_strSQL;
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

			// Get the conjunction for the where clause
			string strConjunction = m_bAnyTags ? "\nUNION\n" : "\nINTERSECT\n";

			strQuery += "(" + strMainQueryTemp;
			replaceVariable(strQuery, gstrTAG_NAME_VALUE, m_vecTags[0]);

			// Build the rest of the query
			for (size_t i=1; i < nSize; i++)
			{
				string strTemp = strMainQueryTemp;
				replaceVariable(strTemp, gstrTAG_NAME_VALUE, m_vecTags[i]);
				strQuery += strConjunction + strTemp;
			}

			strQuery += ") AS FAMFile";
		}
		break;

	case eAllFilesPriority:
		{
			strQuery += "FAMFile WHERE FAMFile.Priority = "
				+ asString((long)m_ePriority);
		}
		break;

	default:
		THROW_LOGIC_ERROR_EXCEPTION("ELI27723");
	}

	return strQuery;
}
//--------------------------------------------------------------------------------------------------
IRandomMathConditionPtr SelectFileSettings::getRandomCondition()
{
	// Create the random math condition if necessary
	if (m_bLimitByRandomCondition)
	{
		IRandomMathConditionPtr ipRandomCondition(CLSID_RandomMathCondition);
		ASSERT_RESOURCE_ALLOCATION("ELI27730", ipRandomCondition != NULL);

		// Set the percentage
		ipRandomCondition->Percent = m_nRandomPercent;

		return ipRandomCondition;
	}

	return NULL;
}
//--------------------------------------------------------------------------------------------------