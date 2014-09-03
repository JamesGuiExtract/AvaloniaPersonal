// WorkItemRecord.cpp : Implementation of CWorkItemRecord

#include "stdafx.h"
#include "WorkItemRecord.h"
#include <UCLIDException.h>
#include <cpputil.h>
#include <ComUtils.h>

using namespace std;
//-------------------------------------------------------------------------------------------------
// CWorkItemRecord
//-------------------------------------------------------------------------------------------------
CWorkItemRecord::CWorkItemRecord() :
m_nWorkItemID(0),
m_nWorkItemGroupID(0),
m_eWorkItemStatus(kWorkUnitPending),
m_strInput(""),
m_strOutput(""),
m_strUPI("")
{
}

CWorkItemRecord::~CWorkItemRecord()
{
	try
	{
		
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI37186");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkItemRecord::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IWorkItemRecord
	};

	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IWorkItemRecord
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkItemRecord::get_WorkItemID(LONG* pnWorkItemID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		*pnWorkItemID = m_nWorkItemID;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37023"); 

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkItemRecord::put_WorkItemID(LONG nWorkItemID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_nWorkItemID = nWorkItemID;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37024");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkItemRecord::get_WorkItemGroupID(LONG* pnWorkItemGroupID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		*pnWorkItemGroupID = m_nWorkItemGroupID;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37025"); 

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkItemRecord::put_WorkItemGroupID(LONG nWorkItemGroupID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_nWorkItemGroupID = nWorkItemGroupID;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37026");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkItemRecord::get_Status(EWorkItemStatus *pStatus)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		*pStatus = m_eWorkItemStatus;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37027");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkItemRecord::put_Status(EWorkItemStatus Status)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_eWorkItemStatus = Status;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37028");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkItemRecord::get_Input(BSTR *pInput)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		*pInput = _bstr_t(m_strInput.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37029");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkItemRecord::put_Input(BSTR Input)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_strInput = asString(Input);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37030");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkItemRecord::get_Output(BSTR *pOutput)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		*pOutput = _bstr_t(m_strOutput.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37031");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkItemRecord::put_Output(BSTR Output)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_strOutput = asString(Output);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37032");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkItemRecord::get_UPI(BSTR *pUPI)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		*pUPI = _bstr_t(m_strUPI.c_str()).Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37033");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkItemRecord::put_UPI(BSTR UPI)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_strUPI = asString(UPI);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37034");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkItemRecord::get_StringizedException(BSTR *pStringizedException)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		*pStringizedException = _bstr_t(m_strStringizedException.c_str()).Detach();
        
        return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37141");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkItemRecord::put_StringizedException(BSTR StringizedException)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_strStringizedException = asString(StringizedException);

	    return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37142");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkItemRecord::get_FileName(BSTR *pFileName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		*pFileName = _bstr_t(m_strFileName.c_str()).Detach();
	    
        return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37143");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkItemRecord::put_FileName(BSTR FileName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_strFileName = asString(FileName);
        
        return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37144");

}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkItemRecord::get_BinaryOutput(IUnknown **ppBinaryOutput)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		IPersistStreamPtr ipObj = m_ipBinaryOutput;
		*ppBinaryOutput = (ipObj != __nullptr) ? ipObj.Detach(): __nullptr;

        return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37173");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkItemRecord::put_BinaryOutput(IUnknown *pBinaryOutput)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
        m_ipBinaryOutput = pBinaryOutput;
		
		// Throw exception if the object passed in does not implement IPersistStream.
		if ( pBinaryOutput != __nullptr && m_ipBinaryOutput == __nullptr)
		{
			UCLIDException ue("ELI37217", "WorkItem BinaryOutput value should implement IPersistStream");
			throw ue;
		}

        return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37174");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkItemRecord::get_BinaryInput(IUnknown **ppBinaryInput)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		IPersistStreamPtr ipObj = m_ipBinaryInput;
		*ppBinaryInput = (ipObj != __nullptr) ? ipObj.Detach(): __nullptr;

        return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37212");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkItemRecord::put_BinaryInput(IUnknown *pBinaryInput)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_ipBinaryInput = pBinaryInput;
		
		// Throw exception if the object passed in does not implement IPersistStream.
		if ( pBinaryInput != __nullptr && m_ipBinaryInput == __nullptr)
		{
			UCLIDException ue("ELI37217", "WorkItem BinaryInput value should implement IPersistStream");
			throw ue;
		}

        return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37213");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkItemRecord::get_FileID(long* pnFileID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		*pnFileID = m_nFileID;
	
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37273"); 
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CWorkItemRecord::put_FileID(long nFileID)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_nFileID = nFileID;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI37274");
}