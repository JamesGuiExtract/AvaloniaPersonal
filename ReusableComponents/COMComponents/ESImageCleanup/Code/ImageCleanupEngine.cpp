// ImageCleanupEngine.cpp : Implementation of CImageCleanupEngine

#include "stdafx.h"
#include "ImageCleanupEngine.h"

#include <InliteNamedMutexConstants.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <MiscLeadUtils.h>
#include <PDFInputOutputMgr.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <Misc.h>
#include <LeadToolsFormatHelpers.h>

#include <string>
#include <afxmt.h>

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
const string gstrRELATIVE_PATH_TO_CLEANUPIMAGE_EXE = "\\.\\CleanupImage.exe";

const double gdINVERT_PERCENTAGE = 60.0;

//--------------------------------------------------------------------------------------------------
// CImageCleanupEngine
//--------------------------------------------------------------------------------------------------
CImageCleanupEngine::CImageCleanupEngine() :
m_hFileBitmaps(NULL),
m_strPathToCleanupImageEXE("")
{
}
//--------------------------------------------------------------------------------------------------
CImageCleanupEngine::~CImageCleanupEngine()
{
	try
	{
		// as per [p13 #4827] check to see if the bitmap list handle is NULL, if
		// not then destroy the bitmap list - fixed based on LeadTools support forum article:
		// http://support.leadtools.com/SupportPortal/cs/forums/18620/ShowPost.aspx
		freeBitmapList();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI17089");
}

//--------------------------------------------------------------------------------------------------
// ISupportErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CImageCleanupEngine::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IImageCleanupEngine
	};

	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//--------------------------------------------------------------------------------------------------
// IImageCleanupEngine
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CImageCleanupEngine::CleanupImageInternalUseOnly(BSTR bstrInputFile, 
								 BSTR bstrOutputFile, IImageCleanupSettings* pImageCleanupSettings)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	INIT_EXCEPTION_AND_TRACING("MLI00007");

	try
	{
		validateLicense();

		// wrap the image cleanup settings as a smart pointer
		ESImageCleanupLib::IImageCleanupSettingsPtr ipImageCleanupSettings(pImageCleanupSettings);
		ASSERT_RESOURCE_ALLOCATION("ELI17090", ipImageCleanupSettings != NULL);

		if (ipImageCleanupSettings->ImageCleanupOperations->Size() <= 0)
		{
			UCLIDException ue("ELI17092", "Vector of cleaning operations was empty!");
			throw ue;
		}

		// create the manager for the output file
		PDFInputOutputMgr outMgr(asString(bstrOutputFile), false);
		_lastCodePos = "10";

		// open the dirty image
		FILEINFO fileInfo;
		unsigned long lPageCount = openDirtyImage(asString(bstrInputFile), fileInfo);
		_lastCodePos = "20";
		if (lPageCount <= 0)
		{
			UCLIDException ue("ELI17093", "Image had no pages!");
			ue.addDebugInfo("bstrInputFile", asString(bstrInputFile));
			ue.addDebugInfo("lPageCount", lPageCount);
			throw ue;
		}

		// get a vector of pages to clean
		vector<int> vecPages;
		getVectorOfPages(vecPages, ipImageCleanupSettings, lPageCount);
		_lastCodePos = "30";

		// Attach mutex to CSingleLock and lock it
		CSingleLock snglLock(&sg_mutexINLITE_MUTEX, TRUE);

		// get the clear image server
		ICiServerPtr ipciServer(CLSID_CiServer);
		ASSERT_RESOURCE_ALLOCATION("ELI17099", ipciServer != NULL);
		_lastCodePos = "38";

		// get a repair image pointer
		ICiRepairPtr ipciRepair = ipciServer->CreateRepair();
		ASSERT_RESOURCE_ALLOCATION("ELI17100", ipciRepair != NULL);
		_lastCodePos = "39";

		// iterate through the pages of the image performing cleanup operations
		_lastCodePos = "40";
		for (vector<int>::iterator it = vecPages.begin(); it != vecPages.end(); it++)
		{
			// pages are stored zero based so clean PageNumber-1
			cleanImagePage((*it-1), ipciServer, ipciRepair,
				ipImageCleanupSettings->ImageCleanupOperations);
			_lastCodePos = "40 Page#" + asString(*it);
		}
		_lastCodePos = "50";

		// finished with clear image objects, unlock the mutex
		snglLock.Unlock();	

		// write the cleaned image to the output file
		writeCleanImage(lPageCount, fileInfo, outMgr);
		_lastCodePos = "60";
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17095");

	return S_OK;
}
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CImageCleanupEngine::CleanupImage(BSTR bstrInputFile, BSTR bstrOutputFile, 
											   BSTR bstrImageCleanupSettingsFile)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// command line should be <input file> <output file> <settings file>
		string strCommandLineArgs = "\"" + asString(bstrInputFile) + "\" \""; 
		strCommandLineArgs += asString(bstrOutputFile) + "\" \""; 
		strCommandLineArgs += asString(bstrImageCleanupSettingsFile) + "\"";

		// run the ESImageCleanupEngineExe with the above command line and the wait
		// flag set to infinite so that we wait until the exe finishes
		runExtractEXE(getESImageCleanupEngineEXE(), strCommandLineArgs, INFINITE);

	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17833");

	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// Private methods
