// XMLVersion2Writer.cpp : Implementation of XMLVersion2Writer
#include "stdafx.h"
#include "AFOutputHandlers.h"
#include "XMLVersion2Writer.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <io.h>
#include <COMUtils.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const string gstrTOP_LEVEL_ATTRIBUTE_NAME = "FlexData";
const string gstrFIELD_LEVEL_ATTRIBUTE_NAME = "Field";
const string gstrFIELD_LEVEL_NAME_LABEL = "FieldName";
const string gstrFIELD_LEVEL_TYPE_LABEL = "FieldType";
const string gstrFULLTEXT_LEVEL_ATTRIBUTE_NAME = "FullText";
const string gstrFIELDLINE_LEVEL_ATTRIBUTE_NAME = "SpatialLine";
const string gstrLINETEXT_LEVEL_ATTRIBUTE_NAME = "LineText";
const string gstrZONE_LEVEL_ATTRIBUTE_NAME = "SpatialLineZone";
const string gstrBOUNDS_LEVEL_ATTRIBUTE_NAME = "SpatialLineBounds";
const string gstrPAGE_LEVEL_ATTRIBUTE_NAME = "PageNumber";

const string gstrCONFIDENCE_ATTRIBUTE_NAME = "AverageCharConfidence";

const string gstrXMLNS_NAME = "xmlns:xsi";
const string gstrXMLNS_VALUE = "http://www.w3.org/2001/XMLSchema-instance";
const string gstrXSI_NAME = "xsi:noNamespaceSchemaLocation";

