
#include "stdafx.h"
#include "MiscLeadUtils.h"
#include "ImageConversion.h"
#include "PDFInputOutputMgr.h"
#include "LeadToolsFormatHelpers.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <RegConstants.h>
#include <RegistryPersistenceMgr.h>
#include <mathUtil.h>
#include <LicenseMgmt.h>
#include <LtWrappr.h>
#include <ltann.h>			// LeadTools Annotation functions
#include <StringCSIS.h>
#include <TemporaryFileName.h>

#include <cmath>
#include <cstdio>

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// Registry path and key for LeadTools serialization
const std::string gstrLEADTOOLS_SERIALIZATION_PATH = "\\VendorSpecificUtils\\LeadUtils";
const std::string gstrSERIALIZATION_KEY = "Serialization"; 

// Path to the leadtools compression flag folder
const std::string gstrLEADTOOLS_COMPRESSION_VALUE_FOLDER =
	"\\VendorSpecificUtils\\LeadUtils\\CompressionFlags";

// Default value for JPEG compression flag (produces reasonably small file while
// maintaining fairly high image quality)
const int giDEFAULT_JPEG_COMPRESSION_FLAG = 80;

//-------------------------------------------------------------------------------------------------
// Private class - LeadToolsPDFLoadLocker
//-------------------------------------------------------------------------------------------------
// Declare CMutex object
CMutex LeadToolsPDFLoadLocker::ms_mutex;

// The maximum opacity (ie. completely opaque)
L_INT giMAX_OPACITY = 255;

// Default values for registry items
bool	LeadToolsPDFLoadLocker::ms_bRegistryValueRead = false;
bool	LeadToolsPDFLoadLocker::ms_bSerializeLeadToolsCalls = false;

