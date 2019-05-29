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
#include <Misc.h>
#include <MiscLeadUtils.h>
#include <LeadtoolsBitmapFreeer.h>
#include <StringCSIS.h>
#include <PdfSecurityValues.h>
#include <ByteStream.h>
#include <ByteStreamManipulator.h>
#include <EncryptionEngine.h>
#include <RegistryPersistenceMgr.h>
#include <RegConstants.h>
#include <KernelAPI.h>
#include <ScansoftErr.h>
#include <OcrMethods.h>

#include "..\..\..\..\ReusableComponents\COMComponents\UCLIDRasterAndOCRMgmt\OCREngines\SSOCR2\Code\OcrConstants.h"

#include <string>
#include <set>
#include <unordered_map>
#include <LeadToolsLicenseRestrictor.h>

using namespace std;

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

// Registry settings for whether an image should be expanded or not when converting to PDF
const string gstrIMAGE_FORMAT_CONVERTER = "\\ImageFormatConverter";
const string gstrEXPAND_FOR_PDF = "ExpandImageForPdfConversion";
const string gstrDEFAULT_EXPAND_FOR_PDF = "1";

//-------------------------------------------------------------------------------------------------
// Macros
//-------------------------------------------------------------------------------------------------

// the following macro is used to simplify the process of throwing exception 
// when a RecAPI call has failed
#define THROW_UE(strELICode, strExceptionText, rc) \
	{ \
		UCLIDException ue(strELICode, strExceptionText); \
		loadScansoftRecErrInfo(ue, rc); \
		throw ue; \
	}

// the following macro is used to simplify the process of checking the return code
// from the RecApi calls and throwing exception if the return code is not REC_OK
#define THROW_UE_ON_ERROR(strELICode, strExceptionText, RecAPICall) \
	{ \
		RECERR rc = ##RecAPICall; \
		if (rc != REC_OK) \
		{ \
			THROW_UE(strELICode, strExceptionText, rc); \
		} \
	}

