// RunRules.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "RunRules.h"
#include "RunRulesDlg.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <UCLIDExceptionDlg.h>
#include <cpputil.h>
#include <ComponentLicenseIDs.h>

#include <vector>
#include <string>

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// CRunRulesApp
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CRunRulesApp, CWinApp)
	//{{AFX_MSG_MAP(CRunRulesApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG
	ON_COMMAND(ID_HELP, CWinApp::OnHelp)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CRunRulesApp construction
//-------------------------------------------------------------------------------------------------
CRunRulesApp::CRunRulesApp()
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}

//-------------------------------------------------------------------------------------------------
// The one and only CRunRulesApp object
//-------------------------------------------------------------------------------------------------
CRunRulesApp theApp;
//-------------------------------------------------------------------------------------------------
bool bExitSuccess = true;

//-------------------------------------------------------------------------------------------------
// Local functions
//-------------------------------------------------------------------------------------------------
void validateLicense()
{
	static const unsigned long THIS_APP_ID = gnRUN_RULES_OBJECT;

	VALIDATE_LICENSE( THIS_APP_ID, "ELI15234", "Run Rules" );
}
//-------------------------------------------------------------------------------------------------
void validateServerLicense()
{
	// Check for Core Server
	VALIDATE_LICENSE( gnFLEXINDEX_IDSHIELD_SERVER_CORE, "ELI15252", "RunRules for non-text input" );
}
//-------------------------------------------------------------------------------------------------
void usage(const string& strError)
{
	string strUsage = "";
	if(strError.length() > 0)
	{
		strUsage = strError + "\n\n";
	}

	strUsage += "This application takes a mininum of 2 arguments:\n";
	strUsage += "A rules file (.rsd or .rsd.etf) and an \n"
				"input document file (eg. uss, txt, tif, etc.).\n\n";
	strUsage += "Usage:\n";
	strUsage += "RunRules.exe <strRulesFile> <strDocumentFile> [OPTIONS]\n";
	strUsage += "OPTIONS:\n";
	strUsage += "/c -	Create .uss File - Create <strDocumentFile>.uss\n";
	strUsage += "	if it doesn't exist\n";
	strUsage += "/i -	Ignore .uss File - unless /i is used this\n"
				"	program will try to find a .uss file\n"
				"	associated with <strDocumentFile> and\n"
				"	use that instead of <strDocumentFile> itself\n";
	strUsage += "/ef <ExceptionFile> - log exceptions to the specified file\n";
	strUsage += "   If no exception file is specified will log exceptions to\n";
	strUsage += "   the default Extract Exception log\n";
	AfxMessageBox(strUsage.c_str());
}

