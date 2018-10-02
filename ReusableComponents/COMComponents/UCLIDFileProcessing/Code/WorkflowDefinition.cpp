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
STDMETHODIMP CWorkflowDefinition::get_StartAction(BSTR *pStartAction)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI41862", pStartAction != __nullptr);

		*pStartAction = _bstr_t(m_strStartAction.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41863");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkflowDefinition::put_StartAction(BSTR StartAction)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_strStartAction = asString(StartAction);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41864");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkflowDefinition::get_EndAction(BSTR *pEndAction)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI41865", pEndAction != __nullptr);

		*pEndAction = _bstr_t(m_strEndAction.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41866");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkflowDefinition::put_EndAction(BSTR EndAction)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_strEndAction = asString(EndAction);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41867");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkflowDefinition::get_PostWorkflowAction(BSTR *pPostWorkflowAction)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI41868", pPostWorkflowAction != __nullptr);

		*pPostWorkflowAction = _bstr_t(m_strPostWorkflowAction.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41869");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkflowDefinition::put_PostWorkflowAction(BSTR PostWorkflowAction)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_strPostWorkflowAction = asString(PostWorkflowAction);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41870");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkflowDefinition::get_DocumentFolder(BSTR *pDocumentFolder)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI41871", pDocumentFolder != __nullptr);

		*pDocumentFolder = _bstr_t(m_strDocumentFolder.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41872");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkflowDefinition::put_DocumentFolder(BSTR DocumentFolder)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_strDocumentFolder = asString(DocumentFolder);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41873");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkflowDefinition::get_OutputAttributeSet(BSTR *pOutputAttributeSet)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI41874", pOutputAttributeSet != __nullptr);

		*pOutputAttributeSet = _bstr_t(m_strOutputAttributeSet.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41875");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkflowDefinition::put_OutputAttributeSet(BSTR OutputAttributeSet)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_strOutputAttributeSet = asString(OutputAttributeSet);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI41876");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkflowDefinition::get_OutputFileMetadataField(BSTR *pOutputFileMetadataField)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI42046", pOutputFileMetadataField != __nullptr);

		*pOutputFileMetadataField = _bstr_t(m_strOutputFileMetadataField.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI42047");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkflowDefinition::put_OutputFileMetadataField(BSTR OutputFileMetadataField)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_strOutputFileMetadataField = asString(OutputFileMetadataField);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI42048");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkflowDefinition::get_OutputFilePathInitializationFunction(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI43189", pVal != __nullptr);

		*pVal = get_bstr_t(m_strOutputFilePathInitializationFunction).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI43190");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkflowDefinition::put_OutputFilePathInitializationFunction(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_strOutputFilePathInitializationFunction = asString(newVal);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI43191");

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
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkflowDefinition::get_EditAction(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI45213", pVal != __nullptr);

		*pVal = get_bstr_t(m_strEditAction).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI45214");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkflowDefinition::put_EditAction(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_strEditAction = asString(newVal);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI45215");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkflowDefinition::get_PostEditAction(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI45216", pVal != __nullptr);

		*pVal = get_bstr_t(m_strPostEditAction).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI45217");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkflowDefinition::put_PostEditAction(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_strPostEditAction = asString(newVal);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI45218");

	return S_OK;
}
