#pragma once

#include <string>

class DataConfidenceLevel
{
public:
	DataConfidenceLevel(std::string strINIFile);

	// Applies default settings for manual data
	// and uses lColor
	void		applyManualSettings(long lColor);

	// Gets the Long Name
	std::string	getLongName();

	// Gets the Short Name
	std::string getShortName();

	// Gets the Description
	std::string getDescription();

	// Gets the Display Color
	long		getDisplayColor();

	// Gets the Query
	std::string getQuery();

	// Returns the Display flag
	bool		isMarkedForDisplay();

	// Returns the Verify flag
	bool		isMarkedForVerify();

	// Returns the Output flag
	bool		isMarkedForOutput();

	// Returns Redacted->NonRedacted warning flag
	bool		isNonRedactWarningEnabled();

	// Returns NonRedacted->Redacted warning flag
	bool		isRedactWarningEnabled();

	// Reads the specified section of the INI file
	void		readSettings(std::string strFolder);

	// Sets Redacted->NonRedacted warning flag
	void		setWarnIfNoRedact(bool bWarning);

	// Sets NonRedacted->Redacted warning flag
	void		setWarnIfRedact(bool bWarning);

private:

	//////////
	// Methods
	//////////

	// Sets the Description
	void setDescription(const std::string& strDescription);

	// Sets the Display Color
	void setDisplayColor(const std::string& strColor);

	// Sets the Long Name
	void setLongName(const std::string& strLongName);

	// Sets the Display flag
	void setMarkedForDisplay(bool bMarked);

	// Sets the Output flag
	void setMarkedForOutput(bool bMarked);

	// Sets the Verify flag
	void setMarkedForVerify(bool bMarked);

	// Sets the Query
	void setQuery(const std::string& strQuery);

	// Gets specified setting from previously specified (m_strFolder) 
	// section of INI file.  Returns empty string if not found.
	// Throws exception if not found AND bRequired = true
	std::string	getSetting(std::string strKey, bool bRequired);

	// Sets the Short Name
	void setShortName(const std::string& strShortName);

	///////////////
	// Data Members
	///////////////

	std::string m_strINIFile;
	std::string m_strFolder;

	std::string m_strLongName;
	std::string m_strShortName;
	std::string m_strDescription;
	std::string m_strQuery;

	long		m_lDisplayColor;

	bool		m_bMarkedForDisplay;
	bool		m_bMarkedForVerify;
	bool		m_bMarkedForOutput;

	// Warn user if normally redacted item is changed to non-redacted
	bool		m_bWarnIfNoRedact;

	// Warn user if normally non-redacted item is changed to redacted
	bool		m_bWarnIfRedact;
};
