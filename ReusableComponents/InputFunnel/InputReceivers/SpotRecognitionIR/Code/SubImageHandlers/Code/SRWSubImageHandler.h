// SRWSubImageHandler.h : Declaration of the CSRWSubImageHandler

#pragma once

#include "resource.h"       // main symbols

#include <map>
#include <set>
using namespace std;

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
	STDMETHOD(raw_NotifyAboutToDestroy)(IInputReceiver* pIR);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	////////////////////
	// Variables
	////////////////////

	IInputManager* m_ipInputManager;

	// Keep track of the sub-image window hierarchy so that when a sub-image window is closed, any
	// child sub-image windows are cleaned up properly.
	map<IInputReceiver*, set<IInputReceiver*>> m_mapSubImageHierarchy;

	////////////////////
	// Methods
	////////////////////
	void notifyAboutToDestroy(IInputReceiver* pIR);

	void validateLicense();
};

