#include "stdafx.h"
#include "RedactionCustomComponents.h"
#include "RedactionUISettings.h"
#include "RedactionCCConstants.h"

#include <ByteStreamManipulator.h>
#include <UCLIDException.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const unsigned long gnCurrentVersion = 11;
const string gstrDEFAULT_META_FILE_NAME = "<SourceDocName>.xml";
const string gstrDEFAULT_FEEDBACK_FOLDER = "$DirOf(<SourceDocName>)\\FeedbackData";
const string gstrDEFAULT_REDACTION_TEXT = gstrEXEMPTION_CODES_TAG;

//-------------------------------------------------------------------------------------------------
// RedactionUISettings
//-------------------------------------------------------------------------------------------------
RedactionUISettings::RedactionUISettings(void)
:	m_bReviewAllPages(true),
	m_bAlwaysOutputImage(false),
	m_strOutputImageName(gstrDEFAULT_REDACTED_IMAGE_FILENAME),
	m_bAlwaysOutputMeta(true),
	m_strMetaOutputName(gstrDEFAULT_META_FILE_NAME),
	m_strInputDataFile(gstrDEFAULT_TARGET_FILENAME),
	m_bInputRedactedImage(false),
	m_bCarryForwardAnnotations(false),
	m_bApplyRedactionsAsAnnotations(false),
	m_bCollectFeedback(false),
	m_eFeedbackCollectOption(
		(UCLID_REDACTIONCUSTOMCOMPONENTSLib::EFeedbackCollectOption)kFeedbackCollectAll),
	m_strFeedbackDataFolder(gstrDEFAULT_FEEDBACK_FOLDER),
	m_bCollectFeedbackImage(true),
	m_bRequireRedactionTypes(true),
	m_bFeedbackOriginalFilenames(true),
	m_bRequireExemptionCodes(false),
	m_strRedactionText(gstrDEFAULT_REDACTION_TEXT),
	m_crBorderColor(0),
	m_crFillColor(0),
	m_iFontSize(8)
{
	memset(&m_lgFont, 0, sizeof(LOGFONT));
	lstrcpyn(m_lgFont.lfFaceName, "Times New Roman", LF_FACESIZE);
	m_lgFont.lfWeight = FW_NORMAL;
}
//-------------------------------------------------------------------------------------------------
RedactionUISettings::RedactionUISettings(RedactionUISettings &rUISettings)
{
	m_bReviewAllPages = rUISettings.m_bReviewAllPages;
	m_bAlwaysOutputImage = rUISettings.m_bAlwaysOutputImage;
	m_strOutputImageName = rUISettings.m_strOutputImageName;
	m_bAlwaysOutputMeta = rUISettings.m_bAlwaysOutputMeta;
	m_strMetaOutputName = rUISettings.m_strMetaOutputName;
	m_bCarryForwardAnnotations = rUISettings.m_bCarryForwardAnnotations;
	m_bApplyRedactionsAsAnnotations = rUISettings.m_bApplyRedactionsAsAnnotations;
	m_bCollectFeedback = rUISettings.m_bCollectFeedback;
	m_eFeedbackCollectOption = rUISettings.m_eFeedbackCollectOption;
	m_strFeedbackDataFolder = rUISettings.m_strFeedbackDataFolder;
	m_bCollectFeedbackImage = rUISettings.m_bCollectFeedbackImage;
	m_bFeedbackOriginalFilenames = rUISettings.m_bFeedbackOriginalFilenames;
	m_bRequireRedactionTypes = rUISettings.m_bRequireRedactionTypes;
	m_strInputDataFile = rUISettings.m_strInputDataFile;
	m_bInputRedactedImage = rUISettings.m_bInputRedactedImage;
	m_bRequireExemptionCodes = rUISettings.m_bRequireExemptionCodes;
	m_strRedactionText = rUISettings.m_strRedactionText;
	m_crBorderColor = rUISettings.m_crBorderColor;
	m_crFillColor = rUISettings.m_crFillColor;
	m_lgFont = rUISettings.m_lgFont;
	m_iFontSize = rUISettings.m_iFontSize;
}
//-------------------------------------------------------------------------------------------------
RedactionUISettings &RedactionUISettings::operator =(RedactionUISettings &rUISettings)
{
	m_bReviewAllPages = rUISettings.m_bReviewAllPages;
	m_bAlwaysOutputImage = rUISettings.m_bAlwaysOutputImage;
	m_strOutputImageName = rUISettings.m_strOutputImageName;
	m_bAlwaysOutputMeta = rUISettings.m_bAlwaysOutputMeta;
	m_strMetaOutputName = rUISettings.m_strMetaOutputName;
	m_bCarryForwardAnnotations = rUISettings.m_bCarryForwardAnnotations;
	m_bApplyRedactionsAsAnnotations = rUISettings.m_bApplyRedactionsAsAnnotations;
	m_bCollectFeedback = rUISettings.m_bCollectFeedback;
	m_eFeedbackCollectOption = rUISettings.m_eFeedbackCollectOption;
	m_strFeedbackDataFolder = rUISettings.m_strFeedbackDataFolder;
	m_bCollectFeedbackImage = rUISettings.m_bCollectFeedbackImage;
	m_bFeedbackOriginalFilenames = rUISettings.m_bFeedbackOriginalFilenames;
	m_bRequireRedactionTypes = rUISettings.m_bRequireRedactionTypes;
	m_strInputDataFile = rUISettings.m_strInputDataFile;
	m_bInputRedactedImage = rUISettings.m_bInputRedactedImage;
	m_bRequireExemptionCodes = rUISettings.m_bRequireExemptionCodes;
	m_strRedactionText = rUISettings.m_strRedactionText;
	m_crBorderColor = rUISettings.m_crBorderColor;
	m_crFillColor = rUISettings.m_crFillColor;
	m_lgFont = rUISettings.m_lgFont;
	m_iFontSize = rUISettings.m_iFontSize;

	return *this;
}
//-------------------------------------------------------------------------------------------------
RedactionUISettings::~RedactionUISettings(void)
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16481");
}
//-------------------------------------------------------------------------------------------------
bool RedactionUISettings::getReviewAllPages()
{
	return m_bReviewAllPages;
}
//-------------------------------------------------------------------------------------------------
void RedactionUISettings::setReviewAllPages(bool bReviewAllPages)
{
	m_bReviewAllPages = bReviewAllPages;
}
//-------------------------------------------------------------------------------------------------
bool RedactionUISettings::getAlwaysOutputImage()
{
	return m_bAlwaysOutputImage;
}
//-------------------------------------------------------------------------------------------------
void RedactionUISettings::setAlwaysOutputImage( bool bAlwaysOutputImage )
{
	m_bAlwaysOutputImage = bAlwaysOutputImage;
}
//-------------------------------------------------------------------------------------------------
std::string RedactionUISettings::getOutputImageName()
{
	return m_strOutputImageName;
}
//-------------------------------------------------------------------------------------------------
void RedactionUISettings::setOutputImageName(const std::string strOutputImageName)
{
	m_strOutputImageName = strOutputImageName;
}
//-------------------------------------------------------------------------------------------------
bool RedactionUISettings::getAlwaysOutputMeta()
{
	return m_bAlwaysOutputMeta;
}
//-------------------------------------------------------------------------------------------------
void RedactionUISettings::setAlwaysOutputMeta(bool bAlwaysOutputMeta)
{
	m_bAlwaysOutputMeta = bAlwaysOutputMeta;
}
//-------------------------------------------------------------------------------------------------
std::string RedactionUISettings::getMetaOutputName()
{
	return m_strMetaOutputName;
}
//-------------------------------------------------------------------------------------------------
void RedactionUISettings::setMetaOutputName(const std::string strMetaOutputName)
{
	m_strMetaOutputName = strMetaOutputName;
}
//-------------------------------------------------------------------------------------------------
bool RedactionUISettings::getCarryForwardAnnotations()
{
	// Return setting to caller
	return m_bCarryForwardAnnotations;
}
//-------------------------------------------------------------------------------------------------
void RedactionUISettings::setCarryForwardAnnotations(bool bCarryForward)
{
	// Save new setting
	m_bCarryForwardAnnotations = bCarryForward;
}
//-------------------------------------------------------------------------------------------------
bool RedactionUISettings::getApplyRedactionsAsAnnotations()
{
	// Return setting to caller
	return m_bApplyRedactionsAsAnnotations;
}
//-------------------------------------------------------------------------------------------------
void RedactionUISettings::setApplyRedactionsAsAnnotations(bool bApply)
{
	// Save new setting
	m_bApplyRedactionsAsAnnotations = bApply;
}
//-------------------------------------------------------------------------------------------------
void RedactionUISettings::setCollectFeedback(bool bCollectFeedback)
{
	m_bCollectFeedback = bCollectFeedback;
}
//-------------------------------------------------------------------------------------------------
bool RedactionUISettings::getCollectFeedback()
{
	return m_bCollectFeedback;
}
//-------------------------------------------------------------------------------------------------
void RedactionUISettings::setFeedbackCollectOption(
	UCLID_REDACTIONCUSTOMCOMPONENTSLib::EFeedbackCollectOption eFeedbackCollectOption)
{
	m_eFeedbackCollectOption = eFeedbackCollectOption;
}
//-------------------------------------------------------------------------------------------------
UCLID_REDACTIONCUSTOMCOMPONENTSLib::EFeedbackCollectOption 
	RedactionUISettings::getFeedbackCollectOption()
{
	return m_eFeedbackCollectOption;
}
//-------------------------------------------------------------------------------------------------
void RedactionUISettings::setFeedbackDataFolder(string strFeedbackDataFolder)
{
	m_strFeedbackDataFolder = strFeedbackDataFolder;
}
//-------------------------------------------------------------------------------------------------
string RedactionUISettings::getFeedbackDataFolder()
{
	return m_strFeedbackDataFolder;
}
//-------------------------------------------------------------------------------------------------
void RedactionUISettings::setCollectFeedbackImage(bool bCollectFeedbackImage)
{
	m_bCollectFeedbackImage = bCollectFeedbackImage;
}
//-------------------------------------------------------------------------------------------------
bool RedactionUISettings::getCollectFeedbackImage()
{
	return m_bCollectFeedbackImage;
}
//-------------------------------------------------------------------------------------------------
void RedactionUISettings::setFeedbackOriginalFilenames(bool bFeedbackOriginalFilenames)
{
	m_bFeedbackOriginalFilenames = bFeedbackOriginalFilenames;
}
//-------------------------------------------------------------------------------------------------
bool RedactionUISettings::getFeedbackOriginalFilenames()
{
	return m_bFeedbackOriginalFilenames;
}
//-------------------------------------------------------------------------------------------------
bool RedactionUISettings::getRequireRedactionTypes()
{
	return m_bRequireRedactionTypes;
}
//-------------------------------------------------------------------------------------------------
void RedactionUISettings::setRequireRedactionTypes(bool bRequireRedactionTypes)
{
	m_bRequireRedactionTypes = bRequireRedactionTypes;
}
//-------------------------------------------------------------------------------------------------
string RedactionUISettings::getInputDataFile()
{
	return m_strInputDataFile;
}
//-------------------------------------------------------------------------------------------------
void RedactionUISettings::setInputDataFile(const string& strInputDataFile)
{
	m_strInputDataFile = strInputDataFile;
}
//-------------------------------------------------------------------------------------------------
bool RedactionUISettings::getInputRedactedImage()
{
	return m_bInputRedactedImage;
}
//-------------------------------------------------------------------------------------------------
void RedactionUISettings::setInputRedactedImage(bool bInputRedactedImage)
{
	m_bInputRedactedImage = bInputRedactedImage;
}
//-------------------------------------------------------------------------------------------------
bool RedactionUISettings::getRequireExemptionCodes()
{
	return m_bRequireExemptionCodes;
}
//-------------------------------------------------------------------------------------------------
void RedactionUISettings::setRequireExemptionCodes(bool bRequireExemptionCodes)
{
	m_bRequireExemptionCodes = bRequireExemptionCodes;
}
//-------------------------------------------------------------------------------------------------
string RedactionUISettings::getRedactionText()
{
	return m_strRedactionText;
}
//-------------------------------------------------------------------------------------------------
void RedactionUISettings::setRedactionText(const string& strRedactionText)
{
	m_strRedactionText = strRedactionText;
}
//-------------------------------------------------------------------------------------------------
COLORREF RedactionUISettings::getBorderColor()
{
	return m_crBorderColor;
}
//-------------------------------------------------------------------------------------------------
void RedactionUISettings::setBorderColor(COLORREF crBorderColor)
{
	m_crBorderColor = crBorderColor;
}
//-------------------------------------------------------------------------------------------------
COLORREF RedactionUISettings::getFillColor()
{
	return m_crFillColor;
}
//-------------------------------------------------------------------------------------------------
void RedactionUISettings::setFillColor(COLORREF crFillColor)
{
	m_crFillColor = crFillColor;
}
//-------------------------------------------------------------------------------------------------
LOGFONT RedactionUISettings::getFont()
{
	return m_lgFont;
}
//-------------------------------------------------------------------------------------------------
void RedactionUISettings::setFont(const LOGFONT& lgFont)
{
	m_lgFont = lgFont;
}
//-------------------------------------------------------------------------------------------------
int RedactionUISettings::getFontSize()
{
	return m_iFontSize;
}
//-------------------------------------------------------------------------------------------------
void RedactionUISettings::setFontSize(int iFontSize)
{
	m_iFontSize = iFontSize;
}
//-------------------------------------------------------------------------------------------------
// Version 2: 
//    Added 4 booleans for the HC, MC, LC, and Clues checkboxes.
// Version 3: 
//    Added support for m_bCarryForwardAnnotations & m_bApplyRedactionsAsAnnotations
// Version 4:
//	  Added support for m_bEnableVOAOutput and m_strVOAOutputName
// Version 5:
//	  Added support for m_strDocCategory
// Version 6:
//	  Removed support for HC, MC, LC, and Clues checkboxes, m_strDocCategory
//    Added Message to user if older version is loaded
// Version 7:
//		Added support for m_ulMetaDataOutputVersion [p16 #2379]
// Version 8:
//    Removed support for output VOA filename
//    Added options for redaction accuracy feedback collection
// Version 9:
//		Added support for m_bRequireRedactionTypes [p16 #2833]
// Version 10:
//      Added support for redaction text, redaction color, and legislation guard
// Version 11:
//      Removed support for m_ulMetaDataOutputVersion [FlexIDSCore #3342]
void RedactionUISettings::Load ( IStream *pStream)
{
	// Reset settings
	clear();

	// Read the bytestream data from the IStream object
	unsigned long ulDataLength = 0;
	pStream->Read(&ulDataLength, sizeof(ulDataLength), NULL);
	ByteStream data(ulDataLength);
	pStream->Read(data.getData(), ulDataLength, NULL);
	ByteStreamManipulator dataReader(ByteStreamManipulator::kRead, data);

	// Read the individual data items from the bytestream
	unsigned long nDataVersion = 0;
	dataReader >> nDataVersion;

	// Check for newer version
	if (nDataVersion > gnCurrentVersion)
	{
		// Throw exception
		UCLIDException ue( "ELI13184", 
			"Unable to load newer Redaction Verification UI Settings" );
		ue.addDebugInfo( "Current Version", gnCurrentVersion );
		ue.addDebugInfo( "Version to Load", nDataVersion );
		throw ue;
	}
	dataReader >> m_bReviewAllPages;

	bool bTemp;
	if ( nDataVersion < 6 )
	{
		dataReader >> bTemp;
		dataReader >> bTemp;
	}

	// Version 2 loads the extra checkboxes
	if ( nDataVersion >= 2 && nDataVersion < 6 )
	{
		dataReader >> bTemp;
		dataReader >> bTemp;
		dataReader >> bTemp;
		dataReader >> bTemp;
	}

	if ( nDataVersion < 6 )
	{
		dataReader >> bTemp;
	}
	dataReader >> m_bAlwaysOutputImage;
	dataReader >> m_strOutputImageName;
	dataReader >> m_bAlwaysOutputMeta;
	dataReader >> m_strMetaOutputName;

	// Ignore the meta data output version
	if (nDataVersion >= 7 && nDataVersion <= 10)
	{
		unsigned long ulTemp;
		dataReader >> ulTemp;
	}
	
	// Load Annotation settings
	if ( nDataVersion >= 3 )
	{
		dataReader >> m_bCarryForwardAnnotations;
		dataReader >> m_bApplyRedactionsAsAnnotations;
	}

	// Load feedback collection settings
	if(nDataVersion >= 8)
	{
		dataReader >> m_bCollectFeedback;
		long lTemp;
		dataReader >> lTemp;
		m_eFeedbackCollectOption = 
			(UCLID_REDACTIONCUSTOMCOMPONENTSLib::EFeedbackCollectOption) lTemp;
		dataReader >> m_strFeedbackDataFolder;
		dataReader >> m_bCollectFeedbackImage;
		dataReader >> m_bFeedbackOriginalFilenames;
	}
	else if( nDataVersion >= 4 )
	{
		// Versions 4-7 optionally created a VOA file for each input file

		// get whether to create a VOA file, which is equivalent to collecting feedback
		dataReader >> m_bCollectFeedback;

		// get the VOA filename
		string strTempVOAOutputName;
		dataReader >> strTempVOAOutputName;

		// display a warning message if a VOA file was set to be created
		if(m_bCollectFeedback)
		{
			// set the feedback data folder to the directory of the VOA output filename
			m_strFeedbackDataFolder = "$DirOf(" + strTempVOAOutputName + ")";

			// set the other options to the behavior in version 4-7
			m_bCollectFeedbackImage = false;
			m_bFeedbackOriginalFilenames = true;
			m_eFeedbackCollectOption = (UCLID_REDACTIONCUSTOMCOMPONENTSLib::EFeedbackCollectOption)
				kFeedbackCollectAll;

			// display message
			MessageBox(NULL, "You have opened a FPS file saved with ID Shield v5.0 in which "
				"collection of redaction information was enabled. This feature has been improved "
				"in subsequent releases, and there are now more configurable settings for this "
				"feature.\r\n\r\nAll of your previous settings have been imported to best reflect "
				"your ID Shield v5.0 settings.\r\n\r\nIt is strongly recommended that you review "
				"the settings in the ID Shield \"Verify redactions\" task and ensure that they "
				"meet your requirements.", "Warning", MB_OK | MB_ICONINFORMATION);
		}
	}

	// read the require redaction types setting [p16 #2833]
	if (nDataVersion >= 9)
	{
		dataReader >> m_bRequireRedactionTypes;
	}

	// Load the doc category setting - Only for Version 5
	if( nDataVersion == 5 )
	{
		string strTemp;
		dataReader >> strTemp;
	}

	// Load and ignore doc type info
	if (nDataVersion < 6)
	{
		unsigned long ulSize;
		dataReader >> ulSize;
		for ( unsigned long u = 0; u < ulSize; u++ )
		{
			string strValue;
			dataReader >> strValue;
		}
	}

	if (nDataVersion >= 10)
	{
		// Legislation guard
		dataReader >> m_strInputDataFile;
		dataReader >> m_bInputRedactedImage;

		// Redaction text
		dataReader >> m_bRequireExemptionCodes;
		dataReader >> m_strRedactionText;
		dataReader >> m_crBorderColor;
		dataReader >> m_crFillColor;

		string strFontName;
		dataReader >> strFontName;
		lstrcpyn(m_lgFont.lfFaceName, strFontName.c_str(), LF_FACESIZE);
		
		bool bItalic;
		dataReader >> bItalic;
		m_lgFont.lfItalic = bItalic ? gucIS_ITALIC : 0;

		bool bBold;
		dataReader >> bBold;
		m_lgFont.lfWeight = bBold ? FW_BOLD : FW_NORMAL;

		long lTemp;
		dataReader >> lTemp;
		m_iFontSize = (int) lTemp;
	}

	// Provide message to user if older version ( before version 6 ) is being loaded
	if (nDataVersion < 6)
	{
		CString zPrompt;
		zPrompt.Format( "This FPS file includes an older version of the \"Verify redactions\" "
			"task.\r\n\r\nPlease refer to the Conditional Processing section of the \"How to "
			"verify redactions\" page in the Help file for information about the "
			"recommended configuration for verification." );
		MessageBox( NULL, zPrompt, "Warning", MB_OK | MB_ICONINFORMATION );
	}
}
//-------------------------------------------------------------------------------------------------
void RedactionUISettings::Save ( IStream *pStream)
{
	// Create a bytestream and stream this object's data into it
	ByteStream data;
	ByteStreamManipulator dataWriter( ByteStreamManipulator::kWrite, data );

	dataWriter << gnCurrentVersion;
	dataWriter << m_bReviewAllPages;

	// Save output image and metadata settings
	dataWriter << m_bAlwaysOutputImage;
	dataWriter << m_strOutputImageName;
	dataWriter << m_bAlwaysOutputMeta;
	dataWriter << m_strMetaOutputName;
	
	// Save Annotation settings
	dataWriter << m_bCarryForwardAnnotations;
	dataWriter << m_bApplyRedactionsAsAnnotations;

	// Save redaction accuracy settings
	dataWriter << m_bCollectFeedback;
	dataWriter << (long) m_eFeedbackCollectOption;
	dataWriter << m_strFeedbackDataFolder;
	dataWriter << m_bCollectFeedbackImage;
	dataWriter << m_bFeedbackOriginalFilenames;
	
	// Save require redaction types - added as per [p16 #2833]
	dataWriter << m_bRequireRedactionTypes; 

	// Save input data file and image options (legislation guard)
	dataWriter << m_strInputDataFile;
	dataWriter << m_bInputRedactedImage;

	// Save exemption code and text options
	dataWriter << m_bRequireExemptionCodes;
	dataWriter << m_strRedactionText;

	// Save redaction color options
	dataWriter << m_crBorderColor;
	dataWriter << m_crFillColor;

	// Save font options
	dataWriter << string(m_lgFont.lfFaceName);
	dataWriter << (m_lgFont.lfItalic == gucIS_ITALIC);
	dataWriter << asCppBool(m_lgFont.lfWeight >= FW_BOLD);
	dataWriter << (long) m_iFontSize;

	dataWriter.flushToByteStream();

	// Write the bytestream data into the IStream object
	unsigned long ulDataLength = data.getLength();
	pStream->Write( &ulDataLength, sizeof(ulDataLength), NULL );
	pStream->Write( data.getData(), ulDataLength, NULL );
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void RedactionUISettings::clear()
{
	// Carry Forward Annotations and Apply redactions 
	// as annotations will be reset to false
	m_bCarryForwardAnnotations = false;
	m_bApplyRedactionsAsAnnotations = false;

	// clear redaction accuracy feedback options
	m_bCollectFeedback = false;
	m_eFeedbackCollectOption = 
		(UCLID_REDACTIONCUSTOMCOMPONENTSLib::EFeedbackCollectOption) kFeedbackCollectAll;
	m_strFeedbackDataFolder = gstrDEFAULT_FEEDBACK_FOLDER;
	m_bCollectFeedbackImage = true;
	m_bFeedbackOriginalFilenames = true;

	// default require redaction types to true [p16 #2833]
	m_bRequireRedactionTypes = true;

	// Input data file and image options
	m_strInputDataFile = gstrDEFAULT_TARGET_FILENAME;
	m_bInputRedactedImage = false;

	// Exemption code and text options
	m_bRequireExemptionCodes = false;
	m_strRedactionText = gstrDEFAULT_REDACTION_TEXT;

	// Redaction color options
	m_crBorderColor = 0;
	m_crFillColor = 0;

	// Font options
	memset(&m_lgFont, 0, sizeof(LOGFONT));
	lstrcpyn(m_lgFont.lfFaceName, "Times New Roman", LF_FACESIZE);
	m_lgFont.lfWeight = FW_NORMAL;
	m_iFontSize = 8;
}
//-------------------------------------------------------------------------------------------------
