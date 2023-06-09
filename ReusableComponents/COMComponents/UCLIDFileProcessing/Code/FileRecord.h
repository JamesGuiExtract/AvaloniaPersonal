// FileRecord.h : Declaration of the CFileRecord

#pragma once
#include "resource.h"       // main symbols
#include "UCLIDFileProcessing.h"

#include <string>

using namespace std;

#if defined(_WIN32_WCE) && !defined(_CE_DCOM) && !defined(_CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA)
#error "Single-threaded COM objects are not properly supported on Windows CE platform, such as the Windows Mobile platforms that do not include full DCOM support. Define _CE_ALLOW_SINGLE_THREADED_OBJECTS_IN_MTA to force ATL to support creating single-thread COM object's and allow use of it's single-threaded COM object implementations. The threading model in your rgs file was set to 'Free' as that is the only threading model supported in non DCOM Windows CE platforms."
#endif

// CFileRecord

class ATL_NO_VTABLE CFileRecord :
	public CComObjectRootEx<CComMultiThreadModel>,
	public CComCoClass<CFileRecord, &CLSID_FileRecord>,
	public ISupportErrorInfo,
	public IDispatchImpl<IFileRecord, &IID_IFileRecord, &LIBID_UCLID_FILEPROCESSINGLib, /*wMajor =*/ 1, /*wMinor =*/ 0>
{
public:
	CFileRecord();

DECLARE_REGISTRY_RESOURCEID(IDR_FILERECORD)


BEGIN_COM_MAP(CFileRecord)
	COM_INTERFACE_ENTRY(IFileRecord)
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
	STDMETHOD(get_FileID)(LONG* pVal);
	STDMETHOD(put_FileID)(LONG newVal);
	STDMETHOD(get_Name)(BSTR* pVal);
	STDMETHOD(put_Name)(BSTR newVal);
	STDMETHOD(get_FileSize)(LONGLONG* pVal);
	STDMETHOD(put_FileSize)(LONGLONG newVal);
	STDMETHOD(get_Pages)(LONG* pVal);
	STDMETHOD(put_Pages)(LONG newVal);
	STDMETHOD(get_ActionID)(LONG* pVal);
	STDMETHOD(put_ActionID)(LONG newVal);
	STDMETHOD(GetFileData)(LONG* plFileID, LONG* plActionID, BSTR* pbstrFileName,
		LONGLONG* pllFileSize, LONG* plPages, EFilePriority* pePriority, LONG *plWorkflowID);
	STDMETHOD(SetFileData)(LONG lFileID, LONG lActionID, BSTR bstrFileName,
		LONGLONG llFileSize, LONG lPages, EFilePriority ePriority, LONG lWorkflowID);
	STDMETHOD(get_Priority)(EFilePriority* pePriority);
	STDMETHOD(put_Priority)(EFilePriority ePriority);
	STDMETHOD(get_FallbackStatus)(EActionStatus* peaFallbackStatus);
	STDMETHOD(put_FallbackStatus)(EActionStatus eaFallbackStatus);
	STDMETHOD(get_WorkflowID)(LONG* pVal);
	STDMETHOD(put_WorkflowID)(LONG newVal);

private:

	// Variables
	long m_lFileID;
	long m_lActionID;
	string m_strName;
	long long m_llFileSize;
	long m_lPages;
	UCLID_FILEPROCESSINGLib::EFilePriority m_ePriority;
	UCLID_FILEPROCESSINGLib::EActionStatus m_eaFallbackStatus;
	long m_lWorkflowID;
};

OBJECT_ENTRY_AUTO(__uuidof(FileRecord), CFileRecord)
