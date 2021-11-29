// ESConvertToPDF.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "ESConvertToPDF.h"
#include "ScansoftErr.h"
#include "RecAPIManager.h"
#include "OcrMethods.h"
#include <UCLIDException.h>
#include <UCLIDExceptionDlg.h>
#include <TemporaryFileName.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <OCRConstants.h>
#include <RecAPIPlus.h>
#include <Recpdf.h>
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
				// Validate and initiate licensing
				validateLicense(); 

				// If the output does not need to be PDF/A compliant and does not need security,
				// the RecPDF API can be used to try to add searchable text without touching the
				// source image (if the source image is a PDF).
				bool bUseRecPDFAPI =
					!m_bPDFA && m_strUserPassword.empty() && m_strOwnerPassword.empty();
				// Otherwise, use the legacy RecAPI which will re-build the document (convert/modify
				// the images.
				bool bUseLegacyAPI = !bUseRecPDFAPI;

				if (bUseRecPDFAPI)
				{
					try
					{
						try
						{
							convertToSearchablePDF(true);
						}
						CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI36761");
					}
					catch (UCLIDException &ue)
					{
						UCLIDException ueOuter("ELI36762",
							"Application trace: Convert to searchable failed. Attempting legacy method...",
							ue);
						ueOuter.addDebugInfo("Filename", m_strInputFile);
						ueOuter.log();
						
						bUseLegacyAPI = true;
					}
				}

				// If RecPDF API could not be used, or conversion using RecAPI failed, use the
				// legacy RecAPI instead.
				if (bUseLegacyAPI)
				{
					convertToSearchablePDF(false);
				}

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
void CESConvertToPDFApp::convertToSearchablePDF(bool bUseRecPdfApi)
{
	// initialize the RecAPI license
	// NOTE: this is separate from the Extract licensing which occurred earlier
	licenseOCREngine();

	// Use Auto for the legacy method so that existing text is preserved
	// https://extract.atlassian.net/browse/ISSUE-17173
	PDF_PROC_MODE processingMode = bUseRecPdfApi
		? PDF_PM_GRAPHICS_ONLY
		: PDF_PM_AUTO;

	// https://extract.atlassian.net/browse/ISSUE-12184
	// When using bUseRecDFAPI, a new CRecAPIManager instance will be generated and used after
	// every [g_nPAGE_BATCH_SIZE] pages to ensure that memory usage is not excessive when processing
	// large documents.
	auto apRecAPIManager = make_unique<CRecAPIManager>(this, m_strInputFile, processingMode);

	IMG_INFO imgInfo = {0};
	IMF_FORMAT imgFormat;
	apRecAPIManager->getImageInfo(imgInfo, imgFormat);
	int nPageCount = apRecAPIManager->getPageCount();

	// Create a temporary output PDF file. This file will be copied to the final output location
	// once the process is complete.
	TemporaryFileName tfnDocument(true, "", ".pdf", true);
	RPDF_DOC pdfDoc;

	// https://extract.atlassian.net/browse/ISSUE-11940
	// If the source document was a PDF, the OCR text can be added to the original document
	// without touching the images at all (preventing any possible degradation in quality).
	bool bSourceIsPDF = imgFormat == FF_PDF
		|| imgFormat == FF_PDF_MRC
		|| ((imgFormat >= FF_PDF_MIN) && (imgFormat <= FF_PDF_MRC_LOSSLESS));

	// If the PDF will be created from an HPAGE array then it is necessary to preserve the original pages
	// so that the output can specify II_ORIGINAL (II_CURRENT would mean pages rotated by the preprocessing step)
	if (!bSourceIsPDF || !bUseRecPdfApi)
	{
		RECERR rc = kRecSetPreserveOriginalImg(0, TRUE);
		throwExceptionIfNotSuccess(rc, "ELI50259", "Failed to set preserve original image setting.", m_strInputFile);
	}

	if (bUseRecPdfApi)
	{
		RECERR rc = rPdfInit();
		throwExceptionIfNotSuccess(rc, "ELI36754", "Unable to initialize PDF processing engine.");

		if (bSourceIsPDF)
		{
			copyFile(m_strInputFile, tfnDocument.getName());

			rc = rPdfOpen(tfnDocument.getName().c_str(), __nullptr, &pdfDoc);
			throwExceptionIfNotSuccess(rc, "ELI36744", "Failed to open document as PDF.", m_strInputFile);
		}

		// FF_PDF_SUPERB was causing unacceptable growth in PDF size in some cases for color
		// documents. For the time being, unless a document is bitonal, use FF_PDF_GOOD rather than
		// FF_PDF_SUPERB.
		IMF_FORMAT outFormat = imgInfo.BitsPerPixel == 1 ? FF_PDF_SUPERB : FF_PDF_GOOD;

		for (int i = 0; i < nPageCount; i += g_nPAGE_BATCH_SIZE)
		{
			if (i != 0)
			{
				// Free all RecAPI memory and re-initialize the API for the next g_nPAGE_BATCH_SIZE
				// pages.
				apRecAPIManager = make_unique<CRecAPIManager>(this, m_strInputFile, processingMode);
			}

			int nPagesToProcess = min(g_nPAGE_BATCH_SIZE, nPageCount - i);

			// The returned HPAGE instances will have OCR text that can be applied to an output document.
			HPAGE *pPages = apRecAPIManager->getOCRedPages(i, nPagesToProcess);
			
			if (!bSourceIsPDF)
			{
				// Save all the document pages into a new output document.
				addPagesToOutput(pPages, tfnDocument.getName().c_str(), outFormat, nPagesToProcess);
				
				// If this is not the first set of pages, the pdfDoc instance used for the last set
				// needs to be closed before initializing for the next set of pages.
				if (i != 0)
				{
					rc = rPdfClose(pdfDoc);
					throwExceptionIfNotSuccess(rc, "ELI37020",
						"Failed to close PDF document.", m_strInputFile);
				}
				
				rc = rPdfOpen(tfnDocument.getName().c_str(), __nullptr, &pdfDoc);
				throwExceptionIfNotSuccess(rc, "ELI37021",
					"Failed to open document as PDF.", m_strInputFile);

			}

			// Apply the OCR from pages to the output document.
			applySearchableTextWithRecPDFAPI(pdfDoc, pPages, i, nPagesToProcess);
		}

		rc = rPdfClose(pdfDoc);
		throwExceptionIfNotSuccess(rc, "ELI36750", "Failed to close PDF document.", m_strInputFile);

		rc = rPdfQuit();
		throwExceptionIfNotSuccess(rc, "ELI36751", "Failed to shut down PDF processing engine.",
			m_strInputFile);

		// RecPDF API calls to add searchable text can result in corrupted images:
		// https://extract.atlassian.net/browse/ISSUE-12163
		// Validate the output file can be read.
		validatePDF(tfnDocument.getName().c_str());
	}
	else
	{
		HPAGE *pPages = apRecAPIManager->getOCRedPages(0, nPageCount);

		// If not using the PDF API, use RecAPI to convert to searchable PDF.
		RECERR rc = kRecSetDTXTFormat(0, DTXT_PDFIOT);
		throwExceptionIfNotSuccess(rc, "ELI36845", "Unable to set direct text format.", m_strInputFile);

		setIntSetting("Kernel.DTxt.PDF.BWFormat", 2); // TIF_G4
		setIntSetting("Kernel.DTxt.PDF.BWQuality", IMF_IMAGEQUALITY_SUPERB);

		// IMF_IMAGEQUALITY_SUPERB was causing unacceptable growth in PDF size in some cases for
		// color documents. For the time being, unless a document is bitonal, use
		// IMF_IMAGEQUALITY_GOOD rather than IMF_IMAGEQUALITY_SUPERB.
		setIntSetting("Kernel.DTxt.PDF.ColorQuality", IMF_IMAGEQUALITY_GOOD);

		// kRecConvert2DTXT is going to expect that there is not already a file in the output
		// location.
		deleteFile(tfnDocument.getName().c_str());

		rc = kRecConvert2DTXTEx(0, pPages, nPageCount, II_ORIGINAL, tfnDocument.getName().c_str());

		// Errors are negative, warnings are positive, OK is zero (see RECERR.h)
		bool isError = rc < 0;
		bool isWarning = rc > 0;

		if (isError)
		{
			throwExceptionIfNotSuccess(rc, "ELI36846", "Failed to output document", m_strInputFile);
		}
		else if (isWarning)
		{
			UCLIDException ue("ELI52993", "Application trace: A warning was reported while creating a searchable PDF");
			loadScansoftRecErrInfo(ue, rc);
			ue.addDebugInfo("Input file", m_strInputFile);
			ue.addDebugInfo("Output file", m_strOutputFile);
			ue.log();
		}
	}

	// Copy the temporary output file to its final output location.
	copyFile(tfnDocument.getName(), m_strOutputFile);

	// Make sure the file can be read
	waitForFileToBeReadable(m_strOutputFile);
}
//-------------------------------------------------------------------------------------------------
void CESConvertToPDFApp::addPagesToOutput(HPAGE *pPages, const string& strOutputPDF, 
										  IMF_FORMAT outFormat, int nPageCount)
{
	// Add each page to the output document.
	for(int i = 0; i < nPageCount; i++)  
	{
		HPAGE hPage = pPages[i];

		RECERR rc = kRecSaveImgFA(0, strOutputPDF.c_str(), outFormat, hPage, II_ORIGINAL, true);
		throwExceptionIfNotSuccess(rc, "ELI36753", "Failed to save document page.",
			m_strInputFile, i + 1);
	}
}
//-------------------------------------------------------------------------------------------------
void CESConvertToPDFApp::applySearchableTextWithRecPDFAPI(RPDF_DOC pdfDoc, HPAGE *pages,
													   int nStartPage, int nPageCount)
{
	RPDF_OPERATION op;
	RECERR rc = rPdfOpStart(&op);
	throwExceptionIfNotSuccess(rc, "ELI37016", "Failed to start PDF operation.", m_strInputFile);

	rc = rPdfOpAddFile(op, pdfDoc);
	throwExceptionIfNotSuccess(rc, "ELI37017", "Failed to initialize PDF operation.", m_strInputFile);

	rc = rPdfOpMergeTextToPages(op, pdfDoc, nStartPage, pages, nPageCount);
	throwExceptionIfNotSuccess(rc, "ELI37018", "Failed to add searchable PDF text.", m_strInputFile);

	rc = rPdfOpExecute(op);
	throwExceptionIfNotSuccess(rc, "ELI37019", "Failed to execute PDF operation.", m_strInputFile);
}
//-------------------------------------------------------------------------------------------------
void CESConvertToPDFApp::validatePDF(const string& strFileName)
{
	int i = 0;
	try
	{
		try
		{
			CRecAPIManager recAPIManager(this, strFileName, PDF_PM_NORMAL);

			int nPageCount = recAPIManager.getPageCount();

			for(i = 0; i < nPageCount; i++)  
			{
				HPAGE hPage;
				loadPageFromImageHandle(strFileName, recAPIManager.m_hFile, i, &hPage);

				RECERR rc = kRecFreeImg(hPage);
				if (rc != REC_OK)
				{
					UCLIDException ue("ELI36841",
						"Application trace: Unable to release page image. Possible memory leak.");
					loadScansoftRecErrInfo(ue, rc);
					ue.log();
				}
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI36843");
	}
	catch (UCLIDException &ue)
	{
		UCLIDException uexOuter("ELI36844", "Output PDF validation failed", ue);
		throw uexOuter;
	}
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
	// set the RecAPI engine license
	RECERR rc = kRecSetLicense(__nullptr, gpszOEM_KEY);
	if(rc != REC_OK && rc != API_INIT_WARN)
	{
		UCLIDException ue("ELI18566", "Unable to load OCR engine license file.");
		loadScansoftRecErrInfo(ue, rc);
		throw ue;
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
