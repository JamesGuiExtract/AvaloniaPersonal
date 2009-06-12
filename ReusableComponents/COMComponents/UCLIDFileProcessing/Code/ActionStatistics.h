// ActionStatistics.h : Declaration of the CActionStatistics

#pragma once
#include "resource.h"       // main symbols

#include "UCLIDFileProcessing.h"


#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif



// CActionStatistics

class ATL_NO_VTABLE CActionStatistics :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CActionStatistics, &CLSID_ActionStatistics>,
	public ISupportErrorInfo,
	public IDispatchImpl<IActionStatistics, &IID_IActionStatistics, &LIBID_UCLID_FILEPROCESSINGLib, /*wMajor =*/ 1, /*wMinor =*/ 0>,
	public IDispatchImpl<ICopyableObject, &__uuidof(ICopyableObject), &LIBID_UCLID_COMUTILSLib, /* wMajor = */ 1>
{
public:
	CActionStatistics();

	DECLARE_REGISTRY_RESOURCEID(IDR_ACTIONSTATISTICS)


	BEGIN_COM_MAP(CActionStatistics)
		COM_INTERFACE_ENTRY(IActionStatistics)
		COM_INTERFACE_ENTRY2(IDispatch, ICopyableObject)
		COM_INTERFACE_ENTRY(ISupportErrorInfo)
		COM_INTERFACE_ENTRY(ICopyableObject)
	END_COM_MAP()


	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
	}

	// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

	// IActionStatistics
	STDMETHOD(get_NumDocuments)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_NumDocuments)(/*[in]*/ long newVal);
	STDMETHOD(get_NumDocumentsComplete)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_NumDocumentsComplete)(/*[in]*/ long newVal);
	STDMETHOD(get_NumDocumentsFailed)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_NumDocumentsFailed)(/*[in]*/ long newVal);
	STDMETHOD(get_NumPages)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_NumPages)(/*[in]*/ long newVal);
	STDMETHOD(get_NumPagesComplete)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_NumPagesComplete)(/*[in]*/ long newVal);
	STDMETHOD(get_NumPagesFailed)(/*[out, retval]*/ long *pVal);
	STDMETHOD(put_NumPagesFailed)(/*[in]*/ long newVal);
	STDMETHOD(get_NumBytes)(/*[out, retval]*/ LONGLONG *pVal);
	STDMETHOD(put_NumBytes)(/*[in]*/ LONGLONG newVal);
	STDMETHOD(get_NumBytesComplete)(/*[out, retval]*/ LONGLONG *pVal);
	STDMETHOD(put_NumBytesComplete)(/*[in]*/ LONGLONG newVal);
	STDMETHOD(get_NumBytesFailed)(/*[out, retval]*/ LONGLONG *pVal);
	STDMETHOD(put_NumBytesFailed)(/*[in]*/ LONGLONG newVal);
	
	// ICopyableObject Methods
	STDMETHOD(raw_Clone)(IUnknown * * pObject);
	STDMETHOD(raw_CopyFrom)(IUnknown * pObject);

private:
	long m_nNumDocuments;
	long m_nNumDocumentsComplete;
	long m_nNumDocumentsFailed;
	long m_nNumPages;
	long m_nNumPagesComplete;
	long m_nNumPagesFailed;
	long long m_llNumBytes;
	long long m_llNumBytesComplete;
	long long m_llNumBytesFailed;
};

OBJECT_ENTRY_AUTO(__uuidof(ActionStatistics), CActionStatistics)
