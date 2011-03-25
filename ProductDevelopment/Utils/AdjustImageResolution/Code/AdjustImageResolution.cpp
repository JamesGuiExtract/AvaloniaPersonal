// AdjustImageResolution.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "AdjustImageResolution.h"
#include "AdjustImageResolutionDlg.h"

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

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// CAdjustImageResolutionApp
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CAdjustImageResolutionApp, CWinApp)
	ON_COMMAND(ID_HELP, &CWinApp::OnHelp)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CAdjustImageResolutionApp construction
//-------------------------------------------------------------------------------------------------
CAdjustImageResolutionApp::CAdjustImageResolutionApp()
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}

//-------------------------------------------------------------------------------------------------
// The one and only CAdjustImageResolutionApp object
//-------------------------------------------------------------------------------------------------
CAdjustImageResolutionApp theApp;

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
bool needsAdjustment(const FILEINFO &fileInfo, int nNewXResolution, int nNewYResolution, 
					 int nMaxHeight, int nMaxWidth)
{
	// Check actual width and height if a non-zero maximum is defined
	if (nMaxHeight > 0 || nMaxWidth > 0)
	{
		// No need to adjust file if image is smaller than maximum size
		double dActualWidth  = (double)fileInfo.Width  / (double)fileInfo.XResolution;
		double dActualHeight = (double)fileInfo.Height / (double)fileInfo.YResolution;
		if (dActualWidth < nMaxWidth && dActualHeight < nMaxHeight)
		{
			return false;
		}
	}

	// File needs adjustment if resolution doesn't match desired
	return (fileInfo.XResolution != nNewXResolution || fileInfo.YResolution != nNewYResolution);
}
//-------------------------------------------------------------------------------------------------
// TODO: Remove this function and its use in favor of ImageUtils method
void setImageResolution(const string& strImageFileName, int nNewXResolution, int nNewYResolution, 
						int nMaxHeight, int nMaxWidth)
{
	INIT_EXCEPTION_AND_TRACING("MLI02780");
	try
	{
		// Scope for the input PDF manager
		{
			// Retrieve file information - page count and file format
			FILEINFO fileInfo;
			getFileInformation(strImageFileName, true, fileInfo);
			_lastCodePos = "20";

			int nPageCount = fileInfo.TotalPages;
			int format = fileInfo.Format;

			// Ensure at least one page needs adjustment [LegacyRCAndUtils #5009]
			bool bNeedsAdjustment = needsAdjustment(fileInfo, nNewXResolution, nNewYResolution, 
				nMaxHeight, nMaxWidth);
			_lastCodePos = "30";

			LOADFILEOPTION lfo = GetLeadToolsSizedStruct<LOADFILEOPTION>(0);
			for (int i = 2; !bNeedsAdjustment && i <= nPageCount; i++)
			{
				// Set page number
				lfo.PageNumber = i;
				getFileInformation(strImageFileName, false, fileInfo, &lfo);

				// Check if this page needs adjustment
				bNeedsAdjustment = needsAdjustment(fileInfo, nNewXResolution, nNewYResolution,
					nMaxHeight, nMaxWidth);
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
			string strThread = asString( GetCurrentThreadId() );
			string strPrefix = strProc.c_str() + string( "_" ) + strThread.c_str();
			TemporaryFileName tfn( strPrefix.c_str() );
			_lastCodePos = "60";

			// Get initialized LOADFILEOPTION struct. 
			lfo = GetLeadToolsSizedStruct<LOADFILEOPTION>(ELO_SIGNED);
			_lastCodePos = "70";

			// Get initialized SAVEFILEOPTION struct
			SAVEFILEOPTION sfo = GetLeadToolsSizedStruct<SAVEFILEOPTION>(0);
			L_INT nRet = L_GetDefaultSaveFileOption(&sfo, sizeof(SAVEFILEOPTION));
			throwExceptionIfNotSuccess(nRet, "ELI25294", "Failed getting default save options!");

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

				// Adjust the resolution
				bmh.XResolution = nNewXResolution;
				bmh.YResolution = nNewYResolution;

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
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI25292");

	// Wait for the final output file to be readable
	waitForFileToBeReadable(strImageFileName);

	return;
}
//-------------------------------------------------------------------------------------------------
void usage()
{
	string strUsage = "This application has 3 required arguments and an additional pair of optional arguments:\n";
		strUsage += "An image file (.tif or other) and \n"
					"a desired X resolution in dots per inch and \n"
					"a desired Y resolution in dots per inch and \n"
					"a maximum height in inches and \n"
					"a maximum width in inches.\n\n";
		strUsage += "Maximum height and width must be used together, if neither are \n"
					"present, the image resolutions are applied.  If both are \n"
					"present and the image is smaller than both specified maximums, \n"
					"the image resolution is not modified.\n\n";
		strUsage += "Usage:\n";
		strUsage += "AdjustImageResolution.exe <strImage> <nXResolution> <nYResolution> -Hn -Wn\n"
					"where:\n"
					"n is an integer number of inches.\n\n";
		AfxMessageBox(strUsage.c_str());
}
//-------------------------------------------------------------------------------------------------
void validateLicense()
{
	static const unsigned long THIS_APP_ID = gnFLEXINDEX_IDSHIELD_SERVER_CORE;

	VALIDATE_LICENSE(THIS_APP_ID, "ELI13409", "AdjustImageResolution" );
}

//-------------------------------------------------------------------------------------------------
// CAdjustImageResolutionApp initialization
//-------------------------------------------------------------------------------------------------
BOOL CAdjustImageResolutionApp::InitInstance()
{
	AfxEnableControlContainer();

	CoInitializeEx(NULL, COINIT_MULTITHREADED);
	try
	{
		// Define default image sizes
		int nMaxHeight = -1;
		int nMaxWidth = -1;

		// Setup exception handling
		static UCLIDExceptionDlg exceptionDlg;
		UCLIDException::setExceptionHandler( &exceptionDlg );

		vector<string> vecParams;
		int i;
		for (i = 0; i < __argc; i++)
		{
			vecParams.push_back( __argv[i]);
		}

		// Make sure the number of parameters either 4 or 6
		unsigned int uiParamCount = (unsigned int)vecParams.size();
		if ((uiParamCount != 4) && (uiParamCount != 6))
		{
			usage();
			return FALSE;
		}

		// Retrieve input file and resolution parameters
		string strImageName = vecParams[1];
		int nXResolution = asLong( vecParams[2] );
		int nYResolution = asLong( vecParams[3] );

		// Check for maximum size parameters
		if (uiParamCount == 6)
		{
			for (i = 4; i < 6; i++)
			{
				// Retrieve the parameter
				string strParam = vecParams[i];

				// Check for height
				if (strParam.find( "-H" ) != string::npos)
				{
					// Extract the value
					strParam.erase( 0, 2 );
					nMaxHeight = asLong( strParam );
				}
				// Check for width
				else if (strParam.find( "-W" ) != string::npos)
				{
					// Extract the value
					strParam.erase( 0, 2 );
					nMaxWidth = asLong( strParam );
				}
			}

			// Confirm that both have been defined
			if ((nMaxHeight == -1) || (nMaxWidth == -1))
			{
				usage();
				return FALSE;
			}
		}

		// Make sure the image file exists
		::validateFileOrFolderExistence( strImageName );

		// Load license files and validate the license
		LicenseManagement::sGetInstance().loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);
		validateLicense();

		// Set the image resolution
		setImageResolution(strImageName, nXResolution, nYResolution, nMaxHeight, nMaxWidth);
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI13408");
	CoUninitialize();

	// Since the dialog has been closed, return FALSE so that we exit the
	//  application, rather than start the application's message pump.
	return TRUE;
}
//-------------------------------------------------------------------------------------------------
