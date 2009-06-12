#pragma once

#include <string>

using namespace std;

////////////////////
// General Constants
////////////////////

// Auto zoom has 10 zoom level
// Each zoom level corresponds to the a stop in the sliderbar
const int	giZOOM_LEVEL_COUNT = 10;
const int	giZOOM_LEVEL[giZOOM_LEVEL_COUNT] = {
	1, 3, 5, 7, 9, 12, 15, 18, 21, 25};

// Set the default zoom level index to 3
const int	giDEFAULT_ZOOM_LEVEL_INDEX = 3;

// String constant for default INI file
const string	gstrINI_FILE = "IDShield.ini";

///////////////////////////////////////////////////
// Constants for [General] Settings within INI file
///////////////////////////////////////////////////

// String constant for General Section
const string	gstrGENERAL_SECTION = "General";

// String constant for number of levels
const string	gstrNUM_LEVELS = "NumConfidenceLevels";

// String constant for Include Pages setting
const string	gstrINCLUDE_PAGES = "IncludePages";

// String constant for Auto-zoom setting
const string	gstrAUTO_ZOOM = "AutoZoom";

// String constant for Auto-zoom's scaling amount setting
const string gstrAUTO_ZOOM_SCALE = "AutoZoomScale";

// String constant for tool to auto-select after creating a highlight
const string	gstrAUTO_SELECT_TOOL = "AutoPan";

// String constant for the color of zone entity's selection border
const string gstrSELECTION_COLOR = "SelectionBorderColor";

// String constant for whether to open up a voa file when saving with the SHIFT key held down
const string gstrOPEN_VOA_ON_SHIFT_SAVE = "OpenVoaOnShiftSave";

/////////////////////////////////////////////////
// Constants for [Input] Settings within INI file
/////////////////////////////////////////////////

// String constant for Input Section
const string	gstrINPUT_SECTION = "Input";

// String constant for Document Verification - unused after 2.0.0.8 (see below)
const string	gstrDOCUMENT_VERIFICATION = "DocumentVerification";

// String constant for Verify All documents - used after 2.0.0.8
const string	gstrVERIFY_ALL_DOCUMENTS = "VerifyAllDocuments";

// String constant for Verify Data documents - used after 2.0.0.8
const string	gstrVERIFY_DATA_DOCUMENTS = "VerifyDataDocuments";

// String constant for Verify Type documents - used after 2.0.0.8
const string	gstrVERIFY_TYPE_DOCUMENTS = "VerifyTypedDocuments";

// String constant for Document Types to be verified - used after 2.0.0.8
const string	gstrVERIFY_DOCUMENT_TYPES = "VerifyDocumentTypes";

/////////////////////////////////////////////////
// Constants for [Image] Settings within INI file
/////////////////////////////////////////////////

// String constant for Image Section
const string	gstrIMAGE_SECTION = "Image";

// String constant for Image Creation
const string	gstrFORCE_IMAGE_CREATION = "ForceImageCreation";

// String constant for Image Name
const string	gstrIMAGE_NAME = "ImageName";

////////////////////////////////////////////////////
// Constants for [Metadata] Settings within INI file
////////////////////////////////////////////////////

// String constant for Metadata Section
const string	gstrMETADATA_SECTION = "Metadata";

// String constant for Metadata Creation
const string	gstrMETADATA_CREATION = "MetadataCreation";

// String constant for Metadata Name
const string	gstrMETADATA_NAME = "MetadataName";

//////////////////////////////////////////////////
// Constants for [LevelX] Settings within INI file
//////////////////////////////////////////////////

// String constant for prefix to Level Section
const string	gstrLEVEL_SECTION_PREFIX = "Level";

// String constant for Long Name
const string	gstrLONG_NAME = "LongName";

// String constant for Short Name
const string	gstrSHORT_NAME = "ShortName";

// String constant for Query as applied to IIUnknownVector of IAttributes
const string	gstrQUERY = "Query";

// String constant for Color
const string	gstrDISPLAY_COLOR = "Color";

// String constant for Output
const string	gstrOUTPUT = "Output";

// String constant for Display
const string	gstrDISPLAY = "Display";

// String constant for Verification
const string	gstrVERIFY = "Verify";

// String constant for Warning user if normally redacted item is toggled OFF
const string	gstrWARN_IF_TOGGLE_OFF = "WarnIfNonRedact";

// String constant for Warning user if normally non-redacted item is toggled ON
const string	gstrWARN_IF_TOGGLE_ON  = "WarnIfRedact";

////////////////////////////////////////////////////////////////////
// Constants for [RedactionDataTypes] Settings within INI file
////////////////////////////////////////////////////////////////////
// added as per [p16 #2379]
const string	gstrREDACTION_DATA_TYPES_SECTION = "RedactionDataTypes";

// string constant for the number of redaction data types
const string	gstrREDACTION_TYPES_COUNT = "NumRedactionDataTypes";

// string constant for the prefix to each redaction data type
const string	gstrREDACTION_TYPES_PREFIX = "RedactionDataType";

// string constant for the default redaction data type
const string	gstrREDACTION_DEFAULT_TYPE = "DefaultRedactionDataType";