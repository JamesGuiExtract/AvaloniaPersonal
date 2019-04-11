//==================================================================================================
//
// COPYRIGHT (c) 2008 EXTRACT SYSTEMS, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	PasteImage.cpp
//
// PURPOSE:	Implementation of the PasteImage functionality.
//
// AUTHORS:	Jeff Shergalis
//
//==================================================================================================

#include "stdafx.h"
#include "PasteImage.h"
#include "MiscLeadUtils.h"
#include "LeadToolsBitmapFreeer.h"
#include "LeadToolsFormatHelpers.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <LicenseMgmt.h>
#include <Misc.h>
#include "LeadToolsLicenseRestrictor.h"

//--------------------------------------------------------------------------------------------------
// Exported DLL functions
//--------------------------------------------------------------------------------------------------
void pasteImageAtLocation(const string& strInputImage, const string& strOutputImage,
						  const string& strPasteImage, double dHorizPercent, 
						  double dVertPercent, const string& strPagesToStamp)
{
	ASSERT_ARGUMENT("ELI20174", dHorizPercent >= 0.0 && dHorizPercent <= 100.0);
	ASSERT_ARGUMENT("ELI20175", dVertPercent >= 0.0 && dVertPercent <= 100.0);

	INIT_EXCEPTION_AND_TRACING("MLI00025");

	try
	{
		try
		{
			// verify that LeadTools is licensed 
			unlockDocumentSupport();
			_lastCodePos = "10";

			// Create a temporary file for writing [LRCAU #5408]
			TemporaryFileName tmpOutFile(true, "", getExtensionFromFullPath(strOutputImage).c_str());

			// Needed outside PDF manager scope
			long nPageCount = 0;

			// Wait for the input file to be readable
			waitForFileToBeReadable(strInputImage, true);

			// Get the file info for the input image
			FILEINFO fileInfo;
			getFileInformation(strInputImage, true, fileInfo);

			// Get the page count and format information
			nPageCount = fileInfo.TotalPages;
			int iFormat = fileInfo.Format;

			// Get the vector of page numbers to stamp
			validatePageNumbers(strPagesToStamp);
			set<int> setPages = getPageNumbersAsSet(nPageCount, strPagesToStamp, true);

			// create a load files option
			LOADFILEOPTION lfo = GetLeadToolsSizedStruct<LOADFILEOPTION>(
				ELO_IGNOREVIEWPERSPECTIVE | ELO_ROTATED);

			// Create the save options
			SAVEFILEOPTION sfo = GetLeadToolsSizedStruct<SAVEFILEOPTION>(0);

			// Get the default save options
			{
				LeadToolsLicenseRestrictor leadToolsLicenseGuard;
				throwExceptionIfNotSuccess(L_GetDefaultSaveFileOption(&sfo, sizeof(SAVEFILEOPTION)),
					"ELI20071", "Failed getting default save options!");
			}

			// Loop through each page of the image
			_lastCodePos = "40";
			for (long i = 1; i <= nPageCount; i++)
			{
				string strPageCount = asString(i);

				// Set FILEINFO_FORMATVALID (this will speed up the L_LoadBitmap calls)
				fileInfo = GetLeadToolsSizedStruct<FILEINFO>(FILEINFO_FORMATVALID);
				fileInfo.Format = iFormat;

				// Set the 1-relative page number in the LOADFILEOPTION structure 
				lfo.PageNumber = i;

				// Declare the bitmaphandle and set the freer class to release it
				BITMAPHANDLE hBitmap = {0};
				LeadToolsBitmapFreeer inFreer(hBitmap);
				_lastCodePos = "40_A_Page# " + strPageCount;

				// Load this image page
				loadImagePage(strInputImage, hBitmap, fileInfo, lfo, false);

				// Check for stamp page
				if (setPages.find(i) != setPages.end())
				{
					// Declare the bitmaphandle and set the freer class to release it
					BITMAPHANDLE hPasteBitmap;
					LeadToolsBitmapFreeer freer(hPasteBitmap, true);
					_lastCodePos = "40_A_10_Page# " + strPageCount;

					// Get a file info for the stamp
					FILEINFO flPasteInfo = GetLeadToolsSizedStruct<FILEINFO>(0);

					// load the stamp image
					loadStampBitmap(strPasteImage, fileInfo, flPasteInfo, hPasteBitmap);
					_lastCodePos = "40_A_20_Page# " + strPageCount;

					// Place the stamp
					placeStamp(hPasteBitmap, hBitmap, dHorizPercent, dVertPercent);
				}
				_lastCodePos = "40_B_Page# " + strPageCount;

				// Save this page
				sfo.PageNumber = i;
				saveImagePage(hBitmap, tmpOutFile.getName(), fileInfo, sfo);
				_lastCodePos = "40_C_Page# " + strPageCount;
			}
			_lastCodePos = "50";

			// Ensure that the temporary file has the same number of pages as the original
			long nTmpCount = getNumberOfPagesInImage(tmpOutFile.getName());
			if (nTmpCount != nPageCount)
			{
				UCLIDException ue("ELI27290", "Page count mismatch!");
				ue.addDebugInfo("Temporary File", tmpOutFile.getName());
				ue.addDebugInfo("Original Page Count", nPageCount);
				ue.addDebugInfo("Temporary Page Count", nTmpCount);
				throw ue;
			}
			_lastCodePos = "60";

			// Move the temporary file to the destination file
			copyFile(tmpOutFile.getName(), strOutputImage, false, true);
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20168");
	}
	catch(UCLIDException& ue)
	{
		// Add additional debug info
		ue.addDebugInfo("Input Image", strInputImage);
		ue.addDebugInfo("Destination Image", strOutputImage);
		ue.addDebugInfo("Image To Paste", strPasteImage);

		throw ue;
	}
}

//--------------------------------------------------------------------------------------------------
// Non-exported local helper functions
//--------------------------------------------------------------------------------------------------
void loadStampBitmap(const string& strPasteImage, const FILEINFO& flInInfo,
					 FILEINFO& rflPasteInfo, BITMAPHANDLE& rhPasteBitmap)
{
	try
	{
		// check to be sure the BITMAPHANDLE has not been allocated yet
		ASSERT_ARGUMENT("ELI20061", !(rhPasteBitmap.Flags.Allocated));

		// create a load files option
		LOADFILEOPTION lfo = GetLeadToolsSizedStruct<LOADFILEOPTION>(
			ELO_IGNOREVIEWPERSPECTIVE | ELO_ROTATED);

		// get the file info for the stamp image
		getFileInformation(strPasteImage, true, rflPasteInfo);

		// ensure that the stamp image contains only 1 page
		if (rflPasteInfo.TotalPages > 1)
		{
			UCLIDException ue("ELI20081", "Watermark image cannot be multi-page!");
			ue.addDebugInfo("File name", strPasteImage);
			ue.addDebugInfo("Number of pages", rflPasteInfo.TotalPages);
			throw ue;
		}

		// if BitsPerPixel are not the same, convert the stamp image to the source BitsPerPixel
		LeadToolsLicenseRestrictor leadToolsLicenseGuard;
		if (rflPasteInfo.BitsPerPixel != flInInfo.BitsPerPixel)
		{
			// load the paste image, set the pixel depth to match the source image
			throwExceptionIfNotSuccess(L_LoadBitmapResize((char*) strPasteImage.c_str(),
				&rhPasteBitmap, sizeof(rhPasteBitmap), rflPasteInfo.Width, rflPasteInfo.Height, 
				flInInfo.BitsPerPixel, SIZE_NORMAL, flInInfo.Order, &lfo, &rflPasteInfo),
				"ELI20063", "Could not open and resize the watermark image!", 
				strPasteImage);
		}
		else
		{
			// load the paste image
			throwExceptionIfNotSuccess(L_LoadBitmap((char*) strPasteImage.c_str(), 
				&rhPasteBitmap, sizeof(rhPasteBitmap), 0, 0,
				&lfo, &rflPasteInfo), "ELI20064", "Could not open the watermark image",
				strPasteImage);
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27278");
}
//--------------------------------------------------------------------------------------------------
long getXCoordinate(double dHorizPercent, L_INT liWidth)
{
	double dXCoordinate = (dHorizPercent / 100.0) * ((double) liWidth);
	return (long) dXCoordinate;
}
//--------------------------------------------------------------------------------------------------
long getYCoordinate(double dVertPercent, L_INT liHeight)
{
	double dYCoordinate = (dVertPercent / 100.0) * ((double) liHeight);
	return (long) dYCoordinate;
}
//--------------------------------------------------------------------------------------------------
void validateStampLocation(long lX, long lY, long lDestWidth, long lDestHeight, 
						   long lStampWidth, long lStampHeight)
{
	// check if after paste the pasted piece would be outside source image dimensions
	if ((lX + lStampWidth) > lDestWidth 
		|| (lY + lStampHeight) > lDestHeight)
	{
		UCLIDException ue("ELI20065", "Invalid coordinates for stamping image!");
		ue.addDebugInfo("X coordinate", lX);
		ue.addDebugInfo("Y coordinate", lY);
		ue.addDebugInfo("Stamp Width", lStampWidth);
		ue.addDebugInfo("Stamp Height", lStampHeight);
		ue.addDebugInfo("Destination Width", lDestWidth);
		ue.addDebugInfo("Destination Height", lDestHeight);
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
void validateStampImageSize(long lDestWidth, long lDestHeight, long lStampWidth, long lStampHeight)
{
	// check to be sure the paste image is smaller than the source image
	if (lStampWidth > lDestWidth || lStampHeight > lDestHeight)
	{
		UCLIDException ue("ELI20066", "Stamp image is too large!");
		ue.addDebugInfo("Destination Width", lDestWidth);
		ue.addDebugInfo("Destination Height", lDestHeight);
		ue.addDebugInfo("Stamp Width", lStampWidth);
		ue.addDebugInfo("Stamp Height", lStampHeight);
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
void placeStamp(BITMAPHANDLE& hStampBmp, BITMAPHANDLE& hDestBmp, double dHorizPercent,
				double dVertPercent)
{
	try
	{
		// Get the width and height of the stamp and the destination
		long lStampWidth = hStampBmp.Width;
		long lStampHeight = hStampBmp.Height;
		long lDestWidth = hDestBmp.Width;
		long lDestHeight = hDestBmp.Height;

		// validate the size of the stamp image
		validateStampImageSize(lDestWidth, lDestHeight, lStampWidth, lStampHeight);

		// get the X and Y coordinates based on the percentage
		long lX = getXCoordinate(dHorizPercent, lDestWidth);
		long lY = getYCoordinate(dVertPercent, lDestHeight);

		// validate the coordinates
		validateStampLocation(lX, lY, lDestWidth, lDestHeight, lStampWidth, lStampHeight);

		// ensure that both the stamp image and the source image have the same
		// color palette [p13 #4789]
		matchPalette(hDestBmp, hStampBmp);

		// Stamp the image on the page
		LeadToolsLicenseRestrictor leadToolsLicenseGuard;
		L_INT nRet = L_CombineBitmap(&hDestBmp, lX, lY, lStampWidth, lStampHeight,
			&hStampBmp, 0, 0, CB_DST_0 | CB_OP_OR, 0);
		if (nRet != SUCCESS)
		{
			UCLIDException ue("ELI20069", "Failed to paste the stamp image to the bitmap!");
			ue.addDebugInfo("Destination Width", lDestWidth);
			ue.addDebugInfo("Destination Height", lDestHeight);
			ue.addDebugInfo("Stamp Width", lStampWidth);
			ue.addDebugInfo("Stamp Height", lStampHeight);
			ue.addDebugInfo("Horizontal %", dHorizPercent);
			ue.addDebugInfo("Vertical %", dVertPercent);
			ue.addDebugInfo("Computed X", lX);
			ue.addDebugInfo("Computed Y", lY);
			throw ue;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27277");
}
//--------------------------------------------------------------------------------------------------
void matchPalette(BITMAPHANDLE& hBmpSource, BITMAPHANDLE& rhBmpDest)
{
	// [p13 #4789]
	// code based on the recommendation given in the lead tools support forum:
	// Use L_ColorResBitmap to change a bitmaps BPP and/or palette as needed
	// http://support.leadtools.com/SupportPortal/cs/forums/18378/ShowPost.aspx

	// only 1-8 bit images are palettized
	if (hBmpSource.BitsPerPixel <= 8)
	{
		LeadToolsLicenseRestrictor leadToolsLicenseGuard;

		// set the destination image's color palette to match the source image
		throwExceptionIfNotSuccess(L_ColorResBitmap(&rhBmpDest, &rhBmpDest, sizeof(BITMAPHANDLE),
			hBmpSource.BitsPerPixel, CRF_USERPALETTE, hBmpSource.pPalette, 0, 
			hBmpSource.nColors, NULL, NULL), "ELI20164", 
			"Unable to update the stamp image color palette!");
	}
}
//--------------------------------------------------------------------------------------------------