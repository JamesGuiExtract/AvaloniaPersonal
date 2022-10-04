#pragma once

#include <UCLIDException.h>
#include <KernelAPI.h>
#include <Recpdf.h>

#include <string>

using namespace std;

class CESConvertToPDF
{
public:
	CESConvertToPDF(
		string inputFile,
		string outputFile,
		bool removeOriginal,
		bool outputPdfA,
		string userPassword,
		string ownerPassword,
		bool passwordsAreEncrypted,
		long permissions);

	CESConvertToPDF(const CESConvertToPDF&) = delete;

	void ConvertToPDF();

private:

	friend class CRecAPIManager;

	// user-specified input filename to convert into a searchable pdf
	string m_strInputFile;

	// user-specified output pdf filename
	string m_strOutputFile;

	// User password to apply to PDF
	string m_strUserPassword;

	// Owner password to apply to PDF
	string m_strOwnerPassword;

	// Permissions to apply to PDF
	int m_nPermissions;

	// whether to remove original image after conversion (true) or retain the original (false)
	// user-specified
	bool m_bRemoveOriginal;

	// Whether the output PDF should be PDF/A compliant
	bool m_bPDFA;

	// whether or not the app failed to execute (true) or ran successfully (false)
	bool m_bIsError;

	// filename of exception log if exceptions should be logged, "" if exceptions should be displayed
	string m_strExceptionLogFile;

	//---------------------------------------------------------------------------------------------
	// Executes the conversion to searchable PDF. If bUseRecDFAPI = true, the RecAPI will be used
	// exclusively which means each image will be converted (modified) into the output document.
	// If bUseRecDFAPI = false, the RecPDF API will be used. The RecPDF API has the advantage of
	// being able to insert text without touching the image if the source document is a PDF itself.
	// However, the RecPDFAPI is not able to be used in cases where the output document requires
	// security.
	void convertToSearchablePDF(bool bUseRecDFAPI);
	//---------------------------------------------------------------------------------------------
	// Adds the specified pages to the strOutputPDF in the specified outFormat. The images are
	// converted/modified (no way around this).
	void addPagesToOutput(HPAGE* pPages, const string& strOutputPDF, IMF_FORMAT outFormat,
		int nPageCount);
	//---------------------------------------------------------------------------------------------
	// Applies the searchable text in pages to the specified strImageFile using the RecPDF API.
	void applySearchableTextWithRecPDFAPI(RPDF_DOC pdfDoc, HPAGE* pages, int nStartPage, int nPageCount);
	//---------------------------------------------------------------------------------------------
	// Validates that the specified PDF can be read. Added use in response to issue where RecPDF
	// API call can result in corrupted images:
	// https://extract.atlassian.net/browse/ISSUE-12163
	void validatePDF(const string& strFileName);
	//---------------------------------------------------------------------------------------------
	void validateConfiguration();
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To decrypt the specified string using the Pdf security values
	void decryptString(string& rstrEncryptedString);
	//---------------------------------------------------------------------------------------------
	// Sets a string setting for the OCR engine
	void setStringSetting(const string& strSetting, const string& strValue);
	//---------------------------------------------------------------------------------------------
	// Sets a boolean setting for the OCR engine
	void setBoolSetting(const string& strSetting, bool bValue);
	//---------------------------------------------------------------------------------------------
	// Sets an integer setting for the OCR engine (this is also used to ENUM settings)
	void setIntSetting(const string& strSetting, int nValue);
	//---------------------------------------------------------------------------------------------
	// Checks a security setting value agains the m_nPermissions value to see if it is enabled
	// or not, returns true if enabled and false if not enabled
	bool isPdfSecuritySettingEnabled(int nSetting);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Throws an exception if this object is unlicensed
	static void validateLicense();
};
