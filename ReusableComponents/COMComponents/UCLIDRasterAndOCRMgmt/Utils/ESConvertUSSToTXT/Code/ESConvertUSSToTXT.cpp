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
: m_eCounterToDecrement(kRedaction)
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
	string strUsage = "This application has 2 required arguments and three optional arguments:\n";
		strUsage += "An input filename for conversion and an output filename.\n\n"
			"The optional argument (/m:none) indicates that a missing input file should "
			"not create an output text file.  The optional argument (/m:zero) indicates that "
			"a missing input file should create a zero-length output text file.\n\n"
			"The 2nd optional argument indicates the counter that should be decremented by the "
			"page count and can be one of the following:\n"
			"\t/Indexing\n"
			"\t/Pagination\n"
			"\t/Redaction\n\n"
			"The optional argument (/ef <filename>) specifies the location of an \n"
			"exception log that will store any thrown exception.  Without an exception \n"
			"log, any thrown exception will be displayed.\n\n";
		strUsage += "Usage:\n";
		strUsage += "ESConvertUSSToTXT.exe <strInputFile> <strOutputFile> [/m:<none|zero>] [/Indexing|/Pagination|/Redaction] [/ef <filename>]\n"
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

				// Make sure the number of parameters is between 2 and 6
				unsigned int uiParamCount = (unsigned int)vecParams.size();
				if ((uiParamCount < 2) || (uiParamCount > 6))
				{
					usage();
					return FALSE;
				}

				// Set default handling option for missing input file
				EHandlingType eNoUSS = kHandlingType_Exception;

				// Retrieve file names and output type
				string strInputName = vecParams[0];
				string strOutputName = vecParams[1];

				// Set a flag to indicate if the counter switch has been specified
				bool bCounterSwitchSpecified = false;

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
						else if (!bCounterSwitchSpecified && strArgument.find( "/indexing" ) == 0)
						{
							bCounterSwitchSpecified = true;
							m_eCounterToDecrement = kIndexing;
						}
						else if (!bCounterSwitchSpecified && strArgument.find( "/pagination" ) == 0)
						{
							bCounterSwitchSpecified = true;
							m_eCounterToDecrement = kPagination;
						}
						else if (!bCounterSwitchSpecified && strArgument.find( "/redaction" ) == 0)
						{
							bCounterSwitchSpecified = true;
							m_eCounterToDecrement = kRedaction;
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
				LicenseManagement::loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);
				validateLicense();

				// Make sure the input file exists
				if (eNoUSS == kHandlingType_Exception)
				{
					::validateFileOrFolderExistence( strInputName );
				}

				// Convert the file
				//convertUSSFile( strInputName, strOutputName, eNoUSS );

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
void CESConvertUSSToTXTApp:: convertUSSFile(
	const std::string strInputFileName, 
	const std::string strOutputFileName, 
	const EHandlingType eNoUSSFile)
{
	try
	{
		// Create the Spatial String object
		ISpatialStringPtr ipSS( CLSID_SpatialString );
		ASSERT_RESOURCE_ALLOCATION("ELI20372", ipSS != __nullptr);

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
			if (!oFile.is_open())
			{
				UCLIDException ue("ELI34229", "Output file could not be opened.");
				ue.addDebugInfo("Filename", strOutputFileName);
				ue.addWin32ErrorInfo();
				throw ue;
			}

			// Write the text to the output file
			string strText = asString( ipSS->String );
			oFile << strText;
		}
		else if (eNoUSSFile == kHandlingType_ZeroLengthOutput)
		{
			// Open the output file
			oFile.open( strOutputFileName.c_str() );
			if (!oFile.is_open())
			{
				UCLIDException ue("ELI34230", "Output file could not be opened.");
				ue.addDebugInfo("Filename", strOutputFileName);
				ue.addWin32ErrorInfo();
				throw ue;
			}

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
	ASSERT_ARGUMENT( "ELI20378", ipText != __nullptr );

	// Check to see if USB Counters are to be ignored 
	if (usbCountersDisabled())
	{
		return;
	}

	UCLIDException ue("ELI46762", "ESConvertUSSToTXT app does not support Databse Counters.");
	throw ue;
}
//-------------------------------------------------------------------------------------------------
bool CESConvertUSSToTXTApp::usbCountersDisabled()
{
	return LicenseManagement::isLicensed( gnIGNORE_RULE_EXECUTION_COUNTER_DECREMENTS ); 
}
//-------------------------------------------------------------------------------------------------
void CESConvertUSSToTXTApp::validateLicense()
{
	// Requires SCANSOFT_OEM_OCR license (P13 #4936)
	static const unsigned long THIS_APP_ID = gnSCANSOFT_OEM_OCR_FEATURE;

	VALIDATE_LICENSE(THIS_APP_ID, "ELI20369", "ESConvertUSSToTXT" );
}
//-------------------------------------------------------------------------------------------------
