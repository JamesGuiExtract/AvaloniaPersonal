#pragma once

#include <string>
#include <memory>

#include <IConfigurationSettingsPersistenceMgr.h>

class ConfigMgrIF
{
public:
	ConfigMgrIF();

	// get current used ocr engine prog id as string
	std::string getOCREngineProgID();
	// set current ocr engine prog id
	void setOCREngineProgID(const std::string& strProgID);
	
private:
	static const std::string OPTION_FOLDER;
	static const std::string OCR_ENGINE_PROG_ID_KEY_NAME;
	static const std::string DEFAULT_PROG_ID;

	std::unique_ptr<IConfigurationSettingsPersistenceMgr> ma_pUserCfgMgr;
};