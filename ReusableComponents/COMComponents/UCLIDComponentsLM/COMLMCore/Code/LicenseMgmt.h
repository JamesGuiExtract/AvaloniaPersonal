//============================================================================
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	LicenseMgmt.h
//
// PURPOSE:	Definition of the LicenseManagement class
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//============================================================================

#pragma once

#include "COMLMCore.h"
#include "LMData.h"
#include "TimeRollbackPreventer.h"

#include <Win32Event.h>
#include <UCLIDException.h>

#include <afxmt.h>
#include <string>
#include <cmath>
#include <cstdlib>

using namespace std;

// Filename for IcoMap Components license file where
// all components except IcoMapApp will be fully licensed and not locked 
// to any machine.
// NOTE: This file has a unique set of passwords
const string g_strIcoMapComponentsLicenseName = "extractsystems_icomap.esl";

//--------------------------------------------------------------------------------------------------
// LicenseManagement class
//--------------------------------------------------------------------------------------------------
class EXPORT_LM LicenseManagement
{
public:
    friend class LicenseCounter;

    //=======================================================================
    // PURPOSE: Query if the license state is corrupt
    // REQUIRE: Nothing
    // PROMISE: Return true if the license state is invalid(corrupt)
    // ARGS:	None
    static bool getBadState();

    //=======================================================================
    // PURPOSE: Initializes a license information object from a license file 
    //				and its associated passwords.  Adds the object to a 
    //				vector.
    // REQUIRE: Filename cannot be empty.  File must exist.  Passwords 
    //				cannot be zero.
    // PROMISE: Nothing
    // ARGS:	lKey - OEM password
    static void ignoreLockConstraints(long lKey);

    //=======================================================================
    // PURPOSE: Initializes a license information object from a license file 
    //				using UCLID-specific passwords.  Adds the object to a 
    //				vector.
    // REQUIRE: Filename cannot be empty.  File must exist.
    // PROMISE: Nothing
    // ARGS:	strLicenseFile - File containing doubly-encrypted license 
    //				information
    //			strValue - The encrypted day code string
    //			iType - type of passwords to be used in the UCLID String
    //			   0 = Regular UCLID passwords
    static void initializeLicenseFromFile(const string& strLicenseFile, const string& strValue,
        int iType = 0);

    //=======================================================================
    // PURPOSE: Initializes a license information object from a license file 
    //				and its associated passwords.  Adds the object to a 
    //				vector.
    // REQUIRE: Filename cannot be empty.  File must exist.  Passwords 
    //				cannot be zero.
    // PROMISE: Nothing
    // ARGS:	strLicenseFile - File containing doubly-encrypted license 
    //				information
    //			ulKey1 - first part of encryption password
    //			ulKey2 - second part of encryption password
    //			ulKey3 - third part of encryption password
    //			ulKey4 - fourth part of encryption password
    //			bUserString - use User String portion from file, 
    //			    otherwise use UCLID String from file
    static void initializeLicenseFromFile(const string& strLicenseFile, 
        unsigned long ulKey1, unsigned long ulKey2, unsigned long ulKey3, 
        unsigned long ulKey4, bool bUserString);

    //=======================================================================
    // PURPOSE: Checks to see if the specified component is licensed.
    // REQUIRE: License information data structure has been initialized.
    // PROMISE: Returns true if specified component is contained in the 
    //				vector AND (is licensed OR expiration date has not been 
    //				passed), otherwise false.
    // ARGS:	ulComponentID - ID of component whose license state is being
    //				questioned
    static bool isLicensed(unsigned long ulComponentID);

    //=======================================================================
    // PURPOSE: Checks to see if the specified licensed component is
    //			temporarily licensed.
    // REQUIRE: ulComponentID is a licensed component and License information
    //			structure has been initialized
    // PROMISE: Returns true if the specified component is temporarily licensed
    //			and returns false if it is permanently licensed.
    static bool isTemporaryLicense(unsigned long ulComponentID);

    //=======================================================================
    // PURPOSE: Returns the expiration date of the specified component ID
    // REQUIRE: ulComponentID is a temporarily licensed component
    // PROMISE: Returns the expiration date of the specified component
    static CTime getExpirationDate(unsigned long ulComponentID);

