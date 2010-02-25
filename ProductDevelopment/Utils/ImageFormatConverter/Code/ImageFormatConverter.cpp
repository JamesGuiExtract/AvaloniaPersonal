// ImageFormatConverter.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "ImageFormatConverter.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <UCLIDExceptionDlg.h>
#include <cpputil.h>
#include <TemporaryFileName.h>
#include <l_bitmap.h>		// LeadTools Imaging library
#include <ltkey.h>
#include <ComponentLicenseIDs.h>
#include <MiscLeadUtils.h>
#include <LeadtoolsBitmapFreeer.h>
#include <StringCSIS.h>
#include <PdfSecurityValues.h>
#include <ByteStream.h>
#include <ByteStreamManipulator.h>
#include <EncryptionEngine.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const COLORREF gCOLOR_BLACK = 0x000000;
const COLORREF gCOLOR_WHITE = 0xFFFFFF;

// Default jpg compression
const L_INT giJPG_COMPRESS_DEFAULT = 50;

//-------------------------------------------------------------------------------------------------
// CImageFormatConverterApp
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CImageFormatConverterApp, CWinApp)
	ON_COMMAND(ID_HELP, &CWinApp::OnHelp)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CImageFormatConverterApp construction
//-------------------------------------------------------------------------------------------------
CImageFormatConverterApp::CImageFormatConverterApp()
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}

//-------------------------------------------------------------------------------------------------
// The one and only CImageFormatConverterApp object
//-------------------------------------------------------------------------------------------------
CImageFormatConverterApp theApp;
int nExitCode = EXIT_SUCCESS;

//-------------------------------------------------------------------------------------------------
// Supported file types
//-------------------------------------------------------------------------------------------------
typedef enum EConverterFileType
{
	kFileType_None,
	kFileType_Tif,
	kFileType_Pdf,
	kFileType_Jpg
}	EConverterFileType;

