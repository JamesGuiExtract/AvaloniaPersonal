//============================================================================
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	LMData.h
//
// PURPOSE:	Definition of the LicenseManagement data class LMData
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//============================================================================

#pragma once

#include "COMLMCore.h"
#include "ComponentData.h"
#include <RegConstants.h>
#include <cpputil.h>

#include <string>
#include <map>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// Limits for types of special passwords
//   0 : Regular UCLID passwords
//   1 : Special IcoMap passwords
//   2 : Special Simple Rule Writing passwords
const int giMinPasswordType = 0;
const int giMaxPasswordType = 2;

// Define four UCLID passwords used for interleaved string of 
// encrypted license data
const unsigned long	gulUCLIDKey1 = 0x411065D2;
const unsigned long	gulUCLIDKey2 = 0x7F9D16C2;
const unsigned long	gulUCLIDKey3 = 0x6E5C66AA;
const unsigned long	gulUCLIDKey4 = 0x15D27BA6;

// Define four UCLID passwords used for hard-coded application
// passwords
const unsigned long	gulUCLIDKey5 = 0x17A64E1D;
const unsigned long	gulUCLIDKey6 = 0xDA80339;
const unsigned long	gulUCLIDKey7 = 0x1A0955D7;
const unsigned long	gulUCLIDKey8 = 0x6EE23DA7;
const unsigned long	gulUCLIDOEM  = 0x1F930;

// Define the default folder for license files
const char gpszLicenseFolder[] = 
//	"\\\\rover\\internal\\Common\\Sales\\SBL\\";
	"I:\\Common\\Sales\\SBL\\";

// Define the default CommonComponents folder (UCLID Software)
const char gpszCommonComponentsFolderUCLID[] = 
	"C:\\Program Files\\UCLID Software\\CommonComponents\\";

// Define the default CommonComponents folder (Extract Systems)
const char gpszCommonComponentsFolderExtract[] = 
	"C:\\Program Files\\Extract Systems\\CommonComponents\\";

#ifdef _DEBUG
// Information for Registry items
const string gstrLICENSE_SECTION = "\\COMLicense";
const string gstrTARGETFOLDER_KEY = "TargetFolder";
const string gstrDEFAULT_TARGETFOLDER = "";

// Define the default Utilities Registry folder
const string gstrUTILITIES_FOLDER = gstrREG_ROOT_KEY + "\\Utilities";
#endif

class LMData
{
public:

	//=======================================================================
	// PURPOSE: Constructs an LMData object.  The default state includes 
	//				blank names and an issue date set to the current system
	//				time.  The component map is empty. If names and values
	//				are specified, they will override the defaults
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	strComputerName - the computer name
	//			ulSerialNumber - the disk serial number
	//			strMACAddress - the MAC address
	LMData(const string& strComputerName = "", unsigned long ulSerialNumber = 0,
		const string& strMACAddress = "");


	//=======================================================================
	// PURPOSE: Destroys an LMData object.
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	None
	~LMData();

	//=======================================================================
	// PURPOSE: Adds a ComponentData item to the map.  The object will be
	//				mapped to the specified ID and will be licensed.
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	ulComponentID - ID of component being added
	void addLicensedComponent(unsigned long ulComponentID);

	//=======================================================================
	// PURPOSE: Adds a ComponentData item to the map.  The object will be
	//				mapped to the specified ID and will not be licensed.  The
	//				object's expiration date will be as specified.
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	ulComponentID - ID of component being added
	//				dateExpiration - date (and time) that component expires
	void addUnlicensedComponent(unsigned long ulComponentID, 
		const CTime& dateExpiration);

	//=======================================================================
	// PURPOSE: Converts data into encrypted string of printable characters.
	// REQUIRE: Nothing
	// PROMISE: Returns string of variable length depending on the number of 
	//				components are in the map.
	// ARGS:	bAddDashes - inserts dashes after every four characters
	string compressData(bool bAddDashes=false);

	//=======================================================================
	// PURPOSE: Converts data into encrypted string of printable characters.
	//				Uses specified key values to further encrypt the string 
	//				before returning it to the caller.
	// REQUIRE: Key values cannot be zero.
	// PROMISE: Returns doubly-encrypted string.
	// ARGS:	ulKey1 - first part of encryption password
	//				ulKey2 - second part of encryption password
	//				ulKey3 - third part of encryption password
	//				ulKey4 - fourth part of encryption password
	string compressDataToString(unsigned long ulKey1, unsigned long ulKey2, 
		unsigned long ulKey3, unsigned long ulKey4);

