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
		if (m_spRuleExecutionEnv)
		{
			m_spRuleExecutionEnv->PopRSDFileName();
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
STDMETHODIMP CRuleExecutionSession::SetRSDFileName(BSTR strFileName, long *pnStackSize)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// validate arguments
		ASSERT_ARGUMENT("ELI11545", pnStackSize != __nullptr);

		// if the rule execution environment object has not yet been
		// created, create it
		if (m_spRuleExecutionEnv == __nullptr)
		{
			m_spRuleExecutionEnv.CreateInstance(CLSID_RuleExecutionEnv);
			ASSERT_RESOURCE_ALLOCATION("ELI07487", m_spRuleExecutionEnv != __nullptr);
		}

		// push ipRuleSet's filename onto the rule execution environment's stack
		// and return the size of the rule execution stack after the push operation
		*pnStackSize = m_spRuleExecutionEnv->PushRSDFileName(strFileName);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI07492")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
