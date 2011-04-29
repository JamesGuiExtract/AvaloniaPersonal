// RuleExecutionSession.h : Declaration of the CRuleExecutionSession

#pragma once

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// CRuleExecutionSession
class ATL_NO_VTABLE CRuleExecutionSession : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CRuleExecutionSession, &CLSID_RuleExecutionSession>,
	public ISupportErrorInfo,
	public IDispatchImpl<IRuleExecutionSession, &IID_IRuleExecutionSession, &LIBID_UCLID_AFCORELib>
{
public:
	CRuleExecutionSession();
	~CRuleExecutionSession();

DECLARE_REGISTRY_RESOURCEID(IDR_RULEEXECUTIONSESSION)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CRuleExecutionSession)
	COM_INTERFACE_ENTRY(IRuleExecutionSession)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IRuleExecutionSession
	STDMETHOD(SetRSDFileName)(/*[in]*/ BSTR bstrFileName,
		/*[out, retval]*/ long *pnStackSize);

	STDMETHOD (SetFKBVersion)(/*[in]*/ BSTR bstrFKBVersion);

private:
	// pointer to the singleton rule execution environment object
	UCLID_AFCORELib::IRuleExecutionEnvPtr m_ipRuleExecutionEnv;

	UCLID_AFCORELib::IRuleExecutionEnvPtr getRuleExecutionEnv();
};
