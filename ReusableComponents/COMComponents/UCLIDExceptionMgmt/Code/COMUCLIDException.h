// COMUCLIDException.h : Declaration of the CCOMUCLIDException

#pragma once

#include "resource.h"       // main symbols

#include <memory>

class UCLIDException;

/////////////////////////////////////////////////////////////////////////////
// CCOMUCLIDException
class ATL_NO_VTABLE CCOMUCLIDException : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CCOMUCLIDException, &CLSID_COMUCLIDException>,
	public ISupportErrorInfo,
	public IDispatchImpl<ICOMUCLIDException, &IID_ICOMUCLIDException, &LIBID_UCLID_EXCEPTIONMGMTLib>
{
private:
	std::unique_ptr<UCLIDException> m_upException;

public:
	CCOMUCLIDException();
	~CCOMUCLIDException();

DECLARE_REGISTRY_RESOURCEID(IDR_COMUCLIDEXCEPTION)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CCOMUCLIDException)
	COM_INTERFACE_ENTRY(ICOMUCLIDException)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, ICOMUCLIDException)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// ICOMUCLIDException
public:
	STDMETHOD(SaveTo)(/*[in]*/ BSTR strFullFileName, /*[in]*/ VARIANT_BOOL bAppend);
	STDMETHOD(Log)();
	STDMETHOD(GetTopText)(/*[out, retval]*/ BSTR *pbstrText);
	STDMETHOD(GetTopELICode)(/*[out, retval]*/ BSTR *pbstrCode);
	STDMETHOD(AsStringizedByteStream)(/*[out, retval]*/ BSTR *pbstrData);
	STDMETHOD(CreateFromString)(/*[in]*/ BSTR bstrELICode, /*[in]*/ BSTR bstrData);
	STDMETHOD(AddDebugInfo)(/*[in]*/ BSTR bstrKeyName, /*[in]*/ BSTR bstrStringizedValue);
	STDMETHOD(Display)();
	STDMETHOD(CreateWithInnerException)(BSTR strELICode, BSTR strText, 
		ICOMUCLIDException *pInnerException);
	STDMETHOD(AddStackTraceEntry)(BSTR strStackTrace);
	STDMETHOD(GetStackTraceEntry)(long nIndex, BSTR *pstrStackTrace);
	STDMETHOD(GetInnerException)(ICOMUCLIDException **ppInnerException);
	STDMETHOD(GetStackTraceCount)(long *pnIndex);
	STDMETHOD(GetDebugInfo)(long nIndex, BSTR* pbstrKeyName, BSTR* pbstrStringizedValue);
	STDMETHOD(GetDebugInfoCount)(long *pnIndex);
	STDMETHOD(GetApplicationName)(BSTR* pbstrAppName);
	STDMETHOD(LogWithSpecifiedInfo)(BSTR bstrMachineName, BSTR bstrUserName, long nDateTimeUtc,
		long nPid, BSTR bstrAppName);

private:
	// Method creates exception with the data from the COMUCLIDException object passed
	// NOTE: Memory allocated with this method must be released using delete.
	UCLIDException *createUCLIDException(UCLID_EXCEPTIONMGMTLib::ICOMUCLIDExceptionPtr ipException);
};

