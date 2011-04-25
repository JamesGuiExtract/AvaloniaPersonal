
#include "stdafx.h"
#include "ImageEditCfg.h"
#include "resource.h"

#include <RegistryPersistenceMgr.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <RegConstants.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// Registry folder and key for settings
const string& gstrIMAGE_EDIT_REGISTRY_FOLDER = gstrRC_REG_PATH + "\\OcxAndDlls\\ImageEdit";
const string& gstrANTI_ALIASING = "AntiAliasing";
const string& gstrANNOTATION = "Annotation";

//-------------------------------------------------------------------------------------------------
// ImageEditCtrlCfg
//-------------------------------------------------------------------------------------------------
ImageEditCtrlCfg::ImageEditCtrlCfg()
{
	try
	{
		// Create an instance of the registry persistence manager
		m_apCfgMgr = unique_ptr<IConfigurationSettingsPersistenceMgr>(
			new RegistryPersistenceMgr(HKEY_CURRENT_USER, ""));
		ASSERT_RESOURCE_ALLOCATION("ELI13250", m_apCfgMgr.get() != __nullptr);
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI13251")
}
//-------------------------------------------------------------------------------------------------
ImageEditCtrlCfg::~ImageEditCtrlCfg()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16468");
}
//-------------------------------------------------------------------------------------------------
bool ImageEditCtrlCfg::isAntiAliasingEnabled()
{
	bool bIsEnabled = false;

	// Check key existence
	if (!m_apCfgMgr->keyExists( gstrIMAGE_EDIT_REGISTRY_FOLDER, gstrANTI_ALIASING ))
	{
		// Use default (OFF) if empty
		m_apCfgMgr->createKey( gstrIMAGE_EDIT_REGISTRY_FOLDER, gstrANTI_ALIASING, "1" );
	}

	// Convert found string to bool - functionality must also be licensed
	string strSetting = m_apCfgMgr->getKeyValue( gstrIMAGE_EDIT_REGISTRY_FOLDER, 
		gstrANTI_ALIASING );
	if (!strSetting.empty() && strSetting == "1" && isAntiAliasingLicensed())
	{
		bIsEnabled = true;
	}

	// Return validated result
	return bIsEnabled;
}
//-------------------------------------------------------------------------------------------------
void ImageEditCtrlCfg::setAntiAliasing(bool bEnable)
{
	// Default setting is OFF
	string strSetting = "0";
	if (bEnable)
	{
		strSetting = "1";
	}

	// Save the setting
	m_apCfgMgr->setKeyValue( gstrIMAGE_EDIT_REGISTRY_FOLDER, gstrANTI_ALIASING, 
		strSetting );
}
//-------------------------------------------------------------------------------------------------
bool ImageEditCtrlCfg::isAnnotationEnabled()
{
	bool bIsEnabled = false;

	// Check key existence
	if (!m_apCfgMgr->keyExists( gstrIMAGE_EDIT_REGISTRY_FOLDER, gstrANNOTATION ))
	{
		// Use default (ON) if empty
		m_apCfgMgr->createKey( gstrIMAGE_EDIT_REGISTRY_FOLDER, gstrANNOTATION, "1" );
	}

	// Convert found string to bool - functionality must also be licensed
	string strSetting = m_apCfgMgr->getKeyValue( gstrIMAGE_EDIT_REGISTRY_FOLDER, 
		gstrANNOTATION );
	if (!strSetting.empty() && strSetting == "1")
	{
		// Check annotation license
		if ( LicenseManagement::isAnnotationLicensed() )
		{
			bIsEnabled = true;
		}
	}

	// Return validated result
	return bIsEnabled;
}
//-------------------------------------------------------------------------------------------------
void ImageEditCtrlCfg::setAnnotation(bool bEnable)
{
	// Default setting is OFF
	string strSetting = "0";
	if (bEnable)
	{
		strSetting = "1";
	}

	// Save the setting
	m_apCfgMgr->setKeyValue( gstrIMAGE_EDIT_REGISTRY_FOLDER, gstrANNOTATION, 
		strSetting );
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
bool ImageEditCtrlCfg::isAntiAliasingLicensed()
{
	bool bLicensed = true;

	try
	{
		// Check Anti-Aliasing license
		validateAALicense();
	}
	catch (...)
	{
		// Anti-Aliasing component is not licensed
		bLicensed = false;
	}

	return bLicensed;
}
//-------------------------------------------------------------------------------------------------
void ImageEditCtrlCfg::validateAALicense()
{
	static const unsigned long ANTI_ALIASING_COMPONENT_ID = gnANTI_ALIASING_FEATURE;

	VALIDATE_LICENSE( ANTI_ALIASING_COMPONENT_ID, "ELI13410", 
			"Image Window Anti-Aliasing" );
}
//-------------------------------------------------------------------------------------------------
