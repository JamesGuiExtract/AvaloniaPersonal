// LongToObjectMap.h : Declaration of the CLongToObjectMap

#ifndef __LONGTOOBJECTMAP_H_
#define __LONGTOOBJECTMAP_H_

#include "resource.h"       // main symbols
#include <map>

/////////////////////////////////////////////////////////////////////////////
// CLongToObjectMap
class ATL_NO_VTABLE CLongToObjectMap : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CLongToObjectMap, &CLSID_LongToObjectMap>,
	public IPersistStream,
	public ISupportErrorInfo,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ILongToObjectMap, &IID_ILongToObjectMap, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IShallowCopyable, &IID_IShallowCopyable, &LIBID_UCLID_COMUTILSLib>
{
public:
	CLongToObjectMap();
	~CLongToObjectMap();

DECLARE_REGISTRY_RESOURCEID(IDR_LONGTOOBJECTMAP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CLongToObjectMap)
	COM_INTERFACE_ENTRY(ILongToObjectMap)
	COM_INTERFACE_ENTRY2(IDispatch, ILongToObjectMap)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IShallowCopyable)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// ILongToObjectMap
	STDMETHOD(get_Size)(/*[out, retval]*/ long *pVal);
	STDMETHOD(GetKeys)(/*[out, retval]*/ IVariantVector** pKeys);
	STDMETHOD(Clear)();
	STDMETHOD(RemoveItem)(/*[in]*/ long key);
	STDMETHOD(Contains)(/*[in]*/ long key, /*[out, retval]*/ VARIANT_BOOL *bFound);
	STDMETHOD(GetValue)(/*[in]*/ long key, /*[out, retval]*/ IUnknown* *pObject);
	STDMETHOD(Set)(/*[in]*/ long key, /*[in]*/ IUnknown* pObject);
	STDMETHOD(GetKeyValue)(/*[in]*/ long nIndex, /*[out]*/ long *pstrKey, /*[out]*/ IUnknown* *pObject);
	STDMETHOD(RenameKey)(/*[in]*/ long strKey, /*[in]*/ long strNewKeyName);

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
	// check license
	void validateLicense();

	// clear this object
	void clear();

	std::map<long, IUnknownPtr> m_mapKeyToValue;

	// flag to indicate if this object's state has changed since the last 
	// save-to-stream operation
	bool m_bDirty;
};

#endif //__LONGTOOBJECTMAP_H_
