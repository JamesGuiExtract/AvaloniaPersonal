// RuleExecutionSession.cpp : Implementation of CRuleExecutionSession

#include "stdafx.h"
#include "AFCore.h"
#include "RuleExecutionSession.h"

#include <UCLIDException.h>

//-------------------------------------------------------------------------------------------------
// CRuleExecutionSession
//-------------------------------------------------------------------------------------------------
CRuleExecutionSession::CRuleExecutionSession()
{
}
//-------------------------------------------------------------------------------------------------
CRuleExecutionSession::~CRuleExecutionSession()
{
	try
	{
		// pop the filename on the rule execution environment's stack
		if (m_ipRuleExecutionEnv)
		{
			m_ipRuleExecutionEnv->PopRSDFileName();
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI07491")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleExecutionSession::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IRuleExecutionSession
	};
	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleExecutionSession::SetRSDFileName(BSTR bstrFileName, long *pnStackSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate arguments
		ASSERT_ARGUMENT("ELI11545", pnStackSize != __nullptr);

		// Push ipRuleSet's filename onto the rule execution environment's stack
		// and return the size of the rule execution stack after the push operation
		*pnStackSize = getRuleExecutionEnv()->PushRSDFileName(bstrFileName);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07492")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CRuleExecutionSession::SetFKBVersion(BSTR bstrFKBVersion)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// push ipRuleSet's filename onto the rule execution environment's stack
		// and return the size of the rule execution stack after the push operation
		getRuleExecutionEnv()->FKBVersion = bstrFKBVersion;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI32480")
}
//-------------------------------------------------------------------------------------------------
UCLID_AFCORELib::IRuleExecutionEnvPtr CRuleExecutionSession::getRuleExecutionEnv()
{
	if (m_ipRuleExecutionEnv == __nullptr)
	{
		m_ipRuleExecutionEnv.CreateInstance(CLSID_RuleExecutionEnv);
		ASSERT_RESOURCE_ALLOCATION("ELI32481", m_ipRuleExecutionEnv != __nullptr);
	}

	return m_ipRuleExecutionEnv;
}
//-------------------------------------------------------------------------------------------------
