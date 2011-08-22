
#include "stdafx.h"
#include "MiscLeadUtils.h"
#include "ImageConversion.h"
#include "LeadToolsBitmapFreeer.h"
#include "LeadToolsFormatHelpers.h"

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

#include <cmath>
#include <cstdio>
#include <algorithm>
#include <string>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// Registry path and key for LeadTools serialization
const string gstrLEADTOOLS_SERIALIZATION_PATH = "\\VendorSpecificUtils\\LeadUtils";
const string gstrSERIALIZATION_KEY = "Serialization"; 
const string gstrDEFAULT_SERIALIZATION = "0"; 

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
	bool bRetainAnnotations, bool bApplyAsAnnotations, const string& strUserPassword,
	const string& strOwnerPassword, int nPermissions)
{
	fillImageArea(strImageFileName, strOutputImageName, nLeft, nTop, nRight, nBottom, nPage,
		color, 0, "", 0, bRetainAnnotations, bApplyAsAnnotations, strUserPassword,
		strOwnerPassword, nPermissions);
}
//-------------------------------------------------------------------------------------------------
void fillImageArea(const string& strImageFileName, const string& strOutputImageName, long nLeft, 
	long nTop, long nRight, long nBottom, long nPage, const COLORREF crFillColor, 
	const COLORREF crBorderColor, const string& strText, const COLORREF crTextColor, 
	bool bRetainAnnotations, bool bApplyAsAnnotations, const string& strUserPassword,
	const string& strOwnerPassword, int nPermissions)
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
		bApplyAsAnnotations, strUserPassword, strOwnerPassword, nPermissions);
}
//-------------------------------------------------------------------------------------------------
void fillImageArea(const string& strImageFileName, const string& strOutputImageName, 
				   vector<PageRasterZone>& rvecZones, bool bRetainAnnotations, 
				   bool bApplyAsAnnotations, const string& strUserPassword,
				   const string& strOwnerPassword, int nPermissions)
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
		LicenseManagement::verifyFileTypeLicensed(strImageFileName);
		LicenseManagement::verifyFileTypeLicensed(strOutputImageName);

		// Sort the vector of zones by page
		sort(rvecZones.begin(), rvecZones.end(), compareZoneByPage);

		// Get the retry counts and timeout value
		int iRetryCount(0), iRetryTimeout(0);
		getFileAccessRetryCountAndTimeout(iRetryCount, iRetryTimeout);
		_lastCodePos = "20";

		// Check if outputting a PDF file
		bool bOutputIsPdf = isPDFFile(strOutputImageName);

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
		// IgnoreViewPerspective to avoid a black region at the bottom of the image
		LOADFILEOPTION lfo =
			GetLeadToolsSizedStruct<LOADFILEOPTION>(ELO_IGNOREVIEWPERSPECTIVE);

		// Validate each zone
		validateRedactionZones(rvecZones, nNumberOfPages);

		// Create the brush and pen collections
		BrushCollection brushes;
		PenCollection pens;

		// If output is a PDF, write to temporary tif and convert at the end
		string strOutputWorking = strOutputImageName;
		unique_ptr<TemporaryFileName> pPDFOut(__nullptr);
		if (bOutputIsPdf)
		{
			pPDFOut.reset(new TemporaryFileName(true, __nullptr, ".tif"));
			strOutputWorking = pPDFOut->getName();
		}

		// loop to allow for multiple attempts to fill an image area (P16 #2593)
		bool bSuccessful = false;
		bool bAnnotationsAppliedToDocument = false;
		_lastCodePos = "40";
		for (int i=0; i < gnOUTPUT_IMAGE_RETRIES; i++)
		{
			// Write the output to a temporary file so that the creation of the
			// redacted image appears as an atomic operation [FlexIDSCore #3547]
			TemporaryFileName tempOutFile(true, NULL,
				getExtensionFromFullPath(strOutputWorking).c_str(), true);

			// Flag to indicate if any annotations been applied, if so then
			// L_AnnSetTag will need to be called to reset them
			bool bAnnotationsAppliedToPage = false;

			// Declare objects outside of try scope so that they can be released if an exception
			// is thrown
			HANNOBJECT hContainer = NULL; // Annotation container for redactions
			_lastCodePos = "50";
			try
			{
				try
				{
					int nRet = FAILURE;

					// Get initialized SAVEFILEOPTION struct
					SAVEFILEOPTION sfOptions = GetLeadToolsSizedStruct<SAVEFILEOPTION>(0);
					nRet = L_GetDefaultSaveFileOption(&sfOptions, sizeof(sfOptions));
					throwExceptionIfNotSuccess(nRet, "ELI27299", "Unable to get default save options.");
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

						bool bLoadExistingAnnotations = bRetainAnnotations
							&& hasAnnotations(strImageFileName, lfo, iFormat);

						// Create Annotation container sized to image extent if applying as annotations
						// or retaining existing annotations
						if (bApplyAsAnnotations || bLoadExistingAnnotations)
						{
							ANNRECT rect = {0, 0, fileInfo.Width, fileInfo.Height};
							nRet = L_AnnCreateContainer( NULL, &rect, FALSE, &hContainer );
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
										// Insert the existing annotations from File Container
										// into the main container. This destroys the File Container.
										nRet = L_AnnInsert(hContainer, hFileContainer, TRUE);
										throwExceptionIfNotSuccess( nRet, "ELI14632", 
											"Could not insert existing annotation objects.");
										bAnnotationsAppliedToPage = true;
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

								_lastCodePos = "70_D_Page#" + strPageNumber;
								if (bApplyAsAnnotations)
								{
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

									// Apply annotation text
									applyAnnotationText((*it), hContainer, ltDC.m_hDC,
										fileInfo.YResolution, rect);

									bAnnotationsAppliedToPage = true;
								}
								else
								{
									// Draw the redaction
									drawRedactionZone(ltDC.m_hDC, *it,
										fileInfo.YResolution, brushes, pens);
								}
								_lastCodePos = "70_E_Page#" + strPageNumber;
							} // end if this zone is on this page
						} // end for each zone
						_lastCodePos = "70_F_Page#" + strPageNumber;

						// Set the page number for save options
						sfOptions.PageNumber = i;

						if (!bAnnotationsAppliedToPage)
						{
							// Save the image page
							saveImagePage(hBitmap, tempOutFile.getName(), fileInfo, sfOptions);
						}
						else
						{
							// Save the collected redaction annotations
							//   Save in WANG-mode for greatest compatibility
							//   The next call to SaveBitmap will include these annotations
							nRet = L_AnnSaveTag(hContainer, ANNFMT_WANGTAG, FALSE );
							throwExceptionIfNotSuccess(nRet, "ELI14611", 
								"Could not save redaction annotation objects.");

							// Save the image page with the annotations
							saveImagePage(hBitmap, tempOutFile.getName(), fileInfo, sfOptions);

							// Set annotations added to document flag [FlexIDSCore #3131]
							bAnnotationsAppliedToDocument = true;

							// Clear any previously defined annotations
							// If not done, any annotations applied to this page may be applied to 
							// successive pages [FlexIDSCore #2216]
							nRet = L_SetTag(ANNTAG_TIFF, 0, 0, NULL);

							// Reset annotations applied to page flag
							bAnnotationsAppliedToPage = false;
						}
						_lastCodePos = "70_H_Page#" + strPageNumber;

						// Destroy the annotation container
						if (hContainer != __nullptr)
						{
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
				uex.addDebugInfo("Input Image File", strImageFileName);
				uex.addDebugInfo("Output Image File", strOutputImageName);

				// Need to clear annotation tags if any where applied
				if (bAnnotationsAppliedToPage)
				{
					// Clear any previously defined annotations
					// If not done, any annotations applied to this page may be applied to 
					// successive pages [FlexIDSCore #2216]
					L_SetTag(ANNTAG_TIFF, 0, 0, NULL);
					bAnnotationsAppliedToPage = false;
				}

				// Destroy the annotation containers
				if (hContainer != __nullptr)
				{
					try
					{
						// Destroy the annotation container
						throwExceptionIfNotSuccess(L_AnnDestroy(hContainer, ANNFLAG_RECURSE), 
							"ELI27297",	"Application trace: Unable to destroy annotation container.");
					}
					catch(UCLIDException& ex)
					{
						ex.log();
					}
					hContainer = NULL;
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
				copyFile(tempOutFile.getName(), strOutputWorking);
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
			if (bOutputIsPdf)
			{
				convertTIFToPDF(pPDFOut->getName(), strOutputImageName,
					bAnnotationsAppliedToDocument, strUserPassword, strOwnerPassword, nPermissions);

				if(bAnnotationsAppliedToDocument)
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
	L_GetDefaultSaveFileOption( &sfOptions, sizeof ( sfOptions ));

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
			L_INT nRet = L_LoadBitmap( (char*)strPage.c_str(), &hTmpBmp, 
				sizeof(BITMAPHANDLE), 0, ORDER_RGB, NULL, 0);
			throwExceptionIfNotSuccess(nRet, "ELI09044", "Unable to load bitmap.", strPage);

			// Save the page to the multipage image using the format of the first page of the image
			sfOptions.PageNumber = i + 1;
			nRet = L_SaveBitmap(pszOutput, &hTmpBmp, fileInfo.Format, 
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

			nRet = L_FileInfo(pszFileName, &rFileInfo, sizeof(FILEINFO), flags, pLFO);

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
int getNumberOfPagesInImage( const string& strImageFileName )
{
	try
	{
		// Get initialized FILEINFO struct
		FILEINFO fileInfo;
		getFileInformation(strImageFileName, true, fileInfo);

		// Return actual page count
		return fileInfo.TotalPages;
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
	if ( !LicenseManagement::isPDFLicensed() )
	{
		// pdf support is not licensed
		return;
	}
	else
	{
		bool bCouldNotUnlock = false;

		// Only unlock read support if not already unlocked
		if ( L_IsSupportLocked(L_SUPPORT_PDF_READ) == L_TRUE )
		{
			// Unlock support for PDF Reading
			L_UnlockSupport(L_SUPPORT_PDF_READ, L_KEY_PDF_READ);

			// check if pdf support was unlocked
			if( L_IsSupportLocked(L_SUPPORT_PDF_READ) == L_TRUE )
			{
				// log an exception
				UCLIDException ue("ELI19815", "Unable to unlock PDF read support.");
				ue.addDebugInfo("PDF Read Key", L_KEY_PDF_READ, true);
				ue.log();

				// set the could not unlock flag
				bCouldNotUnlock = true;
			}
		}

		// only unlock write support if not already unlocked
		if ( L_IsSupportLocked(L_SUPPORT_PDF_SAVE) == L_TRUE )
		{
			// unlock support for PDF writing
			L_UnlockSupport(L_SUPPORT_PDF_SAVE, L_KEY_PDF_SAVE);

			// check if pdf support was unlocked
			if( L_IsSupportLocked(L_SUPPORT_PDF_SAVE) == L_TRUE )
			{
				// log an exception
				UCLIDException ue("ELI19863", "Unable to unlock PDF save support.");
				ue.addDebugInfo("PDF Save Key", L_KEY_PDF_SAVE, true);
				ue.log();
				bCouldNotUnlock = true;
			}
		}

		// if pdf support was not unlocked, stop now.
		if(bCouldNotUnlock)
		{
			return;
		}
	}

	// Get initialized FILEPDFOPTIONS struct
	FILEPDFOPTIONS pdfOptions = GetLeadToolsSizedStruct<FILEPDFOPTIONS>(0);
	// Individual scope for L_GetPDFOptions() and L_SetPDFOptions()
	{
		// Retrieve default load options
		L_GetPDFOptions( &pdfOptions, sizeof(pdfOptions) );

		// Only set options if not already the correct options
		if ( pdfOptions.nXResolution != iOpenXRes ||
				pdfOptions.nYResolution != iOpenYRes ||
				pdfOptions.nDisplayDepth != nDisplayDepth )
		{
			// Define desired resolution and display depth settings
			pdfOptions.nXResolution = iOpenXRes;
			pdfOptions.nYResolution = iOpenYRes;
			pdfOptions.nDisplayDepth = nDisplayDepth;

			// Apply settings
			L_SetPDFOptions( &pdfOptions );
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
		L_UnlockSupport(L_SUPPORT_DOCUMENT, L_KEY_DOCUMENT);

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

	// create the load file options
	LOADFILEOPTION lfo = GetLeadToolsSizedStruct<LOADFILEOPTION>(ELO_IGNOREVIEWPERSPECTIVE);
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
		// IgnoreViewPerspective to avoid a black region at the bottom of the image
		LOADFILEOPTION lfo = GetLeadToolsSizedStruct<LOADFILEOPTION>(ELO_IGNOREVIEWPERSPECTIVE);

		// Get the default load options and set the page
		throwExceptionIfNotSuccess(L_GetDefaultLoadFileOption(&lfo, sizeof(LOADFILEOPTION)),
			"ELI13283", "Unable to get default file load options for LeadTools imaging library.");
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

				nRet = L_LoadBitmap(pszImageFile, &rBitmap, sizeof(BITMAPHANDLE), 0,
					ORDER_RGB, &lfo, &rFileInfo);

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
void saveImagePage(BITMAPHANDLE& hBitmap, const string& strOutputFile, FILEINFO& flInfo,
				   long lPageNumber)
{
	try
	{
		// Create default save file options and set the page number
		SAVEFILEOPTION sfo = GetLeadToolsSizedStruct<SAVEFILEOPTION>(0);
		throwExceptionIfNotSuccess(L_GetDefaultSaveFileOption(&sfo, sizeof(SAVEFILEOPTION)),
			"ELI27292", "Unable to get default save file options.");
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
				nRet = L_SaveBitmap(pszOutFile, &hBitmap, nFileFormat, nBitsPerPixel,
					nCompressionFactor, &sfo);

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

	static CMutex mutex;

	try
	{
		// Default value to 0 (this flag will work for most compression values although
		// the files will be very large).
		int nReturn = 0;

		// Get the string value for the format
		string strFormat = getStringFromFormat(nFormat);

		// Get a registry persistance manager to search for a compression value
		// for this file type
		RegistryPersistenceMgr rpm(HKEY_LOCAL_MACHINE, gstrRC_REG_PATH);

		// Mutex while accessing the map
		CSingleLock lg(&mutex, TRUE);

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
				// to the default value
				bool bKnownFormat = false;
				switch(nFormat)
				{
				case FILE_TIF_JPEG:
				case FILE_TIF_JPEG_411:
				case FILE_TIF_JPEG_422:
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

			// Get the current annotation options
			L_UINT uOptions = 0;
			throwExceptionIfNotSuccess(L_AnnGetOptions(&uOptions), "ELI24470",
				"Could not get annotation options.");

			// Ensure text options are available
			uOptions |= OPTIONS_NEW_TEXT_OPTIONS;

			// Set the options
			throwExceptionIfNotSuccess(L_AnnSetOptions(NULL, uOptions), "ELI24471",
				"Could not set text annotation options.");

			// Creat a text annotation object
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
void drawRedactionZone(HDC hDC, const PageRasterZone& rZone, int nYResolution)
{
	BrushCollection brushes;
	PenCollection pens;
	drawRedactionZone(hDC, rZone, nYResolution, brushes, pens);
}
//-------------------------------------------------------------------------------------------------
void drawRedactionZone(HDC hDC, const PageRasterZone& rZone, int nYResolution,
					   BrushCollection& rBrushes, PenCollection& rPens)
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
		if ( rZone.m_strText.size() > 0 )
		{
			addTextToImage(hDC, rZone, nYResolution);
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI28128");
}
//-------------------------------------------------------------------------------------------------
