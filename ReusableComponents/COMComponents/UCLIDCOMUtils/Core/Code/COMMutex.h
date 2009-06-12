// COMMutex.h : Declaration of the CCOMMutex

#pragma once
#include "resource.h"       // main symbols

#include "UCLIDCOMUtils.h"
#include <string>

#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif



// CCOMMutex

class ATL_NO_VTABLE CCOMMutex :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CCOMMutex, &CLSID_COMMutex>,
	public ISupportErrorInfo,
	public IDispatchImpl<ICOMMutex, &IID_ICOMMutex, &LIBID_UCLID_COMUTILSLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
public:
	CCOMMutex();
	~CCOMMutex();
	
DECLARE_REGISTRY_RESOURCEID(IDR_COMMUTEX)

BEGIN_COM_MAP(CCOMMutex)
	COM_INTERFACE_ENTRY(ICOMMutex)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
	}

public:
// ICOMMutex
	STDMETHOD(CreateNamed)(BSTR bstrMutexName);
	STDMETHOD(Acquire)(void);
	STDMETHOD(ReleaseNamedMutex)(void);

private:
	// Variables
	std::string m_strMutexName;	
	CMutex *m_pMutex;
};

OBJECT_ENTRY_AUTO(__uuidof(COMMutex), CCOMMutex)
