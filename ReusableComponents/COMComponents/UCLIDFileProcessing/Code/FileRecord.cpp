// FileRecord.cpp : Implementation of CFileRecord

#include "stdafx.h"
#include "FileRecord.h"
#include <UCLIDException.h>
#include <cpputil.h>
#include <ComUtils.h>

using namespace std;
//-------------------------------------------------------------------------------------------------
// CFileRecord
//-------------------------------------------------------------------------------------------------
CFileRecord::CFileRecord() :
m_strName(""),
m_lActionID(0),
m_lFileID(0),
m_llFileSize(0),
m_lPages(0),
m_ePriority((UCLID_FILEPROCESSINGLib::EFilePriority)kPriorityDefault)
{
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileRecord::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IFileRecord
	};

	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// IFileRecord
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileRecord::get_FileID(LONG* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		*pVal = m_lFileID;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14183"); 

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileRecord::put_FileID(LONG newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_lFileID = newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14184");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileRecord::get_Name(BSTR* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		*pVal = _bstr_t(m_strName.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14185");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileRecord::put_Name(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_strName = asString(newVal);
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14186");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileRecord::get_FileSize(LONGLONG* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		*pVal = m_llFileSize;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14187");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileRecord::put_FileSize(LONGLONG newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_llFileSize = newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14188");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileRecord::get_Pages(LONG* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		*pVal = m_lPages;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14189");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileRecord::put_Pages(LONG newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_lPages = newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI14190");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileRecord::get_ActionID(LONG* pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI26739", pVal != __nullptr);

		*pVal = m_lActionID;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26740");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileRecord::put_ActionID(LONG newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_lActionID = newVal;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26741");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileRecord::GetFileData(LONG *plFileID, LONG *plActionID, BSTR *pbstrFileName,
									  LONGLONG *pllFileSize, LONG *plPages,
									  EFilePriority *pePriority)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check the arguments
		ASSERT_ARGUMENT("ELI26744", plFileID != __nullptr);
		ASSERT_ARGUMENT("ELI26745", plActionID != __nullptr);
		ASSERT_ARGUMENT("ELI26746", pbstrFileName != __nullptr);
		ASSERT_ARGUMENT("ELI26747", pllFileSize != __nullptr);
		ASSERT_ARGUMENT("ELI26748", plPages != __nullptr);
		ASSERT_ARGUMENT("ELI27649", pePriority != __nullptr);

		// Copy the values
		*plFileID = m_lFileID;
		*plActionID = m_lActionID;
		*pbstrFileName = _bstr_t(m_strName.c_str()).Detach();
		*pllFileSize = m_llFileSize;
		*plPages = m_lPages;
		*pePriority = (EFilePriority) m_ePriority;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26749");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileRecord::SetFileData(LONG lFileID, LONG lActionID, BSTR bstrFileName,
									  LONGLONG llFileSize, LONG lPages, EFilePriority ePriority)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Copy the values
		m_lFileID = lFileID;
		m_lActionID = lActionID;
		m_strName = asString(bstrFileName);
		m_llFileSize = llFileSize;
		m_lPages = lPages;
		m_ePriority = (UCLID_FILEPROCESSINGLib::EFilePriority) ePriority;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI26883");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileRecord::get_Priority(EFilePriority* pePriority)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		ASSERT_ARGUMENT("ELI27651", pePriority != __nullptr);

		*pePriority = (EFilePriority) m_ePriority;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27652");
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CFileRecord::put_Priority(EFilePriority ePriority)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		m_ePriority = (UCLID_FILEPROCESSINGLib::EFilePriority) ePriority;
		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI27653");
}
//-------------------------------------------------------------------------------------------------