	//=======================================================================
	// PURPOSE: Checks all components for expiration
	// REQUIRE: Nothing
	// PROMISE: Return true if at least one component is not fully licensed
	// ARGS:	None
	bool containsExpiringComponent();

	//=======================================================================
	// PURPOSE: Converts encrypted string into data structure.
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	strLicenseKey - encrypted string of printable characters
	void extractData(string strLicenseKey);

	//=======================================================================
	// PURPOSE: Converts encrypted license information from the specified 
	//				string into data structure.
	// REQUIRE: License string must not be empty.  Key values cannot be zero.
	// PROMISE: Nothing
	// ARGS:	strLicenseString - string containing encrypted license data
	//				ulKey1 - first part of encryption password
	//				ulKey2 - second part of encryption password
	//				ulKey3 - third part of encryption password
	//				ulKey4 - fourth part of encryption password
	void extractDataFromString(const string& strLicenseString, 
		unsigned long ulKey1, unsigned long ulKey2, unsigned long ulKey3, 
		unsigned long ulKey4);

	//=======================================================================
	// PURPOSE: Creates one unsigned long to be used as a password for OEM 
	//				developers to unlock User License restrictions in their 
	//				license file.
	// REQUIRE: ulKey1, ulKey2, ulKey3, and ulKey4 must not be zero
	// PROMISE: ulKey5 = ulKey1 % 42579 + ulKey2 % 33125 + ulKey3 % 84172 + 
	//				ulKey4 % 76182
	// ARGS:	None
	unsigned long generateOEMPassword(unsigned long ulKey1, 
		unsigned long ulKey2, unsigned long ulKey3, unsigned long ulKey4);

	//=======================================================================
	// PURPOSE: Provides the issue date.
	// REQUIRE: Nothing
	// PROMISE: Provides a copy of the issue time
	// ARGS:	None
	CTime& getIssueDate();

	//=======================================================================
	// PURPOSE: Retrieves the count of components in the map.
	// REQUIRE: Nothing
	// PROMISE: Returns the component count.
	// ARGS:	None
	unsigned long getComponentCount();

	//=======================================================================
	// PURPOSE: Retrieves a copy of the license data for the specified 
	//				component.
	// REQUIRE: isComponentFound(ulID) returns true
	// PROMISE: Returns a copy of the component data object.
	// ARGS:	ulID - ID of component whose license data is being requested.
	const ComponentData & getComponentData(unsigned long ulID);

	//=======================================================================
	// PURPOSE: Provides the name of the user's computer or and empty string 
	//			if not yet defined.
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	None
	string getComputerName();

	//=======================================================================
	// PURPOSE: Provides the current version number in string form.
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	None
	string getCurrentVersion();

	//=======================================================================
	// PURPOSE: Retrieves the first component ID in the map.
	// REQUIRE: Nothing
	// PROMISE: Returns ID of first component or -1 if map is empty
	// ARGS:	None
	long getFirstComponentID();

	//=======================================================================
	// PURPOSE: Retrieves the next component ID in the map after lID.
	// REQUIRE: Nothing
	// PROMISE: Returns ID of next component or -1 if map has no more elements
	// ARGS:	lID - ID of current component
	long getNextComponentID(long lID);

	//=======================================================================
	// PURPOSE: Provides the issuer name.
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	None
	string getIssuerName();

	//=======================================================================
	// PURPOSE: Provides the licensee name.
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	None
	string getLicenseeName();

	//=======================================================================
	// PURPOSE: Provides the organization name.
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	None
	string getOrganizationName();

	//=======================================================================
	// PURPOSE: Provides the flag describing use of computer name from the 
	//				User License string.
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	None
	bool getUseComputerName();

	//=======================================================================
	// PURPOSE: Provides the flag describing use of MAC address from the 
	//				User License string.
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	None
	bool getUseMACAddress();

	//=======================================================================
	// PURPOSE: Provides the flag describing use of disk serial number from 
	//				the User License string.
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	None
	bool getUseSerialNumber();

