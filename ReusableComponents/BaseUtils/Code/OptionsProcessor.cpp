//==================================================================================================
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	OptionsProcessor.cpp
//
// PURPOSE:	Provides support for standardized options file processing
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//==================================================================================================

#include "stdafx.h"
#include "OptionsProcessor.h"
#include "UCLIDException.h"

/////////////////////////////////////////////////////////////////////////////
OptionsProcessor::OptionsProcessor(const char *pszDelimiter)
{
	// Store the delimiter string
	m_strDelimiter = pszDelimiter;
}

/////////////////////////////////////////////////////////////////////////////
void OptionsProcessor::addOption(const string& strOptionName, 
								 const string& strDefaultValue)
{
	// Check for empty option name string
	if (strOptionName.length() > 0)
	{
		// Add the item to the map
		m_mapOptionNameToValue[strOptionName] = strDefaultValue;
	}
}

/////////////////////////////////////////////////////////////////////////////
bool OptionsProcessor::getBooleanOptionValue(const string& strOptionName)
{
	// Retrieve the string
	string	strValue = m_mapOptionNameToValue[strOptionName];

	// Check for option name presence
	if (strValue.length() == 0)
	{
		// Throw exception - option not found
		UCLIDException ue( "ELI03537", "Option was not found." );
		ue.addDebugInfo( "Option Name", strOptionName );
		throw ue;
	}

	// Check for false
	if ((strValue == "0") || (strValue == "F") || (strValue == "false") || 
		(strValue == "off"))
	{
		return false;
	}
	// Check for true
	else if ((strValue == "1") || (strValue == "T") || (strValue == "true") || 
		(strValue == "on"))
	{
		return true;
	}
	// Value string is not recognized as Boolean
	else
	{
		// Throw exception - option is not Boolean
		UCLIDException ue( "ELI03538", "Option is not Boolean." );
		ue.addDebugInfo( "Option Name", strOptionName );
		throw ue;
	}
}

/////////////////////////////////////////////////////////////////////////////
const string& OptionsProcessor::getStringOptionValue(const string& strOptionName)
{
	// Check for option name presence
	if (m_mapOptionNameToValue[strOptionName].length() == 0)
	{
		// Throw exception - option not found
		UCLIDException ue( "ELI03539", "Option was not found." );
		ue.addDebugInfo( "Option Name", strOptionName );
		throw ue;
	}

	return m_mapOptionNameToValue[strOptionName];
}

/////////////////////////////////////////////////////////////////////////////
bool OptionsProcessor::isValidOptionString(const string& strInput)
{
	// Check the string for a delimiter
	long lPos = strInput.find( m_strDelimiter );
	if (lPos <= 0)
	{
		// Delimiter not found or at begining of string
		return false;
	}
	else
	{
		// Delimiter found
		return true;
	}
}

/////////////////////////////////////////////////////////////////////////////
void OptionsProcessor::processOptionString(const string& strInput)
{
	// First check validity of the input string
	if (!isValidOptionString( strInput ))
	{
		// Throw exception - Invalid option string
		UCLIDException ue( "ELI03540", "Invalid option." );
		ue.addDebugInfo( "Processed string", strInput );
		ue.addDebugInfo( "Delimiter", m_strDelimiter );
		throw ue;
	}
	else
	{
		// Locate the delimiter
		long lPos = strInput.find( m_strDelimiter );

		// Retrieve option name
		string	strName = strInput.substr( 0, lPos );

		// Retrieve option value
		long lStart = lPos + m_strDelimiter.length();
		string	strValue = strInput.substr( lStart, strInput.length() - lStart );

		// Add the entry to the map
		m_mapOptionNameToValue[strName] = strValue;
	}
}

/////////////////////////////////////////////////////////////////////////////