    //=======================================================================
    // PURPOSE: Initializes a license information object from all license 
    //				files in a specified directory.  Each file will be 
    //				decrypted using UCLID-specific passwords.  Adds the 
    //				objects to a vector.
    // REQUIRE: Nothing
    // PROMISE: The Extract license file folder will be searched.
    // ARGS:	strValue - The encrypted day code string
    //			iType - type of passwords to be used in the UCLID String
    //			   0 = Regular UCLID passwords
    static void loadLicenseFilesFromFolder(const string& strValue, int iType = 0);

    //=======================================================================
    // PURPOSE: Initializes a license information object from all license 
    //				files in a specified directory.  Each file will be 
    //				decrypted using UCLID-specific passwords.  Adds the 
    //				objects to a vector.
    // REQUIRE: Directory name cannot be empty.  Directory must exist.
    // PROMISE: Nothing
    // ARGS:	strDirectory - Folder containing zero or more license files
    //			strValue - The encrypted day code string
    //			iType - type of passwords to be used in the UCLID String
    //			   0 = Regular UCLID passwords
    static void loadLicenseFilesFromFolder(string strDirectory, const string& strValue,
        int iType = 0);

    //=======================================================================
    // PURPOSE: This function checks for licensing of PDF Functionality
    static bool isPDFLicensed();

	//=======================================================================
	// PURPOSE: This function returns true if PDF Read licensed, if PDF Read-Write
	// is licensed and PDF Read is not it will return false
	static bool isPDFReadLicensed();

    //=======================================================================
    // PURPOSE: This function checks the extension of the filename and throws
    //			an UCLIDException if the file type is not licensed for Read Write
    static void verifyFileTypeLicensedRW(string strFileName );
	
	//=======================================================================
	// PURPOSE: This function checks the extension of the filename and throws
	//			an UCLIDException if the file type is not licensed for Read Only
	static void verifyFileTypeLicensedRO(string strFileName);
    
	//=======================================================================
    // PURPOSE: This function checks licensing of Annotation functionality
    static bool isAnnotationLicensed();

    //=======================================================================
    // PURPOSE: To validate the license state of a particular licenseId
    static void validateLicense(unsigned long ulLicenseID, const string& strELICode,
		const string& strComponentName);

    //=======================================================================
	// PURPOSE: To initialize the TRP date/time values
	// ARGS:	strLicenseCode - the encrypted day code
	static void initTrpData(const string& strLicenseCode);

    //=======================================================================
	// PURPOSE: To generate the user license string for this machine
	// ARGS:	strLicenseCode - the encrypted day code
	static string getUserLicense(const string& strLicenseCode);

    //=======================================================================
    // PURPOSE: To reset the cached licensed state which will force the
    //			licenseID's to be checked on the next validateLicense call
    //
    // NOTE:	The main purpose of this function is so that during testing
    //			we can reset the cached license state of components so that
    //			the next call to validateLicense will force a call to
    //			isLicensed.
    static void resetCache();

    //=======================================================================
    // PURPOSE: This function returns true if there has been a call to 
    //			loadLicenseFilesFromFolder has been called.
    static bool filesLoadedFromFolder()
    {
        return m_bFilesLoadedFromFolder;
    }

    //=======================================================================
    // PURPOSE: To enable all currently licensed component IDs
    // Added as per [LegacyRCAndUtils #4993]
    static void enableAll();

    //=======================================================================
    // PURPOSE: To enable the specified licensed component ID
    // Added as per [LegacyRCAndUtils #4993]
    static void enableId(unsigned long ulComponentID);

    //=======================================================================
    // PURPOSE: To disable all currently licensed component IDs
    // Added as per [LegacyRCAndUtils #4993]
    static void disableAll();

    //=======================================================================
    // PURPOSE: To disable the specified licensed component ID
    // Added as per [LegacyRCAndUtils #4993]
    static void disableId(unsigned long ulComponentID);

	//=======================================================================
    // PURPOSE: Unlicenses all components
	// Added to address [FlexIDSCore:5286]
    static void unlicenseAll();

	//=======================================================================
    // PURPOSE: Unlicenses the specified component.
    static void unlicenseId(unsigned long ulComponentID);

	//=======================================================================
    // PURPOSE: A special purpose function that should be called only via
	// Extract.Interop.SecureObjectCreator -> Extract.Licensing.LicenseUtilities.InitRegisteredObjects
	// in order to validate that the SecureObjectCreator implementation being used is ours.
	// In particular, this initializes an encrypted day code (LICENSE_MGMT_PASSWORD) to be able to
	// validate SecureObjectCreator have registered themselves via Extract.Licenseing as expected.
	static void initRegisteredObjects();

