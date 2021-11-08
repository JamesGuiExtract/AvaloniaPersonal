// ImageUtils.h : Declaration of the CImageUtils

#pragma once

#include "resource.h"       // main symbols

#include <MiscLeadUtils.h>

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
};