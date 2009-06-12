#pragma once

#include <map>
#include <string>
#include <vector>

using namespace std;

// Represents an exemption code's summary and description.
struct ExemptionCodeInfo
{
	// The exemption code
	string m_strCode;
	
	// A summary of the exemption in a few words
	string m_strSummary;

	// A detailed description of the exemption code
	string m_strDescription;
};

// Represents data associated with exemption code categories
struct ExemptionCategoryInfo
{
	// The abbreviated name of the category
	string m_abbreviation;

	// The exemption codes in this category
	vector<ExemptionCodeInfo> m_vecCodes;
};

// Represents the master list of exemption codes.
class MasterExemptionCodeList
{
public:

	// Reads the master exemption code list from the xml files in the specified directory
	MasterExemptionCodeList(const string& strExemptionDirectory);

	// Copy constructor
	MasterExemptionCodeList(const MasterExemptionCodeList& exemptionCodes);

	// Gets the full name of all the exemption categories
	void getCategoryNames(vector<string> &rvecCategories) const;

	// Gets all the exemption codes in the specified category
	void getCodesInCategory(vector<ExemptionCodeInfo> &rvecCodes, 
		const string& strCategory) const;

	// Gets the description of the specified exemption code
	string getDescription(const string& strCategory, const string& strCode) const;

	// Gets the abbreviated name of the specified category
	// Returns "" if category isn't found.
	string getCategoryAbbreviation(const string& strCategory) const;

	// Gets the full name of the specified abbreviated category. 
	// Returns "" if category isn't found.
	string getFullCategoryName(const string& strAbbreviation) const;

private:

	// Gets a vector of exemption codes from the specified xml file
	void addCategoryCodesFromXml(const string& strXmlFile);

	// Retrieves the text of the specified attribute as a string
	string getAttributeAsString(MSXML::IXMLDOMNamedNodeMapPtr ipAttributes, 
		const char* pszAttributeName) const;

	// Maps exemption code categories to vectors of exemption codes
	map<string, ExemptionCategoryInfo> m_categoryToCodes;
};