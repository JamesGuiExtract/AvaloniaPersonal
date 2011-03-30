
#pragma once

#include <IConfigurationSettingsPersistenceMgr.h>
#include <memory>

#include <vector>
#include <string>

//-------------------------------------------------------------------------------------------------
// class NotifyOptions
//-------------------------------------------------------------------------------------------------
enum ENotificationType
{
	kInvalidNotificationType = 0,
	kPopupNotification,
	kEmailNotification
};

enum ENotificationEvent
{
	kInvalidNotificationEvent = 0,
	kExceptionLogged,
	kApplicationCrashed,
	kExceptionsLoggedFrequently,
	kCPUUsageIsLow
};

class NotifyOptions
{
public:
	//---------------------------------------------------------------------------------------------
	// ctor
	NotifyOptions();
	//---------------------------------------------------------------------------------------------
	// PROMISE:	To return true only if the particular type of notification event
	//			has the particular type of notification enabled.
	bool notificationIsEnabled(ENotificationType eType, ENotificationEvent eEvent);
	//---------------------------------------------------------------------------------------------
	// REQUIRE:	isNotifyEnabled() == true
	// PROMISE:	To return a vector of strings representing the recipients that
	//			need to be notified when a failure is detected.
	IVariantVectorPtr getEmailRecipients() const;
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To get the settings associated with the kExceptionsLoggedFrequently
	//			notification event.
	unsigned long getNumExceptions() const;
	unsigned long getExceptionCheckDurationInSeconds() const;
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To get the settings associated with the kCPUUsageIsLow
	//			notification event.
	unsigned long getCPUThreshold() const;
	unsigned long getCPUCheckDurationInSeconds() const;
	//---------------------------------------------------------------------------------------------
	// show the user interface for editing the options
	void showUserInterface();
	//---------------------------------------------------------------------------------------------
	unsigned long getMinSecondsBetweenEmails(ENotificationEvent eEvent) const;
	//---------------------------------------------------------------------------------------------
	// Get the Ini file name
	const std::string& getINIFileName() const;

private:

	const std::string& getSettingsFolderName(ENotificationEvent eEvent) const;
	const std::string& getEnabledOrNotKeyName(ENotificationType eType) const;

	std::unique_ptr<IConfigurationSettingsPersistenceMgr> m_apSettings;
};
//-------------------------------------------------------------------------------------------------
