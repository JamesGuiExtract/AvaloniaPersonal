// ESOCR.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "ESOCR.h"

#include <LicenseMgmt.h>
#include <UCLIDExceptionDlg.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <ComUtils.h>
#include <MiscLeadUtils.h>
#include <ComponentLicenseIDs.h>

#include <io.h>

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
// Max image height in inches
const double g_dMAX_IMAGE_HEIGHT	= 8.0;

// Max image width in inches
const double g_dMAX_IMAGE_WIDTH		= 8.5;

//-------------------------------------------------------------------------------------------------
// CESOCRApp
//-------------------------------------------------------------------------------------------------
BEGIN_MESSAGE_MAP(CESOCRApp, CWinApp)
	//{{AFX_MSG_MAP(CESOCRApp)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG
	ON_COMMAND(ID_HELP, CWinApp::OnHelp)
END_MESSAGE_MAP()

//-------------------------------------------------------------------------------------------------
// CESOCRApp construction
//-------------------------------------------------------------------------------------------------
CESOCRApp::CESOCRApp()
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}

//-------------------------------------------------------------------------------------------------
// The one and only CESOCRApp object
CESOCRApp theApp;

//-------------------------------------------------------------------------------------------------
void validateLicense()
{
	static const unsigned long THIS_APP_ID = gnOCR_ON_CLIENT_FEATURE;

	VALIDATE_LICENSE(THIS_APP_ID, "ELI12637", "ESOCR" );
}

