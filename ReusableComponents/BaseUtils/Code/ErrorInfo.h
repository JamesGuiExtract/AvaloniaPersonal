
#pragma once

#include <string>

class ErrorInfo : public IErrorInfo
{
private:
	ULONG m_ulRefCount;
	std::string m_strDescription;

public:
	ErrorInfo(const std::string& strDescription);
	~ErrorInfo();
	STDMETHOD (QueryInterface)(REFIID riid, void **ppv);
	STDMETHOD_ (ULONG, AddRef)();
	STDMETHOD_ (ULONG, Release)();
    STDMETHOD (GetGUID)(GUID *pGUID);
    STDMETHOD (GetSource)(BSTR *pBstrSource);
    STDMETHOD (GetDescription)(BSTR *pBstrDescription);
    STDMETHOD (GetHelpFile)(BSTR *pBstrHelpFile);
    STDMETHOD (GetHelpContext)(DWORD *pdwHelpContext);
};
