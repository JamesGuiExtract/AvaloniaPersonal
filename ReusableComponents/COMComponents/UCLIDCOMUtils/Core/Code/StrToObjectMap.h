
#pragma once
#include "resource.h"       // main symbols

#include <map>

#include <stringCSIS.h>

/////////////////////////////////////////////////////////////////////////////
// CStrToObjectMap
class ATL_NO_VTABLE CStrToObjectMap : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CStrToObjectMap, &CLSID_StrToObjectMap>,
	public IPersistStream,
	public ISupportErrorInfo,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IStrToObjectMap, &IID_IStrToObjectMap, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IShallowCopyable, &IID_IShallowCopyable, &LIBID_UCLID_COMUTILSLib>
{
public:
	CStrToObjectMap();
	~CStrToObjectMap();

DECLARE_REGISTRY_RESOURCEID(IDR_STRTOOBJECTMAP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CStrToObjectMap)
	COM_INTERFACE_ENTRY(IStrToObjectMap)
	COM_INTERFACE_ENTRY2(IDispatch, IStrToObjectMap)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IShallowCopyable)
END_COM_MAP()

public:
	STDMETHOD(get_CaseSensitive)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(put_CaseSensitive)(/*[in]*/ VARIANT_BOOL newVal);
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IStrToObjectMap
	STDMETHOD(get_Size)(/*[out, retval]*/ long *pVal);
	STDMETHOD(GetKeys)(/*[out, retval]*/ IVariantVector** pKeys);
	STDMETHOD(Clear)();
	STDMETHOD(RemoveItem)(/*[in]*/ BSTR key);
	STDMETHOD(Contains)(/*[in]*/ BSTR key, /*[out, retval]*/ VARIANT_BOOL *bFound);
	STDMETHOD(GetValue)(/*[in]*/ BSTR key, /*[out, retval]*/ IUnknown* *pObject);
	STDMETHOD(Set)(/*[in]*/ BSTR key, /*[in]*/ IUnknown* pObject);
	STDMETHOD(GetKeyValue)(/*[in]*/ long nIndex, /*[out]*/ BSTR *pstrKey, /*[out]*/ IUnknown* *pObject);
	STDMETHOD(RenameKey)(/*[in]*/ BSTR strKey, /*[in]*/ BSTR strNewKeyName);

// IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

// ICopyableObject
	STDMETHOD(Clone)(IUnknown * * pObject);
	STDMETHOD(CopyFrom)(IUnknown * pObject);

// IShallowCopyable
	STDMETHOD(ShallowCopy)(IUnknown** pObject);

private:
	//////////////
	// Variables
	//////////////

	std::map<stringCSIS, IUnknownPtr> m_mapKeyToValue;

	// flag to indicate case sensitivity
	bool m_bCaseSensitive;

	// flag to indicate if this object's state has changed since the last 
	// save-to-stream operation
	bool m_bDirty;

	////////////
	// Methods
	////////////

	void validateLicense();

	void clear();
};
