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
// CESConvertToPDF
//-------------------------------------------------------------------------------------------------
CESConvertToPDF::CESConvertToPDF(
	std::string inputFile,
	std::string outputFile,
	bool removeOriginal,
	bool outputPdfA,
	std::string userPassword,
	std::string ownerPassword,
	bool passwordsAreEncrypted,
	long permissions) :

	m_strInputFile(inputFile),
	m_strOutputFile(outputFile),
	m_bRemoveOriginal(removeOriginal),
	m_bPDFA(outputPdfA),
	m_bIsError(true),          // assume error until successfully completed
	m_strUserPassword(userPassword),
	m_strOwnerPassword(ownerPassword),
	m_nPermissions(permissions)
{
	// If the passwords were encrypted, decrypt them now
	if (passwordsAreEncrypted)
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
}

void CESConvertToPDF::ConvertToPDF()
{
	try
	{
		validateLicense();
		validateConfiguration();

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
			catch (UCLIDException& ue)
			{
				if (isExceptionFromNLSFailure(ue))
				{
					throw;
				}

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
		if (m_bRemoveOriginal)
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
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI53643");
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CESConvertToPDF::convertToSearchablePDF(bool bUseRecPdfApi)
{
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

	IMG_INFO imgInfo = { 0 };
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
			HPAGE* pPages = apRecAPIManager->getOCRedPages(i, nPagesToProcess);

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
		HPAGE* pPages = apRecAPIManager->getOCRedPages(0, nPageCount);

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
void CESConvertToPDF::addPagesToOutput(HPAGE* pPages, const string& strOutputPDF,
	IMF_FORMAT outFormat, int nPageCount)
{
	// Add each page to the output document.
	for (int i = 0; i < nPageCount; i++)
	{
		HPAGE hPage = pPages[i];

		RECERR rc = kRecSaveImgFA(0, strOutputPDF.c_str(), outFormat, hPage, II_ORIGINAL, true);
		throwExceptionIfNotSuccess(rc, "ELI36753", "Failed to save document page.",
			m_strInputFile, i + 1);
	}
}
//-------------------------------------------------------------------------------------------------
void CESConvertToPDF::applySearchableTextWithRecPDFAPI(RPDF_DOC pdfDoc, HPAGE* pages,
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
void CESConvertToPDF::validatePDF(const string& strFileName)
{
	int i = 0;
	try
	{
		try
		{
			CRecAPIManager recAPIManager(this, strFileName, PDF_PM_NORMAL);

			int nPageCount = recAPIManager.getPageCount();

			for (i = 0; i < nPageCount; i++)
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
	catch (UCLIDException& ue)
	{
		UCLIDException uexOuter("ELI36844", "Output PDF validation failed", ue);
		throw uexOuter;
	}
}
//-------------------------------------------------------------------------------------------------
void CESConvertToPDF::validateConfiguration()
{
	// Check for /pdfa and either /user or /owner
	if (m_bPDFA && (!m_strUserPassword.empty() || !m_strOwnerPassword.empty()))
	{
		throw UCLIDException("ELI53644", "Invalid configuration: cannot output PdfA and set a password!");
	}

	// validate input file
	if (!fileExistsAndIsReadable(m_strInputFile))
	{
		UCLIDException ue("ELI53645", "Invalid filename. Input file must be readable.");
		ue.addDebugInfo("Input filename", m_strInputFile);
		throw ue;
	}
	else if (isValidFolder(m_strInputFile))
	{
		UCLIDException ue("ELI53646", "Invalid filename. Input file cannot be a folder.");
		ue.addDebugInfo("Input filename", m_strInputFile);
		throw ue;
	}

	// validate output file
	if (isFileOrFolderValid(m_strOutputFile))
	{
		if (isValidFolder(m_strOutputFile))
		{
			UCLIDException ue("ELI53647", "Invalid filename. Output file cannot be a folder.");
			ue.addDebugInfo("Output filename", m_strOutputFile);
			throw ue;
		}
		else if (isFileReadOnly(m_strOutputFile))
		{
			UCLIDException ue("ELI53648", "Invalid filename. Output file is write-protected.");
			ue.addDebugInfo("Output filename", m_strOutputFile);
			throw ue;
		}
	}
	else
	{
		// validate output directory
		char pszOutputFullPath[MAX_PATH + 1];
		if (!_fullpath(pszOutputFullPath, m_strOutputFile.c_str(), MAX_PATH))
		{
			// throw or display an error
			UCLIDException ue("ELI53649", "Invalid path for output file.");
			ue.addDebugInfo("Output filename", m_strOutputFile);
			throw ue;
		}
		string strOutputDir(getDirectoryFromFullPath(pszOutputFullPath));
		if (!isValidFolder(strOutputDir))
		{
			// create the output file's directory
			createDirectory(strOutputDir);
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CESConvertToPDF::decryptString(string& rstrEncryptedString)
{
	// Build the key
	ByteStream bytesKey;
	ByteStreamManipulator bytesManipulatorKey(
		ByteStreamManipulator::kWrite, bytesKey);
	bytesManipulatorKey << gulPdfKey1;
	bytesManipulatorKey << gulPdfKey2;
	bytesManipulatorKey << gulPdfKey3;
	bytesManipulatorKey << gulPdfKey4;
	bytesManipulatorKey.flushToByteStream(8);

	// Decrypt the string
	ByteStream bytes(rstrEncryptedString);
	ByteStream decrypted;
	MapLabel encryptionEngine;
	encryptionEngine.getMapLabel(decrypted, bytes, bytesKey);

	// Get the decrypted string
	ByteStreamManipulator bsm(ByteStreamManipulator::kRead, decrypted);
	bsm >> rstrEncryptedString;
}
//-------------------------------------------------------------------------------------------------
void CESConvertToPDF::setStringSetting(const string& strSetting, const string& strValue)
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
	catch (UCLIDException& ue)
	{
		ue.addDebugInfo("Setting Name", strSetting);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void CESConvertToPDF::setBoolSetting(const string& strSetting, bool bValue)
{
	setIntSetting(strSetting, asMFCBool(bValue));
}
//-------------------------------------------------------------------------------------------------
void CESConvertToPDF::setIntSetting(const string& strSetting, int nValue)
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
bool CESConvertToPDF::isPdfSecuritySettingEnabled(int nSetting)
{
	return isFlagSet(m_nPermissions, nSetting);
}
//-------------------------------------------------------------------------------------------------
void CESConvertToPDF::validateLicense()
{
	// ensure this feature is licensed
	VALIDATE_LICENSE(gnCREATE_SEARCHABLE_PDF_FEATURE, "ELI18710", "ESConvertToPDF");
}
//-------------------------------------------------------------------------------------------------
