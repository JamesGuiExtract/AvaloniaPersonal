#pragma once

#include <string>
#include <vector>

using namespace std;

typedef enum ERedactChoice
{
	kRedactYes,
	kRedactNo,
	kRedactNA,
}	ERedactChoice;

class DataDisplaySettings
{
public:
	DataDisplaySettings(string strText, string strCategory, string strType, 
		int iPage, ERedactChoice eChoice, bool bWarnNoRedact, bool bWarnRedact);

	~DataDisplaySettings();

	// Gets and sets the collection of Highlight IDs
	vector<long>	getHighlightIDs();
	void				setHighlightIDs(vector<long> vecIDs);

	// Gets the Category - [p16 #2379]
	string		getCategory();

	// Gets the exemption codes
	string getExemptionCodes();
	void setExemptionCodes(const string& strCodes);

	// gets the type - NOTE: redefinition of type as per [p16 #2379] - JDS 12/12/2007
	string		getType();

	// Gets the Page Number
	int				getPageNumber();

	// Gets the Redact Choice
	ERedactChoice	getRedactChoice();

	// Gets the Text
	string		getText();

	// Changes the Redact Choice to kRedactNo if m_eRedactChoice == kRedactYes, 
	// otherwise to kRedactYes.
	void			toggleRedactChoice(HWND hWndParent);

	// this will return true if this item has already been reviewed
	bool			getReviewed();

	// this will set the reviewed state to true.  It begins as false;
	void			setReviewed();

	// this will set the type string
	// [p16 #2722]
	void			setType(const string& strNewType);

private:

	//////////
	// Methods
	//////////

	///////////////
	// Data Members
	///////////////

	// Text for Text column (from external IAttribute.String.Value)
	string			m_strText;

	// added as per [p16 #2379] - JDS 12/12/2007
	// Text for Category column (from external DataConfidenceLevel.m_strShortName)
	string			m_strCategory;

	// Exemption codes
	string m_strExemptionCodes;

	// redefined as per [p16 #2379] - JDS 12/12/2007
	// test for the type column (from the attribute - SSN, CCNum, etc...)
	string			m_strType;

	// Value for Page column = first page number from Spatial String
	int					m_iPageNumber;

	// Indicates which icon should be displayed in Redact column
	// Original value is from external DataConfidenceLevel.m_bMarkedForVerify
	// where: true  --> kRedactYes
	//        false --> kRedactNA
	ERedactChoice		m_eRedactChoice;

	// IDs of highlights within Spot Recognition Window
	vector<long>	m_vecHighlightIDs;

	bool m_bReviewed;

	// Warn user if redaction toggled OFF
	bool	m_bWarnNoRedact;
	// Warn user if redaction toggled ON
	bool	m_bWarnRedact;
};
