
#pragma once

#include "resource.h"       // main symbols
#include <SystemHookMsgManager.h>

#include <map>
#include <vector>

/////////////////////////////////////////////////////////////////////////////
// CInputTargetManager
/////////////////////////////////////////////////////////////////////////////
class ATL_NO_VTABLE CInputTargetManager : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CInputTargetManager, &CLSID_InputTargetManager>,
	public ISupportErrorInfo,
	public IDispatchImpl<IInputTargetManager, &IID_IInputTargetManager, &LIBID_UCLID_INPUTTARGETFRAMEWORKLib>,
	public IDispatchImpl<ILicensedComponent, &IID_ILicensedComponent, &LIBID_UCLID_COMLMLib>
{
public:
	CInputTargetManager();
	~CInputTargetManager();

DECLARE_REGISTRY_RESOURCEID(IDR_INPUTTARGETMANAGER)

DECLARE_CLASSFACTORY_SINGLETON(CInputTargetManager)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CInputTargetManager)
	COM_INTERFACE_ENTRY(IInputTargetManager)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IInputTargetManager)
	COM_INTERFACE_ENTRY(ILicensedComponent)
END_COM_MAP()

public:
//	ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

//	IInputTargetManager
	STDMETHOD(NotifyInputTargetWindowClosed)(/*[in]*/ IInputTarget *pInputTarget);
	STDMETHOD(NotifyInputTargetWindowActivated)(/*[in]*/ IInputTarget *pInputTarget);
	STDMETHOD(AddInputTarget)(/*[in]*/ IInputTarget *pInputTarget);

// ILicensedComponent
	STDMETHOD(raw_IsLicensed)(VARIANT_BOOL * pbValue);

private:
	//////////////////////
	// Helper functions
	//////////////////////
	UCLID_INPUTTARGETFRAMEWORKLib::IInputTargetManagerPtr getThisAsCOMPtr();

	void validateLicense();
	
	//////////////////////
	// Member variables
	//////////////////////
	IInputTarget *m_pLastActiveInputTarget;
	std::vector<IInputTarget *> m_vecInputTargets;
	std::vector<HWND> m_vecWindowHandles;
	std::map<HWND, IInputTarget *> m_mapHandleToInputTarget;
	IHighlightWindowPtr m_ipHighlightWindow;
};
