// RedactFromXML.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "RedactFromXML.h"

#include <UCLIDException.h>
#include <COMUtils.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <INIFilePersistenceMgr.h>
#include <RedactionCCConstants.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// Message map
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CRedactFromXMLApp, CWinApp)
	//{{AFX_MSG_MAP(CRedactFromXMLApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG
	ON_COMMAND(ID_HELP, CWinApp::OnHelp)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CRedactFromXMLApp
//-------------------------------------------------------------------------------------------------
CRedactFromXMLApp::CRedactFromXMLApp()
	: m_strInputFile(""),
	  m_strXMLFile(""),
	  m_strOutputFile(""),
	  m_bRedactAsAnnotation(false),
	  m_bBurnInRedactions(false),
	  m_bRetainAnnotations(false),
	  m_bDiscardAnnotations(false),
	  m_bIsError(true) // assume error until successfully completed
{
}
//-------------------------------------------------------------------------------------------------

// The one and only CRedactFromXMLApp object
CRedactFromXMLApp theApp;

//-------------------------------------------------------------------------------------------------
BOOL CRedactFromXMLApp::InitInstance()
{
	try
	{		
		// Get the command line arguments
		if ( !getArguments(__argc, __argv) )
		{
			return FALSE;
		}

		// License this EXE
		LicenseManagement::sGetInstance().loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);
		validateLicense();
		if (m_bRedactAsAnnotation && !LicenseManagement::sGetInstance().isAnnotationLicensed())
		{
			// create and throw exception
			throw UCLIDException("ELI18675", "Annotation support is not licensed.");
		}

		// scope for COM objects
		CoInitializeEx(NULL, COINIT_APARTMENTTHREADED);
		{
			// Get a vector of raster zones from the IDShield metadata xml			
			vector<PageRasterZone> vecZones = getRasterZonesFromXML(m_strXMLFile);

			// Redact the area if any zones were found or always outputting a file
			// Calling fillImageArea will handle removing existing annotations even
			// if the output file has no redactions [FlexIDSCore #3584 & #3585]
			if (vecZones.size() > 0 || m_bAlwaysOutput)
			{
				// Save redactions
				fillImageArea(m_strInputFile, m_strOutputFile, vecZones, m_bRetainAnnotations, 
					m_bRedactAsAnnotation);
			}
		} // end scope for COM objects

		CoUninitialize();

		// successful completion
		m_bIsError = false;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI18674");

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
int CRedactFromXMLApp::ExitInstance() 
{
	return (m_bIsError ? EXIT_FAILURE : EXIT_SUCCESS);
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
bool CRedactFromXMLApp::displayUsage(const string& strFileName)
{
	// create the usage message
	string strUsage = "Creates a redacted image from an image file and an IDShield metadata xml file.\n\n";
	strUsage += strFileName;
	strUsage += " <input_image> <metadata_xml> <output_image> [/a|/b] [/O] [/r|/d]\n\n";
	strUsage += " input_image\tSpecifies the original image file from which to create the redacted image.\n";
	strUsage += " metadata_xml\tSpecifies IDShield metadata xml file associated with the input image.\n";
	strUsage += " output_image\tSpecifies the path to the redacted image.\n";
	strUsage += " /a\t\tApply redactions as annotations. Cannot be used with /b.\n";
	strUsage += " /b\t\tBurn redaction into the image. Cannot be used with /a.\n";
	strUsage += " /O\t\tCreate output image for this file, even if no redactions were found.\n";
	strUsage += " /r\t\tRetain existing annotations in input file. Cannot be used with /d.\n";
	strUsage += " /d\t\tDiscard existing annotations in input file. Cannot be used with /r.\n";

	// display the message
	AfxMessageBox(strUsage.c_str(), MB_ICONWARNING);

	return false;
}
//-------------------------------------------------------------------------------------------------
bool CRedactFromXMLApp::getArguments(const int argc, char* argv[])
{
	try
	{
		// check for a valid number of arguments
		if (argc < 4 || argc > 8)
		{
			return displayUsage(argv[0]);
		}

		// get the file names
		m_strInputFile = argv[1];
		validateFileOrFolderExistence(m_strInputFile);
		m_strXMLFile = argv[2];
		validateFileOrFolderExistence(m_strXMLFile);
		m_strOutputFile = argv[3];

		// ensure output directory exists
		string strOutputDir(getDirectoryFromFullPath(m_strOutputFile));
		if (!directoryExists(strOutputDir))
		{
			createDirectory(strOutputDir);
		}

		// check for optional flags
		m_bRedactAsAnnotation = false;
		m_bRetainAnnotations = false;
		m_bAlwaysOutput = false;
		for(int i=4; i<argc; i++)
		{
			string strArg(argv[i]);

			if (strArg == "/a")
			{
				m_bRedactAsAnnotation = true;
			}
			else if (strArg == "/b")
			{
				m_bBurnInRedactions = true;
			}
			else if (strArg == "/O")
			{
				m_bAlwaysOutput = true;
			}
			else if (strArg == "/r")
			{
				m_bRetainAnnotations = true;
			}
			else if (strArg == "/d")
			{
				m_bDiscardAnnotations = true;
			}
			else
			{
				return displayUsage(argv[0]);
			}
		}

		if (m_bRedactAsAnnotation && m_bBurnInRedactions)
		{
			throw UCLIDException("ELI29390", "Cannot redact as annotation and burn in redaction.");
		}
		if (m_bRetainAnnotations && m_bDiscardAnnotations)
		{
			throw UCLIDException("ELI29391", "Cannot retain and discard annotations.");
		}

		return true;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29852");
}
//-------------------------------------------------------------------------------------------------
long CRedactFromXMLApp::getAttributeAsLong(MSXML::IXMLDOMNamedNodeMapPtr ipAttributes, const char* pszAttributeName)
{
	try
	{
		return asLong( getAttributeAsString(ipAttributes, pszAttributeName) );
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29851");
}
//-------------------------------------------------------------------------------------------------
string CRedactFromXMLApp::getAttributeAsString(MSXML::IXMLDOMNamedNodeMapPtr ipAttributes, 
										const char* pszAttributeName)
{
	try
	{
		MSXML::IXMLDOMNodePtr ipAttribute = ipAttributes->getNamedItem(pszAttributeName);
		ASSERT_RESOURCE_ALLOCATION("ELI18672", ipAttribute != __nullptr);
		return asString(ipAttribute->text);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29850");
}
//-------------------------------------------------------------------------------------------------
COLORREF CRedactFromXMLApp::getColorFromNode(MSXML::IXMLDOMNodePtr ipNode)
{
	try
	{
		// Return either black or white depending on the node's value
		return asString(ipNode->text) == "White" ? RGB(255,255,255) : RGB(0,0,0);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29849");
}
//-------------------------------------------------------------------------------------------------
string CRedactFromXMLApp::getRedactionTextFromRedactionNode(MSXML::IXMLDOMNodePtr ipRedaction, 
															const string& strTextFormat)
{
	try
	{
		// Get the children of the redaction node
		MSXML::IXMLDOMNodeListPtr ipChildren = ipRedaction->childNodes;
		ASSERT_RESOURCE_ALLOCATION("ELI25123", ipChildren != __nullptr);

		// Get the exemption code node under this redaction node
		MSXML::IXMLDOMNodePtr ipExemptions = getFirstNodeNamed(ipChildren, "ExemptionCode");
		string strExemptions;
		if (ipExemptions != __nullptr)
		{
			// Get the exemption codes for this redaction
			MSXML::IXMLDOMNamedNodeMapPtr ipExemptionAttributes = ipExemptions->attributes;
			ASSERT_RESOURCE_ALLOCATION("ELI25119", ipExemptionAttributes != __nullptr);
			strExemptions = getAttributeAsString(ipExemptionAttributes, "Code");
		}

		// Get the type of this redaction
		MSXML::IXMLDOMNamedNodeMapPtr ipRedactionAttributes = ipRedaction->attributes;
		ASSERT_RESOURCE_ALLOCATION("ELI25121", ipRedactionAttributes != __nullptr);
		string strType = getAttributeAsString(ipRedactionAttributes, "Type");

		// Replace the tags
		string strResult = strTextFormat;
		replaceVariable(strResult, gstrEXEMPTION_CODES_TAG, strExemptions);
		replaceVariable(strResult, gstrFIELD_TYPE_TAG, strType);

		// Return the expanded redaction text
		return strResult;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29848");
}
//-------------------------------------------------------------------------------------------------
LOGFONT CRedactFromXMLApp::getFontFromNode(MSXML::IXMLDOMNodePtr ipNode, int& riPointSize)
{
	try
	{
		// Prepare the default LOGFONT
		LOGFONT font = {0};
		font.lfItalic = FALSE;
		font.lfWeight = FW_NORMAL;

		// Get the font as a string
		string& strFont = asString(ipNode->text);

		// Check for a semicolon (this indicates a font style is specified)
		size_t uiDelimiter = strFont.find_last_of(';');
		if (uiDelimiter == string::npos)
		{
			// There is no style information, set the index to the end of the string
			uiDelimiter = strFont.length();
		}
		else
		{
			// Check if the font is bold
			size_t uiStyle = strFont.find("Bold", uiDelimiter + 1);
			if (uiStyle != string::npos)
			{
				font.lfWeight = FW_BOLD;
			}

			// Check if the font is italic
			uiStyle = strFont.find("Italic", uiDelimiter + 1);
			if (uiStyle != string::npos)
			{
				font.lfItalic = gucIS_ITALIC;
			}
		}

		// Search backwards for the font size
		size_t uiEndPointSize = strFont.rfind("pt", uiDelimiter + 1);
		if (uiEndPointSize == string::npos)
		{
			throw UCLIDException("ELI25116", "Unable to determine font size");
		}
		size_t uiStartPointSize = strFont.find_last_not_of(" 0123456789", uiEndPointSize - 1);
		if (uiStartPointSize == string::npos)
		{
			throw UCLIDException("ELI25117", "Unable to determine font size");
		}
		uiStartPointSize++;

		// Get the font size
		string strFontSize = strFont.substr(uiStartPointSize, uiEndPointSize - uiStartPointSize);
		riPointSize = asLong(strFontSize);

		// Get the font name
		string strFontName = strFont.substr(0, uiStartPointSize);
		lstrcpyn(font.lfFaceName, strFontName.c_str(), LF_FACESIZE);

		// Return the font
		return font;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29847");
}
//-------------------------------------------------------------------------------------------------
MSXML::IXMLDOMNodePtr CRedactFromXMLApp::getFirstNodeNamed(MSXML::IXMLDOMNodeListPtr ipNodes, 
		const string& strName)
{
	try
	{
		// Iterate through each node
		long lCount = ipNodes->length;
		for (long i = 0; i < lCount; i++)
		{
			// Get the ith node
			MSXML::IXMLDOMNodePtr ipNode = ipNodes->item[i];
			ASSERT_RESOURCE_ALLOCATION("ELI25113", ipNode != __nullptr);

			// Return this node if it is a match
			if (asString(ipNode->nodeName) == strName)
			{
				return ipNode;
			}
		}

		// It was not found, return NULL
		return NULL;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29846");
}
//-------------------------------------------------------------------------------------------------
PageRasterZone CRedactFromXMLApp::getRasterZonePrototypeFromNode(MSXML::IXMLDOMElementPtr ipNode, 
																 string &rstrTextFormat)
{
	try
	{
		// Create the default raster zone to return
		PageRasterZone prototype;
		rstrTextFormat = "";

		MSXML::IXMLDOMNodeListPtr ipSettings = __nullptr;
		if (ipNode != __nullptr)
		{
			// Get the child nodes of the session
			MSXML::IXMLDOMNodeListPtr ipChildren = ipNode->childNodes;
			ASSERT_RESOURCE_ALLOCATION("ELI25111", ipChildren != __nullptr);

			// Get the children of the output options node
			MSXML::IXMLDOMNodePtr ipOutputNode = getFirstNodeNamed(ipChildren, "OutputOptions");
			if (ipOutputNode != __nullptr)
			{
				ipChildren = ipOutputNode->childNodes;
				ASSERT_RESOURCE_ALLOCATION("ELI29902", ipChildren != __nullptr);
			}

			// Get the redaction appearance settings
			MSXML::IXMLDOMNodePtr ipSettingsNode = 
				getFirstNodeNamed(ipChildren, "RedactionTextAndColorSettings");
			if (ipSettingsNode != __nullptr)
			{
				// Get the individual settings
				ipSettings = ipSettingsNode->childNodes;
			}
		}

		if (ipSettings == __nullptr)
		{
			// No settings were found. Return default prototype.
			return prototype;
		}

		// Iterate through each setting
		long lCount = ipSettings->length;
		for (long i = 0; i < lCount; i++)
		{
			MSXML::IXMLDOMNodePtr ipSetting = ipSettings->item[i];
			ASSERT_RESOURCE_ALLOCATION("ELI25114", ipSetting != __nullptr);

			string strNodeName = asString(ipSetting->nodeName);
			if (strNodeName == "TextFormat")
			{
				rstrTextFormat = asString(ipSetting->text);
			}
			else if (strNodeName == "FillColor")
			{
				prototype.m_crFillColor = getColorFromNode(ipSetting);
				prototype.m_crTextColor = invertColor(prototype.m_crFillColor);
			}
			else if (strNodeName == "BorderColor")
			{
				prototype.m_crBorderColor = getColorFromNode(ipSetting);
			}
			else if (strNodeName == "Font")
			{
				int iPointSize = 0;
				prototype.m_font = getFontFromNode(ipSetting, iPointSize);
				prototype.m_iPointSize = iPointSize;
			}
		}

		// Return the prototype
		return prototype;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29845");
}
//-------------------------------------------------------------------------------------------------
vector<PageRasterZone> CRedactFromXMLApp::getRasterZonesFromNode(MSXML::IXMLDOMElementPtr ipNode, 
	PageRasterZone& prototype, const string& strTextFormat)
{
	try
	{
		// Create a vector to hold the raster zones
		vector<PageRasterZone> vecZones;

		// If there are no raster zones, return an empty vector
		if (ipNode == __nullptr)
		{
			return vecZones;
		}

		// Get the redactions
		MSXML::IXMLDOMNodeListPtr ipRedactionList = ipNode->getElementsByTagName("Redaction");
		ASSERT_RESOURCE_ALLOCATION("ELI18644", ipRedactionList != __nullptr);

		// Iterate through each redaction
		long lNumRedactions = ipRedactionList->length;
		for (long i = 0; i < lNumRedactions; i++)
		{
			// Get the ith redaction
			MSXML::IXMLDOMNodePtr ipRedaction(ipRedactionList->item[i]);
			ASSERT_RESOURCE_ALLOCATION("ELI18646", ipRedaction != __nullptr);

			// Get the attributes of this redaction
			MSXML::IXMLDOMNamedNodeMapPtr ipRedactionAttributes(ipRedaction->attributes);
			ASSERT_RESOURCE_ALLOCATION("ELI18647", ipRedactionAttributes != __nullptr);

			// Check if this redaction was correct
			if (isRedactionEnabled(ipRedactionAttributes))
			{
				// Get exemption codes for this redaction
				prototype.m_strText = getRedactionTextFromRedactionNode(ipRedaction, strTextFormat);

				// Get the lines associated with this redaction
				MSXML::IXMLDOMNodeListPtr ipLines = ipRedaction->childNodes;
				ASSERT_RESOURCE_ALLOCATION("ELI18649", ipLines != __nullptr);

				// Iterate through each line of this redaction
				long lNumLines = ipLines->length;
				for (long j = 0; j < lNumLines; j++)
				{
					// Get the jth line
					MSXML::IXMLDOMNodePtr ipLine = ipLines->item[j];
					ASSERT_RESOURCE_ALLOCATION("ELI18650", ipLine != __nullptr);

					string strNodeName = asString(ipLine->nodeName);

					// Version 4+ uses zone instead of line
					if (strNodeName == "Zone")
					{
						PageRasterZone zone = getRasterZoneFromXML(ipLine, prototype);
						vecZones.push_back(zone);
						continue;
					}

					// Skip this node if it is not a line
					if (strNodeName != "Line")
					{
						continue;
					}

					// Get the zones associated with this line
					MSXML::IXMLDOMNodeListPtr ipXMLZones = ipLine->childNodes;
					ASSERT_RESOURCE_ALLOCATION("ELI18651", ipXMLZones != __nullptr);

					// Iterate through each zone of this line
					long lNumXMLZones = ipXMLZones->length;
					for (long k = 0; k < lNumXMLZones; k++)
					{
						// Get the kth zone
						MSXML::IXMLDOMNodePtr ipXMLZone = ipXMLZones->item[k];
						ASSERT_RESOURCE_ALLOCATION("ELI18652", ipXMLZone != __nullptr);

						// Add this zone to the list
						if (asString(ipXMLZone->nodeName) == "Zone")
						{
							PageRasterZone zone = getRasterZoneFromXML(ipXMLZone, prototype);
							vecZones.push_back(zone);
						}
					} // End iterate through zones
				} // End iterate through lines
			} // End if found output redaction
		} // End iterate through redactions

		return vecZones;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29844");
}
//-------------------------------------------------------------------------------------------------
PageRasterZone CRedactFromXMLApp::getRasterZoneFromXML(MSXML::IXMLDOMNodePtr ipZone, 
													   PageRasterZone& prototype)
{
	try
	{
		// Get the attributes of this zone
		MSXML::IXMLDOMNamedNodeMapPtr ipZoneAttributes(ipZone->attributes);
		ASSERT_RESOURCE_ALLOCATION("ELI18653", ipZoneAttributes != __nullptr);

		// Store the zone
		prototype.m_nStartX = getAttributeAsLong(ipZoneAttributes, "StartX");
		prototype.m_nStartY = getAttributeAsLong(ipZoneAttributes, "StartY");
		prototype.m_nEndX = getAttributeAsLong(ipZoneAttributes, "EndX");
		prototype.m_nEndY = getAttributeAsLong(ipZoneAttributes, "EndY");
		prototype.m_nHeight = getAttributeAsLong(ipZoneAttributes, "Height");
		prototype.m_nPage = getAttributeAsLong(ipZoneAttributes, "PageNumber");

		return prototype;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29843");
}
//-------------------------------------------------------------------------------------------------
vector<PageRasterZone> CRedactFromXMLApp::getRasterZonesFromXML(const string& strXMLFile)
{
	try
	{
		// create an xml document object
		MSXML::IXMLDOMDocumentPtr ipXMLDocument(CLSID_DOMDocument);
		ASSERT_RESOURCE_ALLOCATION("ELI18640", ipXMLDocument != __nullptr);

		// ensure asynchronous processing is disabled
		ipXMLDocument->async = VARIANT_FALSE;

		// load the XML File
		if (VARIANT_FALSE == ipXMLDocument->load( _variant_t(strXMLFile.c_str()) ))
		{
			UCLIDException ue("ELI18641", "Unable to load XML file.");
			ue.addDebugInfo("XML filename", strXMLFile);
			throw ue;
		}

		// get the root element
		MSXML::IXMLDOMElementPtr ipRoot = ipXMLDocument->documentElement; 
		ASSERT_RESOURCE_ALLOCATION("ELI18642", ipRoot != __nullptr);

		// get the root node's name
		string strRootNodeName = asString(ipRoot->nodeName);

		// get the version number of the ID Shield meta data
		long lVersionNumber = 1;
		if (strRootNodeName == "IDShieldMetadata" || strRootNodeName == "IDShieldMetaData" || 
			strRootNodeName == "IDShieldMetaDataEx")
		{
			// get the attributes of the root node
			MSXML::IXMLDOMNamedNodeMapPtr ipRootAttributes = ipRoot->attributes;
			ASSERT_RESOURCE_ALLOCATION("ELI19093", ipRootAttributes != __nullptr);

			// get the version attribute of this node
			MSXML::IXMLDOMNodePtr ipAttribute = ipRootAttributes->getNamedItem("Version");

			// set the version number if it was found
			// NOTE: the first version had no version attribute, only the IDShieldMetaData tag
			if (ipAttribute != __nullptr)
			{
				lVersionNumber = asLong( asString(ipAttribute->text) );
			}
		}
		else
		{
			// throw an exception
			UCLIDException ue("ELI18643", "XML is not IDShield metadata.");
			ue.addDebugInfo("XML filename", strXMLFile);
			throw ue;
		}

		// Ensure this metadata version is supported
		if (lVersionNumber > giCURRENT_META_DATA_VERSION)
		{
			UCLIDException ue("ELI19094", "Unsupported IDShield metadata version.");
			ue.addDebugInfo("XML filename", strXMLFile);
			ue.addDebugInfo("Version number", lVersionNumber);
			ue.addDebugInfo("Maximum supported version", giCURRENT_META_DATA_VERSION);
			throw ue;
		}

		// Previous versions didn't support multiple session, the root contains one session
		if (lVersionNumber != 3)
		{
			// Version 4+ gets the zone prototype from the last redaction session
			MSXML::IXMLDOMElementPtr ipSession = __nullptr;
			if (lVersionNumber < 4)
			{
				ipSession = ipRoot;
			}
			else
			{
				ipSession = getLastRedactionSessionNode(ipRoot);

				// Only read the annotations settings if there is a last redaction session
				// [LRCAU #5760]
				if (ipSession != __nullptr)
				{
					// Read the annotation settings from the last redaction session 
					// if not overridden by command-line parameters.
					updateAnnotationSettings(ipSession);
				}
			}

			// Get the zone prototype
			string strTextFormat;
			PageRasterZone prototype = getRasterZonePrototypeFromNode(ipSession, strTextFormat);

			// Version 4+ gets the raster zones to redact from the current revisions node
			MSXML::IXMLDOMElementPtr ipRevisions = 
				lVersionNumber < 4 ? ipRoot : getFirstNodeNamed(ipRoot->childNodes, "CurrentRevisions");

			return getRasterZonesFromNode(ipRevisions, prototype, strTextFormat);
		}

		// Get the session nodes
		MSXML::IXMLDOMNodeListPtr ipSessions = ipRoot->childNodes;
		ASSERT_RESOURCE_ALLOCATION("ELI25124", ipSessions != __nullptr);

		// Iterate through the session node
		vector<PageRasterZone> vecZones;
		long lCount = ipSessions->length;
		for (long i = 0; i < lCount; i++)
		{
			// Get the ith session
			MSXML::IXMLDOMNodePtr ipSession = ipSessions->item[i];
			ASSERT_RESOURCE_ALLOCATION("ELI25125", ipSession != __nullptr);

			// Get the zone prototype and text format from this sessions redaction appearance settings
			string strTextFormat;
			PageRasterZone prototype = getRasterZonePrototypeFromNode(ipRoot, strTextFormat);

			// Get the raster zones of this session
			vector<PageRasterZone>& vecCurrentZones = 
				getRasterZonesFromNode(ipSession, prototype, strTextFormat);

			// Append this session's raster zones to the master list
			vecZones.insert(vecZones.end(), vecCurrentZones.begin(), vecCurrentZones.end());
		}

		// Return the result
		return vecZones;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29840");
}
//-------------------------------------------------------------------------------------------------
MSXML::IXMLDOMElementPtr CRedactFromXMLApp::getLastRedactionSessionNode(
	MSXML::IXMLDOMElementPtr ipRoot)
{
	try
	{
		MSXML::IXMLDOMNodeListPtr ipSessions = ipRoot->getElementsByTagName("RedactedFileOutputSession");
		ASSERT_RESOURCE_ALLOCATION("ELI29384", ipSessions != __nullptr);

		// Iterate over each automated redaction session
		MSXML::IXMLDOMElementPtr ipLastSession = __nullptr;
		long lLastSessionId = 0;
		long lCount = ipSessions->length;
		for	(long i = 0; i < lCount; i++)
		{
			MSXML::IXMLDOMNodePtr ipSession = ipSessions->item[i];
			ASSERT_RESOURCE_ALLOCATION("ELI29385", ipSession != __nullptr);

			// Get the session's attributes
			MSXML::IXMLDOMNamedNodeMapPtr ipAttributes(ipSession->attributes);
			ASSERT_RESOURCE_ALLOCATION("ELI29386", ipAttributes != __nullptr);

			// Get the session id
			MSXML::IXMLDOMNodePtr ipSessionId = ipAttributes->getNamedItem("ID");
			ASSERT_RESOURCE_ALLOCATION("ELI29387", ipSessionId != __nullptr);
			long lSessionId = asLong( asString(ipSessionId->text) );

			// The last session is the session with the largest session id
			if (lSessionId > lLastSessionId)
			{
				ipLastSession = ipSession;
				lLastSessionId = lSessionId;
			}
		}

		return ipLastSession;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29841");
}
//-------------------------------------------------------------------------------------------------
void CRedactFromXMLApp::updateAnnotationSettings(MSXML::IXMLDOMElementPtr ipSession)
{
	try
	{
		// Get the children of the session node
		MSXML::IXMLDOMNodeListPtr ipSessionChildren = ipSession->childNodes;
		ASSERT_RESOURCE_ALLOCATION("ELI29392", ipSessionChildren != __nullptr);

		// Get the output options
		MSXML::IXMLDOMNodePtr ipOutputOptions = getFirstNodeNamed(ipSessionChildren, "OutputOptions");
		ASSERT_RESOURCE_ALLOCATION("ELI29393", ipOutputOptions != __nullptr);

		// Get the children of the output options
		MSXML::IXMLDOMNodeListPtr ipOptionsChildren = ipOutputOptions->childNodes;
		ASSERT_RESOURCE_ALLOCATION("ELI29394", ipOptionsChildren != __nullptr);

		// Redact as annotations or burn in redactions?
		if (!m_bRedactAsAnnotation && !m_bBurnInRedactions)
		{
			MSXML::IXMLDOMNodePtr ipRedactAsAnnotation = 
				getFirstNodeNamed(ipOptionsChildren, "ApplyRedactionsAsAnnotations");
			ASSERT_RESOURCE_ALLOCATION("ELI29388", ipRedactAsAnnotation != __nullptr);

			if (asString(ipRedactAsAnnotation->text) == "Yes")
			{
				m_bRedactAsAnnotation = true;
			}
			else
			{
				m_bBurnInRedactions = true;
			}
		}

		// Retain existing annotations or discard existing annotations?
		if (!m_bRetainAnnotations && !m_bDiscardAnnotations)
		{
			MSXML::IXMLDOMNodePtr ipRetainAnnotations = 
				getFirstNodeNamed(ipOptionsChildren, "RetainExistingAnnotations");
			ASSERT_RESOURCE_ALLOCATION("ELI29389", ipRetainAnnotations != __nullptr);

			if (asString(ipRetainAnnotations->text) == "Yes")
			{
				m_bRetainAnnotations = true;
			}
			else
			{
				m_bDiscardAnnotations = true;
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29842");
}
//-------------------------------------------------------------------------------------------------
bool CRedactFromXMLApp::isRedactionEnabled(MSXML::IXMLDOMNamedNodeMapPtr ipAttributes)
{
	MSXML::IXMLDOMNodePtr ipEnabled = ipAttributes->getNamedItem("Enabled");
	if (ipEnabled == __nullptr)
	{
		ipEnabled = ipAttributes->getNamedItem("Output");
		ASSERT_RESOURCE_ALLOCATION("ELI29346", ipEnabled != __nullptr);
	}

	return asString(ipEnabled->text) == "1";
}
//-------------------------------------------------------------------------------------------------
void CRedactFromXMLApp::validateLicense()
{
	VALIDATE_LICENSE(gnIDSHIELD_CORE_OBJECTS, "ELI18673", "RedactFromXML");
}
//-------------------------------------------------------------------------------------------------