//-------------------------------------------------------------------------------------------------
// XMLVersion2Writer
//-------------------------------------------------------------------------------------------------
XMLVersion2Writer::XMLVersion2Writer(bool bRemoveSpatialInfo)
:	m_bUseNamedAttributes(false), m_bRemoveSpatialInfo(bRemoveSpatialInfo)
{
}
//-------------------------------------------------------------------------------------------------
void XMLVersion2Writer::UseNamedAttributes(bool bUseNamedAttributes)
{
	try
	{
		// Store setting
		m_bUseNamedAttributes = bUseNamedAttributes;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI12907")
}
//-------------------------------------------------------------------------------------------------
void XMLVersion2Writer::UseSchemaName(const string& strSchemaName)
{
	try
	{
		// Save the setting
		m_strSchemaName = strSchemaName;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI12916")
}
//-------------------------------------------------------------------------------------------------
void XMLVersion2Writer::WriteFile(const string& strFile, IIUnknownVector *pAttributes)
{
	try
	{
		// Create XML document object and populate nodes
		MSXML::IXMLDOMDocumentPtr ipXMLDOMDocument( CLSID_DOMDocument );
		ASSERT_RESOURCE_ALLOCATION("ELI19160", ipXMLDOMDocument != NULL);

		// Create Top-Level Element
		MSXML::IXMLDOMNodePtr ipAttributesNode = ipXMLDOMDocument->createElement( 
			gstrTOP_LEVEL_ATTRIBUTE_NAME.c_str() );
		ASSERT_RESOURCE_ALLOCATION("ELI19162", ipAttributesNode != NULL);

		// Add Schema information if desired
		if (m_strSchemaName.length() > 0)
		{
			// Get XML element
			MSXML::IXMLDOMElementPtr ipAttr = ipAttributesNode;
			ASSERT_RESOURCE_ALLOCATION("ELI12917", ipAttr != NULL);

			// Add xmlns:xsi item
			ipAttr->setAttribute( _bstr_t( gstrXMLNS_NAME.c_str() ), 
				_bstr_t( gstrXMLNS_VALUE.c_str() ) );

			// Add xsi:noNamespaceSchemaLocation item
			ipAttr->setAttribute( _bstr_t( gstrXSI_NAME.c_str() ), 
				_bstr_t( m_strSchemaName.c_str() ) );
		}

		// Populate Top-Level nodes
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
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI12918")
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void XMLVersion2Writer::addNodesForAttributes(MSXML::IXMLDOMDocumentPtr ipXMLDOMDocument,
											  MSXML::IXMLDOMNodePtr ipNode, 
											  IIUnknownVectorPtr ipAttributes)
{
	// Ensure proper arguments passed in
	ASSERT_ARGUMENT("ELI19164", ipXMLDOMDocument != NULL);
	ASSERT_ARGUMENT("ELI19166", ipNode != NULL);
	ASSERT_ARGUMENT("ELI19167", ipAttributes != NULL);

	// Iterate through each of the attributes and handle them
	long nNumAttributes = ipAttributes->Size();
	for (long i = 0; i < nNumAttributes; i++)
	{
		// Get the current attribute
		UCLID_AFCORELib::IAttributePtr ipAttribute = ipAttributes->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI19168", ipAttribute != NULL);

		/////////////////////////
		// Create an "Field" node for the current attribute
		/////////////////////////
		MSXML::IXMLDOMNodePtr ipAttributeNode;
		if (m_bUseNamedAttributes)
		{
			// Use Attribute Name
			ipAttributeNode = ipXMLDOMDocument->createElement( ipAttribute->Name );
			ASSERT_RESOURCE_ALLOCATION("ELI12903", ipAttributeNode != NULL);
		}
		else
		{
			ipAttributeNode = ipXMLDOMDocument->createElement( 
				gstrFIELD_LEVEL_ATTRIBUTE_NAME.c_str() );
			ASSERT_RESOURCE_ALLOCATION("ELI19169", ipAttributeNode != NULL);

			// Set the name attribute for the element
			MSXML::IXMLDOMElementPtr ipAttr = ipAttributeNode;
			ipAttr->setAttribute(gstrFIELD_LEVEL_NAME_LABEL.c_str(), ipAttribute->Name);
		}

		// Set the type attribute for the element optionally if it is defined
		_bstr_t _bstrAttrType = ipAttribute->Type;
		if (_bstrAttrType.length() > 0)
		{
			MSXML::IXMLDOMElementPtr ipAttr = ipAttributeNode;
			ipAttr->setAttribute(_bstr_t( gstrFIELD_LEVEL_TYPE_LABEL.c_str() ), 
				_bstrAttrType);
		}

		// Get the spatial string from the attribute
		ISpatialStringPtr ipText = ipAttribute->Value;
		ASSERT_RESOURCE_ALLOCATION("ELI15536", ipText != NULL);

		//////////////////////////////
		// Create the FullText element
		//////////////////////////////
		MSXML::IXMLDOMElementPtr ipValueElement = getValueElement(ipXMLDOMDocument, ipText);

		// Append the FullText node to the attribute
		ipAttributeNode->appendChild(ipValueElement);

		// Only add the spatial line information if not removing spatial info [FlexIDSCore #3557]
		if (!m_bRemoveSpatialInfo)
		{
			//////////////////////////////////
			// Create the FieldLine element(s)
			//////////////////////////////////
			// Each line in the text will get a node
			IIUnknownVectorPtr ipLines = ipText->GetLines();
			ASSERT_RESOURCE_ALLOCATION("ELI19170", ipLines != NULL);

			long nNumLines = ipLines->Size();
			for (long nLineNum = 0; nLineNum < nNumLines; nLineNum++)
			{
				// Get the spatial string on the line
				ISpatialStringPtr ipLine = ipLines->At( nLineNum );
				ASSERT_RESOURCE_ALLOCATION("ELI19171", ipLine != NULL);

				// Skip this line if NOT spatial 
				if (ipLine->HasSpatialInfo() == VARIANT_TRUE)
				{
					// Create the FieldLine element
					MSXML::IXMLDOMElementPtr ipLineElement = getLineElement(
						ipXMLDOMDocument, ipAttribute, ipLine, nLineNum );
					ASSERT_RESOURCE_ALLOCATION("ELI19176", ipLineElement != NULL)

						// Append the FieldLine node to the attribute
						ipAttributeNode->appendChild( ipLineElement );
				}
			}
		}

		// Append the attribute node to the parent node
		ipNode->appendChild( ipAttributeNode );

		/////////////////////////
		// Create nodes for the sub-attributes if they exist
		/////////////////////////
		IIUnknownVectorPtr ipSubAttributes = ipAttribute->SubAttributes;
		if (ipSubAttributes != NULL && ipSubAttributes->Size() > 0)
		{
			// Add sub-attributes parallel to the SpatialLine elements
			addNodesForAttributes( ipXMLDOMDocument, ipAttributeNode, ipSubAttributes );
		}
	}
}
//-------------------------------------------------------------------------------------------------
MSXML::IXMLDOMElementPtr XMLVersion2Writer::getValueElement(MSXML::IXMLDOMDocumentPtr ipXMLDOMDocument, 
															ISpatialStringPtr ipValue)
{
	ASSERT_ARGUMENT("ELI19173", ipValue != NULL);

	// Create the attribute value node
	MSXML::IXMLDOMNodePtr ipValueNode = ipXMLDOMDocument->createElement( 
		gstrFULLTEXT_LEVEL_ATTRIBUTE_NAME.c_str() );
	ASSERT_RESOURCE_ALLOCATION("ELI19172", ipValueNode != NULL);

	// Create a text node with the actual attribute value's text
	// removing any unprintable characters (P16 #1413)
	string strTest = asString(ipValue->String);
	string strValue = removeUnprintableCharacters( strTest );
	MSXML::IXMLDOMNodePtr ipValueNodeText = ipXMLDOMDocument->createTextNode( 
		strValue.c_str() );
	ASSERT_RESOURCE_ALLOCATION("ELI19174", ipValueNodeText != NULL);
	ipValueNode->appendChild( ipValueNodeText );

	// Get access to the XML element interface of the Value node
	MSXML::IXMLDOMElementPtr ipValueElement = ipValueNode;
	ASSERT_RESOURCE_ALLOCATION("ELI19175", ipValueElement != NULL);

	if (!m_bRemoveSpatialInfo && ipValue->HasSpatialInfo() == VARIANT_TRUE)
	{
		// Retrieve Average Character Confidence
		long lMin = 0; 
		long lMax = 0; 
		long lAvg = 0;
		ipValue->GetCharConfidence( &lMin, &lMax, &lAvg );
		string strAvg = asString( lAvg );

		// Add Average Character Confidence - P16 #1680
		_bstr_t _bstrConfAttrName = gstrCONFIDENCE_ATTRIBUTE_NAME.c_str();
		ipValueElement->setAttribute( _bstrConfAttrName, strAvg.c_str() );
	}

	return ipValueElement;
}
//-------------------------------------------------------------------------------------------------
MSXML::IXMLDOMElementPtr XMLVersion2Writer::getLineElement(MSXML::IXMLDOMDocumentPtr ipXMLDOMDocument, 
														   IAttributePtr ipAttribute, 
														   ISpatialStringPtr ipLine, 
														   long nLineNum)
{
	// Line must be spatial
	if (ipLine->HasSpatialInfo() != VARIANT_TRUE)
	{
		UCLIDException ue( "ELI12919", "Line text must be spatial for XML output!");
		throw ue;
	}

	// Create the SpatialLine element
	MSXML::IXMLDOMNodePtr ipLineNode = ipXMLDOMDocument->createElement( 
		gstrFIELDLINE_LEVEL_ATTRIBUTE_NAME.c_str() );
	ASSERT_RESOURCE_ALLOCATION("ELI12899", ipLineNode != NULL);

	// Get access to the XML element interface of the SpatialLine node
	MSXML::IXMLDOMElementPtr ipSpatialElement = ipLineNode;
	ASSERT_RESOURCE_ALLOCATION("ELI12920", ipSpatialElement != NULL);

	// Add PageNumber
	string	strPage = asString( ipLine->GetFirstPageNumber() );
	ipSpatialElement->setAttribute( gstrPAGE_LEVEL_ATTRIBUTE_NAME.c_str(), strPage.c_str() );

	//////////////////////////////
	// Create the LineText element
	//////////////////////////////
	MSXML::IXMLDOMNodePtr ipLineTextNode = ipXMLDOMDocument->createElement( 
		gstrLINETEXT_LEVEL_ATTRIBUTE_NAME.c_str() );
	ASSERT_RESOURCE_ALLOCATION("ELI12900", ipLineTextNode != NULL);

	// Create a text node with the actual line text
	// removing any unprintable characters (P16 #1413)
	string strTest = ipLine->String;
	string strValue = removeUnprintableCharacters( strTest );
	MSXML::IXMLDOMNodePtr ipText = ipXMLDOMDocument->createTextNode( 
		strValue.c_str() );
	ASSERT_RESOURCE_ALLOCATION("ELI12902", ipText != NULL);
	ipLineTextNode->appendChild( ipText );

	// Append the LineText node as a child of the FieldLine node
	ipLineNode->appendChild( ipLineTextNode );

	//////////////////////////////////
	// Get and add Avg Char Confidence
	//////////////////////////////////
	long lMin = 0; 
	long lMax = 0; 
	long lAvg = 0;
	ipLine->GetCharConfidence( &lMin, &lMax, &lAvg );
	string strAvg = asString( lAvg );

	// Get access to the XML element interface of the LineText node
	MSXML::IXMLDOMElementPtr ipElement = ipLineTextNode;
	ASSERT_RESOURCE_ALLOCATION("ELI12901", ipElement != NULL);

	// Add Average Character Confidence - P16 #1680
	_bstr_t _bstrConfAttrName = gstrCONFIDENCE_ATTRIBUTE_NAME.c_str();
	ipElement->setAttribute( _bstrConfAttrName, strAvg.c_str() );

	/////////////////////////////////////
	// Create the SpatialLineZone and SpatialLineBounds elements
	/////////////////////////////////////
	appendSpatialElements(ipLineNode, ipXMLDOMDocument, ipAttribute, ipLine, nLineNum);
	
	return ipLineNode;
}
//-------------------------------------------------------------------------------------------------
MSXML::IXMLDOMElementPtr XMLVersion2Writer::getRectBoundsElement(
	MSXML::IXMLDOMDocumentPtr ipXMLDOMDocument, IRasterZonePtr ipZone, 
	ILongToObjectMapPtr ipPageInfoMap)
{
	ASSERT_ARGUMENT("ELI19205", ipXMLDOMDocument != NULL);
	ASSERT_ARGUMENT("ELI19206", ipZone != NULL);
	ASSERT_ARGUMENT("ELI19869", ipPageInfoMap != NULL);

	// Create the rectangular boundary element
	MSXML::IXMLDOMElementPtr ipRectBoundsElement = ipXMLDOMDocument->
		createElement( gstrBOUNDS_LEVEL_ATTRIBUTE_NAME.c_str() );
	ASSERT_RESOURCE_ALLOCATION("ELI15426", ipRectBoundsElement != NULL);

	ISpatialPageInfoPtr ipPageInfo = ipPageInfoMap->GetValue(ipZone->PageNumber);
	ASSERT_RESOURCE_ALLOCATION("ELI30328", ipPageInfo != NULL);

	// Get the page bounds (for use by GetRectangularBounds)
	ILongRectanglePtr ipPageBounds(CLSID_LongRectangle);
	ASSERT_RESOURCE_ALLOCATION("ELI30314", ipPageBounds != NULL);
	ipPageBounds->SetBounds(0, 0, ipPageInfo->Width, ipPageInfo->Height);

	// Get the rectangular bounds
	ILongRectanglePtr ipBounds = ipZone->GetRectangularBounds(ipPageBounds);
	ASSERT_RESOURCE_ALLOCATION("ELI15427", ipBounds != NULL);
	
	// Set the attributes of the rectangular bounds element
	string strTop = asString(ipBounds->Top);
	string strLeft = asString(ipBounds->Left);
	string strBottom = asString(ipBounds->Bottom);
	string strRight = asString(ipBounds->Right);

	ipRectBoundsElement->setAttribute(_bstr_t("Top"), strTop.c_str());
	ipRectBoundsElement->setAttribute(_bstr_t("Left"), strLeft.c_str());
	ipRectBoundsElement->setAttribute(_bstr_t("Bottom"), strBottom.c_str());
	ipRectBoundsElement->setAttribute(_bstr_t("Right"), strRight.c_str());

	return ipRectBoundsElement;
}
//-------------------------------------------------------------------------------------------------
MSXML::IXMLDOMElementPtr XMLVersion2Writer::getZoneElement(
	MSXML::IXMLDOMDocumentPtr ipXMLDOMDocument, IRasterZonePtr ipZone)
{
	ASSERT_ARGUMENT("ELI19207", ipXMLDOMDocument != NULL);
	ASSERT_ARGUMENT("ELI19208", ipZone != NULL);

	// Create a new zone element
	MSXML::IXMLDOMElementPtr ipZoneElement = ipXMLDOMDocument->
		createElement( gstrZONE_LEVEL_ATTRIBUTE_NAME.c_str() );
	ASSERT_RESOURCE_ALLOCATION("ELI15428", ipZoneElement != NULL);

	// Set the attributes of the zone element				
	string strStartX = asString(ipZone->StartX);
	string strStartY = asString(ipZone->StartY);
	string strEndX = asString(ipZone->EndX);
	string strEndY = asString(ipZone->EndY);
	string strHeight = asString(ipZone->Height);

	ipZoneElement->setAttribute(_bstr_t("StartX"), strStartX.c_str());
	ipZoneElement->setAttribute(_bstr_t("StartY"), strStartY.c_str());
	ipZoneElement->setAttribute(_bstr_t("EndX"), strEndX.c_str());
	ipZoneElement->setAttribute(_bstr_t("EndY"), strEndY.c_str());
	ipZoneElement->setAttribute(_bstr_t("Height"), strHeight.c_str());

	return ipZoneElement;
}
//-------------------------------------------------------------------------------------------------
void XMLVersion2Writer::appendSpatialElements(MSXML::IXMLDOMNodePtr ipLineNode, 
	MSXML::IXMLDOMDocumentPtr ipXMLDOMDocument,	IAttributePtr ipAttribute, 
	ISpatialStringPtr ipLine, long nLineNum)
{
	ASSERT_ARGUMENT("ELI19209", ipLineNode != NULL);
	ASSERT_ARGUMENT("ELI19210", ipXMLDOMDocument != NULL);
	ASSERT_ARGUMENT("ELI19211", ipAttribute != NULL);
	ASSERT_ARGUMENT("ELI19212", ipLine != NULL);

	// get the spatial line's raster zone(s)
	IIUnknownVectorPtr ipZones = ipLine->GetOriginalImageRasterZones();
	ASSERT_RESOURCE_ALLOCATION("ELI15429", ipZones != NULL);

	// get the attribute value
	ISpatialStringPtr ipValue = ipAttribute->Value;
	ASSERT_RESOURCE_ALLOCATION("ELI15430", ipValue != NULL);

	// There should be at least one zone. Verify that.
	long nNumZones = ipZones->Size();
	if (nNumZones < 1)
	{
		UCLIDException ue("ELI15431", "Unexpected number of zones found for line text!");						
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

	// get the spatial page info map
	ILongToObjectMapPtr ipPageInfoMap(ipValue->SpatialPageInfos);
	ASSERT_RESOURCE_ALLOCATION("ELI19870", ipPageInfoMap != NULL);

	// iterate through each zone
	for(long i=0; i<nNumZones; i++)
	{
		// Get the zone
		IRasterZonePtr ipZone = ipZones->At(i);
		ASSERT_RESOURCE_ALLOCATION("ELI15432", ipZone != NULL);

		// append the zone element to the specified line node
		ipLineNode->appendChild( getZoneElement(ipXMLDOMDocument, ipZone) );

		// append the bounds element
		ipLineNode->appendChild( getRectBoundsElement(ipXMLDOMDocument, ipZone, ipPageInfoMap) );
	}
}
//-------------------------------------------------------------------------------------------------
