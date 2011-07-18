// SafeNetLicenseCfg.cpp: implementation of the SafeNetLicenseCfg class.
//
//////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "SafeNetLicenseCfg.h"

#include <UCLIDException.h>
#include <RegistryPersistenceMgr.h>
#include <RegConstants.h>
#include <cpputil.h>

#include <string>

using namespace std;

//-------------------------------------------------------------------------------------------------
//constants
//-------------------------------------------------------------------------------------------------
const string gstrSAFENET_UTILS_CFG_FOLDER  = "\\SafeNetUtils";
const string gstrSAFENET_SERVER_NAME = "ContactServer";
const string gstrDEFAULT_SAFENET_SERVER = "SP_LOCAL_MODE";

const string gstrDEFAULT_ALERT_SEND_ADDR = "support@extractsystems.com";
const string gstrDEFAULT_ALERT_SEND_DISPLAY = "Extract Systems LM";
const string gstrDEFAULT_WAIT_TIMEOUT_FOR_COUNTER_OUT = "60"; // Seconds
const string gstrDEFAULT_NUMBER_OF_USB_KEY_RETRIES = "10";
const string gstrDEFAULT_WAIT_TIMEOUT_FOR_USB_KEY_RETRY = "300"; // Seconds(5 min)

//-------------------------------------------------------------------------------------------------
// Construction/Destruction
//-------------------------------------------------------------------------------------------------
SafeNetLicenseCfg::SafeNetLicenseCfg()
{
	// create instance of the persistence mgr
	ma_pUserCfgMgr = unique_ptr<IConfigurationSettingsPersistenceMgr>(new 
		RegistryPersistenceMgr( HKEY_LOCAL_MACHINE, gstrREG_ROOT_KEY ));
	ASSERT_RESOURCE_ALLOCATION( "ELI11312", ma_pUserCfgMgr.get() != __nullptr );
}
//-------------------------------------------------------------------------------------------------
SafeNetLicenseCfg::~SafeNetLicenseCfg()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16489");
}
//-------------------------------------------------------------------------------------------------
string SafeNetLicenseCfg::getContactServerName()
{
	string strServerName;
	// if the key does not exist create it and give a default value of SP_LOCAL_MODE
	if (!ma_pUserCfgMgr->keyExists( gstrSAFENET_UTILS_CFG_FOLDER, gstrSAFENET_SERVER_NAME ))
	{
		ma_pUserCfgMgr->createKey( gstrSAFENET_UTILS_CFG_FOLDER, gstrSAFENET_SERVER_NAME, 
				getComputerName() );
		strServerName = getComputerName();
	}
	else
	{
		strServerName = ma_pUserCfgMgr->getKeyValue( 
			gstrSAFENET_UTILS_CFG_FOLDER, gstrSAFENET_SERVER_NAME, getComputerName());
	}
	return strServerName;
}
//-------------------------------------------------------------------------------------------------
void SafeNetLicenseCfg::setServerName(string strServerName)
{
	ma_pUserCfgMgr->setKeyValue( gstrSAFENET_UTILS_CFG_FOLDER, 
		gstrSAFENET_SERVER_NAME, strServerName );
}
//-------------------------------------------------------------------------------------------------
void SafeNetLicenseCfg::setAlertToList( string strAlertToList )
{
	ma_pUserCfgMgr->setKeyValue( gstrSAFENET_UTILS_CFG_FOLDER, 
		"AlertToList", strAlertToList );
}
//-------------------------------------------------------------------------------------------------
string SafeNetLicenseCfg::getAlertToList ()
{
	try
	{

		return ma_pUserCfgMgr->getKeyValue( 
			gstrSAFENET_UTILS_CFG_FOLDER, "AlertToList", "" );
	}
	catch (...)
	{
	}
	return "";
}
//-------------------------------------------------------------------------------------------------
void SafeNetLicenseCfg::setSendAlert( bool bSendAlert )
{
	if ( bSendAlert )
	{
		ma_pUserCfgMgr->setKeyValue( gstrSAFENET_UTILS_CFG_FOLDER, 
			"SendAlert", "1");
	}
	else
	{
		ma_pUserCfgMgr->setKeyValue( gstrSAFENET_UTILS_CFG_FOLDER, 
			"SendAlert", "0");
	}
}
//-------------------------------------------------------------------------------------------------
bool SafeNetLicenseCfg::getSendAlert()
{
	try
	{
		string strSendAlert;

		strSendAlert = ma_pUserCfgMgr->getKeyValue( 
			gstrSAFENET_UTILS_CFG_FOLDER, "SendAlert", "0" );
		if ( strSendAlert == "1" )
		{
			return true;
		}
	}
	catch (...)
	{
	}
	return false;
}
//-------------------------------------------------------------------------------------------------
void SafeNetLicenseCfg::setCounterAlertLevel( string strCounterName, DWORD nAlertLevel )
{
	ma_pUserCfgMgr->setKeyValue( gstrSAFENET_UTILS_CFG_FOLDER + "\\" + strCounterName, 
		"AlertLevel", asString(nAlertLevel));
}
//-------------------------------------------------------------------------------------------------
DWORD SafeNetLicenseCfg::getCounterAlertLevel( string strCounterName )
{
	try
	{
		string strAlertLevel;
		ma_pUserCfgMgr->createFolder( gstrSAFENET_UTILS_CFG_FOLDER + "\\" + strCounterName );
		strAlertLevel = ma_pUserCfgMgr->getKeyValue( 
			gstrSAFENET_UTILS_CFG_FOLDER + "\\" + strCounterName, "AlertLevel", "0" );
		return asUnsignedLong(strAlertLevel);
	}
	catch (...)
	{
	}
	// if not valid return 0
	return 0;
}
//-------------------------------------------------------------------------------------------------
void SafeNetLicenseCfg::setCounterAlertMultiple( string strCounterName, DWORD nAlertMultiple )
{
	ma_pUserCfgMgr->setKeyValue( gstrSAFENET_UTILS_CFG_FOLDER + "\\" + strCounterName, 
		"AlertMultiple", asString(nAlertMultiple));
}
//-------------------------------------------------------------------------------------------------
DWORD SafeNetLicenseCfg::getCounterAlertMultiple( string strCounterName )
{
	try
	{

		string strAlertMultiple;
		ma_pUserCfgMgr->createFolder( gstrSAFENET_UTILS_CFG_FOLDER + "\\" + strCounterName );
		strAlertMultiple = ma_pUserCfgMgr->getKeyValue( 
			gstrSAFENET_UTILS_CFG_FOLDER + "\\" + strCounterName, "AlertMultiple", "0" );
		return asUnsignedLong(strAlertMultiple);
	}
	catch (...)
	{
	}
	// if not valid return 0
	return 0;
}
//-------------------------------------------------------------------------------------------------
string SafeNetLicenseCfg::getAlertSendAddress()
{
	try
	{
		if ( !ma_pUserCfgMgr->keyExists( gstrSAFENET_UTILS_CFG_FOLDER, "AlertSendAddress" ) )
		{
			ma_pUserCfgMgr->setKeyValue( gstrSAFENET_UTILS_CFG_FOLDER, "AlertSendAddress", gstrDEFAULT_ALERT_SEND_ADDR );
			return gstrDEFAULT_ALERT_SEND_ADDR; 
		}
		return ma_pUserCfgMgr->getKeyValue( 
			gstrSAFENET_UTILS_CFG_FOLDER, "AlertSendAddress", gstrDEFAULT_ALERT_SEND_ADDR );
	}
	catch (...)
	{
	}
	return gstrDEFAULT_ALERT_SEND_ADDR;

}
//-------------------------------------------------------------------------------------------------
string SafeNetLicenseCfg::getAlertSendDisplay()
{
	try
	{
		if ( !ma_pUserCfgMgr->keyExists( gstrSAFENET_UTILS_CFG_FOLDER, "AlertSendDisplay" ) )
		{
			ma_pUserCfgMgr->setKeyValue( gstrSAFENET_UTILS_CFG_FOLDER, "AlertSendDisplay", gstrDEFAULT_ALERT_SEND_DISPLAY );
			return gstrDEFAULT_ALERT_SEND_DISPLAY; 
		}
		return ma_pUserCfgMgr->getKeyValue( 
			gstrSAFENET_UTILS_CFG_FOLDER, "AlertSendDisplay", gstrDEFAULT_ALERT_SEND_DISPLAY );
	}
	catch (...)
	{
	}
	return gstrDEFAULT_ALERT_SEND_DISPLAY;

}
//-------------------------------------------------------------------------------------------------
void SafeNetLicenseCfg::setSendToExtract( bool bSendToExtract )
{
	if ( bSendToExtract )
	{
		ma_pUserCfgMgr->setKeyValue( gstrSAFENET_UTILS_CFG_FOLDER, 
			"SendToExtract", "1");
	}
	else
	{
		ma_pUserCfgMgr->setKeyValue( gstrSAFENET_UTILS_CFG_FOLDER, 
			"SendToExtract", "0");
	}
}
//-------------------------------------------------------------------------------------------------
bool SafeNetLicenseCfg::getSendToExtract()
{
	try
	{
		string strSendToExtract;

		strSendToExtract= ma_pUserCfgMgr->getKeyValue( 
			gstrSAFENET_UTILS_CFG_FOLDER, "SendToEXtract", "0" );
		if ( strSendToExtract == "1" )
		{
			return true;
		}
	}
	catch (...)
	{
	}
	return false;
}
//-------------------------------------------------------------------------------------------------
int SafeNetLicenseCfg::getWaitTimeoutForCounterOut()
{
	try
	{
		if ( !ma_pUserCfgMgr->keyExists( gstrSAFENET_UTILS_CFG_FOLDER, "WaitTimeoutForCounterOut" ) )
		{
			ma_pUserCfgMgr->setKeyValue( gstrSAFENET_UTILS_CFG_FOLDER, "WaitTimeoutForCounterOut", gstrDEFAULT_WAIT_TIMEOUT_FOR_COUNTER_OUT );
			return asLong(gstrDEFAULT_WAIT_TIMEOUT_FOR_COUNTER_OUT); 
		}
		return asLong(ma_pUserCfgMgr->getKeyValue( 
			gstrSAFENET_UTILS_CFG_FOLDER, "WaitTimeoutForCounterOut", gstrDEFAULT_WAIT_TIMEOUT_FOR_COUNTER_OUT ));
	}
	catch (...)
	{
	}
	return asLong(gstrDEFAULT_WAIT_TIMEOUT_FOR_COUNTER_OUT);

}
//-------------------------------------------------------------------------------------------------
double SafeNetLicenseCfg::getRetryWaitTime()
{
	try
	{
		if ( !ma_pUserCfgMgr->keyExists( gstrSAFENET_UTILS_CFG_FOLDER, "WaitTimeoutUSBKeyRetry" ) )
		{
			ma_pUserCfgMgr->setKeyValue( gstrSAFENET_UTILS_CFG_FOLDER, "WaitTimeoutUSBKeyRetry", 
				gstrDEFAULT_WAIT_TIMEOUT_FOR_USB_KEY_RETRY );
			return asDouble(gstrDEFAULT_WAIT_TIMEOUT_FOR_USB_KEY_RETRY ); 
		}
		return asDouble(ma_pUserCfgMgr->getKeyValue( 
			gstrSAFENET_UTILS_CFG_FOLDER, "WaitTimeoutUSBKeyRetry", gstrDEFAULT_WAIT_TIMEOUT_FOR_USB_KEY_RETRY ));
	}
	catch (...)
	{
	}
	return asDouble(gstrDEFAULT_WAIT_TIMEOUT_FOR_USB_KEY_RETRY);
}
//-------------------------------------------------------------------------------------------------
long SafeNetLicenseCfg::getNumberRetries()
{
	try
	{
		if ( !ma_pUserCfgMgr->keyExists( gstrSAFENET_UTILS_CFG_FOLDER, "NumberOfUSBKeyRetries" ) )
		{
			ma_pUserCfgMgr->setKeyValue( gstrSAFENET_UTILS_CFG_FOLDER, "NumberOfUSBKeyRetries", 
				gstrDEFAULT_NUMBER_OF_USB_KEY_RETRIES);
			return asLong(gstrDEFAULT_WAIT_TIMEOUT_FOR_COUNTER_OUT); 
		}
		return asLong(ma_pUserCfgMgr->getKeyValue( 
			gstrSAFENET_UTILS_CFG_FOLDER, "NumberOfUSBKeyRetries", gstrDEFAULT_WAIT_TIMEOUT_FOR_COUNTER_OUT ));
	}
	catch (...)
	{
	}
	return asLong(gstrDEFAULT_NUMBER_OF_USB_KEY_RETRIES);
}
//-------------------------------------------------------------------------------------------------
