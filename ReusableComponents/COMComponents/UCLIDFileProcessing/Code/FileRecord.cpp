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
CFileRecord::CFileRecord()
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
