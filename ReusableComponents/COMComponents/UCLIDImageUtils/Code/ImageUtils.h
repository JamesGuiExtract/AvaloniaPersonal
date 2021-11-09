// ImageUtils.h : Declaration of the CImageUtils

#pragma once

#include "resource.h"       // main symbols

#include <MiscLeadUtils.h>
#include <LeadToolsBitmap.h>

#include <string>
#include <vector>

/////////////////////////////////////////////////////////////////////////////
// CImageUtils
class ATL_NO_VTABLE CImageUtils : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CImageUtils, &CLSID_ImageUtils>,
	public ISupportErrorInfo,
	public IDispatchImpl<IImageUtils, &IID_IImageUtils, &LIBID_UCLID_IMAGEUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CImageUtils();
	~CImageUtils();

DECLARE_REGISTRY_RESOURCEID(IDR_IMAGEUTILS)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CImageUtils)
	COM_INTERFACE_ENTRY(IImageUtils)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IImageUtils)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IImageUtils
public:
	STDMETHOD(CreateMultiPageImage)(IVariantVector *pvecSinglePageImages, 
		BSTR strOutputImageFileName);
	STDMETHOD(IsMultiPageImage)( BSTR strImageFileName, VARIANT_BOOL *pResult );
	STDMETHOD(GetPageCount)(BSTR bstrImageFileName, long* pnPageCount);
	STDMETHOD(GetImagePageNumbers)(BSTR strImageFileName, BSTR strSpecificPages, 
		IVariantVector **pvecPageNumbers);
	STDMETHOD(IsTextInZone)( IImageStats *pImageStats, long nConsecutiveRows, long nMinPercent, 
		long nMaxPercent, VARIANT_BOOL *pResult);
	STDMETHOD(GetImageStats)(BSTR strImage, IRasterZone * pRaster, IImageStats ** ppImageStats);
	STDMETHOD(GetSpatialPageInfos)(BSTR bstrFileName, IIUnknownVector **pvecSpatialPageInfos);
	STDMETHOD(GetSpatialPageInfo)(BSTR fileName, long pageNumber, ISpatialPageInfo **spatialPageInfo);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);
	
private:

	void validateLicense();

	// Keep an instance of the last page that was loaded to speed-up subsequent calls that use the same page
	// https://extract.atlassian.net/browse/ISSUE-17794
	struct CachedImagePage {
		std::string fileName;
		long pageNumber;
		std::unique_ptr<LeadToolsBitmap> bitmap;
	} m_cachedBitmap;

	// Keep an instance of the last stats that were loaded to speed-up subsequent calls for the same
	// area of the same page
	// https://extract.atlassian.net/browse/ISSUE-17794
	struct CachedImageStats {
		std::string fileName;
		IRasterZonePtr rasterZone;
		UCLID_IMAGEUTILSLib::IImageStatsPtr imageStats;
	} m_cachedImageStats;

	// Keep track of the last time the image was written to validate cached data
	CTime m_cachedImageLastWriteTime;

	// Load and cache a page from an image file
	BITMAPHANDLE* loadImagePage(const std::string& imageFilename, long pageNumber);

	// Check that the filename, page number and last write time match what was cached
	bool isCachedDataValid(const std::string& imageFilename, long pageNumber);
};