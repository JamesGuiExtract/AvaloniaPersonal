// ESConvertToPDF.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "ESConvertToPDF.h"
#include "ScansoftErr.h"
#include "RecMemoryReleaser.h"
#include "OcrMethods.h"

#include <UCLIDException.h>
#include <UCLIDExceptionDlg.h>
#include <TemporaryFileName.h>
#include <OCRConstants.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <RecAPIPlus.h>
#include <StringCSIS.h>
#include <cppUtil.h>
#include <PdfSecurityValues.h>
#include <ByteStream.h>
#include <ByteStreamManipulator.h>
#include <EncryptionEngine.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------

// constants for the required RecAPI modules
const int giNUM_REQUIRED_MODULES = 13;
const ModuleDescriptionType gREQUIRED_MODULES[giNUM_REQUIRED_MODULES] = 
{
	{INFO_API,    "API main OCR module"},
	{INFO_MOR,    "MOR multi-lingual omnifont recognition"},
	{INFO_DOT,    "DOT 9-pin draft dot-matrix recognition"},
	{INFO_DCM,    "DCM legacy page-layout decomposition"},
	{INFO_IMG,    "IMG image handling"},
	{INFO_IMF,    "IMF image file I/O"},
	{INFO_CHR,    "CHR character set and code page handling"},
	{INFO_MTX,    "MTC M/TEXT omnifont recognition"},
	{INFO_MAT,    "MAT matrix matching recognition"},
	{INFO_PLUS2W, "PLUS2W 2-way voting omnifont recognition"},
	{INFO_FRX,    "FRX FireWorx omnifont recognition"},
	{INFO_PLUS3W, "PLUS3W 3-way voting omnifont recognition"},
	{INFO_XOCR,	  "XOCR standard page parse"}
};

