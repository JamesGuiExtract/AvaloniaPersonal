// ClipboardObjectManager.h : Declaration of the CClipboardObjectManager

#pragma once

#include "ClipboardManagerWnd.h"
#include "resource.h"       // main symbols

#include <memory>

/////////////////////////////////////////////////////////////////////////////
// CClipboardObjectManager
class ATL_NO_VTABLE CClipboardObjectManager : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CClipboardObjectManager, &CLSID_ClipboardObjectManager>,
	public ISupportErrorInfo,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IClipboardObjectManager, &IID_IClipboardObjectManager, &LIBID_UCLID_COMUTILSLib>
{
public:
	CClipboardObjectManager();

DECLARE_REGISTRY_RESOURCEID(IDR_CLIPBOARDOBJECTMANAGER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CClipboardObjectManager)
	COM_INTERFACE_ENTRY(IClipboardObjectManager)
	COM_INTERFACE_ENTRY2(IDispatch, IClipboardObjectManager)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IClipboardObjectManager
	STDMETHOD(Clear)();
	STDMETHOD(CopyObjectToClipboard)(/*[in]*/ IUnknown *pObj);
	STDMETHOD(GetObjectInClipboard)(/*[out, retval]*/ IUnknown **pObj);
	STDMETHOD(ObjectIsOfType)(/*[in]*/ IID riid, /*[out, retval]*/ VARIANT_BOOL *pbValue);
	STDMETHOD(ObjectIsIUnknownVectorOfType)(/*[in]*/ IID riid, /*[out, retval]*/ VARIANT_BOOL *pbValue);
	STDMETHOD(ObjectIsTypeWithDescription)(/*[in]*/ IID riid, /*[out, retval]*/ VARIANT_BOOL *pbValue);
	STDMETHOD(IUnknownVectorIsOWDOfType)(/*[in]*/ IID riid, /*[out, retval]*/ VARIANT_BOOL *pbValue);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	std::unique_ptr<ClipboardManagerWnd> m_apCBMWnd;

	// Checks if the object is an IClipboardCopyable or contains an IClipboardCopyable
	// object.  In either case it will call the NotifyCopyFromClipboard method of the
	// IClipboardCopyableObject
	void notifyCopiedFromClipboard(const IUnknownPtr& ipObj);

	void validateLicense();
};
