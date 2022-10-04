// ESConvertToPDF.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "ESConvertToPDFApp.h"
#include <UCLIDException.h>
#include <UCLIDExceptionDlg.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <cppUtil.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------

// The number of pages that should be processed before freeing all memory from the Nuance RecAPI
// to prevent excessive usage of memory on large documents.
const int g_nPAGE_BATCH_SIZE = 10;

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// Message map
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CESConvertToPDFApp, CWinApp)
	//{{AFX_MSG_MAP(CESConvertToPDFApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG
	ON_COMMAND(ID_HELP, CWinApp::OnHelp)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CESConvertToPDFApp
//-------------------------------------------------------------------------------------------------
CESConvertToPDFApp::CESConvertToPDFApp()
	: m_strInputFile(""), 
	  m_strOutputFile(""),
	  m_bRemoveOriginal(false),
	  m_bPDFA(false),
	  m_bIsError(true),          // assume error until successfully completed
	  m_strExceptionLogFile(""),
	  m_strUserPassword(""),
	  m_strOwnerPassword(""),
	  m_bPasswordsAreEncrypted(false),
	  m_nPermissions(0)
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}
//-------------------------------------------------------------------------------------------------

// The one and only CESConvertToPDFApp object
CESConvertToPDFApp theApp;

//-------------------------------------------------------------------------------------------------
BOOL CESConvertToPDFApp::InitInstance()
{
	CoInitializeEx(NULL, COINIT_MULTITHREADED);

	try
	{
		// set exception handling dialog
		UCLIDExceptionDlg ue_dlg;
		UCLIDException::setExceptionHandler(&ue_dlg);

		// get valid arguments
		if (getAndValidateArguments(__argc, __argv))
		{
			// load the license files
			LicenseManagement::loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);

			IImageFormatConverterPtr converter = getImageFormatConverter();

			converter->CreateSearchablePdf(
				m_strInputFile.c_str(),
				m_strOutputFile.c_str(),
				asVariantBool(m_bRemoveOriginal),
				asVariantBool(m_bPDFA),
				m_strUserPassword.c_str(),
				m_strOwnerPassword.c_str(),
				asVariantBool(m_bPasswordsAreEncrypted),
				m_nPermissions);

			// completed successfully
			m_bIsError = false;
		}
	}
	catch (...)
	{
		UCLIDException ue = uex::fromCurrent("ELI18536");

		// check if the exception log parameter was set
		if (m_strExceptionLogFile.empty())
		{
			// no log file was specified, so display the exception
			ue.display();
		}
		else
		{
			// log the exception
			ue.log(m_strExceptionLogFile, true);
		}
	}

	CoUninitialize();

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
int CESConvertToPDFApp::ExitInstance() 
{
	return (m_bIsError ? EXIT_FAILURE : EXIT_SUCCESS);
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
bool CESConvertToPDFApp::displayUsage(const string& strFileName, const string& strErrMsg)
{
	// if there is an error message, prepend it
	string strUsage;
	if( !strErrMsg.empty() )
	{
		strUsage = strErrMsg;
		strUsage += "\n\n";
	}
		
	// create the usage message
	strUsage += "Converts a machine-printed text image file into a searchable pdf file.\n\n";
	strUsage += strFileName;
	strUsage += " <source> <destination> [/R] [/pdfa] [/user \"<Password>\"]";
	strUsage += " [/owner \"<Password>\" <Permissions>] [/ef <logfile>]\n";
	strUsage += "NOTE: You cannot specify both /pdfa and security settings (/user and/or /owner)\n\n";
	strUsage += " source\t\tSpecifies the image file to convert into a searchable pdf file.\n";
	strUsage += " destination\tSpecifies the filename of the searchable pdf file to create.\n";
	strUsage += " /R\t\tRemove original image file after conversion.\n";
	strUsage += " /pdfa\t\tSpecifies that the output file should be PDF/A compliant.\n";
	strUsage += " /user\t\tSpecifies the user password to apply to the PDF.\n";
	strUsage += " /owner\t\tSpecified the owner password and permissions to apply to the PDF.\n";
	strUsage += " Permissions - An integer that is the sum of all permissions to set.\n";
	strUsage += " \t\tAllow low quality printing = 1.\n";
	strUsage += " \t\tAllow high quality printing = 2.\n";
	strUsage += " \t\tAllow document modifications = 4.\n";
	strUsage += " \t\tAllow copying/extraction of contents = 8.\n";
	strUsage += " \t\tAllow accessibility access to contents = 16.\n";
	strUsage += " \t\tAllow adding/modifying text annotations = 32.\n";
	strUsage += " \t\tAllow filling in form fields = 64.\n";
	strUsage += " \t\tAllow document assembly = 128.\n";
	strUsage += " \t\tAllow all options = 255.\n";
	strUsage += " /ef\t\tLog all errors to an exception file.\n";
	strUsage += " logfile\t\tSpecifies the filename of an exception log to create.\n";

	// display the message
	AfxMessageBox(strUsage.c_str(), m_bIsError ? MB_ICONWARNING : MB_ICONINFORMATION);

	// done.
	return false;
}
//-------------------------------------------------------------------------------------------------
bool CESConvertToPDFApp::errMsg(const string& strELICode, const string& strErrorDescription, 
				 const string& strDebugInfoKey, const string& strDebugInfoValue)
{
	// check if the error message should be displayed to the user
	if(m_strExceptionLogFile.empty())
	{
		AfxMessageBox( 
			(strErrorDescription + "\n\n" + strDebugInfoKey + ": " + strDebugInfoValue).c_str(),
			MB_ICONWARNING);
	}
	else
	{
		// throw an exception
		UCLIDException ue(strELICode, strErrorDescription);
		ue.addDebugInfo(strDebugInfoKey, strDebugInfoValue);
		throw ue;
	}

	return false;
}
//-------------------------------------------------------------------------------------------------
bool CESConvertToPDFApp::getAndValidateArguments(const int argc, char* argv[])
{
	if(argc == 2 && string(argv[1]) == "/?")
	{
		m_bIsError = false;
		return displayUsage(argv[0]);
	}
	else if(argc < 3 || argc > 13)
	{
		// invalid number of arguments
		return displayUsage(argv[0], "Invalid number of arguments.");
	}

	m_bPasswordsAreEncrypted = false;
	for(int i=3; i<argc; i++)
	{
		string curArg(argv[i]);

		// check if current argument is recognized
		if(curArg == "/ef")
		{
			// get the exception log filename if specified
			i++;
			if(i<argc)
			{
				// store the exception log filename
				m_strExceptionLogFile = argv[i];	
			}
			else
			{
				return displayUsage(argv[0], "Invalid parameters: exception log filename expected after /ef");
			}
		}
		else if(curArg == "/R")
		{
			// set the remove original flag
			m_bRemoveOriginal = true;
		}
		else if (curArg == "/pdfa")
		{
			m_bPDFA = true;
		}
		else if (curArg == "/user")
		{
			// Get the password
			i++;
			if (i < argc)
			{
				// store the password
				m_strUserPassword = argv[i];
			}
			else
			{
				return displayUsage(argv[0], "Invalid parameters: user password expected after /user");
			}
		}
		else if (curArg == "/owner")
		{
			// Get the password
			i++;
			if (i < argc)
			{
				// store the password
				m_strOwnerPassword = argv[i];
			}
			else
			{
				return displayUsage(argv[0],
					"Invalid parameters: owner password expected after /owner.");
			}

			// Get the permissions
			i++;
			if (i < argc)
			{
				// Get the permissions value and validate it
				try
				{
					m_nPermissions = (int) asLong(argv[i]);
					if (m_nPermissions < 0 || m_nPermissions > 255)
					{
						return displayUsage(argv[0],
							"Invalid parameters: permissions value must be between 0 and 255.");
					}
				}
				catch(...)
				{
					return displayUsage(argv[0],
						"Invalid parameters: permissions value expected (number between 0 and 255).");
				}
			}
			else
			{
				return displayUsage(argv[0],
					"Invalid parameters: owner permissions expected after password.");
			}
		}
		else if (curArg == "/enc")
		{
			m_bPasswordsAreEncrypted = true;
		}
		else
		{
			return displayUsage(argv[0], "Invalid parameter: " + string(curArg));
		}
	}

	// Check for /pdfa and either /user or /owner
	if (m_bPDFA && (!m_strUserPassword.empty() || !m_strOwnerPassword.empty()))
	{
		return displayUsage(argv[0],
			"Invalid parameters: cannot specify both /pdfa and either /user or /owner.");
	}

	// get the input file and output file
	// expand to full paths because they will be passed to another process without the same working directory
	m_strInputFile = buildAbsolutePath(argv[1]); 
	m_strOutputFile = buildAbsolutePath(argv[2]);

	// validate input file
	if(!fileExistsAndIsReadable(m_strInputFile))
	{
		// throw or display an error
		return errMsg("ELI18550", "Invalid filename. Input file must be readable.", 
			"Input filename", m_strInputFile);
	}
	else if(isValidFolder(m_strInputFile))
	{
		// throw or display an error
		return errMsg("ELI18551", "Invalid filename. Input file cannot be a folder.",
			"Input filename", m_strInputFile);
	}

	// validate output file
	if( isFileOrFolderValid(m_strOutputFile) )
	{
		if( isValidFolder(m_strOutputFile) )
		{
			// throw or display an error
			return errMsg("ELI18548", "Invalid filename. Output file cannot be a folder.", 
				"Output filename", m_strOutputFile);
		}
		else if( isFileReadOnly(m_strOutputFile) )
		{
			// throw or display an error
			return errMsg("ELI18549", "Invalid filename. Output file is write-protected.", 
				"Output filename", m_strOutputFile);
		}
		else if(m_strExceptionLogFile.empty() &&
			IDOK != 
			AfxMessageBox(("Are you sure want to overwrite " + m_strOutputFile + "?").c_str(), 
			MB_OKCANCEL) )
		{
			// since we are not logging exceptions, prompt the user to overwrite the output file.
			// if the user does not choose OK, exit.
			m_bIsError = false;
			return false;
		}
	}
	else
	{
		// validate output directory
		char pszOutputFullPath[MAX_PATH + 1];
		if( !_fullpath(pszOutputFullPath, m_strOutputFile.c_str(), MAX_PATH) )
		{
			// throw or display an error
			return errMsg("ELI18552", "Invalid path for output file.",
				"Output filename", m_strOutputFile);
		}
		string strOutputDir( getDirectoryFromFullPath(pszOutputFullPath) );
		if( !isValidFolder(strOutputDir) )
		{
			// the output directory folder doesn't exist.
			// if we are not logging exceptions, prompt user if folder should be created.
			if(m_strExceptionLogFile.empty() && 
				IDOK != AfxMessageBox(
				("Output directory " + strOutputDir + " does not exist. Create?").c_str(), MB_OKCANCEL))
			{
				m_bIsError = false;
				return false;
			}

			// create the output file's directory
			createDirectory(strOutputDir);
		}
	}

	// if we reached this far, the arguments were valid
	return true;
}
//-------------------------------------------------------------------------------------------------
IImageFormatConverterPtr CESConvertToPDFApp::getImageFormatConverter()
{
	IImageFormatConverterPtr ipImageFormatConverter(CLSID_ScansoftOCR);
	ASSERT_RESOURCE_ALLOCATION("ELI53658", ipImageFormatConverter != __nullptr);

	// license the OCR engine
	IPrivateLicensedComponentPtr ipPL(ipImageFormatConverter);
	ASSERT_RESOURCE_ALLOCATION("ELI53659", ipPL != __nullptr);
	ipPL->InitPrivateLicense(LICENSE_MGMT_PASSWORD.c_str());

	return ipImageFormatConverter;
}
