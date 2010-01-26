// RedactionCCConstants.h

#include <string>

using namespace std;

// Current meta data version
const int giCURRENT_META_DATA_VERSION = 4;

// Exemption codes tag
const string gstrEXEMPTION_CODES_TAG = "<ExemptionCodes>";

// Field type tag
const string gstrFIELD_TYPE_TAG = "<FieldType>";

// Default filename for redacted image files
const string gstrDEFAULT_REDACTED_IMAGE_FILENAME = "$InsertBeforeExt(<SourceDocName>,.redacted)";

// Default ID Shield data file name
const string gstrDEFAULT_TARGET_FILENAME = "<SourceDocName>.voa";

// Default message when using default settings
const string gstrDEFAULT_TARGET_MESSAGE = "You are using default settings.";

// Italic font is indicated by this value
const unsigned char gucIS_ITALIC = 255;