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
#include <StringCSIS.h>

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
// Will perform the image conversion and will also retain the existing annotations
// if bRetainAnnotations is true.
// If the output format is a tif then the annotations will remain as annotations
// if the output format is a pdf then the redaction annotations will be burned into the image.
void convertImage(const string strInputFileName, const string strOutputFileName, 
				  EConverterFileType eOutputType, bool bRetainAnnotations, HINSTANCE hInst)
{
	HANNOBJECT hFileContainer = NULL;
	ANNENUMCALLBACK pfnCallBack = NULL;
	BITMAPHANDLE hBitmap = {0};
	try
	{
		try
		{
			// Provide multi-thread protection for PDF images
			LeadToolsPDFLoadLocker ltInputPDF( strInputFileName );
			LeadToolsPDFLoadLocker ltOutputPDF( strOutputFileName );

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
				// Set output format
				nType = FILE_RAS_PDF_JBIG2;

				// This flag will cause the out put image to have the same( or nearly the same ) 
				// dimensions as original input file
				sfOptions.Flags = ESO_PDF_SAVE_USE_BITMAP_DPI;

				// Set the burn annotations flag to true
				bBurnAnnotations = true;
				break;

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

			// Get the retry count and timeout values
			int iRetryCount(0), iRetryTimeout(0);
			getFileAccessRetryCountAndTimeout(iRetryCount, iRetryTimeout);

			// Get the input and output file names as char*
			char* pszInputFile = (char*) strInputFileName.c_str();
			char* pszOutputFile = (char*) strOutputFileName.c_str();

			// Handle pages individually to deal with situation where existing annotations
			// need to be retained
			for (int i = 1; i <= nPages; i++)
			{
				// Set FILEINFO_FORMATVALID (this will speed up the L_LoadBitmap calls)
				fileInfo = GetLeadToolsSizedStruct<FILEINFO>(FILEINFO_FORMATVALID);
				fileInfo.Format = iFormat;

				// Set the 1-relative page number in the LOADFILEOPTION structure 
				lfo.PageNumber = i;

				// Load the current page from the bitmap
				nRet = L_LoadBitmap(pszInputFile, &hBitmap, sizeof(BITMAPHANDLE), 0, 0,
					&lfo, &fileInfo);
				if (nRet != SUCCESS)
				{
					UCLIDException ue("ELI23583", "Could not obtain page.");
					ue.addDebugInfo("Error Code", nRet);
					ue.addDebugInfo("Error Description", getErrorCodeDescription(nRet));
					ue.addDebugInfo("File Name", strInputFileName);
					ue.addDebugInfo("Page Number", i);
					throw ue;
				}

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

				// Save this page of the original file
				int nNumFailedAttempts = 0;
				while (nNumFailedAttempts < iRetryCount)
				{
					// Save this page of the image
					nRet = L_SaveBitmap( pszOutputFile, &hBitmap, 
						nType, nBitsPerPixel, nQFactor, &sfOptions );

					// Check result
					if (nRet == SUCCESS)
					{
						// Exit loop
						break;
					}
					else
					{
						// Increment counter
						nNumFailedAttempts++;

						// Sleep before retrying the Save
						Sleep( iRetryTimeout );
					}
				}
				if (nRet != SUCCESS)
				{
					UCLIDException ue("ELI23589", "Could not save image page.");
					ue.addDebugInfo("Image To Output", strOutputFileName);
					ue.addDebugInfo("Actual Page", i);
					ue.addDebugInfo("Error description", getErrorCodeDescription(nRet));
					ue.addDebugInfo("Actual Error Code", nRet);
					ue.addDebugInfo("Retries attempted", nNumFailedAttempts);
					ue.addDebugInfo("Max Retries", iRetryCount);
					throw ue;
				}
				else
				{
					if (nNumFailedAttempts > 0)
					{
						UCLIDException ue("ELI23590",
							"Application Trace:Saved image page successfully after retry.");
						ue.addDebugInfo("Retries", nNumFailedAttempts);
						ue.addDebugInfo("Image To Output", strOutputFileName);
						ue.addDebugInfo("Actual Page", i);
						ue.log();
					}
				}

				if (bSavedTag)
				{
					// Clear any previously defined annotations
					// If not done, any annotations applied to this page may be applied to 
					// successive pages (P16 #2216)
					nRet = L_SetTag( ANNTAG_TIFF, 0, 0, NULL );
				}

				// Free the bitmap handle
				if (hBitmap.Flags.Allocated)
				{
					L_FreeBitmap(&hBitmap);
				}
			}	// end for each page

			// Wait for the file to be readable before continuing
			waitForFileToBeReadable(strOutputFileName);
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
		// If a bitmap is allocated, free it
		if (hBitmap.Flags.Allocated)
		{
				L_FreeBitmap(&hBitmap);
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
	string strUsage = "This application has 3 required arguments and 2 optional argument:\n";
		strUsage += "An input image file (.tif or .pdf) and \n"
					"an output image file (.pdf or .tif) and \n"
					"an output file type (/pdf, /tif or /jpg).\n\n"
					"The optional argument (/retain) will cause any redaction annotations to \n"
					"be burned into the resulting image (if the source is tif and destination \n"
					"is a pdf or jpg, if source and dest are both tif then all annotations are \n"
					"retained, if the source is pdf then there are no annotations to retain).\n"
					"The optional argument (/ef <filename>) fully specifies the location \n"
					"of an exception log that will store any thrown exception.  Without \n"
					"an exception log, any thrown exception will be displayed.\n\n";
		strUsage += "Usage:\n";
		strUsage += "ImageFormatConverter.exe <strInput> <strOutput> <out_type> [/ef <filename>]\n"
					"where:\n"
					"out_type is /pdf, /tif or /jpg,\n"
					"<filename> is the fully-qualified path to an exception log.\n\n";
		AfxMessageBox(strUsage.c_str());
}
//-------------------------------------------------------------------------------------------------
void validateLicense()
{
	// Requires PDF license
	static const unsigned long THIS_APP_ID = gnPDF_READWRITE_FEATURE;

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

		CoInitializeEx(NULL, COINIT_MULTITHREADED);

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

				// Make sure the number of parameters is 3 or 5
				unsigned int uiParamCount = (unsigned int)vecParams.size();
				if ((uiParamCount < 3) || (uiParamCount > 6))
				{
					usage();
					return FALSE;
				}

				// Retrieve file names and output type
				string strInputName = vecParams[0];
				string strOutputName = vecParams[1];
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
				
				bool bRetainAnnotations = false;
				// Check for retain annotation flag
				if (uiParamCount == 4 || uiParamCount == 6)
				{
					string strTemp = vecParams[3];
					makeLowerCase(strTemp);
					if (strTemp != "/retain")
					{
						usage();
						return FALSE;
					}

					bRetainAnnotations = true;
				}

				// Check for exception-to-file option
				if (uiParamCount == 5 || uiParamCount == 6)
				{
					// Retrieve /ef argument string
					string strArgument = vecParams[uiParamCount-2];
					makeLowerCase( strArgument );
					if (strArgument != "/ef")
					{
						usage();
						return FALSE;
					}

					// Retrieve filename
					strLocalExceptionLog = vecParams[uiParamCount-1];
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
				if ( LicenseManagement::sGetInstance().isPDFLicensed() )
				{
					initPDFSupport();
				}
				else
				{
					throw UCLIDException("ELI19883", "PDF read/write support is not licensed.");
				}

				// Convert the file
				convertImage( strInputName, strOutputName, eOutputType, bRetainAnnotations, this->m_hInstance );

				// No UI needed, just return
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI15898");
		}
		catch(UCLIDException ue)
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

		CoUninitialize();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15899")

	// Since the dialog has been closed, return FALSE so that we exit the
	//  application, rather than start the application's message pump.
	return nExitCode;
}
//-------------------------------------------------------------------------------------------------