//-------------------------------------------------------------------------------------------------
// CESOCRApp initialization
//-------------------------------------------------------------------------------------------------
BOOL CESOCRApp::InitInstance()
{
	// Initialize flags
	m_bNoError = true;
	bool bDisplayExceptions = true;

	// Set up the exception handling aspect.
	static UCLIDExceptionDlg exceptionDlg;
	UCLIDException::setExceptionHandler( &exceptionDlg );			

	CoInitializeEx(NULL, COINIT_MULTITHREADED);

	try
	{
		try
		{
			// Check command-line items
			if ((__argc < 3) || (__argc > 4))
			{
				displayUsage();
				return FALSE;
			}

			// Modified as per [LegacyRCAndUtils #4986]
			// Retrieve path to input image
			string strImagePath = buildAbsolutePath(__argv[1]);

			// Modified as per [LegacyRCAndUtils #4986]
			// Retrieve path to output text file
			string strTextPath = buildAbsolutePath(__argv[2]);

			// Check for additional options
			if ((__argc > 3))
			{
				if (_strcmpi(__argv[3], "/se") == 0)
				{
					// Suppress display of exceptions
					bDisplayExceptions = false;
				}
				else
				{
					// Display usage
					displayUsage();
					return FALSE;
				}
			}

			// Initialize and check license
			LicenseManagement::sGetInstance().loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);

			// Check license
			validateLicense();

			// Validate image path
			if (!isFileOrFolderValid( strImagePath ))
			{
				UCLIDException ue( "ELI12929", "Invalid path to image file." );
				ue.addDebugInfo( "Input Image Path", strImagePath );
				throw ue;
			}

			// Validate write access to text path
			if (!canCreateFile( strTextPath ))
			{
				// Create and throw exception
				UCLIDException ue( "ELI12639", "Cannot create output file.");
				ue.addDebugInfo( "Output File Path", strTextPath );
				throw ue;
			}

			// Validate image for size
			validateImage(strImagePath);

			// Prepare OCR Engine
			prepareOCREngine();

			// OCR the image
			ISpatialStringPtr ipText = m_ipOCRUtils->RecognizeTextInImageFile(
				strImagePath.c_str(), 1, m_ipOCREngine, NULL);
			ASSERT_RESOURCE_ALLOCATION("ELI16154", ipText != __nullptr);

			// Write the text to the output file
			ipText->SaveTo( strTextPath.c_str(), VARIANT_TRUE, VARIANT_TRUE );
		
			// Cleanup objects
			m_ipOCRUtils = __nullptr;
			m_ipOCREngine = __nullptr;
			ipText = __nullptr;
		}
		// Modified as per [LegacyRCAndUtils #4986]
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI21595");
	}
	catch(UCLIDException& ue)
	{
		// set the no error flag to false
		m_bNoError = false;

		m_ipOCREngine = __nullptr;
		m_ipOCRUtils = __nullptr;

		if (bDisplayExceptions)
		{
			ue.display();
		}
		else
		{
			ue.log();
		}
	}

	CoUninitialize();

	return FALSE;
}
//-------------------------------------------------------------------------------------------------
int CESOCRApp::ExitInstance() 
{
	// Check results
	if (m_bNoError)
	{
		return EXIT_SUCCESS;
	}
	else
	{
		return EXIT_FAILURE;
	}
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
void CESOCRApp::prepareOCREngine()
{
	// Create OCR engine object
	m_ipOCREngine.CreateInstance( CLSID_ScansoftOCR );
	ASSERT_RESOURCE_ALLOCATION( "ELI12648", m_ipOCREngine != __nullptr );

	// Initialize the private OCR engine license
	_bstr_t _bstrPrivateLicenseCode = get_bstr_t(LICENSE_MGMT_PASSWORD.c_str());
	IPrivateLicensedComponentPtr ipScansoftEngine( m_ipOCREngine );
	ipScansoftEngine->InitPrivateLicense( _bstrPrivateLicenseCode );

	// Create OCRUtils object
	m_ipOCRUtils.CreateInstance( CLSID_OCRUtils );
	ASSERT_RESOURCE_ALLOCATION( "ELI12649", m_ipOCRUtils != __nullptr );
}
//-------------------------------------------------------------------------------------------------
void CESOCRApp::validateImage(const string& strImagePath)
{
	// Get image resolution
	int nXRes(0), nYRes(0);
	getImageXAndYResolution( strImagePath, nXRes, nYRes);

	// Get image page count
	int nPages = getNumberOfPagesInImage( strImagePath );

	// Get image height and width
	int nHeight(0), nWidth(0);
	getImagePixelHeightAndWidth(strImagePath, nHeight, nWidth);

	// Check single-page image
	if (nPages > 1)
	{
		// Create and throw exception
		UCLIDException ue("ELI12645", "Must be single-page image.");
		ue.addDebugInfo( "Image", strImagePath );
		ue.addDebugInfo( "Page Count", nPages );
		throw ue;
	}

	// Image height <= 8 inches
	double dPixelLimit = g_dMAX_IMAGE_HEIGHT * nYRes;
	if ((double)nHeight > dPixelLimit)
	{
		// Create and throw exception
		UCLIDException ue("ELI12646", "Image is too tall.");
		ue.addDebugInfo( "Image", strImagePath );
		ue.addDebugInfo( "Max Height", g_dMAX_IMAGE_HEIGHT );

		double dHeight = (double)nHeight / (double)nYRes;
		ue.addDebugInfo( "Image Height", dHeight );
		throw ue;
	}

	// Image width <= 8.5 inches
	dPixelLimit = g_dMAX_IMAGE_WIDTH * nXRes;
	if ((double)nWidth > dPixelLimit)
	{
		// Create and throw exception
		UCLIDException ue("ELI12647", "Image is too wide.");
		ue.addDebugInfo( "Image", strImagePath );
		ue.addDebugInfo( "Max Width", g_dMAX_IMAGE_WIDTH );

		double dWidth = (double)nWidth / (double)nXRes;
		ue.addDebugInfo( "Image Width", dWidth );
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void CESOCRApp::displayUsage()
{
	// Display error message with usage
	CString	zUsage = "ESOCR Usage:\r\narg1 = input image filename\r\narg2 = output text filename\r\noptional arg3 = /se to suppress exception display";
	MessageBox( NULL, zUsage, "Error", MB_OK | MB_ICONERROR );
	m_bNoError = false;
}
//-------------------------------------------------------------------------------------------------
