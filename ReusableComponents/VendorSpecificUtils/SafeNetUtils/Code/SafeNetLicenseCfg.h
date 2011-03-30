// SafeNetLicenseCfg.h: interface for the SafeNetLicenseCfg class.
//
//////////////////////////////////////////////////////////////////////

#pragma once

#include "SafeNetUtils.h"
#include <IConfigurationSettingsPersistenceMgr.h>
#include <string>
#include <memory>

using namespace std;

// Purpose:	To maintain the server information for the Safenet Keys in the registry
class SAFENETUTILS_API SafeNetLicenseCfg  
{
public:
	SafeNetLicenseCfg();
	virtual ~SafeNetLicenseCfg();

	// Sets the server name to use in the registry
	void setServerName( string strServerName );
	// Gets the value registry key for the server name
	string getContactServerName();

	void setAlertToList( string strAlertToList );
	string getAlertToList ();

	void setSendAlert( bool bSendAlert );
	bool getSendAlert();

	void setCounterAlertLevel( string strCounterName, DWORD nAlertLevel );
	DWORD getCounterAlertLevel( string strCounterName );

	void setCounterAlertMultiple( string strCounterName, DWORD nAlertMultiple );
	DWORD getCounterAlertMultiple( string strCounterName );

	void setSendToExtract( bool bSendToExtract);
	bool getSendToExtract();

	string getAlertSendAddress();
	string getAlertSendDisplay();

	// Gets the wait timeout for USB key retry
	double getRetryWaitTime();

	// Gets the number of retries for the USB key retry
	long getNumberRetries();

	// This gets the timeout for the dialog that displays when a counter is exhausted
	// -1 indicates no wait(dialog is not displayed and the counter decrement will fail
	// 0 indicates an infinite wait - dialog will be displayed until OK button is pressed
	// any other amount will be the number of seconds to wait before retrying
	int getWaitTimeoutForCounterOut();

private:
	// Handles registry settings
	std::unique_ptr<IConfigurationSettingsPersistenceMgr> ma_pUserCfgMgr;

};

