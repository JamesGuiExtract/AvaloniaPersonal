// LongToLongMap.h : Declaration of the CLongToLongMap

#pragma once

#include "resource.h"       // main symbols

#include <map>
#include <string>
#include <comdef.h>

/////////////////////////////////////////////////////////////////////////////
// CLongToLongMap
class ATL_NO_VTABLE CLongToLongMap : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CLongToLongMap, &CLSID_LongToLongMap>,
	public ISupportErrorInfo,
	public IDispatchImpl<ILongToLongMap, &IID_ILongToLongMap, &LIBID_UCLID_COMUTILSLib>,
	public IPersistStream,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CLongToLongMap();
	~CLongToLongMap();

DECLARE_REGISTRY_RESOURCEID(IDR_LONGTOLONGMAP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CLongToLongMap)
	COM_INTERFACE_ENTRY(ILongToLongMap)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, ILongToLongMap)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ILongToLongMap
	STDMETHOD(get_Size)(/*[out, retval]*/ long *pVal);
	STDMETHOD(GetKeys)(/*[out, retval]*/ IVariantVector** pKeys);
	STDMETHOD(Clear)();
	STDMETHOD(RemoveItem)(/*[in]*/ long key);
	STDMETHOD(Contains)(/*[in]*/ long key, /*[out, retval]*/ VARIANT_BOOL *bFound);
	STDMETHOD(GetValue)(/*[in]*/ long key, long *pValue);
	STDMETHOD(Set)(/*[in]*/ long key, /*[in]*/ long value);
	STDMETHOD(GetKeyValue)(/*[in]*/ long nIndex, /*[out]*/ long *pKey, /*[out]*/ long *pValue);
	STDMETHOD(RenameKey)(/*[in]*/ long key, /*[in]*/ long newKeyName);

//IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// ICopyableObject
	STDMETHOD(Clone)(IUnknown * * pObject);
	STDMETHOD(CopyFrom)(IUnknown * pObject);

private:
	////////////
	// Methods
	////////////
	// check license
	void validateLicense();

	//////////////
	// Variables
	//////////////
	std::map<long, long> m_mapKeyToValue;

	// flag to indicate if this object's state has changed since the last 
	// save-to-stream operation
	bool m_bDirty;
};
