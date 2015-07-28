
#pragma once

#include "StdAfx.h"
#include "XMLFileWriter.h"

#include <string>
using namespace std;

class XMLVersion2Writer : public XMLFileWriter
{
public:
	XMLVersion2Writer(bool bRemoveSpatialInfo = false);

	void UseNamedAttributes(bool bUseNamedAttributes);

	void UseSchemaName(const string& strSchemaName);
	
	void ValueAsFullText(bool bValueAsFullText);

	void RemoveEmptyNodes(bool bRemoveEmpytNodes);

	void WriteFile(const string& strFile, IIUnknownVector *pAttributes);

private:
	// Data members
	bool m_bUseNamedAttributes;

	string m_strSchemaName;

	bool m_bValueAsFullText;

	bool m_bRemoveEmptyNodes;

	// If true then spatial info will not be written out when the XML is written
	bool m_bRemoveSpatialInfo;

	// helper functions
	MSXML::IXMLDOMElementPtr getValueElement(MSXML::IXMLDOMDocumentPtr ipXMLDOMDocument, 
		ISpatialStringPtr ipValue);

	MSXML::IXMLDOMElementPtr getLineElement(MSXML::IXMLDOMDocumentPtr ipXMLDOMDocument, 
		IAttributePtr ipAttribute, ISpatialStringPtr ipLine, long nLineNum);

	// Adds value and AverageCharConfidence of the ipValue to the given element
	void addValueToElement(MSXML::IXMLDOMElementPtr ipElement, ISpatialStringPtr ipValue);

	//---------------------------------------------------------------------------------------------
	// PROMISE: Returns a valid xml DOM element created from ipXMLDOMDocument containing as 
	//          attributes the simplified rectangular bounds of ipZone.
	MSXML::IXMLDOMElementPtr getRectBoundsElement(MSXML::IXMLDOMDocumentPtr ipXMLDOMDocument, 
		IRasterZonePtr ipZone, ILongToObjectMapPtr ipPageInfoMap);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Returns a valid xml DOM element created from ipXMLDOMDocument containing as 
	//          attributes the precise spatial information of the specified raster zone.
	MSXML::IXMLDOMElementPtr getZoneElement(MSXML::IXMLDOMDocumentPtr ipXMLDOMDocument, 
		IRasterZonePtr ipZone);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Appends the appropriate spatial information of ipLine to ipLineNode, a node in 
	//          ipXMLDOMDocument. ipAttribute and nLineNum are used to provide detailed error info.
	// PROMISE: Appends a rectangular bounds element and zone element for each raster zone in ipLine.
	void appendSpatialElements(MSXML::IXMLDOMNodePtr ipLineNode, 
		MSXML::IXMLDOMDocumentPtr ipXMLDOMDocument,	IAttributePtr ipAttribute, 
		ISpatialStringPtr ipLine, long nLineNum);
	
	void addNodesForAttributes(MSXML::IXMLDOMDocumentPtr ipXMLDOMDocument,
		MSXML::IXMLDOMNodePtr ipNode, IIUnknownVectorPtr ipAttributes);

	// Checks the attribute to see if it is empty
	// it is empty if the value is empty and there are no sub attributes
	bool isAttributeEmpty(IAttributePtr ipAttribute);
};
