
#include "stdafx.h"
#include "SpatialStringViewerCfg.h"
#include "resource.h"
#include "SpatialStringViewerDlg.h"

#include <RegistryPersistenceMgr.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <RegConstants.h>

const string& gstrROOT_REGISTRY_FOLDER = gstrREG_ROOT_KEY + "\\Utilities\\SpatialStringViewer";
const string& gstrLAST_POS_FOLDER = gstrROOT_REGISTRY_FOLDER + "\\LastPos";
const string& gstrLEFT_POS = "Left";
const string& gstrTOP_POS = "Top";
const string& gstrRIGHT_POS = "Right";
const string& gstrBOTTOM_POS = "Bottom";

const string& gstrFIND_FOLDER = gstrROOT_REGISTRY_FOLDER + "\\Find";
const string& gstrDISTRIBUTION_FOLDER = gstrROOT_REGISTRY_FOLDER + "\\Distribution";
const string& gstrSHOW_ADVANCED = "Show Advanced";
const string& gstrLAST_REG_EXP = "Last Reg Exp";

//-------------------------------------------------------------------------------------------------
SpatialStringViewerCfg::SpatialStringViewerCfg(CSpatialStringViewerDlg *pDlg)
:m_pDlg(pDlg)
{
	try
	{
		// create an instance of the registry persistence manager
		m_apCfgMgr = unique_ptr<IConfigurationSettingsPersistenceMgr>(new 
			RegistryPersistenceMgr(HKEY_CURRENT_USER, ""));
		ASSERT_RESOURCE_ALLOCATION("ELI06724", m_apCfgMgr.get() != __nullptr);
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI06725")
}
//-------------------------------------------------------------------------------------------------
SpatialStringViewerCfg::~SpatialStringViewerCfg()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16493");
}
//-------------------------------------------------------------------------------------------------
void SpatialStringViewerCfg::saveCurrentWindowPosition()
{
	// get the current position of the window
	CRect rect;
	m_pDlg->GetWindowRect(&rect);

	// create the necessary entries in the registry
	m_apCfgMgr->createFolder(gstrROOT_REGISTRY_FOLDER);
	m_apCfgMgr->createFolder(gstrLAST_POS_FOLDER);

	// store the window position in the registry
	m_apCfgMgr->setKeyValue(gstrLAST_POS_FOLDER, gstrLEFT_POS, asString(rect.left));
	m_apCfgMgr->setKeyValue(gstrLAST_POS_FOLDER, gstrTOP_POS, asString(rect.top));
	m_apCfgMgr->setKeyValue(gstrLAST_POS_FOLDER, gstrRIGHT_POS, asString(rect.right ));
	m_apCfgMgr->setKeyValue(gstrLAST_POS_FOLDER, gstrBOTTOM_POS, asString(rect.bottom));
}
//-------------------------------------------------------------------------------------------------
void SpatialStringViewerCfg::restoreLastWindowPosition()
{
	CRect rect;

	try
	{
		// get the values of left,right,top,bottom
		rect.left = asUnsignedLong(m_apCfgMgr->getKeyValue(gstrLAST_POS_FOLDER, gstrLEFT_POS));
		rect.top = asUnsignedLong(m_apCfgMgr->getKeyValue(gstrLAST_POS_FOLDER, gstrTOP_POS));
		rect.right = asUnsignedLong(m_apCfgMgr->getKeyValue(gstrLAST_POS_FOLDER, gstrRIGHT_POS));
		rect.bottom = asUnsignedLong(m_apCfgMgr->getKeyValue(gstrLAST_POS_FOLDER, gstrBOTTOM_POS));

		m_pDlg->MoveWindow(&rect);
	}
	catch (...)
	{
		// if any exceptions were caught while we tried to restore the last
		// window position, it's probably because the registry keys didn't exist.
		// if that's the case, just return
		return;
	}
}
//-------------------------------------------------------------------------------------------------
bool SpatialStringViewerCfg::isAdvancedShown()
{
	if (!m_apCfgMgr->keyExists(gstrFIND_FOLDER, gstrSHOW_ADVANCED))
	{
		// default to hide
		m_apCfgMgr->createKey(gstrFIND_FOLDER, gstrSHOW_ADVANCED, "0");
		return false;
	}

	return m_apCfgMgr->getKeyValue(gstrFIND_FOLDER, gstrSHOW_ADVANCED) == "1";
}
//-------------------------------------------------------------------------------------------------
void SpatialStringViewerCfg::showAdvanced(bool bShow)
{
	string strShow = bShow ? "1" : "0";
	m_apCfgMgr->setKeyValue(gstrFIND_FOLDER, gstrSHOW_ADVANCED, strShow);
}
//-------------------------------------------------------------------------------------------------
std::string SpatialStringViewerCfg::getLastRegularExpression()
{
	if (!m_apCfgMgr->keyExists(gstrFIND_FOLDER, gstrLAST_REG_EXP))
	{
		// default to empty
		m_apCfgMgr->createKey(gstrFIND_FOLDER, gstrLAST_REG_EXP, "");
	}

	return m_apCfgMgr->getKeyValue(gstrFIND_FOLDER, gstrLAST_REG_EXP);
}
//-------------------------------------------------------------------------------------------------
void SpatialStringViewerCfg::saveLastRegularExpression(std::string strRegExp)
{
	m_apCfgMgr->setKeyValue(gstrFIND_FOLDER, gstrLAST_REG_EXP, strRegExp);
}
//-------------------------------------------------------------------------------------------------
void SpatialStringViewerCfg::getLastFindWindowPos(int& x, int& y)
{
	// get the values of left,right,top,bottom
	try
	{
		x = asLong(m_apCfgMgr->getKeyValue(gstrFIND_FOLDER, gstrLEFT_POS));
		y = asLong(m_apCfgMgr->getKeyValue(gstrFIND_FOLDER, gstrTOP_POS));
	}
	catch(...)
	{
		x = 0;
		y = 0;
		return;
	}
}
//-------------------------------------------------------------------------------------------------
void SpatialStringViewerCfg::saveLastFindWindowPos(int x, int y)
{
	m_apCfgMgr->setKeyValue(gstrFIND_FOLDER, gstrLEFT_POS, asString(x));
	m_apCfgMgr->setKeyValue(gstrFIND_FOLDER, gstrTOP_POS, asString(y));
}
//-------------------------------------------------------------------------------------------------
void SpatialStringViewerCfg::getLastDistributionWindowPos(int &x, int &y)
{
	try
	{
		x = asLong(m_apCfgMgr->getKeyValue(gstrDISTRIBUTION_FOLDER, gstrLEFT_POS));
		y = asLong(m_apCfgMgr->getKeyValue(gstrDISTRIBUTION_FOLDER, gstrTOP_POS));
	}
	catch(...)
	{
		x = 0;
		y = 0;
	}
}
//-------------------------------------------------------------------------------------------------
void SpatialStringViewerCfg::saveLastDistributionWindowPos(int x, int y)
{
	m_apCfgMgr->setKeyValue(gstrDISTRIBUTION_FOLDER, gstrLEFT_POS, asString(x));
	m_apCfgMgr->setKeyValue(gstrDISTRIBUTION_FOLDER, gstrTOP_POS, asString(y));
}
//-------------------------------------------------------------------------------------------------
