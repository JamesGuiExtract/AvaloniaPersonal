
#include "stdafx.h"
#include "ErrorInfo.h"
#include "UCLIDException.h"

#include <comdef.h>

using namespace std;

ErrorInfo::ErrorInfo(const string& strDescription)
:m_ulRefCount(0), m_strDescription(strDescription)
{
}

ErrorInfo::~ErrorInfo()
{
	try
	{
	}
	CATCH_AND_LOG_ALL_EXCEPTIONS("ELI16381");
}

STDMETHODIMP ErrorInfo::QueryInterface(REFIID riid, void **ppv)
{
	if (riid == IID_IUnknown)
	{
		*ppv = static_cast<IUnknown *>(this);
	}
	else if (riid == IID_IErrorInfo)
	{
		*ppv = static_cast<IErrorInfo *>(this);
	}
	else
	{
		*ppv = __nullptr;
	}
	
	if (*ppv != __nullptr)
	{
		static_cast<IUnknown *>(*ppv)->AddRef();
		return S_OK;
	}
	else
		return E_NOINTERFACE;
}

ULONG ErrorInfo::AddRef()
{
	InterlockedIncrement(reinterpret_cast<LPLONG>(&m_ulRefCount));
	return m_ulRefCount;
}

ULONG ErrorInfo::Release()
{
	if (!InterlockedDecrement(reinterpret_cast<LPLONG>(&m_ulRefCount)))
	{
		delete this;
		return 0;
	}

	return m_ulRefCount;
}

STDMETHODIMP ErrorInfo::GetGUID(/* [out] */ GUID __RPC_FAR *pGUID)
{
	pGUID;
	return S_OK;
}
    
STDMETHODIMP ErrorInfo::GetSource(/* [out] */ BSTR __RPC_FAR *pBstrSource)
{
	pBstrSource;
	return S_OK;
}
    
STDMETHODIMP ErrorInfo::GetDescription(/* [out] */ BSTR __RPC_FAR *pBstrDescription)
{
	*pBstrDescription = _bstr_t(m_strDescription.c_str()).Detach();
	return S_OK;
}
    
STDMETHODIMP ErrorInfo::GetHelpFile(/* [out] */ BSTR __RPC_FAR *pBstrHelpFile)
{
	pBstrHelpFile;
	return S_OK;
}
    
STDMETHODIMP ErrorInfo::GetHelpContext(/* [out] */ DWORD __RPC_FAR *pdwHelpContext)
{
	pdwHelpContext;
	return S_OK;
}