	//=======================================================================
    // PURPOSE: A special purpose function that should be called only via
	// Extract.Interop.SecureObjectCreator -> Extract.Licensing.LicenseUtilities.RegisterObject
	// in order to validate that the SecureObjectCreator implementation being used is ours.
	// In particular, this registers a SecureObjectCreator's InstanceID using the previously
	// initialized encrypted day code (LICENSE_MGMT_PASSWORD). This prevents 3rd party code from
	// being able to use registerObjectBase to do the registration that is expected of our
	// SecureObjectCreator implementation.
	static void registerObject(long objectCode);

protected:
    //=======================================================================
    // PURPOSE: Updates the m_LicenseData object with later expiration dates
    //              or by setting the fully licensed flag.  The "best" 
    //              settings will end up stored in m_LicenseData.
    // REQUIRE: None
    // PROMISE: Each component ID in rData will be checked against the 
    //              same ID in m_LicenseData.
    // ARGS:	rData - source of update information
    static void	updateLicenseData( LMData& rData );

    static void validateState();

private:

    //////////
    // Methods
    //////////

    // Create the TRP object.  Will throw exception if unsuccessful.
    static void createTRPObject();
	static void closeTRPObject();

	static bool m_bDoNotCheckTempLicenseYet;

    // Throws exception if the Annotation support is not licensed
    static void validateAnnotationLicense();

    // Throws exception if the PDF Read-Write is not licensed
    static void validatePDFLicense();

	// Throws exception if the PDF Read is not licensed
	// if used for loading license files the PDF Read Write should be
	// checked first
	static void validatePDFReadLicense();

    // Checks whether a component ID is licensed or not
    // NOTE: This method expects that the caller has protected this call
    //		 via a mutex.  This method is not thread safe.
    // NOTE: Added as per [LegacyRCAndUtils #4994] which
    //		 moved validateLicense into the LicenseManagement class
    //		 and thus required validateLicense and isLicensed
    //		 to use the same logic to check the licensed state of a
    //		 component.
    static bool internalIsLicensed(unsigned long ulLicenseID);

	// Will either start or stop the TRP service based on whether the current license
	// data contains an expiring component
	static void startOrCloseTrpBasedOnLicenseData();

	// Checks to see if high memory test mode should be activated for this processes if specified
	// by HKLM\SOFTWARE\[Wow6432Node]\Extract Systems\ReusableComponents\BaseUtils\HighMemoryTestMode
	static void initializeHighMemTestMode();

    ///////
    // Data
    ///////

    // mutex to prevent simultaneous access to this object
    static CMutex m_lock;

    // Did the User License checking indicate invalid license
    static bool	m_bUserLicenseFailure;
    
    // OEM password associated with passwords provided in InitializeFromFile()
    static long	m_lOEMPassword;

    // Has the OEM password been successfully provided
    static bool	m_bOEMPasswordOK;

    // Flag to indicate that files have been loaded from folder
    static bool m_bFilesLoadedFromFolder;

    // Collected license information
    static LMData	m_LicenseData;

	// Event that is signaled by the TRP if there is a license validation issue
	static Win32Event m_licenseStateIsInvalidEvent;

	// The TRP object initialized if there is a temporary license
	static unique_ptr<CMutex> m_upTrpRunning;
	static unique_ptr<CMutex> m_upValidState;

    // maps that are used by the validateLicense function to cache
    // the licensed state values for components that have already been validated
    static map<unsigned long, int> m_mapIdToDayLicensed;
    static map<unsigned long, bool> m_mapIdToLicensed;

	// Indicates whether in this process we have already checked on whether to activate high
	// memory test mode.
	static volatile bool m_initializedHighMemoryMode;

