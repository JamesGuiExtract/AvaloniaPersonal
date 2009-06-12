// Token.h : Declaration of the CToken

#pragma once

#include "resource.h"       // main symbols

#include <string>

/////////////////////////////////////////////////////////////////////////////
// CToken
class ATL_NO_VTABLE CToken : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CToken, &CLSID_Token>,
	public ISupportErrorInfo,
	public IDispatchImpl<IToken, &IID_IToken, &LIBID_UCLID_COMUTILSLib>
{
public:
	CToken()
	{
	}

DECLARE_REGISTRY_RESOURCEID(IDR_TOKEN)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CToken)
	COM_INTERFACE_ENTRY(IToken)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IToken)
END_COM_MAP()

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IToken
	STDMETHOD(GetTokenInfo)(/*[in, out]*/ long* pnTokenStart, /*[in, out]*/ long* pnTokenEnd,
		/*[in, out]*/ BSTR *pName, /*[in, out]*/ BSTR *pValue);
	STDMETHOD(InitToken)(/*[in]*/ long nTokenStart, /*[in]*/ long nTokenEnd, 
		/*[in]*/ BSTR strName, /*[in]*/ BSTR strValue);
	STDMETHOD(get_Value)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_Value)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_Name)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_Name)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_EndPosition)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_EndPosition)(/*[in]*/ long newVal);
	STDMETHOD(get_StartPosition)(/*[out, retval]*/ long  *pVal);
	STDMETHOD(put_StartPosition)(/*[in]*/ long  newVal);
	STDMETHOD(GetStartAndEndPosition)(long* plStartPos, long* plEndPos);

private:
	long		m_nTokenStartPos;
	long		m_nTokenEndPos;
	std::string	m_strName;
	std::string	m_strValue;

};

