#include "stdafx.h"
#include "FAMDBAdminUtils.h"
#include "UCLIDException.h"

//-------------------------------------------------------------------------------------------------
// CFAMDBAdminUtils
//-------------------------------------------------------------------------------------------------
CFAMDBAdminUtils::CFAMDBAdminUtils()
{
}
//--------------------------------------------------------------------------------------------------
CFAMDBAdminUtils::~CFAMDBAdminUtils()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16551");
}

//-------------------------------------------------------------------------------------------------
// Public Methods
//--------------------------------------------------------------------------------------------------
void CFAMDBAdminUtils::addStatusInComboBox(CComboBox& comboStatus)
{
	// Insert the action status to the ComboBox
	// The items are inserted the same order as the EActionStatus in FAM
	comboStatus.InsertString(0, "Unattempted");
	comboStatus.InsertString(1, "Pending");
	comboStatus.InsertString(2, "Processing");
	comboStatus.InsertString(3, "Completed");
	comboStatus.InsertString(4, "Failed");
}
//--------------------------------------------------------------------------------------------------