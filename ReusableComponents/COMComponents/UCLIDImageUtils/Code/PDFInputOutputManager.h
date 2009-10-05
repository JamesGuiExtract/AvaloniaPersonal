// PDFInputOutputManager.h : Declaration of the CPDFInputOutputManager

#pragma once
#include "resource.h"       // main symbols

#include "UCLIDImageUtils.h"

#include <PDFInputOutputMgr.h>

#include <memory>

using namespace std;

/////////////////////////////////////////////////////////////////////////////
// CPDFInputOutputManager
/////////////////////////////////////////////////////////////////////////////
class ATL_NO_VTABLE CPDFInputOutputManager :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CPDFInputOutputManager, &CLSID_PDFInputOutputManager>,
	public ISupportErrorInfo,
	public IDispatchImpl<IPDFInputOutputManager, &IID_IPDFInputOutputManager, &LIBID_UCLID_IMAGEUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CPDFInputOutputManager();
	~CPDFInputOutputManager();

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct();
	void FinalRelease();

DECLARE_REGISTRY_RESOURCEID(IDR_PDFINPUTOUTPUTMANAGER)

BEGIN_COM_MAP(CPDFInputOutputManager)
	COM_INTERFACE_ENTRY(IPDFInputOutputManager)
	COM_INTERFACE_ENTRY2(IDispatch, IPDFInputOutputManager)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

// IPDFInputOutputManager
	STDMETHOD(SetFileData)(BSTR bstrOriginalFileName, VARIANT_BOOL bFileUsedAsInput);
	STDMETHOD(IsInputFile)(VARIANT_BOOL* pbIsInputFile);
	STDMETHOD(get_FileName)(BSTR* pbstrFileName);
	STDMETHOD(get_FileNameInformationString)(BSTR* pbstrFileNameInformationString);

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	/////////////////
	// Variables
	/////////////////

	// The underlying PDFInputOutputMgr that this COM interface is wrapping
	auto_ptr<PDFInputOutputMgr> m_apPDFManager;

	/////////////////
	// Methods
	/////////////////

	void validateLicense();
};
