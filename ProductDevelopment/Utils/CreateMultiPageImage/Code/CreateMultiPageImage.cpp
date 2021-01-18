// CreateMultiPageImage.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "CreateMultiPageImage.h"

#include <l_bitmap.h>
#include <cpputil.h>
#include <UCLIDException.h>
#include <MiscLeadUtils.h>
#include <StringTokenizer.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <CsisUtils.h>

#include <vector>
#include <string>
#include <map>
#include <io.h>

using namespace std;

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// add license management password function
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

int giTotalImagesProcessed = 0;
int giImagesCreated = 0;
int giImagesFailed = 0;

// enumeration to capture the various filename types in which single-page images
// can be stored in a folder
enum EFileNameType
{
	kNone,
	kName_Dot_DDD_Dot_Tif,		// 0000123.001.tif, 0000123.002.tif, etc
	// NOTE: The order is very important when searching for the 5 digit, 4 digit, and 3 digit ext
	kName_Dot_DDDDD,				// 000123.00001
	kName_Dot_DDDD,					// 000123.0001
	kName_Dot_DDD,				// 0000123.001, 0000123.002, etc
	kName_Hypen_Ds_Dot_Tif,		// 0000123-1.tif, 0000123-2.tif
	kName_Underscore_P_Ds_Dot_Tif,	// 0000123_P1.tif, 0000123_P2.tif, etc
	kName_Underscore_DDDD_Dot_Tif,  // ImgA_0001.tif, ImgA_0002.tif, etc
	kDDDDDDDD_Dot_Tif,				// 00000001.tif, 00000002.tif, etc
	// NOTE: add new future entries here...
	kLastFileNameType
};

