#include "stdafx.h"
#include "IDShieldData.h"

#include <UCLIDException.h>
#include <COMUtils.h>

//-------------------------------------------------------------------------------------------------
// IDShieldData Class
//-------------------------------------------------------------------------------------------------
IDShieldData::IDShieldData(void)
{
	clear();
}
//-------------------------------------------------------------------------------------------------
IDShieldData::IDShieldData(IIUnknownVectorPtr ipAttributes)
{
	calculateFromVector(ipAttributes);
}
//-------------------------------------------------------------------------------------------------
IDShieldData::~IDShieldData(void)
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20390");
}
//-------------------------------------------------------------------------------------------------
void IDShieldData::clear()
{
	m_lNumHCDataFound = 0;
	m_lNumMCDataFound = 0;
	m_lNumLCDataFound = 0;
	m_lNumCluesFound = 0;
	m_lTotalRedactions = 0;
	m_lTotalManualRedactions = 0;
	m_lNumPagesAutoAdvanced = 0;
}
//-------------------------------------------------------------------------------------------------
void IDShieldData::calculateFromVector(IIUnknownVectorPtr ipAttributes, const set<string>& setRedactLabels)
{
	// Clear the counts
	clear();

	// Process all attributes
	long nSize = ipAttributes->Size();
	for (long n = 0; n < nSize; n++)
	{
		IAttributePtr ipAttr = ipAttributes->At(n);
		ASSERT_RESOURCE_ALLOCATION("ELI19036", ipAttr != NULL);

		// Get the attribute name
		string strName = asString(ipAttr->Name);

		// Determine if the redaction totals should be updated
		bool bAddToTotals = setRedactLabels.find(strName) != setRedactLabels.end();

		// Count as redacted or not redacted
		addToCounts(strName, bAddToTotals);
	}
}
//-------------------------------------------------------------------------------------------------
void IDShieldData::calculateFromVector(IIUnknownVectorPtr ipAttributes)
{
	// Clear the counts
	clear();

	// Count all attributes as redacted
	long nSize = ipAttributes->Size();
	for (long n = 0; n < nSize; n++)
	{
		countRedacted(ipAttributes->At(n));
	}
}
//-------------------------------------------------------------------------------------------------
void IDShieldData::countRedacted(IAttributePtr ipAttribute)
{
	ASSERT_ARGUMENT("ELI19113", ipAttribute != NULL);

	// Get the attribute name
	string strName = asString(ipAttribute->Name);

	// Add to counts and to redaction totals
	addToCounts(strName, true);
}
//-------------------------------------------------------------------------------------------------
void IDShieldData::countNotRedacted(IAttributePtr ipAttribute)
{
	ASSERT_ARGUMENT("ELI19114", ipAttribute != NULL);

	// Get the attribute name
	string strName = asString(ipAttribute->Name);

	// Add to counts but not redaction totals
	addToCounts(strName, false);
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void IDShieldData::addToCounts(const string& strLabel, bool bAddToTotals)
{
	if (strLabel == gstrHCDATA_LABEL)
	{
		m_lNumHCDataFound++;
	}
	else if (strLabel == gstrMCDATA_LABEL)
	{
		m_lNumMCDataFound++;
	}
	else if (strLabel == gstrLCDATA_LABEL)
	{
		m_lNumLCDataFound++;
	}
	else if (strLabel == gstrCLUES_LABEL)
	{
		m_lNumCluesFound++;
	}
	else if (strLabel == gstrMANUAL_LABEL)
	{
		m_lTotalManualRedactions++;
	}

	// Check for add to totals
	if (bAddToTotals)
	{
		m_lTotalRedactions++;
	}
}
//-------------------------------------------------------------------------------------------------
