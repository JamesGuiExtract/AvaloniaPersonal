// InputEntity.h : Declaration of the CInputEntity
#pragma once

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CInputEntity
class ATL_NO_VTABLE CInputEntity : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CInputEntity, &CLSID_InputEntity>,
	public ISupportErrorInfo,
	public IDispatchImpl<IInputEntity, &IID_IInputEntity, &LIBID_UCLID_INPUTFUNNELLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CInputEntity();
	~CInputEntity();


DECLARE_REGISTRY_RESOURCEID(IDR_INPUTENTITY)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CInputEntity)
	COM_INTERFACE_ENTRY(IInputEntity)
//DEL 	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IInputEntity)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IInputEntity
	STDMETHOD(InitInputEntity)(/*[in]*/ IInputEntityManager *pEntityManager, /*[in]*/ BSTR strID);
	STDMETHOD(GetOCRImage)(/*[out, retval]*/ BSTR *pbstrImageFileName);
	STDMETHOD(HasBeenOCRed)(/*[out, retval]*/ VARIANT_BOOL *pbHasBeenOCRed);
	STDMETHOD(GetPersistentSourceName)(/*[out, retval]*/ BSTR *pstrSourceName);
	STDMETHOD(IsFromPersistentSource)(/*[out, retval]*/ VARIANT_BOOL *pbIsFromPersistentSource);
	STDMETHOD(IsMarkedAsUsed)(/*[out, retval]*/ VARIANT_BOOL *pbIsMarkedAsUsed);
	STDMETHOD(MarkAsUsed)(/*[in]*/ VARIANT_BOOL bValue);
	STDMETHOD(CanBeMarkedAsUsed)(/*[out, retval]*/ VARIANT_BOOL *pbCanBeMarkedAsUsed);
	STDMETHOD(GetText)(/*[out, retval]*/ BSTR *pstrText);
	STDMETHOD(SetText)(/*[in]*/ BSTR strText);
	STDMETHOD(CanBeDeleted)(/*[out, retval]*/ VARIANT_BOOL *pbCanBeDeleted);
	STDMETHOD(Delete)();
	STDMETHOD(GetIndirectSource)(BSTR *pstrIndirectSource);
	STDMETHOD(HasIndirectSource)(VARIANT_BOOL *pbHasIndirectSourceName);
	STDMETHOD(GetOCRZones)(/*[out, retval]*/ IIUnknownVector **pRasterZones);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	// input entity manager for this input entity
	UCLID_INPUTFUNNELLib::IInputEntityManagerPtr m_ipEntityMgr;
	// this input entity id in BSTR format
	_bstr_t m_bstrEntityID;

	// validate license, throw exception if the component is not licensed
	void validateLicense();
};

