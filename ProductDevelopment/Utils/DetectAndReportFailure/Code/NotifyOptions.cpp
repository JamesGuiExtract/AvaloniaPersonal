
#include "stdafx.h"
#include "NotifyOptions.h"

#include <cpputil.h>
#include <UCLIDException.h>
#include <INIFilePersistenceMgr.h>

using namespace std;

//-------------------------------------------------------------------------------------------------
// NotifyOptions
//-------------------------------------------------------------------------------------------------
NotifyOptions::NotifyOptions()
{
	try
	{
		// initialize the INI file persistence manager
		m_apSettings.reset(new INIFilePersistenceMgr(getINIFileName()));
		ASSERT_RESOURCE_ALLOCATION("ELI12374", m_apSettings.get() != __nullptr);
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI12433")
}
//-------------------------------------------------------------------------------------------------
bool NotifyOptions::notificationIsEnabled(ENotificationType eType, ENotificationEvent eEvent)
{
	try
	{
		// return whether failure reporting is enabled or not at a high level
		return m_apSettings->getKeyValue(getSettingsFolderName(eEvent),
			getEnabledOrNotKeyName(eType), "") == "1";
	}
	catch (UCLIDException& ue)
	{
		UCLIDException uexOuter("ELI12471", "Unable to retrieve notification enabled status!", ue);
		uexOuter.addDebugInfo("eType", (int) eType);
		uexOuter.addDebugInfo("eEvent", (int) eEvent);
		throw uexOuter;
	}
}
//-------------------------------------------------------------------------------------------------
IVariantVectorPtr NotifyOptions::getEmailRecipients() const
{
	try
	{
		IVariantVectorPtr ipRecipients(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI12428", ipRecipients != NULL);

		// get number of recipients
		static const string strRECIPIENTS_FOLDER = "EmailRecipients";
		string strNumRecipients = m_apSettings->getKeyValue(strRECIPIENTS_FOLDER, "NumRecipients", "");
		unsigned long ulNumRecipients = asUnsignedLong(strNumRecipients);

		// populate the vector with each of the recipients
		for (unsigned int ui = 0; ui < ulNumRecipients; ui++)
		{
			string strKey = "Recipient";
			strKey += asString(ui + 1);
			string strRecipient = m_apSettings->getKeyValue(strRECIPIENTS_FOLDER, strKey, "");
			ipRecipients->PushBack(_bstr_t(strRecipient.c_str()));
		}

		// return the list of recipients
		return ipRecipients;
	}
	catch (UCLIDException& ue)
	{
		UCLIDException uexOuter("ELI12378", "Unable to retrieve recipients list!", ue);
		throw uexOuter;
	}
}
//-------------------------------------------------------------------------------------------------
unsigned long NotifyOptions::getNumExceptions() const
{
	try
	{
		// return the number of exceptions
		string strNumExceptions = m_apSettings->getKeyValue(
			getSettingsFolderName(kExceptionsLoggedFrequently), "NumExceptions", "");
		return asUnsignedLong(strNumExceptions);
	}
	catch (UCLIDException& ue)
	{
		UCLIDException uexOuter("ELI12384", "Unable to retrieve value for # of exceptions!", ue);
		throw uexOuter;
	}
}
//-------------------------------------------------------------------------------------------------
unsigned long NotifyOptions::getExceptionCheckDurationInSeconds() const
{
	try
	{
		// return the time period
		string strTimePeriod = m_apSettings->getKeyValue(
			getSettingsFolderName(kExceptionsLoggedFrequently), "ExceptionCheckDurationInSeconds", "");
		return asUnsignedLong(strTimePeriod);
	}
	catch (UCLIDException& ue)
	{
		UCLIDException uexOuter("ELI12385", "Unable to retrieve value for the exception check time duration!", ue);
		throw uexOuter;
	}
}
//-------------------------------------------------------------------------------------------------
unsigned long NotifyOptions::getCPUThreshold() const
{
	try
	{
		// return the CPU usage percent threshold
		string strThreshold = m_apSettings->getKeyValue(
			getSettingsFolderName(kCPUUsageIsLow), "CPUThreshold", "");
		
		// ensure that the threshold is not greater than 100
		unsigned long ulThreshold = asUnsignedLong(strThreshold);
		if (ulThreshold > 100)
		{
			ulThreshold = 100;
		}

		return ulThreshold;
	}
	catch (UCLIDException& ue)
	{
		UCLIDException uexOuter("ELI12670", "Unable to retrieve value for CPUThreshold!", ue);
		throw uexOuter;
	}
}
//-------------------------------------------------------------------------------------------------
unsigned long NotifyOptions::getCPUCheckDurationInSeconds() const
{
	try
	{
		// return the time period
		string strTimePeriod = m_apSettings->getKeyValue(
			getSettingsFolderName(kCPUUsageIsLow), "CPUCheckDurationInSeconds", "");
		return asUnsignedLong(strTimePeriod);
	}
	catch (UCLIDException& ue)
	{
		UCLIDException uexOuter("ELI12671", "Unable to retrieve value for the CPU check time duration!", ue);
		throw uexOuter;
	}
}
//-------------------------------------------------------------------------------------------------
unsigned long NotifyOptions::getMinSecondsBetweenEmails(ENotificationEvent eEvent) const
{
	try
	{
		// return the time period
		string strTimePeriod = m_apSettings->getKeyValue(
			getSettingsFolderName(eEvent), "MinSecondsBetweenEmails", "");
		return asUnsignedLong(strTimePeriod);
	}
	catch (UCLIDException& ue)
	{
		UCLIDException uexOuter("ELI12674", "Unable to retrieve value for the MinSecondsBetweenEmails!", ue);
		uexOuter.addDebugInfo("eType", (unsigned long) eEvent);
		throw uexOuter;
	}
}
//-------------------------------------------------------------------------------------------------
void NotifyOptions::showUserInterface()
{
	// for now, bring up the INI file in notepad
	runEXE("notepad.exe", getINIFileName());
}

//-------------------------------------------------------------------------------------------------
// Private/Helper functions
//-------------------------------------------------------------------------------------------------
const string& NotifyOptions::getINIFileName() const
{
	static string strINIFileName;
	if (strINIFileName.empty())
	{
		// ensure that we can access the CWinApp object
		CWinApp *pApp = AfxGetApp();
		ASSERT_ARGUMENT("ELI12373", pApp != NULL);
		
		strINIFileName = getExtractApplicationDataPath() + "\\FDRS\\DetectAndReportFailure.ini";
	}

	return strINIFileName;
}
//-------------------------------------------------------------------------------------------------
const std::string& NotifyOptions::getSettingsFolderName(ENotificationEvent eEvent) const
{
	static const string strEXCEPTION_LOGGED_SETTINGS_FOLDER = "ExceptionLoggedEvent";
	static const string strAPP_CRASHED_SETTINGS_FOLDER = "ApplicationCrashedEvent";
	static const string strEXCEPTIONS_LOGGED_FREQUENTLY_SETTINGS_FOLDER = "ExceptionsLoggedFrequentlyEvent";
	static const string strCPU_USAGE_IS_LOW_SETTINGS_FOLDER = "CPUUsageIsLowEvent";

	switch (eEvent)
	{
	case kExceptionLogged:
		return strEXCEPTION_LOGGED_SETTINGS_FOLDER;
	
	case kApplicationCrashed:
		return strAPP_CRASHED_SETTINGS_FOLDER;
	
	case kExceptionsLoggedFrequently:
		return strEXCEPTIONS_LOGGED_FREQUENTLY_SETTINGS_FOLDER;

	case kCPUUsageIsLow:
		return strCPU_USAGE_IS_LOW_SETTINGS_FOLDER;

	default:
		// we should never reach here
		THROW_LOGIC_ERROR_EXCEPTION("ELI12474")
	}
}
//-------------------------------------------------------------------------------------------------
const std::string& NotifyOptions::getEnabledOrNotKeyName(ENotificationType eType) const
{
	static const string strNOTIFY_BY_EMAIL = "NotifyByEmail";
	static const string strNOTIFY_BY_POPUP = "NotifyByPopup";

	switch (eType)
	{
	case kPopupNotification:
		return strNOTIFY_BY_POPUP;
	
	case kEmailNotification:
		return strNOTIFY_BY_EMAIL;
	
	default:
		// we should never reach here
		THROW_LOGIC_ERROR_EXCEPTION("ELI12475")
	}
}
//-------------------------------------------------------------------------------------------------
