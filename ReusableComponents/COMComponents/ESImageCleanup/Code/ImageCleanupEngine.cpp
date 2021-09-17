// ImageCleanupEngine.cpp : Implementation of CImageCleanupEngine

#include "stdafx.h"
#include "ImageCleanupEngine.h"

#include <InliteNamedMutexConstants.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <COMUtils.h>
#include <MiscLeadUtils.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <Misc.h>
#include <TemporaryFileName.h>
#include <LeadToolsFormatHelpers.h>
#include <LeadToolsBitmapFreeer.h>
#include <LeadToolsLicenseRestrictor.h>

#include <afxmt.h>
#include <vector>
#include <set>
#include <string>

//--------------------------------------------------------------------------------------------------
// Constants
//--------------------------------------------------------------------------------------------------
const string gstrRELATIVE_PATH_TO_CLEANUPIMAGE_EXE = "\\.\\CleanupImage.exe";

const double gdINVERT_PERCENTAGE = 60.0;

//--------------------------------------------------------------------------------------------------
// CImageCleanupEngine
//--------------------------------------------------------------------------------------------------
CImageCleanupEngine::CImageCleanupEngine() :
m_strPathToCleanupImageEXE("")
{
}
//--------------------------------------------------------------------------------------------------
CImageCleanupEngine::~CImageCleanupEngine()
{
	try
	{
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
		ASSERT_RESOURCE_ALLOCATION("ELI17090", ipImageCleanupSettings != __nullptr);

		IIUnknownVectorPtr ipOperations = ipImageCleanupSettings->ImageCleanupOperations;
		ASSERT_RESOURCE_ALLOCATION("ELI27417", ipOperations != __nullptr);

		vector<ESImageCleanupLib::IImageCleanupOperationPtr> vecEnabledCleanupOperations;
		getEnabledCleanupOperations(vecEnabledCleanupOperations, ipOperations);

		// Ensure there is at least one enabled operation in the list
		if (vecEnabledCleanupOperations.size() <= 0)
		{
			UCLIDException ue("ELI17092", "There are no enabled cleanup operations!");
			throw ue;
		}

		// Get the input and output file names
		string strInputFile = asString(bstrInputFile);
		string strOutputFile = asString(bstrOutputFile);

		string strWorkingInput = strInputFile;
		unique_ptr<TemporaryFileName> pTempInput(__nullptr);
		if (isPDF(strInputFile))
		{
			pTempInput.reset(new TemporaryFileName(true, __nullptr, ".tif"));
			strWorkingInput = pTempInput->getName();
			convertPDFToTIF(strInputFile, strWorkingInput);
		}

		bool bOutputPDF = isPDFFile(strOutputFile);
		TemporaryFileName tempFile(true, NULL,
			bOutputPDF ? ".tif" : getExtensionFromFullPath(strOutputFile).c_str());

		// Get a temporary file so that write operations appear atomic
		_lastCodePos = "10";

		int nPageCount = 0;
		FILEINFO fileInfo;
		getFileInformation(strWorkingInput, true, fileInfo);

		// Cache the format
		int iFormat = fileInfo.Format;

		// Get the page count
		nPageCount = fileInfo.TotalPages;

		// Store whether image is 1 bit per pixel or not
		bool bOneBitPerPixel = fileInfo.BitsPerPixel == 1;
		if (!bOneBitPerPixel && fileInfo.BitsPerPixel != 8 && fileInfo.BitsPerPixel != 24)
		{
			UCLIDException ue("ELI20364", "Unsupported bits per pixel!");
			ue.addDebugInfo("Supported bits per pixel", "1, 8, or 24");
			ue.addDebugInfo("BitsPerPixel", fileInfo.BitsPerPixel);
			ue.addDebugInfo("Input File", strInputFile);
			throw ue;
		}

		// Get a vector of pages to clean
		set<int> setPagesToClean;
		getSetOfPages(setPagesToClean, ipImageCleanupSettings, nPageCount);
		_lastCodePos = "30";

		// Get the load file options. ELO_ROTATED means that the image will not be auto-rotated
		// Don't ignore the view perspective because then the output image will be interpreted differently
		// than the input for non-standard view perspectives
		// https://extract.atlassian.net/browse/ISSUE-7220
		LOADFILEOPTION lfo = GetLeadToolsSizedStruct<LOADFILEOPTION>(ELO_ROTATED);

		// Get the save file options
		SAVEFILEOPTION sfo =
			GetLeadToolsSizedStruct<SAVEFILEOPTION>(0);

		{
			LeadToolsLicenseRestrictor leadToolsLicenseGuard;

			throwExceptionIfNotSuccess(L_GetDefaultSaveFileOption(&sfo, sizeof(SAVEFILEOPTION)),
				"ELI27308", "Unable to get default save options!");
		}

		// Attach mutex to CSingleLock and lock it
		CSingleLock snglLock(&sg_mutexINLITE_MUTEX, TRUE);

		// get the clear image server
		ICiServerPtr ipciServer(CLSID_CiServer);
		ASSERT_RESOURCE_ALLOCATION("ELI17099", ipciServer != __nullptr);
		_lastCodePos = "40";

		// get a repair image pointer
		ICiRepairPtr ipciRepair = ipciServer->CreateRepair();
		ASSERT_RESOURCE_ALLOCATION("ELI17100", ipciRepair != __nullptr);

		// Need to process each page individually
		_lastCodePos = "50";
		for (int i=1; i <= nPageCount; i++)
		{
			string strPageNum = asString(i);

			fileInfo = GetLeadToolsSizedStruct<FILEINFO>(FILEINFO_FORMATVALID);
			fileInfo.Format = iFormat;

			// Set the page
			lfo.PageNumber = i;
			sfo.PageNumber = i;

			// Load the page
			BITMAPHANDLE hBitmap = {0};
			LeadToolsBitmapFreeer freer(hBitmap);
			loadImagePage(strWorkingInput, hBitmap, fileInfo, lfo, false);
			_lastCodePos = "50_A_Page#" + strPageNum;

			// Check if this page is in the page to clean set
			if (setPagesToClean.find(i) != setPagesToClean.end())
			{
				// For bitonal images need to check the palette and switch it if it is not {black, white}
				// [p13 #4826 & #4702]
				if (bOneBitPerPixel)
				{
					swapPalette(hBitmap);
				}

				cleanImagePage(hBitmap, ipciServer, ipciRepair, vecEnabledCleanupOperations);
			}
			_lastCodePos = "50_B_Page#" + strPageNum;

			// Save the bitmap page to the output image
			saveImagePage(hBitmap, tempFile.getName(), fileInfo, sfo);
			_lastCodePos = "50_C_Page#" + strPageNum;
		}
		_lastCodePos = "60";

		// Finished with clear image objects, unlock the mutex
		snglLock.Unlock();	

		_lastCodePos = "70";

		// Ensure the output image is the same size as the input image
		int nOutputPageCount = getNumberOfPagesInImage(tempFile.getName());
		if (nOutputPageCount != nPageCount)
		{
			UCLIDException ue("ELI27309", "Page count mismatch!");
			ue.addDebugInfo("Input Image", strInputFile);
			ue.addDebugInfo("Temporary Output File", tempFile.getName());
			ue.addDebugInfo("Output Image", strOutputFile);
			ue.addDebugInfo("Input Page Count", nPageCount);
			ue.addDebugInfo("Output Page Count", nOutputPageCount);

			throw ue;
		}
		_lastCodePos = "80";

		if (bOutputPDF)
		{
			convertTIFToPDF(tempFile.getName(), strOutputFile);
		}
		else
		{
			// Copy the temp file to the output file
			copyFile(tempFile.getName(), strOutputFile, true);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI17095");
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
void CImageCleanupEngine::cleanImagePage(BITMAPHANDLE& rhBitmap, const ICiServerPtr& ipciServer, 
	 const ICiRepairPtr& ipciRepair, const vector<ESImageCleanupLib::IImageCleanupOperationPtr>& vecOperations)
{
	INIT_EXCEPTION_AND_TRACING("MLI00009");

	try
	{
		{
			LeadToolsLicenseRestrictor leadToolsLicenseGuard;

			// lock the bitmap to allow safe access to the bitmaps memory
			L_AccessBitmap(&rhBitmap);
		}

		// Ensure the bitmap is unlocked and the image is closed if an exception is thrown
		ICiImagePtr ipciImage = __nullptr;
		try
		{
			// get the size of the memory chunk for the bitmap
			unsigned long ulMemorySize = (unsigned long) rhBitmap.Size;

			// get the pointer to the bitmap in memory
			L_UCHAR* pBuf = rhBitmap.Addr.Windows.pData;
			_lastCodePos = "10";

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

			// Create the image and load it from memory
			ipciImage = ipciServer->CreateImage();
			ASSERT_RESOURCE_ALLOCATION("ELI17101", ipciImage != __nullptr);
			ipciImage->CreateBpp(rhBitmap.Width, rhBitmap.Height, rhBitmap.BitsPerPixel);
			ipciImage->LoadFromMemory(vFileInMemory);
			_lastCodePos = "40";

			// Load the image into the repair object
			ipciRepair->Image = ipciImage;

			// Iterate through the vector of clean up operations and perform them
			long nSize = (long) vecOperations.size(); 
			_lastCodePos = "50";
			for (long i=0; i < nSize; i++)
			{
				// Get the clean up operation
				ESImageCleanupLib::IImageCleanupOperationPtr ipOp = vecOperations[i];
				ASSERT_RESOURCE_ALLOCATION("ELI17102", ipOp != __nullptr);

				// Perform the operation
				ipOp->Perform((void*) ipciRepair);
				_lastCodePos = "50_Op#" + asString(i);
			}

			// Save the image to memory
			vFileInMemory = ipciImage->SaveToMemory();
			_lastCodePos = "60";

			// get a pointer to the SafeArray and lock it for access
			L_UCHAR* pNewImage = NULL;
			SafeArrayAccessData(vFileInMemory.parray, (void**) &pNewImage);
			_lastCodePos = "70";

			// determine the new row size (LeadTools used 4 byte row alignment, ClearImage uses 2
			unsigned long ulNewRowSize = ipciImage->LineBytes;

			// copy the cleaned page back into memory one row at a time using the new row size
			_lastCodePos = "80";
			{
				LeadToolsLicenseRestrictor leadToolsLicenseGuard;
				for (long j = 0; j < rhBitmap.Height; j++)
				{
					L_INT32 nRet = (L_INT32)L_PutBitmapRow(&rhBitmap, (pNewImage + (j * ulNewRowSize)),
						j, ulNewRowSize);
					if (nRet < 0)
					{
						throwExceptionIfNotSuccess(nRet, "ELI17103", "Could not put the bitmap row.");
					}
					_lastCodePos = "80 Row# " + asString(j);
				}
			}

			// unlock the safe array
			SafeArrayUnaccessData(vFileInMemory.parray);
			_lastCodePos = "90";

			// close the image to release the memory held by it
			ipciImage->Close();
			ipciImage = __nullptr;
			_lastCodePos = "100";

			{
				LeadToolsLicenseRestrictor leadToolsLicenseGuard;
				// Unlock the bitmap
				L_ReleaseBitmap(&rhBitmap);
			}
		}
		catch(...)
		{
			// Close the image if it has been allocated and has not been closed
			if (ipciImage != __nullptr && ipciImage->IsValid == ciTrue)
			{
				try
				{
					ipciImage->Close();
					ipciImage = __nullptr;
				}
				CATCH_AND_LOG_ALL_EXCEPTIONS("ELI27311");
			}

			{
				LeadToolsLicenseRestrictor leadToolsLicenseGuard;
				// Unlock the bitmap
				L_ReleaseBitmap(&rhBitmap);
			}

			throw;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI17104");
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupEngine::getSetOfPages(set<int>& rsetPageNumbers, 
							const ESImageCleanupLib::IImageCleanupSettingsPtr& ipSettings,
							int nPageCount)
{
	ASSERT_ARGUMENT("ELI17523", ipSettings != __nullptr);

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
				if (nPageCount > 1)
				{
					strSpecifiedPages = "1-" + asString(nPageCount);
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

		// Validate the specified pages string
		validatePageNumbers(strSpecifiedPages);

		// Get the sorted vector of page numbers
		vector<int> vecPageNumbers = getPageNumbers(nPageCount, strSpecifiedPages);

		// Place each page number in the set
		rsetPageNumbers.insert(vecPageNumbers.begin(), vecPageNumbers.end());
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
void CImageCleanupEngine::getEnabledCleanupOperations(
	vector<ESImageCleanupLib::IImageCleanupOperationPtr>& rvecOperations,
	const IIUnknownVectorPtr& ipOperations)
{
	try
	{
		ASSERT_ARGUMENT("ELI27304", ipOperations != __nullptr);

		// Clear the vector
		rvecOperations.clear();

		long nSize = ipOperations->Size();
		for (long i=0; i < nSize; i++)
		{
			// Get the object with description from the vector
			IObjectWithDescriptionPtr ipOWD = ipOperations->At(i);
			ASSERT_RESOURCE_ALLOCATION("ELI27305", ipOWD != __nullptr);

			if (ipOWD->Enabled == VARIANT_TRUE)
			{
				// Get the cleanup operation
				ESImageCleanupLib::IImageCleanupOperationPtr ipOperation = ipOWD->Object;
				ASSERT_RESOURCE_ALLOCATION("ELI27306", ipOperation != __nullptr);

				// Add the operation to the vector
				rvecOperations.push_back(ipOperation);
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI27302");
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupEngine::swapPalette(BITMAPHANDLE& rhBitmap)
{
	// If the first color in the palette is not black then need to change the palette
	if (rhBitmap.pPalette[0].rgbBlue != 0)
	{
		// Create a new temporary palette set to {black, white}
		L_RGBQUAD tmpPalette[2];
		tmpPalette[0].rgbBlue = tmpPalette[0].rgbGreen = tmpPalette[0].rgbRed 
			= tmpPalette[0].rgbReserved = tmpPalette[1].rgbReserved = 0;
		tmpPalette[1].rgbBlue = tmpPalette[1].rgbGreen = tmpPalette[1].rgbRed = 255;

		{
			LeadToolsLicenseRestrictor leadToolsLicenseGuard;

			// Swap the palette
			L_INT nRet = L_ColorResBitmap(&rhBitmap, &rhBitmap, sizeof(BITMAPHANDLE), 1,
				CRF_USERPALETTE, &tmpPalette[0], NULL, 2, NULL, NULL);
			throwExceptionIfNotSuccess(nRet, "ELI20320", "Could not modify color palette.");
		}
	}
}
//--------------------------------------------------------------------------------------------------
void CImageCleanupEngine::validateLicense()
{
	VALIDATE_LICENSE( gnIMAGE_CLEANUP_ENGINE_FEATURE, "ELI17157", "Image Cleanup Engine" );
}
//--------------------------------------------------------------------------------------------------