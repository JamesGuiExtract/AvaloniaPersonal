//-------------------------------------------------------------------------------------------------
//
// COPYRIGHT (c) 2002 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	LicenseMgmt.cpp
//
// PURPOSE:	Implementation of the LicenseManagement class
//
// NOTES:	
//
// AUTHORS:	Wayne Lenius
//
//-------------------------------------------------------------------------------------------------

#include "stdafx.h"
#include "LicenseMgmt.h"
#include "SpecialIcoMap.h"
#include "SpecialSimpleRules.h"
#include "ComponentLicenseIDs.h"
#include "ExtractTRP2Constants.h"

#include <UCLIDException.h>
#include <EncryptionEngine.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <MutexUtils.h>

#include <io.h>

extern AFX_EXTENSION_MODULE COMLMCoreDLL;

using namespace std;

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// Statics
//-------------------------------------------------------------------------------------------------
static const string g_strLicenseExtension = ".lic";

CMutex LicenseManagement::m_lock;
bool LicenseManagement::m_bUserLicenseFailure = false;
long LicenseManagement::m_lOEMPassword = 0;
bool LicenseManagement::m_bOEMPasswordOK = false;
bool LicenseManagement::m_bFilesLoadedFromFolder = false;
bool LicenseManagement::m_bDoNotCheckTempLicenseYet = false;
LMData LicenseManagement::m_LicenseData;
map<unsigned long, int> LicenseManagement::m_mapIdToDayLicensed;
map<unsigned long, bool> LicenseManagement::m_mapIdToLicensed;
Win32Event LicenseManagement::m_licenseStateIsInvalidEvent;
unique_ptr<CMutex> LicenseManagement::m_upTrpRunning(__nullptr);
unique_ptr<CMutex> LicenseManagement::m_upValidState(getGlobalNamedMutex(gpszGoodStateMutex));

