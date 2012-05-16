

#pragma once

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CIcoMapInputContext
class ATL_NO_VTABLE CIcoMapInputContext : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CIcoMapInputContext, &CLSID_IcoMapInputContext>,
	public ISupportErrorInfo,
	public IDispatchImpl<IIcoMapInputContext, &IID_IIcoMapInputContext, &LIBID_ICOMAPAPPLib>,
	public IDispatchImpl<IInputContext, &IID_IInputContext, &LIBID_UCLID_INPUTFUNNELLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ISRWEventHandler, &IID_ISRWEventHandler, &LIBID_UCLID_SPOTRECOGNITIONIRLib>
{
public:
	CIcoMapInputContext();
	~CIcoMapInputContext();

DECLARE_REGISTRY_RESOURCEID(IDR_ICOMAPINPUTCONTEXT)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CIcoMapInputContext)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(IInputContext)
	COM_INTERFACE_ENTRY2(IDispatch, IInputContext)
	COM_INTERFACE_ENTRY(IIcoMapInputContext)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ISRWEventHandler)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IInputContext
	STDMETHOD(raw_Activate)(IInputManager* pInputManager);
	STDMETHOD(raw_NotifyNewIRConnected)(IInputReceiver *pNewInputReceiver);

// IIcoMapInputContext
	STDMETHOD(SetIcoMapApplication)(IIcoMapApplication* pIcoMapApplication);

// ISRWEventHandler
	STDMETHOD(raw_AboutToRecognizeParagraphText)();
	STDMETHOD(raw_AboutToRecognizeLineText)();
	STDMETHOD(raw_NotifyKeyPressed)(long nKeyCode, short shiftState);
	STDMETHOD(raw_NotifyCurrentPageChanged)();
	STDMETHOD(raw_NotifyEntitySelected)(long nZoneID);
	STDMETHOD(raw_NotifyZoneEntityCreated)(long nZoneID);
	STDMETHOD(raw_NotifyFileOpened)(BSTR bstrFileFullPath);
	STDMETHOD(raw_NotifyOpenToolbarButtonPressed)(VARIANT_BOOL *pbContinueWithOpen);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	// This input context does something in addition to the land-records
	// input context and delegates the incoming method calls to the land-records
	// input context after doing special processing specific to IcoMap
	// The following pointer will point to the Land records input context
	IInputContextPtr m_ipLandRecordsIC;

	// raw point to the icomap application
	IIcoMapApplication* m_pIcoMapApplication;

	// connects this object as the event handler for the specified SRIR
	bool connectSRIR(IInputReceiver *pNewInputReceiver);

	void validateLicense();
};
