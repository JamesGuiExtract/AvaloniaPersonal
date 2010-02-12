#pragma once

#include <string>


// Folder name and file names under ComponentData folder
const std::string DOC_CLASSIFIERS_FOLDER = "DocumentClassifiers";
// the file containing all currently available document type names
// it is available under each industry category named folder
const std::string DOC_TYPE_INDEX_FILE = "DocTypes.idx";

/////////////////////////////////////////////////////////
// Used in AFDocument when creating tags for the document
/////////////////////////////////////////////////////////
const std::string DOC_TYPE = "DocType";
const std::string DOC_PROBABILITY = "DocProbability";
const std::string RULE_WORKED_TAG = "RuleWorked";
const std::string DCC_BLOCK_ID_TAG = "BlockID";
const std::string DCC_RULE_ID_TAG = "RuleID";

////////////////////////////////////////////////////////////////
// Defines industries each of which contain document classifiers
////////////////////////////////////////////////////////////////
const std::string gstrCOUNTY_DOC_INDUSTRY = "County Document";
const std::string gstrLEGAL_DESC_INDUSTRY = "Legal Descriptions";

//////////////////////////////////////////
// Defines special tags for document types
//////////////////////////////////////////
const std::string gstrSPECIAL_ANY_UNIQUE = "Any Unique";
const std::string gstrSPECIAL_MULTIPLE_CLASS = "Multiply Classified";
const std::string gstrSPECIAL_UNKNOWN = "Unknown";