	//=======================================================================
	// PURPOSE: Provides the MAC address from the decrypted User License
	//				string.
	// REQUIRE: setUserString() must be called first
	// PROMISE: Nothing
	// ARGS:	None
	string getUserMACAddress();
	//=======================================================================
	// Returns true if the stored MAC address matches the current MAC address
	inline bool checkUserMACAddress()
	{ return getUserMACAddress() == ::getMACAddress(); }

	//=======================================================================
	// PURPOSE: Provides the computer name from the decrypted User License
	//				string.
	// REQUIRE: setUserString() must be called first
	// PROMISE: Nothing
	// ARGS:	None
	string getUserComputerName();
	//=======================================================================
	// Returns true if the stored computer name matches the current computer name
	inline bool checkUserComputerName()
	{ return getUserComputerName() == ::getComputerName(); }

	//=======================================================================
	// PURPOSE: Provides the disk serial number from the decrypted User 
	//				License string.
	// REQUIRE: setUserString() must be called first
	// PROMISE: Nothing
	// ARGS:	None
	unsigned long getUserSerialNumber();
	//=======================================================================
	// Returns true if the stored disk serial number current disk serial number
	inline bool checkUserSerialNumber()
	{ return getUserSerialNumber() == ::getDiskSerialNumber(); }

	//=======================================================================
	// PURPOSE: Provides the version number from the most recent license 
	//				file.
	// REQUIRE: unzipStringFromFile() must be called first
	// PROMISE: Nothing
	// ARGS:	None
	string getVersion();

	//=======================================================================
	// PURPOSE: Provides the expiration date for a specific component ID
	// REQUIRE: ulID is a temporarily licensed component
	// ARGS:	ulID - The ID of the component to get the expiration date for.
	CTime getExpirationDate(unsigned long ulID);

	//=======================================================================
	// PURPOSE: Checks to see if the specified component is found in the map.
	// REQUIRE: Nothing
	// PROMISE: Returns true if specified component is contained in the map,
	//				otherwise false.
	// ARGS:	ulID - ID of component whose license state is being
	//				questioned
	bool isComponentFound(unsigned long ulID);

	//=======================================================================
	// PURPOSE: Checks to see if the specified component is licensed.
	// REQUIRE: Nothing
	// PROMISE: Returns true if specified component is contained in the map
	//				AND (is licensed OR expiration date has not been passed), 
	//				otherwise false.
	// ARGS:	ulComponentID - ID of component whose license state is being
	//				questioned
	bool isLicensed(unsigned long ulComponentID);

	//=======================================================================
	// PURPOSE: Gets all licensed component codes along with the expiration
	//			time for any components that are temporarily licensed.
	// RETURNS: A vector of pairs where the first element in each pair is the
	//			component code and the second is the time any temporarily
	//			licensed codes expired or 0 for permanently licensed codes.
	vector<pair<unsigned long, CTime>> getLicensedComponents();

	//=======================================================================
	// PURPOSE: Gets all licensed package names using the packages.dat
	//			committed at the time of compile. The list of packages is
	//			determined based on all of the components comprising a
	//			particular package being licensed as opposed to the packages
	//			specifically selected when the license code was generated.	
	// RETURNS: A vector of pairs where the first element in each pair is the
	//			package name and the second is the time any temporarily
	//			licensed packages expire or 0 for permanently licensed packages.
	vector<pair<string, CTime>> getLicensedPackageNames();

	//=======================================================================
	// PURPOSE: Checks to see if the specified licensed component is licensed
	//			temporarily or permanently.
	// REQUIRE: ulComponentID refers to a licensed component.
	// PROMISE: Returns true if the specified components license expires,
	//			false if it is permanently licensed.
	// ARGS:	ulComponentID - ID of component whose license is being
	//			questioned.
	bool isTemporaryLicense(unsigned long ulComponentID);

	//=======================================================================
	// PURPOSE: Checks to see if the first component in the collection
	//			is expired or not
	bool isFirstComponentExpired();

	//=======================================================================
	// PURPOSE: Sets the issue date as specified.
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	Date - desired issue date
	void setIssueDate(const CTime& Date);

	//=======================================================================
	// PURPOSE: Sets the issue date to today (current system time).
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	None
	void setIssueDateToToday();

	//=======================================================================
	// PURPOSE: Sets the issuer name as specified.
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	strName - desired issuer name
	void setIssuerName(const string& strName);

