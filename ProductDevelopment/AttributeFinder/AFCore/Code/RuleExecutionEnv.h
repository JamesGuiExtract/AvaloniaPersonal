// RuleExecutionEnv.h : Declaration of the CRuleExecutionEnv

#pragma once

#include "resource.h"       // main symbols

#include <string>
#include <map>
#include <stack>
using namespace std;

#include <afxmt.h>

/////////////////////////////////////////////////////////////////////////////
// CRuleExecutionEnv
class ATL_NO_VTABLE CRuleExecutionEnv : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CRuleExecutionEnv, &CLSID_RuleExecutionEnv>,
	public ISupportErrorInfo,
	public IDispatchImpl<IRuleExecutionEnv, &IID_IRuleExecutionEnv, &LIBID_UCLID_AFCORELib>
{
public:
	CRuleExecutionEnv();

DECLARE_REGISTRY_RESOURCEID(IDR_RULEEXECUTIONENV)

DECLARE_CLASSFACTORY_SINGLETON(CRuleExecutionEnv)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CRuleExecutionEnv)
	COM_INTERFACE_ENTRY(IRuleExecutionEnv)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IRuleExecutionEnv
	STDMETHOD(GetCurrentRSDFileDir)(/*[out, retval]*/ BSTR *pstrRSDFileDir);
	STDMETHOD(GetCurrentRSDFileName)(/*[out, retval]*/ BSTR *pstrFileName);
	STDMETHOD(PushRSDFileName)(/*[in]*/ BSTR strFileName, 
		/*[out, retval]*/ long *pnStackSize);
	STDMETHOD(PopRSDFileName)(/*[out, retval]*/ long *pnStackSize);
	STDMETHOD(IsRSDFileExecuting)(/*[in]*/ BSTR bstrFileName, 
		/*[out, retval]*/ VARIANT_BOOL *pbValue);
	STDMETHOD(get_FKBVersion)(BSTR *pVal);
	STDMETHOD(put_FKBVersion)(BSTR newVal);

private:
	// member variable to keep track of which thread is
	// associated which which RSD file
	// NOTE: this member variable is being defined as static 
	// despite this COM object being a singleton just to ensure
	// that this map does not get deleted for the lifetime of
	// the application (while the COM framework will ensure
	// that multiple instances of this object are not created, 
	// we can't be sure about when the COM framework will 
	// delete the singleton object)
	static map<DWORD, stack<string> > m_mapThreadIDToRSDFileStack;

	// The FKB version in use (per thread).
	static map<DWORD, string> m_mapThreadIDToFKBVersion;

	// method to get the RSD file stack associated with the current thread
	// If no stack is associated with the current thread, an exception will
	// be thrown
	// if bThrowExceptionIfStackEmpty == true, and the stack associated with the
	// current thread is empty, an exception will be thrown.
	stack<string>& getCurrentStack(bool bThrowExceptionIfStackEmpty = false);

	// Gets the FKB version to use for the current thread.
	string& getFKBVersionString();

	// This Mutex will guard against simultaneous accesses (read or write)
	// to the stack (m_mapThreadIDToRSDFileStack)
	static CMutex m_mutex;

};
