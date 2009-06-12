// VariantVector.h : Declaration of the CVariantVector

#pragma once

#include "resource.h"       // main symbols

#include <vector>
/////////////////////////////////////////////////////////////////////////////
// CVariantVector
class ATL_NO_VTABLE CVariantVector : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CVariantVector, &CLSID_VariantVector>,
	public IPersistStream,
	public ISupportErrorInfo,
	public IDispatchImpl<IVariantVector, &IID_IVariantVector, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IShallowCopyable, &IID_IShallowCopyable, &LIBID_UCLID_COMUTILSLib>
{
public:
	CVariantVector();
	~CVariantVector();

DECLARE_REGISTRY_RESOURCEID(IDR_VARIANTVECTOR)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CVariantVector)
	COM_INTERFACE_ENTRY(IVariantVector)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IVariantVector)
	COM_INTERFACE_ENTRY(IPersistStream)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
	COM_INTERFACE_ENTRY(IShallowCopyable)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ICopyableObject
	STDMETHOD(Clone)(/*[out, retval]*/ IUnknown* *pObject);
	STDMETHOD(CopyFrom)(/*[in]*/ IUnknown *pObject);

// IShallowCopyable
	STDMETHOD(ShallowCopy)(IUnknown** pObject);

// IVariantVector
	STDMETHOD(Remove)(/*[in]*/ long nBeginIndex, /*[in]*/ long nNumOfItems);
	STDMETHOD(PushBack)(/*[in]*/ VARIANT vtItem);
	STDMETHOD(Clear)();
	STDMETHOD(get_Size)(/*[out, retval]*/ long *pVal);
	STDMETHOD(get_Item)(/*[in]*/ long nIndex, /*[out, retval]*/ VARIANT *pVal);
	STDMETHOD(Contains)(/*[in]*/ VARIANT vtItem, /*[out, retval]*/ VARIANT_BOOL *bValue);
	STDMETHOD(Find)(/*[in]*/ VARIANT vtItem, /*[out, retval]*/ long *pVal);
	STDMETHOD(Insert)(/*[in]*/ long nIndex, /*[in]*/ VARIANT vtItem);
	STDMETHOD(Append)(/*[in]*/ IVariantVector *pVector);
	STDMETHOD(InsertString)(/*[in]*/ long nIndex, /*[in]*/ BSTR newVal);
	STDMETHOD(Set)(long nIndex, VARIANT vtItem);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

//IPersistStream
	STDMETHOD(GetClassID)(CLSID *pClassID);
	STDMETHOD(IsDirty)(void);
	STDMETHOD(Load)(IStream *pStm);
	STDMETHOD(Save)(IStream *pStm, BOOL fClearDirty);
	STDMETHOD(GetSizeMax)(ULARGE_INTEGER *pcbSize);

private:
	//-------------------------------------------------------------------------------------------------
	UCLID_COMUTILSLib::IVariantVectorPtr getThisAsCOMPtr();

	// check license
	void validateLicense();

	// check that this is a supported type
	void validateType(const _variant_t& vtObject);

	// Variables
	// flag to indicate if this object's state has changed since the last 
	// save-to-stream operation
	bool m_bDirty;

	std::vector<_variant_t> m_vecVarCollection;
};

