#include "stdafx.h"
#include "resource.h"
#include "DataAreaDlg.h"
#include "RedactionCCUtils.h"
#include "RedactionCCConstants.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <ComUtils.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Private methods related to writing metadata file in XML
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::addXMLDataNodes(MSXML::IXMLDOMDocumentPtr ipXMLDOMDocument, 
								   MSXML::IXMLDOMNodePtr ipMain)
{
	// Check arguments passed in
	ASSERT_ARGUMENT("ELI11414", ipXMLDOMDocument != NULL);
	ASSERT_ARGUMENT("ELI11415", ipMain != NULL);

	// Create a node for the Redactions
	MSXML::IXMLDOMNodePtr ipDataNode = ipXMLDOMDocument->createElement("Redactions");
	ASSERT_RESOURCE_ALLOCATION("ELI11453", ipDataNode != NULL);

	// Iterate through each of the Data Items and handle them
	long nNumItems = m_vecDataItems.size();
	for (int i = 0; i < nNumItems; i++)
	{
		// Create the Redaction node
		MSXML::IXMLDOMNodePtr ipRedactionNode = getRedactionNode( ipXMLDOMDocument, 
			m_vecDataItems[i]);

		// Append the Redaction node to the parent node
		ipDataNode->appendChild( ipRedactionNode );
	}

	// Append the Redactions node to the main node
	ipMain->appendChild( ipDataNode );
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::addXMLDocumentNode(MSXML::IXMLDOMDocumentPtr ipXMLDOMDocument, 
									  MSXML::IXMLDOMNodePtr ipMain, const DocumentData& document)
{
	// Check arguments passed in
	ASSERT_ARGUMENT("ELI11416", ipXMLDOMDocument != NULL);
	ASSERT_ARGUMENT("ELI11417", ipMain != NULL);

	// Create an element node for the Document Info
	MSXML::IXMLDOMNodePtr ipDocumentNode = ipXMLDOMDocument->createElement("DocumentInfo");
	ASSERT_RESOURCE_ALLOCATION("ELI11418", ipDocumentNode != NULL);

	// Create the InputFile node
	MSXML::IXMLDOMNodePtr ipInputNode = 
		getTextNode(ipXMLDOMDocument, "InputFile", document.m_strOriginal);
	ASSERT_RESOURCE_ALLOCATION("ELI11489", ipInputNode != NULL);

	// Create the OutputFile node
	MSXML::IXMLDOMNodePtr ipOutputNode = 
		getTextNode(ipXMLDOMDocument, "OutputFile", document.m_strOutput);
	ASSERT_RESOURCE_ALLOCATION("ELI11491", ipOutputNode != NULL);

	// Create the DocType node
	MSXML::IXMLDOMNodePtr ipDocTypeNode = 
		getTextNode(ipXMLDOMDocument, "DocType", document.m_strDocType);
	ASSERT_RESOURCE_ALLOCATION("ELI11789", ipDocTypeNode != NULL);

	// Append the InputFile, OutputFile, DocType nodes to the Document Info node
	ipDocumentNode->appendChild( ipInputNode );
	ipDocumentNode->appendChild( ipOutputNode );
	ipDocumentNode->appendChild( ipDocTypeNode );

	// Append the Document Info node to the main node
	ipMain->appendChild( ipDocumentNode );
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::addXMLRedactionAppearanceNode(MSXML::IXMLDOMDocumentPtr ipXMLDOMDocument,
		MSXML::IXMLDOMNodePtr ipMain)
{
	// Create an element node for redaction appearance settings
	MSXML::IXMLDOMNodePtr ipSettings = ipXMLDOMDocument->createElement("RedactionTextAndColorSettings");
	ASSERT_RESOURCE_ALLOCATION("ELI25095", ipSettings != NULL);

	// Create the TextFormat node
	MSXML::IXMLDOMNodePtr ipTextFormat = 
		getTextNode(ipXMLDOMDocument, "TextFormat", m_UISettings.getRedactionText());
	ASSERT_RESOURCE_ALLOCATION("ELI25098", ipTextFormat != NULL);

	// Create the FillColor node
	bool bIsBlack = m_UISettings.getFillColor() == 0;
	MSXML::IXMLDOMNodePtr ipFillColor = 
		getTextNode(ipXMLDOMDocument, "FillColor", bIsBlack ? "Black" : "White");
	ASSERT_RESOURCE_ALLOCATION("ELI25099", ipFillColor != NULL);

	// Create the BorderColor node
	bIsBlack = m_UISettings.getBorderColor() == 0;
	MSXML::IXMLDOMNodePtr ipBorderColor = 
		getTextNode(ipXMLDOMDocument, "BorderColor", bIsBlack ? "Black" : "White");
	ASSERT_RESOURCE_ALLOCATION("ELI25100", ipBorderColor != NULL);

	// Create Font node
	string strFont = getFontDescription(m_UISettings.getFont(), m_UISettings.getFontSize());
	MSXML::IXMLDOMNodePtr ipFont = getTextNode(ipXMLDOMDocument, "Font", strFont);
	ASSERT_RESOURCE_ALLOCATION("ELI25103", ipFont);

	// Append the nodes to the redaction appearance node
	ipSettings->appendChild(ipTextFormat);
	ipSettings->appendChild(ipFillColor);
	ipSettings->appendChild(ipBorderColor);
	ipSettings->appendChild(ipFont);

	// Append the redaction appearance node to the main node
	ipMain->appendChild(ipSettings);
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::addXMLVerificationNode(MSXML::IXMLDOMDocumentPtr ipXMLDOMDocument, 
	MSXML::IXMLDOMNodePtr ipMain, const DocumentData& document)
{
	// Check arguments passed in
	ASSERT_ARGUMENT("ELI11419", ipXMLDOMDocument != NULL);
	ASSERT_ARGUMENT("ELI11420", ipMain != NULL);

	// Create an element node for Verification Info
	MSXML::IXMLDOMNodePtr ipVerificationNode = ipXMLDOMDocument->createElement("VerificationInfo");
	ASSERT_RESOURCE_ALLOCATION("ELI11452", ipVerificationNode != NULL);

	///////
	// USER
	///////
	// Create an element node for the User
	MSXML::IXMLDOMNodePtr ipUserNode = ipXMLDOMDocument->createElement("User");
	ASSERT_RESOURCE_ALLOCATION("ELI11421", ipUserNode != NULL);

	// Get user login name, full name and computer name
	string strLogin = getCurrentUserName();
	string strComputer = getComputerName();

	// Create the LoginID node
	MSXML::IXMLDOMNodePtr ipIDNode = getTextNode(ipXMLDOMDocument, "LoginID", strLogin);
	ASSERT_RESOURCE_ALLOCATION("ELI11475", ipIDNode != NULL);

	// Create the Computer node
	MSXML::IXMLDOMNodePtr ipComputerNode = getTextNode(ipXMLDOMDocument, "Computer", strComputer);
	ASSERT_RESOURCE_ALLOCATION("ELI11477", ipComputerNode != NULL);

	// Append the FullName, LoginID and Computer nodes to the User node
	ipUserNode->appendChild( ipIDNode );
	ipUserNode->appendChild( ipComputerNode );

	// Append the User node to the parent node
	ipVerificationNode->appendChild( ipUserNode );

	////////////////////////////////////////////
	// VERIFIED, DATE, TIMESTARTED, TOTALSECONDS
	////////////////////////////////////////////
	// Create an element node for Verified
	MSXML::IXMLDOMNodePtr ipVerifiedNode = 
		getTextNode(ipXMLDOMDocument, "VerifiedByUser", m_bVerified ? "Yes" : "No" );
	ASSERT_RESOURCE_ALLOCATION("ELI11828", ipVerifiedNode != NULL);

	// create a node for the name of the voa file
	MSXML::IXMLDOMNodePtr ipVOAFileNameNode = 
		getTextNode(ipXMLDOMDocument, "VOAFileName", document.m_strVoa);
	ASSERT_RESOURCE_ALLOCATION("ELI25151", ipVOAFileNameNode != NULL);
	
	// create a VOA file present node
	bool bVoaValid = isFileOrFolderValid(document.m_strVoa);
	MSXML::IXMLDOMNodePtr ipVOAFilePresentNode = 
		getTextNode(ipXMLDOMDocument, "VOAFilePresent", bVoaValid ? "Yes" : "No" );
	ASSERT_RESOURCE_ALLOCATION("ELI16855", ipVOAFilePresentNode != NULL);

	// Create an element node for the Date
	CString zDate = m_tmStarted.Format( "%m/%d/%Y" );
	MSXML::IXMLDOMNodePtr ipDateNode = getTextNode(ipXMLDOMDocument, "Date", LPCTSTR(zDate));
	ASSERT_RESOURCE_ALLOCATION("ELI11479", ipDateNode != NULL);

	// Create an element node for the Time Started
	CString zTime = m_tmStarted.Format( "%I:%M%p" );
	MSXML::IXMLDOMNodePtr ipTimeNode = getTextNode(ipXMLDOMDocument, "TimeStarted", LPCTSTR(zTime));
	ASSERT_RESOURCE_ALLOCATION("ELI11481", ipTimeNode != NULL);

	// Create an element node for the Total Seconds
	// (Use the total elapsed time for the current document [FlexIDSCore #3485])
	MSXML::IXMLDOMNodePtr ipSpanNode = getTextNode(ipXMLDOMDocument, "TotalSeconds",
		asString(m_swCurrTimeViewed.getElapsedTime()));
	ASSERT_RESOURCE_ALLOCATION("ELI11483", ipSpanNode != NULL);

	// Append the VerifiedByUser, VOAFileName, VOAFilePresent, Date, TimeStarted and 
	// TotalSeconds nodes to the VerificationInfo node
	ipVerificationNode->appendChild( ipVerifiedNode );
	ipVerificationNode->appendChild( ipVOAFileNameNode );
	ipVerificationNode->appendChild( ipVOAFilePresentNode );
	ipVerificationNode->appendChild( ipDateNode );
	ipVerificationNode->appendChild( ipTimeNode );
	ipVerificationNode->appendChild( ipSpanNode );

	// Append the VerificationInfo node to the main node
	ipMain->appendChild( ipVerificationNode );
}
//-------------------------------------------------------------------------------------------------
string CDataAreaDlg::getFontDescription(const LOGFONT& lgFont, int iPointSize)
{
	// Append the font name and size
	string strFont = lgFont.lfFaceName;
	strFont += " ";
	strFont += asString(iPointSize);
	strFont += "pt";

	// Append the font style
	bool bBold = lgFont.lfWeight >= FW_BOLD;
	bool bItalic = lgFont.lfItalic == gucIS_ITALIC;
	if (bBold || bItalic)
	{
		strFont += "; ";

		if (bBold)
		{
			strFont += "Bold ";
		}
		if (bItalic)
		{
			strFont += "Italic";
		}
	}

	// Return the result
	return strFont;
}
//-------------------------------------------------------------------------------------------------
MSXML::IXMLDOMNodePtr CDataAreaDlg::getRootNode(MSXML::IXMLDOMDocumentPtr ipXMLDOMDocument, 
		const string& strMetadataFile)
{
	// Check if the meta data file exists
	if (!isValidFile(strMetadataFile))
	{
		// Create the ID Shield meta data node
		MSXML::IXMLDOMNodePtr ipRoot = ipXMLDOMDocument->createElement("IDShieldMetaData");
		
		// Add an attribute for the metadata version
		MSXML::IXMLDOMElementPtr ipElement = ipRoot;
		ASSERT_RESOURCE_ALLOCATION("ELI18546", ipElement != NULL);

		// Set the meta data version
		ipElement->setAttribute(_bstr_t("Version"), asString(giCURRENT_META_DATA_VERSION).c_str());

		// Append the root node
		ipXMLDOMDocument->appendChild(ipRoot);

		// Return the resultant root node
		return ipRoot;
	}

	// Ensure asynchronous processing is disabled
	ipXMLDOMDocument->async = VARIANT_FALSE;

	// Load the XML File
	if (VARIANT_FALSE == ipXMLDOMDocument->load( _variant_t(strMetadataFile.c_str()) ))
	{
		UCLIDException ue("ELI25079", "Unable to load XML file.");
		ue.addDebugInfo("XML filename", strMetadataFile);
		throw ue;
	}

	// Get the root element
	MSXML::IXMLDOMElementPtr ipRoot = ipXMLDOMDocument->documentElement; 
	ASSERT_RESOURCE_ALLOCATION("ELI25080", ipRoot != NULL);

	// Get the root node's name
	string strRootNodeName = asString(ipRoot->nodeName);
	if (strRootNodeName != "IDShieldMetaData")
	{
		// Throw an exception
		UCLIDException ue("ELI25082", "Invalid metadata root node name.");
		ue.addDebugInfo("XML filename", strMetadataFile);
		throw ue;
	}

	// Get the attributes of the root node
	MSXML::IXMLDOMNamedNodeMapPtr ipRootAttributes = ipRoot->attributes;
	ASSERT_RESOURCE_ALLOCATION("ELI25081", ipRootAttributes != NULL);

	// Get the version attribute of this node
	MSXML::IXMLDOMNodePtr ipAttribute = ipRootAttributes->getNamedItem("Version");

	// Get the version number of the ID Shield meta data
	// NOTE: The first version had no version attribute, only the IDShieldMetaData tag
	long lVersionNumber = 1;		
	if (ipAttribute == NULL)
	{
		// Create an attribute for the metadata version
		ipAttribute = ipXMLDOMDocument->createAttribute("Version");
		ASSERT_RESOURCE_ALLOCATION("ELI25087", ipAttribute != NULL);

		// Add it to the root attributes
		ipRootAttributes->setNamedItem(ipAttribute);
	}
	else
	{
		lVersionNumber = asLong( asString(ipAttribute->text) );
	}

	// Ensure this metadata version is supported
	if (lVersionNumber > giCURRENT_META_DATA_VERSION)
	{
		UCLIDException ue("ELI25083", "Unsupported IDShield metadata version.");
		ue.addDebugInfo("XML filename", strMetadataFile);
		ue.addDebugInfo("Version number", lVersionNumber);
		ue.addDebugInfo("Maximum supported version", giCURRENT_META_DATA_VERSION);
		throw ue;
	}

	// If we are on the current version, we are done
	if (lVersionNumber == giCURRENT_META_DATA_VERSION)
	{
		return ipRoot;
	}

	// Update the version number to the current value
	ipAttribute->nodeValue = _variant_t(asString(giCURRENT_META_DATA_VERSION).c_str());

	// Create a session node
	MSXML::IXMLDOMElementPtr ipSessionNode = ipXMLDOMDocument->createElement("VerificationSession");
	ASSERT_RESOURCE_ALLOCATION("ELI25091", ipSessionNode != NULL);

	// Get the child nodes of the root
	MSXML::IXMLDOMNodeListPtr ipChildren = ipRoot->childNodes;
	ASSERT_RESOURCE_ALLOCATION("ELI25092", ipChildren != NULL);

	// Iterate through the children
	long lChildren = ipChildren->length;
	for (long i = 0; i < lChildren; i++)
	{
		// Move each child under the session node
		ipSessionNode->appendChild(ipRoot->firstChild);
	}

	// Add the session node to the root
	ipRoot->appendChild(ipSessionNode);

	// Return the result
	return ipRoot;
}
//-------------------------------------------------------------------------------------------------
MSXML::IXMLDOMNodePtr CDataAreaDlg::getTextNode(MSXML::IXMLDOMDocumentPtr ipXMLDOMDocument,
												const string& strName, const string& strText)
{
	// Create the node
	MSXML::IXMLDOMNodePtr ipNode = ipXMLDOMDocument->createElement(strName.c_str());
	ASSERT_RESOURCE_ALLOCATION("ELI25096", ipNode != NULL);

	// Create its associated text
	MSXML::IXMLDOMNodePtr ipText = ipXMLDOMDocument->createTextNode(strText.c_str());
	ASSERT_RESOURCE_ALLOCATION("ELI25097", ipText != NULL);
	ipNode->appendChild(ipText);

	// Return the node
	return ipNode;
}
//-------------------------------------------------------------------------------------------------
string CDataAreaDlg::getMetadataFileName(string strInputFile)
{
	// Get the out put file name with or without tags
	string strOut = m_UISettings.getMetaOutputName();

	// Call ExpandTagsAndTFE() to expand tags and functions
	strOut = CRedactionCustomComponentsUtils::ExpandTagsAndTFE(
		m_ipFAMTagManager, strOut, strInputFile);

	string strDir = getDirectoryFromFullPath(strOut, false);

	// Create the redacted directory if necessary
	createDirectory(strDir);

	return strOut;
}
//-------------------------------------------------------------------------------------------------
MSXML::IXMLDOMNodePtr CDataAreaDlg::getRedactionNode(MSXML::IXMLDOMDocumentPtr ipXMLDOMDocument, 
													 DataItem &di)
{
	// Check arguments passed in
	ASSERT_ARGUMENT("ELI11493", ipXMLDOMDocument != NULL);

	// Create the Redaction node
	MSXML::IXMLDOMNodePtr ipRedactionNode = ipXMLDOMDocument->createElement( "Redaction" );
	ASSERT_RESOURCE_ALLOCATION("ELI11456", ipRedactionNode != NULL);

	MSXML::IXMLDOMElementPtr ipElement = ipRedactionNode;
	ASSERT_RESOURCE_ALLOCATION("ELI18542", ipElement != NULL);

	// Set category and type elements [p16 #2379]
	ipElement->setAttribute( _bstr_t("Category"),
		di.m_pDisplaySettings->getCategory().c_str());

	ipElement->setAttribute(_bstr_t("Type"),
		di.m_pDisplaySettings->getType().c_str());

	// Set Output attribute
	string strOutput = "0";
	if (di.m_pDisplaySettings->getRedactChoice() == kRedactYes)
	{
		strOutput = "1";
	}
	ipElement->setAttribute( _bstr_t( "Output" ), strOutput.c_str() );

	// Retrieve this Settings object
	DataDisplaySettings* pDDS = di.m_pDisplaySettings;
	ASSERT_RESOURCE_ALLOCATION("ELI11454", pDDS != NULL);

	// Retrieve this Attribute object
	IAttributePtr ipAttribute = di.m_ipAttribute;
	ASSERT_RESOURCE_ALLOCATION("ELI11455", ipAttribute != NULL);

	// Get the Spatial String
	ISpatialStringPtr ipValue = ipAttribute->Value;
	ASSERT_RESOURCE_ALLOCATION("ELI11457", ipValue != NULL);

	// The Spatial String is expected to be Spatial
	if (ipValue->HasSpatialInfo() == VARIANT_TRUE)
	{
		// Get the lines associated with the spatial string so
		// that a "Line" node can be created underneath the value
		// for each line of text in the value
		IIUnknownVectorPtr ipLines = ipValue->GetLines();
		ASSERT_RESOURCE_ALLOCATION("ELI11458", ipLines != NULL);

		// Create Line XML nodes for each line in the value
		long nNumLines = ipLines->Size();
		for (long nLineNum = 0; nLineNum < nNumLines; nLineNum++)
		{
			// Get the spatial string on the line
			ISpatialStringPtr ipLine = ipLines->At( nLineNum );
			ASSERT_RESOURCE_ALLOCATION("ELI11459", ipLine != NULL);

			// Certain advanced replace rules can insert non-spatial information
			// at the end of a spatial string separated by a newline. This would cause
			// non-spatial information at this point. It is not possible to 
			// GetRasterZones() from a spatial string that does not have spatial information.
			// P13 : 4184
			if( ipLine->HasSpatialInfo() == VARIANT_FALSE )
			{
				continue;
			}

			// Get the associated RasterZone vector
			IIUnknownVectorPtr vecRasterZones = ipLine->GetOriginalImageRasterZones();
			ASSERT_RESOURCE_ALLOCATION("ELI14653", vecRasterZones != NULL);
			
			// Make sure that there is at least 1 zone in the vector
			long lSize = vecRasterZones->Size();
			if (lSize <= 0)
			{
				THROW_LOGIC_ERROR_EXCEPTION("ELI25206");
			}
			
			// Create a Line element
			MSXML::IXMLDOMNodePtr ipLineNode = ipXMLDOMDocument->createElement( "Line" );
			ASSERT_RESOURCE_ALLOCATION("ELI11460", ipLineNode != NULL);

			// Ensure all characters in the string are printable characters (P16 #2356)
			string strText = asString(ipLine->String);
			strText = removeUnprintableCharacters( strText );

			// Add a FullText sub element
			MSXML::IXMLDOMNodePtr ipFullTextNode = 
				getTextNode(ipXMLDOMDocument, "FullText", strText);
			ASSERT_RESOURCE_ALLOCATION("ELI18543", ipFullTextNode != NULL);

			// Append the nodes
			ipLineNode->appendChild(ipFullTextNode);

			// Iterate through each raster zone of the line. [LegacyRCAndUtils #5187]
			// Note: Hybrid strings can have more than one raster zone per line.
			for (long i = 0; i < lSize; i++)
			{
				// Get the raster zone from the vector
				IRasterZonePtr	ipZone = vecRasterZones->At(i);
				ASSERT_RESOURCE_ALLOCATION("ELI11461", ipZone != NULL);

				// Create a Zone element
				MSXML::IXMLDOMElementPtr ipZoneElement = ipXMLDOMDocument->createElement( "Zone" );
				ASSERT_RESOURCE_ALLOCATION("ELI11466", ipZoneElement != NULL);

				// Set the StartX attribute for the Zone element
				string strValue = asString( ipZone->StartX );
				ipZoneElement->setAttribute( _bstr_t("StartX"), _bstr_t( strValue.c_str() ) );

				// Set the StartY attribute for the Zone element
				strValue = asString( ipZone->StartY );
				ipZoneElement->setAttribute( _bstr_t("StartY"), _bstr_t( strValue.c_str() ) );

				// Set the EndX attribute for the Zone element
				strValue = asString( ipZone->EndX );
				ipZoneElement->setAttribute( _bstr_t("EndX"), _bstr_t( strValue.c_str() ) );

				// Set the EndY attribute for the Zone element
				strValue = asString( ipZone->EndY );
				ipZoneElement->setAttribute( _bstr_t("EndY"), _bstr_t( strValue.c_str() ) );

				// Set the Height attribute for the Zone element
				strValue = asString( ipZone->Height );
				ipZoneElement->setAttribute( _bstr_t("Height"), _bstr_t( strValue.c_str() ) );

				// Set the PageNumber attribute for the Zone element
				strValue = asString( ipZone->PageNumber );
				ipZoneElement->setAttribute( _bstr_t("PageNumber"), _bstr_t( strValue.c_str() ) );

				// Append the Zone element as a child of the Line node
				ipLineNode->appendChild( ipZoneElement );
			}

			// Append the Line node as a child of the Redaction node
			ipRedactionNode->appendChild( ipLineNode );
		}
	}
	// Perhaps it is a Manual redaction
	else
	{
		// Create a Line node
		MSXML::IXMLDOMNodePtr ipLineNode = ipXMLDOMDocument->createElement( "Line" );
		ASSERT_RESOURCE_ALLOCATION("ELI11497", ipLineNode != NULL);

		// Add a FullText sub element
		MSXML::IXMLDOMNodePtr ipFullTextNode = getTextNode(ipXMLDOMDocument, "FullText" , pDDS->getText());
		ASSERT_RESOURCE_ALLOCATION("ELI18544", ipFullTextNode != NULL);
		ipLineNode->appendChild(ipFullTextNode);

		// Create a Zone element
		MSXML::IXMLDOMElementPtr ipZoneElement = ipXMLDOMDocument->createElement( "Zone" );
		ASSERT_RESOURCE_ALLOCATION("ELI11498", ipZoneElement != NULL);

		// Set the attributes of the rectangular bounds element
		IRasterZonePtr ipZone = di.getFirstRasterZone();
		ASSERT_RESOURCE_ALLOCATION("ELI23560", ipZone != NULL);
		string strStartX = asString( ipZone->StartX );
		string strStartY = asString( ipZone->StartY );
		string strEndX = asString( ipZone->EndX );
		string strEndY = asString( ipZone->EndY );
		string strHeight = asString( ipZone->Height );
		string strPageNumber = asString( ipZone->PageNumber );
		ipZoneElement->setAttribute( _bstr_t("StartX"), strStartX.c_str() );
		ipZoneElement->setAttribute( _bstr_t("StartY"), strStartY.c_str() );
		ipZoneElement->setAttribute( _bstr_t("EndX"), strEndX.c_str() );
		ipZoneElement->setAttribute( _bstr_t("EndY"), strEndY.c_str() );
		ipZoneElement->setAttribute( _bstr_t("Height"), strHeight.c_str() );
		ipZoneElement->setAttribute( _bstr_t("PageNumber"), strPageNumber.c_str() );

		// Append the Zone element as a child of the Line node
		ipLineNode->appendChild( ipZoneElement );

		// Append the Line node as a child of the Redaction node
		ipRedactionNode->appendChild( ipLineNode );
	}

	// Add an exemption code if exemption codes are present
	ExemptionCodeList& exemptions = di.getExemptionCodes();
	if (!exemptions.isEmpty())
	{
		// Get the exemption codes and exemption categories as strings
		string& strCategory = exemptions.getCategory();
		string& strCode = exemptions.getAsString();

		// Create the ExemptionCode node
		MSXML::IXMLDOMElementPtr ipExemption = ipXMLDOMDocument->createElement("ExemptionCode");
		ASSERT_RESOURCE_ALLOCATION("ELI25106", ipExemption != NULL);
		ipRedactionNode->appendChild(ipExemption);

		// Create the attributes for the ExemptionCode node
		ipExemption->setAttribute("Category", _variant_t(strCategory.c_str()) );
		ipExemption->setAttribute("Code", _variant_t(strCode.c_str()) );
	}

	return ipRedactionNode;
}
//-------------------------------------------------------------------------------------------------
void CDataAreaDlg::writeMetadata(const DocumentData& document)
{
	try
	{
		// Skip creation of metadata file if no redactions made AND 
		// file creation is not forced
		long lRedactCount = getRedactionsCount();
		if ((lRedactCount == 0) && (!m_UISettings.getAlwaysOutputMeta()))
		{
			return;
		}

		// Create output filename
		string strOut = getMetadataFileName(document.m_strOriginal);

		// Create XML document object
		MSXML::IXMLDOMDocumentPtr ipXMLDOMDocument(CLSID_DOMDocument);
		ASSERT_RESOURCE_ALLOCATION("ELI11469", ipXMLDOMDocument != NULL);

		// Get the root node
		MSXML::IXMLDOMNodePtr ipRootNode = getRootNode(ipXMLDOMDocument, strOut);
		ASSERT_RESOURCE_ALLOCATION("ELI11470", ipRootNode != NULL);

		// Check if this is a history queue item
		if (isInHistoryQueue())
		{
			// This verification session is the same as the last one. 
			// Remove the previous before creating a new one. [FlexIDSCore #3354]
			MSXML::IXMLDOMNodePtr ipLastSession = ipRootNode->lastChild;
			if (ipLastSession != NULL)
			{
				ipRootNode->removeChild(ipLastSession);
			}
		}

		// Create main verification session node
		MSXML::IXMLDOMNodePtr ipMainNode = ipXMLDOMDocument->createElement("VerificationSession");
		ASSERT_RESOURCE_ALLOCATION("ELI25152", ipMainNode != NULL);
		ipRootNode->appendChild(ipMainNode);

		// Add DocumentInfo node
		addXMLDocumentNode(ipXMLDOMDocument, ipMainNode, document);

		// Add VerificationInfo node
		addXMLVerificationNode(ipXMLDOMDocument, ipMainNode, document);

		// Add RedactionTextAndColorSettings node
		addXMLRedactionAppearanceNode(ipXMLDOMDocument, ipMainNode);

		// Add the Redaction nodes
		addXMLDataNodes(ipXMLDOMDocument, ipMainNode);

		// The save method interprets the % and the next 2 characters as a hex char so
		// replacing the % with %25 will cause a % to be in the filename to be correct
		replaceVariable(strOut, "%", "%25");

		// Write the XML file
		_variant_t varString = _T( strOut.c_str() );
		ipXMLDOMDocument->save(varString);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25228");
}
//-------------------------------------------------------------------------------------------------
