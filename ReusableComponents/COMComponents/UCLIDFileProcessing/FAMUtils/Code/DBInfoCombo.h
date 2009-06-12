#pragma once

#include "FAMUtils.h"

#include <Win32Util.h>

#include <string>
#include <vector>

using namespace std;

class FAMUTILS_API DBInfoCombo : public CComboBox
{
public:
	// Enum for the type of info displayed in combo box list
	enum EDBInfoType {
		kServerName,
		kDatabaseName
	};

	DBInfoCombo(EDBInfoType eDBInfoType);
	
	// sets the SQL server if the type is set to kDatabaseName 
	void setSQLServer(const std::string strServer);

	// Returns the EDBInfoType for the combo box
	EDBInfoType getDBInfoType();

protected:
	DECLARE_MESSAGE_MAP()
	
	afx_msg void OnCbnSelendok();
	afx_msg void OnCbnDropdown();

private:

	// Contains the type of info combo box list contains
	EDBInfoType m_eDBInfoType;

	// Used for the server if the combo box type is kDatabaseName
	string m_strServer;

	// Stores the current running platform
	PLATFORM m_winPlatform;

	// Gets a list of installed SQL instances from the registry and adds (local)\<instance> to
	// the list box.  If the instance is named MSSQL
	void addLocalInstances();
};
