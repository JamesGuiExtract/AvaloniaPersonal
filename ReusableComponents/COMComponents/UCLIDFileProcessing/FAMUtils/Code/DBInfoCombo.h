#pragma once

#include "FAMUtils.h"

#include <Win32Util.h>

#include <string>
#include <vector>
#include <set>

using namespace std;

class FAMUTILS_API DBInfoCombo : public CComboBox
{
public:
	// Enum for the type of info displayed in combo box list
	enum EDBInfoType {
		kServerName,
		kDatabaseName,
		kCustomList
	};

	DBInfoCombo(EDBInfoType eDBInfoType);
	
	// sets the SQL server if the type is set to kDatabaseName 
	void setSQLServer(const std::string strServer);

	// Returns the EDBInfoType for the combo box
	EDBInfoType getDBInfoType();
	
	// Allows a special value to be added to the options in the dropdown in addition to the items
	// that would normally appear.
	void showSpecialValue(const string& strValue);

	// Converts the combo to a read-only (CBS_DROPDOWNLIST style). Unlike the default editable
	// control, The combo's values will not be sorted.
	void convertToDropDownList();

	// Sets the list of items when kCustomList is being used.
	void setList(const vector<string>& vecList, const string& strDefaultValue);

protected:
	DECLARE_MESSAGE_MAP()
	
	afx_msg void OnCbnSelendok();
	afx_msg void OnCbnDropdown();

private:

	// Contains the type of info combo box list contains
	EDBInfoType m_eDBInfoType;

	// Used for the server if the combo box type is kDatabaseName
	string m_strServer;

	// Stores whether current OS is >= Vista
	bool m_bVistaOrLater;

	// A set of all the special values to be displayed in the dropdown.
	set<string> m_setSpecialValues;

	// When using kCustomList, the default value to select in the combo.
	string m_strDefaultValue;

	// Gets a list of installed SQL instances from the registry and adds (local)\<instance> to
	// the list box.  If the instance is named MSSQL
	void addLocalInstances();
};
