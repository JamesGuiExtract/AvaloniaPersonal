// Token.cpp : Implementation of CToken
#include "stdafx.h"
#include "UCLIDCOMUtils.h"
#include "Token.h"

#include <UCLIDException.h>
#include <COMUtils.h>

//-------------------------------------------------------------------------------------------------
// CToken
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CToken::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_IToken
	};

	for (int i = 0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
		{
			return S_OK;
		}
	}

	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
STDMETHODIMP CToken::InitToken(long nTokenStart, long nTokenEnd, BSTR strName, BSTR strValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		// store the token start/end position
		m_nTokenStartPos = nTokenStart;
		m_nTokenEndPos = nTokenEnd;

		// Store the strings
		m_strName = asString( strName );
		m_strValue = asString( strValue );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03849")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CToken::GetTokenInfo(long *pnTokenStart, long *pnTokenEnd, BSTR *pName, BSTR *pValue)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI22968", pnTokenStart != __nullptr);
		ASSERT_ARGUMENT("ELI22969", pnTokenEnd != __nullptr);

		// Provide the start/end points
		*pnTokenStart = m_nTokenStartPos;
		*pnTokenEnd = m_nTokenEndPos;

		// Provide the optional strings
		if (pName != __nullptr)
		{
			*pName = _bstr_t( m_strName.c_str() ).copy();
		}

		if (pValue != __nullptr)
		{
			*pValue = _bstr_t( m_strValue.c_str() ).copy();
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI03850")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CToken::get_StartPosition(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI22971", pVal != __nullptr);

		*pVal = m_nTokenStartPos;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05449")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CToken::put_StartPosition(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_nTokenStartPos = newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05451")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CToken::get_EndPosition(long *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI22972", pVal != __nullptr);

		*pVal = m_nTokenEndPos;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05452")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CToken::put_EndPosition(long newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_nTokenEndPos = newVal;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05453")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CToken::get_Name(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI22973", pVal != __nullptr);

		*pVal = _bstr_t(m_strName.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05454")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CToken::put_Name(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_strName = asString( newVal );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05455")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CToken::get_Value(BSTR *pVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		ASSERT_ARGUMENT("ELI22974", pVal != __nullptr);

		*pVal = _bstr_t(m_strValue.c_str()).copy();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05456")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CToken::put_Value(BSTR newVal)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState())

	try
	{
		m_strValue = asString( newVal );
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI05457")

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CToken::GetStartAndEndPosition(long *plStartPos, long *plEndPos)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// Check arguments
		ASSERT_ARGUMENT("ELI25733", plStartPos != __nullptr);
		ASSERT_ARGUMENT("ELI25734", plEndPos != __nullptr);

		// Set the return values
		*plStartPos = m_nTokenStartPos;
		*plEndPos = m_nTokenEndPos;

		return S_OK;
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI25735");
}
//-------------------------------------------------------------------------------------------------
