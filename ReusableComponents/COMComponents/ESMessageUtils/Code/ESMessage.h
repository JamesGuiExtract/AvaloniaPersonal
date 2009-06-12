// ESMessage.h : Declaration of the CESMessage

#pragma once

#include "resource.h"       // main symbols
#include <string>

#import "..\..\..\APIs\LeadTools_13\bin\LTCML13N.DLL" no_namespace \
rename("SendMessage", "LTSendMessage") \
rename("GetMessage", "LTGetMessage")

 

/////////////////////////////////////////////////////////////////////////////
// CESMessage
class ATL_NO_VTABLE CESMessage : 
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CESMessage, &CLSID_ESMessage>,
	public ISupportErrorInfo,
	public IDispatchImpl<IESMessage, &IID_IESMessage, &LIBID_ESMESSAGEUTILSLib>,
	public IDispatchImpl<IObjectUserInterface, &IID_IObjectUserInterface, &LIBID_UCLID_COMUTILSLib>
{
public:
	CESMessage();

DECLARE_REGISTRY_RESOURCEID(IDR_ESMESSAGE)

DECLARE_PROTECT_FINAL_CONSTRUCT()

BEGIN_COM_MAP(CESMessage)
	COM_INTERFACE_ENTRY(IESMessage)
//DEL 	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
	COM_INTERFACE_ENTRY2(IDispatch, IESMessage)
	COM_INTERFACE_ENTRY(IObjectUserInterface)
END_COM_MAP()

// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

public:
	STDMETHOD(Clear)();
	STDMETHOD(get_Sender)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_Sender)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_Recipients)(/*[out, retval]*/ IVariantVector * *pVal);
	STDMETHOD(put_Recipients)(/*[in]*/ IVariantVector * newVal);
	STDMETHOD(get_FileAttachments)(/*[out, retval]*/ IVariantVector * *pVal);
	STDMETHOD(put_FileAttachments)(/*[in]*/ IVariantVector * newVal);
	STDMETHOD(get_BodyText)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_BodyText)(/*[in]*/ BSTR newVal);
	STDMETHOD(get_Subject)(/*[out, retval]*/ BSTR *pVal);
	STDMETHOD(put_Subject)(/*[in]*/ BSTR newVal);
	STDMETHOD(put_EmailSettings)(/*[in]*/ IEmailSettings * newVal);
// IESMessage
	STDMETHOD(Send)();
// IObjectUserInterface
	STDMETHOD(raw_DisplayReadOnly)();
	STDMETHOD(raw_DisplayReadWrite)();
	STDMETHOD(raw_SupportsReadOnly)(VARIANT_BOOL * bSupportsReadOnly);
	STDMETHOD(raw_SupportsReadWrite)(VARIANT_BOOL * bSupportsReadWrite);


private:
	// Lead tools email object
	ILEADSmtpPtr m_ipltSMTP;

	// Email Settings object
	ESMESSAGEUTILSLib::IEmailSettingsPtr m_ipEmailSettings;

	// Vector to hold the list of file attachments
	IVariantVectorPtr m_ipFileAttachments;
	// Vector to hold the recipients
	IVariantVectorPtr m_ipRecipients;

	std::string  m_strBodyText;
	
	static CMutex ms_mutex;

	// Methods
	// return the m_ipltSMTP 
	// if m_ipltSMTP is null will initialize the with the email settings
	ILEADSmtpPtr getLtSMTP();

	void setupSender();
	// adds the list of Attachments to the email before sending or showing send dialog
	void addAttachments();
	// adds the list of Recipients to the email before sending or showing send dialog
	void addRecipients();
};

