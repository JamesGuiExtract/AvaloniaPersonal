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
#include <LeadToolsBitmapFreeer.h>
#include <ComponentLicenseIDs.h>

using namespace std;

//-------------------------------------------------------------------------------------------------
// Constants
//-------------------------------------------------------------------------------------------------
const COLORREF gcrDefaultFGColor = RGB(0,0,0);

//-------------------------------------------------------------------------------------------------
// CImageUtils
//-------------------------------------------------------------------------------------------------
CImageUtils::CImageUtils()
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
		ASSERT_RESOURCE_ALLOCATION("ELI09060", ipvecImages != NULL );

		// Copy Strings to vecImages;
		vector<string> vecImages;
		int nNumImages = ipvecImages->Size;
		for (int i = 0; i < nNumImages; i++ )
		{
			// Load the vector with the SinglePageImage names
			string strPageFileName = asString(_bstr_t(ipvecImages->GetItem(i)));
			// Make sure that if the file being opened is a pdf file that PDF support is licensed
			LicenseManagement::sGetInstance().verifyFileTypeLicensed(  strPageFileName );
			vecImages.push_back( strPageFileName );
		}
		string strOutputFileName = asString(strOutputImageFileName);
		// Make sure that if the file being saved is a pdf file that PDF support is licensed
		LicenseManagement::sGetInstance().verifyFileTypeLicensed(  strOutputFileName );
		// Create the Multipage image file
		createMultiPageImage(vecImages, strOutputFileName, false);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09059")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageUtils::IsMultiPageImage( BSTR strImageFileName, VARIANT_BOOL *pResult )
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI20473", pResult != NULL);

		validateLicense();

		string strImage = asString(strImageFileName);
		
		// Make sure that if the file being opened is a pdf file that PDF support is licensed
		LicenseManagement::sGetInstance().verifyFileTypeLicensed( strImage );
		
		// Get number of pages in image file
		int nPages = getNumberOfPagesInImage( strImage );
		
		// if number of pages is > 1 return true
		*pResult = asVariantBool(nPages > 1 ) ;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI09167")

	return S_OK;
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
		LicenseManagement::sGetInstance().verifyFileTypeLicensed( strImage );

		// get total number of pages of this image
		int nTotalNumberOfPages = ::getNumberOfPagesInImage(strImage);

		// get a vector of specified page numbers
		vector<int> vecPageNumbers = ::getPageNumbers(nTotalNumberOfPages, strDefinedPages);

		IVariantVectorPtr ipVecPageNumbers(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI10269", ipVecPageNumbers != NULL);

		int nSize = vecPageNumbers.size();
		for (int n=0; n<nSize; n++)
		{
			_variant_t _vPageNumber((long)vecPageNumbers[n]);
			ipVecPageNumbers->PushBack(_vPageNumber);
		}

		*pvecPageNumbers = ipVecPageNumbers.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI10265")

	return S_OK;
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
		ASSERT_RESOURCE_ALLOCATION("ELI13300", ipImageStats != NULL );

		long nCurrConsecutiveRows = 0;
		IVariantVectorPtr ipPixelCounts = ipImageStats->FGPixelsInRow;
		ASSERT_RESOURCE_ALLOCATION("ELI13301", ipPixelCounts != NULL );

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
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13286");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageUtils::GetImageStats(BSTR strImage, IRasterZone * pRaster, 
										IImageStats ** ppImageStats)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		validateLicense();

		ASSERT_ARGUMENT("ELI27291", ppImageStats != NULL);

		BITMAPHANDLE bmPageBitmap;
		LeadToolsBitmapFreeer freeerPage( bmPageBitmap, true );

		UCLID_RASTERANDOCRMGMTLib::IRasterZonePtr ipZone(pRaster);
		ASSERT_RESOURCE_ALLOCATION("ELI13285", ipZone != NULL );

		string strImageName = asString(strImage);
		
		// Make sure that if the file being opened is a pdf file that PDF support is licensed
		LicenseManagement::sGetInstance().verifyFileTypeLicensed( strImageName );

		// Load the page into bitmap
		loadImagePage( strImageName, pRaster->PageNumber, bmPageBitmap );

		// Rotate this bitmap if needed
		handleRotatedImage( strImageName, pRaster->PageNumber, &bmPageBitmap );

		BITMAPHANDLE bmZoneBitmap;
		LeadToolsBitmapFreeer freeerZone( bmZoneBitmap, true );
		extractZoneAsBitmap( &bmPageBitmap, pRaster->StartX, pRaster->StartY, 
			pRaster->EndX, pRaster->EndY, pRaster->Height, &bmZoneBitmap );

		UCLID_IMAGEUTILSLib::IImageStatsPtr ipImageStats(CLSID_ImageStats);
		ASSERT_RESOURCE_ALLOCATION("ELI13299", ipImageStats != NULL );

		IVariantVectorPtr ipVecFGPixelsInRow(CLSID_VariantVector);
		ASSERT_RESOURCE_ALLOCATION("ELI13298", ipVecFGPixelsInRow != NULL );

		ipImageStats->Height = bmZoneBitmap.Height;
		ipImageStats->Width = bmZoneBitmap.Width;
		// Set foreground color to default
		ipImageStats->FGColor = gcrDefaultFGColor;
		ipImageStats->FGPixelsInRow = ipVecFGPixelsInRow;

		long nCurrConsecutiveRows = 0;
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

		*ppImageStats = (IImageStats*) ipImageStats.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13297");

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ILicensedComponent
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CImageUtils::raw_IsLicensed(VARIANT_BOOL * pbValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI18693", pbValue != NULL);

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
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI18694");

	return S_OK;
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
void CImageUtils::handleRotatedImage(const std::string& strImageFileName, int nPageNumber, 
									 BITMAPHANDLE* phBitmap)
{
	// Get original view perspective of this page
	int nOriginalVP = getImageViewPerspective( strImageFileName, nPageNumber );

	// Determine required rotation angle
	int nRotationAngle = 0;
	switch (nOriginalVP)
	{
	case 0:
	case 1:
	case 4:
		// No change is needed
		break;

	case 7:
	case 8:
		nRotationAngle = 90;
		break;

	case 2:
	case 3:
		nRotationAngle = 180;
		break;

	case 5:
	case 6:
		nRotationAngle = 270;
		break;

	default:
		THROW_LOGIC_ERROR_EXCEPTION("ELI16812");
	}

	// Is rotation needed?
	if (nRotationAngle != 0)
	{
		// Verify that Document support is licensed
		unlockDocumentSupport();

		// Rotate the loaded bitmap, resizing as needed and filling new pixels with WHITE
		int nRet = L_RotateBitmap( phBitmap, nRotationAngle*100, ROTATE_RESIZE, 
			RGB(255,255,255) );
		throwExceptionIfNotSuccess( nRet, "ELI16810", "Could not rotate the bitmap.");
	}
}
//-------------------------------------------------------------------------------------------------
