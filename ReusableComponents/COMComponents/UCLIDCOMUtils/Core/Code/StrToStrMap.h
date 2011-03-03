// StrToStrMap.h : Declaration of the CStrToStrMap

#pragma once

#include "resource.h"       // main symbols

#include <map>
#include <string>
#include <comdef.h>

/////////////////////////////////////////////////////////////////////////////
// CStrToStrMap
class ATL_NO_VTABLE CStrToStrMap : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CStrToStrMap, &CLSID_StrToStrMap>,
	public ISupportErrorInfo,
	public IDispatchImpl<IStrToStrMap, &IID_IStrToStrMap, &LIBID_UCLID_COMUTILSLib>,
	public IPersistStream,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CStrToStrMap();
	~CStrToStrMap();

DECLARE_REGISTRY_RESOURCEID(IDR_STRTOSTRMAP)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CStrToStrMap)
	COM_INTERFACE_ENTRY(IStrToStrMap)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IStrToStrMap)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

// IStrToStrMap
	STDMETHOD(get_Size)(/*[out, retval]*/ long *pVal);
	STDMETHOD(GetKeys)(/*[out, retval]*/ IVariantVector** pKeys);
	STDMETHOD(Clear)();
	STDMETHOD(RemoveItem)(/*[in]*/ BSTR key);
	STDMETHOD(Contains)(/*[in]*/ BSTR key, /*[out, retval]*/ VARIANT_BOOL *bFound);
	STDMETHOD(GetValue)(/*[in]*/ BSTR key, BSTR *pValue);
	STDMETHOD(Set)(/*[in]*/ BSTR key, /*[in]*/ BSTR value);
	STDMETHOD(GetKeyValue)(/*[in]*/ long nIndex, /*[out]*/ BSTR *pstrKey, /*[out]*/ BSTR *pstrValue);
	STDMETHOD(RenameKey)(/*[in]*/ BSTR strKey, /*[in]*/ BSTR strNewKeyName);
	STDMETHOD(Merge)(/*[in]*/ IStrToStrMap *pMapToMerge, /*[in]*/ EMergeMethod eMergeMethod);
	STDMETHOD(MergeKeyValue)(/*[in]*/ BSTR bstrKey, /*[in]*/ BSTR bstrValue, 
		/*[in]*/ EMergeMethod eMergeMethod);
	STDMETHOD(GetAllKeyValuePairs)(IIUnknownVector** ppPairs);

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
	//////////////
	// Variables
	//////////////
	std::map<std::string, std::string> m_mapKeyToValue;

	// flag to indicate if this object's state has changed since the last 
	// save-to-stream operation
	bool m_bDirty;

	////////////
	// Methods
	////////////

	// Add the specified key/value pair to the map using the provided method
	void mergeKeyValue(const _bstr_t &bstrKey, const _bstr_t &bstrValue, EMergeMethod eMergeMethod);

	// Internal clear method
	void clear();

	// check license
	void validateLicense();
};
