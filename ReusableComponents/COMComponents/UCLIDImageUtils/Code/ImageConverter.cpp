// ImageConverter.cpp : Implementation of CImageConverter

#include "stdafx.h"
#include "ImageConverter.h"
#include <COMUtils.h>
#include <UCLIDException.h>
#include <TemporaryFileName.h>
#include <KernelAPI.h>
#include <LicenseMgmt.h>
#include <ComponentLicenseIDs.h>
#include <ScansoftErr.h>
#include <OcrMethods.h>

#include "..\..\UCLIDRasterAndOCRMgmt\OCREngines\SSOCR2\Code\OcrConstants.h"

#include <set>

//-------------------------------------------------------------------------------------------------
// Macros
//-------------------------------------------------------------------------------------------------

// the following macro is used to simplify the process of throwing exception 
// when a RecAPI call has failed
#define THROW_UE(strELICode, strExceptionText, rc) \
	{ \
		UCLIDException ue(strELICode, strExceptionText); \
		loadScansoftRecErrInfo(ue, rc); \
		throw ue; \
	}

// the following macro is used to simplify the process of checking the return code
// from the RecApi calls and throwing exception if the return code is not REC_OK
#define THROW_UE_ON_ERROR(strELICode, strExceptionText, RecAPICall) \
	{ \
		RECERR rc = ##RecAPICall; \
		if (rc != REC_OK) \
		{ \
			THROW_UE(strELICode, strExceptionText, rc); \
		} \
	}


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
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI45140");
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
			UCLIDException ue("ELI45141",
				"Application trace: Unable to release recognition data. Possible memory leak.");
			loadScansoftRecErrInfo(ue, rc);
			ue.log();
		}

		rc = kRecFreeImg(m_pMemoryType);

		// log any errors
		if (rc != REC_OK)
		{
			UCLIDException ue("ELI45142",
				"Application trace: Unable to release page image. Possible memory leak.");
			loadScansoftRecErrInfo(ue, rc);
			ue.log();
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI45143");
}

