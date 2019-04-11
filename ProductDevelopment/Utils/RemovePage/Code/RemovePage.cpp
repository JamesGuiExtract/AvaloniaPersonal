// RemoveImagePage.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "RemovePage.h"


#include <l_bitmap.h>
#include <cpputil.h>
#include <io.h>
#include <UCLIDException.h>
#include <MiscLeadUtils.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <LeadToolsLicenseRestrictor.h>

#include <vector>
#include <string>
using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

using namespace std;

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
void RemovePageFromImage ( string strFileName )
{

	string strExt = getExtensionFromFullPath(strFileName, true);
	if ( strExt != ".tif")
	{
		UCLIDException ue("ELI08562", "File is not a TIF image" );
		ue.addDebugInfo("Filename", strFileName );
		throw ue;
	}
	
	
	HBITMAPLIST hFileBitmaps;
	FILEINFO fileInfo;
	fileInfo.Flags = 0;
	int err;
	
	LeadToolsLicenseRestrictor leadToolsLicenseGuard;
	// Load image
	err = L_LoadBitmapList( _bstr_t(strFileName.c_str()), &hFileBitmaps, 0, 0, NULL, &fileInfo );
	if ( err != SUCCESS )
	{
		UCLIDException ue("ELI08553", "Unable to load image file" );
		ue.addDebugInfo("FileName", strFileName );
		throw ue;
	}
	
	// Delete 1st page
	err = L_DeleteBitmapListItems( hFileBitmaps, 0,1 );
	if ( err != SUCCESS )
	{
		UCLIDException ue("ELI08554", "Unable to remove page" );
		ue.addDebugInfo("FileName", strFileName );
		throw ue;
	}

	// create save file name with _t appended to it
	string strSaveFileName = getPathAndFileNameWithoutExtension( strFileName ) + "_t.tif";
	
	// Save image in the same file
	err = L_SaveBitmapList(_bstr_t(strSaveFileName.c_str()), hFileBitmaps, 
		fileInfo.Format, 0, PQ1, NULL);
	if ( err != SUCCESS )
	{
		UCLIDException ue("ELI08555", "Unable to save image file" );
		ue.addDebugInfo("FileName", strSaveFileName );
		throw ue;
	}
	// Clean up
	err = L_DestroyBitmapList(hFileBitmaps);
	if ( err != SUCCESS )
	{
		UCLIDException ue("ELI08556", "Unable to destroy bitmap handle" );
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void validateLicense()
{
	VALIDATE_LICENSE(gnFLEXINDEX_IDSHIELD_SERVER_CORE, "ELI46651", "RemovePage");
}
//-------------------------------------------------------------------------------------------------
int main(int argc, char *argv[])
{
	try
	{
		// print banner
		cout << "Remove Image Page Utility" << endl;
		cout << "Copyright 2004, UCLID Software, LLC." << endl;
		cout << "All rights reserved." << endl;
		cout << endl;

		// Load license files ( this is need for IVariantVector )
		LicenseManagement::loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);
		validateLicense();

		InitLeadToolsLicense();

		// check for correct # of arguments
		if (argc != 2 )
		{
			cout << "ERROR: Incorrect usage!" << endl;

			cout << "This utility takes a tif image file name as an argument" << endl;
			cout << "The image file will be copied to a file of the same name" << endl;
			cout << "with _t appended before the extension." << endl;
						
			return EXIT_FAILURE;
		}

		CoInitializeEx(NULL, COINIT_MULTITHREADED);

		// get the root directory
		string strRootDir = argv[1];
		// make filename with a full path; if it doesn't have a path make it the current dir
		string strFileName = getAbsoluteFileName( strRootDir, strRootDir, true);

		RemovePageFromImage( strFileName );
		return EXIT_SUCCESS;
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI08557")

	CoUninitialize();

		// if we reached here, it's because an exception was caught.
	cout << endl;
	cout << "NOTE: Process terminated prematurely because of error condition!" << endl;
	cout << endl;
	return EXIT_FAILURE;
}