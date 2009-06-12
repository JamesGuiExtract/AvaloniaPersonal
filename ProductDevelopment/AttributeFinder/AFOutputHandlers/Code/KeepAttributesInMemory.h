// KeepAttributesInMemory.h : Declaration of the CKeepAttributesInMemory

#pragma once

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CKeepAttributesInMemory
class ATL_NO_VTABLE CKeepAttributesInMemory : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CKeepAttributesInMemory, &CLSID_KeepAttributesInMemory>,
	public IPersistStream,
	public ISupportErrorInfo,
	public IDispatchImpl<IKeepAttributesInMemory, &IID_IKeepAttributesInMemory, &LIBID_UCLID_AFOUTPUTHANDLERSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IOutputHandler, &IID_IOutputHandler, &LIBID_UCLID_AFCORELib>
{
public:
	CKeepAttributesInMemory();
	~CKeepAttributesInMemory();

DECLARE_REGISTRY_RESOURCEID(IDR_KEEPATTRIBUTESINMEMORY)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CKeepAttributesInMemory)
	COM_INTERFACE_ENTRY(IOutputHandler)
	COM_INTERFACE_ENTRY2(IDispatch, IOutputHandler)
	COM_INTERFACE_ENTRY(IKeepAttributesInMemory)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IOutputHandler
	STDMETHOD(raw_ProcessOutput)(IIUnknownVector* pAttributes, IAFDocument *pAFDoc, 
		IProgressStatus *pProgressStatus);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IKeepAttributesInMemory
	STDMETHOD(GetAttributes)(/*[out, retval]*/ IIUnknownVector* *pvecAttributes);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

private:
	/////////////
	// Methods
	/////////////
	void validateLicense();

	/////////////
	// Member variables
	/////////////
	IIUnknownVectorPtr m_ipvecAttributes;

	// flag to keep track of whether object is dirty
	bool m_bDirty;
};
