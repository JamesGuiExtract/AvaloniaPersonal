// WorkflowDefinition.cpp : Implementation of CWorkflowDefinition

#include "stdafx.h"
#include "WorkflowDefinition.h"
#include <UCLIDException.h>
#include <cpputil.h>
#include <ComUtils.h>

using namespace std;

//-------------------------------------------------------------------------------------------------
// CWorkflowDefinition
//-------------------------------------------------------------------------------------------------
CWorkflowDefinition::CWorkflowDefinition()
: m_nID(0)
, m_eType(kUndefined)
, m_nLoadBalanceWeight(1)
{
}
//-------------------------------------------------------------------------------------------------
CWorkflowDefinition::~CWorkflowDefinition()
{
	try
	{
		
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI41849");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkflowDefinition::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IWorkflowDefinition
	};

	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IWorkflowDefinition
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkflowDefinition::get_ID(LONG* pnID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI41850", pnID != __nullptr);
		*pnID = m_nID;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41851"); 

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkflowDefinition::put_ID(LONG nID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_nID = nID;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41852");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkflowDefinition::get_Name(BSTR *pName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI41853", pName != __nullptr);

		*pName = _bstr_t(m_strName.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41854");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkflowDefinition::put_Name(BSTR Name)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_strName = asString(Name);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41855");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkflowDefinition::get_Type(EWorkflowType* pnType)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI41856", pnType != __nullptr);
		*pnType = m_eType;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41857");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkflowDefinition::put_Type(EWorkflowType nType)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_eType = nType;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41858");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkflowDefinition::get_Description(BSTR *pDescription)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI41859", pDescription != __nullptr);

		*pDescription = _bstr_t(m_strDescription.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41860");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkflowDefinition::put_Description(BSTR Description)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_strDescription = asString(Description);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41861");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkflowDefinition::get_LoadBalanceWeight(LONG* pnWeight)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI43423", pnWeight != __nullptr);
		*pnWeight = m_nLoadBalanceWeight;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI43424");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkflowDefinition::put_LoadBalanceWeight(LONG nWeight)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_nLoadBalanceWeight = nWeight;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI43425");

	return S_OK;
}
