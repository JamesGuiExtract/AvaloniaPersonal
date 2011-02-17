// StringPair.h : Declaration of the CStringPair

#pragma once

#include "resource.h"       // main symbols

#include <string>

/////////////////////////////////////////////////////////////////////////////
// CStringPair
class ATL_NO_VTABLE CStringPair : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CStringPair, &CLSID_StringPair>,
	public ISupportErrorInfo,
	public IDispatchImpl<IStringPair, &IID_IStringPair, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IPersistStream,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CStringPair();
	~CStringPair();

DECLARE_REGISTRY_RESOURCEID(IDR_STRINGPAIR)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CStringPair)
	COM_INTERFACE_ENTRY(IStringPair)
	COM_INTERFACE_ENTRY2(IDispatch, IStringPair)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IStringPair
	STDMETHOD(get_StringValue)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_StringValue)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_StringKey)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_StringKey)(/*[in]*/ BSTR newVal);
	STDMETHOD(SetKeyValuePair)(BSTR bstrKey, BSTR bstrVal);
	STDMETHOD(GetKeyValuePair)(BSTR* pbstrKey, BSTR* pbstrVal);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ICopyableObject
	STDMETHOD(Clone)(/*[out, retval]*/ IUnknown* *pObject);
	STDMETHOD(CopyFrom)(/*[in]*/ IUnknown *pObject);

//IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

private:
	////////////////
	// Variables
	////////////////
	std::string m_strKey;
	std::string m_strValue;
	// flag to indicate if this object's state has changed since the last 
	// save-to-stream operation
	bool m_bDirty;

	///////////////
	// Methods
	///////////////
	// check license
	void validateLicense();
};

