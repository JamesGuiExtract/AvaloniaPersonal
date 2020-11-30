#include "stdafx.h"
#include "FileSetCondition.h"
#include "FileSetConditionDlg.h"

#include <cpputil.h>
#include <Misc.h>
#include <UCLIDException.h>

//--------------------------------------------------------------------------------------------------
// FileSetCondition
//--------------------------------------------------------------------------------------------------
FileSetCondition::FileSetCondition(void)
{
}
//--------------------------------------------------------------------------------------------------
FileSetCondition::FileSetCondition(const FileSetCondition &settings)
{
	*this = settings;
}
//--------------------------------------------------------------------------------------------------
// Public methods
//--------------------------------------------------------------------------------------------------
bool FileSetCondition::configure(const UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr& ipFAMDB,
								  const string& strQueryHeader)
{
	FileSetCondition settings(*this);
	FileSetConditionDlg dialog(ipFAMDB, settings);
	if (dialog.DoModal() == IDOK)
	{
		*this = dialog.getSettings();
		return true;
	}

	return false;
}
//--------------------------------------------------------------------------------------------------
SelectFileCondition* FileSetCondition::clone()
{
	FileSetCondition *pClone = new FileSetCondition();
	*pClone = *this;
	return pClone;
}
//--------------------------------------------------------------------------------------------------
string FileSetCondition::getSummaryString(bool bFirstCondition)
{
	return "that are in the file set \"" + m_strFileSetName + "\"";
}
//--------------------------------------------------------------------------------------------------
string FileSetCondition::buildQuery(const UCLID_FILEPROCESSINGLib::IFileProcessingDBPtr& ipFAMDB, 
									const string& strSelect, long nWorkflowID)
{
	string strQuery;

	IVariantVectorPtr ipVariantVector = ipFAMDB->GetFileSetFileIDs(m_strFileSetName.c_str());
	ASSERT_RESOURCE_ALLOCATION("ELI37352", ipVariantVector != __nullptr);

	long nCount = ipVariantVector->Size;

	if (nCount == 0)
	{
		// If the file set is empty, the normal query will not end up with valid syntax. Use an
		// always FALSE WHERE clause instead.
		strQuery = "SELECT " + strSelect + " FROM FAMFile WITH (NOLOCK) WHERE 1 = 0";
	}
	else
	{
		strQuery = "SELECT " + strSelect + " FROM FAMFile WITH (NOLOCK) WHERE [ID] IN (";

		for (long i = 0; i < nCount; i++)
		{
			if (i > 0)
			{
				strQuery += ",";
			}
			strQuery += asString(ipVariantVector->Item[i].lVal);
		}

		strQuery += ")";
	}

	return strQuery;
}

