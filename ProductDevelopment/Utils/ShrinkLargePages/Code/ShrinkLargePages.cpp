// ShrinkLargePages.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "ShrinkLargePages.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <ComUtils.h>
#include <UCLIDExceptionDlg.h>
#include <cpputil.h>
#include <TemporaryFileName.h>
#include <l_bitmap.h>		// LeadTools Imaging library
#include <ComponentLicenseIDs.h>
#include <MiscLeadUtils.h>
#include <LeadToolsFormatHelpers.h>
#include <LeadToolsBitmapFreeer.h>

#include <vector>
#include <string>
#include <LeadToolsLicenseRestrictor.h>

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// CShrinkLargePagesApp
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CShrinkLargePagesApp, CWinApp)
	ON_COMMAND(ID_HELP, &CWinApp::OnHelp)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CShrinkLargePagesApp construction
//-------------------------------------------------------------------------------------------------
CShrinkLargePagesApp::CShrinkLargePagesApp()
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}

//-------------------------------------------------------------------------------------------------
// The one and only CShrinkLargePagesApp object
//-------------------------------------------------------------------------------------------------
CShrinkLargePagesApp theApp;

//-------------------------------------------------------------------------------------------------
void shrinkLargeImages(const string& strImageFileName, int nMaxSize, bool bForceImageUpdate, L_INT nScalingMethod)
{
	INIT_EXCEPTION_AND_TRACING("MLI3332");
	try
	{
		{
			// Retrieve file information - page count and file format
			FILEINFO fileInfo;
			getFileInformation(strImageFileName, true, fileInfo);
			_lastCodePos = "20";

			int nPageCount = fileInfo.TotalPages;
			int format = fileInfo.Format;

			bool bNeedsAdjustment = bForceImageUpdate;

			if (!bNeedsAdjustment)
			{
				_lastCodePos = "30";

				LOADFILEOPTION lfo = GetLeadToolsSizedStruct<LOADFILEOPTION>(0);
				for (int nPage = 1; !bNeedsAdjustment && nPage <= nPageCount; nPage++)
				{
					if (nPage > 1)
					{
						lfo.PageNumber = nPage;
						getFileInformation(strImageFileName, false, fileInfo, &lfo);
					}

					if (max(fileInfo.Width, fileInfo.Height) > nMaxSize)
					{
						bNeedsAdjustment = true;
						break;
					}
				}
			}
			
			_lastCodePos = "40";

			// If no page needs to be adjusted, we are done.
			if (!bNeedsAdjustment)
			{
				return;
			}

			// Specify appropriate compression level for file types
			// which support this setting. [LRCAU #5189]
			L_INT nCompression = getCompressionFactor(fileInfo.Format);

			int iRetryCount(0), iRetryTimeout(0);
			getFileAccessRetryCountAndTimeout(iRetryCount, iRetryTimeout);
			_lastCodePos = "50";

			// Create temporary file to store output if image is to be overwritten
			// Use ProcID_ThreadID as prefix - to minimize thread-related problems
			string strProc = getCurrentProcessID();
			string strThread = asString(GetCurrentThreadId());
			string strPrefix = strProc.c_str() + string("_") + strThread.c_str();
			TemporaryFileName tfn(true, strPrefix.c_str());
			_lastCodePos = "60";

			// Get initialized LOADFILEOPTION struct. 
			LOADFILEOPTION lfo = GetLeadToolsSizedStruct<LOADFILEOPTION>(ELO_SIGNED);
			_lastCodePos = "70";

			// Get initialized SAVEFILEOPTION struct
			SAVEFILEOPTION sfo = GetLeadToolsSizedStruct<SAVEFILEOPTION>(0);
			{
				LeadToolsLicenseRestrictor leadToolsLicenseGuard;
				L_INT nRet = L_GetDefaultSaveFileOption(&sfo, sizeof(SAVEFILEOPTION));
				throwExceptionIfNotSuccess(nRet, "ELI51412", "Failed getting default save options!");
			}

			// Load, adjust, and save the file - one page at a time
			for (int i = 1; i <= nPageCount; i++)
			{
				string strPage = asString(i);

				// Set page numbers to load and save
				lfo.PageNumber = i;
				sfo.PageNumber = i;

				// Load the image page
				BITMAPHANDLE bmh;
				LeadToolsBitmapFreeer freer(bmh);
				loadImagePage(strImageFileName, bmh, fileInfo, lfo, false);

				_lastCodePos = "75_B_" + strPage;

				int nCurrentSize = max(bmh.Width, bmh.Height);
				if (nCurrentSize > nMaxSize)
				{
					double scale = (double)nMaxSize / nCurrentSize;
					int newWidth = min(nMaxSize, (int)round(bmh.Width * scale));
					int newHeight = min(nMaxSize, (int)round(bmh.Height * scale));

					throwExceptionIfNotSuccess(
						L_SizeBitmap(&bmh, newWidth, newHeight, nScalingMethod)
						, "ELI51411", "Image processing error");
				}
				saveImagePage(bmh, tfn.getName(), format, nCompression, bmh.BitsPerPixel, sfo);

				_lastCodePos = "75_D_" + strPage;
			} // end for each page
			_lastCodePos = "80";

			// Wait for the file to finish writing
			waitForFileToBeReadable(tfn.getName());
			_lastCodePos = "90";

			// Overwrite original file
			copyFile(tfn.getName(), strImageFileName);
			_lastCodePos = "100";
		} // End scope for input PDF manager
		_lastCodePos = "110";
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI51413");

	// Wait for the final output file to be readable
	waitForFileToBeReadable(strImageFileName);

	return;
}
//-------------------------------------------------------------------------------------------------
void usage()
{
	string strUsage = "ShrinkLargePages.exe <Image> <MaxSize> [/force] [/method m] [/ef logfilename]\n\n";
	strUsage +=
		"Image: An image file (.tif or other) and \n\n"
		"MaxSize: A number representing he maximum number of pixels on any axis of the image. "
		"If either the width or height is greater, the image will be resized "
		"such that the largest dimension does not exceed MaxSize.\n\n"
		"/force:  If specified, all image pages will be regenerated regardless of size.\n\n"
		"/method: If specified, m is an integer representing the interpolation method used for resizing.\n"
		"   0 = SIZE_NORMAL (fast but poor quality)\n"
		"   1 = SIZE_FAVORBLACK\n"
		"   2 = SIZE_RESAMPLE (default)\n"
		"   4 = SIZE_BICUBIC\n"
		"   8 = SIZE_SCALETOGRAY\n"
		"  16 = SIZE_OLD_RESAMPLE\n"
		"  32 = SIZE_PREMULTIPLYALPHA\n";
		AfxMessageBox(strUsage.c_str());
}
//-------------------------------------------------------------------------------------------------
void validateLicense()
{
	static const unsigned long THIS_APP_ID = gnFLEXINDEX_IDSHIELD_SERVER_CORE;

	VALIDATE_LICENSE(THIS_APP_ID, "ELI51415", "ShrinkLargePages" );
}

//-------------------------------------------------------------------------------------------------
// CShrinkLargePagesApp initialization
//-------------------------------------------------------------------------------------------------
BOOL CShrinkLargePagesApp::InitInstance()
{
	AfxEnableControlContainer();

	CoInitializeEx(NULL, COINIT_MULTITHREADED);
	string strExceptionLogFile = "";

	try
	{
		try
		{
			// Setup exception handling
			static UCLIDExceptionDlg exceptionDlg;
			UCLIDException::setExceptionHandler(&exceptionDlg);

			vector<string> vecParams;
			for (int i = 0; i < __argc; i++)
			{
				vecParams.push_back(__argv[i]);
			}

			if (vecParams.size() < 3)
			{
				usage();
				return FALSE;
			}

			// Retrieve input file and resolution parameters
			string strImageName = vecParams[1];
			int nMaxSize = asLong(vecParams[2]);
			bool bForceImageUpdate = false;
			L_INT nScalingMethod = SIZE_RESAMPLE;

			for (size_t i = 3; i < vecParams.size(); i++)
			{
				string strParam = vecParams[i];
				if (_strcmpi(strParam.c_str(), "/force") == 0)
				{
					bForceImageUpdate = true;
				}
				else if (_strcmpi(strParam.c_str(), "/method") == 0 && vecParams.size() > i + 1)
				{
					i++;
					nScalingMethod = asLong(vecParams[i]);
				}
				else if (_strcmpi(strParam.c_str(), "/ef") == 0 && vecParams.size() > i + 1)
				{
					// if an exception log file is specified, process it first
					// so that any exceptions that are thrown after this point will
					// be logged and not displayed
					i++;
					strExceptionLogFile = vecParams[i];
				}
				else
				{
					usage();
					return FALSE;
				}
			}

			// Make sure the image file exists
			::validateFileOrFolderExistence(strImageName);

			// Load license files and validate the license
			LicenseManagement::loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);
			validateLicense();

			InitLeadToolsLicense();

			// Set the image resolution
			shrinkLargeImages(strImageName, nMaxSize, bForceImageUpdate, nScalingMethod);
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI51414");
	}
	catch (UCLIDException& ue)
	{
		// check for exception log file specified
		if (!strExceptionLogFile.empty())
		{
			// log the exception to the specified file
			ue.log(strExceptionLogFile);
		}
		else
		{
			// no log file specified so display the exception
			ue.log();
		}
	}

	CoUninitialize();

	// Since the dialog has been closed, return FALSE so that we exit the
	//  application, rather than start the application's message pump.
	return TRUE;
}
//-------------------------------------------------------------------------------------------------