//-------------------------------------------------------------------------------------------------
// LicenseManagement
//-------------------------------------------------------------------------------------------------
bool LicenseManagement::getBadState()
{
	bool bStateIsBad = true;
	try
	{
		// prevent simultaneous access to this object from multiple threads
		CSingleLock guard(&m_lock, TRUE);
		try
		{
			validateState();
			bStateIsBad = false;
		}
		catch (...)
		{
			bStateIsBad = true;
			throw;
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI15439")

	return bStateIsBad;
}
//-------------------------------------------------------------------------------------------------
void LicenseManagement::ignoreLockConstraints(long lKey)
{
	// prevent simultaneous access to this object from multiple threads
	CSingleLock guard(&m_lock, TRUE);

	// Check for bad license state
	validateState();

	// Check that OEM password was previously defined
	if (m_lOEMPassword == 0)
	{
		// Create and throw exception
		UCLIDException ue("ELI03776", 
			"IgnoreLockConstraints() called before InitializeLicenseFromFile()!");
		throw ue;
	}

	// Compare the passwords
	if (m_lOEMPassword == lKey)
	{
		// Set flag
		m_bOEMPasswordOK = true;
	}
	else
	{
		// Clear the flag
		m_bOEMPasswordOK = false;

		// Create and throw exception
		UCLIDException ue("ELI03777", 
			"Invalid OEM password in IgnoreLockConstraints()!");
		ue.addDebugInfo( "Provided OEM password", lKey );
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
// The long string contained on line 2 of the license file consists of two 
// interlaced encrypted strings: User String and UCLID String.  Each string 
// contains identical licensing information.  The User String encrypts the 
// already encrypted license information with four provided DWORD passwords.  
// The UCLID String encrypts the already encrypted license information with 
// four hardcoded DWORD passwords.
void LicenseManagement::initializeLicenseFromFile(const std::string& strLicenseFile, 
												  unsigned long ulKey1, 
												  unsigned long ulKey2, 
												  unsigned long ulKey3, 
												  unsigned long ulKey4, 
												  bool bUserString /*= true*/)
{
	// Check for bad license state
	validateState();

	string	strFQPath = strLicenseFile;

	// Check file name for backslash indicating fully qualified path
	if (strLicenseFile.find( '\\' ) == string::npos)
	{
		// Not found, append filename to path from license folder
		strFQPath = getExtractLicenseFilesPath() + "\\" + strLicenseFile;
	}

	// Retrieve the specified String from the license file
	LMData	Data;
	string strUserData = Data.unzipStringFromFile( strFQPath, bUserString );

	// Extract license data from the User String
	Data.extractDataFromString( strUserData, ulKey1, ulKey2, ulKey3, ulKey4 );

	// Determine the associated OEM password
	long lOEMPassword = Data.generateOEMPassword( ulKey1, ulKey2, ulKey3, 
		ulKey4 );

	///////////////////////////
	// Check User License items
	///////////////////////////

	// Check computer name, disk serial number and MAC address
	if ((Data.getUseComputerName() && !Data.checkUserComputerName())
		|| (Data.getUserSerialNumber() && !Data.checkUserSerialNumber())
		|| (Data.getUseMACAddress() && !Data.checkUserMACAddress()))
	{
		// No need to keep checking
		return;
	}

	/////////////////////
	// Store license data
	/////////////////////
	try
	{
		if (Data.containsExpiringComponent() && Data.isFirstComponentExpired())
		{
			// Attempt to rename the license file to expired
			try
			{
				moveFile(strFQPath, strFQPath + ".expired", true, true);
			}
			catch(UCLIDException& uex)
			{
				uex.log();
			}

			// Log an application trace about the expired file
			UCLIDException uex("ELI32449", "Application Trace: License file is expired.");
			uex.addDebugInfo("License File Name", strFQPath);
			uex.log();

			// Do not do anything else with this license data since it is expired
			return;
		}

		// prevent simultaneous access to this object from multiple threads
		CSingleLock guard(&m_lock, TRUE);

		// Update data member with this object
		updateLicenseData( Data );

		// Update the OEM password (log a trace if the password was previously set and
		// it has changed)
		if (m_lOEMPassword != 0 && m_lOEMPassword != lOEMPassword)
		{
			UCLIDException ue("ELI32456", "Application Trace: OEM sdk mismatch.");
			ue.addDebugInfo("Previous OEM", m_lOEMPassword, true);
			ue.addDebugInfo("New OEM", lOEMPassword, true);
			ue.log();
		}
		m_lOEMPassword = lOEMPassword;

		if (!m_bDoNotCheckTempLicenseYet)
		{
			startOrCloseTrpBasedOnLicenseData();
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI10682");
}
//-------------------------------------------------------------------------------------------------
void LicenseManagement::initializeLicenseFromFile(const std::string& strLicenseFile,
												  const std::string& strValue,
												  int iType)
{
	// prevent simultaneous access to this object from multiple threads
	CSingleLock guard(&m_lock, TRUE);

	if (!IS_VALID_PRIVATE_LICENSE(strValue))
	{
		throw UCLIDException("ELI21647", "Unable to initialize license!");
	}

	// Check for bad license state
	validateState();

	// Retrieve passwords to be applied to the UCLID String
	unsigned long	ulKey1;
	unsigned long	ulKey2;
	unsigned long	ulKey3;
	unsigned long	ulKey4;
	switch (iType)
	{
		case 0:
			// Use regular UCLID passwords
			ulKey1 = gulUCLIDKey1;
			ulKey2 = gulUCLIDKey2;
			ulKey3 = gulUCLIDKey3;
			ulKey4 = gulUCLIDKey4;
			break;

		case 1:
			// Use special IcoMap passwords
			ulKey1 = gulIcoMapKey1;
			ulKey2 = gulIcoMapKey2;
			ulKey3 = gulIcoMapKey3;
			ulKey4 = gulIcoMapKey4;
			break;

		case 2:
			// Use special Simple Rule Writing passwords
			ulKey1 = gulSimpleRulesKey1;
			ulKey2 = gulSimpleRulesKey2;
			ulKey3 = gulSimpleRulesKey3;
			ulKey4 = gulSimpleRulesKey4;
			break;

		default:
			UCLIDException ue( "ELI12415", "Invalid special password type." );
			ue.addDebugInfo( "Desired Type", iType );
			throw ue;
	}

	// Extract license data using the desired passwords
	// applied to the UCLID String from the file
	initializeLicenseFromFile( strLicenseFile, ulKey1, ulKey2, ulKey3, ulKey4, false );
}
//-------------------------------------------------------------------------------------------------
bool LicenseManagement::isLicensed(unsigned long ulComponentID)
{
	// prevent simultaneous access to this object from multiple threads
	CSingleLock guard(&m_lock, TRUE);

	// Check for bad license state
	validateState();

	return internalIsLicensed(ulComponentID);
}
//-------------------------------------------------------------------------------------------------
bool LicenseManagement::isTemporaryLicense(unsigned long ulComponentID)
{
	// prevent simultaneous access to this object from multiple threads
	CSingleLock guard(&m_lock, TRUE);

	// Check for bad license state
	validateState();

	// Check that the component is licensed
	ASSERT_ARGUMENT("ELI23063", internalIsLicensed(ulComponentID));

	// Return whether it is a temporary license or not
	return m_LicenseData.isTemporaryLicense(ulComponentID);
}
//-------------------------------------------------------------------------------------------------
CTime LicenseManagement::getExpirationDate(unsigned long ulComponentID)
{
	CSingleLock guard(&m_lock, TRUE);

	// Check for bad license state
	validateState();

	// Check that the component is licensed
	ASSERT_ARGUMENT("ELI23092", internalIsLicensed(ulComponentID));

	// Check that the component is temporarily licensed
	ASSERT_ARGUMENT("ELI23093", m_LicenseData.isTemporaryLicense(ulComponentID));

	// Return the expiration date
	return m_LicenseData.getExpirationDate(ulComponentID);
}
//-------------------------------------------------------------------------------------------------
void LicenseManagement::loadLicenseFilesFromFolder(const std::string& strValue, int iType)
{
	// prevent simultaneous access to this object from multiple threads
	CSingleLock guard(&m_lock, TRUE);

	// Check for bad license state
	validateState();

	// Use the default extract license folder path
	loadLicenseFilesFromFolder( getExtractLicenseFilesPath(), strValue, iType );
}
//-------------------------------------------------------------------------------------------------
void LicenseManagement::loadLicenseFilesFromFolder(std::string strDirectory,
												   const std::string& strValue,
												   int iType)
{
	// prevent simultaneous access to this object from multiple threads
	CSingleLock guard(&m_lock, TRUE);

	// Check for bad license state
	validateState();

	// Check size of directory name
	long lLength = strDirectory.length();
	if (lLength == 0)
	{
		// Throw exception
		UCLIDException ue( "ELI03892", "Directory string is empty!" );
		throw ue;
	}

	// Check existence of directory
	if (!isValidFolder(strDirectory))
	{
		// Throw exception
		UCLIDException ue( "ELI03896", "Directory does not exist!" );
		ue.addDebugInfo( "Name", strDirectory );
		ue.addWin32ErrorInfo();
		throw ue;
	}

	// Create the license file search string
	if (strDirectory[lLength - 1] != '\\')
	{
		// First append a backslash
		strDirectory += string( "\\" );
	}
	strDirectory += string( "*.lic" );

	// Find each license file in the directory
	CFileFind	finder;
	string	strLicenseFile;
	bool bResult = asCppBool(finder.FindFile(strDirectory.c_str()));

	// Don't launch trp until after loading all license files
	m_bDoNotCheckTempLicenseYet = true;
	while (bResult)
	{
		bResult = asCppBool(finder.FindNextFile());

		// Retrieve path to this license file
		strLicenseFile = (LPCTSTR)finder.GetFilePath();

		// Check extension
		string strExt = getExtensionFromFullPath( strLicenseFile, true );
		if (strExt != g_strLicenseExtension)
		{
			continue;
		}

		// Do not attempt to load the IcoMap 4.0 internal-use License File 
		// because it has its own passwords
		if (getFileNameFromFullPath( strLicenseFile, true ) == g_strIcoMapComponentsLicenseName)
		{
			continue;
		}

		// Add this information to the license object
		try
		{
			try
			{
				initializeLicenseFromFile( strLicenseFile, strValue, iType );
			}
			catch (UCLIDException& ue)
			{
				// Encapsulate in outer exception to show up as the highest level 
				// ELI string for this exception (P16 #2493)
				UCLIDException uexOuter("ELI16901", "Unable to decrypt license file!", ue);
				uexOuter.addDebugInfo( "License File", strLicenseFile );
				uexOuter.addDebugInfo( "File Type", iType );
				throw uexOuter;
			}
		}
		CATCH_AND_LOG_ALL_EXCEPTIONS("ELI03904")
	}
	m_bDoNotCheckTempLicenseYet = false;
	startOrCloseTrpBasedOnLicenseData();

	// Set the files loaded from folder flag to true
	m_bFilesLoadedFromFolder = true;
}
//-------------------------------------------------------------------------------------------------
bool LicenseManagement::isPDFLicensed()
{
	bool bLicensed = true;

	try
	{
		// Check PDF license
		validatePDFLicense();
	}
	catch (...)
	{
		// PDF Read/Write is not licensed
		bLicensed = false;
	}

	return bLicensed;
}
//-------------------------------------------------------------------------------------------------
void LicenseManagement::verifyFileTypeLicensed( std::string strFileName )
{
	if (isPDFFile( strFileName ))
	{
		if ( !isPDFLicensed() )
		{
			UCLIDException ue("ELI13429", "PDF read/write support is not licensed.");
			ue.addDebugInfo("PDFFileName", strFileName );
			throw ue;
		}
	}
}
//-------------------------------------------------------------------------------------------------
bool LicenseManagement::isAnnotationLicensed()
{
	bool bLicensed = true;

	try
	{
		// Check Annotation license
		validateAnnotationLicense();
	}
	catch (...)
	{
		// Annotation Read/Write is not licensed
		bLicensed = false;
	}

	return bLicensed;
}
//-------------------------------------------------------------------------------------------------
void LicenseManagement::validateLicense(unsigned long ulLicenseID, const string& strELICode,
										const string& strComponentName)
{
	// Check if license management is in a bad state
	try
	{
		validateState();
	}
	catch(...)
	{
		UCLIDException extractException(strELICode, strComponentName +
			" component is not licensed - license state is corrupt.");
		extractException.addDebugInfo("Component Name", strComponentName);
		UCLIDException uexOuter(gstrLICENSE_CORRUPTION_ELI,
			"License state is corrupt!", extractException);
		throw uexOuter;
	}

	// Get the current day
	int nThisDay = CTime::GetCurrentTime().GetDay();

	// prevent simultaneous access to this object from multiple threads
	CSingleLock guard(&m_lock, TRUE);

	// Check if we have a stored day value for this license ID
	map<unsigned long, int>::iterator it = m_mapIdToDayLicensed.find(ulLicenseID);
	if (it != m_mapIdToDayLicensed.end())
	{
		// Check the day
		if (nThisDay != it->second)
		{
			// Different day, store the new day and set the component's license state to false
			it->second = nThisDay;
			m_mapIdToLicensed[ulLicenseID] = false;
		}
	}
	// New component, store the day value and set the licensed state to false to force
	// a check of its license state
	else
	{
		m_mapIdToDayLicensed[ulLicenseID] = nThisDay;
		m_mapIdToLicensed[ulLicenseID] = false;
	}

	// check if the component is already in a licensed state
	bool bLicensed = true;
	if (!m_mapIdToLicensed[ulLicenseID])
	{
		// check the license state
		bLicensed = internalIsLicensed(ulLicenseID);
		m_mapIdToLicensed[ulLicenseID] = bLicensed;
	}

	if (bLicensed)
	{
		return;
	}

	// Component is not licensed, build a new exception and throw it
	// NOTE: We need to encrypt the license ID value so that our component ID's are
	//		 not exposed to the user.
	UCLIDException ee(strELICode, strComponentName + " component is not licensed.");
	ee.addDebugInfo("Component Name", strComponentName);
	ee.addDebugInfo("Component ID", ulLicenseID, true);
	throw ee;
}
//-------------------------------------------------------------------------------------------------
void LicenseManagement::initTrpData(const string& strLicenseCode)
{
	try
	{
		if (!IS_VALID_PRIVATE_LICENSE(strLicenseCode))
		{
			throw UCLIDException("ELI32462", "Unable to initialize data.");
		}

		CSingleLock guard(&m_lock, TRUE);

		TimeRollbackPreventer trp(false);
		trp.checkDateTimeItems();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI32461");
}
//-------------------------------------------------------------------------------------------------
string LicenseManagement::getUserLicense(const string& strLicenseCode)
{
	try
	{
		if (!IS_VALID_PRIVATE_LICENSE(strLicenseCode))
		{
			throw UCLIDException("ELI32468", "Unable to initialize data.");
		}

		// Use the LM structure to generate the user string
		LMData lm(getComputerName(), getDiskSerialNumber(), getMACAddress());
		return lm.getUserLicenseString();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI32469");
}
//-------------------------------------------------------------------------------------------------
void LicenseManagement::resetCache()
{
	// prevent simultaneous access to this object from multiple threads
	CSingleLock guard(&m_lock, TRUE);

	for(map<unsigned long, bool>::iterator it = m_mapIdToLicensed.begin();
		it != m_mapIdToLicensed.end(); it++)
	{
		it->second = false;
	}
}
//-------------------------------------------------------------------------------------------------
void LicenseManagement::enableAll()
{
	CSingleLock guard(&m_lock, TRUE);

	m_LicenseData.enableAll();
}
//-------------------------------------------------------------------------------------------------
void LicenseManagement::enableId(unsigned long ulComponentID)
{
	CSingleLock guard(&m_lock, TRUE);

	m_LicenseData.enableId(ulComponentID);
}
//-------------------------------------------------------------------------------------------------
void LicenseManagement::disableAll()
{
	CSingleLock guard(&m_lock, TRUE);

	m_LicenseData.disableAll();
}
//-------------------------------------------------------------------------------------------------
void LicenseManagement::disableId(unsigned long ulComponentID)
{
	CSingleLock guard(&m_lock, TRUE);

	m_LicenseData.disableId(ulComponentID);
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void LicenseManagement::createTRPObject()
{
	try
	{
		// Check for TRP already running
		if (m_upTrpRunning.get() == __nullptr)
		{
			// Lock around TRP initialization
			CSingleLock guard(&m_lock, TRUE);

			if (m_upTrpRunning.get() == __nullptr)
			{
				m_upTrpRunning.reset(getGlobalNamedMutex(gpszTrpRunning));
				ASSERT_RESOURCE_ALLOCATION("ELI32536", m_upTrpRunning.get() != __nullptr);

				// Compute the path to the EXE
				string strEXEPath = getModuleDirectory(COMLMCoreDLL.hModule);
				strEXEPath += "\\";
				strEXEPath += gstrTRP_EXE_NAME.c_str();

				// Launch the exe and sleep for a second to let it initialize
				runEXE(strEXEPath);
				Sleep(1000);
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI32448");
}
//-------------------------------------------------------------------------------------------------
void LicenseManagement::closeTRPObject()
{
	try
	{
		// Check for TRP already launched
		if (m_upTrpRunning.get() != __nullptr)
		{
			// Lock around TRP initialization
			CSingleLock guard(&m_lock, TRUE);

			if (m_upTrpRunning.get() != __nullptr)
			{
				// Compute the path to the EXE
				string strEXEPath = getModuleDirectory(COMLMCoreDLL.hModule);
				strEXEPath += "\\";
				strEXEPath += gstrTRP_EXE_NAME.c_str();

				// Run the EXE with the exit flag
				runEXE(strEXEPath, "/exit");

				m_upTrpRunning.reset(__nullptr);
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI32538");
}
//-------------------------------------------------------------------------------------------------
void LicenseManagement::updateLicenseData( LMData& rData )
{
	// Check through each component ID in rData

	// Find first component ID
	long lTestID = rData.getFirstComponentID();
	while (lTestID != -1)
	{
		// Get the associated ComponentData
		ComponentData	CD = rData.getComponentData( lTestID );

		// Check for this ID in data member
		if (m_LicenseData.isComponentFound( lTestID ))
		{
			// Retrieve component data currently saved
			ComponentData	currentCD = m_LicenseData.getComponentData( lTestID );

			// Is this component already fully licensed?
			if (!currentCD.m_bIsLicensed)
			{
				// Is new component already fully licensed?
				if (CD.m_bIsLicensed)
				{
					// Just add this ID as licensed
					m_LicenseData.addLicensedComponent( lTestID );
				}
				// Neither are licensed, compare expiration dates
				else
				{
					// Only update the Component Data if expiration date is later
					if (CD.m_ExpirationDate > currentCD.m_ExpirationDate)
					{
						m_LicenseData.addUnlicensedComponent( lTestID, CD.m_ExpirationDate );
					}	// end if new CD has later expiration date
				}		// end else new CD is not licensed yet either
			}			// end if this CD is not licensed
		}				// end if this ID is present
		// Not found yet, just add this one
		else
		{
			// Is this Component fully licensed?
			if (CD.m_bIsLicensed)
			{
				m_LicenseData.addLicensedComponent( lTestID );
			}
			// Not licensed
			else
			{
				// Get expiration date
				CTime	dateExpire = CD.m_ExpirationDate;

				// Add this component with the expiration date
				m_LicenseData.addUnlicensedComponent( lTestID, dateExpire );
			}	// end else new CD is not licensed
		}		// end else this CD not found yet

		// Get the next component ID
		lTestID = rData.getNextComponentID( lTestID );
	}			// end while each new CD
}
//-------------------------------------------------------------------------------------------------
void LicenseManagement::validateState()
{
	// If the time rollback preventer has been initialized, then check its state
	if (m_upTrpRunning.get() != __nullptr)
	{
		CSingleLock (&m_lock, TRUE);

		if (m_upTrpRunning.get() != __nullptr)
		{
			// Check that both TRP is running and the valid handle is set
			CSingleLock lRunning(m_upTrpRunning.get(), FALSE);
			CSingleLock lValid(m_upValidState.get(), FALSE);

			if (lRunning.Lock(0) == TRUE || lValid.Lock(0) == TRUE)
			{
				throw UCLIDException("ELI13089", "Extract Systems license state has been corrupted!");
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------
bool LicenseManagement::internalIsLicensed(unsigned long ulComponentID)
{
	//////////////////////////
	// Check User License flag
	//////////////////////////
	if (!m_bOEMPasswordOK)
	{
		// OEM password has not been provided, 
		// therefore check for User License constraints
		if (m_bUserLicenseFailure)
		{
			return false;
		}
		// else No User License Failure was noted
		// so go ahead and check the components
	}
	// else OEM Password has been provided
	// so there is no need to check the User License information
	// so go ahead and check the components

	///////////////////
	// Check components
	///////////////////

	return m_LicenseData.isLicensed(ulComponentID);
}
//-------------------------------------------------------------------------------------------------
void LicenseManagement::startOrCloseTrpBasedOnLicenseData()
{
	// Now check if we have any expiring components and launch TRP if necessary
	if (m_LicenseData.containsExpiringComponent())
	{
		// Create call does nothing if TRP is already running
		createTRPObject();
	}
	// If no expiring components then close TRP if it is running
	else if (m_upTrpRunning.get() != __nullptr)
	{
		closeTRPObject();
	}
}
//-------------------------------------------------------------------------------------------------
void LicenseManagement::validateAnnotationLicense()
{
	static const unsigned long ANNOTATION_ID = gnANNOTATION_FEATURE;

	VALIDATE_LICENSE( ANNOTATION_ID, "ELI14553", "Annotation Support" );
}
//-------------------------------------------------------------------------------------------------
void LicenseManagement::validatePDFLicense()
{
	static const unsigned long ALLOW_PDF_READWRITE_ID = gnPDF_READWRITE_FEATURE;

	VALIDATE_LICENSE( ALLOW_PDF_READWRITE_ID, "ELI13431", "PDF Read Write" );
}
//-------------------------------------------------------------------------------------------------
