// FileProcessingManagerProcess.h : Declaration of the CFileProcessingManagerProcess

#pragma once
#include "resource.h"       // main symbols

#include "FAMProcess.h"

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
	HRESULT FinalConstruct()
	{
		return CoCreateFreeThreadedMarshaler(
			GetControllingUnknown(), &m_pUnkMarshaler.p);
	}

	void FinalRelease()
	{
		m_pUnkMarshaler.Release();
	}

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
	STDMETHOD(Start)(BSTR bstrFPSFile);
	STDMETHOD(Stop)();
	STDMETHOD(GetCounts)(LONG* plNumFilesProcessed, LONG* plNumProcessingErrors,
		LONG* plNumFilesSupplied, LONG* plNumSupplyingErrors);
	STDMETHOD(get_ProcessID)(LONG* plPID);

private:
	//----------------------------------------------------------------------------------------------
	// Variables
	//----------------------------------------------------------------------------------------------
	CComPtr<IUnknown> m_pUnkMarshaler;

	//----------------------------------------------------------------------------------------------
	// Methods
	//----------------------------------------------------------------------------------------------
	void validateLicense();
};

OBJECT_ENTRY_AUTO(__uuidof(FileProcessingManagerProcess), CFileProcessingManagerProcess)
