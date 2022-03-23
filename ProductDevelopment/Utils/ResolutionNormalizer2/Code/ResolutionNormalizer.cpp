// ResolutionNormalizer.cpp : Defines the entry point for the application.
//

#include "stdafx.h"
#include "ResolutionNormalizer.h"
#include "RecMemoryReleaser.h"

#include <UCLIDException.h>
#include <UCLIDExceptionDlg.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <cpputil.h>
#include <KernelAPI.h>
#include <TemporaryFileName.h>

#include <ScansoftErr.h>
#include <OcrMethods.h>

#include "..\..\..\..\ReusableComponents\COMComponents\UCLIDRasterAndOCRMgmt\OCREngines\SSOCR2\Code\OcrConstants.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

CResolutionNormalizerApp theApp;

//-------------------------------------------------------------------------------------------------
// CResolutionNormalizerApp
//-------------------------------------------------------------------------------------------------
CResolutionNormalizerApp::CResolutionNormalizerApp()
{
}
//-------------------------------------------------------------------------------------------------
BOOL CResolutionNormalizerApp::InitInstance()
{
	int nExitCode = EXIT_SUCCESS;

	try
	{
		string strExceptionLog;

		try
		{
			try
			{
				static UCLIDExceptionDlg exceptionDlg;
				UCLIDException::setExceptionHandler( &exceptionDlg );

				LicenseManagement::loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);
				validateLicense();

				string strFileName;
				double dResFactor = 1.5;

				vector<string> vecParams;
				int i;
				for (i = 1; i < __argc; i++)
				{
					string strArg = __argv[i];

					if (strArg.find( "/?" ) != string::npos)
					{
						usage("");
						return FALSE;
					}
					else if (strArg.find( "/ef" ) != string::npos)
					{
						i++;
						if (i >= __argc)
						{
							usage("Log filename expected.");
							return FALSE;
						}

						strExceptionLog = __argv[i];
					}
					else if (i == 1)
					{
						strFileName = strArg;
					}
					else if (i == 2)
					{
						try
						{
							dResFactor = asDouble(strArg);
						}
						catch (...)
						{
							usage(Util::Format("Unable to parse resolution factor: \"%s\".", strArg.c_str()));
							return FALSE;
						}

						if (dResFactor < 1)
						{
							usage(Util::Format("Resolution factor must be >= 1: \"%s\".", strArg.c_str()));
							return FALSE;
						}
					}
					else
					{
						usage(Util::Format("Unrecognized option: \"%s\"", strArg.c_str()));
						return FALSE;
					}
				}

				processFile(strFileName, dResFactor);
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI39964");
		}
		catch(UCLIDException& ue)
		{
			nExitCode = EXIT_FAILURE;

			// If /ef parameter was used specify log file exceptions should be written to.
			if (!strExceptionLog.empty())
			{
				ue.log( strExceptionLog, false );
			}
			// Otherwise display the exception.
			else
			{
				ue.display();
			}
		}
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI39965");

	return nExitCode;
}
//-------------------------------------------------------------------------------------------------
void CResolutionNormalizerApp::processFile(const string& strFileName, double dResFactor)
{
	string strTempFileName;

	try
	{
		try
		{
			validateFileOrFolderExistence(strFileName);

			int nOriginaPageCount(0);
			int nPagesUpdated(0);

			// Make a copy of the document to process so that if anything goes wrong, the original is
			// not affected.
			string strExt = getExtensionFromFullPath(strFileName);
			ASSERT_RUNTIME_CONDITION("ELI39976", _strcmpi(".pdf", strExt.c_str()) != 0,
				"ResolutionNormalizer2 does not support PDF files.");
			unique_ptr<TemporaryFileName> pTempOutputFile(new TemporaryFileName(true, NULL, strExt.c_str()));
			strTempFileName = pTempOutputFile->getName();
			copyFile(strFileName, strTempFileName);

			initNuanceEngineAndLicense();

			normalizeResolution(strTempFileName, dResFactor, &nOriginaPageCount, &nPagesUpdated);

			if (nPagesUpdated > 0)
			{
				finalizeAndValidateOutput(strTempFileName, nOriginaPageCount);

				// Normalization succeeded; replace the original file with the normalized version.

				// https://extract.atlassian.net/browse/ISSUE-13775
				// Retry 3 times to account for access denied exceptions encountered during an
				// endurance test.
				UCLIDException ueMoveError;
				for (int i = 0; i < 3; i++)
				{

					try
					{
						try
						{
							moveFile(strTempFileName, strFileName, true);
							return;
						}
						CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI39978")
					}
					catch (UCLIDException &ue)
					{
						UCLIDException ueTrace("ELI39980", "Application trace: Failed to move output.", ue);
						ueTrace.addDebugInfo("Retries", i);
						ueTrace.log();

						ueMoveError = UCLIDException("ELI39979", "Failed to move output after retries.", ue);
					}

					Sleep(1000);
				}

				throw ueMoveError;
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI39977");
	}
	catch (UCLIDException &ue)
	{
		try
		{
			if (!strTempFileName.empty() && isValidFile(strTempFileName))
			{
				deleteFile(strTempFileName);
			}
		}
		CATCH_AND_LOG_ALL_EXCEPTIONS("ELI39973");

		ue.addDebugInfo("Filename", strFileName);
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void CResolutionNormalizerApp::initNuanceEngineAndLicense()
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
				THROW_UE("ELI39929", "Unable to load Nuance engine license file!", rc);
			}
			catch (UCLIDException& ue)
			{
				loadScansoftRecErrInfo(ue, rc);
				throw ue;
			}
		}

		// Initialization of OCR engine	
		rc = kRecInit("Extract Systems", "ResolutionNormalizer");
		if (rc != REC_OK && rc != API_INIT_WARN)
		{
			// create the exception object to throw to outer scope
			THROW_UE("ELI39930", "Unable to initialize Nuance engine!", rc);
		}

		// If API_INIT_WARN, ensure that the required modules are available
		if (rc == API_INIT_WARN)
		{
			LPKRECMODULEINFO pModules;
			size_t size;
			THROW_UE_ON_ERROR("ELI39931", "Unable to obtain modules information from the Nuance engine!",
				kRecGetModulesInfo(&pModules, &size));
			
			// if a required library module is not there, do not continue.
			if (pModules[INFO_MOR].Version <= 0)
			{
				THROW_UE("ELI39932", "Unable to find required MOR module for Nuance engine to run.", rc);
			}
			if(pModules[INFO_MTX].Version <= 0)
			{
				THROW_UE("ELI39933", "Unable to find required MTX module for Nuance engine to run.", rc);
			}
			if (pModules[INFO_PLUS2W].Version <= 0)
			{
				THROW_UE("ELI39934", "Unable to find required PLUS2W module for Nuance engine to run.", rc);
			}
			if (pModules[INFO_PLUS3W].Version <= 0)
			{
				THROW_UE("ELI39935", "Unable to find required PLUS3W module for Nuance engine to run.", rc);
			}
			if (pModules[INFO_HNR].Version <= 0)
			{
				THROW_UE("ELI39936", "Unable to find required HNR module for Nuance engine to run.", rc);
			}
			if (pModules[INFO_RER].Version <= 0)
			{
				THROW_UE("ELI39937", "Unable to find required RER module for Nuance engine to run.", rc);
			}
			if(pModules[INFO_DOT].Version <= 0)
			{
				THROW_UE("ELI39938", "Unable to find required DOT module for Nuance engine to run.", rc);
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI39939")
}
//-------------------------------------------------------------------------------------------------
void CResolutionNormalizerApp::normalizeResolution(const string& strFileName, double dResFactor,
	int *pnPageCount, int *pnPagesUpdated)
{
	ASSERT_ARGUMENT("ELI39969", pnPageCount != nullptr);
	ASSERT_ARGUMENT("ELI39970", pnPagesUpdated != nullptr);

	*pnPageCount = 0;
	*pnPagesUpdated = 0;
	int nPage(0);

	try
	{
		try
		{
			HIMGFILE hImage;
			THROW_UE_ON_ERROR("ELI39940", "Unable to open source image file.",
				kRecOpenImgFile(strFileName.c_str(), &hImage, IMGF_RDWR, FF_SIZE));

			// Ensure that the memory stored for the image file is released
			RecMemoryReleaser<tagIMGFILEHANDLE> imageFileMemoryReleaser(hImage);

			THROW_UE_ON_ERROR("ELI39941", "Unable to get page count.",
				kRecGetImgFilePageCount(hImage, pnPageCount));

			for (nPage = 0; nPage < *pnPageCount; nPage++)
			{
				if (normalizePageResolution(strFileName, hImage, nPage, dResFactor))
				{
					(*pnPagesUpdated)++;
				}
			}
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI39942");
	}
	catch (UCLIDException &ue)
	{
		ue.addDebugInfo("Page", asString(nPage + 1));
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
bool CResolutionNormalizerApp::normalizePageResolution(const string& strFileName, HIMGFILE hImage,
													   int nPage, double dResFactor)
{
	try
	{
		IMG_INFO imgInfo = {0};
		IMF_FORMAT imgFormat;

		// NOTE: RecAPI uses zero-based page number indexes
		THROW_UE_ON_ERROR("ELI39959", "Failed to identify image format.",
			kRecGetImgFilePageInfo(0, hImage, nPage, &imgInfo, &imgFormat));

		LONG nMaxRes = max(imgInfo.DPI.cx, imgInfo.DPI.cy);
		LONG nMinRes = min(imgInfo.DPI.cx, imgInfo.DPI.cy);

		HPAGE hImagePage;
		loadPageFromImageHandle(strFileName, hImage, nPage, &hImagePage);

		// Ensure that the memory stored for the image page is released.
		RecMemoryReleaser<RECPAGESTRUCT> pageMemoryReleaser(hImagePage);

		// If DPI isn't symmetric (to within dResFactor), make it symmetric
		if (nMaxRes > 0 && abs((double)nMaxRes / (double)nMinRes) > dResFactor)
		{
			SIZE newDPI = { nMaxRes, nMaxRes };

			SIZE newSize =
				{
					imgInfo.Size.cx * (nMaxRes / imgInfo.DPI.cx),
					imgInfo.Size.cy * (nMaxRes / imgInfo.DPI.cy)
				};

			// Use of newSize when calling kRecGetImgArea will scale pBitmap to the desired
			// dimensions.
			BYTE *pBitmap = nullptr;
			THROW_UE_ON_ERROR("ELI39960", "Failed to scale page resolution.",
				kRecGetImgArea(0, hImagePage, II_CURRENT, NULL, &newSize, &imgInfo, &pBitmap));

			// Create a new HPAGE of the desired size to which the scaled image data will be applied.
			HPAGE hNewImagePage = nullptr;
			THROW_UE_ON_ERROR("ELI39961", "Failed to initialize page.",
				kRecCreateImg(0, newSize.cx, newSize.cy, imgInfo.DPI.cx, imgInfo.DPI.cy,
					imgInfo.BitsPerPixel, &hNewImagePage));

			RecMemoryReleaser<RECPAGESTRUCT> newPageMemoryReleaser(hNewImagePage);

			THROW_UE_ON_ERROR("ELI39962", "Failed to apply new page resolution.",
				kRecPutImgArea(0, &imgInfo, pBitmap, hNewImagePage, 0, 0, NULL));

			THROW_UE_ON_ERROR("ELI39975", "Failed to update page.",
				kRecUpdateImgFilePage(0, hNewImagePage, II_CURRENT, hImage, nPage, imgFormat));

			return true;
		}
		else
		{
			// Unless the unmodified pages are updated as well, they end up corrupted in the output.
			THROW_UE_ON_ERROR("ELI39974", "Failed to update page.",
				kRecUpdateImgFilePage(0, hImagePage, II_CURRENT, hImage, nPage, imgFormat));

			return false;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI39968");
}
//-------------------------------------------------------------------------------------------------
void CResolutionNormalizerApp::finalizeAndValidateOutput(const string& strFileName, int nExpectedPageCount)
{
	try
	{
		// Updating existing pages using the Nuance API leaves the old version of pages in the file in a
		// deleted state. The pack call here is necessary to remove those.
		THROW_UE_ON_ERROR("ELI39955", "Failed to finalize image file.",
			kRecPackImgFile(0, strFileName.c_str()));

		HIMGFILE hImage;
		THROW_UE_ON_ERROR("ELI39956", "Unable to open image file.",
			kRecOpenImgFile(strFileName.c_str(), &hImage, IMGF_READ, FF_SIZE));

		// Ensure that the memory stored for the image file is released
		RecMemoryReleaser<tagIMGFILEHANDLE> imageFileMemoryReleaser(hImage);

		int nPageCount = 0;
		THROW_UE_ON_ERROR("ELI39957", "Unable to get page count.",
			kRecGetImgFilePageCount(hImage, &nPageCount));

		if (nPageCount != nExpectedPageCount)
		{
			UCLIDException ue("ELI39958",
				"Page(s) failed to process correctly; normalization aborted");
			ue.addDebugInfo("Expected pages", nExpectedPageCount);
			ue.addDebugInfo("Found pages", nPageCount);
			throw ue;
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI40379");
}
//-------------------------------------------------------------------------------------------------
void CResolutionNormalizerApp::usage(const string& strError)
{
	string strUsage = strError.empty()
		? ""
		: strError + "\r\n------------\r\n";

	strUsage += "ResolutionNormalizer2.exe <Filename> [<ResolutionFactor>] [/ef <ExceptionFile>]\r\n"
		"Filename: Name of image file where disproportional image resolutions\r\n"
		"     (where one axis has a higher DPI than the other) will be normalized\r\n"
		"     such that both the horizontal and vertical DPI ends up matching the\r\n"
		"     the greater of the two on the input image. The image will be\r\n"
		"     modified in-place.\r\n"
		"ResolutionFactor: A floating point value indicating how many times\r\n"
		"     greater one axis's DPI must be than the other before the page DPI is\r\n"
		"     normalized. If not specified, the default is 1.5.\r\n"
		"/ef <ExceptionFile>: Log exceptions to the specified file rather than\r\n"
		"     display them\r\n\r\n"
		"NOTE: This utility does not support PDF files.\r\n";
		AfxMessageBox(strUsage.c_str(), strError.empty()
			? (MB_OK | MB_ICONINFORMATION)
			: (MB_OK | MB_ICONERROR));
}
//-------------------------------------------------------------------------------------------------
void CResolutionNormalizerApp::validateLicense()
{
	// Requires Flex Index/ID Shield core license
	VALIDATE_LICENSE(gnFLEXINDEX_IDSHIELD_CORE_OBJECTS, "ELI39966", "ResolutionNormalizer");
	// Need to be licensed for the Nuance engine as well.
	VALIDATE_LICENSE(gnOCR_ON_CLIENT_FEATURE, "ELI39967", "ResolutionNormalizer");
}