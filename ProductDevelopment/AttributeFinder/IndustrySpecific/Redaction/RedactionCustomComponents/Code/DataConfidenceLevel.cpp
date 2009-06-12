#include "stdafx.h"
#include "DataConfidenceLevel.h"
#include "Settings.h"

#include <cpputil.h>
#include <UCLIDException.h>
#include <INIFilePersistenceMgr.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

using namespace std;

//-------------------------------------------------------------------------------------------------
// DataConfidenceLevel
//-------------------------------------------------------------------------------------------------
DataConfidenceLevel::DataConfidenceLevel(std::string strINIFile)
  : m_strINIFile(strINIFile),
    m_lDisplayColor(-1),
    m_bMarkedForDisplay(false),
    m_bMarkedForVerify(false),
	m_bMarkedForOutput(false),
	m_bWarnIfNoRedact(false),
	m_bWarnIfRedact(false)
{
}
//-------------------------------------------------------------------------------------------------
void DataConfidenceLevel::applyManualSettings(long lColor)
{
	// Set the Short Name
	m_strShortName = "Man";

	// Set the Long Name
	m_strLongName = "Manual Redactions";

	// Clear the Query
	m_strQuery = "";

	// Set the color
	m_lDisplayColor = lColor;

	// Set for Display, Output and Verify
	m_bMarkedForDisplay = true;
	m_bMarkedForOutput = true;
	m_bMarkedForVerify = true;
}
//-------------------------------------------------------------------------------------------------
long DataConfidenceLevel::getDisplayColor(void)
{
	return m_lDisplayColor;
}
//-------------------------------------------------------------------------------------------------
string DataConfidenceLevel::getDescription(void)
{
	return m_strDescription;
}
//-------------------------------------------------------------------------------------------------
string DataConfidenceLevel::getLongName(void)
{
	return m_strLongName;
}
//-------------------------------------------------------------------------------------------------
string DataConfidenceLevel::getQuery(void)
{
	return m_strQuery;
}
//-------------------------------------------------------------------------------------------------
string DataConfidenceLevel::getShortName(void)
{
	return m_strShortName;
}
//-------------------------------------------------------------------------------------------------
bool DataConfidenceLevel::isMarkedForDisplay()
{
	return m_bMarkedForDisplay;
}
//-------------------------------------------------------------------------------------------------
bool DataConfidenceLevel::isMarkedForOutput()
{
	return m_bMarkedForOutput;
}
//-------------------------------------------------------------------------------------------------
bool DataConfidenceLevel::isMarkedForVerify()
{
	return m_bMarkedForVerify;
}
//-------------------------------------------------------------------------------------------------
bool DataConfidenceLevel::isNonRedactWarningEnabled()
{
	return m_bWarnIfNoRedact;
}
//-------------------------------------------------------------------------------------------------
bool DataConfidenceLevel::isRedactWarningEnabled()
{
	return m_bWarnIfRedact;
}
//-------------------------------------------------------------------------------------------------
void DataConfidenceLevel::readSettings(std::string strFolder)
{
	// Save folder
	m_strFolder = strFolder;

	// Retrieve the Short Name
	m_strShortName = getSetting( gstrSHORT_NAME, true );

	// Retrieve the Long Name (not required)
	m_strLongName = getSetting( gstrLONG_NAME, false );

	// Retrieve the Query
	m_strQuery = getSetting( gstrQUERY, true );

	// Retrieve the Display setting
	string strTemp = getSetting( gstrDISPLAY, true );
	m_bMarkedForDisplay = (strTemp == "1");

	// Retrieve this Color, convert to Long, add to collection
	// Color is required only if m_bMarkedForDisplay = true
	strTemp = getSetting( gstrDISPLAY_COLOR, m_bMarkedForDisplay );
	if (strTemp.length() > 0)
	{
		m_lDisplayColor = getRGBFromString( strTemp, '.' );
	}

	// Retrieve this Output setting
	strTemp = getSetting( gstrOUTPUT, true );
	m_bMarkedForOutput = (strTemp == "1");

	// Retrieve this Verify setting
	strTemp = getSetting( gstrVERIFY, true );
	m_bMarkedForVerify = (strTemp == "1");

	// Check Redact->NonRedact warning
	strTemp = getSetting( gstrWARN_IF_TOGGLE_OFF, false );
	if (strTemp.length() > 0)
	{
		m_bWarnIfNoRedact = (strTemp == "1");
	}

	// Check NonRedact->Redact warning
	strTemp = getSetting( gstrWARN_IF_TOGGLE_ON, false );
	if (strTemp.length() > 0)
	{
		m_bWarnIfRedact = (strTemp == "1");
	}
}
//-------------------------------------------------------------------------------------------------
void DataConfidenceLevel::setWarnIfNoRedact(bool bWarning)
{
	m_bWarnIfNoRedact = bWarning;
}
//-------------------------------------------------------------------------------------------------
void DataConfidenceLevel::setWarnIfRedact(bool bWarning)
{
	m_bWarnIfRedact = bWarning;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
std::string	DataConfidenceLevel::getSetting(std::string strKey, bool bRequired)
{
	// Create temporary PersistenceMgr
	INIFilePersistenceMgr	mgrSettings( m_strINIFile );

	// Create folder name from strSection
	string strFolder = m_strINIFile;
	strFolder += "\\";
	strFolder += m_strFolder.c_str();

	// Retrieve the value
	string strValue = mgrSettings.getKeyValue( strFolder, strKey );

	// Check result
	if (strValue.size() == 0 && bRequired)
	{
		UCLIDException ue( "ELI11228", "Required setting not defined." );
		ue.addDebugInfo( "Required Setting", strKey );
		ue.addDebugInfo( "Section Name", m_strFolder );
		throw ue;
	}

	return strValue;
}
//-------------------------------------------------------------------------------------------------
void DataConfidenceLevel::setDescription(const string& strDescription)
{
	m_strDescription = strDescription;
}
//-------------------------------------------------------------------------------------------------
void DataConfidenceLevel::setDisplayColor(const string& strColor)
{
	// Convert "R.G.B" into long
	m_lDisplayColor = getRGBFromString( strColor, '.' );
}
//-------------------------------------------------------------------------------------------------
void DataConfidenceLevel::setLongName(const string& strLongName)
{
	m_strLongName = strLongName;
}
//-------------------------------------------------------------------------------------------------
void DataConfidenceLevel::setMarkedForDisplay(bool bMarked)
{
	m_bMarkedForDisplay = bMarked;
}
//-------------------------------------------------------------------------------------------------
void DataConfidenceLevel::setMarkedForOutput(bool bMarked)
{
	m_bMarkedForOutput = bMarked;
}
//-------------------------------------------------------------------------------------------------
void DataConfidenceLevel::setMarkedForVerify(bool bMarked)
{
	m_bMarkedForVerify = bMarked;
}
//-------------------------------------------------------------------------------------------------
void DataConfidenceLevel::setQuery(const string& strQuery)
{
	m_strQuery = strQuery;
}
//-------------------------------------------------------------------------------------------------
void DataConfidenceLevel::setShortName(const string& strShortName)
{
	m_strShortName = strShortName;
}
//-------------------------------------------------------------------------------------------------
