// DocumentSorterConsoleApp.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"

#include <LicenseMgmt.h>
#include <UCLIDException.h>
#include <cpputil.h>
#include <ComponentLicenseIDs.h>

#include <iostream>

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
void printUsage()
{
	cout << "This application requires 4 arguments:" << endl;
	cout << " arg1: Industry Category Name. eg. County Document." << endl;
	cout << " arg2: Input root directory containing all image files to process" << endl;
	cout << " arg3: Output root directory to store documents after classification." << endl;
	cout << " arg4: Indicate whether or not all the images need to be OCRed first.\r\n" 
			"		Use 1 if OCR is required. 0 otherwise" << endl;
	cout << endl;
}
//-------------------------------------------------------------------------------------------------
void validateLicense()
{
	static const unsigned long THIS_APP_ID = gnFLEXINDEX_SERVER_OBJECTS;

	VALIDATE_LICENSE(THIS_APP_ID, "ELI07383", "Document Sort Console App" );
}
//-------------------------------------------------------------------------------------------------
int main(int argc, char* argv[])
{
	try
	{
		cout << "Document Sorter Utility" << endl;
		cout << "Copyright 2010, Extract Systems, LLC." << endl;
		cout << "All rights reserved." << endl;
		cout << endl;
		
		// ensure correct number of parameters.
		if (argc != 5)
		{
			printUsage();
			return 0;
		}
		
		// get the first parameter -- the input root directory containing
		// all image files for processing
		string strIndustryCategoryName(argv[1]);
		string strInputRootDir(argv[2]);
		string strOuputRootDir(argv[3]);
		string strOCRRequired(argv[4]);

		VARIANT_BOOL bOCRRequired = strOCRRequired == "1" ? VARIANT_TRUE : VARIANT_FALSE;

		CoInitializeEx(NULL, COINIT_MULTITHREADED);
		{
			try
			{
				// Load license file(s)
				LicenseManagement::loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);

				// Check license
				validateLicense();

				// Create the Document Sorter object
				IDocumentSorterPtr ipDocSorter(CLSID_DocumentSorter);
				ASSERT_RESOURCE_ALLOCATION("ELI06283", ipDocSorter != __nullptr);

				// Do the sorting
				ipDocSorter->SortDocuments(_bstr_t(strInputRootDir.c_str()),
					_bstr_t(strOuputRootDir.c_str()), 
					_bstr_t(strIndustryCategoryName.c_str()),
					bOCRRequired);
			}
			CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI15786")
		}
		CoUninitialize();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI06282")
		
	return 0;
}
//-------------------------------------------------------------------------------------------------
