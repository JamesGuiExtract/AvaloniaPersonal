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

// RecAPI settings class that has the settings that govern PDF output.
string strOUTPUT_SETTINGS_CLASS = "Kernel.Imf.PDF";

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
				licenseOCREngine();

				applyOCRSettings();

				bool bUseRecDFAPI = !applySecuritySettings();
				bool bUseLegacyAPI = !bUseRecDFAPI;

				// Use RecDFAPI when possible as it allows for images to be preserved without
				// altering them in any way. Cannot be used if security settings are to be used.
				if (bUseRecDFAPI)
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
void CESConvertToPDFApp::applyOCRSettings()
{
	// OCR should be accurate rather than fast.
	setIntSetting("Kernel.OcrMgr.PDF.TradeOff", TO_ACCURATE);

	// Use the more accurate 3-way voting engine (rather than the default 2-way voting engine).
	setBoolSetting("Kernel.OcrMgr.PreferAccurateEngine", true);
	
	// OCR should be performed only on images.
	setIntSetting("Kernel.OcrMgr.PDF.ProcessingMode", PDF_PM_GRAPHICS_ONLY);
}
//-------------------------------------------------------------------------------------------------
void CESConvertToPDFApp::convertToSearchablePDF(bool bUseRecDFAPI)
{
	// ensure engine resources are released when
	// mainEngineMemoryReleaser goes out of scope.
	MainRecMemoryReleaser mainEngineMemoryReleaser;

	// Preserve the original resolution in the output PDF.
	setBoolSetting(strOUTPUT_SETTINGS_CLASS + ".LoadOriginalDPI", true);

	// If output is to be PDF/A compliant then need to set the PDF/A compatibility mode
	// Adding searchable text with the RecPDFAPI breaks 1a and 1b compliance, but we seem to be
	// 2a compliant. (confirmed using:
	// http://www.datalogics.com/products/callas/callaspdfA-onlinedemo.asp, though the RecPDFAPI
	// does not make any claims with regards to PDF/A compliance.)
	if (m_bPDFA)
	{
		setIntSetting(strOUTPUT_SETTINGS_CLASS + ".Compatibility", R2ID_PDFA2A);
	}

	// Create a temporary output PDF file. This file will be copied to the final output location
	// once the process is complete.
	TemporaryFileName tfnDocument(true, "", ".pdf", true);

	// open the input image file
	HIMGFILE hInputFile = openImageFile(m_strInputFile);

	// ensure that the memory for the input file is released when the object goes out of scope
	RecMemoryReleaser<tagIMGFILEHANDLE> inputImageFileReleaser(hInputFile);

	IMG_INFO imgInfo = {0};
	IMF_FORMAT imgFormat;
	RECERR rc = kRecGetImgFilePageInfo(0, hInputFile, 0, &imgInfo, &imgFormat);
	throwExceptionIfNotSuccess(rc, "ELI36759", "Failed to indentify image format.", m_strInputFile);

	int nPageCount;
	rc = kRecGetImgFilePageCount(hInputFile, &nPageCount);
	throwExceptionIfNotSuccess(rc, "ELI36757", "Unable to get page count.", m_strInputFile);

	// The returned HPAGE instaces wil have OCR text that can be applied to an output document.
	HPAGE *pPages = getOCRedPages(hInputFile, nPageCount);

	if (bUseRecDFAPI)
	{
		// https://extract.atlassian.net/browse/ISSUE-11940
		// If the source document was a PDF, the OCR text can be added to the original document
		// without touching the images at all (preventing any possible degradation in quality).
		if (imgFormat >= FF_PDF_MIN && imgFormat <= FF_PDF_MRC_LOSSLESS)
		{
			copyFile(m_strInputFile, tfnDocument.getName());
		}
		else
		{
			// FF_PDF_SUPERB was causing unacceptable growth in PDF size in some cases for color
			// documents. For the time being, unless a document is bitonal, use FF_PDF_GOOD rather than
			// FF_PDF_SUPERB.
			IMF_FORMAT outFormat = imgInfo.BitsPerPixel == 1 ? FF_PDF_SUPERB : FF_PDF_GOOD;

			// Save all the document pages into a new output document.
			addPagesToOutput(pPages, tfnDocument.getName().c_str(), outFormat, nPageCount);
		}

		// Apply the OCR from pages to the output document.
		applySearchableTextWithRecAPI(tfnDocument.getName().c_str(), pPages, nPageCount);
	}
	else
	{
		// If not using RecAPI, use the RecAPI to convert to searchable PDF.
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

		rc = kRecConvert2DTXT(0, pPages, nPageCount, tfnDocument.getName().c_str());
		throwExceptionIfNotSuccess(rc, "ELI36846", "Failed to output document.", m_strInputFile);
	}

	freePageData(pPages, nPageCount);

	// Copy the temporary output file to its final output location.
	copyFile(tfnDocument.getName(), m_strOutputFile);

	// Make sure the file can be read
	waitForFileToBeReadable(m_strOutputFile);
}
//-------------------------------------------------------------------------------------------------
bool CESConvertToPDFApp::applySecuritySettings()
{
	if (m_strUserPassword.empty() && m_strOwnerPassword.empty())
	{
		return false;	
	}

	// If either password is defined, enable 128 bit security
	setIntSetting(strOUTPUT_SETTINGS_CLASS + ".PDFSecurity.Type", R2ID_PDFSECURITY128BITS);

	// If there is a user password defined, set it
	if (!m_strUserPassword.empty())
	{
		setStringSetting(strOUTPUT_SETTINGS_CLASS + ".PDFSecurity.UserPassword", m_strUserPassword);
	}
	// If there is an owner password defined, set it and the associated permissions
	if (!m_strOwnerPassword.empty())
	{
		// Set the owner password
		setStringSetting(strOUTPUT_SETTINGS_CLASS + ".PDFSecurity.OwnerPassword", m_strOwnerPassword);

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
		setBoolSetting(strOUTPUT_SETTINGS_CLASS + ".PDFSecurity.EnablePrint", bAllowPrinting);
		setBoolSetting(strOUTPUT_SETTINGS_CLASS + ".PDFSecurity.EnablePrintQ", bAllowHighQuality);
		setBoolSetting(strOUTPUT_SETTINGS_CLASS + ".PDFSecurity.EnableModify", bAllowDocModify);
		setBoolSetting(strOUTPUT_SETTINGS_CLASS + ".PDFSecurity.EnableCopy", bAllowCopy);
		setBoolSetting(strOUTPUT_SETTINGS_CLASS + ".PDFSecurity.EnableExtract", bAllowExtract);
		setBoolSetting(strOUTPUT_SETTINGS_CLASS + ".PDFSecurity.EnableAdd", bAllowAddModifyAnnot);
		setBoolSetting(strOUTPUT_SETTINGS_CLASS + ".PDFSecurity.EnableForms", bAllowForms);
		setBoolSetting(strOUTPUT_SETTINGS_CLASS + ".PDFSecurity.EnableAssemble",
			isPdfSecuritySettingEnabled(giAllowDocumentAssembly));
	}

	return true;
}
//-------------------------------------------------------------------------------------------------
HIMGFILE CESConvertToPDFApp::openImageFile(const string& strFileName)
{
	// Get the retry count and timeout
	int iRetryCount(-1), iRetryTimeout(-1);
	getFileAccessRetryCountAndTimeout(iRetryCount, iRetryTimeout);

	HIMGFILE hInputFile = NULL;
	int iNumRetries = 0;
	RECERR rc = kRecOpenImgFile(strFileName.c_str(), &hInputFile, IMGF_READ, FF_SIZE);
	while (rc != REC_OK)
	{
		// Increment the retry count and try again
		iNumRetries++;
		rc = kRecOpenImgFile(strFileName.c_str(), &hInputFile, IMGF_READ, FF_SIZE);

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

	return hInputFile;
}
//-------------------------------------------------------------------------------------------------
HPAGE* CESConvertToPDFApp::getOCRedPages(HIMGFILE hInputFile, int nPageCount)
{
	HPAGE *pPages = new HPAGE[nPageCount];
	for(int i = 0; i < nPageCount; i++)  
	{
		HPAGE& hPage = pPages[i];

		// load the ith page
		loadPageFromImageHandle(m_strInputFile, hInputFile, i, &hPage);

		try
		{
			try
			{
				// recognize the text on this page
				RECERR rc = kRecRecognize(0, hPage, 0);
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
			// [LegacyRCAndUtils:6363]
			// Rather than abort the entire conversion of OCR fails on a given page, simply
			// log the exception and let the conversion complete (albeit without searchable text
			// on this page.
			ue.log();
		}
	}

	return pPages;
}
//-------------------------------------------------------------------------------------------------
void CESConvertToPDFApp::addPagesToOutput(HPAGE *pPages, const string& strOutputPDF, 
										  IMF_FORMAT outFormat, int nPageCount)
{
	// Add each page to the output document.
	for(int i = 0; i < nPageCount; i++)  
	{
		HPAGE hPage = pPages[i];

		RECERR rc = kRecSaveImgFA(0, strOutputPDF.c_str(), outFormat, hPage, II_CURRENT, true);
		throwExceptionIfNotSuccess(rc, "ELI36753", "Failed to save document page.",
			m_strInputFile, i + 1);
	}
}
//-------------------------------------------------------------------------------------------------
void CESConvertToPDFApp::freePageData(HPAGE *pPages, int nPageCount)
{
	for(int i = 0; i < nPageCount; i++)  
	{
		HPAGE& hPage = pPages[i];
		RECERR rc = kRecFreeImg(hPage);
		if (rc != REC_OK)
		{
			UCLIDException ue("ELI36749", 
				"Application trace: Unable to release page image. Possible memory leak.");
			loadScansoftRecErrInfo(ue, rc);
			ue.log();
		}
	}

	delete pPages;
}
//-------------------------------------------------------------------------------------------------
void CESConvertToPDFApp::applySearchableTextWithRecAPI(const string& strImageFile, HPAGE *pages, 
													   int nPageCount)
{
	RECERR rc = rPdfInit();
	throwExceptionIfNotSuccess(rc, "ELI36754", "Unable to initialize PDF processing engine.");

	RPDF_DOC pdfDoc;
	rc = rPdfOpen(strImageFile.c_str(), __nullptr, &pdfDoc);
	throwExceptionIfNotSuccess(rc, "ELI36744", "Failed to open document as PDF.", m_strInputFile);

	RPDF_OPERATION op;
	rc = rPdfOpStart(&op);
	throwExceptionIfNotSuccess(rc, "ELI36745", "Failed to start PDF operation.", m_strInputFile);

	rc = rPdfOpAddFile(op, pdfDoc);
	throwExceptionIfNotSuccess(rc, "ELI36746", "Failed to initialize PDF operation.", m_strInputFile);

	rc = rPdfOpMergeTextToPages(op, pdfDoc, 0, pages, nPageCount);
	throwExceptionIfNotSuccess(rc, "ELI36747", "Failed to add searchable PDF text.", m_strInputFile);

	rc = rPdfOpExecute(op);
	throwExceptionIfNotSuccess(rc, "ELI36748", "Failed to execute PDF operation.", m_strInputFile);

	rc = rPdfClose(pdfDoc);
	throwExceptionIfNotSuccess(rc, "ELI36750", "Failed to close PDF document.", m_strInputFile);

	rc = rPdfQuit();
	throwExceptionIfNotSuccess(rc, "ELI36751", "Failed to shut down PDF processing engine.",
		m_strInputFile);

	// RecPDF API calls to add searchable text can result in corrupted images:
	// https://extract.atlassian.net/browse/ISSUE-12163
	// Validate the output file can be read.
	validatePDF(strImageFile.c_str());
}
//-------------------------------------------------------------------------------------------------
void CESConvertToPDFApp::validatePDF(const string& strFileName)
{
	int i = 0;
	try
	{
		try
		{
			HIMGFILE hFile = openImageFile(strFileName);

			// ensure that the memory for the input file is released when the object goes out of scope
			RecMemoryReleaser<tagIMGFILEHANDLE> inputImageFileReleaser(hFile);

			int nPageCount;
			RECERR rc = kRecGetImgFilePageCount(hFile, &nPageCount);
			throwExceptionIfNotSuccess(rc, "ELI36842", "Unable to get page count.", m_strInputFile);

			for(i = 0; i < nPageCount; i++)  
			{
				HPAGE hPage;
				loadPageFromImageHandle(m_strInputFile, hFile, i, &hPage);

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
