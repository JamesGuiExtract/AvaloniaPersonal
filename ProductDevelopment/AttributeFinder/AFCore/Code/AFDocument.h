// AFDocument.h : Declaration of the CAFDocument

#pragma once

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CAFDocument
class ATL_NO_VTABLE CAFDocument : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CAFDocument, &CLSID_AFDocument>,
	public ISupportErrorInfo,
	public IDispatchImpl<IAFDocument, &IID_IAFDocument, &LIBID_UCLID_AFCORELib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ICopyableObject, &IID_ICopyableObject, &LIBID_UCLID_COMUTILSLib>
{
public:
	CAFDocument();
	~CAFDocument();

DECLARE_REGISTRY_RESOURCEID(IDR_AFDOCUMENT)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CAFDocument)
	COM_INTERFACE_ENTRY(IAFDocument)
	COM_INTERFACE_ENTRY2(IDispatch,IAFDocument)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ICopyableObject)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IAFDocument
	STDMETHOD(get_ObjectTags)(/*[out, retval]*/ IStrToObjectMap* *pVal);
	STDMETHOD(put_ObjectTags)(/*[in]*/ IStrToObjectMap* newVal);
	STDMETHOD(get_StringTags)(/*[out, retval]*/ IStrToStrMap* *pVal);
	STDMETHOD(put_StringTags)(/*[in]*/ IStrToStrMap* newVal);
	STDMETHOD(get_Text)(/*[out, retval]*/ ISpatialString* *pVal);
	STDMETHOD(put_Text)(/*[in]*/ ISpatialString* newVal);

// ICopyableObject
	STDMETHOD(raw_Clone)(IUnknown ** pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL *pbValue);

private:
	//////////
	// Variables
	//////////
	ISpatialStringPtr m_ipText;
	IStrToStrMapPtr m_ipStringTags;
	IStrToObjectMapPtr m_ipObjectTags;

	//////////
	// Methods
	//////////
	void validateLicense();
};
