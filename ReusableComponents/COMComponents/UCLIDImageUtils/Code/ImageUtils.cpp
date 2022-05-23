// ImageUtils.cpp : Implementation of CImageUtils
#include "stdafx.h"
#include "UCLIDImageUtils.h"
#include "ImageUtils.h"

#include <UCLIDException.h>
#include <LicenseMgmt.h>
#include <misc.h>
#include <cpputil.h>
#include <ComUtils.h>
#include <MiscLeadUtils.h>
#include <ExtractZoneAsImage.h>
#include <LeadToolsBitmap.h>
#include <LeadToolsBitmapFreeer.h>
#include <ComponentLicenseIDs.h>
#include <LeadToolsLicenseRestrictor.h>
#include <ExtractMFCUtils.h>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const COLORREF gcrDefaultFGColor = RGB(0,0,0);

//-------------------------------------------------------------------------------------------------
// CImageUtils
//-------------------------------------------------------------------------------------------------
CImageUtils::CImageUtils()
	: m_cachedBitmap()
	, m_cachedImageStats()
	, m_cachedImageLastWriteTime(0)
{
	try
	{
		// If PDF support is licensed initialize support
		// NOTE: no exception is thrown or logged if PDF support is not licensed.
		initPDFSupport();
	}
	CATCH_DISPLAY_AND_RETHROW_ALL_EXCEPTIONS("ELI19864");
}
//-------------------------------------------------------------------------------------------------
CImageUtils::~CImageUtils()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI19101");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageUtils::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IImageUtils,
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
// IImageUtils
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageUtils::CreateMultiPageImage(IVariantVector *pvecSinglePageImages, 
											   BSTR strOutputImageFileName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		IVariantVectorPtr ipvecImages( pvecSinglePageImages );
		ASSERT_RESOURCE_ALLOCATION("ELI09060", ipvecImages != __nullptr );

		// Copy Strings to vecImages;
		vector<string> vecImages;
		int nNumImages = ipvecImages->Size;
		for (int i = 0; i < nNumImages; i++ )
		{
			// Load the vector with the SinglePageImage names
			string strPageFileName = asString(_bstr_t(ipvecImages->GetItem(i)));
			// Make sure that if the file being opened is a pdf file that PDF support is licensed
			LicenseManagement::verifyFileTypeLicensedRO(  strPageFileName );
			vecImages.push_back( strPageFileName );
		}
		string strOutputFileName = asString(strOutputImageFileName);
		// Make sure that if the file being saved is a pdf file that PDF support is licensed
		LicenseManagement::verifyFileTypeLicensedRW(  strOutputFileName );
		// Create the Multipage image file
		createMultiPageImage(vecImages, strOutputFileName, false);
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09059")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageUtils::IsMultiPageImage(BSTR strImageFileName, VARIANT_BOOL* pResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI20473", pResult != __nullptr);

		validateLicense();

		string strImage = asString(strImageFileName);

		// Get number of pages in image file
		int nPages = getNumberOfPagesInImage(strImage);

		// if number of pages is > 1 return true
		*pResult = asVariantBool(nPages > 1);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI48428")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageUtils::GetPageCount(BSTR bstrImageFileName, long* pnPageCount)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI48340", pnPageCount != __nullptr);

		validateLicense();

		string strImageFileName = asString(bstrImageFileName);

		*pnPageCount = getNumberOfPagesInImage(strImageFileName);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI48341")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageUtils::GetImagePageNumbers(BSTR strImageFileName,
											  BSTR strSpecificPages, 
											  IVariantVector **pvecPageNumbers)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// validate the specific page string
		string strDefinedPages = asString(strSpecificPages);
		string strImage = asString(strImageFileName);
		
		// Make sure that if the file being opened is a pdf file that PDF support is licensed
		LicenseManagement::verifyFileTypeLicensedRO( strImage );

		// get total number of pages of this image
		int nTotalNumberOfPages = ::getNumberOfPagesInImage(strImage);

		// get a vector of specified page numbers
		vector<int> vecPageNumbers = ::getPageNumbers(nTotalNumberOfPages, strDefinedPages);

		IVariantVectorPtr ipVecPageNumbers(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI10269", ipVecPageNumbers != __nullptr);

		int nSize = vecPageNumbers.size();
		for (int n=0; n<nSize; n++)
		{
			_variant_t _vPageNumber((long)vecPageNumbers[n]);
			ipVecPageNumbers->PushBack(_vPageNumber);
		}

		*pvecPageNumbers = ipVecPageNumbers.Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10265")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageUtils::IsTextInZone( IImageStats *pImageStats, long nConsecutiveRows, 
									   long nMinPercent, long nMaxPercent, VARIANT_BOOL *pResult)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		// Setup result to be false;
		*pResult = VARIANT_FALSE;
		UCLID_IMAGEUTILSLib::IImageStatsPtr ipImageStats(pImageStats);
		ASSERT_RESOURCE_ALLOCATION("ELI13300", ipImageStats != __nullptr );

		long nCurrConsecutiveRows = 0;
		IVariantVectorPtr ipPixelCounts = ipImageStats->FGPixelsInRow;
		ASSERT_RESOURCE_ALLOCATION("ELI13301", ipPixelCounts != __nullptr );

		long nWidth = ipImageStats->Width;

		for ( long iRow = 0; iRow < ipPixelCounts->Size /*bmZoneBitmap.Height*/; iRow++ )
		{
			long nPixelsFoundInRow = ipPixelCounts->GetItem(iRow);
			int nPercent = 100 * nPixelsFoundInRow/nWidth /*bmZoneBitmap.Width*/;
			if ( nPercent >= nMinPercent && nPercent <= nMaxPercent )
			{
				nCurrConsecutiveRows++;
				if ( nCurrConsecutiveRows >= nConsecutiveRows )
				{
					*pResult = VARIANT_TRUE;
					return S_OK;
				}
			}
			else
			{
				nCurrConsecutiveRows = 0;
			}
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13286");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageUtils::GetImageStats(BSTR strImage, IRasterZone * pRaster, 
										IImageStats ** ppImageStats)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI27291", ppImageStats != __nullptr);

		UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipZone(pRaster);
		ASSERT_RESOURCE_ALLOCATION("ELI13285", ipZone != __nullptr );

		string strImageName = asString(strImage);

		// Used cached result if the image and zone are the same as the last time this method was called
		if (isCachedDataValid(strImageName, ipZone->PageNumber)
			&& ipZone->Equals(m_cachedImageStats.rasterZone))
		{
			UCLID_IMAGEUTILSLib::IImageStatsPtr ipCopy = m_cachedImageStats.imageStats;
			*ppImageStats = (IImageStats*) ipCopy.Detach();
		
			return S_OK;
		}
		
		// Make sure that if the file being opened is a pdf file that PDF support is licensed
		LicenseManagement::verifyFileTypeLicensedRO( strImageName );

		// Load the page into bitmap
		BITMAPHANDLE* pPageBitmap = loadImagePage(strImageName, pRaster->PageNumber);

		BITMAPHANDLE bmZoneBitmap;
		LeadToolsBitmapFreeer freeerZone( bmZoneBitmap, true );
		extractZoneAsBitmap(pPageBitmap, pRaster->StartX, pRaster->StartY, 
			pRaster->EndX, pRaster->EndY, pRaster->Height, &bmZoneBitmap );

		// Convert to bitonal
		if (bmZoneBitmap.BitsPerPixel != 1)
		{
			convertImageColorDepth(bmZoneBitmap, strImageName, 1, false, true);
		}

		UCLID_IMAGEUTILSLib::IImageStatsPtr ipImageStats(CLSID_ImageStats);
		ASSERT_RESOURCE_ALLOCATION("ELI13299", ipImageStats != __nullptr );

		IVariantVectorPtr ipVecFGPixelsInRow(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI13298", ipVecFGPixelsInRow != __nullptr );

		ipImageStats->Height = bmZoneBitmap.Height;
		ipImageStats->Width = bmZoneBitmap.Width;
		// Set foreground color to default
		ipImageStats->FGColor = gcrDefaultFGColor;
		ipImageStats->FGPixelsInRow = ipVecFGPixelsInRow;

		long nCurrConsecutiveRows = 0;

		LeadToolsLicenseRestrictor leadToolsLicenseGuard;
		for ( long iRow = 0; iRow < bmZoneBitmap.Height; iRow++ )
		{
			long nPixelsFoundInRow = 0;
			for ( long iCol = 0; iCol < bmZoneBitmap.Width; iCol++ )
			{
				COLORREF crColor = getPixelColor( bmZoneBitmap, iRow, iCol );
				// Check if pixel is black
				if ( crColor == gcrDefaultFGColor )
				{
					nPixelsFoundInRow++;
				}
			}
			ipVecFGPixelsInRow->PushBack(nPixelsFoundInRow);
		}

		// Cache result for subsequent calls
		m_cachedImageStats = {
			strImageName,
			ipZone,
			ipImageStats
		};

		*ppImageStats = (IImageStats*) ipImageStats.Detach();
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13297");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageUtils::GetSpatialPageInfos(BSTR bstrFileName, IIUnknownVector **pvecSpatialPageInfos)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI45167", pvecSpatialPageInfos != __nullptr);

		string strFileName = asString(bstrFileName);
		string strUssFileName = strFileName + ".uss";
		ILongToObjectMapPtr ipPageInfos(CLSID_LongToObjectMap);
		if (isFileOrFolderValid(strUssFileName))
		{
			ISpatialStringPtr ipUssData(CLSID_SpatialString);
			ASSERT_RESOURCE_ALLOCATION("ELI45168", ipUssData != __nullptr);

			IIUnknownVectorPtr ipPages = ipUssData->LoadPagesFromFile(strUssFileName.c_str());
			for (long i = 0, len = ipPages->Size(); i < len; i++)
			{
				ipUssData = ipPages->At(i);
				long pageNumber = i + 1;
				if (ipUssData->SpatialPageInfos->Contains(pageNumber))
				{
					ipPageInfos->Set(pageNumber, ipUssData->SpatialPageInfos->GetValue(pageNumber));
				}
			}
		}

		IIUnknownVectorPtr ipSpatialPageInfos(CLSID_IUnknownVector);
		ASSERT_RESOURCE_ALLOCATION("ELI45169", ipSpatialPageInfos != __nullptr);

		int nPages = getNumberOfPagesInImage(strFileName);
		for (int nPage = 1; nPage <= nPages; nPage++)
		{
			ISpatialPageInfoPtr ipSpatialPageInfo = __nullptr;

			// Get page info from uss file if possible
			if (ipPageInfos != __nullptr && ipPageInfos->Contains(nPage))
			{
				ipSpatialPageInfo = ipPageInfos->GetValue(nPage);
			}
			// Otherwise read it from the image itself (will not include rotation)
			if (ipSpatialPageInfo == __nullptr)
			{
				ipSpatialPageInfo.CreateInstance(CLSID_SpatialPageInfo);
				ASSERT_RESOURCE_ALLOCATION("ELI45170", ipSpatialPageInfo != __nullptr);

				int nWidth = 0;
				int nHeight = 0;
				getImagePixelHeightAndWidth(strFileName, nHeight, nWidth, nPage);

				ipSpatialPageInfo->Initialize(nWidth, nHeight, kRotNone, 0);
			}

			ipSpatialPageInfos->PushBack(ipSpatialPageInfo);
		}

		*pvecSpatialPageInfos = ipSpatialPageInfos.Detach();

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI45171");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageUtils::GetSpatialPageInfo(BSTR fileName, long pageNumber, ISpatialPageInfo **spatialPageInfo)
{
	AFX_MANAGE_STATE(AfxGetAppModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI49588", spatialPageInfo != __nullptr);

		string strFileName = asString(fileName);
		string strUssFileName = strFileName + ".uss";
		if (isFileOrFolderValid(strUssFileName))
		{
			ISpatialStringPtr ipUssData(CLSID_SpatialString);
			ASSERT_RESOURCE_ALLOCATION("ELI49589", ipUssData != __nullptr);

			ipUssData->LoadPageFromFile(strUssFileName.c_str(), pageNumber);
			if (ipUssData->HasSpatialInfo() && ipUssData->SpatialPageInfos->Contains(pageNumber))
			{
				ISpatialPageInfoPtr ipSpatialPageInfo = ipUssData->SpatialPageInfos->GetValue(pageNumber);
				*spatialPageInfo = ipSpatialPageInfo.Detach();
				return S_OK;
			}
		}

		ISpatialPageInfoPtr ipSpatialPageInfo(CLSID_SpatialPageInfo);
		ASSERT_RESOURCE_ALLOCATION("ELI49590", ipSpatialPageInfo != __nullptr);

		int nWidth = 0;
		int nHeight = 0;
		getImagePixelHeightAndWidth(strFileName, nHeight, nWidth, pageNumber);

		ipSpatialPageInfo->Initialize(nWidth, nHeight, kRotNone, 0);

		*spatialPageInfo = ipSpatialPageInfo.Detach();
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI49591");
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageUtils::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18693", pbValue != __nullptr);

		try
		{
			validateLicense();
			// if validateLicense doesn't throw any exception, then pbValue is true
			*pbValue = VARIANT_TRUE;
		}
		catch(...)
		{
			*pbValue = VARIANT_FALSE;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18694");
}

//-------------------------------------------------------------------------------------------------
// Private Methods
//-------------------------------------------------------------------------------------------------
void CImageUtils::validateLicense()
{
	static const unsigned long IMAGE_UTILS_ID = gnEXTRACT_CORE_OBJECTS;

	VALIDATE_LICENSE( IMAGE_UTILS_ID, "ELI09061", "Image Utils" );
}
//-------------------------------------------------------------------------------------------------
bool CImageUtils::isCachedDataValid(const std::string& imageFilename, long pageNumber)
{
	return m_cachedBitmap.fileName == imageFilename
		&& m_cachedBitmap.pageNumber == pageNumber
		&& m_cachedImageLastWriteTime == getFileModificationTimeStamp(imageFilename);
}
//-------------------------------------------------------------------------------------------------
BITMAPHANDLE* CImageUtils::loadImagePage(const std::string& imageFilename, long pageNumber)
{
	if (isCachedDataValid(imageFilename, pageNumber))
	{
		return &(m_cachedBitmap.bitmap.get())->m_hBitmap;
	}

	m_cachedBitmap = {
		imageFilename,
		pageNumber,
		std::make_unique<LeadToolsBitmap>(imageFilename, pageNumber, 0, -1, false, true)
	};

	LeadToolsBitmap* bmp = m_cachedBitmap.bitmap.get();
	ASSERT_RESOURCE_ALLOCATION("ELI44665", bmp != __nullptr);

	m_cachedImageLastWriteTime = getFileModificationTimeStamp(imageFilename);

	return &bmp->m_hBitmap;
}
//-------------------------------------------------------------------------------------------------
