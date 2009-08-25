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
#include "PDFInputOutputMgr.h"
#include "LeadToolsBitmapFreeer.h"
#include "LeadToolsFormatHelpers.h"

#include <UCLIDException.h>
#include <cpputil.h>
#include <LicenseMgmt.h>

//--------------------------------------------------------------------------------------------------
// Exported DLL functions
//--------------------------------------------------------------------------------------------------
void pasteImageAtLocation(const string& strInputImage, const string& strOutputImage,
						  const string& strPasteImage, double dHorizPercent, 
						  double dVertPercent, long lPage)
{
	ASSERT_ARGUMENT("ELI20174", dHorizPercent >= 0.0 && dHorizPercent <= 100.0);
	ASSERT_ARGUMENT("ELI20175", dVertPercent >= 0.0 && dVertPercent <= 100.0);

	// variable to hold the allocated bitmap list 
	// declared outside the try scope so that if there is an exception
	// we can release any memory that may already have been allocated
	HBITMAPLIST hInBitmapList = NULL;

	INIT_EXCEPTION_AND_TRACING("MLI00025");

	try
	{
		try
		{
			// verify that LeadTools is licensed 
			unlockDocumentSupport();

			// create a load files option
			LOADFILEOPTION lfo = GetLeadToolsSizedStruct<LOADFILEOPTION>(
				ELO_IGNOREVIEWPERSPECTIVE | ELO_ROTATED);
			_lastCodePos = "10";

			// create an input manager for the input image
			PDFInputOutputMgr inImage(strInputImage, true);
			_lastCodePos = "20";

			// create a file info struct for the input image
			FILEINFO flInInfo = GetLeadToolsSizedStruct<FILEINFO>(0);
			_lastCodePos = "30";

			// attempt to load the bitmap list, if successful also set the bListAllocated flag
			throwExceptionIfNotSuccess(L_LoadBitmapList(_bstr_t(inImage.getFileName().c_str()), 
				&hInBitmapList, 0, 0, &lfo, &flInInfo), "ELI20057", 
				"Could not open the input image", inImage.getFileNameInformationString());
			_lastCodePos = "40";

			// get the number of pages
			L_INT liTotalPages = flInInfo.TotalPages;

			// if lPage == -1, set lPage to last page
			if (lPage == -1) 
			{ 
				lPage = liTotalPages;
			}

			// validate the page number
			validatePageNumber(lPage, liTotalPages);
			_lastCodePos = "50";

			// createa a file info struct for the stamp image
			FILEINFO flPasteInfo = GetLeadToolsSizedStruct<FILEINFO>(0);
			_lastCodePos = "60";
			
			// declare the bitmaphandle and set the freer class to release it
			BITMAPHANDLE hPasteBitmap;
			LeadToolsBitmapFreeer freer(hPasteBitmap, true);
			_lastCodePos = "65";

			// load the stamp image
			loadStampBitmap(strPasteImage, flInInfo, flPasteInfo, hPasteBitmap);
			_lastCodePos = "70";

			// perform the paste operation (coordinates and image dimensions
			// will be validated inside the placeStamp function)
			placeStamp(hPasteBitmap, hInBitmapList, lPage, dHorizPercent, dVertPercent);
			_lastCodePos = "110";

			// now save the modified image to the output file
			saveStampedImage(hInBitmapList, liTotalPages, strOutputImage, flInInfo);
			_lastCodePos = "120";

			// free the bitmap list
			if (hInBitmapList != NULL)
			{
				throwExceptionIfNotSuccess(L_DestroyBitmapList(hInBitmapList), "ELI20058",
					"Unable to destroy bitmaplist!");
				_lastCodePos = "140";
				hInBitmapList = NULL;
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20168");
	}
	catch(UCLIDException& ue)
	{
		// make sure we clean up any allocated memory if an exception is thrown
		try
		{
			if (hInBitmapList != NULL)
			{
				throwExceptionIfNotSuccess(L_DestroyBitmapList(hInBitmapList), "ELI20059",
					"Unable to destroy bitmaplist!");
				hInBitmapList = NULL;
			}
		}
		CATCH_AND_LOG_ALL_EXCEPTIONS("ELI20060");

		throw ue;
	}
}

//--------------------------------------------------------------------------------------------------
// Non-exported local helper functions
//--------------------------------------------------------------------------------------------------
void loadStampBitmap(const string& strPasteImage, const FILEINFO& flInInfo,
					 FILEINFO& rflPasteInfo, BITMAPHANDLE& rhPasteBitmap)
{
	// check to be sure the BITMAPHANDLE has not been allocated yet
	ASSERT_ARGUMENT("ELI20061", !(rhPasteBitmap.Flags.Allocated));

	// create an input manager for the stamp image
	PDFInputOutputMgr pasteImage(strPasteImage, true);

	// create a load files option
	LOADFILEOPTION lfo = GetLeadToolsSizedStruct<LOADFILEOPTION>(
		ELO_IGNOREVIEWPERSPECTIVE | ELO_ROTATED);

	{
		// Limit ltLocker scope to the L_FileInfo call
		LeadToolsPDFLoadLocker ltLocker(false);

		// get the file info for the stamp image
		throwExceptionIfNotSuccess(L_FileInfo(_bstr_t(pasteImage.getFileName().c_str()),
			&rflPasteInfo, sizeof(rflPasteInfo), FILEINFO_TOTALPAGES, NULL), "ELI20062", 
			"Could not obtain watermark file information!", 
			pasteImage.getFileNameInformationString());
	}

	// ensure that the stamp image contains only 1 page
	if (rflPasteInfo.TotalPages > 1)
	{
		UCLIDException ue("ELI20081", "Watermark image cannot be multi-page!");
		ue.addDebugInfo("File name", strPasteImage);
		ue.addDebugInfo("Number of pages", rflPasteInfo.TotalPages);
		throw ue;
	}

	// if BitsPerPixel are not the same, convert the stamp image to the source BitsPerPixel
	if (rflPasteInfo.BitsPerPixel != flInInfo.BitsPerPixel)
	{
		// load the paste image, set the pixel depth to match the source image
		throwExceptionIfNotSuccess(L_LoadBitmapResize(_bstr_t(pasteImage.getFileName().c_str()),
			&rhPasteBitmap, sizeof(rhPasteBitmap), rflPasteInfo.Width, rflPasteInfo.Height, 
			flInInfo.BitsPerPixel, SIZE_NORMAL, flInInfo.Order, &lfo, &rflPasteInfo),
			"ELI20063", "Could not open and resize the watermark image!", 
			pasteImage.getFileNameInformationString());
	}
	else
	{
		// load the paste image
		throwExceptionIfNotSuccess(L_LoadBitmap(_bstr_t(pasteImage.getFileName().c_str()), 
			&rhPasteBitmap, sizeof(rhPasteBitmap), 0, 0,
			&lfo, &rflPasteInfo), "ELI20064", "Could not open the watermark image",
			pasteImage.getFileNameInformationString());
	}
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
void validateStampLocation(long lX, long lY, const BITMAPHANDLE& hBmpSource, 
						   const BITMAPHANDLE& hBmpStamp)
{
	// check if after paste the pasted piece would be outside source image dimensions
	if ((lX + hBmpStamp.Width) > hBmpSource.Width 
		|| (lY + hBmpStamp.Height) > hBmpSource.Height)
	{
		UCLIDException ue("ELI20065", "Invalid coordinates for stamping image!");
		ue.addDebugInfo("X coordinate", lX);
		ue.addDebugInfo("Y coordinate", lY);
		ue.addDebugInfo("Stamp Width", hBmpStamp.Width);
		ue.addDebugInfo("Stamp Height", hBmpStamp.Height);
		ue.addDebugInfo("Source Width", hBmpSource.Width);
		ue.addDebugInfo("Source Height", hBmpSource.Height);
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
void validateStampImageSize(const BITMAPHANDLE& hBmpSource, const BITMAPHANDLE& hBmpStamp)
{
	// check to be sure the paste image is smaller than the source image
	if (hBmpStamp.Width > hBmpSource.Width || hBmpStamp.Height > hBmpSource.Height)
	{
		UCLIDException ue("ELI20066", "Stamp image is too large!");
		ue.addDebugInfo("Source Width", hBmpSource.Width);
		ue.addDebugInfo("Stamp Width", hBmpStamp.Width);
		ue.addDebugInfo("Source Height", hBmpSource.Height);
		ue.addDebugInfo("Stamp Height", hBmpStamp.Height);
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
void validatePageNumber(long lPageNumber, long lNumberOfPages)
{
	if (lPageNumber > lNumberOfPages)
	{
		UCLIDException ue("ELI20067", "Invalid page number to apply stamp on!");
		ue.addDebugInfo("Page to stamp", lPageNumber);
		ue.addDebugInfo("Pages in image", lNumberOfPages);
		throw ue;
	}
}
//--------------------------------------------------------------------------------------------------
void placeStamp(BITMAPHANDLE& hPasteBmp, HBITMAPLIST& hInBitmapList,
				long lPageNumber, double dHorizPercent, double dVertPercent)
{
	// get the particular image page
	BITMAPHANDLE hSourceBmp;
	throwExceptionIfNotSuccess(L_GetBitmapListItem(hInBitmapList, lPageNumber-1, &hSourceBmp, 
		sizeof(BITMAPHANDLE)), "ELI20068", "Could not get image page from the bitmap list!"); 

	// validate the size of the stamp image
	validateStampImageSize(hSourceBmp, hPasteBmp);

	// get the X and Y coordinates based on the percentage
	long lX = getXCoordinate(dHorizPercent, hSourceBmp.Width);
	long lY = getYCoordinate(dVertPercent, hSourceBmp.Height);

	// validate the coordinates
	validateStampLocation(lX, lY, hSourceBmp, hPasteBmp);

	// ensure that both the stamp image and the source image have the same
	// color palette [p13 #4789]
	matchPalette(hSourceBmp, hPasteBmp);

	// now stamp the image page
	throwExceptionIfNotSuccess(L_CombineBitmap(&hSourceBmp, lX, lY, hPasteBmp.Width,
		hPasteBmp.Height, &hPasteBmp, 0, 0, CB_DST_0 | CB_OP_OR, 0), "ELI20069",
		"Failed to paste the stamp image to the bitmap!");

	// now save the modified page back to the bitmaplist
	throwExceptionIfNotSuccess(L_SetBitmapListItem(hInBitmapList, lPageNumber-1, &hSourceBmp),
		"ELI20070", "Failed to save the stamped page back to the bitmap list!");
}
//--------------------------------------------------------------------------------------------------
void saveStampedImage(HBITMAPLIST& hInBitmapList, long lNumberOfPages, const string& strOutImage,
					  const FILEINFO& flInInfo)
{
	INIT_EXCEPTION_AND_TRACING("MLI00024");

	try
	{
		// Get the retry count and timeout
		int iRetryCount(0), iRetryTimeout(0);
		getFileAccessRetryCountAndTimeout(iRetryCount, iRetryTimeout);

		// Create a temporary file to write the image to [LRCAU #5408]
		TemporaryFileName tmpFile(NULL, getExtensionFromFullPath(strOutImage).c_str());

		// Scope for PDF manager
		{
			PDFInputOutputMgr outFile(tmpFile.getName(), false);

			// create the save options
			SAVEFILEOPTION sfo = GetLeadToolsSizedStruct<SAVEFILEOPTION>(0);
			_lastCodePos = "10";

			// get the default save options
			int nRet = L_GetDefaultSaveFileOption(&sfo, sizeof(SAVEFILEOPTION));
			throwExceptionIfNotSuccess(nRet, "ELI20071", "Failed getting default save options!"); 

			// Get the compression factor for the input image type
			L_INT nCompression = getCompressionFactor(flInInfo.Format);

			// Get the file name from the output file manager
			char* pszFileName = (char*) outFile.getFileName().c_str();

			// loop through all the pages and save them to the out file
			for (long i = 0; i < lNumberOfPages; i++)
			{
				string strPageNumber = asString(i+1);

				// load one page of the bitmap
				BITMAPHANDLE bPageHandle;
				nRet = L_GetBitmapListItem(hInBitmapList, i, &bPageHandle, sizeof(BITMAPHANDLE));
				throwExceptionIfNotSuccess(nRet, "ELI20072",
					"Failed loading bitmap page from list!");
				_lastCodePos = "20 Page# " + strPageNumber;

				// save the loaded page
				sfo.PageNumber = i+1;
				int nNumFailedAttempts = 0;
				while (nNumFailedAttempts < iRetryCount)
				{
					nRet = L_SaveBitmap(pszFileName, &bPageHandle, 
						flInInfo.Format, flInInfo.BitsPerPixel, nCompression, &sfo);

					// check result
					if (nRet == SUCCESS)
					{
						// exit loop
						break;
					}
					else
					{
						nNumFailedAttempts++;

						Sleep(iRetryTimeout);
					}
				}

				if (nRet != SUCCESS)
				{
					UCLIDException ue("ELI20074", "Cannot save page");
					ue.addDebugInfo("Destination file", strOutImage);
					ue.addDebugInfo("Temporary file", tmpFile.getName());
					ue.addDebugInfo("PDF Manager File", outFile.getFileName());
					ue.addDebugInfo("Actual Error Code", nRet);
					ue.addDebugInfo("Error Message", getErrorCodeDescription(nRet));
					ue.addDebugInfo("Page Number", i+1);
					ue.addDebugInfo("Number Of Retries", nNumFailedAttempts);
					ue.addDebugInfo("Max Number Of Retries", iRetryCount);
					ue.addDebugInfo("Compression Flag", nCompression);
					ue.addDebugInfo("Total Number of pages", lNumberOfPages);
					addFormatDebugInfo(ue, flInInfo.Format);
					throw ue;
				}
				else
				{
					if (nNumFailedAttempts > 0)
					{
						UCLIDException ue("ELI20367",
							"Application Trace: Successfully saved image page after retry.");
						ue.addDebugInfo("Page number", i+1);
						ue.addDebugInfo("Destination file", strOutImage);
						ue.addDebugInfo("File Name", outFile.getFileNameInformationString());
						ue.addDebugInfo("Retries", nNumFailedAttempts);
						ue.log();
					}
				}
				_lastCodePos = "30 Page# " + strPageNumber;
			}
			// done saving pages
		} // end scope for PDF manager

		// Make sure the file can be read
		waitForFileToBeReadable(tmpFile.getName());
		_lastCodePos = "40";

		// Check that the input image and the output image have the same number of pages
		long nTmpCount = getNumberOfPagesInImage(tmpFile.getName());
		if (nTmpCount != lNumberOfPages)
		{
			UCLIDException ue("ELI27272", "Page count mismatch when saving image!");
			ue.addDebugInfo("Destination file", strOutImage);
			ue.addDebugInfo("Destination page count", lNumberOfPages);
			ue.addDebugInfo("Temporary file", tmpFile.getName());
			ue.addDebugInfo("Temporary page count", nTmpCount);
			throw ue;
		}
		_lastCodePos = "50";

		// Copy the temporary file to the destination file
		copyFile(tmpFile.getName(), strOutImage, false);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI20075");
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
		// set the destination image's color palette to match the source image
		throwExceptionIfNotSuccess(L_ColorResBitmap(&rhBmpDest, &rhBmpDest, sizeof(BITMAPHANDLE),
			hBmpSource.BitsPerPixel, CRF_USERPALETTE, hBmpSource.pPalette, 0, 
			hBmpSource.nColors, NULL, NULL), "ELI20164", 
			"Unable to update the stamp image color palette!");
	}
}
//--------------------------------------------------------------------------------------------------