//-------------------------------------------------------------------------------------------------
// CRunRulesApp initialization
//-------------------------------------------------------------------------------------------------
BOOL CRunRulesApp::InitInstance()
{
	AfxEnableControlContainer();

	bExitSuccess = true;

	string strExceptionLogFile = "";
	bool bIgnoreUss = false;
	bool bCreateUss = false;

	CoInitializeEx(NULL, COINIT_MULTITHREADED);
	try
	{
		try
		{
			try
			{
				static UCLIDExceptionDlg exceptionDlg;
				UCLIDException::setExceptionHandler(&exceptionDlg);

				vector<string> vecParams;
				int i;
				for(i = 0; i < __argc; i++)
				{
					vecParams.push_back( __argv[i]);
				}

				// make sure the number of parameters is at least 3
				if(vecParams.size() < 3)
				{
					usage("Incorrect number of parameters!");
					return FALSE;
				}

				for(i = 3; i < __argc; i++)
				{
					string strArg = vecParams[i];
					makeLowerCase(strArg);
					if( strArg == "/ef" )
					{
						// Look for exception file
						// 1. There is another argument
						// 2. It does not contain '/'
						if (i+1 < __argc && vecParams[i+1].find("/") == string::npos)
						{
							strExceptionLogFile = vecParams[++i];
						}
						else
						{
							// Now log file defined, just log to standard exception
							strExceptionLogFile = UCLIDException::getDefaultLogFileFullPath();
						}
					}
					else if( strArg == "/i" )
					{
						bIgnoreUss = true;
					}
					else if( strArg == "/c" )
					{
						bCreateUss = true;
					}
					else
					{
						usage("Unrecognized parameter: " + vecParams[i]);
						return FALSE;
					}
				}

				// all arguments are supposed to be interpreted relative
				// to the current working directory
				string strDummyFileInCurrentDir = getCurrentDirectory() + "\\dummy.txt";
				string strRSDFileName = getAbsoluteFileName(strDummyFileInCurrentDir, vecParams[1]);

				// make sure the rsd file exists
				::validateFileOrFolderExistence(strRSDFileName);

				// determine the source doc name relative to the current directory
				string strSourceFileName = getAbsoluteFileName(strDummyFileInCurrentDir, vecParams[2]);

				// Initialize license
				LicenseManagement::loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);
				validateLicense();

				string strUssFile = strSourceFileName + ".uss";

				if (bCreateUss)
				{
					if (!isFileOrFolderValid(strUssFile))
					{
						IOCREnginePtr ipOCR(CLSID_ScansoftOCR);
						ASSERT_RESOURCE_ALLOCATION("ELI12502", ipOCR != __nullptr);
						
						// license OCR engine
						IPrivateLicensedComponentPtr ipScansoftEngine(ipOCR);
						ASSERT_RESOURCE_ALLOCATION("ELI16151", ipScansoftEngine != __nullptr);
						ipScansoftEngine->InitPrivateLicense( get_bstr_t(LICENSE_MGMT_PASSWORD) );

						ISpatialStringPtr ipSS = ipOCR->RecognizeTextInImage(
							strSourceFileName.c_str(), 1, -1, UCLID_RASTERANDOCRMGMTLib::kNoFilter, 
							"", UCLID_RASTERANDOCRMGMTLib::kRegistry, VARIANT_TRUE, NULL);
						ipSS->SaveTo(get_bstr_t(strUssFile), VARIANT_TRUE, VARIANT_FALSE);
					}
				}

				// see if a .uss file exists for the specified file
				// if so we will use that for processing instead of 
				// the specified file
				// It is known that if a .uss file is specified this will check for
				// "filename.uss.uss" but that won't break anything
				if (!bIgnoreUss)
				{
					if(isFileOrFolderValid(strUssFile))
					{
						strSourceFileName = strUssFile;
					}
				}

				// make sure the source doc exists
				::validateFileOrFolderExistence(strSourceFileName);

				// Check file extension, must additionally have SERVER_CORE 
				// license if non-TXT file
				string strExt = getExtensionFromFullPath( strSourceFileName, true );
				if (strExt != ".txt")
				{
					validateServerLicense();
				}

				// Create AFDocument for attribute finding
				IAFDocumentPtr ipAFDoc(CLSID_AFDocument);
				ASSERT_RESOURCE_ALLOCATION("ELI10110", ipAFDoc != __nullptr);

				// Create Attribute Finder Engine
				IAttributeFinderEnginePtr ipAFEngine(CLSID_AttributeFinderEngine);
				ASSERT_RESOURCE_ALLOCATION("ELI10109", ipAFEngine != __nullptr);

				// Find attributes from the source document
				ipAFEngine->FindAttributes(ipAFDoc, strSourceFileName.c_str(),
					-1, strRSDFileName.c_str(), NULL, VARIANT_FALSE, NULL);
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI10134");
		}
		catch(UCLIDException& ue)
		{
			bExitSuccess = false;

			// If the exception log file was not specified then throw exception
			// (it will be caught and displayed by the outer handler)
			if (strExceptionLogFile.empty())
			{
				throw ue;
			}
			else
			{
				// Log the exception to the specified file
				ue.log(strExceptionLogFile);
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI10108");
	// CoUninitialize removed to prevent unhandled exception on exit (P16 #2642)
	//CoUninitialize();

	// Ensure all accumulated USB clicks are decremented.
	try
	{
		IRuleSetPtr ipRuleSet(CLSID_RuleSet);
		ASSERT_RESOURCE_ALLOCATION("ELI33405", ipRuleSet != __nullptr);

		ipRuleSet->Cleanup();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI33404");

	// Since the dialog has been closed, return FALSE so that we exit the
	//  application, rather than start the application's message pump.
	return TRUE;
}
//-------------------------------------------------------------------------------------------------
int CRunRulesApp::Run()
{
	if(bExitSuccess)
	{
		return EXIT_SUCCESS;
	}
	else
	{
		return EXIT_FAILURE;
	}
}
//-------------------------------------------------------------------------------------------------
int CRunRulesApp::ExitInstance()
{
	if(bExitSuccess)
	{
		return EXIT_SUCCESS;
	}
	else
	{
		return EXIT_FAILURE;
	}
}
//-------------------------------------------------------------------------------------------------
