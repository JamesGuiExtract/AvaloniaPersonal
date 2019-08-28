// ImageConverter.h : Declaration of the CImageConverter

#pragma once

#include "UCLIDImageUtils.h"
#include "resource.h"       // main symbols

#include <string>
using namespace std;

// Releases memory allocated by RecAPI (Nuance) calls. Create this object after the RecAPI call has
// allocated space for the object. MemoryType is the data type of the object to release when 
// RecMemoryReleaser goes out of scope.
template<typename MemoryType>
class RecMemoryReleaser
{
public:
	RecMemoryReleaser(MemoryType* pMemoryType);
	~RecMemoryReleaser();

private:
	MemoryType* m_pMemoryType;
};

/////////////////////////////////////////////////////////////////////////////
// CImageConverter
class ATL_NO_VTABLE CImageConverter :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CImageConverter, &CLSID_ImageConverter>,
	public ISupportErrorInfo,
	public IDispatchImpl<IImageConverter, &IID_IImageConverter, &LIBID_UCLID_IMAGEUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CImageConverter();
	~CImageConverter();

DECLARE_REGISTRY_RESOURCEID(IDR_IMAGECONVERTER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CImageConverter)
	COM_INTERFACE_ENTRY(IImageConverter)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IImageConverter)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IImageConverter
public:
	STDMETHOD(GetPDFImage)(BSTR bstrFileName, int nPage, VARIANT_BOOL vbUseSeparateProcess, VARIANT *pImageData);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);
	
private:

	string m_strImageFormatConverterEXE;

	////////////////////
	// Methods
	////////////////////
	void validateLicense();
	//---------------------------------------------------------------------------------------------
	void initNuanceEngineAndLicense();
	//---------------------------------------------------------------------------------------------
	void convertPageToPDF(const string& strInputFileName, int nPage, VARIANT *pImageData);
	//---------------------------------------------------------------------------------------------
	void convertPageToPdfWithSeparateProcess(const string& strInputFileName, int nPage, VARIANT *pImageData);
	//---------------------------------------------------------------------------------------------
	void readFileDataToVariant(const string& strFileName, VARIANT *pFileData);
};

OBJECT_ENTRY_AUTO(__uuidof(ImageConverter), CImageConverter)