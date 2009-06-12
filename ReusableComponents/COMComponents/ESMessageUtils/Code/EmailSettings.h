// EmailSettings.h : Declaration of the CEmailSettings

#pragma once

#include "resource.h"       // main symbols
#include <string>

/////////////////////////////////////////////////////////////////////////////
// CEmailSettings
class ATL_NO_VTABLE CEmailSettings : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CEmailSettings, &CLSID_EmailSettings>,
	public ISupportErrorInfo,
	public IDispatchImpl<IEmailSettings, &IID_IEmailSettings, &LIBID_ESMESSAGEUTILSLib>,
	public IDispatchImpl<IObjectSettings, &IID_IObjectSettings, &LIBID_UCLID_COMUTILSLib>,
	public IDispatchImpl<IObjectUserInterface, &IID_IObjectUserInterface, &LIBID_UCLID_COMUTILSLib>
{
public:
	CEmailSettings();

DECLARE_REGISTRY_RESOURCEID(IDR_EMAILSETTINGS)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CEmailSettings)
	COM_INTERFACE_ENTRY(IEmailSettings)
//DEL 	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IEmailSettings)
	COM_INTERFACE_ENTRY(IObjectSettings)
	COM_INTERFACE_ENTRY(IObjectUserInterface)
END_COM_MAP()

	// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

public:
	// IEmailSettings
	STDMETHOD(get_EmailSignature)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_EmailSignature)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_SenderEmailAddr)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_SenderEmailAddr)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_SenderName)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_SenderName)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_SMTPPassword)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_SMTPPassword)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_SMTPUserName)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_SMTPUserName)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_SMTPPort)(/*[out, retval]*/ LONG *pVal);
	STDMETHOD(put_SMTPPort)(/*[in]*/ LONG newVal);
	STDMETHOD(get_SMTPServer)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_SMTPServer)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_SMTPTimeout)(/*[out, retval]*/ LONG *pVal);
	STDMETHOD(put_SMTPTimeout)(/*[in]*/ LONG newVal);

	// IObjectSettings
	STDMETHOD(raw_LoadFromRegistry)(BSTR bstrKey);
	STDMETHOD(raw_SaveToRegistry)(BSTR bstrKey);
	
	// IObjectUserInterface
	STDMETHOD(raw_DisplayReadOnly)();
	STDMETHOD(raw_DisplayReadWrite)();
	STDMETHOD(raw_SupportsReadOnly)(VARIANT_BOOL * bSupportsReadOnly);
	STDMETHOD(raw_SupportsReadWrite)(VARIANT_BOOL * bSupportsReadWrite);

private:
	// Variables
	std::string m_strSMTPServer;
	long m_lSMTPPort;
	std::string m_strSMTPUserName;
	std::string m_strSMTPPassword;
	std::string m_strSenderName;
	std::string m_strSenderEmailAddr;
	std::string m_strEmailSignature;
	
	// Used for SNTP timeout
	long m_nSMTPTimeout;

	// Method
	// Purpose: To take a full registry path with the HKLM, HKCU, HKEY_LOCAL_MACHINE or
	//			HKEY_CURRENT_USER and return the predefined HKEY and the remaining path
	//			in the argument strRegPath
	HKEY parseKey( const std::string strRegFullPath, std::string & strRegPath );
};

