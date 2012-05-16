//==================================================================================================
//
// COPYRIGHT (c) 2001 UCLID SOFTWARE, LLC., IN PUBLISHED AND UNPUBLISHED WORKS
// ALL RIGHTS RESERVED.
//
// FILE:	IcoMapCtl.h
//
// PURPOSE:	
//
// NOTES:	
//
// AUTHORS: Arvind Ganesan (Aug 2001 to present)
//			John Hurd (till July 2001)
//
//==================================================================================================

#pragma once

#include "resource.h"       // main symbols
//#include "auto_ptr2.h"

// Forward declaration
class IcoMapDlg;
class SafeNetLicenseMgr;  

/////////////////////////////////////////////////////////////////////////////
// CIcoMapCtl
class ATL_NO_VTABLE CIcoMapCtl : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CIcoMapCtl, &CLSID_IcoMap>,
	public ISupportErrorInfo,
	public IDispatchImpl<IIcoMapApplication, &IID_IIcoMapApplication, &LIBID_ICOMAPAPPLib>,
	public IDispatchImpl<ICategorizedComponent, &IID_ICategorizedComponent, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IInputReceiver, &IID_IInputReceiver, &LIBID_UCLID_INPUTFUNNELLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<IInputTarget, &IID_IInputTarget, &LIBID_UCLID_INPUTTARGETFRAMEWORKLib>
{
public:
	CIcoMapCtl();
	~CIcoMapCtl();

DECLARE_REGISTRY_RESOURCEID(IDR_ICOMAPCTL)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CIcoMapCtl)
	COM_INTERFACE_ENTRY(IIcoMapApplication)
	COM_INTERFACE_ENTRY2(IDispatch, IIcoMapApplication)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY(ICategorizedComponent)
	COM_INTERFACE_ENTRY(IInputReceiver)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(IInputTarget)
END_COM_MAP()


public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IIcoMapApplication
	STDMETHOD(OnFeatureSelected)(VARIANT_BOOL bReadOnly);
	STDMETHOD(get_EnableFeatureSelection)(/*[out, retval]*/ VARIANT_BOOL *pVal);
	STDMETHOD(SetIcoMapAsCurrentTool)(/*[in]*/ VARIANT_BOOL bIsCurrent);
	STDMETHOD(SetDisplayAdapter)(IDisplayAdapter * ipDisplayAdapter);
	STDMETHOD(SetAttributeManager)(IAttributeManager * ipAttributeManager);
	STDMETHOD(SetPoint)(DOUBLE dX, DOUBLE dY);
	STDMETHOD(SetText)(BSTR text);
	STDMETHOD(ShowIcoMapWindow)(VARIANT_BOOL bShow);
	STDMETHOD(DestroyWindows)();
	STDMETHOD(get_Initialized)(VARIANT_BOOL * pVal);
	STDMETHOD(NotifySketchModified)(long nActualNumOfSegments);
	STDMETHOD(Reset)();
	STDMETHOD(EnableFeatureCreation)(VARIANT_BOOL bEnable);
	STDMETHOD(ProcessKeyUp)(/*[in]*/ long lKeyCode, /*[in]*/ long lShiftKey);
	STDMETHOD(ProcessKeyDown)(/*[in]*/ long lKeyCode, /*[in]*/ long lShiftKey);

// ICategorizedComponent
	STDMETHOD(raw_GetComponentDescription)(BSTR * pbstrComponentDescription);

// IInputReceiver
	STDMETHOD(raw_get_ParentWndHandle)(/*[out, retval]*/ long *pVal);
	STDMETHOD(raw_put_ParentWndHandle)(/*[in]*/ long newVal);
	STDMETHOD(raw_get_WindowShown)(VARIANT_BOOL * pVal);
	STDMETHOD(raw_get_InputIsEnabled)(VARIANT_BOOL * pVal);
	STDMETHOD(raw_get_HasWindow)(VARIANT_BOOL * pVal);
	STDMETHOD(raw_get_WindowHandle)(LONG * pVal);
	STDMETHOD(raw_EnableInput)(BSTR strInputType, BSTR strPrompt);
	STDMETHOD(raw_DisableInput)();
	STDMETHOD(raw_SetEventHandler)(IIREventHandler * pEventHandler);
	STDMETHOD(raw_ShowWindow)(VARIANT_BOOL bShow);
	STDMETHOD(raw_get_UsesOCR)(VARIANT_BOOL *pVal);
	STDMETHOD(raw_SetOCRFilter)(IOCRFilter *pFilter);
	STDMETHOD(raw_SetOCREngine)(IOCREngine *pEngine);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

//	IInputTarget
	STDMETHOD(raw_SetApplicationHook)(IUnknown * pHook);
	STDMETHOD(raw_Activate)();
	STDMETHOD(raw_Deactivate)();
	STDMETHOD(raw_IsVisible)(VARIANT_BOOL *pbValue);

private:
	IcoMapDlg* m_pIcoMapDlg;					// pointer to the application's main dialg
	bool m_bDisplayAdapterSet;					// whether or not setDisplayAdapter() has been called
	bool m_bAttributeManagerSet;				// whether or not setAttributeManager() has been called
	bool m_bInputEnabled;						// whether or not IcoMap command is enabled

	// This was made into a global so it could be accessed in CIcoMapDlg so that it can be 
	// released properly
	//auto_ptr2<SafeNetLicenseMgr> ma_pSnLM;

	IcoMapDlg* getIcoMapDlg(void);
	void destroyIcoMapDlg(void);

	// check license
	void validateLicense();

};
