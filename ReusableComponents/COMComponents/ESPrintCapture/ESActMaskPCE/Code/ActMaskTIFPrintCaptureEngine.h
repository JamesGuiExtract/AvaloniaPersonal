// ActMaskTIFPrintCaptureEngine.h : Declaration of the CActMaskTIFPrintCaptureEngine

#pragma once
#include "resource.h"       
#include "ESActMaskPCE.h"

#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

//--------------------------------------------------------------------------------------------------
// CActMaskTIFPrintCaptureEngine
//--------------------------------------------------------------------------------------------------
class ATL_NO_VTABLE CActMaskTIFPrintCaptureEngine :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CActMaskTIFPrintCaptureEngine, &CLSID_ActMaskTIFPrintCaptureEngine>,
	public ISupportErrorInfo,
	public IDispatchImpl<IActMaskTIFPrintCaptureEngine, &IID_IActMaskTIFPrintCaptureEngine, &LIBID_ESActMaskPCELib, /*wMajor =*/ 1, /*wMinor =*/ 0>,
	public IDispatchImpl<IPrintCaptureEngine, &IID_IPrintCaptureEngine, &LIBID_ESPrintCaptureCoreLib, /* wMajor = */ 1>
{
public:
	//----------------------------------------------------------------------------------------------
	// Constructor/Destructor
	//----------------------------------------------------------------------------------------------
	CActMaskTIFPrintCaptureEngine();
	~CActMaskTIFPrintCaptureEngine();

	DECLARE_REGISTRY_RESOURCEID(IDR_ACTMASKTIFPRINTCAPTUREENGINE)

	//----------------------------------------------------------------------------------------------
	// COM Map
	//----------------------------------------------------------------------------------------------
	BEGIN_COM_MAP(CActMaskTIFPrintCaptureEngine)
		COM_INTERFACE_ENTRY(IActMaskTIFPrintCaptureEngine)
		COM_INTERFACE_ENTRY2(IDispatch, IPrintCaptureEngine)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
		COM_INTERFACE_ENTRY(IPrintCaptureEngine)
	END_COM_MAP()

	//----------------------------------------------------------------------------------------------
	// ISupportsErrorInfo
	//----------------------------------------------------------------------------------------------
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

	//----------------------------------------------------------------------------------------------
	// IPrintCaptureEngine Methods
	//----------------------------------------------------------------------------------------------
	STDMETHOD(raw_Install)(BSTR bstrHandlerApp);
	STDMETHOD(raw_Uninstall)();

	//----------------------------------------------------------------------------------------------
	DECLARE_PROTECT_FINAL_CONSTRUCT()
	HRESULT FinalConstruct();
	void FinalRelease();
};

OBJECT_ENTRY_AUTO(__uuidof(ActMaskTIFPrintCaptureEngine), CActMaskTIFPrintCaptureEngine)
