#include "StdAfx.h"
#include "ImageFormatConversion.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <cpputil.h>
#include <TemporaryFileName.h>
#include <ComponentLicenseIDs.h>
#include <Misc.h>
#include <KernelAPI.h>
#include <ScansoftErr.h>
#include <OcrMethods.h>

#include <string>

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

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
// Image format conversion functions
//-------------------------------------------------------------------------------------------------
namespace ifc {
	void convertImage(
		const string strInputFileName,
		const string strOutputFileName,
		EConverterFileType eOutputType,
		bool bPreserveColor,
		string strPagesToRemove,
		IMF_FORMAT nExplicitFormat,
		int nCompressionLevel)
	{
		// [LegacyRCAndUtils:6275]
		// Since we will be using the Nuance engine, ensure we are licensed for it.
		VALIDATE_LICENSE(gnOCR_ON_CLIENT_FEATURE, "ELI34323", "ImageFormatConverter");

		IMF_FORMAT nFormat(FF_TIFNO);
		bool bOutputImageOpened(false);
		int nPage(0);
		unique_ptr<HIMGFILE> uphOutputImage(__nullptr);
		unique_ptr<TemporaryFileName> pTempOutputFile;

		try
		{
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

			IMG_INFO imgInfo = { 0 };
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
					&& (!bPreserveColor || nFormat != FF_TIFNO && nFormat != FF_TIFPB && nFormat != FF_TIFLZW && nFormat != FF_TIFJPGNEW))
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
				else if (eOutputType == kFileType_Tif && nFormat == FF_TIFJPGNEW)
				{
					THROW_UE_ON_ERROR("ELI54153", "Cannot save to image page in JPG in TIF format.",
						kRecSaveImgForce(0, *uphOutputImage, nFormat, hImagePage, II_CURRENT, TRUE));
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
		catch (...)
		{
			UCLIDException ue = uex::fromCurrent("ELI34281");
			ue.addDebugInfo("Source image", strInputFileName);
			ue.addDebugInfo("Page", asString(nPage + 1));
			ue.addDebugInfo("Format", asString((int)nFormat));

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

			throw ue;
		}
	}
	//-------------------------------------------------------------------------------------------------
	void convertImagePage(
		const string strInputFileName,
		const string strOutputFileName,
		EConverterFileType eOutputType,
		bool bPreserveColor,
		int nPage,
		IMF_FORMAT nExplicitFormat,
		int nCompressionLevel)
	{
		// [LegacyRCAndUtils:6275]
		// Since we will be using the Nuance engine, ensure we are licensed for it.
		VALIDATE_LICENSE(gnOCR_ON_CLIENT_FEATURE, "ELI47263", "ImageFormatConverter");

		IMF_FORMAT nFormat(FF_TIFNO);
		bool bOutputImageOpened(false);
		unique_ptr<HIMGFILE> uphOutputImage(__nullptr);

		try
		{
			HIMGFILE hInputImage;
			THROW_UE_ON_ERROR("ELI47264", "Unable to open source image file.",
				kRecOpenImgFile(strInputFileName.c_str(), &hInputImage, IMGF_READ, FF_SIZE));

			// Ensure that the memory stored for the image file is released
			RecMemoryReleaser<tagIMGFILEHANDLE> inputImageFileMemoryReleaser(hInputImage);

			int nPageCount = 0;
			THROW_UE_ON_ERROR("ELI47265", "Unable to get page count.",
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

			// If the destination file name exists before Nuance tries to open it, it will throw an error.
			// If the destination file name exists before Nuance tries to open it, it will throw an error.
			if (isFileOrFolderValid(strOutputFileName))
			{
				deleteFile(strOutputFileName);
			}

			// Don't use RecMemoryReleaser on the output image because we will need to manually
			// close it at the end of this method to be able to copy it to its permanent location.
			uphOutputImage = std::make_unique<HIMGFILE>();
			THROW_UE_ON_ERROR("ELI47266", "Unable to create destination image file.",
				kRecOpenImgFile(strOutputFileName.c_str(), uphOutputImage.get(), IMGF_RDWR, FF_SIZE));

			bOutputImageOpened = true;

			ASSERT_RUNTIME_CONDITION("ELI47267", nPage > 0 && nPage <= nPageCount, "Page must be less than or equal to input document page count");

			IMG_INFO imgInfo = { 0 };
			bool bConversionSet = false;
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
				THROW_UE_ON_ERROR("ELI47268", "Failed to identify image format.",
					kRecGetImgFilePageInfo(0, hInputImage, nPage - 1, &imgInfo, &imgFormat));
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
				&& (!bPreserveColor || nFormat != FF_TIFNO && nFormat != FF_TIFPB && nFormat != FF_TIFLZW && nFormat != FF_TIFJPGNEW))
			{
				if (!bConversionSet)
				{
					THROW_UE_ON_ERROR("ELI47269", "Unable to set image conversion method.",
						kRecSetImgConvMode(0, CNV_AUTO));
					bConversionSet = true;
				}
			}
			else if (bConversionSet)
			{
				THROW_UE_ON_ERROR("ELI47270", "Unable to set image conversion method.",
					kRecSetImgConvMode(0, CNV_NO));
				bConversionSet = false;
			}

			// NOTE: RecAPI uses zero-based page number indexes
			HPAGE hImagePage;
			loadPageFromImageHandle(strInputFileName, hInputImage, nPage - 1, &hImagePage);

			// Ensure that the memory stored for the image page is released.
			RecMemoryReleaser<RECPAGESTRUCT> pageMemoryReleaser(hImagePage);

			if (eOutputType == kFileType_Jpg)
			{
				THROW_UE_ON_ERROR("ELI47271", "Cannot save to image page in jpg format.",
					kRecSaveImgForce(0, *uphOutputImage, nFormat, hImagePage, II_CURRENT, FALSE));
			}
			else if (eOutputType == kFileType_Tif && nFormat == FF_TIFJPGNEW)
			{
				THROW_UE_ON_ERROR("ELI54293", "Cannot save to image page in JPG in TIF format.",
					kRecSaveImgForce(0, *uphOutputImage, nFormat, hImagePage, II_CURRENT, TRUE));
			}
			else
			{
				THROW_UE_ON_ERROR("ELI47272", "Cannot save to image page in the specified format.",
					kRecSaveImg(0, *uphOutputImage, nFormat, hImagePage, II_CURRENT, TRUE));
			}

			kRecCloseImgFile(*uphOutputImage);

		}
		catch (...)
		{
			UCLIDException ue = uex::fromCurrent("ELI47273");
			ue.addDebugInfo("Source image", strInputFileName);
			ue.addDebugInfo("Page", asString(nPage));
			ue.addDebugInfo("Format", asString((int)nFormat));

			// We need to close the out image file if the output image file was opened but we didn't
			// make it to the "happy case" close call.
			if (bOutputImageOpened && nPage != -1)
			{
				try
				{
					kRecCloseImgFile(*uphOutputImage);
				}
				CATCH_AND_LOG_ALL_EXCEPTIONS("ELI47274");
			}

			throw ue;
		}
	}
}
//-------------------------------------------------------------------------------------------------