	//=======================================================================
	// PURPOSE: Sets the licensee name as specified.
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	strName - desired licensee name
	void setLicenseeName(const string& strName);

	//=======================================================================
	// PURPOSE: Sets the organization name as specified.
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	strName - desired organization name
	void setOrganizationName(const string& strName);

	//=======================================================================
	// PURPOSE: Sets the flag for use of User License Computer Name.
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	bUseName - True if Computer Name will be checked, False 
	//				otherwise
	void setUseComputerName(bool bUseName);

	//=======================================================================
	// PURPOSE: Sets the flag for use of User License MAC address.
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	bUseAddress - True if MAC address will be checked, False 
	//				otherwise
	void setUseMACAddress(bool bUseAddress);

	//=======================================================================
	// PURPOSE: Sets the User License string data as specified.
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	strData - User License string
	void setUserString(const string& strData);

	//=======================================================================
	// PURPOSE: Gets a user license string from the specified data
	string getUserLicenseString();

	//=======================================================================
	// PURPOSE: Sets the flag for use of User License Disk Serial Number.
	// REQUIRE: Nothing
	// PROMISE: Nothing
	// ARGS:	bUseNumber - True if Disk Serial Number will be checked, 
	//				False otherwise
	void setUseSerialNumber(bool bUseNumber);

	//=======================================================================
	// PURPOSE: Retrieves desired string from the specified file.
	// REQUIRE: Filename cannot be an empty string.  File must exist.
	// PROMISE: Returns the specified string
	// ARGS:	strLicenseFile - file containing encrypted license data
	//				bUserString - True for User String, False for UCLID String
	string unzipStringFromFile(const string& strLicenseFile, bool bUserString);

	//=======================================================================
	// PURPOSE: Converts two doubly-encrypted strings into a single string
	//				alternating characters between strings.  The final string 
	//				is written to the specified file.
	// REQUIRE: The filename cannot be an empty string.  strData1 cannot be 
	//				empty.
	// PROMISE: Returns True if successful, False otherwise.
	// ARGS:	strLicenseFile - output filename
	//				strData1 - first doubly-encrypted string
	//				strData2 - second doubly-encrypted string
	bool zipStringsToFile(const string& strLicenseFile, const string& strVersion, 
		const string& strData1, const string& strData2);

	//=======================================================================
	// PURPOSE: Sets the disabled flag to false for each of the
	//			currently licensed component ID's
	// Added as per [LegacyRCAndUtils #4993]
	void enableAll();

	//=======================================================================
	// PURPOSE: Sets the disabled flag to false for a specific licensed
	//			component ID
	// Added as per [LegacyRCAndUtils #4993]
	void enableId(unsigned long ulComponentID);

	//=======================================================================
	// PURPOSE: Sets the disabled flag to true for each of the
	//			currently licensed component ID's
	// Added as per [LegacyRCAndUtils #4993]
	void disableAll();

	//=======================================================================
	// PURPOSE: Sets the disabled flag to true for a specific licensed
	//			component ID
	// Added as per [LegacyRCAndUtils #4993]
	void disableId(unsigned long ulComponentID);

	//=======================================================================
    // PURPOSE: Unlicenses all components
	// Added to address [FlexIDSCore:5286]
    void unlicenseAll();

	//=======================================================================
    // PURPOSE: Unlicenses the specified component.
    void unlicenseId(unsigned long ulComponentID);

private:

///////////////
// DATA MEMBERS
///////////////
private:

	// Name of person who creates and packages the component data
	string		m_strIssuerName;

	// Name of person to whom the license is sent
	string		m_strLicenseeName;

	// Name of organization/company to whom the license is sent
	string		m_strOrganizationName;

	// Date (and time) that license was created and packaged
	CTime			m_IssueDate;

	// Settings for User License data
	bool			m_bUseComputerName;
	bool			m_bUseMACAddress;
	bool			m_bUseSerialNumber;
	string		m_strUserString;
	string		m_strUserComputerName;
	unsigned long	m_ulUserSerialNumber;
	string		m_strUserMACAddress;
	string		m_strActualComputerName;
	unsigned long	m_ulActualSerialNumber;
	string		m_strActualMACAddress;
	string		m_strVersion;

	// Collection of component IDs and associated license flags and 
	// expiration dates
	map<unsigned long, ComponentData> m_mapCompIDToData;
};
