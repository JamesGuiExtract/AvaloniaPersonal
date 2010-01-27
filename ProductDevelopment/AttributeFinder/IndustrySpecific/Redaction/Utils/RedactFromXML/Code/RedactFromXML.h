// RedactFromXML.h : main header file for the CRedactFromXML application
//

#pragma once

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"

#include <MiscLeadUtils.h>

#include <string>
#include <vector>

using namespace std;

// Redact from XML Metadata application class
class CRedactFromXMLApp : public CWinApp
{
public:
	CRedactFromXMLApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CRedactFromXMLApp)
	public:
	virtual BOOL InitInstance();
	virtual int ExitInstance();
	//}}AFX_VIRTUAL

// Implementation

	//{{AFX_MSG(CRedactFromXMLApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:

	// input image filename from which to create redactions
	string m_strInputFile;

	// IDShield metadata filename to parse for redactions
	string m_strXMLFile;

	//  output redacted image filename 
	string m_strOutputFile;
	
	// whether to redact as annotations (true) or use the default setting (false)
	bool m_bRedactAsAnnotation;

	// whether to burn redactions directly into the image (true) or use the default setting (false)
	bool m_bBurnInRedactions;

	// whether to retain original annotations (true) or use the default setting (false)
	bool m_bRetainAnnotations;

	// whether to discard existing annotation (true) or use the default setting (false)
	bool m_bDiscardAnnotations;

	// whether to create an output only if the document contained redactions (false)
	// or always create an output image (true)
	bool m_bAlwaysOutput;

	// whether or not the app failed to execute (true) or ran successfully (false)
	bool m_bIsError;

	//---------------------------------------------------------------------------------------------
	// PROMISE: Displays usage information in a message box and returns false. strFileName is 
	//          expected to be the filename of the currently running executable.
	static bool displayUsage(const string& strFileName);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Set the member variables based on the command line arguments specified by argc
	//          and argv. Validates all input. Returns true if successful, returns false and 
	//          displays usage information if arguments are not valid.
	bool getArguments(const int argc, char* argv[]);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Gets the attribute named pszAttributeName from ipAttributes and returns its value
	//          as a long, otherwise throws an exception.
	static long getAttributeAsLong(MSXML::IXMLDOMNamedNodeMapPtr ipAttributes, 
		const char* pszAttributeName);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Gets the attribute named pszAttributeName from ipAttributes and returns its value
	//          as a string, otherwise throws an exception.
	static string getAttributeAsString(MSXML::IXMLDOMNamedNodeMapPtr ipAttributes, 
								const char* pszAttributeName);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Gets color from the value of the specified node.
	static COLORREF getColorFromNode(MSXML::IXMLDOMNodePtr ipNode);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Gets a string representing the exemption codes and other text for the specified 
	//          redaction node
	// PARAMS:  ipRedaction - The redaction node
	//          strTextFormat - The format in which redactions appear. May contain tags.
	static string getRedactionTextFromRedactionNode(MSXML::IXMLDOMNodePtr ipRedaction, 
		const string& strTextFormat);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Gets the font and point size from the value of the specified node.
	static LOGFONT getFontFromNode(MSXML::IXMLDOMNodePtr ipNode, int& riPointSize);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Gets the first node from the specified list with the specified name or NULL if
	//          no such node exists.
	static MSXML::IXMLDOMNodePtr getFirstNodeNamed(MSXML::IXMLDOMNodeListPtr ipNode, 
		const string& strName);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Gets a raster zone to use as a prototype for all other raster zones in the 
	//          specified node.
	// PROMISE: Returns a raster zone initialized with redaction appearance settings and the 
	//          redaction text format.
	static PageRasterZone getRasterZonePrototypeFromNode(MSXML::IXMLDOMElementPtr ipNode,
		string &rstrTextFormat);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Creates a vector of PageRasterZones corresponding to the redactions of the 
	//          specified node.
	static vector<PageRasterZone> getRasterZonesFromNode(MSXML::IXMLDOMElementPtr ipNode, 
		PageRasterZone& prototype, const string& strTextFormat);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Gets the page rastse zone from the specified xml node.
	static PageRasterZone getRasterZoneFromXML(MSXML::IXMLDOMNodePtr ipZone, 
		PageRasterZone& prototype);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Returns a vector of raster zones corresponding to all the output redactions in the 
	//          IDShield xml metadata file specified.
	vector<PageRasterZone> getRasterZonesFromXML(const string& strXMLFile);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Gets the last redaction session node from the root node of an xml file.
	static MSXML::IXMLDOMElementPtr getLastRedactionSessionNode(MSXML::IXMLDOMElementPtr ipRoot);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Updates the annotation settings from the specified automated redaction session iff 
	// a command line setting has not been specified.
	void updateAnnotationSettings(MSXML::IXMLDOMElementPtr ipSession);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Returns true if one of the specified attributes are marked as enabled; false otherwise.
	static bool isRedactionEnabled(MSXML::IXMLDOMNamedNodeMapPtr ipAttributes);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Throws an exception if this EXE is not licensed, completes successfully otherwise.
	static void validateLicense();
	//---------------------------------------------------------------------------------------------
};