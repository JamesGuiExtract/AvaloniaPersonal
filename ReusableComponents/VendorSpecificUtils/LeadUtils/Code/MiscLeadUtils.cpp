
#include "stdafx.h"
#include "MiscLeadUtils.h"
#include "ImageConversion.h"
#include "LeadToolsBitmapFreeer.h"
#include "LeadToolsFormatHelpers.h"
#include "ExtractZoneAsImage.h"
#include "LocalPDFOptions.h"
#include "LeadToolsLicensing.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <RegConstants.h>
#include <RegistryPersistenceMgr.h>
#include <mathUtil.h>
#include <LicenseMgmt.h>
#include <LtWrappr.h>
#include <ltann.h>			// LeadTools Annotation functions
#include <TemporaryFileName.h>
#include <PdfSecurityValues.h>
#include <ByteStream.h>
#include <ByteStreamManipulator.h>
#include <EncryptionEngine.h>
#include <ValueRestorer.h>
#include <ComponentLicenseIDs.h>
#include <MiscNuanceUtils.h>

#include <cmath>
#include <cstdio>
#include <algorithm>
#include <string>


// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

// Class created to initialize leadtools license before any application uses this dll
// https://extract.atlassian.net/browse/ISSUE-16441
class InitLicenseClass
{
public:
	InitLicenseClass()
	{
		InitLeadToolsLicense();
	}
};

static InitLicenseClass leadtoolLicense;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// Registry path and key for LeadTools serialization
const string gstrLEADTOOLS_SERIALIZATION_PATH = "\\VendorSpecificUtils\\LeadUtils";
const string gstrSERIALIZATION_KEY = "Serialization"; 
const string gstrDEFAULT_SERIALIZATION = "0"; 

const string gstrSKIP_IMAGE_AREA_CONFIRMATION_KEY = "SkipImageAreaConfirmation";
const string gstrDEFAULT_SKIP_IMAGE_AREA_CONFIRMATION = "0";

// Path to the leadtools compression flag folder
const string gstrLEADTOOLS_COMPRESSION_VALUE_FOLDER =
	"\\VendorSpecificUtils\\LeadUtils\\CompressionFlags";

// Default value for JPEG compression flag (produces reasonably small file while
// maintaining fairly high image quality)
const int giDEFAULT_JPEG_COMPRESSION_FLAG = 80;

const int giDEFAULT_PDF_DISPLAY_DEPTH = 24;
const int giDEFAULT_PDF_RESOLUTION = 300;

// The maximum opacity (ie. completely opaque)
L_INT giMAX_OPACITY = 255;

const COLORREF gCOLOR_BLACK = RGB(0,0,0);
const COLORREF gCOLOR_WHITE = RGB(255,255,255);

// The tolerance for confirmImageAreas that indicates how many pixels away an image area can be
// compared to where it is expected to be and within that tolerance what percent of pixels can be
// something other than the expected value.
// Until a better algorithm is implemented, we need to give black zones much more tolerance for
// error than we do white zones.
const int giZONE_CONFIRMATION_OFFSET_TOLERANCE = 2;
const int giWHITE_ZONE_CONFIRMATION_PERCENT_TOLERANCE = 5;
const int giBLACK_ZONE_CONFIRMATION_PERCENT_TOLERANCE = 25;

//-------------------------------------------------------------------------------------------------
// Predefined Local Functions
//-------------------------------------------------------------------------------------------------
// PROMISE: To return the angle in radians for a PageRasterZone
double rasterAngle( const PageRasterZone &rZone );
//-------------------------------------------------------------------------------------------------
// PROMISE: To draw the text for the PageRasterZone on the device context
void addTextToImage(HDC hDC, const PageRasterZone &rZone, int iVerticalDpi);
//-------------------------------------------------------------------------------------------------
// PROMISE: To return the path to the folder containing LeadUtils.dll with trailing \.
string getLeadUtilsDirectory();
//-------------------------------------------------------------------------------------------------
// PROMISE: Gets a font size in points that fits within the specified zone.
int getFontSizeThatFits(HDC hDC, const PageRasterZone& zone, int iVerticalDpi);
//-------------------------------------------------------------------------------------------------
// PROMISE: Calculates a font size that fits within the specified zone.
// PARAMS:  hDC - The device context on which to select the font
//          zone - Provides the details for the font to use and where the font should fit
//          iVerticalDpi - The vertical dots per inch of the document
//          phFont - Set to the font that will fit. Ignored if NULL.
//          piFontSize - Set to the font size in pixels that fits. Ignored if NULL.
void calculateFontThatFits(HDC hDC, const PageRasterZone& zone, int iVerticalDpi, HFONT* phFont, 
	int* piFontSize);
//-------------------------------------------------------------------------------------------------
// PURPOSE: To validate that each zone has valid dimensions and appears on a valid page
void validateRedactionZones(const vector<PageRasterZone>& vecZones, long nNumberOfPages);
//-------------------------------------------------------------------------------------------------
// PURPOSE: To apply the specified text (if any) to the annotation rectangle
void applyAnnotationText(const PageRasterZone& rZone, HANNOBJECT& hContainer,
						 HDC hDC, int iYResolution, ANNRECT& rect);
//-------------------------------------------------------------------------------------------------
// PURPOSE: To return true if leftZone.m_nPage < rightZone.m_nPage
bool compareZoneByPage(const PageRasterZone& leftZone, const PageRasterZone& rightZone);
//-------------------------------------------------------------------------------------------------
// PROMISE: To convert the specified page zone into an Annotation Rectangle (ANNRECT)
void pageZoneToAnnRect(const PageRasterZone& rZone, ANNRECT& rect);
//-------------------------------------------------------------------------------------------------
// PURPOSE: To encrypt the specified string using the PdfSecurity keys
string encryptString(const string& strString);
//-------------------------------------------------------------------------------------------------
// Callback function for L_AnnEnumerate that will burn redactions into the image.
// For each rect or redact annotation object get the color and if the
// color is either black or white, burn the annotation into the image
L_INT EXT_CALLBACK burnRedactions(HANNOBJECT hObject, L_VOID* pUserData);

//-------------------------------------------------------------------------------------------------
// Exported classes
//-------------------------------------------------------------------------------------------------

