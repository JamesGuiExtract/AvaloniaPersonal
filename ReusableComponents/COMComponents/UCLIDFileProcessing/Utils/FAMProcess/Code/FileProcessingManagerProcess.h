// FileProcessingManagerProcess.h : Declaration of the CFileProcessingManagerProcess

#pragma once
#include "resource.h"       // main symbols

#include "FAMProcess.h"

#include <string>

using namespace std;

// CFileProcessingManagerProcess

class ATL_NO_VTABLE CFileProcessingManagerProcess :
	public CComObjectRootEx<CComSingleThreadModel>,
	public CComCoClass<CFileProcessingManagerProcess, &CLSID_FileProcessingManagerProcess>,
	public ISupportErrorInfo,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IFileProcessingManagerProcess, &IID_IFileProcessingManagerProcess, &LIBID_FAMProcessLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
public:
	CFileProcessingManagerProcess();

DECLARE_REGISTRY_RESOURCEID(IDR_FILEPROCESSINGMANAGERPROCESS)


BEGIN_COM_MAP(CFileProcessingManagerProcess)
	COM_INTERFACE_ENTRY(IFileProcessingManagerProcess)
	COM_INTERFACE_ENTRY2(IDispatch, IFileProcessingManagerProcess)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY_AGGREGATE(IID_IMarshal, m_pUnkMarshaler.p)
END_COM_MAP()

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	DECLARE_GET_CONTROLLING_UNKNOWN()

	HRESULT FinalConstruct();
	void FinalRelease();

	//----------------------------------------------------------------------------------------------
	// ISupportsErrorInfo
	//----------------------------------------------------------------------------------------------
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

	//----------------------------------------------------------------------------------------------
	// ILicensedComponent
	//----------------------------------------------------------------------------------------------
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

	//----------------------------------------------------------------------------------------------
	// IFileProcessingManagerProcess
	//----------------------------------------------------------------------------------------------
	STDMETHOD(Ping)();
	STDMETHOD(Start)(LONG lNumberOfFilesToProcess);
	STDMETHOD(Stop)();
	STDMETHOD(GetCounts)(LONG* plNumFilesProcessedSuccessfully, LONG* plNumProcessingErrors,
		LONG* plNumFilesSupplied, LONG* plNumSupplyingErrors);
	STDMETHOD(get_ProcessID)(LONG* plPID);
	STDMETHOD(get_IsRunning)(VARIANT_BOOL* pvbRunning);
	STDMETHOD(get_FPSFile)(BSTR* pbstrFPSFile);
	STDMETHOD(put_FPSFile)(BSTR bstrFPSFile);
	STDMETHOD(get_AuthenticationRequired)(VARIANT_BOOL* vbAuthenticationRequired);

private:
	//----------------------------------------------------------------------------------------------
	// Variables
	//----------------------------------------------------------------------------------------------
	CComPtr<IUnknown> m_pUnkMarshaler;

	// The FPS file to process
	string m_strFPSFile;

	// The File processing manager used to perform processing
	IFileProcessingManagerPtr m_ipFPM;

	//----------------------------------------------------------------------------------------------
	// Methods
	//----------------------------------------------------------------------------------------------
	void validateLicense();
};

OBJECT_ENTRY_AUTO(__uuidof(FileProcessingManagerProcess), CFileProcessingManagerProcess)
