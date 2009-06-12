// ActionStatistics.cpp : Implementation of CActionStatistics

#include "stdafx.h"
#include "ActionStatistics.h"
#include <UCLIDException.h>



//-------------------------------------------------------------------------------------------------
// CActionStatistics
//-------------------------------------------------------------------------------------------------
CActionStatistics::CActionStatistics()
{
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IActionStatistics
	};

	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IActionStatistics
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::get_NumDocuments(/*[out, retval]*/ long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		*pVal = m_nNumDocuments;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14050")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::put_NumDocuments(/*[in]*/ long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_nNumDocuments = newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14051")

	return S_OK;}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::get_NumDocumentsComplete(/*[out, retval]*/ long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		*pVal = m_nNumDocumentsComplete;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14052")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::put_NumDocumentsComplete(/*[in]*/ long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_nNumDocumentsComplete = newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14053")

	return S_OK;}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::get_NumDocumentsFailed(/*[out, retval]*/ long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		*pVal = m_nNumDocumentsFailed;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14054")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::put_NumDocumentsFailed(/*[in]*/ long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_nNumDocumentsFailed = newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14055")

	return S_OK;}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::get_NumPages(/*[out, retval]*/ long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		*pVal = m_nNumPages;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14056")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::put_NumPages(/*[in]*/ long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_nNumPages = newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14057")

	return S_OK;}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::get_NumPagesComplete(/*[out, retval]*/ long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		*pVal = m_nNumPagesComplete;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14058")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::put_NumPagesComplete(/*[in]*/ long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_nNumPagesComplete = newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14059")

	return S_OK;}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::get_NumPagesFailed(/*[out, retval]*/ long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		*pVal = m_nNumPagesFailed;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14060")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::put_NumPagesFailed(/*[in]*/ long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_nNumPagesFailed = newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14061")

	return S_OK;}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::get_NumBytes(/*[out, retval]*/ LONGLONG *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		*pVal = m_llNumBytes;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14062")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::put_NumBytes(/*[in]*/ LONGLONG newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_llNumBytes = newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14063")

	return S_OK;}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::get_NumBytesComplete(/*[out, retval]*/ LONGLONG *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		*pVal = m_llNumBytesComplete;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14064")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::put_NumBytesComplete(/*[in]*/ LONGLONG newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_llNumBytesComplete = newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14065")

	return S_OK;}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::get_NumBytesFailed(/*[out, retval]*/ LONGLONG *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		*pVal = m_llNumBytesFailed;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14066")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::put_NumBytesFailed(/*[in]*/ LONGLONG newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_llNumBytesFailed = newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14067")

	return S_OK;
}

//-------------------------------------------------------------------------------------------------
// ICopyableObject Methods
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::raw_Clone(IUnknown * * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ICopyableObjectPtr ipObjCopy;
		ipObjCopy.CreateInstance( CLSID_ActionStatistics );
		ASSERT_RESOURCE_ALLOCATION("ELI14068", ipObjCopy != NULL );

		IUnknownPtr ipUnk = this;
		ipObjCopy->CopyFrom(ipUnk);

		// Return the new object to the caller
		*pObject = ipObjCopy.Detach();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14069")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::raw_CopyFrom(IUnknown * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_FILEPROCESSINGLib::IActionStatisticsPtr ipActionStats(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI14031", ipActionStats != NULL);

		m_nNumDocuments = ipActionStats->NumDocuments;
		m_nNumDocumentsComplete = ipActionStats->NumDocumentsComplete;
		m_nNumDocumentsFailed = ipActionStats->NumDocumentsFailed;
		m_nNumPages = ipActionStats->NumPages;
		m_nNumPagesComplete = ipActionStats->NumPagesComplete;
		m_nNumPagesFailed = ipActionStats->NumPagesFailed;
		m_llNumBytes = ipActionStats->NumBytes;
		m_llNumBytesComplete = ipActionStats->NumBytesComplete;
		m_llNumBytesFailed = ipActionStats->NumBytesFailed;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14070")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
