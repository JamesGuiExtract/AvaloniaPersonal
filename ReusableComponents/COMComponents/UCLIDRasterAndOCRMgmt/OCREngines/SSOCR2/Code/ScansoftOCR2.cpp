// ScansoftOCR2.cpp : Implementation of CScansoftOCR2
#include "stdafx.h"
#include "SSOCR2.h"
#include "ScansoftOCR2.h"
#include "ScansoftOCRCfg.h"
#include "OCRConstants.h"
#include "OcrMethods.h"

#include <ScansoftErr.h>
#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <cpputil.h>
#include <RegistryPersistenceMgr.h>
#include <COMUtils.h>
#include <SpecialIcoMap.h>
#include <mathUtil.h>
#include <TemporaryFileName.h>

#include <io.h>
#include <cmath>
#include <memory>
#include <string>
#include <iostream>

using namespace std;

// add license management functions
DEFINE_LICENSE_MGMT_PASSWORD_FUNCTION;

//-------------------------------------------------------------------------------------------------
// Globals and statics
//-------------------------------------------------------------------------------------------------

string CScansoftOCR2::ms_strLastDisplayedFilterChars = "";

// These are all for displaying progress
CScansoftOCR2* CScansoftOCR2::ms_pInstance;

long CScansoftOCR2::ms_lProcessID;
long CScansoftOCR2::ms_lPercentComplete;
long CScansoftOCR2::ms_lPageIndex;
long CScansoftOCR2::ms_lPageNumber;

CMutex CScansoftOCR2::ms_mutexProgressStatus;

bool CScansoftOCR2::ms_bUpdateProgressStatus;

long CScansoftOCR2::ms_lCurrentPageNumber;
unsigned int CScansoftOCR2::ms_uiCurrentPageIndex;

// ScanSoft LETTERs
const WORD gwEND_OF_LINE_OR_ZONE = R_ENDOFLINE | R_ENDOFZONE;
const WORD gwEND_OF_LINE_PARA_OR_ZONE = gwEND_OF_LINE_OR_ZONE | R_ENDOFPARA;

// CPPLetters
const CPPLetter gletterSLASH_R('\r','\r','\r',-1,-1,-1,-1,-1,false,false,false,0,100,0);
const CPPLetter gletterSLASH_N('\n','\n','\n',-1,-1,-1,-1,-1,false,false,false,0,100,0);
const CPPLetter gletterSPACE(' ',' ',' ',-1,-1,-1,-1,-1,false,false,false,0,100,0);
const CPPLetter gletterTAB('\t','\t','\t',-1,-1,-1,-1,-1,false,false,false,0,100,0);

// ScanSoft Deskew Limitation.
// Minimum angle in degrees that the RecAPI engine is able to deskew
// based on the quality of the image and the accuracy setting of the engine.
const double gdDESKEW_LOW_QUAL_LOW_ACC   = 0.46;
const double gdDESKEW_HIGH_QUAL_LOW_ACC  = 0.29;
const double gdDESKEW_LOW_QUAL_HIGH_ACC  = 0.29;
const double gdDESKEW_HIGH_QUAL_HIGH_ACC = 0.17;

// minimum height for user zones
const int giMIN_OMNIFONT_ZONE_HEIGHT = 80;
const double gdMIN_RER_ZONE_HEIGHT_INCHES = 0.1;

// maximum line slope to still be considered horizontal
const double gdMAX_HORIZONTAL_LINE_SLOPE = 0.02;

