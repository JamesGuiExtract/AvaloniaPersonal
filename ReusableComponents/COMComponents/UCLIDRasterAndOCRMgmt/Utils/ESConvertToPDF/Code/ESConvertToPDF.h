// ESConvertToPDF.h : main header file for the CESConvertToPDF application
//

#pragma once

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"

#include <UCLIDException.h>
#include <KernelAPI.h>
#include <Recpdf.h>

#include <string>

using namespace std;

// Convert to Searchable PDF application class
class CESConvertToPDFApp : public CWinApp
{
public:
	CESConvertToPDFApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CESConvertToPDFApp)
	public:
	virtual BOOL InitInstance();
	virtual int ExitInstance();
	//}}AFX_VIRTUAL

// Implementation

	//{{AFX_MSG(CESConvertToPDFApp)
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

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
	void addPagesToOutput(HPAGE *pPages, const string& strOutputPDF, IMF_FORMAT outFormat,
		int nPageCount);
	//---------------------------------------------------------------------------------------------
	// Applies the searchable text in pages to the specified strImageFile using the RecPDF API.
	void applySearchableTextWithRecPDFAPI(RPDF_DOC pdfDoc, HPAGE *pages, int nStartPage, int nPageCount);
	//---------------------------------------------------------------------------------------------
	// Validates that the specified PDF can be read. Added use in response to issue where RecPDF
	// API call can result in corrupted images:
	// https://extract.atlassian.net/browse/ISSUE-12163
	void validatePDF(const string& strFileName);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Displays usage information for this executable and an error message if specified.
	//          Returns false.
	// PARAMS:  (1) strFileName - the name of this executable.
	//          (2) strErrMsg - an optional error message to display before usage information.
	bool displayUsage(const string& strFileName, const string& strErrMsg="");
	//---------------------------------------------------------------------------------------------
	// PURPOSE: If m_strExceptionLogFile is empty, displays the error description and associated 
	//          debug information in an MFC dialog. If m_strExceptionLogFile is not empty, throws 
	//          an exception using the ELI code specified and the associated error information.
	bool errMsg(const string& strELICode, const string& strErrorDescription, 
		const string& strDebugInfoKey, const string& strDebugInfoValue);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: Parses command line arguments and sets the values of the member variables
	//          appropriately. Returns true if an input file and output file that are ready for 
	//          processing have been provided. The user may be prompted whether to overwrite output 
	//          files or create an output directory if no exception log was specified. If an 
	//          exception log is specified, no dialog will be displayed, files will be 
	//          automatically overwritten, and directories will be automatically created. 
	//          m_bIsError may be set to false if display usage was requested or if the arguments 
	//          were valid but the user cancelled when prompted. 
	bool getAndValidateArguments(const int argc, char* argv[]);
	//---------------------------------------------------------------------------------------------
	// PURPOSE: To decrypt the specified string using the Pdf security values
	void decryptString(string& rstrEncryptedString);
	//---------------------------------------------------------------------------------------------
	// PROMISE: Makes the appropriate RecAPI calls to license the RecAPIPlus layers of the OCR engine.
	// REQUIRE: A writable convlist.txt file must exist in the same directory as the RecAPIPlus dll.
	void licenseOCREngine();
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
	// PROMISE: Throws an exception if this executable is unlicensed. Returns true otherwise.
	static void validateLicense();
};

//-------------------------------------------------------------------------------------------------
// Variable definitions
//-------------------------------------------------------------------------------------------------

// a RecAPI module and its associated description
struct ModuleDescriptionType
{
	const KRECMODULES eModule;
	const string strModuleDescription;
};