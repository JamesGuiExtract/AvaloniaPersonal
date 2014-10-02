// WorkItemRecord.h : Declaration of the CWorkItemRecord

#pragma once
#include "resource.h"       // main symbols
#include "UCLIDFileProcessing.h"

#include <string>


using namespace std;

#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

// CWorkItemRecord

class ATL_NO_VTABLE CWorkItemRecord :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CWorkItemRecord, &CLSID_WorkItemRecord>,
	public ISupportErrorInfo,
	public IDispatchImpl<IWorkItemRecord, &IID_IWorkItemRecord, &LIBID_UCLID_FILEPROCESSINGLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
public:
	CWorkItemRecord();
	~CWorkItemRecord();

DECLARE_REGISTRY_RESOURCEID(IDR_WORKITEMRECORD)


BEGIN_COM_MAP(CWorkItemRecord)
	COM_INTERFACE_ENTRY(IWorkItemRecord)
	COM_INTERFACE_ENTRY(IDispatch)
	COM_INTERFACE_ENTRY(ISupportErrorInfo)
END_COM_MAP()

	DECLARE_PROTECT_FINAL_CONSTRUCT()

	HRESULT FinalConstruct()
	{
		return S_OK;
	}

	void FinalRelease()
	{
	}

public:
// ISupportsErrorInfo
	STDMETHOD(InterfaceSupportsErrorInfo)(REFIID riid);

// IFileRecord
	STDMETHOD(get_WorkItemID)(LONG* pnWorkItemID);
	STDMETHOD(put_WorkItemID)(LONG nWorkItemID);
	STDMETHOD(get_WorkItemGroupID)(long* pnWorkItemGroupID);
	STDMETHOD(put_WorkItemGroupID)(long nWorkItemGroupID);
	STDMETHOD(get_Status)(EWorkItemStatus *pStatus);
	STDMETHOD(put_Status)(EWorkItemStatus Status);
	STDMETHOD(get_Input)(BSTR *pInput);
	STDMETHOD(put_Input)(BSTR Input);
	STDMETHOD(get_Output)(BSTR* pOutput);
	STDMETHOD(put_Output)(BSTR Output);
	STDMETHOD(get_UPI)(BSTR* pUPI);
	STDMETHOD(put_UPI)(BSTR UPI);
	STDMETHOD(get_StringizedException)(BSTR *pStringizedException);
	STDMETHOD(put_StringizedException)(BSTR StringizedException);
	STDMETHOD(get_FileName)(BSTR *pFileName);
	STDMETHOD(put_FileName)(BSTR FileName);
	STDMETHOD(get_BinaryOutput)(IUnknown **ppBinaryOutput);
	STDMETHOD(put_BinaryOutput)(IUnknown *pBinaryOutput);
	STDMETHOD(get_BinaryInput)(IUnknown **ppBinaryInput);
	STDMETHOD(put_BinaryInput)(IUnknown *pBinaryInput);
	STDMETHOD(get_FileID)(long* pnFileID);
	STDMETHOD(put_FileID)(long nFileID);
	STDMETHOD(get_WorkGroupUPI)(BSTR* pUPI);
	STDMETHOD(put_WorkGroupUPI)(BSTR UPI);
	STDMETHOD(get_Priority)(EFilePriority* pePriority);
	STDMETHOD(put_Priority)(EFilePriority ePriority);

private:

	// Variables
	long m_nWorkItemID;
	long m_nWorkItemGroupID;
	EWorkItemStatus m_eWorkItemStatus;
	string m_strInput;
	string m_strOutput;
	string m_strUPI;
	string m_strStringizedException;
	string m_strFileName;
	IPersistStreamPtr m_ipBinaryOutput;
	IPersistStreamPtr m_ipBinaryInput;
	long m_nFileID;
	string m_strWorkGroupUPI;
	EFilePriority m_ePriority;
};

OBJECT_ENTRY_AUTO(__uuidof(WorkItemRecord), CWorkItemRecord)
