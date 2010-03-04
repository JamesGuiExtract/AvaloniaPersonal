// ObjectPropertiesUI.h : Declaration of the CObjectPropertiesUI

#ifndef __OBJECTPROPERTIESUI_H_
#define __OBJECTPROPERTIESUI_H_

/////////////////////////////////////////////////////////////////////////////
// CObjectPropertiesUI
class ATL_NO_VTABLE CObjectPropertiesUI : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CObjectPropertiesUI, &CLSID_ObjectPropertiesUI>,
	public ISupportErrorInfo,
	public IDispatchImpl<IObjectPropertiesUI, &IID_IObjectPropertiesUI, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:

DECLARE_REGISTRY_RESOURCEID(IDR_OBJECTPROPERTIESUI)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CObjectPropertiesUI)
	COM_INTERFACE_ENTRY(IObjectPropertiesUI)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IObjectPropertiesUI)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IObjectPropertiesUI
	STDMETHOD(DisplayProperties1)(IUnknown *pObj, BSTR strTitle, VARIANT_BOOL *pAppliedChanges);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	// check license
	void validateLicense();
};

#endif //__OBJECTPROPERTIESUI_H_