	// Keeps track of the number of calls to registerObject. This will be expected to always match
	// a the count of calls to registerObjectBase. If it does not, someone has been trying to
	// register objects outside of SecureObjectCreator and should trigger license validation failure.
	static long m_nRegisteredObjectCount;
};
//============================================================================
// PURPOSE: Use the following macro as a general way to check if a particular
//			component is licensed.  If it is established that a particular component
//			is licensed by a line of code that uses this macro, from then onwards, 
//			that line of code will execute quicker because the following code 
//			prevents re-checking of a license once it has been established that
//			the license exists.
//
// UPDATE:  [P13 #4878] - WEL 04/17/2008
//			Modify the macro to reset the static bool bLicensed at the beginning of 
//			each day.  This will cause a re-evaluation via isLicensed() for each 
//			component so that when an evaluation license has expired, the software 
//			will stop operation.
//
#define VALIDATE_LICENSE(ulCompID, strELICode, strComponentName) \
    { \
        static DWORD gdwLAST_VALIDATE_CALL_TIME = 0; \
        static bool gbVALIDATE_LICENSE = true; \
        if (gbVALIDATE_LICENSE || (GetTickCount() / 1000) != gdwLAST_VALIDATE_CALL_TIME) \
        { \
            gbVALIDATE_LICENSE = true; \
            LicenseManagement::validateLicense(ulCompID, strELICode, strComponentName); \
\
            gbVALIDATE_LICENSE = false; \
            gdwLAST_VALIDATE_CALL_TIME = (GetTickCount() / 1000); \
        } \
    }
//============================================================================
// PURPOSE: Use the following macro to add the "getEncryptedDayCode" function
//			to any file that need it (the function has been renamed to
//			getMapLabel so that it is not obvious what the purpose of the
//			function is based upon its name).  Added as per [LRCAndU #4991]
//
// NOTE:	Algorithm description - We compute a code value based on the
//			following information: month, day, year, and day of year.
//			This value is computed via the following formula:
//			(Y * M) + ([{Y * M} % D] * M) + (M^2 * D) + ([{D % M} + D] * M) +
//				(Y * M * DoY) + ([Y * {M + D}] % DoY)
//			The return value for the algorithm comes from computing an
//			__int64 value whose high and low word values are computed via the
//			following formula:
//			HIWORD - Computed by taking the natural log of the code value,
//					 multiplying it by 1000000000000000.0 and then converting
//					 it to an unsigned long to truncate the remaining decimals
//			LOWORD - Computed by seeding the random number generator with the
//					 code value dumping the first two random value and adding the
//					 next three values together dumping the next two values
//					 and adding the next two together
#define DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION inline string getMapLabel() \
    { \
        __time64_t ltime; \
        tm _tm; \
        bool bGetTimeAgain; \
        do \
        {\
            time( &ltime ); \
            if (_localtime64_s( &_tm, &ltime ) != 0) \
            { \
                throw UCLIDException ("ELI19415", "Unable to get necessary data."); \
            } \
            bGetTimeAgain = false; \
            if (_tm.tm_hour == 23 && _tm.tm_min == 59 && _tm.tm_sec > 53) \
            { \
                Sleep(1000 * (60 - _tm.tm_sec)); \
                bGetTimeAgain = true; \
            } \
        } \
        while (bGetTimeAgain); \
        int iMonth = _tm.tm_mon + 1; \
        int iDay = _tm.tm_mday; \
        int iYear = _tm.tm_year + 1900; \
        int iDayOfYear = _tm.tm_yday + 1; \
        int iDayOfWeek = _tm.tm_wday + 1; \
        unsigned long ulCode = iYear * iMonth + ((iYear * iMonth) % iDay) * iMonth + \
                               iMonth * iMonth * iDay + \
                               (iDay % iMonth + iDay) * iMonth + \
                               (iYear * iMonth * iDayOfYear * iDayOfWeek) + \
                               ((iYear * (iMonth + iDay)) % iDayOfYear); \
        srand((unsigned int) ulCode); \
        rand(); \
        unsigned long ln = (unsigned long)(log((double)ulCode) * 1000000000000000.0); \
        unsigned long sum = rand() + rand() + rand(); \
        rand(); rand(); \
        sum += rand() + rand(); \
        unsigned __int64 returnVal = ln; \
        returnVal <<= 32; \
        returnVal |= sum; \
        return asString(returnVal); \
    }
//============================================================================
// PURPOSE:	The purpose of this macro is to validate a private license string
//			against the internally generated private license string.
//
// REQUIRE: That the DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION is already defined
//			in the file where IS_VALID_PRIVATE_LICENSE is called.
#define IS_VALID_PRIVATE_LICENSE(strLicense) \
        (asUnsignedLong(strLicense) == asUnsignedLong(getMapLabel()))
//============================================================================
// PURPOSE: To provide easy access to the license management private license
//			password function.
//
// REQUIRE: That the DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION is already defined
//			in the file where LICENSE_MGMT_PASSWORD is called.
#define LICENSE_MGMT_PASSWORD getMapLabel()
//============================================================================
