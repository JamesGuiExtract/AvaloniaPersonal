// ActionStatistics.cpp : Implementation of CActionStatistics

#include "stdafx.h"
#include "ActionStatistics.h"
#include <UCLIDException.h>



//-------------------------------------------------------------------------------------------------
// CActionStatistics
//-------------------------------------------------------------------------------------------------
CActionStatistics::CActionStatistics() :
m_nNumDocuments(0),
m_nNumDocumentsPending(0),
m_nNumDocumentsComplete(0),
m_nNumDocumentsFailed(0),
m_nNumDocumentsSkipped(0),
m_nNumPages(0),
m_nNumPagesPending(0),
m_nNumPagesComplete(0),
m_nNumPagesFailed(0),
m_nNumPagesSkipped(0),
m_llNumBytes(0),
m_llNumBytesPending(0),
m_llNumBytesComplete(0),
m_llNumBytesFailed(0),
m_llNumBytesSkipped(0)
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
		ASSERT_ARGUMENT("ELI26808", pVal != NULL);

		*pVal = m_nNumDocuments;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14050")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::put_NumDocuments(/*[in]*/ long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_nNumDocuments = newVal;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14051")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::get_NumDocumentsPending(/*[out, retval]*/ long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI30725", pVal != NULL);

		*pVal = m_nNumDocumentsPending;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI30726")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::put_NumDocumentsPending(/*[in]*/ long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_nNumDocumentsPending = newVal;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI30727")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::get_NumDocumentsComplete(/*[out, retval]*/ long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI26809", pVal != NULL);

		*pVal = m_nNumDocumentsComplete;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14052")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::put_NumDocumentsComplete(/*[in]*/ long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_nNumDocumentsComplete = newVal;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14053")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::get_NumDocumentsFailed(/*[out, retval]*/ long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI26810", pVal != NULL);

		*pVal = m_nNumDocumentsFailed;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14054")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::put_NumDocumentsFailed(/*[in]*/ long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_nNumDocumentsFailed = newVal;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14055")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::get_NumDocumentsSkipped(/*[out, retval]*/ long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI26811", pVal != NULL);

		*pVal = m_nNumDocumentsSkipped;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26812")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::put_NumDocumentsSkipped(/*[in]*/ long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_nNumDocumentsSkipped = newVal;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26813")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::get_NumPages(/*[out, retval]*/ long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI26819", pVal != NULL);

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

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14057")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::get_NumPagesPending(/*[out, retval]*/ long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI30728", pVal != NULL);

		*pVal = m_nNumPagesPending;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI30729")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::put_NumPagesPending(/*[in]*/ long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_nNumPagesPending = newVal;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI30730")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::get_NumPagesComplete(/*[out, retval]*/ long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI26818", pVal != NULL);

		*pVal = m_nNumPagesComplete;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14058")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::put_NumPagesComplete(/*[in]*/ long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_nNumPagesComplete = newVal;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14059")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::get_NumPagesFailed(/*[out, retval]*/ long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI26817", pVal != NULL);

		*pVal = m_nNumPagesFailed;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14060")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::put_NumPagesFailed(/*[in]*/ long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_nNumPagesFailed = newVal;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14061")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::get_NumPagesSkipped(/*[out, retval]*/ long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI26820", pVal != NULL);

		*pVal = m_nNumPagesSkipped;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26821")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::put_NumPagesSkipped(/*[in]*/ long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_nNumPagesSkipped = newVal;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26822")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::get_NumBytes(/*[out, retval]*/ LONGLONG *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI26816", pVal != NULL);

		*pVal = m_llNumBytes;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14062")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::put_NumBytes(/*[in]*/ LONGLONG newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_llNumBytes = newVal;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14063")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::get_NumBytesPending(/*[out, retval]*/ LONGLONG *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI30731", pVal != NULL);

		*pVal = m_llNumBytesPending;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI30732")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::put_NumBytesPending(/*[in]*/ LONGLONG newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_llNumBytesPending = newVal;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI30733")
}//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::get_NumBytesComplete(/*[out, retval]*/ LONGLONG *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI26815", pVal != NULL);

		*pVal = m_llNumBytesComplete;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14064")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::put_NumBytesComplete(/*[in]*/ LONGLONG newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_llNumBytesComplete = newVal;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14065")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::get_NumBytesFailed(/*[out, retval]*/ LONGLONG *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI26814", pVal != NULL);

		*pVal = m_llNumBytesFailed;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14066")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::put_NumBytesFailed(/*[in]*/ LONGLONG newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_llNumBytesFailed = newVal;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14067")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::get_NumBytesSkipped(/*[out, retval]*/ LONGLONG *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI26823", pVal != NULL);

		*pVal = m_llNumBytesSkipped;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26824")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::put_NumBytesSkipped(/*[in]*/ LONGLONG newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_llNumBytesSkipped = newVal;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26825")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::GetAllStatistics(long *plNumDocs, long *plNumDocsPending, 
		long* plNumDocsComplete, long* plNumDocsFailed, long* plNumDocsSkipped, 
		long* plNumPages, long* plNumPagesPending, long* plNumPagesComplete, 
		long* plNumPagesFailed, long* plNumPagesSkipped, LONGLONG* pllNumBytes, 
		LONGLONG* pllNumBytesPending, LONGLONG* pllNumBytesComplete, 
		LONGLONG* pllNumBytesFailed, LONGLONG* pllNumBytesSkipped)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Copy the statistics data to non null parameters
		if (plNumDocs != NULL)
		{
			*plNumDocs = m_nNumDocuments;
		}
		if (plNumDocsPending != NULL)
		{
			*plNumDocsPending = m_nNumDocumentsPending;
		}
		if (plNumDocsComplete != NULL)
		{
			*plNumDocsComplete = m_nNumDocumentsComplete;
		}
		if (plNumDocsFailed != NULL)
		{
			*plNumDocsFailed = m_nNumDocumentsFailed;
		}
		if (plNumDocsSkipped != NULL)
		{
			*plNumDocsSkipped = m_nNumDocumentsSkipped;
		}
		if (plNumPages != NULL)
		{
			*plNumPages = m_nNumPages;
		}
		if (plNumPagesPending != NULL)
		{
			*plNumPagesPending = m_nNumPagesPending;
		}
		if (plNumPagesComplete != NULL)
		{
			*plNumPagesComplete = m_nNumPagesComplete;
		}
		if (plNumPagesFailed != NULL)
		{
			*plNumPagesFailed = m_nNumPagesFailed;
		}
		if (plNumPagesSkipped != NULL)
		{
			*plNumPagesSkipped = m_nNumPagesSkipped;
		}
		if (pllNumBytes != NULL)
		{
			*pllNumBytes = m_llNumBytes;
		}
		if (pllNumBytes != NULL)
		{
			*pllNumBytesPending = m_llNumBytesPending;
		}
		if (pllNumBytesComplete != NULL)
		{
			*pllNumBytesComplete = m_llNumBytesComplete;
		}
		if (pllNumBytesFailed != NULL)
		{
			*pllNumBytesFailed = m_llNumBytesFailed;
		}
		if (pllNumBytesSkipped != NULL)
		{
			*pllNumBytesSkipped = m_llNumBytesSkipped;
		}

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26841")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::SetAllStatistics(long lNumDocs, long lNumDocsPending, 
		long lNumDocsComplete, long lNumDocsFailed, long lNumDocsSkipped, long lNumPages, 
		long lNumPagesPending,  long lNumPagesComplete, long lNumPagesFailed, long lNumPagesSkipped, 
		LONGLONG llNumBytes, LONGLONG llNumBytesPending, LONGLONG llNumBytesComplete, 
		LONGLONG llNumBytesFailed, LONGLONG llNumBytesSkipped)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Copy the statistics data
		m_nNumDocuments = lNumDocs;
		m_nNumDocumentsPending = lNumDocsPending;
		m_nNumDocumentsComplete = lNumDocsComplete;
		m_nNumDocumentsFailed = lNumDocsFailed;
		m_nNumDocumentsSkipped = lNumDocsSkipped;
		m_nNumPages = lNumPages;
		m_nNumPagesPending = lNumPagesPending;
		m_nNumPagesComplete = lNumPagesComplete;
		m_nNumPagesFailed = lNumPagesFailed;
		m_nNumPagesSkipped = lNumPagesSkipped;
		m_llNumBytes = llNumBytes;
		m_llNumBytesPending = llNumBytesPending;
		m_llNumBytesComplete = llNumBytesComplete;
		m_llNumBytesFailed = llNumBytesFailed;
		m_llNumBytesSkipped = llNumBytesSkipped;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26862");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::GetTotals(long *plNumDocs, long *plNumPages, LONGLONG *pllNumBytes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check arguments
		ASSERT_ARGUMENT("ELI26842", plNumDocs != NULL);
		ASSERT_ARGUMENT("ELI26843", plNumPages != NULL);
		ASSERT_ARGUMENT("ELI26844", pllNumBytes != NULL);

		*plNumDocs = m_nNumDocuments;
		*plNumPages = m_nNumPages;
		*pllNumBytes = m_llNumBytes;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26845")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::SetTotals(long lNumDocs, long lNumPages, LONGLONG llNumBytes)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_nNumDocuments = lNumDocs;
		m_nNumPages = lNumPages;
		m_llNumBytes = llNumBytes;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26846")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::GetComplete(long *plNumDocsComplete, long *plNumPagesComplete,
											 LONGLONG *pllNumBytesComplete)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check arguments
		ASSERT_ARGUMENT("ELI26847", plNumDocsComplete != NULL);
		ASSERT_ARGUMENT("ELI26848", plNumPagesComplete != NULL);
		ASSERT_ARGUMENT("ELI26849", pllNumBytesComplete != NULL);

		*plNumDocsComplete = m_nNumDocumentsComplete;
		*plNumPagesComplete = m_nNumPagesComplete;
		*pllNumBytesComplete = m_llNumBytesComplete;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26850")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::SetComplete(long lNumDocsComplete, long lNumPagesComplete,
											 LONGLONG llNumBytesComplete)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_nNumDocumentsComplete = lNumDocsComplete;
		m_nNumPagesComplete = lNumPagesComplete;
		m_llNumBytesComplete = llNumBytesComplete;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26851")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::GetFailed(long *plNumDocsFailed, long *plNumPagesFailed,
											 LONGLONG *pllNumBytesFailed)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check arguments
		ASSERT_ARGUMENT("ELI26852", plNumDocsFailed != NULL);
		ASSERT_ARGUMENT("ELI26853", plNumPagesFailed != NULL);
		ASSERT_ARGUMENT("ELI26854", pllNumBytesFailed != NULL);

		*plNumDocsFailed = m_nNumDocumentsFailed;
		*plNumPagesFailed = m_nNumPagesFailed;
		*pllNumBytesFailed = m_llNumBytesFailed;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26855")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::SetFailed(long lNumDocsFailed, long lNumPagesFailed,
											 LONGLONG llNumBytesFailed)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_nNumDocumentsFailed = lNumDocsFailed;
		m_nNumPagesFailed = lNumPagesFailed;
		m_llNumBytesFailed = llNumBytesFailed;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26856")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::GetSkipped(long *plNumDocsSkipped, long *plNumPagesSkipped,
											 LONGLONG *pllNumBytesSkipped)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check arguments
		ASSERT_ARGUMENT("ELI26857", plNumDocsSkipped != NULL);
		ASSERT_ARGUMENT("ELI26858", plNumPagesSkipped != NULL);
		ASSERT_ARGUMENT("ELI26859", pllNumBytesSkipped != NULL);

		*plNumDocsSkipped = m_nNumDocumentsSkipped;
		*plNumPagesSkipped = m_nNumPagesSkipped;
		*pllNumBytesSkipped = m_llNumBytesSkipped;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26860")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::SetSkipped(long lNumDocsSkipped, long lNumPagesSkipped,
											 LONGLONG llNumBytesSkipped)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_nNumDocumentsSkipped = lNumDocsSkipped;
		m_nNumPagesSkipped = lNumPagesSkipped;
		m_llNumBytesSkipped = llNumBytesSkipped;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26861")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::GetPending(long *plNumDocsPending, long *plNumPagesPending,
											 LONGLONG *pllNumBytesPending)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check arguments
		ASSERT_ARGUMENT("ELI30737", plNumDocsPending != NULL);
		ASSERT_ARGUMENT("ELI30738", plNumPagesPending != NULL);
		ASSERT_ARGUMENT("ELI30739", pllNumBytesPending != NULL);

		*plNumDocsPending = m_nNumDocumentsPending;
		*plNumPagesPending = m_nNumPagesPending;
		*pllNumBytesPending = m_llNumBytesPending;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI30740")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::SetPending(long lNumDocsPending, long lNumPagesPending,
											 LONGLONG llNumBytesPending)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_nNumDocumentsPending = lNumDocsPending;
		m_nNumPagesPending = lNumPagesPending;
		m_llNumBytesPending = llNumBytesPending;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI30741")
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

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14069")
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CActionStatistics::raw_CopyFrom(IUnknown * pObject)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		UCLID_FILEPROCESSINGLib::IActionStatisticsPtr ipActionStats(pObject);
		ASSERT_RESOURCE_ALLOCATION("ELI14031", ipActionStats != NULL);

		// Get the stats from the other statistics object
		ipActionStats->GetAllStatistics(&m_nNumDocuments, &m_nNumDocumentsPending, 
			&m_nNumDocumentsComplete, &m_nNumDocumentsFailed, &m_nNumDocumentsSkipped, 
			&m_nNumPages, &m_nNumPagesPending, &m_nNumPagesComplete,
			&m_nNumPagesFailed, &m_nNumPagesSkipped, &m_llNumBytes, &m_llNumBytesPending,
			&m_llNumBytesComplete, &m_llNumBytesFailed, &m_llNumBytesSkipped);

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14070")
}
//-------------------------------------------------------------------------------------------------