//-------------------------------------------------------------------------------------------------
// Local functions
//-------------------------------------------------------------------------------------------------
// Callback function for L_AnnEnumerate that will burn redactions into the image.
// For each rect or redact annotation object get the color and if the
// color is either black or white, burn the annotation into the image
L_INT EXT_CALLBACK BurnRedactions(HANNOBJECT hObject, L_VOID* pUserData)
{
	L_INT nRet = SUCCESS;
	try
	{
		// Get the bitmap handle
		pBITMAPHANDLE pbmp = (pBITMAPHANDLE) pUserData;

		// Get the annotation type
		L_UINT ObjectType;
		nRet = L_AnnGetType(hObject, &ObjectType);
		throwExceptionIfNotSuccess(nRet, "ELI23577", "Failed to get annotation type.");

		// If the type is either rect or redact then get the color
		if (ObjectType == ANNOBJECT_RECT || ObjectType == ANNOBJECT_REDACT)
		{
			// Get the color
			COLORREF color;
			nRet = L_AnnGetBackColor(hObject, &color);
			throwExceptionIfNotSuccess(nRet, "ELI23578", "Failed to get annotation color.");

			// If the color is black or white then "burn" the annotation into image
			if (color == gCOLOR_BLACK || color == gCOLOR_WHITE)
			{
				// Burn the annotation into the image
				nRet = L_AnnRealize(pbmp, NULL, hObject, FALSE);
				throwExceptionIfNotSuccess(nRet, "ELI23579", "Failed to burn annotation into image.");
			}
		}
	}
	catch(UCLIDException& uex)
	{
		uex.log();
		return nRet;
	}

	return nRet;
}
//-------------------------------------------------------------------------------------------------
// Will decrypt the specified string using the PdfSecurity values
void decryptString(string& rstrEncryptedString)
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
	EncryptionEngine ee;
	ee.decrypt(decrypted, bytes, bytesKey);

	// Get the decrypted string
	ByteStreamManipulator bsm (ByteStreamManipulator::kRead, decrypted);
	bsm >> rstrEncryptedString;
}
//-------------------------------------------------------------------------------------------------
unsigned int getLtPermissions(long nPermissions)
{
	unsigned int uiLtPermissions = 0;
	if (nPermissions > 0)
	{
		if (isFlagSet(nPermissions, giAllowLowQualityPrinting))
		{
			uiLtPermissions |= PDF_SECURITYFLAGS_REV2_PRINTDOCUMENT;
		}
		if (isFlagSet(nPermissions, giAllowHighQualityPrinting))
		{
			// Need to allow document printing to allow high quality printing
			uiLtPermissions |= PDF_SECURITYFLAGS_REV2_PRINTDOCUMENT;
			uiLtPermissions |= PDF_SECURITYFLAGS_REV3_PRINTFAITHFUL;
		}
		if (isFlagSet(nPermissions, giAllowDocumentModifications))
		{
			uiLtPermissions |= PDF_SECURITYFLAGS_REV2_MODIFYDOCUMENT;
			uiLtPermissions |= PDF_SECURITYFLAGS_REV2_MODIFYANNOTATION;
			uiLtPermissions |= PDF_SECURITYFLAGS_REV3_FILLFORM;
		}
		if (isFlagSet(nPermissions, giAllowContentCopying))
		{
			uiLtPermissions |= PDF_SECURITYFLAGS_REV3_EXTRACTTEXTGRAPHICS;
			uiLtPermissions |= PDF_SECURITYFLAGS_REV2_EXTRACTTEXT;
		}
		if (isFlagSet(nPermissions, giAllowContentCopyingForAccessibility))
		{
			uiLtPermissions |= PDF_SECURITYFLAGS_REV2_EXTRACTTEXT;
		}
		if (isFlagSet(nPermissions, giAllowAddingModifyingAnnotations))
		{
			uiLtPermissions |= PDF_SECURITYFLAGS_REV2_MODIFYANNOTATION;
			uiLtPermissions |= PDF_SECURITYFLAGS_REV3_FILLFORM;
		}
		if (isFlagSet(nPermissions, giAllowFillingInFields))
		{
			uiLtPermissions |= PDF_SECURITYFLAGS_REV3_FILLFORM;
		}
		if (isFlagSet(nPermissions, giAllowDocumentAssembly))
		{
			uiLtPermissions |= PDF_SECURITYFLAGS_REV3_ASSEMBLEDOCUMENT;
		}
	}

	return uiLtPermissions;
}
//-------------------------------------------------------------------------------------------------
// Will perform the image conversion and will also retain the existing annotations
// if bRetainAnnotations is true.
// If the output format is a tif then the annotations will remain as annotations
// if the output format is a pdf then the redaction annotations will be burned into the image.
void convertImage(const string strInputFileName, const string strOutputFileName, 
				  EConverterFileType eOutputType, bool bRetainAnnotations, HINSTANCE hInst,
				  const string& strUserPassword, const string& strOwnerPassword, long nPermissions)
{
	HANNOBJECT hFileContainer = NULL;
	ANNENUMCALLBACK pfnCallBack = NULL;
	try
	{
		try
		{
			// Create a temporary file for the output [LRCAU #5583]
			TemporaryFileName tmpOutput("", NULL,
				getExtensionFromFullPath(strOutputFileName).c_str(), true);
			const string& strTempOut = tmpOutput.getName();

			L_INT nRet = FAILURE;

			// Get the file info
			FILEINFO fileInfo = GetLeadToolsSizedStruct<FILEINFO>(0);
			nRet = L_FileInfo((char*)strInputFileName.c_str(), &fileInfo, sizeof(FILEINFO),
				FILEINFO_TOTALPAGES, NULL);
			throwExceptionIfNotSuccess(nRet, "ELI25229",
				"Could not obtain FileInfo", strInputFileName);

			// Get the total number of pages from the file info
			L_INT nPages = fileInfo.TotalPages;

			// Get the file format
			int iFormat = fileInfo.Format;

			// Get initialized SAVEFILEOPTION struct
			SAVEFILEOPTION sfOptions = GetLeadToolsSizedStruct<SAVEFILEOPTION>(0);
			L_GetDefaultSaveFileOption( &sfOptions, sizeof ( sfOptions ));

			// Set type-specific output options
			L_INT	nType;
			L_INT   nQFactor = PQ1;
			int		nBitsPerPixel = 1; 
			bool	bBurnAnnotations = false;
			switch (eOutputType)
			{
			case kFileType_Pdf:
				{
					// Check for security settings for PDF files
					if (!strUserPassword.empty() || !strOwnerPassword.empty())
					{
						FILEPDFSAVEOPTIONS pdfsfo = GetLeadToolsSizedStruct<FILEPDFSAVEOPTIONS>(0);
						throwExceptionIfNotSuccess(
							L_GetPDFSaveOptions(&pdfsfo, sizeof(FILEPDFSAVEOPTIONS)), "ELI29752",
							"Failed to get PDF save options.");

						pdfsfo.b128bit = L_TRUE;
						if (!strUserPassword.empty())
						{
							errno_t err = strncpy_s((char*)pdfsfo.szUserPassword, 255,
								strUserPassword.c_str(),  strUserPassword.length());
							if (err != 0)
							{
								UCLIDException ue("ELI29762", "Unable to set user password.");
								ue.addWin32ErrorInfo(err);
								throw ue;
							}
						}
						if (!strOwnerPassword.empty())
						{
							errno_t err = strncpy_s((char*)pdfsfo.szOwnerPassword, 255,
								strOwnerPassword.c_str(),  strOwnerPassword.length());
							if (err != 0)
							{
								UCLIDException ue("ELI29763", "Unable to set owner password.");
								ue.addWin32ErrorInfo(err);
								throw ue;
							}

							// Set the permissions
							pdfsfo.dwEncryptFlags = getLtPermissions(nPermissions);
						}

						throwExceptionIfNotSuccess(L_SetPDFSaveOptions(&pdfsfo),
							"ELI29792", "Unable to set PDF save options.");
					}

					// Set output format
					nType = FILE_RAS_PDF_JBIG2;

					// This flag will cause the out put image to have the same( or nearly the same ) 
					// dimensions as original input file
					sfOptions.Flags = ESO_PDF_SAVE_USE_BITMAP_DPI;

					// Set the burn annotations flag to true
					bBurnAnnotations = true;
					break;
				}

			case kFileType_Tif:
				// Set output format
				nType = FILE_CCITT_GROUP4;
				break;

			case kFileType_Jpg:
				// Set the output format (preserve color in JPG images)
				nType = FILE_JPEG;
				nBitsPerPixel = 24;

				// PQ1 is invalid for JPEG, specify compression
				// between 1(no loss) and 255(high compress)
				nQFactor = giJPG_COMPRESS_DEFAULT; 

				// Set the burn annotations flag to true
				bBurnAnnotations = true;
				break;

				// Other file types not supported at this time
			default:
				UCLIDException ue( "ELI23582", "Other file types are not supported at this time!" );
				ue.addDebugInfo( "Input File", strInputFileName );
				ue.addDebugInfo( "Output File", strOutputFileName );
				ue.addDebugInfo( "Output Type", (int)eOutputType );
				throw ue;
				break;
			}

			// Get initialized LOADFILEOPTION struct. 
			// IgnoreViewPerspective to avoid a black region at the bottom of the image
			LOADFILEOPTION lfo =
				GetLeadToolsSizedStruct<LOADFILEOPTION>(ELO_IGNOREVIEWPERSPECTIVE);

			// Get the input file name as a char*
			char* pszInputFile = (char*) strInputFileName.c_str();

			// Handle pages individually to deal with situation where existing annotations
			// need to be retained
			for (int i = 1; i <= nPages; i++)
			{
				// Set FILEINFO_FORMATVALID (this will speed up the L_LoadBitmap calls)
				fileInfo = GetLeadToolsSizedStruct<FILEINFO>(FILEINFO_FORMATVALID);
				fileInfo.Format = iFormat;

				// Set the 1-relative page number in the LOADFILEOPTION structure 
				lfo.PageNumber = i;

				BITMAPHANDLE hBitmap = {0};
				LeadToolsBitmapFreeer freer(hBitmap);
				loadImagePage(strInputFileName, hBitmap, fileInfo, lfo, false, false);

				// Load the existing annotations if bRetainAnnotations and they exist.
				if(bRetainAnnotations && hasAnnotations(strInputFileName, lfo, iFormat))
				{
					nRet = L_AnnLoad(pszInputFile, &hFileContainer, &lfo);
					throwExceptionIfNotSuccess(nRet, "ELI23584", "Could not load annotations.",
						strInputFileName);
				}

				// Check for NULL or empty container
				bool bSavedTag = false;
				if (hFileContainer != NULL)
				{
					// Retrieve the first annotation item
					HANNOBJECT	hFirst = NULL;
					nRet = L_AnnGetItem( hFileContainer, &hFirst );
					throwExceptionIfNotSuccess( nRet, "ELI23585", 
						"Could not get item from annotation container." );

					if (hFirst != NULL)
					{

						if (bBurnAnnotations)
						{
							// Setup the callback function for the annotation enumeration
							pfnCallBack =
								(ANNENUMCALLBACK) MakeProcInstance((FARPROC) BurnRedactions, hInst);

							// Burn the redaction annotations into the image
							nRet = L_AnnEnumerate(hFileContainer, pfnCallBack,(L_VOID*) &hBitmap,
								ANNFLAG_RECURSE, NULL);
							throwExceptionIfNotSuccess(nRet, "ELI23586",
								"Could not burn annotations into the image.");

							// Free the callback proc instance
							FreeProcInstance((FARPROC) BurnRedactions);
							pfnCallBack = NULL;
						}
						else
						{
							// Save the collected annotations from this page
							//   Save in WANG-mode for greatest compatibility
							//   The next call to SaveBitmap will include these annotations
							nRet = L_AnnSaveTag( hFileContainer, ANNFMT_WANGTAG, FALSE );
							throwExceptionIfNotSuccess( nRet, "ELI23587", 
								"Could not save annotation objects." );
							bSavedTag = true;
						}
					}
					// else non-NULL container is empty, so nothing to save

					// Destroy the annotation container
					nRet = L_AnnDestroy(hFileContainer, 0);
					throwExceptionIfNotSuccess(nRet, "ELI23588",
						"Unable to destroy annotation container.");
					hFileContainer = NULL;
				}
				// else container is NULL, so nothing to save

				// Set the page number
				sfOptions.PageNumber = i;

				// Save the image page
				saveImagePage(hBitmap, strTempOut, nType, nQFactor, nBitsPerPixel, sfOptions, false);

				if (bSavedTag)
				{
					// Clear any previously defined annotations
					// If not done, any annotations applied to this page may be applied to 
					// successive pages (P16 #2216)
					nRet = L_SetTag( ANNTAG_TIFF, 0, 0, NULL );
				}
			}	// end for each page

			// Wait for the file to be readable before continuing
			waitForFileToBeReadable(strTempOut);

			// Check for a matching page count
			long nOutPages = getNumberOfPagesInImage(strTempOut);
			if (nPages != nOutPages)
			{
				UCLIDException uex("ELI28839", "Output page count mismatch.");
				uex.addDebugInfo("Input File", strInputFileName);
				uex.addDebugInfo("Input Page Count", nPages);
				uex.addDebugInfo("Temporary Output File", strTempOut);
				uex.addDebugInfo("Temporary Output Page Count", nOutPages);
				uex.addDebugInfo("Output File", strOutputFileName);
				throw uex;
			}

			// Ensure the outut directory exists
			string strOutDir = getDirectoryFromFullPath(strOutputFileName);
			if (!isValidFolder(strOutDir))
			{
				createDirectory(strOutDir);
			}

			// Move the temporary file to the output file location
			// (overwrite the output file if it exists)
			moveFile(strTempOut, strOutputFileName, true, true);
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI23592");
	}
	catch(UCLIDException& uex)
	{
		// Clean up resources
		if (pfnCallBack != NULL)
		{
			// Free the callback proc instance
			FreeProcInstance((FARPROC) BurnRedactions);
		}
		if (hFileContainer != NULL)
		{
			try
			{
				// Destroy the annotation container
				throwExceptionIfNotSuccess(L_AnnDestroy(hFileContainer, 0), "ELI23593",
					"Unable to destroy annotation container.");
			}
			catch(UCLIDException& ex)
			{
				ex.log();
			}
			hFileContainer = NULL;
		}
		if (bRetainAnnotations)
		{
			// Clear any previously defined annotations
			// If not done, any annotations applied to this page may be applied to 
			// successive pages (P16 #2216)
			L_SetTag( ANNTAG_TIFF, 0, 0, NULL );
		}

		throw uex;
	}
}
//-------------------------------------------------------------------------------------------------
void usage()
{
	string strUsage = "This application has 3 required arguments and 4 optional arguments:\n";
		strUsage += "An input image file (.tif or .pdf) and \n"
					"an output image file (.pdf or .tif) and \n"
					"an output file type (/pdf, /tif or /jpg).\n\n"
					"The optional argument (/retain) will cause any redaction annotations to \n"
					"be burned into the resulting image (if the source is tif and destination \n"
					"is a pdf or jpg, if source and dest are both tif then all annotations are \n"
					"retained, if the source is pdf then there are no annotations to retain).\n"
					"The optional arguments for applying passwords only apply if out_type is /pdf.\n"
					" /user\t\tSpecifies the user password to apply to the PDF.\n"
					" /owner\t\tSpecified the owner password and permissions to apply to the PDF.\n"
					" Permissions - An integer that is the sum of all permissions to set.\n"
					" \t\tAllow low quality printing = 1.\n"
					" \t\tAllow high quality printing = 2.\n"
					" \t\tAllow document modifications = 4.\n"
					" \t\tAllow copying/extraction of contents = 8.\n"
					" \t\tAllow accessibility access to contents = 16.\n"
					" \t\tAllow adding/modifying text annotations = 32.\n"
					" \t\tAllow filling in form fields = 64.\n"
					" \t\tAllow document assembly = 128.\n"
					" \t\tAllow all options = 255.\n"
					"The optional argument (/ef <filename>) fully specifies the location \n"
					"of an exception log that will store any thrown exception.  Without \n"
					"an exception log, any thrown exception will be displayed.\n\n";
		strUsage += "Usage:\n";
		strUsage += "ImageFormatConverter.exe <strInput> <strOutput> <out_type> [/retain] "
					"[/user \"<Password>\"] [/owner \"<Password>\" <Permissions>] [/ef <filename>]\n"
					"where:\n"
					"out_type is /pdf, /tif or /jpg,\n"
					"<Password> is the password to apply (user and/or owner) to the PDF (requires out_type = /pdf).\n"
					"<Permissions> is an integer between 0 and 255.\n"
					"<filename> is the fully-qualified path to an exception log.\n\n";
		AfxMessageBox(strUsage.c_str());
}
//-------------------------------------------------------------------------------------------------
void validateLicense()
{
	// Requires Flex Index/ID Shield core license [LRCAU #5589]
	static const unsigned long THIS_APP_ID = gnFLEXINDEX_IDSHIELD_CORE_OBJECTS;

	VALIDATE_LICENSE( THIS_APP_ID, "ELI15897", "ImageFormatConverter" );
}

//-------------------------------------------------------------------------------------------------
// CImageFormatConverterApp initialization
//-------------------------------------------------------------------------------------------------
BOOL CImageFormatConverterApp::InitInstance()
{
	try
	{
		AfxEnableControlContainer();

		// Define empty string for local exception log
		string strLocalExceptionLog;

		try
		{
			try
			{
				// Setup exception handling
				static UCLIDExceptionDlg exceptionDlg;
				UCLIDException::setExceptionHandler( &exceptionDlg );

				// Retrieve command-line parameters for ImageFormatConverter.exe
				vector<string> vecParams;
				int i;
				for (i = 1; i < __argc; i++)
				{
					vecParams.push_back( __argv[i]);
				}

				// Make sure the number of parameters is 3 or 12
				size_t uiParamCount = vecParams.size();
				if ((uiParamCount < 3) || (uiParamCount > 12))
				{
					usage();
					return FALSE;
				}

				// Retrieve file names and output type
				string strInputName = buildAbsolutePath(vecParams[0]);
				string strOutputName = buildAbsolutePath(vecParams[1]);
				if (stringCSIS::sEqual(strInputName, strOutputName))
				{
					UCLIDException uex("ELI25307", "Input and Output files must differ!");
					uex.addDebugInfo("Input File", vecParams[0]);
					uex.addDebugInfo("Output File", vecParams[1]);
					throw uex;
				}

				EConverterFileType eOutputType = kFileType_None;

				// Check output type
				string strParam = vecParams[2];
				makeLowerCase(strParam);

				// Check for TIF
				if (strParam.find( "/tif" ) != string::npos)
				{
					// Set type
					eOutputType = kFileType_Tif;
				}
				// Check for PDF
				else if (strParam.find( "/pdf" ) != string::npos)
				{
					// Set type
					eOutputType = kFileType_Pdf;
				}
				else if (strParam.find( "/jpg") != string::npos)
				{
					// Set type
					eOutputType = kFileType_Jpg;
				}
				else
				{
					usage();
					return FALSE;
				}

				// Search the remainder of the parameters for other optional arguments
				bool bRetainAnnotations = false;
				string strUserPassword = "";
				string strOwnerPassword = "";
				long nOwnerPermissions = 0;
				bool bEncryptedPasswords = false;
				for (size_t i=3; i < uiParamCount; i++)
				{
					string strTemp = vecParams[i];
					string strLower = strTemp;
					if (strTemp == "/retain")
					{
						bRetainAnnotations = true;
					}
					else if (strTemp == "/user")
					{
						i++;
						if (i >= uiParamCount)
						{
							usage();
							return FALSE;
						}

						strUserPassword = vecParams[i];
					}
					else if (strTemp == "/owner")
					{
						i++;
						if (i >= uiParamCount)
						{
							usage();
							return FALSE;
						}

						strOwnerPassword = vecParams[i];

						i++;
						if (i >= uiParamCount)
						{
							usage();
							return FALSE;
						}

						try
						{
							nOwnerPermissions = asLong(vecParams[i]);
							if (nOwnerPermissions < 0 || nOwnerPermissions > 255)
							{
								usage();
								return FALSE;
							}
						}
						catch(...)
						{
							usage();
							return FALSE;
						}
					}
					else if (strTemp == "/ef")
					{
						i++;
						if (i >= uiParamCount)
						{
							usage();
							return FALSE;
						}
						// Retrieve filename

						strLocalExceptionLog = vecParams[i];
					}
					// NOTE the /enc argument is an internal use argument and should not
					// be exposed in the usage message
					else if (strTemp == "/enc")
					{
						bEncryptedPasswords = true;
					}
					else
					{
						usage();
						return FALSE;
					}
				}

				if (eOutputType != kFileType_Pdf
					&& (!strUserPassword.empty() || !strOwnerPassword.empty()))
				{
					throw UCLIDException("ELI29766",
						"Cannot apply passwords to non-PDF file type.");
				}

				// Check if the passwords need to be decrypted
				if (bEncryptedPasswords)
				{
					if (!strUserPassword.empty())
					{
						decryptString(strUserPassword);
					}
					if (!strOwnerPassword.empty())
					{
						decryptString(strOwnerPassword);
					}
				}

				// Make sure the image file exists
				::validateFileOrFolderExistence( strInputName );

				// Load license files and validate the license
				LicenseManagement::sGetInstance().loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);
				validateLicense();

				if (bRetainAnnotations)
				{
					// Unlock Leads document support
					unlockDocumentSupport();
				}

				// Unlock support for PDF Reading and Writing
				// (Only unlocks if PDF support is licensed)
				initPDFSupport();

				// Convert the file
				convertImage(strInputName, strOutputName, eOutputType, bRetainAnnotations,
					this->m_hInstance, strUserPassword, strOwnerPassword, nOwnerPermissions);

				// No UI needed, just return
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI15898");
		}
		catch(UCLIDException& ue)
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
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15899");

	// Since the dialog has been closed, return FALSE so that we exit the
	//  application, rather than start the application's message pump.
	return nExitCode;
}
//-------------------------------------------------------------------------------------------------