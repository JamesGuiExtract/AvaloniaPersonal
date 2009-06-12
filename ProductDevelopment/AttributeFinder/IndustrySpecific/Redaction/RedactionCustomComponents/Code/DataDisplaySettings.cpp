#include "stdafx.h"
#include "DataDisplaySettings.h"

#include <UCLIDException.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

using namespace std;

//-------------------------------------------------------------------------------------------------
// DataDisplaySettings
//-------------------------------------------------------------------------------------------------
DataDisplaySettings::DataDisplaySettings(std::string strText, std::string strCategory, 
										 std::string strType, int iPage, 
										 ERedactChoice eChoice, bool bWarnNoRedact, bool bWarnRedact)
  : m_strText(strText),
    m_strCategory(strCategory),
	m_strType(strType),
    m_iPageNumber(iPage),
    m_eRedactChoice(eChoice),
	m_bReviewed(false),
	m_bWarnNoRedact(bWarnNoRedact),
	m_bWarnRedact(bWarnRedact)
{
}
//-------------------------------------------------------------------------------------------------
DataDisplaySettings::~DataDisplaySettings(void)
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI18523");
}
//-------------------------------------------------------------------------------------------------
std::vector<long> DataDisplaySettings::getHighlightIDs()
{
	return m_vecHighlightIDs;
}
//-------------------------------------------------------------------------------------------------
int DataDisplaySettings::getPageNumber(void)
{
	return m_iPageNumber;
}
//-------------------------------------------------------------------------------------------------
ERedactChoice DataDisplaySettings::getRedactChoice(void)
{
	return m_eRedactChoice;
}
//-------------------------------------------------------------------------------------------------
string DataDisplaySettings::getText(void)
{
	return m_strText;
}
//-------------------------------------------------------------------------------------------------
string DataDisplaySettings::getCategory(void)
{
	return m_strCategory;
}
//-------------------------------------------------------------------------------------------------
string DataDisplaySettings::getExemptionCodes()
{
	return m_strExemptionCodes;
}
//-------------------------------------------------------------------------------------------------
void DataDisplaySettings::setExemptionCodes(const string& strCodes)
{
	m_strExemptionCodes = strCodes;
}
//-------------------------------------------------------------------------------------------------
string DataDisplaySettings::getType(void)
{
	return m_strType;
}
//-------------------------------------------------------------------------------------------------
void DataDisplaySettings::setHighlightIDs(std::vector<long> m_vecIDs)
{
	m_vecHighlightIDs = m_vecIDs;
}
//-------------------------------------------------------------------------------------------------
void DataDisplaySettings::toggleRedactChoice(HWND hWndParent)
{
	switch (m_eRedactChoice)
	{
	// Yes becomes No
	case kRedactYes:
		// Check for user warning
		if (m_bWarnNoRedact)
		{
			// Present warning to user
			int iRes = MessageBox(hWndParent,
				"This item was tagged for redaction.  Change to no redaction?", 
				"Confirm Non-Redaction", MB_YESNO | MB_ICONQUESTION | MB_DEFBUTTON2);

			// Toggle to not redacted if user confirms
			if (iRes == IDYES)
			{
				m_eRedactChoice = kRedactNo;
			}
		}
		else
		{
			// No warning, just toggle to not redacted
			m_eRedactChoice = kRedactNo;
		}
		break;

	// No and N/A become Yes
	case kRedactNo:
	case kRedactNA:
		// Check for user warning
		if (m_bWarnRedact)
		{
			// Present warning to user
			int iRes = MessageBox( hWndParent, 
				"This item was not tagged for redaction.  Change to redacted?", 
				"Confirm Redaction", MB_YESNO | MB_ICONQUESTION | MB_DEFBUTTON2);

			// Toggle to redacted if user confirms
			if (iRes == IDYES)
			{
				m_eRedactChoice = kRedactYes;
			}
		}
		else
		{
			// No warning, just toggle to redacted
			m_eRedactChoice = kRedactYes;
		}
		break;

	default:
		// We should never reach here
		THROW_LOGIC_ERROR_EXCEPTION("ELI11232")
	}
}
//-------------------------------------------------------------------------------------------------
bool DataDisplaySettings::getReviewed()
{
	return m_bReviewed;
}
//-------------------------------------------------------------------------------------------------
void DataDisplaySettings::setReviewed()
{
	m_bReviewed = true;
}
//-------------------------------------------------------------------------------------------------
void DataDisplaySettings::setType(const string& strNewType)
{
	m_strType = strNewType;
}
//-------------------------------------------------------------------------------------------------
