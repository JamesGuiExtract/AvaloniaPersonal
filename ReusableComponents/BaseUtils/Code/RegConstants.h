#pragma once

#include <string>
using namespace std;

// Constant for the root key that product settings will be saved 
const std::string gstrREG_ROOT_KEY = "Software\\Extract Systems";

const std::string gstrEMAIL_REG_PATH = "HKLM\\" + gstrREG_ROOT_KEY + "\\EmailSettings";

const std::string gstrRC_REG_PATH = gstrREG_ROOT_KEY + "\\ReusableComponents";

const std::string gstrCOM_COMPONENTS_REG_PATH = gstrRC_REG_PATH + "\\COMComponents";

const std::string gstrBASEUTILS_REG_PATH = gstrRC_REG_PATH + "\\BaseUtils";

const std::string gstrVENDORSPECIFIC_REG_PATH = gstrRC_REG_PATH + "\\VendorSpecificUtils";
