// ESConvertUSSToTXT.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "ESConvertUSSToTXT.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <UCLIDExceptionDlg.h>
#include <cpputil.h>
#include <ComponentLicenseIDs.h>

#include <fstream>

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// CESConvertUSSToTXTApp
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CESConvertUSSToTXTApp, CWinApp)
	ON_COMMAND(ID_HELP, &CWinApp::OnHelp)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// Globals
//-------------------------------------------------------------------------------------------------
int nExitCode = EXIT_SUCCESS;

//-------------------------------------------------------------------------------------------------
// CESConvertUSSToTXTApp construction
//-------------------------------------------------------------------------------------------------
CESConvertUSSToTXTApp::CESConvertUSSToTXTApp()
{
}

//-------------------------------------------------------------------------------------------------
// The one and only CESConvertUSSToTXTApp object
//-------------------------------------------------------------------------------------------------
CESConvertUSSToTXTApp theApp;

//-------------------------------------------------------------------------------------------------
// Local functions
//-------------------------------------------------------------------------------------------------
void usage()
{
	string strUsage = "This application has 2 required arguments and two optional arguments:\n";
		strUsage += "An input filename for conversion and an output filename.\n\n"
			"The optional argument (/m:none) indicates that a missing input file should "
			"not create an output text file.  The optional argument (/m:zero) indicates that "
			"a missing input file should create a zero-length output text file.\n"
			"The optional argument (/ef <filename>) specifies the location of an \n"
			"exception log that will store any thrown exception.  Without an exception \n"
			"log, any thrown exception will be displayed.\n\n";
		strUsage += "Usage:\n";
		strUsage += "ESConvertUSSToTXT.exe <strInputFile> <strOutputFile> [/m:<none|zero>] [/ef <filename>]\n"
					"where:\n"
					"\t<strInputFile> is a USS file to be converted\n"
					"\t<strOutputFile> is the output TXT file\n"
					"\t<none> or <zero> indicate special handling for a missing USS file\n";
					"\t<filename> is the fully-qualified path to an exception log.\n\n";
		AfxMessageBox(strUsage.c_str());
}