//-------------------------------------------------------------------------------------------------
// RecMemoryReleaser
//-------------------------------------------------------------------------------------------------
template<typename MemoryType>
RecMemoryReleaser<MemoryType>::RecMemoryReleaser(MemoryType* pMemoryType)
 : m_pMemoryType(pMemoryType)
{

}
//-------------------------------------------------------------------------------------------------
template<>
RecMemoryReleaser<tagIMGFILEHANDLE>::~RecMemoryReleaser()
{
	try
	{
		// The image may have been closed before the call to destructor. Don't both checking error
		// code.
		kRecCloseImgFile(m_pMemoryType);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI34283");
}
//-------------------------------------------------------------------------------------------------
template<>
RecMemoryReleaser<RECPAGESTRUCT>::~RecMemoryReleaser()
{
	try
	{
		RECERR rc = kRecFreeRecognitionData(m_pMemoryType);

		// log any errors
		if (rc != REC_OK)
		{
			UCLIDException ue("ELI34284", 
				"Application trace: Unable to release recognition data. Possible memory leak.");
			loadScansoftRecErrInfo(ue, rc);
			ue.log();
		}

		rc = kRecFreeImg(m_pMemoryType);

		// log any errors
		if (rc != REC_OK)
		{
			UCLIDException ue("ELI34285", 
				"Application trace: Unable to release page image. Possible memory leak.");
			loadScansoftRecErrInfo(ue, rc);
			ue.log();
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI34286");
}

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
	MapLabel encryptionEngine;
	encryptionEngine.getMapLabel(decrypted, bytes, bytesKey);

	// Get the decrypted string
	ByteStreamManipulator bsm (ByteStreamManipulator::kRead, decrypted);
	bsm >> rstrEncryptedString;
}
//-------------------------------------------------------------------------------------------------
// Gets the padding distance necessary when converting to a PDF
int getPadDistance(int iDimension)
{
	int iPadDistance = 0;
	int iMod = iDimension % 25;
	if (iMod > 8)
	{
		iPadDistance = 25 - iMod;
	}
	else if (iMod == 1 || iMod == 5)
	{
		iPadDistance = 3;
	}
	else if (iMod == 2 || iMod == 6)
	{
		iPadDistance = 2;
	}
	else if (iMod == 3 || iMod == 7)
	{
		iPadDistance = 1;
	}

	return iPadDistance;
}
//-------------------------------------------------------------------------------------------------
bool expandImageWhenConvertingPdf()
{
	RegistryPersistenceMgr regMgr(HKEY_CURRENT_USER, gstrREG_ROOT_KEY + "\\Utilities");
	if (!regMgr.keyExists(gstrIMAGE_FORMAT_CONVERTER, gstrEXPAND_FOR_PDF))
	{
		regMgr.createKey(gstrIMAGE_FORMAT_CONVERTER, gstrEXPAND_FOR_PDF, gstrDEFAULT_EXPAND_FOR_PDF);
	}

	return asCppBool(regMgr.getKeyValue(gstrIMAGE_FORMAT_CONVERTER, gstrEXPAND_FOR_PDF,
		gstrDEFAULT_EXPAND_FOR_PDF));
}
//-------------------------------------------------------------------------------------------------
void initNuanceEngineAndLicense()
{
	try
	{
		// initialize the OEM license using the license file that is expected to exist
		// in the same directory as this DLL 
		RECERR rc = kRecSetLicense(__nullptr, gpszOEM_KEY);
		if (rc != REC_OK && rc != API_INIT_WARN)
		{
			// create the exception object to throw to outer scope
			try
			{
				THROW_UE("ELI34307", "Unable to load Nuance engine license file!", rc);
			}
			catch (UCLIDException& ue)
			{
				loadScansoftRecErrInfo(ue, rc);
				throw ue;
			}
		}

		// Initialization of OCR engine	
		rc = kRecInit("Extract Systems", "ImageFormatConverter");
		if (rc != REC_OK && rc != API_INIT_WARN)
		{
			// create the exception object to throw to outer scope
			THROW_UE("ELI34308", "Unable to initialize Nuance engine!", rc);
		}

		// if rc is API_INIT_WARN, ensure that the required modules are available
		if (rc == API_INIT_WARN)
		{
			LPKRECMODULEINFO pModules;
			size_t size;
			THROW_UE_ON_ERROR("ELI34298", "Unable to obtain modules information from the Nuance engine!",
				kRecGetModulesInfo(&pModules, &size));
			
			// if a required library module is not there, do not continue.
			if (pModules[INFO_MOR].Version <= 0)
			{
				THROW_UE("ELI34299", "Unable to find required MOR module for Nuance engine to run.", rc);
			}
			if(pModules[INFO_MTX].Version <= 0)
			{
				THROW_UE("ELI34300", "Unable to find required MTX module for Nuance engine to run.", rc);
			}
			if (pModules[INFO_PLUS2W].Version <= 0)
			{
				THROW_UE("ELI34301", "Unable to find required PLUS2W module for Nuance engine to run.", rc);
			}
			if (pModules[INFO_PLUS3W].Version <= 0)
			{
				THROW_UE("ELI34302", "Unable to find required PLUS3W module for Nuance engine to run.", rc);
			}
			if (pModules[INFO_HNR].Version <= 0)
			{
				THROW_UE("ELI34303", "Unable to find required HNR module for Nuance engine to run.", rc);
			}
			if (pModules[INFO_RER].Version <= 0)
			{
				THROW_UE("ELI34304", "Unable to find required RER module for Nuance engine to run.", rc);
			}
			if(pModules[INFO_DOT].Version <= 0)
			{
				THROW_UE("ELI34305", "Unable to find required DOT module for Nuance engine to run.", rc);
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI34306")
}
//-------------------------------------------------------------------------------------------------
// Will perform the image conversion
void nuanceConvertImage(const string strInputFileName, const string strOutputFileName, 
				  EConverterFileType eOutputType, bool bPreserveColor, string strPagesToRemove,
				  IMF_FORMAT nExplicitFormat, int nCompressionLevel)
{
	// [LegacyRCAndUtils:6275]
	// Since we will be using the Nuance engine, ensure we are licensed for it.
	VALIDATE_LICENSE( gnOCR_ON_CLIENT_FEATURE, "ELI34323", "ImageFormatConverter" );

	IMF_FORMAT nFormat(FF_TIFNO);
	bool bOutputImageOpened(false);
	int nPage(0);
	unique_ptr<HIMGFILE> uphOutputImage(__nullptr);
	unique_ptr<TemporaryFileName> pTempOutputFile;

	try
	{
		try
		{
			// initialize the Nuance engine and any necessary licensing thereof
			initNuanceEngineAndLicense();

			HIMGFILE hInputImage;
			THROW_UE_ON_ERROR("ELI34295", "Unable to open source image file.",
				kRecOpenImgFile(strInputFileName.c_str(), &hInputImage, IMGF_READ, FF_SIZE));

			// Ensure that the memory stored for the image file is released
			RecMemoryReleaser<tagIMGFILEHANDLE> inputImageFileMemoryReleaser(hInputImage);

			int nPageCount = 0;
			THROW_UE_ON_ERROR("ELI37426", "Unable to get page count.",
				kRecGetImgFilePageCount(hInputImage, &nPageCount));

			// Set format and temp file extension based on the ouput type. If the extension doesn't
			// match the format, the nuance engine will throw and error when saving.
			string strExt;
			switch (eOutputType)
			{
				case kFileType_Tif:
					strExt = ".tif";
					break;
				case kFileType_Pdf:
					strExt = ".pdf";
					break;

				case kFileType_Jpg:
					strExt = ".jpg";
					break;
			}

			if (nCompressionLevel > 0)
			{
				kRecSetCompressionLevel(0, nCompressionLevel);
			}

			// Create a temporary file into which the results should be written until the entire
			// document has been output.
			pTempOutputFile.reset(new TemporaryFileName(true, NULL, strExt.c_str()));
			string strTempOutputFileName = pTempOutputFile->getName();
			// If the destination file name exists before Nuance tries to open it, it will throw an error.
			deleteFile(strTempOutputFileName);

			// Don't use RecMemoryReleaser on the output image because we will need to manually
			// close it at the end of this method to be able to copy it to its permanent location.
			uphOutputImage.reset(new HIMGFILE);
			THROW_UE_ON_ERROR("ELI34296", "Unable to create destination image file.",
				kRecOpenImgFile(strTempOutputFileName.c_str(), uphOutputImage.get(), IMGF_RDWR, FF_SIZE));

			bOutputImageOpened = true;

			if (nPageCount > 1 && eOutputType == kFileType_Jpg)
			{
				throw UCLIDException("ELI34293", "Cannot output multi-page image in jpg format.");
			}

			// [LegacyRCAndUtils:6461]
			// Do not include any pages from strPagesToRemove in the output.
			set<int> setPagesToRemove;
			if (!strPagesToRemove.empty())
			{
				vector<int> vecPagesToRemove = getPageNumbers(nPageCount, strPagesToRemove, true);
				setPagesToRemove = set<int>(vecPagesToRemove.begin(), vecPagesToRemove.end());
			}

			IMG_INFO imgInfo = {0};
			bool bOuputAtLeastOnePage = false;
			bool bConversionSet = false;
			for (nPage = 0; nPage < nPageCount; nPage++)
			{
				if (setPagesToRemove.find(nPage + 1) != setPagesToRemove.end())
				{
					continue;
				}

				bOuputAtLeastOnePage = true;

				nFormat = FF_TIFNO;
				if (nExplicitFormat >= 0)
				{
					nFormat = nExplicitFormat;
				}
				else if (eOutputType == kFileType_Tif && !bPreserveColor)
				{
					nFormat = FF_TIFG4;
				}
				else if (eOutputType == kFileType_Jpg)
				{
					nFormat = FF_JPG_SUPERB;
				}
				else
				{
					IMF_FORMAT imgFormat;
					THROW_UE_ON_ERROR("ELI36840", "Failed to identify image format.",
						kRecGetImgFilePageInfo(0, hInputImage, nPage, &imgInfo, &imgFormat));
					int nBitsPerPixel = imgInfo.BitsPerPixel;

					switch (eOutputType)
					{
					case kFileType_Tif:
					{
						nFormat = (nBitsPerPixel == 1) ? FF_TIFG4 : FF_TIFLZW;
					}
					break;

					case kFileType_Pdf:
					{
						// FF_PDF_SUPERB was causing unacceptable growth in PDF size in some cases for color
						// documents. For the time being, unless a document is bitonal, use FF_PDF_GOOD rather than
						// FF_PDF_SUPERB.
						nFormat = (nBitsPerPixel == 1) ? FF_PDF_SUPERB : FF_PDF_GOOD;
					}
					break;
					}
				}

				// https://extract.atlassian.net/browse/ISSUE-12162
				// kRecSetImgConvMode(0, CNV_AUTO) should be called only in the case that we are
				// outputting tif files without preserving color depth, otherwise color information will
				// be stripped out.
				if (eOutputType == kFileType_Tif
					&& (!bPreserveColor || nFormat != FF_TIFNO && nFormat != FF_TIFPB && nFormat != FF_TIFLZW))
				{
					if (!bConversionSet)
					{
						THROW_UE_ON_ERROR("ELI37423", "Unable to set image conversion method.",
							kRecSetImgConvMode(0, CNV_AUTO));
						bConversionSet = true;
					}
				}
				else if (bConversionSet)
				{
					THROW_UE_ON_ERROR("ELI43553", "Unable to set image conversion method.",
						kRecSetImgConvMode(0, CNV_NO));
					bConversionSet = false;
				}

				// NOTE: RecAPI uses zero-based page number indexes
				HPAGE hImagePage;
				loadPageFromImageHandle(strInputFileName, hInputImage, nPage, &hImagePage);

				// Ensure that the memory stored for the image page is released.
				RecMemoryReleaser<RECPAGESTRUCT> pageMemoryReleaser(hImagePage);

				if (eOutputType == kFileType_Jpg)
				{
					THROW_UE_ON_ERROR("ELI34291", "Cannot save to image page in jpg format.",
						kRecSaveImgForce(0, *uphOutputImage, nFormat, hImagePage, II_CURRENT, FALSE));
				}
				else
				{
					THROW_UE_ON_ERROR("ELI34292", "Cannot save to image page in the specified format.",
						kRecSaveImg(0, *uphOutputImage, nFormat, hImagePage, II_CURRENT, TRUE));
				}
			}

			// [ImageFormatConverter:6469]
			// If all pages were removed via the /RemovePages option, throw an exception.
			if (!bOuputAtLeastOnePage)
			{
				UCLIDException uex("ELI36149", "RemovePages option not valid; all pages removed");
				uex.addDebugInfo("Input File", strInputFileName);
				uex.addDebugInfo("Input Page Count", nPageCount);
				uex.addDebugInfo("RemovePages option", strPagesToRemove);
				throw uex;
			}

			nPage = -1;

			// Close manually; if it is not closed before copying it to the permanent location, it
			// may be copied in a corrupted state.
			kRecCloseImgFile(*uphOutputImage);

			// Ensure the outut directory exists
			string strOutDir = getDirectoryFromFullPath(strOutputFileName);
			if (!isValidFolder(strOutDir))
			{
				createDirectory(strOutDir);
			}

			copyFile(strTempOutputFileName, strOutputFileName);
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI34281");
	}
	catch (UCLIDException &ue)
	{
		// We need to close the out image file if the output image file was opened but we didn't
		// make it to the "happy case" close call.
		if (bOutputImageOpened && nPage != -1)
		{
			try
			{
				kRecCloseImgFile(*uphOutputImage);
			}
			CATCH_AND_LOG_ALL_EXCEPTIONS("ELI34312");
		}

		ue.addDebugInfo("Source image", strInputFileName);
		ue.addDebugInfo("Page", asString(nPage + 1));
		ue.addDebugInfo("Format", asString((int)nFormat));
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
// Will perform the image conversion and will also retain the existing annotations
// if bRetainAnnotations is true.
// If the output format is a tif then the annotations will remain as annotations
// if the output format is a pdf then the redaction annotations will be burned into the image.
void convertImage(const string strInputFileName, const string strOutputFileName, 
				  EConverterFileType eOutputType, bool bRetainAnnotations, HINSTANCE hInst,
				  const string& strUserPassword, const string& strOwnerPassword, long nPermissions,
				  long nViewPerspective, bool bPreserveColor, string strPagesToRemove)
{
	HANNOBJECT hFileContainer = NULL;
	ANNENUMCALLBACK pfnCallBack = NULL;
	try
	{
		try
		{
			// Create a temporary file for the output [LRCAU #5583]
			TemporaryFileName tmpOutput(true, "", NULL,
				getExtensionFromFullPath(strOutputFileName).c_str(), true);
			const string& strTempOut = tmpOutput.getName();

			L_INT nRet = FAILURE;

			// Get the file info
			FILEINFO fileInfo;
			getFileInformation(strInputFileName, true, fileInfo); 

			// Get the total number of pages from the file info
			L_INT nPages = fileInfo.TotalPages;

			// Get the file format
			int iFormat = fileInfo.Format;

			// Get initialized SAVEFILEOPTION struct
			SAVEFILEOPTION sfOptions = GetLeadToolsSizedStruct<SAVEFILEOPTION>(0);
			{
				LeadToolsLicenseRestrictor leadToolsLicenseGuard;
				L_GetDefaultSaveFileOption(&sfOptions, sizeof(sfOptions));
			};

			// Set type-specific output options
			L_INT	nType;
			L_INT   nQFactor = PQ1;
			int		nBitsPerPixel = fileInfo.BitsPerPixel; 
			bool	bBurnAnnotations = false;
			unique_ptr<PDFSecuritySettings> pSecuritySettings(__nullptr);
			switch (eOutputType)
			{
			case kFileType_Pdf:
				{
					// Check for security settings for PDF files
					if (!strUserPassword.empty() || !strOwnerPassword.empty())
					{
						pSecuritySettings.reset(new PDFSecuritySettings(strUserPassword,
							strOwnerPassword, nPermissions, true));
					}

					nBitsPerPixel = fileInfo.BitsPerPixel;					

					// Set output format
					nType = FILE_UNKNOWN_FORMAT;
					if (nBitsPerPixel == 1)
					{
						nType = FILE_RAS_PDF_G4;
					}
					else
					{
						nType = FILE_RAS_PDF_JPEG;

						// https://extract.atlassian.net/browse/ISSUE-15132
						// FILE_RAS_PDF_JPEG is 24-bit JPEG and fails if color depth is 32-bit, e.g.
						// Allow 8 or 24 bit per documentation
						if (nBitsPerPixel != 8)
						{
							nBitsPerPixel = 24;
						}
					}
					nQFactor = getCompressionFactor(nType);

					// This flag will cause the out put image to have the same( or nearly the same ) 
					// dimensions as original input file
					sfOptions.Flags = ESO_PDF_SAVE_USE_BITMAP_DPI;

					// Set the burn annotations flag to true
					bBurnAnnotations = true;
					break;
				}

			case kFileType_Tif:
				// Set output format
				nBitsPerPixel = bPreserveColor ? nBitsPerPixel : 1;
				nType = (nBitsPerPixel == 1) ? FILE_CCITT_GROUP4 : FILE_TIFLZW;
				nQFactor = QS;
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
			// IgnoreViewPerspective to avoid a black region at the bottom of the image except if
			// the user wants to manually specify the view perspective in which case we should load
			// with the original perspective, then convert to the desired perspective.
			LOADFILEOPTION lfo = GetLeadToolsSizedStruct<LOADFILEOPTION>(
				(nViewPerspective > 0) ? 0 : ELO_IGNOREVIEWPERSPECTIVE);

			// Get the input file name as a char*
			char* pszInputFile = (char*) strInputFileName.c_str();

			// Whether the bitmap needs to be expanded before converting it to a PDF
			bool bExpandForPdfConversion =
				eOutputType == kFileType_Pdf && expandImageWhenConvertingPdf();

			// [LegacyRCAndUtils:6461]
			// Do not include any pages from strPagesToRemove in the output.
			set<int> setPagesToRemove;
			if (!strPagesToRemove.empty())
			{
				vector<int> vecPagesToRemove = getPageNumbers(nPages, strPagesToRemove, true);
				setPagesToRemove = set<int>(vecPagesToRemove.begin(), vecPagesToRemove.end());
			}

			// Handle pages individually to deal with situation where existing annotations
			// need to be retained
			int nOutputPageNum = 0;
			for (int i = 1; i <= nPages; i++)
			{
				if (setPagesToRemove.find(i) != setPagesToRemove.end())
				{
					continue;
				}

				// Set FILEINFO_FORMATVALID (this will speed up the L_LoadBitmap calls)
				fileInfo = GetLeadToolsSizedStruct<FILEINFO>(FILEINFO_FORMATVALID);
				fileInfo.Format = iFormat;

				// Set the 1-relative page number in the LOADFILEOPTION structure 
				lfo.PageNumber = i;

				BITMAPHANDLE hBitmap = {0};
				LeadToolsBitmapFreeer freer(hBitmap);
				loadImagePage(strInputFileName, hBitmap, fileInfo, lfo, false);

				// Convert view perspective is specified.
				if (nViewPerspective > 0)
				{
					LeadToolsLicenseRestrictor leadToolsLicenseGuard;

					nRet = L_ChangeBitmapViewPerspective(&hBitmap, &hBitmap, sizeof(BITMAPHANDLE), nViewPerspective);
					throwExceptionIfNotSuccess(nRet, "ELI34136", 
						"ChangeBitmapViewPerspective operation failed.", strInputFileName); 
				}

				// Load the existing annotations if bRetainAnnotations and they exist.
				if(bRetainAnnotations && hasAnnotations(strInputFileName, lfo, iFormat))
				{
					LeadToolsLicenseRestrictor leadToolsLicenseGuard;

					nRet = L_AnnLoad(pszInputFile, &hFileContainer, &lfo);
					throwExceptionIfNotSuccess(nRet, "ELI23584", "Could not load annotations.",
						strInputFileName);
				}

				// Check for NULL or empty container
				bool bSavedTag = false;
				if (hFileContainer != __nullptr)
				{
					LeadToolsLicenseRestrictor leadToolsLicenseGuard;

					// Retrieve the first annotation item
					HANNOBJECT	hFirst = NULL;
					nRet = L_AnnGetItem( hFileContainer, &hFirst );
					throwExceptionIfNotSuccess( nRet, "ELI23585", 
						"Could not get item from annotation container." );

					if (hFirst != __nullptr)
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

				// Ensure PDF will load/save with same width and height, may need to pad pixels
				if (bExpandForPdfConversion)
				{
					// Get the amount of width and height padding needed
					int iPadWidth = getPadDistance(fileInfo.Width);
					int iPadHeight = getPadDistance(fileInfo.Height);

					// Adjust bitmap resolution to 300x300
					hBitmap.XResolution = 300;
					hBitmap.YResolution = 300;

					// If padding is necessary, pad the bitmap
					if (iPadWidth > 0 || iPadHeight > 0)
					{
						// Create a new bitmap handle
						BITMAPHANDLE tmpBmp = {0};
						LeadToolsBitmapFreeer tmpFree(tmpBmp);

						LeadToolsLicenseRestrictor leadToolsLicenseGuard;

						nRet = L_CreateBitmap(&tmpBmp, sizeof(BITMAPHANDLE), TYPE_CONV,
							fileInfo.Width + iPadWidth, fileInfo.Height + iPadHeight,
							fileInfo.BitsPerPixel, fileInfo.Order, hBitmap.pPalette, TOP_LEFT, NULL, 0);
						throwExceptionIfNotSuccess(nRet, "ELI29936", "Unable to create empty bitmap.");
						tmpBmp.XResolution = 300;
						tmpBmp.YResolution = 300;

						// Set the bitmap to all white pixels
						nRet = L_FillBitmap(&tmpBmp, RGB(255,255,255));
						throwExceptionIfNotSuccess(nRet, "ELI29937", "Unable to fill bitmap.");

						// Copy in the original image pixels
						nRet = L_CopyBitmapRect(&tmpBmp, &hBitmap, sizeof(BITMAPHANDLE), 0, 0, fileInfo.Width, fileInfo.Height);
						throwExceptionIfNotSuccess(nRet, "ELI29938", "Unable to combine bitmaps.");

						// Free the original bitmap
						L_FreeBitmap(&hBitmap);

						// Copy back the new larger bitmap
						nRet = L_CopyBitmap(&hBitmap, &tmpBmp, sizeof(BITMAPHANDLE));
						throwExceptionIfNotSuccess(nRet, "ELI29939", "Unable to copy bitmap.");
					}
				}

				// If any pages are specified in strPagesToRemove, the output page number will
				// differ from the source page number.
				nOutputPageNum++;

				// Set the page number
				sfOptions.PageNumber = nOutputPageNum;

				// Save the image page
				saveImagePage(hBitmap, strTempOut, nType, nQFactor, nBitsPerPixel, sfOptions);

				if (bSavedTag)
				{
					LeadToolsLicenseRestrictor leadToolsLicenseGuard;

					// Clear any previously defined annotations
					// If not done, any annotations applied to this page may be applied to 
					// successive pages (P16 #2216)
					nRet = L_SetTag( ANNTAG_TIFF, 0, 0, NULL );
				}
			}	// end for each page

			// [ImageFormatConverter:6469]
			// If all pages were removed via the /RemovePages option, throw an exception.
			if (nOutputPageNum == 0)
			{
				UCLIDException uex("ELI36148", "RemovePages option not valid; all pages removed");
				uex.addDebugInfo("Input File", strInputFileName);
				uex.addDebugInfo("Input Page Count", nPages);
				uex.addDebugInfo("RemovePages option", strPagesToRemove);
				throw uex;
			}
				
			// Wait for the file to be readable before continuing
			waitForFileToBeReadable(strTempOut);

			// Check for a matching page count
			long nOutPages = getNumberOfPagesInImage(strTempOut);
			if ((nPages - setPagesToRemove.size()) != nOutPages)
			{
				UCLIDException uex("ELI28839", "Output page count mismatch.");
				uex.addDebugInfo("Input File", strInputFileName);
				uex.addDebugInfo("Input Page Count", nPages);
				uex.addDebugInfo("Temporary Output File", strTempOut);
				uex.addDebugInfo("Temporary Output Page Count", nOutPages);
				uex.addDebugInfo("Output File", strOutputFileName);
				throw uex;
			}

			// Ensure the output directory exists
			string strOutDir = getDirectoryFromFullPath(strOutputFileName);
			if (!isValidFolder(strOutDir))
			{
				createDirectory(strOutDir);
			}

			// Move the temporary file to the output file location
			// (overwrite the output file if it exists)
			moveFile(strTempOut, strOutputFileName, true );
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI23592");
	}
	catch(UCLIDException& uex)
	{
		// Clean up resources
		if (pfnCallBack != __nullptr)
		{
			// Free the callback proc instance
			FreeProcInstance((FARPROC) BurnRedactions);
		}
		if (hFileContainer != __nullptr)
		{
			try
			{
				LeadToolsLicenseRestrictor leadToolsLicenseGuard;

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
			LeadToolsLicenseRestrictor leadToolsLicenseGuard;

			// Clear any previously defined annotations
			// If not done, any annotations applied to this page may be applied to 
			// successive pages (P16 #2216)
			L_SetTag( ANNTAG_TIFF, 0, 0, NULL );
		}

		throw uex;
	}
}
//-------------------------------------------------------------------------------------------------
unordered_map<string, pair<IMF_FORMAT, string>> getFormats()
{
	unordered_map<string, pair<IMF_FORMAT, string>> mapFormats;
	mapFormats["tifno"]           = make_pair(FF_TIFNO, "           \tUncompressed TIFF image format.");
	mapFormats["tifpb"]           = make_pair(FF_TIFPB, "           \tPackbits TIFF image format.");
	mapFormats["tifhu"]           = make_pair(FF_TIFHU, "           \tGroup 3 Modified TIFF image format.");
	mapFormats["tifg31"]          = make_pair(FF_TIFG31, "          \tStandard G3 1D TIFF image format.");
	mapFormats["tifg32"]          = make_pair(FF_TIFG32, "          \tStandard G3 2D TIFF image format.");
	mapFormats["tifg4"]           = make_pair(FF_TIFG4, "           \tStandard G4 TIFF image format.");
	mapFormats["tiflzw"]          = make_pair(FF_TIFLZW, "          \tTIFF-LZW image format incorporating Unisys compression.");
	mapFormats["jpg"]             = make_pair(FF_JPG, "             \tJPEG format with configurable compression level (1-5).");
	mapFormats["jpg_superb"]      = make_pair(FF_JPG_SUPERB, "      \t(deprecated) JPEG format with negligible information loss.");
	mapFormats["jpg_good"]        = make_pair(FF_JPG_GOOD, "        \t(deprecated) JPEG format with average information loss.  (Results in medium-size image files when saving.)");
	mapFormats["jpg_min"]         = make_pair(FF_JPG_MIN, "         \t(deprecated) JPEG format optimized for minimum image file size. Worst image quality.");
	mapFormats["jpg2k"]           = make_pair(FF_JPG2K, "           \tJPEG2000 format with configurable compression level (1-5).");
	mapFormats["pdf"]             = make_pair(FF_PDF, "             \tAdobe PDF format with configurable compression level (1-5).");
	mapFormats["pdf_min"]         = make_pair(FF_PDF_MIN, "         \t(deprecated) Adobe PDF format. Minimum image file size.");
	mapFormats["pdf_good"]        = make_pair(FF_PDF_GOOD, "        \t(deprecated) Adobe PDF format. Results in medium-size image files when saving.");
	mapFormats["pdf_superb"]      = make_pair(FF_PDF_SUPERB, "      \t(deprecated) Adobe PDF format with negligible information loss.");
	mapFormats["pdf_mrc"]         = make_pair(FF_PDF_MRC, "         \tAdobe PDF format with MRC technology with configurable compression level.");
	mapFormats["pdf_mrc_min"]     = make_pair(FF_PDF_MRC_MIN, "     \t(deprecated) Adobe PDF format with MRC technology. Optimized for minimum image file size.");
	mapFormats["pdf_mrc_good"]    = make_pair(FF_PDF_MRC_GOOD, "    \t(deprecated) Adobe PDF format with MRC technology. (Results in medium-size image files when saving.)");
	mapFormats["pdf_mrc_superb"]  = make_pair(FF_PDF_MRC_SUPERB, "  \t(deprecated) Adobe PDF format with MRC technology. PDF with small information loss.");
	return mapFormats;
}
//-------------------------------------------------------------------------------------------------
void usage()
{
	string strUsage = "This application has 3 required arguments and 10 optional arguments:\n";
		strUsage += "An input image file (.tif or .pdf) \n"
					"An output image file (.pdf or .tif) \n"
					"An output file type (/pdf, /tif or /jpg).\n\n"
					"The optional argument (/retain) will cause any redaction annotations to be burned into the resulting image (if the source is tif and destination \n"
					"\tis a pdf or jpg, if source and dest are both tif then all annotations are retained, if the source is pdf then there are no annotations to retain).\n"
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
					"The optional argument /vp [perspective_id] will set the view perspective of the output to the specified value (1-8) or to 1 (top-left) if the perspective_id is not specified.\n"
					"The optional argument /am will use an alternate method to perform the conversion. This option is not compatible with any other optional argument except '/RemovePages', '/ef', '/color' and '/format'. \n"
					"The optional argument /RemovePages will exclude the specified pages from the output. The pages can be specified as an individual page number, a comma-separated list, \n"
					"\ta range of pages denoted with a hyphen, or a dash followed by a number to indicate the last x pages should be removed. \n"
					"The optional argument /color will preserve the color depth of the source image even if the output is a tif image. If this option is not used, all tif output images will be bitonal regardless of source bit depth. \n"
					"The optional argument (/ef <filename>) fully specifies the location of an exception log that will store any thrown exception. Without an exception log, any thrown exception will be displayed.\n"
					"The optional argument (/format <format>) allows specification of the Nuance file format (only works when '/am' is specified). Available formats:\n";
		auto mapFormats = getFormats();
		map<IMF_FORMAT, pair<string, string>> mapOrderedFormats;
		for (auto it = mapFormats.begin(); it != mapFormats.end(); ++it)
		{
			mapOrderedFormats[it->second.first] = make_pair(it->first, it->second.second);
		}
		for (auto it = mapOrderedFormats.begin(); it != mapOrderedFormats.end(); ++it)
		{
			strUsage += " \t\t" + it->second.first + it->second.second + "\n";
		}
		strUsage += "The optional argument (/compression <1-5>) allows specification of the compression level for applicable Nuance file formats (pdf, pdf_mrc, jpg, jpg2k).\n";
		strUsage += "\twhere 1 is the highest level of compression and 5 is the weakest level.\n";
		strUsage += "\nUsage:\n";
		strUsage += "ImageFormatConverter.exe <strInput> <strOutput> <out_type> [/retain] "
					"[/user \"<Password>\"] [/owner \"<Password>\" <Permissions>] [/vp [perspective_id]] "
					"[/am] [/RemovePages \"<Pages>\"] [/color] [/ef <filename>] [/format <format>] [/compression <level>]\n"
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

				// Make sure the number of parameters is 3 or 15
				size_t uiParamCount = vecParams.size();
				if ((uiParamCount < 3) || (uiParamCount > 15))
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
				long nViewPerspective = 0;
				bool bUseNuance = false;
				bool bPreserveColor = false;
				string strPagesToRemove;
				IMF_FORMAT eExplicitFormat = (IMF_FORMAT)-1;
				int nCompressionLevel = -1;
				for (size_t i=3; i < uiParamCount; i++)
				{
					string strTemp = vecParams[i];
					makeLowerCase(strTemp);
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
					else if (strTemp == "/vp")
					{
						// Use a view perspective of 1 (top-left) unless the user has specified the
						// perspective to use with the following parameter.
						nViewPerspective = 1;
						
						// Check for a parameter that specifies which view perspective to use.
						if (i < uiParamCount - 1)
						{
							string strNextParam = vecParams[i + 1];
							if (strNextParam.length() == 1 && isDigitChar(strNextParam[0]))
							{
								nViewPerspective = asLong(strNextParam);
								if (nViewPerspective < 1 || nViewPerspective > 8)
								{
									usage();
									return FALSE;
								}

								i++;
							}
						}
					}
					else if (strTemp == "/am")
					{
						bUseNuance = true;
					}
					// [LegacyRCAndUtils:6461]
					// Allows specified pages to be excluded from the output
					else if (strTemp == "/removepages")
					{
						i++;
						if (i >= uiParamCount)
						{
							usage();
							return FALSE;
						}
						
						strPagesToRemove = vecParams[i];
					}
					else if (strTemp == "/color")
					{
						bPreserveColor = true;
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
					else if (strTemp == "/format")
					{
						if (++i == uiParamCount)
						{
							usage();
							return FALSE;
						}

						unordered_map<string, pair<IMF_FORMAT, string>> mapFormats = getFormats();
						strTemp = vecParams[i];
						makeLowerCase(strTemp);
						auto search = mapFormats.find(strTemp);
						if (search == mapFormats.end())
						{
							usage();
							return FALSE;
						}
						eExplicitFormat = search->second.first;
					}
					else if (strTemp == "/compression")
					{
						if (++i == uiParamCount)
						{
							usage();
							return FALSE;
						}

						strTemp = vecParams[i];
						try
						{
							nCompressionLevel = asLong(strTemp);
						}
						catch (...) {}

						if (nCompressionLevel < 1 || nCompressionLevel > 5)
						{
							string strMsg = "Invalid compression level! Expecting 1-5, got '" + strTemp + "'";
							AfxMessageBox(strMsg.c_str());
							return FALSE;
						}
					}
					else
					{
						usage();
						return FALSE;
					}
				}

				if (!bUseNuance && eExplicitFormat >= 0)
				{
					throw UCLIDException("ELI43551",
						"Cannot specify /format without specifying /am");
				}

				if (eOutputType != kFileType_Pdf
					&& (!strUserPassword.empty() || !strOwnerPassword.empty()))
				{
					throw UCLIDException("ELI29766",
						"Cannot apply passwords to non-PDF file type.");
				}

				CoInitializeEx(NULL, COINIT_MULTITHREADED);

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
				LicenseManagement::loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);
				validateLicense();

				InitLeadToolsLicense();

				// [LegacyRCAndUtils:6471]
				if (!strPagesToRemove.empty())
				{
					VALIDATE_LICENSE(gnREMOVE_IMAGE_PAGES, "ELI36150", "RemovePages" );
				}

				if (bUseNuance)
				{
					if (bRetainAnnotations || !strUserPassword.empty() || !strOwnerPassword.empty() ||
						nOwnerPermissions != 0 || nViewPerspective != 0)
					{
						usage();
						return FALSE;
					}

					nuanceConvertImage(strInputName, strOutputName, eOutputType, bPreserveColor,
						strPagesToRemove, eExplicitFormat, nCompressionLevel);
				}
				else if (!(isPDFFile(strInputName) || eOutputType == kFileType_Pdf) || LicenseManagement::isPDFLicensed())
				{
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
						this->m_hInstance, strUserPassword, strOwnerPassword, nOwnerPermissions,
						nViewPerspective, bPreserveColor, strPagesToRemove);
				}
				else
				{
					UCLIDException noLeadToolsPDF("ELI46777", "Leadtools PDF Read+Write needs to be licensed");
					throw noLeadToolsPDF;
				}
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

	CoUninitialize();

	// Since the dialog has been closed, return FALSE so that we exit the
	//  application, rather than start the application's message pump.
	return nExitCode;
}
//-------------------------------------------------------------------------------------------------
