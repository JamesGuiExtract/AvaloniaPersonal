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
#include <cpputil.h>
#include <COMUtils.h>

#include <io.h>

extern AFX_EXTENSION_MODULE COMLMCoreDLL;

using namespace std;

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

static const string g_strLicenseExtension = ".lic";

LicenseManagement* LicenseManagement::ms_pLM;

// add license management functions
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// LicenseManagement
//-------------------------------------------------------------------------------------------------
LicenseManagement::LicenseManagement()
:	m_bErrorInitializingTRP(false),
	m_bFilesLoadedFromFolder(false)
{
	m_bUserLicenseFailure = false;
	m_lOEMPassword = 0;
	m_bOEMPasswordOK = false;
	m_hwndTRP = NULL;
}
//-------------------------------------------------------------------------------------------------
LicenseManagement::LicenseManagement(const LicenseManagement& toCopy)
{
	throw UCLIDException("ELI02640", 
		"Internal error: copy constructor of singleton class called!");
}
//-------------------------------------------------------------------------------------------------
LicenseManagement& LicenseManagement::sGetInstance()
{
	static LicenseManagement lm;
	return lm;
}
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
LicenseManagement& LicenseManagement::operator = (const LicenseManagement& toAssign)
{
	throw UCLIDException("ELI02641", 
		"Internal error: assignment operator of singleton class called!");
}
//-------------------------------------------------------------------------------------------------
LicenseManagement::~LicenseManagement()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16431");
}
//-------------------------------------------------------------------------------------------------
void LicenseManagement::terminate()
{
	// Do not close the ExtractTRP2 EXE, leave it running for any other applications.
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
	// prevent simultaneous access to this object from multiple threads
	CSingleLock guard(&m_lock, TRUE);

	// Check for bad license state
	validateState();

	string	strFQPath = strLicenseFile;

	// Check file name for backslash indicating fully qualified path
	if (strLicenseFile.find( '\\' ) == -1)
	{
		// Not found, append filename to path from bin folder
		strFQPath = getModuleDirectory(COMLMCoreDLL.hModule) + "\\" + strLicenseFile;
	}

	// Retrieve the specified String from the license file
	LMData	Data;
	string	strUserData;
	strUserData = Data.unzipStringFromFile( strFQPath, bUserString );

	// Extract license data from the User String
	Data.extractDataFromString( strUserData, ulKey1, ulKey2, ulKey3, ulKey4 );

	// Determine the associated OEM password
	m_lOEMPassword = Data.generateOEMPassword( ulKey1, ulKey2, ulKey3, 
		ulKey4 );

	///////////////////////////
	// Check User License items
	///////////////////////////

	// Check Computer Name
	if (Data.getUseComputerName())
	{
		// Compare computer names
		string strName;
		strName = getComputerName();
		if (strName.compare( Data.getUserComputerName() ) != 0)
		{
			// No need to keep checking
			return;
		}
	}

	// Check Disk Serial Number
	if (Data.getUseSerialNumber())
	{
		// Compare disk serial numbers
		unsigned long ulNumber;
		ulNumber = getDiskSerialNumber();
		if (ulNumber != Data.getUserSerialNumber())
		{
			// No need to keep checking
			return;
		}
	}

	// Check MAC Address
	if (Data.getUseMACAddress())
	{
		// Compare addresses
		string strAddress;
		strAddress = getMACAddress();
		if (strAddress.compare( Data.getUserMACAddress() ) != 0)
		{
			// No need to keep checking
			return;
		}
	}

	/////////////////////
	// Store license data
	/////////////////////
	try
	{
		try
		{
			// Check for an expiring component
			if (Data.containsExpiringComponent())
			{
				// Check if first component is already expired
				if (!Data.isLicensed( Data.getFirstComponentID() ))
				{
					// Force license file to Normal
					SetFileAttributes( strLicenseFile.c_str(), FILE_ATTRIBUTE_NORMAL );

					// New filename just appends ".expired"
					string strNew = strLicenseFile.c_str() + string( ".expired" );

					// Rename the license file
					CFile::Rename( strLicenseFile.c_str(), strNew.c_str() );
				}
				else if (m_hwndTRP == NULL)
				{
					createTRPObject();
				}
			}

			// Update data member with this object
			updateLicenseData( Data );
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI10682")
	}
	catch (UCLIDException ue)
	{
		ue.log();
	}
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

	// Find and use the Common Components folder that contains COMLMCore.dll
	string strLocal = getModuleDirectory( COMLMCoreDLL.hModule );
	loadLicenseFilesFromFolder( strLocal, strValue, iType );
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
	BOOL	bResult = finder.FindFile( strDirectory.c_str() );

	while (bResult)
	{
		bResult = finder.FindNextFile();

		// Retrieve path to this license file
		strLicenseFile = (finder.GetFilePath()).operator LPCTSTR();

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
void LicenseManagement::validateLicense(unsigned long ulLicenseID, string strELICode,
										string strComponentName)
{
	// prevent simultaneous access to this object from multiple threads
	CSingleLock guard(&m_lock, TRUE);

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
	CTime tmNow = CTime::GetCurrentTime();
	int nThisDay = tmNow.GetDay();

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
void LicenseManagement::resetCache()
{
	for(map<unsigned long, bool>::iterator it = m_mapIdToLicensed.begin();
		it != m_mapIdToLicensed.end(); it++)
	{
		it->second = false;
	}
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void LicenseManagement::createTRPObject()
{
	// Unless this method completely executes, we want this object to
	// work as if TRP initialization failed.
	m_bErrorInitializingTRP = true;

	// Find the window associated with the Time Rollback Preventer
	// and run the EXE if it is not yet running
	m_hwndTRP = getTRPWindow();

	// Authenticate the TRP object to make sure it is our object
	LRESULT authCode = SendMessage( m_hwndTRP, gGET_AUTHENTICATION_CODE_MSG, 0, 0 );
	if (authCode != asUnsignedLong(LICENSE_MGMT_PASSWORD))
	{
		throw UCLIDException("ELI15469", "Unable to authenticate TRP object!");
	}

	// If we got here, then the TRP was succesfully initialized
	m_bErrorInitializingTRP = false;
}
//-------------------------------------------------------------------------------------------------
HWND LicenseManagement::getTRPWindow()
{
	// "#32770" is the WNDCLASS name of a dialog and that is what we 
	// are looking for
	HWND hWnd = ::FindWindow( MAKEINTATOM(32770), gstrTRP_WINDOW_TITLE.c_str() );

	// If window is not found, start the EXE associated with the Time Rollback Preventer
	if (hWnd == NULL)
	{
		// Compute the path to the EXE
		string strEXEPath = getModuleDirectory(COMLMCoreDLL.hModule);
		strEXEPath += "\\";
		strEXEPath += gstrTRP_EXE_NAME.c_str();

		// Run the EXE
		ProcessInformationWrapper piw;
		runEXE( strEXEPath, "", 0, &piw );

		// Wait for EXE to come up.  If the window is not findable within
		// the maximum allowable time, throw an exception
		const unsigned long ulMAX_WAIT_TIME_IN_SECONDS = 10;
		time_t startTime = time(NULL);
		time_t endTime = time(NULL);
		bool bTRPProcessStillRunning = false;
		do
		{
			// check if the TRP process is still running
			bTRPProcessStillRunning = (WaitForSingleObject(piw.pi.hProcess, 0) == WAIT_TIMEOUT);

			// Find the dialog window with expected title
			hWnd = ::FindWindow( MAKEINTATOM(32770), gstrTRP_WINDOW_TITLE.c_str() );
			if (hWnd == NULL)
			{
				// Not found, update time and sleep before next attempt
				endTime = time(NULL);
				Sleep(50);
			}
		}
		while (bTRPProcessStillRunning && hWnd == NULL &&
			endTime - startTime <= ulMAX_WAIT_TIME_IN_SECONDS);

		// Throw an exception if not found
		if (hWnd == NULL)
		{
			throw UCLIDException("ELI15470", 
				"Unable to establish connection with TRP object!");
		}
	}

	// At this point we are guaranteed that hWnd is not NULL
	return hWnd;
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
	// If the TRP initialization failed, throw an exception
	if (m_bErrorInitializingTRP)
	{
		throw UCLIDException("ELI13092", 
			"Extract Systems license state is assumed to be corrupted - unable to initialize TRP!");
	}

	// If the time rollback preventer has been initialized, then check its state
	if (m_hwndTRP != NULL)
	{
		const unsigned long ulMIN_TIME_BETWEEN_CHECKS = 60; // seconds
		static time_t ls_lastCheckTime = 0;
		static bool ls_bStateIsValid = false;

		// If the number of seconds elapsed since the last check is more 
		// than ulMIN_TIME_BETWEEN_CHECKS seconds, then check again.
		time_t currentTime = time(NULL);
		time_t timeDelta = currentTime - ls_lastCheckTime;
		if (timeDelta >= ulMIN_TIME_BETWEEN_CHECKS || timeDelta < 0)
		{
			ls_lastCheckTime = currentTime;

			// Check the state of the licensing
			LRESULT returnCode = ::SendMessage(m_hwndTRP, gSTATE_IS_VALID_MSG, 0, 0);
			ls_bStateIsValid = (returnCode == guiVALID_STATE_CODE);
		}
		
		// If the most reecntly checked state is not valid, then throw an exception
		if (!ls_bStateIsValid)
		{
			throw UCLIDException("ELI13089", "Extract Systems license state has been corrupted!");
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

	// Component is not licensed if not found
	bool	bResult = false;

	// Check the master LMData object
	if (m_LicenseData.isLicensed( ulComponentID ))
	{
		bResult = true;
	}

	// Return the search result
	return bResult;
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