//-------------------------------------------------------------------------------------------------
// CESConvertUSSToTXTApp initialization
//-------------------------------------------------------------------------------------------------
BOOL CESConvertUSSToTXTApp::InitInstance()
{
	try
	{
		AfxEnableControlContainer();

		CoInitializeEx(NULL, COINIT_MULTITHREADED);

		// Define empty string for local exception log
		string strLocalExceptionLog;

		try
		{
			try
			{
				// Setup exception handling
				static UCLIDExceptionDlg exceptionDlg;
				UCLIDException::setExceptionHandler( &exceptionDlg );

				// Retrieve command-line parameters for ESConvertUSSToTXT.exe
				vector<string> vecParams;
				int i;
				for (i = 1; i < __argc; i++)
				{
					vecParams.push_back( __argv[i]);
				}

				// Make sure the number of parameters is between 2 and 5
				unsigned int uiParamCount = (unsigned int)vecParams.size();
				if ((uiParamCount < 2) || (uiParamCount > 5))
				{
					usage();
					return FALSE;
				}

				// Set default handling option for missing input file
				EHandlingType eNoUSS = kHandlingType_Exception;

				// Retrieve file names and output type
				string strInputName = vecParams[0];
				string strOutputName = vecParams[1];

				// Check for additional options
				if (uiParamCount > 2)
				{
					// Loop through the remaining arguments
					for (unsigned int i = 2; i < uiParamCount; i++)
					{
						// Retrieve argument string
						string strArgument = vecParams[i];
						makeLowerCase( strArgument );

						// Check for error file argument
						if (strArgument == "/ef")
						{
							if (uiParamCount >= i+1)
							{
								// Retrieve filename
								strLocalExceptionLog = vecParams[++i];
							}
							else
							{
								// /ef option without required filename
								usage();
								return FALSE;
							}
						}
						// Check for special handling of empty USS file argument
						else if (strArgument.find( "/m:" ) == 0)
						{
							// Check for create no output file
							if (strArgument.find( "/m:none" ) == 0)
							{
								eNoUSS = kHandlingType_NoOutput;
							}
							// Check for create zero-length output file
							else if (strArgument.find( "/m:zero" ) == 0)
							{
								eNoUSS = kHandlingType_ZeroLengthOutput;
							}
							// Unsupported option
							else
							{
								usage();
								return FALSE;
							}
						}
						// Unsupported argument
						else
						{
							usage();
							return FALSE;
						}
					}
				}

				// Load license files and validate the license
				LicenseManagement::sGetInstance().loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);
				validateLicense();

				// Make sure the input file exists
				if (eNoUSS == kHandlingType_Exception)
				{
					::validateFileOrFolderExistence( strInputName );
				}

				// Convert the file
				convertUSSFile( strInputName, strOutputName, eNoUSS );

				// No UI needed, just return
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20370");
		}
		catch(UCLIDException ue)
		{
			// Set failure code
			nExitCode = EXIT_FAILURE;

			// Deal with the exception
			if (strLocalExceptionLog.empty())
			{
				// If not logged locally, it should be displayed
				ue.display();
			}
			else
			{
				// Log the exception
				ue.log( strLocalExceptionLog, false );
			}
		}

		CoUninitialize();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI20371")

	// Since the dialog has been closed, return FALSE so that we exit the
	//  application, rather than start the application's message pump.
	return nExitCode;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CESConvertUSSToTXTApp::convertUSSFile(const string strInputFileName, 
										   const string strOutputFileName, 
										   const EHandlingType eNoUSSFile)
{
	try
	{
		// Create the Spatial String object
		ISpatialStringPtr ipSS( CLSID_SpatialString );
		ASSERT_RESOURCE_ALLOCATION("ELI20372", ipSS != NULL);

		ofstream oFile;

		// Check existence of input file
		if (fileExistsAndIsReadable( strInputFileName ))
		{
			// Load the Spatial String from the input ( USS ) file
			ipSS->LoadFrom( get_bstr_t(strInputFileName.c_str()), VARIANT_FALSE );

			// Decrement the USB counter
			decrementCounter( ipSS );

			// Open the output file
			oFile.open( strOutputFileName.c_str() );

			// Write the text to the output file
			string strText = asString( ipSS->String );
			oFile << strText;
		}
		else if (eNoUSSFile == kHandlingType_ZeroLengthOutput)
		{
			// Open the output file
			oFile.open( strOutputFileName.c_str() );

			// Write an empty string to the output file
			oFile << "";
		}
		// else do not create an output file

		// If the file was opened, then close it and wait for it to be readable
		if (oFile.is_open())
		{
			oFile.close();
			waitForFileToBeReadable(strOutputFileName);
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20379");
}
//-------------------------------------------------------------------------------------------------
void CESConvertUSSToTXTApp::decrementCounter(ISpatialStringPtr ipText)
{
	ASSERT_ARGUMENT( "ELI20378", ipText != NULL );

	// Check to see if USB Counters are to be ignored 
	if (usbCountersDisabled())
	{
		return;
	}

	// Use page-level redaction counter at this time - WEL 02/06/2008
	bool bUsePagesRedactionCounter = true;
	bool bUseDocsRedactionCounter = false;

	// Only check counters if necessary
	if (bUsePagesRedactionCounter || bUseDocsRedactionCounter)
	{
		// Create the License Manager object
		SafeNetLicenseMgr snlMgr( gusblFlexIndex );

		// DO NOT check USB Key serial number

		// Update counters as needed
		if (bUsePagesRedactionCounter)
		{
			// Decrement counter once if non-spatial (P16 #1907)
			long nNumberOfPages = 1;

			// Decrement counter once for each page if spatial
			if (ipText->HasSpatialInfo() == VARIANT_TRUE)
			{
				nNumberOfPages = ipText->GetLastPageNumber() - ipText->GetFirstPageNumber() + 1;
			}

			snlMgr.decreaseCellValue( gdcellIDShieldRedactionCounter, nNumberOfPages );
		}

		if (bUseDocsRedactionCounter)
		{
			// Decrement counter once for the document.
			snlMgr.decreaseCellValue( gdcellIDShieldRedactionCounter, 1 );
		}
	}
}
//-------------------------------------------------------------------------------------------------
bool CESConvertUSSToTXTApp::usbCountersDisabled()
{
	static const unsigned long DISABLE_USB_COUNTERS = gnIGNORE_USB_DECREMENT_FEATURE;

	return LicenseManagement::sGetInstance().isLicensed( DISABLE_USB_COUNTERS ); 
}
//-------------------------------------------------------------------------------------------------
void CESConvertUSSToTXTApp::validateLicense()
{
	// Requires SCANSOFT_OEM_OCR license (P13 #4936)
	static const unsigned long THIS_APP_ID = gnSCANSOFT_OEM_OCR_FEATURE;

	VALIDATE_LICENSE(THIS_APP_ID, "ELI20369", "ESConvertUSSToTXT" );
}
//-------------------------------------------------------------------------------------------------
