#include "stdafx.h"
#include "ConfigMgrIF.h"

#include <RegistryPersistenceMgr.h>
#include <cpputil.h>
#include <UCLIDException.h>
#include <RegConstants.h>

using namespace std;

const string ConfigMgrIF::OPTION_FOLDER = "\\Options";
const string ConfigMgrIF::OCR_ENGINE_PROG_ID_KEY_NAME = "OCREngineProgID";
const string ConfigMgrIF::DEFAULT_PROG_ID = "SSOCR.ScansoftOCR.1";

//--------------------------------------------------------------------------------------------------
ConfigMgrIF::ConfigMgrIF()
{
	string strRootFolder = gstrREG_ROOT_KEY + string("\\InputFunnel");
	ma_pUserCfgMgr = unique_ptr<IConfigurationSettingsPersistenceMgr>(
		new RegistryPersistenceMgr(HKEY_CURRENT_USER, strRootFolder));
}
//--------------------------------------------------------------------------------------------------
string ConfigMgrIF::getOCREngineProgID()
{
	if (!ma_pUserCfgMgr->keyExists(OPTION_FOLDER, OCR_ENGINE_PROG_ID_KEY_NAME))
	{
		// set default to SSOCR
		ma_pUserCfgMgr->createKey(OPTION_FOLDER, OCR_ENGINE_PROG_ID_KEY_NAME, DEFAULT_PROG_ID);
		return DEFAULT_PROG_ID;
	}

	return ma_pUserCfgMgr->getKeyValue(OPTION_FOLDER, OCR_ENGINE_PROG_ID_KEY_NAME,
		DEFAULT_PROG_ID);
}
//--------------------------------------------------------------------------------------------------
void ConfigMgrIF::setOCREngineProgID(const string& strProgID)
{
	ma_pUserCfgMgr->setKeyValue(OPTION_FOLDER, OCR_ENGINE_PROG_ID_KEY_NAME, strProgID);
}
//--------------------------------------------------------------------------------------------------

