
#pragma once

#include "StdAfx.h"
#include "XMLFileWriter.h"

#include <string>
using namespace std;

class XMLVersion1Writer : public XMLFileWriter
{
public:
	XMLVersion1Writer();

	void WriteFile(const string& strFile, IIUnknownVector *pAttributes);

private:

	// helper functions
	MSXML::IXMLDOMElementPtr getValueElement(MSXML::IXMLDOMDocumentPtr ipXMLDOMDocument, 
		IAttributePtr ipAttribute);

	MSXML::IXMLDOMElementPtr getLineElement(MSXML::IXMLDOMDocumentPtr ipXMLDOMDocument, 
		IAttributePtr ipAttribute, ISpatialStringPtr ipLine, long nLineNum);
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
	void appendSpatialElements(MSXML::IXMLDOMNodePtr ipLineElement, 
		MSXML::IXMLDOMDocumentPtr ipXMLDOMDocument,	IAttributePtr ipAttribute, 
		ISpatialStringPtr ipLine, long nLineNum);

	void addNodesForAttributes(MSXML::IXMLDOMDocumentPtr ipXMLDOMDocument,
		MSXML::IXMLDOMNodePtr ipNode, IIUnknownVectorPtr ipAttributes);
};
