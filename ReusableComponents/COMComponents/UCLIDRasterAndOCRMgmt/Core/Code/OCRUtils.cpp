// OCRUtils.cpp : Implementation of COCRUtils
#include "stdafx.h"
#include "UCLIDRasterAndOCRMgmt.h"
#include "OCRUtils.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <TemporaryFileName.h>
#include <l_bitmap.h>		// LeadTools Imaging library
#include <MiscLeadUtils.h>
#include <ComponentLicenseIDs.h>

#include <algorithm>
#include <fstream>
#include <io.h>
#include <ComUtils.h>

//-------------------------------------------------------------------------------------------------
// COCRUtils
//-------------------------------------------------------------------------------------------------
COCRUtils::COCRUtils()
{
	try
	{
		// if PDF support is licensed, initialize
		// NOTE: no exception is thrown or logged if PDF support is not licensed.
		initPDFSupport();
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI19865");
}
//-------------------------------------------------------------------------------------------------
COCRUtils::~COCRUtils()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16537");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRUtils::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IOCRUtils,
		&IID_ILicensedComponent
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IOCRUtils
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRUtils::BatchOCR(BSTR strRootDirOrFile, 
								 IOCREngine *pEngine, 
								 VARIANT_BOOL bRecursive,
								 long nMaxNumOfPages,
								 VARIANT_BOOL bCreateUSSFile,
								 VARIANT_BOOL bCompressUSSFile,
								 VARIANT_BOOL bSkipCreation,
								 IProgressStatus* pProgressStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		validateLicense();

		UCLID_RASTERANDOCRMGMTLib::IOCREnginePtr ipOCREngine(pEngine);
		if (ipOCREngine == __nullptr)
		{
			throw UCLIDException("ELI06056", "OCR Engine object shall not be null.");
		}

		// if strRootDirOrFile is a filename, then just process the
		string strFile = asString(strRootDirOrFile);
		DWORD dwAttr = GetFileAttributes(strFile.c_str());
		if (!(dwAttr & FILE_ATTRIBUTE_DIRECTORY))
		{
			// strRootDirOrFile is a file, not a directory
			processImageFile(strFile, nMaxNumOfPages, bCreateUSSFile, bCompressUSSFile, bSkipCreation, 
				ipOCREngine, pProgressStatus);
			return S_OK;
		}

		// root directory
		string strRootDirectory = strFile;
		// make sure the directory is valid
		if (!isValidFolder(strRootDirectory))
		{
			UCLIDException ue("ELI06087", "Root directory doesn't exist.");
			ue.addDebugInfo("RootDirectory", strRootDirectory);
			ue.addWin32ErrorInfo();
			throw ue;
		}

		// get all image files in the directory recursively
		vector<string> vecImageFiles = getImageFilesInDir(strRootDirectory, asCppBool(bRecursive) );
		// before processing the image files, let's sort the vector so that 
		// all image files are lined up ascendingly
		sort(vecImageFiles.begin(), vecImageFiles.end());

		// indicate whether there's an exception thrown or not
		bool bExceptionOccured = false;
		// create an uclid exception object to record any failure for each file if any
		UCLIDException uclidException("ELI06098", "Failed to process file(s). Click Details button to view list of files.");
		// OCR each image files and output to the same directory as the image file.
		for (unsigned int n = 0;  n < vecImageFiles.size(); n++)
		{
			string strImageFile = vecImageFiles[n];

			// catch any exception and log them and then continue with the next file
			try
			{
				try
				{
					processImageFile(strImageFile, nMaxNumOfPages, bCreateUSSFile, bCompressUSSFile, 
						bSkipCreation, ipOCREngine, pProgressStatus);
				}
				catch (...)
				{
					bExceptionOccured = true;
					uclidException.addDebugInfo("FileName", strImageFile);
					// rethrow whatever the exception
					throw;
				}
			}
			CATCH_AND_LOG_ALL_EXCEPTIONS("ELI06090");
		}

		// if there's any exception occurred, throw uclidException to the outer scope
		if (bExceptionOccured)
		{
			throw uclidException;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06053")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRUtils::RecognizeTextInImageFile(BSTR strImageFileName, 
												 long lNumPages, 
												 IOCREngine* pOCREngine,
												 IProgressStatus* pProgressStatus,
												 ISpatialString **pstrText)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_RASTERANDOCRMGMTLib::IOCREnginePtr ipOCREngine(pOCREngine);
		ASSERT_RESOURCE_ALLOCATION("ELI06943", ipOCREngine != __nullptr);

		// Retrieve the text from the image file
		string	strFile = asString(strImageFileName);
		UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipText 
			= recognizeImage(strFile, lNumPages, true, ipOCREngine, pProgressStatus);

		// Provide Spatial String back to caller
		*pstrText = (ISpatialString *)ipText.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI06934")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP COCRUtils::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	try
	{
		validateLicense();

		// If validateLicense doesn't throw any exception, then pbValue is true
		*pbValue = VARIANT_TRUE;
	}
	catch(...)
	{
		*pbValue = VARIANT_FALSE;
	}

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private methods
//-------------------------------------------------------------------------------------------------
int COCRUtils::get3DigitFilePageNumber(const std::string& strFullFileName)
{
	// Check to see if the image file has 3-digit leading the extension
	// for instance, d:\temp\3445654.001.tif
	// If it does, the output file name is going to be the actual image file without
	// the 3-digit, like 3445654.txt
	string strTemp = ::getFileNameWithoutExtension(strFullFileName);
	string str3Digit = ::getExtensionFromFullPath(strTemp);
	// remove the leading period
	if (!str3Digit.empty())
	{
		str3Digit = str3Digit.substr(1);
	}

	// set to 0 first
	int nPageNumber = 0;
	if (str3Digit.size() == 3)
	{
		// make sure each every one of the 3 characters is numeric
		for (int n=0; n<3; n++)
		{
			if (!::isdigit((unsigned char) str3Digit[n]))
			{
				// if one of the 3 character is non-digit, return 0
				return 0;
			}
		}

		nPageNumber = ::asUnsignedLong(str3Digit);
	}

	return nPageNumber;
}
//-------------------------------------------------------------------------------------------------
vector<string> COCRUtils::getImageFilesInDir(const string& strDirectory, bool bRecursive)
{
	// vector of any type of files
	vector<string> vecAllFiles;
	// get all files recursively under strDirectory
	::getFilesInDir(vecAllFiles, strDirectory, "*.*", bRecursive);
	
	// vector of image files
	vector<string> vecImageFiles;
	
	// eliminate any file that's not image file
	for each ( string strFileName in vecAllFiles )
	{
		// get extension out from the file
		string strFileExtension = ::getExtensionFromFullPath(strFileName);
		::makeLowerCase(strFileExtension);

		// Check for image file extension
		if ( isImageFileExtension( strFileExtension ) )
		{
			if (isPDFFile( strFileName ))
			{
				// Check to see if PDF support is licensed
				if ( LicenseManagement::isPDFLicensed() )
				{
					vecImageFiles.push_back( strFileName );
				}
			}
			else
			{
				vecImageFiles.push_back( strFileName );
			}
		}
	}
	return vecImageFiles;
}
//-------------------------------------------------------------------------------------------------
void COCRUtils::processImageFile(const string& strImageFile, int nMaxNumOfPages,
								 VARIANT_BOOL bCreateUSSFile, 
								 VARIANT_BOOL bCompressUSSFile, 
								 VARIANT_BOOL bSkipCreation,
								 UCLID_RASTERANDOCRMGMTLib::IOCREnginePtr ipOCREngine,
								 IProgressStatus* pProgressStatus)
{
	// Make sure that if the image is a pdf that PDF support is licensed
	LicenseManagement::verifyFileTypeLicensed( strImageFile );

	// define a string to store the output file name without extension
	// by default it is the name of the input image file
	string strOutputFileName = ::getDirectoryFromFullPath(strImageFile) + "\\";

	// whether or not to append the output to the output file
	bool bAppendOutput = false;

	// default to all
	int nNumOfPagesToProcess = -1;
	// get the actual page number for this image file is 3-digit exists
	int nFilePageNum = get3DigitFilePageNumber(strImageFile);
	if (nFilePageNum > 0)
	{
		// Only recognize max number of pages (i.e. in this case, images)
		if (nMaxNumOfPages > 0 && nFilePageNum > nMaxNumOfPages)
		{
			// if the page number this file represents is larger than
			// the max num of pages, do not OCR this image file.
			return;
		}

		// get rid of the actual file extension
		string strTemp = ::getFileNameWithoutExtension(strImageFile);
		// further strip off the 3-digit, and append the document name to the path
		strOutputFileName += ::getFileNameWithoutExtension(strTemp);

		// always replace existing output file if the page number is 1
		// otherwise append
		if (nFilePageNum > 1)
		{
			bAppendOutput = true;
		}
	}
	// if this image is not the 3-digit type of image file
	else
	{
		// Get initialized FILEINFO struct
		FILEINFO fileInfo;
		getFileInformation(strImageFile, true, fileInfo);

		int nTotalPages = fileInfo.TotalPages;
		
		// the actual number of pages to process for this image file
		nNumOfPagesToProcess = (nMaxNumOfPages < nTotalPages && nMaxNumOfPages > 0)? 
								nMaxNumOfPages : -1;

		strOutputFileName = strImageFile;
	}

	// save the spatial string to a UCLID Spatial String (USS) file
	// or to a TXT file depending upon bCreateUSSFile
	strOutputFileName += (bCreateUSSFile == VARIANT_TRUE) ? ".uss" : ".txt";

	// If the file exists, we shall check bSkipCreation
	if (isValidFile(strOutputFileName))
	{
		// Perhaps the file does not need to be created www
		if (bSkipCreation == VARIANT_TRUE)
		{
			// Just return if the file already exists
			return;
		}
		else if (isFileReadOnly(strOutputFileName))
		{
			// If the creation of the file shall not be skipped,
			// and the file already exists with read-only access,
			// then turn off the read-only flag of the file
			SetFileAttributes(strOutputFileName.c_str(), FILE_ATTRIBUTE_NORMAL);
		}
	}

	// OCR the image and store the output to a file
	UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipSpatialString = 
		recognizeImage(strImageFile, nNumOfPagesToProcess, 
		asCppBool(bCreateUSSFile), ipOCREngine, pProgressStatus);
	
	// if the output should be appended, then read the spatial string
	// in the current file
	UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr ipFinalString;
	if (bAppendOutput)
	{
		ipFinalString.CreateInstance(CLSID_SpatialString);
		ASSERT_RESOURCE_ALLOCATION("ELI06737", ipFinalString != __nullptr);

		ipFinalString->LoadFrom(get_bstr_t(strOutputFileName.c_str()), VARIANT_FALSE);
		ipFinalString->AppendString(get_bstr_t("\r\n\r\n"));
		ipFinalString->Append(ipSpatialString);
	}
	else
	{
		ipFinalString = ipSpatialString;
	}

	// write the final string to the specified USS file
	ipFinalString->SaveTo(get_bstr_t(strOutputFileName.c_str()), 
		bCompressUSSFile, VARIANT_TRUE);
}
//-------------------------------------------------------------------------------------------------
UCLID_RASTERANDOCRMGMTLib::ISpatialStringPtr COCRUtils::recognizeImage(
								const string& strImageFile, 
								int nNumOfPagesToRecognize, 
								bool bReturnSpatialInfo,
								UCLID_RASTERANDOCRMGMTLib::IOCREnginePtr ipOCREngine,
								IProgressStatus* pProgressStatus)
{
	// Recognize the text
	return ipOCREngine->RecognizeTextInImage(strImageFile.c_str(), 1, nNumOfPagesToRecognize,
		UCLID_RASTERANDOCRMGMTLib::kNoFilter, "", UCLID_RASTERANDOCRMGMTLib::kRegistry, 
		asVariantBool(bReturnSpatialInfo), pProgressStatus);
}
//-------------------------------------------------------------------------------------------------
void COCRUtils::validateLicense()
{
	static const unsigned long THIS_COMPONENT_ID = gnEXTRACT_CORE_OBJECTS;

	VALIDATE_LICENSE(THIS_COMPONENT_ID, "ELI06052", "OCR Utils");
}
//-------------------------------------------------------------------------------------------------