//-------------------------------------------------------------------------------------------------
// CImageConverter
//-------------------------------------------------------------------------------------------------
CImageConverter::CImageConverter()
{
	try
	{
		// construct the path to ImageFormatConverter relative to the common components directory
		m_strImageFormatConverterEXE = getModuleDirectory(_Module.m_hInst);
		m_strImageFormatConverterEXE += "\\ImageFormatConverter.exe";
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI47275");
}
//-------------------------------------------------------------------------------------------------
CImageConverter::~CImageConverter()
{
}

//-------------------------------------------------------------------------------------------------
// IImageConverter
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageConverter::GetPDFImage(BSTR bstrFileName, int nPage, VARIANT_BOOL vbUseSeparateProcess,
	VARIANT *pImageData)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	
	try
	{
		// Check license
		validateLicense();

		ASSERT_ARGUMENT("ELI45144", pImageData != __nullptr);

		string strFileName = asString(bstrFileName);

		if (vbUseSeparateProcess == VARIANT_FALSE)
		{
			convertPageToPDF(strFileName, nPage, pImageData);
		}
		else
		{
			convertPageToPdfWithSeparateProcess(strFileName, nPage, pImageData);
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI45145");
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageConverter::raw_IsLicensed(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

		try
	{
		ASSERT_ARGUMENT("ELI45028", pbValue != __nullptr);

		try
		{
			// check the license
			validateLicense();

			// If no exception, then pbValue is true
			*pbValue = VARIANT_TRUE;
		}
		catch (...)
		{
			*pbValue = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI45029");

	return S_OK;
}

//--------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//--------------------------------------------------------------------------------------------------
STDMETHODIMP CImageConverter::InterfaceSupportsErrorInfo(REFIID riid)
{
	try
	{
		static const IID* arr[] =
		{
			&IID_IImageConverter,
			&IID_ILicensedComponent
		};

		for (int i = 0; i < sizeof(arr) / sizeof(arr[0]); i++)
		{
			if (InlineIsEqualGUID(*arr[i], riid))
				return S_OK;
		}

		return S_FALSE;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI45030")
}

//-------------------------------------------------------------------------------------------------
void CImageConverter::validateLicense()
{
	VALIDATE_LICENSE(gnFLEXINDEX_IDSHIELD_CORE_OBJECTS, "ELI45146", "ImageConverter");
}
//-------------------------------------------------------------------------------------------------
void CImageConverter::initNuanceEngineAndLicense()
{
	try
	{
		// initialize the OEM license using the license file that is expected to exist
		// in the same directory as this DLL 
		RECERR rc = kRecSetLicense(__nullptr, gpszOEM_KEY);
		if (rc != REC_OK && rc != API_INIT_WARN)
		{
			THROW_UE("ELI45148", "Unable to load Nuance engine license file!", rc);
		}

		// Initialization of OCR engine	
		rc = kRecInit("Extract Systems", "ImageFormatConverter");
		if (rc != REC_OK && rc != API_INIT_WARN)
		{
			THROW_UE("ELI45149", "Unable to initialize Nuance engine!", rc);
		}

		// if rc is API_INIT_WARN, ensure that the required modules are available
		if (rc == API_INIT_WARN)
		{
			LPKRECMODULEINFO pModules;
			size_t size;
			THROW_UE_ON_ERROR("ELI45150", "Unable to obtain modules information from the Nuance engine!",
				kRecGetModulesInfo(&pModules, &size));

			// if a required library module is not there, do not continue.
			if (pModules[INFO_MOR].Version <= 0)
			{
				THROW_UE("ELI45151", "Unable to find required MOR module for Nuance engine to run.", rc);
			}
			if (pModules[INFO_MTX].Version <= 0)
			{
				THROW_UE("ELI45152", "Unable to find required MTX module for Nuance engine to run.", rc);
			}
			if (pModules[INFO_PLUS2W].Version <= 0)
			{
				THROW_UE("ELI45153", "Unable to find required PLUS2W module for Nuance engine to run.", rc);
			}
			if (pModules[INFO_PLUS3W].Version <= 0)
			{
				THROW_UE("ELI45154", "Unable to find required PLUS3W module for Nuance engine to run.", rc);
			}
			if (pModules[INFO_HNR].Version <= 0)
			{
				THROW_UE("ELI45155", "Unable to find required HNR module for Nuance engine to run.", rc);
			}
			if (pModules[INFO_RER].Version <= 0)
			{
				THROW_UE("ELI45156", "Unable to find required RER module for Nuance engine to run.", rc);
			}
			if (pModules[INFO_DOT].Version <= 0)
			{
				THROW_UE("ELI45157", "Unable to find required DOT module for Nuance engine to run.", rc);
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI45158")
}
//-------------------------------------------------------------------------------------------------
void CImageConverter::convertPageToPDF(const string& strInputFileName, int nPage, VARIANT *pImageData)
{
	bool bOutputImageOpened(false);
	unique_ptr<HIMGFILE> uphOutputImage(__nullptr);
	unique_ptr<TemporaryFileName> pTempOutputFile;

	try
	{
		try
		{
			// initialize the Nuance engine and any necessary licensing thereof
			initNuanceEngineAndLicense();

			HIMGFILE hInputImage;
			THROW_UE_ON_ERROR("ELI45159", "Unable to open source image file.",
				kRecOpenImgFile(strInputFileName.c_str(), &hInputImage, IMGF_READ, FF_SIZE));

			// Ensure that the memory stored for the image file is released
			RecMemoryReleaser<tagIMGFILEHANDLE> inputImageFileMemoryReleaser(hInputImage);

			IMG_INFO imgInfo = { 0 };
			IMF_FORMAT imgFormat;
			THROW_UE_ON_ERROR("ELI45160", "Failed to identify image format.",
				kRecGetImgFilePageInfo(0, hInputImage, nPage - 1, &imgInfo, &imgFormat));
			int nBitsPerPixel = imgInfo.BitsPerPixel;

			// Keep the same image format if the source image is a PDF. Otherwise,
			// FF_PDF_SUPERB was causing unacceptable growth in PDF size in some cases for color
			// documents. For the time being, unless a document is bitonal, use FF_PDF_GOOD rather than
			// FF_PDF_SUPERB.
			IMF_FORMAT nFormat = (imgFormat >= FF_PDF_MIN && imgFormat <= FF_PDF_MRC_SUPERB)
				? imgFormat
				: (nBitsPerPixel == 1) ? FF_PDF_SUPERB : FF_PDF_GOOD;

			// NOTE: RecAPI uses zero-based page number indexes
			HPAGE hImagePage;
			loadPageFromImageHandle(strInputFileName, hInputImage, nPage - 1, &hImagePage);

			// Ensure that the memory stored for the image page is released.
			RecMemoryReleaser<RECPAGESTRUCT> pageMemoryReleaser(hImagePage);

			pTempOutputFile.reset(new TemporaryFileName(true, NULL, ".pdf"));
			string strTempOutputFileName = pTempOutputFile->getName();
			// If the destination file name exists before Nuance tries to open it, it will throw an error.
			deleteFile(strTempOutputFileName);

			uphOutputImage.reset(new HIMGFILE);
			THROW_UE_ON_ERROR("ELI45161", "Unable to create destination image file.",
				kRecOpenImgFile(strTempOutputFileName.c_str(), uphOutputImage.get(), IMGF_RDWR, FF_SIZE));

			bOutputImageOpened = true;

			THROW_UE_ON_ERROR("ELI45162", "Cannot save to image page in the specified format.",
				kRecSaveImg(0, *uphOutputImage, nFormat, hImagePage, II_CURRENT, TRUE));

			kRecCloseImgFile(*uphOutputImage);

			readFileDataToVariant(strTempOutputFileName, pImageData);
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI45164");
	}
	catch (UCLIDException &ue)
	{
		// We need to close the out image file if the output image file was opened but we didn't
		// make it to the "happy case" close call.
		if (bOutputImageOpened && nPage != -1)
		{
			try
			{
				kRecCloseImgFile(*uphOutputImage);
			}
			CATCH_AND_LOG_ALL_EXCEPTIONS("ELI45165");
		}

		ue.addDebugInfo("Source image", strInputFileName);
		ue.addDebugInfo("Page", asString(nPage + 1));
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void CImageConverter::convertPageToPdfWithSeparateProcess(const string& strInputFileName, int nPage, VARIANT *pImageData)
{
	unique_ptr<TemporaryFileName> pTempOutputFile;

	try
	{
		try
		{
			pTempOutputFile = std::make_unique<TemporaryFileName>(true, (const char*)NULL, ".pdf", true);
			string strTempOutputFileName = pTempOutputFile->getName();

			// Execute the utility to convert page to PDF
			string strArgs = "\"" + strInputFileName + "\" \"" + strTempOutputFileName + "\" /pdf /page " + asString(nPage);
			DWORD dwExitCode = runExeWithProcessKiller(m_strImageFormatConverterEXE, true, strArgs);

			ASSERT_RUNTIME_CONDITION("ELI47276", dwExitCode == EXIT_SUCCESS, "ImageFormatConverter Failed");

			readFileDataToVariant(strTempOutputFileName, pImageData);
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI45164");
	}
	catch (UCLIDException &ue)
	{
		ue.addDebugInfo("Source image", strInputFileName);
		ue.addDebugInfo("Page", asString(nPage + 1));
		throw ue;
	}
}
//-------------------------------------------------------------------------------------------------
void CImageConverter::readFileDataToVariant(const string& strFileName, VARIANT *pFileData)
{
	void *pArrayData = __nullptr;
	ifstream ifs(strFileName.c_str(), ios::in | ios::binary);
	try
	{
		try
		{
			stringstream data;
			ifs.seekg(0, ifs.end);
			long length = (long)ifs.tellg();

			SAFEARRAYBOUND rgsabound[1];
			rgsabound[0].cElements = length;
			rgsabound[0].lLbound = 0;

			pFileData->vt = VT_ARRAY | VT_UI1;
			pFileData->parray = SafeArrayCreate(VT_UI1, 1, rgsabound);

			SafeArrayAccessData(pFileData->parray, &pArrayData);
			ifs.seekg(0, ifs.beg);
			ifs.read((char *)pArrayData, length);
			if (ifs.gcount() != length)
			{
				UCLIDException ue("ELI45163", "Image read failed");
				ue.addDebugInfo("Read bytes", ifs.gcount());
				ue.addDebugInfo("Expected bytes", length);
			}
			SafeArrayUnaccessData(pFileData->parray);
			ifs.close();
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI45166");
	}
	catch (UCLIDException &ue)
	{
		if (pArrayData != __nullptr)
		{
			SafeArrayUnaccessData(pFileData->parray);
		}
		ifs.close();

		throw ue;
	}
}