//-------------------------------------------------------------------------------------------------
// PDFSecuritySettings
//-------------------------------------------------------------------------------------------------
PDFSecuritySettings::PDFSecuritySettings(const string& strUserPassword,
	const string& strOwnerPassword, long nPermissions, bool bSetPDFLoadOptions) :
	m_bSetLoadOptions(bSetPDFLoadOptions)
{
	try
	{
		setPDFSaveOptions(strUserPassword, strOwnerPassword, nPermissions);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI32200");
}
//-------------------------------------------------------------------------------------------------
PDFSecuritySettings::~PDFSecuritySettings()
{
	try
	{
		LeadToolsLicenseRestrictor leadToolsLicenseGuard;
		if (m_pOriginalOptions.get() != __nullptr)
		{
			try
			{
				throwExceptionIfNotSuccess(L_SetPDFSaveOptions(m_pOriginalOptions.get()),
					"ELI32199", "Unable to set PDF save options back to default.");
			}
			catch(UCLIDException& uex)
			{
				uex.log();
			}
		}
		if (m_pOriginalLoadOptions.get() != __nullptr)
		{
			try
			{
				throwExceptionIfNotSuccess(L_SetPDFOptions(m_pOriginalLoadOptions.get()),
					"ELI32217", "Unable to set PDF load options back to default.");
			}
			catch(UCLIDException& uex)
			{
				uex.log();
			}
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI32198");
}
//-------------------------------------------------------------------------------------------------
void PDFSecuritySettings::setPDFSaveOptions(const string& strUserPassword,
	const string& strOwnerPassword, long nPermissions)
{
	size_t nUserLength = strUserPassword.length();
	size_t nOwnerLength = strOwnerPassword.length();
	if (nUserLength > FILEPDFOPTIONS_MAX_PASSWORD_LEN
		|| nOwnerLength > FILEPDFOPTIONS_MAX_PASSWORD_LEN)
	{
		UCLIDException ue("ELI32221", "Specified password is too long.");
		ue.addDebugInfo("Max Password Length", FILEPDFOPTIONS_MAX_PASSWORD_LEN);
		ue.addDebugInfo("User Password Length", nUserLength);
		ue.addDebugInfo("Owner Password Length", nOwnerLength);
		throw ue;
	}

	if (nUserLength > 0 || nOwnerLength > 0)
	{
		FILEPDFSAVEOPTIONS pdfsfo = GetLeadToolsSizedStruct<FILEPDFSAVEOPTIONS>(0);
		
		LeadToolsLicenseRestrictor leadToolsLicenseGuard;

		throwExceptionIfNotSuccess(
			L_GetPDFSaveOptions(&pdfsfo, sizeof(FILEPDFSAVEOPTIONS)), "ELI32201",
			"Failed to get PDF save options.");

		// Store the original options
		m_pOriginalOptions.reset(new FILEPDFSAVEOPTIONS());
		*m_pOriginalOptions = pdfsfo;

		pdfsfo.b128bit = L_TRUE;
		if (nUserLength > 0)
		{
			errno_t err = strncpy_s((char*)pdfsfo.szUserPassword, FILEPDFOPTIONS_MAX_PASSWORD_LEN,
				strUserPassword.c_str(),  nUserLength);
			if (err != 0)
			{
				UCLIDException ue("ELI32202", "Unable to set user password.");
				ue.addWin32ErrorInfo(err);
				throw ue;
			}

			if (m_bSetLoadOptions)
			{
				FILEPDFOPTIONS pdfOptions = GetLeadToolsSizedStruct<FILEPDFOPTIONS>(0);
				throwExceptionIfNotSuccess(L_GetPDFOptions(&pdfOptions, sizeof(FILEPDFOPTIONS)),
					"ELI32218", "Failed to get PDF options.");
				m_pOriginalLoadOptions.reset(new FILEPDFOPTIONS());
				*m_pOriginalLoadOptions = pdfOptions;
				err = strncpy_s((char*)pdfOptions.szPassword, FILEPDFOPTIONS_MAX_PASSWORD_LEN,
					strUserPassword.c_str(), nUserLength);
				if (err != 0)
				{
					UCLIDException ue("ELI32219", "Unable to set PDF load password.");
					ue.addWin32ErrorInfo(err);
					throw ue;
				}

				throwExceptionIfNotSuccess(L_SetPDFOptions(&pdfOptions),
					"ELI32220", "Failed to set PDF options.");
			}
		}
		if (nOwnerLength > 0)
		{
			errno_t err = strncpy_s((char*)pdfsfo.szOwnerPassword, FILEPDFOPTIONS_MAX_PASSWORD_LEN,
				strOwnerPassword.c_str(),  nOwnerLength);
			if (err != 0)
			{
				UCLIDException ue("ELI32203", "Unable to set owner password.");
				ue.addWin32ErrorInfo(err);
				throw ue;
			}

			// Set the permissions
			pdfsfo.dwEncryptFlags = getLeadtoolsPermissions(nPermissions);
		}

		throwExceptionIfNotSuccess(L_SetPDFSaveOptions(&pdfsfo),
			"ELI32204", "Unable to set PDF save options.");
	}
}
//-------------------------------------------------------------------------------------------------
unsigned int PDFSecuritySettings::getLeadtoolsPermissions(long nPermissions)
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
// Exported DLL Functions
//-------------------------------------------------------------------------------------------------
namespace
{
	static bool bLeadToolsLicenseHasBeenLoaded = false;
}
void InitLeadToolsLicense()
{
	if (bLeadToolsLicenseHasBeenLoaded)
	{
		return;
	}
	try
	{
		LicenseManagement::loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);
		string leadtoolsLicensePath = getModuleDirectory("BaseUtils.dll");
		if (LicenseManagement::isPDFLicensed())
		{
			leadtoolsLicensePath += "\\LEADTOOLS_PDF.OCL";
			throwExceptionIfNotSuccess(
				L_SetLicenseFile((L_CHAR*)leadtoolsLicensePath.c_str(), (L_CHAR*)gstrLEADTOOLS_DEVELOPER_PDF_KEY.c_str()),
				"ELI41718", "Unable to load LeadTools PDF license file",
				leadtoolsLicensePath
			);
		}
		else if (LicenseManagement::isPDFReadLicensed())
		{
			leadtoolsLicensePath += "\\LEADTOOLS_PDF_READ.OCL";
			throwExceptionIfNotSuccess(
				L_SetLicenseFile((L_CHAR*)leadtoolsLicensePath.c_str(), (L_CHAR*)gstrLEADTOOLS_DEVELOPER_PDF_READ_KEY.c_str()),
				"ELI46760", "Unable to load LeadTools PDF Read license file",
				leadtoolsLicensePath
			);
		}
		else
		{
			leadtoolsLicensePath += "\\LEADTOOLS.OCL";
			throwExceptionIfNotSuccess(
				L_SetLicenseFile((L_CHAR*)leadtoolsLicensePath.c_str(), (L_CHAR*)gstrLEADTOOLS_DEVELOPER_KEY.c_str()),
				"ELI46680", "Unable to load LeadTools license file",
				leadtoolsLicensePath
			);
		}

		bLeadToolsLicenseHasBeenLoaded = true;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI41717")

}
//-------------------------------------------------------------------------------------------------
string getErrorCodeDescription(int iErrorCode)
{
	return LBase::GetErrorString(iErrorCode);
}
//-------------------------------------------------------------------------------------------------
void throwExceptionIfNotSuccess(L_INT iErrorCode, const string& strELICode, 
								const string& strErrorDescription,
								const string& strFileName)
{
	if (iErrorCode != SUCCESS)
	{
		// build the exception
		UCLIDException ue(strELICode, strErrorDescription);
		ue.addDebugInfo("Error description", getErrorCodeDescription(iErrorCode));
		ue.addDebugInfo("Error code", iErrorCode);

		// add the image file name if it is available [p13 #4839]
		if (!strFileName.empty())
		{
			ue.addDebugInfo("File name", strFileName);
		}

		// throw the exception
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void fillImageArea(const string& strImageFileName, const string& strOutputImageName, long nLeft, 
	long nTop, long nRight, long nBottom, long nPage, const COLORREF color, 
	bool bRetainAnnotations, bool bApplyAsAnnotations, bool bConfirmApplication,
	const string& strUserPassword, const string& strOwnerPassword, int nPermissions)
{
	fillImageArea(strImageFileName, strOutputImageName, nLeft, nTop, nRight, nBottom, nPage,
		color, 0, "", 0, bRetainAnnotations, bApplyAsAnnotations, bConfirmApplication,
		strUserPassword, strOwnerPassword, nPermissions);
}
//-------------------------------------------------------------------------------------------------
void fillImageArea(const string& strImageFileName, const string& strOutputImageName, long nLeft, 
	long nTop, long nRight, long nBottom, long nPage, const COLORREF crFillColor, 
	const COLORREF crBorderColor, const string& strText, const COLORREF crTextColor, 
	bool bRetainAnnotations, bool bApplyAsAnnotations, bool bConfirmApplication,
	const string& strUserPassword, const string& strOwnerPassword, int nPermissions)
{
	vector<PageRasterZone> vecZones;
	PageRasterZone zone;
	zone.m_nPage = nPage;
	zone.m_nStartX = nLeft;
	zone.m_nEndX = nRight;
	zone.m_nStartY = zone.m_nEndY = (nBottom - nTop) / 2;
	zone.m_nHeight = nBottom - nTop;
	zone.m_crFillColor = crFillColor;
	zone.m_crBorderColor = crBorderColor;
	zone.m_strText = strText;
	zone.m_crTextColor = crTextColor;
	vecZones.push_back(zone);
	fillImageArea(strImageFileName, strOutputImageName, vecZones, bRetainAnnotations, 
		bApplyAsAnnotations, bConfirmApplication, strUserPassword, strOwnerPassword,
		nPermissions);
}
//-------------------------------------------------------------------------------------------------
void fillImageArea(const string& strImageFileName, const string& strOutputImageName, 
				   vector<PageRasterZone>& rvecZones, bool bRetainAnnotations, 
				   bool bApplyAsAnnotations, bool bConfirmApplication,
				   const string& strUserPassword, const string& strOwnerPassword, int nPermissions)
{
	INIT_EXCEPTION_AND_TRACING("MLI02774");
	try
	{
		// Check if an annotation license is required
		if (bRetainAnnotations || bApplyAsAnnotations)
		{
			if (!LicenseManagement::isAnnotationLicensed())
			{
				UCLIDException ue("ELI24863", "Saving redactions as annotations is not licensed.");
				ue.addDebugInfo("Redaction Source", strImageFileName);
				ue.addDebugInfo("Redaction Target", strOutputImageName);
				throw ue;
			}

			// Ensure document support is licensed
			unlockDocumentSupport();
		}
		_lastCodePos = "10";

		// Make sure that if the file being opened/saved is a pdf file that PDF support is licensed
		LicenseManagement::verifyFileTypeLicensedRO(strImageFileName);
		LicenseManagement::verifyFileTypeLicensedRW(strOutputImageName);

		// Sort the vector of zones by page
		sort(rvecZones.begin(), rvecZones.end(), compareZoneByPage);

		// Get the retry counts and timeout value
		int iRetryCount(0), iRetryTimeout(0);
		getFileAccessRetryCountAndTimeout(iRetryCount, iRetryTimeout);
		_lastCodePos = "20";

		// Check if outputting a PDF file
		bool bOutputIsPdf = isPDFFile(strOutputImageName);
		bool bForcedBurnOfAnnotations = false;

		char* pszInputFile = (char*) strImageFileName.c_str();

		FILEINFO fileInfo;
		getFileInformation(strImageFileName, true, fileInfo);

		// Get the number of pages
		long nNumberOfPages = fileInfo.TotalPages;

		// Cache the file format
		int iFormat = fileInfo.Format;

		// If the input format is not Tiff and the output is not PDF
		// and retaining or applying annotations then throw an exception
		// [FlexIDSCore #4115]
		if ((bRetainAnnotations || bApplyAsAnnotations)
			&& !isTiff(iFormat) && !bOutputIsPdf)
		{
			UCLIDException uex("ELI29824", "Cannot apply annotations to a non-tiff image.");
			uex.addDebugInfo("Redaction Source", strImageFileName);
			uex.addDebugInfo("Redaction Target", strOutputImageName);
			uex.addDebugInfo("Image Format", getStringFromFormat(iFormat));
			throw uex;
		}

		// Get initialized LOADFILEOPTION struct.
		// Do not ignore view perspective because this could cause the image to be misinterpreted
		// https://extract.atlassian.net/browse/ISSUE-7220
		LOADFILEOPTION lfo = GetLeadToolsSizedStruct<LOADFILEOPTION>(ELO_ROTATED);

		// Validate each zone
		validateRedactionZones(rvecZones, nNumberOfPages);

		// Create the brush and pen collections
		BrushCollection brushes;
		PenCollection pens;

		// Keep track of the dimensions of each page for use for PDFs in the confirmImageAreas call.
		map<int, pair<int, int>> mapPageResolutions;

		// loop to allow for multiple attempts to fill an image area (P16 #2593)
		bool bSuccessful = false;
		_lastCodePos = "40";

		// [FlexIDSCore:4963]
		// Per discussion with Arvind, use the file access retries for any failure applying the
		// redactions, but be sure not to nest saveImagePage retries inside of these retries
		bool bSavingImagePage = false;

		// If skipImageAreaConfirmation registry value is set, don't honor bConfirmApplication.
		bConfirmApplication &= !skipImageAreaConfirmation();

		// Indicates whether the application of text has been skipped for any redaction in order to
		// validate applied image areas per bConfirmApplication.
		bool bSkippedApplicationOfText = false;

		for (int i=0; i < iRetryCount; i++)
		{
			// Write the output to a temporary file so that the creation of the
			// redacted image appears as an atomic operation [FlexIDSCore #3547]
			TemporaryFileName tempOutFile(true, NULL,
				getExtensionFromFullPath(strOutputImageName).c_str(), true);

			// Flag to indicate if any annotations been applied, if so then
			// L_AnnSetTag will need to be called to reset them
			bool bAnnotationsAppliedToPage = false;

			// Declare objects outside of try scope so that they can be released if an exception
			// is thrown
			HANNOBJECT hContainer = __nullptr; // Annotation container for redactions
			ANNENUMCALLBACK pfnBurnAnnotationsCallBack = __nullptr; // Callback for burnRedactions
			_lastCodePos = "50";
			try
			{
				try
				{
					int nRet = FAILURE;

					// Get initialized SAVEFILEOPTION struct
					SAVEFILEOPTION sfOptions = GetLeadToolsSizedStruct<SAVEFILEOPTION>(0);
					{
						LeadToolsLicenseRestrictor leadToolsLicenseGuard;

						nRet = L_GetDefaultSaveFileOption(&sfOptions, sizeof(sfOptions));
						throwExceptionIfNotSuccess(nRet, "ELI27299", "Unable to get default save options.");
					}
					_lastCodePos = "60";

					// Get the pointer to the first raster zone (we will remember the
					// last zone applied so that the entire collection does not need
					// to be walked for each page
					vector<PageRasterZone>::iterator it = rvecZones.begin();

					// Process the image one page at a time
					_lastCodePos = "70";
					for (long i=1; i <= nNumberOfPages; i++)
					{
						string strPageNumber = asString(i);

						// Set the load option for the current page
						lfo.PageNumber = i;

						// Set FILEINFO_FORMATVALID (this will speed up the L_LoadBitmap calls)
						fileInfo = GetLeadToolsSizedStruct<FILEINFO>(FILEINFO_FORMATVALID);
						fileInfo.Format = iFormat;

						// Get a bitmap handle and wrap it with a bitmap freer
						BITMAPHANDLE hBitmap = {0};
						LeadToolsBitmapFreeer freer(hBitmap);

						// Load the bitmap
						loadImagePage(strImageFileName, hBitmap, fileInfo, lfo, false);
						_lastCodePos = "70_A_Page#" + strPageNumber;

						// L_ColorResBitmap fails for 2bpp images so convert to 4bpp
						if (fileInfo.BitsPerPixel == 2)
						{
							fileInfo.BitsPerPixel = 4;
							LeadToolsLicenseRestrictor leadToolsLicenseGuard;
							nRet = L_ColorResBitmap(&hBitmap, &hBitmap, sizeof(BITMAPHANDLE), fileInfo.BitsPerPixel,
								CRF_EXACTCOLORS, NULL, NULL, 0, NULL, NULL);
							throwExceptionIfNotSuccess(nRet, "ELI53486", "Could not convert image page", strImageFileName);
						}

						mapPageResolutions[i] = pair<int, int>(fileInfo.XResolution, fileInfo.YResolution);
						
						// https://extract.atlassian.net/browse/ISSUE-12096
						// We are no longer outputting PDFs as bitonal by having first converted them
						// to tiff images. But ensure we don't save PDFs in an uncompressed format
						// (FILE_RAS_PDF) because the resulting files are enormous. Change
						// FILE_RAS_PDF to FILE_RAS_PDF_G4 or FILE_RAS_PDF_JPEG depending on whether
						// the image is bitonal.
						if (fileInfo.Format == FILE_RAS_PDF ||
							(bOutputIsPdf && !isPDF(fileInfo.Format)))
						{
							if (fileInfo.BitsPerPixel == 1)
							{
								fileInfo.Format = FILE_RAS_PDF_G4;
							}
							else
							{
								fileInfo.Format = FILE_RAS_PDF_JPEG;

								// https://extract.atlassian.net/browse/ISSUE-15132
								// FILE_RAS_PDF_JPEG is 24-bit JPEG and fails if color depth is 32-bit, e.g.
								// Allow 8 or 24 bit per documentation
								if (fileInfo.BitsPerPixel != 8)
								{
									fileInfo.BitsPerPixel = 24;
								}
							}
						}

						bool bLoadExistingAnnotations = bRetainAnnotations
							&& hasAnnotations(strImageFileName, lfo, iFormat);

						// Create Annotation container sized to image extent if applying as annotations
						// or retaining existing annotations
						if (bApplyAsAnnotations || bLoadExistingAnnotations)
						{
							LeadToolsLicenseRestrictor leadToolsLicenseGuard;

							ANNRECT rect = {0, 0, (L_DOUBLE)fileInfo.Width, (L_DOUBLE)fileInfo.Height};
							nRet = L_AnnCreateContainer(NULL, &rect, FALSE, &hContainer );
							throwExceptionIfNotSuccess(nRet, "ELI14581",
								"Could not create annotation container.");

							// Apply general settings to annotation container
							nRet = L_AnnSetUserMode(hContainer, ANNUSER_DESIGN);
							throwExceptionIfNotSuccess(nRet, "ELI14605",
								"Could not set annotation user mode.");
						}
						_lastCodePos = "70_B_Page#" + strPageNumber;

						// Load the existing annotations if required
						if (bLoadExistingAnnotations)
						{
							HANNOBJECT hFileContainer = NULL; // Annotation container to hold existing annotations

							LeadToolsLicenseRestrictor leadToolsLicenseGuard;
							try
							{
								// Load any existing annotations on this page
								nRet = L_AnnLoad(pszInputFile, &hFileContainer, &lfo);
								throwExceptionIfNotSuccess(nRet, "ELI14630", 
									"Could not load annotations.", strImageFileName);

								// Check for NULL or empty container
								if (hFileContainer != __nullptr)
								{
									HANNOBJECT hFirst;
									nRet = L_AnnGetItem(hFileContainer, &hFirst);
									throwExceptionIfNotSuccess(nRet, "ELI14631", 
										"Could not get item from annotation container.");

									if (hFirst != __nullptr)
									{
										// https://extract.atlassian.net/browse/ISSUE-5411
										// If the retain annotations setting is being used but the
										// output is PDF, existing redaction annotations should be
										// burned into the image.
										if (bOutputIsPdf)
										{
											// Setup the callback function for the annotation enumeration
											pfnBurnAnnotationsCallBack = (ANNENUMCALLBACK)
												MakeProcInstance((FARPROC) burnRedactions, hInst);

											// Burn the redaction annotations into the image
											nRet = L_AnnEnumerate(hFileContainer, pfnBurnAnnotationsCallBack,
												(L_VOID*) &hBitmap, ANNFLAG_RECURSE, NULL);
											throwExceptionIfNotSuccess(nRet, "ELI36838",
												"Could not burn annotations into the image.");

											// Free the callback procedure instance
											FreeProcInstance((FARPROC) burnRedactions);
											pfnBurnAnnotationsCallBack = __nullptr;

											nRet = L_AnnDestroy(hFileContainer, 0);
											throwExceptionIfNotSuccess(nRet, "ELI36839",
												"Unable to destroy annotation container.");

											bForcedBurnOfAnnotations = true;
										}
										else
										{
											// Insert the existing annotations from File Container
											// into the main container. This destroys the File Container.
											nRet = L_AnnInsert(hContainer, hFileContainer, TRUE);
											throwExceptionIfNotSuccess( nRet, "ELI14632", 
												"Could not insert existing annotation objects.");
											bAnnotationsAppliedToPage = true;
										}
									}
									else
									{
										nRet = L_AnnDestroy(hFileContainer, 0);
										throwExceptionIfNotSuccess(nRet, "ELI23570",
											"Unable to destroy annotation container.");
									}

									// The file container was destroyed 
									// either by L_AnnInsert or L_AnnDestroy
									hFileContainer = NULL;
								}
							}
							catch(...)
							{
								if (hFileContainer != __nullptr)
								{
									try
									{
										// Destroy the annotation container
										throwExceptionIfNotSuccess(L_AnnDestroy(hFileContainer, ANNFLAG_RECURSE), 
											"ELI23567",	"Application trace: Unable to destroy annotation container.");
									}
									catch(UCLIDException& ex)
									{
										ex.log();
									}
									hFileContainer = NULL;
								}

								throw;
							}
							// else container is NULL, so nothing to insert
						}
						_lastCodePos = "70_C_Page#" + strPageNumber;

						// Create a new device context manager for this page
						LeadtoolsDCManager ltDC;

						// Check each zone
						for (; it != rvecZones.end(); it++)
						{
							// Get the page from the zone
							long nZonePage = it->m_nPage;

							// Check if this page is greater than the current page
							if (nZonePage > i)
							{
								// If we have passed the current page, just break from the loop
								break;
							}
							// Handle this zone if it is on this page
							else if (nZonePage == i)
							{
								// Create the device context if it has not been created yet
								if (ltDC.m_hDC == NULL)
								{
									ltDC.createFromBitmapHandle(hBitmap);
								}

								// https://extract.atlassian.net/browse/ISSUE-5411
								// If the output is PDF and the apply as annotations setting is
								// being used, force the annotations to be burned instead.
								if (bOutputIsPdf && bApplyAsAnnotations)
								{
									bForcedBurnOfAnnotations = true;
									bApplyAsAnnotations = false;
								}

								_lastCodePos = "70_D_Page#" + strPageNumber;
								if (bApplyAsAnnotations)
								{
									LeadToolsLicenseRestrictor leadToolsLicenseGuard;

									// Create a redaction annotation object
									HANNOBJECT hRedaction;
									nRet = L_AnnCreate(ANNOBJECT_REDACT, &hRedaction);
									throwExceptionIfNotSuccess(nRet, "ELI14582", 
										"Could not create redaction annotation object.");

									// Make redaction object visible
									nRet = L_AnnSetVisible(hRedaction, TRUE, 0, NULL);
									throwExceptionIfNotSuccess(nRet, "ELI15083", 
										"Could not set visibility for redaction annotation object.");

									// Set the redaction color
									nRet = L_AnnSetBackColor(hRedaction, it->m_crFillColor, 0);
									throwExceptionIfNotSuccess(nRet, "ELI14607",
										"Could not set annotation back color.");

									// Set the tiff tag
									nRet = L_AnnSetTag(hRedaction, ANNTAG_TIFF, 0);
									throwExceptionIfNotSuccess(nRet, "ELI14608", 
										"Could not set annotation tag.");

									// Convert the zone to an annotation rectangle
									ANNRECT rect;
									pageZoneToAnnRect((*it), rect);

									nRet = L_AnnSetRect(hRedaction, &rect);
									throwExceptionIfNotSuccess(nRet, "ELI14609", 
										"Could not bound redaction annotation object." );

									// Insert the redaction object into the container
									nRet = L_AnnInsert(hContainer, hRedaction, FALSE);
									throwExceptionIfNotSuccess(nRet, "ELI14610", 
										"Could not insert redaction annotation object.");

									// Apply annotation text unless bConfirmApplication is true.
									// In that case, skip application of the text until after the
									// zone itself has been validated.
									if (!it->m_strText.empty())
									{
										if (bConfirmApplication)
										{
											bSkippedApplicationOfText = true;
										}
										else
										{
											applyAnnotationText((*it), hContainer, ltDC.m_hDC,
												fileInfo.YResolution, rect);
										}
									}

									bAnnotationsAppliedToPage = true;
								}
								else
								{
									// Apply annotation text unless bConfirmApplication is true.
									// In that case, skip application of the text until after the
									// zone itself has been validated.
									bool bApplyText = !it->m_strText.empty();
									if (bApplyText && bConfirmApplication)
									{
										bApplyText = false;
										bSkippedApplicationOfText = true;
									}

									// Draw the redaction
									drawRedactionZone(ltDC.m_hDC, *it,
										fileInfo.YResolution, brushes, pens, bApplyText);
								}
								_lastCodePos = "70_E_Page#" + strPageNumber;
							} // end if this zone is on this page
						} // end for each zone
						_lastCodePos = "70_F_Page#" + strPageNumber;

						// Set the page number for save options
						sfOptions.PageNumber = i;

						if (!bAnnotationsAppliedToPage)
						{
							{
								ValueRestorer<bool>(bSavingImagePage, false);
								bSavingImagePage = true;

								// Save the image page
								saveImagePage(hBitmap, tempOutFile.getName(), fileInfo, sfOptions);
							}
						}
						else
						{
							// Save the collected redaction annotations
							//   Save in WANG-mode for greatest compatibility
							//   The next call to SaveBitmap will include these annotations
							{
								LeadToolsLicenseRestrictor leadToolsLicenseGuard;
								nRet = L_AnnSaveTag(hContainer, ANNFMT_WANGTAG, FALSE);
								throwExceptionIfNotSuccess(nRet, "ELI14611",
									"Could not save redaction annotation objects.");
							}

							{
								ValueRestorer<bool>(bSavingImagePage, false);
								bSavingImagePage = true;

								// Save the image page with the annotations
								saveImagePage(hBitmap, tempOutFile.getName(), fileInfo, sfOptions);
							}

							// Clear any previously defined annotations
							// If not done, any annotations applied to this page may be applied to 
							// successive pages [FlexIDSCore #2216]
							{
								LeadToolsLicenseRestrictor leadToolsLicenseGuard;

								nRet = L_SetTag(ANNTAG_TIFF, 0, 0, NULL);
							}

							// Reset annotations applied to page flag
							bAnnotationsAppliedToPage = false;
						}
						_lastCodePos = "70_H_Page#" + strPageNumber;

						// Destroy the annotation container
						if (hContainer != __nullptr)
						{
							LeadToolsLicenseRestrictor leadToolsLicenseGuard;
							nRet = L_AnnDestroy(hContainer, ANNFLAG_RECURSE);
							throwExceptionIfNotSuccess(nRet, "ELI15361",
								"Could not destroy annotation container.");
							hContainer = NULL;
						}
						_lastCodePos = "70_I_Page#" + strPageNumber;
					} // end for each page
				}
				CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI23568");
			}
			catch(UCLIDException& uex)
			{
				// Need to clear annotation tags if any where applied
				if (bAnnotationsAppliedToPage)
				{
					// Clear any previously defined annotations
					// If not done, any annotations applied to this page may be applied to 
					// successive pages [FlexIDSCore #2216]
					LeadToolsLicenseRestrictor leadToolsLicenseGuard;
					L_SetTag(ANNTAG_TIFF, 0, 0, NULL);
					bAnnotationsAppliedToPage = false;
				}

				// Destroy the annotation containers
				if (hContainer != __nullptr)
				{
					try
					{
						// Destroy the annotation container
						LeadToolsLicenseRestrictor leadToolsLicenseGuard;
						throwExceptionIfNotSuccess(L_AnnDestroy(hContainer, ANNFLAG_RECURSE), 
							"ELI27297",	"Application trace: Unable to destroy annotation container.");
					}
					catch(UCLIDException& ex)
					{
						ex.log();
					}
					hContainer = NULL;
				}

				uex.addDebugInfo("Input Image File", strImageFileName);
				uex.addDebugInfo("Output Image File", strOutputImageName);
				uex.addDebugInfo("Attempt", asString(i+1));

				if (!bSavingImagePage && i < iRetryCount - 1)
				{
					if (i == 0)
					{
						uex.log();
					}

					Sleep(iRetryTimeout);
					continue;
				}

				throw uex;
			}
			_lastCodePos = "80";

			// check the number of pages in the output
			int nNumberOfPagesInOutput = getNumberOfPagesInImage(tempOutFile.getName());

			// if the page numbers don't match log an exception and retry
			if (nNumberOfPages != nNumberOfPagesInOutput)
			{
				UCLIDException ue("ELI23562", "Application Trace: Output page count mismatch.");
				ue.addDebugInfo("Attempt", i+1);
				ue.addDebugInfo("Source Pages", nNumberOfPages);
				ue.addDebugInfo("Source Image", strImageFileName);
				ue.addDebugInfo("Output Pages", nNumberOfPagesInOutput);
				ue.addDebugInfo("Output Image", strOutputImageName);
				ue.addDebugInfo("Temporary Image", tempOutFile.getName());
				ue.log();
			}
			// else page numbers match
			else
			{
				// saved successfully, break from loop
				bSuccessful = true;

				// Since save was successful, copy the temp file to the output file
				// [FlexIDSCore #3547]
				copyFile(tempOutFile.getName(), strOutputImageName);
				break;
			}
		}
		_lastCodePos = "90";

		// failed after retrying, throw a failure exception
		if (!bSuccessful)
		{
			UCLIDException ue("ELI23563", "Failed to properly write the output image.");
			ue.addDebugInfo("Source Image", strImageFileName);
			ue.addDebugInfo("Output Image", strOutputImageName);
			throw ue;
		}
		else
		{
			// [FlexIDSCore:5190]
			// If the filled image areas are to be confirmed, do so before the output is converted
			// to PDF.
			if (bConfirmApplication)
			{
				confirmImageAreas(
					strOutputImageName, mapPageResolutions, rvecZones, bApplyAsAnnotations);

				// If any redaction text was skipped in order to be able to confirm the zones, the
				// fillImageArea call will need to be repeated, this time with bConfirmApplication
				// as false. (The assumption is if we were able to apply redactions correctly the
				// first time, we will be able to do so the second time as well.
				if (bSkippedApplicationOfText)
				{
					fillImageArea(strImageFileName, strOutputImageName, rvecZones, bRetainAnnotations, 
						   bApplyAsAnnotations, false, strUserPassword, strOwnerPassword, nPermissions);

					// Return right away since the output will have already been converted to PDF if
					// necessary.
					return;
				}
			}

			if (bOutputIsPdf)
			{
				if (!strOwnerPassword.empty() || !strUserPassword.empty())
				{
					createSecurePDF(
						strOutputImageName, strUserPassword, strOwnerPassword, nPermissions);
				}
				
				if (bForcedBurnOfAnnotations)
				{
					// Log application trace if annotations added to the document and
					// output is a PDF [FlexIDSCore #3131 - JDS - 12/18/2008] 
					UCLIDException uex("ELI23594",
						"Application trace: Burned annotations into a PDF.");
					uex.addDebugInfo("Input Image File", strImageFileName);
					uex.addDebugInfo("Output Image File", strOutputImageName);
					uex.log();
				}
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25288");
}
//-------------------------------------------------------------------------------------------------
void confirmImageAreas(const string& strImageFileName,
					   const map<int, pair<int, int>>& mapPageResolutions,
					   vector<PageRasterZone>& rvecZones, bool bAppiedAsAnnotations)
{
	try
	{
		// Check if an annotation license is required
		if (bAppiedAsAnnotations)
		{
			if (!LicenseManagement::isAnnotationLicensed())
			{
				UCLIDException ue("ELI35331", "Validation of annotations is not licensed.");
				ue.addDebugInfo("Redaction Source", strImageFileName);
				throw ue;
			}

			// Ensure document support is licensed
			unlockDocumentSupport();
		}

		LicenseManagement::verifyFileTypeLicensedRO(strImageFileName);

		// Sort the vector of zones by page
		// (Assuming this is called from fillImageArea, the zones should already be sorted... but
		// sorting them again causes no harm).
		sort(rvecZones.begin(), rvecZones.end(), compareZoneByPage);

		char* pszInputFile = (char*) strImageFileName.c_str();

		// (getFileInformation contains file access retry logic)
		FILEINFO fileInfo;
		getFileInformation(strImageFileName, true, fileInfo);

		// Get the number of pages
		long nNumberOfPages = fileInfo.TotalPages;

		// Cache the file format
		int iFormat = fileInfo.Format;

		// Get initialized LOADFILEOPTION struct.
		// Do not ignore view perspective because this could cause the image to be misinterpreted
		// https://extract.atlassian.net/browse/ISSUE-7220
		LOADFILEOPTION lfo = GetLeadToolsSizedStruct<LOADFILEOPTION>(ELO_ROTATED);

		// Validate each zone
		validateRedactionZones(rvecZones, nNumberOfPages);

		L_INT nRet = 0;

		// Process the image one page at a time
		for (long i=1; i <= nNumberOfPages; i++)
		{
			string strPageNumber = asString(i);

			// Set the load option for the current page
			lfo.PageNumber = i;

			// https://extract.atlassian.net/browse/ISSUE-12275
			// Ensure the page is loaded in the same DPI as was used when drawing the image areas.
			// NOTE: This applies only for PDF documents.
			CLocalPDFOptions localPDFOptions;
			localPDFOptions.m_pdfRasterizeDocOptions.uXResolution = mapPageResolutions.at(i).first;
			localPDFOptions.m_pdfRasterizeDocOptions.uYResolution = mapPageResolutions.at(i).second;
			localPDFOptions.ApplyPDFOptions("ELI37113", "Failed to apply PDF options.");

			// Set FILEINFO_FORMATVALID (this will speed up the L_LoadBitmap calls)
			fileInfo = GetLeadToolsSizedStruct<FILEINFO>(FILEINFO_FORMATVALID);
			fileInfo.Format = iFormat;

			// Get a bitmap handle and wrap it with a bitmap freer
			BITMAPHANDLE hBitmap = {0};
			LeadToolsBitmapFreeer freer(hBitmap);

			// Load the bitmap (loadImagePage contains file access retry logic)
			loadImagePage(strImageFileName, hBitmap, fileInfo, lfo);

			// [FlexIDSCore:5198]
			// Convert page to a bitonal image; otherwise found pixel color may differ slightly from
			// the expected pixel color (sometimes due to compression, other times I'm not clear
			// why).
			const long nDEFAULT_NUMBER_OF_COLORS = 0;
			L_UINT flags = CRF_FIXEDPALETTE | CRF_NODITHERING;
			{
				LeadToolsLicenseRestrictor leadToolsLicenseGuard;
				nRet = L_ColorResBitmap(&hBitmap, &hBitmap, sizeof(BITMAPHANDLE), 1, flags,
					NULL, NULL, nDEFAULT_NUMBER_OF_COLORS, NULL, NULL);
				throwExceptionIfNotSuccess(nRet, "ELI35342", "Could not load image page.",
					strImageFileName);
			}

			// If checking redactions that have been applied as annotations, check them by burning
			// them into hBitmap, then checking the pixels in hBitmap.
			if (bAppiedAsAnnotations && hasAnnotations(strImageFileName, lfo, iFormat))
			{
				// Annotation container to hold existing annotations
				HANNOBJECT hFileContainer = __nullptr; 
				LeadToolsLicenseRestrictor leadToolsLicenseGuard;
				try
				{
					// Load any existing annotations on this page
					nRet = L_AnnLoad(pszInputFile, &hFileContainer, &lfo);
					throwExceptionIfNotSuccess(nRet, "ELI35332", 
						"Could not load annotations.", strImageFileName);

					ASSERT_RESOURCE_ALLOCATION("ELI35333", hFileContainer != __nullptr);

					// Apply the annotations to hBitmap so the pixels colors can be verified.
					nRet = L_AnnRealize(&hBitmap, NULL, hFileContainer, FALSE);
					throwExceptionIfNotSuccess(nRet, "ELI35334", 
						"Failed to apply validate annotation.", strImageFileName);

					try
					{
						// Destroy the annotation container
						throwExceptionIfNotSuccess(L_AnnDestroy(hFileContainer, ANNFLAG_RECURSE), 
							"ELI35920",	"Application trace: Unable to destroy annotation container.");
					}
					catch(UCLIDException& ex)
					{
						ex.log();
					}
					hFileContainer = __nullptr;
				}
				catch(...)
				{
					if (hFileContainer != __nullptr)
					{
						try
						{
							// Destroy the annotation container
							throwExceptionIfNotSuccess(L_AnnDestroy(hFileContainer, ANNFLAG_RECURSE), 
								"ELI35335",	"Application trace: Unable to destroy annotation container.");
						}
						catch(UCLIDException& ex)
						{
							ex.log();
						}
						hFileContainer = __nullptr;
					}

					throw;
				}
			}

			// Create a new device context manager for this page
			LeadtoolsDCManager ltDC;

			// Check each zone on the page.
			for (vector<PageRasterZone>::iterator it = rvecZones.begin(); it != rvecZones.end(); it++)
			{
				// Get the page from the zone
				long nZonePage = it->m_nPage;

				// Check if this page is greater than the current page
				if (nZonePage > i)
				{
					// If we have passed the current page, just break from the loop
					break;
				}
				else if (nZonePage == i)
				{
					// Create a bitmap to store the contents of the image zone to check.
					BITMAPHANDLE hBitmapImageZone = {0};
					LeadToolsBitmapFreeer zoneFreer(hBitmapImageZone, true);

					// Extract the zone into hBitmapImageZone.
					extractZoneAsBitmap(&hBitmap, it->m_nStartX, it->m_nStartY, it->m_nEndX,
						it->m_nEndY, it->m_nHeight, &hBitmapImageZone);

					// 2/12/2012 SNK
					// On skewed redaction zones, extractZoneAsBitmap will end up leaving padding
					// around the outside of the zone. I was unable to directly solve the issue in
					// time for the 9.1 release. However, in almost all cases extractZoneAsBitmap
					// does have the redaction oriented correctly and centered in the image area;
					// therefore, if when checking the pixels of this zone, we exclude the extra area
					// around the edge that wasn't present in the original zone, we should avoid
					// testing any area that is not actually part of the redaction.
					int nExcessWidth = hBitmapImageZone.Width - 
									   (int)sqrt(pow((double)(it->m_nEndX - it->m_nStartX), 2) +
												 pow((double)(it->m_nEndY - it->m_nStartY), 2));
					int nExcessHeight = hBitmapImageZone.Height - it->m_nHeight;

					int nXPadding = giZONE_CONFIRMATION_OFFSET_TOLERANCE + ((nExcessWidth + 1) / 2);
					int nYPadding = giZONE_CONFIRMATION_OFFSET_TOLERANCE + ((nExcessHeight + 1) / 2);

					// Until a better algorithm is implemented, we need to give black give black
					// zones much more tolerance for error than we do white zones.
					int nPercentTolerance = (it->m_crFillColor == RGB(0,0,0))
						? giBLACK_ZONE_CONFIRMATION_PERCENT_TOLERANCE
						: giWHITE_ZONE_CONFIRMATION_PERCENT_TOLERANCE;

					// [FlexIDSCore:5198]
					// Allow for a certain number of pixels to not match the expected value without
					// determining that the redaction failed to be applied.
					int nPixelErrorCount = 0;
					int nPixelErrorThreshold =
						(hBitmapImageZone.Height - 2 * giZONE_CONFIRMATION_OFFSET_TOLERANCE)
						* ( hBitmapImageZone.Width - 2 * giZONE_CONFIRMATION_OFFSET_TOLERANCE)
						* nPercentTolerance / 100;

					// Loop though each row of the extracted zone to confirm the image area has been
					// applied.
					LeadToolsLicenseRestrictor leadToolsLicenseGuard;
					for (int nRow = nYPadding; nRow < hBitmapImageZone.Height - nYPadding; nRow++)
					{
						// Loop through each pixel in the row.
						for (int nCol = nXPadding; nCol < hBitmapImageZone.Width - nXPadding; nCol++)
						{
							// Get the color of the current pixel.
							COLORREF crPixel =  L_GetPixelColor(&hBitmapImageZone, nRow, nCol);

							if (crPixel == it->m_crFillColor)
							{
								// If the pixel is the fill color, this pixel is okay.
								continue;
							}
							else if (crPixel == it->m_crBorderColor &&
								(nRow == giZONE_CONFIRMATION_OFFSET_TOLERANCE ||
								 nCol == giZONE_CONFIRMATION_OFFSET_TOLERANCE ||
								 nRow == hBitmapImageZone.Height - giZONE_CONFIRMATION_OFFSET_TOLERANCE - 1 ||
								 nCol == hBitmapImageZone.Width - giZONE_CONFIRMATION_OFFSET_TOLERANCE - 1))
							{
								// If the pixel is the border color and the pixel is within
								// giZONE_CONFIRMATION_OFFSET_TOLERANCE of the edge of the zone, it is okay.
								continue;
							}

							nPixelErrorCount++;
							if (nPixelErrorCount > nPixelErrorThreshold)
							{
								// If we got here, the current pixel does not appear to reflect a
								// properly applied image area.
								UCLIDException ue("ELI35336", "Redaction validation failed.");
								ue.addDebugInfo("Page", it->m_nPage);
								ue.addDebugInfo("StartX", it->m_nStartX);
								ue.addDebugInfo("StartY", it->m_nStartY);
								ue.addDebugInfo("EndX", it->m_nEndX);
								ue.addDebugInfo("EndY", it->m_nEndY);
								ue.addDebugInfo("Height", it->m_nHeight);
								ue.addDebugInfo("Row", nRow);
								throw ue;
							}
						}
					}
				}
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI35337");
}
//-------------------------------------------------------------------------------------------------
void createMultiPageImage(vector<string> vecImageFiles, string strOutputFileName,
	bool bOverwriteExistingFile)
{
	// Check for file existence if overwrite is false
	if (!bOverwriteExistingFile)
	{
		// if file exists, set flag to not write the file
		if (isFileOrFolderValid(strOutputFileName))
		{
			UCLIDException ue("ELI12853", "File already exists.");
			ue.addDebugInfo("ImageName", strOutputFileName);
			throw ue;
		}
	}

	// Handle 0 images (error condition)
	int nNumImages = vecImageFiles.size();
	if (nNumImages == 0)
	{
		UCLIDException ue("ELI12855", "Vector containing sub-image filenames is empty.");
		ue.addDebugInfo("NumOfImages", nNumImages);
		ue.addDebugInfo("OutputImage", strOutputFileName);
		throw ue;
	}

	// Create a temporary file for the output
	TemporaryFileName tmpOutput(true, "", NULL, getExtensionFromFullPath(strOutputFileName).c_str(),
		true);
	const string& strTempOut = tmpOutput.getName();
	char* pszOutput = (char*)strTempOut.c_str();

	// Get file info
	FILEINFO fileInfo;
	getFileInformation(vecImageFiles[0], false, fileInfo);

	// Get the appropriate compression factor for the specified format [LRCAU #5284]
	L_INT nCompression = getCompressionFactor(fileInfo.Format);

	// Get initialized SAVEFILEOPTION struct
	SAVEFILEOPTION sfOptions = GetLeadToolsSizedStruct<SAVEFILEOPTION>(0);
	{
		LeadToolsLicenseRestrictor leadToolsLicenseGuard;
		L_GetDefaultSaveFileOption(&sfOptions, sizeof(sfOptions));
	}

	// for each page that exists for this image, if an image file
	// exists with the corresponding name, then load it and add it
	// to the multi-page image

	// Loop through each image page
	for (int i = 0; i < nNumImages; i++ )
	{
		// Retrieve this filename and update page number
		const string& strPage = vecImageFiles[i];

		// if the image page file exists, load it and add it to the
		// bitmap list.
		if (isValidFile(strPage))
		{
			// Temporary holder for a bitmap
			BITMAPHANDLE hTmpBmp = {0};
			LeadToolsBitmapFreeer freer(hTmpBmp);

			// Set flags to get file information when loading bitmap
			loadImagePage(strPage, 1, hTmpBmp, false);

			// Save the page to the multi-page image using the format of the first page of the image
			sfOptions.PageNumber = i + 1;

			LeadToolsLicenseRestrictor leadToolsLicenseGuard;
			L_INT nRet = L_SaveBitmap(pszOutput, &hTmpBmp, fileInfo.Format, 
				fileInfo.BitsPerPixel, nCompression, &sfOptions);
			throwExceptionIfNotSuccess(nRet, "ELI09045",
				"Unable to insert page in image.", strPage);
		}
		else
		{
			UCLIDException ue("ELI12851", "Unable to locate page image.");
			ue.addDebugInfo("Filename", strPage);
			ue.addDebugInfo("PageNumber", i + 1);
			throw ue; 
		}
	}

	// Ensure the image has the correct number of pages
	int nNumberWritten = getNumberOfPagesInImage(strTempOut);
	if (nNumImages != nNumberWritten)
	{
		UCLIDException ue("ELI30100", "Page count mismatch.");
		ue.addDebugInfo("Expected Number Of Pages", nNumImages);
		ue.addDebugInfo("Number Of Pages Written", nNumberWritten);
		ue.addDebugInfo("Output File Name", strOutputFileName);
		throw ue;
	}

	// Move the temporary file to its final destination
	copyFile(strTempOut, strOutputFileName, bOverwriteExistingFile);
}
//-------------------------------------------------------------------------------------------------
void getFileInformation(const string& strImageFileName, bool bIncludePageCount, FILEINFO& rFileInfo,
	LOADFILEOPTION* pLFO)
{
	int nNumFailedAttempts = 0;
	try
	{
		validateFileOrFolderExistence(strImageFileName, "ELI32190");

		// Get initialized FILEINFO struct
		rFileInfo = GetLeadToolsSizedStruct<FILEINFO>(0);

		int nRet = FAILURE;
		L_UINT flags = bIncludePageCount ? FILEINFO_TOTALPAGES : 0;
		char* pszFileName = (char*)strImageFileName.c_str();
		while (nNumFailedAttempts < gnNUMBER_ATTEMPTS_BEFORE_FAIL)
		{
			DWORD dwStartIndex = 0;
			if (getExtensionFromFullPath(strImageFileName, true) == ".pdf")
			{
				dwStartIndex = getPDFStartIndex(strImageFileName);
			}

			if (dwStartIndex == 0)
			{
				LeadToolsLicenseRestrictor leadToolsLicenseGuard;
				nRet = L_FileInfo(pszFileName, &rFileInfo, sizeof(FILEINFO), flags, pLFO);
			}
			else
			{
				L_VOID *hInfo;
				L_UCHAR cBuf[1024];
				L_INT nRead = sizeof(cBuf);
				DWORD dwNumOfBytesRead = 0;

				CHandle hFile(CreateFile(pszFileName, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, 0, NULL));
				SetFilePointer(hFile, dwStartIndex, __nullptr, FILE_BEGIN);

				LeadToolsLicenseRestrictor leadToolsLicenseGuard;
				nRet = L_StartFeedInfo(&hInfo, &rFileInfo, sizeof(FILEINFO), flags, pLFO);
				if (nRet == SUCCESS)
				{
					while (true)
					{
						if (!ReadFile(hFile, cBuf, nRead, &dwNumOfBytesRead, __nullptr))
						{
							nRet = ERROR_FILE_READ;
							break;
						}
						if (dwNumOfBytesRead == 0)
						{
							break;
						}
						nRet = L_FeedInfo(hInfo, cBuf, dwNumOfBytesRead);
						if (nRet != SUCCESS) // SUCCESS_ABORT, enough info provided, or an error
						{
							break;
						}
					}
					nRet = L_StopFeedInfo(hInfo);
				}
			}
								
			// Check result
			if (nRet == SUCCESS)
			{
				if (nNumFailedAttempts != 0)
				{
					UCLIDException ue("ELI32191",
						"Application Trace: Successfully gathered file information.");
					ue.addDebugInfo("File name", strImageFileName);
					ue.addDebugInfo("Retries attempted", nNumFailedAttempts);
					ue.log();
				}

				// Exit loop
				break;
			}
			else
			{
				// Increment counter
				nNumFailedAttempts++;

				// Sleep before retrying the FileInfo call
				Sleep(gnSLEEP_BETWEEN_RETRY_MS);
			}
		}

		// Throw exception if all retries failed
		throwExceptionIfNotSuccess(nRet, "ELI32192", "Could not obtain image info.",
			strImageFileName);
	}
	catch (UCLIDException& ue)
	{
		ue.addDebugInfo("Retries attempted", nNumFailedAttempts);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
int getNumberOfPagesInImage(const char* szImageFileName)
{
	string strImageFileName = szImageFileName;
	return getNumberOfPagesInImage(strImageFileName);
}
//-------------------------------------------------------------------------------------------------
int getNumberOfPagesInImage(const string& strImageFileName)
{
	try
	{
		int pageCount = 0;

		EFileType fileTypeFromExtension = getFileType(strImageFileName);
		if (fileTypeFromExtension == kRichTextFile || fileTypeFromExtension == kTXTFile)
		{
			return 1;
		}

		try
		{
			pageCount = getNumberOfPagesInImageNuance(strImageFileName);
		}
		catch (UCLIDException e)
		{
			e.log();
			pageCount = 0;
		}

		// There are some instances where Nuance is unable to get the page count
		if (pageCount == 0)
		{
			// Get initialized FILEINFO struct
			FILEINFO fileInfo;
			getFileInformation(strImageFileName, true, fileInfo);

			// Return actual page count
			return fileInfo.TotalPages;
		}
		// Return actual page count
		return pageCount;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI15314");
}
//-------------------------------------------------------------------------------------------------
void getImageXAndYResolution(const string& strImageFileName, int& riXResolution, 
							 int& riYResolution, int nPageNum/* = 1*/)
{
	try
	{
		// Get initialized LOADFILEOPTION struct. 
		LOADFILEOPTION lfo = GetLeadToolsSizedStruct<LOADFILEOPTION>(0);
		lfo.PageNumber = nPageNum;

		// Get initialized FILEINFO struct
		FILEINFO fileInfo;
		getFileInformation(strImageFileName, false, fileInfo, &lfo);

		riXResolution = fileInfo.XResolution;
		riYResolution = fileInfo.YResolution;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI32193");
}
//-------------------------------------------------------------------------------------------------
void getImagePixelHeightAndWidth(const string& strImageFileName, int& riHeight, int& riWidth,
								 int nPageNum)
{
	try
	{
		// Get initialized LOADFILEOPTION struct. 
		LOADFILEOPTION lfo = GetLeadToolsSizedStruct<LOADFILEOPTION>(0);
		lfo.PageNumber = nPageNum;

		FILEINFO fileInfo;
		getFileInformation(strImageFileName, false, fileInfo, &lfo);

		riHeight = fileInfo.Height;
		riWidth = fileInfo.Width;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI32194");
}
//-------------------------------------------------------------------------------------------------
void initPDFSupport()
{
	int nDisplayDepth = giDEFAULT_PDF_DISPLAY_DEPTH;
	int iOpenXRes(giDEFAULT_PDF_RESOLUTION), iOpenYRes(giDEFAULT_PDF_RESOLUTION);

	// check if PDF is licensed to initialize support
	bool isReadWriteLicensed = LicenseManagement::isPDFLicensed();
	bool isReadLicensed = isReadWriteLicensed || LicenseManagement::isPDFReadLicensed();

	if (!isReadLicensed)
	{
		// pdf support is not licensed
		return;
	}
	else
	{
		bool bCouldNotUnlockRead = false;
		bool bCouldNotUnlockWrite = false;

		// Only unlock read support if not already unlocked
		if (L_IsSupportLocked(L_SUPPORT_RASTER_PDF_READ) == L_TRUE)
		{
			// Unlock support for PDF Reading
			InitLeadToolsLicense();
			// check if pdf support was unlocked
			if (L_IsSupportLocked(L_SUPPORT_RASTER_PDF_READ) == L_TRUE)
			{
				// log an exception
				UCLIDException ue("ELI19815", "Unable to unlock PDF read support.");
				ue.addDebugInfo("PDF Read Key", L_KEY_PDF_READ, true);
				ue.log();

				// set the could not unlock flag
				bCouldNotUnlockRead = true;
			}
		}

		// only unlock write support if not already unlocked
		if (isReadWriteLicensed && L_IsSupportLocked(L_SUPPORT_RASTER_PDF_SAVE) == L_TRUE)
		{
			// unlock support for PDF writing
			InitLeadToolsLicense();

			// check if pdf support was unlocked
			if (L_IsSupportLocked(L_SUPPORT_RASTER_PDF_SAVE) == L_TRUE)
			{
				// log an exception
				UCLIDException ue("ELI19863", "Unable to unlock PDF save support.");
				ue.addDebugInfo("PDF Save Key", L_KEY_PDF_SAVE, true);
				ue.log();

				// set the could not unlock flag
				bCouldNotUnlockWrite = true;
			}
		}

		// if pdf support was not unlocked, stop now.
		if (bCouldNotUnlockRead && bCouldNotUnlockWrite)
		{
			return;
		}
	}

	// Get initialized FILEPDFOPTIONS struct
	FILEPDFOPTIONS pdfOptions = GetLeadToolsSizedStruct<FILEPDFOPTIONS>(0);

	LeadToolsLicenseRestrictor leadToolsLicenseGuard;

	// Individual scope for L_GetPDFOptions() and L_SetPDFOptions()
	{
		// Retrieve default load options
		L_GetPDFOptions(&pdfOptions, sizeof(pdfOptions));

		// Only set options if not already the correct options
		if (pdfOptions.nDisplayDepth != nDisplayDepth)
		{
			// Define desired display depth settings
			pdfOptions.nDisplayDepth = nDisplayDepth;

			// Apply settings
			L_SetPDFOptions(&pdfOptions);
		}
	}

	RASTERIZEDOCOPTIONS rasterizeDocOptions = GetLeadToolsSizedStruct<RASTERIZEDOCOPTIONS>(0);
	{
		L_GetRasterizeDocOptions(&rasterizeDocOptions, rasterizeDocOptions.uStructSize);
		if (rasterizeDocOptions.uXResolution != iOpenXRes ||
			rasterizeDocOptions.uYResolution != iOpenYRes)
		{
			// Define desired resolution 
			rasterizeDocOptions.uXResolution = iOpenXRes;
			rasterizeDocOptions.uYResolution = iOpenYRes;

			// Apply Settings
			L_SetRasterizeDocOptions(&rasterizeDocOptions);
		}
	}
}
//-------------------------------------------------------------------------------------------------
int getImageViewPerspective(const string& strImageFileName, int nPageNum)
{
	try
	{
		// Treat PDF images as having TOP_LEFT perspective
		if (isPDFFile(strImageFileName))
		{
			return TOP_LEFT;
		}

		// Get initialized LOADFILEOPTION struct and set the page number
		LOADFILEOPTION lfo = GetLeadToolsSizedStruct<LOADFILEOPTION>(0);
		lfo.PageNumber = nPageNum;

		// Get the file info
		FILEINFO fileInfo;
		getFileInformation(strImageFileName, false, fileInfo, &lfo);

		// Return ViewPerspective field
		return fileInfo.ViewPerspective;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI32208");
}
//-------------------------------------------------------------------------------------------------
void unlockDocumentSupport()
{
	// Unlock support for Document toolkit for annotations
	if (LicenseManagement::isAnnotationLicensed())
	{
		// Unlock Document/Medical support only if 
		// Annotation package is licensed (P13 #4499)
		InitLeadToolsLicense();

		// check if document support was unlocked
		if(L_IsSupportLocked(L_SUPPORT_DOCUMENT) == L_TRUE)
		{
			UCLIDException ue("ELI19816", "Unable to unlock document toolkit support.");
			ue.addDebugInfo("Document Key", L_KEY_DOCUMENT, true);
			throw ue;
		}
	}
	else
	{
		UCLIDException ue( "ELI16799", "Document toolkit support is not licensed." );
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
bool isLeadToolsSerialized()
{
	// Setup Registry persistence item
	RegistryPersistenceMgr rpm( HKEY_LOCAL_MACHINE, gstrRC_REG_PATH );

	// Check for registry key
	if (!rpm.keyExists( gstrLEADTOOLS_SERIALIZATION_PATH, gstrSERIALIZATION_KEY ))
	{
		// Create key if not found, default to false
		rpm.createKey( gstrLEADTOOLS_SERIALIZATION_PATH, gstrSERIALIZATION_KEY,
			gstrDEFAULT_SERIALIZATION );
		return asCppBool(gstrDEFAULT_SERIALIZATION);
	}

	return asCppBool(rpm.getKeyValue( gstrLEADTOOLS_SERIALIZATION_PATH, gstrSERIALIZATION_KEY,
		gstrDEFAULT_SERIALIZATION)); 
}
//-------------------------------------------------------------------------------------------------
bool skipImageAreaConfirmation()
{
	// Avoid repeated hits to the registry.
	static bool bInitialized = false;
	static bool bSkipImageAreaConfirmation = false;

	if (!bInitialized)
	{
		// Setup Registry persistence item
		RegistryPersistenceMgr rpm(HKEY_LOCAL_MACHINE, gstrRC_REG_PATH);

		// Check for registry key
		if (!rpm.keyExists(gstrLEADTOOLS_SERIALIZATION_PATH, gstrSKIP_IMAGE_AREA_CONFIRMATION_KEY))
		{
			// Create key if not found, default to false
			rpm.createKey(gstrLEADTOOLS_SERIALIZATION_PATH, gstrSKIP_IMAGE_AREA_CONFIRMATION_KEY,
				gstrDEFAULT_SKIP_IMAGE_AREA_CONFIRMATION);
			bSkipImageAreaConfirmation = asCppBool(gstrDEFAULT_SKIP_IMAGE_AREA_CONFIRMATION);
		}

		bSkipImageAreaConfirmation = asCppBool(rpm.getKeyValue(gstrLEADTOOLS_SERIALIZATION_PATH,
			gstrSKIP_IMAGE_AREA_CONFIRMATION_KEY, gstrDEFAULT_SKIP_IMAGE_AREA_CONFIRMATION));

		bInitialized = true;
	}

	return bSkipImageAreaConfirmation;
}
//-------------------------------------------------------------------------------------------------
void convertTIFToPDF(const string& strTIF, const string& strPDF, bool bRetainAnnotations,
					 const string& strUserPassword, const string& strOwnerPassword,
					 int nPermissions)
{
	try
	{
		try
		{
			// Build path to ImageFormatConverter application
			string strEXEPath = getLeadUtilsDirectory();
			strEXEPath += gstrCONVERTER_EXE_NAME.c_str();

			// Provide image paths and output type
			string strArguments = "\"";
			strArguments += strTIF.c_str();
			strArguments += "\" \"";
			strArguments += strPDF.c_str();
			strArguments += "\" ";
			strArguments += gstrCONVERT_TO_PDF_OPTION;
			if (bRetainAnnotations)
			{
				strArguments += " ";
				strArguments += gstrCONVERT_RETAIN_ANNOTATIONS;
			}

			bool bSecurityAdded = false;
			if (!strUserPassword.empty())
			{
				strArguments += " /user \"";
				strArguments += encryptString(strUserPassword);
				strArguments += "\"";
				bSecurityAdded = true;
			}
			if (!strOwnerPassword.empty())
			{
				strArguments += " /owner \"";
				strArguments += encryptString(strOwnerPassword);
				strArguments += "\" ";
				strArguments += asString(nPermissions);
				bSecurityAdded = true;
			}
			if (bSecurityAdded)
			{
				strArguments += " /enc";
			}

			// Run the EXE with arguments and appropriate wait time (P13 #4415)
			// Use infinite wait time (P13 #4634)
			runExtractEXE( strEXEPath, strArguments, INFINITE );
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25223");
	}
	catch(UCLIDException& ue)
	{
		ue.addDebugInfo("Tif To Convert", strTIF);
		ue.addDebugInfo("PDF Destination", strPDF);
		ue.addDebugInfo("Retain Annotations", bRetainAnnotations ? "True" : "False");
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void convertPDFToTIF(const string& strPDF, const string& strTIF)
{
	try
	{
		try
		{
			// Build path to ImageFormatConverter application
			string strEXEPath = getLeadUtilsDirectory();
			strEXEPath += gstrCONVERTER_EXE_NAME.c_str();

			// Provide image paths and output type
			string strArguments = "\"";
			strArguments += strPDF.c_str();
			strArguments += "\" \"";
			strArguments += strTIF.c_str();
			strArguments += "\" ";
			strArguments += gstrCONVERT_TO_TIF_OPTION.c_str();

			// Run the EXE with arguments and appropriate wait time (P13 #4415)
			// Use infinite wait time (P13 #4634)
			runExeWithProcessKiller(strEXEPath, true, strArguments);
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25221")
	}
	catch(UCLIDException& ue)
	{
		ue.addDebugInfo("PDF To Convert", strPDF);
		ue.addDebugInfo("Tif Destination", strTIF);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void createSecurePDF(const string& strPDF, const string& strUserPassword,
					 const string& strOwnerPassword, int nPermissions)
{
	try
	{
		try
		{
			// Build path to ImageFormatConverter application
			string strEXEPath = getLeadUtilsDirectory();
			strEXEPath += gstrCONVERTER_EXE_NAME.c_str();

			// ImageFormatConverter doesn't accept input/output documents being the same... so start
			// by moving the source document to a temporary file name.
			TemporaryFileName strTempOutput(true, __nullptr, ".pdf");
			moveFile(strPDF, strTempOutput.getName(), true);

			// Provide image paths and output type
			string strArguments = "\"";
			strArguments += strTempOutput.getName();
			strArguments += "\" \"";
			strArguments += strPDF.c_str();
			strArguments += "\" ";
			strArguments += gstrCONVERT_TO_PDF_OPTION;

			bool bSecurityAdded = false;
			if (!strUserPassword.empty())
			{
				strArguments += " /user \"";
				strArguments += encryptString(strUserPassword);
				strArguments += "\"";
				bSecurityAdded = true;
			}
			if (!strOwnerPassword.empty())
			{
				strArguments += " /owner \"";
				strArguments += encryptString(strOwnerPassword);
				strArguments += "\" ";
				strArguments += asString(nPermissions);
				bSecurityAdded = true;
			}
			if (bSecurityAdded)
			{
				strArguments += " /enc";
			}

			// Run the EXE with arguments and appropriate wait time (P13 #4415)
			// Use infinite wait time (P13 #4634)
			runExtractEXE( strEXEPath, strArguments, INFINITE );
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI37114");
	}
	catch(UCLIDException& ue)
	{
		ue.addDebugInfo("PDF Destination", strPDF);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void pageZoneToPoints( const PageRasterZone &rZone, POINT &p1, POINT &p2, POINT &p3, POINT &p4)
{
	// calculate the 4 corner points of the raster zone;
	// p1 = point above start point
	// p2 = point above end point
	// p3 = point below end point
	// p4 = point below start point
	
	// calculate the angle of the line dy/dx
	double dDiffY = rZone.m_nEndY - rZone.m_nStartY;
	double dDiffX = rZone.m_nEndX  - rZone.m_nStartX;
	double dAngle = atan2(dDiffY, dDiffX);
	
	double dDeltaX = rZone.m_nHeight / 2.0 * sin(dAngle);
	double dDeltaY = rZone.m_nHeight / 2.0 * cos(dAngle);
	
	// calculate the 4 points
	p1.x = (long)(rZone.m_nStartX - dDeltaX);
	p1.y = (long)(rZone.m_nStartY + dDeltaY);
	
	p2.x = (long)(rZone.m_nEndX - dDeltaX);
	p2.y = (long)(rZone.m_nEndY + dDeltaY);
	
	p3.x = (long)(rZone.m_nEndX + dDeltaX);
	p3.y = (long)(rZone.m_nEndY - dDeltaY);
	
	p4.x = (long)(rZone.m_nStartX + dDeltaX);
	p4.y = (long)(rZone.m_nStartY - dDeltaY);
}
//-------------------------------------------------------------------------------------------------
bool isTiff(int iFormat)
{
	switch(iFormat)
	{
	case FILE_CCITT:
	case FILE_CCITT_GROUP3_1DIM:
	case FILE_CCITT_GROUP3_2DIM:
	case FILE_CCITT_GROUP4:
	case FILE_JTIF:
	case FILE_LEAD2JTIF:
	case FILE_LEAD1JTIF:
	case FILE_TIF:
	case FILE_TIF_CMP:
	case FILE_TIF_CMYK:
	case FILE_TIF_JBIG:
	case FILE_TIF_PACKBITS:
	case FILE_TIF_PACKBITS_CMYK:
	case FILE_TIF_PACKBITS_YCC:
	case FILE_TIF_YCC:
	case FILE_TIFLZW:
	case FILE_TIFLZW_CMYK:
	case FILE_TIFLZW_YCC:
		return true;

	default:
		return false;
	}
}
//-------------------------------------------------------------------------------------------------
bool isTiff(const string& strImageFile)
{
	try
	{
		// Get the file info for the image file
		FILEINFO fileInfo;
		getFileInformation(strImageFile, false, fileInfo);

		return isTiff(fileInfo.Format);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI32214");
}
//-------------------------------------------------------------------------------------------------
bool isPDF(int iFormat)
{
	switch(iFormat)
	{
	case FILE_RAS_PDF:
	case FILE_RAS_PDF_G3_1D:
	case FILE_RAS_PDF_G3_2D:
	case FILE_RAS_PDF_G4:
	case FILE_RAS_PDF_JPEG:
	case FILE_RAS_PDF_JPEG_422:
	case FILE_RAS_PDF_JPEG_411:
	case FILE_RAS_PDF_LZW:
	case FILE_RAS_PDF_JBIG2:
	case FILE_PDF_LEAD_MRC:
	case FILE_RAS_PDF_CMYK:
	case FILE_RAS_PDF_LZW_CMYK:
		return true;

	default:
		return false;
	}
}
//-------------------------------------------------------------------------------------------------
bool isPDF(const string& strImageFile)
{
	try
	{
		// Get the file info for the image file
		FILEINFO fileInfo;
		getFileInformation(strImageFile, false, fileInfo);

		return isPDF(fileInfo.Format);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI32215");
}
//-------------------------------------------------------------------------------------------------
bool hasAnnotations(const string& strFilename, LOADFILEOPTION &lfo, int iFileFormat)
{
	// if this is not a tiff file it does not contain annotations.
	if(!isTiff(iFileFormat))
	{
		return false;
	}

	// attempt to read annotations from the tiff tag
	L_UINT16 uType = 0;
	L_UINT uCount = 0;

	LeadToolsLicenseRestrictor leadToolsLicenseGuard;

	int iRet = L_ReadFileTag((char*)strFilename.c_str(), ANNTAG_TIFF, &uType, &uCount, NULL, &lfo);

	// if there is no annotation tag, this file does not contain annotations
	if(iRet == ERROR_TAG_MISSING)
	{
		return false;
	}

	// if some other error was found, throw an exception
	if(iRet <= 0)
	{
		throwExceptionIfNotSuccess(iRet, "ELI20788", "Could not load annotations from tiff tag.",
			strFilename);
	}

	// return true if there is at least one annotation object
	return uCount > 0;
}
//-------------------------------------------------------------------------------------------------
bool hasAnnotations(const string& strFilename, int iPageNumber)
{
	// check if this is a pdf file
	if( isPDFFile(strFilename) )
	{
		return false;
	}

	LeadToolsLicenseRestrictor leadToolsLicenseGuard;

	// create the load file options
	// Do not ignore view perspective because this could cause the image to be misinterpreted
	// https://extract.atlassian.net/browse/ISSUE-7220
	LOADFILEOPTION lfo = GetLeadToolsSizedStruct<LOADFILEOPTION>(ELO_ROTATED);
	lfo.PageNumber = iPageNumber;

	// get file info
	FILEINFO fileInfo = GetLeadToolsSizedStruct<FILEINFO>(0);
	int iRet = L_FileInfo((char*) strFilename.c_str(), &fileInfo, sizeof(FILEINFO), 0, &lfo);
	throwExceptionIfNotSuccess(iRet, "ELI20804", "Could not obtain image info.", strFilename);

	// check whether the image contains annotations
	return hasAnnotations(strFilename, lfo, fileInfo.Format);
}

//-------------------------------------------------------------------------------------------------
// Local Functions
//-------------------------------------------------------------------------------------------------
double rasterAngle( const PageRasterZone &rZone )
{
	// Get Angle in Radians
	double dDiffY = rZone.m_nEndY - rZone.m_nStartY;
	double dDiffX = rZone.m_nEndX - rZone.m_nStartX;
	double dAngle = atan2(dDiffY, dDiffX);

	// Express as an angle from horizontal between -PI/2 and PI/2.
	// Prevents upside down text. [FlexIDSCore #3433]
	if (dAngle >= MathVars::PI / 2)
	{
		dAngle -= MathVars::PI;
	}
	else if (dAngle < -MathVars::PI / 2)
	{
		dAngle += MathVars::PI;
	}

	// Ensure the angle is expressed relative to the longest length of the zone.
	// Prevents text from being written sideways inside a redaction. [FlexIDSCore #3433]
	if (sqrt(dDiffX * dDiffX + dDiffY * dDiffY) < rZone.m_nHeight)
	{
		dAngle = dAngle >= 0 ? dAngle - MathVars::PI / 2 : dAngle + MathVars::PI / 2;
	}

	return dAngle;
}
//-------------------------------------------------------------------------------------------------
POINT findMidPointOfZone( POINT ptStart, POINT ptEnd )
{
	POINT ptCenterPoint;
	ptCenterPoint.x = ptStart.x + (ptEnd.x - ptStart.x)/2;
	ptCenterPoint.y = ptStart.y + (ptEnd.y - ptStart.y)/2;
	
	return ptCenterPoint;
}
//-------------------------------------------------------------------------------------------------
void addTextToImage(HDC hDC, const PageRasterZone &rZone, int iVerticalDpi)
{
	int nTextLength = rZone.m_strText.size();
	if (nTextLength <= 0)
	{
		return;
	}

	HFONT hFont = NULL;
	try
	{
		// Create and select the font to use to draw the zone
		int iFontSize = 0;
		calculateFontThatFits(hDC, rZone, iVerticalDpi, &hFont, &iFontSize);

		// Set Background mode
		SetBkMode(hDC, TRANSPARENT); 
		
		// Set the text color
		SetTextColor(hDC, rZone.m_crTextColor);

		// Calculate the angle of the line dy/dx
		double dAngle = rasterAngle(rZone);
		
		// Calculate the center point
		POINT center = 
		{
			(rZone.m_nStartX + rZone.m_nEndX) / 2, 
			(rZone.m_nStartY + rZone.m_nEndY) / 2
		};

		// Add the internal leading to the font size [FlexIDSCore #3434]
		TEXTMETRIC metric = {0};
		GetTextMetrics(hDC, &metric);
		int iSize = iFontSize + metric.tmInternalLeading;

		center.x -= (long) (iSize * sin(dAngle) / 2);
		center.y += (long) (iSize * cos(dAngle) / 2);

		// Put the text in the center of the zone
		SetTextAlign(hDC, TA_CENTER | TA_BOTTOM);
		TextOut(hDC, center.x, center.y, rZone.m_strText.c_str(), nTextLength);

		SetBkMode(hDC, OPAQUE);
		DeleteObject( hFont );
		hFont = NULL;
	}
	catch (...)
	{
		if (hFont != __nullptr)
		{
			DeleteObject(hFont);
			hFont = NULL;
		}

		throw;
	}
}
//-------------------------------------------------------------------------------------------------
void loadImagePage(const string& strImageFileName, unsigned long ulPage, BITMAPHANDLE &rBitmap,
				   bool bChangeViewPerspective)
{
	try
	{
		// Get initialized FILEINFO struct
		FILEINFO fileInfo = GetLeadToolsSizedStruct<FILEINFO>(0);

		loadImagePage(strImageFileName, ulPage, rBitmap, fileInfo, bChangeViewPerspective);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27279");
}
//-------------------------------------------------------------------------------------------------
void loadImagePage(const string& strImageFileName, unsigned long ulPage, BITMAPHANDLE &rBitmap,
				   FILEINFO& rflInfo, bool bChangeViewPerspective)
{
	try
	{
		// Get initialized LOADFILEOPTION struct. 
		// Do not ignore view perspective because this could cause the image to be misinterpreted
		// https://extract.atlassian.net/browse/ISSUE-7220
		LOADFILEOPTION lfo = GetLeadToolsSizedStruct<LOADFILEOPTION>(ELO_ROTATED);

		// Get the default load options and set the page
		{
			LeadToolsLicenseRestrictor leadToolsLicenseGuard;

			throwExceptionIfNotSuccess(L_GetDefaultLoadFileOption(&lfo, sizeof(LOADFILEOPTION)),
				"ELI13283", "Unable to get default file load options for LeadTools imaging library.");
		}
		lfo.PageNumber = ulPage;

		loadImagePage(strImageFileName, rBitmap, rflInfo, lfo, bChangeViewPerspective);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28126");
}
//-------------------------------------------------------------------------------------------------
void loadImagePage(const string& strImageFileName, BITMAPHANDLE& rBitmap,
				   FILEINFO& rFileInfo, LOADFILEOPTION& lfo, bool bChangeViewPerspective)
{
	try
	{
		try
		{
			int iRetryCount(0), iRetryTimeout(0);
			getFileAccessRetryCountAndTimeout(iRetryCount, iRetryTimeout);

			// Get the file name as a char*
			char* pszImageFile = (char*) strImageFileName.c_str();

			// Default return to success
			L_INT nRet = SUCCESS;

			// Perform the save operation in a loop
			long nNumFailedAttempts = 0;
			while (nNumFailedAttempts < iRetryCount)
			{

				DWORD dwStartIndex = 0;
				if (getExtensionFromFullPath(strImageFileName, true) == ".pdf")
				{
					dwStartIndex = getPDFStartIndex(strImageFileName);
				}
				if (dwStartIndex == 0)
				{
					LeadToolsLicenseRestrictor leadToolsLicenseGuard;

					nRet = L_LoadBitmap(pszImageFile, &rBitmap, sizeof(BITMAPHANDLE), 0, ORDER_RGB, &lfo, &rFileInfo);
				}
				else
				{
					LeadToolsLicenseRestrictor leadToolsLicenseGuard;

					CHandle hFile(CreateFile(pszImageFile, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, 0, NULL));
					DWORD dwFileSize = GetFileSize(hFile, NULL) - dwStartIndex;
					nRet = L_LoadFileOffset((L_HFILE)hFile.operator HANDLE(), dwStartIndex, dwFileSize, &rBitmap, sizeof(BITMAPHANDLE), 0,
						ORDER_RGB, LOADFILE_ALLOCATE | LOADFILE_STORE, NULL, NULL, &lfo, &rFileInfo);
				}

				// Check result
				if (nRet == SUCCESS)
				{
					// Exit loop
					break;
				}
				else
				{
					// Increment the attempt count and sleep
					nNumFailedAttempts++;
					Sleep(iRetryTimeout);
				}
			}

			// If still not success, throw an exception
			if (nRet != SUCCESS)
			{
				UCLIDException ue("ELI13284", "Cannot load page");
				ue.addDebugInfo("Actual Error Code", nRet);
				ue.addDebugInfo("Error Message", getErrorCodeDescription(nRet));
				ue.addDebugInfo("Number Of Retries", nNumFailedAttempts);
				ue.addDebugInfo("Max Number Of Retries", iRetryCount);
				throw ue;
			}
			// Check if a retry was necessary, if so log an application trace
			else if (nNumFailedAttempts > 0)
			{
				UCLIDException ue("ELI29835",
					"Application Trace: Successfully loaded image page after retry.");
				ue.addDebugInfo("Image File Name", strImageFileName);
				ue.addDebugInfo("Page Number", lfo.PageNumber);
				ue.addDebugInfo("Retries", nNumFailedAttempts);
				ue.log();
			}

			// If bChangeViewPerspective == true && the view perspective is not TOP_LEFT 
			// then attempt to change the view perspective
			if (bChangeViewPerspective && rBitmap.ViewPerspective != TOP_LEFT)
			{
				LeadToolsLicenseRestrictor leadToolsLicenseGuard;

				throwExceptionIfNotSuccess(L_ChangeBitmapViewPerspective(NULL, &rBitmap,
					sizeof(BITMAPHANDLE), TOP_LEFT),"ELI14634",
					"Unable to change bitmap perspective.");
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI29836");
	}
	catch(UCLIDException& ue)
	{
		ue.addDebugInfo("File To Load", strImageFileName);
		ue.addDebugInfo("Page Number", lfo.PageNumber);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
DWORD getPDFStartIndex(const string& strFileName)
{
	CHandle hFile(CreateFile(strFileName.c_str(), GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, 0, NULL));
	// Allow up to 1KB prefix
	const int nMaxPrefix = 1024;
	char cBuf[nMaxPrefix + 4];
	DWORD dwNumOfBytesRead;
	if (!ReadFile(hFile, cBuf, sizeof(cBuf), &dwNumOfBytesRead, NULL)
		|| dwNumOfBytesRead < 4)
	{
		return 0;
	}
	int nMaxStartIndex = dwNumOfBytesRead - 4;
	for (int i = 0; i < nMaxStartIndex; ++i)
	{
		if (cBuf[i] == '%'
			&& cBuf[i+1] == 'P'
			&& cBuf[i+2] == 'D'
			&& cBuf[i+3] == 'F')
		{
			return i;
		}
	}
	return 0;
}
//-------------------------------------------------------------------------------------------------
void saveImagePage(BITMAPHANDLE& hBitmap, const string& strOutputFile, FILEINFO& flInfo,
				   long lPageNumber)
{
	try
	{
		// Create default save file options and set the page number
		SAVEFILEOPTION sfo = GetLeadToolsSizedStruct<SAVEFILEOPTION>(0);
		{
			LeadToolsLicenseRestrictor leadToolsLicenseGuard;

			throwExceptionIfNotSuccess(L_GetDefaultSaveFileOption(&sfo, sizeof(SAVEFILEOPTION)),
				"ELI27292", "Unable to get default save file options.");
		}
		sfo.PageNumber = lPageNumber;

		saveImagePage(hBitmap, strOutputFile, flInfo, sfo);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27282");
}
//-------------------------------------------------------------------------------------------------
void saveImagePage(BITMAPHANDLE& hBitmap, const string& strOutputFile, FILEINFO& flInfo,
				   SAVEFILEOPTION& sfo)
{
	try
	{
		int nFileFormat = flInfo.Format;
		int nBitsPerPixel = flInfo.BitsPerPixel;
		int nCompressionFactor = getCompressionFactor(nFileFormat);

		saveImagePage(hBitmap, strOutputFile, nFileFormat, nCompressionFactor, 
			nBitsPerPixel, sfo);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28838");
}
//-------------------------------------------------------------------------------------------------
void saveImagePage(BITMAPHANDLE& hBitmap, const string& strOutputFile, int nFileFormat,
				   int nCompressionFactor, int nBitsPerPixel, SAVEFILEOPTION& sfo)
{
	try
	{
		try
		{
			// Get the retry count and timeout
			int iRetryCount(0), iRetryTimeout(0);
			getFileAccessRetryCountAndTimeout(iRetryCount, iRetryTimeout);

			// Get the file name as a char*
			char* pszOutFile = (char*) strOutputFile.c_str();

			// Default return to success
			L_INT nRet = SUCCESS;

			// Perform the save operation in a loop
			long nNumFailedAttempts = 0;
			while (nNumFailedAttempts < iRetryCount)
			{
				{
					LeadToolsLicenseRestrictor leadToolsLicenseGuard;

					nRet = L_SaveBitmap(pszOutFile, &hBitmap, nFileFormat, nBitsPerPixel,
						nCompressionFactor, &sfo);
				}

				// Check result
				if (nRet == SUCCESS)
				{
					// Exit loop
					break;
				}
				else
				{
					// Increment the attempt count and sleep
					nNumFailedAttempts++;
					Sleep(iRetryTimeout);
				}
			}

			// If still not success, throw an exception
			if (nRet != SUCCESS)
			{
				UCLIDException ue("ELI27283", "Cannot save page");
				ue.addDebugInfo("Actual Error Code", nRet);
				ue.addDebugInfo("Error Message", getErrorCodeDescription(nRet));
				ue.addDebugInfo("Number Of Retries", nNumFailedAttempts);
				ue.addDebugInfo("Max Number Of Retries", iRetryCount);
				ue.addDebugInfo("Compression Flag", nCompressionFactor);
				addFormatDebugInfo(ue, nFileFormat);
				throw ue;
			}
			// Check if a retry was necessary, if so log an application trace
			else if (nNumFailedAttempts > 0)
			{
				UCLIDException ue("ELI20367",
					"Application Trace: Successfully saved image page after retry.");
				ue.addDebugInfo("Page Number", sfo.PageNumber);
				ue.addDebugInfo("Output File Name", strOutputFile);
				ue.addDebugInfo("Retries", nNumFailedAttempts);
				ue.log();
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27281");
	}
	catch(UCLIDException& uex)
	{
		uex.addDebugInfo("Output File Name", strOutputFile);
		uex.addDebugInfo("Page Number", sfo.PageNumber);

		throw uex;
	}
}
//-------------------------------------------------------------------------------------------------
COLORREF getPixelColor(BITMAPHANDLE &rBitmap, int iRow, int iCol)
{
	// Do not put a LeadToolsLicenseRestrictor at this level this method typically gets called in loop
	// and it will work better to use that at a higher level

	return L_GetPixelColor( &rBitmap, iRow, iCol );
}
//-------------------------------------------------------------------------------------------------
string getLeadUtilsDirectory()
{
	// Build and return path
	string strDLLPath = ::getModuleDirectory( "LeadUtils.dll" );
	strDLLPath += "\\";
	return strDLLPath;
}
//-------------------------------------------------------------------------------------------------
int getFontSizeThatFits(HDC hDC, const PageRasterZone& zone, int iVerticalDpi)
{
	int iFontSize = 0;
	calculateFontThatFits(hDC, zone, iVerticalDpi, NULL, &iFontSize);
	return iFontSize;
}
//-------------------------------------------------------------------------------------------------
void calculateFontThatFits(HDC hDC, const PageRasterZone& zone, int iVerticalDpi, HFONT* phFont, 
	int* piFontSize)
{
	HFONT hFont = NULL;
	try
	{
		// Create structure to create font indirectly
		LOGFONT lf = zone.m_font;
		lf.lfHeight = -MulDiv(zone.m_iPointSize, iVerticalDpi, 72);

		// Text angle equal to the angle of the raster zone in tenths of a degree
		double dAngle = rasterAngle(zone);
		lf.lfEscapement = (long) floor(dAngle * -1800.0 / MathVars::PI + .5);
		lf.lfOrientation = lf.lfEscapement;

		// Create and select the font
		hFont = CreateFontIndirect(&lf);
		SelectObject(hDC, hFont);

		// Calculate the length of the area for the text
		float fDiffX = (float) zone.m_nStartX - zone.m_nEndX;
		float fDiffY = (float) zone.m_nStartY - zone.m_nEndY;
		float fWidth = sqrt(fDiffX * fDiffX + fDiffY * fDiffY);
		float fHeight = (float) zone.m_nHeight;

		// Text will always be written across the longest length [FlexIDSCore #3442]
		if (fWidth < fHeight)
		{
			swap(fWidth, fHeight);
		}
		
		// Check to see how much of the string will fit in the rectangular area
		SIZE sizeOfString = {0};
		const char* pszText = zone.m_strText.c_str();
		GetTextExtentPoint32(hDC, pszText, zone.m_strText.size(), &sizeOfString);

		// If text doesn't fit, shrink it to fit
		if (sizeOfString.cx > fWidth || sizeOfString.cy > fHeight) 
		{
			// Determine the amount needed to scale the string 
			// horizontally and vertically to get it to fit
			float scaleX = fWidth / (float) sizeOfString.cx;
			float scaleY = fHeight / (float) sizeOfString.cy;

			// Scale the font so that it fits both horizontally and vertically
			long lNewHeight = (long)(lf.lfHeight * min(scaleX, scaleY));

			// Only grow or shrink in one direction to prevent an infinite loop [FlexIDSCore #3431]
			bool bShrink = lNewHeight < lf.lfHeight;

			// Loop, each time guessing a better font size, until no better font size can be found
			// [FlexIDSCore #3403, #3408]
			do
			{
				// Store the new font size
				lf.lfHeight = lNewHeight;

				// Create and select the new font
				DeleteObject(hFont);
				hFont = CreateFontIndirect(&lf);
				SelectObject(hDC, hFont);

				// Check to see how much of the string will fit in the rectangular area
				GetTextExtentPoint32(hDC, pszText, zone.m_strText.size(), &sizeOfString);

				// Determine the amount needed to scale the string 
				// horizontally and vertically to get it to fit
				scaleX = fWidth / (float) sizeOfString.cx;
				scaleY = fHeight / (float) sizeOfString.cy;

				// Scale the font so that it fits both horizontally and vertically
				lNewHeight = (long)(lf.lfHeight * min(scaleX, scaleY));
			}
			while (bShrink ? lNewHeight < lf.lfHeight : lNewHeight > lf.lfHeight);
		}

		// Check whether the font should be returned
		if (phFont == NULL)
		{
			// Free the font
			DeleteObject(hFont);
			hFont = NULL;
		}
		else
		{
			// Return the font
			*phFont = hFont;
		}

		// Check whether the font size should be returned
		if (piFontSize != __nullptr)
		{
			*piFontSize = -lf.lfHeight;
		}
	}
	catch(...)
	{
		if (hFont != __nullptr)
		{
			DeleteObject(hFont);
		}

		throw;
	}
}
//-------------------------------------------------------------------------------------------------
int getCompressionFactor(const string& strFormat)
{
	try
	{
		L_INT nFormat = getFormatFromString(strFormat);
		return getCompressionFactor(nFormat);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25418");
}
//-------------------------------------------------------------------------------------------------
int getCompressionFactor(L_INT nFormat)
{
	// Static map to store the format to compression factor values
	static map<L_INT, int> smapFormatToCompressionFactor;

	static CCriticalSection criticalSection;

	try
	{
		// Default value to 0 (this flag will work for most compression values although
		// the files will be very large).
		int nReturn = 0;

		// Get the string value for the format
		string strFormat = getStringFromFormat(nFormat);

		// Get a registry persistence manager to search for a compression value
		// for this file type
		RegistryPersistenceMgr rpm(HKEY_LOCAL_MACHINE, gstrRC_REG_PATH);

		// Mutex while accessing the map
		CSingleLock lg(&criticalSection, TRUE);

		// Look for the value in the map
		map<L_INT, int>::iterator it = smapFormatToCompressionFactor.find(nFormat);
		if (it != smapFormatToCompressionFactor.end())
		{
			// Get the value from the map
			nReturn = it->second;
		}
		else
		{
			// Check for registry key
			if (rpm.keyExists(gstrLEADTOOLS_COMPRESSION_VALUE_FOLDER, strFormat))
			{
				// Get the value from the registry
				nReturn = (int)
					asLong(rpm.getKeyValue(gstrLEADTOOLS_COMPRESSION_VALUE_FOLDER, strFormat, ""));
			}
			else
			{
				// If format is a known type then get default value and set the registry key
				// to the default value.
				// [FlexIDSCore:5055]
				// Not specifying a compression factor for a JPEG image either causes a failure or
				// generates a corrupt image. Per chat with LeadTools support, it is safe to use
				// a compression factor for any JPEG related format... if it is not needed, it is
				// not used. Therefore, adding a compression factor for any JPEG related format.
				bool bKnownFormat = false;
				switch(nFormat)
				{
				case FILE_CMP:
				case FILE_DICOM_JPEG_GRAY:
				case FILE_DICOM_JPEG_COLOR:
				case FILE_EXIF_JPEG:
				case FILE_EXIF_JPEG_411:
				// FILE_EXIF_JPEG_422 is the same as FILE_EXIF_JPEG.
				case FILE_FPX_JPEG:
				case FILE_FPX_SINGLE_COLOR:
				case FILE_FPX_JPEG_QFACTOR:
				case FILE_J2K:
				case FILE_JLS:
				case FILE_JP2:
				case FILE_JPM:
				case FILE_JPX:
				case FILE_JPEG:
				case FILE_JPEG_411:
				case FILE_JPEG_422:
				case FILE_JPEG_LAB:
				case FILE_JPEG_LAB_411:
				case FILE_JPEG_LAB_422:
				case FILE_JPEG_RGB:
				case FILE_JXR:
				case FILE_JXR_420:
				case FILE_JXR_422:
				case FILE_JXR_GRAY:
				case FILE_JXR_CMYK:
				case FILE_PPT_JPEG:
				case FILE_RAS_PDF_JPEG:
				case FILE_RAS_PDF_JPEG_411:
				case FILE_RAS_PDF_JPEG_422:
				case FILE_RAW_JPEG:
				case FILE_TIF_J2K:
				case FILE_TIF_JPEG:
				case FILE_TIF_JPEG_411:
				case FILE_TIF_JPEG_422:
				case FILE_TIFX_JPEG:
				case FILE_XPS_JPEG:
				case FILE_XPS_JPEG_411:
				case FILE_XPS_JPEG_422:
					nReturn = giDEFAULT_JPEG_COMPRESSION_FLAG;
					bKnownFormat = true;
					break;
				}

				// If the type was a known format, set the registry key with the default value
				if (bKnownFormat)
				{
					rpm.setKeyValue(gstrLEADTOOLS_COMPRESSION_VALUE_FOLDER,
						strFormat, asString(nReturn));
				}
			}

			// Store the compression factor in the map (even if there was no key in the registry)
			smapFormatToCompressionFactor[nFormat] = nReturn;
		}

		// Unlock the mutex
		lg.Unlock();

		// Return the compression factor
		return nReturn;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25419");
}
//-------------------------------------------------------------------------------------------------
void validateRedactionZones(const vector<PageRasterZone>& vecZones, long nNumberOfPages)
{
	try
	{
		// Validate each raster zone in the collection
		for (vector<PageRasterZone>::const_iterator it = vecZones.begin();
			it != vecZones.end(); it++)
		{
			// Validate non-empty zone
			if (it->isEmptyZone()) 
			{
				UCLIDException ue("ELI09200", "Empty zone.");
				throw ue;
			}

			// Validate page number
			long nPage = it->m_nPage;
			if( nPage > nNumberOfPages || nPage < 1 )
			{
				UCLIDException ue("ELI09201", "Page number selected does not exist.");
				ue.addDebugInfo("Page", nPage );
				ue.addDebugInfo("Total number of pages", nNumberOfPages);
				throw ue;
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27293");
}
//-------------------------------------------------------------------------------------------------
void applyAnnotationText(const PageRasterZone& rZone, HANNOBJECT& hContainer, HDC hDC, int iYResolution, ANNRECT& rect)
{
	try
	{
		// Check if any text was specified
		if (!rZone.m_strText.empty())
		{
			int iFontSize = getFontSizeThatFits(hDC, rZone, iYResolution);

			LeadToolsLicenseRestrictor leadToolsLicenseGuard;

			// Get the current annotation options
			L_UINT uOptions = 0;
			throwExceptionIfNotSuccess(L_AnnGetOptions(&uOptions), "ELI24470",
				"Could not get annotation options.");

			// Ensure text options are available
			uOptions |= OPTIONS_NEW_TEXT_OPTIONS;

			// Set the options
			throwExceptionIfNotSuccess(L_AnnSetOptions(NULL, uOptions), "ELI24471",
				"Could not set text annotation options.");

			// Create a text annotation object
			HANNOBJECT hText;
			throwExceptionIfNotSuccess(L_AnnCreate(ANNOBJECT_TEXT, &hText), "ELI24465", 
				"Could not create text annotation object.");

			// Make text object visible
			throwExceptionIfNotSuccess(L_AnnSetVisible(hText, TRUE, 0, NULL), "ELI24467", 
				"Could not set visibility for redaction annotation object.");

			// Set the font size
			throwExceptionIfNotSuccess(L_AnnSetFontSize(hText, iFontSize, 0), "ELI24472",
				"Could not set font size.");

			// Set the font name
			throwExceptionIfNotSuccess(L_AnnSetFontName(hText, (char*)rZone.m_font.lfFaceName, 0),
				"ELI24473", "Could not set font name.");

			// Set text color
			ANNTEXTOPTIONS textOptions = 
				GetLeadToolsSizedStruct<ANNTEXTOPTIONS>(0);
			textOptions.bShowText = TRUE;
			textOptions.bShowBorder = FALSE;
			textOptions.crText = rZone.m_crTextColor;
			textOptions.uFlags = ANNTEXT_ALL;
			throwExceptionIfNotSuccess(L_AnnSetTextOptions(hText, &textOptions, 0), "ELI24474",
				"Could not set font name.");

			// Set the tiff tag
			throwExceptionIfNotSuccess(L_AnnSetTag(hText, ANNTAG_TIFF, 0), "ELI24468", 
				"Could not set annotation tag.");

			// Set the spatial boundaries for the text annotation
			throwExceptionIfNotSuccess(L_AnnSetRect(hText, &rect), "ELI24469", 
				"Could not bound text annotation object.");

			// Set the text
			throwExceptionIfNotSuccess(L_AnnSetText(hText, (char*)rZone.m_strText.c_str(), 0),
				"ELI24475", "Could not set text.");

			// Insert the text object into the container
			throwExceptionIfNotSuccess(L_AnnInsert(hContainer, hText, FALSE), "ELI24466", 
				"Could not insert text annotation object." );
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27294");
}
//-------------------------------------------------------------------------------------------------
void createLeadDC(HDC& hDC, BITMAPHANDLE& hBitmap)
{
	// Create a device context if it has not been created already
	if (hDC == NULL)
	{
		LeadToolsLicenseRestrictor leadToolsLicenseGuard;

		hDC = L_CreateLeadDC(&hBitmap);
		if (hDC == NULL)
		{
			UCLIDException uex("ELI24891", "Unable to create device context.");
			throw uex;
		}
	}
}
//-------------------------------------------------------------------------------------------------
void deleteLeadDC(HDC& hDC)
{
	if (hDC != __nullptr)
	{
		LeadToolsLicenseRestrictor leadToolsLicenseGuard;

		if (L_DeleteLeadDC(hDC) == L_FALSE)
		{
			// Still set this to NULL, even if we failed
			hDC = NULL;
			UCLIDException ue("ELI28230", "Failed to delete device context.");
			throw ue;
		}

		hDC = NULL;
	}
}
//-------------------------------------------------------------------------------------------------
bool compareZoneByPage(const PageRasterZone& leftZone, const PageRasterZone& rightZone)
{
	return leftZone.m_nPage < rightZone.m_nPage;
}
//-------------------------------------------------------------------------------------------------
void pageZoneToAnnRect(const PageRasterZone &rZone, ANNRECT& rRect)
{
	// Calculate points on bounding rectangle
	POINT p1, p2, p3, p4;
	pageZoneToPoints(rZone, p1, p2, p3, p4);

	// Apply bounding RECT to redaction object
	rRect.top = min(p1.y, min(p2.y, min(p3.y, p4.y)));
	rRect.left = min(p1.x, min(p2.x, min(p3.x, p4.x)));
	rRect.bottom = max(p1.y, max(p2.y, max(p3.y, p4.y)));
	rRect.right = max(p1.x, max(p2.x, max(p3.x, p4.x)));
}
//-------------------------------------------------------------------------------------------------
string encryptString(const string& strString)
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

	// Encrypt the string
	ByteStream bytes;
	ByteStreamManipulator bsmBytes(ByteStreamManipulator::kWrite, bytes);
	bsmBytes << strString;
	bsmBytes.flushToByteStream(8);

	ByteStream encrypted;
	MapLabel encryptionEngine;
	encryptionEngine.setMapLabel(encrypted, bytes, bytesKey);

	return encrypted.asString();
}
//-------------------------------------------------------------------------------------------------
void drawRedactionZone(HDC hDC, const PageRasterZone& rZone, int nYResolution,
					   bool bApplyText /*= true*/)
{
	BrushCollection brushes;
	PenCollection pens;
	drawRedactionZone(hDC, rZone, nYResolution, brushes, pens, bApplyText);
}
//-------------------------------------------------------------------------------------------------
void drawRedactionZone(HDC hDC, const PageRasterZone& rZone, int nYResolution,
					   BrushCollection& rBrushes, PenCollection& rPens, bool bApplyText/* = true*/)
{
	try
	{
		// Set the appropriate brush and pen
		if (SelectObject(hDC, rBrushes.getColoredBrush(rZone.m_crFillColor)) == NULL)
		{
			UCLIDException ue("ELI28227", "Failed to set fill color.");
			ue.addWin32ErrorInfo();
			throw ue;
		}
		if (SelectObject(hDC, rPens.getColoredPen(rZone.m_crBorderColor)) == NULL)
		{
			UCLIDException ue("ELI28228", "Failed to set border color.");
			ue.addWin32ErrorInfo();
			throw ue;
		}

		// Convert the Zone to rectangle corner points
		POINT aPoints[4];
		pageZoneToPoints( rZone, aPoints[0], aPoints[1],
			aPoints[2], aPoints[3]);

		// Draw the Polygon
		if (Polygon(hDC, (POINT *) &aPoints, 4) == FALSE)
		{
			UCLIDException ue("ELI28229", "Failed to draw redaction zone.");
			ue.addWin32ErrorInfo();
			throw ue;
		}

		// If there is text to add, add it
		if (bApplyText && rZone.m_strText.size() > 0)
		{
			addTextToImage(hDC, rZone, nYResolution);
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28128");
}
//-------------------------------------------------------------------------------------------------
// Callback function for L_AnnEnumerate that will burn redactions into the image.
// For each rect or redact annotation object get the color and if the
// color is either black or white, burn the annotation into the image
L_INT EXT_CALLBACK burnRedactions(HANNOBJECT hObject, L_VOID* pUserData)
{
	L_INT nRet = SUCCESS;
	try
	{
		// Get the bitmap handle
		pBITMAPHANDLE pbmp = (pBITMAPHANDLE) pUserData;

		// Get the annotation type
		L_UINT ObjectType;
		nRet = L_AnnGetType(hObject, &ObjectType);
		throwExceptionIfNotSuccess(nRet, "ELI36833", "Failed to get annotation type.");

		// If the type is either rect or redact then get the color
		if (ObjectType == ANNOBJECT_RECT || ObjectType == ANNOBJECT_REDACT)
		{
			// Get the color
			COLORREF color;
			nRet = L_AnnGetBackColor(hObject, &color);
			throwExceptionIfNotSuccess(nRet, "ELI36834", "Failed to get annotation color.");

			// If the color is black or white then "burn" the annotation into image
			if (color == gCOLOR_BLACK || color == gCOLOR_WHITE)
			{
				// Burn the annotation into the image
				nRet = L_AnnRealize(pbmp, NULL, hObject, FALSE);
				throwExceptionIfNotSuccess(nRet, "ELI36835", "Failed to burn annotation into image.");
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
void convertImageColorDepth(BITMAPHANDLE& hBitmap, string strImageFileName, long nBitsPerPixel,
	bool bUseDithering, bool bUseAdaptiveThresholdToConvertToBitonal)
{
	// If specified, when converting from color/grayscale to bitonal, use an adaptive threshold algorithm
	// https://extract.atlassian.net/browse/ISSUE-14801
	if (bUseAdaptiveThresholdToConvertToBitonal && nBitsPerPixel == 1)
	{
		unlockDocumentSupport();
		L_INT nRet = L_AutoBinarizeBitmap(&hBitmap, 0, AUTO_BINARIZE_PRE_AUTO | AUTO_BINARIZE_THRESHOLD_AUTO);
		throwExceptionIfNotSuccess(nRet, "ELI44666",
			"Internal error: Unable to binarize image!", strImageFileName);
	}

	const long nDEFAULT_NUMBER_OF_COLORS = 0;
	L_UINT flags = CRF_FIXEDPALETTE | (bUseDithering ? CRF_ORDEREDDITHERING : CRF_NODITHERING);
	L_INT nRet = L_ColorResBitmap(&hBitmap, &hBitmap, sizeof(BITMAPHANDLE), nBitsPerPixel, flags,
		NULL, NULL, nDEFAULT_NUMBER_OF_COLORS, NULL, NULL);

	throwExceptionIfNotSuccess(nRet, "ELI42166",
			"Internal error: Unable to convert image to specified bits-per-pixel!", strImageFileName);

	// Set the image palette to white, black to ensure consistency in reading image data
	// (0 = white, 1 = black)
	if (nBitsPerPixel == 1)
	{
		L_RGBQUAD palette[2] = { {255, 255, 255, 0}, {0, 0, 0, 0} };
		nRet = L_ColorResBitmap(&hBitmap, &hBitmap, sizeof(BITMAPHANDLE),
			hBitmap.BitsPerPixel, CRF_USERPALETTE, palette, NULL, 2, NULL, NULL);
		throwExceptionIfNotSuccess(nRet, "ELI22238",
			"Internal error: Failed to set image palette.", strImageFileName);
	}
}
//-------------------------------------------------------------------------------------------------
