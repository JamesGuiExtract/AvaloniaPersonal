#include "StdAfx.h"
#include "RecAPIManager.h"
#include "ScansoftErr.h"
#include "OcrMethods.h"
#include "ESConvertToPDF.h"

#include <PdfSecurityValues.h>
#include <RecAPIPlus.h>

#include <UCLIDException.h>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------

// RecAPI settings class that has the settings that govern PDF output.
string strOUTPUT_SETTINGS_CLASS = "Kernel.Imf.PDF";

//---------------------------------------------------------------------------------------------
// CRecAPIManager class
//---------------------------------------------------------------------------------------------
CRecAPIManager::CRecAPIManager(CESConvertToPDF* pApp, const string& strFileName, PDF_PROC_MODE processingMode)
	: m_pApp(pApp)
	, m_hFile(__nullptr)
	, m_pPages(__nullptr)
	, m_nLoadedPageCount(-1)
{
	ASSERT_ARGUMENT("ELI37012", m_pApp != __nullptr);

	applySettings(processingMode);
	openImageFile(strFileName);
}
//---------------------------------------------------------------------------------------------
CRecAPIManager::~CRecAPIManager()
{
	// Attempt to free all memory being used by the RecAPI. Log rather than throw any errors.
	try
	{
		try
		{
			if (m_hFile != __nullptr)
			{
				for (int i = 0; i < m_nLoadedPageCount; i++)
				{
					HPAGE& hPage = m_pPages[i];

					RECERR rc = kRecFreeRecognitionData(hPage);
					if (rc != REC_OK)
					{
						UCLIDException ue("ELI37013", "Application trace: Unable to release "
							"recognition data. Possible memory leak.");
						loadScansoftRecErrInfo(ue, rc);
						ue.log();
					}

					rc = kRecFreeImg(hPage);
					if (rc != REC_OK)
					{
						UCLIDException ue("ELI36749", "Application trace: Unable to release page "
							"image. Possible memory leak.");
						loadScansoftRecErrInfo(ue, rc);
						ue.log();
					}
				}

				RECERR rc = kRecCloseImgFile(m_hFile);

				if (rc != REC_OK)
				{
					UCLIDException ue("ELI40289",
						"Application trace: Unable to close image file. Possible memory leak.");
					loadScansoftRecErrInfo(ue, rc);
					ue.log();
				}
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI37014")
	}
	catch (UCLIDException& ue)
	{
		ue.log();
	}
}
//-------------------------------------------------------------------------------------------------
int CRecAPIManager::getPageCount()
{
	int nPageCount = -1;
	RECERR rc = kRecGetImgFilePageCount(m_hFile, &nPageCount);
	throwExceptionIfNotSuccess(rc, "ELI36842", "Unable to get page count.",
		m_pApp->m_strInputFile);

	return nPageCount;
}
//-------------------------------------------------------------------------------------------------
void CRecAPIManager::getImageInfo(IMG_INFO& rimgInfo, IMF_FORMAT& rimgFormat)
{
	RECERR rc = kRecGetImgFilePageInfo(0, m_hFile, 0, &rimgInfo, &rimgFormat);
	throwExceptionIfNotSuccess(rc, "ELI36759", "Failed to identify image format.",
		m_pApp->m_strInputFile);
}
//-------------------------------------------------------------------------------------------------
HPAGE* CRecAPIManager::getOCRedPages(int nStartPage, int nPageCount)
{
	if (m_pPages != __nullptr)
	{
		// getOCRedPages should be called at most once per instance.
		THROW_LOGIC_ERROR_EXCEPTION("ELI37015");
	}

	m_pPages = new HPAGE[nPageCount];
	int i = 0;
	for (i = nStartPage; i < nStartPage + nPageCount; i++)
	{
		HPAGE& hPage = m_pPages[i - nStartPage];

		// load the ith page
		loadPageFromImageHandle(m_pApp->m_strInputFile, m_hFile, i, &hPage);

		try
		{
			try
			{
				// Preprocess the image to take care of rotation
				// https://extract.atlassian.net/browse/ISSUE-16740
				RECERR rc = kRecPreprocessImg(0, hPage);
				throwExceptionIfNotSuccess(rc, "ELI50258", "Failed to preprocess image page.",
					m_pApp->m_strInputFile, i);

				rc = kRecRecognize(0, hPage, 0);
				if (rc != REC_OK && rc != NO_TXT_WARN && rc != ZONE_NOTFOUND_ERR)
				{
					// log an error
					UCLIDException ue("ELI18589", "Unable to recognize text on page.");
					loadScansoftRecErrInfo(ue, rc);
					ue.addDebugInfo("Input filename", m_pApp->m_strInputFile);
					ue.addDebugInfo("Page number", i + 1);

					// add page size information [P13 #4603]
					if (rc == IMG_SIZE_ERR)
					{
						addPageSizeDebugInfo(ue, m_hFile, i);
					}

					throw ue;
				}
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI18628");
		}
		catch (UCLIDException ue)
		{
			// [LegacyRCAndUtils:6363]
			// Rather than abort the entire conversion of OCR fails on a given page, simply
			// log the exception and let the conversion complete (albeit without searchable text
			// on this page.
			ue.log();
		}
	}

	m_nLoadedPageCount = i - nStartPage;

	return m_pPages;
}

//---------------------------------------------------------------------------------------------
// Private Members
//---------------------------------------------------------------------------------------------
void CRecAPIManager::applySettings(PDF_PROC_MODE processingMode)
{
	// Common and default settings should already have been set at this point
	// (See CScansoftOCR::raw_CreateSearchablePdf)

	// OCR should be accurate rather than fast.
	m_pApp->setIntSetting("Kernel.OcrMgr.PDF.TradeOff", TO_ACCURATE);

	// Use the more accurate 3-way voting engine (rather than the default 2-way voting engine).
	m_pApp->setBoolSetting("Kernel.OcrMgr.PreferAccurateEngine", true);

	m_pApp->setIntSetting("Kernel.OcrMgr.PDF.ProcessingMode", processingMode);

	// Preserve the original resolution in the output PDF.
	m_pApp->setBoolSetting(strOUTPUT_SETTINGS_CLASS + ".LoadOriginalDPI", true);

	// If output is to be PDF/A compliant then need to set the PDF/A compatibility mode
	// Adding searchable text with the RecPDFAPI breaks 1a and 1b compliance, but we seem to be
	// 2a compliant. (confirmed using:
	// http://www.datalogics.com/products/callas/callaspdfA-onlinedemo.asp, though the RecPDFAPI
	// does not make any claims with regards to PDF/A compliance.)
	if (m_pApp->m_bPDFA)
	{
		m_pApp->setIntSetting(strOUTPUT_SETTINGS_CLASS + ".Compatibility", R2ID_PDFA2A);
	}

	if (!m_pApp->m_strUserPassword.empty() || !m_pApp->m_strOwnerPassword.empty())
	{
		// If either password is defined, enable 128 bit security
		m_pApp->setIntSetting(strOUTPUT_SETTINGS_CLASS + ".PDFSecurity.Type",
			R2ID_PDFSECURITY128BITS);

		// If there is a user password defined, set it
		if (!m_pApp->m_strUserPassword.empty())
		{
			m_pApp->setStringSetting(strOUTPUT_SETTINGS_CLASS + ".PDFSecurity.UserPassword",
				m_pApp->m_strUserPassword);
		}
		// If there is an owner password defined, set it and the associated permissions
		if (!m_pApp->m_strOwnerPassword.empty())
		{
			// Set the owner password
			m_pApp->setStringSetting(strOUTPUT_SETTINGS_CLASS + ".PDFSecurity.OwnerPassword",
				m_pApp->m_strOwnerPassword);

			// Get security settings

			// Allow printing if either high quality or low quality is specified
			bool bAllowHighQuality = m_pApp->isPdfSecuritySettingEnabled(giAllowHighQualityPrinting);
			bool bAllowPrinting = bAllowHighQuality
				|| m_pApp->isPdfSecuritySettingEnabled(giAllowLowQualityPrinting);

			// Allow adding/modifying annotations if either allow modifications or
			// allow adding/modifying annotations is specified
			bool bAllowDocModify = m_pApp->isPdfSecuritySettingEnabled(giAllowDocumentModifications);
			bool bAllowAddModifyAnnot = bAllowDocModify ||
				m_pApp->isPdfSecuritySettingEnabled(giAllowAddingModifyingAnnotations);

			// Allow form fill in if either adding/modifying annotations is allowed or
			// filling in forms is specified
			bool bAllowForms = bAllowAddModifyAnnot
				|| m_pApp->isPdfSecuritySettingEnabled(giAllowFillingInFields);

			// Allow extraction for accessibility if either content copying or accessibility
			// is specified
			bool bAllowCopy = m_pApp->isPdfSecuritySettingEnabled(giAllowContentCopying);
			bool bAllowExtract = bAllowCopy
				|| m_pApp->isPdfSecuritySettingEnabled(giAllowContentCopyingForAccessibility);

			// Set the security settings
			m_pApp->setBoolSetting(strOUTPUT_SETTINGS_CLASS + ".PDFSecurity.EnablePrint", bAllowPrinting);
			m_pApp->setBoolSetting(strOUTPUT_SETTINGS_CLASS + ".PDFSecurity.EnablePrintQ", bAllowHighQuality);
			m_pApp->setBoolSetting(strOUTPUT_SETTINGS_CLASS + ".PDFSecurity.EnableModify", bAllowDocModify);
			m_pApp->setBoolSetting(strOUTPUT_SETTINGS_CLASS + ".PDFSecurity.EnableCopy", bAllowCopy);
			m_pApp->setBoolSetting(strOUTPUT_SETTINGS_CLASS + ".PDFSecurity.EnableExtract", bAllowExtract);
			m_pApp->setBoolSetting(strOUTPUT_SETTINGS_CLASS + ".PDFSecurity.EnableAdd", bAllowAddModifyAnnot);
			m_pApp->setBoolSetting(strOUTPUT_SETTINGS_CLASS + ".PDFSecurity.EnableForms", bAllowForms);
			m_pApp->setBoolSetting(strOUTPUT_SETTINGS_CLASS + ".PDFSecurity.EnableAssemble",
				m_pApp->isPdfSecuritySettingEnabled(giAllowDocumentAssembly));
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CRecAPIManager::openImageFile(const string& strFileName)
{
	// Get the retry count and timeout
	int iRetryCount(-1), iRetryTimeout(-1);
	getFileAccessRetryCountAndTimeout(iRetryCount, iRetryTimeout);

	int iNumRetries = 0;
	RECERR rc = kRecOpenImgFile(strFileName.c_str(), &m_hFile, IMGF_READ, FF_SIZE);
	while (rc != REC_OK)
	{
		// Increment the retry count and try again
		iNumRetries++;
		rc = kRecOpenImgFile(strFileName.c_str(), &m_hFile, IMGF_READ, FF_SIZE);

		// If opened successfully, log an application trace and break from the loop
		if (rc == REC_OK)
		{
			UCLIDException ue("ELI28853", "Application Trace: Opened image after retrying.");
			ue.addDebugInfo("Number of retries", iNumRetries);
			ue.addDebugInfo("Image Name", m_pApp->m_strInputFile);
			ue.log();

			// Exit the while loop
			break;
		}
		// Check if the error is not IMF_OPEN_ERROR, if not then throw an exception
		else if (rc != IMF_OPEN_ERR)
		{
			UCLIDException ue("ELI18587", "Unable to open input file.");
			loadScansoftRecErrInfo(ue, rc);
			ue.addDebugInfo("Input filename", m_pApp->m_strInputFile);
			throw ue;
		}

		// Check the retry count
		if (iNumRetries < iRetryCount)
		{
			// Sleep and retry
			Sleep(iRetryTimeout);
		}
		else
		{
			// Reached max retry count, throw an exception
			UCLIDException ue("ELI28854", "Unable to open input file after retrying.");
			loadScansoftRecErrInfo(ue, rc);
			ue.addDebugInfo("Image Name", m_pApp->m_strInputFile);
			ue.addDebugInfo("Number of retries", iNumRetries);
			ue.addDebugInfo("Max number of retries", iRetryCount);
			throw ue;
		}
	}
}