//--------------------------------------------------------------------------------------------------
unsigned long CImageCleanupEngine::openDirtyImage(const string& rstrFileName, FILEINFO& rFileInfo)
{
	INIT_EXCEPTION_AND_TRACING("MLI00008");

	try
	{
		// get a manager for the input file 
		PDFInputOutputMgr ltPDF(rstrFileName, true);
		_lastCodePos = "10";

		// create and set the LOADFILEOPTIONS - ignore view and rotated ensure that lead tools
		// loads the image as it is in the file without changing the view perspective or
		// rotating the image
		LOADFILEOPTION lfo = GetLeadToolsSizedStruct<LOADFILEOPTION>(
			ELO_IGNOREVIEWPERSPECTIVE | ELO_ROTATED);
		_lastCodePos = "20";

		// reset the FILEINFO object
		rFileInfo = GetLeadToolsSizedStruct<FILEINFO>(0);
		_lastCodePos = "30";

		// Destroy the old bitmap list if necessary. [LegacyRCAndUtils #4932]
		freeBitmapList();

		//load image
		int nRet = L_LoadBitmapList(get_bstr_t(ltPDF.getFileName()), &m_hFileBitmaps, 0, 0, &lfo, 
			&rFileInfo);
		throwExceptionIfNotSuccess(nRet, "ELI17096", "Could not open the image.", 
			ltPDF.getFileNameInformationString());

		// for bitonal images need to check the palette and switch it if it is not {black, white}
		// [p13 #4826 & #4702]
		if (rFileInfo.BitsPerPixel == 1)
		{
			// get the first bitmap from the list so that its palette can be examined
			BITMAPHANDLE bmpTemp;
			nRet = L_GetBitmapListItem(m_hFileBitmaps, 0, &bmpTemp, sizeof(BITMAPHANDLE));
			throwExceptionIfNotSuccess(nRet, "ELI20319", 
				"Could not get first bitmap from the list.");

			// if the first color in the palette is not black then need to change the palette
			if (bmpTemp.pPalette[0].rgbBlue != 0)
			{
				// create a new temporary palette set to {black, white}
				L_RGBQUAD* pTmpPalette = new L_RGBQUAD[2];
				pTmpPalette[0].rgbBlue = pTmpPalette[0].rgbGreen = pTmpPalette[0].rgbRed 
					= pTmpPalette[0].rgbReserved = pTmpPalette[1].rgbReserved = 0;
				pTmpPalette[1].rgbBlue = pTmpPalette[1].rgbGreen = pTmpPalette[1].rgbRed = 255;
		
				// replace the palette for the image
				nRet = L_ColorResBitmapList(m_hFileBitmaps, 1, CRF_USERPALETTE,
					pTmpPalette, NULL, 2);

				// delete the temporary palette
				delete [] pTmpPalette;

				// now check for an error (do this after the memory cleanup)
				throwExceptionIfNotSuccess(nRet, "ELI20320", "Could not modify color palette.");
			}
		}
		// already handled check for 1 bit, now check for 8 or 24 bit, anything else throw
		// an exception [p13 #4848]
		else if (rFileInfo.BitsPerPixel != 8 && rFileInfo.BitsPerPixel != 24)
		{
			UCLIDException ue("ELI20364", "Unsupported bits per pixel!");
			ue.addDebugInfo("Supported bits per pixel", "1, 8, 24");
			ue.addDebugInfo("BitsPerPixel", rFileInfo.BitsPerPixel);
			ue.addDebugInfo("Image file", rstrFileName);
			throw ue;
		}

		// get the page count from the FILEINFO structure
		L_UINT nPages = rFileInfo.TotalPages;

		return nPages;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI17098");
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupEngine::cleanImagePage(int iPageNumber, ICiServerPtr ipciServer, 
										 ICiRepairPtr ipciRepair, 
										 IIUnknownVectorPtr ipVectorOfCleanupOperations)
{
	INIT_EXCEPTION_AND_TRACING("MLI00009");
	
	try
	{
		// get the particular image page
		BITMAPHANDLE bPageHandle;
		L_INT32 nRet = L_GetBitmapListItem(m_hFileBitmaps, iPageNumber, &bPageHandle, 
			sizeof(BITMAPHANDLE)); 
		throwExceptionIfNotSuccess(nRet, "ELI17107", "Could not get image page from bitmap list.");

		// lock the bitmap to allow safe access to the bitmaps memory
		L_AccessBitmap(&bPageHandle);
		_lastCodePos = "10";

		// get the size of the memory chunk for the bitmap
		unsigned long ulMemorySize = (unsigned long) bPageHandle.Size;

		// get the pointer to the bitmap in memory
		L_UCHAR* pBuf = bPageHandle.Addr.Windows.pData;

		// prepare the variant and safe array for loading from memory
		SAFEARRAYBOUND bounds = {ulMemorySize, 0};
		L_UCHAR* pByte = NULL;
		_variant_t vFileInMemory;
		vFileInMemory.vt = (VT_ARRAY | VT_UI1);
		vFileInMemory.parray = SafeArrayCreate(VT_UI1, 1, &bounds);
		_lastCodePos = "20";

		// copy the bitmap into the safearray for loading into the ClearImage image object
		SafeArrayAccessData(vFileInMemory.parray, (void**) &pByte);
		memcpy(pByte, pBuf, ulMemorySize); 
		SafeArrayUnaccessData(vFileInMemory.parray);
		_lastCodePos = "30";

		// create the image and load it from memory
		ICiImagePtr ipciImage = ipciServer->CreateImage();
		ASSERT_RESOURCE_ALLOCATION("ELI17101", ipciImage != NULL);

		ipciRepair->Image = ipciImage;
		ipciRepair->Image->CreateBpp(bPageHandle.Width, bPageHandle.Height, 
			bPageHandle.BitsPerPixel);
		ipciRepair->Image->LoadFromMemory(vFileInMemory);
		_lastCodePos = "40";

		// NOTE: This is the spot in the code where the image used to be inverted, it is
		//		 no longer necessary because we check the palette when the image
		//		 is loaded and change the palette if needed at that point, there
		//		 is no need to invert the image and then re-invert it when image
		//		 cleanup is done.  This will also make it easier to add an auto-invert
		//		 cleanup operation in the future
		//		 [p13 #4826] - JDS 01/30/2008

		// iterate through the vector of clean up operations and perform them
		long lVecSize = ipVectorOfCleanupOperations->Size();
		_lastCodePos = "60";
		for (long i=0; i < lVecSize; i++)
		{
			// get the clean up operation
			IObjectWithDescriptionPtr ipCleanWithDescr = ipVectorOfCleanupOperations->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI17226", ipCleanWithDescr != NULL);
			ESImageCleanupLib::IImageCleanupOperationPtr ipCleanupOperation = 
				ipCleanWithDescr->Object;
			ASSERT_RESOURCE_ALLOCATION("ELI17102", ipCleanupOperation != NULL);

			// if operation is enabled then perform it 
			if (ipCleanWithDescr->Enabled == VARIANT_TRUE)
			{
				// perform the cleanup operation - store the operation # and description 
				// in the _lastCodePos
				_lastCodePos = "60 Operation# " + asString(i) + ": " 
					+ asString(ipCleanWithDescr->Description);
				ipCleanupOperation->Perform((void*) ipciRepair);
			}
		}
		_lastCodePos = "65";

		// save the image to memory and close it
		vFileInMemory = ipciRepair->Image->SaveToMemory();
		_lastCodePos = "80";

		// get a pointer to the SafeArray and lock it for access
		L_UCHAR* pNewImage = NULL;
		SafeArrayAccessData(vFileInMemory.parray, (void**) &pNewImage);
		_lastCodePos = "90";

		// determine the new row size (LeadTools used 4 byte row alignment, ClearImage uses 2
		unsigned long ulNewRowSize = ipciRepair->Image->LineBytes;

		// copy the cleaned page back into memory one row at a time using the new row size
		_lastCodePos = "100";
		for (long j=0; j < bPageHandle.Height; j++)
		{
			nRet = (L_INT32) L_PutBitmapRow(&bPageHandle, (pNewImage+(j*ulNewRowSize)), 
				j, ulNewRowSize);
			_lastCodePos = "100 Row# " + asString(j);
			if (nRet < 0)
			{
				throwExceptionIfNotSuccess(nRet, "ELI17103", "Could not put the bitmap row.");
			}
		}

		// unlock the safe array
		SafeArrayUnaccessData(vFileInMemory.parray);
		_lastCodePos = "110";

		// close the image to release the memory held by the repair pointer
		ipciRepair->Image->Close();
		_lastCodePos = "120";

		// unlock the bitmap
		L_ReleaseBitmap(&bPageHandle);
		_lastCodePos = "130";

		// The image has changed, set the bitmap list item [LegacyRCAndUtils #5299]
		nRet = L_SetBitmapListItem(m_hFileBitmaps, iPageNumber, &bPageHandle);
		throwExceptionIfNotSuccess(nRet, "ELI25414", "Cannot set bitmap list item.");
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI17104");
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupEngine::writeCleanImage(unsigned long lNumberOfPages, const FILEINFO& rFileInfo,
										  PDFInputOutputMgr& rOutMgr)
{
	INIT_EXCEPTION_AND_TRACING("MLI00010");

	try
	{
		// create the save options
		SAVEFILEOPTION sfo = GetLeadToolsSizedStruct<SAVEFILEOPTION>(0);
		_lastCodePos = "10";

		// get the default save options
		int nRet = L_GetDefaultSaveFileOption(&sfo, sizeof(SAVEFILEOPTION));
		throwExceptionIfNotSuccess(nRet, "ELI17108", "Failed getting default save options!"); 

		// Specify appropriate compression level for file types
		// which support this setting. [LRCAU #5189 - JDS 03/31/2009]
		L_INT nCompression = getCompressionFactor(rFileInfo.Format);

		// Get the retry count and timeout values
		int iRetryCount(0), iRetryTimeout(0);
		getFileAccessRetryCountAndTimeout(iRetryCount, iRetryTimeout);

		// loop through all the pages and save them to the out file
		for (unsigned long i = 0; i < lNumberOfPages; i++)
		{
			// load one page of the bitmap
			BITMAPHANDLE bPageHandle;
			nRet = L_GetBitmapListItem(m_hFileBitmaps, i, &bPageHandle, sizeof(BITMAPHANDLE));
			throwExceptionIfNotSuccess(nRet, "ELI17109", "Failed loading bitmap page from list!");
			_lastCodePos = "20 Page# " + asString(i);
			
			// save the loaded page
			sfo.PageNumber = i+1;
			int nNumFailedAttempts = 0;
			while (nNumFailedAttempts < iRetryCount)
			{
				nRet = L_SaveBitmap(get_bstr_t(rOutMgr.getFileName()), &bPageHandle, 
					rFileInfo.Format,
					rFileInfo.BitsPerPixel, nCompression, &sfo);

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
				UCLIDException ue("ELI17105", "Cannot save page!");
				ue.addDebugInfo("Original File Name", rOutMgr.getFileNameInformationString());
				ue.addDebugInfo("PDF Manager File", rOutMgr.getFileName());
				ue.addDebugInfo("Page Number", i+1);
				ue.addDebugInfo("Actual Error Code", nRet);
				ue.addDebugInfo("Error string", getErrorCodeDescription(nRet));
				ue.addDebugInfo("Retries Attempted", nNumFailedAttempts);
				ue.addDebugInfo("Max Retries", iRetryCount);
				ue.addDebugInfo("Compression Flag", nCompression);
				addFormatDebugInfo(ue, rFileInfo.Format);
				throw ue;
			}
			else
			{
				// if saved successfully after a retry then log the save with the
				// number of retries required [p13 #4840]
				if (nNumFailedAttempts > 0)
				{
					UCLIDException ue("ELI20354", "Saved page successfully after retry");
					ue.addDebugInfo("Number of retries", nNumFailedAttempts);
					ue.addDebugInfo("File name", rOutMgr.getFileNameInformationString());
					ue.log();
				}
			}
			_lastCodePos = "30 Page# " + asString(i);
		}

		// Make sure the file can be read
		waitForFileToBeReadable(rOutMgr.getFileName());

		// free the bitmap list if it has been allocated
		freeBitmapList();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI17106");
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupEngine::getVectorOfPages(vector<int>& rvecPageNumbers, 
							ESImageCleanupLib::IImageCleanupSettingsPtr ipSettings, long lPageCount)
{
	ASSERT_ARGUMENT("ELI17523", ipSettings != NULL);

	try
	{
		// get the specified pages string
		string strSpecifiedPages = "";

		// check what the page range type is
		switch(ipSettings->ICPageRangeType)
		{
		case ESImageCleanupLib::kCleanAll:
			{
				// for clean all set specified pages to 1-lPageCount
				// (if the image has only 1 page, then set to page 1)
				if (lPageCount > 1)
				{
					strSpecifiedPages = "1-" + asString(lPageCount);
				}
				else
				{
					strSpecifiedPages = "1";
				}
			}
			break;
			
		case ESImageCleanupLib::kCleanSpecified:
			{
				// for clean specified, user already set the specified string
				strSpecifiedPages = asString(ipSettings->SpecifiedPages);
			}
			break;

		case ESImageCleanupLib::kCleanFirst:
			{
				// for clean first lNumberOfPages set the specified pages
				// to 1-lNumberOfPages (in the case of 1 page just set to
				// page 1)
				string strTemp = asString(ipSettings->SpecifiedPages);
				if (strTemp == "1")
				{
					strSpecifiedPages = "1";
				}
				else
				{
					strSpecifiedPages = "1-" + strTemp;
				}
			}
			break;

		case ESImageCleanupLib::kCleanLast:
			{
				// for clean last set specified pages to -specifiedpages
				strSpecifiedPages = "-" + asString(ipSettings->SpecifiedPages);
			}
			break;

		default:
			{
				UCLIDException ue("ELI17525", "Unrecognized page range type!");
				ue.addDebugInfo("Page Range Type", (long)ipSettings->ICPageRangeType);
				throw ue;
			}
		}

		// validate the specified pages string
		validatePageNumbers(strSpecifiedPages);

		// get the sorted vector of page numbers
		rvecPageNumbers = getPageNumbers((int)lPageCount, strSpecifiedPages);
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI17524");
}
//--------------------------------------------------------------------------------------------------
string CImageCleanupEngine::getESImageCleanupEngineEXE()
{
	try
	{
		// check for an existing valid path to CleanupImage.exe
		if (m_strPathToCleanupImageEXE == "")
		{
			// build path to CleanupImage.exe relative to ESImageCleanup.dll
			string strPathToEXE = getModuleDirectory(_Module.m_hInst);

			strPathToEXE += gstrRELATIVE_PATH_TO_CLEANUPIMAGE_EXE;

			// change relative path to absolute path
			simplifyPathName(strPathToEXE);

			// validate existence of CleanupImage.exe
			if (!isFileOrFolderValid(strPathToEXE))
			{
				UCLIDException ue("ELI17841", "Cannot find CleanupImage.exe!");
				ue.addDebugInfo("Path to EXE", strPathToEXE);
				throw ue;
			}

			m_strPathToCleanupImageEXE = strPathToEXE;
		}

		// return the absolute path
		return m_strPathToCleanupImageEXE;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI17835");
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupEngine::validateLicense()
{
	VALIDATE_LICENSE( gnIMAGE_CLEANUP_ENGINE_FEATURE, "ELI17157", "Image Cleanup Engine" );
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupEngine::freeBitmapList()
{
	// free the bitmap list if it has been allocated
	if (m_hFileBitmaps != NULL)
	{
		int nRet = L_DestroyBitmapList(m_hFileBitmaps);
		throwExceptionIfNotSuccess(nRet, "ELI17110", "Failed freeing bitmap list!");

		// as per [p13 #4827] set the bitmap list handle to NULL after destroying it
		m_hFileBitmaps = NULL;
	}
}
//--------------------------------------------------------------------------------------------------