// RecAPI output converter
const char* pszOutputFormat = "Converters.Text.PDFImageOnText";

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
		try
		{
			// set exception handling dialog
			UCLIDExceptionDlg ue_dlg;
			UCLIDException::setExceptionHandler(&ue_dlg);

			// get valid arguments
			if(getAndValidateArguments(__argc, __argv) )
			{
				// validate licensing
				validateLicense(); 

				// do the work
				convertToSearchablePDF();

				// remove the original file if that option was specified
				if(m_bRemoveOriginal)
				{
					// Do not remove the original if the input and output files are the same
					// [LRCAU #5595]
					if (!stringCSIS::sEqual(getUNCPath(m_strInputFile), getUNCPath(m_strOutputFile)))
					{
						deleteFile(m_strInputFile);
					}
				}

				// completed successfully
				m_bIsError = false;
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI18536");
	}
	catch(UCLIDException ue)
	{
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
void CESConvertToPDFApp::convertToSearchablePDF()
{
	// initialize the RecAPI license
	// NOTE: this is separate from the Extract licensing which occurred earlier
	licenseOCREngine();

	// ensure engine resources are released when
	// mainEngineMemoryReleaser goes out of scope.
	MainRecMemoryReleaser mainEngineMemoryReleaser;

	// set the output format of the document
	RECERR rc = RecSetOutputFormat(0, pszOutputFormat);
	if (rc != REC_OK)
	{
		UCLIDException ue("ELI18580", "Unable to set output format.");
		loadScansoftRecErrInfo(ue, rc);
		throw ue;
	}

	string strOutputFormat = pszOutputFormat;

	// If either password is defined, enable 128 bit security
	if (!m_strUserPassword.empty() || !m_strOwnerPassword.empty())
	{
		setIntSetting(strOutputFormat + ".PDFSecurity.Type", R2ID_PDFSECURITY128BITS);
	}

	// If there is a user password defined, set it
	if (!m_strUserPassword.empty())
	{
		setStringSetting(strOutputFormat + ".PDFSecurity.UserPassword", m_strUserPassword);
	}
	// If there is an owner password defined, set it and the associated permissions
	if (!m_strOwnerPassword.empty())
	{
		// Set the owner password
		setStringSetting(strOutputFormat + ".PDFSecurity.OwnerPassword", m_strOwnerPassword);

		// Get security settings

		// Allow printing if either high quality or low quality is specified
		bool bAllowHighQuality = isPdfSecuritySettingEnabled(giAllowHighQualityPrinting);
		bool bAllowPrinting = bAllowHighQuality
			|| isPdfSecuritySettingEnabled(giAllowLowQualityPrinting);

		// Allow adding/modifying annotations if either allow modifications or
		// allow adding/modifying annotations is specified
		bool bAllowDocModify = isPdfSecuritySettingEnabled(giAllowDocumentModifications);
		bool bAllowAddModifyAnnot = bAllowDocModify ||
			isPdfSecuritySettingEnabled(giAllowAddingModifyingAnnotations);

		// Allow form fill in if either adding/modifying annotations is allowed or
		// filling in forms is specified
		bool bAllowForms = bAllowAddModifyAnnot
			|| isPdfSecuritySettingEnabled(giAllowFillingInFields);

		// Allow extraction for accessibility if either content copying or accessibility
		// is specified
		bool bAllowCopy = isPdfSecuritySettingEnabled(giAllowContentCopying);
		bool bAllowExtract = bAllowCopy
			|| isPdfSecuritySettingEnabled(giAllowContentCopyingForAccessibility);

		// Set the security settings
		setBoolSetting(strOutputFormat + ".PDFSecurity.EnablePrint", bAllowPrinting);
		setBoolSetting(strOutputFormat + ".PDFSecurity.EnablePrintQ", bAllowHighQuality);
		setBoolSetting(strOutputFormat + ".PDFSecurity.EnableModify", bAllowDocModify);
		setBoolSetting(strOutputFormat + ".PDFSecurity.EnableCopy", bAllowCopy);
		setBoolSetting(strOutputFormat + ".PDFSecurity.EnableExtract", bAllowExtract);
		setBoolSetting(strOutputFormat + ".PDFSecurity.EnableAdd", bAllowAddModifyAnnot);
		setBoolSetting(strOutputFormat + ".PDFSecurity.EnableForms", bAllowForms);
		setBoolSetting(strOutputFormat + ".PDFSecurity.EnableAssemble",
			isPdfSecuritySettingEnabled(giAllowDocumentAssembly));
	}

	// If output is to be PDF/A compliant then need to set the PDF/A compatibility mode
	if (m_bPDFA)
	{
		setIntSetting(strOutputFormat + ".Compatibility", R2ID_PDFA);
	}

	// create temporary OmniPage document file.
	// file will be automatically deleted when tfnDocument goes out of scope.
	TemporaryFileName tfnDocument(true, "", ".opd", true);

	// create output document
	HDOC hDoc;
	rc = RecCreateDoc(0, tfnDocument.getName().c_str(), &hDoc, 0);
	if (rc != REC_OK)
	{
		UCLIDException ue("ELI18586", "Unable to create output document.");
		loadScansoftRecErrInfo(ue, rc);
		throw ue;
	}

	// ensure the output document is closed when the memory releaser object goes out of scope
	RecMemoryReleaser<tagRECDOCSTRUCT> outputDocumentMemoryReleaser(hDoc);

	// Get the retry count and timeout
	int iRetryCount(-1), iRetryTimeout(-1);
	getFileAccessRetryCountAndTimeout(iRetryCount, iRetryTimeout);

	// open the input image file
	HIMGFILE hInputFile = NULL;
	int iNumRetries = 0;
	rc = kRecOpenImgFile(m_strInputFile.c_str(), &hInputFile, IMGF_READ, FF_SIZE);
	while (rc != REC_OK)
	{
		// Increment the retry count and try again
		iNumRetries++;
		rc = kRecOpenImgFile(m_strInputFile.c_str(), &hInputFile, IMGF_READ, FF_SIZE);

		// If opened successfully, log an application trace and break from the loop
		if(rc == REC_OK)
		{
			UCLIDException ue("ELI28853", "Application Trace: Opened image after retrying.");
			ue.addDebugInfo("Number of retries", iNumRetries);
			ue.addDebugInfo("Image Name", m_strInputFile);
			ue.log();

			// Exit the while loop
			break;
		}
		// Check if the error is not IMF_OPEN_ERROR, if not then throw an exception
		else if (rc != IMF_OPEN_ERR)
		{
			UCLIDException ue("ELI18587", "Unable to open input file.");
			loadScansoftRecErrInfo(ue, rc);
			ue.addDebugInfo("Input filename", m_strInputFile);
			throw ue;
		}

		// Check the retry count
		if(iNumRetries < iRetryCount)
		{
			// Sleep and retry
			Sleep(iRetryTimeout);
		}
		else
		{
			// Reached max retry count, throw an exception
			UCLIDException ue("ELI28854", "Unable to open input file after retrying.");
			loadScansoftRecErrInfo(ue, rc);
			ue.addDebugInfo("Image Name", m_strInputFile);
			ue.addDebugInfo("Number of retries", iNumRetries);
			ue.addDebugInfo("Max number of retries", iRetryCount);
			throw ue;
		}
	}

	// ensure that the memory for the input file is released when the object goes out of scope
	RecMemoryReleaser<tagIMGFILEHANDLE> inputImageFileReleaser(hInputFile);

	// get the page count
	int iPages;
	rc = kRecGetImgFilePageCount(hInputFile, &iPages);
	if(rc != REC_OK)
	{
		// log an error
		UCLIDException ue("ELI18614", "Unable to get page count.");
		loadScansoftRecErrInfo(ue, rc);
		ue.addDebugInfo("Input filename", m_strInputFile);
		throw ue;
	}

	// iterate through each page
	HPAGE hPage;
	for(int i=0; i<iPages; i++)  
	{
		// load the ith page
		loadPageFromImageHandle(m_strInputFile, hInputFile, i, &hPage);

		try
		{
			try
			{
				// recognize the text on this page
				rc = kRecRecognize(0, hPage, 0);
				if (rc != REC_OK && rc != NO_TXT_WARN && rc != ZONE_NOTFOUND_ERR)
				{
					// log an error
					UCLIDException ue("ELI18589", "Unable to recognize text on page.");
					loadScansoftRecErrInfo(ue, rc);
					ue.addDebugInfo("Input filename", m_strInputFile);
					ue.addDebugInfo("Page number", i+1);

					// add page size information [P13 #4603]
					if(rc == IMG_SIZE_ERR)
					{
						addPageSizeDebugInfo(ue, hInputFile, i);
					}

					throw ue;
				}
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI18628");
		}
		catch(UCLIDException ue)
		{
			// free the memory allocated for this page
			rc = kRecFreeImg(hPage);

			// log any errors
			if (rc != REC_OK)
			{
				UCLIDException ue2("ELI18629", "Application trace: Unable to release page image. "
					"Possible memory leak.");
				loadScansoftRecErrInfo(ue2, rc);
				ue2.addDebugInfo("Input filename", m_strInputFile);
				ue2.addDebugInfo("Page number", i+1);
				ue2.log();
			}

			// throw the original exception
			throw ue;
		}

		// add this page to the output document
		// NOTE: After this call, the memory allocated for hPage is now managed by the engine.
		// It is important not to release it.
		rc = RecInsertPage(0, hDoc, hPage, i);
		if (rc != REC_OK)
		{
			// log an error
			UCLIDException ue("ELI18788", "Unable to add page to output document.");
			loadScansoftRecErrInfo(ue, rc);
			ue.addDebugInfo("Input filename", m_strInputFile);
			ue.addDebugInfo("Page number", i+1);
			throw ue;
		}
	}

	// convert document to searchable pdf
	rc = RecConvert2Doc(0, hDoc, m_strOutputFile.c_str());
	if (rc != REC_OK)
	{
		// log an error
		UCLIDException ue("ELI18590", "Unable to convert document to searchable pdf.");
		loadScansoftRecErrInfo(ue, rc);
		ue.addDebugInfo("Input filename", m_strInputFile);
		ue.addDebugInfo("Output filename", m_strOutputFile);
		throw ue;
	}

	// Make sure the file can be read
	waitForFileToBeReadable(m_strOutputFile);
}
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

	bool bEncrypted = false;
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
			bEncrypted = true;
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
	m_strInputFile = argv[1]; 
	m_strOutputFile = argv[2];

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

	// If the passwords were encrypted, decrypt them now
	if (bEncrypted)
	{
		if (!m_strUserPassword.empty())
		{
			decryptString(m_strUserPassword);
		}
		if (!m_strOwnerPassword.empty())
		{
			decryptString(m_strOwnerPassword);
		}
	}

	// if we reached this far, the arguments were valid
	return true;
}
//-------------------------------------------------------------------------------------------------
void CESConvertToPDFApp::decryptString(string& rstrEncryptedString)
{
	// Build the key
	ByteStream bytesKey;
	ByteStreamManipulator bytesManipulatorKey(
		ByteStreamManipulator::kWrite, bytesKey);
	bytesManipulatorKey << gulPdfKey1;
	bytesManipulatorKey << gulPdfKey2;
	bytesManipulatorKey << gulPdfKey3;
	bytesManipulatorKey << gulPdfKey4;
	bytesManipulatorKey.flushToByteStream( 8 );

	// Decrypt the string
	ByteStream bytes(rstrEncryptedString);
	ByteStream decrypted;
	MapLabel encryptionEngine;
	encryptionEngine.getMapLabel(decrypted, bytes, bytesKey);

	// Get the decrypted string
	ByteStreamManipulator bsm (ByteStreamManipulator::kRead, decrypted);
	bsm >> rstrEncryptedString;
}
//-------------------------------------------------------------------------------------------------
void CESConvertToPDFApp::licenseOCREngine()
{
	// the license file is expected in the same directory as this exe.
	// get the location of this exe.
	string strLicFile( getCurrentProcessEXEDirectory() );
	strLicFile += "\\";
	strLicFile += gpszLICENSE_FILE_NAME;

	// set the RecAPI engine license
	RECERR rc = kRecSetLicense(strLicFile.c_str(), gpszOEM_KEY);
	if(rc != REC_OK && rc != API_INIT_WARN)
	{
		UCLIDException ue("ELI18566", "Unable to load OCR engine license file.");
		loadScansoftRecErrInfo(ue, rc);
		throw ue;
	}

	// initialize RecAPI Plus
	rc = RecInitPlus("Extract Systems", "ESConvertToPDF");
	if (rc != REC_OK)
	{
		// build an exception to store this error information
		UCLIDException ue("ELI18567", "Unable to initialize OCR engine.");
		loadScansoftRecErrInfo(ue, rc);

		// check if this is only a warning
		if(rc == API_INIT_WARN)
		{
			// this is only a warning, no need to throw an exception yet
			bool bThrowException = false;

			// get information about the initialized modules
			LPKRECMODULEINFO pModules;
			size_t size;
			RECERR rc = kRecGetModulesInfo(&pModules, &size);

			// ensure the modules were retrieved
			if(rc != REC_OK)
			{
				// add this error information to the original
				UCLIDException uexOuter("ELI18569", "Unable to get OCR module information.", ue);
				loadScansoftRecErrInfo(uexOuter, rc);
				
				// throw all exception information together
				throw uexOuter;
			}

			// check if required modules are present
			for(int i=0; i<giNUM_REQUIRED_MODULES; i++)
			{
				// module is present if the version number is non-zero
				if(pModules[gREQUIRED_MODULES[i].eModule].Version <= 0)
				{
					// add the debug information about this module
					ue.addDebugInfo("Missing module", gREQUIRED_MODULES[i].strModuleDescription);

					// set the flag to throw an exception
					bThrowException = true;
				}
			}

			// throw an exception if at least one required module is not present,
			// otherwise it is okay to ignore the API_INIT_WARN.
			if(bThrowException)
			{
				throw ue;
			}
		}
		else
		{
			// this wasn't a warning. it's an error.
			throw ue;
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CESConvertToPDFApp::setStringSetting(const string& strSetting, const string& strValue)
{
	try
	{
		try
		{
			HSETTING hSetting;
			RECERR rc = kRecSettingGetHandle(NULL, strSetting.c_str(), &hSetting, NULL);
			if (rc != REC_OK)
			{
				UCLIDException ue("ELI29758", "Unable to get the OCR engine setting.");
				loadScansoftRecErrInfo(ue, rc);
				throw ue;
			}

			STSTYPES type;
			rc = kRecSettingGetType(hSetting, &type);
			if (rc != REC_OK)
			{
				UCLIDException ue("ELI29764", "Unable to get setting type.");
				loadScansoftRecErrInfo(ue, rc);
				throw ue;
			}

			if (type == STS_STRING)
			{
				rc = kRecSettingSetString(0, hSetting, strValue.c_str());
				if (rc != REC_OK)
				{
					UCLIDException ue("ELI29759", "Unable to set OCR engine setting value.");
					loadScansoftRecErrInfo(ue, rc);
					throw ue;
				}
			}
			else if (type == STS_USTRING)
			{
				// Need a wide character string, copy string to _bstr_t
				_bstr_t bstrTemp = strValue.c_str();
				rc = kRecSettingSetUString(0, hSetting, bstrTemp);
				if (rc != REC_OK)
				{
					UCLIDException ue("ELI29765", "Unable to set OCR engine setting value.");
					loadScansoftRecErrInfo(ue, rc);
					throw ue;
				}
			}
			else
			{
				UCLIDException ue("ELI29768", "Specified setting is not a string type.");
				ue.addDebugInfo("Setting Type", type);
				throw ue;
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29769");
	}
	catch(UCLIDException& ue)
	{
		ue.addDebugInfo("Setting Name", strSetting);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void CESConvertToPDFApp::setBoolSetting(const string& strSetting, bool bValue)
{
	setIntSetting(strSetting, asMFCBool(bValue));
}
//-------------------------------------------------------------------------------------------------
void CESConvertToPDFApp::setIntSetting(const string& strSetting, int nValue)
{
	HSETTING hSetting;
	RECERR rc = kRecSettingGetHandle(NULL, strSetting.c_str(), &hSetting, NULL);
	if (rc != REC_OK)
	{
		UCLIDException ue("ELI29760", "Unable to get the OCR engine setting.");
		loadScansoftRecErrInfo(ue, rc);
		ue.addDebugInfo("Setting Name", strSetting, false);
		throw ue;
	}

	rc = kRecSettingSetInt(0, hSetting, nValue);
	if (rc != REC_OK)
	{
		UCLIDException ue("ELI29761", "Unable to set OCR engine setting value.");
		loadScansoftRecErrInfo(ue, rc);
		ue.addDebugInfo("Setting Name", strSetting, false);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
bool CESConvertToPDFApp::isPdfSecuritySettingEnabled(int nSetting)
{
	return isFlagSet(m_nPermissions, nSetting);
}
//-------------------------------------------------------------------------------------------------
void CESConvertToPDFApp::validateLicense()
{
	// load the license files
	LicenseManagement::loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);

	// ensure this feature is licensed
	VALIDATE_LICENSE(gnCREATE_SEARCHABLE_PDF_FEATURE, "ELI18710", "ESConvertToPDF");
}
//-------------------------------------------------------------------------------------------------
