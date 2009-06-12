#pragma once

#include <map>
#include <set>

using namespace std;

// Represents a list of exemption codes.
class ExemptionCodeList
{
public:

	// Constructor
	ExemptionCodeList();

	// Gets/sets the category of exemption codes in the list
	string getCategory() const;
	void setCategory(const string& strCategory);

	// true if the exemption code list has a category specified, false if it doesn't
	bool hasCategory() const;

	// Adds the specified exemption code to the list
	void addCode(const string& strCode);

	// true if the exemption code list has the specified code, false if it doesn't
	bool hasCode(const string& strCode) const;

	// Gets/sets the other text associated with the exemption codes
	// This may be an additional exemption code or a reason the exemption codes are applied
	string getOtherText() const;
	void setOtherText(const string& strText);

	// Returns the exemption codes and other text as a comma separated string
	string getAsString() const;

	// Returns true if the list has no exemption codes and no 'other text'; false otherwise
	bool isEmpty() const;

	// Equality operator
	friend bool operator==(const ExemptionCodeList &left, const ExemptionCodeList &right);
	friend bool operator!=(const ExemptionCodeList &left, const ExemptionCodeList &right);
	
private:

	// The exemption code category
	string m_strCategory;

	// The exemption codes applied
	set<string> m_setCodes;

	// Other text associated with the list of exemption codes
	string m_strText;
};