//-------------------------------------------------------------------------------------------------
// PURPOSE: To return the files in strRootDir that represent the names of first pages
// of a document.
EFileNameType getFirstPageFiles(const string& strRootDir, bool bRecursive,
								vector<string>& rvecFiles)
{
	// create map of file-name-type's to the appropriate wild cards for searching
	static map<EFileNameType, string> ls_mapFileNameTypeToWildCard;
	if (ls_mapFileNameTypeToWildCard.empty())
	{
		ls_mapFileNameTypeToWildCard[kName_Dot_DDD_Dot_Tif] = "*.001.tif";
		ls_mapFileNameTypeToWildCard[kName_Dot_DDD] = "*.001";
		ls_mapFileNameTypeToWildCard[kName_Hypen_Ds_Dot_Tif] = "*-1.tif";
		ls_mapFileNameTypeToWildCard[kName_Underscore_P_Ds_Dot_Tif] = "*_P1.tif";
		ls_mapFileNameTypeToWildCard[kName_Underscore_DDDD_Dot_Tif] = "*_0001.tif";
		ls_mapFileNameTypeToWildCard[kDDDDDDDD_Dot_Tif] = "00000001.tif";
		ls_mapFileNameTypeToWildCard[kName_Dot_DDDD] = "*.0001";
		ls_mapFileNameTypeToWildCard[kName_Dot_DDDDD] = "*.00001";
	}

	// iterate through each of the various filename types and see if files matching
	// the appropriate wild card specs can be found
	for (int i = 1; i < kLastFileNameType; i++)
	{
		// clear the results vector
		rvecFiles.clear();
		getFilesInDir(rvecFiles, strRootDir, 
			ls_mapFileNameTypeToWildCard[(EFileNameType) i], bRecursive);
		
		// if at least one file was found, then return
		if (!rvecFiles.empty())
		{
			return (EFileNameType) i;
		}
	}

	return kNone;
}
//-------------------------------------------------------------------------------------------------
void printUsage()
{
	cout << "This utility creates multi-page images from single-page images." << endl;
	cout << "The single page images are expected to be named as described below." << endl;
	cout << "The starting page number is required to be 1." << endl;
	cout << endl;
	cout << "This application operates with either 2, 3, 5 or 6 arguments:" << endl;
	cout << " Filename and folder based usage:" << endl;
	cout << "	arg1: Root directory for searching (e.g. \"C:\\ImageFiles\")" << endl;
	cout << "	arg2: <Optional> Use /s to indicate recursive search." << endl;
	cout << "		    By default, the searches are not recursive." << endl;
	cout << endl;
	cout << "    Sample Input Files                              Output File" << endl;
	cout << "      0000123.001.tif, 0000123.002.tif, etc           0000123.tif" << endl;
	cout << "      0000123.001, 0000123.002, etc                   0000123.tif" << endl;
	cout << "      0000123-1.tif, 0000123-2.tif, etc               0000123.tif" << endl;
	cout << "      0000123_P1.tif, 0000123_P2.tif, etc             0000123.tif" << endl;
	cout << "      ImgA_0001.tif, ImgA_0002.tif, etc               ImgA.tif" << endl;
	cout << "      00000001.tif, 00000002.tif, etc                 a.tif" << endl;
	cout << endl;
	cout << " Input file usage:" << endl;
	cout << "	arg1: Input file name" << endl;
	cout << "	arg2: Delimiter" << endl;
	cout << "	arg3: Output image name column position in the input file" << endl;
	cout << "	arg4: Single page input image name column position in the input file" << endl;
	cout << "	arg5: Output path for completed images" << endl;
	cout << "	[arg6]: Input path to single page image files" << endl;
}
//-------------------------------------------------------------------------------------------------
string getMultiPageImageName(const string& strPage1, const EFileNameType eFileType)
{
	switch (eFileType)
	{
	case kName_Dot_DDD_Dot_Tif:
		{
			// get the location of the last period
			size_t lastDot = strPage1.find_last_of('.');
			if (lastDot == string::npos || lastDot == 0)
			{
				UCLIDException ue("ELI08507", "Image not in expected format!");
				ue.addDebugInfo("strPage1", strPage1);
				ue.addDebugInfo("lastDot", lastDot);
				throw ue;
			}

			// get the location of the period before the last period
			size_t lastToLastDot = strPage1.find_last_of('.', lastDot - 1);
			if (lastToLastDot == string::npos)
			{
				UCLIDException ue("ELI08508", "Image not in expected format!");
				ue.addDebugInfo("strPage1", strPage1);
				ue.addDebugInfo("lastDot", lastDot);
				ue.addDebugInfo("lastToLastDot", lastToLastDot);
				throw ue;
			}

			// compute the root file name and return
			string strRootFileName = strPage1;
			strRootFileName.erase(lastToLastDot, lastDot - lastToLastDot);
			return strRootFileName;
		}
		break;

	case kName_Dot_DDD:
	case kName_Dot_DDDD:
	case kName_Dot_DDDDD:
		{
			// get the location of the last period
			size_t lastDot = strPage1.find_last_of('.');
			if (lastDot == string::npos || lastDot == 0)
			{
				UCLIDException ue("ELI08516", "Image not in expected format!");
				ue.addDebugInfo("strPage1", strPage1);
				ue.addDebugInfo("lastDot", lastDot);
				throw ue;
			}

			// erase the "[0]+1" after the period in "12312321.[0]+1"
			string strRootFileName(strPage1, 0, lastDot);
			
			// append .tif and return
			strRootFileName += ".tif";
			return strRootFileName;
		}
		break;

	case kName_Hypen_Ds_Dot_Tif:
		{
			// get the location of the last period
			size_t lastDot = strPage1.find_last_of('.');
			if (lastDot == string::npos || lastDot == 0)
			{
				UCLIDException ue("ELI08512", "Image not in expected format!");
				ue.addDebugInfo("strPage1", strPage1);
				ue.addDebugInfo("lastDot", lastDot);
				throw ue;
			}

			// get the location of the hyphen before the last period
			size_t lastHypenFromLastDot = strPage1.find_last_of('-', lastDot - 1);
			if (lastHypenFromLastDot == string::npos)
			{
				UCLIDException ue("ELI08513", "Image not in expected format!");
				ue.addDebugInfo("strPage1", strPage1);
				ue.addDebugInfo("lastDot", lastDot);
				ue.addDebugInfo("lastHypenFromLastDot", lastHypenFromLastDot);
				throw ue;
			}

			// compute the root file name and return
			string strRootFileName = strPage1;
			strRootFileName.erase(lastHypenFromLastDot, lastDot - lastHypenFromLastDot);
			return strRootFileName;
		}
		break;

	case kName_Underscore_P_Ds_Dot_Tif:
	case kName_Underscore_DDDD_Dot_Tif:
		{
			// get the location of the last period
			size_t lastDot = strPage1.find_last_of('.');
			if (lastDot == string::npos || lastDot == 0)
			{
				UCLIDException ue("ELI09174", "Image not in expected format!");
				ue.addDebugInfo("strPage1", strPage1);
				ue.addDebugInfo("lastDot", lastDot);
				throw ue;
			}

			// get the location of the underscore before the last period
			size_t lastUnderscoreFromLastDot = strPage1.find_last_of('_', lastDot - 1);
			if (lastUnderscoreFromLastDot == string::npos)
			{
				UCLIDException ue("ELI09175", "Image not in expected format!");
				ue.addDebugInfo("strPage1", strPage1);
				ue.addDebugInfo("lastDot", lastDot);
				ue.addDebugInfo("lastUnderscoreFromLastDot", lastUnderscoreFromLastDot);
				throw ue;
			}

			// compute the root file name and return
			string strRootFileName = strPage1;
			strRootFileName.erase(lastUnderscoreFromLastDot, lastDot - lastUnderscoreFromLastDot);
			return strRootFileName;
		}
		break;

	case kDDDDDDDD_Dot_Tif:
		{
			// Get folder
			string strFolder = getDirectoryFromFullPath( strPage1 );

			// Use a.tif as the filename
			string strRootFileName = strFolder.c_str() + string( "\\" ) + string( "a.tif" );
			return strRootFileName;
		}
		break;

	// throw exception if eFileType is not one of the above
	default:
		{
			UCLIDException ue("ELI08506", "Invalid file type!");
			ue.addDebugInfo("eFileType", (long) eFileType);
			throw ue;
		}
	}

	// we should never reach here
	THROW_LOGIC_ERROR_EXCEPTION("ELI08511")
}
//-------------------------------------------------------------------------------------------------
string getSinglePageImageName(string strMultiPageImageFileName, 
							  EFileNameType eFileType, unsigned long ulPage)
{
	switch (eFileType)
	{
	case kName_Dot_DDD_Dot_Tif:
		{
			// build the 3 digit zero padded page number extension like ".001"
			string strDDDPage = padCharacter(asString(ulPage), true, '0', 3);
			strDDDPage.insert(0, ".");

			// find the last dot in the multi-page tif file
			size_t lastDot = strMultiPageImageFileName.find_last_of('.');
			if (lastDot == string::npos || lastDot == 0)
			{
				UCLIDException ue("ELI08509", "Image not in expected format!");
				ue.addDebugInfo("strImage", strMultiPageImageFileName);
				ue.addDebugInfo("lastDot", lastDot);
				throw ue;
			}

			// insert the ".001" at the location of the "." in "12312321.tif"
			// and return the resulting string
			strMultiPageImageFileName.insert(lastDot, strDDDPage);
			return strMultiPageImageFileName;
		}
		break;

	case kName_Dot_DDD:
	case kName_Dot_DDDD:
	case kName_Dot_DDDDD:
		{
			int nPadCharacterCount = 3;
			if (eFileType == kName_Dot_DDDD)
			{
				nPadCharacterCount = 4;
			}
			else if (eFileType == kName_Dot_DDDDD)
			{
				nPadCharacterCount = 5;
			}

			// build the 3 digit zero padded page number extension like ".[0]+1"
			string strDDDPage = padCharacter(asString(ulPage), true, '0', nPadCharacterCount);

			// find the last dot in the multi-page tif file
			size_t lastDot = strMultiPageImageFileName.find_last_of('.');
			if (lastDot == string::npos || lastDot == 0)
			{
				UCLIDException ue("ELI08515", "Image not in expected format!");
				ue.addDebugInfo("strImage", strMultiPageImageFileName);
				ue.addDebugInfo("lastDot", lastDot);
				throw ue;
			}

			// erase the "tif" extension in "12312321.tif"
			strMultiPageImageFileName.erase(lastDot + 1);
			
			// append the "001" after "12312321." to create the name of the image file's page
			strMultiPageImageFileName += strDDDPage;

			// return the computed string
			return strMultiPageImageFileName;
		}
		break;

	case kName_Hypen_Ds_Dot_Tif:
		{
			// build the page number string without any zero-padding
			string strPage = asString(ulPage);
			strPage.insert(0, "-");

			// find the last dot in the multi-page tif file
			size_t lastDot = strMultiPageImageFileName.find_last_of('.');
			if (lastDot == string::npos || lastDot == 0)
			{
				UCLIDException ue("ELI08514", "Image not in expected format!");
				ue.addDebugInfo("strImage", strMultiPageImageFileName);
				ue.addDebugInfo("lastDot", lastDot);
				throw ue;
			}

			// insert the "-1" at the location of the "." in "12312321.tif"
			// and return the resulting string
			strMultiPageImageFileName.insert(lastDot, strPage);
			return strMultiPageImageFileName;
		}
		break;

	case kName_Underscore_P_Ds_Dot_Tif:
		{
			// build the page number string without any zero-padding
			string strPage = asString(ulPage);
			strPage.insert(0, "_P");

			// find the last dot in the multi-page tif file
			size_t lastDot = strMultiPageImageFileName.find_last_of('.');
			if (lastDot == string::npos || lastDot == 0)
			{
				UCLIDException ue("ELI09176", "Image not in expected format!");
				ue.addDebugInfo("strImage", strMultiPageImageFileName);
				ue.addDebugInfo("lastDot", lastDot);
				throw ue;
			}

			// insert the "_P1" at the location of the "." in "12312321.tif"
			// and return the resulting string
			strMultiPageImageFileName.insert(lastDot, strPage);
			return strMultiPageImageFileName;
		}
		break;

	case kName_Underscore_DDDD_Dot_Tif:
		{
			// build the page number string that front-zero-padded to 4 digits total
			string strPage = asString(ulPage);
			strPage = padCharacter(strPage, true, '0', 4);
			strPage.insert(0, "_");

			// find the last dot in the multi-page tif file
			size_t lastDot = strMultiPageImageFileName.find_last_of('.');
			if (lastDot == string::npos || lastDot == 0)
			{
				UCLIDException ue("ELI12090", "Image not in expected format!");
				ue.addDebugInfo("strImage", strMultiPageImageFileName);
				ue.addDebugInfo("lastDot", lastDot);
				throw ue;
			}

			// insert the "_0001" at the location of the "." in "12312321.tif"
			// and return the resulting string
			strMultiPageImageFileName.insert(lastDot, strPage);
			return strMultiPageImageFileName;
		}
		break;

	case kDDDDDDDD_Dot_Tif:
		{
			// Get folder
			string strFolder = getDirectoryFromFullPath( strMultiPageImageFileName );

			// Build the 8 digit zero padded page number like "00000001.tif"
			string strDDDFile = padCharacter(asString(ulPage), true, '0', 8);
			strDDDFile += string (".tif" );

			string strPath = strFolder.c_str() + string( "\\" ) + strDDDFile.c_str();
			return strPath;
		}
		break;

	// throw exception if eFileType is not one of the above
	default:
		{
			UCLIDException ue("ELI19356", "Invalid file type!");
			ue.addDebugInfo("eFileType", (long) eFileType);
			throw ue;
		}
	}

	// we should never reach here
	THROW_LOGIC_ERROR_EXCEPTION("ELI08510")
}
//-------------------------------------------------------------------------------------------------
void createMultiPageImage(string strPage1, EFileNameType eFileType)
{
	// get the name of the output multi-page image file
	string strOutputFileName = getMultiPageImageName(strPage1, eFileType);

	vector<string> vecImageFiles;
	int iPage = 1;
	do
	{
		// build the name of the single-page image file
		string strPage = getSinglePageImageName(strOutputFileName, eFileType, iPage);

		// if the image page file exists, load it and add it to the
		// bitmap list.
		if (isValidFile(strPage))
		{
			vecImageFiles.push_back(strPage);
		}
		else
		{
			// the page does not exist.  The image has no more
			// pages...so break out of this loop.
			// decrement the page number so that iPage represents
			// the correct number of pages in the image
			iPage--;
			break;
		}

		iPage++;

	} while (true);

	if (iPage >= 1)
	{

		try
		{
			try
			{
				cout << "Creating " << strOutputFileName << endl;
				createMultiPageImage(vecImageFiles, strOutputFileName, false );
				cout << "Created " << strOutputFileName << endl << endl;
				giImagesCreated++;
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI30101");
		}
		catch(UCLIDException& uex)
		{
			UCLIDException ue("ELI30102", "Unable to create multi page image.", uex);
			ue.addDebugInfo("MultiPage Image File Name", strOutputFileName);
			ue.log();
			cout << "Error creating " << strOutputFileName << endl << endl;
			giImagesFailed++;
		}
	}
}
//-------------------------------------------------------------------------------------------------
void process(const string& strRootDir, bool bRecursive)
{
	// get all the files as requested by the user
	vector<string> vecFiles;
	cout << endl;
	cout << "Searching for files. Please wait..." << endl;
	
	// get the first page files and the filetype
	EFileNameType eFileType = getFirstPageFiles(strRootDir, bRecursive, vecFiles);

	if (eFileType == kNone)
	{
		cout << "No files found!" << endl;
		return;
	}

	// process all the files that meet the user's specifications.
	vector<string>::const_iterator iter;
	for (iter = vecFiles.begin(); iter != vecFiles.end(); iter++)
	{
		const string& strFile = *iter;
		createMultiPageImage(strFile, eFileType);
		giTotalImagesProcessed++;
	}
}
//-------------------------------------------------------------------------------------------------
void processInputFile(ifstream &inputFile, string &strDelim, int iImageNamePos, int iImagePagePos, string &strInputPath, string &strOutputPath)
{
	if(!isValidFolder(strOutputPath))
	{
		createDirectory(strOutputPath);
	}

	csis_map<vector<string>>::type mapMultiPageImages;

	// read input file
	while (!inputFile.eof())
	{
		string strBuffer;
		getline(inputFile, strBuffer);
		if (inputFile.fail() && !inputFile.eof())
		{
			throw UCLIDException("ELI29966", "Error reading from input file.");
		}

		if(strBuffer != "")
		{
			vector<string> vecTokens;
			
			// extract tokens from each line of the input file
			StringTokenizer::sGetTokens(strBuffer, strDelim, vecTokens);

			// confirm that the column positions specified are within range
			if(((int)vecTokens.size() >= iImagePagePos) && ((int)vecTokens.size() >= iImageNamePos))
			{
				string strOutputFile = vecTokens[iImageNamePos-1];
				string strInputFileName = strInputPath + vecTokens[iImagePagePos-1];
				simplifyPathName(strInputFileName);
				if (!isAbsolutePath(strInputFileName))
				{
					UCLIDException ue("ELI29899",
						"Must specify absolute path in text file or specify input path on command line.");
					ue.addDebugInfo("Input File Name", vecTokens[iImagePagePos-1]);
					throw ue;
				}

				mapMultiPageImages[strOutputFile].push_back(strInputFileName);
			}
			else
			{
				UCLIDException ue("ELI11799", "Invalid input position specified!");
				ue.addDebugInfo("InputLine", strBuffer);
				ue.addDebugInfo("NumOfColumns", vecTokens.size());
				ue.addDebugInfo("ImageNamePos", iImageNamePos);
				ue.addDebugInfo("ImagePagePos", iImagePagePos);
				throw ue;		
			}
		}
	}

	csis_map<vector<string>>::type::iterator mapIter;

	// cycle through the map and create the multi-page images
	for(mapIter = mapMultiPageImages.begin(); mapIter != mapMultiPageImages.end(); mapIter++)
	{
		string strOutputFileName = strOutputPath + (mapIter->first) + ".tif";
		createMultiPageImage(mapIter->second, strOutputFileName, false);	
	}
}
//-------------------------------------------------------------------------------------------------
void validateLicense()
{
	VALIDATE_LICENSE(gnFLEXINDEX_IDSHIELD_SERVER_CORE, "ELI13437", "CreateMultiPageImage" );
}
//-------------------------------------------------------------------------------------------------
int main(int argc, char *argv[])
{
	try
	{
		CoInitializeEx(NULL, COINIT_MULTITHREADED);
		// print banner
		cout << "CreateMultiPageImage Utility" << endl;
		cout << "Copyright 2021, Extract Systems, LLC." << endl;
		cout << "All rights reserved." << endl;
		cout << endl;

		// Load license files ( this is need for IVariantVector )
		LicenseManagement::loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);
		validateLicense();

		InitLeadToolsLicense();

		// check for correct # of arguments
		if (argc != 2 && argc != 3 && argc != 6 && argc != 7)
		{
			cout << "ERROR: Incorrect usage!" << endl;
			printUsage();
			return EXIT_FAILURE;
		}

		if (argc == 6 || argc == 7)
		{
			string strDelim(argv[2]);
			int iImageNamePos = atoi(argv[3]);
			int iImageFilePos = atoi(argv[4]);

			// Get the absolute path for the output folder and ensure the directory
			// ends in a '\'
			string strOutputPath = buildAbsolutePath(argv[5]);
			if (strOutputPath[strOutputPath.length()-1] != '\\')
			{
				strOutputPath += "\\";
			}

			// Check for optional input path argument
			string strInputPath;
			if (argc == 7)
			{
				strInputPath = buildAbsolutePath(argv[6]);
				if (strInputPath[strInputPath.length()-1] != '\\')
				{
					strInputPath += "\\";
				}

				if (!isValidFolder(strInputPath))
				{
					UCLIDException ue("ELI29898", "Input folder does not exist.");
					ue.addDebugInfo("Input Folder", strInputPath);
					throw ue;
				}
			}

			string strInputFile(argv[1]);
			ifstream inputFile(strInputFile.c_str());

			if (inputFile.fail()) 
			{
				UCLIDException ue("ELI11798", "Unable to open file!");
				ue.addDebugInfo("Filename", strInputFile);
				throw ue;
			}
			else
			{
				processInputFile(inputFile, strDelim, iImageNamePos, iImageFilePos, strInputPath, strOutputPath);
				return EXIT_SUCCESS;
			}
		}
		else
		{
			// Check for "CreateMultiPageImage /?" command-line (P13 #4345)
			if (_strcmpi(argv[1], "/?") == 0)
			{
				printUsage();
				return EXIT_SUCCESS;
			}

			// get the root directory
			string strRootDir = argv[1];

			// get the correct state of the recursive flag
			bool bRecursive = false;
			if (argc == 3)
			{
				if (_strcmpi(argv[2], "/s") == 0)
				{
					bRecursive = true;
				}
				else
				{
					cout << "ERROR: Invalid second argument!" << endl;
					printUsage();
					return EXIT_FAILURE;
				}
			}

			// process the files per the user's specifications
			CTime start = CTime::GetCurrentTime();
			process(strRootDir, bRecursive);
			CTime end = CTime::GetCurrentTime();
			CTimeSpan duration = end - start;
			cout << endl;
			cout << giTotalImagesProcessed << " image(s) processed in " << 
				duration.GetTotalSeconds() << " seconds." << endl;
			cout << giImagesCreated << " image(s) created successfully, " <<
				giImagesFailed << " image(s) failed." << endl;
			if (giImagesFailed > 0)
			{
				cout << "See exception log for details on failed images." << endl;
			}
			cout << endl;
			return EXIT_SUCCESS;
		}
		CoUninitialize();
	}
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI19321")

	// if we reached here, it's because an exception was caught.
	cout << endl;
	cout << "NOTE: Process terminated prematurely because of error condition!" << endl;
	cout << endl;
	return EXIT_FAILURE;
}
//-------------------------------------------------------------------------------------------------
