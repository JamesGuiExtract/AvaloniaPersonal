// InputManagerSingleton.h : Declaration of the CInputManagerSingleton

#ifndef __INPUTMANAGERSINGLETON_H_
#define __INPUTMANAGERSINGLETON_H_

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CInputManagerSingleton
class ATL_NO_VTABLE CInputManagerSingleton : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CInputManagerSingleton, &CLSID_InputManagerSingleton>,
	public ISupportErrorInfo,
	public IDispatchImpl<IInputManagerSingleton, &IID_IInputManagerSingleton, &LIBID_UCLID_INPUTFUNNELLib>
{
public:
	CInputManagerSingleton();
	~CInputManagerSingleton();

DECLARE_REGISTRY_RESOURCEID(IDR_INPUTMANAGERSINGLETON)

DECLARE_CLASSFACTORY_SINGLETON(CInputManagerSingleton)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CInputManagerSingleton)
	COM_INTERFACE_ENTRY(IInputManagerSingleton)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IInputManagerSingleton
	STDMETHOD(GetInstance)(/*[out, retval]*/ IInputManager **pInputManager);
	STDMETHOD(DeleteInstance)();

private:
	UCLID_INPUTFUNNELLib::IInputManagerPtr m_ipInputManager;
};

#endif //__INPUTMANAGERSINGLETON_H_
