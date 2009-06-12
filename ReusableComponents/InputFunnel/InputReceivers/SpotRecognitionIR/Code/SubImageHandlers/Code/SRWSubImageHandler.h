// SRWSubImageHandler.h : Declaration of the CSRWSubImageHandler

#pragma once

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CSRWSubImageHandler
class ATL_NO_VTABLE CSRWSubImageHandler : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CSRWSubImageHandler, &CLSID_SRWSubImageHandler>,
	public ISupportErrorInfo,
	public IDispatchImpl<ISRWSubImageHandler, &IID_ISRWSubImageHandler, &LIBID_UCLID_SUBIMAGEHANDLERSLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>,
	public IDispatchImpl<ISubImageHandler, &IID_ISubImageHandler, &LIBID_UCLID_SPOTRECOGNITIONIRLib>
{
public:
	CSRWSubImageHandler();
	~CSRWSubImageHandler();

DECLARE_REGISTRY_RESOURCEID(IDR_SRWSUBIMAGEHANDLER)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CSRWSubImageHandler)
	COM_INTERFACE_ENTRY(ISRWSubImageHandler)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, ISRWSubImageHandler)
	COM_INTERFACE_ENTRY(ILicensedComponent)
	COM_INTERFACE_ENTRY(ISubImageHandler)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ISRWSubImageHandler
	STDMETHOD(SetInputManager)(/*[in]*/ IInputManager *pInputManager);

// ISubImageHandler
	STDMETHOD(raw_NotifySubImageCreated)(ISpotRecognitionWindow *pSourceSRWindow, 
		IRasterZone *pSubImageZone, double dRotationAngle);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
//	IInputManagerPtr m_ipInputManager;
	IInputManager* m_ipInputManager;

	////////////////////
	// Helper functions
	void validateLicense();
};

