// ImageStats.h : Declaration of the CImageStats

#pragma once
#include "resource.h"       // main symbols

#include "UCLIDImageUtils.h"


#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif



// CImageStats

class ATL_NO_VTABLE CImageStats :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CImageStats, &CLSID_ImageStats>,
	public ISupportErrorInfo,
	public IDispatchImpl<IImageStats, &IID_IImageStats, &LIBID_UCLID_IMAGEUTILSLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
public:
	CImageStats();

DECLARE_REGISTRY_RESOURCEID(IDR_IMAGESTATS)


BEGIN_COM_MAP(CImageStats)
	COM_INTERFACE_ENTRY(IImageStats)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
END_COM_MAP()



	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
	}

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IImageStats
	STDMETHOD(get_FGPixelsInRow)(IVariantVector ** pVal);
	STDMETHOD(put_FGPixelsInRow)(IVariantVector * newVal);
	STDMETHOD(get_FGColor)(COLORREF* pVal);
	STDMETHOD(put_FGColor)(COLORREF newVal);
	STDMETHOD(get_Width)(long* pVal);
	STDMETHOD(put_Width)(long newVal);
	STDMETHOD(get_Height)(long* pVal);
	STDMETHOD(put_Height)(long newVal);

private:
	// Variables;
	
	// width of the image in pixels
	long m_lWidth;
	
	// height of the image in pixels
	long m_lHeight;

	// Color used as the Foreground color
	COLORREF m_crFGColor;

	// vector containing the number of foreground pixels per row in the image
	IVariantVectorPtr m_ipVecFGPixelsInRow;

};

OBJECT_ENTRY_AUTO(__uuidof(ImageStats), CImageStats)