//-------------------------------------------------------------------------------------------------
LeadToolsPDFLoadLocker::LeadToolsPDFLoadLocker(const std::string& strFileName)
:m_pLock(NULL)
{
	// If a file is PDF file, acquire ownership of the mutex
	// so that other thread could not call the loadbitmap method
	if (isPDFFile( strFileName ))
	{
		m_pLock = new CSingleLock(&ms_mutex, TRUE);		
	}
	// Check registry setting to determine mutex ownership
	else
	{
		// Read registry setting once
		if (!ms_bRegistryValueRead)
		{
			ms_bSerializeLeadToolsCalls = isLeadToolsSerialized();
			ms_bRegistryValueRead = true;
		}

		// Create lock object if serialization is required
		if (ms_bSerializeLeadToolsCalls)
		{
			m_pLock = new CSingleLock(&ms_mutex, TRUE);		
		}
	}
}
//-------------------------------------------------------------------------------------------------
LeadToolsPDFLoadLocker::LeadToolsPDFLoadLocker(const bool bProgrammaticallyForceLock)
:m_pLock(NULL)
{
	// Acquire ownership of the mutex so that other threads cannot call L_xxx() functions
	if (bProgrammaticallyForceLock)
	{
		m_pLock = new CSingleLock(&ms_mutex, TRUE);		
	}
	// Check registry setting to determine mutex ownership
	else
	{
		// Read registry setting once
		if (!ms_bRegistryValueRead)
		{
			ms_bSerializeLeadToolsCalls = isLeadToolsSerialized();
			ms_bRegistryValueRead = true;
		}

		// Create lock object if serialization is required
		if (ms_bSerializeLeadToolsCalls)
		{
			m_pLock = new CSingleLock(&ms_mutex, TRUE);		
		}
	}
}
//-------------------------------------------------------------------------------------------------
LeadToolsPDFLoadLocker::~LeadToolsPDFLoadLocker()
{
	try
	{
		if (m_pLock)
		{
			// Release the mutex
			delete m_pLock;
			m_pLock = NULL;
		}
	}	
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16469");
}

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
// PROMISE: To return the path to the Leadtools PDF initialization files
string getPDFInitializationDirectory();
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
// Exported DLL Functions
//-------------------------------------------------------------------------------------------------
string getErrorCodeDescription(int iErrorCode)
{
	return LBase::GetErrorString(iErrorCode);
}
//-------------------------------------------------------------------------------------------------
void throwExceptionIfNotSuccess(L_INT iErrorCode, const std::string& strELICode, 
								const std::string& strErrorDescription,
								const std::string& strFileName)
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
	bool bRetainAnnotations, bool bApplyAsAnnotations)
{
	fillImageArea(strImageFileName, strOutputImageName, nLeft, nTop, nRight, nBottom, nPage,
		color, 0, "", 0, bRetainAnnotations, bApplyAsAnnotations);
}
//-------------------------------------------------------------------------------------------------
void fillImageArea(const string& strImageFileName, const string& strOutputImageName, long nLeft, 
	long nTop, long nRight, long nBottom, long nPage, const COLORREF crFillColor, 
	const COLORREF crBorderColor, const string& strText, const COLORREF crTextColor, 
	bool bRetainAnnotations, bool bApplyAsAnnotations)
{
	vector<PageRasterZone> vecZones;
	PageRasterZone zone;
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
		bApplyAsAnnotations);
}
//-------------------------------------------------------------------------------------------------
void fillImageArea(const std::string& strImageFileName, const std::string& strOutputImageName, 
				   const std::vector<PageRasterZone> &vecZones, bool bRetainAnnotations, 
				   bool bApplyAsAnnotations)
{
	INIT_EXCEPTION_AND_TRACING("MLI02774");
	try
	{
		// Check for empty collection of redaction areas
		if ( vecZones.size() == 0 ) 
		{
			// Nothing to do
			return;
		}

		// Check if an annotation license is required
		if (bRetainAnnotations || bApplyAsAnnotations)
		{
			if (!LicenseManagement::sGetInstance().isAnnotationLicensed())
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
		LicenseManagement::sGetInstance().verifyFileTypeLicensed(strImageFileName);
		LicenseManagement::sGetInstance().verifyFileTypeLicensed(strOutputImageName);

		// Get the retry counts and timeout value
		int iRetryCount(0), iRetryTimeout(0);
		getFileAccessRetryCountAndTimeout(iRetryCount, iRetryTimeout);
		_lastCodePos = "20";

		// loop to allow for multiple attempts to fill an image area (P16 #2593)
		bool bSuccessful = false;
		bool bAnnotationsAppliedToDocument = false;
		for (int i=0; i < gnOUTPUT_IMAGE_RETRIES; i++)
		{
			// Write the output to a temporary file so that the creation of the
			// redacted image appears as an atomic operation [FlexIDSCore #3547]
			TemporaryFileName tempOutFile(NULL,
				getExtensionFromFullPath(strOutputImageName).c_str(), true);

			// Flag to indicate if any annotations been applied, if so then
			// L_AnnSetTag will need to be called to reset them
			bool bAnnotationsAppliedToPage = false;

			// Declare objects outside of try scope so that they can be released if an exception
			// is thrown
			HBITMAPLIST hFileBitmaps = NULL; // Bitmap list for the input image
			L_UINT nPages = 0; // Number of pages for this image
			HDC hDC = NULL; // Windows DC for drawing functions
			HANNOBJECT hFileContainer = NULL; // Annotation container to hold existing annotations
			HANNOBJECT hContainer = NULL; // Annotation container for redactions
			try
			{
				try
				{
					// Get initialized FILEINFO struct
					FILEINFO fileInfo = GetLeadToolsSizedStruct<FILEINFO>(0);
					int nRet = FAILURE;

					// Convert the PDF input image to a temporary TIF
					PDFInputOutputMgr ltPDF( strImageFileName, true );
					_lastCodePos = "30";

					// Get initialized LOADFILEOPTION struct.
					// IgnoreViewPerspective to avoid a black region at the bottom of the image
					LOADFILEOPTION lfo =
						GetLeadToolsSizedStruct<LOADFILEOPTION>(ELO_IGNOREVIEWPERSPECTIVE);
					_lastCodePos = "40";

					// Load image
					nRet = L_LoadBitmapList((char*)(ltPDF.getFileName().c_str()), &hFileBitmaps, 0, 0, 
						&lfo, &fileInfo );
					if (nRet != SUCCESS)
					{
						UCLIDException uex("ELI09198", "Could not open the image.");
						uex.addDebugInfo("Error Code", nRet);
						uex.addDebugInfo("Error Message", getErrorCodeDescription(nRet));
						uex.addDebugInfo("Original File", ltPDF.getFileNameInformationString());
						uex.addDebugInfo("PDF Manager File", ltPDF.getFileName());
						throw uex;
					}

					// Get Number of pages
					nRet = L_GetBitmapListCount( hFileBitmaps, &nPages );
					throwExceptionIfNotSuccess(nRet, "ELI09199", "Could not obtain page count.");

					// Validate each zone
					long nNumZones = vecZones.size();
					_lastCodePos = "50";
					for ( int k = 0; k < nNumZones; k++ )
					{
						// Validate position
						if ( vecZones[k].m_nStartX == 0 && vecZones[k].m_nStartY == 0 && 
							vecZones[k].m_nEndX== 0 && vecZones[k].m_nEndY == 0 &&
							vecZones[k].m_nPage == 0 )
						{
							UCLIDException ue("ELI09200", "No valid position.");
							throw ue;
						}

						// Validate page number
						if( (L_UINT) vecZones[k].m_nPage > nPages || vecZones[k].m_nPage < 1 )
						{
							UCLIDException ue("ELI09201", "Page number selected does not exist.");
							ue.addDebugInfo("Page", vecZones[k].m_nPage );
							throw ue;
						}
					}
					_lastCodePos = "60";

					// Get initialized SAVEFILEOPTION struct
					SAVEFILEOPTION sfOptions = GetLeadToolsSizedStruct<SAVEFILEOPTION>(0);
					L_GetDefaultSaveFileOption(&sfOptions, sizeof(sfOptions));
					_lastCodePos = "70";

					// Create PDFOutputManager object to handle file conversion
					// If result will be PDF
					// - Each page processed in the loop below will be appended to an internal temporary file
					// - Desired PDF output file will be created from conversion of this temporary TIF
					PDFInputOutputMgr outMgr( tempOutFile.getName(), false );
					_lastCodePos = "80";

					// Create the brush and pen collections
					BrushCollection brushes;
					PenCollection pens;

					// Get the appropriate compression factor for the file type [LRCAU #5248]
					// NOTE: Compression flag is ignored by file formats that do not support
					// compression.  For file types which support this flag and what values
					// are appropriate for those file types look at Leadtools help for the
					// L_SaveBitmap call and click the link about Compression Quality Factors
					L_INT nCompression = getCompressionFactor(fileInfo.Format);

					// Handle pages individually for loading of existing annotations
					// and saving of redactions as new annotations
					_lastCodePos = "90";
					for (unsigned int j = 0; j < nPages; j++)
					{
						// Set the 1-relative page number in the LOADFILEOPTION structure used for loading
						lfo.PageNumber = j + 1;

						// Create Annotation container sized to image extent if applying as annotations
						if (bApplyAsAnnotations || bRetainAnnotations)
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
						_lastCodePos = "100";

						// Load existing annotations, if desired and if file
						// contains annotations. [P16 #3046]
						if (bRetainAnnotations && hasAnnotations(ltPDF.getFileName(), lfo,
							fileInfo.Format))
						{
							// Load any existing annotations on this page
							nRet = L_AnnLoad((char*)ltPDF.getFileName().c_str(), &hFileContainer, &lfo);
							throwExceptionIfNotSuccess(nRet, "ELI14630", 
								"Could not load annotations.", ltPDF.getFileNameInformationString());

							// Check for NULL or empty container
							if (hFileContainer != NULL)
							{
								HANNOBJECT hFirst;
								nRet = L_AnnGetItem(hFileContainer, &hFirst);
								throwExceptionIfNotSuccess(nRet, "ELI14631", 
									"Could not get item from annotation container.");

								if (hFirst != NULL)
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
							// else container is NULL, so nothing to insert
						}
						_lastCodePos = "110";

						// Get the Page to modify
						BITMAPHANDLE hBitmap;
						nRet = L_GetBitmapListItem(hFileBitmaps, j, &hBitmap, sizeof(BITMAPHANDLE));
						throwExceptionIfNotSuccess(nRet, "ELI19358", "Could not obtain page.");

						// Check each zone
						for (vector<PageRasterZone>::const_iterator it = vecZones.begin();
							it != vecZones.end(); it++)
						{
							// Handle this zone if it is on this page
							if (it->m_nPage == j + 1)
							{
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


									///////////////////
									// Calculate points on bounding rectangle
									///////////////////
									POINT p1, p2, p3, p4;
									pageZoneToPoints((*it), p1, p2, p3, p4);

									// Apply bounding RECT to redaction object
									ANNRECT rect;
									rect.top = min(p1.y, min(p2.y, min(p3.y, p4.y)));
									rect.left = min(p1.x, min(p2.x, min(p3.x, p4.x)));
									rect.bottom = max(p1.y, max(p2.y, max(p3.y, p4.y)));
									rect.right = max(p1.x, max(p2.x, max(p3.x, p4.x)));
									nRet = L_AnnSetRect(hRedaction, &rect);
									throwExceptionIfNotSuccess(nRet, "ELI14609", 
										"Could not bound redaction annotation object." );

									// Insert the redaction object into the container
									nRet = L_AnnInsert(hContainer, hRedaction, FALSE);
									throwExceptionIfNotSuccess(nRet, "ELI14610", 
										"Could not insert redaction annotation object.");
									bAnnotationsAppliedToPage = true;

									// Check if any text was specified
									if (it->m_strText.size() > 0)
									{
										// Create a device context (needed to ensure font size fits)
										if (hDC == NULL)
										{
											hDC = L_CreateLeadDC(&hBitmap);
											if (hDC == NULL)
											{
												UCLIDException uex("ELI24891",
													"Unable to create device context.");
												uex.addDebugInfo("Image file name",
													ltPDF.getFileNameInformationString());
												uex.addDebugInfo("Page number", j+1);
												throw uex;
											}
										}

										int iFontSize = 
											getFontSizeThatFits(hDC, *it, fileInfo.YResolution);

										// Get the current annotation options
										L_UINT uOptions = 0;
										nRet = L_AnnGetOptions(&uOptions);
										throwExceptionIfNotSuccess(nRet, "ELI24470",
											"Could not get annotation options.");

										// Ensure text options are available
										uOptions |= OPTIONS_NEW_TEXT_OPTIONS;
										nRet = L_AnnSetOptions(NULL, uOptions);
										throwExceptionIfNotSuccess(nRet, "ELI24471",
											"Could not set text annotation options.");

										// Creat a text annotation object
										HANNOBJECT hText;
										nRet = L_AnnCreate(ANNOBJECT_TEXT, &hText);
										throwExceptionIfNotSuccess(nRet, "ELI24465", 
											"Could not create text annotation object.");

										// Make text object visible
										nRet = L_AnnSetVisible(hText, TRUE, 0, NULL);
										throwExceptionIfNotSuccess(nRet, "ELI24467", 
											"Could not set visibility for redaction annotation object.");

										// Set the font size
										nRet = L_AnnSetFontSize(hText, iFontSize, 0);
										throwExceptionIfNotSuccess(nRet, "ELI24472",
											"Could not set font size.");

										// Set the font name
										nRet = L_AnnSetFontName(hText, (char*)it->m_font.lfFaceName, 0);
										throwExceptionIfNotSuccess(nRet, "ELI24473",
											"Could not set font name.");

										// Set text color
										ANNTEXTOPTIONS textOptions = 
											GetLeadToolsSizedStruct<ANNTEXTOPTIONS>(0);
										textOptions.bShowText = TRUE;
										textOptions.bShowBorder = FALSE;
										textOptions.crText = it->m_crTextColor;
										textOptions.uFlags = ANNTEXT_ALL;
										nRet = L_AnnSetTextOptions(hText, &textOptions, 0);
										throwExceptionIfNotSuccess(nRet, "ELI24474",
											"Could not set font name.");

										// Set the tiff tag
										nRet = L_AnnSetTag(hText, ANNTAG_TIFF, 0);
										throwExceptionIfNotSuccess(nRet, "ELI24468", 
											"Could not set annotation tag.");

										// Set the spatial boundaries for the text annotation
										nRet = L_AnnSetRect(hText, &rect);
										throwExceptionIfNotSuccess(nRet, "ELI24469", 
											"Could not bound text annotation object.");

										// Set the text
										nRet = L_AnnSetText(hText, (char*)it->m_strText.c_str(), 0);
										throwExceptionIfNotSuccess(nRet, "ELI24475",
											"Could not set text.");

										// Insert the text object into the container
										nRet = L_AnnInsert(hContainer, hText, FALSE);
										throwExceptionIfNotSuccess(nRet, "ELI24466", 
											"Could not insert text annotation object." );
									}
								}
								else
								{
									if (hDC == NULL)
									{
										hDC = L_CreateLeadDC( &hBitmap );
										if (hDC == NULL)
										{
											UCLIDException uex("ELI23576",
												"Unable to create device context!");
											uex.addDebugInfo("Image File Name",
												ltPDF.getFileNameInformationString());
											uex.addDebugInfo("Page number", j+1);
											throw uex;
										}
									}

									// Set the appropriate brush and pen
									SelectObject(hDC, brushes.getColoredBrush(it->m_crFillColor));
									SelectObject(hDC, pens.getColoredPen(it->m_crBorderColor));

									// Convert the Zone to rectangle corner points
									POINT aPoints[4];
									pageZoneToPoints( *it, aPoints[0], aPoints[1],
										aPoints[2], aPoints[3]);

									// Draw the Polygon
									Polygon(hDC, (POINT *) &aPoints, 4);

									// If there is text to add, add it
									if ( it->m_strText.size() > 0 )
									{
										addTextToImage(hDC, *it, fileInfo.YResolution);
									}
								}
							}	// end if this zone is on this page
						}		// end for each zone
						_lastCodePos = "130";

						// Delete the Device Context
						if (hDC != NULL)
						{
							L_DeleteLeadDC( hDC );
							hDC = NULL;

							// If the image has changed, set the bitmap list item 
							// [LegacyRCAndUtils #5299]
							if (!bApplyAsAnnotations)
							{
								L_SetBitmapListItem(hFileBitmaps, j, &hBitmap);
							}
						}
						_lastCodePos = "140";

						// Save the collected redaction annotations
						//   Save in WANG-mode for greatest compatibility
						//   The next call to SaveBitmap will include these annotations
						if (bAnnotationsAppliedToPage)
						{
							// Set annotations added to document flag [FlexIDSCore #3131]
							bAnnotationsAppliedToDocument = true;

							nRet = L_AnnSaveTag(hContainer, ANNFMT_WANGTAG, FALSE );
							throwExceptionIfNotSuccess(nRet, "ELI14611", 
								"Could not save redaction annotation objects.");
						}
						_lastCodePos = "150";

						// Set the page number for save options
						sfOptions.PageNumber = j + 1;

						// Save this page of the original file
						int nNumFailedAttempts = 0;
						while (nNumFailedAttempts < iRetryCount)
						{
							nRet = L_SaveBitmap((char*)(outMgr.getFileName().c_str()), &hBitmap, 
								fileInfo.Format, fileInfo.BitsPerPixel, nCompression, &sfOptions);

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
								Sleep( iRetryTimeout);
							}
						}
						if (nRet != SUCCESS)
						{
							UCLIDException ue("ELI09202", "Could not save image.");
							ue.addDebugInfo("Output Image", strOutputImageName);
							ue.addDebugInfo("Temporary Image", tempOutFile.getName());
							ue.addDebugInfo("Output Manager File", outMgr.getFileName());
							ue.addDebugInfo("Actual Page", j + 1);
							ue.addDebugInfo("Error description", getErrorCodeDescription(nRet));
							ue.addDebugInfo("Actual Error Code", nRet);
							ue.addDebugInfo("Retries attempted", nNumFailedAttempts);
							ue.addDebugInfo("Max Retries", iRetryCount);
							ue.addDebugInfo("Compression Factor", nCompression);
							addFormatDebugInfo(ue, fileInfo.Format);
							throw ue;
						}
						else
						{
							if (nNumFailedAttempts > 0)
							{
								UCLIDException ue("ELI20366",
									"Application Trace:Saved image page successfully after retry.");
								ue.addDebugInfo("Retries", nNumFailedAttempts);
								ue.addDebugInfo("Temporary Image", tempOutFile.getName());
								ue.addDebugInfo("Output Image", strOutputImageName);
								ue.addDebugInfo("Page", j+1);
								ue.log();
							}
						}
						_lastCodePos = "160";

						if (bAnnotationsAppliedToPage)
						{
							// Clear any previously defined annotations
							// If not done, any annotations applied to this page may be applied to 
							// successive pages [FlexIDSCore #2216]
							nRet = L_SetTag(ANNTAG_TIFF, 0, 0, NULL);

							// Reset loaded annotation flag
							bAnnotationsAppliedToPage = false;
						}

						// Destroy the annotation container
						if (hContainer != NULL)
						{
							nRet = L_AnnDestroy(hContainer, ANNFLAG_RECURSE);
							throwExceptionIfNotSuccess(nRet, "ELI15361",
								"Could not destroy annotation container.");
							hContainer = NULL;
						}
						_lastCodePos = "170";
					}	// end for each page

					L_DestroyBitmapList(hFileBitmaps);
					hFileBitmaps = NULL;

					// Wait for the file to be readable before continuing
					waitForFileToBeReadable(outMgr.getFileName());
				}
				CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI23568");
			}
			catch(UCLIDException& uex)
			{
				uex.addDebugInfo("Input Image File", strImageFileName);
				uex.addDebugInfo("Output Image File", strOutputImageName);

				if (bAnnotationsAppliedToPage)
				{
					// Clear any previously defined annotations
					// If not done, any annotations applied to this page may be applied to 
					// successive pages [FlexIDSCore #2216]
					L_SetTag(ANNTAG_TIFF, 0, 0, NULL);
				}
				if (hFileContainer != NULL)
				{
					try
					{
						// Destroy the annotation container
						throwExceptionIfNotSuccess(L_AnnDestroy(hFileContainer, ANNFLAG_RECURSE), 
							"ELI23567",	"Unable to destroy annotation container.");
					}
					catch(UCLIDException& ex)
					{
						ex.log();
					}
					hFileContainer = NULL;
				}

				if (hDC != NULL)
				{
					L_DeleteLeadDC( hDC );
					hDC = NULL;
				}
				if (hFileBitmaps != NULL)
				{
					L_DestroyBitmapList(hFileBitmaps);
					hFileBitmaps = NULL;
				}
				// Destroy the annotation container
				if (hContainer != NULL)
				{
					L_AnnDestroy(hContainer, ANNFLAG_RECURSE);
					hContainer = NULL;
				}

				throw uex;
			}
			_lastCodePos = "180";

			// check the number of pages in the output
			int nNumberOfPagesInOutput = getNumberOfPagesInImage(tempOutFile.getName());
			_lastCodePos = "190";

			// if the page numbers don't match log an exception and retry
			if (nPages != nNumberOfPagesInOutput)
			{
				UCLIDException ue("ELI23562", "Application Trace: Output page count mismatch.");
				ue.addDebugInfo("Attempt", i+1);
				ue.addDebugInfo("Source Pages", nPages);
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
		_lastCodePos = "200";

		// failed after retrying, throw a failure exception
		if (!bSuccessful)
		{
			UCLIDException ue("ELI23563", "Failed to properly write the output image!");
			ue.addDebugInfo("Source Image", strImageFileName);
			ue.addDebugInfo("Output Image", strOutputImageName);
			throw ue;
		}
		else
		{
			if(bAnnotationsAppliedToDocument && isPDFFile(strOutputImageName))
			{
				// Log application trace if annotations added to the document and
				// output is a PDF [FlexIDSCore #3131 - JDS - 12/18/2008] 
				UCLIDException uex("ELI23594",
					"Application trace: Applied or retained annotations on a PDF.");
				uex.addDebugInfo("Input Image File", strImageFileName);
				uex.addDebugInfo("Output Image File", strOutputImageName);
				uex.log();
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
			UCLIDException ue("ELI12853", "File already exists!");
			ue.addDebugInfo("ImageName", strOutputFileName);
			throw ue;
		}
	}

	// Handle 0 images (error condition)
	int nNumImages = vecImageFiles.size();
	if (nNumImages == 0)
	{
		UCLIDException ue("ELI12855", "Vector containing sub-image filenames is empty!");
		ue.addDebugInfo("NumOfImages", nNumImages);
		ue.addDebugInfo("OutputImage", strOutputFileName);
		throw ue;
	}
	// Handle single-page case
	else if (nNumImages == 1)
	{
		// for single page images, just copy the old image into the new name
		copyFile(vecImageFiles[0], strOutputFileName);
	}
	// Handle multi-page case

	HBITMAPLIST	hBmpList = NULL; // create a bitmap list for the multi-page image
	try
	{
		try
		{
			// Create PDFOutputManager object to handle file conversion
			// If result will be PDF
			// - Each page processed in the vector will be appended to an internal temporary file
			// - Desired PDF output file will be created from conversion of this temporary TIF
			PDFInputOutputMgr outMgr( strOutputFileName, false );

			L_INT nRet;
			nRet = L_CreateBitmapList(&hBmpList);
			throwExceptionIfNotSuccess(nRet, "ELI09042", "Unable to create bitmap list!");

			// for each page that exists for this image, if an image file
			// exists with the corresponding name, then load it and add it
			// to the multi-page image

			// Get initialized FILEINFO struct
			FILEINFO fileInfo = GetLeadToolsSizedStruct<FILEINFO>(0);

			// Loop through each image page
			for (int i = 0; i < nNumImages; i++ )
			{
				// Retrieve this filename and update page number
				string strPage = vecImageFiles[i];

				// if the image page file exists, load it and add it to the
				// bitmap list.
				if (isFileOrFolderValid(strPage))
				{
					// Temporary holder for a bitmap
					BITMAPHANDLE hTmpBmp;

					// Convert the PDF input image to a temporary TIF
					PDFInputOutputMgr ltPDF( strPage, true );

					// Set flags to get file information when loading bitmap
					fileInfo.Flags = 0;
					nRet = L_LoadBitmap( (char*)(ltPDF.getFileName().c_str()), &hTmpBmp,
						sizeof(BITMAPHANDLE), 0, ORDER_RGB, NULL, &fileInfo);
					throwExceptionIfNotSuccess(nRet, "ELI09044", "Unable to load bitmap!", strPage);

					try
					{
						// Add this page to the list
						nRet = L_InsertBitmapListItem(hBmpList, -1, &hTmpBmp);
						throwExceptionIfNotSuccess(nRet, "ELI09045",
							"Unable to insert page in image!");
					}
					catch(UCLIDException& uex)
					{
						if (hTmpBmp.Flags.Allocated)
						{
							L_FreeBitmap(&hTmpBmp);
						}

						throw uex;
					}
				}
				else
				{
					UCLIDException ue("ELI12851", "Unable to locate page image!");
					ue.addDebugInfo("Filename", strPage);
					ue.addDebugInfo("PageNumber", i + 1);
					throw ue; 
				}
			}

			// Get the appropriate compression factor for the specified format [LRCAU #5284]
			L_INT nCompression = getCompressionFactor(fileInfo.Format);

			// save the bitmap list as a multi-page tif image using the format of the
			// last page of the image 
			nRet = L_SaveBitmapList( (char*)(outMgr.getFileName().c_str()), hBmpList, 
				fileInfo.Format, fileInfo.BitsPerPixel, nCompression, NULL);
			if (nRet != SUCCESS)
			{
				UCLIDException ue("ELI09046", "Unable to save multi-page image!");
				ue.addDebugInfo("Original File Name", strOutputFileName);
				ue.addDebugInfo("PDF Manager Name", outMgr.getFileName());
				ue.addDebugInfo("Actual Error Code", nRet);
				ue.addDebugInfo("Error String", getErrorCodeDescription(nRet));
				ue.addDebugInfo("Compression Flag", nCompression);
				addFormatDebugInfo(ue, fileInfo.Format);
				throw ue;
			}

			// release the memory associated with bitmap list
			L_DestroyBitmapList(hBmpList);
			hBmpList = NULL;
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI23569");
	}
	catch(UCLIDException& uex)
	{
		if (hBmpList != NULL)
		{
			L_DestroyBitmapList(hBmpList);
			hBmpList = NULL;
		}

		throw uex;
	}

	// Make sure the file can be read
	waitForFileToBeReadable(strOutputFileName);
}
//-------------------------------------------------------------------------------------------------
int getNumberOfPagesInImage( const string& strImageFileName )
{
	int nNumFailedAttempts = 0;

	try
	{
		// Get initialized FILEINFO struct
		FILEINFO fileInfo = GetLeadToolsSizedStruct<FILEINFO>(0);

		// Get Number of Pages
		int nRet = FAILURE;
		while (nNumFailedAttempts < gnNUMBER_ATTEMPTS_BEFORE_FAIL)
		{
			// Leadtools pdf support is not thread safe. Lock access if this is a PDF.
			{
				LeadToolsPDFLoadLocker ltLocker(strImageFileName);

				nRet = L_FileInfo( (char*)( strImageFileName.c_str() ), &fileInfo, 
				sizeof(FILEINFO), FILEINFO_TOTALPAGES, NULL);
			}

			// Check result
			if (nRet == SUCCESS)
			{
				if (nNumFailedAttempts != 0)
				{
					UCLIDException ue("ELI20365",
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
				Sleep( gnSLEEP_BETWEEN_RETRY_MS );
			}
		}

		// Throw exception if all retries failed
		throwExceptionIfNotSuccess(nRet, "ELI09166", "Could not obtain image info.",
			strImageFileName);

		// Return actual page count
		return fileInfo.TotalPages;
	}
	catch (UCLIDException& ue)
	{
		UCLIDException uexOuter("ELI15314", "Unable to determine image page count!", ue);
		uexOuter.addDebugInfo("Retries attempted", nNumFailedAttempts);
		throw uexOuter;
	}
}
//-------------------------------------------------------------------------------------------------
void getImageXAndYResolution(const string& strImageFileName, int& riXResolution, 
							 int& riYResolution)
{
	// Get initialized FILEINFO struct
	FILEINFO fileInfo = GetLeadToolsSizedStruct<FILEINFO>(0);

	// Convert a PDF input image to a temporary TIF
	PDFInputOutputMgr ltPDF( strImageFileName, true );

	LeadToolsPDFLoadLocker ltLocker(false);

	// Get File Information
	throwExceptionIfNotSuccess(L_FileInfo( (char*)( ltPDF.getFileName().c_str() ), &fileInfo, 
		sizeof(FILEINFO), FILEINFO_TOTALPAGES, NULL ), 
		"ELI20250", "Could not obtain image info!", ltPDF.getFileNameInformationString());

	riXResolution = fileInfo.XResolution;
	riYResolution = fileInfo.YResolution;
}
//-------------------------------------------------------------------------------------------------
void getImagePixelHeightAndWidth(const string& strImageFileName, int& riHeight, int& riWidth,
								 int nPageNum)
{
	// Get initialized FILEINFO struct
	FILEINFO fileInfo = GetLeadToolsSizedStruct<FILEINFO>(0);

	// Convert a PDF input image to a temporary TIF
	PDFInputOutputMgr ltPDF( strImageFileName, true );

	// Get initialized LOADFILEOPTION struct. 
	LOADFILEOPTION lfo = GetLeadToolsSizedStruct<LOADFILEOPTION>(0);
	lfo.PageNumber = nPageNum;

	LeadToolsPDFLoadLocker ltLocker(false);

	// Get File Information
	throwExceptionIfNotSuccess(L_FileInfo( (char*)( ltPDF.getFileName().c_str() ), &fileInfo, 
		sizeof(FILEINFO), FILEINFO_TOTALPAGES, &lfo), 
		"ELI20247", "Could not obtain image info!", ltPDF.getFileNameInformationString());

	riHeight = fileInfo.Height;
	riWidth = fileInfo.Width;
}
//-------------------------------------------------------------------------------------------------
void initPDFSupport(int nDisplayDepth, int iOpenXRes, int iOpenYRes )
{
	// check if PDF is licensed to initialize support
	if ( !LicenseManagement::sGetInstance().isPDFLicensed() )
	{
		// pdf support is not licensed
		return;
	}
	else
	{
		// Provide multi-thread protection for PDF images
		LeadToolsPDFLoadLocker ltPDF( true );

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
		// Provide multi-thread protection for PDF images
		LeadToolsPDFLoadLocker ltPDF( true );

		// Retrieve default load options
		L_GetPDFOptions( &pdfOptions, sizeof(pdfOptions) );

		// Set the PDF initialization directory [LRCAU #5102]
		string strTempDir = getPDFInitializationDirectory();
		try
		{
			throwExceptionIfNotSuccess(L_SetPDFInitDir((char*)(strTempDir.c_str())),
				"ELI24246", "Application Trace: Unable to set initial PDF directory");
		}
		catch(UCLIDException& uex)
		{
			uex.addDebugInfo("PDF Directory", strTempDir);
			uex.log();
		}

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
int getImageViewPerspective(const string strImageFileName, int nPageNum)
{
	// Treat PDF images as having TOP_LEFT perspective
	if (isPDFFile(strImageFileName))
	{
		return TOP_LEFT;
	}

	// Get initialized FILEINFO struct
	FILEINFO fileInfo = GetLeadToolsSizedStruct<FILEINFO>(0);

	// Get initialized LOADFILEOPTION struct. 
	LOADFILEOPTION lfo = GetLeadToolsSizedStruct<LOADFILEOPTION>(0);

	// Set page number
	lfo.PageNumber = nPageNum;

	// Convert a PDF input image to a temporary TIF
	PDFInputOutputMgr ltPDF( strImageFileName, true );

	LeadToolsPDFLoadLocker ltLocker(false);

	// Get File Information
	int nRet = L_FileInfo( (char*)( ltPDF.getFileName().c_str() ), &fileInfo, 
		sizeof(FILEINFO), 0, &lfo );

	throwExceptionIfNotSuccess(nRet, "ELI16655", 
		"Could not obtain image info.", ltPDF.getFileNameInformationString() );

	// Return ViewPerspective field
	return fileInfo.ViewPerspective;
}
//-------------------------------------------------------------------------------------------------
void unlockDocumentSupport()
{
	// Unlock support for Document toolkit for annotations
	if (LicenseManagement::sGetInstance().isAnnotationLicensed())
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
		UCLIDException ue( "ELI16799", "Document toolkit support is not licensed!" );
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
		rpm.createKey( gstrLEADTOOLS_SERIALIZATION_PATH, gstrSERIALIZATION_KEY, asString( 0 ) );
		return false;
	}

	return rpm.getKeyValue( gstrLEADTOOLS_SERIALIZATION_PATH, gstrSERIALIZATION_KEY ) == "1" 
		? true : false;
}
//-------------------------------------------------------------------------------------------------
void convertTIFToPDF(const std::string& strTIF, const std::string& strPDF, bool bRetainAnnotations)
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
void convertPDFToTIF(const std::string& strPDF, const std::string& strTIF)
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
			runExtractEXE( strEXEPath, strArguments, INFINITE );
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
	
	// calculate the 4 points
	p1.x = rZone.m_nStartX - (long) ((rZone.m_nHeight/2) * sin (dAngle));
	p1.y = rZone.m_nStartY + (long) ((rZone.m_nHeight/2) * cos (dAngle));
	
	p2.x = rZone.m_nEndX - (long) ((rZone.m_nHeight/2) * sin (dAngle));
	p2.y = rZone.m_nEndY + (long) ((rZone.m_nHeight/2) * cos (dAngle));
	
	p3.x = rZone.m_nEndX + (long) ((rZone.m_nHeight/2) * sin (dAngle));
	p3.y = rZone.m_nEndY - (long) ((rZone.m_nHeight/2) * cos (dAngle));
	
	p4.x = rZone.m_nStartX + (long) ((rZone.m_nHeight/2) * sin (dAngle));
	p4.y = rZone.m_nStartY - (long) ((rZone.m_nHeight/2) * cos (dAngle));
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
bool hasAnnotations(string strFilename, LOADFILEOPTION &lfo, int iFileFormat)
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
		throwExceptionIfNotSuccess(iRet, "ELI20788", "Could not load annotations from tiff tag.");
	}

	// return true if there is at least one annotation object
	return uCount > 0;
}
//-------------------------------------------------------------------------------------------------
bool hasAnnotations(string strFilename, int iPageNumber)
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
		if (hFont != NULL)
		{
			DeleteObject(hFont);
			hFont = NULL;
		}

		throw;
	}
}
//-------------------------------------------------------------------------------------------------
void loadImagePage(const string strImageFileName, unsigned long ulPage, BITMAPHANDLE &rBitmap)
{
	L_INT nRet;

	// Get initialized LOADFILEOPTION struct. 
	// IgnoreViewPerspective to avoid a black region at the bottom of the image
	LOADFILEOPTION LoadOptions = GetLeadToolsSizedStruct<LOADFILEOPTION>(ELO_IGNOREVIEWPERSPECTIVE);

	// get the default load options
	nRet = L_GetDefaultLoadFileOption(&LoadOptions, sizeof(LOADFILEOPTION));
	throwExceptionIfNotSuccess(nRet, "ELI13283", 
		"Unable to get default file load options for LeadTools imaging library.");

	// Convert a PDF input image to a temporary TIF
	PDFInputOutputMgr ltPDF( strImageFileName, true );

	// Get initialized FILEINFO struct
	FILEINFO fileInfo = GetLeadToolsSizedStruct<FILEINFO>(0);

	LoadOptions.PageNumber = ulPage;
	nRet = L_LoadBitmap( (char*)( ltPDF.getFileName().c_str()), 
		&rBitmap, sizeof(BITMAPHANDLE), 0, ORDER_RGB, &LoadOptions, &fileInfo);
	if (nRet != SUCCESS)
	{
		UCLIDException ue("ELI13284", "Unable to get load image file/page.");
		ue.addDebugInfo("Error code", nRet);
		ue.addDebugInfo("Filename", strImageFileName);
		ue.addDebugInfo("PageNumber", ulPage);
		throw ue;
	}

	// if the bitmap was loaded successfully, check the perspective to make
	// sure that (0,0) is at the top left corner
	if (rBitmap.ViewPerspective != TOP_LEFT)
	{
		nRet = L_ChangeBitmapViewPerspective(NULL, &rBitmap, sizeof(BITMAPHANDLE), TOP_LEFT);
		throwExceptionIfNotSuccess(nRet, "ELI14634", "Unable to change bitmap perspective.");
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
string getPDFInitializationDirectory()
{
#ifdef DEBUG
		string strTemp = getLeadUtilsDirectory()
			+ "..\\..\\ReusableComponents\\APIs\\LeadTools_16\\PDF";
#else
		string strTemp = getLeadUtilsDirectory() + "pdf";
#endif
		simplifyPathName(strTemp);
		return strTemp;
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
		if (piFontSize != NULL)
		{
			*piFontSize = -lf.lfHeight;
		}
	}
	catch(...)
	{
		if (hFont != NULL)
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

	try
	{
		// Default value to 0 (this flag will work for most compression values although
		// the files will be very large).
		int nReturn = 0;

		// Get the string value for the format
		string strFormat = getStringFromFormat(nFormat);

		// Look for the value in the map
		map<L_INT, int>::iterator it = smapFormatToCompressionFactor.find(nFormat);
		if (it != smapFormatToCompressionFactor.end())
		{
			// Get the value from the map
			nReturn = it->second;
		}
		else
		{
			// Get a registry persistance manager to search for a compression value
			// for this file type
			RegistryPersistenceMgr rpm(HKEY_LOCAL_MACHINE, gstrRC_REG_PATH);

			// Check for registry key
			if (rpm.keyExists(gstrLEADTOOLS_COMPRESSION_VALUE_FOLDER, strFormat))
			{
				// Get the value from the registry
				nReturn = (int)
					asLong(rpm.getKeyValue(gstrLEADTOOLS_COMPRESSION_VALUE_FOLDER, strFormat));
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

		// Return the compression factor
		return nReturn;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25419");
}
//-------------------------------------------------------------------------------------------------