// maximum number of pixels for zones to be considered overlapping
const int giMAX_ZONE_OVERLAP = 8;

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
// CScansoftOCR2
//-------------------------------------------------------------------------------------------------
CScansoftOCR2::CScansoftOCR2()
	: m_bPrivateLicenseInitialized(false),
	m_bRunThirdRecPass(true),
	m_bOrderZones(true),
	m_eTradeoff(TO_ACCURATE),
	m_bSkipPageOnFailure(false),
	m_bRequireOnePageSuccess(false),
	m_uiMaxOcrPageFailurePercentage(25),
	m_uiMaxOcrPageFailureNumber(10),
	m_uiDecompositionMethods(1),
	m_bEnableDespeckleMode(true),
	m_eForceDespeckle(kNeverForce),
	m_eForceDespeckleMethod(DESPECKLE_PEPPERANDSALT),
	m_nForceDespeckleLevel(2),
	m_eFilter(kNoFilter),
	m_bFilterContainsAlpha(true),
	m_bFilterContainsNumeral(true),
	m_ipSpatialString(CLSID_SpatialString),
	m_hImageFile(NULL),
	m_hPage(NULL),
	m_eDisplayFilterCharsType(kDisplayCharsTypeNone),
	m_eventKillTimeoutThread(false),
	m_eventProgressMade(false),
	m_nTimeoutLength(120000),
	m_bLimitToBasicLatinCharacters(true),
	m_bSettingsApplied(false),
	m_ePrimaryDecompositionMethod(kAutoDecomposition),
	m_eDefaultFillingMethod(FM_OMNIFONT),
	m_bOutputMultipleSpaceCharacterSequences(false),
	m_bOutputOneSpaceCharacterPerCount(false),
	m_bOutputTabCharactersForTabSpaceType(false),
	m_bAssignSpatialInfoToSpaceCharacters(false),
	m_bIgnoreParagraphFlag(false),
	m_bTreatZonesAsParagraphs(false),
	m_eOCRFindType(kFindStandardOCR),
	m_bReturnUnrecognizedCharacters(false),
	m_bLocateZonesInSpecifiedZone(false),
	m_bIgnoreAreaOutsideSpecifiedZone(false)
{
	try
	{
		ASSERT_RESOURCE_ALLOCATION("ELI17066", m_ipSpatialString != __nullptr);

		// NOTE: we are loading license files here because this project is a 
		// COM EXE project.  ScansoftOCR2 is privately licensed - no license 
		// file is required to make this particular component operational.
		// However, other components that this component uses (such as 
		// IUnknownVector) are defined in other projects, and those do
		// not support private licensing.  Hence, we have to load all license
		// files from this EXE so that those supporting COM objects will be 
		// operational (assuming a valid license file is found).
		LicenseManagement::loadLicenseFilesFromFolder(LICENSE_MGMT_PASSWORD);

		try
		{
			// Initialize licensed components inside the special IcoMap Components license file
			// using unique passwords as applied to the UCLID String
			LicenseManagement::initializeLicenseFromFile( 
				g_strIcoMapComponentsLicenseName, gulIcoMapKey1, gulIcoMapKey2, 
				gulIcoMapKey3, gulIcoMapKey4, false);
		}
		catch(...)
		{
		}
		
		// set the static instance
		CSingleLock lock(&ms_mutexProgressStatus, TRUE);
		ms_pInstance = this;

		// set static progress status variables to defaults
		ms_lProcessID = PID_IMGINPUT;
		ms_lPercentComplete = 0;
		ms_lPageIndex = 0;
		ms_lPageNumber = 0;
		ms_uiCurrentPageIndex = 0;
		ms_lCurrentPageNumber = 0;
		ms_bUpdateProgressStatus = false;

		init();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI11013")
}
//-------------------------------------------------------------------------------------------------
CScansoftOCR2::~CScansoftOCR2()
{
	try
	{
		THROW_UE_ON_ERROR("ELI18325", "Unable to free OCR engine resources. Possible memory leak.",
			RecQuitPlus() );

		recursiveRemoveDirectory(getTemporaryDataFolder(GetCurrentProcessId()));
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI11014")
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CScansoftOCR2::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IScansoftOCR2,
		&IID_ILicensedComponent,
		&IID_IPrivateLicensedComponent
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IScansoftOCR2
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CScansoftOCR2::RecognizeText(BSTR bstrImageFileName, IVariantVector* pPageNumbers, ILongRectangle* pZone,
	long lRotationInDegrees, EFilterCharacters eFilter,	BSTR bstrCustomFilterCharacters, EOcrTradeOff eTradeOff,
	VARIANT_BOOL vbDetectHandwriting, VARIANT_BOOL vbReturnUnrecognized, VARIANT_BOOL vbReturnSpatialInfo, 
	VARIANT_BOOL vbUpdateProgressStatus, EPageDecompositionMethod eDecompMethod,
	BSTR* pStream)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());
	
	try
	{
		// validate the license
		validateLicense();

		// store whether to update the progress status
		ms_bUpdateProgressStatus = asCppBool(vbUpdateProgressStatus);

		// store whether to detect handwriting
		bool bDetectHandwriting = asCppBool(vbDetectHandwriting);

		// use a smart pointer
		IVariantVectorPtr ipPageNumbers(pPageNumbers);
		ASSERT_RESOURCE_ALLOCATION("ELI11061", ipPageNumbers != __nullptr);

		// convert the page number variant vector to a regular vector
		int iSize = ipPageNumbers->Size;
		static vector<long> vecPageNumbers;
		vecPageNumbers.clear();
		vecPageNumbers.reserve(iSize);
		for(int i = 0; i < iSize; i++)
		{
			_variant_t var = ipPageNumbers->GetItem(i);
			vecPageNumbers.push_back(var.lVal);
		}

		// set user zone and decomposition sequence if appropriate
		LPRECT lpRect = NULL;
		RECT rect;
		if(pZone)
		{
			// get a smart pointer
			ILongRectanglePtr ipRect(pZone);

			// Store the bounds in the rectandle
			ipRect->GetBounds(&rect.left, &rect.top, &rect.right, &rect.bottom);
	
			lpRect = &rect;

			// a decomposition sequence is necessary to help identify the locations of handwritten 
			// characters, but not used if locating printed text within a zone
			if(bDetectHandwriting || m_bLocateZonesInSpecifiedZone)
			{
				setDecompositionSequence(eDecompMethod);
			}
		}
		else
		{
			// set the decomposition sequence
			setDecompositionSequence(eDecompMethod);
		}

		// set filter characters
		setCharacterSetFilter(bstrCustomFilterCharacters, eFilter);

		// Set the speed-accuracy trade off
		setTradeOff(eTradeOff);
		
		// OCR the image (the result is stored in m_ipSpatialString)
		recognizeText(asString(bstrImageFileName), vecPageNumbers, lpRect, lRotationInDegrees, 
			bDetectHandwriting, asCppBool(vbReturnUnrecognized), asCppBool(vbReturnSpatialInfo));

		IPersistStreamPtr ipObj = m_ipSpatialString;
		ASSERT_RESOURCE_ALLOCATION("ELI18368", ipObj != __nullptr);
		*pStream = writeObjectToBSTR(ipObj, false).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11017");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CScansoftOCR2::CreateOutputImage(BSTR bstrImageFileName, BSTR bstrFormat, BSTR bstrOutputFileName)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());
	
	try
	{
		// validate the license
		validateLicense();

		THROW_UE_ON_ERROR("ELI46466", "Unable to set output format",
			RecSetOutputFormat(0, get_bstr_t(bstrFormat)));

		HDOC hDoc;
		HIMGFILE hFile;
		HPAGE hPage;
		int nPageCount;

		THROW_UE_ON_ERROR("ELI46467", "Unable to create doc",
			RecCreateDoc(0, "", &hDoc, DOC_NORMAL));
		// Ensure document gets closed
		RecMemoryReleaser<RECDOCSTRUCT> DocumentMemoryReleaser(hDoc);

		_bstr_t bstrtImageFileName = get_bstr_t(bstrImageFileName);
		THROW_UE_ON_ERROR("ELI46468", "Unable to open input file",
			kRecOpenImgFile(bstrtImageFileName, &hFile, IMGF_READ, FF_SIZE));
		// Ensure image gets closed
		RecMemoryReleaser<tagIMGFILEHANDLE> ImageFileMemoryReleaser(hFile);

		THROW_UE_ON_ERROR("ELI46469", "Unable to get image page count",
			kRecGetImgFilePageCount(hFile, &nPageCount));

		string strImageFileName = asString(bstrtImageFileName);
		for (int i = 0; i < nPageCount; ++i)
		{
			try
			{
				loadPageFromImageHandle(strImageFileName, hFile, i, &hPage);
			}
			catch (UCLIDException ue)
			{
				ue.addDebugInfo("Image", asString(bstrtImageFileName));
				ue.addDebugInfo("Page", i + 1);
				throw ue;
			}
			try
			{
				THROW_UE_ON_ERROR("ELI49484", "Unable to preprocess image page",
					kRecPreprocessImg(0, hPage));

				THROW_UE_ON_ERROR("ELI46471", "Unable to recognize image page",
					kRecRecognize(0, hPage, NULL));
			}
			catch (UCLIDException ue)
			{
				ue.addDebugInfo("Image", strImageFileName);
				ue.addDebugInfo("Page", i + 1);
				ue.log();
			}

			THROW_UE_ON_ERROR("ELI46472", "Unable to insert page",
				RecInsertPage(0, hDoc, hPage, -1));
		}

		THROW_UE_ON_ERROR("ELI46473", "Unable convert document",
			RecConvert2Doc(0, hDoc, get_bstr_t(bstrOutputFileName)));

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI46479");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CScansoftOCR2::SetOutputFormat(BSTR bstrFormat)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());
	
	try
	{
		// validate the license
		validateLicense();

		THROW_UE_ON_ERROR("ELI49535", "Unable to set output format",
			RecSetOutputFormat(0, get_bstr_t(bstrFormat)));

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI49511");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CScansoftOCR2::GetPID(long* pPID)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());
	try
	{
		DWORD pid = GetCurrentProcessId();
		*pPID = (long)pid;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11133")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CScansoftOCR2::SupportsTrainingFiles(VARIANT_BOOL *pbValue)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());
	try
	{
		// validate the return value
		ASSERT_ARGUMENT("ELI18371", pbValue != __nullptr)

		// validate the license
		validateLicense();

		// return true, as this implementation supports training files
		*pbValue = asVariantBool( supportsTrainingFiles() );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11020")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CScansoftOCR2::LoadTrainingFile(BSTR strTrainingFileName)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		// validate the license
		validateLicense();

		string strTrainingFile = asString(strTrainingFileName);

		loadTrainingFile(strTrainingFile);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11021")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CScansoftOCR2::WillPerformThirdRecognitionPass(
	VARIANT_BOOL *vbWillPerformThirdRecognitionPass)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		*vbWillPerformThirdRecognitionPass = asVariantBool(m_bRunThirdRecPass);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16202");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CScansoftOCR2::GetProgress(long* plProcessID, long* plPercentComplete, 
										long* plPageIndex, long* plPageNumber)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		// enter critical section
		CSingleLock lock(&ms_mutexProgressStatus, TRUE);

		// update the progress status variables
		*plProcessID = ms_lProcessID;
		*plPercentComplete = ms_lPercentComplete;
		*plPageIndex = ms_lPageIndex;
		*plPageNumber = ms_lPageNumber;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16206");

	return S_OK;	
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CScansoftOCR2::GetPrimaryDecompositionMethod(
	EPageDecompositionMethod *ePrimaryDecompositionMethod)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		*ePrimaryDecompositionMethod = m_ePrimaryDecompositionMethod;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16762");

	return S_OK;	
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CScansoftOCR2::SetOCRParameters(IOCRParameters* pOCRParameters, VARIANT_BOOL vbReApply)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		// If first use or not just a second try due to failure then apply settings
		if (!m_bSettingsApplied || asCppBool(vbReApply))
		{
			// If settings were previously applied then set everything back to the default values so
			// that none of the previous values remain.
			// This should be done because IOCRParameters collections are unbounded and the new
			// settings may not have all the parameters that the registry or the last collection had.
			if (m_bSettingsApplied)
			{
				// Set everything to default values
				HSETTING hSetting;
				THROW_UE_ON_ERROR("ELI45929", "Unable to get OCR setting.",
					kRecSettingGetHandle(NULL, "Kernel", &hSetting, NULL));
				THROW_UE_ON_ERROR("ELI45930", "Unable to set default options",
					kRecSettingSetToDefault(0, hSetting, TRUE));

				// Set Extract-specific default settings
				applyCommonSettings();
			}

			IOCRParametersPtr ipOCRParameters;
			if (pOCRParameters != __nullptr)
			{
				ipOCRParameters = pOCRParameters;
				ASSERT_RESOURCE_ALLOCATION("ELI46009", ipOCRParameters != __nullptr);
			}
			
			if (ipOCRParameters != __nullptr && ipOCRParameters->Size > 0)
			{
				applySettingsFromParameters(ipOCRParameters);
			}
			else
			{
				applySettingsFromRegistry();
			}

			m_bSettingsApplied = true;
		}

		return S_OK;	
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16762");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CScansoftOCR2::WriteOCRSettingsToFile(BSTR bstrFileName, VARIANT_BOOL vbWriteDefaults,
	VARIANT_BOOL vbWriteExtractImplementedSettings)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		string strFileName = asString(bstrFileName);
		kRecSettingSave(0, NULL, strFileName.c_str(), asCppBool(vbWriteDefaults), FALSE);

		if (asCppBool(vbWriteExtractImplementedSettings))
		{
			ofstream outfile(strFileName, std::ofstream::out | std::ofstream::app);

			outfile << "\nAssignSpatialInfoToSpaceCharacters = " << m_bAssignSpatialInfoToSpaceCharacters;
			outfile << "\nEnableDespeckleMode = " << m_bEnableDespeckleMode;
			outfile << "\nForceDespeckle = " << m_eForceDespeckle;
			outfile << "\nIgnoreParagraphFlag = " << m_bIgnoreParagraphFlag;
			outfile << "\nLimitToBasicLatinCharacters = " << m_bLimitToBasicLatinCharacters;
			outfile << "\nMaxOcrPageFailureNumber = " << m_uiMaxOcrPageFailureNumber;
			outfile << "\nMaxOcrPageFailurePercentage = " << m_uiMaxOcrPageFailurePercentage;
			outfile << "\nOrderZones = " << m_bOrderZones;
			outfile << "\nOutputMultipleSpaceCharacterSequences = " << m_bOutputMultipleSpaceCharacterSequences;
			outfile << "\nOutputOneSpaceCharacterPerCount = " << m_bOutputOneSpaceCharacterPerCount;
			outfile << "\nOutputTabCharactersForTabSpaceType = " << m_bOutputTabCharactersForTabSpaceType;
			outfile << "\nPrimaryDecompositionMethod = " << m_ePrimaryDecompositionMethod;
			outfile << "\nRequireOnePageSuccess = " << m_bRequireOnePageSuccess;
			outfile << "\nSkipPageOnFailure = " << m_bSkipPageOnFailure;
			outfile << "\nTimeoutLength = " << m_nTimeoutLength;
			outfile << "\nTreatZonesAsParagraphs = " << m_bTreatZonesAsParagraphs;
			outfile << "\nOCRFindType = " << m_eOCRFindType;
			outfile << "\nReturnUnrecognizedCharacters = " << m_bReturnUnrecognizedCharacters;
			outfile << "\nLocateZonesInSpecifiedZone = " << m_bLocateZonesInSpecifiedZone;
			outfile << "\nIgnoreAreaOutsideSpecifiedZone = " << m_bIgnoreAreaOutsideSpecifiedZone;

			outfile.close();
		}

		return S_OK;	
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI16762");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CScansoftOCR2::GetPDFImage(BSTR bstrFileName, int nPage, VARIANT* pImageData)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		// validate the license
		validateLicense();

		string strInputFileName = asString(bstrFileName);
		bool bOutputImageOpened(false);
		unique_ptr<HIMGFILE> uphOutputImage(__nullptr);
		unique_ptr<TemporaryFileName> pTempOutputFile;

		try
		{
			try
			{
				HIMGFILE hInputImage;
				THROW_UE_ON_ERROR("ELI49594", "Unable to open source image file.",
					kRecOpenImgFile(strInputFileName.c_str(), &hInputImage, IMGF_READ, FF_SIZE));

				// Ensure that the memory stored for the image file is released
				RecMemoryReleaser<tagIMGFILEHANDLE> inputImageFileMemoryReleaser(hInputImage);

				IMG_INFO imgInfo = { 0 };
				IMF_FORMAT imgFormat;
				THROW_UE_ON_ERROR("ELI49595", "Failed to identify image format.",
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
				THROW_UE_ON_ERROR("ELI49596", "Unable to create destination image file.",
					kRecOpenImgFile(strTempOutputFileName.c_str(), uphOutputImage.get(), IMGF_RDWR, FF_SIZE));

				bOutputImageOpened = true;

				THROW_UE_ON_ERROR("ELI49597", "Cannot save to image page in the specified format.",
					kRecSaveImg(0, *uphOutputImage, nFormat, hImagePage, II_CURRENT, TRUE));

				kRecCloseImgFile(*uphOutputImage);

				readFileDataToVariant(strTempOutputFileName, pImageData);
			}
			CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI49598");
		}
		catch (UCLIDException & ue)
		{
			// We need to close the out image file if the output image file was opened but we didn't
			// make it to the "happy case" close call.
			if (bOutputImageOpened && nPage != -1)
			{
				try
				{
					kRecCloseImgFile(*uphOutputImage);
				}
				CATCH_AND_LOG_ALL_EXCEPTIONS("ELI49599");
			}

			ue.addDebugInfo("Source image", strInputFileName);
			ue.addDebugInfo("Page", asString(nPage + 1));
			throw ue;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI49600");
}

//-------------------------------------------------------------------------------------------------
// IPrivateLicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CScansoftOCR2::raw_InitPrivateLicense(BSTR strPrivateLicenseKey)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());
	try
	{
		// NOTE: this method shall not check regular license

		// if the private license key is not valid, throw an exception
		if (!IS_VALID_PRIVATE_LICENSE( asString(strPrivateLicenseKey) ))
		{
			throw UCLIDException("ELI11024", "Invalid private license key!");
		}

		// the private license key is valid, set the bit.
		m_bPrivateLicenseInitialized = true;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19393")
	
	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CScansoftOCR2::raw_IsPrivateLicensed(VARIANT_BOOL *pbIsLicensed)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());
	try
	{
		// NOTE: this method shall not check regular license

		*pbIsLicensed = asVariantBool(m_bPrivateLicenseInitialized);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI19394")
	
	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CScansoftOCR2::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18326", pbValue != __nullptr);

		try
		{
			// validate license
			validateLicense();

			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI11027")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// Private functions
//-------------------------------------------------------------------------------------------------
void CScansoftOCR2::validateLicense()
{
	// SSOCR2 MUST be privately licensed
	if (m_bPrivateLicenseInitialized)
	{
		return;
	}

	// Prepare and throw an exception if component is not licensed
	UCLIDException ue("ELI11028", "ScansoftOCR2 component is not privately licensed!");
	ue.addDebugInfo("Component Name", "Scansoft OCR Engine Internals");
	throw ue;
}
//-------------------------------------------------------------------------------------------------
EOrientation CScansoftOCR2::getOrientation(int nRotationInDegrees, IMG_ROTATE &rimgRotate)
{
	// Update rimgRotate or retain the value from automatic decomposition
	switch (nRotationInDegrees)
	{
	case 90:
		// Force orientation to ROT_RIGHT
		rimgRotate = ROT_RIGHT;
		break;

	case 180:
		// Force orientation to ROT_DOWN
		rimgRotate = ROT_DOWN;
		break;

	case 270:
		// Force orientation to ROT_LEFT
		rimgRotate = ROT_LEFT;
		break;

	case 360:
		// Force orientation to ROT_NO
		rimgRotate = ROT_NO;
		break;

	default:
		// Only 0 is legal here
		if (nRotationInDegrees != 0)
		{
			UCLIDException ue("ELI16660", "Invalid setting for rotation angle!");
			ue.addDebugInfo( "Desired Rotation", nRotationInDegrees );
			throw;
		}
	}

	// Update return the EOrientation value corresponding to rimgRotate
	switch (rimgRotate)
	{
	case ROT_NO:
		return kRotNone;

	case ROT_RIGHT:
		return kRotRight;

	case ROT_DOWN:
		return kRotDown;

	case ROT_LEFT:
		return kRotLeft;

	default:
		// Other orientations are not supported
		UCLIDException ue("ELI16662", "Invalid orientation for SpatialPageInfo!");
		ue.addDebugInfo( "Orientation", rimgRotate );
		throw;
	}
}
//-------------------------------------------------------------------------------------------------
bool CScansoftOCR2::isDeskewable(double dDeskewDegrees, IMG_INFO imgInfo)
{
	// maximum deskewable angle (kRecDeskewImg will return an error for angles larger than this)
	// https://extract.atlassian.net/browse/ISSUE-15701
	const double dMaximumDeskewDegrees = 30;
	if (dDeskewDegrees > dMaximumDeskewDegrees)
	{
		return false;
	}

	// minimum deskewable angle (kRecDeskewImg will ignore angles lower than this).
	double dMinimumDeskewDegrees;

	// check if image is black and white, or palette-color.
	// (higher minimum deskew for these image types).
	if(imgInfo.BitsPerPixel <= 4 || imgInfo.BitsPerPixel == 8 && imgInfo.IsPalette)
	{
		// retrieve the minimum deskew based on the accuracy setting.
		// (higher accuracy means lower minimum deskew)
		dMinimumDeskewDegrees = (m_eTradeoff == TO_FAST ? 
			gdDESKEW_LOW_QUAL_LOW_ACC : gdDESKEW_LOW_QUAL_HIGH_ACC);
	}
	else // the image is grayscale or true-color (lower minimum deskew)
	{
		// retrieve the minimum deskew based on the accuracy setting.
		// (higher accuracy means lower minimum deskew)
		dMinimumDeskewDegrees = (m_eTradeoff == TO_FAST ? 
			gdDESKEW_HIGH_QUAL_LOW_ACC : gdDESKEW_HIGH_QUAL_HIGH_ACC);
	}

	// the image is deskewable if the absolute value of the 
	// deskew angle is larger than the miminum cut-off
	return dDeskewDegrees > dMinimumDeskewDegrees || 
		dDeskewDegrees < -dMinimumDeskewDegrees;
}
//-------------------------------------------------------------------------------------------------
void CScansoftOCR2::recognizeText(string strFileName, const vector<long>& vecPageNumbers, 
	RECT* pZone, long nRotationInDegrees, bool bDetectHandwriting, bool bReturnUnrecognized, 
	bool bReturnSpatialInfo)
{
	// clear the spatial string
	m_ipSpatialString = __nullptr;

	// create the vector of letters
	vector<CPPLetter> vecLetters;
	vector<CPPLetter>* pvecLetters = NULL;
	
	// create the vector of letters, if such information was requested
	if (bReturnSpatialInfo)
	{
		pvecLetters = &vecLetters;
	}

	// Create map object to collect spatial page info items
	ILongToObjectMapPtr ipPageInfos(CLSID_LongToObjectMap);
	ASSERT_RESOURCE_ALLOCATION("ELI15216", ipPageInfos != __nullptr);

	// create string to hold non-spatial text, in case that was requested
	string strText;

	// start the timeout thread before any RecApi calls are made
	CWinThread* pTimeoutThread = AfxBeginThread(timeoutThreadProc, this);

	// scope for the killThreadOnDestruct object
	{
		// signal the timeout thread to stop when killThreadOnDestruct goes out of scope
		RecMemoryReleaser<Win32Event> killThreadOnDestruct(&m_eventKillTimeoutThread);

		// recognize text on the specified pages
		recognizeTextOnPages(strFileName, vecPageNumbers, pZone, nRotationInDegrees, 
			bDetectHandwriting, bReturnUnrecognized,
			strText, pvecLetters, ipPageInfos);
	}

#ifdef _DEBUG
	// log the output in debug mode
	logDebugInfo(strText, pvecLetters);
#endif

	// store the result into m_ipSpatialString
	storeResultAsSpatialString(strFileName, pvecLetters, strText, ipPageInfos);
}
//-------------------------------------------------------------------------------------------------
void CScansoftOCR2::recognizeTextOnPages(const string& strFileName, 
	const vector<long> vecPageNumbers, LPRECT pZone, long nRotationInDegrees, 
	bool bDetectHandwriting, bool bReturnUnrecognized,
	string& rstrText, vector<CPPLetter>* pvecLetters, ILongToObjectMapPtr ipPageInfos)
{
	// load the specified image file
	THROW_UE_ON_ERROR("ELI16639", "Unable to load specified image file in the OCR engine!",
		kRecOpenImgFile(strFileName.c_str(), &m_hImageFile, IMGF_READ, FF_SIZE) );

	// ensure that the memory stored for the image file is released
	RecMemoryReleaser<tagIMGFILEHANDLE> ImageFileMemoryReleaser(m_hImageFile);

	// get the number of pages to process
	unsigned int uiCount = vecPageNumbers.size();

	// Clear all page failures
	m_vecFailedPages.clear();

	// Maintain an exception that aggregates together all exceptions generated by failed
	// decomposition attempts.
	unique_ptr<UCLIDException> apAggregateException;

	// recognize each page in sequence, accumulate the text, 
	// and return the total accumulated text
	for (ms_uiCurrentPageIndex = 0; ms_uiCurrentPageIndex < uiCount; ms_uiCurrentPageIndex++)
	{
		ms_lCurrentPageNumber = vecPageNumbers[ms_uiCurrentPageIndex];

		// create a vector to store the page letters if appropriate
		vector<CPPLetter> vecPageLetters;
		vector<CPPLetter>* pvecPageLetters = NULL;
		if(pvecLetters != __nullptr)
		{
			pvecPageLetters = &vecPageLetters;
		}
		string strPageText = "";

		for (unsigned int i = 0; i < m_uiDecompositionMethods; i++)
		{
			try
			{
				try
				{
					// Try a new decomposition method until successful
					setDecompositionMethodIndex(i);

					// Recognize the text in the current page
					rotateAndRecognizeTextInImagePage(strFileName, ms_lCurrentPageNumber, pZone, 
						nRotationInDegrees, bDetectHandwriting, bReturnUnrecognized, strPageText, 
						pvecPageLetters, ipPageInfos);

					break;
				}
				CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26799")
			}
			catch (UCLIDException& ue)
			{
				if (!m_bSkipPageOnFailure)
				{
					throw ue;
				}

				// Reset the page text
				vecPageLetters.clear();
				strPageText.clear();

				// Retain this exception if it is the last decomposition method
				if (i >= (m_uiDecompositionMethods - 1))
				{
					// Store this failed page.
					m_vecFailedPages.push_back(ms_lCurrentPageNumber);

					// Create or append to the aggregate exception.
					string message = "Application trace: Unable to OCR page.";
					if (apAggregateException.get() == NULL)
					{
						apAggregateException.reset(new UCLIDException("ELI26800", message));
					}
					else
					{
						apAggregateException.reset(new UCLIDException("ELI26801", message, 
							*apAggregateException));
					}

					// Add as debug info all history entries from ue
					apAggregateException->addDebugInfo("Exception History", ue);

					// Check if all pages failed
					if (m_bRequireOnePageSuccess && m_vecFailedPages.size() == vecPageNumbers.size())
					{
						apAggregateException.reset(new UCLIDException("ELI46008", 
							"Failure to OCR image. All pages failed.", *apAggregateException));

						throw *apAggregateException;
					}

					// Check if max failures reached
					if (m_vecFailedPages.size() > m_uiMaxOcrPageFailureNumber || 
						m_vecFailedPages.size() * 100.0 / uiCount > (double)m_uiMaxOcrPageFailurePercentage)
					{
						apAggregateException.reset(new UCLIDException("ELI26802", 
							"Failure to OCR image. Max page failures reached.", *apAggregateException));

						string strFailedPages = asString(m_vecFailedPages[0]);
						for (unsigned int i = 1; i < m_vecFailedPages.size(); i++)
						{
							strFailedPages += ", " + asString(m_vecFailedPages[i]);
						}
						apAggregateException->addDebugInfo("Failed page numbers", strFailedPages, false);

						throw *apAggregateException;
					}
				}
			}
		}

		// insert \r\n\r\n in between pages as long
		// as there will be text on both sides of the \r\n
		if (hasContent(rstrText, pvecLetters) &&
			hasContent(strPageText, pvecPageLetters))
		{
			// we should never reach here when ms_uiCurrentPageIndex <= 1
			if (ms_lCurrentPageNumber <= 1)
			{
				THROW_LOGIC_ERROR_EXCEPTION("ELI06667");
			}

			// append \r\n\r\n to either the string or the vector, depending upon whether
			// the target vector pointer is NULL
			if (pvecLetters == NULL)
			{
				rstrText.append("\r\n\r\n");
			}
			else
			{
				appendSlashRSlashN(pvecLetters, 2);
			}
		}

		// append the contents of the page just recognized
		if (hasContent(strPageText, pvecPageLetters))
		{
			appendContent(rstrText, pvecLetters, strPageText, pvecPageLetters);
		}
	}

	// If any pages failed, log an exception.
	if (m_vecFailedPages.size() > 0)
	{
		apAggregateException.reset(new UCLIDException("ELI26826", 
			"Application trace: At least one page of document failed to OCR.", *apAggregateException));

		string strFailedPages = asString(m_vecFailedPages[0]);
		for (unsigned int i = 1; i < m_vecFailedPages.size(); i++)
		{
			strFailedPages += ", " + asString(m_vecFailedPages[i]);
		}
		apAggregateException->addDebugInfo("Failed page numbers", strFailedPages, false);

		apAggregateException->log();
	}
}
//-------------------------------------------------------------------------------------------------
void CScansoftOCR2::storeResultAsSpatialString(const string& strFileName, 
	vector<CPPLetter>* pvecLetters, const string& strText, const ILongToObjectMapPtr& ipPageInfos)
{
	// create a new spatial string to return the caller
	m_ipSpatialString.CreateInstance(CLSID_SpatialString);
	ASSERT_RESOURCE_ALLOCATION("ELI18370", m_ipSpatialString != __nullptr);

	// strFileName may contain a relative path.
	// get the full path
	char pszTemp[MAX_PATH + 1];
	if (_fullpath(pszTemp, strFileName.c_str(), MAX_PATH) == NULL)
	{
		UCLIDException ue("ELI06807", "Unable to get full path.");
		ue.addDebugInfo("path", strFileName);
		ue.addWin32ErrorInfo();
		throw ue;
	}

	// depending upon whether the caller requested spatial information
	// update the spatial string in the appropriate way
	if(pvecLetters != __nullptr)
	{
		// set the letters in the spatial string and update it
		if (!pvecLetters->empty())
		{
			m_ipSpatialString->CreateFromLetterArray(pvecLetters->size(), &((*pvecLetters)[0]),
				pszTemp, ipPageInfos);
		}
		else
		{
			// If the letter vector is empty, clear the string and set the source doc name
			m_ipSpatialString->CreateNonSpatialString("", pszTemp);
		}
	}
	else
	{
		// update just the text alone.
		m_ipSpatialString->CreateNonSpatialString(strText.c_str(), pszTemp);
	}

	// Store the OCR engine version [LRCAU #5512]
	m_ipSpatialString->OCREngineVersion = m_strVersion.c_str();

	// Trim the spaces from the front and the back of the OCR'text.
	// P16 2286
	m_ipSpatialString->Trim(" ", " ");
}
//-------------------------------------------------------------------------------------------------
void CScansoftOCR2::setCharacterSetFilter(const _bstr_t& bstrCharSet, EFilterCharacters eFilter)
{
	// determine if the custom filter option is enabled
	bool bCustomFilter = (eFilter & kCustomFilter) != 0;

	// if an empty string was passed to this method and the custom filter option is set,
	// understand that the caller wants to disable the custom filter.
	if(bCustomFilter && bstrCharSet.length() == 0)
	{
		eFilter = EFilterCharacters(eFilter & ~kCustomFilter);
		bCustomFilter = false;
	}

	// if the filter option is the same as the currently set filter option
	// and no custom filter set is enabled, we are done.
	if(eFilter == m_eFilter && !bCustomFilter)
	{
		return;
	}

	// determine the set of filter characters and the RecAPI default filter
	string strFilterChars;
	CHR_FILTER filter = FILTER_PLUS;
	m_bFilterContainsAlpha = false;
	m_bFilterContainsNumeral = false;
	if(eFilter == kNoFilter)
	{
		// no filter has been selected, enable the recognition of all characters
		filter = FILTER_ALL;
		m_bFilterContainsAlpha = true;
		m_bFilterContainsNumeral = true;
	}
	else if(eFilter == kNumeralFilter)
	{
		// only numeral characters need to be recognized
		filter = FILTER_DIGIT;
		m_bFilterContainsNumeral = true;
	}
	else
	{
		// set the appropriate filter options
		if(bCustomFilter)
		{
			strFilterChars += asString(bstrCharSet);

			// check if the custom filter contained any alphabetic or numeral characters
			int iSize = strFilterChars.size();
			for(int i=0; i < iSize; i++)
			{
				if(isAlphaChar(strFilterChars[i]) )
				{
					m_bFilterContainsAlpha = true;
				}
				else if(isDigitChar(strFilterChars[i]))
				{
					m_bFilterContainsNumeral = true;
				}
			}
		}
		if((eFilter & kAlphaFilter) != 0)
		{
			strFilterChars += "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
			m_bFilterContainsAlpha = true;
		}
		if((eFilter & kNumeralFilter) != 0)
		{
			// the RecAPI engine has a special predefined filter for numerals
			filter = CHR_FILTER(filter | FILTER_DIGIT);
			m_bFilterContainsNumeral = true;
		}
		if((eFilter & kPeriodFilter) != 0)
		{
			strFilterChars += ".";
		}
		if((eFilter & kHyphenFilter) != 0)
		{
			strFilterChars += "-";
		}
		if((eFilter & kUnderscoreFilter) != 0)
		{
			strFilterChars += "_";
		}
		if((eFilter & kCommaFilter) != 0)
		{
			strFilterChars += ",";
		}
		if((eFilter & kForwardSlashFilter) != 0)
		{
			strFilterChars += "/";
		}
	}

	// add the set of filter characters to the list of characters to be recognized
	if(!strFilterChars.empty())
	{
		_bstr_t _bstrCharSet(strFilterChars.c_str());
		WCHAR *pFilterPlus =  _bstrCharSet;
		THROW_UE_ON_ERROR("ELI18213", "Unable to set filter characters in the OCR engine.",
			kRecSetFilterPlus(0, pFilterPlus));
	}

	// setup the default filter option to filter only the specified characters
	THROW_UE_ON_ERROR("ELI03431", "Unable to set default filter options in the OCR engine.",
		kRecSetDefaultFilter(0, filter) );

	// retain the most recently enabled filter set
	m_eFilter = eFilter;
	
	// display a messagebox with the enabled characters if the setting 
	// to display this messagebox is enabled.
	if (m_eDisplayFilterCharsType == kDisplayCharsTypeAlways || 
		(m_eDisplayFilterCharsType == kDisplayCharsTypeOnChange && 
		strFilterChars != ms_strLastDisplayedFilterChars))
	{
		if(eFilter == kNoFilter)
		{
			AfxMessageBox("OCR Filters removed");
		}
		else
		{
			AfxMessageBox(strFilterChars.c_str());
		}
		ms_strLastDisplayedFilterChars = strFilterChars;
	}	
}
//-------------------------------------------------------------------------------------------------
bool CScansoftOCR2::supportsTrainingFiles()
{
	return true;
}
//-------------------------------------------------------------------------------------------------
void CScansoftOCR2::loadTrainingFile(std::string strTrainingFileName)
{
	// nullify any training file that may be currently loaded
	THROW_UE_ON_ERROR("ELI10508", "Unable to load specified training file in the OCR engine!",
		kRecSetTrainingFileName(0, NULL));

	// load the training file specified by the caller
	_bstr_t _bstrTrainingFileName(strTrainingFileName.c_str());
	if (_bstrTrainingFileName.length() > 0)
	{
		// create the name of the training file
		string strTrainingFile = getThisDLLFolder() + "\\";
		strTrainingFile += strTrainingFileName;
		strTrainingFile += ".stf";

		if (isValidFile(strTrainingFileName))
		{
			// the training file exists - try to load it
			THROW_UE_ON_ERROR("ELI03481", "Unable to load specified training file in the OCR engine!",
				kRecSetTrainingFileName(0, strTrainingFile.c_str()));
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CScansoftOCR2::init()
{
	try
	{
		// initialize the Scansoft OCR engine and any necessary licensing thereof
		initEngineAndLicense();

		// Get the RecAPI version number
		m_strVersion = "Nuance " + asString(kRecGetVersion() / 100.0, 2);

		// Initialize our registry settings manager
		m_apCfg = unique_ptr<ScansoftOCRCfg>(new ScansoftOCRCfg());

		applyCommonSettings();

		// load the filter chars display related settings from our
		// persistence settings store
		m_eDisplayFilterCharsType = m_apCfg->getDisplayFilterChars();
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26100")
}
//-------------------------------------------------------------------------------------------------
void CScansoftOCR2::initEngineAndLicense()
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
				THROW_UE("ELI03386", "Unable to load OCR engine license file!", rc);
			}
			catch (UCLIDException& ue)
			{
				loadScansoftRecErrInfo(ue, rc);
				throw ue;
			}
		}

		// Initialization of OCR engine	
		// Use this to make CreateOutputImage, and thus CreateHtmlFromImage.exe, work:
		rc = RecInitPlus("Extract Systems", "SSOCR2");

		if (rc != REC_OK && rc != API_INIT_WARN)
		{
			// create the exception object to throw to outer scope
			THROW_UE("ELI03371", "Unable to initialize OCR engine!", rc);
		}

		// if rc is API_INIT_WARN, ensure that the required modules are available
		if (rc == API_INIT_WARN)
		{
			LPKRECMODULEINFO pModules;
			size_t size;
			THROW_UE_ON_ERROR("ELI03377", "Unable to obtain modules information from the OCR engine!",
				kRecGetModulesInfo(&pModules, &size));
			
			// if a required library module is not there, do not continue.
			if (pModules[INFO_MOR].Version <= 0)
			{
				THROW_UE("ELI03378", "Unable to find required MOR module for OCR engine to run.", rc);
			}
			if(pModules[INFO_MTX].Version <= 0)
			{
				THROW_UE("ELI16768", "Unable to find required MTX module for OCR engine to run.", rc);
			}
			if (pModules[INFO_PLUS2W].Version <= 0)
			{
				THROW_UE("ELI10510", "Unable to find required PLUS2W module for OCR engine to run.", rc);
			}
			if (pModules[INFO_PLUS3W].Version <= 0)
			{
				THROW_UE("ELI10509", "Unable to find required PLUS3W module for OCR engine to run.", rc);
			}
			if (pModules[INFO_HNR].Version <= 0)
			{
				THROW_UE("ELI11135", "Unable to find required HNR module for OCR engine to run.", rc);
			}
			if (pModules[INFO_RER].Version <= 0)
			{
				THROW_UE("ELI11136", "Unable to find required RER module for OCR engine to run.", rc);
			}
			if(pModules[INFO_DOT].Version <= 0)
			{
				THROW_UE("ELI17016", "Unable to find required DOT module for OCR engine to run.", rc);
			}
		}
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI26101")
}
//-------------------------------------------------------------------------------------------------
string CScansoftOCR2::getThisDLLFolder()
{
	return getModuleDirectory(_Module.m_hInst);
}
//-------------------------------------------------------------------------------------------------
string CScansoftOCR2::getDebugRepresentation(char cChar)
{
	string strResult;

	// return the debug representation of the characters
	switch (cChar)
	{
	// return the C++ notation for non-printable characters
	case ' ': 
		strResult = "   "; break;
	case '\r':
		strResult = "\\r "; break;
	case '\n':
		strResult = "\\n "; break;
	case '\t':
		strResult = "\\t "; break;
	default:
		{
			// return the usual representation for the printable characters
			strResult.assign(1, cChar);
			strResult = padCharacter(strResult, false, ' ', 3);
			break;
		}
	}

	return strResult;
}
//-------------------------------------------------------------------------------------------------
void CScansoftOCR2::logDebugInfo(const string& strText,
								vector<CPPLetter>* pvecLetters)
{
	// determine the location of the log file
	string strLogFile = getModuleDirectory(_Module.m_hInst);
	strLogFile += "\\SSOCR.log";

	// write the string length and vector length to the output file
	ofstream outfile(strLogFile.c_str());
	if (!outfile.is_open())
	{
		UCLIDException ue("ELI34218", "Log file could not be opened.");
		ue.addDebugInfo("Filename", strLogFile);
		ue.addWin32ErrorInfo();
		throw ue;
	}

	// write information about the type of debug info:
	long nLength = (pvecLetters == NULL) ? strText.length() : pvecLetters->size();
	string strType = (pvecLetters == NULL) ? string("String") : string("Vector");
	outfile << "Type: " << strType << endl;
	outfile << "Length: " << nLength << endl;

	// iterate through the character positions and print
	// information about them to the output file
	for (int i = 0; i < nLength; i++)
	{
		// write position information to the output file
		string strTemp = padCharacter(asString(i), false, ' ', 6);
		outfile << strTemp;

		// output information about the character in the string or letter
		if (pvecLetters == NULL)
		{
			char cTemp = strText[i];
			outfile << getDebugRepresentation(cTemp);
		}
		else
		{
			// output information about the letter in the letters vector
			CPPLetter& letter = (*pvecLetters)[i];

			char cTemp = (char) letter.m_usGuess1;
			outfile << getDebugRepresentation(cTemp);
		}

		outfile << endl;
	}

	// Close the file and wait for it to be readable
	outfile.close();
	waitForFileToBeReadable(strLogFile);
}
//-------------------------------------------------------------------------------------------------
void CScansoftOCR2::appendSlashRSlashN(vector<CPPLetter>* pvecLetters, unsigned long ulCount)
{
	// remove any preceeding space character [P13 #4750]
	if(!pvecLetters->empty() && pvecLetters->back().m_usGuess1 == ' ')
	{
		// get and remove the space character
		// NOTE: we only need to get and remove a single space, 
		// because we enforce that double spaces cannot occur.
		CPPLetter letterRemovedSpace = pvecLetters->back();
		pvecLetters->pop_back();

		// since we are removing this space, we should make sure that the preceeding character
		// has the appropriate end of zone and end of paragraph information
		if(!pvecLetters->empty())
		{
			CPPLetter& rletterPreceeding = pvecLetters->back();
			
			// transfer the removed space's end of paragraph and zone to the preceeding character
			rletterPreceeding.m_bIsEndOfParagraph = letterRemovedSpace.m_bIsEndOfParagraph;
			rletterPreceeding.m_bIsEndOfZone = letterRemovedSpace.m_bIsEndOfZone;
		}
	}

	// append as many \r\n as requested
	for (unsigned int i = 0; i < ulCount; i++)
	{
		pvecLetters->push_back(gletterSLASH_R);
		pvecLetters->push_back(gletterSLASH_N);
	}
}
//-------------------------------------------------------------------------------------------------
void CScansoftOCR2::addRecognizedLettersToVector(vector<CPPLetter>* pvecLetters, long nPageNum,
												 long lVerticalDPI, bool bReturnUnrecognized)
{
	// ensure that the vector is valid
	ASSERT_ARGUMENT("ELI06187", pvecLetters != __nullptr);

	// get the spatial information for each of the recognized characters
	// from the OCR engine
	LETTER *pScansoftLetters;
	long lNumLetters;
	RECERR rc;
	rc = kRecGetLetters(m_hPage, II_CURRENT, &pScansoftLetters, &lNumLetters);

	// check for errors
	if(rc != REC_OK)
	{
		if (rc != NO_TXT_WARN)
		{
			UCLIDException ue("ELI05777", "Unable to obtain spatial information for the recognized characters!");
			loadScansoftRecErrInfo(ue, rc);
			ue.addDebugInfo("Page Number", nPageNum);
			throw ue;
		}

		// no text was found
		return;
	}
	
	// ensure the memory for the ScanSoft letter object is handled
	RecMemoryReleaser<LETTER> LetterMemoryReleaser(pScansoftLetters);

	// reserve at least enough space for the letters
	pvecLetters->reserve((unsigned int) lNumLetters);

	// create, if not already created, a CPPLetter to hold the current recognized character.
	// ensure that all letters will be stored as spatial.
	static CPPLetter letter(-1,-1,-1,-1,-1,-1,-1,-1,false,false,true,0,100,0);

	// set all recognized letters to the appropriate page number
	letter.m_usPageNumber = (unsigned short) nPageNum;

	// calculate the number to multiply a character's height (in pixels)
	// to determine that character's font size (in points)
	double dFontSizeMultiplier;
	dFontSizeMultiplier = 100.0 / lVerticalDPI;

	// populate the vector with spatial information for each of the recognized characters	
	LETTER* pCurLetter;
	const LETTER* pLastLetter;
	pLastLetter = &pScansoftLetters[lNumLetters-1];
	
	for (pCurLetter = pScansoftLetters; pCurLetter <= pLastLetter; pCurLetter++)
	{
		// [LegacyRCAndUtils:6246]
		// To provide results consistent with the code prior to fixing the problem where we ignored
		// the last character, if the last character is whitespace (seems to be the case in most
		// situations), disregard it.
		if (pCurLetter == pLastLetter && isWhitespaceChar(pCurLetter->code))
		{
			break;
		}

		bool bIsRecognized = isRecognizedCharacter(pCurLetter->code);

		if (bIsRecognized)
		{
			convertToCodePage((unsigned short*)&pCurLetter->code);
		}

		// add this character to the vector if it is a positive-width, recognized character
		// or if it is a positive-width character AND unrecognized characters should be returned.
		// NOTE: RecAPI v15 uses a width-less dummy space to mark the end of a line.
		// We don't add these dummy spaces to maintain backwards-compatibility.
		if(pCurLetter->width > 0 && (bIsRecognized || bReturnUnrecognized || m_bReturnUnrecognizedCharacters))
		{
			// check if this character is a space
			bool bIsSpace = pCurLetter->code == ' ';
			bool bIsTabSpace = bIsSpace && pCurLetter->spcInfo.spcType == SPC_TAB;

			if (!bIsSpace || m_bAssignSpatialInfoToSpaceCharacters)
			{
				// store the position of the current letter
				letter.m_ulTop = pCurLetter->top;
				letter.m_ulLeft = pCurLetter->left;
				letter.m_ulBottom = pCurLetter->top + pCurLetter->height;
				letter.m_ulRight = pCurLetter->left + pCurLetter->width;

				// get the first guess for this character if it was recognized
				// or the unrecognized symbol if it was not
				// NOTE: The structure of LETTER has changed from RecAPI version 12 to version 15. 
				//       LETTER now contains an array of values of all possible guesses, rather
				//       than the first three guesses. The value of the first guess is being stored
				//       in CPPLetter's first, second, and third guess, since the second and third 
				//       guesses are not currently used and since it is preferable to give them a 
				//       somewhat meaningful value rather than leave them uninitialized. In future 
				//       builds, CPPLetter will be modified to either 
				//       (1) Store and use all possible guesses for the character.
				//       (2) Only store the first guess.
				letter.m_usGuess1 = letter.m_usGuess2 = letter.m_usGuess3 = 
					(bIsTabSpace && m_bOutputTabCharactersForTabSpaceType
						? '\t'
						: bIsRecognized ? pCurLetter->code : gcUNRECOGNIZED);

				if (m_eOCRFindType == kFindBarcodesOnly)
				{
					letter.m_usGuess2 = RH_BARTYPE(pCurLetter->info);
				}
			}

			if(bIsSpace)
			{
				// Unless m_bOutputMultipleSpaceCharacterSequences = true, append this space if and only if the preceding character is not a space [P13 #4868]
				if(m_bOutputMultipleSpaceCharacterSequences || !pvecLetters->empty() && !isWhitespaceChar(pvecLetters->back().m_usGuess1))
				{
					int nSpaceCount = m_bOutputOneSpaceCharacterPerCount
						? max(pCurLetter->spcInfo.spcCount, 1)
						: 1;

					if (m_bAssignSpatialInfoToSpaceCharacters)
					{
						if (nSpaceCount == 1)
						{
							pvecLetters->push_back(letter);
						}
						else
						{
							// Divide the space character into multiple characters, if required,
							// spreading the spatial info among them
							double dWidth = ((double)pCurLetter->width) / nSpaceCount;
							unsigned long ulStart = pCurLetter->left;
							unsigned long ulLastRight = ulStart;

							for (int i = 1; i <= nSpaceCount; ++i)
							{
								letter.m_ulLeft = ulLastRight;
								letter.m_ulRight = ulStart + (unsigned long)round(i * dWidth);
								ulLastRight = letter.m_ulRight;
								pvecLetters->push_back(letter);
							}
						}
					}
					else
					{
						// append this space as a non-spatial character [P13 #4772]
						for (int i = 0; i < nSpaceCount; ++i)
						{
							pvecLetters->push_back(bIsTabSpace && m_bOutputTabCharactersForTabSpaceType
								? gletterTAB
								: gletterSPACE);
						}
					}
				}
			}
			else // this is a non-space letter
			{
				// get the font size
				// NOTE: RecAPI v15 no longer provides fontSize information. [P13 #4469]
				// This is the formula that RecAPI v12.7 used internally to determine fontSize.
				letter.m_ucFontSize = (unsigned char) (pCurLetter->capHeight * dFontSizeMultiplier);

				if (bIsRecognized)
				{
					// Scansoft uses 0 as highest confidence 100 as lowest so we need to reverse that.
					// Ignore the bit that ScanSoft uses to denote suspect words.
					// Use a maximum on 99 so that the lowest possible confidence for non-space letters is 1
					// (see else clause below)
					// https://extract.atlassian.net/browse/ISSUE-15752
					letter.m_ucCharConfidence = 100 - min(99, (pCurLetter->err & RE_ERROR_LEVEL_MASK));

					// set font attributes
					letter.m_ucFont = 0;
					if((pCurLetter->fontAttrib & R_ITALIC) != 0)
					{
						letter.setItalic(true);
					}
					if((pCurLetter->fontAttrib & R_BOLD) != 0)
					{
						letter.setBold(true);
					}
					if((pCurLetter->fontAttrib & R_SERIF) != 0)
					{
						letter.setSerif(true);
					}
					else if((pCurLetter->fontAttrib & R_SANSSERIF) != 0)
					{
						letter.setSansSerif(true);
					}
					if((pCurLetter->fontAttrib & R_PROPORTIONAL) != 0)
					{
						letter.setProportional(true);
					}
					if((pCurLetter->fontAttrib & R_UNDERLINE) != 0)
					{
						letter.setUnderline(true);
					}
					if((pCurLetter->fontAttrib & R_SUPERSCRIPT) != 0)
					{
						letter.setSuperScript(true);
					}
					if((pCurLetter->fontAttrib & R_SUBSCRIPT) != 0)
					{
						letter.setSubScript(true);
					}
				}
				else
				{
					// [FlexIDSCore:4590]
					// Assign unrecognized characters a confidence of 1 so that when testing based
					// on confidence it can't be confused with an empty string.
					letter.m_ucCharConfidence = 1;

					// If the character is not recognized, don't allow any font attributes to be
					// applied.
					letter.m_ucFont = 0;
				}
			
				// append the letter object to the list of letters for this page
				pvecLetters->push_back(letter);
			}
		}

		// ensure that the vector of letters is not empty
		// [LegacyRCAndUtils:6246]
		// To provide results consistent with the code prior to fixing the problem where we
		// ignored the last character, if this is the last character, do not add any newlines.
		if(!pvecLetters->empty() && pCurLetter != pLastLetter)
		{
			// if this is the end of a paragraph, line, or zone,
			// add that information to the last recognized page letter
			setLastPageLetterBoundary(pvecLetters, pCurLetter);
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CScansoftOCR2::setLastPageLetterBoundary(vector<CPPLetter>* pvecPageLetters, 
											  LETTER* pletterScanSoft)
{
	// get the last recognized character from the list of page letters
	CPPLetter &rletterCPP = pvecPageLetters->back();

	// store whether this letter is the end of a paragraph or a zone
	rletterCPP.m_bIsEndOfParagraph = (pletterScanSoft->makeup & R_ENDOFPARA) != 0;
	rletterCPP.m_bIsEndOfZone = (pletterScanSoft->makeup & R_ENDOFZONE) != 0;

	// insert a blank line if this is the end of a paragraph
	// or zone, depending on the values of m_bIgnoreParagraphFlag and m_bTreatZonesAsParagraphs
	if (rletterCPP.m_bIsEndOfParagraph && !m_bIgnoreParagraphFlag
		|| rletterCPP.m_bIsEndOfZone && m_bTreatZonesAsParagraphs)
	{
		// append \r\n\r\n
		appendSlashRSlashN(pvecPageLetters, 2);
	}
	else if ((pletterScanSoft->makeup & gwEND_OF_LINE_OR_ZONE) != 0)
	{
		// this letter is the end of a line or zone
		// append \r\n
		appendSlashRSlashN(pvecPageLetters, 1);
	}
}
//-------------------------------------------------------------------------------------------------
void CScansoftOCR2::getRecognizedText(string& rstrText, bool bReturnUnrecognized)
{
	// get the letters of the recognized text
	LETTER *pScansoftLetters;
	long nNumLetters;
	RECERR rc = kRecGetLetters(m_hPage, II_CURRENT, &pScansoftLetters, &nNumLetters);
	
	// check for errors
	if(rc != REC_OK && rc != NO_TXT_WARN)
	{
		UCLIDException ue("ELI16646", "Unable to obtain recognized text.");
		loadScansoftRecErrInfo(ue, rc);
		ue.log();
	}

	// keep track of memory for scansoft letters object
	RecMemoryReleaser<LETTER> LetterMemoryReleaser(pScansoftLetters);

	// iterate through each letter, adding it to the string
	rstrText = "";
	for (int i = 0; i < nNumLetters; i++)
	{
		bool bIsRecognized = isRecognizedCharacter(pScansoftLetters[i].code);
		if(bReturnUnrecognized || bIsRecognized)
		{
			if (bIsRecognized)
			{
				convertToCodePage((unsigned short*)&pScansoftLetters[i].code);
				rstrText += (char)pScansoftLetters[i].code;
			}
			else
			{
				rstrText += gcUNRECOGNIZED;
			}
		}
	}	
}
//-------------------------------------------------------------------------------------------------
bool CScansoftOCR2::hasContent(const string& strText, 
							  const vector<CPPLetter>* pvecLetters)
{
	// return true depending upon whether the string or vector has any content
	if (pvecLetters == NULL)
	{
		return strText.length() > 0;
	}
	else
	{
		return pvecLetters->size() > 0;
	}
}
//-------------------------------------------------------------------------------------------------
void CScansoftOCR2::appendContent(string& rstrText, vector<CPPLetter>* pvecLetters,
								 const string& strTextToAppend, 
								 const vector<CPPLetter>* pvecLettersToAppend)
{
	// append either the string, or the vector to the corresponding string or
	// vector, depending upon whether the target vector pointer is NULL
	if (pvecLetters == NULL)
	{
		rstrText.append(strTextToAppend);
	}
	else
	{
		pvecLetters->insert(pvecLetters->end(), pvecLettersToAppend->begin(), pvecLettersToAppend->end());
	}
}
//-------------------------------------------------------------------------------------------------
RECERR __stdcall CScansoftOCR2::ProgressMon(LPPROGRESSMONITOR mon, void* pContext) // Static
{
	try
	{
		// signal the timeout thread that progress has been made
		if (ms_pInstance)
		{
			ms_pInstance->m_eventProgressMade.signal();
		}
		
		// check if progress status updates are enabled
		if (ms_bUpdateProgressStatus)
		{
			// enter critical section
			CSingleLock lock(&ms_mutexProgressStatus, TRUE);

			// update progress status variables
			ms_lProcessID = mon->ProcessId;
			ms_lPercentComplete = mon->Percent;
			ms_lPageIndex = ms_uiCurrentPageIndex;
			ms_lPageNumber = ms_lCurrentPageNumber;

			// exit critical section
			lock.Unlock();

			// empty the message queue to check if the outer scope of
			// IScansoftOCR2 is trying to receive progress status updates
	        emptyWindowsMessageQueue();
		}		
		return REC_OK;
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI12826");

	return API_PROCESS_ABORTED_ERR;
}
//-------------------------------------------------------------------------------------------------
UINT CScansoftOCR2::timeoutThreadProc(void* pParam)
{
	CoInitializeEx(NULL, COINIT_MULTITHREADED);
	try
	{
		CScansoftOCR2* pOCR = (CScansoftOCR2*)pParam;
		pOCR->timeoutLoop();
	}
	// TODO: what should be done on an exception
	CATCH_AND_DISPLAY_ALL_EXCEPTIONS("ELI11146");
	CoUninitialize();

	return 0;
}
//-------------------------------------------------------------------------------------------------
void CScansoftOCR2::timeoutLoop()
{
	HANDLE eventHandles[2];
	eventHandles[0] = m_eventKillTimeoutThread.getHandle();
	eventHandles[1] = m_eventProgressMade.getHandle();
	while(1)
	{
		DWORD ret = WaitForMultipleObjects(2, eventHandles, FALSE, m_nTimeoutLength);
		// If the wait times out that means the timeout has expired so we can kill the process
		if( ret == WAIT_TIMEOUT )
		{
			RecQuitPlus();

			DWORD pid = GetCurrentProcessId();
			HANDLE hProcess = OpenProcess(PROCESS_ALL_ACCESS, FALSE, pid);
			TerminateProcess(hProcess, 0);

			// Cleanup temporary data files
			recursiveRemoveDirectory(getTemporaryDataFolder(pid));
			break;
		}
		// If the kill thread object gets signaled
		// we will exit the thread
		else if (ret == WAIT_OBJECT_0)
		{
			break;
		}
		// if the progress made event is signaled we will 
		// just continue with the loop because when 
		// we get back to the wait at the top the wait will be reset
		else if (ret == WAIT_OBJECT_0 + 1)
		{
//			getLogFile()->writeLine("CScansoftOCR2::timeoutLoop Progress Made Wait for kill or progress");
		}
		else
		{
			THROW_LOGIC_ERROR_EXCEPTION("ELI11147");
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CScansoftOCR2::rotateAndRecognizeTextInImagePage(const string& strImageFileName, long nPageNum, 
	LPRECT pZone, long nRotationInDegrees, bool bDetectHandwriting, bool bReturnUnrecognized,
	string& rstrText, vector<CPPLetter>* pvecLetters, ILongToObjectMapPtr ipPageInfos)
{
	// If force despeckle is called, color/grayscale images need to be binarized on load or it will fail
	if (m_eForceDespeckle == kAlwaysForce)
	{
		kRecSetImgConvMode(0, CNV_SET);
	}

	// load the specified page of the image file
	// NOTE: RecAPI uses zero-based page number indexes
	loadPageFromImageHandle(strImageFileName, m_hImageFile, nPageNum-1, &m_hPage);

	// Ensure that the page image and recognition data are released
	RecMemoryReleaser<RECPAGESTRUCT> PageMemoryReleaser(m_hPage);

	// Delete all zones
	THROW_UE_ON_ERROR("ELI12720", "Unable to delete zones!", 
		kRecDeleteAllZones(m_hPage) );

	// Fill area outside of zone so that it doesn't influence things like rotation/skew detection and zone location
	if (pZone != __nullptr && m_bIgnoreAreaOutsideSpecifiedZone)
	{
		THROW_UE_ON_ERROR("ELI47242", "Unable to fill outside of specified zone!",
			kRecFillImgArea(0, m_hPage, pZone, 0xFFFFFFFF, FILL_OUTSIDE));
	}

	// Determine requirement for rotation and/or deskew
	int slope = 0;
	IMG_ROTATE imgRotate = ROT_NO;
	THROW_UE_ON_ERROR("ELI12719", "Unable to detect image skew in the OCR engine!",
			kRecDetectImgSkew(0, m_hPage, &slope, &imgRotate));

	// Get the orientation to use based on nRotationInDegrees.
	EOrientation orientation = getOrientation(nRotationInDegrees, imgRotate);

	// Define a new zone from pZone info
	// NOTE: Handwriting recognition is handled after rotation and deskew
	// If the area outside the zone was already filled then don't need to create a user zone
	// (not creating a user zone helps makes OCR results when reOCRing via Select Page Region more consistent with standard OCR results)
	if (pZone != __nullptr && !bDetectHandwriting && !m_bIgnoreAreaOutsideSpecifiedZone)
	{
		// Define and initialize a ZONE
		ZONE zone;
		memset( &zone, 0, sizeof(ZONE) );

		// Set rectangle
		zone.rectBBox.left = pZone->left;
		zone.rectBBox.top = pZone->top;
		zone.rectBBox.right = pZone->right;
		zone.rectBBox.bottom = pZone->bottom;

		// Set other items
		zone.fm = m_eDefaultFillingMethod;
		zone.rm = m_eDefaultFillingMethod == FM_OMNIFONT
			? RM_OMNIFONT_PLUS3W
			: RM_AUTO; // Use this so that a compatible recognition module will be chosen automatically based on the filling method
		zone.type = m_bLocateZonesInSpecifiedZone ? WT_AUTO : WT_FLOW;
		zone.filter = FILTER_DEFAULT;

		try
		{
			// Insert the new zone
			THROW_UE_ON_ERROR("ELI12721", "Unable to insert zone.",
				kRecInsertZone(m_hPage, II_CURRENT, &zone, 0) );
		}
		catch(UCLIDException& uex)
		{
			// Add zone debug info
			uex.addDebugInfo("Zone left", zone.rectBBox.left);
			uex.addDebugInfo("Zone top", zone.rectBBox.top);
			uex.addDebugInfo("Zone right", zone.rectBBox.right);
			uex.addDebugInfo("Zone bottom", zone.rectBBox.bottom);
			uex.addDebugInfo("pZone left", pZone->left);
			uex.addDebugInfo("pZone top", pZone->top);
			uex.addDebugInfo("pZone right", pZone->right);
			uex.addDebugInfo("pZone bottom", pZone->bottom);
			throw uex;
		}
	}
	// else OCR engine will locate the zones

	// Get and store image size information
	IMG_INFO info;
	THROW_UE_ON_ERROR("ELI12724", "Unable to get image info in the OCR engine!",
			kRecGetImgInfo(0, m_hPage, II_CURRENT, &info));

	// calculate the deskew in degrees
	double dDeskew = convertRadiansToDegrees(atan2((double) slope, 1000.0));

	// check if this angle of deskew is large/small enough to be used by the RecAPI engine
	if ( isDeskewable(dDeskew, info) )
	{
		// deskew the image
		THROW_UE_ON_ERROR("ELI16828", "Unable to deskew image.",
			kRecDeskewImg(0, m_hPage, slope));
	}
	else
	{
		// no deskew will be applied
		dDeskew = 0;
	}

	// Create a SpatialPageInfo object to hold the gathered page data.
	ISpatialPageInfoPtr ipPageInfo(CLSID_SpatialPageInfo);
	ASSERT_RESOURCE_ALLOCATION("ELI12722", ipPageInfo != __nullptr);

	ipPageInfo->Initialize(info.Size.cx, info.Size.cy, orientation, dDeskew);
		
	// rotate the image
	THROW_UE_ON_ERROR("ELI12723", "Unable to rotate image.",
		kRecRotateImg(0, m_hPage, imgRotate));

	if (m_eForceDespeckle == kAlwaysForce
		|| m_eForceDespeckle == kForceWhenBitonal && info.BitsPerPixel == 1)
	{
		THROW_UE_ON_ERROR("ELI36823", "Unable to despeckle image.",
			kRecForceDespeckleImg(0, m_hPage, pZone, m_eForceDespeckleMethod, m_nForceDespeckleLevel));
	}
	
	// create zones for handwriting recognition
	if(bDetectHandwriting)
	{
		LPRECT pArea = pZone;
		RECT rectArea;

		if(!pArea)
		{
			rectArea.top = rectArea.left = 0;
			rectArea.bottom = info.Size.cy;
			rectArea.right = info.Size.cx;

			pArea = &rectArea;
		}

		// [FlexIDSCore:4406]
		// If the image has a rotated orientation, the specified area to OCR must be rotated into
		// text's coordinate system.
		if (imgRotate != ROT_NO)
		{
			long degress;
			switch (imgRotate)
			{
				case ROT_LEFT:	degress = -90; break;
				case ROT_DOWN:	degress = 180; break;
				case ROT_RIGHT: degress = 90; break;
				default:
					THROW_LOGIC_ERROR_EXCEPTION("ELI32933");
			}

			rotateRectangle(*pArea, info.Size.cx, info.Size.cy, degress);
		}

		createZonesFromLineRemoval(pArea);

		prepareHandwritingZones(pArea, info);
	}
	else if(pZone == NULL || m_bLocateZonesInSpecifiedZone)
	{
		// OCR Engine will find appropriate zones
		RECERR rc = kRecLocateZones(0, m_hPage);

		// if zones were not located, stop OCRing this page
		if (rc != REC_OK)
		{
			// log an error, unless no zones were found at all (eg. the image contained no text)
			if (rc != ZONE_NOTFOUND_ERR && rc != ZONE_NOTFOUND_WARN)
			{
				UCLIDException ue("ELI16769", "Unable to locate zones. OCR aborted.");
				loadScansoftRecErrInfo(ue, rc);
				ue.addDebugInfo("Image Name", strImageFileName);
				ue.addDebugInfo("Page Number", nPageNum);
				ue.log();
			}

			return;
		}

		// order the zones if the zone ordering option is set
		if(m_bOrderZones)
		{
			orderZones();
		}
	}
	// else Zone was defined before RecRotateImg()

	// Recognize the text in the rotated zone or zones
	RECERR rc = kRecRecognize(0, m_hPage, NULL);

	if (rc == REC_OK)
	{
		// check if the result should be returned as 
		// a vector of CPPLetters or as a standard string
		if (pvecLetters != __nullptr)
		{
			// store the text as a vector of CPPLetters
			addRecognizedLettersToVector(pvecLetters, nPageNum, info.DPI.cy, bReturnUnrecognized);	
		}
		else
		{
			// store the recognized text as a standard string
			getRecognizedText(rstrText, bReturnUnrecognized);
		}

		// Add the local SpatialPageInfo object as the appropriate entry
		// in the Page Infos map
		if (ipPageInfos)
		{
			ipPageInfos->Set( nPageNum, ipPageInfo );
		}
	}
	else if (rc == API_GPFAULT_ERR)
	{
		UCLIDException ue("ELI12726", "Unrecoverable Error in the ENGine!");
		loadScansoftRecErrInfo(ue, rc);
		ue.addDebugInfo("Image Name", strImageFileName);
		ue.addDebugInfo("Page Number", nPageNum);
		throw ue;
	}
	else if (rc == IMG_SIZE_ERR)
	{
		// prepare error message
		UCLIDException ue("ELI16245", "Unsupported image size.");
		loadScansoftRecErrInfo(ue, rc);
		ue.addDebugInfo("Image Name", strImageFileName);
		ue.addDebugInfo("Page Number", nPageNum);

		// add the page size debug info
		addPageSizeDebugInfo(ue, info);

		// log error
		ue.log();
	}
	else if (rc == API_TIMEOUT_ERR)
	{
		UCLIDException ue("ELI46052", "Recognition timeout");
		loadScansoftRecErrInfo(ue, rc);
		ue.addDebugInfo("Image Name", strImageFileName);
		ue.addDebugInfo("Page Number", nPageNum);
		throw ue;
	}
	else if (rc == API_HARDTIMEOUT_ERR)
	{
		UCLIDException ue("ELI46053", "Recognition hard timeout");
		loadScansoftRecErrInfo(ue, rc);
		ue.addDebugInfo("Image Name", strImageFileName);
		ue.addDebugInfo("Page Number", nPageNum);
		ue.log();

		RecQuitPlus();

		// Nuance recommends terminating the process and starting clean if there is a hard timeout
		DWORD pid = GetCurrentProcessId();
		HANDLE hProcess = OpenProcess(PROCESS_ALL_ACCESS, FALSE, pid);
		TerminateProcess(hProcess, 0);

		// Cleanup temporary data files
		recursiveRemoveDirectory(getTemporaryDataFolder(pid));
	}
	else if(rc != NO_TXT_WARN)
	{
		// we have come across an unexpected return code
		// we need to keep trying to recognize the text in 
		// the remaining zones...so don't throw an exception
		// just log the exception and move on
		UCLIDException ue("ELI12727", "Unable to recognize text on image page.");
		loadScansoftRecErrInfo(ue, rc);
		ue.addDebugInfo("Image Name", strImageFileName);
		ue.addDebugInfo("Page Number", nPageNum);
		ue.log();
	}
}
//-------------------------------------------------------------------------------------------------
void CScansoftOCR2::createZonesFromLineRemoval(const RECT* pArea)
{
	THROW_UE_ON_ERROR("ELI18032", "Unable to remove horizontal lines from image prior to recognition.",
		kRecLineRemoval(0, m_hPage, pArea));

	int iLines;
	THROW_UE_ON_ERROR("ELI18040", "Unable to obtain count of detected lines.",
		kRecGetLineCount(m_hPage, &iLines));

	static list<long> listHorizontals;
	static RLINE line;
	double dLineSlope, dHorizontalDistance;
	RECERR rc;

	listHorizontals.clear();
	for(int i=0; i<iLines; i++)
	{
		rc = kRecGetLineInfo(m_hPage, II_CURRENT, &line, i);

		if(rc != REC_OK)
		{
			// log an error
			UCLIDException ue("ELI18041", "Unable to get detected line information.");
			loadScansoftRecErrInfo(ue, rc);
			ue.addDebugInfo("Line Index", i);
			ue.log();

			// skip this line and continue to the next one
			continue;
		}

		// compute the horizontal distance the line spans
		dHorizontalDistance = line.start.x - line.end.x;

		// skip this line if it is vertical
		if(dHorizontalDistance == 0)
		{
			continue;
		}

		// compute the slope of this line
		dLineSlope = (line.start.y - line.end.y) / dHorizontalDistance;

		// check if this line is horizontal (or very near horizontal)
		if(dLineSlope < gdMAX_HORIZONTAL_LINE_SLOPE && dLineSlope > -gdMAX_HORIZONTAL_LINE_SLOPE)
		{
			// store the y coordinate of the top of the line
			listHorizontals.push_back(long(min(line.start.y,line.end.y) - line.width/2.0));
		}
	}

	// sort the y-coordinates of the detected horizontal lines
	listHorizontals.sort();

	// clear the zones
	m_listZones.clear();

	// prepare a zone for insertion
	static ZONE zone;
	memset(&zone, 0, sizeof(ZONE));

	zone.type = WT_FLOW;
	zone.fm = m_eDefaultFillingMethod;
	zone.rm = RM_OMNIFONT_PLUS3W;
	zone.filter = FILTER_ALL;

	// set the top, left, and right coordinates
	zone.rectBBox.left = pArea->left;
	zone.rectBBox.right = pArea->right;
	zone.rectBBox.top = pArea->top;

	// iterate through each y-coordinate
	for each(long horizontal in listHorizontals)
	{
		// check if the distance between this horizontal line and the top of the 
		// current zone is at least the minimum height for an omnifont zone
		if(horizontal - zone.rectBBox.top >= giMIN_OMNIFONT_ZONE_HEIGHT)
		{
			// add this region to the list of zones and the RecAPI engine
			zone.rectBBox.bottom = horizontal;
			m_listZones.push_back(zone);

			// set the top of the next zone to the bottom of the most recently added zone
			zone.rectBBox.top = horizontal;
		}
		else if(!m_listZones.empty() && m_listZones.back().rectBBox.bottom != horizontal)
		{
			// the distance was less than the minimum for an omnifont zone.
			// this line was skipped if the first zone has not been added yet or 
			// if this line is aligned with the bottom of the most recently added zone.

			// expand the bottom of the most recently added zone to the top of this line
			m_listZones.back().rectBBox.bottom = horizontal;

			// set the top of the next zone to bottom of the most recently added zone
			zone.rectBBox.top = horizontal;
		}
	}
	
	// check if no zones have been added yet, or if the distance between the most recently added zone 
	// and bottom of the area being OCRed is larger than the omnifont zone
	if(m_listZones.empty() || pArea->bottom - m_listZones.back().rectBBox.top >= giMIN_OMNIFONT_ZONE_HEIGHT)
	{
		// add the last zone
		zone.rectBBox.bottom = pArea->bottom;
		m_listZones.push_back(zone);
	}
	else 
	{
		// m_listZones isn't empty, and the distance between the most recently added zone and the bottom
		// of the area being OCRed is smaller than the minimum omnifont zone.

		// expand the most recently added zone to the base of the area being OCRed.
		m_listZones.back().rectBBox.bottom = pArea->bottom;
	}

	// insert m_listZones into the RecAPI engine
	insertZones();
}
//-------------------------------------------------------------------------------------------------
void CScansoftOCR2::prepareHandwritingZones(const RECT* pArea, const IMG_INFO& imgInfo)
{
	// create a backup copy of m_listZones and clear m_listZones
	static list<ZONE> listBackupZones;
	listBackupZones.clear();
	listBackupZones.splice(listBackupZones.begin(),m_listZones);

	// instantiate iterators if they are not already instantiated
	static list<ZONE>::iterator iterZone;
	static list<ZONE>::iterator iterOtherZone;
	static list<ZONE>::iterator iterEnd = m_listZones.end();
	static list<ZONE>::iterator iterBackupEnd = listBackupZones.end();

	// recognize the printed text in the area
	// NOTE: we do not need to wrap m_hPage in RecMemoryReleaser,
	// because the outer scope has already done this.
	RECERR rc = kRecRecognize(0, m_hPage, NULL);
	
	// ensure text was recognized
	while(rc != NO_TXT_WARN)
	{
		if(rc != REC_OK)
		{
			THROW_UE("ELI18053", "Unable to recognize printed text.", rc);
		}

		// get the recognized letters
		LETTER* pLetters;
		long lNumLetters;
		rc = kRecGetLetters(m_hPage, II_CURRENT, &pLetters, &lNumLetters);

		// ensure letters were retrieved
		if(rc != REC_OK)
		{
			if(rc != NO_TXT_WARN)
			{
				THROW_UE("ELI18054", "Unable to get recognized letters.", rc);
			}

			// no text was found, stop here
			break;
		}

		// ensure the memory allocated for the letters is released when this object goes out of scope.
		RecMemoryReleaser<LETTER> LetterMemoryReleaser(pLetters);

		// prepare a handwriting zone for insertion
		static ZONE zoneLine;
		memset(&zoneLine, 0, sizeof(ZONE));

		zoneLine.type = WT_FLOW;
		zoneLine.filter = FILTER_DEFAULT;
		zoneLine.fm = FM_HANDPRINT;
		zoneLine.rm = RM_RER;

		// set its dimensions the minimum/maximum values possible.
		// NOTE: zoneLine will contain the smallest region containing
		// all the letters of a line of recognized text.
		zoneLine.rectBBox.top = pArea->bottom;
		zoneLine.rectBBox.left = pArea->right;
		zoneLine.rectBBox.bottom = pArea->top;
		zoneLine.rectBBox.right = pArea->left;

		// compute the minimum height of the RER zone in inches
		int iMinHeightInPixels = int(gdMIN_RER_ZONE_HEIGHT_INCHES * imgInfo.DPI.cy + 0.5);

		// determine whether this image is black and white
		bool bIsBW = (imgInfo.BitsPerPixel == 1);

		// iterate through each letter
		const LETTER* pLastLetter = &pLetters[lNumLetters-1];
		for(LETTER* pCurLetter = (LETTER*) pLetters; pCurLetter <= pLastLetter; pCurLetter++)
		{
			// [LegacyRCAndUtils:6246]
			// To provide results consistent with the code prior to fixing the problem where we
			// ignored the last character, if the last character is whitespace (seems to be the
			// case in most situations), disregard it.
			if (pCurLetter == pLastLetter && isWhitespaceChar(pCurLetter->code))
			{
				break;
			}

			// check if this letter is a space
			if(pCurLetter->code != L' ')
			{
				// remove this letter if it is a high-confidence character and one of the following is true:
				// (a) it is an alphabetic character and the filter contains no alphabetic characters
				// (b) it is a numeral and the filter contains no numerals
				if((!m_bFilterContainsAlpha && isAlphaChar(pCurLetter->code) || 
					!m_bFilterContainsNumeral && isDigitChar(pCurLetter->code)) &&
					(pCurLetter->err & RE_ERROR_LEVEL_MASK) < RE_SUSPECT_THR )
				{
					// remove this letter from the image stored in memory
					removeLetter(pCurLetter, bIsBW);
				}
				else
				{
					// pCurLetter is a non-space character that is not being removed from the image.
					// ensure that zoneLine contains this letter and all the previous letters on this line.
					zoneLine.rectBBox.top = min(zoneLine.rectBBox.top, pCurLetter->top);
					zoneLine.rectBBox.left = min(zoneLine.rectBBox.left, pCurLetter->left);
					zoneLine.rectBBox.bottom = max(zoneLine.rectBBox.bottom, pCurLetter->top + pCurLetter->height);
					zoneLine.rectBBox.right = max(zoneLine.rectBBox.right, pCurLetter->left + pCurLetter->width);
				}
			}

			// check if this letter is the end of a line, paragraph, or zone
			if((pCurLetter->makeup & gwEND_OF_LINE_PARA_OR_ZONE) != 0)
			{
				// if zoneLine is a valid region, insert it into the list of zones.
				if(zoneLine.rectBBox.bottom - zoneLine.rectBBox.top > iMinHeightInPixels
					&& zoneLine.rectBBox.left < zoneLine.rectBBox.right)

				{
					m_listZones.push_back(zoneLine);
				}

				// reset zoneLine to its minimum/maximum possible values
				zoneLine.rectBBox.top = pArea->bottom;
				zoneLine.rectBBox.left = pArea->right;
				zoneLine.rectBBox.bottom = pArea->top;
				zoneLine.rectBBox.right = pArea->left;
			}
		}

		// add the last line found, if the region is valid
		if(zoneLine.rectBBox.bottom - zoneLine.rectBBox.top > iMinHeightInPixels
			&& zoneLine.rectBBox.left < zoneLine.rectBBox.right
			)
		{
			m_listZones.push_back(zoneLine);
		}

		// iterate through each zone
		iterZone = m_listZones.begin();
		for( ; iterZone != iterEnd; iterZone++)
		{
			// check if iterZone is touching the leftmost and rightmost sides of the OCR area
			bool bIsLeftmost = (iterZone->rectBBox.left != pArea->left);
			bool bIsRightmost = (iterZone->rectBBox.right != pArea->right);

			// skip this zone if it spans the width of pArea
			if(bIsLeftmost || bIsRightmost)
			{
				// iterate through every other zone in m_listZones
				for(iterOtherZone = m_listZones.begin(); iterOtherZone != iterEnd; iterOtherZone++)
				{
					// check if any other zone in listZones is horizontally aligned with iterZone.
					// NOTE: if a zone doesn't horizontally overlap by giMAX_ZONE_OVERLAP pixels, 
					// we do not consider them aligned.
					if(iterZone->rectBBox.bottom - iterOtherZone->rectBBox.top >= giMAX_ZONE_OVERLAP &&
						iterOtherZone->rectBBox.bottom - iterZone->rectBBox.top >= giMAX_ZONE_OVERLAP)
					{
						// check if the other zone is to the left of iterZone
						if(bIsLeftmost && iterOtherZone->rectBBox.left < iterZone->rectBBox.left)
						{
							bIsLeftmost = false;

							// if iterZone is also not the rightmost zone, stop now.
							if(!bIsRightmost)
							{
								break;
							}
						}

						// check if the other zone is to the right of iterZone
						if(bIsRightmost && iterOtherZone->rectBBox.right > iterZone->rectBBox.right)
						{
							bIsRightmost = false;

							// if iterZone is also not the leftmost zone, stop now.
							if(!bIsLeftmost)
							{
								break;
							}
						}
					}
				}

				// if there are no zones to the left or right iterZone,
				// expand iterZone in that direction.
				if(bIsLeftmost)
				{
					iterZone->rectBBox.left = pArea->left;
				}
				if(bIsRightmost)
				{
					iterZone->rectBBox.right = pArea->right;
				}
			}
		}

		// clear the recognition data
		THROW_UE_ON_ERROR("ELI18187", "Unable to free recognition data. Possible memory leak.",
			kRecFreeRecognitionData(m_hPage));

		break;
	}

	// if no zones were detected, restore the original zones.
	if(m_listZones.empty())
	{
		// convert each original zone to handprint recognition
		for(iterZone = listBackupZones.begin(); iterZone != iterBackupEnd; iterZone++)
		{
			iterZone->type = WT_FLOW;
			iterZone->filter = FILTER_DEFAULT;
			iterZone->fm = FM_HANDPRINT;
			iterZone->rm = RM_RER;
		}

		m_listZones.splice(m_listZones.begin(), listBackupZones);
	}

	// insert m_listZones into the RecAPI engine
	insertZones();
}
//-------------------------------------------------------------------------------------------------
void CScansoftOCR2::insertZones()
{
	// delete all the zones in the RecAPI engine
	THROW_UE_ON_ERROR("ELI18042", "Unable to delete zones.",
		kRecDeleteAllZones(m_hPage));

	// instantiate variables if not already instantitated
	RECERR rc;
	static list<ZONE>::iterator iterZone, iterEnd = m_listZones.end();

	// insert each zone into the RecAPI engine
	int i=0;
	for(iterZone = m_listZones.begin(); iterZone != iterEnd; iterZone++)
	{
		rc = kRecInsertZone(m_hPage, II_CURRENT, &(*iterZone), i);

		if (rc != REC_OK)
		{
			// log an error
			UCLIDException ue("ELI18052", "Unable to insert zone.");
			loadScansoftRecErrInfo(ue, rc);
			ue.addDebugInfo("Zone index", i);
			ue.log();
			
			// continue on to the next zone
			continue;
		}

		i++;
	}
}
//-------------------------------------------------------------------------------------------------
void CScansoftOCR2::removeLetter(const LETTER* pLetter, bool bIsBW)
{
	// get the boundaries of this letter 
	static RECT rectLetter;
	rectLetter.left = pLetter->left;
	rectLetter.top = pLetter->top;
	rectLetter.right = pLetter->left + pLetter->width;
	rectLetter.bottom = pLetter->top + pLetter->height;
	
	// remove this letter from the image in memory
	// NOTE: kRecClearImgArea whitens the area of a black and white image,
	// and blackens the area of a non-black and white image.
	THROW_UE_ON_ERROR("ELI18055", "Unable to remove character.",
		kRecClearImgArea(0, m_hPage, &rectLetter) );

	// invert the image area just blackened if the image is not black and white (see note above)	
	if(!bIsBW)
	{
		THROW_UE_ON_ERROR("ELI18056", "Unable to invert image area.",
			kRecInvertImgArea(0, m_hPage, &rectLetter));
	}
}
//-------------------------------------------------------------------------------------------------
void CScansoftOCR2::orderZones()
{
	// get total zone count
	int nZones;
	THROW_UE_ON_ERROR("ELI12725", "Unable to get zone count!",
		kRecGetOCRZoneCount(m_hPage, &nZones));

	// prepare variables for zone-ordering
	m_listZones.clear();
	RECERR rc;
	static ZONE zone;
	static list<ZONE>::iterator iterZone, iterEnd;
	long lLastTop=0, lLastLeft=0, lCurTop, lCurLeft, lIterTop;

	// assume the zones are in order until we encounter one that is out of order
	bool bZonesInOrder = true;
	
	// iterate through each recognized zone in the OCR engine
	for(int i=0; i<nZones; i++)
	{
		// get the ith zone
		rc = kRecGetOCRZoneInfo(m_hPage, II_CURRENT, &zone, i);

		// ensure the zone was retrieved properly
		if (rc != REC_OK)
		{
			// log an error
			UCLIDException ue("ELI16764", "Unable to obtain zone information.");
			loadScansoftRecErrInfo(ue, rc);
			ue.addDebugInfo("OCR zone number", i);
			ue.log();

			// try the next zone
			continue;
		}

		// if the text in this zone won't be recognized, just skip it
		if(zone.fm == FM_NO_OCR)
		{
			continue;
		}

		// store this zone's original position in the OCR zone list
		zone.userdata = i;

		// get the coordinates of the current zone
		lCurTop = zone.rectBBox.top;
		lCurLeft = zone.rectBBox.left;

		// ensure that it is below or to the right of the downmost-rightmost zone
		if(lLastTop < lCurTop || 
			(lLastTop == lCurTop && lLastLeft <= lCurLeft) )
		{
			// the current zone is now the downmost-rightmost zone
			lLastTop = lCurTop;
			lLastLeft = lCurLeft;

			// insert the zone at the end of the list
			m_listZones.push_back(zone);
		}
		else
		{
			// mark that the zones were out of order
			bZonesInOrder = false;

			// iterate through the zones to find where the current zone belongs
			iterEnd = m_listZones.end();
			for(iterZone = m_listZones.begin(); iterZone != iterEnd; iterZone++)
			{
				lIterTop = (*iterZone).rectBBox.top;

				// stop when the current zone comes before the next one
				// (ie. this is where the current zone should be for top-down order)
				if(lCurTop < lIterTop ||
					(lCurTop == lIterTop && lCurLeft <= (*iterZone).rectBBox.left))
				{
					break;
				}
			}

			// insert the current zone at this location
			m_listZones.insert(iterZone, zone);
		}
	}
	
	// if the zones were out of order, insert them as user zones in top-down order
	if(!bZonesInOrder)
	{
		// clear the list of zone layouts
		m_listZoneLayouts.clear();
		static ZoneLayoutType zoneLayout;

		// ensure that any memory allocated for the ZoneLayouts is released
		RecMemoryReleaser<CScansoftOCR2> ZoneMemoryReleaser(this);

		// iterate through each zone in the ordered list.
		// NOTE: this loop is separate from the reinsertion loop below, because
		// inserting a single user zone, deletes all the OCR zones according to
		// the ScanSoft documentation
		int i=0;
		iterEnd = m_listZones.end();
		for(iterZone = m_listZones.begin(); iterZone != iterEnd; iterZone++)
		{
			// get the zone layout for this zone from the OCR zone list
			rc = kRecGetOCRZoneLayout(m_hPage, II_CURRENT, 
				&zoneLayout.prectSubZones, &zoneLayout.iNumSubZones, (*iterZone).userdata);

			// ensure that the zone layout was retrieved correctly
			if(rc != REC_OK)
			{
				// log an error
				UCLIDException ue("ELI16861", "Unable to retrieve zone layout.");
				loadScansoftRecErrInfo(ue, rc);
				ue.addDebugInfo("User zone number", i);
				ue.addDebugInfo("OCR zone number", (*iterZone).userdata);
				ue.log();

				// insert an empty zone layout for this zone
				zoneLayout.prectSubZones = NULL;
				zoneLayout.iNumSubZones = 0;
			}

			// add the zone layout to the stored list
			m_listZoneLayouts.push_back(zoneLayout);
		}

		// clear out all user and OCR zones
		THROW_UE_ON_ERROR("ELI16770", "Unable to clear OCR zones.", 
			kRecDeleteAllZones(m_hPage) );

		// iterate through each zone and each zone layout in the ordered list
		static list<ZoneLayoutType>::iterator iterZoneLayout;
		i=0;
		iterZoneLayout = m_listZoneLayouts.begin();
		iterEnd = m_listZones.end();
		for(iterZone = m_listZones.begin(); iterZone != iterEnd; iterZone++, iterZoneLayout++)
		{
			// insert the zone into the RecAPI engine's user zone list
			rc = kRecInsertZone(m_hPage, II_CURRENT, &(*iterZone), i);

			// ensure that the zone was inserted correctly
			if(rc != REC_OK)
			{
				// log an error
				UCLIDException ue("ELI16765", "Unable to insert zone.");
				loadScansoftRecErrInfo(ue, rc);
				ue.addDebugInfo("User zone number", i);
				ue.addDebugInfo("OCR zone number", (*iterZone).userdata);
				ue.log();

				// try the next zone
				continue;
			}

			// check if this zone has sub zones to insert
			if ((*iterZoneLayout).iNumSubZones > 0)
			{
				// set the zone layout for the corresponding zone in the user zone list
				rc = kRecSetZoneLayout(m_hPage, II_CURRENT, 
					(*iterZoneLayout).prectSubZones, (*iterZoneLayout).iNumSubZones, i);

				// ensure that the zone layout was inserted successfully
				if(rc != REC_OK)
				{
					// log an error
					UCLIDException ue("ELI16862", "Unable to set zone layout.");
					loadScansoftRecErrInfo(ue, rc);
					ue.addDebugInfo("User zone number", i);
					ue.addDebugInfo("OCR zone number", (*iterZone).userdata);
					ue.log();

					// try the next zone
					continue;
				}
			}

			// iterate to the next location in the RecAPI engine's user zone list
			i++;
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CScansoftOCR2::freeZoneLayoutMemory()
{
	static list<ZoneLayoutType>::iterator iterZoneLayout;
	static list<ZoneLayoutType>::iterator iterEnd;
	LPRECT prectSubZones;
	RECERR rc;

	// iterate through each zone layout in the list
	iterEnd = m_listZoneLayouts.end();
	for(iterZoneLayout = m_listZoneLayouts.begin(); iterZoneLayout != iterEnd; iterZoneLayout++)
	{
		// get the subzones described by this zone layout
		prectSubZones = (*iterZoneLayout).prectSubZones;

		// check if any memory was allocated for these subzones
		if (prectSubZones != __nullptr)
		{
			// free memory allocated by kRecGetOCRZoneLayout
			rc = kRecFree(prectSubZones);

			// log any errors
			if (rc != REC_OK)
			{
				UCLIDException ue("ELI16869", 
					"Application trace: Unable to release zone. Possible memory leak.");
				loadScansoftRecErrInfo(ue, rc);
				ue.log();
			}
		}
	}
}
//-------------------------------------------------------------------------------------------------
void CScansoftOCR2::setDecompositionMethodIndex(unsigned int index)
{
	ASSERT_ARGUMENT("ELI26798", index >= 0 && index < m_uiDecompositionMethods);

	// Set the appropriate page decomposition algorithm 
	THROW_UE_ON_ERROR("ELI06368", "Unable to set the page decomposition method in the OCR engine.",
		kRecSetDecompMethod(0, m_decompositionMethods[index]));
}
//-------------------------------------------------------------------------------------------------
void CScansoftOCR2::setDecompositionSequence(EPageDecompositionMethod eDecompMethod)
{
	switch (eDecompMethod)
	{
	case kAutoDecomposition:
		m_uiDecompositionMethods = 3;
		m_decompositionMethods[0] = DCM_AUTO;
		m_decompositionMethods[1] = DCM_STANDARD;
		m_decompositionMethods[2] = DCM_LEGACY;
		break;
	case kStandardDecomposition:
		m_uiDecompositionMethods = 2;
		m_decompositionMethods[0] = DCM_STANDARD;
		m_decompositionMethods[1] = DCM_LEGACY;
		break;
	case kLegacyDecomposition:
		m_uiDecompositionMethods = 2;
		m_decompositionMethods[0] = DCM_LEGACY;
		m_decompositionMethods[1] = DCM_STANDARD;
		break;
	default:
		// we should never reach here
		THROW_LOGIC_ERROR_EXCEPTION("ELI13197");
	}
}
//-------------------------------------------------------------------------------------------------
void CScansoftOCR2::setTradeOff(EOcrTradeOff eTradeOff)
{
	switch(eTradeOff)
	{
	case kAccurate:
		m_eTradeoff = TO_ACCURATE;
		break;
	case kBalanced:
		m_eTradeoff = TO_BALANCED;
		break;
	case kFast:
		m_eTradeoff = TO_FAST;
		break;
	case kRegistry:
		m_eTradeoff = m_apCfg->getTradeoff();
		break;
	default:
		// Unexpected tradeoff value
		THROW_LOGIC_ERROR_EXCEPTION("ELI22524");
	}

	// set tradeoff
	THROW_UE_ON_ERROR("ELI03412", "Unable to set tradeoff option in the OCR engine.",
		kRecSetRMTradeoff(0, m_eTradeoff));
}
//-------------------------------------------------------------------------------------------------
void CScansoftOCR2::applyCommonSettings()
{
	try
	{
		// NOTE: Turning on the CNV_NO mode makes OCR output sometimes very bad!
		// keep color/greyscale images in memory as color/greyscale images
		// NOTE: the RecAPI documentation recommends this step to improve accruacy
		//THROW_UE_ON_ERROR("ELI03413", "Unable to set image conversion options in the OCR engine!",
		//	RecSetImgConvMode(CNV_NO));

		// specify the default symbol for rejected chars
		THROW_UE_ON_ERROR("ELI03766", "Unable to set default rejection symbol in the OCR engine!",
			kRecSetRejectionSymbol(0, gwcUNRECOGNIZED));

		// specify the code page setting of the ENGine.
		THROW_UE_ON_ERROR("ELI03380", "Unable to set code page in the OCR engine!",
			kRecSetCodePage(0, "Windows ANSI"));
		
		// enable automatic image inversion mode
		THROW_UE_ON_ERROR("ELI03415", "Unable to set image inversion mode in the OCR engine!",
			kRecSetImgInvert(0, INV_AUTO));

		// enable automatic image deskewing mode
		THROW_UE_ON_ERROR("ELI03414", "Unable to set image deskewing mode in the OCR engine!",
			kRecSetImgDeskew(0, DSK_AUTO));

		// enable automatic image rotation mode
		THROW_UE_ON_ERROR("ELI03416", "Unable to set image rotation mode in the OCR engine!",
			kRecSetImgRotation(0, ROT_AUTO));

		// enable to the progress monitor for progress status updates and monitoring timeout status
		THROW_UE_ON_ERROR("ELI09029", "Unable to set the Progress Monitor Callback!",
			kRecSetCBProgMon(0, ProgressMon, NULL));

		HSETTING hSetting;
		// https://extract.atlassian.net/browse/ISSUE-12265
		// It appears up thru Nuance v18, either the LoadOriginalDPI must not have been available, 
		// must have defaulted to false, or must not have worked. Previous versions of Nuance appear
		// to have always assumed DPI of 300x300 (which matches LeadTools code that is forcing a DPI
		// of 300x300). But starting with version 19, the DPI was being set by the image file which
		// can mean it conflicts with how LeadTools loads, displays and saves these images.
		// For the time being, force to 300 DPI to maintain consistency with previous versions.
		THROW_UE_ON_ERROR("ELI37096", "Unable to get OCR setting.",
			kRecSettingGetHandle(NULL, "Kernel.Imf.PDF.LoadOriginalDPI", &hSetting, NULL) );
		THROW_UE_ON_ERROR("ELI37097", "", kRecSettingSetInt(0, hSetting, FALSE));

		THROW_UE_ON_ERROR("ELI37098", "Unable to get OCR setting.",
			kRecSettingGetHandle(NULL, "Kernel.Imf.PDF.Resolution", &hSetting, NULL) );
		THROW_UE_ON_ERROR("ELI37099", "", kRecSettingSetInt(0, hSetting, 300));

		// Extract-implemented settings that don't exist in the registry should be set to legacy values in case there were previously applied parameters
		m_bLimitToBasicLatinCharacters = true;
		m_bRequireOnePageSuccess = false;
		m_bOutputMultipleSpaceCharacterSequences = false;
		m_bOutputOneSpaceCharacterPerCount = false;
		m_bOutputTabCharactersForTabSpaceType = false;
		m_bAssignSpatialInfoToSpaceCharacters = false;
		m_bIgnoreParagraphFlag = false;
		m_bTreatZonesAsParagraphs = false;
		m_eOCRFindType = kFindStandardOCR;
		m_eDefaultFillingMethod = FM_OMNIFONT;
		m_bReturnUnrecognizedCharacters = false;
		m_bLocateZonesInSpecifiedZone = false;
		m_bIgnoreAreaOutsideSpecifiedZone = false;
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI45914")
}
//-------------------------------------------------------------------------------------------------
void CScansoftOCR2::applySettingsFromRegistry()
{
	try
	{
		// Set primary decomposition method
		m_ePrimaryDecompositionMethod = (EPageDecompositionMethod)m_apCfg->getPrimaryDecompositionMethod();
		// Set the speed-accuracy trade off
		setTradeOff(kRegistry);
		// get settings manager
		HSETTING hSetting;
		THROW_UE_ON_ERROR("ELI16766", "Unable to get OCR setting.",
			kRecSettingGetHandle(NULL, "Kernel.OcrMgr.PreferAccurateEngine", &hSetting, NULL) );
		// enable or disable third recognition pass
		m_bRunThirdRecPass = m_apCfg->getPerformThirdRecognitionPass();
		THROW_UE_ON_ERROR("ELI16767", "Unable to set third recognition pass option",
			kRecSettingSetInt(0, hSetting, m_bRunThirdRecPass) );
		// get the timeout length
		m_nTimeoutLength = m_apCfg->getTimeoutLength();
		// get whether or not to order zones
		m_bOrderZones = m_apCfg->getZoneOrdering();
		// Get whether pages should be skipped when they fail ocr.
		m_bSkipPageOnFailure = m_apCfg->getSkipPageOnFailure();
		// Get the maximum percentage of pages that can fail without failing the document
		m_uiMaxOcrPageFailurePercentage = m_apCfg->getMaxOcrPageFailurePercentage();
		// Get the maximum number of pages that can fail without failing the document
		m_uiMaxOcrPageFailureNumber = m_apCfg->getMaxOcrPageFailureNumber();

		// https://extract.atlassian.net/browse/ISSUE-12160
		// Get the despeckling options to use.
		unsigned long ulDespeckleMode = m_apCfg->getDespeckleMode();
		// Bit 0
		m_bEnableDespeckleMode = ((ulDespeckleMode & 1) == 1);
		// Bit 1
		m_eForceDespeckle = ((ulDespeckleMode & 2) == 2) ? kForceWhenBitonal : kNeverForce;
		// Bits 2-6
		m_eForceDespeckleMethod = (DESPECKLE_METHOD)((ulDespeckleMode >> 2) & 0x1F);
		// Bits 7+
		m_nForceDespeckleLevel = (long)(ulDespeckleMode >> 7);
		// Enable automatic image-despeckling mode per the low-bit of m_apCfg->getDespeckleMode().
		THROW_UE_ON_ERROR("ELI03417", "Unable to set image despeckling mode in the OCR engine!",
			kRecSetImgDespeckleMode(0, m_bEnableDespeckleMode ? TRUE : FALSE));
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI45914")
}
//-------------------------------------------------------------------------------------------------
void CScansoftOCR2::applySettingsFromParameters(IOCRParametersPtr ipOCRParameters)
{
	try
	{
		long nCount = ipOCRParameters->Size;

		if (nCount == 0)
		{
			return;
		}

		IVariantVectorPtr ipPairs = ipOCRParameters;
		ASSERT_RESOURCE_ALLOCATION("ELI46003", ipPairs != __nullptr);

		bool bFirstLanguage = true;
		bool bBarTypesSet = false;
		BAR_ENA barTypes[BAR_SIZE];
		for (int i = 0; i < BAR_SIZE; i++)
		{
			barTypes[i] = BAR_DISABLED;
		}

		for (long i = 0; i < nCount; i++)
		{	
			_variant_t vtKey, vtValue;
			HSETTING hSetting;
			IVariantPairPtr ipPair = ipPairs->Item[i];
			ipPair->GetKeyValuePair(&vtKey, &vtValue);

			if (vtKey.vt == VT_I4 && vtValue.vt == VT_I4)
			{
				long nKey = vtKey.lVal;
				long nValue = vtValue.lVal;

				switch ((EOCRParameter)nKey)
				{
				case kForceDespeckleMode:
					m_eForceDespeckle = (EForceDespeckleMode)nValue;
					break;
				case kForceDespeckleMethod:
					m_eForceDespeckleMethod = (DESPECKLE_METHOD)nValue;
					break;
				case kForceDespeckleLevel:
					m_nForceDespeckleLevel = nValue;
					break;
				case kAutoDespeckleMode:
					m_bEnableDespeckleMode = !!nValue;
					THROW_UE_ON_ERROR("ELI45921", "Unable to set image despeckling mode in the OCR engine!",
						kRecSetImgDespeckleMode(0, m_bEnableDespeckleMode ? TRUE : FALSE));
					break;
				case kZoneOrdering:
					m_bOrderZones = !!nValue;
					break;
				case kLimitToBasicLatinCharacters:
					m_bLimitToBasicLatinCharacters = !!nValue;
					break;
				case kLanguage:
					if (bFirstLanguage)
					{
						kRecManageLanguages(0, SET_LANG, (LANGUAGES)nValue);
						bFirstLanguage = false;
					}
					else
					{
						kRecManageLanguages(0, ADD_LANG, (LANGUAGES)nValue);
					}
					break;
				case kSkipPageOnFailure:
					m_bSkipPageOnFailure = !!nValue;
					break;
				case kRequireOnePageSuccess:
					m_bRequireOnePageSuccess = !!nValue;
					break;
				case kMaxPageFailureNumber:
					m_uiMaxOcrPageFailureNumber = (unsigned long)nValue;
					break;
				case kMaxPageFailurePercent:
					m_uiMaxOcrPageFailurePercentage = (unsigned long)nValue;
					break;
				case kDefaultDecompositionMethod:
					m_ePrimaryDecompositionMethod = (EPageDecompositionMethod)nValue;
					break;
				case kTradeoff:
					setTradeOff((EOcrTradeOff)nValue);
					break;
				case kDefaultFillingMethod:
					m_eDefaultFillingMethod = (FILLINGMETHOD)nValue;
					break;
				case kTimeout:
					m_nTimeoutLength = nValue;
					break;
				case kOutputMultipleSpaceCharacterSequences:
					m_bOutputMultipleSpaceCharacterSequences = !!nValue;
					break;
				case kOutputOneSpaceCharacterPerCount:
					m_bOutputOneSpaceCharacterPerCount = !!nValue;
					break;
				case kOutputTabCharactersForTabSpaceType:
					m_bOutputTabCharactersForTabSpaceType = !!nValue;
					break;
				case kAssignSpatialInfoToSpaceCharacters:
					m_bAssignSpatialInfoToSpaceCharacters = !!nValue;
					break;
				case kIgnoreParagraphFlag:
					m_bIgnoreParagraphFlag = !!nValue;
					break;
				case kTreatZonesAsParagraphs:
					m_bTreatZonesAsParagraphs = !!nValue;
					break;
				case kOCRType:
					m_eOCRFindType = (EOCRFindType)nValue;
					break;
				case kBarCodeType:
					if (nValue >= 0 && nValue < BAR_SIZE)
					{
						barTypes[nValue] = BAR_ENABLED;
						bBarTypesSet = true;
					}
					break;
				case kReturnUnrecognizedCharacters:
					m_bReturnUnrecognizedCharacters = !!nValue;
					break;
				case kLocateZonesInSpecifiedZone:
					m_bLocateZonesInSpecifiedZone = !!nValue;
					break;
				case kIgnoreAreaOutsideSpecifiedZone:
					m_bIgnoreAreaOutsideSpecifiedZone = !!nValue;
					break;
				}
			}
			// Interpret string-int/double/string pair values as RecSettings
			else if (vtKey.vt == VT_BSTR)
			{
				string strKey = asString(vtKey.bstrVal);
				THROW_UE_ON_ERROR("ELI45929", "Unable to get OCR setting.",
					kRecSettingGetHandle(NULL, strKey.c_str(), &hSetting, NULL));

				if (vtValue.vt == VT_I4)
				{
					long nValue = vtValue;
					THROW_UE_ON_ERROR("ELI45930", "Unable to set " + strKey + " option",
						kRecSettingSetInt(0, hSetting, nValue));
				}
				else if (vtValue.vt == VT_R8)
				{
					double dValue = vtValue;
					THROW_UE_ON_ERROR("ELI46038", "Unable to set " + strKey + " option",
						kRecSettingSetDouble(0, hSetting, dValue));
				}
				else if (vtValue.vt == VT_BSTR)
				{
					_bstr_t bstrValue = vtValue;
					THROW_UE_ON_ERROR("ELI46039", "Unable to set " + strKey + " option",
						kRecSettingSetString(0, hSetting, bstrValue));
				}
			}
		}


		// Setup the engine for non-standard OCR, if necessary
		if (m_eOCRFindType == kFindMICROnly)
		{
			m_eDefaultFillingMethod = FM_MICR;

			WCHAR* langPlus = L"\u2446\u2447\u2448\u2449";
			THROW_UE_ON_ERROR("ELI47229", "Unable to set languages plus of the OCR engine!",
				kRecSetLanguagesPlus(0, langPlus));

			THROW_UE_ON_ERROR("ELI47230", "Unable to set default recognition module of the OCR engine!",
				kRecSetDefaultRecognitionModule(0, RM_MAT));
		}
		else if (m_eOCRFindType == kFindBarcodesOnly)
		{
			m_eDefaultFillingMethod = FM_BARCODE;

			THROW_UE_ON_ERROR("ELI47232", "Unable to set default recognition module of the OCR engine",
				kRecSetDefaultRecognitionModule(0, RM_BAR));

			if (!bBarTypesSet)
			{
				THROW_UE_ON_ERROR("ELI47234", "Unable to get auto barcode types from the OCR engine!",
					kRecGetAutoBarTypes(0, barTypes));
			}

			THROW_UE_ON_ERROR("ELI47235", "Unable to set barcode types of the OCR engine!",
				kRecSetBarTypes(0, barTypes));
		}

		THROW_UE_ON_ERROR("ELI47231", "Unable to set default filling method in the OCR engine!",
					kRecSetDefaultFillingMethod(0, m_eDefaultFillingMethod));
	}
	CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI45922")
}
//-------------------------------------------------------------------------------------------------
bool CScansoftOCR2::zoneIsLessThan(ZONE zoneLeft, ZONE zoneRight)
{
	return zoneLeft.rectBBox.top < zoneRight.rectBBox.top ||
		(zoneLeft.rectBBox.top == zoneRight.rectBBox.top &&
		zoneLeft.rectBBox.left < zoneRight.rectBBox.left);
}
//-------------------------------------------------------------------------------------------------
bool CScansoftOCR2::isBasicLatinCharacter(unsigned short usLetterCode)
{
	// NOTE: 176 is the degree symbol.
	// Don't allow 0 as a letter code
	// https://extract.atlassian.net/browse/ISSUE-14802
	return usLetterCode > 0 && usLetterCode <= 126 || usLetterCode == 176;
}
//-------------------------------------------------------------------------------------------------
bool CScansoftOCR2::isMICRCharacter(unsigned short usLetterCode)
{
	return (usLetterCode >= 0x2446 && usLetterCode <= 0x2449) || usLetterCode == 0xFFFD;
}
//-------------------------------------------------------------------------------------------------
bool CScansoftOCR2::isRecognizedCharacter(unsigned short usLetterCode)
{
	return usLetterCode > 0 && usLetterCode != 0xFFFD
		&& (!m_bLimitToBasicLatinCharacters
			|| isBasicLatinCharacter(usLetterCode)
			|| (m_eOCRFindType == kFindMICROnly) && isMICRCharacter(usLetterCode)
			);
}
//-------------------------------------------------------------------------------------------------
void CScansoftOCR2::convertToCodePage(unsigned short *usLetterCode)
{
	if (m_eOCRFindType == kFindMICROnly)
	{
		switch (*usLetterCode)
		{
		case 0x2446:
			*usLetterCode = 'T';
			break;

		case 0x2447:
			*usLetterCode = 'A';
			break;

		case 0x2448:
			*usLetterCode = 'U';
			break;

		case 0x2449:
			*usLetterCode = 'D';
			break;
		}
	}

	if (isBasicLatinCharacter(*usLetterCode))
	{
		return;
	}

	size_t buffLen;
	BYTE buff;
	buffLen = sizeof(buff);
	RECERR ret = kRecConvertUnicode2CodePage(0, *usLetterCode, (LPBYTE)&buff, &buffLen);
	if (ret != REC_OK)
	{
		*usLetterCode = gcUNRECOGNIZED;
	}
	else
	{
		*usLetterCode = buff;
	}
}
//-------------------------------------------------------------------------------------------------
void CScansoftOCR2::readFileDataToVariant(const string& strFileName, VARIANT* pFileData)
{
	void* pArrayData = __nullptr;
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
			ifs.read((char*)pArrayData, length);
			if (ifs.gcount() != length)
			{
				UCLIDException ue("ELI49603", "Image read failed");
				ue.addDebugInfo("Read bytes", ifs.gcount());
				ue.addDebugInfo("Expected bytes", length);
			}
			SafeArrayUnaccessData(pFileData->parray);
			ifs.close();
		}
		CATCH_ALL_AND_RETHROW_AS_UCLID_EXCEPTION("ELI49604");
	}
	catch (UCLIDException & ue)
	{
		if (pArrayData != __nullptr)
		{
			SafeArrayUnaccessData(pFileData->parray);
		}
		ifs.close();

		throw ue;
	}
}

//-------------------------------------------------------------------------------------------------
// RecMemoryReleaser
//-------------------------------------------------------------------------------------------------
template<typename MemoryType>
CScansoftOCR2::RecMemoryReleaser<MemoryType>::RecMemoryReleaser(MemoryType* pMemoryType)
 : m_pMemoryType(pMemoryType)
{

}
//-------------------------------------------------------------------------------------------------
template<>
CScansoftOCR2::RecMemoryReleaser<tagIMGFILEHANDLE>::~RecMemoryReleaser()
{
	try
	{
		RECERR rc = kRecCloseImgFile(m_pMemoryType);

		// log any errors
		if (rc != REC_OK)
		{
			UCLIDException ue("ELI16944", 
				"Application trace: Unable to close image file. Possible memory leak.");
			loadScansoftRecErrInfo(ue, rc);
			ue.log();
		}

		// Delete files in temp dir when cleaning up an image so that they don't accumulate
		// https://extract.atlassian.net/browse/ISSUE-16868
		string tempDataDir = getTemporaryDataFolder(GetCurrentProcessId());
		vector<string> vecSubDirs;
		getAllSubDirsAndDeleteAllFiles(tempDataDir, vecSubDirs);
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16954");
}
//-------------------------------------------------------------------------------------------------
template<>
CScansoftOCR2::RecMemoryReleaser<RECPAGESTRUCT>::~RecMemoryReleaser()
{
	try
	{
		RECERR rc = kRecFreeRecognitionData(m_pMemoryType);

		// log any errors
		if (rc != REC_OK)
		{
			UCLIDException ue("ELI16942", 
				"Application trace: Unable to release recognition data. Possible memory leak.");
			loadScansoftRecErrInfo(ue, rc);
			ue.log();
		}

		rc = kRecFreeImg(m_pMemoryType);

		// log any errors
		if (rc != REC_OK)
		{
			UCLIDException ue("ELI16943", 
				"Application trace: Unable to release page image. Possible memory leak.");
			loadScansoftRecErrInfo(ue, rc);
			ue.log();
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16955");
}
//-------------------------------------------------------------------------------------------------
template<>
CScansoftOCR2::RecMemoryReleaser<LETTER>::~RecMemoryReleaser()
{
	try
	{
		RECERR rc = kRecFree(m_pMemoryType);

		// log any errors
		if (rc != REC_OK)
		{
			UCLIDException ue("ELI16941", 
				"Application trace: Unable to release letter data. Possible memory leak.");
			loadScansoftRecErrInfo(ue, rc);
			ue.log();
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16956");
}
//-------------------------------------------------------------------------------------------------
template<>
CScansoftOCR2::RecMemoryReleaser<CScansoftOCR2>::~RecMemoryReleaser()
{
	try
	{
		m_pMemoryType->freeZoneLayoutMemory();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16957");
}
//-------------------------------------------------------------------------------------------------
template<>
CScansoftOCR2::RecMemoryReleaser<Win32Event>::~RecMemoryReleaser()
{
	try
	{
		m_pMemoryType->signal();
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI19906");
}
//-------------------------------------------------------------------------------------------------
template<>
CScansoftOCR2::RecMemoryReleaser<RECDOCSTRUCT>::~RecMemoryReleaser()
{
	try
	{
		RECERR rc = RecCloseDoc(0, m_pMemoryType);

		// log any errors
		if (rc != REC_OK)
		{
			UCLIDException ue("ELI49533", 
				"Application trace: Unable to close document. Possible memory leak.");
			loadScansoftRecErrInfo(ue, rc);
			ue.log();
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI49534");
}