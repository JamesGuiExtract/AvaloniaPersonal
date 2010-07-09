// XMLVersion1Writer.cpp : Implementation of XMLVersion1Writer
#include "stdafx.h"
#include "AFOutputHandlers.h"
#include "XMLVersion1Writer.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <COMUtils.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const string gstrSPATIAL_XML_ATTRIBUTE_NAME = "Spatial";
const string gstrSPATIAL_YES = "1";
const string gstrSPATIAL_NO = "0";

const string gstrCONFIDENCE_ATTRIBUTE_NAME = "AverageCharConfidence";

//-------------------------------------------------------------------------------------------------
// XMLVersion1Writer
//-------------------------------------------------------------------------------------------------
XMLVersion1Writer::XMLVersion1Writer(bool bRemoveSpatialInfo) :
m_bRemoveSpatialInfo(bRemoveSpatialInfo)
{
}
//-------------------------------------------------------------------------------------------------
void XMLVersion1Writer::WriteFile(const string& strFile, IIUnknownVector *pAttributes)
{
	try
	{
		// Create XML document object and populate nodes
		MSXML::IXMLDOMDocumentPtr ipXMLDOMDocument( CLSID_DOMDocument );
		ASSERT_RESOURCE_ALLOCATION("ELI12866", ipXMLDOMDocument != NULL);

		// Create "Attributes" Element
		MSXML::IXMLDOMNodePtr ipAttributesNode = ipXMLDOMDocument->createElement("Attributes");
		ASSERT_RESOURCE_ALLOCATION("ELI12867", ipAttributesNode != NULL);

		// Populate Attribute nodes
		addNodesForAttributes( ipXMLDOMDocument, ipAttributesNode, pAttributes );
		ipXMLDOMDocument->appendChild( ipAttributesNode );
		
		// The output filename may be associated with a folder that does not
		// exist.  If so, try to create that folder
		string strFolder = getDirectoryFromFullPath( strFile );
		if (!directoryExists( strFolder ))
		{
			createDirectory( strFolder );
		}

		// The save method interprets the % and the next 2 characters as a hex char so
		// replacing the % with %25 will cause a % to be in the filename to be correct
		string strFileName = strFile;
		replaceVariable( strFileName, "%", "%25" );

		// Write the XML file
		_variant_t varString = _T( strFileName.c_str() );
		ipXMLDOMDocument->save( varString );

		// Make sure the file can be read
		waitForFileToBeReadable(strFileName);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI12868")
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void XMLVersion1Writer::addNodesForAttributes(MSXML::IXMLDOMDocumentPtr ipXMLDOMDocument,
											  MSXML::IXMLDOMNodePtr ipNode, 
											  IIUnknownVectorPtr ipAttributes)
{
	// Ensure proper arguments passed in
	ASSERT_ARGUMENT("ELI12869", ipXMLDOMDocument != NULL);
	ASSERT_ARGUMENT("ELI12870", ipNode != NULL);
	ASSERT_ARGUMENT("ELI12871", ipAttributes != NULL);

	// Iterate through each of the attributes and handle them
	long nNumAttributes = ipAttributes->Size();
	for (int i = 0; i < nNumAttributes; i++)
	{
		// Get the current attribute
		UCLID_AFCORELib::IAttributePtr ipAttribute = ipAttributes->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI12872", ipAttribute != NULL);

		// Create an element node for the current attribute
		MSXML::IXMLDOMNodePtr ipAttributeNode = ipXMLDOMDocument->createElement("Attribute");
		ASSERT_RESOURCE_ALLOCATION("ELI12873", ipAttributeNode != NULL);
		
		// Set the name attribute for the element
		MSXML::IXMLDOMElementPtr ipAttr = ipAttributeNode;
		ipAttr->setAttribute(_bstr_t("Name"), ipAttribute->Name);

		// Set the type attribute for the element optionally if it is defined
		_bstr_t _bstrAttrType = ipAttribute->Type;
		if (string(_bstrAttrType) != "")
		{
			ipAttr->setAttribute(_bstr_t("Type"), _bstrAttrType);
		}

		// Create the value element
		MSXML::IXMLDOMElementPtr ipValueElement = getValueElement(ipXMLDOMDocument, 
			ipAttribute);

		// Append the value node to the attribute, and 
		// the attribute node to the parent node
		ipAttributeNode->appendChild(ipValueElement);
		ipNode->appendChild(ipAttributeNode);

		// Create nodes for the sub-attributes if they exist
		IIUnknownVectorPtr ipSubAttributes = ipAttribute->SubAttributes;
		if (ipSubAttributes != NULL && ipSubAttributes->Size() > 0)
		{
			MSXML::IXMLDOMNodePtr ipSubAttributesNode = ipXMLDOMDocument->createElement("SubAttributes");
			addNodesForAttributes(ipXMLDOMDocument, ipSubAttributesNode, ipSubAttributes);
			ipAttributeNode->appendChild(ipSubAttributesNode);
		}
	}
}
//-------------------------------------------------------------------------------------------------
MSXML::IXMLDOMElementPtr XMLVersion1Writer::getValueElement(MSXML::IXMLDOMDocumentPtr ipXMLDOMDocument, 
															IAttributePtr ipAttribute)
{
	// Create the attribute value node
	MSXML::IXMLDOMNodePtr ipValueNode = ipXMLDOMDocument->createElement("Value");
	ASSERT_RESOURCE_ALLOCATION("ELI12874", ipValueNode != NULL);

	// Get the attribute and its value into local vars
	ISpatialStringPtr ipValue = ipAttribute->Value;
	ASSERT_RESOURCE_ALLOCATION("ELI12875", ipValue != NULL);

	// Create a text node with the actual attribute value's text
	// removing any unprintable characters (P16 #1413)
	string strTest = asString(ipValue->String);
	string strValue = removeUnprintableCharacters( strTest );
	MSXML::IXMLDOMNodePtr ipValueNodeText = ipXMLDOMDocument->createTextNode( 
		strValue.c_str() );
	ASSERT_RESOURCE_ALLOCATION("ELI12876", ipValueNodeText != NULL);
	ipValueNode->appendChild(ipValueNodeText);
	
	// Set the spatial bounds-attributes on the value element if the attribute's
	// value is spatial
	MSXML::IXMLDOMElementPtr ipValueElement = ipValueNode;
	ASSERT_RESOURCE_ALLOCATION("ELI12877", ipValueElement != NULL);

	// Only add the spatial information if not removing spatial information
	// and it has spatial info [FlexIDSCore #3557]
	_bstr_t _bstrIsSpatialAttrName = gstrSPATIAL_XML_ATTRIBUTE_NAME.c_str();
	if (!m_bRemoveSpatialInfo && ipValue->HasSpatialInfo() == VARIANT_TRUE)
	{
		// Mark the value as spatial
		ipValueElement->setAttribute(_bstrIsSpatialAttrName, gstrSPATIAL_YES.c_str());

		// Retrieve Average Character Confidence
		long lMin = 0; 
		long lMax = 0; 
		long lAvg = 0;
		ipValue->GetCharConfidence( &lMin, &lMax, &lAvg );
		string strAvg = asString( lAvg );

		// Add Average Character Confidence - P16 #1680
		ipValueElement->setAttribute( gstrCONFIDENCE_ATTRIBUTE_NAME.c_str(), strAvg.c_str());

		// Get the lines associated with the spatial string so
		// that a "Line" node can be created underneath the value
		// for each line of text in the value
		IIUnknownVectorPtr ipLines = ipValue->GetLines();
		ASSERT_RESOURCE_ALLOCATION("ELI12878", ipLines != NULL);

		// Create Line XML nodes for each line in the value
		long nNumLines = ipLines->Size();
		for (long nLineNum = 0; nLineNum < nNumLines; nLineNum++)
		{
			// Get the spatial string on the line
			ISpatialStringPtr ipLine = ipLines->At(nLineNum);
			ASSERT_RESOURCE_ALLOCATION("ELI12879", ipLine != NULL);

			MSXML::IXMLDOMElementPtr ipLineElement = getLineElement(ipXMLDOMDocument,
				ipAttribute, ipLine, nLineNum);
			ASSERT_RESOURCE_ALLOCATION("ELI12880", ipLineElement != NULL)

			// Append the line node as a child of the value node
			ipValueElement->appendChild(ipLineElement);
		}
	}
	else
	{
		// Mark the value as not spatial
		ipValueElement->setAttribute(gstrSPATIAL_XML_ATTRIBUTE_NAME.c_str(), gstrSPATIAL_NO.c_str());
	}

	return ipValueElement;
}
//-------------------------------------------------------------------------------------------------
MSXML::IXMLDOMElementPtr XMLVersion1Writer::getLineElement(MSXML::IXMLDOMDocumentPtr ipXMLDOMDocument, 
														   IAttributePtr ipAttribute, 
														   ISpatialStringPtr ipLine, 
														   long nLineNum)
{
	// Create the SpatialLine element
	MSXML::IXMLDOMNodePtr ipLineNode = ipXMLDOMDocument->createElement("Line");
	ASSERT_RESOURCE_ALLOCATION("ELI12881", ipLineNode != NULL);

	// Create a text node with the actual line's text
	// removing any unprintable characters (P16 #1413)
	string strLine = asString(ipLine->String);
	string strValue = removeUnprintableCharacters( strLine );
	MSXML::IXMLDOMNodePtr ipLineNodeText = ipXMLDOMDocument->
		createTextNode( strValue.c_str() );
	ASSERT_RESOURCE_ALLOCATION("ELI12882", ipLineNodeText != NULL);
	ipLineNode->appendChild(ipLineNodeText);
	
	// Get access to the XML element interface of the line node
	MSXML::IXMLDOMElementPtr ipLineElement = ipLineNode;
	ASSERT_RESOURCE_ALLOCATION("ELI12883", ipLineElement != NULL);

	if (ipLine->HasSpatialInfo() == VARIANT_TRUE)
	{
		// Set the line as spatial
		ipLineElement->setAttribute(gstrSPATIAL_XML_ATTRIBUTE_NAME.c_str(), gstrSPATIAL_YES.c_str());

		// append the zone and bound elements
		appendSpatialElements(ipLineElement, ipXMLDOMDocument, ipAttribute, ipLine, nLineNum);
	}
	else
	{
		// Set the line as not spatial
		ipLineElement->setAttribute(gstrSPATIAL_XML_ATTRIBUTE_NAME.c_str(), gstrSPATIAL_NO.c_str());
	}

	return ipLineElement;
}
//-------------------------------------------------------------------------------------------------
MSXML::IXMLDOMElementPtr XMLVersion1Writer::getRectBoundsElement(MSXML::IXMLDOMDocumentPtr ipXMLDOMDocument, 
		IRasterZonePtr ipZone, ILongToObjectMapPtr ipPageInfoMap)
{
	ASSERT_ARGUMENT("ELI19197", ipXMLDOMDocument != NULL);
	ASSERT_ARGUMENT("ELI19198", ipZone != NULL);
	ASSERT_ARGUMENT("ELI19868", ipPageInfoMap != NULL);

	// Create the rectangular boundary element
	MSXML::IXMLDOMElementPtr ipRectBoundsElement = ipXMLDOMDocument->
		createElement("RectBounds");
	ASSERT_RESOURCE_ALLOCATION("ELI12886", ipRectBoundsElement != NULL);

	ISpatialPageInfoPtr ipPageInfo = ipPageInfoMap->GetValue(ipZone->PageNumber);
	ASSERT_RESOURCE_ALLOCATION("ELI30327", ipPageInfo != NULL);

	// Get the page bounds (for use by GetRectangularBounds)
	ILongRectanglePtr ipPageBounds(CLSID_LongRectangle);
	ASSERT_RESOURCE_ALLOCATION("ELI30313", ipPageBounds != NULL);
	ipPageBounds->SetBounds(0, 0, ipPageInfo->Width, ipPageInfo->Height);

	// Get the rectangular bounds
	ILongRectanglePtr ipBounds = ipZone->GetRectangularBounds(ipPageBounds);
	ASSERT_RESOURCE_ALLOCATION("ELI12887", ipBounds != NULL);
	
	// Set the attributes of the rectangular bounds element
	string strPage = asString(ipZone->PageNumber);
	string strTop = asString(ipBounds->Top);
	string strLeft = asString(ipBounds->Left);
	string strBottom = asString(ipBounds->Bottom);
	string strRight = asString(ipBounds->Right);
	ipRectBoundsElement->setAttribute(_bstr_t("Top"), strTop.c_str());
	ipRectBoundsElement->setAttribute(_bstr_t("Left"), strLeft.c_str());
	ipRectBoundsElement->setAttribute(_bstr_t("Bottom"), strBottom.c_str());
	ipRectBoundsElement->setAttribute(_bstr_t("Right"), strRight.c_str());
	ipRectBoundsElement->setAttribute(_bstr_t("PageNumber"), strPage.c_str());

	return ipRectBoundsElement;
}
//-------------------------------------------------------------------------------------------------
MSXML::IXMLDOMElementPtr XMLVersion1Writer::getZoneElement(MSXML::IXMLDOMDocumentPtr ipXMLDOMDocument, 
		IRasterZonePtr ipZone)
{
	ASSERT_ARGUMENT("ELI19199", ipXMLDOMDocument != NULL);
	ASSERT_ARGUMENT("ELI19200", ipZone != NULL);

	// Create a new zone element
	MSXML::IXMLDOMElementPtr ipZoneElement = ipXMLDOMDocument->
		createElement("Zone");
	ASSERT_RESOURCE_ALLOCATION("ELI12888", ipZoneElement != NULL);

	// Set the attributes of the rectangular bounds element				
	string strStartX = asString(ipZone->StartX);
	string strStartY = asString(ipZone->StartY);
	string strEndX = asString(ipZone->EndX);
	string strEndY = asString(ipZone->EndY);
	string strHeight = asString(ipZone->Height);
	string strPageNumber = asString(ipZone->PageNumber);
	ipZoneElement->setAttribute(_bstr_t("StartX"), strStartX.c_str());
	ipZoneElement->setAttribute(_bstr_t("StartY"), strStartY.c_str());
	ipZoneElement->setAttribute(_bstr_t("EndX"), strEndX.c_str());
	ipZoneElement->setAttribute(_bstr_t("EndY"), strEndY.c_str());
	ipZoneElement->setAttribute(_bstr_t("Height"), strHeight.c_str());
	ipZoneElement->setAttribute(_bstr_t("PageNumber"), strPageNumber.c_str());

	return ipZoneElement;
}
//-------------------------------------------------------------------------------------------------
void XMLVersion1Writer::appendSpatialElements(MSXML::IXMLDOMNodePtr ipLineElement, 
		MSXML::IXMLDOMDocumentPtr ipXMLDOMDocument,	IAttributePtr ipAttribute, 
		ISpatialStringPtr ipLine, long nLineNum)
{
	ASSERT_ARGUMENT("ELI19201", ipLineElement != NULL);
	ASSERT_ARGUMENT("ELI19202", ipXMLDOMDocument != NULL);
	ASSERT_ARGUMENT("ELI19203", ipAttribute != NULL);
	ASSERT_ARGUMENT("ELI19204", ipLine != NULL);

	// get the raster zones of the specified line
	IIUnknownVectorPtr ipZones = ipLine->GetOriginalImageRasterZones();
	ASSERT_RESOURCE_ALLOCATION("ELI12889", ipZones != NULL);

	// get the attribute value
	ISpatialStringPtr ipValue = ipAttribute->Value;
	ASSERT_RESOURCE_ALLOCATION("ELI12890", ipValue != NULL);

	// there should be at least one zone. Verify that.
	long nNumZones = ipZones->Size();
	if (nNumZones < 1)
	{
		UCLIDException ue("ELI12891", "Unexpected number of zones found for line text!");						
		string strLineText = asString(ipLine->String);
		string strAttrName = asString(ipAttribute->Name);
		string strAttrValue =  asString(ipValue->String);
		string strAttrType = asString(ipAttribute->Type);
		ue.addDebugInfo("AttrName", strAttrName);
		ue.addDebugInfo("AttrValue", strAttrValue);
		ue.addDebugInfo("AttrType", strAttrType);
		ue.addDebugInfo("nLineNum", nLineNum);
		ue.addDebugInfo("strLineText", strLineText);
		ue.addDebugInfo("NumZones", nNumZones);
		throw ue;
	}

	// get the page info map
	ILongToObjectMapPtr ipPageInfoMap(ipValue->SpatialPageInfos);
	ASSERT_RESOURCE_ALLOCATION("ELI19871", ipPageInfoMap != NULL);

	// iterate through each zone
	for(long i=0; i<nNumZones; i++)
	{
		// Get the zone
		IRasterZonePtr ipZone = ipZones->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI12892", ipZone != NULL);

		// Create the rectangular boundary element and append as child to line element
		MSXML::IXMLDOMElementPtr ipRectBoundsElement = getRectBoundsElement(
			ipXMLDOMDocument, ipZone, ipPageInfoMap);
		ASSERT_RESOURCE_ALLOCATION("ELI12884", ipRectBoundsElement != NULL);
		ipLineElement->appendChild(ipRectBoundsElement);
		
		// Create the zone element and append as child to line element
		MSXML::IXMLDOMElementPtr ipZoneElement = getZoneElement(ipXMLDOMDocument, ipZone);
		ASSERT_RESOURCE_ALLOCATION("ELI12885", ipZoneElement != NULL);
		ipLineElement->appendChild(ipZoneElement);
	}
}
//-------------------------------------------------------------------------------------------------
