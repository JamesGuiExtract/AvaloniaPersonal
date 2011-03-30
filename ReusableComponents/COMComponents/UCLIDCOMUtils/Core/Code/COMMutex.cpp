// COMMutex.cpp : Implementation of CCOMMutex

#include "stdafx.h"
#include "COMMutex.h"
#include "COMUtils.h"

#include <UCLIDException.h>
#include <MutexUtils.h>

//-------------------------------------------------------------------------------------------------
// CCOMMutex
//-------------------------------------------------------------------------------------------------
CCOMMutex::CCOMMutex()
:	m_strMutexName(""), m_pMutex(NULL)
{
}
//-------------------------------------------------------------------------------------------------
CCOMMutex::~CCOMMutex()
{
	try
	{
		if ( m_pMutex != __nullptr )
		{
			delete m_pMutex;
		}
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16507");
}

//-------------------------------------------------------------------------------------------------
// ISupportsErrorInfo
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCOMMutex::InterfaceSupportsErrorInfo(REFIID riid)
{
	static const IID* arr[] = 
	{
		&IID_ICOMMutex
	};

	for (int i=0; i < sizeof(arr) / sizeof(arr[0]); i++)
	{
		if (InlineIsEqualGUID(*arr[i],riid))
			return S_OK;
	}
	return S_FALSE;
}

//-------------------------------------------------------------------------------------------------
// ICOMMutex
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCOMMutex::CreateNamed(BSTR bstrMutexName)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());
	try
	{
		if ( m_pMutex != __nullptr )
		{
			// if this name is different from a previous call throw an exception
			if ( m_strMutexName != asString(bstrMutexName) )
			{
				UCLIDException ue("ELI13248", "Mutex already has a name!" );
				throw ue;
			}
		}
		else
		{
			m_strMutexName = asString (bstrMutexName);
			if ( m_strMutexName != "" )
			{
				m_pMutex = getGlobalNamedMutex(m_strMutexName);
				ASSERT_RESOURCE_ALLOCATION("ELI13245", m_pMutex != __nullptr );
			}
			else
			{
				UCLIDException ue ( "ELI13249", "Mutex name must be a non-empty string!");
				throw ue;
			}
		}
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13242");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCOMMutex::Acquire(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// if mutex object has not been initialized throw an exception
		ASSERT_RESOURCE_ALLOCATION("ELI13244", m_pMutex != __nullptr );

		m_pMutex->Lock();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13243");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
STDMETHODIMP CCOMMutex::ReleaseNamedMutex(void)
{
	AFX_MANAGE_STATE(AfxGetStaticModuleState());

	try
	{
		// if mutex object has not been initialize throw an exception
		ASSERT_RESOURCE_ALLOCATION("ELI13246", m_pMutex != __nullptr );

		m_pMutex->Unlock();
	}
	CATCH_ALL_AND_RETURN_AS_COM_ERROR("ELI13247");

	return S_OK;
}
//-------------------------------------------------------------------------------------------------
