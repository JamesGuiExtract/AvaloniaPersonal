
#pragma once

#include <string>
#include <RegConstants.h>

// string constants for various commonly accessed registry keys/folders
const std::string gstrAF_REG_ROOT_FOLDER_PATH = gstrREG_ROOT_KEY + std::string("\\AttributeFinder");
const std::string gstrAF_REG_SETTINGS_FOLDER = "\\Settings";
const std::string gstrAF_REG_SETTINGS_FOLDER_PATH = gstrAF_REG_ROOT_FOLDER_PATH + gstrAF_REG_SETTINGS_FOLDER;
const std::string gstrAF_REG_UTILS_FOLDER_PATH = gstrAF_REG_ROOT_FOLDER_PATH + std::string("\\Utils");

const std::string gstrAF_AUTO_ENCRYPT_KEY = "AutoEncrypt";
const std::string gstrAF_DEFAULT_AUTO_ENCRYPT = "0";
const std::string gstrAF_AUTO_ENCRYPT_KEY_PATH = gstrAF_REG_SETTINGS_FOLDER_PATH + std::string("\\") + gstrAF_AUTO_ENCRYPT_KEY;

const std::string gstrAF_AFCORE_KEY = "AFCore";
const std::string gstrAF_AFCORE_KEY_PATH = gstrAF_REG_ROOT_FOLDER_PATH + std::string("\\") + gstrAF_AFCORE_KEY;

const std::string gstrAF_CACHE_RSD_KEY = "CacheRSD";
const std::string gstrAF_DEFAULT_CACHE_RSD = "0";

const std::string gstrRULE_EXEC_ID_TAG_NAME = "RuleExecutionID";
const std::string gstrRULE_EXEC_ID_TAG = "<" + gstrRULE_EXEC_ID_TAG_NAME + ">";
const std::string gstrRSD_FILE_OPEN_FILTER =	"Ruleset definition files (*.rsd;*.rsd.etf)|*.rsd;*.rsd.etf|"
												"Ruleset definition files (*.rsd)|*.rsd|"
												"Encrypted ruleset files (*rsd.etf)|*rsd.etf|"
												"All Files (*.*)|*.*||";

const std::string gstrAF_REG_EAVGENERATOR_FOLDER_PATH = 
	gstrAF_REG_UTILS_FOLDER_PATH + std::string("\\EAVGenerator");
